# [RASM_APPHOST_API_CRONOS]

`Cronos` parses five- and six-field cron expressions — hash-jitter `H` fields included — and computes DST-correct forward and reverse occurrences over UTC instants and explicit time zones. `CronExpression` values persist as text, rebuild through `TryParse`, and feed every AppHost schedule row through one occurrence rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Cronos`
- package: `Cronos` (MIT)
- assembly: `Cronos`
- namespace: `Cronos`
- rail: schedule

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cron expression family

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------- | :------------ | :------------------------------------- |
|  [01]   | `CronExpression`       | class         | parse, occurrence math, value equality |
|  [02]   | `CronFormat`           | enum          | five- vs six-field admission           |
|  [03]   | `CronFormatException`  | class         | invalid-expression parse failure       |
|  [04]   | `MissingSeedException` | class         | `H` field parsed without a jitter seed |

[PUBLIC_TYPE_SCOPE]: format cases

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------- | :------------ | :------------------------------------ |
|  [01]   | `CronFormat.Standard`       | enum          | five-field minute-resolution schedule |
|  [02]   | `CronFormat.IncludeSeconds` | enum          | six-field second-resolution schedule  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and template construction

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                          |
| :-----: | :--------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `Parse(string)`                          | factory | throwing five-field standard parse    |
|  [02]   | `Parse(string, CronFormat)`              | factory | format-selected parse                 |
|  [03]   | `Parse(string, int)`                     | factory | seeded `H`-jitter parse               |
|  [04]   | `Parse(string, CronFormat, int)`         | factory | format plus jitter parse              |
|  [05]   | `TryParse(string, out)`                  | factory | non-throwing standard parse           |
|  [06]   | `TryParse(string, CronFormat, out)`      | factory | non-throwing format parse             |
|  [07]   | `TryParse(string, int, out)`             | factory | non-throwing jitter parse             |
|  [08]   | `TryParse(string, CronFormat, int, out)` | factory | non-throwing format-plus-jitter parse |
|  [09]   | `Yearly`..`EveryMinute`                  | static  | five-field canonical presets          |
|  [10]   | `EverySecond`                            | static  | six-field canonical preset            |
|  [11]   | `{Yearly..EveryMinute}WithJitter(int)`   | factory | seeded canonical preset               |

[ENTRYPOINT_SCOPE]: occurrence operations

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `GetNextOccurrence(DateTime)`                                            | instance | next UTC instant                       |
|  [02]   | `GetNextOccurrence(DateTime, TimeZoneInfo)`                              | instance | next zoned instant, UTC `DateTime`     |
|  [03]   | `GetNextOccurrence(DateTimeOffset, TimeZoneInfo)`                        | instance | next zoned `DateTimeOffset`            |
|  [04]   | `GetPreviousOccurrence(DateTime)`                                        | instance | previous UTC instant                   |
|  [05]   | `GetPreviousOccurrence(DateTime, TimeZoneInfo)`                          | instance | previous zoned instant, UTC `DateTime` |
|  [06]   | `GetPreviousOccurrence(DateTimeOffset, TimeZoneInfo)`                    | instance | previous zoned `DateTimeOffset`        |
|  [07]   | `GetOccurrences(DateTime, DateTime)`                                     | instance | lazy ascending UTC window              |
|  [08]   | `GetOccurrences(DateTime, DateTime, TimeZoneInfo)`                       | instance | lazy ascending zoned window            |
|  [09]   | `GetOccurrences(DateTimeOffset, DateTimeOffset, TimeZoneInfo)`           | instance | lazy ascending offset window           |
|  [10]   | `GetOccurrencesDescending(DateTime, DateTime)`                           | instance | lazy descending UTC window             |
|  [11]   | `GetOccurrencesDescending(DateTime, DateTime, TimeZoneInfo)`             | instance | lazy descending zoned window           |
|  [12]   | `GetOccurrencesDescending(DateTimeOffset, DateTimeOffset, TimeZoneInfo)` | instance | lazy descending offset window          |
|  [13]   | `ToString()`                                                             | instance | normalized expression text             |
|  [14]   | `Equals(CronExpression)` / `==` / `!=`                                   | operator | schedule value identity                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Standard format orders five fields: minute, hour, day-of-month, month, day-of-week.
- `IncludeSeconds` prepends a mandatory second field for six-field expressions.
- Special characters: `*` `,` `-` `/` `L` `W` `#` `?` and the jitter `H`.
- Macros: `@yearly`/`@annually`, `@monthly`, `@weekly`, `@daily`/`@midnight`, `@hourly`, `@every_minute`, `@every_second`.
- `H` demands a deterministic jitter seed; a seedless `H` raises `MissingSeedException`.
- `DateTime` inputs demand `DateTimeKind.Utc`; any other kind raises `ArgumentException`.
- Absent occurrences return a nullable `DateTime?`/`DateTimeOffset?`, never an exception.
- Range queries stay lazy, from-inclusive and to-exclusive by default; `from > to` raises `ArgumentException`.
- DST fall-back fires interval expressions in both ambiguous instants and point schedules once.
- DST spring-forward shifts a skipped-local occurrence to the first valid instant past the transition.
- `TimeZoneInfo.Utc` bypasses zone mapping on the fast path.

[STACKING]:
- `NodaTime`(`.api/api-nodatime.md`): occurrence math crosses at `Instant` — the schedule port feeds `Instant.ToDateTimeOffset()` into the occurrence call and lifts the returned UTC `DateTime?`/`DateTimeOffset?` back to `Instant`.
- `SchedulePort` (Runtime/time.md): wraps each expression in `OccurrenceSpec.Cron`, fires through `GetNextOccurrence`, audits history through `GetPreviousOccurrence`/`GetOccurrences`/`GetOccurrencesDescending`, seeds fleet spread into the `{…}WithJitter(int)` templates through `XxHash3.HashToUInt64`, and proves reload identity through `CronExpression.Equals`.

[LOCAL_ADMISSION]:
- Schedules persist as expression text and rebuild through `TryParse` at composition, so `CronFormatException` never crosses the configuration boundary.
- Occurrence math consumes and emits UTC instants; zone projection stays inside the occurrence call.
- Hash jitter carries an explicit deterministic seed keyed to schedule identity.

[RAIL_LAW]:
- Package: `Cronos`
- Owns: cron parsing and forward/reverse occurrence calculation
- Accept: UTC instants, explicit `TimeZoneInfo` zones, and per-identity jitter seeds
- Reject: hand-rolled cron parsing, local wall-clock occurrence math, and drop-in schedulers (Quartz, Hangfire, NCrontab)
