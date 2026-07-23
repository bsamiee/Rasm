# [TS_RUNTIME_API_EFFECT_AI_OPENAI]

`@effect/ai-openai` (MIT) · dual CJS+ESM, `sideEffects:[]`, per-module `exports` subpaths (`@effect/ai-openai/OpenAiClient`) · marker TSDECL `node_modules/@effect/ai-openai/dist/dts/*.d.ts` · peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect` through catalog ownership · tier node|browser (`FetchHttpClient.layer`)

`@effect/ai-openai` binds the OpenAI Responses API onto `@effect/ai`, resolving the provider-agnostic `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Tool`/`Telemetry` tags. It is the ONLY admitted provider carrying all four asymmetry capabilities — language model, embeddings (two batching modalities), tokenizer, and a provider-namespaced GenAI telemetry module — so `ai/model.ts` reads it as the fully-populated reference row of the capability-asymmetry table, and `ai/embed.ts` binds its `EmbeddingModel` to the `store/retrieve` `Embedder` port. Eight owner modules re-export through the barrel (`OpenAiClient`, `OpenAiConfig`, `OpenAiEmbeddingModel`, `OpenAiLanguageModel`, `OpenAiTelemetry`, `OpenAiTokenizer`, `OpenAiTool`, `Generated`); every provider-facing symbol below is one parameterized surface, and the OpenAI wire corpus (`Generated`) is a category with named anchors, never enumerated. Success/failure flows through the core `AiError.AiError`; all I/O is `Effect`/`Stream`.

## [01]-[ASYMMETRY]

Each provider is ONE row with asymmetry columns; a new provider is a row, never a fork. OpenAI's columns against the four admitted siblings (the contrast is the whole point of the table):

| [INDEX] | [COLUMN]               | [OPENAI]                            | [ANTHROPIC]                 | [GOOGLE]         | [BEDROCK]         |
| :-----: | :--------------------- | :---------------------------------- | :-------------------------- | :--------------- | :---------------- |
|  [01]   | provider id            | `"openai"`                          | anthropic                   | google           | amazon-bedrock    |
|  [02]   | language model         | `OpenAiLanguageModel` (Responses)   | Messages                    | generateContent  | Converse          |
|  [03]   | embedding model        | `OpenAiEmbeddingModel` (batched/DL) | —                           | raw client       | —                 |
|  [04]   | tokenizer              | `OpenAiTokenizer.make({model})`     | value                       | —                | —                 |
|  [05]   | provider-defined tools | 4 (`OpenAiTool`)                    | 5 families                  | 4                | 8 (via Anthropic) |
|  [06]   | telemetry module       | `OpenAiTelemetry`                   | —                           | —                | —                 |
|  [07]   | model-id kind          | `ChatModel`/`ModelIdsResponsesEnum` | 21-id enum                  | free `string`    | 91-id enum        |
|  [08]   | auth                   | `Redacted` apiKey + org/project     | apiKey + version            | apiKey           | SigV4 keys        |
|  [09]   | per-request Config tag | `Config` (`strict`, `verbosity`)    | +`disableParallelToolCalls` | +`toolConfig`    | Converse fields   |
|  [10]   | streaming fold         | `ResponseStreamEvent` (49-member)   | 8-member                    | response re-emit | 11-member         |

## [02]-[CLIENT]

`OpenAiClient` is a `Context.TagClass` (id `@effect/ai-openai/OpenAiClient`) wrapping the generated REST `Client` plus curated Responses/embedding entrypoints. `client` is the low-level escape hatch (section [09]); `createResponse`/`createResponseStream`/`createEmbedding` are the curated rails; `streamRequest` decodes an arbitrary SSE response against a `Schema` for uncurated endpoints.

[SERVICE]: `Service.client: Generated.Client` `Service.streamRequest: <A,I,R>(request:HttpClientRequest.HttpClientRequest,schema:Schema.Schema<A,I,R>)=>Stream.Stream<A,AiError.AiError,R>` `Service.createResponse: (options:typeof Generated.CreateResponse.Encoded)=>Effect.Effect<Generated.Response,AiError.AiError>` `Service.createResponseStream: (options:Omit<typeof Generated.CreateResponse.Encoded,"stream">)=>Stream.Stream<ResponseStreamEvent,AiError.AiError>` `Service.createEmbedding: (options:typeof Generated.CreateEmbeddingRequest.Encoded)=>Effect.Effect<Generated.CreateEmbeddingResponse,AiError.AiError>`
[STREAM_COMPLETION_REQUEST]: `StreamCompletionRequest = Omit<typeof Generated.CreateChatCompletionRequest.Encoded,"stream">`

ONE constructor pattern, three arities over the same option shape — `make` (scoped effect), `layer` (no error channel), `layerConfig` (`ConfigError`, options wrapped in `Config.Config`). `apiKey`/`organizationId`/`projectId` are `Redacted`; `transformClient` mutates the transport once at build. Every arity requires `HttpClient.HttpClient` beneath; only `make` additionally requires `Scope`.

[SURFACES]: `make` `layer` `layerConfig` `Layer` `each`

`ResponseStreamEvent` is the ONE canonical streaming-fold surface — a 49-member `Schema.Union` of `Schema.Class` events, each discriminated on a `type` literal and carrying `sequence_number: Int`. Consumers `Stream.runForEach` and `Match.value(event).pipe(Match.discriminator("type")(...))`; the design never maintains parallel handlers. Members group by lifecycle family (`response.<created|queued|in_progress|completed|incomplete|failed>` each carrying `Generated.Response`), output-item (`response.output_item.<added|done>` carrying `Generated.OutputItem`), content-part (`response.content_part.<added|done>`), text (`response.output_text.<delta|done|annotation.added>` with `LogProbs`), refusal (`response.refusal.<delta|done>`), function-call-args (`response.function_call_arguments.<delta|done>`), reasoning (`response.reasoning_summary_part.*`, `response.reasoning_summary_text.*`, `response.reasoning_text.*` with `SummaryPart`), the provider-executed-tool progress families (`file_search`/`web_search`/`image_generation`/`mcp_call`/`mcp_list_tools`/`code_interpreter`/`custom_tool` each `in_progress|searching|generating|completed|failed|delta|done`), and `error`.

[RESPONSE_STREAM_EVENT]: `ResponseStreamEvent = typeof ResponseStreamEvent.Type`
[LOG_PROBS]: `LogProbs.token: string` `LogProbs.logprob: number` `LogProbs.top_logprobs: ReadonlyArray<{token:string;logprob:number}>`
[SUMMARY_PART]: `SummaryPart.type: "summary_text"` `SummaryPart.text: string`
[SURFACES]: `ResponseStreamEvent: Schema.Union<[/* 49 typeof Response*Event members */]>`

## [03]-[LANGUAGE_MODEL]

`OpenAiLanguageModel` binds the Responses API onto the core `LanguageModel`/`Model` contracts. Model argument is the widened `(string & {}) | Model` — the enum drives autocomplete without rejecting newer ids. ONE model/layer family: `model`/`modelWithTokenizer` return a provider-tagged `AiModel.Model<"openai", …>` (both a `Layer` and an `Effect<Layer>`); `layer`/`layerWithTokenizer` install the tag directly; `make` yields the raw `Service`; `withConfigOverride` is the dual per-effect Config scope. `*WithTokenizer` folds `Tokenizer.Tokenizer` into the model's provided tags (section [05]).

[MODEL]: `Model = typeof Generated.ChatModel.Encoded|typeof Generated.ModelIdsResponsesEnum.Encoded`
[WITH_CONFIG_OVERRIDE]: `withConfigOverride.call(Config.Service) -> <A,E,R>(self:Effect.Effect<A,E,R>)=>Effect.Effect<A,E,R>` `withConfigOverride.call(Effect.Effect<A,E,R>,Config.Service) -> Effect.Effect<A,E,R>`
[SURFACES]: `model((string&{})|Model,Omit<Config.Service,"model">?) -> AiModel.Model<"openai",LanguageModel.LanguageModel,OpenAiClient>` `modelWithTokenizer((string&{})|Model,Omit<Config.Service,"model">?) -> AiModel.Model<"openai",LanguageModel.LanguageModel|Tokenizer.Tokenizer,OpenAiClient>` `make({model:(string&{})|Model;config?:Omit<Config.Service,"model">}) -> Effect.Effect<LanguageModel.Service,never,OpenAiClient>` `layer({model;config?}) -> Layer.Layer<LanguageModel.LanguageModel,never,OpenAiClient>` `layerWithTokenizer({model;config?}) -> Layer.Layer<LanguageModel.LanguageModel|Tokenizer.Tokenizer,never,OpenAiClient>`

`Config` is a `Context.TagClass` (id `@effect/ai-openai/OpenAiLanguageModel/Config`, `static getOrUndefined`) — the per-request override carrier, the whole `CreateResponse` request minus SDK-owned keys (`input`/`tools`/`tool_choice`/`stream`/`text`) made partial, plus three OpenAI-specific knobs. It is the tier-routing seam `ai/model.ts` writes per call.

[CONFIG.SERVICE]: `Config.Service.fileIdPrefixes: ReadonlyArray<string>` `Config.Service.text: {readonly verbosity?:"low"|"medium"|"high"}` `Config.Service.strict: boolean`

`declare module` augments `@effect/ai/Prompt` and `@effect/ai/Response` with an optional `openai` key — ONE boundary-hook pattern; internal code reads canonical `@effect/ai` shapes, the edge maps these slots:

| [INDEX] | [AUGMENTS] | [INTERFACES]                                    | [OPENAI_SLOT]                                                  |
| :-----: | :--------- | :---------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Prompt`   | `FilePartOptions`                               | `imageDetail?: ImageDetail.Encoded`                            |
|  [02]   | `Prompt`   | `ReasoningPartOptions`                          | `itemId?`, `encryptedContent?`                                 |
|  [03]   | `Prompt`   | `TextPartOptions`, `ToolCallPartOptions`        | `itemId?`                                                      |
|  [04]   | `Response` | `TextPartMetadata`                              | `itemId?`, `refusal?`                                          |
|  [05]   | `Response` | `TextStartPartMetadata`, `ToolCallPartMetadata` | `itemId?`                                                      |
|  [06]   | `Response` | `Reasoning{,Start,End}PartMetadata`             | `itemId?`, `encryptedContent?`                                 |
|  [07]   | `Response` | `ReasoningDeltaPartMetadata`                    | `itemId?`                                                      |
|  [08]   | `Response` | `DocumentSourcePartMetadata`                    | `{ type:"file_citation"; index }`                              |
|  [09]   | `Response` | `UrlSourcePartMetadata`                         | `{ type:"url_citation"; startIndex; endIndex }`                |
|  [10]   | `Response` | `FinishPartMetadata`                            | `serviceTier?: "default"\|"auto"\|"flex"\|"scale"\|"priority"` |

