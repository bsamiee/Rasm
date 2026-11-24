# [HTTP]

## [01]-[CONTRACT]

One `HttpApi` declaration of groups and endpoints derives server routes, OpenAPI document, typed client, URL builder, and in-memory test client:

```ts
class Hold extends Schema.Class<Hold, { readonly brand: unique symbol }>('Hold')({ id: Schema.String.check(Schema.isUUID()).pipe(Schema.brand('HoldId')), account: Schema.String, amount: Schema.BigDecimal, placedAt: Schema.DateTimeUtc }) {}

class Api extends HttpApi.make('ledger').add(
    HttpApiGroup.make('holds')
        .add(
            HttpApiEndpoint.get('list', '/holds', { query: { after: Schema.optionalKey(Schema.DateTimeUtc) }, success: HttpApiSchema.WithHeaders(Schema.Array(Hold), { 'x-total-count': Schema.Int }) }),
            HttpApiEndpoint.post('place', '/holds', { payload: Hold.mapFields(Struct.pick(['account', 'amount'])), success: Hold.pipe(HttpApiSchema.status('Created')), error: Overdrawn }),
            HttpApiEndpoint.delete('release', '/holds/:id', { params: { id: Hold.fields.id }, error: Missing }),
        )
        .middleware(Authenticated),
) {}
```

`Overdrawn` and `Missing` are reason classes of the package's `LedgerError`, each annotated with `{ httpApiStatus }`, and `Authenticated` an `HttpApiMiddleware.Service`. Endpoint fields take domain schemas, and endpoints apply `Schema.toCodecStringTree` to params, query, headers, and form bodies and `Schema.toCodecJson` to JSON bodies:
- Payload fields on `GET`, `HEAD`, and `OPTIONS` decode from query string, and a fields object as a body payload decodes as a form, a schema as JSON
- `payload`, `success`, and `error` take one schema or an array, the server choosing a payload by content type and the client a response by status
- `HttpApiSchema.status('Created')` names a status by literal, and `HttpApiSchema.Empty(code)`, `NoContent`, `Created`, and `Accepted` are bodiless
- `HttpApiSchema.WithHeaders(Success, headers)` types headers that handlers set through `withHeaders` and clients resolve as `{ body, headers }`
- `.middleware(M)`, `.prefix(p)`, and `annotateEndpoints` reach only endpoints added before the call
- `HttpApiGroup.make('g', { topLevel: true })` places a group's methods at the client root, and `.addHttpApi(other)` merges another API's groups

## [02]-[ERRORS]

HttpApi sends each declared error with its status and decodes it back to the same class on the client, an undeclared `HttpServerRespondable` error rendering its own response untyped:
- `HttpApiError.<Status>` classes cover each common status with `.make()`, and each `<Status>NoContent` schema encodes an empty body
- `HttpApiError.Problem<X>()(tag, { type, status, title }, fields)` declares an RFC 9457 error, `type` keeping classes that share a status apart
- `HttpApiSchema.asProblem({ title })` renders an existing error class as a problem body
- `HttpApiMiddleware.ProblemDetails` on `.middleware` with `layerProblemDetails({ includeDetails: true })` answers decode failures per path
- `HttpApiMiddleware.layerSchemaErrorTransform(M, transform)` maps decode failures to a package error
- `HttpApiSchemaError.kind` names the failing slot, `Body` and `ResponseHeaders` failures answering 500 and request slots 400
- Built-in and `Problem` errors below 500 skip the `ErrorReporter`, and a package error class sets `[ErrorReporter.ignore] = true` to skip it

## [03]-[MIDDLEWARE]

Middleware is a service keyed per API, group, or endpoint. Middleware declaring `security` holds one handler per scheme, and one providing a service wraps the handler effect in `Effect.provideService`:

```ts
class Authenticated extends HttpApiMiddleware.Service<Authenticated, { provides: Account }>()('@app/Authenticated', { security: { bearer: HttpApiSecurity.bearer }, error: Unauthorized }) {}

const AuthenticatedLive = Layer.effect(
    Authenticated,
    Sessions.useSync((sessions) => Authenticated.of({ bearer: (effect, { credential }) => Effect.flatMap(sessions.resolve(Redacted.value(credential)), (account) => Effect.provideService(effect, Account, account)) })),
);
```

`Account` is the service handlers read through `yield* Account`, `Sessions.resolve` reads a token to an `Account` or fails with `Unauthorized`, and `AuthenticatedLive` reads `Sessions` once at build. Schemes run in declaration order, a missing credential arriving as an empty `Redacted` and a failed scheme passing to the next:
- `HttpApiSecurity.bearer`, `basic`, and `apiKey({ key, in })` declare a credential's place, `in` defaulting to `'header'`
- `basic` decodes to `{ username, password }` with a `Redacted` password
- `apiKey({ in: 'cookie' })` reads an unsigned cookie, and `HttpApiBuilder.securitySetCookie(scheme, value)` sets it from a sign-in handler

