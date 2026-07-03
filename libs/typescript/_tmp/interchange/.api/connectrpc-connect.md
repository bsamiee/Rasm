# [API_CATALOGUE] @connectrpc/connect

`@connectrpc/connect` supplies the protocol-agnostic client and server CONTRACT for Connect, gRPC, and gRPC-web — it ships no `Transport`, no fetch, no HTTP. It owns the `Transport` interface (two methods: `unary`, `stream`), the typed `Client<T>` mapped-type projection built by `createClient`, the interceptor pipeline, the `ConnectError`/`Code` error model with wire-detail decode, typed per-call context propagation, and the server-side `ConnectRouter` registration surface. The interchange branch consumes only the CLIENT half over the `@connectrpc/connect-web` transports; the server-routing surface is catalogued for completeness and never dialed browser-side.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect`
- package: `@connectrpc/connect` (2.1.2, Apache-2.0, © The Connect Authors)
- module format: dual ESM (`./dist/esm`) + CJS (`./dist/cjs`), `type: module`; barrel export `.` plus four protocol-implementer deep subpaths `./protocol`, `./protocol-connect`, `./protocol-grpc`, `./protocol-grpc-web`
- runtime target: isomorphic (browser, node, worker); protocol-agnostic contract only — a concrete `Transport` lands from a sibling package, never here; peer-depends `@bufbuild/protobuf` for the descriptor runtime and `findDetails` decode
- asset: pure-TypeScript runtime library; the pnpm store entry materializes `.js` but the `.d.ts` is a content-less stub on disk, so member spellings are grounded against the shipped `.js` runtime exports and the connect-es v2 source, re-confirmed against the emitted `.d.ts` at transcription-lock
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and transport
- rail: transport

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [RAIL]                                              |
| :-----: | :----------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `Transport`        | protocol contract | `unary`/`stream` RPC dispatch; the one composed-through interface |
|  [02]   | `Client<T>`        | typed client map  | mapped type: method-kind → promise/async-iterable RPC face |
|  [03]   | `CallbackClient<T>`| typed client map  | callback-style mirror of `Client<T>`; unary + server-stream only |
|  [04]   | `CallOptions`      | call options      | `signal`, `timeoutMs`, `headers`, `contextValues`, `onHeader`, `onTrailer` |
|  [05]   | `Interceptor`      | middleware type   | `(next: AnyFn) => AnyFn` request/response wrapping |
|  [06]   | `ContextKey<T>`    | context key       | `{ id: symbol; defaultValue: T }` typed per-call slot |
|  [07]   | `ContextValues`    | context bag       | `get`/`set`/`delete` typed context values          |

[PUBLIC_TYPE_SCOPE]: interceptor envelopes
- rail: transport

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [RAIL]                                 |
| :-----: | :--------------- | :---------------- | :------------------------------------- |
|  [01]   | `UnaryRequest`   | request envelope  | single input message, `stream: false`  |
|  [02]   | `UnaryResponse`  | response envelope | single output message, `stream: false` |
|  [03]   | `StreamRequest`  | request envelope  | async-iterable input, `stream: true`   |
|  [04]   | `StreamResponse` | response envelope | async-iterable output, `stream: true`  |

[PUBLIC_TYPE_SCOPE]: error and codes
- rail: transport

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [RAIL]                                                                       |
| :-----: | :------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `ConnectError` | typed error   | extends `Error`; `code`, `metadata: Headers`, `rawMessage`, `details`, `cause` |
|  [02]   | `Code`         | numeric enum  | 16 gRPC status codes `1..16` (`Canceled=1`…`Unauthenticated=16`); NO `0`/`OK` |

[PUBLIC_TYPE_SCOPE]: server-side routing (Node handler surface; not browser-consumed)
- rail: transport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [RAIL]                            |
| :-----: | :--------------------- | :------------------ | :-------------------------------- |
|  [01]   | `ConnectRouter`        | router interface    | service and RPC registration      |
|  [02]   | `ConnectRouterOptions` | router config       | grpc / grpcWeb / connect toggles  |
|  [03]   | `HandlerContext`       | handler context     | per-RPC signal, headers, trailers |
|  [04]   | `ServiceImpl<T>`       | impl type map       | service method implementations    |
|  [05]   | `MethodImpl<M>`        | impl type           | single-method implementation      |
|  [06]   | `MethodImplSpec`       | discriminated union | kind-tagged impl + descriptor     |
|  [07]   | `ServiceImplSpec`      | impl wrapper        | service + named method impls      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction — one polymorphic factory over the method-kind axis
- rail: transport

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                                                     |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `createClient<T>(service, transport)`  | client factory | typed `Client<T>` from a `DescService`; ONE factory dispatching every method kind internally |
|  [02]   | `createCallbackClient<T>(service, tr)` | client factory | callback-style `CallbackClient<T>`; internally supports unary + server-stream only |
|  [03]   | `makeAnyClient<T>(service, createFn)`  | any-client     | untyped client for reflection use; the shared base `createClient` builds on |

[ENTRYPOINT_SCOPE]: context values
- rail: transport

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `createContextKey<T>(defaultValue, options?)` | key factory    | `{ id: Symbol(options?.description), defaultValue }` |
|  [02]   | `createContextValues()`                       | bag factory    | empty `ContextValues` bag                    |

[ENTRYPOINT_SCOPE]: error construction and inspection
- rail: transport

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]    | [RAIL]                                                       |
| :-----: | :----------------------------------------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `new ConnectError(message, code?, metadata?, outgoing?, cause?)` | error constructor | typed RPC error; `code` defaults `Code.Unknown`; `.message` is `[code] message` |
|  [02]   | `ConnectError.from(reason, code?)`                           | static converter  | coerce any caught value; fetch `AbortError`/`TimeoutError` → `Code.Canceled` |
|  [03]   | `error.findDetails(desc \| registry)`                        | detail accessor   | decode typed error-detail messages from the `grpc-status-details-bin` trailer |

[ENTRYPOINT_SCOPE]: interceptors, router, and header/CORS utilities
- rail: transport

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :---------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `createConnectRouter(options?)`           | router factory  | server-side service registration (Node)       |
|  [02]   | `createHandlerContext(init)`              | context factory | unit-test handler context creation            |
|  [03]   | `createMethodImplSpec(method, impl)`      | spec factory    | kind-tagged method implementation             |
|  [04]   | `createServiceImplSpec(service, impl)`    | spec factory    | service implementation wrapper                |
|  [05]   | `createRouterTransport(routes, options?)` | test transport  | in-memory `Transport` from a router (test seam) |
|  [06]   | `encodeBinaryHeader(value, desc?)`        | header util     | base64-encode a message (with `desc`), string, or bytes as a `-bin` header |
|  [07]   | `decodeBinaryHeader(value, desc?)`        | header util     | base64-decode a binary header value           |
|  [08]   | `appendHeaders(...headers)`               | header util     | merge `HeadersInit` sources into one `Headers` |
|  [09]   | `cors`                                    | CORS config     | preflight method/header constant object for the three protocols |

[ENTRYPOINT_SCOPE]: protocol-implementer tier (deep subpaths; not consumed browser-side)
- rail: transport

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [RAIL]                                                    |
| :-----: | :--------------------------------------------------------------- | :--------------- | :------------------------------------------------------- |
|  [01]   | `@connectrpc/connect/protocol` — `createEnvelopeReadableStream`, `compressionNegotiate`, `createDeadlineSignal`, `createUniversalHandlerClient` | transport-build primitives | the framing/envelope/compression/signal kit a custom `Transport` is built from |
|  [02]   | `@connectrpc/connect/protocol-connect` — `createTransport`, `codeFromHttpStatus`, `codeToHttpStatus`, `trailerMux`/`trailerDemux` | Connect protocol impl | the Code↔HTTP-status map and trailer mux the finished `connect-web` transports own |
|  [03]   | `@connectrpc/connect/protocol-grpc`, `.../protocol-grpc-web`     | protocol impl    | the gRPC and gRPC-web wire framings the sibling transports embed |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- `Transport` owns exactly two methods, `unary(method, signal, timeoutMs, header, input, contextValues)` and `stream(...)`; every client method composes through one of the two, so a `Client<T>` is a pure projection of a `Transport` over a service descriptor
- `Client<T>` is a mapped type over `method.methodKind`: unary → `(req, opts?) => Promise<O>`, server-streaming → `(req, opts?) => AsyncIterable<O>`, client-streaming → `(req: AsyncIterable<I>, opts?) => Promise<O>`, bidi-streaming → `(req: AsyncIterable<I>, opts?) => AsyncIterable<O>`
- `createClient` is the ONE polymorphic factory: it wraps `makeAnyClient` and switches on `method.methodKind` to bind the four internal per-kind call fns — the per-kind fns are NOT public exports (no barrel re-export, no `exports`-map subpath); a call site holds only `createClient`, never a per-kind factory
- `Interceptor` is `(next: AnyFn) => AnyFn`; `applyInterceptors` folds the array right-to-left so the FIRST array entry wraps OUTERMOST and sees a request first, the LAST entry sits innermost against the call — interceptors run in array order
- `Code` is a numeric enum valued `1..16` (`Canceled=1`, `Unknown=2`, `InvalidArgument=3`, `DeadlineExceeded=4`, `NotFound=5`, `AlreadyExists=6`, `PermissionDenied=7`, `ResourceExhausted=8`, `FailedPrecondition=9`, `Aborted=10`, `OutOfRange=11`, `Unimplemented=12`, `Internal=13`, `Unavailable=14`, `DataLoss=15`, `Unauthenticated=16`); there is no `0`/`OK` because success is not an error, `Code.Unknown=2` is the default, and `Code[n]` reverse-maps a numeric code to its name
- `ConnectError` extends `Error`: `code: Code`, `metadata: Headers` (a Web `Headers`, read trailers via `.get()`), `rawMessage` (message without the `[code]` prefix), `details` (outgoing typed messages, server-side), `cause`, plus a cross-realm `Symbol.hasInstance`; `findDetails` accepts a `DescMessage` OR a `Registry` and silently drops undecodable details

[STACKS_WITH]:
- `@connectrpc/connect-web` (`.api/connectrpc-connect-web.md`): the concrete `Transport` this contract only declares — `createConnectTransport`/`createGrpcWebTransport` return the `Transport` `createClient` composes; `WireTransportLive` (`Transport/transport.md`) selects one by `TransportProtocol` and passes the `Interceptor` array through the factory's `interceptors` option, never `applyInterceptors` by hand
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): the `DescService`/`DescMethod` descriptors `createClient` keys on and the `create`/`fromBinary` runtime `findDetails` calls internally to decode `google.protobuf.Any`-wrapped error details against a `Registry` — `faultDetailRail.fromConnect` (`Ingress/fault.md`) passes the generated `FaultDetailSchema` descriptor to `findDetails`
- `effect` (`.api/effect.md`): every dial crosses `Effect.tryPromise({ try: () => client.method(req), catch: faultDetailRail.fromConnect })` so the `Promise`/throw boundary becomes a typed `FaultDetail` on the `E` channel; `ConnectError.from` + `findDetails` feed the `Data.TaggedEnum` fault family that `Match.tagsExhaustive` renders; the interceptor resolves the token + span in one `Runtime.runPromise(runtime)(stampEffect)` over a captured `Effect.runtime` snapshot; wire retry is composition-time `Effect.retry(call, { schedule: retrySchedule, while: retryableWire })` keyed on `Code.Unavailable`, since the transport factory exposes no retry knob; a server-stream leg lifts through `Stream.fromAsyncIterable(client.method(req), faultDetailRail.fromConnect)` with a one-frame `Stream.buffer` backpressure window
- gateway/verb dispatch (`Transport/gateway.md`): `clients.control[verb](payload)` is keyed property access off the closed `ControlVerb` domain over a `Client<typeof ControlService>`, never a `Match` chain restating one verb per arm

[LOCAL_ADMISSION]:
- `Transport` implementations live in `@connectrpc/connect-web` (browser) or `@connectrpc/connect-node` (server); this package only defines the contract, so never import a transport factory from here.
- `ContextKey<T>` is the canonical per-call context slot; pass `ContextValues` via `CallOptions.contextValues` or set it on `req.contextValues` inside an interceptor — never thread a bare config object.
- `ConnectError` must not be constructed by domain logic; only the boundary fold (`faultDetailRail.fromConnect`) converts a cause to a typed `FaultDetail`, and only a server boundary adapter raises a `ConnectError` with outgoing `details`.
- Read binary trailer metadata through `decodeBinaryHeader`; never hand-roll base64 header codec.

[RAIL_LAW]:
- Package: `@connectrpc/connect`
- Owns: the protocol-agnostic client/server contract, the `createClient` mapped-type projection, the interceptor pipeline, the `ConnectError`/`Code` error model with wire-detail decode, context propagation, and server routing
- Accept: `Transport` implementations from sibling packages; `DescService`/`DescMethod` descriptors from `@bufbuild/protobuf`; `Interceptor`s stamped over a captured `Effect.runtime`
- Reject: direct HTTP fetch logic (belongs in a transport implementation), the non-public per-kind `create*Fn`/`applyInterceptors` as call-site entrypoints (use `createClient` + the transport `interceptors` option), hand-rolled base64 header encoding, a `Match` chain over a verb literal where keyed client access dispatches
