# [DATA_RETAIN]

Aging stays lawful without rewriting: the log is append-only forever, so this page owns the three ways data ages — retention-class windows driving ledger expiry, outbox and fact grooming, and partition drop behind the causal frontier; crypto-shredding, where subject-bearing fields seal under a per-subject data key whose wrapped form is the ONLY thing this folder stores, so destroying it makes every sealed read fold to a redaction marker, totally; and the per-subject DSAR export fold, one portability read over journal events, audit facts, and object references riding the same subject spine erasure uses. Core causality supplies the stability frontier as a value, partitions at or below a snapshotted frontier drop through the `partition` grant, and compaction is a capability, never a default. Above every ender stands the legal-hold suspension ledger: a declared matter outranks the class window on every subject-scoped closer until it lifts.

## [01]-[INDEX]

- [02]-[RETENTION_ROWS]: the retention-class vocabulary, window policy, frontier handoff, partition rows, the legal-hold suspension ledger.
- [03]-[SHREDDER]: the wrapped-key ledger, seal/open folds, erasure as key destruction.
- [04]-[DSAR_EXPORT]: the per-subject portability fold over journal plus object rows.

## [02]-[RETENTION_ROWS]

- Owner: the `Retain.Class` vocabulary — one `as const` key tuple feeding `Schema.Literal` and the window-row table, so wire admission and the type derive from one anchor pair — with the frontier ledger recording the causal handoff, the `_GROOMS` roster naming every relation that ages by wall clock and its two renderings, the `legal_hold` suspension ledger with its `hold`/`lift`/`held` verbs and the `Retain.holding` predicate pair both renderings and the object plane compose, and the partition rows that realize aging on the spine.
- Packages: `effect` (`Array`, `Duration`, `Option`, `Record`, `Schema`); `@effect/sql`; `@rasm/ts/core` (`Causal.Retention` — the `{floor, stamp}` compaction coordinate — `Identity.tenancy`, the tenancy axis every sweep row states, and `Fault.Class` for the hold refusal's kind); the `partition` and `cron` grants gate execution.
- Entry: every aging consumer reads one vocabulary — object references store a class key, `Retain.groom(key)` sweeps in process and `Retain.groomText(key, dialect)` renders the scheduled statements `read/fold#MAINTENANCE` registers, `journal/fact.md` keys its fact streams by the same classes, and the granted maintenance rows execute the partition drops; no window literal and no groom predicate exists outside this page.
- Growth: a new class is one row — every sweep, groom, and lifecycle rule inherits it; a new aging surface reads the table, never mints a window; a new cost depth is one `_depths` entry the object plane's own storage-class map then answers.
- Law: a class prices AGE and DEPTH in one row — `lifetime.bound` says how long the class lives and `transitions` says how its bytes get cheaper while they do, so the retention vocabulary is a cost tier rather than a delete timer and the object plane's lifecycle rules generate both halves from this one table with no window or class literal outside it.
- Law: a class answers `fits`, `lifetime`, and `degrade`, and decides NEITHER admission NOR tenancy — writers stamp the class column and `_GROOMS` settles sweep tenancy, so a class column for either answers it by guess; `lifetime` carries the bound beside the OWNER closing it, because a wall-clock sweep and a key destruction close a life under different authorities and `permanent` is closed by the shredder alone.
- Law: `degrade` is load-bearing per class — `regulatory` names the subject-scoped reach of the hold gate outright, so evidence keyed by no subject is known to groom on schedule whatever matter it serves, and `permanent` names the columns surviving erasure, so a compliance reader plans for the row staying queryable after its subject goes dark.
- Law: the depth vocabulary is retention's and the storage-class SPELLING is the object plane's — age is what selects a depth, so the rung names one, and `object/store.md` maps depth onto the engine's own class and filters the ladder against that engine's conformance cell; naming a vendor storage class here drags one engine's vocabulary into the aging owner every other plane reads.
- Law: depth ascends with position, so an engine honouring one honours every shallower rung and the filter is an index compare rather than a per-engine roster; `ephemeral` carries no rung at all, because every archive class bills a thirty-day minimum and a rung under a seven-day window pays that floor on bytes already gone.
- Law: a rung states its own forfeit — `_DEPTHS` carries `restore`, the posture that costs a reader hours behind a restore verb, because cost order alone cannot tell a consumer whether bytes answer now; `regulatory` and `permanent` both run the ladder to `frozen`, so long-lived evidence is exactly the material a portability or incident read finds unavailable synchronously, and a consumer told only the class plans for a latency the class never mentioned.
- Law: the journal itself never ages by wall clock — partition drop is lawful only at-or-below a frontier the causality owner finalized AND a snapshot at-or-above it exists; `Retain.handoff` records the `Causal.Retention` coordinate into the frontier ledger under a monotonic guard (a stale handoff commits nothing), the drop statement generates from the recorded row, and compaction can never orphan unreplayable history.
- Law: partitioning is a `pg_partman` image fact — the ensure registers `journal_event` as a partitioned parent only where the `partition` grant holds, and the drop itself is the granted maintenance row `lane/postgres.md` gates; the sqlite profiles never partition (their compaction is the export-snapshot-and-truncate posture `lane/sqlite.md` owns).
- Law: grooming is ONE roster and two renderings, never a relation pair a caller names — `_GROOMS` states each aging relation's age column, its eligibility gate, and where its class comes from, because only a relation CARRYING a `retention` column can be swept class by class: the ledger, the outbox, and the quarantine carry none, so a class-keyed predicate against them names a column no DDL declares and the sweep dies on its first run. A row whose whole content belongs to one class states that class instead, `permanent` folds to a no-op by `Duration.isFinite`, and the roster feeds ONE age table through two BINDING postures — `Retain.groom(key)` binds values through `sql.onDialectOrElse`, `Retain.groomText(key, dialect)` renders the statement TEXT the scheduled plane's extension contract takes, and both read `_AGE` and `_seconds`, so the dialect axis rides each road and a maintenance row scheduled on an embedded profile can never receive the spine's own spelling; every literal on both roads is sealed `Retain.Policy` material, so no caller value ever reaches a groom statement.
- Law: isolation spells `tenancy` on every row and never a second name — `residency`, `partition`, and `scope` are dead spellings no reader compares across families; sweeps run at the `tenancy` their row states and never at the ambient one, so with `multi` on every current row `Retain.groom` runs from maintenance on an unpinned client and a sweep issued inside `Tenant.within` is the named defect: RLS narrows that same DELETE to one tenant, every other tenant then ages forever, and the statement reports success either way.
- Law: the three planes here answer tenancy differently and each answers it structurally — grooming is `multi` across a whole relation, custody and DSAR are `single` on `(app, tenant, subject)`, and the frontier ledger is `single` on `(app, tenant, aggregate)`; one page-wide tenancy answer is wrong for two of the three.
- Law: the roster is the boundary the maintenance plane spends — `read/fold#MAINTENANCE` schedules `Retain.groomText` rows and spells no DELETE of its own, so a relation that starts aging is one row here rather than one sweep on each plane with a different predicate and no way to tell which one governs.
- Law: the `incremental` grant upgrades large grooms to exactly-once checkpointed batch folds where it probes true.
- Law: the partition drop spells the EXACT boundary — `Retain.dropText` renders one DO block enumerating children through `partman.show_partitions` and dropping each whose `show_partition_info` exclusive `child_end_id` sits at or below `floor + 1`, detach then drop; `part_config.retention` and `drop_partition_id` are REFUSED for frontier drops because both price a distance below a live maximum RE-READ at execution, so appends landing between the frontier read and the drop move the effective boundary past the finalized frontier and drop history no snapshot covers.
- Law: `run_maintenance(p_parent_table)` serves premake alone — the config row keeps `retention` NULL, so maintenance creates partitions and never drops one; pg_partman never drops a set's final child either, so an idle stream retains one partition whatever the floor, a floor the drop text inherits rather than re-derives.
- Law: the drop floor is the ledger's `min(snapshotted)` gated on TOTAL stream coverage — a journal stream absent from the frontier ledger zeroes the floor inside the same statement, because a per-stream frontier proves nothing about a sibling stream sharing the partitioned sequence spine.
- Law: a declared HOLD outranks every ender while it lives — `legal_hold` keys `(app, tenant, matter, subject)`, `Retain.hold` records a declaration and `Retain.lift` closes one matter whole; a hold SUSPENDS rather than reclassifies, so the row's class and clock never move, lifting ages an over-window row out on the next sweep at once, and no pre-hold state exists to restore or forge.
- Law: this plane admits no operator identity, so a hold records a DECLARED custody fact — `declared_by` carries the principal the security seam authenticated, authorization stays the caller's security composition, and the declaration trail is the caller's own audit record through the security audit port; the `owner` column mints once through the custody key's own projection, so the object-plane join and the DSAR scan read one spelling.
- Law: the hold gate rides every subject-scoped closer — a `_GROOMS` row naming a `held` subject column composes `Retain.holding.subject` in BOTH renderings, so the scheduled statement and the in-process sweep honour one suspension; relations carrying no subject column hold nothing, which is `operational`'s standing degrade; `erase` refuses a held subject with the typed `RetainHold` naming its live matters, because a destroyed key is the one closer no lift recovers; the object plane composes `Retain.holding.owner` at its retag fold and `lift` answers the lifted owner roster the maintenance seam re-tags.

```typescript signature
import { Array, Duration, Effect, Option, pipe, Record, Schema } from "effect"
import { SqlClient, SqlSchema } from "@effect/sql"
import { Fault, Identity } from "@rasm/ts/core"
import type { Capability } from "../lane/capability.ts"
import { Journal, StreamKey } from "./append.ts"

const _classes = ["ephemeral", "operational", "regulatory", "permanent"] as const

// Cost depth ascends with position, so an engine honouring a depth honours every shallower one and the ladder filter
// is an index compare rather than a per-engine roster: `cool` is reduced-access at millisecond reads, `cold` an
// archive class that still answers immediately, `frozen` a deep archive answering in hours behind a restore verb.
// Retention owns the vocabulary because AGE selects it; the object plane owns the storage-class SPELLING.
const _depths = ["cool", "cold", "frozen"] as const

// Position carries the COST ordering; this table carries what a rung gives up, because access posture is not derivable
// from depth order and every reader downstream needs it as data. `restore` names the rung whose bytes answer only
// behind a restore verb measured in hours — a portability export, an incident read, and a rebuild all have to plan for
// that latency, and a rung stating it only in prose leaves each of them to rediscover it from a timeout.
const _DEPTHS = {
  cool: { restore: false },
  cold: { restore: false },
  frozen: { restore: true },
} as const satisfies { readonly [D in Retain.Depth]: { readonly restore: boolean } }

// Rows answer the descriptor coordinates a class decides. `fits` is the sentence a reader selects on, `degrade`
// names the forfeit, and `lifetime` is the whole survival answer — how long the bound runs, and WHICH AUTHORITY
// closes it. Admission and tenancy are NOT this family's to decide: writers stamp the class column, `_GROOMS` below
// settles sweep tenancy, and a class answering either answers it by guess.
// `lifetime.owner` carries the load-bearing half: a wall-clock sweep and a key destruction end a life under different
// authorities, and `permanent` proves why naming it matters — nothing sweeps it, so only the shredder ever closes it.
const _Policy = {
  ephemeral: {
    fits: "operational exhaust whose worth expires with the operation that produced it",
    lifetime: { bound: Duration.decode("7 days"), owner: "groom" },
    transitions: [],
    degrade: "no archive rung — every archive class bills a thirty-day minimum, so a rung under this window charges for bytes already gone",
  },
  operational: {
    fits: "routine trails an operator reads while an incident is still live",
    lifetime: { bound: Duration.decode("90 days"), owner: "groom" },
    transitions: [{ after: Duration.decode("30 days"), depth: "cool" }],
    degrade: "its groomed relations carry no subject column, so no hold reaches them and an investigation outliving the window loses its trail on schedule",
  },
  regulatory: {
    fits: "compliance and billing evidence whose life a statute decides rather than a reader",
    lifetime: { bound: Duration.decode("2555 days"), owner: "groom" },
    transitions: [
      { after: Duration.decode("30 days"), depth: "cool" },
      { after: Duration.decode("90 days"), depth: "cold" },
      { after: Duration.decode("365 days"), depth: "frozen" },
    ],
    degrade: "the hold gate is subject-scoped — evidence keyed by no subject grooms on schedule whatever matter it serves, and the frozen rung answers a portability read only behind an hours-long restore",
  },
  permanent: {
    fits: "the record whose loss no replay, rebuild, or restore recovers",
    lifetime: { bound: Duration.infinity, owner: "shred" },
    transitions: [
      { after: Duration.decode("30 days"), depth: "cool" },
      { after: Duration.decode("90 days"), depth: "cold" },
      { after: Duration.decode("365 days"), depth: "frozen" },
    ],
    degrade: "no sweep ever ends it — erasure is key destruction alone, so every unsealed column stays queryable after a subject's sealed fields go dark",
  },
} as const

const _Class = Schema.Literal(..._classes)

const _Subject = Schema.NonEmptyString.pipe(Schema.maxLength(200), Schema.brand("Subject"))

declare namespace Retain {
  type Class = (typeof _classes)[number]
  type Depths = typeof _depths
  type Depth = Depths[number]
  type Dialect = keyof typeof _AGE
  type Ender = "groom" | "shred" // the authority that closes a lifetime: a wall-clock sweep, or key destruction
  type Rung = { readonly after: Duration.Duration; readonly depth: Depth }
  type Row = (typeof _Policy)[Class]
  type Subject = typeof _Subject.Type
  type _Rows<T extends Record<Class, {
    readonly degrade: string
    readonly fits: string
    readonly lifetime: { readonly bound: Duration.Duration; readonly owner: Ender }
    readonly transitions: ReadonlyArray<Rung>
  }> = typeof _Policy> = T
  type _Depths<D extends Depth = Row["transitions"][number]["depth"]> = D // every rung's depth is a rostered one, both directions
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

// One row per relation that ages by wall clock, and the whole difference between them is data. `clazz` names where
// a row's retention class comes from: `"row"` where the relation CARRIES a `retention` column and each class sweeps
// under its own window, a class key where the relation's whole content belongs to one class. The distinction is
// load-bearing rather than cosmetic — `idempotency_ledger`, `outbox`, and `projection_quarantine` declare no
// `retention` column at all, so a class-keyed predicate against them names a column no DDL wrote. `live` is the
// eligibility gate a relation states before age is even asked (an undelivered deliverable and an unreplayed
// quarantine row are still owed, whatever their age), and it is sealed policy text, never a caller value.
// `tenancy` states the isolation a sweep runs at under the ONE spelling every plane compares on, and it reads the
// closed `Identity.tenancy` roster directly because this column selects an executable refusal rather than explaining
// a row to a selecting profile — no descriptor cell of a mechanism substitutes for the axis a DELETE is gated on.
// It stays a column rather than an assumption because these relations register RLS: a DELETE carrying no tenant
// predicate removes whatever the ambient session GUC leaves visible, so one statement is estate-wide from maintenance
// and one-tenant-wide from inside a pin, with nothing in the result telling those apart. Every row here reads `multi`
// and `Retain.groom` refuses a pinned client for that reason; a future per-tenant relation states `single` and takes
// its pin instead of inheriting one.
// `held` names the relation's subject column where one exists, and the hold gate composes only there: a hold is a
// subject-scoped suspension, so a relation carrying no subject column states `none` and grooms unconditionally —
// which is the honest answer, never a gate against a column no DDL wrote.
const _GROOMS = {
  facts: { clazz: "row", column: "recorded_at", held: Option.some("subject"), live: Option.none(), relation: "fact_journal", tenancy: "multi" },
  ledger: { clazz: "operational", column: "touched_at", held: Option.none(), live: Option.none(), relation: "idempotency_ledger", tenancy: "multi" },
  outbox: {
    clazz: "ephemeral",
    column: "delivered_at",
    held: Option.none(),
    live: Option.some("delivered_at IS NOT NULL"),
    relation: "outbox",
    tenancy: "multi",
  },
  quarantine: {
    clazz: "operational",
    column: "replayed_at",
    held: Option.none(),
    live: Option.some("replayed_at IS NOT NULL"),
    relation: "projection_quarantine",
    tenancy: "multi",
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly tenancy: Identity.Tenancy
  readonly clazz: Retain.Class | "row"
  readonly column: string
  readonly held: Option.Option<string>
  readonly live: Option.Option<string>
  readonly relation: string
}>

declare namespace Retain {
  type Groomed = keyof typeof _GROOMS
  type Groom = (typeof _GROOMS)[Groomed]
}

// A class-carrying relation sweeps once per FINITE class and a fixed-class relation sweeps once, so `permanent`
// folds out of both by the same `Duration.isFinite` read and neither road spells the exclusion twice.
const _windows = (row: Retain.Groom): ReadonlyArray<Retain.Class> =>
  Array.filter(row.clazz === "row" ? _classes : [row.clazz], (clazz) => Duration.isFinite(_Policy[clazz].lifetime.bound))

// Window seconds truncate ONCE for both renderings: a bound fragment rounding one way and a rendered statement the
// other would sweep two different populations off one policy row, and the drift is invisible until a boundary case.
const _seconds = (window: Duration.Duration): number => Math.trunc(Duration.toSeconds(window))

// Age predicates carry the DIALECT axis on both roads. Text rendering that spells only the spine's `now()` and
// `interval` emits statements the embedded engines never declared, so the scheduled maintenance row this roster feeds
// dies on its first run everywhere the spine is absent — and the roster's whole claim is that neither rendering may
// disagree with it. Only BINDING posture differs: sweeps bind values, the scheduled plane takes text.
const _AGE = {
  pg: (column: string, seconds: number): string => `${column} < now() - interval '${seconds} seconds'`,
  sqlite: (column: string, seconds: number): string => `${column} < datetime('now', '-${seconds} seconds')`,
} as const

const _aged = (sql: SqlClient.SqlClient, row: Retain.Groom, window: Duration.Duration) =>
  sql.onDialectOrElse({
    orElse: () => sql`${sql(row.column)} < datetime('now', ${`-${_seconds(window)} seconds`})`,
    pg: () => sql`${sql(row.column)} < now() - make_interval(secs => ${_seconds(window)})`,
  })

// ONE hold predicate serves every composer: the subject face gates a groomed relation's rows, the owner face gates
// the object plane's retag fold. Both render as plain SQL over sealed roster values, so the bound sweep, the
// scheduled text, and the store's join spell one suspension and none can honour a hold another missed.
const _holding = {
  subject: (relation: string, column: string): string =>
    `NOT EXISTS (SELECT 1 FROM legal_hold h WHERE h.lifted_at IS NULL AND h.app = ${relation}.app AND h.tenant = ${relation}.tenant AND h.subject = ${relation}.${column})`,
  owner: (alias: string): string =>
    `EXISTS (SELECT 1 FROM legal_hold h WHERE h.lifted_at IS NULL AND h.owner = ${alias}.owner)`,
} as const

const _swept = (sql: SqlClient.SqlClient, row: Retain.Groom, clazz: Retain.Class) =>
  sql`DELETE FROM ${sql(row.relation)} WHERE ${
    sql.and([
      ...Option.match(row.live, { onNone: () => [], onSome: (gate) => [sql.literal(gate)] }),
      ...Option.match(row.held, { onNone: () => [], onSome: (column) => [sql.literal(_holding.subject(row.relation, column))] }),
      ...(row.clazz === "row" ? [sql`retention = ${clazz}`] : []),
      _aged(sql, row, _Policy[clazz].lifetime.bound),
    ])
  }`

const _groom = (key: Retain.Groomed) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.forEach(_windows(_GROOMS[key]), (clazz) => _swept(sql, _GROOMS[key], clazz), { concurrency: 1, discard: true }))

// The scheduled plane registers statement TEXT — its extension's own contract — where the in-process road binds
// values, so the row renders twice and the ROSTER is what neither rendering may disagree with. Every literal here
// reads off `_Policy` and `_GROOMS`, so the interpolation surface is sealed by construction.
const _groomText = (key: Retain.Groomed, dialect: Retain.Dialect): ReadonlyArray<string> =>
  pipe(_GROOMS[key], (row) =>
    Array.map(_windows(row), (clazz) =>
      `DELETE FROM ${row.relation} WHERE ${
        Array.join(
          [
            ...Option.toArray(row.live),
            ...Option.match(row.held, { onNone: () => [], onSome: (column) => [_holding.subject(row.relation, column)] }),
            ...(row.clazz === "row" ? [`retention = '${clazz}'`] : []),
            _AGE[dialect](row.column, _seconds(_Policy[clazz].lifetime.bound)),
          ],
          " AND ",
        )
      }`))

// The scheduled drop renders as ONE pg DO block off the recorded ledger. The floor is `min(snapshotted)` gated on
// TOTAL stream coverage — a journal stream with no frontier row zeroes the floor inside the same statement, so no
// per-stream proof speaks for a sibling sharing the sequence spine. Children drop on their EXCLUSIVE end against
// the floor, never on pg_partman's retention distance: that knob re-reads the live max at execution, so appends
// landing after the frontier read move its boundary past the finalized frontier. `run_maintenance` stays
// premake-only with `retention` NULL, and pg_partman's final-child floor rides through untouched.
const _dropText = (parent: string): string => `DO $drop$
DECLARE
  floor BIGINT;
  child RECORD;
  bounds RECORD;
BEGIN
  SELECT CASE WHEN EXISTS (
      SELECT 1 FROM (SELECT DISTINCT app, tenant, aggregate FROM ${parent}) s
      LEFT JOIN retain_frontier f USING (app, tenant, aggregate) WHERE f.aggregate IS NULL)
    THEN NULL ELSE (SELECT min(snapshotted) FROM retain_frontier) END INTO floor;
  IF floor IS NULL THEN RETURN; END IF;
  FOR child IN SELECT partition_schemaname, partition_tablename FROM partman.show_partitions('${parent}', 'ASC') LOOP
    SELECT child_start_id, child_end_id INTO bounds
      FROM partman.show_partition_info(format('%I.%I', child.partition_schemaname, child.partition_tablename));
    IF bounds.child_end_id IS NOT NULL AND bounds.child_end_id <= floor + 1 THEN
      EXECUTE format('ALTER TABLE ${parent} DETACH PARTITION %I.%I', child.partition_schemaname, child.partition_tablename);
      EXECUTE format('DROP TABLE %I.%I', child.partition_schemaname, child.partition_tablename);
    END IF;
  END LOOP;
END $drop$;`

const _holdDdl: Capability.Ensure = {
  relation: "legal_hold",
  pg: `CREATE TABLE IF NOT EXISTS legal_hold (
    app TEXT NOT NULL, tenant TEXT NOT NULL, matter TEXT NOT NULL, subject TEXT NOT NULL,
    owner TEXT NOT NULL,
    declared_by TEXT NOT NULL,
    declared_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    lifted_at TIMESTAMPTZ,
    PRIMARY KEY (app, tenant, matter, subject));
  CREATE INDEX IF NOT EXISTS legal_hold_subject ON legal_hold (app, tenant, subject) WHERE lifted_at IS NULL;
  CREATE INDEX IF NOT EXISTS legal_hold_owner ON legal_hold (owner) WHERE lifted_at IS NULL;`,
  sqlite: `CREATE TABLE IF NOT EXISTS legal_hold (
    app TEXT NOT NULL, tenant TEXT NOT NULL, matter TEXT NOT NULL, subject TEXT NOT NULL,
    owner TEXT NOT NULL,
    declared_by TEXT NOT NULL,
    declared_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    lifted_at TEXT,
    PRIMARY KEY (app, tenant, matter, subject));
  CREATE INDEX IF NOT EXISTS legal_hold_subject ON legal_hold (app, tenant, subject) WHERE lifted_at IS NULL;
  CREATE INDEX IF NOT EXISTS legal_hold_owner ON legal_hold (owner) WHERE lifted_at IS NULL;`,
}

// Single-shape-classed like the version-conflict family: `denied` because no re-drive lifts a matter and the
// recovery verb — `lift` — is the caller's, which is exactly non-retryable and caller-blamed.
class RetainHold extends Schema.TaggedError<RetainHold>()("RetainHold", {
  subject: Schema.String,
  matters: Schema.Array(Schema.String),
}) {
  get class(): Fault.Class.Kind {
    return "denied"
  }
  override get message(): string {
    return `<retain:held> ${this.subject}: ${this.matters.join(" ")}`
  }
}

declare namespace Retain {
  type Hold = {
    readonly matter: string
    readonly subjects: Array.NonEmptyReadonlyArray<SubjectKey>
    readonly declaredBy: string
  }
}

// Re-declaring a lifted pair re-arms it: the ON CONFLICT arm clears `lifted_at` and refreshes the declarant, so a
// matter re-opened against the same subject is one row's live state, never a second row a scan double-counts.
const _hold = (declaration: Retain.Hold) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    sql`INSERT INTO legal_hold ${
      sql.insert(Array.map(declaration.subjects, (key) => ({
        app: key.app,
        tenant: key.tenant,
        matter: declaration.matter,
        subject: key.subject,
        owner: key.owner, // minted once through the custody key's own projection: the join and the scan read one spelling
        declared_by: declaration.declaredBy,
      })))
    } ON CONFLICT (app, tenant, matter, subject) DO UPDATE
      SET lifted_at = NULL, declared_by = excluded.declared_by`)

// Lifting answers the lifted OWNER roster as a value, because the object plane's retag fold runs above this
// stratum: the maintenance seam walks each owner's keys through the store's reference read and re-derives tags.
const _lift = (app: string, tenant: string, matter: string) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.map(
      SqlSchema.findAll({
        Request: Schema.Struct({ app: Schema.String, tenant: Schema.String, matter: Schema.String }),
        Result: Schema.Struct({ owner: Schema.String }),
        execute: (who) =>
          sql`UPDATE legal_hold SET lifted_at = ${Journal.now(sql)}
              WHERE app = ${who.app} AND tenant = ${who.tenant} AND matter = ${who.matter} AND lifted_at IS NULL
              RETURNING owner`,
      })({ app, tenant, matter }),
      (rows) => Array.dedupe(Array.map(rows, (row) => row.owner)),
    ))

const _heldMatters = (key: SubjectKey) =>
  Effect.flatMap(SqlClient.SqlClient, (sql) =>
    Effect.map(
      SqlSchema.findAll({
        Request: SubjectKey,
        Result: Schema.Struct({ matter: Schema.String }),
        execute: (who) =>
          sql`SELECT matter FROM legal_hold
              WHERE app = ${who.app} AND tenant = ${who.tenant} AND subject = ${who.subject} AND lifted_at IS NULL`,
      })(key),
      Array.map((row) => row.matter),
    ))
```

## [03]-[SHREDDER]

- Owner: `SubjectKey`, the `(app, tenant, subject)` custody identity; the `subject_key` ledger holding one `WrappedKey` per key; the `seal`/`open` folds composing the security `Shredder` envelope algebra; and `erase`, destroying the wrapped key material and marking the tombstone in one statement.
- Packages: `@rasm/ts/security` (`Shredder`, `WrappedKey`, `SealedEnvelope` — the one direct `data → security` edge); `effect` (`Effect`, `Option`, `Schema`).
- Entry: an app seals subject-bearing fields at construction — `Retain.seal(key, bytes)` before the payload enters the publish transaction; reads meeting sealed fields call `Retain.open(key, envelope)` and receive `Option<bytes>` — `none` IS the erased state, folded by the consumer into its redaction marker.
- Receipt: `erase` returns `Option<{ subject: SubjectKey, destroyedAt }>` — some is the auditable tombstone the fact stream records, none means no live key existed; the log bytes remain, provably unreadable either way. A subject under a live hold refuses FIRST with the typed `RetainHold` naming its matters, never a silent none a caller reads as already-erased — key destruction is the one closer no lift recovers.
- Growth: a new custody posture (a KMS-held KEK) is a security-side construction row — this ledger stores whatever `WrappedKey` the Shredder wraps, so custody changes never touch this page.
- Law: custody is tenant-scoped structurally — every lookup, upsert, erase, subject-index row, and DSAR scan keys on `(app, tenant, subject)`; equal subject strings in two tenants never share key material or export rows, and no ambient RLS setting substitutes for the composite identity.
- Law: the ledger stores ONLY the wrapped form — `Shredder.mint` issues the data key, `wrap` seals it under the master KEK, and the raw `CryptoKey` never crosses this seam; `seal` is one atomic upsert realizing the `conflictClaim` primitive: the fresh mint inserts, a concurrent or replayed subject keeps the stored wrapped key through the `coalesce` arm and the loser seals under the winner's key by unwrapping the RETURNING row, and a destroyed subject resurrects under a NEW key (old envelopes stay unreadable forever) because the `CASE` arm clears the tombstone only when `wrapped` was NULL.
- Law: `open` is total — a destroyed or absent key folds to `Option.none`, never a fault, because erasure is a lawful state every reader renders, not an error to recover from; a genuine unwrap failure on live material is the security fault it already is.
- Law: erasure is key destruction ONLY — `UPDATE subject_key SET wrapped = NULL, destroyed_at = …` — no journal row is touched, no payload rewritten; the append-only invariant survives the right to erasure because unreadable IS erased.
- Law: the tombstone carries the `rasm.data.retain.erase` observe point — a landed erasure fans the app-armed taps with the tenant-scoped subject coordinate after the destroying statement returns, so compliance observers subscribe to the fact instead of instrumenting this fold, and an absent registry costs nothing.
- Law: the envelope travels as `SealedEnvelope` — IV and ciphertext as opaque encoded bytes inside the payload field, opaque to this page and to the log; the exposure control is the READ plane's, where a projection row declaring the field `Model.Sensitive` strips it from every JSON variant, because the journal families are domain classes carrying no variant axis and a claim of stripping made here names a mechanism this page cannot run.

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
  Effect.filterOrElse(
    _heldMatters(key),
    (matters) => !Array.isNonEmptyReadonlyArray(matters),
    (matters) => Effect.fail(new RetainHold({ subject: key.subject, matters })),
  ).pipe(Effect.zipRight(Effect.flatMap(SqlClient.SqlClient, (sql) =>
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
    )))
```

## [04]-[DSAR_EXPORT]

- Owner: `Retain.dsar` — the one portability fold: every journal event indexed to the tenant-scoped `SubjectKey`, lifted through the journal family's own upcast plan into the live member, joined with the key's object references AND the fact stream's own subject-indexed audit rows, streamed as one export document; sealed fields inside payloads stay sealed here — the exporting consumer composes `Retain.open` per field it knows the shape of, because field shapes are app material — and the subject-index slot this page publishes to the write transaction; `Retain.RefRead` is this page's PORT for the object leg, so the reference relation keeps one reader surface.
- Packages: `effect` (`Stream`, `Array`); `journal/evolve.md` (`Upcast.Plan` — the one read-lift road; `Upcast.Envelope` — the persisted coordinate the export row spreads); `journal/append.md` (the read stream and the `Slot` contract); `read/live.md` (`Live.merged` — the slot's empty coordinate); the object plane's reference read arrives as the `RefRead` argument — `object/store.md` publishes the one implementation.
- Entry: the subject index is written at publish time — `Retain.slot(subjects)` mints the `Journal.Slot` an app carries in its publish intent: the caller's `subjects` projection names each event's subject keys, the slot stamps `(subject, sequence)` rows inside the commit, and the DSAR read is therefore an index scan, never a full-log crawl; the caller hands `dsar` the same folded `Upcast.Plan` its journal binding holds AND the store's published reference read, so export, replay, and the reference relation each lift through one anchor.
- Law: the object leg is a HANDED contract, never cross-strata SQL — this page declares the `RefRead` port and the object plane satisfies it exactly as the upcast plan arrives, so the strata direction holds, an `object_ref` schema change ripples through one published read, and this plane carries none of the store's SQL.
- Growth: a new export surface (object bytes bundled, format variants) is a projection of the same fold — the subject spine never changes.
- Law: the export and the erasure share one spine — the same `subject_journal` index that finds events to export finds nothing to rewrite on erasure, proving the two rights compose: export reads what remains readable, erasure makes fields unreadable, and both leave the log bytes untouched.
- Law: every subject-bearing plane answers the SAME custody coordinate — the event index, the object-reference owner, and the fact stream's own subject column each key on `(app, tenant, subject)`, so one erase destroys one data key and redacts all three at once; a plane reachable by subject but absent from this fold exports nothing and proves nothing, which is the portability hole the shared coordinate forecloses.
- Law: the fold is streaming and decoded to the live family — each row admits through `_EntryRow`, the envelope family's row projection plus this join's `recorded_at`, and reaches `plan.decode` whole, so historical event versions upcast and the export carries admitted members rather than a raw envelope; a malformed or unplanned historical row quarantines as `ParseError` on the stream, and the `subject_journal.sequence` join runs engine-side against the BIGINT column, so no sequence value crosses the process untyped.
- Law: sensitive projection columns never enter the export — the `Model.Sensitive` field class strips them from every JSON variant by construction wherever the read plane's own row declares it; sealed payload fields export opened only where the consuming exporter composes `Retain.open` against a live key, and an erased subject's fields export as the redaction marker the `Option.none` fold names.
- Law: the object leg exports REFERENCES, never bytes — a key whose class ran the ladder to a `restore`-posture rung answers no synchronous fetch, so the consumer bundling an archive reads `Retain.depthRows` and plans the restore; an export promising bytes it cannot produce inside a request is the portability failure this leg refuses to pretend away.

```typescript signature
import { Array, Stream, type ParseResult } from "effect"
import { Live } from "../read/live.ts"
import { Upcast } from "./evolve.ts"

declare namespace Retain {
  type Entry<A> = {
    readonly event: A
    readonly recordedAt: string
  }
  type Ref = { readonly key: string; readonly retention: Class }
  // The object leg's port: declared here where the strata floor sits, satisfied by the store's published read,
  // handed to `dsar` exactly as the upcast plan is — so the reference relation has ONE reader surface.
  type RefRead = (
    owner: string,
  ) => Effect.Effect<ReadonlyArray<Ref>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
  type Export<A> = {
    readonly subject: SubjectKey
    readonly events: Stream.Stream<Entry<A>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
    readonly facts: Effect.Effect<
      ReadonlyArray<{ readonly stream: string; readonly retention: Class; readonly payload: string; readonly recorded_at: string }>,
      SqlError.SqlError | ParseResult.ParseError,
      SqlClient.SqlClient
    >
    readonly objects: Effect.Effect<ReadonlyArray<Ref>, SqlError.SqlError | ParseResult.ParseError, SqlClient.SqlClient>
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
  ...Upcast.Envelope.row.fields,
  recorded_at: Schema.String,
})

const _FactRow = Schema.Struct({
  stream: Schema.String,
  retention: _Class,
  payload: Schema.String,
  recorded_at: Schema.String,
})

const _admitEntry = Schema.decodeUnknown(_EntryRow)

const _dsar = <A>(subject: SubjectKey, plan: Upcast.Plan<A>, refs: Retain.RefRead): Retain.Export<A> => ({
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
            // Read-lift road: historical versions upcast, the live family proves the landing, malformed history quarantines as ParseError
            const event = yield* plan.decode(row)
            return { event, recordedAt: row.recorded_at } satisfies Retain.Entry<A>
          })),
      )),
  ),
  // Fact rows are the SECOND indexed plane a subject spans: their own `subject` column carries the same custody
  // coordinate this key ledger keys on, so portability reads audit evidence off that partial index rather than crawling
  // its log, and a sealed identifier exports through the consumer's own `open` exactly as a sealed event field does.
  facts: Effect.flatMap(SqlClient.SqlClient, (sql) =>
    SqlSchema.findAll({
      Request: SubjectKey,
      Result: _FactRow,
      // `payload` casts for the same reason `recorded_at` does: the spine stores it as a json column the driver hands
      // back as a live object where the sqlite profiles hand back TEXT, so the export's own string type only holds
      // on both engines once the statement states the crossing. The band exports as the stored document verbatim —
      // decoding it to the live fact family would import the fact owner this page already publishes `Retain` to.
      execute: (who) =>
        sql`SELECT stream, retention, CAST(payload AS TEXT) AS payload, CAST(recorded_at AS TEXT) AS recorded_at FROM fact_journal
            WHERE app = ${who.app} AND tenant = ${who.tenant} AND subject = ${who.subject}
            ORDER BY sequence`,
    })(subject)),
  // The owner coordinate projects off the DECODED class's own getter, so the handed contract receives a value the
  // one mint spelled — the composition binding `refs` is `object/store.md`'s published reference read.
  objects: refs(subject.owner),
})

const Retain = {
  Class: _Class,
  Subject: _Subject,
  SubjectKey,
  Policy: _Policy,
  depths: _depths, // ordered shallow-to-deep: the object plane's conformance filter reads position, never a roster
  depthRows: _DEPTHS, // what each rung gives up, keyed by the same names the order above ranks
  handoff: _handoff,
  grooms: _GROOMS,
  groom: _groom,
  groomText: _groomText, // the scheduled plane's rendering of the same roster, dialect carried as a parameter
  dropText: _dropText, // the frontier-exact partition drop the scheduled plane registers under the partition grant
  holding: _holding, // the one hold predicate pair: subject face for groomed relations, owner face for the store's retag
  hold: _hold,
  lift: _lift,
  held: _heldMatters,

  seal: _seal,
  open: _open,
  erase: _erase,
  slot: _slot,
  dsar: _dsar,
  ddl: [_frontierDdl, _subjectDdl, _subjectIndexDdl, _holdDdl],
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Retain, RetainHold, SubjectKey }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
