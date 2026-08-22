# [RASM_API_COMMONPROTOS]

`Google.Api.CommonProtos` owns the generated `google.rpc` and `google.type` vocabularies the corpus imports: the `Status` envelope every gRPC rich error rides, the ten standard error details and their registry, the canonical `Code` roster, and the calendar scalars (`Date`, `TimeOfDay`, `DateTime`, `DayOfWeek`) the element and host families declare. It is a generated-message distribution over `Google.Protobuf`; the carriage of `Status` onto a call is `Grpc.StatusProto`'s.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Api.CommonProtos`
- package: `Google.Api.CommonProtos` (Apache-2.0)
- assembly: `Google.Api.CommonProtos`
- namespace: `Google.Rpc`, `Google.Rpc.Context`, `Google.Type`, `Google.Api`
- depends: `Google.Protobuf`
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `google.rpc` status and error details

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Status`                                   | message       | `Code`, `Message`, `Details` (`RepeatedField<Any>`)                                                |
|  [02]   | `Code`                                     | enum          | canonical codes, numerically equal to `Grpc.Core.StatusCode`                                       |
|  [03]   | `StandardErrorTypeRegistry`                | static class  | `TypeRegistry` over the ten standard detail messages                                               |
|  [04]   | `ErrorInfo`                                | message       | `Reason`, `Domain`, `Metadata` (`MapField<string,string>`)                                         |
|  [05]   | `RetryInfo`                                | message       | `RetryDelay` (`Duration`)                                                                          |
|  [06]   | `DebugInfo`                                | message       | `StackEntries`, `Detail`                                                                           |
|  [07]   | `QuotaFailure` + `.Types.Violation`        | message       | `Subject`, `Description`, `ApiService`, `QuotaMetric`, `QuotaId`, `QuotaValue`, `FutureQuotaValue` |
|  [08]   | `PreconditionFailure` + `.Types.Violation` | message       | `Type`, `Subject`, `Description`                                                                   |
|  [09]   | `BadRequest` + `.Types.FieldViolation`     | message       | `Field`, `Description`, `Reason`, `LocalizedMessage`                                               |
|  [10]   | `RequestInfo`                              | message       | `RequestId`, `ServingData`                                                                         |
|  [11]   | `ResourceInfo`                             | message       | `ResourceType`, `ResourceName`, `Owner`, `Description`                                             |
|  [12]   | `Help` + `.Types.Link`                     | message       | `Description`, `Url`                                                                               |
|  [13]   | `LocalizedMessage`                         | message       | `Locale`, `Message`                                                                                |

[PUBLIC_TYPE_SCOPE]: `google.type` calendar and value scalars

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Date`           | message       | `Year`, `Month`, `Day`; zero members spell an unspecified part |
|  [02]   | `TimeOfDay`      | message       | `Hours`, `Minutes`, `Seconds`, `Nanos`                         |
|  [03]   | `DateTime`       | message       | civil fields plus `TimeOffset` oneof `UtcOffset` / `TimeZone`  |
|  [04]   | `DayOfWeek`      | enum          | `Unspecified` + Monday…Sunday                                  |
|  [05]   | `Interval`       | message       | `StartTime`, `EndTime` (`Timestamp`)                           |
|  [06]   | `Money`          | message       | `CurrencyCode`, `Units`, `Nanos`; `DecimalValue` projection    |
|  [07]   | `Decimal`        | message       | `Value` decimal text                                           |
|  [08]   | `DateExtensions` | static class  | `ToDate(DateTime)`, `ToDate(DateTimeOffset)`                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: status details

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                                                                                                      |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Status.GetDetail<T>() -> T?`                                        | instance | first detail whose type name matches `T`; `null` when absent                                                                      |
|  [02]   | `Status.UnpackDetailMessages() -> IEnumerable<IMessage>`             | instance | every detail resolvable by the standard registry                                                                                  |
|  [03]   | `Status.UnpackDetailMessages(TypeRegistry) -> IEnumerable<IMessage>` | instance | every detail resolvable by the caller's registry                                                                                  |
|  [04]   | `StandardErrorTypeRegistry.Registry -> TypeRegistry`                 | static   | ErrorInfo, BadRequest, RetryInfo, DebugInfo, QuotaFailure, PreconditionFailure, RequestInfo, ResourceInfo, Help, LocalizedMessage |

- `Status.GetDetail<T>`: an `Any` of the right type name that fails to unpack throws; the estate filters by `Any.Is(FaultDetail.Descriptor)` and unpacks under `Op.Catch` to keep malformed distinct from absent.
- `UnpackDetailMessages(registry)`: silently drops a detail the registry cannot resolve, so an estate detail reaches it only through a registry carrying the estate descriptors.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Status.details` is the ONE extension slot on a gRPC error; an estate detail is one `Any` inside it beside the standard details, never a second trailer.
- `Code` and `Grpc.Core.StatusCode` share numerics, so `(int)StatusCode` seeds `Status.Code` and `ToRpcException` reads it back with no table.
- `RetryInfo` states a delay alone; `terminal` versus `transient` is unspellable in it, which is why the estate's `FaultRecovery` oneof rides beside a `RetryInfo` emitted on the throttled arm.
- `google.type.Date`/`TimeOfDay`/`DayOfWeek` are the calendar wire scalars; `NodaTime.Serialization.Protobuf` owns their projection onto `LocalDate`/`LocalTime`/`IsoDayOfWeek`.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): every type here is a generated `IMessage<T>` with `Parser` and `Descriptor`; `TypeRegistry.FromFiles` over the estate `<F>Reflection` descriptors resolves `Status.details` beside `StandardErrorTypeRegistry`.
- `Grpc.StatusProto`(`.api/api-grpc-statusproto.md`): carries `Status` onto the trailer and back.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Date.ToLocalDate`, `TimeOfDay.ToLocalTime`, `DayOfWeek.ToIsoDayOfWeek` and their inverses.
- `ProtoValidate`(`.api/api-protovalidate.md`): a `Violation` projects onto `BadRequest.Types.FieldViolation{Field = path, Reason = ruleId, Description = message}`.
- `Rasm.Contracts`(`Rasm.Contracts/.api/rasm-contracts.md`): `fault.v1.FaultDetail.violations` is `repeated google.rpc.BadRequest.FieldViolation`; `element.v1` and `host.v1` declare `google.type.Date`, `DateTime`, `TimeOfDay`.
- `Rasm.AppHost` (`Runtime/ports#WIRE_LAW`): `FaultWire.Raise` mints `Status{Code, Message, Details}`; `FaultWire.Decode` filters `Details` on `Any.Is(FaultDetail.Descriptor)`; `FaultWire.Pack` fills `FaultDetail.violations` from the admission's `FieldViolation` rows.

[LOCAL_ADMISSION]:
- `Status` enters and leaves only through `Grpc.StatusProto`; the estate never serializes it by hand.
- Calendar scalars cross into domain time through the NodaTime bridge at the wire edge; no `google.type` value lives past a seam.

[RAIL_LAW]:
- Package: `Google.Api.CommonProtos`
- Owns: the `google.rpc` status and error-detail messages, the canonical code roster, the standard detail registry, and the `google.type` scalars
- Accept: `Status` as the one rich-error envelope, standard details packed beside the estate detail, calendar scalars as declared wire fields
- Reject: a hand-written status or error-detail record, `ErrorInfo.Metadata` carrying typed estate columns as strings, `RetryInfo` standing in for the estate recovery oneof
