# [API_CATALOGUE] @effect/ai

Provider-agnostic LLM core for the Effect runtime: the `@effect/ai` package — language models,
embeddings, stateful chat, tools/toolkits, the prompt and response data algebra, tokenization, GenAI
telemetry, and the MCP server. This page owns the core package only. The concrete provider adapter
packages that resolve a provider-tagged `Model` into these core service tags are catalogued on their
own pages (`effect-ai-openai.md`, `effect-ai-anthropic.md`, `effect-ai-google.md`, and the
OpenRouter/Amazon-Bedrock adapter pages); see PROVIDER_RESOLUTION below for the seam they bind to. The
core defines the `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Chat`/`IdGenerator` service tags and the
prompt/response/tool data model; all success/failure flows through the `AiError` union; all I/O is
`Effect`/`Stream`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai`
- package: `@effect/ai`
- entry: `@effect/ai` (re-exports modules `AiError`, `Chat`, `EmbeddingModel`, `IdGenerator`, `LanguageModel`, `McpSchema`, `McpServer`, `Model`, `Prompt`, `Response`, `Telemetry`, `Tokenizer`, `Tool`, `Toolkit`)
- asset: provider-agnostic LLM service tags, prompt/response/tool data algebra, stateful chat, embeddings, tokenizer, GenAI OpenTelemetry conventions, MCP server
- rail: ai-core

## [02]-[PUBLIC_TYPES]

### @effect/ai — service tags and model entry points

[PUBLIC_TYPE_SCOPE]: language model service — rail: ai-core

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]       | [CAPABILITY]                  |
| :-----: | :-------------------------------------- | :------------------ | :---------------------------- |
|  [01]   | `LanguageModel.LanguageModel`           | `Context.Tag` class | service tag                   |
|  [02]   | `LanguageModel.Service`                 | interface           | generation contract           |
|  [03]   | `LanguageModel.generateText`            | function            | text generation               |
|  [04]   | `LanguageModel.generateObject`          | function            | schema-shaped generation      |
|  [05]   | `LanguageModel.streamText`              | function            | streaming generation          |
|  [06]   | `LanguageModel.make`                    | constructor         | provider service construction |
|  [07]   | `LanguageModel.GenerateTextOptions`     | options interface   | text request options          |
|  [08]   | `LanguageModel.GenerateObjectOptions`   | options interface   | object request options        |
|  [09]   | `LanguageModel.ToolChoice<Tools>`       | union               | tool-selection policy         |
|  [10]   | `LanguageModel.GenerateTextResponse`    | class               | generated-text receipt        |
|  [11]   | `LanguageModel.GenerateObjectResponse`  | class               | generated-object receipt      |
|  [12]   | `LanguageModel.ProviderOptions`         | interface           | provider-normalized request   |
|  [13]   | `LanguageModel.ConstructorParams`       | interface           | provider implementation seam  |
|  [14]   | `LanguageModel.ExtractError<Options>`   | conditional type    | tool error inference          |
|  [15]   | `LanguageModel.ExtractContext<Options>` | conditional type    | tool context inference        |

```ts contract
// LanguageModel.Service — the provider-agnostic contract
interface Service {
  readonly generateText: <Options extends NoExcessProperties<GenerateTextOptions<any>, Options>, Tools extends Record<string, Tool.Any> = {}>(
    options: Options & GenerateTextOptions<Tools>
  ) => Effect.Effect<GenerateTextResponse<Tools>, ExtractError<Options>, ExtractContext<Options>>
  readonly generateObject: <A, I extends Record<string, unknown>, R, Options extends NoExcessProperties<GenerateObjectOptions<any, A, I, R>, Options>, Tools extends Record<string, Tool.Any> = {}>(
    options: Options & GenerateObjectOptions<Tools, A, I, R>
  ) => Effect.Effect<GenerateObjectResponse<Tools, A>, ExtractError<Options>, R | ExtractContext<Options>>
  readonly streamText: <Options extends NoExcessProperties<GenerateTextOptions<any>, Options>, Tools extends Record<string, Tool.Any> = {}>(
    options: Options & GenerateTextOptions<Tools>
  ) => Stream.Stream<Response.StreamPart<Tools>, ExtractError<Options>, ExtractContext<Options>>
}

