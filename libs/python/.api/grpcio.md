# [PY_BRANCH_API_GRPCIO]

`grpcio` supplies synchronous and async (`grpc.aio`) gRPC channel, server, stub, and interceptor types plus credential and compression constructors. It is the wire-transport layer for all gRPC service communication, wrapping a C-extension core via `_cython/cygrpc`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio`
- package: `grpcio`
- module: `grpc`
- asset: runtime library
- rail: transport
- namespaces: `grpc`, `grpc.aio`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and credentials family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                         |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Channel`             | abstract      | managed connection to server                                   |
|  [02]   | `ChannelCredentials`  | credentials   | TLS/composite channel security                                 |
|  [03]   | `CallCredentials`     | credentials   | per-call auth token attachment                                 |
|  [04]   | `ChannelConnectivity` | enum          | `IDLE`, `CONNECTING`, `READY`, `TRANSIENT_FAILURE`, `SHUTDOWN` |
|  [05]   | `Compression`         | enum          | `NoCompression`, `Deflate`, `Gzip`                             |
|  [06]   | `LocalConnectionType` | enum          | `LOCAL_TCP`, `UDS`                                             |
|  [07]   | `AuthMetadataPlugin`  | abstract      | async per-call auth metadata source                            |
|  [08]   | `AuthMetadataContext` | value         | method URI for auth metadata plugins                           |

[PUBLIC_TYPE_SCOPE]: callable family
- rail: transport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------- | :------------ | :----------------------------------- |
|  [01]   | `UnaryUnaryMultiCallable`   | callable      | invoke unary-unary RPC               |
|  [02]   | `UnaryStreamMultiCallable`  | callable      | invoke unary-server-stream RPC       |
|  [03]   | `StreamUnaryMultiCallable`  | callable      | invoke client-stream-unary RPC       |
|  [04]   | `StreamStreamMultiCallable` | callable      | invoke bidirectional-stream RPC      |
|  [05]   | `Future`                    | abstract      | async result handle for RPCs         |
|  [06]   | `Call`                      | abstract      | in-flight RPC state and cancellation |

[PUBLIC_TYPE_SCOPE]: server family
- rail: transport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------- |
|  [01]   | `Server`             | server        | managed gRPC server                        |
|  [02]   | `ServerCredentials`  | credentials   | TLS server security configuration          |
|  [03]   | `ServicerContext`    | abstract      | server-side RPC call context               |
|  [04]   | `GenericRpcHandler`  | abstract      | open-ended method dispatch handler         |
|  [05]   | `ServiceRpcHandler`  | abstract      | generated service handler base             |
|  [06]   | `RpcMethodHandler`   | value         | serializers and behavior per method        |
|  [07]   | `HandlerCallDetails` | value         | method name for interceptor dispatch       |
|  [08]   | `ServerInterceptor`  | abstract      | server-side RPC intercept contract         |
|  [09]   | `RpcContext`         | abstract      | shared RPC operation context               |
|  [10]   | `StatusCode`         | enum          | gRPC status codes (`OK`, `CANCELLED`, ...) |
|  [11]   | `Status`             | value         | code plus details string                   |
|  [12]   | `RpcError`           | exception     | gRPC call failure exception                |

[PUBLIC_TYPE_SCOPE]: interceptor family
- rail: transport

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------ | :------------ | :------------------------------- |
|  [01]   | `UnaryUnaryClientInterceptor`   | abstract      | intercept unary-unary calls      |
|  [02]   | `UnaryStreamClientInterceptor`  | abstract      | intercept unary-stream calls     |
|  [03]   | `StreamUnaryClientInterceptor`  | abstract      | intercept stream-unary calls     |
|  [04]   | `StreamStreamClientInterceptor` | abstract      | intercept bidirectional calls    |
|  [05]   | `ClientCallDetails`             | value         | method/metadata for interceptors |

[PUBLIC_TYPE_SCOPE]: grpc.aio family
- rail: transport

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :---------------------- | :------------ | :------------------------------- |
|  [01]   | `aio.Channel`           | async         | async-managed channel            |
|  [02]   | `aio.Server`            | async         | async gRPC server                |
|  [03]   | `aio.ServicerContext`   | async         | async server-side call context   |
|  [04]   | `aio.ClientInterceptor` | abstract      | async client interceptor base    |
|  [05]   | `aio.ServerInterceptor` | abstract      | async server interceptor base    |
|  [06]   | `aio.UnaryUnaryCall`    | call          | async unary-unary in-flight RPC  |
|  [07]   | `aio.UnaryStreamCall`   | call          | async unary-stream in-flight RPC |
|  [08]   | `aio.StreamUnaryCall`   | call          | async stream-unary in-flight RPC |
|  [09]   | `aio.StreamStreamCall`  | call          | async bidi-stream in-flight RPC  |
|  [10]   | `aio.AioRpcError`       | exception     | async RPC failure exception      |
|  [11]   | `aio.Metadata`          | value         | gRPC metadata carrier for aio    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel factory
- rail: transport

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `insecure_channel(target, options, compression)`                             | channel factory | plaintext channel           |
|  [02]   | `secure_channel(target, credentials, options, compression)`                  | channel factory | TLS/mTLS channel            |
|  [03]   | `ssl_channel_credentials(root_certificates, private_key, certificate_chain)` | credentials     | TLS client credentials      |
|  [04]   | `composite_channel_credentials(channel_credentials, *call_credentials)`      | credentials     | combine TLS + call auth     |
|  [05]   | `access_token_call_credentials(access_token)`                                | credentials     | bearer token per-call auth  |
|  [06]   | `metadata_call_credentials(metadata_plugin, name)`                           | credentials     | plugin-backed per-call auth |
|  [07]   | `intercept_channel(channel, *interceptors)`                                  | channel wrap    | attach client interceptors  |
|  [08]   | `aio.insecure_channel(target, options, ...)`                                 | async factory   | async plaintext channel     |
|  [09]   | `aio.secure_channel(target, credentials, ...)`                               | async factory   | async TLS channel           |

