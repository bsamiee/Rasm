# [API_CATALOGUE] effect-ai-openai

Grounded from installed `node_modules` type declarations (`@effect/ai-openai@0.40.0`, `dist/dts/*.d.ts`). Covers every load-bearing public surface the TS planning pages consume — the eight owner modules re-exported from the package root. The `Generated` module (1236 OpenAPI-derived `Schema.Class` owners + the `make` Client factory, a 219-method REST `Client` interface, and a `ClientError` rail) is summarized as a category, not enumerated; only the named anchors the owner modules reference are spelled exactly. Sibling provider packages (`@effect/ai`, `@effect/ai-anthropic`) share the same `LanguageModel`/`EmbeddingModel`/`Tool`/`Tokenizer`/`Telemetry` contracts — this package is the OpenAI binding.

Package root re-exports (namespace barrels):

```ts
// @effect/ai-openai
export * as Generated from "./Generated.js"
export * as OpenAiClient from "./OpenAiClient.js"
export * as OpenAiConfig from "./OpenAiConfig.js"
export * as OpenAiEmbeddingModel from "./OpenAiEmbeddingModel.js"
export * as OpenAiLanguageModel from "./OpenAiLanguageModel.js"
export * as OpenAiTelemetry from "./OpenAiTelemetry.js"
export * as OpenAiTokenizer from "./OpenAiTokenizer.js"
export * as OpenAiTool from "./OpenAiTool.js"
```

---

## [1] — OpenAiClient

The provider transport owner: a `Context.TagClass` whose service wraps the generated REST `Client` plus the Responses-API streaming surface.

```ts
// @effect/ai-openai/OpenAiClient
declare const OpenAiClient_base: Context.TagClass<OpenAiClient, "@effect/ai-openai/OpenAiClient", Service>
export declare class OpenAiClient extends OpenAiClient_base {}

export interface Service {
  readonly client: Generated.Client
  readonly streamRequest: <A, I, R>(
    request: HttpClientRequest.HttpClientRequest,
    schema: Schema.Schema<A, I, R>
  ) => Stream.Stream<A, AiError.AiError, R>
  readonly createResponse: (
    options: typeof Generated.CreateResponse.Encoded
  ) => Effect.Effect<Generated.Response, AiError.AiError>
  readonly createResponseStream: (
    options: Omit<typeof Generated.CreateResponse.Encoded, "stream">
  ) => Stream.Stream<ResponseStreamEvent, AiError.AiError>
  readonly createEmbedding: (
    options: typeof Generated.CreateEmbeddingRequest.Encoded
  ) => Effect.Effect<Generated.CreateEmbeddingResponse, AiError.AiError>
}

export type StreamCompletionRequest = Omit<typeof Generated.CreateChatCompletionRequest.Encoded, "stream">
```

Constructors / layers (the wire-edge entry — note `make` requires `Scope`, `layer*` do not):

```ts
export declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly organizationId?: Redacted.Redacted | undefined
  readonly projectId?: Redacted.Redacted | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>

export declare const layer: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly organizationId?: Redacted.Redacted | undefined
  readonly projectId?: Redacted.Redacted | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Layer.Layer<OpenAiClient, never, HttpClient.HttpClient>

export declare const layerConfig: (options: {
  readonly apiKey?: Config.Config<Redacted.Redacted | undefined> | undefined
  readonly apiUrl?: Config.Config<string | undefined> | undefined
  readonly organizationId?: Config.Config<Redacted.Redacted | undefined> | undefined
  readonly projectId?: Config.Config<Redacted.Redacted | undefined> | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Layer.Layer<OpenAiClient, ConfigError, HttpClient.HttpClient>
```

`layer` carries no error channel; `layerConfig` carries `ConfigError`. Both require `HttpClient.HttpClient` from `@effect/platform` (browser binding is `FetchHttpClient.layer`). The single canonical secret-resolution surface is `Redacted.Redacted` for `apiKey`/`organizationId`/`projectId`.

### ResponseStreamEvent

The streaming-response algebra: a 49-member `Schema.Union` of `Schema.Class` events, each tagged by a `type` literal. `createResponseStream` and `streamRequest` fold this union.

