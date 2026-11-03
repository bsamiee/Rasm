# [HTTP]

## [01]-[CONTRACT]

One `HttpApi` declaration of groups and endpoints derives server routes, OpenAPI document, typed client, URL builder, and in-memory test client. Contract modules hold the classes client and server both import:

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
- `HttpApiSchema.status('Created')` names a status by its `HttpStatus` literal, and `HttpApiSchema.NoContent`, `Created`, and `Accepted` are bodiless responses
- Payloads derive from the model through `Model.jsonCreate`, `jsonUpdate`, or `Class.mapFields`
- `HttpApiSchema.WithHeaders(Success, headers)` types response headers, a handler returns `HttpApiSchema.withHeaders({ body, headers })`, and the client resolves `{ body, headers }`
- `HttpApiSchema.asText`, `asUint8Array`, `asFormUrlEncoded`, `asMultipart(limits)`, and `asMultipartStream(limits)` select a body encoding
- `.middleware(M)`, `.prefix(p)`, and `annotateEndpoints` reach the endpoints added before the call, and close a group or API
- `HttpApi.ParseOptions` annotates the API for server, client, and URL builder alike, with `ParamsParseOptions` through `ErrorParseOptions` per slot
- `HttpApiGroup.make('g', { topLevel: true })` places a group's methods at the client root, and `.addHttpApi(other)` merges another API's groups

## [02]-[ERRORS]

HttpApi sends each declared error with its status and decodes it back to the same class on the client:
- `HttpApiError.<Status>` classes cover each common status with `.make()`, and each `<Status>NoContent` schema encodes an empty body
- `HttpApiError.Problem<X>()(tag, { type, status, title }, fields)` declares an RFC 9457 error, `type` keeping classes that share a status apart
- `HttpApiSchema.asProblem({ title })` renders an existing error class as a problem body
- `HttpApiMiddleware.ProblemDetails` with `HttpApiMiddleware.layerProblemDetails({ includeDetails: true })` answers request decode failures as `BadRequestProblem` with each failing path
- `HttpApiSchemaError.kind` names the failing slot, `Body` and `ResponseHeaders` failures answering 500 and request slots 400
- Errors below 500 skip the `ErrorReporter`, and 500 and above report
- Every error a client branches on is declared on its endpoint, and an undeclared `HttpServerRespondable` error renders its own response with no client type

## [03]-[MIDDLEWARE]

Middleware are services keyed per API, group, or endpoint, each providing a service to handlers and declaring its errors and security schemes:
- `HttpApiMiddleware.Service<M, { provides, requires }>()(id, { error, security, requiredForClient })` declares one, and its layer holds a handler per security key
- Each security scheme's handler runs in declaration order, a missing credential arriving as an empty `Redacted`, and the handler rejects an empty value
- `HttpApiSecurity.apiKey({ in: 'cookie', key })` reads a validated cookie, and `HttpApiSecurity.basic` decodes `{ username, password }`
- Middleware wraps decoding, and a timeout it applies bounds body receipt and the handler together
- Middleware that `requires` a service sits before middleware that `provides` it
- Services beside `requires` and provided ones are read at layer build and closed over
- `requiredForClient: true` makes the derived client require `HttpApiMiddleware.layerClient(M, ({ request, next }) => next(...))`, and `layerClient` takes an effect reading `Config`

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

Stream schemas fill payload or success slots, the handler receiving or returning a lazy `Stream` and the client sending or yielding one:
- `HttpApiSchema.StreamUint8Array({ maxBytes })`, `StreamNdjson({ schema, maxBytes, maxElementBytes })`, and `StreamSse({ data, error })` are the stream forms
- `StreamSse` types a mid-stream `error`, and a failure in the other forms ends the connection
- `maxBytes` bounds a streamed payload with no default, and `Stream.timeoutOrElse` bounds a stalled upload with a failure
- Client retries and redirects replay a payload stream, and a one-shot source buffers before it sends
- Multipart payloads decode `Multipart.FilesSchema` to persisted files, and `Multipart.schemaJson(schema)` decodes a JSON field
- `HttpApiSchema.Upgrade({ protocol })` hands the handler a `Socket` on Node servers and clients

## [06]-[CLIENTS]

`HttpApiClient.make(Api, options)` derives the client, and a service with a `make` holding it derives its shape:

```ts
class Ledger extends Context.Service<Ledger>()('@app/ledger/Ledger', {
    make: Effect.flatMap(
        Config.schema(Schema.Struct({ baseUrl: Schema.URL, timeout: Schema.DurationFromString, base: Schema.DurationFromString, retries: Schema.Int }), 'ledger'),
        ({ baseUrl, timeout, base, retries }) => HttpApiClient.make(Api, { baseUrl, transformClient: flow(HttpClient.timeout(timeout), HttpClient.retryTransient({ schedule: Schedule.exponential(base), times: retries })) }),
    ),
}) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(FetchHttpClient.layer));
}
```

`transformClient` holds request policies. Declared error statuses decode after `execute` into their classes, and `ledger.holds.release({ params: { id } })` fails with `Missing` for `Effect.catchTag`:
- `HttpApiClient.group` and `HttpApiClient.endpoint` derive one group's or endpoint's client, and `responseMode` returns the raw response beside the decoded value
- `HttpApiClient.urlBuilder(Api, { baseUrl })` derives typed URLs for a consumer outside Effect (an upload widget, a link)
- `HttpApiTest.groups(Api, ['g'])` runs a typed client over the real encode, route, and decode path in memory, and `NodeHttpServer.layerTest` serves on an ephemeral port with a prefixed `HttpClient`
- `HttpClient.mapRequest(flow(HttpClientRequest.prependUrl(base), HttpClientRequest.acceptJson))` sets a base once on a plain client
- `HttpIncomingMessage.schemaBodyJson(S)` decodes a response body, and `HttpClientResponse.matchStatus({ 200: ..., '4xx': ..., orElse })` folds by status
- `HttpClient.retryTransient` retries transport failures and 408, 429, and 5xx gateway statuses, reading `Retry-After`
- `HttpClient.timeout` before `retryTransient` bounds each attempt, after it the whole loop, and a timeout is a transient `TransportError`
- `HttpClient.withRateLimiter` adapts to rate-limit headers with `times` bounding 429 retries, and `withCircuitBreaker` follows it and the retry

## [07]-[ROUTER]

Raw routes are layers registered on `HttpRouter`, for answers a contract cannot type:
- `HttpRouter.add(method, path, handler)` registers one route, `HttpRouter.use((router) => ...)` a set, and `router.prefixed(p)` a prefix
- `HttpRouter.middleware(fn, { global: true })` wraps every response, `HttpRouter.cors(options)` adds CORS, and `HttpRouter.provideRequest(layer)` supplies per-request services
- `HttpMiddleware.xForwardedHeaders({ trustedProxies })` sets protocol, host, and remote address from trusted proxy hops
- `HttpMiddleware.compression({ algorithms })` negotiates encodings, and `Cache-Control: no-transform` exempts a secret-bearing route
- `HttpStaticServer.layer({ root, prefix, spa, cacheControl })` serves files with ranges and validators
- `HttpServerResponse.raw(response)` passes a host-built web `Response` through `toWeb` unchanged unless `toWeb` drops its body or `content-length` or merges outer headers, and `fromWeb` re-streams its body
- `HttpStatus.fromLiteral('PreconditionFailed')` derives a status number, `Mime.getType(path)` a media type, and `Etag.toString` a header value
- `HttpRouter.schemaPathParams(S)` and `HttpServerRequest.schemaSearchParams(S)` decode raw string records through `Schema.toCodecStringTree`
