# [TS_RUNTIME_API_EFFECT_AI_AMAZON_BEDROCK]

`AmazonBedrockLanguageModel` resolves the provider-agnostic `LanguageModel` tag against the Bedrock Converse API — the most divergent provider row: SigV4-signed regional HTTPS, a hand-written `AmazonBedrockSchema` codec, an AWS binary event-stream channel, native guardrail assessment, and Claude-on-Bedrock tools reused from `@effect/ai-anthropic`.

`Effect`/`Stream`/`Channel` carry every I/O and the core `AiError` carries every failure; no embedding, tokenizer, or telemetry owner binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/ai-amazon-bedrock`
- package: `@effect/ai-amazon-bedrock` (MIT)
- module: per-namespace subpath exports (`@effect/ai-amazon-bedrock/AmazonBedrockClient`); dual CJS+ESM, `sideEffects:[]`
- runtime: node-only — SigV4 request signing has no browser binding; peers `@effect/ai`, `@effect/platform`, `@effect/experimental`, `effect`
- depends: `@effect/ai-anthropic` — `prepareTools` + `Generated.BetaCacheControlEphemeral` run Claude on Bedrock
- rail: ai-provider — the Amazon Bedrock Converse row of the LLM core

## [02]-[CLIENT]

[CLIENT_TYPE_SCOPE]: the SigV4 client tag and its curated Converse surface

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `AmazonBedrockClient` | class         | `Context.TagClass` service tag wrapping the low-level `Client` |
|  [02]   | `Service`             | interface     | curated Converse methods on the `AiError` rail                 |
|  [03]   | `Client`              | interface     | raw `converse` on the `HttpClientError \| ParseError` rail     |

[CLIENT_ENTRY_SCOPE]: construct the client and call Converse

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `make(options) -> Effect<Service, never, HttpClient \| Scope>`                 | factory  | build the SigV4 client                     |
|  [02]   | `layer(options) -> Layer<AmazonBedrockClient, never, HttpClient>`              | factory  | client layer                               |
|  [03]   | `layerConfig(options) -> Layer<AmazonBedrockClient, ConfigError, HttpClient>`  | factory  | `Config`-wrapped creds, adds `ConfigError` |
|  [04]   | `Service.converse(opts) -> Effect<ConverseResponse, AiError>`                  | instance | one Converse call                          |
|  [05]   | `Service.converseStream(opts) -> Stream<ConverseResponseStreamEvent, AiError>` | instance | Converse stream fold                       |
|  [06]   | `Service.streamRequest(request, schema) -> Stream<A, AiError, R>`              | instance | decode an arbitrary event-stream response  |
|  [07]   | `Client.converse(opts) -> Effect<ConverseResponse.Type>`                       | instance | raw low-level call                         |

- `make`/`layer`/`layerConfig`: `accessKeyId` plain `string`, `secretAccessKey`/`sessionToken` `Redacted<string>`, `region`/`apiUrl`/`transformClient` optional; `converse`/`converseStream` forward Anthropic beta opt-ins via `params["anthropic-beta"]`.

## [03]-[LANGUAGE_MODEL]

[LANGUAGE_MODEL_TYPE_SCOPE]: the Converse binding onto the core generation contract

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                                             |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `Model`                      | union         | `typeof BedrockFoundationModelId.Encoded`; `(string & {})` admits inference-profile ARNs |
|  [02]   | `Config`                     | class         | `Context.TagClass` per-call override; `static getOrUndefined`                            |
|  [03]   | `Config.Service`             | interface     | `ConverseRequest.Encoded` minus `messages`/`system`/`toolConfig`, partial                |
|  [04]   | `AmazonBedrockReasoningInfo` | union         | `thinking`(`signature`) \| `redacted_thinking`(`redactedData`)                           |

[LANGUAGE_MODEL_ENTRY_SCOPE]: resolve the provider row and override per call

| [INDEX] | [SURFACE]                                                                             | [SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------------ | :------ | :------------------------------------- |
|  [01]   | `model(id, config?) -> Model<"amazon-bedrock", LanguageModel, AmazonBedrockClient>`   | factory | resolve the core `LanguageModel` tag   |
|  [02]   | `make({model, config?}) -> Effect<LanguageModel.Service, never, AmazonBedrockClient>` | factory | build the model service                |
|  [03]   | `layer({model, config?}) -> Layer<LanguageModel, never, AmazonBedrockClient>`         | factory | model layer                            |
|  [04]   | `withConfigOverride(config)` / `(self, config)`                                       | fold    | dual data-first/last `Config` override |

- `model`/`make`/`layer`: no tokenizer fold and no `prepareTools` — the model consumes Anthropic's; `Config.guardrailConfig` is the per-call switch that turns on native Bedrock guardrails.

`declare module` augmentations attach a `bedrock` slot to the core `Prompt`/`Response` interfaces:

| [INDEX] | [AUGMENTS] | [INTERFACES]                                      | [BEDROCK_SLOT]                                              |
| :-----: | :--------- | :------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `Prompt`   | `System`/`User`/`Assistant`/`Tool MessageOptions` | `cachePoint?` (`CachePointBlock.Encoded`)                   |
|  [02]   | `Prompt`   | `ReasoningPartOptions`                            | `AmazonBedrockReasoningInfo`                                |
|  [03]   | `Response` | `ReasoningPartMetadata`                           | `AmazonBedrockReasoningInfo`                                |
|  [04]   | `Response` | `FinishPartMetadata`                              | `trace?` (`ConverseTrace`) + `usage.cacheWriteInputTokens?` |

## [04]-[TOOL]

[TOOL_ENTRY_SCOPE]: the Anthropic-on-Bedrock local tools as date-suffixed constructors

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `AnthropicBash_20241022` / `_20250124`                                   | factory | `{command, restart?}` -> `String` |
|  [02]   | `AnthropicComputerUse_20241022` / `_20250124`                            | factory | `action` 5→15 verbs -> `String`   |
|  [03]   | `AnthropicTextEditor_20241022` / `_20250124` / `_20250429` / `_20250728` | factory | `command` 5→4 verbs -> `Void`     |

- every constructor returns `Tool.ProviderDefined<"Anthropic<Name>", {…}, true>`: `failure: Schema.Never`, `requiresHandler: true` (the app runs the tool), `failureMode` defaulting `"error"`.
- `AnthropicComputerUse` alone carries `cache_control` (reusing `@effect/ai-anthropic/Generated.BetaCacheControlEphemeral`) with `display_height_px`/`display_width_px`/`display_number?` args; `Bash` and `TextEditor` carry only `failureMode`.
- this module exports the constructors alone — no `ProviderDefinedTools` union, `Coordinate`, or `getProviderDefinedToolName`.

## [05]-[SCHEMA]

[SCHEMA_TYPE_SCOPE]: the hand-written Converse wire codec — `Schema.Class` records, `Schema.Literal` enums, `type`-tagged `Schema.Union` folds

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `ConverseRequest`             | class         | Converse request wire; `Config.Service` derives from its `Encoded`            |
|  [02]   | `ConverseResponse`            | class         | `output`/`metrics`/`usage`/`stopReason`/`trace`/`performanceConfig`           |
|  [03]   | `ConverseResponseStreamEvent` | union         | the ConverseStream fold                                                       |
|  [04]   | `ContentBlock`                | union         | the message-content fold                                                      |
|  [05]   | `BedrockFoundationModelId`    | class         | `Schema.Literal` spanning every Bedrock provider family                       |
|  [06]   | `StopReason`                  | enum          | `Schema.Literal` stop vocabulary                                              |
|  [07]   | `GuardrailTraceAssessment`    | class         | assessment tree feeding `ConverseTrace` -> `FinishPartMetadata.bedrock.trace` |
|  [08]   | `GuardrailAssessment`         | class         | per-policy assessment record                                                  |
|  [09]   | `CachePointBlock`             | class         | the caching-breakpoint block                                                  |

- compose by `typeof X.Encoded` (wire) / `typeof X.Type` (decoded); `AmazonBedrockClient` consumes `ConverseRequest`/`ConverseResponse`/`ConverseResponseStreamEvent`.
- `ConverseRequest` carries `modelId`/`messages`/`system`/`toolConfig`/`guardrailConfig`/`inferenceConfig`/`performanceConfig`/`promptVariables`/`requestMetadata`/`additionalModelRequestFields`/`additionalModelResponseFieldPaths`.
- `ContentBlock` arms: `cachePoint` `document` `guardContent` `image` `reasoningContent` `text` `toolResult` `toolUse` `video`.
- `ConverseResponseStreamEvent` arms: `messageStart` `messageStop` `contentBlockStart` `contentBlockDelta` `contentBlockStop` `metadata`, with faults `internalServerException` `modelStreamErrorException` `serviceUnavailableException` `throttlingException` `validationException`.
- `GuardrailAssessment` records the `contentPolicy`/`contextualGroundingPolicy`/`sensitiveInformationPolicy`/`topicPolicy`/`wordPolicy`/`invocationMetrics` policy owners; `GuardrailTraceAssessment` keys `inputAssessment`/`outputAssessments` maps over it.

## [06]-[EVENT_STREAM]

[EVENT_STREAM_ENTRY_SCOPE]: the AWS binary event-stream frame decoder

| [INDEX] | [SURFACE]                                                                        | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `makeChannel(schema, {bufferSize?}?) -> Channel<Chunk<A>, Chunk<Uint8Array>, …>` | factory | decode AWS event-stream frames |

- `makeChannel`: backs Service `converseStream`/`streamRequest`; consumes a `Chunk<Uint8Array>` input, emits decoded `Chunk<A>`, and joins the upstream input error `IE` with the `Schema` `ParseError`.

## [07]-[CONFIG]

[CONFIG_TYPE_SCOPE]: the request-scoped client transform

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `AmazonBedrockConfig` | class         | `Context.TagClass` request-scoped transform; `static getOrUndefined` |
|  [02]   | `Service`             | interface     | `transformClient?` per-request `HttpClient` mutation                 |

[CONFIG_ENTRY_SCOPE]: apply the transform to an effect

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `withClientTransform(transform)` / `(self, transform)` | fold    | dual data-first/last client transform |

- `AmazonBedrockConfig` carries the upstream copy-paste tag id `@effect/ai-google/AmazonBedrockConfig`, not `@effect/ai-amazon-bedrock/…`; `ai/model.ts` matches this exact spelling.

## [08]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AmazonBedrockLanguageModel.model(id)` resolves the same `LanguageModel` tag every sibling resolves, so provider choice is one `Layer` swap.
- `Service` methods map the low-level `HttpClientError | ParseError` rail onto `AiError`; the raw `Client.converse` alone surfaces the untranslated rail.
- Credentials ride `Redacted`; `layerConfig` wraps each in `Config` and adds `ConfigError`.
- Native guardrails are a per-call `Config.guardrailConfig` switch, and the assessment tree surfaces on `FinishPartMetadata.bedrock.trace`.

