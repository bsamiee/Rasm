# [PY_BRANCH_API_GRPCIO]

`grpcio` owns the branch gRPC wire transport: synchronous and async (`grpc.aio`) channel, server, stub, and interceptor surfaces with the credential and compression constructors every service seam binds. Message schema stays with the codec owner — the transport carries `bytes` and never owns the wire type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio`
- package: `grpcio` (`Apache-2.0`, gRPC Authors)
- module: `grpc`
- namespaces: `grpc`, `grpc.aio`
- asset: C-extension runtime over the C-core (`_cython/cygrpc`), not pure-Python
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and credentials family

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------- | :------------ | :----------------------------------- |
|  [01]   | `Channel`             | abstract      | managed connection to a backend host |
|  [02]   | `ChannelCredentials`  | credentials   | TLS/composite channel security       |
|  [03]   | `CallCredentials`     | credentials   | per-call auth token attachment       |
|  [04]   | `ChannelConnectivity` | enum          | connectivity state machine           |
|  [05]   | `Compression`         | enum          | payload compression selector         |
|  [06]   | `LocalConnectionType` | enum          | loopback transport selector          |
|  [07]   | `AuthMetadataPlugin`  | abstract      | async per-call auth metadata source  |
|  [08]   | `AuthMetadataContext` | value         | method URI for auth plugins          |

[ChannelConnectivity]: `IDLE` `CONNECTING` `READY` `TRANSIENT_FAILURE` `SHUTDOWN`
[Compression]: `NoCompression` `Deflate` `Gzip`
[LocalConnectionType]: `LOCAL_TCP` `UDS`

[PUBLIC_TYPE_SCOPE]: callable family

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------- | :------------ | :----------------------------------- |
|  [01]   | `UnaryUnaryMultiCallable`   | callable      | invoke unary-unary RPC               |
|  [02]   | `UnaryStreamMultiCallable`  | callable      | invoke unary-server-stream RPC       |
|  [03]   | `StreamUnaryMultiCallable`  | callable      | invoke client-stream-unary RPC       |
|  [04]   | `StreamStreamMultiCallable` | callable      | invoke bidirectional-stream RPC      |
|  [05]   | `Future`                    | abstract      | async result handle for RPCs         |
|  [06]   | `Call`                      | abstract      | in-flight RPC state and cancellation |

[PUBLIC_TYPE_SCOPE]: server family

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------- | :------------ | :----------------------------------- |
|  [01]   | `Server`             | server        | managed gRPC server                  |
|  [02]   | `ServerCredentials`  | credentials   | TLS server security configuration    |
|  [03]   | `ServicerContext`    | abstract      | server-side RPC call context         |
|  [04]   | `GenericRpcHandler`  | abstract      | open-ended method dispatch handler   |
|  [05]   | `ServiceRpcHandler`  | abstract      | generated service handler base       |
|  [06]   | `RpcMethodHandler`   | value         | serializers and behavior per method  |
|  [07]   | `HandlerCallDetails` | value         | method name for interceptor dispatch |
|  [08]   | `ServerInterceptor`  | abstract      | server-side RPC intercept contract   |
|  [09]   | `RpcContext`         | abstract      | shared RPC operation context         |
|  [10]   | `StatusCode`         | enum          | canonical gRPC status code set       |
|  [11]   | `Status`             | value         | code plus details string             |
|  [12]   | `RpcError`           | exception     | gRPC call failure exception          |

[PUBLIC_TYPE_SCOPE]: interceptor family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------------------ | :------------ | :------------------------------- |
|  [01]   | `UnaryUnaryClientInterceptor`   | abstract      | intercept unary-unary calls      |
|  [02]   | `UnaryStreamClientInterceptor`  | abstract      | intercept unary-stream calls     |
|  [03]   | `StreamUnaryClientInterceptor`  | abstract      | intercept stream-unary calls     |
|  [04]   | `StreamStreamClientInterceptor` | abstract      | intercept bidirectional calls    |
|  [05]   | `ClientCallDetails`             | value         | method/metadata for interceptors |

[PUBLIC_TYPE_SCOPE]: grpc.aio family

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `aio.Channel`                       | async         | async-managed channel                           |
|  [02]   | `aio.Server`                        | async         | async gRPC server                               |
|  [03]   | `aio.ServicerContext`               | async         | async server-side call context                  |
|  [04]   | `aio.ClientInterceptor`             | abstract      | async client interceptor base                   |
|  [05]   | `aio.ServerInterceptor`             | abstract      | async server interceptor base                   |
|  [06]   | `aio.UnaryUnaryClientInterceptor`   | abstract      | async unary-unary client interceptor            |
|  [07]   | `aio.UnaryStreamClientInterceptor`  | abstract      | async unary-stream client interceptor           |
|  [08]   | `aio.StreamUnaryClientInterceptor`  | abstract      | async stream-unary client interceptor           |
|  [09]   | `aio.StreamStreamClientInterceptor` | abstract      | async bidi-stream client interceptor            |
|  [10]   | `aio.UnaryUnaryCall`                | call          | async unary-unary in-flight RPC                 |
|  [11]   | `aio.UnaryStreamCall`               | call          | async unary-stream in-flight RPC                |
|  [12]   | `aio.StreamUnaryCall`               | call          | async stream-unary in-flight RPC                |
|  [13]   | `aio.StreamStreamCall`              | call          | async bidi-stream in-flight RPC                 |
|  [14]   | `aio.AioRpcError`                   | exception     | async RPC failure carrying `code()`/`details()` |
|  [15]   | `aio.Metadata`                      | value         | multi-dict gRPC metadata carrier                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel construction and lifecycle

| [INDEX] | [SURFACE]                                                   | [SHAPE]         | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `insecure_channel(target, options, compression)`            | channel factory | plaintext channel                                    |
|  [02]   | `secure_channel(target, credentials, options, compression)` | channel factory | TLS/mTLS channel                                     |
|  [03]   | `intercept_channel(channel, *interceptors)`                 | channel wrap    | attach client interceptors, compose left-to-right    |
|  [04]   | `channel_ready_future(channel)`                             | readiness       | `Future` resolving when channel reaches `READY`      |
|  [05]   | `protos(protobuf_path)`                                     | runtime codegen | import generated message module at runtime           |
|  [06]   | `services(protobuf_path)`                                   | runtime codegen | import generated stub module at runtime              |
|  [07]   | `protos_and_services(protobuf_path)`                        | runtime codegen | import message and stub modules without a build step |
|  [08]   | `aio.insecure_channel(target, options, ...)`                | async factory   | async plaintext channel                              |
|  [09]   | `aio.secure_channel(target, credentials, ...)`              | async factory   | async TLS channel                                    |
|  [10]   | `aio.init_grpc_aio()` / `aio.shutdown_grpc_aio()`           | async lifecycle | bind/release the aio C-core event loop               |

[ENTRYPOINT_SCOPE]: channel and call credentials — every row is a credentials factory.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `ssl_channel_credentials(root_certificates, private_key, certificate_chain)` | TLS client credentials                             |
|  [02]   | `composite_channel_credentials(channel_credentials, *call_credentials)`      | combine TLS and call auth                          |
|  [03]   | `access_token_call_credentials(access_token)`                                | bearer token per-call auth                         |
|  [04]   | `metadata_call_credentials(metadata_plugin, name)`                           | plugin-backed per-call auth                        |
|  [05]   | `composite_call_credentials(*call_credentials)`                              | merge multiple per-call creds                      |
|  [06]   | `local_channel_credentials(local_connect_type)`                              | UDS/loopback channel creds (`LocalConnectionType`) |
|  [07]   | `compute_engine_channel_credentials(call_credentials)`                       | GCE metadata-server channel creds                  |
|  [08]   | `xds_channel_credentials(fallback_credentials)`                              | xDS-driven channel creds with fallback             |
|  [09]   | `alts_channel_credentials(service_accounts)`                                 | ALTS mutual-auth channel creds                     |

[ENTRYPOINT_SCOPE]: server factory

| [INDEX] | [SURFACE]                                                                       | [SHAPE]        | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :--------------------------- |
|  [01]   | `server(thread_pool, handlers, interceptors, options, ...)`                     | server factory | sync gRPC server             |
|  [02]   | `ssl_server_credentials(...)`                                                   | credentials    | mTLS server creds            |
|  [03]   | `insecure_server_credentials()`                                                 | credentials    | plaintext server credentials |
|  [04]   | `dynamic_ssl_server_credentials(...)`                                           | credentials    | hot-reloadable server TLS    |
|  [05]   | `local_server_credentials(local_connect_type)`                                  | credentials    | UDS/loopback server creds    |
|  [06]   | `xds_server_credentials(fallback_credentials)` / `alts_server_credentials(...)` | credentials    | xDS / ALTS server creds      |
|  [07]   | `unary_unary_rpc_method_handler(behavior, ...)`                                 | handler        | create unary-unary handler   |
|  [08]   | `unary_stream_rpc_method_handler(behavior, ...)`                                | handler        | create unary-stream handler  |
|  [09]   | `stream_unary_rpc_method_handler(behavior, ...)`                                | handler        | create stream-unary handler  |
|  [10]   | `stream_stream_rpc_method_handler(behavior, ...)`                               | handler        | create bidi-stream handler   |
|  [11]   | `method_handlers_generic_handler(service, method_handlers)`                     | handler        | service handler map          |
|  [12]   | `aio.server(migration_thread_pool, handlers, interceptors, ...)`                | async server   | async gRPC server            |

- `ssl_server_credentials` args: `private_key_certificate_chain_pairs, root_certificates, require_client_auth`
- `dynamic_ssl_server_credentials` args: `initial_certificate_configuration, certificate_configuration_fetcher, require_client_authentication`

[ENTRYPOINT_SCOPE]: StatusCode values

| [INDEX] | [SYMBOL]                         | [VALUE] | [CAPABILITY]                |
| :-----: | :------------------------------- | :-----: | :-------------------------- |
|  [01]   | `StatusCode.OK`                  |    0    | success                     |
|  [02]   | `StatusCode.CANCELLED`           |    1    | caller cancelled            |
|  [03]   | `StatusCode.UNKNOWN`             |    2    | unknown error               |
|  [04]   | `StatusCode.INVALID_ARGUMENT`    |    3    | bad request argument        |
|  [05]   | `StatusCode.DEADLINE_EXCEEDED`   |    4    | timeout                     |
|  [06]   | `StatusCode.NOT_FOUND`           |    5    | resource not found          |
|  [07]   | `StatusCode.ALREADY_EXISTS`      |    6    | resource exists             |
|  [08]   | `StatusCode.PERMISSION_DENIED`   |    7    | auth denied                 |
|  [09]   | `StatusCode.RESOURCE_EXHAUSTED`  |    8    | quota exceeded              |
|  [10]   | `StatusCode.FAILED_PRECONDITION` |    9    | state violation             |
|  [11]   | `StatusCode.ABORTED`             |   10    | concurrency conflict        |
|  [12]   | `StatusCode.UNIMPLEMENTED`       |   12    | method not implemented      |
|  [13]   | `StatusCode.INTERNAL`            |   13    | internal error              |
|  [14]   | `StatusCode.UNAVAILABLE`         |   14    | server unavailable          |
|  [15]   | `StatusCode.UNAUTHENTICATED`     |   16    | missing/invalid credentials |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- channels are thread-safe and reusable — one channel per backend host per process, shared across stubs
- generated stubs bind a `Channel` (sync) or `aio.Channel` (async), returning `Future`/`Call` sync or coroutine/async-iterator async
- interceptors compose left-to-right via `intercept_channel`; server interceptors bind at `server()` construction
- per-call deadline rides the invocation `timeout` seconds, never a channel-level option
- `RpcError` and `aio.AioRpcError` carry `code()`/`details()`/`trailing_metadata()`; the service boundary catches and maps to a domain error
- `ChannelConnectivity` transitions `IDLE`->`CONNECTING`->`READY`->`TRANSIENT_FAILURE`->`SHUTDOWN`, observed via `channel.subscribe(callback)` and gated by `channel_ready_future`
- `Compression.Gzip`/`Deflate` set payload compression per channel or per call
- runtime codegen `protos`/`services`/`protos_and_services` import `.proto`-derived modules at runtime, dropping the protoc build step for un-pinned modules
- `aio.server(interceptors=[...])` drains one ordered lifecycle: `add_secure_port(addr, creds)`/`add_insecure_port(addr)`, `start()`, `stop(grace)`, `wait_for_termination(timeout)`; `add_insecure_port` admits only in-cluster and loopback targets
- `aio.ServicerContext` carries the per-RPC surface handlers compose directly: `abort`/`set_code`/`set_details` status egress, `invocation_metadata`/`send_initial_metadata`/`set_trailing_metadata` metadata seam, `read`/`write` streaming pump, `auth_context`/`peer_identities`/`peer`/`time_remaining` verified peer identity and inbound deadline — never a self-asserted metadata claim

[STACKING]:
- `protobuf`(`.api/protobuf.md`) / `msgspec`(`.api/msgspec.md`): the RPC body is `bytes` — the servicer decodes through the message owner (generated `protobuf` message or `msgspec.Decoder(type=T)`), validates, and re-encodes the response via `Encoder.encode_into` over a reused buffer; the transport carries bytes, never the schema
- `grpcio-tools`(`.api/grpcio-tools.md`): its `ProtoFinder`/`ProtoLoader` meta-path hooks back the `protos`/`services`/`protos_and_services` runtime dynamic-stub import
- `opentelemetry-api`(`.api/opentelemetry-api.md`): a client `UnaryUnaryClientInterceptor` stamps the active `Context` via `propagate.inject(metadata)`; a `ServerInterceptor` continues the trace via `propagate.extract(invocation_metadata)` + `start_as_current_span(kind=SpanKind.SERVER)`, `aio.Metadata` the W3C `traceparent` carrier
- `structlog`(`.api/structlog.md`): `RpcError.code().name`, `details()`, and `trailing_metadata()` are the bound fields the boundary logger emits mapping a gRPC failure to the domain rail
- retry rail (`stamina`): transient `StatusCode.UNAVAILABLE`/`DEADLINE_EXCEEDED`/`RESOURCE_EXHAUSTED` from `RpcError.code()` is the retriable predicate fed to `stamina.retry_context`; the channel is reused across attempts, never recreated, and the retry span nests under the client interceptor's otel span

[LOCAL_ADMISSION]:
- `grpc.aio` for async service code, sync `grpc` for thread-pool wrappers; `aio.init_grpc_aio()` binds the C-core to the running loop on first channel/server use
- mTLS binds `ssl_channel_credentials(root_certificates, private_key, certificate_chain)`; `dynamic_ssl_server_credentials` rotates server certs without downtime
- loopback/UDS pairs `local_channel_credentials(LocalConnectionType.UDS)` with `local_server_credentials` for in-host sidecar transport
- `CredentialPolicy` decode (the Python half of the C#-minted axis) is the single mint point for the TLS, local-loopback, and per-call credential families where the runtime serve leg composes this transport for HTTP/2 RPC — one RPC transport, one channel type per security mode; TLS material comes from the caller-owned settings model, never inline literals

[RAIL_LAW]:
- Package: `grpcio`
- Owns: gRPC channel, server, credentials, interceptors, and call lifecycle
- Accept: one channel per host, `secure_channel` + `ssl_channel_credentials` in production, `grpc.aio` for async code
- Reject: per-request channel creation, catching broad `Exception` over `RpcError`, plaintext channels in production
