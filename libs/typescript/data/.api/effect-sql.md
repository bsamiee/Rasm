# [TS_DATA_API_EFFECT_SQL]

`@effect/sql` is the SQL core the whole `store` plane types against — one `SqlClient` `Context.Tag` a driver `Layer` satisfies, so the journal, projection, capability, scope, retrieve, and object rows code once against the neutral contract and the app root picks the runtime (`-pg` durable spine, `-sqlite-node`/`-bun` server lane, `-sqlite-wasm` browser lane). `sql` builds a `Fragment` that is simultaneously an `Effect<ReadonlyArray<A>>`, a `Stream`, and a `Pipeable`, carrying a dialect-parameterized `Compiler` and the polymorphic helper surface (`sql.insert`/`update`/`in`/`and`/`or`/`csv`/`join`/`onDialect`) — a new dialect is a driver `Layer` plus an `onDialect` arm, never a parallel journal. `SqlSchema` decodes rows through a `Schema` at both request and result edge; `SqlResolver` batches N+1 through `effect`'s `Request`/`RequestResolver` with span links; `Model.Class` derives six wire variants (`select`/`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate`) from one declaration; `withTransaction` nests as savepoints and makes the OCC-append + outbox + idempotency-ledger atomic. `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` bind the SQL spine under `@effect/experimental`'s overlay `Storage` Tags. `Migrator` ships but stays branch-banned — the record of truth is the append-only journal with read-time upcasting, and DDL is idempotent declarative ensure split `iac`↔`store`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql`
- package: `@effect/sql`
- license: `MIT`
- effect-peer: `effect ^catalog`, `@effect/platform ^catalog`, `@effect/experimental ^catalog` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`, `.api/effect-experimental.md`)
- dependency: `uuid ^11` (bundled; binary-UUID mint for `Model.UuidV4Insert`)
- module format: ESM + CJS dual (`dist/dts` typings); per-module deep-import subpaths (`@effect/sql/SqlClient`, `/Statement`, `/Model`, …), `sideEffects: []`
- runtime: dialect-neutral abstract core — no driver binding of its own; a `-pg`/`-sqlite-node`/`-sqlite-bun`/`-sqlite-wasm` package binds the `SqlClient` `Layer`. Portable to every runtime the driver reaches
- rail: the `store` SQL contract (journal spine, projection lanes, capability probes, tenancy scopes, dialect lanes, retrieval, object metadata) — the neutral vocabulary `work` imports as its `SqlClient` port
- modules: `SqlClient`, `Statement`, `SqlSchema`, `SqlResolver`, `SqlConnection`, `SqlError`, `SqlStream`, `Model`, `Migrator` (banned), `SqlEventJournal`, `SqlEventLogServer`, `SqlPersistedQueue`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client contract, connection, and transaction spine
- rail: boundaries
- `SqlClient` is the one `Context.Tag` every row yields; it extends `Statement.Constructor` so the client value IS the `sql` template. `Connection` is the driver-level execute surface a `Connection.Acquirer` (`Effect<Connection, SqlError, Scope>`) leases; `TransactionConnection` carries the in-flight connection + savepoint depth so nested `withTransaction` folds to savepoints, not a second BEGIN. `MakeOptions` carries `acquirer`/`compiler`/`transactionAcquirer`/`beginTransaction`/`commit`/`rollback`/`savepoint`/`transformRows`; `Connection` exposes `execute`/`executeRaw`/`executeStream`/`executeValues`/`executeUnprepared`.

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
- rail: surfaces-and-dispatch
- `Statement<A>` is the polymorphic query value: a `Fragment`, an `Effect<ReadonlyArray<A>>` (yield to run), and `Pipeable` at once. `Segment` is the closed union `Literal`/`Identifier`/`Parameter`/`ArrayHelper`/`Record*Helper`/`Custom` the `Compiler` folds to `[sql, params]`. `sql` callable `Constructor` carries `sql`…`` / `sql(id)` / `.unsafe` / `.literal` / `.insert` / `.update` / `.updateValues` / `.in` / `.and` / `.or` / `.csv` / `.join` / `.onDialect` / `.onDialectOrElse` — a dialect (`pg`/`sqlite`/`mysql`/`mssql`/`clickhouse`) is an `onDialect` arm-key, never a `sql.<dialect>` method. `Dialect` is the five-way discriminant `sql.onDialect` branches on.

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
- rail: rails-and-effects
- `SqlSchema` and `SqlResolver` are the parse-not-validate query surface: a `Request` `Schema` validates input, a `Result` `Schema` decodes rows, and decode failures ride `ParseError` in the `Effect` error channel — never an untyped row. `SqlResolver<T, I, A, E, R>`/`SqlRequest<T, A, E>` layer `effect`'s `Request`/`RequestResolver` batching over that, exposing `execute`/`request`/`cachePopulate`/`cacheInvalidate` with a `spanLink` per request. Variant axis splits `Model.VariantsDatabase` (`select`/`insert`/`update`) from `Model.VariantsJson` (`json`/`jsonCreate`/`jsonUpdate`). `SqlError` is the one tagged fault the whole rail fails into.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                             |
| :-----: | :---------------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `SqlError` (tagged `YieldableError`)            | fault rail      | driver failure on the `Effect` channel; `wire/fault` classifies |
|  [02]   | `ResultLengthMismatch`                          | tagged error    | `SqlResolver.ordered` guard — result count ≠ request count      |
|  [03]   | `SqlResolver` / `SqlRequest`                    | batched request | a resolver over `RequestResolver` (generics in lead)            |
|  [04]   | `Model.Any` / `Model.AnyNoContext`              | model bound     | constraint `makeRepository`/`makeDataLoaders` accept            |
|  [05]   | `Model.VariantsDatabase` / `Model.VariantsJson` | variant axis    | DB-variant trio vs JSON-variant trio (members in lead)          |

