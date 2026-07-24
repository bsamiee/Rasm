# [TS_RUNTIME_API_EFFECT_AI_OPENROUTER]

`@effect/ai-openrouter` binds OpenRouter's aggregation endpoint onto `@effect/ai`, resolving the provider-agnostic `LanguageModel` tag against one gateway fronting every upstream provider; a model id is a free-form `${provider}/${model}` string carrying no id enum.

Aggregation rides the response — `FinishPartMetadata.openrouter` carries resolved-upstream `provider`, per-call cost, and `cacheControl` breakpoint savings — while the surface binds no embedding, tokenizer, telemetry, or provider-tool; failure flows through `AiError`, all I/O `Effect`/`Stream`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-openrouter`
- package: `@effect/ai-openrouter` (MIT)
- module: dual CJS+ESM, `sideEffects:[]`, per-module subpath exports (`@effect/ai-openrouter/OpenRouterClient`)
- runtime: node|browser; peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect`
- rail: ai — the OpenRouter aggregator provider row

## [02]-[CLIENT]

[CLIENT_TYPE_SCOPE]: the streaming-fold chunk family and the request-scoped client transform

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :--------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ChatStreamingResponseChunk` | class         | streaming chunk re-emitted per event; resolved-upstream carrier    |
|  [02]   | `ChatStreamingChoice`        | class         | per-choice `delta` + `finish_reason` / `native_finish_reason`      |
|  [03]   | `ChatStreamingMessageChunk`  | class         | delta `content` / `reasoning` / `tool_calls` / `annotations`       |
|  [04]   | `OpenRouterConfig`           | class         | request-scoped `HttpClient` transform tag; `static getOrUndefined` |

- `ChatStreamingResponseChunk`: consumers fold `chunk.choices[].delta` with no tagged event union; `provider` names the resolved upstream, `usage` carries cost, `system_fingerprint` and each choice's `native_finish_reason` disambiguate the true upstream, and `model` is the `${provider}/${model}` id.

[CLIENT_ENTRY_SCOPE]: build the client, curated chat completions, and the raw REST escape

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `make(options) -> Effect<Service, never, HttpClient>`          | ctor     | non-scoped client; requires `HttpClient`          |
|  [02]   | `layer(options) -> Layer<OpenRouterClient, never, HttpClient>` | factory  | client `Layer`                                    |
|  [03]   | `layerConfig(options) -> Layer<…, ConfigError, HttpClient>`    | factory  | options in `Config.Config`                        |
|  [04]   | `Service.createChatCompletion(ChatGenerationParams)`           | instance | `-> Effect<ChatResponse, AiError>`                |
|  [05]   | `Service.createChatCompletionStream(ChatGenerationParams)`     | instance | `-> Stream<ChatStreamingResponseChunk, AiError>`  |
|  [06]   | `Service.client -> Generated.Client`                           | property | raw generated REST client                         |
|  [07]   | `OpenRouterConfig.withClientTransform(transform)`              | fold     | request-scoped `HttpClient` transform, dual arity |

- `make`/`layer`/`layerConfig` options: `apiKey` (`Redacted`), `apiUrl`, `referrer`, `title`, `transformClient`; `referrer`/`title` set the `openrouter.ai` app-attribution ranking headers, `transformClient` mutates transport once at build.
- `createChatCompletionStream` omits the `stream` key, set internally.
- `OpenRouterConfig.withClientTransform` mutates the request `HttpClient` per call without rebuilding transport, distinct from the build-time `transformClient`.

## [03]-[LANGUAGE_MODEL]

[LANGUAGE_MODEL_TYPE_SCOPE]: the per-request override tag and the reasoning-slot metadata

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `OpenRouterLanguageModel.Config` | class         | per-request `ChatGenerationParams` override tag; `static getOrUndefined` |
|  [02]   | `OpenRouterReasoningInfo`        | union         | plain-signature or encrypted redacted-data reasoning metadata            |

- `Config` derives from `Generated.ChatGenerationParams.Encoded` with `messages`/`response_format`/`tools`/`tool_choice`/`stream` removed and every key made partial; its aggregation knobs (`provider`, `reasoning`, `route`, `models`) are the routing policy `ai/model.ts` writes as data per call.

`declare module` augmentations attach an optional `openrouter` key onto the `@effect/ai` `Prompt`/`Response` interfaces; internal code reads canonical shapes and the edge maps these slots.

| [INDEX] | [AUGMENTS] | [INTERFACES]                                      | [OPENROUTER_SLOT]            |
| :-----: | :--------- | :------------------------------------------------ | :--------------------------- |
|  [01]   | `Prompt`   | `System`/`User`/`Assistant`/`Tool MessageOptions` | `cacheControl?`              |
|  [02]   | `Prompt`   | `Text`/`Reasoning`/`ToolResult PartOptions`       | `cacheControl?`              |
|  [03]   | `Prompt`   | `FilePartOptions`                                 | `fileName?`, `cacheControl?` |
|  [04]   | `Response` | `Reasoning{,Start,Delta}PartMetadata`             | `OpenRouterReasoningInfo`    |
|  [05]   | `Response` | `UrlSourcePartMetadata`                           | `content?`                   |
|  [06]   | `Response` | `FinishPartMetadata`                              | `provider?`, `usage.cost`    |

[LANGUAGE_MODEL_ENTRY_SCOPE]: resolve the core `LanguageModel` tag and scope its config

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `model(string, Config.Service?) -> AiModel.Model<"openrouter", …>` | factory | resolve the shared `LanguageModel` tag |
|  [02]   | `make({model, config?}) -> Effect<LanguageModel.Service, …>`       | ctor    | raw model service                      |
|  [03]   | `layer({model, config?}) -> Layer<LanguageModel, …>`               | factory | install the tag                        |
|  [04]   | `withConfigOverride(Config.Service)`                               | fold    | per-effect `Config` scope, dual arity  |

- `model`/`make`/`layer` require `OpenRouterClient` and provide the core `LanguageModel` tag.

## [04]-[GENERATED]

[GENERATED_TYPE_SCOPE]: the machine-generated OpenRouter REST corpus — wire schemas, enums, the `Client` interface, and the per-status `ClientError` rail

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ChatGenerationParams`                    | class         | chat request; `Config` derives from its `Encoded`                 |
|  [02]   | `ChatResponse`                            | class         | non-stream chat response                                          |
|  [03]   | `ChatGenerationTokenUsage`                | class         | prompt/completion tokens + resolved cost                          |
|  [04]   | `ProviderPreferences`                     | class         | upstream routing + price/throughput/latency sort policy           |
|  [05]   | `ChatGenerationParamsReasoningEffortEnum` | enum          | `xhigh` `high` `medium` `low` `minimal` `none`                    |
|  [06]   | `ChatGenerationParamsRouteEnum`           | enum          | `fallback` `sort`                                                 |
|  [07]   | `CacheControlEphemeral`                   | class         | caching-breakpoint marker                                         |
|  [08]   | `ReasoningDetail`                         | union         | `Summary` / `Encrypted` / `Text` reasoning-part detail            |
|  [09]   | `ClientError`                             | class         | per-HTTP-status typed fault beside `HttpClientError`/`ParseError` |

