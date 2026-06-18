# [RASM_APPUI_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods,
durations, intervals, clocks, resolvers, and text patterns for AppHost receipts,
health snapshots, support windows, and persisted clock facts.

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

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                |
| :-----: | :--------------- | :----------------- | :-------------------- |
|   [1]   | `Instant`        | global instant     | receipt timestamp     |
|   [2]   | `OffsetDateTime` | offset timestamp   | exported timestamp    |
|   [3]   | `ZonedDateTime`  | zoned timestamp    | user-facing timestamp |
|   [4]   | `LocalDate`      | calendar date      | date-only policy      |
|   [5]   | `LocalTime`      | local time         | time-only policy      |
|   [6]   | `LocalDateTime`  | local timestamp    | zone-mapped input     |
|   [7]   | `YearMonth`      | calendar month     | period boundary       |
|   [8]   | `AnnualDate`     | recurring date     | annual schedule       |
|   [9]   | `Offset`         | UTC offset         | offset identity       |
|  [10]   | `OffsetDate`     | date plus offset   | offset date payload   |
|  [11]   | `OffsetTime`     | time plus offset   | offset time payload   |
|  [12]   | `Duration`       | fixed elapsed span | elapsed quantity      |
|  [13]   | `Period`         | calendar span      | calendar quantity     |
|  [14]   | `PeriodBuilder`  | mutable period     | period construction   |
|  [15]   | `Interval`       | instant interval   | receipt window        |
|  [16]   | `DateInterval`   | calendar interval  | date window           |

[PUBLIC_TYPE_SCOPE]: zone calendar and clock family
- rail: time

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :----------------------- | :---------------- | :----------------------- |
|   [1]   | `IClock`                 | clock contract    | current instant seam     |
|   [2]   | `SystemClock`            | clock provider    | real clock               |
|   [3]   | `ZonedClock`             | zoned clock       | clock plus zone          |
|   [4]   | `CalendarSystem`         | calendar provider | calendar identity        |
|   [5]   | `IsoDayOfWeek`           | weekday enum      | schedule identity        |
|   [6]   | `DateTimeZone`           | zone model        | zone mapping             |
|   [7]   | `DateTimeZoneProviders`  | provider registry | zone provider access     |
|   [8]   | `IDateTimeZoneProvider`  | provider contract | zone lookup              |
|   [9]   | `DateTimeZoneCache`      | provider cache    | zone source cache        |
|  [10]   | `TzdbDateTimeZoneSource` | TZDB source       | zone data source         |
|  [11]   | `ZoneInterval`           | zone interval     | offset transition span   |
|  [12]   | `ZoneLocalMapping`       | local mapping     | ambiguous/skipped result |
|  [13]   | `Resolvers`              | mapping resolver  | local-time resolution    |
|  [14]   | `AmbiguousTimeException` | mapping exception | duplicate local time     |
|  [15]   | `SkippedTimeException`   | mapping exception | skipped local time       |

[PUBLIC_TYPE_SCOPE]: text interop and high-performance family
- rail: time

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------- | :---------------- | :------------------------ |
|   [1]   | `IPattern<T>`              | pattern contract  | parse/format abstraction  |
|   [2]   | `ParseResult<T>`           | parse result      | parse success/failure     |
|   [3]   | `InstantPattern`           | instant pattern   | persisted instant text    |
|   [4]   | `LocalDatePattern`         | date pattern      | calendar text             |
|   [5]   | `LocalDateTimePattern`     | timestamp pattern | local timestamp text      |
|   [6]   | `OffsetDateTimePattern`    | offset pattern    | offset timestamp text     |
|   [7]   | `ZonedDateTimePattern`     | zone pattern      | zoned timestamp text      |
|   [8]   | `DurationPattern`          | duration pattern  | elapsed span text         |
|   [9]   | `PeriodPattern`            | period pattern    | calendar span text        |
|  [10]   | `TypeConverterSettings`    | converter policy  | type-converter admission  |
|  [11]   | `XmlSerializationSettings` | XML policy        | XML projection admission  |
|  [12]   | `Instant64`                | compact instant   | high-performance instant  |
|  [13]   | `Duration64`               | compact duration  | high-performance duration |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant local and span operations
- rail: time

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :------------------------------- | :----------------- | :-------------------------- |
|   [1]   | `IClock.GetCurrentInstant`       | clock read         | current instant             |
|   [2]   | `SystemClock.Instance`           | clock singleton    | real clock                  |
|   [3]   | `Instant.FromUnixTime*`          | epoch conversion   | external timestamp intake   |
|   [4]   | `Instant.ToUnixTime*`            | epoch conversion   | external timestamp output   |
|   [5]   | `Instant.InUtc`                  | zone projection    | UTC timestamp projection    |
|   [6]   | `Instant.InZone`                 | zone projection    | zoned timestamp projection  |
|   [7]   | `Instant.WithOffset`             | offset projection  | offset timestamp projection |
|   [8]   | `LocalDate.AtStartOfDayInZone`   | zone mapping       | date boundary projection    |
|   [9]   | `LocalDateTime.InZoneStrictly`   | strict mapping     | rejects ambiguous input     |
|  [10]   | `LocalDateTime.InZoneLeniently`  | lenient mapping    | resolves ambiguous input    |
|  [11]   | `LocalDateTime.InZone(resolver)` | resolver mapping   | explicit mapping policy     |
|  [12]   | `Duration.From*`                 | duration factory   | elapsed span construction   |
|  [13]   | `Period.From*`                   | period factory     | calendar span construction  |
|  [14]   | `Period.Between`                 | period calculation | calendar span difference    |
|  [15]   | `PeriodBuilder.Build`            | period builder     | mutable span finalization   |
|  [16]   | `Interval.Contains`              | interval predicate | receipt-window membership   |

