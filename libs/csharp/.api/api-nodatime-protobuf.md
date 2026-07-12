# [RASM_API_NODATIME_PROTOBUF]

`NodaTime.Serialization.Protobuf` supplies bidirectional extensions between NodaTime temporal values and protobuf wire types: `Instant` and `Duration` pair with the well-known `Timestamp` and `Duration` messages, while `LocalDate`, `LocalTime`, and `IsoDayOfWeek` pair with the common `Date`, `TimeOfDay`, and `DayOfWeek` messages. The package owns conversion only; the remote-contract rail carries protobuf messages, and the interior carries NodaTime values.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.Protobuf`
- package: `NodaTime.Serialization.Protobuf`
- assembly: `NodaTime.Serialization.Protobuf`
- namespace: `NodaTime.Serialization.Protobuf`
- asset: runtime library (managed; Apache-2.0; `lib/netstandard2.0` only)
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion extension owners
- rail: remote-contracts

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]    | [CAPABILITY]                  |
| :-----: | :------------------- | :---------------- | :---------------------------- |
|  [01]   | `NodaExtensions`     | extension surface | projects NodaTime to protobuf |
|  [02]   | `ProtobufExtensions` | extension surface | projects protobuf to NodaTime |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: NodaTime to protobuf projection (`NodaExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                  | [CAPABILITY]              |
| :-----: | :-------------------- | :---------------------------- | :------------------------ |
|  [01]   | `ToTimestamp`         | `Instant` extension           | emits `Timestamp`         |
|  [02]   | `ToProtobufDuration`  | NodaTime `Duration` extension | emits protobuf `Duration` |
|  [03]   | `ToDate`              | `LocalDate` extension         | emits `Google.Type.Date`  |
|  [04]   | `ToTimeOfDay`         | `LocalTime` extension         | emits `TimeOfDay`         |
|  [05]   | `ToProtobufDayOfWeek` | `IsoDayOfWeek` extension      | emits `DayOfWeek` enum    |

[ENTRYPOINT_SCOPE]: protobuf to NodaTime projection (`ProtobufExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                  | [CAPABILITY]              |
| :-----: | :--------------- | :---------------------------- | :------------------------ |
|  [01]   | `ToInstant`      | `Timestamp` extension         | emits `Instant`           |
|  [02]   | `ToNodaDuration` | protobuf `Duration` extension | emits NodaTime `Duration` |
|  [03]   | `ToLocalDate`    | `Google.Type.Date` extension  | emits `LocalDate`         |
|  [04]   | `ToLocalTime`    | `TimeOfDay` extension         | emits `LocalTime`         |
|  [05]   | `ToIsoDayOfWeek` | `DayOfWeek` enum extension    | emits `IsoDayOfWeek`      |

## [04]-[IMPLEMENTATION_LAW]

[CONVERSION_CONTRACTS]:
- namespace: `NodaTime.Serialization.Protobuf`
- well-known root: `Instant`/`Duration` pair with `Google.Protobuf.WellKnownTypes.Timestamp`/`Duration` (the `Google.Protobuf` package owns the message types)
- common-proto root: `LocalDate`/`LocalTime`/`IsoDayOfWeek` pair with `Google.Type.Date`/`TimeOfDay`/`DayOfWeek` — the `Google.Type` protos ship in the transitive `Google.Api.CommonProtos` dependency, not in `Google.Protobuf`
- direction law: `NodaExtensions` projects outward; `ProtobufExtensions` projects inward; no codec or message ownership
- Wire shape: Remote contracts carry `Timestamp`, `Duration`, `Date`, `TimeOfDay`, and `DayOfWeek`; rail interiors carry NodaTime values.
- Inbound: The seam applies `ToInstant`, `ToNodaDuration`, `ToLocalDate`, `ToLocalTime`, or `ToIsoDayOfWeek` once before lifting the result onto `Fin` or `Validation`.
- Outbound: The seam applies `ToTimestamp`, `ToProtobufDuration`, `ToDate`, `ToTimeOfDay`, or `ToProtobufDayOfWeek` once; `FrameEdge.Transaction` writes `hlc.ToTimestamp` into the request's physical-clock field.
- In-process: `RemoteTransport.InProcess` applies the same conversions through its in-memory handler, preserving the temporal wire contract without a live remote.

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
