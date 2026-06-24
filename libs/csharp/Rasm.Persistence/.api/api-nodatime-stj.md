# [RASM_PERSISTENCE_API_NODATIME_STJ]

`NodaTime.Serialization.SystemTextJson` wires NodaTime semantic-time types into
`System.Text.Json` through three orthogonal registration paths: (1) the
`Extensions.ConfigureForNodaTime` options-level converter list, configurable per-type
through the mutable `NodaJsonSettings` bag and pre-built `NodaConverters` singletons /
provider-bound factories; (2) the `NodaTimeDefaultJsonConverterFactory` — one
`JsonConverterFactory` registering every NodaTime type plus its nullable-struct mirror in
one entry, Tzdb-locked; (3) the `[NodaTimeDefaultJsonConverter]` per-property attribute.
In `SuiteContracts.Wire` it stacks after the Thinktecture value-object/smart-enum factory,
partitioning the STJ type space so neither factory ever collides.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- version: `1.4.0`
- license: `Apache-2.0`
- assembly: `NodaTime.Serialization.SystemTextJson`
- namespace: `NodaTime.Serialization.SystemTextJson`
- asset: runtime library
- target: net6.0 (consumer-bound fallback; multi-targets net6.0/netstandard2.0 — the
  net10.0 consumer binds the net6.0 asset, the highest available)
- rail: wire (JSON codec registration)

## [02]-[PUBLIC_TYPES]

The public converter surface is the three base classes plus `NodaPatternConverter<T>`;
every concrete per-type converter (`NodaIntervalConverter`, `NodaDateTimeZoneConverter`,
`NodaDateIntervalConverter`, `NodaIsoIntervalConverter`, `NodaIsoDateIntervalConverter`,
`NodaNullableConverter<T>`) is `internal` and reached only through a `NodaConverters`
singleton — none is a public registration handle. The two registration drivers
(`NodaTimeDefaultJsonConverterAttribute`, `NodaTimeDefaultJsonConverterFactory`) are the
attribute-default and factory-default paths.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]                | [RAIL]                                          |
| :-----: | :------------------------------------ | :--------------------------- | :---------------------------------------------- |
|  [01]   | `Extensions`                          | static extension class       | options-list registration entry point           |
|  [02]   | `NodaJsonSettings`                    | mutable property bag         | per-type converter assignment (17 slots)         |
|  [03]   | `NodaConverters`                      | static factory class         | pre-built singletons + provider-bound factories  |
|  [04]   | `NodaConverterBase<T>`                | abstract base `JsonConverter<T>` | nullity + `CanConvert` + `ReadAsPropertyName` |
|  [05]   | `NodaPatternConverter<T>`             | concrete `NodaConverterBase<T>` | `IPattern<T>` read/write + optional validator |
|  [06]   | `DelegatingConverterBase<T>`          | abstract `JsonConverter<T>`  | wraps a singleton; not a `NodaConverterBase<T>`  |
|  [07]   | `NodaTimeDefaultJsonConverterFactory` | `JsonConverterFactory`       | one-entry factory for all 16 types + nullables   |
|  [08]   | `NodaTimeDefaultJsonConverterAttribute` | `JsonConverterAttribute`   | per-property `[NodaTimeDefaultJsonConverter]`    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: options-list registration (path 1)
- rail: wire

`ConfigureForNodaTime(provider)` constructs a `NodaJsonSettings(provider)` then registers;
`ConfigureForNodaTime(settings)` calls `AddConverters(options.Converters)` for an explicitly
configured bag. `WithIso*` swap the default Interval/DateInterval converters for the ISO
string forms in an already-configured options object (replace-in-place).

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]     | [SEMANTICS]                                |
| :-----: | :----------------------------------------------- | :----------------- | :----------------------------------------- |
|  [01]   | `options.ConfigureForNodaTime(IDateTimeZoneProvider)` | provider overload | builds `NodaJsonSettings(provider)`, registers |
|  [02]   | `options.ConfigureForNodaTime(NodaJsonSettings)` | settings overload  | `AddConverters(options.Converters)`        |
|  [03]   | `options.WithIsoIntervalConverter()`             | interval swap      | replaces Interval converters with ISO string |
|  [04]   | `options.WithIsoDateIntervalConverter()`         | date-interval swap | replaces DateInterval converters with ISO string |

[ENTRYPOINT_SCOPE]: `NodaJsonSettings` per-type slots
- rail: wire

Mutable, not thread-safe. Each slot is a `JsonConverter` `{ get; set; }`; set a slot to
`null` to suppress that type. Default ctor binds `DateTimeZoneProviders.Tzdb`; the
`IDateTimeZoneProvider` ctor binds any provider (the `DateTimeZoneConverter` and
`ZonedDateTimeConverter` slots are provider-derived). Slots: `InstantConverter`,
`IntervalConverter`, `LocalDateConverter`, `LocalTimeConverter`, `LocalDateTimeConverter`,
`AnnualDateConverter`, `YearMonthConverter`, `DateIntervalConverter`, `OffsetConverter`,
`DateTimeZoneConverter`, `DurationConverter`, `PeriodConverter`, `OffsetDateConverter`,
`OffsetTimeConverter`, `OffsetDateTimeConverter`, `ZonedDateTimeConverter`.

