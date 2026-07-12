# [DATA_APPEND]

The ONE write owner of the record of truth: journal, outbox, and idempotency ledger as a single atomic surface. Streams are keyed `(app, tenant, aggregate)` as one `StreamKey` value, events are closed `Schema.TaggedClass` families with `eventVersion` stamped from the evolve plan at write, and optimistic concurrency is an `Occ` value checked under a per-stream advisory transaction lock with the unique `(stream, version)` constraint as the structural backstop. `Journal.of(spec)` binds a family once and yields the whole bound surface — `append`, `head`, `read`, and `publish`, where publish composes the `(xmax = 0)` ledger claim, the OCC append, the outbox insert, the inline projection slots, and the ledger settle into ONE commit: replays return the stored receipt, deliverable rows become facts atomically with the events they announce, and the NOTIFY wake plus the reactivity stamp fire only when the commit lands. The same statements run the pg spine and every sqlite profile through the dialect arms; this page owns queue-as-data — the relay claim and completion statements the work plane drains through its `SqlClient` port — while execution semantics stay across that seam.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                            |
| :-----: | :------------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `STREAM_VOCABULARY` | `StreamKey`, the event-family contract, the persisted row models, the ensure rows |
|  [02]   | `APPEND_SURFACE`    | `Occ`, the locked OCC append, `VersionConflict`, the receipt, the bulk lane       |
|  [03]   | `LEDGER_CLAIM`      | the idempotency ledger — key brand, `(xmax = 0)` claim, replay receipt            |
|  [04]   | `ATOMIC_PUBLISH`    | the one publish transaction — claim, append, outbox, slots, settle, wake          |
|  [05]   | `READ_SURFACE`      | `head` and the windowed `read` stream lifted through the evolve plan              |
|  [06]   | `RELAY_ROWS`        | the deliverable model, the SKIP-LOCKED claim/complete pair, the overlay bindings  |

## [02]-[STREAM_VOCABULARY]

- Owner: `StreamKey` — one `Schema.Class` whose fields are the core identity brands plus the aggregate brand-in-field; the interior `_Row` model typing the persisted event row; the journal ensure rows the provisioning plane applies and `lane/capability.md` proves.
- Packages: `effect` (`Schema`); `@effect/sql` (`Model`); `@rasm/ts/core` (`AppIdentity`, `TenantContext`).
- Growth: a new stream dimension is a `StreamKey` field plus a column pair in the ensure rows — every keyed surface in the folder re-keys with it because the class is the one spelling of stream identity.
- Law: events are app-authored closed `Schema.TaggedClass` families — the journal stores their encoded form plus the `(tag, eventVersion)` coordinate and never interprets payloads, so a family evolves without touching this page.
- Law: the payload column is `Model.JsonFromString` — TEXT in the database variants, native object in the JSON variants — so the object-versus-text dialect difference is the model's, and no page hand-parses a payload column.
- Law: `sequence` is the global total order (identity column), `version` the per-stream order (the OCC coordinate); both are engine-generated or engine-checked, never computed in process.
- Law: `sequence` is bigint-safe end to end — every process-side read decodes through `Journal.Sequence` (bigint, string, or number driver posture folds to `bigint`), because the global identity column grows unbounded across every stream and a `Number()` coercion past 2^53 silently corrupts checkpoints and joins; per-stream `version` stays number-valued because aggregate cardinality is provably bounded, and it decodes through `Journal.Version` — the number-or-string codec — because a BIGINT column crosses the wire as text on the spine driver and as number on the sqlite profiles.
- Law: `recordedAt` is write time minted by `Model.DateTimeInsert` — domain time lives inside event payloads, and conflating the two is the named defect.
- Boundary: the tenant column is what `Tenancy.rls("journal_event")` predicates over; `Model.makeRepository` is banned on this table — the journal never UPDATEs or DELETEs events, and erasure is `journal/retain.md`'s key destruction.

