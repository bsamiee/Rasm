# [TS_RUNTIME_API_CONNECTRPC_CONNECT_NODE]

`@connectrpc/connect-node` is the Node dual-role transport `runtime` holds where `core`'s browser-only `@connectrpc/connect-web` cannot reach: one package carrying BOTH the server adapter `connectNodeAdapter` — projecting a `@connectrpc/connect` `ConnectRouter` into an `http.RequestListener`, the `serve/live.md` Mount port row — AND the three Node client `Transport` factories (`createConnectTransport`, `createGrpcTransport`, `createGrpcWebTransport`) feeding `net/client.md`'s lane table. Client design matches `connect-web` — every factory returns the same `@connectrpc/connect` `Transport`, so `createClient` stays protocol-agnostic and the `connect | grpc | grpc-web` choice is a lane policy row, never a second client.

Node adds the whole reason `runtime` needs a distinct package: a live server surface (the `ConnectRouter`/`ServiceImpl`/`HandlerContext` family `connect` marks OUT of client `wire`'s role runs HERE at the serve tier), a zlib compression algebra (`compressionGzip`/`compressionBrotli`, absent from the browser fetch transport), an `httpVersion` arm picking the Node `http`/`https` vs `http2` module (gRPC pins `http2`), and an explicit HTTP/2 keepalive manager (`Http2SessionManager` with PING-frame session pooling) for the long-lived fleet-worker connection. A hand-written W3C `Interceptor` pair carries trace context both directions — no TS `otelconnect` exists — riding the `interceptors` option on client egress and the router on server ingress, so the C# gRPC host and the TS services converse with unbroken causality.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-node`
- package: `@connectrpc/connect-node`
- license: `Apache-2.0`
- peer: `@connectrpc/connect` (exact — the `Transport`/`Interceptor`/`ConnectRouter`/`ConnectRouterOptions`/`ContextValues`/`ConnectError` types; `core/.api/connectrpc-connect.md`), `@bufbuild/protobuf ^catalog` (the JSON/binary read-write option types; `core/.api/bufbuild-protobuf.md`)
- effect-peer: none direct — the server `NodeHandlerFn` mounts under `serve/live.md`, the client `Transport` is wrapped in `effect` at `net/client.md` (`../../.api/effect.md`)
- catalog-verdict: KEEP — the Node server+client transport authority; `connect-web` is browser-only with no server surface and no `http2` client lane, and `connect` owns the transport-agnostic client, fault, and router types both consume
- runtime: node ≥20; ESM+CJS dual; server over `http`/`https`/`http2`, client over `http`/`https`/`http2` (the gRPC arm pins `http2`)
- modules: `.` — `createConnectTransport`, `createGrpcTransport`, `createGrpcWebTransport` and their options; `connectNodeAdapter` and `ConnectNodeAdapterOptions`; `compressionGzip`, `compressionBrotli`; `Http2SessionManager`, `Http2SessionOptions`; the `@private`/careful-use adapter internals `createNodeHttpClient`, `universalRequestFromNodeRequest`, `universalResponseToNodeResponse`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client transport options trio — one shape, a protocol discriminant and an `httpVersion` arm
- rail: net/client
- Three options records share one near-identical field set; `protocol` picks the factory and `httpVersion` picks the Node module. All three produce the identical `@connectrpc/connect` `Transport` the `Client<T>` consumes; rows past the three headers are fields carried on the records.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                                  |
| :-----: | :------------------------------------------ | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `ConnectTransportOptions`                   | transport policy  | connect arm — `http`/`https`/`http2`, `useHttpGet`, binary-default  |
|  [02]   | `GrpcTransportOptions`                      | transport policy  | grpc arm — `http2`-only, gRPC gateway compat                        |
|  [03]   | `GrpcWebTransportOptions`                   | transport policy  | grpc-web arm — `http`/`https`/`http2`, binary-default               |
|  [04]   | `.baseUrl: string`                          | endpoint          | route root `<baseUrl>/<pkg>.<Service>/<Method>`; from `host/config` |
|  [05]   | `.httpVersion: "1.1" \| "2"`                | transport arm     | Node module select — `http`/`https` vs `http2`; grpc pins `"2"`     |
|  [06]   | `.interceptors?: Interceptor[]`             | onion             | the `connect` `Interceptor` chain — the W3C trace pair, auth, retry  |
|  [07]   | `.useBinaryFormat?`                         | codec select      | binary vs JSON; binary content-stable, JSON debuggable              |
|  [08]   | `.binaryOptions?` / `.jsonOptions?`         | codec options     | `@bufbuild/protobuf` read-write options for the selected format     |
|  [09]   | `.acceptCompression?` / `.sendCompression?` | compression       | `Compression[]` / `Compression` — the zlib gzip/brotli algebra      |
|  [10]   | `.compressMinBytes?`                        | compression floor | below-threshold messages ship uncompressed; default 1 KiB          |
|  [11]   | `.readMaxBytes?` / `.writeMaxBytes?`        | frame bound       | per-message cap against pathological payloads; default ~4 GiB       |
|  [12]   | `.defaultTimeoutMs?`                        | deadline          | transport-wide deadline; per-call `CallOptions.timeoutMs` overrides |
|  [13]   | `.nodeOptions?`                             | socket / TLS      | passed to `http`/`https` `request()` or `http2` `connect()`         |
|  [14]   | `.useHttpGet?` (Connect arm)                | verb              | GET for idempotent side-effect-free unary methods                   |

[PUBLIC_TYPE_SCOPE]: the server adapter and the HTTP/2 session surface
- rail: serve/live
- `ConnectNodeAdapterOptions` extends the `connect` `ConnectRouterOptions`; `connectNodeAdapter` turns it into a `NodeHandlerFn` compatible with `http.RequestListener`. `Http2SessionManager`/`Http2SessionOptions` own the client-lane keepalive.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                        |
| :-----: | :------------------------------------------ | :-------------- | :-------------------------------------------------------- |
|  [01]   | `ConnectNodeAdapterOptions`                 | server mount    | extends `ConnectRouterOptions`; the router mount options  |
|  [02]   | `.routes: (router) => void`                 | route builder   | `router.service(Service, impl)` mounts the emitted service |
|  [03]   | `.contextValues?: (req) => ContextValues`   | per-req context | tenant/principal/deadline per inbound request             |
|  [04]   | `.fallback?: NodeHandlerFn`                 | 404 fallback    | handler when no RPC path matches                          |
|  [05]   | `.requestPathPrefix?: string`               | mount prefix    | serve all handlers under a path prefix                   |
|  [06]   | `NodeHandlerFn`                             | handler         | `(req, res) => void` — the `http.RequestListener` value   |
|  [07]   | `NodeServerRequest`/`NodeServerResponse`    | node io         | `http.IncomingMessage` \| `http2.Http2ServerRequest` + res |
|  [08]   | `Http2SessionManager`                       | keepalive class | `state`/`connect`/`request`/`abort`/`notifyResponseByteRead` |
|  [09]   | `Http2SessionOptions`                       | keepalive knobs | PING interval/timeout/idle-connection/idle-timeout ms     |
|  [10]   | `NodeHttpClientOptions`                     | client fn opts  | `createNodeHttpClient` shape — `@private`, no semver       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the three client transport factories and the lane keepalive
- rail: net/client
- Each factory is a pure `(options) => Transport`; the returned value is handed to `createClient(service, transport)` from `connect`. `net/client.md` calls exactly one per configured client, selected by the lane's `protocol`+`httpVersion` policy row.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                        |
| :-----: | :------------------------------------------- | :---------------- | :-------------------------------------------------------- |
|  [01]   | `createConnectTransport(options): Transport` | connect arm       | `protocol:"connect"` — `http`/`https`/`http2`, `useHttpGet` |
|  [02]   | `createGrpcTransport(options): Transport`    | grpc arm          | `protocol:"grpc"` — `http2`-only, native gRPC gateway     |
|  [03]   | `createGrpcWebTransport(options): Transport` | grpc-web arm      | `protocol:"grpc-web"` — `http`/`https`/`http2`, binary    |
|  [04]   | `compressionGzip` / `compressionBrotli`      | compression const | zlib `Compression` for `sendCompression`/`acceptCompression` |
|  [05]   | `new Http2SessionManager(url, ping?, opts?)` | keepalive         | shared `http2` connection kept alive with PING frames, pooled |

[ENTRYPOINT_SCOPE]: the server adapter and the framework-adapter internals
- rail: serve/live
- `connectNodeAdapter` is the Mount port entry; the `universal*` helpers and `createNodeHttpClient` are the lower-tier careful-use surface a non-standard Node framework host reaches for, never ordinary mount code.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------------------------------------- | :---------------- | :------------------------------------------------------ |
|  [01]   | `connectNodeAdapter(options): NodeHandlerFn`       | server mount      | `ConnectRouter` → `http.RequestListener`; Mount port row |
|  [02]   | `universalRequestFromNodeRequest(req,res,json,ctx)` | framework adapter | Node request → `UniversalServerRequest`; careful-use   |
|  [03]   | `universalResponseToNodeResponse(res, nodeRes)`    | framework adapter | `UniversalServerResponse` → Node response; careful-use  |
|  [04]   | `createNodeHttpClient(options): UniversalClientFn` | client fn         | `@private` internal — builds the universal client       |

## [04]-[IMPLEMENTATION_LAW]

[CONNECT_NODE_TOPOLOGY]:
- dual role, one package: connect-node is BOTH the server adapter (`connectNodeAdapter` — the `serve/live.md` Mount port) and the three Node client transports (the `net/client.md` lane), the reason `runtime` holds it distinct from `core`'s browser-only `connect-web` (`core/.api/connectrpc-connect-web.md`), which exposes no server surface and no `http2` client transport.
- transport axis is protocol × httpVersion: the three factories return the same `@connectrpc/connect` `Transport`, so `createClient` (`core/.api/connectrpc-connect.md`) is protocol-agnostic; the "connect | grpc | grpc-web" choice is a `net/client` lane policy row, and `httpVersion` `"1.1"`/`"2"` selects the Node module (`http`/`https` vs `http2`) with the gRPC arm pinned to `"2"`. One `Client<T>` shape across all arms — never a second client per protocol.
- server mount is descriptor-driven: `connectNodeAdapter({ routes })` mounts a `ConnectRouter` whose `router.service(Service, impl)` binds the same `@bufbuild/protobuf` `DescService` the C# `SdkTarget.TypeScript` generator emits; the returned `NodeHandlerFn` is `http.RequestListener`-compatible and slots into `serve/live.md`'s Mount port beside the assembled `HttpApi`; that `ConnectRouter`/`ServiceImpl`/`HandlerContext` server family — `connect` marks it OUT of client `wire`'s role — runs live HERE, at the runtime serve tier.
- compression is a node-only algebra: `compressionGzip`/`compressionBrotli` (zlib) feed `acceptCompression`/`sendCompression` under a `compressMinBytes` threshold — the browser fetch transport cannot compress, so this is a runtime-lane capability the `net/client` table owns.
- HTTP/2 keepalive is explicit: `Http2SessionManager` keeps a long-lived `http2` connection alive with PING frames (`Http2SessionOptions` `pingIntervalMs`/`pingTimeoutMs`/`pingIdleConnection`/`idleConnectionTimeoutMs`, the `GRPC_ARG_KEEPALIVE_*` mapping) and reopens transparently past a `GOAWAY`; `sessionManager`/`sessionProvider` pool connections across the fleet-worker lane — the long-lived-connection posture the browser transport never exposes.

[INTEGRATION_LAW]:
- Stack with `@connectrpc/connect` (`core/.api/connectrpc-connect.md`): the three factories return the `Transport` consumed by `createClient`; `connectNodeAdapter` mounts a `ConnectRouter` and threads per-request `ContextValues`; the `ConnectError`/`Code` fault fold, the `Interceptor` onion, and `CallOptions` all stay `connect`-owned — connect-node is the Node wire and the server host, `connect` is the client and router vocabulary.
- Stack with `@bufbuild/protobuf` (`core/.api/bufbuild-protobuf.md`): client and server share the emitted `DescService` descriptors; `useBinaryFormat` + `binaryOptions`/`jsonOptions` select the codec — binary the content-stable path for the C#-emitted services, JSON the debuggable Connect default.
- Stack with `effect` + `@effect/platform-node` (`../../.api/effect.md`, `../../.api/effect-platform-node.md`): client transports construct once at the `net/client.md` composition root, each unary method lifting through `Effect.tryPromise` and each server-streaming method through `Stream.fromAsyncIterable`; `CallOptions.signal` binds fiber interruption to `Code.Canceled`; the server `NodeHandlerFn` mounts under the platform-node HTTP server at `serve/live.md`; `nodeOptions` carries the TLS/socket policy `Config`-decoded from `host/config`.
- Stack with `@effect/opentelemetry` (`runtime/.api/effect-opentelemetry.md`): the hand-written W3C `Interceptor` pair reads `Tracer.currentOtelSpan` and writes/reads `traceparent` — injected on client egress through the `interceptors` option and extracted on server ingress inside the router — carrying trace context both directions because no TS `otelconnect` exists; `otel/emit.md`'s `Propagation` owns the header codec.
- Stack with the `net/client.md` lane budget: the transport inherits the lane table's timeout/retry/circuit rows — `defaultTimeoutMs` and `readMaxBytes`/`writeMaxBytes` are `Config`-decoded policy values and the retry `Schedule` gates on retryable `ConnectError.code`; a bespoke gRPC client bypassing the lane budget is the defect.

[LOCAL_ADMISSION]:
- select exactly one transport factory per configured client via the `net/client` lane's `protocol`+`httpVersion` row; never instantiate two arms for one descriptor or branch invocation logic downstream of `createClient`.
- mount the server via `connectNodeAdapter({ routes })` with `contextValues` extracting the per-request principal/tenant; the returned `NodeHandlerFn` is the `serve/live.md` Mount port row, never a hand-written Node request switch.
- pass the shared W3C `Interceptor` pair through `interceptors` (client) and the router (server); a transport or router bypassing trace propagation is the defect.
- keep `baseUrl`/`useBinaryFormat`/`defaultTimeoutMs`/`readMaxBytes`/`compressMinBytes` as `Config`-decoded values; a hardcoded endpoint, timeout, or frame cap is the parameterization defect.
- `Http2SessionManager` and `contextValues` carry no process-global state — per-transport session, per-request context — so two apps compose the serve port and the client lane without registry or connection collision.
- reach past `connectNodeAdapter` to the `@private`/careful-use `createNodeHttpClient` and `universalRequestFromNodeRequest`/`universalResponseToNodeResponse` only when hosting a non-standard Node server framework, never in ordinary mount code.

[RAIL_LAW]:
- Package: `@connectrpc/connect-node`
- Owns: the three Node client `Transport` factories (`createConnectTransport`, `createGrpcTransport`, `createGrpcWebTransport`) with their protocol×`httpVersion` option records, the `connectNodeAdapter` server mount projecting a `ConnectRouter` into an `http.RequestListener`, the zlib compression pair (`compressionGzip`/`compressionBrotli`), and the `Http2SessionManager` keepalive/pooling class with `Http2SessionOptions`
- Accept: one factory per client selected by the `net/client` `protocol`+`httpVersion` row, the returned `Transport` handed to `connect`'s `createClient`, `connectNodeAdapter` mounting the emitted `DescService` router at the `serve/live.md` Mount port, the shared W3C `Interceptor` pair on both egress and ingress, gzip/brotli compression under `compressMinBytes`, `Http2SessionManager` keepalive under `Config`-decoded ping budgets, `Config`-decoded `baseUrl`/timeout/frame-caps
- Reject: a client concept sourced here that `connect` owns (`CallOptions`/`ContextValues`/the fault algebra), two transport arms live for one descriptor, a hand-written Node request switch instead of `connectNodeAdapter`, a transport or router bypassing trace propagation or the net-lane budget, hardcoded endpoint/timeout/frame values, reaching for the `@private` `createNodeHttpClient`/`universal*` helpers in ordinary mount code
