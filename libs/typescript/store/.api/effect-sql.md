# [@effect/sql] — the dialect-neutral SQL rail: the `SqlClient` Tag, the fragment-DSL statement, schema-typed queries, batching resolvers, the `Model` variant system, and the transaction spine every journal, projection, and lane row composes

`@effect/sql` is the SQL core the whole `store` plane types against — one `SqlClient` `Context.Tag` a driver `Layer` satisfies, so the journal, projection, capability, scope, retrieve, and object rows code once against the neutral contract and the app root picks the runtime (`-pg` durable spine, `-sqlite-node`/`-bun` server lane, `-sqlite-wasm` browser lane). The `sql` template is a `Fragment` that is simultaneously an `Effect<ReadonlyArray<A>>`, a `Stream`, and a `Pipeable`, carrying a dialect-parameterized `Compiler` and the polymorphic helper surface (`sql.insert`/`update`/`in`/`and`/`or`/`csv`/`join`/`onDialect`) — a new dialect is a driver `Layer` plus an `onDialect` arm, never a parallel journal. `SqlSchema` decodes rows through a `Schema` at both request and result edge; `SqlResolver` batches N+1 through `effect`'s `Request`/`RequestResolver` with span links; `Model.Class` derives six wire variants (`select`/`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate`) from one declaration; `withTransaction` nests as savepoints and makes the OCC-append + outbox + idempotency-ledger atomic. `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` bind the SQL spine under `@effect/experimental`'s overlay `Storage` Tags. The `Migrator` family is present but branch-banned — the record of truth is the append-only journal with read-time upcasting, and DDL is idempotent declarative ensure split `iac`↔`store`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql`
- package: `@effect/sql`
- version: `0.51.1`
- license: `MIT`
- effect-peer: `effect ^3.21.2`, `@effect/platform ^0.96.1`, `@effect/experimental ^0.60.0` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`, `.api/effect-experimental.md`)
- dependency: `uuid ^11` (bundled; binary-UUID mint for `Model.UuidV4Insert`)
- module format: ESM + CJS dual (`dist/dts` typings); per-module deep-import subpaths (`@effect/sql/SqlClient`, `/Statement`, `/Model`, …), `sideEffects: []`
- runtime: dialect-neutral abstract core — no driver binding of its own; a `-pg`/`-sqlite-node`/`-sqlite-bun`/`-sqlite-wasm` package provides the `SqlClient` `Layer`. Portable to every runtime the driver supports
- rail: the `store` SQL contract (journal spine, projection lanes, capability probes, tenancy scopes, dialect lanes, retrieval, object metadata) — the neutral vocabulary `work` imports as its `SqlClient` port
- modules: `SqlClient`, `Statement`, `SqlSchema`, `SqlResolver`, `SqlConnection`, `SqlError`, `SqlStream`, `Model`, `Migrator` (banned), `SqlEventJournal`, `SqlEventLogServer`, `SqlPersistedQueue`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client contract, connection, and transaction spine
- rail: boundaries
- `SqlClient` is the one `Context.Tag` every row yields; it extends `Statement.Constructor` so the client value IS the `sql` template. `Connection` is the driver-level execute surface a `Connection.Acquirer` (`Effect<Connection, SqlError, Scope>`) leases; `TransactionConnection` carries the in-flight connection + savepoint depth so nested `withTransaction` folds to savepoints, not a second BEGIN.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `SqlClient` (interface, extends `Constructor`)      | `Context.Tag`     | the neutral client every `journal`/`project`/`capability`/`retrieve` row yields; `work` imports it as the `SqlClient` port |
|  [02]   | `SqlClient.MakeOptions`                             | driver config     | `acquirer`/`compiler`/`transactionAcquirer`/`begin`/`commit`/`rollback`/`savepoint`/`transformRows` — a driver assembles the client from this |
|  [03]   | `SqlClient.safe` / `.withoutTransforms()`           | client variant    | `safe` for SafeQL static analysis; `withoutTransforms` drops row/column name transforms for raw passes |
|  [04]   | `Connection` / `Connection.Acquirer`               | driver surface    | `execute`/`executeRaw`/`executeStream`/`executeValues`/`executeUnprepared`; the leased connection each statement runs on |
|  [05]   | `Connection.Row` (`{ [column: string]: unknown }`)  | row shape         | the untyped result row `SqlSchema`/`Model` decode into domain values |
|  [06]   | `TransactionConnection`                             | `Context.Tag`     | in-transaction `[conn, depth]` — nested `withTransaction` reads depth to emit a savepoint |
|  [07]   | `SqlClient.SafeIntegers`                            | `Context.Reference` | per-fiber bigint-safety toggle for large-integer columns (journal sequence numbers) |

