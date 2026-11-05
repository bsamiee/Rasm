# [AI]

## [01]-[TOOLS]

Tools are schema contracts a `Toolkit` binds to handlers, and one toolkit serves `LanguageModel` and `McpServer` alike:
- `Tool.make(name, options)` defaults `parameters` to `Tool.EmptyParams` and `success` to `Schema.Void`
- `Toolkit` decodes arguments, and parameters with a non-JSON encoding (`DateTimeUtc`, `BigDecimal`, `Option`) declare `Schema.toCodecJson(X)`
- `Tool.Readonly`, `Destructive`, `Idempotent`, and `OpenWorld` annotate behavior, unannotated tools advertising themselves destructive and open-world
- `Tool.getDescription` returns the text clients list, `description` or else a description annotation on `parameters`
- `Tool.Strict` rejects excess parameter keys and advertises `additionalProperties: false`
- `failureMode: 'return'` returns a typed handler failure as the result, and a defect dies
- `dependencies: [S]` makes `S` a per-request service the handler reads
- `Toolkit.make(...tools)` builds a toolkit, `Toolkit.merge` joins toolkits, and `kit.toLayer(kit.of({...}))` binds handlers typed by their tools
- Handler layers capture the services read while they build, and every handler reads them with no `Effect.provide`
- `Tool.Parameters<T>`, `Tool.Success<T>`, `Tool.Failure<T>`, and `Toolkit.Tools<K>` read a tool's types

## [02]-[MCP_SERVER]

`McpServer` holds tools, resources, templates, and prompts a transport serves, a request-level service reaching handlers through middleware:

```ts
class Caller extends Context.Service<Caller, { readonly account: string; readonly admin: boolean }>()('@app/mcp/Caller') {}

const Reads = Toolkit.make(Tool.make('balance', { description: 'Balance of calling account', success: Schema.Finite, failure: LedgerError, dependencies: [Caller] }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Destructive, false), Context.add(Tool.OpenWorld, false))));
const Writes = Toolkit.make(Tool.make('freeze', { description: 'Freezes an account', parameters: Schema.Struct({ account: Schema.NonEmptyString }), failure: LedgerError, dependencies: [Caller] }).annotate(Tool.OpenWorld, false));

const Authenticated = McpServer.middleware<{ provides: Caller }>()(Tokens.useSync((tokens) => (effect, { headers }) => Effect.flatMap(tokens.resolve(Headers.get(headers, 'authorization')), (caller) => Effect.provideService(effect, Caller, caller))));

const Server = McpServer.layerHttp({
    name: 'ledger',
    version,
    path: '/mcp',
    protocols: [McpProtocol.v2025_11_25],
    registrations: Layer.mergeAll(
        McpServer.toolkit(Reads).pipe(Layer.provide([Authenticated.layer, Reads.toLayer(Accounts.useSync((accounts) => Reads.of({ balance: () => Caller.use(({ account }) => accounts.balance(account)) })))])),
        McpServer.toolkit(Writes).pipe(Layer.provide([McpServer.visibility(() => Caller.useSync(Struct.get('admin'))).combine(Authenticated).layer, Writes.toLayer(Accounts.useSync((accounts) => Writes.of({ freeze: ({ account }) => accounts.freeze(account) })))])),
    ),
});
```

