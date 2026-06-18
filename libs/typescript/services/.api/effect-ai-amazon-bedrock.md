# [API_CATALOGUE] effect-ai-amazon-bedrock

Grounded from installed `node_modules` type declarations (`@effect/ai-amazon-bedrock` `0.16.0`). Covers the load-bearing public surface the TS planning pages consume for an Amazon-Bedrock-backed `@effect/ai` provider over the Bedrock **Converse** / **ConverseStream** API: the `AmazonBedrockClient` service and its three constructors (`make`/`layer`/`layerConfig`), the `AmazonBedrockLanguageModel` model/layer family with its provider-options and provider-metadata module augmentations, the `AmazonBedrockConfig` per-effect HTTP-transform overlay, the provider-defined `AmazonBedrockTool` family (Anthropic-on-Bedrock bash/computer-use/text-editor tools), the `AmazonBedrockSchema` Converse codec module (request/response/stream-event wire schemas plus the full Bedrock guardrail-trace corpus and the `BedrockFoundationModelId` literal catalogue), and the `EventStreamEncoding` AWS event-stream channel parser. The package surfaces six modules through `index.d.ts`, each re-exported under its own namespace: `AmazonBedrockClient`, `AmazonBedrockConfig`, `AmazonBedrockLanguageModel`, `AmazonBedrockSchema`, `AmazonBedrockTool`, `EventStreamEncoding`. Service tags carry the identifiers `@effect/ai-amazon-bedrock/AmazonBedrockClient`, `@effect/ai-amazon-bedrock/AmazonBedrockLanguageModel/Config`, and — note the upstream copy-paste — `@effect/ai-google/AmazonBedrockConfig` for `AmazonBedrockConfig`. Cross-package owners (`Effect`, `Layer`, `Stream`, `Channel`, `Chunk`, `Config`, `Redacted`, `Schema`, `HttpClient`/`HttpClientRequest`/`HttpClientError`, `ParseError`, `@effect/ai` `LanguageModel`/`Model`/`Tool`/`AiError`, and `@effect/ai-anthropic/Generated` for the provider-tool cache-control schema) resolve against `effect.md`, the `@effect/ai` core surface, and `effect-ai-anthropic.md`.

Tier: `node` — the provider issues SigV4-signed HTTPS calls to a regional Bedrock endpoint and requires an `HttpClient.HttpClient`; it carries no browser-only or DOM dependency, so the binding belongs to the node bundle.

---

## [1] — AmazonBedrockClient

Module `@effect/ai-amazon-bedrock/AmazonBedrockClient`. The provider HTTP service plus the three constructors. `AmazonBedrockClient` is a `Context.TagClass` whose service value is `Service`.

### Service tag

```ts
// Context.TagClass<AmazonBedrockClient, "@effect/ai-amazon-bedrock/AmazonBedrockClient", Service>
export declare class AmazonBedrockClient extends AmazonBedrockClient_base {}
```

### Service

```ts
export interface Service {
  readonly client: Client
  readonly streamRequest: <A, I, R>(
    request: HttpClientRequest.HttpClientRequest,
    schema: Schema.Schema<A, I, R>
  ) => Stream.Stream<A, AiError.AiError, R>
  readonly converse: (options: {
    readonly params?: { "anthropic-beta"?: string | undefined } | undefined
    readonly payload: typeof ConverseRequest.Encoded
  }) => Effect.Effect<ConverseResponse, AiError.AiError>
  readonly converseStream: (options: {
    readonly params?: { "anthropic-beta"?: string | undefined } | undefined
    readonly payload: typeof ConverseRequest.Encoded
  }) => Stream.Stream<ConverseResponseStreamEvent, AiError.AiError>
}
```

`client` is the low-level typed endpoint surface (below); `converse`/`converseStream` are the high-level Converse/ConverseStream entrypoints (the `params["anthropic-beta"]` header forwards Anthropic beta opt-ins for Claude-on-Bedrock); `streamRequest` decodes an arbitrary AWS event-stream response against a `Schema` for endpoints not covered by the high-level methods.