[ENTRYPOINT_SCOPE]: `NodaConverters` pre-built singletons (static, pattern-backed)
- rail: wire

Each is a `public static JsonConverter<T>` (Duration/RoundtripDuration are non-generic
`JsonConverter` properties delegating to internal `…Impl` singletons). `RoundtripPeriod`
is the `AddConverters` default for `Period`; `NormalizingIsoPeriod` is the lossy alternative.

| [INDEX] | [MEMBER]                        | [BACKING_PATTERN]                  | [NOTES]                              |
| :-----: | :------------------------------ | :--------------------------------- | :----------------------------------- |
|  [01]   | `InstantConverter`              | `InstantPattern.ExtendedIso`       | —                                    |
|  [02]   | `LocalDateConverter`            | `LocalDatePattern.Iso`             | ISO calendar validator applied       |
|  [03]   | `LocalDateTimeConverter`        | `LocalDateTimePattern.ExtendedIso` | ISO calendar validator applied       |
|  [04]   | `LocalTimeConverter`            | `LocalTimePattern.ExtendedIso`     | —                                    |
|  [05]   | `AnnualDateConverter`           | `AnnualDatePattern.Iso`            | —                                    |
|  [06]   | `YearMonthConverter`            | `YearMonthPattern.Iso`             | —                                    |
|  [07]   | `OffsetConverter`               | `OffsetPattern.GeneralInvariant`   | —                                    |
|  [08]   | `OffsetDateTimeConverter`       | `OffsetDateTimePattern.Rfc3339`    | ISO calendar validator applied       |
|  [09]   | `OffsetDateConverter`           | `OffsetDatePattern.GeneralIso`     | ISO calendar validator applied       |
|  [10]   | `OffsetTimeConverter`           | `OffsetTimePattern.ExtendedIso`    | —                                    |
|  [11]   | `DurationConverter`             | `DurationPattern.JsonRoundtrip`    | non-generic `JsonConverter` property |
|  [12]   | `RoundtripDurationConverter`    | `DurationPattern.Roundtrip`        | non-generic `JsonConverter` property |
|  [13]   | `RoundtripPeriodConverter`      | `PeriodPattern.Roundtrip`          | `AddConverters` default for `Period` |
|  [14]   | `NormalizingIsoPeriodConverter` | `PeriodPattern.NormalizingIso`     | lossy: 90m -> 1h30m                  |
|  [15]   | `IntervalConverter`             | internal `NodaIntervalConverter`   | start/end sub-object                 |
|  [16]   | `IsoIntervalConverter`          | internal `NodaIsoIntervalConverter`| ISO-8601 interval string             |
|  [17]   | `DateIntervalConverter`         | internal `NodaDateIntervalConverter` | start/end sub-object               |
|  [18]   | `IsoDateIntervalConverter`      | internal `NodaIsoDateIntervalConverter` | ISO-8601 date-interval string    |

[ENTRYPOINT_SCOPE]: `NodaConverters` provider-bound factories
- rail: wire

These two require a runtime `IDateTimeZoneProvider` (the converter list and
`NodaJsonSettings(provider)` route through them). The standalone factory/attribute path
([04]) instead hard-binds `DateTimeZoneProviders.Tzdb`.

| [INDEX] | [MEMBER]                                              | [RETURNS]                                                         |
| :-----: | :---------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `CreateZonedDateTimeConverter(IDateTimeZoneProvider)` | `JsonConverter<ZonedDateTime>` (`NodaPatternConverter` + ISO validator) |
|  [02]   | `CreateDateTimeZoneConverter(IDateTimeZoneProvider)`  | `JsonConverter<DateTimeZone>` (internal `NodaDateTimeZoneConverter`) |

[ENTRYPOINT_SCOPE]: factory + attribute defaults (paths 2 and 3)
- rail: wire

`NodaTimeDefaultJsonConverterFactory` is a `JsonConverterFactory` whose `CanConvert`/
`CreateConverter` dispatch over a static `Dictionary<Type, JsonConverter>` seeded with
every NodaTime type AND its `T?` nullable-struct mirror (via an internal
`NodaNullableConverter<T>`); it hard-binds `DateTimeZoneProviders.Tzdb` for `DateTimeZone`
and `ZonedDateTime`. The attribute is the per-property opt-in form of the same default.

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]        | [SEMANTICS]                                |
| :-----: | :----------------------------------------------------- | :------------------ | :----------------------------------------- |
|  [01]   | `new NodaTimeDefaultJsonConverterFactory()`            | options `Converters` add | one factory entry covers all 16 types + nullables, Tzdb-locked |
|  [02]   | `factory.CanConvert(Type)` / `CreateConverter(Type, options)` | factory override | dictionary dispatch; null for unknown types |
|  [03]   | `[NodaTimeDefaultJsonConverter]` on a property         | attribute           | per-property default converter (Tzdb-locked) |

