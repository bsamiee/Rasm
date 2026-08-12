# [RUNTIME_QUEUE]

Durable-work intake: restart-surviving job families on the native `DurableQueue`, keyed quotas spent through two arms over one store-backed counter — `DurableRateLimiter` inside a step, the raw limiter outside one — and the pg-composed lane policy over the data wave's outbox statements — claim admission, claim lease, urgency order, park ceiling, and operator replay as one verdict vocabulary spelled ONCE for every drain in the branch. Service-class pricing arrives settled from `entity#WORK_CLASS`.

Decomposition is ruled: `@effect/cluster` and `@effect/workflow` natively own persistence, dedup, and worker execution, while the ordering-and-parking layer is pg-composed — the journal's `SKIP LOCKED` claim with an `ORDER BY` urgency term carries the queue engines' visibility-timeout and archive semantics as lease and park columns, and no third layer exists.

Dead-lettering lives here alone: a parked deliverable is typed evidence on the fact journal, replay an operator fold re-minting deliverables from that evidence. Its module ships on the `./server` subpath as `runtime/src/work/queue.ts`.

## [01]-[INDEX]

- [02]-[JOB_FAMILY]: persisted job declaration law, dedup projection, class-priced workers; `Job`.
- [03]-[THROTTLE]: keyed quotas — algorithm rows, tenant keys, cost weights, the durable and paced spend arms; `Throttle`.
- [04]-[LANE_POLICY]: claim lease, urgency order, batch geometry, claim admission, the verdict fold; `Lane`.
- [05]-[PARK_REPLAY]: dead-letter evidence, the park ceiling, poison short-circuit, operator replay; `Lane`.

## [02]-[JOB_FAMILY]

- Law: dedup identity is the payload projection — `idempotencyKey` derives from payload content exactly as `flow#FLOW_LAW`'s `executionId` does, so a re-enqueued equal payload joins the in-flight item instead of duplicating work; a caller-minted job id is the rejected form.
- Law: a job body is `Step.run` material — the worker's handle composes the flow mint for its deadline geometry, so queue workers and workflow activities carry identical budget shapes and evidence; the family's declared `success` threads the handle's result through the step's persisted exit to the suspended `submit` caller, and the declared `error` unions the spec's fault schema with `StepFault` so a budget trip persists beside domain failure under one wire family — a family whose result is fire-and-forget declares `Schema.Void` as its `success` row, never a second void-only mint.
- Law: fire-and-forget is a modality of the same family — a scoped caller may supervise `process` with `Effect.forkScoped`, while a request that must acknowledge durable admission keeps awaiting the declared success; an unscoped daemon fiber or a second "unawaited" queue declaration is unspellable.
- Growth: a new job kind is one `DurableQueue.make` value with one worker Layer row at the composition root; a family outgrowing single-item settlement into multi-step orchestration promotes to a `flow` definition, re-homing the payload schema unchanged.
- Boundary: the queue's item store is the `PersistedQueueFactory` arm of the `entity#MAILBOX` tier row — a distinct store from the cluster envelope `MessageStorage` beside it, which `DurableQueue.worker`'s own requirement names by type and which `MessageStorage` cannot satisfy; the tier selection at the root is what makes every worker Layer composable, and no queue table, poll loop, or storage row exists on this page.
- Packages: `@effect/workflow` (`DurableQueue`); `effect` (`Effect`, `Function`, `Schema`); `@rasm/ts/core` (`Fault.Budget`); `./entity.ts` (`WorkClass`).

