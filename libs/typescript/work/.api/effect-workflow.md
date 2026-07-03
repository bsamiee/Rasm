# [@effect/workflow] — durable workflow + activity definitions with compensation/saga folds over a swappable engine

`@effect/workflow` is the durable-execution vocabulary `work/flow` and `work/queue` compose: a `Workflow` is a `Schema`-typed, idempotency-keyed, suspend-and-replay computation; an `Activity` is a once-executed, durably-recorded effect inside it; and the surrounding primitives (`DurableDeferred` external-signal await, `DurableClock` durable timer, `DurableQueue` persisted job, `DurableRateLimiter` durable throttle) are the durable analogs of their in-memory `effect` peers. Every definition runs against the `WorkflowEngine` Tag — satisfied by `WorkflowEngine.layerMemory` for specs OR by `@effect/cluster` `ClusterWorkflowEngine.layer` (`work/.api/effect-cluster.md`) for durable sharded execution over `MessageStorage` — so the SAME workflow rides both by Layer selection at the app root. Durability is suspend/replay: an `Activity` executes once, its exit is persisted, and on resume the workflow replays recorded activities without re-running side effects, with `withCompensation` running saga finalizers on whole-workflow failure. Retry/timeout budgets ride `effect/Schedule` from `kernel/fault`, never a hand-rolled loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/workflow`
- package: `@effect/workflow`
- version: `0.18.2`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x`, `@effect/rpc ^0.75.x`, `@effect/experimental ^0.60.x` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`, `.api/effect-experimental.md`)
- engine-peer: `@effect/cluster` (`work/.api/effect-cluster.md`) provides the durable `WorkflowEngine` via `ClusterWorkflowEngine.layer`; not a hard dependency — `layerMemory` runs without it
- runtime: node/bun durable lanes — the memory engine is universal (specs), the durable engine rides cluster `MessageStorage` on `@effect/sql` (a node/bun store driver)
- catalog-verdict: KEEP — the one durable-execution engine; a hand-rolled saga, retry-persistence loop, or state-machine runner is the named reinvention defect
- modules: `Workflow`, `Activity`, `DurableDeferred`, `DurableClock`, `DurableQueue`, `DurableRateLimiter`, `WorkflowEngine`, `WorkflowProxy`, `WorkflowProxyServer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `Workflow` + `Activity` definitions — Schema-typed durable computations
- rail: durable-execution
- A `Workflow` carries payload/success/error `Schema`s and a deterministic `idempotencyKey`; an `Activity` IS an `Effect` whose exit is durably recorded. Both are closed, versioned Schema families — the same decode-once law as `store/journal`.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :---------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Workflow.Workflow<Name, Payload, Success, Error>`         | workflow def    | `work/flow/durable` — one durable definition; `execute`/`poll`/`interrupt`/`resume`/`executionId` on the value |
|  [02]   | `Activity.Activity<Success, Error, R>`                     | activity def    | `work/flow/activity` — extends `Effect`; the once-executed durable step |
|  [03]   | `Workflow.Result<A, E>` = `Complete<A, E>` \| `Suspended`  | result ADT      | `poll` return; `Match` the two arms — a suspended run awaits an external signal |
|  [04]   | `Activity.CurrentAttempt`                                  | context ref     | the attempt counter (`number`) inside an activity; feeds `idempotencyKey({ includeAttempt })` |
|  [05]   | `Workflow.AnyStructSchema` / `Workflow.Any`               | erased def      | registry/proxy bounds over heterogeneous workflow sets         |

[PUBLIC_TYPE_SCOPE]: the durable primitives — deferred, clock, queue, rate limiter
- rail: durable-execution
- Each is the durable analog of an in-memory `effect` primitive: `DurableDeferred` is a token-addressable `Deferred` that survives restarts, `DurableClock` a durable `Effect.sleep`, `DurableQueue` a `@effect/experimental` `PersistedQueue` with a completion `DurableDeferred`, `DurableRateLimiter` a durable `@effect/experimental` `RateLimiter`.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :---------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `DurableDeferred.DurableDeferred<Success, Error>`          | external signal | `work/flow` — await a human approval / webhook callback across restarts |
|  [02]   | `DurableDeferred.Token`                                    | branded string  | the durable correlation handle; `tokenFromPayload`/`tokenFromExecutionId` derive it for out-of-band resolution |
|  [03]   | `DurableClock.DurableClock`                                | durable timer   | `work/queue/schedule` — sleep across process restarts; short sleeps run in memory |
|  [04]   | `DurableQueue.DurableQueue<Payload, Success, Error>`      | persisted job   | `work/queue/job` — a `PersistedQueue` whose items complete via a `DurableDeferred` |
|  [05]   | `WorkflowEngine`                                           | engine Tag      | the execution service; `layerMemory` (spec) or `ClusterWorkflowEngine.layer` (durable) satisfies it |
|  [06]   | `WorkflowEngine.WorkflowInstance`                          | execution context Tag | per-run state (executionId, workflow, scope, suspended/interrupted flags, cause); the durable-instance handle |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: define + execute a workflow with compensation
- rail: durable-execution
- `Workflow.make` declares the durable computation with a deterministic `idempotencyKey`; `.toLayer(execute)` registers its body; `.execute(payload)` runs it (or returns the executionId with `{ discard: true }`). `withCompensation` is the saga fold — top-level effects register finalizers called on whole-workflow failure (NOT nested activities).

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Workflow.make({ name, payload, idempotencyKey, success?, error?, suspendedRetrySchedule?, annotations? })` | declare | `work/flow/durable` — the durable definition; `idempotencyKey` derives the replay key |
|  [02]   | `Workflow.fromTaggedRequest(schema, { suspendedRetrySchedule? })`                              | declare        | build a workflow from a `Schema.TaggedRequest` (payload+success+failure in one) |
|  [03]   | `workflow.toLayer((payload, executionId) => Effect<Success, Error, R>)`                        | register       | bind the workflow body; the `Layer` the app root provides |
|  [04]   | `workflow.execute(payload, { discard? })` / `.poll(executionId)` / `.interrupt` / `.resume`    | drive          | run/observe/cancel/resume; `discard: true` returns the executionId string, fire-and-forget |
|  [05]   | `workflow.executionId(payload)`                                                                | determinism    | the content-derived id; the same payload resumes the same run |
|  [06]   | `Workflow.withCompensation(effect, (value, cause) => cleanup)`                                 | saga           | register a compensation finalizer on a top-level effect; runs on whole-workflow failure |
|  [07]   | `Workflow.intoResult` / `wrapActivityResult` / `suspend` / `provideScope`                      | result / control | lift an effect to the `Result` ADT; request suspension; scope an activity |

