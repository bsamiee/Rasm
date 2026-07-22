# [RASM_APPHOST_API_SERVICE_DISCOVERY]

`Microsoft.Extensions.ServiceDiscovery` resolves an outbound service name into a live endpoint set and balances calls across it: `ServiceEndpointResolver` folds configuration and pass-through providers into a change-token-refreshed `ServiceEndpointSource`, and the `HttpClient`/gRPC integration picks one instance per request through the registered round-robin selector. AppHost's wire/coordination rail dials cluster membership and election endpoints by service name over this surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.ServiceDiscovery`
- package: `Microsoft.Extensions.ServiceDiscovery`
- assembly: `Microsoft.Extensions.ServiceDiscovery`
- assembly: `Microsoft.Extensions.ServiceDiscovery.Abstractions`
- namespace: `Microsoft.Extensions.ServiceDiscovery`, `Microsoft.Extensions.ServiceDiscovery.Configuration`, `Microsoft.Extensions.ServiceDiscovery.Http`, `Microsoft.Extensions.ServiceDiscovery.LoadBalancing`, `Microsoft.Extensions.ServiceDiscovery.PassThrough`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: discovery

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: abstractions and endpoint family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]       | [CAPABILITY]                   |
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

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]       | [CAPABILITY]                     |
| :-----: | :-------------------------------------------- | :------------------ | :------------------------------- |
|  [01]   | `ServiceEndpointResolver`                     | standalone resolver | service-name endpoint resolution |
|  [02]   | `ServiceDiscoveryOptions`                     | options             | scheme and refresh policy        |
|  [03]   | `ConfigurationServiceEndpointProviderOptions` | options             | configuration section binding    |
|  [04]   | `IServiceDiscoveryHttpMessageHandlerFactory`  | handler factory     | resolving `HttpMessageHandler`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------ | :------ | :---------------------------------------- |
|  [01]   | `AddServiceDiscovery()`                           | static  | core plus configuration plus pass-through |
|  [02]   | `AddServiceDiscovery(Action)`                     | static  | core registration with options            |
|  [03]   | `AddServiceDiscoveryCore()`                       | static  | resolver, watcher, selector wiring        |
|  [04]   | `AddServiceDiscoveryCore(Action)`                 | static  | core wiring with options binding          |
|  [05]   | `AddConfigurationServiceEndpointProvider()`       | static  | `IConfiguration` endpoint provider        |
|  [06]   | `AddConfigurationServiceEndpointProvider(Action)` | static  | configuration provider with options       |
|  [07]   | `AddPassThroughServiceEndpointProvider()`         | static  | no-resolution pass-through provider       |
|  [08]   | `IHttpClientBuilder.AddServiceDiscovery()`        | static  | resolving handler plus gRPC filter        |

[ENTRYPOINT_SCOPE]: resolution and selection operations

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `ServiceEndpointResolver.GetEndpointsAsync`                | instance | `ValueTask<ServiceEndpointSource>` |
|  [02]   | `ServiceEndpointQuery.TryParse`                            | static   | input to query value               |
|  [03]   | `ServiceEndpoint.TryParse`                                 | static   | string to endpoint value           |
|  [04]   | `ServiceEndpoint.Create`                                   | factory  | `EndPoint` plus features           |
|  [05]   | `IServiceEndpointProvider.PopulateAsync`                   | instance | builder endpoint contribution      |
|  [06]   | `IServiceEndpointProviderFactory.TryCreateProvider`        | instance | query-keyed provider               |
|  [07]   | `IServiceEndpointBuilder.AddChangeToken`                   | instance | refresh change-token sink          |
|  [08]   | `ServiceDiscoveryOptions.ApplyAllowedSchemes`              | instance | allowed-scheme intersection        |
|  [09]   | `IServiceDiscoveryHttpMessageHandlerFactory.CreateHandler` | instance | resolving handler over inner       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `...Abstractions` assembly holds the public consumer contract; the main assembly carries the resolver, providers, and selector.
- `ServiceEndpointResolver` caches one resolver per service name in a `ConcurrentDictionary`, evicts idle entries on a cleanup timer, and is `IAsyncDisposable`.
- Resolving a service name that yields no endpoints throws `InvalidOperationException`; the round-robin selector faults identically on an empty endpoint set.
- Round-robin is the sole shipped selector — `internal`, registered as the default, advancing by `Interlocked.Increment` modulo endpoint count; no random selector ships.
- Refresh rides `ServiceEndpointSource.ChangeToken`; consumers observe membership change through the token, never by polling endpoint values.
- `ServiceDiscoveryOptions.AllowAllSchemes` defaults `true`, `RefreshPeriod` defaults to 60 seconds absent an active change callback, and `AllowedSchemes` gates schemes once `AllowAllSchemes` is `false`.
- `ConfigurationServiceEndpointProvider` binds the `"Services"` configuration section by default; `PassThroughServiceEndpointProvider` returns an already-addressable `EndPoint` unresolved.
- `IHttpClientBuilder.AddServiceDiscovery` installs a resolving delegating handler and a filter that disables built-in gRPC load balancing for resolved clients.

[STACKING]:
- `Microsoft.Extensions.Http.Resilience`(`.api/api-resilience.md`): the resolving delegating handler that `IHttpClientBuilder.AddServiceDiscovery` installs chains ahead of `AddStandardResilienceHandler` on one outbound pipeline — resolution picks the instance, the resilience handler owns retry and circuit-breaking over it.
- `Grpc.Net.Client`(`libs/csharp/.api/api-grpc-client.md`): `IHttpClientBuilder.AddServiceDiscovery` installs the gRPC load-balancing filter, so a `Wire/coordination` channel resolves its cluster election endpoint through this resolver rather than a hand-subclassed `Resolver`/`LoadBalancer`.
- `Wire/coordination`: dials cluster membership and election endpoints by service name through `ServiceEndpointResolver.GetEndpointsAsync`, the round-robin selector picking one instance per call.

[LOCAL_ADMISSION]:
- Membership and election targets resolve as service names through `ServiceEndpointResolver.GetEndpointsAsync`, never hard-coded host strings.
- Instance selection stays inside the resolver's round-robin selector, never reimplemented at a call site.
- Providers register explicitly — `AddConfigurationServiceEndpointProvider` for `IConfiguration`-backed cluster rows, `AddPassThroughServiceEndpointProvider` for already-addressable endpoints.
- Scheme filtering is package policy through `ServiceDiscoveryOptions.AllowedSchemes`, never a call-site URI check.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.ServiceDiscovery`
- Owns: outbound endpoint resolution and client-side load balancing
- Accept: service-name queries and registered endpoint providers
- Reject: hard-coded endpoint strings or hand-rolled instance round-robin
</content>
</invoke>
