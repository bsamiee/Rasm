# [DATA_POSTGRES]

PostgreSQL's guarantee-lane spine owns first-party capabilities, concurrency primitives with explicit denials, ruled extensions, and driver Layers binding neutral `SqlClient` to pg. Rows and Layer mints carry every fact; `lane/capability.md` probes, `lane/tenant.md` scopes, and journal statements consume the grant vocabulary. ONE derived union binds spine, primitives, extensions, sqlite degradation, gates, and deployment image. A capability is one row; pruning an extension deletes its row and image fact.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                                 |
| :-----: | :----------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `SPINE_ROWS`       | the first-party capability rows — identity mint, derivation, evidence, integrity forms |
|  [02]   | `PRIMITIVE_TABLE`  | the concurrency/queue primitives with their upholds AND denies columns                 |
|  [03]   | `EXTENSION_MATRIX` | the ruled extension rows, the derived grant union, demands, the image projection       |
|  [04]   | `DRIVER_ROWS`      | the `PgClient` Layer mints, the listener bus, the jsonb fragment                       |
|  [05]   | `PROFILE_HARVEST`  | the one engine-profile receipt family and the spine's statements and EXPLAIN arms      |

## [02]-[SPINE_ROWS]

- Owner: the `_spine` key tuple — one closed key per first-party engine capability; the derived `Pg.Spine` union is the first-party half of the grant vocabulary the sqlite degradation table mirrors row for row, and the SQL idiom each key names lives in this cluster's law lines, spliced by ensure authors as settled fact.
- Packages: none — the rows are engine facts, not packages.
- Entry: `journal/append.md` composes `uuidv7()` defaults and `RETURNING` evidence per these laws; `journal/evolve.md` reads `virtualGenerated` for derived snapshot columns; ensure authors read `temporal` for range-exclusive constraints; `lane/tenant.md` predicates the `rls` row's policy family; `lane/capability.md` seeds the pg granted set from `Pg.core.pg`.
- Growth: a new engine capability is one tuple key — the grant union, the sqlite mirror row, and every gate inherit it; a capability subsumed by a newer engine form deletes its extension row and lands here.
- Law: `uuidv7` is the identity-mint row — timestamp-ordered, index-local, keyset-paginatable; the extension that duplicated it is pruned from the matrix, and a surrogate key column defaults `uuidv7()` on the pg spine while the sqlite lanes mint in-app through the degradation row.
- Law: `returningOldNew` is the single-statement evidence form — `RETURNING old.*, new.*` on INSERT/UPDATE/DELETE/MERGE discriminates insert-versus-update and yields before/after evidence without a second scan or a trigger; receipt-bearing writes splice it instead of re-reading.
- Law: `virtualGenerated` is the compute-on-read default — a derived column costs no write amplification; `STORED` is the explicit opt-in an ensure states only where read-path cost dominates.
- Law: `temporal` is constraint-level range integrity — `WITHOUT OVERLAPS` keys and `PERIOD` foreign keys move validity-window enforcement into the engine; an application-level overlap check beside an available temporal constraint is the named defect.
- Law: `skipScan` widens every multicolumn index — a missing leading-column predicate no longer forces a second index; index ensures are authored against the widest query family, not per-predicate.
- Law: `asyncIo` is the read-path throughput row — `io_method` is a deployment fact the image projection carries implicitly; no statement composes it, and the row exists so the degradation table answers it honestly.
- Law: `rls` is the tenancy predicate plane — row-level security policies read the transaction-local GUC; the policy DDL and the pinning transformer are `lane/tenant.md`'s, and this row exists so the grant vocabulary names what the sqlite lanes replace with residency.

```typescript signature
const _spine = ["uuidv7", "returningOldNew", "virtualGenerated", "temporal", "skipScan", "asyncIo", "rls"] as const

declare namespace Pg {
  type Spine = (typeof _spine)[number]
}
```

## [03]-[PRIMITIVE_TABLE]

