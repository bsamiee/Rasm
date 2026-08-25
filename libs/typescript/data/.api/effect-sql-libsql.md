# [TS_DATA_API_EFFECT_SQL_LIBSQL]

`@effect/sql-libsql` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to the `@libsql/client` SDK — the edge-replica profile of the one sqlite lane, a local replica serving reads while writes forward to the remote primary. This driver owns the interactive-transaction machinery — write-mode `transaction` with `SAVEPOINT` nesting — the D1 profile refuses; tenancy is database-per-tenant.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer on the `./server` subpath
- rail: data/lane
- `layer` yields `Layer<LibsqlClient \| SqlClient>` infallibly; `layerConfig` adds only `ConfigError`; `make` returns `Effect<LibsqlClient, never, Scope \| Reactivity>` for construction inside a larger acquire graph.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :---------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `LibsqlClient.layer(config)`                                | driver layer   | fixed-config profile row                       |
|  [02]   | `LibsqlClient.layerConfig(Config.Wrap<LibsqlClientConfig>)` | driver layer   | env/secret-mount resolution — the standing row |
|  [03]   | `LibsqlClient.make(config)`                                 | scoped make    | construction inside a larger acquire graph     |

## [03]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): libsql rides the `sqlite` arm of `sql.onDialect` and supplies only the `SqlClient.MakeOptions` (sqlite `Compiler`, connection acquirer, interactive-transaction machinery) the neutral `make` folds; the fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, and the overlay-storage Layers compose unchanged.
- Stack across `data`: one `lane/sqlite` row — local-replica reads, primary-serialized writes; LISTEN/NOTIFY degrades to the sync-pull cadence, RLS to database-per-tenant.

[LOCAL_ADMISSION]:
- Provide the layer on the `./server` subpath at the app root only; neutral rows yield `SqlClient`.
- `url`/`authToken`/`encryptionKey` ride `Config.redacted`; sync cadence rides a `Config` duration.
- libsql is contract-compatible with sqlite, not byte-compatible with the C `sqlite3` engine; the lane degradation table records every divergence.
- `SqlError.cause` is `LibsqlError { code: string; rawCode?: number }`, and its `code` carries two vocabularies: the local path re-wraps the driver's own `SQLITE_*` code while the hrana path carries the server's response code verbatim, so a classifier reading one field answers whichever transport served the statement; on the local path `rawCode` is the extended numeric code and `message` prefixes the engine sentence with the code name, so containment matching still reaches the column roster.
- Local-replica DDL rides `withTransaction` atomically as on the C engine — a minted shadow and its `sqlite_sequence` row roll back together, `ALTER TABLE … RENAME` carries the counter row under the new name, and `CREATE TABLE … AS SELECT` drops every key, constraint, default, and `NOT NULL` — so `journal/evolve`'s own-DDL mint and its `INSERT INTO sqlite_sequence … SELECT` seed run unchanged on this profile.
