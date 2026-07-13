# [TS_RUNTIME_API_EFFECT_AI]

The provider-agnostic core every `model/*`, `embed/*`, `tool/*`, and `agent/*` page composes. It defines
the `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Chat`/`IdGenerator` service Tags, the `Prompt`/`Response`
part algebras, `Tool`/`Toolkit` as `Schema`-typed data, GenAI OpenTelemetry conventions, and — natively —
the MCP server (`McpServer`/`McpSchema`). Provider packages never subclass these: each resolves a
provider-tagged `Model` (`Model.make(name, layer)`) into these Tags, so a provider is one row on the
capability-asymmetry table, never a fork. All generation is `Effect`/`Stream`; all failure flows through
the tagged `AiError` union; every tool-augmented call type-infers its added errors and context from the
toolkit (`ExtractError`/`ExtractContext`). The `ai/model.ts` guardrail gate wraps `generateText`/
`streamText`; `ai/agent.ts` composes `Chat.Persistence`; `ai/tool.ts` hosts on `McpServer`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai`
- package: `@effect/ai` (version ``, license MIT, `type: module`)
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

```ts signature
// One options shape drives all three modalities; Tools is inferred from the toolkit.
interface GenerateTextOptions<Tools extends Record<string, Tool.Any>> {
  readonly prompt: Prompt.RawInput
  readonly toolkit?: Toolkit.WithHandler<Tools> | Effect.Effect<Toolkit.WithHandler<Tools>, any, any>
  readonly toolChoice?: ToolChoice<{ [N in keyof Tools]: Tools[N]["name"] }[keyof Tools]>
  readonly concurrency?: Concurrency
  readonly disableToolCallResolution?: boolean // true => framework does NOT auto-run tool handlers
}
interface GenerateObjectOptions<Tools, A, I extends Record<string, unknown>, R> extends GenerateTextOptions<Tools> {
  readonly objectName?: string
  readonly schema: Schema.Schema<A, I, R>
}
type ToolChoice<Tools extends string> =
  | "auto" | "none" | "required"
  | { readonly tool: Tools }
  | { readonly mode?: "auto" | "required"; readonly oneOf: ReadonlyArray<Tools> }

// Free functions add the LanguageModel Tag to R; errors/context inferred from Options.
// LanguageModel.Service is the same three methods WITHOUT the Tag in R — what a provider make() yields (openai/google [03]).
declare const generateText: <Options, Tools extends Record<string, Tool.Any> = {}>(
  options: Options & GenerateTextOptions<Tools>
) => Effect.Effect<GenerateTextResponse<Tools>, ExtractError<Options>, LanguageModel | ExtractContext<Options>>
declare const generateObject: <A, I, R, Options, Tools = {}>(options: Options & GenerateObjectOptions<Tools, A, I, R>)
  => Effect.Effect<GenerateObjectResponse<Tools, A>, ExtractError<Options>, LanguageModel | R | ExtractContext<Options>>
declare const streamText: <Options, Tools = {}>(options: Options & GenerateTextOptions<Tools>)
  => Stream.Stream<Response.StreamPart<Tools>, ExtractError<Options>, LanguageModel | ExtractContext<Options>>
declare const make: (params: ConstructorParams) => Effect.Effect<Service>

class GenerateTextResponse<Tools> {
  readonly content: Array<Response.Part<Tools>>
  get text(): string; get reasoning(): Array<Response.ReasoningPart>; get reasoningText(): string | undefined
  get toolCalls(): Array<Response.ToolCallParts<Tools>>; get toolResults(): Array<Response.ToolResultParts<Tools>>
  get finishReason(): Response.FinishReason; get usage(): Response.Usage
}
class GenerateObjectResponse<Tools, A> extends GenerateTextResponse<Tools> { readonly value: A }