[STACKING]:
- `@effect/ai`(`.api/effect-ai.md`): `model(id)` satisfies `LanguageModel.LanguageModel`, the tools return core `Tool.ProviderDefined`/`Tool.FailureMode`, and the `bedrock` slot augments `Prompt.ProviderOptions`/`Response.ProviderMetadata`; no `EmbeddingModel`/`Tokenizer`/`Telemetry` tag binds.
- `@effect/ai-anthropic`(`.api/effect-ai-anthropic.md`): imports `prepareTools` (the prepared tool/tool-choice/beta triple) and `Generated.BetaCacheControlEphemeral` (the `AnthropicComputerUse` `cache_control` schema), forwarding beta opt-ins via `params["anthropic-beta"]` to run Claude-on-Bedrock tools.
- `@effect/platform`(`.api/effect-platform.md`): every `make`/`layer*` requires the `HttpClient.HttpClient` Tag from the `net/client` default-policy row, and the node runtime binds `NodeHttpClient.layer`.
- `effect`(`.api/effect.md`): `EventStreamEncoding.makeChannel` stacks on `Channel`; `Redacted`+`Config` resolve SigV4 creds through `layerConfig`; `Schema` decodes the Converse codec; `Match.discriminator("type")` folds `ConverseResponseStreamEvent` and `ContentBlock`; `Effect.catchTag` branches `AiError`.
- `ai/model.ts`: binds the Bedrock row by one `Layer` swap, sets `Config.guardrailConfig` per call, and reads native guardrails off `FinishPartMetadata.bedrock.trace`, matching the `@effect/ai-google/AmazonBedrockConfig` tag-id spelling.
- `ai/tool.ts`: binds the `requiresHandler:true` tools through `Toolkit.toLayer`.

