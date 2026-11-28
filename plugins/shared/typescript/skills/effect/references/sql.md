# [SQL]

## [01]-[STATEMENTS]

`` sql`...` `` returns a `Statement<A>` that is both an `Effect` of rows and a fragment, so a statement stays a value until run and nests inside another unexecuted. Each interpolated value binds as one parameter, a JavaScript array included, and fragments and helper segments splice in. Clauses a condition adds join by conditional spread, as a lone clause compiles without parentheses and the plain predicate keeps its index, here the membership test `Relation.select` builds per column on SQLite:

```ts
const matched = sql.or([
    sql`${column} IN (SELECT e.value FROM json_each(${body}) e)`,
    ...(nullable ? [sql`(${column} IS NULL AND EXISTS (SELECT 1 FROM json_each(${body}) e WHERE e.value IS NULL))`] : []),
]);
```

`body` is the JSON text of the requested values, and an `IS NULL` branch on a key column turns the primary key search into a scan. The text a segment produces selects its helper:

| [INDEX] | [SEGMENT]                     | [HELPER]                                                                                          |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | Identifier                    | `sql(name)`, `transformQueryNames` applied, then split on each dot                                |
|  [02]   | Raw text, no parameter        | `sql.literal(text)`                                                                               |
|  [03]   | Raw statement with parameters | `sql.unsafe(text, params)`                                                                        |
|  [04]   | List bound once               | JSON text read through `json_each(${text})` on SQLite, `= any(${list})` on PostgreSQL             |
|  [05]   | List bound per element        | `sql.in(column, values)`, an empty list compiling to `1=0`                                        |
|  [06]   | Rows to insert or update      | `sql.insert(rows).returning('*')`, `sql.update(row, omit)`                                        |
|  [07]   | Many rows updated by key      | `sql.updateValues(rows, alias)`, refused on SQLite and D1                                         |
|  [08]   | Joined clauses                | `sql.and`, `sql.or`, `sql.csv`, `sql.join(separator, parentheses, fallback)`                      |
|  [09]   | Dialect branch                | `sql.onDialect({ pg, sqlite, mysql, mssql, clickhouse })`, `onDialectOrElse({ orElse, ...arms })` |
|  [10]   | Segment with no client        | `Statement.fragment([Statement.parameter(path)])`, `Statement.identifier`, `Statement.csv`        |

Helpers compile by fixed rules a statement relies on:
- `sql.in` writes one placeholder per element, so each list length prepares its own statement, where row `[04]` keeps one statement text
- `sql.and` and `sql.or` compile an empty list to `1=1`, so an empty `sql.or` matches every row, and an empty `sql.csv` compiles to nothing
- PostgreSQL binds a JavaScript array as one typed array, its elements sharing one inferred type
- Statements derive values (`substr`, `instr`, a `MAX(...) FILTER (WHERE ...)` pivot) and filter in `WHERE`, never in a pass over rows

Statements return what their last member selects:
- `.values` returns positional rows, and `.raw` the driver result (`{ changes, lastInsertRowid }` for a write on `sqlite-node`)
- `.unprepared` skips the prepared cache, and `.compile(true)` returns SQL text and parameters without name transforms
- `.stream` returns rows as a `Stream` on PostgreSQL and ClickHouse, and dies on `sqlite-node` and D1
- `sql.withoutTransforms()` returns the client without `transformQueryNames` and `transformResultNames`, for statements over catalog names
- `sql.reactive(keys, query)` reruns an effect as a `Stream` each time `keys` invalidate, `sql.reactiveMailbox` as a scoped queue
- `Effect.provideService(SqlClient.SafeIntegers, true)` returns 64-bit integers as `bigint` on `sqlite-node`
- PostgreSQL `int8` decodes to `bigint` unless a `types` registry overrides it

## [02]-[QUERIES]

`SqlSchema` members encode a request through `Request`, hand `execute` its encoded form, and decode rows through `Result`, and the rows a statement returns select the member:

