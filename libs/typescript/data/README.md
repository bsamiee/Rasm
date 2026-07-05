# [TS_DATA]

`libs/typescript/data` is the durable-data wave of the branch: the append-only journal (write owner, evolution, fact rail, retention), the guarantee-lane matrix (postgres spine, five-profile sqlite, analytical OLAP, latency cache, fail-closed capability probes, tenancy enforcement), the content-addressed object plane (store, resumable stream, local filesystem, remote-origin filesystem), and the read side (typed CRUD, batching, projections, reactive reads, five-lane retrieval). The journal is the record of truth; every other lane is a semantic guarantee with engines as rows. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[POSTGRES](.planning/lane/postgres.md)
- [02]-[SQLITE](.planning/lane/sqlite.md)
- [03]-[OLAP](.planning/lane/olap.md)
- [04]-[CACHE](.planning/lane/cache.md)
- [05]-[CAPABILITY](.planning/lane/capability.md)
- [06]-[TENANT](.planning/lane/tenant.md)
- [07]-[APPEND](.planning/journal/append.md)
- [08]-[EVOLVE](.planning/journal/evolve.md)
- [09]-[FACT](.planning/journal/fact.md)
- [10]-[RETAIN](.planning/journal/retain.md)
- [11]-[STORE](.planning/object/store.md)
- [12]-[STREAM](.planning/object/stream.md)
- [13]-[FILE](.planning/object/file.md)
- [14]-[REMOTE](.planning/object/remote.md)
- [15]-[QUERY](.planning/read/query.md)
- [16]-[BATCH](.planning/read/batch.md)
- [17]-[FOLD](.planning/read/fold.md)
- [18]-[LIVE](.planning/read/live.md)
- [19]-[SEARCH](.planning/read/search.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[RELATIONAL]:
- `@effect/sql`
- `@effect/sql-pg`
- `@effect/sql-sqlite-node`
- `@effect/sql-sqlite-bun`
- `@effect/sql-sqlite-wasm`
- `@effect/sql-libsql`
- `@effect/sql-d1`

[ANALYTICAL]:
- `@effect/sql-clickhouse`
- `@duckdb/node-api`
- `@duckdb/duckdb-wasm`
- `apache-arrow`

[OBJECT_PLANE]:
- `@aws-sdk/client-s3`
- `@aws-sdk/lib-storage`
- `@aws-sdk/s3-request-presigner`
- `@tus/server`
- `@tus/s3-store`

[FILESYSTEM]:
- `sharp`
- `chokidar`

[REMOTE_TRANSFER]:
- `basic-ftp`
- `webdav`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`

[OVERLAY]:
- `@effect/experimental`

[REMOTE_ROOT]:
- `ssh2`