### Constructor

```ts
export declare const make: (options: {
  readonly apiUrl?: string | undefined
  readonly accessKeyId: string
  readonly secretAccessKey: Redacted.Redacted<string>
  readonly sessionToken?: Redacted.Redacted<string> | undefined
  readonly region?: string | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>
```

`accessKeyId`/`secretAccessKey`/`sessionToken`/`region` carry SigV4 credentials; `secretAccessKey` and `sessionToken` are `Redacted` to keep them out of logs. `apiUrl` overrides the derived regional Bedrock endpoint.

### Client (low-level endpoint surface)

```ts
export interface Client {
  readonly converse: (options: {
    readonly params?: { "anthropic-beta"?: string | undefined }
    readonly payload: typeof ConverseRequest.Encoded
  }) => Effect.Effect<typeof ConverseResponse.Type, HttpClientError.HttpClientError | ParseError>
}
```

The low-level `client.converse` surfaces the raw HTTP error rail (`HttpClientError.HttpClientError | ParseError`) whereas the service `converse`/`converseStream` map onto `AiError.AiError`.

### Layers

```ts
export declare const layer: (options: {
  readonly apiUrl?: string | undefined
  readonly accessKeyId: string
  readonly secretAccessKey: Redacted.Redacted<string>
  readonly sessionToken?: Redacted.Redacted<string> | undefined
  readonly region?: string | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Layer.Layer<AmazonBedrockClient, never, HttpClient.HttpClient>

export declare const layerConfig: (options: {
  readonly apiUrl?: Config.Config<string> | undefined
  readonly accessKeyId: Config.Config<string>
  readonly secretAccessKey: Config.Config<Redacted.Redacted>
  readonly sessionToken?: Config.Config<Redacted.Redacted> | undefined
  readonly region?: Config.Config<string> | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Layer.Layer<AmazonBedrockClient, ConfigError, HttpClient.HttpClient>
```

`layer` takes literal credentials; `layerConfig` takes `Config` cells (resolving from environment) and adds `ConfigError` to the layer error channel. Both require an `HttpClient.HttpClient` (node binding: `NodeHttpClient.layer`).

---

## [2] — AmazonBedrockLanguageModel

Module `@effect/ai-amazon-bedrock/AmazonBedrockLanguageModel`. Builds an `@effect/ai` `LanguageModel` over an `AmazonBedrockClient`. Carries the model-id literal type, the per-request `Config` tag, the provider-options/provider-metadata module augmentations, and the model/layer family.

### Model id

```ts
export type Model = typeof BedrockFoundationModelId.Encoded   // see AmazonBedrockSchema [4]
```

Entrypoints accept `(string & {}) | Model` — the open-string escape hatch keeps the `BedrockFoundationModelId` literal union as autocomplete without rejecting newer ids or cross-region inference-profile ARNs.

### Config tag

```ts
// Context.TagClass<Config, "@effect/ai-amazon-bedrock/AmazonBedrockLanguageModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<typeof Config.Service | undefined>
}
export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof ConverseRequest.Encoded, "messages" | "system" | "toolConfig">>> {}
}
```

`Config.Service` is the per-request override carrier: every `ConverseRequest` field except `messages`/`system`/`toolConfig` (owned by the SDK request builder) made partial — i.e. `modelId`, `guardrailConfig`, `inferenceConfig`, `performanceConfig`, `promptVariables`, `requestMetadata`, `additionalModelRequestFields`, `additionalModelResponseFieldPaths`.

### Ai-model and layer family

