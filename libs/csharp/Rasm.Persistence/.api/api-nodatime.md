# [RASM_PERSISTENCE_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods, durations,
intervals, clocks, resolvers, and text patterns for AppHost receipts, op-log HLC `Physical` stamps,
health snapshots, support windows, and persisted clock facts. Its value types implement the
generic-math operator interfaces, so they compose into arithmetic constraints; its `Extensions`
namespace bridges every BCL `DateTimeOffset`/`TimeSpan`/`IClock` seam; and its wire projections stack
onto the sibling `NodaTime.Serialization.SystemTextJson`, `Npgsql ... NodaTime`, and the snapshot
MessagePack `InstantFormatter` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime` (3.3.2)
- assembly: `NodaTime` (binds `lib/net8.0`; the package multi-targets `net8.0`/`netstandard2.0` and the `net10.0` consumer takes `net8.0` — the asset carrying the generic-math interfaces)
- namespace: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`
- license: Apache-2.0
- asset: runtime library (embeds TZDB)
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

`Instant` and `Duration` implement the System.Numerics generic-math interfaces —
`IAdditionOperators<Instant,Duration,Instant>`, `ISubtractionOperators` (instant−instant→duration),
`IComparisonOperators`, `IEqualityOperators`, and `IMinMaxValue` — so an `Instant`/`Duration` slots
into a generic-constrained numeric algorithm and into `MinValue`/`MaxValue` saturation directly.

[PUBLIC_TYPE_SCOPE]: zone calendar and clock family
- rail: time

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [RAIL]                   |
| :-----: | :----------------------- | :----------------- | :----------------------- |
|  [01]   | `IClock`                 | clock contract     | current instant seam     |
|  [02]   | `SystemClock`            | clock provider     | real clock               |
|  [03]   | `ZonedClock`             | zoned clock        | clock plus zone+calendar |
|  [04]   | `CalendarSystem`         | calendar provider  | calendar identity        |
|  [05]   | `IsoDayOfWeek`           | weekday enum       | schedule identity        |
|  [06]   | `DateTimeZone`           | zone model         | zone mapping             |
|  [07]   | `DateTimeZoneProviders`  | provider registry  | zone provider access     |
|  [08]   | `IDateTimeZoneProvider`  | provider contract  | zone lookup              |
|  [09]   | `DateTimeZoneCache`      | provider cache     | zone source cache        |
|  [10]   | `TzdbDateTimeZoneSource`  | TZDB source        | zone data source         |
|  [11]   | `ZoneInterval`           | zone interval      | offset transition span   |
|  [12]   | `ZoneLocalMapping`       | local mapping      | ambiguous/skipped result |
|  [13]   | `Resolvers`              | resolver holder    | predefined resolver set  |
|  [14]   | `ZoneLocalMappingResolver` | resolver delegate | local→zoned strategy     |
|  [15]   | `AmbiguousTimeResolver`  | resolver delegate  | duplicate-time strategy  |
|  [16]   | `SkippedTimeResolver`    | resolver delegate  | gap-time strategy        |
|  [17]   | `AmbiguousTimeException` | mapping exception  | duplicate local time     |
|  [18]   | `SkippedTimeException`   | mapping exception  | skipped local time       |

[PUBLIC_TYPE_SCOPE]: text + BCL-interop family (`.Text`, `.Extensions`, `.Xml`)
- rail: time

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------- | :---------------- | :------------------------ |
|  [01]   | `IPattern<T>`              | pattern contract  | parse/format abstraction  |
|  [02]   | `ParseResult<T>`           | parse result      | `Success`/`Value`/`Exception` rail |
|  [03]   | `InstantPattern`           | instant pattern   | persisted instant text    |
|  [04]   | `LocalDatePattern`         | date pattern      | calendar text             |
|  [05]   | `LocalDateTimePattern`     | timestamp pattern | local timestamp text      |
|  [06]   | `OffsetDateTimePattern`    | offset pattern    | offset timestamp text     |
|  [07]   | `ZonedDateTimePattern`     | zone pattern      | zoned timestamp text      |
|  [08]   | `DurationPattern`          | duration pattern  | elapsed span text         |
|  [09]   | `PeriodPattern`            | period pattern    | calendar span text        |
|  [10]   | `OffsetPattern`            | offset pattern    | offset-only text (`IPattern<Offset>`) |
|  [11]   | `AnnualDatePattern`        | annual-date pattern | recurring month-day text (`IPattern<AnnualDate>`) |
|  [12]   | `YearMonthPattern`         | year-month pattern | calendar-month text (`IPattern<YearMonth>`) |
|  [13]   | `LocalTimePattern`         | local-time pattern | time-only text (`IPattern<LocalTime>`) |
|  [14]   | `OffsetDatePattern`        | offset-date pattern | date+offset text (`IPattern<OffsetDate>`) |
|  [15]   | `OffsetTimePattern`        | offset-time pattern | time+offset text (`IPattern<OffsetTime>`) |
|  [16]   | `DateTimeOffsetExtensions` | BCL bridge        | `ToInstant`/`ToOffsetDateTime`/`ToZonedDateTime` |
|  [17]   | `TimeSpanExtensions`       | BCL bridge        | `ToDuration`/`ToOffset`   |
|  [18]   | `DurationExtensions`       | BCL bridge        | `ToTimeSpan`              |
|  [19]   | `ClockExtensions`          | clock bridge      | `InZone`/`InUtc`/`InTzdbSystemDefaultZone` → `ZonedClock` |
|  [20]   | `XmlSerializationSettings` | XML policy        | `NodaTime.Xml` projection admission |

