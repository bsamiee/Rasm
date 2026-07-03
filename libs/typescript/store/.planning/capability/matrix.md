# [STORE_MATRIX]

The PG 18.4 extension matrix as one sealed vocabulary table: every extension the store can exploit is a `Capability.Row` here — pgvector with VectorChord as the stronger drop-in row, pg_search, timescaledb, pg_partman, pgmq, pg_cron, pg_ivm, pg_duckdb, pg_graphql, pg_jsonschema, pgaudit, postgis, pg_uuidv7, h3, and the contrib pair pg_trgm/fuzzystrmatch — beside one core-family table granting the in-core lanes (LISTEN/NOTIFY channelization, advisory-lock claims, COPY bulk) per dialect. Rows are deployment-image facts the `iac/kube` CNPG image realizes and `capability/row.md` probes fail-closed; a new extension is one row here plus one image fact there, never a JS dependency and never a code path.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                             |
| :-----: | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `EXTENSION_ROWS` | the sealed extension table, the derived grant union, the image projection for `iac`  |
|  [02]   | `CORE_FAMILIES`  | the per-dialect in-core grant table — channel, advisory, copy                        |

## [2]-[EXTENSION_ROWS]

- Owner: the interior `_rows` anchor plus the exported `Matrix` assembly — kinds, grants, the row list `Capability.Default` consumes, and the `{extension, floor}` image projection `iac/kube/data.md` reads.
- Packages: `effect` (`Types` for the flattened alias); the extensions themselves are image facts, not packages.
- Entry: `Matrix.rows` feeds `Capability.Default(Matrix.rows, ensures)` at `scope/handle.md`; `Matrix.image` is the one value the CNPG image derivation consumes.
- Growth: a new extension is one `_rows` entry — the derived `Kind`/`Grant` unions, the probe roster, and the image projection all move with it; zero consumer edits.
- Law: floors are `[R11]` candidates — probes decide at runtime, a floor bump is a row edit; `vchord`, `postgis`, and `h3` carry the presence-only `"0.0.0"` floor until their pins land.
- Law: VectorChord is the stronger drop-in — both `pgvector` and `vchord` grant `"vector"`, `vchord` alone grants `"vchord"`, and `retrieve/index.md` selects its index method by the narrower grant; dropping in VectorChord is an image change, not a query change.
- Law: `pg_uuidv7` rides `[R11]` against the native PG 18 `uuidv7()` — if the native function covers the surface the row dissolves into `[3]`'s core table; until then it probes like every image row.
- Law: `probeSql` is uniform by construction — `_probe(name)` mints the `pg_extension` lookup per row, so an exotic probe is a per-row override, never a second probe path.
- Boundary: what a grant unlocks lives with its consumer — `"vector"`/`"search"`/`"trigram"`/`"phonetic"` in `retrieve/`, `"cron"`/`"ivm"` in `project/rebuild.md`, `"partition"` in `journal/retain.md`, `"queue"` as the pgmq queue-as-data row `work` reaches through its `SqlClient` port; `"analytics"`, `"graphql"`, `"jsonschema"`, `"audit"`, `"geo"`, `"h3"`, `"timeseries"`, and `"uuidv7"` are granted surface for app-level statements under the same gate.

