// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Upstream;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Upstream;

public sealed class UpstreamUpdateCommand(ILogger<UpstreamUpdateCommand> logger)
    : BaseSignalRCommand<UpstreamUpdateOptions>
{
    private const string _commandTitle = "SignalR Upstream Update";
    private readonly ILogger<UpstreamUpdateCommand> _logger = logger;
    private readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalRName;
    private readonly Option<string> _templatesOption = SignalROptionDefinitions.Templates;

    public override string Name => "update";

    public override string Description =>
        """
        Updates the upstream endpoint configuration for an Azure SignalR Service.
        Upstream endpoints are used to send events from SignalR to serverless functions or webhooks.
        You can specify multiple URL templates with variables like {hub}, {category}, and {event} to route events.
        """;

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup();
        command.AddOption(_signalRNameOption);
        command.AddOption(_templatesOption);
    }

    protected override UpstreamUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalRName = parseResult.GetValueForOption(_signalRNameOption);
        options.Templates = parseResult.GetValueForOption(_templatesOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            // Required validation step using the base Validate method
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            // Parse templates from the input string
            var upstreams = ParseUpstreamTemplates(options.Templates!);

            // Get the appropriate service from DI
            var service = context.GetService<ISignalRService>();

            // Call service operation
            var results = await service.UpdateUpstreamsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalRName!,
                upstreams,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            // Set results if any were returned
            context.Response.Results = results.Any()
                ? ResponseResult.Create(new UpstreamUpdateCommandResult(results),
                    SignalRJsonContext.Default.UpstreamUpdateCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            // Log error with context information
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static List<Models.Upstream> ParseUpstreamTemplates(string templatesInput)
    {
        var upstreams = new List<Models.Upstream>();

        // Split by semicolon to get individual templates
        var templateStrings = templatesInput.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var templateString in templateStrings)
        {
            var upstream = new Models.Upstream();

            // Split by comma to get key-value pairs
            var pairs = templateString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                if (keyValue.Length != 2) continue;

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                switch (key)
                {
                    case "url-template":
                        upstream.UrlTemplate = value;
                        break;
                    case "hub-pattern":
                        upstream.HubPattern = value;
                        break;
                    case "event-pattern":
                        upstream.EventPattern = value;
                        break;
                    case "category-pattern":
                        upstream.CategoryPattern = value;
                        break;
                    case "managed-identity":
                        upstream.Auth = new UpstreamAuth()
                        {
                            AuthType = string.IsNullOrEmpty(value) ? "ManagedIdentity" : "None",
                            ManagedIdentity = value
                        };
                        break;
                }
            }

            if (!string.IsNullOrEmpty(upstream.UrlTemplate))
            {
                upstreams.Add(upstream);
            }
        }

        return upstreams;
    }

    internal record UpstreamUpdateCommandResult(IEnumerable<Azure.Mcp.Tools.SignalR.Models.Upstream> Upstreams);
}
