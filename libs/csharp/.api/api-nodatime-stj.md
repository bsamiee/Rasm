# [RASM_API_NODATIME_STJ]

`NodaTime.Serialization.SystemTextJson` owns System.Text.Json codec registration for NodaTime semantic-time values: `NodaConverters` mints the pattern-backed converter singletons and the provider-bound zone factories, `NodaJsonSettings` holds one settable converter slot per type, and `ConfigureForNodaTime` folds that slot set onto a `JsonSerializerOptions` converter list.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- bound asset: `lib/net6.0` for `net10.0` consumers
- namespace: `NodaTime.Serialization.SystemTextJson`
- rail: wire-json

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration and converter family

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Extensions`                            | static class   | folds a converter set onto serializer options       |
|  [02]   | `NodaJsonSettings`                      | sealed class   | one settable converter slot per NodaTime type       |
|  [03]   | `NodaConverters`                        | static class   | mints the converter singletons and zone factories   |
|  [04]   | `NodaConverterBase<T>`                  | abstract class | nullity, type admission, and read-fault boilerplate |
|  [05]   | `NodaPatternConverter<T>`               | sealed class   | codecs one type through a pattern and a validator   |
|  [06]   | `DelegatingConverterBase<T>`            | abstract class | names a converter type over a shared instance       |
|  [07]   | `NodaTimeDefaultJsonConverterAttribute` | attribute      | binds the Tzdb default converter to one member      |
|  [08]   | `NodaTimeDefaultJsonConverterFactory`   | sealed class   | resolves the Tzdb default converter per type        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and converter construction

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------------ | :------ | :----------------------------------------- |
|  [01]   | `Extensions.ConfigureForNodaTime(JsonSerializerOptions, IDateTimeZoneProvider)` | static  | appends the provider-bound converter set   |
|  [02]   | `Extensions.ConfigureForNodaTime(JsonSerializerOptions, NodaJsonSettings)`      | static  | appends every non-null settings slot       |
|  [03]   | `Extensions.WithIsoIntervalConverter(JsonSerializerOptions)`                    | static  | swaps `Interval` to the ISO string codec   |
|  [04]   | `Extensions.WithIsoDateIntervalConverter(JsonSerializerOptions)`                | static  | swaps `DateInterval` to ISO string form    |
|  [05]   | `NodaJsonSettings()`                                                            | ctor    | binds `DateTimeZoneProviders.Tzdb`         |
|  [06]   | `NodaJsonSettings(IDateTimeZoneProvider)`                                       | ctor    | binds host zone and zoned converters       |
|  [07]   | `NodaPatternConverter<T>(IPattern<T>)`                                          | ctor    | codecs one type on any NodaTime pattern    |
|  [08]   | `NodaPatternConverter<T>(IPattern<T>, Action<T>)`                               | ctor    | adds a pre-write value validator           |
|  [09]   | `DelegatingConverterBase<T>(JsonConverter<T>)`                                  | ctor    | wraps a singleton under a named type       |
|  [10]   | `NodaTimeDefaultJsonConverterFactory()`                                         | ctor    | admits value and `Nullable<T>` forms alike |

- `NodaJsonSettings`: both constructors seed every slot from `NodaConverters` — `PeriodConverter` the roundtrip period form, the two zone slots the provider factories — and a slot left null drops its type from the registration `ConfigureForNodaTime` returns for chaining.
- `Extensions.WithIsoIntervalConverter`: removes every converter already claiming the type before appending, so an interval swap lands regardless of call order.

[SETTINGS_SLOTS]: `InstantConverter` `IntervalConverter` `LocalDateConverter` `LocalDateTimeConverter` `LocalTimeConverter` `AnnualDateConverter` `YearMonthConverter` `DateIntervalConverter` `OffsetConverter` `DateTimeZoneConverter` `DurationConverter` `PeriodConverter` `OffsetDateConverter` `OffsetTimeConverter` `OffsetDateTimeConverter` `ZonedDateTimeConverter`

[ENTRYPOINT_SCOPE]: `NodaConverters` singletons and provider-bound factories

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `InstantConverter`                                    | property | `Instant` on `InstantPattern.ExtendedIso`             |
|  [02]   | `LocalDateConverter`                                  | property | `LocalDate` on `LocalDatePattern.Iso`                 |
|  [03]   | `LocalDateTimeConverter`                              | property | `LocalDateTime` on `LocalDateTimePattern.ExtendedIso` |
|  [04]   | `LocalTimeConverter`                                  | property | `LocalTime` on `LocalTimePattern.ExtendedIso`         |
|  [05]   | `AnnualDateConverter`                                 | property | `AnnualDate` on `AnnualDatePattern.Iso`               |
|  [06]   | `YearMonthConverter`                                  | property | `YearMonth` on `YearMonthPattern.Iso`                 |
|  [07]   | `IntervalConverter`                                   | property | `Interval` as a start/end object                      |
|  [08]   | `IsoIntervalConverter`                                | property | `Interval` as one ISO interval string                 |
|  [09]   | `DateIntervalConverter`                               | property | `DateInterval` as a start/end object                  |
|  [10]   | `IsoDateIntervalConverter`                            | property | `DateInterval` as one ISO interval string             |
|  [11]   | `OffsetConverter`                                     | property | `Offset` on `OffsetPattern.GeneralInvariant`          |
|  [12]   | `OffsetDateTimeConverter`                             | property | `OffsetDateTime` on `OffsetDateTimePattern.Rfc3339`   |
|  [13]   | `OffsetDateConverter`                                 | property | `OffsetDate` on `OffsetDatePattern.GeneralIso`        |
|  [14]   | `OffsetTimeConverter`                                 | property | `OffsetTime` on `OffsetTimePattern.ExtendedIso`       |
|  [15]   | `DurationConverter`                                   | property | `Duration` on `DurationPattern.JsonRoundtrip`         |
|  [16]   | `RoundtripDurationConverter`                          | property | `Duration` on `DurationPattern.Roundtrip`             |
|  [17]   | `RoundtripPeriodConverter`                            | property | `Period` on `PeriodPattern.Roundtrip`                 |
|  [18]   | `NormalizingIsoPeriodConverter`                       | property | `Period` on `PeriodPattern.NormalizingIso`            |
|  [19]   | `CreateZonedDateTimeConverter(IDateTimeZoneProvider)` | factory  | `ZonedDateTime` on an invariant pattern with zone id  |
|  [20]   | `CreateDateTimeZoneConverter(IDateTimeZoneProvider)`  | factory  | `DateTimeZone` resolved against the provider zone set |

- ISO calendar guard: `LocalDateConverter`, `LocalDateTimeConverter`, `OffsetDateConverter`, `OffsetDateTimeConverter`, and every `CreateZonedDateTimeConverter` product reject a non-ISO calendar value at write.
- `NodaConverters.NormalizingIsoPeriodConverter`: normalization is lossy — a 90-minute period round-trips as an hour and 30 minutes.
- `NodaConverters.DurationConverter` and `NodaConverters.RoundtripDurationConverter`: both type as the non-generic `JsonConverter`, so a `JsonConverter<Duration>` binding site takes a cast.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- System.Text.Json resolves the options converter list ahead of `TypeInfoResolver` metadata and takes the first entry claiming a type, so a registered NodaTime converter outranks the source-generated `JsonTypeInfo` for that type.
- `NodaConverterBase<T>` wraps every read fault as `JsonException` and leaves `WriteJsonPropertyNameImpl` throwing, so a converter serving dictionary keys overrides it.
- `NodaTimeDefaultJsonConverterFactory` binds `DateTimeZoneProviders.Tzdb` at type initialization and answers for both the value type and its `Nullable<T>` form; `NodaTimeDefaultJsonConverterAttribute` resolves through the same table.

[STACKING]:
- `NodaTime`(`.api/api-nodatime.md`): `NodaPatternConverter<T>(IPattern<T>, Action<T>)` admits any `NodaTime.Text` pattern with a pre-write validator, so a wire needing a non-default text shape assigns one converter instance into the matching `NodaJsonSettings` slot rather than subclassing; the same catalog's `IDateTimeZoneProvider` feeds both zone factories, and its type roster is the space these converters cover.
- `Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`): `ThinktectureJsonConverterFactory` and `ConfigureForNodaTime` append to one `JsonSerializerOptions.Converters` list under first-claim-wins order, the factory placed ahead of the per-type converters; the two claim disjoint type spaces, so order never contests.
- `System.Text.Json`(`.api/api-json-schema.md`): a NodaTime converter describes no schema, so `JsonSchemaExporter` emits the unconstrained node for a converted type until a `TransformSchemaNode` arm keyed on `JsonSchemaExporterContext.TypeInfo` writes the string form the pattern produces.
- Within the branch, one app-root `SuiteContracts.Wire` expression owns the merge: the Thinktecture factory registers, `NodaJsonSettings` takes its per-type replacements, `ConfigureForNodaTime` folds the slot set last, and the interval swaps run over the merged list.

[LOCAL_ADMISSION]:
- A wire profile registers semantic time once through `ConfigureForNodaTime` on the shared options object, its zone provider supplied by the host rather than taken from the parameterless settings constructor.
- A non-default text shape enters as one `NodaPatternConverter<T>` assigned into its settings slot; a member-local shape enters as `NodaTimeDefaultJsonConverterAttribute` or a `DelegatingConverterBase<T>` subclass, since `[JsonConverter]` identifies a converter by type alone.
- A type the profile carries elsewhere leaves its slot null, so the registration and the owning converter never double-bind.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: System.Text.Json registration, text shape, and calendar validation for NodaTime semantic-time values
- Accept: converter selection as data — a settings slot assignment, a pattern-built converter instance, a provider-bound zone factory, and the interval shape swaps
- Reject: a hand-rolled converter for a covered type, a hand-assembled converter list beside `ConfigureForNodaTime`, and a second options owner for the same wire
