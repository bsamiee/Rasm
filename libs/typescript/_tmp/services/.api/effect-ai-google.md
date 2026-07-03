# [API_CATALOGUE] effect-ai-google

Grounded from installed `node_modules` type declarations (`@effect/ai-google` `0.15.0`, MIT, `dist/dts/*.d.ts`; peers `@effect/ai` `0.36.0`, `@effect/platform`, `effect` `3.21.4`; ESM, node `>=24`). Covers every load-bearing public surface the TS planning pages consume — the five owner modules re-exported from the package root. The `Generated` module (238 Gemini-OpenAPI-derived schema owners — 176 `Schema.Class`, 31 `Schema.Literal`, 23 `Schema.Struct`, 8 `Schema.Record` — plus an 80-method REST `Client`) is summarized as a category, not enumerated; only the named anchors the owner modules reference are spelled exactly. Sibling provider packages (`@effect/ai`, `@effect/ai-openai`, `@effect/ai-anthropic`) share the same `LanguageModel`/`Tool` contracts — this package is the Google Generative AI (Gemini) binding. Tier: `node` (admitted under the node-durable bundle alongside the other `@effect/ai*` provider layers); the package is platform-neutral by construction (depends only on `@effect/ai`, `@effect/platform`, and `effect`) but is routed through the node-tier driver host (node HTTP binding `NodeHttpClient.layer`, browser `FetchHttpClient.layer`). It resolves through the shared adapter seam — `effect-ai.md` `PROVIDER_RESOLUTION`/`STACKING_LAW` own the composition-root layer stack (`HttpClient` → `GoogleClient.layer` → `GoogleLanguageModel.model` → core `LanguageModel.LanguageModel`, one arm of the `ai.md` `AiProvider` literal axis); this page documents only Google's DELTA surface (the narrower auth/config set, `thoughtSignature` reasoning continuity, grounding/safety/URL-context finish metadata, the four provider-defined tools, and the raw-`client`-only embeddings/token-counting).

Package root re-exports (namespace barrels):

```ts
// @effect/ai-google
export * as Generated from "./Generated.js"
export * as GoogleClient from "./GoogleClient.js"
export * as GoogleConfig from "./GoogleConfig.js"
export * as GoogleLanguageModel from "./GoogleLanguageModel.js"
export * as GoogleTool from "./GoogleTool.js"
```

This package is materially narrower than `@effect/ai-openai`: there is **no** `GoogleEmbeddingModel`, `GoogleTokenizer`, or `GoogleTelemetry` owner module. Embeddings and token-counting are reached only through the raw `Generated.Client` REST methods (`EmbedContent`, `BatchEmbedContents`, `CountTokens`); there is no curated `EmbeddingModel`/`Tokenizer` binding. The provider exposes one language-model binding plus four provider-defined tools.

---

## [1] — GoogleClient

The provider transport owner: a `Context.TagClass` whose service wraps the generated REST `Client` plus the Gemini `generateContent` streaming surface.

```ts
// @effect/ai-google/GoogleClient
declare const GoogleClient_base: Context.TagClass<GoogleClient, "@effect/ai-google/GoogleClient", Service>
export declare class GoogleClient extends GoogleClient_base {}

export interface Service {
  readonly client: Generated.Client
  readonly streamRequest: <A, I, R>(
    request: HttpClientRequest.HttpClientRequest,
    schema: Schema.Schema<A, I, R>
  ) => Stream.Stream<A, AiError.AiError, R>
  readonly generateContent: (
    request: typeof Generated.GenerateContentRequest.Encoded
  ) => Effect.Effect<Generated.GenerateContentResponse, AiError.AiError>
  readonly generateContentStream: (
    request: typeof Generated.GenerateContentRequest.Encoded
  ) => Stream.Stream<Generated.GenerateContentResponse, AiError.AiError>
}
```

Constructors / layers (the wire-edge entry — note `make` requires `Scope`, `layer*` do not):