`NodaTime.Text.TypeConverterSettings` admits a `DateTimeZoneProvider` for the `TypeConverter`
projection. There is no `Instant64`/`Duration64` type in this package — the compact-instant surface
is the single nanosecond-resolution `Instant`/`Duration`; do not catalog phantom 64-bit variants.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instant, span, and clock operations
- rail: time

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]     | [RAIL]                      |
| :-----: | :------------------------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `IClock.GetCurrentInstant` / `SystemClock.Instance` | clock read         | current instant seam        |
|  [02]   | `IClock.InZone(zone[, calendar])` / `.InUtc()` / `.InTzdbSystemDefaultZone()` | clock bridge | `IClock` → `ZonedClock` |
|  [03]   | `ZonedClock.GetCurrentZonedDateTime`/`...OffsetDateTime`/`...LocalDateTime`/`...Date`/`...TimeOfDay` | zoned read | calendar-aware current value |
|  [04]   | `Instant.FromUnixTime{Seconds,Milliseconds,Ticks}` / `.ToUnixTime*` | epoch conversion   | external timestamp intake/output |
|  [05]   | `Instant.FromDateTimeOffset` / `.ToDateTimeOffset` / `.FromDateTimeUtc` / `.ToDateTimeUtc` | BCL conversion | DateTime boundary mapping |
|  [06]   | `Instant.InUtc` / `.InZone(zone[, calendar])` / `.WithOffset(offset[, calendar])` | projection | UTC/zoned/offset timestamp projection |
|  [07]   | `Instant.{Min,Max}(x,y)` / `Instant.{MinValue,MaxValue}` | saturation        | clamp / sentinel bounds     |
|  [08]   | `Duration.From{Days,Hours,Minutes,Seconds,Milliseconds,Ticks,Nanoseconds}` | duration factory   | elapsed span construction   |
|  [09]   | `Period.From{Years,Months,Weeks,Days,Hours,...,Nanoseconds}` / `.Normalize` / `.ToDuration` | period factory | calendar span construction |
|  [10]   | `Period.Between(start, end [, PeriodUnits])` / `PeriodBuilder.Build` | period calculation | calendar span difference / mutable finalisation |
|  [11]   | `Interval.Contains` / `DateInterval.Contains` / `.Intersection` | interval predicate | receipt-window membership   |

[ENTRYPOINT_SCOPE]: zone, calendar, and resolver operations
- rail: time

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]   | [RAIL]                     |
| :-----: | :------------------------------------------------------- | :--------------- | :------------------------- |
|  [01]   | `DateTimeZoneProviders.{Tzdb,Bcl,Serialization}`         | provider access  | canonical / BCL / serialized zone source |
|  [02]   | `IDateTimeZoneProvider.Ids` / `.GetZoneOrNull(id)` / `this[id]` | zone lookup | enumerate / nullable / throwing lookup (one polymorphic id→zone surface) |
|  [03]   | `DateTimeZone.{Utc, ForOffset(offset)}`                  | fixed zone       | UTC / fixed-offset zone    |
|  [04]   | `DateTimeZone.MapLocal(local)` → `ZoneLocalMapping`      | local mapping    | unambiguous/ambiguous/skipped model |
|  [05]   | `DateTimeZone.ResolveLocal(local, ZoneLocalMappingResolver)` | resolver mapping | explicit local resolve     |
|  [06]   | `DateTimeZone.AtStrictly(local)` / `.AtLeniently(local)` | strict/lenient   | throw on gap/overlap / always-resolve |
|  [07]   | `DateTimeZone.AtStartOfDay(date)` / `LocalDate.AtStartOfDayInZone(zone)` | day boundary | midnight-in-zone (gap-safe) |
|  [08]   | `DateTimeZone.GetZoneInterval(instant)` / `.GetZoneIntervals(interval)` / `.GetUtcOffset(instant)` | offset lookup | transition span(s) / wall offset at instant |
|  [09]   | `LocalDateTime.InZoneStrictly` / `.InZoneLeniently` / `.InZone(zone, resolver)` | local→zoned | strict / lenient / explicit policy |
|  [10]   | `Resolvers.{ReturnEarlier,ReturnLater,ThrowWhenAmbiguous}` | ambiguous resolver | duplicate-time strategy   |
|  [11]   | `Resolvers.{ReturnStartOfIntervalAfter,ReturnEndOfIntervalBefore,ReturnForwardShifted,ThrowWhenSkipped}` | skipped resolver | gap-time strategy |
|  [12]   | `Resolvers.{StrictResolver,LenientResolver}` / `Resolvers.CreateMappingResolver(ambiguous, skipped)` | composite resolver | prebuilt / compose-from-parts |

