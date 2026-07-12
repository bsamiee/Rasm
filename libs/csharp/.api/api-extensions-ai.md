# [RASM_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` owns the provider-agnostic client contracts, tool and content models, history reduction, and JSON-schema utilities consumed through `IChatClient`, `IEmbeddingGenerator`, `AIFunction`, and each multimodal client contract.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- Package: `Microsoft.Extensions.AI.Abstractions`
- Assembly: `Microsoft.Extensions.AI.Abstractions` (`lib/net10.0`; targets `net9.0`, `net8.0`, `netstandard2.0`, and `net462`)
- Namespace: `Microsoft.Extensions.AI`
- License: MIT
- Asset: Runtime contracts, content models, and JSON-schema utilities
- Companion: `Microsoft.Extensions.AI` owns the concrete middleware and typed structured-output extension.
- Builder: `IChatClient.AsBuilder()` and `ChatClientBuilder`
- Middleware: `UseFunctionInvocation`, `UseOpenTelemetry`, `UseDistributedCache`, and `UseLogging`
- Structured output: `ChatClientStructuredOutputExtensions.GetResponseAsync<T>(…)` returns `ChatResponse<T>`; `GetStreamingResponseAsync` remains untyped.
- Boundary: `Rasm.Compute` references only `Microsoft.Extensions.AI.Abstractions`, while `Rasm.AppHost` binds the concrete core.
- Implementor: `OnnxRuntimeGenAIChatClient` satisfies the shared `IChatClient` streaming contract.
- Facade: `OnnxRuntimeGenAIChatClient` and `OnnxRuntimeGenAIChatClientOptions`
- Rail: model-client

## [02]-[PUBLIC_TYPES]

[RAIL]: model-client

[CHAT_CLIENTS]:

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]              |
| :-----: | :----------------------- | :--------------- | :------------------------ |
|  [01]   | `IChatClient`            | client contract  | provider-neutral chat     |
|  [02]   | `DelegatingChatClient`   | delegating base  | middleware composition    |
|  [03]   | `ChatClientExtensions`   | extension class  | convenience operations    |
|  [04]   | `ChatResponseExtensions` | extension class  | response accumulation     |
|  [05]   | `ChatMessage`            | message model    | chat content carrier      |
|  [06]   | `ChatRole`               | role value       | message role vocabulary   |
|  [07]   | `ChatOptions`            | request options  | request policy carrier    |
|  [08]   | `ChatResponse`           | response model   | response envelope         |
|  [09]   | `ChatResponseUpdate`     | streaming update | incremental response unit |
|  [10]   | `ChatFinishReason`       | finish reason    | stop vocabulary           |
|  [11]   | `ChatToolMode`           | tool-mode base   | tool dispatch policy      |
|  [12]   | `AutoChatToolMode`       | tool mode        | automatic invocation      |
|  [13]   | `NoneChatToolMode`       | tool mode        | invocation disabled       |
|  [14]   | `RequiredChatToolMode`   | tool mode        | invocation required       |
|  [15]   | `ChatResponseFormat`     | response format  | response format base      |
|  [16]   | `ChatResponseFormatText` | response format  | text responses            |
|  [17]   | `ChatResponseFormatJson` | response format  | JSON responses            |
|  [18]   | `ChatClientMetadata`     | client metadata  | provider identity         |
|  [19]   | `IChatReducer`           | history reducer  | context reduction         |
|  [20]   | `UsageDetails`           | usage model      | token counts              |

[CHAT_MEMBERS]: Message, role, response, and usage members retain one row per symbol.

| [INDEX] | [SYMBOL]                         | [CAPABILITY]          |
| :-----: | :------------------------------- | :-------------------- |
|  [01]   | `ChatMessage.Role`               | message role          |
|  [02]   | `ChatMessage.Contents`           | content parts         |
|  [03]   | `ChatMessage.Text`               | concatenated text     |
|  [04]   | `ChatMessage.Clone()`            | independent copy      |
|  [05]   | `ChatRole.System`                | system message        |
|  [06]   | `ChatRole.User`                  | user message          |
|  [07]   | `ChatRole.Assistant`             | assistant message     |
|  [08]   | `ChatRole.Tool`                  | tool message          |
|  [09]   | `ChatResponse.Text`              | response text         |
|  [10]   | `ChatResponse.Messages`          | response messages     |
|  [11]   | `ChatResponse.Usage`             | usage details         |
|  [12]   | `ChatResponse.FinishReason`      | stop condition        |
|  [13]   | `ChatResponse.ContinuationToken` | response continuation |
|  [14]   | `UsageDetails.InputTokenCount`   | input tokens          |
|  [15]   | `UsageDetails.OutputTokenCount`  | output tokens         |
|  [16]   | `UsageDetails.TotalTokenCount`   | total tokens          |

