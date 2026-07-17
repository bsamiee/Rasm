# [TS_RUNTIME_API_EFFECT_AI_ANTHROPIC]

`@effect/ai-anthropic` catalog · MIT · dual CJS+ESM, `sideEffects:[]`, per-module `exports` subpaths (`@effect/ai-anthropic/AnthropicClient`) · marker TSDECL `node_modules/@effect/ai-anthropic/dist/dts/*.d.ts` · peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect` through catalog ownership · tier node|browser (`FetchHttpClient.layer`)

Anthropic binding onto `@effect/ai` resolves the provider-agnostic `LanguageModel`/`Tokenizer`/`Tool` tags against the Anthropic Messages (beta) API. Its asymmetry row carries a language model, the `AnthropicTokenizer` that `ai/model.ts` names as one of its two budget owners, and the richest provider-defined tool family — but no embedding or telemetry module. It uniquely exports `prepareTools`, the tool-preparation helper `@effect/ai-amazon-bedrock` reuses to run Claude on Bedrock, so this catalog is the upstream seam for that sibling. Six owner modules re-export through the barrel (`AnthropicClient`, `AnthropicConfig`, `AnthropicLanguageModel`, `AnthropicTokenizer`, `AnthropicTool`, `Generated`); every provider-facing symbol is one parameterized surface, and the OpenAPI wire corpus (`Generated`) is a category with named anchors. Success/failure flows through the core `AiError.AiError`; all I/O is `Effect`/`Stream`.

## [01]-[ASYMMETRY]

| [INDEX] | [COLUMN]               | [ANTHROPIC]                               | [OPENAI]             | [GOOGLE]         | [BEDROCK]        |
| :-----: | :--------------------- | :---------------------------------------- | :------------------- | :--------------- | :--------------- |
|  [01]   | provider id            | `"anthropic"`                             | openai               | google           | amazon-bedrock   |
|  [02]   | language model         | `AnthropicLanguageModel` (Messages beta)  | Responses            | generateContent  | Converse         |
|  [03]   | embedding model        | —                                         | batched+DL           | raw client       | —                |
|  [04]   | tokenizer              | `AnthropicTokenizer.make` (bare value)    | `make({model})`      | —                | —                |
|  [05]   | provider-defined tools | 5 families / 11 date-suffixed ctors       | 4                    | 4                | 8 (via this pkg) |
|  [06]   | telemetry module       | —                                         | `OpenAiTelemetry`    | —                | —                |
|  [07]   | model-id kind          | `Generated.Model.Encoded` (21 claude ids) | enum                 | free `string`    | 90-id enum       |
|  [08]   | auth                   | `Redacted` apiKey + version + org/project | apiKey + org/project | apiKey           | SigV4 keys       |
|  [09]   | per-request Config     | `Config` + `disableParallelToolCalls`     | `strict`/`verbosity` | `toolConfig`     | Converse fields  |
|  [10]   | streaming fold         | `MessageStreamEvent` (8-member union)     | 49-member            | response re-emit | 11-member        |
|  [11]   | cross-provider export  | `prepareTools` → Amazon Bedrock           | —                    | —                | consumes it      |

## [02]-[CLIENT]

`AnthropicClient` is a `Context.TagClass` (id `@effect/ai-anthropic/AnthropicClient`) wrapping the generated `Client` plus curated beta-message entrypoints. `createMessage`/`createMessageStream` target the `Beta*` request/response owners (the high-level surface is beta-only); `streamRequest` decodes an arbitrary SSE response against a `Schema`; `apiKey` rides the `x-api-key` header.

```ts signature
export interface Service {
  readonly client: Generated.Client
  readonly streamRequest: <A, I, R>(request: HttpClientRequest.HttpClientRequest, schema: Schema.Schema<A, I, R>) => Stream.Stream<A, AiError.AiError, R>
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

ONE constructor pattern, three arities. `make` alone carries `organizationId`/`projectId`; `layer`/`layerConfig` drop them. `anthropicVersion` defaults `"2023-06-01"`, `apiUrl` defaults `"https://api.anthropic.com"`. `make` requires `HttpClient | Scope`; the layers require `HttpClient`; `layerConfig` adds `ConfigError` with each option wrapped in `Config.Config<… | undefined>`.

```ts signature
declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined              // default "https://api.anthropic.com"
  readonly anthropicVersion?: string | undefined    // default "2023-06-01"
  readonly organizationId?: Redacted.Redacted | undefined   // make only
  readonly projectId?: Redacted.Redacted | undefined        // make only
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>
declare const layer:       (options: { apiKey?; apiUrl?; anthropicVersion?; transformClient? }) => Layer.Layer<AnthropicClient, never, HttpClient.HttpClient>
declare const layerConfig: (options: { each field Config.Config<… | undefined> })              => Layer.Layer<AnthropicClient, ConfigError, HttpClient.HttpClient>
```

`MessageStreamEvent` is the ONE streaming-fold surface — an 8-member `Schema.Union` discriminated on `type`. Lifecycle members (`ping`, `error`, `message_start` carrying `BetaMessage`, `message_delta`, `message_stop`) bracket the content-block members (`content_block_start` carrying `BetaContentBlock`, `content_block_delta`, `content_block_stop`); the `content_block_delta.delta` is itself a 5-arm union folded by `type`.

```ts signature
declare const MessageStreamEvent: Schema.Union<[typeof PingEvent, typeof ErrorEvent, typeof MessageStartEvent, typeof MessageDeltaEvent,
  typeof MessageStopEvent, typeof ContentBlockStartEvent, typeof ContentBlockDeltaEvent, typeof ContentBlockStopEvent]>
type MessageStreamEvent = typeof MessageStreamEvent.Type
// ContentBlockDeltaEvent.delta arms (type-tagged Schema.Class):
//   CitationsDelta("citations_delta") | InputJsonContentBlockDelta("input_json_delta") | SignatureContentBlockDelta("signature_delta")
//   | TextContentBlockDelta("text_delta") | ThinkingContentBlockDelta("thinking_delta")
// MessageDeltaEvent carries MessageDelta{ stop_reason; stop_sequence } + MessageDeltaUsage{ input/output/cache_creation/cache_read tokens; server_tool_use: ServerToolUsage }
// ErrorEvent.error.type ∈ invalid_request_error | authentication_error | permission_error | not_found_error | request_too_large | rate_limit_error | api_error | overloaded_error
```

## [03]-[LANGUAGE_MODEL]

`AnthropicLanguageModel` binds Messages onto the core `LanguageModel`/`Model` contracts; the model argument is the widened `(string & {}) | Model` over the 21 Claude ids. ONE model/layer family with the tokenizer fold, plus `prepareTools` — exposed for downstream integrations (Amazon Bedrock) that run Claude models and need the prepared tool/tool-choice/beta triple.

```ts signature
export type Model = typeof Generated.Model.Encoded
// claude-{3-7,3-5,3}-{sonnet,haiku,opus}-{latest,dated} + claude-{sonnet,opus,haiku}-4[-{0,1,5}][-dated] + claude-4-{sonnet,opus}-dated  (21 ids)
declare const model:              (model: (string & {}) | Model, config?: Omit<Config.Service, "model">) => AiModel.Model<"anthropic", LanguageModel.LanguageModel, AnthropicClient>
declare const modelWithTokenizer: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">) => AiModel.Model<"anthropic", LanguageModel.LanguageModel | Tokenizer.Tokenizer, AnthropicClient>
declare const make / layer / layerWithTokenizer / withConfigOverride    // same shapes as the OpenAI family
declare const prepareTools: (options: LanguageModel.ProviderOptions, config: Config.Service) => Effect.Effect<{
  readonly betas: ReadonlySet<string>
  readonly tools: ReadonlyArray<AnthropicTools> | undefined
  readonly toolChoice: typeof Generated.BetaToolChoice.Encoded | undefined
}, AiError.AiError>
export type AnthropicTools = typeof Generated.BetaTool.Encoded | typeof Generated.BetaBashTool20241022.Encoded | … | typeof Generated.BetaTextEditor20250728.Encoded
export type AnthropicReasoningInfo =
  | { readonly type: "thinking";          readonly signature: typeof Generated.ResponseThinkingBlock.fields.thinking.Encoded }
  | { readonly type: "redacted_thinking"; readonly redactedData: typeof Generated.RequestRedactedThinkingBlock.fields.data.Encoded }
```

`Config` (tag `@effect/ai-anthropic/AnthropicLanguageModel/Config`, `static getOrUndefined`) is the `CreateMessageParams` minus SDK-owned keys (`messages`/`tools`/`tool_choice`/`stream`) made partial, plus `disableParallelToolCalls` — the tier-routing seam `ai/model.ts` writes per call.

```ts signature
namespace Config { interface Service extends Simplify<Partial<Omit<typeof Generated.CreateMessageParams.Encoded, "messages"|"tools"|"tool_choice"|"stream">>> { readonly disableParallelToolCalls?: boolean } }
```

`declare module` augmentations attach an optional `anthropic` key — ONE boundary-hook pattern. Prompt caching is the dominant one: every message/part options interface gains `cacheControl?: CacheControlEphemeral.Encoded`.

| [INDEX] | [AUGMENTS] | [INTERFACES]                                | [ANTHROPIC_SLOT]                                   |
| :-----: | :--------- | :------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Prompt`   | `System/User/Assistant/Tool MessageOptions` | `cacheControl?`                                    |
|  [02]   | `Prompt`   | `Text/ToolCall/ToolResult PartOptions`      | `cacheControl?`                                    |
|  [03]   | `Prompt`   | `ReasoningPartOptions`                      | `AnthropicReasoningInfo & { cacheControl? }`       |
|  [04]   | `Prompt`   | `FilePartOptions`                           | `citations?`, `documentTitle?`, `documentContext?` |
|  [05]   | `Response` | `Reasoning{,Start,Delta}PartMetadata`       | `AnthropicReasoningInfo`                           |
|  [06]   | `Response` | `FinishPartMetadata`                        | `{ usage?: BetaUsage; stopSequence? }`             |
|  [07]   | `Response` | `DocumentSourcePartMetadata`                | `char_location` \| `page_location` citation        |
|  [08]   | `Response` | `UrlSourcePartMetadata`                     | `{ source:"url"; citedText; encryptedIndex }`      |

## [04]-[TOKENIZER]

`AnthropicTokenizer.make` is a bare `Tokenizer.Service` value (not a factory function — distinct from `OpenAiTokenizer.make`, which takes `{ model }`); `layer` binds the `Tokenizer.Tokenizer` tag dependency-free, and `AnthropicLanguageModel.layerWithTokenizer`/`modelWithTokenizer` fold it in. This is the canonical tokenizer `ai/model.ts` names as its primary budget owner.

```ts signature
declare const make: Tokenizer.Service
declare const layer: Layer.Layer<Tokenizer.Tokenizer>
```

## [05]-[TOOL]

`AnthropicTool` exports the closed-form `ProviderDefinedTools` union schema plus date-suffixed constructors, each ONE instance of `<Mode extends Tool.FailureMode | undefined>(args) => Tool.ProviderDefined<"Anthropic<Name>", { …; failureMode: Mode extends undefined ? "error" : Mode }, requiresHandler>`. `requiresHandler` marks the intra-family asymmetry: locally-executed tools (`Bash`/`ComputerUse`/`TextEditor`) are `true` and need an app handler; provider-executed tools (`CodeExecution`/`WebSearch`) are `false`. Versioning is by date suffix — the same tag with an evolving `parameters` literal.

| [INDEX] | [TAG]                    | [CTORS]                                            | [PARAMETERS_AXIS_SUCCESS]                                |
| :-----: | :----------------------- | :------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `AnthropicBash`          | `Bash_{20241022,20250124}`                         | `{ command; restart? }` / `String`                       |
|  [02]   | `AnthropicComputerUse`   | `ComputerUse_{20241022,20250124}`                  | `action` literal (5 → 15 verbs) + coordinates / `String` |
|  [03]   | `AnthropicTextEditor`    | `TextEditor_{20241022,20250124,20250429,20250728}` | `command` literal (5 → 4 verbs) / `Void`                 |
|  [04]   | `AnthropicCodeExecution` | `CodeExecution_{20250522,20250825}`                | `EmptyParams` / code-exec result blocks, typed failure   |
|  [05]   | `AnthropicWebSearch`     | `WebSearch_20250305`                               | `EmptyParams` / `Array<RequestWebSearchResultBlock>`     |

```ts signature
declare const ProviderDefinedTools: Schema.Union<[/* 10 Beta*Tool schemas: bash×2, code-execution, computer-use×2, text-editor×4, web-search */]>
declare const Coordinate: Schema.Tuple2<typeof Schema.Number, typeof Schema.Number>
declare const getProviderDefinedToolName: (name: string) => string | undefined   // wire name ("web_search") → toolkit id ("AnthropicWebSearch")
```

## [06]-[CONFIG]

`AnthropicConfig` (`Context.TagClass`, id `@effect/ai-anthropic/AnthropicConfig`, `static getOrUndefined`) is the request-scoped client transform — a per-request `HttpClient` mutation without rebuilding transport, dual data-first/data-last. Distinct from the layer-construction `transformClient`.

```ts signature
namespace AnthropicConfig { interface Service { readonly transformClient?: (client: HttpClient) => HttpClient } }
declare const withClientTransform: { (t: (c: HttpClient) => HttpClient): <A,E,R>(self) => …; <A,E,R>(self, t): … }
```

## [07]-[GENERATED]

`Generated` is the machine-generated Anthropic REST surface: 341 exported owners (`Schema.Class` wire schemas, `Schema.Literal` enums, `Schema.Union` families) plus the `make` factory, a `Client` interface spanning the stable endpoints and their `Beta*` mirrors, and the `ClientError` rail. That surface doubles: a stable family and a `Beta*` mirror carrying beta-only blocks (MCP tool blocks, web-fetch, code-execution result blocks, container/skill params). Planning code composes by `typeof X.Encoded` (wire) / `typeof X.Type` (decoded) and reaches REST via `AnthropicClient.Service.client.<endpoint>`; the high-level methods use the `Beta*` owners. Stable endpoints fail with `ClientError<"ErrorResponse", …>`, beta with `ClientError<"BetaErrorResponse", …>`, both joined by `HttpClientError | ParseError`.

Load-bearing anchors (exact spelling): `Model` (the 21-id literal), `CreateMessageParams`/`BetaCreateMessageParams`, `Message`/`BetaMessage`, `MessagesPostParams`/`BetaMessagesPostParams`, `ContentBlock`/`BetaContentBlock`, `Usage`/`BetaUsage`, `StopReason`/`BetaStopReason` (Beta adds `model_context_window_exceeded`), `ToolChoice`/`BetaToolChoice`, `CacheControlEphemeral`/`BetaCacheControlEphemeral` (the caching breakpoint — also consumed by `@effect/ai-amazon-bedrock`), `ResponseThinkingBlock`, `RequestRedactedThinkingBlock`, `RequestCitationsConfig`, `ErrorResponse`/`BetaErrorResponse`, and the `Beta*Tool20*` provider-tool schemas. Endpoints span messages, message-batches, token-counting, models, files, and skills — each with its `Beta*` mirror.

## [08]-[INTEGRATION]

- Universal Effect rails: `AnthropicLanguageModel.model(id)` produces the same `LanguageModel.LanguageModel` tag as every sibling — provider choice is a single `Layer` swap in `ai/model.ts`. `Redacted`+`Config` own credential resolution; `Stream` folds `MessageStreamEvent`; `Schema` decodes `Config`/tools/responses; `Match.discriminator("type")` dispatches the streaming union and its nested `delta` arms; `Effect.catchTag` branches `AiError`. Compose top-down: `Effect.provide(AnthropicLanguageModel.modelWithTokenizer(id))` over `AnthropicClient.layer({ apiKey })` over an `HttpClient` layer.
- `@effect/platform` seam: every `layer*` requires `HttpClient.HttpClient` from the `net/client` default-policy row; browser binds `FetchHttpClient.layer`, node `NodeHttpClient.layer`.
- `@effect/ai` core (sibling catalog `effect-ai.md`): satisfies `LanguageModel.LanguageModel`, `Tokenizer.Tokenizer`, `Tool.ProviderDefined`/`Tool.FailureMode`; consumes `LanguageModel.ProviderOptions` in `prepareTools`; augments `Prompt`/`Response` provider slots. No embedding or telemetry tag — those asymmetry cells are empty.
- Sibling providers: `prepareTools` + `Generated.BetaCacheControlEphemeral` are imported by `@effect/ai-amazon-bedrock` (which peer-depends on this package) to run Claude on Bedrock — this catalog is that seam's upstream; see `effect-ai-amazon-bedrock.md`.
- Design consumers: `ai/model.ts` (row + tier-routing + guardrail gate), `ai/model.ts` (`AnthropicTokenizer` as the primary budget owner), `ai/tool.ts`+`ai/tool.ts` (the tool family projected — `requiresHandler:true` tools bind app handlers via `Toolkit.toLayer`). No `embed/*` binding (embeddings are OpenAI-only).
