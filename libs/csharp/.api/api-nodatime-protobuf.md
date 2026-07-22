# [RASM_API_NODATIME_PROTOBUF]

`NodaTime.Serialization.Protobuf` owns temporal conversion at the remote-contract boundary: NodaTime values project onto protobuf messages outward and back inward through paired inverses. Message ownership, codecs, and wire frames stay with their owning packages.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NodaTime.Serialization.Protobuf`
- package: `NodaTime.Serialization.Protobuf` (Apache-2.0, The Noda Time authors)
- assembly: `NodaTime.Serialization.Protobuf` (binds `lib/netstandard2.0`)
- namespace: `NodaTime.Serialization.Protobuf`
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: paired conversion owners

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :------------------- | :------------ | :------------------------------ |
|  [01]   | `NodaExtensions`     | class         | projects NodaTime onto the wire |
|  [02]   | `ProtobufExtensions` | class         | projects the wire onto NodaTime |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: outward projection (`NodaExtensions`)

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `ToTimestamp(Instant) -> Timestamp`                                | static  | floors at `NodaConstants.BclEpoch` |
|  [02]   | `ToProtobufDuration(NodaTime.Duration) -> WellKnownTypes.Duration` | static  | bounded to the protobuf window     |
|  [03]   | `ToDate(LocalDate) -> Date`                                        | static  | floors at ISO year 1               |
|  [04]   | `ToTimeOfDay(LocalTime) -> TimeOfDay`                              | static  | total over every local time        |
|  [05]   | `ToProtobufDayOfWeek(IsoDayOfWeek) -> DayOfWeek`                   | static  | `None` maps to `Unspecified`       |

[ENTRYPOINT_SCOPE]: inward projection (`ProtobufExtensions`)

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `ToInstant(Timestamp) -> Instant`                              | static  | validates seconds and nanos        |
|  [02]   | `ToNodaDuration(WellKnownTypes.Duration) -> NodaTime.Duration` | static  | validates range and sign agreement |
|  [03]   | `ToLocalTime(TimeOfDay) -> LocalTime`                          | static  | rejects leap second and 24:00      |
|  [04]   | `ToLocalDate(Date) -> LocalDate`                               | static  | rejects zero-valued fields         |
|  [05]   | `ToIsoDayOfWeek(DayOfWeek) -> IsoDayOfWeek`                    | static  | `Unspecified` maps to `None`       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- An out-of-range value throws `ArgumentOutOfRangeException`; a malformed or null message throws `ArgumentException` or `ArgumentNullException`.
- `Google.Api.CommonProtos` owns the `Google.Type` calendar messages this surface converts.
- Inward dates land in the ISO calendar.
- `IsoDayOfWeek.None` and `DayOfWeek.Unspecified` map to each other, so an unset weekday field round-trips without an option wrapper.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): outward projections mint the well-known messages that `MessageParser<T>` and the `IBufferWriter<byte>` write path then carry as ordinary contract fields.
- `NodaTime`(`.api/api-nodatime.md`): a clock read or pattern parse mints the outward argument, and the inward leg returns to that same value family.
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): converted messages ride request and response fields, while `CallOptions.Deadline` takes BCL `DateTime?` and draws its budget from `Instant.ToDateTimeUtc`.
- remote-contracts seam: one bidirectional stamp owner pairs each outward projection with its inverse over the same message type, so a contract field's read and write legs share one conversion row.

[LOCAL_ADMISSION]:
- Temporal values cross remote contracts as protobuf messages and convert once at the seam, so interior owners hold NodaTime values alone.
- Seam call sites project each conversion throw onto the typed rail.

[RAIL_LAW]:
- Package: `NodaTime.Serialization.Protobuf`
- Owns: temporal conversion between NodaTime values and protobuf wire messages
- Accept: paired boundary extension calls on the seam's inward and outward legs
- Reject: hand-rolled epoch arithmetic between NodaTime values and wire payloads
