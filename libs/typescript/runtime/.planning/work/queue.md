# [RUNTIME_QUEUE]

The durable-work intake: restart-surviving job families on the native `DurableQueue`, durable keyed throttles on `DurableRateLimiter`, and the pg-composed lane policy over the data wave's outbox statements — claim lease, urgency order, park ceiling, and operator replay as one verdict vocabulary spelled ONCE for every drain in the branch. The decomposition is ruled: the engine layer is native (`@effect/cluster` + `@effect/workflow` own persistence, dedup, and worker execution), the ordering-and-parking layer is pg-composed (the journal's `SKIP LOCKED` claim with an `ORDER BY` urgency term — the visibility-timeout and archive semantics of the external queue engines mined as lease and park columns, the engines themselves rejected as a second job-table paradigm), and no third layer exists. Dead-lettering lives here and only here: a parked deliverable is typed evidence on the fact journal, replay is an operator fold that re-mints deliverables from that evidence, and a sibling page that re-spells park-or-replay is the named twice-owned defect. Service-class pricing arrives settled from `entity#WORK_CLASS`. The module ships on the `./server` exports subpath as `runtime/src/work/queue.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                        | [PUBLIC]   |
| :-----: | :------------- | :------------------------------------------------------------------------------ | :--------- |
|  [01]   | `JOB_FAMILY`   | the persisted job declaration law, dedup projection, class-priced workers        | `Job`      |
|  [02]   | `THROTTLE`     | durable keyed quotas — algorithm rows, tenant keys, cost weights                 | `Throttle` |
|  [03]   | `LANE_POLICY`  | claim lease, urgency order, batch geometry, the verdict fold over the outbox     | `Lane`     |
|  [04]   | `PARK_REPLAY`  | dead-letter evidence, the park ceiling, poison short-circuit, operator replay    | `Lane`     |

## [2]-[JOB_FAMILY]

[JOB_FAMILY]:
- Owner: `Job` — the persisted job family law: `DurableQueue.make({ name, payload, idempotencyKey, success, error })` declares the family with Schema-typed payload and a pure dedup projection, `DurableQueue.process(queue, payload, { retrySchedule })` enqueues and suspends the caller until a worker settles the item, and `DurableQueue.worker(queue, handle, { concurrency })` is the consuming Layer whose parallelism and retry geometry are the `WorkClass` row's columns — `concurrency` from the class, `retrySchedule` from `Budget.schedule(row.budget)` — so a job family is priced by naming a class, never by carrying knobs.
- Law: dedup identity is the payload projection — `idempotencyKey` derives from payload content exactly as `flow#FLOW_LAW`'s `executionId` does, so a re-enqueued equal payload joins the in-flight item instead of duplicating work; a caller-minted job id is the rejected form.
- Law: a job body is `Step.run` material — the worker's handle composes the flow mint for its deadline geometry, so queue workers and workflow activities carry identical budget shapes and evidence.
- Law: fire-and-forget is a modality of the same family — a caller that needs no result races `process` with `Effect.forkDaemon` at its own seam; a second "unawaited" queue declaration is unspellable.
- Growth: a new job kind is one `DurableQueue.make` value plus one worker Layer row at the composition root; a family outgrowing single-item settlement into multi-step orchestration promotes to a `flow` definition, re-homing the payload schema unchanged.
- Boundary: the queue's persistence rides the engine's `MessageStorage` from `entity#MAILBOX`; no queue table, poll loop, or storage row exists on this page.
- Packages: `@effect/workflow` (`DurableQueue`); `effect` (`Effect`, `Schema`); `@rasm/ts/core` (`Budget`); `./entity.ts` (`WorkClass`).

```typescript
import { DurableQueue, DurableRateLimiter } from "@effect/workflow"
import type { SqlClient } from "@effect/sql"
import { Data, Duration, Effect, Match, Schema, Stream } from "effect"
import { type AuditFact, Fact, Journal } from "@rasm/ts/data"
import { Budget, FaultClass } from "@rasm/ts/core"
import { WorkClass } from "./entity.ts"
import { Step } from "./flow.ts"

declare namespace Job {
  type Spec<Name extends string, A, I> = {
    readonly name: Name
    readonly payload: Schema.Schema<A, I>
    readonly clazz: WorkClass.Kind
    readonly key: (payload: A) => string
  }
}

const _job = <Name extends string, A, I>(spec: Job.Spec<Name, A, I>) => {
  const row = WorkClass[spec.clazz]
  const queue = DurableQueue.make({ name: spec.name, payload: spec.payload, idempotencyKey: spec.key })
  return {
    queue,
    submit: (payload: A) => DurableQueue.process(queue, payload, { retrySchedule: Budget.schedule(row.budget) }),
    worker: <E extends { readonly class: FaultClass.Kind }, R>(
      handle: (payload: A) => Effect.Effect<unknown, E, R>,
    ) =>
      DurableQueue.worker(queue, (payload: A) => Step.run(spec.name, spec.clazz, handle(payload)), {
        concurrency: row.concurrency,
      }),
  } as const
}

const Job = { of: _job }
```