- Owner: the `_primitives` anchor — one row per first-party concurrency/queue primitive, each carrying `upholds` and `denies`; the denies column is the table's reason to exist, because every composed correctness lane is built from what a primitive refuses as much as from what it grants.
- Packages: none — engine facts.
- Entry: `journal/append.md` composes `advisory` for OCC serialization, `conflictClaim` for the idempotency ledger, `skipLocked` for relay claims, and `channel` for the post-commit wake; the projection and work drains read the same rows through their `SqlClient` ports.
- Growth: a new primitive is one row with both columns filled — a row missing its denies column is an unfinished admission.
- Law: `skipLocked` claims exactly-one-live-transaction, never delivery — a crashed claimant releases silently, so every drain pairs it with a visibility or attempts column for redelivery; global ordering under concurrency is refused by construction and priority is an `ORDER BY` term, never an assumption.
- Law: `channel` is a transactional wake pulse, never a queue — delivery fires only on COMMIT, deduped per channel/payload per transaction, absent listeners hear nothing, and the async queue is bounded in-memory; every listener re-polls on reconnect and the pulse only collapses poll latency.
- Law: `conflictClaim` — `INSERT … ON CONFLICT DO UPDATE … RETURNING` with an explicit insert/update marker — is atomic first-writer discrimination without reading transaction internals; what it refuses is replay truth across statements, so the ledger row, never the claim, carries the stored receipt.
- Law: `advisory` locks die with their session or transaction — application-defined mutual exclusion without row DDL, refusing persistence; a lock protecting state across restarts is a schema row, never an advisory claim.
- Law: `copy` is the maximal-throughput bulk lane under WAL and refuses per-row error routing — batch atomicity is all-or-nothing, so a partial-tolerant ingest splits its batch above the statement.
- Law: `partition` (declarative partitioning with replication) refuses automated lifecycle — premake and retention drop are the `pg_partman` extension row's, and `journal/retain.md` gates on that grant.

```typescript signature
const _primitives = {
  skipLocked: {
    upholds: "non-blocking competing-consumer claim, one live tx per row",
    denies: "delivery after claimant crash; global order under concurrency",
  },
  advisory: {
    upholds: "app-defined mutual exclusion without row DDL, xact or session scoped",
    denies: "persistence across sessions; cross-database scope",
  },
  channel: {
    upholds: "transactional wake pulse, commit-gated, per-tx deduped",
    denies: "delivery to absent listeners; payloads past 8000 bytes; queue durability",
  },
  conflictClaim: {
    upholds: "atomic first-writer discrimination via an explicit insert/update marker",
    denies: "replay truth across statements — the ledger row carries the receipt",
  },
  merge: {
    upholds: "multi-action conditional upsert with old/new evidence",
    denies: "ON CONFLICT-equivalent race serialization — concurrent inserts still error",
  },
  copy: {
    upholds: "maximal-throughput bulk ingest/egress under WAL",
    denies: "per-row error routing — batch atomicity is all-or-nothing",
  },
  partition: {
    upholds: "declarative lifecycle sharding plus logical/streaming replication",
    denies: "automated partition premake/retention — the pg_partman row's charter",
  },
} as const

declare namespace Pg {
  type Primitive = keyof typeof _primitives
  type _PrimitiveRows<
    T extends { readonly [P in Primitive]: { readonly upholds: string; readonly denies: string } } = typeof _primitives,
  > = T
}
```

## [04]-[EXTENSION_MATRIX]

