# [RASM_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` owns the provider-neutral model-client contracts every Rasm model consumer binds — chat, embedding, image, speech, hosted-file, and realtime — with the `AIContent` part algebra their messages carry, the `AITool`/`AIFunction` tool algebra, and the JSON-schema utilities that mint one schema for structured output and tool binding alike. Consumers hold contracts and receive implementations through DI; provider SDKs, middleware, and builder composition stop at the host boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions` (MIT, Microsoft)
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- rail: model-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat contract, request policy, and response envelope

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]    | [CAPABILITY]                   |
| :-----: | :--------------------------------------- | :--------------- | :----------------------------- |
|  [01]   | `IChatClient`                            | client contract  | provider-neutral chat          |
|  [02]   | `IChatReducer`                           | reducer contract | history reduction              |
|  [03]   | `ChatClientExtensions`                   | extension class  | convenience calls              |
|  [04]   | `ChatResponseExtensions`                 | extension class  | response accumulation          |
|  [05]   | `ChatMessage`                            | message model    | chat content carrier           |
|  [06]   | `ChatRole`                               | struct           | message role vocabulary        |
|  [07]   | `ChatOptions`                            | request options  | request policy carrier         |
|  [08]   | `ChatResponse`                           | response model   | response envelope              |
|  [09]   | `ChatResponseUpdate`                     | streaming update | incremental response unit      |
|  [10]   | `ChatFinishReason`                       | struct           | stop vocabulary                |
|  [11]   | `ChatToolMode`                           | tool-mode base   | tool dispatch policy           |
|  [12]   | `AutoChatToolMode`                       | tool mode        | automatic invocation           |
|  [13]   | `NoneChatToolMode`                       | tool mode        | invocation disabled            |
|  [14]   | `RequiredChatToolMode`                   | tool mode        | invocation required            |
|  [15]   | `ChatResponseFormat`                     | format base      | response format root           |
|  [16]   | `ChatResponseFormatText`                 | format value     | text responses                 |
|  [17]   | `ChatResponseFormatJson`                 | format value     | JSON and JSON-schema responses |
|  [18]   | `ReasoningOptions`                       | request options  | reasoning budget               |
|  [19]   | `ReasoningEffort`                        | enum             | effort vocabulary              |
|  [20]   | `ReasoningOutput`                        | enum             | emitted-reasoning vocabulary   |
|  [21]   | `UsageDetails`                           | usage model      | token accounting               |
|  [22]   | `ResponseContinuationToken`              | continuation     | resumable response handle      |
|  [23]   | `AdditionalPropertiesDictionary`         | property bag     | provider-keyed values          |
|  [24]   | `AdditionalPropertiesDictionary<TValue>` | property bag     | typed provider-keyed values    |

[CHAT_MESSAGE]: `Role` `AuthorName` `Contents` `Text` `MessageId` `CreatedAt` `RawRepresentation` `AdditionalProperties` `Clone()`
[CHAT_RESPONSE]: `Messages` `Text` `ResponseId` `ConversationId` `ModelId` `CreatedAt` `FinishReason` `Usage` `ContinuationToken` `RawRepresentation` `AdditionalProperties`
[CHAT_ROLE]: `System` `User` `Assistant` `Tool`
[CHAT_FINISH_REASON]: `Stop` `Length` `ToolCalls` `ContentFilter`
[USAGE_DETAILS]: `InputTokenCount` `OutputTokenCount` `TotalTokenCount` `CachedInputTokenCount` `ReasoningTokenCount` `InputTextTokenCount` `InputAudioTokenCount` `OutputTextTokenCount` `OutputAudioTokenCount` `AdditionalCounts`

[PUBLIC_TYPE_SCOPE]: embedding generation

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :--------------------------------------- | :---------------- | :-------------------- |
|  [01]   | `IEmbeddingGenerator`                    | discovery root    | service traversal     |
|  [02]   | `IEmbeddingGenerator<TInput,TEmbedding>` | typed contract    | batch generation      |
|  [03]   | `EmbeddingGeneratorExtensions`           | extension class   | scalar projections    |
|  [04]   | `Embedding`                              | embedding base    | model and dimensions  |
|  [05]   | `Embedding<T>`                           | embedding carrier | typed dense vector    |
|  [06]   | `BinaryEmbedding`                        | embedding carrier | bit-packed vector     |
|  [07]   | `GeneratedEmbeddings<TEmbedding>`        | batch result      | list with batch usage |
|  [08]   | `EmbeddingGenerationOptions`             | request options   | generation policy     |