```typescript signature
import { DurableQueue, DurableRateLimiter } from "@effect/workflow"
import { RateLimiter as Fleet } from "@effect/experimental"
import type { SqlClient, SqlError } from "@effect/sql"
import { Array, Data, Duration, Effect, Function, Match, Option, Schema, Stream } from "effect"
import { type AuditFact, Fact, Journal } from "@rasm/ts/data"
import { Fault } from "@rasm/ts/core"
import { Pulse } from "../otel/meter.ts"
import { WorkClass } from "./entity.ts"
import { Step, StepFault } from "./flow.ts"

declare namespace Job {
  type Spec<Name extends string, A, I, S, SI, E extends { readonly class: Fault.Class.Kind }, EI> = {
    readonly name: Name
    readonly payload: Schema.Schema<A, I>
    readonly success: Schema.Schema<S, SI>
    readonly error: Schema.Schema<E, EI>
    readonly clazz: WorkClass.Kind
    readonly key: (payload: A) => string
  }
}

const _job = <Name extends string, A, I, S, SI, E extends { readonly class: Fault.Class.Kind }, EI>(
  spec: Job.Spec<Name, A, I, S, SI, E, EI>,
) => {
  const row = WorkClass[spec.clazz]
  const queue = DurableQueue.make({
    name: spec.name,
    payload: spec.payload,
    idempotencyKey: spec.key,
    success: spec.success,
    error: Schema.Union(spec.error, StepFault),
  })
  return {
    queue,
    // `Job.Spec`'s `E` closes the DOMAIN channel on `class`; the PERSISTENCE channel stays open — this schedule sees
    // `PersistedQueueError`, whose `_tag`/`message`/`cause` carry none, so the default gate refuses every re-drive
    submit: (payload: A) =>
      DurableQueue.process(queue, payload, { retrySchedule: Fault.Budget.schedule(row.budget, Function.constTrue) }),
    worker: <R>(handle: (payload: A) => Effect.Effect<S, E, R>) =>
      DurableQueue.worker(
        queue,
        (payload: A) =>
          Step.run(spec.name, spec.clazz, {
            success: spec.success,
            error: spec.error,
            execute: handle(payload),
          }),
        { concurrency: row.concurrency },
      ),
  } as const
}

const Job = { of: _job }
```

## [03]-[THROTTLE]

- Owner: `Throttle` — keyed quotas as one row table spent through two arms: `Throttle.spend(row, subject)` runs `DurableRateLimiter.rateLimit` as an activity whose consumption survives replay, so a retried step never double-spends its quota, and `Throttle.pace(row, subject)` is its transformer twin for every caller that sits OUTSIDE a durable step. Each generic row carries its scope, algorithm, window, limit, compound-key projection, and cost projection, so consumers cannot pass a key or cost inconsistent with the selected quota.
- Law: cost is a parameter — a heavyweight item spends `cost > 1` against the same row; a parallel "heavy" quota row for the same scope is the rejected form. The column spells `cost` because the serving edge's own quota table spells it identically: window, limit, key, and cost are the four columns every posture in the branch shares, and only the posture differs.
- Law: a row states its FAN AXIS, never its projections — every quota keys tenant-then-axis and costs the subject's own weight, so one generator mints both closures from the axis name and a row carries scope, algorithm, window, limit, and that one word. Hand-written projections re-spell the subject shape once per closure and let a row drift its key grammar silently; the table closes against `Row<never>`, whose contravariant subject admits every row while still refusing a bad algorithm, a missing projection, or a mistyped cost.
- Law: the BUCKET is the scope joined to the projection, derived at the spend seams and nowhere else — `DurableRateLimiter`'s `name` namespaces the activity and its `DurableClock` sleep while the store receives `key` verbatim, so two rows fanning on different axes whose values coincide would spend one another's tokens under a bare projection. Both arms fold the identical `_bucket`, which is also what makes them one counter rather than two: a row's quota holds whether a step or a drain spent it.
- Law: the store is a PORT, never a backing this page names — both arms resolve the one `Fleet.RateLimiter` Tag (the durable activity requires it exactly as the raw accessor does), and the composition root binds `layerStoreMemory` on a single node or a shared store-backed Layer across a fleet; a page that named a store would make one deployment's topology every deployment's, and a Redis or relational bucket store enters as that Layer's own seam.
- Law: exhaustion delays and never refuses on BOTH arms — the durable arm's exceeded posture is a `DurableClock` sleep sized to the window turn, so a step that overdraws parks durably and resumes past process death with the spend already consumed, and the paced arm carries `onExceeded: "delay"` for the same posture without the replay guarantee. Each leaves only `RateLimitStoreError` (`_tag: "RateLimiterError"` — the quota STORE failed) on its channel, which classifies `unavailable` so a lane judge defers it on the lease; a hand-written wait-for-window loop, or a page modeling exhaustion as a refusal fault, contradicts the shipped posture and stays unspellable.
- Law: the two arms split by SPELLABILITY, never by preference — `DurableRateLimiter.rateLimit` is an `Activity`, so it exists only inside a durable step, while the lane drain, the outbox relay, and every enqueue-side pacer run outside one and would otherwise re-mint a quota the table already holds; `pace` is what keeps them on the declared row instead.
- Law: process-plane admission pressure is a REFUSAL and lives at `serve/route#SEAM_ROWS`'s `Seam.quota`, which prices the request edge against this same store and answers a burst caller with a Problem; a `Throttle` row prices durable work and answers by waiting, so the two tables share four columns and split on the one thing a caller feels — one concern appearing in both is the split the row's `scope` name makes visible at review.
- Growth: a new quota is one table row both arms inherit; a new pacing shape is an `algorithm` value the shipped surface names.
- Packages: `@effect/workflow` (`DurableRateLimiter`); `@effect/experimental` (`RateLimiter` — the accessor, the store Tag, and the store fault); `effect` (`Duration`, `Effect`).

