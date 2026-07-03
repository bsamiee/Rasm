# [WORK_DURABLE]

A durable workflow is a Schema-typed, idempotency-keyed, suspend-and-replay computation defined against the `WorkflowEngine` Tag alone: `Workflow.make` declares it, `.toLayer` binds its body, and whether it runs in-memory for a spec or sharded over `MessageStorage` is the composition root's Layer selection through `Engine.workflow` — a definition that names its engine has hardcoded the deployment into the domain. This page owns the definition law, the saga fold, and the external-signal gate. Definitions are authored on the package surface directly — `Workflow.make` with a deterministic `idempotencyKey`, `Workflow.fromTaggedRequest` where a `Schema.TaggedRequest` already carries the triple — with this page's `Flow` supplying the policy the surface leaves open: the suspended-retry pulse from the kernel budget rows, and `Flow.gate`, the token-addressed `DurableDeferred` composed with a durable expiry so a human approval or webhook callback resolves across restarts or lapses as typed evidence. Compensation is `Workflow.withCompensation` at the workflow's top level only; a hand saga runner, a nested-in-activity rollback, a polled external condition, and an ad-hoc run id are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                        |
| :-----: | :----------- | :------------------------------------------------------------------------------ |
|  [01]   | [DEFINITION] | the workflow declaration law, replay identity, drive/observe surface            |
|  [02]   | [SAGA]       | the compensation fold, its top-level constraint, undo evidence                  |
|  [03]   | [SIGNAL]     | the `Flow` owner: suspended-retry pulse, the gated deferred with durable expiry |

## [2]-[DEFINITION]

[DEFINITION]:
- Owner: the definition law over the package surface. `Workflow.make({ name, payload, idempotencyKey, success, error, suspendedRetrySchedule })` is the declaration — payload, success, and error are Schema owners, `idempotencyKey` is a pure projection of the payload, and `suspendedRetrySchedule` takes `Flow.pulse` so a suspended run re-polls under the kernel lease budget. `Workflow.fromTaggedRequest(schema)` derives the same definition from a `Schema.TaggedRequest` whose payload/success/failure triple already exists — one declaration, never a parallel restatement.
- Law: replay identity is content-derived — `workflow.executionId(payload)` computes from the declared `idempotencyKey`, so executing an equal payload resumes the same run instead of forking a duplicate; the key projects business identity (the order key, the tenant scope plus aggregate), never a timestamp or random id, because a nondeterministic key silently defeats every replay and dedupe guarantee downstream.
- Law: the drive surface is the definition's own — `workflow.execute(payload)` runs to the typed result, `{ discard: true }` returns the execution id for fire-and-forget, `.poll(executionId)` reads the `Workflow.Result` ADT whose `Complete | Suspended` arms fold through `Match.valueTags`, `.interrupt` cancels, `.resume` re-drives a suspended run — so observation and control never bypass the engine.
- Law: the body is `(payload, executionId) => Effect` bound once through `workflow.toLayer` — every effectful line inside composes `Step.run` bodies, engine primitives, and pure folds, because a side effect outside an activity boundary re-runs on every replay; the registered Layer is what the composition root merges beside `Engine.routing` and the storage row.
- Boundary: the engine bridge and its `Sharding | MessageStorage` requirements are `engine/entity.md`'s `Engine.workflow`; step budgets are `flow/activity.md`'s; wire exposure of a workflow set — `WorkflowProxy.toHttpApiGroup`/`toRpcGroup` deriving execute/poll/interrupt endpoints with the typed client — is the `edge` contribution law consuming this page's values.
- Entry: `Workflow.make({...})` then `workflow.toLayer(body)`; `workflow.execute(payload)` from any rail that holds the engine.
- Growth: a new workflow is one declaration plus one body Layer; a new observation need is a member the package value already carries.

## [3]-[SAGA]

[SAGA]:
- Owner: the compensation fold — `Workflow.withCompensation(effect, (value, cause) => undo)` registers a finalizer on a top-level workflow effect that runs when the whole workflow later fails: the saga rollback as a fold over already-succeeded stages, LIFO from the failure point, each undo receiving the stage's own committed value and the terminal cause as evidence.
- Law: the constraint is load-bearing — compensation registers for top-level workflow effects only, never for effects nested inside an activity body; a rollback belongs at the workflow altitude where stage boundaries are durable facts, and an undo buried in a step would replay wrong because the step's exit is already recorded.
- Law: the undo is itself durable work — each compensation arm is a `Step.run` body with its own budget row (a refund, a suppression record, an un-reservation), because a rollback that can vanish in a crash is not a rollback; undo faults are resolved inside the arm — degrade to evidence, never re-fail the failing workflow.
- Law: a saga stage is the pair at one declaration — `Workflow.withCompensation(Step.run({...}), (value, cause) => Step.run({...}))` — so reading the workflow body top-to-bottom reads the whole forward-and-rollback ladder; a compensation registry assembled elsewhere restates what the declaration order already states.
- Boundary: what a given channel's undo means — un-send, suppress, mark-void — is each deliver page's law; this page owns where the fold attaches and what evidence it receives.
- Growth: a new saga pattern is one more `withCompensation` pair in a body — the fold itself is closed.

