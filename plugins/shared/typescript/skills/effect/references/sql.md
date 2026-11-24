# [SQL]

## [01]-[STATEMENTS]

`` sql`...` `` returns a `Statement<A>` that is both an `Effect` of rows and a fragment, and a statement run later stays a value or nests unexecuted. Interpolated values bind as parameters, and fragments and helper segments splice in. `SqlSchema` wraps one statement in a function from a request schema to decoded rows:

```ts
const byKinds = SqlSchema.findAll({
    Request: Schema.Struct({ owner: Schema.String, kinds: Schema.Array(Schema.String) }),
    Result: Note,
    execute: ({ owner, kinds }) => SqlClient.SqlClient.use((sql) => sql`SELECT * FROM ${sql('notes')} WHERE ${sql.and([sql`owner = ${owner}`, sql.in('kind', kinds)])}`),
});
```

`Note` is the row schema. `execute` receives the encoded request, and `byKinds` builds once at module scope, an `Effect.fn` taking it as its body. The text a segment produces selects its helper:

| [INDEX] | [SEGMENT]                     | [HELPER]                                                                                          |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Identifier                    | `sql(name)`, `transformQueryNames` applied, then split on each dot                                |
|  [02]   | Raw text, no parameter        | `sql.literal(text)`                                                                               |
|  [03]   | Raw statement with parameters | `sql.unsafe(text, params)`                                                                        |
|  [04]   | List membership               | `sql.in(column, values)`, an empty list compiling to `1=0`                                        |
|  [05]   | Rows to insert or update      | `sql.insert(rows).returning('*')`, `sql.update(row, omit)`                                        |
|  [06]   | Many rows updated by key      | `sql.updateValues(rows, alias)`, refused on SQLite and D1                                         |
|  [07]   | Joined clauses                | `sql.and`, `sql.or` (empty `1=1`), `sql.csv`, `sql.join`                                          |
|  [08]   | Dialect branch                | `sql.onDialect({ pg, sqlite, mysql, mssql, clickhouse })`, `onDialectOrElse({ orElse, ...arms })` |
|  [09]   | Segment with no client        | `Statement.fragment([Statement.identifier(name)])`, `Statement.parameter`, `Statement.csv`        |

PostgreSQL binds a JavaScript array as one typed array (`= any(${list})`), its elements sharing one inferred type. The member a statement ends in selects what it returns:
- `.values` returns positional rows, and `.raw` the driver result without name transforms
- `.unprepared` skips the prepared cache, and `.compile()` returns SQL text and parameters
- `.stream` returns rows as a `Stream` on PostgreSQL and ClickHouse, and dies on `sqlite-node` and D1
- `sql.withoutTransforms()` returns the client without `transformQueryNames` and `transformResultNames`, for statements over catalog names
- `sql.reactive(keys, query)` reruns an effect as a `Stream` each time `keys` invalidate, `sql.reactiveMailbox` as a scoped queue
- `Effect.provideService(SqlClient.SafeIntegers, true)` returns 64-bit integers as `bigint` on `sqlite-node`
- PostgreSQL `int8` decodes to `bigint` unless a `types` registry overrides it

## [02]-[QUERIES]

Rows a statement returns select the `SqlSchema` member:

| [INDEX] | [MEMBER]                  | [ROWS]                                                      |
| :-----: | :------------------------ | :---------------------------------------------------------- |
|  [01]   | `SqlSchema.findAll`       | Any count                                                   |
|  [02]   | `SqlSchema.findNonEmpty`  | At least one, `NoSuchElementError` on none                  |
|  [03]   | `SqlSchema.findOne`       | First row, later rows ignored, `NoSuchElementError` on none |
|  [04]   | `SqlSchema.findOneOption` | First row as an `Option`                                    |
|  [05]   | `SqlSchema.void`          | None read, no `Result` schema                               |

Statements returning rows on every run (an aggregate, `INSERT ... RETURNING` of non-empty rows) turn `NoSuchElementError` into `Effect.die`, and `Schema.fromJsonString(Schema.Array(Row))` in a request field hands `execute` the JSON text a list binds. Repositories and resolvers derive from schemas:
- `SqlModel.makeRepository(Model, { tableName, spanPrefix, idColumn })` derives `insert`, `update`, `findById`, `delete`, and `*Void` writes
- Repository `insert` and `update` die on no returned row
- `softDeleteColumn` filters repository reads and updates and turns `delete` into a timestamp write
- `SqlModel.makeResolvers` returns batched `insert`, `insertVoid`, `findById`, and `delete` resolvers, requiring `SqlClient` and `Scope`
- `SqlResolver` batches key on the caller's transaction alone, run with the first caller's services, and can join callers of different tenants
- `SqlResolver.ordered` fails a batch whose row count differs from its request count, `grouped` returns many rows per key, and `void` batches writes