[ENTRYPOINT_SCOPE]: activities — the once-executed durable steps with retry budgets
- rail: durable-execution
- `Activity.make` records a step's exit durably; retry/timeout budgets come from `effect/Schedule` (`kernel/fault`), never a loop. `idempotencyKey` and `raceAll` are the dedup and speculative-execution seams.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Activity.make({ name, execute, success?, error?, interruptRetryPolicy? })`           | declare        | `work/flow/activity` — the durable step; `interruptRetryPolicy` a `Schedule` |
|  [02]   | `Activity.retry(effect, options)`                                                     | retry          | Effect.Retry options (`times`/`while`/`until`/`schedule`); the per-activity budget from `kernel/fault` |
|  [03]   | `Activity.idempotencyKey(name, { includeAttempt? })`                                  | dedup          | the durable step key; `includeAttempt` splits retries into distinct keys |
|  [04]   | `Activity.raceAll(name, activities)`                                                  | race           | first durable step to complete wins; the speculative-execution fold |

[ENTRYPOINT_SCOPE]: durable primitives — deferred, clock, queue, rate limiter
- rail: durable-execution
- The external-signal, timer, queue, and throttle surfaces. `DurableDeferred` is resolved out-of-band by `Token`; `DurableQueue` wraps `@effect/experimental` `PersistedQueue`; `DurableRateLimiter` wraps `@effect/experimental` `RateLimiter` with the algorithm as a policy value.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `DurableDeferred.make(name, { success?, error? })` — `await` / `into(effect)`         | external signal | suspend the workflow until resolved; `into` binds an effect's result to the deferred |
|  [02]   | `DurableDeferred.token` / `tokenFromPayload` / `tokenFromExecutionId`                  | correlate      | derive the `Token` an out-of-band caller resolves against |
|  [03]   | `DurableDeferred.succeed` / `fail` / `failCause` / `done` / `raceAll`                  | resolve        | out-of-band completion by `Token` (webhook, human approval, sibling service) |
|  [04]   | `DurableClock.make({ name, duration })` / `DurableClock.sleep({ name, duration, threshold? })` | durable timer | sleep across restarts; `threshold` runs sub-window sleeps in memory |
|  [05]   | `DurableQueue.make({ name, payload, idempotencyKey, success?, error? })`               | declare queue  | `work/queue/job` — the persisted job family; `idempotencyKey` the dedup key |
|  [06]   | `DurableQueue.process(queue, payload, { retrySchedule? })`                             | offer          | enqueue + suspend until a worker finishes; `retrySchedule` over `PersistedQueueError` is the DLQ/replay budget |
|  [07]   | `DurableQueue.worker(queue, f, { concurrency? })` / `makeWorker`                       | consume        | the worker `Layer`/effect; bounded concurrency processing |
|  [08]   | `DurableRateLimiter.rateLimit({ name, algorithm?, window, limit, key, tokens? })`      | throttle       | a durable-throttle `Activity`; `algorithm` = `fixed-window` \| `token-bucket` policy value |

