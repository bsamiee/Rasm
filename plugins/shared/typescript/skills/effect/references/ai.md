# [AI]

## [01]-[TOOLS]

Tools are schema contracts a `Toolkit` binds to handlers, and one toolkit serves `McpServer`, `LanguageModel`, and `Chat` alike. `Toolkit` decodes parameters and encodes results through each schema's JSON codec (`Schema.toCodecJson`), so a tool declares its domain schemas unwrapped:
- `Tool.make` defaults `parameters` to `Tool.EmptyParams`, `success` to `Schema.Void`, `failure` to `Schema.Never`, and `failureMode` to `'error'`
- `Tool.dynamic` takes an Effect schema or a raw JSON Schema read at run time, `success` defaulting to `Schema.Unknown`
- `Tool.providerDefined({ id, customName, providerName })(args)` declares a provider-run tool, `requiresHandler: true` adding a local handler
- `failureMode: 'return'` hands a declared handler failure back as the tool result, and every other failure fails the call
- `dependencies: [S]` makes `S` a per-request service a middleware or transport provides to the handler
- `execution: 'client'` declares a tool the requesting application runs, its calls left unresolved with no handler
- `needsApproval` holds a call for an approval the next turn answers through `Prompt.toolApprovalResponsePart`
- `toModelOutput(result)` sends content parts (images, files) to the model, and callers keep the typed result
- `Toolkit.merge` joins toolkits with a later tool replacing an earlier one of its name, and `kit.toLayer(kit.of({...}))` binds handlers

Annotations state behavior to MCP clients and model providers, and `annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.OpenWorld, false)))` sets them in one call:
- `Tool.Readonly` and `Tool.Idempotent` default `false`, `Tool.OpenWorld` `true`, and an unset `Tool.Destructive` lists as `!Readonly`
- `Tool.Title` sets the listed title, and `Tool.Meta` the listed `_meta` (`{ 'anthropic/alwaysLoad': true }`)
- `Tool.getDescription` returns `description` or the description annotation on `parameters`
- Tool lists send that description and the `title`, `description`, `default`, and `examples` annotations of `parameters`, `expected` and `message` reaching no client
- Handlers add a next call to a library failure where the toolkit is declared, built from the tool (`tool.name`, `parametersSchema`, `Tool.Parameters<typeof tool>`)
- `Tool.Strict` set `true` makes `McpServer` refuse excess keys and list `additionalProperties: false`, a raw JSON Schema tool dying at registration
- `Tool.Strict` left `undefined` defers to the provider's setting, and `false` sends `strict: false`

## [02]-[LANGUAGE_MODELS]

`LanguageModel` generates text, objects, and streams through a provider layer of a provider package (`@effect/ai-openai`, `@effect/ai-anthropic`):
- `LanguageModel.generateText({ prompt, toolkit, toolChoice })` resolves tool calls through the toolkit, `toolChoice` typed by its tool names
- `disableToolCallResolution: true` returns tool calls unresolved, `toolCallValidation: 'deferred'` leaving their parameters `unknown`
- `LanguageModel.generateObject({ prompt, schema })` decodes `response.value` through `schema`, failing `AiError` with reason `StructuredOutputError`
- `allowEmpty: true` on `generateObject` returns `Option.none()` for empty output
- `LanguageModel.streamText` streams response parts, each narrowed on `part.type`
- `Chat.empty` and `Chat.fromPrompt` start a session keeping history
- `Chat.layerPersisted({ storeId })` provides `Chat.Persistence`, whose `getOrCreate(chatId)` returns a chat saved each turn
- `chat.streamText({ maxSteps })` runs tool rounds until a round needs no tool, finishes incomplete, leaves a call unanswered, or `maxSteps` ends
- `Model.make(provider, name, layer)` names a provider's model layer for an `ExecutionPlan` to fall back across
- `isRetryable` and `retryAfter` of an `AiError` read its reason, a `RateLimitError` always retryable
- `EmbeddingModel.make({ embedMany, batchKey })` batches concurrent `embed` calls into one `embedMany` per `batchKey(context)` value
- `EmbeddingModel` batches run in their first caller's context, so `batchKey` returns every service `embedMany` reads
- `LanguageModel.make({ generateText, streamText })` builds the service from provider functions
- `AgUi.toPrompt` converts an AG-UI run input and `AgUi.fromStreamParts` stream parts to events, `AgUiServer.layerHttp({ path, agent })` serving them

## [03]-[DECISIONS]

`Decision.make({ input, decisions })` declares questions over one schema-typed input, and a `DecisionModel` answers them in one provider call:
- `Decision.classify({ instructions, criteria })` maps each label to its description, and `Decision.rate` lists ordered levels
- `classify` and `rate` throw at construction when `criteria` offer no choice
- `Decision.probability` asks for a probability, and `Decision.locate({ instructions, image })` for one box over a `Decision.Image` field
- `DecisionModel.decideSettled(definition, { input })` answers each key with a `Result`, a label or rating level with its probabilities
- Rating answers hold a weighted `rating` position beside the level
- Unanswered keys hold `Result.fail` of an `Unanswered` with `reason` `Missing`, `Refused`, `NoDistribution`, or `Invalid`
- `decideSettled` fails on encoding and provider errors alone
- `DecisionModel.decide` fails on the first unanswered decision, a refusal with `ContentPolicyError`
- Models answering images or `Decision.locate` are built with `supportsImages` and `supportsLocate`

## [04]-[CHAT_PLATFORMS]

`effect/chat` drives chat platforms through adapters, and its `Chat` service routes each call by the platform a `ChatThread` names:
- `Chat.layer` provides the router and an empty `ChatAdapter.Registry`, each adapter layer stacking on it through `Layer.provideMerge`
- `chat.post(ChatThread.make(platform, id), { markdown })` posts one message, a platform lacking an operation failing with an `Unsupported` reason
- `ChatError` reasons classify retry as `'retry'`, `'uncertain'`, or `'denied'`, and `ChatAdapter.retrySafe` retries `'retry'` alone
- `ChatMarkdown.post` splits Markdown outgrowing one message
- `ChatMarkdown.stream(thread, deltas)` writes a `LanguageModel` text stream as it arrives
- Adapters deliver inbound events to `ChatHandler` at least once, `NotAccepted` refusing one and `ChatEvent.dedupeKey` keying its deliveries
- `ChatInteraction.button`, `select`, `modal`, and `command` declare interactions, `ChatInteraction.group` binding their handlers
