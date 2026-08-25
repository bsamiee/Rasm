# [RASM_API_GRPC_CORE_API]

`Grpc.Core.Api` owns the host-neutral gRPC call surface both server rails bind: the `Marshaller` codec pairs, `Method<TReq,TResp>` descriptors keyed by `FullName`, and the `ServerServiceDefinition` handler registry on the definition side, and `ServerCallContext`, the stream writers, and `Metadata` on the per-call side. Transport, hosting, and channel construction stay with the managed `Grpc.Net.Client` and `Grpc.AspNetCore.Server` hosts, and the status, fault, and call-policy carriers this package also ships are catalogued once at the client rail.

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

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `ServerCallContext`                    | class         | per-call server state root                |
|  [02]   | `IServerStreamWriter<T>`               | interface     | server-streaming response writer          |
|  [03]   | `IAsyncStreamWriter<T>`                | interface     | base async write contract                 |
|  [04]   | `WriteOptions`                         | class         | per-write flag carrier                    |
|  [05]   | `WriteFlags`                           | enum          | `[Flags]` write bits                      |
|  [06]   | `Metadata`                             | class         | `IList<Metadata.Entry>` header list       |
|  [07]   | `Metadata.Entry`                       | class         | one ASCII or binary header pair           |
|  [08]   | `IAsyncStreamReader<T>`                | interface     | pull-side stream read contract            |
|  [09]   | `IClientStreamWriter<T>`               | interface     | client request-stream writer              |
|  [10]   | `AsyncDuplexStreamingCall<TReq,TResp>` | class         | bidi call: both streams, status, trailers |

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

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `ServerCallContext.Method -> string`                                   | property | RPC method name                               |
|  [02]   | `ServerCallContext.Host -> string`                                     | property | called host name                              |
|  [03]   | `ServerCallContext.Peer -> string`                                     | property | remote endpoint URI                           |
|  [04]   | `ServerCallContext.Deadline -> DateTime`                               | property | call deadline                                 |
|  [05]   | `ServerCallContext.RequestHeaders -> Metadata`                         | property | inbound request headers                       |
|  [06]   | `ServerCallContext.ResponseTrailers -> Metadata`                       | property | outbound trailing headers                     |
|  [07]   | `ServerCallContext.Status -> Status`                                   | property | status sent at finish                         |
|  [08]   | `ServerCallContext.WriteOptions -> WriteOptions?`                      | property | next-write flags                              |
|  [09]   | `ServerCallContext.CancellationToken -> CancellationToken`             | property | call cancellation signal                      |
|  [10]   | `ServerCallContext.AuthContext -> AuthContext`                         | property | peer auth state                               |
|  [11]   | `ServerCallContext.UserState -> IDictionary<object, object>`           | property | interceptor state bag                         |
|  [12]   | `ServerCallContext.WriteResponseHeadersAsync(Metadata) -> Task`        | method   | flush leading headers                         |
|  [13]   | `ServerCallContext.CreatePropagationToken(ContextPropagationOptions?)` | method   | deadline-propagation token                    |
|  [14]   | `IAsyncStreamWriter<T>.WriteAsync(T) -> Task`                          | method   | emit one message                              |
|  [15]   | `IAsyncStreamWriter<T>.WriteAsync(T, CancellationToken) -> Task`       | method   | cancellable emit                              |
|  [16]   | `IAsyncStreamWriter<T>.WriteOptions -> WriteOptions?`                  | property | per-write flags                               |
|  [17]   | `WriteOptions(WriteFlags)`                                             | ctor     | flag carrier                                  |
|  [18]   | `WriteOptions.Default`                                                 | static   | shared no-flag default                        |
|  [19]   | `MoveNext<T>(IAsyncStreamReader<T>) -> Task<bool>`                     | static   | manual reader pump                            |
|  [20]   | `IAsyncStreamReader<T>.Current -> T`                                   | property | the message the last `MoveNext` read          |
|  [21]   | `IAsyncStreamReader<T>.MoveNext(CancellationToken) -> Task<bool>`      | method   | advance; false at end of stream               |
|  [22]   | `IClientStreamWriter<T>.CompleteAsync() -> Task`                       | method   | half-close the request stream                 |
|  [23]   | `AsyncDuplexStreamingCall<TReq,TResp>.RequestStream`                   | property | the `IClientStreamWriter<TReq>`               |
|  [24]   | `AsyncDuplexStreamingCall<TReq,TResp>.ResponseStream`                  | property | the `IAsyncStreamReader<TResp>`               |
|  [25]   | `AsyncDuplexStreamingCall<TReq,TResp>.ResponseHeadersAsync`            | property | `Task<Metadata>` leading headers              |
|  [26]   | `AsyncDuplexStreamingCall<TReq,TResp>.GetStatus()` / `GetTrailers()`   | method   | terminal status and trailers after completion |
|  [27]   | `AsyncDuplexStreamingCall<TReq,TResp>.Dispose()`                       | method   | cancel an undrained call                      |
|  [28]   | `Metadata()`                                                           | ctor     | empty header list                             |
|  [29]   | `Metadata.Add(string, string)`                                         | method   | append ASCII header                           |
|  [30]   | `Metadata.Add(string, byte[])`                                         | method   | append binary header                          |
|  [31]   | `Metadata.Add(Metadata.Entry)`                                         | method   | append an entry                               |
|  [32]   | `Metadata.Get(string) -> Metadata.Entry?`                              | method   | first entry by key                            |
|  [33]   | `Metadata.GetValue(string) -> string?`                                 | method   | ASCII value by key                            |
|  [34]   | `Metadata.GetValueBytes(string) -> byte[]?`                            | method   | binary value by key                           |
|  [35]   | `Metadata.GetAll(string) -> IEnumerable<Metadata.Entry>`               | method   | all entries by key                            |
|  [36]   | `Metadata.BinaryHeaderSuffix`                                          | const    | `"-bin"` key marker                           |
|  [37]   | `Metadata.Empty`                                                       | static   | shared read-only empty                        |
|  [38]   | `Metadata.Entry(string, string)`                                       | ctor     | ASCII header pair                             |
|  [39]   | `Metadata.Entry(string, byte[])`                                       | ctor     | binary header pair                            |
|  [40]   | `Metadata.Entry.Key -> string`                                         | property | lowercased key                                |
|  [41]   | `Metadata.Entry.Value -> string`                                       | property | ASCII value                                   |
|  [42]   | `Metadata.Entry.ValueBytes -> byte[]`                                  | property | raw value bytes                               |
|  [43]   | `Metadata.Entry.IsBinary -> bool`                                      | property | binary-entry flag                             |

