# [RASM_APPHOST_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods,
durations, intervals, clocks, resolvers, and text patterns for AppHost receipts,
health snapshots, support windows, and persisted clock facts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime`
- assembly: `NodaTime` (binds `lib/net8.0`; multi-targets `net8.0`/`netstandard2.0`, the `net10.0` consumer takes `net8.0` — the asset carrying the generic-math operator interfaces)
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`
- license: Apache-2.0
- asset: runtime library (embeds TZDB)
- rail: time

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: time value family
- rail: time

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:--------------- |:----------------- |:-------------------- |
| [01] | `Instant` | global instant | receipt timestamp |
| [02] | `OffsetDateTime` | offset timestamp | exported timestamp |
| [03] | `ZonedDateTime` | zoned timestamp | user-facing timestamp |
| [04] | `LocalDate` | calendar date | date-only policy |
| [05] | `LocalTime` | local time | time-only policy |
| [06] | `LocalDateTime` | local timestamp | zone-mapped input |
| [07] | `YearMonth` | calendar month | period boundary |
| [08] | `AnnualDate` | recurring date | annual schedule |
| [09] | `Offset` | UTC offset | offset identity |
| [10] | `OffsetDate` | date plus offset | offset date payload |
| [11] | `OffsetTime` | time plus offset | offset time payload |
| [12] | `Duration` | fixed elapsed span | elapsed quantity |
| [13] | `Period` | calendar span | calendar quantity |
| [14] | `PeriodBuilder` | mutable period | period construction |
| [15] | `Interval` | instant interval | receipt window |
| [16] | `DateInterval` | calendar interval | date window |

`Instant` and `Duration` implement the `System.Numerics` generic-math operator interfaces — `IAdditionOperators<Instant,Duration,Instant>`, `ISubtractionOperators` (instant−instant→duration), `IComparisonOperators`, `IEqualityOperators`, and `IMinMaxValue` — so an `Instant`/`Duration` slots into a generic-constrained numeric algorithm and into `MinValue`/`MaxValue` saturation directly.

[PUBLIC_TYPE_SCOPE]: zone calendar and clock family
- rail: time

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:----------------------- |:---------------- |:----------------------- |
| [01] | `IClock` | clock contract | current instant seam |
| [02] | `SystemClock` | clock provider | real clock |
| [03] | `ZonedClock` | zoned clock | clock plus zone |
| [04] | `CalendarSystem` | calendar provider | calendar identity |
| [05] | `IsoDayOfWeek` | weekday enum | schedule identity |
| [06] | `DateTimeZone` | zone model | zone mapping |
| [07] | `DateTimeZoneProviders` | provider registry | zone provider access |
| [08] | `IDateTimeZoneProvider` | provider contract | zone lookup |
| [09] | `DateTimeZoneCache` | provider cache | zone source cache |
| [10] | `TzdbDateTimeZoneSource` | TZDB source | zone data source |
| [11] | `ZoneInterval` | zone interval | offset transition span |
| [12] | `ZoneLocalMapping` | local mapping | ambiguous/skipped result |
| [13] | `Resolvers` | resolver holder | predefined resolver set |
| [14] | `ZoneLocalMappingResolver` | resolver delegate | local→zoned strategy |
| [15] | `AmbiguousTimeResolver` | resolver delegate | duplicate-time strategy |
| [16] | `SkippedTimeResolver` | resolver delegate | gap-time strategy |
| [17] | `AmbiguousTimeException` | mapping exception | duplicate local time |
| [18] | `SkippedTimeException` | mapping exception | skipped local time |

