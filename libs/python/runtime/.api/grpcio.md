# [PY_RUNTIME_API_GRPCIO]

`grpcio` supplies the HTTP/2 RPC transport for the runtime transport/serve rail: the `grpc.aio` asyncio surface (server factory, channel factories, servicer context, server/client interceptors, unary and streaming stub callables) backed by the synchronous `grpc` channel/credential/status vocabulary. The package owner composes `grpc.aio.server`, the channel factories, and the `grpc.StatusCode`/credential/compression vocabulary into the `ServerHost` serve leg and the `Credential` admission path; it never re-implements the HTTP/2 framing, flow control, or wire codec the C-core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio`
- package: `grpcio`
- import: `grpc`
- owner: `runtime`
- rail: transport, serve
- version: `1.81.1`
- license: Apache-2.0
- gate: `[CORE]` ungated — `grpcio` resolves and installs on cp315 and is declared as an explicit ungated core direct dependency for runtime transport; it already enters the resolution transitively via the `google-cloud-storage` stack (`grpcio-status`, `grpc-google-iam-v1`)
- companion: `grpcio-tools` is the codegen/dev package (the `protoc` plugin plus `grpc_tools.protoc`/`grpc_tools.protoc.main` and the bundled `.proto` includes); it remains `[GATED]` `; python_version<'3.15'` because the bundled `protoc` build has no cp315 wheel, and it carries no separate catalog
- namespaces: `grpc`, `grpc.aio`
- installed: `1.81.1` resolves on cp315 as a transitive core dependency via `google-cloud-storage` (`grpcio-status` / `grpc-google-iam-v1`), now declared as an explicit ungated core direct dependency
- capability: asyncio HTTP/2 RPC server and channels, unary/streaming stub invocation, TLS channel/server credentials, status-code and compression vocabulary, server and client interceptor injection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: async serve and channel family
- rail: serve

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------- | :------------ | :------------------------------- |
|  [01]   | `grpc.aio.Server`           | server        | asyncio RPC server handle        |
|  [02]   | `grpc.aio.Channel`          | channel       | asyncio client channel           |
|  [03]   | `grpc.aio.ServicerContext`  | context       | per-RPC server-side context      |
|  [04]   | `grpc.aio.UnaryUnaryCall`   | call          | unary-unary client call handle   |
|  [05]   | `grpc.aio.UnaryStreamCall`  | call          | unary-stream client call handle  |
|  [06]   | `grpc.aio.StreamUnaryCall`  | call          | stream-unary client call handle  |
|  [07]   | `grpc.aio.StreamStreamCall` | call          | stream-stream client call handle |

[PUBLIC_TYPE_SCOPE]: interceptor family
- rail: serve

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------- | :------------ | :-------------------------------- |
|  [01]   | `grpc.aio.ServerInterceptor` | interceptor   | async server-leg interceptor base |
|  [02]   | `grpc.aio.ClientInterceptor` | interceptor   | async client-leg interceptor base |
|  [03]   | `grpc.ServerInterceptor`     | interceptor   | sync server-leg interceptor base  |

[PUBLIC_TYPE_SCOPE]: status and policy vocabulary
- rail: transport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :----------------- | :------------ | :------------------------------------- |
|  [01]   | `grpc.StatusCode`  | enum          | canonical RPC status-code vocabulary   |
|  [02]   | `grpc.Compression` | enum          | per-call/channel compression algorithm |

[PUBLIC_TYPE_SCOPE]: credential vocabulary
- rail: transport

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `grpc.ServerCredentials`   | credential    | opaque server-side transport credential handle              |
|  [02]   | `grpc.ChannelCredentials`  | credential    | opaque client-side channel credential handle                |
|  [03]   | `grpc.LocalConnectionType` | enum          | local-credential locality: `UDS` (unix socket), `LOCAL_TCP` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serve and channel construction
- rail: serve

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `grpc.aio.server(interceptors=..., ...)`            | serve          | construct the asyncio RPC server        |
|  [02]   | `grpc.aio.insecure_channel(target, ...)`            | connect        | open a plaintext asyncio client channel |
|  [03]   | `grpc.aio.secure_channel(target, credentials, ...)` | connect        | open a TLS asyncio client channel       |

[ENTRYPOINT_SCOPE]: async server lifecycle
- rail: serve
- defined on `grpc.aio.Server` (PUBLIC_TYPES [01]); the serve leg binds, starts, awaits, and drains through these.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `grpc.aio.Server.add_secure_port(addr, creds)`  | bind           | bind an address with `ServerCredentials`         |
|  [02]   | `grpc.aio.Server.add_insecure_port(addr)`       | bind           | bind a plaintext address                         |
|  [03]   | `grpc.aio.Server.start()`                       | lifecycle      | begin serving (awaitable)                        |
|  [04]   | `grpc.aio.Server.stop(grace)`                   | lifecycle      | graceful drain over the grace period (awaitable) |
|  [05]   | `grpc.aio.Server.wait_for_termination(timeout)` | lifecycle      | block until the server terminates (awaitable)    |

[ENTRYPOINT_SCOPE]: credential admission
- rail: transport

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `grpc.ssl_channel_credentials(...)`                  | credential     | mint client-side TLS channel credentials    |
|  [02]   | `grpc.ssl_server_credentials(...)`                   | credential     | mint server-side TLS credentials            |
|  [03]   | `grpc.local_server_credentials(local_connect_type)`  | credential     | mint server-side local/loopback credentials |
|  [04]   | `grpc.local_channel_credentials(local_connect_type)` | credential     | mint client-side local/loopback credentials |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- serve law: the serve leg constructs one `grpc.aio.server(...)`, registering servicers and passing `interceptors=[...]` at construction; the server is started and gracefully stopped deterministically under the anyio lane, never leaked across the event loop.
- channel law: clients open scoped `grpc.aio.insecure_channel`/`secure_channel` context managers; `insecure_channel` is admitted only for in-cluster/loopback targets, while external targets require `secure_channel` with `ssl_channel_credentials`.
- credential law: TLS material is minted through `ssl_channel_credentials`/`ssl_server_credentials` from the caller-owned `Credential` settings model, never from inline literals; the UDS loopback leg authenticates by socket locality through the `local_server_credentials(grpc.LocalConnectionType.UDS)`/`local_channel_credentials` pair, the kernel-reported peer credential standing in for a PEM bundle; the `Credential` owner is the single admission point for both the TLS and the local-loopback families.
- context law: server handlers read and set per-RPC metadata, deadline, and status through `grpc.aio.ServicerContext`; failures set `grpc.StatusCode` on the context rather than raising raw exceptions across the wire.
- interceptor law: cross-cutting concerns wire `grpc.aio.ServerInterceptor`/`ClientInterceptor` into the server/channel at construction; the sync `grpc.ServerInterceptor` covers blocking call sites only. Tracing interceptors come settled from `.api/opentelemetry-instrumentation-grpc.md` (`aio_server_interceptor`), not hand-rolled here.
- compression law: per-channel and per-call compression selects a `grpc.Compression` member; the runtime never re-implements the compression codec the C-core owns.

[LOCAL_ADMISSION]:
- The transport/serve surface composes `grpcio` for HTTP/2 RPC; the runtime owns no second RPC transport and no parallel channel type per security mode.
- Codegen is a build-time concern owned by the gated `grpcio-tools` companion (`grpc_tools.protoc`, `; python_version<'3.15'`); generated stubs are committed artifacts, never regenerated at runtime, and `grpcio-tools` carries no separate catalog.

[GATE_LAW]:
- `grpcio` 1.81.1 is `[CORE]` and ungated: it resolves and installs on cp315 and is declared as an explicit ungated core direct dependency for runtime transport. It is also pulled transitively through the `google-cloud-storage` stack (`grpcio-status` / `grpc-google-iam-v1`), so the explicit core declaration only names the transport leg the runtime already imports.
- The `grpcio-tools` codegen companion remains `[GATED]` `; python_version<'3.15'`: the bundled `protoc` build ships no cp315 wheel, so proto codegen runs on the gated sub-3.15 companion lane while the cp315 core imports `grpc`/`grpc.aio` directly and never imports `grpc_tools`.

[RAIL_LAW]:
- Package: `grpcio`
- Owns: asyncio HTTP/2 RPC serve and client channels, unary/streaming stub invocation, TLS credential minting, status-code and compression vocabulary, and server/client interceptor injection
- Accept: one `grpc.aio.server` with construction-time interceptors, the `grpc.aio.Server` `add_secure_port`/`add_insecure_port`/`start`/`stop`/`wait_for_termination` lifecycle surface, scoped `insecure_channel`/`secure_channel` sessions, `ssl_*_credentials`/`local_*_credentials` from the `Credential` model, `StatusCode`/`Compression`/`LocalConnectionType` vocabulary, settled tracing interceptors from the OTel grpc owner
- Reject: a second RPC transport or per-security-mode channel type, inline credential literals, hand-rolled framing/compression/tracing interceptors, runtime stub regeneration, a cp315-core import of the gated `grpc_tools` codegen companion, and a separate catalog for the `grpcio-tools` companion
