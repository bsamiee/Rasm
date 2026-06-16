# [RASM_APPHOST_API_NODATIME_STJ]

`NodaTime.Serialization.SystemTextJson` wires NodaTime types into `JsonSerializerOptions`
via a per-type converter list. The entry point is the `Extensions.ConfigureForNodaTime`
extension on `JsonSerializerOptions`; `NodaJsonSettings` is the mutable property bag that
holds the per-type converter assignments; `NodaConverters` supplies the pre-built singletons
and provider-bound factories.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- namespace: `NodaTime.Serialization.SystemTextJson`
- asset: runtime library (net6.0 / netstandard2.0)
- rail: wire (JSON codec registration)

## [2]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [RAIL]                            |
| :-----: | :--------------------------- | :------------------- | :-------------------------------- |
|   [1]   | `Extensions`                 | static extension     | options registration entry point  |
|   [2]   | `NodaJsonSettings`           | mutable property bag | per-type converter assignment     |
|   [3]   | `NodaConverters`             | static factory class | pre-built converter singletons    |
|   [4]   | `NodaConverterBase<T>`       | abstract base        | nullity + CanConvert boilerplate  |
|   [5]   | `NodaPatternConverter<T>`    | pattern-backed       | IPattern<T> read/write + validate |
|   [6]   | `DelegatingConverterBase<T>` | delegation base      | per-property attribute dispatch   |
|   [7]   | `NodaNullableConverter<T>`   | nullable wrapper     | nullable struct bridging          |
|   [8]   | `NodaDateTimeZoneConverter`  | zone converter       | DateTimeZone via provider lookup  |
|   [9]   | `NodaIntervalConverter`      | interval converter   | Interval as start/end sub-object  |
|  [10]   | `NodaIsoIntervalConverter`   | interval converter   | Interval as ISO-8601 string       |

## [3]-[ENTRYPOINTS]

### Registration

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]     | [RAIL]                                    |
| :-----: | :----------------------------------------------- | :----------------- | :---------------------------------------- |
|   [1]   | `options.ConfigureForNodaTime(provider)`         | provider overload  | constructs `NodaJsonSettings(provider)`   |
|   [2]   | `options.ConfigureForNodaTime(nodaJsonSettings)` | settings overload  | calls `AddConverters(options.Converters)` |
|   [3]   | `options.WithIsoIntervalConverter()`             | interval swap      | replaces Interval converters with ISO     |
|   [4]   | `options.WithIsoDateIntervalConverter()`         | date-interval swap | replaces DateInterval converters with ISO |

### NodaConverters singletons (static, pattern-backed)

| [INDEX] | [MEMBER]                        | [BACKING_PATTERN]                | [NOTES]                        |
| :-----: | :------------------------------ | :------------------------------- | :----------------------------- |
|   [1]   | `InstantConverter`              | `InstantPattern.ExtendedIso`     |                                |
|   [2]   | `LocalDateConverter`            | `LocalDatePattern.Iso`           |                                |
|   [3]   | `LocalDateTimeConverter`        | pattern                          |                                |
|   [4]   | `LocalTimeConverter`            | pattern                          |                                |
|   [5]   | `AnnualDateConverter`           | pattern                          |                                |
|   [6]   | `YearMonthConverter`            | pattern                          |                                |
|   [7]   | `OffsetConverter`               | `OffsetPattern.GeneralInvariant` |                                |
|   [8]   | `OffsetDateTimeConverter`       | `OffsetDateTimePattern.Rfc3339`  | ISO calendar validator applied |
|   [9]   | `OffsetDateConverter`           | `OffsetDatePattern.GeneralIso`   | ISO calendar validator applied |
|  [10]   | `OffsetTimeConverter`           | `OffsetTimePattern.ExtendedIso`  |                                |
|  [11]   | `DurationConverter`             | `DurationPattern.JsonRoundtrip`  |                                |
|  [12]   | `RoundtripDurationConverter`    | `DurationPattern.Roundtrip`      |                                |
|  [13]   | `RoundtripPeriodConverter`      | `PeriodPattern.Roundtrip`        | default PeriodConverter row    |
|  [14]   | `NormalizingIsoPeriodConverter` | `PeriodPattern.NormalizingIso`   | lossy: 90m -> 1h30m            |
|  [15]   | `IntervalConverter`             | `NodaIntervalConverter`          | start/end sub-properties       |
|  [16]   | `IsoIntervalConverter`          | `NodaIsoIntervalConverter`       | ISO-8601 interval string       |
|  [17]   | `IsoDateIntervalConverter`      | `NodaIsoDateIntervalConverter`   | ISO-8601 date-interval string  |
|  [18]   | `DateIntervalConverter`         | `NodaDateIntervalConverter`      |                                |