## [4]-[SIGNAL]

[SIGNAL]:
- Owner: `Flow` — the definition policy owner. `Flow.pulse` is the suspended-retry schedule every definition references (the kernel `lease` budget compiled once — jittered, bounded, class-gated). `Flow.gate(name, shapes)` mints the external-signal surface: a named `DurableDeferred` whose `success`/`error` schemas type the out-of-band verdict, `token` derives the durable correlation handle an out-of-band caller resolves against, and `sealed(duration)` is the awaited arm composed with a durable expiry — `DurableClock.sleep` racing the deferred so an unanswered gate lapses into the typed `GateLapse` instead of suspending forever.
- Law: resolution is token-addressed, never polled — the workflow suspends on the deferred; the resolving side (`deliver/webhook.md`'s inbound callback ack, a human-approval verb on `edge`) derives the same token from the execution id and calls `DurableDeferred.succeed`/`fail`/`done` — the suspended run resumes where it stopped, across process restarts, with the verdict typed by the gate's own schemas.
- Law: expiry is durable and classified — the race's clock arm is a named `DurableClock.sleep`, so the deadline survives restarts exactly like the wait; `GateLapse` carries the gate name and window and classifies `expired`, so an upstream budget gate treats a lapsed approval as the transient-or-terminal policy the caller's fault table declares.
- Law: the loser is inert by construction — a verdict arriving after the lapse resolves a deferred no one awaits, and a resumed race re-reads the recorded clock exit; both primitives are replay-idempotent, which is what licenses the race spelling inside a durable body.
- Boundary: the deferred, clock, and token primitives are the package's; HMAC verification of the inbound callback that resolves a token is `security`/`edge` material; this page owns the gate composition and its policy values.
- Entry: `const gate = Flow.gate("<name>", { success, error })` at the definition; `yield* gate.sealed("3 days")` in the body; `DurableDeferred.succeed(gate.deferred, token, verdict)` from the resolving rail.
- Growth: a new signal pattern (a quorum gate, a two-phase ack) is a member on `Flow` composing the same primitives.
- Packages: `@effect/workflow` (`DurableClock`, `DurableDeferred`, `Workflow`, `WorkflowEngine`), `effect` (`Data`, `Duration`, `Effect`, `Schema`), `@rasm/ts/kernel` (`Budget`, `FaultClass`).

```typescript
import { DurableClock, DurableDeferred, type WorkflowEngine } from "@effect/workflow"
import { Budget, type FaultClass } from "@rasm/ts/kernel"
import { Data, type Duration, Effect, type Schema } from "effect"

class GateLapse extends Data.TaggedError("GateLapse")<{
  readonly gate: string
  readonly window: Duration.DurationInput
}> {
  get class(): FaultClass.Kind {
    return "expired"
  }
}

declare namespace Flow {
  type Shapes<A, AI, E, EI> = {
    readonly success: Schema.Schema<A, AI>
    readonly error: Schema.Schema<E, EI>
  }
  type Gate<A, E> = {
    readonly deferred: DurableDeferred.DurableDeferred<A, E>
    readonly token: (executionId: string) => DurableDeferred.Token
    readonly sealed: (
      window: Duration.DurationInput,
    ) => Effect.Effect<A, E | GateLapse, WorkflowEngine.WorkflowEngine | WorkflowEngine.WorkflowInstance>
  }
  type Lapse = GateLapse
}

const _gate = <A, AI, E, EI>(name: string, shapes: Flow.Shapes<A, AI, E, EI>): Flow.Gate<A, E> => {
  const deferred = DurableDeferred.make(`${name}/signal`, shapes)
  return {
    deferred,
    token: (executionId) => DurableDeferred.tokenFromExecutionId(deferred, executionId),
    sealed: (window) =>
      Effect.race(
        DurableDeferred.await(deferred),
        Effect.zipRight(
          DurableClock.sleep({ name: `${name}/expiry`, duration: window }),
          Effect.fail(new GateLapse({ gate: name, window })),
        ),
      ),
  }
}

const Flow: {
  readonly pulse: Budget.Gated
  readonly gate: <A, AI, E, EI>(name: string, shapes: Flow.Shapes<A, AI, E, EI>) => Flow.Gate<A, E>
} = {
  pulse: Budget.schedule("lease"),
  gate: _gate,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Flow }
```
