# [TS_RUNTIME_API_EFFECT_AI_GOOGLE]

`@effect/ai-google` binds Gemini's generateContent API onto the `@effect/ai` core, resolving the provider-agnostic `LanguageModel` and `Tool` tags against Gemini and attaching a `google` slot to the core `Prompt`/`Response` part interfaces.

It curates a language model and four provider-executed tools; embeddings and token-counting stay on the raw `Generated.Client`, so `ai/embed.ts` routes a Google embedding through the low-level `EmbedContent`/`CountTokens` rail. Failure flows through `AiError`, all I/O through `Effect`/`Stream`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-google`
- package: `@effect/ai-google` (MIT)
- module: per-namespace subpath exports (`@effect/ai-google/GoogleClient`), dual CJS+ESM, `sideEffects:[]`
- runtime: node|browser, platform-neutral; peers `@effect/ai`, `@effect/experimental`, `@effect/platform`, `effect`
- rail: ai-google provider — Gemini generateContent onto the `@effect/ai` core

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the service tags and their resolved shapes

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `GoogleClient`         | class         | `Context.TagClass` id `@effect/ai-google/GoogleClient`      |
|  [02]   | `GoogleClient.Service` | interface     | raw `Generated.Client` plus curated generateContent entries |
|  [03]   | `GoogleConfig`         | class         | `Context.TagClass` id `@effect/ai-google/GoogleConfig`      |
|  [04]   | `Config`               | class         | `Context.TagClass` id `.../GoogleLanguageModel/Config`      |
|  [05]   | `Model`                | type          | `string`; Gemini ships no model-id enum, autocomplete open  |

- `Config`, `GoogleConfig`: each carries a `static getOrUndefined` reader for the resolved service.
- `Config.Service`: `GenerateContentRequest.Encoded` minus `contents`/`tools`/`toolConfig`/`systemInstruction` made partial, with a partial `toolConfig.functionCallingConfig: Omit<FunctionCallingConfig.Encoded,"mode">` — the tier-routing seam `ai/model.ts` writes per call.
- `GoogleConfig.Service`: one optional `transformClient: (HttpClient) => HttpClient` mutating the request client.

[PUBLIC_TYPE_SCOPE]: the provider boundary-hook augmentations — each `declare module` attaches one optional `google` key

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Prompt.{Reasoning,Text,ToolCall}PartOptions` | interface     | `google.thoughtSignature?` reasoning-continuity carrier     |
|  [02]   | `Response` streaming part-metadata            | interface     | `google.thoughtSignature?` on each text/reasoning/tool part |
|  [03]   | `Response.FinishPartMetadata`                 | interface     | `google` folds grounding/safety/urlContext/usage metadata   |

- `Response.FinishPartMetadata.google`: aggregates `groundingMetadata?`, `safetyRatings?` (`ReadonlyArray<SafetyRating>`), `urlContextMetadata?`, and `usageMetadata?` off a finished response.

[PUBLIC_TYPE_SCOPE]: the machine-generated Google Generative AI REST wire surface

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `GenerateContentRequest` `GenerateContentResponse`                          | class         | the generateContent wire pair        |
|  [02]   | `FunctionCallingConfig` `FunctionCallingConfigMode`                         | class/enum    | function-calling mode enum           |
|  [03]   | `GroundingMetadata` `SafetyRating` `UrlContextMetadata` `UsageMetadata`     | class         | finish-part metadata shapes          |
|  [04]   | `ExecutableCodeLanguage` `CodeExecutionResult` `DynamicRetrievalConfigMode` | class/enum    | tool wire shapes                     |
|  [05]   | `Content` `Part` `Blob` `FileData`                                          | class         | the Gemini content algebra           |
|  [06]   | `FunctionCall` `FunctionResponse` `ExecutableCode`                          | class         | the Gemini call/result algebra       |
|  [07]   | `Tool` `FunctionDeclaration` `Schema` `Type`                                | class         | Gemini function-param schema dialect |
|  [08]   | `Client`                                                                    | interface     | PascalCase-operation REST endpoints  |

