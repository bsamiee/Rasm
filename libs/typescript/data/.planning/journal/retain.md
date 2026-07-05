# [DATA_RETAIN]

Lawful data aging without rewriting: the log is append-only forever, so this page owns the three ways data ages — retention-class windows (one policy table driving ledger expiry, outbox grooming, fact grooming, and partition drop behind the causal frontier), crypto-shredding (subject-bearing payload fields sealed under a per-subject data key whose wrapped form is the ONLY thing this folder stores; erasure is destroying the `WrappedKey`, after which unwrap fails, open becomes impossible, and every sealed read folds to a redaction marker, totally), and the per-subject DSAR export fold (one portability read over journal events and object references riding the same subject spine erasure uses). The stability frontier arrives from the core causality owner as a value; partitions at or below a snapshotted frontier drop through the `partition` grant — compaction is a capability, never a default.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         |
| :-----: | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `RETENTION_ROWS` | the retention-class vocabulary, window policy, frontier handoff, partition rows     |
|  [02]   | `SHREDDER`       | the wrapped-key ledger, seal/open folds, erasure as key destruction                 |
|  [03]   | `DSAR_EXPORT`    | the per-subject portability fold over journal plus object rows                      |

## [2]-[RETENTION_ROWS]

- Owner: the `Retain.Class` vocabulary — one `as const` key tuple feeding `Schema.Literal` plus the window-row table, so wire admission and the type derive from one anchor pair — plus the frontier ledger recording the causal handoff and the partition rows that realize aging on the spine.
- Packages: `effect` (`Duration`, `Schema`); `@effect/sql`; `@rasm/ts/core` (`Causal.Retention` — the `{floor, stamp}` compaction coordinate); the `partition` and `cron` grants gate execution.
- Entry: every aging consumer reads one vocabulary — object references store a class key, ledger and outbox grooming read `Retain.Policy[clazz].window`, `journal/fact.md` keys its fact streams by the same classes, and the scheduled maintenance rows execute the drops; no window literal exists outside this table.
- Growth: a new class is one row — every sweep, groom, and lifecycle rule inherits it; a new aging surface reads the table, never mints a window.
- Law: the journal itself never ages by wall clock — partition drop is lawful only at-or-below a frontier the causality owner finalized AND a snapshot at-or-above it exists; the frontier ledger records the `Causal.Retention` handoff, and the drop statement generates from it, so compaction can never orphan unreplayable history.
- Law: partitioning is a `pg_partman` image fact — the ensure registers `journal_event` as a partitioned parent only where the `partition` grant holds; the sqlite profiles never partition (their compaction is the export-snapshot-and-truncate posture `lane/sqlite.md` owns).
- Law: ledger, outbox, and fact grooming are ordinary deletes on non-truth tables — past their class windows, run as scheduled maintenance rows reading this vocabulary; the `incremental` grant upgrades large grooms to exactly-once checkpointed batch folds where it probes true.

```typescript
import { Duration, Schema } from "effect"
import type { Capability } from "../lane/capability.ts"

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
}

const _frontierDdl: Capability.Ensure = {
  relation: "retain_frontier",
  pg: `CREATE TABLE IF NOT EXISTS retain_frontier (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    floor JSONB NOT NULL,
    stamp TEXT NOT NULL,
    snapshotted BIGINT NOT NULL,
    handed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (app, tenant, aggregate));`,
  sqlite: `CREATE TABLE IF NOT EXISTS retain_frontier (
    app TEXT NOT NULL, tenant TEXT NOT NULL, aggregate TEXT NOT NULL,
    floor TEXT NOT NULL,
    stamp TEXT NOT NULL,
    snapshotted INTEGER NOT NULL,
    handed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (app, tenant, aggregate));`,
}
```

## [3]-[SHREDDER]

