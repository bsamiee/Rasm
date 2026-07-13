# [TS_BRANCH_API_EFFECT_PLATFORM]

`@effect/platform` is the platform-neutral service-contract tier the branch composes for every host, wire, and edge boundary: the declarative HTTP-API family (`HttpApi`/`HttpApiGroup`/`HttpApiEndpoint` as data, `HttpApiBuilder` handlers, `HttpApiClient` derived typed SDKs, `OpenApi`/`HttpApiScalar` emission, `HttpApiSecurity`/`HttpApiMiddleware`), the request/response client (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`, `FetchHttpClient`), the server + router (`HttpServer`, `HttpRouter`, `HttpApp`, `HttpLayerRouter`, `HttpMiddleware`), the web-value codecs (`HttpBody`, `Headers`, `Cookies`, `UrlParams`, `Url`, `Etag`, `Multipart`, `HttpMethod`), the system-API contracts as `Context.Tag`s (`FileSystem`, `Path`, `KeyValueStore`, `Command`/`CommandExecutor`, `Terminal`, `Socket`/`SocketServer`, `Worker`/`WorkerRunner`), the frame codecs (`MsgPack`, `Ndjson`, `Transferable`, `Template`), and the config/logger boundary (`PlatformConfigProvider`, `PlatformLogger`, `Runtime`, `PlatformError`). Every contract is an abstract Tag with no runtime binding of its own — a per-runtime package (`platform-node`, `-bun`, `-browser`) provides the `Layer` — so a domain folder codes once against `HttpClient`/`FileSystem`/`Worker` and the app root picks the runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform`
- package: `@effect/platform` (MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`@effect/platform/HttpApi`, `@effect/platform/FileSystem`, …)
- runtime target: platform-neutral abstract contracts — no runtime binding; a `platform-node`/`-bun`/`-browser` `Layer` satisfies each Tag. `find-my-way-ts` (router match), `msgpackr` (`MsgPack`), and `multipasta` (`Multipart`) are the only bundled runtime deps
- peer: `effect@^catalog`
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); Tag contracts + `Schema`-typed endpoint declarations
- rail: platform contracts (host, wire, edge, store; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declarative HTTP-API — the contribution family `edge` assembles
- rail: boundaries

| [INDEX] | [SYMBOL]                                                                                                                                                           | [TYPE_FAMILY]      | [CONSUMER]                                                                                                                                                                                                              |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `HttpApi<Groups, E, R>`                                                                                                                                            | api value          | `edge/api` — the app owns exactly one assembled `HttpApi`; groups are contributed data                                                                                                                                  |
|  [02]   | `HttpApiGroup<Name, Endpoints>`                                                                                                                                    | group              | domain folders contribute a `HttpApiGroup`; the god-contract is structurally impossible                                                                                                                                 |
|  [03]   | `HttpApiEndpoint<Name, Method>`                                                                                                                                    | endpoint           | one endpoint = `Schema` request + success + error; `.setPath`/`.addSuccess`/`.addError` builders                                                                                                                        |
|  [04]   | `HttpApiSchema.Multipart` / `.param` / `.withEncoding`                                                                                                             | payload modality   | `edge` multipart uploads, path-param schemas, non-JSON response encodings                                                                                                                                               |
|  [05]   | `HttpApiSecurity.Bearer` / `.ApiKey` / `.Basic`                                                                                                                    | auth scheme        | `security` — declared on the endpoint, decoded to a credential the middleware verifies                                                                                                                                  |
|  [06]   | `HttpApiMiddleware.Tag<Self>()(id, { provides?, failure?, security?, optional? })` / `HttpApiMiddleware.TagClass.BaseSecurity` / `HttpApiError.HttpApiDecodeError` | middleware / fault | `runtime/serve/api` typed middleware Tags — the `security` option threads `HttpApiSecurity` schemes into the emitted spec and the implementation is one handler record keyed by scheme receiving the decoded credential |
|  [07]   | `HttpApiClient` (derived) / `OpenApi.OpenAPISpec`                                                                                                                  | client / spec      | `edge/emit` — one `HttpApi` yields the typed client SDK and the OpenAPI document; neither drifts                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: client, server, and routing
- rail: boundaries

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]      | [CONSUMER]                                                                                           |
| :-----: | :----------------------------------------------------- | :----------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `HttpClient.HttpClient`                                | client Tag         | `net/client` default-policy client; `core`/`ai`/OTLP compose it   |
|  [02]   | `HttpClientRequest` / `HttpClientResponse`             | request / response | immutable request builder; `Schema`-decoding response accessors   |
|  [03]   | `HttpClientError.RequestError` / `.ResponseError`      | client fault       | typed transport/decoding faults in the `Effect` channel           |
|  [04]   | `HttpServer.HttpServer` / `HttpApp.Default`            | server Tag / app   | `edge/serve` served value; a runtime `Layer` binds the socket     |
|  [05]   | `HttpRouter.HttpRouter` / `HttpLayerRouter.HttpRouter` | router             | `edge` route table; `HttpLayerRouter` for mixed HTTP + API mounts |
|  [06]   | `HttpServerRequest` / `HttpServerResponse`             | server io          | `edge` handlers; `HttpServerRespondable` self-renders errors      |
|  [07]   | `HttpServerError.RouteNotFound` / `HttpMiddleware`     | server fault / mw  | `edge/problem` maps RFC 9457; mw transforms the `HttpApp`         |

