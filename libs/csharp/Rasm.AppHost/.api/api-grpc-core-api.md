# [RASM_APPHOST_API_GRPC_CORE_API]

`Grpc.Core.Api` supplies the server-side gRPC contract surface consumed by AppHost ControlService: call context, streaming reader/writer interfaces, metadata collection, status model, exception type, call options, marshaller, method descriptor, and service definition builder.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Core.Api`

- package: `Grpc.Core.Api`
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- asset: runtime library
- rail: gRPC server boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: call context and streaming interfaces

| [INDEX] | [SYMBOL]                 | [KIND]         | [ROLE]                                                       |
| :-----: | :----------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `ServerCallContext`      | abstract class | per-call server context: headers, deadline, CT               |
|  [02]   | `IServerStreamWriter<T>` | interface      | server-side outbound stream; extends `IAsyncStreamWriter<T>` |
|  [03]   | `IAsyncStreamReader<T>`  | interface      | server-side inbound stream; `Current`+`MoveNext`             |
|  [04]   | `IAsyncStreamWriter<T>`  | interface      | `WriteAsync(T)`, `WriteOptions` property                     |

[PUBLIC_TYPE_SCOPE]: metadata and status

| [INDEX] | [SYMBOL]         | [KIND] | [ROLE]                                                        |
| :-----: | :--------------- | :----- | :------------------------------------------------------------ |
|  [01]   | `Metadata`       | class  | `IList<Metadata.Entry>`; request headers and trailers         |
|  [02]   | `Metadata.Entry` | class  | single key/value or key/binary pair; key lowercased           |
|  [03]   | `Status`         | struct | `StatusCode` + `Detail` string result carrier                 |
|  [04]   | `StatusCode`     | enum   | 17 gRPC status codes (OK=0 … DataLoss=15)                     |
|  [05]   | `RpcException`   | class  | `Exception` subtype carrying `Status` and trailing `Metadata` |

[PUBLIC_TYPE_SCOPE]: method descriptor and service definition

| [INDEX] | [SYMBOL]                  | [KIND] | [ROLE]                                                            |
| :-----: | :------------------------ | :----- | :---------------------------------------------------------------- |
|  [01]   | `Marshaller<T>`           | class  | serializer + deserializer pair for a single message type          |
|  [02]   | `Marshallers`             | class  | static helpers: `Create<T>(ser,des)`, built-in `StringMarshaller` |
|  [03]   | `Method<TReq,TResp>`      | class  | `MethodType`, `ServiceName`, `Name`, `FullName`, marshallers      |
|  [04]   | `MethodType`              | enum   | `Unary`, `ClientStreaming`, `ServerStreaming`, `DuplexStreaming`  |
|  [05]   | `ServerServiceDefinition` | class  | method-to-handler registry; `Builder` + `BindService`             |
|  [06]   | `CallOptions`             | struct | client-side call options: headers, deadline, CT, credentials      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ServerCallContext` — call properties

`AuthContext` carries no stability guarantee.

| [INDEX] | [SURFACE]                             | [KIND]   | [RAIL]                                   |
| :-----: | :------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `Method`                              | property | method name string                       |
|  [02]   | `Host`                                | property | host name string                         |
|  [03]   | `Peer`                                | property | remote endpoint URI string               |
|  [04]   | `Deadline`                            | property | `DateTime` (UTC)                         |
|  [05]   | `RequestHeaders`                      | property | inbound `Metadata`                       |
|  [06]   | `CancellationToken`                   | property | call and deadline cancellation           |
|  [07]   | `ResponseTrailers`                    | property | mutable outbound `Metadata`              |
|  [08]   | `Status`                              | property | get/set outbound `Status`                |
|  [09]   | `WriteOptions`                        | property | get/set write behaviour flags            |
|  [10]   | `AuthContext`                         | property | auth metadata                            |
|  [11]   | `UserState`                           | property | `IDictionary<object,object>` slot        |
|  [12]   | `WriteResponseHeadersAsync(Metadata)` | method   | send response headers before first write |
|  [13]   | `CreatePropagationToken(options?)`    | method   | context propagation to child calls       |

