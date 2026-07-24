# [TS_RUNTIME_API_EFFECT_AI_OPENAI]

`@effect/ai-openai` binds the OpenAI Responses and Embeddings APIs onto `@effect/ai`, resolving the provider-agnostic `LanguageModel`, `EmbeddingModel`, `Tokenizer`, `Tool`, and `Telemetry` tags so provider choice is one composition-root `Layer` swap.

Every provider symbol is one parameterized surface over the `Generated` REST corpus; all I/O rides `Effect`/`Stream` and every failure flows through the core `AiError.AiError`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-openai`
- package: `@effect/ai-openai` (MIT)
- module: per-module subpath exports (`@effect/ai-openai/OpenAiClient`), dual CJS+ESM, `sideEffects:[]`
- runtime: node|browser isomorphic; every constructor requires `HttpClient` from a `@effect/platform` client `Layer`; peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect`
- rail: openai-provider — the reference provider binding the Responses API onto the `@effect/ai` tags

## [02]-[CLIENT]

[CLIENT_TYPE_SCOPE]: the curated client tag, its service, and the streaming-fold surface

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]         | [CAPABILITY]                                        |
| :-----: | :-------------------- | :-------------------- | :-------------------------------------------------- |
|  [01]   | `OpenAiClient`        | class (`Context.Tag`) | tag `@effect/ai-openai/OpenAiClient` over `Service` |
|  [02]   | `Service`             | interface             | curated REST rails wrapping `Generated.Client`      |
|  [03]   | `ResponseStreamEvent` | union                 | Responses SSE fold, each event `type`-discriminated |
|  [04]   | `LogProbs`            | class                 | per-token logprob with `top_logprobs`               |
|  [05]   | `SummaryPart`         | class                 | reasoning-summary text part                         |

[CLIENT_ENTRY_SCOPE]: construct the client and reach curated REST

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `make(options) -> Effect<Service, never, HttpClient \| Scope>`                       | factory  | scoped client build                 |
|  [02]   | `layer(options) -> Layer<OpenAiClient, never, HttpClient>`                           | factory  | tag Layer, no error channel         |
|  [03]   | `layerConfig(options) -> Layer<OpenAiClient, ConfigError, HttpClient>`               | factory  | options in `Config.Config`          |
|  [04]   | `Service.createResponse(CreateResponse) -> Effect<Response>`                         | instance | one Responses call                  |
|  [05]   | `Service.createResponseStream(options) -> Stream<ResponseStreamEvent>`               | instance | streamed Responses fold             |
|  [06]   | `Service.createEmbedding(CreateEmbeddingRequest) -> Effect<CreateEmbeddingResponse>` | instance | one embedding call                  |
|  [07]   | `Service.streamRequest(HttpClientRequest, Schema<A,I,R>) -> Stream<A, AiError, R>`   | instance | decode an arbitrary SSE endpoint    |
|  [08]   | `Service.client`                                                                     | property | raw `Generated.Client` escape hatch |

- every `Service` call fails into `AiError` and takes the `Generated` request as its `.Encoded` wire shape; `make`/`layer`/`layerConfig` options carry `apiKey`/`organizationId`/`projectId` as `Redacted`, `apiUrl` overriding the endpoint, and `transformClient` mutating transport once at build; every arity requires `HttpClient`, only `make` also requires `Scope`.
- `Service.createResponseStream`: `options` is `CreateResponse.Encoded` minus `stream`.
- `ResponseStreamEvent`: each event discriminates on a `type` literal and carries `sequence_number: Int`; consumers `Stream.runForEach` + `Match.discriminator("type")`, never parallel handlers. Events group by family — response lifecycle, output-item, content-part, text, refusal, function-call-args, reasoning, provider-executed-tool progress, and `error`.

## [03]-[LANGUAGE_MODEL]

[LANGUAGE_MODEL_TYPE_SCOPE]: the per-request Config carrier and the model-id union

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]         | [CAPABILITY]                                               |
| :-----: | :--------------- | :-------------------- | :--------------------------------------------------------- |
|  [01]   | `Config`         | class (`Context.Tag`) | tag `.../OpenAiLanguageModel/Config`, per-request override |
|  [02]   | `Config.Service` | interface             | `CreateResponse` request minus SDK keys, made partial      |
|  [03]   | `Model`          | union                 | `ChatModel` and `ModelIdsResponsesEnum` id enums           |

