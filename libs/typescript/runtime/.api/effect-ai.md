# [TS_RUNTIME_API_EFFECT_AI]

`@effect/ai` mints the provider-agnostic core every `model/*`, `embed/*`, `tool/*`, and `agent/*` page composes: it defines the `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Chat`/`IdGenerator` service Tags, the `Prompt`/`Response` part algebras, `Tool`/`Toolkit` as `Schema`-typed data, GenAI OpenTelemetry conventions, and — natively — the MCP server (`McpServer`/`McpSchema`). Provider packages never subclass these: each resolves a provider-tagged `Model` (`Model.make(name, layer)`) into these Tags, so a provider is one row on the capability-asymmetry table, never a fork. All generation rides `Effect`/`Stream`; all failure flows through the tagged `AiError` union; every tool-augmented call type-infers its added errors and context from the toolkit (`ExtractError`/`ExtractContext`). `ai/model.ts` wraps `generateText`/`streamText` in the guardrail gate; `ai/agent.ts` composes `Chat.Persistence`; `ai/tool.ts` hosts on `McpServer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai`
- package: `@effect/ai` (MIT)
- entry: `@effect/ai` re-exports namespaces `AiError`, `Chat`, `EmbeddingModel`, `IdGenerator`, `LanguageModel`, `McpSchema`, `McpServer`, `Model`, `Prompt`, `Response`, `Telemetry`, `Tokenizer`, `Tool`, `Toolkit`
- peer floor: `effect ^catalog`, `@effect/platform ^catalog`, `@effect/experimental ^catalog`, `@effect/rpc ^catalog` (MCP transport)
- asset: provider-agnostic LLM service Tags, prompt/response/tool data algebra, stateful chat, embeddings, tokenizer, GenAI OTel conventions, native MCP server
- rail: ai-core

## [02]-[LANGUAGE_MODEL]

[PUBLIC_TYPE_SCOPE]: generation contract + free functions — rail: ai-core

| [INDEX] | [SYMBOL]                                    | [FAMILY]         | [CAPABILITY]                             |
| :-----: | :------------------------------------------ | :--------------- | :--------------------------------------- |
|  [01]   | `LanguageModel.LanguageModel`               | `Context.Tag`    | service tag `"@effect/ai/LanguageModel"` |
|  [02]   | `LanguageModel.generateText`                | function         | text generation (adds Tag to `R`)        |
|  [03]   | `LanguageModel.generateObject`              | function         | `Schema`-shaped structured output        |
|  [04]   | `LanguageModel.streamText`                  | function         | streaming `Response.StreamPart` fold     |
|  [05]   | `LanguageModel.make`                        | constructor      | build `Service` from provider params     |
|  [06]   | `LanguageModel.ToolChoice<Tools>`           | union            | tool-selection policy value              |
|  [07]   | `LanguageModel.GenerateTextResponse`        | class            | text receipt (`.text`/`.usage`/…)        |
|  [08]   | `LanguageModel.GenerateObjectResponse`      | class            | object receipt (`.value`)                |
|  [09]   | `LanguageModel.ProviderOptions`             | interface        | provider-normalized request              |
|  [10]   | `LanguageModel.ConstructorParams`           | interface        | provider implementation seam             |
|  [11]   | `LanguageModel.ExtractError/ExtractContext` | conditional type | toolkit error/context inference          |

