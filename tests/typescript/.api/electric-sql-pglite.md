# [@electric-sql/pglite] — in-process pg for the fast unit lane, no server extensions

[PACKAGE_SURFACE]:
- package: `@electric-sql/pglite` · version `0.5.3` · license `Apache-2.0`
- module: ESM (`type: module`) with a CJS mirror (`.cjs` + `.d.cts` under every export); subpath map is real — `.`, `./live`, `./worker`, `./template`, and `./contrib/*` per-extension entries.
- asset: `dist/index.d.ts` (barrel over the hashed type bundle `dist/pglite-*.d.ts`); WASM binary + fs bundle ship inside the package (`fsBundle` / `pgliteWasmModule` overridable).
- runtime: WebAssembly PGlite (a single Postgres build compiled to WASM) — single-connection, in-process, no socket, no Docker; runs identically in node, bun, browser, and worker.
- plane: `plane:dev` — the `_testkit` fast unit lane; the container lane's counterpart is `testcontainers.md` (real pg-18.4-with-server-extensions).
- rail: persistence-verification / in-process-sql.

`@electric-sql/pglite` is the fast half of the `_testkit` harness (`tests/typescript/_testkit`): the whole database is a WASM instance the spec constructs, seeds with raw DDL, and discards — microsecond startup versus the container lane's seconds. The `_testkit` unit lane wraps one `PGlite` in an effect `Layer` (acquire `PGlite.create` → release `close`) shared across a spec block via `@effect/vitest` `layer(...)`, exposing the `query`/`sql`/`exec`/`transaction` surface. It is the lane for query logic that needs no SERVER extension (pgvector, postgis, the CNPG image rows) — those force the container lane. The `tests/typescript/_architecture` suite bans `@effect/sql/Migrator` and `@effect/sql-pg/PgMigrator` branch-wide, so schema setup here is raw `exec(ddl)`, never a migrator.

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

```ts contract
// dataDir absent ⇒ in-memory (the unit-lane default); prefer PGlite.create over `new` so extension namespaces type through.
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
  blob?: Blob                       // COPY … TO output
}
type PGliteInterface<T extends Extensions = Extensions> = InitializedExtensions<T> & {
  readonly waitReady: Promise<void>; readonly ready: boolean; readonly closed: boolean; readonly debug: DebugLevel
  close(): Promise<void>
  query<T>(query: string, params?: any[], options?: QueryOptions): Promise<Results<T>>
  sql<T>(sqlStrings: TemplateStringsArray, ...params: any[]): Promise<Results<T>>   // tagged-template, auto-parametrized
  exec(query: string, options?: QueryOptions): Promise<Array<Results>>             // multi-statement DDL — the seeding rail
  describeQuery(query: string): Promise<DescribeQueryResult>
  transaction<T>(callback: (tx: Transaction) => Promise<T>): Promise<T>            // auto rollback on throw
  runExclusive<T>(fn: () => Promise<T>): Promise<T>                                // serialize against the one connection
  listen(channel: string, cb: (payload: string) => void, tx?: Transaction): Promise<(tx?: Transaction) => Promise<void>>
  unlisten(channel: string, cb?: (payload: string) => void, tx?: Transaction): Promise<void>
  onNotification(cb: (channel: string, payload: string) => void): () => void
  execProtocol(message: Uint8Array, options?: ExecProtocolOptions): Promise<ExecProtocolResult>  // raw wire frames
  dumpDataDir(compression?: DumpTarCompressionOptions): Promise<File | Blob>       // snapshot; reload via loadDataDir
  refreshArrayTypes(): Promise<void>
}
```

[PUBLIC_TYPE_SCOPE]: construction bag — the fields the unit lane sets.