```ts
export type ResponseStreamEvent = typeof ResponseStreamEvent.Type
export declare const ResponseStreamEvent: Schema.Union<[
  typeof ResponseCreatedEvent, typeof ResponseQueuedEvent, typeof ResponseInProgressEvent,
  typeof ResponseCompletedEvent, typeof ResponseIncompleteEvent, typeof ResponseFailedEvent,
  typeof ResponseOutputItemAddedEvent, typeof ResponseOutputItemDoneEvent,
  typeof ResponseContentPartAddedEvent, typeof ResponseContentPartDoneEvent,
  typeof ResponseOutputTextDeltaEvent, typeof ResponseOutputTextDoneEvent,
  typeof ResponseOutputTextAnnotationAddedEvent,
  typeof ResponseRefusalDeltaEvent, typeof ResponseRefusalDoneEvent,
  typeof ResponseFunctionCallArgumentsDeltaEvent, typeof ResponseFunctionCallArgumentsDoneEvent,
  typeof ResponseFileSearchCallInProgressEvent, typeof ResponseFileSearchCallSearchingEvent,
  typeof ResponseFileSearchCallCompletedEvent,
  typeof ResponseWebSearchCallInProgressEvent, typeof ResponseWebSearchCallSearchingEvent,
  typeof ResponseWebSearchCallCompletedEvent,
  typeof ResponseReasoningSummaryPartAddedEvent, typeof ResponseReasoningSummaryPartDoneEvent,
  typeof ResponseReasoningSummaryTextDeltaEvent, typeof ResponseReasoningSummaryTextDoneEvent,
  typeof ResponseReasoningTextDeltaEvent, typeof ResponseReasoningTextDoneEvent,
  typeof ResponseImageGenerationCallInProgressEvent, typeof ResponseImageGenerationCallGeneratingEvent,
  typeof ResponseImageGenerationCallPartialImageEvent, typeof ResponseImageGenerationCallCompletedEvent,
  typeof ResponseMcpCallArgumentsDeltaEvent, typeof ResponseMcpCallArgumentsDoneEvent,
  typeof ResponseMcpCallInProgressEvent, typeof ResponseMcpCallCompletedEvent, typeof ResponseMcpCallFailedEvent,
  typeof ResponseMcpListToolsInProgressEvent, typeof ResponseMcpListToolsCompletedEvent,
  typeof ResponseMcpListToolsFailedEvent,
  typeof ResponseCodeInterpreterCallInProgressEvent, typeof ResponseCodeInterpreterCallInterpretingEvent,
  typeof ResponseCodeInterpreterCallCompletedEvent, typeof ResponseCodeInterpreterCallCodeDeltaEvent,
  typeof ResponseCodeInterpreterCallCodeDoneEvent,
  typeof ResponseCustomToolCallInputDeltaEvent, typeof ResponseCustomToolCallInputDoneEvent,
  typeof ResponseErrorEvent
]>
```

Every member is an exported `Schema.Class` discriminated on its `type` literal. Each carries `sequence_number: Schema.Int` plus event-specific fields. The shared field shapes (verbatim from the declarations):

