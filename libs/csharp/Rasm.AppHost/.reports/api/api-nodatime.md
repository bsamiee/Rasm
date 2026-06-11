# [RASM_APPHOST_API_NODATIME]

`NodaTime` supplies semantic time values for AppHost receipts, health snapshots, support windows, and persisted clock facts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- assembly: `NodaTime`
- namespace: `NodaTime`
- asset: runtime library
- rail: time

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: time value family
- rail: time

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :--------------- | :--------------- | :------------------------ |
|   [1]   | `Instant`        | time value       | records semantic time     |
|   [2]   | `LocalDate`      | time value       | records semantic time     |
|   [3]   | `LocalDateTime`  | time value       | records semantic time     |
|   [4]   | `ZonedDateTime`  | time value       | records semantic time     |
|   [5]   | `DateTimeZone`   | time value       | records semantic time     |
|   [6]   | `Duration`       | time value       | records semantic time     |
|   [7]   | `Period`         | time value       | records semantic time     |
|   [8]   | `IClock`         | contract surface | defines boundary contract |
|   [9]   | `SystemClock`    | clock provider   | anchors time contract     |
|  [10]   | `InstantPattern` | text pattern     | anchors time contract     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: time surface
- rail: time

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]       | [CAPABILITY]              |
| :-----: | :--------------------------------- | :----------------- | :------------------------ |
|   [1]   | `SystemClock.Instance`             | singleton property | selects real clock        |
|   [2]   | `Instant.FromUnixTimeMilliseconds` | instant factory    | converts epoch value      |
|   [3]   | `DateTimeZoneProviders.Tzdb`       | zone provider      | resolves time zone        |
|   [4]   | `Period.Between`                   | period factory     | computes calendar span    |
|   [5]   | `Duration.FromMilliseconds`        | duration factory   | creates duration value    |
|   [6]   | `InstantPattern.ExtendedIso`       | pattern property   | formats persisted instant |

## [4]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- duration types: `Duration`, `Period`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns

[LOCAL_ADMISSION]:
- Receipts store semantic instants, not local wall-clock values.
- Calendar and time-zone values stay explicit wherever persisted or exported.
- Elapsed timing and delays remain `TimeProvider` work; calendar truth remains NodaTime work.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic timestamps and calendar values
- Accept: receipts store instants and zones
- Reject: local DateTime vocabulary

