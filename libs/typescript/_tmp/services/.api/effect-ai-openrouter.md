# [API_CATALOGUE] effect-ai-openrouter

Grounded from installed `node_modules` type declarations (`@effect/ai-openrouter` `0.11.0`, MIT, `dist/dts/*.d.ts`; peers `@effect/ai` `0.36.0`, `@effect/platform`, `effect` `3.21.4`; ESM, node `>=24`). Covers the load-bearing public surface the TS planning pages consume for an OpenRouter-backed `@effect/ai` provider: the `OpenRouterClient` service with its two curated chat methods (non-streaming `createChatCompletion` + streaming `createChatCompletionStream`) and the streaming-chunk schema family, the `OpenRouterLanguageModel` model/layer family with its provider-options and provider-metadata `declare module` augmentations, the `OpenRouterConfig` per-effect HTTP-transform overlay, and the `Generated` OpenAPI codec module (request/response wire schemas plus the typed `Client` endpoint surface and its `ClientError` discriminated-failure carrier). Unlike `@effect/ai-openai`, this package ships only four modules through `index.d.ts` — there is no embedding-model, tokenizer, provider-defined-tool, or telemetry module; OpenRouter is a chat-completions aggregator binding.

Package root re-exports (namespace barrels):

```ts
// @effect/ai-openrouter
export * as Generated from "./Generated.js"
export * as OpenRouterClient from "./OpenRouterClient.js"
export * as OpenRouterConfig from "./OpenRouterConfig.js"
export * as OpenRouterLanguageModel from "./OpenRouterLanguageModel.js"
```

Service tags carry the identifiers `@effect/ai-openrouter/OpenRouterClient`, `@effect/ai-openrouter/OpenRouterConfig`, and `@effect/ai-openrouter/OpenRouterLanguageModel/Config`. Cross-package owners (`Effect`, `Layer`, `Schema`, `Stream`, `Config`, `ConfigError`, `Redacted`, `Context`, `HttpClient`, and `@effect/ai` `LanguageModel`/`Model`/`AiError`) resolve against `effect.md` and the `@effect/ai` core surface. This package carries the `node` tier tag — it requires `HttpClient.HttpClient` from `@effect/platform` (node binding `NodeHttpClient.layer`, browser binding `FetchHttpClient.layer`). It resolves through the shared adapter seam — `effect-ai.md` `PROVIDER_RESOLUTION`/`STACKING_LAW` own the composition-root layer stack (`HttpClient` → `OpenRouterClient.layer` → `OpenRouterLanguageModel.model` → core `LanguageModel.LanguageModel`, one arm of the `ai.md` `AiProvider` literal axis); this page documents only OpenRouter's DELTA surface (the aggregator routing/provider-preference request controls, the per-request dollar-cost usage projection, the reasoning-format algebra, and the four alternate API-surface request families).

---

## [1] — OpenRouterClient

Module `@effect/ai-openrouter/OpenRouterClient`. The provider HTTP service, the two curated chat methods, and the four streaming-chunk `Schema.Class` owners, plus the three transport constructors. `OpenRouterClient` is a `Context.TagClass` whose service value is `Service`.

### Service tag

```ts
declare const OpenRouterClient_base: Context.TagClass<OpenRouterClient, "@effect/ai-openrouter/OpenRouterClient", Service>
export declare class OpenRouterClient extends OpenRouterClient_base {}
```

### Service

```ts
export interface Service {
  // direct access to the generated REST client for endpoints not covered by the curated methods
  readonly client: Generated.Client
  readonly createChatCompletion: (
    options: typeof Generated.ChatGenerationParams.Encoded
  ) => Effect.Effect<Generated.ChatResponse, AiError.AiError>
  readonly createChatCompletionStream: (
    options: Omit<typeof Generated.ChatGenerationParams.Encoded, "stream">
  ) => Stream.Stream<ChatStreamingResponseChunk, AiError.AiError>
}
```

`createChatCompletion`/`createChatCompletionStream` collapse all transport failures to `AiError.AiError` (`@effect/ai/AiError`); the raw `client.*` escape hatch surfaces `HttpClientError.HttpClientError | ParseError | ClientError<…>` instead (see [4]). Note there is no `streamRequest` low-level entry (the OpenAI/Anthropic siblings expose one); the only streaming surface is `createChatCompletionStream`, which folds `ChatStreamingResponseChunk`.

