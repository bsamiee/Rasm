# [RASM_APPHOST_API_NODATIME_STJ]

`NodaTime.Serialization.SystemTextJson` wires NodaTime types into `JsonSerializerOptions` via a per-type converter list. The entry point is `Extensions.ConfigureForNodaTime` on `JsonSerializerOptions`; `NodaJsonSettings` is the mutable property bag that holds the per-type converter assignments; `NodaConverters` supplies the pre-built singletons and provider-bound factories.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- namespace: `NodaTime.Serialization.SystemTextJson`
- asset: runtime library
- rail: wire (JSON codec registration)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter and settings family
- rail: wire

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [RAIL]                            |
| :-----: | :--------------------------- | :------------------- | :-------------------------------- |
|  [01]   | `Extensions`                 | static extension     | options registration entry point  |
|  [02]   | `NodaJsonSettings`           | mutable property bag | per-type converter assignment     |
|  [03]   | `NodaConverters`             | static factory class | pre-built converter singletons    |
|  [04]   | `NodaConverterBase<T>`       | abstract base        | nullity + CanConvert boilerplate  |
|  [05]   | `NodaPatternConverter<T>`    | pattern-backed       | IPattern<T> read/write + validate |
|  [06]   | `DelegatingConverterBase<T>` | delegation base      | per-property attribute dispatch   |
|  [07]   | `NodaNullableConverter<T>`   | nullable wrapper     | nullable struct bridging          |
|  [08]   | `NodaDateTimeZoneConverter`  | zone converter       | DateTimeZone via provider lookup  |
|  [09]   | `NodaIntervalConverter`      | interval converter   | Interval as start/end sub-object  |
|  [10]   | `NodaIsoIntervalConverter`   | interval converter   | Interval as ISO-8601 string       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: wire

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]     | [RAIL]                                    |
| :-----: | :----------------------------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `options.ConfigureForNodaTime(provider)`         | provider overload  | constructs `NodaJsonSettings(provider)`   |
|  [02]   | `options.ConfigureForNodaTime(nodaJsonSettings)` | settings overload  | calls `AddConverters(options.Converters)` |
|  [03]   | `options.WithIsoIntervalConverter()`             | interval swap      | replaces Interval converters with ISO     |
|  [04]   | `options.WithIsoDateIntervalConverter()`         | date-interval swap | replaces DateInterval converters with ISO |

[ENTRYPOINT_SCOPE]: `NodaConverters` singletons — static, pattern-backed
- rail: wire

| [INDEX] | [MEMBER]                        | [BACKING_PATTERN]                  | [VALIDATOR]                   |
| :-----: | :------------------------------ | :--------------------------------- | :---------------------------- |
|  [01]   | `InstantConverter`              | `InstantPattern.ExtendedIso`       | —                             |
|  [02]   | `LocalDateConverter`            | `LocalDatePattern.Iso`             | —                             |
|  [03]   | `LocalDateTimeConverter`        | `LocalDateTimePattern.ExtendedIso` | —                             |
|  [04]   | `LocalTimeConverter`            | `LocalTimePattern.ExtendedIso`     | —                             |
|  [05]   | `AnnualDateConverter`           | `AnnualDatePattern.Iso`            | —                             |
|  [06]   | `YearMonthConverter`            | `YearMonthPattern.Iso`             | —                             |
|  [07]   | `OffsetConverter`               | `OffsetPattern.GeneralInvariant`   | —                             |
|  [08]   | `OffsetDateTimeConverter`       | `OffsetDateTimePattern.Rfc3339`    | ISO calendar                  |
|  [09]   | `OffsetDateConverter`           | `OffsetDatePattern.GeneralIso`     | ISO calendar                  |
|  [10]   | `OffsetTimeConverter`           | `OffsetTimePattern.ExtendedIso`    | —                             |
|  [11]   | `DurationConverter`             | `DurationPattern.JsonRoundtrip`    | —                             |
|  [12]   | `RoundtripDurationConverter`    | `DurationPattern.Roundtrip`        | —                             |
|  [13]   | `RoundtripPeriodConverter`      | `PeriodPattern.Roundtrip`          | default `PeriodConverter`     |
|  [14]   | `NormalizingIsoPeriodConverter` | `PeriodPattern.NormalizingIso`     | lossy: 90m -> 1h30m           |
|  [15]   | `IntervalConverter`             | `NodaIntervalConverter`            | start/end sub-properties      |
|  [16]   | `IsoIntervalConverter`          | `NodaIsoIntervalConverter`         | ISO-8601 interval string      |
|  [17]   | `IsoDateIntervalConverter`      | `NodaIsoDateIntervalConverter`     | ISO-8601 date-interval string |
|  [18]   | `DateIntervalConverter`         | `NodaDateIntervalConverter`        | —                             |

