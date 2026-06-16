# [API_CATALOGUE] effect-ai-anthropic

Grounded from installed `node_modules` type declarations (`@effect/ai-anthropic` `0.26.0`). Covers the load-bearing public surface the TS planning pages consume for an Anthropic-backed `@effect/ai` provider: the `AnthropicClient` service and its two layer constructors, the `AnthropicLanguageModel` model/layer family with its provider-options and provider-metadata module augmentations, the `AnthropicConfig` per-effect HTTP-transform overlay, the `AnthropicTokenizer`, the provider-defined `AnthropicTool` family, and the `Generated` OpenAPI codec module (request/response wire schemas plus the typed `Client` endpoint surface). The package surfaces six modules through `index.d.ts`, each re-exported under its own namespace: `AnthropicClient`, `AnthropicConfig`, `AnthropicLanguageModel`, `AnthropicTokenizer`, `AnthropicTool`, `Generated`. Service tags carry the identifiers `@effect/ai-anthropic/AnthropicClient`, `@effect/ai-anthropic/AnthropicConfig`, and `@effect/ai-anthropic/AnthropicLanguageModel/Config`. Cross-package owners (`Effect`, `Layer`, `Schema`, `Stream`, `Config`, `Redacted`, `HttpClient`, `@effect/ai` `LanguageModel`/`Tokenizer`/`Model`/`Tool`/`AiError`) resolve against `api-effect-core.md` and the `@effect/ai` core surface.

---

## [1] — AnthropicClient

Module `@effect/ai-anthropic/AnthropicClient`. The provider HTTP service plus the streaming-event union and the two `Layer` constructors. `AnthropicClient` is a `Context.TagClass` whose service value is `Service`.

### Service tag

```ts
// Context.TagClass<AnthropicClient, "@effect/ai-anthropic/AnthropicClient", Service>
export declare class AnthropicClient extends AnthropicClient_base {}
```

### Service

```ts
export interface Service {
  readonly client: Generated.Client
  readonly streamRequest: <A, I, R>(
    request: HttpClientRequest.HttpClientRequest,
    schema: Schema.Schema<A, I, R>
  ) => Stream.Stream<A, AiError.AiError, R>
  readonly createMessage: (options: {
    readonly params?: typeof Generated.BetaMessagesPostParams.Encoded | undefined
    readonly payload: typeof Generated.BetaCreateMessageParams.Encoded
  }) => Effect.Effect<Generated.BetaMessage, AiError.AiError>
  readonly createMessageStream: (options: {
    readonly params?: typeof Generated.BetaMessagesPostParams.Encoded | undefined
    readonly payload: Omit<typeof Generated.BetaCreateMessageParams.Encoded, "stream">
  }) => Stream.Stream<MessageStreamEvent, AiError.AiError>
}
```

`client` is the full generated endpoint surface (section [6]); `createMessage`/`createMessageStream` are the high-level beta-message entrypoints; `streamRequest` decodes an arbitrary SSE response against a `Schema` for provider endpoints not covered by the high-level methods.

### Constructor

```ts
export declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined           // defaults "https://api.anthropic.com"
  readonly anthropicVersion?: string | undefined  // defaults "2023-06-01"
  readonly organizationId?: Redacted.Redacted | undefined
  readonly projectId?: Redacted.Redacted | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>
```

The `apiKey` is sent in the `x-api-key` header; `Redacted` wrapping keeps it out of logs.

### MessageStreamEvent

```ts
export declare const MessageStreamEvent: Schema.Union<[
  typeof PingEvent, typeof ErrorEvent, typeof MessageStartEvent, typeof MessageDeltaEvent,
  typeof MessageStopEvent, typeof ContentBlockStartEvent, typeof ContentBlockDeltaEvent, typeof ContentBlockStopEvent
]>
export type MessageStreamEvent = typeof MessageStreamEvent.Type
```

Eight `Schema.Class` event members, each discriminated by a `type` literal:

```ts
export declare class PingEvent          { readonly type: "ping" }
export declare class ErrorEvent         { readonly type: "error"; readonly error: { readonly type: "invalid_request_error" | "authentication_error" | "permission_error" | "not_found_error" | "request_too_large" | "rate_limit_error" | "api_error" | "overloaded_error"; readonly message: string } }
export declare class MessageStartEvent  { readonly type: "message_start"; readonly message: Generated.BetaMessage }
export declare class MessageStopEvent   { readonly type: "message_stop" }
export declare class MessageDeltaEvent  { readonly type: "message_delta"; readonly delta: MessageDelta; readonly usage: MessageDeltaUsage }
export declare class ContentBlockStartEvent { readonly type: "content_block_start"; readonly index: number; readonly content_block: Generated.BetaContentBlock }
export declare class ContentBlockDeltaEvent { readonly type: "content_block_delta"; readonly index: number; readonly delta: CitationsDelta | InputJsonContentBlockDelta | SignatureContentBlockDelta | TextContentBlockDelta | ThinkingContentBlockDelta }
export declare class ContentBlockStopEvent  { readonly type: "content_block_stop"; readonly index: number }
```

