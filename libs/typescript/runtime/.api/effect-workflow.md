# [TS_RUNTIME_API_EFFECT_WORKFLOW]

`@effect/workflow` is the durable-execution vocabulary `work/flow` and `work/queue` compose: a `Workflow` is a `Schema`-typed, idempotency-keyed, suspend-and-replay computation; an `Activity` is a once-executed, durably-recorded effect inside it; and the surrounding primitives (`DurableDeferred` external-signal await, `DurableClock` durable timer, `DurableQueue` persisted job, `DurableRateLimiter` durable throttle) are the durable analogs of their in-memory `effect` peers. Every definition runs against the `WorkflowEngine` Tag — satisfied by `WorkflowEngine.layerMemory` for specs OR by `@effect/cluster` `ClusterWorkflowEngine.layer` (`runtime/.api/effect-cluster.md`) for durable sharded execution over `MessageStorage` — so the SAME workflow rides both by Layer selection at the app root. Durability is suspend/replay: an `Activity` executes once, its exit is persisted, and on resume the workflow replays recorded activities without re-running side effects, with `withCompensation` running saga finalizers on whole-workflow failure. Retry/timeout budgets ride `effect/Schedule` from `core/value/fault`, never a hand-rolled loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/workflow`
- package: `@effect/workflow`
- license: `MIT`
- effect-peer: `effect catalog`, `@effect/platform catalog`, `@effect/rpc catalog`, `@effect/experimental catalog` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`, `.api/effect-experimental.md`)
- engine-peer: `@effect/cluster` (`runtime/.api/effect-cluster.md`) provides the durable `WorkflowEngine` via `ClusterWorkflowEngine.layer`; not a hard dependency — `layerMemory` runs without it
- runtime: node/bun durable lanes — the memory engine is universal (specs), the durable engine rides cluster `MessageStorage` on `@effect/sql` (a node/bun store driver)
- catalog-verdict: KEEP — the one durable-execution engine; a hand-rolled saga, retry-persistence loop, or state-machine runner is the named reinvention defect
- modules: `Workflow`, `Activity`, `DurableDeferred`, `DurableClock`, `DurableQueue`, `DurableRateLimiter`, `WorkflowEngine`, `WorkflowProxy`, `WorkflowProxyServer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Workflow` + `Activity` definitions — Schema-typed durable computations
- rail: durable-execution
- A `Workflow` carries payload/success/error `Schema`s and a deterministic `idempotencyKey`; an `Activity` IS an `Effect` whose exit is durably recorded. Both are closed, versioned Schema families — the same decode-once law as `store/journal`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                              |
| :-----: | :------------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Workflow.Workflow<Name, Payload, Success, Error>` | workflow def  | `work/flow`: `execute`/`poll`/`interrupt`/`resume`/`executionId` |
|  [02]   | `Activity.Activity<Success, Error, R>`             | activity def  | `work/flow` — extends `Effect`; once-executed step               |
|  [03]   | `Workflow.Result<A, E>`                            | result ADT    | `poll` return; `Match` the `Complete<A, E>`/`Suspended` arms     |
|  [04]   | `Activity.CurrentAttempt`                          | context ref   | `number` attempt counter; `idempotencyKey({ includeAttempt })`   |
|  [05]   | `Workflow.AnyStructSchema` / `Workflow.Any`        | erased def    | registry/proxy bounds over heterogeneous workflow sets           |

[PUBLIC_TYPE_SCOPE]: the durable primitives — deferred, clock, queue, rate limiter
- rail: durable-execution
- Each is the durable analog of an in-memory `effect` primitive: `DurableDeferred` is a token-addressable `Deferred` that survives restarts, `DurableClock` a durable `Effect.sleep`, `DurableQueue` a `@effect/experimental` `PersistedQueue` with a completion `DurableDeferred`, `DurableRateLimiter` a durable `@effect/experimental` `RateLimiter`.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                            |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `DurableDeferred.DurableDeferred<Success, Error>`    | external signal | `work/flow` — await approval/webhook across restarts           |
|  [02]   | `DurableDeferred.Token`                              | branded string  | `tokenFromPayload`/`tokenFromExecutionId` derive the handle    |
|  [03]   | `DurableClock.DurableClock`                          | durable timer   | `work/schedule` — sleep across restarts; sub-window in memory  |
|  [04]   | `DurableQueue.DurableQueue<Payload, Success, Error>` | persisted job   | `work/queue` — `PersistedQueue` via a `DurableDeferred`        |
|  [05]   | `WorkflowEngine`                                     | engine Tag      | `layerMemory` (spec) / `ClusterWorkflowEngine.layer` (durable) |
|  [06]   | `WorkflowEngine.WorkflowInstance`                    | instance Tag    | per-run executionId, scope, suspend/interrupt flags, cause     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: define + execute a workflow with compensation
- rail: durable-execution
- `Workflow.make` declares the durable computation with a deterministic `idempotencyKey`; `.toLayer(execute)` registers its body; `.execute(payload)` runs it (or returns the executionId with `{ discard: true }`). `withCompensation` is the saga fold — top-level effects register finalizers called on whole-workflow failure (NOT nested activities). `make` also takes optional `success?`/`error?`/`suspendedRetrySchedule?`/`annotations?`, and the `toLayer` body is `(payload, executionId) => Effect<Success, Error, R>`.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :---------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Workflow.make({ name, payload, idempotencyKey })`                | declare        | durable def; `idempotencyKey` → replay key          |
|  [02]   | `Workflow.fromTaggedRequest(schema, { suspendedRetrySchedule? })` | declare        | `Schema.TaggedRequest`: payload+success+failure     |
|  [03]   | `workflow.toLayer(execute)`                                       | register       | bind the workflow body as an app-root `Layer`       |
|  [04]   | `workflow.execute(payload, { discard? })`                         | drive          | run; `discard: true` returns the executionId        |
|  [05]   | `workflow.poll(executionId)` / `.interrupt` / `.resume`           | drive          | observe/cancel/resume a run                         |
|  [06]   | `workflow.executionId(payload)`                                   | determinism    | content-derived id; equal payload → same run        |
|  [07]   | `Workflow.withCompensation(effect, (value, cause) => cleanup)`    | saga           | top-level finalizer; runs on whole-workflow failure |
|  [08]   | `Workflow.intoResult` / `wrapActivityResult`                      | result         | lift an effect to the `Result` ADT                  |
|  [09]   | `suspend` / `provideScope`                                        | control        | request suspension; scope an activity               |
|  [10]   | `SuspendOnFailure` / `CaptureDefects` / `workflow.annotate`       | annotate       | suspend run on error; capture defect                |

