# [TYPESCRIPT_API_EFFECT_AI]

Provider-agnostic LLM core for the Effect runtime: the `@effect/ai` package — language models,
embeddings, stateful chat, tools/toolkits, the prompt and response data algebra, tokenization, GenAI
telemetry, and the MCP server. This page owns the core package only. The concrete provider adapter
packages that resolve a provider-tagged `Model` into these core service tags are catalogued on their
own pages (`api-effect-ai-openai.md`, `api-effect-ai-anthropic.md`, `api-effect-ai-google.md`, and the
OpenRouter/Amazon-Bedrock adapter pages); see PROVIDER_RESOLUTION below for the seam they bind to. The
core defines the `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Chat`/`IdGenerator` service tags and the
prompt/response/tool data model; all success/failure flows through the `AiError` union; all I/O is
`Effect`/`Stream`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai`
- package: `@effect/ai`
- entry: `@effect/ai` (re-exports modules `AiError`, `Chat`, `EmbeddingModel`, `IdGenerator`, `LanguageModel`, `McpSchema`, `McpServer`, `Model`, `Prompt`, `Response`, `Telemetry`, `Tokenizer`, `Tool`, `Toolkit`)
- asset: provider-agnostic LLM service tags, prompt/response/tool data algebra, stateful chat, embeddings, tokenizer, GenAI OpenTelemetry conventions, MCP server
- rail: ai-core

## [2]-[PUBLIC_TYPES]

### @effect/ai — service tags and model entry points

[PUBLIC_TYPE_SCOPE]: language model service
- rail: ai-core

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]       | [RAIL]                                                                            |
| :-----: | :-------------------------------------- | :------------------ | :-------------------------------------------------------------------------------- |
|   [1]   | `LanguageModel.LanguageModel`           | `Context.Tag` class | tag `"@effect/ai/LanguageModel"` → `Service`                                      |
|   [2]   | `LanguageModel.Service`                 | interface           | `generateText` / `generateObject` / `streamText`                                  |
|   [3]   | `LanguageModel.generateText`            | function            | tag-resolving free function (requires `LanguageModel`)                            |
|   [4]   | `LanguageModel.generateObject`          | function            | schema-typed structured output                                                    |
|   [5]   | `LanguageModel.streamText`              | function            | `Stream<Response.StreamPart<Tools>>`                                              |
|   [6]   | `LanguageModel.make`                    | constructor         | builds `Service` from provider `ConstructorParams`                                |
|   [7]   | `LanguageModel.GenerateTextOptions`     | options interface   | `prompt`, `toolkit?`, `toolChoice?`, `concurrency?`, `disableToolCallResolution?` |
|   [8]   | `LanguageModel.GenerateObjectOptions`   | options interface   | extends text options + `schema`, `objectName?`                                    |
|   [9]   | `LanguageModel.ToolChoice<Tools>`       | union               | `"auto" \| "none" \| "required" \| {tool} \| {mode?,oneOf}`                       |
|  [10]   | `LanguageModel.GenerateTextResponse`    | class               | accessors `text`/`reasoning`/`toolCalls`/`usage`/`finishReason`                   |
|  [11]   | `LanguageModel.GenerateObjectResponse`  | class               | extends text response + `value: A`                                                |
|  [12]   | `LanguageModel.ProviderOptions`         | interface           | normalized options passed to provider impls                                       |
|  [13]   | `LanguageModel.ConstructorParams`       | interface           | provider `generateText` / `streamText` impls                                      |
|  [14]   | `LanguageModel.ExtractError<Options>`   | conditional type    | infers error rail from toolkit shape                                              |
|  [15]   | `LanguageModel.ExtractContext<Options>` | conditional type    | infers required services from toolkit                                             |

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
|   [1]   | `Model.Model<P,Prov,Req>` | branded interface   | both a `Layer` and an `Effect<Layer>`; carries `provider`    |
|   [2]   | `Model.make`              | constructor         | `(provider, layer) => Model` — tags a layer with provider id |
|   [3]   | `Model.ProviderName`      | `Context.Tag` class | tag `"@effect/ai/Model/ProviderName"` → `string`             |
|   [4]   | `Model.TypeId`            | unique const        | `"~@effect/ai/Model"`                                        |

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
|   [1]   | `EmbeddingModel.EmbeddingModel` | `Context.Tag` class | tag `"@effect/ai/EmbeddingModel"` → `Service`              |
|   [2]   | `EmbeddingModel.Service`        | interface           | `embed(input)` / `embedMany(input, {concurrency?})`        |
|   [3]   | `EmbeddingModel.Result`         | interface           | `{ index, embeddings }`                                    |
|   [4]   | `EmbeddingModel.make`           | constructor         | per-call batching + optional cache `{capacity,timeToLive}` |
|   [5]   | `EmbeddingModel.makeDataLoader` | constructor         | windowed data-loader batching `{window, maxBatchSize?}`    |

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
|   [1]   | `Chat.Chat`              | `Context.Tag` class | tag `"@effect/ai/Chat"` → `Service`                                                  |
|   [2]   | `Chat.Service`           | interface           | `history: Ref`, `export`, `exportJson`, `generateText`/`streamText`/`generateObject` |
|   [3]   | `Chat.empty`             | constructor effect  | new empty chat session                                                               |
|   [4]   | `Chat.fromPrompt`        | constructor         | seed chat from `Prompt.RawInput`                                                     |
|   [5]   | `Chat.fromExport`        | constructor         | rehydrate from `export` payload (`unknown`)                                          |
|   [6]   | `Chat.fromJson`          | constructor         | rehydrate from JSON string                                                           |
|   [7]   | `Chat.Persistence`       | `Context.Tag` class | tag `"@effect/ai/Chat/Persisted"` → `Persistence.Service`                            |
|   [8]   | `Chat.Persisted`         | interface           | extends `Service` + `id`, `save`                                                     |
|   [9]   | `Chat.makePersisted`     | constructor         | persisted chat by `storeId`                                                          |
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

[PUBLIC_TYPE_SCOPE]: tokenizer, id generator, telemetry
- rail: ai-core

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [RAIL]                                                                                        |
| :-----: | :----------------------------------------- | :------------------ | :-------------------------------------------------------------------------------------------- |
|   [1]   | `Tokenizer.Tokenizer`                      | `Context.Tag` class | tag `"@effect/ai/Tokenizer"` → `Service`                                                      |
|   [2]   | `Tokenizer.Service`                        | interface           | `tokenize(input)` / `truncate(input, tokens)`                                                 |
|   [3]   | `Tokenizer.make`                           | constructor         | from a `tokenize` impl                                                                        |
|   [4]   | `IdGenerator.IdGenerator`                  | `Context.Tag` class | tag `"@effect/ai/IdGenerator"` → `Service`                                                    |
|   [5]   | `IdGenerator.Service`                      | interface           | `generateId(): Effect<string>`                                                                |
|   [6]   | `IdGenerator.MakeOptions`                  | options interface   | `alphabet`, `prefix?`, `separator`, `size`                                                    |
|   [7]   | `IdGenerator.defaultIdGenerator`           | value               | default `Service`                                                                             |
|   [8]   | `IdGenerator.make` / `.layer`              | constructor/layer   | `layer` fails with `Cause.IllegalArgumentException`                                           |
|   [9]   | `Telemetry.addGenAIAnnotations`            | function            | writes GenAI semantic-convention span attributes                                              |
|  [10]   | `Telemetry.addSpanAttributes`              | function            | low-level prefixed-attribute writer (`keyPrefix`, attrs) that `addGenAIAnnotations` builds on |
|  [11]   | `Telemetry.GenAITelemetryAttributeOptions` | options             | `BaseAttributes` (system) + `operation?`, `request?`, `response?`, `token?`, `usage?`         |
|  [12]   | `Telemetry.SpanTransformer`                | interface           | span-mutation callback shape held by the tag                                                  |
|  [13]   | `Telemetry.CurrentSpanTransformer`         | `Context.Tag` class | tag `"@effect/ai/Telemetry/CurrentSpanTransformer"` → `SpanTransformer`                       |
|  [14]   | `Telemetry.WellKnownSystem`                | union               | `"anthropic" \| "aws.bedrock" \| "openai" \| "gemini" \| …`                                   |
|  [15]   | `Telemetry.WellKnownOperationName`         | union               | `"chat" \| "embeddings" \| "text_completion"`                                                 |

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

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]     | [RAIL]                                                                |
| :-----: | :---------------------------------------------------- | :---------------- | :-------------------------------------------------------------------- |
|   [1]   | `Prompt.Prompt`                                       | branded interface | conversation; `Prompt` Schema; `TypeId` `"~@effect/ai/Prompt"`        |
|   [2]   | `Prompt.RawInput`                                     | union             | `string \| Iterable<MessageEncoded> \| Prompt`                        |
|   [3]   | `Prompt.make`                                         | constructor       | `(input: RawInput) => Prompt`                                         |
|   [4]   | `Prompt.empty` / `.fromMessages`                      | constructor       | empty prompt / from `ReadonlyArray<Message>`                          |
|   [5]   | `Prompt.fromResponseParts`                            | constructor       | lift `Response.AnyPart[]` into a prompt                               |
|   [6]   | `Prompt.merge`                                        | combinator        | concatenate prompts (dual)                                            |
|   [7]   | `Prompt.setSystem` / `prependSystem` / `appendSystem` | combinator        | system-message manipulation (dual)                                    |
|   [8]   | `Prompt.Message`                                      | union             | `System \| User \| Assistant \| Tool` message                         |
|   [9]   | `Prompt.makeMessage` / `*Message`                     | constructor       | `systemMessage`/`userMessage`/`assistantMessage`/`toolMessage`        |
|  [10]   | `Prompt.Part`                                         | union             | `Text \| Reasoning \| File \| ToolCall \| ToolResult` part            |
|  [11]   | `Prompt.makePart` / `*Part`                           | constructor       | `textPart`/`reasoningPart`/`filePart`/`toolCallPart`/`toolResultPart` |
|  [12]   | `Prompt.FromJson`                                     | Schema transform  | JSON ⇄ `Prompt`                                                       |
|  [13]   | `Prompt.ProviderOptions`                              | Schema record     | per-provider option bag keyed by provider name                        |

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

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]  | [RAIL]                                                                                                 |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------------------------------------------------------------- |
|   [1]   | `Response.AnyPart`                                      | union          | every decoded part (text/reasoning/tool/file/source/finish/error)                                      |
|   [2]   | `Response.Part<Tools>`                                  | union          | non-streaming parts parameterized by toolkit                                                           |
|   [3]   | `Response.StreamPart<Tools>`                            | union          | streaming parts incl. `*-start`/`*-delta`/`*-end` + `error`                                            |
|   [4]   | `Response.makePart`                                     | constructor    | `(type, params) => Extract<AnyPart, {type}>`                                                           |
|   [5]   | `Response.FinishReason`                                 | Schema literal | `"stop" \| "length" \| "content-filter" \| "tool-calls" \| "error" \| "pause" \| "other" \| "unknown"` |
|   [6]   | `Response.Usage`                                        | Schema class   | `inputTokens`/`outputTokens`/`totalTokens`/`reasoningTokens?`/`cachedInputTokens?`                     |
|   [7]   | `Response.ToolCallParts<Tools>`                         | mapped union   | tool-call parts narrowed to toolkit tool names                                                         |
|   [8]   | `Response.ToolResultParts<Tools>`                       | mapped union   | tool-result parts narrowed to toolkit tool names                                                       |
|   [9]   | `Response.ProviderMetadata`                             | Schema record  | per-provider metadata bag on every part                                                                |
|  [10]   | `Response.TextPart` / `ToolCallPart` / `FinishPart` / … | Schema + ctor  | each part type has interface + `Schema` const + lowercase ctor                                         |
|  [11]   | `Response.AllParts(toolkit)`                            | Schema factory | full decoded-parts schema for a toolkit                                                                |

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

[PUBLIC_TYPE_SCOPE]: tool and toolkit model
- rail: ai-core

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [RAIL]                                                                            |
| :-----: | :------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------------------------- |
|   [1]   | `Tool.Tool<Name,Config,Req>`                                   | branded interface | user-defined tool; `TypeId` `"~@effect/ai/Tool"`                                  |
|   [2]   | `Tool.Any`                                                     | interface         | erased tool; toolkit element bound                                                |
|   [3]   | `Tool.ProviderDefined<Name,Cfg>`                               | branded interface | provider-executed tool; `ProviderDefinedTypeId`                                   |
|   [4]   | `Tool.make`                                                    | constructor       | `(name, {description?,parameters?,success?,failure?,failureMode?,dependencies?})` |
|   [5]   | `Tool.providerDefined`                                         | constructor       | `({id,toolkitName,providerName,args,…})` provider tool                            |
|   [6]   | `Tool.fromTaggedRequest`                                       | constructor       | lift a `Schema.TaggedRequest` into a `Tool`                                       |
|   [7]   | `Tool.FailureMode`                                             | union             | `"error" \| "return"`                                                             |
|   [8]   | `Tool.Handler` / `HandlerResult` / `HandlerError`              | interface/type    | handler contract + per-tool error inference                                       |
|   [9]   | `Tool.getJsonSchema` / `getDescription`                        | function          | JSON-schema + description extraction (plus `*FromSchemaAst` AST variants)         |
|  [10]   | `Tool.isUserDefined` / `isProviderDefined`                     | guard             | discriminate `Tool` vs `ProviderDefined` on an `unknown`                          |
|  [11]   | `Tool.Title`/`Readonly`/`Destructive`/`Idempotent`/`OpenWorld` | annotation refs   | MCP-style tool annotation tags (`"@effect/ai/Tool/<Name>"`)                       |
|  [11]   | `Toolkit.Toolkit<Tools>`                                       | branded interface | tool collection; `TypeId` `"~@effect/ai/Toolkit"`                                 |
|  [12]   | `Toolkit.make`                                                 | constructor       | `(...tools) => Toolkit`                                                           |
|  [13]   | `Toolkit.merge`                                                | combinator        | merge multiple toolkits                                                           |
|  [14]   | `Toolkit.WithHandler<Tools>`                                   | interface         | toolkit bound to handlers; `handle(name, params)`                                 |
|  [15]   | `Toolkit.empty`                                                | value             | `Toolkit<{}>`                                                                     |

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
|   [1]   | `AiError.AiError`           | union + Schema     | `HttpRequestError \| HttpResponseError \| MalformedInput \| MalformedOutput \| UnknownError` |
|   [2]   | `AiError.HttpRequestError`  | tagged error class | transport-layer request failure                                                              |
|   [3]   | `AiError.HttpResponseError` | tagged error class | response failure (carries response detail)                                                   |
|   [4]   | `AiError.MalformedInput`    | tagged error class | input does not match expected shape                                                          |
|   [5]   | `AiError.MalformedOutput`   | tagged error class | output cannot be parsed/validated                                                            |
|   [6]   | `AiError.UnknownError`      | tagged error class | catch-all runtime failure                                                                    |
|   [7]   | `AiError.isAiError`         | guard              | `(u: unknown) => u is AiError`                                                               |
|   [8]   | `AiError.TypeId`            | unique const       | `"~@effect/ai/AiError"`                                                                      |

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

| [INDEX] | [PROVIDER]      | [PAGE]                       | [MODEL_ENTRY]                                                                     |
| :-----: | :-------------- | :--------------------------- | :-------------------------------------------------------------------------------- |
|   [1]   | OpenAI          | `api-effect-ai-openai.md`    | `OpenAiLanguageModel.model` / `.modelWithTokenizer`; `OpenAiEmbeddingModel.model` |
|   [2]   | Anthropic       | `api-effect-ai-anthropic.md` | `AnthropicLanguageModel.model` / `.modelWithTokenizer`                            |
|   [3]   | Google (Gemini) | `api-effect-ai-google.md`    | `GoogleLanguageModel.model`                                                       |
|   [4]   | OpenRouter      | (adapter page)               | `OpenRouterLanguageModel.model`                                                   |
|   [5]   | Amazon Bedrock  | (adapter page)               | `AmazonBedrockLanguageModel.model`                                                |

## [3]-[IMPLEMENTATION_LAW]

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
