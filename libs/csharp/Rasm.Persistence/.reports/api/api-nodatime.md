# [RASM_PERSISTENCE_API_NODATIME]

`NodaTime` supplies precise instants, local dates, local times, offsets,
durations, intervals, periods, calendars, time zones, patterns, and zone
resolvers for store profiles, snapshots, and support receipts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- assembly: `NodaTime`
- namespace: `NodaTime`
- asset: runtime library
- rail: temporal-values

## [2]-[PUBLIC_TYPES]

[TEMPORAL_TYPES]: value surfaces
- rail: temporal-values

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :--------------- | :--------------- | :------------------------ |
|   [1]   | `Instant`        | timeline value   | identifies global time    |
|   [2]   | `LocalDate`      | local date       | carries date              |
|   [3]   | `LocalTime`      | local time       | carries clock time        |
|   [4]   | `LocalDateTime`  | local date-time  | carries local timestamp   |
|   [5]   | `OffsetDateTime` | offset date-time | carries offset timestamp  |
|   [6]   | `ZonedDateTime`  | zoned date-time  | carries zone timestamp    |
|   [7]   | `Duration`       | duration value   | carries elapsed time      |
|   [8]   | `Period`         | calendar amount  | carries calendar distance |
|   [9]   | `Interval`       | instant range    | carries time range        |
|  [10]   | `DateInterval`   | date range       | carries date range        |
|  [11]   | `Offset`         | offset value     | carries UTC offset        |
|  [12]   | `YearMonth`      | partial date     | carries year-month        |

[ZONE_TYPES]: clock and zone surfaces
- rail: temporal-values

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]       | [CAPABILITY]             |
| :-----: | :---------------------- | :------------------- | :----------------------- |
|   [1]   | `IClock`                | clock contract       | reads current instant    |
|   [2]   | `SystemClock`           | clock implementation | reads system instant     |
|   [3]   | `DateTimeZone`          | zone root            | maps instants and locals |
|   [4]   | `DateTimeZoneProviders` | provider root        | resolves zone providers  |
|   [5]   | `IDateTimeZoneProvider` | provider contract    | resolves zones           |
|   [6]   | `ZoneInterval`          | zone interval        | describes offset period  |
|   [7]   | `ZoneLocalMapping`      | mapping result       | resolves local ambiguity |
|   [8]   | `Resolvers`             | resolver root        | resolves skipped times   |

[TEXT_TYPES]: parsing and formatting surfaces
- rail: temporal-values

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :---------------------- | :------------- | :----------------------- |
|   [1]   | `InstantPattern`        | pattern        | parses instants          |
|   [2]   | `LocalDatePattern`      | pattern        | parses local dates       |
|   [3]   | `LocalTimePattern`      | pattern        | parses local times       |
|   [4]   | `LocalDateTimePattern`  | pattern        | parses local timestamps  |
|   [5]   | `OffsetDateTimePattern` | pattern        | parses offset timestamps |
|   [6]   | `ZonedDateTimePattern`  | pattern        | parses zoned timestamps  |
|   [7]   | `DurationPattern`       | pattern        | parses durations         |
|   [8]   | `PeriodPattern`         | pattern        | parses periods           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: temporal operations
- rail: temporal-values

| [INDEX] | [SURFACE]           | [CALL_SHAPE]    | [CAPABILITY]              |
| :-----: | :------------------ | :-------------- | :------------------------ |
|   [1]   | `GetCurrentInstant` | clock call      | reads current instant     |
|   [2]   | `FromUtc`           | factory call    | creates local date-time   |
|   [3]   | `InZone`            | conversion call | maps instant to zone      |
|   [4]   | `AtStrictly`        | zone call       | maps local time strictly  |
|   [5]   | `AtLeniently`       | zone call       | maps local time leniently |
|   [6]   | `ResolveLocal`      | zone call       | resolves local mapping    |
|   [7]   | `Plus`              | arithmetic call | adds duration or period   |
|   [8]   | `Minus`             | arithmetic call | subtracts time values     |
|   [9]   | `ToDateTimeUtc`     | BCL bridge      | projects UTC DateTime     |

[ENTRYPOINT_SCOPE]: parsing and formatting
- rail: temporal-values

| [INDEX] | [SURFACE]                    | [CALL_SHAPE] | [CAPABILITY]        |
| :-----: | :--------------------------- | :----------- | :------------------ |
|   [1]   | `Parse`                      | pattern call | parses text values  |
|   [2]   | `Format`                     | pattern call | formats text values |
|   [3]   | `CreateWithInvariantCulture` | pattern call | creates pattern     |
|   [4]   | `WithCulture`                | pattern call | applies culture     |

## [4]-[IMPLEMENTATION_LAW]

[TEMPORAL_POLICY]:
- namespace: `NodaTime`
- instant root: `Instant`
- local root: local date, local time, and local date-time
- zone root: date-time zones, providers, intervals, and resolvers
- text root: NodaTime patterns

[LOCAL_ADMISSION]:
- Persisted time values use NodaTime types where time semantics matter.
- Store profiles declare instant, local, offset, zone, duration, period, and interval handling.
- Ambiguous and skipped local times require explicit resolver policy.
- BCL time values are boundary projections, not internal Persistence vocabulary.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: temporal value algebra
- Accept: precise persisted time values
- Reject: raw `DateTime` semantics in store contracts