[PUBLIC_TYPE_SCOPE]: the fragment-DSL statement and dialect compiler
- rail: surfaces-and-dispatch
- `Statement<A>` is the polymorphic query value: a `Fragment` (composable SQL segments), an `Effect<ReadonlyArray<A>>` (yield to run), and `Pipeable` at once. `Segment` is the closed union the `Compiler` folds to `[sql, params]`; `Constructor` is the `sql` callable carrying the whole helper surface. `Dialect` is the five-way discriminant `sql.onDialect` branches on — the one seam that lets a single journal/projection row compile for pg and sqlite alike.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Statement<A>` (`Fragment` + `Effect` + `Pipeable`) | query value        | the runnable statement; `.raw`/`.withoutTransform`/`.stream`/`.values`/`.unprepared`/`.compile()` project it |
|  [02]   | `Fragment` / `FragmentId`                           | SQL fragment       | composable clause value; `sql.and`/`or`/`csv`/`join`/`literal` build fragments spliced into a statement |
|  [03]   | `Dialect` (`"sqlite" \| "pg" \| "mysql" \| "mssql" \| "clickhouse"`) | discriminant | the value `sql.onDialect`/`onDialectOrElse` dispatches on; `lane/sqlite` vs the pg spine |
|  [04]   | `Constructor` (the `sql` callable)                  | query surface      | the complete member set — `sql\`…\`` / `sql(id)` / `.unsafe` / `.literal` / `.insert` / `.update` / `.updateValues` / `.in` / `.and` / `.or` / `.csv` / `.join` / `.onDialect` / `.onDialectOrElse`; a dialect (`pg`/`sqlite`/`mysql`/`mssql`/`clickhouse`) is an `onDialect` arm-KEY, never a `sql.<dialect>` method |
|  [05]   | `Segment` (`Literal`/`Identifier`/`Parameter`/`ArrayHelper`/`Record*Helper`/`Custom`) | closed union | the compiler input; `RecordInsertHelper.returning`/`RecordUpdateHelper` carry the RETURNING clause (`xmax=0` outbox claim) |
|  [06]   | `Compiler` / `Statement.Transformer`               | codec / transform  | dialect `compile(fragment) → [sql, params]`; a `Transformer` rewrites statements Layer-wide (snake↔camel names) |
|  [07]   | `PrimitiveKind` / `Helper`                         | value taxonomy     | the parameter-binding kinds the compiler placeholders; helper-fragment union |

