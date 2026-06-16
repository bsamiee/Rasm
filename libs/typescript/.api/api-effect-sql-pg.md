# [API_CATALOGUE] @effect/sql-pg

Decompile-verified from the installed distribution at `node_modules/@effect/sql-pg/dist/dts`
(version `0.52.1`). The package is a thin Postgres dialect over `@effect/sql`: it owns the
`PgClient` service (a `SqlClient` specialization), its config and pool constructors, the Postgres
statement compiler, the `PgJson` custom fragment, and a `PgMigrator` that re-exports the generic
`@effect/sql/Migrator` loaders. Peer surface: `effect ^3.21`, `@effect/sql ^0.51`,
`@effect/platform ^0.96`, `@effect/experimental ^0.60`, and the host `pg` driver.

The package root `.` re-exports two namespaces only — every symbol is reached through one of them.

```ts
// @effect/sql-pg
export * as PgClient from "@effect/sql-pg/PgClient"
export * as PgMigrator from "@effect/sql-pg/PgMigrator"
```

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-pg`
- package: `@effect/sql-pg`
- entry: `@effect/sql-pg` (namespace re-export), `@effect/sql-pg/PgClient`, `@effect/sql-pg/PgMigrator`
- asset: `PgClient` service + tag, config/pool constructors, layer constructors, Postgres statement compiler, `PgJson` custom fragment, migrator runner/layer over `@effect/sql/Migrator`
- rail: persistence / sql-postgres

## [2]-[PUBLIC_TYPES]

### @effect/sql-pg/PgClient — client service family

[PUBLIC_TYPE_SCOPE]: type id, model, tag
- rail: persistence
- entry: `@effect/sql-pg/PgClient`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :--------------- | :----------------- | :----------------------------------------------------------- |
|   [1]   | `TypeId`         | const + type alias | branded id `"~@effect/sql-pg/PgClient"`                      |
|   [2]   | `PgClient`       | interface          | extends `Client.SqlClient`; adds Postgres-only members       |
|   [3]   | `PgClient` (tag) | `Context.Tag`      | `Context.Tag<PgClient, PgClient>` — service accessor         |
|   [4]   | `PgClientConfig` | interface          | full connection/transform/types config (all fields optional) |

[PUBLIC_TYPE_SCOPE]: constructors
- rail: persistence

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------ | :------------ | :--------------------------------------------- |
|   [1]   | `make`                    | constructor   | scoped effect → `PgClient`                     |
|   [2]   | `PgClientFromPoolOptions` | interface     | wraps an existing `Pg.Pool` acquire effect     |
|   [3]   | `fromPool`                | constructor   | build `PgClient` from a caller-owned `pg` pool |
|   [4]   | `makeCompiler`            | constructor   | `Statement.Compiler` for the Postgres dialect  |

[PUBLIC_TYPE_SCOPE]: layers
- rail: persistence

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :-------------- | :------------ | :------------------------------------------------------------ |
|   [1]   | `layerConfig`   | layer ctor    | from `Config.Config.Wrap<PgClientConfig>`; adds `ConfigError` |
|   [2]   | `layer`         | layer ctor    | from a plain `PgClientConfig`                                 |
|   [3]   | `layerFromPool` | layer ctor    | from `PgClientFromPoolOptions`                                |

[PUBLIC_TYPE_SCOPE]: custom types
- rail: persistence

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [RAIL]                                                             |
| :-----: | :--------- | :-------------- | :----------------------------------------------------------------- |
|   [1]   | `PgCustom` | type alias      | `= PgJson` — union of dialect custom fragment kinds (one member)   |
|   [2]   | `PgJson`   | `Custom` + ctor | tagged `Custom<"PgJson", unknown>`; callable as a fragment builder |

Every layer constructor provisions BOTH `PgClient` and the abstract `Client.SqlClient` tag
(`Layer.Layer<PgClient | Client.SqlClient, …>`), so downstream code may depend on either tag.

```ts contract
import * as PgClient from "@effect/sql-pg/PgClient"
import type { Custom, Fragment } from "@effect/sql/Statement"
import * as Statement from "@effect/sql/Statement"
import * as Client from "@effect/sql/SqlClient"
import { SqlError } from "@effect/sql/SqlError"
import * as Config from "effect/Config"
import type * as ConfigError from "effect/ConfigError"
import * as Context from "effect/Context"
import * as Duration from "effect/Duration"
import * as Effect from "effect/Effect"
import * as Layer from "effect/Layer"
import * as Redacted from "effect/Redacted"
import * as Reactivity from "@effect/experimental/Reactivity"
import * as Scope from "effect/Scope"
import * as Stream from "effect/Stream"
import type { Duplex } from "node:stream"
import type { ConnectionOptions } from "node:tls"
import * as Pg from "pg"