- `FunctionCallingConfigMode`: `MODE_UNSPECIFIED|AUTO|ANY|NONE|VALIDATED`.
- `Generated.Schema`/`Generated.Type`: the Gemini function-parameter JSON-schema dialect, not `effect/Schema`.
- `Client` methods return `Effect<typeof <Response>.Type, HttpClientError.HttpClientError | ParseError>`; its internal `transformClient` is effectful (`(client) => Effect<HttpClient>`), unlike the synchronous transform on the public surfaces.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct the client and call generateContent — three constructors share `opts = { apiKey?: Redacted, apiUrl?: string, transformClient? }`, `optsConfig` wrapping `apiKey`/`apiUrl` in `Config`

| [INDEX] | [SURFACE]                                                                                           | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `make(opts) -> Effect<Service, never, HttpClient\|Scope>`                                           | factory  | scoped client         |
|  [02]   | `layer(opts) -> Layer<GoogleClient, never, HttpClient>`                                             | factory  | client Layer          |
|  [03]   | `layerConfig(optsConfig) -> Layer<GoogleClient, ConfigError, HttpClient>`                           | factory  | config-resolved Layer |
|  [04]   | `generateContent(GenerateContentRequest.Encoded) -> Effect<GenerateContentResponse, AiError>`       | instance | one-shot generate     |
|  [05]   | `generateContentStream(GenerateContentRequest.Encoded) -> Stream<GenerateContentResponse, AiError>` | instance | streamed generate     |
|  [06]   | `streamRequest(HttpClientRequest, Schema<A,I,R>) -> Stream<A, AiError, R>`                          | instance | decode arbitrary SSE  |
|  [07]   | `client -> Generated.Client`                                                                        | property | raw REST rail         |

- `make`: `apiUrl` defaults to the host root; `layer`/`layerConfig` default to the `/v1beta` root. Gemini authenticates by the `Redacted` `apiKey` alone — no org or project id.
- `generateContentStream`: re-emits a full `GenerateContentResponse` per chunk, so a consumer folds `response.candidates[]` deltas, never a tagged `event.type` union.

[ENTRYPOINT_SCOPE]: bind generateContent onto the core `LanguageModel` — `id = (string & {}) | Model`, `config = Omit<Config.Service, "model">`

| [INDEX] | [SURFACE]                                                                              | [SHAPE] | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------------------- | :------ | :------------------------ |
|  [01]   | `model(id, config?) -> Model<"google", LanguageModel, GoogleClient>`                   | factory | provider row for the fold |
|  [02]   | `make({ model, config? }) -> Effect<LanguageModel.Service, never, GoogleClient>`       | factory | resolve the service       |
|  [03]   | `layer({ model, config? }) -> Layer<LanguageModel.LanguageModel, never, GoogleClient>` | factory | provide the tag           |

- Write the `Config` tag directly for a per-effect override; no tokenizer binding and no `withConfigOverride` exist on this provider.

[ENTRYPOINT_SCOPE]: the four provider-executed tools — each `<Mode extends Tool.FailureMode|undefined>(args) -> Tool.ProviderDefined<Tag, Cfg, false>`, `requiresHandler:false`

| [INDEX] | [SURFACE]                                                                                      | [SHAPE] | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `CodeExecution({}) -> ProviderDefined<"GoogleCodeExecution">`                                  | factory | model-run code execution |
|  [02]   | `GoogleSearch({}) -> ProviderDefined<"GoogleSearch">`                                          | factory | grounding switch         |
|  [03]   | `GoogleSearchRetrieval({mode?,dynamicThreshold?}) -> ProviderDefined<"GoogleSearchRetrieval">` | factory | dynamic-retrieval switch |
|  [04]   | `UrlContext({}) -> ProviderDefined<"GoogleUrlContext">`                                        | factory | grounding switch         |

