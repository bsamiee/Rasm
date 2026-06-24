# [RASM_COMPUTE_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods,
durations, intervals, clocks, resolvers, and text patterns for AppHost receipts,
health snapshots, support windows, and persisted clock facts. Version `3.3.2` is
Apache-2.0 and ships `lib/net8.0`+`lib/netstandard2.0`; the workspace `net10.0`
consumer binds `lib/net8.0`, which is the build that carries the `NodaTime.HighPerformance`
`Instant64`/`Duration64` `long`-backed tier and the generic-math operator interfaces
(`IAdditionOperators`/`IComparisonOperators`/`IMinMaxValue`) those compact types implement.
The Protobuf and System.Text.Json wire seams live in the sibling `NodaTime.Serialization.*`
packages (`.api/api-nodatime-protobuf.md`); this catalog is the core temporal owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- assembly: `NodaTime`
- namespace: `NodaTime`, `NodaTime.Text`, `NodaTime.Text.Patterns`, `NodaTime.TimeZones`, `NodaTime.TimeZones.Cldr`, `NodaTime.Xml`, `NodaTime.HighPerformance`
- asset: runtime library (managed; Apache-2.0; consumer-bound TFM `lib/net8.0`)
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

[PUBLIC_TYPE_SCOPE]: text interop and high-performance family
- rail: time

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------- | :---------------- | :------------------------ |
|  [01]   | `IPattern<T>`              | pattern contract  | parse/format abstraction  |
|  [02]   | `ParseResult<T>`           | parse result      | parse success/failure     |
|  [03]   | `InstantPattern`           | instant pattern   | persisted instant text    |
|  [04]   | `LocalDatePattern`         | date pattern      | calendar text             |
|  [05]   | `LocalDateTimePattern`     | timestamp pattern | local timestamp text      |
|  [06]   | `OffsetDateTimePattern`    | offset pattern    | offset timestamp text     |
|  [07]   | `ZonedDateTimePattern`     | zone pattern      | zoned timestamp text      |
|  [08]   | `DurationPattern`          | duration pattern  | elapsed span text         |
|  [09]   | `PeriodPattern`            | period pattern    | calendar span text        |
|  [10]   | `Text.TypeConverterSettings`    | converter policy  | type-converter `DateTimeZoneProvider` admission (namespace `NodaTime.Text`)  |
|  [11]   | `Xml.XmlSerializationSettings`  | XML policy        | XML projection `DateTimeZoneProvider` admission (namespace `NodaTime.Xml`)  |
|  [12]   | `HighPerformance.Instant64`     | compact instant   | `long`-tick instant (namespace `NodaTime.HighPerformance`); bridge `Instant64.FromInstant(Instant)` / `.ToInstant()`; `UnixEpoch`/`MinValue`/`MaxValue`, `PlusTicks`/`PlusNanoseconds`/`Plus(Duration64)`/`Minus`, generic-math operators |
|  [13]   | `HighPerformance.Duration64`    | compact duration  | `long`-tick duration (namespace `NodaTime.HighPerformance`); bridge `Duration64.FromDuration(Duration)` / `.ToDuration()`; `Zero`/`MaxValue`/`MinValue`, additive/negation/comparison generic-math operators |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant local and span operations
- rail: time

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `IClock.GetCurrentInstant`       | clock read         | current instant             |
|  [02]   | `SystemClock.Instance`           | clock singleton    | real clock                  |
|  [03]   | `Instant.FromUnixTime*`          | epoch conversion   | external timestamp intake   |
|  [04]   | `Instant.ToUnixTime*`            | epoch conversion   | external timestamp output   |
|  [05]   | `Instant.InUtc`                  | zone projection    | UTC timestamp projection    |
|  [06]   | `Instant.InZone`                 | zone projection    | zoned timestamp projection  |
|  [07]   | `Instant.WithOffset`             | offset projection  | offset timestamp projection |
|  [08]   | `LocalDate.AtStartOfDayInZone`   | zone mapping       | date boundary projection    |
|  [09]   | `LocalDateTime.InZoneStrictly`   | strict mapping     | rejects ambiguous input     |
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
|  [13]   | `Resolvers.ReturnEarlier`             | ambiguous resolver     | earlier duplicate time (`AmbiguousTimeResolver`) |
|  [14]   | `Resolvers.ReturnLater`               | ambiguous resolver     | later duplicate time       |
|  [15]   | `Resolvers.ThrowWhenAmbiguous`        | ambiguous resolver     | rejects duplicate time     |
|  [16]   | `Resolvers.ReturnForwardShifted`      | skipped resolver       | shifts skipped time forward (`SkippedTimeResolver`) |
|  [17]   | `Resolvers.ReturnStartOfIntervalAfter`| skipped resolver       | start of the interval after the gap |
|  [18]   | `Resolvers.ReturnEndOfIntervalBefore` | skipped resolver       | end of the interval before the gap |
|  [19]   | `Resolvers.ThrowWhenSkipped`          | skipped resolver       | rejects skipped time       |
|  [20]   | `Resolvers.StrictResolver`            | mapping resolver       | `CreateMappingResolver(ThrowWhenAmbiguous, ThrowWhenSkipped)` — rejects non-unique time |
|  [21]   | `Resolvers.LenientResolver`           | mapping resolver       | `CreateMappingResolver(ReturnEarlier, ReturnForwardShifted)` — resolves non-unique time |
|  [22]   | `Resolvers.CreateMappingResolver`     | resolver composer      | folds an ambiguous + skipped resolver into one `ZoneLocalMappingResolver` |

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
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.Text.Patterns`, `NodaTime.TimeZones`, `NodaTime.TimeZones.Cldr`, `NodaTime.Xml`, `NodaTime.HighPerformance`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- calendar types: `CalendarSystem`, `YearMonth`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit resolver, ambiguous mapping, skipped mapping
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns
- interop surfaces: BCL conversion extensions, `NodaTime.Xml.XmlSerializationSettings`, `NodaTime.Text.TypeConverterSettings` (both expose a `DateTimeZoneProvider` admission seam)
- compact surfaces: `NodaTime.HighPerformance.Instant64`/`Duration64` — `long`-tick value types bridged to the semantic tier through `Instant64.FromInstant`/`.ToInstant()` and `Duration64.FromDuration`/`.ToDuration()`; they implement the generic-math operator interfaces, so a hot-path accumulation stays in the compact tier and crosses to `Instant`/`Duration` only at the receipt boundary. The semantic `Instant`/`Duration` stay the persisted and exported truth; the compact tier is an interior arithmetic accelerator, never the wire shape.

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