- Owner: the `_rows` anchor and assembled projections — ruled `{extension, floor, probeSql, capabilities, layer, flags}` rows, deployment flags, dependency pairs, the derived `Grant` union, the image projection, and per-dialect `_core` seed.
- Packages: none — extensions are deployment-image facts, never JS dependencies.
- Entry: `lane/capability.md` probes `Pg.rows` fail-closed at Layer construction and enforces `Pg.demands`; the image derivation consumes `Pg.image`; every retrieval, projection, maintenance, and retention gate reads the derived grant vocabulary.
- Growth: a new extension is one row — the unions, the probe roster, and the image projection move with it, zero consumer edits; a floor bump is a field edit; a new deployment fact is one `_flags` entry; a new dependency edge is one `_demands` pair.
- Law: the BM25 row is `vchord_bm25` — it pairs with the admitted VectorChord index engine and grants `bm25`; the trigram and phonetic contrib rows carry the fuzzy lanes beneath it, and core FTS remains the boolean-lexeme floor the relevance lane begins past.
- Law: VectorChord is the stronger drop-in over the pgvector contract — both rows grant `vector`, `vchord` alone grants `vchord`, and index-method selection reads the narrower grant; swapping the engine is an image change, never a query change.
- Law: the queue class has no extension row — the SKIP-LOCKED primitive and relay rows in `journal/append.md` own the shape, and visibility-timeout redelivery is an attempts/lease column pair; a second job-table paradigm is split-brain.
- Law: native `uuidv7()` subsumes the identity-mint extension class entirely — no row exists, and any image fact naming one is stale.
- Law: flags price deployment facts and derive from one tuple — `timescaledb` carries `tsl` (source-available licensing), `excludesSharding` (mutually exclusive with a sharding engine in one database), and `preload`; the `preload` flag on it, `pg_cron`, and `pg_stat_statements` marks the `shared_preload_libraries` demand the deploy plane's CNPG derivation filters on, so a new preload-demanding extension is a flag edit with zero deploy-plane code change; every flag travels into the image projection so the deployment derivation prices the roster, and a core-layer row carrying any flag joins the projection too — contrib ships in every image, but its deployment fact still needs the derivation to see it.
- Law: dependency demands are data — `_demands` pairs a row flag with the grant it requires (`requiresCron` demands `cron`), `lane/capability.md` refuses a flagged row whose demanded grant is absent, and the deploy plane's `_DEMANDS` table reads the same pairs; `pg_incremental`'s exactly-once checkpointed batch folds are the maintenance plane's incremental lane, admitted only where `pg_cron` also probes true.
- Law: `pg_parquet` grants the object-store COPY egress — `COPY TO/FROM` Parquet against the object plane — interchange only, never a query engine; the OLAP lane owns querying what it writes.
- Law: the standard probe is structural — a row without `probeSql` rides the one batched catalog scan `lane/capability.md` owns; `probeSql` exists ONLY as the exotic per-row override, so probe dispatch reads field presence, never string shape.