[ENTRYPOINT_SCOPE]: activities — the once-executed durable steps with retry budgets
- rail: durable-execution
- `Activity.make` records a step's exit durably; retry/timeout budgets come from `effect/Schedule` (`core/value/fault`), never a loop. `idempotencyKey` and `raceAll` are the dedup and speculative-execution seams. `make` takes `{ name, execute, success?, error?, interruptRetryPolicy? }`, and `Activity.retry` accepts `Effect.Retry` options with `schedule` typed out (`times`/`while`/`until` only).

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                             |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `Activity.make({ name, execute })`                   | declare        | `work/flow` durable step; `interruptRetryPolicy` a `Schedule`   |
|  [02]   | `Activity.retry(effect, options)`                    | retry          | pacing from `core/value/fault`; gates composed on the body      |
|  [03]   | `Activity.idempotencyKey(name, { includeAttempt? })` | dedup          | step key; `includeAttempt` splits retries into distinct keys    |
|  [04]   | `Activity.raceAll(name, activities)`                 | race           | first durable step to complete wins; speculative-execution fold |

[ENTRYPOINT_SCOPE]: durable primitives — deferred, clock, queue, rate limiter
- rail: durable-execution
- External-signal, timer, queue, and throttle surfaces. `DurableDeferred` resolves out-of-band by `Token`; `DurableClock.sleep` runs sub-window sleeps in memory below `inMemoryThreshold` (default 60s). `DurableQueue` wraps `@effect/experimental` `PersistedQueue`, and `DurableQueue.process` retries a `PersistedQueueError` on its `retrySchedule` (the DLQ/replay budget). `DurableRateLimiter.rateLimit({ name, algorithm?, window, limit, key, tokens? })` wraps the `@effect/experimental` `RateLimiter` with `algorithm` a `fixed-window`/`token-bucket` policy value.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                         |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `DurableDeferred.make(name, { success?, error? })`                       | external signal | `await`/`into(effect)` binds the result     |
|  [02]   | `DurableDeferred.token`                                                  | correlate       | the deferred's own correlation `Token`      |
|  [03]   | `tokenFromPayload` / `tokenFromExecutionId`                              | correlate       | derive the resolvable `Token`               |
|  [04]   | `DurableDeferred.succeed` / `fail` / `failCause`                         | resolve         | out-of-band completion by `Token`           |
|  [05]   | `DurableDeferred.done` / `raceAll`                                       | resolve         | complete or first-wins by `Token`           |
|  [06]   | `DurableClock.make({ name, duration })`                                  | durable timer   | schedule a durable sleep                    |
|  [07]   | `DurableClock.sleep({ name, duration, inMemoryThreshold? })`             | durable timer   | sleep across restarts; sub-window in memory |
|  [08]   | `DurableQueue.make({ name, payload, idempotencyKey, success?, error? })` | declare queue   | `work/queue` job; `idempotencyKey` dedups   |
|  [09]   | `DurableQueue.process(queue, payload, { retrySchedule? })`               | offer           | enqueue/suspend; `retrySchedule` budget     |
|  [10]   | `DurableQueue.worker(queue, f, { concurrency? })` / `makeWorker`         | consume         | worker `Layer`/effect; bounded concurrency  |
|  [11]   | `DurableRateLimiter.rateLimit(options)`                                  | throttle        | durable `Activity`; `algorithm` value       |