| [INDEX] | [MEMBER]                  | [ROWS]                                                      |
| :-----: | :------------------------ | :---------------------------------------------------------- |
|  [01]   | `SqlSchema.findAll`       | Any count                                                   |
|  [02]   | `SqlSchema.findNonEmpty`  | At least one, `NoSuchElementError` on none                  |
|  [03]   | `SqlSchema.findOne`       | First row, later rows ignored, `NoSuchElementError` on none |
|  [04]   | `SqlSchema.findOneOption` | First row as an `Option`                                    |
|  [05]   | `SqlSchema.void`          | None read, no `Result` schema                               |

Finders build once at module scope, and an `Effect.fn` takes one as its body with each later step joined through `Function.flow`, as a trailing `Effect.fn` transform infers no error channel from the finder:

```ts
const readDocument = Effect.fn('readDocument')(
    Function.flow(
        SqlSchema.findOne({
            Request: documents.schema.fields.state,
            Result: Draft,
            execute: (state) => SqlClient.SqlClient.use((sql) => sql`SELECT COALESCE(d.revision, 0) AS revision, COALESCE(d.body, ${initial}) AS data FROM (SELECT ${state} AS state) s LEFT JOIN documents d ON d.state = s.state`),
        }),
        Effect.catchTag('NoSuchElementError', Effect.die),
        Effect.catchTag(['SqlError', 'SchemaError'], unavailable),
    ),
);
```

`documents` is the table's `Relation`, its fields typing `Request`, `Draft` the row schema, `initial` the encoded empty document, and `unavailable` maps a failure to `ServiceUnavailable`. Rows cross the finder by these rules:
- Statements returning rows on every run (a `LEFT JOIN` from one value row, an insert returning rows) turn `NoSuchElementError` into `Effect.die`
- `RETURNING` names the columns a caller reads, and a write no caller reads runs through `SqlSchema.void`, as finders decode every returned row
- `Row.pipe(Schema.Array, Schema.fromJsonString)` in a request field hands `execute` the JSON text a list binds

Finders decode rows as one array, so a column holding a stored document catches its own decoding failure, and one stale row leaves every other row readable:

```ts
const containers = Relation.make('containers', { bundle: Schema.String, language: Schema.String, locator: Locator, body: Stored(Entry) }, { key: ['bundle', 'language', 'locator'], withoutRowid: true });
```

`Stored` wraps a document schema in the absent-document `Schema.catchDecoding` form of typescript:coding, `Entry` is the document schema, and `Locator` its key. Tables a program owns are `Relation` declarations of `@rasm/database`, reconciled to the live database with no migration files, each deriving its statements from its schema:
- `Relation.make(name, fields, options)` keys on `id` unless `key` names columns, and `options` declare indexes, references, and full-text search
- `Relation.insert` and `Relation.ensure` die on no returned row, `ensure` returning the stored row for each key already present
- `Relation.upsert(relation, rows, ...kept)` returns `{ row, inserted }` for each offered row it inserted or changed, reading back key columns alone, and never updates `kept` columns
- Upserts refuse rows sharing a key through the request schema and write nothing for rows equal to the stored ones
- `Relation.select(relation, where, { order, limit, per })` filters each column by a value list or `{ not }`, `per` limiting rows per value
- `Relation.remove` returns the rows it deleted, and `Relation.search` ranks full-text matches with optional highlight marks
- `Relation.withTenant` runs an effect in a transaction holding the tenant setting PostgreSQL row policies read
- `projections` declare a column read from a stored document (`{ from: 'body', path: ['objectKey'] }`) or a condition (`{ where }`)
- Projected columns are generated `VIRTUAL` columns that writes omit and indexes and `WHERE` read, never a second written copy

Resolvers batch concurrent requests into one statement:
- `SqlResolver` batches key on the caller's transaction alone and run with the first caller's services
- Callers of different tenants share a batch outside `Relation.withTenant`, as only its transaction separates them
- `SqlResolver.ordered` fails a batch whose row count differs from its request count, `grouped` returns many rows per key, `void` batches writes

## [03]-[TRANSACTIONS]

