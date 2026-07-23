# [TS_BRANCH_API_EFFECT_PLATFORM]

`@effect/platform` mints the platform-neutral service-contract tier every host, wire, and serve boundary composes: the declarative HTTP-API family whose one contract value derives server, typed client, and OpenAPI spec; the system-API contracts as abstract `Context.Tag`s; and the frame codecs typing socket and worker transport end to end. Every contract binds no runtime of its own — a per-runtime package (`platform-node`, `-bun`, `-browser`) satisfies the `Layer` — so a domain folder codes once against `HttpClient`/`FileSystem`/`Worker` and the app root picks the runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform`
- package: `@effect/platform` (MIT)
- module: ESM + CJS dual (`dist/esm`/`dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`@effect/platform/HttpApi`, `/FileSystem`, …)
- runtime: platform-neutral abstract contracts — no runtime binding; a `platform-node`/`-bun`/`-browser` `Layer` satisfies each Tag
- depends: `find-my-way-ts` (router match), `msgpackr` (`MsgPack`), `multipasta` (`Multipart`) bundled; peer `effect`
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`) — Tag contracts + `Schema`-typed endpoint declarations
- rail: platform contracts — proc, net, serve, data

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declarative HTTP-API — the contribution family `serve/api` assembles
- rail: boundaries

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]    | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `HttpApi<Groups, E, R>`                                | api value        | `serve/api` — one assembled api             |
|  [02]   | `HttpApiGroup<Name, Endpoints>`                        | group            | `serve` — domain folders contribute a group |
|  [03]   | `HttpApiEndpoint<Name, Method>`                        | endpoint         | `serve` — request, success, error `Schema`  |
|  [04]   | `HttpApiSchema.Multipart` / `.param` / `.withEncoding` | payload modality | `serve` — uploads, path params, encodings   |
|  [05]   | `HttpApiSecurity.Bearer` / `.ApiKey` / `.Basic`        | auth scheme      | `security` — endpoint-declared credential   |
|  [06]   | `HttpApiMiddleware.Tag` / `.TagClass.BaseSecurity`     | middleware       | `serve/api` — typed middleware Tags         |
|  [07]   | `HttpApiClient` (derived) / `OpenApi.OpenAPISpec`      | client / spec    | `serve/api` — typed SDK, OpenAPI doc        |

[PUBLIC_TYPE_SCOPE]: client, server, and routing
- rail: boundaries

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]      | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `HttpClient.HttpClient`                                | client Tag         | `net/client` — default-policy client      |
|  [02]   | `HttpClientRequest` / `HttpClientResponse`             | request / response | `net` — request builder, decode access    |
|  [03]   | `HttpClientError.RequestError` / `.ResponseError`      | client fault       | `net` — transport, decode faults          |
|  [04]   | `HttpServer.HttpServer` / `HttpApp.Default`            | server Tag / app   | `serve/route` — runtime `Layer` binds it  |
|  [05]   | `HttpRouter.HttpRouter` / `HttpLayerRouter.HttpRouter` | router             | `serve/route` — route table, mixed mounts |
|  [06]   | `HttpServerRequest` / `HttpServerResponse`             | server io          | `serve` handlers — self-rendering errors  |
|  [07]   | `HttpServerError.RouteNotFound` / `HttpMiddleware`     | server fault / mw  | `serve/problem` — RFC 9457 mapping        |

[PUBLIC_TYPE_SCOPE]: system-API contracts — abstract Tags a runtime `Layer` satisfies
- rail: system-apis
- `FileSystem` mints scoped temp paths whose deletion ties to the `Scope`; `Command`'s `Process` exposes `stdin` as a `Sink`, `stdout`/`stderr` as `Stream`, `exitCode` as an `Effect`; `Worker.WorkerManager`/`Worker.Spawner`/`WorkerRunner.PlatformRunner`/`Socket.WebSocketConstructor` are the runtime-provided Tags the bindings satisfy.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `FileSystem.FileSystem`                         | fs Tag        | `proc`, `data/lane`                     |
|  [02]   | `Path.Path`                                     | path Tag      | `proc`, `iac`                           |
|  [03]   | `KeyValueStore.KeyValueStore` / `SchemaStore`   | kv Tag        | `data/lane`, `browser/persist`          |
|  [04]   | `Command.Command` / `CommandExecutor.Process`   | subprocess    | `proc/exec` — declarative               |
|  [05]   | `Terminal.Terminal`                             | tty Tag       | `serve/cli` — line/key input, display   |
|  [06]   | `Socket.Socket` / `SocketServer`                | socket        | `net/channel`, `core/interchange/frame` |
|  [07]   | `Worker.WorkerPool` / `WorkerRunner`            | worker        | `proc/worker`, `browser/fetch` pools    |
|  [08]   | `PlatformError` (`BadArgument` / `SystemError`) | system fault  | `core/interchange/codec` — one rail     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: declaring, handling, serving, and consuming an `HttpApi`
- rail: boundaries
- consumer: `serve/api` unless the cell names another rail
- `HttpApiEndpoint.get`/`.post`/`.del`: `.setPath` `.setPayload(schema)` `.addSuccess` `.addError`. `HttpApiGroup.make`: `.add(endpoint)` `.addError` `.prefix` `.middleware(tag)`. `HttpApi.make`: `.add(group)` `.addError` `.annotate` `.middleware`. `HttpApiBuilder.group(api, name, h => h.handle(name, handler))`, `.api(api)`, `.serve(middleware?)`, `toWebHandler(api, options)`, `.middlewareCors(options)`, `.middlewareOpenApi()`, `.securityDecode`. `HttpApiClient.make(api, { baseUrl, transformClient })`; `OpenApi.fromApi(api)`, `HttpApiScalar.layer()`, `HttpApiSwagger.layer()`.

| [INDEX] | [SURFACE]                                              | [SHAPE]          | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `HttpApiEndpoint.get` / `.post` / `.del`               | declare endpoint | path, request, success, error `Schema`s                    |
|  [02]   | `HttpApiGroup.make(name)`                              | declare group    | domain folders build a group; errors + mw ride it          |
|  [03]   | `HttpApi.make(id)`                                     | assemble api     | one `HttpApi` from selected groups                         |
|  [04]   | `HttpApiBuilder.group(...)`                            | implement        | bind each endpoint; a missing handler won't compile        |
|  [05]   | `HttpApiBuilder.serve`                                 | serve            | `serve/route` — api `Layer`, serve `Layer`, or web handler |
|  [06]   | `HttpApiBuilder.middlewareCors`                        | api middleware   | CORS, OpenAPI route, security decode                       |
|  [07]   | `HttpApiClient.make`                                   | derive client    | typed SDK from the `HttpApi` value                         |
|  [08]   | `OpenApi.fromApi` / `HttpApiScalar` / `HttpApiSwagger` | docs             | spec document + reference UI                               |

[ENTRYPOINT_SCOPE]: `HttpClient` — request policy and typed responses
- rail: system-apis
- `HttpClientRequest.get`/`.post`: `.setBody` `.bodyJson` `.bearerToken` `.setUrlParams`. `HttpClient.execute(request): Effect<HttpClientResponse, HttpClientError, Scope>`; policy `.retryTransient({ schedule })` `.filterStatusOk` `.followRedirects` `.mapRequest`; observability `.withTracerPropagation` `.withTracerDisabledWhen` `.tapRequest`. `HttpClientResponse.schemaJson(schema)` `.matchStatus({...})` `.stream` decode the body.

| [INDEX] | [SURFACE]                                                    | [SHAPE]       | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `HttpClientRequest.get` / `.post`                            | build request | `net/client`, `ai` — immutable request |
|  [02]   | `HttpClient.execute` / `.get`                                | dispatch      | typed response in `Effect` channel     |
|  [03]   | `HttpClient.retryTransient` / `.filterStatusOk`              | policy        | `net/client` — retry, filter, auth     |
|  [04]   | `HttpClient.withTracerPropagation` / `.tapRequest`           | observability | `otel` — egress propagation            |
|  [05]   | `HttpClientResponse.schemaJson` / `.matchStatus` / `.stream` | decode        | `core/interchange`, `ai` — decode body |
|  [06]   | `FetchHttpClient.layer` / `HttpClient.layerMergedContext`    | provide       | `net` — `fetch` `Layer`, undici swap   |

[ENTRYPOINT_SCOPE]: server, router, and middleware
- rail: boundaries
- `HttpRouter`: `.empty` `.get(path, handler)` `.mountApp(prefix, app)` `.use(middleware)`. `HttpServerResponse`: `.json(data)` `.schemaJson(schema)(value)` `.text` `.stream` `.file` `.setCookie` `.setHeaders`. `HttpServerRequest`: `.schemaBodyJson(schema)` `.schemaHeaders` `.schemaSearchParams` `.upgrade`. `HttpMiddleware`: `.cors(options)` `.logger` `.xForwardedHeaders` `.searchParamsParser`.
- `HttpLayerRouter`: `.use` `.add(method, path, handler)` `.addAll` `.addHttpApi(api, { openapiPath? })` `.middleware` `.cors()` `.disableLogger` `.serve` `.toWebHandler` `.params` `.schemaJson` `.schemaPathParams`. `HttpMultiplex`: `.make` `.empty` `.add(predicate, app)` `.headerExact` `.headerRegex` `.headerStartsWith` `.hostExact` `.hostRegex`. `HttpServerRespondable`: `.symbol` `.toResponse` `.toResponseOrElse` `.isRespondable`. `ChannelSchema`: `.make` `.duplex` `.duplexUnknown({ inputSchema, outputSchema })`.
- `HttpApiScalar.layer({ path? })` `.layerCdn` `.layerHttpLayerRouter({ api?, path? })` and `HttpApiSwagger.layer({ path })` mount the docs UI. `HttpApiError` prebuilt status faults: `.HttpApiDecodeError` `.BadRequest` `.Unauthorized` `.Forbidden` `.NotFound` `.Conflict` `.InternalServerError` `.ServiceUnavailable`.

| [INDEX] | [SURFACE]                               | [SHAPE]        | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `HttpRouter`                            | route          | `serve/route` — compose routes; `mountApp` mounts a sub-`HttpApp`            |
|  [02]   | `HttpServerResponse`                    | respond        | `serve` handlers; cookie/header decorators on the response value             |
|  [03]   | `HttpServerRequest`                     | ingress decode | `serve` — body/header/query decode; `.upgrade` yields the WebSocket `Socket` |
|  [04]   | `HttpMiddleware`                        | wrap           | `serve/route` — cross-cutting transforms on the handler `HttpApp`            |
|  [05]   | `HttpLayerRouter`                       | layer route    | `serve/route` — `Layer`-native router; raw routes + an `HttpApi`             |
|  [06]   | `HttpServer.serve` / `.layerTestClient` | run / test     | `serve/route` binds the app to the server `Layer`; test client               |
|  [07]   | `HttpMultiplex`                         | multiplex      | `serve/route` — host/header dispatch across several `HttpApp`s               |
|  [08]   | `HttpServerRespondable`                 | self-render    | `serve/problem` — a domain value self-renders; outbound-fault law            |
|  [09]   | `HttpApiScalar` / `HttpApiSwagger`      | docs ui        | `serve/api` — Scalar reference UI beside the derived OpenAPI route           |
|  [10]   | `HttpApiError` faults                   | status faults  | status-tagged endpoint errors; `serve/problem` folds escaped ones            |
|  [11]   | `ChannelSchema`                         | typed channel  | `serve/live` — `Schema`-typed bidirectional `Channel`                        |

[ENTRYPOINT_SCOPE]: system-API contracts and frame codecs
- rail: system-apis
- `Command`: `.make(cmd, ...args)` `.pipeTo(next)` `.stream` `.string` `.exitCode` `.env`. `KeyValueStore`: `.layerFileSystem(dir)` `.layerMemory` `.layerSchema(schema)` `.prefix(k)`. `Worker`: `.makePool` `.makePoolLayer` (`Layer` form over a `Spawner`) `.makePoolSerialized({ size })` with `WorkerRunner.layerSerialized(handlers)`. `Socket`: `.toChannel(socket)` `.makeWebSocket(url)` `.layerWebSocket`.
- `MsgPack.duplexSchema({ inputSchema, outputSchema })` `.pack` and `Ndjson.duplexSchema` `.duplex` `.duplexString` frame `Schema`-typed messages over a byte `Channel`. `Multipart`: `.toPersisted(parts)` `.schemaPersisted(schema)` `.withLimits(opts)` `.withLimitsStream(parts, opts)` — `withLimits.Options` carries `Option`-shaped `maxParts`/`maxFileSize` beside `maxFieldSize`/`maxTotalSize`. `Transferable.schema(schema)`, `Template.make\`…\``, `HttpBody.json`/`.formData`.