[ENTRYPOINT_SCOPE]: `Metadata` — collection operations

| [INDEX] | [SURFACE]                             | [KIND]   | [RAIL]                           |
| :-----: | :------------------------------------ | :------- | :------------------------------- |
|  [01]   | `Metadata()`                          | ctor     | empty mutable collection         |
|  [02]   | `Metadata.Empty`                      | static   | frozen empty singleton           |
|  [03]   | `Add(key, value)` / `Add(key, bytes)` | method   | ASCII or binary entry append     |
|  [04]   | `Get(key)`                            | method   | last entry by key or `null`      |
|  [05]   | `GetValue(key)`                       | method   | string value of last ASCII entry |
|  [06]   | `GetValueBytes(key)`                  | method   | bytes value of last entry        |
|  [07]   | `GetAll(key)`                         | method   | all entries with matching key    |
|  [08]   | `Count` / `IsReadOnly`                | property | list state                       |

[ENTRYPOINT_SCOPE]: `Status` construction and `StatusCode` cases

| [INDEX] | [SURFACE]                           | [KIND] | [RAIL]                     |
| :-----: | :---------------------------------- | :----- | :------------------------- |
|  [01]   | `Status(statusCode, detail)`        | ctor   | primary result constructor |
|  [02]   | `Status.DefaultSuccess`             | static | `OK` + empty detail        |
|  [03]   | `Status.DefaultCancelled`           | static | `Cancelled` + empty detail |
|  [04]   | `StatusCode.OK` = 0                 | case   | success                    |
|  [05]   | `StatusCode.Cancelled` = 1          | case   | caller-cancelled           |
|  [06]   | `StatusCode.Unknown` = 2            | case   | unknown failure            |
|  [07]   | `StatusCode.InvalidArgument` = 3    | case   | bad request input          |
|  [08]   | `StatusCode.DeadlineExceeded` = 4   | case   | deadline expired           |
|  [09]   | `StatusCode.NotFound` = 5           | case   | entity absent              |
|  [10]   | `StatusCode.AlreadyExists` = 6      | case   | entity already present     |
|  [11]   | `StatusCode.PermissionDenied` = 7   | case   | authorization failure      |
|  [12]   | `StatusCode.ResourceExhausted` = 8  | case   | quota or resource depleted |
|  [13]   | `StatusCode.FailedPrecondition` = 9 | case   | required state absent      |
|  [14]   | `StatusCode.Aborted` = 10           | case   | concurrency conflict       |
|  [15]   | `StatusCode.OutOfRange` = 11        | case   | value outside range        |
|  [16]   | `StatusCode.Unimplemented` = 12     | case   | method not supported       |
|  [17]   | `StatusCode.Internal` = 13          | case   | invariant broken           |
|  [18]   | `StatusCode.Unavailable` = 14       | case   | transient service failure  |
|  [19]   | `StatusCode.DataLoss` = 15          | case   | unrecoverable data loss    |
|  [20]   | `StatusCode.Unauthenticated` = 16   | case   | missing credentials        |

[ENTRYPOINT_SCOPE]: `RpcException` construction

| [INDEX] | [SURFACE]                                 | [KIND] | [RAIL]                          |
| :-----: | :---------------------------------------- | :----- | :------------------------------ |
|  [01]   | `RpcException(Status)`                    | ctor   | status-only; trailers = `Empty` |
|  [02]   | `RpcException(Status, string message)`    | ctor   | custom local message            |
|  [03]   | `RpcException(Status, Metadata trailers)` | ctor   | with trailing metadata          |
|  [04]   | `RpcException(Status, Metadata, string)`  | ctor   | full form                       |
|  [05]   | `.Status`, `.StatusCode`, `.Trailers`     | props  | read-back of constructed values |

[ENTRYPOINT_SCOPE]: method descriptor and service definition

