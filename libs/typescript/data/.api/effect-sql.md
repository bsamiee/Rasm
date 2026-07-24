# [TS_DATA_API_EFFECT_SQL]

`@effect/sql` mints the neutral SQL core the `store` plane types against: one `SqlClient` `Context.Tag` a driver `Layer` satisfies, so every row codes against the contract while the app root selects the runtime.

`sql` builds a `Fragment` that is at once `Effect`, `Stream`, and `Pipeable`, dialect-parameterized through `onDialect`; `SqlSchema`/`SqlResolver`/`Model` parse every row through a `Schema`, `withTransaction` nests as savepoints, and the append-only journal — never `Migrator` — holds truth.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql`
- package: `@effect/sql` (MIT)
- effect-peer: `effect`, `@effect/platform`, `@effect/experimental`
- dependency: `uuid` (bundled; binary-UUID mint for `Model.UuidV4Insert`)
- module format: ESM + CJS dual (`dist/dts` typings); per-module deep-import subpaths (`@effect/sql/SqlClient`, `/Statement`, `/Model`, …), `sideEffects: []`
- runtime: dialect-neutral abstract core with no driver binding; a `-pg`/`-sqlite-node`/`-sqlite-bun`/`-sqlite-wasm` package binds the `SqlClient` `Layer`, so the core rides every runtime the driver reaches
- rail: the `store` SQL contract every plane composes as its `SqlClient` port
- modules: `SqlClient`, `Statement`, `SqlSchema`, `SqlResolver`, `SqlConnection`, `SqlError`, `SqlStream`, `Model`, `Migrator` (banned), `SqlEventJournal`, `SqlEventLogServer`, `SqlPersistedQueue`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client contract, connection, and transaction spine
- `SqlClient` extends `Statement.Constructor`, so the client value IS the `sql` template; a `Connection.Acquirer` (`Effect<Connection, SqlError, Scope>`) leases the driver `Connection`, and `TransactionConnection` carries `[conn, depth]` so a nested `withTransaction` folds to a savepoint, not a second `BEGIN`.
- `MakeOptions` fields: `acquirer`/`compiler`/`transactionAcquirer`/`beginTransaction`/`commit`/`rollback`/`savepoint`/`transformRows`; `Connection` members: `execute`/`executeRaw`/`executeStream`/`executeValues`/`executeUnprepared`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]       | [CONSUMER_BOUNDARY]                                          |
| :-----: | :------------------------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `SqlClient` (interface, extends `Constructor`)     | `Context.Tag`       | the neutral client every row yields; `work` `SqlClient` port |
|  [02]   | `SqlClient.MakeOptions`                            | driver config       | a driver assembles the client from this (fields in lead)     |
|  [03]   | `SqlClient.safe` / `.withoutTransforms()`          | client variant      | `safe` for SafeQL; `withoutTransforms` drops name transforms |
|  [04]   | `Connection` / `Connection.Acquirer`               | driver surface      | the leased connection statements run on (members in lead)    |
|  [05]   | `Connection.Row` (`{ [column: string]: unknown }`) | row shape           | the untyped result row `SqlSchema`/`Model` decode            |
|  [06]   | `TransactionConnection`                            | `Context.Tag`       | in-transaction `[conn, depth]` — nested savepoint depth      |
|  [07]   | `SqlClient.SafeIntegers`                           | `Context.Reference` | per-fiber bigint-safety toggle for large-integer columns     |

[PUBLIC_TYPE_SCOPE]: the fragment-DSL statement and dialect compiler
- `Statement<A>` is at once a `Fragment`, an `Effect<ReadonlyArray<A>>`, and `Pipeable`; `Segment` is the closed union (`Literal`/`Identifier`/`Parameter`/`ArrayHelper`/`Record*Helper`/`Custom`) the `Compiler` folds to `[sql, params]`.
- `sql` `Constructor` members: `` sql`…` ``/`sql(id)`/`.unsafe`/`.literal`/`.insert`/`.update`/`.updateValues`/`.in`/`.and`/`.or`/`.csv`/`.join`/`.onDialect`/`.onDialectOrElse`; a `Dialect` (`pg`/`sqlite`/`mysql`/`mssql`/`clickhouse`) is an `onDialect` arm-key, never a `sql.<dialect>` method.
- every execution opens a `sql.execute` span (`.stream` a scoped same-name span) stamping the driver `spanAttributes` beside `db.operation.name`/`db.query.text`; `withTransaction` opens `sql.transaction` marking `db.transaction.commit`/`savepoint`/`rollback` events board queries key on.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :----------------------------------- | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | `Statement<A>`                       | query value       | runnable statement; `.stream`/`.values`/`.raw`/`.compile()` project   |
|  [02]   | `Fragment` / `FragmentId`            | SQL fragment      | composable clause; `sql.and`/`or`/`csv`/`join`/`literal` build+splice |
|  [03]   | `Dialect`                            | discriminant      | `sql.onDialect`/`onDialectOrElse` dispatch; `lane/sqlite` vs pg spine |
|  [04]   | `Constructor` (the `sql` callable)   | query surface     | the complete `sql` member set (roster in lead)                        |
|  [05]   | `Segment`                            | closed union      | compiler input; `Record*Helper.returning` RETURNING clause            |
|  [06]   | `Compiler` / `Statement.Transformer` | codec / transform | `compile(fragment) → [sql, params]`; `Transformer` snake↔camel        |
|  [07]   | `PrimitiveKind` / `Helper`           | value taxonomy    | parameter-binding kinds the compiler placeholders; helper union       |

[PUBLIC_TYPE_SCOPE]: schema-typed query, batching resolver, and error rail
- `SqlSchema`/`SqlResolver` parse-not-validate: a request `Schema` validates input, a result `Schema` decodes the untyped `Connection.Row`, and a decode miss rides `ParseError` on the `Effect` channel. `SqlResolver<T, I, A, E, R>`/`SqlRequest<T, A, E>` layer `effect`'s `Request`/`RequestResolver` batching with a `spanLink` per request inside a `sql.Resolver.batch <tag>` window span.
- `Model.VariantsDatabase` (`select`/`insert`/`update`) splits from `Model.VariantsJson` (`json`/`jsonCreate`/`jsonUpdate`); `SqlError` is the one tagged fault the rail fails into.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                             |
| :-----: | :---------------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `SqlError` (tagged `YieldableError`)            | fault rail      | driver failure on the `Effect` channel; `wire/fault` classifies |
|  [02]   | `ResultLengthMismatch`                          | tagged error    | `SqlResolver.ordered` guard — result count ≠ request count      |
|  [03]   | `SqlResolver` / `SqlRequest`                    | batched request | a resolver over `RequestResolver` (generics in lead)            |
|  [04]   | `Model.Any` / `Model.AnyNoContext`              | model bound     | constraint `makeRepository`/`makeDataLoaders` accept            |
|  [05]   | `Model.VariantsDatabase` / `Model.VariantsJson` | variant axis    | DB-variant trio vs JSON-variant trio (members in lead)          |

[PUBLIC_TYPE_SCOPE]: the `Model` variant-schema field families
- `Model.Class` derives one struct into six wire variants, a field's per-variant presence its type; `Field`/`FieldOnly`/`FieldExcept`/`fieldEvolve`/`fieldFromKey` build, narrow, and rename a variant field set, and `Override` forces a value into a generated variant. A journal event, snapshot header, projection row, or ledger row composes from the field families below, never a hand-written per-entity trio.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                              |
| :-----: | :------------------------------------------------ | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `Model.Generated` / `Model.GeneratedByApp`        | column origin    | DB-generated (select+update) vs app-generated (all DB variants)  |
|  [02]   | `Model.Sensitive`                                 | exposure control | DB-only, stripped from JSON — `retain`/DSAR never leaks it       |
|  [03]   | `Model.FieldOption`                               | nullability      | `null`-able in DB, missing-key `Option` in JSON — six variants   |
|  [04]   | `Model.DateTimeInsert*` / `Model.DateTimeUpdate*` | temporal field   | `FromDate`/`FromNumber` → `DateTime.Utc`; `occurredAt`           |
|  [05]   | `Model.JsonFromString`                            | embedded json    | object as TEXT in DB, native in JSON — event/snapshot body       |
|  [06]   | `Model.UuidV4Insert` / `Model.UuidV4WithGenerate` | binary uuid      | branded `Uint8Array` UUID minted on insert — stream/aggregate id |
|  [07]   | `Model.BooleanFromNumber`                         | sqlite bool      | `0\|1` ↔ boolean for the sqlite lane (no native boolean)         |
|  [08]   | field combinators (`Field`/`FieldOnly`/…)         | field combinator | build/narrow/rename a variant field set (roster in lead)         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: composing and running a statement
- `` sql`… ${id}` `` yields `Statement<A>`; `yield*` runs it and `.stream`/`.values`/`.raw`/`.unprepared` project it, `.stream` over the `SqlStream.asyncPauseResume(register, bufferSize?)` backpressured cursor whose `register` emits `single`/`chunk`/`array`/`fail`/`end` with `onInterrupt`/`onPause`/`onResume`.
- `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` requires all five arms; `sql.onDialectOrElse` requires `orElse` with each arm optional. DML: `sql.insert(rows)`/`update(row, omit?)`/`updateValues(rows, alias)`/`.returning(cols)`; clauses: `in`/`and`/`or`/`csv`/`join`; escapes: `unsafe`/`literal`/`Statement.unsafeFragment`; transformers: `setTransformer`/`withTransformer`/`withTransformerDisabled`.

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                         |
| :-----: | :---------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `` sql`… ${id}` `` → `Statement<A>`                   | run/project    | every read/write; `.stream` for `project/async` cursors     |
|  [02]   | `sql.insert`/`sql.update`/`sql.updateValues`          | build DML      | `journal/append` insert + RETURNING; `updateValues` pg-only |
|  [03]   | `sql.in`/`sql.and`/`sql.or`/`sql.csv`/`sql.join`      | clause build   | keyset/facet WHERE in `retrieve/hybrid`; `csv` for ORDER BY |
|  [04]   | `sql.onDialect` / `sql.onDialectOrElse`               | dialect branch | emit pg vs sqlite SQL from one definition (arms in lead)    |
|  [05]   | `sql.unsafe`/`sql.literal`/`Statement.unsafeFragment` | escape hatch   | provably-safe literal splice the fragment API can't express |
|  [06]   | `Statement.compile(withoutTransform?)`                | reflect        | `capability/row` probe, telemetry span attrs, SafeQL check  |
|  [07]   | `setTransformer`/`withTransformer`                    | name transform | Layer-wide snake↔camel mapping; off in raw passes           |
|  [08]   | `SqlStream.asyncPauseResume(register, bufferSize?)`   | cursor stream  | backpressured async-emit a driver wraps its `pg-cursor` in  |

[ENTRYPOINT_SCOPE]: transactions, savepoints, and reactive reads
- `sql.withTransaction(effect)` leases one connection for every inner statement, nesting to savepoints by `TransactionConnection` depth; `sql.reserve` yields `Effect<Connection, SqlError, Scope>` for LISTEN/NOTIFY, COPY, and advisory locks.
- `sql.reactive(keys, effect)` and `sql.reactiveMailbox(keys, effect)` (over `@effect/experimental` `Reactivity`) re-emit a `Stream`/`ReadonlyMailbox` when `Reactivity.invalidate(keys)` fires — the `project/inline` read-your-writes signal; `SqlClient.make(options)` returns `Effect<SqlClient, never, Reactivity>` and `makeWithTransaction(opts)` is the driver's transaction machinery.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :------------------------------------ | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `sql.withTransaction(effect)`         | transaction      | `journal/append` — OCC append + `outbox` + ledger claim atomic        |
|  [02]   | `SqlClient.makeWithTransaction(opts)` | driver txn       | driver transaction machinery; nested depth → savepoint (opts in lead) |
|  [03]   | `sql.reserve`                         | lease conn       | scoped raw-connection lease for LISTEN/NOTIFY, COPY, advisory locks   |
|  [04]   | `sql.reactive(keys, effect)`          | reactive read    | `project/inline` — re-run when `Reactivity.invalidate(keys)` fires    |
|  [05]   | `sql.reactiveMailbox(keys, effect)`   | reactive mailbox | pull-model reactive consumer for `browser/persist` read lanes         |
|  [06]   | `SqlClient.make(options)`             | assemble client  | a driver builds the neutral client from `MakeOptions`                 |

[ENTRYPOINT_SCOPE]: schema-typed query and batching resolver
- `SqlSchema` typed query: input `Schema` in, result `Schema` out, `ParseError` on the error channel — `findAll`/`findOne` (→ `Option`)/`single` (→ `A | NoSuchElement`)/`void`, each `{ Request, Result, execute }`.
- `SqlResolver` batches the same over `RequestResolver`: `ordered(tag, opts)` (`ResultLengthMismatch` on count skew), `grouped(tag, opts)` (per-key `RequestGroupKey`/`ResultGroupKey`), `findById(tag, opts)` → `Option`, `void(tag, opts)`; `resolver.execute`/`.cachePopulate`/`.cacheInvalidate` dispatch and cache, collapsing N+1 to one round-trip.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `SqlSchema.findAll`/`findOne`/`single`/`void` | typed query    | projection reads into `state`; one query, arity in ctor            |
|  [02]   | `SqlResolver.ordered(tag, opts)`              | batch resolver | 1:1 order-matched (`ResultLengthMismatch`) — `project/async`       |
|  [03]   | `SqlResolver.grouped(tag, opts)`              | batch resolver | 1:N grouped by key — `retrieve/hybrid` fan-in of per-key sets      |
|  [04]   | `SqlResolver.findById(tag, opts)` → `Option`  | batch resolver | id→`Option` DataLoader; `withContext: true` threads a `Schema` req |
|  [05]   | `SqlResolver.void(tag, opts)`                 | batch resolver | batched write, no decode — windowed touch/complete                 |
|  [06]   | `resolver.execute`/`.cacheInvalidate`         | dispatch/cache | run a batched request; seed/evict the per-resolver cache           |

[ENTRYPOINT_SCOPE]: the `Model` domain class and its repository/loader helpers
- `Model.Class<Self>(id)(fields)` yields `.select`/`.insert`/`.update`/`.json`/`.jsonCreate`/`.jsonUpdate` schemas, accessed as `Row.insert`/… and `Model.fields(Row)`; variant builders are `Field`/`FieldOnly`/`FieldExcept`/`Struct`/`Union`/`extract`.
- `Model.makeRepository(Row, { tableName, spanPrefix, idColumn })` and `Model.makeDataLoaders(Row, { …, window, maxBatchSize? })` serve projection, snapshot, and idempotency-ledger tables, spanning `<spanPrefix>.<op>` / `<spanPrefix>/<op>`; the append-only journal issues no UPDATE/DELETE, so neither backs it.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                               |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `class Row extends Model.Class<Row>("Row")({ … })` | declare model   | one class → six variants; journal/snapshot/projection/ledger rows |
|  [02]   | `Row.insert` / `Row.update` / `Row.json` / …       | variant access  | per-edge schema `SqlSchema`/`SqlResolver` decode; JSON for `edge` |
|  [03]   | `Model.makeRepository(Row, opts)`                  | crud repo       | projection/snapshot/ledger CRUD; `SqlClient` — NOT the journal    |
|  [04]   | `Model.makeDataLoaders(Row, opts)`                 | batched loader  | windowed `insert`/`findById`/`delete` loaders, read-heavy         |
|  [05]   | variant builders `Field`/`FieldOnly`/…             | variant compose | nested/renamed field sets; `Union` for multi-`_tag` families      |

[ENTRYPOINT_SCOPE]: the `@effect/experimental` overlay-storage SQL bindings
- `SqlEventJournal.layer({ eventLogTable?, remotesTable? })` → `Layer<EventJournal, SqlError, SqlClient>`; `SqlEventLogServer.layerStorage({ entryTablePrefix?, remoteIdTable?, insertBatchSize? })` → `Layer<EventLogServer.Storage, SqlError, SqlClient | EventLogEncryption>`, `layerStorageSubtle(options?)` the zero-knowledge Web-Crypto variant; `SqlPersistedQueue.layerStore({ tableName?, pollInterval?, lockRefreshInterval?, lockExpiration? })` → `Layer<PersistedQueueStore, SqlError, SqlClient>`.
- these bind the SQL spine under `@effect/experimental`'s overlay Tags (`.api/effect-experimental.md`): the overlay accelerates local-first reads, the SQL journal stays the record of truth.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                         |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `SqlEventJournal.layer(opts)`                    | journal store     | durable-node `EventJournal` entry store                     |
|  [02]   | `SqlEventLogServer.layerStorage(opts)`           | sync server store | E2E-encrypted sync-server storage; `edge/live` mounts it    |
|  [03]   | `SqlEventLogServer.layerStorageSubtle(options?)` | sync server store | zero-knowledge Web-Crypto variant, no `EventLogEncryption`  |
|  [04]   | `SqlPersistedQueue.layerStore(opts)`             | durable queue     | `work` durable-job store — SKIP-LOCKED poll + lease refresh |

## [04]-[IMPLEMENTATION_LAW]

[SQL_TOPOLOGY]:
- one Tag, driver-supplied Layer: `SqlClient` is an abstract `Context.Tag` every `store` row yields (`const sql = yield* SqlClient`), and the app root binds exactly one driver `Layer`, so runtime portability is a `Layer` selection and the journal code never names a dialect. A new lane adds one driver `Layer` and one `sql.onDialect` arm where SQL differs — never a parallel journal, projection, or client family.
- polymorphic statement surface: one `sql` value serves every read and write — `.stream` for a backpressured cursor, `.values` for raw tuples, `.raw` for driver-native output, `.unprepared` to skip the prepared-statement cache, `.compile()` to reflect `[sql, params]` — so no `query`/`queryOne`/`queryMany` proliferation exists; arity lives in the `SqlSchema` combinator (`findAll`/`findOne`/`single`) and dialect in `onDialect`.
- transactions nest as savepoints: `sql.withTransaction(effect)` leases one connection for every inner statement, and a nested call reads `TransactionConnection` depth to emit `SAVEPOINT`/`RELEASE`/`ROLLBACK TO`, so the OCC append, transactional outbox, and idempotency claim commit atomically while a composed sub-operation rolls back to its savepoint alone.
- parse-not-validate at both edges: `SqlSchema`/`SqlResolver`/`Model` decode every untyped `Connection.Row` through a `Schema`, a request schema validating input and a decode miss riding `ParseError` on the `Effect` error channel; no untyped row reaches domain code and `SqlError` (a tagged `YieldableError`) carries every query fault.
- migrations are banned: `Migrator` (`make`, `fromGlob`/`fromBabelGlob`/`fromRecord`, the driver `SqliteMigrator`/`PgMigrator` re-exports) ships but is not admitted; `store` is no-migration by construction — schema evolution is a read-time `journal/upcast` total fold, and DDL is idempotent declarative ensure split `iac` (apply at provision) ↔ `store` (verify at startup). Runtime never mutates schema.

[INTEGRATION_LAW]:
- Stack on `effect` (`.api/effect.md`): the statement is an `Effect`, `withTransaction` an effect transformer, `SqlError` a `catchTag`-discriminated member of the error channel; `Model` fields ARE `Schema` (`Generated`/`Sensitive`/`DateTimeInsert` compose `Schema.brand`/`optionalWith`/`transform`) and `SqlSchema`/`SqlResolver` lift `ParseError` into `E`. This tier adds no rail — it is `effect` applied to durable persistence.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): `reactive`/`reactiveMailbox` require the `Reactivity` service — `project/inline` emits `Reactivity.invalidate(keys)` after an OCC append; `Model.Class` builds on `VariantSchema`; `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` satisfy the overlay's `EventJournal`/`EventLogServer.Storage`/`PersistedQueueStore` Tags `[SQL_OVERLAY_BACKING]`, the durable backing never a second authority `[OVERLAY_BOUNDARY_RULING]`.
- Stack on `@effect/platform` (`.api/effect-platform.md`): the driver `layerConfig` reads its DSN/filename from `Config` behind `PlatformConfigProvider`, and a `sql.reserve`d `Connection` frames LISTEN/NOTIFY and COPY over the platform `Socket`; the banned `SqliteMigrator`'s `FileSystem`/`Path`/`CommandExecutor` needs are exactly why it stays out.
- Stack across `store`: the driver Layer's `SqlClient` is the sole seam `journal`/`project`/`capability`/`scope`/`retrieve`/`object` share; `scope/tenant` sets the `app.current_tenant` GUC inside `withTransaction`, `capability/row` compiles fail-closed extension probes via `sql.unsafe`/`.compile()`, and `work`/`security/session` declare a `SqlClient`/journal port the `store` Layer satisfies rather than importing `store`.

[LOCAL_ADMISSION]:
- Yield the neutral `SqlClient` Tag in every row; a driver package (`-pg`/`-sqlite-*`) is imported only at the app composition root and the runtime-scoped `./server`/`./browser`/`./wasm` subpaths.
- Build every query through the `sql` fragment DSL and `sql.onDialect` for dialect variance; `sql.unsafe`/`.literal` is the sole escape, only for provably-safe literals.
- Type all I/O through `SqlSchema`/`SqlResolver`/`Model`, one `SqlSchema` combinator discriminating arity, so no raw `Connection.Row` read and no `query`/`getById`/`getMany` family survive.
- Wrap the OCC-append + outbox + ledger commit in `sql.withTransaction`; nested composition folds to savepoints, never a manual `BEGIN`/`COMMIT` pair.
- `Model.makeRepository`/`makeDataLoaders` serve projection, snapshot, and ledger tables only; the append-only journal issues no UPDATE/DELETE on its events (crypto-shredding is key destruction in `retain`, never a row rewrite).
- A schema change is an `iac` declarative-ensure edit with a `store` startup verify; `Migrator` is banned branch-wide.

[RAIL_LAW]:
- Package: `@effect/sql`
- Owns: the `SqlClient` neutral `Context.Tag` + `Connection`/`TransactionConnection` spine, the `sql` fragment DSL (`Statement`/`Fragment`/`Segment`/`Compiler` + unsafe/literal/insert/update/updateValues/in/and/or/csv/join/onDialect/onDialectOrElse), the `SqlStream.asyncPauseResume` backpressured-cursor primitive behind `.stream`, `SqlSchema` typed queries, `SqlResolver` batching resolvers, the `SqlError`/`ResultLengthMismatch` fault rail, the `Model` variant-schema system (six wire variants + field families + `makeRepository`/`makeDataLoaders`), `reserve`/`reactive`/`reactiveMailbox`/`withTransaction`, and the `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` overlay-storage Layers
- Accept: one driver `Layer` per runtime behind the neutral `SqlClient` Tag, the `sql` DSL + `onDialect` for all query construction, `SqlSchema`/`SqlResolver`/`Model` for typed I/O, `withTransaction` for atomic commits, `reactive` for read-your-writes, the SQL overlay bindings as the durable backing under `@effect/experimental` `[SQL_OVERLAY_BACKING]`/`[OVERLAY_BOUNDARY_RULING]`
- Reject: a driver import outside the composition root / runtime subpaths, string-built or per-dialect-forked SQL, untyped `Connection.Row` reads, `query`/`getById`/`getMany` operation families, manual `BEGIN`/`COMMIT`, `makeRepository` on the event journal, and any `Migrator` use (migrations are banned — DDL is `iac`↔`store` declarative ensure)
