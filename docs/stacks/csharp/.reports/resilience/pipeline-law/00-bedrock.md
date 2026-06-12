# pipeline-law — bedrock

## composition order

- A resilience pipeline executes strategies in declaration order with the first-added strategy outermost: the builder chains components through delegating links, so `AddRetry` before `AddTimeout` means retry observes timeout's rejections.
- Order is the policy: two pipelines with identical strategies in different order are different policies, and the difference is recoverable only from the declaration sequence — no runtime surface reports it.
- Placement decides resource accounting, not just fault flow: a rate limiter outside retry admits one permit per logical call; inside retry it charges one permit per attempt.
- The limiter's lease is held across the entire inner execution — every retry, breaker probe, and deadline inside it rides the one admission.
- A breaker inside retry accumulates per-attempt health statistics (throughput counts attempts); outside retry it sees one outcome per logical call and its `MinimumThroughput` floor is reached N-times slower — breaker placement rescales every breaker threshold.
- A strategy's predicate vocabulary is defined by what sits below it: an outer retry's `ShouldHandle` receives `TimeoutRejectedException` from an inner timeout, `BrokenCircuitException` from an inner breaker, and `RateLimiterRejectedException` from an inner limiter.
- `IsolatedCircuitException` derives from `BrokenCircuitException` — one catch arm covers organic and operator-forced opens, while the subtype keeps them distinguishable.
- Predicates written before placement is fixed are unfounded — the same predicate is correct in one order and dead code in another.
- The builder is single-use: adding a strategy after `Build` throws `InvalidOperationException`.
- Every `Add*` call validates its options record eagerly through data-annotation rules and throws `ValidationException` at composition; `Build` validates the builder again — an invalid policy value is unconstructible, never a first-request fault.
- An empty builder builds a pass-through pipeline; `ResiliencePipeline.Empty` and `ResiliencePipeline<T>.Empty` are the canonical no-op values for absent-policy rows.
- `AddStrategy(factory, options)` is the custom-strategy admission; the factory receives `StrategyBuilderContext` carrying exactly `Telemetry` and `TimeProvider` — custom strategies get first-class telemetry and clock injection with zero extra wiring.
- Non-generic `ResilienceStrategy` instances admit into generic builders, so cross-cutting strategies (context stamping, claim recording, snapshotting) are written once and compose into every typed pipeline.
- `AddPipeline` nests an existing pipeline as an external component: execution composes, disposal does not propagate — nesting never transfers ownership.
- Strategy instances the builder itself constructs do dispose with the pipeline: the bridge component disposes `IDisposable`/`IAsyncDisposable` strategy instances on pipeline teardown.
- Outcome conversion across bridges preserves the captured exception dispatch info — original stack traces survive strategy boundaries and rethrow.
- The pipeline exposes no public dispose surface; dispose behavior is fixed at construction.
- Registry-owned pipelines throw `InvalidOperationException` naming registry ownership on any disposal attempt; the empty pipeline ignores disposal.
- Executing a disposed pipeline throws `ObjectDisposedException` — a dead pipeline fails loudly, never silently passes through.
- The composite short-circuits a pre-cancelled context token into a cancelled outcome without entering any strategy — cancellation observed before admission costs nothing.
- `ResiliencePipelineBuilderBase` carries two builder-level seams consumed by every strategy: `TimeProvider` (one injection point drives every delay, deadline, sampling window, and chaos clock) and `ContextPool` (custom context creation, default capture policy, operation-key resolution).
- An unset builder `TimeProvider` resolves to the system clock — clock injection is opt-in with zero configuration cost when unused.
- The builder copy constructor propagates only `Name`, `TimeProvider`, and `TelemetryListener`: when the registry derives a typed builder from the configured base builder, a `ContextPool` set on the base does not flow into typed registry pipelines — pool customization must be applied where the typed builder is configured.

## execution surface and context

