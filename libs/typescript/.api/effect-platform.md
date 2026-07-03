# [TS_BRANCH_API_EFFECT_PLATFORM]

`@effect/platform` is the platform-neutral service-contract tier the branch composes for every host, wire, and edge boundary: the declarative HTTP-API family (`HttpApi`/`HttpApiGroup`/`HttpApiEndpoint` as data, `HttpApiBuilder` handlers, `HttpApiClient` derived typed SDKs, `OpenApi`/`HttpApiScalar` emission, `HttpApiSecurity`/`HttpApiMiddleware`), the request/response client (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`, `FetchHttpClient`), the server + router (`HttpServer`, `HttpRouter`, `HttpApp`, `HttpLayerRouter`, `HttpMiddleware`), the web-value codecs (`HttpBody`, `Headers`, `Cookies`, `UrlParams`, `Url`, `Etag`, `Multipart`, `HttpMethod`), the system-API contracts as `Context.Tag`s (`FileSystem`, `Path`, `KeyValueStore`, `Command`/`CommandExecutor`, `Terminal`, `Socket`/`SocketServer`, `Worker`/`WorkerRunner`), the frame codecs (`MsgPack`, `Ndjson`, `Transferable`, `Template`), and the config/logger boundary (`PlatformConfigProvider`, `PlatformLogger`, `Runtime`, `PlatformError`). Every contract is an abstract Tag with no runtime binding of its own — a per-runtime package (`platform-node`, `-bun`, `-browser`) provides the `Layer` — so a domain folder codes once against `HttpClient`/`FileSystem`/`Worker` and the app root picks the runtime.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform`
- package: `@effect/platform` (0.96.2, MIT, © Effectful Technologies)
- module format: ESM, `sideEffects: false`; per-module deep-import subpaths (`@effect/platform/HttpApi`, `@effect/platform/FileSystem`, …)
- runtime target: platform-neutral abstract contracts — no runtime binding; a `platform-node`/`-bun`/`-browser` `Layer` satisfies each Tag. `find-my-way-ts` (router match), `msgpackr` (`MsgPack`), and `multipasta` (`Multipart`) are the only bundled runtime deps
- peer: `effect@^3.21.4`
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); Tag contracts + `Schema`-typed endpoint declarations
- rail: platform contracts (host, wire, edge, store; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declarative HTTP-API — the contribution family `edge` assembles
- rail: boundaries

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------ | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `HttpApi<Groups, E, R>`                                  | api value          | `edge/api` — the app owns exactly one assembled `HttpApi`; groups are contributed data |
|  [02]   | `HttpApiGroup<Name, Endpoints>`                         | group             | domain folders contribute a `HttpApiGroup`; the god-contract is structurally impossible |
|  [03]   | `HttpApiEndpoint<Name, Method>`                         | endpoint          | one endpoint = `Schema` request + success + error; `.setPath`/`.addSuccess`/`.addError` builders |
|  [04]   | `HttpApiSchema.Multipart` / `.param` / `.withEncoding`  | payload modality  | `edge` multipart uploads, path-param schemas, non-JSON response encodings |
|  [05]   | `HttpApiSecurity.Bearer` / `.ApiKey` / `.Basic`         | auth scheme       | `security` — declared on the endpoint, decoded to a credential the middleware verifies |
|  [06]   | `HttpApiMiddleware.TagClass` / `HttpApiError.HttpApiDecodeError` | middleware / fault | `edge/middleware` typed middleware Tags; the built-in decode-failure error |
|  [07]   | `HttpApiClient` (derived) / `OpenApi.OpenAPISpec`        | client / spec     | `edge/emit` — one `HttpApi` yields the typed client SDK and the OpenAPI document; neither drifts |

[PUBLIC_TYPE_SCOPE]: client, server, and routing
- rail: boundaries

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------ | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `HttpClient.HttpClient`                                 | client Tag         | `host/net` default-policy client; `wire/invoke`, `ai/model`, telemetry OTLP compose it |
|  [02]   | `HttpClientRequest` / `HttpClientResponse`              | request / response | immutable request builder; response carries `Schema`-decoding accessors |
|  [03]   | `HttpClientError.RequestError` / `.ResponseError`       | client fault       | typed transport/decoding failures in the `Effect` error channel |
|  [04]   | `HttpServer.HttpServer` / `HttpApp.Default`             | server Tag / app   | `edge/serve` — the served value; a `platform-node`/`-bun` `Layer` binds the socket |
|  [05]   | `HttpRouter.HttpRouter` / `HttpLayerRouter.HttpRouter`  | router             | `edge` route table; `HttpLayerRouter` is the `Layer`-composable router for mixed HTTP + API mounts |
|  [06]   | `HttpServerRequest` / `HttpServerResponse`              | server io          | `edge` handlers; `HttpServerRespondable` lets a domain error render itself to a response |
|  [07]   | `HttpServerError.RouteNotFound` / `HttpMiddleware`      | server fault / mw  | `edge/problem` maps to RFC 9457; middleware transforms the handler `HttpApp` |

[PUBLIC_TYPE_SCOPE]: system-API contracts — abstract Tags a runtime `Layer` satisfies
- rail: system-apis

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CONSUMER]                                                          |
| :-----: | :---------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `FileSystem.FileSystem`                          | fs Tag         | `host`, `store/lane` — `readFileString`/`writeFile`/`watch`/`stream`/`makeTempDirectory` |
|  [02]   | `Path.Path`                                      | path Tag       | `host`, `iac` — platform-correct `join`/`resolve`/`basename`; `layerPosix`/`layerWin32` |
|  [03]   | `KeyValueStore.KeyValueStore` / `SchemaStore`    | kv Tag         | `store/lane`, `browser/persist` — `layerMemory`/`layerFileSystem`/`layerSchema`, `prefix` scope |
|  [04]   | `Command.Command` / `CommandExecutor.Process`    | subprocess     | `host/exec/process.ts` — declarative command with `pipeTo`, stream stdout, typed exit code |
|  [05]   | `Terminal.Terminal`                              | tty Tag        | `edge/cli` — line/key input + display for interactive verbs |
|  [06]   | `Socket.Socket` / `SocketServer`                 | socket         | `host/net/channel.ts`, `wire/transport` — framed duplex over TCP/WebSocket as an Effect `Channel` |
|  [07]   | `Worker.WorkerPool` / `WorkerRunner`             | worker         | `host/exec`, `browser/transport/pool.ts` — `Schema`-serialized off-thread request/response pools |
|  [08]   | `PlatformError` (`BadArgument` / `SystemError`)  | system fault   | the one error rail every system-API contract fails into; `wire/fault` classifies it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: declaring, handling, serving, and consuming an `HttpApi`
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `HttpApiEndpoint.get(name)(path)` / `.post` / `.del` … `.setPayload(schema)` / `.addSuccess` / `.addError` | declare endpoint | `edge` — one endpoint carries its request, success, and error `Schema`s |
|  [02]   | `HttpApiGroup.make(name).add(endpoint)` / `.addError` / `.prefix` / `.middleware(tag)`          | declare group  | domain folders build a group value; group-wide errors and middleware ride the declaration |
|  [03]   | `HttpApi.make(id).add(group)` / `.addError` / `.annotate` / `.middleware`                       | assemble api   | `edge/api` — the app assembles one `HttpApi` from selected groups |
|  [04]   | `HttpApiBuilder.group(api, name, (h) => h.handle(endpointName, handler))`                       | implement      | `edge` — bind each declared endpoint to an `Effect` handler; missing handler is a compile error |
|  [05]   | `HttpApiBuilder.api(api)` / `.serve(middleware?)` / `HttpApiBuilder.toWebHandler(layer)`        | serve          | `edge/serve` — the api `Layer`, the serve `Layer`, or a `Request => Response` web handler |
|  [06]   | `HttpApiBuilder.middlewareCors(options)` / `.middlewareOpenApi()` / `.securityDecode`           | api middleware | `edge/middleware` — CORS, the OpenAPI route, and security-scheme decode as builder layers |
|  [07]   | `HttpApiClient.make(api, { baseUrl, transformClient })`                                         | derive client  | `edge/emit`, any consumer — a fully typed SDK from the same `HttpApi` value |
|  [08]   | `OpenApi.fromApi(api)` / `HttpApiScalar.layer()` / `HttpApiSwagger.layer()`                     | docs           | `edge/emit` — the spec document and the interactive reference UI, derived not authored |