```typescript
import { Schema } from "effect"
import { Model } from "@effect/sql"
import { AppIdentity, TenantContext } from "@rasm/ts/core"
import type { Capability } from "../lane/capability.ts"

class StreamKey extends Schema.Class<StreamKey>("StreamKey")({
  app: AppIdentity.fields.app,
  tenant: TenantContext.fields.tenant,
  aggregate: Schema.NonEmptyString.pipe(
    Schema.pattern(/^[a-z][a-z0-9-]*\/[A-Za-z0-9._:-]+$/),
    Schema.brand("Aggregate"),
  ),
}) {}

class _Row extends Model.Class<_Row>("JournalEvent")({
  sequence: Model.Generated(Schema.Number),
  app: AppIdentity.fields.app,
  tenant: TenantContext.fields.tenant,
  aggregate: StreamKey.fields.aggregate,
  version: Schema.Number,
  tag: Schema.NonEmptyString,
  event_version: Schema.Int,
  payload: Model.JsonFromString(Schema.Unknown),
  recorded_at: Model.DateTimeInsert,
}) {}

const _journalDdl: Capability.Ensure = {
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
}
```

## [03]-[APPEND_SURFACE]

- Owner: `Occ` — the tagged concurrency expectation; `VersionConflict` — the one domain fault of the write path; `Journal.Sequence` — the bigint sequence codec; `_append` — the locked OCC insert every write funnels through, whose `RETURNING` carries the landed global sequences into the receipt.
- Packages: `effect` (`Effect`, `Array`, `Data`, `Schema`); `@effect/sql` (`SqlClient`, `SqlSchema`, `sql.insert`, `sql.onDialectOrElse`, `sql.reserve`, `SqlError`).
- Entry: `bound.append(stream, events, occ)` — ONE entry whose plural modality is the input shape (`A | NonEmptyReadonlyArray<A>`), never an `appendMany` sibling; standalone it owns its commit, inside `publish` it folds to a savepoint.
- Receipt: `Journal.Receipt` — `{ stream, version, count, first, rows }` — the new head, the appended count, the first written version, and the encoded rows the outbox re-projects, each carrying its landed global `sequence`; the ledger stores it for replay and the publish wake announces the last sequence so drains skip empty cycles.
- Growth: a new write-side invariant is a guard inside `_append`, never a second append; a new event tag costs this page nothing — the plan stamps its `eventVersion` and the union admits it.
- Law: concurrency is `Occ` — `Exact` fails as `VersionConflict` when the locked head disagrees, `None` demands version zero, `Any` serializes under the lock and appends at head; the advisory lock is `pg_advisory_xact_lock(hashtextextended(...))` on the spine and degrades to the single writer through `onDialectOrElse` — the unique constraint remains the structural backstop on every profile.
- Law: the conflict carries evidence — `expected` and `actual` — so recovery is reload-fold-retry as data, and retrying rides a `Schedule` gated on the tag, never a loop.
- Law: `eventVersion` is stamped from `plan.latest(tag)` at write — the write coordinate and the read lift share one anchor; a tag the plan does not know is a defect at the append site, caught before any row is written.
- Law: `Journal.now(sql)` is the one dialect-now fragment — every sibling statement that stamps a timestamp splices it, so the dialect pair exists in exactly one spelling folder-wide.
- RESEARCH: the grant-gated COPY bulk-ingest arm — encoded rows streamed over a `sql.reserve`d connection where `require("copy")` holds, for replay imports and migration-scale ingest only (the OCC lock is the correctness seam COPY cannot honor) — lands once the reserved-connection COPY member spelling is catalogued; until then bulk imports batch through `_append`.
- Boundary: encode faults are `ParseError` on the admission rail; the atomic composition is `[5]`'s.