// The provider-facing normalized shape and the impl contract make() consumes.
interface ProviderOptions {
  readonly prompt: Prompt.Prompt
  readonly tools: ReadonlyArray<Tool.Any>
  readonly responseFormat: { readonly type: "text" } | { readonly type: "json"; readonly objectName: string; readonly schema: Schema.Schema.Any }
  readonly toolChoice: ToolChoice<any>
  readonly span: Span // provider spans thread into @effect/opentelemetry
}
interface ConstructorParams {
  readonly generateText: (o: ProviderOptions) => Effect.Effect<Array<Response.PartEncoded>, AiError.AiError, IdGenerator>
  readonly streamText: (o: ProviderOptions) => Stream.Stream<Response.StreamPartEncoded, AiError.AiError, IdGenerator>
}
```

## [03]-[MODEL]

[PUBLIC_TYPE_SCOPE]: the capability-asymmetry row abstraction — rail: ai-core

`Model.make(name, layer)` is the provider row: a value that is BOTH a `Layer` (provide it to bind the Tags)
AND an `Effect` (yield it to lift the provider's dependencies into a parent). It auto-provides `ProviderName`,
so the guardrail/tier-routing folds read the active provider by yielding one Tag. `ai/model.ts` folds every
provider (anthropic/openai/google/bedrock/openrouter) onto this one constructor — asymmetry lives in the row's
config, never in a parallel API.

```ts signature
const TypeId = "~@effect/ai/Model"
interface Model<in out Provider, in out Provides, in out Requires>
  extends Layer.Layer<Provides | ProviderName, never, Requires>,
          Effect.Effect<Layer.Layer<Provides | ProviderName>, never, Requires> {
  readonly provider: Provider
}
declare class ProviderName extends Context.Tag("@effect/ai/Model/ProviderName")<ProviderName, string> {}
declare const make: <const Provider extends string, Provides, Requires>(
  provider: Provider, layer: Layer.Layer<Provides, never, Requires>
) => Model<Provider, Provides, Requires>
```

[PROVIDER_ROWS]: the five sibling catalogs this table owner folds — each resolves `model(...)`/`layer(...)` into the core
`LanguageModel.LanguageModel` (plus `EmbeddingModel`/`Tokenizer` where the asymmetry column is populated), so provider
choice is a single composition-root layer swap. The row grammar (Client `make`/`layer`/`layerConfig` with `Redacted` key +
`HttpClient` requirement, per-request `Config` + `withConfigOverride`, the `<Provider>LanguageModel.model`/`.layer` entry — `.modelWithTokenizer` where the row carries a Tokenizer — returning `AiModel.Model<provider,…>`)
is uniform across all five; the asymmetry lives in the row config, catalogued per page.

| [INDEX] | [PROVIDER]      | [CATALOG]                     | [EXTRA_ASYMMETRY]                                                      |
| :-----: | :-------------- | :---------------------------- | :--------------------------------------------------------------------- |
|  [01]   | OpenAI          | `effect-ai-openai.md`         | embeddings ×2 + `OpenAiTokenizer` + `OpenAiTelemetry`                  |
|  [02]   | Anthropic       | `effect-ai-anthropic.md`      | `AnthropicTokenizer` Layer + `prepareTools` export                     |
|  [03]   | Google (Gemini) | `effect-ai-google.md`         | raw-client embeddings/token-count only                                 |
|  [04]   | Amazon Bedrock  | `effect-ai-amazon-bedrock.md` | SigV4 creds + native guardrail assessment + Anthropic-on-Bedrock tools |
|  [05]   | OpenRouter      | `effect-ai-openrouter.md`     | aggregator provider-routing + per-response cost metadata               |

## [04]-[EMBEDDING_MODEL]

[PUBLIC_TYPE_SCOPE]: vector embedding + built-in batch/cache — rail: ai-embed

`make` already owns request batching (`maxBatchSize`) and an Effect `Cache` (`capacity`/`timeToLive`);
`makeDataLoader` owns time-window coalescing (Scope-scoped). `ai/embed.ts` satisfies the
`store/retrieve` `Embedder` port with the `EmbeddingModel` Tag; `ai/embed.ts` feeds `embedMany`.

```ts signature
declare class EmbeddingModel extends Context.Tag("@effect/ai/EmbeddingModel")<EmbeddingModel, Service> {}
interface Service {
  readonly embed: (input: string) => Effect.Effect<Array<number>, AiError.AiError>
  readonly embedMany: (input: ReadonlyArray<string>, options?: { readonly concurrency?: Concurrency })
    => Effect.Effect<Array<Array<number>>, AiError.AiError>
}
interface Result { readonly index: number; readonly embeddings: Array<number> }
declare const make: (o: {
  readonly embedMany: (input: ReadonlyArray<string>) => Effect.Effect<Array<Result>, AiError.AiError>
  readonly maxBatchSize?: number
  readonly cache?: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }
}) => Effect.Effect<Service>
declare const makeDataLoader: (o: {
  readonly embedMany: (input: ReadonlyArray<string>) => Effect.Effect<Array<Result>, AiError.AiError>
  readonly window: Duration.DurationInput
  readonly maxBatchSize?: number
}) => Effect.Effect<Service, never, Scope>
```

## [05]-[TOOL_AND_TOOLKIT]

[PUBLIC_TYPE_SCOPE]: `Schema`-typed tools as data — rail: ai-tool

`Tool.make` defines a user tool from `parameters`/`success`/`failure` `Schema`s; `providerDefined` declares a
provider-executed tool (web search, code exec); `fromTaggedRequest` lifts an existing `Schema.TaggedRequest`.
`failureMode: "return"` keeps handler errors in the tool result (self-correcting model loop) instead of the
error channel. The MCP-aligned annotations (`Readonly`/`Destructive`/`Idempotent`/`OpenWorld`/`Title`) project
one-to-one onto MCP tool hints. `Toolkit.make(...tools)` is the collection; `.toLayer(handlers)`/`.toContext`
bind handlers; `merge` composes toolkits (later wins). Every provider tool roster (`OpenAiTool`/`AnthropicTool`/
`GoogleTool`/`AmazonBedrockTool`) is typed `Tool.ProviderDefined<"<Provider><Name>", { …; failureMode }, RequiresHandler>` —
`ProviderDefined`/`FailureMode`/`Any` are the core brands those catalogs return, never per-provider types.

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

```ts signature
declare const make: <const Name extends string, Params extends Schema.Struct.Fields | EmptyParams = ...,
  Success extends Schema.Schema.Any = typeof Schema.Void, Failure extends Schema.Schema.All = typeof Schema.Never,
  Mode extends FailureMode | undefined = undefined, Deps extends Array<Context.Tag<any, any>> = []>(
  name: Name, options?: {
    readonly description?: string; readonly parameters?: Params; readonly success?: Success
    readonly failure?: Failure; readonly failureMode?: Mode /* "error" | "return" */; readonly dependencies?: Deps
  }
) => Tool<Name, { parameters; success; failure; failureMode }, Context.Tag.Identifier<Deps[number]>>
declare const providerDefined: (o: {
  readonly id: `${string}.${string}`; readonly toolkitName: string; readonly providerName: string
  readonly args: Schema.Struct.Fields; readonly requiresHandler?: boolean
  readonly parameters?: Schema.Struct.Fields; readonly success?: Schema.Schema.Any; readonly failure?: Schema.Schema.All
}) => (args) => ProviderDefined</* … */>
// the provider-return brands — every sibling tool roster is typed as ProviderDefined; FailureMode threads the routing policy:
type FailureMode = "error" | "return"
interface Any extends Pipeable { readonly failureMode: FailureMode }   // erased bound; Record<string, Tool.Any> keys options/toolkits
interface ProviderDefined<Name extends string, Config, RequiresHandler extends boolean = false> extends Any { readonly failureMode: FailureMode }
declare const getJsonSchema: (tool: Tool<any, any>) => JsonSchema.JsonSchema7
declare const fromTaggedRequest: <S extends AnyTaggedRequestSchema>(schema: S) => FromTaggedRequest<S>