### Constructors / layers

`make` does NOT require `Scope` (the OpenAI sibling's `make` does); both `make` and `layer` carry no error channel, `layerConfig` carries `ConfigError`. All three require `HttpClient.HttpClient`. `layerConfig` wraps each option in `Config.Config<…>` (non-optional inner type, unlike OpenAI's `Config.Config<… | undefined>`). The two OpenRouter-specific ranking options — `referrer` (site URL) and `title` (site title) — are unique to this provider.

```ts
export declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly referrer?: string | undefined
  readonly title?: string | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient>

export declare const layer: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly referrer?: string | undefined
  readonly title?: string | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<OpenRouterClient, never, HttpClient.HttpClient>

export declare const layerConfig: (options: {
  readonly apiKey?: Config.Config<Redacted.Redacted> | undefined
  readonly apiUrl?: Config.Config<string> | undefined
  readonly referrer?: Config.Config<string> | undefined
  readonly title?: Config.Config<string> | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<OpenRouterClient, ConfigError, HttpClient.HttpClient>
```

### ChatStreamingResponseChunk — the streaming fold surface

The OpenRouter streaming algebra is a chunk hierarchy (not a tagged-union of distinct event classes as in OpenAI). `createChatCompletionStream` yields `ChatStreamingResponseChunk`; planning pages fold over `chunk.choices[i].delta` and `chunk.usage`. Four exported `Schema.Class` owners (each field listed verbatim from the decoded `Type` shape):

```ts
export declare class ChatStreamingMessageToolCall extends ChatStreamingMessageToolCall_base {}
// { index: number; id?: string; type?: "function";
//   function: { name?: string; arguments?: string } }

export declare class ChatStreamingMessageChunk extends ChatStreamingMessageChunk_base {}
// { role?: "assistant"; content?: string; reasoning?: string;
//   reasoning_details?: ReadonlyArray<Generated.ReasoningDetailSummary | Generated.ReasoningDetailEncrypted | Generated.ReasoningDetailText>;
//   images?: ReadonlyArray<Generated.ChatMessageContentItemImage>;
//   refusal?: string;
//   tool_calls?: ReadonlyArray<ChatStreamingMessageToolCall>;
//   annotations?: ReadonlyArray<Generated.FileAnnotationDetail | Generated.URLCitationAnnotationDetail> }

export declare class ChatStreamingChoice extends ChatStreamingChoice_base {}
// { index: number; delta?: ChatStreamingMessageChunk;
//   finish_reason?: "length" | "content_filter" | "error" | "stop" | "tool_calls";
//   native_finish_reason?: string;
//   logprobs?: Generated.ChatMessageTokenLogprobs }

export declare class ChatStreamingResponseChunk extends ChatStreamingResponseChunk_base {}
// { id?: string; model?: `${string}/${string}`; provider?: string;
//   created: DateTime.Utc;                          // Schema.DateTimeUtcFromNumber
//   choices: ReadonlyArray<ChatStreamingChoice>;
//   error?: { code: string | number | null; message: string; param?: string; type?: string };
//   system_fingerprint?: string;
//   usage?: Generated.ChatGenerationTokenUsage }
```

`finish_reason` and `native_finish_reason` are distinct: the former is the normalized OpenRouter enum, the latter the raw upstream-provider reason string. The chunk-level `error` slot is the in-band streaming-error carrier — a non-throwing failure surfaces here rather than on the `AiError` channel.

---

## [2] — OpenRouterLanguageModel

Module `@effect/ai-openrouter/OpenRouterLanguageModel`. Binds OpenRouter chat onto `@effect/ai`'s provider-agnostic `LanguageModel`/`Model` contracts. The provider id literal is `"openrouter"`.

### Config tag + request-config carrier

`Config` is a `Context.TagClass` request-scoped override carrier (not the transport config). `Config.Service` is the full `ChatGenerationParams.Encoded` shape minus the five model-owned/structural keys, so per-request callers can override routing/sampling/reasoning without rebuilding the layer.

