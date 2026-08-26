# [RASM_APPHOST_API_POLLY_CORE]

`Polly.Core` mints the resilience-pipeline substrate for every shared and non-HTTP call path: builders fold ordered strategies into one executable `ResiliencePipeline`, a keyed registry resolves pipelines by policy identity, and a pooled `ResilienceContext` threads outcome, cancellation, and telemetry through each run. Every knob is a validated options property proved at `Add*` time, and `Polly.Simmy` chaos ships inside this same assembly. HTTP boundary resilience composes this substrate through its own handler package.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline, builder, and execution family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]       | [CAPABILITY]                                    |
| :-----: | :----------------------------------- | :------------------ | :---------------------------------------------- |
|  [01]   | `ResiliencePipeline`                 | executable pipeline | non-generic execution, `Empty` absent value     |
|  [02]   | `ResiliencePipeline<T>`              | executable pipeline | result-typed execution, `TResult : T` calls     |
|  [03]   | `ResiliencePipelineBuilder`          | builder             | non-generic strategy chain                      |
|  [04]   | `ResiliencePipelineBuilder<T>`       | builder             | result-typed strategy chain                     |
|  [05]   | `ResiliencePipelineBuilderBase`      | builder base        | shared identity, clock, and pool slots          |
|  [06]   | `ResilienceStrategy`                 | strategy base       | custom non-generic strategy arm                 |
|  [07]   | `ResilienceStrategy<T>`              | strategy base       | custom result-typed strategy arm                |
|  [08]   | `ResilienceStrategyOptions`          | options base        | strategy `Name` slot every options row holds    |
|  [09]   | `StrategyBuilderContext`             | build context       | custom-strategy telemetry and clock             |
|  [10]   | `ResilienceContext`                  | execution context   | operation metadata                              |
|  [11]   | `ResilienceContextPool`              | context pool        | context reuse                                   |
|  [12]   | `ResilienceContextCreationArguments` | lease arguments     | pooled-checkout parameter carrier               |
|  [13]   | `ResilienceProperties`               | property bag        | typed side channel on the context               |
|  [14]   | `ResiliencePropertyKey<T>`           | property key        | typed context property key                      |
|  [15]   | `Outcome<T>`                         | result value        | exception/result outcome                        |
|  [16]   | `Outcome`                            | outcome factory     | outcome minting entry                           |
|  [17]   | `PredicateBuilder<T>`                | predicate builder   | handled outcome predicate                       |
|  [18]   | `PredicateBuilder`                   | predicate builder   | non-generic form (`: PredicateBuilder<object>`) |
|  [19]   | `PredicateResult`                    | predicate factory   | completed `ValueTask<bool>` verdicts            |

[PUBLIC_TYPE_SCOPE]: strategy options family — generic arity is per strategy, never uniform

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CAPABILITY]                                         |
| :-----: | :--------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `RetryStrategyOptions<T>`          | strategy options | retry schedule                                       |
|  [02]   | `RetryStrategyOptions`             | strategy options | non-generic retry (`: RetryStrategyOptions<object>`) |
|  [03]   | `DelayBackoffType`                 | backoff enum     | `Constant`/`Linear`/`Exponential` growth             |
|  [04]   | `TimeoutStrategyOptions`           | strategy options | execution timeout, non-generic only                  |
|  [05]   | `CircuitBreakerStrategyOptions<T>` | strategy options | circuit-breaker policy                               |
|  [06]   | `CircuitBreakerStrategyOptions`    | strategy options | non-generic breaker (`: …Options<object>`)           |
|  [07]   | `CircuitState`                     | state enum       | `Closed`/`Open`/`HalfOpen`/`Isolated`                |
|  [08]   | `CircuitBreakerStateProvider`      | state provider   | breaker state observation, single-attach             |
|  [09]   | `CircuitBreakerManualControl`      | control surface  | isolate/close across every attached breaker          |
|  [10]   | `HedgingStrategyOptions<T>`        | strategy options | hedged execution, generic only                       |
|  [11]   | `FallbackStrategyOptions<T>`       | strategy options | fallback policy, generic only                        |

[PUBLIC_TYPE_SCOPE]: registry family

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]     | [CAPABILITY]                                   |
| :-----: | :---------------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `ResiliencePipelineRegistry<TKey>`        | registry          | keyed lookup, `IDisposable`/`IAsyncDisposable` |
|  [02]   | `ResiliencePipelineProvider<TKey>`        | provider          | keyed pipeline provider                        |
|  [03]   | `ResiliencePipelineRegistryOptions<TKey>` | registry options  | comparers and telemetry-name formatters        |
|  [04]   | `ConfigureBuilderContext<TKey>`           | configure context | reload token and dispose-reclaim seats         |

