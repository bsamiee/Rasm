# [RASM_PERSISTENCE_API_NPGSQL_EF_NODATIME]

`Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` maps NodaTime CLR values onto PostgreSQL temporal, range, and multirange store types for the EF Core Npgsql provider, and translates NodaTime method, member, and aggregate calls to SQL that never client-evaluates. Every value enters through one `UseNodaTime()` call on the provider options builder, feeding the store-provider rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` (`PostgreSQL` license)
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- namespace: `Microsoft.EntityFrameworkCore` public extensions; mappings, translators, and plugins under `Npgsql.EntityFrameworkCore.PostgreSQL.*.Internal`
- depends: `Npgsql.EntityFrameworkCore.PostgreSQL` base provider; ADO codecs via transitive `Npgsql.NodaTime`
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PLUGIN_TYPES]: plugin admission and services

| [INDEX] | [SYMBOL]                                          | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `NpgsqlNodaTimeDbContextOptionsBuilderExtensions` | admits the plugin via `UseNodaTime`              |
|  [02]   | `NpgsqlNodaTimeServiceCollectionExtensions`       | registers plugin services under explicit DI      |
|  [03]   | `NpgsqlNodaTimeDbFunctionsExtensions`             | projects NodaTime SQL aggregate and distance ops |
|  [04]   | `NpgsqlNodaTimeOptionsExtension`                  | carries plugin policy on the options graph       |
|  [05]   | `NodaTimeDataSourceConfigurationPlugin`           | enables the NodaTime ADO wire                    |
|  [06]   | `NpgsqlNodaTimeTypeMappingSourcePlugin`           | resolves NodaTime store mappings                 |
|  [07]   | `NpgsqlNodaTimeCodeGeneratorPlugin`               | emits plugin admission when scaffolding          |
|  [08]   | `NpgsqlNodaTimeDesignTimeServices`                | admits design-time tooling                       |

[MAPPING_TYPES]: NodaTime CLR-to-store mappings

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :----------------------------------- |
|  [01]   | `TimestampTzInstantMapping`        | `Instant` -> `timestamptz`           |
|  [02]   | `TimestampTzZonedDateTimeMapping`  | `ZonedDateTime` -> `timestamptz`     |
|  [03]   | `TimestampTzOffsetDateTimeMapping` | `OffsetDateTime` -> `timestamptz`    |
|  [04]   | `TimestampLocalDateTimeMapping`    | `LocalDateTime` -> `timestamp`       |
|  [05]   | `LegacyTimestampInstantMapping`    | `Instant` -> `timestamp`             |
|  [06]   | `DateMapping`                      | `LocalDate` -> `date`                |
|  [07]   | `TimeMapping`                      | `LocalTime` -> `time`                |
|  [08]   | `TimeTzMapping`                    | `OffsetTime` -> `timetz`             |
|  [09]   | `DurationIntervalMapping`          | `Duration` -> `interval`             |
|  [10]   | `PeriodIntervalMapping`            | `Period` -> `interval`               |
|  [11]   | `IntervalRangeMapping`             | `Interval` -> `tstzrange`            |
|  [12]   | `IntervalMultirangeMapping`        | `Interval[]` -> `tstzmultirange`     |
|  [13]   | `DateIntervalRangeMapping`         | `DateInterval` -> `daterange`        |
|  [14]   | `DateIntervalMultirangeMapping`    | `DateInterval[]` -> `datemultirange` |
|  [15]   | `DateTimeZoneMapping`              | `DateTimeZone` -> zone-id text       |

[TRANSLATOR_TYPES]: query translation plugins

| [INDEX] | [SYMBOL]                                            | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------- | :-------------------------------- |
|  [01]   | `NpgsqlNodaTimeMethodCallTranslatorPlugin`          | admits method-call translation    |
|  [02]   | `NpgsqlNodaTimeMethodCallTranslator`                | translates NodaTime method calls  |
|  [03]   | `NpgsqlNodaTimeMemberTranslatorPlugin`              | admits member-access translation  |
|  [04]   | `NpgsqlNodaTimeMemberTranslator`                    | translates NodaTime member access |
|  [05]   | `NpgsqlNodaTimeAggregateMethodCallTranslatorPlugin` | admits aggregate translation      |
|  [06]   | `NpgsqlNodaTimeAggregateMethodTranslator`           | translates NodaTime aggregates    |
|  [07]   | `NpgsqlNodaTimeEvaluatableExpressionFilterPlugin`   | pins NodaTime calls into SQL      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin admission

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `UseNodaTime(NpgsqlDbContextOptionsBuilder)`           | admits the plugin; returns the builder for chaining |
|  [02]   | `AddEntityFrameworkNpgsqlNodaTime(IServiceCollection)` | registers plugin services under explicit DI         |

[ENTRYPOINT_SCOPE]: SQL function projections

Every projection is a `public static` `DbFunctions` extension legal only inside an EF query tree; `Sum` and `Average` return nullable.

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `Period? Sum(IEnumerable<Period>)`               | sums `Period` values                                                |
|  [02]   | `Duration? Sum(IEnumerable<Duration>)`           | sums `Duration` values                                              |
|  [03]   | `Period? Average(IEnumerable<Period>)`           | averages `Period` values                                            |
|  [04]   | `Duration? Average(IEnumerable<Duration>)`       | averages `Duration` values                                          |
|  [05]   | `Distance(Instant, Instant)`                     | integer distance between instants                                   |
|  [06]   | `Distance(ZonedDateTime, ZonedDateTime)`         | integer distance between zoned datetimes                            |
|  [07]   | `Distance(LocalDateTime, LocalDateTime)`         | integer distance between local datetimes                            |
|  [08]   | `Distance(LocalDate, LocalDate)`                 | integer distance between dates                                      |
|  [09]   | `RangeAgg(IEnumerable<Interval>)`                | aggregates `Interval` values to `Interval[]`                        |
|  [10]   | `RangeAgg(IEnumerable<DateInterval>)`            | aggregates `DateInterval` values to `DateInterval[]`                |
|  [11]   | `RangeIntersectAgg(IEnumerable<Interval>)`       | intersects `Interval` sequence to scalar `Interval`                 |
|  [12]   | `RangeIntersectAgg(IEnumerable<DateInterval>)`   | intersects `DateInterval` sequence to scalar `DateInterval`         |
|  [13]   | `RangeIntersectAgg(IEnumerable<Interval[]>)`     | intersects multirange `Interval[]` sequence to `Interval[]`         |
|  [14]   | `RangeIntersectAgg(IEnumerable<DateInterval[]>)` | intersects multirange `DateInterval[]` sequence to `DateInterval[]` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- NodaTime mapping is temporal store policy on the PostgreSQL provider profile; a value enters through `UseNodaTime()` and lands as the store type its mapping owns.
- Method, member, and aggregate translation with the evaluatable-expression filter pin NodaTime calls into SQL, never client-evaluated.

[STACKING]:
- `Npgsql.EntityFrameworkCore.PostgreSQL`(`.api/api-npgsql-ef.md`): `UseNodaTime(NpgsqlDbContextOptionsBuilder)` chains onto the `UseNpgsql` options builder and returns it, stacking beside `UseNetTopologySuite` (`.api/api-nts-ef.md`) and the Pgvector resolver (`.api/api-pgvector-ef.md`) on one builder; `AddEntityFrameworkNpgsqlNodaTime(IServiceCollection)` is the explicit-DI mirror.
- `NodaTime`(`libs/csharp/.api/api-nodatime.md`): the mapped CLR vocabulary is the `api-nodatime` clock model, and `api-nodatime-stj`(`libs/csharp/.api/api-nodatime-stj.md`) carries the same values across the System.Text.Json wire, so a `timestamptz` column and a serialized snapshot share one clock and never diverge.
- `Element/identity` validity and audit columns bind `Instant` -> `timestamptz` and `Interval` -> `tstzrange`; `RangeAgg`/`RangeIntersectAgg` over `Interval`/`DateInterval` build the set algebra `Version/timetravel` AS-OF and `Version/retention` window queries read, and `Distance`/`Sum`/`Average` over durations feed the retention aggregates, all inside EF query trees.

[LOCAL_ADMISSION]:
- NodaTime mapping enters only through the PostgreSQL store-profile declaration; persisted time semantics carry NodaTime types.
- Mapping, translator, scaffolding, and options types are internal EF service registrations; the consumer surface is `UseNodaTime`, `AddEntityFrameworkNpgsqlNodaTime`, and the `DbFunctions` projections.
- `Distance` returns `int` for all four overloads — the PostgreSQL `<->` operator difference, never a `Duration` or `TimeSpan`.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- Owns: NodaTime mapping and translation for the PostgreSQL EF provider
- Accept: profile-declared NodaTime mapping
- Reject: BCL time semantics in PostgreSQL store contracts