- `MoveNext<T>`: `Grpc.Core.AsyncStreamReaderExtensions` ships this pump here; the `ReadAllAsync<T>` drain over the same static class name is `Grpc.Net.Common`'s half.
- `AsyncDuplexStreamingCall`: `GetStatus()`/`GetTrailers()` throw before the response stream completes; a bidi driver writes every request, calls `CompleteAsync()`, drains `ResponseStream.ReadAllAsync(token)`, then reads status — and `Dispose()` on an undrained call is the cancellation idiom.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every server method is one `Method<TReq,TResp>` descriptor; its `FullName` (`serviceName/name`) keys dispatch and matches the generated proto service name exactly.
- `ServerServiceDefinition.Builder.AddMethod` registers a handler per method; `Build()` rejects a duplicate `FullName` with `ArgumentException`.
- `Marshaller<T>` carries both a byte-array codec and a contextual codec (`ContextualSerializer` / `ContextualDeserializer`); the contextual pair binds buffer pooling.
- `ServerCallContext` is the per-call state root: `RequestHeaders` reads inbound, `ResponseTrailers` and `Status` write outbound, `WriteResponseHeadersAsync(Metadata)` flushes leading headers before the first message, and `CreatePropagationToken(ContextPropagationOptions?)` mints the token a downstream `CallOptions.WithPropagationToken` binds to inherit the deadline.
- `WriteOptions(WriteFlags)` sets per-write behavior: `BufferHint` (1) defers the network flush to coalesce writes, `NoCompress` (2) sends the message uncompressed regardless of channel encoding.
- `Metadata` is an `IList<Metadata.Entry>`: a binary header carries the `-bin` suffix (`BinaryHeaderSuffix`) and stores raw `ValueBytes`, and `Metadata.Empty` is the shared read-only instance.
- Registered handlers fold every failure onto the fault carriers, so `Status.Detail` with trailing `Metadata` is the only peer-visible fault channel this package produces.

