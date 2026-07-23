# [TS_CORE_API_CONNECTRPC_CONNECT]

`@connectrpc/connect` is the protocol-neutral RPC client `interchange/invoke` composes and the runtime `interchange/invoke` binds the C#-emitted capability SDK to. It splits cleanly into a `Transport` (the wire protocol — supplied by `@connectrpc/connect-web`) and a `createClient(service, transport)` that projects a `DescService` descriptor into a fully typed `Client<T>` whose method shapes are derived by the type system (unary → `Promise`, server-streaming → `AsyncIterable`, client-streaming → `Promise` over an input `AsyncIterable`, bidi → `AsyncIterable`). The `service` is the `@bufbuild/protobuf` `DescService` the C# `SdkTarget.TypeScript` generator emits, so the SDK is one descriptor, not a hand-written client family. The advanced surface `wire` leans on is the fault + resilience seam: `ConnectError` (a `Code` from the 16-value gRPC enum + `metadata` `Headers` + protobuf `details` retrievable by `findDetails`, with a `ConnectError.from` that normalizes `AbortError`/`TimeoutError` to `Code.Canceled`) is the `interchange/codec` `fromConnect` fold source; the `Interceptor` onion carries W3C trace propagation and auth; `CallOptions` (`signal`/`timeoutMs`/`contextValues`/`onHeader`/`onTrailer`) binds Effect interruption and per-call context; and the `./protocol` toolkit (`createMethodUrl`, `Serialization`, `runUnaryCall`/`runStreamingCall`, the envelope stream codec, the async-iterable transforms, `createDeadlineSignal`) is the kit for a fully Effect-native `Transport` over the shared `@effect/platform` `HttpClient`. The server-side router surface exists but is out of `wire`'s decode/client role.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect`
- package: `@connectrpc/connect` (Apache-2.0)
- peer: `@bufbuild/protobuf ^catalog` (`DescService`/`DescMethod*`/`MessageInitShape`/`MessageShape`/`create`/`fromBinary`/`toBinary`; `.api/bufbuild-protobuf.md`)
- effect-peer: none direct — client `Promise`/`AsyncIterable` cross into `effect` `Effect.tryPromise`/`Stream.fromAsyncIterable` at the `interchange/invoke` seam (`.api/effect.md`)
- catalog-verdict: KEEP — the invocation-client authority; `@connectrpc/connect-web` supplies its `Transport`, `@bufbuild/protobuf` its descriptors
- runtime: isomorphic, `sideEffects:false`; the client is transport-agnostic — browser (`connect-web` fetch) or any `Transport`
- modules / subpaths: `.` (client + error + interceptor + context), `./protocol` (transport-construction toolkit), `./protocol-connect`, `./protocol-grpc`, `./protocol-grpc-web`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transport, the typed client, and per-call options
- rail: interchange/invoke
- `Transport` is the two-method protocol abstraction (`unary`/`stream`); `Client<Desc>` is the descriptor-derived typed client; `CallOptions` is the per-call knob record carrying `timeoutMs`/`headers`/`signal`/`onHeader`/`onTrailer`/`contextValues`. `ContextValues` carries `.get`/`.set`/`.delete`. Streaming request/response messages are `AsyncIterable<MessageShape<...>>`, the direct `effect` `Stream` bridge.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                             |
| :-----: | :------------------------------------------------ | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `Transport` (`.unary`, `.stream`)                 | protocol port    | from `connect-web`; `createClient` is generic over it           |
|  [02]   | `Client<Desc extends DescService>`                | typed client     | unary→`Promise`, stream→`AsyncIterable`; from descriptor        |
|  [03]   | `CallOptions`                                     | call knobs       | `signal` binds interruption; `timeoutMs`≤0 disables the default |
|  [04]   | `CallbackClient<Desc>` / `AnyClient`              | client variant   | callback + dynamic flavors; `wire` uses the promise `Client`    |
|  [05]   | `Interceptor` = `(next) => (req) => Promise<res>` | middleware       | the layered onion around a call — trace, auth, logging          |
|  [06]   | `UnaryRequest` / `UnaryResponse`                  | interceptor io   | `stream:false` arm — `message`/`method`/`header`/`signal`       |
|  [07]   | `StreamRequest` / `StreamResponse`                | interceptor io   | `stream:true` arm — `message`, `trailer`, `contextValues`       |
|  [08]   | `ContextValues` / `ContextKey<T>`                 | per-call context | tenant, deadline, HLC through interceptors without global state |