Supporting delta/usage schema classes (all `Schema.Class`):

```ts
export declare class MessageDelta      { readonly stop_reason?: "end_turn" | "max_tokens" | "stop_sequence" | "tool_use" | "pause_turn" | "refusal" | null | undefined; readonly stop_sequence?: string | null | undefined }
export declare class ServerToolUsage   { readonly web_search_requests?: number | null | undefined }
export declare class MessageDeltaUsage { readonly input_tokens?: number | null; readonly output_tokens?: number | null; readonly cache_creation_input_tokens?: number | null; readonly cache_read_input_tokens?: number | null; readonly server_tool_use?: ServerToolUsage | null }
export declare class CitationsDelta             { readonly type: "citations_delta"; readonly citation: Generated.BetaResponseCharLocationCitation | Generated.BetaResponsePageLocationCitation | Generated.BetaResponseContentBlockLocationCitation | Generated.BetaResponseWebSearchResultLocationCitation | Generated.BetaResponseSearchResultLocationCitation }
export declare class InputJsonContentBlockDelta { readonly type: "input_json_delta"; readonly partial_json: string }
export declare class SignatureContentBlockDelta { readonly type: "signature_delta"; readonly signature: string }
export declare class TextContentBlockDelta      { readonly type: "text_delta"; readonly text: string }
export declare class ThinkingContentBlockDelta  { readonly type: "thinking_delta"; readonly thinking: string }
```

### Layers

```ts
export declare const layer: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly anthropicVersion?: string | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<AnthropicClient, never, HttpClient.HttpClient>

export declare const layerConfig: (options: {
  readonly apiKey?: Config.Config<Redacted.Redacted | undefined> | undefined
  readonly apiUrl?: Config.Config<string | undefined> | undefined
  readonly anthropicVersion?: Config.Config<string | undefined> | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<AnthropicClient, ConfigError, HttpClient.HttpClient>
```

`layer` takes literal values; `layerConfig` takes `Config` cells (resolving from environment) and adds `ConfigError` to the layer error channel. Both require an `HttpClient.HttpClient` (browser binding is `FetchHttpClient.layer`).

---

## [2] — AnthropicLanguageModel

Module `@effect/ai-anthropic/AnthropicLanguageModel`. Builds an `@effect/ai` `LanguageModel` over an `AnthropicClient`. Carries the model id literal type, the per-request `Config` tag, the provider-options and provider-metadata module augmentations, and the tool-preparation helper.

### Model id

```ts
export type Model = typeof Generated.Model.Encoded
// literal union (Generated.Model):
// "claude-3-7-sonnet-latest" | "claude-3-7-sonnet-20250219" | "claude-3-5-haiku-latest" | "claude-3-5-haiku-20241022"
// | "claude-haiku-4-5" | "claude-haiku-4-5-20251001" | "claude-sonnet-4-20250514" | "claude-sonnet-4-0"
// | "claude-4-sonnet-20250514" | "claude-sonnet-4-5" | "claude-sonnet-4-5-20250929" | "claude-3-5-sonnet-latest"
// | "claude-3-5-sonnet-20241022" | "claude-3-5-sonnet-20240620" | "claude-opus-4-0" | "claude-opus-4-20250514"
// | "claude-4-opus-20250514" | "claude-opus-4-1-20250805" | "claude-3-opus-latest" | "claude-3-opus-20240229" | "claude-3-haiku-20240307"
```

Entrypoints accept `(string & {}) | Model` — the open-string escape hatch keeps the literal union as autocomplete without rejecting newer ids.

### Config tag

```ts
// Context.TagClass<Config, "@effect/ai-anthropic/AnthropicLanguageModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<typeof Config.Service | undefined>
}
export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof Generated.CreateMessageParams.Encoded, "messages" | "tools" | "tool_choice" | "stream">>> {
    readonly disableParallelToolCalls?: boolean
  }
}
```

`Config.Service` is the per-request override carrier: every `CreateMessageParams` field except `messages`/`tools`/`tool_choice`/`stream` (owned by the SDK request builder) made partial, plus `disableParallelToolCalls`.

### Ai-model and layer family