```ts
// lifecycle events carry the full response object
export declare class ResponseCreatedEvent    // { type: "response.created";    response: Generated.Response; sequence_number: number }
export declare class ResponseQueuedEvent      // { type: "response.queued";     response; sequence_number }
export declare class ResponseInProgressEvent  // { type: "response.in_progress"; response; sequence_number }
export declare class ResponseCompletedEvent   // { type: "response.completed";  response; sequence_number }
export declare class ResponseIncompleteEvent  // { type: "response.incomplete"; response; sequence_number }
export declare class ResponseFailedEvent      // { type: "response.failed";     response; sequence_number }

// output-item events
export declare class ResponseOutputItemAddedEvent
//   { type: "response.output_item.added"; output_index: number; sequence_number;
//     item: Generated.OutputItem | { type: "web_search_call"; id: string; status: "failed"|"in_progress"|"completed"|"searching" } }
export declare class ResponseOutputItemDoneEvent
//   { type: "response.output_item.done"; output_index; item: Generated.OutputItem; sequence_number }

// content-part events: part: Union<OutputTextContent | RefusalContent | ReasoningTextContent>
export declare class ResponseContentPartAddedEvent  // type: "response.content_part.added";  output_index; content_index; item_id: string; part
export declare class ResponseContentPartDoneEvent   // type: "response.content_part.done";   output_index; content_index; item_id; part

// text deltas: delta/text: string; logprobs?: ReadonlyArray<LogProbs> | null
export declare class ResponseOutputTextDeltaEvent   // type: "response.output_text.delta";   delta; logprobs?
export declare class ResponseOutputTextDoneEvent    // type: "response.output_text.done";    text;  logprobs?
export declare class ResponseOutputTextAnnotationAddedEvent
//   type: "response.output_text.annotation.added"; annotation_index; annotation: Generated.Annotation

// refusal / function-call-arguments / reasoning-summary / reasoning-text deltas: delta or finalized string field
export declare class ResponseRefusalDeltaEvent                 // type: "response.refusal.delta";        delta
export declare class ResponseRefusalDoneEvent                  // type: "response.refusal.done";         refusal
export declare class ResponseFunctionCallArgumentsDeltaEvent   // type: "response.function_call_arguments.delta"; delta
export declare class ResponseFunctionCallArgumentsDoneEvent    // type: "response.function_call_arguments.done";  arguments
export declare class ResponseReasoningSummaryPartAddedEvent    // summary_index; part: SummaryPart
export declare class ResponseReasoningSummaryPartDoneEvent     // summary_index; part: SummaryPart
export declare class ResponseReasoningSummaryTextDeltaEvent    // summary_index; delta
export declare class ResponseReasoningSummaryTextDoneEvent     // summary_index; text
export declare class ResponseReasoningTextDeltaEvent           // delta
export declare class ResponseReasoningTextDoneEvent            // text

// tool-call progress events (status-only: type + output_index + item_id + sequence_number)
export declare class ResponseFileSearchCallInProgressEvent  // type: "response.file_search_call.in_progress"
export declare class ResponseFileSearchCallSearchingEvent   // ".searching"
export declare class ResponseFileSearchCallCompletedEvent   // ".completed"
export declare class ResponseWebSearchCallInProgressEvent
export declare class ResponseWebSearchCallSearchingEvent
export declare class ResponseWebSearchCallCompletedEvent
export declare class ResponseImageGenerationCallInProgressEvent
export declare class ResponseImageGenerationCallGeneratingEvent
export declare class ResponseImageGenerationCallPartialImageEvent
export declare class ResponseImageGenerationCallCompletedEvent
export declare class ResponseMcpCallArgumentsDeltaEvent
export declare class ResponseMcpCallArgumentsDoneEvent
export declare class ResponseMcpCallInProgressEvent
export declare class ResponseMcpCallCompletedEvent
export declare class ResponseMcpCallFailedEvent
export declare class ResponseMcpListToolsInProgressEvent
export declare class ResponseMcpListToolsCompletedEvent
export declare class ResponseMcpListToolsFailedEvent
export declare class ResponseCodeInterpreterCallInProgressEvent
export declare class ResponseCodeInterpreterCallInterpretingEvent
export declare class ResponseCodeInterpreterCallCompletedEvent
export declare class ResponseCodeInterpreterCallCodeDeltaEvent  // delta
export declare class ResponseCodeInterpreterCallCodeDoneEvent   // code
export declare class ResponseCustomToolCallInputDeltaEvent      // delta
export declare class ResponseCustomToolCallInputDoneEvent       // input
export declare class ResponseErrorEvent                         // type: "error"; error fields

// shared value objects
export declare class LogProbs
//   { token: string; logprob: number; top_logprobs: ReadonlyArray<{ token: string; logprob: number }> }
export declare class SummaryPart   // { type: "summary_text"; text: string }
```

`ResponseStreamEvent` is the one canonical fold surface for OpenAI streaming; the planning pages discriminate on `event.type` rather than maintaining parallel event handlers. All errors collapse to `AiError.AiError` (`@effect/ai/AiError`) — the package never surfaces a bespoke error rail.

---

## [2] — OpenAiLanguageModel

The chat/Responses-API language-model binding onto `@effect/ai`'s provider-agnostic `LanguageModel`/`Model` contracts.

```ts
// @effect/ai-openai/OpenAiLanguageModel
export type Model = typeof Generated.ChatModel.Encoded | typeof Generated.ModelIdsResponsesEnum.Encoded

declare const Config_base: Context.TagClass<Config, "@effect/ai-openai/OpenAiLanguageModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<Config.Service | undefined>
}

export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof Generated.CreateResponse.Encoded, "input" | "tools" | "tool_choice" | "stream" | "text">>> {
    readonly fileIdPrefixes?: ReadonlyArray<string>
    readonly text?: { readonly verbosity?: "low" | "medium" | "high" }
    readonly strict?: boolean   // default true; gate for OpenAI structured-outputs strict mode
  }
}
```

