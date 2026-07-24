# [TS_DATA_API_EFFECT_SQL_PG]

`@effect/sql-pg` binds the neutral `SqlClient` Tag to `node-postgres` as a pooled, span-instrumented `PgClient`, the durable spine every store row composes.

Driver owns the pool, wire protocol, LISTEN/NOTIFY, and the OTel span; every pg capability rides a parameterized `sql` statement over inherited `reserve`/`withTransaction`, never a bespoke driver method.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-pg`
- package: `@effect/sql-pg` (MIT)
- effect-peer: `effect`, `@effect/sql`, `@effect/experimental`, `@effect/platform`
- backing: bundles `pg` (`node-postgres`) with `pg-pool`, `pg-cursor` (server-side cursor behind `SqlStream`), `pg-types` (OID codec), `pg-connection-string`
- runtime: `runtime:node`/bun — imports `node:stream`, `node:tls`; the browser plane binds `lane/wasm` on `@effect/sql-sqlite-wasm` instead
- rail: store/journal
- modules: `PgClient`, `PgMigrator`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `PgClient` service and its pg-native additions
- Domain rows compose the neutral `SqlClient`; only rows needing LISTEN/NOTIFY or jsonb reach the `PgClient` Tag, whose `listen`/`notify` are one parameterized channel bus, never a fixed channel roster.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                              |
| :-----: | :------------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `PgClient.PgClient` (Tag) / `interface PgClient`                    | service Tag     | `journal`/`project` PG rows                      |
|  [02]   | `PgClient.listen(channel: string): Stream<string, SqlError>`        | notify bus      | `project/async` LISTEN wake stream per channel   |
|  [03]   | `PgClient.notify(channel, payload: string): Effect<void, SqlError>` | notify bus      | `project/inline` NOTIFY; `journal/outbox` signal |
|  [04]   | `PgClient.json(_: unknown): Fragment`                               | jsonb fragment  | `journal`/`retrieve` jsonb writes; `PgJson` type |
|  [05]   | `PgClient.config: PgClientConfig`                                   | resolved config | span/transform introspection; `applicationName`  |

[PUBLIC_TYPE_SCOPE]: configuration and bring-your-own pool
- `PgClientConfig` parameterizes pool, TLS, timeouts, and name transforms; secrets are `Redacted`. `PgClientFromPoolOptions.acquire` (`Effect<pg.Pool, SqlError, Scope>`) hands an app-owned `pg.Pool` to the driver so `scope/handle` shares one pool across tenant Layers.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `url`/`host`/`port`/`database`/`ssl`              | connection     | `host/config` provider; `url`/`password` are `Redacted.Redacted`    |
|  [02]   | `maxConnections`/`minConnections`/`connectionTTL` | pool sizing    | `scope/handle` per-app pool budget; `iac` deployment facts          |
|  [03]   | `idleTimeout`/`connectTimeout`                    | pool timeout   | idle reclaim + connect deadline; `iac` facts                        |
|  [04]   | `transformResultNames`/`transformQueryNames`      | name transform | snake_case ⇄ camelCase at the wire; rows stay camelCase             |
|  [05]   | `transformJson` / `types` (`CustomTypesConfig`)   | codec          | jsonb toggle; `pg-types` OID parser overrides (`numeric`)           |
|  [06]   | `applicationName` / `spanAttributes`              | telemetry      | `pg_stat_activity` correlation; per-query OTel span attributes      |
|  [07]   | `PgClientFromPoolOptions.acquire`                 | pool adopt     | `scope/handle` — share one app-owned `pg.Pool` across tenant Layers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- `layer` is the app-root row, `layerConfig` wraps every field in `Config` for env/secret resolution, `layerFromPool` adopts an app-owned pool — all three provide `PgClient | SqlClient`, `layerConfig` adding `ConfigError` to the `SqlError` channel. `make`/`fromPool` are the scoped forms; `makeCompiler` returns a `Statement.Compiler` driving the raw-SQL testkit at `tests/typescript/_testkit`.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                   |
| :-----: | :---------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `PgClient.layer(config: PgClientConfig)`                    | driver layer   | `scope/handle` per-app driver row (fixed config)      |
|  [02]   | `PgClient.layerConfig(config: Config.Wrap<PgClientConfig>)` | driver layer   | `host/config` env/secret resolution; standing row     |
|  [03]   | `PgClient.layerFromPool(options: PgClientFromPoolOptions)`  | driver layer   | `scope/handle` shared-pool tenancy fan-out            |
|  [04]   | `PgClient.make(config)` / `PgClient.fromPool(options)`      | scoped make    | scoped construction inside a larger acquire graph     |
|  [05]   | `PgClient.makeCompiler(transform?, transformJson?)`         | compiler       | custom identifier transform / raw-SQL testkit harness |

[ENTRYPOINT_SCOPE]: the pg journal patterns — `sql` over `reserve`/`withTransaction`/`listen`
- RLS GUC binds through `set_config(..., true)` inside `withTransaction` because a bare `SET LOCAL` cannot bind parameters.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                        |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `sql`\``INSERT … ON CONFLICT (idempotency_key) … RETURNING (xmax = 0)`\` | idempotency    | `journal/outbox` first-writer claim        |
|  [02]   | `client.withTransaction(`\``SELECT pg_advisory_xact_lock($claim)`\``)`   | xact lock      | `journal/append` OCC; lock frees at commit |
|  [03]   | `client.reserve` → `conn` → `sql`\``SELECT pg_advisory_lock($k)`\`       | session lock   | `project/rebuild` compaction; unlock       |
|  [04]   | `sql`\``… FOR UPDATE SKIP LOCKED LIMIT $n`\`                             | async lane     | `project/async` competing-consumer drain   |
|  [05]   | `client.reserve` → `conn` → `COPY … FROM STDIN` write stream             | COPY bulk      | `journal`/`retrieve` bulk ingest           |
|  [06]   | `client.listen(channel)` ⇒ `Stream` ← `client.notify(channel, id)`       | notify wake    | `project/async` LISTEN/NOTIFY-woken lanes  |
|  [07]   | `sql`\``SELECT set_config('rasm.tenant', ${tenantId}, true)`\`    | RLS GUC        | `scope/tenant` per-transaction scope       |
|  [08]   | `client.reactive(keys, query): Stream` ← `Reactivity.invalidate(keys)`   | reactive read  | `project/inline` read-your-writes          |

## [04]-[IMPLEMENTATION_LAW]

[PG_SPINE_TOPOLOGY]:
- `PgClient` IS a `SqlClient`: `layer*` binds `PgClient | SqlClient`, so domain rows depend on the neutral `SqlClient` Tag and stay dialect-agnostic; only `listen`/`notify`/`json` rows yield the `PgClient` Tag. This lets `sql.onDialect({ pg, sqlite })` compile one journal/projection statement the PG spine and `lane/sqlite` rows both run.
- durable law is SQL, never driver API: every pg capability is a parameterized `sql` fragment over `reserve` (dedicated `Connection`) and `withTransaction` (transaction scope). Driver owns the pool, wire protocol, LISTEN/NOTIFY, and the OTel span — nothing more; a new capability is a new statement.
- one atomic commit: `journal/append`, `journal/outbox`, and the idempotency ledger share one `withTransaction`, so the event, the outbox row, and the `(xmax = 0)` claim settle together — the exactly-once boundary.
- `reserve` is the session-state boundary: session advisory locks and COPY streams pin a connection across statements; `reserve` yields a scoped `Connection` returning on scope close, where a bare `client`\``…`\` runs pool-per-statement and holds no session lock.
- extension capability is fail-closed SQL: each `capability/row` `probeSql` gates its feature at `Layer` construction; extensions are `iac`/CNPG deployment-image facts, never a JS dependency, and identity mint is native `uuidv7()`.

[INTEGRATION_LAW]:
- Stack with `@effect/sql` core (`.api/effect-sql.md`): the inherited `SqlClient` surface — typed queries, batching resolvers, the `Model` variant schemas, the backpressured cursor — runs over `PgClient`'s pooled connection, stacked never re-implemented.
- Stack with `@effect/experimental` (`.api/effect-experimental.md`): `make`/`reactive`/`reactiveMailbox` require the `Reactivity` service — `project/inline` emits `Reactivity.invalidate(keys)` after an OCC append and `reactive(keys, query)` re-runs the read. `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` are SQL backings the EventLog sync server and durable queue bind to; the store's journal stays the record of truth, EventLog an overlay.
- Stack with `@effect/opentelemetry`: every statement auto-opens a span carrying `PgClientConfig.spanAttributes` with the SQL text, and the `NodeSdk`/`Otlp.layer` under the graph exports them with no per-query wiring; `applicationName` correlates the span to `pg_stat_activity`.
- Stack with the sqlite lanes (`.api/effect-sql-sqlite-node.md`, `-bun`, `-wasm`): `sql.onDialect`'s `pg`/`sqlite` arms write one statement across dialects; `lane/sqlite` owns the capability-degradation table naming what the sqlite lanes drop against this spine.
- Stack with `iac`/`security`: extensions and pool budgets are CNPG deployment-image facts `iac` provisions and `store` verifies at startup; `PgClientConfig.url`/`password` are `Redacted` from `host/config`, and `journal/retain` composes the `security/sign` `Shredder` for crypto-shredding.

[LOCAL_ADMISSION]:
- compose the neutral `SqlClient` Tag in domain rows; reach for the `PgClient` Tag only for `listen`/`notify`/`json`. A `PgClient`-hardcoded row that rides `SqlClient` blocks the sqlite lanes.
- express advisory locks, COPY, idempotency claims, SKIP LOCKED, and the RLS GUC as `sql` fragments over `reserve`/`withTransaction`; the statement is the parameterized owner and the driver ships no wrapper.
- `PgMigrator` (`run`/`layer`, re-exporting `@effect/sql/Migrator`) is banned branch-wide: DDL is idempotent declarative ensure — `iac` applies, `store` verifies, runtime never mutates schema — with read-time upcasting instead of migrations.
- secrets (`url`, `password`) are `Redacted`; pool sizing (`maxConnections`, `connectionTTL`) is a `Config`/`iac` fact, never a row literal.

[RAIL_LAW]:
- Package: `@effect/sql-pg`
- Owns: the pooled `PgClient` binding of `SqlClient` to `pg`, `listen`/`notify` channelization, the `json`/`PgJson` jsonb fragment, `layer`/`layerConfig`/`layerFromPool`/`fromPool` construction, `makeCompiler`, and the branch-banned `PgMigrator`
- Accept: `PgClient` as a `SqlClient` provider; advisory-lock/COPY/idempotency/SKIP-LOCKED/RLS-GUC as `sql` over `reserve`/`withTransaction`; `listen`/`notify` as the `project/async` wake bus; `reactive` composing `@effect/experimental` `Reactivity`; secrets `Redacted`; extensions as `iac`/CNPG facts
- Reject: a `PgClient`-typed row that is `SqlClient`, a hand-rolled driver wrapper for a capability `sql` already owns, `PgMigrator` or any runtime schema mutation, hardcoded credentials or pool sizes, LISTEN/NOTIFY or COPY re-implemented outside `sql`/`reserve`, EventLog treated as the record of truth
