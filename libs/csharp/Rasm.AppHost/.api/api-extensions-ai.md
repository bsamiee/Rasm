# [RASM_APPHOST_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` supplies provider-agnostic contracts for chat,
embedding generation, image generation, speech-to-text, text-to-speech, realtime sessions,
hosted file management, tool definition and invocation, content part modeling, and JSON
schema utilities that AppHost capability-agent owners consume through `IChatClient`,
`IEmbeddingGenerator`, `AIFunction`, and the extended modal clients without coupling to a
specific AI provider.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- version: `10.7.0`
- asset: runtime library (net10.0)
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat client contracts
- rail: capability-agent

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                        |
| :-----: | :--------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `IChatClient`          | client contract  | provider-agnostic chat — `IDisposable`        |
|  [02]   | `DelegatingChatClient` | delegating base  | middleware pipeline composition base          |
|  [03]   | `ChatMessage`          | message model    | `Role`, `Contents`, `AuthorName`, `MessageId` |
|  [04]   | `ChatRole`             | role value       | `System`, `User`, `Assistant`, `Tool`         |
|  [05]   | `ChatOptions`          | request options  | temperature, tools, format, reasoning, seed   |
|  [06]   | `ChatResponse`         | response model   | `Messages`, `ResponseId`, `ConversationId`    |
|  [07]   | `ChatResponseUpdate`   | streaming update | incremental streaming response unit           |
|  [08]   | `ChatFinishReason`     | finish reason    | stop condition vocabulary                     |
|  [09]   | `ChatToolMode`         | tool mode        | `AutoChatToolMode`, `None`, `Required`        |
|  [10]   | `ChatResponseFormat`   | response format  | `Text` / `Json` / `ChatResponseFormatJson`    |
|  [11]   | `ChatClientMetadata`   | client metadata  | provider name and model identity              |
|  [12]   | `UsageDetails`         | usage model      | input/output/total token counts               |
|  [13]   | `IChatReducer`         | reducer contract | shrinks a `ChatMessage` list                  |
|  [14]   | `ReasoningOptions`     | reasoning config | `Effort`, `Output` for chain-of-thought       |
|  [15]   | `ReasoningEffort`      | effort value     | level of reasoning computation                |
|  [16]   | `ReasoningOutput`      | output mode      | how reasoning content is surfaced             |

[PUBLIC_TYPE_SCOPE]: embedding generator contracts
- rail: capability-agent

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]      | [RAIL]                                 |
| :-----: | :--------------------------------- | :----------------- | :------------------------------------- |
|  [01]   | `IEmbeddingGenerator`              | non-generic base   | service-discovery root — `IDisposable` |
|  [02]   | `IEmbeddingGenerator<TInput,TEmb>` | typed contract     | `GenerateAsync` surface                |
|  [03]   | `DelegatingEmbeddingGenerator`     | delegating base    | middleware composition base            |
|  [04]   | `Embedding<TEmbedding>`            | embedding carrier  | typed embedding payload                |
|  [05]   | `GeneratedEmbeddings<T>`           | batch result       | embedding batch response               |
|  [06]   | `EmbeddingGenerationOptions`       | request options    | model and additional properties        |
|  [07]   | `EmbeddingGeneratorMetadata`       | generator metadata | provider name and model identity       |
|  [08]   | `BinaryEmbedding`                  | binary embedding   | bit-packed embedding vector            |

[PUBLIC_TYPE_SCOPE]: tool and function contracts
- rail: capability-agent

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [RAIL]                                        |
| :-----: | :--------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `AITool`                     | tool base        | `Name`, `Description`, `AdditionalProperties` |
|  [02]   | `AIFunctionDeclaration`      | declaration base | `JsonSchema`, `ReturnJsonSchema`              |
|  [03]   | `AIFunction`                 | function tool    | adds `InvokeAsync` + `UnderlyingMethod`       |
|  [04]   | `AIFunctionFactory`          | function factory | creates `AIFunction` from delegate/reflection |
|  [05]   | `AIFunctionFactoryOptions`   | factory options  | JSON schema and binding policy                |
|  [06]   | `AIFunctionArguments`        | argument carrier | `IDictionary<string,object?>` keyed args      |
|  [07]   | `ApprovalRequiredAIFunction` | approval gate    | requires explicit approval before invoke      |
|  [08]   | `DelegatingAIFunction`       | delegating base  | composable function wrapper                   |
|  [09]   | `HostedMcpServerTool`        | hosted MCP tool  | MCP server tool reference for AI service      |
|  [10]   | `HostedWebSearchTool`        | hosted tool      | web-search hosted tool reference              |
|  [11]   | `HostedFileSearchTool`       | hosted tool      | file-search hosted tool reference             |
|  [12]   | `HostedCodeInterpreterTool`  | hosted tool      | code-interpreter hosted tool reference        |
|  [13]   | `HostedToolSearchTool`       | hosted tool      | tool-search hosted tool reference             |
|  [14]   | `HostedImageGenerationTool`  | hosted tool      | image-generation hosted tool reference        |

