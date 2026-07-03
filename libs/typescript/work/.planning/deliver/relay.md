# [WORK_RELAY]

The outbox relay is one cluster singleton draining one claim loop: rows the store's transactional outbox committed atomically with their journal append are claimed in batches under `FOR UPDATE SKIP LOCKED` through the composed `SqlClient` port, dispatched through the closed channel record — `webhook | mail | report` — under one fan-out policy row, and settled by verdict: delete on delivery, defer with exponential backoff while the fault classifies transient, park as reviewable evidence once terminal or spent. Delayed delivery is a claim predicate — a row's `deliver_at` in the future is simply not yet claimable, the same instant-based contract the cluster's `DeliverAt` interface applies to entity messages — and per-tenant egress quota is the store-backed distributed limiter keyed by the row's tenant scope, so one tenant's burst delays its own lane and never a sibling's. Wakes are a port: the drain always polls at its cadence, and the store's LISTEN lane accelerates it through the `Wake` Tag when provided. A second drain process, a hand poll loop beside the singleton, an unquotaed fan-out, and a dropped row are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                       |
| :-----: | :------------- | :-------------------------------------------------------------------------------- |
|  [01]   | [CLAIM_SHAPE]  | the outbox row contract, the ports (channels, wake), the fan-out policy row        |
|  [02]   | [DRAIN_LOOP]   | the singleton drain: claim, quota gate, dispatch, settle/defer/park verdicts       |

## [2]-[CLAIM_SHAPE]

[CLAIM_SHAPE]:
- Owner: the relay's contract surface. `Relay.Row` is the decoded claim shape — the row id, the channel discriminant, the tenant `scope` string (the kernel partition key spelling), the opaque payload band (app-encoded JSON the channel handler decodes at its own seam), and the attempt count. `Relay.Channels` is the dispatch port: one Tag whose service is the mapped handler record over the closed channel vocabulary, satisfied at the app root by composing the sibling owners — `Hook.deliver` behind the `webhook` arm, `Mailer.send` behind `mail`, the rendered-artifact job behind `report` — so the drain stays total over channels while payload decode stays with each channel's schema. `Relay.Wake` is the acceleration port: a pulse stream the store LISTEN lane satisfies, with `Wake.silent` shipped so absence is a selection, never a wiring hole.
- Law: the fan-out policy is one row — `_FLOW` carries the claim batch width, the dispatch lane count, the poll cadence, the park ceiling, and the per-tenant quota (`window`, `limit`, `algorithm`, `onExceeded: "delay"`) — so tuning the relay is editing one anchor and every axis is recoverable from it.
- Law: the row contract is a cross-plane shape seam — the store's outbox page authors the relation and its ensure DDL, this page states the decoded shape and the column spellings its claim reads (`id`, `channel`, `scope`, `payload`, `attempts`, `deliver_at`, `claimed_at`, `parked_at`), and the two must agree by name-level parity; the table name itself is the policy row's `table` field, never scattered.
- Law: the payload band stays opaque here — the relay routes and meters, the channel handler decodes; a relay that parsed payloads would re-own every channel's vocabulary and break the one-owner law.
- Boundary: writing outbox rows is the store's transactional-outbox law (atomic with the append); the channel implementations are `deliver/webhook.md`, `deliver/mail.md`, `deliver/report.md`; entity-message delay via `DeliverAt` (`toMillis` on the payload) is the engine's own scheduled-delivery contract, mirrored here by the `deliver_at` predicate.
- Entry: `Relay.Channels` and `Relay.Wake` provided at the composition root beside the store driver.
- Growth: a new egress channel is one literal on the vocabulary plus one handler row at the root; a new quota axis is one `_FLOW.quota` field.
- Packages: `effect` (`Context`, `Duration`, `Effect`, `Layer`, `Schema`, `Stream`, `Types`), `@rasm/ts/kernel` (`Budget`).

