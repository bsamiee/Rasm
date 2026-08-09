# [RUNTIME_FLOW]

Durable execution as suspend-and-replay: a `Workflow` is a Schema-typed, idempotency-keyed computation whose recorded activities replay without re-running side effects, and this page owns the whole altitude — the `Step` mint that gives every activity its budget geometry from the one `core/value/fault#RETRY_BUDGET` table, the definition law with deterministic execution identity and the engine Tag the root satisfies from `entity#GRID`, the saga compensation fold, the `Signal` external-signal owner (token-addressed `DurableDeferred` plus the branch's ONE durable timer over `DurableClock`), and the `WorkflowProxy` projection that mounts a workflow set on the serving plane. The altitude ruling arrives settled from the core machine page: an in-process transition system is the `Machine` actor; a computation demanding replay, activity memoization, compensation, or cross-process sharding lives here — promotion re-homes the vocabulary, never re-shapes it. A definition never names its engine: `WorkflowEngine.layerMemory` runs specs, `ClusterWorkflowEngine.layer` (composed at `entity#GRID`) runs production, and the swap is one root Layer. The module ships on the `./server` exports subpath as `runtime/src/work/flow.ts`.

## [01]-[INDEX]

- [02]-[STEP_MINT]: the budgeted activity mint — deadline geometry, class-gated retry, run evidence; `Step`.
- [03]-[FLOW_LAW]: definition, execution identity, the engine Tag, failure posture, drive members, the result fold; `Flow`.
- [04]-[SAGA_FOLD]: top-level compensation registration and the rollback law; `Flow`.
- [05]-[SIGNAL_GATE]: the token-addressed external signal and the one durable timer; `Signal`.
- [06]-[WIRE_PROXY]: workflow sets as serving-plane contribution pairings; `Flow`.

## [02]-[STEP_MINT]

[STEP_MINT]:
- Law: an activity executes exactly once per key — the engine persists its `Exit` and replay short-circuits; a body that re-runs for its side effect, an ad-hoc run counter, or a sleep loop beside the schedule is the rejected form. `Activity.CurrentAttempt` is the attempt evidence a body reads, `WorkflowEngine.WorkflowInstance` the per-run evidence beside it — executionId, scope, suspended and interrupted flags — so a body stamps its own run identity onto spans without a parallel context; `Activity.idempotencyKey(name, { includeAttempt: true })` splits retries into distinct durable keys where each attempt's evidence matters.
- Law: the hedged-execution row is `Activity.raceAll` at the package surface — speculative arms run as durable steps and the first completion wins durably, so a tail-latency hedge is a declared arm set, never a hand-raced fiber pair or a local rename of the combinator.
- Law: in-body pacing composes on the body, bounds compose on the declaration — `Activity.retry(effect, { while })` carries attempt gates the shipped surface types without a schedule; the schedule lives on `interruptRetryPolicy` and the budget row, one geometry per step.
- Boundary: durable throttles are `queue#THROTTLE` rows composed inside a body; this mint owns geometry, never quota.
- Packages: `@effect/workflow` (`Activity`); `effect` (`Effect`, `Duration`, `Function`); `@rasm/ts/core` (`Fault.Budget`, `Fault.Class`).

```typescript
import { Activity, DurableClock, DurableDeferred, Workflow, WorkflowProxy, WorkflowProxyServer } from "@effect/workflow"
import type { HttpApi } from "@effect/platform"
import { Cause, Context, Data, Duration, Effect, Exit, Function, Match, Schema } from "effect"
import { Fault } from "@rasm/ts/core"
import { WorkClass } from "./entity.ts"

// One row per refusal the durable altitude can raise: the class derives, so no mint can name a class its reason contradicts.
const _family = Fault.Class.family(["attempt", "total", "hold"] as const, {
  attempt: { class: "expired" },
  total: { class: "exhausted" },
  hold: { class: "expired" },
})

class StepFault extends Schema.TaggedError<StepFault>()("StepFault", {
  reason: _family.schema,
  step: Schema.String,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<step:${this.reason}> ${this.step}`
  }
}

