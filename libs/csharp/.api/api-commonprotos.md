# [RASM_API_COMMONPROTOS]

`Google.Api.CommonProtos` owns the generated `google.rpc` and `google.type` vocabularies the corpus imports: the `Status` envelope every gRPC rich error rides, the ten standard error details and their registry, the canonical `Code` roster, and the calendar scalars (`Date`, `TimeOfDay`, `DateTime`, `DayOfWeek`) the element and host families declare. It is a generated-message distribution over `Google.Protobuf`; the carriage of `Status` onto a call is `Grpc.StatusProto`'s.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Api.CommonProtos`
- package: `Google.Api.CommonProtos` (BSD-3-Clause)
- assembly: `Google.Api.CommonProtos`
- namespace: `Google.Rpc`, `Google.Rpc.Context`, `Google.Type`, `Google.Api`
- depends: `Google.Protobuf`
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `google.rpc` status and error details

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Status`                                   | message       | `Code`, `Message`, `Details` (`RepeatedField<Any>`)          |
|  [02]   | `Code`                                     | enum          | canonical codes, numerically equal to `Grpc.Core.StatusCode` |
|  [03]   | `StandardErrorTypeRegistry`                | static class  | `TypeRegistry` over the ten standard detail messages         |
|  [04]   | `ErrorInfo`                                | message       | `Reason`, `Domain`, `Metadata` (`MapField<string,string>`)   |
|  [05]   | `RetryInfo`                                | message       | `RetryDelay` (`Duration`)                                    |
|  [06]   | `DebugInfo`                                | message       | `StackEntries`, `Detail`                                     |
|  [07]   | `QuotaFailure` + `.Types.Violation`        | message       | `Violations` (`RepeatedField<Violation>`)                    |
|  [08]   | `PreconditionFailure` + `.Types.Violation` | message       | `Type`, `Subject`, `Description`                             |
|  [09]   | `BadRequest` + `.Types.FieldViolation`     | message       | `Field`, `Description`, `Reason`, `LocalizedMessage`         |
|  [10]   | `RequestInfo`                              | message       | `RequestId`, `ServingData`                                   |
|  [11]   | `ResourceInfo`                             | message       | `ResourceType`, `ResourceName`, `Owner`, `Description`       |
|  [12]   | `Help` + `.Types.Link`                     | message       | `Description`, `Url`                                         |
|  [13]   | `LocalizedMessage`                         | message       | `Locale`, `Message`                                          |

- `QuotaFailure.Types.Violation`: `Subject`, `Description`, `ApiService`, `QuotaMetric`, `QuotaId`, `QuotaValue`, `FutureQuotaValue`.

[PUBLIC_TYPE_SCOPE]: `google.type` calendar and value scalars

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Date`                         | message       | `Year`, `Month`, `Day`; zero members spell an unspecified part |
|  [02]   | `TimeOfDay`                    | message       | `Hours`, `Minutes`, `Seconds`, `Nanos`                         |
|  [03]   | `DateTime`                     | message       | civil fields plus the `time_offset` oneof                      |
|  [04]   | `DateTime.TimeOffsetOneofCase` | enum          | `None`, `UtcOffset`, `TimeZone`                                |
|  [05]   | `TimeZone`                     | message       | IANA zone `Id` plus optional database `Version`                |
|  [06]   | `DayOfWeek`                    | enum          | `Unspecified` + Monday…Sunday                                  |
|  [07]   | `Interval`                     | message       | `StartTime`, `EndTime` (`Timestamp`)                           |
|  [08]   | `Money`                        | message       | `CurrencyCode`, `Units`, `Nanos`; `DecimalValue` projection    |
|  [09]   | `Decimal`                      | message       | `Value` decimal text                                           |
|  [10]   | `DateExtensions`               | static class  | `ToDate(DateTime)`, `ToDate(DateTimeOffset)`                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: status details

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `Status.GetDetail<T>() -> T?`                                        | instance | first detail whose type name matches `T`, else `null` |
|  [02]   | `Status.UnpackDetailMessages() -> IEnumerable<IMessage>`             | instance | every detail resolvable by the standard registry      |
|  [03]   | `Status.UnpackDetailMessages(TypeRegistry) -> IEnumerable<IMessage>` | instance | every detail resolvable by the caller's registry      |
|  [04]   | `StandardErrorTypeRegistry.Registry -> TypeRegistry`                 | static   | every standard detail message                         |

- `Status.GetDetail<T>`: an `Any` of the right type name that fails to unpack throws; the estate filters by `Any.Is(FaultDetail.Descriptor)` and unpacks under `Op.Catch` to keep malformed distinct from absent.
- `UnpackDetailMessages(registry)`: silently drops a detail the registry cannot resolve, so an estate detail reaches it only through a registry carrying the estate descriptors.