```ts contract
interface PGliteOptions<TExtensions extends Extensions = Extensions> {
  dataDir?: string                  // absent ⇒ in-memory; "idb://name" ⇒ IdbFs; "memory://" explicit
  extensions?: TExtensions          // client-side wasm extensions keyed by namespace (see [03])
  relaxedDurability?: boolean       // skip fsync flush-to-fs — the unit-lane speed switch
  debug?: DebugLevel
  username?: string; database?: string
  loadDataDir?: Blob | File         // restore a dumpDataDir tarball — the frozen-fixture reload path
  parsers?: ParserOptions; serializers?: SerializerOptions   // pgType(number) → decode/encode overrides
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

```ts contract
// query`SELECT * FROM ${identifier`t`} ${withFilter ? sql`WHERE a = ${x}` : sql``}`  → { query: 'SELECT * FROM "t" WHERE a = $1', params: [x] }
declare function sql(strings: TemplateStringsArray, ...values: any[]): TemplateContainer
declare function identifier(strings: TemplateStringsArray, ...values: any[]): TemplatePart
declare function raw(strings: TemplateStringsArray, ...values: any[]): TemplatePart
```

Barrel also exports `parse` (wire parser) and `formatQuery`, and `protocol` (the `messages` frame namespace) for `execProtocol`-level assertions.

## [03]-[EXTENSIONS_AND_LANES]

The extension mechanism is ONE parameterized shape, not a fixed roster: an `Extension` is `{ name, setup }` keyed into `PGliteOptions.extensions` by namespace, and `PGlite.create` types the resulting namespace onto the handle (`PGliteInterfaceExtensions`). The `./contrib/*` roster (`amcheck`, `auto_explain`, `bloom`, `btree_gin`, `btree_gist`, `citext`, `cube`, `earthdistance`, `fuzzystrmatch`, `hstore`, `intarray`, `isn`, `lo`, `ltree`, `pg_trgm`, `pgcrypto`, `seg`, `tablefunc`, `tsm_system_rows`, `unaccent`, `uuid_ossp`, … — 33 bundled) and the first-party `live` extension are SEED ROWS on that shape. This is the CLIENT-side wasm-contrib surface — orthogonal to the SERVER extensions (`pgvector`, `postgis`, the CNPG image rows) that force the `testcontainers` lane; "no server extensions" names that boundary, not a ban on `live` or the bundled contribs.

```ts contract
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

[STACK: `PGlite` + `effect/Layer` + `@effect/vitest`] — the unit lane is a shared Layer, not a per-spec construct. `Layer.scoped(Tag, Effect.acquireRelease(Effect.promise(() => PGlite.create({ relaxedDurability: true })), db => Effect.promise(() => db.close())))` builds the handle once; the standalone `layer(PgLiteTest)("suite", (it) => …)` combinator (from `@effect/vitest`, see `fast-check.md` [05]) shares it across the block, and `Effect.tryPromise` wraps each `db.query`/`db.exec` into the folder's typed error rail. Seed DDL runs once in the Layer's acquire via `db.exec(schemaSql)`.

[STACK CONSTRAINT: no migrator] — `@effect/sql` (0.51) is admitted substrate, but the `tests/typescript/_architecture` suite asserts ZERO `@effect/sql/Migrator` / `@effect/sql-pg/PgMigrator` imports branch-wide. There is no `@effect/sql-pglite` dialect; the unit lane does NOT bridge PGlite through `@effect/sql-pg` (that binds the real `pg` driver / the container lane). Schema for a PGlite spec is raw `exec(ddl)` or a `dumpDataDir` fixture reload via `loadDataDir`; the container/real-pg lane (`testcontainers.md`) is migrator-free too, seeding server-extension DDL via raw `@effect/sql-pg` `sql` execute — the ban is branch-wide, so no lane runs a migrator, and the container lane owns real-pg, not a migration path.

[STACK: frozen-fixture reload] — a spec that must assert against a known database state reloads a `dumpDataDir` tarball through `PGliteOptions.loadDataDir`, aligning with the `tests/contracts/` byte-frozen fixtures: the tarball is the frozen bytes, PGlite the reproducer.

## [05]-[RAIL_LAW]

- Owns: an in-process, single-connection Postgres for the fast verification lane; raw SQL execution, transactions with auto-rollback, tagged-template parametrization, reactive `live` queries, and `dumpDataDir`/`loadDataDir` snapshots.
- Accept: `PGlite.create` with `relaxedDurability: true` and an in-memory `dataDir` for unit specs; raw `exec` DDL seeding; the `live` extension for convergence specs; the `./contrib/*` client extensions a query under test needs.
- Reject: `@effect/sql-pg` / `PgMigrator` bridging (the container lane owns the real-pg `@effect/sql-pg` `PgClient` seam; migrations exist in NO lane — `PgMigrator` is banned branch-wide by the `tests/typescript/_architecture` suite, both lanes seed via raw DDL); server-extension-dependent assertions (route to `testcontainers`); concurrent access without `runExclusive` (single connection); importing from any `plane:runtime` folder — dev lane only.
- Boundary: PGlite is one Postgres WASM connection — no parallel sessions, no `pg_stat` cross-connection visibility, no server extensions; when a spec needs any of those, it is a container-lane spec by definition.