`sql.withTransaction` runs an effect in one transaction and a nested call in a savepoint, and the engine layer supplies atomicity where a driver has none: `Rows.atomic(statements)` runs `sql.withTransaction(Effect.all(statements))` on SQLite and PostgreSQL and `D1Client.batch` on D1. Transactions behave by these rules:
- `sqlite-node` begins writable transactions with `BEGIN IMMEDIATE`, each one taking the write lock, reads included
- Stores a program only reads open with `readonly: true`, which begins with `BEGIN` and skips WAL
- `SqlClient.BeginTransaction` replaces `BEGIN` for each top-level transaction it covers, provided around the one `withTransaction` it serves
- `SqlError` reasons include `UniqueViolation` and `ConstraintError`, and `isRetryable` holds for connection, deadlock, serialization, and timeouts
- PostgreSQL classifies failures by SQLSTATE, SQLite by result code (busy and locked as `LockTimeoutError`), and D1 as `UnknownError`
- Retries wrap the outermost `withTransaction`, each attempt rerunning the effect and its `COMMIT`, and a nested retry repeats its savepoint alone
- `ConnectionError` during `COMMIT` leaves the outcome unknown, and a retry around it repeats a write that committed
- `sql.afterCommit(effect)` runs after the outermost commit, at once outside a transaction, and never on rollback or process exit
- `Reactivity.invalidate` inside a transaction defers to commit when it reads the transaction's `Reactivity`
- Fibers outliving their transaction fail each statement, and work meant outside it runs under `Context.omit(sql.transactionService)`

`sql.reserve` returns one scoped `Connection` that runs each statement through `executeUnprepared`, where `` sql`...` `` takes a pooled connection. Locks spanning a transaction are session locks on a reserved connection, polled outside any transaction, as a transaction-scoped lock holds a snapshot a peer's `CREATE INDEX CONCURRENTLY` waits on:

```ts
const exclusive = <A, E, R>(sql: SqlClient.SqlClient, effect: Effect.Effect<A, E, R>) =>
    Effect.scoped(
        Effect.flatMap(sql.reserve, (connection) => {
            const run = (statement: Statement.Statement<object>) => connection.executeUnprepared(...statement.compile(true), undefined);
            const lock = sql`SELECT 1 WHERE pg_try_advisory_lock(hashtextextended(current_schema(), 0))`;
            const held = Effect.acquireRelease(Effect.repeat(run(lock), { until: Array.isReadonlyArrayNonEmpty, schedule: Schedule.exponential(Duration.millis(1)) }), () =>
                Effect.orDie(run(sql`SELECT pg_advisory_unlock(hashtextextended(current_schema(), 0))`)),
            );
            return Effect.andThen(held, effect);
        }),
    );
```

Statements deferred past their plan run on that connection only while the observation they were planned under holds, `Effect.when(run(statement), Effect.map(Effect.forEach(observed, run), Array.every(Array.isReadonlyArrayNonEmpty)))`.

## [04]-[DRIVERS]

Driver layers provide their driver service and `SqlClient` with their own `Reactivity`, each `layerConfig` taking `Config` values, and programs read a driver service for its own members:

| [INDEX] | [DRIVER]                          | [TRANSACTIONS]               | [OWN_MEMBERS]                                      |
| :-----: | :-------------------------------- | :--------------------------- | :------------------------------------------------- |
|  [01]   | `PgClient` (`pg`)                 | `BEGIN`, savepoints          | `listen`, `notify`, `json`, `poolStats`            |
|  [02]   | `SqliteClient` (`sqlite-node`)    | `BEGIN IMMEDIATE` for writes | `backup`, `loadExtension`                          |
|  [03]   | `D1Client` (`d1`)                 | Die, `batch` atomic          | `batch`                                            |
|  [04]   | `ClickhouseClient` (`clickhouse`) | `BEGIN TRANSACTION`          | `param`, `asCommand`, `insertQuery`, `withQueryId` |