- The execution surface is one polymorphic family: `Execute`/`ExecuteAsync` overloads discriminate on (callback, context, state), (callback, state, token), and (callback, token) shapes for both void and result-typed callbacks.
- A fully synchronous execution path exists alongside the async one — sync callers do not adapt through blocking waits.
- `ExecuteOutcomeAsync(callback, context, state)` is the no-throw form: the result arrives as `Outcome<T>` carrying either the value or the captured exception — the entrypoint for rails that forbid exception flow.
- `Outcome.FromResult`/`Outcome.FromException` are the construction surface for custom strategies and hedging actions — outcomes are built, never thrown.
- `ResiliencePipeline<T>` execution constrains `TResult : T` — one typed pipeline serves an entire result hierarchy, with subtype executions riding the same strategy state.
- State-threading is the canonical callback shape: every overload accepts a `TState` so hot paths pass tuples instead of capturing — closure-free execution is a first-class design axis of the surface, not an optimization trick.
- `ResilienceContext` carries `OperationKey`, `CancellationToken`, `ContinueOnCapturedContext`, and `Properties`; all are strategy-visible.
- `ContinueOnCapturedContext` is context-level and consumed by every strategy await — context-capture policy is one declaration per execution, not a per-strategy concern.
- `ResilienceProperties` is typed-key storage: `ResiliencePropertyKey<TValue>` (string-named), `Set`, and `GetValue(key, default)` — string-keyed at the wire, typed at every access point; `Properties` is the typed side channel between caller and strategies.
- `ResilienceContextPool.Shared.Get` overloads admit `operationKey`, `continueOnCapturedContext`, and the cancellation token at lease time; `Return` is mandatory and execution code never retains a returned context — strict lease ownership.
- `OperationKey` is the per-execution telemetry dimension: set at context lease, it travels with the context and lands as the `operation.key` tag on every event of that execution.
- `OperationKey` and the context token are internally settable only — fixed at lease, immune to mid-execution mutation; the deadline strategy's internal token swap is the single sanctioned exception, and it restores on exit.
- `PredicateBuilder<TResult>` composes handled-outcome predicates as rows — `Handle<TException>()`, `Handle<TException>(predicate)`, `HandleInner<TException>()`, `HandleResult(value | predicate)` — and converts implicitly into all four `ShouldHandle` delegate slots (retry, hedging, fallback, breaker); `Build()` compiles the rows into one predicate.

## strategy policy values

