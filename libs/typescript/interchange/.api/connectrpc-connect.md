# [API_CATALOGUE] @connectrpc/connect

`@connectrpc/connect` supplies the protocol-agnostic client and server primitives for Connect, gRPC, and gRPC-web: the `Transport` contract, typed `Client<T>` generation via `createClient`, interceptor pipeline, `ConnectError` with `Code`, context value propagation, and the server-side `ConnectRouter` registration surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect`
- package: `@connectrpc/connect`
- module: `@connectrpc/connect` (barrel); deep imports under `@connectrpc/connect/protocol`, `@connectrpc/connect/protocol-connect`, `@connectrpc/connect/protocol-grpc`, `@connectrpc/connect/protocol-grpc-web`
- asset: runtime library
- rail: transport

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and transport
- rail: transport

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                              |
| :-----: | :-------------- | :---------------- | :---------------------------------- |
|   [1]   | `Transport`     | protocol contract | unary and stream RPC dispatch       |
|   [2]   | `Client<T>`     | typed client map  | promise/async-iterable RPC methods  |
|   [3]   | `CallOptions`   | call options      | per-call timeout, headers, signal   |
|   [4]   | `Interceptor`   | middleware type   | request/response wrapping           |
|   [5]   | `ContextKey<T>` | context key       | typed per-call context slot         |
|   [6]   | `ContextValues` | context bag       | get/set/delete typed context values |

[PUBLIC_TYPE_SCOPE]: interceptor envelopes
- rail: transport

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [RAIL]                                 |
| :-----: | :--------------- | :---------------- | :------------------------------------- |
|   [1]   | `UnaryRequest`   | request envelope  | single input message, `stream: false`  |
|   [2]   | `UnaryResponse`  | response envelope | single output message, `stream: false` |
|   [3]   | `StreamRequest`  | request envelope  | async-iterable input, `stream: true`   |
|   [4]   | `StreamResponse` | response envelope | async-iterable output, `stream: true`  |

[PUBLIC_TYPE_SCOPE]: error and codes
- rail: transport

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :------------- | :------------ | :---------------------------------------- |
|   [1]   | `ConnectError` | typed error   | `code`, `metadata`, `rawMessage`, details |
|   [2]   | `Code`         | numeric enum  | 16 standard RPC error codes               |

[PUBLIC_TYPE_SCOPE]: server-side routing
- rail: transport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [RAIL]                            |
| :-----: | :--------------------- | :------------------ | :-------------------------------- |
|   [1]   | `ConnectRouter`        | router interface    | service and RPC registration      |
|   [2]   | `ConnectRouterOptions` | router config       | grpc / grpcWeb / connect toggles  |
|   [3]   | `HandlerContext`       | handler context     | per-RPC signal, headers, trailers |
|   [4]   | `ServiceImpl<T>`       | impl type map       | service method implementations    |
|   [5]   | `MethodImpl<M>`        | impl type           | single-method implementation      |
|   [6]   | `MethodImplSpec`       | discriminated union | kind-tagged impl + descriptor     |
|   [7]   | `ServiceImplSpec`      | impl wrapper        | service + named method impls      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: transport

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `createClient<T>(service, transport)`   | client factory | typed `Client<T>` from a `DescService`    |
|   [2]   | `createUnaryFn(transport, method)`      | fn factory     | standalone unary method handle            |
|   [3]   | `createServerStreamingFn(transport, m)` | fn factory     | standalone server-streaming method handle |
|   [4]   | `createClientStreamingFn(transport, m)` | fn factory     | standalone client-streaming method handle |
|   [5]   | `createBiDiStreamingFn(transport, m)`   | fn factory     | standalone bidi-streaming method handle   |

[ENTRYPOINT_SCOPE]: context values
- rail: transport

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `createContextKey<T>(defaultValue, options?)` | key factory    | typed context slot key         |
|   [2]   | `createContextValues()`                       | bag factory    | empty `ContextValues` instance |

[ENTRYPOINT_SCOPE]: error construction and inspection
- rail: transport

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]    | [RAIL]                                  |
| :-----: | :----------------------------------------- | :---------------- | :-------------------------------------- |
|   [1]   | `new ConnectError(msg, code?, meta?, ...)` | error constructor | typed RPC error with code and metadata  |
|   [2]   | `ConnectError.from(reason, code?)`         | static converter  | coerce any caught value to ConnectError |
|   [3]   | `error.findDetails(desc\|registry)`        | detail accessor   | decode typed error detail messages      |

[ENTRYPOINT_SCOPE]: interceptors and router
- rail: transport

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]    | [RAIL]                             |
| :-----: | :-------------------------------------- | :---------------- | :--------------------------------- |
|   [1]   | `applyInterceptors(next, interceptors)` | interceptor chain | wraps a call fn with interceptors  |
|   [2]   | `createConnectRouter(options?)`         | router factory    | server-side service registration   |
|   [3]   | `createHandlerContext(init)`            | context factory   | unit-test handler context creation |
|   [4]   | `createMethodImplSpec(method, impl)`    | spec factory      | kind-tagged method implementation  |
|   [5]   | `createServiceImplSpec(service, impl)`  | spec factory      | service implementation wrapper     |

[ENTRYPOINT_SCOPE]: utility exports
- rail: transport

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `encodeBinaryHeader(bytes)`               | header util    | base64-encode binary header value  |
|   [2]   | `decodeBinaryHeader(value)`               | header util    | base64-decode binary header value  |
|   [3]   | `appendHeaders(target, init)`             | header util    | merge `HeadersInit` into `Headers` |
|   [4]   | `cors`                                    | CORS config    | CORS option constants object       |
|   [5]   | `makeAnyClient<T>(service, transport)`    | any-client     | untyped client for reflection use  |
|   [6]   | `createRouterTransport(routes, options?)` | test transport | in-memory transport from a router  |

## [4]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- `Transport` owns two methods: `unary` and `stream`; all client code composes through this interface
- `Client<T>` is a mapped type: unary methods become `(req, opts?) => Promise<O>`, server-streaming become `(req, opts?) => AsyncIterable<O>`, client-streaming become `(req: AsyncIterable<I>, opts?) => Promise<O>`, bidi-streaming become `(req: AsyncIterable<I>, opts?) => AsyncIterable<O>`
- `Interceptor` is `(next: AnyFn) => AnyFn`; interceptors in the array are applied last-to-first so the last entry wraps outermost
- `ConnectRouter.service` accepts a `Partial<ServiceImpl<T>>`; missing methods are registered as `unimplemented`
- `Code` is a numeric enum with 16 values matching gRPC status codes; `Code.Unknown = 2` is the default when no code is supplied to `ConnectError`

[LOCAL_ADMISSION]:
- `Transport` implementations live in `@connectrpc/connect-web` (browser) or `@connectrpc/connect-node` (server); this package only defines the contract.
- `ContextKey<T>` is the canonical per-call context slot; pass `ContextValues` via `CallOptions.contextValues` or set them inside an interceptor's `req.contextValues`.
- `ConnectError` must not be constructed by domain logic; only boundary adapters convert domain errors to `ConnectError` before returning across the RPC boundary.

[RAIL_LAW]:
- Package: `@connectrpc/connect`
- Owns: protocol-agnostic client/server contract, interceptor pipeline, error model, context propagation, server routing
- Accept: `Transport` implementations from sibling packages; service descriptors from `@bufbuild/protobuf`
- Reject: direct HTTP fetch logic (belongs in a transport implementation), hand-rolled base64 header encoding (use `encodeBinaryHeader`/`decodeBinaryHeader`)