```typescript
import { Singleton } from "@effect/cluster"
import { RateLimiter } from "@effect/experimental"
import { SqlClient, SqlSchema } from "@effect/sql"
import { Budget } from "@rasm/ts/kernel"
import { Context, Duration, Effect, Layer, Metric, Schedule, Schema, Stream, type Types } from "effect"
import { Storage } from "../engine/storage.ts"

const _channels = ["webhook", "mail", "report"] as const

const _FLOW = {
  table: "deliver_outbox",
  batch: 64,
  lanes: 8,
  poll: Duration.seconds(5),
  park: Budget.bulk.attempts,
  quota: { window: Duration.minutes(1), limit: 600, algorithm: "token-bucket", onExceeded: "delay" },
} as const

class _Row extends Schema.Class<_Row>("RelayRow")({
  id: Schema.String,
  channel: Schema.Literal(..._channels),
  scope: Schema.String,
  payload: Schema.String,
  attempts: Schema.Int,
}) {}

declare namespace Relay {
  type Channel = (typeof _channels)[number]
  type Row = _Row
  type Handlers = { readonly [K in Channel]: (row: _Row) => Effect.Effect<void, unknown> }
}

class _Channels extends Context.Tag("work/deliver/Channels")<_Channels, Relay.Handlers>() {}

class _Wake extends Context.Tag("work/deliver/Wake")<_Wake, {
  readonly pulses: Stream.Stream<void>
}>() {
  static readonly silent: Layer.Layer<_Wake> = Layer.succeed(_Wake, { pulses: Stream.empty })
}
```

## [3]-[DRAIN_LOOP]