[EMBEDDING]: `ModelId` `CreatedAt` `Dimensions` `AdditionalProperties`
[EMBEDDING_PAYLOAD]: `Embedding<T>.Vector` `BinaryEmbedding.Vector` `GeneratedEmbeddings<TEmbedding>.Usage`
[EMBEDDING_OPTIONS]: `Dimensions` `ModelId` `RawRepresentationFactory` `AdditionalProperties`

[PUBLIC_TYPE_SCOPE]: tool algebra and function binding

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CAPABILITY]            |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------- |
|  [01]   | `AITool`                                         | tool base         | named tool description  |
|  [02]   | `AIFunctionDeclaration`                          | declaration base  | non-invocable manifest  |
|  [03]   | `AIFunction`                                     | function tool     | local invocation        |
|  [04]   | `ApprovalRequiredAIFunction`                     | approval gate     | explicit approval       |
|  [05]   | `AIFunctionFactory`                              | function factory  | function construction   |
|  [06]   | `AIFunctionFactoryOptions`                       | factory options   | binding policy          |
|  [07]   | `AIFunctionArguments`                            | argument carrier  | keyed arguments with DI |
|  [08]   | `AIFunctionNameAttribute`                        | binding attribute | function-name override  |
|  [09]   | `AIParameterNameAttribute`                       | binding attribute | parameter-name override |
|  [10]   | `HostedWebSearchTool`                            | hosted tool       | web search              |
|  [11]   | `HostedCodeInterpreterTool`                      | hosted tool       | code execution          |
|  [12]   | `HostedFileSearchTool`                           | hosted tool       | file search             |
|  [13]   | `HostedImageGenerationTool`                      | hosted tool       | image generation        |
|  [14]   | `HostedToolSearchTool`                           | hosted tool       | deferred-tool lookup    |
|  [15]   | `HostedMcpServerTool`                            | hosted tool       | remote MCP server       |
|  [16]   | `HostedMcpServerToolApprovalMode`                | approval base     | approval vocabulary     |
|  [17]   | `HostedMcpServerToolAlwaysRequireApprovalMode`   | approval value    | unconditional approval  |
|  [18]   | `HostedMcpServerToolNeverRequireApprovalMode`    | approval value    | approval bypass         |
|  [19]   | `HostedMcpServerToolRequireSpecificApprovalMode` | approval value    | per-tool-name approval  |

[FUNCTION_BINDING]: `AIFunctionFactoryOptions.SerializerOptions` `JsonSchemaCreateOptions` `Name` `Description` `AdditionalProperties` `ConfigureParameterBinding` `MarshalResult` `ExcludeResultSchema`
[FUNCTION_ARGUMENTS]: `AIFunctionArguments.Services` `AIFunctionArguments.Context`

