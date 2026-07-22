# [RASM_API_NODATIME]

`NodaTime` owns semantic time truth: an instant is the only persisted time value, a local calendar value reaches an instant only through an explicit zone and resolver, and every text round-trip folds through a non-throwing `ParseResult<T>`. `TimeProvider` keeps elapsed timing and delay, crossing onto this surface at one lift.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime`
- package: `NodaTime` (Apache-2.0, Jon Skeet)
- assembly: `NodaTime`
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Calendars`, `NodaTime.Extensions`, `NodaTime.HighPerformance`, `NodaTime.Xml`
- rail: time

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: time value family

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :-------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Instant`       | struct        | nanosecond global instant, generic-math operator-borne    |
|  [02]   | `Duration`      | struct        | nanosecond elapsed span, full arithmetic operator set     |
|  [03]   | `Instant64`     | struct        | long-backed instant for allocation-tight paths            |
|  [04]   | `Duration64`    | struct        | long-backed span for allocation-tight paths               |
|  [05]   | `Period`        | class         | calendar-unit span, generic-math operator-borne           |
|  [06]   | `PeriodBuilder` | class         | per-unit mutable period assembly                          |
|  [07]   | `PeriodUnits`   | enum          | unit mask `Period.Between` folds over                     |
|  [08]   | `Interval`      | struct        | half-open instant range, either end unbounded             |
|  [09]   | `DateInterval`  | class         | inclusive `LocalDate` range, enumerable and set-algebraic |
|  [10]   | `NodaConstants` | class         | unit-conversion constants and the BCL/Unix epochs         |

[ZONE_FREE_VALUES]: `LocalDate` `LocalTime` `LocalDateTime` `YearMonth` `AnnualDate`
[OFFSET_BORNE_VALUES]: `Offset` `OffsetDate` `OffsetTime` `OffsetDateTime` `ZonedDateTime`

[PUBLIC_TYPE_SCOPE]: clock zone and calendar family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `IClock`                 | interface     | current-instant seam                            |
|  [02]   | `SystemClock`            | class         | process-wide real clock                         |
|  [03]   | `ZonedClock`             | class         | clock bound to a zone and calendar              |
|  [04]   | `CalendarSystem`         | class         | calendar identity, era and month arithmetic     |
|  [05]   | `IWeekYearRule`          | interface     | week-year and week-of-year arithmetic           |
|  [06]   | `WeekYearRules`          | class         | ISO and `CalendarWeekRule`-derived week rules   |
|  [07]   | `DateAdjusters`          | class         | `LocalDate` adjuster catalogue                  |
|  [08]   | `TimeAdjusters`          | class         | `LocalTime` truncation adjusters                |
|  [09]   | `DateTimeZone`           | class         | offset behavior over instants and local values  |
|  [10]   | `DateTimeZoneProviders`  | class         | built-in provider access                        |
|  [11]   | `IDateTimeZoneProvider`  | interface     | id-keyed zone lookup contract                   |
|  [12]   | `DateTimeZoneCache`      | class         | caching provider over one source                |
|  [13]   | `IDateTimeZoneSource`    | interface     | zone data source contract                       |
|  [14]   | `TzdbDateTimeZoneSource` | class         | embedded TZDB source with Windows and CLDR maps |
|  [15]   | `ZoneInterval`           | class         | one constant-offset span                        |
|  [16]   | `ZoneLocalMapping`       | class         | zero, one, or two results for one local value   |
|  [17]   | `ZoneEqualityComparer`   | class         | option-scoped zone equality                     |
|  [18]   | `Resolvers`              | class         | predefined resolver catalogue                   |

[CALENDAR_IDENTITY]: `Era` `IsoDayOfWeek`
[MAPPING_DELEGATES]: `ZoneLocalMappingResolver` `AmbiguousTimeResolver` `SkippedTimeResolver`
[ZONE_FAULTS]: `AmbiguousTimeException` `SkippedTimeException` `DateTimeZoneNotFoundException`

[PUBLIC_TYPE_SCOPE]: text and interop family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `IPattern<T>`                | interface     | non-throwing parse and format contract    |
|  [02]   | `ParseResult<T>`             | class         | parse rail carrying value, fault, and map |
|  [03]   | `CompositePatternBuilder<T>` | class         | predicate-selected pattern composition    |
|  [04]   | `ClockExtensions`            | class         | `IClock` to `ZonedClock` binding          |
|  [05]   | `TimeProviderExtensions`     | class         | `TimeProvider` to `IClock` lift           |

[PATTERN_TYPES]: `InstantPattern` `LocalDatePattern` `LocalTimePattern` `LocalDateTimePattern` `OffsetPattern` `OffsetDatePattern` `OffsetTimePattern` `OffsetDateTimePattern` `ZonedDateTimePattern` `AnnualDatePattern` `YearMonthPattern` `DurationPattern` `PeriodPattern`
[BCL_BRIDGE_OWNERS]: `DateTimeExtensions` `DateTimeOffsetExtensions` `TimeSpanExtensions` `DateOnlyExtensions` `TimeOnlyExtensions` `DayOfWeekExtensions` `IsoDayOfWeekExtensions` `StopwatchExtensions` `DateTimeZoneProviderExtensions`
[ZONE_PROVIDER_POLICY]: `XmlSerializationSettings` `TypeConverterSettings`
[TEXT_FAULTS]: `InvalidPatternException` `UnparsableValueException`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: clock instant and span operations

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `IClock.GetCurrentInstant() -> Instant`                     | instance | sole current-time read            |
|  [02]   | `SystemClock.Instance`                                      | property | process-wide real clock           |
|  [03]   | `TimeProvider.ToClock() -> IClock`                          | static   | lifts the BCL clock onto the rail |
|  [04]   | `TimeProvider.ToZonedClock(CalendarSystem) -> ZonedClock`   | static   | zone-bound BCL clock              |
|  [05]   | `IClock.InZone(DateTimeZone, CalendarSystem) -> ZonedClock` | static   | binds zone and calendar           |
|  [06]   | `ZonedClock.GetCurrentZonedDateTime() -> ZonedDateTime`     | instance | zoned now                         |
|  [07]   | `ZonedClock.GetCurrentDate() -> LocalDate`                  | instance | today in the bound zone           |
|  [08]   | `Instant.FromDateTimeUtc(DateTime) -> Instant`              | factory  | `DateTimeKind.Utc` intake         |
|  [09]   | `Instant.FromDateTimeOffset(DateTimeOffset) -> Instant`     | factory  | offset-bearing intake             |
|  [10]   | `Instant.ToUnixTimeSecondsAndNanoseconds() -> (long, int)`  | instance | lossless epoch export             |
|  [11]   | `Instant.InUtc() -> ZonedDateTime`                          | instance | UTC projection without a lookup   |
|  [12]   | `Instant.InZone(DateTimeZone, CalendarSystem)`              | instance | zoned projection                  |
|  [13]   | `Instant.WithOffset(Offset, CalendarSystem)`                | instance | offset projection                 |
|  [14]   | `Instant.Max(Instant, Instant) -> Instant`                  | static   | saturating fold step              |
|  [15]   | `Instant64.ToInstant() -> Instant`                          | instance | widens the compact instant        |
|  [16]   | `Duration.FromNanoseconds(Int128) -> Duration`              | factory  | full-range span intake            |
|  [17]   | `Duration.ToInt128Nanoseconds() -> Int128`                  | instance | lossless span export              |
|  [18]   | `Duration.BclCompatibleTicks`                               | property | `TimeSpan`-domain tick export     |
|  [19]   | `Duration.TotalSeconds`                                     | property | fractional elapsed seconds        |
|  [20]   | `Period.Between(LocalDateTime, LocalDateTime, PeriodUnits)` | static   | unit-masked calendar difference   |
|  [21]   | `Period.Normalize() -> Period`                              | instance | canonical unit redistribution     |
|  [22]   | `Period.ToBuilder() -> PeriodBuilder`                       | instance | per-unit mutation handle          |
|  [23]   | `PeriodBuilder.Build() -> Period`                           | instance | finalizes the mutable span        |
|  [24]   | `Interval.Contains(Instant) -> bool`                        | instance | window membership                 |
|  [25]   | `Interval.Deconstruct(out Instant?, out Instant?)`          | instance | nullable-endpoint destructure     |
|  [26]   | `DateInterval.Intersection(DateInterval)`                   | instance | overlap, null when disjoint       |
|  [27]   | `DateInterval.Union(DateInterval)`                          | instance | merge, null when non-contiguous   |

- `Interval.Start`/`Interval.End`: reads throw unless the matching `HasStart`/`HasEnd` gate is true.
- `DateInterval` enumerates its own `LocalDate` members through `GetEnumerator`.

[EPOCH_INTAKE]: `Instant.FromUnixTimeSeconds(long)` `Instant.FromUnixTimeMilliseconds(long)` `Instant.FromUnixTimeTicks(long)` `Instant.FromJulianDate(double)`
[EPOCH_EXPORT]: `Instant.ToUnixTimeSeconds()` `Instant.ToUnixTimeMilliseconds()` `Instant.ToUnixTimeTicks()` `Instant.ToJulianDate()`
[DURATION_UNITS]: `Duration.FromDays` `Duration.FromHours` `Duration.FromMinutes` `Duration.FromSeconds` `Duration.FromMilliseconds` `Duration.FromTicks` `Duration.FromNanoseconds` `Duration.FromTimeSpan` `Duration.Zero`
[PERIOD_UNITS]: `Period.FromYears` `Period.FromMonths` `Period.FromWeeks` `Period.FromDays` `Period.FromHours` `Period.FromMinutes` `Period.FromSeconds` `Period.FromMilliseconds` `Period.FromTicks` `Period.FromNanoseconds` `Period.Zero`

[ENTRYPOINT_SCOPE]: zone mapping and calendar operations

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `DateTimeZoneProviders.Tzdb`                                                  | property | embedded TZDB provider         |
|  [02]   | `DateTimeZoneProviders.Bcl`                                                   | property | `TimeZoneInfo`-backed provider |
|  [03]   | `IDateTimeZoneProvider[string] -> DateTimeZone`                               | property | required id lookup             |
|  [04]   | `IDateTimeZoneProvider.GetZoneOrNull(string) -> DateTimeZone?`                | instance | optional id lookup             |
|  [05]   | `IDateTimeZoneProvider.Ids -> ReadOnlyCollection<string>`                     | property | ordinal-sorted id roster       |
|  [06]   | `IDateTimeZoneProvider.GetSystemDefault() -> DateTimeZone`                    | instance | machine zone                   |
|  [07]   | `IDateTimeZoneProvider.GetAllZones() -> IEnumerable<DateTimeZone>`            | static   | materializes every zone        |
|  [08]   | `TzdbDateTimeZoneSource.Default`                                              | property | source behind `Tzdb`           |
|  [09]   | `DateTimeZone.ForOffset(Offset) -> DateTimeZone`                              | factory  | fixed-offset zone              |
|  [10]   | `DateTimeZone.MapLocal(LocalDateTime) -> ZoneLocalMapping`                    | instance | models gap and overlap         |
|  [11]   | `DateTimeZone.ResolveLocal(LocalDateTime, ZoneLocalMappingResolver)`          | instance | applies explicit policy        |
|  [12]   | `DateTimeZone.AtStrictly(LocalDateTime) -> ZonedDateTime`                     | instance | rejects gap and overlap        |
|  [13]   | `DateTimeZone.AtLeniently(LocalDateTime) -> ZonedDateTime`                    | instance | resolves gap and overlap       |
|  [14]   | `DateTimeZone.AtStartOfDay(LocalDate) -> ZonedDateTime`                       | instance | day-boundary projection        |
|  [15]   | `DateTimeZone.GetUtcOffset(Instant) -> Offset`                                | instance | offset at an instant           |
|  [16]   | `DateTimeZone.GetZoneInterval(Instant) -> ZoneInterval`                       | instance | offset span at an instant      |
|  [17]   | `DateTimeZone.GetZoneIntervals(Interval, Options)`                            | instance | transitions across a window    |
|  [18]   | `Resolvers.CreateMappingResolver(AmbiguousTimeResolver, SkippedTimeResolver)` | static   | composes one resolver pair     |
|  [19]   | `ZoneLocalMapping.Count`                                                      | property | 0 gap, 1 unique, 2 ambiguous   |
|  [20]   | `ZonedDateTime.IsDaylightSavingTime() -> bool`                                | instance | daylight state at the value    |
|  [21]   | `ZonedDateTime.WithZone(DateTimeZone) -> ZonedDateTime`                       | instance | re-zones one instant           |
|  [22]   | `LocalDate.With(Func<LocalDate, LocalDate>) -> LocalDate`                     | fold     | applies one adjuster           |
|  [23]   | `CalendarSystem.ForId(string) -> CalendarSystem`                              | factory  | calendar by id                 |
|  [24]   | `IWeekYearRule.GetLocalDate(int, int, IsoDayOfWeek, CalendarSystem)`          | instance | week-year construction         |
|  [25]   | `WeekYearRules.ForMinDaysInFirstWeek(int) -> IWeekYearRule`                   | factory  | custom week-boundary rule      |

[LOCAL_SIDE_MAPPING]: `LocalDateTime.InZoneStrictly(DateTimeZone)` `LocalDateTime.InZoneLeniently(DateTimeZone)` `LocalDateTime.InZone(DateTimeZone, ZoneLocalMappingResolver)` `LocalDate.AtStartOfDayInZone(DateTimeZone)`
[AMBIGUOUS_RESOLVERS]: `Resolvers.ReturnEarlier` `Resolvers.ReturnLater` `Resolvers.ThrowWhenAmbiguous`
[SKIPPED_RESOLVERS]: `Resolvers.ReturnStartOfIntervalAfter` `Resolvers.ReturnEndOfIntervalBefore` `Resolvers.ReturnForwardShifted` `Resolvers.ThrowWhenSkipped`
[MAPPING_RESOLVERS]: `Resolvers.StrictResolver` `Resolvers.LenientResolver`
[DATE_ADJUSTERS]: `DateAdjusters.StartOfMonth` `DateAdjusters.EndOfMonth` `DateAdjusters.DayOfMonth` `DateAdjusters.Month` `DateAdjusters.Next` `DateAdjusters.NextOrSame` `DateAdjusters.Previous` `DateAdjusters.PreviousOrSame` `DateAdjusters.AddPeriod`
[TIME_ADJUSTERS]: `TimeAdjusters.TruncateToSecond` `TimeAdjusters.TruncateToMinute` `TimeAdjusters.TruncateToHour`
[CALENDARS]: `CalendarSystem.Iso` `CalendarSystem.Gregorian` `CalendarSystem.Julian` `CalendarSystem.Coptic` `CalendarSystem.Badi` `CalendarSystem.HebrewCivil` `CalendarSystem.HebrewScriptural` `CalendarSystem.IslamicBcl` `CalendarSystem.UmAlQura` `CalendarSystem.PersianSimple` `CalendarSystem.PersianArithmetic` `CalendarSystem.PersianAstronomical`

[ENTRYPOINT_SCOPE]: text and interop operations

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `IPattern<T>.Parse(string) -> ParseResult<T>`                  | instance | non-throwing parse                  |
|  [02]   | `IPattern<T>.Format(T) -> string`                              | instance | typed render                        |
|  [03]   | `IPattern<T>.AppendFormat(T, StringBuilder) -> StringBuilder`  | instance | append without an intermediate      |
|  [04]   | `ParseResult<T>.Success`                                       | property | rail discriminant                   |
|  [05]   | `ParseResult<T>.TryGetValue(T, out T) -> bool`                 | instance | fallback fold                       |
|  [06]   | `ParseResult<T>.GetValueOrThrow() -> T`                        | instance | throwing extraction                 |
|  [07]   | `ParseResult<T>.Convert<TTarget>(Func<T, TTarget>)`            | instance | maps the value arm                  |
|  [08]   | `ParseResult<T>.ConvertError<TTarget>()`                       | instance | re-tags the fault arm               |
|  [09]   | `ZonedDateTimePattern.Create`                                  | factory  | full zoned-pattern mint             |
|  [10]   | `LocalDatePattern.WithCalendar(CalendarSystem)`                | instance | swaps calendar                      |
|  [11]   | `ZonedDateTimePattern.WithResolver(ZoneLocalMappingResolver)`  | instance | swaps mapping policy                |
|  [12]   | `ZonedDateTimePattern.WithZoneProvider(IDateTimeZoneProvider)` | instance | swaps zone lookup                   |
|  [13]   | `CompositePatternBuilder<T>.Add(IPattern<T>, Func<T, bool>)`   | instance | registers a format predicate        |
|  [14]   | `CompositePatternBuilder<T>.Build() -> IPattern<T>`            | instance | folds the set to one pattern        |
|  [15]   | `Stopwatch.ElapsedDuration() -> Duration`                      | static   | measured elapsed intake             |
|  [16]   | `XmlSerializationSettings.DateTimeZoneProvider`                | property | zone provider XML round-trip binds  |
|  [17]   | `TypeConverterSettings.DateTimeZoneProvider`                   | property | zone provider `TypeConverter` binds |

A pattern type mints through its own static family and reconfigures through instance transforms returning a new pattern, each transform riding the types whose fields admit it.

- `ZonedDateTimePattern`: every mint takes an `IDateTimeZoneProvider`, and `Create` takes `(string, CultureInfo, ZoneLocalMappingResolver, IDateTimeZoneProvider, ZonedDateTime)`.
- `PeriodPattern`: singletons only, carrying no mint or transform family.

[PATTERN_MINTS]: `CreateWithInvariantCulture(string)` `CreateWithCurrentCulture(string)` `Create(string, CultureInfo)` `Create(string, CultureInfo, T)`
[PATTERN_TRANSFORMS]: `WithCulture(CultureInfo)` `WithTemplateValue(T)` `WithTwoDigitYearMax(int)` `WithPatternText(string)`
[ROUNDTRIP_PATTERNS]: `InstantPattern.ExtendedIso` `LocalDatePattern.Iso` `LocalTimePattern.ExtendedIso` `LocalDateTimePattern.ExtendedIso` `OffsetDateTimePattern.Rfc3339` `OffsetDatePattern.GeneralIso` `OffsetTimePattern.ExtendedIso` `OffsetTimePattern.GeneralIso` `OffsetPattern.GeneralInvariant` `OffsetPattern.GeneralInvariantWithZ` `AnnualDatePattern.Iso` `YearMonthPattern.Iso` `DurationPattern.Roundtrip` `DurationPattern.JsonRoundtrip` `PeriodPattern.Roundtrip`
[BCL_INTAKE]: `DateTime.ToInstant` `DateTime.ToLocalDateTime` `DateTimeOffset.ToInstant` `DateTimeOffset.ToOffsetDateTime` `DateTimeOffset.ToZonedDateTime` `TimeSpan.ToDuration` `TimeSpan.ToOffset` `DateOnly.ToLocalDate` `TimeOnly.ToLocalTime` `DayOfWeek.ToIsoDayOfWeek`
[BCL_EXPORT]: `Instant.ToDateTimeUtc` `Instant.ToDateTimeOffset` `ZonedDateTime.ToDateTimeOffset` `ZonedDateTime.ToDateTimeUtc` `OffsetDateTime.ToDateTimeOffset` `LocalDate.ToDateOnly` `LocalTime.ToTimeOnly` `LocalDateTime.ToDateTimeUnspecified` `Duration.ToTimeSpan` `Offset.ToTimeSpan` `IsoDayOfWeek.ToDayOfWeek`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- An instant is the only persisted time value; every local, offset, and zoned form projects from it or resolves back to it.
- A local value reaches an instant only through an explicit `DateTimeZone` and a `ZoneLocalMappingResolver`, so a gap or an overlap is a decided outcome.
- Every parse folds through `ParseResult<T>`; text throws only where the caller reaches `GetValueOrThrow`.
- `CalendarSystem` travels inside the value, so a calendar change is a projection on the value rather than ambient state.

[STACKING]:
- `NodaTime.Serialization.SystemTextJson`(`.api/api-nodatime-stj.md`): `ConfigureForNodaTime` registers pattern-backed converters on `JsonSerializerOptions`, each binding one `IPattern<T>` singleton from this surface, and the zoned factories take the `IDateTimeZoneProvider` chosen here.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Instant`/`Duration` project onto the well-known `Timestamp`/`Duration` messages and `LocalDate`/`LocalTime`/`IsoDayOfWeek` onto the common `Date`/`TimeOfDay`/`DayOfWeek` messages, `NodaConstants.BclEpoch` bounding the outbound range.
- Text rail: an `IPattern<T>` configures through `WithCulture`, `WithTemplateValue`, `WithCalendar`, `WithResolver`, and `WithZoneProvider`, `CompositePatternBuilder<T>` folds a predicate-selected set of them into one pattern, and results chain through `ParseResult<T>.Convert` before landing on the typed rail at `TryGetValue`.
- Clock rail: `TimeProvider.ToClock` lifts the BCL clock onto `IClock`, `ClockExtensions.InZone` binds zone and calendar into a `ZonedClock`, and `Resolvers.CreateMappingResolver` supplies the mapping policy every local-to-zoned projection under that clock takes.
- Arithmetic rail: `Instant`, `Duration`, and `Period` carry the `System.Numerics` operator interfaces, so each slots into a generic-constrained numeric fold with `IMinMaxValue` saturation, and `NodaTime.HighPerformance` supplies the long-backed `Instant64`/`Duration64` pair converting through `ToInstant`/`FromInstant`.

[LOCAL_ADMISSION]:
- Receipts store instants; a persisted or exported value carries its calendar and zone explicitly.
- `DateTimeZoneProviders.Tzdb` is the zone source, its TZDB data embedded in the assembly.
- Text persistence binds a roundtrip or invariant pattern singleton.
- `TimeProvider` owns elapsed timing and delay, and `ToClock` is the one crossing onto calendar truth.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic instants, calendar values, zone mapping, and typed time text
- Accept: instant-keyed receipts, resolver-decided local-to-zoned projection, `ParseResult<T>` parse folds
- Reject: `DateTime` wall-clock vocabulary and hand-rolled epoch arithmetic
