# [RASM_API_NODATIME_STJ]

`NodaTime.Serialization.SystemTextJson` is the System.Text.Json codec surface for NodaTime semantic-time types. It registers converter instances on `JsonSerializerOptions`, with `NodaJsonSettings` carrying the mutable per-type converter assignment and `NodaConverters` supplying the canonical pattern-backed singleton and provider-bound converter factories. It is the shared JSON wire adapter for packages that expose `Instant`, `LocalDate`, `DateInterval`, `DateTimeZone`, `ZonedDateTime`, `OffsetDateTime`, `Duration`, or `Period` over STJ.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- bound asset: `lib/net6.0` for `net10.0` consumers
- namespace: `NodaTime.Serialization.SystemTextJson`
- dependency: `NodaTime` `[,)`
- direct owners: `Rasm.AppHost`, `Rasm.Persistence`
- rail: wire-json

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration and converter family
- rail: wire-json

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:------------------- |:------------------------------------------------ |
| [01] | `Extensions` | static extension | registers and specializes NodaTime converters on STJ options |
| [02] | `NodaJsonSettings` | mutable settings bag | per-type converter assignment and provider-bound defaults |
| [03] | `NodaConverters` | converter catalog | singleton converters plus provider-bound factories |
| [04] | `NodaConverterBase<T>` | converter base | nullity, `CanConvert`, and property-name behavior |
| [05] | `NodaPatternConverter<T>` | pattern converter | `IPattern<T>` parse/format with optional validator |
| [06] | `DelegatingConverterBase<T>` | delegation base | property-level attribute dispatch through another converter |
| [07] | `NodaTimeDefaultJsonConverterAttribute` | attribute | default converter attachment for a NodaTime property or type |
| [08] | `NodaTimeDefaultJsonConverterFactory` | converter factory | one-entry factory for every NodaTime type + nullable mirror, Tzdb-locked |

The concrete per-type converters (`NodaNullableConverter<T>`, `NodaDateTimeZoneConverter`, `NodaIntervalConverter`, `NodaIsoIntervalConverter`, `NodaDateIntervalConverter`, `NodaIsoDateIntervalConverter`) are `internal sealed` and reached ONLY through the `NodaConverters` singletons and provider-bound factories below — none is a public registration handle.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations
- rail: wire-json

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------------------- |:----------------- |:------------------------------------------------ |
| [01] | `options.ConfigureForNodaTime(IDateTimeZoneProvider)` | provider overload | constructs `NodaJsonSettings(provider)` and appends converters |
| [02] | `options.ConfigureForNodaTime(NodaJsonSettings)` | settings overload | appends each non-null converter property from the settings bag |
| [03] | `options.WithIsoIntervalConverter()` | interval swap | replaces existing `Interval` converters with `NodaConverters.IsoIntervalConverter` |
| [04] | `options.WithIsoDateIntervalConverter()` | date-interval swap | replaces existing `DateInterval` converters with `NodaConverters.IsoDateIntervalConverter` |
| [05] | `new NodaJsonSettings()` | settings ctor | binds defaults to `DateTimeZoneProviders.Tzdb` |
| [06] | `new NodaJsonSettings(IDateTimeZoneProvider)` | settings ctor | binds zone and zoned-date-time converters to the supplied provider |
| [07] | `[NodaTimeDefaultJsonConverter]` / `CreateConverter(Type)` | attribute path | property-level default converter without an options-level registration call |

[ENTRYPOINT_SCOPE]: `NodaConverters` singleton converters
- rail: wire-json

