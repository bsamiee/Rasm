# [RASM_APPHOST_API_POLLY_CORE]

`Polly.Core` supplies resilience pipeline builders, executable pipelines,
context pooling, outcome values, predicate builders, strategy options, registry
surfaces, and telemetry event contracts for non-HTTP and shared resilience rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Polly.Core`
- package: `Polly.Core`
- assembly: `Polly.Core`
- namespace: `Polly`
- namespace: `Polly.Retry`
- namespace: `Polly.Timeout`
- namespace: `Polly.CircuitBreaker`
- namespace: `Polly.Hedging`
- namespace: `Polly.Fallback`
- namespace: `Polly.Registry`
- namespace: `Polly.Telemetry`
- asset: runtime library
- rail: resilience

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and execution family
- rail: resilience

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                      |
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
- rail: resilience

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [RAIL]                    |
| :-----: | :--------------------------------- | :--------------- | :------------------------ |
|  [01]   | `RetryStrategyOptions<T>`          | strategy options | retry schedule            |
|  [02]   | `TimeoutStrategyOptions`           | strategy options | execution timeout         |
|  [03]   | `CircuitBreakerStrategyOptions<T>` | strategy options | circuit-breaker policy    |
|  [04]   | `HedgingStrategyOptions<T>`        | strategy options | hedged execution          |
|  [05]   | `FallbackStrategyOptions<T>`       | strategy options | fallback policy           |
|  [06]   | `ResiliencePipelineRegistry<TKey>` | registry         | keyed pipeline lookup     |
|  [07]   | `ResiliencePipelineProvider<TKey>` | provider         | keyed pipeline provider   |
|  [08]   | `CircuitBreakerStateProvider`      | state provider   | breaker state observation |
|  [09]   | `CircuitBreakerManualControl`      | control surface  | manual breaker control    |

[PUBLIC_TYPE_SCOPE]: telemetry and rejection family
- rail: resilience

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [RAIL]                     |
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
- rail: resilience

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]    | [RAIL]                    |
| :-----: | :------------------ | :---------------- | :------------------------ |
|  [01]   | `AddRetry`          | builder extension | retry strategy            |
|  [02]   | `AddTimeout`        | builder extension | timeout strategy          |
|  [03]   | `AddCircuitBreaker` | builder extension | circuit-breaker strategy  |
|  [04]   | `AddHedging`        | builder extension | hedging strategy          |
|  [05]   | `AddFallback`       | builder extension | fallback strategy         |
|  [06]   | `AddPipeline`       | builder extension | nested pipeline strategy  |
|  [07]   | `AddStrategy`       | builder extension | custom strategy admission |

[ENTRYPOINT_SCOPE]: execution operations
- rail: resilience

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------- | :------------- | :------------------------- |
|  [01]   | `Execute`                      | pipeline run   | synchronous execution      |
|  [02]   | `ExecuteAsync`                 | pipeline run   | asynchronous execution     |
|  [03]   | `ExecuteOutcomeAsync`          | pipeline run   | captured outcome execution |
|  [04]   | `ResilienceContextPool.Get`    | context lease  | pooled context checkout    |
|  [05]   | `ResilienceContextPool.Return` | context return | pooled context return      |

## [04]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- namespaces: pipeline, retry, timeout, circuit breaker, hedging, fallback, registry, telemetry
- builder surface: ordered strategy composition through pipeline builders
- execution surface: pipeline execution over callbacks, cancellation, context, and state
- outcome surface: result/exception values for strategy predicates and advanced callbacks
- context surface: pooled context carrying operation key, cancellation, properties, and telemetry identity
- registry surface: keyed pipeline construction and lookup
- telemetry surface: strategy event values, sources, severities, and listeners
- property-key surface: `readonly struct ResiliencePropertyKey<T>` in namespace `Polly` carries a single `string key` constructor and a `string Key` accessor; it keys `ResilienceContext.Properties` for typed property set/get
- fallback-carrier surface: `readonly struct FallbackActionArguments<T>` in namespace `Polly.Fallback` carries `ResilienceContext Context` and inbound `Outcome<T> Outcome` accessors and a `(ResilienceContext context, Outcome<T> outcome)` constructor; `FallbackStrategyOptions<T>.FallbackAction` is typed `Func<FallbackActionArguments<T>, ValueTask<Outcome<T>>>`

[LOCAL_ADMISSION]:
- Resilience policy is a composed value built once and injected as a boundary capability.
- Retry, timeout, circuit breaker, fallback, hedging, and custom strategy order is explicit.
- Context pooling requires strict get/return ownership; execution code does not retain contexts.
- Outcomes flow through predicates and strategy callbacks without throwing from policy callbacks.
- Pipeline registry keys are policy identities, not service-locator strings.

[RAIL_LAW]:
- Package: `Polly.Core`
- Owns: shared and non-HTTP resilience pipelines
- Accept: explicit strategy chains and keyed pipeline policy
- Reject: nested ad hoc retry loops
