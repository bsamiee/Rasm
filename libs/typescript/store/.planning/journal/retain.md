# [STORE_RETAIN]

Retention without rewriting: the log is append-only forever, so this page owns the three lawful ways data ages — retention policy rows (class-keyed windows driving ledger expiry, outbox grooming, partition drop behind the causal frontier), crypto-shredding (subject-bearing payload fields sealed under a per-subject key through the `security/sign` `Shredder`; erasure is key destruction and every sealed read thereafter folds to a redaction marker, totally), and the per-subject DSAR export fold (one portability read over journal events and object references riding the same subject spine erasure uses). The stability frontier arrives from `state/causal` as a value; partitions at or below a snapshotted frontier drop through `pg_partman` rows gated on the `"partition"` grant — compaction is a capability, never a default.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         |
| :-----: | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `RETENTION_ROWS` | the retention-class vocabulary, window policy, frontier handoff, partition rows     |
|  [02]   | `SHREDDER`       | the subject-key ledger, seal/open folds, erasure as key destruction                 |
|  [03]   | `DSAR_EXPORT`    | the per-subject portability fold over journal plus object rows                      |

## [2]-[RETENTION_ROWS]

- Owner: the `Retain.Class` vocabulary — one `as const` key tuple feeding `Schema.Literal` plus the window-row table, so the wire admission and the type derive from one anchor pair — plus the frontier ledger and the partition rows that realize aging on the pg spine.
- Packages: `effect` (`Duration`); `@effect/sql`; `capability/matrix.md` grants (`"partition"`, `"cron"`).
- Entry: every aging consumer reads one vocabulary — `object_ref.retention` stores a class key, ledger grooming reads `Retain.Policy[clazz].window`, and `project/rebuild.md`'s scheduled rows execute the drops; no window literal exists outside this table.
- Growth: a new class is one row — every sweep, groom, and lifecycle rule inherits it; a new aging surface reads the table, never mints a window.
- Law: the journal itself never ages by wall clock — partition drop is lawful only at-or-below a frontier that `state/causal` finalized AND a snapshot at-or-above it exists; the frontier ledger records the handoff, and the drop statement is generated from it, so compaction can never orphan unreplayable history.
- Law: partitioning is a `pg_partman` image fact — the ensure registers `journal_event` as a partitioned parent only where the `"partition"` grant holds; the sqlite lanes never partition (their compaction is the `export`-snapshot-and-truncate posture `lane/sqlite.md` owns).
- Law: ledger and outbox grooming are ordinary deletes on non-truth tables — `idempotency_ledger` past its class window, delivered `outbox` rows past theirs; both run as `project/rebuild.md` scheduled rows reading this vocabulary.

```typescript
import { Duration, Schema } from "effect"
import type { Capability } from "../capability/row.ts"

const _classes = ["ephemeral", "operational", "regulatory", "permanent"] as const

const _Policy = {
  ephemeral: { window: Duration.decode("7 days") },
  operational: { window: Duration.decode("90 days") },
  regulatory: { window: Duration.decode("2555 days") },
  permanent: { window: Duration.infinity },
} as const

const _Class = Schema.Literal(..._classes)

const _Subject = Schema.NonEmptyString.pipe(Schema.maxLength(200), Schema.brand("Subject"))

declare namespace Retain {
  type Class = (typeof _classes)[number]
  type Row = (typeof _Policy)[Class]
  type Subject = typeof _Subject.Type
  type _Rows<T extends Record<Class, { readonly window: Duration.Duration }> = typeof _Policy> = T
  type _Keys<K extends Class = keyof typeof _Policy> = K
}

const _frontierDdl: Capability.Ensure = {
  relation: "retain_frontier",
  pg: `CREATE TABLE IF NOT EXISTS retain_frontier (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    frontier BIGINT NOT NULL,
    snapshotted BIGINT NOT NULL,
    handed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (app, tenant, aggregate));`,
  sqlite: `CREATE TABLE IF NOT EXISTS retain_frontier (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    frontier INTEGER NOT NULL,
    snapshotted INTEGER NOT NULL,
    handed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (app, tenant, aggregate));`,
}
```

## [3]-[SHREDDER]

- Owner: the `subject_key` ledger, the `seal`/`open` folds composing the `security/sign` AES-GCM envelope primitive, and `erase` — the one erasure verb, which destroys key material and marks the tombstone in a single transaction.
- Packages: `@rasm/ts/security` (`Shredder` — the one direct `store → security` edge, node-only subpath); `effect` (`Option`, `Redacted`).
- Entry: an app marks subject-bearing events by sealing their sensitive fields at construction — `Retain.seal(subject, bytes)` before the payload enters `Outbox.publish`; reads that meet sealed fields call `Retain.open(subject, sealed)` and receive `Option<bytes>` — `none` IS the erased state, folded by the consumer into its redaction marker.
- Receipt: `erase` returns `Option<{ subject, destroyedAt }>` — some is the auditable tombstone `telemetry`'s audit stream records, none means no live key existed to destroy; the log bytes remain, provably unreadable either way.
- Growth: a new key custody posture (KMS envelope over the subject key) is a `security/sign` provider row — this ledger stores whatever sealed key material the Shredder mints, so custody changes never touch this page.
- Law: `open` is total — a destroyed or absent key folds to `Option.none`, never a fault, because erasure is a lawful state every reader must render, not an error to recover from.
- Law: erasure is key destruction ONLY — `UPDATE subject_key SET material = NULL, destroyed_at = now()` — no journal row is touched, no payload rewritten; the append-only invariant survives the right to erasure because unreadable is erased.
- Law: key material rests sealed — the row stores the Shredder's envelope output, never raw key bytes, and travels as `Redacted` in process.