| [INDEX] | [MEMBER] | [BACKING_PATTERN] | [CAPABILITY] |
|:-----: |:------------------------------- |:--------------------------------- |:------------------------------------------------ |
| [01] | `InstantConverter` | `InstantPattern.ExtendedIso` | `Instant` string codec |
| [02] | `LocalDateConverter` | `LocalDatePattern.Iso` | `LocalDate` ISO calendar codec |
| [03] | `LocalDateTimeConverter` | `LocalDateTimePattern.ExtendedIso` | `LocalDateTime` codec |
| [04] | `LocalTimeConverter` | `LocalTimePattern.ExtendedIso` | `LocalTime` codec |
| [05] | `AnnualDateConverter` | `AnnualDatePattern.Iso` | `AnnualDate` codec |
| [06] | `YearMonthConverter` | `YearMonthPattern.Iso` | `YearMonth` codec |
| [07] | `IntervalConverter` | `NodaIntervalConverter` | `Interval` object codec |
| [08] | `IsoIntervalConverter` | `NodaIsoIntervalConverter` | ISO interval string codec |
| [09] | `DateIntervalConverter` | `NodaDateIntervalConverter` | `DateInterval` object codec |
| [10] | `IsoDateIntervalConverter` | `NodaIsoDateIntervalConverter` | ISO date-interval string codec |
| [11] | `OffsetConverter` | `OffsetPattern.GeneralInvariant` | `Offset` codec |
| [12] | `OffsetDateTimeConverter` | `OffsetDateTimePattern.Rfc3339` | `OffsetDateTime` codec with ISO calendar validation |
| [13] | `OffsetDateConverter` | `OffsetDatePattern.GeneralIso` | `OffsetDate` codec with ISO calendar validation |
| [14] | `OffsetTimeConverter` | `OffsetTimePattern.ExtendedIso` | `OffsetTime` codec |
| [15] | `DurationConverter` | `DurationPattern.JsonRoundtrip` | JSON round-trip `Duration` codec |
| [16] | `RoundtripDurationConverter` | `DurationPattern.Roundtrip` | round-trip `Duration` codec |
| [17] | `RoundtripPeriodConverter` | `PeriodPattern.Roundtrip` | round-trip `Period` codec |
| [18] | `NormalizingIsoPeriodConverter` | `PeriodPattern.NormalizingIso` | normalizing ISO `Period` codec; normalization is lossy |

[ENTRYPOINT_SCOPE]: `NodaConverters` provider-bound factories
- rail: wire-json

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------------------- |:----------------- |:------------------------------------------------ |
| [01] | `CreateZonedDateTimeConverter(IDateTimeZoneProvider)` | provider factory | zone-aware `ZonedDateTime` converter with ISO calendar validation |
| [02] | `CreateDateTimeZoneConverter(IDateTimeZoneProvider)` | provider factory | `DateTimeZone` converter backed by the supplied provider |

## [04]-[IMPLEMENTATION_LAW]

[SETTINGS_LIFECYCLE]:
- `NodaJsonSettings` is a mutable options bag. Construct it, optionally replace or null specific converter properties, pass it to `ConfigureForNodaTime`, then treat the resulting `JsonSerializerOptions` converter list as the durable registration.
- The default settings constructor binds `DateTimeZoneProviders.Tzdb`; use the provider constructor when a host-specific `IDateTimeZoneProvider` owns zone lookup.
- `ConfigureForNodaTime` appends converter instances to the options converter list. Registration order matters because the first STJ converter claiming a type wins.

[CONVERTER_BOUNDARY]:
- `NodaConverterBase<T>` and `NodaPatternConverter<T>` own NodaTime parse/format behavior; consumers do not hand-roll converters for individual NodaTime types.
- `WithIsoIntervalConverter()` and `WithIsoDateIntervalConverter()` are the only shared swaps from object-shaped interval/date-interval JSON to ISO string JSON.
- `NodaTimeDefaultJsonConverterAttribute` is the property/type-local route when the converter belongs with one member rather than with shared options.

[STACKING]:
- NodaTime converters own semantic-time types. Thinktecture STJ converters own Thinktecture-generated value objects, smart enums, and unions. Register both on the same options object when a wire profile carries both generated owners and semantic time.
- The source-generated STJ metadata for NodaTime types is not the owner once `ConfigureForNodaTime` has registered a matching converter. The options converter list owns these types.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: System.Text.Json converter registration for NodaTime semantic-time types.
- Accept: `ConfigureForNodaTime` as the shared registration call; `WithIsoIntervalConverter` and `WithIsoDateIntervalConverter` as explicit interval shape swaps; provider-bound zone factories.
- Reject: manual converter list construction for covered NodaTime types, hand-rolled NodaTime converters, duplicate registrations beside `ConfigureForNodaTime`.