[LANGUAGE_MODEL_ENTRY_SCOPE]: bind the Responses API onto the core `LanguageModel`/`Model` contracts

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `model((string&{})\|Model, config?) -> AiModel.Model<"openai", LanguageModel>` | factory | provider-tagged model value           |
|  [02]   | `modelWithTokenizer(...) -> AiModel.Model<"openai", LanguageModel\|Tokenizer>` | factory | model value folding the tokenizer tag |
|  [03]   | `make({model, config?}) -> Effect<LanguageModel.Service>`                      | factory | raw service                           |
|  [04]   | `layer({model, config?}) -> Layer<LanguageModel>`                              | factory | install the tag                       |
|  [05]   | `layerWithTokenizer({model, config?}) -> Layer<LanguageModel\|Tokenizer>`      | factory | install tag with tokenizer            |
|  [06]   | `withConfigOverride(Config.Service) -> (Effect) -> Effect`                     | fold    | dual per-effect Config scope          |

- every constructor requires the `OpenAiClient` tag; `model`/`modelWithTokenizer` return a value that is BOTH a `Layer` and an `Effect<Layer>`, and `*WithTokenizer` folds `Tokenizer.Tokenizer` into the provided tags.
- model argument widens to `(string & {}) | Model`, so the enum drives autocomplete without rejecting newer ids.
- `Config.Service` omits `input`/`tools`/`tool_choice`/`stream`/`text`, made partial, and adds `fileIdPrefixes`, `text.verbosity` (`low`/`medium`/`high`), and `strict`; `static getOrUndefined` reads it, and `ai/model.ts` writes it per call as the tier-routing seam.
- `declare module` augments the `@effect/ai` `Prompt`/`Response` interfaces with an optional `openai` slot; internal code reads canonical shapes and the edge maps them. Distinctive slot payloads: `itemId`/`encryptedContent` on reasoning parts, `imageDetail` on file parts, `refusal` on text metadata, `file_citation`/`url_citation` index shapes on source metadata, and `serviceTier` (`default`/`auto`/`flex`/`scale`/`priority`) on finish metadata.

## [04]-[EMBEDDING_MODEL]

[EMBEDDING_MODEL_TYPE_SCOPE]: the batched and data-loader config extensions

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]         | [CAPABILITY]                                                   |
| :-----: | :------------------ | :-------------------- | :------------------------------------------------------------- |
|  [01]   | `Config`            | class (`Context.Tag`) | tag `.../OpenAiEmbeddingModel/Config`                          |
|  [02]   | `Config.Batched`    | interface             | `maxBatchSize` with an optional `{capacity, timeToLive}` cache |
|  [03]   | `Config.DataLoader` | interface             | `window: Duration` request coalescing                          |
|  [04]   | `Model`             | union                 | `CreateEmbeddingRequestModelEnum` id                           |

[EMBEDDING_MODEL_ENTRY_SCOPE]: the sole provider `EmbeddingModel` binding

| [INDEX] | [SURFACE]                                                                                 | [SHAPE] | [CAPABILITY]             |
| :-----: | :---------------------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `model((string&{})\|Model, {mode, ...config}) -> AiModel.Model<"openai", EmbeddingModel>` | factory | polymorphic on `mode`    |
|  [02]   | `makeDataLoader({model, config}) -> Effect<EmbeddingModel.Service, never, Scope>`         | factory | background batcher       |
|  [03]   | `layerBatched({model, config?}) -> Layer<EmbeddingModel>`                                 | factory | batched Layer            |
|  [04]   | `layerDataLoader({model, config}) -> Layer<EmbeddingModel>`                               | factory | data-loader Layer        |
|  [05]   | `withConfigOverride(Config.Service) -> (Effect) -> Effect`                                | fold    | dual per-effect override |

- every constructor requires the `OpenAiClient` tag; `model` discriminates on `mode` — `"batched"` coalesces calls up to `maxBatchSize` with an optional bounded cache, `"data-loader"` windows requests over a `Duration`.
- `makeDataLoader` also requires `Scope` for its background batcher; the layers scope internally, and `ai/embed.ts` reads this binding as the `store/retrieve` `Embedder` port source.

## [05]-[TOKENIZER]

[TOKENIZER_ENTRY_SCOPE]: model-keyed token counting installed dependency-free

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `make({model}) -> Tokenizer.Service`           | factory | pure, model-keyed tokenizer    |
|  [02]   | `layer({model}) -> Layer<Tokenizer.Tokenizer>` | factory | install the tag, no dependency |

- `make` returns a `Tokenizer.Service` with no `Effect` wrapper; `OpenAiLanguageModel.layerWithTokenizer`/`modelWithTokenizer` fold `layer` in implicitly.

## [06]-[TOOL]

[TOOL_ENTRY_SCOPE]: provider-executed tool constructors the provider runs and the app collects with `Toolkit.make`