[PUBLIC_TYPE_SCOPE]: multimodal and realtime contracts

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]     | [CAPABILITY]              |
| :-----: | :------------------------------- | :---------------- | :------------------------ |
|  [01]   | `IImageGenerator`                | client contract   | image generation and edit |
|  [02]   | `ImageGenerationRequest`         | request model     | prompt with source images |
|  [03]   | `ImageGenerationOptions`         | request options   | generation policy         |
|  [04]   | `ImageGenerationResponse`        | response model    | generated image contents  |
|  [05]   | `ImageGenerationResponseFormat`  | enum              | payload-shape vocabulary  |
|  [06]   | `ISpeechToTextClient`            | client contract   | speech transcription      |
|  [07]   | `SpeechToTextOptions`            | request options   | transcription policy      |
|  [08]   | `TranscriptionOptions`           | request options   | transcription refinement  |
|  [09]   | `SpeechToTextResponse`           | response model    | transcribed text          |
|  [10]   | `SpeechToTextResponseUpdate`     | streaming update  | incremental transcript    |
|  [11]   | `SpeechToTextResponseUpdateKind` | struct            | transcript-event kinds    |
|  [12]   | `ITextToSpeechClient`            | client contract   | speech synthesis          |
|  [13]   | `TextToSpeechOptions`            | request options   | synthesis policy          |
|  [14]   | `TextToSpeechResponse`           | response model    | synthesized audio         |
|  [15]   | `TextToSpeechResponseUpdate`     | streaming update  | incremental audio         |
|  [16]   | `TextToSpeechResponseUpdateKind` | struct            | audio-event kinds         |
|  [17]   | `IHostedFileClient`              | client contract   | hosted-file lifecycle     |
|  [18]   | `HostedFileClientOptions`        | request options   | per-call file policy      |
|  [19]   | `HostedFileDownloadStream`       | stream base       | streamed file payload     |
|  [20]   | `IRealtimeClient`                | client contract   | session creation          |
|  [21]   | `IRealtimeClientSession`         | session contract  | bidirectional session     |
|  [22]   | `RealtimeSessionOptions`         | session options   | session policy            |
|  [23]   | `RealtimeSessionKind`            | struct            | session-mode vocabulary   |
|  [24]   | `RealtimeAudioFormat`            | format model      | session audio encoding    |
|  [25]   | `VoiceActivityDetectionOptions`  | detection options | turn-detection policy     |
|  [26]   | `RealtimeConversationItem`       | conversation item | session history unit      |
|  [27]   | `RealtimeResponseStatus`         | status vocabulary | session response state    |
|  [28]   | `RealtimeClientMessage`          | message base      | client-sent event         |
|  [29]   | `RealtimeServerMessage`          | message base      | server-emitted event      |
|  [30]   | `RealtimeServerMessageType`      | struct            | server-event vocabulary   |

[REALTIME_CLIENT_MESSAGES]: `SessionUpdateRealtimeClientMessage` `InputAudioBufferAppendRealtimeClientMessage` `InputAudioBufferCommitRealtimeClientMessage` `CreateConversationItemRealtimeClientMessage` `CreateResponseRealtimeClientMessage`
[REALTIME_SERVER_MESSAGES]: `ErrorRealtimeServerMessage` `InputAudioTranscriptionRealtimeServerMessage` `OutputTextAudioRealtimeServerMessage` `ResponseCreatedRealtimeServerMessage` `ResponseOutputItemRealtimeServerMessage`
[DELEGATING_BASES]: `DelegatingChatClient` `DelegatingEmbeddingGenerator<TInput,TEmbedding>` `DelegatingImageGenerator` `DelegatingSpeechToTextClient` `DelegatingTextToSpeechClient` `DelegatingHostedFileClient` `DelegatingRealtimeClient` `DelegatingAIFunctionDeclaration` `DelegatingAIFunction`
[CLIENT_METADATA]: `ChatClientMetadata` `EmbeddingGeneratorMetadata` `ImageGeneratorMetadata` `SpeechToTextClientMetadata` `TextToSpeechClientMetadata` `HostedFileClientMetadata`
[CLIENT_EXTENSIONS]: `ImageGeneratorExtensions` `SpeechToTextClientExtensions` `SpeechToTextResponseUpdateExtensions` `TextToSpeechClientExtensions` `TextToSpeechResponseUpdateExtensions` `HostedFileClientExtensions`

