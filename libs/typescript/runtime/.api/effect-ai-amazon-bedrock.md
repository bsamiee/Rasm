# [TS_RUNTIME_API_EFFECT_AI_AMAZON_BEDROCK]

`@effect/ai-amazon-bedrock` catalog · MIT · dual CJS+ESM, `sideEffects:[]`, per-module `exports` subpaths (`@effect/ai-amazon-bedrock/AmazonBedrockClient`) · marker TSDECL `node_modules/@effect/ai-amazon-bedrock/dist/dts/*.d.ts` · peers `@effect/ai`, `@effect/ai-anthropic`, `@effect/platform`, `@effect/experimental`, `effect` through catalog ownership · tier node (SigV4-signed regional HTTPS; no browser binding)

Amazon Bedrock binding onto `@effect/ai` resolves the provider-agnostic `LanguageModel`/`Tool` tags against the Bedrock Converse/ConverseStream API. It is the most divergent row on the capability-asymmetry table — SigV4 credentials instead of a bearer key, a hand-written `AmazonBedrockSchema` codec instead of an OpenAPI `Generated` module, an AWS binary event-stream channel (`EventStreamEncoding`) instead of SSE, native guardrail assessment (the `GuardrailConfiguration` request field → the full `GuardrailTraceAssessment` response tree), and a peer dependency on `@effect/ai-anthropic` whose `prepareTools` + `BetaCacheControlEphemeral` it reuses to run Claude on Bedrock. No embedding, tokenizer, or telemetry owner. Six owner modules re-export through the barrel (`AmazonBedrockClient`, `AmazonBedrockConfig`, `AmazonBedrockLanguageModel`, `AmazonBedrockSchema`, `AmazonBedrockTool`, `EventStreamEncoding`). Success/failure flows through the core `AiError.AiError`; all I/O is `Effect`/`Stream`/`Channel`.

## [01]-[ASYMMETRY]

| [INDEX] | [COLUMN]               | [AMAZON_BEDROCK]                           | [OPENAI]                    | [ANTHROPIC]       | [GOOGLE]         |
| :-----: | :--------------------- | :----------------------------------------- | :-------------------------- | :---------------- | :--------------- |
|  [01]   | provider id            | `"amazon-bedrock"`                         | openai                      | anthropic         | google           |
|  [02]   | language model         | `AmazonBedrockLanguageModel`               | Responses                   | Messages          | generateContent  |
|  [03]   | embedding model        | —                                          | curated ×2                  | —                 | raw client       |
|  [04]   | tokenizer              | —                                          | `make({model})`             | value             | —                |
|  [05]   | provider-defined tools | 8 ctors / 3 tags                           | 4                           | 5 families        | 4                |
|  [06]   | telemetry module       | —                                          | `OpenAiTelemetry`           | —                 | —                |
|  [07]   | model-id kind          | 90-id `Encoded`                            | enum                        | 21-id enum        | free `string`    |
|  [08]   | auth                   | SigV4: `accessKeyId`/`Redacted`            | apiKey+org/proj             | apiKey+version    | apiKey           |
|  [09]   | codec module           | `AmazonBedrockSchema` (66, hand-written)   | OpenAPI `Generated` (~1238) | OpenAPI (341)     | OpenAPI (~238)   |
|  [10]   | stream decode          | AWS binary event-stream                    | SSE                         | SSE               | SSE              |
|  [11]   | streaming fold         | 11-member                                  | 49-member                   | 8-member          | response re-emit |
|  [12]   | native guardrails      | `GuardrailTraceAssessment`                 | —                           | —                 | `safetyRatings`  |
|  [13]   | sibling dep            | `@effect/ai-anthropic`                     | —                           | upstream          | —                |
|  [14]   | Config transform tag   | id `@effect/ai-google/AmazonBedrockConfig` | `OpenAiConfig`              | `AnthropicConfig` | `GoogleConfig`   |

## [02]-[CLIENT]