[EMBEDDING_GENERATORS]:

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]           |
| :-----: | :------------------------------------------------ | :---------------- | :--------------------- |
|  [01]   | `IEmbeddingGenerator`                             | non-generic base  | service discovery      |
|  [02]   | `IEmbeddingGenerator<TInput,TEmbedding>`          | typed contract    | batch generation       |
|  [03]   | `DelegatingEmbeddingGenerator<TInput,TEmbedding>` | delegating base   | middleware composition |
|  [04]   | `EmbeddingGeneratorExtensions`                    | extension class   | scalar projections     |
|  [05]   | `Embedding`                                       | embedding carrier | payload base           |
|  [06]   | `Embedding<T>`                                    | embedding carrier | typed payload          |
|  [07]   | `GeneratedEmbeddings<TEmbedding>`                 | batch result      | generated batch        |
|  [08]   | `BinaryEmbedding`                                 | binary embedding  | bit-packed vector      |
|  [09]   | `EmbeddingGenerationOptions`                      | request options   | generation policy      |

[EMBEDDING_MEMBERS]:

| [INDEX] | [SYMBOL]                                          | [CAPABILITY]        |
| :-----: | :------------------------------------------------ | :------------------ |
|  [01]   | `Embedding<T>.Vector`                             | typed payload       |
|  [02]   | `GeneratedEmbeddings<TEmbedding>.Usage`           | batch usage         |
|  [03]   | `EmbeddingGenerationOptions.ModelId`              | model selection     |
|  [04]   | `EmbeddingGenerationOptions.AdditionalProperties` | provider properties |

[TOOLS_AND_FUNCTIONS]:

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CAPABILITY]           |
| :-----: | :------------------------------------------------ | :--------------- | :--------------------- |
|  [01]   | `AITool`                                          | tool base        | tool description       |
|  [02]   | `AIFunctionDeclaration`                           | declaration base | non-invocable manifest |
|  [03]   | `AIFunction`                                      | function tool    | local invocation       |
|  [04]   | `AIFunctionFactory`                               | function factory | function construction  |
|  [05]   | `AIFunctionArguments`                             | argument carrier | keyed arguments        |
|  [06]   | `AIFunctionFactoryOptions`                        | factory options  | binding policy         |
|  [07]   | `ApprovalRequiredAIFunction`                      | approval gate    | explicit approval      |
|  [08]   | `HostedWebSearchTool`                             | hosted tool      | web search             |
|  [09]   | `HostedCodeInterpreterTool`                       | hosted tool      | code execution         |
|  [10]   | `HostedFileSearchTool`                            | hosted tool      | file search            |
|  [11]   | `HostedImageGenerationTool`                       | hosted tool      | image generation       |
|  [12]   | `HostedToolSearchTool`                            | hosted tool      | tool search            |
|  [13]   | `HostedMcpServerTool`                             | hosted MCP       | remote server tool     |
|  [14]   | `HostedMcpServerToolApprovalMode`                 | approval mode    | approval vocabulary    |
|  [15]   | `HostedMcpServerToolApprovalMode.AlwaysRequire`   | approval value   | unconditional approval |
|  [16]   | `HostedMcpServerToolApprovalMode.NeverRequire`    | approval value   | approval bypass        |
|  [17]   | `HostedMcpServerToolApprovalMode.RequireSpecific` | approval factory | selective approval     |
|  [18]   | `HostedVectorStoreContent`                        | hosted content   | vector-store reference |

