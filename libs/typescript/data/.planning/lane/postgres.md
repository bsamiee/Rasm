# [DATA_POSTGRES]

PostgreSQL's guarantee-lane spine owns first-party capabilities, concurrency primitives with explicit denials, ruled extensions, and driver Layers binding neutral `SqlClient` to pg. Rows and Layer mints carry every fact; `lane/capability.md` probes, `lane/tenant.md` scopes, and journal statements consume the grant vocabulary. ONE derived union binds spine, primitives, extensions, sqlite degradation, gates, and deployment image. Capability is one row; pruning an extension deletes its row and image fact.

## [01]-[INDEX]

- [02]-[SPINE_ROWS]: first-party capability rows — identity mint, derivation, evidence, integrity forms.
- [03]-[PRIMITIVE_TABLE]: concurrency/queue primitives with their upholds AND denies columns.
- [04]-[EXTENSION_MATRIX]: ruled extension rows, derived grant union, demands, image projection.
- [05]-[DRIVER_ROWS]: `PgClient` Layer mints, listener bus, jsonb fragment.
- [06]-[PROFILE_HARVEST]: one engine-profile receipt family beside the spine's statements and EXPLAIN arms.

## [02]-[SPINE_ROWS]

- Owner: the `_spine` key tuple — one closed key per first-party engine capability; the derived `Pg.Spine` union is the first-party half of the grant vocabulary the sqlite degradation table mirrors row for row, and the SQL idiom each key names lives in this cluster's law lines, spliced by ensure authors as settled fact.
- Packages: none — the rows are engine facts, not packages.
- Entry: `journal/append.md` composes `uuidv7()` defaults and `RETURNING` evidence per these laws and ships the `partition` spine without the stream unique; ensure authors read `temporal` for range-exclusive constraints; `lane/tenant.md` predicates the `rls` row's policy family; `lane/capability.md` seeds the pg granted set from `Pg.core`.
- Growth: a new engine capability is one tuple key — the grant union, the sqlite mirror row, and every gate inherit it; a capability subsumed by a newer engine form deletes its extension row and lands here.
- Law: `uuidv7` is the identity-mint row — timestamp-ordered, index-local, keyset-paginatable; the extension that duplicated it is pruned from the matrix, and a surrogate key column defaults `uuidv7()` on the pg spine while the sqlite lanes mint in-app through the degradation row.
- Law: `returningOldNew` is the single-statement evidence form — `RETURNING old.*, new.*` on INSERT/UPDATE/DELETE/MERGE discriminates insert-versus-update and yields before/after evidence without a second scan or a trigger; receipt-bearing writes splice it instead of re-reading.
- Law: `virtualGenerated` is the compute-on-read default — a derived column costs no write amplification; `STORED` is the explicit opt-in an ensure states only where read-path cost dominates.
- Law: `temporal` is constraint-level range integrity — `WITHOUT OVERLAPS` keys and `PERIOD` foreign keys move validity-window enforcement into the engine; an application-level overlap check beside an available temporal constraint is the named defect. Scalar-keyed temporal constraints index through GiST, so an ensure pairing a scalar tenant or entity column with a validity range gates on `gistScalar` and the contrib row granting it — omitting that gate authors DDL a stock build refuses at constraint creation.
- Law: `skipScan` widens every multicolumn index — a missing leading-column predicate no longer forces a second index; index ensures are authored against the widest query family, not per-predicate.
- Law: `asyncIo` is the read-path throughput row — `io_method` is a deployment fact the image projection carries implicitly; no statement composes it, and the row exists so the degradation table answers it honestly.
- Law: `rls` is the tenancy predicate plane — row-level security policies read the transaction-local GUC; the policy DDL and the pinning transformer are `lane/tenant.md`'s, and this row exists so the grant vocabulary names what the sqlite lanes replace with residency.

```typescript
const _spine = ["uuidv7", "returningOldNew", "virtualGenerated", "temporal", "skipScan", "asyncIo", "rls"] as const

declare namespace Pg {
  type Spine = (typeof _spine)[number]
}
```

## [03]-[PRIMITIVE_TABLE]