```typescript signature
declare namespace Throttle {
  type Subject<Axis extends string> = { readonly tenant: string; readonly weight: number } & { readonly [K in Axis]: string }
  type Row<A> = {
    readonly scope: string
    readonly algorithm: "fixed-window" | "token-bucket"
    readonly window: Duration.DurationInput
    readonly limit: number
    readonly key: (subject: A) => string
    readonly cost: (subject: A) => number
  }
}

// Both projections derive from the ONE axis a row fans on: the compound key is always tenant-then-axis and the cost is
// always the subject's own weight, so a row states its axis and never re-spells the arithmetic or its subject shape.
const _keyed = <const Axis extends string>(axis: Axis) => ({
  key: (subject: Throttle.Subject<Axis>) => `${subject.tenant}:${subject[axis]}`,
  cost: (subject: Throttle.Subject<Axis>) => subject.weight,
})

const _rows = {
  tenantEgress: { scope: "tenant-egress", algorithm: "token-bucket", window: Duration.minutes(1), limit: 600, ..._keyed("channel") },
  providerCall: { scope: "provider-call", algorithm: "fixed-window", window: Duration.minutes(1), limit: 240, ..._keyed("provider") },
  reportRender: { scope: "report-render", algorithm: "token-bucket", window: Duration.minutes(5), limit: 50, ..._keyed("format") },
  // contravariance makes `Row<never>` the shape check that admits every subject: scope, algorithm, window, limit, and
  // both projections refuse at the table instead of at the one `spend` call that happened to name a broken row
} as const satisfies { readonly [Name: string]: Throttle.Row<never> }

// `DurableRateLimiter`'s `name` namespaces the ACTIVITY and its DurableClock sleep; the store receives `key` verbatim,
// so a bare projection lets two rows fanning on different axes spend one bucket the moment their values coincide.
// Deriving the bucket here is also what joins the arms: one row, one counter, whether a step or a drain spent it.
const _bucket = <A>(row: Throttle.Row<A>, subject: A): string => `${row.scope}:${row.key(subject)}`

const _spend = <A>(row: Throttle.Row<A>, subject: A) =>
  DurableRateLimiter.rateLimit({
    name: row.scope,
    algorithm: row.algorithm,
    window: row.window,
    limit: row.limit,
    key: _bucket(row, subject),
    tokens: row.cost(subject),
  })

// The arm for every caller outside a durable step. `delay` makes the `Exceeded` fault unreachable while the shipped
// union still names it, so this arm dies on it exactly as the durable activity does internally — both channels then
// carry `RateLimitStoreError` alone, which is the one quota fault a lane judge ever grades.
const _pace = <A>(row: Throttle.Row<A>, subject: A) =>
<X, E, R>(self: Effect.Effect<X, E, R>): Effect.Effect<X, E | Fleet.RateLimitStoreError, R | Fleet.RateLimiter> =>
  Effect.flatMap(Fleet.makeWithRateLimiter, (limit) =>
    limit({
      algorithm: row.algorithm,
      key: _bucket(row, subject),
      limit: row.limit,
      onExceeded: "delay",
      tokens: row.cost(subject),
      window: row.window,
    })(self)).pipe(
      Effect.catchTag("RateLimiterError", (fault) => fault.reason === "Exceeded" ? Effect.die(fault) : Effect.fail(fault)),
    )

const Throttle = { ..._rows, pace: _pace, spend: _spend }
```

