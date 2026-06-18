# [PY_BRANCH_API_GRPCIO]

`grpcio` supplies synchronous and async (`grpc.aio`) gRPC channel, server, stub, and interceptor types plus credential and compression constructors. It is the wire-transport layer for all gRPC service communication, wrapping a C-extension core via `_cython/cygrpc`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio`
- package: `grpcio`
- module: `grpc`
- asset: runtime library
- rail: transport
- namespaces: `grpc`, `grpc.aio`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and credentials family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                         |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------- |
|   [1]   | `Channel`             | abstract      | managed connection to server                                   |
|   [2]   | `ChannelCredentials`  | credentials   | TLS/composite channel security                                 |
|   [3]   | `CallCredentials`     | credentials   | per-call auth token attachment                                 |
|   [4]   | `ChannelConnectivity` | enum          | `IDLE`, `CONNECTING`, `READY`, `TRANSIENT_FAILURE`, `SHUTDOWN` |
|   [5]   | `Compression`         | enum          | `NoCompression`, `Deflate`, `Gzip`                             |
|   [6]   | `LocalConnectionType` | enum          | `LOCAL_TCP`, `UDS`                                             |
|   [7]   | `AuthMetadataPlugin`  | abstract      | async per-call auth metadata source                            |
|   [8]   | `AuthMetadataContext` | value         | method URI for auth metadata plugins                           |

[PUBLIC_TYPE_SCOPE]: callable family
- rail: transport

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------- | :------------ | :----------------------------------- |
|   [1]   | `UnaryUnaryMultiCallable`   | callable      | invoke unary-unary RPC               |
|   [2]   | `UnaryStreamMultiCallable`  | callable      | invoke unary-server-stream RPC       |
|   [3]   | `StreamUnaryMultiCallable`  | callable      | invoke client-stream-unary RPC       |
|   [4]   | `StreamStreamMultiCallable` | callable      | invoke bidirectional-stream RPC      |
|   [5]   | `Future`                    | abstract      | async result handle for RPCs         |
|   [6]   | `Call`                      | abstract      | in-flight RPC state and cancellation |

[PUBLIC_TYPE_SCOPE]: server family
- rail: transport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------- |
|   [1]   | `Server`             | server        | managed gRPC server                        |
|   [2]   | `ServerCredentials`  | credentials   | TLS server security configuration          |
|   [3]   | `ServicerContext`    | abstract      | server-side RPC call context               |
|   [4]   | `GenericRpcHandler`  | abstract      | open-ended method dispatch handler         |
|   [5]   | `ServiceRpcHandler`  | abstract      | generated service handler base             |
|   [6]   | `RpcMethodHandler`   | value         | serializers and behavior per method        |
|   [7]   | `HandlerCallDetails` | value         | method name for interceptor dispatch       |
|   [8]   | `ServerInterceptor`  | abstract      | server-side RPC intercept contract         |
|   [9]   | `RpcContext`         | abstract      | shared RPC operation context               |
|  [10]   | `StatusCode`         | enum          | gRPC status codes (`OK`, `CANCELLED`, ...) |
|  [11]   | `Status`             | value         | code plus details string                   |
|  [12]   | `RpcError`           | exception     | gRPC call failure exception                |

[PUBLIC_TYPE_SCOPE]: interceptor family
- rail: transport

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------ | :------------ | :------------------------------- |
|   [1]   | `UnaryUnaryClientInterceptor`   | abstract      | intercept unary-unary calls      |
|   [2]   | `UnaryStreamClientInterceptor`  | abstract      | intercept unary-stream calls     |
|   [3]   | `StreamUnaryClientInterceptor`  | abstract      | intercept stream-unary calls     |
|   [4]   | `StreamStreamClientInterceptor` | abstract      | intercept bidirectional calls    |
|   [5]   | `ClientCallDetails`             | value         | method/metadata for interceptors |

[PUBLIC_TYPE_SCOPE]: grpc.aio family
- rail: transport

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :---------------------- | :------------ | :------------------------------- |
|   [1]   | `aio.Channel`           | async         | async-managed channel            |
|   [2]   | `aio.Server`            | async         | async gRPC server                |
|   [3]   | `aio.ServicerContext`   | async         | async server-side call context   |
|   [4]   | `aio.ClientInterceptor` | abstract      | async client interceptor base    |
|   [5]   | `aio.ServerInterceptor` | abstract      | async server interceptor base    |
|   [6]   | `aio.UnaryUnaryCall`    | call          | async unary-unary in-flight RPC  |
|   [7]   | `aio.UnaryStreamCall`   | call          | async unary-stream in-flight RPC |
|   [8]   | `aio.StreamUnaryCall`   | call          | async stream-unary in-flight RPC |
|   [9]   | `aio.StreamStreamCall`  | call          | async bidi-stream in-flight RPC  |
|  [10]   | `aio.AioRpcError`       | exception     | async RPC failure exception      |
|  [11]   | `aio.Metadata`          | value         | gRPC metadata carrier for aio    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel factory
- rail: transport

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :--------------------------------------------------------------------------- | :-------------- | :-------------------------- |
|   [1]   | `insecure_channel(target, options, compression)`                             | channel factory | plaintext channel           |
|   [2]   | `secure_channel(target, credentials, options, compression)`                  | channel factory | TLS/mTLS channel            |
|   [3]   | `ssl_channel_credentials(root_certificates, private_key, certificate_chain)` | credentials     | TLS client credentials      |
|   [4]   | `composite_channel_credentials(channel_credentials, *call_credentials)`      | credentials     | combine TLS + call auth     |
|   [5]   | `access_token_call_credentials(access_token)`                                | credentials     | bearer token per-call auth  |
|   [6]   | `metadata_call_credentials(metadata_plugin, name)`                           | credentials     | plugin-backed per-call auth |
|   [7]   | `intercept_channel(channel, *interceptors)`                                  | channel wrap    | attach client interceptors  |
|   [8]   | `aio.insecure_channel(target, options, ...)`                                 | async factory   | async plaintext channel     |
|   [9]   | `aio.secure_channel(target, credentials, ...)`                               | async factory   | async TLS channel           |

[ENTRYPOINT_SCOPE]: server factory
- rail: transport

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `server(thread_pool, handlers, interceptors, options, ...)`                | server factory | sync gRPC server             |
|   [2]   | `ssl_server_credentials(key_chain_pairs, root_certs, require_client_auth)` | credentials    | mTLS server creds            |
|   [3]   | `insecure_server_credentials()`                                            | credentials    | plaintext server credentials |
|   [4]   | `unary_unary_rpc_method_handler(behavior, ...)`                            | handler        | create unary-unary handler   |
|   [5]   | `unary_stream_rpc_method_handler(behavior, ...)`                           | handler        | create unary-stream handler  |
|   [6]   | `stream_unary_rpc_method_handler(behavior, ...)`                           | handler        | create stream-unary handler  |
|   [7]   | `stream_stream_rpc_method_handler(behavior, ...)`                          | handler        | create bidi-stream handler   |
|   [8]   | `method_handlers_generic_handler(service, method_handlers)`                | handler        | service handler map          |
|   [9]   | `aio.server(migration_thread_pool, handlers, interceptors, ...)`           | async server   | async gRPC server            |

[ENTRYPOINT_SCOPE]: StatusCode values
- rail: transport

| [INDEX] | [SYMBOL]                         | [VALUE] | [RAIL]                      |
| :-----: | :------------------------------- | ------: | :-------------------------- |
|   [1]   | `StatusCode.OK`                  |       0 | success                     |
|   [2]   | `StatusCode.CANCELLED`           |       1 | caller cancelled            |
|   [3]   | `StatusCode.UNKNOWN`             |       2 | unknown error               |
|   [4]   | `StatusCode.INVALID_ARGUMENT`    |       3 | bad request argument        |
|   [5]   | `StatusCode.DEADLINE_EXCEEDED`   |       4 | timeout                     |
|   [6]   | `StatusCode.NOT_FOUND`           |       5 | resource not found          |
|   [7]   | `StatusCode.ALREADY_EXISTS`      |       6 | resource exists             |
|   [8]   | `StatusCode.PERMISSION_DENIED`   |       7 | auth denied                 |
|   [9]   | `StatusCode.RESOURCE_EXHAUSTED`  |       8 | quota exceeded              |
|  [10]   | `StatusCode.FAILED_PRECONDITION` |       9 | state violation             |
|  [11]   | `StatusCode.ABORTED`             |      10 | concurrency conflict        |
|  [12]   | `StatusCode.UNIMPLEMENTED`       |      12 | method not implemented      |
|  [13]   | `StatusCode.INTERNAL`            |      13 | internal error              |
|  [14]   | `StatusCode.UNAVAILABLE`         |      14 | server unavailable          |
|  [15]   | `StatusCode.UNAUTHENTICATED`     |      16 | missing/invalid credentials |

## [4]-[IMPLEMENTATION_LAW]

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
