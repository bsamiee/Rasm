# [API_CATALOGUE] @connectrpc/connect-web

`@connectrpc/connect-web` supplies the two browser-native `Transport` implementations — `createConnectTransport` and `createGrpcWebTransport` — both Fetch-API-backed, carrying `baseUrl`, wire-format selection, an interceptor chain, per-transport timeout floor, and a `fetch` override for credentials/mocking. It is the concrete edge of the `transport.md` protocol-selection axis: one polymorphic transport factory keyed on protocol, never two parallel dial paths, feeding `@connectrpc/connect` `createClient` for every generated service.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@connectrpc/connect-web`
- package: `@connectrpc/connect-web` (2.1.2, Apache-2.0, © The Connect Authors)
- module format: dual ESM (`dist/esm`) + CJS (`dist/cjs`), `type: module`; single barrel export `.` re-exporting `createConnectTransport`/`createGrpcWebTransport` + the two option types — no deep subpaths
- runtime target: browser only (`globalThis.fetch` + `ReadableStream`); peer-depends `@bufbuild/protobuf` (descriptor/codec knobs) + `@connectrpc/connect` (`Transport`/`Interceptor`/`ContextValues` contract); no Node HTTP/2 (`@connectrpc/connect-node` owns that)
- asset: pure-TypeScript runtime library shipping `.js` + `.d.ts`; both factories return the `Transport` interface from `@connectrpc/connect`, so the concrete transport type never crosses the composition root
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport option shapes
- rail: transport

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [BOUNDARY_NOTE]                                               |
| :-----: | :------------------------ | :---------------- | :----------------------------------------------------------- |
|  [01]   | `ConnectTransportOptions` | options interface | Connect protocol; adds `useHttpGet` over the shared fields   |
|  [02]   | `GrpcWebTransportOptions` | options interface | gRPC-web protocol; shared fields, no `useHttpGet`            |

[PUBLIC_TYPE_SCOPE]: shared option fields (both interfaces)
- rail: transport

| [INDEX] | [FIELD]              | [TYPE]                                            | [BOUNDARY_NOTE]                                        |
| :-----: | :------------------- | :------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | `baseUrl`            | `string` (required)                               | requests target `<baseUrl>/<pkg>.<Service>/<Method>`  |
|  [02]   | `useBinaryFormat`    | `boolean?`                                        | Connect default `false` (JSON); gRPC-web default `true` (binary) |
|  [03]   | `interceptors`       | `Interceptor[]?`                                  | `@connectrpc/connect` chain, applied last-to-first    |
|  [04]   | `jsonOptions`        | `Partial<JsonReadOptions & JsonWriteOptions>?`    | `@bufbuild/protobuf` JSON codec knobs (`registry` for `Any`) |
|  [05]   | `binaryOptions`      | `Partial<BinaryReadOptions & BinaryWriteOptions>?` | `@bufbuild/protobuf` binary codec knobs               |
|  [06]   | `fetch`              | `typeof globalThis.fetch?`                        | override injecting `credentials`, headers, or a test mock |
|  [07]   | `defaultTimeoutMs`   | `number?`                                         | per-transport floor; `CallOptions.timeoutMs` overrides per call |
|  [08]   | `useHttpGet`         | `boolean?` (Connect only)                         | Connect GET for side-effect-free unary; enables edge caching |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport factories
- rail: transport

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]    | [BOUNDARY_NOTE]                                     |
| :-----: | :-------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `createConnectTransport(options)` | transport factory | `Transport` over the Connect protocol; unary + server-stream over fetch |
|  [02]   | `createGrpcWebTransport(options)` | transport factory | `Transport` over gRPC-web; binary framing over fetch, backend-dictated |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- `ConnectTransportOptions.baseUrl` is required; all requests target `<baseUrl>/<pkg>.<Service>/<Method>`.
- `useBinaryFormat` defaults `false` on Connect (JSON wire) and `true` on gRPC-web (not all gRPC-web servers accept JSON); `useHttpGet` (Connect only) defaults `false` and enables GET only for side-effect-free unary methods.
- gRPC-web transport does not implement `grpc-web-text` (base64 over XHR); it uses binary framing over fetch, and browser gRPC-web carries server-streaming but not client/bidi (no full-duplex over fetch), so a bidi leg pins the Connect transport.
- `defaultTimeoutMs` is the per-transport deadline floor a long stream's `CallOptions.signal` composes with; `CallOptions.timeoutMs` overrides per call.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Transport/transport.md` binds ONE `WireTransportLive` `Effect.Service` whose factory switches on a protocol-selection value — `createConnectTransport` for the Connect default (JSON or binary, server-streams over fetch, `useHttpGet` idempotent edge caching) and `createGrpcWebTransport` only where the backend dictates gRPC-web framing — so call sites hold `Transport` and the concrete factory never leaks past the composition root, the polymorphic dial rather than two parallel client stacks; a Connect server-stream leg folds into `Stream.fromAsyncIterable(client.method(req), faultDetailRail.fromConnect)` over the client's `AsyncIterable<O>`, and a rejected dial crosses `Effect.tryPromise` onto the typed `E` channel rather than throwing across the fold
- `@connectrpc/connect` (`.api/connectrpc-connect.md`): both factories return the `Transport` `createClient(<Service>Schema, transport)` composes — one client per generated `GenService`; the `interceptors` option threads the `Interceptor` chain (applied last-to-first, first entry outermost) that stamps correlation/`traceparent`/bearer onto every `req.header` and reads `ContextValues`, and `binaryOptions`-encoded trailer bytes pair with `encodeBinaryHeader`/`decodeBinaryHeader` at the metadata edge; a rejected call surfaces as `ConnectError` that `Ingress/fault.md` runs `ConnectError.from(reason)` then `error.findDetails(registry)` over, rebuilding the `Data.TaggedEnum` fault family through `Match.tagsExhaustive` — never a bare throw across the fold
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): `jsonOptions`/`binaryOptions` thread the codec knobs (`registry` for `Any`/extension round-trips, `writeUnknownFields` for additive tolerance) straight into the transport, so decode posture is set once at dial time, never per call

[LOCAL_ADMISSION]:
- Both factories return `Transport` from `@connectrpc/connect`; call sites hold only `Transport`, never the concrete type.
- `defaultTimeoutMs` is the per-transport floor; `CallOptions.timeoutMs` overrides per request.
- `jsonOptions`/`binaryOptions` accept partial `@bufbuild/protobuf` codec options; set `registry` there when the wire carries `Any` or extensions.

[RAIL_LAW]:
- Package: `@connectrpc/connect-web`
- Owns: browser `Transport` implementations for the Connect and gRPC-web protocols
- Accept: descriptors and codec options from `@bufbuild/protobuf`; interceptors and `ContextValues` from `@connectrpc/connect`
- Reject: Node-only transports (use `@connectrpc/connect-node`), gRPC/H2 cleartext, a second transport per service, concrete-type leakage past the composition root
