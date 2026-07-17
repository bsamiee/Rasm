# [TS_RUNTIME_API_EFFECT_AI_OPENROUTER]

`@effect/ai-openrouter` catalog · MIT · dual CJS+ESM, `sideEffects:[]`, per-module `exports` subpaths (`@effect/ai-openrouter/OpenRouterClient`) · marker TSDECL `node_modules/@effect/ai-openrouter/dist/dts/*.d.ts` · peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect` through catalog ownership · tier node|browser (`FetchHttpClient.layer`)

`@effect/ai-openrouter` binds the OpenRouter aggregation API onto `@effect/ai`, resolving the provider-agnostic `LanguageModel` tag against OpenRouter's chat-completions surface. Its asymmetry is aggregation: one endpoint fronts every upstream provider, so the row carries provider-routing preferences (`ProviderPreferences` sorting by price/throughput/latency), reasoning-effort control, prompt-caching breakpoints (`cacheControl`), and per-response cost plus resolved-upstream transparency in the finish-part metadata — the exact columns tier-routing reads. It ships no embedding, tokenizer, telemetry, or provider-defined-tool module (tools ride the upstream provider), so `ai/embed.ts` never binds an OpenRouter `EmbeddingModel`. Four owner modules re-export through the barrel (`OpenRouterClient`, `OpenRouterConfig`, `OpenRouterLanguageModel`, `Generated`); the OpenAPI wire corpus (`Generated`) is a category with named anchors. Success/failure flows through the core `AiError.AiError`; all I/O is `Effect`/`Stream`.

## [01]-[ASYMMETRY]

| [INDEX] | [COLUMN]               | [OPENROUTER]                            | [OPENAI]             | [ANTHROPIC]                | [GOOGLE]         |
| :-----: | :--------------------- | :-------------------------------------- | :------------------- | :------------------------- | :--------------- |
|  [01]   | provider id            | `"openrouter"`                          | openai               | anthropic                  | google           |
|  [02]   | language model         | `OpenRouterLanguageModel` (completions) | Responses            | Messages                   | generateContent  |
|  [03]   | embedding model        | — (chat/completions only)               | batched+DL           | —                          | raw client       |
|  [04]   | tokenizer              | —                                       | `make({model})`      | value                      | —                |
|  [05]   | provider-defined tools | — (upstream-owned)                      | 4                    | 5 families                 | 4                |
|  [06]   | telemetry module       | —                                       | `OpenAiTelemetry`    | —                          | —                |
|  [07]   | model-id kind          | free `string` (`provider/model`)        | enum                 | 21-id enum                 | free `string`    |
|  [08]   | auth                   | `Redacted` apiKey + referrer/title      | + org/project        | + version                  | apiKey           |
|  [09]   | per-request Config     | `Config` + provider routing             | `strict`/`verbosity` | `disableParallelToolCalls` | `toolConfig`     |
|  [10]   | streaming fold         | `ChatStreamingResponseChunk` re-emit    | 49-member            | 8-member                   | response re-emit |
|  [11]   | aggregation            | `ProviderPreferences` sort + cost       | —                    | —                          | —                |

## [02]-[CLIENT]

`OpenRouterClient` is a `Context.TagClass` (id `@effect/ai-openrouter/OpenRouterClient`) wrapping the generated `Client` plus curated chat-completion entrypoints. `createChatCompletion`/`createChatCompletionStream` target the `ChatGenerationParams` request; distinct from the siblings, no `streamRequest` escape hatch exists — the streaming rail re-emits `ChatStreamingResponseChunk` per chunk, and `referrer`/`title` set the `openrouter.ai` app-attribution ranking headers.

```ts signature
export interface Service {
  readonly client: Generated.Client
  readonly createChatCompletion:       (options: typeof Generated.ChatGenerationParams.Encoded) => Effect.Effect<Generated.ChatResponse, AiError.AiError>
  readonly createChatCompletionStream: (options: Omit<typeof Generated.ChatGenerationParams.Encoded, "stream">) => Stream.Stream<ChatStreamingResponseChunk, AiError.AiError>
}
```

ONE constructor pattern, three arities. Distinct from the siblings, `make` returns a non-scoped `Effect` requiring `HttpClient` alone — no `Scope`. `layer`/`layerConfig` install the tag; `layerConfig` wraps each option in `Config.Config` and adds `ConfigError`. `referrer`/`title` ride every arity as the `openrouter.ai` ranking headers.

```ts signature
declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined
  readonly referrer?: string | undefined     // openrouter.ai app-attribution ranking header
  readonly title?: string | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient>            // no Scope, unlike the sibling make
