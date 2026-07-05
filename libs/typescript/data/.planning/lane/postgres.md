# [DATA_POSTGRES]

The PostgreSQL spine of the guarantee-lane matrix: one sealed vocabulary owner carrying the first-party capability rows the engine grants outright, the concurrency-primitive table whose load-bearing column is what each primitive does NOT guarantee, the ruled extension matrix every probe and image derivation reads, and the driver Layer rows that bind the neutral `SqlClient` to the pooled pg wire. Everything here is data or a Layer mint — the fail-closed probe service is `lane/capability.md`'s, the per-scope Layer family is `lane/tenant.md`'s, and every journal statement composes these rows through the grant vocabulary instead of assuming the engine. A new engine capability is one row; a pruned extension is one deleted row plus its image fact; no consumer edit exists on either path.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                              |
| :-----: | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `SPINE_ROWS`       | the first-party capability rows — identity mint, derivation, evidence, integrity forms |
|  [02]   | `PRIMITIVE_TABLE`  | the concurrency/queue primitives with their upholds AND denies columns                  |
|  [03]   | `EXTENSION_MATRIX` | the ruled extension rows, the derived grant union, the image projection                 |
|  [04]   | `DRIVER_ROWS`      | the `PgClient` Layer mints, the notify bus, the jsonb fragment                          |

## [2]-[SPINE_ROWS]

- Owner: the `_spine` key tuple — one closed key per first-party engine capability; the derived `Pg.Spine` union is the in-core half of the grant vocabulary the sqlite degradation table mirrors, and the SQL idiom each key names lives in this cluster's law lines, spliced by ensure authors as settled fact.
- Packages: none — the rows are engine facts, not packages.
- Entry: `journal/append.md` composes `uuidv7()` defaults and `RETURNING` evidence per these laws; `journal/evolve.md` reads `virtualGenerated` for derived snapshot columns; ensure authors read `temporal` for range-exclusive constraints; `lane/capability.md` seeds the pg granted set from `Pg.core`.
- Growth: a new engine capability is one tuple key — the union, the sqlite mirror row, and every gate inherit it; a capability subsumed by a newer engine form deletes its extension row and lands here.
- Law: `uuidv7` is the identity-mint row — timestamp-ordered, index-local, keyset-paginatable; the extension that duplicated it is pruned from the matrix, and a surrogate key column defaults `uuidv7()` on the pg spine while the sqlite lanes mint in-app through the degradation row.
- Law: `returningOldNew` is the single-statement evidence form — `RETURNING old.*, new.*` on INSERT/UPDATE/DELETE/MERGE discriminates insert-versus-update and yields before/after evidence without a second scan or a trigger; receipt-bearing writes splice it instead of re-reading.
- Law: `virtualGenerated` is the compute-on-read default — a derived column costs no write amplification; `STORED` is the explicit opt-in an ensure states only where read-path cost dominates.
- Law: `temporal` is constraint-level range integrity — `WITHOUT OVERLAPS` keys and `PERIOD` foreign keys move validity-window enforcement into the engine; an application-level overlap check beside an available temporal constraint is the named defect.
- Law: `skipScan` widens every multicolumn index — a missing leading-column predicate no longer forces a second index; index ensures are authored against the widest query family, not per-predicate.

```typescript
const _spine = ["uuidv7", "returningOldNew", "virtualGenerated", "temporal", "skipScan", "asyncIo"] as const

declare namespace Pg {
  type Spine = (typeof _spine)[number]
}
```

## [3]-[PRIMITIVE_TABLE]