```typescript
import { Array, Data, Effect, HashMap, Option, type ParseResult } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"
import { Upcast } from "./evolve.ts"

class VersionConflict extends Data.TaggedError("VersionConflict")<{
  readonly stream: StreamKey
  readonly expected: number
  readonly actual: number
}> {}

const _Sequence = Schema.Union(Schema.BigIntFromSelf, Schema.BigInt, Schema.BigIntFromNumber)

const _Version = Schema.Union(Schema.Number, Schema.NumberFromString)

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
    readonly rows: ReadonlyArray<{ readonly sequence: bigint; readonly version: number; readonly tag: string; readonly payload: string }>
  }
}

const _Occ = Data.taggedEnum<Journal.Occ>()

const _Landed = Schema.Struct({ sequence: _Sequence, version: _Version })

const _head = (sql: SqlClient.SqlClient, stream: StreamKey) =>
  SqlSchema.single({
    Request: StreamKey,
    Result: Schema.Struct({ head: _Version }),
    execute: (key) =>
      sql`SELECT coalesce(max(version), 0) AS head FROM journal_event
          WHERE app = ${key.app} AND tenant = ${key.tenant} AND aggregate = ${key.aggregate}`,
  })(stream).pipe(Effect.map((row) => row.head))

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
              event_version: eventVersion,
              payload,
            })))
          const landed = yield* Effect.flatMap(
            sql`INSERT INTO journal_event ${sql.insert(rows)} RETURNING sequence, version`,
            Schema.decodeUnknown(Schema.Array(_Landed)),
          )
          const bySequence = HashMap.fromIterable(Array.map(landed, (row) => [row.version, row.sequence] as const))
          return {
            stream,
            version: held + batch.length,
            count: batch.length,
            first: held + 1,
            rows: Array.map(rows, (row) => ({
              sequence: Option.getOrElse(HashMap.get(bySequence, row.version), () => 0n),
              version: row.version,
              tag: row.tag,
              payload: row.payload,
            })),
          } satisfies Journal.Receipt
        }),
      ))
```

## [04]-[LEDGER_CLAIM]

- Owner: the `idempotency_ledger` ensure row, the `IdempotencyKey` brand, and `_claim` — the one statement that inserts-or-touches and reports first-writer truth plus the stored receipt in a single round trip; `_settle` writes the receipt after the append succeeds.
- Packages: `@effect/sql` (`sql.insert`, `sql.onDialectOrElse`); `effect` (`Option`, `Schema`).
- Receipt: `Journal.Claim` — `{ key, first, held }` — `first` from `(xmax = 0)` on the spine and the `claimed_at = touched_at` stamp equality on the sqlite arm (both defaults evaluate in the inserting statement, so only the conflict update separates them); a replay is served entirely from this row, and the whole claim decodes through one `SqlSchema.single` — the `inserted` flag through the dialect-honest `_Flag` codec, the stored receipt through `Upcast.json(_Receipt)` — so no ledger cell is ever hand-coerced.
- Growth: a new ledger dimension (scope column, expiry class) is a column pair plus a field on the claim row — the statement shape never changes.
- Law: the claim is one statement — `INSERT … ON CONFLICT (key) DO UPDATE SET touched_at = … RETURNING (xmax = 0) AS inserted, receipt` — the spine's `conflictClaim` primitive row realized; a SELECT-then-INSERT pair is the torn spelling.
- Law: the ledger stores the receipt after the append succeeds, so a replayed key returns the ORIGINAL receipt — idempotency means the duplicate caller cannot distinguish itself from the first writer.
- Law: ledger rows age by `touched_at` under a `journal/retain.md` window — a replay past the window is a fresh publish by declaration, and the window is a policy value, never a literal.