[PUBLIC_TYPE_SCOPE]: content parts and annotations

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]     | [CAPABILITY]             |
| :-----: | :---------------------------- | :---------------- | :----------------------- |
|  [01]   | `AIContent`                   | content base      | discriminated part       |
|  [02]   | `TextContent`                 | text content      | message text             |
|  [03]   | `TextReasoningContent`        | reasoning content | model reasoning text     |
|  [04]   | `DataContent`                 | binary content    | inline bytes or data URI |
|  [05]   | `UriContent`                  | URI content       | external reference       |
|  [06]   | `HostedFileContent`           | reference content | hosted-file handle       |
|  [07]   | `HostedVectorStoreContent`    | reference content | vector-store handle      |
|  [08]   | `ErrorContent`                | signal content    | structured failure       |
|  [09]   | `UsageContent`                | signal content    | mid-stream token use     |
|  [10]   | `InputRequestContent`         | interaction part  | model-side input request |
|  [11]   | `InputResponseContent`        | interaction part  | caller-side input reply  |
|  [12]   | `ToolApprovalRequestContent`  | approval part     | approval prompt          |
|  [13]   | `ToolApprovalResponseContent` | approval part     | approval decision        |
|  [14]   | `AIAnnotation`                | annotation base   | grounded-source carrier  |
|  [15]   | `AnnotatedRegion`             | region base       | annotated span root      |
|  [16]   | `TextSpanAnnotatedRegion`     | region value      | annotated text span      |
|  [17]   | `CitationAnnotation`          | annotation        | source citation          |

[LOCAL_TOOL_PARTS]: `FunctionCallContent` `FunctionResultContent`
[HOSTED_TOOL_PARTS]: `ToolCallContent` `ToolResultContent` `McpServerToolCallContent` `McpServerToolResultContent` `CodeInterpreterToolCallContent` `CodeInterpreterToolResultContent` `WebSearchToolCallContent` `WebSearchToolResultContent` `ImageGenerationToolCallContent` `ImageGenerationToolResultContent`

[PUBLIC_TYPE_SCOPE]: JSON-schema utilities

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]               |
| :-----: | :----------------------------- | :--------------- | :------------------------- |
|  [01]   | `AIJsonUtilities`              | schema utility   | schema and hash operations |
|  [02]   | `AIJsonSchemaCreateOptions`    | schema options   | inference policy           |
|  [03]   | `AIJsonSchemaCreateContext`    | struct           | per-node inference context |
|  [04]   | `AIJsonSchemaTransformOptions` | transform policy | rewrite policy             |
|  [05]   | `AIJsonSchemaTransformContext` | struct           | per-node rewrite context   |
|  [06]   | `AIJsonSchemaTransformCache`   | transform cache  | rewrite caching            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chat calls, convenience overloads, and streaming accumulation; every `AddMessages` overload appends into the caller's `IList<ChatMessage>`

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `IChatClient.GetResponseAsync(IEnumerable<ChatMessage>, ChatOptions)`              | instance | full response            |
|  [02]   | `IChatClient.GetStreamingResponseAsync(IEnumerable<ChatMessage>, ChatOptions)`     | instance | update stream            |
|  [03]   | `IChatClient.GetService(Type, object)`                                             | instance | inner-service traversal  |
|  [04]   | `ChatClientExtensions.GetResponseAsync(string, ChatOptions)`                       | static   | single-prompt response   |
|  [05]   | `ChatClientExtensions.GetResponseAsync(ChatMessage, ChatOptions)`                  | static   | single-message response  |
|  [06]   | `ChatClientExtensions.GetStreamingResponseAsync(string, ChatOptions)`              | static   | single-prompt stream     |
|  [07]   | `ChatClientExtensions.GetStreamingResponseAsync(ChatMessage, ChatOptions)`         | static   | single-message stream    |
|  [08]   | `ChatClientExtensions.GetService<TService>(object)`                                | static   | optional service         |
|  [09]   | `ChatClientExtensions.GetRequiredService(Type, object)`                            | static   | required service         |
|  [10]   | `ChatClientExtensions.GetRequiredService<TService>(object)`                        | static   | required typed service   |
|  [11]   | `IChatReducer.ReduceAsync(IEnumerable<ChatMessage>)`                               | instance | bounded history          |
|  [12]   | `ChatResponseExtensions.ToChatResponse(IEnumerable<ChatResponseUpdate>)`           | static   | folded response          |
|  [13]   | `ChatResponseExtensions.ToChatResponseAsync(IAsyncEnumerable<ChatResponseUpdate>)` | static   | folded stream            |
|  [14]   | `ChatResponseExtensions.AddMessages(ChatResponse)`                                 | static   | history append           |
|  [15]   | `ChatResponseExtensions.AddMessages(IEnumerable<ChatResponseUpdate>)`              | static   | history append           |
|  [16]   | `ChatResponseExtensions.AddMessages(ChatResponseUpdate, Func<AIContent,bool>)`     | static   | filtered append          |
|  [17]   | `ChatResponseExtensions.AddMessagesAsync(IAsyncEnumerable<ChatResponseUpdate>)`    | static   | streamed append          |
|  [18]   | `ChatResponse.ToChatResponseUpdates()`                                             | instance | response re-projection   |
|  [19]   | `ChatResponse(ChatMessage)`                                                        | ctor     | single-message response  |
|  [20]   | `ChatResponse(IList<ChatMessage>)`                                                 | ctor     | multi-message response   |
|  [21]   | `ChatMessage(ChatRole, string)`                                                    | ctor     | text message             |
|  [22]   | `ChatMessage(ChatRole, IList<AIContent>)`                                          | ctor     | multimodal message       |
|  [23]   | `ChatMessage.Clone()`                                                              | instance | independent message      |
|  [24]   | `ChatOptions.Clone()`                                                              | instance | independent options      |
|  [25]   | `ChatResponseFormat.ForJsonSchema(JsonElement, string, string)`                    | factory  | schema-bound JSON format |