[ENTRYPOINT_SCOPE]: engine layers + wire exposure
- rail: durable-execution/boundaries
- Engine Layer selection plus the proxy that exposes workflows over the wire.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                          |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `WorkflowEngine.layerMemory`                                            | engine (spec)  | in-memory execution for specs; no durability |
|  [02]   | `WorkflowEngine.makeUnsafe(encoded)` / `WorkflowEngine.Encoded`         | engine (raw)   | custom engine behind the erased interface    |
|  [03]   | `WorkflowProxy.toRpcGroup(workflows, { prefix? })`                      | wire expose    | workflow set → `RpcGroup` (`edge`)           |
|  [04]   | `WorkflowProxy.toHttpApiGroup(name, workflows)` / `WorkflowProxyServer` | wire expose    | `HttpApiGroup`; execute/discard/resume       |

## [04]-[IMPLEMENTATION_LAW]

[WORKFLOW_TOPOLOGY]:
- one definition, swappable engine: every `Workflow`/`Activity`/`DurableDeferred`/`DurableClock`/`DurableQueue` runs against the `WorkflowEngine` Tag. `WorkflowEngine.layerMemory` is the spec engine; `ClusterWorkflowEngine.layer` (over `Sharding` + `MessageStorage`) is the durable sharded engine. Definition never names its engine — the app root selects, so promoting a workflow from spec to durable is a Layer swap, not a rewrite.
- durability is suspend/replay, not re-run: an `Activity` executes exactly once and its exit is persisted through the engine's `MessageStorage` (`ClusterSchema.Persisted` on the durable path). On resume the workflow REPLAYS recorded activities from the durable log — side effects never repeat. `idempotencyKey` (workflow) and `Activity.idempotencyKey` (step) are the replay keys; `executionId(payload)` makes re-execution of an equal payload resume the same run.
- compensation is the saga rail, top-level only: `withCompensation(effect, (value, cause) => cleanup)` registers a finalizer that runs when the WHOLE workflow fails — the saga rollback. Compensation registers for top-level workflow effects, not for effects nested inside an activity; a rollback belongs at the workflow body, not inside a step.
- retry/timeout budgets are `Schedule`, not loops: `interruptRetryPolicy`, `Activity.retry`, `suspendedRetrySchedule`, and `DurableQueue` `retrySchedule` all take an `effect/Schedule` sourced from `core/value/fault` degradation budgets (`Schedule.exponential`/`.jittered`). A hand-rolled retry counter or sleep loop is the defect.
- failure policy is an annotation, not a try/catch: `SuspendOnFailure` on a workflow suspends the whole run on any error for a later `resume(executionId)` instead of failing it, and `CaptureDefects` (default on) folds a defect into the `Result` rather than crashing the fiber. Both ride `workflow.annotate`/`annotateContext` or `make`'s `annotations?` — the durable-failure switch is data on the definition.
- external signals are token-addressed: a workflow `await`s a `DurableDeferred`; an out-of-band caller resolves it by `Token` (`tokenFromPayload`/`tokenFromExecutionId` → `succeed`/`fail`/`done`). Suspended workflow resumes when the token is set — the human-approval / webhook-callback pattern.