[PUBLIC_TYPE_SCOPE]: text + BCL-interop family (`.Text`, `.Extensions`, `.Xml`)
- rail: time

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:------------------------- |:---------------- |:------------------------ |
| [01] | `IPattern<T>` | pattern contract | parse/format abstraction |
| [02] | `ParseResult<T>` | parse result | `Success`/`Value`/`Exception` rail |
| [03] | `InstantPattern` | instant pattern | persisted instant text |
| [04] | `LocalDatePattern` | date pattern | calendar text |
| [05] | `LocalDateTimePattern` | timestamp pattern | local timestamp text |
| [06] | `OffsetDateTimePattern` | offset pattern | offset timestamp text |
| [07] | `ZonedDateTimePattern` | zone pattern | zoned timestamp text |
| [08] | `DurationPattern` | duration pattern | elapsed span text |
| [09] | `PeriodPattern` | period pattern | calendar span text |
| [10] | `OffsetPattern` | offset pattern | offset-only text (`IPattern<Offset>`) |
| [11] | `AnnualDatePattern` | annual-date pattern | recurring month-day text (`IPattern<AnnualDate>`) |
| [12] | `YearMonthPattern` | year-month pattern | calendar-month text (`IPattern<YearMonth>`) |
| [13] | `LocalTimePattern` | local-time pattern | time-only text (`IPattern<LocalTime>`) |
| [14] | `OffsetDatePattern` | offset-date pattern | date+offset text (`IPattern<OffsetDate>`) |
| [15] | `OffsetTimePattern` | offset-time pattern | time+offset text (`IPattern<OffsetTime>`) |
| [16] | `DateTimeOffsetExtensions` | BCL bridge | `ToInstant`/`ToOffsetDateTime`/`ToZonedDateTime` |
| [17] | `TimeSpanExtensions` | BCL bridge | `ToDuration`/`ToOffset` |
| [18] | `DurationExtensions` | BCL bridge | `ToTimeSpan` |
| [19] | `ClockExtensions` | clock bridge | `InZone`/`InUtc`/`InTzdbSystemDefaultZone` → `ZonedClock` |
| [20] | `XmlSerializationSettings` | XML policy | `NodaTime.Xml` projection admission |

`NodaTime.Text.TypeConverterSettings` admits a `DateTimeZoneProvider` for the `TypeConverter` projection. There is no `Instant64`/`Duration64` type in this package — the compact-instant surface is the single nanosecond-resolution `Instant`/`Duration`; do not catalog phantom 64-bit variants.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant local and span operations
- rail: time

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:------------------------------- |:----------------- |:-------------------------- |
| [01] | `IClock.GetCurrentInstant` | clock read | current instant |
| [02] | `SystemClock.Instance` | clock singleton | real clock |
| [03] | `Instant.FromUnixTime*` | epoch conversion | external timestamp intake |
| [04] | `Instant.ToUnixTime*` | epoch conversion | external timestamp output |
| [05] | `Instant.FromDateTimeUtc(DateTime)` | BCL intake | `DateTimeKind.Utc` timestamp intake |
| [06] | `Instant.FromDateTimeOffset(DateTimeOffset)` | BCL intake | offset-bearing timestamp intake |
| [07] | `Instant.InUtc` | zone projection | UTC timestamp projection |
| [08] | `Instant.InZone` | zone projection | zoned timestamp projection |
| [09] | `Instant.WithOffset` | offset projection | offset timestamp projection |
| [10] | `LocalDate.AtStartOfDayInZone` | zone mapping | date boundary projection |
| [11] | `LocalDateTime.InZoneStrictly` | strict mapping | rejects ambiguous input |
| [12] | `LocalDateTime.InZoneLeniently` | lenient mapping | resolves ambiguous input |
| [13] | `LocalDateTime.InZone(resolver)` | resolver mapping | explicit mapping policy |
| [14] | `Duration.From*` | duration factory | elapsed span construction |
| [15] | `Duration.Zero` | duration anchor | empty elapsed span (`default`) |
| [16] | `Period.From*` | period factory | calendar span construction |
| [17] | `Period.Between` | period calculation | calendar span difference |
| [18] | `PeriodBuilder.Build` | period builder | mutable span finalization |
| [19] | `Interval.Contains` | interval predicate | receipt-window membership |