[PUBLIC_TYPE_SCOPE]: system-API contracts — abstract Tags a runtime `Layer` satisfies
- rail: system-apis
- Each contract is an abstract `Context.Tag` a per-runtime `Layer` satisfies. `FileSystem` exposes `readFileString`/`writeFile`/`watch`/`stream`/`makeTempDirectory`; `Path` `join`/`resolve`/`basename` (`layerPosix`/`layerWin32`); `KeyValueStore` `layerMemory`/`layerFileSystem`/`layerSchema` with `prefix` scope; `Command` declarative `pipeTo`/stream-stdout/typed-exit; `Socket` framed duplex over TCP/WebSocket as an Effect `Channel`. `Worker.WorkerManager`/`Worker.Spawner`/`WorkerRunner.PlatformRunner` and `Socket.WebSocketConstructor` are the runtime-provided Tags the `-node`/`-bun`/`-browser` bindings satisfy.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY] | [CONSUMER]                                             |
| :-----: | :-------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `FileSystem.FileSystem`                                         | fs Tag        | `host`, `store/lane`                                  |
|  [02]   | `Path.Path`                                                     | path Tag      | `host`, `iac`                                         |
|  [03]   | `KeyValueStore.KeyValueStore` / `SchemaStore`                   | kv Tag        | `data lane`, `browser/persist`                        |
|  [04]   | `Command.Command` / `CommandExecutor.Process`                  | subprocess    | `runtime/src/proc/exec.ts` declarative command        |
|  [05]   | `Terminal.Terminal`                                            | tty Tag       | `edge/cli` — line/key input + display                 |
|  [06]   | `Socket.Socket` / `SocketServer` / `Socket.WebSocketConstructor` | socket      | `net/channel`, `core interchange/frame`               |
|  [07]   | `Worker.WorkerPool` / `WorkerManager` / `Spawner` / `WorkerRunner.PlatformRunner` | worker | `proc/worker`, `browser/fetch` pools           |
|  [08]   | `PlatformError` (`BadArgument` / `SystemError`)                | system fault  | one error rail; `core interchange/codec` classifies it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: declaring, handling, serving, and consuming an `HttpApi`
- rail: boundaries
- `HttpApiEndpoint.get(name)`/`.post`/`.del` carry `.setPath`/`.setPayload(schema)`/`.addSuccess`/`.addError`; `HttpApiGroup.make(name)` carries `.add(endpoint)`/`.addError`/`.prefix`/`.middleware(tag)`; `HttpApi.make(id)` carries `.add(group)`/`.addError`/`.annotate`/`.middleware`. `HttpApiBuilder.group(api, name, (h) => h.handle(endpointName, handler))` binds handlers; `.api(api)`/`.serve(middleware?)`/`toWebHandler(api, options)` yield the api/serve `Layer` or web handler. `HttpApiBuilder.middlewareCors(options)`/`.middlewareOpenApi()`/`.securityDecode`; `HttpApiClient.make(api, { baseUrl, transformClient })`; `OpenApi.fromApi(api)`/`HttpApiScalar.layer()`/`HttpApiSwagger.layer()`.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]   | [CONSUMER]                                                  |
| :-----: | :--------------------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `HttpApiEndpoint.get` / `.post` / `.del` | declare endpoint | `edge` — path, request, success, error `Schema`s           |
|  [02]   | `HttpApiGroup.make(name)`                | declare group    | domain folders build a group; errors + mw ride it          |
|  [03]   | `HttpApi.make(id)`                       | assemble api     | `edge/api` — one `HttpApi` from selected groups            |
|  [04]   | `HttpApiBuilder.group(...)`              | implement        | `edge` — bind each endpoint; a missing handler won't compile |
|  [05]   | `HttpApiBuilder.serve`                   | serve            | `runtime/serve` — api `Layer`, serve `Layer`, or web handler |
|  [06]   | `HttpApiBuilder.middlewareCors`          | api middleware   | `edge/middleware` — CORS, OpenAPI route, security decode   |
|  [07]   | `HttpApiClient.make`                     | derive client    | `edge/emit` — typed SDK from the `HttpApi` value           |
|  [08]   | `OpenApi.fromApi` / `HttpApiScalar` / `HttpApiSwagger` | docs | `edge/emit` — spec document + reference UI                 |