[ENTRYPOINT_SCOPE]: `ChatOptions` request properties, each independently set per call

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `ChatOptions.Temperature -> float?`                                   | property | sampling temperature  |
|  [02]   | `ChatOptions.TopP -> float?`                                          | property | nucleus sampling      |
|  [03]   | `ChatOptions.TopK -> int?`                                            | property | candidate sampling    |
|  [04]   | `ChatOptions.MaxOutputTokens -> int?`                                 | property | output cap            |
|  [05]   | `ChatOptions.Seed -> long?`                                           | property | reproducibility       |
|  [06]   | `ChatOptions.FrequencyPenalty -> float?`                              | property | frequency penalty     |
|  [07]   | `ChatOptions.PresencePenalty -> float?`                               | property | presence penalty      |
|  [08]   | `ChatOptions.StopSequences -> IList<string>?`                         | property | stopping vocabulary   |
|  [09]   | `ChatOptions.ResponseFormat -> ChatResponseFormat?`                   | property | response shape        |
|  [10]   | `ChatOptions.Tools -> IList<AITool>?`                                 | property | available tools       |
|  [11]   | `ChatOptions.ToolMode -> ChatToolMode?`                               | property | invocation policy     |
|  [12]   | `ChatOptions.AllowMultipleToolCalls -> bool?`                         | property | parallel tool calls   |
|  [13]   | `ChatOptions.Reasoning -> ReasoningOptions?`                          | property | reasoning budget      |
|  [14]   | `ChatOptions.ModelId -> string?`                                      | property | model override        |
|  [15]   | `ChatOptions.Instructions -> string?`                                 | property | system instruction    |
|  [16]   | `ChatOptions.ConversationId -> string?`                               | property | stateful conversation |
|  [17]   | `ChatOptions.AllowBackgroundResponses -> bool?`                       | property | background responses  |
|  [18]   | `ChatOptions.ContinuationToken -> ResponseContinuationToken?`         | property | response resume       |
|  [19]   | `ChatOptions.RawRepresentationFactory -> Func<IChatClient,object?>?`  | property | provider request hook |
|  [20]   | `ChatOptions.AdditionalProperties -> AdditionalPropertiesDictionary?` | property | provider properties   |

[ENTRYPOINT_SCOPE]: embedding generation and scalar projections; every generation call carries an optional `EmbeddingGenerationOptions`

| [INDEX] | [SURFACE]                                                                              | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `IEmbeddingGenerator<TInput,TEmbedding>.GenerateAsync(IEnumerable<TInput>)`            | instance | batch generation      |
|  [02]   | `IEmbeddingGenerator.GetService(Type, object)`                                         | instance | inner service         |
|  [03]   | `EmbeddingGeneratorExtensions.GenerateAsync(TInput) -> TEmbedding`                     | static   | one embedding         |
|  [04]   | `EmbeddingGeneratorExtensions.GenerateVectorAsync(TInput) -> ReadOnlyMemory<TElement>` | static   | raw vector            |
|  [05]   | `EmbeddingGeneratorExtensions.GenerateAndZipAsync(IEnumerable<TInput>)`                | static   | value-embedding pairs |
|  [06]   | `EmbeddingGeneratorExtensions.GetService<TService>(object)`                            | static   | optional service      |
|  [07]   | `EmbeddingGeneratorExtensions.GetRequiredService<TService>(object)`                    | static   | required service      |
|  [08]   | `EmbeddingGenerationOptions.Clone()`                                                   | instance | independent options   |

