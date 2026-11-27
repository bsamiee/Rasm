# [HTTP]

## [01]-[CONTRACT]

One `HttpApi` of groups and endpoints derives server routes, OpenAPI document, typed client, and URL builder:

```ts
const draftEtag = { 'draft-etag': Revision };
const Api = HttpApi.make('portfolio')
    .add(
        HttpApiGroup.make('documents')
            .add(
                HttpApiEndpoint.get('draft', '/draft', { success: HttpApiSchema.WithHeaders(PortfolioData, draftEtag) }),
                HttpApiEndpoint.put('save', '/draft', { payload: StoredPortfolio, headers: { 'if-match': Revision }, success: HttpApiSchema.WithHeaders(HttpApiSchema.NoContent, draftEtag), error: [PreconditionFailed, PlacementConflict] }),
            )
            .middleware(Owner),
        HttpApiGroup.make('assets')
            .add(
                HttpApiEndpoint.post('sign', '/assets/signatures', { payload: Presignable, success: Presigned }),
                HttpApiEndpoint.delete('delete', '/assets/:id', { params: { id: AssetId }, error: HttpApiError.ConflictNoContent }),
            )
            .middleware(Owner)
            .add(HttpApiEndpoint.get('read', '/assets/:id/:width?', { params: { id: AssetId, width: Schema.OptionFromOptionalKey(Schema.FiniteFromString.check(Schema.isInt())) }, error: HttpApiError.NotFoundNoContent })),
    )
    .prefix('/api');
```

`PortfolioData`, `StoredPortfolio`, `AssetId`, `Presignable`, and `Presigned` are domain schemas, `Revision` decodes a quoted ETag to its revision number, `PreconditionFailed` and `PlacementConflict` are `Schema.TaggedError` classes annotated with `httpApiStatus`, and `Owner` is an `HttpApiMiddleware.Service`. `read` joins `assets` after `.middleware(Owner)`, so published files serve without the owner check. Endpoint slots take domain schemas, and each endpoint derives its wire codecs:
- String slots (params, query, headers, forms) take `Schema.toCodecStringTree`, JSON bodies `toCodecJson`, `disableCodecs: true` neither
- Query, form, and bodiless payload fields encode flat, an object field dropping, so a structured value takes `Schema.fromJsonString(S)`
- Payloads on `GET`, `HEAD`, `OPTIONS`, and `TRACE` decode from the query string, a fields object on a body method as a form, a schema as JSON
- `HttpApiSchema.asText`, `asUint8Array`, `asFormUrlEncoded`, `asMultipart`, and `asJson({ contentType })` set another body encoding
- `payload`, `success`, and `error` take one schema or an array, the server choosing a payload by content type and the client a response by status
- Requests without `content-type` decode as JSON, and an unlisted content type answers 415
- `success` defaults to `HttpApiSchema.NoContent`, `status('Created')` sets a status by literal, `Empty(code)` and `Created` are bodiless
- `HttpApiSchema.WithHeaders(S, headers)` types headers handlers set through `withHeaders({ body, headers })` and clients read as `{ body, headers }`
- `.middleware(M)`, `.prefix(p)`, and `annotateEndpoints` reach endpoints and groups added before the call, and `.annotate` reaches every one
- `HttpApi.ParseOptions` annotated `{ onExcessProperty: 'error' }` refuses unknown fields, a slot class (`QueryParseOptions`) one slot
- `HttpApi.RedactedNames` annotated `{ query, headers, params }` redacts those values in spans and request logs, sending them unchanged, and a slot field typed `Schema.Redacted` needs no listing
- `HttpApiGroup.make('handles', { topLevel: true })` places a group's methods at the client root, and `.addHttpApi(other)` merges another API's groups

## [02]-[ERRORS]

