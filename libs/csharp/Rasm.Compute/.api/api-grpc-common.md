# [RASM_COMPUTE_API_GRPC_COMMON]

`Grpc.Net.Common` owns the shared gRPC compression-provider contracts, the channel-connectivity vocabulary, and the client-side stream-reader drain. Compression registers at channel and service composition, never the call site, and the surface feeds the remote-wire rail under the managed `Grpc.Net.Client` and `Grpc.AspNetCore.Server` hosts that own transport and channel construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Common`
- package: `Grpc.Net.Common` (Apache-2.0)
- assembly: `Grpc.Net.Common`
- namespace: `Grpc.Net.Compression`, `Grpc.Core`
- asset: pure-managed library; no native asset, no RID burden
- rail: remote-wire

- Registers the call surface(`libs/csharp/.api/api-grpc-core-api.md`): `ServerCallContext`, the stream writers, `WriteOptions`, `Metadata`, and the marshaller and service-definition rows are `Grpc.Core.Api`'s and resolve at the branch catalogue; this folder holds the compression and connectivity half alone.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compression and connectivity contracts

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ICompressionProvider`        | interface     | encoding-name plus stream-pair rule |
|  [02]   | `GzipCompressionProvider`     | class         | gzip provider (`"gzip"`)            |
|  [03]   | `DeflateCompressionProvider`  | class         | deflate provider (`"deflate"`)      |
|  [04]   | `AsyncStreamReaderExtensions` | class         | `ReadAllAsync<T>` reader drain      |
|  [05]   | `ConnectivityState`           | enum          | channel connectivity states         |

- `ConnectivityState`: `Idle` `Connecting` `Ready` `TransientFailure` `Shutdown`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compression providers and the stream-reader drain

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `ICompressionProvider.EncodingName -> string`                             | property | grpc-encoding name           |
|  [02]   | `ICompressionProvider.CreateCompressionStream(Stream, CompressionLevel?)` | method   | compressing stream wrapper   |
|  [03]   | `ICompressionProvider.CreateDecompressionStream(Stream)`                  | method   | decompressing stream wrapper |
|  [04]   | `GzipCompressionProvider(CompressionLevel)`                               | ctor     | default gzip level           |
|  [05]   | `DeflateCompressionProvider(CompressionLevel)`                            | ctor     | default deflate level        |
|  [06]   | `ReadAllAsync<T>(IAsyncStreamReader<T>) -> IAsyncEnumerable<T>`           | static   | reader message drain         |

- `ReadAllAsync<T>`: extends the `Grpc.Core.AsyncStreamReaderExtensions` static class name `Grpc.Core.Api` also contributes to; the `MoveNext<T>` pump over the same name is that assembly's half.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Compression providers register on `GrpcChannelOptions.CompressionProviders` (client) and `GrpcServiceOptions.CompressionProviders` (server, transitive `Grpc.AspNetCore.Server`), never the call site; each provider's `EncodingName` flows into the `grpc-encoding` and `grpc-accept-encoding` request and response headers.
- `ConnectivityState` is the vocabulary `GrpcChannel.State` reports and `WaitForStateChangedAsync` parks on, so a connectivity fold reads it and never drives it.

[STACKING]:
- `Grpc.Net.Client`(`libs/csharp/.api/api-grpc-client.md`): `ICompressionProvider` rows register on `GrpcChannelOptions.CompressionProviders`, the per-call `grpc-internal-encoding-request` metadata key selects one by `EncodingName`, and `ReadAllAsync<T>` drains a server-streaming response into one `IAsyncEnumerable<T>`.
- `Grpc.AspNetCore.Server`(`libs/csharp/.api/api-grpc-aspnetcore.md`): the same `ICompressionProvider` rows register on the server `GrpcServiceOptions.CompressionProviders`; `GrpcServiceOptions.ResponseCompressionAlgorithm` sets the server default, and a per-call `grpc-internal-encoding-request` negotiates against the peer's advertised set.
- `Grpc.Core.Api`(`libs/csharp/.api/api-grpc-core-api.md`): `IServerStreamWriter<T>.WriteAsync` on the server pairs with the `ReadAllAsync<T>` drain here so one streaming contract carries the same `IMessage<T>` payloads end to end, and `WriteOptions.BufferHint` coalesces the server writes this drain reads as one sequence.
- `Runtime/channels#TRANSPORT_AXIS`: `WireChannels` reads `GrpcChannel.State` and parks on `WaitForStateChangedAsync`, folding each prior→observed `ConnectivityState` pairing into a typed `WireTransition` the receipt carries; the pump terminates on that transition's own `Absorbing` column, and a `ConnectivityState` the fold does not name lands `Unknown` carrying the observed value rather than re-labelling itself `Idle`.

[LOCAL_ADMISSION]:
- Compression-provider registration stays explicit at channel (`GrpcChannelOptions.CompressionProviders`) and service (`GrpcServiceOptions.CompressionProviders`) composition; a per-call provider mint is the deleted form.
- `ConnectivityState` is read-only channel evidence, not a client-driven state machine.

[RAIL_LAW]:
- Package: `Grpc.Net.Common`
- Owns: the gRPC compression-provider contracts, the `ConnectivityState` vocabulary, and the `ReadAllAsync<T>` stream-reader drain
- Accept: compression registration at channel and service composition, and connectivity reads folded to a typed transition
- Reject: managed transport hosting, client-channel construction, gRPC-Web translation, and a second compression list beside the registered one
