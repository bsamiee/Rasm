# [TS_DATA]

`data` is the branch's durable-persistence plane — one body: the guarantee-lane matrix pricing what each engine promises, the append-only journal as the record of truth, the content-addressed object plane over the one `ContentKey`, and the typed read side. Fleet-scale apps share one pool and one code path with tenancy a scope value, and wire peers demand bit-identical content identity, so an artifact hashed in any runtime is reusable by every other.

Its bar is trust made structural: no engine boots unproven — every extension and relation demand is proven at `Layer` construction; truth is never rewritten — evolution is read-time upcasting behind the causal frontier; atomicity is one commit — outbox rows, projection slots, the idempotency claim, and their events settle together, a replay returning the stored receipt; erasure is cryptographically total — destroying the sole wrapped key folds every sealed read to a redaction marker.

A backend enters as a semantic-guarantee row on its owning lane, never a sibling shape. This folder stores only wrapped key material — custody stays with security — and enforces the security-declared tenancy contract at every write through the one pinned transaction path; the deploy plane applies schema at provision while data proves it fail-closed at startup and never mutates it.

## [01]-[ROUTER]

- [01]-[LANE](.planning/lane/): A backend is a semantic-guarantee row on its lane — fail-closed proof and the single `Tenant.within` write path.
- [02]-[JOURNAL](.planning/journal/): One atomic write owner folding journal, outbox, and idempotency into one commit; upcasting over migrations.
- [03]-[OBJECT](.planning/object/): Every object key IS the one `ContentKey` — one admission fold over the store, stream, file, and remote planes.
- [04]-[READ](.planning/read/): Every row leaves a relation as a decoded value — arity, staleness, and reactivity as combinators on one owner.

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
- `@effect/sql-mysql2` — read-oriented interop lane binding `SqlClient` to `mysql2`; its compiler reports the `mysql` dialect, lighting the otherwise-idle `sql.onDialect` `mysql` arm.
- `@effect/sql-mssql` — read-oriented interop lane binding `SqlClient` to SQL Server over `tedious`; its compiler reports the `mssql` dialect, lighting the idle `sql.onDialect` `mssql` arm, and adds the typed `param` fragment and strongly-typed stored-procedure `call`.

[ANALYTICAL]:
- `@effect/sql-clickhouse`
- `@duckdb/node-api`
- `@duckdb/duckdb-wasm`
- `@qualithm/arrow-flight-client` — Flight SQL wire for remote columnar engines over the `@connectrpc/connect` transport, decoding to Arrow tables.
- `apache-arrow` — carries the zero-copy columnar format shared with the interface plane.
- `parquet-wasm` — engine-free Parquet codec round-tripping `apache-arrow` Tables over IPC or the Arrow C Data Interface; the durable lake format at rest the Arrow wire lacks.

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