- Owner: the `_primitives` anchor — one row per first-party concurrency/queue primitive, each carrying `upholds` and `denies`, beside `_primitiveKeys`, the name roster consumers gate on; the denies column is the table's reason to exist, because every composed correctness lane is built from what a primitive refuses as much as from what it grants.
- Packages: `effect` (`Record`) — the rows themselves are engine facts, never packages.
- Entry: `journal/append.md` composes `advisory` for OCC serialization, `conflictClaim` for the idempotency ledger, `skipLocked` for relay claims, and `channel` for the post-commit wake; the projection and work drains read the same rows through their `SqlClient` ports.
- Growth: a new primitive is one row with both columns filled — a row missing its denies column is an unfinished admission — and `Pg.primitiveKeys` carries it to every consumer with zero consumer edits.
- Law: this table is the ONE primitive roster, published as a value beside its type — a consumer gating on primitive names (the deploy plane's pooling-mode intersection) reads `Pg.primitiveKeys` and refuses an unknown name; a transcribed key set drifts the day a row lands.
- Law: `serializable` is engine-enforced true snapshot isolation — predicate locks refuse a read-write cycle at COMMIT instead of blocking readers, so an invariant spanning rows no single statement touches needs no advisory lock; what it refuses is commit certainty, and a serialization failure is a retryable typed refusal the caller re-runs whole from the same inputs. Version-check OCC and this row are alternatives, never a stack.
- Law: two-phase commit earns no row — `PREPARE TRANSACTION` needs a nonzero `max_prepared_transactions` no stock image sets, and an orphaned prepared transaction pins the xmin horizon until an operator resolves it by hand; `journal/append.md`'s transactional outbox exists BECAUSE of that refusal, so cross-store atomicity commits once locally and relays after, never a distributed vote.
- Law: `skipLocked` claims exactly-one-live-transaction, never delivery — a crashed claimant releases silently, so every drain pairs it with a visibility or attempts column for redelivery; global ordering under concurrency is refused by construction and priority is an `ORDER BY` term, never an assumption.
- Law: `channel` is a transactional wake pulse, never a queue — delivery fires only on COMMIT, deduped per channel/payload per transaction, absent listeners hear nothing, and the async queue is bounded in-memory; every listener re-polls on reconnect and the pulse only collapses poll latency.
- Law: `conflictClaim` — `INSERT … ON CONFLICT DO UPDATE … RETURNING` with an explicit insert/update marker — is atomic first-writer discrimination without reading transaction internals; what it refuses is replay truth across statements, so the ledger row, never the claim, carries the stored receipt.
- Law: `advisory` locks die with their session or transaction — application-defined mutual exclusion without row DDL, refusing persistence; a lock protecting state across restarts is a schema row, never an advisory claim.
- Law: `copy` is the maximal-throughput bulk lane under WAL and refuses per-row error routing — batch atomicity is all-or-nothing, so a partial-tolerant ingest splits its batch above the statement.
- Law: `partition` (declarative partitioning with replication) refuses automated lifecycle — premake and retention drop are the `pg_partman` extension row's, and `journal/retain.md` gates on that grant.

```typescript
import { Record } from "effect"

const _primitives = {
  serializable: {
    upholds: "true snapshot isolation — predicate-safe read-write cycles refused at commit, readers never blocked",
    denies: "commit certainty under contention — a serialization failure is the caller's whole-unit retry",
  },
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

const _primitiveKeys = Record.keys(_primitives)

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
- Growth: a new extension is one row — the unions, the probe roster, and the image projection move with it, zero consumer edits; a floor bump is a field edit; a new deployment fact is one `_flags` entry; a new dependency edge is one `_demands` row, and a mutual exclusion is one `excludes` demand row against the grant it forbids.
- Law: the BM25 row is `vchord_bm25` — it pairs with the admitted VectorChord index engine and grants `bm25`; the trigram and phonetic contrib rows carry the fuzzy lanes beneath it, and core FTS remains the boolean-lexeme floor the relevance lane begins past.
- Law: VectorChord is the stronger drop-in over the pgvector contract — both rows grant `vector`, `vchord` alone grants `vchord`, and index-method selection reads the narrower grant; swapping the engine is an image change, never a query change.
- Law: the queue class has no extension row — the SKIP-LOCKED primitive and relay rows in `journal/append.md` own the shape, and visibility-timeout redelivery is an attempts/lease column pair; a second job-table paradigm is split-brain.
- Law: native `uuidv7()` subsumes the identity-mint extension class entirely — no row exists, and any image fact naming one is stale.
- Law: flags price deployment facts and derive from one tuple — `timescaledb` carries `tsl` (source-available licensing) and `preload`; the `preload` flag on it, `pg_cron`, and `pg_stat_statements` marks the `shared_preload_libraries` demand the deploy plane's CNPG derivation filters on, so a new preload-demanding extension is a flag edit with zero deploy-plane code change; every flag travels into the image projection so the deployment derivation prices the roster, and a core-layer row carrying any flag joins the projection too — contrib ships in every image, but its deployment fact still needs the derivation to see it.
- Law: dependency demands are data — `_demands` rows a relation with the flag it applies to and the grant it names (`requires` pairs `requiresCron` with `cron`), `lane/capability.md` refuses a flagged row whose `requires` grant is absent or whose `excludes` grant is present, and the deploy plane reads the same rows; `pg_incremental`'s exactly-once checkpointed batch folds are the maintenance plane's incremental lane, admitted only where `pg_cron` also probes true.
- Law: `_backend` pairs each contract capability key with the grant that proves it, so backend observation resolves against the one granted set; an identity extension-to-extension row reads version evidence instead and reports every core-seeded grant missing.
- Law: `_core` is the pg dialect seed alone — a sqlite profile seeds the grants its own `lane/sqlite.md` degradation row proves native, so no pg-authored arm speaks for a lane it cannot probe.
- Law: `pg_parquet` grants the object-store COPY egress — `COPY TO/FROM` Parquet against the object plane — interchange only, never a query engine; the OLAP lane owns querying what it writes.
- Law: `btree_gist` is the scalar-key operator-class row — GiST indexes range and geometric types alone on a stock build, so a `WITHOUT OVERLAPS` key or exclusion constraint mixing a scalar column with a validity range needs it before the spine's `temporal` grant means anything; contrib ships it in every image and the embedded pin carries it too, so the gate costs a probe rather than an image change.
- Law: the standard probe is structural — a row without `probeSql` rides the one batched catalog scan `lane/capability.md` owns; `probeSql` exists ONLY as the exotic per-row override, so probe dispatch reads field presence, never string shape.

```typescript
import { Record } from "effect"
import type { Backend, Capability } from "./capability.ts"

const _flags = ["tsl", "preload", "requiresCron"] as const

const _rows = {
  pgvector: { extension: "vector", floor: "0.8.3", capabilities: ["vector"], layer: "image", flags: [] },
  vchord: { extension: "vchord", floor: "1.1.1", capabilities: ["vector", "vchord"], layer: "image", flags: [] },
  vchord_bm25: { extension: "vchord_bm25", floor: "0.3.0", capabilities: ["bm25"], layer: "image", flags: [] },
  timescaledb: { extension: "timescaledb", floor: "2.28.2", capabilities: ["timeseries"], layer: "image", flags: ["tsl", "preload"] },
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
  btree_gist: { extension: "btree_gist", floor: "0.0.0", capabilities: ["gistScalar"], layer: "core", flags: [] },
  pg_stat_statements: { extension: "pg_stat_statements", floor: "0.0.0", capabilities: ["statements"], layer: "core", flags: ["preload"] },
  pg_trgm: { extension: "pg_trgm", floor: "0.0.0", capabilities: ["trigram"], layer: "core", flags: [] },
  fuzzystrmatch: { extension: "fuzzystrmatch", floor: "0.0.0", capabilities: ["phonetic", "fuzzy"], layer: "core", flags: [] },
} as const

const _demands = [{ relation: "requires", flag: "requiresCron", grant: "cron" }] as const

const _backend = Record.values(_rows).flatMap((row) =>
  row.capabilities.map((grant) => ({ canonical: row.extension, local: grant }))
) satisfies ReadonlyArray<Backend.Adapter>

const _core = [..._spine, ..._primitiveKeys] as const

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
  type _Demands<T extends ReadonlyArray<Capability.Demand<Flag, Grant>> = typeof _demands> = T
}
```

## [05]-[DRIVER_ROWS]

- Owner: the two driver Layer mints over `PgClient` — the `Config`-wrapped per-database row and the shared-pool adoption row `lane/tenant.md` fans tenant scopes across; the listener bus rides the concrete Tag, while transactional notification remains a neutral-statement operation; the assembled `Pg` export closes the page at the profile cluster.
- Packages: `@effect/sql-pg` (`PgClient.layer`, `PgClient.layerConfig`, `PgClient.layerFromPool`, `PgClient.PgClientConfig`, `PgClient.listen`, `PgClient.notify`, `PgClient.makeCompiler`); `effect` (`Config`, `Duration`, `Predicate`, `Redacted`); `@rasm/core` (`Fault.Budget`).
- Entry: `lane/tenant.md` composes `Pg.client` and `Pg.fromPool` inside its `Stores` lookup; `journal/append.md` owns the optional `PgClient.listen` subscription as `Journal.wake(app)` and writes `pg_notify(channel, payload)` through its transaction-bound neutral client.
- Growth: a pool or transport knob is one `Config` field on `_coordinate` (`maxConnections`, `minConnections`, `connectTimeout`, `connectionTTL`, `idleTimeout`, `ssl` are the standing rows); a second physical spine (a read replica) is one more mint call with its own database coordinate, keyed by the scope that owns it.
- Law: domain rows yield the neutral `SqlClient` — the concrete Tag is reached for `listen` and for `makeCompiler`, the embedded profile's own neutral-fragment compiler, and for nothing else; `PgClient.notify` calls the pool directly and is therefore rejected for a pulse whose contract is transaction-gated. Typing a row against `PgClient` while it composes no pg-native member blocks every other lane, the named defect.
- Law: the jsonb crossing carries NO fragment helper on this lane, because every payload column this branch writes is bound as a statement PARAMETER whose type the target column resolves — `Model.JsonFromString` is the one owner and the driver's own inference is the whole mechanism; a `json` fragment admitted here is capability with no call site, and the first page reaching for it instead of the model forks how a payload crosses.
- Law: coordinates are discrete `Config` fields and NEVER a DSN beside a database name — the driver hands its config record to the node client, which re-parses `connectionString` over that record, so a `database` argument passed with a `url` loses to whatever the DSN spells and loses to null when it spells none; every dedicated-database scope then serves from the spine's own database, type-checked, booted, and probed green. Passwords ride `Redacted`, budgets and deadlines ride `Config`, and `applicationName` pins the span-to-`pg_stat_activity` correlation so a fleet of processes disambiguates itself.
- Law: `connectTimeout` is the boot schedule's precondition — construction probes one round trip under exactly this deadline and fails typed, so an unreachable host surfaces as a `SqlError` the schedule retries rather than a construction that hangs past every budget and reaches no retry at all; `minConnections` pre-warms a dedicated spine and `ssl` defaults closed.
- Law: the shared-pool row is the tenancy fan-out primitive — one app-owned pool acquired once, adopted by every row-scoped and schema-scoped tenant Layer through `layerFromPool`, so a diamond of N apps on one database costs one pool.
- Law: construction is resilient at the Layer value — `Layer.retry` rides both mints under the branch's ONE retry owner, `Fault.Budget.schedule("lease")`, whose compiled curve already carries jitter, reset, attempt bound, and elapsed ceiling, so this lane spells no cadence of its own; its gate is the only argument it states, because a boot fault is one of two tags and the default class gate reads a `class` property `SqlError` does not carry — the tag predicate therefore rides explicitly, a malformed config fails immediately, a transient pool-acquire refusal re-attempts as graph policy, and a persistent refusal still fails typed at the budget.

```typescript
import { Array, Config, type ConfigError, Duration, Layer, Option, Predicate, Record } from "effect"
import type { SqlClient, SqlError } from "@effect/sql"
import { PgClient } from "@effect/sql-pg"
import { Fault } from "@rasm/core"