Entry points (model accepts any `string & {}` widened literal or a known `Model` enum value; `config` always omits `model`):

```ts
export declare const model: (
  model: (string & {}) | Model,
  config?: Omit<Config.Service, "model">
) => AiModel.Model<"openai", LanguageModel.LanguageModel, OpenAiClient>

export declare const modelWithTokenizer: (
  model: (string & {}) | Model,
  config?: Omit<Config.Service, "model">
) => AiModel.Model<"openai", LanguageModel.LanguageModel | Tokenizer.Tokenizer, OpenAiClient>

export declare const make: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Omit<Config.Service, "model">
}) => Effect.Effect<LanguageModel.Service, never, OpenAiClient>

export declare const layer: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Omit<Config.Service, "model">
}) => Layer.Layer<LanguageModel.LanguageModel, never, OpenAiClient>

export declare const layerWithTokenizer: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Omit<Config.Service, "model">
}) => Layer.Layer<LanguageModel.LanguageModel | Tokenizer.Tokenizer, never, OpenAiClient>

export declare const withConfigOverride: {
  (overrides: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, overrides: Config.Service): Effect.Effect<A, E, R>
}
```

The module also augments `@effect/ai/Prompt` and `@effect/ai/Response` via `declare module` to attach OpenAI-specific `providerOptions.openai` / `providerMetadata.openai` slots (image `imageDetail`, reasoning `itemId`/`encryptedContent`, citation `index`/`startIndex`/`endIndex`, finish `serviceTier`). These are the boundary hooks for OpenAI-specific prompt/response metadata; internal code reads them through the canonical `@effect/ai` Prompt/Response shapes.

`model`/`modelWithTokenizer` produce an `AiModel.Model<"openai", ...>` that resolves against the `OpenAiClient` service. `layer*` install a `LanguageModel.LanguageModel` (optionally `+ Tokenizer.Tokenizer`) requiring `OpenAiClient`. `Config` is a request-scoped override carrier, not the transport config.

---

## [3] — OpenAiEmbeddingModel

Embedding binding with two batching modalities behind one polymorphic `model` entry plus per-mode layers.

```ts
// @effect/ai-openai/OpenAiEmbeddingModel
export type Model = typeof Generated.CreateEmbeddingRequestModelEnum.Encoded

declare const Config_base: Context.TagClass<Config, "@effect/ai-openai/OpenAiEmbeddingModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<Config.Service | undefined>
}

export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof Generated.CreateEmbeddingRequest.Encoded, "input">>> {}
  interface Batched extends Omit<Config.Service, "model"> {
    readonly maxBatchSize?: number
    readonly cache?: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }
  }
  interface DataLoader extends Omit<Config.Service, "model"> {
    readonly window: Duration.DurationInput
    readonly maxBatchSize?: number
  }
}

// one polymorphic entry discriminating on `mode`
export declare const model: (
  model: (string & {}) | Model,
  { mode, ...config }: Simplify<
    ({ readonly mode: "batched" } & Config.Batched) |
    ({ readonly mode: "data-loader" } & Config.DataLoader)
  >
) => AiModel.Model<"openai", EmbeddingModel.EmbeddingModel, OpenAiClient.OpenAiClient>

export declare const makeDataLoader: (options: {
  readonly model: (string & {}) | Model
  readonly config: Config.DataLoader
}) => Effect.Effect<EmbeddingModel.Service, never, OpenAiClient.OpenAiClient | Scope>

export declare const layerBatched: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Config.Batched
}) => Layer.Layer<EmbeddingModel.EmbeddingModel, never, OpenAiClient.OpenAiClient>

export declare const layerDataLoader: (options: {
  readonly model: (string & {}) | Model
  readonly config: Config.DataLoader
}) => Layer.Layer<EmbeddingModel.EmbeddingModel, never, OpenAiClient.OpenAiClient>

export declare const withConfigOverride: {
  (config: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, config: Config.Service): Effect.Effect<A, E, R>
}
```

`"batched"` coalesces calls up to `maxBatchSize` with an optional bounded cache; `"data-loader"` windows requests over a `Duration`. `makeDataLoader` requires `Scope` (it spawns a background batcher); the layers are scoped internally.