| [INDEX] | [SURFACE]                                | [SHAPE]        | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Command`                                | subprocess     | `proc/exec` — command pipelines, streamed, typed exit                  |
|  [02]   | `KeyValueStore`                          | kv store       | `data/lane`, `browser/persist` — schema-typed KV, prefix-scoped        |
|  [03]   | `Worker` / `WorkerRunner`                | worker pool    | `proc/worker`, `browser/fetch` — `Schema`-serialized off-thread pools  |
|  [04]   | `Socket`                                 | socket channel | `net/channel`, `core/interchange/frame` — duplex bytes as a `Channel`  |
|  [05]   | `MsgPack` / `Ndjson`                     | frame codec    | `core/interchange/codec`, `serve/live` — MsgPack/NDJSON framing        |
|  [06]   | `Multipart`                              | upload         | `serve/api` — decode multipart uploads to persisted files under limits |
|  [07]   | `Transferable` / `Template` / `HttpBody` | payload        | `browser/fetch` transfer, `serve` templating, request/response bodies  |

[ENTRYPOINT_SCOPE]: config, logging, and process lifecycle boundary
- rail: system-apis
- `PlatformConfigProvider`: `.fromDotEnv(path)` `.layerDotEnv` `.fromFileTree` `.layerFileTree`. `PlatformLogger.toFile(path, { batchWindow })`; `Runtime`: `.makeRunMain(f)` `.RunMain` `.defaultTeardown`. `Headers.redact(headers, keys)`, `Cookies.toCookieHeader`, `UrlParams.schemaStruct(schema)`. `Etag`: `.Generator` `.layer` `.layerWeak` with `HttpServerResponse.setBody(HttpBody.fileWeb(file))`.

| [INDEX] | [SURFACE]                           | [SHAPE]       | [CAPABILITY]                                                                   |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `PlatformConfigProvider`            | config source | `proc/config` — dotenv + file-tree (K8s secret mount) providers                |
|  [02]   | `PlatformLogger`                    | log sink      | `otel` — durable batched file logging behind `Logger`                          |
|  [03]   | `Runtime`                           | run-main      | each runtime's `runMain` type; `-node`/`-bun`/`-browser` drain signals on exit |
|  [04]   | `Headers` / `Cookies` / `UrlParams` | web value     | `security` redaction, `serve` cookie serialization, typed query-param decode   |
|  [05]   | `Etag`                              | caching       | `serve/route` static-asset ETag generation and immutable-asset responses       |

## [04]-[MEMBER_SIGNATURES]

Shipped declarations for the platform members whose call shape and behavioral contract a roster cell cannot carry — owning module, generic parameters, parameter lists, return types.

[SIGNATURE_SCOPE]: `Cookies` — construction, collection, and header rendering
- rail: boundaries

[COOKIE]: `Cookie.name: string` `Cookie.value: string` `Cookie.valueEncoded: string` `Cookie.options: {…}`
[COOKIE_OPTIONS]: `domain` `expires` `maxAge` `path` `priority` `httpOnly` `secure` `partitioned` `sameSite`
[SURFACES]: `makeCookie(string,string,Cookie["options"]|undefined?) -> Either.Either<Cookie,CookiesError>` `unsafeMakeCookie(string,string,Cookie["options"]|undefined?) -> Cookie` `fromIterable(Iterable<Cookie>) -> Cookies` `toCookieHeader(Cookies) -> string` `toSetCookieHeaders(Cookies) -> Array<string>`

- `makeCookie` returns `Either` — a refused name/value/attribute is a `CookiesError`; the write edge lifts it `Effect.orDie`, and `unsafeMakeCookie` is the throw-on-refusal twin for boot-edge literals.
- `Cookie["options"]` is the whole attribute vocabulary the codec renders.
- `toSetCookieHeaders` is plural, one `Set-Cookie` per cookie; `toCookieHeader` is the request-side single-header join. `Cookies` owns `Set-Cookie` serialization.

[SIGNATURE_SCOPE]: `Headers.redact` — the log-path mask
- rail: boundaries

[REDACT]: `redact.call(string|RegExp|ReadonlyArray<string|RegExp>) -> (self:Headers)=>Record<string,string|Redacted.Redacted>` `redact.call(Headers,string|RegExp|ReadonlyArray<string|RegExp>) -> Record<string,string|Redacted.Redacted>`

- `redact` replaces matched values with `Redacted` carriers, so a logged header bag prints `<redacted>` for matched keys with zero call-site masking.

[SIGNATURE_SCOPE]: `HttpApiSecurity.bearer` / `HttpApiMiddleware.Tag` / `HttpApiBuilder.securityDecode` — the declarative guard seam
- rail: services-and-layers

[BEARER]: `Bearer._tag: "Bearer"`
[TAG_OPTIONS]: `optional` `failure` `provides` `security`
[SURFACES]: `bearer: Bearer` `Tag() -> <Name,Options>(string,Options?) -> TagClass<Self,Name,Options>` `securityDecode(Security) -> Effect<HttpApiSecurity.Type<Security>,never,HttpServerRequest|ParsedSearchParams>`

- `HttpApiMiddleware.Tag`'s `security` record keys the credential decoders the implementation receives — `{ bearer: HttpApiSecurity.bearer }` hands it a `bearer: (token: Redacted<string>) => Effect` slot, so the scheme grammar carries the decode rather than a bare `Context.Tag`.
- `securityDecode` never fails — an absent credential decodes to its scheme's empty carrier, so refusal is the guard implementation's verdict; a cookie-scheme guard decodes through the same member over `HttpApiSecurity.apiKey`'s cookie variant.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every system contract is a `Context.Tag` with no built-in binding: a domain folder yields the Tag inside `Effect.gen` and never imports a runtime, and the app root binds the `platform-node`/`-bun`/`-browser` `Layer`. Runtime portability follows — a bun swap is a `Layer` selection, not a fork.
- HTTP-API family runs declaration-then-implementation: `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` are pure data carrying `Schema` request/success/error shapes; `HttpApiBuilder.group` binds each endpoint to an `Effect` handler, exhaustively checked against the declaration. One `HttpApi` value derives the server (`HttpApiBuilder.serve`), the typed client (`HttpApiClient.make`), and the OpenAPI document (`OpenApi.fromApi`) from one source, so spec, docs, client, and server never drift.
- Client and server are `Schema`-symmetric: `HttpClientRequest`/`HttpServerRequest` decode inbound through `schemaBodyJson`, `HttpClientResponse`/`HttpServerResponse` encode outbound through `schemaJson`, and both fail into a tagged error family (`HttpClientError`, `HttpServerError`, `HttpApiDecodeError`) that flows the `Effect` error channel to the `serve/problem` RFC 9457 mapping.
- `HttpLayerRouter` is the `Layer`-composable server: routes and middleware are `Layer`s, `addHttpApi` mounts a full declarative api beside raw routes, and the whole server is one `Layer` the app root binds — the model `serve/route` uses when an app mixes an `HttpApi`, an EventLog sync `HttpApp`, and static assets under one server.
- Frame codecs (`MsgPack`, `Ndjson`) and `Socket` compose as Effect `Channel`s: `Socket.toChannel` turns a duplex connection into a byte channel, and `MsgPack.duplexSchema`/`Ndjson.duplexSchema` layer a `Schema`-typed message channel over it — the framed transport rail is decode-typed end to end with backpressure, never a raw event emitter.
- `Worker`/`WorkerRunner` are `Schema`-serialized RPC over a thread/process boundary: the pool sends a decoded request and awaits a decoded response, so `browser/fetch` moves content-key verification off-thread and `proc/worker` runs CPU-heavy work in a pool with the same typed contract as an in-process call.

[STACKING]:
- `effect`(`.api/effect.md`): every contract is an `Effect`-returning service keyed by `Context.Tag`; endpoint payloads, request/response bodies are `Schema`; middleware and client policy are effect transformers. Platform tier adds no new rail — `effect` applied to the boundary.
- `@effect/platform-node`(`.api/effect-platform-node.md`): `NodeContext.layer` satisfies `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker` in one Layer, `NodeHttpServer.layer` binds `HttpServer`, `NodeHttpClient.layerUndici` binds `HttpClient`. `@effect/platform-bun`(`.api/effect-platform-bun.md`) and `-browser`(`.api/effect-platform-browser.md`) are peer swaps behind the same Tags.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): `HttpClient.withTracerPropagation` and `HttpMiddleware` inject the W3C `traceparent`, `Tracer.makeExternalSpan`/`withSpanContext` continue an inbound one into an Effect parent span, and `Otlp.layer` under the graph exports every server and client-egress span over the shared `HttpClient` with no handler change; `otel/emit` owns the extract-and-continue seam.
- `security`(`security/.api/jose.md`, `arctic.md`, `node-rs-argon2.md`): `HttpApiSecurity.bearer`/`.apiKey` declares the endpoint scheme, `HttpApiBuilder.securityDecode` decodes the credential from `HttpServerRequest`, and the `security` middleware verifies it (`jose` JWKS validation, `@node-rs/argon2` API-key digest) inside the `Effect` rail.
- `@effect/rpc`(`runtime/.api/effect-rpc.md`): `RpcGroup`/`RpcServer` is the second `serve` contribution family beside `HttpApiGroup`, served over a `Protocol` Layer built on `HttpRouter`/`HttpApp`/`Socket`/`Worker`. `@effect/cli`(`runtime/.api/effect-cli.md`): a `Command` value composes the `Terminal`/`FileSystem`/`Path` `Environment`, `Command.run` the argv boundary — one assembly law across HTTP, RPC, and CLI surfaces.
- `@effect/sql`(`data/.api/effect-sql.md`) and `@effect/cluster`(`runtime/.api/effect-cluster.md`) build on these contracts (`Socket`, `FileSystem`) and expose `SqlClient`/`MessageStorage` Tags the app root satisfies with a `store` driver `Layer`; `SqlMessageStorage.layer` binds the cluster message store onto the SQL spine.

[LOCAL_ADMISSION]:
- Domain code yields the abstract Tag (`FileSystem.FileSystem`, `HttpClient.HttpClient`, `Command.Command`, `Worker`); the runtime binding stays at the app root, preserving portability, tracing, and the error rail.
- HTTP surfaces declare `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` with `HttpApiBuilder` handlers for the free typed client and OpenAPI; raw `HttpRouter`/`HttpLayerRouter` serves only non-API mounts (static assets, sync server).
- `HttpClient.retryTransient`/`.filterStatusOk`/`.mapRequest` compose as policy on the shared `net/client` client.
- `Schema` gates every edge — `schemaBodyJson`/`schemaHeaders`/`schemaSearchParams` on ingress, `schemaJson` on egress.
- `MsgPack.duplexSchema`/`Ndjson.duplexSchema` over `Socket.toChannel` frames transport; `PlatformConfigProvider.layerDotEnv`/`.layerFileTree` behind `Config` sources env.

[RAIL_LAW]:
- Package: `@effect/platform`
- Owns: the declarative HTTP-API family, `HttpClient`/`HttpServer`/`HttpRouter`/`HttpLayerRouter`, the web-value codecs (`HttpBody`/`Headers`/`Cookies`/`UrlParams`/`Url`/`Etag`/`Multipart`), the system-API Tags (`FileSystem`/`Path`/`KeyValueStore`/`Command`/`Terminal`/`Socket`/`Worker`), the frame codecs (`MsgPack`/`Ndjson`/`Transferable`/`Template`), and the config/logger/run-main boundary
- Accept: abstract Tags provided by a runtime `Layer`, `HttpApi*` declarations + `HttpApiBuilder` handlers, `HttpClient` policy transformers, `Schema` decode/encode at every boundary, `MsgPack`/`Ndjson` over `Socket.toChannel`, `PlatformConfigProvider` behind `Config`
- Reject: direct `node:*`/`undici`/`ws`/`fetch` imports in domain code, hand-rolled routers where a declarative `HttpApi` fits, untyped request/response bodies, raw socket listeners or length-prefix parsers, `process.env` reads outside the config provider