[PUBLIC_TYPE_SCOPE]: the `Model` variant-schema field families
- rail: shapes
- `Model.Class` derives one struct into six wire variants; a field's presence per variant is its type. Field families below are the parameterized vocabulary — a journal event, snapshot header, projection row, or idempotency-ledger row is a `Model.Class` composed from these, never per-entity insert/update/json schemas hand-written three times. `Model.Field`/`FieldOnly`/`FieldExcept`/`fieldEvolve`/`fieldFromKey` build/narrow/rename a variant field set, and `Override` forces a value into a generated variant.

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
- rail: surfaces-and-dispatch
- `sql` is the one query surface; `` sql`… ${id}` `` → `Statement<A>`, `yield*` runs it, and `.stream`/`.values`/`.raw`/`.unprepared` project it (a driver backs `.stream` with the `SqlStream.asyncPauseResume` backpressured cursor). `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` requires all five arms and returns the selected one; `sql.onDialectOrElse({ orElse, sqlite?, pg?, mysql?, mssql?, clickhouse? })` requires `orElse` with each dialect arm optional. DML helpers are `sql.insert(rows)`/`sql.update(row, omit?)`/`sql.updateValues(rows, alias)`/`.returning(cols)`; clause helpers `sql.in(col, values)`/`sql.and(clauses)`/`sql.or(clauses)`/`sql.csv(prefix, values)`/`sql.join(lit)`; escape hatches `sql.unsafe(text, params?)`/`sql.literal(text)`/`Statement.unsafeFragment(text, params?)`; transformers `Statement.setTransformer(f)`/`withTransformer(f)`/`withTransformerDisabled`. `SqlStream.asyncPauseResume(register, bufferSize?)` → `Stream<A, E, R>` backs `Statement.stream`; `register` exposes `single`/`chunk`/`array`/`fail`/`end` emit plus `onInterrupt`/`onPause`/`onResume` so demand pauses the cursor.

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
- rail: rails-and-effects
- `withTransaction` wraps any `Effect` so every statement inside runs on one leased connection; nested calls become savepoints via `TransactionConnection` depth. `reactive`/`reactiveMailbox` (over `@effect/experimental` `Reactivity`) turn a query into a `Stream` that re-emits when its keys are invalidated — the `project/inline` read-your-writes signal. `sql.withTransaction(effect)` returns `Effect<A, E | SqlError, R>`, `sql.reserve` returns `Effect<Connection, SqlError, Scope>`, and `sql.reactive(keys, effect)` returns `Stream<A, E, R>`. `SqlClient.makeWithTransaction({ transactionTag, acquireConnection, begin, savepoint, commit, rollback, … })` is a driver's transaction machinery; `sql.reactiveMailbox(keys, effect)` returns `Effect<ReadonlyMailbox<A, E>, never, R | Scope>`, and `SqlClient.make(options)` returns `Effect<SqlClient, never, Reactivity>`.

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]                                                   |
| :-----: | :------------------------------------ | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `sql.withTransaction(effect)`         | transaction      | `journal/append` — OCC append + `outbox` + ledger claim atomic        |
|  [02]   | `SqlClient.makeWithTransaction(opts)` | driver txn       | driver transaction machinery; nested depth → savepoint (opts in lead) |
|  [03]   | `sql.reserve`                         | lease conn       | scoped raw-connection lease for LISTEN/NOTIFY, COPY, advisory locks   |
|  [04]   | `sql.reactive(keys, effect)`          | reactive read    | `project/inline` — re-run when `Reactivity.invalidate(keys)` fires    |
|  [05]   | `sql.reactiveMailbox(keys, effect)`   | reactive mailbox | pull-model reactive consumer for `browser/persist` read lanes         |
|  [06]   | `SqlClient.make(options)`             | assemble client  | a driver builds the neutral client from `MakeOptions`                 |