---

## [4] — OpenAiTool

Provider-defined tool constructors yielding `@effect/ai/Tool.ProviderDefined`. Each returns a tool tagged `"OpenAi<Name>"` with `args` (configuration schema), `parameters` (model-supplied call args), `success`/`failure` schemas, and a `failureMode` defaulting to `"error"`.

```ts
// @effect/ai-openai/OpenAiTool
export declare const CodeInterpreter: <Mode extends Tool.FailureMode | undefined = undefined>(args: {
  readonly container: string | { readonly type: "auto"; readonly file_ids?: readonly string[] | undefined }
}) => Tool.ProviderDefined<"OpenAiCodeInterpreter", {
  readonly args: Schema.Struct<{ container: Schema.Union<[typeof Schema.String, Schema.Struct<{ type; file_ids }>]> }>
  readonly parameters: Schema.Struct<{ code: Schema.NullOr<typeof Schema.String>; container_id: typeof Schema.String }>
  readonly success: Schema.NullOr<Schema.Array$<Schema.Union<[typeof Generated.CodeInterpreterOutputLogs, typeof Generated.CodeInterpreterOutputImage]>>>
  readonly failure: typeof Schema.Never
  readonly failureMode: Mode extends undefined ? "error" : Mode
}, false>

export declare const FileSearch: <Mode extends Tool.FailureMode | undefined = undefined>(args: {
  readonly vector_store_ids: readonly string[]
  readonly max_num_results?: number | null | undefined
  readonly ranking_options?: {
    readonly ranker?: "auto" | "default-2024-11-15" | null | undefined
    readonly score_threshold?: number | null | undefined
    readonly hybrid_search?: { readonly embedding_weight: number; readonly text_weight: number } | null | undefined
  } | null | undefined
  readonly filters?: ComparisonFilter | CompoundFilter | null | undefined
}) => Tool.ProviderDefined<"OpenAiFileSearch", {
  readonly args: Schema.Struct<{ max_num_results; ranking_options: Generated.RankingOptions; vector_store_ids; filters: Generated.Filters }>
  readonly parameters: Tool.EmptyParams
  readonly success: Schema.SchemaClass<{ status: "failed"|"in_progress"|"completed"|"incomplete"|"searching"; queries: readonly string[]; results?: ... }, ..., never>
  readonly failure: typeof Schema.Never
  readonly failureMode: Mode extends undefined ? "error" : Mode
}, false>

export declare const WebSearch: <Mode extends Tool.FailureMode | undefined = undefined>(args: {
  readonly user_location?: { type?: "approximate"|null; country?; region?; city?; timezone? } | null | undefined
  readonly search_context_size?: "low" | "medium" | "high" | null | undefined
  readonly filters?: { readonly allowed_domains?: readonly string[] | null | undefined } | null | undefined
}) => Tool.ProviderDefined<"OpenAiWebSearch", {
  readonly args: Schema.Struct<{ user_location; search_context_size: Generated.WebSearchToolSearchContextSize; filters }>
  readonly parameters: Schema.Struct<{ action: Schema.Union<[Generated.WebSearchActionSearch, Generated.WebSearchActionOpenPage, Generated.WebSearchActionFind]> }>
  readonly success: Schema.Struct<{ status: typeof Generated.WebSearchToolCallStatus }>
  readonly failure: typeof Schema.Never
  readonly failureMode: Mode extends undefined ? "error" : Mode
}, false>

export declare const WebSearchPreview: <Mode extends Tool.FailureMode | undefined = undefined>(args: {
  readonly user_location?: { type: "approximate"; country?; region?; city?; timezone? } | null | undefined
  readonly search_context_size?: "low" | "medium" | "high" | null | undefined
}) => Tool.ProviderDefined<"OpenAiWebSearchPreview", {
  readonly args: Schema.Struct<{ user_location: Generated.ApproximateLocation; search_context_size: Generated.SearchContextSize }>
  readonly parameters: Schema.Struct<{ action: Schema.Union<[Generated.WebSearchActionSearch, Generated.WebSearchActionOpenPage, Generated.WebSearchActionFind]> }>
  readonly success: Schema.Struct<{ status: typeof Generated.WebSearchToolCallStatus }>
  readonly failure: typeof Schema.Never
  readonly failureMode: Mode extends undefined ? "error" : Mode
}, false>
```

