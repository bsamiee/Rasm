# [SERVICES_TRANSACTIONAL_OUTBOX]

The reliable-event-publishing owner — `Outbox`, a single `Model.Class` written in the SAME Postgres transaction as the domain mutation, and `OutboxRelay`, a durable relay draining the outbox via `FOR UPDATE SKIP LOCKED` claim plus a `LISTEN`/`NOTIFY` wake, publishing through a `DeliverySink` tagged axis to the internal RPC surface and external sinks. The outbox closes the dual-write gap: no domain change commits without its event enqueued in the same transaction, so the event publishes at-least-once after the commit even across a crash. The durable claim is the source of truth; `NOTIFY` is at-most-once, 8 KB-capped, and non-persistent, so it is ONLY the wake, never the delivery. The relay is idempotent and exactly-once against re-delivery. The owner rides the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient` and crosses no .NET wire. This is the net-new `eventing/` sub-domain's first page.

## [1]-[INDEX]

One cluster: `[2]-[TRANSACTIONAL_OUTBOX]` owns the same-txn `Outbox` `Model.Class`, the `FOR UPDATE SKIP LOCKED` relay loop, the `LISTEN`/`NOTIFY` wake, and the `DeliverySink` tagged axis.

RESEARCH: the logical-replication push variant rests on `@effect/sql-pg` exposing a logical-replication slot/decode surface beyond `listen`/`notify`; the catalogue confirms `listen`/`notify` only, so the relay's polling-plus-`NOTIFY`-wake arm is the settled fence and the logical-replication CDC arm is the open probe gated on confirming `@effect/sql-pg` logical-replication support.

## [2]-[TRANSACTIONAL_OUTBOX]

- Owner: `Outbox`, the single outbox-row `Model.Class` written same-txn with the domain mutation; `OutboxRelay`, the durable claim-and-publish relay; `DeliverySink`, the `Data.TaggedEnum` over the delivery target (internal RPC vs external).
- Cases: `Outbox` is one `Model.Class` row — id, aggregate key, event type, payload, a `pending`/`delivered`/`failed` status, an attempt count, and the insert timestamp — written through the SAME `SqlClient.withTransaction` the domain mutation runs in, so the outbox row and the domain change commit atomically or roll back together, closing the dual-write gap `persistence/work-and-signals#WORK_AND_SIGNALS` `EventJournal`/`Notifications` lack; the same transaction fires `notify(channel, ...)` after the insert so a relay parked on `listen(channel)` wakes immediately. `OutboxRelay` is the drain loop — it claims a batch of `pending` rows with `SELECT ... FOR UPDATE SKIP LOCKED` so concurrent relay runners never claim the same row, publishes each through the `DeliverySink` arm, marks the row `delivered` (or increments the attempt and re-queues on failure), and parks on `listen(channel)` for the next wake, falling back to a poll interval because `NOTIFY` is at-most-once and non-persistent — the durable claim is the source of truth, `NOTIFY` only the wake. `DeliverySink` is the closed `Data.TaggedEnum` — `Internal` publishes to the `messaging/internal-rpc#INTERNAL_RPC` `RpcGroup`, `External` publishes to an outbound sink — folded by `Match.tagsExhaustive` so a new sink breaks the fold at compile time. The relay is idempotent and exactly-once against re-delivery: the `FOR UPDATE SKIP LOCKED` claim plus the per-row status transition means a row delivered once is never re-delivered, and a crash mid-publish re-claims the still-`pending` row on restart.
- Entry: the outbox shares the one `persistence/store-boundary#STORE_BOUNDARY` `PgClient` — the `Outbox` `Model.Class` is one entity on the `EntityRegistry`, the same-txn write rides `SqlClient.withTransaction`, and the relay wakes off the same `listen`/`notify` channel pair the store boundary reserves for the durable tier; the relay registers as a `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` `ScheduledWork.singleton` so exactly one runner drains the outbox (the `FOR UPDATE SKIP LOCKED` claim makes multi-runner safe, but the singleton bounds the poll cost); the `DeliverySink.Internal` arm publishes through `messaging/internal-rpc#INTERNAL_RPC`, giving `persistence/work-and-signals#WORK_AND_SIGNALS` `EventJournal`/`Notifications` their atomic-publish contract.
- Wire: the owner crosses no .NET wire — the outbox publishes node-internal domain events through the internal RPC surface and external sinks, never a .NET wire contract; the C# branch owns no transactional-outbox seam this page decodes.
- Packages: `@effect/sql` and `@effect/sql-pg` for the same-txn `Model.Class` write (`SqlClient.withTransaction`), the `FOR UPDATE SKIP LOCKED` claim, and the `listen`/`notify` channel through the one `PgClient`; `@effect/cluster` for the `ScheduledWork.singleton` the relay registers on; `effect` for the relay `Stream` loop and the `DeliverySink` `Data.TaggedEnum` fold.
- Growth: a new delivery target lands as one `DeliverySink` `Data.TaggedEnum` variant breaking the `Match.tagsExhaustive` fold at compile time, never a parallel relay; a new event type lands as one `Outbox` payload row, never a sibling table; the logical-replication push variant lands as one CDC arm beside the polling-plus-`NOTIFY`-wake arm once `@effect/sql-pg` logical-replication support confirms.
- Boundary: the named defects — a second SQL surface beside the one `PgClient`; a `NOTIFY`-as-delivery instead of `NOTIFY`-as-wake plus the durable claim as source of truth; a non-idempotent relay re-delivering on re-claim; an outbox row written outside the domain mutation's transaction breaking atomicity; a parallel relay per sink instead of the one `DeliverySink` tagged axis; a multi-runner drain without `FOR UPDATE SKIP LOCKED` double-claiming a row. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import { Model, SqlClient, SqlError } from "@effect/sql"
import { Data, Effect, Match, Schema, Stream } from "effect"