[ENTRYPOINT_SCOPE]: text + interop operations
- rail: time

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]   | [RAIL]                     |
| :-----: | :--------------------------------------------------------- | :--------------- | :------------------------- |
|  [01]   | `InstantPattern.{ExtendedIso,General}`                     | pattern singleton | persisted instant format   |
|  [02]   | `OffsetDateTimePattern.{Rfc3339,GeneralIso,ExtendedIso,FullRoundtrip}` | pattern singleton | external/roundtrip timestamp format |
|  [03]   | `PeriodPattern.{Roundtrip,NormalizingIso}` / `DurationPattern.{Roundtrip,JsonRoundtrip}` | pattern singleton | persisted span format |
|  [04]   | `OffsetPattern.{GeneralInvariant,GeneralInvariantWithZ}` / `AnnualDatePattern.Iso` / `YearMonthPattern.Iso` / `LocalTimePattern.ExtendedIso` / `OffsetDatePattern.GeneralIso` / `OffsetTimePattern.{GeneralIso,ExtendedIso}` | pattern singleton | per-value-type pattern singletons the temporal codecs bind (offset-only `Z`-suffix / recurring month-day / calendar-month / time-only / date+offset / time+offset) |
|  [05]   | `<Pattern>.CreateWithInvariantCulture(text)` / `.CreateWithCurrentCulture(text)` / `.Create(text, culture)` | pattern factory | invariant / culture-bound parsing |
|  [06]   | `IPattern<T>.Parse(text)` → `ParseResult<T>` / `.Format(value)` / `.AppendFormat(value, builder)` | pattern op | typed parse result / text output / builder append |
|  [07]   | `ParseResult<T>.{Success, Value, GetValueOrThrow(), TryGetValue(...)}` | parse rail | success-flag / value / throwing / try projection |
|  [08]   | `<Pattern>.{WithCulture, WithTemplateValue, WithCalendar}` | pattern transform | culture / default-value / calendar replacement |
|  [09]   | `ZonedDateTimePattern.WithResolver(ZoneLocalMappingResolver)` / `.WithZoneProvider(IDateTimeZoneProvider)` | zoned pattern transform | local-mapping + zone-lookup policy |
|  [10]   | `DateTimeOffsetExtensions.{ToInstant,ToOffsetDateTime,ToZonedDateTime}` / `TimeSpanExtensions.{ToDuration,ToOffset}` / `DurationExtensions.ToTimeSpan` | BCL conversion | DateTimeOffset/TimeSpan ↔ Noda value |

## [04]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.Xml`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime` (all generic-math operator-bearing)
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`, `YearMonth`
- calendar types: `CalendarSystem`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb` (default, embedded TZDB), `.Bcl`, `.Serialization`
- zone mapping: `MapLocal`/`ResolveLocal` returning `ZoneLocalMapping`; `AtStrictly`/`AtLeniently`; the three resolver delegate types composed by `Resolvers.CreateMappingResolver`
- text patterns: per-type `IPattern<T>` singletons + `Create*` factories returning a `ParseResult<T>` rail (never an exception on `Parse`)
- interop surfaces: `Extensions` BCL bridges, `XmlSerializationSettings`, `Text.TypeConverterSettings`

[LOCAL_ADMISSION]:
- Receipts and op-log `Physical` stamps store semantic `Instant` values, never local wall-clock `DateTime`.
- Calendar and time-zone values stay explicit wherever persisted or exported; a `LocalDateTime` requires an explicit zone and a `ZoneLocalMappingResolver` before it becomes an `Instant` — gap/ambiguity handling is a stated policy, not a default coercion.
- Text persistence uses invariant or roundtrip patterns (`InstantPattern.ExtendedIso`, `OffsetDateTimePattern.Rfc3339`, `PeriodPattern.Roundtrip`), never culture-ambient formatting; inbound parse reads the `ParseResult<T>` rail and lifts `.Success=false` into a typed boundary failure.
- Wire stacking is delegated, not re-derived here: STJ projection rides `NodaTime.Serialization.SystemTextJson` (`api-nodatime-stj`) registered on `PersistenceWireContext`; the Postgres store type rides `Npgsql ... NodaTime` (`api-npgsql-ef-nodatime`) mapping `Instant`→`timestamptz`; the MessagePack snapshot codec maps `Instant` through the page-owned `InstantFormatter : IMessagePackFormatter<Instant>` on the `Version/snapshots#CODEC_AXIS` resolver chain — this package supplies the value type and its `ToUnixTimeTicks`/`FromUnixTimeTicks` round-trip, the formatter rows live with their codecs.
- Elapsed timing and delays remain `TimeProvider` work; calendar truth and zone mapping remain NodaTime work. Tests bind the deterministic `FakeClock`/`IClock` seam from `NodaTime.Testing`, never `SystemClock` directly.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic timestamps, calendar values, zone mapping, and the parse/format pattern rail
- Accept: receipts store `Instant`/zone; local→instant declares an explicit resolver; parse reads `ParseResult<T>`
- Reject: local `DateTime` vocabulary, culture-ambient formatting, implicit ambiguous/skipped coercion