[STACKS_WITH]:
- `@effect/cluster` (`runtime/.api/effect-cluster.md`): `ClusterWorkflowEngine.layer` satisfies `WorkflowEngine` over `Sharding` + `MessageStorage` — durable workflows run sharded on the cluster runtime. THE core seam: `work/flow` defines, `work/entity` provides the engine.
- `@effect/experimental` (`.api/effect-experimental.md`): `DurableQueue` wraps `PersistedQueue`/`PersistedQueueFactory`; `DurableRateLimiter` wraps `RateLimiter` (the SAME limiter the `serve/api` middleware uses, here durable-wrapped as an `Activity` with `algorithm` as a policy value). Persisted backing is the data-owned `KeyValueStore`/SQL driver.
- `effect` (`.api/effect.md`): payload/success/error are `Schema`; the `Result` ADT folds through `Match`; retry budgets are `Schedule`; a `Workflow` composes ordinary `Effect.gen` bodies with `Activity` steps and `DurableClock.sleep` — the durable layer adds no new rail, it is `effect` made replay-durable.
- `@effect/rpc` + `@effect/platform` (`.api/effect-platform.md`, `runtime/.api/effect-rpc.md`): `WorkflowProxy.toRpcGroup`/`toHttpApiGroup` turns a workflow set into a `serve` contribution group — one `execute`, one `discard`-to-executionId, and one `resume` endpoint per workflow — with the typed client and OpenAPI for free, `WorkflowProxyServer.layerRpcHandlers`/`layerHttpApi` binding the handlers; `interchange/invoke` or `work/deliver` resolves a `DurableDeferred` `Token` from an inbound signed callback.
- `work/report` + `work/deliver` (`runtime/.api/exceljs.md`, `runtime/.api/nodemailer.md`): a `deliver` job is one `Activity` in a workflow — idempotent, retryable, resumable; the compensation finalizer un-sends or marks-suppressed on rollback.

[LOCAL_ADMISSION]:
- Define workflows/activities against the `WorkflowEngine` Tag; never hardcode `layerMemory` or the cluster engine inside a definition — the app root selects.
- Derive the replay key from `idempotencyKey`/`executionId`; never mint an ad-hoc run id or re-run an activity for its side effect.
- Register rollback through `withCompensation` at the workflow top level; never nest compensation inside an activity, and never hand-roll a saga.
- Take retry/timeout budgets from `core/value/fault` `Schedule`s (`interruptRetryPolicy`/`retrySchedule`/`suspendedRetrySchedule`); never a manual retry loop or `setTimeout`.
- Resolve external waits through a `DurableDeferred` `Token`; never poll for an out-of-band condition.
- Wrap `DurableQueue` over `@effect/experimental` `PersistedQueue` and `DurableRateLimiter` over its `RateLimiter`; never re-implement a persisted queue or distributed limiter.

[RAIL_LAW]:
- Package: `@effect/workflow`
- Owns: the `Workflow`/`Activity` durable definitions, the `Result` suspend/complete ADT, `withCompensation` saga folds, the `SuspendOnFailure`/`CaptureDefects` failure annotations, `DurableDeferred` token-addressed external signals, `DurableClock` durable timers, `DurableQueue` persisted jobs, `DurableRateLimiter` durable throttles, the `WorkflowEngine`/`WorkflowInstance` Tags, and `WorkflowProxy` wire exposure
- Accept: definitions bound to the `WorkflowEngine` Tag (memory for specs, `ClusterWorkflowEngine` for durable), Schema-typed payload/success/error, deterministic `idempotencyKey`/`executionId`, `withCompensation` top-level saga finalizers, `Schedule`-sourced retry budgets, `DurableDeferred` `Token` external resolution, `DurableQueue`/`DurableRateLimiter` over the `@effect/experimental` persisted primitives, `WorkflowProxy` for edge exposure
- Reject: a hand-rolled saga/state-machine/retry-persistence loop, a definition hardcoding its engine, a re-run of an activity for its side effect, compensation nested inside an activity, a manual poll for an external signal, a second persisted-queue or distributed-rate-limiter implementation, a hand-rolled durable timer