HttpApi encodes each declared error with its status and decodes it back to the same class on the client, and an undeclared error renders itself when it implements `HttpServerRespondable` and dies as a defect otherwise:
- `HttpApiError.<Status>.make()` sends a `{ _tag }` body, and `<Status>NoContent` an empty body clients decode to the class
- Handlers keep each domain reason as its declared class and turn an infrastructure failure into a logged `HttpApiError.ServiceUnavailable`
- `HttpApiError.Problem<X>()(tag, { type, status, title }, fields)` declares an RFC 9457 error, `type` keeping classes that share a status apart
- `HttpApiSchema.asProblem({ title, type })` renders an existing error class as a problem body
- `HttpApiMiddleware.ProblemDetails` on the API with `layerProblemDetails({ includeDetails: true })` answers decode failures per path
- `HttpApiMiddleware.layerSchemaErrorTransform(M, transform)` maps decode failures to a response or a declared error of `M`
- `HttpApiSchemaError.kind` names the failing slot: `Params`, `Query`, `Headers`, and `Payload` answer 400, `Body` and `ResponseHeaders` 500
- Request-slot failures and built-in or `Problem` errors below 500 skip the `ErrorReporter`, as do classes setting `[ErrorReporter.ignore]`

## [03]-[MIDDLEWARE]

Middleware is a service keyed per API, group, or endpoint that wraps decoding and the handler. Middleware with no `security` is one function `(effect, { endpoint, group }) => Effect`, and a guard captures at build every service its per-request check reads beside the request:

```ts
const OwnerLive = Layer.effect(
    Owner,
    Effect.map(Effect.context<Exclude<Effect.Services<typeof authorize>, HttpServerRequest.HttpServerRequest>>(), (context) => (effect) => sameOrigin.pipe(Effect.andThen(authorize), Effect.provideContext(context), Effect.andThen(effect), Effect.map(unshared))),
);
```

`authorize` fails with `Forbidden` unless the session belongs to an allowed owner, `sameOrigin` refuses a cross-site write, and `unshared` marks the response `Cache-Control: private, no-store`. `Effect.provideContext` merges the captured services over each request's context:
- Endpoint middleware added last runs first, so middleware that `requires` a service is added before the middleware that `provides` it
- Middleware providing a service wraps the handler effect in `Effect.provideService`
- Timeouts a middleware applies bound body receipt and the handler together
- `requiredForClient: true` makes derived clients require `HttpApiMiddleware.layerClient(M, handler)`, taking a handler or an effect of one

Middleware declaring `security: { <name>: scheme }` holds one handler `(effect, { credential, endpoint, group })` per scheme, and schemes run in declaration order:
- Missing credentials arrive as an empty `Redacted`, a scheme's own failure passes to the next scheme, and a handler failure ends the chain
- `HttpApiSecurity.bearer`, `basic`, `http({ scheme })`, and `apiKey({ key, in })` declare a credential's place, `in` defaulting to `'header'`
- `basic` decodes to `{ username, password }` with a `Redacted` password split at the first colon
- `apiKey({ in: 'cookie' })` reads an unsigned cookie that `HttpApiBuilder.securitySetCookie(scheme, value)` sets `secure` and `httpOnly`

## [04]-[HANDLERS]

`HttpApiBuilder.group` builds one layer per group, and `HttpApiBuilder.layer(Api)` registers every group on the `HttpRouter`:

```ts
const documentGroup = HttpApiBuilder.group(Api, 'documents', (handlers) =>
    handlers.handleAll({
        draft: () => Effect.map(readDocument('draft'), ({ data, revision }) => HttpApiSchema.withHeaders({ body: data, headers: { 'draft-etag': revision } })),
        save: ({ payload, headers }) => writeDocument(payload, headers['if-match']),
    }),
);

const app = <A, E, R>(services: Layer.Layer<A, E, R>) => HttpApiBuilder.layer(Api).pipe(Layer.provide([documentGroup, assetGroup]), Layer.provide(OwnerLive), Layer.merge(page), HttpRouter.provideRequest(services), Layer.provide(services));
```

