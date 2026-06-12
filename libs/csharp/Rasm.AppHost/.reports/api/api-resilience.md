# [RASM_APPHOST_API_RESILIENCE]

`Microsoft.Extensions.Http.Resilience` supplies outbound HTTP resilience handlers,
standard pipelines, hedging pipelines, routing groups, predicates, request context
bridges, and policy options for remote hops.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Resilience`
- package: `Microsoft.Extensions.Http.Resilience`
- assembly: `Microsoft.Extensions.Http.Resilience`
- namespace: `Microsoft.Extensions.Http.Resilience`
- asset: runtime library
- rail: resilience

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handler and pipeline family
- rail: resilience

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [RAIL]                    |
| :-----: | :--------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `ResilienceHandler`                      | delegating handler | outbound pipeline         |
|   [2]   | `ResilienceHandlerContext`               | handler context    | named pipeline context    |
|   [3]   | `IHttpResiliencePipelineBuilder`         | builder contract   | custom pipeline builder   |
|   [4]   | `IHttpStandardResiliencePipelineBuilder` | builder contract   | standard pipeline builder |
|   [5]   | `IStandardHedgingHandlerBuilder`         | hedging builder    | standard hedging setup    |
|   [6]   | `IRoutingStrategyBuilder`                | routing builder    | hedging route setup       |

[PUBLIC_TYPE_SCOPE]: strategy options family
- rail: resilience

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [RAIL]                  |
| :-----: | :------------------------------------- | :---------------- | :---------------------- |
|   [1]   | `HttpStandardResilienceOptions`        | standard policy   | retry/timeout/breaker   |
|   [2]   | `HttpStandardHedgingResilienceOptions` | hedging policy    | hedging pipeline        |
|   [3]   | `HttpRetryStrategyOptions`             | retry policy      | transient retry         |
|   [4]   | `HttpTimeoutStrategyOptions`           | timeout policy    | attempt timeout         |
|   [5]   | `HttpCircuitBreakerStrategyOptions`    | breaker policy    | circuit breaker         |
|   [6]   | `HttpRateLimiterStrategyOptions`       | rate-limit policy | request concurrency     |
|   [7]   | `HttpHedgingStrategyOptions`           | hedging policy    | parallel attempt policy |
|   [8]   | `HedgingEndpointOptions`               | endpoint policy   | endpoint resilience     |

[PUBLIC_TYPE_SCOPE]: routing and predicate family
- rail: resilience

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :-------------------------------------- | :---------------- | :----------------------- |
|   [1]   | `OrderedGroupsRoutingOptions`           | route policy      | ordered endpoint groups  |
|   [2]   | `WeightedGroupsRoutingOptions`          | route policy      | weighted endpoint groups |
|   [3]   | `UriEndpoint`                           | endpoint value    | route target             |
|   [4]   | `WeightedUriEndpoint`                   | weighted endpoint | weighted route target    |
|   [5]   | `UriEndpointGroup`                      | endpoint group    | ordered group member     |
|   [6]   | `WeightedUriEndpointGroup`              | weighted group    | weighted group member    |
|   [7]   | `WeightedGroupSelectionMode`            | selection enum    | weighted selection       |
|   [8]   | `HttpClientResiliencePredicates`        | predicate family  | standard transient test  |
|   [9]   | `HttpClientHedgingResiliencePredicates` | predicate family  | hedging transient test   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler operations
- rail: resilience

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]        | [RAIL]                     |
| :-----: | :------------------------------------ | :-------------------- | :------------------------- |
|   [1]   | `AddStandardResilienceHandler`        | HTTP client extension | standard pipeline setup    |
|   [2]   | `AddStandardHedgingHandler`           | HTTP client extension | hedging pipeline setup     |
|   [3]   | `AddResilienceHandler`                | HTTP client extension | custom pipeline setup      |
|   [4]   | `RemoveAllResilienceHandlers`         | HTTP client extension | pipeline reset             |
|   [5]   | `Configure(configuration)`            | options binding       | configuration-section bind |
|   [6]   | `Configure(Action)`                   | options delegate      | direct policy setup        |
|   [7]   | `Configure(Action, IServiceProvider)` | options delegate      | service-aware setup        |
|   [8]   | `SelectPipelineByAuthority`           | pipeline selector     | authority-keyed pipeline   |
|   [9]   | `SelectPipelineBy`                    | pipeline selector     | custom-keyed pipeline      |
|  [10]   | `DisableForUnsafeHttpMethods`         | retry filter          | idempotency guard          |
|  [11]   | `DisableFor`                          | retry filter          | method-specific guard      |

[ENTRYPOINT_SCOPE]: routing and context operations
- rail: resilience

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :---------------------------- | :---------------- | :---------------------- |
|   [1]   | `ConfigureOrderedGroups`      | routing setup     | ordered route groups    |
|   [2]   | `ConfigureWeightedGroups`     | routing setup     | weighted route groups   |
|   [3]   | `IsTransient`                 | predicate         | retry/hedging predicate |
|   [4]   | `IsTransientHttpFailure`      | predicate         | status-code predicate   |
|   [5]   | `IsTransientHttpException`    | predicate         | exception predicate     |
|   [6]   | `ResilienceHandler.SendAsync` | handler execution | async HTTP pipeline     |
|   [7]   | `EnableReloads<TOptions>`     | context reload    | named options reload    |
|   [8]   | `GetOptions<TOptions>`        | context lookup    | named options lookup    |
|   [9]   | `OnPipelineDisposed`          | context callback  | pipeline disposal hook  |
|  [10]   | `GetResilienceContext`        | request extension | request context read    |
|  [11]   | `SetResilienceContext`        | request extension | request context write   |
|  [12]   | `GetRequestMessage`           | Polly extension   | context request read    |
|  [13]   | `SetRequestMessage`           | Polly extension   | context request write   |

## [4]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.Resilience`
- handler rails: standard resilience handler, custom resilience handler, standard hedging handler
- policy families: retry, timeout, circuit breaker, rate limiter, attempt timeout, total request timeout
- hedging families: hedging strategy, endpoint timeout, endpoint breaker, endpoint rate limiter
- selection surface: authority-based pipeline selection and custom request-key selection
- routing surface: ordered groups, weighted groups, weighted endpoints, selection mode
- predicate surface: transient HTTP status, exception, timeout, cancellation boundaries
- context surface: request-message resilience context, pipeline options reload, pipeline disposal callback
- generated validators: option validators for strategy and routing option records

[LOCAL_ADMISSION]:
- Each outbound seam has one resilience policy chain.
- Hedging is admitted only when the remote operation is idempotent by policy.
- Routing groups are explicit package policy, not hidden URI rewriting.
- Request metadata enters the Polly context through request-message extensions.
- Domain retry schedules and HTTP resilience pipelines never stack on the same seam.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Http.Resilience`
- Owns: HTTP boundary resilience
- Accept: outbound retry policy stays seam-local
- Reject: nested retry loops
