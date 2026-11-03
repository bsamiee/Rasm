# [SQL]

## [01]-[STATEMENTS]

`` sql`...` `` returns a `Statement<A>` that is both an `Effect` of rows and a fragment, and statements nest unexecuted. Interpolated values bind as parameters:
- `sql(name)` escapes an identifier split on each dot, and `sql.literal(text)` inserts raw SQL where a statement binds no parameter (PostgreSQL `COMMENT ON`, `SET LOCAL`, `DO`)
- `Statement.fragment`, `parameter`, `identifier`, and `csv` build constant fragments at module scope without a client
- `sql.insert(rows).returning('*')`, `sql.update(row, omit)`, `sql.in(values)`, `sql.and(parts)`, and `sql.csv(parts)` build clauses, and `sql.insert` takes a non-empty list
- Lists bind as one parameter, `= any(${list})` or `jsonb_array_elements(${json})` on PostgreSQL and `json_each(${json})` on SQLite, and D1 refuses over 100 parameters per query
- `sql.onDialect({ pg, sqlite, mysql, mssql, clickhouse })` takes an arm per dialect, `sql.onDialectOrElse({ orElse, ...arms })` the arms that differ
- `sql.withoutTransforms()` returns the client without `transformQueryNames` and `transformResultNames`, for statements over names a catalog read returns
- Statements run later stay `Statement` values, `.stream` returns rows as a `Stream`, `.values` positional rows, and `.compile(withoutTransform)` the SQL text with its parameters
- `sql.reactive(keys, query)` reruns a query as a `Stream` each time `keys` invalidate
- `SqlClient.SafeIntegers` set `true` returns 64-bit integers as `bigint`

## [02]-[QUERIES]

`SqlSchema` builds a function from a request schema to decoded rows, encoding the request and decoding each result:
- `findAll`, `findNonEmpty`, `findOne`, `findOneOption`, and `void` state row count, `findOne` failing with `NoSuchElementError` where `findOneOption` returns `Option`
- `SqlSchema` functions build at module scope with an `execute` reading `SqlClient.SqlClient.use`, and `Effect.fn` takes one as its body
- `Schema.Tuple([row])` decoded and mapped through `([first]) => first` restates `findOne`
- `Schema.BooleanFromBit` decodes the `0` or `1` SQLite returns for a boolean expression
- `SqlModel.makeRepository(Model, { tableName, spanPrefix, idColumn })` derives `insert`, `update`, `findById`, and `delete` from a `Model.Class`, and `softDeleteColumn` filters reads and turns delete into a timestamp
- `SqlModel.makeResolvers` returns the same operations as batched resolvers
- `SqlResolver` batches key on the caller's transaction, and `SqlResolver.ordered` fails a batch with a row count differing from its request count

## [03]-[TRANSACTIONS]

`sql.withTransaction` runs an effect in one transaction, a nested call in a savepoint, and the outermost call owns the retry of a contended write:

```ts
const rebalance = (base: Duration.Input, retries: number) =>
    SqlClient.SqlClient.use((sql) =>
        Effect.all([release, claim]).pipe(
            sql.withTransaction,
            Effect.provideService(SqlClient.BeginTransaction, 'BEGIN ISOLATION LEVEL SERIALIZABLE'),
            Effect.retry({ schedule: Schedule.exponential(base).pipe(Schedule.jittered), times: retries, while: (error) => SqlError.isSqlError(error) && (error.reason._tag === 'SerializationError' || error.reason._tag === 'DeadlockError') }),
        ),
    );
```

`rebalance(base, retries)` runs writes `release` and `claim` that move a shard lease between nodes in one serializable transaction, and reruns both after a conflict the database rolled back:
- `SqlError.reason` is a tagged union, and `Effect.catchReason('SqlError', 'UniqueViolation', f)` recovers one
- `D1Client` fails every statement with an `UnknownError` reason, where SQLite and PostgreSQL drivers classify each failure
- `isRetryable` on a `SqlError` holds for connection, deadlock, serialization, lock, and statement timeouts, and a connection lost during `COMMIT` leaves its outcome unknown
- Writes guarded by stored state hold the guard: `INSERT ... SELECT ... WHERE revision = ${expected}` for a revision, `DO UPDATE ... WHERE body IS DISTINCT FROM excluded.body` to skip an unchanged row
- `SqlClient.BeginTransaction` replaces `BEGIN` for each top-level transaction it covers
- `sql.afterCommit(effect)` runs after the outermost commit, discarded on rollback and lost on process exit, and work that must happen goes in a table the transaction writes
- `Reactivity.invalidate` inside a transaction defers to commit when it reads the transaction's `Reactivity`

## [04]-[DRIVERS]

Driver layers provide the driver tag and `SqlClient` with their own `Reactivity`, and programs read `SqlClient`:
- `PgClient.layerConfig`, `D1Client.layer`, and `SqliteClient.layer` build a client, and code reads the driver tag for driver members alone (`listen`, `batch`)
- `PgClient` takes `prepare: false` behind a transaction-mode pooler, `multiplex: true` to pipeline fibers on a pooled connection, and `password` as an effect read per connection
- `PgClient` takes `types` from a `PgTypes.makeRegistry()` its layer builds, where `PgTypes.register` writes a process-wide registry at module load
- `PgClient.listen(channel)` returns a scoped `Queue` of notifications, and `notify(channel, payload)` sends one
- `D1Client` runs no transactions, and `D1Client.batch([a, b])` writes statements atomically with a tuple typed per statement
- `D1Client.batch` lists end in a `SELECT` of the written state, read through `Array.lastNonEmpty`
- `sql.onDialect` takes the `sqlite` arm on D1 and SQLite alike, and `Effect.serviceOption(D1Client.D1Client)` selects `batch` or `sql.withTransaction`
- Live schema reads `pragma_table_list`, `pragma_table_xinfo`, and `pragma_index_list` on SQLite, `information_schema` and `pg_catalog` on PostgreSQL
