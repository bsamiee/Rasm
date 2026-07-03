# [@effect/ai-openrouter] — the OpenRouter provider Layer family: a LanguageModel row on the capability-asymmetry table

One of the five provider Layer families `model/provider.ts` folds onto the capability-asymmetry table.
`OpenRouterLanguageModel.model(id)` returns a `Model<"openrouter", LanguageModel, OpenRouterClient>` row —
`Model.make` under the hood, so it is a `Layer` and an `Effect` that resolve `@effect/ai`'s `LanguageModel`
Tag. OpenRouter's asymmetry is aggregation: one API fronts every upstream provider, so the row carries
provider-routing preferences (`ProviderPreferences`, price/throughput/latency sort), reasoning-effort control,
prompt-caching breakpoints (`cacheControl`), and per-response cost + upstream-provider transparency in the
finish-part metadata — exactly the columns tier-routing reads. The client authenticates with a `Redacted`
API key (or `effect/Config`), threads through the `host/net` `HttpClient`, and the whole 348-class OpenRouter
OpenAPI surface is available typed via `Generated` for endpoints the AI abstraction does not cover.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-openrouter`
- package: `@effect/ai-openrouter` (version `0.11.0`, license MIT, `type: module`)
- entry: re-exports namespaces `Generated`, `OpenRouterClient`, `OpenRouterConfig`, `OpenRouterLanguageModel`
- peer floor: `@effect/ai ^0.36.0`, `@effect/platform ^0.96.1`, `@effect/experimental ^0.60.0`, `effect ^3.21.3`
- asset: OpenRouter `LanguageModel` provider Layers + typed OpenRouter OpenAPI client; requires `HttpClient`
- provides: `@effect/ai` `LanguageModel.LanguageModel` Tag (no `EmbeddingModel` — OpenRouter is chat/completions)
- rail: ai-provider

## [02]-[CLIENT]

[PUBLIC_TYPE_SCOPE]: `OpenRouterClient` service + construction — rail: ai-provider

The Tag exposes typed `createChatCompletion`/`createChatCompletionStream` plus the raw `Generated.Client` for
uncovered endpoints. `layer` takes literals; `layerConfig` reads `effect/Config` so the key/URL come from the
environment as a `ConfigError`-typed Layer. `transformClient` composes the `host/net` default-policy `HttpClient`.

```ts contract
declare class OpenRouterClient extends Context.Tag("@effect/ai-openrouter/OpenRouterClient")<OpenRouterClient, Service> {}
interface Service {
  readonly client: Generated.Client // full typed OpenRouter OpenAPI surface
  readonly createChatCompletion: (options: typeof Generated.ChatGenerationParams.Encoded)
    => Effect.Effect<Generated.ChatResponse, AiError.AiError>
  readonly createChatCompletionStream: (options: Omit<typeof Generated.ChatGenerationParams.Encoded, "stream">)
    => Stream.Stream<ChatStreamingResponseChunk, AiError.AiError>
}
declare const make: (o: MakeOptions) => Effect.Effect<Service, never, HttpClient.HttpClient>
declare const layer: (o: MakeOptions) => Layer.Layer<OpenRouterClient, never, HttpClient.HttpClient>
declare const layerConfig: (o: ConfigOptions) => Layer.Layer<OpenRouterClient, ConfigError, HttpClient.HttpClient>
interface MakeOptions {
  readonly apiKey?: Redacted.Redacted        // secrets stay Redacted — never logged
  readonly apiUrl?: string
  readonly referrer?: string                 // openrouter.ai app-attribution ranking header
  readonly title?: string
  readonly transformClient?: (client: HttpClient.HttpClient) => HttpClient.HttpClient
}
// ConfigOptions mirrors MakeOptions with Config.Config<Redacted>/Config.Config<string> fields.

// Streaming chunk schemas (Schema.Class): the routing-transparency asymmetry rides here.
class ChatStreamingResponseChunk { // .provider (upstream), .usage (ChatGenerationTokenUsage), .system_fingerprint, .model
  readonly choices: ReadonlyArray<ChatStreamingChoice> // .delta (ChatStreamingMessageChunk), .finish_reason, .native_finish_reason
}
```

## [03]-[LANGUAGE_MODEL]

[PUBLIC_TYPE_SCOPE]: the provider row + generation config — rail: ai-provider

`model(id, config?)` is the row `provider.ts` selects; `layer`/`make` are the direct forms. `Config` is a
per-request generation-parameter Tag (temperature, top_p, reasoning, provider routing — everything except
messages/tools/response_format/stream); `withConfigOverride` scopes an override onto an Effect. Module
augmentation adds OpenRouter columns to `@effect/ai`'s `Prompt` and `Response` shapes.

```ts contract
// The capability-asymmetry row: a Model that is Layer + Effect, provides LanguageModel, requires OpenRouterClient.
declare const model: (model: string, config?: Omit<Config.Service, "model">)
  => AiModel.Model<"openrouter", LanguageModel.LanguageModel, OpenRouterClient>
declare const make: (o: { readonly model: string; readonly config?: Omit<Config.Service, "model"> })
  => Effect.Effect<LanguageModel.Service, never, OpenRouterClient>
declare const layer: (o: { readonly model: string; readonly config?: Omit<Config.Service, "model"> })
  => Layer.Layer<LanguageModel.LanguageModel, never, OpenRouterClient>