- Retry defaults: 3 attempts in addition to the original call, `DelayBackoffType.Constant`, 2s base delay, no jitter, `MaxDelay` unset.
- The default predicate of retry, hedging, fallback, and breaker handles any exception except `OperationCanceledException` — caller cancellation is never treated as a handled failure unless a predicate explicitly opts in.
- `MaxRetryAttempts = int.MaxValue` retries forever; the attempt counter stops incrementing instead of overflowing.
- Retry predicate arguments carry the attempt number — the handled class can tighten or widen by attempt (aggressive classes only on early attempts) without a custom strategy.
- Delay algebra: Constant = base; Linear = (n+1)·base; Exponential = 2ⁿ·base.
- Jitter on Constant/Linear is ±25% uniform around the computed value.
- Jitter on Exponential switches the whole curve to decorrelated jitter — `2^(n+rand)·tanh(√(4(n+rand)))` differenced against the previous draw and scaled by base/1.4 — which spreads correlated retry storms more smoothly than independent jitter.
- Curve overflow folds to `MaxDelay ?? TimeSpan.MaxValue`; `Delay = TimeSpan.Zero` retries immediately regardless of backoff type.
- `MaxDelay` caps only the computed curve; a `DelayGenerator` return bypasses the cap entirely — server-directed delays are uncapped by design, and any operator-guaranteed delay ceiling must be enforced inside the generator itself.
- A null or negative `DelayGenerator` return falls back to the computed delay — the generator is an override channel, not a replacement obligation.
- `Randomizer` is a settable thread-safe 0–1 source on retry and chaos options — the determinism seam for jitter and injection in proof rails.
- After the `OnRetry` event the failed attempt's result object is disposed automatically to prevent resource accumulation across attempts — event delegates copy what they need; storing the result is a use-after-dispose.
- Retry and hedging report per-attempt telemetry (`attempt.number`, `attempt.handled`) with the final attempt reported through a distinct channel — attempt-distribution histograms come from the pipeline, not caller instrumentation.
- Timeout defaults: 30s, valid range 10ms–24h.
- `TimeoutGenerator` decides per execution — a result ≤ `TimeSpan.Zero` or `Timeout.InfiniteTimeSpan` disables the deadline for that execution only, making the deadline a per-call policy value.
- Timeout mechanics: a pooled cancellation source is linked under the caller token; `context.CancellationToken` is swapped to the child for the callback and restored afterward.
- An `OperationCanceledException` converts to `TimeoutRejectedException` (carrying the timeout value) only when the child fired and the caller token did not — caller cancellation always passes through untyped.
- The timeout outcome is re-stamped with the caller token before returning — layers above never observe the internal child token.
- Circuit breaker defaults: `FailureRatio` 0.1, `MinimumThroughput` 100 (floor 2), `SamplingDuration` 30s (floor 0.5s), `BreakDuration` 5s (floor 0.5s).
- `BreakDurationGenerator` makes break length adaptive — escalating breaks with consecutive opens is a generator row, not a custom strategy.
- The half-open contract is one probe: a handled failure re-opens for another break duration; a success closes and resets statistics.
- `OnOpened`/`OnClosed`/`OnHalfOpened` are eventually consistent — the circuit may have moved on by callback time — but invocation order always preserves transition order.
- Current-state reads belong to `CircuitBreakerStateProvider`, which is single-attach: a second pipeline reusing one provider throws at build — state observation is per-breaker by construction.
- `CircuitBreakerManualControl` is a multi-breaker control plane: one control registered across N breakers isolates and closes them as a set; `new CircuitBreakerManualControl(isIsolated: true)` starts the set dark at construction.
- `IsolateAsync` is sticky until `CloseAsync`; closing resets statistics rather than restoring prior state; rejection while isolated is `IsolatedCircuitException` — operator-forced and organically-open circuits stay distinguishable by exception type.
- Hedging defaults: `Delay` 2s, `MaxHedgedAttempts` 1, range 1–10.
- Hedging `Delay = TimeSpan.Zero` launches all attempts at once (pure parallel race); `Timeout.InfiniteTimeSpan` makes hedging strictly sequential — ordered failover; the two extremes and tail-latency hedging are one policy value apart.
- `ActionGenerator` returning null is the exhaustion channel: no further hedges spawn and the best outcome so far decides; the default generator replays the original callback.
- When the primary execution is synchronous, the default action generator offloads hedged attempts through the thread pool — hedging never serializes on a synchronous caller, and hedged work leaves the caller's thread.
- Pending hedging losers are cancelled when a winner is accepted — hedging cleans up its own concurrency.
- Fallback is the only strategy that substitutes a failed outcome with a constructed one at pipeline level: `ShouldHandle` plus a required `FallbackAction` producing the replacement outcome.
- Rate limiter: with no `RateLimiter` delegate the strategy constructs a concurrency limiter from `DefaultRateLimiterOptions` (1000 permits, 0 queue) and owns its disposal.
- `AddConcurrencyLimiter(permitLimit, queueLimit)` is declaration sugar over the same strategy options — the bulkhead case is one inline call.
- `AddRateLimiter(RateLimiter)` admits a concrete limiter through a one-permit acquire per execution — admission cost is fixed at one lease per logical call at that strategy's position.
- A limiter supplied through the delegate or `AddRateLimiter(RateLimiter)` is never disposed by the pipeline — ownership follows construction, and externally owned limiters are shareable across pipelines.
- Rate-limit rejection throws `RateLimiterRejectedException` with `RetryAfter` lifted from lease metadata when the limiter provides it; the `OnRejected` callback observes and projects only — recovery decisions belong to outer strategies reading the typed exception.

