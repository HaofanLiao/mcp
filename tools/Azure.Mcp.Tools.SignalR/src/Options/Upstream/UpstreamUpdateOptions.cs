// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SignalR.Options.Upstream;

/// <summary>
/// Options for updating Azure SignalR Service upstream endpoint configurations.
/// </summary>
public sealed class UpstreamUpdateOptions : BaseSignalROptions
{
    /// <summary>
    /// Semicolon-separated list of upstream templates. Each template can contain comma-separated key-value pairs:
    /// url-template, hub-pattern, event-pattern, category-pattern, auth-type, managed-identity-resource.
    /// Example: "url-template=http://host.com,category-pattern=connections;url-template=http://host2.com,hub-pattern=chat"
    /// </summary>
    public string? Templates { get; set; }
}