interface GenerateTextOptions<Tools extends Record<string, Tool.Any>> {
  readonly prompt: Prompt.RawInput
  readonly toolkit?: Toolkit.WithHandler<Tools> | Effect.Effect<Toolkit.WithHandler<Tools>, any, any> | undefined
  readonly toolChoice?: ToolChoice<{ [Name in keyof Tools]: Tools[Name]["name"] }[keyof Tools]> | undefined
  readonly concurrency?: Concurrency | undefined
  readonly disableToolCallResolution?: boolean | undefined
}
interface GenerateObjectOptions<Tools extends Record<string, Tool.Any>, A, I extends Record<string, unknown>, R>
  extends GenerateTextOptions<Tools> {
  readonly objectName?: string | undefined
  readonly schema: Schema.Schema<A, I, R>
}

type ToolChoice<Tools extends string> =
  | "auto" | "none" | "required"
  | { readonly tool: Tools }
  | { readonly mode?: "auto" | "required"; readonly oneOf: ReadonlyArray<Tools> }

// free functions add LanguageModel to the R channel
declare const generateText: <Options, Tools extends Record<string, Tool.Any> = {}>(
  options: Options & GenerateTextOptions<Tools>
) => Effect.Effect<GenerateTextResponse<Tools>, ExtractError<Options>, LanguageModel | ExtractContext<Options>>
declare const generateObject: <A, I, R, Options, Tools extends Record<string, Tool.Any> = {}>(
  options: Options & GenerateObjectOptions<Tools, A, I, R>
) => Effect.Effect<GenerateObjectResponse<Tools, A>, ExtractError<Options>, LanguageModel | R | ExtractContext<Options>>
declare const streamText: <Options, Tools extends Record<string, Tool.Any> = {}>(
  options: Options & GenerateTextOptions<Tools>
) => Stream.Stream<Response.StreamPart<Tools>, ExtractError<Options>, LanguageModel | ExtractContext<Options>>
declare const make: (params: ConstructorParams) => Effect.Effect<Service>

// provider-facing normalized shape + the impl contract make() consumes
interface ProviderOptions {
  readonly prompt: Prompt.Prompt
  readonly tools: ReadonlyArray<Tool.Any>
  readonly responseFormat:
    | { readonly type: "text" }
    | { readonly type: "json"; readonly objectName: string; readonly schema: Schema.Schema.Any }
  readonly toolChoice: ToolChoice<any>
  readonly span: Span
}
interface ConstructorParams {
  readonly generateText: (options: ProviderOptions) => Effect.Effect<Array<Response.PartEncoded>, AiError.AiError, IdGenerator>
  readonly streamText: (options: ProviderOptions) => Stream.Stream<Response.StreamPartEncoded, AiError.AiError, IdGenerator>
}

