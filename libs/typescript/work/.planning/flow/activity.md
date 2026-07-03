# [WORK_ACTIVITY]

An activity is the once-executed durable step: `Activity.make` records its exit through the engine's storage, replay never re-runs a recorded side effect, and every resilience number it wears comes from the kernel `Budget` rows — never a literal, never a loop. This page owns two folder-wide laws. `Step.run` is the one budgeted mint: it composes the per-attempt deadline below the class-gated retry and the whole-step deadline above it — the rails layering geometry read directly off `Budget[kind].attempt`/`.total` and `Budget.schedule(kind)` — with `Step.metered` adding the durable throttle row where a step consumes a fleet-shared quota. The fault convention is sealed here for every page after: a work fault is a reason-discriminated `Schema.TaggedError` whose `as const` policy table carries a `class` column drawn from the kernel `FaultClass` vocabulary, and the fault projects `get class()` from its row — so `Budget.schedule`'s structural gate re-drives exactly the transient reasons with zero call-site predicates. An inline schedule, a retry counter, a re-run for a side effect, and a fault family without its class column are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                          |
| :-----: | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | [STEP]        | the budgeted activity mint, the deadline geometry, the durable throttle row       |
|  [02]   | [STEP_POLICY] | the folder fault convention, dedupe keys, speculative race, attempt evidence      |

## [2]-[STEP]

[STEP]:
- Owner: `Step` — the budgeted step mint. `Step.run({ name, budget, execute })` composes `Activity.make` over the attempt-deadlined body, `Activity.retry` under the compiled kernel schedule, and the total deadline over the whole retried step: `Budget[budget].attempt` composes below the retry so each try owns its slice, `Budget.schedule(budget)` re-drives only what the kernel classifies transient, and `Budget[budget].total` composes above so the step's whole life is bounded — three policy values, one declaration, the geometry recoverable from the mint alone. `Step.metered` runs the same mint behind a `DurableRateLimiter.rateLimit` row, so a provider-quota step spends a durable token before each attempt and a restart cannot double-spend the window.
- Law: the deadline fault is this page's `StepLapse` — it carries the step name, the budget kind, and the phase (`attempt` re-drives under class `expired`; `total` is the same class surfacing after the schedule is spent) — so a deadline is evidence on the typed channel, never a bare `TimeoutException` caught downstream.
- Law: replay is why the body must be honest — an `execute` that already ran to success replays from the recorded exit; a side effect hidden outside the activity boundary repeats on every replay, so every effectful line of a durable flow lives inside a `Step.run` body or an engine-owned primitive.
- Law: quota rows are policy values — `{ window, limit, algorithm }` with the runtime `key` supplied at the call, `token-bucket` for burst-absorbing provider quotas, `fixed-window` for hard contracts — and the throttle is a durable activity itself, so the spent token survives a crash between gate and work.
- Boundary: the schedule compilation, jitter, reset, and gate live on the kernel `Budget` owner — this page selects rows and places transformers; the engine that persists exits is `engine/storage.md`'s row selection; interruption semantics under an activity's `interruptRetryPolicy` take the same `Budget.schedule` values.
- Entry: `Step.run({ name, budget, execute })`; `Step.metered({ name, budget, execute, quota, key })` where a fleet quota gates the step.
- Growth: a new resilience envelope is a kernel budget row every step inherits by name; a new step-level axis (a hedge, a priority tag) is one `Spec` field.
- Packages: `@effect/workflow` (`Activity`, `DurableRateLimiter`, `WorkflowEngine`), `effect` (`Data`, `Duration`, `Effect`), `@rasm/ts/kernel` (`Budget`, `FaultClass`).

