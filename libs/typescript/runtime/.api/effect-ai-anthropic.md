# [TS_RUNTIME_API_EFFECT_AI_ANTHROPIC]

`AnthropicLanguageModel` resolves the provider-agnostic `LanguageModel`/`Tokenizer`/`Tool` tags against Anthropic's Messages-beta API; `AnthropicClient` wraps the OpenAPI-`Generated` REST corpus, every call folding `Effect`/`Stream` with failure on core `AiError.AiError`.

`prepareTools` is the sole cross-provider export, `@effect/ai-amazon-bedrock` reusing it to run Claude on Bedrock; prompt caching rides `CacheControlEphemeral` through the `Prompt`/`Response` augmentation slots.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-anthropic`
- package: `@effect/ai-anthropic` (MIT)
- module: dual CJS+ESM, `sideEffects:[]`, one subpath export per namespace (`@effect/ai-anthropic/AnthropicLanguageModel`)
- runtime: node|browser; peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect` through catalog ownership
- rail: ai-provider — the Anthropic Messages-beta binding onto the ai-core tags

## [02]-[CLIENT]

[CLIENT_TYPE_SCOPE]: the `x-api-key` tag-class service and the streaming-fold union

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `AnthropicClient`    | class         | tag `@effect/ai-anthropic/AnthropicClient` over `Generated.Client` |
|  [02]   | `MessageStreamEvent` | union         | the SSE fold discriminated on `type`                               |

[CLIENT_ENTRY_SCOPE]: curated beta-message methods, raw REST access, and the three constructors

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Service.createMessage(options) -> Effect<BetaMessage, AiError>`              | instance | high-level beta message               |
|  [02]   | `Service.createMessageStream(options) -> Stream<MessageStreamEvent, AiError>` | instance | streamed beta message                 |
|  [03]   | `Service.streamRequest(HttpClientRequest, Schema) -> Stream<A, AiError, R>`   | instance | decode arbitrary SSE against a schema |
|  [04]   | `Service.client`                                                              | property | raw `Generated.Client` REST surface   |
|  [05]   | `make(options) -> Effect<Service, never, HttpClient \| Scope>`                | factory  | scoped client                         |
|  [06]   | `layer(options) -> Layer<AnthropicClient, never, HttpClient>`                 | factory  | composition-root layer                |
|  [07]   | `layerConfig(options) -> Layer<AnthropicClient, ConfigError, HttpClient>`     | factory  | each option a `Config.Config`         |

- `make`: `options` carry `apiKey`/`apiUrl`/`anthropicVersion`/`transformClient`, and `make` alone adds `organizationId`/`projectId` as `Redacted`; `layer`/`layerConfig` drop the last two.
- `make`: `anthropicVersion` defaults `"2023-06-01"`, `apiUrl` defaults `"https://api.anthropic.com"`, `apiKey` rides the `x-api-key` header.
- `Service.createMessage`: `options` are `{params?, payload}` over the `Beta*` owners — the high-level surface is beta-only; `createMessageStream`'s `payload` omits `stream`.
- `MessageStreamEvent`: brackets content-block members (`content_block_start` carrying `BetaContentBlock`, `content_block_delta`, `content_block_stop`) with lifecycle members (`ping`, `error`, `message_start` carrying `BetaMessage`, `message_delta`, `message_stop`); `content_block_delta.delta` folds a nested 5-arm union (`citations_delta`, `input_json_delta`, `signature_delta`, `text_delta`, `thinking_delta`).

## [03]-[LANGUAGE_MODEL]

[LANGUAGE_MODEL_TYPE_SCOPE]: the model-id alias, the per-call config, and the provider-metadata carriers

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Model`                  | type alias    | `Generated.Model.Encoded` — the closed Claude-id vocabulary    |
|  [02]   | `Config`                 | class         | tag `.../AnthropicLanguageModel/Config`; the per-call override |
|  [03]   | `AnthropicReasoningInfo` | union         | `thinking` (signature) \| `redacted_thinking` (redactedData)   |
|  [04]   | `AnthropicTools`         | union         | the encoded provider-tool schemas `prepareTools` emits         |