`readDocument` and `writeDocument` run the draft's statements, `writeDocument` failing with `PreconditionFailed` or `PlacementConflict`, `assetGroup` builds the other group, `page` is a raw route rendering the published site, and `services` holds one runtime's database and object storage layers:
- `HttpApiBuilder.group` fails to compile (`Endpoint not handled: <id>`) for an uncovered endpoint, `handleAll` for an unknown key
- Build functions return the handlers or an effect of them, reading services once at layer build
- `handlers.handle(id, fn, { uninterruptible: true })` runs a write to completion, and `handleRaw` skips payload decoding
- `HttpApiBuilder.handler(Api, group, endpoint, fn)` types one handler apart, and `HttpApiBuilder.endpoint` builds one route effect
- Handlers return a raw `HttpServerResponse` (a redirect, a stored file, a ranged body) in place of the declared success
- `HttpApiBuilder.layer` dies at build for a group with no layer and for endpoints sharing a method and path, one endpoint covering each variant
- `HttpApiBuilder.layer` reads `Etag.Generator`, `FileSystem`, `HttpPlatform`, and `Path`, which a host server layer supplies
- `HttpApiBuilder.layer(Api, { openapiPath })` serves the document, `HttpApiScalar.layer(Api)` or `HttpApiSwagger.layer(Api)` documentation at `/docs`

## [05]-[STREAMS]

Stream schemas (`HttpApiSchema.StreamUint8Array({ maxBytes })`, `StreamNdjson({ schema, maxBytes, maxElementBytes })`) fill payload or success slots, the handler receiving or returning a lazy `Stream` and the client sending or yielding one:
- `StreamSse({ data | events, error, keepAlive })` fills success slots alone, typing a mid-stream `error` and commenting each idle interval
- NDJSON and byte streams hold no error schema, and an element failing to encode aborts the response
- `maxBytes` bounds a payload with no default (413 to a larger `content-length`, a failure mid-read), `maxElementBytes` an NDJSON line at 1 MiB
- Success sets mix buffered and stream schemas on one endpoint, `HEAD` takes no stream, and an endpoint takes one streaming payload
- Fetch runtimes with no request streams fail a stream payload unless `FetchHttpClient.StreamBodyFallback` is `'buffer'`
- `HttpApiSchema.asMultipart(limits)` decodes `Multipart.FilesSchema` fields to persisted files, `asMultipartStream` streams the parts
- `HttpApiSchema.Upgrade({ protocol })` hands the handler an `upgrade()` returning a `Socket`, WebSocket excluded, a plain request answering 426

## [06]-[CLIENTS]

`HttpApiClient.make(Api, { baseUrl, transformClient })` reads `HttpClient` and derives one method per endpoint:

```ts
const signed = Effect.fn('signed')(function* (request: unknown) {
    const client = yield* HttpApiClient.make(Api);
    return yield* client.assets.sign({ payload: yield* Schema.decodeUnknownEffect(Presignable)(request) });
}, Effect.provide(FetchHttpClient.layer));
```

`signed` answers the uploader's signing callback. Declared errors decode into their classes and undeclared statuses into a `DecodeError` reason, so `client.documents.save({ payload, headers: { 'if-match': revision } })` fails with `PreconditionFailed` holding the stored revision:
- `HttpApiClient.group`, `endpoint`, and `makeWith({ httpClient })` derive one group, one endpoint, or a client over a given `HttpClient`
- Calls take `responseMode: 'decoded-and-response'` for `[value, response]` or `'response-only'` for the undecoded response
- `HttpApiClient.decodeResponse(Api, { group, endpoint })` decodes a response read later or elsewhere, built once per endpoint
- `HttpApiClient.urlBuilder(Api, { baseUrl })` encodes typed links, relative without `baseUrl`, `Option.none()` dropping a `:name?` segment
- URL builder links reach an endpoint over a transport outside Effect, its method read from `Api.groups.<group>.endpoints.<endpoint>.method`
- `openapigen --format httpapi` of `@effect/openapi-generator` prints an `HttpApi` module from an OpenAPI document, `--patch` fixing it first

