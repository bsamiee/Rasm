# [RASM_COMPUTE_API_GRPC_COMMON]

`Grpc.Net.Common` supplies the shared gRPC compression-provider contracts and connectivity vocabulary, and `Grpc.Core.Api` supplies the base gRPC API surface the AppHost `ControlService` consumes: server call context, streaming writers, and call metadata. This catalogue covers both base packages that sit under the `Grpc.Net.Client` and `Grpc.AspNetCore` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Common`
- package: `Grpc.Net.Common`
- assembly: `Grpc.Net.Common`
- namespace: `Grpc.Net.Compression`, `Grpc.Core`
- asset: runtime library
- rail: remote-wire

[PACKAGE_SURFACE]: `Grpc.Core.Api`
- package: `Grpc.Core.Api`
- assembly: `Grpc.Core.Api`
- namespace: `Grpc.Core`
- asset: runtime library
- rail: remote-server

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Grpc.Net.Common` compression and connectivity contracts
- rail: remote-wire

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]       | [CAPABILITY]                                   |
| :-----: | :---------------------------- | :------------------- | :--------------------------------------------- |
|  [01]   | `ICompressionProvider`        | compression contract | declares an encoding stream pair               |
|  [02]   | `GzipCompressionProvider`     | gzip provider        | compresses gRPC payloads as gzip               |
|  [03]   | `DeflateCompressionProvider`  | deflate provider     | compresses gRPC payloads as deflate            |
|  [04]   | `AsyncStreamReaderExtensions` | reader extension     | reads a stream reader as `IAsyncEnumerable<T>` |
|  [05]   | `ConnectivityState`           | channel-state enum   | reports channel connectivity                   |

[PUBLIC_TYPE_SCOPE]: `Grpc.Core.Api` server-call and metadata contracts
- rail: remote-server

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]       | [CAPABILITY]                           |
| :-----: | :----------------------- | :------------------- | :------------------------------------- |
|  [01]   | `ServerCallContext`      | server call context  | exposes per-call server state          |
|  [02]   | `IServerStreamWriter<T>` | server stream writer | writes a server-streaming response     |
|  [03]   | `IAsyncStreamWriter<T>`  | async stream writer  | base write contract for stream writers |
|  [04]   | `Metadata`               | call metadata        | carries request and trailing headers   |
|  [05]   | `Metadata.Entry`         | metadata entry       | one ASCII or binary header pair        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compression-provider members
- rail: remote-wire

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]      | [CAPABILITY]                     |
| :-----: | :----------------------------------------------- | :---------------- | :------------------------------- |
|  [01]   | `ICompressionProvider.EncodingName`              | contract property | reports the `grpc-encoding` name |
|  [02]   | `ICompressionProvider.CreateCompressionStream`   | contract method   | wraps a stream for compression   |
|  [03]   | `ICompressionProvider.CreateDecompressionStream` | contract method   | wraps a stream for decompression |
|  [04]   | `GzipCompressionProvider(CompressionLevel)`      | constructor       | sets the default gzip level      |
|  [05]   | `DeflateCompressionProvider(CompressionLevel)`   | constructor       | sets the default deflate level   |
|  [06]   | `AsyncStreamReaderExtensions.ReadAllAsync<T>`    | extension method  | enumerates the reader's messages |

[ENTRYPOINT_SCOPE]: server-call and metadata members
- rail: remote-server

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]     | [CAPABILITY]                        |
| :-----: | :-------------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `ServerCallContext.RequestHeaders`            | context property | reads inbound request `Metadata`    |
|  [02]   | `ServerCallContext.ResponseTrailers`          | context property | writes outbound trailing `Metadata` |
|  [03]   | `ServerCallContext.CancellationToken`         | context property | observes call cancellation          |
|  [04]   | `ServerCallContext.WriteResponseHeadersAsync` | context call     | flushes response headers            |
|  [05]   | `IServerStreamWriter<T>.WriteAsync`           | stream write     | emits one server-streaming message  |
|  [06]   | `IAsyncStreamWriter<T>.WriteOptions`          | writer property  | sets per-write flags                |
|  [07]   | `Metadata.Add`                                | metadata write   | appends an ASCII or binary header   |
|  [08]   | `Metadata.Get` / `GetValue` / `GetAll`        | metadata read    | reads a header by key               |

[ENTRYPOINT_SCOPE]: `Grpc.Net.Common` decompile-verified members
- source: `Grpc.Net.Common` 2.80.0 decompile
- rail: remote-wire

