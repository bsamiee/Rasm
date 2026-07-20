# [RUNTIME_QUEUE]

Durable-work intake: restart-surviving job families on the native `DurableQueue`, durable keyed throttles on `DurableRateLimiter`, and the pg-composed lane policy over the data wave's outbox statements — claim admission, claim lease, urgency order, park ceiling, and operator replay as one verdict vocabulary spelled ONCE for every drain in the branch. Service-class pricing arrives settled from `entity#WORK_CLASS`.

Decomposition is ruled: `@effect/cluster` and `@effect/workflow` natively own persistence, dedup, and worker execution; the ordering-and-parking layer is pg-composed — the journal's `SKIP LOCKED` claim with an `ORDER BY` urgency term carries the queue engines' visibility-timeout and archive semantics as lease and park columns, the engines rejected as a second job-table paradigm — and no third layer exists. Dead-lettering lives here alone: a parked deliverable is typed evidence on the fact journal, replay an operator fold re-minting deliverables from that evidence. Its module ships on the `./server` subpath as `runtime/src/work/queue.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                        | [PUBLIC]   |
| :-----: | :------------ | :---------------------------------------------------------------------------- | :--------- |
|  [01]   | `JOB_FAMILY`  | the persisted job declaration law, dedup projection, class-priced workers     | `Job`      |
|  [02]   | `THROTTLE`    | durable keyed quotas — algorithm rows, tenant keys, cost weights              | `Throttle` |
|  [03]   | `LANE_POLICY` | claim lease, urgency order, batch geometry, claim admission, the verdict fold | `Lane`     |
|  [04]   | `PARK_REPLAY` | dead-letter evidence, the park ceiling, poison short-circuit, operator replay | `Lane`     |

## [02]-[JOB_FAMILY]

[JOB_FAMILY]:
- Owner: `Job` — the persisted job family law: `DurableQueue.make({ name, payload, idempotencyKey, success, error })` declares the family with Schema-typed payload, result, fault, and a pure dedup projection; `DurableQueue.process(queue, payload, { retrySchedule })` enqueues and suspends the caller until a worker settles the item, answering the family's declared `success` value; and `DurableQueue.worker(queue, handle, { concurrency })` is the consuming Layer whose parallelism and retry geometry are the `WorkClass` row's columns — `concurrency` from the class, `retrySchedule` from `Budget.schedule(row.budget)` — so a job family is priced by naming a class, never by carrying knobs.
- Law: dedup identity is the payload projection — `idempotencyKey` derives from payload content exactly as `flow#FLOW_LAW`'s `executionId` does, so a re-enqueued equal payload joins the in-flight item instead of duplicating work; a caller-minted job id is the rejected form.
- Law: a job body is `Step.run` material — the worker's handle composes the flow mint for its deadline geometry, so queue workers and workflow activities carry identical budget shapes and evidence; the family's declared `success` threads the handle's result through the step's persisted exit to the suspended `submit` caller, and the declared `error` unions the spec's fault schema with `StepFault` so a budget trip persists beside domain failure under one wire family — a family whose result is fire-and-forget declares `Schema.Void` as its `success` row, never a second void-only mint.
- Law: fire-and-forget is a modality of the same family — a scoped caller may supervise `process` with `Effect.forkScoped`, while a request that must acknowledge durable admission keeps awaiting the declared success; an unscoped daemon fiber or a second "unawaited" queue declaration is unspellable.
- Growth: a new job kind is one `DurableQueue.make` value with one worker Layer row at the composition root; a family outgrowing single-item settlement into multi-step orchestration promotes to a `flow` definition, re-homing the payload schema unchanged.
- Boundary: the queue's persistence rides the engine's `MessageStorage` from `entity#MAILBOX`; no queue table, poll loop, or storage row exists on this page.
- Packages: `@effect/workflow` (`DurableQueue`); `effect` (`Effect`, `Schema`); `@rasm/ts/core` (`Budget`); `./entity.ts` (`WorkClass`).

