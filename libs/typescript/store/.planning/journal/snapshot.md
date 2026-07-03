# [STORE_SNAPSHOT]

The snapshot store is a read accelerator keyed `snapshot_schema_version`: one latest-per-stream row holding the folded state's encoded body, its stream version, and the schema version its author stamped, so a load is one row plus the journal tail instead of a full replay. Bodies lift through the `Upcast.chain` fold — the single-shape twin of the event plan — so a state reshape is one step appended and every historical snapshot stays loadable; a snapshot is always discardable evidence, never truth: dropping the table costs a replay, nothing more. Cadence is a policy value the projection lanes consult, and the monotonic upsert guard makes concurrent snapshotters harmless.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                       |
| :-----: | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `SNAPSHOT_ROW` | the ensure row, the bound save/load surface, the monotonic upsert                 |
|  [02]   | `CADENCE`      | the snapshot-due policy row and the snapshot-plus-tail load fold                  |

## [2]-[SNAPSHOT_ROW]

- Owner: `Snapshot.of(spec)` — binds one state schema plus its `Upcast.Lift` and yields `{ save, load }` over the neutral `SqlClient`; the `journal_snapshot` ensure row with its latest-only primary key.
- Packages: `effect` (`Effect`, `Option`, `Schema`); `@effect/sql` (`SqlClient`); `journal/upcast.md` (`Upcast.body` — the one dialect-honest JSON-column read); the monotonic upsert is one dialect-shared statement, because pg and sqlite carry the same `ON CONFLICT … DO UPDATE … WHERE` form.
- Entry: `bound.save(stream, state, version)` and `bound.load(stream)` — the only snapshot road; `project/*` lanes and rebuilds compose these, and nothing else touches the table.
- Receipt: `load` yields `Option<{ state, version }>` — present means fold-from-`version + 1`, absent means replay from origin; the option IS the protocol.
- Growth: a state reshape is one `Upcast.Chain` step plus a bumped `latest` stamped on subsequent saves; a second snapshotted shape for one stream family is a second `Snapshot.of` binding over its own spec, never a widened row.
- Law: the upsert is monotonic — `ON CONFLICT (app, tenant, aggregate) DO UPDATE … WHERE excluded.version > journal_snapshot.version` — a stale snapshotter racing a fresh one commits nothing, so cadence needs no coordination.
- Law: `snapshot_schema_version` is stamped from `lift.latest` at save and consumed by `lift.decode` at load — write coordinate and read fold share one anchor exactly as events do.
- Law: a load whose body fails the lift is `ParseError` on the admission rail — the consuming lane discards the snapshot and replays; corruption degrades to cost, never to wrong state.
- Boundary: the C#-minted `SnapshotHeader` (canonical-CBOR content-stable bytes) arrives decoded through `wire/codec/snapshot` and lands here as an ordinary save by its consuming lane — this page never re-decodes wire bytes.

```typescript
import { Effect, Option, Schema, type ParseResult } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import type { Capability } from "../capability/row.ts"
import { Journal, StreamKey } from "./append.ts"
import { Upcast } from "./upcast.ts"

declare namespace Snapshot {
  type Spec<S, I> = {
    readonly state: Schema.Schema<S, I>
    readonly lift: Upcast.Lift<S>
  }
  type Held<S> = {
    readonly state: S
    readonly version: number
  }
  type Bound<S> = {
    readonly save: (stream: StreamKey, state: S, version: number) => Effect.Effect<
      void,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
    readonly load: (stream: StreamKey) => Effect.Effect<
      Option.Option<Held<S>>,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
  }
}

const _ddl: Capability.Ensure = {
  relation: "journal_snapshot",
  pg: `CREATE TABLE IF NOT EXISTS journal_snapshot (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version BIGINT NOT NULL,
    snapshot_schema_version INT NOT NULL,
    body JSONB NOT NULL,
    taken_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (app, tenant, aggregate));`,
  sqlite: `CREATE TABLE IF NOT EXISTS journal_snapshot (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version INTEGER NOT NULL,
    snapshot_schema_version INTEGER NOT NULL,
    body TEXT NOT NULL,
    taken_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (app, tenant, aggregate));`,
}

const _save = <S, I>(spec: Snapshot.Spec<S, I>) =>
  (stream: StreamKey, state: S, version: number) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const body = yield* Schema.encode(Schema.parseJson(spec.state))(state)
      yield* sql`INSERT INTO journal_snapshot ${sql.insert([{
        app: stream.app,
        tenant: stream.tenant,
        aggregate: stream.aggregate,
        version,
        snapshot_schema_version: spec.lift.latest,
        body,
      }])} ON CONFLICT (app, tenant, aggregate) DO UPDATE
        SET version = excluded.version, snapshot_schema_version = excluded.snapshot_schema_version,
            body = excluded.body, taken_at = ${Journal.now(sql)}
        WHERE excluded.version > journal_snapshot.version`
    })

const _load = <S, I>(spec: Snapshot.Spec<S, I>) =>
  (stream: StreamKey) =>
    Effect.gen(function* () {
      const sql = yield* SqlClient.SqlClient
      const rows = yield* sql`SELECT version, snapshot_schema_version, body FROM journal_snapshot
        WHERE app = ${stream.app} AND tenant = ${stream.tenant} AND aggregate = ${stream.aggregate}`
      const row = rows[0]
      if (row === undefined) return Option.none<Snapshot.Held<S>>()
      const state = yield* spec.lift.decode(Number(row["snapshot_schema_version"]), Upcast.body(row["body"]))
      return Option.some({ state, version: Number(row["version"]) })
    })
```

## [3]-[CADENCE]

- Owner: the `due` policy fold plus the `hydrate` composition — snapshot-plus-tail is one load: the option folds to a seed and a `from` window, the journal read stream folds the tail.
- Packages: `effect` (`Stream`); `journal/append.md` (`Journal.Bound.read`).
- Entry: lanes call `Snapshot.due(version, policy)` after each apply and `bound.save` when it answers true; `Snapshot.hydrate(bound, journal, stream, fold)` is the one state-recovery entry every lane and rebuild composes.
- Growth: a new cadence shape (byte budget, elapsed time) is a field on the policy row consumed inside `due` — the call sites never change.
- Law: cadence is data — `{ every: n }` as the modulo row; snapshotting is always safe to skip and safe to repeat, so `due` is a pure fold and no lane coordinates with another.

```typescript
import { Stream } from "effect"

declare namespace Snapshot {
  type Cadence = { readonly every: number }
}

const _due = (version: number, cadence: Snapshot.Cadence): boolean =>
  version > 0 && version % cadence.every === 0

const _hydrate = <S, A extends Journal.Event>(
  bound: Snapshot.Bound<S>,
  journal: Journal.Bound<A>,
  stream: StreamKey,
  fold: { readonly seed: S; readonly step: (state: S, event: A) => S },
) =>
  Effect.gen(function* () {
    const held = yield* bound.load(stream)
    const origin = Option.match(held, {
      onNone: () => ({ state: fold.seed, from: 1 }),
      onSome: (row) => ({ state: row.state, from: row.version + 1 }),
    })
    return yield* Stream.runFold(
      journal.read(stream, { from: origin.from }),
      origin.state,
      fold.step,
    )
  })

const Snapshot = {
  of: <S, I>(spec: Snapshot.Spec<S, I>): Snapshot.Bound<S> => ({ save: _save(spec), load: _load(spec) }),
  due: _due,
  hydrate: _hydrate,
  ddl: [_ddl],
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Snapshot }
```
