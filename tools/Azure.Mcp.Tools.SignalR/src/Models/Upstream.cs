// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SignalR.Models;

public class Upstream
{
     /// <summary> The template of the upstream URL. Template can include these variables: {hub}, {category}, {event}. </summary>
    [JsonPropertyName("urlTemplate")]
    public string? UrlTemplate { get; set; }

     /// <summary> The hub pattern to match. If not specified, applies to all hubs. </summary>
    [JsonPropertyName("hubPattern")]
    public string? HubPattern { get; set; }

     /// <summary> The event pattern to match. If not specified, applies to all events. </summary>
    [JsonPropertyName("eventPattern")]
    public string? EventPattern { get; set; }

     /// <summary> The category pattern to match. If not specified, applies to all categories. </summary>
    [JsonPropertyName("categoryPattern")]
    public string? CategoryPattern { get; set; }

     /// <summary> Authentication settings for the upstream endpoint. </summary>
    [JsonPropertyName("auth")]
    public UpstreamAuth? Auth { get; set; }
}

public class UpstreamAuth
{
    /// <summary> Upstream auth type enum. </summary>
    [JsonPropertyName("authType")]
    public string? AuthType { get; set; }

    /// <summary> Managed identity settings for upstream. </summary>
    [JsonPropertyName("managedIdentity")]
    public string? ManagedIdentity { get; set; }
}