[STACKING]:
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): the fault and call-policy half of one rail — a handler's `Status`/`RpcException` mint and the `CallOptions.With*` threading a propagated outbound call takes both read their members there.
- `Grpc.StatusProto`(`.api/api-grpc-statusproto.md`): the producer edge raises `FaultWire.Raise` — `google.rpc.Status{Code, Message, Details = {Any.Pack(FaultDetail)}}.ToRpcException()` at `Rasm.AppHost/Runtime/ports#WIRE_LAW` — from every failing handler arm, and the client admits `RpcException.GetRpcStatus()` as opaque `RemoteFault` evidence under `Rasm.Compute`'s `WireFault` transport rail; `ServerCallContext.ResponseTrailers` is never written by hand.
- `Grpc.AspNetCore.Server`(`.api/api-grpc-aspnetcore.md`): a hosted service method takes `ServerCallContext` and `IServerStreamWriter<T>` from this surface, and a registered interceptor reads `Metadata` off the same call.
- `Grpc.Net.Common`(`Rasm.Compute/.api/api-grpc-common.md`): the compression-provider contracts and the `ConnectivityState` vocabulary are that catalogue's, and `IAsyncStreamReader<T>.ReadAllAsync` there is the client-side drain pairing with `IServerStreamWriter<T>.WriteAsync` here.
- `Rasm.AppHost`: `ControlServiceImpl` derives the generated `ControlService.ControlServiceBase`, reads `ServerCallContext` per surviving verb, and leaves every failure through `FaultWire.Raise`.
- `Rasm.Compute`: generated `artifact.ArtifactService.Fetch` takes `FetchRequest{sha256}`, pairs server `IServerStreamWriter<FetchResponse>.WriteAsync` with client `IAsyncStreamReader<FetchResponse>.ReadAllAsync`, and unwraps each required `frame`. `Put` pairs server `IAsyncStreamReader<PutRequest>` with client `AsyncClientStreamingCall<PutRequest, PutResponse>`, wrapping every shared frame and admitting the required response artifact. `WriteOptions.BufferHint` may coalesce server-stream writes; `CompleteAsync()` closes the Put request stream before its unary response is read.

[LOCAL_ADMISSION]:
- Contextual marshallers are the admitted codec form; the byte-array pair enters only where a payload has no pooled writer.
- Every binary metadata key carries the `-bin` suffix contract, never an ad hoc encoding.
- Handlers register through `ServerServiceDefinition` descriptors or the hosted `MapGrpcService<TService>` binder, never a hand-rolled dispatch table keyed off `Method.FullName`.

[RAIL_LAW]:
- Package: `Grpc.Core.Api`
- Owns: the gRPC method descriptor, the service-definition registry, the marshaller pairs a server rail registers, and the per-call context, streaming, and metadata surface a handler binds
- Accept: hand-registered `Method` descriptors, contextual marshallers, server-streaming responses, and call-metadata reads
- Reject: managed transport hosting, client-channel construction, gRPC-Web translation, a parallel error DTO beside `Status`, and the fault and call-policy member surface `.api/api-grpc-client.md` owns
