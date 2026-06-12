# [RASM_PERSISTENCE_API_NPGSQL_EF_NODATIME]

`Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` plugs NodaTime values into the
EF Core PostgreSQL provider, supplying type mappings, query translators,
aggregate translation, and SQL function projections.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- namespace: `Microsoft.EntityFrameworkCore`
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: plugin admission and services
- rail: store-provider

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]           |
| :-----: | :------------------------------------------------ | :----------------- | :--------------------- |
|   [1]   | `NpgsqlNodaTimeDbContextOptionsBuilderExtensions` | builder extension  | admits plugin          |
|   [2]   | `NpgsqlNodaTimeServiceCollectionExtensions`       | service extension  | admits plugin services |
|   [3]   | `NpgsqlNodaTimeDbFunctionsExtensions`             | function surface   | projects SQL functions |
|   [4]   | `NpgsqlNodaTimeOptionsExtension`                  | options extension  | carries plugin policy  |
|   [5]   | `NodaTimeDataSourceConfigurationPlugin`           | data source plugin | enables NodaTime wire  |
|   [6]   | `NpgsqlNodaTimeTypeMappingSourcePlugin`           | mapping plugin     | resolves type mappings |
|   [7]   | `NpgsqlNodaTimeCodeGeneratorPlugin`               | scaffolding plugin | emits plugin admission |
|   [8]   | `NpgsqlNodaTimeDesignTimeServices`                | design services    | admits design tooling  |

[MAPPING_TYPES]: NodaTime store mappings
- rail: temporal-values

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]   | [CAPABILITY]                    |
| :-----: | :--------------------------------- | :--------------- | :------------------------------ |
|   [1]   | `TimestampTzInstantMapping`        | instant mapping  | maps `Instant` to timestamptz   |
|   [2]   | `TimestampTzZonedDateTimeMapping`  | zoned mapping    | maps `ZonedDateTime`            |
|   [3]   | `TimestampTzOffsetDateTimeMapping` | offset mapping   | maps `OffsetDateTime`           |
|   [4]   | `TimestampLocalDateTimeMapping`    | local mapping    | maps `LocalDateTime`            |
|   [5]   | `LegacyTimestampInstantMapping`    | legacy mapping   | maps `Instant` to timestamp     |
|   [6]   | `DateMapping`                      | date mapping     | maps `LocalDate`                |
|   [7]   | `TimeMapping`                      | time mapping     | maps `LocalTime`                |
|   [8]   | `TimeTzMapping`                    | timetz mapping   | maps `OffsetTime`               |
|   [9]   | `DurationIntervalMapping`          | interval mapping | maps `Duration`                 |
|  [10]   | `PeriodIntervalMapping`            | interval mapping | maps `Period`                   |
|  [11]   | `IntervalRangeMapping`             | range mapping    | maps `Interval` ranges          |
|  [12]   | `IntervalMultirangeMapping`        | multirange map   | maps `Interval` multiranges     |
|  [13]   | `DateIntervalRangeMapping`         | range mapping    | maps `DateInterval` ranges      |
|  [14]   | `DateIntervalMultirangeMapping`    | multirange map   | maps `DateInterval` multiranges |
|  [15]   | `DateTimeZoneMapping`              | zone mapping     | maps `DateTimeZone`             |

[TRANSLATOR_TYPES]: query translation plugins
- rail: store-provider

| [INDEX] | [SYMBOL]                                            | [PACKAGE_ROLE]       | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------- | :------------------- | :--------------------------- |
|   [1]   | `NpgsqlNodaTimeMethodCallTranslatorPlugin`          | method plugin        | admits method translation    |
|   [2]   | `NpgsqlNodaTimeMethodCallTranslator`                | method translator    | translates method calls      |
|   [3]   | `NpgsqlNodaTimeMemberTranslatorPlugin`              | member plugin        | admits member translation    |
|   [4]   | `NpgsqlNodaTimeMemberTranslator`                    | member translator    | translates member access     |
|   [5]   | `NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin` | aggregate plugin     | admits aggregate translation |
|   [6]   | `NpgsqlNodaTimeAggregateMethodTranslator`           | aggregate translator | translates aggregates        |
|   [7]   | `NpgsqlNodaTimeEvaluatableExpressionFilterPlugin`   | evaluation filter    | keeps NodaTime in SQL        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin admission
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :--------------------------------- | :---------------- | :------------------------ |
|   [1]   | `UseNodaTime`                      | provider option   | maps NodaTime values      |
|   [2]   | `AddEntityFrameworkNpgsqlNodaTime` | service extension | registers plugin services |

[ENTRYPOINT_SCOPE]: SQL function projections
- rail: temporal-values

| [INDEX] | [SURFACE]           | [CALL_SHAPE]            | [CAPABILITY]                   |
| :-----: | :------------------ | :---------------------- | :----------------------------- |
|   [1]   | `Sum`               | `DbFunctions` extension | sums periods and durations     |
|   [2]   | `Average`           | `DbFunctions` extension | averages periods and durations |
|   [3]   | `Distance`          | `DbFunctions` extension | measures temporal distance     |
|   [4]   | `RangeAgg`          | `DbFunctions` extension | aggregates ranges              |
|   [5]   | `RangeIntersectAgg` | `DbFunctions` extension | intersects ranges              |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the NodaTime plugin is temporal mapping policy for the PostgreSQL store profile
- admission root: `UseNodaTime` on the PostgreSQL provider options
- mapping root: timestamptz, date, time, interval, range, and multirange mappings
- query root: NodaTime method, member, and aggregate translation

[LOCAL_ADMISSION]:
- NodaTime mapping enters only through the PostgreSQL store-profile declaration.
- Persisted time semantics use NodaTime types per the temporal-values rail.
- Range and multirange mappings are profile metadata, not public service families.
- SQL function projections are query facts and stay inside profile queries.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- Owns: NodaTime mapping for the PostgreSQL EF provider
- Accept: profile-declared NodaTime mapping
- Reject: BCL time semantics in PostgreSQL store contracts