[DRAIN_LOOP]:
- Owner: `Relay.run` — the drain registered as `Singleton.make("deliver/relay", drain)`, so exactly one instance runs across every shard and a runner loss re-homes it. The drain binds its fused accessors once — claim, settle, defer, park over the composed `SqlClient` — then folds the pulse stream: the spaced poll merged with the wake port's pulses, each pulse running one claim-and-dispatch pass. The claim is one statement — `UPDATE … SET claimed_at = now() WHERE id IN (SELECT … WHERE claimed_at IS NULL AND parked_at IS NULL AND deliver_at <= now() ORDER BY deliver_at LIMIT … FOR UPDATE SKIP LOCKED) RETURNING …` — so concurrent claimers can never double-claim and a crashed holder's rows return at the next lease sweep.
- Law: dispatch is quota-gated per row — the experimental store-backed limiter consumes `{ key: row.scope, …quota }` before the channel handler runs, `"delay"` suspends the lane rather than failing, and the window spans every process sharing the limiter store, so tenant fairness holds even if a second relay ever ran.
- Law: settlement is a classification fold — success deletes the row; a fault folds through `Storage.classify`: transient classes defer (attempts bump, `claimed_at` clears, `deliver_at` advances by the `bulk` budget's exponential backoff computed from the attempt ordinal), terminal classes and a spent park ceiling park the row (`parked_at = now()`) as reviewable evidence — a parked row is the relay's quarantine intake, replayed by clearing its mark, never deleted by the drain.
- Law: the loop is deathless by construction — a pass's own fault (a torn claim, a settle refusal) logs with its cause and the pass ends; the next pulse re-drives, so the poll cadence is the loop's retry policy and no fault can kill the singleton; the parked counter is the one metric the drain emits.
- Law: backoff is the kernel row applied at the store — the defer interval grows `Budget.bulk.base` by the row's factor per attempt, capped at the `bulk` window, so relay redelivery and step redelivery share one budget vocabulary.
- Boundary: `Singleton` registration rides `engine/entity.md`'s assembly; the classification fold is `engine/storage.md`'s; the limiter store provisioning is a `boundaries`-tier row the root satisfies.
- Entry: `Relay.run` merged at the composition root with `Relay.Channels`, `Relay.Wake` (or `Wake.silent`), the limiter store, and the store driver.
- Growth: a new settlement verdict is one arm in the fold; a new drain signal is a member on the wake port.
- Packages: `@effect/cluster` (`Singleton`), `@effect/experimental` (`RateLimiter`), `@effect/sql` (`SqlClient`, `SqlSchema`), `effect` (`Duration`, `Effect`, `Metric`, `Schedule`, `Stream`), `@rasm/ts/kernel` (`Budget`), `../engine/storage.ts` (`Storage`).

```typescript
const _parked = Metric.counter("relay_parked_total", { incremental: true })

const _backoff = (attempts: number): Duration.Duration =>
  Duration.min(Duration.times(Budget.bulk.base, Budget.bulk.factor ** attempts), Budget.bulk.window)

const _drain = Effect.gen(function* () {
  const sql = yield* SqlClient.SqlClient
  const channels = yield* _Channels
  const wake = yield* _Wake
  const limited = yield* RateLimiter.makeWithRateLimiter
  const claim = SqlSchema.findAll({
    Request: Schema.Int,
    Result: _Row,
    execute: (batch) => sql`
      UPDATE ${sql(_FLOW.table)} SET claimed_at = now()
      WHERE id IN (
        SELECT id FROM ${sql(_FLOW.table)}
        WHERE claimed_at IS NULL AND parked_at IS NULL AND deliver_at <= now()
        ORDER BY deliver_at
        LIMIT ${batch}
        FOR UPDATE SKIP LOCKED
      )
      RETURNING id, channel, scope, payload, attempts`,
  })
  const settle = SqlSchema.void({
    Request: Schema.String,
    execute: (id) => sql`DELETE FROM ${sql(_FLOW.table)} WHERE id = ${id}`,
  })
  const defer = SqlSchema.void({
    Request: Schema.Struct({ id: Schema.String, seconds: Schema.Number }),
    execute: ({ id, seconds }) => sql`
      UPDATE ${sql(_FLOW.table)}
      SET attempts = attempts + 1, claimed_at = NULL, deliver_at = now() + make_interval(secs => ${seconds})
      WHERE id = ${id}`,
  })
  const park = SqlSchema.void({
    Request: Schema.String,
    execute: (id) => sql`UPDATE ${sql(_FLOW.table)} SET parked_at = now() WHERE id = ${id}`,
  })
  const dispatched = (row: _Row): Effect.Effect<void> =>
    limited({ key: row.scope, ..._FLOW.quota })(channels[row.channel](row)).pipe(
      Effect.matchEffect({
        onSuccess: () => settle(row.id),
        onFailure: (fault) =>
          Storage.transient(fault) && row.attempts + 1 < _FLOW.park
            ? defer({ id: row.id, seconds: Duration.toSeconds(_backoff(row.attempts)) })
            : Effect.zipRight(Metric.increment(_parked), park(row.id)),
      }),
      Effect.catchAllCause((cause) => Effect.logError("relay settle refused", cause)),
    )
  const pass = claim(_FLOW.batch).pipe(
    Effect.flatMap((rows) => Effect.forEach(rows, dispatched, { concurrency: _FLOW.lanes, discard: true })),
    Effect.catchAllCause((cause) => Effect.logError("relay pass refused", cause)),
  )
  yield* Stream.runDrain(
    Stream.mapEffect(
      Stream.merge(wake.pulses, Stream.repeatEffectWithSchedule(Effect.void, Schedule.spaced(_FLOW.poll)), {
        haltStrategy: "right",
      }),
      () => pass,
    ),
  )
})

const _run = Singleton.make("deliver/relay", _drain)

declare namespace Relay {
  type Shape = Types.Simplify<{
    readonly run: typeof _run
    readonly Row: typeof _Row
    readonly Channels: typeof _Channels
    readonly Wake: typeof _Wake
  }>
}

const Relay: Relay.Shape = { run: _run, Row: _Row, Channels: _Channels, Wake: _Wake }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Relay }
```
