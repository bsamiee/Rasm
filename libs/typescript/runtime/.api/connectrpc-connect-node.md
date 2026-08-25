# [TS_RUNTIME_API_CONNECTRPC_CONNECT_NODE]

`@connectrpc/connect-node` is the Node dual-role package `runtime` owns: `connectNodeAdapter` projects a server router into an `http.RequestListener`, while the public Connect, gRPC-Web, and gRPC client factories form the scoped Node capability `net/client` hands `Invoke.Dial`. One `Http2SessionManager` supplies all three client transports for a peer.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client transport options trio — the factory names the protocol, `httpVersion` selects the Node module where applicable, and all three yield `Transport`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]     | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :------------------------------------ | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `ConnectTransportOptions`             | transport policy  | connect arm — `NodeTransportOptions` base, `useHttpGet`, binary     |
|  [02]   | `GrpcTransportOptions`                | transport policy  | grpc arm — `NodeHttp2TransportOptions` base, gRPC gateway compat    |
|  [03]   | `GrpcWebTransportOptions`             | transport policy  | grpc-web arm — `NodeTransportOptions` base, binary-default          |
|  [04]   | `.baseUrl: string`                    | endpoint          | route root `<baseUrl>/<pkg>.<Service>/<Method>`; from `proc/config` |
|  [05]   | `.httpVersion: "1.1" \| "2"`          | transport arm     | connect and grpc-web ONLY — `http`/`https` vs `http2`               |
|  [06]   | grpc record declares no `httpVersion` | transport arm     | its base carries none; `createGrpcTransport` injects `"2"` itself   |
|  [07]   | `.sessionManager?`                    | h2 residency      | supersedes `nodeOptions` AND every ping knob on the same record     |
|  [08]   | `.interceptors?: Interceptor[]`       | onion             | the `connect` `Interceptor` chain — the W3C trace pair, auth, retry |
|  [09]   | `.useBinaryFormat?`                   | codec select      | binary vs JSON; binary content-stable, JSON debuggable              |
|  [10]   | `.binaryOptions?` / `.jsonOptions?`   | codec options     | `@bufbuild/protobuf` read-write options for the selected format     |
|  [11]   | `.acceptCompression?: Compression[]`  | compression       | unset accepts gzip and br; `[]` opts out of response compression    |
|  [12]   | `.sendCompression?: Compression`      | compression       | unset ships requests UNCOMPRESSED — the pin is a declared value     |
|  [13]   | `.compressMinBytes?`                  | compression floor | below-threshold messages ship uncompressed; default 1 KiB           |
|  [14]   | `.readMaxBytes?` / `.writeMaxBytes?`  | frame bound       | per-message cap against pathological payloads; default ~4 GiB       |
|  [15]   | `.defaultTimeoutMs?`                  | deadline          | transport-wide deadline; per-call `CallOptions.timeoutMs` overrides |
|  [16]   | `.nodeOptions?`                       | socket / TLS      | passed to `http`/`https` `request()` or `http2` `connect()`         |
|  [17]   | `.useHttpGet?` (Connect arm)          | verb              | GET for idempotent side-effect-free unary methods                   |

[SESSION_MANAGER_SEAM]: `.sessionManager` accepts the concrete exported `Http2SessionManager`; providing it supersedes `nodeOptions` and the inline ping fields, so the adapter scopes one manager and never duplicates its residency on a transport record.

[PUBLIC_TYPE_SCOPE]: server adapter and HTTP/2 session surface — `ConnectNodeAdapterOptions` extends `ConnectRouterOptions`, `Http2SessionManager`/`Http2SessionOptions` own the client-lane keepalive; rail serve/live.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                                     |
| :-----: | :---------------------------------------- | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `ConnectNodeAdapterOptions`               | server mount    | extends `ConnectRouterOptions`; the router mount options                |
|  [02]   | `.routes: (router) => void`               | route builder   | `router.service(Service, impl)` mounts the emitted service              |
|  [03]   | `.contextValues?: (req) => ContextValues` | per-req context | tenant/principal/deadline per inbound request                           |
|  [04]   | `.fallback?: NodeHandlerFn`               | 404 fallback    | handler when no RPC path matches                                        |
|  [05]   | `.requestPathPrefix?: string`             | mount prefix    | serve all handlers under a path prefix                                  |
|  [06]   | `NodeHandlerFn`                           | handler         | `(req, res) => void` — the `http.RequestListener` value                 |
|  [07]   | `NodeServerRequest`/`NodeServerResponse`  | node io         | `http.IncomingMessage` \| `http2.Http2ServerRequest` + res              |
|  [08]   | `Http2SessionManager`                     | keepalive class | `authority`, `state()`, `error()`, `connect`, `request`, `abort`        |
|  [09]   | `.notifyResponseByteRead(stream)`         | keepalive duty  | a reader calls it per successful read or PING frames fire needlessly    |
|  [10]   | `Http2SessionOptions`                     | keepalive knobs | `pingIntervalMs`, `pingTimeoutMs`, `pingIdleConnection`, idle timeout   |
|  [11]   | `ConnectRouterOptions`                    | router options  | inherited public protocol enablement, codec, frame, and lifetime policy |
|  [12]   | `.interceptors` on every transport option | client onion    | `Interceptor[]` on the connect, grpc, and grpc-web records              |