```ts
export declare const model: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">)
  => AiModel.Model<"amazon-bedrock", LanguageModel.LanguageModel, AmazonBedrockClient>
export declare const make: (options: { readonly model: (string & {}) | Model; readonly config?: Omit<Config.Service, "model"> })
  => Effect.Effect<LanguageModel.Service, never, AmazonBedrockClient>
export declare const layer: (options: { readonly model: (string & {}) | Model; readonly config?: Omit<Config.Service, "model"> })
  => Layer.Layer<LanguageModel.LanguageModel, never, AmazonBedrockClient>
export declare const withConfigOverride: {
  (config: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, config: Config.Service): Effect.Effect<A, E, R>
}
```

`model` produces a reusable `AiModel.Model` (provider id `"amazon-bedrock"`); `make`/`layer` produce the service/`Layer` directly; `withConfigOverride` is the dual (data-first / data-last) per-effect config scope. Unlike the Anthropic provider, Bedrock exposes **no** `modelWithTokenizer`/`layerWithTokenizer`/`prepareTools` — there is no `Tokenizer` binding in this package.

### Provider reasoning info

```ts
export type AmazonBedrockReasoningInfo =
  | { readonly type: "thinking"; readonly signature: string }          // encrypted, verifies Bedrock-generated thinking
  | { readonly type: "redacted_thinking"; readonly redactedData: string } // safety-flagged, encrypted
```

### Module augmentations

The module augments `@effect/ai/Prompt` and `@effect/ai/Response` to attach the optional `bedrock` provider-options/metadata key:

- `@effect/ai/Prompt`: `SystemMessageOptions`, `UserMessageOptions`, `AssistantMessageOptions`, `ToolMessageOptions` each gain `bedrock?: { cachePoint?: typeof CachePointBlock.Encoded | undefined } | undefined`. `ReasoningPartOptions` gains `bedrock?: AmazonBedrockReasoningInfo | undefined`.
- `@effect/ai/Response`: `ReasoningPartMetadata` gains `bedrock?: AmazonBedrockReasoningInfo | undefined`. `FinishPartMetadata` gains `bedrock?: { trace?: ConverseTrace | undefined; usage: { cacheWriteInputTokens?: number | undefined } } | undefined`.

---

## [3] — AmazonBedrockConfig

Module `@effect/ai-amazon-bedrock/AmazonBedrockConfig`. The per-effect HTTP-client transform overlay, distinct from the layer-construction `transformClient`.

```ts
// Context.TagClass<AmazonBedrockConfig, "@effect/ai-google/AmazonBedrockConfig", AmazonBedrockConfig.Service>
export declare class AmazonBedrockConfig extends AmazonBedrockConfig_base {
  static readonly getOrUndefined: Effect.Effect<typeof AmazonBedrockConfig.Service | undefined>
}
export declare namespace AmazonBedrockConfig {
  interface Service { readonly transformClient?: (client: HttpClient) => HttpClient }
}
export declare const withClientTransform: {
  (transform: (client: HttpClient) => HttpClient): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, transform: (client: HttpClient) => HttpClient): Effect.Effect<A, E, R>
}
```

`withClientTransform` installs a request-scoped HTTP transform (logging, retry, metrics) on the effects it wraps, dual data-first/data-last. The tag identifier string is `@effect/ai-google/AmazonBedrockConfig` (upstream copy-paste from the Google provider); planning owners must match this exact spelling when referencing the tag, not `@effect/ai-amazon-bedrock/...`.

---

## [4] — AmazonBedrockSchema

Module `@effect/ai-amazon-bedrock/AmazonBedrockSchema`. The hand-written Bedrock Converse codec module: `Schema.Class` wire schemas, `Schema.Literal` enums, and `Schema.Union` discriminated families modelling the Converse request, the Converse response, the ConverseStream event stream, the full Bedrock guardrail-trace corpus, and the `BedrockFoundationModelId` catalogue. Planning pages compose this module by `typeof X.Encoded` (wire) and `typeof X.Type` / `X` (decoded). The high-level `AmazonBedrockClient` methods consume `ConverseRequest`, `ConverseResponse`, and `ConverseResponseStreamEvent`.

### Foundation-model catalogue

