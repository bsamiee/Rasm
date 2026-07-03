# [STORE_APPEND]

The ONE append surface of the record of truth: streams are keyed `(appKey, tenantId, aggregate)` as one `StreamKey` value, events are closed `Schema.TaggedClass` families with app-authored `eventVersion` stamped from the upcast plan at write, and optimistic concurrency is an `Occ` value — exact expected version, no-stream, or any — checked under a per-stream advisory transaction lock with a unique `(stream, version)` constraint as the structural backstop. `Journal.of(spec)` binds a family once and yields the whole surface — `append`, `head`, `read` — over the neutral `SqlClient`, so the same statements run the pg spine and the sqlite lanes; app-authored events never cross the C# wire, and the C#-minted `CrdtOpWire` stream arrives already decoded through `wire/codec/crdt` as ordinary family values.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                          |
| :-----: | :----------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `STREAM_VOCABULARY`| `StreamKey`, the event-family contract, the persisted row shape, the ensure rows   |
|  [02]   | `APPEND_SURFACE`   | `Occ`, the OCC append under the advisory lock, `VersionConflict`, the receipt      |
|  [03]   | `READ_SURFACE`     | `head` and the windowed `read` stream lifted through the upcast plan               |
|  [04]   | `OVERLAY`          | the EventLog SQL backings — overlay only, never the record of truth                |

## [2]-[STREAM_VOCABULARY]

- Owner: `StreamKey` — one `Schema.Class` whose fields are the kernel identity brands plus the aggregate brand-in-field; the interior `_Row` model typing the persisted event row; `Journal.ddl` — the ensure rows `iac` applies and `Capability.verify` proves.
- Packages: `effect` (`Schema`); `@effect/sql` (`Model`); `@rasm/ts/kernel` (`AppKey`, `TenantId`).
- Growth: a new stream dimension is a `StreamKey` field plus a column pair in the ensure rows — every keyed surface in the folder re-keys with it because the class is the one spelling of stream identity.
- Law: events are app-authored closed `Schema.TaggedClass` families — the journal stores their encoded form as JSON text plus the `(tag, eventVersion)` coordinate; it never interprets payloads, so the family evolves without touching this page.
- Law: `sequence` is the global total order (identity column), `version` the per-stream order (OCC coordinate); both are engine-generated or engine-checked, never computed in process.
- Law: `recordedAt` is write time minted by `Model.DateTimeInsert` — domain time lives inside event payloads (`Hlc` where causality matters), and conflating the two is the named defect.
- Boundary: the tenant column is what `Tenancy.rls("journal_event")` predicates over — the RLS policy ensure is minted there and applied by `iac` beside these table ensures; `Model.makeRepository` is banned on this table (the journal never UPDATEs or DELETEs events).

```typescript
import { Schema } from "effect"
import { Model } from "@effect/sql"
import { AppKey, TenantId } from "@rasm/ts/kernel"

class StreamKey extends Schema.Class<StreamKey>("StreamKey")({
  app: AppKey,
  tenant: TenantId,
  aggregate: Schema.NonEmptyString.pipe(
    Schema.pattern(/^[a-z][a-z0-9-]*\/[A-Za-z0-9._:-]+$/),
    Schema.brand("Aggregate"),
  ),
}) {}

class _Row extends Model.Class<_Row>("JournalEvent")({
  sequence: Model.Generated(Schema.Number),
  app: AppKey,
  tenant: TenantId,
  aggregate: StreamKey.fields.aggregate,
  version: Schema.Number,
  tag: Schema.NonEmptyString,
  eventVersion: Schema.Int,
  payload: Schema.parseJson(Schema.Unknown),
  recordedAt: Model.DateTimeInsert,
}) {}

const _ddl: ReadonlyArray<Capability.Ensure> = [
  {
    relation: "journal_event",
    pg: `CREATE TABLE IF NOT EXISTS journal_event (
      sequence BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
      app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
      version BIGINT NOT NULL,
      tag TEXT NOT NULL, event_version INT NOT NULL,
      payload JSONB NOT NULL,
      recorded_at TIMESTAMPTZ NOT NULL DEFAULT now(),
      UNIQUE (app, tenant, aggregate, version));
    CREATE INDEX IF NOT EXISTS journal_event_stream ON journal_event (app, tenant, aggregate, version);`,
    sqlite: `CREATE TABLE IF NOT EXISTS journal_event (
      sequence INTEGER PRIMARY KEY AUTOINCREMENT,
      app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
      version INTEGER NOT NULL,
      tag TEXT NOT NULL, event_version INTEGER NOT NULL,
      payload TEXT NOT NULL,
      recorded_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
      UNIQUE (app, tenant, aggregate, version));`,
  },
]
```

## [3]-[APPEND_SURFACE]