[PUBLIC_TYPE_SCOPE]: schema-typed query, batching resolver, and error rail
- rail: rails-and-effects
- `SqlSchema` and `SqlResolver` are the parse-not-validate query surface: a `Request` `Schema` validates input, a `Result` `Schema` decodes rows, and decode failures ride `ParseError` in the `Effect` error channel — never an untyped row. `SqlResolver` layers `effect`'s `Request`/`RequestResolver` batching over that with per-request span links and a populate/invalidate cache. `SqlError` is the one tagged fault the whole rail fails into.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `SqlError` (tagged `YieldableError`)                | fault rail         | every statement/driver failure; flows the `Effect` error channel — `wire/fault` classifies, never a throw |
|  [02]   | `ResultLengthMismatch`                              | tagged error       | `SqlResolver.ordered` guard — result count ≠ request count; the batching-integrity fault |
|  [03]   | `SqlResolver<T, I, A, E, R>` / `SqlRequest<T, A, E>` | batched request   | a resolver over `RequestResolver`; `execute`/`request`/`cachePopulate`/`cacheInvalidate`; `spanLink` per request |
|  [04]   | `Model.Any` / `Model.AnyNoContext`                 | model bound        | the constraint `makeRepository`/`makeDataLoaders` accept; a `Model.Class` with all six variant schemas |
|  [05]   | `Model.VariantsDatabase` / `Model.VariantsJson`    | variant axis       | `"select"\|"insert"\|"update"` vs `"json"\|"jsonCreate"\|"jsonUpdate"` — the wire-shape discriminant |

[PUBLIC_TYPE_SCOPE]: the `Model` variant-schema field families
- rail: shapes
- `Model.Class` derives one struct into six wire variants; a field's presence per variant is its type. The field families below are the parameterized vocabulary — a journal event, snapshot header, projection row, or idempotency-ledger row is a `Model.Class` composed from these, never per-entity insert/update/json schemas hand-written three times.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Model.Generated` / `Model.GeneratedByApp`         | column origin      | DB-generated (select+update, no insert) vs app-generated (all DB variants) — journal sequence / stream ids |
|  [02]   | `Model.Sensitive`                                  | exposure control   | DB variants only, stripped from every JSON variant — the `retain`/DSAR export never leaks it |
|  [03]   | `Model.FieldOption`                                | nullability        | `null`-able in DB variants, missing-key `Option` in JSON — one field, all six variants optionalized |
|  [04]   | `Model.DateTimeInsert*` / `Model.DateTimeUpdate*`  | temporal field     | `insertFromDate`/`FromNumber` — auto-`DateTime.Utc` on insert/update, serialized per column type; journal `occurredAt` |
|  [05]   | `Model.JsonFromString`                             | embedded json      | object stored as TEXT in DB variants, native object in JSON variants — event payload / snapshot body column |
|  [06]   | `Model.UuidV4Insert` / `Model.UuidV4WithGenerate`  | binary uuid        | branded `Uint8Array` UUID minted on insert — stream/aggregate id keyed `(appKey, tenantId, aggregate)` |
|  [07]   | `Model.BooleanFromNumber`                          | sqlite bool        | `0\|1` ↔ boolean transform for the sqlite lane (no native boolean) |
|  [08]   | `Model.Field` / `FieldOnly` / `FieldExcept` / `fieldEvolve` / `fieldFromKey` | field combinator | build/narrow/rename a variant field set; `Override` forces a value into a generated variant |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: composing and running a statement
- rail: surfaces-and-dispatch
- `sql` is the one query surface; the statement is yielded to run, or projected to `.stream`/`.values`/`.raw` (a driver backs `.stream` with the `SqlStream.asyncPauseResume` backpressured cursor). The record helpers (`sql.insert`/`update`/`in`) build parameterized DML with a `.returning`, and `sql.onDialect` is how one row compiles for both the pg spine and the sqlite lanes.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `` sql`SELECT … WHERE id = ${id}` `` → `Statement<A>`; `yield*` runs it, `.stream` / `.values` / `.raw` / `.unprepared` project | run/project | every read/write row; `.stream` for `project/async` SKIP-LOCKED cursors, `.values` for tuple bulk |
|  [02]   | `sql.insert(rows)` / `sql.update(row, omit?)` / `sql.updateValues(rows, alias)` / `.returning(cols)` | build DML     | `journal/append` insert + RETURNING; `updateValues` is pg-only (sqlite `updateValues: never`) |
|  [03]   | `sql.in(col, values)` / `sql.and(clauses)` / `sql.or(clauses)` / `sql.csv(prefix, values)` / `sql.join(lit)` | clause build | keyset/facet WHERE fragments in `retrieve/hybrid`; `csv` for ORDER BY / GROUP BY |
|  [04]   | `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` (all five arms REQUIRED, returns the selected arm) / `sql.onDialectOrElse({ orElse, sqlite?, pg?, mysql?, mssql?, clickhouse? })` (`orElse` required, each dialect arm optional) | dialect branch | the one seam a journal/projection row uses to emit pg vs sqlite SQL from a single definition; a `-pg`/`-sqlite` driver lane selects the matching arm — `sql.onDialect({ pg, sqlite })` arm-selection is the coherent form, never a `sql.pg`/`sql.sqlite` method |
|  [05]   | `sql.unsafe(text, params?)` / `sql.literal(text)` / `Statement.unsafeFragment(text, params?)`      | escape hatch   | provably-safe literal splice (extension DDL probes) where the fragment API cannot express the form |
|  [06]   | `Statement.compile(withoutTransform?)` → `[sql, params]`                                            | reflect        | `capability/row` probe compilation, telemetry span attributes, SafeQL static check |
|  [07]   | `Statement.setTransformer(f)` (Layer) / `withTransformer(f)` / `withTransformerDisabled`            | name transform | Layer-wide snake↔camel column mapping; disabled inside raw/driver-native passes |
|  [08]   | `SqlStream.asyncPauseResume(register, bufferSize?)` → `Stream<A, E, R>`                             | cursor stream  | the backpressured async-emit primitive a driver wraps its server-side cursor (`pg-cursor`) in to back `Statement.stream`; `register` exposes `single`/`chunk`/`array`/`fail`/`end` emit plus `onInterrupt`/`onPause`/`onResume` hooks so demand pauses the cursor |