```ts
export declare const model: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">)
  => AiModel.Model<"anthropic", LanguageModel.LanguageModel, AnthropicClient>
export declare const modelWithTokenizer: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">)
  => AiModel.Model<"anthropic", LanguageModel.LanguageModel | Tokenizer.Tokenizer, AnthropicClient>
export declare const make: (options: { readonly model: (string & {}) | Model; readonly config?: Omit<Config.Service, "model"> })
  => Effect.Effect<LanguageModel.Service, never, AnthropicClient>
export declare const layer: (options: { readonly model: (string & {}) | Model; readonly config?: Omit<Config.Service, "model"> })
  => Layer.Layer<LanguageModel.LanguageModel, never, AnthropicClient>
export declare const layerWithTokenizer: (options: { readonly model: (string & {}) | Model; readonly config?: Omit<Config.Service, "model"> })
  => Layer.Layer<LanguageModel.LanguageModel | Tokenizer.Tokenizer, never, AnthropicClient>
export declare const withConfigOverride: {
  (config: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, config: Config.Service): Effect.Effect<A, E, R>
}
```

`model`/`modelWithTokenizer` produce a reusable `AiModel.Model` (provider id `"anthropic"`); `layer`/`layerWithTokenizer` produce the service `Layer` directly; `withConfigOverride` is the dual (data-first / data-last) per-effect config scope.

### Provider reasoning info

```ts
export type AnthropicReasoningInfo =
  | { readonly type: "thinking"; readonly signature: typeof Generated.ResponseThinkingBlock.fields.thinking.Encoded }
  | { readonly type: "redacted_thinking"; readonly redactedData: typeof Generated.RequestRedactedThinkingBlock.fields.data.Encoded }
```

### Module augmentations

The module augments `@effect/ai/Prompt` and `@effect/ai/Response` to attach the optional `anthropic` provider-options/metadata key:

- `@effect/ai/Prompt`: `SystemMessageOptions`, `UserMessageOptions`, `AssistantMessageOptions`, `ToolMessageOptions`, `TextPartOptions`, `ToolCallPartOptions`, `ToolResultPartOptions` each gain `anthropic?: { cacheControl?: typeof Generated.CacheControlEphemeral.Encoded }`. `ReasoningPartOptions` gains `anthropic?: Simplify<AnthropicReasoningInfo & { cacheControl?: ... }>`. `FilePartOptions` additionally gains `citations?: typeof Generated.RequestCitationsConfig.Encoded`, `documentTitle?: string`, `documentContext?: string`.
- `@effect/ai/Response`: `ReasoningPartMetadata`, `ReasoningStartPartMetadata`, `ReasoningDeltaPartMetadata` gain `anthropic?: AnthropicReasoningInfo`. `FinishPartMetadata` gains `anthropic?: { usage?: Generated.BetaUsage; stopSequence?: string }`. `DocumentSourcePartMetadata` gains a `char_location`/`page_location` citation shape; `UrlSourcePartMetadata` gains a `url` source shape with `citedText`/`encryptedIndex`.

### Tool preparation

```ts
export type AnthropicTools =
  | typeof Generated.BetaTool.Encoded
  | typeof Generated.BetaBashTool20241022.Encoded | typeof Generated.BetaBashTool20250124.Encoded
  | typeof Generated.BetaComputerUseTool20241022.Encoded | typeof Generated.BetaComputerUseTool20250124.Encoded
  | typeof Generated.BetaTextEditor20241022.Encoded | typeof Generated.BetaTextEditor20250124.Encoded
  | typeof Generated.BetaTextEditor20250429.Encoded | typeof Generated.BetaTextEditor20250728.Encoded

export declare const prepareTools: (options: LanguageModel.ProviderOptions, config: Config.Service)
  => Effect.Effect<{
    readonly betas: ReadonlySet<string>
    readonly tools: ReadonlyArray<AnthropicTools> | undefined
    readonly toolChoice: typeof Generated.BetaToolChoice.Encoded | undefined
  }, AiError.AiError>
```

`prepareTools` is exposed for downstream providers (e.g. Amazon Bedrock) reusing Anthropic models; the SDK request builder calls it internally.

---

## [3] — AnthropicConfig

Module `@effect/ai-anthropic/AnthropicConfig`. The per-effect HTTP-client transform overlay, distinct from the layer-construction `transformClient`.

```ts
// Context.TagClass<AnthropicConfig, "@effect/ai-anthropic/AnthropicConfig", AnthropicConfig.Service>
export declare class AnthropicConfig extends AnthropicConfig_base {
  static readonly getOrUndefined: Effect.Effect<typeof AnthropicConfig.Service | undefined>
}
export declare namespace AnthropicConfig {
  interface Service { readonly transformClient?: (client: HttpClient) => HttpClient }
}
export declare const withClientTransform: {
  (transform: (client: HttpClient) => HttpClient): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, transform: (client: HttpClient) => HttpClient): Effect.Effect<A, E, R>
}
```

`withClientTransform` installs a request-scoped HTTP transform (logging, retry, metrics) on the effects it wraps, dual data-first/data-last.

---

## [4] — AnthropicTokenizer

Module `@effect/ai-anthropic/AnthropicTokenizer`. The `@effect/ai` `Tokenizer` binding.

