# [TS_TESTS_API_ELECTRIC_SQL_PGLITE]

`@electric-sql/pglite` is the fast half of the `testkit` harness (`tests/typescript/testkit`): the whole database is a WASM instance the spec constructs, seeds with raw DDL, and discards — microsecond startup versus the container lane's seconds. `testkit`'s unit lane wraps one `PGlite` in an effect `Layer` (acquire `PGlite.create` → release `close`) shared across a spec block via `@effect/vitest` `layer(...)`, exposing the `query`/`sql`/`exec`/`transaction` surface.

It is the lane for query logic that needs no SERVER extension (pgvector, postgis, the CNPG image rows); schema setup here is raw `exec(ddl)`, never a migrator.

## [01]-[CORE]

[PUBLIC_TYPE_SCOPE]: the database handle and its result carriers — one interface owns every access mode.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `PGlite`                     | class               | the handle; `implements PGliteInterface, AsyncDisposable`               |
|  [02]   | `PGliteInterface<T>`         | type (intersection) | the full member contract + `InitializedExtensions<T>` namespaces        |
|  [03]   | `BasePGlite`                 | abstract class      | shared `query`/`sql`/`exec`/`transaction`/`describeQuery` impl base     |
|  [04]   | `Results<T>`                 | type                | `{ rows: Row<T>[]; affectedRows?; fields: {name,dataTypeID}[]; blob? }` |
|  [05]   | `Row<T>`                     | type alias          | `= T` — parametrized row shape (`rowMode:'object'` default)             |
|  [06]   | `Transaction`                | interface           | scoped `query`/`sql`/`exec` + `rollback` + `listen` + `closed`          |
|  [07]   | `QueryOptions`               | interface           | `rowMode`, `parsers`, `serializers`, `blob`, `onNotice`, `paramTypes`   |
|  [08]   | `PGliteOptions<TExtensions>` | interface           | construction bag — `dataDir`, `extensions`, `relaxedDurability`, fs     |
|  [09]   | `DescribeQueryResult`        | type                | prepared-statement param/result type descriptors                        |
|  [10]   | `DumpDataDirResult`          | interface           | `{ tarball: Uint8Array; extension; filename }` — snapshot payload       |
|  [11]   | `DebugLevel`                 | union `0..5`        | log verbosity                                                           |
|  [12]   | `IdbFs` / `MemoryFS`         | class               | persistence backends — IndexedDB vs in-memory (unit lane default)       |
|  [13]   | `Mutex`                      | class               | the single-connection serialization primitive `runExclusive` uses       |

```ts
declare class PGlite extends BasePGlite implements PGliteInterface, AsyncDisposable {
  constructor(dataDir?: string, options?: PGliteOptions)
  constructor(options?: PGliteOptions)
  static create<O extends PGliteOptions>(options?: O): Promise<PGlite & PGliteInterfaceExtensions<O["extensions"]>>
  static create<O extends PGliteOptions>(dataDir?: string, options?: O): Promise<PGlite & PGliteInterfaceExtensions<O["extensions"]>>
}
type Results<T = { [k: string]: any }> = {
  rows: Row<T>[]
  affectedRows?: number
  fields: { name: string; dataTypeID: number }[]
  blob?: Blob
}
type PGliteInterface<T extends Extensions = Extensions> = InitializedExtensions<T> & {
  readonly waitReady: Promise<void>; readonly ready: boolean; readonly closed: boolean; readonly debug: DebugLevel
  close(): Promise<void>
  query<T>(query: string, params?: any[], options?: QueryOptions): Promise<Results<T>>
  sql<T>(sqlStrings: TemplateStringsArray, ...params: any[]): Promise<Results<T>>
  exec(query: string, options?: QueryOptions): Promise<Array<Results>>
  describeQuery(query: string): Promise<DescribeQueryResult>
  transaction<T>(callback: (tx: Transaction) => Promise<T>): Promise<T>
  runExclusive<T>(fn: () => Promise<T>): Promise<T>
  listen(channel: string, cb: (payload: string) => void, tx?: Transaction): Promise<(tx?: Transaction) => Promise<void>>
  unlisten(channel: string, cb?: (payload: string) => void, tx?: Transaction): Promise<void>
  onNotification(cb: (channel: string, payload: string) => void): () => void
  execProtocol(message: Uint8Array, options?: ExecProtocolOptions): Promise<ExecProtocolResult>
  dumpDataDir(compression?: DumpTarCompressionOptions): Promise<File | Blob>
  refreshArrayTypes(): Promise<void>
}
```

[PUBLIC_TYPE_SCOPE]: construction bag — the fields the unit lane sets.

```ts
interface PGliteOptions<TExtensions extends Extensions = Extensions> {
  dataDir?: string
  extensions?: TExtensions
  relaxedDurability?: boolean
  debug?: DebugLevel
  username?: string; database?: string
  loadDataDir?: Blob | File
  parsers?: ParserOptions; serializers?: SerializerOptions
  initialMemory?: number; fsBundle?: Blob | File
  pgliteWasmModule?: WebAssembly.Module; initdbWasmModule?: WebAssembly.Module
  startParams?: string[]; postgresqlconf?: string[] | string
}
```