```ts
export declare class BedrockFoundationModelId extends Schema.Literal<[/* 90 ids */]> {}
```

90 literal model ids spanning every Bedrock provider family — Amazon Titan/Nova (`amazon.titan-*`, `amazon.nova-{premier,pro,lite,micro,canvas,reel,sonic}-*`), Stability (`stability.stable-diffusion-xl-*`), AI21 Jamba (`ai21.jamba-*`), Anthropic Claude (`anthropic.claude-{instant,v2,3-sonnet,3-haiku,3-opus,3-5-sonnet,3-7-sonnet,3-5-haiku,opus-4,sonnet-4}-*`), Cohere Command/Embed (`cohere.command-*`, `cohere.embed-*`), DeepSeek (`deepseek.r1-v1:0`), Meta Llama (`meta.llama{3,3-1,3-2,3-3,4}-*`), and Mistral (`mistral.{mistral,mixtral,pixtral}-*`). Regenerable via `aws bedrock list-foundation-models --output json | jq '[.modelSummaries[].modelId]'`.

### Converse request owners

```ts
export declare class ConverseRequest {
  readonly modelId: string
  readonly messages: readonly Message[]
  readonly system?: readonly SystemContentBlock[]              // Union: { cachePoint } | { guardContent } | { text }
  readonly toolConfig?: ToolConfiguration
  readonly guardrailConfig?: GuardrailConfiguration
  readonly inferenceConfig?: InferenceConfiguration
  readonly performanceConfig?: PerformanceConfiguration
  readonly promptVariables?: { readonly [x: string]: { readonly text: string } }
  readonly requestMetadata?: { readonly [x: string]: string }
  readonly additionalModelRequestFields?: { readonly [x: string]: unknown }
  readonly additionalModelResponseFieldPaths?: readonly string[]
}

export declare class Message {                                 // role + content blocks
  readonly role: "user" | "assistant"
  readonly content: readonly (typeof ContentBlock.Type)[]
}
export declare class InferenceConfiguration { maxTokens?; stopSequences?; temperature?; topP? }
export declare class PerformanceConfiguration { latency?: "standard" | "optimized" }
export declare class GuardrailConfiguration { guardrailIdentifier; guardrailVersion; trace?: "enabled" | "disabled" | "enabled_full" }
export declare class ToolConfiguration { tools: readonly Tool[]; toolChoice?: ToolChoice }
export declare class Tool { cachePoint?: CachePointBlock; toolSpec?: ToolSpecification }
export declare class ToolSpecification { name; inputSchema: { json: { [x:string]: unknown } }; description? }
export declare class ToolChoice extends Schema.Union<[
  Schema.Struct<{ any: {} }>, Schema.Struct<{ auto: {} }>, Schema.Struct<{ tool: { name: string } }>
]> {}
export declare class SystemContentBlock extends Schema.Union<[
  Schema.Struct<{ cachePoint: CachePointBlock }>,
  Schema.Struct<{ guardContent: GuardrailConverseContentBlock }>,
  Schema.Struct<{ text: string }>
]> {}
```

### Content blocks (the `ContentBlock` discriminated union, `type`-tagged)