```ts
export declare const make: Tokenizer.Service
export declare const layer: Layer.Layer<Tokenizer.Tokenizer>
```

`layer` provides a standalone `Tokenizer`; `layerWithTokenizer`/`modelWithTokenizer` in section [2] fold this in alongside the language model.

---

## [5] — AnthropicTool

Module `@effect/ai-anthropic/AnthropicTool`. The provider-defined tool family, each a constructor returning an `@effect/ai` `Tool.ProviderDefined`. Versioned variants are date-suffixed; the closed-form union schema is `ProviderDefinedTools`.

```ts
export declare const ProviderDefinedTools: Schema.Union<[
  typeof Generated.BetaBashTool20241022, typeof Generated.BetaBashTool20250124,
  typeof Generated.BetaCodeExecutionTool20250522,
  typeof Generated.BetaComputerUseTool20241022, typeof Generated.BetaComputerUseTool20250124,
  typeof Generated.BetaTextEditor20241022, typeof Generated.BetaTextEditor20250124,
  typeof Generated.BetaTextEditor20250429, typeof Generated.BetaTextEditor20250728,
  typeof Generated.BetaWebSearchTool20250305
]>
export type ProviderDefinedTools = typeof ProviderDefinedTools.Type

export declare const Coordinate: Schema.Tuple2<typeof Schema.Number, typeof Schema.Number>
export declare const getProviderDefinedToolName: (name: string) => string | undefined
```

Each tool constructor is generic in `Mode extends Tool.FailureMode | undefined`; the `failureMode` resolves `Mode extends undefined ? "error" : Mode`. Provider-defined tool ids are `"AnthropicBash"`, `"AnthropicCodeExecution"`, `"AnthropicComputerUse"`, `"AnthropicTextEditor"`, `"AnthropicWebSearch"`. `getProviderDefinedToolName` maps a wire tool name (e.g. `"web_search"`) to its toolkit id (`"AnthropicWebSearch"`).

```ts
export declare const Bash_20241022: <Mode extends Tool.FailureMode | undefined = undefined>(args: { readonly failureMode?: Mode }) => Tool.ProviderDefined<"AnthropicBash", { args: Schema.Struct<{}>; parameters: Schema.Struct<{ command: typeof Schema.NonEmptyString; restart: Schema.optional<typeof Schema.Boolean> }>; success: typeof Schema.String; failure: typeof Schema.Never; failureMode: Mode extends undefined ? "error" : Mode }, true>
export declare const Bash_20250124: <Mode ...>(args: { readonly failureMode?: Mode }) => Tool.ProviderDefined<"AnthropicBash", { /* identical parameter shape to Bash_20241022 */ }, true>

export declare const CodeExecution_20250522: <Mode ...>(args: { readonly cache_control?: { readonly type: "ephemeral"; readonly ttl?: "5m" | "1h" | null } | null }) => Tool.ProviderDefined<"AnthropicCodeExecution", { args: Schema.Struct<{ cache_control: Schema.optionalWith<typeof Generated.BetaCacheControlEphemeral, { nullable: true }> }>; parameters: Tool.EmptyParams; success: typeof Generated.BetaResponseCodeExecutionResultBlock; failure: typeof Generated.BetaResponseCodeExecutionToolResultError; failureMode: Mode extends undefined ? "error" : Mode }, false>
export declare const CodeExecution_20250825: <Mode ...>(args: { readonly cache_control?: ... }) => Tool.ProviderDefined<"AnthropicCodeExecution", { /* success/failure unions over Beta bash/text-editor code-execution result + error blocks */ }, false>

export declare const ComputerUse_20241022: <Mode ...>(args: { readonly display_height_px: number; readonly display_width_px: number; readonly cache_control?: ...; readonly display_number?: number | null; readonly failureMode?: Mode }) => Tool.ProviderDefined<"AnthropicComputerUse", { parameters: Schema.Struct<{ action: Schema.Literal<["screenshot", "left_click", "type", "key", "mouse_move"]>; coordinate: ...; text: ... }>; success: typeof Schema.String; failure: typeof Schema.Never; ... }, true>
export declare const ComputerUse_20250124: <Mode ...>(args: { /* same display args */ }) => Tool.ProviderDefined<"AnthropicComputerUse", { parameters: Schema.Struct<{ action: Schema.Literal<["screenshot", "left_click", "type", "key", "mouse_move", "scroll", "left_click_drag", "middle_click", "right_click", "double_click", "triple_click", "left_mouse_down", "left_mouse_up", "hold_key", "wait"]>; coordinate; start_coordinate; text; scroll_direction: Schema.Literal<["up", "down", "left", "right"]>; scroll_amount; duration }>; ... }, true>

export declare const TextEditor_20241022: <Mode ...>(args: { readonly failureMode?: Mode }) => Tool.ProviderDefined<"AnthropicTextEditor", { parameters: Schema.Struct<{ command: Schema.Literal<["view", "create", "str_replace", "insert", "undo_edit"]>; path; file_text; insert_line; new_str; old_str; view_range }>; success: typeof Schema.Void; failure: typeof Schema.Never; ... }, true>
export declare const TextEditor_20250124: <Mode ...> // command literal ["view","create","str_replace","insert","undo_edit"]
export declare const TextEditor_20250429: <Mode ...> // command literal ["view","create","str_replace","insert"]
export declare const TextEditor_20250728: <Mode ...> // command literal ["view","create","str_replace","insert"]

export declare const WebSearch_20250305: <Mode ...>(args: { readonly cache_control?: ...; readonly allowed_domains?: readonly string[] | null; readonly blocked_domains?: readonly string[] | null; readonly max_uses?: number | null; readonly user_location?: { readonly type: "approximate"; readonly city?: string | null; readonly country?: string | null; readonly region?: string | null; readonly timezone?: string | null } | null }) => Tool.ProviderDefined<"AnthropicWebSearch", { parameters: Tool.EmptyParams; success: Schema.Array$<typeof Generated.RequestWebSearchResultBlock>; failure: typeof Generated.ResponseWebSearchToolResultError; ... }, false>
```

