// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Options;

/// <summary>
/// Option definitions for Azure SignalR commands.
/// </summary>
public static class SignalROptionDefinitions
{
    /// <summary>
    /// SignalR service name option.
    /// </summary>
    public static readonly Option<string> SignalRName = new(
        aliases: ["--signalr-name", "-n"],
        description: "The name of the SignalR service")
    {
        IsRequired = true
    };

    public static readonly Option<string> Templates = new(
        aliases: ["--templates", "-t"],
        description: """
                 Semicolon-separated list of upstream templates. Each template can contain comma-separated key-value pairs:
                 url-template, hub-pattern, event-pattern, category-pattern.
                 Example: "url-template="http://host-connections.com,category-pattern=connections;url-template="http://host-connections.com,hub-pattern=chat"
                 """)
    {
        IsRequired = true
    };
}