declare const layer:       (options: { apiKey?; apiUrl?; referrer?; title?; transformClient? }) => Layer.Layer<OpenRouterClient, never, HttpClient.HttpClient>
declare const layerConfig: (options: { each field Config.Config<…> })                          => Layer.Layer<OpenRouterClient, ConfigError, HttpClient.HttpClient>
```

`ChatStreamingResponseChunk` is the ONE streaming-fold surface — re-emitted per chunk, no tagged event union (unlike OpenAI/Anthropic). Consumers fold `chunk.choices[].delta` deltas; the aggregation transparency rides the chunk: `provider` names the resolved upstream, `usage` carries `ChatGenerationTokenUsage` (cost included), `system_fingerprint` and each choice's `native_finish_reason` disambiguate the true upstream, and `model` is the `${provider}/${model}` id.

```ts signature
class ChatStreamingResponseChunk {   // .id, .created (DateTimeUtc), .error?
  readonly model?: `${string}/${string}`
  readonly provider?: string
  readonly system_fingerprint?: string
  readonly usage?: Generated.ChatGenerationTokenUsage
  readonly choices: ReadonlyArray<ChatStreamingChoice>   // .index, .delta?, .finish_reason?, .native_finish_reason?, .logprobs?
}
// ChatStreamingChoice.delta = ChatStreamingMessageChunk: role? content? reasoning? reasoning_details?(ReasoningDetail union) images? refusal? tool_calls? annotations?
```

## [03]-[LANGUAGE_MODEL]

`OpenRouterLanguageModel` binds chat completions onto the core `LanguageModel`/`Model` contracts. Model argument is a bare `string` — OpenRouter ships no model-id enum, and every id is a free-form `${provider}/${model}` slug. ONE model/layer family: `model`/`make`/`layer` plus `withConfigOverride` (the dual per-effect Config scope); no tokenizer fold, no provider-tool constructors.

```ts signature
declare const model: (model: string, config?: Omit<Config.Service, "model">) => AiModel.Model<"openrouter", LanguageModel.LanguageModel, OpenRouterClient>
declare const make:  (options: { model: string; config?: Omit<Config.Service, "model"> }) => Effect.Effect<LanguageModel.Service, never, OpenRouterClient>
declare const layer: (options: { model: string; config? }) => Layer.Layer<LanguageModel.LanguageModel, never, OpenRouterClient>
declare const withConfigOverride: { (o: Config.Service): <A,E,R>(self) => …; <A,E,R>(self, o): … }
```

`Config` (tag `@effect/ai-openrouter/OpenRouterLanguageModel/Config`, `static getOrUndefined`) is the `ChatGenerationParams` minus SDK-owned keys (`messages`/`response_format`/`tools`/`tool_choice`/`stream`) made partial — the tier-routing seam `ai/model.ts` writes per call. Its aggregation knobs (`provider`, `reasoning`, `route`, `models`) are the routing policy as data.

```ts signature
namespace Config { interface Service extends Simplify<Partial<Omit<typeof Generated.ChatGenerationParams.Encoded, "messages"|"response_format"|"tools"|"tool_choice"|"stream">>> {} }
// carries provider (ProviderPreferences), reasoning (ChatGenerationParamsReasoningEffortEnum), route (ChatGenerationParamsRouteEnum), models[], temperature, top_p, seed, stop, max_tokens, frequency_penalty, …
```

`declare module` augmentations attach an optional `openrouter` key — ONE boundary-hook pattern. Prompt caching is the dominant one: every message/part options interface gains `cacheControl?: CacheControlEphemeral.Encoded`; the finish metadata carries per-response cost and the resolved upstream.

| [INDEX] | [AUGMENTS] | [INTERFACES]                                | [OPENROUTER_SLOT]            |
| :-----: | :--------- | :------------------------------------------ | :--------------------------- |
|  [01]   | `Prompt`   | `System/User/Assistant/Tool MessageOptions` | `cacheControl?`              |
|  [02]   | `Prompt`   | `Text/Reasoning/ToolResult PartOptions`     | `cacheControl?`              |
|  [03]   | `Prompt`   | `FilePartOptions`                           | `fileName?`, `cacheControl?` |
|  [04]   | `Response` | `Reasoning{,Start,Delta}PartMetadata`       | `OpenRouterReasoningInfo`    |
|  [05]   | `Response` | `UrlSourcePartMetadata`                     | `{ content? }`               |
|  [06]   | `Response` | `FinishPartMetadata`                        | `{ provider?; usage? }`      |

```ts signature
// FinishPartMetadata.openrouter.usage: { cost?; costDetails?; promptTokensDetails?; completionTokensDetails? } — the cost/latency tier-routing signal
export type OpenRouterReasoningInfo =
  | { readonly type: "reasoning";           readonly signature: string | undefined }
  | { readonly type: "encrypted_reasoning"; readonly format: typeof Generated.ReasoningDetailSummary.Type["format"]; readonly redactedData: string }