```typescript
import { DurableQueue, DurableRateLimiter } from "@effect/workflow"
import type { SqlClient } from "@effect/sql"
import { Data, Duration, Effect, Match, Option, Schema, Stream } from "effect"
import { type AuditFact, Fact, Journal } from "@rasm/ts/data"
import { Budget, FaultClass } from "@rasm/ts/core"
import { Pulse } from "../otel/meter.ts"
import { WorkClass } from "./entity.ts"
import { Step, StepFault } from "./flow.ts"

declare namespace Job {
  type Spec<Name extends string, A, I, S, SI, E extends { readonly class: FaultClass.Kind }, EI> = {
    readonly name: Name
    readonly payload: Schema.Schema<A, I>
    readonly success: Schema.Schema<S, SI>
    readonly error: Schema.Schema<E, EI>
    readonly clazz: WorkClass.Kind
    readonly key: (payload: A) => string
  }
}

const _job = <Name extends string, A, I, S, SI, E extends { readonly class: FaultClass.Kind }, EI>(
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
    submit: (payload: A) => DurableQueue.process(queue, payload, { retrySchedule: Budget.schedule(row.budget) }),
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

[THROTTLE]:
- Owner: `Throttle` — durable keyed quotas: `DurableRateLimiter.rateLimit({ name, algorithm, window, limit, key, tokens })` runs as an activity whose consumption survives replay, so a retried step never double-spends its quota. Each generic row carries its scope, algorithm, window, limit, compound-key projection, and cost projection; `Throttle.spend(row, subject)` is the one entry, so consumers cannot pass a key or token count inconsistent with the selected quota.
- Law: cost is a parameter — a heavyweight item spends `tokens > 1` against the same row; a parallel "heavy" quota row for the same scope is the rejected form.
- Law: exhaustion delays, never refuses — the durable limiter's exceeded posture is a `DurableClock` sleep sized to the window turn, so a step that overdraws its quota parks durably and resumes past process death with the spend already consumed; the fault channel carries only `RateLimitStoreError` (`_tag: "RateLimiterError"` — the quota STORE failed), which classifies `unavailable` so a lane judge defers it on the lease. A hand-written wait-for-window loop, or a page modeling exhaustion as a refusal fault, contradicts the shipped posture and is unspellable.
- Law: process-plane admission pressure (request shedding, connection caps) is the serving gate's concern; a `Throttle` row prices durable work, and one concern appearing in both tables is the split the row's `scope` name makes visible at review.
- Growth: a new quota is one table row; a new pacing shape is an `algorithm` value the shipped surface names.
- Packages: `@effect/workflow` (`DurableRateLimiter`); `effect` (`Duration`).

```typescript
declare namespace Throttle {
  type Row<A> = {
    readonly scope: string
    readonly algorithm: "fixed-window" | "token-bucket"
    readonly window: Duration.DurationInput
    readonly limit: number
    readonly key: (subject: A) => string
    readonly tokens: (subject: A) => number
  }
}

const _rows = {
  tenantEgress: {
    scope: "tenant-egress", algorithm: "token-bucket", window: Duration.minutes(1), limit: 600,
    key: (subject: { readonly tenant: string; readonly channel: string; readonly weight: number }) => `${subject.tenant}:${subject.channel}`,
    tokens: (subject: { readonly tenant: string; readonly channel: string; readonly weight: number }) => subject.weight,
  },
  providerCall: {
    scope: "provider-call", algorithm: "fixed-window", window: Duration.minutes(1), limit: 240,
    key: (subject: { readonly tenant: string; readonly provider: string; readonly weight: number }) => `${subject.tenant}:${subject.provider}`,
    tokens: (subject: { readonly tenant: string; readonly provider: string; readonly weight: number }) => subject.weight,
  },
  reportRender: {
    scope: "report-render", algorithm: "token-bucket", window: Duration.minutes(5), limit: 50,
    key: (subject: { readonly tenant: string; readonly format: string; readonly weight: number }) => `${subject.tenant}:${subject.format}`,
    tokens: (subject: { readonly tenant: string; readonly format: string; readonly weight: number }) => subject.weight,
  },
} as const