| [INDEX] | [SURFACE]                | [SHAPE] | [CAPABILITY]        |
| :-----: | :----------------------- | :------ | :------------------ |
|  [01]   | `CodeInterpreter(args)`  | factory | sandboxed code run  |
|  [02]   | `FileSearch(args)`       | factory | vector-store search |
|  [03]   | `WebSearch(args)`        | factory | web search          |
|  [04]   | `WebSearchPreview(args)` | factory | preview web search  |

- each ctor is `<Mode extends Tool.FailureMode \| undefined>(args) => Tool.ProviderDefined<"OpenAi<Name>", {args; parameters; success; failure; failureMode}, false>`; the provider runs the tool (`requiresHandler=false`), `failure` is `Schema.Never` on all four, and `Mode` threads the failure-routing policy into the static type.
- `CodeInterpreter` args `container: string \| {type:"auto"; file_ids?}`; parameters `{code: NullOr<String>; container_id}`; success `NullOr<Array<CodeInterpreterOutputLogs \| CodeInterpreterOutputImage>>`.
- `FileSearch` args `vector_store_ids`, `max_num_results?`, `ranking_options?`, `filters?`; parameters `Tool.EmptyParams`; success `{status; queries; results?}` (`SchemaClass`).
- `WebSearch`/`WebSearchPreview` parameters `{action: search \| open_page \| find}`, success `{status: WebSearchToolCallStatus}`; `WebSearch` adds `filters.allowed_domains?`.
- `FileSearch.filters` is the OpenAI filter algebra — a comparison node `{type: eq\|ne\|gt\|gte\|lt\|lte; key; value: string\|number\|boolean\|ReadonlyArray<string\|number>}` or a compound node `{type: and\|or; filters: ReadonlyArray<comparison>}`.

## [07]-[TELEMETRY]

[TELEMETRY_TYPE_SCOPE]: OpenAI-namespaced GenAI attribute extensions

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `RequestAttributes`       | interface     | `responseFormat`, `serviceTier` request attrs     |
|  [02]   | `ResponseAttributes`      | interface     | `serviceTier`, `systemFingerprint` response attrs |
|  [03]   | `WellKnownResponseFormat` | union         | `"json_object" \| "json_schema" \| "text"`        |
|  [04]   | `WellKnownServiceTier`    | union         | `"auto" \| "default"`                             |

[TELEMETRY_ENTRY_SCOPE]: fold OpenAI GenAI attributes onto the active span

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `addGenAIAnnotations(OpenAiTelemetryAttributeOptions) -> (Span) -> void` | fold    | dual; mutates the `Span` in place |

- `OpenAiTelemetry` extends the core `Telemetry` GenAI attribute set; `addGenAIAnnotations` mutates the `effect/Tracer` `Span` in place — the one boundary-kernel mutation and the seam onto `@effect/opentelemetry`. Both `RequestAttributes` and `ResponseAttributes` fold under the `gen_ai.openai.request` prefix, so response attributes resolve under the request namespace.

## [08]-[CONFIG]

[CONFIG_TYPE_SCOPE]: the request-scoped client-transform carrier

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]         | [CAPABILITY]                                                  |
| :-----: | :--------------------- | :-------------------- | :------------------------------------------------------------ |
|  [01]   | `OpenAiConfig`         | class (`Context.Tag`) | tag `@effect/ai-openai/OpenAiConfig`, `static getOrUndefined` |
|  [02]   | `OpenAiConfig.Service` | interface             | `{transformClient?}` transport mutation                       |

[CONFIG_ENTRY_SCOPE]: scope a per-request transport mutation onto any effect

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `withClientTransform((HttpClient)=>HttpClient) -> (Effect) -> Effect` | fold    | dual, request-scoped transport swap |

- `OpenAiConfig` carries a per-region/per-request `HttpClient` mutation without rebuilding transport, distinct from the layer-construction `transformClient`.

## [09]-[GENERATED]

[GENERATED_ENTRY_SCOPE]: the machine-generated OpenAI REST surface reached through named anchors

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `make(HttpClient, {transformClient?}) -> Client`                                    | factory  | build the raw REST client |
|  [02]   | `Client.<operationId>(options?) -> Effect<Response, HttpClientError \| ParseError>` | instance | one full-REST operation   |
|  [03]   | `Client.httpClient`                                                                 | property | underlying `HttpClient`   |

- planning code reaches REST via `OpenAiClient.Service.client.<operationId>(...)`, composing request bodies as `typeof X.Encoded`; a `Generated` operation fails into `HttpClientError.HttpClientError | ParseError`, never a package `ClientError`.
- `CreateResponse` — `OpenAiLanguageModel.Config` derives from its `Encoded`. `Annotation` unions `FileCitationBody`/`UrlCitationBody`/`ContainerFileCitationBody`/`FilePath`.

