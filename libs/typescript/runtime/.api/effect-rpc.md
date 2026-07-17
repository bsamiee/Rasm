# [TS_RUNTIME_API_EFFECT_RPC]

`@effect/rpc` is the Schema-typed procedure-call family that mirrors `HttpApi` as the edge's second contribution law: `Rpc.make` defines one procedure from a tag plus payload/success/error/stream/defect `Schema`s, `RpcGroup.make(...rpcs)` collects procedures into a composable group carrying its own middleware, and the group is handled once (`toLayer`) and served over an orthogonal, swappable *Protocol* Layer (http | websocket | worker | stdio) crossed with an orthogonal *Serialization* Layer (json | ndjson | jsonRpc | msgpack). Streaming responses are first-class (`RpcSchema.Stream`), middleware is a schema-typed `TagClass` shared by server and client, the client is derived from the exact same group (`RpcClient.make`) so request/response types never drift, and an in-memory `RpcTest.makeClient` short-circuits the transport. It is the typed-procedure owner behind the RPC contribution family; the wire protocol, framing, and interruption are internal and never hand-rolled.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/rpc`
- package: `@effect/rpc`
- license: MIT
- asset: ESM `.d.ts` declaration surface (`dist/dts/*.d.ts`); peer `effect` + `@effect/platform`
- owner: `edge`
- rail: rpc
- peer: `effect` (`Schema`/`Layer`/`Effect`/`Stream`/`Context`/`FiberRef`), `@effect/platform` (`HttpRouter`/`HttpApp`/`HttpClient`/`Headers`/`Socket`/`Worker` — the transports), `msgpackr` (the msgpack serialization engine)
- namespaces: `Rpc` (procedure), `RpcGroup` (contribution family), `RpcServer` (protocol + serve rows), `RpcClient` (derived typed caller), `RpcMiddleware` (schema-typed middleware), `RpcSerialization` (wire codec rows), `RpcSchema` (streaming response schema), `RpcMessage` (wire protocol union), `RpcClientError`, `RpcWorker` (worker init), `RpcTest` (in-memory client)
- capability: Schema-typed request/response/error/stream procedures, group assembly with group-scoped middleware, server binding to orthogonal protocol × serialization Layer rows (http/websocket/worker/stdio × json/ndjson/jsonRpc/msgpack), a group-derived typed client with per-request headers, first-class streaming, and a transport-free test client
- import: `import { Rpc, RpcGroup, RpcServer, RpcClient, RpcSerialization, RpcMiddleware } from "@effect/rpc"`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: procedure model (`Rpc`)
- rail: rpc
- `Rpc<Tag, Payload, Success, Error, Middleware>` is one Schema-typed procedure. `Rpc.*` type projections (`Payload`/`Success`/`Error`/`Exit`/`Context`/`Middleware`) recover each axis for handler and client typing without re-declaration. `Rpc.Wrapper` marks a handler response `fork`ed (concurrent) or `uninterruptible`.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY]   | [RAIL]                                             |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Rpc.Rpc<Tag, Payload, Success, Error, Middleware>`              | branded record  | one Schema-typed procedure                         |
|  [02]   | `Rpc.Handler<Tag>`                                               | interface       | the handler function type derived from a procedure |
|  [03]   | `Rpc.Any` / `Rpc.AnyWithProps`                                   | interface       | existential procedure for group/collection typing  |
|  [04]   | `Rpc.Payload`/`Success`/`Error`/`Exit`/`Context`/`Middleware<R>` | type projection | recover one axis of a procedure                    |
|  [05]   | `Rpc.Wrapper`                                                    | interface       | `fork`/`uninterruptible` response marker           |
|  [06]   | `Rpc.From<S>`                                                    | interface       | `Schema.TaggedRequest` → `Rpc` bridge              |

[PUBLIC_TYPE_SCOPE]: group contribution family (`RpcGroup`)
- rail: rpc
- `RpcGroup<Rpcs>` is the RPC analogue of `HttpApiGroup`: domain folders export it as data, the app merges and handles it. `HandlersFrom<R>` is the exact handler-record type the group requires — the compiler rejects a missing or mistyped handler.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [RAIL]                                             |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `RpcGroup.RpcGroup<Rpcs>`                          | branded record  | a collected, middleware-carrying set of procedures |
|  [02]   | `RpcGroup.Rpcs<G>` / `RpcGroup.Any`                | type projection | recover the procedure union / existential group    |
|  [03]   | `RpcGroup.HandlersFrom<R>` / `HandlerFrom<R, Tag>` | type projection | required handler record / one handler type         |
|  [04]   | `RpcGroup.HandlersContext<R>`                      | type projection | the aggregate `R` the handlers demand              |

[PUBLIC_TYPE_SCOPE]: server, client, streaming, middleware
- rail: rpc
- Server is a `Layer` demanding a `Protocol`; the client is derived from the group so it shares every Schema. `RpcSchema.Stream<A, E>` types a streaming response as an `effect/Stream`. `RpcMiddleware.TagClass` is a schema-typed middleware definition usable on both ends.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :--------------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `RpcServer.RpcServer<Rpcs>`                          | service       | the served handler set demanding a `Protocol` Layer       |
|  [02]   | `RpcClient.RpcClient<Rpcs>` / `RpcClient.Flat<Rpcs>` | service       | group-derived typed caller (nested / flattened)           |
|  [03]   | `RpcSchema.Stream<A, E>`                             | schema        | streaming response schema (`Stream<A, E>`)                |
|  [04]   | `RpcMiddleware.TagClass<Self, Name, Options>`        | Context.Tag   | schema-typed middleware definition (`failure`/`provides`) |
|  [05]   | `RpcSerialization.Parser` / `RpcSerialization`       | service       | wire-codec parser contract / the serialization Tag        |
|  [06]   | `RpcClientError`                                     | error class   | client transport `TaggedError`                            |
|  [07]   | `RpcMessage.FromClient`/`FromServer`                 | wire union    | the wire protocol message unions                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: procedure definition (`Rpc`)
- rail: rpc
- `Rpc.make(tag, options)` is the one procedure constructor: `payload`/`success`/`error` are `Schema`s (or `Struct.Fields`), `stream: true` lifts `success` to `RpcSchema.Stream`, `primaryKey` keys request dedup, `defect` schemas an uncaught failure. `fromTaggedRequest` bridges an existing `Schema.TaggedRequest`. Handler responses wrap in `fork`/`uninterruptible`.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `Rpc.make(tag, options)`                         | constructor    | define one Schema-typed procedure                    |
|  [02]   | `Rpc.fromTaggedRequest(schema)`                  | bridge         | lift a `Schema.TaggedRequest` class to an `Rpc`      |
|  [03]   | `Rpc.exitSchema(rpc)`                            | schema         | the `Schema<Exit>` for a procedure's result          |
|  [04]   | `Rpc.fork(value)` / `Rpc.uninterruptible(value)` | wrapper        | mark a handler response concurrent / uninterruptible |

[ENTRYPOINT_SCOPE]: group assembly + handlers (`RpcGroup`)
- rail: rpc
- `RpcGroup.make(...rpcs)` collects procedures; `.middleware(M)` scopes a middleware to the procedures added so far; `.merge` unions groups. `.toLayer(build)` implements every handler as a `Layer` (accepts an `Effect` yielding the handler record for handlers needing setup); `.toLayerHandler(tag, build)` implements one.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `RpcGroup.make(...rpcs)`                     | assembly       | collect procedures into a group (contributed as data)    |
|  [02]   | `group.middleware(M)` / `group.merge(other)` | assembly       | scope a `TagClass` middleware / union two groups         |
|  [03]   | `group.of(handlers)`                         | typing         | identity-typed handler record (compiler-checked shape)   |
|  [04]   | `group.toLayer(build)`                       | handler        | implement all handlers as a `Layer` (record or `Effect`) |
|  [05]   | `group.toLayerHandler(tag, build)`           | handler        | implement one procedure's handler as a `Layer`           |

[ENTRYPOINT_SCOPE]: server protocol rows (http | websocket | worker | stdio)
- rail: rpc
- `RpcServer.layer(group, options?)` is the served handler set; it demands a `Protocol` provided by one `layerProtocol*` row (the transport axis) plus one `RpcSerialization` layer (the codec axis) — orthogonal and swappable. `layerHttpRouter` fuses handler+protocol+mount for the common HTTP/WS case; `toWebHandler` yields a `fetch` handler for edge runtimes. Every surface is an `RpcServer.*` member, and each `layerProtocol*` row pairs its unlayered `makeProtocol*` twin.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `layer(group, { concurrency?, spanPrefix?, disableTracing? })` | serve          | the handler-set Layer (demands `Protocol` + handlers) |
|  [02]   | `layerProtocolHttp({ path, … })` / `makeProtocolHttp`          | protocol       | HTTP transport over `HttpRouter`                      |
|  [03]   | `layerProtocolHttpRouter` / `makeProtocolHttpRouter`           | protocol       | HTTP transport on an existing `HttpRouter`            |
|  [04]   | `layerProtocolWebsocket` / `makeProtocolWebsocket`             | protocol       | WebSocket transport (the `live` socket row)           |
|  [05]   | `layerProtocolWebsocketRouter` / `makeProtocolWebsocketRouter` | protocol       | WebSocket transport on an existing router             |
|  [06]   | `layerProtocolWorkerRunner` / `makeProtocolWorkerRunner`       | protocol       | `WorkerRunner` transport (the `worker` row)           |
|  [07]   | `layerProtocolStdio({ stdin, stdout })` / `makeProtocolStdio`  | protocol       | stdio transport (`stdin` Stream / `stdout` Sink)      |
|  [08]   | `layerProtocolSocketServer` / `makeProtocolSocketServer`       | protocol       | raw socket transport over a `SocketServer`            |
|  [09]   | `makeProtocolWithHttpApp` / `makeProtocolWithHttpAppWebsocket` | protocol (raw) | `Protocol` from an existing `HttpApp` (unary / +WS)   |
|  [10]   | `layerHttpRouter({ group, path, protocol? })`                  | serve+mount    | fused handler+protocol mount on an `HttpRouter` path  |
|  [11]   | `toHttpApp(group, options?)`                                   | serve          | `HttpApp` value from a group                          |
|  [12]   | `toWebHandler(group, { middleware?, memoMap? })`               | serve          | `fetch` `{ handler, dispose }` for edge runtimes      |

[ENTRYPOINT_SCOPE]: client protocol rows + per-request headers (`RpcClient`)
- rail: rpc
- `RpcClient.make(group, options?)` derives a fully-typed caller from the *same* group. Client protocol rows mirror the server; `currentHeaders` is a `FiberRef<Headers>`, `withHeaders` scopes per-call headers (auth token, trace context), and `withHeadersEffect` derives them from an effect — no header threads through every call. Every surface is an `RpcClient.*` member.

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :-------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `make(group, { spanPrefix?, flatten?, generateRequestId? })`          | derive         | build the group-typed caller (an `Effect`) |
|  [02]   | `layerProtocolHttp({ url, transformClient?, retryTransientErrors? })` | protocol       | HTTP client transport over `HttpClient`    |
|  [03]   | `layerProtocolSocket({ retryTransientErrors? })`                      | protocol       | socket client transport                    |
|  [04]   | `layerProtocolWorker({ size, concurrency? })`                         | protocol       | worker-pool client transport               |
|  [05]   | `makeProtocolHttp(client)`                                            | protocol (raw) | unlayered HTTP client `Protocol`           |
|  [06]   | `makeProtocolSocket(options?)`                                        | protocol (raw) | unlayered socket client `Protocol`         |
|  [07]   | `makeProtocolWorker(options)`                                         | protocol (raw) | unlayered worker client `Protocol`         |
|  [08]   | `withHeaders` / `withHeadersEffect` / `currentHeaders`                | headers        | per-call / effect-derived / the `FiberRef` |

[ENTRYPOINT_SCOPE]: serialization codec rows (`RpcSerialization`)
- rail: rpc
- Serialization is the second orthogonal axis: a bare `RpcSerialization` Layer crossed with any protocol. `layer*` rows are the wire codecs, `makeMsgPack` tunes `msgpackr`, and the bare `json`/`ndjson`/`jsonRpc`/`ndJsonRpc`/`msgPack` values back the layers.

| [INDEX] | [SURFACE]                                                        | [CODEC_FAMILY] | [RAIL]                                      |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `RpcSerialization.layerJson`                                     | layer          | JSON serialization Layer                    |
|  [02]   | `RpcSerialization.layerNdjson`                                   | layer          | NDJSON serialization Layer                  |
|  [03]   | `RpcSerialization.layerJsonRpc(options?)`                        | layer          | JSON-RPC serialization Layer                |
|  [04]   | `RpcSerialization.layerNdJsonRpc(options?)`                      | layer          | NDJSON-RPC serialization Layer              |
|  [05]   | `RpcSerialization.layerMsgPack`                                  | layer          | MessagePack serialization Layer             |
|  [06]   | `RpcSerialization.makeMsgPack(msgpackrOptions?)`                 | tuned          | msgpack Layer with tuned `msgpackr` options |
|  [07]   | `RpcSerialization.json`/`ndjson`/`jsonRpc`/`ndJsonRpc`/`msgPack` | bare           | the bare codec values behind the layers     |

[ENTRYPOINT_SCOPE]: middleware, streaming, worker, test
- rail: rpc
- `RpcMiddleware.Tag` defines a schema-typed middleware once for both ends. `RpcSchema.Stream` constructs a streaming-response schema; `RpcWorker.layerInitialMessage` sends a typed handshake payload; `RpcTest.makeClient` calls handlers directly with no transport.

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `RpcMiddleware.Tag<Self>()(id, { failure?, provides?, wrap?, requiredForClient? })` | middleware     | define the middleware `TagClass` |
|  [02]   | `RpcMiddleware.layerClient(tag, service)`                                           | middleware     | client-side arm of a middleware  |
|  [03]   | `RpcSchema.Stream({ success, failure })`                                            | streaming      | streaming-response schema        |
|  [04]   | `RpcSchema.isStreamSchema(u)`                                                       | streaming      | stream-schema guard              |
|  [05]   | `RpcWorker.layerInitialMessage(schema, build)`                                      | worker         | send the worker handshake        |
|  [06]   | `RpcWorker.initialMessage(schema)`                                                  | worker         | receive the worker handshake     |
|  [07]   | `RpcTest.makeClient(group, { flatten? })`                                           | test           | in-memory client, no transport   |

## [04]-[IMPLEMENTATION_LAW]

[RPC_TOPOLOGY]:
- `RpcGroup` is the second contribution family under the edge assembly law (peer to `HttpApiGroup`): domain folders export groups as *data*, the app merges selected groups and assembles exactly one served value at the app root. No assembled server exists lib-side, so the god-contract cannot be spelled.
- a procedure is defined once by `Rpc.make` from Schemas; the request/response/error/stream types flow into both the handler (`RpcGroup.toLayer`) and the derived client (`RpcClient.make`) from the same group, so client and server can never drift.
- transport and serialization are two orthogonal `Layer` axes crossed at the app root: one `RpcServer.layerProtocol*` (http | websocket | socket-server | worker | stdio) × one `RpcSerialization.layer*` (json | ndjson | jsonRpc | ndJsonRpc | msgpack). Switch transport or codec by swapping one Layer row, never by rewriting procedures or handlers.
- middleware is a schema-typed `RpcMiddleware.TagClass` scoped to a group via `.middleware(M)`; `provides` narrows the handler `R`, `failure` types the rejection, and `requiredForClient` forces the client to satisfy it — one definition governs both ends.
- streaming is first-class: `stream: true` on `Rpc.make` (or `RpcSchema.Stream`) makes the response an `effect/Stream<A, E>` that the protocol frames as `ResponseChunk` messages terminated by `ResponseExit`; never hand-frame a stream over a unary call.

[STACKS_WITH]:
- `@effect/platform` (`.api/effect-platform.md`): the transports are platform contracts — `layerProtocolHttp`/`layerHttpRouter` mount on `HttpRouter`/`HttpApp` (the same mount vocabulary the `api/serve` rows use), `layerProtocolWebsocket` rides `Socket`, `layerProtocolWorkerRunner` rides `Worker`/`WorkerRunner`, and `RpcClient.layerProtocolHttp` composes an `HttpClient`. `RpcClient.currentHeaders` is an `@effect/platform` `Headers.Headers` FiberRef. An RPC server and an `HttpApi` coexist on one `HttpRouter` mounted at distinct paths.
- `@effect/platform-node` / `@effect/platform-bun` (`.api/effect-platform-node.md`, `.api/effect-platform-bun.md`): the concrete serve is `NodeHttpServer.layer`/`BunHttpServer.layer` under the protocol layer; `RpcServer.toWebHandler` yields the `fetch` handler for the `toWebHandler` serve row and edge/WASM runtimes. `layerProtocolWorkerRunner` binds `NodeWorkerRunner`/`BunWorkerRunner` for the `work` compute-worker rows.
- `effect` (`.api/effect.md`): every procedure axis is a `Schema`; handlers are `Effect`s and streaming handlers are `Stream`s; groups compose as `Layer`s; `RpcMessage` unions refine with `Match`. A domain `Data.TaggedError` becomes an `Rpc` `error` Schema and reconstructs client-side as the same tagged failure — one error vocabulary spans the wire.
- `wire` + `security` + `telemetry` (folder rails): `RpcSerialization.layerMsgPack`/`makeMsgPack` is the binary codec the `wire` folder standardizes for internal service-to-service RPC; `RpcMiddleware` carries the `security` auth/API-key admission (the same closed middleware family the HTTP path uses) with `provides` injecting the authenticated principal; `disableTracing`/`spanPrefix` and the `telemetry` W3C trace-context headers flow through `RpcClient.currentHeaders` so a call is one continuous distributed span.

[LOCAL_ADMISSION]:
- Domain folders export `RpcGroup` values (procedures + group middleware) as data and their handler `Layer` via `toLayer`; the app root merges the selected groups, crosses one protocol row with one serialization row, and binds the concrete serve layer — the whole RPC surface is assembled, never centrally declared.
- `live` socket row selects `layerProtocolWebsocket`, the `work` compute rows select `layerProtocolWorkerRunner`, MCP/CLI rows select `layerProtocolStdio`, and edge/fetch runtimes select `toWebHandler` — one group, many protocol rows, chosen by deployment.
- Internal service-to-service calls select `layerMsgPack` for compactness; browser/public calls select `layerJson`/`layerNdjson`. Client is always `RpcClient.make(group)` against the exact group — a hand-written fetch client for a procedure is forbidden.
- Specs drive `RpcTest.makeClient(group)` to exercise handlers with full Schema typing and zero transport; the same group backs the test and production clients.

[RAIL_LAW]:
- Package: `@effect/rpc`
- Owns: Schema-typed procedure definition, group contribution/assembly with group-scoped middleware, the orthogonal protocol × serialization serve axes, the group-derived typed client with per-request headers, first-class streaming, and the transport-free test client
- Accept: `Rpc.make` procedures, `RpcGroup.make`/`.middleware`/`.toLayer` assembly, one `layerProtocol*` × one `RpcSerialization.layer*` at the app root, `RpcClient.make` derived callers, `RpcMiddleware.Tag` schema-typed middleware, `RpcSchema.Stream` streaming, `RpcTest.makeClient` in specs
- Reject: hand-rolled JSON-over-HTTP procedure endpoints, a client typed separately from its group, manual stream framing over unary calls, transport or codec choice baked into handlers, untyped/ad-hoc middleware, a centralized RPC contract with lib-side existence