```typescript
const _IdempotencyKey = Schema.NonEmptyString.pipe(Schema.maxLength(200), Schema.brand("IdempotencyKey"))

const _Receipt = Schema.Struct({
  stream: StreamKey,
  version: Schema.Number,
  count: Schema.Number,
  first: Schema.Number,
  rows: Schema.Array(Schema.Struct({ sequence: Schema.BigInt, version: Schema.Number, tag: Schema.String, payload: Schema.String })),
})

declare namespace Journal {
  type Key = typeof _IdempotencyKey.Type
  type Claim = {
    readonly key: Key
    readonly first: boolean
    readonly held: Option.Option<Journal.Receipt>
  }
}

const _Flag = Schema.transform(Schema.Union(Schema.Boolean, Schema.Number), Schema.Boolean, {
  strict: true,
  decode: (raw) => raw === true || raw === 1,
  encode: (flag) => flag,
})

const _Claimed = Schema.Struct({
  inserted: _Flag,
  receipt: Schema.OptionFromNullOr(Upcast.json(_Receipt)),
})

const _claim = (sql: SqlClient.SqlClient, stream: StreamKey, key: Journal.Key) =>
  SqlSchema.single({
    Request: Schema.Struct({ key: _IdempotencyKey, app: StreamKey.fields.app, tenant: StreamKey.fields.tenant }),
    Result: _Claimed,
    execute: (row) =>
      sql.onDialectOrElse({
        orElse: () =>
          sql`INSERT INTO idempotency_ledger ${sql.insert([row])}
              ON CONFLICT (key) DO UPDATE SET touched_at = ${_now(sql)}
              RETURNING (claimed_at = touched_at) AS inserted, receipt`,
        pg: () =>
          sql`INSERT INTO idempotency_ledger ${sql.insert([row])}
              ON CONFLICT (key) DO UPDATE SET touched_at = ${_now(sql)}
              RETURNING (xmax = 0) AS inserted, receipt`,
      }),
  })({ key, app: stream.app, tenant: stream.tenant }).pipe(
    Effect.map((row): Journal.Claim => ({ key, first: row.inserted, held: row.receipt })),
  )

const _settle = (sql: SqlClient.SqlClient, key: Journal.Key, receipt: Journal.Receipt) =>
  Effect.flatMap(
    Schema.encode(Schema.parseJson(_Receipt))(receipt),
    (held) => sql`UPDATE idempotency_ledger SET receipt = ${held} WHERE key = ${key}`,
  )

const _ledgerDdl: Capability.Ensure = {
  relation: "idempotency_ledger",
  pg: `CREATE TABLE IF NOT EXISTS idempotency_ledger (
    key TEXT PRIMARY KEY,
    app TEXT NOT NULL, tenant TEXT NOT NULL,
    receipt JSONB,
    claimed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    touched_at TIMESTAMPTZ NOT NULL DEFAULT now());`,
  sqlite: `CREATE TABLE IF NOT EXISTS idempotency_ledger (
    key TEXT PRIMARY KEY,
    app TEXT NOT NULL, tenant TEXT NOT NULL,
    receipt TEXT,
    claimed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    touched_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')));`,
}
```

## [05]-[ATOMIC_PUBLISH]

- Owner: `bound.publish(intent)` — the one write entry apps and edges call; everything the commit must carry is a field of `Journal.Intent`, and the inline projection slots arrive as values, never as imports.
- Packages: `effect` (`Effect`, `Option`); `@effect/sql` (`sql.withTransaction`); `@effect/sql-pg` (`PgClient.notify` — the wake, spine only, read as an optional service).
- Entry: `bound.publish(intent)` runs inside `Tenancy.within` composed by the caller's scope; `intent` carries stream, events, occ, the optional idempotency key, and the slot values the inline projection lane inhabits.
- Receipt: `Journal.Published` — `{ journal, key, replay }` — the append receipt, the claiming key when present, and `replay: true` when the ledger served a duplicate.
- Growth: a new atomic participant is one step inside the transaction fold, never a second publish; a new wake consumer subscribes the channel — the name derives from the app key, parameterized ingress.
- Law: ordering inside the transaction is load-bearing — claim first (a replay short-circuits before any write), append second, outbox third, slots fourth, settle last; NOTIFY issues inside the transaction because the spine delivers it at commit, so a rolled-back publish wakes nobody.
- Law: the NOTIFY payload is the last landed global `sequence` — a drain daemon compares it against its checkpoint and skips the claim transaction when no work exists, so a high-fanout deployment pays zero empty wake cycles; the payload is an accelerator only, and a garbled payload costs one probing cycle, never correctness.
- Law: publish is total over its faults — `VersionConflict`, `SqlError`, `ParseError`; nothing else escapes, and a defect inside a slot dies rather than half-committing because the transaction rolls back whole.
- Law: the reactivity invalidation keys collect from the slots and stamp exactly once per commit — read-your-writes is the slot contract's, restated nowhere.

```mermaid
sequenceDiagram
  participant P as publish(intent)
  participant T as withTransaction
  participant L as ledger
  participant J as journal_event
  participant O as outbox
  participant S as slots
  P->>T: Tenancy.within
  T->>L: claim(key) — (xmax = 0)
  alt replay
    L-->>P: held receipt, replay: true
  else first writer
    T->>J: append(stream, events, occ)
    T->>O: insert deliverable rows
    T->>S: fold + upsert read models
    T->>L: settle(receipt)
    T-->>P: commit — NOTIFY delivers, keys invalidate
  end