- Owner: the `_primitives` anchor — one row per first-party concurrency/queue primitive, each carrying `upholds` and `denies`; the denies column is the table's reason to exist, because every composed correctness lane is built from what a primitive refuses as much as from what it grants.
- Packages: none — engine facts.
- Entry: `journal/append.md` composes `advisory` for OCC serialization, `conflictClaim` for the idempotency ledger, `skipLocked` for relay claims, and `channel` for the post-commit wake; the projection and work drains read the same rows through their `SqlClient` ports.
- Growth: a new primitive is one row with both columns filled — a row missing its denies column is an unfinished admission.
- Law: `skipLocked` claims exactly-one-live-transaction, never delivery — a crashed claimant releases silently, so every drain pairs it with a visibility or attempts column for redelivery; global ordering under concurrency is refused by construction and priority is an `ORDER BY` term, never an assumption.
- Law: `channel` is a transactional wake pulse, never a queue — delivery fires only on COMMIT, deduped per channel/payload per transaction, absent listeners hear nothing, and the async queue is bounded in-memory; every listener re-polls on reconnect and the pulse only collapses poll latency.
- Law: `conflictClaim` — `INSERT … ON CONFLICT DO UPDATE RETURNING (xmax = 0)` — is the atomic first-writer discrimination and needs nothing further; it is the one primitive row admitted as complete.
- Law: `advisory` locks die with their session or transaction — application-defined mutual exclusion without row DDL, refusing persistence; a lock protecting state across restarts is a schema row, never an advisory claim.
- Law: `copy` is the maximal-throughput bulk lane under WAL and refuses per-row error routing — batch atomicity is all-or-nothing, so a partial-tolerant ingest splits its batch above the statement.
- Law: `partition` (declarative partitioning plus replication) refuses automated lifecycle — premake and retention drop are the `pg_partman` extension row's, and `journal/retain.md` gates on that grant.

```typescript
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
    upholds: "atomic first-writer discrimination via (xmax = 0)",
    denies: "",
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
    T extends Record<Primitive, { readonly upholds: string; readonly denies: string }> = typeof _primitives,
  > = T
}
```

## [4]-[EXTENSION_MATRIX]

- Owner: the `_rows` anchor plus the assembled projections — the ruled extension estate as `{extension, floor, probeSql, capabilities, layer, flags}` rows, the derived `Grant` union, the `{extension, floor}` image projection the deployment image realizes, and the per-dialect `_core` grant table.
- Packages: none — extensions are deployment-image facts, never JS dependencies.
- Entry: `lane/capability.md` probes `Pg.rows` fail-closed at Layer construction; the image derivation consumes `Pg.image`; every retrieval, projection, maintenance, and retention gate reads the derived grant vocabulary.
- Growth: a new extension is one row — the unions, the probe roster, and the image projection move with it, zero consumer edits; a floor bump is a field edit.
- Law: the BM25 row is `vchord_bm25` — it pairs with the admitted VectorChord index engine and grants `bm25`; the trigram and phonetic contrib rows carry the fuzzy lanes beneath it, and core FTS remains the boolean-lexeme floor the relevance lane begins past.
- Law: VectorChord is the stronger drop-in over the pgvector contract — both rows grant `vector`, `vchord` alone grants `vchord`, and index-method selection reads the narrower grant; swapping the engine is an image change, never a query change.
- Law: the queue class has no extension row — the SKIP-LOCKED primitive plus the relay rows in `journal/append.md` own the shape, and the visibility-timeout redelivery idiom is mined into the relay claim as an attempts/lease column pair; a second job-table paradigm inside the database is the refused split-brain.
- Law: native `uuidv7()` subsumes the identity-mint extension class entirely — no row exists, and any image fact naming one is stale.
- Law: flags price deployment facts — `timescaledb` carries `tsl` (source-available licensing), `excludesSharding` (mutually exclusive with a sharding engine in one database), and `preload`; the `preload` flag on it and `pg_cron` marks the `shared_preload_libraries` demand the deploy plane's CNPG derivation filters on, so a new preload-demanding extension is a flag edit with zero deploy-plane code change; every flag travels into the image projection so the deployment derivation prices the roster.
- Law: `pg_incremental` hard-requires the `cron` grant — its exactly-once checkpointed batch folds are the maintenance plane's incremental lane; the probe admits it only where `pg_cron` also probes true.
- Law: `pg_parquet` grants the object-store COPY egress — `COPY TO/FROM` Parquet against the object plane — interchange only, never a query engine; the OLAP lane owns querying what it writes.
- Law: the standard probe is structural — a row without `probeSql` rides the one batched `pg_extension` scan `lane/capability.md` owns; `probeSql` exists ONLY as the exotic per-row override, so probe dispatch reads field presence, never string shape.

