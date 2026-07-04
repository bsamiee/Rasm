# [@effect/ai-google] — the Google provider Layer family: LanguageModel/EmbeddingModel rows on the capability-asymmetry table

`@effect/ai-google` 0.15.0 · MIT · dual CJS+ESM, `sideEffects:[]`, per-module `exports` subpaths (`@effect/ai-google/GoogleClient`) · marker TSDECL `node_modules/@effect/ai-google/dist/dts/*.d.ts` · peers `@effect/ai@^0.36`, `@effect/platform@^0.96`, `@effect/experimental@^0.60`, `effect@^3.21` · tier node|browser (platform-neutral; depends only on `@effect/ai`, `@effect/platform`, `effect`)

The Google Generative AI (Gemini) binding onto `@effect/ai`: it resolves the provider-agnostic `LanguageModel`/`Tool` tags against the Gemini **generateContent** API. Its asymmetry row is the leanest of the admitted providers — a language model and four provider-defined tools, but **no** curated embedding, tokenizer, or telemetry owner module; embeddings and token-counting exist only through the raw `Generated.Client` (`EmbedContent`/`BatchEmbedContents`/`CountTokens`), so `embed/embedder.ts` cannot bind a Google `EmbeddingModel` the way it binds OpenAI, and the design routes Google embeddings through the low-level client. Five owner modules re-export through the barrel (`GoogleClient`, `GoogleConfig`, `GoogleLanguageModel`, `GoogleTool`, `Generated`); the Gemini wire corpus (`Generated`) is a category with named anchors. Success/failure flows through the core `AiError.AiError`; all I/O is `Effect`/`Stream`.

## [01]-[ASYMMETRY] — the row `model/provider.ts` folds

| [COLUMN]              | [Google]                                  | openai       | anthropic    | bedrock          |
| :-------------------- | :---------------------------------------- | :----------- | :----------- | :--------------- |
| provider id           | `"google"`                                | openai       | anthropic    | amazon-bedrock   |
| language model        | `GoogleLanguageModel` (generateContent)   | Responses    | Messages     | Converse         |
| embedding model       | raw client only (`EmbedContent`)          | curated ×2   | —            | —                |
| tokenizer             | — (raw `CountTokens` only)                | `make({model})` | value     | —                |
| provider-defined tools | 4 (`GoogleTool`)                          | 4            | 5 families   | 8                |
| telemetry module      | —                                         | `OpenAiTelemetry` | —        | —                |
| model-id kind         | free-form `string` (no enum)              | enum         | 21-id enum   | 91-id enum       |
| auth                  | `Redacted` apiKey only                    | +org/project | +version     | SigV4 keys       |
| per-request Config    | `GoogleLanguageModel/Config` (+`toolConfig`) | `strict` | `disableParallelToolCalls` | Converse fields |
| streaming fold        | `GenerateContentResponse` re-emit (no distinct union) | 49-member | 8-member | 11-member    |
| `withConfigOverride`  | — (no per-effect override)                | ✓            | ✓            | ✓                |

## [02]-[CLIENT] — transport owner, constructor triple, streaming re-emit

`GoogleClient` is a `Context.TagClass` (id `@effect/ai-google/GoogleClient`) wrapping the generated `Client` plus curated generateContent entrypoints. Distinct from the OpenAI/Anthropic siblings, the streaming surface re-emits the SAME `GenerateContentResponse` schema per chunk rather than a distinct tagged event union — consumers fold `response.candidates[]` deltas, not `event.type`. `streamRequest` decodes arbitrary SSE against a `Schema`.

```ts contract
export interface Service {
  readonly client: Generated.Client
  readonly streamRequest: <A, I, R>(request: HttpClientRequest.HttpClientRequest, schema: Schema.Schema<A, I, R>) => Stream.Stream<A, AiError.AiError, R>
  readonly generateContent:       (request: typeof Generated.GenerateContentRequest.Encoded) => Effect.Effect<Generated.GenerateContentResponse, AiError.AiError>
  readonly generateContentStream: (request: typeof Generated.GenerateContentRequest.Encoded) => Stream.Stream<Generated.GenerateContentResponse, AiError.AiError>
}
```

ONE constructor pattern, three arities over `apiKey`/`apiUrl`/`transformClient` — no `organizationId`/`projectId` (Gemini authenticates by API key alone). `apiUrl` default is asymmetric between arities: `make` → host root, `layer`/`layerConfig` → the `/v1beta` versioned root. `make` requires `HttpClient | Scope`; the layers require `HttpClient`; `layerConfig` adds `ConfigError`.

