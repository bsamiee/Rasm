# [TS_DATA]

`data` is the branch's durable-persistence plane: the guarantee-lane matrix, the append-only journal as the record of truth, the content-addressed object plane over the one `ContentKey`, and the typed read side. Consumers bind guarantee lanes; an engine name never reaches them.

## [01]-[ROUTER]

[LANE]:
- [01]-[CACHE](.planning/lane/cache.md): Correctness-neutral latency — a lost node costs one cold recompute, stampedes collapse to one.
- [02]-[CAPABILITY](.planning/lane/capability.md): Engine admission proving every extension row, relation, and demand before a guarantee lane boots.
- [03]-[OLAP](.planning/lane/olap.md): Analytical throughput without durability claims — leased sessions, residence fills, the lake.
- [04]-[POSTGRES](.planning/lane/postgres.md): Relational guarantee spine — first-party capability rows and explicit concurrency denials.
- [05]-[SQLITE](.planning/lane/sqlite.md): One embedded contract across node, bun, wasm-OPFS, libSQL, and D1 — degradation keyed both ways.
- [06]-[TENANT](.planning/lane/tenant.md): Tenant isolation cases keyed off the app key, enforced before any statement runs.

[JOURNAL]:
- [07]-[APPEND](.planning/journal/append.md): Journal, outbox, and idempotency settling atomically — a replay returns its stored receipt.
- [08]-[EVOLVE](.planning/journal/evolve.md): Schema evolution without migrations — author-stamped versions lift at read through total chains.
- [09]-[FACT](.planning/journal/fact.md): Audit evidence and usage metering as one polymorphic fact family on one buffered rail.
- [10]-[RETAIN](.planning/journal/retain.md): Lawful aging — the log never rewrites; windows expire ledgers, shredding folds reads to redaction.

[OBJECT]:
- [11]-[ASSET](.planning/object/asset.md): Delivered-asset admission — a category is a row with its own transforms and derive plane.
- [12]-[FILE](.planning/object/file.md): Filesystem and derivative planes on one spine — open, admit, emit, mint, store, refer.
- [13]-[REMOTE](.planning/object/remote.md): Every non-local byte tree behind one origin-addressed surface — SFTP, FTP, WebDAV, object peers.
- [14]-[STORE](.planning/object/store.md): Verified object custody, confined event `dataref` residence, grants, lifecycle, and GC.
- [15]-[STREAM](.planning/object/stream.md): Resumable content-addressed intake — bounded chunks, verified offsets, one identity to the key.

[READ]:
- [16]-[BATCH](.planning/read/batch.md): Request families declared once — structural dedup folding N identical lookups into one windowed resolver.
- [17]-[FOLD](.planning/read/fold.md): Durable projections — one plan bound to one keyed relation at three staleness budgets.
- [18]-[LIVE](.planning/read/live.md): Read-your-writes as a coordinate vocabulary written at the mutation and consumed at the query.
- [19]-[QUERY](.planning/read/query.md): Typed CRUD — every row leaves decoded, every request proves against a schema first.
- [20]-[SEARCH](.planning/read/search.md): FTS, trigram, phonetic, fuzzy, and semantic rows ranked and joined by one fusion fold.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[RELATIONAL]:
- `@effect/sql`
- `@effect/sql-pg`
- `@effect/sql-sqlite-node`
- `@effect/sql-sqlite-bun`
- `@effect/sql-sqlite-wasm`
- `@effect/sql-libsql`
- `@effect/sql-d1`
- `@effect/sql-mysql2` — Read-oriented interop lane; its compiler lights the `sql.onDialect` `mysql` arm.
- `@effect/sql-mssql` — `tedious`-backed read lane lighting the `mssql` arm; adds typed `param` and stored-procedure `call`.
- `@electric-sql/pglite` — Embedded PostgreSQL engine behind the lane-owned `SqlClient` adapter and generation gate.
- `@electric-sql/pglite-tools` — `pgDump` exports hydration artifacts consumed only by fresh PGLite instances.

[ANALYTICAL]:
- `@effect/sql-clickhouse`
- `@duckdb/node-api`
- `@duckdb/duckdb-wasm`
- `@qualithm/arrow-flight-client` — Flight SQL wire to remote columnar engines, decoding to Arrow tables.
- `apache-arrow` — Carries the zero-copy columnar format shared with the interface plane.
- `parquet-wasm` — Engine-free Parquet codec; the durable at-rest lake format the Arrow wire lacks.

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
- `sharp` — This folder's ONE libvips composer; every raster decode, transform, and re-encode folds through it.
- `chokidar`

[ASSET_PIPELINE]:
- `@gltf-transform/core` — glTF 2.0 as a property graph behind one `PlatformIO` read/write surface; indices re-derive at write.
- `@gltf-transform/extensions` — glTF extension vocabulary as typed properties, admitted through an explicit IO roster.
- `@gltf-transform/functions` — Transform rows folded through one `document.transform(...)`; every codec injected, never imported.
- `ktx-parse` — KTX2 container read as plain data; payload class, transfer, primaries, alpha, and layer shape classify without a transcoder.
- `meshoptimizer` — Wasm mesh kernel: vertex and index codec, reordering, simplification, clustering, tangents.
- `watlas` — xatlas wasm binding: UV-atlas chart generation and packing; the injected instance behind the `unwrap` transform row.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[CONTRACT_BINDINGS]:
- `@bufbuild/protobuf` — Generated message types and semantic equality for backend contract composition.
- `@rasm\/contracts` — Generated appearance, organization, and parity contract descriptors consumed at data boundaries.

[BRANCH_PEERS]:
- `@rasm/ts/core` — Content identity, generated-message codecs, event envelopes, observation conventions, and shared value rails.
- `@rasm/ts/security` — Lease and custody contracts consumed through data-owned boundary ports.

[TYPING_RAILS]:
- `effect`

[EVENT_FABRIC]:
- `cloudevents` — Outbox message-envelope projection at the claim seam, minted through the core owner.

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/experimental`
