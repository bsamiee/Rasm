# [RASM_COMPUTE_API_GRPC_CLIENT_WEB]

`Grpc.Net.Client.Web` supplies the gRPC-Web HTTP message handler and mode
selector that translate client gRPC calls into `application/grpc-web` or
`application/grpc-web-text` transport for HTTP/1.1 and browser-constrained
paths.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client.Web`
- package: `Grpc.Net.Client.Web`
- assembly: `Grpc.Net.Client.Web`
- namespace: `Grpc.Net.Client.Web`
- asset: runtime library
- rail: remote-client

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport translation contracts
- rail: remote-client

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]  | [CAPABILITY]                |
| :-----: | :--------------- | :-------------- | :-------------------------- |
|   [1]   | `GrpcWebHandler` | message handler | translates gRPC to gRPC-Web |
|   [2]   | `GrpcWebMode`    | mode selector   | selects wire content type   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler construction
- rail: remote-client

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]     | [CAPABILITY]                  |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------- |
|   [1]   | `GrpcWebHandler()`                                | constructor      | creates default handler       |
|   [2]   | `GrpcWebHandler(HttpMessageHandler)`              | constructor      | wraps inner handler           |
|   [3]   | `GrpcWebHandler(GrpcWebMode)`                     | constructor      | creates handler with mode     |
|   [4]   | `GrpcWebHandler(GrpcWebMode, HttpMessageHandler)` | constructor      | wraps inner handler with mode |
|   [5]   | `GrpcWebMode`                                     | handler property | sets wire mode                |
|   [6]   | `HttpVersion`                                     | handler property | obsolete version override     |

[ENTRYPOINT_SCOPE]: mode cases
- rail: remote-client

| [INDEX] | [SURFACE]                 | [CALL_SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------ | :----------- | :----------------------------- |
|   [1]   | `GrpcWebMode.GrpcWeb`     | enum case    | sends binary gRPC-Web payloads |
|   [2]   | `GrpcWebMode.GrpcWebText` | enum case    | sends base64 gRPC-Web payloads |

[ENTRYPOINT_SCOPE]: transport pipeline
- rail: remote-client

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]      | [CAPABILITY]                           |
| :-----: | :------------------------- | :---------------- | :------------------------------------- |
|   [1]   | `GrpcWebHandler.SendAsync` | pipeline override | intercepts `application/grpc` requests |

[ENTRYPOINT_SCOPE]: `GrpcWebHandler` and `GrpcWebMode` decompile-verified members
- source: `Grpc.Net.Client.Web` 2.80.0 decompile
- rail: remote-client#CALL_SPINE

| [INDEX] | [MEMBER]                  | [SIGNATURE]                                                                | [USED_BY]              | [EVIDENCE]       |
| :-----: | :------------------------ | :------------------------------------------------------------------------- | :--------------------- | :--------------- |
|   [1]   | `GrpcWebHandler.ctor`     | `GrpcWebHandler()`                                                         | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [2]   | `GrpcWebHandler.ctor`     | `GrpcWebHandler(HttpMessageHandler innerHandler)`                          | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [3]   | `GrpcWebHandler.ctor`     | `GrpcWebHandler(GrpcWebMode mode)`                                         | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [4]   | `GrpcWebHandler.ctor`     | `GrpcWebHandler(GrpcWebMode mode, HttpMessageHandler innerHandler)`        | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [5]   | `GrpcWebMode.GrpcWeb`     | `GrpcWeb = 0` — `application/grpc-web` binary wire format                  | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [6]   | `GrpcWebMode.GrpcWebText` | `GrpcWebText = 1` — `application/grpc-web-text` base64 wire format         | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [7]   | `GrpcWebHandler.GrpcWebMode` | `GrpcWebMode GrpcWebMode { get; set; }` — mode property on the handler  | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [8]   | `GrpcWebHandler.HttpVersion` | `[Obsolete] Version? HttpVersion { get; set; }` — use `GrpcChannelOptions.HttpVersion` instead | remote-lane#CALL_SPINE | decompile 2.80.0 |
|   [9]   | `GrpcWebHandler.SendAsync`   | `protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)` | remote-lane#CALL_SPINE | decompile 2.80.0 |

## [4]-[IMPLEMENTATION_LAW]

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

[LOCAL_ADMISSION]:
- The handler enters the channel through `GrpcChannelOptions.HttpHandler` only.
- Mode selection is remote execution policy and stays explicit at channel composition.
- Internal content, stream, and protocol types are not extension surfaces.

[RAIL_LAW]:
- Package: `Grpc.Net.Client.Web`
- Owns: gRPC-Web transport translation for client channels
- Accept: unary and server-streaming remote execution calls over HTTP/1.1-constrained paths
- Reject: client-streaming, duplex, and server hosting surface