## [04]-[IMPLEMENTATION_LAW]

[REGISTRATION_PATHS]:
- Path 1 (options list, default in `Wire`): `ConfigureForNodaTime(provider)` or
  `ConfigureForNodaTime(settings)` appends per-type converters to `options.Converters`;
  the provider is honored, so a non-Tzdb `IDateTimeZoneProvider` is respected for
  `DateTimeZone`/`ZonedDateTime`. Use this when the provider is configurable.
- Path 2 (factory): add a single `NodaTimeDefaultJsonConverterFactory()` to
  `options.Converters`. Covers all types + nullable mirrors in one entry but hard-binds
  Tzdb — never use it where a custom provider is required.
- Path 3 (attribute): `[NodaTimeDefaultJsonConverter]` on a property; the per-member
  opt-in form, also Tzdb-locked.

[SETTINGS_LIFECYCLE]:
- `NodaJsonSettings` is mutable, not thread-safe: construct, optionally mutate per-type
  slots (set a slot to `null` to suppress that type), call `ConfigureForNodaTime`, discard.
- Default ctor binds `DateTimeZoneProviders.Tzdb`; the `IDateTimeZoneProvider` ctor binds
  any provider and re-derives the `DateTimeZone`/`ZonedDateTime` slots from it.
- `AddConverters` insertion order: Instant, Interval, LocalDate, LocalDateTime, LocalTime,
  AnnualDate, YearMonth, DateInterval, Offset, DateTimeZone, Duration, Period,
  OffsetDateTime, OffsetDate, OffsetTime, ZonedDateTime. First converter wins per type; no
  double-registration. `WithIso*` replace-in-place after the fact.

[CONVERTER_BASE_HIERARCHY]:
- `NodaConverterBase<T>` is the base for all built-in converters; it handles nullable-struct
  bridging, `CanConvert` (sealed-class vs. assignable check), and `ReadAsPropertyName`
  delegation.
- `NodaPatternConverter<T>` extends the base with an `IPattern<T>` and an optional post-parse
  `Action<T>` validator (the ISO-calendar enforcer applied to `LocalDate`, `LocalDateTime`,
  `OffsetDate`, `OffsetDateTime`, `ZonedDateTime`).
- `DelegatingConverterBase<T>` is independent of `NodaConverterBase<T>`; subclass it when a
  `[JsonConverter]`-decorated property needs a distinct converter type that wraps a reusable
  singleton (the package ships `NodaTimeDefaultJsonConverterAttribute` rather than requiring
  this for the common case).

[PRECEDENCE_LAW]:
- STJ resolves the options-level `Converters` list before consulting `TypeInfoResolver`
  metadata. First converter in the list that claims the type wins.
- In `SuiteContracts.Wire`: `ThinktectureJsonConverterFactory` is added first (position 0);
  `ConfigureForNodaTime` appends NodaTime per-type converters after it.
- `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` declines
  types carrying `[JsonConverter]`; no NodaTime type carries that attribute by default.
- NodaTime converters own `Instant`, `LocalDate`, `LocalDateTime`, `LocalTime`, `AnnualDate`,
  `YearMonth`, `DateInterval`, `Offset`, `DateTimeZone`, `Duration`, `Period`,
  `OffsetDateTime`, `OffsetDate`, `OffsetTime`, `ZonedDateTime`, `Interval` — the converter
  list wins for each; the source-gen `JsonTypeInfo` for those types is never reached.
- Thinktecture owns all value-objects, smart-enums, and keyed-unions; NodaTime owns the
  semantic-time types. The two factories partition the type space — no collision possible.

[STACKING_LAW]:
- Wire rail: this codec composes with `api-thinktecture-json` (value-object/smart-enum STJ
  factory) under one `JsonSerializerOptions` — Thinktecture factory at position 0, NodaTime
  per-type converters appended, both before the `TypeInfoResolver`.
- Sep ingress: CSV columns carrying semantic-time values parse through these converters'
  read path off a `Col` span (`api-sep`); the parsed `Instant`/`LocalDate` then carries
  through to the EF/linq2db store.
- CloudEvents/JsonPatch: the same configured `JsonSerializerOptions` is reused for
  CloudEvents STJ formatting and JSON Patch document (de)serialization, so NodaTime
  timestamps round-trip identically across the egress and patch rails.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: STJ converter registration for NodaTime semantic-time types across the list,
  factory, and attribute paths
- Accept: `ConfigureForNodaTime` as the canonical options-list call (position after the
  Thinktecture factory, before `TypeInfoResolver`); the factory/attribute paths only where
  a Tzdb-locked default is acceptable
- Reject: manual converter-list construction; hand-rolled NodaTime converters; the
  Tzdb-locked factory/attribute path where a configurable `IDateTimeZoneProvider` is
  required; duplicate registration alongside `ConfigureForNodaTime`