[GENERATE_TEXT_OPTIONS]: `GenerateTextOptions.prompt: Prompt.RawInput` `GenerateTextOptions.toolkit: Toolkit.WithHandler<Tools>|Effect.Effect<Toolkit.WithHandler<Tools>,any,any>` `GenerateTextOptions.toolChoice: ToolChoice<{[N in keyof Tools]:Tools[N]["name"]}[keyof Tools]>` `GenerateTextOptions.concurrency: Concurrency` `GenerateTextOptions.disableToolCallResolution: boolean`
[GENERATE_OBJECT_OPTIONS]: `GenerateObjectOptions.objectName: string` `GenerateObjectOptions.schema: Schema.Schema<A,I,R>`
[TOOL_CHOICE]: `ToolChoice = |"auto"|"none"|"required"|{readonly tool:Tools}|{readonly mode?:"auto"|"required";readonly oneOf:ReadonlyArray<Tools>}`
[GENERATE_TEXT_RESPONSE]: `GenerateTextResponse.content: Array<Response.Part<Tools>>` `GenerateTextResponse.text: string` `GenerateTextResponse.reasoning: Array<Response.ReasoningPart>` `GenerateTextResponse.reasoningText: string|undefined` `GenerateTextResponse.toolCalls: Array<Response.ToolCallParts<Tools>>` `GenerateTextResponse.toolResults: Array<Response.ToolResultParts<Tools>>` `GenerateTextResponse.finishReason: Response.FinishReason` `GenerateTextResponse.usage: Response.Usage`
[GENERATE_OBJECT_RESPONSE]: `GenerateObjectResponse.value: A`
[PROVIDER_OPTIONS]: `ProviderOptions.prompt: Prompt.Prompt` `ProviderOptions.tools: ReadonlyArray<Tool.Any>` `ProviderOptions.responseFormat: {readonly type:"text"}|{readonly type:"json";readonly objectName:string;readonly schema:Schema.Schema.Any}` `ProviderOptions.toolChoice: ToolChoice<any>` `ProviderOptions.span: Span`
[CONSTRUCTOR_PARAMS]: `ConstructorParams.generateText: (o:ProviderOptions)=>Effect.Effect<Array<Response.PartEncoded>,AiError.AiError,IdGenerator>` `ConstructorParams.streamText: (o:ProviderOptions)=>Stream.Stream<Response.StreamPartEncoded,AiError.AiError,IdGenerator>`
[SURFACES]: `generateText(Options&GenerateTextOptions<Tools>) -> Effect.Effect<GenerateTextResponse<Tools>,ExtractError<Options>,LanguageModel|ExtractContext<Options>>` `generateObject(Options&GenerateObjectOptions<Tools,A,I,R>) -> Effect.Effect<GenerateObjectResponse<Tools,A>,ExtractError<Options>,LanguageModel|R|ExtractContext<Options>>` `streamText(Options&GenerateTextOptions<Tools>) -> Stream.Stream<Response.StreamPart<Tools>,ExtractError<Options>,LanguageModel|ExtractContext<Options>>` `make(ConstructorParams) -> Effect.Effect<Service>`

## [03]-[MODEL]

[PUBLIC_TYPE_SCOPE]: the capability-asymmetry row abstraction — rail: ai-core