### NodaConverters factories (provider-bound)

| [INDEX] | [MEMBER]                                              | [RETURNS]                                                         |
| :-----: | :---------------------------------------------------- | :---------------------------------------------------------------- |
|   [1]   | `CreateZonedDateTimeConverter(IDateTimeZoneProvider)` | `NodaPatternConverter<ZonedDateTime>` with ISO calendar validator |
|   [2]   | `CreateDateTimeZoneConverter(IDateTimeZoneProvider)`  | `NodaDateTimeZoneConverter(provider)`                             |

## [4]-[IMPLEMENTATION_LAW]

[SETTINGS_LIFECYCLE]:
- `NodaJsonSettings` is mutable, not thread-safe. Construct, optionally mutate per-type converter
  properties (set a property to `null` to suppress that type), call `ConfigureForNodaTime`, discard.
- Default constructor binds `DateTimeZoneProviders.Tzdb`; the provider overload accepts any
  `IDateTimeZoneProvider`.
- `AddConverters` insertion order: Instant, Interval, LocalDate, LocalDateTime, LocalTime,
  AnnualDate, YearMonth, DateInterval, Offset, DateTimeZone, Duration, Period, OffsetDateTime,
  OffsetDate, OffsetTime, ZonedDateTime. First converter wins per type; no double-registration.

[CONVERTER_BASE_HIERARCHY]:
- `NodaConverterBase<T>` is the base for all built-in converters; it handles nullable-struct
  bridging, `CanConvert` (sealed-class vs. assignable check), and `ReadAsPropertyName` delegation.
- `NodaPatternConverter<T>` extends the base with an `IPattern<T>` and an optional post-parse
  `Action<T>` validator (used for ISO calendar enforcement on `OffsetDate`, `OffsetDateTime`,
  `ZonedDateTime`).
- `DelegatingConverterBase<T>` is independent of `NodaConverterBase<T>`; use it when a
  `[JsonConverterAttribute]`-decorated property requires a distinct type that wraps a reusable
  singleton converter instance.

[PRECEDENCE_LAW]:
- STJ resolves options-level `Converters` list before consulting `TypeInfoResolver` metadata.
  First converter in the list that claims the type wins.
- In `SuiteContracts.Wire`: `ThinktectureJsonConverterFactory` is added first (factory, position 0);
  `ConfigureForNodaTime` appends NodaTime per-type converters after it.
- `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` declines types
  carrying `[JsonConverter]`; no NodaTime type carries that attribute.
- NodaTime converters own `Instant`, `LocalDate`, `LocalDateTime`, `LocalTime`, `AnnualDate`,
  `YearMonth`, `DateInterval`, `Offset`, `DateTimeZone`, `Duration`, `Period`, `OffsetDateTime`,
  `OffsetDate`, `OffsetTime`, `ZonedDateTime`, `Interval` — the converter list wins for each;
  the source-gen `JsonTypeInfo` for those types is never reached.
- Thinktecture factory owns all value-objects, smart-enums, and keyed-unions; NodaTime converters
  own the semantic-time types. The two factories partition the type space — no collision is possible.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: STJ converter registration for NodaTime semantic-time types
- Accept: `ConfigureForNodaTime` as the sole registration call; position after `TypeInfoResolver`
  binding in `Wire` is the precedence law
- Reject: manual converter list construction, hand-rolled NodaTime converters, duplicate
  registration alongside `ConfigureForNodaTime`