## [04]-[EMBEDDING_MODEL]

`OpenAiEmbeddingModel` is the only provider `EmbeddingModel` binding — the `ai/embed.ts` source of the `store/retrieve` `Embedder` port. ONE polymorphic `model` discriminates on `mode`: `"batched"` coalesces calls up to `maxBatchSize` with an optional bounded `{capacity, timeToLive}` cache; `"data-loader"` windows requests over a `Duration`. `makeDataLoader` requires `Scope` (background batcher); the layers scope internally. `Config` (tag `@effect/ai-openai/OpenAiEmbeddingModel/Config`) is the `CreateEmbeddingRequest` minus `input`, made partial, with `Batched`/`DataLoader` extensions.

[SURFACES]: `Model` `model` `makeDataLoader` `layerBatched` `layerDataLoader` `withConfigOverride` `Config` `Service` `Batched` `DataLoader`

## [05]-[TOKENIZER]

`OpenAiTokenizer.make` is pure (no Effect wrapper), keyed by model; `layer` installs the `Tokenizer.Tokenizer` tag dependency-free. `OpenAiLanguageModel.layerWithTokenizer`/`modelWithTokenizer` fold it in implicitly. This is one of the two tokenizer owners `ai/model.ts` budgets read (the other is `AnthropicTokenizer`).

