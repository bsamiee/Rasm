# [RASM_COMPUTE_API_GRPC_CLIENT_WEB]

`Grpc.Net.Client.Web` translates client gRPC calls into `application/grpc-web` or `application/grpc-web-text` transport for HTTP/1.1 and browser-constrained paths. `GrpcWebHandler` wraps the channel handler chain as a `DelegatingHandler`, carrying unary and server-streaming calls across a gRPC-Web frame while client-streaming, duplex, and the typed-fault rail stay on the HTTP/2 `Grpc.Net.Client` channel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client.Web`
- package: `Grpc.Net.Client.Web`
- assembly: `Grpc.Net.Client.Web`
- namespace: `Grpc.Net.Client.Web`
- asset: runtime library
- rail: remote-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport translation contracts

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------- | :------------ | :---------------------------- |
|  [01]   | `GrpcWebHandler` | class         | translates gRPC to gRPC-Web   |
|  [02]   | `GrpcWebMode`    | enum          | selects the wire content type |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: handler construction and the pipeline override

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `GrpcWebHandler()`                                                | ctor     | default handler                        |
|  [02]   | `GrpcWebHandler(HttpMessageHandler)`                              | ctor     | wraps an inner handler                 |
|  [03]   | `GrpcWebHandler(GrpcWebMode)`                                     | ctor     | handler bound to a mode                |
|  [04]   | `GrpcWebHandler(GrpcWebMode, HttpMessageHandler)`                 | ctor     | wraps an inner handler with a mode     |
|  [05]   | `GrpcWebHandler.GrpcWebMode`                                      | property | reads or sets the wire mode            |
|  [06]   | `GrpcWebHandler.SendAsync(HttpRequestMessage, CancellationToken)` | override | intercepts `application/grpc` requests |

[ENTRYPOINT_SCOPE]: mode selection

| [INDEX] | [SURFACE]                 | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :------------------------ | :-------- | :--------------------------------- |
|  [01]   | `GrpcWebMode.GrpcWeb`     | enum case | binary `application/grpc-web`      |
|  [02]   | `GrpcWebMode.GrpcWebText` | enum case | base64 `application/grpc-web-text` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GrpcWebHandler.SendAsync` rewrites only requests carrying the `application/grpc` content type; all other traffic passes to the inner handler untouched.
- Request content wraps per `GrpcWebMode`; `GrpcWebText` adds the `application/grpc-web-text` accept header, and a matching gRPC-Web response unwraps with trailing headers restored and its version normalized to HTTP/2.
- Browser server-streaming calls require the `GrpcWebText` base64 mode to stream correctly.
- Under WebAssembly the handler renames `User-Agent` to `X-User-Agent` and flags the request for streaming responses.

[STACKING]:
- `Grpc.Core.Api`(`api-grpc-common.md`): a translated call classifies its `RpcException` through the same status fold the HTTP/2 rail uses, so gRPC-Web rewrites the frame, never the typed-fault vocabulary.
- remote-client folder: `GrpcWebHandler(GrpcWebMode.GrpcWeb, inner)` wraps the BCL `SocketsHttpHandler` and enters `Grpc.Net.Client` through `GrpcChannelOptions.HttpHandler`; the channel-policy owner threads the `SocketsHttpHandler` keepalive and pooling members (`KeepAlivePingDelay`, `EnableMultipleHttp2Connections`, `PooledConnectionIdleTimeout`) and carries HTTP-version posture on `GrpcChannelOptions.HttpVersion`/`HttpVersionPolicy`, leaving the handler to rewrite the wire frame alone.

[LOCAL_ADMISSION]:
- `GrpcChannelOptions.HttpHandler` is the handler's sole entry into the channel.
- Mode selection is remote-execution policy and stays explicit at channel composition.
- Internal content, stream, and protocol types are not extension surfaces.

[RAIL_LAW]:
- Package: `Grpc.Net.Client.Web`
- Owns: gRPC-Web transport translation for client channels
- Accept: unary and server-streaming remote-execution calls over HTTP/1.1-constrained paths
- Reject: client-streaming, duplex, and server hosting surface