[LOCAL_ADMISSION]:
- Bind the Bedrock provider as one `Model.make` row resolved into the shared `LanguageModel` tag, swapped by `Layer` in `ai/model.ts`.
- Resolve SigV4 credentials as `Redacted`+`Config` through `layerConfig`, the node runtime satisfying `HttpClient` at the app root.
- Run Claude-on-Bedrock tools through Anthropic's `prepareTools`, binding `requiresHandler:true` tools via `Toolkit.toLayer`.
- Turn on native guardrails per call and read the assessment off `FinishPartMetadata.bedrock.trace`.

[RAIL_LAW]:
- Package: `@effect/ai-amazon-bedrock`
- Owns: the Bedrock Converse binding — `AmazonBedrockClient` SigV4 client, `AmazonBedrockLanguageModel` model resolution with per-call `Config`, the hand-written `AmazonBedrockSchema` codec, `AmazonBedrockTool` Claude-on-Bedrock local tools, `EventStreamEncoding` event-stream channel, `AmazonBedrockConfig` request-scoped transform, and native `GuardrailTraceAssessment`
- Accept: `model(id)` resolved into the shared `LanguageModel` tag, SigV4 creds as `Redacted`+`Config` via `layerConfig`, `converseStream` folded through `EventStreamEncoding.makeChannel`, guardrails set by `Config.guardrailConfig` and read on `FinishPartMetadata.bedrock.trace`, Claude tools reusing Anthropic's `prepareTools`
- Reject: a hand-rolled SigV4 signer, a hand-parsed event-stream frame decoder, a per-provider generation API, a re-declared Anthropic tool schema, an app-level moderation fold where native guardrails serve, an `HttpClientError`/`ParseError` escaping the `AiError` rail