```ts
export declare const make: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined        // default "https://generativelanguage.googleapis.com"
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Effect.Effect<Service, never, HttpClient.HttpClient | Scope.Scope>

export declare const layer: (options: {
  readonly apiKey?: Redacted.Redacted | undefined
  readonly apiUrl?: string | undefined        // default "https://generativelanguage.googleapis.com/v1beta"
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<GoogleClient, never, HttpClient.HttpClient>

export declare const layerConfig: (options: {
  readonly apiKey?: Config.Config<Redacted.Redacted | undefined> | undefined
  readonly apiUrl?: Config.Config<string | undefined> | undefined
  readonly transformClient?: ((client: HttpClient.HttpClient) => HttpClient.HttpClient) | undefined
}) => Layer.Layer<GoogleClient, ConfigError, HttpClient.HttpClient>
```

`layer` carries no error channel; `layerConfig` carries `ConfigError`. Both require `HttpClient.HttpClient` from `@effect/platform` (browser binding is `FetchHttpClient.layer`). The single canonical secret-resolution surface is `Redacted.Redacted` for `apiKey`. The `make`/`layer` option set is narrower than OpenAI's — no `organizationId`/`projectId` (Gemini authenticates by API key only); `apiUrl` defaults differ between `make` (host root) and `layer`/`layerConfig` (`/v1beta` versioned root). All errors collapse to `AiError.AiError` (`@effect/ai/AiError`); the package never surfaces a bespoke error rail.

`generateContent` / `generateContentStream` are the curated entry points consuming `Generated.GenerateContentRequest.Encoded`; raw `client.*` is the escape hatch for the other 78 endpoints (embeddings, token counting, caching, files, tuned models, operations).

---

## [2] — GoogleLanguageModel

The Gemini `generateContent` language-model binding onto `@effect/ai`'s provider-agnostic `LanguageModel`/`Model` contracts.

```ts
// @effect/ai-google/GoogleLanguageModel
export type Model = string   // free-form Gemini model id; no generated enum (unlike OpenAi's ChatModel union)

declare const Config_base: Context.TagClass<Config, "@effect/ai-google/GoogleLanguageModel/Config", Config.Service>
export declare class Config extends Config_base {
  static readonly getOrUndefined: Effect.Effect<Config.Service | undefined>
}

export declare namespace Config {
  interface Service extends Simplify<Partial<Omit<typeof Generated.GenerateContentRequest.Encoded, "contents" | "tools" | "toolConfig" | "systemInstruction">>> {
    readonly toolConfig: Partial<{
      readonly functionCallingConfig: Omit<typeof Generated.FunctionCallingConfig.Encoded, "mode">
    }>
  }
}
```

Entry points (model accepts any `string & {}` widened literal or the `Model` alias; `config` always omits `model`):

```ts
export declare const model: (
  model: (string & {}) | Model,
  config?: Omit<Config.Service, "model">
) => AiModel.Model<"google", LanguageModel.LanguageModel, GoogleClient>

export declare const make: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Omit<Config.Service, "model">
}) => Effect.Effect<LanguageModel.Service, never, GoogleClient>

export declare const layer: (options: {
  readonly model: (string & {}) | Model
  readonly config?: Omit<Config.Service, "model">
}) => Layer.Layer<LanguageModel.LanguageModel, never, GoogleClient>
```

There is no `modelWithTokenizer` / `layerWithTokenizer` / `withConfigOverride` here (Gemini ships no tokenizer binding; those OpenAI surfaces have no Google analogue). `model` produces an `AiModel.Model<"google", ...>` that resolves against the `GoogleClient` service; `layer` installs a `LanguageModel.LanguageModel` requiring `GoogleClient`. `Config` is a request-scoped override carrier (a `Context.TagClass` with `static getOrUndefined`), not the transport config.

The module augments `@effect/ai/Prompt` and `@effect/ai/Response` via `declare module` to attach Google-specific provider slots — the boundary hooks for Gemini-specific prompt/response metadata; internal code reads them through the canonical `@effect/ai` Prompt/Response shapes:

```ts
// declare module "@effect/ai/Prompt" — ProviderOptions.google.thoughtSignature?: string
//   on ReasoningPartOptions, TextPartOptions, ToolCallPartOptions
// declare module "@effect/ai/Response" — ProviderMetadata.google
//   thoughtSignature?: string  on TextStartPartMetadata, TextDeltaPartMetadata, ReasoningPartMetadata,
//     ReasoningStartPartMetadata, ReasoningDeltaPartMetadata, ToolParamsStartPartMetadata,
//     ToolParamsDeltaPartMetadata, ToolCallPartMetadata
//   on FinishPartMetadata:
//     groundingMetadata?:  Generated.GroundingMetadata
//     safetyRatings?:      ReadonlyArray<Generated.SafetyRating>
//     urlContextMetadata?: Generated.UrlContextMetadata
//     usageMetadata?:      Generated.UsageMetadata
```