`Model.make(name, layer)` is the provider row: a value that is BOTH a `Layer` (provide it to bind the Tags) AND an `Effect` (yield it to lift the provider's dependencies into a parent). It auto-binds `ProviderName`, so the guardrail/tier-routing folds read the active provider by yielding one Tag. `ai/model.ts` folds every provider (anthropic/openai/google/bedrock/openrouter) onto this one constructor — asymmetry lives in the row's config, never in a parallel API.

[MODEL]: `Model.provider: Provider`
[SURFACES]: `TypeId = "~@effect/ai/Model"` `make(Provider,Layer.Layer<Provides,never,Requires>) -> Model<Provider,Provides,Requires>`

[PROVIDER_ROWS]: the five sibling catalogs this table owner folds — each resolves `model(...)`/`layer(...)` into the core `LanguageModel.LanguageModel` (plus `EmbeddingModel`/`Tokenizer` where the asymmetry column is populated), so provider choice is a single composition-root layer swap. That row grammar (Client `make`/`layer`/`layerConfig` with `Redacted` key + `HttpClient` requirement, per-request `Config` + `withConfigOverride`, the `<Provider>LanguageModel.model`/`.layer` entry — `.modelWithTokenizer` where the row carries a Tokenizer — returning `AiModel.Model<provider,…>`) is uniform across all five; the asymmetry lives in the row config, catalogued per page.

| [INDEX] | [PROVIDER]      | [CATALOG]                     | [EXTRA_ASYMMETRY]                                                      |
| :-----: | :-------------- | :---------------------------- | :--------------------------------------------------------------------- |
|  [01]   | OpenAI          | `effect-ai-openai.md`         | embeddings ×2 + `OpenAiTokenizer` + `OpenAiTelemetry`                  |
|  [02]   | Anthropic       | `effect-ai-anthropic.md`      | `AnthropicTokenizer` Layer + `prepareTools` export                     |
|  [03]   | Google (Gemini) | `effect-ai-google.md`         | raw-client embeddings/token-count only                                 |
|  [04]   | Amazon Bedrock  | `effect-ai-amazon-bedrock.md` | SigV4 creds + native guardrail assessment + Anthropic-on-Bedrock tools |
|  [05]   | OpenRouter      | `effect-ai-openrouter.md`     | aggregator provider-routing + per-response cost metadata               |

## [04]-[EMBEDDING_MODEL]

[PUBLIC_TYPE_SCOPE]: vector embedding + built-in batch/cache — rail: ai-embed

`make` already owns request batching (`maxBatchSize`) and an Effect `Cache` (`capacity`/`timeToLive`); `makeDataLoader` owns time-window coalescing (Scope-scoped). `ai/embed.ts` satisfies the `store/retrieve` `Embedder` port with the `EmbeddingModel` Tag; `ai/embed.ts` feeds `embedMany`.

[SERVICE]: `Service.embed: (input:string)=>Effect.Effect<Array<number>,AiError.AiError>` `Service.embedMany: (input:ReadonlyArray<string>,options?:{…})=>Effect.Effect<Array<Array<number>>,AiError.AiError>`
[RESULT]: `Result.index: number` `Result.embeddings: Array<number>`
[SURFACES]: `make({…}) -> Effect.Effect<Service>` `makeDataLoader({…}) -> Effect.Effect<Service,never,Scope>`

## [05]-[TOOL_AND_TOOLKIT]

[PUBLIC_TYPE_SCOPE]: `Schema`-typed tools as data — rail: ai-tool

`Tool.make` defines a user tool from `parameters`/`success`/`failure` `Schema`s; `providerDefined` declares a provider-executed tool (web search, code exec); `fromTaggedRequest` lifts an existing `Schema.TaggedRequest`. `failureMode: "return"` keeps handler errors in the tool result (self-correcting model loop) instead of the error channel. MCP-aligned annotations (`Readonly`/`Destructive`/`Idempotent`/`OpenWorld`/`Title`) project one-to-one onto MCP tool hints. `Toolkit.make(...tools)` is the collection; `.toLayer(handlers)`/`.toContext` bind handlers; `merge` composes toolkits (later wins). Every provider tool roster (`OpenAiTool`/`AnthropicTool`/`GoogleTool`/`AmazonBedrockTool`) is typed `Tool.ProviderDefined<"<Provider><Name>", { …; failureMode }, RequiresHandler>` — `ProviderDefined`/`FailureMode`/`Any` are the core brands those catalogs return, never per-provider types.

| [INDEX] | [SYMBOL]                                               | [FAMILY]     | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------- | :----------- | :-------------------------------------------------------- |
|  [01]   | `Tool.make`                                            | constructor  | user tool from Schemas                                    |
|  [02]   | `Tool.providerDefined`                                 | constructor  | provider-executed tool                                    |
|  [03]   | `Tool.fromTaggedRequest`                               | constructor  | `TaggedRequest` -> tool                                   |
|  [04]   | `Tool.getJsonSchema`                                   | projection   | `JsonSchema7` for the provider wire                       |
|  [05]   | `Tool.Readonly/Destructive/Idempotent/OpenWorld/Title` | annotation   | MCP tool hints                                            |
|  [06]   | `Tool.isUserDefined/isProviderDefined`                 | guard        | tool discrimination                                       |
|  [07]   | `Toolkit.make`                                         | constructor  | tool collection as data                                   |
|  [08]   | `Toolkit.merge`                                        | combinator   | compose toolkits                                          |
|  [09]   | `Toolkit#toLayer/toContext`                            | provision    | bind handlers -> `Layer`/`Context`                        |
|  [10]   | `Toolkit.empty`                                        | value        | seed / default                                            |
|  [11]   | `Tool.ProviderDefined<Name,Cfg,ReqH>`                  | branded type | the type every provider tool roster returns               |
|  [12]   | `Tool.FailureMode`                                     | union        | `"error" \| "return"` failure-routing policy              |
|  [13]   | `Tool.Any`                                             | erased bound | `Record<string, Tool.Any>` keys every options/toolkit sig |

[SURFACES]: `make` `Name` `providerDefined` `FailureMode` `Any` `ProviderDefined` `getJsonSchema` `fromTaggedRequest` `Toolkit` `empty` `merge` `Toolkits`

## [06]-[PROMPT_AND_RESPONSE]

[PUBLIC_TYPE_SCOPE]: prompt construction + response part algebra — rail: ai-core

`Prompt` is the conversation value: `make`/`fromMessages`/`fromResponseParts` build it, `merge`/`setSystem`/`prependSystem`/`appendSystem` are the context-assembly combinators `ai/model.ts` folds app-passed retrieval into. `RawInput` is what `generateText({ prompt })` accepts (string | encoded messages | `Prompt`). `Response` is the discriminated part union `streamText` yields — the design's streaming fold discriminates on `part.type`.

[SURFACES]: `RawInput` `empty` `make` `fromMessages` `fromResponseParts` `merge` `setSystem` `FinishReason` `Usage` `makePart` `prependSystem` `appendSystem` `stop`

[AUGMENTATION_BASE]: the provider boundary-hook seam — every provider `declare module`-augments these interface twins, never the schema — rail: ai-core

Each `Prompt` message/part carries a `Prompt.ProviderOptions` slot and each `Response` part a `Response.ProviderMetadata` slot (both `Schema.Record<string, Record<string, unknown> | undefined>` bases). A provider package attaches its one optional provider-named key (`openai`/`anthropic`/`google`/`bedrock`/`openrouter`) by `declare module`-augmenting the SAME interfaces below — the roster every sibling catalog's augmentation table targets. Internal code reads canonical shapes; the edge maps the slots.

| [INDEX] | [MODULE]              | [AUGMENTATION_BASE_INTERFACES]                                                         |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `@effect/ai/Prompt`   | `System`/`User`/`Assistant`/`Tool` `MessageOptions`                                    |
|  [02]   | `@effect/ai/Prompt`   | `Text`/`Reasoning`/`File`/`ToolCall`/`ToolResult` `PartOptions`                        |
|  [03]   | `@effect/ai/Response` | `Text{,Start,Delta,End}PartMetadata`                                                   |
|  [04]   | `@effect/ai/Response` | `Reasoning{,Start,Delta,End}PartMetadata`                                              |
|  [05]   | `@effect/ai/Response` | `ToolParams{Start,Delta,End}PartMetadata`                                              |
|  [06]   | `@effect/ai/Response` | `ToolCall`/`ToolResult` `PartMetadata`                                                 |
|  [07]   | `@effect/ai/Response` | `File`/`DocumentSource`/`UrlSource`/`ResponseMetadata`/`Finish`/`Error` `PartMetadata` |

[SURFACES]: `FinishPartMetadata` `FilePartOptions`

## [07]-[CHAT]

[PUBLIC_TYPE_SCOPE]: stateful conversation + persistence — rail: ai-agent

`Chat` maintains history in a `Ref<Prompt>` and mirrors `generateText`/`streamText`/`generateObject` while appending both turns. `export`/`exportJson` serialize; `fromJson`/`fromExport`/`fromPrompt` restore. `Persistence` + `layerPersisted` back durable sessions — the substrate `ai/agent.ts` composes for durable agents over `work` entities.

[SURFACES]: `Chat` `Service` `empty` `fromPrompt` `fromExport` `fromJson` `ChatNotFoundError` `Persistence` `Persisted` `makePersisted` `layerPersisted` `Layer`

## [08]-[TOKENIZER]

[PUBLIC_TYPE_SCOPE]: token counting + budget truncation — rail: ai-token

`ai/model.ts` budgets own this Tag; the two provider tokenizers `AnthropicTokenizer` (bare `Service` value) and `OpenAiTokenizer.make({model})` are the Layers that bind it (`effect-ai-anthropic.md` [04] / `effect-ai-openai.md` [05]); `modelWithTokenizer`/`layerWithTokenizer` fold either into the provided Tags. `truncate` is the budget enforcement — trim a `Prompt` to a token ceiling before generation.

[SERVICE]: `Service.tokenize: (input:Prompt.RawInput)=>Effect.Effect<Array<number>,AiError.AiError>` `Service.truncate: (input:Prompt.RawInput,tokens:number)=>Effect.Effect<Prompt.Prompt,AiError.AiError>`
[SURFACES]: `make({readonly tokenize:(content:Prompt.Prompt)=>Effect.Effect<Array<number>,AiError.AiError>}) -> Service`

## [09]-[MCP_HOSTING]

[PUBLIC_TYPE_SCOPE]: native MCP server + protocol schema — rail: ai-tool

`ai/tool.ts` hosts app toolkits as MCP tools with NO extra dependency: `McpServer.toolkit(toolkit)` registers a `Toolkit` as MCP tools, and one transport Layer (`layerStdio`/`layerHttp`/`layerHttpRouter`) serves it. `resource`/`prompt`/`elicit` round out server capability; `McpSchema` is the full MCP wire (Schema.Class-based) with typed error subclasses. This is the sole MCP host — `@modelcontextprotocol/sdk` is client-only.

| [INDEX] | [SYMBOL]                                | [FAMILY]    | [CAPABILITY]                         |
| :-----: | :-------------------------------------- | :---------- | :----------------------------------- |
|  [01]   | `McpServer.toolkit / registerToolkit`   | tool        | register a `Toolkit` as MCP tools    |
|  [02]   | `McpServer.layerStdio`                  | transport   | stdio-served server Layer            |
|  [03]   | `McpServer.layerHttp / layerHttpRouter` | transport   | HTTP-served server Layer             |
|  [04]   | `McpServer.resource / registerResource` | capability  | resource + typed-param template      |
|  [05]   | `McpServer.prompt / registerPrompt`     | capability  | `Schema`-typed prompt w/ completions |
|  [06]   | `McpServer.elicit`                      | capability  | server-requested structured input    |
|  [07]   | `McpSchema.*`                           | wire        | MCP protocol Schemas + typed errors  |
|  [08]   | `McpSchema.param`                       | constructor | typed resource-template parameter    |

[SURFACES]: `toolkit` `layerStdio` `layerHttp` `resource` `prompt` `elicit` `param` `uri`

## [10]-[TELEMETRY_IDS_ERRORS]

[PUBLIC_TYPE_SCOPE]: GenAI telemetry, id generation, error rail — rail: ai-core

`Telemetry.addGenAIAnnotations(span, opts)` writes OpenTelemetry GenAI semantic-convention attributes (system/operation/request.model/usage.*) onto the current span — the bridge every provider call rides into `@effect/opentelemetry`. `system` draws from `WellKnownSystem` (`anthropic`/`aws.bedrock`/`gemini`/`openai`/…) and `operation.name` from `WellKnownOperationName`; the provider `*Telemetry` modules extend `GenAITelemetryAttributes` via `AttributesWithPrefix<Req, "gen_ai.<provider>.request">` over `AllAttributes` — the seam `OpenAiTelemetry` composes onto (`effect-ai-openai.md` [07]). `IdGenerator` is the pluggable tool-call id source. `AiError` is the one tagged failure union — `Match.tag` dispatch, never ad-hoc throws.

[WELL_KNOWN_SYSTEM]: `WellKnownSystem = "anthropic"|"aws.bedrock"|…`
[WELL_KNOWN_OPERATION_NAME]: `WellKnownOperationName = "chat"|"embeddings"|"text_completion"`
[ATTRIBUTES_WITH_PREFIX]: ``AttributesWithPrefix = {[K in keyof A as `${Prefix}.${K&string}`]:A[K]}``
[ALL_ATTRIBUTES]: `AllAttributes = BaseAttributes&OperationAttributes&TokenAttributes&UsageAttributes&RequestAttributes&ResponseAttributes`
[MAKE_OPTIONS]: `MakeOptions.alphabet: string` `MakeOptions.prefix: string` `MakeOptions.separator: string` `MakeOptions.size: number`
[AI_ERROR]: `AiError = HttpRequestError|HttpResponseError|MalformedInput|MalformedOutput|UnknownError`
[SURFACES]: `addGenAIAnnotations(Span,GenAITelemetryAttributeOptions) -> void` `addSpanAttributes(Span,GenAITelemetryAttributes) -> void` `defaultIdGenerator: Service` `make(MakeOptions) -> Effect.Effect<Service,Cause.IllegalArgumentException>` `layer(MakeOptions) -> Layer.Layer<IdGenerator,Cause.IllegalArgumentException>` `isAiError(unknown) -> u is AiError`

## [11]-[INTEGRATION]

[STACK]: provider rows + the ONE guardrail gate — rail: ai-core
- `ai/model.ts` is a table of `Model.make(name, providerLayer)` rows the app root binds one of; guardrail gate wraps `generateText`/`streamText` in a fold where input moderation runs before the call, output moderation + `Schema`-refusal admission run over the `Response.Part` stream, and a rejected call short-circuits into `AiError` — one admission surface over every row, not a per-provider fork.
- Tier-routing reads `Model.ProviderName` + provider finish-part cost metadata to pick the row; a new provider is a new row.
- `disableToolCallResolution: true` hands tool execution to the guardrail so no handler runs before admission; `failureMode: "return"` keeps a failed tool call in-band for a self-correcting model loop.

[STACK]: universal-tier rails — rail: ai-core
- `effect`: `Schema` types every tool/prompt/response and the `generateObject` output; `Stream` carries `streamText` deltas folded by `part.type`; `Layer`/`Context.Tag` bind every service; `Match.tag` dispatches `AiError`; `Cache`/`Duration` back `EmbeddingModel.make`.
- `@effect/platform`: provider Layers require `HttpClient` — `net/client` supplies the default-policy client (timeout/retry/proxy); `McpServer.layerHttp` composes `HttpRouter`/`HttpLayerRouter`.
- `@effect/opentelemetry`: `Telemetry.addGenAIAnnotations` writes GenAI semconv onto the `ProviderOptions.span`; the `telemetry` folder's OTLP exporter ships them — one span per generation, tool call, and agent run.
- `@effect/experimental` + `@effect/rpc`: `Chat.layerPersisted` durable sessions and the `McpServer` RPC transport ride these peers; `agent/*` composes `Chat.Persistence` over `work` cluster entities.
- `@effect/vitest`: kit-driven law specs exercise providers behind a stub `LanguageModel.make` and drive MCP hosting through an in-memory transport.

[STACK]: `embed` -> `store` `Embedder` port — rail: ai-embed
- `ai/embed.ts` publishes the `EmbeddingModel` Tag (batched + cached via `make`, or window-coalesced via `makeDataLoader`) as the `Layer` the app root wires into the `store/retrieve` `Embedder` port, and `ai` imports no `store` code.
- `ai/model.ts` folds app-passed `store/retrieve` results into a `Prompt` via `merge`/`appendSystem` — retrieval arrives as values, never a `store` import edge.