// --- type id ---
const TypeId: TypeId
type TypeId = "~@effect/sql-pg/PgClient"

// --- model: PgClient extends the abstract SqlClient with Postgres-only members ---
interface PgClient extends Client.SqlClient {
  readonly [TypeId]: TypeId
  readonly config: PgClientConfig
  readonly json: (_: unknown) => Fragment
  readonly listen: (channel: string) => Stream.Stream<string, SqlError>
  readonly notify: (channel: string, payload: string) => Effect.Effect<void, SqlError>
}

// --- tag (service accessor) ---
const PgClient: Context.Tag<PgClient, PgClient>

// --- config (all fields optional; `?` + `| undefined` exactOptional-correct) ---
interface PgClientConfig {
  readonly url?: Redacted.Redacted | undefined
  readonly host?: string | undefined
  readonly port?: number | undefined
  readonly path?: string | undefined
  readonly ssl?: boolean | ConnectionOptions | undefined
  readonly database?: string | undefined
  readonly username?: string | undefined
  readonly password?: Redacted.Redacted | undefined
  readonly stream?: (() => Duplex) | undefined
  readonly idleTimeout?: Duration.DurationInput | undefined
  readonly connectTimeout?: Duration.DurationInput | undefined
  readonly maxConnections?: number | undefined
  readonly minConnections?: number | undefined
  readonly connectionTTL?: Duration.DurationInput | undefined
  readonly applicationName?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly transformResultNames?: ((str: string) => string) | undefined
  readonly transformQueryNames?: ((str: string) => string) | undefined
  readonly transformJson?: boolean | undefined
  readonly types?: Pg.CustomTypesConfig | undefined
}

// --- constructors ---
const make: (options: PgClientConfig) =>
  Effect.Effect<PgClient, SqlError, Scope.Scope | Reactivity.Reactivity>

interface PgClientFromPoolOptions {
  readonly acquire: Effect.Effect<Pg.Pool, SqlError, Scope.Scope>
  readonly applicationName?: string | undefined
  readonly spanAttributes?: Record<string, unknown> | undefined
  readonly transformResultNames?: ((str: string) => string) | undefined
  readonly transformQueryNames?: ((str: string) => string) | undefined
  readonly transformJson?: boolean | undefined
  readonly types?: Pg.CustomTypesConfig | undefined
}

// Build a PgClient from a caller-owned `pg` pool; you own the pool lifecycle via `acquire`
// (typically `Effect.acquireRelease`).
const fromPool: (options: PgClientFromPoolOptions) =>
  Effect.Effect<PgClient, SqlError, Scope.Scope | Reactivity.Reactivity>

// --- layers (provision both PgClient and Client.SqlClient) ---
const layerConfig: (config: Config.Config.Wrap<PgClientConfig>) =>
  Layer.Layer<PgClient | Client.SqlClient, ConfigError.ConfigError | SqlError>

const layer: (config: PgClientConfig) =>
  Layer.Layer<PgClient | Client.SqlClient, SqlError>

const layerFromPool: (options: PgClientFromPoolOptions) =>
  Layer.Layer<PgClient | Client.SqlClient, SqlError>

// --- Postgres statement compiler ---
const makeCompiler: (
  transform?: (_: string) => string,
  transformJson?: boolean
) => Statement.Compiler