[ENTRYPOINT_SCOPE]: `HttpClient` — request policy and typed responses
- rail: system-apis
- `HttpClientRequest.get(url)`/`.post(url)` carry `.setBody`/`.bodyJson`/`.bearerToken`/`.setUrlParams`; `HttpClient.execute(request)` returns `Effect<HttpClientResponse, HttpClientError, Scope>`; policy `.retryTransient({ schedule })`/`.filterStatusOk`/`.followRedirects`/`.mapRequest`; observability `.withTracerPropagation`/`.withTracerDisabledWhen`/`.tapRequest`; `HttpClientResponse.schemaJson(schema)`/`.matchStatus({...})`/`.stream` decode the body.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER]                                                       |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------------------- |
|  [01]   | `HttpClientRequest.get` / `.post`                             | build request  | `net/client`, `ai`, `core interchange/invoke` — immutable request |
|  [02]   | `HttpClient.execute` / `.get`                                 | dispatch       | typed response in the `Effect` channel                          |
|  [03]   | `HttpClient.retryTransient` / `.filterStatusOk`               | policy         | `net/client` — retry idempotent, reject non-2xx, add auth       |
|  [04]   | `HttpClient.withTracerPropagation` / `.tapRequest`            | observability  | `telemetry` — W3C egress propagation, per-request span control  |
|  [05]   | `HttpClientResponse.schemaJson` / `.matchStatus` / `.stream`  | decode         | `core interchange`/`ai` — decode body; status-class dispatch    |
|  [06]   | `FetchHttpClient.layer` / `HttpClient.layerMergedContext`     | provide        | default `fetch` client `Layer`; `platform-node` swaps undici    |

