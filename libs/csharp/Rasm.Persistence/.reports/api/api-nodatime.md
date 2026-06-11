# [RASM_PERSISTENCE_API_NODATIME]

`NodaTime` supplies semantic time values for store profiles, retention, receipts, and provider mappings.

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
|   [8]   | `OffsetDateTime` | time value       | records semantic time     |
|   [9]   | `IClock`         | contract surface | defines boundary contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: time operations
- rail: time

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]       | [CAPABILITY]           |
| :-----: | :--------------------------- | :----------------- | :--------------------- |
|   [1]   | `SystemClock.Instance`       | singleton property | selects real clock     |
|   [2]   | `Instant.FromUnixTimeTicks`  | instant factory    | converts epoch ticks   |
|   [3]   | `DateTimeZoneProviders.Tzdb` | zone provider      | resolves time zone     |
|   [4]   | `Duration.FromTimeSpan`      | duration factory   | creates duration value |
|   [5]   | `Period.FromDays`            | member surface     | drives time behavior   |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic persisted time
- Accept: store receipts use time values
- Reject: DateTime as store law