// --- custom fragment types ---
type PgCustom = PgJson
interface PgJson extends Custom<"PgJson", unknown> {}
// PgJson is also a callable fragment builder (not exported as a value name; reached via PgClient.json):
declare const PgJson: (i0: unknown, i1: void, i2: void) => Fragment
```

### @effect/sql-pg/PgMigrator — migration runner family

[PUBLIC_TYPE_SCOPE]: runner + layer
- rail: persistence / migration
- entry: `@effect/sql-pg/PgMigrator`

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :------- | :------------ | :---------------------------------------------------------- |
|   [1]   | `run`    | constructor   | runs pending migrations; yields `ReadonlyArray<[id, name]>` |
|   [2]   | `layer`  | layer ctor    | runs migrations as a `Layer<never, …>` startup effect       |

The module also `export *` re-exports the entire generic `@effect/sql/Migrator` and
`@effect/sql/Migrator/FileSystem` surfaces (see [3]), so `MigratorOptions`, `Loader`,
`MigrationError`, `fromGlob`, `fromRecord`, `fromFileSystem`, etc. are all reachable through
`PgMigrator`.

```ts contract
import * as PgMigrator from "@effect/sql-pg/PgMigrator"
import * as Migrator from "@effect/sql/Migrator"
import type { CommandExecutor } from "@effect/platform/CommandExecutor"
import { FileSystem } from "@effect/platform/FileSystem"
import { Path } from "@effect/platform/Path"
import type * as Client from "@effect/sql/SqlClient"
import type { SqlError } from "@effect/sql/SqlError"
import * as Effect from "effect/Effect"
import * as Layer from "effect/Layer"
import { PgClient } from "@effect/sql-pg/PgClient"

const run: <R2 = never>(options: Migrator.MigratorOptions<R2>) =>
  Effect.Effect<
    ReadonlyArray<readonly [id: number, name: string]>,
    Migrator.MigrationError | SqlError,
    FileSystem | Path | PgClient | Client.SqlClient | CommandExecutor | R2
  >

const layer: <R>(options: Migrator.MigratorOptions<R>) =>
  Layer.Layer<
    never,
    Migrator.MigrationError | SqlError,
    PgClient | Client.SqlClient | CommandExecutor | FileSystem | Path | R
  >
```

## [3]-[RE_EXPORTED_MIGRATOR_SURFACE]

`PgMigrator` re-exports `@effect/sql/Migrator` (and the `FileSystem` loader). These are the
generic symbols a Postgres migration setup consumes — captured here because the Postgres page is
the consumption site and the spellings are load-bearing for `run`/`layer` options.

[PUBLIC_TYPE_SCOPE]: migrator options + loaders
- rail: migration
- entry: `@effect/sql/Migrator`, `@effect/sql/Migrator/FileSystem`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------ |
|   [1]   | `MigratorOptions<R>` | interface     | `{ loader; schemaDirectory?; table? }`                        |
|   [2]   | `Loader<R>`          | type alias    | effect yielding `ReadonlyArray<ResolvedMigration>`            |
|   [3]   | `ResolvedMigration`  | tuple alias   | `[id: number, name: string, load: Effect<…, …, SqlClient>]`   |
|   [4]   | `Migration`          | interface     | `{ id; name; createdAt }` applied-row record                  |
|   [5]   | `MigrationError`     | tagged error  | `_tag: "MigrationError"`; `reason` enum below                 |
|   [6]   | `make`               | constructor   | dialect-parameterized migrator builder (`dumpSchema` hook)    |
|   [7]   | `fromGlob`           | loader        | `Record<string, () => Promise<any>>` → `Loader`               |
|   [8]   | `fromBabelGlob`      | loader        | `Record<string, any>` → `Loader`                              |
|   [9]   | `fromRecord`         | loader        | `Record<string, Effect<void, unknown, SqlClient>>` → `Loader` |
|  [10]   | `fromFileSystem`     | loader        | `(directory: string) => Loader<FileSystem>`                   |

```ts contract
interface MigratorOptions<R = never> {
  readonly loader: Loader<R>
  readonly schemaDirectory?: string
  readonly table?: string
}

type Loader<R = never> = Effect.Effect<ReadonlyArray<ResolvedMigration>, MigrationError, R>

type ResolvedMigration = readonly [
  id: number,
  name: string,
  load: Effect.Effect<any, any, Client.SqlClient>
]

interface Migration {
  readonly id: number
  readonly name: string
  readonly createdAt: Date
}

class MigrationError extends YieldableError {
  readonly _tag: "MigrationError"
  readonly cause?: unknown
  readonly reason: "bad-state" | "import-error" | "failed" | "duplicates" | "locked"
  readonly message: string
}