// response accessors (GenerateTextResponse<Tools>)
class GenerateTextResponse<Tools extends Record<string, Tool.Any>> {
  readonly content: Array<Response.Part<Tools>>
  get text(): string
  get reasoning(): Array<Response.ReasoningPart>
  get reasoningText(): string | undefined
  get toolCalls(): Array<Response.ToolCallParts<Tools>>
  get toolResults(): Array<Response.ToolResultParts<Tools>>
  get finishReason(): Response.FinishReason
  get usage(): Response.Usage
}
class GenerateObjectResponse<Tools extends Record<string, Tool.Any>, A> extends GenerateTextResponse<Tools> {
  readonly value: A
}
```

[PUBLIC_TYPE_SCOPE]: model adapter and provider name
- rail: ai-core

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]       | [RAIL]                                                       |
| :-----: | :------------------------ | :------------------ | :----------------------------------------------------------- |
|  [01]   | `Model.Model<P,Prov,Req>` | branded interface   | both a `Layer` and an `Effect<Layer>`; carries `provider`    |
|  [02]   | `Model.make`              | constructor         | `(provider, layer) => Model` — tags a layer with provider id |
|  [03]   | `Model.ProviderName`      | `Context.Tag` class | tag `"@effect/ai/Model/ProviderName"` → `string`             |
|  [04]   | `Model.TypeId`            | unique const        | `"~@effect/ai/Model"`                                        |

```ts contract
interface Model<in out Provider, in out Provides, in out Requires>
  extends Layer.Layer<Provides | ProviderName, never, Requires>,
          Effect.Effect<Layer.Layer<Provides | ProviderName>, never, Requires> {
  readonly [TypeId]: TypeId
  readonly provider: Provider
}
declare const make: <const Provider extends string, Provides, Requires>(
  provider: Provider,
  layer: Layer.Layer<Provides, never, Requires>
) => Model<Provider, Provides, Requires>
```

[PUBLIC_TYPE_SCOPE]: embedding model
- rail: ai-core

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                                                     |
| :-----: | :------------------------------ | :------------------ | :--------------------------------------------------------- |
|  [01]   | `EmbeddingModel.EmbeddingModel` | `Context.Tag` class | tag `"@effect/ai/EmbeddingModel"` → `Service`              |
|  [02]   | `EmbeddingModel.Service`        | interface           | `embed(input)` / `embedMany(input, {concurrency?})`        |
|  [03]   | `EmbeddingModel.Result`         | interface           | `{ index, embeddings }`                                    |
|  [04]   | `EmbeddingModel.make`           | constructor         | per-call batching + optional cache `{capacity,timeToLive}` |
|  [05]   | `EmbeddingModel.makeDataLoader` | constructor         | windowed data-loader batching `{window, maxBatchSize?}`    |

```ts contract
interface Service {
  readonly embed: (input: string) => Effect.Effect<Array<number>, AiError.AiError>
  readonly embedMany: (input: ReadonlyArray<string>, options?: {
    readonly concurrency?: Types.Concurrency | undefined
  }) => Effect.Effect<Array<Array<number>>, AiError.AiError>
}
interface Result { readonly index: number; readonly embeddings: Array<number> }
declare const make: (options: {
  readonly embedMany: (input: ReadonlyArray<string>) => Effect.Effect<Array<Result>, AiError.AiError>
  readonly maxBatchSize?: number
  readonly cache?: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }
}) => Effect.Effect<Service>
declare const makeDataLoader: (options: {
  readonly embedMany: (input: ReadonlyArray<string>) => Effect.Effect<Array<Result>, AiError.AiError>
  readonly window: Duration.DurationInput
  readonly maxBatchSize?: number
}) => Effect.Effect<Service, never, Scope.Scope>
```

[PUBLIC_TYPE_SCOPE]: stateful chat
- rail: ai-core

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [RAIL]                                                                               |
| :-----: | :----------------------- | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `Chat.Chat`              | `Context.Tag` class | tag `"@effect/ai/Chat"` → `Service`                                                  |
|  [02]   | `Chat.Service`           | interface           | `history: Ref`, `export`, `exportJson`, `generateText`/`streamText`/`generateObject` |
|  [03]   | `Chat.empty`             | constructor effect  | new empty chat session                                                               |
|  [04]   | `Chat.fromPrompt`        | constructor         | seed chat from `Prompt.RawInput`                                                     |
|  [05]   | `Chat.fromExport`        | constructor         | rehydrate from `export` payload (`unknown`)                                          |
|  [06]   | `Chat.fromJson`          | constructor         | rehydrate from JSON string                                                           |
|  [07]   | `Chat.Persistence`       | `Context.Tag` class | tag `"@effect/ai/Chat/Persisted"` → `Persistence.Service`                            |
|  [08]   | `Chat.Persisted`         | interface           | extends `Service` + `id`, `save`                                                     |
|  [09]   | `Chat.makePersisted`     | constructor         | persisted chat by `storeId`                                                          |
|  [10]   | `Chat.layerPersisted`    | layer               | persistence backplane layer by `storeId`                                             |
|  [11]   | `Chat.ChatNotFoundError` | tagged error        | persistence lookup miss                                                              |

```ts contract
interface Service {
  readonly history: Ref.Ref<Prompt.Prompt>
  readonly export: Effect.Effect<unknown, AiError.AiError>
  readonly exportJson: Effect.Effect<string, AiError.MalformedOutput>
  readonly generateText: Service["generateText"] // same signature shape as LanguageModel.Service.generateText, R adds LanguageModel.LanguageModel
  readonly streamText: Service["streamText"]
  readonly generateObject: Service["generateObject"]
}
declare const empty: Effect.Effect<Service>
declare const fromPrompt: (prompt: Prompt.RawInput) => Effect.Effect<Service>
declare const fromExport: (data: unknown) => Effect.Effect<Service, ParseError, LanguageModel.LanguageModel>
declare const fromJson: (data: string) => Effect.Effect<Service, ParseError, LanguageModel.LanguageModel>
```

[PUBLIC_TYPE_SCOPE]: tokenizer
- rail: ai-core

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]       | [CAPABILITY]         |
| :-----: | :-------------------- | :------------------ | :------------------- |
|  [01]   | `Tokenizer.Tokenizer` | `Context.Tag` class | tokenizer service    |
|  [02]   | `Tokenizer.Service`   | interface           | tokenize or truncate |
|  [03]   | `Tokenizer.make`      | constructor         | service construction |

[PUBLIC_TYPE_SCOPE]: id generator
- rail: ai-core

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]       | [CAPABILITY]         |
| :-----: | :------------------------------- | :------------------ | :------------------- |
|  [01]   | `IdGenerator.IdGenerator`        | `Context.Tag` class | id service           |
|  [02]   | `IdGenerator.Service`            | interface           | id generation        |
|  [03]   | `IdGenerator.MakeOptions`        | options interface   | generator policy     |
|  [04]   | `IdGenerator.defaultIdGenerator` | value               | default service      |
|  [05]   | `IdGenerator.make` / `.layer`    | constructor/layer   | service construction |

[PUBLIC_TYPE_SCOPE]: telemetry
- rail: ai-core

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [CAPABILITY]               |
| :-----: | :----------------------------------------- | :------------------ | :------------------------- |
|  [01]   | `Telemetry.addGenAIAnnotations`            | function            | GenAI span annotation      |
|  [02]   | `Telemetry.addSpanAttributes`              | function            | prefixed span attributes   |
|  [03]   | `Telemetry.GenAITelemetryAttributeOptions` | options             | GenAI attribute payload    |
|  [04]   | `Telemetry.SpanTransformer`                | interface           | span mutation callback     |
|  [05]   | `Telemetry.CurrentSpanTransformer`         | `Context.Tag` class | span transformer tag       |
|  [06]   | `Telemetry.WellKnownSystem`                | union               | provider system vocabulary |
|  [07]   | `Telemetry.WellKnownOperationName`         | union               | operation vocabulary       |

```ts contract
// Tokenizer.Service
interface Service {
  readonly tokenize: (input: Prompt.RawInput) => Effect.Effect<Array<number>, AiError.AiError>
  readonly truncate: (input: Prompt.RawInput, tokens: number) => Effect.Effect<Prompt.Prompt, AiError.AiError>
}
// IdGenerator
interface MakeOptions {
  readonly alphabet: string
  readonly prefix?: string | undefined
  readonly separator: string
  readonly size: number
}
declare const defaultIdGenerator: Service
declare const layer: (options: MakeOptions) => Layer.Layer<IdGenerator, Cause.IllegalArgumentException>
// Telemetry — dual data-first / data-last
declare const addGenAIAnnotations: {
  (span: Span, options: GenAITelemetryAttributeOptions): void
  (options: GenAITelemetryAttributeOptions): (span: Span) => void
}
type WellKnownSystem = "anthropic" | "aws.bedrock" | "az.ai.inference" | "az.ai.openai" | "cohere"
  | "deepseek" | "gemini" | "groq" | "ibm.watsonx.ai" | "mistral_ai" | "openai" | "perplexity"
  | "vertex_ai" | "xai"