[LANGUAGE_MODEL_ENTRY_SCOPE]: the model/layer family (each returning `Model<"anthropic", …, AnthropicClient>` or its `Layer`), the tool-preparation helper, and the config override

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------------- |
|  [01]   | `model(id, config?) -> Model<…, LanguageModel>`                           | factory | resolve the core language-model tag    |
|  [02]   | `modelWithTokenizer(id, config?) -> Model<…, LanguageModel \| Tokenizer>` | factory | fold in the tokenizer tag              |
|  [03]   | `make({model, config?}) -> Effect<LanguageModel.Service>`                 | factory | build the provider service             |
|  [04]   | `layer({model, config?}) -> Layer<LanguageModel>`                         | factory | bind the tag                           |
|  [05]   | `layerWithTokenizer(options) -> Layer<LanguageModel \| Tokenizer>`        | factory | bind both tags                         |
|  [06]   | `prepareTools(ProviderOptions, Config.Service) -> Effect<{…}, AiError>`   | static  | the prepared tool/choice/beta triple   |
|  [07]   | `withConfigOverride(Config.Service) -> (Effect) -> Effect`                | fold    | scope a per-call config override       |
|  [08]   | `Config.getOrUndefined`                                                   | static  | read the ambient config or `undefined` |

- `model`: `id` is `(string & {}) | Model` — the open string arm over the closed Claude-id literal.
- `make`/`layer`/`layerWithTokenizer`: require the `AnthropicClient` tag in `R`.
- `Config.Service`: `CreateMessageParams.Encoded` minus `messages`/`tools`/`tool_choice`/`stream`, made partial, adding `disableParallelToolCalls: boolean`.
- `prepareTools`: returns `betas: ReadonlySet<string>`, `tools?: ReadonlyArray<AnthropicTools>`, `toolChoice?: BetaToolChoice.Encoded`; exposed for cross-provider reuse.

`declare module` augmentations attach an optional `anthropic` key to the `@effect/ai` `Prompt`/`Response` interface twins, prompt caching dominating through `cacheControl?: CacheControlEphemeral.Encoded`:

| [INDEX] | [AUGMENTS] | [INTERFACES]                                | [ANTHROPIC_SLOT]                                                    |
| :-----: | :--------- | :------------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | `Prompt`   | `System/User/Assistant/Tool MessageOptions` | `cacheControl?`                                                     |
|  [02]   | `Prompt`   | `Text/ToolCall/ToolResult PartOptions`      | `cacheControl?`                                                     |
|  [03]   | `Prompt`   | `ReasoningPartOptions`                      | `AnthropicReasoningInfo & { cacheControl? }`                        |
|  [04]   | `Prompt`   | `FilePartOptions`                           | `cacheControl?`, `citations?`, `documentTitle?`, `documentContext?` |
|  [05]   | `Response` | `Reasoning{,Start,Delta}PartMetadata`       | `AnthropicReasoningInfo`                                            |
|  [06]   | `Response` | `FinishPartMetadata`                        | `{ usage?: BetaUsage; stopSequence? }`                              |
|  [07]   | `Response` | `DocumentSourcePartMetadata`                | `char_location` \| `page_location` citation                         |
|  [08]   | `Response` | `UrlSourcePartMetadata`                     | `{ source:"url"; citedText; encryptedIndex }`                       |

## [04]-[TOKENIZER]

[TOKENIZER_ENTRY_SCOPE]: the bare tokenizer value and its dependency-free layer

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                         |
| :-----: | :---------------------------------- | :------ | :----------------------------------- |
|  [01]   | `make: Tokenizer.Service`           | value   | the tokenizer service, not a factory |
|  [02]   | `layer: Layer<Tokenizer.Tokenizer>` | factory | bind the tokenizer tag               |

- `make`: `AnthropicLanguageModel.modelWithTokenizer`/`layerWithTokenizer` fold this value into the provided tags.

## [05]-[TOOL]

[TOOL_TYPE_SCOPE]: the closed provider-tool schema union and the computer-use coordinate

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `ProviderDefinedTools` | union         | the `Beta*Tool` schema set every provider tool decodes into |
|  [02]   | `Coordinate`           | struct        | `Schema.Tuple2<Number, Number>` pixel pair                  |

