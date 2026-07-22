# [RASM_APPHOST_API_GRPC_CORE_API]

`Grpc.Core.Api` owns the gRPC service-definition surface AppHost `ControlService` binds: `Marshaller` codec pairs, `Method<TReq,TResp>` descriptors, the `ServerServiceDefinition` handler registry, the `Status`/`StatusCode` result model with `RpcException` as its typed fault carrier, and `CallOptions` for propagated outbound calls.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Core.Api`
- package: `Grpc.Core.Api`
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- asset: runtime library
- rail: gRPC service-definition boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: status model, method descriptors, and call configuration

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `Status`                  | struct        | `StatusCode` + `Detail` result carrier          |
|  [02]   | `StatusCode`              | enum          | gRPC status result vocabulary                   |
|  [03]   | `RpcException`            | class         | typed fault carrying `Status` and trailers      |
|  [04]   | `Marshaller<T>`           | class         | serializer + deserializer pair per message type |
|  [05]   | `Marshallers`             | class         | marshaller factory and `StringMarshaller`       |
|  [06]   | `Method<TReq,TResp>`      | class         | method descriptor keyed by `FullName`           |
|  [07]   | `MethodType`              | enum          | the four call-shape cases                       |
|  [08]   | `ServerServiceDefinition` | class         | method-to-handler registration builder          |
|  [09]   | `CallOptions`             | struct        | outbound call headers, deadline, credentials    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Status` construction and the `StatusCode` roster

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                      |
| :-----: | :--------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Status(StatusCode, string)`             | ctor    | primary code + detail result      |
|  [02]   | `Status(StatusCode, string, Exception?)` | ctor    | with client-side `DebugException` |
|  [03]   | `Status.DefaultSuccess`                  | static  | `OK` + empty detail               |
|  [04]   | `Status.DefaultCancelled`                | static  | `Cancelled` + empty detail        |

[StatusCode]: `OK=0` `Cancelled=1` `Unknown=2` `InvalidArgument=3` `DeadlineExceeded=4` `NotFound=5` `AlreadyExists=6` `PermissionDenied=7` `ResourceExhausted=8` `FailedPrecondition=9` `Aborted=10` `OutOfRange=11` `Unimplemented=12` `Internal=13` `Unavailable=14` `DataLoss=15` `Unauthenticated=16`

[ENTRYPOINT_SCOPE]: `RpcException` construction

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------- | :------- | :------------------------------ |
|  [01]   | `RpcException(Status)`                   | ctor     | status-only; trailers = `Empty` |
|  [02]   | `RpcException(Status, string)`           | ctor     | custom local message            |
|  [03]   | `RpcException(Status, Metadata)`         | ctor     | with trailing metadata          |
|  [04]   | `RpcException(Status, Metadata, string)` | ctor     | full form                       |
|  [05]   | `.Status` / `.StatusCode` / `.Trailers`  | property | read-back of constructed values |

[ENTRYPOINT_SCOPE]: marshaller, method descriptor, and service definition

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Marshaller<T>(Func<T,byte[]>, Func<byte[],T>)`                                 | ctor     | byte-array codec pair                   |
|  [02]   | `Marshaller<T>(Action<T,SerializationContext>, Func<DeserializationContext,T>)` | ctor     | contextual pooling codec                |
|  [03]   | `Marshallers.Create<T>(serializer, deserializer)`                               | static   | byte-array or contextual shorthand      |
|  [04]   | `Marshallers.StringMarshaller`                                                  | static   | UTF-8 string marshaller                 |
|  [05]   | `Method<TReq,TResp>(MethodType, string, string, Marshaller, Marshaller)`        | ctor     | method descriptor                       |
|  [06]   | `Method.Type` / `.FullName` / `.RequestMarshaller`                              | property | call shape, dispatch key, codecs        |
|  [07]   | `ServerServiceDefinition.CreateBuilder()`                                       | factory  | begin registration                      |
|  [08]   | `Builder.AddMethod(Method<TReq,TResp>, handler)`                                | instance | register handler; 4 streaming overloads |
|  [09]   | `Builder.Build()`                                                               | instance | immutable `ServerServiceDefinition`     |
|  [10]   | `ServerServiceDefinition.BindService(ServiceBinderBase)`                        | instance | replay registrations into a binder      |

[ENTRYPOINT_SCOPE]: `CallOptions` — outbound call configuration

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `CallOptions(Metadata?, DateTime?, CancellationToken, ...)`                     | ctor     | full options struct                      |
|  [02]   | `WithHeaders(Metadata)` / `WithDeadline(DateTime)`                              | instance | immutable update                         |
|  [03]   | `WithCancellationToken(CancellationToken)` / `WithCredentials(CallCredentials)` | instance | immutable update                         |
|  [04]   | `WithPropagationToken(ContextPropagationToken)` / `WithWaitForReady(bool)`      | instance | propagation and `TransientFailure` retry |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every server method is one `Method<TReq,TResp>` descriptor; its `FullName` (`serviceName/name`) keys dispatch and matches the generated proto service name exactly.
- `ServerServiceDefinition.Builder.AddMethod` registers a handler per method; `Build()` rejects a duplicate `FullName` with `ArgumentException`.
- `Marshaller<T>` carries both a byte-array codec and a contextual codec (`ContextualSerializer` / `ContextualDeserializer`); the contextual pair binds buffer pooling.
- Failure propagates as `Status` (`StatusCode` + `Detail`) or a thrown `RpcException`; `Status.Detail` and trailing `Metadata` are the only peer-visible fault channel.

[STACKING]:
- `Grpc.Core.Api`(`../../Rasm.Compute/.api/api-grpc-common.md`): the per-call `ServerCallContext`, stream reader/writer, and `Metadata` surface the same `ControlService` binds is owned there; this catalog composes it as the method-registration and fault half of one rail.
- `Rasm.AppHost` `ControlService`: builds its service as one `ServerServiceDefinition` of `Method<TReq,TResp>` descriptors over `Marshallers.Create` codec pairs, folding every handler failure onto the `Status`/`RpcException` rail.

[LOCAL_ADMISSION]:
- `Status.DebugException` and `RpcException.Message` are local diagnostics; `Status.Detail` and trailing `Metadata` carry the wire fault.
- `CallOptions.WithPropagationToken` binds the `ContextPropagationToken` a parent context mints, inheriting its deadline on the downstream call.

[RAIL_LAW]:
- Package: `Grpc.Core.Api`
- Owns: the gRPC method descriptor, service-definition registry, marshaller pairs, and `Status`/`StatusCode`/`RpcException` fault model
- Accept: hand-registered `Method` descriptors, contextual marshallers, and typed-fault propagation on the AppHost control rail
- Reject: a parallel error DTO beside `Status`; the per-call context, streaming, and metadata surface the Compute `api-grpc-common.md` catalog owns
