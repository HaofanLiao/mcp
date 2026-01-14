// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WebPubSub.Options;

/// <summary>
/// Option definitions for Azure Web PubSub commands.
/// </summary>
public static class WebPubSubOptionDefinitions
{
    public const string WebPubSubName = "webpubsub";

    /// <summary>
    /// Web PubSub service name option.
    /// </summary>
    public static readonly Option<string> WebPubSub = new($"--{WebPubSubName}")
    {
        Description = "The name of the Web PubSub service"
    };
}
