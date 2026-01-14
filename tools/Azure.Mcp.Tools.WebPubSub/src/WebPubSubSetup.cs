// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WebPubSub.Commands.Runtime;
using Azure.Mcp.Tools.WebPubSub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.WebPubSub;

public class WebPubSubSetup : IAreaSetup
{
    public string Name => "webpubsub";

    public string Title => "Azure Web PubSub Service";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWebPubSubService, WebPubSubService>();

        services.AddSingleton<RuntimeGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var webpubsub = new CommandGroup(Name,
            "Azure Web PubSub operations - Commands for managing Azure Web PubSub Service resources. Includes operations for listing Web PubSub services.", Title);

        var runtime = new CommandGroup("runtime",
            "Runtime operations - Commands for managing Azure Web PubSub Service resources.");
        webpubsub.AddSubGroup(runtime);

        var runtimeGet = serviceProvider.GetRequiredService<RuntimeGetCommand>();
        runtime.AddCommand(runtimeGet.Name, runtimeGet);

        return webpubsub;
    }
}