const _spend = <A>(row: Throttle.Row<A>, subject: A) =>
  DurableRateLimiter.rateLimit({
    name: row.scope,
    algorithm: row.algorithm,
    window: row.window,
    limit: row.limit,
    key: row.key(subject),
    tokens: row.tokens(subject),
  })

const Throttle = { ..._rows, spend: _spend }
```

## [04]-[LANE_POLICY]

[LANE_POLICY]:
- Owner: `Lane` — the drain policy over the data journal's outbox statements. Data's wave owns the relation and the two statements (`Journal.claimBatch(sql, { app, take, leaseSeconds })` — `FOR UPDATE SKIP LOCKED` with attempt increment, `Journal.complete(sql, ids)` — the delivered mark); this page owns what a drain DOES with them: the lease width (`leaseSeconds` derived from the class row's per-attempt budget — the visibility-timeout semantic mined from the external queue engines, expressed as the claim statement's own re-claim predicate), the urgency term (the `ORDER BY` column populated from `WorkClass[clazz].urgency` at enqueue so interactive deliverables pass bulk ones under contention), the batch geometry (`take` sized by the drain's class row), the claim admission seam, and the verdict fold.
- Law: admission happens at the lane seam, exactly once — `Lane.row(payload, drain)` is the one admission mint: it fuses a payload `Schema` with a domain drain so the data-owned raw claim `payload` decodes before any domain code runs, a decode failure folds to an `invalid`-classed park through the poison short-circuit, and the drain receives the admitted payload with the claim meta (`id`, `sequence`, `tag`, `attempts`) — a raw `payload: unknown` reaching a domain drain, or a drain-local decoder, is the consumer-local-admission defect this mint forecloses, and payload shape authority is always recoverable from the row that routed the tag.
- Law: the verdict vocabulary is closed — `Settled` (the effect landed: `Journal.complete`), `Deferred` (transient fault: the row stays claimed and the lease expiry re-delivers it, attempts already incremented), `Parked` (the ceiling, a non-retryable class, a failed admission, or an unrouted stream: `[5]`'s evidence fold) — and every drain in the branch folds claims through `Lane.settle`, so retry-with-redelivery is spelled once and a drain-local retry loop is unspellable. `Lane.settle` answers the batch's verdict roster, so a relay meters its pass from the returned values instead of a second count.
- Law: defer is passive — no un-claim write, no backoff column; the lease IS the backoff, and its width is the class row's per-attempt budget, so redelivery pacing derives from the same geometry as in-process retry.
- Law: wake is the journal's NOTIFY pulse — the drain sleeps on the data wave's wake stream and claims on pulse or lease-width tick, whichever fires; a tight poll loop is the rejected form.
- Growth: a new lane dimension (deliver-at scheduling, a channel filter) is a deliverable column with a claim predicate on the data statement; a new drain family is one `Lane.row` handed to the route — the verdict fold never widens.
- Packages: `@rasm/ts/data` (`Journal`); `effect` (`Match`, `Effect`, `Option`, `Schema`); `./entity.ts` (`WorkClass`).

```typescript
type LaneVerdict = Data.TaggedEnum<{
  Settled: {}
  Deferred: { readonly class: FaultClass.Kind }
  Parked: { readonly class: FaultClass.Kind; readonly detail: string }
}>
const LaneVerdict = Data.taggedEnum<LaneVerdict>()

declare namespace Lane {
  type Claim = {
    readonly id: bigint
    readonly sequence: bigint
    readonly tag: string
    readonly payload: unknown
    readonly attempts: number
  }
  type Meta = Omit<Claim, "payload">
  type Admit<R> = (claim: Claim) => Effect.Effect<LaneVerdict, never, R>
}