- Owner: `Journal.of(spec)` — binds one event family plus its upcast plan and returns the bound surface; `Occ` — the tagged concurrency expectation; `VersionConflict` — the one domain fault of the write path.
- Packages: `effect` (`Effect`, `Array`, `Data`, `Schema`); `@effect/sql` (`SqlClient`, `sql.insert`, `sql.onDialectOrElse`, `SqlError`).
- Entry: `bound.append(stream, events, occ)` — ONE entry whose plural modality is the input shape (`A | NonEmptyReadonlyArray<A>`), never an `appendMany` sibling; every write in the folder funnels here, and `journal/outbox.md` composes it inside the atomic publish transaction.
- Receipt: `Journal.Receipt` — `{ stream, version, count, first }` — the new head, the appended count, and the first written stream version; `project/inline.md` folds from it and `outbox` stores it in the idempotency ledger for replay.
- Growth: a new write-side invariant is a guard inside `_append` (one expression), never a second append; a new event tag costs this page nothing — the plan stamps its `eventVersion` and the union admits it.
- Law: concurrency is `Occ` — `Exact` fails as `VersionConflict` when the locked head disagrees, `None` demands version zero, `Any` serializes under the lock and appends at head; the advisory lock is `pg_advisory_xact_lock(hashtextextended(...))` on the pg spine and degrades to the single-writer file on sqlite through `onDialectOrElse` — the unique constraint remains the structural backstop on every lane.
- Law: the conflict carries evidence — `expected` and `actual` versions — so the caller's recovery is reload-fold-retry as data, and retrying rides a `Schedule` gated on the `VersionConflict` tag, never a loop.
- Law: `eventVersion` is stamped from `plan.latest(tag)` at write — the write coordinate and the read lift share one anchor; an event whose tag the plan does not know is a defect at the append site, caught before any row is written.
- Law: `Journal.now(sql)` is the one dialect-now fragment — every sibling statement that stamps a timestamp splices it, so `now()` versus `strftime` exists in exactly one spelling folder-wide.
- Law: the append body rides `sql.withTransaction` — standalone it owns its commit, and inside `journal/outbox.md`'s publish it folds to a savepoint — so the advisory xact lock and the head check share one transaction on every path; an unwrapped append would release the lock per statement and race its own OCC check.
- Boundary: encode faults are `ParseError` on the same rail admission uses; the atomic composition with the outbox and ledger is `journal/outbox.md`'s.

```typescript
import { Array, Data, Effect, type ParseResult } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import type { Capability } from "../capability/row.ts"
import { Upcast } from "./upcast.ts"

class VersionConflict extends Data.TaggedError("VersionConflict")<{
  readonly stream: StreamKey
  readonly expected: number
  readonly actual: number
}> {}

declare namespace Journal {
  type Occ = Data.TaggedEnum<{
    Exact: { readonly version: number }
    None: {}
    Any: {}
  }>
  type Conflict = VersionConflict
  type Event = { readonly _tag: string }
  type Spec<A extends Event, I> = {
    readonly family: Schema.Schema<A, I>
    readonly plan: Upcast.Plan<A>
  }
  type Receipt = {
    readonly stream: StreamKey
    readonly version: number
    readonly count: number
    readonly first: number
    readonly rows: ReadonlyArray<{ readonly version: number; readonly tag: string; readonly payload: string }>
  }
  type Bound<A extends Event> = {
    readonly append: (
      stream: StreamKey,
      events: A | Array.NonEmptyReadonlyArray<A>,
      occ: Occ,
    ) => Effect.Effect<Receipt, Conflict | SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
    readonly head: (stream: StreamKey) => Effect.Effect<number, SqlError.SqlError, SqlClient.SqlClient>
    readonly read: (
      stream: StreamKey,
      window?: { readonly from?: number; readonly to?: number },
    ) => Stream.Stream<A, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
  }
}

const _Occ = Data.taggedEnum<Journal.Occ>()

const _head = (sql: SqlClient.SqlClient, stream: StreamKey): Effect.Effect<number, SqlError.SqlError> =>
  Effect.map(
    sql`SELECT coalesce(max(version), 0) AS head FROM journal_event
        WHERE app = ${stream.app} AND tenant = ${stream.tenant} AND aggregate = ${stream.aggregate}`.values,
    (cells) => Number(cells[0]?.[0] ?? 0),
  )

const _append = <A extends Journal.Event, I>(spec: Journal.Spec<A, I>) =>
  (stream: StreamKey, events: A | Array.NonEmptyReadonlyArray<A>, occ: Journal.Occ) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      sql.withTransaction(
        Effect.gen(function* () {
          const batch = Array.ensure(events)
          yield* sql.onDialectOrElse({
            orElse: () => sql`SELECT 1`,
            pg: () =>
              sql`SELECT pg_advisory_xact_lock(hashtextextended(${stream.app} || ':' || ${stream.tenant} || ':' || ${stream.aggregate}, 0))`,
          })
          const held = yield* _head(sql, stream)
          yield* _Occ.$match(occ, {
            Exact: ({ version }) =>
              held === version
                ? Effect.void
                : Effect.fail(new VersionConflict({ stream, expected: version, actual: held })),
            None: () =>
              held === 0
                ? Effect.void
                : Effect.fail(new VersionConflict({ stream, expected: 0, actual: held })),
            Any: () => Effect.void,
          })
          const encode = Schema.encode(Schema.parseJson(spec.family))
          const rows = yield* Effect.forEach(batch, (event, index) =>
            Effect.zipWith(encode(event), Effect.orDie(spec.plan.latest(event._tag)), (payload, eventVersion) => ({
              app: stream.app,
              tenant: stream.tenant,
              aggregate: stream.aggregate,
              version: held + 1 + index,
              tag: event._tag,
              eventVersion,
              payload,
            })))
          yield* sql`INSERT INTO journal_event ${sql.insert(rows)}`
          return {
            stream,
            version: held + batch.length,
            count: batch.length,
            first: held + 1,
            rows: rows.map((row) => ({ version: row.version, tag: row.tag, payload: row.payload })),
          } satisfies Journal.Receipt
        }),
      ))
```