## [02]-[TEMPLATING]

`./template` composes SQL fragments without losing parametrization — the safe alternative to string concatenation in a spec's query builders.

| [INDEX] | [SURFACE]                      | [PRODUCES]          | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `query` (tagged template)      | `TemplatedQuery`    | `{ query, params }` with `$n` placeholders assigned     |
|  [02]   | `sql` (tagged template)        | `TemplateContainer` | nestable fragment — parametrized, composes into `query` |
|  [03]   | `identifier` (tagged template) | `TemplatePart`      | auto-escaped identifier (never a parameter)             |
|  [04]   | `raw` (tagged template)        | `TemplatePart`      | verbatim string, no escaping/parametrization            |

```ts
declare function sql(strings: TemplateStringsArray, ...values: any[]): TemplateContainer
declare function identifier(strings: TemplateStringsArray, ...values: any[]): TemplatePart
declare function raw(strings: TemplateStringsArray, ...values: any[]): TemplatePart
```

Barrel also exports `parse` (wire parser) and `formatQuery`, and `protocol` (the `messages` frame namespace) for `execProtocol`-level assertions.

## [03]-[EXTENSIONS_AND_LANES]

Extension mechanism is ONE parameterized shape, not a fixed roster: an `Extension` is `{ name, setup }` keyed into `PGliteOptions.extensions` by namespace, and `PGlite.create` types the resulting namespace onto the handle (`PGliteInterfaceExtensions`).

Seed rows on that shape are the `./contrib/*` roster (`amcheck`, `auto_explain`, `bloom`, `btree_gin`, `btree_gist`, `citext`, `cube`, `earthdistance`, `fuzzystrmatch`, `hstore`, `intarray`, `isn`, `lo`, `ltree`, `pg_trgm`, `pgcrypto`, `seg`, `tablefunc`, `tsm_system_rows`, `unaccent`, `uuid_ossp`, … — 33 bundled) and the first-party `live` extension.

This is the CLIENT-side wasm-contrib surface — orthogonal to the SERVER extensions (`pgvector`, `postgis`, the CNPG image rows) that force the `testcontainers` lane; "no server extensions" names that boundary, not a ban on `live` or the bundled contribs.

```ts
interface Extension<TNamespace = any> { name: string; setup: ExtensionSetup<TNamespace> }
type Extensions = { [namespace: string]: Extension | URL }
```

[SUBPATH: `./live`] — reactive queries for convergence/incremental-view specs. `live` is an `Extension`; `PGlite.create({ extensions: { live } })` types a `PGliteWithLive` whose `.live` namespace drives three modes:

| [INDEX] | [SURFACE]                                      | [PRODUCES]       | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `live.query(sql, params?, cb?)`                | `LiveQuery<T>`   | full result set re-fired on any dependency change |
|  [02]   | `live.incrementalQuery(sql, params, key, cb?)` | `LiveQuery<T>`   | keyed diff-minimal re-materialization             |
|  [03]   | `live.changes(sql, params, key, cb?)`          | `LiveChanges<T>` | `Change<T>[]` insert/update/delete stream by key  |

Each returns `{ initialResults, unsubscribe, refresh }` and accepts an options object (`LiveQueryOptions` with `signal?: AbortSignal`) as the alternative arity — one surface, request-shape discriminated.

[SUBPATH: `./worker`] — `PGliteWorker` (extends `BasePGlite`, same `PGliteInterface`) runs the instance in a Web Worker with cross-tab leader election; `worker({ init })` is the worker-side entry, `LeaderChangedError` the tab-handoff signal. Used only when a browser spec must share one instance across tabs; the node unit lane uses the main-thread `PGlite`.

## [04]-[INTEGRATION]

[STACK: `PGlite` + `effect/Layer` + `@effect/vitest`] — the unit lane is a shared Layer, not a per-spec construct. `Layer.scoped(Tag, Effect.acquireRelease(Effect.promise(() => PGlite.create({ relaxedDurability: true })), db => Effect.promise(() => db.close())))` builds the handle once; the standalone `layer(PgLiteTest)("suite", (it) => …)` combinator (from `@effect/vitest`, see `fast-check.md` [05]) shares it across the block, and `Effect.tryPromise` wraps each `db.query`/`db.exec` into the folder's typed error channel. Seed DDL runs once in the Layer's acquire via `db.exec(schemaSql)`.

[STACK CONSTRAINT: no migrator] — no spec lane imports `@effect/sql/Migrator` or `@effect/sql-pg/PgMigrator`. There is no `@effect/sql-pglite` dialect; the unit lane does NOT bridge PGlite through `@effect/sql-pg` (that binds the real `pg` driver). Schema for a PGlite spec is raw `exec(ddl)` or a `dumpDataDir` fixture reload via `loadDataDir`.

[STACK: frozen-fixture reload] — a spec that must assert against a known database state reloads a `dumpDataDir` tarball through `PGliteOptions.loadDataDir`, aligning with the `libs/contracts/conformance/` byte-frozen vectors: the tarball is the frozen bytes, PGlite the reproducer.