## registry and keyed DI

- `ResiliencePipelineRegistry<TKey>` holds two maps under independent comparers: builder registrations and materialized pipelines — claim identity and instance identity are separately tunable.
- `TryAddBuilder(key, configure)` registers a deferred factory and returns false when the key already carries one — duplicate registration is detectable at the call site, never stacked.
- `TryGetPipeline`/`GetPipeline` materialize lazily from a registered builder on first access; `GetOrAddPipeline` is the inline form against the same cache.
- `GetPipeline` on an unknown key throws `KeyNotFoundException` naming the key — resolution failures identify the missing row.
- `GetOrAddPipeline` accepts the configure-with-context form directly — reload tokens and dispose callbacks are registry capabilities, not container privileges.
- Generic and non-generic pipelines are disjoint namespaces per result type: the same key yields distinct `ResiliencePipeline` and `ResiliencePipeline<T>` instances from per-`TResult` sub-registries — key identity silently includes the result type, and a "shared" key across result types is two pipelines.
- `BuilderNameFormatter`/`InstanceNameFormatter` split a composite key into the `pipeline.name` and `pipeline.instance` telemetry dimensions — keys are designed as (name, instance) records with each axis formatted independently, so one builder name fans into per-instance metrics.
- `ConfigureBuilderContext<TKey>` exposes `PipelineKey`, `AddReloadToken`, and `OnPipelineDisposed` — the per-pipeline reload and resource-release hooks.
- `AddReloadToken` accumulates: multiple tokens link into one source; non-cancellable or already-cancelled tokens are ignored silently.
- `EnableReloads(optionsMonitor, name)` bridges options-monitor change notifications to reload tokens matched by options name, and unregisters its own monitor subscription through `OnPipelineDisposed` — reload wiring cleans itself up.
- Reload mechanics: a fired token re-runs the configure delegate to build a fresh component and registers fresh tokens — one token generation per pipeline generation; in-flight executions complete on the old component.
- The discarded component is disposed in the background after pending executions drain — one-second polls under a thirty-second cap, then disposed regardless; executions longer than the cap race their own component's disposal.
- A throwing reload keeps the old component and reports `ReloadFailed` — configuration errors degrade to the last good pipeline, never to an unguarded seam; reload telemetry is `OnReload`, with background-dispose failure reported as `DisposeFailed`.
- Registry-built pipelines wrap in fixed order — dispose-callbacks, execution tracking, then the reloadable shell.
- The reloadable shell materializes only when at least one reload token was registered: static pipelines pay nothing for reload capability they did not declare.
- Registry disposal force-disposes every materialized pipeline including those still referenced by holders, and all subsequent registry access throws `ObjectDisposedException` — disposal is a fence, not a request.
- `AddResiliencePipeline(key, configure)` registers three surfaces at once: a keyed-service projection so `GetRequiredKeyedService<ResiliencePipeline>(key)` resolves through the provider; the singleton `ResiliencePipelineRegistry<TKey>`/`ResiliencePipelineProvider<TKey>` pair; and telemetry wired from the container's `ILoggerFactory`.
- A container-registered `TimeProvider` flows into the registry's builder automatically — DI pipelines arrive observable and test-clockable with zero extra declarations.
- `AddResiliencePipelines<TKey>` defers key enumeration to startup: the configure callback adds many keyed pipelines computed from configuration or services — dynamic hop sets are one enumeration, not N registration calls.
- Container configure delegates receive `AddResiliencePipelineContext<TKey>` exposing `ServiceProvider`, `GetOptions<TOptions>(name)`, `EnableReloads<TOptions>(name)`, and `OnPipelineDisposed` — options-driven pipelines read named options and subscribe to their changes through one context.
- `AddResiliencePipelineRegistry(configure)` customizes comparers, formatters, and the builder factory at registration — registry shape is itself a policy value.
- The registry's `BuilderFactory` is options-bound to pull the container-configured `ResiliencePipelineBuilder`, so telemetry and time flow into every registry pipeline from one composition-root declaration.
- One registry exists per key type per container (marker-guarded registration); the key type partitions the resilience namespace — a process standardizes on one key shape or its pipeline claims fragment across invisible registries.

