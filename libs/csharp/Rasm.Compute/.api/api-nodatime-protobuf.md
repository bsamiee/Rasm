# [RASM_COMPUTE_API_NODATIME_PROTOBUF]

`NodaTime.Serialization.Protobuf` supplies bidirectional extension conversions
between NodaTime temporal values and protobuf wire types: `Instant`/`Duration`
against `google.protobuf` well-known types, and `LocalDate`/`LocalTime`/
`IsoDayOfWeek` against `Google.Type` common protos.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.Protobuf`
- package: `NodaTime.Serialization.Protobuf`
- assembly: `NodaTime.Serialization.Protobuf`
- namespace: `NodaTime.Serialization.Protobuf`
- asset: runtime library
- rail: remote-contracts

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion extension owners
- rail: remote-contracts

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]    | [CAPABILITY]                  |
| :-----: | :------------------- | :---------------- | :---------------------------- |
|   [1]   | `NodaExtensions`     | extension surface | projects NodaTime to protobuf |
|   [2]   | `ProtobufExtensions` | extension surface | projects protobuf to NodaTime |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: NodaTime to protobuf projection (`NodaExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                  | [CAPABILITY]              |
| :-----: | :-------------------- | :---------------------------- | :------------------------ |
|   [1]   | `ToTimestamp`         | `Instant` extension           | emits `Timestamp`         |
|   [2]   | `ToProtobufDuration`  | NodaTime `Duration` extension | emits protobuf `Duration` |
|   [3]   | `ToDate`              | `LocalDate` extension         | emits `Google.Type.Date`  |
|   [4]   | `ToTimeOfDay`         | `LocalTime` extension         | emits `TimeOfDay`         |
|   [5]   | `ToProtobufDayOfWeek` | `IsoDayOfWeek` extension      | emits `DayOfWeek` enum    |

[ENTRYPOINT_SCOPE]: protobuf to NodaTime projection (`ProtobufExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                  | [CAPABILITY]              |
| :-----: | :--------------- | :---------------------------- | :------------------------ |
|   [1]   | `ToInstant`      | `Timestamp` extension         | emits `Instant`           |
|   [2]   | `ToNodaDuration` | protobuf `Duration` extension | emits NodaTime `Duration` |
|   [3]   | `ToLocalDate`    | `Google.Type.Date` extension  | emits `LocalDate`         |
|   [4]   | `ToLocalTime`    | `TimeOfDay` extension         | emits `LocalTime`         |
|   [5]   | `ToIsoDayOfWeek` | `DayOfWeek` enum extension    | emits `IsoDayOfWeek`      |

## [4]-[IMPLEMENTATION_LAW]

[CONVERSION_CONTRACTS]:
- namespace: `NodaTime.Serialization.Protobuf`
- well-known root: `Instant`/`Duration` pair with `Google.Protobuf.WellKnownTypes.Timestamp`/`Duration`
- common-proto root: `LocalDate`/`LocalTime`/`IsoDayOfWeek` pair with `Google.Type.Date`/`TimeOfDay`/`DayOfWeek` via `Google.Api.CommonProtos`
- direction law: `NodaExtensions` projects outward; `ProtobufExtensions` projects inward; no codec or message ownership

[RANGE_CONTRACTS]:
- `ToTimestamp` rejects instants before `NodaConstants.BclEpoch` (0001-01-01 CE) with `ArgumentOutOfRangeException`
- `ToProtobufDuration` rejects durations outside the protobuf ±315576000000s window (~10,000 years) with `ArgumentOutOfRangeException`
- `ToInstant` and `ToNodaDuration` reject invalid or null wire payloads with `ArgumentException`/`ArgumentNullException`
- `ToLocalTime` rejects invalid time-of-day, leap-second, and 24:00 payloads with `ArgumentException`
- `ToIsoDayOfWeek` maps `Unspecified` to `IsoDayOfWeek.None` and rejects out-of-range enum values

[LOCAL_ADMISSION]:
- Temporal values cross remote Compute contracts as protobuf well-known and common-proto types, never as serialized NodaTime text.
- Conversion happens at the wire boundary; internal code carries NodaTime values only.
- Range failures are boundary faults to project onto typed rails at the call site; the package throws.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.Protobuf`
- Owns: NodaTime-to-protobuf temporal conversion
- Accept: boundary extension calls on NodaTime and protobuf temporal values
- Reject: handwritten epoch arithmetic between NodaTime and wire payloads
