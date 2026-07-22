# [DATA_RETAIN]

Lawful data aging without rewriting: the log is append-only forever, so this page owns the three ways data ages — retention-class windows (one policy table driving ledger expiry, outbox grooming, fact grooming, and partition drop behind the causal frontier), crypto-shredding (subject-bearing payload fields sealed under a per-subject data key whose wrapped form is the ONLY thing this folder stores; erasure is destroying the `WrappedKey`, after which unwrap fails, open becomes impossible, and every sealed read folds to a redaction marker, totally), and the per-subject DSAR export fold (one portability read over journal events and object references riding the same subject spine erasure uses). The stability frontier arrives from the core causality owner as a value; partitions at or below a snapshotted frontier drop through the `partition` grant — compaction is a capability, never a default.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                          |
| :-----: | :--------------- | :------------------------------------------------------------------------------ |
|  [01]   | `RETENTION_ROWS` | the retention-class vocabulary, window policy, frontier handoff, partition rows |
|  [02]   | `SHREDDER`       | the wrapped-key ledger, seal/open folds, erasure as key destruction             |
|  [03]   | `DSAR_EXPORT`    | the per-subject portability fold over journal plus object rows                  |

## [02]-[RETENTION_ROWS]

- Owner: the `Retain.Class` vocabulary — one `as const` key tuple feeding `Schema.Literal` plus the window-row table, so wire admission and the type derive from one anchor pair — plus the frontier ledger recording the causal handoff and the partition rows that realize aging on the spine.
- Packages: `effect` (`Duration`, `Schema`); `@effect/sql`; `@rasm/ts/core` (`Causal.Retention` — the `{floor, stamp}` compaction coordinate); the `partition` and `cron` grants gate execution.
- Entry: every aging consumer reads one vocabulary — object references store a class key, ledger and outbox grooming read `Retain.Policy[clazz].window`, `journal/fact.md` keys its fact streams by the same classes, and the scheduled maintenance rows execute the drops; no window literal exists outside this table.
- Growth: a new class is one row — every sweep, groom, and lifecycle rule inherits it; a new aging surface reads the table, never mints a window.
- Law: the journal itself never ages by wall clock — partition drop is lawful only at-or-below a frontier the causality owner finalized AND a snapshot at-or-above it exists; `Retain.handoff` records the `Causal.Retention` coordinate into the frontier ledger under a monotonic guard (a stale handoff commits nothing), the drop statement generates from the recorded row, and compaction can never orphan unreplayable history.
- Law: partitioning is a `pg_partman` image fact — the ensure registers `journal_event` as a partitioned parent only where the `partition` grant holds, and the drop itself is the granted maintenance row `lane/postgres.md` gates; the sqlite profiles never partition (their compaction is the export-snapshot-and-truncate posture `lane/sqlite.md` owns).
- Law: ledger, outbox, and fact grooming are ordinary deletes on non-truth tables — `Retain.groom(relation, column)` sweeps every finite class window in one pass, the interval arithmetic spelled once per dialect through `sql.onDialectOrElse`, and `permanent` folds to a no-op by `Duration.isFinite`; the `incremental` grant upgrades large grooms to exactly-once checkpointed batch folds where it probes true.
- RESEARCH: the `pg_partman` retention-apply statement pair — the `part_config` retention update the frontier row keys and the maintenance invocation that drops eligible partitions — is catalogued against the extension surface before the drop fence settles; until then the frontier ledger row is the sole drop input and no partition drops outside the granted maintenance row.

```typescript signature
import { Duration, Effect, Schema } from "effect"
import { SqlClient } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal, StreamKey } from "./append.ts"

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

const _floorJson = Schema.encode(Schema.parseJson(Schema.Unknown))

const _handoff = (
  stream: StreamKey,
  frontier: { readonly floor: unknown; readonly stamp: string; readonly snapshotted: number },
) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const floor = yield* _floorJson(frontier.floor)
    yield* sql`INSERT INTO retain_frontier ${sql.insert([{
      app: stream.app,
      tenant: stream.tenant,
      aggregate: stream.aggregate,
      floor,
      stamp: frontier.stamp,
      snapshotted: frontier.snapshotted,
    }])} ON CONFLICT (app, tenant, aggregate) DO UPDATE
      SET floor = excluded.floor, stamp = excluded.stamp, snapshotted = excluded.snapshotted, handed_at = ${Journal.now(sql)}
      WHERE excluded.snapshotted > retain_frontier.snapshotted`
  })

const _groom = (relation: string, column: string) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.forEach(_classes, (clazz) => {
      const window = _Policy[clazz].window
      return Duration.isFinite(window)
        ? sql.onDialectOrElse({
            orElse: () =>
              sql`DELETE FROM ${sql(relation)} WHERE retention = ${clazz}
                  AND ${sql(column)} < datetime('now', ${`-${Math.trunc(Duration.toSeconds(window))} seconds`})`,
            pg: () =>
              sql`DELETE FROM ${sql(relation)} WHERE retention = ${clazz}
                  AND ${sql(column)} < now() - make_interval(secs => ${Duration.toSeconds(window)})`,
          })
        : Effect.void
    }, { concurrency: 1, discard: true }))
```

