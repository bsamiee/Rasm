# [RASM_APPUI_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods, durations, intervals, clocks, resolvers, BCL/`TimeProvider` bridges, and culture-invariant text patterns for AppHost receipts, health snapshots, support windows, and persisted clock facts. Its value types implement the .NET generic-math operator interfaces, so they compose into generic numeric/comparison constraints.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- version: `3.3.2`
- license: `Apache-2.0`
- assembly: `NodaTime`
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`, `NodaTime.Utility` (269 types across 16 namespaces)
- companions (separate assemblies, central manifest): `NodaTime.Serialization.SystemTextJson`, `NodaTime.Serialization.Protobuf`, `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`, `NodaTime.Testing` (`FakeClock`)
- asset: runtime library
- rail: time

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: time value family — all are `readonly struct` value types implementing `IEquatable`/`IComparable`/`IFormattable`/`IXmlSerializable` and (where ordered/additive) the generic-math operator interfaces — rail: time

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
|  [17]   | `DateAdjusters` / `TimeAdjusters` | adjuster functions | start/end-of-month, truncation projections |

[PUBLIC_TYPE_SCOPE]: zone, calendar, and clock family — rail: time

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                   |
| :-----: | :----------------------- | :---------------- | :----------------------- |
|  [01]   | `IClock`                 | clock contract    | current instant seam     |
|  [02]   | `SystemClock`            | clock provider    | real clock               |
|  [03]   | `ZonedClock`             | zoned clock       | clock plus zone          |
|  [04]   | `CalendarSystem`         | calendar provider | calendar identity (Iso/Persian/Hebrew/Islamic/Gregorian) |
|  [05]   | `IsoDayOfWeek`           | weekday enum      | schedule identity        |
|  [06]   | `DateTimeZone`           | zone model        | zone mapping             |
|  [07]   | `DateTimeZoneProviders`  | provider registry | zone provider access     |
|  [08]   | `IDateTimeZoneProvider`  | provider contract | zone lookup              |
|  [09]   | `DateTimeZoneCache`      | provider cache    | zone source cache        |
|  [10]   | `TzdbDateTimeZoneSource` | TZDB source       | zone data source         |
|  [11]   | `ZoneInterval`           | zone interval     | offset transition span   |
|  [12]   | `ZoneLocalMapping`       | local mapping     | ambiguous/skipped result |
|  [13]   | `Resolvers`              | mapping resolvers | local-time resolution + composition |
|  [14]   | `AmbiguousTimeResolver` / `SkippedTimeResolver` / `ZoneLocalMappingResolver` | resolver delegates | resolver building blocks |
|  [15]   | `AmbiguousTimeException` / `SkippedTimeException` | mapping exceptions | duplicate / skipped local time |

[PUBLIC_TYPE_SCOPE]: text, BCL-bridge, and interop family — rail: time

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------------------ | :---------------- | :------------------------ |
|  [01]   | `IPattern<T>`                         | pattern contract  | parse/format abstraction  |
|  [02]   | `ParseResult<T>`                      | parse result      | parse success/failure     |
|  [03]   | `InstantPattern`                      | instant pattern   | persisted instant text    |
|  [04]   | `LocalDatePattern`                    | date pattern      | calendar text             |
|  [05]   | `LocalDateTimePattern`                | timestamp pattern | local timestamp text      |
|  [06]   | `OffsetDateTimePattern`               | offset pattern    | offset timestamp text     |
|  [07]   | `ZonedDateTimePattern`                | zone pattern      | zoned timestamp text      |
|  [08]   | `DurationPattern`                     | duration pattern  | elapsed span text         |
|  [09]   | `PeriodPattern`                       | period pattern    | calendar span text        |
|  [10]   | `CompositePatternBuilder<T>`          | composite pattern | multi-format parse builder |
|  [11]   | `NodaTime.Text.TypeConverterSettings` | converter policy  | static `DateTimeZoneProvider` for `TypeConverter` parsing |
|  [12]   | `NodaTime.Xml.XmlSerializationSettings` | XML policy      | static `DateTimeZoneProvider` for `IXmlSerializable` |
|  [13]   | `NodaTime.Extensions.TimeProviderExtensions` | TimeProvider bridge | `TimeProvider -> IClock`/`ZonedClock`/`Instant` |
|  [14]   | `NodaTime.Extensions.ClockExtensions` | clock bridge      | `IClock -> ZonedClock` (`InZone`/`InUtc`/system) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant, local, and span operations — rail: time

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `IClock.GetCurrentInstant`       | clock read         | current instant             |
|  [02]   | `SystemClock.Instance`           | clock singleton    | real clock                  |
|  [03]   | `Instant.FromUnixTime{Seconds,Milliseconds,Ticks}` | epoch conversion | external timestamp intake   |
|  [04]   | `Instant.ToUnixTime{Seconds,Milliseconds,Ticks}` / `ToUnixTimeSecondsAndNanoseconds` | epoch conversion | external timestamp output   |
|  [05]   | `Instant.InUtc`                  | zone projection    | UTC timestamp projection    |
|  [06]   | `Instant.InZone`                 | zone projection    | zoned timestamp projection  |
|  [07]   | `Instant.WithOffset`             | offset projection  | offset timestamp projection |
|  [08]   | `Instant.{Plus,Minus,Min,Max}` + `+`/`-`/`<`/`==` operators | generic-math arithmetic | `IAdditionOperators<Instant,Duration,Instant>` etc. |
|  [09]   | `LocalDate.AtStartOfDayInZone`   | zone mapping       | date boundary projection    |
|  [10]   | `LocalDateTime.InZoneStrictly`   | strict mapping     | rejects ambiguous input     |
|  [11]   | `LocalDateTime.InZoneLeniently`  | lenient mapping    | resolves ambiguous input    |
|  [12]   | `LocalDateTime.InZone(zone, resolver)` | resolver mapping | explicit mapping policy      |
|  [13]   | `Duration.From{Days,Hours,Minutes,Seconds,Milliseconds,Ticks,Nanoseconds}` | duration factory | elapsed span construction   |
|  [14]   | `Duration.Zero` / `Duration.Total{Days,Hours,Minutes,Seconds,Milliseconds,Ticks,Nanoseconds}` | duration anchor/accessor | zero (additive identity) + `double` whole-span read |
|  [15]   | `Period.From*` / `Period.Between(start, end, units)` | period factory/calc | calendar span construct + diff |
|  [16]   | `PeriodBuilder.Build`            | period builder     | mutable span finalization   |
|  [17]   | `Interval.Contains` / `DateInterval.Contains` | interval predicate | receipt-window membership   |

[ENTRYPOINT_SCOPE]: zone, calendar, and resolver operations — `Resolvers.CreateMappingResolver` is the composition combinator that fuses an ambiguous + skipped resolver into a `ZoneLocalMappingResolver` — rail: time

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]         | [RAIL]                     |
| :-----: | :------------------------------------ | :--------------------- | :------------------------- |
|  [01]   | `DateTimeZoneProviders.Tzdb`          | TZDB provider          | canonical zone lookup      |
|  [02]   | `DateTimeZoneProviders.Bcl`           | BCL provider           | BCL zone bridge            |
|  [03]   | `DateTimeZoneProviders.Serialization` | serialization provider | serialized zone lookup     |
|  [04]   | `IDateTimeZoneProvider.Ids` / `GetAllZones()` | provider inventory | zone id / zone enumeration |
|  [05]   | `IDateTimeZoneProvider.GetZoneOrNull` | optional lookup        | nullable zone lookup       |
|  [06]   | `IDateTimeZoneProvider[string]`       | required lookup        | throwing zone lookup       |
|  [07]   | `DateTimeZone.MapLocal`               | local mapping          | `ZoneLocalMapping` (ambiguous/skipped) |
|  [08]   | `DateTimeZone.ResolveLocal(local, resolver)` | resolver mapping | explicit local resolve     |
|  [09]   | `DateTimeZone.AtStrictly`             | strict mapping         | throws on gaps/overlaps    |
|  [10]   | `DateTimeZone.AtLeniently`            | lenient mapping        | resolves gaps/overlaps     |
|  [11]   | `DateTimeZone.GetZoneInterval`        | interval lookup        | offset interval at instant |
|  [12]   | `DateTimeZone.GetZoneIntervals`       | interval range         | offset intervals in span   |
|  [13]   | `Resolvers.{ReturnEarlier,ReturnLater,ThrowWhenAmbiguous}` | ambiguous resolver | duplicate-local-time policy |
|  [14]   | `Resolvers.{ReturnForwardShifted,ReturnEndOfIntervalBefore,ReturnStartOfIntervalAfter,ThrowWhenSkipped}` | skipped resolver | gap-local-time policy |
|  [15]   | `Resolvers.{StrictResolver,LenientResolver}` | prebuilt resolver | strict (throw) / lenient (earlier+forward) |
|  [16]   | `Resolvers.CreateMappingResolver(AmbiguousTimeResolver, SkippedTimeResolver)` | resolver composition | custom mapping policy |

[ENTRYPOINT_SCOPE]: text, BCL, and TimeProvider interop — rail: time

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]          | [RAIL]                     |
| :-----: | :------------------------------ | :---------------------- | :------------------------- |
|  [01]   | `InstantPattern.ExtendedIso` / `General`     | pattern singleton       | persisted instant format   |
|  [02]   | `OffsetDateTimePattern.Rfc3339` | pattern singleton       | external timestamp format  |
|  [03]   | `PeriodPattern.{Roundtrip,NormalizingIso}`   | pattern singleton       | persisted period format    |
|  [04]   | `<Pattern>.CreateWithInvariantCulture`       | pattern factory         | invariant text parsing     |
|  [05]   | `<Pattern>.CreateWithCurrentCulture`         | pattern factory         | culture-bound text parsing |
|  [06]   | `IPattern<T>.Parse` -> `ParseResult<T>`      | pattern operation       | typed parse result (`Success`/`Value`/`Exception`) |
|  [07]   | `IPattern<T>.Format`            | pattern operation       | typed text output          |
|  [08]   | `IPattern<T>.AppendFormat`      | pattern operation       | builder text output        |
|  [09]   | `<Pattern>.{WithCulture,WithTemplateValue,WithCalendar}` | pattern transform | culture/default/calendar replacement |
|  [10]   | `ZonedDateTimePattern.{WithResolver,WithZoneProvider}` | zoned pattern transform | local-mapping / zone-lookup policy |
|  [11]   | `CompositePatternBuilder<T>.{Add,Build}`     | composite pattern       | multi-format parse fallback |
|  [12]   | `OffsetDateTime.ToDateTimeOffset` / `Instant.FromDateTimeOffset` | BCL conversion | `DateTimeOffset` round-trip |
|  [13]   | `Instant.{ToDateTimeUtc,FromDateTimeUtc}`    | BCL conversion          | `DateTime`(UTC) round-trip |
|  [14]   | `Duration.ToTimeSpan` / `Duration.FromTimeSpan` | BCL conversion       | `TimeSpan` round-trip      |
|  [15]   | `TimeProvider.ToClock()` / `.GetCurrentInstant()` / `.ToZonedClock([CalendarSystem])` | TimeProvider bridge | back an `IClock`/`ZonedClock` with the BCL `TimeProvider` |
|  [16]   | `IClock.{InZone(zone[,calendar]),InUtc,InTzdbSystemDefaultZone,InBclSystemDefaultZone}` | clock->zoned bridge | build a `ZonedClock` from an `IClock` |

## [04]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime` — all implement generic-math operator interfaces (`IAdditionOperators`/`ISubtractionOperators`/`IComparisonOperators`/`IEqualityOperators`/`IMinMaxValue`) and `IXmlSerializable`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`, plus `DateAdjusters`/`TimeAdjusters`
- calendar types: `CalendarSystem` (Iso/Gregorian/Julian/Persian/Hebrew/Islamic/Coptic/Badi/UmAlQura), `YearMonth`, `IsoDayOfWeek`
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb` (canonical), `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit/composed resolver, ambiguous mapping, skipped mapping; `Resolvers.CreateMappingResolver` composes a custom policy
- text patterns: `InstantPattern`, local/offset/zoned patterns, `CompositePatternBuilder<T>`
- interop surfaces: BCL `DateTimeOffset`/`DateTime`/`TimeSpan` conversions, `NodaTime.Extensions` (`TimeProviderExtensions`, `ClockExtensions`, `DateTimeZoneProviderExtensions`), `NodaTime.Xml.XmlSerializationSettings`, `NodaTime.Text.TypeConverterSettings`

[STACKING]:
- The clock seam stacks with the BCL `TimeProvider`: `timeProvider.ToClock()` yields an `IClock` and `.ToZonedClock(zone-calendar)` yields a `ZonedClock`, so receipt timestamps come from NodaTime while elapsed-timing/delays stay `TimeProvider` work and tests substitute a `FakeTimeProvider` (or NodaTime's own `FakeClock` from `NodaTime.Testing`) at one seam. `IClock.InUtc()` / `InZone(zone)` is the idiomatic clock->`ZonedClock` composition over the bare `ZonedClock` constructor.
- Persistence stacks with the serialization companions: `NodaTime.Serialization.SystemTextJson` registers converters that emit the `InstantPattern.ExtendedIso` / `OffsetDateTimePattern.Rfc3339` forms on the `System.Text.Json` wire; `NodaTime.Serialization.Protobuf` bridges to the protobuf seam; `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` maps these types to PostgreSQL `timestamptz`/`interval`. The same pattern singletons back hand-rolled text persistence.
- Zone resolution is a single composed policy: build one `ZoneLocalMappingResolver` via `Resolvers.CreateMappingResolver(ambiguous, skipped)` (or pick `StrictResolver`/`LenientResolver`) and thread it through `LocalDateTime.InZone(zone, resolver)` and `ZonedDateTimePattern.WithResolver` — never branch per call site on ambiguity/skip.
- Generic-math interfaces let a numeric/aggregation owner constrain on `IAdditionOperators<Instant,Duration,Instant>` / `IMinMaxValue<Instant>` and fold instants/durations without type-specific arithmetic.

[LOCAL_ADMISSION]:
- Receipts store semantic instants (`Instant`), not local wall-clock values.
- Calendar and time-zone values stay explicit wherever persisted or exported; the zone seam is `DateTimeZoneProviders.Tzdb`.
- Local date-times require an explicit zone and a chosen resolver before they become instants.
- Text persistence uses invariant or roundtrip pattern singletons, not culture-ambient formatting; `ParseResult<T>` carries the typed parse failure rather than an exception at the boundary.
- Elapsed timing and delays remain `TimeProvider` work, bridged into the time rail via `TimeProviderExtensions.ToClock()`; calendar truth remains NodaTime work.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic timestamps, calendar values, zone resolution, and the `TimeProvider` clock bridge
- Accept: receipts store instants and zones; clocks are `IClock` (system, zoned, or `TimeProvider`-backed)
- Reject: local `DateTime` vocabulary; culture-ambient parsing; per-call-site ambiguity branching
