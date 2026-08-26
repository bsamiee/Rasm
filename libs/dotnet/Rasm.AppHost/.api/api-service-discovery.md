# [RASM_APPHOST_API_SERVICE_DISCOVERY]

`Microsoft.Extensions.ServiceDiscovery` resolves an outbound service name into a live endpoint set and balances calls across it: `ServiceEndpointResolver` folds configuration and pass-through providers into a change-token-refreshed `ServiceEndpointSource`, and the `HttpClient`/gRPC integration picks one instance per request through the registered round-robin selector. AppHost's wire/coordination layer dials cluster membership by service name through the resolving named `HttpClient` this surface decorates.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

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

[PUBLIC_MEMBER_SCOPE]: `ServiceEndpointQuery`

| [INDEX] | [MEMBER]          | [TYPE]                  | [CAPABILITY]                                 |
| :-----: | :---------------- | :---------------------- | :------------------------------------------- |
|  [01]   | `IncludedSchemes` | `IReadOnlyList<string>` | ordered scheme preference split from the URI |
|  [02]   | `ServiceName`     | `string`                | the resolved host, endpoint prefix stripped  |
|  [03]   | `EndpointName`    | `string?`               | the `_name.` host prefix, absent when unused |

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `...Abstractions` assembly holds the public consumer contract; the main assembly carries the resolver, providers, and selector.
- `ServiceEndpointResolver` caches one resolver per service name in a `ConcurrentDictionary`, evicts idle entries on a cleanup timer, and is `IAsyncDisposable`.
- Resolving a service name that yields no endpoints throws `InvalidOperationException`; the round-robin selector faults identically on an empty endpoint set.
- Round-robin is the sole shipped selector — `internal`, registered as the default, advancing by `Interlocked.Increment` modulo endpoint count; no random selector ships.
- Refresh rides `ServiceEndpointSource.ChangeToken`; consumers observe membership change through the token, never by polling endpoint values.
- `ServiceDiscoveryOptions.AllowAllSchemes` defaults `true`, `RefreshPeriod` defaults to 60 seconds absent an active change callback, and `AllowedSchemes` gates schemes once `AllowAllSchemes` is `false`; `ApplyAllowedSchemes` intersects a requested list against that set and answers the whole allowed list when the request is empty.
- Query scheme is an ORDERED PREFERENCE, not one value: `ServiceEndpointQuery.TryParse` splits `Uri.Scheme` on `'+'`, so `https+http://name` resolves https endpoints ahead of http ones and a single-scheme authority forecloses that fallback.
- Query host carries an optional endpoint name: a leading `_` before the first `.` splits `EndpointName` from `ServiceName`, so `_grpc.mesh` names one endpoint of the `mesh` service; a schemeless input parses under a synthetic scheme and yields an EMPTY `IncludedSchemes`.
- `ConfigurationServiceEndpointProvider` binds the `"Services"` configuration section by default; `PassThroughServiceEndpointProvider` returns an already-addressable `EndPoint` unresolved.
- `IHttpClientBuilder.AddServiceDiscovery` installs a resolving delegating handler and a filter that disables built-in gRPC load balancing for resolved clients.

[STACKING]:
- `Microsoft.Extensions.Http.Resilience`(`.api/api-resilience.md`): the resolving delegating handler that `IHttpClientBuilder.AddServiceDiscovery` installs chains ahead of `AddStandardResilienceHandler` on one outbound pipeline — resolution picks the instance, the resilience handler owns retry and circuit-breaking over it.
- `Grpc.Net.Client`(`libs/dotnet/.api/api-grpc-client.md`): `IHttpClientBuilder.AddServiceDiscovery` installs the gRPC load-balancing filter, so a `Wire/coordination` channel resolves its cluster election endpoint through this resolver rather than a hand-subclassed `Resolver`/`LoadBalancer`.
- `Wire/coordination`: `Membership`'s named `HttpClient` carries `AddServiceDiscovery()` with the standard resilience handler, so resolution and round-robin selection run inside the client per request — no fence calls `ServiceEndpointResolver` directly.

[LOCAL_ADMISSION]:
- Membership targets resolve as service names inside the `AddServiceDiscovery()`-decorated client, never hard-coded host strings and never a direct resolver call.
- Instance selection stays inside the resolver's round-robin selector, never reimplemented at a call site.
- Providers register explicitly — `AddConfigurationServiceEndpointProvider` for `IConfiguration`-backed cluster rows, `AddPassThroughServiceEndpointProvider` for already-addressable endpoints.
- Scheme filtering is package policy through `ServiceDiscoveryOptions.AllowedSchemes`, never a call-site URI check.
- Dialled authorities carry their scheme preference as composition DATA — `Wire/coordination`'s `DialScheme` rows spell the ordered list this query parses — so no projection hard-codes a scheme for an endpoint family that states none.