## [03]-[SHREDDER]

- Owner: `SubjectKey`, the `(app, tenant, subject)` custody identity; the `subject_key` ledger holding one `WrappedKey` per key; the `seal`/`open` folds composing the security `Shredder` envelope algebra; and `erase`, destroying the wrapped key material and marking the tombstone in one statement.
- Packages: `@rasm/ts/security` (`Shredder`, `WrappedKey`, `SealedEnvelope` — the one direct `data → security` edge); `effect` (`Effect`, `Option`, `Schema`).
- Entry: an app seals subject-bearing fields at construction — `Retain.seal(key, bytes)` before the payload enters the publish transaction; reads meeting sealed fields call `Retain.open(key, envelope)` and receive `Option<bytes>` — `none` IS the erased state, folded by the consumer into its redaction marker.
- Receipt: `erase` returns `Option<{ subject: SubjectKey, destroyedAt }>` — some is the auditable tombstone the fact stream records, none means no live key existed; the log bytes remain, provably unreadable either way.
- Growth: a new custody posture (a KMS-held KEK) is a security-side construction row — this ledger stores whatever `WrappedKey` the Shredder wraps, so custody changes never touch this page.
- Law: custody is tenant-scoped structurally — every lookup, upsert, erase, subject-index row, and DSAR scan keys on `(app, tenant, subject)`; equal subject strings in two tenants never share key material or export rows, and no ambient RLS setting substitutes for the composite identity.
- Law: the ledger stores ONLY the wrapped form — `Shredder.mint` issues the data key, `wrap` seals it under the master KEK, and the raw `CryptoKey` never crosses this seam; `seal` is one atomic upsert realizing the `conflictClaim` primitive: the fresh mint inserts, a concurrent or replayed subject keeps the stored wrapped key through the `coalesce` arm and the loser seals under the winner's key by unwrapping the RETURNING row, and a destroyed subject resurrects under a NEW key (old envelopes stay unreadable forever) because the `CASE` arm clears the tombstone only when `wrapped` was NULL.
- Law: `open` is total — a destroyed or absent key folds to `Option.none`, never a fault, because erasure is a lawful state every reader renders, not an error to recover from; a genuine unwrap failure on live material is the security fault it already is.
- Law: erasure is key destruction ONLY — `UPDATE subject_key SET wrapped = NULL, destroyed_at = …` — no journal row is touched, no payload rewritten; the append-only invariant survives the right to erasure because unreadable IS erased.
- Law: the tombstone carries the `rasm.data.retain.erase` observe point — a landed erasure fans the app-armed taps with the tenant-scoped subject coordinate after the destroying statement returns, so compliance observers subscribe to the fact instead of instrumenting this fold, and an absent registry costs nothing.
- Law: the envelope travels as `SealedEnvelope` — IV and ciphertext as opaque encoded bytes inside the payload field; the field is `Model.Sensitive`-classed on any projection row so the sealed form never leaks into a JSON variant.