[ENTRYPOINT_SCOPE]: transactions, savepoints, and reactive reads
- rail: rails-and-effects
- `withTransaction` wraps any `Effect` so every statement inside runs on one leased connection; nested calls become savepoints via `TransactionConnection` depth. `reactive`/`reactiveMailbox` (over `@effect/experimental` `Reactivity`) turn a query into a `Stream` that re-emits when its keys are invalidated — the `project/inline` read-your-writes signal.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `sql.withTransaction(effect)` → `Effect<A, E \| SqlError, R>`                                       | transaction    | `journal/append` — OCC append + `outbox` insert + idempotency-ledger claim commit atomically |
|  [02]   | `SqlClient.makeWithTransaction({ transactionTag, acquireConnection, begin, savepoint, commit, rollback, … })` | driver txn | a driver's transaction machinery; nested depth → savepoint/rollback-to-savepoint |
|  [03]   | `sql.reserve` → `Effect<Connection, SqlError, Scope>`                                               | lease conn     | scoped raw-connection lease for LISTEN/NOTIFY channels, COPY bulk, advisory-lock claims |
|  [04]   | `sql.reactive(keys, effect)` → `Stream<A, E, R>`                                                    | reactive read  | `project/inline` — a query stream re-run when `Reactivity.invalidate(keys)` fires after an append |
|  [05]   | `sql.reactiveMailbox(keys, effect)` → `Effect<ReadonlyMailbox<A, E>, never, R \| Scope>`            | reactive mailbox | pull-model reactive consumer for `browser/persist` read lanes |
|  [06]   | `SqlClient.make(options)` → `Effect<SqlClient, never, Reactivity>`                                  | assemble client | a driver builds the neutral client from `MakeOptions` (the driver `Layer`'s core) |

[ENTRYPOINT_SCOPE]: schema-typed query and batching resolver
- rail: rails-and-effects
- `SqlSchema.*` is the typed query: input `Schema` in, result `Schema` out, `ParseError` on the error channel. `SqlResolver.*` is the batched form over `RequestResolver` — the projection and retrieval read lanes collapse N+1 into one round-trip, ordered or grouped or by-id, with a populate/invalidate cache.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `SqlSchema.findAll({ Request, Result, execute })` / `findOne(…)` → `Option` / `single(…)` → `A \| NoSuchElement` / `void(…)` | typed query | projection reads decoding journal rows into `state` vocabulary; the one polymorphic query, arity in the constructor |
|  [02]   | `SqlResolver.ordered(tag, { Request, Result, execute })`                                            | batch resolver | 1:1 request→result, order-matched (`ResultLengthMismatch` guard) — `project/async` batched loads |
|  [03]   | `SqlResolver.grouped(tag, { Request, RequestGroupKey, Result, ResultGroupKey, execute })`           | batch resolver | 1:N grouped by extracted key — `retrieve/hybrid` fan-in of per-key result sets |
|  [04]   | `SqlResolver.findById(tag, { Id, Result, ResultId, execute })` → `Option`                           | batch resolver | id→`Option` DataLoader; `withContext: true` threads a `Schema` requirement into the batch |
|  [05]   | `resolver.execute(input)` / `.cachePopulate(id, result)` / `.cacheInvalidate(id)`                  | dispatch/cache | run a batched request; seed or evict the per-resolver cache on write |

[ENTRYPOINT_SCOPE]: the `Model` domain class and its repository/loader helpers
- rail: shapes
- `Model.Class<Self>(id)(fields)` is one declaration yielding `.select`/`.insert`/`.update`/`.json`/`.jsonCreate`/`.jsonUpdate` schemas. `makeRepository` and `makeDataLoaders` are the mutable-CRUD and batched-loader helpers — admissible for projection tables, the snapshot store, and the idempotency ledger, but NOT the record of truth (the append-only journal never issues UPDATE/DELETE on its events).

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `class Row extends Model.Class<Row>("Row")({ id: Model.Generated(Id), …, createdAt: Model.DateTimeInsert })` | declare model | one class → six wire variants; `journal` event rows, `snapshot` headers, projection rows, ledger rows |
|  [02]   | `Row.insert` / `Row.update` / `Row.json` / `Row.jsonCreate` / `Row.jsonUpdate` / `Model.fields(Row)` | variant access | the per-edge schema `SqlSchema`/`SqlResolver` decode/encode with; JSON variants for the `edge` wire |
|  [03]   | `Model.makeRepository(Row, { tableName, spanPrefix, idColumn })` → `{ insert, update, findById, delete, … }` | crud repo | projection-table / snapshot-store / idempotency-ledger CRUD; requires `SqlClient` — NOT the event journal |
|  [04]   | `Model.makeDataLoaders(Row, { tableName, spanPrefix, idColumn, window, maxBatchSize? })`            | batched loader | windowed `insert`/`findById`/`delete` DataLoaders over `SqlClient \| Scope` for read-heavy projection lanes |
|  [05]   | `Model.Class` variant builders `Field` / `FieldOnly` / `FieldExcept` / `Struct` / `Union` / `extract` | variant compose | build nested/renamed field sets; `Union` for polymorphic (multi-`_tag`) journal event families |

[ENTRYPOINT_SCOPE]: the `@effect/experimental` overlay-storage SQL bindings
- rail: rails-and-effects
- These bind the SQL spine under `@effect/experimental`'s overlay `Context.Tag`s (`.api/effect-experimental.md` `[R4]`/`[R19]`). The EventLog overlay's SERVER side and durable queue persist onto the same journal-owning `SqlClient` — the overlay accelerates local-first reads, the SQL journal stays the record of truth.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `SqlEventJournal.layer({ eventLogTable?, remotesTable? })` → `Layer<EventJournal, SqlError, SqlClient>` | journal store | binds `@effect/experimental` `EventJournal` to SQL — the durable-node local-first entry store |
|  [02]   | `SqlEventLogServer.layerStorage({ entryTablePrefix?, remoteIdTable?, insertBatchSize? })` → `Layer<EventLogServer.Storage, SqlError, SqlClient \| EventLogEncryption>` | sync server store | the E2E-encrypted EventLog sync server's SQL storage; `edge/live` mounts the handler |
|  [03]   | `SqlEventLogServer.layerStorageSubtle(options?)`                                                    | sync server store | Web-Crypto (`layerSubtle`) variant of the server storage — zero-knowledge server, no `EventLogEncryption` dep |
|  [04]   | `SqlPersistedQueue.layerStore({ tableName?, pollInterval?, lockRefreshInterval?, lockExpiration? })` → `Layer<PersistedQueueStore, SqlError, SqlClient>` | durable queue | `work` durable-job store — SKIP-LOCKED poll + lease refresh over the SQL spine |

## [04]-[IMPLEMENTATION_LAW]

[SQL_TOPOLOGY]:
- one Tag, driver-supplied Layer: `SqlClient` is an abstract `Context.Tag` with no binding. Every `store` row yields it (`const sql = yield* SqlClient`); the app root provides exactly one driver `Layer` (`PgClient.layer` durable, `SqliteClient.layer` node/bun, `SqliteClient.layer` wasm browser). Runtime portability is a `Layer` selection — the journal code never names a dialect. A new lane is one driver `Layer` plus, where SQL differs, one `sql.onDialect` arm; never a parallel journal, projection, or client family.
- the statement is the polymorphic surface: `sql\`…\`` returns a value that is a `Fragment`, an `Effect`, and `Pipeable` at once — you `yield*` it to get rows, read `.stream` for a backpressured cursor, `.values` for raw tuples, `.raw` for driver-native output, `.unprepared` to skip the prepared-statement cache, `.compile()` to reflect `[sql, params]`. There is no `query`/`queryOne`/`queryMany` proliferation: arity lives in the `SqlSchema` combinator (`findAll`/`findOne`/`single`), and dialect lives in `onDialect`.
- transactions nest as savepoints: `sql.withTransaction(effect)` leases one connection and runs every statement inside it; a nested `withTransaction` reads `TransactionConnection` depth and emits `SAVEPOINT`/`RELEASE`/`ROLLBACK TO`, so the OCC append + transactional outbox + idempotency-ledger claim commit atomically and a composed sub-operation rolls back to its savepoint without aborting the outer commit.
- parse-not-validate at both edges: `SqlSchema`/`SqlResolver`/`Model` decode every row through a `Schema` — the request schema validates input, the result schema decodes the untyped `Connection.Row`, and a decode miss is a `ParseError` on the `Effect` error channel. No untyped row reaches domain code; no exception carries a query failure (`SqlError` is a tagged `YieldableError`).
- migrations are banned: the `Migrator` family (`Migrator.make`, `fromGlob`/`fromBabelGlob`/`fromRecord`, and the driver `SqliteMigrator`/`PgMigrator` re-exports) exists in the package but is NOT admitted. `store` is no-migration by construction — the record of truth is the append-only journal, schema evolution is a read-time `journal/upcast` total fold, and DDL is idempotent declarative ensure split `iac` (apply at provision) ↔ `store` (verify at startup). Runtime never mutates schema.

[INTEGRATION_LAW]:
- Stack on `effect` (`.api/effect.md`): the statement is an `Effect`; `withTransaction` is an effect transformer; `SqlError` is a tagged member of the `Effect` error channel discriminated by `catchTag`. `Model` fields ARE `Schema` (`Generated`/`Sensitive`/`DateTimeInsert` compose `Schema.brand`/`optionalWith`/`transform`), and `SqlSchema`/`SqlResolver` lift `ParseResult.ParseError` into `E`. `sql.reactive` yields a `Stream`; `SqlResolver` composes `Request`/`RequestResolver` for batching. The SQL tier adds no new rail — it is `effect` applied to durable persistence.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): `SqlClient.reactive`/`reactiveMailbox` require the `Reactivity` service — `project/inline` emits `Reactivity.invalidate(keys)` after an OCC append and the reactive query re-runs (read-your-writes). `Model.Class` builds on `@effect/experimental` `VariantSchema`. `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` satisfy the overlay's `EventJournal`/`EventLogServer.Storage`/`PersistedQueueStore` Tags `[R4]`, so the SQL journal is the durable backing beneath the local-first overlay — never a second authority `[R19]`.
- Stack on `@effect/platform` (`.api/effect-platform.md`): the driver `layerConfig` reads its DSN/filename from `Config` behind `PlatformConfigProvider`; a `sql.reserve`d `Connection` frames LISTEN/NOTIFY and COPY over the platform `Socket`. The banned `SqliteMigrator`'s `FileSystem`/`Path`/`CommandExecutor` requirements are exactly why it stays out — `store` needs no filesystem-driven schema step.
- Stack across `store`: the driver Layer's `SqlClient` is the sole seam `journal`/`project`/`capability`/`scope`/`retrieve`/`object` share; `scope/tenant` sets the `app.current_tenant` GUC inside `withTransaction`; `capability/row` compiles fail-closed extension probes via `sql.unsafe`/`.compile()`; `work` and `security/session` never import `store` — they declare a `SqlClient`/journal port the `store` Layer satisfies at the app root.

