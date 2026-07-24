# [TS_RUNTIME_API_CONNECTRPC_CONNECT_NODE]

`@connectrpc/connect-node` is the Node dual-role transport `runtime` owns: the `connectNodeAdapter` server mount projecting a `@connectrpc/connect` `ConnectRouter` into an `http.RequestListener` (the `serve/live.md` Mount port), and the three client `Transport` factories `createConnectTransport`/`createGrpcTransport`/`createGrpcWebTransport` feeding `net/client.md`'s lane.

Every factory returns the same `Transport`, so `createClient` stays protocol-agnostic and the `connect | grpc | grpc-web` choice is a lane policy row, never a second client.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-node`
- package: `@connectrpc/connect-node` (Apache-2.0)
- peer: `@connectrpc/connect` (exact — `Transport`/`Interceptor`/`ConnectRouter`/`ConnectRouterOptions`/`ContextValues`/`ConnectError`; `core/.api/connectrpc-connect.md`), `@bufbuild/protobuf` (JSON/binary read-write option types; `core/.api/bufbuild-protobuf.md`)
- effect-peer: none direct — the server `NodeHandlerFn` mounts at `serve/live.md`, the client `Transport` wraps in `effect` at `net/client.md` (`../../.api/effect.md`)
- catalog-verdict: KEEP — the Node server+client transport authority
- module: single `.` export, dual ESM+CJS; server and client over `http`/`https`/`http2`, the gRPC arm pins `http2`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client transport options trio — `protocol` selects the factory, `httpVersion` selects the Node module, all three yield the identical `Transport` the `Client<T>` consumes; rail net/client. Rows past the three headers are the shared record fields.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :------------------------------------------ | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `ConnectTransportOptions`                   | transport policy  | connect arm — `http`/`https`/`http2`, `useHttpGet`, binary-default  |
|  [02]   | `GrpcTransportOptions`                      | transport policy  | grpc arm — `http2`-only, gRPC gateway compat                        |
|  [03]   | `GrpcWebTransportOptions`                   | transport policy  | grpc-web arm — `http`/`https`/`http2`, binary-default               |
|  [04]   | `.baseUrl: string`                          | endpoint          | route root `<baseUrl>/<pkg>.<Service>/<Method>`; from `host/config` |
|  [05]   | `.httpVersion: "1.1" \| "2"`                | transport arm     | Node module select — `http`/`https` vs `http2`; grpc pins `"2"`     |
|  [06]   | `.interceptors?: Interceptor[]`             | onion             | the `connect` `Interceptor` chain — the W3C trace pair, auth, retry |
|  [07]   | `.useBinaryFormat?`                         | codec select      | binary vs JSON; binary content-stable, JSON debuggable              |
|  [08]   | `.binaryOptions?` / `.jsonOptions?`         | codec options     | `@bufbuild/protobuf` read-write options for the selected format     |
|  [09]   | `.acceptCompression?` / `.sendCompression?` | compression       | `Compression[]` / `Compression` — the zlib gzip/brotli algebra      |
|  [10]   | `.compressMinBytes?`                        | compression floor | below-threshold messages ship uncompressed; default 1 KiB           |
|  [11]   | `.readMaxBytes?` / `.writeMaxBytes?`        | frame bound       | per-message cap against pathological payloads; default ~4 GiB       |
|  [12]   | `.defaultTimeoutMs?`                        | deadline          | transport-wide deadline; per-call `CallOptions.timeoutMs` overrides |
|  [13]   | `.nodeOptions?`                             | socket / TLS      | passed to `http`/`https` `request()` or `http2` `connect()`         |
|  [14]   | `.useHttpGet?` (Connect arm)                | verb              | GET for idempotent side-effect-free unary methods                   |

[PUBLIC_TYPE_SCOPE]: server adapter and HTTP/2 session surface — `ConnectNodeAdapterOptions` extends `ConnectRouterOptions`, `Http2SessionManager`/`Http2SessionOptions` own the client-lane keepalive; rail serve/live.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                          |
| :-----: | :---------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `ConnectNodeAdapterOptions`               | server mount    | extends `ConnectRouterOptions`; the router mount options     |
|  [02]   | `.routes: (router) => void`               | route builder   | `router.service(Service, impl)` mounts the emitted service   |
|  [03]   | `.contextValues?: (req) => ContextValues` | per-req context | tenant/principal/deadline per inbound request                |
|  [04]   | `.fallback?: NodeHandlerFn`               | 404 fallback    | handler when no RPC path matches                             |
|  [05]   | `.requestPathPrefix?: string`             | mount prefix    | serve all handlers under a path prefix                       |
|  [06]   | `NodeHandlerFn`                           | handler         | `(req, res) => void` — the `http.RequestListener` value      |
|  [07]   | `NodeServerRequest`/`NodeServerResponse`  | node io         | `http.IncomingMessage` \| `http2.Http2ServerRequest` + res   |
|  [08]   | `Http2SessionManager`                     | keepalive class | `state`/`connect`/`request`/`abort`/`notifyResponseByteRead` |
|  [09]   | `Http2SessionOptions`                     | keepalive knobs | PING interval/timeout/idle-connection/idle-timeout ms        |
|  [10]   | `NodeHttpClientOptions`                   | client fn opts  | `createNodeHttpClient` shape — `@private`, no semver         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the three `(options) -> Transport` factories and the lane keepalive — `net/client.md` calls one per client on the `protocol`+`httpVersion` row, the returned value handed to `connect`'s `createClient`; rail net/client.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                           |
| :-----: | :------------------------------------------- | :---------------- | :------------------------------------------------------------ |
|  [01]   | `createConnectTransport(options): Transport` | connect arm       | `protocol:"connect"` — `http`/`https`/`http2`, `useHttpGet`   |
|  [02]   | `createGrpcTransport(options): Transport`    | grpc arm          | `protocol:"grpc"` — `http2`-only, native gRPC gateway         |
|  [03]   | `createGrpcWebTransport(options): Transport` | grpc-web arm      | `protocol:"grpc-web"` — `http`/`https`/`http2`, binary        |
|  [04]   | `compressionGzip` / `compressionBrotli`      | compression const | zlib `Compression` for `sendCompression`/`acceptCompression`  |
|  [05]   | `new Http2SessionManager(url, ping?, opts?)` | keepalive         | shared `http2` connection kept alive with PING frames, pooled |

[ENTRYPOINT_SCOPE]: the server mount and framework-adapter internals — `connectNodeAdapter` is the Mount port; the `universal*`/`createNodeHttpClient` helpers are careful-use surface a non-standard Node host reaches for, never ordinary mount code; rail serve/live.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                      |
| :-----: | :-------------------------------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `connectNodeAdapter(options): NodeHandlerFn`        | server mount      | `ConnectRouter` → `http.RequestListener`; Mount port row |
|  [02]   | `universalRequestFromNodeRequest(req,res,json,ctx)` | framework adapter | Node request → `UniversalServerRequest`; careful-use     |
|  [03]   | `universalResponseToNodeResponse(res, nodeRes)`     | framework adapter | `UniversalServerResponse` → Node response; careful-use   |
|  [04]   | `createNodeHttpClient(options): UniversalClientFn`  | client fn         | `@private` internal — builds the universal client        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- dual role, one package: connect-node is BOTH the `connectNodeAdapter` server mount (the `serve/live.md` Mount port) and the three client transports (the `net/client.md` lane) — the reason `runtime` holds it distinct from `core`'s browser-only `connect-web` (`core/.api/connectrpc-connect-web.md`), which exposes no server surface and no `http2` client lane.
- transport axis is `protocol` × `httpVersion`: all three factories return the same `@connectrpc/connect` `Transport`, so `createClient` is protocol-agnostic; `httpVersion` `"1.1"`/`"2"` selects the Node module (`http`/`https` vs `http2`), the gRPC arm pinned to `"2"`. One `Client<T>` across every arm.
- server mount is descriptor-driven: `connectNodeAdapter({ routes })` binds `router.service(Service, impl)` over the `@bufbuild/protobuf` `DescService` the C# `SdkTarget.TypeScript` generator emits; the returned `NodeHandlerFn` is `http.RequestListener`-compatible, and the live `ConnectRouter`/`ServiceImpl`/`HandlerContext` server family runs at the runtime serve tier.
- node-only lane capabilities: `compressionGzip`/`compressionBrotli` (zlib) feed `acceptCompression`/`sendCompression` under `compressMinBytes`, and `Http2SessionManager` keeps a long-lived `http2` connection alive with PING frames (the `GRPC_ARG_KEEPALIVE_*` mapping), reopening past a `GOAWAY` and pooling via `sessionManager`/`sessionProvider` — neither exists on the browser fetch transport.

[STACKING]:
- `@connectrpc/connect`(`core/.api/connectrpc-connect.md`): the three factories return the `Transport` for `createClient`; `connectNodeAdapter` mounts a `ConnectRouter` threading per-request `ContextValues`; the `ConnectError`/`Code` fold, the `Interceptor` onion, and `CallOptions` stay `connect`-owned.
- `@bufbuild/protobuf`(`core/.api/bufbuild-protobuf.md`): client and server share the emitted `DescService`; `useBinaryFormat` + `binaryOptions`/`jsonOptions` select the codec — binary content-stable for the C#-emitted services, JSON the debuggable Connect default.
- `effect` + `@effect/platform-node`(`../../.api/effect.md`, `../../.api/effect-platform-node.md`): transports construct once at the `net/client.md` root, each unary method lifting through `Effect.tryPromise` and each server-streaming through `Stream.fromAsyncIterable`; `CallOptions.signal` binds fiber interruption to `Code.Canceled`; the `NodeHandlerFn` mounts under the platform-node HTTP server at `serve/live.md`; `nodeOptions` carries `Config`-decoded TLS/socket policy.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): the hand-written W3C `Interceptor` pair reads `Tracer.currentOtelSpan` and writes/reads `traceparent` — injected on client egress via `interceptors`, extracted on server ingress in the router — carrying trace both directions since no TS `otelconnect` exists; `otel/emit.md`'s `Propagation` owns the header codec.
- `net/client.md` lane budget (within-lib): the transport inherits the lane table's timeout/retry/circuit rows — `defaultTimeoutMs` and `readMaxBytes`/`writeMaxBytes` are `Config`-decoded and the retry `Schedule` gates on retryable `ConnectError.code`; a bespoke gRPC client bypassing the lane budget is the defect.

[LOCAL_ADMISSION]:
- mount the server through `connectNodeAdapter({ routes })` with `contextValues` extracting the per-request principal/tenant; the returned `NodeHandlerFn` is the `serve/live.md` Mount port, never a hand-written Node request switch.
- keep `baseUrl`/`useBinaryFormat`/`defaultTimeoutMs`/`readMaxBytes`/`compressMinBytes` `Config`-decoded; a hardcoded endpoint, timeout, or frame cap is the parameterization defect.
- `Http2SessionManager` and `contextValues` carry no process-global state — per-transport session, per-request context — so two apps compose the serve port and client lane without registry or connection collision.
- reach the `@private` `createNodeHttpClient`/`universalRequestFromNodeRequest`/`universalResponseToNodeResponse` only when hosting a non-standard Node server framework, never in ordinary mount code.

[RAIL_LAW]:
- Package: `@connectrpc/connect-node`
- Owns: the three Node client `Transport` factories with their `protocol`×`httpVersion` option records, the `connectNodeAdapter` server mount projecting a `ConnectRouter` into an `http.RequestListener`, the zlib `compressionGzip`/`compressionBrotli` pair, and the `Http2SessionManager`/`Http2SessionOptions` keepalive/pooling class
- Accept: one factory per client on the `net/client` `protocol`+`httpVersion` row, the `Transport` handed to `connect`'s `createClient`, `connectNodeAdapter` mounting the emitted `DescService` router at the `serve/live.md` Mount port, the shared W3C `Interceptor` pair on egress and ingress, gzip/brotli under `compressMinBytes`, `Config`-decoded `baseUrl`/timeout/frame-caps and ping budgets
- Reject: a client concept `connect` owns sourced here (`CallOptions`/`ContextValues`/the fault algebra), two transport arms live for one descriptor, a hand-written Node request switch instead of `connectNodeAdapter`, a transport or router bypassing trace propagation or the net-lane budget, hardcoded endpoint/timeout/frame values, the `@private` `createNodeHttpClient`/`universal*` helpers in ordinary mount code
