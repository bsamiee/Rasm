# [RASM_COMPUTE_API_GRPC_CLIENT_WEB]

`Grpc.Net.Client.Web` supplies the gRPC-Web HTTP message handler and mode
selector that translate client gRPC calls into `application/grpc-web` or
`application/grpc-web-text` transport for HTTP/1.1 and browser-constrained
paths.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client.Web`
- package: `Grpc.Net.Client.Web`
- assembly: `Grpc.Net.Client.Web`
- namespace: `Grpc.Net.Client.Web`
- asset: runtime library
- rail: remote-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport translation contracts
- rail: remote-client

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]               | [CAPABILITY]                |
| :-----: | :--------------- | :--------------------------- | :-------------------------- |
|  [01]   | `GrpcWebHandler` | `DelegatingHandler` subclass | translates gRPC to gRPC-Web |
|  [02]   | `GrpcWebMode`    | mode selector                | selects wire content type   |

`GrpcWebHandler` is a `DelegatingHandler`: it MUST wrap an inner `HttpMessageHandler` (the channel transport), so the gRPC-Web translation composes *on top of* a `SocketsHttpHandler` rather than replacing it — this is the stacking seam with `Grpc.Net.Client`'s `GrpcChannelOptions.HttpHandler`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler construction
- rail: remote-client

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]     | [CAPABILITY]                  |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------- |
|  [01]   | `GrpcWebHandler()`                                | constructor      | creates default handler       |
|  [02]   | `GrpcWebHandler(HttpMessageHandler)`              | constructor      | wraps inner handler           |
|  [03]   | `GrpcWebHandler(GrpcWebMode)`                     | constructor      | creates handler with mode     |
|  [04]   | `GrpcWebHandler(GrpcWebMode, HttpMessageHandler)` | constructor      | wraps inner handler with mode |
|  [05]   | `GrpcWebMode`                                     | handler property | sets wire mode                |
|  [06]   | `HttpVersion`                                     | handler property | obsolete version override     |

[ENTRYPOINT_SCOPE]: mode cases
- rail: remote-client

| [INDEX] | [SURFACE]                 | [CALL_SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------ | :----------- | :----------------------------- |
|  [01]   | `GrpcWebMode.GrpcWeb`     | enum case    | sends binary gRPC-Web payloads |
|  [02]   | `GrpcWebMode.GrpcWebText` | enum case    | sends base64 gRPC-Web payloads |

[ENTRYPOINT_SCOPE]: transport pipeline
- rail: remote-client

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]      | [CAPABILITY]                           |
| :-----: | :------------------------- | :---------------- | :------------------------------------- |
|  [01]   | `GrpcWebHandler.SendAsync` | pipeline override | intercepts `application/grpc` requests |

[ENTRYPOINT_SCOPE]: `GrpcWebHandler` and `GrpcWebMode` decompile-verified members
- source: `Grpc.Net.Client.Web` decompile
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`
- members [01]-[04],[07]-[09] hang off `GrpcWebHandler`; [05]-[06] are `GrpcWebMode` enum values; `SendAsync` is the `protected override` `DelegatingHandler` pipeline override

| [INDEX] | [MEMBER]      | [SIGNATURE]                                                                                            |
| :-----: | :------------ | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `ctor`        | `GrpcWebHandler()`                                                                                     |
|  [02]   | `ctor`        | `GrpcWebHandler(HttpMessageHandler innerHandler)`                                                      |
|  [03]   | `ctor`        | `GrpcWebHandler(GrpcWebMode mode)`                                                                     |
|  [04]   | `ctor`        | `GrpcWebHandler(GrpcWebMode mode, HttpMessageHandler innerHandler)`                                    |
|  [05]   | `GrpcWeb`     | `GrpcWeb = 0` — `application/grpc-web` binary wire format                                              |
|  [06]   | `GrpcWebText` | `GrpcWebText = 1` — `application/grpc-web-text` base64 wire format                                     |
|  [07]   | `GrpcWebMode` | `GrpcWebMode GrpcWebMode { get; set; }` — mode property on the handler                                 |
|  [08]   | `HttpVersion` | `[Obsolete] Version? HttpVersion { get; set; }` — use `GrpcChannelOptions.HttpVersion` instead         |
|  [09]   | `SendAsync`   | `Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)` |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TRANSLATION]:
- namespace: `Grpc.Net.Client.Web`
- interception: `SendAsync` rewrites only requests carrying the `application/grpc` content type; all other traffic passes through the inner handler untouched
- request: content is wrapped per mode; `GrpcWebText` adds the `application/grpc-web-text` accept header
- response: matching gRPC-Web content is unwrapped with trailing headers restored and the response version normalized to HTTP/2
- browser: under WebAssembly the handler moves `User-Agent` to `X-User-Agent` and enables streaming responses

[MODE_POLICY]:
- `GrpcWeb` carries binary gRPC messages over `application/grpc-web`
- `GrpcWebText` carries base64 gRPC messages over `application/grpc-web-text`
- base64 mode is required for server-streaming calls to stream correctly in browser apps

[CALL_SHAPE_POSTURE]:
- gRPC-Web transport carries unary and server-streaming calls
- client-streaming and duplex calls stay on the HTTP/2 `Grpc.Net.Client` channel rail
- HTTP version policy lives on `GrpcChannelOptions`; the handler `HttpVersion` override is obsolete

[STACK_INTEGRATION]:
- Single rail: the gRPC-Web row composes one handler chain — `GrpcWebHandler(GrpcWebMode.GrpcWeb, inner)` wrapping the BCL `SocketsHttpHandler` whose keepalive/pooling members (`KeepAlivePingDelay`, `EnableMultipleHttp2Connections`, `PooledConnectionIdleTimeout`) are threaded from the same `Grpc.Net.Client` channel-policy owner — and the chain enters `Grpc.Net.Client` via `GrpcChannelOptions.HttpHandler`. The handler never owns transport tuning; it only rewrites the wire frame.
- `GrpcWebMode.GrpcWeb` (binary) is the admitted browser/HTTP-1.1 row; `GrpcWebText` (base64) is the google-client-only spelling the binary mode supersedes on the .NET client.
- HTTP-version posture lives on `GrpcChannelOptions.HttpVersion`/`HttpVersionPolicy` (BCL `System.Net.Http`), never on the obsolete handler `HttpVersion` override.
- The translated calls still classify their `RpcException` through the one `Grpc.Core.Api` status fold the HTTP/2 rail uses — gRPC-Web changes the frame, not the typed-fault rail.

[LOCAL_ADMISSION]:
- The handler enters the channel through `GrpcChannelOptions.HttpHandler` only.
- Mode selection is remote execution policy and stays explicit at channel composition.
- Internal content, stream, and protocol types are not extension surfaces.

[RAIL_LAW]:
- Package: `Grpc.Net.Client.Web`
- Owns: gRPC-Web transport translation for client channels
- Accept: unary and server-streaming remote execution calls over HTTP/1.1-constrained paths
- Reject: client-streaming, duplex, and server hosting surface
