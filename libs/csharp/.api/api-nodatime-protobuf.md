# [RASM_COMPUTE_API_NODATIME_PROTOBUF]

`NodaTime.Serialization.Protobuf` supplies bidirectional extension conversions
between NodaTime temporal values and protobuf wire types: `Instant`/`Duration`
against the `Google.Protobuf.WellKnownTypes` `Timestamp`/`Duration`, and
`LocalDate`/`LocalTime`/`IsoDayOfWeek` against the `Google.Type` common protos
(`Date`/`TimeOfDay`/`DayOfWeek`) shipped by `Google.Api.CommonProtos`. Version
 is Apache-2.0 and ships `lib/netstandard2.0` only (the `net10.0` consumer
binds it directly). The package is two static extension classes and nothing else —
it owns no message type, no codec, and no `Google.Protobuf`/`Grpc.Net.Client`
surface; it composes ONTO the wire types those packages own, converting at the gRPC
boundary so the `remote-contracts` rail carries temporal values as well-known /
common-proto messages and the interior carries NodaTime values exclusively.

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

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------- |:---------------- |:---------------------------- |
| [01] | `NodaExtensions` | extension surface | projects NodaTime to protobuf |
| [02] | `ProtobufExtensions` | extension surface | projects protobuf to NodaTime |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: NodaTime to protobuf projection (`NodaExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:-------------------- |:---------------------------- |:------------------------ |
| [01] | `ToTimestamp` | `Instant` extension | emits `Timestamp` |
| [02] | `ToProtobufDuration` | NodaTime `Duration` extension | emits protobuf `Duration` |
| [03] | `ToDate` | `LocalDate` extension | emits `Google.Type.Date` |
| [04] | `ToTimeOfDay` | `LocalTime` extension | emits `TimeOfDay` |
| [05] | `ToProtobufDayOfWeek` | `IsoDayOfWeek` extension | emits `DayOfWeek` enum |

[ENTRYPOINT_SCOPE]: protobuf to NodaTime projection (`ProtobufExtensions`)
- rail: remote-contracts

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------- |:---------------------------- |:------------------------ |
| [01] | `ToInstant` | `Timestamp` extension | emits `Instant` |
| [02] | `ToNodaDuration` | protobuf `Duration` extension | emits NodaTime `Duration` |
| [03] | `ToLocalDate` | `Google.Type.Date` extension | emits `LocalDate` |
| [04] | `ToLocalTime` | `TimeOfDay` extension | emits `LocalTime` |
| [05] | `ToIsoDayOfWeek` | `DayOfWeek` enum extension | emits `IsoDayOfWeek` |

## [04]-[IMPLEMENTATION_LAW]

[CONVERSION_CONTRACTS]:
- namespace: `NodaTime.Serialization.Protobuf`
- well-known root: `Instant`/`Duration` pair with `Google.Protobuf.WellKnownTypes.Timestamp`/`Duration` (the `Google.Protobuf` package owns the message types)
- common-proto root: `LocalDate`/`LocalTime`/`IsoDayOfWeek` pair with `Google.Type.Date`/`TimeOfDay`/`DayOfWeek` — the `Google.Type` protos ship in the transitive `Google.Api.CommonProtos` dependency, not in `Google.Protobuf`
- direction law: `NodaExtensions` projects outward; `ProtobufExtensions` projects inward; no codec or message ownership
- rail stacking: a remote Compute contract field is a `Timestamp`/`Duration`/`Date`/`TimeOfDay`/`DayOfWeek` message (the `api-protobuf` `Google.Protobuf.WellKnownTypes` / `Google.Type` owner mints the carrier) on the `Grpc.Net.Client` channel and a NodaTime value at the rail — the inbound dispatch reads `ToInstant`/`ToNodaDuration`/`ToLocalDate`/`ToLocalTime`/`ToIsoDayOfWeek` once at the seam onto a `Fin`/`Validation` rail, and the outbound response writes `ToTimestamp`/`ToProtobufDuration`/`ToDate`/`ToTimeOfDay`/`ToProtobufDayOfWeek` once; the `Runtime/wire` `FrameEdge.Transaction` realized form writes `hlc.ToTimestamp` onto the `TransactionRequest` HLC-physical field, the single staging site for the clock seam. Under the `RemoteTransport.InProcess` row (`.api/api-microsoftaspnetcoretesthost.md`) the same conversions ride the in-memory handler so the temporal wire contract is proven without a live remote

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
