# [TS_DATA_API_EFFECT_SQL_LIBSQL]

`@effect/sql-libsql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `@libsql/client` SDK — the edge-replica profile of the one sqlite lane, a local replica serving reads while writes forward to the remote primary. This driver owns the interactive-transaction machinery — write-mode `transaction` with `SAVEPOINT` nesting — the D1 profile refuses; tenancy is database-per-tenant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-libsql`
- package: `@effect/sql-libsql` (MIT)
- effect-peer: `effect`, `@effect/sql` (the extended `SqlClient` core; `.api/effect-sql.md`), `@effect/experimental` (`Reactivity`), `@effect/platform`
- backing: `@libsql/client` (embedded-replica + remote-sync protocol)
- runtime: node/bun server and edge hosts; the browser lane is `@effect/sql-sqlite-wasm`
- modules: `LibsqlClient`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `LibsqlClient` service and its config
- rail: data/lane
- `LibsqlClient` extends `SqlClient`, adding `[TypeId]` and a resolved `config`; every lane row yields the neutral Tag and only construction reaches the concrete one. `LibsqlClientConfig` splits `Full` (driver-owned connection) from `Live` (an app-owned `Libsql.Client` adopted as a value) over a shared `Base` carrying the `spanAttributes`/`transformResultNames`/`transformQueryNames` transforms.

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                     |
| :-----: | :--------------------------------------------------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `LibsqlClient` (Tag) / `interface LibsqlClient`                        | service Tag         | `lane/sqlite` libsql profile row        |
|  [02]   | `LibsqlClientConfig.Full.url` (`string \| URL`)                        | connection          | `file:`/`libsql:` url; `Config`-sourced |
|  [03]   | `LibsqlClientConfig.Full.authToken` (`Redacted.Redacted`)              | credential          | remote-primary auth; never a literal    |
|  [04]   | `LibsqlClientConfig.Full.syncUrl` / `.syncInterval`                    | replica sync        | replica pull cadence; wake coordinate   |
|  [05]   | `LibsqlClientConfig.Full.encryptionKey` (`Redacted.Redacted`)          | at-rest crypt       | replica-file encryption                 |
|  [06]   | `LibsqlClientConfig.Full.intMode` (`"number" \| "bigint" \| "string"`) | codec               | large-int posture, journal seq columns  |
|  [07]   | `LibsqlClientConfig.Full.tls` / `.concurrency`                         | transport           | TLS mode; driver concurrency cap        |
|  [08]   | `LibsqlClientConfig.Live.liveClient` (`Libsql.Client`)                 | client adopt        | app-owned client shared across Layers   |
|  [09]   | `LibsqlClientConfig.Base`                                              | telemetry/transform | shared with every dialect driver        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer on the `./server` subpath
- rail: data/lane
- `layer` yields `Layer<LibsqlClient \| SqlClient>` infallibly; `layerConfig` adds only `ConfigError`; `make` returns `Effect<LibsqlClient, never, Scope \| Reactivity>` for construction inside a larger acquire graph.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :---------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `LibsqlClient.layer(config)`                                | driver layer   | fixed-config profile row                       |
|  [02]   | `LibsqlClient.layerConfig(Config.Wrap<LibsqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `LibsqlClient.make(config)`                                 | scoped make    | construction inside a larger acquire graph     |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): libsql rides the `sqlite` arm of `sql.onDialect` and supplies only the `SqlClient.MakeOptions` (sqlite `Compiler`, connection acquirer, interactive-transaction machinery) the neutral `make` folds; the fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, and the overlay-storage Layers compose unchanged.
- Stack across `data`: one `lane/sqlite` row — local-replica reads, primary-serialized writes; LISTEN/NOTIFY degrades to the sync-pull cadence, RLS to database-per-tenant.

[LOCAL_ADMISSION]:
- Provide the layer on the `./server` subpath at the app root only; neutral rows yield `SqlClient`.
- `url`/`authToken`/`encryptionKey` ride `Config.redacted`; sync cadence rides a `Config` duration.
- libsql is contract-compatible with sqlite, not byte-compatible with the C `sqlite3` engine; the lane degradation table records every divergence.

[RAIL_LAW]:
- Package: `@effect/sql-libsql`
- Owns: the libSQL binding of `SqlClient` — `layer`/`layerConfig`/`make`, the `Full`/`Live`/`Base` config family, the embedded-replica sync knobs, the interactive-transaction machinery
- Accept: the edge-replica profile row under the one sqlite lane, `Config`-sourced credentials, database-per-tenant isolation
- Reject: a driver import in a neutral row, a second relational contract for the edge, byte-level sqlite assumptions, a hardcoded sync cadence or credential