The `google.thoughtSignature` slot is the canonical carrier for Gemini "thinking" continuity across reasoning/text/tool parts; `FinishPartMetadata.google` is the single surface aggregating grounding, safety, URL-context, and usage telemetry off a finished response.

---

## [3] — GoogleTool

Provider-defined tool constructors yielding `@effect/ai/Tool.ProviderDefined`. Each returns a tool tagged `"Google<Name>"` with `args` (configuration schema), `parameters` (model-supplied call args), `success`/`failure` schemas, and a `failureMode` defaulting to `"error"`. The `Mode` type parameter threads the failure-handling policy into the tool's static type.

```ts
// @effect/ai-google/GoogleTool
export declare const CodeExecution: <Mode extends Tool.FailureMode | undefined = undefined>(args: {}) =>
  Tool.ProviderDefined<"GoogleCodeExecution", {
    readonly args: Schema.Struct<{}>
    readonly parameters: Schema.Struct<{ language: typeof Generated.ExecutableCodeLanguage; code: typeof Schema.String }>
    readonly success: typeof Generated.CodeExecutionResult
    readonly failure: typeof Schema.Never
    readonly failureMode: Mode extends undefined ? "error" : Mode
  }, false>

export declare const GoogleSearch: <Mode extends Tool.FailureMode | undefined = undefined>(args: {}) =>
  Tool.ProviderDefined<"GoogleSearch", {
    readonly args: Schema.Struct<{}>
    readonly parameters: Tool.EmptyParams
    readonly success: typeof Schema.Void
    readonly failure: typeof Schema.Never
    readonly failureMode: Mode extends undefined ? "error" : Mode
  }, false>

export declare const GoogleSearchRetrieval: <Mode extends Tool.FailureMode | undefined = undefined>(args: {
  readonly mode?: "MODE_UNSPECIFIED" | "MODE_DYNAMIC" | null | undefined
  readonly dynamicThreshold?: number | null | undefined
}) => Tool.ProviderDefined<"GoogleSearchRetrieval", {
    readonly args: Schema.Struct<{
      readonly mode: Schema.optionalWith<typeof Generated.DynamicRetrievalConfigMode, { nullable: true }>
      readonly dynamicThreshold: Schema.optionalWith<typeof Schema.Number, { nullable: true }>
    }>
    readonly parameters: Tool.EmptyParams
    readonly success: typeof Schema.Void
    readonly failure: typeof Schema.Never
    readonly failureMode: Mode extends undefined ? "error" : Mode
  }, false>

export declare const UrlContext: <Mode extends Tool.FailureMode | undefined = undefined>(args: {}) =>
  Tool.ProviderDefined<"GoogleUrlContext", {
    readonly args: Schema.Struct<{}>
    readonly parameters: Tool.EmptyParams
    readonly success: typeof Schema.Void
    readonly failure: typeof Schema.Never
    readonly failureMode: Mode extends undefined ? "error" : Mode
  }, false>
```