- Owner: the `subject_key` ledger holding one `WrappedKey` per subject, the `seal`/`open` folds composing the security `Shredder`'s five-verb envelope algebra, and `erase` — the one erasure verb, destroying the wrapped key material and marking the tombstone in a single statement.
- Packages: `@rasm/ts/security` (`Shredder`, `WrappedKey`, `SealedEnvelope` — the one direct `data → security` edge); `effect` (`Effect`, `Option`, `Schema`).
- Entry: an app seals subject-bearing fields at construction — `Retain.seal(subject, bytes)` before the payload enters the publish transaction; reads meeting sealed fields call `Retain.open(subject, envelope)` and receive `Option<bytes>` — `none` IS the erased state, folded by the consumer into its redaction marker.
- Receipt: `erase` returns `Option<{ subject, destroyedAt }>` — some is the auditable tombstone the fact stream records, none means no live key existed; the log bytes remain, provably unreadable either way.
- Growth: a new custody posture (a KMS-held KEK) is a security-side construction row — this ledger stores whatever `WrappedKey` the Shredder wraps, so custody changes never touch this page.
- Law: the ledger stores ONLY the wrapped form — `Shredder.mint` issues the data key, `wrap` seals it under the master KEK, and the raw `CryptoKey` never crosses this seam; `seal` is one atomic upsert realizing the `conflictClaim` primitive: the fresh mint inserts, a concurrent or replayed subject keeps the stored wrapped key through the `coalesce` arm and the loser seals under the winner's key by unwrapping the RETURNING row, and a destroyed subject resurrects under a NEW key (old envelopes stay unreadable forever) because the `CASE` arm clears the tombstone only when `wrapped` was NULL.
- Law: `open` is total — a destroyed or absent key folds to `Option.none`, never a fault, because erasure is a lawful state every reader renders, not an error to recover from; a genuine unwrap failure on live material is the security fault it already is.
- Law: erasure is key destruction ONLY — `UPDATE subject_key SET wrapped = NULL, destroyed_at = …` — no journal row is touched, no payload rewritten; the append-only invariant survives the right to erasure because unreadable IS erased.
- Law: the envelope travels as `SealedEnvelope` — IV and ciphertext as opaque encoded bytes inside the payload field; the field is `Model.Sensitive`-classed on any projection row so the sealed form never leaks into a JSON variant.

```typescript
import { Effect, Option } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"
import { SealedEnvelope, Shredder, WrappedKey } from "@rasm/ts/security"
import { Journal } from "./append.ts"

declare namespace Retain {
  type Tombstone = {
    readonly subject: Subject
    readonly destroyedAt: string
  }
}

const _WrappedRow = Schema.Struct({ wrapped: Schema.NullOr(Schema.Uint8ArrayFromSelf) })

const _subjectDdl: Capability.Ensure = {
  relation: "subject_key",
  pg: `CREATE TABLE IF NOT EXISTS subject_key (
    subject TEXT PRIMARY KEY,
    wrapped BYTEA,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    destroyed_at TIMESTAMPTZ);`,
  sqlite: `CREATE TABLE IF NOT EXISTS subject_key (
    subject TEXT PRIMARY KEY,
    wrapped BLOB,
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    destroyed_at TEXT);`,
}

const _dataKey = (shredder: Shredder, subject: Retain.Subject) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const found = SqlSchema.findOne({
      Request: _Subject,
      Result: _WrappedRow,
      execute: (who) => sql`SELECT wrapped FROM subject_key WHERE subject = ${who} AND destroyed_at IS NULL`,
    })
    return yield* Effect.transposeOption(
      Option.map(
        Option.flatMapNullable(yield* found(subject), (row) => row.wrapped),
        (wrapped) => shredder.unwrap(new WrappedKey({ wrapped })),
      ))
  })

const _seal = (shredder: Shredder, subject: Retain.Subject, bytes: Uint8Array) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const minted = yield* shredder.mint()
    const wrapped = yield* shredder.wrap(minted)
    const sealed = SqlSchema.single({
      Request: Schema.Struct({ subject: _Subject, wrapped: Schema.Uint8ArrayFromSelf }),
      Result: Schema.Struct({ wrapped: Schema.Uint8ArrayFromSelf }),
      execute: (row) =>
        sql`INSERT INTO subject_key ${sql.insert([row])}
            ON CONFLICT (subject) DO UPDATE
            SET wrapped = coalesce(subject_key.wrapped, excluded.wrapped),
                destroyed_at = CASE WHEN subject_key.wrapped IS NULL THEN NULL ELSE subject_key.destroyed_at END
            RETURNING wrapped`,
    })
    const held = yield* sealed({ subject, wrapped: wrapped.wrapped })
    const dataKey = yield* shredder.unwrap(new WrappedKey({ wrapped: held.wrapped }))
    return yield* shredder.seal(dataKey, bytes)
  })

const _open = (shredder: Shredder, subject: Retain.Subject, envelope: SealedEnvelope) =>
  Effect.flatMap(_dataKey(shredder, subject), (held) =>
    Effect.transposeOption(Option.map(held, (key) => shredder.open(key, envelope))))

const _erase = (subject: Retain.Subject) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.map(
      SqlSchema.findOne({
        Request: _Subject,
        Result: Schema.Struct({ subject: _Subject, destroyed_at: Schema.String }),
        execute: (who) =>
          sql`UPDATE subject_key SET wrapped = NULL, destroyed_at = ${Journal.now(sql)}
              WHERE subject = ${who} AND destroyed_at IS NULL
              RETURNING subject, destroyed_at`,
      })(subject),
      Option.map((row) => ({ subject: row.subject, destroyedAt: row.destroyed_at }) satisfies Retain.Tombstone),
    ))
```