// Toolkit — Effect<WithHandler<Tools>, never, HandlersFor<Tools>>, Inspectable, Pipeable
interface Toolkit<Tools extends Record<string, Tool.Any>> {
  readonly tools: Tools
  of<H extends HandlersFrom<Tools>>(handlers: H): H
  toContext<H extends HandlersFrom<Tools>, EX, RX>(build: H | Effect.Effect<H, EX, RX>): Effect.Effect<Context.Context<Tool.HandlersFor<Tools>>, EX, RX>
  toLayer<H extends HandlersFrom<Tools>, EX, RX>(build: H | Effect.Effect<H, EX, RX>): Layer.Layer<Tool.HandlersFor<Tools>, EX, Exclude<RX, Scope>>
}
declare const empty: Toolkit<{}>
declare const make: <Tools extends ReadonlyArray<Tool.Any>>(...tools: Tools) => Toolkit<ToolsByName<Tools>>
declare const merge: <const Toolkits extends ReadonlyArray<Any>>(...toolkits: Toolkits) => Toolkit<MergedTools<Toolkits>>
```

## [06]-[PROMPT_AND_RESPONSE]

[PUBLIC_TYPE_SCOPE]: prompt construction + response part algebra — rail: ai-core

`Prompt` is the conversation value: `make`/`fromMessages`/`fromResponseParts` build it, `merge`/`setSystem`/
`prependSystem`/`appendSystem` are the context-assembly combinators `ai/model.ts` folds app-passed retrieval into.
`RawInput` is what `generateText({ prompt })` accepts (string | messages | `Prompt`). `Response` is the
discriminated part union `streamText` yields — the design's streaming fold discriminates on `part.type`.

```ts signature
// Prompt: message + part ADT with encoded twins and lowercase constructors.
type RawInput = string | Prompt | ReadonlyArray<Message> | /* … */
declare const make: (input: RawInput) => Prompt
declare const fromMessages: (messages: ReadonlyArray<Message>) => Prompt
declare const fromResponseParts: (parts: ReadonlyArray<Response.AnyPart>) => Prompt
declare const merge: (self: Prompt, other: Prompt) => Prompt
declare const setSystem / prependSystem / appendSystem: (self: Prompt, system: string) => Prompt
// Messages: SystemMessage | UserMessage | AssistantMessage | ToolMessage (+ systemMessage/userMessage/… ctors)
// Parts: TextPart | ReasoningPart | FilePart | ToolCallPart | ToolResultPart (+ *Encoded + *Options + lowercase ctors)