[PUBLIC_TYPE_SCOPE]: telemetry, chaos, and rejection family

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `ExecutionRejectedException`               | rejection exception | strategy rejection, carries `TelemetrySource` |
|  [02]   | `TimeoutRejectedException`                 | rejection exception | timeout rejection, carries `Timeout`          |
|  [03]   | `BrokenCircuitException`                   | rejection exception | open circuit, carries `RetryAfter`            |
|  [04]   | `IsolatedCircuitException`                 | rejection exception | operator-forced open, derives from the above  |
|  [05]   | `ResilienceEvent`                          | telemetry value     | severity beside event name                    |
|  [06]   | `ResilienceEventSeverity`                  | severity enum       | `None`…`Critical`, `None` suppresses          |
|  [07]   | `ResilienceTelemetrySource`                | telemetry source    | pipeline, instance, and strategy identity     |
|  [08]   | `ResilienceStrategyTelemetry`              | telemetry writer    | custom-strategy event reporting               |
|  [09]   | `TelemetryListener`                        | telemetry listener  | event listener                                |
|  [10]   | `ChaosStrategyOptions`                     | chaos options base  | enablement, rate, and randomizer gate         |
|  [11]   | `ChaosStrategy` / `ChaosStrategy<T>`       | chaos base          | custom gated injection arm                    |
|  [12]   | `ChaosLatencyStrategyOptions`              | chaos options       | Simmy latency injection                       |
|  [13]   | `ChaosFaultStrategyOptions`                | chaos options       | Simmy fault injection                         |
|  [14]   | `ChaosOutcomeStrategyOptions<T>`           | chaos options       | Simmy outcome substitution                    |
|  [15]   | `ChaosBehaviorStrategyOptions`             | chaos options       | Simmy behavior injection                      |
|  [16]   | `Polly.Simmy.Fault.FaultGenerator`         | weighted catalogue  | weighted exception mix                        |
|  [17]   | `Polly.Simmy.Outcomes.OutcomeGenerator<T>` | weighted catalogue  | weighted result-or-exception mix              |

[PUBLIC_TYPE_SCOPE]: callback argument carriers — every one a `readonly struct` over the executing context

| [INDEX] | [SYMBOL]                              | [READS]                                     |
| :-----: | :------------------------------------ | :------------------------------------------ |
|  [01]   | `RetryPredicateArguments<T>`          | outcome beside `AttemptNumber`              |
|  [02]   | `RetryDelayGeneratorArguments<T>`     | outcome beside attempt for delay override   |
|  [03]   | `OnRetryArguments<T>`                 | outcome, attempt, applied delay, elapsed    |
|  [04]   | `CircuitBreakerPredicateArguments<T>` | outcome under breaker classification        |
|  [05]   | `BreakDurationGeneratorArguments`     | failure rate, count, and half-open probes   |
|  [06]   | `OnCircuitOpenedArguments<T>`         | outcome, break duration, and `IsManual`     |
|  [07]   | `OnCircuitClosedArguments<T>`         | outcome and `IsManual` on reset             |
|  [08]   | `OnCircuitHalfOpenedArguments`        | context alone on probe admission            |
|  [09]   | `TimeoutGeneratorArguments`           | context for a per-execution deadline        |
|  [10]   | `OnTimeoutArguments`                  | context and the elapsed timeout             |
|  [11]   | `HedgingPredicateArguments<T>`        | outcome and a nullable attempt number       |
|  [12]   | `HedgingActionGeneratorArguments<T>`  | primary and action contexts plus callback   |
|  [13]   | `HedgingDelayGeneratorArguments`      | attempt for a per-attempt hedge delay       |
|  [14]   | `OnHedgingArguments<T>`               | both contexts and attempt, never an outcome |
|  [15]   | `FallbackPredicateArguments<T>`       | outcome under fallback classification       |
|  [16]   | `FallbackActionArguments<T>`          | context and inbound outcome to substitute   |
|  [17]   | `OnFallbackArguments<T>`              | outcome the substitution replaced           |
|  [18]   | `EnabledGeneratorArguments`           | context for a per-execution chaos gate      |
|  [19]   | `InjectionRateGeneratorArguments`     | context for a per-execution chaos rate      |
|  [20]   | `FaultGeneratorArguments`             | context for the injected exception draw     |
|  [21]   | `OutcomeGeneratorArguments`           | context for the substituted outcome draw    |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder composition and identity

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `ResiliencePipelineBuilderBase.Name`              | property | `pipeline.name` telemetry dimension         |
|  [02]   | `ResiliencePipelineBuilderBase.InstanceName`      | property | `pipeline.instance` telemetry dimension     |
|  [03]   | `ResiliencePipelineBuilderBase.TimeProvider`      | property | one clock for delay, deadline, sampling     |
|  [04]   | `ResiliencePipelineBuilderBase.ContextPool`       | property | pool the context-free overloads lease from  |
|  [05]   | `ResiliencePipelineBuilderBase.TelemetryListener` | property | listener seat below `ConfigureTelemetry`    |
|  [06]   | `AddRetry`                                        | fold     | retry strategy                              |
|  [07]   | `AddTimeout`                                      | fold     | timeout strategy, `TimeSpan` or options     |
|  [08]   | `AddCircuitBreaker`                               | fold     | circuit-breaker strategy                    |
|  [09]   | `AddHedging`                                      | fold     | hedging strategy, typed builder only        |
|  [10]   | `AddFallback`                                     | fold     | fallback strategy, typed builder only       |
|  [11]   | `AddPipeline`                                     | fold     | nested pipeline as external component       |
|  [12]   | `AddStrategy(factory, options)`                   | fold     | custom strategy admission                   |
|  [13]   | `AddStrategy(factory)`                            | fold     | custom strategy under unnamed empty options |
|  [14]   | `Build()`                                         | instance | composite materialization, single-use       |