[PUBLIC_TYPE_SCOPE]: content and annotation contracts
- rail: capability-agent

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]       | [RAIL]                             |
| :-----: | :------------------------- | :------------------ | :--------------------------------- |
|  [01]   | `AIContent`                | content base        | discriminated content part base    |
|  [02]   | `TextContent`              | text content        | plain text message part            |
|  [03]   | `DataContent`              | binary content      | base64-encoded or stream data part |
|  [04]   | `UriContent`               | URI content         | URI-referenced content part        |
|  [05]   | `TextReasoningContent`     | reasoning content   | model chain-of-thought text        |
|  [06]   | `FunctionCallContent`      | call request        | tool invocation request part       |
|  [07]   | `FunctionResultContent`    | call result         | tool invocation result part        |
|  [08]   | `ErrorContent`             | error content       | structured error from model        |
|  [09]   | `UsageContent`             | usage content       | usage statistics content part      |
|  [10]   | `ToolCallContent`          | hosted call request | provider-managed tool call part    |
|  [11]   | `ToolResultContent`        | hosted call result  | provider-managed tool result part  |
|  [12]   | `InputRequestContent`      | input request       | model input request content part   |
|  [13]   | `InputResponseContent`     | input response      | model input response content part  |
|  [14]   | `HostedFileContent`        | hosted file ref     | reference to server-side file      |
|  [15]   | `HostedVectorStoreContent` | vector store ref    | reference to a hosted vector store |
|  [16]   | `AIAnnotation`             | annotation base     | structured annotation on content   |
|  [17]   | `CitationAnnotation`       | citation annotation | source citation annotation         |
|  [18]   | `AnnotatedRegion<T>`       | annotated region    | typed region with annotations      |

[PUBLIC_TYPE_SCOPE]: modal client contracts (experimental `MEAI001`)
- rail: capability-agent

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :----------------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `ISpeechToTextClient`          | STT contract      | `GetTextAsync`, `GetStreamingTextAsync`         |
|  [02]   | `ITextToSpeechClient`          | TTS contract      | `GetAudioAsync`, `GetStreamingAudioAsync`       |
|  [03]   | `IImageGenerator`              | image contract    | `GenerateAsync(ImageGenerationRequest)`         |
|  [04]   | `IRealtimeClient`              | realtime contract | `CreateSessionAsync` → `IRealtimeClientSession` |
|  [05]   | `IHostedFileClient`            | file contract     | `UploadAsync`, `DownloadAsync`, `DeleteAsync`   |
|  [06]   | `DelegatingSpeechToTextClient` | delegating base   | STT middleware base                             |
|  [07]   | `DelegatingTextToSpeechClient` | delegating base   | TTS middleware base                             |
|  [08]   | `DelegatingImageGenerator`     | delegating base   | image middleware base                           |
|  [09]   | `DelegatingRealtimeClient`     | delegating base   | realtime middleware base                        |
|  [10]   | `DelegatingHostedFileClient`   | delegating base   | file client middleware base                     |