[MODEL_ID_ENUMS]: `ChatModel` `ModelIdsResponsesEnum` `CreateEmbeddingRequestModelEnum` `ReasoningEffort` `ImageDetail`
[REQUEST_RESPONSE_SHAPES]: `CreateResponse` `Response` `OutputItem` `CreateChatCompletionRequest` `CreateEmbeddingRequest` `CreateEmbeddingResponse`
[TOOL_SUB_SCHEMAS]: `RankingOptions` `Filters` `ApproximateLocation` `SearchContextSize` `WebSearchToolSearchContextSize` `WebSearchToolCallStatus` `WebSearchActionSearch` `WebSearchActionOpenPage` `WebSearchActionFind` `CodeInterpreterOutputLogs` `CodeInterpreterOutputImage` `Annotation` `OutputTextContent` `RefusalContent` `ReasoningTextContent`

## [10]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every provider symbol resolves a core `@effect/ai` tag, so provider choice is one composition-root `Layer` swap; all I/O rides `Effect`/`Stream`, failures flow through the `AiError.AiError` union, and the streaming union folds on the `type` discriminant.
- `model` widens its argument to `(string & {}) | Model`, so a newer id never hard-rejects while the enum still drives autocomplete.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `OpenAiLanguageModel.model`/`.layer` resolve `LanguageModel.LanguageModel`, `OpenAiEmbeddingModel.layerBatched`/`layerDataLoader` resolve `EmbeddingModel.EmbeddingModel`, `OpenAiTokenizer.layer`/`OpenAiLanguageModel.layerWithTokenizer` resolve `Tokenizer.Tokenizer`, the four `OpenAiTool` ctors return `Tool.ProviderDefined`, `OpenAiTelemetry.addGenAIAnnotations` extends `Telemetry.GenAITelemetryAttributes`, and the `Prompt`/`Response` `declare module` augmentations attach the `openai` slot onto the core interfaces.
- `@effect/platform`(`.api/effect-platform.md`): `make`/`layer`/`layerConfig` require `HttpClient.HttpClient` from the `net/client` default-policy row, `Service.streamRequest` takes an `HttpClientRequest.HttpClientRequest`, and a `Generated` op fails into `HttpClientError.HttpClientError | ParseError`; `FetchHttpClient.layer`, `NodeHttpClient.layerUndici`, or `BrowserHttpClient.layerXMLHttpRequest` satisfy the tag at the app root.
- `@effect/ai-openrouter`(`.api/effect-ai-openrouter.md`): a peer provider carrying its own `OpenRouterClient` and `Generated`, sharing only the `@effect/ai` `LanguageModel` tag — no direct member seam; both are `Model.make` rows the `ai/model.ts` Layer selects between.
- `ai/model.ts`: composes the `OpenAiLanguageModel.model(id)` row and writes `OpenAiLanguageModel.Config` per call for tier routing; `ai/embed.ts` binds `OpenAiEmbeddingModel` to the `store/retrieve` `Embedder` port; `ai/tool.ts` projects the four `OpenAiTool` constructors through `Toolkit.make` onto the MCP lane; `OpenAiTelemetry.addGenAIAnnotations` writes the active `otel` span.

[LOCAL_ADMISSION]:
- OpenAI is the reference provider row of `@effect/ai` — the only admitted provider populating language, embedding, tokenizer, and telemetry.
- Reach REST through `OpenAiClient`, resolve credentials as `Redacted` via `layerConfig`, and compose provider dependencies top-down: `Effect.provide(OpenAiLanguageModel.model(id))` over `OpenAiClient.layer({ apiKey })` over an `HttpClient` Layer.

[RAIL_LAW]:
- Package: `@effect/ai-openai`
- Owns: the OpenAI Responses/Embeddings binding onto `@effect/ai` — `OpenAiClient` curated rails with `streamRequest`, `OpenAiLanguageModel` with the per-request `Config` override, `OpenAiEmbeddingModel` batched/data-loader, `OpenAiTokenizer`, the four `OpenAiTool` provider-executed constructors, `OpenAiTelemetry` GenAI attributes, `OpenAiConfig` request-scoped client transform, and the `Generated` REST corpus
- Accept: a provider symbol resolving a core `@effect/ai` tag, `Redacted` credentials through `layerConfig`, per-request tuning through `Config` + `withConfigOverride`, the `ResponseStreamEvent` fold under `Match.discriminator("type")`, and `AiError` through `catchTag`
- Reject: a hand-rolled OpenAI HTTP call, a raw `Generated.make` or individual schema import in planning code, a per-provider generation API beside the shared tags, an unredacted key, and a thrown fault where the `AiError` rail carries it