[LOCAL_ADMISSION]:
- Use `SqlClient` (the neutral Tag) in every row; never import a driver package (`-pg`/`-sqlite-*`) inside `journal`/`project`/`capability`/`retrieve`/`scope` — drivers are admitted only at the app composition root and on the runtime-scoped `./server`/`./browser`/`./wasm` subpaths.
- Use the `sql` fragment DSL (`sql.insert`/`update`/`in`/`and`/`or`/`csv`/`join`) and `sql.onDialect` for dialect variance; never string-concatenate SQL or hand-write a per-dialect journal — `sql.unsafe`/`.literal` is the only escape and only for provably-safe literals.
- Use `SqlSchema`/`SqlResolver`/`Model` for typed I/O; never read a raw `Connection.Row` in domain code, never a `query`/`getById`/`getMany` family — one `SqlSchema` combinator discriminates arity.
- Use `sql.withTransaction` for the OCC-append + outbox + ledger atomic commit; never a manual `BEGIN`/`COMMIT` statement pair — nested composition must fold to savepoints.
- `Model.makeRepository`/`makeDataLoaders` are for projection/snapshot/ledger tables only; the append-only event journal never issues UPDATE/DELETE on its events (crypto-shredding is key destruction in `retain`, never a row rewrite).
- The `Migrator` family is banned branch-wide; a schema change is an `iac` declarative-ensure edit plus a `store` startup verify, never a migration script.