---

## [6] — Generated

Module `@effect/ai-anthropic/Generated`. The OpenAPI-derived codec module: 341 exported owners (`Schema.Class` wire schemas, `Schema.Literal` enums, `Schema.Union` discriminated families) plus the typed `Client` endpoint interface and its constructors. The surface is doubled: a stable family (`Request*`/`Response*`/`Tool`/`Message`/`Usage`/`CreateMessageParams`/error classes/`Complete*` legacy/`Files`+`Skills`+`MessageBatches` endpoint params) and a `Beta*` mirror carrying the beta-only blocks (MCP tool blocks, web-fetch, code-execution result blocks, file image/document sources, container/skill params). The planning pages compose this module by `typeof X.Encoded` (wire) and `typeof X.Type` / `X` (decoded); the high-level `AnthropicClient` methods use the `Beta*` request/response owners, while the low-level `client` field exposes every endpoint below.

### Client endpoint surface

```ts
export declare const make: (httpClient: HttpClient.HttpClient, options?: {
  readonly transformClient?: ((client: HttpClient.HttpClient) => Effect.Effect<HttpClient.HttpClient>) | undefined
}) => Client

export interface Client {
  readonly httpClient: HttpClient.HttpClient
  readonly messagesPost: (options: { readonly params?: typeof MessagesPostParams.Encoded; readonly payload: typeof CreateMessageParams.Encoded }) => Effect.Effect<typeof Message.Type, HttpClientError.HttpClientError | ParseError | ClientError<"ErrorResponse", typeof ErrorResponse.Type>>
  readonly completePost: (options: { readonly params?: typeof CompletePostParams.Encoded; readonly payload: typeof CompletionRequest.Encoded }) => Effect.Effect<typeof CompletionResponse.Type, ...>
  readonly modelsList: (options?: typeof ModelsListParams.Encoded) => Effect.Effect<typeof ListResponseModelInfo.Type, ...>
  readonly modelsGet: (modelId: string, options?: typeof ModelsGetParams.Encoded) => Effect.Effect<typeof ModelInfo.Type, ...>
  readonly messageBatchesList: (options?: typeof MessageBatchesListParams.Encoded) => Effect.Effect<typeof ListResponseMessageBatch.Type, ...>
  readonly messageBatchesPost: (options: { readonly params?: ...; readonly payload: typeof CreateMessageBatchParams.Encoded }) => Effect.Effect<typeof MessageBatch.Type, ...>
  readonly messageBatchesRetrieve: (messageBatchId: string, options?: ...) => Effect.Effect<typeof MessageBatch.Type, ...>
  readonly messageBatchesDelete: (messageBatchId: string, options?: ...) => Effect.Effect<typeof DeleteMessageBatchResponse.Type, ...>
  readonly messageBatchesCancel: (messageBatchId: string, options?: ...) => Effect.Effect<typeof MessageBatch.Type, ...>
  readonly messageBatchesResults: (messageBatchId: string, options?: ...) => Effect.Effect<void, ...>
  readonly messagesCountTokensPost: (options: { readonly params?: ...; readonly payload: typeof CountMessageTokensParams.Encoded }) => Effect.Effect<typeof CountMessageTokensResponse.Type, ...>
  readonly listFilesV1FilesGet: (options?: ...) => Effect.Effect<typeof FileListResponse.Type, ...>
  readonly uploadFileV1FilesPost: (options: { readonly params?: ...; readonly payload: typeof UploadFileV1FilesPostRequest.Encoded }) => Effect.Effect<typeof FileMetadataSchema.Type, ...>
  readonly getFileMetadataV1FilesFileIdGet: (fileId: string, options?: ...) => Effect.Effect<typeof FileMetadataSchema.Type, ...>
  readonly deleteFileV1FilesFileIdDelete: (fileId: string, options?: ...) => Effect.Effect<typeof FileDeleteResponse.Type, ...>
  readonly downloadFileV1FilesFileIdContentGet: (fileId: string, options?: ...) => Effect.Effect<void, HttpClientError.HttpClientError | ParseError>
  readonly listSkillsV1SkillsGet: (options?: ...) => Effect.Effect<typeof ListSkillsResponse.Type, ...>
  readonly createSkillV1SkillsPost: (options: { readonly params?: ...; readonly payload: typeof BodyCreateSkillV1SkillsPost.Encoded }) => Effect.Effect<typeof CreateSkillResponse.Type, ...>
  readonly getSkillV1SkillsSkillIdGet: (skillId: string, options?: ...) => Effect.Effect<typeof GetSkillResponse.Type, ...>
  readonly deleteSkillV1SkillsSkillIdDelete: (skillId: string, options?: ...) => Effect.Effect<typeof DeleteSkillResponse.Type, ...>
  readonly listSkillVersionsV1SkillsSkillIdVersionsGet: (skillId: string, options?: ...) => Effect.Effect<typeof ListSkillVersionsResponse.Type, ...>
  readonly createSkillVersionV1SkillsSkillIdVersionsPost: (skillId: string, options: { readonly params?: ...; readonly payload: typeof BodyCreateSkillVersionV1SkillsSkillIdVersionsPost.Encoded }) => Effect.Effect<typeof CreateSkillVersionResponse.Type, ...>
  readonly getSkillVersionV1SkillsSkillIdVersionsVersionGet: (skillId: string, version: string, options?: ...) => Effect.Effect<typeof GetSkillVersionResponse.Type, ...>
  readonly deleteSkillVersionV1SkillsSkillIdVersionsVersionDelete: (skillId: string, version: string, options?: ...) => Effect.Effect<typeof DeleteSkillVersionResponse.Type, ...>
  // Beta mirror: betaMessagesPost, betaModelsList, betaModelsGet, betaMessageBatchesList/Post/Retrieve/Delete/Cancel/Results,
  //   betaMessagesCountTokensPost, betaListFilesV1FilesGet, betaUploadFileV1FilesPost, betaGetFileMetadataV1FilesFileIdGet,
  //   betaDeleteFileV1FilesFileIdDelete, betaDownloadFileV1FilesFileIdContentGet, betaListSkillsV1SkillsGet,
  //   betaCreateSkillV1SkillsPost, betaGetSkillV1SkillsSkillIdGet, betaDeleteSkillV1SkillsSkillIdDelete,
  //   betaListSkillVersions..., betaCreateSkillVersion..., betaGetSkillVersion..., betaDeleteSkillVersion...
  //   — each parameterized/returning the Beta* params/response owners and failing with ClientError<"BetaErrorResponse", typeof BetaErrorResponse.Type>
}

export interface ClientError<Tag extends string, E> {
  readonly _tag: Tag
  readonly request: HttpClientRequest.HttpClientRequest
  readonly response: HttpClientResponse.HttpClientResponse
  readonly cause: E
}
export declare const ClientError: <Tag extends string, E>(tag: Tag, cause: E, response: HttpClientResponse.HttpClientResponse) => ClientError<Tag, E>
```