const _BOOT = Fault.Budget.schedule("lease", Predicate.isTagged("SqlError"))

const _coordinate = (database: string): Config.Config.Wrap<PgClient.PgClientConfig> => ({
  host: Config.string("DATA_PG_HOST"),
  port: Config.integer("DATA_PG_PORT").pipe(Config.withDefault(5432)),
  username: Config.string("DATA_PG_USER"),
  password: Config.redacted("DATA_PG_PASSWORD"),
  database: Config.succeed(database),
  ssl: Config.boolean("DATA_PG_SSL").pipe(Config.withDefault(true)),
  maxConnections: Config.integer("DATA_PG_POOL_MAX").pipe(Config.withDefault(16)),
  minConnections: Config.integer("DATA_PG_POOL_MIN").pipe(Config.withDefault(0)),
  connectTimeout: Config.duration("DATA_PG_CONNECT_TIMEOUT").pipe(Config.withDefault(Duration.seconds(5))),
  connectionTTL: Config.duration("DATA_PG_CONN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
  idleTimeout: Config.duration("DATA_PG_IDLE_TIMEOUT").pipe(Config.withDefault(Duration.minutes(1))),
  applicationName: Config.string("DATA_PG_APP").pipe(Config.withDefault("data")),
})

const _client = (database: string): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  PgClient.layerConfig(_coordinate(database)).pipe(Layer.retry(_BOOT))

const _fromPool = (
  acquire: PgClient.PgClientFromPoolOptions["acquire"],
): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, SqlError.SqlError> =>
  PgClient.layerFromPool({ acquire }).pipe(Layer.retry(_BOOT))

```

## [06]-[PROFILE_HARVEST]

- Owner: the ONE engine-profile receipt family — `Pg.Profile`, the schema-owned per-query evidence shape — with spine harvest arms and assembled `Pg` export: `_CUMULATIVE`/`_COUNTER` publish the statement-column roster every fold reads, `_statements` decodes snapshots, `_delta` folds window receipts by the view's whole identity key, and `_explain` folds JSON plans into loop-corrected operator rows beside plan-wide counters.
- Packages: `@effect/sql` (`SqlSchema`, `Statement` — the profiled statement arrives as a composed `Fragment` value, never a string; `sql.csv` and `sql.literal` splice the page's own sealed rosters); `effect` (`Schema`, `Array`, `DateTime`, `HashMap`, `Option`, `Record`).
- Entry: `lane/sqlite.md` and `lane/olap.md` harvest each admitted profile engine into this same class through their own arms; the maintenance composition that owns the harvest cadence projects each receipt's `wallMillis` onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine`, and the receipt stays the truth the instrument lossily projects.
- Receipt: `Pg.Profile` — `{ engine, statement, wallMillis, rows, operators, counters, window }` — operator timing and cardinality are `Option`-carried because engines expose asymmetric depth, `counters` is the open engine-specific evidence record, and `window` is `Option`-carried because only cumulative-source arms (statements) carry one; an absent counter is omission, never a zero forgery.
- Growth: a new engine arm is one `_PROFILE_ENGINES` key with its owning harvest fence; a new evidence axis is a `counters` entry; a statements column is ONE `_CUMULATIVE` entry carrying its `_COUNTER` name, and the SELECT list, reset guard, baseline, and emitted counters all move with it.
- Law: `pg_stat_statements` is cumulative shared state — receipts are `_delta` window deltas, never raw counters; any backwards counter marks a reset and makes the later snapshot the whole delta, so no receipt turns negative. Calls-floor gating applies to the WINDOW delta, never the snapshot — snapshots retain the full row set for prior-state matching, so a query crossing the floor mid-window reports only its window increment, never its cumulative history baselined as new.
- Law: identity is the view's WHOLE key — one normalized query reports a separate row per role, per database, and per nesting level, so a fold keyed on `queryid` alone matches several closed rows against one prior row and emits each one's cumulative history as this window's delta; `_statKey` makes the match total and nested rows stay distinguishable from their top-level callers.
- Law: cumulative columns alone enter the fold — `min`, `max`, `mean`, and `stddev` exec-time columns are refused because subtraction recovers no windowed extremum from a pair of cumulative reads, and reading one as a counter reports the whole history's peak as this window's; the EXPLAIN arm answers the per-statement tail question those columns cannot.
- Law: `EXPLAIN` EXECUTES the statement under `ANALYZE` — the arm scopes to explicit diagnosis calls, never ambient reads, and the profiled statement is a `Fragment` spliced whole, so parameter binding survives and no probe re-derives SQL by string assembly; `_EXPLAIN` names the option set as one sealed roster, so widening the evidence a plan carries is one entry rather than a second hand-spelled statement.
- Law: plan arithmetic is PER-LOOP — `Actual Total Time` and `Actual Rows` are averages over `Actual Loops`, so operator rows multiply through and a nested loop's inner side reports its real share instead of a fraction; plan-wide counters carry the worst cardinality misestimate in the tree beside the spill and read-block tallies `BUFFERS` prices, because a bad shape traced to statistics and a bad shape traced to indexing take different repairs.
- Law: both PostgreSQL engines run BOTH arms and each stamps the caller's engine — the embedded pin ships `pg_stat_statements` in its own contrib set, so the statements arm gates on the `statements` GRANT the capability probe publishes rather than on an engine name; an engine literal inside the fold refuses evidence the deployment demonstrably carries.
- Law: both arms take their engine AT THE CALL — `_delta` reads an engine argument and `_explain` carries no dialect default — so no receipt stamps an engine it merely assumed; this branch declares zero construction sites for either, a recorded negative rather than an unrepaired caller, because the maintenance composition owning the harvest cadence lands outside it and supplies the engine from the scope that already selected the profile; minting a default here to fill that absence stamps `pg` onto every PGLite receipt the first composition produces.
- Law: the statements row rides `_rows` as a core-layer contrib carrying `preload` — `lane/capability.md`'s batched catalog probe inherits it with zero probe edits, the `statements` grant gates both arms fail-closed, and the flag-bearing core row reaches the image projection so the deploy derivation configures `shared_preload_libraries`.

```typescript
import { Array, DateTime, Effect, HashMap, Option, Record, Schema } from "effect"
import { SqlSchema, type SqlClient, type Statement } from "@effect/sql"

const _PG_DIALECT = ["pg", "pglite"] as const

const _PROFILE_ENGINES = [..._PG_DIALECT, "sqliteServer", "sqliteWasm", "libsql", "d1", "duckdbNode", "duckdbWasm", "clickhouse"] as const

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

const _CUMULATIVE = [
  "calls", "total_exec_time", "total_plan_time", "plans", "rows",
  "shared_blks_hit", "shared_blks_read", "shared_blks_dirtied", "shared_blks_written",
  "temp_blks_read", "temp_blks_written", "wal_records", "wal_bytes",
] as const

const _COUNTER = {
  calls: "calls",
  total_exec_time: "execMillis",
  total_plan_time: "planMillis",
  plans: "plans",
  rows: "rows",
  shared_blks_hit: "sharedHit",
  shared_blks_read: "sharedRead",
  shared_blks_dirtied: "sharedDirtied",
  shared_blks_written: "sharedWritten",
  temp_blks_read: "tempRead",
  temp_blks_written: "tempWritten",
  wal_records: "walRecords",
  wal_bytes: "walBytes",
} as const satisfies Record<Pg.StatColumn, string>

const _overColumns = <V>(value: V): { readonly [P in Pg.StatColumn]: V } =>
  Object.fromEntries(Array.map(_CUMULATIVE, (column) => [column, value] as const)) as {
    readonly [P in Pg.StatColumn]: V
  }

const _StatRow = Schema.Struct({
  userid: Schema.String,
  dbid: Schema.String,
  queryid: Schema.String,
  toplevel: Schema.Boolean,
  query: Schema.String,
  ..._overColumns(Schema.Number),
})

const _statKey = (row: typeof _StatRow.Type): string => `${row.userid}:${row.dbid}:${row.queryid}:${row.toplevel}`

const _ZEROES: { readonly [P in Pg.StatColumn]: number } = _overColumns(0)

declare namespace Pg {
  type ProfileEngine = (typeof _PROFILE_ENGINES)[number]
  type PgEngine = (typeof _PG_DIALECT)[number]
  type Profile = _Profile
  type StatColumn = (typeof _CUMULATIVE)[number]
  type StatSnapshot = { readonly at: DateTime.Utc; readonly rows: ReadonlyArray<typeof _StatRow.Type> }
  type _Counters<T extends Record<StatColumn, string> = typeof _COUNTER> = T
}

const _statements = (sql: SqlClient.SqlClient) =>
  Effect.map(
    Effect.zip(
      DateTime.now,
      SqlSchema.findAll({
        Request: Schema.Void,
        Result: _StatRow,
        execute: () =>
          sql`SELECT userid::text AS userid, dbid::text AS dbid, queryid::text AS queryid, toplevel, query,
                     ${sql.csv([..._CUMULATIVE])}
              FROM pg_stat_statements`,
      })(void 0),
    ),
    ([at, rows]): Pg.StatSnapshot => ({ at, rows }),
  )

const _continued = (earlier: typeof _StatRow.Type, closed: typeof _StatRow.Type): boolean =>
  Array.every(_CUMULATIVE, (column) => earlier[column] <= closed[column])

const _baseline = (row: typeof _StatRow.Type): typeof _StatRow.Type => ({ ...row, ..._ZEROES })

const _counters = (earlier: typeof _StatRow.Type, row: typeof _StatRow.Type): Record<string, number> =>
  Record.fromEntries(Array.map(_CUMULATIVE, (column) => [_COUNTER[column], row[column] - earlier[column]] as const))

const _profileDelta = (
  engine: Pg.PgEngine,
  earlier: typeof _StatRow.Type,
  row: typeof _StatRow.Type,
  opened: Pg.StatSnapshot,
  closed: Pg.StatSnapshot,
): Option.Option<Pg.Profile> =>
  row.calls === earlier.calls
    ? Option.none()
    : Option.some(new _Profile({
          engine,
          statement: row.query,
          wallMillis: row.total_exec_time - earlier.total_exec_time,
          rows: Math.max(0, Math.trunc(row.rows - earlier.rows)),
          operators: [],
          counters: _counters(earlier, row),
          window: Option.some({ opened: opened.at, closed: closed.at }),
      }))

const _deltaRows = (
  engine: Pg.PgEngine,
  prior: HashMap.HashMap<string, typeof _StatRow.Type>,
  opened: Pg.StatSnapshot,
  closed: Pg.StatSnapshot,
  floor: number,
): ReadonlyArray<Pg.Profile> =>
  Array.filterMap(closed.rows, (row) =>
    Option.flatMap(
      Option.filter(
        Option.some(Option.getOrElse(
          Option.filter(HashMap.get(prior, _statKey(row)), (held) => _continued(held, row)),
          () => _baseline(row),
        )),
        (earlier) => row.calls - earlier.calls >= floor,
      ),
      (earlier) => _profileDelta(engine, earlier, row, opened, closed),
    ))

const _delta = (
  opened: Pg.StatSnapshot,
  closed: Pg.StatSnapshot,
  floor: number,
  engine: Pg.PgEngine,
): ReadonlyArray<Pg.Profile> =>
  _deltaRows(
    engine,
    HashMap.fromIterable(Array.map(opened.rows, (row) => [_statKey(row), row] as const)),
    opened,
    closed,
    floor,
  )

const _EXPLAIN = ["ANALYZE", "BUFFERS", "VERBOSE", "SETTINGS", "WAL", "FORMAT JSON"] as const

interface _PlanNodeEncoded {
  readonly "Node Type": string
  readonly "Actual Total Time": number
  readonly "Actual Rows": number
  readonly "Actual Loops": number
  readonly "Plan Rows": number
  readonly "Shared Read Blocks"?: number
  readonly "Temp Written Blocks"?: number
  readonly Plans?: ReadonlyArray<_PlanNodeEncoded>
}

interface _PlanNode {
  readonly "Node Type": string
  readonly "Actual Total Time": number
  readonly "Actual Rows": number
  readonly "Actual Loops": number
  readonly "Plan Rows": number
  readonly "Shared Read Blocks": Option.Option<number>
  readonly "Temp Written Blocks": Option.Option<number>
  readonly Plans: Option.Option<ReadonlyArray<_PlanNode>>
}

const _Node: Schema.Schema<_PlanNode, _PlanNodeEncoded> = Schema.Struct({
  "Node Type": Schema.String,
  "Actual Total Time": Schema.Number,
  "Actual Rows": Schema.Number,
  "Actual Loops": Schema.Number,
  "Plan Rows": Schema.Number,
  "Shared Read Blocks": Schema.optionalWith(Schema.Number, { as: "Option" }),
  "Temp Written Blocks": Schema.optionalWith(Schema.Number, { as: "Option" }),
  Plans: Schema.optionalWith(Schema.Array(Schema.suspend((): Schema.Schema<_PlanNode, _PlanNodeEncoded> => _Node)), { as: "Option" }),
})

const _Report = Schema.Array(Schema.Struct({ Plan: _Node, "Execution Time": Schema.Number }))
const _ExplainRow = Schema.Struct({ "QUERY PLAN": _Report })

const _operators = (node: _PlanNode): ReadonlyArray<Pg.Profile["operators"][number]> => [
  {
    name: node["Node Type"],
    millis: Option.some(node["Actual Total Time"] * node["Actual Loops"]),
    rows: Option.some(Math.trunc(node["Actual Rows"] * node["Actual Loops"])),
  },
  ...Option.match(node.Plans, { onNone: () => [], onSome: Array.flatMap(_operators) }),
]

const _nodes = (node: _PlanNode): ReadonlyArray<_PlanNode> => [
  node,
  ...Option.match(node.Plans, { onNone: () => [], onSome: Array.flatMap(_nodes) }),
]

const _ratio = (node: _PlanNode): number => {
  const actual = Math.max(node["Actual Rows"] * node["Actual Loops"], 1)
  const planned = Math.max(node["Plan Rows"], 1)
  return Math.max(actual / planned, planned / actual)
}

const _planCounters = (plan: _PlanNode): Record<string, number> =>
  Array.reduce(_nodes(plan), { misestimate: 1, tempWritten: 0, sharedRead: 0 }, (held, node) => ({
    misestimate: Math.max(held.misestimate, _ratio(node)),
    tempWritten: held.tempWritten + Option.getOrElse(node["Temp Written Blocks"], () => 0),
    sharedRead: held.sharedRead + Option.getOrElse(node["Shared Read Blocks"], () => 0),
  }))

const _explain = (sql: SqlClient.SqlClient, engine: Pg.PgEngine) =>
  (statement: Statement.Fragment, label: string) =>
    Effect.map(
      SqlSchema.findAll({
        Request: Schema.Void,
        Result: _ExplainRow,
        execute: () => sql`EXPLAIN ${sql.literal(`(${Array.join(_EXPLAIN, ", ")})`)} ${statement}`,
      })(void 0),
      Array.flatMap((row) => Array.map(row["QUERY PLAN"], (plan) =>
        new _Profile({
          engine,
          statement: label,
          wallMillis: plan["Execution Time"],
          rows: Math.trunc(plan.Plan["Actual Rows"] * plan.Plan["Actual Loops"]),
          operators: _operators(plan.Plan),
          counters: _planCounters(plan.Plan),
          window: Option.none(),
        }))),
    )

const Pg = {
  spine: _spine,
  primitives: _primitives,
  primitiveKeys: _primitiveKeys,
  Profile: _Profile,
  profile: { engines: _PROFILE_ENGINES, columns: _CUMULATIVE, counters: _COUNTER, statements: _statements, delta: _delta, explain: _explain },
  rows: Record.values(_rows),
  image: Array.filterMap(Record.values(_rows), (row) =>
    row.layer === "image" || Array.isNonEmptyReadonlyArray(row.flags)
      ? Option.some({ extension: row.extension, floor: row.floor, flags: row.flags })
      : Option.none()),
  core: _core,
  demands: _demands,
  backend: _backend,
  client: _client,
  fromPool: _fromPool,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