```typescript signature
import { Effect, Option } from "effect"
import { SqlClient, SqlSchema, type SqlError } from "@effect/sql"
import { SealedEnvelope, Shredder, WrappedKey } from "@rasm/ts/security"
import { Hook, Journal } from "./append.ts"

declare namespace Retain {
  type Tombstone = {
    readonly subject: SubjectKey
    readonly destroyedAt: string
  }
}

class SubjectKey extends Schema.Class<SubjectKey>("SubjectKey")({
  app: StreamKey.fields.app,
  tenant: StreamKey.fields.tenant,
  subject: _Subject,
}) {
  get owner(): string {
    return `subject:${encodeURIComponent(this.app)}:${encodeURIComponent(this.tenant)}:${encodeURIComponent(this.subject)}`
  }
}

const _WrappedRow = Schema.Struct({ wrapped: Schema.NullOr(Schema.Uint8ArrayFromSelf) })

const _subjectDdl: Capability.Ensure = {
  relation: "subject_key",
  pg: `CREATE TABLE IF NOT EXISTS subject_key (
    app TEXT NOT NULL, tenant TEXT NOT NULL, subject TEXT NOT NULL,
    wrapped BYTEA,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    destroyed_at TIMESTAMPTZ,
    PRIMARY KEY (app, tenant, subject));`,
  sqlite: `CREATE TABLE IF NOT EXISTS subject_key (
    app TEXT NOT NULL, tenant TEXT NOT NULL, subject TEXT NOT NULL,
    wrapped BLOB,
    created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    destroyed_at TEXT,
    PRIMARY KEY (app, tenant, subject));`,
}

const _dataKey = (shredder: Shredder, key: SubjectKey) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const found = SqlSchema.findOne({
      Request: SubjectKey,
      Result: _WrappedRow,
      execute: (who) =>
        sql`SELECT wrapped FROM subject_key
            WHERE app = ${who.app} AND tenant = ${who.tenant} AND subject = ${who.subject} AND destroyed_at IS NULL`,
    })
    return yield* Effect.transposeOption(
      Option.map(
        Option.flatMapNullable(yield* found(key), (row) => row.wrapped),
        (wrapped) => shredder.unwrap(new WrappedKey({ wrapped })),
      ))
  })

const _seal = (shredder: Shredder, key: SubjectKey, bytes: Uint8Array) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const minted = yield* shredder.mint()
    const wrapped = yield* shredder.wrap(minted)
    const sealed = SqlSchema.single({
      Request: Schema.Struct({ ...SubjectKey.fields, wrapped: Schema.Uint8ArrayFromSelf }),
      Result: Schema.Struct({ wrapped: Schema.Uint8ArrayFromSelf }),
      execute: (row) =>
        sql`INSERT INTO subject_key ${sql.insert([row])}
            ON CONFLICT (app, tenant, subject) DO UPDATE
            SET wrapped = coalesce(subject_key.wrapped, excluded.wrapped),
                destroyed_at = CASE WHEN subject_key.wrapped IS NULL THEN NULL ELSE subject_key.destroyed_at END
            RETURNING wrapped`,
    })
    const held = yield* sealed({ app: key.app, tenant: key.tenant, subject: key.subject, wrapped: wrapped.wrapped })
    const dataKey = yield* shredder.unwrap(new WrappedKey({ wrapped: held.wrapped }))
    return yield* shredder.seal(dataKey, bytes)
  })

const _open = (shredder: Shredder, key: SubjectKey, envelope: SealedEnvelope) =>
  Effect.flatMap(_dataKey(shredder, key), (held) =>
    Effect.transposeOption(Option.map(held, (key) => shredder.open(key, envelope))))

const _erase = (key: SubjectKey) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.map(
      SqlSchema.findOne({
        Request: SubjectKey,
        Result: Schema.Struct({ ...SubjectKey.fields, destroyed_at: Schema.String }),
        execute: (who) =>
          sql`UPDATE subject_key SET wrapped = NULL, destroyed_at = ${Journal.now(sql)}
              WHERE app = ${who.app} AND tenant = ${who.tenant} AND subject = ${who.subject} AND destroyed_at IS NULL
              RETURNING app, tenant, subject, CAST(destroyed_at AS TEXT) AS destroyed_at`,
      })(key),
      Option.map((row) => ({
        subject: new SubjectKey({ app: row.app, tenant: row.tenant, subject: row.subject }),
        destroyedAt: row.destroyed_at,
      }) satisfies Retain.Tombstone),
    )).pipe(
      Effect.tap(Option.match({
        onNone: () => Effect.void, // no live key existed: nothing was destroyed, nothing fans
        onSome: (tombstone) => Hook.tapped("retainErase", { tenant: tombstone.subject.tenant, subject: tombstone.subject.subject }),
      })),
    )
```

## [04]-[DSAR_EXPORT]