[ENTRYPOINT_SCOPE]: schema-typed query and batching resolver
- rail: rails-and-effects
- `SqlSchema.*` is the typed query: input `Schema` in, result `Schema` out, `ParseError` on the error channel — `findAll`/`findOne` (→ `Option`)/`single` (→ `A | NoSuchElement`)/`void`, each `{ Request, Result, execute }`. `SqlResolver.*` is the batched form over `RequestResolver` — `ordered(tag, { Request, Result, execute })`, `grouped(tag, { Request, RequestGroupKey, Result, ResultGroupKey, execute })`, `findById(tag, { Id, Result, ResultId, execute })` → `Option`, `void(tag, { Request, execute })`; `resolver.execute(input)`/`.cachePopulate(id, result)`/`.cacheInvalidate(id)` dispatch and cache. Read lanes collapse N+1 into one round-trip.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `SqlSchema.findAll`/`findOne`/`single`/`void` | typed query    | projection reads into `state`; one query, arity in ctor            |
|  [02]   | `SqlResolver.ordered(tag, opts)`              | batch resolver | 1:1 order-matched (`ResultLengthMismatch`) — `project/async`       |
|  [03]   | `SqlResolver.grouped(tag, opts)`              | batch resolver | 1:N grouped by key — `retrieve/hybrid` fan-in of per-key sets      |
|  [04]   | `SqlResolver.findById(tag, opts)` → `Option`  | batch resolver | id→`Option` DataLoader; `withContext: true` threads a `Schema` req |
|  [05]   | `SqlResolver.void(tag, opts)`                 | batch resolver | batched write, no decode — windowed touch/complete                 |
|  [06]   | `resolver.execute`/`.cacheInvalidate`         | dispatch/cache | run a batched request; seed/evict the per-resolver cache           |

[ENTRYPOINT_SCOPE]: the `Model` domain class and its repository/loader helpers
- rail: shapes
- `Model.Class<Self>(id)(fields)` is one declaration yielding `.select`/`.insert`/`.update`/`.json`/`.jsonCreate`/`.jsonUpdate` schemas; variant access is `Row.insert`/`Row.update`/`Row.json`/`Row.jsonCreate`/`Row.jsonUpdate`/`Model.fields(Row)`. `Model.makeRepository(Row, { tableName, spanPrefix, idColumn })` → `{ insert, update, findById, delete, … }` and `Model.makeDataLoaders(Row, { tableName, spanPrefix, idColumn, window, maxBatchSize? })` are the mutable-CRUD and batched-loader helpers — admissible for projection tables, the snapshot store, and the idempotency ledger, but NOT the record of truth (the append-only journal never issues UPDATE/DELETE). Variant builders are `Field`/`FieldOnly`/`FieldExcept`/`Struct`/`Union`/`extract`.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                               |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `class Row extends Model.Class<Row>("Row")({ … })` | declare model   | one class → six variants; journal/snapshot/projection/ledger rows |
|  [02]   | `Row.insert` / `Row.update` / `Row.json` / …       | variant access  | per-edge schema `SqlSchema`/`SqlResolver` decode; JSON for `edge` |
|  [03]   | `Model.makeRepository(Row, opts)`                  | crud repo       | projection/snapshot/ledger CRUD; `SqlClient` — NOT the journal    |
|  [04]   | `Model.makeDataLoaders(Row, opts)`                 | batched loader  | windowed `insert`/`findById`/`delete` loaders, read-heavy         |
|  [05]   | variant builders `Field`/`FieldOnly`/…             | variant compose | nested/renamed field sets; `Union` for multi-`_tag` families      |