[SERVER_POLICY_SEAM]: `ConnectNodeAdapterOptions` publicly extends `ConnectRouterOptions`, so protocol enablement, interceptors, compression, frame ceilings, deadlines, and shutdown policy arrive on the same server mount record as `routes`.

[ADAPTER_MUTATION_TRAP]: `connectNodeAdapter` WRITES its default compression roster back onto the options object it was handed before building the router, so the record is not treated as read-only — a frozen literal throws under strict mode and a record shared across two adapters carries the first one's mutation into the second. Build a fresh record per adapter, or declare `acceptCompression` and leave nothing to default.

[HANDLER_REJECTION_TRAP]: the returned `NodeHandlerFn` returns void and never rejects — it drives the universal handler's promise itself, returns silently on `Code.Aborted`, and routes every other failure to `console.error`. So a mounting fence's own error rail sees NOTHING from a served call: an RPC fault renders as a Connect wire error on the response and a transport fault reaches the package's console sink alone, which also means the sink escapes any log rail the host installed.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the three public `(options) -> Transport` factories, compression values, and HTTP/2 manager forming the scoped Node adapter; rail net/client

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                            |
| :-----: | :------------------------------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `createConnectTransport(options): Transport` | connect arm       | `protocol:"connect"` — `http`/`https`/`http2`, `useHttpGet`    |
|  [02]   | `createGrpcTransport(options): Transport`    | grpc arm          | `protocol:"grpc"` — `http2`-only, native gRPC gateway          |
|  [03]   | `createGrpcWebTransport(options): Transport` | grpc-web arm      | `protocol:"grpc-web"` — `http`/`https`/`http2`, binary         |
|  [04]   | `compressionGzip` / `compressionBrotli`      | compression const | zlib `Compression` the root hands `Invoke.Dial`'s seam         |
|  [05]   | `new Http2SessionManager(url, ping?, opts?)` | keepalive         | one `http2` connection; `opts` is `http2.ClientSessionOptions` |

[ENTRYPOINT_SCOPE]: the public server mount and framework-adapter helpers — `connectNodeAdapter` is the Mount port; rail serve/live

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]    | [CONSUMER_BOUNDARY]                                      |
| :-----: | :-------------------------------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `connectNodeAdapter(options): NodeHandlerFn`        | server mount      | `ConnectRouter` → `http.RequestListener`; Mount port row |
|  [02]   | `universalRequestFromNodeRequest(req,res,json,ctx)` | framework adapter | Node request → `UniversalServerRequest`; careful-use     |
|  [03]   | `universalResponseToNodeResponse(res, nodeRes)`     | framework adapter | `UniversalServerResponse` → Node response; careful-use   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- dual role, one package: connect-node is BOTH the `connectNodeAdapter` server mount (the `serve/live.md` Mount port) and the three client transports (the `net/client.md` lane) — the reason `runtime` holds it distinct from `core`'s browser-only `connect-web` (`core/.api/connectrpc-connect-web.md`), which exposes no server surface and no `http2` client lane.
- the client seat is a total public adapter: `createConnectTransport`, `createGrpcWebTransport`, and `createGrpcTransport` form its three factory arms, and `core:interchange/invoke#DIAL_AXIS` selects one arm from the discriminated lane; the Node adapter carries all three while the browser/Bun adapter cannot spell native gRPC.
- server mount is descriptor-driven: `connectNodeAdapter({ routes })` binds `router.service(Service, impl)` over the `@bufbuild/protobuf` `DescService` the C# `SdkTarget.TypeScript` generator emits; the returned `NodeHandlerFn` is `http.RequestListener`-compatible, and the live `ConnectRouter`/`ServiceImpl`/`HandlerContext` server family runs at the runtime serve tier.
- node-only lane capabilities: `compressionGzip`/`compressionBrotli` (zlib) feed `acceptCompression`/`sendCompression` under `compressMinBytes`, and `Http2SessionManager` keeps one `http2` connection alive with PING frames (the `GRPC_ARG_KEEPALIVE_*` mapping), maintaining a `GOAWAY`-flagged connection until its streams drain and opening a fresh one for new requests — neither reaches the browser fetch transport.
- session residency is caller-folded: `sessionManager` binds ONE manager per transport and the manager tracks ONE connection, so a per-origin pool is the composing fence's own scoped map — no undici dispatcher and no agent-style pool option governs this arm, and binding `sessionManager` voids `nodeOptions` and every ping knob on that same record.
- `GOAWAY` reopen is connection replacement inside one attempt, never a retry: no transport record publishes a retry or reconnect schedule, so an attempt curve over an rpc call belongs wholly to the composing fence's budget ledger.