[ENTRYPOINT_SCOPE]: retry, timeout, and hedging policy

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `RetryStrategyOptions<T>.MaxRetryAttempts`    | property | attempt bound, `[Range(1, int.MaxValue)]` |
|  [02]   | `RetryStrategyOptions<T>.BackoffType`         | property | delay curve selector                      |
|  [03]   | `RetryStrategyOptions<T>.UseJitter`           | property | jitter arming on the selected curve       |
|  [04]   | `RetryStrategyOptions<T>.Delay`               | property | curve base, `[Range]` zero to one day     |
|  [05]   | `RetryStrategyOptions<T>.MaxDelay`            | property | curve ceiling, never a generator ceiling  |
|  [06]   | `RetryStrategyOptions<T>.ShouldHandle`        | property | handled-outcome predicate, `[Required]`   |
|  [07]   | `RetryStrategyOptions<T>.DelayGenerator`      | property | per-attempt delay override                |
|  [08]   | `RetryStrategyOptions<T>.OnRetry`             | property | retry event callback                      |
|  [09]   | `RetryStrategyOptions<T>.Randomizer`          | property | jitter determinism hook, `[Required]`     |
|  [10]   | `TimeoutStrategyOptions.Timeout`              | property | deadline, `[Range]` 10 ms to one day      |
|  [11]   | `TimeoutStrategyOptions.TimeoutGenerator`     | property | per-execution deadline override           |
|  [12]   | `TimeoutStrategyOptions.OnTimeout`            | property | timeout event callback                    |
|  [13]   | `HedgingStrategyOptions<T>.MaxHedgedAttempts` | property | hedge bound, `[Range(1, 10)]`             |
|  [14]   | `HedgingStrategyOptions<T>.Delay`             | property | launch spacing between hedged attempts    |
|  [15]   | `HedgingStrategyOptions<T>.ActionGenerator`   | property | hedged-action factory, `[Required]`       |
|  [16]   | `HedgingStrategyOptions<T>.DelayGenerator`    | property | per-attempt hedge spacing override        |
|  [17]   | `HedgingStrategyOptions<T>.ShouldHandle`      | property | hedge-triggering predicate, `[Required]`  |
|  [18]   | `HedgingStrategyOptions<T>.OnHedging`         | property | hedged-launch callback                    |

