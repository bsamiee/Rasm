# [RASM_COMPUTE_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` supplies provider-agnostic contracts for chat, embedding,
image generation, speech-to-text, text-to-speech, realtime sessions, and hosted-file clients; the
tool/function model with hosted-tool (web search, code interpreter, MCP, image gen, file search,
vector store) call/result content; a discriminated `AIContent` part hierarchy with annotations and
reasoning; chat-history reduction; and JSON-schema generation/transform utilities — consumed by
Compute model owners through `IChatClient`, `IEmbeddingGenerator`, `AIFunction`, and the multimodal
client contracts without coupling to a specific provider.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions` (consumer-bound `lib/net10.0`; multi-targets
 `net9.0`, `net8.0`, `netstandard2.0`, `net462`)
- namespace: `Microsoft.Extensions.AI`
- license: MIT
- asset: runtime library (contracts + content model + JSON-schema utilities)
- companion: the concrete `Microsoft.Extensions.AI` core package owns the middleware that is
 NOT in Abstractions — `IChatClient.AsBuilder()` / `ChatClientBuilder`, `UseFunctionInvocation`,
 `UseOpenTelemetry`, `UseDistributedCache`, `UseLogging`, and the typed structured-output
 `ChatClientStructuredOutputExtensions.GetResponseAsync<T>(…)` → `ChatResponse<T>` extension (there is
 NO typed streaming twin; the streaming surface is the untyped `GetStreamingResponseAsync`). `Rasm.Compute`
 references ONLY `Microsoft.Extensions.AI.Abstractions`, so the concrete core is NOT a Compute
 dependency — it is referenced by `Rasm.AppHost` and documented once there at
 `libs/csharp/Rasm.AppHost/.api/api-extensions-ai-middleware.md` (the model-governance composition edge). This catalog
 documents the contracts that pipeline builds on; the builder surface is out-of-package here by design.
- in-repo implementors: `OnnxRuntimeGenAIChatClient` (`libs/csharp/Rasm.Compute/.api/api-onnxruntimegenai.md`) is the local generative
 `IChatClient` implementation that satisfies the streaming contract documented below; its facade defines
 only `OnnxRuntimeGenAIChatClient` + `OnnxRuntimeGenAIChatClientOptions`, while `IChatClient` /
 `ChatResponse` / `ChatResponseUpdate` / `ChatMessage` / `ChatOptions` resolve here.
- rail: model-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat client contracts
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:--------------------- |:--------------- |:------------------------------------------------------ |
| [01] | `IChatClient` | client contract | `IDisposable` provider-agnostic chat interface |
| [02] | `DelegatingChatClient` | delegating base | middleware composition base |
| [03] | `ChatClientExtensions` | extension class | `string`/`ChatMessage` overloads + `GetRequiredService` |
| [04] | `ChatResponseExtensions` | extension class | stream→response fold + history accumulation |
| [05] | `ChatMessage` | message model | `Role`, `Contents`, `Text`, `Clone()` |
| [06] | `ChatRole` | role value | `System`, `User`, `Assistant`, `Tool` |
| [07] | `ChatOptions` | request options | sampling, tools, format, reasoning, conversation |
| [08] | `ChatResponse` | response model | `Text`, `Messages`, `Usage`, `FinishReason`, token |
| [09] | `ChatResponseUpdate` | streaming update | incremental streaming unit (`ToChatResponse` foldable) |
| [10] | `ChatFinishReason` | finish reason | stop condition vocabulary |
| [11] | `ChatToolMode` | tool-mode base | `AutoChatToolMode`, `NoneChatToolMode`, `RequiredChatToolMode` |
| [12] | `ChatResponseFormat` | response format | `ChatResponseFormatText`, `ChatResponseFormatJson` |
| [13] | `ChatClientMetadata` | client metadata | provider name and model identity |
| [14] | `IChatReducer` | history reducer | trims/summarizes conversation before send |
| [15] | `UsageDetails` | usage model | input/output/total token counts |

[PUBLIC_TYPE_SCOPE]: embedding generator contracts
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:----------------------------- |:----------------- |:------------------------------------ |
| [01] | `IEmbeddingGenerator` | non-generic base | service-discovery root |
| [02] | `IEmbeddingGenerator<TInput,TEmbedding>` | typed contract | `GenerateAsync` over a batch |
| [03] | `DelegatingEmbeddingGenerator<TInput,TEmbedding>` | delegating base | middleware composition base |
| [04] | `EmbeddingGeneratorExtensions` | extension class | scalar/zip/vector convenience overloads |
| [05] | `Embedding` / `Embedding<T>` | embedding carrier | base + typed embedding payload |
| [06] | `GeneratedEmbeddings<TEmbedding>` | batch result | embedding batch + `Usage` |
| [07] | `BinaryEmbedding` | binary embedding | bit-packed embedding vector |
| [08] | `EmbeddingGenerationOptions` | request options | model + additional properties |

[PUBLIC_TYPE_SCOPE]: tool and function contracts
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:--------------------------------------------------- |:--------------- |:------------------------------------------- |
| [01] | `AITool` | tool base | description base for any tool |
| [02] | `AIFunctionDeclaration` | declaration base | non-invocable tool description (manifest) |
| [03] | `AIFunction` | function tool | invocable; adds `InvokeAsync` |
| [04] | `AIFunctionFactory` | function factory | delegate/`MethodInfo`/schema → `AIFunction` |
| [05] | `AIFunctionArguments` | argument carrier | keyed argument dictionary |
| [06] | `AIFunctionFactoryOptions` | factory options | JSON schema + binding + marshalling policy |
| [07] | `ApprovalRequiredAIFunction` | approval gate | wraps an `AIFunction` behind explicit approval |
| [08] | `HostedWebSearchTool` / `HostedCodeInterpreterTool` / `HostedFileSearchTool` / `HostedImageGenerationTool` / `HostedToolSearchTool` | hosted tools | provider-executed (server-side) tools |
| [09] | `HostedMcpServerTool` + `HostedMcpServerToolApprovalMode` (`AlwaysRequire`/`NeverRequire`/`RequireSpecific`) | hosted MCP | remote MCP server tool + approval policy |
| [10] | `HostedVectorStoreContent` | hosted content | provider vector-store reference |

[PUBLIC_TYPE_SCOPE]: multimodal generation and realtime contracts
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:----------------------------------------------------------------- |:--------------- |:------------------------------------------- |
| [01] | `IImageGenerator` (+ `DelegatingImageGenerator`, `ImageGeneratorExtensions`) | image contract | `ImageGenerationRequest` → `ImageGenerationResponse` |
| [02] | `ISpeechToTextClient` (+ `DelegatingSpeechToTextClient`, extensions) | STT contract | `Stream` → text + streaming updates |
| [03] | `ITextToSpeechClient` (+ `DelegatingTextToSpeechClient`, extensions) | TTS contract | text → audio + streaming updates |
| [04] | `IHostedFileClient` (+ `DelegatingHostedFileClient`, extensions) | file contract | upload/list/download provider-hosted files |
| [05] | `IRealtimeClient` / `IRealtimeClientSession` | realtime contract | bidirectional session (`RealtimeServerMessage`/`RealtimeClientMessage`) |
| [06] | `ReasoningOptions` / `ReasoningEffort` / `ReasoningOutput` | reasoning model | reasoning-budget request + emitted thought |

[PUBLIC_TYPE_SCOPE]: content and annotation contracts (`AIContent` hierarchy)
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:--------------------------------------------------------------------- |:------------------ |:-------------------------------------- |
| [01] | `AIContent` | content base | discriminated content-part base |
| [02] | `TextContent` | text content | plain text message part |
| [03] | `TextReasoningContent` | reasoning content | model chain-of-thought text |
| [04] | `DataContent` / `UriContent` | binary / URI | inline base64/stream part / URI ref |
| [05] | `FunctionCallContent` / `FunctionResultContent` | local tool round-trip | call request / result (paired by `CallId`) |
| [06] | `ToolCallContent` / `ToolResultContent` | hosted tool | generic provider-executed tool round-trip |
| [07] | `McpServerToolCallContent` / `McpServerToolResultContent` | MCP round-trip | remote MCP tool call / result |
| [08] | `CodeInterpreterToolCallContent` / `CodeInterpreterToolResultContent` | code interp round-trip | server code execution call / result |
| [09] | `WebSearchToolCallContent` / `WebSearchToolResultContent` / `ImageGenerationToolCallContent` / `...ResultContent` | hosted round-trip | search / image-gen call + result |
| [10] | `ToolApprovalRequestContent` / `ToolApprovalResponseContent` | approval round-trip | human-in-the-loop tool approval |
| [11] | `ErrorContent` / `UsageContent` / `HostedFileContent` | signal content | structured error / usage / file ref part |
| [12] | `AIAnnotation` / `CitationAnnotation` / `TextSpanAnnotatedRegion` | annotation | source citation + annotated text span |

[PUBLIC_TYPE_SCOPE]: JSON schema utilities
- rail: model-client

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
|:-----: |:-------------------------------------------------------------- |:------------- |:-------------------------------------- |
| [01] | `AIJsonUtilities` | schema utility | schema generation, hashing, transform |
| [02] | `AIJsonSchemaCreateOptions` / `AIJsonSchemaCreateContext` | schema options | generation policy + per-type context |
| [03] | `AIJsonSchemaTransformOptions` / `AIJsonSchemaTransformCache` | transform | post-generation schema rewrite + cache |
| [04] | `AdditionalPropertiesDictionary` / `DataUri` | carrier | untyped provider props / data-URI parse |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chat client operations and streaming fold
- rail: model-client

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:------------------------------------------------------------------------------------------ |:------------- |:--------------------------------------------- |
| [01] | `IChatClient.GetResponseAsync(IEnumerable<ChatMessage>, ChatOptions?, CancellationToken)` | request call | `Task<ChatResponse>` |
| [02] | `IChatClient.GetStreamingResponseAsync(messages, options?, ct)` | streaming call | `IAsyncEnumerable<ChatResponseUpdate>` |
| [03] | `IChatClient.GetService(Type, object? serviceKey = null)` | service call | inner-service / metadata discovery |
| [04] | `ChatClientExtensions.GetResponseAsync(this IChatClient, string \| ChatMessage, options?, ct)` | extension | single-prompt convenience overloads |
| [05] | `ChatClientExtensions.GetStreamingResponseAsync(string \| ChatMessage, …)` | extension | single-prompt streaming overloads |
| [06] | `ChatClientExtensions.GetService<TService>(…)` / `GetRequiredService<TService>(…)` | extension | typed inner-service resolution |
| [07] | `ChatResponseExtensions.ToChatResponse(this IEnumerable<ChatResponseUpdate>)` / `ToChatResponseAsync(this IAsyncEnumerable<…>, ct)` | extension | fold a stream into one `ChatResponse` |
| [08] | `ChatResponseExtensions.AddMessages(this IList<ChatMessage>, ChatResponse \| ChatResponseUpdate, …)` / `AddMessagesAsync(...)` | extension | append turns to running history |
| [09] | `ChatResponse.ToChatResponseUpdates` / `ChatResponse(ChatMessage \| IList<ChatMessage>)` | projection | response ↔ update conversion / construction |
| [10] | `ChatOptions.Clone()` | copy call | clone before per-request mutation |

[ENTRYPOINT_SCOPE]: embedding generator operations
- rail: model-client

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:-------------------------------------------------------------------------------------------------------- |:------------- |:-------------------------------------- |
| [01] | `IEmbeddingGenerator<TInput,TEmbedding>.GenerateAsync(IEnumerable<TInput>, options?, ct)` | generate | `GeneratedEmbeddings<TEmbedding>` batch |
| [02] | `EmbeddingGeneratorExtensions.GenerateVectorAsync<TInput,TElement>(generator, value, options?, ct)` | extension | scalar → `ReadOnlyMemory<TElement>` |
| [03] | `EmbeddingGeneratorExtensions.GenerateAsync<TInput,TEmbedding>(generator, value, …)` | extension | scalar → single `TEmbedding` |
| [04] | `EmbeddingGeneratorExtensions.GenerateAndZipAsync<TInput,TEmbedding>(generator, values, …)` | extension | `(Value, Embedding)[]` pairs |
| [05] | `IEmbeddingGenerator.GetService(Type, object?)` / `GetService<TService>(…)` | service call | inner-service discovery |

[ENTRYPOINT_SCOPE]: tool, function, and multimodal operations
- rail: model-client

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:------------------------------------------------------------------------------------- |:-------------- |:-------------------------------------- |
| [01] | `AIFunctionFactory.Create(Delegate, string? name = null, string? description = null, JsonSerializerOptions?)` | factory call | function from delegate |
| [02] | `AIFunctionFactory.Create(Delegate \| MethodInfo, AIFunctionFactoryOptions?)` | factory call | factory with full schema/binding policy |
| [03] | `AIFunctionFactory.Create(MethodInfo, object? target, …)` / `(MethodInfo, Func<AIFunctionArguments,object> createInstance, …)` | factory call | static / per-call-instance binding |
| [04] | `AIFunctionFactory.CreateDeclaration(string name, string? description, JsonElement jsonSchema, JsonElement? returnJsonSchema = null)` | factory call | schema-first non-invocable declaration |
| [05] | `AIFunction.InvokeAsync(AIFunctionArguments? arguments = null, ct)` | invoke call | `ValueTask<object?>` invocation |
| [06] | `AIFunction.AsDeclarationOnly()` / `AIFunction.UnderlyingMethod` / `JsonSerializerOptions` | projection | declaration view / backing `MethodInfo` |
| [07] | `IImageGenerator.GenerateAsync(ImageGenerationRequest, ImageGenerationOptions?, ct)` | generate | `Task<ImageGenerationResponse>` |
| [08] | `ISpeechToTextClient.GetTextAsync(Stream, options?, ct)` / `GetStreamingTextAsync(...)` | transcribe | audio → text (+ streaming) |
| [09] | `ITextToSpeechClient.GetAudioAsync(string, options?, ct)` / `GetStreamingAudioAsync(...)` | synthesize | text → audio (+ streaming) |
| [10] | `IRealtimeClient.CreateSessionAsync(RealtimeSessionOptions?, ct)` | session | `Task<IRealtimeClientSession>` |

[ENTRYPOINT_SCOPE]: `ChatOptions` properties
- rail: model-client

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:--------------------------------------------------- |:------------- |:----------------------------------- |
| [01] | `Temperature` / `TopP` / `TopK` | option | `float?`/`float?`/`int?` sampling |
| [02] | `MaxOutputTokens` / `Seed` | option | `int?` cap / `long?` reproducibility |
| [03] | `FrequencyPenalty` / `PresencePenalty` / `StopSequences` | option | `float?` penalties / `IList<string>?` |
| [04] | `ResponseFormat` | option | `ChatResponseFormat?` (text or JSON-schema) |
| [05] | `Tools` / `ToolMode` / `AllowMultipleToolCalls` | option | `IList<AITool>?` / `ChatToolMode?` / `bool?` |
| [06] | `Reasoning` | option | `ReasoningOptions?` reasoning budget |
| [07] | `ModelId` / `ConversationId` | option | `string?` model override / stateful id |
| [08] | `AllowBackgroundResponses` | option | `bool?` background streaming gate |
| [09] | `AdditionalProperties` | option | `AdditionalPropertiesDictionary?` provider keys |

[ENTRYPOINT_SCOPE]: JSON schema and content operations
- rail: model-client

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
|:-----: |:----------------------------------------------------------------- |:------------- |:-------------------------------------- |
| [01] | `AIJsonUtilities.CreateJsonSchema(Type, …, AIJsonSchemaCreateOptions?)` | schema call | generate a JSON schema for a CLR type |
| [02] | `AIJsonUtilities.DefaultOptions` | property | shared `JsonSerializerOptions` |
| [03] | `AIJsonUtilities.TransformSchema(JsonElement, AIJsonSchemaTransformOptions)` | schema call | rewrite a schema (strip/annotate) |
| [04] | `new DataContent(string dataUri, string? mediaType = null)` / `DataContent(ReadOnlyMemory<byte>, mediaType)` | ctor | inline binary content part |
| [05] | `ChatMessage(ChatRole, string? \| IList<AIContent>?)` / `ChatMessage.Clone()` | ctor / copy | construct + clone message |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `IChatClient: IDisposable` exposes `GetResponseAsync` + `GetStreamingResponseAsync` + `GetService`.
- `IEmbeddingGenerator: IDisposable` is the non-generic discovery root; `IEmbeddingGenerator<TInput,
 TEmbedding>` carries `GenerateAsync`. The same `Delegating*` + `GetService` shape repeats across
 `IImageGenerator`, `ISpeechToTextClient`, `ITextToSpeechClient`, `IHostedFileClient`, `IRealtimeClient`.
- middleware: `DelegatingChatClient` / `DelegatingEmbeddingGenerator` (and the multimodal `Delegating*`
 bases) wrap an inner client; `GetService(Type, object?)` walks the chain to locate metadata or a
 concrete provider object. All members are thread-safe for concurrent use.
- this catalog documents the `Microsoft.Extensions.AI.Abstractions` net10.0 `IChatClient`
 streaming contract (`GetResponseAsync` → `Task<ChatResponse>`, `GetStreamingResponseAsync` →
 `IAsyncEnumerable<ChatResponseUpdate>`, `GetService(Type, object?)`) that every in-repo provider —
 including the local `OnnxRuntimeGenAIChatClient` (`libs/csharp/Rasm.Compute/.api/api-onnxruntimegenai.md`) — implements; the central
 package pin is the single M.E.AI floor across both catalogs, and the GenAI facade follows that pin.
- the executable pipeline (`AsBuilder()`/`UseFunctionInvocation`/`UseOpenTelemetry`/`UseDistributedCache`)
 and the typed `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` → `ChatResponse<T>` live in the
 concrete `Microsoft.Extensions.AI` core package — referenced by `Rasm.AppHost`, documented at
 `libs/csharp/Rasm.AppHost/.api/api-extensions-ai-middleware.md`, never re-declared here.

[FUNCTION_SURFACE]:
- `AIFunction: AIFunctionDeclaration` adds `InvokeAsync` (`ValueTask<object?>`); a declaration is the
 schema-only manifest form sent to the model, an `AIFunction` is the locally-invocable form.
- `AIFunctionFactory.Create` derives a JSON schema from delegate/`MethodInfo` parameters by reflection;
 `CreateDeclaration` takes a hand-authored `JsonElement` schema (+ optional return schema) for tools
 whose execution is not a local delegate. `AsDeclarationOnly()` strips invocability for the manifest.
- `AIFunctionArguments` is a keyed dictionary; factory binding maps named JSON properties to typed
 parameters using `JsonSerializerOptions`. `ApprovalRequiredAIFunction` gates invocation behind an
 approval round-trip surfaced as `ToolApprovalRequestContent` / `ToolApprovalResponseContent`.
- hosted tools (`HostedWebSearchTool`, `HostedCodeInterpreterTool`, `HostedMcpServerTool`, …) are
 provider-EXECUTED: register them in `ChatOptions.Tools` and consume their results as the paired
 hosted `*ToolCallContent`/`*ToolResultContent` parts rather than invoking a local delegate.

[CONTENT_TOPOLOGY]:
- `AIContent` is the abstract discriminant; every part flows through `ChatMessage.Contents:
 IList<AIContent>` and `ChatResponse.Messages`. `FunctionCallContent.CallId` pairs with
 `FunctionResultContent.CallId` for local tool round-trips; the hosted families
 (`ToolCallContent`/`ToolResultContent`, `McpServerTool*`, `CodeInterpreterTool*`, `WebSearchTool*`,
 `ImageGenerationTool*`) pair their own call/result parts the same way.
- `TextReasoningContent` carries model chain-of-thought; `CitationAnnotation` + `TextSpanAnnotatedRegion`
 attach grounded source spans; `ErrorContent`/`UsageContent` surface structured failure and token use.

[STACKING] — single dense model rail with sibling Compute libs:
- the `Microsoft.Extensions.AI` core builder pipeline (`inner.AsBuilder().UseFunctionInvocation()
.UseDistributedCache(hybridCache).UseOpenTelemetry().Build()`) is composed at the `Rasm.AppHost`
 governance edge — the only project that references the concrete core — and documented at
 `libs/csharp/Rasm.AppHost/.api/api-extensions-ai-middleware.md`; `Microsoft.Extensions.Caching.Hybrid` is the
 `IDistributedCache`-shaped response cache. Compute supplies the inner provider, never the builder:
 `Microsoft.ML.OnnxRuntime` / `OnnxRuntimeGenAI` back a local `IChatClient`/`IEmbeddingGenerator` behind
 these same contracts (`OnnxRuntimeGenAIChatClient`, `libs/csharp/Rasm.Compute/.api/api-onnxruntimegenai.md`).
- fold streaming with `ChatResponseExtensions.ToChatResponseAsync`, then `AddMessages` to grow the
 `IList<ChatMessage>` history; apply an `IChatReducer` before the next send to bound context.
- embeddings: `GenerateVectorAsync` yields `ReadOnlyMemory<float>` that `System.Numerics.Tensors`
 (`TensorPrimitives.CosineSimilarity`) scores directly — no copy; `BinaryEmbedding` packs to bits for
 Hamming search.
- structured output: hand `AIJsonUtilities.CreateJsonSchema(type)` into `ChatResponseFormat`/an
 `AIFunction` so the typed-output round-trip and tool arguments share one schema; the core
 `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` overload (AppHost-owned) then materializes a
 `ChatResponse<T>` whose `.Result` is the bound `T`.

[LOCAL_ADMISSION]:
- Compute consumes `IChatClient`/`IEmbeddingGenerator` (and the multimodal contracts) injected from DI;
 no direct provider construction inside model owners.
- `AIFunction`/hosted tools register in `ChatOptions.Tools`; local functions are invoked by the core
 function-invocation middleware, hosted tools by the provider.
- `ChatOptions.Clone()` (and `ChatMessage.Clone()`) is the canonical copy path before per-request
 mutation; never mutate a shared options or message instance.
- `AdditionalProperties` (`AdditionalPropertiesDictionary`) on options/responses/content carries
 provider-specific keys outside the typed surface.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: provider-agnostic AI client contracts, the tool/function + hosted-tool model, the `AIContent`
 part hierarchy, and JSON-schema utilities
- Accept: chat / embedding / image / speech / realtime via injected clients; tool registration via
 `AIFunctionFactory` and hosted tools; schema generation via `AIJsonUtilities`
- Reject: provider SDK types, direct HTTP calls, and model-specific option objects inside compute
 owners; pipeline middleware composition belongs to the `Microsoft.Extensions.AI` core package
 (`Rasm.AppHost`-referenced, documented at `libs/csharp/Rasm.AppHost/.api/api-extensions-ai-middleware.md`), never
 re-declared as a Compute dependency or catalog