[ENTRYPOINT_SCOPE]: server, router, and middleware
- rail: boundaries
- `HttpRouter` carries `.empty`/`.get(path, handler)`/`.mountApp(prefix, app)`/`.use(middleware)`; `HttpServerResponse` `.json(data)`/`.schemaJson(schema)(value)`/`.text`/`.stream`/`.file` decorated by `.setCookie`/`.setHeaders`; `HttpServerRequest` `.schemaBodyJson(schema)`/`.schemaHeaders`/`.schemaSearchParams`/`.upgrade`; `HttpMiddleware` `.cors(options)`/`.logger`/`.xForwardedHeaders`/`.searchParamsParser`.
- `HttpLayerRouter` carries `.use`/`.add(method, path, handler)`/`.addAll`/`.addHttpApi(api, { openapiPath? })`/`.middleware`/`.cors()`/`.disableLogger`/`.serve`/`.toWebHandler`/`.params`/`.schemaJson`/`.schemaPathParams`; `HttpMultiplex` `.make`/`.empty`/`.add(predicate, app)`/`.headerExact`/`.headerRegex`/`.headerStartsWith`/`.hostExact`/`.hostRegex`; `HttpServerRespondable` `.symbol`/`.toResponse`/`.toResponseOrElse`/`.isRespondable`; `ChannelSchema` `.make`/`.duplex`/`.duplexUnknown({ inputSchema, outputSchema })`.
- `HttpApiScalar.layer({ path? })`/`.layerCdn`/`.layerHttpLayerRouter({ api?, path? })` and `HttpApiSwagger.layer({ path })` mount the docs UI; `HttpApiError.HttpApiDecodeError`/`.BadRequest`/`.Unauthorized`/`.Forbidden`/`.NotFound`/`.Conflict`/`.InternalServerError`/`.ServiceUnavailable` are the prebuilt status faults.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [CONSUMER]                                                                       |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `HttpRouter`                            | route          | `edge` — compose routes; `mountApp` mounts a sub-`HttpApp` (EventLog sync)       |
|  [02]   | `HttpServerResponse`                    | respond        | `edge` handlers; cookie/header decorators on the response value                 |
|  [03]   | `HttpServerRequest`                     | ingress decode | `edge` — body/header/query decode; `.upgrade` yields the WebSocket `Socket`      |
|  [04]   | `HttpMiddleware`                        | wrap           | `edge/middleware` — cross-cutting transforms on the handler `HttpApp`            |
|  [05]   | `HttpLayerRouter`                       | layer route    | `runtime/serve/route` — `Layer`-native router; raw routes + an `HttpApi`         |
|  [06]   | `HttpServer.serve` / `.layerTestClient` | run / test     | `runtime/serve/route` binds the app to the server `Layer`; test client           |
|  [07]   | `HttpMultiplex`                         | multiplex      | `runtime/serve/route` — host/header dispatch across several `HttpApp`s           |
|  [08]   | `HttpServerRespondable`                 | self-render    | `runtime/serve/problem` — a domain value self-renders; outbound-fault law         |
|  [09]   | `HttpApiScalar` / `HttpApiSwagger`      | docs ui        | `runtime/serve/api` — Scalar reference UI beside the derived OpenAPI route       |
|  [10]   | `HttpApiError` faults                   | status faults  | status-tagged endpoint errors; `runtime/serve/problem` folds escaped ones        |
|  [11]   | `ChannelSchema`                         | typed channel  | `runtime/serve/live` — `Schema`-typed bidirectional `Channel`                    |