```ts contract
declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined    // make default "https://generativelanguage.googleapis.com"
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>
declare const layer:       (options: { apiKey?; apiUrl?; transformClient? }) => Layer.Layer<GoogleClient, never, HttpClient.HttpClient>   // apiUrl default ".../v1beta"
declare const layerConfig: (options: { apiKey?: Config.Config<Redacted | undefined>; apiUrl?: Config.Config<string | undefined>; transformClient? }) => Layer.Layer<GoogleClient, ConfigError, HttpClient.HttpClient>
```

## [03]-[LANGUAGE_MODEL] — the row constructor, per-request Config, boundary augmentations

`GoogleLanguageModel` binds generateContent onto the core `LanguageModel`/`Model` contracts. The model argument is `(string & {}) | Model` where `Model = string` — Gemini ships no generated model-id enum, so autocomplete is open. ONE model/layer family, narrower than OpenAI/Anthropic: `model`/`make`/`layer` only — no `modelWithTokenizer`/`layerWithTokenizer` (no tokenizer binding) and no `withConfigOverride` (per-effect override is absent; use the `Config` tag directly).

```ts contract
export type Model = string
declare const model: (model: (string & {}) | Model, config?: Omit<Config.Service, "model">) => AiModel.Model<"google", LanguageModel.LanguageModel, GoogleClient>
declare const make:  (options: { model: (string & {}) | Model; config?: Omit<Config.Service, "model"> }) => Effect.Effect<LanguageModel.Service, never, GoogleClient>
declare const layer: (options: { model; config? }) => Layer.Layer<LanguageModel.LanguageModel, never, GoogleClient>
```

`Config` (tag `@effect/ai-google/GoogleLanguageModel/Config`, `static getOrUndefined`) is the `GenerateContentRequest` minus SDK-owned keys (`contents`/`tools`/`toolConfig`/`systemInstruction`) made partial, with a re-added partial `toolConfig.functionCallingConfig` (the mode-less function-calling policy). It is the tier-routing seam `provider.ts` writes per call.

```ts contract
namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof Generated.GenerateContentRequest.Encoded, "contents"|"tools"|"toolConfig"|"systemInstruction">>> {
    readonly toolConfig: Partial<{ readonly functionCallingConfig: Omit<typeof Generated.FunctionCallingConfig.Encoded, "mode"> }>
  }
}
```

The `declare module` augmentations attach an optional `google` key — ONE boundary-hook pattern. `google.thoughtSignature` is the reasoning-continuity carrier threaded across many part interfaces; `FinishPartMetadata.google` is the single surface aggregating grounding, safety, URL-context, and usage off a finished response — the `safetyRatings` slot is what the `provider.ts` guardrail gate reads for output moderation.

| [AUGMENTS]        | [INTERFACES]                                                                                  | [`google` slot]                                              |
| :---------------- | :-------------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
| `Prompt`          | `Reasoning/Text/ToolCall PartOptions`                                                          | `thoughtSignature?`                                          |
| `Response`        | `Text{Start,Delta}`, `Reasoning{,Start,Delta}`, `ToolParams{Start,Delta}`, `ToolCall` PartMetadata | `thoughtSignature?`                                      |
| `Response`        | `FinishPartMetadata`                                                                           | `groundingMetadata?`, `safetyRatings?`, `urlContextMetadata?`, `usageMetadata?` |

## [04]-[TOOL] — provider-defined tool family as one pattern + roster

`GoogleTool` exports four provider-executed tool constructors (`requiresHandler:false` — the provider runs them), each ONE instance of `<Mode extends Tool.FailureMode | undefined>(args) => Tool.ProviderDefined<"Google<Name>", { …; failureMode: Mode extends undefined ? "error" : Mode }, false>`. Only `CodeExecution` carries model-supplied `parameters` and a real `success` schema; the other three are grounding switches (`EmptyParams`, `Void` success).

| [ctor]                  | [tag]                     | [args]                                | [parameters / success]                              |
| :---------------------- | :------------------------ | :------------------------------------ | :-------------------------------------------------- |
| `CodeExecution`         | `GoogleCodeExecution`     | `{}`                                  | `{ language: ExecutableCodeLanguage; code }` / `CodeExecutionResult` |
| `GoogleSearch`          | `GoogleSearch`            | `{}`                                  | `EmptyParams` / `Void` — grounding for Gemini 2.0+  |
| `GoogleSearchRetrieval` | `GoogleSearchRetrieval`   | `{ mode?; dynamicThreshold? }`        | `EmptyParams` / `Void` — legacy Gemini 1.5 dynamic retrieval |
| `UrlContext`            | `GoogleUrlContext`        | `{}`                                  | `EmptyParams` / `Void`                              |

`GoogleSearchRetrieval.args.mode` is `DynamicRetrievalConfigMode` (`MODE_UNSPECIFIED | MODE_DYNAMIC`), its `dynamicThreshold` (0.0–1.0) gates the model's autonomous decision to search; prefer `GoogleSearch` for Gemini 2.0+. `failure` is `Schema.Never` on all four.

