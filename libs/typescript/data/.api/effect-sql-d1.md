# [TS_DATA_API_EFFECT_SQL_D1]

`@effect/sql-d1` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to a Workers `D1Database` binding — the managed-edge profile of the ONE sqlite lane. Binding arrives as a value from the Workers environment (`env.DB`), so the config adopts the live handle, never a connection string. D1 admits no interactive transaction (`transactionAcquirer` dies): statements run batch/exec-shaped over a prepared-statement cache, `updateValues` is `never`, and the lane degradation table records both refusals. Read replication (sequential consistency per session) and PITR are platform facts outside the driver.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-d1`
- package: `@effect/sql-d1`
- license: `MIT`
- effect-peer: `effect`, `@effect/sql` (`.api/effect-sql.md`), `@effect/experimental` (`Reactivity`), `@effect/platform`
- backing: the Workers `D1Database` runtime binding (no bundled driver); `@cloudflare/workers-types` supplies the binding types
- runtime: Cloudflare Workers only; the node/bun lanes are `-sqlite-node`/`-sqlite-bun`, the browser lane `-sqlite-wasm`
- modules: `D1Client`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `D1Client` service and its config
- rail: data/lane
- `D1Client extends SqlClient`, so providing the layer yields both Tags; neutral rows compose `SqlClient`. `D1ClientConfig` carries the shared `spanAttributes`/`transformResultNames`/`transformQueryNames` transforms over the Workers binding.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                             |
| :-----: | :----------------------------------------------------- | :------------------ | :---------------------------------------------- |
|  [01]   | `D1Client` (Tag) / `interface D1Client`                | service Tag         | `lane/sqlite` D1 profile row                    |
|  [02]   | `D1Client.updateValues: never`                         | refused member      | the lane degradation row — per-row updates only |
|  [03]   | `D1ClientConfig.db` (`D1Database`)                     | binding adopt       | the Workers `env.DB` handle passed as a value   |
|  [04]   | `D1ClientConfig.prepareCacheSize` / `.prepareCacheTTL` | statement cache     | hot-path lever; `Config`-sourced                |
|  [05]   | `D1ClientConfig` (base)                                | telemetry/transform | shared driver base                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the driver Layer
- rail: data/lane
- `layer`/`layerConfig` yield `D1Client \| SqlClient` in one Layer, error only `ConfigError` (each wires `Reactivity` internally); `make` returns `Effect<D1Client, never, Scope \| Reactivity>`. Handle `env.DB` stays a value, never a connection string; the prepared-statement cache defaults to 200 entries over a 10-minute TTL.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `D1Client.layer(config)`                            | driver layer   | the Workers composition root passing `env.DB`       |
|  [02]   | `D1Client.layerConfig(Config.Wrap<D1ClientConfig>)` | driver layer   | cache knobs from `Config`; the handle stays a value |
|  [03]   | `D1Client.make(config)`                             | scoped make    | construction inside a larger acquire graph          |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): the query DSL, `SqlSchema`/`SqlResolver`/`Model` typed IO, and dialect arms are inherited; D1 rides the `sqlite` arm of `sql.onDialect`.
- Stack across `data`: the profile is one `lane/sqlite` row — per-database single-writer, database-per-tenant isolation, no LISTEN/NOTIFY, no interactive `withTransaction` (the lane's atomic-publish path is refused here; a D1 host composes the journal through batch statements or routes writes to the pg spine).

[LOCAL_ADMISSION]:
- Provide the layer at the Workers composition root only; neutral rows yield `SqlClient`.
- 10 GB per-database cap plus Sessions/Time-Travel semantics are platform facts consumed as lane degradation rows, never re-modeled.

[RAIL_LAW]:
- Package: `@effect/sql-d1`
- Owns: the D1 binding of `SqlClient` — `layer`/`layerConfig`/`make`, the `D1ClientConfig` handle-adopt config, the `updateValues: never` refusal
- Accept: the managed-edge profile row under the one sqlite lane contract, the binding handle as a value, batch-shaped writes
- Reject: a driver import in a neutral row, interactive-transaction assumptions, a second relational contract for the edge, re-modeled platform replication semantics