## [4]-[READ_SURFACE]

- Owner: the bound `head` and `read` members — `read` is a backpressured statement stream lifted row-by-row through the upcast plan into live family values.
- Packages: `effect` (`Stream`); `@effect/sql` (`Statement.stream` backed by `SqlStream.asyncPauseResume` on the pg cursor).
- Entry: `bound.read(stream, window?)` — the one replay road; `project/*` lanes, `journal/retain.md`'s DSAR fold, and snapshot-plus-tail loads all compose it with a `from` window instead of minting their own SELECT.
- Growth: a new read shape (by tag, by time) is a window field, never a sibling read.
- Law: rows leave the statement as `Upcast.Raw` and exist as nothing else — the decoded family value is the only shape past this seam, so a malformed historical payload surfaces as `ParseError` exactly once, at the lift.

```typescript
import { Stream } from "effect"

const _read = <A extends Journal.Event, I>(spec: Journal.Spec<A, I>) =>
  (stream: StreamKey, window?: { readonly from?: number; readonly to?: number }) =>
    Stream.unwrap(
      Effect.map(SqlClient.SqlClient, (sql) =>
        sql`SELECT tag, event_version, payload, version FROM journal_event
            WHERE app = ${stream.app} AND tenant = ${stream.tenant} AND aggregate = ${stream.aggregate}
              AND version >= ${window?.from ?? 1} AND version <= ${window?.to ?? Number.MAX_SAFE_INTEGER}
            ORDER BY version`.stream.pipe(
          Stream.mapEffect((row) =>
            spec.plan.decode({
              tag: String(row["tag"]),
              version: Number(row["event_version"]),
              payload: Upcast.body(row["payload"]),
            })),
        )),
    )
```

## [5]-[OVERLAY]

- Owner: the EventLog overlay bindings — the SQL backings that let the local-first overlay persist onto this journal-owning `SqlClient`, exposed as `Journal.overlay` rows — and the assembled `Journal` export closing the page: `of`, `now`, `ddl`, `overlay`, the `Occ` constructor, and the `Conflict` class under one name.
- Packages: `@effect/sql` (`SqlEventJournal`, `SqlEventLogServer`); `@effect/experimental` is the overlay's own tier, composed by `browser/persist` and `edge/live`, never here.
- Entry: `Journal.overlay.journal` = `SqlEventJournal.layer` (`Layer<EventJournal, SqlError, SqlClient>` — the durable-node local-first entry store); `Journal.overlay.server` = `SqlEventLogServer.layerStorage` (the E2E-encrypted sync server's storage, mounted at `edge/live`'s protocol-handler port); `Journal.overlay.serverSubtle` = `SqlEventLogServer.layerStorageSubtle` (the Web-Crypto zero-knowledge variant).
- Law: `[R19]` is the boundary — the overlay accelerates local-first reads and offline sync; a record whose loss corrupts state lives in THIS journal and is projected to the overlay, never the reverse; the overlay tables are not the record of truth and no projection lane reads them.
- Law: `[R4]` is the adoption gate — the overlay backings are adopted only if their table bootstrap is verifiably ensure-shaped (idempotent, additive, runnable by `iac`); otherwise their DDL is owned locally as ensure rows beside `_ddl`, and the layers still bind — adopt-or-own, never migrate.

```typescript
import { SqlEventJournal, SqlEventLogServer } from "@effect/sql"

const _overlay = {
  journal: SqlEventJournal.layer,
  server: SqlEventLogServer.layerStorage,
  serverSubtle: SqlEventLogServer.layerStorageSubtle,
} as const

const Journal = {
  of: <A extends Journal.Event, I>(spec: Journal.Spec<A, I>): Journal.Bound<A> => ({
    append: _append(spec),
    head: (stream) => Effect.flatMap(SqlClient.SqlClient, (sql) => _head(sql, stream)),
    read: _read(spec),
  }),
  now: (sql: SqlClient.SqlClient) =>
    sql.onDialectOrElse({
      orElse: () => sql.literal("strftime('%Y-%m-%dT%H:%M:%fZ','now')"),
      pg: () => sql.literal("now()"),
    }),
  ddl: _ddl,
  overlay: _overlay,
  Occ: _Occ,
  Conflict: VersionConflict,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Journal, StreamKey }
```