## pipeline telemetry

- `ConfigureTelemetry(loggerFactory | options)` installs the listener on the builder; the telemetry stage wraps the whole composite and observes total execution.
- `ConfigureTelemetry` attaches through the shared builder-base constraint — generic and non-generic builders take telemetry identically.
- `TelemetryOptions.LoggerFactory` defaults to the null logger — listeners and metering still run without logging, so metric-only telemetry is a configuration choice, not a custom listener.
- The severity ladder is closed and ordered — `None`, `Debug`, `Information`, `Warning`, `Error`, `Critical` — and `None` means the event is not recorded at all.
- Without a listener the telemetry flag is false and the composite skips all event construction — unconfigured telemetry is zero-cost; container-registered pipelines arrive with it already on.
- Instruments: counter `resilience.polly.strategy.events`; histograms `resilience.polly.strategy.attempt.duration` and `resilience.polly.pipeline.duration`, both in milliseconds; log category `Polly`.
- Canonical tag set: `event.name`, `event.severity`, `pipeline.name`, `pipeline.instance`, `strategy.name`, `operation.key`, `exception.type`, plus `attempt.number`/`attempt.handled` on attempt events — dashboards and alerts key on these names across every pipeline in a process.
- `TelemetryOptions.SeverityProvider` remaps event severity at the listener from (source, event, context) — demote expected retry churn, promote breaker opens; severity `None` suppresses recording entirely; severity is the one knob gating both log level and metric emission per event class.
- `ResultFormatter` projects outcome results into telemetry payloads — HTTP responses project to status-code integers by default; it is an observability projection and cannot mutate the outcome.
- `MeteringEnrichers` append tags to every metric point through pooled tag lists.
- `TelemetryListeners` receive raw event structs before logging and metering — the bridge from pipeline events into an external fact stream is a listener row, not a wrapper around execution.
- `TelemetryOptions` has a copy constructor — per-pipeline telemetry overrides layer over one shared base options instance without mutating it.
- Event names with default severities: `PipelineExecuting` (Debug), `PipelineExecuted` (Information), `OnRetry` (Warning), `OnTimeout` (Error), `OnRateLimiterRejected` (Error), `OnReload` (Information), `ReloadFailed` and `DisposeFailed` (Error).
- Every rejection exception carries `TelemetrySource` (pipeline name, instance name, strategy name) stamped at throw — the rejecting layer is recoverable from the exception value alone, without log correlation.
- Strategy options carry a `Name` property defaulting per type (`Retry`, `Timeout`, `CircuitBreaker`, `Hedging`, `Fallback`, `RateLimiter`, `Chaos.*`); two strategies of one type in one pipeline need distinct names or their telemetry merges — (pipeline.name, strategy.name) is the deduplication key.

## chaos strategies

- Four chaos strategies admit through the same builder grammar as resilience strategies: `AddChaosFault(rate, () => exception)`, `AddChaosLatency(rate, latency)`, `AddChaosOutcome(rate, () => result)`, and `AddChaosBehavior(rate, token => effect)` — chaos is ordered policy, not a bolted-on harness.
- Outcome injection lives on the generic builder only — substituting a result requires the result type.
- Shared chaos options: `InjectionRate` defaults to 0.001 with range 0–1; `Enabled` defaults to true.
- The injection gate composes two per-execution decisions as a conjunction: the enabled decision, then the injection-rate draw against the randomizer.
- `InjectionRateGenerator` and `EnabledGenerator` make both decisions per execution from the live context — generator presence makes the scalar ignored, so targeting (one tenant, one environment, one fraction) is a generator row, not a build-time fork.
- `FaultGenerator`/`OutcomeGenerator<T>` are weighted catalogues — `AddException<TException>(weight)` and result rows with default weight 100 — convertible implicitly to the options delegate: a realistic fault mix is declared as weights, not branching.
- Latency chaos defaults to 30s injected delay; injection reports its event first, delays through the builder's `TimeProvider` (deterministic chaos under a fake clock), and still observes cancellation before invoking the real callback — injected latency is cancellable and surfaces as a cancelled outcome, never a hang.
- Outcome chaos returns the generated outcome without invoking the callback at all; a null generator return skips injection and falls through to the real call — the per-execution opt-out channel.
- Fault chaos throws on the exception rail; behavior chaos runs its side effect before the callback — the four strategies partition the failure planes (exception rail, result rail, time plane, side-effect plane) so one concern is injected per declaration.
- Chaos events are `Chaos.OnLatency`, `Chaos.OnFault`, `Chaos.OnOutcome`, `Chaos.OnBehavior`, all Information severity.
- Placement law: chaos sits below the strategies under test — fault injection above a breaker proves nothing because the breaker never observes it; the canonical arrangement is resilience chain, then chaos block, then the real callback.

