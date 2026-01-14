// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.WebPubSub.Options;
using Azure.Mcp.Tools.WebPubSub.Options.Runtime;
using Azure.Mcp.Tools.WebPubSub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.WebPubSub.Commands.Runtime;

/// <summary>
/// Shows details of an Azure Web PubSub Service.
/// </summary>
public sealed class RuntimeGetCommand(ILogger<RuntimeGetCommand> logger)
    : BaseWebPubSubCommand<RuntimeGetOptions>
{
    private const string CommandTitle = "Show Service Details";
    private readonly ILogger<RuntimeGetCommand> _logger = logger;

    public override string Id => "c1f04a8e-3b2d-4c5e-9d7f-a8b6e4c2d1f0";

    public override string Name => "get";

    public override string Description =>
        """
        Gets or lists details of Azure Web PubSub Runtimes. If a specific Web PubSub name is used, the details of that
        Web PubSub runtime will be retrieved. Otherwise, all Web PubSub Runtimes in the specified subscription or resource
        group will be retrieved. Returns runtime information including identity, network ACLs, and configuration settings.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WebPubSubOptionDefinitions.WebPubSub);
    }

    protected override RuntimeGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WebPubSub ??= parseResult.GetValueOrDefault<string>(WebPubSubOptionDefinitions.WebPubSub.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var webPubSubService = context.GetService<IWebPubSubService>();
            var runtimes = await webPubSubService.GetRuntimeAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.WebPubSub,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy,
                cancellationToken);

            _logger.LogInformation("Found {Count} Web PubSub service(s) in subscription {SubscriptionId}",
                runtimes.Count(), options.Subscription);

            context.Response.Results = ResponseResult.Create(new(runtimes ?? []), WebPubSubJsonContext.Default.RuntimeGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred showing Web PubSub service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RuntimeGetCommandResult(IEnumerable<Models.Runtime> Runtimes);
}
