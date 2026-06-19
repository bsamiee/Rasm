# [RASM_PERSISTENCE_API_NPGSQL_EF_NODATIME]

`Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` plugs NodaTime values into the
EF Core PostgreSQL provider, supplying type mappings, query translators,
aggregate translation, and SQL function projections.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- namespace: `Microsoft.EntityFrameworkCore`
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: plugin admission and services
- rail: store-provider

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------------------------------- |
|  [01]   | `NpgsqlNodaTimeDbContextOptionsBuilderExtensions` | builder extension  | admits plugin via `UseNodaTime(NpgsqlDbContextOptionsBuilder)` |
|  [02]   | `NpgsqlNodaTimeServiceCollectionExtensions`       | service extension  | `AddEntityFrameworkNpgsqlNodaTime`                             |
|  [03]   | `NpgsqlNodaTimeDbFunctionsExtensions`             | function surface   | projects NodaTime SQL aggregates and distance functions        |
|  [04]   | `NpgsqlNodaTimeOptionsExtension`                  | options extension  | carries plugin policy                                          |
|  [05]   | `NodaTimeDataSourceConfigurationPlugin`           | data source plugin | enables NodaTime wire                                          |
|  [06]   | `NpgsqlNodaTimeTypeMappingSourcePlugin`           | mapping plugin     | resolves type mappings                                         |
|  [07]   | `NpgsqlNodaTimeCodeGeneratorPlugin`               | scaffolding plugin | emits plugin admission                                         |
|  [08]   | `NpgsqlNodaTimeDesignTimeServices`                | design services    | admits design tooling                                          |

[MAPPING_TYPES]: NodaTime store mappings
- rail: temporal-values

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]   | [CAPABILITY]                    |
| :-----: | :--------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `TimestampTzInstantMapping`        | instant mapping  | maps `Instant` to `timestamptz` |
|  [02]   | `TimestampTzZonedDateTimeMapping`  | zoned mapping    | maps `ZonedDateTime`            |
|  [03]   | `TimestampTzOffsetDateTimeMapping` | offset mapping   | maps `OffsetDateTime`           |
|  [04]   | `TimestampLocalDateTimeMapping`    | local mapping    | maps `LocalDateTime`            |
|  [05]   | `LegacyTimestampInstantMapping`    | legacy mapping   | maps `Instant` to `timestamp`   |
|  [06]   | `DateMapping`                      | date mapping     | maps `LocalDate`                |
|  [07]   | `TimeMapping`                      | time mapping     | maps `LocalTime`                |
|  [08]   | `TimeTzMapping`                    | timetz mapping   | maps `OffsetTime`               |
|  [09]   | `DurationIntervalMapping`          | interval mapping | maps `Duration`                 |
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
|  [01]   | `NpgsqlNodaTimeMethodCallTranslatorPlugin`          | method plugin        | admits method translation    |
|  [02]   | `NpgsqlNodaTimeMethodCallTranslator`                | method translator    | translates method calls      |
|  [03]   | `NpgsqlNodaTimeMemberTranslatorPlugin`              | member plugin        | admits member translation    |
|  [04]   | `NpgsqlNodaTimeMemberTranslator`                    | member translator    | translates member access     |
|  [05]   | `NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin` | aggregate plugin     | admits aggregate translation |
|  [06]   | `NpgsqlNodaTimeAggregateMethodTranslator`           | aggregate translator | translates aggregates        |
|  [07]   | `NpgsqlNodaTimeEvaluatableExpressionFilterPlugin`   | evaluation filter    | keeps NodaTime in SQL        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin admission
- rail: store-provider

`UseNodaTime` is the `NpgsqlNodaTimeDbContextOptionsBuilderExtensions.UseNodaTime(NpgsqlDbContextOptionsBuilder)` provider-options entrypoint.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :--------------------------------- | :---------------- | :------------------------ |
|  [01]   | `UseNodaTime`                      | provider option   | maps NodaTime values      |
|  [02]   | `AddEntityFrameworkNpgsqlNodaTime` | service extension | registers plugin services |

[ENTRYPOINT_SCOPE]: SQL function projections
- rail: temporal-values

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]            | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------- | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `Sum(IEnumerable<Period>)`                       | `DbFunctions` extension | sums `Period` values                                                |
|  [02]   | `Sum(IEnumerable<Duration>)`                     | `DbFunctions` extension | sums `Duration` values                                              |
|  [03]   | `Average(IEnumerable<Period>)`                   | `DbFunctions` extension | averages `Period` values                                            |
|  [04]   | `Average(IEnumerable<Duration>)`                 | `DbFunctions` extension | averages `Duration` values                                          |
|  [05]   | `Distance(Instant, Instant)`                     | `DbFunctions` extension | integer distance between instants                                   |
|  [06]   | `Distance(ZonedDateTime, ZonedDateTime)`         | `DbFunctions` extension | integer distance between zoned datetimes                            |
|  [07]   | `Distance(LocalDateTime, LocalDateTime)`         | `DbFunctions` extension | integer distance between local datetimes                            |
|  [08]   | `Distance(LocalDate, LocalDate)`                 | `DbFunctions` extension | integer distance between dates                                      |
|  [09]   | `RangeAgg(IEnumerable<Interval>)`                | `DbFunctions` extension | aggregates `Interval` values to `Interval[]`                        |
|  [10]   | `RangeAgg(IEnumerable<DateInterval>)`            | `DbFunctions` extension | aggregates `DateInterval` values to `DateInterval[]`                |
|  [11]   | `RangeIntersectAgg(IEnumerable<Interval>)`       | `DbFunctions` extension | intersects `Interval` sequence to scalar `Interval`                 |
|  [12]   | `RangeIntersectAgg(IEnumerable<DateInterval>)`   | `DbFunctions` extension | intersects `DateInterval` sequence to scalar `DateInterval`         |
|  [13]   | `RangeIntersectAgg(IEnumerable<Interval[]>)`     | `DbFunctions` extension | intersects multirange `Interval[]` sequence to `Interval[]`         |
|  [14]   | `RangeIntersectAgg(IEnumerable<DateInterval[]>)` | `DbFunctions` extension | intersects multirange `DateInterval[]` sequence to `DateInterval[]` |

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the NodaTime plugin is temporal mapping policy for the PostgreSQL store profile
- admission root: `UseNodaTime` on `NpgsqlDbContextOptionsBuilder` — single method, no overloads
- mapping root: `timestamptz`, `date`, `time`, `timetz`, `interval`, range, and multirange mappings
- query root: NodaTime method, member, and aggregate translation

[LOCAL_ADMISSION]:
- NodaTime mapping enters only through the PostgreSQL store-profile declaration.
- Persisted time semantics use NodaTime types per the temporal-values rail.
- Range and multirange mappings are profile metadata, not public service families.
- SQL function projections are query facts and stay inside profile queries.
- `Distance` overloads return `int` (not `TimeSpan`) for all NodaTime types — this differs from the BCL `DbFunctions` extension which returns `TimeSpan` for `DateTime`.
- All four `RangeIntersectAgg` overloads are decompile-verified: scalar `Interval`/`DateInterval` and multirange `Interval[]`/`DateInterval[]` inputs.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- Owns: NodaTime mapping for the PostgreSQL EF provider
- Accept: profile-declared NodaTime mapping
- Reject: BCL time semantics in PostgreSQL store contracts