const _run = <A, AI, AR, E extends { readonly class: Fault.Class.Kind }, EI, ER, R>(
  name: string,
  clazz: WorkClass.Kind,
  spec: {
    readonly success: Schema.Schema<A, AI, AR>
    readonly error: Schema.Schema<E, EI, ER>
    readonly execute: Effect.Effect<A, E, R>
  },
) => {
  const row = Fault.Budget.at(WorkClass[clazz].budget)
  return Activity.make({
    name,
    success: spec.success,
    error: Schema.Union(spec.error, StepFault),
    execute: spec.execute.pipe(
      Effect.timeoutFail({
        duration: row.attempt,
        onTimeout: () => new StepFault({ reason: "attempt", step: name }),
      }),
    ),
    // interrupt-only `Cause` values carry no failure the class rail reads, so the default gate refuses every replay
    interruptRetryPolicy: Fault.Budget.schedule(WorkClass[clazz].budget, Function.constTrue),
  }).pipe(Effect.timeoutFail({
    duration: row.total,
    onTimeout: () => new StepFault({ reason: "total", step: name }),
  }))
}

const Step = { run: _run }
```

## [03]-[FLOW_LAW]

[FLOW_LAW]:
- Owner: the definition law at the package surface — `Workflow.make({ name, payload, idempotencyKey, success, error, suspendedRetrySchedule })` with the payload a Schema family and `idempotencyKey` a pure projection of it, so `executionId(payload)` is content-derived and an equal payload resumes the same run instead of forking a duplicate; `Workflow.fromTaggedRequest` lifts a `Schema.TaggedRequest` whose payload/success/failure already travel as one declaration — the same triple an `Actor` protocol or a `Tool.fromTaggedRequest` row speaks, so one request class serves entity message, tool row, and workflow without re-declaration; `Flow` carries only the members the package lacks — the `verdict` fold and the saga pair (`compensated`, `saga`) — never renames of what it ships.
- Law: the body binds through `workflow.toLayer((payload, executionId) => body)` and drives through the value's own members — `execute(payload, { discard: true })` for fire-and-forget returning the executionId, `poll` answering the `Result` ADT (`Complete | Suspended`) folded with `Match`, `interrupt`/`resume` for operator control; a suspended run is a durable fact awaiting a `Signal`, never an error.
- Law: suspension and result-lifting are first-class verbs — `Workflow.suspend` parks the run explicitly where a body reaches a wait-for-world point, `Workflow.intoResult` lifts an effect into the durable `Result` so a sub-computation settles as data, `wrapActivityResult` is the activity-level twin, and `Workflow.provideScope` scopes a resource to the run's own lifetime — each a package member the body composes, never a local control convention.
- Law: failure posture is declared data on the definition — `Flow.policy({ suspendOnFailure, captureDefects })` folds the two `Context.Reference` annotations into one `Context` value that rides `make`'s `annotations` slot, so a definition states whether a failed run parks for operator resume or settles failed, and whether a defect is captured as a typed exit or escapes as a defect. `Verdict.Suspended`, `interrupt`, and `resume` presuppose the suspend annotation, so a definition that never states it inherits the package default and the whole verdict arm is unreachable by construction; `Workflow.annotate`/`annotateContext` on the value are the same fold applied after the fact, never a second posture vocabulary.
- Receipt: `Flow.Verdict` — the settled projection of a polled `Result`: `Settled` with the success value, `Suspended` with the execution id, `Interrupted` with it, `Failed` with the dominant fault class, and `Unknown` for the `undefined` a `poll` answers when no run under that execution id exists — the package's own partiality folded into the closed family at the seam, so dashboards and the serving plane's status endpoints never meet a bare `undefined`.
- Law: a run's lifetime spans process death and ends at exactly one of four owners — the body completing it, the saga fold compensating it, an operator `interrupt`ing it, or a `Signal` that never settles holding it suspended until one of the three arrives; a suspended run is durable existence, so no consumer reads a deadline this altitude never sets.
- Law: `degrade` is stated — replay proves each activity runs once per KEY, not that the world saw one effect, so an activity whose side effect is externally observable and non-idempotent needs its own dedup at the boundary it reaches.
- Growth: a new workflow is one definition value plus one `toLayer` body; an operator capability (bulk interrupt, drain-before-deploy) is a fold over `poll`/`interrupt` at the composition root.
- Packages: `@effect/workflow` (`Workflow`, `WorkflowEngine`); `effect` (`Context`, `Effect`, `Match`, `Schema`).

```typescript
type FlowVerdict<A> = Data.TaggedEnum<{
  Settled: { readonly value: A }
  Suspended: { readonly executionId: string }
  Interrupted: { readonly executionId: string }
  Failed: { readonly class: Fault.Class.Kind }
  Unknown: { readonly executionId: string }
}>

interface FlowVerdictDefinition extends Data.TaggedEnum.WithGenerics<1> {
  readonly taggedEnum: FlowVerdict<this["A"]>
}
const _Verdict = Data.taggedEnum<FlowVerdictDefinition>()