```ts
export declare class ContentBlock extends Schema.Union<[
  /* type: "cachePoint"        */ { cachePoint: CachePointBlock },
  /* type: "document"          */ { document: DocumentBlock },
  /* type: "guardContent"      */ { guardContent: GuardrailConverseImageBlock | GuardrailConverseTextBlock },
  /* type: "image"             */ { image: ImageBlock },
  /* type: "reasoningContent"  */ { reasoningContent: ReasoningContentBlock },
  /* type: "text"              */ { text: string },
  /* type: "toolResult"        */ { toolResult: ToolResultBlock },
  /* type: "toolUse"           */ { toolUse: ToolUseBlock },
  /* type: "video"             */ { video: VideoBlock }
]> {}

export declare class CachePointBlock { readonly type: "default" }
export declare const DocumentFormat: Schema.Literal<["csv","doc","docx","html","md","pdf","txt","xls","xlsx"]>
export declare class DocumentBlock { name; format: DocumentFormat; source: { bytes: string } }
export declare const ImageFormat: Schema.Literal<["gif","jpeg","png","webp"]>
export declare class ImageBlock { format: ImageFormat; source: { bytes: string } }
export declare class JsonBlock { readonly json: unknown }
export declare class VideoBlock {
  format: Schema.Literal<["flv","mkv","mov","mp4","mpg","mpeg","three_gp","webm"]>
  source: { bytes: string } | { s3Location: { uri: string; bucketOwner?: string } }
}
export declare class ToolUseBlock { name; toolUseId; input: unknown }
export declare class ToolResultBlock {
  content: readonly ({ document: DocumentBlock } | { image: ImageBlock } | { text: string } | { json: JsonBlock } | { video: VideoBlock })[]
  toolUseId: string
  status?: "success" | "error"
}
export declare class ReasoningContentBlock extends Schema.Union<[
  /* type: "reasoning"          */ { reasoningText: { text: string; signature?: string } },
  /* type: "redacted-reasoning" */ { redactedContent: string }
]> {}
export declare class GuardrailConverseImageBlock { format: "png" | "jpg"; source: { bytes: string } }
export declare class GuardrailConverseTextBlock { text: string; qualifiers?: readonly ("guard_content" | "grounding_source" | "query")[] }
export declare class GuardrailConverseContentBlock extends Schema.Union<[GuardrailConverseImageBlock, GuardrailConverseTextBlock]> {}
```

### Converse response owners

```ts
export declare class ConverseResponse {
  readonly output: ConverseOutput
  readonly metrics: ConverseMetrics
  readonly usage: TokenUsage
  readonly stopReason: "content_filtered" | "end_turn" | "tool_use" | "max_tokens" | "stop_sequence" | "guardrail_intervened"
  readonly trace?: ConverseTrace
  readonly performanceConfig?: { latency: "standard" | "optimized" }
  readonly additionalModelResponseFields?: { readonly [x: string]: unknown }
}
export declare class ConverseOutput { readonly message: Message }
export declare class ConverseMetrics { readonly latencyMs: Duration }          // Schema.DurationFromMillis
export declare class TokenUsage { inputTokens; outputTokens; totalTokens; cacheReadInputTokens?; cacheWriteInputTokens? }
export declare const IntZeroOrGreater: Schema.filter<typeof Schema.Int>
export declare const StopReason: Schema.Literal<["end_turn","tool_use","max_tokens","stop_sequence","guardrail_intervened","content_filtered"]>
export type StopReason = typeof StopReason.Type
```

### ConverseStream event stream