- `CodeExecution` alone carries model-supplied `parameters` (`{ language: ExecutableCodeLanguage, code }`) and a real `CodeExecutionResult` `success`; the other three expose `Tool.EmptyParams` and a `Void` success, `failure` is `Schema.Never` on all four.
- `GoogleSearchRetrieval`: `mode` is `DynamicRetrievalConfigMode` (`MODE_UNSPECIFIED|MODE_DYNAMIC`) and `dynamicThreshold` (0.0–1.0) gates the model's autonomous search decision.

[ENTRYPOINT_SCOPE]: request-scoped `HttpClient` transform without rebuilding transport

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `withClientTransform(transform) -> (Effect) -> Effect` | instance | data-last request-client mutation  |
|  [02]   | `withClientTransform(Effect, transform) -> Effect`     | instance | data-first request-client mutation |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Generation rides `Effect`/`Stream` and fails into `AiError.AiError`; `generateContentStream` re-emits a full `GenerateContentResponse` per chunk, so a consumer folds `candidates[]` deltas rather than a tagged event union.
- Gemini authenticates by one `Redacted` apiKey and carries no model-id enum, tokenizer binding, per-effect `withConfigOverride`, or telemetry module; per-request tuning writes the `Config` tag, and embeddings and token-counting ride the raw `Generated.Client`.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `GoogleLanguageModel.model(id)` resolves the core `LanguageModel.LanguageModel` tag and `GoogleTool.*` return `Tool.ProviderDefined`/`Tool.FailureMode`, so provider choice is one `Layer` swap; the `declare module` augmentations attach the `google` slot onto the core `Prompt`/`Response` part interfaces, and no `EmbeddingModel`/`Tokenizer`/`Telemetry` tag binds here.
- `@effect/platform`(`.api/effect-platform.md`): every `layer*` requires the `HttpClient.HttpClient` Tag and `Service.streamRequest` consumes an `HttpClientRequest.HttpClientRequest`; `FetchHttpClient.layer` (browser) or `NodeHttpClient.layerUndici` (node) satisfies the Tag at the app root.
- `ai/model.ts`: folds the Google row into the provider table, reads `FinishPartMetadata.google.safetyRatings` for the output-moderation guardrail gate, and writes the `Config` tag per call for tier routing.
- `ai/tool.ts`: projects the four `GoogleTool` provider-defined tools under the shared safety owner.
- `ai/embed.ts`: binds no Google `EmbeddingModel` — a Google embedding routes through `GoogleClient.Service.client.EmbedContent` on the low-level `Generated.Client` rail, or through another provider's tag.

[LOCAL_ADMISSION]:
- Bind Gemini through `GoogleClient.layer({ apiKey })` under an `HttpClient` layer, then `GoogleLanguageModel.model(id)` over it; the app root picks the runtime `HttpClient`.
- Write the `Config` tag for per-request tuning and `GoogleConfig.withClientTransform` for a request-scoped client mutation; reach embeddings, token-counting, caching, batches, and operations through `Service.client.*` alone.

[RAIL_LAW]:
- Package: `@effect/ai-google`
- Owns: the Gemini `GoogleClient` (`make`/`layer`/`layerConfig` with generateContent), `GoogleLanguageModel` (`model`/`make`/`layer` with the per-request `Config` tag), the four `GoogleTool` provider-defined tools, `GoogleConfig` request-scoped transform, and the `Generated` REST wire surface
- Accept: `GoogleLanguageModel.model(id)` resolved into the core `LanguageModel` tag, the `Config` tag written per call, `GoogleTool` tools projected through `ai/tool.ts`, credential via `Redacted`+`Config`, `HttpClient` satisfied at the app root
- Reject: a hand-rolled Gemini REST client, a fabricated `createEmbedding` or tokenizer wrapper, a per-effect config-override call, a model-id enum invention, or a second provider generation API