// Response: non-stream Part ADT + stream StreamPart ADT (discriminate on .type)
declare const FinishReason: /* "stop" | "length" | "content-filter" | "tool-calls" | "error" | "pause" | "other" | "unknown" */
class Usage { // all fields number | undefined
  readonly inputTokens; readonly outputTokens; readonly totalTokens; readonly reasoningTokens?; readonly cachedInputTokens?
}
// non-stream: TextPart, ReasoningPart, ToolCallPart, ToolResultPart (ToolResultSuccess|ToolResultFailure),
//   FilePart, DocumentSourcePart, UrlSourcePart, ResponseMetadataPart, FinishPart, ErrorPart
// stream deltas: Text{Start,Delta,End}Part, Reasoning{Start,Delta,End}Part, ToolParams{Start,Delta,End}Part
declare const makePart: <T extends string>(type: T, params: ConstructorParams) => Part
```

[AUGMENTATION_BASE]: the provider boundary-hook seam — every provider `declare module`-augments these interface twins, never the schema — rail: ai-core

Each `Prompt` message/part carries a `Prompt.ProviderOptions` slot and each `Response` part a `Response.ProviderMetadata` slot (both
`Schema.Record<string, Record<string, unknown> | undefined>` bases). A provider package attaches its one optional
provider-named key (`openai`/`anthropic`/`google`/`bedrock`/`openrouter`) by `declare module`-augmenting the SAME interfaces
below — the roster every sibling catalog's augmentation table targets. Internal code reads canonical shapes; the edge maps the slots.

| [INDEX] | [MODULE]              | [AUGMENTATION_BASE_INTERFACES]                                                         |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `@effect/ai/Prompt`   | `System`/`User`/`Assistant`/`Tool` `MessageOptions`                                    |
|  [02]   | `@effect/ai/Prompt`   | `Text`/`Reasoning`/`File`/`ToolCall`/`ToolResult` `PartOptions`                        |
|  [03]   | `@effect/ai/Response` | `Text{,Start,Delta,End}PartMetadata`                                                   |
|  [04]   | `@effect/ai/Response` | `Reasoning{,Start,Delta,End}PartMetadata`                                              |
|  [05]   | `@effect/ai/Response` | `ToolParams{Start,Delta,End}PartMetadata`                                              |
|  [06]   | `@effect/ai/Response` | `ToolCall`/`ToolResult` `PartMetadata`                                                 |
|  [07]   | `@effect/ai/Response` | `File`/`DocumentSource`/`UrlSource`/`ResponseMetadata`/`Finish`/`Error` `PartMetadata` |

```ts signature
// providers augment the interface twin with one optional provider key — the boundary hook, no schema fork:
declare module "@effect/ai/Response" { interface FinishPartMetadata { readonly openai?: { serviceTier?: … } } }
declare module "@effect/ai/Prompt"   { interface FilePartOptions   { readonly anthropic?: { cacheControl?: … } } }
```

## [07]-[CHAT]

[PUBLIC_TYPE_SCOPE]: stateful conversation + persistence — rail: ai-agent

`Chat` maintains history in a `Ref<Prompt>` and mirrors `generateText`/`streamText`/`generateObject` while
appending both turns. `export`/`exportJson` serialize; `fromJson`/`fromExport`/`fromPrompt` restore.
`Persistence` + `layerPersisted` back durable sessions — the exact substrate `ai/agent.ts` and
`ai/agent.ts` compose for durable agents over `work` entities.

```ts signature
declare class Chat extends Context.Tag("@effect/ai/Chat")<Chat, Service> {}
interface Service {
  readonly history: Ref.Ref<Prompt.Prompt>
  readonly export: Effect.Effect<unknown, AiError.AiError>
  readonly exportJson: Effect.Effect<string, AiError.MalformedOutput>
  readonly generateText: (...) => Effect.Effect<...>; readonly streamText: (...) => Stream.Stream<...>
  readonly generateObject: (...) => Effect.Effect<...>
}
declare const empty: Effect.Effect<Service> // LanguageModel is required by the Service methods, not construction
declare const fromPrompt: (prompt: Prompt.RawInput) => Effect.Effect<Service, never, never>
declare const fromExport: (data: unknown) => Effect.Effect<Service, ParseError, LanguageModel>
declare const fromJson: (data: string) => Effect.Effect<Service, ParseError, LanguageModel>
declare class ChatNotFoundError extends /* Schema.TaggedError */ {}
declare class Persistence extends Context.Tag("@effect/ai/Chat/Persisted")<Persistence, Persistence.Service> {}
interface Persisted { /* stored history record */ }
// durable sessions ride @effect/experimental Persistence.BackingPersistence
declare const makePersisted: (o: { readonly id: string; /* … */ }) => Effect.Effect<Persistence.Service, never, Scope | BackingPersistence>
declare const layerPersisted: (o: { /* … */ }) => Layer.Layer<Persistence, never, BackingPersistence>
```

## [08]-[TOKENIZER]

[PUBLIC_TYPE_SCOPE]: token counting + budget truncation — rail: ai-token

`ai/model.ts` budgets own this Tag; the two provider tokenizers `AnthropicTokenizer` (bare `Service` value) and
`OpenAiTokenizer.make({model})` are the Layers that provide it (`effect-ai-anthropic.md` [04] / `effect-ai-openai.md` [05]);
`modelWithTokenizer`/`layerWithTokenizer` fold either into the provides set. `truncate` is the budget enforcement — trim a
`Prompt` to a token ceiling before generation.

```ts signature
declare class Tokenizer extends Context.Tag("@effect/ai/Tokenizer")<Tokenizer, Service> {}
interface Service {
  readonly tokenize: (input: Prompt.RawInput) => Effect.Effect<Array<number>, AiError.AiError>
  readonly truncate: (input: Prompt.RawInput, tokens: number) => Effect.Effect<Prompt.Prompt, AiError.AiError>
}
declare const make: (o: { readonly tokenize: (content: Prompt.Prompt) => Effect.Effect<Array<number>, AiError.AiError> }) => Service
```

## [09]-[MCP_HOSTING]

[PUBLIC_TYPE_SCOPE]: native MCP server + protocol schema — rail: ai-tool

`ai/tool.ts` hosts app toolkits as MCP tools with NO extra dependency: `McpServer.toolkit(toolkit)` registers a
`Toolkit` as MCP tools, and one transport Layer (`layerStdio`/`layerHttp`/`layerHttpRouter`) serves it.
`resource`/`prompt`/`elicit` round out server capability; `McpSchema` is the full MCP wire (Schema.Class-based)
with typed error subclasses. This is the sole MCP host — `@modelcontextprotocol/sdk` is client-only.

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

```ts signature
declare const toolkit: <Tools extends Record<string, AiTool.Any>>(toolkit: Toolkit.Toolkit<Tools>)
  => Layer.Layer<never, never, AiTool.HandlersFor<Tools> | Exclude<AiTool.Requirements<Tools>, McpServerClient>>