```ts
declare const Config_base: Context.TagClass<Config, "@effect/ai-openrouter/OpenRouterLanguageModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<typeof Config.Service | undefined>
}

export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<
    typeof Generated.ChatGenerationParams.Encoded,
    "messages" | "response_format" | "tools" | "tool_choice" | "stream"
  >>> {}
}
```

### Entry points

The `model` argument is a plain `string` (no widened-literal `(string & {}) | Model` enum union as in the OpenAI sibling — OpenRouter model ids are open-ended `"author/slug"` strings); `config` always omits `model`. There is no `modelWithTokenizer`/`layerWithTokenizer` (no tokenizer module).

```ts
export declare const model: (
  model: string,
  config?: Omit<Config.Service, "model">
) => AiModel.Model<"openrouter", LanguageModel.LanguageModel, OpenRouterClient>

export declare const make: (options: {
  readonly model: string
  readonly config?: Omit<Config.Service, "model">
}) => Effect.Effect<LanguageModel.Service, never, OpenRouterClient>

export declare const layer: (options: {
  readonly model: string
  readonly config?: Omit<Config.Service, "model">
}) => Layer.Layer<LanguageModel.LanguageModel, never, OpenRouterClient>

export declare const withConfigOverride: {
  (config: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, config: Config.Service): Effect.Effect<A, E, R>
}
```

### Provider-metadata projection

```ts
export type OpenRouterReasoningInfo =
  | { readonly type: "reasoning"; readonly signature: string | undefined }
  | { readonly type: "encrypted_reasoning"
      readonly format: typeof Generated.ReasoningDetailSummary.Type["format"]  // the reasoning-format enum below
      readonly redactedData: string }
```

### `declare module` augmentations

The module augments `@effect/ai/Prompt` and `@effect/ai/Response` to attach OpenRouter-specific `providerOptions.openrouter` / `providerMetadata.openrouter` slots. Internal code reads these through the canonical `@effect/ai` Prompt/Response shapes; this catalogue records the exact slots.

```ts
// declare module "@effect/ai/Prompt"  — every message/part options interface gains an `openrouter?` slot:
//   SystemMessageOptions, UserMessageOptions, AssistantMessageOptions, ToolMessageOptions,
//   TextPartOptions, ReasoningPartOptions, ToolResultPartOptions
//     -> { cacheControl?: typeof Generated.CacheControlEphemeral.Encoded }
//   FilePartOptions
//     -> { fileName?: string; cacheControl?: typeof Generated.CacheControlEphemeral.Encoded }

// declare module "@effect/ai/Response"  — provider-metadata slots:
//   ReasoningPartMetadata, ReasoningStartPartMetadata, ReasoningDeltaPartMetadata
//     -> { openrouter?: OpenRouterReasoningInfo }
//   UrlSourcePartMetadata
//     -> { openrouter?: { content?: string } }
//   FinishPartMetadata
//     -> { openrouter?: {
//            provider?: string
//            usage?: {
//              cost?: number
//              costDetails?: { upstream_inference_cost?: number }
//              promptTokensDetails?: { audio_tokens?: number; cached_tokens?: number }
//              completionTokensDetails?: {
//                reasoning_tokens?; audio_tokens?; accepted_prediction_tokens?; rejected_prediction_tokens?: number }
//            }
//          } }
```

The single canonical prompt-caching hook is `providerOptions.openrouter.cacheControl` (`CacheControlEphemeral`), and the single canonical cost/usage projection is `providerMetadata.openrouter.usage` on the finish part. The `FinishPartMetadata.usage.cost` field is OpenRouter's distinguishing feature — per-request dollar cost is surfaced through provider metadata, not the token-usage struct.

---

## [3] — OpenRouterConfig

Module `@effect/ai-openrouter/OpenRouterConfig`. The per-effect client-transform override carrier — the one surface for mutating the underlying `HttpClient` per-request without rebuilding the transport layer. Identical in shape to the OpenAI/Anthropic `*Config` siblings.