## [3]-[THROTTLE]

[THROTTLE]:
- Owner: `Throttle` — durable keyed quotas: `DurableRateLimiter.rateLimit({ name, algorithm, window, limit, key, tokens })` runs as an activity whose consumption survives replay, so a retried step never double-spends its quota. The quota table is data: each row names its scope (`tenant-egress`, `provider-call`, `report-render`), its algorithm as a policy value (`fixed-window` for hard caps, `token-bucket` for burst-tolerant pacing), its window, its limit, and its key projection — a tenant key, a provider name, a channel — and `Throttle.spend(row, key, tokens)` is the one entry every consumer composes inside a step body.
- Law: cost is a parameter — a heavyweight item spends `tokens > 1` against the same row; a parallel "heavy" quota row for the same scope is the rejected form.
- Law: quota refusal classifies `exhausted` — the caller's budget gate reads `retryable: true` and re-drives under its own schedule after the window turns; a hand-written wait-for-window loop beside the limiter is unspellable.
- Law: process-plane admission pressure (request shedding, connection caps) is the serving gate's concern; a `Throttle` row prices durable work, and one concern appearing in both tables is the split the row's `scope` name makes visible at review.
- Growth: a new quota is one table row; a new pacing shape is an `algorithm` value the shipped surface names.
- Packages: `@effect/workflow` (`DurableRateLimiter`); `effect` (`Duration`).

```typescript
declare namespace Throttle {
  type Row = {
    readonly scope: string
    readonly algorithm: "fixed-window" | "token-bucket"
    readonly window: Duration.DurationInput
    readonly limit: number
  }
}

const _rows = {
  tenantEgress: { scope: "tenant-egress", algorithm: "token-bucket", window: Duration.minutes(1), limit: 600 },
  providerCall: { scope: "provider-call", algorithm: "fixed-window", window: Duration.minutes(1), limit: 240 },
  reportRender: { scope: "report-render", algorithm: "token-bucket", window: Duration.minutes(5), limit: 50 },
} as const satisfies { readonly [name: string]: Throttle.Row }

const _spend = (row: Throttle.Row, key: string, tokens = 1) =>
  DurableRateLimiter.rateLimit({
    name: row.scope,
    algorithm: row.algorithm,
    window: row.window,
    limit: row.limit,
    key,
    tokens,
  })

const Throttle = { ..._rows, spend: _spend }
```

## [4]-[LANE_POLICY]

