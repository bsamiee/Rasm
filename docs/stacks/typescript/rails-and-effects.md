# [TS_RAILS_AND_EFFECTS]

This page is the carrier algebra. `Effect<A, E, R>` is the one rail every fallible, contextual, or deferred computation rides; this page legislates which carrier states an outcome, how dependence and independence compose, how a fault family is sized, routed, accumulated, and folded, how a resource bracket stacks against resilience, and how recurrence and telemetry ride the rail as policy values and transformers. A carrier is chosen once at admission and threaded unchanged: the narrowest carrier that states the real outcome transports the value, reusable transforms preserve it, and collapse to a bare value happens only at the run seam. The interior is total over admitted carriers — raw promises, thrown exceptions, `null` sentinels, and boolean status flags never travel it.

Six siblings own surfaces this algebra composes as settled material: the fiber that executes the rail — its interruption, racing, supervision, and shared cells — is `concurrency.md`'s; incremental dataflow under the same fault law is `streams.md`'s; the decode seam that mints admitted values and lifts `ParseError` is `boundaries.md`'s; the `Effect.fn` definition seam and `Match` terminals that attach and collapse these policies are `surfaces-and-dispatch.md`'s; the Tag requirement channel and its Layer satisfaction are `services-and-layers.md`'s; the declaration mechanics of tagged families are `shapes.md`'s. This page states what crosses onto the rail and which policy value transports it.

## [01]-[RAIL_CHOOSER]

Choose the narrowest carrier that states the real outcome; a wider carrier is earned only by a capability the narrower one cannot carry — a typed cause, deferral, context, or Cause-tree evidence. The chooser rows are this page's map: each row's law lives in the section that owns it.

| [INDEX] | [SURFACE]                                     | [OWNS]                              | [REJECT]                            |
| :-----: | :-------------------------------------------- | :---------------------------------- | :---------------------------------- |
|  [01]   | `Effect<A, E, R>`                             | fallible, contextual, deferred step | bare `Promise`, thrown control flow |
|  [02]   | `Option<A>`                                   | non-failing absence                 | `null`/`undefined` sentinels        |
|  [03]   | `Either<A, E>`                                | pure typed branch, per-slot operand | boolean-plus-payload records        |
|  [04]   | `Exit<A, E>` / `Cause<E>`                     | folded outcome evidence             | boolean status flags                |
|  [05]   | `Effect.all` mode, `partition`, `validateAll` | independent-batch disposition       | first-failure loop over a batch     |
|  [06]   | one `TaggedError` family + policy table       | fault architecture                  | class-per-cause spam, string reason |
|  [07]   | `Effect.acquireRelease` + `Scope`             | lifetime on the rail                | `try`/`finally` in domain flow      |
|  [08]   | `Schedule<Out, In, R>`                        | recurrence policy value             | hand-rolled retry and poll loops    |
|  [09]   | `Effect.withSpan` / `Metric` / `annotateLogs` | telemetry transformers              | branch-local logging, span pairs    |

[CARRIER_EMBEDDING]:
- Law: `Option` and `Either` are subtypes of `Effect` — a pure carrier composes onto the rail directly through `yield*` and `pipe`, so an inbound lift adapter is dead weight; `Effect.option`, `Effect.either`, and `Effect.exit` are outbound folds that reify a channel as a value, never inbound converters.
- Law: a yielded `Option.none` lifts as `Cause.NoSuchElementException` — a foreign fault in a domain channel — so absence converts at the point of knowledge: `Effect.mapError` re-spells the miss into the owning family in the same expression, and a bare `Option` reaches `yield*` only where the surface's boundary owns that re-spell.
- Law: promotion is earned by capability alone — `Option` carries non-failing absence, `Either` adds the typed cause to a pure branch, `Effect` adds deferral, requirements, and Cause tracking; a total pure transform modeled as `Effect` buys nothing and costs the run seam.
- Law: dependence licenses sequence, independence licenses product — `Effect.gen` and `Effect.flatMap` when a step consumes the prior value, `Effect.zipWith` and `Effect.all` with `{ concurrent: true }` when operands are independent; degrees of parallelism beyond the pair are `concurrency.md`'s decision.
- Reject: `Option.match` into `Effect.fail`/`Effect.succeed` re-deriving the embedding; `await` inside domain flow; an `Effect.gen` chain over operands no step depends on — it silently serializes and reports only the first fault.

