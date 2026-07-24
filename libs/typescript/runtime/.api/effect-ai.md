# [TS_RUNTIME_API_EFFECT_AI]

`@effect/ai` mints the provider-agnostic LLM core — service tags, the `Prompt`/`Response` part algebras, `Tool`/`Toolkit` as `Schema`-typed data, GenAI OpenTelemetry conventions, and the native MCP server.

Every provider resolves a `Model.make` row into these tags, so provider choice is one composition-root layer swap.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai`
- package: `@effect/ai` (MIT)
- module: per-namespace subpath exports (`@effect/ai/LanguageModel`), `sideEffects:[]`
- runtime: node|browser; peers `effect`, `@effect/platform`, `@effect/experimental`, `@effect/rpc`
- asset: provider-agnostic LLM service tags, prompt/response/tool data algebra, stateful chat, embeddings, tokenizer, GenAI OTel conventions, native MCP server
- rail: ai-core

## [02]-[LANGUAGE_MODEL]

[PUBLIC_TYPE_SCOPE]: generation contract and the free functions binding it

| [INDEX] | [SYMBOL]                                    | [FAMILY]         | [CAPABILITY]                             |
| :-----: | :------------------------------------------ | :--------------- | :--------------------------------------- |
|  [01]   | `LanguageModel.LanguageModel`               | `Context.Tag`    | service tag `"@effect/ai/LanguageModel"` |
|  [02]   | `LanguageModel.generateText`                | function         | text generation (adds tag to `R`)        |
|  [03]   | `LanguageModel.generateObject`              | function         | `Schema`-shaped structured output        |
|  [04]   | `LanguageModel.streamText`                  | function         | streaming `Response.StreamPart` fold     |
|  [05]   | `LanguageModel.make`                        | constructor      | build `Service` from provider params     |
|  [06]   | `LanguageModel.ToolChoice<Tools>`           | union            | tool-selection policy value              |
|  [07]   | `LanguageModel.GenerateTextResponse`        | class            | text receipt (`.text`/`.usage`/…)        |
|  [08]   | `LanguageModel.GenerateObjectResponse`      | class            | object receipt (`.value`)                |
|  [09]   | `LanguageModel.ProviderOptions`             | interface        | provider-normalized request              |
|  [10]   | `LanguageModel.ConstructorParams`           | interface        | provider implementation seam             |
|  [11]   | `LanguageModel.ExtractError/ExtractContext` | conditional type | toolkit error/context inference          |

[GENERATE_TEXT_OPTIONS]: `prompt: Prompt.RawInput` `toolkit: Toolkit.WithHandler<Tools> | Effect.Effect<…>` `toolChoice: ToolChoice<Tools>` `concurrency: Concurrency` `disableToolCallResolution: boolean`
[GENERATE_OBJECT_OPTIONS]: `objectName: string` `schema: Schema.Schema<A,I,R>`
[TOOL_CHOICE]: `ToolChoice = "auto" | "none" | "required" | {tool} | {mode?:"auto"|"required"; oneOf}`
[GENERATE_TEXT_RESPONSE]: `content: Array<Response.Part<Tools>>` `text: string` `reasoning: Array<Response.ReasoningPart>` `reasoningText?: string` `toolCalls: Array<Response.ToolCallParts<Tools>>` `toolResults: Array<Response.ToolResultParts<Tools>>` `finishReason: Response.FinishReason` `usage: Response.Usage` (`GenerateObjectResponse` extends it with `value: A`)
[PROVIDER_OPTIONS]: `prompt: Prompt.Prompt` `tools: ReadonlyArray<Tool.Any>` `responseFormat: {type:"text"} | {type:"json"; objectName; schema}` `toolChoice: ToolChoice<any>` `span: Span`
[CONSTRUCTOR_PARAMS]: `generateText: (ProviderOptions) => Effect<Array<Response.PartEncoded>, AiError, IdGenerator>` `streamText: (ProviderOptions) => Stream<Response.StreamPartEncoded, AiError, IdGenerator>`
[SURFACES]: `generateText(GenerateTextOptions) -> Effect<GenerateTextResponse, ExtractError, LanguageModel|ExtractContext>` `generateObject(GenerateObjectOptions) -> Effect<GenerateObjectResponse, ExtractError, LanguageModel|R|ExtractContext>` `streamText(GenerateTextOptions) -> Stream<Response.StreamPart, ExtractError, LanguageModel|ExtractContext>` `make(ConstructorParams) -> Effect<Service>`

## [03]-[MODEL]

[PUBLIC_TYPE_SCOPE]: the capability-asymmetry row abstraction

`Model.make(name, layer)` returns a value that is BOTH a `Layer` (provide it to bind the tags) AND an `Effect` (yield it to lift the provider's dependencies into a parent); it auto-binds `ProviderName`, so a tier-routing fold reads the active provider by yielding one tag. Asymmetry lives in the row's config, never a parallel API.

[MODEL]: `provider: Provider`
[SURFACES]: `TypeId = "~@effect/ai/Model"` `make(Provider, Layer.Layer<Provides,never,Requires>) -> Model<Provider,Provides,Requires>`

[PROVIDER_ROWS]: each sibling catalog resolves its `model(...)`/`layer(...)` into the core `LanguageModel` tag, and `EmbeddingModel`/`Tokenizer` where the asymmetry column populates. Every row shares one grammar — a Client `make`/`layer`/`layerConfig` binding a `Redacted` key and requiring `HttpClient`, a per-request `Config` with `withConfigOverride`, and a `<Provider>LanguageModel.model`/`.layer` (`.modelWithTokenizer` where the row carries a tokenizer) returning `Model<provider,…>`, the asymmetry catalogued per page.

| [INDEX] | [PROVIDER]      | [CATALOG]                     | [EXTRA_ASYMMETRY]                                                      |
| :-----: | :-------------- | :---------------------------- | :--------------------------------------------------------------------- |
|  [01]   | OpenAI          | `effect-ai-openai.md`         | embeddings ×2 + `OpenAiTokenizer` + `OpenAiTelemetry`                  |
|  [02]   | Anthropic       | `effect-ai-anthropic.md`      | `AnthropicTokenizer` layer + `prepareTools` export                     |
|  [03]   | Google (Gemini) | `effect-ai-google.md`         | raw-client embeddings/token-count only                                 |
|  [04]   | Amazon Bedrock  | `effect-ai-amazon-bedrock.md` | SigV4 creds + native guardrail assessment + Anthropic-on-Bedrock tools |
|  [05]   | OpenRouter      | `effect-ai-openrouter.md`     | aggregator provider-routing + per-response cost metadata               |

## [04]-[EMBEDDING_MODEL]

[PUBLIC_TYPE_SCOPE]: vector embedding with built-in batch and cache

`make` owns request batching (`maxBatchSize`) and an Effect `Cache` (`capacity`/`timeToLive`); `makeDataLoader` owns Scope-scoped time-window coalescing.

[SERVICE]: `embed: (input:string) => Effect<Array<number>, AiError>` `embedMany: (input:ReadonlyArray<string>, options?) => Effect<Array<Array<number>>, AiError>`
[RESULT]: `index: number` `embeddings: Array<number>`
[SURFACES]: `make({…}) -> Effect<Service>` `makeDataLoader({…}) -> Effect<Service, never, Scope>`

## [05]-[TOOL_AND_TOOLKIT]

[PUBLIC_TYPE_SCOPE]: `Schema`-typed tools as data

`Tool.make` defines a user tool from `parameters`/`success`/`failure` `Schema`s; `providerDefined` declares a provider-executed tool; `fromTaggedRequest` lifts a `Schema.TaggedRequest`. `failureMode: "return"` keeps handler errors in the tool result for a self-correcting loop; the MCP-aligned annotations project onto MCP tool hints. `Toolkit.make(...tools)` collects, `toLayer`/`toContext` bind handlers, `merge` composes toolkits (later wins). Every provider tool roster returns the core `Tool.ProviderDefined`/`FailureMode`/`Any` brands, never a per-provider type.

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

## [06]-[PROMPT_AND_RESPONSE]

[PUBLIC_TYPE_SCOPE]: prompt construction and the response part algebra

`Prompt` is the conversation value: `make`/`fromMessages`/`fromResponseParts` build it, `merge`/`setSystem`/`prependSystem`/`appendSystem` assemble context; `RawInput` is what `generateText({ prompt })` accepts (string, encoded messages, or `Prompt`). `Response` is the discriminated part union `streamText` yields, folded on `part.type`.

[SURFACES]: `RawInput` `empty` `make` `fromMessages` `fromResponseParts` `merge` `setSystem` `prependSystem` `appendSystem` `makePart` `FinishReason` `Usage`

[AUGMENTATION_BASE]: the provider boundary-hook seam — every provider `declare module`-augments these interface twins, never the schema

Each `Prompt` message/part carries a `Prompt.ProviderOptions` slot and each `Response` part a `Response.ProviderMetadata` slot (both `Schema.Record<string, Record<string, unknown> | undefined>` bases). A provider attaches its one optional provider-named key by augmenting the same interfaces below; internal code reads canonical shapes, and the edge maps the slots.

| [INDEX] | [MODULE]              | [AUGMENTATION_BASE_INTERFACES]                                                         |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `@effect/ai/Prompt`   | `System`/`User`/`Assistant`/`Tool` `MessageOptions`                                    |
|  [02]   | `@effect/ai/Prompt`   | `Text`/`Reasoning`/`File`/`ToolCall`/`ToolResult` `PartOptions`                        |
|  [03]   | `@effect/ai/Response` | `Text{,Start,Delta,End}PartMetadata`                                                   |
|  [04]   | `@effect/ai/Response` | `Reasoning{,Start,Delta,End}PartMetadata`                                              |
|  [05]   | `@effect/ai/Response` | `ToolParams{Start,Delta,End}PartMetadata`                                              |
|  [06]   | `@effect/ai/Response` | `ToolCall`/`ToolResult` `PartMetadata`                                                 |
|  [07]   | `@effect/ai/Response` | `File`/`DocumentSource`/`UrlSource`/`ResponseMetadata`/`Finish`/`Error` `PartMetadata` |

## [07]-[CHAT]

[PUBLIC_TYPE_SCOPE]: stateful conversation and persistence

`Chat` keeps history in a `Ref<Prompt>` and mirrors `generateText`/`streamText`/`generateObject` while appending both turns; `export`/`exportJson` serialize, `fromJson`/`fromExport`/`fromPrompt` restore, and `Persistence` + `layerPersisted` back durable sessions.

[SURFACES]: `Chat` `Service` `empty` `fromPrompt` `fromExport` `fromJson` `ChatNotFoundError` `Persistence` `Persisted` `makePersisted` `layerPersisted` `Layer`

## [08]-[TOKENIZER]

[PUBLIC_TYPE_SCOPE]: token counting and budget truncation

Two provider tokenizers bind this tag — `AnthropicTokenizer` a bare `Service` value, `OpenAiTokenizer.make({model})` a factory — and `modelWithTokenizer`/`layerWithTokenizer` fold either into the provided tags; `truncate` trims a `Prompt` to a token ceiling before generation.

[SERVICE]: `tokenize: (input:Prompt.RawInput) => Effect<Array<number>, AiError>` `truncate: (input:Prompt.RawInput, tokens:number) => Effect<Prompt.Prompt, AiError>`
[SURFACES]: `make({tokenize: (Prompt.Prompt) => Effect<Array<number>, AiError>}) -> Service`

## [09]-[MCP_HOSTING]

[PUBLIC_TYPE_SCOPE]: native MCP server and protocol schema

`McpServer.toolkit(toolkit)` registers a `Toolkit` as MCP tools with no extra dependency, and one transport layer (`layerStdio`/`layerHttp`/`layerHttpRouter`) serves it; `resource`/`prompt`/`elicit` round out server capability, and `McpSchema` is the full `Schema.Class`-based MCP wire with typed error subclasses. This is the sole MCP host.

| [INDEX] | [SYMBOL]                                | [FAMILY]    | [CAPABILITY]                         |
| :-----: | :-------------------------------------- | :---------- | :----------------------------------- |
|  [01]   | `McpServer.toolkit / registerToolkit`   | tool        | register a `Toolkit` as MCP tools    |
|  [02]   | `McpServer.layerStdio`                  | transport   | stdio-served server layer            |
|  [03]   | `McpServer.layerHttp / layerHttpRouter` | transport   | HTTP-served server layer             |
|  [04]   | `McpServer.resource / registerResource` | capability  | resource + typed-param template      |
|  [05]   | `McpServer.prompt / registerPrompt`     | capability  | `Schema`-typed prompt w/ completions |
|  [06]   | `McpServer.elicit`                      | capability  | server-requested structured input    |
|  [07]   | `McpSchema.*`                           | wire        | MCP protocol Schemas + typed errors  |
|  [08]   | `McpSchema.param`                       | constructor | typed resource-template parameter    |

## [10]-[TELEMETRY_IDS_ERRORS]

[PUBLIC_TYPE_SCOPE]: GenAI telemetry, id generation, error rail

`Telemetry.addGenAIAnnotations(span, opts)` writes GenAI semantic-convention attributes onto the span; a provider `*Telemetry` module extends `GenAITelemetryAttributes` via `AttributesWithPrefix` over `AllAttributes`. `IdGenerator` is the pluggable tool-call id source; `AiError` is the one tagged failure union under `Match.tag` dispatch.

[WELL_KNOWN_SYSTEM]: `WellKnownSystem = "anthropic" | "aws.bedrock" | …`
[WELL_KNOWN_OPERATION_NAME]: `WellKnownOperationName = "chat" | "embeddings" | "text_completion"`
[ATTRIBUTES_WITH_PREFIX]: ``AttributesWithPrefix = {[K in keyof A as `${Prefix}.${K&string}`]: A[K]}``
[ALL_ATTRIBUTES]: `AllAttributes = BaseAttributes & OperationAttributes & TokenAttributes & UsageAttributes & RequestAttributes & ResponseAttributes`
[MAKE_OPTIONS]: `alphabet: string` `prefix?: string` `separator: string` `size: number`
[AI_ERROR]: `AiError = HttpRequestError | HttpResponseError | MalformedInput | MalformedOutput | UnknownError`
[SURFACES]: `addGenAIAnnotations(Span, GenAITelemetryAttributeOptions) -> void` `addSpanAttributes(Span, GenAITelemetryAttributes) -> void` `defaultIdGenerator: Service` `make(MakeOptions) -> Effect<Service, Cause.IllegalArgumentException>` `layer(MakeOptions) -> Layer<IdGenerator, Cause.IllegalArgumentException>` `isAiError(unknown) -> u is AiError`

## [11]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Generation rides `Effect`/`Stream`; failure flows through the tagged `AiError` union under `Match.tag` dispatch; a new provider is a new `Model.make` row, never a fork.

[STACKING]:
- `effect`: `Schema` types every tool/prompt/response and the `generateObject` output; `Stream` folds `streamText` deltas by `part.type`; `Layer`/`Context.Tag` bind every service; `Cache`/`Duration` back `EmbeddingModel.make`.
- `effect-ai-openai.md`/`effect-ai-anthropic.md`/`effect-ai-google.md`/`effect-ai-amazon-bedrock.md`/`effect-ai-openrouter.md`: each `model(...)`/`layer(...)` resolves the core `LanguageModel`/`EmbeddingModel`/`Tokenizer` tags; `[03]` catalogs the per-provider asymmetry.
- `effect-opentelemetry.md` (`@effect/opentelemetry`): `Telemetry.addGenAIAnnotations` writes GenAI semconv onto `ProviderOptions.span`, one span per generation, tool call, and agent run over the OTLP rail.
- `effect-rpc.md` + `@effect/experimental`: `Chat.layerPersisted` durable sessions and the `McpServer` RPC transport ride these peers.
- `modelcontextprotocol-sdk.md` (`@modelcontextprotocol/sdk`): admits as MCP client only; `McpServer.toolkit` registering a `Toolkit` under one transport layer is the sole host path.
- `@effect/platform`: every provider `layer*` requires `HttpClient` from `net/client`; `McpServer.layerHttp` composes `HttpRouter`.
- `ai/model.ts`: folds the provider rows into one guardrail gate over `generateText`/`streamText` — input moderation before the call, output moderation and `Schema`-refusal admission over the `Response.Part` stream, a rejected call short-circuiting into `AiError`; tier-routing reads `Model.ProviderName` and finish-part cost metadata; `disableToolCallResolution: true` hands execution to the gate, `failureMode: "return"` keeps a failed call in-band.
- `ai/embed.ts`: publishes the `EmbeddingModel` tag as the `Layer` wired into the `store/retrieve` `Embedder` port, retrieval folding into a `Prompt` via `merge`/`appendSystem`; `ai/agent.ts` composes `Chat.Persistence` over `work` cluster entities.

[LOCAL_ADMISSION]:
- `@effect/ai` with its five provider siblings is the admitted LLM surface; `@modelcontextprotocol/sdk` admits as MCP client only, never a second host.

[RAIL_LAW]:
- Package: `@effect/ai`
- Owns: the provider-agnostic LLM contract — service tags, prompt/response/tool algebra, chat, embeddings, tokenizer, GenAI telemetry, MCP hosting.
- Accept: one `Model.make` row per provider resolved into the shared tags, every call folded through the `ai/model.ts` guardrail gate.
- Reject: a per-provider generation API, an ad-hoc `throw`, a hand-rolled MCP host, a second embedding or tokenizer port.