[LANE_POLICY]:
- Owner: `Lane` — the drain policy over the data journal's outbox statements. The data wave owns the relation and the two statements (`Journal.claimBatch(sql, app, take, leaseSeconds)` — `FOR UPDATE SKIP LOCKED` with attempt increment, `Journal.complete(sql, ids)` — the delivered mark); this page owns what a drain DOES with them: the lease width (`leaseSeconds` derived from the class row's per-attempt budget — the visibility-timeout semantic mined from the external queue engines, expressed as the claim statement's own re-claim predicate), the urgency term (the `ORDER BY` column populated from `WorkClass[clazz].urgency` at enqueue so interactive deliverables pass bulk ones under contention), the batch geometry (`take` sized by the drain's class row), and the verdict fold.
- Law: the verdict vocabulary is closed — `Settled` (the effect landed: `Journal.complete`), `Deferred` (transient fault: the row stays claimed and the lease expiry re-delivers it, attempts already incremented), `Parked` (the ceiling or a non-retryable class: `[5]`'s evidence fold) — and every drain in the branch folds claims through `Lane.settle`, so retry-with-redelivery is spelled once and a drain-local retry loop is unspellable.
- Law: defer is passive — no un-claim write, no backoff column; the lease IS the backoff, and its width is the class row's per-attempt budget, so redelivery pacing derives from the same geometry as in-process retry.
- Law: wake is the journal's NOTIFY pulse — the drain sleeps on the data wave's wake stream and claims on pulse or lease-width tick, whichever fires; a tight poll loop is the rejected form.
- Growth: a new lane dimension (deliver-at scheduling, a channel filter) is a deliverable column plus a claim predicate on the data statement — the verdict fold never widens.
- Packages: `@rasm/ts/data` (`Journal`); `effect` (`Match`, `Effect`, `DateTime`); `./entity.ts` (`WorkClass`).

```typescript
type LaneVerdict = Data.TaggedEnum<{
  Settled: {}
  Deferred: { readonly class: FaultClass.Kind }
  Parked: { readonly class: FaultClass.Kind; readonly detail: string }
}>
const LaneVerdict = Data.taggedEnum<LaneVerdict>()

declare namespace Lane {
  type Claim = {
    readonly id: number
    readonly stream: string
    readonly body: unknown
    readonly attempts: number
  }
  type Drain<R> = (claim: Claim) => Effect.Effect<LaneVerdict, never, R>
}

const _ceiling = 8

const _judge = (claim: Lane.Claim, fault: { readonly class: FaultClass.Kind; readonly detail: string }): LaneVerdict =>
  FaultClass[fault.class].retryable && claim.attempts < _ceiling
    ? LaneVerdict.Deferred({ class: fault.class })
    : LaneVerdict.Parked({ class: fault.class, detail: fault.detail })

const _settle = <R, R2>(
  sql: SqlClient.SqlClient,
  clazz: WorkClass.Kind,
  drain: Lane.Drain<R>,
  park: (claim: Lane.Claim, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) => Effect.Effect<void, never, R2>,
) =>
(claims: ReadonlyArray<Lane.Claim>) =>
  Effect.forEach(claims, (claim) =>
    drain(claim).pipe(
      Effect.flatMap((verdict) =>
        Match.value(verdict).pipe(
          Match.tag("Settled", () => Journal.complete(sql, [claim.id])),
          Match.tag("Deferred", () => Effect.void),
          Match.tag("Parked", (parked) => park(claim, parked).pipe(Effect.zipRight(Journal.complete(sql, [claim.id])))),
          Match.exhaustive,
        )
      ),
    ), { concurrency: WorkClass[clazz].concurrency })
```

## [5]-[PARK_REPLAY]

[PARK_REPLAY]:
- Owner: the dead-letter fold — a `Parked` verdict appends one typed evidence row through the data wave's fact rail (`Fact.record`): the deliverable's identity as the target, the dominant fault class and attempt count as `Change` rows, `operational` retention — so the dead set is queryable history on the record of truth, never a second table. `Lane.replay` is the operator entry: it folds a parked-evidence read (an audit projection the caller supplies) through the drain's own `remit` re-entry with attempts reset, and records the replay fact — replay is itself evidence.
- Law: poison short-circuits — a non-retryable class (`invalid`, `malformed`, `denied`, `breached`, `defect`) parks on first failure regardless of the ceiling, because redelivering a deterministic failure spends lease windows to learn nothing; the judge fold above encodes this by reading the class table's `retryable` column, and a page-local poison list is unspellable.
- Law: parking is terminal for the claim, never for the work — the outbox row completes so the drain set stays bounded; the evidence row is the work's continued existence, and replay is the one path back.
- Receipt: the park evidence row carries `{ stream, deliverable, class, attempts, detail }` — the shape operator tooling lists, counts by class, and feeds back into `replay`.
- Growth: a replay posture (selective by class, dry-run census) is a predicate parameter on the one `replay` fold; a park-notification hook is a tap on the audit stream at its consumer, never a callback here.
- Packages: `@rasm/ts/data` (`AuditFact`, `Fact`, `Journal`); `effect` (`Effect`, `Stream`).

```typescript
const _park = (claim: Lane.Claim, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) =>
  Fact.record({
    action: "deliverable.parked",
    actor: { key: "lane", kind: "service" },
    change: [
      { _tag: "Assigned", path: "/class", next: verdict.class },
      { _tag: "Assigned", path: "/attempts", next: String(claim.attempts) },
      { _tag: "Assigned", path: "/detail", next: verdict.detail },
    ],
    retention: "operational",
    target: { key: String(claim.id), kind: "deliverable", parent: claim.stream },
  })

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
  judge: _judge,
  settle: _settle,
  park: _park,
  replay: _replay,
  ceiling: _ceiling,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Job, Lane, LaneVerdict, Throttle }
```