[ENTRYPOINT_SCOPE]: server factory
- rail: transport

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `server(thread_pool, handlers, interceptors, options, ...)`                | server factory | sync gRPC server             |
|  [02]   | `ssl_server_credentials(key_chain_pairs, root_certs, require_client_auth)` | credentials    | mTLS server creds            |
|  [03]   | `insecure_server_credentials()`                                            | credentials    | plaintext server credentials |
|  [04]   | `unary_unary_rpc_method_handler(behavior, ...)`                            | handler        | create unary-unary handler   |
|  [05]   | `unary_stream_rpc_method_handler(behavior, ...)`                           | handler        | create unary-stream handler  |
|  [06]   | `stream_unary_rpc_method_handler(behavior, ...)`                           | handler        | create stream-unary handler  |
|  [07]   | `stream_stream_rpc_method_handler(behavior, ...)`                          | handler        | create bidi-stream handler   |
|  [08]   | `method_handlers_generic_handler(service, method_handlers)`                | handler        | service handler map          |
|  [09]   | `aio.server(migration_thread_pool, handlers, interceptors, ...)`           | async server   | async gRPC server            |

[ENTRYPOINT_SCOPE]: StatusCode values
- rail: transport

| [INDEX] | [SYMBOL]                         | [VALUE] | [RAIL]                      |
| :-----: | :------------------------------- | ------: | :-------------------------- |
|  [01]   | `StatusCode.OK`                  |       0 | success                     |
|  [02]   | `StatusCode.CANCELLED`           |       1 | caller cancelled            |
|  [03]   | `StatusCode.UNKNOWN`             |       2 | unknown error               |
|  [04]   | `StatusCode.INVALID_ARGUMENT`    |       3 | bad request argument        |
|  [05]   | `StatusCode.DEADLINE_EXCEEDED`   |       4 | timeout                     |
|  [06]   | `StatusCode.NOT_FOUND`           |       5 | resource not found          |
|  [07]   | `StatusCode.ALREADY_EXISTS`      |       6 | resource exists             |
|  [08]   | `StatusCode.PERMISSION_DENIED`   |       7 | auth denied                 |
|  [09]   | `StatusCode.RESOURCE_EXHAUSTED`  |       8 | quota exceeded              |
|  [10]   | `StatusCode.FAILED_PRECONDITION` |       9 | state violation             |
|  [11]   | `StatusCode.ABORTED`             |      10 | concurrency conflict        |
|  [12]   | `StatusCode.UNIMPLEMENTED`       |      12 | method not implemented      |
|  [13]   | `StatusCode.INTERNAL`            |      13 | internal error              |
|  [14]   | `StatusCode.UNAVAILABLE`         |      14 | server unavailable          |
|  [15]   | `StatusCode.UNAUTHENTICATED`     |      16 | missing/invalid credentials |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- channels are thread-safe and reusable; create one channel per backend host per process and share across stubs
- `grpc.aio` requires an event loop; `aio.init_grpc_aio()` is called implicitly at first channel/server use on Python 3.10+
- generated stubs accept a `Channel` (sync) or `aio.Channel` (async) and return `Future`/`Call` (sync) or coroutines/async iterators (async)
- interceptors compose left-to-right with `intercept_channel`; server interceptors are passed at `server()` construction
- deadlines: pass `timeout` (seconds) to individual call invocations; do not rely on channel-level options for per-call deadlines
- `RpcError` carries `code()` and `details()` accessors; catch at service boundary and map to domain errors

[LOCAL_ADMISSION]:
- One channel instance per server address; stubs are lightweight wrappers around a shared channel.
- Use `grpc.aio` for async service code; `grpc` (sync) for sync or thread-pool wrappers.
- mTLS: `ssl_channel_credentials(root_certificates=ca_pem, private_key=key_pem, certificate_chain=cert_pem)`.

[RAIL_LAW]:
- Package: `grpcio`
- Owns: gRPC channel, server, credentials, interceptors, and call lifecycle
- Accept: one channel per host, `secure_channel` + `ssl_channel_credentials` for production, `aio` for async code
- Reject: per-request channel creation, catching broad `Exception` instead of `RpcError`, plaintext channels in production
