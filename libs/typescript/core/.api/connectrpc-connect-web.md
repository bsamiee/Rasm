# [TS_CORE_API_CONNECTRPC_CONNECT_WEB]

`@connectrpc/connect-web` supplies the two `Transport` implementations `interchange/invoke` selects between — `createConnectTransport` (the Connect protocol) and `createGrpcWebTransport` (gRPC-web) — and nothing else: it has no client surface of its own, because `@connectrpc/connect`'s `createClient` is generic over the `Transport` both factories return. That identity is the whole design: the `interchange/invoke` "protocol axis (connect | grpc-web)" is a single policy discriminant selecting one factory over one near-identical options record (`baseUrl`, `useBinaryFormat`, `interceptors`, `jsonOptions`/`binaryOptions`, `fetch`, `defaultTimeoutMs`; Connect adds `useHttpGet` for cacheable side-effect-free unary methods), never two client families. Both transports are fetch-based, so the `fetch` override is the integration seam onto the universal rails — an instrumented fetch carrying the `host/net` policy and `@effect/opentelemetry` `traceparent` header, or a credentials-bearing fetch. The catalog's job is to hold that axis as one parameterized surface and to fence the boundary: gRPC-web here does not implement the base64 `grpc-web-text` format, so a transport needing upload/download progress or an `arraybuffer` response reaches for the `@effect/platform-browser` XHR `HttpClient` instead, not this package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-web`
- package: `@connectrpc/connect-web`
- license: `Apache-2.0`
- peer: `@connectrpc/connect catalog` (exact — the `Transport`/`Interceptor` types; `.api/connectrpc-connect.md`), `@bufbuild/protobuf ^catalog` (the JSON/binary read-write option types; `.api/bufbuild-protobuf.md`)
- effect-peer: none direct — the returned `Transport` is consumed by `createClient`, wrapped in `effect` at `interchange/invoke` (`.api/effect.md`)
- catalog-verdict: KEEP — the browser transport authority; the `@connectrpc/connect` client is transport-agnostic and needs exactly this pair
- runtime: browser-primary (fetch API); node ≥18 capable via global `fetch`; NO `grpc-web-text` (no base6 catalog/XHR streaming — that lane is `@effect/platform-browser` XHR)
- modules: `.` — `createConnectTransport`, `createGrpcWebTransport`, `ConnectTransportOptions`, `GrpcWebTransportOptions`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transport-options pair — one shape, one protocol discriminant
- rail: interchange/invoke
- The two options records are the same field set save `useHttpGet` (Connect only) and the `useBinaryFormat` default (Connect → JSON, gRPC-web → binary). `wire` models the axis as one policy record plus a `protocol` discriminant that picks the factory; both produce the identical `Transport` the `connect` `Client<T>` consumes.

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                                            |
| :-----: | :-------------------------------------------------------- | :--------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `ConnectTransportOptions`                                 | transport policy | `interchange/invoke` connect arm — `useBinaryFormat` (default JSON), `useHttpGet`              |
|  [02]   | `GrpcWebTransportOptions`                                 | transport policy | `interchange/invoke` grpc-web arm — `useBinaryFormat` (default binary)                         |
|  [03]   | `.baseUrl` (both)                                         | endpoint         | `<baseUrl>/<pkg>.<Service>/<Method>` route root; from `host/config`                            |
|  [04]   | `.fetch?: typeof globalThis.fetch` (both)                 | transport seam   | the instrumented-fetch override — net-client policy, OTel headers, credentials                 |
|  [05]   | `.interceptors?: Interceptor[]` (both)                    | onion            | the `connect` `Interceptor` chain — trace propagation, auth, retry                             |
|  [06]   | `.useBinaryFormat?` / `.binaryOptions?` / `.jsonOptions?` | codec select     | `@bufbuild/protobuf` binary vs JSON read-write options; binary is content-stable               |
|  [07]   | `.defaultTimeoutMs?` / `.useHttpGet?` (Connect only)      | deadline / verb  | transport-wide deadline (per-call `CallOptions.timeoutMs` overrides); GET for idempotent unary |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two factories behind the one protocol discriminant
- rail: interchange/invoke
- Both are pure `(options) => Transport` factories; the returned value is handed to `createClient(service, transport)`. `wire` calls exactly one per configured client, selected by the `protocol` policy value — the factories are never both live for one descriptor.

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                              |
| :-----: | :-------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------- |
|  [01]   | `createConnectTransport(options: ConnectTransportOptions): Transport` | connect arm    | `interchange/invoke` `protocol:"connect"` — JSON-default, `useHttpGet`-capable   |
|  [02]   | `createGrpcWebTransport(options: GrpcWebTransportOptions): Transport` | grpc-web arm   | `interchange/invoke` `protocol:"grpc-web"` — binary-default, gRPC gateway compat |
|  [03]   | `{ fetch: instrumentedFetch }`                                        | fetch seam     | `host/net` policy + `@effect/opentelemetry` `traceparent` on egress              |
|  [04]   | `{ interceptors: [traceInterceptor, authInterceptor] }`               | onion          | the shared `connect` `Interceptor` chain applied to every call                   |

## [04]-[IMPLEMENTATION_LAW]

[CONNECT_WEB_TOPOLOGY]:
- one axis, one client: both factories return the same `@connectrpc/connect` `Transport`, so `createClient` is protocol-agnostic and the "connect | grpc-web" choice is a policy discriminant selecting the factory — not a second client, a second SDK, or a second code path. `interchange/invoke` carries one client shape and a `protocol` field.
- transport-only, no client surface: this package exports nothing but the two factories and their option types; the invocation, the typed `Client<T>`, `CallOptions`, `Interceptor`, and `ConnectError` all live in `@connectrpc/connect` — a `wire` file importing an invocation concept from `connect-web` is looking in the wrong owner.
- format is a codec parameter: `useBinaryFormat` + `binaryOptions`/`jsonOptions` select the `@bufbuild/protobuf` wire codec; binary is the content-stable, compact path for the C#-emitted services, JSON the human-debuggable default of the Connect arm.
- the fetch boundary is fenced: both transports use the fetch API; gRPC-web here omits `grpc-web-text`, so streaming rides fetch's `ReadableStream` body — a lane needing XHR upload/download progress or an `arraybuffer` response is `@effect/platform-browser`'s XHR `HttpClient`, not this package.

[INTEGRATION_LAW]:
- Stack with `@connectrpc/connect` (`.api/connectrpc-connect.md`): the factory output is the `Transport` argument to `createClient(service, transport)`; the `interceptors` are the shared `connect` `Interceptor` chain; `defaultTimeoutMs` seeds the deadline `CallOptions.timeoutMs` overrides. This package is the wire, `connect` is the client.
- Stack with `@effect/platform` `HttpClient` / `@effect/platform-browser` (`.api/effect-platform.md`, `.api/effect-platform-browser.md`): the `fetch` override binds the transport to an instrumented fetch — the `host/net` default-policy client wrapped as a `fetch`, or a credentials-bearing fetch; when binary-frame progress or `arraybuffer` is required, the transport is bypassed for `BrowserHttpClient.layerXMLHttpRequest`.
- Stack with `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): W3C `traceparent` propagation is injected by a `connect` `Interceptor` (reading `Tracer.currentOtelSpan`) rather than by rewriting `fetch`, so egress spans continue the active trace across both transport arms uniformly.
- Stack with `effect` (`.api/effect.md`) + `the `Dial.Config` policy record decoded at the interchange/invoke seam`: `baseUrl`/`defaultTimeoutMs`/`useBinaryFormat`/`protocol` are `Config`-decoded policy values from `host/config`, never hardcoded; the transport is constructed once at the `interchange/invoke` composition root and the client wrapped in `Effect.tryPromise`/`Stream.fromAsyncIterable` there.

