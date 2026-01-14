// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WebPubSub.Models;

/// <summary>
/// Represents the SKU configuration for a Web PubSub service.
/// </summary>
public sealed class Sku
{
    /// <summary> The capacity (unit count) for the SKU. </summary>
    public int? Capacity { get; set; }

    /// <summary> The SKU name. </summary>
    public string? Name { get; set; }

    /// <summary> The SKU size. </summary>
    public string? Size { get; set; }

    /// <summary> The SKU tier. </summary>
    public string? Tier { get; set; }
}
