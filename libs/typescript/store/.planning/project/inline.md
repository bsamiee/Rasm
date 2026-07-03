# [STORE_INLINE]

The inline lane binds the `state` fold algebra to durability inside the write transaction: an `InlineLane` is a fold spec — seed, step, and a keyed upsert target — packaged as the `Outbox.Slot` value the publish transaction executes, so the read model commits atomically with the events that produced it and a reader who follows the write sees it, structurally. The read side is `sql.reactive` over the same invalidation keys the slot stamps — read-your-writes is one coordinate vocabulary written at the mutation and consumed at the query, never a poll and never a cache to bust by hand. Folds arrive as VALUES shaped by `state/fold/algebra`; this page owns their durable binding, not their algebra.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     |
| :-----: | :---------------- | :---------------------------------------------------------------------------- |
|  [01]   | `LANE_BINDING`    | `InlineLane.of` — fold spec to `Outbox.Slot`, the keyed upsert, the key stamp   |
|  [02]   | `REACTIVE_READ`   | the `sql.reactive`/`reactiveMailbox` read surface over the stamped keys         |

## [2]-[LANE_BINDING]

- Owner: `InlineLane.of(spec)` — one constructor turning `{ name, fold, table, cell, encode }` into the slot value `Outbox.publish` executes; the lane's read-model ensure row rides the spec.
- Packages: `effect` (`Effect`, `Schema`); `@effect/sql` (`SqlClient`, `sql.insert`, `sql.onDialect` for the upsert pair).
- Entry: the app constructs lanes beside its journal binding and passes them in `Outbox.Intent.slots` — the lane never imports the outbox, the outbox never imports the lane; the slot SHAPE is the whole contract.
- Receipt: the lane's effect is void by design — its receipt is the committed row plus the stamped keys; evidence of projection health is the async lanes' concern, because an inline failure rolls back the whole publish.
- Growth: a new read model is one more lane value in the intent — zero edits anywhere; a new fold shape is `state`'s business arriving as a value.
- Law: the fold runs current-row-first — the slot loads the cell's held state (`SELECT … FOR UPDATE` on pg through `onDialectOrElse`, the bare read on sqlite's single writer), folds the batch from it, and upserts — so an inline lane is incremental over the stream without replaying it.
- Law: the upsert is one statement — `INSERT … ON CONFLICT (cell) DO UPDATE SET state = excluded.state, version = excluded.version` — and carries the stream version, so a torn or reordered apply is structurally impossible inside the one transaction.
- Law: inline lanes are for read models whose staleness budget is zero — everything else belongs on the async lanes; a slow fold inside the write transaction is the named misplacement, and the lane roster per publish should be small by design pressure, not by rule.
- Boundary: the fold value's algebra (keyed folds, CRDT merge) is `state/fold/algebra.md`'s; the slot's execution site and ordering are `journal/outbox.md`'s.

```typescript
import { Effect, Schema } from "effect"
import { SqlClient } from "@effect/sql"
import type { Capability } from "../capability/row.ts"
import { Journal, type StreamKey } from "../journal/append.ts"
import type { Outbox } from "../journal/outbox.ts"

declare namespace InlineLane {
  type Fold<S, A> = {
    readonly seed: S
    readonly step: (state: S, event: A) => S
  }
  type Spec<S, A, I> = {
    readonly name: string
    readonly fold: Fold<S, A>
    readonly table: string
    readonly cell: (stream: StreamKey) => string
    readonly state: Schema.Schema<S, I>
  }
}

const _ddl = (table: string): Capability.Ensure => ({
  relation: table,
  pg: `CREATE TABLE IF NOT EXISTS ${table} (
    cell TEXT PRIMARY KEY,
    state JSONB NOT NULL,
    version BIGINT NOT NULL,
    folded_at TIMESTAMPTZ NOT NULL DEFAULT now());`,
  sqlite: `CREATE TABLE IF NOT EXISTS ${table} (
    cell TEXT PRIMARY KEY,
    state TEXT NOT NULL,
    version INTEGER NOT NULL,
    folded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')));`,
})