```typescript signature
import { Record } from "effect"

const _flags = ["tsl", "excludesSharding", "preload", "requiresCron"] as const

const _rows = {
  pgvector: { extension: "vector", floor: "0.8.3", capabilities: ["vector"], layer: "image", flags: [] },
  vchord: { extension: "vchord", floor: "1.1.1", capabilities: ["vector", "vchord"], layer: "image", flags: [] },
  vchord_bm25: { extension: "vchord_bm25", floor: "0.3.0", capabilities: ["bm25"], layer: "image", flags: [] },
  timescaledb: { extension: "timescaledb", floor: "2.28.2", capabilities: ["timeseries"], layer: "image", flags: ["tsl", "excludesSharding", "preload"] },
  pg_partman: { extension: "pg_partman", floor: "5.4.3", capabilities: ["partition"], layer: "image", flags: [] },
  pg_cron: { extension: "pg_cron", floor: "1.6.7", capabilities: ["cron"], layer: "image", flags: ["preload"] },
  pg_ivm: { extension: "pg_ivm", floor: "1.15", capabilities: ["ivm"], layer: "image", flags: [] },
  pg_incremental: { extension: "pg_incremental", floor: "1.5.0", capabilities: ["incremental"], layer: "image", flags: ["requiresCron"] },
  pg_duckdb: { extension: "pg_duckdb", floor: "1.1.1", capabilities: ["analytics"], layer: "image", flags: [] },
  pg_parquet: { extension: "pg_parquet", floor: "0.5.1", capabilities: ["parquet"], layer: "image", flags: [] },
  pg_graphql: { extension: "pg_graphql", floor: "1.6.1", capabilities: ["graphql"], layer: "image", flags: [] },
  pg_jsonschema: { extension: "pg_jsonschema", floor: "0.3.4", capabilities: ["jsonschema"], layer: "image", flags: [] },
  pgaudit: { extension: "pgaudit", floor: "18.0", capabilities: ["audit"], layer: "image", flags: [] },
  postgis: { extension: "postgis", floor: "3.6.2", capabilities: ["geo"], layer: "image", flags: [] },
  h3: { extension: "h3", floor: "4.5.0", capabilities: ["h3"], layer: "image", flags: [] },
  pg_stat_statements: { extension: "pg_stat_statements", floor: "0.0.0", capabilities: ["statements"], layer: "core", flags: ["preload"] },
  pg_trgm: { extension: "pg_trgm", floor: "0.0.0", capabilities: ["trigram"], layer: "core", flags: [] },
  fuzzystrmatch: { extension: "fuzzystrmatch", floor: "0.0.0", capabilities: ["phonetic", "fuzzy"], layer: "core", flags: [] },
} as const

const _demands = [["requiresCron", "cron"]] as const

const _core = {
  pg: [..._spine, ...Record.keys(_primitives)],
  sqlite: [],
} as const

declare namespace Pg {
  type Kind = keyof typeof _rows
  type Row = (typeof _rows)[Kind]
  type Flag = (typeof _flags)[number]
  type Grant = Spine | Primitive | Row["capabilities"][number]
  type Demand = (typeof _demands)[number]
  type Image = ReadonlyArray<{ readonly extension: string; readonly floor: string; readonly flags: ReadonlyArray<Flag> }>
  type _Rows<T extends { readonly [P in Kind]: {
    readonly extension: string
    readonly floor: string
    readonly probeSql?: string
    readonly capabilities: ReadonlyArray<string>
    readonly layer: "image" | "core"
    readonly flags: ReadonlyArray<Flag>
  } } = typeof _rows> = T
  type _Demands<T extends ReadonlyArray<readonly [Flag, Grant]> = typeof _demands> = T
}
```

## [05]-[DRIVER_ROWS]

- Owner: the two driver Layer mints over `PgClient` — the `Config`-wrapped per-database row and the shared-pool adoption row `lane/tenant.md` fans tenant scopes across; the listener bus and jsonb fragment ride the concrete Tag, while transactional notification remains a neutral-statement operation; the assembled `Pg` export closes the page at the profile cluster.
- Packages: `@effect/sql-pg` (`PgClient.layer`, `PgClient.layerConfig`, `PgClient.layerFromPool`, `PgClient.listen`, `PgClient.notify`, `PgClient.json`); `effect` (`Config`, `Duration`, `Redacted`, `Schedule`).
- Entry: `lane/tenant.md` composes `Pg.client` and `Pg.fromPool` inside its `Stores` lookup; `journal/append.md` owns the optional `PgClient.listen` subscription as `Journal.wake(app)` and writes `pg_notify(channel, payload)` through its transaction-bound neutral client.
- Growth: a pool knob is a `Config` field on `_client` (`maxConnections`, `connectionTTL`, `idleTimeout` are the standing rows); a second physical spine (a read replica) is one more mint call with its own database coordinate, keyed by the scope that owns it.
- Law: domain rows yield the neutral `SqlClient` — the concrete Tag is reached only for `listen`/`json`; `PgClient.notify` calls the pool directly and is therefore rejected for a pulse whose contract is transaction-gated. A row typed against `PgClient` while composing no pg-native member blocks every other lane and is the named defect.
- Law: credentials are `Redacted` and pool budgets are `Config` facts — `url`, `maxConnections`, `connectionTTL`, `idleTimeout` never appear as literals in any row; `applicationName` pins the span-to-`pg_stat_activity` correlation and defaults from config so a fleet of processes disambiguates itself.
- Law: the shared-pool row is the tenancy fan-out primitive — one app-owned pool acquired once, adopted by every row-scoped and schema-scoped tenant Layer through `layerFromPool`, so a diamond of N apps on one database costs one pool.
- Law: construction is resilient at the Layer value — `Layer.retry` under the jittered bounded schedule rides both mints, gated by `Schedule.whileInput` to the `SqlError` tag so a malformed config fails immediately while a transient pool-acquire refusal at boot re-attempts as graph policy; a persistent refusal still fails typed after the budget.