Middleware with no `security` is one function `(effect, { endpoint, group }) => Effect`, and a guard captures at build every service its per-request `check` reads beside the request:

```ts
const GuardLive = Layer.effect(
    Guard,
    Effect.map(Effect.context<Exclude<Effect.Services<typeof check>, HttpServerRequest.HttpServerRequest>>(), (context) => (effect) => check.pipe(Effect.provideContext(context), Effect.andThen(effect))),
);
```

`Guard` is an `HttpApiMiddleware.Service` declaring `error` alone, and `Effect.provideContext` merges the captured services over each request's context. Middleware wraps decoding and the handler:
- Timeouts a middleware applies bound body receipt and the handler together
- Endpoint middleware added last runs first, and middleware that `requires` a service is added before the middleware that `provides` it
- `requiredForClient: true` makes derived clients require `HttpApiMiddleware.layerClient(M, handler)`, taking a handler or an effect of one

## [04]-[HANDLERS]

`HttpApiBuilder.group` builds one layer per group, and `HttpApiBuilder.layer(Api)` registers every group on the `HttpRouter`:

```ts
const HoldsLive = HttpApiBuilder.group(
    Api,
    'holds',
    Effect.fn(function* (handlers) {
        const holds = yield* Holds;
        return handlers.handleAll({
            list: ({ query }) => Effect.map(holds.list(Option.fromUndefinedOr(query.after)), (body) => HttpApiSchema.withHeaders({ body, headers: { 'x-total-count': body.length } })),
            place: ({ payload }) => holds.place(payload).pipe(Effect.catchReason('LedgerError', 'Overdrawn', Effect.fail, Effect.die)),
            release: ({ params }) => holds.release(params.id).pipe(Effect.catchReason('LedgerError', 'Missing', Effect.fail, Effect.die)),
        });
    }),
);

const Server = HttpRouter.serve(HttpApiBuilder.layer(Api, { openapiPath: '/openapi.json' })).pipe(Layer.provide(HoldsLive), Layer.provide([Holds.layer, AuthenticatedLive, NodeHttpServer.layerConfig(createServer, { port: Config.Port('PORT') })]));
```

`Holds` is a service with `list`, `place`, and `release`, `place` and `release` failing with `LedgerError`. Each handler moves the reason its endpoint declares into the error channel, and the rest stay defects:
- `HttpApiBuilder.group` fails to compile naming an endpoint no handler covers, and `handleAll` refuses a key naming no endpoint
- `handlers.handle(id, fn, { uninterruptible: true })` runs a write to completion, and `handleRaw` reads `request.stream` with no payload decoding
- `HttpApiBuilder.handler(Api, group, endpoint, fn)` types one handler declared apart from its group for `handlers.handle`
- Handlers return a raw `HttpServerResponse` (a redirect, a file, a ranged body) in place of the declared success
- `HttpApiBuilder.layer` dies at build for a group with no layer and for endpoints sharing a method and path
- `HttpApiScalar.layer(Api)` or `HttpApiSwagger.layer(Api)` serves documentation at `/docs`, and `OpenApi.fromApi(Api)` returns the document

## [05]-[STREAMS]

Stream schemas (`HttpApiSchema.StreamUint8Array({ maxBytes })`, `StreamNdjson({ schema, maxBytes, maxElementBytes })`) fill payload or success slots, the handler receiving or returning a lazy `Stream` and the client sending or yielding one:
- `StreamSse({ data | events, error, keepAlive })` fills success slots alone and types a mid-stream `error`
- Failures in other stream forms end the connection
- `maxBytes` bounds a streamed payload with no default, a breach answering 413, and `Stream.timeoutOrElse` bounds a stalled upload
- Client retries rerun a request and its lazy payload `Stream`
- Runtimes with no request streams fail unless `FetchHttpClient.StreamBodyFallback` is `'buffer'`
- Multipart payloads decode `Multipart.FilesSchema` or `SingleFileSchema` to persisted files, JSON fields through `Schema.fromJsonString(S)`
- `HttpApiSchema.Upgrade({ protocol })` hands the handler an `upgrade()` effect returning a `Socket`, WebSocket excluded

## [06]-[CLIENTS]

`HttpApiClient.make(Api, options)` derives the client, and a service with a `make` holding it derives its shape:

```ts
class Ledger extends Context.Service<Ledger>()('@app/ledger', {
    make: Effect.flatMap(Config.schema(Schema.Struct({ baseUrl: Schema.URL, timeout: Schema.DurationFromString, base: Schema.DurationFromString, retries: Schema.Int }), 'ledger'), ({ baseUrl, timeout, base, retries }) =>
        HttpApiClient.make(Api, { baseUrl, transformClient: flow(HttpClient.timeout(timeout), HttpClient.retryTransient({ schedule: Schedule.exponential(base), times: retries })) }),
    ),
}) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(FetchHttpClient.layer));
}
```