`LedgerError` is the package error, `version` the package version, `Accounts` holds `balance` and `freeze`, and `Tokens.resolve` reads a token to a `Caller` or fails with an `McpSchema.McpErrorBase` error. `Server` requires `HttpRouter`, `Accounts`, and `Tokens` alone, the middleware discharging every `Caller` marker:
- Handler services become `McpServer.McpRequest<'Requires', S>` markers that middleware or the transport discharges, `Layer.provide` keeping them
- `registrations` build before the transport copies its options, and server `instructions` read every registration made while the layer builds
- Middleware wraps `tools/call`, `resources/read`, `prompts/get`, `completion/complete`, and `skills/get`, and `MiddlewareMethod` matches keep a default arm
- Middleware reads `{ method, target, payload, requestContext, headers, sessionId }`, `requestContext.requestMetadata` holding protocol `_meta`
- String entries of `requestMetadata` through `Headers.fromInput` give `HttpTraceContext.fromHeaders` a remote span parent
- `target` holds tool name, read URI, prompt name, or `completion/complete` prompt or URI template, a `FindMyWay` router mapping read URIs to templates
- Handler results reach middleware as opaque `MiddlewareSuccess` values `Schema.is` narrows, and one global middleware spans and meters every method
- `m.layer` scopes middleware to registrations it reaches, `m.layerGlobal` to every one through `McpServer`, and `a.combine(b)` runs `b` outside `a`
- `McpServer.middleware(fn, { global: true })` runs outside local middleware, middleware provided both ways runs once, and none runs for `initialize`
- `McpServer.visibility` hides registrations per caller and `McpSchema.EnabledWhen` per client profile
- List requests run middleware around `visibility` alone
- Middleware errors extend `McpSchema.McpErrorBase` with a code in `-32000` to `-32019` and reach the client as JSON-RPC errors
- Declared `failure` errors reach the client as `isError` text and other failures as one generic message, defects going to the log and `ErrorReporter`
- Tool results send `structuredContent` with one JSON text block, and a tool returns a struct
- `McpServer.resource` templates take typed `McpSchema.param` path segments alone, with no `{?query}` expansion
- `McpServer.resource` and `prompt` send `content` failures as `InternalError`, `server.addResource` and `addResourceTemplate` keep `InvalidParams`
- Completion handlers return typed values the server encodes and counts
- `McpSchema.optional(S)` declares a field a client omits or sends as `undefined`, encoding `undefined` as no key
- `McpServer.prompt({ name, parameters, content })` derives argument requirement from field optionality and description from annotations
- `McpSchema.ProgressToken` decodes `requestMetadata.progressToken` of `Effect.serviceOption(McpSchema.McpRequestContext)`
- Progress steps go through a data-last `Stream` operator sending `server.notifications['notifications/progress']` per element, returning `self` when no `ProgressToken` decodes
- `Layer.effect(McpServer.McpServer)` returning `McpServer.McpServer.of({ ...server, addTool })` decorates every registration below it
- Decorating layers install server-wide middleware through `yield* Layer.build(a.combine(b).layerGlobal)`
- Effects that decorated tools share run once under `Effect.cached`
- `McpServer.elicit({ message, schema })` decodes an answer and requires the `McpServerClient` a tuple of sessionful protocols supplies
- `McpServer.layerStdio` holds stdout for the protocol and needs `Logger.LogToStderr` or `Logger.withConsoleError(Logger.formatJson)`, `layerHttp` checks `Origin` alone, and `layerHttpRequestScoped` serves sessionless hosts
- `McpProtocol` exports one adapter per version and no list of them, and `protocols` names each version a server serves
- `McpTest.makeClient(registrations, { protocols })` runs a client against the server in memory over real encoding

## [03]-[MCP_CLIENT]

`McpClient` reads a remote server and adapts its tools into a `Toolkit`:
- `McpClient.layerHttp({ name, version, protocol, url })` reads `HttpClient`, and `layerStdio` spawns a `ChildProcess` through `ChildProcessSpawner`
- `client.callTool({ tool, arguments, schema })` decodes `structuredContent` through `schema` and fails with reason `ToolError` on `isError`
- `McpClient.toolkit(client, prefix)` returns a toolkit a language model calls

## [04]-[LANGUAGE_MODELS]

`LanguageModel` generates text, objects, and streams through the provider a layer supplies:
- `LanguageModel.generateText({ prompt, toolkit, toolChoice })` resolves tool calls through the toolkit, `toolChoice` typed by its tool names
- `LanguageModel.generateObject({ prompt, schema })` decodes `response.value` through `schema`, failing `AiError` with reason `StructuredOutputError`
- `LanguageModel.streamText` streams parts narrowed on `part.type`, and responses expose `text`, `toolCalls`, `finishReason`, and `usage` getters
- `needsApproval` on a tool yields an approval request, answered next turn with `Prompt.toolApprovalResponsePart`
- `Chat.empty`, `Chat.fromPrompt`, and `Chat.fromJson` start sessions keeping history, and `Chat.layerPersisted({ storeId })` saves each generation
- `<Provider>Client` layers read `HttpClient` (`AnthropicClient.layerConfig({ apiKey: Config.Redacted('ANTHROPIC_API_KEY') })`)
- `CloudflareClient.layerConfig()` and `TypeSafeClient.layerConfig()` read their environment variables by default
- `<Provider>LanguageModel.model(name, config)` layers hold provider and model names, and `ExecutionPlan.make` over them falls back across providers
- `AiError.reason` is a tagged union, `isRetryable` holding for a `RateLimitError` with its `retryAfter`
- `EmbeddingModel` batches concurrent `embed` calls into one `embedMany`

## [05]-[DECISIONS]

`Decision` declares classification, rating, probability, and location questions over one schema-typed input, and a `DecisionModel` answers each with probabilities:

```ts
const Triage = Decision.make({
    input: Schema.Struct({ subject: Schema.NonEmptyString, body: Schema.String }),
    decisions: {
        team: Decision.classify({ instructions: 'Team owning ticket', criteria: { billing: 'Charges, refunds, invoices', platform: 'Outages, errors, performance' } }),
        severity: Decision.rate({ instructions: 'Impact on customer', criteria: ['cosmetic', 'degraded', 'blocking'] }),
        urgent: Decision.probability({ instructions: 'Ticket needs action today' }),
    },
});
```

`DecisionModel.decideSettled(Triage, { input })` answers with `team` as a `Result` over `'billing' | 'platform'` and `severity` over the rating literals, each derived from its criteria, and the root provides a model (`TypeSafeDecisionModel.model('jev-latest')`):
- `DecisionModel.decide` fails unless every decision is answered, and `decideSettled` fails an unanswered decision with an `Unanswered` reason
- `Decision.locate({ image })` targets a `Decision.Image` field on a model built with locate support