```typescript signature
import { Array, Config, type ConfigError, Duration, Layer, Option, Record, Schedule } from "effect"
import type { SqlClient, SqlError } from "@effect/sql"
import { PgClient } from "@effect/sql-pg"

const _BOOT = Schedule.exponential("250 millis").pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(5)),
  Schedule.whileInput((fault: ConfigError.ConfigError | SqlError.SqlError) => fault._tag === "SqlError"),
)

const _client = (database: string): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  PgClient.layerConfig({
    url: Config.redacted("DATA_PG_URL"),
    database: Config.succeed(database),
    maxConnections: Config.integer("DATA_PG_POOL_MAX").pipe(Config.withDefault(16)),
    connectionTTL: Config.duration("DATA_PG_CONN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
    idleTimeout: Config.duration("DATA_PG_IDLE_TIMEOUT").pipe(Config.withDefault(Duration.minutes(1))),
    applicationName: Config.string("DATA_PG_APP").pipe(Config.withDefault("data")),
  }).pipe(Layer.retry(_BOOT))

const _fromPool = (
  acquire: PgClient.PgClientFromPoolOptions["acquire"],
): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, SqlError.SqlError> =>
  PgClient.layerFromPool({ acquire }).pipe(Layer.retry(_BOOT))

```

## [06]-[PROFILE_HARVEST]

- Owner: the ONE engine-profile receipt family — `Pg.Profile`, the schema-owned per-query evidence shape — with spine harvest arms and assembled `Pg` export: `_statements` decodes snapshots, `_delta` folds window receipts by `queryid`, and `_explain` folds JSON plans into operator rows.
- Packages: `@effect/sql` (`SqlSchema`, `Statement` — the profiled statement arrives as a composed `Fragment` value, never a string); `effect` (`Schema`, `DateTime`, `HashMap`, `Option`).
- Entry: `lane/sqlite.md` and `lane/olap.md` harvest each admitted profile engine into this same class through their own arms; the maintenance composition that owns the harvest cadence projects each receipt's `wallMillis` onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine`, and the receipt stays the truth the instrument lossily projects.
- Receipt: `Pg.Profile` — `{ engine, statement, wallMillis, rows, operators, counters, window }` — operator timing and cardinality are `Option`-carried because engines expose asymmetric depth, `counters` is the open engine-specific evidence record, and `window` is `Option`-carried because only cumulative-source arms (statements) carry one; an absent counter is omission, never a zero forgery.
- Growth: a new engine arm is one `_PROFILE_ENGINES` key with its owning harvest fence; a new evidence axis is a `counters` entry; a statements column is a `_StatRow` field and `_delta` line.
- Law: `pg_stat_statements` is cumulative shared state — receipts are `_delta` window deltas keyed by `queryid`, never raw counters; any backwards counter marks a reset and makes the later snapshot the whole delta, so no receipt turns negative. Calls-floor gating applies to the WINDOW delta, never the snapshot — snapshots retain the full row set for prior-state matching, so a query crossing the floor mid-window reports only its window increment, never its cumulative history baselined as new.
- Law: `EXPLAIN (ANALYZE, FORMAT JSON)` EXECUTES the statement — the arm scopes to explicit diagnosis calls, never ambient reads, and the profiled statement is a `Fragment` spliced whole, so parameter binding survives and no probe re-derives SQL by string assembly.
- Law: the statements row rides `_rows` as a core-layer contrib carrying `preload` — `lane/capability.md`'s batched catalog probe inherits it with zero probe edits, the `statements` grant gates both arms fail-closed, and the flag-bearing core row reaches the image projection so the deploy derivation configures `shared_preload_libraries`.