[ENTRYPOINT_SCOPE]: circuit-breaker and fallback policy

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `CircuitBreakerStrategyOptions<T>.FailureRatio`               | property | trip ratio, `[Range(0.0, 1.0)]`          |
|  [02]   | `CircuitBreakerStrategyOptions<T>.MinimumThroughput`          | property | sample floor, `[Range(2, int.MaxValue)]` |
|  [03]   | `CircuitBreakerStrategyOptions<T>.SamplingDuration`           | property | statistics window                        |
|  [04]   | `CircuitBreakerStrategyOptions<T>.BreakDuration`              | property | open-state dwell                         |
|  [05]   | `CircuitBreakerStrategyOptions<T>.BreakDurationGenerator`     | property | escalating break length                  |
|  [06]   | `CircuitBreakerStrategyOptions<T>.ShouldHandle`               | property | failure predicate, `[Required]`          |
|  [07]   | `CircuitBreakerStrategyOptions<T>.OnOpened`                   | property | trip callback                            |
|  [08]   | `CircuitBreakerStrategyOptions<T>.OnClosed`                   | property | reset callback                           |
|  [09]   | `CircuitBreakerStrategyOptions<T>.OnHalfOpened`               | property | probe-admission callback                 |
|  [10]   | `CircuitBreakerStrategyOptions<T>.ManualControl`              | property | operator isolate/close attachment        |
|  [11]   | `CircuitBreakerStrategyOptions<T>.StateProvider`              | property | state-read attachment                    |
|  [12]   | `CircuitBreakerManualControl(bool isIsolated)`                | ctor     | boot-dark construction                   |
|  [13]   | `CircuitBreakerManualControl.IsolateAsync(CancellationToken)` | instance | force every attached breaker open        |
|  [14]   | `CircuitBreakerManualControl.CloseAsync(CancellationToken)`   | instance | release and reset statistics             |
|  [15]   | `CircuitBreakerStateProvider.CircuitState`                    | property | live breaker state                       |
|  [16]   | `FallbackStrategyOptions<T>.FallbackAction`                   | property | substitution action, `[Required]`        |
|  [17]   | `FallbackStrategyOptions<T>.ShouldHandle`                     | property | substitution predicate, `[Required]`     |
|  [18]   | `FallbackStrategyOptions<T>.OnFallback`                       | property | substitution callback                    |

