# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` admits the EF Core SQLite provider,
SQLite options, relational services, SQL generation, migrations, scaffolding,
query translation, and type mapping into one store-provider rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Sqlite`
- package: `Microsoft.EntityFrameworkCore.Sqlite`
- assembly: `Microsoft.EntityFrameworkCore.Sqlite`
- namespace: `Microsoft.EntityFrameworkCore`
- asset: provider admission and runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider options and admission
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :----------------- | :----------------------- |
|  [01]   | `SqliteDbContextOptionsBuilder`           | provider options   | configures SQLite store  |
|  [02]   | `SqliteDbContextOptionsBuilderExtensions` | builder extension  | admits SQLite provider   |
|  [03]   | `SqliteServiceCollectionExtensions`       | service extension  | admits provider services |
|  [04]   | `SqliteOptionsExtension`                  | options extension  | carries provider policy  |
|  [05]   | `SqliteDatabaseFacadeExtensions`          | database extension | exposes SQLite database  |
|  [06]   | `SqliteDesignTimeServices`                | design services    | admits design tooling    |

[RELATIONAL_TYPES]: mapping, query, and migration surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------- | :------------------ | :---------------------- |
|  [01]   | `SqliteTypeMappingSource`      | type mapper         | maps CLR values         |
|  [02]   | `SqliteSqlGenerationHelper`    | SQL helper          | emits SQL identifiers   |
|  [03]   | `SqliteQuerySqlGenerator`      | query generator     | emits query SQL         |
|  [04]   | `SqliteMigrationsSqlGenerator` | migration generator | emits migration SQL     |
|  [05]   | `SqliteUpdateSqlGenerator`     | update generator    | emits update SQL        |
|  [06]   | `SqliteDatabaseCreator`        | database creator    | creates store database  |
|  [07]   | `SqliteModelValidator`         | model validator     | validates store model   |
|  [08]   | `SqliteMigrationDatabaseLock`  | migration lock      | protects migrations     |
|  [09]   | `SqliteAnnotationNames`        | annotation names    | names provider metadata |

[TRANSLATION_TYPES]: query translation surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------------------- | :------------------ | :---------------------- |
|  [01]   | `SqliteMethodCallTranslatorProvider`       | translator provider | translates methods      |
|  [02]   | `SqliteMemberTranslatorProvider`           | translator provider | translates members      |
|  [03]   | `SqliteDateTimeMethodTranslator`           | translator          | translates date methods |
|  [04]   | `SqliteStringMethodTranslator`             | translator          | translates strings      |
|  [05]   | `SqliteMathTranslator`                     | translator          | translates math calls   |
|  [06]   | `SqliteRegexMethodTranslator`              | translator          | translates regex calls  |
|  [07]   | `SqliteQueryableAggregateMethodTranslator` | translator          | translates aggregates   |

[INTERCEPTION_TYPES]: EF Core interception surfaces (`Microsoft.EntityFrameworkCore.Diagnostics`, base assembly)
- rail: store-provider

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]   | [CAPABILITY]                     |
| :-----: | :---------------------------- | :--------------- | :------------------------------- |
|  [01]   | `ConnectionOpenedAsync`       | connection hook  | re-applies PRAGMA ladder         |
|  [02]   | `ReaderExecutedAsync`         | command hook     | emits slow and burst facts       |
|  [03]   | `SavingChangesAsync`          | save hook        | stamps changefeed in transaction |
|  [04]   | `SavedChangesAsync`           | save hook        | invalidates and emits save fact  |
|  [05]   | `TransactionCommittedAsync`   | transaction hook | emits transaction facts          |
|  [06]   | `CommandExecutedEventData`    | event data       | carries slow-query duration      |
|  [07]   | `DbContextEventData`          | event data       | carries context access           |
|  [08]   | `ConnectionEndEventData`      | event data       | carries reopen duration          |
|  [09]   | `TransactionEndEventData`     | event data       | carries commit duration          |
|  [10]   | `InterceptionResult<TResult>` | result struct    | round-trips suppressed results   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]       | [CAPABILITY]            |
| :-----: | :------------------------- | :----------------- | :---------------------- |
|  [01]   | `UseSqlite`                | builder extension  | applies provider policy |
|  [02]   | `AddSqlite<TContext>`      | service extension  | registers context       |
|  [03]   | `AddEntityFrameworkSqlite` | service extension  | registers EF services   |
|  [04]   | `IsSqlite`                 | database extension | identifies provider     |
|  [05]   | `MigrationsAssembly`       | provider option    | selects migration owner |
|  [06]   | `CommandTimeout`           | provider option    | sets command timeout    |
|  [07]   | `MinBatchSize`             | provider option    | sets batch lower bound  |
|  [08]   | `MaxBatchSize`             | provider option    | sets batch upper bound  |

[ENTRYPOINT_SCOPE]: migration and model operations
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]        | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :------------------ | :----------------------------- |
|  [01]   | `SqliteMigrationBuilderExtensions` | migration extension | identifies provider builder    |
|  [02]   | `SqliteTableExtensions`            | metadata extension  | configures tables              |
|  [03]   | `SqliteEntityTypeExtensions`       | metadata extension  | configures entities            |
|  [04]   | `SqliteValueGenerationStrategy`    | metadata value      | classifies generation          |
|  [05]   | `ConfigureDesignTimeServices`      | service hook        | registers design services      |
|  [06]   | `EF.CompileAsyncQuery`             | compiled shape      | caches hot projection delegate |

`EF.CompileAsyncQuery<TContext,TResult>` returns `Func<TContext,IAsyncEnumerable<TResult>>`; parameterized overloads extend through `TParam15`.

## [04]-[IMPLEMENTATION_LAW]

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
