# [RASM_APPHOST_API_POLLY_CORE]

`Polly.Core` mints the resilience-pipeline substrate for every shared and non-HTTP rail: builders fold ordered strategies into one executable `ResiliencePipeline`, a keyed registry resolves pipelines by policy identity, and a pooled `ResilienceContext` threads outcome, cancellation, and telemetry through each run. HTTP boundary resilience composes this substrate through its own handler package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Polly.Core`
- package: `Polly.Core`
- assembly: `Polly.Core`
- namespace: `Polly`, `Polly.Retry`, `Polly.Timeout`, `Polly.CircuitBreaker`, `Polly.Hedging`, `Polly.Fallback`, `Polly.Registry`, `Polly.Simmy`, `Polly.Telemetry`
- asset: runtime library
- rail: resilience

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and execution family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [CAPABILITY]                |
| :-----: | :------------------------------ | :------------------ | :-------------------------- |
|  [01]   | `ResiliencePipeline`            | executable pipeline | non-generic execution       |
|  [02]   | `ResiliencePipeline<T>`         | executable pipeline | result-typed execution      |
|  [03]   | `ResiliencePipelineBuilder`     | builder             | non-generic strategy chain  |
|  [04]   | `ResiliencePipelineBuilder<T>`  | builder             | result-typed strategy chain |
|  [05]   | `ResiliencePipelineBuilderBase` | builder base        | shared builder state        |
|  [06]   | `ResilienceContext`             | execution context   | operation metadata          |
|  [07]   | `ResilienceContextPool`         | context pool        | context reuse               |
|  [08]   | `Outcome<T>`                    | result value        | exception/result outcome    |
|  [09]   | `PredicateBuilder<T>`           | predicate builder   | handled outcome predicate   |
|  [10]   | `ResiliencePropertyKey<T>`      | property key        | typed context property key  |
|  [11]   | `FallbackActionArguments<T>`    | callback arguments  | fallback action carrier     |

[PUBLIC_TYPE_SCOPE]: strategy and registry family

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :--------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `RetryStrategyOptions<T>`          | strategy options | retry schedule                            |
|  [02]   | `DelayBackoffType`                 | backoff enum     | `Constant`/`Linear`/`Exponential` growth  |
|  [03]   | `TimeoutStrategyOptions`           | strategy options | execution timeout                         |
|  [04]   | `CircuitBreakerStrategyOptions<T>` | strategy options | circuit-breaker policy                    |
|  [05]   | `CircuitBreakerStrategyOptions`    | strategy options | non-generic breaker policy (`: <object>`) |
|  [06]   | `HedgingStrategyOptions<T>`        | strategy options | hedged execution                          |
|  [07]   | `FallbackStrategyOptions<T>`       | strategy options | fallback policy                           |
|  [08]   | `ResiliencePipelineRegistry<TKey>` | registry         | keyed pipeline lookup                     |
|  [09]   | `ResiliencePipelineProvider<TKey>` | provider         | keyed pipeline provider                   |
|  [10]   | `CircuitBreakerStateProvider`      | state provider   | breaker state observation                 |
|  [11]   | `CircuitBreakerManualControl`      | control surface  | manual breaker control                    |
|  [12]   | `ChaosLatencyStrategyOptions`      | chaos options    | Simmy latency injection                   |
|  [13]   | `ChaosFaultStrategyOptions`        | chaos options    | Simmy fault injection                     |
|  [14]   | `ChaosOutcomeStrategyOptions<T>`   | chaos options    | Simmy outcome substitution                |
|  [15]   | `ChaosBehaviorStrategyOptions`     | chaos options    | Simmy behavior injection                  |

[PUBLIC_TYPE_SCOPE]: telemetry and rejection family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [CAPABILITY]               |
| :-----: | :--------------------------- | :------------------ | :------------------------- |
|  [01]   | `ExecutionRejectedException` | rejection exception | strategy rejection         |
|  [02]   | `TimeoutRejectedException`   | rejection exception | timeout rejection          |
|  [03]   | `BrokenCircuitException`     | rejection exception | open circuit rejection     |
|  [04]   | `IsolatedCircuitException`   | rejection exception | isolated circuit rejection |
|  [05]   | `ResilienceEvent`            | telemetry value     | resilience event           |
|  [06]   | `ResilienceTelemetrySource`  | telemetry source    | pipeline telemetry origin  |
|  [07]   | `TelemetryListener`          | telemetry listener  | event listener             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder operations

| [INDEX] | [SURFACE]           | [SHAPE] | [CAPABILITY]              |
| :-----: | :------------------ | :------ | :------------------------ |
|  [01]   | `AddRetry`          | fold    | retry strategy            |
|  [02]   | `AddTimeout`        | fold    | timeout strategy          |
|  [03]   | `AddCircuitBreaker` | fold    | circuit-breaker strategy  |
|  [04]   | `AddHedging`        | fold    | hedging strategy          |
|  [05]   | `AddFallback`       | fold    | fallback strategy         |
|  [06]   | `AddPipeline`       | fold    | nested pipeline strategy  |
|  [07]   | `AddStrategy`       | fold    | custom strategy admission |
|  [08]   | `AddChaosLatency`   | fold    | Simmy latency strategy    |
|  [09]   | `AddChaosFault`     | fold    | Simmy fault strategy      |
|  [10]   | `AddChaosOutcome`   | fold    | Simmy outcome strategy    |
|  [11]   | `AddChaosBehavior`  | fold    | Simmy behavior strategy   |

[ENTRYPOINT_SCOPE]: execution operations

| [INDEX] | [SURFACE]                      | [SHAPE]  | [CAPABILITY]                |
| :-----: | :----------------------------- | :------- | :-------------------------- |
|  [01]   | `Execute`                      | instance | synchronous execution       |
|  [02]   | `ExecuteAsync`                 | instance | asynchronous execution      |
|  [03]   | `ExecuteOutcomeAsync`          | instance | captured outcome execution  |
|  [04]   | `ResilienceContextPool.Shared` | static   | static default context pool |
|  [05]   | `ResilienceContextPool.Get`    | instance | pooled context checkout     |
|  [06]   | `ResilienceContextPool.Return` | instance | pooled context return       |

[ENTRYPOINT_SCOPE]: provider, predicate, and outcome operations

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]       |
| :-----: | :------------------------------------------------------ | :------- | :----------------- |
|  [01]   | `ResiliencePipelineProvider<TKey>.GetPipeline`          | instance | untyped pipeline   |
|  [02]   | `ResiliencePipelineProvider<TKey>.GetPipeline<TResult>` | instance | typed pipeline     |
|  [03]   | `ResiliencePipelineProvider<TKey>.TryGetPipeline`       | instance | non-throwing probe |
|  [04]   | `PredicateBuilder<T>.Handle<TException>`                | fold     | exception arm      |
|  [05]   | `PredicateBuilder<T>.HandleResult`                      | fold     | result arm         |
|  [06]   | `Outcome.FromResult<TResult>`                           | factory  | result outcome     |

- `ResiliencePipelineProvider<TKey>.TryGetPipeline`: out-parameter probe returning a non-throwing `bool`.
- `PredicateBuilder<T>.HandleResult`: accepts a `Func<T, bool>` predicate or a result value.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Builders fold strategies in explicit order into one executable `ResiliencePipeline`; `AddStrategy` admits a custom arm on the same chain.
- Execution captures every result and exception as `Outcome<T>`; `PredicateBuilder<T>` composes `ShouldHandle` and strategy callbacks read the outcome without throwing.
- Each strategy ships a generic `…StrategyOptions<T>`; circuit-breaker also ships a non-generic `CircuitBreakerStrategyOptions : <object>` for the non-generic builder.
- One `ResiliencePipelineRegistry<TKey>` owns pipeline identity; `ResiliencePipelineProvider<TKey>` resolves a built pipeline by policy key.
- A pooled `ResilienceContext` carries operation key, cancellation, typed `Properties`, and telemetry identity; `ResilienceContextPool.Shared` is the default pool.

[STACKING]:
- `Microsoft.Extensions.Http.Resilience`(`.api/api-resilience.md`): outbound handlers build their standard and hedging pipelines on `ResiliencePipelineBuilder`, and request metadata bridges into `ResilienceContext.Properties` through the request-context extensions.
- `Polly.Extensions`(`.api/api-polly-extensions.md`): `ConfigureTelemetry` inserts a `TelemetryListener` at the pipeline head, and DI `AddResiliencePipeline`/`AddResiliencePipelineRegistry` register keyed pipelines into one `ResiliencePipelineRegistry<TKey>`.
- `Polly.RateLimiting`(`.api/api-polly-ratelimiting.md`): `AddRateLimiter` folds a rate-limiter strategy onto this builder, rejected admissions surfacing as `RateLimiterRejectedException`.
- AppHost composition: each pipeline is built once through `ResiliencePipelineBuilder`, registered under its policy key, and the built `ResiliencePipeline` is injected as a boundary capability.

[LOCAL_ADMISSION]:
- Resilience policy is a composed value, built once and injected as a boundary capability.
- Strategy order — retry, timeout, circuit breaker, fallback, hedging, custom — is explicit on the builder chain.
- Context pooling holds strict get/return ownership; execution code never retains a context.
- Outcomes flow through predicates and strategy callbacks; policy callbacks never throw.
- Registry keys are policy identities, never service-locator strings.

[RAIL_LAW]:
- Package: `Polly.Core`
- Owns: shared and non-HTTP resilience pipelines
- Accept: explicit strategy chains and keyed pipeline policy
- Reject: nested ad hoc retry loops
