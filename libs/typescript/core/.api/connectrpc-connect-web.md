# [TS_CORE_API_CONNECTRPC_CONNECT_WEB]

`@connectrpc/connect-web` mints the two public fetch `Transport` factories `interchange/invoke` seats as its browser/Bun adapter — `createConnectTransport` and `createGrpcWebTransport`. The package has no native gRPC factory, so `web + grpc` has no supported-pair type or schema arm. Both factories accept a `fetch` override; neither implements `grpc-web-text`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two public option records — one per supported web protocol, each yielding `Transport`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                 |
| :-----: | :---------------------------------- | :--------------- | :------------------------------------------------------------------ |
|  [01]   | `ConnectTransportOptions`           | transport policy | connect arm — `useBinaryFormat` default JSON, `useHttpGet`          |
|  [02]   | `GrpcWebTransportOptions`           | transport policy | grpc-web arm — `useBinaryFormat` default binary                     |
|  [03]   | `.baseUrl`                          | endpoint         | route root `<baseUrl>/<pkg>.<Service>/<Method>`; from `proc/config` |
|  [04]   | `.fetch?: typeof globalThis.fetch`  | transport port   | instrumented-fetch override — net policy, OTel headers, credentials |
|  [05]   | `.interceptors?: Interceptor[]`     | onion            | the `connect` `Interceptor` chain — trace propagation, auth, retry  |
|  [06]   | `.useBinaryFormat?`                 | codec select     | binary vs JSON select; binary content-stable, JSON debuggable       |
|  [07]   | `.binaryOptions?` / `.jsonOptions?` | codec options    | `@bufbuild/protobuf` read-write options for the selected format     |
|  [08]   | `.defaultTimeoutMs?`                | deadline         | transport-wide deadline; per-call `CallOptions.timeoutMs` overrides |
|  [09]   | `.useHttpGet?` (Connect)            | verb             | GET for idempotent side-effect-free unary                           |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two `(options) -> Transport` factories the web adapter records as its complete supported set

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                |
| :-----: | :-------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `createConnectTransport`          | connect arm    | `protocol:"connect"` — JSON-default, `useHttpGet`-capable          |
|  [02]   | `createGrpcWebTransport`          | grpc-web arm   | `protocol:"grpc-web"` — binary-default, gRPC gateway compat        |
|  [03]   | `{ fetch: instrumentedFetch }`    | fetch port     | `net/client` policy + `@effect/opentelemetry` `traceparent` egress |
|  [04]   | `{ interceptors: [trace, auth] }` | onion          | the shared `connect` `Interceptor` chain applied to every call     |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one adapter, two supported protocols: both factories return the same `@connectrpc/connect` `Transport`; `Invoke.Dial` selects one factory from the adapter's total record for each configured lane, and no client code branches on protocol.
- transport-only: this package exports the two factories and their option records; the typed `Client<T>`, `CallOptions`, `Interceptor`, and `ConnectError` all live in `@connectrpc/connect`, and `useBinaryFormat` + `binaryOptions`/`jsonOptions` select the `@bufbuild/protobuf` codec — binary for the C#-emitted services, JSON the Connect default.

[STACKING]:
- `@connectrpc/connect`(`.api/connectrpc-connect.md`): the factory output is the `Transport` argument to `createClient(service, transport)`; `interceptors` feed its shared `Interceptor` chain and `defaultTimeoutMs` seeds the deadline `CallOptions.timeoutMs` overrides per call.
- `@effect/platform-browser`(`.api/effect-platform-browser.md`): a lane needing upload/download progress or an `arraybuffer` response uses `BrowserHttpClient.layerXMLHttpRequest` + `withXHRArrayBuffer`; this package exposes no `grpc-web-text` or XHR transport.
- `@effect/opentelemetry`: a `connect` `Interceptor` reads `Tracer.currentOtelSpan` and writes W3C `traceparent` on egress, continuing the active trace across both transport arms without rewriting `fetch`.
- `effect`(`.api/effect.md`): `baseUrl`/`defaultTimeoutMs`/`useBinaryFormat` and the discriminated adapter lane are decoded once; `Invoke.Dial.web(fetch)` publishes the two factories and the selected transport builds at Dial construction.

[LOCAL_ADMISSION]:
- record both factories on the web adapter and invoke only the configured lane's member; `web + grpc` never reaches a guard because the discriminated lane schema cannot express it.
- pass the host `fetch` and shared `Interceptor` chain through the options; keep `baseUrl`/`useBinaryFormat`/`defaultTimeoutMs` decoded, and keep `useHttpGet` absent while no corpus RPC declares `NO_SIDE_EFFECTS`.