```

```typescript
import { PgClient } from "@effect/sql-pg"

declare namespace Journal {
  type Slot<A> = {
    readonly keys: (stream: StreamKey) => Record<string, ReadonlyArray<string>>
    readonly project: (stream: StreamKey, events: ReadonlyArray<A>, receipt: Journal.Receipt) => Effect.Effect<
      void,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
  }
  type Intent<A> = {
    readonly stream: StreamKey
    readonly events: A | Array.NonEmptyReadonlyArray<A>
    readonly occ: Occ
    readonly key: Option.Option<Key>
    readonly slots: ReadonlyArray<Slot<A>>
  }
  type Published = {
    readonly journal: Receipt
    readonly key: Option.Option<Key>
    readonly replay: boolean
  }
}

const _channel = (app: AppIdentity.Key): string => `journal:${app}`

const _deliverables = (stream: StreamKey, receipt: Journal.Receipt) =>
  receipt.rows.map((row) => ({
    app: stream.app,
    tenant: stream.tenant,
    aggregate: stream.aggregate,
    version: row.version,
    tag: row.tag,
    payload: row.payload,
  }))

const _publish = <A extends Journal.Event, I>(spec: Journal.Spec<A, I>) =>
  (intent: Journal.Intent<A>) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      sql.withTransaction(
        Effect.gen(function* () {
          const claim = yield* Effect.transposeOption(
            Option.map(intent.key, (key) => _claim(sql, intent.stream, key)))
          const replay = Option.flatMap(claim, (held) => held.first ? Option.none() : held.held)
          return yield* Option.match(replay, {
            onSome: (held) =>
              Effect.succeed<Journal.Published>({ journal: held, key: intent.key, replay: true }),
            onNone: () =>
              Effect.gen(function* () {
                const journal = yield* _append(spec)(intent.stream, intent.events, intent.occ)
                const batch = Array.ensure(intent.events)
                yield* sql`INSERT INTO outbox ${sql.insert(_deliverables(intent.stream, journal))}`
                yield* Effect.forEach(intent.slots, (slot) => slot.project(intent.stream, batch, journal), { discard: true })
                yield* Effect.transposeOption(Option.map(intent.key, (key) => _settle(sql, key, journal)))
                yield* Effect.transposeOption(
                  Option.map(yield* Effect.serviceOption(PgClient.PgClient), (pg) =>
                    pg.notify(
                      _channel(intent.stream.app),
                      String(Option.match(Array.last(journal.rows), { onNone: () => 0n, onSome: (row) => row.sequence })),
                    )))
                return { journal, key: intent.key, replay: false } satisfies Journal.Published
              }),
          })
        }),
      ))