[MULTIMODAL_AND_REALTIME]:

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [CAPABILITY]           |
| :-----: | :----------------------------- | :---------------- | :--------------------- |
|  [01]   | `IImageGenerator`              | image contract    | image generation       |
|  [02]   | `DelegatingImageGenerator`     | delegating base   | middleware composition |
|  [03]   | `ImageGeneratorExtensions`     | extension class   | image convenience      |
|  [04]   | `ISpeechToTextClient`          | speech contract   | speech transcription   |
|  [05]   | `DelegatingSpeechToTextClient` | delegating base   | middleware composition |
|  [06]   | `SpeechToTextClientExtensions` | extension class   | transcription helpers  |
|  [07]   | `ITextToSpeechClient`          | speech contract   | speech synthesis       |
|  [08]   | `DelegatingTextToSpeechClient` | delegating base   | middleware composition |
|  [09]   | `TextToSpeechClientExtensions` | extension class   | synthesis helpers      |
|  [10]   | `IHostedFileClient`            | file contract     | hosted-file operations |
|  [11]   | `DelegatingHostedFileClient`   | delegating base   | middleware composition |
|  [12]   | `HostedFileClientExtensions`   | extension class   | hosted-file helpers    |
|  [13]   | `IRealtimeClient`              | realtime contract | session creation       |
|  [14]   | `IRealtimeClientSession`       | realtime contract | bidirectional session  |
|  [15]   | `RealtimeServerMessage`        | realtime message  | server event           |
|  [16]   | `RealtimeClientMessage`        | realtime message  | client event           |
|  [17]   | `ReasoningOptions`             | reasoning model   | reasoning policy       |
|  [18]   | `ReasoningEffort`              | reasoning model   | effort vocabulary      |
|  [19]   | `ReasoningOutput`              | reasoning model   | emitted reasoning      |

[CONTENT_AND_ANNOTATIONS]: `AIContent` owns the part hierarchy.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :--------------------------------- | :---------------- | :-------------------- |
|  [01]   | `AIContent`                        | content base      | discriminated part    |
|  [02]   | `TextContent`                      | text content      | message text          |
|  [03]   | `TextReasoningContent`             | reasoning content | reasoning text        |
|  [04]   | `DataContent`                      | binary content    | inline data           |
|  [05]   | `UriContent`                       | URI content       | external reference    |
|  [06]   | `FunctionCallContent`              | local tool call   | invocation request    |
|  [07]   | `FunctionResultContent`            | local tool result | invocation result     |
|  [08]   | `ToolCallContent`                  | hosted tool call  | provider request      |
|  [09]   | `ToolResultContent`                | hosted result     | provider result       |
|  [10]   | `McpServerToolCallContent`         | MCP call          | remote request        |
|  [11]   | `McpServerToolResultContent`       | MCP result        | remote result         |
|  [12]   | `CodeInterpreterToolCallContent`   | code call         | execution request     |
|  [13]   | `CodeInterpreterToolResultContent` | code result       | execution result      |
|  [14]   | `WebSearchToolCallContent`         | search call       | search request        |
|  [15]   | `WebSearchToolResultContent`       | search result     | search result         |
|  [16]   | `ImageGenerationToolCallContent`   | image call        | generation request    |
|  [17]   | `ImageGenerationToolResultContent` | image result      | generation result     |
|  [18]   | `ToolApprovalRequestContent`       | approval request  | approval prompt       |
|  [19]   | `ToolApprovalResponseContent`      | approval response | approval decision     |
|  [20]   | `ErrorContent`                     | signal content    | structured error      |
|  [21]   | `UsageContent`                     | signal content    | usage signal          |
|  [22]   | `HostedFileContent`                | signal content    | hosted-file reference |
|  [23]   | `AIAnnotation`                     | annotation base   | annotation carrier    |
|  [24]   | `CitationAnnotation`               | annotation        | source citation       |
|  [25]   | `TextSpanAnnotatedRegion`          | annotation        | text span             |

[JSON_SCHEMA_UTILITIES]:

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]    | [CAPABILITY]        |
| :-----: | :------------------------------- | :--------------- | :------------------ |
|  [01]   | `AIJsonUtilities`                | schema utility   | schema operations   |
|  [02]   | `AIJsonSchemaCreateOptions`      | schema options   | generation policy   |
|  [03]   | `AIJsonSchemaCreateContext`      | schema context   | per-type context    |
|  [04]   | `AIJsonSchemaTransformOptions`   | transform policy | rewrite policy      |
|  [05]   | `AIJsonSchemaTransformCache`     | transform cache  | rewrite caching     |
|  [06]   | `AdditionalPropertiesDictionary` | carrier          | provider properties |
|  [07]   | `DataUri`                        | carrier          | data-URI parsing    |

## [03]-[ENTRYPOINTS]

[CHAT_OPERATIONS]: Calls, extensions, projections, and copies retain one row per surface.