[PUBLIC_TYPE_SCOPE]: the fault algebra `interchange/codec` folds
- rail: interchange/codec
- `ConnectError` (`code`/`metadata`/`details`/`rawMessage`/`cause`) is the one transport fault; `Code` is the closed 16-value gRPC-aligned enum that maps to the `HopReason` vocabulary. `details` are protobuf `Any`-wrapped messages the server attaches, decoded by `findDetails`.

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                               |
| :-----: | :---------------------------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `ConnectError`                                                    | transport fault  | `fromConnect` fold source; at `Effect.tryPromise` |
|  [02]   | `Code` (`Canceled`…`Unauthenticated`, 1–16)                       | closed code enum | retryability + `HopReason`; `Match.exhaustive`    |
|  [03]   | `ConnectError.from(reason, code?)`                                | normalizer       | any reason → `ConnectError` (Abort→Canceled)      |
|  [04]   | `ConnectError.findDetails(desc \| registry)`                      | detail decode    | `Any`-wrapped error details → typed messages      |
|  [05]   | `ServiceImpl` / `MethodImpl` / `HandlerContext` / `ConnectRouter` | server-side      | OUT of `wire`'s client role; import is the defect |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the client and the invocation seam
- rail: interchange/invoke
- `createClient(service, transport)` is the one client factory; the `service` is the codegen `DescService`, the `transport` the `connect-web` factory output. `createContextValues`/`createContextKey` build the per-call context; `applyInterceptors` composes the onion.

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                                        |
| :-----: | :------------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `createClient(service, transport): Client<T>`      | client          | the capability SDK from the emitted `DescService`          |
|  [02]   | `createCallbackClient(service, transport)`         | client variant  | the callback flavor; the promise client is `wire`'s path   |
|  [03]   | `makeAnyClient(service, createMethod)`             | dynamic client  | the dynamic-method flavor over an arbitrary method map     |
|  [04]   | `createContextValues()`                            | context bag     | the per-call `ContextValues` store                         |
|  [05]   | `createContextKey(default, { description? })`      | context key     | per-call tenant/deadline/HLC keys threaded to interceptors |
|  [06]   | `applyInterceptors(next, interceptors)`            | onion           | compose trace + auth + retry interceptors around the call  |
|  [07]   | `encodeBinaryHeader(value, desc?)`                 | `-bin` write    | encode a protobuf message into `-bin` header metadata      |
|  [08]   | `decodeBinaryHeader(value, type?)`                 | `-bin` read     | decode `-bin` header metadata into a protobuf message      |
|  [09]   | `appendHeaders(...h)`                              | header merge    | merge multiple `Headers` into one                          |
|  [10]   | `ConnectError.from(reason, code?)`                 | fault normalize | normalize any caught reason → `ConnectError`               |
|  [11]   | `err.findDetails(desc)`                            | detail decode   | decode typed `Any`-wrapped error details                   |
|  [12]   | `createRouterTransport(routes, options?)` / `cors` | in-proc / CORS  | in-memory `Transport` for kit specs; CORS helper           |

