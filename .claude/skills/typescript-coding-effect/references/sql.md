# [SQL]

## [01]-[STATEMENTS]

`` sql`...` `` returns a `Statement<A>` that is both an `Effect` of rows and a fragment, and statements nest unexecuted. Interpolated values bind as parameters:
- `sql(name)` escapes an identifier split on each dot, `sql.literal(text)` and a string `csv` part insert raw SQL, and `sql.unsafe` runs raw text
- `sql.insert(rows).returning('*')`, `sql.update(row, omit)`, `sql.in(values)`, `sql.and(parts)`, and `sql.csv(parts)` build clauses, and `sql.insert` takes a non-empty list
- `sql.onDialect({ pg, sqlite, mysql, mssql, clickhouse })` fails in an unsupported dialect's arm, `sql.driver` naming drivers that share one
- `.stream` returns rows as a `Stream`, `.values` positional rows, and `.compile()` the SQL text with its parameters
- `sql.reactive(keys, query)` reruns a query as a `Stream` each time `keys` invalidate
- `SqlClient.SafeIntegers` set `true` returns 64-bit integers as `bigint`

## [02]-[QUERIES]

`SqlSchema` builds a function from a request schema to decoded rows, encoding the request and decoding each result:
- `findAll`, `findNonEmpty`, `findOne`, `findOneOption`, and `void` state row count, a one-row tuple restating `findOne` and its `NoSuchElementError`
- `SqlModel.makeRepository(Model, { tableName, spanPrefix, idColumn })` derives `insert`, `update`, `findById`, and `delete` from a `Model.Class`, and `softDeleteColumn` filters reads and turns delete into a timestamp
- `SqlModel.makeResolvers` returns the same operations as batched resolvers
- `SqlResolver` batches key on the caller's transaction, and `SqlResolver.ordered` fails a batch with a row count differing from its request count

## [03]-[TRANSACTIONS]

`sql.withTransaction` runs an effect in one transaction, a nested call in a savepoint, and the outermost call owns the retry of a contended write:

```ts
const serializable = <A, E, R>(work: Effect.Effect<A, E, R>) =>
    SqlClient.SqlClient.use((sql) =>
        work.pipe(sql.withTransaction, Effect.provideService(SqlClient.BeginTransaction, 'BEGIN ISOLATION LEVEL SERIALIZABLE'), Effect.retry({ schedule: Schedule.exponential('10 millis').pipe(Schedule.jittered), times: 5, while: (error) => SqlError.isSqlError(error) && (error.reason._tag === 'SerializationError' || error.reason._tag === 'DeadlockError') })),
    );
```

`serializable(Effect.all([debit, credit]))` runs both writes in one serializable transaction and reruns the unit after a conflict the database rolled back:
- `SqlError.reason` is a tagged union (`UniqueViolation`, `ConstraintError`, `SerializationError`, `DeadlockError`, `ConnectionError`), and `Effect.catchReason('SqlError', 'UniqueViolation', f)` recovers one
- `isRetryable` on a `SqlError` holds for connection, deadlock, serialization, lock, and statement timeouts, and a connection lost during `COMMIT` leaves its outcome unknown
- `SqlClient.BeginTransaction` replaces `BEGIN` for each top-level transaction it covers
- `sql.afterCommit(effect)` runs after the outermost commit, discarded on rollback and lost on process exit, and work that must happen goes in a table the transaction writes
- `Reactivity.invalidate` inside a transaction defers to commit when it reads the transaction's `Reactivity`

## [04]-[DRIVERS]

Driver layers provide the driver tag and `SqlClient` with their own `Reactivity`, and programs read `SqlClient`:
- `PgClient.layerConfig`, `D1Client.layer`, and `SqliteClient.layer` build a client, and code reads the driver tag for driver members alone (`listen`, `batch`)
- `PgClient` takes `prepare: false` behind a transaction-mode pooler, `multiplex: true` to pipeline fibers on a pooled connection, and `password` as an effect read per connection
- `PgClient.listen(channel)` returns a scoped `Queue` of notifications, and `notify(channel, payload)` sends one
- `D1Client` runs no transactions, and `D1Client.batch([a, b])` writes statements atomically with a tuple typed per statement
- `Migrator.fromRecord({ '0001_name': effect })` and `MigratorFileSystem.fromFileSystem(dir)` load migrations a driver migrator records