[LOCAL_ADMISSION]:
- select exactly one factory per configured client via the `protocol` policy discriminant; never instantiate both for one descriptor or branch invocation logic on the protocol downstream of `createClient`.
- pass the instrumented `fetch` and the shared `Interceptor` chain through the options; never construct a bare transport that bypasses the `host/net` policy and trace propagation.
- keep `baseUrl`/`useBinaryFormat`/`defaultTimeoutMs`/`useHttpGet` as `Config`-decoded values; a hardcoded endpoint or timeout is the parameterization defect.
- reach past this package to `@effect/platform-browser` XHR only when upload/download progress or an `arraybuffer` response is required — `grpc-web-text` is unsupported here.

[RAIL_LAW]:
- Package: `@connectrpc/connect-web`
- Owns: the two browser `Transport` factories (`createConnectTransport`, `createGrpcWebTransport`) and their near-identical option records that together are the `interchange/invoke` protocol axis
- Accept: one factory per client selected by a `protocol` discriminant, the returned `Transport` handed to `connect`'s `createClient`, the `fetch` override carrying `host/net` policy + OTel propagation, `useBinaryFormat`/`binaryOptions` as the codec selector, `Config`-decoded `baseUrl`/`defaultTimeoutMs`
- Reject: any invocation/client concept sourced from this package (it is transport-only), both factories live for one descriptor, a bare transport bypassing net-client policy and trace propagation, hardcoded endpoint/timeout values, expecting `grpc-web-text`/XHR-streaming here instead of `@effect/platform-browser`
