# [RUNTIME_FLOW]

Durable execution as suspend-and-replay: a `Workflow` is a Schema-typed, idempotency-keyed computation whose recorded activities replay without re-running side effects, and this page owns the whole altitude — the `Step` mint that gives every activity its budget geometry from the one `core/fault#RETRY_BUDGET` table, the `Flow` definition law with deterministic execution identity and the engine Tag the root satisfies from `entity#GRID`, the saga compensation fold, the `Gate` external-signal owner (token-addressed `DurableDeferred` plus the branch's ONE durable timer over `DurableClock`), and the `WorkflowProxy` projection that mounts a workflow set on the serving plane. The altitude ruling arrives settled from the core machine page: an in-process transition system is the `Machine` actor; a computation demanding replay, activity memoization, compensation, or cross-process sharding lives here — promotion re-homes the vocabulary, never re-shapes it. A definition never names its engine: `WorkflowEngine.layerMemory` runs specs, `Grid.engine` runs production, and the swap is one root Layer. The module ships on the `./server` exports subpath as `runtime/src/work/flow.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                            | [PUBLIC] |
| :-----: | :----------- | :----------------------------------------------------------------------------------- | :------- |
|  [01]   | `STEP_MINT`  | the budgeted activity mint — deadline geometry, class-gated retry, the hedged race    | `Step`   |
|  [02]   | `FLOW_LAW`   | definition, execution identity, the engine Tag, drive members, the result fold        | `Flow`   |
|  [03]   | `SAGA_FOLD`  | top-level compensation registration and the rollback law                              | `Flow`   |
|  [04]   | `SIGNAL_GATE`| the token-addressed external signal and the one durable timer                         | `Gate`   |
|  [05]   | `WIRE_PROXY` | workflow sets as serving-plane contribution pairings                                  | `Flow`   |

## [2]-[STEP_MINT]

[STEP_MINT]:
- Owner: `Step` — the once-executed durable step with its budget geometry compiled from a `WorkClass`-priced `Budget` row: `Step.run(name, clazz, execute)` wraps the body in the row's per-attempt deadline (`Effect.timeoutFail` with `Budget[kind].attempt` failing into an `expired`-classed fault), hands `Budget.schedule(kind)` to `interruptRetryPolicy` so an interrupted step re-drives only while its fault classifies retryable, and stacks the whole-call deadline (`Budget[kind].total`) above the activity so the two-tier geometry every rail in the branch layers is identical here.
- Law: an activity executes exactly once per key — the engine persists its `Exit` and replay short-circuits; a body that re-runs for its side effect, an ad-hoc run counter, or a sleep loop beside the schedule is the rejected form. `Activity.CurrentAttempt` is the attempt evidence a body reads; `Activity.idempotencyKey(name, { includeAttempt: true })` splits retries into distinct durable keys where each attempt's evidence matters.
- Law: `Step.race(name, arms)` is the hedged-execution row — `Activity.raceAll` runs speculative arms as durable steps and the first completion wins durably, so a tail-latency hedge is a declared arm set, never a hand-raced fiber pair.
- Law: in-body pacing composes on the body, bounds compose on the declaration — `Activity.retry(effect, { while })` carries attempt gates the shipped surface types without a schedule; the schedule lives on `interruptRetryPolicy` and the budget row, one geometry per step.
- Growth: a new deadline envelope is a `Budget` row on the core table; a new step concern (a spend meter, a progress mark) is a transformer composed at this mint, inherited by every step.
- Boundary: durable throttles are `queue#THROTTLE` rows composed inside a body; this mint owns geometry, never quota.
- Packages: `@effect/workflow` (`Activity`); `effect` (`Effect`, `Duration`); `@rasm/ts/core` (`Budget`, `FaultClass`).

```typescript
import { Activity, DurableClock, DurableDeferred, Workflow, WorkflowEngine, WorkflowProxy } from "@effect/workflow"
import { Cause, Data, Duration, Effect, Match } from "effect"
import { Budget, FaultClass } from "@rasm/ts/core"
import { WorkClass } from "./entity.ts"

class StepFault extends Data.TaggedError("StepFault")<{
  readonly class: FaultClass.Kind
  readonly step: string
  readonly detail: string
}> {}

const _run = <A, E extends { readonly class: FaultClass.Kind }, R>(
  name: string,
  clazz: WorkClass.Kind,
  execute: Effect.Effect<A, E, R>,
) => {
  const row = Budget[WorkClass[clazz].budget]
  return Activity.make({
    name,
    execute: execute.pipe(
      Effect.timeoutFail({
        duration: row.attempt,
        onTimeout: () => new StepFault({ class: "expired", step: name, detail: "attempt budget" }),
      }),
    ),
    interruptRetryPolicy: Budget.schedule(WorkClass[clazz].budget),
  }).pipe(Effect.timeoutFail({
    duration: row.total,
    onTimeout: () => new StepFault({ class: "exhausted", step: name, detail: "total budget" }),
  }))
}

const Step = {
  run: _run,
  race: Activity.raceAll,
  attempt: Activity.CurrentAttempt,
  key: Activity.idempotencyKey,
}
```

## [3]-[FLOW_LAW]

[FLOW_LAW]:
- Owner: `Flow.make` — the definition law: `Workflow.make({ name, payload, idempotencyKey, success, error, suspendedRetrySchedule })` with the payload a Schema family and `idempotencyKey` a pure projection of it, so `executionId(payload)` is content-derived and an equal payload resumes the same run instead of forking a duplicate; `Workflow.fromTaggedRequest` lifts a `Schema.TaggedRequest` whose payload/success/failure already travel as one declaration — the same triple an `Actor` protocol or a `Tool.fromTaggedRequest` row speaks, so one request class serves entity message, tool row, and workflow without re-declaration.
- Law: the body binds through `workflow.toLayer((payload, executionId) => body)` and drives through the value's own members — `execute(payload, { discard: true })` for fire-and-forget returning the executionId, `poll` answering the `Result` ADT (`Complete | Suspended`) folded with `Match`, `interrupt`/`resume` for operator control; a suspended run is a durable fact awaiting a `Gate` signal, never an error.
- Law: the engine is the `WorkflowEngine` Tag — `layerMemory` in specs, `entity#GRID`'s `Grid.engine` in production — and a definition naming its engine is the named defect; `suspendedRetrySchedule` takes a `Budget`-compiled schedule so a suspended run's re-check cadence rides the same geometry vocabulary.
- Receipt: `Flow.Verdict` — the settled projection of a polled `Result`: `settled` with the success value, `suspended` with the execution id, `failed` with the dominant fault class — the shape drive dashboards and the serving plane's status endpoints read.
- Growth: a new workflow is one definition value plus one `toLayer` body; an operator capability (bulk interrupt, drain-before-deploy) is a fold over `poll`/`interrupt` at the composition root.
- Packages: `@effect/workflow` (`Workflow`, `WorkflowEngine`); `effect` (`Schema`, `Match`, `Effect`).

```typescript
type FlowVerdict<A> = Data.TaggedEnum<{
  Settled: { readonly value: A }
  Suspended: { readonly executionId: string }
  Failed: { readonly class: FaultClass.Kind }
}>

interface FlowVerdictDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: FlowVerdict<this["A"]>
}
const _Verdict = Data.taggedEnum<FlowVerdictDefinition>()

const _verdict = <A, E>(executionId: string, result: Workflow.Result<A, E>): FlowVerdict<A> =>
  Match.value(result).pipe(
    Match.tag("Complete", (complete) =>
      complete.exit._tag === "Success"
        ? _Verdict.Settled({ value: complete.exit.value })
        : _Verdict.Failed({ class: FaultClass.of(Cause.squash(complete.exit.cause)) })),
    Match.tag("Suspended", () => _Verdict.Suspended({ executionId })),
    Match.exhaustive,
  )

const Flow = {
  make: Workflow.make,
  fromTaggedRequest: Workflow.fromTaggedRequest,
  verdict: _verdict,
  engineSpec: WorkflowEngine.layerMemory,
}
```

## [4]-[SAGA_FOLD]

[SAGA_FOLD]:
- Owner: the compensation law — `Workflow.withCompensation(effect, (value, cause) => cleanup)` registers a rollback finalizer on a top-level workflow effect; finalizers run LIFO when the whole workflow fails, each receiving the value its step produced and the cause that killed the run.
- Law: compensation is top-level only — a finalizer registered inside an activity body is outside the replay log and silently vanishes on resume; the compile-visible discipline is that every `withCompensation` wraps a `Step.run` result at the workflow body, and a rollback that belongs to a sub-step is evidence the sub-step is a workflow of its own.
- Law: a compensation body is itself durable — it composes `Step.run` so a crashed rollback resumes; a raw effect as compensation re-runs on replay and is the rejected form.
- Law: rollback semantics are domain verbs, not deletions — a mail send compensates by recording a suppression, an object put by releasing its reference, a quota grant by returning the lease; the record of truth is append-only, so compensation appends the inverse fact and never erases.
- Growth: a new rollback concern is one `withCompensation` wrap at the owning workflow; a shared rollback pattern is a function over `Step.run` values, never a second saga surface.
- Packages: `@effect/workflow` (`Workflow`); `effect` (`Cause`).

```typescript
const _compensated = <A, E, R, R2>(
  step: Effect.Effect<A, E, R>,
  rollback: (value: A, cause: Cause.Cause<unknown>) => Effect.Effect<void, never, R2>,
) => Workflow.withCompensation(step, rollback)

const _sagaShape = <A, E, R>(
  forward: Effect.Effect<A, E, R>,
  inverse: (value: A) => Effect.Effect<void, never, R>,
) => _compensated(forward, (value, cause) => Cause.isInterruptedOnly(cause) ? Effect.void : inverse(value))
```

## [5]-[SIGNAL_GATE]

[SIGNAL_GATE]:
- Owner: `Gate` — the external-signal owner and the branch's ONE durable-timer surface. `Gate.declare(name, { success, error })` mints the `DurableDeferred`; a workflow suspends on `Gate.hold` (`DurableDeferred.await` under a durable expiry race); the resolving side derives the token — `DurableDeferred.tokenFromPayload({ workflow, payload })` or `tokenFromExecutionId({ workflow, executionId })` — and settles it out-of-band with `succeed`/`fail`/`done`. A verified inbound callback, a human approval, an agent's held-tool release each resolve the same shape.
- Law: the timer is owned once — `Gate.pause({ name, duration })` wraps `DurableClock.sleep({ name, duration, inMemoryThreshold })`, sub-threshold sleeps run in memory and longer ones survive process death at zero resource; `schedule#CADENCE` composes this member for its named pauses and no second `DurableClock` call site exists in the branch.
- Law: expiry is a race arm, not a poll — `Gate.hold` races the deferred against `Gate.pause` of the hold budget, folding the elapsed arm into an `expired`-classed fault; a loop that polls for an out-of-band condition is unspellable beside it.
- Law: the token is a capability — an out-of-band resolver holds only the `Token`, never the workflow value; the serving plane carries tokens through its signed callback vocabulary, and token derivation stays on this side of the seam so the wire never learns payload shape.
- Boundary: mounting the callback endpoint, verifying its signature, and admitting the caller are serving-plane and security-plane concerns; this owner declares, holds, and settles.
- Growth: a new signal is one `declare` value; a new hold posture (renewable hold, escalating expiry) is a policy field on `hold`.
- Packages: `@effect/workflow` (`DurableDeferred`, `DurableClock`); `effect` (`Effect`, `Duration`).

```typescript
declare namespace Gate {
  type Hold<A, E> = {
    readonly deferred: DurableDeferred.DurableDeferred<A, E>
    readonly expiry: Duration.DurationInput
  }
}

const _pause = (options: { readonly name: string; readonly duration: Duration.DurationInput }) =>
  DurableClock.sleep({ name: options.name, duration: options.duration, inMemoryThreshold: Duration.seconds(60) })

const _hold = <A, E>(hold: Gate.Hold<A, E>) =>
  Effect.raceFirst(
    DurableDeferred.await(hold.deferred),
    _pause({ name: "expiry", duration: hold.expiry }).pipe(
      Effect.zipRight(Effect.fail(new StepFault({ class: "expired", step: "gate", detail: "hold expiry" }))),
    ),
  )

const Gate = {
  declare: DurableDeferred.make,
  hold: _hold,
  pause: _pause,
  token: { fromPayload: DurableDeferred.tokenFromPayload, fromExecutionId: DurableDeferred.tokenFromExecutionId },
  settle: { succeed: DurableDeferred.succeed, fail: DurableDeferred.fail, done: DurableDeferred.done },
}
```

## [6]-[WIRE_PROXY]

[WIRE_PROXY]:
- Owner: the wire projection — `WorkflowProxy.toRpcGroup(workflows, { prefix })` and `WorkflowProxy.toHttpApiGroup(name, workflows)` derive execute/poll/interrupt procedure families from a workflow set, so the serving plane mounts durable executions as an ordinary `serve/api#CONTRIBUTION` pairing and the typed client, OpenAPI rows, and derived SDK arrive from the same declaration.
- Law: the proxy is the ONLY wire existence a workflow has — a hand-written endpoint that executes a workflow re-states the contract and is the rejected form; operator drive (bulk interrupt, replay) composes the same proxied procedures.
- Growth: exposing a new workflow set is one proxy call in the owning contribution; a private workflow simply never enters one.
- Packages: `@effect/workflow` (`WorkflowProxy`, `WorkflowProxyServer`).

```typescript
const _contribution = <const Flows extends ReadonlyArray<Workflow.Any>>(name: string, flows: Flows) => ({
  rpc: WorkflowProxy.toRpcGroup(flows),
  http: WorkflowProxy.toHttpApiGroup(name, flows),
})

const FlowSurface = { contribution: _contribution }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Flow, FlowSurface, Gate, Step, StepFault }
```
