# [HTTP]

## [01]-[CONTRACT]

One `HttpApi` declaration of groups and endpoints derives server routes, OpenAPI document, typed client, URL builder, and in-memory test client:

```ts
class Hold extends Schema.Class<Hold, { readonly brand: unique symbol }>('Hold')({ id: Schema.String.check(Schema.isUUID()).pipe(Schema.brand('HoldId')), account: Schema.String, amount: Schema.BigDecimal, placedAt: Schema.DateTimeUtc }) {}

class Api extends HttpApi.make('ledger').add(
    HttpApiGroup.make('holds').add(
        HttpApiEndpoint.get('list', '/holds', { query: { after: Schema.optionalKey(Schema.DateTimeUtc) }, success: HttpApiSchema.WithHeaders(Schema.Array(Hold), { 'x-total-count': Schema.Int }) }),
        HttpApiEndpoint.post('place', '/holds', { payload: Hold.mapFields(Struct.pick(['account', 'amount'])), success: Hold.pipe(HttpApiSchema.status('Created')), error: Overdrawn }),
        HttpApiEndpoint.delete('release', '/holds/:id', { params: { id: Hold.fields.id }, error: Missing }),
    ),
) {}
```

`Overdrawn` and `Missing` are reason classes of the package's `LedgerError`, each declared with `{ httpApiStatus }` as its third argument. Endpoint fields take domain schemas, and the endpoint applies `Schema.toCodecStringTree` to params, query, and headers and `Schema.toCodecJson` to bodies:
- Payload fields on `GET`, `HEAD`, and `OPTIONS` decode from the query string, and header keys are lowercase
- `payload`, `success`, and `error` take one schema or an array, the server choosing a payload by content type and the client a response by status
- `HttpApiSchema.status('Created')` names a status by `HttpStatus` literal, and `HttpApiSchema.NoContent`, `Created`, and `Accepted` are bodiless
- Payloads derive from the model through `Model.jsonCreate`, `jsonUpdate`, or `Class.mapFields`
- `HttpApiSchema.WithHeaders(Success, headers)` types headers that handlers set through `withHeaders` and clients resolve as `{ body, headers }`
- `HttpApiSchema.asText`, `asUint8Array`, `asFormUrlEncoded`, `asMultipart(limits)`, and `asMultipartStream(limits)` select a body encoding
- `.middleware(M)`, `.prefix(p)`, and `annotateEndpoints` reach only endpoints added before the call
- `HttpApi.ParseOptions` annotates the API for server, client, and URL builder alike, with `ParamsParseOptions` through `ErrorParseOptions` per slot
- `HttpApiGroup.make('g', { topLevel: true })` places a group's methods at the client root, and `.addHttpApi(other)` merges another API's groups

## [02]-[ERRORS]

HttpApi sends each declared error with its status and decodes it back to the same class on the client:
- `HttpApiError.<Status>` classes cover each common status with `.make()`, and each `<Status>NoContent` schema encodes an empty body
- `HttpApiError.Problem<X>()(tag, { type, status, title }, fields)` declares an RFC 9457 error, `type` keeping classes that share a status apart
- `HttpApiSchema.asProblem({ title })` renders an existing error class as a problem body
- `HttpApiMiddleware.ProblemDetails` with `layerProblemDetails({ includeDetails: true })` answers decode failures as `BadRequestProblem` per path
- `HttpApiSchemaError.kind` names the failing slot, `Body` and `ResponseHeaders` failures answering 500 and request slots 400
- Errors below 500 skip the `ErrorReporter`, and 500 and above report
- Declare every error a client branches on at its endpoint, undeclared `HttpServerRespondable` errors rendering their own response untyped

## [03]-[MIDDLEWARE]

Middleware are services keyed per API, group, or endpoint, each providing a service to handlers and declaring its errors and security schemes:

```ts
const GuardLive = Layer.effect(Guard, Effect.map(Effect.context<Exclude<Effect.Services<typeof check>, HttpServerRequest.HttpServerRequest>>(), (context) => (effect) => check.pipe(Effect.provideContext(context), Effect.andThen(effect))));
```

`Guard` is an `HttpApiMiddleware.Service` and `check` its per-request effect. `GuardLive` captures at build every service `check` reads beside the request, and `Effect.provideContext` merges them over each request's context:
- `HttpApiMiddleware.Service<M, { provides, requires }>()(id, { error, security, requiredForClient })` layers hold a handler per security key
- Security handlers run in declaration order, a missing credential arriving as an empty `Redacted` the handler rejects
- `HttpApiSecurity.apiKey({ in: 'cookie', key })` reads a validated cookie, and `HttpApiSecurity.basic` decodes `{ username, password }`
- Middleware wraps decoding, and a timeout it applies bounds body receipt and the handler together
- Middleware that `requires` a service sits before middleware that `provides` it
- `requiredForClient: true` makes derived clients require `HttpApiMiddleware.layerClient(M, handler)`, taking a handler or an effect reading `Config`

## [04]-[HANDLERS]

`HttpApiBuilder.group` builds one layer per group from services read once at layer build, and `HttpApiBuilder.layer(Api)` registers every group on the `HttpRouter`:

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

