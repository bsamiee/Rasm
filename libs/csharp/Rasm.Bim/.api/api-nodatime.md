# [RASM_BIM_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods,
durations, intervals, clocks, resolvers, text patterns, and the `TimeProvider`/BCL
interop bridge for AppHost receipts, health snapshots, support windows, and persisted
clock facts. It stacks under the Bim time rail as the calendar-truth owner: the
`NodaTime.Serialization.SystemTextJson` and `NodaTime.Serialization.Protobuf` adapter
packages (catalogued separately, both pinned in the Interchange manifest) bridge these
value types to the STJ and protobuf wire seams, so a receipt timestamp authored as an
`Instant` round-trips through the wire codec without a hand-rolled converter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- version: `3.3.2`
- license: Apache-2.0
- assembly: `NodaTime`
- namespace: `NodaTime`
- namespace: `NodaTime.Text`
- namespace: `NodaTime.TimeZones`
- namespace: `NodaTime.Extensions`
- namespace: `NodaTime.Calendars`
- namespace: `NodaTime.Xml`
- asset: net8.0, netstandard2.0; the net10.0 consumer binds the `lib/net8.0` asset (no net9/net10 asset ships, so the bound surface is the net8.0 one)
- asset: IL-only AnyCPU managed assembly; bundled IANA TZDB resource; no native binaries
- rail: time

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: time value family
- rail: time

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                |
| :-----: | :--------------- | :----------------- | :-------------------- |
|  [01]   | `Instant`        | global instant     | receipt timestamp     |
|  [02]   | `OffsetDateTime` | offset timestamp   | exported timestamp    |
|  [03]   | `ZonedDateTime`  | zoned timestamp    | user-facing timestamp |
|  [04]   | `LocalDate`      | calendar date      | date-only policy      |
|  [05]   | `LocalTime`      | local time         | time-only policy      |
|  [06]   | `LocalDateTime`  | local timestamp    | zone-mapped input     |
|  [07]   | `YearMonth`      | calendar month     | period boundary       |
|  [08]   | `AnnualDate`     | recurring date     | annual schedule       |
|  [09]   | `Offset`         | UTC offset         | offset identity       |
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
|  [01]   | `IClock`                 | clock contract    | current instant seam     |
|  [02]   | `SystemClock`            | clock provider    | real clock               |
|  [03]   | `ZonedClock`             | zoned clock       | clock plus zone          |
|  [04]   | `CalendarSystem`         | calendar provider | calendar identity        |
|  [05]   | `IsoDayOfWeek`           | weekday enum      | schedule identity        |
|  [06]   | `DateTimeZone`           | zone model        | zone mapping             |
|  [07]   | `DateTimeZoneProviders`  | provider registry | zone provider access     |
|  [08]   | `IDateTimeZoneProvider`  | provider contract | zone lookup              |
|  [09]   | `DateTimeZoneCache`      | provider cache    | zone source cache        |
|  [10]   | `TzdbDateTimeZoneSource` | TZDB source       | zone data source         |
|  [11]   | `ZoneInterval`           | zone interval     | offset transition span   |
|  [12]   | `ZoneLocalMapping`       | local mapping     | ambiguous/skipped result |
|  [13]   | `Resolvers`              | mapping resolver  | local-time resolution    |
|  [14]   | `AmbiguousTimeException` | mapping exception | duplicate local time     |
|  [15]   | `SkippedTimeException`   | mapping exception | skipped local time       |