[ENTRYPOINT_SCOPE]: `HttpClient` — request policy and typed responses
- rail: system-apis

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `HttpClientRequest.get(url)` / `.post(url)` … `.setBody` / `.bodyJson` / `.bearerToken` / `.setUrlParams` | build request | `host/net`, `ai`, `wire/invoke` — immutable request value |
|  [02]   | `HttpClient.execute(request)` / `HttpClient.get(url, options)`                                  | dispatch       | returns `Effect<HttpClientResponse, HttpClientError, Scope>` |
|  [03]   | `HttpClient.retryTransient({ schedule })` / `.filterStatusOk` / `.followRedirects` / `.mapRequest` | policy       | `host/net` default-policy rows — retry idempotent requests, reject non-2xx, add auth headers |
|  [04]   | `HttpClient.withTracerPropagation` / `.withTracerDisabledWhen` / `.tapRequest`                  | observability  | `telemetry` — W3C trace propagation on egress, span control per request predicate |
|  [05]   | `HttpClientResponse.schemaBodyJson(schema)` / `.matchStatus({...})` / `.stream`                 | decode         | `wire`/`ai` — decode the body through one `Schema`; `matchStatus` dispatches on status class |
|  [06]   | `FetchHttpClient.layer` / `HttpClient.layerMergedContext`                                       | provide        | the default `fetch`-backed client `Layer`; `platform-node` swaps in the undici client |