Stable endpoints fail with `ClientError<"ErrorResponse", typeof ErrorResponse.Type>`; beta endpoints fail with `ClientError<"BetaErrorResponse", typeof BetaErrorResponse.Type>`. Both also carry `HttpClientError.HttpClientError | ParseError`.

### Principal request/response owners

```ts
export declare class Model { /* literal union, see section [2] */ }

export declare class CreateMessageParams { // decoded Type
  readonly model: string
  readonly messages: readonly InputMessage[]
  readonly max_tokens: number
  readonly metadata?: Metadata
  readonly service_tier?: "auto" | "standard_only"
  readonly stop_sequences?: readonly string[]
  readonly stream?: boolean
  readonly system?: string | readonly RequestTextBlock[]
  readonly temperature?: number
  readonly thinking?: ThinkingConfigEnabled | ThinkingConfigDisabled
  readonly tool_choice?: ToolChoiceAuto | ToolChoiceAny | ToolChoiceTool | ToolChoiceNone
  readonly tools?: readonly (Tool | BashTool20250124 | TextEditor20250124 | TextEditor20250429 | TextEditor20250728 | WebSearchTool20250305)[]
  readonly top_k?: number
  readonly top_p?: number
}

export declare class Message {
  readonly id: string
  readonly type: "message"
  readonly role: "assistant"
  readonly content: readonly (typeof ContentBlock.Type)[]
  // model, stop_reason (StopReason | null), stop_sequence, usage (Usage)
}

export declare class ContentBlock extends Schema.Union<[
  typeof ResponseTextBlock, typeof ResponseThinkingBlock, typeof ResponseRedactedThinkingBlock,
  typeof ResponseToolUseBlock, typeof ResponseServerToolUseBlock, typeof ResponseWebSearchToolResultBlock
]> {}

export declare class StopReason extends Schema.Literal<["end_turn", "max_tokens", "stop_sequence", "tool_use", "pause_turn", "refusal"]> {}
// Beta mirror BetaStopReason adds one member: ["...", "refusal", "model_context_window_exceeded"]; the high-level createMessage/createMessageStream return BetaMessage, whose stop_reason is BetaStopReason.

export declare class Usage {
  readonly input_tokens: number
  readonly output_tokens: number
  readonly cache_creation?: CacheCreation | null
  readonly cache_creation_input_tokens?: number | null
  readonly cache_read_input_tokens?: number | null
  readonly server_tool_use?: ServerToolUsage | null
  readonly service_tier?: "standard" | "priority" | "batch" | null
}
export declare class CacheCreation { readonly ephemeral_1h_input_tokens?: number; readonly ephemeral_5m_input_tokens?: number }
export declare class ServerToolUsage { readonly web_search_requests?: number }

export declare class ToolChoice extends Schema.Union<[typeof ToolChoiceAuto, typeof ToolChoiceAny, typeof ToolChoiceTool, typeof ToolChoiceNone]> {}
export declare class CacheControlEphemeral { readonly type: "ephemeral"; readonly ttl?: "5m" | "1h" }
export declare class CacheControlEphemeralTtl extends Schema.Literal<["5m", "1h"]> {}
```

