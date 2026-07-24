# [TS_DATA]

`data` is the branch's durable-persistence plane — one body: the guarantee-lane matrix pricing what each engine promises, the append-only journal as the record of truth, the content-addressed object plane over the one `ContentKey`, and the typed read side. Fleet-scale apps share one pool and one code path with tenancy a scope value, and wire peers demand bit-identical content identity, so an artifact hashed in any runtime is reusable by every other.

Its bar is trust made structural: no engine boots unproven — every extension and relation demand is proven at `Layer` construction; truth is never rewritten — evolution is read-time upcasting behind the causal frontier; atomicity is one commit — outbox rows, projection slots, the idempotency claim, and their events settle together, a replay returning the stored receipt; erasure is cryptographically total — destroying the sole wrapped key folds every sealed read to a redaction marker.

Each owning lane admits a backend as a semantic-guarantee row, never a sibling shape. This folder stores only wrapped key material — custody stays with security — and enforces the security-declared tenancy contract at every write through the one pinned transaction path; the deploy plane applies schema at provision while data proves it fail-closed at startup and never mutates it.

## [01]-[ROUTER]

- [01]-[LANE](.planning/lane/): Guarantee-lane matrix — a backend is a semantic row, proving fail-closed at boot, writing through `Tenant.within`.
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
- `sharp`
- `chokidar`

[INTERCHANGE]:
- `cloudevents` — `journal/append.md` mints strict-validated `CloudEvent` values at the claim seam; core owns the catalog, runtime the carriage.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/experimental`