[ENTRYPOINT_SCOPE]: server, router, and middleware
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER]                                                 |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `HttpRouter.empty` / `.get(path, handler)` / `.mountApp(prefix, app)` / `.use(middleware)`         | route          | `edge` — compose routes; `mountApp` mounts a sub-`HttpApp` (the EventLog sync server) |
|  [02]   | `HttpServerResponse.json(data)` / `.schemaJson(schema)(value)` / `.text` / `.stream` / `.file`     | respond        | `edge` handlers; `.setCookie`/`.setHeaders` decorate the response value |
|  [03]   | `HttpServerRequest.schemaBodyJson(schema)` / `.schemaHeaders` / `.schemaSearchParams` / `.upgrade` | ingress decode | `edge` — decode body/headers/query through `Schema`; `.upgrade` yields the WebSocket `Socket` |
|  [04]   | `HttpMiddleware.cors(options)` / `.logger` / `.xForwardedHeaders` / `.searchParamsParser`          | wrap           | `edge/middleware` — cross-cutting transforms on the handler `HttpApp` |
|  [05]   | `HttpLayerRouter.use` / `.addHttpApi(api)` / `.cors()` / `.serve(handler)` / `HttpLayerRouter.add` | layer route    | `edge` — the `Layer`-native router mixing raw HTTP routes with a mounted `HttpApi` |
|  [06]   | `HttpServer.serve(httpApp, middleware?)` / `HttpServer.layerTestClient`                            | run / test     | `edge/serve` binds the app to the runtime server `Layer`; `proof` uses the in-memory test client |

[ENTRYPOINT_SCOPE]: system-API contracts and frame codecs
- rail: system-apis

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Command.make(cmd, ...args)` / `.pipeTo(next)` / `.stream` / `.string` / `.exitCode` / `.env`   | subprocess     | `host/exec/process.ts` — declarative command pipelines, streamed output, typed exit |
|  [02]   | `KeyValueStore.layerFileSystem(dir)` / `.layerMemory` / `.layerSchema(schema)` / `.prefix(k)`   | kv store       | `store/lane`, `browser/persist` — schema-typed KV over fs/memory, prefix-scoped |
|  [03]   | `Worker.makePoolSerialized({ size })` / `WorkerRunner.layerSerialized(handlers)`                | worker pool    | `host/exec`, `browser/transport/pool.ts` — `Schema`-serialized request/response off-thread |
|  [04]   | `Socket.toChannel(socket)` / `Socket.makeWebSocket(url)` / `Socket.layerWebSocket`              | socket channel | `host/net/channel.ts`, `wire/transport` — duplex bytes as an Effect `Channel` with backpressure |
|  [05]   | `MsgPack.duplexSchema({ inputSchema, outputSchema })` / `Ndjson.duplexSchema` / `MsgPack.pack`  | frame codec    | `wire/codec` — `Schema`-typed MessagePack/NDJSON framing over a byte `Channel` |
|  [06]   | `Multipart.toPersisted(parts)` / `Multipart.schemaPersisted(schema)` / `.withLimits(opts)`      | upload         | `edge` — decode multipart form uploads to persisted files under size limits |
|  [07]   | `Transferable.schema(schema)` / `Template.make\`…\`` / `HttpBody.json` / `HttpBody.formData`     | payload        | `browser/transport` zero-copy transfer, `edge` HTML templating, request/response bodies |

