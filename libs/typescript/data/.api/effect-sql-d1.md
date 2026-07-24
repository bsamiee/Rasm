# [TS_DATA_API_EFFECT_SQL_D1]

`@effect/sql-d1` binds the neutral `@effect/sql` `SqlClient` to a Cloudflare Workers `D1Database` binding — the managed-edge profile of the one sqlite lane. Config adopts the live `env.DB` handle as a value, never a connection string, and the driver refuses interactive transactions and multi-row updates, running every statement batch/exec-shaped over a prepared-statement cache.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-d1`
- package: `@effect/sql-d1` (MIT)
- effect-peer: `effect`, `@effect/sql` (`.api/effect-sql.md`), `@effect/experimental` (`Reactivity`), `@effect/platform`
- backing: the Workers `D1Database` runtime binding, no bundled driver; `@cloudflare/workers-types` supplies the binding types
- module format: ESM + CJS dual (`dist/dts` typings); subpath `@effect/sql-d1/D1Client`; `sideEffects: []`
- runtime: Cloudflare Workers only — node/bun ride `-sqlite-node`/`-sqlite-bun`, the browser `-sqlite-wasm`
- rail: the `store` `lane/sqlite` managed-edge dialect — the pg spine's journal and projection contracts minus interactive transactions
- modules: `D1Client`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `D1Client` service and its handle-adopt config

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `D1Client` (Tag / interface, extends `SqlClient`)      | service Tag     | neutral rows compose it as `SqlClient`           |
|  [02]   | `D1Client.config` (`D1ClientConfig`)                   | property        | resolved-config accessor                         |
|  [03]   | `D1Client.updateValues: never`                         | refused member  | declares the per-row-update degradation          |
|  [04]   | `D1ClientConfig.db` (`D1Database`)                     | binding adopt   | adopts the `env.DB` handle as a value            |
|  [05]   | `D1ClientConfig.prepareCacheSize` / `.prepareCacheTTL` | statement cache | prepared-statement hot-path lever from `Config`  |
|  [06]   | `D1ClientConfig` (base)                                | transform base  | shared telemetry and snake↔camel name transforms |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing and providing the driver Layer

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `D1Client.layer(D1ClientConfig)`                                                | Workers composition root passing `env.DB`           |
|  [02]   | `D1Client.layerConfig(Config.Wrap<D1ClientConfig>)`                             | cache knobs from `Config`; the handle stays a value |
|  [03]   | `D1Client.make(D1ClientConfig) -> Effect<D1Client, never, Scope \| Reactivity>` | scoped construction inside a larger acquire graph   |

- `layer`/`layerConfig`: yield `D1Client \| SqlClient` in one Layer (`ConfigError` only), wiring `Reactivity` internally; only `make` surfaces `Reactivity` as a requirement.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- neutral-Tag superset: `D1Client extends SqlClient`, so `layer`/`layerConfig` bind both — journal, projection, and retrieve rows keep yielding the abstract `SqlClient` and stay dialect-portable, a driver-distinct row yields the concrete `D1Client`, and swapping to another sqlite lane or the pg spine is a `Layer` selection at the Workers root.
- no interactive transaction: `transactionAcquirer` dies via `Effect.dieMessage`, so `sql.withTransaction` is refused and `updateValues` is `never`; the lane runs every statement batch/exec-shaped over the prepared-statement cache, and a write needing atomicity composes batch statements or routes to the pg spine.

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): `D1Client extends SqlClient`, so the `sql` fragment DSL, `SqlSchema`/`SqlResolver`/`Model` typed IO, `reactive`, and the overlay-storage Layers compose unchanged; only `withTransaction` and `updateValues` are refused, their dialect gap branched through `sql.onDialect`'s `sqlite` arm the driver compiles via `Statement.makeCompilerSqlite`.
- `data` (`lane/sqlite`): the profile is one lane row — per-database single-writer, database-per-tenant isolation, no LISTEN/NOTIFY, no interactive `withTransaction`; a D1 host composes the journal through batch statements or routes writes to the pg spine.

[LOCAL_ADMISSION]:
- Provide the layer at the Workers composition root only; neutral rows yield `SqlClient`.
- Platform facts — the per-database size cap, Sessions read-replication, Time-Travel PITR — enter as `lane/sqlite` degradation rows, never re-modeled in the driver.

[RAIL_LAW]:
- Package: `@effect/sql-d1`
- Owns: the D1 binding of `SqlClient` — `layer`/`layerConfig`/`make`, the `D1ClientConfig` handle-adopt config, the `updateValues: never` refusal
- Accept: the managed-edge profile row under the one sqlite lane, the `env.DB` handle as a value, batch-shaped writes
- Reject: a driver import in a neutral row, `withTransaction` and interactive-transaction assumptions, a second relational contract for the edge, re-modeled platform replication semantics