const Server = HttpRouter.serve(HttpApiBuilder.layer(Api, { openapiPath: '/openapi.json' })).pipe(Layer.provide(HoldsLive), Layer.provide([Holds.layer, NodeHttpServer.layerConfig(createServer, { port: Config.Port('PORT') })]));
```

`Holds` is a service with `list`, and with `place` and `release` failing with `LedgerError`. Each handler moves the reason its endpoint declares into the error channel, and the rest stay defects:
- `handleAll` takes exactly one handler per endpoint, and a missing one fails to compile naming the endpoint
- `handlers.handle(id, fn, { uninterruptible: true })` runs a write to completion, and `handleRaw` reads `request.stream` with no payload decoding
- Handlers return a raw `HttpServerResponse` (a redirect, a file, a ranged body) in place of the declared success
- `HttpApiBuilder.layer` dies at build for a group with no layer and for endpoints sharing a method and path
- `HttpApiScalar.layer(Api)` or `HttpApiSwagger.layer(Api)` serves documentation, and `OpenApi.fromApi(Api)` returns the document

## [05]-[STREAMS]

Stream schemas (`HttpApiSchema.StreamUint8Array({ maxBytes })`, `StreamNdjson({ schema, maxBytes, maxElementBytes })`, `StreamSse({ data, error })`) fill payload or success slots, the handler receiving or returning a lazy `Stream` and the client sending or yielding one:
- `StreamSse` types a mid-stream `error`, and a failure in the other forms ends the connection
- `maxBytes` bounds a streamed payload with no default, and `Stream.timeoutOrElse` bounds a stalled upload with a failure
- Client retries and redirects replay a payload stream, and a one-shot source buffers before it sends
- Multipart payloads decode `Multipart.FilesSchema` to persisted files, and `Multipart.schemaJson(schema)` decodes a JSON field
- `HttpApiSchema.Upgrade({ protocol })` hands the handler a `Socket` on Node servers and clients

## [06]-[CLIENTS]

`HttpApiClient.make(Api, options)` derives the client, and a service with a `make` holding it derives its shape:

```ts
class Ledger extends Context.Service<Ledger>()('@app/ledger', {
    make: Effect.flatMap(
        Config.schema(Schema.Struct({ baseUrl: Schema.URL, timeout: Schema.DurationFromString, base: Schema.DurationFromString, retries: Schema.Int }), 'ledger'),
        ({ baseUrl, timeout, base, retries }) => HttpApiClient.make(Api, { baseUrl, transformClient: flow(HttpClient.timeout(timeout), HttpClient.retryTransient({ schedule: Schedule.exponential(base), times: retries })) }),
    ),
}) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(FetchHttpClient.layer));
}
```

`transformClient` holds request policies. Declared error statuses decode after `execute` into their classes, and `ledger.holds.release({ params: { id } })` fails with `Missing` for `Effect.catchTag`:
- `HttpApiClient.group` and `endpoint` derive one group's or endpoint's client, and `responseMode` returns the raw response beside the decoded value
- `HttpApiClient.urlBuilder(Api, { baseUrl })` encodes typed links through endpoint schemas, an omitted `baseUrl` returning a relative href
- Optional `:name?` segments typed `Schema.OptionFromOptionalKey(S)` drop out of a URL on `Option.none()`
- `HttpApiTest.groups(Api, ['g'])` runs a typed client in memory over real encoding, routing, and decoding
- `NodeHttpServer.layerTest` serves on an ephemeral port with a prefixed `HttpClient`
- `HttpClient.mapRequest(flow(HttpClientRequest.prependUrl(base), HttpClientRequest.acceptJson))` sets a base once on a plain client
- `Url.setUrlParams(url, UrlParams.fromInput(params))` sets an absolute URL's query, `` `${path}?${UrlParams.toString(record)}` `` a relative href's
- `HttpIncomingMessage.schemaBodyJson(S)` decodes bodies, and `HttpClientResponse.matchStatus({ 200: ..., '4xx': ..., orElse })` folds by status
- `HttpClient.retryTransient` reads `Retry-After` and retries `isTransientError` cases (transport failures, 408, 429, 500, 502, 503, 504)
- `HttpClient.timeout` before `retryTransient` bounds each attempt, after it the whole loop, and a timeout is a transient `TransportError`
- `HttpClient.withRateLimiter` adapts to rate-limit headers with `times` bounding 429 retries, and `withCircuitBreaker` follows it and the retry

## [07]-[ROUTER]

Raw routes are layers registered on `HttpRouter`, for answers a contract cannot type:
- `HttpRouter.add(method, path, handler)` registers a route with an effect or constant `HttpServerResponse` handler, `HttpRouter.use` a set
- `router.prefixed(p)` adds a prefix, and `HttpRouter.cors(options)` adds CORS
- `HttpRouter.middleware(build, { global: true })` builds in the layer scope, where `Layer.build(layer)` holds services that middleware alone reads
- Middleware rewriting a response is `Effect.map(rewrite)`, and one supplying built services runs `Effect.provideContext(app, services)`
- `HttpRouter.provideRequest(layer)` supplies built `layer` services to wrapped routes, discharging their `Request.From<'Requires', A>` markers
- `Layer.provide(layer)` beside `provideRequest(layer)` serves group and middleware builds, the memo map building `layer` once for both
- `HttpMiddleware.xForwardedHeaders({ trustedProxies })` sets protocol, host, and remote address from trusted proxy hops
- `HttpMiddleware.compression({ algorithms })` negotiates encodings, and `Cache-Control: no-transform` exempts a secret-bearing route
- `HttpStaticServer.layer({ root, prefix, spa, cacheControl })` serves files with ranges and validators
- `toWeb` keeps a web `Response` in `HttpServerResponse.raw(response)` unchanged unless it drops body or `content-length` or merges outer headers
- Node servers write a `Raw` body by piping or `end` alone, and global middleware re-streams a raw web `Response` through `fromWeb(toWeb(response))`
- `HttpStatus.fromLiteral('PreconditionFailed')` derives a status number, `Mime.getType(path)` a media type, and `Etag.toString` a header value
- `HttpRouter.schemaPathParams(S)` and `HttpServerRequest.schemaSearchParams(S)` decode raw string records through `Schema.toCodecStringTree`