[ENTRYPOINT_SCOPE]: config, logging, and process lifecycle boundary
- rail: system-apis

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `PlatformConfigProvider.fromDotEnv(path)` / `.layerDotEnv` / `.fromFileTree` / `.layerFileTree` | config source  | `host/config/provider.ts` — dotenv and file-tree (K8s secret mount) config providers |
|  [02]   | `PlatformLogger.toFile(path, { batchWindow })`                                                  | log sink       | `telemetry` — durable batched file logging behind the `Logger` service |
|  [03]   | `Runtime.makeRunMain(f)` / `Runtime.defaultTeardown`                                            | run-main       | the per-runtime `runMain` factory `platform-node`/`-bun` specialize for signal-draining exit |
|  [04]   | `Headers.redact(headers, keys)` / `Cookies.toCookieHeader` / `UrlParams.schemaStruct(schema)`   | web value      | `security` header redaction, `edge` cookie serialization, typed query-param decode |
|  [05]   | `Etag.layer` / `Etag.layerWeak` / `HttpServerResponse.setBody(HttpBody.fileWeb(file))`          | caching        | `edge/serve` static-asset ETag generation and immutable-asset responses |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_TOPOLOGY]:
- Every system contract is a `Context.Tag` with no built-in binding: `FileSystem.FileSystem`, `Path.Path`, `HttpClient.HttpClient`, `Command`/`CommandExecutor`, `Terminal`, `Socket`, `Worker`, `KeyValueStore`. A domain folder yields the Tag inside `Effect.gen` and never imports a runtime; the app root provides the `platform-node`/`-bun`/`-browser` `Layer`. This is what keeps `host`/`wire`/`edge` runtime-portable — a bun swap is a `Layer` selection, not a fork.
- The HTTP-API family is declaration-then-implementation: `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` are pure data carrying `Schema` request/success/error shapes; `HttpApiBuilder.group` binds each endpoint to an `Effect` handler with the handler set checked exhaustively against the declaration. One `HttpApi` value derives the server (`HttpApiBuilder.serve`), the typed client (`HttpApiClient.make`), and the OpenAPI document (`OpenApi.fromApi`) — spec, docs, client, and server cannot drift because they share one source.
- The client and server are `Schema`-symmetric: `HttpClientRequest`/`HttpServerRequest` decode inbound through `schemaBodyJson`, `HttpClientResponse`/`HttpServerResponse` encode outbound through `schemaJson`, and both fail into a tagged error family (`HttpClientError`, `HttpServerError`, `HttpApiDecodeError`) that flows the `Effect` error channel to the `edge/problem` RFC 9457 mapping.
- `HttpLayerRouter` is the `Layer`-composable server: routes and middleware are `Layer`s, `addHttpApi` mounts a full declarative api beside raw routes, and the whole server is one `Layer` the app root provides — the model `edge/serve` uses when an app mixes an `HttpApi`, an EventLog sync `HttpApp`, and static assets under one server.
- Frame codecs (`MsgPack`, `Ndjson`) and `Socket` compose as Effect `Channel`s: `Socket.toChannel` turns a duplex connection into a byte channel, and `MsgPack.duplexSchema`/`Ndjson.duplexSchema` layer a `Schema`-typed message channel over it — the `wire` transport rail is decode-typed end to end with backpressure, never a raw event emitter.
- `Worker`/`WorkerRunner` are `Schema`-serialized RPC over a thread/process boundary: the pool sends a decoded request and awaits a decoded response, so `browser/transport/pool.ts` moves content-key verification off-thread and `host/exec` runs CPU-heavy work in a pool with the same typed contract as an in-process call.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): every contract is an `Effect`-returning service keyed by `Context.Tag`; endpoint payloads, request bodies, and response bodies are `Schema`; middleware and client policy are effect transformers. The platform tier adds no new rail — it is `effect` applied to the boundary.
- `@effect/platform-node` (`.api/effect-platform-node.md`): `NodeContext.layer` satisfies `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker` in one Layer; `NodeHttpServer.layer` binds `HttpServer`; `NodeHttpClient.layerUndici` binds `HttpClient`. `@effect/platform-bun` (`.api/effect-platform-bun.md`) and `-browser` (`.api/effect-platform-browser.md`) are peer swaps behind the same Tags.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): `HttpClient.withTracerPropagation` and `HttpMiddleware` inject/extract W3C trace context; the OTel `Layer` under the graph exports every server span and client egress span with no handler change — `telemetry/otlp/context.ts` owns the extract-and-continue seam.
- `security` folder (`arctic`/`jose`/`@node-rs/argon2`, catalogued at `libs/typescript/security/.api/`): `HttpApiSecurity.bearer`/`.apiKey` declares the scheme, `HttpApiBuilder.securityDecode` decodes the credential, and the `security` middleware verifies it (`jose` JWKS, argon2 API-key digest) inside the `Effect` rail.
- `@effect/rpc` + `@effect/cli` (catalogued at `libs/typescript/edge/.api/`): `RpcGroup`/`RpcServer` is the second `edge` contribution family beside `HttpApiGroup`, and `@effect/cli` `Command` verbs compose `Terminal` — the same assembly law across HTTP, RPC, and CLI entry surfaces.
- `@effect/sql` + `@effect/cluster` (catalogued at `libs/typescript/store|work/.api/`): both are built on these platform contracts (`Socket`, `FileSystem`) and expose their own `SqlClient`/`MessageStorage` Tags the app root satisfies with a `store` driver `Layer`.