[PUBLIC_TYPE_SCOPE]: text pattern family (`NodaTime.Text`)
- rail: time

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :---------------------- | :---------------- | :----------------------- |
|  [01]   | `IPattern<T>`           | pattern contract  | parse/format abstraction |
|  [02]   | `ParseResult<T>`        | parse result      | parse success/failure    |
|  [03]   | `InstantPattern`        | instant pattern   | persisted instant text   |
|  [04]   | `LocalDatePattern`      | date pattern      | calendar text            |
|  [05]   | `LocalTimePattern`      | time pattern      | local time text          |
|  [06]   | `LocalDateTimePattern`  | timestamp pattern | local timestamp text     |
|  [07]   | `OffsetPattern`         | offset pattern    | UTC-offset text          |
|  [08]   | `OffsetDateTimePattern` | offset pattern    | offset timestamp text    |
|  [09]   | `ZonedDateTimePattern`  | zone pattern      | zoned timestamp text     |
|  [10]   | `DurationPattern`       | duration pattern  | elapsed span text        |
|  [11]   | `PeriodPattern`         | period pattern    | calendar span text       |
|  [12]   | `YearMonthPattern`      | month pattern     | calendar-month text      |
|  [13]   | `AnnualDatePattern`     | recurring pattern | annual-date text         |
|  [14]   | `CompositePatternBuilder` | pattern composer | format-dispatched multi-pattern (non-generic builder; `Add(IPattern<T>, predicate)` per branch) |
|  [15]   | `InvalidPatternException` | pattern failure | malformed pattern reject |
|  [16]   | `TypeConverterSettings` | converter policy  | type-converter admission |

[PUBLIC_TYPE_SCOPE]: BCL/`TimeProvider` interop family (`NodaTime.Extensions`, `NodaTime.Xml`)
- rail: time
- note: the BCL bridge is the named seam to the elapsed-timing owner — `TimeProviderExtensions.ToClock`/`GetCurrentInstant` is how a `System.TimeProvider` (the `LOCAL_ADMISSION` elapsed-timing owner) projects into a NodaTime `IClock` without a parallel clock type, so the same injected `TimeProvider` drives both the delay axis and the calendar-truth axis.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [RAIL]                       |
| :-----: | :-------------------------- | :----------------- | :--------------------------- |
|  [01]   | `TimeProviderExtensions`    | `TimeProvider` bridge | `TimeProvider` -> `IClock`/`ZonedClock` |
|  [02]   | `ClockExtensions`           | clock bridge       | `IClock` -> `ZonedClock` (`InZone`/`InUtc`) |
|  [03]   | `DateTimeOffsetExtensions`  | BCL bridge         | `DateTimeOffset` -> `Instant`/`OffsetDateTime`/`ZonedDateTime` |
|  [04]   | `DateTimeZoneProviderExtensions` | provider bridge | provider default-zone resolution |
|  [05]   | `XmlSerializationSettings`  | XML policy         | XML projection admission     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant local and span operations
- rail: time

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :--------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `IClock.GetCurrentInstant`         | clock read         | current instant             |
|  [02]   | `SystemClock.Instance`             | clock singleton    | real clock                  |
|  [03]   | `Instant.FromUnixTimeSeconds`/`FromUnixTimeMilliseconds`/`FromUnixTimeTicks` | epoch conversion | external timestamp intake |
|  [04]   | `Instant.ToUnixTimeSeconds`/`ToUnixTimeMilliseconds`/`ToUnixTimeTicks` | epoch conversion | external timestamp output |
|  [05]   | `Instant.ToUnixTimeSecondsAndNanoseconds` | epoch conversion | `(seconds, nanoseconds)` tuple intake/output |
|  [06]   | `Instant.FromUtc`                  | calendar intake    | UTC y/m/d/h/m[/s] construction |
|  [07]   | `Instant.FromDateTimeUtc`/`FromDateTimeOffset` | BCL intake | `DateTime`/`DateTimeOffset` -> `Instant` |
|  [08]   | `Instant.InUtc`                    | zone projection    | UTC timestamp projection    |
|  [09]   | `Instant.InZone`                   | zone projection    | zoned timestamp projection  |
|  [10]   | `Instant.WithOffset`               | offset projection  | offset timestamp projection |
|  [11]   | `LocalDate.AtStartOfDayInZone`     | zone mapping       | date boundary projection    |
|  [12]   | `LocalDateTime.InZoneStrictly`     | strict mapping     | rejects ambiguous input     |
|  [13]   | `LocalDateTime.InZoneLeniently`    | lenient mapping    | resolves ambiguous input    |
|  [14]   | `LocalDateTime.InZone(resolver)`   | resolver mapping   | explicit mapping policy     |
|  [15]   | `Duration.From*`                   | duration factory   | elapsed span construction   |
|  [16]   | `Period.From*`                     | period factory     | calendar span construction  |
|  [17]   | `Period.Between`                   | period calculation | calendar span difference    |
|  [18]   | `PeriodBuilder.Build`              | period builder     | mutable span finalization   |
|  [19]   | `Interval.Contains`                | interval predicate | receipt-window membership   |
|  [20]   | `Interval.Start`/`End`/`Duration`  | interval projection | receipt-window bounds + elapsed span |

