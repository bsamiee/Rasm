# [PY_RUNTIME_API_GRPCIO]

`grpcio` supplies the gRPC runtime: the async `grpc.aio` server and channel, the synchronous `grpc` client surface, channel/server/call credentials, interceptors, the canonical status-code taxonomy, and compression. It is the runtime owner for the inbound companion gRPC server speaking the existing C# `ComputeService`/`ArtifactSync` contract.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio`
- package: `grpcio`
- import: `grpc`
- version: `1.81.1`
- owner: `runtime`
- rail: transport
- namespaces: `grpc`, `grpc.aio`
- capability: async gRPC server/channel, sync client, credentials, interceptors, status codes, compression
- admission note: pinned with `python_version<'3.13'` in the root manifest; under `requires-python='>=3.15'` it is present only as a marker-free transitive dependency of `specklepy`. First-class admission for the companion server requires the floor/lock-scope decision recorded in the suite TASKLOG.

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: async server/channel family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `aio.Server` | server | async gRPC server |
| [2] | `aio.Channel` | channel | async client channel |
| [3] | `aio.ServicerContext` | context | per-call server context |
| [4] | `aio.UnaryUnaryCall` | call | async unary-unary call |
| [5] | `aio.StreamStreamCall` | call | async bidi-stream call |
| [6] | `aio.ServerInterceptor` | interceptor | server-side interceptor |
| [7] | `aio.ClientInterceptor` | interceptor | client-side interceptor |
| [8] | `aio.Metadata` | metadata | call metadata map |
| [9] | `aio.AioRpcError` | fault | async RPC error |

[PUBLIC_TYPE_SCOPE]: status and credential family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `StatusCode` | enum | canonical RPC status codes |
| [2] | `Status` | status | trailing status value |
| [3] | `Compression` | enum | gzip/deflate/none compression |
| [4] | `ServerCredentials` | credential | server TLS credentials |
| [5] | `ChannelCredentials` | credential | channel TLS credentials |
| [6] | `CallCredentials` | credential | per-call credentials |
| [7] | `RpcError` | fault | sync RPC error |
| [8] | `RpcMethodHandler` | handler | method dispatch descriptor |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server and channel operations
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `aio.server` | build | construct an async server |
| [2] | `aio.Server.add_insecure_port` | bind | bind a plaintext port |
| [3] | `aio.Server.add_secure_port` | bind | bind a TLS port |
| [4] | `aio.Server.start` | lifecycle | start serving |
| [5] | `aio.Server.stop` | drain | graceful shutdown with grace period |
| [6] | `aio.Server.wait_for_termination` | lifecycle | await shutdown |
| [7] | `aio.secure_channel` | connect | TLS client channel |
| [8] | `aio.insecure_channel` | connect | plaintext client channel |
| [9] | `ssl_server_credentials` | credential | build server TLS credentials |
| [10] | `ssl_channel_credentials` | credential | build channel TLS credentials |
| [11] | `ServicerContext.abort` | fault | abort a call with a status |

## [4]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- server law: the companion inbound server is one `grpc.aio.server` under the anyio runner; servicers implement the generated stubs from the C# `.proto` descriptors, never a hand-rolled message loop.
- contract law: the wire contract is the existing C# `ComputeService`/`ArtifactSync` proto; the runtime compiles those descriptors and implements the server side, mirroring the C# contract exactly — no divergent message shapes.
- status law: outcomes map to the `StatusCode` enum; a domain `Error(BoundaryFault)` is converted to the matching status via `ServicerContext.abort`, never a bare exception across the wire.
- credential law: TLS credentials arrive from the settings model through `ssl_server_credentials`; `add_insecure_port` is reserved for loopback/test.
- drain law: `Server.stop(grace)` participates in the host drain choreography; in-flight calls complete within the grace period.
- compression law: large payloads use the `Compression.Gzip` knob; no manual payload compression.

[LOCAL_ADMISSION]:
- The companion-seam surface composes `grpc.aio` for the inbound server; the runtime owns no second RPC server.
- Generated stubs and message types arrive from the proto compilation step (`grpcio-tools`); the runtime implements servicers over them and converts to canonical shapes at the seam.

[RAIL_LAW]:
- Package: `grpcio`
- Owns: the inbound companion gRPC server speaking the C# `ComputeService`/`ArtifactSync` contract, plus client channels for paired calls
- Accept: one `grpc.aio.server`, generated-stub servicers, `StatusCode`/`abort` mapping, settings-model TLS, graceful drain, gzip compression
- Reject: hand-rolled message loops, divergent message shapes, bare exceptions across the wire, insecure ports in production, a second RPC server
