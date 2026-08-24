# [RASM_API_GRPC_STATUSPROTO]

`Grpc.StatusProto` owns the rich-error carriage between a `google.rpc.Status` message and the gRPC call: the producer folds a `Status` carrying typed `Any` details into the `grpc-status-details-bin` trailer and raises it as one `RpcException`, the client reads the same trailer back off the raised exception. It ships four extension classes over `Google.Api.CommonProtos` and `Grpc.Core.Api` alone — no transport, no hosting.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.StatusProto`
- package: `Grpc.StatusProto` (Apache-2.0)
- assembly: `Grpc.StatusProto`
- namespace: `Grpc.Core`
- depends: `Google.Api.CommonProtos` (`Google.Rpc.Status`, `DebugInfo`), `Grpc.Core.Api` (`Metadata`, `RpcException`, `Status`, `StatusCode`)
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension owners, each a static class over one carrier

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `MetadataExtensions`     | static class  | trailer read and write of the `google.rpc.Status`   |
|  [02]   | `RpcExceptionExtensions` | static class  | status read off a raised call                       |
|  [03]   | `RpcStatusExtensions`    | static class  | raise a `google.rpc.Status` as one `RpcException`   |
|  [04]   | `ExceptionExtensions`    | static class  | project a CLR exception onto `google.rpc.DebugInfo` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: trailer carriage

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `MetadataExtensions.StatusDetailsTrailerName`                                | const   | `"grpc-status-details-bin"`                  |
|  [02]   | `Metadata.GetRpcStatus(bool ignoreParseError = false) -> Google.Rpc.Status?` | static  | parse the trailer; `null` when absent        |
|  [03]   | `Metadata.SetRpcStatus(Google.Rpc.Status)`                                   | static  | swaps all trailer entries for one status     |
|  [04]   | `RpcException.GetRpcStatus() -> Google.Rpc.Status?`                          | static  | `Trailers.GetRpcStatus()` on the raised call |

[ENTRYPOINT_SCOPE]: raising and projecting

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `Google.Rpc.Status.ToRpcException() -> RpcException`                   | static  | `Code` → `StatusCode`, `Message` → detail        |
|  [02]   | `Google.Rpc.Status.ToRpcException(StatusCode, string) -> RpcException` | static  | caller-chosen wire code and detail, same trailer |
|  [03]   | `Exception.ToRpcDebugInfo(int innerDepth = 0) -> Google.Rpc.DebugInfo` | static  | type-qualified message, stack lines, inner chain |

- `Metadata.GetRpcStatus`: `ignoreParseError` false THROWS `InvalidProtocolBufferException` on a malformed trailer — absent and malformed are two verdicts only under the default; `true` collapses both to `null`.
- `Status.ToRpcException()`: a `Code` outside `StatusCode` lands `StatusCode.Unknown` on the wire while the trailer keeps the original integer; the trailer, not the wire code, is the detail carrier.
- `Metadata.SetRpcStatus`: removes every existing `grpc-status-details-bin` entry before adding one, so a stamped trailer never carries two statuses.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One trailer key carries the whole richer-error model: `google.rpc.Status{code, message, details}` serialized whole, details as `Any` the client resolves by descriptor.
- `ToRpcException` is the producer mint and `GetRpcStatus` the client read of one correspondence; the trailer constant never reaches consumer code because both members spell it.
- `ToRpcDebugInfo` is the ONE projection of a CLR exception the wire admits; a hand-rendered stack string beside it is the deleted form.

[STACKING]:
- `Google.Api.CommonProtos`(`.api/api-commonprotos.md`): `Google.Rpc.Status` is the message this package serializes; `Status.GetDetail<T>()` and `UnpackDetailMessages(TypeRegistry)` read the details the estate packs.
- `Grpc.Core.Api`(`.api/api-grpc-core-api.md`): `Metadata`, `RpcException`, `Status`, and `StatusCode` are the carriers; `Metadata.BinaryHeaderSuffix` is why the key ends in `-bin`.
- `Rasm.AppHost` (`Runtime/ports#WIRE_LAW`): `FaultWire.Raise` folds a `Fault` into `Google.Rpc.Status{Code, Message, Details = {Any.Pack(FaultDetail)[, Any.Pack(detail.Recovery.RetryAfter)]}}.ToRpcException()` at every failing handler arm, the advice seat packing the detail's own throttled arm rather than a second mint; `FaultWire.Decode` reads `RpcException.GetRpcStatus()` under `Op.Catch`, so a malformed trailer lands typed on `WireBoundary.RemoteStatus` and an absent one answers `None`.
- `Rasm.Compute` (`Runtime/wire#FAULT_PROJECTION`): the client transport rail composes `FaultWire.Decode` before `StatusRail` classifies a residual status.
- `Rasm.Persistence` (`Query/federation#FLIGHT_RESULT_PLANE`): Flight verb refusals raise through `FaultWire.Raise`, never a local `new RpcException(new Status(code, message))`.

[LOCAL_ADMISSION]:
- Every producer status mints through `ToRpcException`; `ServerCallContext.ResponseTrailers` is never written by hand.
- Every client detail read goes through `GetRpcStatus()` under the default parse posture, so malformed and absent stay distinct.

[RAIL_LAW]:
- Package: `Grpc.StatusProto`
- Owns: the `grpc-status-details-bin` carriage of `google.rpc.Status` in both directions and the exception-to-`DebugInfo` projection
- Accept: one `Status.ToRpcException()` per failing arm at the producer, one `RpcException.GetRpcStatus()` per caught call at the client
- Reject: a hand `Metadata.Add("grpc-status-details-bin", …)`, a `Status.Parser.ParseFrom(trailer)` beside `GetRpcStatus`, `ignoreParseError: true` at an admission that distinguishes malformed from absent, a stack string rendered outside `ToRpcDebugInfo`
