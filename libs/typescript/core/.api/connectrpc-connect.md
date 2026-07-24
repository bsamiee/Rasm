# [TS_CORE_API_CONNECTRPC_CONNECT]

`@connectrpc/connect` owns the protocol-neutral RPC invocation surface `interchange/invoke` binds the emitted capability SDK onto: `createClient(service, transport)` projects a `@bufbuild/protobuf` `DescService` into a typed `Client<T>` the type system derives, over a `Transport` any protocol supplies.

`ConnectError`/`Code` is the `interchange/codec` fold source, the `Interceptor` onion the cross-cutting seam, and `./protocol` the kit for an Effect-native `Transport`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect`
- package: `@connectrpc/connect` (Apache-2.0)
- peer: `@bufbuild/protobuf` — `DescService`/`DescMethod`/`MessageInitShape`/`MessageShape`/`create`/`fromBinary`/`toBinary` (`.api/bufbuild-protobuf.md`)
- effect-peer: none direct — client `Promise`/`AsyncIterable` cross into `Effect.tryPromise`/`Stream.fromAsyncIterable` at `interchange/invoke` (`.api/effect.md`)
- runtime: isomorphic, `sideEffects:false`; transport-agnostic — `connect-web` fetch or any `Transport`
- modules: `.` (client, error, interceptor, context), `./protocol` (transport-construction kit), `./protocol-connect`, `./protocol-grpc`, `./protocol-grpc-web`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transport, the descriptor-derived client, and per-call options
- rail: interchange/invoke
- `Client<Desc>` method shapes derive from the descriptor — unary `Promise`, streaming `AsyncIterable<MessageShape<...>>` bridging directly to `effect` `Stream`; `CallOptions` threads `signal`/`timeoutMs`/`contextValues` per call.

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
- `ConnectError` (`code`/`metadata`/`details`/`rawMessage`/`cause`) is the one transport fault; `Code` is the closed 16-value gRPC-aligned enum folding to the `HopReason` vocabulary, and `details` are `Any`-wrapped messages `findDetails` decodes.

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
- `createClient(service, transport)` is the one factory — always the codegen `DescService` over a `connect-web` `Transport`, never a hand-written method map.

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

[ENTRYPOINT_SCOPE]: the `./protocol` kit — a custom Effect-native transport
- rail: interchange/invoke
- `./protocol` assembles a fully `Effect`-owned `Transport` over `@effect/platform` `HttpClient` instead of the fetch-bound `connect-web` factories, inheriting the `host/net` client policy.

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

[TOPOLOGY]:
- transport and client are orthogonal: `Transport` implements the protocol, `createClient` is generic over it, so protocol selection is a `Transport` choice and `Client<T>` holds one shape across every arm.
- `createClient` derives every method signature from the emitted `DescService`, so the SDK is one descriptor and a hand-authored client method is the drift defect.
- `ConnectError` carries the fault: a failed call rejects with `code`/`metadata`/`details`, which `interchange/codec` folds to `HopReason` — the wire fault altitude, distinct from any local `Data.TaggedError`.
- interception is the cross-cutting seam: an `Interceptor` onion attaches trace propagation, auth, and per-call `ContextValues`, never the call site.

[STACKING]:
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): the message runtime under Connect — `createClient` consumes a `DescService`, request inputs are `MessageInitShape<I>` and responses `MessageShape<O>`, and the transport's `Serialization` calls `toBinary`/`fromBinary` internally; `interchange/contract` diffs the same `FileDescriptorSet` via `createFileRegistry`, so client and drift gate share one descriptor source.
- `effect` (`.api/effect.md`): each unary `Promise` lifts through `Effect.tryPromise({ try, catch: ConnectError.from })` and each streaming `AsyncIterable` folds through `Stream.fromAsyncIterable`, wrapped once at `interchange/invoke` so no domain code sees a bare `Promise`.
- `value/fault` + `effect` `Schedule` (`.api/effect.md`): `Effect.retry(Schedule)` retries only on retryable `Code` (`Unavailable`/`DeadlineExceeded`/`Aborted`/`ResourceExhausted`), discriminated over `ConnectError.code` by `Match.exhaustive`; `CallOptions.timeoutMs` and `createDeadlineSignal` carry the per-call deadline.
- `effect` interruption: `CallOptions.signal` is the running fiber's `AbortSignal`, so a scope close or race loss aborts the in-flight RPC with `Code.Canceled`.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): an `Interceptor` reads the active span via `Tracer.currentOtelSpan` and writes `traceparent` into `req.header` on egress, `ContextValues` carrying the tenant/HLC it annotates — W3C propagation without a call-site change.
- `@effect/platform` `HttpClient` (`.api/effect-platform.md`): the `./protocol` kit assembles a `Transport` whose body is an `Effect` over the shared `host/net` `HttpClient`, inheriting its retry/proxy/tracing posture instead of a bare `fetch`.

[RAIL_LAW]:
- Package: `@connectrpc/connect`
- Owns: the `Transport` port, `createClient`/`Client<T>` and the callback/any client variants, `CallOptions`, the `Interceptor` onion, the `ConnectError`/`Code` fault algebra with `from`/`findDetails`, `ContextValues`, the `-bin` header codec, and the `./protocol` transport-construction kit
- Accept: `createClient` over a `connect-web` `Transport` and an emitted `DescService`; client methods lifted through `Effect.tryPromise`/`Stream.fromAsyncIterable`; `ConnectError` folded through `interchange/codec`; retry via `Effect.retry(Schedule)` gated on retryable `Code`; `CallOptions.signal` from Effect interruption; trace propagation via an `Interceptor`; a custom Effect-native `Transport` from `./protocol`
- Reject: a hand-written client method map beside the emitted descriptor, a bare `Promise`/`AsyncIterable` or raw `try`/`catch` in domain code, ad-hoc `Code` inspection outside the `fromConnect` fold, the server-side router surface anywhere in `wire` except `createRouterTransport` in kit-driven specs