[ENTRYPOINT_SCOPE]: system-API contracts and frame codecs
- rail: system-apis
- `Command` `.make(cmd, ...args)`/`.pipeTo(next)`/`.stream`/`.string`/`.exitCode`/`.env`; `KeyValueStore` `.layerFileSystem(dir)`/`.layerMemory`/`.layerSchema(schema)`/`.prefix(k)`; `Worker` `.makePool`/`.makePoolLayer` (the `Layer` form over a `Spawner`)/`.makePoolSerialized({ size })` with `WorkerRunner.layerSerialized(handlers)`; `Socket` `.toChannel(socket)`/`.makeWebSocket(url)`/`.layerWebSocket`.
- `MsgPack` `.duplexSchema({ inputSchema, outputSchema })`/`.pack` and `Ndjson` `.duplexSchema`/`.duplex`/`.duplexString` (text lines) frame `Schema`-typed messages over a byte `Channel`; `Multipart` `.toPersisted(parts)`/`.schemaPersisted(schema)`/`.withLimits(opts)`; `Transferable.schema(schema)`, `Template.make\`…\``, `HttpBody.json`/`.formData`.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CONSUMER]                                                              |
| :-----: | :----------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Command`                            | subprocess     | `runtime/src/proc/exec.ts` — command pipelines, streamed, typed exit   |
|  [02]   | `KeyValueStore`                      | kv store       | `data lane`, `browser/persist` — schema-typed KV, prefix-scoped        |
|  [03]   | `Worker` / `WorkerRunner`            | worker pool    | `proc/worker`, `browser/fetch` — `Schema`-serialized off-thread pools  |
|  [04]   | `Socket`                             | socket channel | `net/channel.ts`, `core interchange/frame` — duplex bytes as a `Channel` |
|  [05]   | `MsgPack` / `Ndjson`                 | frame codec    | `core interchange/codec`, `runtime/serve/live` — MsgPack/NDJSON framing |
|  [06]   | `Multipart`                          | upload         | `edge` — decode multipart uploads to persisted files under limits      |
|  [07]   | `Transferable` / `Template` / `HttpBody` | payload    | `browser/fetch` transfer, `edge` templating, request/response bodies   |

[ENTRYPOINT_SCOPE]: config, logging, and process lifecycle boundary
- rail: system-apis
- `PlatformConfigProvider` `.fromDotEnv(path)`/`.layerDotEnv`/`.fromFileTree`/`.layerFileTree`; `PlatformLogger.toFile(path, { batchWindow })`; `Runtime` `.makeRunMain(f)`/`.RunMain`/`.defaultTeardown`; `Headers.redact(headers, keys)`, `Cookies.toCookieHeader`, `UrlParams.schemaStruct(schema)`; `Etag` `.Generator`/`.layer`/`.layerWeak` with `HttpServerResponse.setBody(HttpBody.fileWeb(file))`.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [CONSUMER]                                                                    |
| :-----: | :----------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `PlatformConfigProvider`             | config source  | `runtime/src/proc/config.ts` — dotenv + file-tree (K8s secret mount) providers |
|  [02]   | `PlatformLogger`                     | log sink       | `telemetry` — durable batched file logging behind `Logger`                   |
|  [03]   | `Runtime`                            | run-main       | each runtime's `runMain` type; `-node`/`-bun`/`-browser` drain signals on exit |
|  [04]   | `Headers` / `Cookies` / `UrlParams`  | web value      | `security` redaction, `edge` cookie serialization, typed query-param decode   |
|  [05]   | `Etag`                               | caching        | `edge/serve` static-asset ETag generation and immutable-asset responses      |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- Every system contract is a `Context.Tag` with no built-in binding: `FileSystem.FileSystem`, `Path.Path`, `HttpClient.HttpClient`, `Command`/`CommandExecutor`, `Terminal`, `Socket`, `Worker`, `KeyValueStore`. A domain folder yields the Tag inside `Effect.gen` and never imports a runtime; the app root provides the `platform-node`/`-bun`/`-browser` `Layer`. This is what keeps `host`/`core interchange`/`edge` runtime-portable — a bun swap is a `Layer` selection, not a fork.
- The HTTP-API family is declaration-then-implementation: `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` are pure data carrying `Schema` request/success/error shapes; `HttpApiBuilder.group` binds each endpoint to an `Effect` handler with the handler set checked exhaustively against the declaration. One `HttpApi` value derives the server (`HttpApiBuilder.serve`), the typed client (`HttpApiClient.make`), and the OpenAPI document (`OpenApi.fromApi`) — spec, docs, client, and server cannot drift because they share one source.
- The client and server are `Schema`-symmetric: `HttpClientRequest`/`HttpServerRequest` decode inbound through `schemaBodyJson`, `HttpClientResponse`/`HttpServerResponse` encode outbound through `schemaJson`, and both fail into a tagged error family (`HttpClientError`, `HttpServerError`, `HttpApiDecodeError`) that flows the `Effect` error channel to the `edge/problem` RFC 9457 mapping.
- `HttpLayerRouter` is the `Layer`-composable server: routes and middleware are `Layer`s, `addHttpApi` mounts a full declarative api beside raw routes, and the whole server is one `Layer` the app root provides — the model `edge/serve` uses when an app mixes an `HttpApi`, an EventLog sync `HttpApp`, and static assets under one server.
- Frame codecs (`MsgPack`, `Ndjson`) and `Socket` compose as Effect `Channel`s: `Socket.toChannel` turns a duplex connection into a byte channel, and `MsgPack.duplexSchema`/`Ndjson.duplexSchema` layer a `Schema`-typed message channel over it — the framed transport rail is decode-typed end to end with backpressure, never a raw event emitter.
- `Worker`/`WorkerRunner` are `Schema`-serialized RPC over a thread/process boundary: the pool sends a decoded request and awaits a decoded response, so `runtime browser/fetch` moves content-key verification off-thread and `proc/worker` runs CPU-heavy work in a pool with the same typed contract as an in-process call.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): every contract is an `Effect`-returning service keyed by `Context.Tag`; endpoint payloads, request bodies, and response bodies are `Schema`; middleware and client policy are effect transformers. The platform tier adds no new rail — it is `effect` applied to the boundary.
- `@effect/platform-node` (`.api/effect-platform-node.md`): `NodeContext.layer` satisfies `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker` in one Layer; `NodeHttpServer.layer` binds `HttpServer`; `NodeHttpClient.layerUndici` binds `HttpClient`. `@effect/platform-bun` (`.api/effect-platform-bun.md`) and `-browser` (`.api/effect-platform-browser.md`) are peer swaps behind the same Tags.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): `HttpClient.withTracerPropagation` and `HttpMiddleware` inject/extract W3C trace context; the OTel `Layer` under the graph exports every server span and client egress span with no handler change — `runtime/src/otel/emit.ts` owns the extract-and-continue seam.
- `security` folder (`arctic`/`jose`/`@node-rs/argon2`, catalogued at `libs/typescript/security/.api/`): `HttpApiSecurity.bearer`/`.apiKey` declares the scheme, `HttpApiBuilder.securityDecode` decodes the credential, and the `security` middleware verifies it (`jose` JWKS, argon2 API-key digest) inside the `Effect` rail.
- `@effect/rpc` + `@effect/cli` (catalogued at `libs/typescript/runtime/.api/`): `RpcGroup`/`RpcServer` is the second `serve` contribution family beside `HttpApiGroup`, and `@effect/cli` `Command` verbs compose `Terminal` — the same assembly law across HTTP, RPC, and CLI entry surfaces.
- `@effect/sql` + `@effect/cluster` (catalogued at `libs/typescript/data/.api/` and `libs/typescript/runtime/.api/`): both are built on these platform contracts (`Socket`, `FileSystem`) and expose their own `SqlClient`/`MessageStorage` Tags the app root satisfies with a `data` driver `Layer`.

[LOCAL_ADMISSION]:
- Use the abstract Tag (`FileSystem.FileSystem`, `HttpClient.HttpClient`, `Command.Command`, `Worker`) in domain code; never import `node:fs`/`node:child_process`/`undici`/`ws` directly — those bypass the runtime-portability, tracing, and error-rail contracts.
- Use `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` declarations with `HttpApiBuilder` handlers for HTTP surfaces; never hand-roll an `HttpRouter` route table where the declarative api gives the typed client and OpenAPI for free — reserve raw `HttpRouter`/`HttpLayerRouter` for non-API mounts (static assets, sync server).
- Use `HttpClient.retryTransient`/`.filterStatusOk`/`.mapRequest` as composed policy on the shared `net/client` client; never a bare `fetch` or a per-call retry loop.
- Use `Schema` decode on every ingress (`schemaBodyJson`, `schemaHeaders`, `schemaSearchParams`) and `Schema` encode on egress (`schemaJson`); never read `request.body` or build a response object untyped.
- Use `MsgPack.duplexSchema`/`Ndjson.duplexSchema` over `Socket.toChannel` for framed transport; never a raw socket event listener or a hand-written length-prefix parser.
- Use `PlatformConfigProvider.layerDotEnv`/`.layerFileTree` behind `Config`; never `process.env` reads in domain code.

[RAIL_LAW]:
- Package: `@effect/platform`
- Owns: the declarative HTTP-API family, `HttpClient`/`HttpServer`/`HttpRouter`/`HttpLayerRouter`, the web-value codecs (`HttpBody`/`Headers`/`Cookies`/`UrlParams`/`Url`/`Etag`/`Multipart`), the system-API Tags (`FileSystem`/`Path`/`KeyValueStore`/`Command`/`Terminal`/`Socket`/`Worker`), the frame codecs (`MsgPack`/`Ndjson`/`Transferable`/`Template`), and the config/logger/run-main boundary
- Accept: abstract Tags provided by a runtime `Layer`, `HttpApi*` declarations + `HttpApiBuilder` handlers, `HttpClient` policy transformers, `Schema` decode/encode at every boundary, `MsgPack`/`Ndjson` over `Socket.toChannel`, `PlatformConfigProvider` behind `Config`
- Reject: direct `node:*`/`undici`/`ws`/`fetch` imports in domain code, hand-rolled routers where a declarative `HttpApi` fits, untyped request/response bodies, raw socket listeners or length-prefix parsers, `process.env` reads outside the config provider
