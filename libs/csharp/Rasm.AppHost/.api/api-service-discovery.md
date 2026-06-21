# [RASM_APPHOST_API_SERVICE_DISCOVERY]

`Microsoft.Extensions.ServiceDiscovery` supplies outbound endpoint resolution, a standalone `ServiceEndpointResolver`, configuration- and pass-through endpoint providers, round-robin client-side load balancing, and `HttpClient` integration over the abstractions surface (`ServiceEndpointQuery`, `ServiceEndpointSource`, `ServiceEndpoint`, `IServiceEndpointProvider`). It serves the AppHost wire/coordination rail that resolves cluster membership and election endpoints and balances calls across resolved instances.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.ServiceDiscovery`
- package: `Microsoft.Extensions.ServiceDiscovery`
- assembly: `Microsoft.Extensions.ServiceDiscovery`
- assembly: `Microsoft.Extensions.ServiceDiscovery.Abstractions`
- namespace: `Microsoft.Extensions.ServiceDiscovery`
- namespace: `Microsoft.Extensions.ServiceDiscovery.Configuration`
- namespace: `Microsoft.Extensions.ServiceDiscovery.Http`
- namespace: `Microsoft.Extensions.ServiceDiscovery.LoadBalancing`
- namespace: `Microsoft.Extensions.ServiceDiscovery.PassThrough`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: discovery

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: abstractions and endpoint family
- rail: discovery

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [RAIL]                         |
| :-----: | :-------------------------------- | :------------------ | :----------------------------- |
|  [01]   | `ServiceEndpointQuery`            | query value         | service name plus schemes      |
|  [02]   | `ServiceEndpointSource`           | endpoint collection | resolved endpoints plus token  |
|  [03]   | `ServiceEndpoint`                 | endpoint value      | single resolved endpoint       |
|  [04]   | `UriEndPoint`                     | endpoint value      | URI-backed `EndPoint`          |
|  [05]   | `IServiceEndpointProvider`        | provider contract   | populate endpoints             |
|  [06]   | `IServiceEndpointProviderFactory` | provider factory    | query-keyed provider creation  |
|  [07]   | `IServiceEndpointBuilder`         | builder contract    | endpoint and change-token sink |
|  [08]   | `IHostNameFeature`                | endpoint feature    | host-name metadata             |

[PUBLIC_TYPE_SCOPE]: resolver, options, and HTTP family
- rail: discovery

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]       | [RAIL]                           |
| :-----: | :-------------------------------------------- | :------------------ | :------------------------------- |
|  [01]   | `ServiceEndpointResolver`                     | standalone resolver | service-name endpoint resolution |
|  [02]   | `ServiceDiscoveryOptions`                     | options             | scheme and refresh policy        |
|  [03]   | `ConfigurationServiceEndpointProviderOptions` | options             | configuration section binding    |
|  [04]   | `IServiceDiscoveryHttpMessageHandlerFactory`  | handler factory     | resolving `HttpMessageHandler`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: discovery

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]        | [RAIL]                                    |
| :-----: | :------------------------------------------ | :-------------------- | :---------------------------------------- |
|  [01]   | `AddServiceDiscovery()`                     | service registration  | core plus configuration plus pass-through |
|  [02]   | `AddServiceDiscovery(configureOptions)`     | service registration  | core registration with options            |
|  [03]   | `AddServiceDiscoveryCore()`                 | service registration  | resolver, watcher, selector wiring        |
|  [04]   | `AddConfigurationServiceEndpointProvider()` | provider registration | `IConfiguration` endpoint provider        |
|  [05]   | `AddPassThroughServiceEndpointProvider()`   | provider registration | no-resolution pass-through provider       |
|  [06]   | `IHttpClientBuilder.AddServiceDiscovery()`  | client integration    | resolving handler plus gRPC filter        |

[ENTRYPOINT_SCOPE]: resolution and selection operations
- rail: discovery

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]    | [RAIL]                             |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------- |
|  [01]   | `ServiceEndpointResolver.GetEndpointsAsync`         | resolution call   | `ValueTask<ServiceEndpointSource>` |
|  [02]   | `ServiceEndpointQuery.TryParse`                     | guarded parse     | input to query value               |
|  [03]   | `ServiceEndpoint.TryParse`                          | guarded parse     | string to endpoint value           |
|  [04]   | `ServiceEndpoint.Create`                            | endpoint factory  | `EndPoint` plus features           |
|  [05]   | `IServiceEndpointProvider.PopulateAsync`            | provider populate | builder endpoint contribution      |
|  [06]   | `IServiceEndpointProviderFactory.TryCreateProvider` | provider create   | query-keyed provider               |
|  [07]   | `ServiceDiscoveryOptions.ApplyAllowedSchemes`       | scheme filter     | allowed-scheme intersection        |

## [04]-[IMPLEMENTATION_LAW]

[DISCOVERY_TOPOLOGY]:
- abstractions surface: `ServiceEndpointQuery`, `ServiceEndpointSource`, `ServiceEndpoint`, `UriEndPoint`, `IServiceEndpointProvider`, `IServiceEndpointProviderFactory`, `IServiceEndpointBuilder`, and `IHostNameFeature` live in `Microsoft.Extensions.ServiceDiscovery.Abstractions` and are the public consumer contract.
- resolver surface: `ServiceEndpointResolver` is the public standalone entry; `GetEndpointsAsync(serviceName, cancellationToken)` returns `ValueTask<ServiceEndpointSource>` and throws `InvalidOperationException` when no endpoints resolve.
- query surface: `ServiceEndpointQuery` carries `ServiceName`, ordered `IncludedSchemes`, and `EndpointName`; `TryParse` converts `scheme+scheme://_endpoint.service` form to a query value.
- endpoint surface: `ServiceEndpoint` exposes `EndPoint` and a feature collection; `UriEndPoint` wraps a `Uri` as a `System.Net.EndPoint`.
- source surface: `ServiceEndpointSource` exposes `Endpoints`, a `ChangeToken` for refresh, and a feature collection.
- provider surface: `IServiceEndpointProvider.PopulateAsync` fills an `IServiceEndpointBuilder`; `IServiceEndpointProviderFactory.TryCreateProvider(query, out provider)` keys provider creation by query.
- selection surface: `IServiceEndpointSelector` and `RoundRobinServiceEndpointSelector` are `internal`; round-robin is the registered default selector and advances by `Interlocked.Increment` modulo endpoint count. No random selector type ships in this assembly.
- configuration provider: `ConfigurationServiceEndpointProvider` resolves from `IConfiguration`; `ConfigurationServiceEndpointProviderOptions.SectionName` defaults to `"Services"`, and `ShouldApplyHostNameMetadata` defaults to a delegate returning `false`.
- pass-through provider: `PassThroughServiceEndpointProvider` returns the input `EndPoint` without resolution for already-addressable targets.
- options surface: `ServiceDiscoveryOptions.AllowAllSchemes` defaults to `true`, `RefreshPeriod` defaults to 60 seconds for providers without active change callbacks, and `AllowedSchemes` gates schemes when `AllowAllSchemes` is `false`.
- HTTP surface: `IHttpClientBuilder.AddServiceDiscovery` installs a `ResolvingHttpDelegatingHandler` plus an `IHttpMessageHandlerBuilderFilter` that disables built-in gRPC load balancing for resolved clients; `IServiceDiscoveryHttpMessageHandlerFactory` creates standalone resolving handlers.
- lifecycle: `ServiceEndpointResolver` is `IAsyncDisposable`, caches per-service resolvers in a `ConcurrentDictionary`, and runs a 10-second cleanup timer that evicts unused resolver entries.

[LOCAL_ADMISSION]:
- Endpoint resolution composes through `ServiceEndpointResolver.GetEndpointsAsync`; membership and election targets are service names, not hard-coded host strings.
- Outbound calls select one instance through the registered round-robin selector; selection stays inside the resolution call, never reimplemented at call sites.
- Provider registration is explicit: `AddConfigurationServiceEndpointProvider` for `IConfiguration`-backed cluster rows and `AddPassThroughServiceEndpointProvider` for already-addressable endpoints.
- `ServiceEndpointSource.ChangeToken` drives refresh; consumers observe membership change through the token, not by polling endpoint values.
- Scheme admission flows through `ServiceDiscoveryOptions`; resolved URIs honor `AllowedSchemes` when `AllowAllSchemes` is disabled.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.ServiceDiscovery`
- Owns: outbound endpoint resolution and client-side load balancing
- Accept: service-name queries and registered endpoint providers
- Reject: hard-coded endpoint strings or hand-rolled instance round-robin