[ENTRYPOINT_SCOPE]: local/calendar value construction, arithmetic, and static anchors
- rail: time

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]      | [RAIL]                       |
| :-----: | :------------------------------------------------------------------ | :------------------ | :--------------------------- |
|  [01]   | `Instant.MinValue`                                                  | sentinel anchor     | the min-instant sentinel an uninitialized capture/window carries (`BimModel` init) |
|  [02]   | `Duration.Zero` / `Period.Zero`                                    | identity anchor     | the zero elapsed-span / zero calendar-span identity a fold seeds |
|  [03]   | `Duration.TotalDays` (+ `TotalHours`/`TotalMinutes`/`TotalSeconds`) | elapsed projection  | the `double` whole-span projection a ratio/earned-value fold divides by |
|  [04]   | `DateTimeZone.Utc`                                                 | fixed-zone anchor   | the UTC `DateTimeZone` a default working-time function binds |
|  [05]   | `LocalDate.FromDateTime` / `LocalDateTime.FromDateTime` (`(DateTime[, CalendarSystem])`) | BCL intake | `DateTime` -> calendar value — the GeometryGym `IfcTaskTime`/`IfcWorkTime` `DateTime` boundary intake |
|  [06]   | `LocalDate.PlusDays(int)` / `LocalDate.Next(IsoDayOfWeek)`          | date arithmetic     | day-step and next-weekday advance over a work-calendar walk |
|  [07]   | `LocalDate.DayOfWeek`                                              | weekday projection  | the `IsoDayOfWeek` a work-week membership test reads |
|  [08]   | `ZonedDateTime.ToInstant()`                                        | zone -> instant     | collapse a zoned wall-clock to the global `Instant` (the CPM/schedule fold) |
|  [09]   | `ZonedDateTime.Date`                                               | zoned projection    | the `LocalDate` a shift-boundary advance steps off |

[ENTRYPOINT_SCOPE]: `TimeProvider`/BCL interop entrypoints
- rail: time

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]     | [RAIL]                       |
| :-----: | :----------------------------------------------- | :----------------- | :--------------------------- |
|  [01]   | `TimeProviderExtensions.ToClock(this TimeProvider)` | `TimeProvider` bridge | injected `TimeProvider` -> NodaTime `IClock` |
|  [02]   | `TimeProviderExtensions.GetCurrentInstant(this TimeProvider)` | `TimeProvider` read | `TimeProvider` now-instant without an `IClock` |
|  [03]   | `TimeProviderExtensions.ToZonedClock(this TimeProvider[, CalendarSystem])` | `TimeProvider` bridge | injected `TimeProvider` -> `ZonedClock` |
|  [04]   | `ClockExtensions.InZone`/`InUtc`/`InTzdbSystemDefaultZone` | clock bridge | `IClock` -> `ZonedClock` |
|  [05]   | `DateTimeOffsetExtensions.ToInstant`/`ToOffsetDateTime`/`ToZonedDateTime` | BCL bridge | `DateTimeOffset` boundary intake |

