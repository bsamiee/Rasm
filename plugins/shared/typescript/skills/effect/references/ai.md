# [AI]

## [01]-[TOOLS]

Tools are schema contracts a `Toolkit` binds to handlers, and one toolkit serves `McpServer`, `LanguageModel`, and a `Chat` alike. `Toolkit` decodes arguments and encodes results through each schema's JSON codec, a declared `Schema.toCodecJson(X)` restating it:
- `Tool.make` defaults `parameters` to `Tool.EmptyParams`, `success` to `Schema.Void`, `failure` to `Schema.Never`, and `failureMode` to `'error'`
- `Tool.dynamic` takes an Effect schema or a raw JSON Schema found at run time, its `success` defaulting to `Schema.Unknown`
- `Tool.providerDefined({ id, customName, providerName })(args)` declares a provider-run tool, `requiresHandler: true` adding a local result handler
- `Tool.Readonly`, `Destructive`, `Idempotent`, and `OpenWorld` annotate behavior
- Unannotated tools advertise themselves destructive and open-world, and `Readonly` tools non-destructive
- `Tool.Title` and `Tool.Meta` annotate the listed title and MCP `_meta`
- `Tool.getDescription` returns `description` or a description annotation on `parameters`
- `Tool.Strict` set `true` makes `McpServer` reject excess keys and advertise `additionalProperties: false`
- Raw JSON Schema tools under `Tool.Strict` die at registration
- `failureMode: 'return'` returns a typed handler failure as the result for `LanguageModel`, and a defect dies
- `dependencies: [S]` makes `S` a per-request service a middleware or transport provides to the handler
- `execution: 'client'` declares a tool the requesting application runs, its calls emitted unresolved with no handler
- `toModelOutput(result)` sends content parts (images, files) to the model in place of an encoded result, callers keeping a typed one
- `Toolkit.merge` joins toolkits with later tools winning a name, and `kit.toLayer(kit.of({...}))` binds handlers

## [02]-[MCP_SERVER]

`McpServer` holds tools, resources, templates, prompts, and skills a transport serves, and a request-level service reaches handlers through middleware:

```ts
class Caller extends Context.Service<Caller, { readonly account: string; readonly admin: boolean }>()('@app/mcp/Caller') {}

const Reads = Toolkit.make(Tool.make('balance', { description: 'Balance of calling account', success: Schema.Struct({ balance: Schema.Finite }), failure: LedgerError, dependencies: [Caller] }).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false))));
const Writes = Toolkit.make(Tool.make('freeze', { description: 'Freezes an account', parameters: Schema.Struct({ account: Schema.NonEmptyString }), failure: LedgerError, dependencies: [Caller] }).annotate(Tool.OpenWorld, false));

const Authenticated = McpServer.middleware<{ provides: Caller }>()(
    Effect.gen(function* () {
        const tokens = yield* Tokens;
        return (effect, { headers }) => Effect.flatMap(tokens.resolve(Headers.get(headers, 'authorization')), (caller) => Effect.provideService(effect, Caller, caller));
    }),
);

const Server = McpServer.layerHttp({
    name: 'ledger',
    version,
    path: '/mcp',
    protocols: [McpProtocol.v2025_11_25],
    registrations: Layer.mergeAll(
        McpServer.toolkit(Reads).pipe(Layer.provide([Authenticated.layer, Reads.toLayer(Accounts.useSync((accounts) => Reads.of({ balance: () => Caller.use(({ account }) => Effect.map(accounts.balance(account), (balance) => ({ balance }))) })))])),
        McpServer.toolkit(Writes).pipe(Layer.provide([McpServer.visibility(() => Caller.useSync(Struct.get('admin'))).combine(Authenticated).layer, Writes.toLayer(Accounts.useSync((accounts) => Writes.of({ freeze: ({ account }) => accounts.freeze(account) })))])),
    ),
});
```

`LedgerError` and `version` are the package error and version, `Accounts` holds `balance` and `freeze`, and `Tokens.resolve` reads a token to a `Caller` or fails with an `McpSchema.McpErrorBase` error. `Server` requires `HttpRouter`, `Accounts`, and `Tokens` alone. Handler services become `McpServer.McpRequest<'Requires', S>` markers, and a `Layer.provide` of `S` keeps them. The layer a middleware enters through selects what it wraps:

| [INDEX] | [FORM]                              | [WRAPS]                                                        |
| :-----: | :---------------------------------- | :------------------------------------------------------------- |
|  [01]   | `m.layer`                           | Registrations it is provided to                                |
|  [02]   | `m.layerGlobal`, `{ global: true }` | Every registration, outside local middleware, run once         |
|  [03]   | `a.combine(b).layer`                | Registrations it reaches, `b` running outside `a`              |
|  [04]   | `McpServer.visibility(check)`       | Hides registrations per caller, list requests running it alone |
|  [05]   | `McpSchema.EnabledWhen` annotation  | Hides registrations per client protocol and capability         |

Middleware wraps `tools/call`, `resources/read`, `prompts/get`, `completion/complete`, and `skills/get`, never `initialize`, and reads `{ method, target, payload, requestContext, headers, sessionId }`:
- `target` holds a tool name, read URI, prompt name, completion reference, or a skill's `SKILL.md` URI
- `MiddlewareMethod` matches keep a default arm
- Middleware wraps a handler's run and neither inspects nor replaces its result
- `{ global: true }` returns a layer requiring `McpServer` with no `.combine`
- `McpServer.registerToolkit`, `registerResource`, and `registerPrompt` register from inside an effect, reading the services it runs with

Failures reach a client by their kind and the protocol a session speaks:

| [INDEX] | [FAILURE]                              | [CLIENT_SEES]                                                           |
| :-----: | :------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Declared `failure`                     | `isError` text: an `Error` message, or its JSON encoding in return mode |
|  [02]   | Parameter decode, `v2025_11_25` onward | `isError` text `Invalid parameters for tool`                            |
|  [03]   | Parameter decode, older protocols      | JSON-RPC `InvalidParams`                                                |
|  [04]   | Undeclared failure                     | One generic message, the cause logged and sent to `ErrorReporter`       |
|  [05]   | Middleware `McpSchema.McpErrorBase`    | JSON-RPC error with its code                                            |
|  [06]   | Defect                                 | JSON-RPC `InternalError`                                                |

Tool successes send `structuredContent` with one JSON text block. Protocols `v2025_06_18` and `v2025_11_25` drop a non-object `structuredContent` and `outputSchema`, and tools return a struct:
- `Schema.Void` successes send empty content, and `McpServer.toolkit(kit, { content })` maps results to content blocks, dropping `structuredContent`
- `McpServer.resource` templates take one schema per `${}` hole, `McpSchema.param(name, schema)` naming it, with no `{?query}` expansion
- Resource and prompt failures reach a client as `InternalError` with their message
- Template and prompt parameter decode failures reach a client as `InvalidParams`
- `McpServer.prompt({ name, parameters, content })` derives argument requirement from field optionality and description from annotations
- `McpSchema.optional(S)` declares a field a client omits, an explicit `undefined` encoding as no key
- `McpServer.skill` and `registerSkill` serve a skill's `SKILL.md` and files
- `McpSchema.ProgressToken` decodes `requestMetadata.progressToken` of `McpSchema.McpRequestContext`
- `server.notifications['notifications/progress']` reports against a decoded progress token
- `McpServer.elicit({ message, schema })` decodes an answer and fails a decline or transport failure with `ElicitationDeclined`
- `McpServer.elicit` requires the `McpServerClient` a sessionful protocol supplies

`McpServer` traces each request and serves it over a transport layer:
- Requests run in a server span named `<method> <target>`, a `traceparent` in `params._meta` becoming its parent with the transport span linked
- Request spans hold `mcp.*`, `jsonrpc.request.id`, `network.transport`, and `gen_ai.*` attributes
- `Toolkit.RecordToolArguments` adds `gen_ai.tool.call.arguments` to request spans
- `McpServer` records no metric, and a global middleware timing each handler records one inside its request span
- `McpServer.layerStdio` holds stdout for the protocol, a server logging through `Logger.LogToStderr`
- `layerHttp` refuses a request with an `Origin` header `allowedOrigins` does not list
- `layerHttpRequestScoped` serves one server per request over stateless `v2026_07_28` alone
- `McpProtocol` exports one adapter per version and no list, `protocols` naming each version a server serves

## [03]-[MCP_CLIENT]

