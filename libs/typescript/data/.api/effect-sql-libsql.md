# [@effect/sql-libsql] — the libSQL/Turso edge-replica dialect driver: the `LibsqlClient` Layer behind the `lane/sqlite` edge profile

`@effect/sql-libsql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the libSQL client — the edge-replica profile of the ONE sqlite lane: a local replica file serves microsecond reads while writes forward to the remote primary, and the sync cadence is a config fact. The driver adds no query surface of its own; every journal, projection, and lane statement is the spine's, routed through `sql.onDialect`'s `sqlite` arm. Tenancy on this profile is database-per-tenant (cheap-database model) — the lane degradation table records the verdict.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-libsql`
- package: `@effect/sql-libsql`
- version: `0.41.0`
- license: `MIT`
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core this extends; `.api/effect-sql.md`)
- backing: `@libsql/client` (embedded-replica + remote sync protocol)
- runtime: `runtime:node`/bun server and edge hosts; never the browser plane (`@effect/sql-sqlite-wasm` owns it)
- modules: `LibsqlClient`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `LibsqlClient` service and its config
- rail: data/lane
- `LibsqlClient extends SqlClient` — providing the layer yields both Tags; lane rows compose the neutral `SqlClient` and only construction reaches the concrete Tag. Config splits `Full` (driver-owned connection) from `Live` (adopt an app-owned `Libsql.Client`).

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `LibsqlClient` (Tag) / `interface LibsqlClient`                 | service Tag     | `lane/sqlite` libsql profile row; `LibsqlClient \| SqlClient` |
|  [02]   | `LibsqlClientConfig.Full.url` (`string \| URL`)                 | connection      | `file:` local replica or `libsql:` remote; `Config`-sourced   |
|  [03]   | `LibsqlClientConfig.Full.authToken` (`Redacted.Redacted`)       | credential      | remote-primary auth; never a literal                          |
|  [04]   | `LibsqlClientConfig.Full.syncUrl` / `.syncInterval`             | replica sync    | embedded-replica pull cadence — the wake-degradation coordinate |
|  [05]   | `LibsqlClientConfig.Full.encryptionKey` (`Redacted.Redacted`)   | at-rest crypt   | replica-file encryption                                       |
|  [06]   | `LibsqlClientConfig.Full.intMode` (`"number" \| "bigint" \| "string"`) / `.tls` / `.concurrency` | codec/transport | large-integer posture for journal sequence columns |
|  [07]   | `LibsqlClientConfig.Live.liveClient` (`Libsql.Client`)          | client adopt    | app-owned client shared across Layers                         |
|  [08]   | `LibsqlClientConfig.Base` (`spanAttributes`/`transformResultNames`/`transformQueryNames`) | telemetry/transform | shared with every dialect driver |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                          |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `LibsqlClient.layer(config): Layer<LibsqlClient \| SqlClient, ConfigError \| SqlError>`     | driver layer   | fixed-config profile row                        |
|  [02]   | `LibsqlClient.layerConfig(Config.Wrap<LibsqlClientConfig>)`                                 | driver layer   | env/secret-mount resolution — the standing row  |
|  [03]   | `LibsqlClient.make(config): Effect<LibsqlClient, SqlError, Scope>`                          | scoped make    | construction inside a larger acquire graph      |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): all query/transaction/typed-IO surface is inherited; the driver owns only the connection, replica sync, and span.
- Stack across `data`: the profile is one `lane/sqlite` row — reads hit the local replica, writes serialize at the primary; LISTEN/NOTIFY degrades to the sync-pull cadence, RLS to database-per-tenant.

[LOCAL_ADMISSION]:
- Provide the layer on the `./server` subpath at the app root only; neutral rows yield `SqlClient`.
- `url`/`authToken`/`encryptionKey` ride `Config.redacted`; sync cadence is a `Config` duration fact, never a literal.
- Compatibility is contract-level, not byte-level — the engine is not the C sqlite3 library; the lane degradation table is the sole divergence record.

[RAIL_LAW]:
- Package: `@effect/sql-libsql`
- Owns: the libSQL binding of `SqlClient` — `layer`/`layerConfig`/`make`, the `Full`/`Live` config family, embedded-replica sync knobs
- Accept: the edge-replica profile row under the one sqlite lane contract, `Config`-sourced credentials, database-per-tenant isolation
- Reject: a driver import in a neutral row, a second relational contract for the edge, byte-level sqlite assumptions, hardcoded sync cadence or credentials