```ts
export declare const ConverseResponseStreamEvent: Schema.Schema<
  /* type-tagged union of 11 members */
  | { type: "messageStart";                 messageStart: MessageStartEvent }
  | { type: "messageStop";                  messageStop: MessageStopEvent }
  | { type: "contentBlockStart";            contentBlockStart: ContentBlockStartEvent }
  | { type: "contentBlockDelta";            contentBlockDelta: ContentBlockDeltaEvent }
  | { type: "contentBlockStop";             contentBlockStop: ContentBlockStopEvent }
  | { type: "metadata";                     metadata: ConverseStreamMetadataEvent }
  | { type: "internalServerException";      internalServerException: { [x:string]: unknown } }
  | { type: "modelStreamErrorException";    modelStreamErrorException: { [x:string]: unknown } }
  | { type: "serviceUnavailableException";  serviceUnavailableException: { [x:string]: unknown } }
  | { type: "throttlingException";          throttlingException: { [x:string]: unknown } }
  | { type: "validationException";          validationException: { [x:string]: unknown } }
>
export type ConverseResponseStreamEvent = typeof ConverseResponseStreamEvent.Type

export declare class MessageStartEvent { readonly role: "user" | "assistant" }
export declare class MessageStopEvent { stopReason: StopReason; additionalModelResponseFields?: { [x:string]: unknown } }
export declare class ContentBlockStartEvent { contentBlockIndex: number; start: ContentBlockStart }
export declare class ContentBlockStart { toolUse?: ToolUseBlockStart }
export declare class ToolUseBlockStart { name: string; toolUseId: string }
export declare class ContentBlockStopEvent { contentBlockIndex: number }
export declare class ContentBlockDeltaEvent { contentBlockIndex: number; delta: ContentBlockDelta }
export declare class ContentBlockDelta extends Schema.Union<[
  /* type: "reasoningContent" */ { reasoningContent: { redactedContent: string } | { signature: string } | { text: string } },
  /* type: "text"             */ { text: string },
  /* type: "toolUse"          */ { toolUse: ToolUseBlockDelta }
]> {}
export declare class ToolUseBlockDelta { readonly input: string }
export declare class ReasoningContentBlockDelta extends Schema.Union<[
  Schema.Struct<{ redactedContent: string }>, Schema.Struct<{ signature: string }>, Schema.Struct<{ text: string }>
]> {}
export declare class ConverseStreamMetadataEvent { metrics: ConverseStreamMetrics; usage: TokenUsage; performanceConfig?: PerformanceConfiguration; trace?: ConverseStreamTrace }
export declare class ConverseStreamMetrics { readonly latencyMs: Duration }
export declare class ConverseStreamTrace { guardrail?: GuardrailTraceAssessment; promptRouter?: PromptRouterTrace }
```

### Guardrail-trace corpus

The `ConverseTrace` / `ConverseStreamTrace` assemble the full Bedrock guardrail assessment tree (all `Schema.Class`, exact names load-bearing):

```ts
export declare class ConverseTrace { guardrail?: GuardrailTraceAssessment; promptRouter?: PromptRouterTrace }
export declare class PromptRouterTrace { invokedModelId?: string }
export declare class GuardrailTraceAssessment {
  actionReason?: string
  inputAssessment?:  { [x:string]: GuardrailAssessment }
  outputAssessments?: { [x:string]: GuardrailAssessment }
  modelOutput?: readonly string[]
}
export declare class GuardrailAssessment {
  contentPolicy?: GuardrailContentPolicyAssessment
  contextualGroundingPolicy?: GuardrailContextualGroundingPolicyAssessment
  invocationMetrics?: GuardrailInvocationMetrics
  sensitiveInformationPolicy?: GuardrailSensitiveInformationPolicyAssessment
  topicPolicy?: GuardrailTopicPolicyAssessment
  wordPolicy?: GuardrailWordPolicyAssessment
}
export declare class GuardrailContentPolicyAssessment { filters: readonly GuardrailContentFilter[] }
export declare class GuardrailContentFilter { type: "HATE"|"INSULTS"|"MISCONDUCT"|"PROMPT_ATTACK"|"SEXUAL"|"VIOLENCE"; action: "BLOCKED"|"NONE"; confidence: "NONE"|"LOW"|"MEDIUM"|"HIGH"; detected?; filterStrength? }
export declare class GuardrailContextualGroundingPolicyAssessment { filters?: readonly GuardrailContextualGroundingFilter[] }
export declare class GuardrailContextualGroundingFilter { type: "GROUNDING"|"RELEVANCE"; action: "BLOCKED"|"NONE"; score: number; threshold: number; detected? }
export declare class GuardrailSensitiveInformationPolicyAssessment { piiEntities: readonly GuardrailPiiEntityFilter[]; regexes: readonly GuardrailRegexFilter[] }
export declare class GuardrailPiiEntityFilter { type: /* 30 PII literals */; action: "ANONYMIZED"|"BLOCKED"|"NONE"; match: string; detected? }
export declare class GuardrailRegexFilter { action: "ANONYMIZED"|"BLOCKED"|"NONE"; name?; match?; regex?; detected? }
export declare class GuardrailTopicPolicyAssessment { topics: readonly GuardrailTopic[] }
export declare class GuardrailTopic { type: "DENY"; name: string; action: "BLOCKED"|"NONE"; detected? }
export declare class GuardrailWordPolicyAssessment { customWords: GuardrailCustomWord; managedWordLists: GuardrailManagedWord }
export declare class GuardrailCustomWord { action: "BLOCKED"|"NONE"; match: string; detected? }
export declare class GuardrailManagedWord { type: "PROFANITY"; action: "BLOCKED"|"NONE"; match: string; detected? }
export declare class GuardrailInvocationMetrics { guardrailCoverage?: GuardrailCoverage; guardrailProcessingLatency?: number; usage?: GuardrailUsage }
export declare class GuardrailCoverage { images?: GuardrailImageCoverage; textCharacters?: GuardrailTextCharactersCoverage }
export declare class GuardrailImageCoverage { guarded?: number; total?: number }
export declare class GuardrailTextCharactersCoverage { guarded?: number; total?: number }
export declare class GuardrailUsage { contentPolicyUnits; contextualGroundingPolicyUnits; sensitiveInformationPolicyFreeUnits; sensitiveInformationPolicyUnits; topicPolicyUnits; wordPolicyUnits; contentPolicyImageUnits? }
```