`AmazonBedrockClient` is a `Context.TagClass` (id `@effect/ai-amazon-bedrock/AmazonBedrockClient`) wrapping a low-level `Client` plus curated Converse entrypoints. `converse`/`converseStream` forward Anthropic beta opt-ins for Claude-on-Bedrock via `params["anthropic-beta"]`; the low-level `client.converse` surfaces the raw `HttpClientError | ParseError` rail whereas the Service methods map onto `AiError.AiError`. `streamRequest` decodes an arbitrary AWS event-stream response against a `Schema`.

```ts signature
export interface Service {
  readonly client: Client
  readonly streamRequest: <A, I, R>(request: HttpClientRequest.HttpClientRequest, schema: Schema.Schema<A, I, R>) => Stream.Stream<A, AiError.AiError, R>
  readonly converse:       (options: { params?: { "anthropic-beta"?: string }; payload: typeof ConverseRequest.Encoded }) => Effect.Effect<ConverseResponse, AiError.AiError>
  readonly converseStream: (options: { params?: { "anthropic-beta"?: string }; payload: typeof ConverseRequest.Encoded }) => Stream.Stream<ConverseResponseStreamEvent, AiError.AiError>
}
export interface Client { readonly converse: (options: { params?; payload: typeof ConverseRequest.Encoded }) => Effect.Effect<typeof ConverseResponse.Type, HttpClientError.HttpClientError | ParseError> }
```

ONE constructor pattern, three arities carrying SigV4 credentials — `accessKeyId` (plain string, non-secret), `secretAccessKey` and `sessionToken` `Redacted<string>`, `region`, `apiUrl` (overrides the derived regional endpoint). `make` requires `HttpClient | Scope`; the layers require `HttpClient`; `layerConfig` wraps each in `Config.Config` and adds `ConfigError`. Node binds `NodeHttpClient.layer`; there is no browser binding.

```ts signature
declare const make: (options: {
  readonly apiUrl?: string | undefined
  readonly accessKeyId: string
  readonly secretAccessKey: Redacted.Redacted<string>
  readonly sessionToken?: Redacted.Redacted<string> | undefined
  readonly region?: string | undefined
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>
declare const layer:       (options: /* same, literal creds */) => Layer.Layer<AmazonBedrockClient, never, HttpClient.HttpClient>
declare const layerConfig: (options: { accessKeyId: Config.Config<string>; secretAccessKey: Config.Config<Redacted>; sessionToken?: Config.Config<Redacted>; region?: Config.Config<string>; apiUrl?: Config.Config<string> }) => Layer.Layer<AmazonBedrockClient, ConfigError, HttpClient.HttpClient>
```

## [03]-[LANGUAGE_MODEL]

