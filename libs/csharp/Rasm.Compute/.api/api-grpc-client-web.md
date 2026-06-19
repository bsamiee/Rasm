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

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]  | [CAPABILITY]                |
| :-----: | :--------------- | :-------------- | :-------------------------- |
|  [01]   | `GrpcWebHandler` | message handler | translates gRPC to gRPC-Web |
|  [02]   | `GrpcWebMode`    | mode selector   | selects wire content type   |

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
- source: `Grpc.Net.Client.Web` 2.80.0 decompile
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]                     | [SIGNATURE]                                                                                                               |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `GrpcWebHandler.ctor`        | `GrpcWebHandler()`                                                                                                        |
|  [02]   | `GrpcWebHandler.ctor`        | `GrpcWebHandler(HttpMessageHandler innerHandler)`                                                                         |
|  [03]   | `GrpcWebHandler.ctor`        | `GrpcWebHandler(GrpcWebMode mode)`                                                                                        |
|  [04]   | `GrpcWebHandler.ctor`        | `GrpcWebHandler(GrpcWebMode mode, HttpMessageHandler innerHandler)`                                                       |
|  [05]   | `GrpcWebMode.GrpcWeb`        | `GrpcWeb = 0` — `application/grpc-web` binary wire format                                                                 |
|  [06]   | `GrpcWebMode.GrpcWebText`    | `GrpcWebText = 1` — `application/grpc-web-text` base64 wire format                                                        |
|  [07]   | `GrpcWebHandler.GrpcWebMode` | `GrpcWebMode GrpcWebMode { get; set; }` — mode property on the handler                                                    |
|  [08]   | `GrpcWebHandler.HttpVersion` | `[Obsolete] Version? HttpVersion { get; set; }` — use `GrpcChannelOptions.HttpVersion` instead                            |
|  [09]   | `GrpcWebHandler.SendAsync`   | `protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)` |

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

[LOCAL_ADMISSION]:
- The handler enters the channel through `GrpcChannelOptions.HttpHandler` only.
- Mode selection is remote execution policy and stays explicit at channel composition.
- Internal content, stream, and protocol types are not extension surfaces.

[RAIL_LAW]:
- Package: `Grpc.Net.Client.Web`
- Owns: gRPC-Web transport translation for client channels
- Accept: unary and server-streaming remote execution calls over HTTP/1.1-constrained paths
- Reject: client-streaming, duplex, and server hosting surface