[ENTRYPOINT_SCOPE]: the `@effect/experimental` overlay-storage SQL bindings
- rail: rails-and-effects
- These bind the SQL spine under `@effect/experimental`'s overlay `Context.Tag`s (`.api/effect-experimental.md` `[R4]`/`[R19]`). EventLog overlay's SERVER side and durable queue persist onto the same journal-owning `SqlClient` — the overlay accelerates local-first reads, the SQL journal stays the record of truth. `SqlEventJournal.layer({ eventLogTable?, remotesTable? })` → `Layer<EventJournal, SqlError, SqlClient>`; `SqlEventLogServer.layerStorage({ entryTablePrefix?, remoteIdTable?, insertBatchSize? })` → `Layer<EventLogServer.Storage, SqlError, SqlClient | EventLogEncryption>` with a `layerStorageSubtle(options?)` Web-Crypto variant; `SqlPersistedQueue.layerStore({ tableName?, pollInterval?, lockRefreshInterval?, lockExpiration? })` → `Layer<PersistedQueueStore, SqlError, SqlClient>`.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                         |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `SqlEventJournal.layer(opts)`                    | journal store     | durable-node `EventJournal` entry store                     |
|  [02]   | `SqlEventLogServer.layerStorage(opts)`           | sync server store | E2E-encrypted sync-server storage; `edge/live` mounts it    |
|  [03]   | `SqlEventLogServer.layerStorageSubtle(options?)` | sync server store | zero-knowledge Web-Crypto variant, no `EventLogEncryption`  |
|  [04]   | `SqlPersistedQueue.layerStore(opts)`             | durable queue     | `work` durable-job store — SKIP-LOCKED poll + lease refresh |

## [04]-[IMPLEMENTATION_LAW]

[SQL_TOPOLOGY]:
- one Tag, driver-supplied Layer: `SqlClient` is an abstract `Context.Tag` with no binding that every `store` row yields (`const sql = yield* SqlClient`), and the app root binds exactly one driver `Layer` (`PgClient.layer` durable, `SqliteClient.layer` node/bun, `SqliteClient.layer` wasm browser). Runtime portability is a `Layer` selection — the journal code never names a dialect. A new lane is one driver `Layer` plus, where SQL differs, one `sql.onDialect` arm; never a parallel journal, projection, or client family.
- polymorphic statement surface: `sql\`…\`` returns a value that is a `Fragment`, an `Effect`, and `Pipeable` at once — the caller `yield*`s it to get rows, read `.stream` for a backpressured cursor, `.values` for raw tuples, `.raw` for driver-native output, `.unprepared` to skip the prepared-statement cache, `.compile()` to reflect `[sql, params]`. There is no `query`/`queryOne`/`queryMany` proliferation: arity lives in the `SqlSchema` combinator (`findAll`/`findOne`/`single`), and dialect lives in `onDialect`.
- transactions nest as savepoints: `sql.withTransaction(effect)` leases one connection and runs every statement inside it; a nested `withTransaction` reads `TransactionConnection` depth and emits `SAVEPOINT`/`RELEASE`/`ROLLBACK TO`, so the OCC append + transactional outbox + idempotency-ledger claim commit atomically and a composed sub-operation rolls back to its savepoint without aborting the outer commit.
- parse-not-validate at both edges: `SqlSchema`/`SqlResolver`/`Model` decode every row through a `Schema` — the request schema validates input, the result schema decodes the untyped `Connection.Row`, and a decode miss is a `ParseError` on the `Effect` error channel. No untyped row reaches domain code; no exception carries a query failure (`SqlError` is a tagged `YieldableError`).
- migrations are banned: the `Migrator` family (`Migrator.make`, `fromGlob`/`fromBabelGlob`/`fromRecord`, and the driver `SqliteMigrator`/`PgMigrator` re-exports) exists in the package but is NOT admitted. `store` is no-migration by construction — the record of truth is the append-only journal, schema evolution is a read-time `journal/upcast` total fold, and DDL is idempotent declarative ensure split `iac` (apply at provision) ↔ `store` (verify at startup). Runtime never mutates schema.