| [INDEX] | [SURFACE]                                                            | [KIND]  | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `new Marshaller<T>(serializer, deserializer)`                        | ctor    | byte-array codec pair                        |
|  [02]   | `Marshallers.Create<T>(Func<T,byte[]>, Func<byte[],T>)`              | factory | shorthand marshaller construction            |
|  [03]   | `new Method<TReq,TResp>(MethodType, serviceName, name, reqM, respM)` | ctor    | method descriptor                            |
|  [04]   | `ServerServiceDefinition.CreateBuilder()`                            | factory | begins a definition builder                  |
|  [05]   | `Builder.AddMethod(method, handler)` (×4 streaming variants)         | method  | registers a handler by method type           |
|  [06]   | `Builder.Build()`                                                    | method  | produces immutable `ServerServiceDefinition` |
|  [07]   | `ServerServiceDefinition.BindService(ServiceBinderBase)`             | method  | replays registrations into a binder          |

[ENTRYPOINT_SCOPE]: `CallOptions` — client-side configuration

| [INDEX] | [SURFACE]                                                        | [KIND]      | [RAIL]                        |
| :-----: | :--------------------------------------------------------------- | :---------- | :---------------------------- |
|  [01]   | `CallOptions(headers?, deadline?, ct?, ...)`                     | ctor        | full options struct           |
|  [02]   | `WithHeaders(Metadata)` / `WithDeadline(DateTime)`               | method      | immutable update pattern      |
|  [03]   | `WithCancellationToken(CT)` / `WithCredentials(CallCredentials)` | method      | immutable update              |
|  [04]   | `WithWaitForReady(bool)` / `IsWaitForReady`                      | method/prop | `TransientFailure` retry flag |

## [04]-[IMPLEMENTATION_LAW]

[CALL_CONTEXT_DISCIPLINE]:

- `ServerCallContext` is abstract; Grpc.AspNetCore supplies the concrete implementation at server construction time. Do not subclass or mock unless authoring a test seam.
- `CancellationToken` fires on deadline expiry and on network-level call termination, not only on explicit client cancel; treat it as the single call-liveness signal.
- `Status` defaults to `OK` if never set; set it before returning from the handler when an error must propagate with a detail string.
- `WriteResponseHeadersAsync` must be called at most once and before any response message write; missing the call is safe — the first write auto-sends empty headers.
- `UserState` is per-call mutable scratch space; keys must be reference-comparable objects, not strings, to avoid collisions across interceptors.

[METADATA_DISCIPLINE]:

- Keys normalize to lowercase and accept only `[a-z0-9._-]`; binary-valued keys must end in `-bin`.
- Binary and ASCII entries coexist in the same collection; check `Entry.IsBinary` before reading `.Value`.
- `Metadata.Empty` is frozen; mutating it throws `InvalidOperationException`.
- `Get(key)` returns the last matching entry; `GetAll(key)` returns all matching entries in insertion order.

[STATUS_AND_ERROR_DISCIPLINE]:

- `Status` is a value struct; equality is by code and detail string, not reference.
- `RpcException.Message` is local only and never transmitted to the peer; use `Status.Detail` for peer-visible error text.
- `DebugException` on `Status` is populated only on the client side from internal stack state; never use it in business logic.

[METHOD_AND_MARSHAL_DISCIPLINE]:

- `Marshaller<T>` carries both a legacy byte-array codec and a contextual codec (`ContextualSerializer` / `ContextualDeserializer`); prefer the contextual overloads for memory pooling when on Grpc.Net.Client.
- `Method<TReq,TResp>.FullName` drives server-side dispatch; it must match the generated proto service name exactly.
- `ServerServiceDefinition.Builder` rejects duplicate `FullName` registrations at build time with `ArgumentException`.
- Server handler delegate types: `UnaryServerMethod<TReq,TResp>`, `ClientStreamingServerMethod`, `ServerStreamingServerMethod`, `DuplexStreamingServerMethod` — each accepts `ServerCallContext` as the last parameter.