[LOCAL_ADMISSION]:
- Use the abstract Tag (`FileSystem.FileSystem`, `HttpClient.HttpClient`, `Command.Command`, `Worker`) in domain code; never import `node:fs`/`node:child_process`/`undici`/`ws` directly — those bypass the runtime-portability, tracing, and error-rail contracts.
- Use `HttpApiEndpoint`/`HttpApiGroup`/`HttpApi` declarations with `HttpApiBuilder` handlers for HTTP surfaces; never hand-roll an `HttpRouter` route table where the declarative api gives the typed client and OpenAPI for free — reserve raw `HttpRouter`/`HttpLayerRouter` for non-API mounts (static assets, sync server).
- Use `HttpClient.retryTransient`/`.filterStatusOk`/`.mapRequest` as composed policy on the shared `host/net` client; never a bare `fetch` or a per-call retry loop.
- Use `Schema` decode on every ingress (`schemaBodyJson`, `schemaHeaders`, `schemaSearchParams`) and `Schema` encode on egress (`schemaJson`); never read `request.body` or build a response object untyped.
- Use `MsgPack.duplexSchema`/`Ndjson.duplexSchema` over `Socket.toChannel` for framed transport; never a raw socket event listener or a hand-written length-prefix parser.
- Use `PlatformConfigProvider.layerDotEnv`/`.layerFileTree` behind `Config`; never `process.env` reads in domain code.

[RAIL_LAW]:
- Package: `@effect/platform`
- Owns: the declarative HTTP-API family, `HttpClient`/`HttpServer`/`HttpRouter`/`HttpLayerRouter`, the web-value codecs (`HttpBody`/`Headers`/`Cookies`/`UrlParams`/`Url`/`Etag`/`Multipart`), the system-API Tags (`FileSystem`/`Path`/`KeyValueStore`/`Command`/`Terminal`/`Socket`/`Worker`), the frame codecs (`MsgPack`/`Ndjson`/`Transferable`/`Template`), and the config/logger/run-main boundary
- Accept: abstract Tags provided by a runtime `Layer`, `HttpApi*` declarations + `HttpApiBuilder` handlers, `HttpClient` policy transformers, `Schema` decode/encode at every boundary, `MsgPack`/`Ndjson` over `Socket.toChannel`, `PlatformConfigProvider` behind `Config`
- Reject: direct `node:*`/`undici`/`ws`/`fetch` imports in domain code, hand-rolled routers where a declarative `HttpApi` fits, untyped request/response bodies, raw socket listeners or length-prefix parsers, `process.env` reads outside the config provider