## [05]-[CONFIG] — per-effect HttpClient transform overlay

`GoogleConfig` (`Context.TagClass`, id `@effect/ai-google/GoogleConfig`, `static getOrUndefined`) is the request-scoped client transform — a per-request `HttpClient` mutation without rebuilding transport, dual data-first/data-last. Mirrors `OpenAiConfig.withClientTransform` exactly.

```ts contract
namespace GoogleConfig { interface Service { readonly transformClient?: (client: HttpClient) => HttpClient } }
declare const withClientTransform: { (t: (c: HttpClient) => HttpClient): <A,E,R>(self) => …; <A,E,R>(self, t): … }
```

## [06]-[GENERATED] — Gemini wire corpus as category + anchors

`Generated` is the machine-generated Google Generative AI REST surface: ~238 schema owners (176 `Schema.Class`, 31 `Schema.Literal`, 23 `Schema.Struct`, 8 `Schema.Record`) plus one 80-method `Client` interface. It is not enumerated — the owner modules reach it through named anchors, and planning code reaches REST via `GoogleClient.Service.client.<PascalCaseOperationId>(...)`. Each method returns `Effect.Effect<typeof <Response>.Type, HttpClientError.HttpClientError | ParseError>`. Note the `Generated`-internal client `transformClient` is **effectful** (`(client) => Effect.Effect<HttpClient>`), unlike the synchronous transform on the public `make`/`layer`/`GoogleConfig` surfaces.

Load-bearing anchors (exact spelling): `GenerateContentRequest` (Config derives from its `Encoded`), `GenerateContentResponse`, `FunctionCallingConfig`/`FunctionCallingConfigMode` (`MODE_UNSPECIFIED|AUTO|ANY|NONE|VALIDATED`), `GroundingMetadata`, `SafetyRating`, `UrlContextMetadata`, `UsageMetadata` (the four `FinishPartMetadata.google` payloads), `ExecutableCodeLanguage`, `CodeExecutionResult`, `DynamicRetrievalConfigMode` (the `GoogleTool` sub-schemas), and the Gemini content algebra (`Content`, `Part`, `Blob`, `FileData`, `FunctionCall`, `FunctionResponse`, `ExecutableCode`, `Tool`, `FunctionDeclaration`, plus `Schema`/`Type` — the Gemini function-parameter JSON-schema dialect, NOT `effect/Schema`).

The only curated `Service` entries are `generateContent`/`generateContentStream`; embeddings (`EmbedContent`/`BatchEmbedContents`, anchors `EmbedContentRequest`/`CountTokensRequest`), token-counting (`CountTokens`), context caching (`*CachedContent*`), batches (`BatchGenerateContent`), files, tuned models, and long-running operations (`GetOperation`/`ListOperations`/`CancelOperation`) are reached only through the raw `client.*` rail — there is no high-level `createEmbedding` wrapper.

## [07]-[INTEGRATION] — how the row stacks into single rails

- Universal Effect rails: `GoogleLanguageModel.model(id)` produces the same `LanguageModel.LanguageModel` tag as every sibling — provider choice is a single `Layer` swap in `provider.ts`. `Redacted`+`Config` own credential resolution; `Stream` folds the re-emitted `GenerateContentResponse` (candidate deltas, not a `type`-union); `Schema` decodes `Config`/responses; `Effect.catchTag` branches `AiError`. Compose top-down: `Effect.provide(GoogleLanguageModel.model(id))` over `GoogleClient.layer({ apiKey })` over an `HttpClient` layer.
- `@effect/platform` seam: every `layer*` requires `HttpClient.HttpClient` from the `host/net` default-policy row; platform-neutral, so `FetchHttpClient.layer` (browser) or `NodeHttpClient.layer` (node) both satisfy it.
- `@effect/ai` core (sibling catalog `effect-ai.md`): satisfies `LanguageModel.LanguageModel`, `Tool.ProviderDefined`/`Tool.FailureMode`; augments `Prompt`/`Response` provider slots. No `EmbeddingModel`, `Tokenizer`, or `Telemetry` tag — those asymmetry cells are empty, and any Google embedding/token-count goes through the raw `Generated.Client`, never a curated binding.
- Sibling providers: the leanest row — `provider.ts` reads Google's empty embedding/tokenizer/telemetry cells against the OpenAI reference row and the free-form (enum-less) model-id column.
- Design consumers: `model/provider.ts` (row + tier-routing + the guardrail gate, which reads `FinishPartMetadata.google.safetyRatings` for output moderation), `tool/toolkit.ts`+`tool/mcp.ts` (the four provider-defined tools projected). No `model/token.ts` tokenizer binding and no `embed/embedder.ts` `EmbeddingModel` — both are asymmetry gaps the design fills through the raw client or another provider.
