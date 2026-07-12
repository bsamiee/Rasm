# [RASM_APPHOST_API_RESILIENCE]

`Microsoft.Extensions.Http.Resilience` supplies outbound HTTP resilience handlers, standard pipelines, hedging pipelines, routing groups, predicates, request context bridges, and policy options for remote hops.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Resilience`
- package: `Microsoft.Extensions.Http.Resilience`
- assembly: `Microsoft.Extensions.Http.Resilience`
- namespace: `Microsoft.Extensions.Http.Resilience`
- asset: runtime library
- rail: resilience

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handler and pipeline family
- rail: resilience

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [RAIL]                    |
| :-----: | :--------------------------------------- | :----------------- | :------------------------ |
|  [01]   | `ResilienceHandler`                      | delegating handler | outbound pipeline         |
|  [02]   | `ResilienceHandlerContext`               | handler context    | named pipeline context    |
|  [03]   | `IHttpResiliencePipelineBuilder`         | builder contract   | custom pipeline builder   |
|  [04]   | `IHttpStandardResiliencePipelineBuilder` | builder contract   | standard pipeline builder |
|  [05]   | `IStandardHedgingHandlerBuilder`         | hedging builder    | standard hedging setup    |
|  [06]   | `IRoutingStrategyBuilder`                | routing builder    | hedging route setup       |

[PUBLIC_TYPE_SCOPE]: strategy options family
- rail: resilience

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [RAIL]                  |
| :-----: | :------------------------------------- | :---------------- | :---------------------- |
|  [01]   | `HttpStandardResilienceOptions`        | standard policy   | retry/timeout/breaker   |
|  [02]   | `HttpStandardHedgingResilienceOptions` | hedging policy    | hedging pipeline        |
|  [03]   | `HttpRetryStrategyOptions`             | retry policy      | transient retry         |
|  [04]   | `HttpTimeoutStrategyOptions`           | timeout policy    | attempt timeout         |
|  [05]   | `HttpCircuitBreakerStrategyOptions`    | breaker policy    | circuit breaker         |
|  [06]   | `HttpRateLimiterStrategyOptions`       | rate-limit policy | request concurrency     |
|  [07]   | `HttpHedgingStrategyOptions`           | hedging policy    | parallel attempt policy |
|  [08]   | `HedgingEndpointOptions`               | endpoint policy   | endpoint resilience     |

[PUBLIC_TYPE_SCOPE]: routing and predicate family
- rail: resilience

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :-------------------------------------- | :---------------- | :----------------------- |
|  [01]   | `OrderedGroupsRoutingOptions`           | route policy      | ordered endpoint groups  |
|  [02]   | `WeightedGroupsRoutingOptions`          | route policy      | weighted endpoint groups |
|  [03]   | `UriEndpoint`                           | endpoint value    | route target             |
|  [04]   | `WeightedUriEndpoint`                   | weighted endpoint | weighted route target    |
|  [05]   | `UriEndpointGroup`                      | endpoint group    | ordered group member     |
|  [06]   | `WeightedUriEndpointGroup`              | weighted group    | weighted group member    |
|  [07]   | `WeightedGroupSelectionMode`            | selection enum    | weighted selection       |
|  [08]   | `HttpClientResiliencePredicates`        | predicate family  | standard transient test  |
|  [09]   | `HttpClientHedgingResiliencePredicates` | predicate family  | hedging transient test   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler operations
- rail: resilience

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]        | [RAIL]                     |
| :-----: | :------------------------------------ | :-------------------- | :------------------------- |
|  [01]   | `AddStandardResilienceHandler`        | HTTP client extension | standard pipeline setup    |
|  [02]   | `AddStandardHedgingHandler`           | HTTP client extension | hedging pipeline setup     |
|  [03]   | `AddResilienceHandler`                | HTTP client extension | custom pipeline setup      |
|  [04]   | `RemoveAllResilienceHandlers`         | HTTP client extension | pipeline reset             |
|  [05]   | `Configure(configuration)`            | options binding       | configuration-section bind |
|  [06]   | `Configure(Action)`                   | options delegate      | direct policy setup        |
|  [07]   | `Configure(Action, IServiceProvider)` | options delegate      | service-aware setup        |
|  [08]   | `SelectPipelineByAuthority`           | pipeline selector     | authority-keyed pipeline   |
|  [09]   | `SelectPipelineBy`                    | pipeline selector     | custom-keyed pipeline      |
|  [10]   | `DisableForUnsafeHttpMethods`         | retry filter          | idempotency guard          |
|  [11]   | `DisableFor`                          | retry filter          | method-specific guard      |

[ENTRYPOINT_SCOPE]: routing and context operations
- rail: resilience

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :---------------------------- | :---------------- | :---------------------- |
|  [01]   | `ConfigureOrderedGroups`      | routing setup     | ordered route groups    |
|  [02]   | `ConfigureWeightedGroups`     | routing setup     | weighted route groups   |
|  [03]   | `IsTransient`                 | predicate         | retry/hedging predicate |
|  [04]   | `IsTransientHttpFailure`      | predicate         | status-code predicate   |
|  [05]   | `IsTransientHttpException`    | predicate         | exception predicate     |
|  [06]   | `ResilienceHandler.SendAsync` | handler execution | async HTTP pipeline     |
|  [07]   | `EnableReloads<TOptions>`     | context reload    | named options reload    |
|  [08]   | `GetOptions<TOptions>`        | context lookup    | named options lookup    |
|  [09]   | `OnPipelineDisposed`          | context callback  | pipeline disposal hook  |
|  [10]   | `GetResilienceContext`        | request extension | request context read    |
|  [11]   | `SetResilienceContext`        | request extension | request context write   |
|  [12]   | `GetRequestMessage`           | Polly extension   | context request read    |
|  [13]   | `SetRequestMessage`           | Polly extension   | context request write   |

## [04]-[IMPLEMENTATION_LAW]

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
