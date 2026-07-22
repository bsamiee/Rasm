# [RASM_APPHOST_API_RESILIENCE]

`Microsoft.Extensions.Http.Resilience` owns the outbound HTTP resilience rail: it folds standard, hedging, and custom `Polly` pipelines onto an `IHttpClientBuilder`, resolves a pipeline per request authority, and bridges request metadata into the resilience context. Every remote hop crosses one seam-local policy chain built on `Polly.Core`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Http.Resilience`
- package: `Microsoft.Extensions.Http.Resilience`
- assembly: `Microsoft.Extensions.Http.Resilience`
- namespace: `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.DependencyInjection`, `Polly`, `System.Net.Http`
- asset: runtime library
- rail: resilience

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handler and pipeline family

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [CAPABILITY]              |
| :-----: | :--------------------------------------- | :----------------- | :------------------------ |
|  [01]   | `ResilienceHandler`                      | delegating handler | outbound pipeline         |
|  [02]   | `ResilienceHandlerContext`               | handler context    | named pipeline context    |
|  [03]   | `IHttpResiliencePipelineBuilder`         | builder contract   | custom pipeline builder   |
|  [04]   | `IHttpStandardResiliencePipelineBuilder` | builder contract   | standard pipeline builder |
|  [05]   | `IStandardHedgingHandlerBuilder`         | hedging builder    | standard hedging setup    |
|  [06]   | `IRoutingStrategyBuilder`                | routing builder    | hedging route setup       |

[PUBLIC_TYPE_SCOPE]: strategy options family

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [CAPABILITY]            |
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

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [CAPABILITY]             |
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

| [INDEX] | [SURFACE]                             | [SHAPE] | [CAPABILITY]               |
| :-----: | :------------------------------------ | :------ | :------------------------- |
|  [01]   | `AddStandardResilienceHandler`        | fold    | standard pipeline setup    |
|  [02]   | `AddStandardHedgingHandler`           | fold    | hedging pipeline setup     |
|  [03]   | `AddResilienceHandler`                | fold    | custom pipeline setup      |
|  [04]   | `RemoveAllResilienceHandlers`         | fold    | pipeline reset             |
|  [05]   | `Configure(configuration)`            | fold    | configuration-section bind |
|  [06]   | `Configure(Action)`                   | fold    | direct policy setup        |
|  [07]   | `Configure(Action, IServiceProvider)` | fold    | service-aware setup        |
|  [08]   | `SelectPipelineByAuthority`           | fold    | authority-keyed pipeline   |
|  [09]   | `SelectPipelineBy`                    | fold    | custom-keyed pipeline      |
|  [10]   | `DisableForUnsafeHttpMethods`         | static  | idempotency guard          |
|  [11]   | `DisableFor`                          | static  | method-specific guard      |
|  [12]   | `ResilienceHandler(pipeline)`         | ctor    | direct pipeline handler    |
|  [13]   | `ResilienceHandler(pipelineProvider)` | ctor    | per-request pipeline       |

[ENTRYPOINT_SCOPE]: routing and context operations

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]             |
| :-----: | :------------------------ | :------- | :----------------------- |
|  [01]   | `ConfigureOrderedGroups`  | fold     | ordered route groups     |
|  [02]   | `ConfigureWeightedGroups` | fold     | weighted route groups    |
|  [03]   | `IsTransient`             | static   | retry/hedging predicate  |
|  [04]   | `EnableReloads<TOptions>` | instance | named options reload     |
|  [05]   | `GetOptions<TOptions>`    | instance | named options lookup     |
|  [06]   | `OnPipelineDisposed`      | instance | pipeline disposal hook   |
|  [07]   | `ServiceProvider`         | property | handler service provider |
|  [08]   | `BuilderName`             | property | pipeline builder name    |
|  [09]   | `InstanceName`            | property | pipeline instance name   |
|  [10]   | `GetResilienceContext`    | static   | request context read     |
|  [11]   | `SetResilienceContext`    | static   | request context write    |
|  [12]   | `GetRequestMessage`       | static   | context request read     |
|  [13]   | `SetRequestMessage`       | static   | context request write    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Standard handler folds retry, total-request-timeout, circuit-breaker, rate-limiter, and attempt-timeout onto one `Polly` pipeline; hedging handler adds parallel endpoint attempts, each with its own timeout and breaker.
- Every strategy binds one `Http…StrategyOptions` record, `HttpStandardResilienceOptions` and `HttpStandardHedgingResilienceOptions` compose the per-strategy records, and source-generated validators reject an invalid option record at build.
- Retry honors the `Retry-After` response header and opts out of unsafe HTTP methods through `DisableForUnsafeHttpMethods`.
- `HttpClientResiliencePredicates.IsTransient` classifies transient status, exception, and timeout outcomes; the hedging predicate family carries its own transient test.
- One handler resolves a pipeline per request key — `SelectPipelineByAuthority` keys on request authority, `SelectPipelineBy` on a custom key.
- Hedging routing folds ordered or weighted `UriEndpoint` groups, and `WeightedGroupSelectionMode` drives weighted selection.
- `ResilienceHandlerContext` threads the handler-scoped service provider, pipeline identity, options reload, and disposal callback into each named pipeline; request metadata bridges into the `Polly` `ResilienceContext` through the request-message extensions.

[STACKING]:
- `Polly.Core`(`.api/api-polly-core.md`): the standard and hedging handlers build their strategy chains on `ResiliencePipelineBuilder`, `ResilienceHandler(pipeline)` wraps the built `ResiliencePipeline<HttpResponseMessage>`, and request metadata threads into `ResilienceContext.Properties` through the request-message extensions.
- `Microsoft.Extensions.ServiceDiscovery`(`.api/api-service-discovery.md`): `AddResilienceHandler` and `IHttpClientBuilder.AddServiceDiscovery` fold onto one client builder with resilience outermost, so a retried attempt re-resolves the endpoint the discovery handler produced.
- Wire outbound composition: the outbound boundary folds one resilience handler per seam and selects the pipeline by authority, so each remote host carries its own retry schedule and breaker state.

[LOCAL_ADMISSION]:
- Each outbound seam carries one resilience policy chain.
- Hedging is admitted only when the remote operation is idempotent by policy.
- Routing groups are explicit package policy, never hidden URI rewriting.
- Request metadata enters the `Polly` context through the request-message extensions.
- Domain retry schedules and HTTP resilience pipelines never stack on one seam.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Http.Resilience`
- Owns: outbound HTTP boundary resilience
- Accept: seam-local retry, timeout, breaker, and hedging pipelines keyed by authority
- Reject: nested retry loops and hand-rolled transient-error classification