[ENTRYPOINT_SCOPE]: zone calendar and resolver operations
- rail: time

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]         | [RAIL]                     |
| :-----: | :------------------------------------ | :--------------------- | :------------------------- |
|  [01]   | `DateTimeZoneProviders.Tzdb`          | TZDB provider          | canonical zone lookup      |
|  [02]   | `DateTimeZoneProviders.Bcl`           | BCL provider           | BCL zone bridge            |
|  [03]   | `DateTimeZoneProviders.Serialization` | serialization provider | serialized zone lookup     |
|  [04]   | `IDateTimeZoneProvider.Ids`           | provider inventory     | zone id enumeration        |
|  [05]   | `GetZoneOrNull`                       | optional lookup        | nullable zone lookup       |
|  [06]   | `Item[string]`                        | required lookup        | throwing zone lookup       |
|  [07]   | `DateTimeZone.MapLocal`               | local mapping          | ambiguous/skipped model    |
|  [08]   | `ResolveLocal`                        | resolver mapping       | explicit local resolve     |
|  [09]   | `AtStrictly`                          | strict mapping         | throws on gaps/overlaps    |
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
|  [01]   | `InstantPattern.ExtendedIso`    | pattern singleton       | persisted instant format   |
|  [02]   | `OffsetDateTimePattern.Rfc3339` | pattern singleton       | external timestamp format  |
|  [03]   | `PeriodPattern.Roundtrip`       | pattern singleton       | persisted period format    |
|  [04]   | `CreateWithInvariantCulture`    | pattern factory         | invariant text parsing     |
|  [05]   | `CreateWithCurrentCulture`      | pattern factory         | culture-bound text parsing |
|  [06]   | `Parse`                         | pattern operation       | typed parse result         |
|  [07]   | `Format`                        | pattern operation       | typed text output          |
|  [08]   | `AppendFormat`                  | pattern operation       | builder text output        |
|  [09]   | `WithCulture`                   | pattern transform       | culture replacement        |
|  [10]   | `WithTemplateValue`             | pattern transform       | default value replacement  |
|  [11]   | `WithResolver`                  | zoned pattern transform | local mapping policy       |
|  [12]   | `WithZoneProvider`              | zoned pattern transform | zone lookup policy         |
|  [13]   | `WithCalendar`                  | pattern transform       | calendar replacement       |
|  [14]   | `ToInstant`                     | BCL conversion          | BCL timestamp intake       |
|  [15]   | `ToDateTimeOffset`              | BCL conversion          | BCL timestamp output       |
|  [16]   | `ToDuration`                    | BCL conversion          | elapsed span intake        |
|  [17]   | `ToTimeSpan`                    | BCL conversion          | elapsed span output        |

## [04]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Extensions`, `NodaTime.Calendars`, `NodaTime.Xml`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- calendar types: `CalendarSystem`, `YearMonth`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit resolver, ambiguous mapping, skipped mapping
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns, `NodaTime.Text.CompositePatternBuilder` for format-dispatched multi-pattern parse
- interop surfaces: `NodaTime.Extensions` BCL/`TimeProvider` bridges (`ToClock`, `GetCurrentInstant`, `ToInstant`), `NodaTime.Xml` settings, `NodaTime.Text.TypeConverterSettings`
- wire adapters (separate packages, both manifest-pinned): `NodaTime.Serialization.SystemTextJson` (STJ converter registration) and `NodaTime.Serialization.Protobuf` (protobuf `Timestamp`/`Duration` mapping) — the value types here are authored canonical and serialized through these adapters, never a hand-rolled JSON/proto converter

[INTEGRATION_STACK]:
- clock seam: a single injected `System.TimeProvider` is the one elapsed-timing root; `TimeProviderExtensions.ToClock` projects it into a NodaTime `IClock` so receipt timestamps (`Instant`) and delay/timeout work share one fake-able clock under test — no parallel `SystemClock` direct read in domain code, no second injected clock type.
- wire seam: an `Instant`/`ZonedDateTime`/`Duration` authored canonical serializes through `NodaTime.Serialization.SystemTextJson` (registered once on the STJ options the Bim wire codec owns) and `NodaTime.Serialization.Protobuf` (the protobuf `Timestamp`/`Duration` field mapping), so the same receipt fact crosses the JSON and protobuf boundaries with identity-preserving round-trip rather than a per-boundary converter.
- hash seam: a persisted clock fact that participates in a snapshot fingerprint formats through an invariant `InstantPattern.ExtendedIso`/`PeriodPattern.Roundtrip` BEFORE the `System.IO.Hashing` `XxHash3.Append` so the identity hash is stable across cultures and machines — a culture-ambient `ToString()` would make the fingerprint non-deterministic.
- text seam: `CompositePatternBuilder` owns multi-format intake (e.g. an external feed mixing RFC-3339 and extended-ISO) as one format-dispatched parser, not a try/catch ladder over sibling `IPattern<T>` instances.

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