[SURFACES]: `make({readonly model:string}) -> Tokenizer.Service` `layer({readonly model:string}) -> Layer.Layer<Tokenizer.Tokenizer>`

## [06]-[TOOL]

`OpenAiTool` exports four provider-executed tool constructors, each ONE instance of the same shape: `<Mode extends Tool.FailureMode | undefined>(args) => Tool.ProviderDefined<"OpenAi<Name>", { args; parameters; success; failure; failureMode: Mode extends undefined ? "error" : Mode }, false>`. Provider runs them (`requiresHandler=false`); the app collects them with `Toolkit.make(...)` and projects them through `ai/tool.ts`, one row per constructor:

| [INDEX] | [CTOR]             | [PARAMETERS]                             | [SUCCESS]                                                                |
| :-----: | :----------------- | :--------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `CodeInterpreter`  | `{ code: NullOr<String>; container_id }` | `NullOr<Array<CodeInterpreterOutputLogs \| CodeInterpreterOutputImage>>` |
|  [02]   | `FileSearch`       | `Tool.EmptyParams`                       | `{ status; queries; results? }` (`SchemaClass`)                          |
|  [03]   | `WebSearch`        | `{ action: search\|open_page\|find }`    | `{ status: WebSearchToolCallStatus }`                                    |
|  [04]   | `WebSearchPreview` | `{ action: search\|open_page\|find }`    | `{ status: WebSearchToolCallStatus }`                                    |