```ts
declare const OpenRouterConfig_base: Context.TagClass<OpenRouterConfig, "@effect/ai-openrouter/OpenRouterConfig", OpenRouterConfig.Service>
export declare class OpenRouterConfig extends OpenRouterConfig_base {
  static readonly getOrUndefined: Effect.Effect<typeof OpenRouterConfig.Service | undefined>
}

export declare namespace OpenRouterConfig {
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

## [4] — Generated (OpenAPI-derived schema corpus)

`Generated` is the machine-generated module covering OpenRouter's full REST surface: 348 exported `Schema.Class` / `Schema.Literal` owners, one `Client` interface (37 endpoint methods), and one `ClientError<Tag, E>` discriminated-failure carrier. It is not enumerated exhaustively here — planning pages reference it through the named anchors the owner modules expose. The load-bearing anchors (verbatim names):

```ts
// request/response shapes consumed by OpenRouterClient.Service
export declare class ChatGenerationParams      // chat request; Config.Service derives from .Encoded minus messages/response_format/tools/tool_choice/stream
export declare class ChatResponse              // non-streaming chat response (object: "chat.completion")
export declare class ChatResponseChoice        // { finish_reason; index; message: AssistantMessage; logprobs? }
export declare class ChatGenerationTokenUsage  // token + cost usage struct (see below)
export declare class ChatError                 // the chat-endpoint error body (ClientError<"ChatError", …>)
export declare class ModelName                 // extends Schema.String — open-ended "author/slug" model id (no enum)

// reasoning algebra (consumed by ChatStreamingMessageChunk.reasoning_details + OpenRouterReasoningInfo)
export declare class ReasoningDetail           // Schema.Union<[ReasoningDetailSummary, ReasoningDetailEncrypted, ReasoningDetailText]>
export declare class ReasoningDetailSummary    // { type: "reasoning.summary";   summary: string; id?; index?; format? }
export declare class ReasoningDetailEncrypted  // { type: "reasoning.encrypted"; data: string;    id?; index?; format? }
export declare class ReasoningDetailText       // { type: "reasoning.text";       text?; signature?; id?; index?; format? }
export declare class OpenResponsesReasoningFormat
//   the `format` enum: "unknown" | "openai-responses-v1" | "azure-openai-responses-v1" | "xai-responses-v1" | "anthropic-claude-v1" | "google-gemini-v1"

// annotation algebra (consumed by ChatStreamingMessageChunk.annotations)
export declare class AnnotationDetail          // Schema.Union<[FileAnnotationDetail, URLCitationAnnotationDetail]>
export declare class FileAnnotationDetail      // { type: "file"; file: { hash; name?; content: ({type:"text";text} | {type:"image_url";image_url:{url}})[] } }
export declare class URLCitationAnnotationDetail
//   { type: "url_citation"; url_citation: { end_index; start_index: number; title; url: string; content? } }

// content + finish + caching + logprobs anchors
export declare class ChatMessageContentItemImage  // { type: "image_url"; image_url: { url: string; detail?: "auto"|"high"|"low" } }
export declare class ChatCompletionFinishReason   // Schema.Literal<["tool_calls", "stop", "length", "content_filter", "error"]>
export declare class ChatMessageTokenLogprobs
export declare class CacheControlEphemeral        // { type: "ephemeral" } — the prompt-cache breakpoint marker
export declare class AssistantMessage             // ChatResponseChoice.message shape