[INTEGRATION_LAW]:
- Stack on `effect` (`.api/effect.md`): the statement is an `Effect`; `withTransaction` is an effect transformer; `SqlError` is a tagged member of the `Effect` error channel discriminated by `catchTag`. `Model` fields ARE `Schema` (`Generated`/`Sensitive`/`DateTimeInsert` compose `Schema.brand`/`optionalWith`/`transform`), and `SqlSchema`/`SqlResolver` lift `ParseResult.ParseError` into `E`. `sql.reactive` yields a `Stream`, and `SqlResolver` composes `Request`/`RequestResolver` for batching — this SQL tier adds no new rail, it is `effect` applied to durable persistence.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): `SqlClient.reactive`/`reactiveMailbox` require the `Reactivity` service — `project/inline` emits `Reactivity.invalidate(keys)` after an OCC append, re-running the reactive query (read-your-writes). `Model.Class` builds on `@effect/experimental` `VariantSchema`. `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` satisfy the overlay's `EventJournal`/`EventLogServer.Storage`/`PersistedQueueStore` Tags `[R4]`, so the SQL journal is the durable backing beneath the local-first overlay, never a second authority `[R19]`.
- Stack on `@effect/platform` (`.api/effect-platform.md`): the driver `layerConfig` reads its DSN/filename from `Config` behind `PlatformConfigProvider`; a `sql.reserve`d `Connection` frames LISTEN/NOTIFY and COPY over the platform `Socket`. Banned `SqliteMigrator`'s `FileSystem`/`Path`/`CommandExecutor` requirements are exactly why it stays out — `store` needs no filesystem-driven schema step.
- Stack across `store`: the driver Layer's `SqlClient` is the sole seam `journal`/`project`/`capability`/`scope`/`retrieve`/`object` share; `scope/tenant` sets the `app.current_tenant` GUC inside `withTransaction`; `capability/row` compiles fail-closed extension probes via `sql.unsafe`/`.compile()`; `work` and `security/session` never import `store` — they declare a `SqlClient`/journal port the `store` Layer satisfies at the app root.

[LOCAL_ADMISSION]:
- Use `SqlClient` (the neutral Tag) in every row; never import a driver package (`-pg`/`-sqlite-*`) inside `journal`/`project`/`capability`/`retrieve`/`scope` — drivers are admitted only at the app composition root and on the runtime-scoped `./server`/`./browser`/`./wasm` subpaths.
- Use the `sql` fragment DSL (`sql.insert`/`update`/`in`/`and`/`or`/`csv`/`join`) and `sql.onDialect` for dialect variance; never string-concatenate SQL or hand-write a per-dialect journal — `sql.unsafe`/`.literal` is the only escape and only for provably-safe literals.
- Use `SqlSchema`/`SqlResolver`/`Model` for typed I/O; never read a raw `Connection.Row` in domain code, never a `query`/`getById`/`getMany` family — one `SqlSchema` combinator discriminates arity.
- Use `sql.withTransaction` for the OCC-append + outbox + ledger atomic commit; never a manual `BEGIN`/`COMMIT` statement pair — nested composition must fold to savepoints.
- `Model.makeRepository`/`makeDataLoaders` are for projection/snapshot/ledger tables only; the append-only event journal never issues UPDATE/DELETE on its events (crypto-shredding is key destruction in `retain`, never a row rewrite).
- `Migrator` is banned branch-wide; a schema change is an `iac` declarative-ensure edit plus a `store` startup verify, never a migration script.

[RAIL_LAW]:
- Package: `@effect/sql`
- Owns: the `SqlClient` neutral `Context.Tag` + `Connection`/`TransactionConnection` spine, the `sql` fragment DSL (`Statement`/`Fragment`/`Segment`/`Compiler` + unsafe/literal/insert/update/updateValues/in/and/or/csv/join/onDialect/onDialectOrElse), the `SqlStream.asyncPauseResume` backpressured-cursor primitive behind `.stream`, `SqlSchema` typed queries, `SqlResolver` batching resolvers, the `SqlError`/`ResultLengthMismatch` fault rail, the `Model` variant-schema system (six wire variants + field families + `makeRepository`/`makeDataLoaders`), `reserve`/`reactive`/`reactiveMailbox`/`withTransaction`, and the `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` overlay-storage Layers
- Accept: one driver `Layer` per runtime behind the neutral `SqlClient` Tag, the `sql` DSL + `onDialect` for all query construction, `SqlSchema`/`SqlResolver`/`Model` for typed I/O, `withTransaction` for atomic commits, `reactive` for read-your-writes, the SQL overlay bindings as the durable backing under `@effect/experimental` `[R4]`/`[R19]`
- Reject: a driver import outside the composition root / runtime subpaths, string-built or per-dialect-forked SQL, untyped `Connection.Row` reads, `query`/`getById`/`getMany` operation families, manual `BEGIN`/`COMMIT`, `makeRepository` on the event journal, and any `Migrator` use (migrations are banned — DDL is `iac`↔`store` declarative ensure)
