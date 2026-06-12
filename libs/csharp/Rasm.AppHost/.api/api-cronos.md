# [RASM_APPHOST_API_CRONOS]

`Cronos` supplies cron expression parsing, five and six field formats, hash-based
jitter, and DST-correct next/previous occurrence calculation over UTC instants and
explicit time zones for AppHost schedule rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Cronos`
- package: `Cronos`
- assembly: `Cronos`
- namespace: `Cronos`
- asset: runtime library
- rail: schedule

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cron expression family
- rail: schedule

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [RAIL]                 |
| :-----: | :--------------------- | :------------- | :--------------------- |
|   [1]   | `CronExpression`       | schedule value | occurrence calculation |
|   [2]   | `CronFormat`           | format enum    | field-count admission  |
|   [3]   | `CronFormatException`  | parse failure  | invalid expression     |
|   [4]   | `MissingSeedException` | jitter failure | `H` without seed       |

[PUBLIC_TYPE_SCOPE]: format cases
- rail: schedule

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                     |
| :-----: | :-------------------------- | :------------ | :------------------------- |
|   [1]   | `CronFormat.Standard`       | five fields   | minute-resolution schedule |
|   [2]   | `CronFormat.IncludeSeconds` | six fields    | second-resolution schedule |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and template construction
- rail: schedule

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]   | [RAIL]                       |
| :-----: | :-------------------------------- | :--------------- | :--------------------------- |
|   [1]   | `Parse(expression)`               | throwing parse   | five-field standard parse    |
|   [2]   | `Parse(expression, format)`       | throwing parse   | format-selected parse        |
|   [3]   | `Parse(expression, jitterSeed)`   | throwing parse   | seeded `H` jitter parse      |
|   [4]   | `Parse(expr, format, jitterSeed)` | throwing parse   | format plus jitter parse     |
|   [5]   | `TryParse(expression, out)`       | guarded parse    | non-throwing standard parse  |
|   [6]   | `TryParse(expr, format, out)`     | guarded parse    | non-throwing format parse    |
|   [7]   | `TryParse(expr, seed, out)`       | guarded parse    | non-throwing jitter parse    |
|   [8]   | `TryParse(expr, fmt, seed, out)`  | guarded parse    | non-throwing full parse      |
|   [9]   | `Yearly`..`EveryMinute`           | static template  | five-field canonical values  |
|  [10]   | `EverySecond`                     | static template  | six-field canonical value    |
|  [11]   | `YearlyWithJitter(seed)` family   | jitter template  | seeded template construction |
|  [12]   | `@yearly`..`@every_second` macros | macro expression | named schedule intake        |

[ENTRYPOINT_SCOPE]: occurrence operations
- rail: schedule

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :----------------------------------------- | :----------------- | :-------------------------- |
|   [1]   | `GetNextOccurrence(fromUtc)`               | point query        | next UTC instant            |
|   [2]   | `GetNextOccurrence(fromUtc, zone)`         | point query        | next zoned instant          |
|   [3]   | `GetNextOccurrence(from, zone)`            | offset point query | next `DateTimeOffset`       |
|   [4]   | `GetPreviousOccurrence(fromUtc)`           | point query        | previous UTC instant        |
|   [5]   | `GetPreviousOccurrence(fromUtc, zone)`     | point query        | previous zoned instant      |
|   [6]   | `GetPreviousOccurrence(from, zone)`        | offset point query | previous `DateTimeOffset`   |
|   [7]   | `GetOccurrences(fromUtc, toUtc)`           | range query        | lazy ascending UTC window   |
|   [8]   | `GetOccurrences(fromUtc, toUtc, zone)`     | range query        | lazy ascending zoned window |
|   [9]   | `GetOccurrences(from, to, zone)`           | offset range query | lazy ascending offsets      |
|  [10]   | `GetOccurrencesDescending(fromUtc, toUtc)` | range query        | lazy descending window      |
|  [11]   | `GetOccurrencesDescending(.., zone)`       | range query        | descending zoned/offset     |
|  [12]   | `ToString`                                 | text projection    | normalized expression text  |
|  [13]   | `Equals` / `==` / `!=`                     | value equality     | schedule identity           |

## [4]-[IMPLEMENTATION_LAW]

[SCHEDULE_TOPOLOGY]:
- namespace: `Cronos` only; four public types
- standard fields: minute, hour, day of month, month, day of week
- `IncludeSeconds`: prepends a mandatory second field for six-field expressions
- special characters: `*`, `,`, `-`, `/`, `L`, `W`, `#`, `?`, and seeded `H` jitter
- macros: `@yearly`/`@annually`, `@monthly`, `@weekly`, `@daily`/`@midnight`, `@hourly`, `@every_minute`, `@every_second`
- jitter: `H` requires a deterministic seed; parsing without one raises `MissingSeedException`
- kind discipline: `DateTime` inputs require `DateTimeKind.Utc`, otherwise `ArgumentException`
- absent occurrence: nullable `DateTime?`/`DateTimeOffset?` result, never an exception
- range queries: lazy enumerables, from-inclusive and to-exclusive by default; `from > to` raises `ArgumentException`
- DST fall-back: interval expressions fire in both ambiguous instances; point schedules fire once
- DST spring-forward: occurrences in skipped local time shift to the first valid moment after the transition
- UTC fast path: `TimeZoneInfo.Utc` bypasses zone mapping entirely

[LOCAL_ADMISSION]:
- Schedules persist as expression text; `CronExpression` values rebuild via `Parse` at composition.
- Occurrence math consumes UTC instants and emits UTC instants; zone projection stays inside the occurrence call.
- Hash jitter carries an explicit deterministic seed per schedule identity.
- Boundary intake uses `TryParse`; `CronFormatException` never crosses the configuration boundary.

[RAIL_LAW]:
- Package: `Cronos`
- Owns: cron expression parsing and occurrence calculation
- Accept: UTC instants and explicit time zones
- Reject: hand-rolled cron parsing or local wall-clock occurrence math