[ENTRYPOINT_SCOPE]: the `./protocol` toolkit — building a custom Effect-native transport
- rail: interchange/invoke
- `./protocol` is the kit a fully `Effect`-owned `Transport` is assembled from over the shared `@effect/platform` `HttpClient`, instead of the fetch-bound `connect-web` factories: the method-URL builder, the serialization lookup, the call runners, the envelope stream codec, the async-iterable transforms, and the deadline/abort signal helpers.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `createMethodUrl(baseUrl, method)` | url build      | `<baseUrl>/<pkg>.<Service>/<Method>` route          |
|  [02]   | `createMethodSerializationLookup`  | codec lookup   | per-method binary/JSON `Serialization` lookup       |
|  [03]   | `createClientMethodSerializers`    | codec pair     | the binary/JSON `Serialization` pair for a method   |
|  [04]   | `runUnaryCall(opts)`               | unary runner   | the invocation a custom `Transport.unary` wraps     |
|  [05]   | `runStreamingCall(opts)`           | stream runner  | the invocation a custom `Transport.stream` wraps    |
|  [06]   | `encodeEnvelope`                   | envelope write | length-prefix a frame for streaming egress          |
|  [07]   | `createEnvelopeReadableStream`     | envelope read  | read length-prefixed frames off a body stream       |
|  [08]   | `EnvelopedMessage`                 | envelope shape | the `{ flags, data }` frame record                  |
|  [09]   | `transformSplitEnvelope`           | envelope split | split a body into enveloped frames                  |
|  [10]   | `pipe`                             | compose        | thread a body through the transform algebra         |
|  [11]   | `createAsyncIterable`              | source         | lift values into an `AsyncIterable` body            |
|  [12]   | `makeIterableAbortable`            | abort          | make an iterable abortable on signal                |
|  [13]   | `sinkAllBytes`                     | sink           | drain a byte iterable to one buffer                 |
|  [14]   | `transformParseEnvelope`           | parse          | parse enveloped frames back to messages             |
|  [15]   | `createDeadlineSignal(timeoutMs)`  | deadline       | deadline `AbortSignal` bound to Effect interruption |
|  [16]   | `createLinkedAbortController`      | linked abort   | chain an abort to a parent signal                   |
|  [17]   | `getAbortSignalReason`             | abort reason   | read the reason off an aborted signal               |
|  [18]   | `createFetchClient`                | fetch client   | the universal client over `fetch`                   |
|  [19]   | `universalClientRequestToFetch`    | request map    | map a universal request to a `fetch` request        |
|  [20]   | `contentTypeMatcher`               | content type   | match the response content-type to a codec          |

## [04]-[IMPLEMENTATION_LAW]