```typescript
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
  pg_trgm: { extension: "pg_trgm", floor: "0.0.0", capabilities: ["trigram"], layer: "core", flags: [] },
  fuzzystrmatch: { extension: "fuzzystrmatch", floor: "0.0.0", capabilities: ["phonetic", "fuzzy"], layer: "core", flags: [] },
} as const

const _core = {
  pg: ["channel", "advisory", "copy", "uuidv7"],
  sqlite: [],
} as const

declare namespace Pg {
  type Kind = keyof typeof _rows
  type Row = (typeof _rows)[Kind]
  type Core = (typeof _core)["pg"][number]
  type Grant = Row["capabilities"][number] | Core
  type Flag = "tsl" | "excludesSharding" | "requiresCron"
  type Image = ReadonlyArray<{ readonly extension: string; readonly floor: string; readonly flags: ReadonlyArray<Flag> }>
  type _Rows<T extends Record<Kind, {
    readonly extension: string
    readonly floor: string
    readonly probeSql?: string
    readonly capabilities: ReadonlyArray<string>
    readonly layer: "image" | "core"
    readonly flags: ReadonlyArray<Flag>
  }> = typeof _rows> = T
}
```

## [5]-[DRIVER_ROWS]

- Owner: the three driver Layer mints over `PgClient` and the assembled `Pg` export — the fixed-config row, the `Config`-wrapped row, and the shared-pool adoption row `lane/tenant.md` fans tenant scopes across; the notify bus and jsonb fragment ride the concrete Tag for the two pages that need them.
- Packages: `@effect/sql-pg` (`PgClient.layer`, `PgClient.layerConfig`, `PgClient.layerFromPool`, `PgClient.listen`, `PgClient.notify`, `PgClient.json`); `effect` (`Config`, `Duration`, `Redacted`).
- Entry: `lane/tenant.md` composes `Pg.client` and `Pg.fromPool` inside its `Stores` lookup; `journal/append.md` reaches the `PgClient` Tag for `notify` and `json`; the projection drain composes `listen` through its optional-service read.
- Growth: a pool knob is a `Config` field on `_pool`; a second physical spine (a read replica) is one more mint call with its own database coordinate, keyed by the scope that owns it.
- Law: domain rows yield the neutral `SqlClient` — the concrete Tag is reached only for `listen`/`notify`/`json`; a row typed against `PgClient` while composing no pg-native member blocks every other lane and is the named defect.
- Law: credentials are `Redacted` and pool budgets are `Config` facts — `url`, `maxConnections`, `connectionTTL` never appear as literals in any row; `applicationName` pins the span-to-`pg_stat_activity` correlation.
- Law: the shared-pool row is the tenancy fan-out primitive — one app-owned pool acquired once, adopted by every row-scoped and schema-scoped tenant Layer through `layerFromPool`, so a diamond of N apps on one database costs one pool.
- Law: construction is resilient at the Layer value — `Layer.retry` under the jittered bounded schedule rides both mints, so a transient pool-acquire refusal at boot re-attempts as graph policy instead of failing the composition root; a persistent refusal still fails typed after the budget.

```typescript
import { Array, Config, type ConfigError, Duration, Layer, Option, Record, Schedule } from "effect"
import type { SqlClient, SqlError } from "@effect/sql"
import { PgClient } from "@effect/sql-pg"

const _BOOT = Schedule.exponential("250 millis").pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(5)))

const _client = (database: string): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  PgClient.layerConfig({
    url: Config.redacted("DATA_PG_URL"),
    database: Config.succeed(database),
    maxConnections: Config.integer("DATA_PG_POOL_MAX").pipe(Config.withDefault(16)),
    connectionTTL: Config.duration("DATA_PG_CONN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
    applicationName: Config.succeed("data"),
  }).pipe(Layer.retry(_BOOT))

const _fromPool = (
  acquire: PgClient.PgClientFromPoolOptions["acquire"],
): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, SqlError.SqlError> =>
  PgClient.layerFromPool({ acquire }).pipe(Layer.retry(_BOOT))

const Pg = {
  spine: _spine,
  primitives: _primitives,
  ..._rows,
  rows: Record.values(_rows),
  image: Array.filterMap(Record.values(_rows), (row) =>
    row.layer === "image"
      ? Option.some({ extension: row.extension, floor: row.floor, flags: row.flags })
      : Option.none()),
  core: _core,
  client: _client,
  fromPool: _fromPool,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Pg }
```