| [INDEX] | [OWNER]                  | [SURFACE]                                                          | [RESULT]                               |
| :-----: | :----------------------- | :----------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `IChatClient`            | `GetResponseAsync(IEnumerable<ChatMessage>, options?, ct)`         | `Task<ChatResponse>`                   |
|  [02]   | `IChatClient`            | `GetStreamingResponseAsync(messages, options?, ct)`                | `IAsyncEnumerable<ChatResponseUpdate>` |
|  [03]   | `IChatClient`            | `GetService(Type, serviceKey?)`                                    | inner service                          |
|  [04]   | `ChatClientExtensions`   | `GetResponseAsync(string, options?, ct)`                           | `Task<ChatResponse>`                   |
|  [05]   | `ChatClientExtensions`   | `GetResponseAsync(ChatMessage, options?, ct)`                      | `Task<ChatResponse>`                   |
|  [06]   | `ChatClientExtensions`   | `GetStreamingResponseAsync(string, options?, ct)`                  | streaming updates                      |
|  [07]   | `ChatClientExtensions`   | `GetStreamingResponseAsync(ChatMessage, options?, ct)`             | streaming updates                      |
|  [08]   | `ChatClientExtensions`   | `GetService<TService>(serviceKey?)`                                | optional service                       |
|  [09]   | `ChatClientExtensions`   | `GetRequiredService<TService>(serviceKey?)`                        | required service                       |
|  [10]   | `ChatResponseExtensions` | `ToChatResponse(IEnumerable<ChatResponseUpdate>)`                  | `ChatResponse`                         |
|  [11]   | `ChatResponseExtensions` | `ToChatResponseAsync(IAsyncEnumerable<ChatResponseUpdate>, ct)`    | `Task<ChatResponse>`                   |
|  [12]   | `ChatResponseExtensions` | `AddMessages(IList<ChatMessage>, ChatResponse)`                    | history mutation                       |
|  [13]   | `ChatResponseExtensions` | `AddMessages(IList<ChatMessage>, IEnumerable<ChatResponseUpdate>)` | history mutation                       |
|  [14]   | `ChatResponseExtensions` | `AddMessages(IList<ChatMessage>, ChatResponseUpdate, filter?)`     | history mutation                       |
|  [15]   | `ChatResponseExtensions` | `AddMessagesAsync(IList<ChatMessage>, updates, ct)`                | history mutation                       |
|  [16]   | `ChatResponse`           | `ToChatResponseUpdates()`                                          | response updates                       |
|  [17]   | `ChatResponse`           | `ChatResponse(ChatMessage)`                                        | response construction                  |
|  [18]   | `ChatResponse`           | `ChatResponse(IList<ChatMessage>)`                                 | response construction                  |
|  [19]   | `ChatOptions`            | `Clone()`                                                          | independent options                    |

[EMBEDDING_OPERATIONS]: Generator contracts own batch generation and service discovery.

| [INDEX] | [OWNER]                                  | [SURFACE]                                          | [RESULT]                          |
| :-----: | :--------------------------------------- | :------------------------------------------------- | :-------------------------------- |
|  [01]   | `IEmbeddingGenerator<TInput,TEmbedding>` | `GenerateAsync(IEnumerable<TInput>, options?, ct)` | `GeneratedEmbeddings<TEmbedding>` |
|  [02]   | `IEmbeddingGenerator`                    | `GetService(Type, serviceKey?)`                    | inner service                     |

`EmbeddingGeneratorExtensions` owns scalar, vector, zipped, and typed service projections.

| [INDEX] | [SURFACE]                                                      | [RESULT]                   |
| :-----: | :------------------------------------------------------------- | :------------------------- |
|  [01]   | `GenerateVectorAsync<TInput,TElement>(value, options?, ct)`    | `ReadOnlyMemory<TElement>` |
|  [02]   | `GenerateAsync<TInput,TEmbedding>(value, options?, ct)`        | `TEmbedding`               |
|  [03]   | `GenerateAndZipAsync<TInput,TEmbedding>(values, options?, ct)` | value-embedding pairs      |
|  [04]   | `GetService<TService>(serviceKey?)`                            | optional service           |

[FUNCTION_AND_MULTIMODAL_OPERATIONS]: `Create` binds delegates or `MethodInfo` values, while `CreateDeclaration` binds schema-only tools.