const _verdict = <A, E>(executionId: string, result: Workflow.Result<A, E> | undefined): FlowVerdict<A> =>
  result === undefined
    ? _Verdict.Unknown({ executionId }) // poll's own partiality: no run under this id — folded here so no consumer meets undefined
    : Match.value(result).pipe(
      Match.tag("Complete", (complete) =>
        Exit.match(complete.exit, {
          onSuccess: (value) => _Verdict.Settled({ value }),
          // interrupt-first: `interrupt` on this page's own drive is an operator act, not a fault; the surviving arm
          // hands the WHOLE cause to `Fault.Class.of`, which harvests every node through the severity lattice — a
          // squash upstream picks one node arbitrarily and spends the dominance fold before it runs.
          onFailure: (cause) =>
            Cause.isInterruptedOnly(cause)
              ? _Verdict.Interrupted({ executionId })
              : _Verdict.Failed({ class: Fault.Class.of(cause) }),
        })),
      Match.tag("Suspended", () => _Verdict.Suspended({ executionId })),
      Match.exhaustive,
    )
```

## [04]-[SAGA_FOLD]

[SAGA_FOLD]:
- Owner: the compensation law — `Workflow.withCompensation(effect, (value, cause) => cleanup)` registers a rollback finalizer on a top-level workflow effect; finalizers run LIFO when the whole workflow fails, each receiving the value its step produced and the cause that killed the run.
- Law: compensation is top-level only — a finalizer registered inside an activity body is outside the replay log and silently vanishes on resume; the compile-visible discipline is that every `withCompensation` wraps a `Step.run` result at the workflow body, and a rollback that belongs to a sub-step is evidence the sub-step is a workflow of its own. Forward and inverse effects retain independent requirement tails, so storage, custody, and transport capabilities compose without a false shared-environment constraint.
- Law: a compensation body is itself durable — it composes `Step.run` so a crashed rollback resumes; a raw effect as compensation re-runs on replay and is the rejected form.
- Law: rollback semantics are domain verbs, not deletions — a mail send compensates by recording a suppression, an object put by releasing its reference, a quota grant by returning the lease; the record of truth is append-only, so compensation appends the inverse fact and never erases.
- Growth: a new rollback concern is one `withCompensation` wrap at the owning workflow; a shared rollback pattern is a function over `Step.run` values, never a second saga surface.
- Packages: `@effect/workflow` (`Workflow`); `effect` (`Cause`).

```typescript
const _compensated = <A, E, R, R2>(
  step: Effect.Effect<A, E, R>,
  rollback: (value: A, cause: Cause.Cause<unknown>) => Effect.Effect<void, never, R2>,
) => Workflow.withCompensation(step, rollback)

const _saga = <A, E, R, R2>(
  forward: Effect.Effect<A, E, R>,
  inverse: (value: A) => Effect.Effect<void, never, R2>,
) => _compensated(forward, (value, cause) => Cause.isInterruptedOnly(cause) ? Effect.void : inverse(value))

const _policy = (posture: { readonly suspendOnFailure: boolean; readonly captureDefects: boolean }): Context.Context<never> =>
  Context.empty().pipe(
    Context.add(Workflow.SuspendOnFailure, posture.suspendOnFailure),
    Context.add(Workflow.CaptureDefects, posture.captureDefects),
  )

