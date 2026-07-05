# [@effect/sql-pg] — the PostgreSQL driver Layer: the pg journal spine, LISTEN/NOTIFY channelization, advisory-lock claims, COPY bulk lanes

`@effect/sql-pg` binds the abstract `@effect/sql` `SqlClient` Tag to `node-postgres` (`pg`) as a pooled, span-instrumented `PgClient` — the durable spine every `journal`, `project`, `capability`, `scope`, and `retrieve` row composes. It adds exactly three members over `SqlClient`: `listen(channel)` / `notify(channel, payload)` (the projection wake-up bus `project/async` rides) and `json` (jsonb fragment). Everything the folder README frames as pg power — advisory-lock claims, COPY bulk lanes, the `ON CONFLICT … RETURNING (xmax = 0)` idempotency claim, `SKIP LOCKED` async lanes, the RLS `app.current_tenant` GUC — is a parameterized `sql` statement over the inherited `reserve` (dedicated `Connection`) and `withTransaction` surfaces, never a bespoke driver method: the driver's job is the pool, the wire protocol, LISTEN/NOTIFY, and the span, and the journal law lives in SQL. `PgMigrator` ships in the package and is banned branch-wide — DDL is idempotent declarative ensure, split `iac` applies / `store` verifies / runtime never mutates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-pg`
- package: `@effect/sql-pg`
- version: `0.52.1`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/sql ^0.51.x` (the `SqlClient` core this extends; `.api/effect-sql.md`), `@effect/experimental ^0.60.x` (`Reactivity` — `make`/`reactive`/`reactiveMailbox` require it; `.api/effect-experimental.md`), `@effect/platform ^0.96.x` (`FileSystem`/`Path`/`CommandExecutor` — `PgMigrator` only; `.api/effect-platform.md`)
- backing: bundles `pg ^8.16` (`node-postgres`) with `pg-pool` (connection pool), `pg-cursor` (server-side streaming cursor behind `SqlStream`), `pg-types` (OID codec), `pg-connection-string`
- runtime: `runtime:node`/bun — imports `node:stream`, `node:tls`; the PG journal spine. The browser plane never imports it — `lane/wasm` on `@effect/sql-sqlite-wasm` is the browser durability lane
- modules: `PgClient`, `PgMigrator`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `PgClient` service and its pg-native additions
- rail: store/journal
- `PgClient extends SqlClient` — providing the `layer` yields both the `PgClient` Tag and the `SqlClient` Tag, so domain rows compose the neutral `SqlClient` and only `journal`/`project` rows needing LISTEN/NOTIFY or jsonb reach for the `PgClient` Tag. `listen`/`notify` are the ONE parameterized channel bus, not a fixed channel roster.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `PgClient.PgClient` (Tag) / `interface PgClient`                | service Tag     | `journal`/`project` PG rows; `PgClient \| SqlClient` provided by one layer  |
|  [02]   | `PgClient.listen(channel: string): Stream<string, SqlError>`   | notify bus      | `project/async` — LISTEN wake stream per channel; one arity, channel is data |
|  [03]   | `PgClient.notify(channel, payload: string): Effect<void, SqlError>` | notify bus  | `project/inline` post-commit NOTIFY; `journal/outbox` change signal          |
|  [04]   | `PgClient.json(_: unknown): Fragment`                          | jsonb fragment  | `journal`/`retrieve` jsonb column writes; `PgCustom` = `PgJson` custom type  |
|  [05]   | `PgClient.config: PgClientConfig`                              | resolved config | span/transform introspection; `applicationName` correlation                |

[PUBLIC_TYPE_SCOPE]: configuration and bring-your-own pool
- rail: store/journal
- `PgClientConfig` parameterizes the pool, TLS, timeouts, and name transforms; secrets are `Redacted`. `PgClientFromPoolOptions.acquire` hands an app-owned `pg.Pool` to the driver so `scope/handle` can share one pool across tenant Layers.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `PgClientConfig` (`url`/`host`/`port`/`database`/`ssl`)         | connection      | `host/config` provider; `url`/`password` are `Redacted.Redacted`           |
|  [02]   | `PgClientConfig` (`maxConnections`/`minConnections`/`connectionTTL`/`idleTimeout`/`connectTimeout`) | pool sizing | `scope/handle` per-app pool budget; `iac` deployment facts |
|  [03]   | `PgClientConfig.transformResultNames`/`transformQueryNames`     | name transform  | snake_case ⇄ camelCase at the wire; internal rows stay canonical camelCase |
|  [04]   | `PgClientConfig.transformJson` / `PgClientConfig.types` (`CustomTypesConfig`) | codec | jsonb transform toggle; `pg-types` OID parser overrides (e.g. `numeric`)   |
|  [05]   | `PgClientConfig.applicationName` / `PgClientConfig.spanAttributes` | telemetry    | `pg_stat_activity` correlation; per-query OTel span attributes             |
|  [06]   | `PgClientFromPoolOptions` (`acquire: Effect<pg.Pool, SqlError, Scope>`) | pool adopt | `scope/handle` — share one app-owned `pg.Pool` across tenant Layers        |

[PUBLIC_TYPE_SCOPE]: the inherited `SqlClient` core the pg rows compose
- rail: store/journal
- The pg-specific surface is thin because the durable law is SQL. These `@effect/sql` members (`.api/effect-sql.md`) carry the journal, projection, and lane contracts; `PgClient` supplies the pooled, LISTEN/NOTIFY-capable connection they run over.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `SqlClient.reserve: Effect<Connection, SqlError, Scope>`        | dedicated conn  | `capability` session `pg_advisory_lock`; COPY FROM STDIN streaming lane    |
|  [02]   | `SqlClient.withTransaction(effect)`                            | transaction     | `journal/append` + `outbox` + idempotency-ledger atomic commit; xact locks |
|  [03]   | `SqlClient.reactive(keys, effect): Stream` / `reactiveMailbox` | reactive query  | `project/inline` read-your-writes via `@effect/experimental` `Reactivity`   |
|  [04]   | `Statement` `sql` `Constructor` (`in`/`unsafe`/`literal`/`insert`/`update`/`updateValues`/`and`/`or`/`csv`/`join`) | statement | every `journal`/`project`/`retrieve` row; safe interpolation, no string glue |
|  [05]   | `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` (the `pg` arm here) / `sql.onDialectOrElse({ orElse, pg? })` | dialect switch | ONE journal/projection statement across the PG spine and the sqlite lanes; `pg` is an arm-KEY, not a `sql.pg` method |
|  [06]   | `SqlSchema.findAll`/`findOne`/`single`                        | result decode   | `project` typed row decode into `Schema` models; parse-fail is a typed rail |
|  [07]   | `SqlResolver.ordered`/`grouped`/`findById`                    | batch resolver  | `project`/`retrieve` N+1 elimination — request-batched, `DataLoader`-style   |
|  [08]   | `Model.makeRepository`/`makeDataLoaders` (`Generated`/`Sensitive`/`DateTimeInsert`) | table model | typed CRUD + insert/update variant schemas over a `journal`/`project` table |
|  [09]   | `SqlStream.asyncPauseResume` / `SqlError` / `ResultLengthMismatch` | stream/fault | `pg-cursor` backpressured result stream; the one tagged SQL error rail      |
|  [10]   | `SqlEventJournal` / `SqlEventLogServer` / `SqlPersistedQueue`  | sql backing     | SQL storage for the `@effect/experimental` EventLog server + persisted queue |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- rail: store/journal
- `layer` is the app-root row; `layerConfig` wraps every field in `Config` for env/secret-mount resolution; `layerFromPool` adopts an app-owned pool. All three provide `PgClient | SqlClient` in one Layer.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `PgClient.layer(config: PgClientConfig): Layer<PgClient \| SqlClient, SqlError>`                    | driver layer   | `scope/handle` per-app driver row (fixed config)         |
|  [02]   | `PgClient.layerConfig(config: Config.Wrap<PgClientConfig>): Layer<PgClient \| SqlClient, ConfigError \| SqlError>` | driver layer | `host/config` env/secret-mount resolution — the standing app-root row |
|  [03]   | `PgClient.layerFromPool(options: PgClientFromPoolOptions): Layer<PgClient \| SqlClient, SqlError>`  | driver layer   | `scope/handle` shared-pool tenancy fan-out               |
|  [04]   | `PgClient.make(config)` / `PgClient.fromPool(options): Effect<PgClient, SqlError, Scope \| Reactivity>` | scoped make | scoped construction inside a larger acquire graph        |
|  [05]   | `PgClient.makeCompiler(transform?, transformJson?): Statement.Compiler`                             | compiler       | custom identifier transform / raw-SQL testkit harness (`tests/typescript/_testkit`)      |

[ENTRYPOINT_SCOPE]: the pg journal patterns — SQL over `reserve`/`withTransaction`/`listen`
- rail: store/journal
- The advisory-lock, COPY, idempotency, and SKIP-LOCKED capabilities are `sql` statements, not driver methods. Each is one parameterized fragment run over the inherited surface, so a new lane is a new statement, never a new API.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `sql`\``INSERT … ON CONFLICT (idempotency_key) DO UPDATE … RETURNING (xmax = 0) AS inserted`\`      | idempotency    | `journal/outbox` — the `(xmax = 0)` first-writer claim   |
|  [02]   | `client.withTransaction(`\``SELECT pg_advisory_xact_lock($claim)`\`` *>> append)`                   | xact lock      | `journal/append` OCC serialization; lock frees at commit |
|  [03]   | `Effect.scoped(client.reserve.pipe(Effect.flatMap((conn) => `\``SELECT pg_advisory_lock($k)`\`\`))` | session lock   | `project/rebuild` singleton compaction; explicit unlock  |
|  [04]   | `sql`\``… FOR UPDATE SKIP LOCKED LIMIT $n`\`                                                         | async lane     | `project/async` — competing-consumer checkpoint drain    |
|  [05]   | `client.reserve` → `conn` → `pg` `COPY … FROM STDIN` write stream                                   | COPY bulk      | `journal`/`retrieve` bulk ingest over a dedicated conn   |
|  [06]   | `client.listen(channel)` ⇒ `Stream` woken by `project/inline`'s `client.notify(channel, id)`        | notify wake    | `project/async` LISTEN/NOTIFY-woken checkpoint lanes     |
|  [07]   | `sql`\``SELECT set_config('app.current_tenant', ${tenantId}, true)`\`` inside withTransaction` (bare `SET LOCAL` cannot bind parameters) | RLS GUC | `scope/tenant` — per-transaction tenant scope, not a fork |
|  [08]   | `client.reactive(keys, query): Stream` ← `Reactivity.invalidate(keys)`                              | reactive read  | `project/inline` read-your-writes signal after an append |

## [04]-[IMPLEMENTATION_LAW]

[PG_SPINE_TOPOLOGY]:
- `PgClient` IS a `SqlClient`: `layer*` provides `PgClient | SqlClient`, so domain rows depend on the neutral `SqlClient` Tag and stay dialect-agnostic; only rows using `listen`/`notify`/`json` yield the `PgClient` Tag. This is what lets `sql.onDialect({ pg, sqlite })` compile one journal/projection statement that the PG spine and the `lane/sqlite` rows both run.
- the durable law is SQL, not driver API: advisory locks (`pg_advisory_xact_lock` / `pg_advisory_lock`), COPY, `ON CONFLICT … RETURNING (xmax = 0)`, `FOR UPDATE SKIP LOCKED`, and the RLS `set_config('app.current_tenant', …, true)` GUC are parameterized `sql` fragments over `reserve` (dedicated `Connection`) and `withTransaction` (transaction scope). The driver owns the pool, the wire protocol, LISTEN/NOTIFY, and the OTel span — nothing more. A new capability is a new statement.
- one atomic commit: `journal/append` + `journal/outbox` + the idempotency ledger share one `withTransaction`, so the event, the outbox row, and the `(xmax = 0)` claim commit or roll back together — the exactly-once boundary.
- `reserve` is the session-state boundary: session advisory locks and COPY streams need a connection pinned across statements; `reserve` yields a scoped `Connection` from the pool that returns on scope close. A bare `client`\``…`\` runs pool-per-statement and cannot hold session locks.
- extension capability is fail-closed SQL: each `capability/row` `probeSql` runs through `sql` and gates the feature; extensions are `iac`/CNPG deployment-image facts (`pgvector`, `vchord`, `vchord_bm25`, `pg_cron`, `pg_ivm`, `pg_partman`), never a JS dependency — identity mint is native `uuidv7()`, no extension row.

[INTEGRATION_LAW]:
- Stack with `@effect/sql` core (`.api/effect-sql.md`): `PgClient` supplies the pooled connection; `SqlSchema.findAll`/`findOne` decode rows into `Schema` models, `SqlResolver.grouped`/`findById` batch reads to kill N+1, `Model.makeRepository` types the journal/projection tables with `Generated`/`Sensitive`/`DateTimeInsert` variant schemas, and `SqlStream.asyncPauseResume` streams a `pg-cursor` under backpressure. The pg catalog documents stacking ONTO this core, never re-implementing it.
- Stack with `@effect/experimental` (`.api/effect-experimental.md`): `make`/`reactive`/`reactiveMailbox` require the `Reactivity` service — `project/inline` emits `Reactivity.invalidate(keys)` after an OCC append and `reactive(keys, query)` re-runs the read (read-your-writes). `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` are the SQL backings the EventLog sync server and durable queue bind to; the store's own journal is raw `@effect/sql`, and EventLog stays an overlay, never the record of truth.
- Stack with `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): every statement auto-opens a span carrying `PgClientConfig.spanAttributes` plus the SQL text; the `NodeSdk`/`Otlp.layer` under the graph exports them with no per-query wiring. `applicationName` correlates the span to `pg_stat_activity`.
- Stack with the sqlite lanes (`.api/effect-sql-sqlite-bun.md`, `-node`, `-wasm`): `sql.onDialect`'s `pg`/`sqlite` arms write one statement across dialects; the `lane/sqlite` capability-degradation table names what the sqlite lanes drop against this spine — RLS → file-per-app, `pg_ivm` → in-process folds, LISTEN/NOTIFY → in-process `Reactivity`, `updateValues` → `never`, COPY → chunked inserts.
- Stack with `iac`/`security`: extensions and pool budgets are CNPG deployment-image facts `iac` provisions and `store` verifies at startup (the DDL split); `PgClientConfig.url`/`password` are `Redacted` from `host/config`, and `journal/retain` composes the `security/sign` `Shredder` for crypto-shredding — the one direct `store → security` edge.

[LOCAL_ADMISSION]:
- compose the neutral `SqlClient` Tag in domain rows; reach for the `PgClient` Tag only for `listen`/`notify`/`json`. A row hardcoded to `PgClient` that could ride `SqlClient` blocks the sqlite lanes.
- express advisory locks, COPY, idempotency claims, SKIP LOCKED, and the RLS GUC as `sql` fragments over `reserve`/`withTransaction`; never invent a driver wrapper — the driver has none, and the statement is the parameterized owner.
- `PgMigrator` (`run`/`layer`, re-exporting `@effect/sql/Migrator`) is banned branch-wide: no migrations by construction, read-time upcasting instead. DDL is idempotent declarative ensure — `iac` applies, `store` verifies, runtime never mutates schema.
- secrets (`url`, `password`) are `Redacted`; pool sizing (`maxConnections`, `connectionTTL`) is a `Config`/`iac` fact, never a literal in a row.

[RAIL_LAW]:
- Package: `@effect/sql-pg`
- Owns: the pooled `PgClient` binding of `SqlClient` to `pg`, `listen`/`notify` channelization, the `json`/`PgJson` jsonb fragment, `layer`/`layerConfig`/`layerFromPool` construction, `fromPool` pool adoption, `makeCompiler`, and the banned `PgMigrator`
- Accept: `PgClient` as a `SqlClient` provider, advisory-lock/COPY/idempotency/SKIP-LOCKED/RLS-GUC expressed as `sql` over `reserve`/`withTransaction`, `listen`/`notify` as the `project/async` wake bus, `reactive` composing `@effect/experimental` `Reactivity`, `spanAttributes` auto-spanned, secrets `Redacted`, extensions as `iac`/CNPG facts
- Reject: a `PgClient`-typed row that could be `SqlClient`, a hand-rolled driver wrapper for a capability SQL already owns, `PgMigrator` or any runtime schema mutation, hardcoded credentials/pool sizes, LISTEN/NOTIFY or COPY re-implemented outside the `sql`/`reserve` surface, EventLog treated as the record of truth
