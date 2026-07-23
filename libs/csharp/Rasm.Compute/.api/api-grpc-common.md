# [RASM_COMPUTE_API_GRPC_COMMON]

`Grpc.Net.Common` owns the shared gRPC compression-provider contracts and the channel-connectivity vocabulary; `Grpc.Core.Api` owns the base server-call surface the AppHost `ControlService` binds — server call context, streaming writers, and call metadata. Compression registers at channel and service composition, never the call site, and both surfaces feed the remote-wire and remote-server rails under the managed `Grpc.Net.Client` and `Grpc.AspNetCore` hosts that own transport and channel construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Common`
- package: `Grpc.Net.Common` (Apache-2.0)
- assembly: `Grpc.Net.Common`
- namespace: `Grpc.Net.Compression`, `Grpc.Core`
- rail: remote-wire

[PACKAGE_SURFACE]: `Grpc.Core.Api`
- package: `Grpc.Core.Api` (Apache-2.0)
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- rail: remote-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Grpc.Net.Common` compression and connectivity contracts

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ICompressionProvider`        | interface     | encoding-name plus stream-pair rule |
|  [02]   | `GzipCompressionProvider`     | class         | gzip provider (`"gzip"`)            |
|  [03]   | `DeflateCompressionProvider`  | class         | deflate provider (`"deflate"`)      |
|  [04]   | `AsyncStreamReaderExtensions` | class         | `ReadAllAsync<T>` reader drain      |
|  [05]   | `ConnectivityState`           | enum          | channel connectivity states         |

- `ConnectivityState`: `Idle` `Connecting` `Ready` `TransientFailure` `Shutdown`

[PUBLIC_TYPE_SCOPE]: `Grpc.Core.Api` server-call, stream-writer, and metadata contracts

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Grpc.Net.Common` compression providers and the stream-reader drain

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `ICompressionProvider.EncodingName -> string`                             | property | grpc-encoding name           |
|  [02]   | `ICompressionProvider.CreateCompressionStream(Stream, CompressionLevel?)` | method   | compressing stream wrapper   |
|  [03]   | `ICompressionProvider.CreateDecompressionStream(Stream)`                  | method   | decompressing stream wrapper |
|  [04]   | `GzipCompressionProvider(CompressionLevel)`                               | ctor     | default gzip level           |
|  [05]   | `DeflateCompressionProvider(CompressionLevel)`                            | ctor     | default deflate level        |
|  [06]   | `ReadAllAsync<T>(IAsyncStreamReader<T>) -> IAsyncEnumerable<T>`           | static   | reader message drain         |

[ENTRYPOINT_SCOPE]: `Grpc.Core.Api` server-call context, stream writers, and metadata

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Compression providers register on `GrpcChannelOptions.CompressionProviders` (client) and `GrpcServiceOptions.CompressionProviders` (server, transitive `Grpc.AspNetCore.Server`), never the call site; each provider's `EncodingName` flows into the `grpc-encoding` and `grpc-accept-encoding` request and response headers.
- `ServerCallContext` is the per-call state root: `RequestHeaders` reads inbound, `ResponseTrailers` and `Status` write outbound, `WriteResponseHeadersAsync(Metadata)` flushes leading headers before the first message, and `CreatePropagationToken(ContextPropagationOptions?)` mints the token a downstream `CallOptions.WithPropagationToken` binds to inherit the deadline.
- `WriteOptions(WriteFlags)` sets per-write behavior: `BufferHint` (1) defers the network flush to coalesce writes, `NoCompress` (2) sends the message uncompressed regardless of channel encoding.
- `Metadata` is an `IList<Metadata.Entry>`: a binary header carries the `-bin` suffix (`BinaryHeaderSuffix`) and stores raw `ValueBytes`, and `Metadata.Empty` is the shared read-only instance.

[STACKING]:
- `Grpc.Net.Client`(`libs/csharp/.api/api-grpc-client.md`): the server edge packs the typed `FaultDetail` into `google.rpc.Status` details on `ServerCallContext.ResponseTrailers`/`Status`, and the client edge unpacks `RpcException.Trailers` onto the same `WireFault` union — one fault vocabulary, both directions.
- `Grpc.AspNetCore.Server`(`libs/csharp/.api/api-grpc-aspnetcore.md`): the same `ICompressionProvider` rows register on the server `GrpcServiceOptions.CompressionProviders` and the client `GrpcChannelOptions.CompressionProviders`; `GrpcServiceOptions.ResponseCompressionAlgorithm` sets the server default, and a per-call `grpc-internal-encoding-request` negotiates against the peer's advertised set.
- AppHost `ControlService`: `IServerStreamWriter<T>.WriteAsync` on the server pairs with `IAsyncStreamReader<T>.ReadAllAsync` on the client so one streaming contract carries the same `IMessage<T>` payloads end to end, and `WriteOptions.BufferHint` coalesces server writes the client drains as one `IAsyncEnumerable<T>`.

[LOCAL_ADMISSION]:
- AppHost `ControlService` binds `ServerCallContext`, `IServerStreamWriter<T>`, and `Metadata` directly as the server-side call surface.
- Compression-provider registration stays explicit at channel (`GrpcChannelOptions.CompressionProviders`) and service (`GrpcServiceOptions.CompressionProviders`) composition.
- A binary metadata key carries the `-bin` suffix contract, never an ad hoc encoding.
- `ConnectivityState` is read-only channel evidence, not a client-driven state machine.

[RAIL_LAW]:
- Package: `Grpc.Net.Common` — owns gRPC compression-provider contracts and the `ConnectivityState` vocabulary
- Package: `Grpc.Core.Api` — owns the base server-call, stream-writer, and metadata surface
- Accept: compression registration, server-streaming responses, and call-metadata reads on the AppHost control rail
- Reject: managed transport hosting, client-channel construction, and gRPC-Web translation, which `Grpc.Net.Client.Web` owns