[ENTRYPOINT_SCOPE]: `NodaConverters` factories — provider-bound
- rail: wire

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]    | [RAIL]                                                            |
| :-----: | :---------------------------------------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `CreateZonedDateTimeConverter(IDateTimeZoneProvider)` | converter factory | `NodaPatternConverter<ZonedDateTime>` with ISO calendar validator |
|  [02]   | `CreateDateTimeZoneConverter(IDateTimeZoneProvider)`  | converter factory | `NodaDateTimeZoneConverter(provider)`                             |

## [04]-[IMPLEMENTATION_LAW]

[SETTINGS_LIFECYCLE]:
- `NodaJsonSettings` is mutable, not thread-safe. Construct, optionally mutate per-type converter properties (set a property to `null` to suppress that type), call `ConfigureForNodaTime`, discard.
- Default constructor binds `DateTimeZoneProviders.Tzdb`; the provider overload accepts any `IDateTimeZoneProvider`.
- `AddConverters` insertion order: Instant, Interval, LocalDate, LocalDateTime, LocalTime, AnnualDate, YearMonth, DateInterval, Offset, DateTimeZone, Duration, Period, OffsetDateTime, OffsetDate, OffsetTime, ZonedDateTime. First converter wins per type; no double-registration.

[CONVERTER_BASE_HIERARCHY]:
- `NodaConverterBase<T>` is the base for all built-in converters; it handles nullable-struct bridging, `CanConvert` (sealed-class vs. assignable check), and `ReadAsPropertyName` delegation.
- `NodaPatternConverter<T>` extends the base with an `IPattern<T>` and an optional post-parse `Action<T>` validator (used for ISO calendar enforcement on `OffsetDate`, `OffsetDateTime`, `ZonedDateTime`).
- `DelegatingConverterBase<T>` is independent of `NodaConverterBase<T>`; use it when a `[JsonConverterAttribute]`-decorated property requires a distinct type that wraps a reusable singleton converter instance.

[PRECEDENCE_LAW]:
- STJ resolves options-level `Converters` list before consulting `TypeInfoResolver` metadata. First converter in the list that claims the type wins.
- In `SuiteContracts.Wire`: `ThinktectureJsonConverterFactory` is added first (factory, position 0); `ConfigureForNodaTime` appends NodaTime per-type converters after it.
- `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` declines types carrying `[JsonConverter]`; no NodaTime type carries that attribute.
- NodaTime converters own `Instant`, `LocalDate`, `LocalDateTime`, `LocalTime`, `AnnualDate`, `YearMonth`, `DateInterval`, `Offset`, `DateTimeZone`, `Duration`, `Period`, `OffsetDateTime`, `OffsetDate`, `OffsetTime`, `ZonedDateTime`, `Interval` — the converter list wins for each; the source-gen `JsonTypeInfo` for those types is never reached.
- Thinktecture factory owns all value-objects, smart-enums, and keyed-unions; NodaTime converters own the semantic-time types. The two factories partition the type space — no collision is possible.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: STJ converter registration for NodaTime semantic-time types
- Accept: `ConfigureForNodaTime` as the sole registration call; position after `TypeInfoResolver` binding in `Wire` is the precedence law
- Reject: manual converter list construction, hand-rolled NodaTime converters, duplicate registration alongside `ConfigureForNodaTime`