The `FileSearch` `filters` arm is the canonical OpenAI filter algebra: a comparison node `{ type: "eq"|"ne"|"gt"|"gte"|"lt"|"lte"; key: string; value: string|number|boolean|ReadonlyArray<string|number> }` or a compound node `{ type: "and"|"or"; filters: ReadonlyArray<comparison> }`. The `Mode` type parameter threads the failure-handling policy into the tool's static type.

---

## [5] — OpenAiTokenizer

Token-counting binding onto `@effect/ai/Tokenizer`.

```ts
// @effect/ai-openai/OpenAiTokenizer
export declare const make: (options: { readonly model: string }) => Tokenizer.Service
export declare const layer: (options: { readonly model: string }) => Layer.Layer<Tokenizer.Tokenizer>
```

`make` is pure (no Effect wrapper); `layer` provides the `Tokenizer.Tokenizer` tag with no dependencies. Composed implicitly by `OpenAiLanguageModel.layerWithTokenizer` / `modelWithTokenizer`.

---

## [6] — OpenAiConfig

A request-scoped client-transform override carrier — the one surface for mutating the underlying `HttpClient` per-request without rebuilding the transport layer.

```ts
// @effect/ai-openai/OpenAiConfig
declare const OpenAiConfig_base: Context.TagClass<OpenAiConfig, "@effect/ai-openai/OpenAiConfig", OpenAiConfig.Service>
export declare class OpenAiConfig extends OpenAiConfig_base {
  static readonly getOrUndefined: Effect.Effect<typeof OpenAiConfig.Service | undefined>
}

export declare namespace OpenAiConfig {
  interface Service {
    readonly transformClient?: (client: HttpClient) => HttpClient
  }
}

export declare const withClientTransform: {
  (transform: (client: HttpClient) => HttpClient): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, transform: (client: HttpClient) => HttpClient): Effect.Effect<A, E, R>
}
```

`withClientTransform` is dual (data-first and data-last) — the canonical way to scope a per-region/per-request `HttpClient` mutation onto any `Effect`.

---

## [7] — OpenAiTelemetry

GenAI OpenTelemetry attribute owner; extends `@effect/ai/Telemetry`'s provider-agnostic GenAI attribute set with OpenAI-namespaced request/response attributes.

```ts
// @effect/ai-openai/OpenAiTelemetry
export type OpenAiTelemetryAttributes = Simplify<
  Telemetry.GenAITelemetryAttributes &
  Telemetry.AttributesWithPrefix<RequestAttributes, "gen_ai.openai.request"> &
  Telemetry.AttributesWithPrefix<ResponseAttributes, "gen_ai.openai.request">
>
export type AllAttributes = Telemetry.AllAttributes & RequestAttributes & ResponseAttributes

export interface RequestAttributes {
  readonly responseFormat?: (string & {}) | WellKnownResponseFormat | null | undefined
  readonly serviceTier?: (string & {}) | WellKnownServiceTier | null | undefined
}
export interface ResponseAttributes {
  readonly serviceTier?: string | null | undefined
  readonly systemFingerprint?: string | null | undefined
}

export type WellKnownResponseFormat = "json_object" | "json_schema" | "text"
export type WellKnownServiceTier = "auto" | "default"

export type OpenAiTelemetryAttributeOptions = Telemetry.GenAITelemetryAttributeOptions & {
  openai?: { request?: RequestAttributes | undefined; response?: ResponseAttributes | undefined } | undefined
}

// dual; mutates the Span in-place
export declare const addGenAIAnnotations: ((options: OpenAiTelemetryAttributeOptions) => (span: Span) => void)
  & ((span: Span, options: OpenAiTelemetryAttributeOptions) => void)
```

`addGenAIAnnotations` is the single span-annotation entry; it mutates the `effect/Tracer` `Span` in place (boundary kernel — the only mutation surface in the package).

---

## [8] — Generated (OpenAPI-derived schema corpus)

`Generated` is the machine-generated module covering the full OpenAI REST surface: 1236 exported `Schema.Class` / `Schema.Literals` owners plus the `make` Client factory, the `Client` interface, and the `ClientError` rail (constructor + shape). It is not enumerated here — planning pages reference it through the named anchors the owner modules expose. The load-bearing anchors (verbatim names):