`GoogleSearch` is the recommended grounding tool for Gemini 2.0+; `GoogleSearchRetrieval` is the legacy Gemini 1.5 dynamic-retrieval variant (its `mode`/`dynamicThreshold` arm gates the model's autonomous decision to search). `CodeExecution` is the only Google tool whose `parameters` carry model-supplied call args (`language` + `code`) and whose `success` is a real schema (`Generated.CodeExecutionResult`); the other three return `Schema.Void` success with `Tool.EmptyParams`. The provider-defined tag prefix is `"Google"` (not `"GoogleGenerated"` or namespaced) — `"GoogleCodeExecution"`, `"GoogleSearch"`, `"GoogleSearchRetrieval"`, `"GoogleUrlContext"`.

---

## [4] — GoogleConfig

A request-scoped client-transform override carrier — the one surface for mutating the underlying `HttpClient` per-request without rebuilding the transport layer.

```ts
// @effect/ai-google/GoogleConfig
declare const GoogleConfig_base: Context.TagClass<GoogleConfig, "@effect/ai-google/GoogleConfig", GoogleConfig.Service>
export declare class GoogleConfig extends GoogleConfig_base {
  static readonly getOrUndefined: Effect.Effect<typeof GoogleConfig.Service | undefined>
}

export declare namespace GoogleConfig {
  interface Service {
    readonly transformClient?: (client: HttpClient) => HttpClient
  }
}

export declare const withClientTransform: {
  (transform: (client: HttpClient) => HttpClient): <A, E, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
  <A, E, R>(self: Effect.Effect<A, E, R>, transform: (client: HttpClient) => HttpClient): Effect.Effect<A, E, R>
}
```

`withClientTransform` is dual (data-first and data-last) — the canonical way to scope a per-region/per-request `HttpClient` mutation onto any `Effect`. Mirrors `OpenAiConfig.withClientTransform` exactly.

---

## [5] — Generated (Gemini-OpenAPI-derived schema corpus)

`Generated` is the machine-generated module covering the full Google Generative AI REST surface: 238 exported schema owners (176 `Schema.Class`, 31 `Schema.Literal`, 23 `Schema.Struct`, 8 `Schema.Record`) plus one `Client` interface. It is not enumerated here — planning pages reference it through the named anchors the owner modules expose. The load-bearing anchors (verbatim names):

```ts
// request/response shapes consumed by GoogleClient.Service and GoogleLanguageModel
export declare class GenerateContentRequest   // Gemini generate-content request (Config.Service derives from its Encoded, minus contents/tools/toolConfig/systemInstruction)
export declare class GenerateContentResponse  // generate-content response object (carries candidates, usageMetadata, ...)
export declare class FunctionCallingConfig    // Config.Service.toolConfig.functionCallingConfig derives from its Encoded, minus `mode`
export declare class FunctionCallingConfigMode // Schema.Literal "MODE_UNSPECIFIED" | "AUTO" | "ANY" | "NONE" | "VALIDATED" function-calling mode enum

// response-metadata anchors attached to @effect/ai/Response FinishPartMetadata.google
export declare class GroundingMetadata        // grounding / search attribution metadata
export declare class SafetyRating             // per-category safety rating
export declare class UrlContextMetadata       // URL-context tool retrieval metadata
export declare class UsageMetadata            // token-usage accounting

// tool sub-schemas referenced by GoogleTool
export declare class ExecutableCodeLanguage   // code-execution language enum (parameters.language)
export declare class CodeExecutionResult      // code-execution success schema
export declare class DynamicRetrievalConfigMode // "MODE_UNSPECIFIED" | "MODE_DYNAMIC" retrieval mode enum

// content algebra (the Gemini prompt/content shapes)
export declare class Content                  // role + parts container
export declare class Part                     // union of part kinds (text, inlineData, functionCall, ...)
export declare class Blob                      // inline binary data { mimeType; data }
export declare class FileData                 // file-reference part { mimeType; fileUri }
export declare class FunctionCall
export declare class FunctionResponse
export declare class ExecutableCode
export declare class Tool                      // request-side tool declaration union
export declare class FunctionDeclaration
export declare class Schema                    // Gemini function-parameter JSON-schema dialect (NOT effect/Schema)
export declare class Type                      // Gemini schema type enum
```

### Generated.Client

An 80-method REST client interface covering every Google Generative AI endpoint. Each method returns `Effect.Effect<typeof <ResponseSchema>.Type, HttpClientError.HttpClientError | ParseError>`. Spelling convention is the PascalCase Gemini `operationId` (string key). Representative anchors:

```ts
export interface Client {
  readonly "GenerateContent": (model: string, options: typeof GenerateContentRequest.Encoded) => Effect.Effect<typeof GenerateContentResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "StreamGenerateContent": (model: string, options: typeof GenerateContentRequest.Encoded) => Effect.Effect<typeof GenerateContentResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "EmbedContent": (model: string, options: typeof EmbedContentRequest.Encoded) => Effect.Effect<typeof EmbedContentResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "BatchEmbedContents": (model: string, options: typeof BatchEmbedContentsRequest.Encoded) => Effect.Effect<typeof BatchEmbedContentsResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "CountTokens": (model: string, options: typeof CountTokensRequest.Encoded) => Effect.Effect<typeof CountTokensResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "GenerateAnswer": (model: string, options: typeof GenerateAnswerRequest.Encoded) => Effect.Effect<typeof GenerateAnswerResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "BatchGenerateContent": (model: string, options: typeof BatchGenerateContentRequest.Encoded) => Effect.Effect<typeof BatchGenerateContentOperation.Type, HttpClientError.HttpClientError | ParseError>
  readonly "ListCachedContents": (options?: typeof ListCachedContentsParams.Encoded | undefined) => Effect.Effect<typeof ListCachedContentsResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "CreateCachedContent": (options: typeof CachedContent.Encoded) => Effect.Effect<typeof CachedContent.Type, HttpClientError.HttpClientError | ParseError>
  readonly "ListModels": (options?: typeof ListModelsParams.Encoded | undefined) => Effect.Effect<typeof ListModelsResponse.Type, HttpClientError.HttpClientError | ParseError>
  readonly "GetOperation": (tunedModel: string, operation: string) => Effect.Effect<typeof Operation.Type, HttpClientError.HttpClientError | ParseError>
  // ...69 further endpoint methods, keyed by Gemini PascalCase operationId
}
```

Distinct from OpenAI: there is no high-level `createEmbedding` curated wrapper on `GoogleClient.Service`. The only curated entries are `generateContent` / `generateContentStream`; embeddings (`EmbedContent` / `BatchEmbedContents`), token counting (`CountTokens`), context caching (`*CachedContent*`), batches (`BatchGenerateContent`, `AsyncBatchEmbedContent`), files (`CreateFile`, `GetFile`, ...), tuned models, and long-running operations (`GetOperation`, `ListOperations`, `CancelOperation`) are reached only through `GoogleClient.Service.client.<operationId>(...)`. Note the `Generated`-internal client option `transformClient` is effectful here (`(client) => Effect.Effect<HttpClient>`), unlike the synchronous `transformClient` on the public `make`/`layer`/`GoogleConfig` surfaces.

---

## [STACKING] — how the Google delta composes onto the shared rails

- Composition root (one arm of the `ai.md` `AiProvider` literal, `effect-ai.md` `STACKING_LAW`): `HttpClient.HttpClient` (`NodeHttpClient.layer`, `effect-platform-node.md`, node tier) → `GoogleClient.layer({ apiKey: Redacted })` / `layerConfig({ apiKey: Config.redacted(...) })` resolving the key through the `security/secret#SECRET_STORE` `SecretStore` `Redacted`/`Config` cells → `GoogleLanguageModel.layer({ model })` → core `LanguageModel.LanguageModel`. Domain code never names Google; it calls the free `LanguageModel.generateText`/`streamText`/`generateObject`, so provider choice is one layer swap.
- Provider-defined tools stack as `Toolkit` rows: `GoogleTool.GoogleSearch({})` / `CodeExecution({})` / `UrlContext({})` / `GoogleSearchRetrieval({...})` each yield a `Tool.ProviderDefined`, collected by `Toolkit.make(...)` alongside user `Tool.make` tools (`agent/runtime#AGENT_RUNTIME` AgentToolkit). The provider executes them (`requiresHandler` is `false`), so no `toolkit.toLayer` handler is bound for the four; only user tools need handlers.
- Reasoning continuity: the `google.thoughtSignature` slot rides `ReasoningPart`/`TextPart`/`ToolCallPart` `providerOptions` inbound and the matching `*PartMetadata` outbound, walked through the canonical `@effect/ai` `Response.StreamPart` fold (`Stream.runForEach`, `effect.md`) — internal code never touches the raw Gemini wire.
- Finish telemetry: `FinishPartMetadata.google` aggregates `groundingMetadata`/`safetyRatings`/`urlContextMetadata`/`usageMetadata` off the settled `finish` part; feed `usageMetadata` into `Telemetry.addGenAIAnnotations` with `system: "gemini"` (`effect-ai.md` `TELEMETRY_LAW`) for the `execution/slo.md` span. All failures collapse to `AiError.AiError`.
- No curated embeddings: the vector arm `search/embedding#EMBEDDING` reaches `client.EmbedContent`/`BatchEmbedContents` through `GoogleClient.Service.client.<operationId>` (raw rail, `HttpClientError | ParseError`) and wraps them in its own `EmbeddingModel.make({ embedMany })`/`makeDataLoader` (`effect-ai.md`), since Google ships no `GoogleEmbeddingModel`.
