// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.WebPubSub.Commands.Runtime;

namespace Azure.Mcp.Tools.WebPubSub.Commands;

/// <summary>
/// JSON serialization context for Azure Web PubSub Service commands.
/// </summary>
[JsonSerializable(typeof(Models.Identity))]
[JsonSerializable(typeof(Models.NetworkAcls))]
[JsonSerializable(typeof(Models.Runtime))]
[JsonSerializable(typeof(Models.RuntimeProperties))]
[JsonSerializable(typeof(Models.Sku))]
[JsonSerializable(typeof(RuntimeGetCommand.RuntimeGetCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class WebPubSubJsonContext : JsonSerializerContext;