type WellKnownOperationName = "chat" | "embeddings" | "text_completion"
```

### @effect/ai — prompt, response, tool data algebra

[PUBLIC_TYPE_SCOPE]: prompt model
- rail: ai-core

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]     | [CAPABILITY]             |
| :-----: | :---------------------------------------------------- | :---------------- | :----------------------- |
|  [01]   | `Prompt.Prompt`                                       | branded interface | conversation value       |
|  [02]   | `Prompt.RawInput`                                     | union             | accepted prompt input    |
|  [03]   | `Prompt.make`                                         | constructor       | input normalization      |
|  [04]   | `Prompt.empty` / `.fromMessages`                      | constructor       | prompt construction      |
|  [05]   | `Prompt.fromResponseParts`                            | constructor       | response-to-prompt lift  |
|  [06]   | `Prompt.merge`                                        | combinator        | prompt concatenation     |
|  [07]   | `Prompt.setSystem` / `prependSystem` / `appendSystem` | combinator        | system-message placement |
|  [08]   | `Prompt.Message`                                      | union             | message algebra          |
|  [09]   | `Prompt.makeMessage` / `*Message`                     | constructor       | message construction     |
|  [10]   | `Prompt.Part`                                         | union             | message-part algebra     |
|  [11]   | `Prompt.makePart` / `*Part`                           | constructor       | part construction        |
|  [12]   | `Prompt.FromJson`                                     | Schema transform  | JSON conversion          |
|  [13]   | `Prompt.ProviderOptions`                              | Schema record     | provider option metadata |

Concrete message factories are `systemMessage`, `userMessage`, `assistantMessage`, and `toolMessage`; concrete part factories are `textPart`, `reasoningPart`, `filePart`, `toolCallPart`, and `toolResultPart`.

```ts contract
type RawInput = string | Iterable<MessageEncoded> | Prompt
type Message = SystemMessage | UserMessage | AssistantMessage | ToolMessage
type Part = TextPart | ReasoningPart | FilePart | ToolCallPart | ToolResultPart
declare const make: (input: RawInput) => Prompt
declare const merge: {
  (self: Prompt, other: Prompt): Prompt
  (other: Prompt): (self: Prompt) => Prompt
}
declare const makeMessage: <const Role extends Message["role"]>(role: Role, params: ...) => Extract<Message, { role: Role }>
declare const makePart: <const Type extends Part["type"]>(type: Type, params: ...) => Extract<Part, { type: Type }>
// each Part/Message has paired Encoded interface + Schema constant of the same name
```

[PUBLIC_TYPE_SCOPE]: response model
- rail: ai-core

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]  | [CAPABILITY]             |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------- |
|  [01]   | `Response.AnyPart`                                      | union          | decoded part algebra     |
|  [02]   | `Response.Part<Tools>`                                  | union          | settled response part    |
|  [03]   | `Response.StreamPart<Tools>`                            | union          | streaming response part  |
|  [04]   | `Response.makePart`                                     | constructor    | part construction        |
|  [05]   | `Response.FinishReason`                                 | Schema literal | finish vocabulary        |
|  [06]   | `Response.Usage`                                        | Schema class   | token usage receipt      |
|  [07]   | `Response.ToolCallParts<Tools>`                         | mapped union   | toolkit call narrowing   |
|  [08]   | `Response.ToolResultParts<Tools>`                       | mapped union   | toolkit result narrowing |
|  [09]   | `Response.ProviderMetadata`                             | Schema record  | provider metadata        |
|  [10]   | `Response.TextPart` / `ToolCallPart` / `FinishPart` / … | Schema + ctor  | concrete part family     |
|  [11]   | `Response.AllParts(toolkit)`                            | Schema factory | toolkit-aware schema     |

```ts contract
type FinishReason = "stop" | "length" | "content-filter" | "tool-calls" | "error" | "pause" | "other" | "unknown"
class Usage {
  readonly inputTokens: number | undefined
  readonly outputTokens: number | undefined
  readonly totalTokens: number | undefined
  readonly reasoningTokens?: number | undefined
  readonly cachedInputTokens?: number | undefined
}
type StreamPart<Tools extends Record<string, Tool.Any>> =
  | TextStartPart | TextDeltaPart | TextEndPart
  | ReasoningStartPart | ReasoningDeltaPart | ReasoningEndPart
  | ToolParamsStartPart | ToolParamsDeltaPart | ToolParamsEndPart
  | ToolCallParts<Tools> | ToolResultParts<Tools>
  | FilePart | DocumentSourcePart | UrlSourcePart | ResponseMetadataPart | FinishPart | ErrorPart