[PUBLIC_TYPE_SCOPE]: JSON schema utilities
- rail: capability-agent

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]  | [RAIL]                                              |
| :-----: | :------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `AIJsonUtilities`                | schema utility | JSON schema generation and SHA-384 hashing          |
|  [02]   | `AIJsonSchemaCreateOptions`      | schema options | schema generation policy                            |
|  [03]   | `AIJsonSchemaCreateContext`      | schema context | per-type schema creation context                    |
|  [04]   | `AIJsonSchemaTransformCache`     | schema cache   | memoized schema transform cache                     |
|  [05]   | `AdditionalPropertiesDictionary` | properties bag | `IDictionary<string,object?>` for options/responses |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chat client operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `IChatClient.GetResponseAsync(messages, options?, ct)`        | request call   | returns `Task<ChatResponse>`                   |
|  [02]   | `IChatClient.GetStreamingResponseAsync(msgs, opts?, ct)`      | streaming call | returns `IAsyncEnumerable<ChatResponseUpdate>` |
|  [03]   | `IChatClient.GetService(serviceType, serviceKey?)`            | service call   | inner service discovery                        |
|  [04]   | `ChatClientExtensions.GetService<TService>(client, key?)`     | extension call | typed service discovery                        |
|  [05]   | `ChatClientExtensions.GetRequiredService(client, type, key?)` | extension call | throwing typed service lookup                  |
|  [06]   | `ChatClientExtensions.GetResponseAsync(client, text, ...)`    | extension call | text shorthand overload                        |
|  [07]   | `ChatOptions.Clone()`                                         | copy call      | shallow clone of request options               |
|  [08]   | `IChatReducer.ReduceAsync(messages, ct)`                      | reduce call    | shrinks `IEnumerable<ChatMessage>`             |
|  [09]   | `ChatResponse.Usage` / `ChatResponse.Messages`               | property       | `UsageDetails?` aggregated usage; `IList<ChatMessage>` response messages |
|  [10]   | `UsageDetails.InputTokenCount` / `OutputTokenCount` / `TotalTokenCount` | property | `long?` token counts the cost meter reads (also `CachedInputTokenCount`/`ReasoningTokenCount`) |
|  [11]   | `ChatResponseExtensions.ToChatResponse(this IEnumerable<ChatResponseUpdate>)` | extension call | folds a streamed update list into one `ChatResponse` (async twin `ToChatResponseAsync(IAsyncEnumerable<…>, CT)`) |
|  [12]   | `ChatToolMode.Auto` / `.None` / `.RequireAny` / `.RequireSpecific(string)`    | static accessor | `AutoChatToolMode`/`NoneChatToolMode`/`RequiredChatToolMode` singletons seated on `ChatOptions.ToolMode` |

[ENTRYPOINT_SCOPE]: embedding generator operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------- |
|  [01]   | `IEmbeddingGenerator<TIn,TEmb>.GenerateAsync(values, options?, ct)` | generate       | embedding batch generation |
|  [02]   | `IEmbeddingGenerator.GetService(serviceType, serviceKey?)`          | service call   | inner service discovery    |
|  [03]   | `EmbeddingGeneratorExtensions.GenerateAsync(gen, value, opts?, ct)` | extension call | scalar input overload      |
|  [04]   | `EmbeddingGeneratorExtensions.GetService<TService>(gen, key?)`      | extension call | typed service discovery    |
|  [05]   | `Embedding<TEmbedding>.Vector`                                      | property       | `ReadOnlyMemory<TEmbedding>` payload; `.Vector.Span` feeds the cosine rank |
|  [06]   | `GeneratedEmbeddings<TEmbedding>[index]`                            | batch indexer  | per-input `Embedding<TEmbedding>` (`IList<Embedding<T>>`) from the batch result |

[ENTRYPOINT_SCOPE]: tool and function operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]  | [RAIL]                                |
| :-----: | :-------------------------------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `AIFunctionFactory.Create(Delegate, options?)`            | factory call    | creates function from delegate        |
|  [02]   | `AIFunctionFactory.Create(MethodInfo, target?, options?)` | factory call    | creates function from reflection      |
|  [03]   | `AIFunction.InvokeAsync(arguments?, ct)`                  | invoke call     | returns `ValueTask<object?>`          |
|  [04]   | `AIFunction.AsDeclarationOnly()`                          | projection call | non-invocable `AIFunctionDeclaration` |
|  [05]   | `AIFunction.UnderlyingMethod`                             | property        | backing `MethodInfo?` or null         |
|  [06]   | `AIFunction.JsonSerializerOptions`                        | property        | serializer options for arguments      |
|  [07]   | `AIFunctionDeclaration.JsonSchema`                        | property        | `JsonElement` parameter schema        |
|  [08]   | `AIFunctionDeclaration.ReturnJsonSchema`                  | property        | `JsonElement?` return schema          |
|  [09]   | `AIFunction.InvokeCoreAsync(AIFunctionArguments, CancellationToken)`       | override seam    | `protected abstract ValueTask<object?>` — the subclass override point `InvokeAsync` delegates to (a custom `AIFunction` overrides this, never `InvokeAsync`) |

