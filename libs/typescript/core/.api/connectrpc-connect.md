# [TS_CORE_API_CONNECTRPC_CONNECT]

`@connectrpc/connect` owns the protocol-neutral RPC invocation surface `interchange/invoke` binds the emitted capability SDK onto: `createClient(service, transport)` projects a `@bufbuild/protobuf` `DescService` into a typed `Client<T>` the type system derives, over a `Transport` any protocol supplies.

`ConnectError`/`Code` is the `interchange/codec` fold source and the `Interceptor` onion the cross-cutting layer. Protocol construction stays with the public web and Node adapter packages.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transport, the descriptor-derived client, and per-call options
- concern: interchange/invoke
- `Client<Desc>` method shapes derive from the descriptor — unary `Promise`, streaming `AsyncIterable<MessageShape<...>>` bridging directly to `effect` `Stream`; `CallOptions` threads `signal`/`timeoutMs`/`contextValues` per call and carries NO retry knob, so re-drive stays the caller's schedule.
- Requests carry `service`/`method`/`requestMethod`/`url`/`signal`/`header`/`contextValues`; RESPONSES carry `service`/`header`/`trailer` alone — an interceptor reading `signal` or `contextValues` off a response finds neither, and `trailer` fills only once the response iterable is fully drained.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                             |
| :-----: | :------------------------------------------------ | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `Transport` (`.unary`, `.stream`)                 | protocol port    | from the selected public web or Node adapter factory            |
|  [02]   | `Client<Desc extends DescService>`                | typed client     | unary→`Promise`, stream→`AsyncIterable`; from descriptor        |
|  [03]   | `CallOptions`                                     | call knobs       | `signal` binds interruption; `timeoutMs`≤0 disables the default |
|  [04]   | `CallbackClient<Desc>` / `AnyClient`              | client variant   | callback + dynamic flavors; `wire` uses the promise `Client`    |
|  [05]   | `Interceptor` = `(next) => (req) => Promise<res>` | middleware       | the layered onion around a call — trace, auth, logging          |
|  [06]   | `UnaryRequest` / `UnaryResponse`                  | interceptor io   | `stream:false` arm — `message`/`method`/`header`/`signal`       |
|  [07]   | `StreamRequest` / `StreamResponse`                | interceptor io   | `stream:true` arm — `message`, `trailer`, `contextValues`       |
|  [08]   | `ContextValues` / `ContextKey<T>`                 | per-call context | tenant, deadline, HLC through interceptors without global state |
|  [09]   | `Registry` (peer) on `findDetails`                | detail registry  | the generated file registry an incoming detail decodes against  |

[PUBLIC_TYPE_SCOPE]: the fault algebra `interchange/codec` folds
- concern: interchange/codec
- `ConnectError` (`code`/`metadata`/`details`/`rawMessage`/`cause`) is the one transport fault; `Code` is the closed 16-value gRPC-aligned enum `interchange/codec`'s `Wire.Hops` keys its ONE row table on, and `details` are `Any`-wrapped messages `findDetails(registry)` decodes against `interchange/format`'s registry.

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                               |
| :-----: | :---------------------------------------------------------------- | :--------------- | :------------------------------------------------ |
|  [01]   | `ConnectError`                                                    | transport fault  | `fromConnect` fold source; at `Effect.tryPromise` |
|  [02]   | `Code` (`Canceled`…`Unauthenticated`, 1–16)                       | closed code enum | `Wire.Hops` key; `satisfies Record<Code, Row>`    |
|  [03]   | `ConnectError.from(reason, code?)`                                | normalizer       | any reason → `ConnectError` (Abort→Canceled)      |
|  [04]   | `ConnectError.findDetails(desc \| registry)`                      | detail decode    | `Any`-wrapped error details → typed messages      |
|  [05]   | `ServiceImpl` / `MethodImpl` / `HandlerContext` / `ConnectRouter` | server-side      | OUT of `wire`'s client role; import is the defect |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the client and the invocation boundary
- concern: interchange/invoke
- `createClient(service, transport)` is the one client factory — always the codegen `DescService` over a selected public adapter `Transport`, never a hand-written method map.
- `createUnaryFn` and `createServerStreamingFn` are internal helpers absent from the root barrel and package export map.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Deadlines propagate on the wire, not just locally: `timeoutMs` builds a deadline signal that aborts with `Code.DeadlineExceeded` AND writes a header — `Connect-Timeout-Ms` carrying bare milliseconds on Connect, `Grpc-Timeout` carrying milliseconds with an `m` unit suffix on gRPC and gRPC-Web. Undefined timeouts send no header at all, and `defaultTimeoutMs` itself defaults to undefined, so an unset deadline crosses as an unbounded call.
- `ConnectError.from` maps an `AbortError` or `TimeoutError` to `Code.Canceled` and DROPS the cause, so an interruption's origin survives only where the fold captures it before normalizing.
- `ConnectError[Symbol.hasInstance]` is structural, so `instanceof` holds across duplicate package copies and no fold needs a nominal identity check.
- transport and client are orthogonal: `Transport` implements the protocol, `createClient` is generic over it, so protocol selection is a `Transport` choice and `Client<T>` holds one shape across every arm.
- `createClient` derives every method signature from the emitted `DescService`, so the SDK is one descriptor and a hand-authored client method is the drift defect.
- Core binds typed calls through public `createClient`; internal client-function factories are neither imports nor transcription sources.
- `ConnectError` carries the fault: a failed call rejects with `code`/`metadata`/`details`, which `interchange/invoke` folds to one of three outcomes by trailer shape — `Remote` off the generated `FaultDetail`, `Transport` off the `Wire.Hops` row for the code, `MalformedDetail` — the wire fault altitude, distinct from any local `Data.TaggedError`.
- interception is the cross-cutting layer: an `Interceptor` onion attaches trace propagation, auth, and per-call `ContextValues`, never the call site.

[STACKING]:
- `@bufbuild/protobuf` (`../../.api/bufbuild-protobuf.md`): Connect consumes generated descriptors and delegates message codecs to the shared runtime.
- `effect` (`.api/effect.md`): each unary `Promise` lifts through `Effect.tryPromise({ try, catch: ConnectError.from })` and each streaming `AsyncIterable` folds through `Stream.fromAsyncIterable`, wrapped once at `interchange/invoke` so no domain code sees a bare `Promise`.
- `value/fault` + `effect` `Schedule` (`.api/effect.md`): the dial's retry gate reads `Fault.Class.retryable` off the `Wire.Hops` row for `ConnectError.code` — one table, never a per-page `Match` over codes; `CallOptions.timeoutMs` carries the per-call deadline.
- `effect` interruption: `CallOptions.signal` is the running fiber's `AbortSignal`, so a scope close or race loss aborts the in-flight RPC with `Code.Canceled`.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): an `Interceptor` reads the active span via `Tracer.currentOtelSpan` and writes `traceparent` into `req.header` on egress, `ContextValues` carrying the tenant/HLC it annotates — W3C propagation without a call-site change.
- `@connectrpc/connect-web` and `@connectrpc/connect-node`: their public factory records realize the supported adapter pairs; this package consumes the selected `Transport` without knowing its host.