- Owner: `Retain.dsar` — the one portability fold: every journal event indexed to the tenant-scoped `SubjectKey`, lifted through the journal family's own upcast plan into the live member, joined with the key's object references, streamed as one export document; sealed fields inside payloads stay sealed here — the exporting consumer composes `Retain.open` per field it knows the shape of, because field shapes are app material; plus the subject-index slot this page publishes to the write transaction.
- Packages: `effect` (`Stream`, `Array`); `journal/evolve.md` (`Upcast.Plan` — the one read-lift road); `journal/append.md` (the read stream and the `Slot` contract); `read/live.md` (`Live.merged` — the slot's empty coordinate); the object plane's reference rows arrive by relation name.
- Entry: the subject index is written at publish time — `Retain.slot(subjects)` mints the `Journal.Slot` an app carries in its publish intent: the caller's `subjects` projection names each event's subject keys, the slot stamps `(subject, sequence)` rows inside the commit, and the DSAR read is therefore an index scan, never a full-log crawl; the caller hands `dsar` the same folded `Upcast.Plan` its journal binding holds, so export and replay lift through one anchor.
- Growth: a new export surface (object bytes bundled, format variants) is a projection of the same fold — the subject spine never changes.
- Law: the export and the erasure share one spine — the same `subject_journal` index that finds events to export finds nothing to rewrite on erasure, proving the two rights compose: export reads what remains readable, erasure makes fields unreadable, and both leave the log bytes untouched.
- Law: the fold is streaming and decoded to the live family — each row admits through the `_EntryRow` schema (`payload` through `Upcast.Column`), then lifts through `plan.decode` so historical event versions upcast and the export carries admitted members, never a raw tag/version/payload envelope; a malformed or unplanned historical row quarantines as `ParseError` on the stream, and the `subject_journal.sequence` join runs engine-side against the BIGINT column, so no sequence value crosses the process untyped.
- Law: sensitive projection columns never enter the export — the `Model.Sensitive` field class strips them from every JSON variant by construction; sealed payload fields export opened only where the consuming exporter composes `Retain.open` against a live key, and an erased subject's fields export as the redaction marker the `Option.none` fold names.

```typescript signature
import { Array, Stream, type ParseResult } from "effect"
import { Live } from "../read/live.ts"
import { Upcast } from "./evolve.ts"

declare namespace Retain {
  type Entry<A> = {
    readonly event: A
    readonly recordedAt: string
  }
  type Export<A> = {
    readonly subject: SubjectKey
    readonly events: Stream.Stream<Entry<A>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
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
    app TEXT NOT NULL, tenant TEXT NOT NULL, subject TEXT NOT NULL, sequence BIGINT NOT NULL,
    PRIMARY KEY (app, tenant, subject, sequence));`,
  sqlite: `CREATE TABLE IF NOT EXISTS subject_journal (
    app TEXT NOT NULL, tenant TEXT NOT NULL, subject TEXT NOT NULL, sequence INTEGER NOT NULL,
    PRIMARY KEY (app, tenant, subject, sequence));`,
}

const _slot = <A>(subjects: (event: A) => ReadonlyArray<Retain.Subject>): Journal.Slot<A> => ({
  keys: () => Live.merged([]), // no reactive reader subscribes the subject index: the empty coordinate stamps nothing
  project: (stream, events, receipt) =>
    Effect.flatMap(SqlClient.SqlClient, (sql) => {
      const rows = Array.flatMap(Array.zip(events, receipt.rows), ([event, row]) =>
        Array.map(subjects(event), (subject) => ({
          app: stream.app,
          tenant: stream.tenant,
          subject,
          sequence: row.sequence, // receipt rows align positionally with the batch: the landed global sequence indexes its event's subjects
        })))
      return Array.isNonEmptyReadonlyArray(rows)
        ? Effect.asVoid(sql`INSERT INTO subject_journal ${sql.insert(rows)} ON CONFLICT DO NOTHING`)
        : Effect.void
    }),
})

const _EntryRow = Schema.Struct({
  tag: Schema.String,
  event_version: Schema.Int.pipe(Schema.positive()),
  payload: Upcast.Column,
  recorded_at: Schema.String,
})

const _RefRow = Schema.Struct({ key: Schema.String, retention: _Class })

const _admitEntry = Schema.decodeUnknown(_EntryRow)

const _dsar = <A>(subject: SubjectKey, plan: Upcast.Plan<A>): Retain.Export<A> => ({
  subject,
  events: Stream.unwrap(
    Effect.map(SqlClient.SqlClient, (sql) =>
      sql`SELECT e.tag, e.event_version, e.payload, CAST(e.recorded_at AS TEXT) AS recorded_at FROM journal_event e
          JOIN subject_journal s ON s.sequence = e.sequence AND s.app = e.app AND s.tenant = e.tenant
          WHERE s.app = ${subject.app} AND s.tenant = ${subject.tenant} AND s.subject = ${subject.subject}
          ORDER BY e.sequence`.stream.pipe(
        Stream.mapEffect((raw) =>
          Effect.gen(function* () {
            const row = yield* _admitEntry(raw)
            // the read-lift road: historical versions upcast, the live family proves the landing, malformed history quarantines as ParseError
            const event = yield* plan.decode({ tag: row.tag, version: row.event_version, payload: row.payload })
            return { event, recordedAt: row.recorded_at } satisfies Retain.Entry<A>
          })),
      )),
  ),
  objects: Effect.flatMap(SqlClient.SqlClient, (sql) =>
    SqlSchema.findAll({
      Request: SubjectKey,
      Result: _RefRow,
      execute: (who) => sql`SELECT key, retention FROM object_ref WHERE owner = ${who.owner} AND released_at IS NULL`,
    })(subject)),
})

const Retain = {
  Class: _Class,
  Subject: _Subject,
  SubjectKey,
  Policy: _Policy,
  handoff: _handoff,
  groom: _groom,
  seal: _seal,
  open: _open,
  erase: _erase,
  slot: _slot,
  dsar: _dsar,
  ddl: [_frontierDdl, _subjectDdl, _subjectIndexDdl],
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Retain, SubjectKey }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
