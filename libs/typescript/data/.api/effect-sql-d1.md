# [@effect/sql-d1] — the Cloudflare D1 dialect driver: the `D1Client` Layer behind the `lane/sqlite` D1 edge profile

`@effect/sql-d1` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to a Workers `D1Database` binding — the managed-edge profile of the ONE sqlite lane. The binding arrives as a value from the Workers environment (`env.DB`), so the config takes the live handle, never a connection string. D1 admits no interactive transaction: statements run batch/exec-shaped, `updateValues` is `never`, and the lane degradation table records both refusals. Read replication (sequential consistency per session) and PITR are platform facts outside the driver.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-d1`
- package: `@effect/sql-d1`
- version: `0.49.0`
- license: `MIT`
- effect-peer: `effect`, `@effect/sql` (`.api/effect-sql.md`)
- backing: the Workers `D1Database` runtime binding (no bundled driver)
- runtime: Cloudflare Workers only; the node/bun lanes are `-sqlite-node`/`-sqlite-bun`, the browser lane `-sqlite-wasm`
- modules: `D1Client`

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `D1Client` (Tag) / `interface D1Client`                         | service Tag     | `lane/sqlite` D1 profile row; `D1Client \| SqlClient`         |
|  [02]   | `D1Client.updateValues: never`                                  | refused member  | the lane degradation row — per-row updates only               |
|  [03]   | `D1ClientConfig.db` (`D1Database`)                              | binding adopt   | the Workers `env.DB` handle passed as a value                 |
|  [04]   | `D1ClientConfig.prepareCacheSize` / `.prepareCacheTTL`          | statement cache | hot-path lever; `Config`-sourced                              |
|  [05]   | `D1ClientConfig` (`spanAttributes`/`transformResultNames`/`transformQueryNames`) | telemetry/transform | shared driver base |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                          |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `D1Client.layer(config): Layer<D1Client \| SqlClient, ConfigError \| SqlError>`             | driver layer   | the Workers composition root passing `env.DB`   |
|  [02]   | `D1Client.layerConfig(Config.Wrap<D1ClientConfig>)`                                         | driver layer   | cache knobs from `Config`; the handle stays a value |
|  [03]   | `D1Client.make(config): Effect<D1Client, SqlError, Scope>`                                  | scoped make    | construction inside a larger acquire graph      |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): the query DSL, `SqlSchema`/`SqlResolver`/`Model` typed IO, and dialect arms are inherited; D1 rides the `sqlite` arm of `sql.onDialect`.
- Stack across `data`: the profile is one `lane/sqlite` row — per-database single-writer, database-per-tenant isolation, no LISTEN/NOTIFY, no interactive `withTransaction` (the lane's atomic-publish path is refused here; a D1 host composes the journal through batch statements or routes writes to the pg spine).

[LOCAL_ADMISSION]:
- Provide the layer at the Workers composition root only; neutral rows yield `SqlClient`.
- The 10 GB per-database cap and Sessions/Time-Travel semantics are platform facts consumed as lane degradation rows, never re-modeled.

[RAIL_LAW]:
- Package: `@effect/sql-d1`
- Owns: the D1 binding of `SqlClient` — `layer`/`layerConfig`/`make`, the `D1ClientConfig` handle-adopt config, the `updateValues: never` refusal
- Accept: the managed-edge profile row under the one sqlite lane contract, the binding handle as a value, batch-shaped writes
- Reject: a driver import in a neutral row, interactive-transaction assumptions, a second relational contract for the edge, re-modeled platform replication semantics