[ENTRYPOINT_SCOPE]: function construction, invocation, and MCP approval

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :------------------------ |
|  [01]   | `AIFunctionFactory.Create(Delegate, string, string, JsonSerializerOptions)`           | factory  | delegate-bound tool       |
|  [02]   | `AIFunctionFactory.Create(Delegate, AIFunctionFactoryOptions)`                        | factory  | policy-bound tool         |
|  [03]   | `AIFunctionFactory.Create(MethodInfo, object, AIFunctionFactoryOptions)`              | factory  | target-bound tool         |
|  [04]   | `AIFunctionFactory.Create(MethodInfo, object, string, string, JsonSerializerOptions)` | factory  | named target-bound tool   |
|  [05]   | `AIFunctionFactory.Create(MethodInfo, Func<AIFunctionArguments,object>)`              | factory  | per-call instance binding |
|  [06]   | `AIFunctionFactory.CreateDeclaration(string, string, JsonElement, JsonElement?)`      | factory  | schema-only manifest      |
|  [07]   | `AIFunction.InvokeAsync(AIFunctionArguments) -> ValueTask<object?>`                   | instance | local invocation          |
|  [08]   | `AIFunction.AsDeclarationOnly() -> AIFunctionDeclaration`                             | instance | manifest projection       |
|  [09]   | `AIFunction.UnderlyingMethod`                                                         | property | backing `MethodInfo`      |
|  [10]   | `AIFunction.JsonSerializerOptions`                                                    | property | serializer policy         |
|  [11]   | `AIFunctionDeclaration.JsonSchema`                                                    | property | parameter schema          |
|  [12]   | `AIFunctionDeclaration.ReturnJsonSchema`                                              | property | return schema             |
|  [13]   | `HostedMcpServerToolApprovalMode.AlwaysRequire`                                       | property | unconditional approval    |
|  [14]   | `HostedMcpServerToolApprovalMode.NeverRequire`                                        | property | approval bypass           |
|  [15]   | `HostedMcpServerToolApprovalMode.RequireSpecific(IList<string>, IList<string>)`       | factory  | per-tool-name approval    |

[ENTRYPOINT_SCOPE]: multimodal, hosted-file, and realtime calls, each carrying its family's optional options value — `ImageGenerationOptions`, `SpeechToTextOptions`, `TextToSpeechOptions`, `HostedFileClientOptions`, `RealtimeSessionOptions`

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `IImageGenerator.GenerateAsync(ImageGenerationRequest)`                    | instance | image generation        |
|  [02]   | `ImageGeneratorExtensions.GenerateImagesAsync(string)`                     | static   | prompt-only generation  |
|  [03]   | `ImageGeneratorExtensions.EditImageAsync(DataContent, string)`             | static   | single-image edit       |
|  [04]   | `ImageGeneratorExtensions.EditImagesAsync(IEnumerable<AIContent>, string)` | static   | multi-image edit        |
|  [05]   | `ISpeechToTextClient.GetTextAsync(Stream)`                                 | instance | transcription           |
|  [06]   | `ISpeechToTextClient.GetStreamingTextAsync(Stream)`                        | instance | streaming transcription |
|  [07]   | `SpeechToTextClientExtensions.GetTextAsync(DataContent)`                   | static   | content transcription   |
|  [08]   | `SpeechToTextResponseUpdateExtensions.ToSpeechToTextResponseAsync()`       | static   | folded transcript       |
|  [09]   | `ITextToSpeechClient.GetAudioAsync(string)`                                | instance | synthesis               |
|  [10]   | `ITextToSpeechClient.GetStreamingAudioAsync(string)`                       | instance | streaming synthesis     |
|  [11]   | `TextToSpeechResponseUpdateExtensions.ToTextToSpeechResponseAsync()`       | static   | folded audio            |
|  [12]   | `IHostedFileClient.UploadAsync(Stream, string, string)`                    | instance | file upload             |
|  [13]   | `IHostedFileClient.DownloadAsync(string)`                                  | instance | file download           |
|  [14]   | `IHostedFileClient.GetFileInfoAsync(string)`                               | instance | file metadata           |
|  [15]   | `IHostedFileClient.ListFilesAsync()`                                       | instance | file enumeration        |
|  [16]   | `IHostedFileClient.DeleteAsync(string)`                                    | instance | file removal            |
|  [17]   | `HostedFileClientExtensions.UploadAsync(DataContent)`                      | static   | content upload          |
|  [18]   | `HostedFileClientExtensions.DownloadAsDataContentAsync(string)`            | static   | inline download         |
|  [19]   | `HostedFileClientExtensions.DownloadToAsync(string, string)`               | static   | download to path        |
|  [20]   | `IRealtimeClient.CreateSessionAsync()`                                     | instance | session creation        |
|  [21]   | `IRealtimeClientSession.SendAsync(RealtimeClientMessage)`                  | instance | client event send       |
|  [22]   | `IRealtimeClientSession.GetStreamingResponseAsync()`                       | instance | server event stream     |