## [04]-[LANE_POLICY]

- Owner: `Lane` — the drain policy over the data journal's outbox statements. Data's wave owns the relation and the two statements (`Journal.claimBatch(sql, { app, take, leaseSeconds })` — `FOR UPDATE SKIP LOCKED` with attempt increment, `Journal.complete(sql, ids)` — the delivered mark); this page owns what a drain DOES with them: the lease width (`leaseSeconds` derived from the class row's per-attempt budget — the visibility-timeout semantic mined from the external queue engines, expressed as the claim statement's own re-claim predicate), the urgency term (the `ORDER BY` column populated from `WorkClass[clazz].urgency` at enqueue so interactive deliverables pass bulk ones under contention), the batch geometry (`take` sized by the drain's class row), the claim admission seam, and the verdict fold.
- Law: `fits` is the coordinate a selector reads FIRST, so this plane states it rather than leaving it inferable from the verdict vocabulary — a lane suits durable work a lease may safely re-run: an outbox drain, a scheduled deliverable, any claim whose payload carries its own dedup projection. Work wanting an in-process answer, per-key ordering, or exactly-once execution is not a lane's and reads `net/pubsub#PORT_SHAPE` instead of bending a claim into one.
- Law: admission happens at the lane seam, exactly once — `Lane.row(payload, drain)` is the one admission mint: it fuses a payload `Schema` with a domain drain so the data-owned raw claim `payload` decodes before any domain code runs, a decode failure folds to an `invalid`-classed park through the poison short-circuit, and the drain receives the admitted payload beside the claim itself — a raw `payload: unknown` reaching a domain drain, or a drain-local decoder, is the consumer-local-admission defect this mint forecloses, and payload shape authority is always recoverable from the row that routed the tag.
- Law: `Meta<Row>` subtracts the raw column and keeps every other one the data-owned claim decoded, so a drain needing a coordinate the statement already answered reads it off its own claim; a projection keyed by claim identity against a map built from the same batch re-proves a join the fold holds in hand and mints an absent-row verdict no input reaches.
- Law: the verdict vocabulary is closed — `Settled` (the effect landed: `Journal.complete`), `Deferred` (transient fault: the row stays claimed and the lease expiry re-delivers it, attempts already incremented), `Parked` (the ceiling, a non-retryable class, a failed admission, or an unrouted stream: `[5]`'s evidence fold) — and every drain in the branch folds claims through `Lane.settle`, so retry-with-redelivery is spelled once and a drain-local retry loop is unspellable. `Lane.settle` answers the batch's verdict roster, so a relay meters its pass from the returned values instead of a second count.
- Law: a store fault is not a verdict — `_judge` rules on a domain fault carrying `class` and `detail`, so the `SqlError` a claim discharge raises has no `LaneVerdict` to become and rides `Lane.settle`'s own error channel to the drain, where the budget gate grades it against the journal's published projection; widening it into the cause channel instead hands it to a grader that reads a `class` property no driver fault carries and refuses every replay, which is what puts the lease-IS-the-backoff law out of reach for the one fault the lease was shaped to absorb.
- Law: defer is passive — no un-claim write, no backoff column; the lease IS the backoff, and its width is the class row's per-attempt budget, so redelivery pacing derives from the same geometry as in-process retry.
- Law: the verdict vocabulary IS the lifetime answer — a claim's custody ends at `Settled` (the drain ends it), at lease expiry under `Deferred` (the clock ends it), or at `Parked` (the ceiling or the class table ends it) — three endings by three owners, so no consumer infers a span this plane never measures.
- Law: a lane decides `tenancy` nowhere — claim admission is app-scoped and the decoded payload carries whatever tenant its own channel declares, so a lane row states the non-decision rather than a value it never realizes.
- Law: the claim consumer's statements ride the MAINTENANCE plane — `Journal.claimBatch` and the one `Journal.complete` discharge are cross-tenant reads and writes on FORCE-RLS relations, so each composes `Tenancy.sweep`, the data-owned transformer pinning only `rasm.plane = 'maintenance'` (`docs/laws/patterns.md` `[SESSION_GUC]`): an unpinned pass reads ZERO deliverables and reports every empty claim as a healthy cycle, and a pass opened inside `Tenant.within` narrows the estate drain to one tenant's slice while every other tenant's leases lapse — the two silent inversions the stated posture forecloses. `Tenancy.sweep` brackets STATEMENTS alone: domain drains, transmits, and throttles run between the claim transaction and the discharge transaction, never inside one, because a wire send inside the pinned transaction holds the claim set's locks across network time.
- Law: `degrade` is stated, not implied — a lease proves single ACTIVE delivery and never single delivery, so a drain whose effect is not idempotent double-runs on lease lapse and the payload's own dedup projection is the only cure.
- Law: the pass discharges ONCE — every terminal verdict yields its claim id and the pass issues one `Journal.complete` roster write, so a `take`-sized batch closes in one statement exactly as the claim that opened it read in one; a per-claim mark inside the fold pays a round trip per row to spell a set the statement already takes.
- Law: a drain's `never` channel leaves the defect as its one remaining failure, so this seam is where the poison list's `defect` row is PRODUCED — the admitted drain folds through `Effect.catchAllDefect` into a `defect`-classed park carrying the residue, because an escaped defect kills the pass and strands every peer claim in the batch on a lease nothing will re-drive until it lapses. Interrupts pass through untouched: a shutdown is not a verdict.
- Law: wake is the journal's NOTIFY pulse — the drain sleeps on the data wave's wake stream and claims on pulse or lease-width tick, whichever fires; a tight poll loop is the rejected form.
- Growth: a new lane dimension (deliver-at scheduling, a channel filter) is a deliverable column with a claim predicate on the data statement; a new drain family is one `Lane.row` handed to the route — the verdict fold never widens.
- Packages: `@rasm/ts/data` (`Journal`, `Tenancy`); `@effect/sql` (`SqlClient`, `SqlError`); `effect` (`Match`, `Effect`, `Option`, `Schema`); `./entity.ts` (`WorkClass`).

```typescript signature
type LaneVerdict = Data.TaggedEnum<{
  Settled: {}
  Deferred: { readonly class: Fault.Class.Kind }
  Parked: { readonly class: Fault.Class.Kind; readonly detail: string }
}>
const LaneVerdict = Data.taggedEnum<LaneVerdict>()

declare namespace Lane {
  // Claim statements answer this floor, never the whole row: a data-owned claim widens it with its own columns
  // and `Row` carries them through admission, so no drain re-joins its own batch by identity to recover one.
  type Claim = {
    readonly id: bigint
    readonly sequence: bigint
    readonly tag: string
    readonly payload: unknown
    readonly attempts: number
  }
  type Meta<Row extends Claim = Claim> = Omit<Row, "payload">
  type Admit<R, Row extends Claim = Claim> = (claim: Row) => Effect.Effect<LaneVerdict, never, R>
}

const _judge = (meta: Lane.Meta, clazz: WorkClass.Kind, fault: { readonly class: Fault.Class.Kind; readonly detail: string }): LaneVerdict =>
  Fault.Class.at(fault.class).retryable && meta.attempts < WorkClass[clazz].attempts
    ? LaneVerdict.Deferred({ class: fault.class })
    : LaneVerdict.Parked({ class: fault.class, detail: fault.detail })

const _row = <A, I, R, Row extends Lane.Claim = Lane.Claim>(
  payload: Schema.Schema<A, I>,
  drain: (payload: A, meta: Lane.Meta<Row>) => Effect.Effect<LaneVerdict, never, R>,
): Lane.Admit<R, Row> =>
(claim) =>
  Schema.decodeUnknown(payload)(claim.payload).pipe(
    Effect.matchEffect({
      onFailure: (fault) => Effect.succeed(LaneVerdict.Parked({ class: "invalid", detail: `<${claim.tag}:${fault.message}>` })),
      // Claims cross whole as `meta`: `Omit` hides the raw column at the type while every other coordinate the
      // data-owned row decoded travels intact, so a projection reads its own claim rather than a keyed re-join.
      onSuccess: (value) =>
        drain(value, claim).pipe(
          // drains carry a `never` channel, so their ONLY remaining failure is a defect, and this is the producer that
          // gives the poison list its `defect` row: uncaught it would kill the pass and strand every peer claim on a
          // lease. Interrupts pass through untouched — a shutdown is not a poison verdict.
          Effect.catchAllDefect((residue) =>
            Effect.succeed(LaneVerdict.Parked({ class: "defect", detail: `<${claim.tag}:${String(residue)}>` }))
          ),
        ),
    }),
  )

// Claims discharge the outbox row on both terminal verdicts and on neither transient one; `Deferred` writes nothing
// at all, because the lease IS the backoff and an un-claim write would race the claimant the lease predicate protects.
const _landed = <R2, Row extends Lane.Claim>(
  park: (claim: Row, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) => Effect.Effect<void, never, R2>,
  claim: Row,
  verdict: LaneVerdict,
): Effect.Effect<Option.Option<bigint>, never, R2> =>
  Match.value(verdict).pipe(
    Match.tag("Settled", () => Effect.succeedSome(claim.id)),
    Match.tag("Deferred", () => Effect.succeedNone),
    Match.tag("Parked", (parked) => Effect.as(park(claim, parked), Option.some(claim.id))),
    Match.exhaustive,
  )

const _settle = <R, R2, Row extends Lane.Claim = Lane.Claim>(
  sql: SqlClient.SqlClient,
  clazz: WorkClass.Kind,
  route: (tag: string) => Option.Option<Lane.Admit<R, Row>>,
  park: (claim: Row, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) => Effect.Effect<void, never, R2>,
) =>
(claims: ReadonlyArray<Row>): Effect.Effect<ReadonlyArray<LaneVerdict>, SqlError.SqlError, R | R2> =>
  Effect.gen(function* () {
    const judged = yield* Effect.forEach(
      claims,
      (claim) =>
        Option.match(route(claim.tag), {
          onNone: () => Effect.succeed(LaneVerdict.Parked({ class: "malformed", detail: `<unrouted:${claim.tag}>` })),
          onSome: (admit) => admit(claim),
        }).pipe(Effect.flatMap((verdict) => Effect.map(_landed(park, claim, verdict), (id) => [verdict, id] as const))),
      { concurrency: WorkClass[clazz].concurrency },
    )
    // ONE discharge statement per pass: `Journal.complete` is a roster write fed by a batched claim read, so a
    // per-claim mark pays `take` round trips to close one claim set the statement was shaped to close in one. Its
    // `SqlError` leaves on the error channel — a store fault is no claim's verdict, and every judged claim rides its
    // unexpired lease into the next pass. The discharge marks rows across every tenant of the app, so it rides the
    // maintenance-plane transformer: unpinned it updates zero rows under FORCE RLS and every settled claim redelivers.
    const discharged = Array.getSomes(Array.map(judged, ([, id]) => id))
    yield* Array.isNonEmptyReadonlyArray(discharged) ? Tenancy.sweep(sql)(Journal.complete(sql, discharged)) : Effect.void
    return Array.map(judged, ([verdict]) => verdict)
  })
```

## [05]-[PARK_REPLAY]

- Owner: the dead-letter fold — a `Parked` verdict appends one typed evidence row through the data wave's fact rail (`Fact.record`): the deliverable's identity as the target, the dominant fault class and attempt count as `Change` rows, `operational` retention — so the dead set is queryable history on the record of truth, never a second table. `Lane.replay` is the operator entry: it folds a parked-evidence read (an audit projection the caller supplies) through the drain's own `remit` re-entry with attempts reset, and records the replay fact — replay is itself evidence.
- Law: poison short-circuits — a non-retryable class (`invalid`, `malformed`, `denied`, `breached`, `defect`) parks on first failure regardless of the ceiling, because redelivering a deterministic failure spends lease windows to learn nothing; the judge fold above encodes this by reading the class table's `retryable` column, and a page-local poison list is unspellable.
- Law: parking is terminal for the claim, never for the work — the outbox row completes so the drain set stays bounded; the evidence row is the work's continued existence, and replay is the one path back.
- Law: the DLQ read is maintenance-plane material — the park evidence rows carry no tenant (the target is the deliverable, so the fact stores NULL tenant, visible only under the plane posture), so the parked-evidence projection a caller hands `Lane.replay` composes `Tenancy.sweep` around its read; an unpinned projection reads an empty dead set and a replay pass reports nothing to re-offer while parked work ages silently. `Fact.record` composes no bracket for the park WRITE — it is a buffered offer whose drain owns its own plane posture at the data seam.
- Receipt: the park evidence row carries `{ tag, deliverable, sequence, class, attempts, detail }` — the shape operator tooling lists, counts by class, and feeds back into `replay` — and the same fold marks the `Pulse` DLQ counter tagged by the claim's stream-prefix channel, so the OTel series and the dead-set history cannot disagree.
- Growth: a replay posture (selective by class, dry-run census) is a predicate parameter on the one `replay` fold; a park-notification hook is a tap on the audit stream at its consumer, never a callback here.
- Packages: `@rasm/ts/data` (`AuditFact`, `Fact`, `Journal`); `effect` (`Effect`, `Stream`); `../otel/meter.ts` (`Pulse`).

```typescript signature
const _channel = (tag: string): string => tag.split(":", 1)[0] ?? tag

const _park = (claim: Lane.Claim, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) =>
  Effect.zipRight(
    Pulse.mark("parked", _channel(claim.tag)),
    Fact.record({
      action: "deliverable.parked",
      actor: { key: "lane", kind: "service" },
      change: [
        { _tag: "Assigned", path: "/sequence", next: String(claim.sequence) },
        { _tag: "Assigned", path: "/class", next: verdict.class },
        { _tag: "Assigned", path: "/attempts", next: String(claim.attempts) },
        { _tag: "Assigned", path: "/detail", next: verdict.detail },
      ],
      retention: "operational",
      target: { key: String(claim.id), kind: "deliverable", parent: claim.tag },
    }),
  )

const _replay = <R, R2>(options: {
  readonly parked: Stream.Stream<AuditFact, never, R>
  readonly admit: (evidence: AuditFact) => boolean
  readonly remit: (evidence: AuditFact) => Effect.Effect<void, never, R2>
}) =>
  options.parked.pipe(
    Stream.filter(options.admit),
    Stream.mapEffect(options.remit),
    Stream.runCount,
    Effect.tap((count) =>
      Fact.record({
        action: "deliverable.replayed",
        actor: { key: "operator", kind: "user" },
        change: [{ _tag: "Assigned", path: "/count", next: String(count) }],
        retention: "operational",
        target: { key: "replay", kind: "deliverable" },
      })
    ),
  )

const Lane = {
  row: _row,
  judge: _judge,
  settle: _settle,
  park: _park,
  replay: _replay,
  ceiling: (clazz: WorkClass.Kind) => WorkClass[clazz].attempts,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Job, Lane, LaneVerdict, Throttle }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