[FOLDED_OUTCOME]:
- Law: async and outcome state is one folded tagged value — `Effect.exit` reifies the outcome, `Exit.match` folds it, and the folded value carries the whole `Cause` as evidence; a `{ loading, error, data }` record re-derives `Exit` by hand and admits the impossible states `Exit` excludes by construction.
- Law: the `Cause` tree retains typed failure, defect, and interruption — it is carried whole and discriminated once at the terminal seam; `[06]`'s outcome fold owns the discrimination order, and a fold that reads `.message` erases the tree the egress `Cause.pretty` renders.
- Use: `Effect.sandbox` when recovery itself must read defects and interruptions on the typed channel; `Effect.exit` when the outcome becomes state.

```typescript
import { Data, Effect, Exit, Option } from "effect"
import type { Cause, Either } from "effect"

class GaugeFault extends Data.TaggedError("GaugeFault")<{ readonly detail: string }> {}

const measured = (seed: number): Effect.Effect<number, GaugeFault> =>
  seed > 0 ? Effect.succeed(seed * 2) : Effect.fail(new GaugeFault({ detail: "<non-positive>" }))

const gauged = (
  cached: Option.Option<number>,
  gate: Either.Either<number, GaugeFault>,
): Effect.Effect<number, GaugeFault> =>
  Effect.gen(function* () {
    const seed = yield* cached.pipe(Effect.mapError(() => new GaugeFault({ detail: "<cold>" })))
    const ceiling = yield* gate
    return yield* Effect.zipWith(measured(seed), measured(ceiling), Math.min, { concurrent: true })
  })

const settled = Exit.match({
  onFailure: (cause: Cause.Cause<GaugeFault>) => ({ kind: "faulted", cause } as const),
  onSuccess: (value: number) => ({ kind: "resolved", value } as const),
})

type Settled = ReturnType<typeof settled>

const phased = (
  cached: Option.Option<number>,
  gate: Either.Either<number, GaugeFault>,
): Effect.Effect<Settled> => Effect.map(Effect.exit(gauged(cached, gate)), settled)

// --- [EXPORTS] ------------------------------------------------------------------------

export { GaugeFault, gauged, measured, phased, settled }
export type { Settled }
```

## [02]-[ACCUMULATION]

Abort versus accumulate is a correctness decision fixed once at the boundary as the combinator or its mode, never a boolean threaded into bodies. A dependent chain is abort-only — a later step consumes the earlier value, so there is nothing left to accumulate against. Independent operands choose the form by what must survive.

| [INDEX] | [FORM]                                     | [ERROR_CHANNEL]              | [SURVIVES]                                         |
| :-----: | :----------------------------------------- | :--------------------------- | :------------------------------------------------- |
|  [01]   | `Effect.all(shape, { mode: "validate" })`  | slot-keyed `Option<E>` shape | every fault, slot-addressed for repair             |
|  [02]   | `Effect.all(shape, { mode: "either" })`    | `never`                      | per-slot `Either` — partial success is the product |
|  [03]   | `Effect.validateAll(items, step)`          | `NonEmptyArray<E>`           | faults only — successes discarded on any fault     |
|  [04]   | `Effect.partition(items, step)`            | `never`                      | both halves — `[excluded, satisfying]`             |