Request policies sit in `transformClient` or on a plain `HttpClient`, and their order decides what each bounds:
- `HttpClient.timeout` before `retryTransient` bounds each attempt, after it the whole loop, and a timeout fails as a transient `TransportError`
- Body reads outside the timed client (`response.json`, `response.stream`) take their own timeout
- `HttpClient.retryTransient({ schedule, times })` retries transport failures and 408, 429, 500, 502, 503, and 504 forever unless `times` bounds it
- `HttpClient.retryTransient` waits at least `Retry-After`
- `HttpClient.withRateLimiter({ limiter, key, limit, window })` adapts to rate-limit headers and retries 429 forever unless `times` bounds it
- `HttpClient.withCircuitBreaker({ breaker })` follows the rate limiter and the retry, so an open circuit skips the retry loop
- `HttpClient.filterStatusOk` fails a non-2xx response with a `StatusCodeError` reason a retry policy reads
- `HttpClient.mapRequest(flow(HttpClientRequest.prependUrl(base), HttpClientRequest.acceptJson))` sets a base once on a plain client
- `HttpClientResponse.schemaBodyJson(S)` decodes a body, and `HttpClientResponse.matchStatus({ 200: ..., '4xx': ..., orElse })` folds by status
- `HttpClient.TracerPropagationEnabled` set `false` keeps `b3` and `traceparent` off hosts refusing them in CORS (presigned uploads)
- `FetchHttpClient.RequestInit` set `{ redirect: 'manual' }` lets `HttpClient.followRedirects` act, as fetch follows redirects itself

## [07]-[ROUTER]

Raw routes are layers registered on `HttpRouter`, for answers a contract cannot type (pages, static files, sign-in, webhooks):
- `HttpRouter.add(method, path, handler)` registers an effect, request function, or constant `HttpServerResponse`, `addAll(routes, { prefix })` a set
- `HttpRouter.middleware(f).layer` provided to routes wraps those routes, `{ global: true }` every route, and an effect in place of `f` builds once
- `HttpRouter.middleware<{ provides: A }>()(build)` types a service it provides, `.combine` joins middleware, and `HttpRouter.cors(options)` is global
- `serve` and `toWebHandler` take a `middleware` option around the whole server chain, and response changes it makes never reach the client
- `HttpRouter.provideRequest(layer)` supplies built `layer` services to wrapped routes, discharging their `Request.From<'Requires', A>` markers
- `Layer.provide(layer)` outside `provideRequest(layer)` builds `layer` once for both, the parent memo map holding it
- `serve`, `toHttpEffect`, and `toWebHandler` build in a forked memo map per call, so services sibling layers share are provided outside them
- Workers build `toWebHandler` at module scope under `Scheduler.MixedScheduler('sync')`, as fibers started inside `fetch` capture that request
- `serve` and `toWebHandler` log each request unless `disableLogger`, and `HttpMiddleware.withLoggerDisabled` exempts a probe route
- `HttpMiddleware.xForwardedHeaders({ trustedProxies })` sets protocol, host, and remote address from trusted proxy hops
- `HttpMiddleware.compression({ algorithms })` negotiates encoding, and `Cache-Control: no-transform` or `Content-Encoding: identity` exempts a route
- `HttpStaticServer.layer({ root, prefix, spa, cacheControl })` serves files with ranges and 304 validators, `HttpStaticServer.make` one route handler
- `toWeb` keeps a web `Response` in `HttpServerResponse.raw(response)` unchanged unless it drops body or `content-length` or merges outer headers
- `HttpStatus.fromLiteral('PreconditionFailed')` derives a status number, `Mime.getType(path)` an `Option` of a media type, `Etag.toString` a header
- `HttpRouter.schemaPathParams(S)`, `HttpServerRequest.schemaSearchParams(S)`, and `schemaBodyUrlParams(S)` decode string records

## [08]-[GRAPHQL]

`GraphQLClient.make(group)` over a `GraphQLGroup` of `GraphQL.query`, `mutation`, and `subscription` operations gives a method per operation, each subscription returning a `Stream`:
- `graphqlgen` of `@effect/graphql-generator` writes each document's operations and `GraphQLGroup` beside it, `--check` failing when stale
- `GraphQLClient.pages` and `items` page through a cursor connection
- `GraphQLProtocol.layerHttp({ url })` runs over `HttpClient`, subscriptions over graphql-sse or graphql-ws when `subscriptions.webSocket` is set
- `GraphQLClient.make(group, { subscriptionRetry })` restarts a subscription after a retryable failure, and the server can resend events
- `GraphQLResolver.findById` batches lookups into one request, `ordered` and `aliased` keeping each result with its key
- `GraphQLClientError.reason` separates `ResponseError`, `TransportError`, `EncodeError`, `DecodeError`, and `PaginationError`