[GENERATED_ENTRY_SCOPE]: the aggregator raw client fronting each upstream's native format and the account endpoints, reached via `Service.client.<op>`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `client.sendChatCompletionRequest(ChatGenerationParams)` | instance | native OpenRouter chat             |
|  [02]   | `client.createResponses(OpenResponsesRequest)`           | instance | OpenAI-compatible Responses format |
|  [03]   | `client.createMessages(AnthropicMessagesRequest)`        | instance | Anthropic Messages format          |
|  [04]   | `client.getCredits()`                                    | instance | purchased/used credits             |
|  [05]   | `client.getUserActivity(GetUserActivityParams?)`         | instance | per-endpoint activity              |
|  [06]   | `client.getModels(GetModelsParams?)`                     | instance | model catalog + properties         |

- `Service.client.<op>` composes by `typeof X.Encoded` / `typeof X.Type`, never `Generated.make` nor an individual schema import.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every language-model entrypoint resolves the provider-agnostic `LanguageModel.LanguageModel` tag, so provider choice is one `Layer` swap and a model id stays a free-form `${provider}/${model}` string.
- Aggregation transparency rides each response: `provider` names the resolved upstream, `usage.cost` attributes spend, and `native_finish_reason` disambiguates the true upstream behind the gateway.
- Generation rides `Effect`/`Stream`; failure flows through the `AiError.AiError` union.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `OpenRouterLanguageModel.model(id)` resolves `LanguageModel.LanguageModel`, so `generateText`/`streamText`/`generateObject` and the guardrail gate run unchanged, and the `declare module` augmentations populate the `Prompt`/`Response` `openrouter` slots; the absent embedding, tokenizer, telemetry, and provider-tool cells leave those rows to a sibling provider.
- `@effect/ai-openai`(`.api/effect-ai-openai.md`): `OpenRouterClient` re-owns the OpenAI-compatible Responses wire in its own `Generated.createResponses(OpenResponsesRequest.Encoded)`, so a design page reaches OpenAI-format aggregation through `OpenRouterClient.Service.client.createResponses` and never composes `OpenAiClient`; each provider is a peer row in `effect-ai.md` `PROVIDER_ROWS` with its own client.
- `@effect/platform`(`libs/typescript/.api/effect-platform.md`): every `layer*` requires `HttpClient.HttpClient` from the `net/client` default-policy row, satisfied by `FetchHttpClient.layer` (browser) or `NodeHttpClient.layerUndici` (node); `transformClient` at build and `OpenRouterConfig.withClientTransform` per request compose proxy/retry/header policy without a transport rebuild.
- `ai/model.ts` ranks provider fallback, reading `FinishPartMetadata.openrouter.usage.cost` + `.provider` per response to attribute spend and pick the next tier, writing the aggregation knobs (`provider`/`reasoning`/`route`/`models`) into `OpenRouterLanguageModel.Config` per call, and setting `cacheControl` breakpoints to cut input cost on repeated system prefixes.

[LOCAL_ADMISSION]:
- OpenRouter admits as one aggregator provider row resolved into the shared `LanguageModel` tag; `Redacted` + `Config` own credential resolution through `layerConfig`.
- Reach REST via `OpenRouterClient.Service.client.<op>`, composing by `typeof X.Encoded` / `typeof X.Type`.

[RAIL_LAW]:
- Package: `@effect/ai-openrouter`
- Owns: the OpenRouter aggregation binding onto `@effect/ai` — the `OpenRouterClient` curated chat rails and raw `Generated` REST corpus, `OpenRouterLanguageModel` resolving the `LanguageModel` tag, the per-request `Config` and `OpenRouterConfig` client transform, and the provider-routing / reasoning / cost / cache-breakpoint metadata
- Accept: one `model(id)` row resolved into the shared tag, `Redacted` + `Config` credentials, aggregation knobs carried as `Config` data, `FinishPart` cost/provider read for tier routing
- Reject: a model-id enum, an `OpenAiClient` composition for OpenAI-format aggregation, a hand-rolled provider-routing or cost meter, an arbitrary-SSE escape hatch when `createChatCompletionStream` owns streaming, an embedding/tokenizer/telemetry port bound here
