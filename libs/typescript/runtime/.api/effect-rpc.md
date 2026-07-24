# [TS_RUNTIME_API_EFFECT_RPC]

`@effect/rpc` mints Schema-typed procedures and serves them across two orthogonal, swappable Layers — one transport (http | websocket | worker | stdio) crossed with one serialization codec (json | ndjson | jsonRpc | msgpack) — as the RPC peer of `HttpApi` under the edge contribution law. A procedure is defined once from its payload/success/error/stream `Schema`s and flows unchanged into both the handler and the group-derived client, so request and response types never drift; the wire protocol, framing, and interruption stay internal and never hand-rolled.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/rpc`
- package: `@effect/rpc` (MIT)
- asset: ESM `.d.ts` declaration surface
- owner: `edge`
- rail: rpc
- peer: `effect` (Schema/Layer/Effect/Stream rails), `@effect/platform` (the transports), `msgpackr` (msgpack engine)
- namespaces: `Rpc` procedure, `RpcGroup` contribution family, `RpcServer` protocol+serve rows, `RpcClient` derived caller, `RpcMiddleware` schema-typed middleware, `RpcSerialization` codec rows, `RpcSchema` streaming schema, `RpcMessage` wire union, `RpcWorker` worker init, `RpcTest` in-memory client, `RpcClientError`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: procedure model (`Rpc`)

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Rpc.Rpc<Tag, Payload, Success, Error, Middleware>`              | branded record  | one Schema-typed procedure                         |
|  [02]   | `Rpc.Handler<Tag>`                                               | interface       | the handler function type derived from a procedure |
|  [03]   | `Rpc.Any` / `Rpc.AnyWithProps`                                   | interface       | existential procedure for group/collection typing  |
|  [04]   | `Rpc.Payload`/`Success`/`Error`/`Exit`/`Context`/`Middleware<R>` | type projection | recover one axis of a procedure                    |
|  [05]   | `Rpc.Wrapper`                                                    | interface       | `fork`/`uninterruptible` response marker           |
|  [06]   | `Rpc.From<S>`                                                    | interface       | `Schema.TaggedRequest` → `Rpc` bridge              |

[PUBLIC_TYPE_SCOPE]: group contribution family (`RpcGroup`)

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `RpcGroup.RpcGroup<Rpcs>`                          | branded record  | a collected, middleware-carrying set of procedures |
|  [02]   | `RpcGroup.Rpcs<G>` / `RpcGroup.Any`                | type projection | recover the procedure union / existential group    |
|  [03]   | `RpcGroup.HandlersFrom<R>` / `HandlerFrom<R, Tag>` | type projection | required handler record / one handler type         |
|  [04]   | `RpcGroup.HandlersContext<R>`                      | type projection | the aggregate `R` the handlers demand              |

[PUBLIC_TYPE_SCOPE]: server, client, streaming, middleware

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                              |
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
- `Rpc.make` options beyond the schemas: `stream: true` lifts `success` to `RpcSchema.Stream`, `primaryKey` keys request dedup, and `defect` schemas an uncaught failure.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `Rpc.make(tag, options)`                         | constructor    | define one Schema-typed procedure                    |
|  [02]   | `Rpc.fromTaggedRequest(schema)`                  | bridge         | lift a `Schema.TaggedRequest` class to an `Rpc`      |
|  [03]   | `Rpc.exitSchema(rpc)`                            | schema         | the `Schema<Exit>` for a procedure's result          |
|  [04]   | `Rpc.fork(value)` / `Rpc.uninterruptible(value)` | wrapper        | mark a handler response concurrent / uninterruptible |

[ENTRYPOINT_SCOPE]: group assembly + handlers (`RpcGroup`)
- `group.middleware(M)` scopes `M` to the procedures added before it.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `RpcGroup.make(...rpcs)`                     | assembly       | collect procedures into a group (contributed as data)    |
|  [02]   | `group.middleware(M)` / `group.merge(other)` | assembly       | scope a `TagClass` middleware / union two groups         |
|  [03]   | `group.of(handlers)`                         | typing         | identity-typed handler record (compiler-checked shape)   |
|  [04]   | `group.toLayer(build)`                       | handler        | implement all handlers as a `Layer` (record or `Effect`) |
|  [05]   | `group.toLayerHandler(tag, build)`           | handler        | implement one procedure's handler as a `Layer`           |

[ENTRYPOINT_SCOPE]: server protocol rows (http | websocket | worker | stdio)
- Each `layerProtocol*` row pairs its unlayered `makeProtocol*` twin.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                          |
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

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CAPABILITY]                               |
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

