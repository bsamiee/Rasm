# [RASM_API_GRPC_CORE_API]

`Grpc.Core.Api` owns the host-neutral gRPC call surface both server rails bind: the `Marshaller` codec pairs, `Method<TReq,TResp>` descriptors keyed by `FullName`, and the `ServerServiceDefinition` handler registry on the definition side, and `ServerCallContext`, the stream writers, and `Metadata` on the per-call side. Transport, hosting, and channel construction stay with the managed `Grpc.Net.Client` and `Grpc.AspNetCore` hosts, and the status, fault, and call-policy carriers this package also ships are catalogued once at the client rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Core.Api`
- package: `Grpc.Core.Api` (Apache-2.0)
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- asset: pure-managed runtime library; no native asset, no RID burden
- rail: remote-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: marshallers, method descriptors, and the handler registry

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `Marshaller<T>`           | class         | serializer + deserializer pair per message type |
|  [02]   | `Marshallers`             | class         | marshaller factory and `StringMarshaller`       |
|  [03]   | `Method<TReq,TResp>`      | class         | method descriptor keyed by `FullName`           |
|  [04]   | `MethodType`              | enum          | the four call-shape cases                       |
|  [05]   | `ServerServiceDefinition` | class         | method-to-handler registration builder          |

[PUBLIC_TYPE_SCOPE]: server-call, stream-writer, and metadata contracts

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :----------------------- | :------------ | :---------------------------------- |
|  [01]   | `ServerCallContext`      | class         | per-call server state root          |
|  [02]   | `IServerStreamWriter<T>` | interface     | server-streaming response writer    |
|  [03]   | `IAsyncStreamWriter<T>`  | interface     | base async write contract           |
|  [04]   | `WriteOptions`           | class         | per-write flag carrier              |
|  [05]   | `WriteFlags`             | enum          | `[Flags]` write bits                |
|  [06]   | `Metadata`               | class         | `IList<Metadata.Entry>` header list |
|  [07]   | `Metadata.Entry`         | class         | one ASCII or binary header pair     |

- `WriteFlags`: `BufferHint` (1) `NoCompress` (2)
- Registers the fault and call-policy carriers(`.api/api-grpc-client.md`): `Status`, `StatusCode`, `RpcException`, and `CallOptions` ship in this assembly and carry their construction, roster, read-back, and `With*` threading at the client rail, which both server rails type against; the rows above are the carriers this catalogue adds beyond them.

## [03]-[ENTRYPOINTS]

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

[ENTRYPOINT_SCOPE]: server-call context, stream writers, and metadata

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `ServerCallContext.Method -> string`                                   | property | RPC method name            |
|  [02]   | `ServerCallContext.Host -> string`                                     | property | called host name           |
|  [03]   | `ServerCallContext.Peer -> string`                                     | property | remote endpoint URI        |
|  [04]   | `ServerCallContext.Deadline -> DateTime`                               | property | call deadline              |
|  [05]   | `ServerCallContext.RequestHeaders -> Metadata`                         | property | inbound request headers    |
|  [06]   | `ServerCallContext.ResponseTrailers -> Metadata`                       | property | outbound trailing headers  |
|  [07]   | `ServerCallContext.Status -> Status`                                   | property | status sent at finish      |
|  [08]   | `ServerCallContext.WriteOptions -> WriteOptions?`                      | property | next-write flags           |
|  [09]   | `ServerCallContext.CancellationToken -> CancellationToken`             | property | call cancellation signal   |
|  [10]   | `ServerCallContext.AuthContext -> AuthContext`                         | property | peer auth state            |
|  [11]   | `ServerCallContext.UserState -> IDictionary<object, object>`           | property | interceptor state bag      |
|  [12]   | `ServerCallContext.WriteResponseHeadersAsync(Metadata) -> Task`        | method   | flush leading headers      |
|  [13]   | `ServerCallContext.CreatePropagationToken(ContextPropagationOptions?)` | method   | deadline-propagation token |
|  [14]   | `IAsyncStreamWriter<T>.WriteAsync(T) -> Task`                          | method   | emit one message           |
|  [15]   | `IAsyncStreamWriter<T>.WriteAsync(T, CancellationToken) -> Task`       | method   | cancellable emit           |
|  [16]   | `IAsyncStreamWriter<T>.WriteOptions -> WriteOptions?`                  | property | per-write flags            |
|  [17]   | `WriteOptions(WriteFlags)`                                             | ctor     | flag carrier               |
|  [18]   | `WriteOptions.Default`                                                 | static   | shared no-flag default     |
|  [19]   | `MoveNext<T>(IAsyncStreamReader<T>) -> Task<bool>`                     | static   | manual reader pump         |
|  [20]   | `Metadata()`                                                           | ctor     | empty header list          |
|  [21]   | `Metadata.Add(string, string)`                                         | method   | append ASCII header        |
|  [22]   | `Metadata.Add(string, byte[])`                                         | method   | append binary header       |
|  [23]   | `Metadata.Add(Metadata.Entry)`                                         | method   | append an entry            |
|  [24]   | `Metadata.Get(string) -> Metadata.Entry?`                              | method   | first entry by key         |
|  [25]   | `Metadata.GetValue(string) -> string?`                                 | method   | ASCII value by key         |
|  [26]   | `Metadata.GetValueBytes(string) -> byte[]?`                            | method   | binary value by key        |
|  [27]   | `Metadata.GetAll(string) -> IEnumerable<Metadata.Entry>`               | method   | all entries by key         |
|  [28]   | `Metadata.BinaryHeaderSuffix`                                          | const    | `"-bin"` key marker        |
|  [29]   | `Metadata.Empty`                                                       | static   | shared read-only empty     |
|  [30]   | `Metadata.Entry(string, string)`                                       | ctor     | ASCII header pair          |
|  [31]   | `Metadata.Entry(string, byte[])`                                       | ctor     | binary header pair         |
|  [32]   | `Metadata.Entry.Key -> string`                                         | property | lowercased key             |
|  [33]   | `Metadata.Entry.Value -> string`                                       | property | ASCII value                |
|  [34]   | `Metadata.Entry.ValueBytes -> byte[]`                                  | property | raw value bytes            |
|  [35]   | `Metadata.Entry.IsBinary -> bool`                                      | property | binary-entry flag          |

- `MoveNext<T>`: the `Grpc.Core.AsyncStreamReaderExtensions` pump this assembly ships; the `ReadAllAsync<T>` drain over the same static class name is `Grpc.Net.Common`'s half.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every server method is one `Method<TReq,TResp>` descriptor; its `FullName` (`serviceName/name`) keys dispatch and matches the generated proto service name exactly.
- `ServerServiceDefinition.Builder.AddMethod` registers a handler per method; `Build()` rejects a duplicate `FullName` with `ArgumentException`.
- `Marshaller<T>` carries both a byte-array codec and a contextual codec (`ContextualSerializer` / `ContextualDeserializer`); the contextual pair binds buffer pooling.
- `ServerCallContext` is the per-call state root: `RequestHeaders` reads inbound, `ResponseTrailers` and `Status` write outbound, `WriteResponseHeadersAsync(Metadata)` flushes leading headers before the first message, and `CreatePropagationToken(ContextPropagationOptions?)` mints the token a downstream `CallOptions.WithPropagationToken` binds to inherit the deadline.
- `WriteOptions(WriteFlags)` sets per-write behavior: `BufferHint` (1) defers the network flush to coalesce writes, `NoCompress` (2) sends the message uncompressed regardless of channel encoding.
- `Metadata` is an `IList<Metadata.Entry>`: a binary header carries the `-bin` suffix (`BinaryHeaderSuffix`) and stores raw `ValueBytes`, and `Metadata.Empty` is the shared read-only instance.
- A registered handler folds every failure onto the fault carriers, so `Status.Detail` with trailing `Metadata` is the only peer-visible fault channel this package produces.

[STACKING]:
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): the fault and call-policy half of one rail — a handler's `Status`/`RpcException` mint and the `CallOptions.With*` threading a propagated outbound call takes both read their members there; the server edge packs numeric `FaultDetail` evidence into `google.rpc.Status` details on `ServerCallContext.ResponseTrailers`/`Status`, and the client admits it as opaque `RemoteFault` evidence under its local `WireFault` transport rail.
- `Grpc.AspNetCore.Server`(`.api/api-grpc-aspnetcore.md`): a hosted service method takes `ServerCallContext` and `IServerStreamWriter<T>` from this surface, and a registered interceptor reads `Metadata` off the same call.
- `Grpc.Net.Common`(`Rasm.Compute/.api/api-grpc-common.md`): the compression-provider contracts and the `ConnectivityState` vocabulary are that catalogue's, and `IAsyncStreamReader<T>.ReadAllAsync` there is the client-side drain pairing with `IServerStreamWriter<T>.WriteAsync` here.
- `grpc_csharp_plugin` generated code: the plugin emits `ControlService` from the repo's own `.proto` at compile time, so no installed artifact carries it and the spec-compile gate is its only rail — but every symbol it DERIVES FROM is on this one, so the generated shape is catalogable even where the generated members are not. The derive-from roster: `ClientBase<T>` and `ClientBase.ClientBaseConfiguration` (the client base and its `NewInstance` clone seam), `CallInvoker`, `ChannelBase`, `Method<TRequest,TResponse>` and `MethodType` (each verb's descriptor), `Marshallers.Create<T>` (the per-message codec pair), `ServerServiceDefinition` and `ServiceBinderBase` (the two `BindService` registration forms), `ServerCallContext` (the second parameter of every server override), `AsyncUnaryCall<T>`, `AsyncServerStreamingCall<T>`, and `IServerStreamWriter<T>` (the call-shape returns), `BindServiceMethodAttribute` (the generated container's registration marker), `CallOptions` and `Metadata` (the client overload pair), and `RpcException`/`Status`/`StatusCode` (the one peer-visible fault channel). The shape those members fix, transcribed from the pre-generated `Grpc.HealthCheck` stubs the same plugin emitted: a unary server override is `public virtual Task<TReply> Verb(TRequest request, ServerCallContext context)`, a streaming one takes `IServerStreamWriter<TReply>` ahead of the context and returns bare `Task`; each client verb is a four-member quartet over the two blocking and two async overloads; `__ServiceName` is the proto package-qualified service name every descriptor keys on; `Descriptor` projects the reflection descriptor. A writer needing a generated spelling reads this shape, never the member rail.
- `Rasm.AppHost`: `ControlService` builds its service as one `ServerServiceDefinition` of `Method<TReq,TResp>` descriptors over `Marshallers.Create` codec pairs, then binds `ServerCallContext`, `IServerStreamWriter<T>`, and `Metadata` as the server-side call surface.
- `Rasm.Compute`: `IServerStreamWriter<T>.WriteAsync` on the server pairs with `IAsyncStreamReader<T>.ReadAllAsync` on the client so one streaming contract carries the same `IMessage<T>` payloads end to end, and `WriteOptions.BufferHint` coalesces server writes the client drains as one `IAsyncEnumerable<T>`.

[LOCAL_ADMISSION]:
- Contextual marshallers are the admitted codec form; the byte-array pair enters only where a payload has no pooled writer.
- Every binary metadata key carries the `-bin` suffix contract, never an ad hoc encoding.
- Handlers register through `ServerServiceDefinition` descriptors or the hosted `MapGrpcService<TService>` binder, never a hand-rolled dispatch table keyed off `Method.FullName`.

[RAIL_LAW]:
- Package: `Grpc.Core.Api`
- Owns: the gRPC method descriptor, the service-definition registry, the marshaller pairs a server rail registers, and the per-call context, streaming, and metadata surface a handler binds
- Accept: hand-registered `Method` descriptors, contextual marshallers, server-streaming responses, and call-metadata reads
- Reject: managed transport hosting, client-channel construction, gRPC-Web translation, a parallel error DTO beside `Status`, and the fault and call-policy member surface `.api/api-grpc-client.md` owns
