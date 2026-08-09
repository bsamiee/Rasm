# [TS_DATA]

`data` is the branch's durable-persistence plane — the guarantee-lane matrix, the append-only journal as the record of truth, the content-addressed object plane over the one `ContentKey`, and the typed read side. Consumers bind guarantee lanes; an engine name never reaches them.

## [01]-[ROUTER]

- [01]-[LANE](.planning/lane/): Owns fail-closed relational guarantees from PostgreSQL capability through immutable generation admission.
- [02]-[JOURNAL](.planning/journal/): Record of truth: journal, outbox, and idempotency settle in one commit; evolution upcasts at read.
- [03]-[OBJECT](.planning/object/): Content-addressed object plane: every key IS the one `ContentKey`, admitted through one fold on every byte plane.
- [04]-[READ](.planning/read/): Typed read side — every row leaves a relation decoded; arity, staleness, and reactivity are combinators on one owner.

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
- `@effect/sql-mysql2` — read-oriented interop lane; its compiler lights the `sql.onDialect` `mysql` arm.
- `@effect/sql-mssql` — `tedious`-backed read lane lighting the `mssql` arm; adds typed `param` and stored-procedure `call`.
- `@electric-sql/pglite` — embedded PostgreSQL engine behind the lane-owned `SqlClient` adapter and generation gate.
- `@electric-sql/pglite-tools` — `pgDump` exports hydration artifacts consumed only by fresh PGLite instances.

[ANALYTICAL]:
- `@effect/sql-clickhouse`
- `@duckdb/node-api`
- `@duckdb/duckdb-wasm`
- `@qualithm/arrow-flight-client` — Flight SQL wire to remote columnar engines, decoding to Arrow tables.
- `apache-arrow` — carries the zero-copy columnar format shared with the interface plane.
- `parquet-wasm` — engine-free Parquet codec; the durable at-rest lake format the Arrow wire lacks.

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
- `sharp` — this folder's ONE libvips composer; every raster decode, transform, and re-encode folds through it.
- `chokidar`

[ASSET_PIPELINE]:
- `@gltf-transform/core` — glTF 2.0 as a property graph behind one `PlatformIO` read/write surface; indices re-derive at write.
- `@gltf-transform/extensions` — glTF extension vocabulary as typed properties, admitted through an explicit IO roster.
- `@gltf-transform/functions` — transform rows folded through one `document.transform(...)`; every codec injected, never imported.
- `ktx-parse` — KTX2 container read as plain data; payload class, transfer, primaries, alpha, and layer shape classify without a transcoder.
- `meshoptimizer` — wasm mesh kernel: vertex and index codec, reordering, simplification, clustering, tangents.
- `watlas` — xatlas wasm binding: UV-atlas chart generation and packing; the injected instance behind the `unwrap` transform row.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[WIRE_ENVELOPE]:
- `cloudevents` — `journal/append.md` mints strict-validated `CloudEvent` values at the claim seam, the branch's one member-level consumer.

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/experimental`