```typescript signature
import { DateTime, HashMap } from "effect"
import type { Statement } from "@effect/sql"
import { SqlSchema } from "@effect/sql"

const _PROFILE_ENGINES = ["pg", "sqliteServer", "sqliteWasm", "libsql", "d1", "duckdbNode"] as const

class _Profile extends Schema.Class<_Profile>("Pg.Profile")({
  engine: Schema.Literal(..._PROFILE_ENGINES),
  statement: Schema.NonEmptyString,
  wallMillis: Schema.NonNegative,
  rows: Schema.NonNegativeInt,
  operators: Schema.Array(Schema.Struct({
    name: Schema.NonEmptyString,
    millis: Schema.OptionFromNullOr(Schema.NonNegative),
    rows: Schema.OptionFromNullOr(Schema.NonNegativeInt),
  })),
  counters: Schema.Record({ key: Schema.String, value: Schema.Number }),
  window: Schema.OptionFromNullOr(Schema.Struct({ opened: Schema.DateTimeUtc, closed: Schema.DateTimeUtc })),
}) {}

const _StatRow = Schema.Struct({
  queryid: Schema.String,
  query: Schema.String,
  calls: Schema.Number,
  total_exec_time: Schema.Number,
  rows: Schema.Number,
  shared_blks_hit: Schema.Number,
  shared_blks_read: Schema.Number,
  wal_bytes: Schema.Number,
})

declare namespace Pg {
  type ProfileEngine = (typeof _PROFILE_ENGINES)[number]
  type Profile = _Profile
  type StatSnapshot = { readonly at: DateTime.Utc; readonly rows: ReadonlyArray<typeof _StatRow.Type> }
}

// Snapshots are UNFILTERED — the floor is a window-delta gate, never a snapshot gate: filtering the snapshot
// drops below-floor rows from the prior map, so a query crossing the floor mid-window baselines to zero and
// reports its whole cumulative history as one window's delta.
const _statements = (sql: SqlClient.SqlClient) =>
  Effect.map(
    Effect.zip(
      DateTime.now,
      SqlSchema.findAll({
        Request: Schema.Void,
        Result: _StatRow,
        execute: () =>
          sql`SELECT queryid::text AS queryid, query, calls, total_exec_time, rows,
                     shared_blks_hit, shared_blks_read, wal_bytes
              FROM pg_stat_statements`,
      })(void 0),
    ),
    ([at, rows]): Pg.StatSnapshot => ({ at, rows }),
  )

const _continued = (earlier: typeof _StatRow.Type, closed: typeof _StatRow.Type): boolean =>
  earlier.calls <= closed.calls &&
  earlier.total_exec_time <= closed.total_exec_time &&
  earlier.rows <= closed.rows &&
  earlier.shared_blks_hit <= closed.shared_blks_hit &&
  earlier.shared_blks_read <= closed.shared_blks_read &&
  earlier.wal_bytes <= closed.wal_bytes

const _baseline = (row: typeof _StatRow.Type): typeof _StatRow.Type => ({
  ...row,
  calls: 0,
  total_exec_time: 0,
  rows: 0,
  shared_blks_hit: 0,
  shared_blks_read: 0,
  wal_bytes: 0,
})

const _profileDelta = (
  earlier: typeof _StatRow.Type,
  row: typeof _StatRow.Type,
  opened: Pg.StatSnapshot,
  closed: Pg.StatSnapshot,
): Option.Option<Pg.Profile> =>
  row.calls === earlier.calls
    ? Option.none()
    : Option.some(new _Profile({
          engine: "pg",
          statement: row.query,
          wallMillis: row.total_exec_time - earlier.total_exec_time,
          rows: Math.max(0, Math.trunc(row.rows - earlier.rows)),
          operators: [],
          counters: {
            calls: row.calls - earlier.calls,
            sharedHit: row.shared_blks_hit - earlier.shared_blks_hit,
            sharedRead: row.shared_blks_read - earlier.shared_blks_read,
            walBytes: row.wal_bytes - earlier.wal_bytes,
          },
          window: Option.some({ opened: opened.at, closed: closed.at }),
      }))

const _deltaRows = (
  prior: HashMap.HashMap<string, typeof _StatRow.Type>,
  opened: Pg.StatSnapshot,
  closed: Pg.StatSnapshot,
  floor: number,
): ReadonlyArray<Pg.Profile> =>
  Array.filterMap(closed.rows, (row) =>
    Option.flatMap(
      Option.filter(
        Option.some(Option.getOrElse(
          Option.filter(HashMap.get(prior, row.queryid), (held) => _continued(held, row)),
          () => _baseline(row),
        )),
        (earlier) => row.calls - earlier.calls >= floor,
      ),
      (earlier) => _profileDelta(earlier, row, opened, closed),
    ))

const _delta = (opened: Pg.StatSnapshot, closed: Pg.StatSnapshot, floor: number): ReadonlyArray<Pg.Profile> =>
  _deltaRows(HashMap.fromIterable(Array.map(opened.rows, (row) => [row.queryid, row] as const)), opened, closed, floor)

interface _PlanNodeEncoded {
  readonly "Node Type": string
  readonly "Actual Total Time": number
  readonly "Actual Rows": number
  readonly Plans?: ReadonlyArray<_PlanNodeEncoded>
}

interface _PlanNode {
  readonly "Node Type": string
  readonly "Actual Total Time": number
  readonly "Actual Rows": number
  readonly Plans: Option.Option<ReadonlyArray<_PlanNode>>
}

const _Node: Schema.Schema<_PlanNode, _PlanNodeEncoded> = Schema.Struct({
  "Node Type": Schema.String,
  "Actual Total Time": Schema.Number,
  "Actual Rows": Schema.Number,
  Plans: Schema.optionalWith(Schema.Array(Schema.suspend((): Schema.Schema<_PlanNode, _PlanNodeEncoded> => _Node)), { as: "Option" }),
})

const _Report = Schema.Array(Schema.Struct({ Plan: _Node, "Execution Time": Schema.Number }))
const _ExplainRow = Schema.Struct({ "QUERY PLAN": _Report })

const _operators = (node: _PlanNode): ReadonlyArray<Pg.Profile["operators"][number]> => [
  { name: node["Node Type"], millis: Option.some(node["Actual Total Time"]), rows: Option.some(Math.trunc(node["Actual Rows"])) },
  ...Option.match(node.Plans, { onNone: () => [], onSome: Array.flatMap(_operators) }),
]

const _explain = (sql: SqlClient.SqlClient) =>
  (statement: Statement.Fragment, label: string) =>
    Effect.map(
      SqlSchema.findAll({
        Request: Schema.Void,
        Result: _ExplainRow,
        execute: () => sql`EXPLAIN (ANALYZE, FORMAT JSON) ${statement}`,
      })(void 0),
      Array.flatMap((row) => Array.map(row["QUERY PLAN"], (plan) =>
        new _Profile({
          engine: "pg",
          statement: label,
          wallMillis: plan["Execution Time"],
          rows: Math.trunc(plan.Plan["Actual Rows"]),
          operators: _operators(plan.Plan),
          counters: {},
          window: Option.none(),
        }))),
    )

const Pg = {
  spine: _spine,
  primitives: _primitives,
  ..._rows,
  Profile: _Profile,
  profile: { engines: _PROFILE_ENGINES, statements: _statements, delta: _delta, explain: _explain },
  rows: Record.values(_rows),
  image: Array.filterMap(Record.values(_rows), (row) =>
    row.layer === "image" || Array.isNonEmptyReadonlyArray(row.flags)
      ? Option.some({ extension: row.extension, floor: row.floor, flags: row.flags })
      : Option.none()),
  core: _core,
  demands: _demands,
  client: _client,
  fromPool: _fromPool,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Pg }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