const Flow = { compensated: _compensated, policy: _policy, saga: _saga, verdict: _verdict }
```

## [05]-[SIGNAL_GATE]

[SIGNAL_GATE]:
- Owner: `Signal` — the external-signal owner and the branch's ONE durable-timer surface. `DurableDeferred.make(name, { success, error })` mints the deferred at the package surface; a workflow suspends on `Signal.hold` (`DurableDeferred.await` under a durable expiry race); the resolving side derives the token — `DurableDeferred.tokenFromPayload({ workflow, payload })` or `tokenFromExecutionId({ workflow, executionId })` — and settles it out-of-band with `succeed`/`fail`/`done`; `Signal` carries only the members the package lacks — the raced hold and the named pause — never renames of what it ships. A verified inbound callback, a human approval, an agent's held-tool release each resolve the same shape.
- Law: in-band settlement is `DurableDeferred.into(effect, deferred)` — a sibling activity's own result binds to the deferred as it settles, so a gate resolved by a computation inside the run needs no out-of-band token round-trip; the out-of-band `succeed`/`fail` path stays the external-caller arm.
- Law: the timer is owned once — `Signal.pause({ name, duration })` wraps `DurableClock.sleep({ name, duration })`; the package's sixty-second threshold keeps short sleeps in memory and persists longer ones, so a caller cannot fork timer policy through a signature knob. `schedule#CADENCE_ROWS` composes this member for named pauses and no second `DurableClock` call site exists in the branch.
- Law: expiry is a race arm, not a poll — `Signal.hold` races the deferred against `Signal.pause` of the hold budget through `DurableDeferred.raceAll`, whose persisted winner makes the race replay-deterministic (a bare fiber race settles nondeterministically on resume), folding the elapsed arm into an `expired`-classed fault; a loop that polls for an out-of-band condition is unspellable beside it.
- Law: the token is a capability — an out-of-band resolver holds only the `Token`, never the workflow value; the serving plane carries tokens through its signed callback vocabulary, and token derivation stays on this side of the seam so the wire never learns payload shape.
- Boundary: mounting the callback endpoint, verifying its signature, and admitting the caller are serving-plane and security-plane concerns; this owner declares, holds, and settles.
- Growth: a new signal is one `DurableDeferred.make` value; a new hold posture (renewable hold, escalating expiry) is a policy field on `hold`.
- Packages: `@effect/workflow` (`DurableDeferred`, `DurableClock`); `effect` (`Effect`, `Duration`).

```typescript
declare namespace Signal {
  type Hold<Success extends Schema.Schema.Any, Error extends Schema.Schema.All> = {
    readonly deferred: DurableDeferred.DurableDeferred<Success, Error>
    readonly expiry: Duration.DurationInput
  }
}

const _pause = (options: { readonly name: string; readonly duration: Duration.DurationInput }) =>
  DurableClock.sleep({ name: options.name, duration: options.duration })

const _hold = <Success extends Schema.Schema.Any, Error extends Schema.Schema.All>(
  hold: Signal.Hold<Success, Error>,
) =>
  DurableDeferred.raceAll({
    name: hold.deferred.name,
    success: hold.deferred.successSchema,
    error: Schema.Union(hold.deferred.errorSchema, StepFault),
    effects: [
      DurableDeferred.await(hold.deferred),
      _pause({ name: `${hold.deferred.name}/expiry`, duration: hold.expiry }).pipe(
        Effect.zipRight(Effect.fail(new StepFault({ reason: "hold", step: hold.deferred.name }))),
      ),
    ],
  })

const Signal = { hold: _hold, pause: _pause }
```

## [06]-[WIRE_PROXY]

[WIRE_PROXY]:
- Owner: the wire projection — `FlowSurface.contribution(name, flows)` derives the execute/poll/interrupt procedure families from a workflow set as `serve/api#CONTRIBUTION` pairing material: `WorkflowProxy.toRpcGroup(flows)` beside `WorkflowProxyServer.layerRpcHandlers(flows)` is exactly the `Contribution.rpc(group, handlers)` pair, and `WorkflowProxy.toHttpApiGroup(name, flows)` beside the api-reading builder `(api) => WorkflowProxyServer.layerHttpApi(api, name, flows)` is exactly the `Contribution.http(group, handlers)` pair — the typed client, OpenAPI rows, and derived SDK arrive from the same declaration.
- Law: the proxy is the ONLY wire existence a workflow has — a hand-written endpoint that executes a workflow re-states the contract and is the rejected form, and a bare group projection whose handler binding the app must rediscover is the half-pairing defect; operator drive (bulk interrupt, replay) composes the same proxied procedures.
- Growth: exposing a new workflow set is one proxy call in the owning contribution; a private workflow simply never enters one.
- Packages: `@effect/workflow` (`WorkflowProxy`, `WorkflowProxyServer`); `@effect/platform` (`HttpApi` — the pairing builder's api parameter).

```typescript
const _contribution = <const Flows extends readonly [Workflow.Any, ...ReadonlyArray<Workflow.Any>]>(name: string, flows: Flows) => ({
  rpc: WorkflowProxy.toRpcGroup(flows),
  rpcHandlers: WorkflowProxyServer.layerRpcHandlers(flows),
  http: WorkflowProxy.toHttpApiGroup(name, flows),
  httpHandlers: <Api extends HttpApi.HttpApi.Any>(api: Api) => WorkflowProxyServer.layerHttpApi(api, name, flows),
})

const FlowSurface = { contribution: _contribution }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Flow, FlowSurface, Signal, Step, StepFault }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