class Outbox extends Model.Class<Outbox>("Outbox")({
  id: Model.Generated(Schema.Number),
  aggregate: Schema.String,
  eventType: Schema.String,
  payload: Schema.parseJson(Schema.Unknown),
  status: Schema.Literal("pending", "delivered", "failed"),
  attempts: Schema.Number,
  enqueuedAt: Model.DateTimeInsert,
}) {}

type DeliverySink = Data.TaggedEnum<{
  readonly Internal: { readonly procedure: string }
  readonly External: { readonly endpoint: string; readonly headers: Record<string, string> }
}>
const DeliverySink = Data.taggedEnum<DeliverySink>()

class OutboxFault extends Schema.TaggedError<OutboxFault>()("OutboxFault", {
  id: Schema.Number,
  sink: Schema.Literal("Internal", "External"),
  attempts: Schema.Number,
  cause: Schema.Unknown,
}) {}

interface OutboxRelay {
  readonly enqueue: <A, E, R>(write: Effect.Effect<A, E, R>, event: { readonly aggregate: string; readonly eventType: string; readonly payload: unknown }) => Effect.Effect<A, E | SqlError.SqlError, R>
  readonly claim: (batch: number) => Effect.Effect<ReadonlyArray<Outbox>, SqlError.SqlError>
  readonly deliver: (sink: (row: Outbox) => DeliverySink) => (row: Outbox) => Effect.Effect<void, OutboxFault>
  readonly drain: Effect.Effect<void, SqlError.SqlError>
  readonly run: Stream.Stream<number, SqlError.SqlError>
}

const CHANNEL = "outbox_event" as const

const enqueue = (sql: SqlClient.SqlClient) =>
  <A, E, R>(write: Effect.Effect<A, E, R>, event: { readonly aggregate: string; readonly eventType: string; readonly payload: unknown }): Effect.Effect<A, E | SqlError.SqlError, R> =>
    sql.withTransaction(
      write.pipe(
        Effect.tap(() => sql`INSERT INTO outbox (aggregate, event_type, payload, status, attempts) VALUES (${event.aggregate}, ${event.eventType}, ${JSON.stringify(event.payload)}, 'pending', 0)`),
        Effect.tap(() => sql`SELECT pg_notify(${CHANNEL}, '1')`),
      ),
    )

const claim = (sql: SqlClient.SqlClient, batch: number): Effect.Effect<ReadonlyArray<Outbox>, SqlError.SqlError> =>
  sql<Outbox>`
    SELECT id, aggregate, event_type AS "eventType", payload, status, attempts, enqueued_at AS "enqueuedAt"
    FROM outbox WHERE status = 'pending'
    ORDER BY id FOR UPDATE SKIP LOCKED LIMIT ${batch}
  `

const deliver = (sink: (row: Outbox) => DeliverySink) => (row: Outbox): Effect.Effect<void, OutboxFault> =>
  Match.value(sink(row)).pipe(
    Match.tagsExhaustive({
      Internal: ({ procedure }) => publishInternal(procedure, row),
      External: ({ endpoint, headers }) => publishExternal(endpoint, headers, row),
    }),
  )

declare const publishInternal: (procedure: string, row: Outbox) => Effect.Effect<void, OutboxFault>
declare const publishExternal: (endpoint: string, headers: Record<string, string>, row: Outbox) => Effect.Effect<void, OutboxFault>

const wake = (client: PgClient.PgClient): Stream.Stream<string, SqlError.SqlError> => client.listen(CHANNEL)
```