[ENTRYPOINT_SCOPE]: execution, context, and outcome

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Execute`                                                        | instance | synchronous execution          |
|  [02]   | `ExecuteAsync`                                                   | instance | asynchronous execution         |
|  [03]   | `ExecuteOutcomeAsync<TResult, TState>(callback, context, state)` | instance | captured outcome execution     |
|  [04]   | `ResiliencePipeline.Empty`                                       | static   | absent-policy pipeline value   |
|  [05]   | `ResilienceContextPool.Shared`                                   | static   | process default context pool   |
|  [06]   | `ResilienceContextPool.Get(string?, CancellationToken)`          | instance | keyed pooled checkout          |
|  [07]   | `ResilienceContextPool.Get(string?, bool?, CancellationToken)`   | instance | checkout fixing capture policy |
|  [08]   | `ResilienceContextPool.Return`                                   | instance | pooled context return          |
|  [09]   | `ResilienceContext.OperationKey`                                 | property | `operation.key` dimension      |
|  [10]   | `ResilienceContext.CancellationToken`                            | property | token every strategy observes  |
|  [11]   | `ResilienceContext.ContinueOnCapturedContext`                    | property | capture policy for every await |
|  [12]   | `ResilienceContext.Properties`                                   | property | typed side channel             |
|  [13]   | `Outcome.FromResult<TResult>` / `Outcome.FromException<TResult>` | factory  | outcome minting                |
|  [14]   | `Outcome<T>.Exception` / `Outcome<T>.Result`                     | property | captured termination readout   |
|  [15]   | `Outcome<T>.ThrowIfException()`                                  | instance | stack-preserving rethrow       |

[ENTRYPOINT_SCOPE]: registry, provider, and predicate

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `ResiliencePipelineProvider<TKey>.GetPipeline`                  | instance | untyped pipeline, throws on miss    |
|  [02]   | `ResiliencePipelineProvider<TKey>.GetPipeline<TResult>`         | instance | typed pipeline, throws on miss      |
|  [03]   | `ResiliencePipelineProvider<TKey>.TryGetPipeline`               | instance | non-throwing probe                  |
|  [04]   | `ResiliencePipelineRegistry<TKey>.TryAddBuilder`                | instance | synchronous claim verdict           |
|  [05]   | `ResiliencePipelineRegistry<TKey>.GetOrAddPipeline`             | instance | materialize under a configure body  |
|  [06]   | `ResiliencePipelineRegistry<TKey>.DisposeAsync`                 | instance | force-dispose every materialization |
|  [07]   | `ResiliencePipelineRegistryOptions<TKey>.BuilderNameFormatter`  | property | key-to-`pipeline.name` render       |
|  [08]   | `ResiliencePipelineRegistryOptions<TKey>.InstanceNameFormatter` | property | key-to-`pipeline.instance` render   |
|  [09]   | `ResiliencePipelineRegistryOptions<TKey>.BuilderComparer`       | property | claim-identity comparer             |
|  [10]   | `ResiliencePipelineRegistryOptions<TKey>.PipelineComparer`      | property | resolution comparer                 |
|  [11]   | `ConfigureBuilderContext<TKey>.PipelineKey`                     | property | key the configure body reads        |
|  [12]   | `ConfigureBuilderContext<TKey>.AddReloadToken`                  | instance | rebuild trigger registration        |
|  [13]   | `ConfigureBuilderContext<TKey>.OnPipelineDisposed`              | instance | reclaim callback registration       |
|  [14]   | `PredicateBuilder<T>.Handle<TException>`                        | fold     | exception arm                       |
|  [15]   | `PredicateBuilder<T>.HandleInner<TException>`                   | fold     | inner and aggregate exception arm   |
|  [16]   | `PredicateBuilder<T>.HandleResult`                              | fold     | result arm, predicate or value      |
|  [17]   | `PredicateBuilder<T>.Build()`                                   | instance | `Predicate<Outcome<T>>` readout     |
|  [18]   | `PredicateResult.True()` / `PredicateResult.False()`            | static   | completed predicate verdicts        |

[ENTRYPOINT_SCOPE]: chaos policy

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `AddChaosLatency`                                           | fold     | time plane, any builder                          |
|  [02]   | `AddChaosFault`                                             | fold     | exception path, any builder                      |
|  [03]   | `AddChaosBehavior`                                          | fold     | side effect before the call, any builder         |
|  [04]   | `AddChaosOutcome<TResult>`                                  | fold     | result path, `ResiliencePipelineBuilder<T>` only |
|  [05]   | `ChaosStrategyOptions.Enabled`                              | property | build-time injection gate                        |
|  [06]   | `ChaosStrategyOptions.EnabledGenerator`                     | property | per-execution injection gate                     |
|  [07]   | `ChaosStrategyOptions.InjectionRate`                        | property | draw threshold, `[Range(0.0, 1.0)]`              |
|  [08]   | `ChaosStrategyOptions.InjectionRateGenerator`               | property | per-execution draw threshold                     |
|  [09]   | `ChaosStrategyOptions.Randomizer`                           | property | draw determinism hook, `[Required]`              |
|  [10]   | `ChaosLatencyStrategyOptions.Latency`                       | property | injected delay                                   |
|  [11]   | `ChaosFaultStrategyOptions.FaultGenerator`                  | property | injected exception, null return skips            |
|  [12]   | `ChaosOutcomeStrategyOptions<T>.OutcomeGenerator`           | property | substituted outcome, `[Required]`                |
|  [13]   | `ChaosBehaviorStrategyOptions.BehaviorGenerator`            | property | injected side effect                             |
|  [14]   | `FaultGenerator.AddException<TException>(int weight = 100)` | fold     | weighted exception row by type                   |
|  [15]   | `FaultGenerator.AddException(Func<…>, int weight = 100)`    | fold     | weighted exception row by factory                |
|  [16]   | `OutcomeGenerator<T>.AddResult(Func<…>, int weight = 100)`  | fold     | weighted result row                              |
|  [17]   | `OutcomeGenerator<T>.AddException(…, int weight = 100)`     | fold     | weighted exception row on the result path        |

- `ResiliencePipelineProvider<TKey>.TryGetPipeline` carries an untyped and a `<TResult>` overload mirroring `GetPipeline`, each `abstract bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline)`: the whole family is SYNC — no async probe exists — and the `[NotNullWhen(true)]` annotation is what lets a `true` branch read the out value without a null check.
- `PredicateBuilder<T>` reaches a `ShouldHandle` slot through four implicit operators — retry, hedging, fallback, and breaker predicate delegates — so one transient row declares once and converts into every slot; `Build()` on a builder carrying zero arms throws `InvalidOperationException`, which surfaces at the `Add*` call the conversion runs inside.
- `HandleInner<TException>` flattens `AggregateException` and walks the whole `InnerException` chain, so a wrapped transport fault classifies without a hand-written unwrap.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Builders fold strategies in explicit order into one executable `ResiliencePipeline` with the first-added strategy outermost; `AddStrategy` admits a custom arm on the same chain and `AddPipeline` nests an already-built one.
- Execution captures every result and exception as `Outcome<T>`; `PredicateBuilder<T>` composes `ShouldHandle` and strategy callbacks read the outcome without throwing.
- Generic arity is per strategy rather than uniform: retry and circuit-breaker ship a generic row beside a non-generic `: <object>` alias, timeout ships a non-generic row alone, and hedging and fallback ship a generic row alone.
- Result-typed foreclosure is a REAL constraint on a non-generic chain: `AddHedging`, `AddFallback`, and `AddChaosOutcome` each bind `ResiliencePipelineBuilder<TResult>` alone and no non-generic `HedgingStrategyOptions` or `FallbackStrategyOptions` exists, so a `ResiliencePipelineBuilder` cannot hedge, substitute, or inject an outcome — and minting a per-result-type registration purely to buy one of those three is the rejected form wherever a one-pipeline-per-key law holds.
- One `ResiliencePipelineRegistry<TKey>` owns pipeline identity; `ResiliencePipelineProvider<TKey>` resolves a built pipeline by policy key, and generic and non-generic registrations occupy DISJOINT namespaces per result type — a key registered through `AddResiliencePipeline<T>` and probed through the untyped `TryGetPipeline` misses with no diagnostic.
- Pooled `ResilienceContext` instances carry operation key, cancellation, capture policy, typed `Properties`, and telemetry identity; `ResilienceContextPool.Shared` is the default pool, `OperationKey` and `CancellationToken` fix at checkout under `internal` setters, and `Return` resets every slot including `Properties`.

[BUILD_LAW]:
- Every `Add*` call validates its options through DataAnnotations at CALL time and `Build` validates the builder again, so an out-of-range knob throws where it is declared rather than at first execution — a `Timeout` under 10 ms, a `MaxHedgedAttempts` above 10, and a `FailureRatio` above 1.0 are unconstructible.
- Builders are single-use: a second `Add*` after `Build` throws `InvalidOperationException`, so one builder instance composes one pipeline and a reused instance is a boot fault rather than a shared chain.
- Zero strategies build to the empty composite, so a conditional arm returning its builder unchanged costs nothing and `ResiliencePipeline.Empty` is the absent-policy value the same shape yields.
- `AddStrategy(factory, options)` hands the factory a `StrategyBuilderContext` carrying exactly `Telemetry` and `TimeProvider`; the arity-free `AddStrategy(factory)` overload substitutes unnamed empty options, so its events publish a null `strategy.name`.
- `ResiliencePipelineBuilderBase.TimeProvider` falls back to `TimeProvider.System`, so one injection point drives every delay, deadline, sampling window, and injected latency, and a fake clock in a suite reaches all of them at once.

[DEFAULT_HAZARDS]: unset knobs carry shipped values that silently change policy meaning

| [INDEX] | [KNOB]                                               | [SHIPPED_DEFAULT] | [CONSEQUENCE_WHEN_UNSET]                          |
| :-----: | :--------------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `RetryStrategyOptions<T>.BackoffType`                | `Constant`        | exponential growth needs an explicit row          |
|  [02]   | `RetryStrategyOptions<T>.Delay`                      | 2 s               | first retry waits two seconds under any curve     |
|  [03]   | `RetryStrategyOptions<T>.MaxRetryAttempts`           | 3                 | four total calls per logical operation            |
|  [04]   | `RetryStrategyOptions<T>.MaxDelay`                   | `null`            | exponential growth runs uncapped                  |
|  [05]   | `CircuitBreakerStrategyOptions<T>.MinimumThroughput` | 100               | a low-traffic dependency never trips its breaker  |
|  [06]   | `CircuitBreakerStrategyOptions<T>.FailureRatio`      | 0.1               | one failure in ten trips a busy dependency        |
|  [07]   | `CircuitBreakerStrategyOptions<T>.SamplingDuration`  | 30 s              | statistics window outlives most attempt deadlines |
|  [08]   | `CircuitBreakerStrategyOptions<T>.BreakDuration`     | 5 s               | open dwell is fixed unless a generator row lands  |
|  [09]   | `TimeoutStrategyOptions.Timeout`                     | 30 s              | attempt deadline exceeds most transport budgets   |
|  [10]   | `HedgingStrategyOptions<T>.MaxHedgedAttempts`        | 1                 | one hedge, never the row's intended fan           |
|  [11]   | `HedgingStrategyOptions<T>.Delay`                    | 2 s               | tail-latency hedging waits two seconds to launch  |
|  [12]   | `ChaosStrategyOptions.Enabled`                       | `true`            | an added chaos row injects the moment it composes |
|  [13]   | `ChaosStrategyOptions.InjectionRate`                 | 0.001             | injection runs at one draw in a thousand          |
|  [14]   | `ChaosLatencyStrategyOptions.Latency`                | 30 s              | injected delay dwarfs any realistic deadline      |
|  [15]   | `CircuitBreakerStateProvider.CircuitState`           | `Closed`          | an unattached provider reports healthy forever    |

[STRATEGY_NAMING]:
- Every shipped options ctor STAMPS its own `Name` — `Retry`, `Timeout`, `CircuitBreaker`, `Hedging`, `Fallback`, `Chaos.Latency`, `Chaos.Fault`, `Chaos.Outcome`, `Chaos.Behavior` — so an unnamed strategy publishes its KIND rather than a null dimension, and two strategies of one kind in one pipeline MERGE their series under that shared spelling.
- Distinct `Name` values are what separate two rows of one kind: `(pipeline.name, strategy.name)` is the telemetry deduplication key, so a chain stacking two limiters, two timeouts, or two chaos rows needs a name per row or its events become one indistinguishable stream.
- `pipeline.name` and `pipeline.instance` come from `ResiliencePipelineBuilderBase.Name`/`InstanceName`, which the registry fills through `BuilderNameFormatter` (defaulting to `key.ToString()`) and `InstanceNameFormatter` (defaulting to `null`) — so an instance dimension exists only where a formatter row lands.
- `ExecutionRejectedException.TelemetrySource` is stamped at throw and carries that same triple, so a rejection resolves its emitting strategy back to the row that raised it without parsing the message.

[LIFETIME_LAW]:
- Ownership follows CONSTRUCTION: strategies the builder constructs dispose with the pipeline, `AddPipeline` nests an external component whose disposal never transfers, and any limiter, generator, or state object a consumer hands in stays that consumer's to release.
- `CircuitBreakerStateProvider` is single-attach — a second strategy reusing one instance throws `InvalidOperationException` at build — so per-seat evidence is structural rather than conventional.
- `CircuitBreakerManualControl` attaches to N breakers and survives pipeline generations; a control constructed `isIsolated: true` isolates each breaker AT REGISTRATION through a blocking synchronous call, so a boot-dark group pays that cost inside pipeline construction.
- Operator-forced transitions stay distinguishable at BOTH boundaries: `IsolatedCircuitException` derives from `BrokenCircuitException` so one catch arm covers organic and forced opens while the type attributes the cause, and `OnOpened`/`OnClosed` carry an `IsManual` flag reading the same discriminant without a type test.
- `ResiliencePipelineRegistry<TKey>.DisposeAsync` force-disposes every materialized pipeline, and a reference held past that point throws — breaker statistics and limiter queues are process-local and intentionally unrecoverable.
- `ConfigureBuilderContext<TKey>.AddReloadToken` SILENTLY DROPS a token that cannot be cancelled or is already cancelled, so a reload wired from a stale source registers nothing and reports nothing.

[DELAY_ALGEBRA]:
- Curve arithmetic is `Constant` = base, `Linear` = (n+1)·base, `Exponential` = 2ⁿ·base, and a zero base short-circuits to zero under every curve.
- `UseJitter` applies ±25% uniform spread on the constant and linear curves and swaps the exponential curve for decorrelated jitter, which is what breaks correlated retry storms; `Randomizer` is the one determinism hook for both.
- `MaxDelay` caps the COMPUTED curve alone — a `DelayGenerator` return bypasses it entirely, so an operator ceiling over server-directed delay belongs inside the generator.
- Generator returns below `TimeSpan.Zero` and null returns alike fall back to the curve, so a generator answering one fault family and declining every other is one expression.
- `OnRetry` runs after the failed attempt's outcome is captured and BEFORE that result is discarded and disposed, so a callback storing the result reads freed state; copy what the event needs inside the callback.

[CHAOS_GATE]:
- Injection runs one fixed per-execution sequence — `EnabledGenerator(context)`, then `InjectionRateGenerator(context)`, then `Randomizer() < threshold` — with a cancellation check ahead of each step, and generator presence makes the corresponding `Enabled`/`InjectionRate` scalar ignored, so targeting one tenant, environment, or fraction is a generator row rather than a build-time fork.
- `Randomizer` receives NO `ResilienceContext` while both generators do, so a run that must decide injection from the executing frame decides at `EnabledGenerator`; a generator-returned threshold is COERCED into `[0.0, 1.0]` rather than validated, so an out-of-band computation silently clamps where the `[Range]`-validated `InjectionRate` property refuses at build.
- Outcome injection substitutes the result WITHOUT invoking the callback and needs the result type, which is why `AddChaosOutcome` binds `ResiliencePipelineBuilder<TResult>` alone while the other three bind `ResiliencePipelineBuilderBase`.
- Latency chaos delays through the builder's `TimeProvider` and observes cancellation before invoking the real callback, so injected latency surfaces as a cancelled outcome rather than a wedged call.

[WEIGHTED_CATALOGUE]:
- `Polly.Simmy.Fault.FaultGenerator` and `Polly.Simmy.Outcomes.OutcomeGenerator<T>` are the weighted mix builders — `AddException<TException>(int weight = 100)` beside `AddException(Func<Exception>, weight)`, `AddException(Func<ResilienceContext, Exception>, weight)`, and `AddResult(…)` on the outcome form — and each converts IMPLICITLY into its options slot, so a realistic fault mix is declared as weights rather than branching.
- Weights are relative rather than fractional: a draw runs against the running total, so rows of 70 and 30 split seven to three exactly as rows of 7 and 3 do.
- Weight draws are NOT addressable: both generators construct their internal helper on a fixed process randomizer through a parameterless ctor, and no options member substitutes it — so a weighted catalogue picks a different row every run even beneath a fully deterministic enablement gate, which forecloses replay-exact chaos through this surface.
- Empty catalogues DIVERGE at the boundary: an `OutcomeGenerator<T>` carrying no rows yields a null outcome and skips injection, while a `FaultGenerator` carrying no rows throws `InvalidOperationException` on the first gated execution — so a conditionally populated fault catalogue needs its own emptiness guard.
- Null returns from a hand-written fault, outcome, or behavior generator skip injection for that execution, which is the per-execution opt-out channel a weighted catalogue forfeits — a zero-weight row is unreachable rather than a skip, so per-execution targeting rides `EnabledGenerator` instead.

[STACKING]:
- `Microsoft.Extensions.Http.Resilience`(`.api/api-resilience.md`): outbound handlers build their standard and hedging pipelines on `ResiliencePipelineBuilder`, and request metadata bridges into `ResilienceContext.Properties` through the request-context extensions.
- `Polly.Extensions`(`.api/api-polly-extensions.md`): `ConfigureTelemetry` seats a `TelemetryListener` at the pipeline head over the `ResilienceEvent`/`ResilienceEventSeverity`/`ResilienceTelemetrySource` values this package raises, and DI `AddResiliencePipeline`/`AddResiliencePipelineRegistry` register keyed pipelines into one `ResiliencePipelineRegistry<TKey>` whose `BuilderNameFormatter` supplies every `pipeline.name`.
- `Polly.RateLimiting`(`.api/api-polly-ratelimiting.md`): `AddRateLimiter` folds a limiter strategy onto `ResiliencePipelineBuilderBase`, rejected admissions surfacing as `RateLimiterRejectedException` on this package's `ExecutionRejectedException` hierarchy with the same `TelemetrySource` stamp.
- `Microsoft.Extensions.DependencyInjection`(`.api/api-di.md`): the registry and provider pair resolve as container services, and `ResiliencePipelineRegistryOptions<TKey>` binds through the options system.
- AppHost composition: `Wire/outbound#KEYED_PIPELINES` folds one keyed pipeline per non-HTTP hop and `Runtime/laneguard#LANE_GUARD` folds one per in-process work lane, both over `ResiliencePipelineBuilder`, both resolving through `ResiliencePipelineProvider<string>.TryGetPipeline`, and both folding terminations through `ExecuteOutcomeAsync` over a pooled context.

[LOCAL_ADMISSION]:
- Resilience policy is a composed value, built once and injected as a boundary capability.
- Strategy order is explicit on the builder chain and derives from what each strategy counts — admission counts logical calls outermost, health statistics count attempts, and each attempt earns the innermost deadline.
- Every strategy knob is a derived policy value on its owning row, never a literal at the composition call.
- Context pooling holds strict get/return ownership; execution code never retains a context.
- Outcomes flow through predicates and strategy callbacks; policy callbacks never throw and never mutate the outcome.
- Registry keys are policy identities, never service-locator strings, and one key type serves one registry per container.
- Chaos rows compose below the strategies under test and arm through a runtime gate, never a build-time fork.