[DISPOSITION_LAW]:
- Law: `mode: "validate"` is the repair report — the error channel keys each fault to its slot as `Option<E>`, so the caller repairs field-by-field; `mode: "either"` moves the split to the success channel when partial success is itself the deliverable.
- Law: `Effect.validateAll` is the all-or-nothing admission gate — on any fault every success is discarded, so choosing it where successes must survive is the named defect; that requirement is `partition`'s row, whose `[excluded, satisfying]` pair cannot fail and feeds the quarantine intake.
- Law: `NonEmptyArray<E>` on the accumulated channel is load-bearing — the fault set is provably inhabited, so `[03]`'s dominant fold consumes it with no emptiness guard.
- Use: `Effect.validate` for a fixed-arity pair accumulating both faults into the `Cause`; `Effect.validateFirst` when alternatives race for one success and the miss report must name every rejected attempt — `Effect.firstSuccessOf` keeps only the terminal fault.
- Reject: a disposition parameter the body re-reads; a `for` loop collecting failures beside the rail; `Effect.forEach` over a batch whose faults the caller must enumerate — its first-failure abort is the dependent-chain default, not the batch report.

```typescript
import { Data, Effect, Number } from "effect"
import type { Array, Option } from "effect"

class SlotFault extends Data.TaggedError("SlotFault")<{ readonly slot: string; readonly raw: string }> {}

const admit = (slot: string) => (raw: string): Effect.Effect<number, SlotFault> =>
  Number.parse(raw).pipe(Effect.mapError(() => new SlotFault({ slot, raw })))

const gated = (raw: {
  readonly floor: string
  readonly ceiling: string
}): Effect.Effect<
  { floor: number; ceiling: number },
  { floor: Option.Option<SlotFault>; ceiling: Option.Option<SlotFault> }
> => Effect.all({ floor: admit("floor")(raw.floor), ceiling: admit("ceiling")(raw.ceiling) }, { mode: "validate" })

const partitioned = (
  raws: ReadonlyArray<string>,
): Effect.Effect<[quarantined: ReadonlyArray<SlotFault>, admitted: ReadonlyArray<number>]> =>
  Effect.partition(raws, admit("intake"))

const sealed = (raws: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<number>, Array.NonEmptyArray<SlotFault>> =>
  Effect.validateAll(raws, admit("seal"))

const chained = (rawFloor: string, rawSpend: string): Effect.Effect<number, SlotFault> =>
  Effect.gen(function* () {
    const floor = yield* admit("floor")(rawFloor)
    const spent = yield* admit("spend")(rawSpend).pipe(
      Effect.filterOrFail((spend) => spend <= floor, () => new SlotFault({ slot: "spend", raw: rawSpend })),
    )
    return floor - spent
  })

// --- [EXPORTS] ------------------------------------------------------------------------

export { SlotFault, admit, chained, gated, partitioned, sealed }
```

## [03]-[FAULT_ARCHITECTURE]

A surface owns one reason-discriminated fault family whose policy lives in one value table. Routing, recovery, severity, and quarantine are all projections of that table — no arm of the program branches on a reason the table already maps.

[FAMILY_SIZING]:
- Law: the family is sized by routing, not by cause — a class per distinct payload-and-recovery route, one `catchTag` arm each, and a `reason` row per cause inside it. A class per cause turns `catchTags` into a switch over causes; one class with a free-string reason is unroutable and unfoldable — both are the named defects, and the declaration mechanics of the classes are `shapes.md`'s.
- Law: one `as const` policy table is the policy's single source of truth — rows carry rank, retry, and quarantine, `keyof typeof` derives the `Reason` union, the contract check is placed by the anchor's export reach, and the class getter projects the row so policy is recoverable from any fault value; a `switch` or `Match` over reasons re-derives what a row already states, and a new cause lands as one row plus zero new branches.
- Law: a fault no consumer arm can act on is a defect, not a channel member — `Effect.die` and `Effect.orDie` escalate it at the routing seam so `E` stays total over actionable faults and no handler carries a dead arm.