```typescript
import { Activity, DurableRateLimiter, type WorkflowEngine } from "@effect/workflow"
import { Budget, type FaultClass } from "@rasm/ts/kernel"
import { Data, type Duration, Effect } from "effect"

class StepLapse extends Data.TaggedError("StepLapse")<{
  readonly name: string
  readonly budget: Budget.Kind
  readonly phase: "attempt" | "total"
}> {
  get class(): FaultClass.Kind {
    return "expired"
  }
}

declare namespace Step {
  type Spec<A, E, R> = {
    readonly name: string
    readonly budget: Budget.Kind
    readonly execute: Effect.Effect<A, E, R>
  }
  type Quota = {
    readonly window: Duration.DurationInput
    readonly limit: number
    readonly algorithm: "fixed-window" | "token-bucket"
  }
  type Lapse = StepLapse
}

const _lapsed = (name: string, budget: Budget.Kind, phase: "attempt" | "total") => () =>
  new StepLapse({ name, budget, phase })

const _run = <A, E, R>(
  spec: Step.Spec<A, E, R>,
): Effect.Effect<A, E | StepLapse, R | WorkflowEngine.WorkflowEngine> =>
  Effect.timeoutFail(
    Activity.retry(
      Activity.make({
        name: spec.name,
        execute: Effect.timeoutFail(spec.execute, {
          duration: Budget[spec.budget].attempt,
          onTimeout: _lapsed(spec.name, spec.budget, "attempt"),
        }),
      }),
      { schedule: Budget.schedule(spec.budget) },
    ),
    { duration: Budget[spec.budget].total, onTimeout: _lapsed(spec.name, spec.budget, "total") },
  )

const _metered = <A, E, R>(
  spec: Step.Spec<A, E, R> & { readonly quota: Step.Quota; readonly key: string },
): Effect.Effect<A, E | StepLapse, R | WorkflowEngine.WorkflowEngine> =>
  Effect.zipRight(
    DurableRateLimiter.rateLimit({ name: `${spec.name}/quota`, key: spec.key, ...spec.quota }),
    _run(spec),
  )

const Step: {
  readonly run: <A, E, R>(spec: Step.Spec<A, E, R>) => Effect.Effect<A, E | StepLapse, R | WorkflowEngine.WorkflowEngine>
  readonly metered: <A, E, R>(
    spec: Step.Spec<A, E, R> & { readonly quota: Step.Quota; readonly key: string },
  ) => Effect.Effect<A, E | StepLapse, R | WorkflowEngine.WorkflowEngine>
} = { run: _run, metered: _metered }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Step }
```

## [3]-[STEP_POLICY]

[STEP_POLICY]:
- Owner: the folder fault convention — every `deliver` and `queue` fault family instantiates one shape: a reason tuple, an `as const` policy table whose rows carry `class: FaultClass.Kind` plus the surface's own columns (`suppress`, `park`, a status), a `Schema.TaggedError` carrying `reason` plus evidence fields, `get policy()` projecting the row, and `get class()` projecting the class column. Classification is thereby structural: `FaultClass.of` probes the `class` property, so `Budget.schedule`'s gate, `Storage.transient`, and the kernel dominance fold all read work faults with zero adapters.
- Law: reasons are sized by evidence-and-recovery, classes by the kernel — a finer cause is a reason row inside the owning family, never an eleventh kernel class; the class column is the only retry authority, so flipping a reason from terminal to transient is a one-cell edit every budget gate inherits.
- Law: the dedupe key is declared, never minted — a step's identity is `Activity.idempotencyKey(name, { includeAttempt })`: the bare form makes retries one durable key (the exactly-once-effect default), `includeAttempt: true` splits attempts into distinct keys for steps whose provider bills per submission; the workflow-level replay key is `flow/durable.md`'s `executionId` law, and an ad-hoc run id beside either is the named defect.
- Law: speculation is the engine's fold — `Activity.raceAll(name, activities)` records the first durable completion as the step's exit and the losers' work is abandoned by the engine, so a hedged read over redundant lanes is one spelling; a hand race over activities re-implements the recorded-exit merge wrong.
- Law: attempt evidence rides the defaulted reference — `yield* Activity.CurrentAttempt` inside a step body reads the engine's own counter for receipts and span annotation, so no counter parameter threads beside the rail; `deliver` receipts stamp it.
- Boundary: fault-family declaration mechanics are the settled Schema owner forms; which reasons exist per channel is each deliver page's table; the kernel owns the class vocabulary this convention projects into.
- Growth: a new fault family elsewhere in the folder is one reason tuple plus one policy table under this shape — the convention itself is closed.