[ENTRYPOINT_SCOPE]: engine layers + wire exposure
- rail: durable-execution/boundaries
- The engine Layer selection and the proxy that exposes workflows over the wire.

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `WorkflowEngine.layerMemory`                                                           | engine (spec)  | in-memory execution for kit-driven specs; no durability      |
|  [02]   | `WorkflowEngine.makeUnsafe(encoded)` / the `WorkflowEngine.Encoded` interface          | engine (raw)   | a custom engine backing behind the erased interface       |
|  [03]   | `WorkflowProxy.toRpcGroup(workflows, { prefix? })`                                     | wire expose    | a workflow set → an `@effect/rpc` `RpcGroup` (`edge` contribution) |
|  [04]   | `WorkflowProxy.toHttpApiGroup(name, workflows)` / `WorkflowProxyServer`               | wire expose    | a workflow set → an `@effect/platform` `HttpApiGroup` + the server binding; typed client for free |

## [04]-[IMPLEMENTATION_LAW]

[WORKFLOW_TOPOLOGY]:
- one definition, swappable engine: every `Workflow`/`Activity`/`DurableDeferred`/`DurableClock`/`DurableQueue` runs against the `WorkflowEngine` Tag. `WorkflowEngine.layerMemory` is the spec engine; `ClusterWorkflowEngine.layer` (over `Sharding` + `MessageStorage`) is the durable sharded engine. The definition never names its engine — the app root selects, so promoting a workflow from spec to durable is a Layer swap, not a rewrite.
- durability is suspend/replay, not re-run: an `Activity` executes exactly once and its exit is persisted through the engine's `MessageStorage` (`ClusterSchema.Persisted` on the durable path). On resume the workflow REPLAYS recorded activities from the durable log — side effects never repeat. `idempotencyKey` (workflow) and `Activity.idempotencyKey` (step) are the replay keys; `executionId(payload)` makes re-execution of an equal payload resume the same run.
- compensation is the saga rail, top-level only: `withCompensation(effect, (value, cause) => cleanup)` registers a finalizer that runs when the WHOLE workflow fails — the saga rollback. The documented constraint is load-bearing: compensation registers for top-level workflow effects, not for effects nested inside an activity; a rollback belongs at the workflow body, not inside a step.
- retry/timeout budgets are `Schedule`, not loops: `interruptRetryPolicy`, `Activity.retry`, `suspendedRetrySchedule`, and `DurableQueue` `retrySchedule` all take an `effect/Schedule` sourced from `kernel/fault` degradation budgets (`Schedule.exponential`/`.jittered`). A hand-rolled retry counter or sleep loop is the defect.
- external signals are token-addressed: a workflow `await`s a `DurableDeferred`; an out-of-band caller resolves it by `Token` (`tokenFromPayload`/`tokenFromExecutionId` → `succeed`/`fail`/`done`). The suspended workflow resumes when the token is set — the human-approval / webhook-callback pattern.

