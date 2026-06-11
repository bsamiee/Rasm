# [RASM_PERSISTENCE_API_NODATIME_JSON]

`NodaTime.Serialization.SystemTextJson` supplies System.Text.Json converters and serializer configuration for NodaTime values.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.SystemTextJson`
- package: `NodaTime.Serialization.SystemTextJson`
- assembly: `NodaTime.Serialization.SystemTextJson`
- namespace: `NodaTime.Serialization.SystemTextJson`
- asset: runtime library
- rail: snapshot-codec

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: JSON converter family
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]      | [CAPABILITY]         |
| :-----: | :-------------------------------------- | :------------------ | :------------------- |
|   [1]   | `Extensions`                            | options extension   | configures JSON      |
|   [2]   | `NodaConverters`                        | converter catalog   | defines codec path   |
|   [3]   | `NodaJsonSettings`                      | settings object     | carries codec policy |
|   [4]   | `NodaConverterBase<T>`                  | converter base      | defines converter    |
|   [5]   | `NodaPatternConverter<T>`               | pattern converter   | serializes value     |
|   [6]   | `NodaTimeDefaultJsonConverterAttribute` | converter attribute | marks converter      |
|   [7]   | `NodaTimeDefaultJsonConverterFactory`   | converter factory   | creates converter    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: converter operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]       | [CAPABILITY]             |
| :-----: | :---------------------------------- | :----------------- | :----------------------- |
|   [1]   | `ConfigureForNodaTime`              | options extension  | applies converter policy |
|   [2]   | `WithIsoIntervalConverter`          | settings extension | applies interval codec   |
|   [3]   | `WithIsoDateIntervalConverter`      | settings extension | applies interval codec   |
|   [4]   | `CreateZonedDateTimeConverter`      | factory call       | creates converter        |
|   [5]   | `CreateDateTimeZoneConverter`       | factory call       | creates converter        |
|   [6]   | `NodaConverters.InstantConverter`   | converter property | serializes instant       |
|   [7]   | `NodaConverters.LocalDateConverter` | converter property | serializes date          |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: NodaTime JSON projection
- Accept: JSON snapshots preserve semantic time
- Reject: manual time string formats
