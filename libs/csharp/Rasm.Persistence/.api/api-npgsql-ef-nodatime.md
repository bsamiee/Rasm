# [RASM_PERSISTENCE_API_NPGSQL_EF_NODATIME]

`Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` plugs NodaTime values into the EF Core PostgreSQL
provider, supplying `timestamptz`/`date`/`time`/`timetz`/`interval`/range/multirange type mappings,
method and member query translators, aggregate translation, an evaluatable-expression filter that
keeps NodaTime calls in SQL, and the `DbFunctions` SQL function projections (`Sum`/`Average` over
durations, `Distance`, `RangeAgg`, `RangeIntersectAgg`). It stacks onto `api-npgsql-ef`: the single
`UseNodaTime()` call registers the plugin alongside `UseNetTopologySuite()` (`api-nts-ef`) and the
Pgvector resolver (`api-pgvector-ef`) on one `NpgsqlDbContextOptionsBuilder`, and the NodaTime CLR
types it maps are the same ones `api-nodatime` and `api-nodatime-stj` carry through the receipt and
wire seams — so a persisted `Instant`/`Period`/`Interval` round-trips one canonical clock vocabulary
from C# domain to `timestamptz` and back.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- version: `10.0.2`
- license: `PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` (`lib/net10.0`, the consumer-bound TFM)
- namespace: `Microsoft.EntityFrameworkCore` (public extensions); plugin internals under `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime.*.Internal`
- provider package: `Npgsql.EntityFrameworkCore.PostgreSQL` (`api-npgsql-ef`); ADO codecs via transitive `Npgsql.NodaTime`
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

All projections are `public static` extensions on `DbFunctions` (the `this DbFunctions _` receiver),
usable only inside an EF query tree; `Sum`/`Average` return nullable (`Period?`/`Duration?`).

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]            | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------- | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `Period? Sum(IEnumerable<Period>)`               | `DbFunctions` extension | sums `Period` values                                                |
|  [02]   | `Duration? Sum(IEnumerable<Duration>)`           | `DbFunctions` extension | sums `Duration` values                                              |
|  [03]   | `Period? Average(IEnumerable<Period>)`           | `DbFunctions` extension | averages `Period` values                                            |
|  [04]   | `Duration? Average(IEnumerable<Duration>)`       | `DbFunctions` extension | averages `Duration` values                                          |
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
- admission root: `UseNodaTime(this NpgsqlDbContextOptionsBuilder)` — single method, no overloads
- mapping root: `timestamptz`, `date`, `time`, `timetz`, `interval`, range, and multirange mappings
- query root: NodaTime method, member, and aggregate translation; the evaluatable-expression filter keeps NodaTime calls in SQL rather than client-evaluating them

[INTEGRATION_STACK]:
- `UseNodaTime()` chains onto the `api-npgsql-ef` `UseNpgsql(...)` options alongside `UseNetTopologySuite()` (`api-nts-ef`) and the Pgvector resolver (`api-pgvector-ef`) — three plugins on one `NpgsqlDbContextOptionsBuilder`, each contributing a `TypeMappingSourcePlugin` and translator plugins through the EF service graph; `AddEntityFrameworkNpgsqlNodaTime(this IServiceCollection)` is the explicit-DI mirror when the provider is registered manually
- the mapped CLR types are the `api-nodatime` vocabulary; `api-nodatime-stj` (`ConfigureForNodaTime`/`WithIsoIntervalConverter`) carries the same `Instant`/`Interval`/`Period` across the System.Text.Json wire, so a value persisted to `timestamptz` and a value serialized into a snapshot frame share one clock model and never diverge
- the `DbFunctions` SQL projections are the query-side surface the design composes: `Distance`/`Sum`/`Average` over durations feed retention/window aggregates, and `RangeAgg`/`RangeIntersectAgg` over `Interval`/`DateInterval` build the validity-window set algebra the temporal/time-travel queries read — these stay inside EF query trees and never client-evaluate
- the `LegacyTimestampInstantMapping` (`Instant` → bare `timestamp`) exists for legacy columns only; the profile binds `Instant` → `timestamptz` through `TimestampTzInstantMapping`, so a wall-clock-without-zone column is the deleted shape

[LOCAL_ADMISSION]:
- NodaTime mapping enters only through the PostgreSQL store-profile declaration; persisted time semantics use NodaTime types per the temporal-values rail
- range and multirange mappings are profile metadata, not public service families; the translator/scaffolding/options plugin types are internal EF service registrations, not a consumer surface
- SQL function projections are query facts and stay inside profile queries
- `Distance` overloads return `int` for all four NodaTime types (`Instant`/`ZonedDateTime`/`LocalDateTime`/`LocalDate`) — the PostgreSQL `<->` operator difference, never a `TimeSpan` or `Duration`; a consumer that expects a duration-typed distance from a NodaTime store contract is the named defect
- all four `RangeIntersectAgg` overloads are decompile-verified: scalar `Interval`/`DateInterval` (returns the scalar) and multirange `Interval[]`/`DateInterval[]` (returns the array)

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- Owns: NodaTime mapping for the PostgreSQL EF provider
- Accept: profile-declared NodaTime mapping
- Reject: BCL time semantics in PostgreSQL store contracts