[STACKS_WITH]:
- `@effect/cluster` (`work/.api/effect-cluster.md`): `ClusterWorkflowEngine.layer` satisfies `WorkflowEngine` over `Sharding` + `MessageStorage` — durable workflows run sharded on the cluster runtime. THE core seam: `work/flow` defines, `work/engine` provides the engine.
- `@effect/experimental` (`.api/effect-experimental.md`): `DurableQueue` wraps `PersistedQueue`/`PersistedQueueFactory`; `DurableRateLimiter` wraps `RateLimiter` (the SAME limiter `edge/api/middleware` uses, here durable-wrapped as an `Activity` with `algorithm` as a policy value). The persisted backing is the store-owned `KeyValueStore`/SQL driver.
- `effect` (`.api/effect.md`): payload/success/error are `Schema`; the `Result` ADT folds through `Match`; retry budgets are `Schedule`; a `Workflow` composes ordinary `Effect.gen` bodies with `Activity` steps and `DurableClock.sleep` — the durable layer adds no new rail, it is `effect` made replay-durable.
- `@effect/rpc` + `@effect/platform` (`.api/effect-platform.md`, `edge/.api/`): `WorkflowProxy.toRpcGroup`/`toHttpApiGroup` turns a workflow set into an `edge` contribution group (execute/poll/interrupt endpoints) with the typed client and OpenAPI for free — a workflow is invokable over the wire, and `wire/invoke` or `deliver/webhook` resolves a `DurableDeferred` `Token` from an inbound signed callback.
- `deliver/report` + `deliver/mail` (`work/.api/exceljs.md`, `work/.api/nodemailer.md`): a `deliver` job is one `Activity` in a workflow — idempotent, retryable, resumable; the compensation finalizer un-sends or marks-suppressed on rollback.

[LOCAL_ADMISSION]:
- Define workflows/activities against the `WorkflowEngine` Tag; never hardcode `layerMemory` or the cluster engine inside a definition — the app root selects.
- Derive the replay key from `idempotencyKey`/`executionId`; never mint an ad-hoc run id or re-run an activity for its side effect.
- Register rollback through `withCompensation` at the workflow top level; never nest compensation inside an activity, and never hand-roll a saga.
- Take retry/timeout budgets from `kernel/fault` `Schedule`s (`interruptRetryPolicy`/`retrySchedule`/`suspendedRetrySchedule`); never a manual retry loop or `setTimeout`.
- Resolve external waits through a `DurableDeferred` `Token`; never poll for an out-of-band condition.
- Wrap `DurableQueue` over `@effect/experimental` `PersistedQueue` and `DurableRateLimiter` over its `RateLimiter`; never re-implement a persisted queue or distributed limiter.

[RAIL_LAW]:
- Package: `@effect/workflow`
- Owns: the `Workflow`/`Activity` durable definitions, the `Result` suspend/complete ADT, `withCompensation` saga folds, `DurableDeferred` token-addressed external signals, `DurableClock` durable timers, `DurableQueue` persisted jobs, `DurableRateLimiter` durable throttles, the `WorkflowEngine`/`WorkflowInstance` Tags, and `WorkflowProxy` wire exposure
- Accept: definitions bound to the `WorkflowEngine` Tag (memory for specs, `ClusterWorkflowEngine` for durable), Schema-typed payload/success/error, deterministic `idempotencyKey`/`executionId`, `withCompensation` top-level saga finalizers, `Schedule`-sourced retry budgets, `DurableDeferred` `Token` external resolution, `DurableQueue`/`DurableRateLimiter` over the `@effect/experimental` persisted primitives, `WorkflowProxy` for edge exposure
- Reject: a hand-rolled saga/state-machine/retry-persistence loop, a definition hardcoding its engine, a re-run of an activity for its side effect, compensation nested inside an activity, a manual poll for an external signal, a second persisted-queue or distributed-rate-limiter implementation, a hand-rolled durable timer