```

## [04]-[CONFIG]

`OpenRouterConfig` (`Context.TagClass`, id `@effect/ai-openrouter/OpenRouterConfig`, `static getOrUndefined`) is the request-scoped client transform — a per-request `HttpClient` mutation without rebuilding transport, dual data-first/data-last. Distinct from the layer-construction `transformClient`.

```ts signature
namespace OpenRouterConfig { interface Service { readonly transformClient?: (client: HttpClient) => HttpClient } }
declare const withClientTransform: { (t: (c: HttpClient) => HttpClient): <A,E,R>(self) => …; <A,E,R>(self, t): … }
```

## [05]-[GENERATED]

`Generated` is the machine-generated OpenRouter REST surface: exported owners (`Schema.Class` wire schemas, `Schema.Literal` enums, `Schema.Union` families) plus the `make` factory, a `Client` interface, and a per-status `ClientError` rail richer than the siblings' — every method's error channel unions typed HTTP-status responses (`BadRequestResponse`, `ProviderOverloadedResponse`, `EdgeNetworkTimeoutResponse`, …) beside `HttpClientError | ParseError`. As the aggregator, the raw client fronts each upstream's native request format: `createChatCompletion` (OpenAI chat), `createResponses` (`OpenResponsesRequest`), and `createMessages` (`AnthropicMessagesRequest`), plus account endpoints (`getCredits`, `getUserActivity`). Planning code composes by `typeof X.Encoded` (wire) / `typeof X.Type` (decoded) and reaches REST via `OpenRouterClient.Service.client.<operationId>(...)`.

Load-bearing anchors (exact spelling): `ChatGenerationParams` (Config derives from its `Encoded`), `ChatResponse`, `ChatGenerationTokenUsage` (prompt/completion/cost usage), `ProviderPreferences` (upstream routing + sort policy), `ChatGenerationParamsReasoningEffortEnum` (`xhigh|high|medium|low|minimal|none`), `ChatGenerationParamsRouteEnum` (recovery routing), `CacheControlEphemeral` (caching breakpoint), and the `ReasoningDetail` family (`ReasoningDetailSummary`/`ReasoningDetailEncrypted`/`ReasoningDetailText`).

## [06]-[INTEGRATION]

- Universal Effect rails: `OpenRouterLanguageModel.model(id)` produces the same `LanguageModel.LanguageModel` tag as every sibling — provider choice is a single `Layer` swap in `ai/model.ts`. `Redacted`+`Config` own credential resolution; `Stream` folds the re-emitted `ChatStreamingResponseChunk` (candidate deltas, not a `type`-union); `Schema` decodes `Config`/responses; `Effect.catchTag` branches `AiError`. Compose top-down: `Effect.provide(OpenRouterLanguageModel.model(id))` over `OpenRouterClient.layerConfig({ apiKey })` over an `HttpClient` layer.
- `@effect/platform` seam: every `layer*` requires `HttpClient.HttpClient` from the `net/client` default-policy row; platform-neutral, so `FetchHttpClient.layer` (browser) or `NodeHttpClient.layer` (node) both satisfy it; `transformClient` and `OpenRouterConfig.withClientTransform` compose proxy/retry/header policy without a rebuild.
- `@effect/ai` core (sibling catalog `effect-ai.md`): satisfies `LanguageModel.LanguageModel`, so `generateText`/`streamText`/`generateObject`, the guardrail gate, and toolkits compose unchanged; augments `Prompt`/`Response` provider slots; `Model.ProviderName` reads `"openrouter"`. No `EmbeddingModel`, `Tokenizer`, `Telemetry`, or provider-tool tag — those asymmetry cells are empty.
- Sibling providers: the aggregator lane — `ai/model.ts` reads OpenRouter's empty embedding/tokenizer/telemetry/tool cells against the OpenAI reference row; cost/latency tier-routing reads `FinishPartMetadata.openrouter.usage.cost` + `.provider` per response to attribute spend and pick the next tier, and `cacheControl` breakpoints cut input cost on repeated system prefixes.
- Design consumers: `ai/model.ts` (row + tier-routing via `Config`'s `provider`/`route`/`models`/`reasoning` policy + the guardrail gate). No `ai/embed.ts` binding and no tokenizer or tool projection — asymmetry gaps the design fills through another provider.