| [INDEX] | [MEMBER]                                            | [SIGNATURE]                                                                                                                          |
| :-----: | :-------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ICompressionProvider.EncodingName`                 | `string EncodingName { get; }`                                                                                                       |
|  [02]   | `ICompressionProvider.CreateCompressionStream`      | `Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)`                                                  |
|  [03]   | `ICompressionProvider.CreateDecompressionStream`    | `Stream CreateDecompressionStream(Stream stream)`                                                                                    |
|  [04]   | `GzipCompressionProvider.ctor`                      | `GzipCompressionProvider(CompressionLevel defaultCompressionLevel)`                                                                  |
|  [05]   | `GzipCompressionProvider.EncodingName`              | `string EncodingName => "gzip"`                                                                                                      |
|  [06]   | `GzipCompressionProvider.CreateCompressionStream`   | `Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)`                                                  |
|  [07]   | `GzipCompressionProvider.CreateDecompressionStream` | `Stream CreateDecompressionStream(Stream stream)`                                                                                    |
|  [08]   | `DeflateCompressionProvider.ctor`                   | `DeflateCompressionProvider(CompressionLevel defaultCompressionLevel)`                                                               |
|  [09]   | `DeflateCompressionProvider.EncodingName`           | `string EncodingName => "deflate"`                                                                                                   |
|  [10]   | `AsyncStreamReaderExtensions.ReadAllAsync`          | `static IAsyncEnumerable<T> ReadAllAsync<T>(this IAsyncStreamReader<T> streamReader, CancellationToken cancellationToken = default)` |
|  [11]   | `ConnectivityState`                                 | `enum ConnectivityState { Idle, Connecting, Ready, TransientFailure, Shutdown }`                                                     |

[ENTRYPOINT_SCOPE]: `Grpc.Core.Api` decompile-verified members
- source: `Grpc.Core.Api` 2.80.0 decompile
- rail: remote-server#CONTROL_SERVICE

| [INDEX] | [MEMBER]                                      | [SIGNATURE]                                                                                 |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `ServerCallContext.Method`                    | `string Method { get; }`                                                                    |
|  [02]   | `ServerCallContext.Host`                      | `string Host { get; }`                                                                      |
|  [03]   | `ServerCallContext.Peer`                      | `string Peer { get; }`                                                                      |
|  [04]   | `ServerCallContext.Deadline`                  | `DateTime Deadline { get; }`                                                                |
|  [05]   | `ServerCallContext.RequestHeaders`            | `Metadata RequestHeaders { get; }`                                                          |
|  [06]   | `ServerCallContext.CancellationToken`         | `CancellationToken CancellationToken { get; }`                                              |
|  [07]   | `ServerCallContext.ResponseTrailers`          | `Metadata ResponseTrailers { get; }`                                                        |
|  [08]   | `ServerCallContext.Status`                    | `Status Status { get; set; }`                                                               |
|  [09]   | `ServerCallContext.WriteOptions`              | `WriteOptions? WriteOptions { get; set; }`                                                  |
|  [10]   | `ServerCallContext.AuthContext`               | `AuthContext AuthContext { get; }`                                                          |
|  [11]   | `ServerCallContext.UserState`                 | `IDictionary<object, object> UserState { get; }`                                            |
|  [12]   | `ServerCallContext.WriteResponseHeadersAsync` | `Task WriteResponseHeadersAsync(Metadata responseHeaders)`                                  |
|  [13]   | `ServerCallContext.CreatePropagationToken`    | `ContextPropagationToken CreatePropagationToken(ContextPropagationOptions? options = null)` |
|  [14]   | `IAsyncStreamWriter<T>.WriteOptions`          | `WriteOptions? WriteOptions { get; set; }`                                                  |
|  [15]   | `IAsyncStreamWriter<T>.WriteAsync`            | `Task WriteAsync(T message)`                                                                |
|  [16]   | `IAsyncStreamWriter<T>.WriteAsync`            | `Task WriteAsync(T message, CancellationToken cancellationToken)`                           |
|  [17]   | `IServerStreamWriter<T>`                      | `interface IServerStreamWriter<in T> : IAsyncStreamWriter<T>`                               |
|  [18]   | `WriteOptions.ctor`                           | `WriteOptions(WriteFlags flags = 0)`; `static readonly WriteOptions Default`                |
|  [19]   | `WriteFlags`                                  | `[Flags] enum WriteFlags { BufferHint = 1, NoCompress = 2 }`                                |
|  [20]   | `AsyncStreamReaderExtensions.MoveNext`        | `static Task<bool> MoveNext<T>(this IAsyncStreamReader<T> streamReader) where T : class` — manual-pump alternative to `Grpc.Net.Common`'s `ReadAllAsync` |
|  [18]   | `Metadata.ctor`                               | `Metadata()`                                                                                |
|  [19]   | `Metadata.Add`                                | `void Add(string key, string value)`                                                        |
|  [20]   | `Metadata.Add`                                | `void Add(string key, byte[] valueBytes)`                                                   |
|  [21]   | `Metadata.Add`                                | `void Add(Metadata.Entry item)`                                                             |
|  [22]   | `Metadata.Get`                                | `Metadata.Entry? Get(string key)`                                                           |
|  [23]   | `Metadata.GetValue`                           | `string? GetValue(string key)`                                                              |
|  [24]   | `Metadata.GetValueBytes`                      | `byte[]? GetValueBytes(string key)`                                                         |
|  [25]   | `Metadata.GetAll`                             | `IEnumerable<Metadata.Entry> GetAll(string key)`                                            |
|  [26]   | `Metadata.BinaryHeaderSuffix`                 | `const string BinaryHeaderSuffix = "-bin"`                                                  |
|  [27]   | `Metadata.Empty`                              | `static readonly Metadata Empty`                                                            |
|  [28]   | `Metadata.Entry.ctor`                         | `Entry(string key, string value)` / `Entry(string key, byte[] valueBytes)`                  |
|  [29]   | `Metadata.Entry.Key`                          | `string Key { get; }`                                                                       |
|  [30]   | `Metadata.Entry.Value`                        | `string Value { get; }`                                                                     |
|  [31]   | `Metadata.Entry.ValueBytes`                   | `byte[] ValueBytes { get; }`                                                                |
|  [32]   | `Metadata.Entry.IsBinary`                     | `bool IsBinary { get; }`                                                                    |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_PROVIDERS]:
- namespace: `Grpc.Net.Compression`
- contract: `ICompressionProvider` pairs an `EncodingName` with a compression and decompression stream factory
- gzip: `GzipCompressionProvider` reports `"gzip"`; deflate: `DeflateCompressionProvider` reports `"deflate"`
- the encoding name flows into the `grpc-encoding` and `grpc-accept-encoding` request and response headers
- compression providers register on `GrpcChannelOptions.CompressionProviders` (client) and `GrpcServiceOptions.CompressionProviders` (server, transitive `Grpc.AspNetCore.Server`), not on the call site

[SERVER_CALL_SURFACE]:
- namespace: `Grpc.Core`
- `ServerCallContext` exposes inbound `RequestHeaders`, outbound `ResponseTrailers`, the call `Status`, `WriteOptions`, `AuthContext`, `UserState`, and the call `CancellationToken`; `CreatePropagationToken(ContextPropagationOptions?)` mints the token a downstream client call binds via `CallOptions.WithPropagationToken` to inherit the deadline.
- `WriteResponseHeadersAsync(Metadata)` flushes leading response headers before the first message
- `IServerStreamWriter<T>.WriteAsync` emits server-streaming messages; `WriteOptions(WriteFlags)` controls per-write behavior — `WriteFlags.BufferHint` (1) defers the network flush to coalesce writes, `WriteFlags.NoCompress` (2) sends the message uncompressed regardless of the channel encoding
- `Metadata` is an `IList<Metadata.Entry>`; binary headers carry the `-bin` suffix (`Metadata.BinaryHeaderSuffix`) and store raw `ValueBytes`; `Metadata.Empty` is the shared read-only empty instance

[STACK_INTEGRATION]:
- Symmetric fault rail: the server edge packs the typed `FaultDetail` into `google.rpc.Status` details and writes it to `ServerCallContext.ResponseTrailers`/`Status`; the `Grpc.Net.Client` client edge unpacks an `RpcException.Trailers` back onto the same typed `WireFault` union — one fault vocabulary, both directions, no parallel error DTO.
- Shared compression axis: the same `ICompressionProvider` rows (`gzip`/`deflate`) register on BOTH the client `GrpcChannelOptions.CompressionProviders` and the server `GrpcServiceOptions.CompressionProviders` (`IList<ICompressionProvider> { get; }`, owned by the transitive `Grpc.AspNetCore.Server` — see `api-grpc-aspnetcore.md`), and the `EncodingName` flows through `grpc-encoding`/`grpc-accept-encoding` so a per-call `grpc-internal-encoding-request` selection negotiates against the peer's advertised set; `GrpcServiceOptions.ResponseCompressionAlgorithm` sets the server's default response encoding.
- Server-streaming bridge: `IServerStreamWriter<T>.WriteAsync` on the server pairs with `IAsyncStreamReader<T>.ReadAllAsync` (the `Grpc.Net.Common` extension) on the client, so one streaming contract carries the same `IMessage<T>` payloads end to end; `WriteOptions.BufferHint` coalesces server writes that the client drains as one `IAsyncEnumerable<T>`.

[LOCAL_ADMISSION]:
- The AppHost `ControlService` consumes `ServerCallContext`, `IServerStreamWriter<T>`, and `Metadata` directly as the server-side call surface.
- Compression provider registration is remote-wire policy and stays explicit at channel (`GrpcChannelOptions.CompressionProviders`) and server (`GrpcServiceOptions.CompressionProviders`) composition.
- Metadata keys are header vocabulary; binary metadata uses the `-bin` suffix contract, never an ad hoc encoding.
- `ConnectivityState` is read-only channel evidence, not a client-driven state machine.

[RAIL_LAW]:
- Package: `Grpc.Net.Common` — owns gRPC compression-provider contracts and connectivity vocabulary
- Package: `Grpc.Core.Api` — owns the base server-call, stream-writer, and metadata API surface
- Accept: compression registration, server-streaming responses, and call-metadata reads on the AppHost control rail
- Reject: managed transport hosting; client-channel construction; gRPC-Web translation, which `Grpc.Net.Client.Web` owns