```

## [06]-[READ_SURFACE]

- Owner: the bound `head` and `read` members — `read` is a backpressured statement stream lifted row-by-row through the evolve plan into live family values.
- Packages: `effect` (`Stream`); `@effect/sql` (`Statement.stream` over the backpressured cursor).
- Entry: `bound.read(stream, window?)` — the one replay road; projection lanes, `journal/retain.md`'s DSAR fold, and snapshot-plus-tail hydration compose it with a `from` window instead of minting their own SELECT.
- Growth: a new read shape (by tag, by time) is a window field, never a sibling read.
- Law: rows leave the statement as the decoded `_EventRow` (payload through `Upcast.Column`) projected into `Upcast.Raw` and exist as nothing else — the decoded family value is the only shape past this seam, so a malformed historical payload surfaces as `ParseError` exactly once, at the lift, and no cursor cell is hand-coerced.

```typescript
import { Stream } from "effect"

const _EventRow = Schema.Struct({
  tag: Schema.String,
  event_version: Schema.Number,
  payload: Upcast.Column,
  version: _Version,
})

const _read = <A extends Journal.Event, I>(spec: Journal.Spec<A, I>) =>
  (stream: StreamKey, window?: { readonly from?: number; readonly to?: number }) =>
    Stream.unwrap(
      Effect.map(SqlClient.SqlClient, (sql) =>
        sql`SELECT tag, event_version, payload, version FROM journal_event
            WHERE app = ${stream.app} AND tenant = ${stream.tenant} AND aggregate = ${stream.aggregate}
              AND version >= ${window?.from ?? 1} AND version <= ${window?.to ?? Number.MAX_SAFE_INTEGER}
            ORDER BY version`.stream.pipe(
          Stream.mapEffect((raw) =>
            Effect.flatMap(Schema.decodeUnknown(_EventRow)(raw), (row) =>
              spec.plan.decode({ tag: row.tag, version: row.event_version, payload: row.payload }))),
        )),
    )
```

## [07]-[RELAY_ROWS]

- Owner: the `outbox` ensure row, the `_Deliverable` model, the two statements the work drain composes — `Journal.claimBatch` (SKIP LOCKED with attempts increment) and `Journal.complete` — the wake channel name, the EventLog overlay bindings, and the assembled `Journal` export.
- Packages: `@effect/sql` (`Model`, `sql.in`, `SqlEventJournal`, `SqlEventLogServer`).
- Entry: the work plane drains through its `SqlClient` port with these statement values — this page publishes the vocabulary, the drain owns fan-out policy, retry budgets, and egress quota; the async projection lane listens on the same channel.
- Growth: a new deliverable dimension (priority, deliver-at) is a column plus a `claimBatch` ORDER BY term — the drain contract never widens.
- Law: `claimBatch` is the competing-consumer claim realizing the `skipLocked` primitive row — attempts increment on every claim so poison rows surface as data, and the visibility-timeout redelivery idiom is the `claimed_at` lease predicate: a claimed row is invisible for `leaseSeconds`, so a crashed claimant's rows redeliver only after the lease lapses and a live claimant is never raced; the sqlite arm serializes on the single writer and drops the lock clause while keeping the lease predicate.
- Law: the overlay bindings are overlay ONLY — the EventLog journal and sync-server storage persist onto this owning `SqlClient`, accelerate local-first reads, and are never the record of truth; a record whose loss corrupts state lives in THIS journal and projects outward, never the reverse.
- Law: `layerStorageSubtle` is the default overlay posture — zero-knowledge storage for the untrusted multi-tenant deployment, where the server persists ciphertext it cannot read; the plain `layerStorage` row is the explicit single-tenant opt-in, selected at the composition root.
- Law: the overlay backings are adopted only while their table bootstrap is verifiably ensure-shaped — idempotent, additive, provision-runnable; otherwise their DDL is owned locally beside these rows and the layers still bind.

```typescript
import { SqlEventJournal, SqlEventLogServer } from "@effect/sql"