| [INDEX] | [SURFACE]                                                        | [CODEC_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `RpcSerialization.layerJson`                                     | layer          | JSON serialization Layer                    |
|  [02]   | `RpcSerialization.layerNdjson`                                   | layer          | NDJSON serialization Layer                  |
|  [03]   | `RpcSerialization.layerJsonRpc(options?)`                        | layer          | JSON-RPC serialization Layer                |
|  [04]   | `RpcSerialization.layerNdJsonRpc(options?)`                      | layer          | NDJSON-RPC serialization Layer              |
|  [05]   | `RpcSerialization.layerMsgPack`                                  | layer          | MessagePack serialization Layer             |
|  [06]   | `RpcSerialization.makeMsgPack(msgpackrOptions?)`                 | tuned          | msgpack Layer with tuned `msgpackr` options |
|  [07]   | `RpcSerialization.json`/`ndjson`/`jsonRpc`/`ndJsonRpc`/`msgPack` | bare           | the bare codec values behind the layers     |

[ENTRYPOINT_SCOPE]: middleware, streaming, worker, test

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `RpcMiddleware.Tag<Self>()(id, { failure?, provides?, wrap?, requiredForClient? })` | middleware     | define the middleware `TagClass` |
|  [02]   | `RpcMiddleware.layerClient(tag, service)`                                           | middleware     | client-side arm of a middleware  |
|  [03]   | `RpcSchema.Stream({ success, failure })`                                            | streaming      | streaming-response schema        |
|  [04]   | `RpcSchema.isStreamSchema(u)`                                                       | streaming      | stream-schema guard              |
|  [05]   | `RpcWorker.layerInitialMessage(schema, build)`                                      | worker         | send the worker handshake        |
|  [06]   | `RpcWorker.initialMessage(schema)`                                                  | worker         | receive the worker handshake     |
|  [07]   | `RpcTest.makeClient(group, { flatten? })`                                           | test           | in-memory client, no transport   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RpcGroup` is the edge contribution family peer to `HttpApiGroup`: domain folders export groups as data, and the app merges selected groups into one served value at the app root.
- A procedure defined by `Rpc.make` flows its request/response/error/stream types into both the handler (`RpcGroup.toLayer`) and the derived client (`RpcClient.make`) from one group, so client and server share one Schema source.
- Transport and serialization are two orthogonal `Layer` axes crossed at the app root: one `RpcServer.layerProtocol*` (http | websocket | socket-server | worker | stdio) with one `RpcSerialization.layer*` (json | ndjson | jsonRpc | ndJsonRpc | msgpack), each switched by swapping one Layer row.
- Middleware is a schema-typed `RpcMiddleware.TagClass` scoped to a group via `.middleware(M)`: `provides` narrows the handler `R`, `failure` types the rejection, and `requiredForClient` forces the client to satisfy it — one definition governs both ends.
- Streaming is first-class: `stream: true` on `Rpc.make` (or `RpcSchema.Stream`) makes the response an `effect/Stream<A, E>` framed as `ResponseChunk` messages terminated by `ResponseExit`.

[STACKING]:
- `@effect/platform` (`.api/effect-platform.md`): the transports are platform contracts — `layerProtocolHttp`/`layerHttpRouter` mount on `HttpRouter`/`HttpApp`, `layerProtocolWebsocket` rides `Socket`, `layerProtocolWorkerRunner` rides `Worker`/`WorkerRunner`, `RpcClient.layerProtocolHttp` composes an `HttpClient`, and `RpcClient.currentHeaders` is a `Headers.Headers` `FiberRef`. An RPC server and an `HttpApi` coexist on one `HttpRouter` at distinct paths.
- `@effect/platform-node` / `@effect/platform-bun` (`.api/effect-platform-node.md`, `.api/effect-platform-bun.md`): the concrete serve is `NodeHttpServer.layer`/`BunHttpServer.layer` under the protocol layer; `RpcServer.toWebHandler` yields the `fetch` handler for edge/WASM runtimes; `layerProtocolWorkerRunner` binds `NodeWorkerRunner`/`BunWorkerRunner`.
- `effect` (`.api/effect.md`): every procedure axis is a `Schema`, handlers are `Effect`s, streaming handlers are `Stream`s, groups compose as `Layer`s, and `RpcMessage` unions refine with `Match`; a domain `Data.TaggedError` becomes an `Rpc` `error` Schema and reconstructs client-side as the same tagged failure.
- `wire` + `security` + `telemetry` (folder rails): `RpcSerialization.layerMsgPack`/`makeMsgPack` is the binary codec `wire` standardizes for internal service-to-service RPC; `RpcMiddleware` carries the `security` auth/API-key admission with `provides` injecting the authenticated principal; `spanPrefix`/`disableTracing` and the `telemetry` W3C trace-context headers flow through `RpcClient.currentHeaders` so a call is one continuous distributed span.

[LOCAL_ADMISSION]:
- Domain folders export `RpcGroup` values with their handler `Layer` via `toLayer`; the app root merges selected groups, crosses one protocol row with one serialization row, and binds the concrete serve layer.
- Deployment selects the protocol row: `live` → `layerProtocolWebsocket`, `work` compute → `layerProtocolWorkerRunner`, MCP/CLI → `layerProtocolStdio`, edge/fetch → `toWebHandler`; one group serves every row.
- Internal service-to-service calls select `layerMsgPack` for compactness; browser and public calls select `layerJson`/`layerNdjson`; the caller is always `RpcClient.make(group)` against the exact group.
- Specs drive `RpcTest.makeClient(group)` with full Schema typing and zero transport; one group backs the test and production clients.

[RAIL_LAW]:
- Package: `@effect/rpc`
- Owns: Schema-typed procedure definition, group contribution/assembly with group-scoped middleware, the orthogonal protocol × serialization serve axes, the group-derived typed client with per-request headers, first-class streaming, and the transport-free test client
- Accept: `Rpc.make` procedures, `RpcGroup.make`/`.middleware`/`.toLayer` assembly, one `layerProtocol*` × one `RpcSerialization.layer*` at the app root, `RpcClient.make` derived callers, `RpcMiddleware.Tag` schema-typed middleware, `RpcSchema.Stream` streaming, `RpcTest.makeClient` in specs
- Reject: hand-rolled JSON-over-HTTP procedure endpoints, a client typed separately from its group, manual stream framing over unary calls, transport or codec choice baked into handlers, untyped/ad-hoc middleware, a centralized RPC contract with lib-side existence