[ENTRYPOINT_SCOPE]: zone calendar and resolver operations
- rail: time

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:------------------------------------ |:--------------------- |:------------------------- |
| [01] | `DateTimeZoneProviders.Tzdb` | TZDB provider | canonical zone lookup |
| [02] | `DateTimeZoneProviders.Bcl` | BCL provider | BCL zone bridge |
| [03] | `DateTimeZoneProviders.Serialization` | serialization provider | serialized zone lookup |
| [04] | `IDateTimeZoneProvider.Ids` | provider inventory | zone id enumeration |
| [05] | `GetZoneOrNull` | optional lookup | nullable zone lookup |
| [06] | `Item[string]` | required lookup | throwing zone lookup |
| [07] | `DateTimeZone.MapLocal` | local mapping | ambiguous/skipped model |
| [08] | `ResolveLocal` | resolver mapping | explicit local resolve |
| [09] | `AtStrictly` | strict mapping | throws on gaps/overlaps |
| [10] | `AtLeniently` | lenient mapping | resolves gaps/overlaps |
| [11] | `GetZoneInterval` | interval lookup | offset interval at instant |
| [12] | `GetZoneIntervals` | interval range | offset intervals in span |
| [13] | `Resolvers.ReturnEarlier` | ambiguous resolver | earlier duplicate time |
| [14] | `Resolvers.ReturnLater` | ambiguous resolver | later duplicate time |
| [15] | `Resolvers.ThrowWhenAmbiguous` | ambiguous resolver | rejects duplicate time |
| [16] | `Resolvers.{ReturnStartOfIntervalAfter,ReturnEndOfIntervalBefore,ReturnForwardShifted,ThrowWhenSkipped}` | skipped resolver | gap-time strategy |
| [17] | `Resolvers.StrictResolver` | mapping resolver | rejects non-unique time |
| [18] | `Resolvers.LenientResolver` | mapping resolver | resolves non-unique time |
| [19] | `Resolvers.CreateMappingResolver(ambiguous, skipped)` | composite resolver | compose a `ZoneLocalMappingResolver` from the two delegates |

[ENTRYPOINT_SCOPE]: text and interop operations
- rail: time

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:------------------------------ |:---------------------- |:------------------------- |
| [01] | `InstantPattern.ExtendedIso` | pattern singleton | persisted instant format |
| [02] | `OffsetDateTimePattern.Rfc3339` | pattern singleton | external timestamp format |
| [03] | `PeriodPattern.Roundtrip` | pattern singleton | persisted period format |
| [04] | `DurationPattern.{Roundtrip,JsonRoundtrip}` | pattern singleton | persisted duration format |
| [05] | `OffsetPattern.{GeneralInvariant,GeneralInvariantWithZ}` / `AnnualDatePattern.Iso` / `YearMonthPattern.Iso` / `LocalTimePattern.ExtendedIso` / `OffsetDatePattern.GeneralIso` / `OffsetTimePattern.{GeneralIso,ExtendedIso}` | pattern singleton | per-value-type pattern singletons the temporal codecs bind |
| [06] | `CreateWithInvariantCulture` | pattern factory | invariant text parsing |
| [07] | `CreateWithCurrentCulture` | pattern factory | culture-bound text parsing |
| [08] | `Parse` | pattern operation | typed parse result |
| [09] | `Format` | pattern operation | typed text output |
| [10] | `AppendFormat` | pattern operation | builder text output |
| [11] | `WithCulture` | pattern transform | culture replacement |
| [12] | `WithTemplateValue` | pattern transform | default value replacement |
| [13] | `WithResolver` | zoned pattern transform | local mapping policy |
| [14] | `WithZoneProvider` | zoned pattern transform | zone lookup policy |
| [15] | `WithCalendar` | pattern transform | calendar replacement |
| [16] | `ToInstant` | BCL conversion | BCL timestamp intake |
| [17] | `ToDateTimeOffset` | BCL conversion | BCL timestamp output |
| [18] | `ToDuration` | BCL conversion | elapsed span intake |
| [19] | `ToTimeSpan` | BCL conversion | elapsed span output |

## [04]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime` (all generic-math operator-bearing)
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`, `YearMonth`
- calendar types: `CalendarSystem`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb` (default, embedded TZDB), `.Bcl`, `.Serialization`
- zone mapping: `MapLocal`/`ResolveLocal` returning `ZoneLocalMapping`; `AtStrictly`/`AtLeniently`; the three resolver delegate types (`ZoneLocalMappingResolver`/`AmbiguousTimeResolver`/`SkippedTimeResolver`) composed by `Resolvers.CreateMappingResolver`
- text patterns: per-type `IPattern<T>` singletons + `Create*` factories returning a `ParseResult<T>` rail (never an exception on `Parse`)
- interop surfaces: `Extensions` BCL bridges, `XmlSerializationSettings`, `Text.TypeConverterSettings`

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