`McpClient` reads a remote server over protocols `v2025_11_25` and `v2026_07_28` and adapts its tools for a language model:
- `McpClient.layerHttp({ name, version, protocol, url })` reads `HttpClient`
- `McpClient.layerStdio({ name, version, protocol, command })` spawns through `ChildProcessSpawner`
- `client.callTool({ tool, arguments, schema })` takes a descriptor `listTools` returned and decodes `structuredContent` through `schema`
- `client.callTool` fails an `isError` result with reason `ToolError`
- `McpClient.toolkit(client, prefix)` returns an effect of a handled toolkit `generateText` takes, with no `toLayer` and no `Toolkit.merge`
- `McpClient.make` over `McpClient.http` or `McpClient.stdio` builds a client per protocol where a caller falls back across them
- `McpClientError.reason` separates `ToolError`, `ValidationError`, `ConfigurationError`, and `TimeoutError`
- Client calls run unbounded unless given `timeout`

## [04]-[LANGUAGE_MODELS]

`LanguageModel` generates text, objects, and streams through a provider layer of a provider package (`@effect/ai-openai`, `@effect/ai-anthropic`):
- `LanguageModel.generateText({ prompt, toolkit, toolChoice })` resolves tool calls through the toolkit, `toolChoice` typed by its tool names
- `disableToolCallResolution: true` returns tool calls unresolved, `toolCallValidation: 'deferred'` leaving their parameters `unknown`
- `LanguageModel.generateObject({ prompt, schema })` decodes `response.value` through `schema`, failing `AiError` with reason `StructuredOutputError`
- `allowEmpty: true` on `generateObject` returns `Option.none()` for empty output
- `LanguageModel.streamText` streams response parts, each narrowed on `part.type`
- `needsApproval` on a tool yields an approval request, answered next turn with `Prompt.toolApprovalResponsePart`
- `Chat.empty` and `Chat.fromPrompt` start a session keeping history
- `Chat.Persistence` of `Chat.layerPersisted({ storeId })` returns chats saved each turn through `getOrCreate(chatId)`
- `chat.streamText({ maxSteps })` runs tool rounds until the model stops calling tools, a call stays unanswered, or `maxSteps` ends
- `Model.make(provider, name, layer)` names a provider's model layer for an `ExecutionPlan` to fall back across
- `isRetryable` and `retryAfter` read an `AiError` reason, a `RateLimitError` always retryable
- `EmbeddingModel.make({ batchKey })` batches concurrent `embed` calls into one `embedMany` per key
- `LanguageModel.make` builds a model from provider functions, a test double answering with fixed parts
- `AgUi.toPrompt` converts an AG-UI run input and `AgUi.fromStreamParts` stream parts to events, `AgUiServer.layerHttp({ path, agent })` serving them

## [05]-[DECISIONS]

`Decision` declares classification, rating, probability, and location questions over one schema-typed input, and a `DecisionModel` answers them in one provider call:

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

`DecisionModel.decideSettled(Triage, { input })` answers `outcomes.team` with a `Result` of a label over `'billing' | 'platform'` and its probabilities, and `severity` with a rating literal and a weighted `rating` position:
- `decideSettled` answers an unanswered decision with `Result.fail` of an `Unanswered` case (`Missing`, `Refused`, `NoDistribution`, `Invalid`)
- `decideSettled` fails on encoding and provider errors alone
- `DecisionModel.decide` fails on the first unanswered decision, a refusal with `ContentPolicyError`
- `Decision.locate({ instructions, image })` answers one bounding box over a `Decision.Image` field
- Models answering `Decision.locate` are built with `supportsImages` and `supportsLocate`

## [06]-[CHAT_PLATFORMS]

`effect/chat` drives chat platforms through adapters, and its `Chat` service routes each call by the platform a `ChatThread` names:
- `Chat.layer` provides the router and an empty `ChatAdapter.Registry`, each adapter layer stacking on it through `Layer.provideMerge`
- `chat.post(ChatThread.make(platform, id), { markdown })` posts one message, a platform lacking an operation failing with an `Unsupported` reason
- `ChatError` reasons classify retry as `'retry'`, `'uncertain'`, or `'denied'`, and `ChatAdapter.retrySafe` retries `'retry'` alone
- `ChatMarkdown.post` splits Markdown outgrowing one message
- `ChatMarkdown.stream(thread, deltas)` writes a `LanguageModel` text stream as it arrives
- Adapters deliver inbound events to `ChatHandler` at least once, `NotAccepted` refusing one and `ChatEvent.dedupeKey` keying its deliveries
- `ChatInteraction.button`, `select`, `modal`, and `command` declare interactions, `ChatInteraction.group` binding their handlers
- `ChatTest.conformance` checks an adapter against the contract