The `GuardrailPiiEntityFilter.type` literal carries 30 PII categories: `ADDRESS`, `AGE`, `AWS_ACCESS_KEY`, `AWS_SECRET_KEY`, `CA_HEALTH_NUMBER`, `CA_SOCIAL_INSURANCE_NUMBER`, `CREDIT_DEBIT_CARD_CVV`, `CREDIT_DEBIT_CARD_EXPIRY`, `CREDIT_DEBIT_CARD_NUMBER`, `DRIVER_ID`, `EMAIL`, `INTERNATIONAL_BANK_ACCOUNT_NUMBER`, `IP_ADDRESS`, `LICENSE_PLATE`, `MAC_ADDRESS`, `NAME`, `PASSWORD`, `PHONE`, `PIN`, `SWIFT_CODE`, `UK_NATIONAL_HEALTH_SERVICE_NUMBER`, `UK_NATIONAL_INSURANCE_NUMBER`, `UK_UNIQUE_TAXPAYER_REFERENCE_NUMBER`, `URL`, `USERNAME`, `US_BANK_ACCOUNT_NUMBER`, `US_BANK_ROUTING_NUMBER`, `US_INDIVIDUAL_TAX_IDENTIFICATION_NUMBER`, `US_PASSPORT_NUMBER`, `US_SOCIAL_SECURITY_NUMBER`, `VEHICLE_IDENTIFICATION_NUMBER`.

---

## [5] — AmazonBedrockTool

Module `@effect/ai-amazon-bedrock/AmazonBedrockTool`. The provider-defined tool family for Anthropic-on-Bedrock models, each a constructor returning an `@effect/ai` `Tool.ProviderDefined`. Each tool is generic in `Mode extends Tool.FailureMode | undefined`, resolving `failureMode` as `Mode extends undefined ? "error" : Mode`; the cache-control schema is reused from `@effect/ai-anthropic/Generated` (`BetaCacheControlEphemeral`). Unlike the Anthropic package this module exposes **no** `ProviderDefinedTools` union schema, `Coordinate`, or `getProviderDefinedToolName` — only the seven date-suffixed constructors below.