| [INDEX] | [OWNER]               | [SURFACE]                                                              | [RESULT]                        |
| :-----: | :-------------------- | :--------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `AIFunctionFactory`   | `Create(Delegate, name?, description?, JsonSerializerOptions?)`        | `AIFunction`                    |
|  [02]   | `AIFunctionFactory`   | `Create(Delegate, AIFunctionFactoryOptions?)`                          | `AIFunction`                    |
|  [03]   | `AIFunctionFactory`   | `Create(MethodInfo, AIFunctionFactoryOptions?)`                        | `AIFunction`                    |
|  [04]   | `AIFunctionFactory`   | `Create(MethodInfo, target, options?)`                                 | bound function                  |
|  [05]   | `AIFunctionFactory`   | `Create(MethodInfo, createInstance, options?)`                         | per-call binding                |
|  [06]   | `AIFunctionFactory`   | `CreateDeclaration(name, description?, jsonSchema, returnJsonSchema?)` | `AIFunctionDeclaration`         |
|  [07]   | `AIFunction`          | `InvokeAsync(arguments?, ct)`                                          | `ValueTask<object?>`            |
|  [08]   | `AIFunction`          | `AsDeclarationOnly()`                                                  | declaration view                |
|  [09]   | `AIFunction`          | `UnderlyingMethod`                                                     | backing `MethodInfo`            |
|  [10]   | `AIFunction`          | `JsonSerializerOptions`                                                | serializer policy               |
|  [11]   | `IImageGenerator`     | `GenerateAsync(ImageGenerationRequest, options?, ct)`                  | `Task<ImageGenerationResponse>` |
|  [12]   | `ISpeechToTextClient` | `GetTextAsync(Stream, options?, ct)`                                   | text response                   |
|  [13]   | `ISpeechToTextClient` | `GetStreamingTextAsync(Stream, options?, ct)`                          | streaming text                  |
|  [14]   | `ITextToSpeechClient` | `GetAudioAsync(string, options?, ct)`                                  | audio response                  |
|  [15]   | `ITextToSpeechClient` | `GetStreamingAudioAsync(string, options?, ct)`                         | streaming audio                 |
|  [16]   | `IRealtimeClient`     | `CreateSessionAsync(RealtimeSessionOptions?, ct)`                      | `Task<IRealtimeClientSession>`  |

[CHAT_OPTIONS]: Every row is one independently mutable request property.

| [INDEX] | [SURFACE]                  | [TYPE]                            | [CAPABILITY]          |
| :-----: | :------------------------- | :-------------------------------- | :-------------------- |
|  [01]   | `Temperature`              | `float?`                          | sampling temperature  |
|  [02]   | `TopP`                     | `float?`                          | nucleus sampling      |
|  [03]   | `TopK`                     | `int?`                            | candidate sampling    |
|  [04]   | `MaxOutputTokens`          | `int?`                            | output cap            |
|  [05]   | `Seed`                     | `long?`                           | reproducibility       |
|  [06]   | `FrequencyPenalty`         | `float?`                          | frequency penalty     |
|  [07]   | `PresencePenalty`          | `float?`                          | presence penalty      |
|  [08]   | `StopSequences`            | `IList<string>?`                  | stopping vocabulary   |
|  [09]   | `ResponseFormat`           | `ChatResponseFormat?`             | response shape        |
|  [10]   | `Tools`                    | `IList<AITool>?`                  | available tools       |
|  [11]   | `ToolMode`                 | `ChatToolMode?`                   | invocation policy     |
|  [12]   | `AllowMultipleToolCalls`   | `bool?`                           | parallel tool calls   |
|  [13]   | `Reasoning`                | `ReasoningOptions?`               | reasoning budget      |
|  [14]   | `ModelId`                  | `string?`                         | model override        |
|  [15]   | `ConversationId`           | `string?`                         | stateful conversation |
|  [16]   | `AllowBackgroundResponses` | `bool?`                           | background responses  |
|  [17]   | `AdditionalProperties`     | `AdditionalPropertiesDictionary?` | provider properties   |

[SCHEMA_AND_CONTENT_OPERATIONS]:

| [INDEX] | [OWNER]           | [SURFACE]                                                    | [RESULT]              |
| :-----: | :---------------- | :----------------------------------------------------------- | :-------------------- |
|  [01]   | `AIJsonUtilities` | `CreateJsonSchema(Type, options?)`                           | JSON schema           |
|  [02]   | `AIJsonUtilities` | `DefaultOptions`                                             | serializer options    |
|  [03]   | `AIJsonUtilities` | `TransformSchema(JsonElement, AIJsonSchemaTransformOptions)` | rewritten schema      |
|  [04]   | `AIJsonUtilities` | `HashDataToString(ReadOnlySpan<object>, options?)`           | schema hash           |
|  [05]   | `DataContent`     | `DataContent(string dataUri, string? mediaType = null)`      | inline binary content |
|  [06]   | `DataContent`     | `DataContent(ReadOnlyMemory<byte>, mediaType)`               | inline binary content |
|  [07]   | `ChatMessage`     | `ChatMessage(ChatRole, string?)`                             | text message          |
|  [08]   | `ChatMessage`     | `ChatMessage(ChatRole, IList<AIContent>?)`                   | multimodal message    |
|  [09]   | `ChatMessage`     | `Clone()`                                                    | independent message   |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `IChatClient: IDisposable` binds `GetResponseAsync`, `GetStreamingResponseAsync`, and `GetService`.
- `IEmbeddingGenerator: IDisposable` is the non-generic discovery root, and `IEmbeddingGenerator<TInput,TEmbedding>` binds `GenerateAsync`.
- Each multimodal client repeats the delegating base and `GetService` shape.
- `DelegatingChatClient` and `DelegatingEmbeddingGenerator` wrap an inner client, while `GetService(Type, object?)` traverses the chain for metadata or a provider object.
- Every client member is thread-safe for concurrent use.
- `OnnxRuntimeGenAIChatClient` implements the shared `IChatClient` streaming contract.
- `Microsoft.Extensions.AI` owns the builder pipeline and typed `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` projection bound by `Rasm.AppHost`.

[FUNCTION_SURFACE]:
- `AIFunction: AIFunctionDeclaration` adds `InvokeAsync` returning `ValueTask<object?>`; the base declaration remains a schema-only manifest.
- `AIFunctionFactory.Create` derives parameter schemas from a delegate or `MethodInfo` through reflection.
- `CreateDeclaration` binds an authored `JsonElement` schema and optional return schema without a local delegate.
- `AsDeclarationOnly()` projects an invocable function to its manifest form.
- `AIFunctionArguments` carries keyed arguments, and factory binding maps named JSON properties to typed parameters through `JsonSerializerOptions`.
- `ApprovalRequiredAIFunction` gates invocation through paired `ToolApprovalRequestContent` and `ToolApprovalResponseContent` parts.
- The provider executes hosted tools registered in `ChatOptions.Tools`; paired hosted call and result content carries each round-trip.

[CONTENT_TOPOLOGY]:
- `AIContent` is the abstract discriminant carried by `ChatMessage.Contents` and `ChatResponse.Messages`.
- `FunctionCallContent.CallId` pairs with `FunctionResultContent.CallId` for local tool round-trips.
- Each hosted tool family pairs its call and result content through the same identity shape.
- `TextReasoningContent` carries model reasoning, while `CitationAnnotation` and `TextSpanAnnotatedRegion` attach grounded source spans.
- `ErrorContent` and `UsageContent` carry structured failure and token use.

[STACKING]:
- `Rasm.AppHost` composes `inner.AsBuilder().UseFunctionInvocation().UseDistributedCache(hybridCache).UseOpenTelemetry().Build()` and binds `Microsoft.Extensions.Caching.Hybrid` as the response cache.
- `Microsoft.ML.OnnxRuntime` and `OnnxRuntimeGenAI` back local `IChatClient` and `IEmbeddingGenerator` implementations behind the shared contracts.
- `ChatResponseExtensions.ToChatResponseAsync` folds streaming output, `AddMessages` grows the history, and `IChatReducer` bounds context before the next send.
- `GenerateVectorAsync` yields `ReadOnlyMemory<float>` consumed directly by `TensorPrimitives.CosineSimilarity`, while `BinaryEmbedding` packs vectors for Hamming search.
- `AIJsonUtilities.CreateJsonSchema(type)` binds the same schema to `ChatResponseFormat` or `AIFunction`, and `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` materializes `ChatResponse<T>.Result`.

[LOCAL_ADMISSION]:
- Compute consumes DI-injected `IChatClient`, `IEmbeddingGenerator`, and multimodal contracts; model owners never construct providers.
- `AIFunction` and hosted tools register in `ChatOptions.Tools`; middleware invokes local functions, while the provider invokes hosted tools.
- `ChatOptions.Clone()` and `ChatMessage.Clone()` create the per-request copies mutated by a caller.
- `AdditionalPropertiesDictionary` carries provider-specific keys outside the typed options, responses, and content surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: Provider-agnostic client contracts, tool and content models, and JSON-schema utilities
- Accept: Injected clients, `AIFunctionFactory`, hosted tools, and `AIJsonUtilities`
- Reject: Provider SDK types, direct HTTP calls, and model-specific options inside Compute owners; `Microsoft.Extensions.AI` owns middleware composition.