[ROUTING_AND_FOLDS]:
- Law: `catchTag` and `catchTags` route between classes; behavior inside a class reads policy fields — the handler record is the routing table, attached inline at the recovery seam, and a partial record leaves the remaining tags on the channel.
- Law: an accumulated fault set collapses to one representative through the rank lattice — `Order.mapInput` projects the table's rank, `Array.max` folds the `NonEmptyArray` the accumulating forms mint; an if-ladder comparing tags re-implements the order the table already declares.
- Law: quarantine is a typed divert, never a dropped element — a quarantinable fault is delivered to a typed intake and continues as `Either.left` on the success channel, so the rail proceeds and the evidence survives to the drain; a recovery that substitutes a default value destroys the evidence and is rejected wherever the fault feeds a report.
- Law: a recovery operator serves pipe and direct call as one `Function.dual` definition whose typed overload pair pre-solves inference at the owner — a parallel pipe-twin beside a data-first function is the named defect.

```typescript
import { Array, Data, Effect, Either, Function, Option, Order } from "effect"

const FaultPolicy = {                                        // exported anchor: plain as const keeps every literal; the merged guard carries the contract
  malformed: { rank: 4, retry: false, quarantine: true },
  contention: { rank: 2, retry: true, quarantine: false },
  exhausted: { rank: 3, retry: true, quarantine: false },
  breached: { rank: 5, retry: false, quarantine: false },
} as const

declare namespace FaultPolicy {
  type Row = { readonly rank: number; readonly retry: boolean; readonly quarantine: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof FaultPolicy> = T
}

type Reason = keyof typeof FaultPolicy

class ShapeFault extends Data.TaggedError("ShapeFault")<{
  readonly reason: Reason
  readonly surface: string
  readonly detail: string
}> {
  get policy(): (typeof FaultPolicy)[Reason] {
    return FaultPolicy[this.reason]
  }
}

class PermitFault extends Data.TaggedError("PermitFault")<{ readonly permit: string }> {}

const byRank: Order.Order<ShapeFault> = Order.mapInput(Order.number, (fault: ShapeFault) => fault.policy.rank)

const dominant = (faults: Array.NonEmptyReadonlyArray<ShapeFault>): ShapeFault => Array.max(faults, byRank)

const salvaged: {
  (intake: (fault: ShapeFault) => Effect.Effect<void>): <A, R>(
    self: Effect.Effect<A, ShapeFault, R>,
  ) => Effect.Effect<Either.Either<A, ShapeFault>, ShapeFault, R>
  <A, R>(
    self: Effect.Effect<A, ShapeFault, R>,
    intake: (fault: ShapeFault) => Effect.Effect<void>,
  ): Effect.Effect<Either.Either<A, ShapeFault>, ShapeFault, R>
} = Function.dual(
  2,
  <A, R>(
    self: Effect.Effect<A, ShapeFault, R>,
    intake: (fault: ShapeFault) => Effect.Effect<void>,
  ): Effect.Effect<Either.Either<A, ShapeFault>, ShapeFault, R> =>
    self.pipe(
      Effect.map(Either.right),
      Effect.catchIf(
        (fault) => fault.policy.quarantine,
        (fault) => Effect.as(intake(fault), Either.left(fault)),
      ),
    ),
)

const routed = <A, R>(
  work: Effect.Effect<A, ShapeFault | PermitFault, R>,
  floor: number,
): Effect.Effect<Option.Option<A>, ShapeFault, R> =>
  work.pipe(
    Effect.map(Option.some),
    Effect.catchTags({
      PermitFault: (fault) => Effect.die(fault),
      ShapeFault: (fault) => (fault.policy.rank <= floor ? Effect.succeed(Option.none<A>()) : Effect.fail(fault)),
    }),
  )

// --- [EXPORTS] ------------------------------------------------------------------------

export { FaultPolicy, PermitFault, ShapeFault, byRank, dominant, routed, salvaged }
export type { Reason }
```