Declared error statuses decode after `execute` into their classes, and `ledger.holds.release({ params: { id } })` fails with `Missing` for `Effect.catchTag`. `transformClient` holds request policies, and their order on an `HttpClient` decides what each bounds:
- `HttpClient.retryTransient` retries transport failures and 408, 429, 500, 502, 503, and 504 forever unless `times` bounds it
- `HttpClient.retryTransient` waits at least `Retry-After`
- `HttpClient.timeout` before `retryTransient` bounds each attempt, after it the whole loop, and a timeout is a transient `TransportError`
- `HttpClient.withRateLimiter({ limiter, window, limit, key })` adapts to rate-limit headers and retries 429 forever unless `times` bounds it
- `HttpClient.withCircuitBreaker` follows the rate limiter and the retry
- `HttpClient.filterStatusOk` fails a non-2xx response with a `StatusCodeError` reason a retry policy reads
- `HttpClient.mapRequest(flow(HttpClientRequest.prependUrl(base), HttpClientRequest.acceptJson))` sets a base once on a plain client
- `HttpClientResponse.schemaBodyJson(S)` decodes a body, and `HttpClientResponse.matchStatus({ 200: ..., '4xx': ..., orElse })` folds by status

Derived clients choose their scope, response mode, and links, and tests run them in memory or on an ephemeral port:
- `HttpApiClient.group({ group, httpClient })` and `endpoint({ group, endpoint, httpClient })` derive one group or one endpoint
- `HttpApiClient.makeWith({ httpClient })` derives a client over a given `HttpClient`
- `responseMode: 'decoded-and-response'` on a call returns `[value, response]`, and `'response-only'` skips decoding
- `HttpApiClient.urlBuilder(Api, { baseUrl })` encodes typed links through endpoint schemas, an omitted `baseUrl` returning a relative href
- Optional `:name?` segments typed `Schema.OptionFromOptionalKey(S)` drop out of a URL on `Option.none()`
- `Url.setUrlParams(url, params)` sets an absolute URL's query, `` `${path}?${UrlParams.toString(record)}` `` a relative href's
- `HttpServer.layerServices` supplies the platform `HttpApiTest.groups(Api, ['g'])` runs over
- `NodeHttpServer.layerTest` serves on an ephemeral port with a prefixed `HttpClient`
- `Layer.succeed(FetchHttpClient.Fetch, replay)` under `FetchHttpClient.layer` answers requests from a recorded `fetch`, with no server

## [07]-[ROUTER]

Raw routes are layers registered on `HttpRouter`, for answers a contract cannot type:
- `HttpRouter.add(method, path, handler)` registers an effect, request function, or constant `HttpServerResponse`, `addAll(routes, { prefix })` a set
- `HttpRouter.cors(options)` adds CORS to every route, and `HttpRouter.middleware(build, { global: true })` builds a middleware in the layer scope
- Middleware rewriting a response is `Effect.map(rewrite)`, and one supplying built services runs `Effect.provideContext(app, services)`
- `HttpRouter.provideRequest(layer)` supplies built `layer` services to wrapped routes, discharging their `Request.From<'Requires', A>` markers
- `Layer.provide(layer)` outside `provideRequest(layer)` builds `layer` once for both, the parent memo map holding it
- `HttpRouter.serve` builds its routes in a forked memo map, and services sibling layers share are provided outside it
- `HttpRouter.toHttpEffect(layer)` and `toWebHandler(layer)` build a router in a forked memo map, a second call building it again
- `HttpRouter.serve` and `toWebHandler` log each request unless `disableLogger`, and `serve` logs its listen address unless `disableListenLog`
- `HttpMiddleware.xForwardedHeaders({ trustedProxies })` sets protocol, host, and remote address from trusted proxy hops
- `HttpMiddleware.compression({ algorithms })` negotiates encoding, and `Cache-Control: no-transform` or `Content-Encoding: identity` exempts a route
- `HttpStaticServer.layer({ root, prefix, spa, cacheControl })` serves files with ranges and validators
- `toWeb` keeps a web `Response` in `HttpServerResponse.raw(response)` unchanged unless it drops body or `content-length` or merges outer headers
- `HttpStatus.fromLiteral('PreconditionFailed')` derives a status number, `Mime.getType(path)` an `Option` of a media type, `Etag.toString` a header
- `HttpRouter.schemaPathParams(S)` and `HttpServerRequest.schemaSearchParams(S)` decode string records, `S` declaring string-encoded fields

## [08]-[GRAPHQL]

`GraphQLClient.make(group)` over a `GraphQLGroup` gives a method per operation, each subscription returning a `Stream`:
- `GraphQLClient.pages` and `items` page through a cursor connection
- `GraphQLProtocol.layerHttp` runs over `HttpClient`, subscriptions over SSE or `subscriptions.webSocket`
- `GraphQLResolver.findById` batches lookups into one request, `ordered` and `aliased` keeping each result with its key
- `GraphQLClientError.reason` separates `ResponseError`, `TransportError`, `EncodeError`, `DecodeError`, and `PaginationError`
- `GraphQLClient.layerTest(Client, group)(handlers)` answers operations through `handlers`