Tag is `OpenAi<Ctor>` for each row. Per-ctor `args` (constructor input):
- [01]-`CodeInterpreter`: `container: string | { type:"auto"; file_ids? }`
- [02]-`FileSearch`: `vector_store_ids`, `max_num_results?`, `ranking_options?`, `filters?`
- [03]-`WebSearch`: `user_location?`, `search_context_size?`, `filters.allowed_domains?`
- [04]-`WebSearchPreview`: `user_location?`, `search_context_size?`

`FileSearch.filters` is the canonical OpenAI filter algebra — a comparison node `{ type:"eq"|"ne"|"gt"|"gte"|"lt"|"lte"; key; value: string|number|boolean|ReadonlyArray<string|number> }` or a compound node `{ type:"and"|"or"; filters: ReadonlyArray<comparison> }`. `failure` is `Schema.Never` on all four; `Mode` threads the failure-routing policy into the static type.

## [07]-[TELEMETRY]

`OpenAiTelemetry` extends the core `Telemetry` GenAI attribute set with OpenAI-namespaced request/response attributes. `addGenAIAnnotations` is dual and mutates the `effect/Tracer` `Span` in place — the one boundary-kernel mutation in the package, and the seam onto `@effect/opentelemetry`. Both `RequestAttributes` and `ResponseAttributes` fold under the `gen_ai.openai.request` prefix, so a consumer reading raw attribute keys resolves response attributes under the request namespace.

[OPEN_AI_TELEMETRY_ATTRIBUTES]: `OpenAiTelemetryAttributes = Simplify<…>`
[ALL_ATTRIBUTES]: `AllAttributes = Telemetry.AllAttributes&RequestAttributes&ResponseAttributes`
[REQUEST_ATTRIBUTES]: `RequestAttributes.responseFormat: (string&{})|WellKnownResponseFormat|null` `RequestAttributes.serviceTier: (string&{})|WellKnownServiceTier|null`
[RESPONSE_ATTRIBUTES]: `ResponseAttributes.serviceTier: string|null` `ResponseAttributes.systemFingerprint: string|null`
[WELL_KNOWN_RESPONSE_FORMAT]: `WellKnownResponseFormat = "json_object"|"json_schema"|"text"`
[WELL_KNOWN_SERVICE_TIER]: `WellKnownServiceTier = "auto"|"default"`
[OPEN_AI_TELEMETRY_ATTRIBUTE_OPTIONS]: `OpenAiTelemetryAttributeOptions = Telemetry.GenAITelemetryAttributeOptions&{openai?:{request?:RequestAttributes;response?:ResponseAttributes}}`
[SURFACES]: `addGenAIAnnotations: ((options:OpenAiTelemetryAttributeOptions)=>(span:Span)=>void)&((span:Span,options:OpenAiTelemetryAttributeOptions)=>void)`

## [08]-[CONFIG]

`OpenAiConfig` (`Context.TagClass`, id `@effect/ai-openai/OpenAiConfig`, `static getOrUndefined`) is the request-scoped client transform — the one surface for a per-region/per-request `HttpClient` mutation without rebuilding transport. Distinct from the layer-construction `transformClient`: `withClientTransform` scopes onto any `Effect`, dual data-first/data-last.

[SURFACES]: `OpenAiConfig` `Service` `withClientTransform`

## [09]-[GENERATED]

`Generated` is the machine-generated OpenAI REST surface: exported owners (`Schema.Class` wire schemas, `Schema.Literal` enums) plus the `make` Client factory, a 219-endpoint `Client` interface, and the `ClientError` rail. It is not enumerated — owner modules reach it through named anchors, and planning code reaches REST via `OpenAiClient.Service.client.<operationId>(...)`, never by importing individual schemas. Load-bearing anchors (exact spelling):

