# [RASM_APPHOST_API_POLLY_CORE]

`Polly.Core` supplies resilience pipeline builders, executable pipelines,
context pooling, outcome values, predicate builders, strategy options, registry
surfaces, and telemetry event contracts for non-HTTP and shared resilience rails.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and execution family
- rail: resilience

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                      |
| :-----: | :------------------------------ | :------------------ | :-------------------------- |
|   [1]   | `ResiliencePipeline`            | executable pipeline | non-generic execution       |
|   [2]   | `ResiliencePipeline<T>`         | executable pipeline | result-typed execution      |
|   [3]   | `ResiliencePipelineBuilder`     | builder             | non-generic strategy chain  |
|   [4]   | `ResiliencePipelineBuilder<T>`  | builder             | result-typed strategy chain |
|   [5]   | `ResiliencePipelineBuilderBase` | builder base        | shared builder state        |
|   [6]   | `ResilienceContext`             | execution context   | operation metadata          |
|   [7]   | `ResilienceContextPool`         | context pool        | context reuse               |
|   [8]   | `Outcome<T>`                    | result value        | exception/result outcome    |
|   [9]   | `PredicateBuilder<T>`           | predicate builder   | handled outcome predicate   |

[PUBLIC_TYPE_SCOPE]: strategy and registry family
- rail: resilience

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [RAIL]                    |
| :-----: | :--------------------------------- | :--------------- | :------------------------ |
|   [1]   | `RetryStrategyOptions<T>`          | strategy options | retry schedule            |
|   [2]   | `TimeoutStrategyOptions`           | strategy options | execution timeout         |
|   [3]   | `CircuitBreakerStrategyOptions<T>` | strategy options | circuit-breaker policy    |
|   [4]   | `HedgingStrategyOptions<T>`        | strategy options | hedged execution          |
|   [5]   | `FallbackStrategyOptions<T>`       | strategy options | fallback policy           |
|   [6]   | `ResiliencePipelineRegistry<TKey>` | registry         | keyed pipeline lookup     |
|   [7]   | `ResiliencePipelineProvider<TKey>` | provider         | keyed pipeline provider   |
|   [8]   | `CircuitBreakerStateProvider`      | state provider   | breaker state observation |
|   [9]   | `CircuitBreakerManualControl`      | control surface  | manual breaker control    |

[PUBLIC_TYPE_SCOPE]: telemetry and rejection family
- rail: resilience

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [RAIL]                     |
| :-----: | :--------------------------- | :------------------ | :------------------------- |
|   [1]   | `ExecutionRejectedException` | rejection exception | strategy rejection         |
|   [2]   | `TimeoutRejectedException`   | rejection exception | timeout rejection          |
|   [3]   | `BrokenCircuitException`     | rejection exception | open circuit rejection     |
|   [4]   | `IsolatedCircuitException`   | rejection exception | isolated circuit rejection |
|   [5]   | `ResilienceEvent`            | telemetry value     | resilience event           |
|   [6]   | `ResilienceTelemetrySource`  | telemetry source    | pipeline telemetry origin  |
|   [7]   | `TelemetryListener`          | telemetry listener  | event listener             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder operations
- rail: resilience

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]    | [RAIL]                    |
| :-----: | :------------------ | :---------------- | :------------------------ |
|   [1]   | `AddRetry`          | builder extension | retry strategy            |
|   [2]   | `AddTimeout`        | builder extension | timeout strategy          |
|   [3]   | `AddCircuitBreaker` | builder extension | circuit-breaker strategy  |
|   [4]   | `AddHedging`        | builder extension | hedging strategy          |
|   [5]   | `AddFallback`       | builder extension | fallback strategy         |
|   [6]   | `AddPipeline`       | builder extension | nested pipeline strategy  |
|   [7]   | `AddStrategy`       | builder extension | custom strategy admission |

[ENTRYPOINT_SCOPE]: execution operations
- rail: resilience

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------- | :------------- | :------------------------- |
|   [1]   | `Execute`                      | pipeline run   | synchronous execution      |
|   [2]   | `ExecuteAsync`                 | pipeline run   | asynchronous execution     |
|   [3]   | `ExecuteOutcomeAsync`          | pipeline run   | captured outcome execution |
|   [4]   | `ResilienceContextPool.Get`    | context lease  | pooled context checkout    |
|   [5]   | `ResilienceContextPool.Return` | context return | pooled context return      |

## [4]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- namespaces: pipeline, retry, timeout, circuit breaker, hedging, fallback, registry, telemetry
- builder surface: ordered strategy composition through pipeline builders
- execution surface: pipeline execution over callbacks, cancellation, context, and state
- outcome surface: result/exception values for strategy predicates and advanced callbacks
- context surface: pooled context carrying operation key, cancellation, properties, and telemetry identity
- registry surface: keyed pipeline construction and lookup
- telemetry surface: strategy event values, sources, severities, and listeners

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