const _of = <S, A, I>(spec: InlineLane.Spec<S, A, I>): Outbox.Slot<A> & { readonly ddl: ReadonlyArray<Capability.Ensure> } => ({
  ddl: [_ddl(spec.table)],
  keys: (stream) => ({ [spec.name]: [spec.cell(stream)] }),
  project: (stream, events, receipt) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const cell = spec.cell(stream)
      const held = yield* sql.onDialectOrElse({
        orElse: () => sql`SELECT state FROM ${sql(spec.table)} WHERE cell = ${cell}`,
        pg: () => sql`SELECT state FROM ${sql(spec.table)} WHERE cell = ${cell} FOR UPDATE`,
      })
      const seed = held[0] === undefined
        ? spec.fold.seed
        : yield* Schema.decodeUnknown(Schema.parseJson(spec.state))(String(held[0].state))
      const folded = events.reduce(spec.fold.step, seed)
      const state = yield* Schema.encode(Schema.parseJson(spec.state))(folded)
      yield* sql`INSERT INTO ${sql(spec.table)} ${sql.insert([{ cell, state, version: receipt.version }])}
        ON CONFLICT (cell) DO UPDATE
        SET state = excluded.state, version = excluded.version, folded_at = ${Journal.now(sql)}`
    }),
})
```

## [3]-[REACTIVE_READ]

- Owner: the lane's read surface — `read` as a decoded one-shot, `changes` as the `sql.reactive` stream that re-runs on every overlapping mutation, `mailbox` as the pull-model twin for browser read lanes.
- Packages: `@effect/sql` (`sql.reactive`, `sql.reactiveMailbox` — both riding the `@effect/experimental` `Reactivity` service the driver layers provide).
- Entry: `lane.read(cell)` for a plain load; `lane.changes(cell)` where the consumer must observe every committed fold — `edge/live` serves these streams, `browser/persist` pulls the mailbox.
- Growth: a cross-cell view is a `changes` over the whole-band key (`{ [name]: [] }` names the band) — the record-key vocabulary already scopes member and band reads, so no second subscription surface exists.
- Law: the keys ARE the contract — the slot stamps `{ [name]: [cell] }` at mutation, the read subscribes the same coordinates; a mutation wakes member readers and whole-band readers both, and nothing else, so delivery is exact and a cadence poll restates what the keys own.
- Law: reads decode through the lane's state schema — the stored JSON never leaks; a stale-schema row surfaces as `ParseError` and the repair is a rebuild (`project/rebuild.md`), never an in-place patch.

```typescript
import { Option, Stream } from "effect"

declare namespace InlineLane {
  type Bound<S, A> = Outbox.Slot<A> & {
    readonly ddl: ReadonlyArray<Capability.Ensure>
    readonly read: (cell: string) => Effect.Effect<Option.Option<S>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
    readonly changes: (cell: string) => Stream.Stream<Option.Option<S>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
  }
}

const _read = <S, A, I>(spec: InlineLane.Spec<S, A, I>) =>
  (cell: string) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      Effect.gen(function* () {
        const rows = yield* sql`SELECT state FROM ${sql(spec.table)} WHERE cell = ${cell}`
        if (rows[0] === undefined) return Option.none<S>()
        return Option.some(yield* Schema.decodeUnknown(Schema.parseJson(spec.state))(String(rows[0].state)))
      }))

const _changes = <S, A, I>(spec: InlineLane.Spec<S, A, I>) =>
  (cell: string) =>
    Stream.unwrap(
      Effect.map(SqlClient.SqlClient, (sql) =>
        sql.reactive({ [spec.name]: [cell] }, _read(spec)(cell))),
    )

const InlineLane = {
  of: <S, A, I>(spec: InlineLane.Spec<S, A, I>): InlineLane.Bound<S, A> => ({
    ..._of(spec),
    read: _read(spec),
    changes: _changes(spec),
  }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { InlineLane }
```