class _Deliverable extends Model.Class<_Deliverable>("OutboxRow")({
  id: Model.Generated(Schema.Number),
  app: AppIdentity.fields.app,
  tenant: TenantContext.fields.tenant,
  aggregate: StreamKey.fields.aggregate,
  version: Schema.Number,
  tag: Schema.NonEmptyString,
  payload: Model.JsonFromString(Schema.Unknown),
  attempts: Schema.Int,
  created_at: Model.DateTimeInsert,
  delivered_at: Model.FieldOption(Schema.DateTimeUtc),
}) {}

const _outboxDdl: Capability.Ensure = {
  relation: "outbox",
  pg: `CREATE TABLE IF NOT EXISTS outbox (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version BIGINT NOT NULL, tag TEXT NOT NULL,
    payload JSONB NOT NULL, attempts INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    claimed_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ);
  CREATE INDEX IF NOT EXISTS outbox_pending ON outbox (app, id) WHERE delivered_at IS NULL;`,
  sqlite: `CREATE TABLE IF NOT EXISTS outbox (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    version INTEGER NOT NULL, tag TEXT NOT NULL,
    payload TEXT NOT NULL, attempts INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    claimed_at TEXT,
    delivered_at TEXT);`,
}

const _now = (sql: SqlClient.SqlClient) =>
  sql.onDialectOrElse({
    orElse: () => sql.literal("strftime('%Y-%m-%dT%H:%M:%fZ','now')"),
    pg: () => sql.literal("now()"),
  })

const _overlay = {
  journal: SqlEventJournal.layer,
  server: SqlEventLogServer.layerStorage,
  serverSubtle: SqlEventLogServer.layerStorageSubtle,
} as const

const Journal = {
  of: <A extends Journal.Event, I>(spec: Journal.Spec<A, I>) => ({
    append: _append(spec),
    head: (stream: StreamKey) => Effect.flatMap(SqlClient.SqlClient, (sql) => _head(sql, stream)),
    read: _read(spec),
    publish: _publish(spec),
  }),
  now: _now,
  channel: _channel,
  claimBatch: (sql: SqlClient.SqlClient, app: AppIdentity.Key, take: number, leaseSeconds: number) =>
    sql.onDialectOrElse({
      orElse: () =>
        sql`UPDATE outbox SET attempts = attempts + 1, claimed_at = ${_now(sql)}
            WHERE id IN (SELECT id FROM outbox WHERE app = ${app} AND delivered_at IS NULL
                         AND (claimed_at IS NULL OR claimed_at < strftime('%Y-%m-%dT%H:%M:%fZ','now', '-' || ${leaseSeconds} || ' seconds'))
                         ORDER BY id LIMIT ${take})
            RETURNING *`,
      pg: () =>
        sql`UPDATE outbox SET attempts = attempts + 1, claimed_at = ${_now(sql)}
            WHERE id IN (SELECT id FROM outbox WHERE app = ${app} AND delivered_at IS NULL
                         AND (claimed_at IS NULL OR claimed_at < now() - make_interval(secs => ${leaseSeconds}))
                         ORDER BY id LIMIT ${take} FOR UPDATE SKIP LOCKED)
            RETURNING *`,
    }),
  complete: (sql: SqlClient.SqlClient, ids: ReadonlyArray<number>) =>
    sql`UPDATE outbox SET delivered_at = ${_now(sql)} WHERE ${sql.in("id", ids)}`,
  ddl: [_journalDdl, _ledgerDdl, _outboxDdl],
  overlay: _overlay,
  Occ: _Occ,
  Key: _IdempotencyKey,
  Sequence: _Sequence,
  Version: _Version,
  Conflict: VersionConflict,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Journal, StreamKey }
```
