# [TS_CORE_API_CONNECTRPC_CONNECT_WEB]

`@connectrpc/connect-web` mints the two browser `Transport` factories `interchange/invoke` selects between — `createConnectTransport` and `createGrpcWebTransport` — one options record behind a `protocol` discriminant, no client surface of its own. Both are fetch-based: the `fetch` override is the seam carrying `host/net` policy and OTel propagation, and an absent `grpc-web-text` routes binary-frame progress or an `arraybuffer` response to `@effect/platform-browser` XHR.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-web`
- package: `@connectrpc/connect-web` (Apache-2.0)
- peer: `@connectrpc/connect` (`Transport`/`Interceptor`; `.api/connectrpc-connect.md`), `@bufbuild/protobuf` (JSON/binary read-write option types; `.api/bufbuild-protobuf.md`)
- effect-peer: none direct — `createClient` consumes the returned `Transport`, wrapped in `effect` at `interchange/invoke` (`.api/effect.md`)
- catalog-verdict: KEEP — the browser transport authority
- module: dual esm/cjs, one `.` export
- runtime: browser-primary (fetch API), node via global `fetch`; no `grpc-web-text`
- rail: interchange/invoke

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transport-options pair — one field set, one `protocol` discriminant, one `Transport` out

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :---------------------------------- | :--------------- | :------------------------------------------------------------------ |
|  [01]   | `ConnectTransportOptions`           | transport policy | connect arm — `useBinaryFormat` default JSON, `useHttpGet`          |
|  [02]   | `GrpcWebTransportOptions`           | transport policy | grpc-web arm — `useBinaryFormat` default binary                     |
|  [03]   | `.baseUrl`                          | endpoint         | route root `<baseUrl>/<pkg>.<Service>/<Method>`; from `host/config` |
|  [04]   | `.fetch?: typeof globalThis.fetch`  | transport seam   | instrumented-fetch override — net policy, OTel headers, credentials |
|  [05]   | `.interceptors?: Interceptor[]`     | onion            | the `connect` `Interceptor` chain — trace propagation, auth, retry  |
|  [06]   | `.useBinaryFormat?`                 | codec select     | binary vs JSON select; binary content-stable, JSON debuggable       |
|  [07]   | `.binaryOptions?` / `.jsonOptions?` | codec options    | `@bufbuild/protobuf` read-write options for the selected format     |
|  [08]   | `.defaultTimeoutMs?`                | deadline         | transport-wide deadline; per-call `CallOptions.timeoutMs` overrides |
|  [09]   | `.useHttpGet?` (Connect)            | verb             | GET for idempotent side-effect-free unary                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two `(options) -> Transport` factories the `protocol` value selects, one per client, and the `fetch`/`interceptors` seams threaded through the options

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `createConnectTransport`          | connect arm    | `protocol:"connect"` — JSON-default, `useHttpGet`-capable           |
|  [02]   | `createGrpcWebTransport`          | grpc-web arm   | `protocol:"grpc-web"` — binary-default, gRPC gateway compat         |
|  [03]   | `{ fetch: instrumentedFetch }`    | fetch seam     | `host/net` policy + `@effect/opentelemetry` `traceparent` on egress |
|  [04]   | `{ interceptors: [trace, auth] }` | onion          | the shared `connect` `Interceptor` chain applied to every call      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one axis, one client: both factories return the same `@connectrpc/connect` `Transport`, so `createClient` is protocol-agnostic and the connect-vs-grpc-web choice is a `protocol` discriminant selecting the factory, not a second client or code path.
- transport-only: this package exports the two factories and their option records; the typed `Client<T>`, `CallOptions`, `Interceptor`, and `ConnectError` all live in `@connectrpc/connect`, and `useBinaryFormat` + `binaryOptions`/`jsonOptions` select the `@bufbuild/protobuf` codec — binary for the C#-emitted services, JSON the Connect default.

[STACKING]:
- `@connectrpc/connect`(`.api/connectrpc-connect.md`): the factory output is the `Transport` argument to `createClient(service, transport)`; `interceptors` feed its shared `Interceptor` chain and `defaultTimeoutMs` seeds the deadline `CallOptions.timeoutMs` overrides per call.
- `@effect/platform-browser`(`.api/effect-platform-browser.md`): the `fetch` override binds the transport to the `host/net` default-policy client wrapped as a `fetch`; a lane needing upload/download progress or an `arraybuffer` response bypasses the transport for `BrowserHttpClient.layerXMLHttpRequest` + `withXHRArrayBuffer`, since `grpc-web-text` is unimplemented here.
- `@effect/opentelemetry`: a `connect` `Interceptor` reads `Tracer.currentOtelSpan` and writes W3C `traceparent` on egress, continuing the active trace across both transport arms without rewriting `fetch`.
- `effect`(`.api/effect.md`): `baseUrl`/`defaultTimeoutMs`/`useBinaryFormat`/`protocol` are `Config`-decoded from `host/config`; the transport builds once at the `interchange/invoke` root and the client wraps in `Effect.tryPromise`/`Stream.fromAsyncIterable`.

[LOCAL_ADMISSION]:
- select one factory per configured client by the `protocol` discriminant; never instantiate both for one descriptor or branch on protocol downstream of `createClient`.
- pass the instrumented `fetch` and the shared `Interceptor` chain through the options and keep `baseUrl`/`useBinaryFormat`/`defaultTimeoutMs`/`useHttpGet` `Config`-decoded; a bare transport bypassing `host/net` policy or a hardcoded endpoint is the parameterization defect.

[RAIL_LAW]:
- Package: `@connectrpc/connect-web`
- Owns: the two browser `Transport` factories and their near-identical option records that together are the `interchange/invoke` protocol axis
- Accept: one factory per client on a `protocol` discriminant, the `Transport` handed to `connect`'s `createClient`, the `fetch` override carrying `host/net` policy + OTel propagation, `useBinaryFormat`/`binaryOptions` as the codec selector, `Config`-decoded `baseUrl`/`defaultTimeoutMs`
- Reject: an invocation or client concept sourced here, both factories live for one descriptor, a bare transport bypassing net-client policy and trace propagation, hardcoded endpoint/timeout, expecting `grpc-web-text`/XHR streaming instead of `@effect/platform-browser`