```ts
// model-id enums (drive OpenAiLanguageModel.Model / OpenAiEmbeddingModel.Model)
export declare class ChatModel                        // Schema.Literals — chat-completions model ids
export declare class ModelIdsResponsesEnum            // Schema.Literals — Responses-API model ids
export declare class CreateEmbeddingRequestModelEnum  // embedding model ids
export declare class ReasoningEffort                  // reasoning-effort enum
export declare class ImageDetail                      // "high" | "low" | "auto"

// request/response shapes consumed by OpenAiClient.Service
export declare class CreateResponse                   // Responses-API request (Config.Service derives from its Encoded, minus input/tools/tool_choice/stream/text)
export declare class Response                         // Responses-API response object
export declare class OutputItem                       // union of output-item kinds (tool calls, messages, reasoning, ...)
export declare class CreateChatCompletionRequest      // chat-completions request (StreamCompletionRequest = Omit<…, "stream">)
export declare class CreateEmbeddingRequest
export declare class CreateEmbeddingResponse

// tool-arg sub-schemas referenced by OpenAiTool
export declare class RankingOptions
export declare class Filters
export declare class ApproximateLocation
export declare class SearchContextSize
export declare class WebSearchToolSearchContextSize
export declare class WebSearchToolCallStatus
export declare class WebSearchActionSearch
export declare class WebSearchActionOpenPage
export declare class WebSearchActionFind
export declare class CodeInterpreterOutputLogs
export declare class CodeInterpreterOutputImage
export declare class Annotation                       // union: FileCitationBody | UrlCitationBody | ContainerFileCitationBody | FilePath
export declare class OutputTextContent
export declare class RefusalContent
export declare class ReasoningTextContent
```

### Generated.Client

A 219-method REST client interface covering every OpenAI endpoint, plus a leading `readonly httpClient: HttpClient.HttpClient` handle (unquoted; not counted among the 219 `operationId` methods). Each endpoint method returns `Effect.Effect<typeof <ResponseSchema>.Type, HttpClientError.HttpClientError | ParseError>`. Spelling convention is the OpenAPI `operationId` (camelCase string key). Representative anchors:

```ts
export interface Client {
  readonly httpClient: HttpClient.HttpClient
  readonly "listAssistants": (options?: typeof ListAssistantsParams.Encoded | undefined) => Effect.Effect<typeof ListAssistantsResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "createAssistant": (options: typeof CreateAssistantRequest.Encoded) => Effect.Effect<typeof AssistantObject.Type, HttpClientError.HttpClientError | ParseError>
  readonly "getAssistant": (assistantId: string) => Effect.Effect<typeof AssistantObject.Type, HttpClientError.HttpClientError | ParseError>
  readonly "modifyAssistant": (assistantId: string, options: typeof ModifyAssistantRequest.Encoded) => Effect.Effect<typeof AssistantObject.Type, HttpClientError.HttpClientError | ParseError>
  readonly "deleteAssistant": (assistantId: string) => Effect.Effect<...>
  readonly "createSpeech": (options: typeof CreateSpeechRequest.Encoded) => Effect.Effect<...>
  readonly "createTranscription": (options: ...) => Effect.Effect<...>
  readonly "createTranslation": (options: ...) => Effect.Effect<...>
  // ...211 further endpoint methods, keyed by OpenAPI operationId
}

// Generated module also exports the low-level Client factory + error rail:
export declare const make: (
  httpClient: HttpClient.HttpClient,
  options?: { readonly transformClient?: ((client: HttpClient.HttpClient) => Effect.Effect<HttpClient.HttpClient>) | undefined }
) => Client
export interface ClientError<Tag extends string, E> {
  readonly _tag: Tag
  readonly request: HttpClientRequest.HttpClientRequest
  readonly response: HttpClientResponse.HttpClientResponse
  readonly cause: E
}
export declare const ClientError: <Tag extends string, E>(tag: Tag, cause: E, response: HttpClientResponse.HttpClientResponse) => ClientError<Tag, E>
```

Planning pages reach the REST surface through `OpenAiClient.Service.client.<operationId>(...)`, not by importing individual `Generated` schemas. `OpenAiClient.make`/`layer*` build the `Generated.Client` internally via `Generated.make` — planning code does not call `Generated.make` directly. The high-level `createResponse` / `createResponseStream` / `createEmbedding` methods on `OpenAiClient.Service` are the curated entry points; raw `client.*` is the escape hatch for endpoints without a curated wrapper.