`AmazonBedrockLanguageModel` binds Converse onto the core `LanguageModel`/`Model` contracts; the model argument is `(string & {}) | Model` over the 90-id `BedrockFoundationModelId` (or a cross-region inference-profile ARN via the open-string arm). ONE model/layer family, narrower than OpenAI/Anthropic: `model`/`make`/`layer`/`withConfigOverride` — no `modelWithTokenizer`/`layerWithTokenizer` (no tokenizer) and no `prepareTools` (it consumes Anthropic's).

```ts signature
export type Model = typeof BedrockFoundationModelId.Encoded   // 90 ids, section [05]
declare const model: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">) => AiModel.Model<"amazon-bedrock", LanguageModel.LanguageModel, AmazonBedrockClient>
declare const make / layer / withConfigOverride   // same shapes as the OpenAI family, minus the tokenizer arms
export type AmazonBedrockReasoningInfo =
  | { readonly type: "thinking";          readonly signature: string }     // encrypted, verifies Bedrock-generated thinking
  | { readonly type: "redacted_thinking"; readonly redactedData: string }  // safety-flagged, encrypted
```

`Config` (tag `@effect/ai-amazon-bedrock/AmazonBedrockLanguageModel/Config`, `static getOrUndefined`) is the `ConverseRequest` minus SDK-owned keys (`messages`/`system`/`toolConfig`) made partial — i.e. `modelId`, `guardrailConfig`, `inferenceConfig`, `performanceConfig`, `promptVariables`, `requestMetadata`, `additionalModelRequestFields`, `additionalModelResponseFieldPaths`. `guardrailConfig` here is the seam that turns on native Bedrock guardrails per call.

```ts signature
namespace Config { interface Service extends Simplify<Partial<Omit<typeof ConverseRequest.Encoded, "messages"|"system"|"toolConfig">>> {} }
```

`declare module` augmentations attach an optional `bedrock` key — ONE boundary-hook pattern. `cachePoint` is the caching breakpoint; `FinishPartMetadata.bedrock.trace` carries the full guardrail assessment tree.

| [INDEX] | [AUGMENTS] | [INTERFACES]                                | [BEDROCK_SLOT]                                                 |
| :-----: | :--------- | :------------------------------------------ | :------------------------------------------------------------- |
|  [01]   | `Prompt`   | `System/User/Assistant/Tool MessageOptions` | `{ cachePoint?: CachePointBlock.Encoded }`                     |
|  [02]   | `Prompt`   | `ReasoningPartOptions`                      | `AmazonBedrockReasoningInfo`                                   |
|  [03]   | `Response` | `ReasoningPartMetadata`                     | `AmazonBedrockReasoningInfo`                                   |
|  [04]   | `Response` | `FinishPartMetadata`                        | `{ trace?: ConverseTrace; usage: { cacheWriteInputTokens? } }` |

## [04]-[TOOL]

`AmazonBedrockTool` exports eight constructors across three tags — the Anthropic-on-Bedrock local tools, each ONE instance of `<Mode extends Tool.FailureMode | undefined>(args) => Tool.ProviderDefined<"Anthropic<Name>", { …; failureMode: Mode extends undefined ? "error" : Mode }, true>` (`requiresHandler:true`; the app runs them). Each `cache_control` arg reuses `@effect/ai-anthropic/Generated.BetaCacheControlEphemeral`. Unlike `@effect/ai-anthropic`'s `AnthropicTool`, this module exposes no `ProviderDefinedTools` union, `Coordinate`, or `getProviderDefinedToolName` — only the eight date-suffixed constructors.

`action`/`command` axes are `Schema.Literal` verb sets; `AnthropicComputerUse` also carries coordinate args.

| [INDEX] | [TAG]                  | [CTORS]                                                            | [PARAMETERS_AXIS_SUCCESS]          |
| :-----: | :--------------------- | :----------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `AnthropicBash`        | `AnthropicBash_20241022`/`_20250124`                               | `{ command; restart? }` / `String` |
|  [02]   | `AnthropicComputerUse` | `AnthropicComputerUse_20241022`/`_20250124`                        | `action` (5→15 verbs) / `String`   |
|  [03]   | `AnthropicTextEditor`  | `AnthropicTextEditor_20241022`/`_20250124`/`_20250429`/`_20250728` | `command` (5→4 verbs) / `Void`     |

`failure` is `Schema.Never` on all eight; `ComputerUse`/`TextEditor` carry `cache_control` args, `Bash` carries only `failureMode`. Each `args` JSDoc tag is misspelled `@catgory` upstream (no runtime effect).

## [05]-[SCHEMA]

`AmazonBedrockSchema` is a hand-written Converse codec (66 exported owners — `Schema.Class` wire schemas, `Schema.Literal` enums, `Schema.Union` families), not OpenAPI-generated. Planning code composes it by `typeof X.Encoded` (wire) / `typeof X.Type` (decoded); `AmazonBedrockClient` consumes `ConverseRequest`/`ConverseResponse`/`ConverseResponseStreamEvent`.

`BedrockFoundationModelId` is a 90-id `Schema.Literal` spanning every Bedrock provider family — Amazon Titan/Nova, Stability, AI21 Jamba, Anthropic Claude, Cohere Command/Embed, DeepSeek, Meta Llama, Mistral. Regenerable via `aws bedrock list-foundation-models --output json | jq '[.modelSummaries[].modelId]'`.

```ts signature
declare const BedrockFoundationModelId: Schema.Literal<[/* 90 ids: amazon.titan-*, amazon.nova-*, stability.*, ai21.jamba-*, anthropic.claude-*, cohere.*, deepseek.r1-*, meta.llama*, mistral.* */]>
export declare class ConverseRequest {
  readonly modelId: string; readonly messages: readonly Message[]
  readonly system?: readonly SystemContentBlock[]   // Union: { cachePoint } | { guardContent } | { text }
  readonly toolConfig?: ToolConfiguration; readonly guardrailConfig?: GuardrailConfiguration
  readonly inferenceConfig?: InferenceConfiguration; readonly performanceConfig?: PerformanceConfiguration
  readonly promptVariables?; requestMetadata?; additionalModelRequestFields?; additionalModelResponseFieldPaths?
}
export declare class ConverseResponse {
  readonly output: ConverseOutput; readonly metrics: ConverseMetrics; readonly usage: TokenUsage
  readonly stopReason: StopReason; readonly trace?: ConverseTrace; readonly performanceConfig?; additionalModelResponseFields?
}
declare const StopReason: Schema.Literal<["end_turn","tool_use","max_tokens","stop_sequence","guardrail_intervened","content_filtered"]>
```

`ContentBlock` is the ONE `type`-tagged content union (9 arms: `cachePoint`, `document`, `guardContent`, `image`, `reasoningContent`, `text`, `toolResult`, `toolUse`, `video`), with leaf owners `DocumentBlock`/`ImageBlock`/`VideoBlock`/`ToolUseBlock`/`ToolResultBlock`/`ReasoningContentBlock`/`CachePointBlock`. `ConverseResponseStreamEvent` is the ONE streaming-fold surface — an 11-member `type`-tagged union: `messageStart`/`messageStop`/`contentBlockStart`/`contentBlockDelta`/`contentBlockStop`/`metadata` plus five AWS exception frames (`internalServerException`, `modelStreamErrorException`, `serviceUnavailableException`, `throttlingException`, `validationException`); the per-event owners are `MessageStartEvent`/`MessageStopEvent`/`ContentBlockStartEvent`/`ContentBlockDeltaEvent`/`ContentBlockStopEvent`/`ConverseStreamMetadataEvent` (+ `ContentBlockStart`/`ToolUseBlockStart`/`ContentBlockDelta`/`ToolUseBlockDelta`/`ReasoningContentBlockDelta`).

Guardrail-trace corpus is ONE assessment tree parameterized by policy kind, feeding `ConverseTrace`/`ConverseStreamTrace` → `FinishPartMetadata.bedrock.trace`. `GuardrailTraceAssessment` holds `inputAssessment`/`outputAssessments` maps of `GuardrailAssessment`, each a record over six policy owners:

```ts signature
class GuardrailAssessment {
  contentPolicy?: GuardrailContentPolicyAssessment                       // filters: GuardrailContentFilter[] (type ∈ HATE|INSULTS|MISCONDUCT|PROMPT_ATTACK|SEXUAL|VIOLENCE)
  contextualGroundingPolicy?: GuardrailContextualGroundingPolicyAssessment // GROUNDING|RELEVANCE, score/threshold
  sensitiveInformationPolicy?: GuardrailSensitiveInformationPolicyAssessment // piiEntities: GuardrailPiiEntityFilter[] + regexes
  topicPolicy?: GuardrailTopicPolicyAssessment; wordPolicy?: GuardrailWordPolicyAssessment
  invocationMetrics?: GuardrailInvocationMetrics                         // coverage + latency + GuardrailUsage per-policy unit counts
}
// GuardrailPiiEntityFilter.type is a 31-category literal (ADDRESS, AGE, AWS_ACCESS_KEY, …, US_SOCIAL_SECURITY_NUMBER, VEHICLE_IDENTIFICATION_NUMBER); action ∈ ANONYMIZED|BLOCKED|NONE
```

## [06]-[EVENT_STREAM]

`EventStreamEncoding.makeChannel` is a single `Channel` constructor decoding the AWS [event-stream](https://docs.aws.amazon.com/lexv2/latest/dg/event-stream-encoding.html) binary frame format into schema-decoded values — what backs the Service `converseStream`/`streamRequest` over the ConverseStream byte stream. It consumes `Chunk<Uint8Array>`, emits `Chunk<A>` of decoded frames, and fails with `IE | ParseError` (upstream input error joined with `Schema` decode failure). This is the Bedrock-specific stacking onto `effect/Channel` that no other provider needs (the others fold SSE).

```ts signature
declare const makeChannel: <A, I, R, IE, Done>(schema: Schema.Schema<A, I, R>, options?: { readonly bufferSize?: number }) =>
  Channel.Channel<Chunk.Chunk<A>, Chunk.Chunk<Uint8Array<ArrayBufferLike>>, IE | ParseError, IE, void, Done, R>
```

## [07]-[CONFIG]

`AmazonBedrockConfig` (`Context.TagClass`, `static getOrUndefined`) is the request-scoped client transform, dual data-first/data-last. Its tag id is the upstream copy-paste **`@effect/ai-google/AmazonBedrockConfig`** (not `@effect/ai-amazon-bedrock/…`); planning owners referencing the tag must match this exact spelling.

```ts signature
namespace AmazonBedrockConfig { interface Service { readonly transformClient?: (client: HttpClient) => HttpClient } }
declare const withClientTransform: { (t: (c: HttpClient) => HttpClient): <A,E,R>(self) => …; <A,E,R>(self, t): … }
```

## [08]-[INTEGRATION]

- Universal Effect rails: `AmazonBedrockLanguageModel.model(id)` produces the same `LanguageModel.LanguageModel` tag as every sibling — provider choice is a single `Layer` swap in `ai/model.ts`. `Config`+`Redacted` own SigV4 credential resolution (`layerConfig`); `Stream`+`Channel` fold `ConverseResponseStreamEvent` through `EventStreamEncoding.makeChannel`; `Schema` decodes the hand-written Converse codec; `Match.discriminator("type")` dispatches the 11-member stream union (including the five AWS exception frames) and the 9-arm `ContentBlock`; `Effect.catchTag` branches `AiError`. Compose top-down: `Effect.provide(AmazonBedrockLanguageModel.model(id))` over `AmazonBedrockClient.layer({ accessKeyId, secretAccessKey, region })` over a node `HttpClient` layer.
- `@effect/platform` seam: every `layer*` requires `HttpClient.HttpClient` from the `net/client` default-policy row; node-only (`NodeHttpClient.layer`) because SigV4 signing has no browser binding.
- `@effect/ai` core (sibling catalog `effect-ai.md`): satisfies `LanguageModel.LanguageModel`, `Tool.ProviderDefined`/`Tool.FailureMode`; augments `Prompt`/`Response` provider slots. No `EmbeddingModel`, `Tokenizer`, or `Telemetry` tag.
- Sibling providers: this row peer-depends on `@effect/ai-anthropic` (see `effect-ai-anthropic.md`) — it consumes that package's `prepareTools` and `Generated.BetaCacheControlEphemeral` to run Claude-on-Bedrock tools, and forwards Anthropic beta opt-ins via `params["anthropic-beta"]`. That `AmazonBedrockConfig` tag id copy-paste bug (`@effect/ai-google/…`) is a spelling hazard `ai/model.ts` must respect.
- Native guardrails: Bedrock is the provider whose asymmetry populates the `ai/model.ts` guardrail-gate cell natively — set `Config.Service.guardrailConfig` per call, and the `GuardrailTraceAssessment` tree surfaces on `FinishPartMetadata.bedrock.trace`, an alternative to the design's app-level moderation folds.
- Design consumers: `ai/model.ts` (row + tier-routing via `Config` + the guardrail gate, read from `FinishPartMetadata.bedrock.trace`), `ai/tool.ts`+`ai/tool.ts` (the eight `requiresHandler:true` tools bound via `Toolkit.toLayer`). No `ai/model.ts` or `ai/embed.ts` binding — both asymmetry cells are empty.