const _judge = (meta: Lane.Meta, clazz: WorkClass.Kind, fault: { readonly class: FaultClass.Kind; readonly detail: string }): LaneVerdict =>
  FaultClass[fault.class].retryable && meta.attempts < WorkClass[clazz].attempts
    ? LaneVerdict.Deferred({ class: fault.class })
    : LaneVerdict.Parked({ class: fault.class, detail: fault.detail })

const _row = <A, I, R>(
  payload: Schema.Schema<A, I>,
  drain: (payload: A, meta: Lane.Meta) => Effect.Effect<LaneVerdict, never, R>,
): Lane.Admit<R> =>
(claim) =>
  Schema.decodeUnknown(payload)(claim.payload).pipe(
    Effect.matchEffect({
      onFailure: (fault) => Effect.succeed(LaneVerdict.Parked({ class: "invalid", detail: `<${claim.tag}:${fault.message}>` })),
      onSuccess: (value) => drain(value, {
        id: claim.id,
        sequence: claim.sequence,
        tag: claim.tag,
        attempts: claim.attempts,
      }),
    }),
  )

const _settle = <R, R2>(
  sql: SqlClient.SqlClient,
  clazz: WorkClass.Kind,
  route: (tag: string) => Option.Option<Lane.Admit<R>>,
  park: (claim: Lane.Claim, verdict: Extract<LaneVerdict, { readonly _tag: "Parked" }>) => Effect.Effect<void, never, R2>,
) =>
(claims: ReadonlyArray<Lane.Claim>): Effect.Effect<ReadonlyArray<LaneVerdict>, never, R | R2> => {
  const landed = (claim: Lane.Claim) => (verdict: LaneVerdict) =>
    Match.value(verdict).pipe(
      Match.tag("Settled", () => Journal.complete(sql, [claim.id])),
      Match.tag("Deferred", () => Effect.void),
      Match.tag("Parked", (parked) => park(claim, parked).pipe(Effect.zipRight(Journal.complete(sql, [claim.id])))),
      Match.exhaustive,
    ).pipe(Effect.as(verdict))
  return Effect.forEach(claims, (claim) =>
    Option.match(route(claim.tag), {
      onNone: () => landed(claim)(LaneVerdict.Parked({ class: "malformed", detail: `<unrouted:${claim.tag}>` })),
      onSome: (admit) => Effect.flatMap(admit(claim), landed(claim)),
    }), { concurrency: WorkClass[clazz].concurrency })
}
```

## [05]-[PARK_REPLAY]

[PARK_REPLAY]:
- Owner: the dead-letter fold — a `Parked` verdict appends one typed evidence row through the data wave's fact rail (`Fact.record`): the deliverable's identity as the target, the dominant fault class and attempt count as `Change` rows, `operational` retention — so the dead set is queryable history on the record of truth, never a second table. `Lane.replay` is the operator entry: it folds a parked-evidence read (an audit projection the caller supplies) through the drain's own `remit` re-entry with attempts reset, and records the replay fact — replay is itself evidence.
- Law: poison short-circuits — a non-retryable class (`invalid`, `malformed`, `denied`, `breached`, `defect`) parks on first failure regardless of the ceiling, because redelivering a deterministic failure spends lease windows to learn nothing; the judge fold above encodes this by reading the class table's `retryable` column, and a page-local poison list is unspellable.
- Law: parking is terminal for the claim, never for the work — the outbox row completes so the drain set stays bounded; the evidence row is the work's continued existence, and replay is the one path back.
- Receipt: the park evidence row carries `{ tag, deliverable, sequence, class, attempts, detail }` — the shape operator tooling lists, counts by class, and feeds back into `replay` — and the same fold marks the `Pulse` DLQ counter tagged by the claim's stream-prefix channel, so the OTel series and the dead-set history cannot disagree.
- Growth: a replay posture (selective by class, dry-run census) is a predicate parameter on the one `replay` fold; a park-notification hook is a tap on the audit stream at its consumer, never a callback here.
- Packages: `@rasm/ts/data` (`AuditFact`, `Fact`, `Journal`); `effect` (`Effect`, `Stream`); `../otel/meter.ts` (`Pulse`).

```typescript
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