// alternate API-surface request/response families (reachable only via raw client.*):
//   OpenResponsesRequest / OpenResponsesNonStreamingResponse        (OpenAI-Responses-compatible endpoint)
//   AnthropicMessagesRequest / AnthropicMessagesResponse            (Anthropic-Messages-compatible endpoint)
//   CompletionCreateParams / CompletionResponse                     (legacy text-completions endpoint)
//   CreateEmbeddingsRequest / CreateEmbeddings200                   (embeddings endpoint — no model module binds it)
```

### ChatGenerationTokenUsage

The cost-bearing usage struct returned on both `ChatResponse.usage` and `ChatStreamingResponseChunk.usage`. OpenRouter extends the standard OpenAI token-usage shape with `cost`/`cost_details` (dollar accounting) and `cache_write_tokens`/`video_tokens`:

```ts
// ChatGenerationTokenUsage.Type
// {
//   completion_tokens: number; prompt_tokens: number; total_tokens: number
//   completion_tokens_details?: { reasoning_tokens?; audio_tokens?; accepted_prediction_tokens?; rejected_prediction_tokens?: number }
//   prompt_tokens_details?:     { cached_tokens?; cache_write_tokens?; audio_tokens?; video_tokens?: number }
//   cost?: number
//   cost_details?: { upstream_inference_cost?: number }
// }
```

### ChatGenerationParams (request shape)

Top-level `.Encoded` keys (the full request; `Config.Service` is `Partial<Omit<…, messages|response_format|tools|tool_choice|stream>>`):

```
provider, plugins, route, user, session_id, messages, model, models,
frequency_penalty, logit_bias, logprobs, top_logprobs, max_completion_tokens, max_tokens,
metadata, presence_penalty, reasoning, response_format, seed, stop, stream, stream_options,
temperature, tool_choice, tools, top_p, debug, image_config, modalities
```

`provider` is the OpenRouter routing-preference struct (`allow_fallbacks`, `require_parameters`, `data_collection: "deny"|"allow"`, `zdr`, `order`, `only`, `ignore`, `quantizations`, `sort`, throughput/latency cutoffs); `models` is the fallback-model array; `route` selects the routing strategy. These three fields plus `plugins` are the OpenRouter-distinguishing request controls, all reachable through `Config.Service` overrides.

### Generated.Client (REST surface)

A 37-method typed client interface. Each method returns `Effect.Effect<typeof <ResponseSchema>.Type, HttpClientError.HttpClientError | ParseError | ClientError<…>>`, where the `ClientError` arms enumerate the per-endpoint typed HTTP-status failure bodies. Spelling convention is the OpenAPI `operationId` (camelCase string key). The curated `createChatCompletion`/`createChatCompletionStream` on `OpenRouterClient.Service` wrap `sendChatCompletionRequest`; raw `client.*` is the escape hatch for the other 36 endpoints.

```ts
export interface Client {
  readonly httpClient: HttpClient.HttpClient
  // --- chat / completions / responses / messages (generation surfaces) ---
  readonly "sendChatCompletionRequest": (options: typeof ChatGenerationParams.Encoded) => Effect.Effect<typeof ChatResponse.Type, HttpClientError.HttpClientError | ParseError | ClientError<"ChatError", typeof ChatError.Type>>
  readonly "createCompletions": (options: typeof CompletionCreateParams.Encoded) => Effect.Effect<typeof CompletionResponse.Type, …ClientError<"ChatError", …>>
  readonly "createResponses": (options: typeof OpenResponsesRequest.Encoded) => Effect.Effect<typeof OpenResponsesNonStreamingResponse.Type, …13 ClientError arms>
  readonly "createMessages": (options: typeof AnthropicMessagesRequest.Encoded) => Effect.Effect<typeof AnthropicMessagesResponse.Type, …8 CreateMessages4xx/5xx arms>
  readonly "createEmbeddings": (options: typeof CreateEmbeddingsRequest.Encoded) => Effect.Effect<typeof CreateEmbeddings200.Type, …>
  // --- models / endpoints / providers (catalog surfaces) ---
  readonly "getModels": (options?: typeof GetModelsParams.Encoded | undefined) => Effect.Effect<typeof ModelsListResponse.Type, …>
  readonly "listModelsUser": () => Effect.Effect<typeof ModelsListResponse.Type, …>
  readonly "listModelsCount": () => Effect.Effect<typeof ModelsCountResponse.Type, …>
  readonly "listEmbeddingsModels": () => Effect.Effect<typeof ModelsListResponse.Type, …>
  readonly "listEndpoints": (author: string, slug: string) => Effect.Effect<typeof ListEndpoints200.Type, …>
  readonly "listEndpointsZdr": () => Effect.Effect<typeof ListEndpointsZdr200.Type, …>
  readonly "listProviders": () => Effect.Effect<typeof ListProviders200.Type, …>
  readonly "getGeneration": (options: typeof GetGenerationParams.Encoded) => Effect.Effect<typeof GetGeneration200.Type, …>
  // --- account / credits / activity ---
  readonly "getCredits": () => Effect.Effect<typeof GetCredits200.Type, …>
  readonly "getUserActivity": (options?: typeof GetUserActivityParams.Encoded | undefined) => Effect.Effect<typeof GetUserActivity200.Type, …>
  readonly "createCoinbaseCharge": (options: typeof CreateChargeRequest.Encoded) => Effect.Effect<typeof CreateCoinbaseCharge200.Type, …>
  // --- API-key provisioning (provisioning-key endpoints) ---
  readonly "list" / "createKeys" / "getKey" / "deleteKeys" / "updateKeys" / "getCurrentKey"
  readonly "exchangeAuthCodeForAPIKey" / "createAuthKeysCode"   // PKCE flow
  // --- guardrails + assignments (provisioning-key endpoints) ---
  readonly "listGuardrails" / "createGuardrail" / "getGuardrail" / "deleteGuardrail" / "updateGuardrail"
  readonly "listKeyAssignments" / "listMemberAssignments"
  readonly "listGuardrailKeyAssignments" / "listGuardrailMemberAssignments"
  readonly "bulkAssignKeysToGuardrail" / "bulkAssignMembersToGuardrail"
  readonly "bulkUnassignKeysFromGuardrail" / "bulkUnassignMembersFromGuardrail"
}