`sql.driver` names the row's driver, and `sql.onDialect` takes the `sqlite` arm on D1 and SQLite alike. Programs owning tables reach a driver through its `@rasm/database` engine layer (`sqliteNodeEngine`, `d1Engine`, `Pg.layer`, `Clickhouse.layer`), which adds `Engine` and `Rows` and sets `Engine.names.snake` name transforms unless given `Engine.names.verbatim`. Drivers differ by these rules:
- `SqliteClient` serializes one `node:sqlite` connection, turns on WAL unless `disableWAL` or `readonly`, and caches 200 prepared statements
- `SqliteClient` `busyTimeout` (5 seconds) waits on the write lock and blocks the event loop meanwhile
- `sqliteNodeEngine` of `@rasm/database/sqlite-node` creates its file's folder through `FileSystem` unless `readonly` or `:memory:`
- `PgClient` pools through `minConnections`, `maxConnections` (10), `idleTimeout` (60 seconds), `connectionTTL`, and `acquireTimeout`
- `PgClient` reads `password` as an effect per connection, and takes `prepare: false` behind a transaction-mode pooler
- `multiplex: true` pipelines statements on a pooled connection while transactions, streams, and listeners reserve one
- Session state set on each connection goes in `startupParameters`, where `onAcquire` runs per checkout with no connection
- `PgClient` option `types` takes a `PgTypes.makeRegistry()`, which starts from the built-in codecs and never reads `PgTypes.register`
- `registry.register(oid, { decode, encode }, { arrayOid })` registers binary codecs returning `Result`, `arrayOid` registering the array codec
- `PgTypes.decode(bytes, oid, 1)` decodes binary input, a `jsonb` codec keeping text passing `bytes.subarray(1)` as `OID.text`
- `pg.json(value)` binds an object as `jsonb` with keys through `transformQueryNames` unless `transformJson: false`
- `pg.listen(channel)` returns a scoped `Queue` of notifications
- `d1.batch([a, b])` writes statements atomically, typed per statement
- `ClickhouseClient` reads `ClientMethod`, `QueryId`, and `ClickhouseSettings` per statement, `Effect.updateService` editing settings for one effect

## [05]-[RECONCILIATION]

`Catalog.layer(catalog, engine, migrator)` reconciles the declared relations to the live database before the layer completes, and logs the undeclared indexes and references it leaves in place:

```ts
const stored = Catalog.layer(Catalog.make({ tables: [applications, containers] }), sqliteNodeEngine({ filename }));
```

`applications` and `containers` are `Relation` values and `filename` the store's path. Plans change a live object only as its declaration states, and refuse every other difference with a `ReconcileError` naming table, columns, and reason:

| [INDEX] | [CHANGE]                             | [DECLARATION]                                                   |
| :-----: | :----------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Column renamed                       | `renamed: { field: 'stored' }`                                  |
|  [02]   | Table renamed                        | `renamedFrom: 'stored'`                                         |
|  [03]   | Column type, nullability, or default | `retype: ['field']`                                             |
|  [04]   | Column dropped                       | `retired: ['column']`                                           |
|  [05]   | Index dropped                        | `retiredIndexes: [['column', ...]]`, name parts after the table |
|  [06]   | Table dropped                        | `Catalog.make({ tables, retired: ['table'] })`                  |

Reconciliation runs by these rules:
- Refusals read `Retype` for a changed definition `retype` lacks, `Required` for a required column without a default on a populated table
- `RetainedReference` refuses a kept table referencing a dropped one, and `Unsupported` a declaration the engine cannot hold
- Retired names stay listed, each listing counting one generation, so a name declared after its retirement creates a new object
- PostgreSQL retypes through `ALTER COLUMN ... TYPE ... USING`, a cast changing a stored value of a non-text column raising `check_violation`
- SQLite rebuilds a table it cannot alter in place (a retype, a `withoutRowid` or reference change, a table not `STRICT`) and each table referencing it
- SQLite rebuilds and drops run under `PRAGMA defer_foreign_keys = on`, and `Catalog.layer` then runs the engine's `open` (`synchronous = NORMAL`, `optimize`)
- Engines without transactions (D1, ClickHouse) re-plan after a failed apply and retry while the remaining statements shrink to a strict subset
- `migrator` reconciles through its own client and grants the runtime client's role (`current_user` on PostgreSQL), `Catalog.make({ reader })` a read role
- `Catalog.reconcile` applies the plan and returns undeclared indexes and references, and `Catalog.plan` returns its statements unapplied