[ENTRYPOINT_SCOPE]: ChatOptions properties
- rail: capability-agent

| [INDEX] | [SURFACE]                  | [TYPE]                | [RAIL]                          |
| :-----: | :------------------------- | :-------------------- | :------------------------------ |
|  [01]   | `Temperature`              | `float?`              | sampling temperature            |
|  [02]   | `MaxOutputTokens`          | `int?`                | max tokens in response          |
|  [03]   | `TopP`                     | `float?`              | nucleus sampling factor         |
|  [04]   | `TopK`                     | `int?`                | top-K token count               |
|  [05]   | `FrequencyPenalty`         | `float?`              | repetition penalty by frequency |
|  [06]   | `PresencePenalty`          | `float?`              | repetition penalty by presence  |
|  [07]   | `Seed`                     | `long?`               | reproducibility seed            |
|  [08]   | `ResponseFormat`           | `ChatResponseFormat?` | output shape constraint         |
|  [09]   | `Tools`                    | `IList<AITool>?`      | tool list                       |
|  [10]   | `ToolMode`                 | `ChatToolMode?`       | tool invocation mode            |
|  [11]   | `ModelId`                  | `string?`             | model override                  |
|  [12]   | `Reasoning`                | `ReasoningOptions?`   | chain-of-thought configuration  |
|  [13]   | `ConversationId`           | `string?`             | stateful conversation ID        |
|  [14]   | `Instructions`             | `string?`             | per-request system instructions |
|  [15]   | `AllowMultipleToolCalls`   | `bool?`               | parallel tool calls gate        |
|  [16]   | `AllowBackgroundResponses` | `bool?`               | background streaming gate       |

[ENTRYPOINT_SCOPE]: modal client operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :-------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `ISpeechToTextClient.GetTextAsync(stream, options?, ct)`              | STT call       | returns `Task<SpeechToTextResponse>`           |
|  [02]   | `ISpeechToTextClient.GetStreamingTextAsync(stream, options?, ct)`     | STT stream     | `IAsyncEnumerable<SpeechToTextResponseUpdate>` |
|  [03]   | `ITextToSpeechClient.GetAudioAsync(text, options?, ct)`               | TTS call       | returns `Task<TextToSpeechResponse>`           |
|  [04]   | `ITextToSpeechClient.GetStreamingAudioAsync(text, options?, ct)`      | TTS stream     | `IAsyncEnumerable<TextToSpeechResponseUpdate>` |
|  [05]   | `IImageGenerator.GenerateAsync(request, options?, ct)`                | image call     | returns `Task<ImageGenerationResponse>`        |
|  [06]   | `IRealtimeClient.CreateSessionAsync(options?, ct)`                    | session call   | returns `Task<IRealtimeClientSession>`         |
|  [07]   | `IHostedFileClient.UploadAsync(stream, mediaType?, name?, opts?, ct)` | file call      | returns `Task<HostedFileContent>`              |

[ENTRYPOINT_SCOPE]: JSON schema and utility operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `AIJsonUtilities.CreateJsonSchema(type, options?)` | schema call    | generates `JsonElement` schema for a type |
|  [02]   | `AIJsonUtilities.DefaultOptions`                   | property       | shared `JsonSerializerOptions`            |
|  [03]   | `DataUriParser.TryParse(dataUri, out DataUri?)`    | parse call     | parses a data URI string                  |