- Model-id enums: `ChatModel`, `ModelIdsResponsesEnum`, `CreateEmbeddingRequestModelEnum`, `ReasoningEffort`, `ImageDetail`.
- Request/response shapes: `CreateResponse` (Config derives from its `Encoded`), `Response`, `OutputItem`, `CreateChatCompletionRequest`, `CreateEmbeddingRequest`, `CreateEmbeddingResponse`.
- Tool sub-schemas: `RankingOptions`, `Filters`, `ApproximateLocation`, `SearchContextSize`, `WebSearchToolSearchContextSize`, `WebSearchToolCallStatus`, `WebSearchActionSearch`/`WebSearchActionOpenPage`/`WebSearchActionFind`, `CodeInterpreterOutputLogs`, `CodeInterpreterOutputImage`, `Annotation` (`FileCitationBody | UrlCitationBody | ContainerFileCitationBody | FilePath`), `OutputTextContent`, `RefusalContent`, `ReasoningTextContent`.

[CLIENT]: `Client.httpClient: HttpClient.HttpClient` `Client.listAssistants: (options?:typeof ListAssistantsParams.Encoded)=>Effect.Effect<typeof ListAssistantsResponse.Type,HttpClientError.HttpClientError|ParseError>`
[CLIENT_ERROR]: `ClientError._tag: Tag` `ClientError.request: HttpClientRequest` `ClientError.response: HttpClientResponse` `ClientError.cause: E`
[SURFACES]: `make(HttpClient.HttpClient,{transformClient?:(c:HttpClient.HttpClient)=>Effect.Effect<HttpClient.HttpClient>}?) -> Client`

`OpenAiClient.make`/`layer*` build the `Generated.Client` internally; planning code never calls `Generated.make` directly. Curated `createResponse`/`createResponseStream`/`createEmbedding` are the entry points; raw `client.*` is the escape hatch for uncurated endpoints.

## [10]-[INTEGRATION]

- Universal Effect rails: provider choice is a single composition-root `Layer` swap — every provider's `model(...)` produces the same `LanguageModel.LanguageModel` tag, so `OpenAiLanguageModel.model("gpt-…")` ↔ any sibling is one line in `ai/model.ts`. `Redacted` + `Config` own credential resolution (`layerConfig`); `Stream` folds `ResponseStreamEvent`; `Schema` decodes `Config`/tool-params/responses; `Duration` bounds the embedding cache/window; `Match.discriminator("type")`/`Effect.catchTag` dispatch the streaming union and the `AiError` rail. Compose the stack top-down: `Effect.provide(OpenAiLanguageModel.model(id))` over `OpenAiClient.layer({ apiKey })` over an `HttpClient` layer.
- `@effect/platform` seam: every `layer*` requires `HttpClient.HttpClient` — the `net/client` default-policy row (timeout/retry/proxy) owns it; browser binds `FetchHttpClient.layer`, node binds `NodeHttpClient.layer`. `streamRequest` takes an `HttpClientRequest` for uncurated SSE endpoints.
- `@effect/ai` core (sibling catalog `effect-ai.md`): satisfies `LanguageModel.LanguageModel` (+ `GenerateTextResponse`/`GenerateObjectResponse` accessors), `EmbeddingModel.EmbeddingModel`, `Tokenizer.Tokenizer`, `Tool.ProviderDefined`/`Tool.FailureMode`, and `Telemetry`. Failures are the core `AiError.AiError` union.
- Sibling providers: OpenAI is the reference row — the only one populating all of embeddings + tokenizer + telemetry; `ai/model.ts` reads the empty asymmetry cells of anthropic/google/bedrock against this row.
- Observability: `OpenAiTelemetry.addGenAIAnnotations` writes GenAI semantic-convention attributes into the active `@effect/opentelemetry` span (`system:"openai"`, `operation.name:"chat"|"embeddings"`, usage/request/response), the pre-bound alternative to raw `Telemetry.addGenAIAnnotations`.
- Design consumers: `ai/model.ts` (row + tier-routing via `Config` + the one guardrail gate), `ai/model.ts` (`OpenAiTokenizer` budgets), `ai/embed.ts` (`OpenAiEmbeddingModel` → `Embedder` port), `ai/tool.ts`+`ai/tool.ts` (the four provider-defined tools projected as MCP tools).
