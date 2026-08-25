# [TS_DATA_API_ELECTRIC_SQL_PGLITE]

`@electric-sql/pglite` runs single-user PostgreSQL in WebAssembly behind one promise-native connection. `lane/sqlite` owns its embedded profile and neutral `SqlClient` adapter.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine, neutral connection contract, execution values, extensions, workers, and filesystems

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `PGlite`                               | engine handle | ready/closed lifecycle plus the complete connection surface     |
|  [02]   | `PGliteOptions<TExtensions>`           | config        | data directory, filesystem, extensions, restore blobs, modules  |
|  [03]   | `PGliteInterface<TExtensions>`         | port          | query, transaction, notification, protocol, and dump contract   |
|  [04]   | `Results<T>` / `Row<T>`                | result        | materialized rows, field OIDs, affected rows, and optional blob |
|  [05]   | `QueryOptions`                         | query config  | row mode, parser/serializer overrides, notice, blob, param OIDs |
|  [06]   | `Transaction`                          | unit of work  | query/sql/exec/listen plus explicit rollback                    |
|  [07]   | `Extension` / `Extensions`             | capability    | named setup rows projected onto the instance type               |
|  [08]   | `LiveNamespace` / `LiveQuery<T>`       | reactive read | result subscription, refresh, and unsubscribe                   |
|  [09]   | `LiveChanges<T>` / `Change<T>`         | change feed   | keyed insert/update/delete/reset projections                    |
|  [10]   | `PGliteWorker` / `PGliteWorkerOptions` | worker proxy  | worker-resident connection plus leader-state observation        |
|  [11]   | `NodeFS`                               | filesystem    | server-runtime filesystem binding                               |
|  [12]   | `OpfsAhpFS` / `OpfsAhpOptions`         | filesystem    | browser OPFS access-handle-pool binding                         |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: acquire, execute, transact, observe, export, and release

Every surface is promise-native; `query` and `sql` return `Results<T>`, `exec` returns `Results[]`, and `transaction` returns the callback value.

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `PGlite.create(PGliteOptions?)`               | factory  | ready option-owned engine              |
|  [02]   | `PGlite.create(string, PGliteOptions?)`       | factory  | ready engine at one data directory     |
|  [03]   | `query<T>(string, unknown[]?, QueryOptions?)` | instance | parameterized materialized query       |
|  [04]   | `sql<T>(TemplateStringsArray, ...unknown[])`  | instance | parameterized tagged-template query    |
|  [05]   | `exec(string, QueryOptions?)`                 | instance | trusted batch or logical restore       |
|  [06]   | `transaction<T>((Transaction) -> Promise<T>)` | bracket  | connection-local atomic unit           |
|  [07]   | `listen(string, callback, Transaction?)`      | instance | notification subscription and disposer |
|  [08]   | `dumpDataDir("none" \| "gzip" \| "auto"?)`    | instance | physical PGDATA `File \| Blob`         |
|  [09]   | `close()` / `[Symbol.asyncDispose]()`         | release  | engine and filesystem teardown         |
|  [10]   | `PGlite.create({ extensions: { live } })`     | factory  | typed `live` namespace installation    |
|  [11]   | `live.query` / `changes` / `incrementalQuery` | reactive | subscribed result or change projection |
|  [12]   | `PGliteWorker.create(Worker, Options?)`       | factory  | worker proxy over `PGliteInterface`    |
|  [13]   | `new NodeFS(string)`                          | ctor     | server storage through `options.fs`    |
|  [14]   | `new OpfsAhpFS(string, OpfsAhpOptions?)`      | ctor     | browser OPFS through `options.fs`      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- PGLite exposes one single-user PostgreSQL connection.
- Embedded PostgreSQL answers `uuidv7()`, `MERGE`, `RETURNING old`/`new`, `VIRTUAL` generated columns, and `WITHOUT OVERLAPS` temporal keys natively.
- Multicolumn btree skip scan rides the embedded planner, so no consumer composes a prefix-scan substitute.
- Startup runs single-user under synchronous IO with no worker process, so no asynchronous IO method is reachable.
- One process or worker owns each writable generation; every proxy terminates at that owner.
- `query`, `sql`, and `exec` materialize arrays; `PGliteInterface` exposes no source stream.
- Neutral streaming requires bounded materialization with an explicit memory ceiling.
- `PGlite.create` awaits readiness and projects configured extension namespaces onto its result type.
- Constructor use requires a separate `waitReady`.
- `dumpDataDir` emits physical PGDATA for `PGliteOptions.loadDataDir`.
- `@electric-sql/pglite-tools` owns logical SQL export; `exec` restores it.

[STACKING]:
- `@effect/sql`: `Statement.makeCompiler` compiles neutral fragments; `SqlConnection.Connection` delegates to `PGlite.query`.
- `SqlClient.make` exposes that connection as the branch `SqlClient` Tag.
- `@effect/experimental`: `Reactivity.layer` supplies `SqlClient.make`; PGLite notifications retain neutral invalidation.
- `effect`: `PGlite.create` and `close` form one `Effect.acquireRelease`; promise faults lift through `Effect.tryPromise`.
- `Effect.makeSemaphore(1)` excludes statements across a complete transaction.
- `@electric-sql/pglite-tools`: `pgDump({ pg })` yields logical SQL for `candidate.exec(await file.text())`.
- `pg.dumpDataDir()` yields distinct physical same-engine recovery material.
- within-lib: configured `live` projects `LiveNamespace`; `PGliteWorker.create` proxies `PGliteInterface`.
- within-lib: `NodeFS` and `OpfsAhpFS` satisfy the `PGliteOptions.fs` seam.

[LOCAL_ADMISSION]:
- Construct at a generation-qualified coordinate; admit exact generation and capability evidence before publication.
- Profile and generation owners select `dataDir`, `loadDataDir`, `noInitDb`, `relaxedDurability`, start parameters, and PG config.
- Hold one semaphore permit per statement and across the complete `transaction` callback.
- One coordinate admits one writer.
- Parameterize values through `query`, tagged `sql`, or the neutral `Statement` compiler.
- Reserve `exec` for trusted contract material and admitted dump restoration.
- Treat memory, IndexedDB, OPFS, and server filesystem as explicit profile rows. No persistence mode silently falls back to memory.
- Capability evidence refuses pooling, server processes, replication, background workers, PITR, and superuser control.
- Capability evidence refuses multi-process writes and source streaming.