[TOOL_ENTRY_SCOPE]: the date-suffixed constructors, each `<Mode>(args) -> Tool.ProviderDefined<tag, {…}, requiresHandler>`

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `Bash_*(args) -> ProviderDefined<"AnthropicBash">`                   | factory | `{command; restart?}` / `String`        |
|  [02]   | `ComputerUse_*(args) -> ProviderDefined<"AnthropicComputerUse">`     | factory | `action` 5→15 verbs + coords / `String` |
|  [03]   | `TextEditor_*(args) -> ProviderDefined<"AnthropicTextEditor">`       | factory | `command` 5→4 verbs / `Void`            |
|  [04]   | `CodeExecution_*(args) -> ProviderDefined<"AnthropicCodeExecution">` | factory | `EmptyParams` / result blocks           |
|  [05]   | `WebSearch_20250305(args) -> ProviderDefined<"AnthropicWebSearch">`  | factory | `EmptyParams` / `Array<…ResultBlock>`   |
|  [06]   | `getProviderDefinedToolName(string) -> string \| undefined`          | static  | wire tool name → provider-defined tag   |

- `Bash`/`ComputerUse`: date suffixes `_20241022`/`_20250124`; `TextEditor` adds `_20250429`/`_20250728`; `CodeExecution` runs `_20250522`/`_20250825`.
- `Bash`/`ComputerUse`/`TextEditor`: `requiresHandler:true`, executed locally by an app handler; `CodeExecution`/`WebSearch` are `requiresHandler:false`, executed provider-side.
- `ProviderDefinedTools`: carries `CodeExecution_20250522`, not the `_20250825` constructor's wider result-block schema; `WebSearch` fails with `ResponseWebSearchToolResultError`.
- `Bash_20241022`: versioning rides the date suffix — one tag carrying an evolving `parameters` literal; `<Mode extends Tool.FailureMode | undefined>` sets `failureMode` to `"error"` when unset.

## [06]-[CONFIG]

[CONFIG_TYPE_SCOPE]: the request-scoped client transform

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `AnthropicConfig` | class         | tag `@effect/ai-anthropic/AnthropicConfig`; per-request `HttpClient` mutation |

[CONFIG_ENTRY_SCOPE]: the transform fold and the ambient read

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `withClientTransform((HttpClient) => HttpClient) -> (Effect) => Effect` | fold    | dual data-first/data-last client mutation |
|  [02]   | `AnthropicConfig.getOrUndefined`                                        | static  | read the ambient transform                |

- `withClientTransform`: mutates the request `HttpClient` without rebuilding transport, distinct from the layer-construction `transformClient` on `make`/`layer`.

## [07]-[GENERATED]