[ENTRYPOINT_SCOPE]: message and content-part members
- rail: capability-agent

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY]  | [RAIL]                                                              |
| :-----: | :------------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `new ChatMessage(ChatRole role, string? content)` / `new ChatMessage(ChatRole, IList<AIContent>?)` | ctor            | constructs a message from text or content parts                    |
|  [02]   | `ChatMessage.Text`                                                                                 | property        | concatenated `TextContent` text (`AIContent.ConcatText()`, reasoning parts skipped) |
|  [03]   | `ChatResponseUpdate.Contents`                                                                      | property        | `IList<AIContent>` streamed-update parts, indexable for in-place rewrite |
|  [04]   | `TextContent(string)` / `TextContent.Text` / `TextReasoningContent.Text`                           | ctor / property | text and chain-of-thought part construction and read              |
|  [05]   | `AIContent.RawRepresentation`                                                                      | property        | `object?` underlying provider object on any content part          |
|  [06]   | `FunctionCallContent.Name` / `.CallId` / `.Arguments`                                              | property        | tool-call name, pairing id, `IDictionary<string,object?>?` args; ctor `(callId, name, arguments?)` |
|  [07]   | `FunctionResultContent.Result` / `.CallId`                                                         | property        | tool-result `object?` payload and pairing id; ctor `(string callId, object? result)` |
|  [08]   | `DelegatingChatClient(IChatClient innerClient)` / `.InnerClient`                                   | ctor / property | protected middleware base ctor and inner-client handle a custom delegating client overrides over |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- namespace: `Microsoft.Extensions.AI`; 180 public types across 5 namespaces
- chat contract: `IChatClient : IDisposable` with `GetResponseAsync` and `GetStreamingResponseAsync`
- embedding contract: `IEmbeddingGenerator : IDisposable` (non-generic); typed surface is `IEmbeddingGenerator<TInput,TEmbedding>`
- experimental modal contracts: `ISpeechToTextClient`, `ITextToSpeechClient`, `IImageGenerator`, `IRealtimeClient`, `IHostedFileClient` all carry `[Experimental("MEAI001")]`
- middleware: `DelegatingChatClient`, `DelegatingEmbeddingGenerator`, and modal delegating bases wrap an inner client for pipeline composition
- service discovery: `GetService(Type, object?)` on each contract locates inner services such as metadata objects
- thread safety: all members of primary contracts are thread-safe for concurrent use; implementations must not be disposed while in use

[FUNCTION_SURFACE]:
- `AITool` is the base; `AIFunctionDeclaration : AITool` adds `JsonSchema` and `ReturnJsonSchema`; `AIFunction : AIFunctionDeclaration` adds `InvokeAsync`
- `AIFunctionFactory.Create` accepts a delegate or `MethodInfo` and derives a JSON schema from parameters via reflection
- `AIFunctionArguments` is a keyed `IDictionary<string,object?>` dictionary; factory binding maps named JSON properties to typed parameters
- `AsDeclarationOnly()` returns a non-invocable `AIFunctionDeclaration` for tool manifests sent to models without execution rights
- hosted tool types (`HostedMcpServerTool`, `HostedWebSearchTool`, etc.) are `AITool` descendants that reference server-managed tools by identity

[CONTENT_TOPOLOGY]:
- `AIContent` is the abstract base; all parts flow through `ChatMessage.Contents : IList<AIContent>`
- `FunctionCallContent.CallId` pairs with `FunctionResultContent.CallId` for tool round-trips
- `ToolCallContent` / `ToolResultContent` are hosted-tool variants for provider-managed tool surfaces
- `AnnotatedRegion<T>` carries arbitrary `AIAnnotation` attachments on typed content regions
- `AdditionalPropertiesDictionary` on options and responses carries provider-specific keys outside the strongly typed surface

[LOCAL_ADMISSION]:
- AppHost consumes `IChatClient` and `IEmbeddingGenerator` injected from DI; no direct provider construction inside capability-agent owners.
- `AIFunction` surfaces registered in `ChatOptions.Tools` are declared by agent owners and invoked by the middleware pipeline.
- `ChatOptions.Clone()` is the canonical copy path before per-request mutation; shared option instances must never be mutated.
- Experimental modal clients (`ISpeechToTextClient`, `ITextToSpeechClient`, `IImageGenerator`, `IRealtimeClient`, `IHostedFileClient`) require `[Experimental("MEAI001")]` suppression at call sites.
- `ReasoningOptions` is provider-transparent; implementations make best-effort mapping and may silently ignore unsupported fields.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: provider-agnostic AI client contracts, content model, and JSON schema utilities
- Accept: chat, embedding, and modal requests via injected clients; tool registration via `AIFunctionFactory`
- Reject: provider SDK types, direct HTTP calls, model-specific option objects, and hand-rolled JSON schema derivation inside capability-agent owners