## [04]-[RESOURCE_BRACKET]

A lifetime rides the rail as a bracket whose release is part of the computation's type, and where the bracket sits against resilience is semantics, not style: a transformer governs everything below it in the pipe.

[BRACKET_SELECTION]:
- Law: the form is selected by two questions — does a value need releasing, and does teardown read the outcome. `Effect.acquireRelease` owns a resource value with an `Exit`-aware release; `Effect.acquireUseRelease` is the closed bracket when use is known at the declaration and no `Scope` should escape; `Effect.ensuring` finalizes unconditionally with no resource; `Effect.onExit` observes the settled outcome; `Effect.onError` observes failure alone; `Effect.addFinalizer` registers into the ambient `Scope` mid-`gen`.
- Law: an `Exit`-aware release is transactional disposal — `Exit.isFailure` selects commit or abort — the discrimination a `finally` cannot express.
- Law: the release channel is `never` by signature — a fallible release resolves its own failure internally, because the primary outcome must survive teardown; a release failure is a defect, never a fault.
- Boundary: uninterruptible windows, shielded teardown, and cross-fiber finalization order are `concurrency.md`'s; a service whose lifetime is its Layer is `services-and-layers.md`'s.

[SCOPE_PLACEMENT]:
- Law: `Effect.scoped` placement against `Effect.retry` decides re-acquisition — retry around the scoped bracket re-acquires per attempt, the poisoned-resource recovery; the bracket around retry holds one acquisition across attempts, the kept-session form. Neither is a default; the fault family names which resource state survives its faults.
- Reject: teardown as an ordinary step after use — it silently skips on failure and interruption; `try`/`finally` in domain flow; a bracket whose acquire is retried by a loop instead of sitting under the same policy value as its use.

```typescript
import { Data, Effect, Exit } from "effect"
import type { Schedule, Scope } from "effect"

class LeaseFault extends Data.TaggedError("LeaseFault")<{
  readonly stage: "open" | "probe"
  readonly key: string
}> {}

type Lease = {
  readonly key: string
  readonly commit: Effect.Effect<void>
  readonly abort: Effect.Effect<void>
  readonly probe: Effect.Effect<number, LeaseFault>
}

const leased = (
  mint: (key: string) => Effect.Effect<Lease, LeaseFault>,
  key: string,
): Effect.Effect<Lease, LeaseFault, Scope.Scope> =>
  Effect.acquireRelease(mint(key), (lease, exit) => (Exit.isFailure(exit) ? lease.abort : lease.commit))

const perAttempt = (
  lease: Effect.Effect<Lease, LeaseFault, Scope.Scope>,
  policy: Schedule.Schedule<unknown, LeaseFault>,
): Effect.Effect<number, LeaseFault> =>
  Effect.retry(Effect.scoped(Effect.flatMap(lease, (live) => live.probe)), policy)

const oneLease = (
  lease: Effect.Effect<Lease, LeaseFault, Scope.Scope>,
  policy: Schedule.Schedule<unknown, LeaseFault>,
): Effect.Effect<number, LeaseFault> =>
  Effect.scoped(Effect.flatMap(lease, (live) => Effect.retry(live.probe, policy)))

const flushed = <A, E, R>(work: Effect.Effect<A, E, R>, drain: Effect.Effect<void>): Effect.Effect<A, E, R> =>
  Effect.ensuring(work, drain)

const audited = (lease: Effect.Effect<Lease, LeaseFault, Scope.Scope>): Effect.Effect<number, LeaseFault, Scope.Scope> =>
  Effect.gen(function* () {
    yield* Effect.addFinalizer((exit) => Effect.annotateCurrentSpan("lease.exit", Exit.isSuccess(exit)))
    const live = yield* lease
    return yield* live.probe
  })

// --- [EXPORTS] ------------------------------------------------------------------------

export { audited, flushed, leased, LeaseFault, oneLease, perAttempt }
export type { Lease }
```

