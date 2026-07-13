# [TS_DATA]

`data` owns the branch's durable-persistence surface: the append-only journal as record of truth, the guarantee-lane matrix pricing what each engine promises, the content-addressed object plane over one `ContentKey`, and the typed read side. A backend enters as a semantic-guarantee row on its owning lane, never a sibling shape; the folder holds no keys and enforces the security-declared tenancy contract at every write.

## [01]-[ROUTER]

[LANE]:
- [01]-[POSTGRES](.planning/lane/postgres.md): First-party relational spine whose extension matrix is ruled data.
- [02]-[SQLITE](.planning/lane/sqlite.md): Embedded lane degrading one relational contract across five profiles.
- [03]-[OLAP](.planning/lane/olap.md): Analytical lane over DuckDB and ClickHouse engine rows.
- [04]-[CACHE](.planning/lane/cache.md): Latency lane — single-flight dedup over restart-surviving cache rows.
- [05]-[CAPABILITY](.planning/lane/capability.md): Fail-closed capability rail probed at `Layer` construction.
- [06]-[TENANT](.planning/lane/tenant.md): Tenancy write path pinning the tenancy GUC across RLS, schema, and database cases.

[JOURNAL]:
- [07]-[APPEND](.planning/journal/append.md): One atomic write owner — journal, outbox, and idempotency ledger in a single commit.
- [08]-[EVOLVE](.planning/journal/evolve.md): Read-time upcasting — per-tag version chains keeping the log append-only.
- [09]-[FACT](.planning/journal/fact.md): Durable fact journal folding audit and metering into one buffered family.
- [10]-[RETAIN](.planning/journal/retain.md): Retention classes, crypto-shredding, and DSAR portability folds.

[OBJECT]:
- [11]-[STORE](.planning/object/store.md): S3-conditional content-addressed object store over the one `ContentKey`.
- [12]-[STREAM](.planning/object/stream.md): Resumable rail — BYOB ingress, checkpointed identity fold, tus server.
- [13]-[FILE](.planning/object/file.md): Filesystem plane — gated content-addressed intake and derivative codec.
- [14]-[REMOTE](.planning/object/remote.md): Remote-origin plane — scheme-dispatched non-local sources through the identity fold.

[READ]:
- [15]-[QUERY](.planning/read/query.md): Typed CRUD with arity as combinator over `Model` codec pairs.
- [16]-[BATCH](.planning/read/batch.md): Request-batching engine — structural dedup and windowed resolvers.
- [17]-[FOLD](.planning/read/fold.md): Durable projection plane binding one `Fold.Plan` at three staleness budgets.
- [18]-[LIVE](.planning/read/live.md): Reactivity-keyed reads — invalidation keys stamped at publish, read at query.
- [19]-[SEARCH](.planning/read/search.md): Five-lane retrieval fused by reciprocal rank inside the database.

## [02]-[DOMAIN_PACKAGES]

Data-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against the adjacent `.api/` folder.

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
- `apache-arrow` (`../ui/.api/apache-arrow.md`)

[OBJECT_TRANSPORT]:
- `@aws-sdk/client-s3`
- `@aws-sdk/lib-storage`
- `@aws-sdk/s3-request-presigner`
- `@tus/server`
- `@tus/s3-store`
- `basic-ftp`
- `webdav`
- `ssh2`

[FILE_MEDIA]:
- `sharp`
- `chokidar`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; the canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/experimental`
