# [TS_RUNTIME_API_EFFECT_WORKFLOW]

`@effect/workflow` owns durable execution: a `Workflow` is a `Schema`-typed, idempotency-keyed computation that suspends and replays; an `Activity` a once-executed step whose exit persists; and the durable primitives are the replay-durable analogs of their in-memory `effect` peers. Every definition binds the `WorkflowEngine` Tag, so one workflow rides the memory engine for specs or the sharded cluster engine for durable execution by Layer choice at the app root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/workflow`
- package: `@effect/workflow` (MIT)
- asset: ESM `.d.ts` (`dist/dts/*.d.ts`); peer `effect` + `@effect/platform` + `@effect/rpc` + `@effect/experimental`
- owner: `work`
- runtime: isomorphic; the durable engine rides node/bun
- rail: durable-execution
- modules: `Workflow`, `Activity`, `DurableDeferred`, `DurableClock`, `DurableQueue`, `DurableRateLimiter`, `WorkflowEngine`, `WorkflowProxy`, `WorkflowProxyServer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Workflow` + `Activity` definitions — Schema-typed durable computations
- A `Workflow` and its `Activity` steps carry closed payload/success/error `Schema`s under the decode-once law of `store/journal`; an `Activity` IS an `Effect`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Workflow.Workflow<Name, Payload, Success, Error>` | workflow def  | `work/flow`: `execute`/`poll`/`interrupt`/`resume`/`executionId` |
|  [02]   | `Activity.Activity<Success, Error, R>`             | activity def  | `work/flow` — extends `Effect`; once-executed step               |
|  [03]   | `Workflow.Result<A, E>`                            | result ADT    | `poll` return; `Match` the `Complete<A, E>`/`Suspended` arms     |
|  [04]   | `Activity.CurrentAttempt`                          | context ref   | `number` attempt counter; `idempotencyKey({ includeAttempt })`   |
|  [05]   | `Workflow.AnyStructSchema` / `Workflow.Any`        | erased def    | registry/proxy bounds over heterogeneous workflow sets           |

[PUBLIC_TYPE_SCOPE]: durable primitives — deferred, clock, queue, rate limiter

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]   | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `DurableDeferred.DurableDeferred<Success, Error>`    | external signal | `work/flow` — await approval/webhook across restarts           |
|  [02]   | `DurableDeferred.Token`                              | branded string  | `tokenFromPayload`/`tokenFromExecutionId` derive the handle    |
|  [03]   | `DurableClock.DurableClock`                          | durable timer   | `work/schedule` — sleep across restarts; sub-window in memory  |
|  [04]   | `DurableQueue.DurableQueue<Payload, Success, Error>` | persisted job   | `work/queue` — `PersistedQueue` via a `DurableDeferred`        |
|  [05]   | `WorkflowEngine`                                     | engine Tag      | `layerMemory` (spec) / `ClusterWorkflowEngine.layer` (durable) |
|  [06]   | `WorkflowEngine.WorkflowInstance`                    | instance Tag    | per-run executionId, scope, suspend/interrupt flags, cause     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: define, drive, and compensate a workflow

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                        |
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

[ENTRYPOINT_SCOPE]: activities — once-executed durable steps

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `Activity.make({ name, execute })`                   | declare        | `work/flow` durable step; `interruptRetryPolicy` a `Schedule`   |
|  [02]   | `Activity.retry(effect, options)`                    | retry          | pacing from `core/value/fault`; gates composed on the body      |
|  [03]   | `Activity.idempotencyKey(name, { includeAttempt? })` | dedup          | step key; `includeAttempt` splits retries into distinct keys    |
|  [04]   | `Activity.raceAll(name, activities)`                 | race           | first durable step to complete wins; speculative-execution fold |

- `Activity.retry`: `Effect.Retry` options minus `schedule` (`times`/`while`/`until`).

[ENTRYPOINT_SCOPE]: durable primitives — deferred, clock, queue, rate limiter

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `DurableDeferred.make(name, { success?, error? })`               | external signal | `await`/`into(effect)` binds the result      |
|  [02]   | `DurableDeferred.token`                                          | correlate       | the deferred's own correlation `Token`       |
|  [03]   | `tokenFromPayload` / `tokenFromExecutionId`                      | correlate       | derive the resolvable `Token`                |
|  [04]   | `DurableDeferred.succeed` / `fail` / `failCause`                 | resolve         | out-of-band completion by `Token`            |
|  [05]   | `DurableDeferred.done` / `raceAll`                               | resolve         | complete or first-wins by `Token`            |
|  [06]   | `DurableClock.make({ name, duration })`                          | durable timer   | schedule a durable sleep                     |
|  [07]   | `DurableClock.sleep({ name, duration, inMemoryThreshold? })`     | durable timer   | sleep across restarts; sub-window in memory  |
|  [08]   | `DurableQueue.make({ name, payload, idempotencyKey })`           | declare queue   | `work/queue` job; `idempotencyKey` dedups    |
|  [09]   | `DurableQueue.process(queue, payload, { retrySchedule? })`       | offer           | enqueue/suspend; `retrySchedule` budget      |
|  [10]   | `DurableQueue.worker(queue, f, { concurrency? })` / `makeWorker` | consume         | worker `Layer`/effect; bounded concurrency   |
|  [11]   | `DurableRateLimiter.rateLimit(options)`                          | throttle        | durable `Activity`; `algorithm` policy value |

- `DurableRateLimiter.rateLimit`: `{ name, window, limit, key, algorithm?, tokens? }`; `algorithm` is `fixed-window`/`token-bucket`.
- `DurableClock.sleep`: sub-window sleeps run in memory below `inMemoryThreshold` (60s default).

[ENTRYPOINT_SCOPE]: engine layers + wire exposure

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `WorkflowEngine.layerMemory`                                            | engine (spec)  | in-memory execution for specs; no durability |
|  [02]   | `WorkflowEngine.makeUnsafe(encoded)` / `WorkflowEngine.Encoded`         | engine (raw)   | custom engine behind the erased interface    |
|  [03]   | `WorkflowProxy.toRpcGroup(workflows, { prefix? })`                      | wire expose    | workflow set → `RpcGroup` (`edge`)           |
|  [04]   | `WorkflowProxy.toHttpApiGroup(name, workflows)` / `WorkflowProxyServer` | wire expose    | `HttpApiGroup`; execute/discard/resume       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One definition binds one Tag, the app root selects the engine: `layerMemory` runs specs, `ClusterWorkflowEngine.layer` (over `Sharding` + `MessageStorage`) runs durable sharded execution, so spec-to-durable promotion is a Layer swap.
- Durability is suspend/replay: an `Activity` executes once and its exit persists through `MessageStorage` (`ClusterSchema.Persisted` on the durable path), and resume replays recorded activities so side effects never repeat. `idempotencyKey`/`Activity.idempotencyKey` are the replay keys; `executionId(payload)` resumes the same run for an equal payload.
- Compensation is the saga rail: `withCompensation(effect, (value, cause) => cleanup)` registers a finalizer that runs on whole-workflow failure. It binds top-level workflow effects only — a rollback lives at the workflow body, never nested inside an activity.
- Retry and timeout budgets are `Schedule`: `interruptRetryPolicy`, `Activity.retry`, `suspendedRetrySchedule`, and `DurableQueue` `retrySchedule` each take an `effect/Schedule` from `core/value/fault` degradation budgets (`Schedule.exponential`/`.jittered`).
- Failure policy is annotation data: `SuspendOnFailure` suspends the whole run on any error for a later `resume(executionId)`, and `CaptureDefects` (default on) folds a defect into the `Result` rather than crashing the fiber; both ride `workflow.annotate` or `make`'s `annotations?`.
- External signals are token-addressed: a workflow `await`s a `DurableDeferred`, an out-of-band caller resolves it by `Token` (`tokenFromPayload`/`tokenFromExecutionId` → `succeed`/`fail`/`done`), and the suspended run resumes when the token is set — the human-approval / webhook-callback pattern.

[STACKING]:
- `@effect/cluster` (`runtime/.api/effect-cluster.md`): `ClusterWorkflowEngine.layer` satisfies `WorkflowEngine` over `Sharding` + `MessageStorage` — the core seam, `work/flow` defines the workflow and `work/entity` binds the engine.
- `@effect/experimental` (`.api/effect-experimental.md`): `DurableQueue` wraps `PersistedQueue`/`PersistedQueueFactory` and `DurableRateLimiter` wraps `RateLimiter` (the `serve/api` middleware limiter, durable-wrapped as an `Activity` with `algorithm` a policy value); persisted backing is the `data`-owned `KeyValueStore`/SQL driver.
- `effect` (`.api/effect.md`): payload/success/error are `Schema`, the `Result` ADT folds through `Match`, retry budgets are `Schedule`, and a `Workflow` composes ordinary `Effect.gen` bodies with `Activity` steps and `DurableClock.sleep` — the durable layer adds no rail, it is `effect` made replay-durable.
- `@effect/rpc` + `@effect/platform` (`runtime/.api/effect-rpc.md`, `.api/effect-platform.md`): `WorkflowProxy.toRpcGroup`/`toHttpApiGroup` turns a workflow set into a `serve` contribution group — one `execute`, one `discard`-to-executionId, one `resume` per workflow — with the typed client and OpenAPI for free, `WorkflowProxyServer.layerRpcHandlers`/`layerHttpApi` binding the handlers.
- `work/report` + `work/deliver` (`runtime/.api/exceljs.md`, `runtime/.api/nodemailer.md`): a `deliver` job is one `Activity` — idempotent, retryable, resumable — and its compensation finalizer un-sends or marks-suppressed on rollback.

[LOCAL_ADMISSION]:
- Define workflows and activities against the `WorkflowEngine` Tag; the app root selects `layerMemory` for specs or `ClusterWorkflowEngine.layer` for durable execution.
- Derive the replay key from `idempotencyKey`/`executionId`, and register rollback through top-level `withCompensation`.
- Source retry and timeout budgets from `core/value/fault` `Schedule`s, and resolve external waits by a `DurableDeferred` `Token`.
- Compose `DurableQueue`/`DurableRateLimiter` over the `@effect/experimental` `PersistedQueue`/`RateLimiter`.

[RAIL_LAW]:
- Package: `@effect/workflow`
- Owns: the `Workflow`/`Activity` durable definitions, the `Result` suspend/complete ADT, `withCompensation` saga folds, the `SuspendOnFailure`/`CaptureDefects` failure annotations, `DurableDeferred` token-addressed signals, `DurableClock` timers, `DurableQueue` persisted jobs, `DurableRateLimiter` throttles, the `WorkflowEngine`/`WorkflowInstance` Tags, and `WorkflowProxy` wire exposure
- Accept: definitions bound to the `WorkflowEngine` Tag (memory for specs, `ClusterWorkflowEngine` for durable), Schema-typed payload/success/error, deterministic `idempotencyKey`/`executionId`, top-level `withCompensation` finalizers, `Schedule`-sourced retry budgets, `DurableDeferred` `Token` resolution, `DurableQueue`/`DurableRateLimiter` over the `@effect/experimental` primitives, `WorkflowProxy` edge exposure
- Reject: a hand-rolled saga, state-machine, or retry-persistence loop; a definition hardcoding its engine; a re-run of an activity for its side effect; compensation nested inside an activity; a manual poll for an external signal; a second persisted-queue or distributed-rate-limiter implementation; a hand-rolled durable timer