[GENERATED_TYPE_SCOPE]: the machine-generated Anthropic REST surface — wire `Schema.Class` owners, `Schema.Literal` enums, `Schema.Union` families, each stable owner mirrored by a `Beta*` twin carrying beta-only blocks (MCP tool, web-fetch, code-execution, container/skill)

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Model`                                               | union         | the closed Claude model-id literal                           |
|  [02]   | `CreateMessageParams` / `BetaCreateMessageParams`     | class         | request payload; the beta twin the high-level client posts   |
|  [03]   | `Message` / `BetaMessage`                             | class         | response message; `BetaMessage` rides `message_start`        |
|  [04]   | `ContentBlock` / `BetaContentBlock`                   | union         | content-block fold; beta adds MCP/web-fetch/code-exec blocks |
|  [05]   | `Usage` / `BetaUsage`                                 | class         | token accounting; `BetaUsage` lands on `FinishPartMetadata`  |
|  [06]   | `StopReason` / `BetaStopReason`                       | union         | stop vocabulary; beta adds `model_context_window_exceeded`   |
|  [07]   | `ToolChoice` / `BetaToolChoice`                       | union         | tool-selection policy; the `prepareTools` output             |
|  [08]   | `CacheControlEphemeral` / `BetaCacheControlEphemeral` | class         | prompt-caching breakpoint; the Bedrock sibling's twin        |
|  [09]   | `Client`                                              | interface     | the REST client — stable endpoints + `Beta*` mirrors         |
|  [10]   | `make(HttpClient, options?) -> Client`                | factory       | build the low-level REST client                              |
|  [11]   | `ErrorResponse` / `BetaErrorResponse`                 | class         | stable + beta fault payload behind the `ClientError` rail    |

- `Client`: endpoints span messages, message-batches, token-counting, models, files, and skills, each with its `Beta*` mirror; compose by `typeof X.Encoded` (wire) / `typeof X.Type` (decoded) and reach REST via `AnthropicClient.Service.client.<endpoint>`, the high-level methods posting the `Beta*` owners.
- `Client`: stable endpoints fail `ClientError<"ErrorResponse">`, beta `ClientError<"BetaErrorResponse">`, both joined with `HttpClientError | ParseError`.
- Each domain family extends by a new `Beta*` twin, never a mutated stable owner.

## [08]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AnthropicLanguageModel.model(id)` resolves the shared `LanguageModel` tag, so provider choice is one `Layer` swap; the high-level client posts only `Beta*` owners.
- `Stream` folds `MessageStreamEvent` on `type` and its `content_block_delta.delta` on a nested 5-arm discriminant; `Redacted`+`Config` own credential resolution, and failure flows `AiError.AiError` under `catchTag`.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `model`/`modelWithTokenizer` resolve `LanguageModel.LanguageModel` + `Tokenizer.Tokenizer`, `ProviderDefinedTools` returns `Tool.ProviderDefined`/`Tool.FailureMode`, `prepareTools` consumes `LanguageModel.ProviderOptions`, and the `declare module` slots augment `Prompt`/`Response`; no embedding or telemetry tag, `effect-ai.md` `[03]` rowing the asymmetry.
- `@effect/ai-amazon-bedrock`(`.api/effect-ai-amazon-bedrock.md`): imports `AnthropicLanguageModel.prepareTools` + `Generated.BetaCacheControlEphemeral` to run Claude on Bedrock — this catalog is that seam's upstream, `AmazonBedrockTool`'s `Anthropic*` constructors reusing the same `cache_control` breakpoint.
- `@effect/platform`(`.api/effect-platform.md`): every `layer`/`layerConfig` requires `HttpClient.HttpClient` from the `net/client` default-policy row, browser binding `FetchHttpClient.layer` and node `NodeHttpClient.layerUndici`; `streamRequest` consumes `HttpClientRequest.HttpClientRequest`.
- `effect`(`.api/effect.md`): `Schema` decodes `Config`/tools/responses and every `Generated` owner, `Redacted` wraps `apiKey`/`organizationId`/`projectId`, `Config`/`ConfigError` back `layerConfig`, and `Match.discriminator("type")` dispatches `MessageStreamEvent` with its `delta` arms.
- `ai/model.ts`: folds the Anthropic provider row into tier-routing and the guardrail gate, binds the `AnthropicTokenizer` budget owner through `modelWithTokenizer`, and sets prompt caching through the `Prompt` `cacheControl` slots.
- `ai/tool.ts`: projects the tool family, binding `requiresHandler:true` tools (`Bash`/`ComputerUse`/`TextEditor`) through `Toolkit.toLayer` while provider-executed tools resolve server-side.

[LOCAL_ADMISSION]:
- Bind Anthropic through `AnthropicClient.layer({ apiKey })` under `AnthropicLanguageModel.modelWithTokenizer(id)`, credentials resolved by `Redacted`/`Config` and `HttpClient` selected at the app root.
- Read the per-call override through `Config`/`withConfigOverride` and prompt caching through the `Prompt` augmentation slots.

[RAIL_LAW]:
- Package: `@effect/ai-anthropic`
- Owns: the Anthropic Messages-beta binding — `AnthropicClient` over `Generated`, `AnthropicLanguageModel` (`model`/`modelWithTokenizer` + `prepareTools` + the `Config` override + the `Prompt`/`Response` augmentations), `AnthropicTokenizer`, the `AnthropicTool` provider-defined family, `AnthropicConfig` request transform, and the `Generated` OpenAPI REST corpus with its `Beta*` mirrors
- Accept: one `AnthropicLanguageModel.model` row resolved into the shared `LanguageModel`/`Tokenizer` tags, `prepareTools` reused by the Bedrock sibling, prompt caching via `CacheControlEphemeral` on the augmentation slots, tools discriminated on `requiresHandler`
- Reject: a hand-rolled Anthropic HTTP client or SSE decoder, a per-provider generation API beside the shared tags, a bespoke tool schema duplicating `ProviderDefinedTools`, a raw `x-api-key` string bypassing `Redacted`