## [05]-[POLICY_VALUE]

Recurrence is a named `Schedule` value composed once beside the fault family it serves; retry, repeat, and timeout are transformers that consume policy values — a loop, a counter, or a timer callback re-implements the algebra by hand and loses composition, jitter, and the gate.

[RECURRENCE_ALGEBRA]:
- Law: the policy is typed to its family through the input channel — `Schedule.whileInput` over the family's own projection gates recurrence on the fault value, so the policy cannot be misapplied to a foreign surface and every consumer inherits the gate; a call-site predicate re-deriving the gate is policy leakage.
- Law: `Schedule.intersect` bounds and `Schedule.union` extends — intersect continues only while both operands continue, at the longer delay; union continues while either does, at the shorter; an infinite backoff unioned with a finite bound is still infinite, so the bounds are `Schedule.intersect(Schedule.recurs(n))` for attempts and `Schedule.upTo(duration)` for elapsed time, stacked when the budget names both.
- Law: `Schedule.jittered` decorrelates a fleet — a bare exponential curve synchronizes retries into waves against the same dependency.
- Use: `Schedule.cron` for calendar recurrence in place of a sleep loop; `Schedule.tapOutput` to observe the policy's own decisions.

[TRANSFORMER_FORMS]:
- Law: `Effect.retry` is one entry over `{ times, while, until, schedule }` — the options object is the collapsed suffix family, and `{ times: n }` mints no `Schedule` for a trivial bound; the fault-keyed gate rides the policy value, the options carry only call-local intent.
- Law: a deadline mints a family fault through `Effect.timeoutFail` — bare `Effect.timeout` widens `E` with the foreign `Cause.TimeoutException`; `Effect.timeoutTo` folds to a value where the deadline is a legitimate outcome rather than a fault.
- Law: layering against retry is budget semantics — a timeout below `Effect.retry` is the per-attempt budget, above it the total budget; both may stack, and which budgets exist is the surface's stated vocabulary, never an accident of pipe order.
- Law: `Effect.repeat` recurs on the success channel — `while`/`until` read the output and the first run is not a recurrence — so polling and convergence are repeat policies over a state-advancing step, never `while` statements.

```typescript
import { Data, type Duration, Effect, Schedule } from "effect"

class PressFault extends Data.TaggedError("PressFault")<{ readonly reason: "jam" | "starve" }> {
  get transient(): boolean {
    return this.reason === "starve"
  }
}

const backoff: Schedule.Schedule<[Duration.Duration, number], PressFault> = Schedule.exponential("40 millis", 2).pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(6)),
  Schedule.upTo("20 seconds"),
  Schedule.whileInput((fault: PressFault) => fault.transient),
)

const resilient = <A, R>(attempt: Effect.Effect<A, PressFault, R>): Effect.Effect<A, PressFault, R> =>
  attempt.pipe(
    Effect.timeoutFail({ duration: "2 seconds", onTimeout: () => new PressFault({ reason: "starve" }) }),
    Effect.retry(backoff),
    Effect.timeoutFail({ duration: "30 seconds", onTimeout: () => new PressFault({ reason: "jam" }) }),
  )

const bounded = <A, R>(attempt: Effect.Effect<A, PressFault, R>): Effect.Effect<A, PressFault, R> =>
  Effect.retry(attempt, { times: 2 })

const converged = <R>(step: Effect.Effect<number, PressFault, R>): Effect.Effect<number, PressFault, R> =>
  Effect.repeat(step, { until: (drift) => drift < 1e-6, times: 256 })

// --- [EXPORTS] ------------------------------------------------------------------------

export { backoff, bounded, converged, PressFault, resilient }
```

## [06]-[OBSERVABILITY]