[CONNECT_TOPOLOGY]:
- transport and client are orthogonal: `Transport` implements the protocol (Connect or gRPC-web, from `connect-web`), `createClient` is generic over it, so the protocol is a `Transport` selection, not a client fork — one `Client<T>` shape across both arms (`interchange/invoke`'s `protocol` axis is the transport factory choice, `.api/connectrpc-connect-web.md`).
- the SDK is a descriptor, not a hand-written client: `createClient(service, transport)` derives every method signature from the `@bufbuild/protobuf` `DescService` the C# `SdkTarget.TypeScript` generator emits; `interchange/invoke` binds the emitted descriptor, and a hand-authored client method is the drift defect.
- the error is the fault-fold source: a failed call rejects with a `ConnectError` carrying `code` (the closed `Code` enum), `metadata` (`Headers`), and protobuf `details`; `interchange/codec`'s `fromConnect` folds `code` → the closed `HopReason` vocabulary and `findDetails` decodes typed detail messages — the wire-only fault altitude, distinct from any local `Data.TaggedError`.
- interception is the cross-cutting seam: an `Interceptor` wraps the invocation as a layered onion — trace propagation, auth headers, and per-call `ContextValues` attach here, not inside the call site.

[INTEGRATION_LAW]:
- Stack with `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): this package is the message RUNTIME under Connect — a `DescMethod` carries `GenMessage` `input`/`output` schemas and the transport's `Serialization` calls `toBinary`/`fromBinary` on them INTERNALLY, so `createClient` consumes a `DescService` while request inputs are `MessageInitShape<I>` (`create`-shaped) and responses `MessageShape<O>` (`fromBinary`-decoded). The RPC path never decodes proto through a `interchange/format` page — direct-decode of the descriptor-typed families is the `codec` altitude, the serialize/deserialize on an invocation is transport-owned. `interchange/invoke` binds the emitted service and `interchange/contract` diffs the same `FileDescriptorSet` via `createFileRegistry` — one descriptor source, client and drift gate cannot diverge.
- Stack with `effect` (`.api/effect.md`): each unary method (`Promise`) lifts through `Effect.tryPromise({ try, catch: (e) => ConnectError.from(e) })`; each server-streaming method (`AsyncIterable`) folds through `Stream.fromAsyncIterable`; the typed client is wrapped once at `interchange/invoke` so no domain code sees a bare `Promise`.
- Stack with `value/fault` + `effect` `Schedule` (`.api/effect.md`): `Effect.retry(Schedule)` — the schedule compiled from `value/fault` budget rows — retries only on retryable `Code` (`Unavailable`/`DeadlineExceeded`/`Aborted`/`ResourceExhausted`), discriminated over `ConnectError.code` by `Match.exhaustive`; `CallOptions.timeoutMs` and `createDeadlineSignal` carry the per-call deadline.
- Stack with `effect` interruption: `CallOptions.signal` is the `AbortSignal` from the running fiber, so an `Effect` scope close or race loss aborts the in-flight RPC with `Code.Canceled`; `wire` never manages the controller by hand.
- Stack with `@effect/opentelemetry`: an `Interceptor` reads the active span via `Tracer.currentOtelSpan` and writes `traceparent` into `req.header` on egress (and continues the returned trailer span); `ContextValues` carries the tenant/HLC the interceptor annotates — W3C propagation without a call-site change.
- Stack with `@effect/platform` `HttpClient` (`.api/effect-platform.md`): the `./protocol` toolkit (`createMethodUrl`, `Serialization`, `runUnaryCall`, the envelope transforms, `createDeadlineSignal`) assembles a `Transport` whose body is an `Effect` over the shared `host/net` `HttpClient` policy — the dense path where the transport inherits the net-client retry/proxy/tracing posture instead of a bare `fetch`.

[LOCAL_ADMISSION]:
- construct the client once via `createClient(emittedService, transport)`; the `service` is always the codegen `DescService`, never a hand-written method map.
- wrap the client at `interchange/invoke` in `Effect.tryPromise`/`Stream.fromAsyncIterable`; a bare `Promise`/`AsyncIterable` or a raw `try`/`catch` around a call in domain code is the leak defect.
- fold every `ConnectError` through `interchange/codec`'s `fromConnect`; `Code` is matched exhaustively for retryability and `HopReason`, never inspected by an ad-hoc `if`.
- the server-side router (`createConnectRouter`/`createHandlerContext`/`ServiceImpl`) is out of `wire`'s role — only `createRouterTransport` is admitted, and only inside kit-driven specs.

[RAIL_LAW]:
- Package: `@connectrpc/connect`
- Owns: the `Transport` protocol port, `createClient`/`Client<T>` descriptor-derived typed client (plus the callback/any variants), `CallOptions`, the `Interceptor` onion, the `ConnectError`/`Code` fault algebra with `from`/`findDetails`, `ContextValues`, the `-bin` header codec, and the `./protocol` transport-construction toolkit
- Accept: `createClient` over a `connect-web` `Transport` and an emitted `DescService`, client methods lifted through `Effect.tryPromise`/`Stream.fromAsyncIterable`, `ConnectError` folded through `interchange/codec`, retry via `Effect.retry(Schedule)` gated on retryable `Code`, `CallOptions.signal` from Effect interruption, trace propagation via an `Interceptor`, a custom Effect-native `Transport` from the `./protocol` kit
- Reject: a hand-written client method map beside the emitted descriptor, a bare `Promise`/`AsyncIterable` or raw `try`/`catch` in domain code, ad-hoc `Code` inspection outside the `fromConnect` fold, the server-side router surface anywhere in `wire` except `createRouterTransport` in kit-driven specs