```ts
export declare const AnthropicBash_20241022: <Mode extends Tool.FailureMode | undefined = undefined>(args: { readonly failureMode?: Mode | undefined })
  => Tool.ProviderDefined<"AnthropicBash", {
    args: Schema.Struct<{}>
    parameters: Schema.Struct<{ command: typeof Schema.NonEmptyString; restart: Schema.optional<typeof Schema.Boolean> }>
    success: typeof Schema.String; failure: typeof Schema.Never; failureMode: Mode extends undefined ? "error" : Mode
  }, true>
export declare const AnthropicBash_20250124: <Mode ...> // identical parameter shape to AnthropicBash_20241022

export declare const AnthropicComputerUse_20241022: <Mode ...>(args: {
  readonly display_height_px: number; readonly display_width_px: number
  readonly cache_control?: { readonly type: "ephemeral"; readonly ttl?: "5m" | "1h" | null } | null
  readonly display_number?: number | null; readonly failureMode?: Mode
}) => Tool.ProviderDefined<"AnthropicComputerUse", {
  args: Schema.Struct<{ cache_control: Schema.optionalWith<typeof Generated.BetaCacheControlEphemeral, { nullable: true }>; display_height_px; display_number; display_width_px }>
  parameters: Schema.Struct<{ action: Schema.Literal<["screenshot","left_click","type","key","mouse_move"]>; coordinate?; text? }>
  success: typeof Schema.String; failure: typeof Schema.Never; failureMode: ...
}, true>
export declare const AnthropicComputerUse_20250124: <Mode ...> // action literal adds scroll/left_click_drag/middle_click/right_click/double_click/triple_click/left_mouse_down/left_mouse_up/hold_key/wait + start_coordinate/scroll_direction/scroll_amount/duration params

export declare const AnthropicTextEditor_20241022: <Mode ...>(args: { readonly failureMode?: Mode })
  => Tool.ProviderDefined<"AnthropicTextEditor", {
    parameters: Schema.Struct<{ command: Schema.Literal<["view","create","str_replace","insert","undo_edit"]>; path; file_text?; insert_line?; new_str?; old_str?; view_range? }>
    success: typeof Schema.Void; failure: typeof Schema.Never; ...
  }, true>
export declare const AnthropicTextEditor_20250124: <Mode ...> // command literal ["view","create","str_replace","insert","undo_edit"]
export declare const AnthropicTextEditor_20250429: <Mode ...> // command literal ["view","create","str_replace","insert"]
export declare const AnthropicTextEditor_20250728: <Mode ...> // command literal ["view","create","str_replace","insert"]
```

Provider-defined tool ids are `"AnthropicBash"`, `"AnthropicComputerUse"`, `"AnthropicTextEditor"`. The `args` JSDoc tag is misspelled `@catgory` upstream; it has no runtime effect.

---

## [6] — EventStreamEncoding

Module `@effect/ai-amazon-bedrock/EventStreamEncoding`. A single `Channel` constructor decoding the AWS [event-stream encoding](https://docs.aws.amazon.com/lexv2/latest/dg/event-stream-encoding.html) binary frame format into typed values; this is what backs the service `converseStream`/`streamRequest` decode of the ConverseStream byte stream.

```ts
export declare const makeChannel: <A, I, R, IE, Done>(
  schema: Schema.Schema<A, I, R>,
  options?: { readonly bufferSize?: number }
) => Channel.Channel<Chunk.Chunk<A>, Chunk.Chunk<Uint8Array<ArrayBufferLike>>, IE | ParseError, IE, void, Done, R>
```

Consumes `Chunk<Uint8Array>` input, emits `Chunk<A>` of schema-decoded frames, and fails with `IE | ParseError` (the upstream input error joined with `Schema` decode failure).

---

## [7] — Gaps

None at the structural level: all six modules of `@effect/ai-amazon-bedrock` `0.16.0` were reflected from the installed `node_modules/@effect/ai-amazon-bedrock/dist/dts` declarations and every exported owner is captured above with exact spelling. The deep nested inline object literals in the `ConverseResponseStreamEvent.Type` `metadata` branch (the fully-expanded guardrail-trace tree) are summarized to their owning `Schema.Class` names rather than re-transcribed inline, since each inline shape is the structural mirror of a named guardrail class in section [4]; planning owners reference the named classes, not the inline expansion.