Telemetry is a transformer stack attached at the owner declaration, and every signal derives its outcome from `Exit` — one fold, one emission point, one bounded dimension vocabulary.

[RAIL_TRANSFORMERS]:
- Law: span, log annotation, and metric attach as transformers on the same pipe that carries resilience — one `Effect.withSpan` per surface, policy recoverable from the declaration; the `Effect.fn` pipeline slot these transformers ride is `surfaces-and-dispatch.md`'s seam.
- Law: `Effect.annotateLogs` record-form at the surface entry stamps every log in the region — per-call-site key spam restates context the region already carries, and `Effect.annotateSpans` carries the same record to the trace side.
- Reject: manual span open/close pairs; `Effect.log` narrating control flow the span already records; telemetry buried inside the body where the declaration no longer states it.

[BOUNDED_DIMENSIONS]:
- Law: a metric tag value is drawn from a bounded vocabulary — the derived outcome union, the family's reason rows — because every distinct value mints a series; interpolating an identifier into a tag is the cardinality defect. Identifier-grade context belongs in span attributes and log annotations, which are per-occurrence, never per-series.
- Law: instruments are declared once beside the family they measure — `Metric.counter` with `incremental: true` for monotonic counts, `Metric.frequency` over the reason vocabulary so its value set is exactly the derived union, `Metric.timerWithBoundaries` with boundaries the budget names, `MetricBoundaries.exponential` where the range spans decades — and `Metric.tagged` at call time is licensed only by a bounded value.

[OUTCOME_FROM_EXIT]:
- Law: one `Exit` fold derives the outcome dimension for every signal, discriminating in interrupt-first order — `Cause.isInterruptedOnly`, then `Cause.failureOption`, then defect — because an interrupted run has no outcome and a defect is not a fault; the fold's return type is the dimension vocabulary, derived from the fold itself, never hand-listed beside it.
- Law: `Effect.onExit` is the single emission point — once per computation, after the outcome settles; outcome strings minted inside recovery arms drift, double-count retried attempts, and never see defects.
- Law: measurement placement follows `[04]`'s stacking law — `Metric.trackDuration` and `Metric.trackErrorWith` below the retry stack measure attempts, above it the composed operation; the choice is the instrument's meaning, stated by its position.

```typescript
import { Cause, Data, Effect, Exit, Metric, Option } from "effect"

class PourFault extends Data.TaggedError("PourFault")<{ readonly reason: "clog" | "spill" }> {}

const outcomeOf = Exit.match({
  onFailure: (cause: Cause.Cause<PourFault>) =>
    Cause.isInterruptedOnly(cause)
      ? ("halted" as const)
      : Option.match(Cause.failureOption(cause), {
          onNone: () => "crashed" as const,
          onSome: (fault) => `rejected:${fault.reason}` as const,
        }),
  onSuccess: () => "resolved" as const,
})

type Outcome = ReturnType<typeof outcomeOf>

const _poured = Metric.counter("pour_total", { description: "<terminal outcomes>", incremental: true })
const _reasons = Metric.frequency("pour_fault_reason")
const _latency = Metric.timerWithBoundaries("pour_latency_millis", [5, 25, 125, 625])

const observed = <A, R>(pour: Effect.Effect<A, PourFault, R>): Effect.Effect<A, PourFault, R> =>
  pour.pipe(
    Metric.trackDuration(_latency),
    Metric.trackErrorWith(_reasons, (fault: PourFault) => fault.reason),
    Effect.onExit((exit) => Metric.increment(Metric.tagged(_poured, "outcome", outcomeOf(exit)))),
    Effect.withSpan("pour.resolve", { attributes: { "pour.surface": "<surface-a>" } }),
    Effect.annotateLogs({ surface: "<surface-a>" }),
  )

// --- [EXPORTS] ------------------------------------------------------------------------

export { observed, outcomeOf, PourFault }
export type { Outcome }
```
