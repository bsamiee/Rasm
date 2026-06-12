# [RASM_PERSISTENCE_API_NODATIME_JSON]

`NodaTime.Serialization.SystemTextJson` supplies System.Text.Json converters,
converter factories, serializer option extensions, default converter attributes,
and NodaTime JSON settings for temporal snapshot and support projections.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- namespace: `NodaTime.Serialization.SystemTextJson`
- asset: runtime library
- rail: snapshot-codec

## [2]-[PUBLIC_TYPES]

[CONVERTER_TYPES]: converter surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :-------------------------------------- | :------------------ | :---------------------- |
|   [1]   | `NodaConverters`                        | converter registry  | exposes converters      |
|   [2]   | `NodaConverterBase<T>`                  | converter base      | defines converter shape |
|   [3]   | `NodaPatternConverter<T>`               | converter base      | converts pattern values |
|   [4]   | `DelegatingConverterBase<T>`            | converter base      | wraps inner converter   |
|   [5]   | `NodaJsonSettings`                      | settings value      | configures converters   |
|   [6]   | `NodaTimeDefaultJsonConverterAttribute` | converter attribute | selects converter       |
|   [7]   | `NodaTimeDefaultJsonConverterFactory`   | converter factory   | creates converter       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serializer option operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]       | [CAPABILITY]                  |
| :-----: | :---------------------------------- | :----------------- | :---------------------------- |
|   [1]   | `ConfigureForNodaTime`              | options extension  | registers converters          |
|   [2]   | `WithIsoIntervalConverter`          | options extension  | swaps interval converter      |
|   [3]   | `WithIsoDateIntervalConverter`      | options extension  | swaps date-interval converter |
|   [4]   | `CreateDateTimeZoneConverter`       | factory call       | creates zone converter        |
|   [5]   | `CreateZonedDateTimeConverter`      | factory call       | creates zoned converter       |
|   [6]   | `NodaConverters.InstantConverter`   | converter property | converts instants             |
|   [7]   | `NodaConverters.LocalDateConverter` | converter property | converts local dates          |
|   [8]   | `NodaConverters.DurationConverter`  | converter property | converts durations            |

## [4]-[IMPLEMENTATION_LAW]

[JSON_PROJECTION]:
- namespace: `NodaTime.Serialization.SystemTextJson`
- options root: serializer option extensions
- converter root: `NodaConverters`
- settings root: `NodaJsonSettings`
- attribute root: default converter attribute and factory

[LOCAL_ADMISSION]:
- JSON temporal projection uses NodaTime converters and never ad hoc string formatting.
- Snapshot JSON profiles declare converter settings beside codec policy.
- Store JSON support bundles preserve temporal type intent.
- JSON conversion is projection material and does not replace stored temporal algebra.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: NodaTime JSON conversion
- Accept: declared temporal JSON projection
- Reject: handwritten temporal string converters