[STACKING]:
- `@connectrpc/connect`(`core/.api/connectrpc-connect.md`): the three factories return the `Transport` for `createClient`; `connectNodeAdapter` mounts a `ConnectRouter` threading per-request `ContextValues`; the `ConnectError`/`Code` fold, the `Interceptor` onion, and `CallOptions` stay `connect`-owned.
- `@bufbuild/protobuf`(`../../.api/bufbuild-protobuf.md`): client and server share emitted `DescService` values and codec options.
- `effect` + `@effect/platform-node`(`../../.api/effect.md`, `../../.api/effect-platform-node.md`): transports construct once at the `net/client.md` root, each unary method lifting through `Effect.tryPromise` and each server-streaming through `Stream.fromAsyncIterable`; `CallOptions.signal` binds fiber interruption to `Code.Canceled`; the `NodeHandlerFn` mounts under the platform-node HTTP server at `serve/live.md`; `nodeOptions` carries `Config`-decoded TLS/socket policy.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): the hand-written W3C `Interceptor` pair reads `Tracer.currentOtelSpan` and writes/reads `traceparent` — injected on client egress via `interceptors`, extracted on server ingress through the inherited router option — carrying trace both directions since no TS `otelconnect` exists; `otel/emit.md`'s `Propagation` owns the header codec. `Interceptor` is `(next: AnyFn) => AnyFn` over an UNEXPORTED `AnyFn = (req: UnaryRequest | StreamRequest) => Promise<UnaryResponse | StreamResponse>`, so a composing fence spells the onion through the exported `Interceptor` alias and never annotates the inner function; the carrier is `header: Headers` on `RequestCommon` — present on BOTH request arms beside `requestMethod`, `url`, `signal`, and `contextValues` — while `ResponseCommon` carries `header`/`trailer` and no context values at all. Server-side the same alias wraps the IMPLEMENTATION invocation rather than the HTTP exchange: it runs after protocol negotiation and message decode, its `req.header` IS the handler context's inbound header bag and its `req.contextValues` IS the `ContextValues` the implementation then reads, and on any streaming method `next` settles once the response iterable is CONSTRUCTED — the messages flow after the onion returned, so an interceptor seats per-call policy and can never bracket a streaming body.
- `@effect/platform-node`(`../../.api/effect-platform-node.md`) node-handler lift: `HttpApp` exposes `fromWebHandler` over a FETCH-shaped handler alone and no member accepting a `NodeHandlerFn`, so an adapter reaches `HttpApp.Default` by pulling the request inside an effect and driving `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse` — the identical accessor pair `serve/route.md`'s rail mount already drives a raw node handler through, and the mount rides `Seam.guard` by construction because the router attaches that middleware once above every mounted row.
- `net/client.md` Node adapter (within-lib): the seat scopes one `Http2SessionManager` per origin and publishes the three public factory closures to the dial seam; frame caps, the retry ladder, execution-plan failover, and the `Code`→class grading are `core:interchange/invoke`'s and `Wire.Hops`'s — a policy or budget minted here is the second owner the branch deleted.

[LOCAL_ADMISSION]:
- mount the server through `connectNodeAdapter({ routes })` with `contextValues` extracting the per-request principal/tenant; the returned `NodeHandlerFn` is the `serve/live.md` Mount port, never a hand-written Node request switch.
- `contextValues` is the EARLIEST server seam and the only one holding the raw Node request — it runs before path dispatch and before any decode, so per-request identity crosses there and an interceptor is left to policy that needs the decoded message.
- `requestPathPrefix` and the mounting route's own prefix are ONE value: the adapter matches `prefix + requestPath` against the raw url, so a mismatch serves a path no client reaches and answers the adapter's 404 for every call.
- keep `baseUrl` `Config`-decoded and every codec, frame, and compression coordinate a declared row value; a hardcoded endpoint or an inherited default a reader cannot see is the parameterization defect.
- declare `sendCompression` rather than inheriting the uncompressed default, since the record exposes the knob and silence ships every request raw.
- `Http2SessionManager` and `contextValues` carry no process-global state — per-transport session, per-request context — so two apps compose the serve port and client lane without registry or connection collision.
- scope each `Http2SessionManager` and `abort()` it on release; a manager outliving its root holds an open connection and every stream on it.
- construct every client arm through its public factory and bind the same scoped `Http2SessionManager`; reach `universalRequestFromNodeRequest`/`universalResponseToNodeResponse` only when hosting a non-standard Node server framework.