[ENTRYPOINT_SCOPE]: schema minting and content construction; both schema factories carry optional description, `JsonSerializerOptions`, and `AIJsonSchemaCreateOptions` tails

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `AIJsonUtilities.CreateJsonSchema(Type)`                                         | static   | type schema                |
|  [02]   | `AIJsonUtilities.CreateFunctionJsonSchema(MethodBase)`                           | static   | method schema              |
|  [03]   | `AIJsonUtilities.TransformSchema(JsonElement, AIJsonSchemaTransformOptions)`     | static   | rewritten schema           |
|  [04]   | `AIJsonUtilities.HashDataToString(ReadOnlySpan<object?>, JsonSerializerOptions)` | static   | stable cache key           |
|  [05]   | `AIJsonUtilities.AddAIContentType<TContent>(JsonSerializerOptions, string)`      | static   | content-type discriminator |
|  [06]   | `AIJsonUtilities.DefaultOptions`                                                 | property | serializer defaults        |
|  [07]   | `DataContent(string, string)`                                                    | ctor     | data-URI content           |
|  [08]   | `DataContent(ReadOnlyMemory<byte>, string)`                                      | ctor     | inline byte content        |
|  [09]   | `DataContent(Uri, string)`                                                       | ctor     | URI-backed content         |
|  [10]   | `DataContent.LoadFromAsync(Stream, string) -> ValueTask<DataContent>`            | static   | stream ingestion           |
|  [11]   | `DataContent.LoadFromAsync(string, string) -> ValueTask<DataContent>`            | static   | file ingestion             |
|  [12]   | `DataContent.SaveToAsync(string) -> ValueTask<string>`                           | instance | file emission              |
|  [13]   | `DataContent.HasTopLevelMediaType(string)`                                       | instance | media-type predicate       |
|  [14]   | `DataContent.Base64Data -> ReadOnlyMemory<char>`                                 | property | base64 payload view        |

- `ChatResponseExtensions.AddMessages`: mutates the supplied list in place and returns nothing.
- `IHostedFileClient.DeleteAsync`: returns `false` for an absent file rather than faulting.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every client contract extends `IDisposable`, pairs a `Task` call with an `IAsyncEnumerable` streaming twin, and resolves `GetService(Type, object?)` by walking its own delegating chain, so metadata and the raw provider object surface through one traversal.
- A delegating base wraps one inner client and forwards every member, so middleware composes by nesting and adds no interface variation.
- Client members are thread-safe, and an implementation may mutate the options instance handed to a call, so one options object never crosses concurrent requests.
- `AIContent` discriminates every message and response part; a tool round trip pairs call and result on one identity, `FunctionCallContent.CallId` matching `FunctionResultContent.CallId`, and each hosted family repeats that shape.
- `AIFunction` extends `AIFunctionDeclaration` with `InvokeAsync`, so a bare declaration is a manifest a provider reads with no local body.
- `AIFunctionFactory` reflects the parameter schema off a delegate or `MethodInfo` and binds named JSON properties to typed parameters through `JsonSerializerOptions`, with `AIFunctionArguments.Services` threading an `IServiceProvider` into that binding.
- Middleware invokes a local `AIFunction` while the provider executes every hosted tool registered in `ChatOptions.Tools`, and `ApprovalRequiredAIFunction` interposes paired approval content on the local path.