```typescript
import type { Types } from "effect"

const _probe = (name: string): string =>
  `SELECT extversion FROM pg_extension WHERE extname = '${name}'`

const _rows = {
  pgvector: { extension: "vector", floor: "0.8.4", probeSql: _probe("vector"), capabilities: ["vector"], layer: "image" },
  vchord: { extension: "vchord", floor: "0.0.0", probeSql: _probe("vchord"), capabilities: ["vector", "vchord"], layer: "image" },
  pg_search: { extension: "pg_search", floor: "0.24.1", probeSql: _probe("pg_search"), capabilities: ["search"], layer: "image" },
  timescaledb: { extension: "timescaledb", floor: "2.28.2", probeSql: _probe("timescaledb"), capabilities: ["timeseries"], layer: "image" },
  pg_partman: { extension: "pg_partman", floor: "5.4.3", probeSql: _probe("pg_partman"), capabilities: ["partition"], layer: "image" },
  pgmq: { extension: "pgmq", floor: "1.11.1", probeSql: _probe("pgmq"), capabilities: ["queue"], layer: "image" },
  pg_cron: { extension: "pg_cron", floor: "1.6.7", probeSql: _probe("pg_cron"), capabilities: ["cron"], layer: "image" },
  pg_ivm: { extension: "pg_ivm", floor: "1.15", probeSql: _probe("pg_ivm"), capabilities: ["ivm"], layer: "image" },
  pg_duckdb: { extension: "pg_duckdb", floor: "1.1.1", probeSql: _probe("pg_duckdb"), capabilities: ["analytics"], layer: "image" },
  pg_graphql: { extension: "pg_graphql", floor: "1.6.1", probeSql: _probe("pg_graphql"), capabilities: ["graphql"], layer: "image" },
  pg_jsonschema: { extension: "pg_jsonschema", floor: "0.3.4", probeSql: _probe("pg_jsonschema"), capabilities: ["jsonschema"], layer: "image" },
  pgaudit: { extension: "pgaudit", floor: "18.0", probeSql: _probe("pgaudit"), capabilities: ["audit"], layer: "image" },
  postgis: { extension: "postgis", floor: "0.0.0", probeSql: _probe("postgis"), capabilities: ["geo"], layer: "image" },
  pg_uuidv7: { extension: "pg_uuidv7", floor: "1.7.0", probeSql: _probe("pg_uuidv7"), capabilities: ["uuidv7"], layer: "image" },
  h3: { extension: "h3", floor: "0.0.0", probeSql: _probe("h3"), capabilities: ["h3"], layer: "image" },
  pg_trgm: { extension: "pg_trgm", floor: "0.0.0", probeSql: _probe("pg_trgm"), capabilities: ["trigram"], layer: "core" },
  fuzzystrmatch: { extension: "fuzzystrmatch", floor: "0.0.0", probeSql: _probe("fuzzystrmatch"), capabilities: ["phonetic", "fuzzy"], layer: "core" },
} as const

declare namespace Matrix {
  type Kind = keyof typeof _rows
  type Row = (typeof _rows)[Kind]
  type Grant = Row["capabilities"][number] | Core
  type Image = ReadonlyArray<{ readonly extension: string; readonly floor: string }>
  type Shape = Types.Simplify<typeof _rows & {
    readonly rows: ReadonlyArray<Row>
    readonly image: Image
    readonly core: typeof _core
  }>
  type _Rows<T extends Record<Kind, {
    readonly extension: string
    readonly floor: string
    readonly probeSql: string
    readonly capabilities: ReadonlyArray<string>
    readonly layer: "image" | "core"
  }> = typeof _rows> = T
}
```

## [3]-[CORE_FAMILIES]

- Owner: the `_core` per-dialect grant table plus the assembled `Matrix` export — in-core families need no probe, they are engine facts granted by lane.
- Growth: a new engine family is one `_core` key with its per-dialect verdict; the sqlite fallbacks for refused keys are `lane/sqlite.md`'s degradation table, keyed by the same grant vocabulary.
- Law: `channel` (LISTEN/NOTIFY), `advisory` (advisory-lock claims), and `copy` (COPY bulk lanes) are pg engine grants — `project/async.md`, `journal/append.md`, and the bulk lanes gate on them exactly like extension grants, so a lane swap changes the granted set, never the consuming statement.
- Law: the core grants merge into the probe report at `Capability.Default` composition — the composing scope passes its dialect's `Matrix.core` row as the factory's `core` argument and the service seeds the granted set with it, so `require("channel")` reads one vocabulary.

```typescript
const _core = {
  pg: ["channel", "advisory", "copy"],
  sqlite: [],
} as const

declare namespace Matrix {
  type Core = (typeof _core)["pg"][number]
}

const Matrix: Matrix.Shape = {
  ..._rows,
  rows: Object.values(_rows),
  image: Object.values(_rows)
    .filter((row) => row.layer === "image")
    .map((row) => ({ extension: row.extension, floor: row.floor })),
  core: _core,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Matrix }
```