const make: <RD = never>(opts: {
  dumpSchema?: (path: string, migrationsTable: string) => Effect.Effect<void, MigrationError, RD>
}) => <R2 = never>(options: MigratorOptions<R2>) =>
  Effect.Effect<ReadonlyArray<readonly [id: number, name: string]>, MigrationError | SqlError, Client.SqlClient | RD | R2>

const fromGlob: (migrations: Record<string, () => Promise<any>>) => Loader
const fromBabelGlob: (migrations: Record<string, any>) => Loader
const fromRecord: (migrations: Record<string, Effect.Effect<void, unknown, Client.SqlClient>>) => Loader
const fromFileSystem: (directory: string) => Loader<FileSystem>  // from @effect/sql/Migrator/FileSystem
```

## [4]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `PgClient` is a `SqlClient` specialization; the abstract query/transaction/fragment surface lives
  on `Client.SqlClient` (`@effect/sql` page), and only `config`, `json`, `listen`, and `notify` are
  Postgres-only additions. Depend on `Client.SqlClient` for portable query logic; depend on the
  `PgClient` tag only for `listen`/`notify`/`json`/`config`.
- Provision via a layer constructor, never bare `make`: `layer` for a static config, `layerConfig`
  for a `Config`-driven config (adds `ConfigError` to the layer error channel and reads from the
  Effect config provider), `layerFromPool` when an external owner controls the `pg` pool lifecycle.
- All three layers yield `PgClient | Client.SqlClient` — a single layer satisfies both tags, so no
  separate `SqlClient` wiring is required.
- `make`/`fromPool` carry `Scope.Scope | Reactivity.Reactivity` in their requirement channel; the
  layer constructors discharge `Scope` internally and `Reactivity` via the bundled default — bare
  `make`/`fromPool` must be run inside a scope with `Reactivity` provided.
- `password` and `url` are `Redacted.Redacted`; build them with `Redacted.make` / read from config
  as redacted so secrets never enter spans or logs.
- LISTEN/NOTIFY: `listen(channel)` is a `Stream.Stream<string, SqlError>` (long-lived subscription);
  `notify(channel, payload)` is a one-shot `Effect`. These are the only push-style surfaces.

[MIGRATOR_TOPOLOGY]:
- `PgMigrator.run` and `PgMigrator.layer` both require `PgClient | Client.SqlClient | FileSystem |
  Path | CommandExecutor` in context; provide the `PgClient` layer plus a platform layer
  (`NodeContext.layer` / `BunContext.layer`) that supplies `FileSystem`, `Path`, `CommandExecutor`.
- Pick the loader by source: `fromFileSystem(dir)` for on-disk SQL/TS migration files (adds
  `FileSystem` to the loader requirement), `fromGlob` for bundler `import.meta.glob`-style lazy
  modules, `fromRecord` for inline programmatic migrations.
- `MigrationError.reason` is a closed five-member enum (`"bad-state" | "import-error" | "failed" |
  "duplicates" | "locked"`); match on it rather than on `message` text.
- `MigratorOptions.table` defaults the applied-migrations bookkeeping table; `schemaDirectory`
  targets `make`'s `dumpSchema` hook for schema-dump-on-migrate setups.

[COMPILER_TOPOLOGY]:
- `makeCompiler(transform?, transformJson?)` returns a `Statement.Compiler` (`@effect/sql/Statement`
  page) — the Postgres dialect compiler. `transform` rewrites identifiers; `transformJson` toggles
  the `PgJson` custom-fragment path. The same transform pair is exposed declaratively on
  `PgClientConfig.transformQueryNames` / `transformResultNames` / `transformJson`.
- `PgJson` is the single member of the `PgCustom` union; it is reached through `PgClient.json(_)`
  (yielding a `Fragment`), not imported as a value. Treat `PgCustom` as the dialect custom-fragment
  extension point.

[RAIL_LAW]:
- `@effect/sql-pg`: persistence rail; node/bun tier only (hard dependency on the `pg` driver and
  `node:stream`/`node:tls`). Never imported in a browser bundle. The wire/transport tiers carry
  query results as decoded domain values, not `PgClient` handles.
