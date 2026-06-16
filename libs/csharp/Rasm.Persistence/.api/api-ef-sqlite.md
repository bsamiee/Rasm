# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` admits the EF Core SQLite provider,
SQLite options, relational services, SQL generation, migrations, scaffolding,
query translation, and type mapping into one store-provider rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Sqlite`
- package: `Microsoft.EntityFrameworkCore.Sqlite`
- assembly: `Microsoft.EntityFrameworkCore.Sqlite`
- namespace: `Microsoft.EntityFrameworkCore`
- asset: provider admission and runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider options and admission
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :----------------- | :----------------------- |
|   [1]   | `SqliteDbContextOptionsBuilder`           | provider options   | configures SQLite store  |
|   [2]   | `SqliteDbContextOptionsBuilderExtensions` | builder extension  | admits SQLite provider   |
|   [3]   | `SqliteServiceCollectionExtensions`       | service extension  | admits provider services |
|   [4]   | `SqliteOptionsExtension`                  | options extension  | carries provider policy  |
|   [5]   | `SqliteDatabaseFacadeExtensions`          | database extension | exposes SQLite database  |
|   [6]   | `SqliteDesignTimeServices`                | design services    | admits design tooling    |

[RELATIONAL_TYPES]: mapping, query, and migration surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------- | :------------------ | :---------------------- |
|   [1]   | `SqliteTypeMappingSource`      | type mapper         | maps CLR values         |
|   [2]   | `SqliteSqlGenerationHelper`    | SQL helper          | emits SQL identifiers   |
|   [3]   | `SqliteQuerySqlGenerator`      | query generator     | emits query SQL         |
|   [4]   | `SqliteMigrationsSqlGenerator` | migration generator | emits migration SQL     |
|   [5]   | `SqliteUpdateSqlGenerator`     | update generator    | emits update SQL        |
|   [6]   | `SqliteDatabaseCreator`        | database creator    | creates store database  |
|   [7]   | `SqliteModelValidator`         | model validator     | validates store model   |
|   [8]   | `SqliteMigrationDatabaseLock`  | migration lock      | protects migrations     |
|   [9]   | `SqliteAnnotationNames`        | annotation names    | names provider metadata |

[TRANSLATION_TYPES]: query translation surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------------------- | :------------------ | :---------------------- |
|   [1]   | `SqliteMethodCallTranslatorProvider`       | translator provider | translates methods      |
|   [2]   | `SqliteMemberTranslatorProvider`           | translator provider | translates members      |
|   [3]   | `SqliteDateTimeMethodTranslator`           | translator          | translates date methods |
|   [4]   | `SqliteStringMethodTranslator`             | translator          | translates strings      |
|   [5]   | `SqliteMathTranslator`                     | translator          | translates math calls   |
|   [6]   | `SqliteRegexMethodTranslator`              | translator          | translates regex calls  |
|   [7]   | `SqliteQueryableAggregateMethodTranslator` | translator          | translates aggregates   |

[INTERCEPTION_TYPES]: EF Core interception surfaces (`Microsoft.EntityFrameworkCore.Diagnostics`, base assembly)
- rail: store-provider

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]   | [CAPABILITY]                     |
| :-----: | :---------------------------- | :--------------- | :------------------------------- |
|   [1]   | `ConnectionOpenedAsync`       | connection hook  | re-applies PRAGMA ladder         |
|   [2]   | `ReaderExecutedAsync`         | command hook     | emits slow and burst facts       |
|   [3]   | `SavingChangesAsync`          | save hook        | stamps changefeed in transaction |
|   [4]   | `SavedChangesAsync`           | save hook        | invalidates and emits save fact  |
|   [5]   | `TransactionCommittedAsync`   | transaction hook | emits transaction facts          |
|   [6]   | `CommandExecutedEventData`    | event data       | carries slow-query duration      |
|   [7]   | `DbContextEventData`          | event data       | carries context access           |
|   [8]   | `ConnectionEndEventData`      | event data       | carries reopen duration          |
|   [9]   | `TransactionEndEventData`     | event data       | carries commit duration          |
|  [10]   | `InterceptionResult<TResult>` | result struct    | round-trips suppressed results   |

```csharp generated
Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken)
ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken)
ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken)
ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken)
```

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]       | [CAPABILITY]            |
| :-----: | :------------------------- | :----------------- | :---------------------- |
|   [1]   | `UseSqlite`                | builder extension  | applies provider policy |
|   [2]   | `AddSqlite<TContext>`      | service extension  | registers context       |
|   [3]   | `AddEntityFrameworkSqlite` | service extension  | registers EF services   |
|   [4]   | `IsSqlite`                 | database extension | identifies provider     |
|   [5]   | `MigrationsAssembly`       | provider option    | selects migration owner |
|   [6]   | `CommandTimeout`           | provider option    | sets command timeout    |
|   [7]   | `MinBatchSize`             | provider option    | sets batch lower bound  |
|   [8]   | `MaxBatchSize`             | provider option    | sets batch upper bound  |

[ENTRYPOINT_SCOPE]: migration and model operations
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]        | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :------------------ | :----------------------------- |
|   [1]   | `SqliteMigrationBuilderExtensions` | migration extension | identifies provider builder    |
|   [2]   | `SqliteTableExtensions`            | metadata extension  | configures tables              |
|   [3]   | `SqliteEntityTypeExtensions`       | metadata extension  | configures entities            |
|   [4]   | `SqliteValueGenerationStrategy`    | metadata value      | classifies generation          |
|   [5]   | `ConfigureDesignTimeServices`      | service hook        | registers design services      |
|   [6]   | `EF.CompileAsyncQuery`             | compiled shape      | caches hot projection delegate |

`EF.CompileAsyncQuery<TContext,TResult>` returns `Func<TContext,IAsyncEnumerable<TResult>>`; parameterized overloads extend through `TParam15`.

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: SQLite is one admitted store profile
- provider root: `UseSqlite`
- model root: EF relational model plus SQLite annotations
- migration root: SQLite SQL generator and migration lock
- query root: SQLite method, member, aggregate, and JSON translation

[LOCAL_ADMISSION]:
- SQLite enters through the unified store-profile algebra.
- Provider-specific options stay profile data and never become public service families.
- Migration, model, query, and update behavior share the same provider rail.
- SQLite cannot define Persistence vocabulary by itself.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Sqlite`
- Owns: EF SQLite provider admission
- Accept: SQLite store profile
- Reject: SQLite-first public services