[RAIL_LAW]:
- Package: `@effect/sql`
- Owns: the `SqlClient` neutral `Context.Tag` + `Connection`/`TransactionConnection` spine, the `sql` fragment DSL (`Statement`/`Fragment`/`Segment`/`Compiler` + unsafe/literal/insert/update/updateValues/in/and/or/csv/join/onDialect/onDialectOrElse), the `SqlStream.asyncPauseResume` backpressured-cursor primitive behind `.stream`, `SqlSchema` typed queries, `SqlResolver` batching resolvers, the `SqlError`/`ResultLengthMismatch` fault rail, the `Model` variant-schema system (six wire variants + field families + `makeRepository`/`makeDataLoaders`), `reserve`/`reactive`/`reactiveMailbox`/`withTransaction`, and the `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` overlay-storage Layers
- Accept: one driver `Layer` per runtime behind the neutral `SqlClient` Tag, the `sql` DSL + `onDialect` for all query construction, `SqlSchema`/`SqlResolver`/`Model` for typed I/O, `withTransaction` for atomic commits, `reactive` for read-your-writes, the SQL overlay bindings as the durable backing under `@effect/experimental` `[R4]`/`[R19]`
- Reject: a driver import outside the composition root / runtime subpaths, string-built or per-dialect-forked SQL, untyped `Connection.Row` reads, `query`/`getById`/`getMany` operation families, manual `BEGIN`/`COMMIT`, `makeRepository` on the event journal, and any `Migrator` use (migrations are banned — DDL is `iac`↔`store` declarative ensure)