## [4]-[DSAR_EXPORT]

- Owner: `Retain.dsar` — the one portability fold: every journal event indexed to the subject, joined with the subject's object references, streamed as one export document; sealed fields inside payloads stay sealed here — the exporting consumer composes `Retain.open` per field it knows the shape of, because field shapes are app material; plus the subject-index slot this page provides to the publish transaction.
- Packages: `effect` (`Stream`); `journal/append.md` (the read stream and the `Slot` contract); the object plane's reference rows arrive by relation name.
- Entry: the subject index is written at publish time — a `Journal.Slot` provided by this page stamps `(subject, sequence)` rows for subject-bearing events, so the DSAR read is an index scan, never a full-log crawl.
- Growth: a new export surface (object bytes bundled, format variants) is a projection of the same fold — the subject spine never changes.
- Law: the export and the erasure share one spine — the same `subject_journal` index that finds events to export finds nothing to rewrite on erasure, proving the two rights compose: export reads what remains readable, erasure makes fields unreadable, and both leave the log bytes untouched.
- Law: the fold is streaming and decoded — journal rows and object references emit incrementally to the egress sink through `Result` schemas (`payload` through `Upcast.Column`), so a large subject exports in bounded memory, a malformed row quarantines as `ParseError`, and no export cell is hand-coerced; the `subject_journal.sequence` join runs engine-side against the BIGINT column, so no sequence value crosses the process untyped.
- Law: sensitive projection columns never enter the export — the `Model.Sensitive` field class strips them from every JSON variant by construction; sealed payload fields export opened only where the consuming exporter composes `Retain.open` against a live key, and an erased subject's fields export as the redaction marker the `Option.none` fold names.

```typescript
import { Stream, type ParseResult } from "effect"
import { Upcast } from "./evolve.ts"

declare namespace Retain {
  type Entry = {
    readonly tag: string
    readonly version: number
    readonly payload: unknown
    readonly recordedAt: string
  }
  type Export = {
    readonly subject: Subject
    readonly events: Stream.Stream<Entry, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
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

const _EntryRow = Schema.Struct({
  tag: Schema.String,
  event_version: Schema.Number,
  payload: Upcast.Column,
  recorded_at: Schema.String,
})

const _RefRow = Schema.Struct({ key: Schema.String, retention: _Class })

const _dsar = (subject: Retain.Subject): Retain.Export => ({
  subject,
  events: Stream.unwrap(
    Effect.map(SqlClient.SqlClient, (sql) =>
      sql`SELECT e.tag, e.event_version, e.payload, e.recorded_at FROM journal_event e
          JOIN subject_journal s ON s.sequence = e.sequence
          WHERE s.subject = ${subject} ORDER BY e.sequence`.stream.pipe(
        Stream.mapEffect((raw) =>
          Effect.map(Schema.decodeUnknown(_EntryRow)(raw), (row): Retain.Entry => ({
            tag: row.tag,
            version: row.event_version,
            payload: row.payload,
            recordedAt: row.recorded_at,
          }))),
      )),
  ),
  objects: Effect.flatMap(SqlClient.SqlClient, (sql) =>
    SqlSchema.findAll({
      Request: _Subject,
      Result: _RefRow,
      execute: (who) => sql`SELECT key, retention FROM object_ref WHERE owner = ${who} AND released_at IS NULL`,
    })(subject)),
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