[ENTRYPOINT_SCOPE]: zone calendar and resolver operations
- rail: time

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]         | [RAIL]                     |
| :-----: | :------------------------------------ | :--------------------- | :------------------------- |
|   [1]   | `DateTimeZoneProviders.Tzdb`          | TZDB provider          | canonical zone lookup      |
|   [2]   | `DateTimeZoneProviders.Bcl`           | BCL provider           | BCL zone bridge            |
|   [3]   | `DateTimeZoneProviders.Serialization` | serialization provider | serialized zone lookup     |
|   [4]   | `IDateTimeZoneProvider.Ids`           | provider inventory     | zone id enumeration        |
|   [5]   | `GetZoneOrNull`                       | optional lookup        | nullable zone lookup       |
|   [6]   | `Item[string]`                        | required lookup        | throwing zone lookup       |
|   [7]   | `DateTimeZone.MapLocal`               | local mapping          | ambiguous/skipped model    |
|   [8]   | `ResolveLocal`                        | resolver mapping       | explicit local resolve     |
|   [9]   | `AtStrictly`                          | strict mapping         | throws on gaps/overlaps    |
|  [10]   | `AtLeniently`                         | lenient mapping        | resolves gaps/overlaps     |
|  [11]   | `GetZoneInterval`                     | interval lookup        | offset interval at instant |
|  [12]   | `GetZoneIntervals`                    | interval range         | offset intervals in span   |
|  [13]   | `Resolvers.ReturnEarlier`             | ambiguous resolver     | earlier duplicate time     |
|  [14]   | `Resolvers.ReturnLater`               | ambiguous resolver     | later duplicate time       |
|  [15]   | `Resolvers.ThrowWhenAmbiguous`        | ambiguous resolver     | rejects duplicate time     |
|  [16]   | `Resolvers.ReturnForwardShifted`      | skipped resolver       | shifts skipped time        |
|  [17]   | `Resolvers.StrictResolver`            | mapping resolver       | rejects non-unique time    |
|  [18]   | `Resolvers.LenientResolver`           | mapping resolver       | resolves non-unique time   |

[ENTRYPOINT_SCOPE]: text and interop operations
- rail: time

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]          | [RAIL]                     |
| :-----: | :------------------------------ | :---------------------- | :------------------------- |
|   [1]   | `InstantPattern.ExtendedIso`    | pattern singleton       | persisted instant format   |
|   [2]   | `OffsetDateTimePattern.Rfc3339` | pattern singleton       | external timestamp format  |
|   [3]   | `PeriodPattern.Roundtrip`       | pattern singleton       | persisted period format    |
|   [4]   | `CreateWithInvariantCulture`    | pattern factory         | invariant text parsing     |
|   [5]   | `CreateWithCurrentCulture`      | pattern factory         | culture-bound text parsing |
|   [6]   | `Parse`                         | pattern operation       | typed parse result         |
|   [7]   | `Format`                        | pattern operation       | typed text output          |
|   [8]   | `AppendFormat`                  | pattern operation       | builder text output        |
|   [9]   | `WithCulture`                   | pattern transform       | culture replacement        |
|  [10]   | `WithTemplateValue`             | pattern transform       | default value replacement  |
|  [11]   | `WithResolver`                  | zoned pattern transform | local mapping policy       |
|  [12]   | `WithZoneProvider`              | zoned pattern transform | zone lookup policy         |
|  [13]   | `WithCalendar`                  | pattern transform       | calendar replacement       |
|  [14]   | `ToInstant`                     | BCL conversion          | BCL timestamp intake       |
|  [15]   | `ToDateTimeOffset`              | BCL conversion          | BCL timestamp output       |
|  [16]   | `ToDuration`                    | BCL conversion          | elapsed span intake        |
|  [17]   | `ToTimeSpan`                    | BCL conversion          | elapsed span output        |

## [4]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- calendar types: `CalendarSystem`, `YearMonth`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit resolver, ambiguous mapping, skipped mapping
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns
- interop surfaces: BCL conversion extensions, XML settings, type converter settings
- compact surfaces: `Instant64`, `Duration64`

[LOCAL_ADMISSION]:
- Receipts store semantic instants, not local wall-clock values.
- Calendar and time-zone values stay explicit wherever persisted or exported.
- Local date-times require an explicit zone and resolver before they become instants.
- Text persistence uses invariant or roundtrip patterns, not culture-ambient formatting.
- Elapsed timing and delays remain `TimeProvider` work; calendar truth remains NodaTime work.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic timestamps and calendar values
- Accept: receipts store instants and zones
- Reject: local DateTime vocabulary