declare const layerStdio: (o: { readonly name: string; readonly version: string; readonly stdin: Stream<Uint8Array,...>; readonly stdout: Sink<...> }) => Layer.Layer<McpServer | McpServerClient, never, ...>
declare const layerHttp: (o: { readonly name: string; readonly version: string; readonly path: HttpRouter.PathInput; readonly routerTag?: ... }) => Layer.Layer<McpServer | McpServerClient>
declare const resource: /* { uri | tagged-template `file://…/${param}` }, name, mimeType?, completion?, content } */
declare const prompt: <E, R, Params>(o: { readonly name: string; readonly parameters?: Schema.Schema<Params, ...>; readonly completion?: ...; readonly content: (params: Params) => Effect.Effect<...> }) => Layer.Layer<...>
declare const elicit: <A, I, R>(o: { readonly message: string; readonly schema: Schema.Schema<A, I, R> }) => Effect.Effect<A, ElicitationDeclined, McpServerClient | R>
// McpSchema: Resource, ResourceTemplate, Prompt, Tool, ToolAnnotations, CallTool/CallToolResult, ContentBlock
//   (TextContent|ImageContent|AudioContent|EmbeddedResource|ResourceLink), CreateMessage (sampling), Complete,
//   Elicit, Root, Client/ServerCapabilities, Implementation; McpError + InvalidParams|InternalError|MethodNotFound|…
declare const param: <Id extends string, S>(id: Id, schema: S) => Param<Id, S>
```

## [10]-[TELEMETRY_IDS_ERRORS]

[PUBLIC_TYPE_SCOPE]: GenAI telemetry, id generation, error rail — rail: ai-core

`Telemetry.addGenAIAnnotations(span, opts)` writes OpenTelemetry GenAI semantic-convention attributes
(system/operation/request.model/usage.*) onto the current span — the bridge every provider call rides into
`@effect/opentelemetry`. `system` draws from `WellKnownSystem` (`anthropic`/`aws.bedrock`/`gemini`/`openai`/…) and
`operation.name` from `WellKnownOperationName`; the provider `*Telemetry` modules extend `GenAITelemetryAttributes` via
`AttributesWithPrefix<Req, "gen_ai.<provider>.request">` over `AllAttributes` — the seam `OpenAiTelemetry` composes onto
(`effect-ai-openai.md` [07]). `IdGenerator` is the pluggable tool-call id source. `AiError` is the one tagged
failure union — `Match.tag` dispatch, never ad-hoc throws.

```ts signature
declare const addGenAIAnnotations: (span: Span, options: GenAITelemetryAttributeOptions) => void
declare const addSpanAttributes: (span: Span, attributes: GenAITelemetryAttributes) => void
declare class CurrentSpanTransformer extends /* span-transformer Context reference */ {}
// options: { system?; operation?: { name }; request?: { model; temperature; maxTokens; … }; usage?: { inputTokens; outputTokens } }
type WellKnownSystem = "anthropic" | "aws.bedrock" | "az.ai.inference" | "az.ai.openai" | "cohere" | "deepseek"
  | "gemini" | "groq" | "ibm.watsonx.ai" | "mistral_ai" | "openai" | "perplexity" | "vertex_ai" | "xai"
