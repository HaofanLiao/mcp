// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.WebPubSub.Models;

namespace Azure.Mcp.Tools.WebPubSub.Services;

/// <summary>
/// Service interface for Azure Web PubSub operations.
/// </summary>
public interface IWebPubSubService
{
    Task<IEnumerable<Runtime>> GetRuntimeAsync(
        string subscription,
        string? resourceGroup,
        string? webPubSubName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
