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
        "--signalr-name")
    {
        Description = "The name of the SignalR service",
        Required = true
    };
}
