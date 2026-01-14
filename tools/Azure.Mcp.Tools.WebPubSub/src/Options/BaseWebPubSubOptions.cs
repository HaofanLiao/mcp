// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WebPubSub.Options;

/// <summary>
/// Base options for Azure Web PubSub commands.
/// </summary>
public class BaseWebPubSubOptions : SubscriptionOptions
{
    [JsonPropertyName(WebPubSubOptionDefinitions.WebPubSubName)]
    public string? WebPubSub { get; set; }
}