## divergent

- strategy-order-semantics — the canonical chain is a derivation, not a convention: admission counts logical calls ⟹ limiter outside retry; the budget bounds the whole loop ⟹ total deadline outside retry; health statistics count attempts ⟹ breaker inside retry; each attempt earns a fresh deadline ⟹ attempt deadline innermost. Any reorder violates exactly one constraint, and which constraint breaks names the failure mode: limiter-inside-retry converts retry storms into permit starvation; breaker-outside-retry multiplies its effective throughput floor by the attempt count; total-deadline-inside-retry re-arms the budget per attempt and unbounds the loop; attempt-deadline-outside-retry lets one slow attempt consume the entire budget.
- strategy-order-semantics — reload and disposal interact with order atomically: reload swaps the entire composite per execution (there is no per-strategy reload), so order changes ride configuration reloads safely and in-flight executions finish under the old order; composite disposal walks components in declaration order, so outermost strategies release first — a strategy whose teardown publishes state observes inner components still alive.
- strategy-order-semantics — fallback placement is a polarity switch: above retry it substitutes after all attempts fail (terminal default); below retry it substitutes per attempt and the retry predicate then sees the substituted outcome, which silently disables retry unless the predicate handles the fallback value — the one arrangement where adding a strategy weakens the pipeline.
- registry-keyed-di — the dynamic-hop design point: hop set enumerated from configuration at startup through deferred bulk registration, one builder row per hop, `EnableReloads` per row against named options, per-row dispose callbacks releasing hop resources — adding a hop is one configuration entry and zero new code surfaces; per-key reload isolation means one hop's config churn rebuilds one pipeline while every other hop's materialized pipeline is untouched.
- registry-keyed-di — per-key isolation has a sharp edge at the result type: per-`TResult` sub-registries mean a hop registered generically and resolved non-generically misses silently (`TryGetPipeline` false) or throws (`GetPipeline`) — hop callables fix the result type at the registration row so resolution shape cannot drift from registration shape.
- registry-keyed-di — the copy-constructor gap is the registry's one configuration leak: typed builders derived from the configured base builder inherit name, clock, and telemetry but not the context pool — a process standardizing context-pool behavior applies it at typed-builder configuration or accepts the shared pool inside registry pipelines.
- chaos-telemetry — chaos events ride the same instruments and tag vocabulary as resilience events, so injected-versus-organic fault ratios are derivable from `event.name` partitions over the same counter with zero extra instrumentation; an injection-rate budget is enforceable as a metric assertion (chaos event count over pipeline execution count tracks the declared rate), and drift between declared and observed injection rate is itself a detector for chaos strategies placed above short-circuiting strategies.
- chaos-telemetry — the governance shape is one policy row: a single `EnabledGenerator` reading a process-level kill cell gates every chaos strategy in the process; per-strategy `InjectionRateGenerator` rows read the same configuration object so the entire chaos posture (on/off, rates, targeting) reloads as one options record through registry reload — chaos becomes a runtime-operable dial rather than a deployment artifact.