[ENTRYPOINT_SCOPE]: `Google.Type` calendar fields

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `Date.Year` / `.Month` / `.Day`                        | property | whole or partial Gregorian calendar date |
|  [02]   | `TimeOfDay.Hours` / `.Minutes` / `.Seconds` / `.Nanos` | property | local wall-clock time                    |
|  [03]   | `DateTime.Year` / `.Month` / `.Day`                    | property | proleptic-Gregorian civil date           |
|  [04]   | `DateTime.Hours` / `.Minutes` / `.Seconds` / `.Nanos`  | property | civil time with nanosecond fraction      |
|  [05]   | `DateTime.UtcOffset`                                   | property | `Duration` arm of `time_offset`          |
|  [06]   | `DateTime.TimeZone`                                    | property | `Google.Type.TimeZone` arm               |
|  [07]   | `DateTime.TimeOffsetCase`                              | property | active offset arm                        |
|  [08]   | `DateTime.ClearTimeOffset()`                           | instance | restore local-time `None` arm            |
|  [09]   | `TimeZone.Id` / `.Version`                             | property | IANA coordinate and database release     |

- Every calendar type is an ordinary generated `IMessage<T>` with `Parser`, `Descriptor`, `Clone`, protobuf equality, and unknown-field preservation from `Google.Protobuf`.
- `Date` permits zero-valued components for partial dates. `DateTime` permits year zero but requires a real month and day by contract; its generated setters do not enforce those semantic ranges.
- `DateTime.TimeOffsetCase.None` is a local civil time. `UtcOffset` and `TimeZone` are mutually exclusive arms, and `ClearTimeOffset()` returns the message to `None`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Status.details` is the ONE extension slot on a gRPC error; an estate detail is one `Any` inside it beside the standard details, never a second trailer.
- `Code` and `Grpc.Core.StatusCode` share numerics, so `(int)StatusCode` seeds `Status.Code` and `ToRpcException` reads it back with no table.
- `RetryInfo` states a delay alone; `terminal` versus `transient` is unspellable in it, which is why the estate wraps it in a `FaultRecovery` oneof whose throttled arm IS a `RetryInfo`, so one instance seats both inside the estate detail and beside it in `Status.details`.
- `google.type.Date`/`TimeOfDay`/`DayOfWeek` are the calendar wire scalars; `NodaTime.Serialization.Protobuf` owns their projection onto `LocalDate`/`LocalTime`/`IsoDayOfWeek`.
- `google.type.DateTime` is deliberately broader than NodaTime `LocalDateTime`: a local-only seam requires `TimeOffsetCase.None` and validates complete date/time fields before constructing the domain value.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): every type here is a generated `IMessage<T>` with `Parser` and `Descriptor`; `TypeRegistry.FromFiles` over the estate `<F>Reflection` descriptors resolves `Status.details` beside `StandardErrorTypeRegistry`.
- `Grpc.StatusProto`(`.api/api-grpc-statusproto.md`): carries `Status` onto the trailer and back.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Date.ToLocalDate`, `TimeOfDay.ToLocalTime`, `DayOfWeek.ToIsoDayOfWeek` and their inverses.
- `Celly.Protovalidate`(`.api/api-celly-protovalidate.md`): each `Violation` projects onto `BadRequest.Types.FieldViolation` at admission.
- `Rasm.Contracts`(`Rasm.Contracts/.api/rasm-contracts.md`): `fault.v1.FaultDetail.violations` is `repeated google.rpc.BadRequest.FieldViolation`; `element.v1`, `ui.v1`, and `declaration.v1` declare `google.type.Date`, `DateTime`, or `TimeOfDay` where owned.
- `Rasm.AppHost` (`Runtime/ports#WIRE_LAW`): `FaultWire.Raise` mints `Status{Code, Message, Details}` and packs the detail's own `Recovery.RetryAfter` as the standard advice seat; `FaultWire.Decode` filters `Details` on `Any.Is(FaultDetail.Descriptor)`; `FaultWire.Pack` fills `FaultDetail.violations` from the admission's `FieldViolation` rows.

[LOCAL_ADMISSION]:
- `Status` enters and leaves only through `Grpc.StatusProto`; the estate never serializes it by hand.
- Calendar scalars cross into domain time through the NodaTime bridge at the wire edge; no `google.type` value lives past a seam.

[RAIL_LAW]:
- Package: `Google.Api.CommonProtos`
- Owns: the `google.rpc` status and error-detail messages, the canonical code roster, the standard detail registry, and the `google.type` scalars
- Accept: `Status` as the one rich-error envelope, standard details packed beside the estate detail, calendar scalars as declared wire fields
- Reject: a hand-written status or error-detail record, `ErrorInfo.Metadata` carrying typed estate columns as strings, `RetryInfo` standing in for the estate recovery oneof