### Wire-error class family

```ts
// each Schema.Class with { type: <literal>, message: string } (and request_id where present)
export declare class ErrorResponse { /* request_id, type: "error", error: <union of below> */ }
export declare class InvalidRequestError  { readonly type: "invalid_request_error"; readonly message: string }
export declare class AuthenticationError  { readonly type: "authentication_error"; readonly message: string }
export declare class BillingError         { readonly type: "billing_error"; readonly message: string }
export declare class PermissionError      { readonly type: "permission_error"; readonly message: string }
export declare class NotFoundError        { readonly type: "not_found_error"; readonly message: string }
export declare class RateLimitError       { readonly type: "rate_limit_error"; readonly message: string }
export declare class GatewayTimeoutError  { readonly type: "timeout_error"; readonly message: string }
export declare class APIError             { readonly type: "api_error"; readonly message: string }
export declare class OverloadedError      { readonly type: "overloaded_error"; readonly message: string }
```

### Owner inventory (load-bearing names, exact spelling)

The remaining `Generated` owners group by axis; planning pages reference these by exact name. Each `Request*` has a `Beta*Request*` mirror and most `Response*` have a `Beta*Response*` mirror.

- Input message / content: `InputMessage`, `InputMessageRole`, `InputContentBlock`, `Metadata`.
- Request content blocks: `RequestTextBlock`, `RequestImageBlock`, `RequestDocumentBlock`, `RequestSearchResultBlock`, `RequestThinkingBlock`, `RequestRedactedThinkingBlock`, `RequestToolUseBlock`, `RequestToolResultBlock`, `RequestServerToolUseBlock`, `RequestWebSearchResultBlock`, `RequestWebSearchToolResultBlock`, `RequestWebSearchToolResultError`, `WebSearchToolResultErrorCode`.
- Sources: `Base64ImageSource`, `Base64ImageSourceMediaType`, `URLImageSource`, `Base64PDFSource`, `URLPDFSource`, `PlainTextSource`, `ContentBlockSource`.
- Citations (request + response): `RequestCharLocationCitation`, `RequestPageLocationCitation`, `RequestContentBlockLocationCitation`, `RequestWebSearchResultLocationCitation`, `RequestSearchResultLocationCitation`, `RequestCitationsConfig`, `ResponseCharLocationCitation`, `ResponsePageLocationCitation`, `ResponseContentBlockLocationCitation`, `ResponseWebSearchResultLocationCitation`, `ResponseSearchResultLocationCitation`.
- Response blocks: `ResponseTextBlock`, `ResponseThinkingBlock`, `ResponseRedactedThinkingBlock`, `ResponseToolUseBlock`, `ResponseServerToolUseBlock`, `ResponseWebSearchResultBlock`, `ResponseWebSearchToolResultBlock`, `ResponseWebSearchToolResultError`.
- Tools / tool-choice: `Tool`, `InputSchema`, `BashTool20250124`, `TextEditor20250124`, `TextEditor20250429`, `TextEditor20250728`, `WebSearchTool20250305`, `UserLocation`, `ToolChoiceAuto`, `ToolChoiceAny`, `ToolChoiceTool`, `ToolChoiceNone`.
- Thinking: `ThinkingConfigEnabled`, `ThinkingConfigDisabled`, `ThinkingConfigParam`.
- Top-level params: `MessagesPostParams`, `CreateMessageParamsServiceTier`, `MessagesCountTokensPostParams`, `CountMessageTokensParams`, `CountMessageTokensResponse`.
- Legacy completions: `CompletePostParams`, `CompletionRequest`, `CompletionResponse`.
- Models endpoint: `ModelsListParams`, `ModelsGetParams`, `ModelInfo`, `ListResponseModelInfo`.
- Message batches: `MessageBatchesListParams`, `MessageBatchesPostParams`, `MessageBatchesRetrieveParams`, `MessageBatchesDeleteParams`, `MessageBatchesCancelParams`, `MessageBatchesResultsParams`, `MessageBatchIndividualRequestParams`, `CreateMessageBatchParams`, `MessageBatch`, `MessageBatchProcessingStatus`, `RequestCounts`, `ListResponseMessageBatch`, `DeleteMessageBatchResponse`.
- Files endpoint: `ListFilesV1FilesGetParams`, `UploadFileV1FilesPostParams`, `UploadFileV1FilesPostRequest`, `GetFileMetadataV1FilesFileIdGetParams`, `DeleteFileV1FilesFileIdDeleteParams`, `DownloadFileV1FilesFileIdContentGetParams`, `FileMetadataSchema`, `FileListResponse`, `FileDeleteResponse`.
- Skills endpoint: `ListSkillsV1SkillsGetParams`, `CreateSkillV1SkillsPostParams`, `BodyCreateSkillV1SkillsPost`, `GetSkillV1SkillsSkillIdGetParams`, `DeleteSkillV1SkillsSkillIdDeleteParams`, `Skill`, `ListSkillsResponse`, `CreateSkillResponse`, `GetSkillResponse`, `DeleteSkillResponse`, `ListSkillVersionsV1SkillsSkillIdVersionsGetParams`, `CreateSkillVersionV1SkillsSkillIdVersionsPostParams`, `BodyCreateSkillVersionV1SkillsSkillIdVersionsPost`, `GetSkillVersionV1SkillsSkillIdVersionsVersionGetParams`, `DeleteSkillVersionV1SkillsSkillIdVersionsVersionDeleteParams`, `SkillVersion`, `ListSkillVersionsResponse`, `CreateSkillVersionResponse`, `GetSkillVersionResponse`, `DeleteSkillVersionResponse`.
- Beta-only blocks (no stable counterpart): `BetaFileImageSource`, `BetaFileDocumentSource`, `BetaRequestServerToolUseBlockName`, `BetaRequestWebFetchResultBlock`, `BetaRequestWebFetchToolResultBlock`, `BetaRequestWebFetchToolResultError`, `BetaWebFetchToolResultErrorCode`, `BetaRequestCodeExecutionOutputBlock`, `BetaRequestCodeExecutionResultBlock`, `BetaRequestCodeExecutionToolResultBlock`, `BetaRequestCodeExecutionToolResultError`, `BetaCodeExecutionToolResultErrorCode`, `BetaRequestBashCodeExecutionOutputBlock`, `BetaRequestBashCodeExecutionResultBlock`, `BetaRequestBashCodeExecutionToolResultBlock`, `BetaRequestBashCodeExecutionToolResultError`, `BetaBashCodeExecutionToolResultErrorCode`, `BetaRequestTextEditorCodeExecutionViewResultBlock`, `BetaRequestTextEditorCodeExecutionViewResultBlockFileType`, `BetaRequestTextEditorCodeExecutionCreateResultBlock`, `BetaRequestTextEditorCodeExecutionStrReplaceResultBlock`, `BetaRequestTextEditorCodeExecutionToolResultBlock`, `BetaTextEditorCodeExecutionToolResultErrorCode`, `BetaRequestMCPToolUseBlock`, `BetaRequestMCPToolResultBlock`, `BetaRequestContainerUploadBlock`, `BetaSkillParams`, `BetaSkillParamsType`, `BetaContainerParams`, `BetaInputTokensClearAtLeast`, `BetaInputTokensTrigger`, `BetaToolUsesKeep`, `BetaToolUsesTrigger`, and the `Beta*` mirrors of every stable owner above (`BetaMessage`, `BetaCreateMessageParams`, `BetaMessagesPostParams`, `BetaContentBlock`, `BetaTool`, `BetaToolChoice`, `BetaUsage`, `BetaErrorResponse`, `BetaCacheControlEphemeral`, the `Beta*Tool20*` provider-tool schemas, etc.).