declare class Config extends Context.Tag("@effect/ai-openrouter/OpenRouterLanguageModel/Config")<Config, Config.Service> {}
// Config.Service = Partial<Generated.ChatGenerationParams.Encoded minus messages|response_format|tools|tool_choice|stream>
//   => temperature, top_p, top_k, provider (ProviderPreferences), reasoning (effort), route, models[], transforms, …
declare const withConfigOverride: {
  (config: Config.Service): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, config: Config.Service): Effect.Effect<A, E, R>
}

// Provider metadata (Response augmentation): the tier-routing signal.
type OpenRouterReasoningInfo =
  | { readonly type: "reasoning"; readonly signature: string | undefined }
  | { readonly type: "encrypted_reasoning"; readonly format: ...; readonly redactedData: string }
// FinishPartMetadata.openrouter: { provider?; usage?: { cost?; costDetails?; promptTokensDetails?; completionTokensDetails? } }
// *MessageOptions/*PartOptions.openrouter.cacheControl?: CacheControlEphemeral  (prompt-caching breakpoint)
```

## [04]-[CLIENT_CONFIG]

[PUBLIC_TYPE_SCOPE]: request-scoped HttpClient transform — rail: ai-provider

`OpenRouterConfig` is the ambient client-transform Tag; `withClientTransform` scopes an `HttpClient`
transformation onto an Effect (organization proxy, header injection, per-tenant metrics) without rebuilding the
client Layer.

```ts contract
declare class OpenRouterConfig extends Context.Tag("@effect/ai-openrouter/OpenRouterConfig")<OpenRouterConfig, Service> {}
interface Service { readonly transformClient?: (client: HttpClient) => HttpClient }
declare const withClientTransform: {
  (transform: (client: HttpClient) => HttpClient): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, transform: (client: HttpClient) => HttpClient): Effect.Effect<A, E, R>
}
```

## [05]-[GENERATED]

[PUBLIC_TYPE_SCOPE]: the typed OpenRouter OpenAPI surface — rail: ai-provider

`Generated` is the derived, `Schema.Class`-backed OpenRouter API (348 schemas). It is the low-level escape hatch
`Service.client` exposes and the source of every asymmetry type the config/metadata reference.

| [INDEX] | [SYMBOL]                            | [FAMILY]     | [CAPABILITY]                          |
| :-----: | :---------------------------------- | :----------- | :------------------------------------ |
|  [01]   | `Generated.ChatGenerationParams`    | Schema.Class | full generation request (model/messages/provider/reasoning/route) |
|  [02]   | `Generated.ChatResponse`            | Schema.Class | non-stream completion + usage         |
|  [03]   | `Generated.ChatGenerationTokenUsage`| Schema.Class | prompt/completion/cost token usage    |
|  [04]   | `Generated.ProviderPreferences`     | Schema.Class | upstream routing + sort policy        |
|  [05]   | `Generated.ChatGenerationParamsReasoningEffortEnum` | enum | reasoning-effort control        |
|  [06]   | `Generated.ChatGenerationParamsRouteEnum` | enum   | fallback routing policy               |
|  [07]   | `Generated.CacheControlEphemeral`   | Schema.Class | prompt-caching breakpoint             |
|  [08]   | `Generated.ReasoningDetail*`        | Schema union | reasoning trace parts                 |
|  [09]   | `Generated.Client` + `Generated.make(httpClient, options?)` | client | raw endpoint access + ClientError |

## [06]-[INTEGRATION]

[STACK]: the capability-asymmetry row + tier-routing — rail: ai-provider
- `provider.ts` folds `OpenRouterLanguageModel.model(id, config)` as one row among anthropic/openai/google/
  bedrock/openrouter; the app root provides the row's `Model` Layer plus its `OpenRouterClient.layerConfig`.
  OpenRouter's row is the aggregator lane: `config.provider` (`ProviderPreferences`) sorts upstreams by
  price/throughput/latency, `config.models` supplies fallbacks, `config.route` sets fallback policy, and
  `config.reasoning` sets effort — the tier-routing policy is data on the row, never a new API.
- Cost/latency tier-routing reads `FinishPartMetadata.openrouter.usage.cost` + `.provider` per response to attribute
  spend and pick the next tier; `system_fingerprint`/`native_finish_reason` on the stream chunk disambiguate the
  actual upstream. `cacheControl` breakpoints on message/part options cut input cost on repeated system prefixes.

[STACK]: universal-tier rails — rail: ai-provider
- `effect`: `Redacted` hides the API key end-to-end; `Config` (`layerConfig`) reads it from the environment as a
  typed `ConfigError` Layer; `Schema` types every `Generated` shape; `Stream` carries the streaming chunks.
- `@effect/platform`: the row requires `HttpClient` — `host/net` provides the default-policy client; `transformClient`
  and `OpenRouterConfig.withClientTransform` compose proxy/retry/header policy without a rebuild.
- `@effect/ai`: the row resolves the `LanguageModel` Tag, so `generateText`/`streamText`/`generateObject`, the
  guardrail gate, toolkits, and telemetry all compose unchanged; `Model.ProviderName` reads `"openrouter"` for routing.
- `@effect/opentelemetry`: `Telemetry.addGenAIAnnotations` records `system: "openrouter"` + usage on the span the
  `telemetry` folder exports.