[STACKING]:
- `Microsoft.Extensions.Caching.Hybrid`(`.api/api-hybrid-cache.md`): `AIJsonUtilities.HashDataToString` mints the request key and a folded `ChatResponse` lands through `HybridCache.GetOrCreateAsync` under an `IHybridCacheSerializer<ChatResponse>`; host chat-caching middleware binds the same `IDistributedCache` L2 the hybrid cache fronts, so one profile serves both lanes.
- `System.Numerics.Tensors`(`.api/api-tensors.md`): `EmbeddingGeneratorExtensions.GenerateVectorAsync` yields a `ReadOnlyMemory<float>` whose span feeds `TensorPrimitives.CosineSimilarity` uncopied, and `BinaryEmbedding.Vector` packs the same generation into a `BitArray` for bit-distance ranking.
- `System.Text.Json`(`.api/api-json-schema.md`): `AIJsonUtilities.CreateJsonSchema` and `CreateFunctionJsonSchema` project one `JsonSerializerOptions` contract into the `JsonElement` that `ChatResponseFormat.ForJsonSchema` and `AIFunctionDeclaration.JsonSchema` both bind, so wire, tool manifest, and structured-output schema cannot drift.
- `OpenTelemetry`(`.api/api-opentelemetry.md`): host chat middleware emits GenAI spans and `UsageDetails` token counts onto the `ActivitySource` and `Meter` names the provider builders admit.
- `Microsoft.Extensions.AI`(`Rasm.AppHost/.api/api-extensions-ai-middleware.md`): `ChatClientBuilder` and `EmbeddingGeneratorBuilder` decorate and build the injected contracts, holding the abstraction-to-concrete split at the DI seam.
- `ModelContextProtocol`(`Rasm.AppHost/.api/api-mcp.md`): `McpClientTool : AIFunction` surfaces each federated server tool as an `AIFunction` row in `ChatOptions.Tools`.
- `Rasm.AppHost`: composes `inner.AsBuilder().UseChatReducer().UseFunctionInvocation().UseDistributedCache().UseOpenTelemetry().UseLogging().Build()` and registers the result as the injected `IChatClient`, so every Compute consumer inherits reduction, tool invocation, caching, and telemetry unchanged at the call site.
- `Rasm.Compute`: folds streaming updates through `ChatResponseExtensions.ToChatResponseAsync`, grows the transcript with `AddMessages`, bounds it through `IChatReducer` before the next send, and backs local `IChatClient` and `IEmbeddingGenerator` with ONNX Runtime behind the shared contracts.

[LOCAL_ADMISSION]:
- `Rasm.Compute` references the abstractions assembly alone and consumes DI-injected contracts; `Rasm.AppHost` references `Microsoft.Extensions.AI` and owns every builder, middleware row, and typed structured-output projection (`ChatClientStructuredOutputExtensions.GetResponseAsync<T> -> ChatResponse<T>`).
- `AIFunction` values and hosted-tool rows enter a request through `ChatOptions.Tools`; a model owner never constructs a provider client.
- `ChatOptions.Clone()`, `ChatMessage.Clone()`, and `EmbeddingGenerationOptions.Clone()` mint the per-request copy a caller mutates.
- `AdditionalPropertiesDictionary` carries provider-specific keys outside the typed options, response, and content surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: provider-neutral chat, embedding, image, speech, hosted-file, and realtime contracts with the content algebra, tool algebra, and AI JSON-schema utilities
- Accept: DI-injected client contracts, `AIFunctionFactory`-minted tools, hosted-tool rows, and `AIJsonUtilities` schemas
- Reject: a provider SDK type, a hand-rolled HTTP call, or a model-specific option class inside a Compute owner