type WellKnownOperationName = "chat" | "embeddings" | "text_completion"
// the provider *Telemetry extension seam — OpenAiTelemetry composes namespaced rows onto these core mapped types:
type AttributesWithPrefix<A extends Record<string, any>, Prefix extends string> = { [K in keyof A as `${Prefix}.${K & string}`]: A[K] }
type AllAttributes = BaseAttributes & OperationAttributes & TokenAttributes & UsageAttributes & RequestAttributes & ResponseAttributes

declare class IdGenerator extends Context.Tag("@effect/ai/IdGenerator")<IdGenerator, Service> {}
interface MakeOptions { readonly alphabet: string; readonly prefix?: string; readonly separator: string; readonly size: number }
declare const defaultIdGenerator: Service
declare const make: (options?: MakeOptions) => Service
declare const layer: (options?: MakeOptions) => Layer.Layer<IdGenerator>

// AiError — tagged union; dispatch with Match.tag
type AiError = HttpRequestError | HttpResponseError | MalformedInput | MalformedOutput | UnknownError
declare const isAiError: (u: unknown) => u is AiError
```

## [11]-[INTEGRATION]

[STACK]: provider rows + the ONE guardrail gate — rail: ai-core
- `ai/model.ts` is a table of `Model.make(name, providerLayer)` rows; the app root provides one. The guardrail
 gate wraps `generateText`/`streamText` in a fold: input moderation runs before the call, output moderation +
 `Schema`-refusal admission run over the `Response.Part` stream, and a rejected call short-circuits into `AiError`
 — one admission surface over every row, not a per-provider fork. Tier-routing reads `Model.ProviderName` +
 provider finish-part cost metadata to pick the row; a new provider is a new row.
- `disableToolCallResolution: true` hands tool execution to the guardrail so no handler runs before admission;
 `failureMode: "return"` keeps a failed tool call in-band for a self-correcting model loop.

[STACK]: universal-tier rails — rail: ai-core
- `effect`: `Schema` types every tool/prompt/response and the `generateObject` output; `Stream` carries
 `streamText` deltas folded by `part.type`; `Layer`/`Context.Tag` bind every service; `Match.tag` dispatches
 `AiError`; `Cache`/`Duration` back `EmbeddingModel.make`.
- `@effect/platform`: provider Layers require `HttpClient` — `net/client` supplies the default-policy client
 (timeout/retry/proxy); `McpServer.layerHttp` composes `HttpRouter`/`HttpLayerRouter`.
- `@effect/opentelemetry`: `Telemetry.addGenAIAnnotations` writes GenAI semconv onto the `ProviderOptions.span`;
 the `telemetry` folder's OTLP exporter ships them — one span per generation, tool call, and agent run.
- `@effect/experimental` + `@effect/rpc`: `Chat.layerPersisted` durable sessions and the `McpServer` RPC
 transport ride these peers; `agent/*` composes `Chat.Persistence` over `work` cluster entities.
- `@effect/vitest`: kit-driven law specs exercise providers behind a stub `LanguageModel.make` and drive MCP hosting
 through an in-memory transport.

[STACK]: `embed` -> `store` `Embedder` port — rail: ai-embed
- `ai/embed.ts` publishes the `EmbeddingModel` Tag (batched + cached via `make`, or window-coalesced via
 `makeDataLoader`) as the `Layer` the app root wires into the `store/retrieve` `Embedder` port; `ai` imports no
 `store` code. `ai/model.ts` folds app-passed `store/retrieve` results into a `Prompt` via `merge`/`appendSystem` —
 retrieval arrives as values, never a `store` import edge.