export interface ClientError<Tag extends string, E> {
  readonly _tag: Tag
  readonly request: HttpClientRequest.HttpClientRequest
  readonly response: HttpClientResponse.HttpClientResponse
  readonly cause: E
}
export declare const ClientError: <Tag extends string, E>(tag: Tag, cause: E, response: HttpClientResponse.HttpClientResponse) => ClientError<Tag, E>
```

The typed HTTP-status failure bodies (`BadRequestResponse`, `UnauthorizedResponse`, `PaymentRequiredResponse`, `NotFoundResponse`, `RequestTimeoutResponse`, `PayloadTooLargeResponse`, `UnprocessableEntityResponse`, `TooManyRequestsResponse`, `InternalServerResponse`, `BadGatewayResponse`, `ServiceUnavailableResponse`, `EdgeNetworkTimeoutResponse`, `ProviderOverloadedResponse`, `ForbiddenResponse`, plus the Anthropic-endpoint `CreateMessages4xx/5xx` family) are each a `Schema.Class` with an `error` data payload; they reach domain code only through the raw `client.*` rail, since the curated `OpenRouterClient.Service` methods erase them to `AiError.AiError`.

---

## [STACKING] — how the OpenRouter delta composes onto the shared rails

- Composition root (one arm of the `ai.md` `AiProvider` literal, `effect-ai.md` `STACKING_LAW`): `HttpClient.HttpClient` (`NodeHttpClient.layer`, `effect-platform-node.md`, node tier) → `OpenRouterClient.layer({ apiKey: Redacted, referrer?, title? })` / `layerConfig(... Config.Config<Redacted>)` resolving through the `security/secret#SECRET_STORE` `SecretStore` → `OpenRouterLanguageModel.layer({ model: "author/slug" })` → core `LanguageModel.LanguageModel`. Domain code calls the free `LanguageModel.generateText`/`streamText`.
- Aggregator routing rides `Config.Service` overrides (the `Config` tag or per-request `withConfigOverride`, `effect-ai.md` `MODEL_RESOLUTION_LAW`): `provider` (fallback/data-collection/ordering/ZDR), `models` (fallback array), `route`, `plugins` — the OpenRouter-distinguishing request axis the core `GenerateTextOptions` cannot express, since they are provider-request fields, not model options.
- Streaming folds the `ChatStreamingResponseChunk` hierarchy under `Stream.runForEach` (`effect.md`), walking `chunk.choices[i].delta` and `chunk.usage`; the in-band `chunk.error` slot is a non-throwing streaming failure, distinct from the `AiError.AiError` channel the curated methods erase transport failures to.
- Cost + caching deltas: `providerMetadata.openrouter.usage.cost` on the settled `finish` part is per-request dollar accounting (absent from the token-usage struct) — feed it into `execution/slo#SLO` cost objectives; `providerOptions.openrouter.cacheControl` (`CacheControlEphemeral`) marks the reusable-content breakpoint on any message/part; reasoning continuity rides `OpenRouterReasoningInfo` on the reasoning `*PartMetadata`, all read through the canonical `@effect/ai` `Prompt`/`Response` shapes.
