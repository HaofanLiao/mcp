// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.WebPubSub.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.WebPubSub;
using Azure.ResourceManager.WebPubSub.Models;

namespace Azure.Mcp.Tools.WebPubSub.Services;

/// <summary>
/// Service for Azure Web PubSub operations using Resource Manager API.
/// </summary>
public sealed class WebPubSubService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), IWebPubSubService
{
    private readonly ISubscriptionService _subscriptionService =
        subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    private readonly ICacheService
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string CacheGroup = "webpubsub";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<IEnumerable<Runtime>> GetRuntimeAsync(
        string subscription,
        string? resourceGroup,
        string? webPubSubName,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var runtimes = new List<Runtime>();
        if (string.IsNullOrEmpty(webPubSubName))
        {
            var cacheKey = string.IsNullOrEmpty(tenant) ? subscription : $"{subscription}_{tenant}";
            cacheKey = string.IsNullOrEmpty(resourceGroup) ? cacheKey : $"{cacheKey}_{resourceGroup}";
            var cachedResults = await _cacheService.GetAsync<List<Runtime>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    var webPubSubResources = subscriptionResource.GetWebPubSubsAsync(cancellationToken);
                    await foreach (var runtime in webPubSubResources)
                    {
                        runtimes.Add(ConvertToRuntimeModel(runtime));
                    }
                }
                else
                {
                    var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                    if (!resourceGroupResource.HasValue)
                    {
                        throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                    }

                    var webPubSubResources = resourceGroupResource.Value.GetWebPubSubs().GetAllAsync(cancellationToken);
                    await foreach (var runtime in webPubSubResources)
                    {
                        runtimes.Add(ConvertToRuntimeModel(runtime));
                    }

                    await _cacheService.SetAsync(CacheGroup, cacheKey, runtimes, s_cacheDuration, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting Web PubSub Runtimes: {ex.Message}", ex);
            }
        }
        else
        {
            ValidateRequiredParameters((nameof(webPubSubName), webPubSubName), (nameof(resourceGroup), resourceGroup));
            var cacheKey = string.IsNullOrEmpty(tenant)
                ? $"{subscription}_{resourceGroup}_{webPubSubName}"
                : $"{subscription}_{tenant}_{resourceGroup}_{webPubSubName}";

            var cachedResults = await _cacheService.GetAsync<List<Runtime>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
            {
                return cachedResults;
            }

            try
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                if (!resourceGroupResource.HasValue)
                {
                    throw new Exception(
                        $"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                }

                var webPubSubResource = await resourceGroupResource.Value.GetWebPubSubs().GetAsync(webPubSubName, cancellationToken);
                if (!webPubSubResource.HasValue)
                {
                    throw new Exception(
                        $"Web PubSub '{webPubSubName}' not found in resource group '{resourceGroup}'");
                }

                runtimes.Add(ConvertToRuntimeModel(webPubSubResource.Value));
                await _cacheService.SetAsync(CacheGroup, cacheKey, runtimes, s_cacheDuration, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting Web PubSub Runtime: {ex.Message}", ex);
            }
        }

        return runtimes;
    }

    private static Runtime ConvertToRuntimeModel(WebPubSubResource resource)
    {
        var runtime = new Runtime
        {
            Id = resource.Id.ToString(),
            Identity = ConvertToIdentityModel(resource.Data.Identity),
            Location = resource.Data.Location,
            Name = resource.Data.Name,
            Properties = new RuntimeProperties
            {
                ExternalIP = resource.Data?.ExternalIP,
                HostName = resource.Data?.HostName,
                NetworkAcls = ConvertToNetworkAclsModel(resource.Data?.NetworkAcls),
                ProvisioningState = resource.Data?.ProvisioningState.ToString(),
                PublicNetworkAccess = resource.Data?.PublicNetworkAccess,
                PublicPort = resource.Data?.PublicPort,
                ServerPort = resource.Data?.ServerPort,
                Version = resource.Data?.Version,
                DisableLocalAuth = resource.Data?.IsLocalAuthDisabled,
                DisableAadAuth = resource.Data?.IsAadAuthDisabled
            },
            Sku = new Sku
            {
                Capacity = resource.Data?.Sku?.Capacity,
                Name = resource.Data?.Sku?.Name,
                Size = resource.Data?.Sku?.Size,
                Tier = resource.Data?.Sku?.Tier.ToString()
            },
            Tags = resource.Data?.Tags
        };
        return runtime ?? throw new InvalidOperationException("Failed to parse Web PubSub runtime data");
    }

    private static NetworkAcls? ConvertToNetworkAclsModel(WebPubSubNetworkAcls? networkAcls)
    {
        if (networkAcls is null)
        {
            return null;
        }

        PublicNetwork? publicNetwork = null;
        if (networkAcls.PublicNetwork is not null)
        {
            var allow = networkAcls.PublicNetwork.Allow?.Select(a => a.ToString()).ToList();
            var deny = networkAcls.PublicNetwork.Deny?.Select(d => d.ToString()).ToList();
            if (allow != null || deny != null)
            {
                publicNetwork = new PublicNetwork { Allow = allow, Deny = deny };
            }
        }

        var privateEndpoints = networkAcls.PrivateEndpoints?.Select(pe => new PrivateEndpoint
        {
            Name = pe.Name,
            Allow = pe.Allow?.Select(a => a.ToString()).ToList(),
            Deny = pe.Deny?.Select(d => d.ToString()).ToList()
        }).ToList();

        return new NetworkAcls
        {
            DefaultAction = networkAcls.DefaultAction?.ToString(),
            PublicNetwork = publicNetwork,
            PrivateEndpoints = privateEndpoints
        };
    }

    private static Models.Identity? ConvertToIdentityModel(ManagedServiceIdentity? identity)
    {
        if (identity is null)
        {
            return null;
        }

        SystemAssignedIdentityInfo? systemAssigned =
            identity.ManagedServiceIdentityType == ManagedServiceIdentityType.SystemAssigned
                ? new SystemAssignedIdentityInfo
                {
                    PrincipalId = identity.PrincipalId.ToString(),
                    TenantId = identity.TenantId.ToString()
                }
                : null;

        UserAssignedIdentityInfo[]? userAssigned =
            identity.ManagedServiceIdentityType == ManagedServiceIdentityType.UserAssigned
            && identity.UserAssignedIdentities is not null
                ? [.. identity.UserAssignedIdentities.Select(kv => new UserAssignedIdentityInfo
                {
                    ClientId = kv.Key.ToString(),
                    PrincipalId = kv.Value.PrincipalId.ToString()
                })]
                : null;

        var managedIdentityInfo = new ManagedIdentityInfo
        {
            SystemAssignedIdentity = systemAssigned,
            UserAssignedIdentities = userAssigned
        };

        return new Models.Identity
        {
            Type = identity.ManagedServiceIdentityType.ToString(),
            ManagedIdentityInfo = managedIdentityInfo
        };
    }
}