```typescript
import { Effect, Option, Redacted } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { Shredder } from "@rasm/ts/security"
import { Journal } from "./append.ts"
import { Upcast } from "./upcast.ts"

declare namespace Retain {
  type Tombstone = {
    readonly subject: Subject
    readonly destroyedAt: string
  }
}

const _subjectDdl: Capability.Ensure = {
  relation: "subject_key",
  pg: `CREATE TABLE IF NOT EXISTS subject_key (
    subject TEXT PRIMARY KEY,
    material BYTEA,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    destroyed_at TIMESTAMPTZ);`,
  sqlite: `CREATE TABLE IF NOT EXISTS subject_key (
    subject TEXT PRIMARY KEY,
    material BLOB,
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    destroyed_at TEXT);`,
}

const _material = Schema.decodeUnknown(Schema.Uint8ArrayFromSelf)

const _seal = (shredder: Shredder, subject: Retain.Subject, bytes: Uint8Array) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const rows = yield* sql`SELECT material FROM subject_key WHERE subject = ${subject} AND destroyed_at IS NULL`
    const material = rows[0] === undefined
      ? yield* Effect.tap(shredder.mint, (minted) =>
          sql`INSERT INTO subject_key ${sql.insert([{ subject, material: Redacted.value(minted) }])}
              ON CONFLICT (subject) DO NOTHING`)
      : Redacted.make(yield* _material(rows[0]["material"]))
    return yield* shredder.seal(material, bytes)
  })

const _open = (shredder: Shredder, subject: Retain.Subject, sealed: Uint8Array) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const rows = yield* sql`SELECT material FROM subject_key WHERE subject = ${subject} AND destroyed_at IS NULL`
    if (rows[0] === undefined) return Option.none<Uint8Array>()
    return Option.some(yield* shredder.open(Redacted.make(yield* _material(rows[0]["material"])), sealed))
  })

const _erase = (subject: Retain.Subject) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.map(
      sql`UPDATE subject_key SET material = NULL, destroyed_at = ${Journal.now(sql)}
          WHERE subject = ${subject} AND destroyed_at IS NULL
          RETURNING subject, destroyed_at`.values,
      (cells) =>
        Option.map(Option.fromNullable(cells[0]), (cell) =>
          ({ subject, destroyedAt: String(cell[1]) }) satisfies Retain.Tombstone),
    ))
```

## [4]-[DSAR_EXPORT]

- Owner: `Retain.dsar` — the one portability fold: every journal event indexed to the subject, opened where sealed, joined with the subject's object references, streamed as one export document.
- Packages: `effect` (`Stream`); `journal/append.md` (`Journal.Bound.read` per indexed stream); `object/key.md`'s `object_ref` rows.
- Entry: the subject index is written at publish time — an `Outbox.Slot` provided by this page stamps `(subject, sequence)` rows for subject-bearing events, so the DSAR read is an index scan, never a full-log crawl.
- Growth: a new export surface (object bytes bundled, format variants) is a projection of the same fold — the subject spine never changes.
- Law: the export and the erasure share one spine — the same `subject_journal` index that finds events to export finds nothing to rewrite on erasure, proving the two rights compose: export reads what remains readable, erasure makes fields unreadable, and both leave the log bytes untouched.
- Law: the fold is streaming — journal rows and object references emit incrementally (`Stream` all the way to the egress sink), so a large subject exports in bounded memory.

```typescript
import { Stream, type ParseResult } from "effect"

declare namespace Retain {
  type Fact = {
    readonly tag: string
    readonly version: number
    readonly payload: unknown
    readonly recordedAt: string
  }
  type Export = {
    readonly subject: Subject
    readonly events: Stream.Stream<Fact, SqlError.SqlError, SqlClient.SqlClient>
    readonly objects: Effect.Effect<
      ReadonlyArray<{ readonly key: string; readonly retention: Class }>,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
  }
}

const _subjectIndexDdl: Capability.Ensure = {
  relation: "subject_journal",
  pg: `CREATE TABLE IF NOT EXISTS subject_journal (
    subject TEXT NOT NULL, sequence BIGINT NOT NULL,
    PRIMARY KEY (subject, sequence));`,
  sqlite: `CREATE TABLE IF NOT EXISTS subject_journal (
    subject TEXT NOT NULL, sequence INTEGER NOT NULL,
    PRIMARY KEY (subject, sequence));`,
}

const _dsar = (subject: Retain.Subject): Retain.Export => ({
  subject,
  events: Stream.unwrap(
    Effect.map(SqlClient.SqlClient, (sql) =>
      sql`SELECT e.tag, e.event_version, e.payload, e.recorded_at FROM journal_event e
          JOIN subject_journal s ON s.sequence = e.sequence
          WHERE s.subject = ${subject} ORDER BY e.sequence`.stream.pipe(
        Stream.map((row): Retain.Fact => ({
          tag: String(row["tag"]),
          version: Number(row["event_version"]),
          payload: Upcast.body(row["payload"]),
          recordedAt: String(row["recorded_at"]),
        })),
      )),
  ),
  objects: Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.flatMap(
      sql`SELECT key, retention FROM object_ref WHERE owner = ${subject} AND released_at IS NULL`,
      Effect.forEach((row) =>
        Effect.map(Schema.decodeUnknown(_Class)(row["retention"]), (retention) => ({ key: String(row["key"]), retention }))),
    )),
})

const Retain = {
  Class: _Class,
  Subject: _Subject,
  Policy: _Policy,
  seal: _seal,
  open: _open,
  erase: _erase,
  dsar: _dsar,
  ddl: [_frontierDdl, _subjectDdl, _subjectIndexDdl],
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Retain }
```