declare const makePart: <const Type extends AnyPart["type"]>(type: Type, params: ...) => Extract<AnyPart, { type: Type }>
```

[PUBLIC_TYPE_SCOPE]: tool model
- rail: ai-core

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------- |
|  [01]   | `Tool.Tool<Name,Config,Req>`                                   | branded interface | user-defined tool      |
|  [02]   | `Tool.Any`                                                     | interface         | erased tool bound      |
|  [03]   | `Tool.ProviderDefined<Name,Cfg>`                               | branded interface | provider-executed tool |
|  [04]   | `Tool.make`                                                    | constructor       | user tool construction |
|  [05]   | `Tool.providerDefined`                                         | constructor       | provider tool modeling |
|  [06]   | `Tool.fromTaggedRequest`                                       | constructor       | tagged-request lift    |
|  [07]   | `Tool.FailureMode`                                             | union             | failure routing policy |
|  [08]   | `Tool.Handler` / `HandlerResult` / `HandlerError`              | interface/type    | handler contract       |
|  [09]   | `Tool.getJsonSchema` / `getDescription`                        | function          | schema and description |
|  [10]   | `Tool.isUserDefined` / `isProviderDefined`                     | guard             | tool discrimination    |
|  [11]   | `Tool.Title`/`Readonly`/`Destructive`/`Idempotent`/`OpenWorld` | annotation refs   | MCP-style annotations  |

[PUBLIC_TYPE_SCOPE]: toolkit model
- rail: ai-core

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :--------------------------- | :---------------- | :-------------------- |
|  [01]   | `Toolkit.Toolkit<Tools>`     | branded interface | tool collection       |
|  [02]   | `Toolkit.make`               | constructor       | toolkit construction  |
|  [03]   | `Toolkit.merge`              | combinator        | toolkit composition   |
|  [04]   | `Toolkit.WithHandler<Tools>` | interface         | handler-bound toolkit |
|  [05]   | `Toolkit.empty`              | value             | empty toolkit         |

```ts contract
declare const make: <const Name extends string, Parameters, Success, Failure, Mode extends FailureMode | undefined = undefined, Dependencies extends Array<Context.Tag<any, any>> = []>(
  name: Name,
  options?: {
    readonly description?: string | undefined
    readonly parameters?: Parameters | undefined
    readonly success?: Success | undefined
    readonly failure?: Failure | undefined
    readonly failureMode?: Mode               // defaults to "error"
    readonly dependencies?: Dependencies | undefined
  }
) => Tool<Name, { parameters; success; failure; failureMode }, ...>
declare const providerDefined: <const Name extends string, Args, Parameters, Success, Failure, RequiresHandler extends boolean = false>(options: {
  readonly id: `${string}.${string}`
  readonly toolkitName: Name
  readonly providerName: string
  readonly args: Args
  readonly requiresHandler?: RequiresHandler | undefined
  readonly parameters?: Parameters | undefined
  readonly success?: Success | undefined
  readonly failure?: Failure | undefined
}) => (...) => ProviderDefined<Name, ...>
type FailureMode = "error" | "return"
// Toolkit
interface Toolkit<in out Tools extends Record<string, Tool.Any>>
  extends Effect.Effect<WithHandler<Tools>, never, Tool.HandlersFor<Tools>>, Inspectable, Pipeable {
  readonly tools: Tools
}
declare const make: <Tools extends ReadonlyArray<Tool.Any>>(...tools: Tools) => Toolkit<ToolsByName<Tools>>
declare const merge: <const Toolkits extends ReadonlyArray<Any>>(...toolkits: Toolkits) => Toolkit<MergedTools<Toolkits>>
interface WithHandler<in out Tools extends Record<string, Tool.Any>> {
  readonly tools: Tools
  readonly handle: <Name extends keyof Tools>(name: Name, params: any) => Effect.Effect<...>
}
// MyToolkit.toLayer({ ToolName: (params) => Effect... }) wires handlers into a Layer
```

[PUBLIC_TYPE_SCOPE]: error rail
- rail: ai-core

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [RAIL]                                                                                       |
| :-----: | :-------------------------- | :----------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `AiError.AiError`           | union + Schema     | `HttpRequestError \| HttpResponseError \| MalformedInput \| MalformedOutput \| UnknownError` |
|  [02]   | `AiError.HttpRequestError`  | tagged error class | transport-layer request failure                                                              |
|  [03]   | `AiError.HttpResponseError` | tagged error class | response failure (carries response detail)                                                   |
|  [04]   | `AiError.MalformedInput`    | tagged error class | input does not match expected shape                                                          |
|  [05]   | `AiError.MalformedOutput`   | tagged error class | output cannot be parsed/validated                                                            |
|  [06]   | `AiError.UnknownError`      | tagged error class | catch-all runtime failure                                                                    |
|  [07]   | `AiError.isAiError`         | guard              | `(u: unknown) => u is AiError`                                                               |
|  [08]   | `AiError.TypeId`            | unique const       | `"~@effect/ai/AiError"`                                                                      |

```ts contract
type AiError = HttpRequestError | HttpResponseError | MalformedInput | MalformedOutput | UnknownError
// each is a Schema.TaggedError class with a string _tag matching the symbol name;
// match with Match.tag / Effect.catchTag on the _tag literal
declare const isAiError: (u: unknown) => u is AiError
```

### [PROVIDER_RESOLUTION] — cross-reference to adapter pages

The concrete provider adapters are catalogued on their own pages and are not duplicated here. Every
adapter follows one shape this core defines: a `Context.Tag` client service
(`make`/`layer`/`layerConfig` constructors taking `Redacted.Redacted` credentials and an `HttpClient`
requirement), a `Config` reference class with a per-call `withConfigOverride` combinator, and
`model(...)` / `layer(...)` (plus `modelWithTokenizer` / `layerWithTokenizer` where the provider ships
a tokenizer) returning `Model.Model<provider, LanguageModel.LanguageModel, Client>` — the
provider-tagged form of the `Model` interface defined in this page's [PUBLIC_TYPES]. Resolving any
adapter `model(...)` satisfies the core `LanguageModel.LanguageModel` tag, so provider choice is a
single composition-root layer swap.

| [INDEX] | [PROVIDER]      | [PAGE]                   | [MODEL_ENTRY]                                                                     |
| :-----: | :-------------- | :----------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | OpenAI          | `effect-ai-openai.md`    | `OpenAiLanguageModel.model` / `.modelWithTokenizer`; `OpenAiEmbeddingModel.model` |
|  [02]   | Anthropic       | `effect-ai-anthropic.md` | `AnthropicLanguageModel.model` / `.modelWithTokenizer`                            |
|  [03]   | Google (Gemini) | `effect-ai-google.md`    | `GoogleLanguageModel.model`                                                       |
|  [04]   | OpenRouter      | (adapter page)           | `OpenRouterLanguageModel.model`                                                   |
|  [05]   | Amazon Bedrock  | (adapter page)           | `AmazonBedrockLanguageModel.model`                                                |

## [03]-[IMPLEMENTATION_LAW]

[SERVICE_TAG_LAW]:
- Every capability is a `Context.Tag` class with a stable string id (`"@effect/ai/<Module>"`); resolve with `yield* LanguageModel.LanguageModel` or call the free `LanguageModel.generateText` which adds the tag to the `R` channel. Internal code references the tag, never a concrete provider type.
- The free functions (`LanguageModel.generateText` / `generateObject` / `streamText`) require `LanguageModel.LanguageModel` in context; the `Service` methods do not. Prefer the free functions at call sites and satisfy the requirement with a provider `Model` layer at the composition root.
- `generateObject` additionally threads the schema's decode context `R` into the requirement channel; provide schema dependencies alongside the model layer.

[MODEL_RESOLUTION_LAW]:
- A provider `model(...)` returns `Model.Model<provider, Provides, Client>` which is simultaneously a `Layer` and an `Effect<Layer>`. Provide it directly (`Effect.provide(OpenAiLanguageModel.model("gpt-4o"))`) to satisfy `LanguageModel.LanguageModel`; the provider `Client` layer (`OpenAiClient.layer({...})`) must be provided beneath it, and an `HttpClient.HttpClient` beneath that.
- Switching providers is a single layer swap at the composition root — `OpenAiLanguageModel.model` ↔ `AnthropicLanguageModel.model` — because both produce the same `LanguageModel.LanguageModel` tag. `Model.make(provider, layer)` is the manual escape hatch and also publishes a `Model.ProviderName` tag for telemetry/branching.
- Use `modelWithTokenizer` / `layerWithTokenizer` when a `Tokenizer.Tokenizer` is required (truncation, token budgeting); otherwise the plain `model` omits it from the provides set.
- The adapter `Client` layer beneath a `model(...)` requires `HttpClient.HttpClient`; credential and transport-policy law (redacted keys vs. `Config`, `transformClient`) lives on the per-provider adapter pages. The core only mandates that the `Client` layer and an `HttpClient` are provided beneath the resolved `Model`.

[DATA_ALGEBRA_LAW]:
- `Prompt`, `Response`, `Tool` parts are dual: each part/message type has a decoded interface, a paired `*Encoded` interface, and a `Schema` constant of the same name. Construct decoded values with the lowercase factories (`Prompt.textPart`, `Response.finishPart`) or the polymorphic `makePart(type, params)` / `makeMessage(role, params)` discriminators — do not hand-build the branded shape.
- `Prompt.RawInput` (`string | Iterable<MessageEncoded> | Prompt`) is the universal input; every `generate*`/`stream*` accepts it, so a bare string, an encoded-message array, or a built `Prompt` are interchangeable at the boundary.
- `Response.StreamPart` carries the streaming lifecycle (`text-start`/`text-delta`/`text-end`, `reasoning-*`, `tool-params-*`, `finish`, `error`); fold over `part.type` in `Stream.runForEach`. The non-streaming `Response.Part` collapses these into settled parts and is what `GenerateTextResponse.content` holds.

[TOOL_LAW]:
- `Tool.make(name, {parameters, success, failure, failureMode})` defines an application tool whose `parameters`/`success`/`failure` are `effect/Schema` structs; `failureMode: "error"` (default) routes tool failures into the effect error channel, `"return"` surfaces them as data in the result. Collect tools with `Toolkit.make(...)`, then bind handlers with `toolkit.toLayer({ ToolName: (params) => Effect... })` to obtain a `Toolkit.WithHandler`.
- `Tool.providerDefined({id, toolkitName, providerName, args, requiresHandler?})` models provider-executed tools (web search, code execution, computer-use); the provider runs them, and a handler is only needed when `requiresHandler` is set.
- Pass the bound toolkit via `GenerateTextOptions.toolkit`; `ExtractError`/`ExtractContext` then widen the call's error and requirement channels by the tools' handler errors and dependencies. `toolChoice` (`"auto" | "none" | "required" | {tool} | {mode,oneOf}`) constrains selection.

[ERROR_LAW]:
- All operations fail with the `AiError` union of `Schema.TaggedError` classes; branch with `Effect.catchTag("HttpResponseError", …)` or `Match.tag` on the `_tag` literal. `AiError.isAiError` guards an `unknown`. Tool-handler and schema-decode errors are unioned in by `ExtractError`, so a typed catch must account for both the transport rail and the domain rail.

[TELEMETRY_LAW]:
- `Telemetry.addGenAIAnnotations(span, options)` (dual; also `addGenAIAnnotations(options)(span)`) writes OpenTelemetry GenAI semantic-convention attributes; populate `system` from `WellKnownSystem`, `operation.name` from `WellKnownOperationName`, and `usage`/`request`/`response` from the model result. The provider `*Telemetry` modules pre-bind the provider system; prefer them over raw attribute writes.

[RAIL_LAW]:
- `@effect/ai` core: provider-agnostic; imported by any tier; carries no transport or credential surface itself.
- Provider packages (`@effect/ai-openai`, `-anthropic`, `-google`, `-openrouter`, `-amazon-bedrock`): each is an additive composition-root row supplying one `Client` layer + one or more `Model` constructors; they depend on `@effect/platform` `HttpClient` and never appear in domain code beyond the root wiring.