## [03]-[TRANSACTIONS]

`sql.withTransaction` runs an effect in one transaction and a nested call in a savepoint, and the outermost call owns the retry of a contended write, here on PostgreSQL:

```ts
const rebalance = (base: Duration.Input, retries: number) =>
    SqlClient.SqlClient.use((sql) =>
        Effect.all([release, claim]).pipe(
            sql.withTransaction,
            Effect.provideService(SqlClient.BeginTransaction, 'BEGIN ISOLATION LEVEL SERIALIZABLE'),
            Effect.retry({ schedule: Schedule.exponential(base).pipe(Schedule.jittered), times: retries, while: (error) => error.reason._tag === 'SerializationError' || error.reason._tag === 'DeadlockError' }),
        ),
    );
```

`release` and `claim` are writes failing with `SqlError`, and each retry reruns both and their `COMMIT` after the database rolled the transaction back:
- `SqlError` reasons include `UniqueViolation`, and `isRetryable` holds for connection, deadlock, serialization, lock, and statement timeouts
- PostgreSQL classifies failures by SQLSTATE, and D1 fails every statement with `UnknownError`
- SQLite reports a busy database as `LockTimeoutError` alone
- `ConnectionError` during `COMMIT` leaves the outcome unknown, a retry around it repeating a write that committed
- `SqlClient.BeginTransaction` replaces `BEGIN` for each top-level transaction it covers, `BEGIN IMMEDIATE` on `sqlite-node` included
- Fibers forked inside a transaction fail once it ends, and a fiber outliving it forks under `Effect.provideContext` without `sql.transactionService`
- `sql.afterCommit(effect)` runs after the outermost commit, at once outside a transaction, and never on rollback or process exit
- `Reactivity.invalidate` inside a transaction defers to commit when it reads the transaction's `Reactivity`
- `sql.reserve` returns one scoped `Connection`, its `executeUnprepared(text, params, undefined)` running each statement on it
- Statements written `` sql`...` `` take a pooled connection

## [04]-[DRIVERS]

Driver layers provide their driver service and `SqlClient` with their own `Reactivity`, each `layerConfig` taking `Config` values, and programs read a driver service for its own members:

| [INDEX] | [DRIVER]                          | [TRANSACTIONS]               | [OWN_MEMBERS]                                      |
| :-----: | :-------------------------------- | :--------------------------- | :------------------------------------------------- |
|  [01]   | `PgClient` (`pg`)                 | `BEGIN`, savepoints          | `listen`, `notify`, `json`, `poolStats`            |
|  [02]   | `SqliteClient` (`sqlite-node`)    | `BEGIN IMMEDIATE` for writes | `backup`, `loadExtension`                          |
|  [03]   | `D1Client` (`d1`)                 | Die, `batch` atomic          | `batch`                                            |
|  [04]   | `ClickhouseClient` (`clickhouse`) | `BEGIN TRANSACTION`          | `param`, `asCommand`, `insertQuery`, `withQueryId` |

`sql.driver` names the row's driver, and `sql.onDialect` takes the `sqlite` arm on D1 and SQLite alike:
- `PgClient` pools through `minConnections`, `maxConnections` (10), `idleTimeout` (60 seconds), `connectionTTL`, and `acquireTimeout`
- `PgClient` reads `password` as an effect per connection, and takes `prepare: false` behind a transaction-mode pooler
- `multiplex: true` pipelines statements on a pooled connection while transactions, streams, and listeners reserve one
- Session state set on each connection goes in `startupParameters`, where `onAcquire` runs per checkout with no connection
- `PgClient` option `types` takes a `PgTypes.makeRegistry()` holding built-in codecs alone
- Clients given `types` never read the module registry `PgTypes.register` writes
- `registry.register(oid, { decode, encode }, { arrayOid })` registers binary codecs returning `Result`, `arrayOid` registering the array codec
- `PgTypes.decode(bytes, oid, 1)` decodes binary input, a `jsonb` codec keeping text passing `bytes.subarray(1)` as `OID.text`
- `pg.json(value)` binds an object as `jsonb` with keys through `transformQueryNames` unless `transformJson: false`
- `pg.listen(channel)` returns a scoped `Queue` of notifications
- `d1.batch([a, b])` writes statements atomically, typed per statement
- `Effect.serviceOption(D1Client.D1Client)` selects `batch` or `sql.withTransaction`
- `SqliteClient` `busyTimeout` (5 seconds) waits on the write lock and blocks the event loop meanwhile
- `ClickhouseClient` references `ClientMethod`, `QueryId`, and `ClickhouseSettings` set per statement through `Effect.provideService`
