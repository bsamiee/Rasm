# [RASM_COMPUTE_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` supplies provider-agnostic contracts for chat,
embedding generation, image generation, speech-to-text, text-to-speech, realtime sessions,
tool definition and invocation, content part modeling, and JSON schema utilities that Compute
model owners consume through `IChatClient`, `IEmbeddingGenerator`, and `AIFunction` without
coupling to a specific AI provider.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- asset: runtime library
- rail: model-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat client contracts
- rail: model-client

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                |
| :-----: | :--------------------- | :--------------- | :------------------------------------ |
|  [01]   | `IChatClient`          | client contract  | provider-agnostic chat interface      |
|  [02]   | `DelegatingChatClient` | delegating base  | middleware composition base           |
|  [03]   | `ChatMessage`          | message model    | input/output message carrier          |
|  [04]   | `ChatRole`             | role value       | `System`, `User`, `Assistant`, `Tool` |
|  [05]   | `ChatOptions`          | request options  | temperature, tools, format, seed      |
|  [06]   | `ChatResponse`         | response model   | completed chat response carrier       |
|  [07]   | `ChatResponseUpdate`   | streaming update | incremental streaming response unit   |
|  [08]   | `ChatFinishReason`     | finish reason    | stop condition vocabulary             |
|  [09]   | `ChatToolMode`         | tool mode        | auto, none, or required               |
|  [10]   | `ChatResponseFormat`   | response format  | text or JSON schema constraint        |
|  [11]   | `ChatClientMetadata`   | client metadata  | provider name and model identity      |
|  [12]   | `UsageDetails`         | usage model      | input/output/total token counts       |

[PUBLIC_TYPE_SCOPE]: embedding generator contracts
- rail: model-client

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                             |
| :-----: | :----------------------------- | :----------------- | :--------------------------------- |
|  [01]   | `IEmbeddingGenerator`          | non-generic base   | non-generic service discovery root |
|  [02]   | `DelegatingEmbeddingGenerator` | delegating base    | middleware composition base        |
|  [03]   | `Embedding<TEmbedding>`        | embedding carrier  | typed embedding payload            |
|  [04]   | `GeneratedEmbeddings<T>`       | batch result       | embedding batch response           |
|  [05]   | `EmbeddingGenerationOptions`   | request options    | model and additional properties    |
|  [06]   | `EmbeddingGeneratorMetadata`   | generator metadata | provider name and model identity   |
|  [07]   | `BinaryEmbedding`              | binary embedding   | bit-packed embedding vector        |

[PUBLIC_TYPE_SCOPE]: tool and function contracts
- rail: model-client

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [RAIL]                                   |
| :-----: | :--------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `AITool`                     | tool base        | tool description base class              |
|  [02]   | `AIFunction`                 | function tool    | invocable tool with schema               |
|  [03]   | `AIFunctionDeclaration`      | declaration base | non-invocable tool description           |
|  [04]   | `AIFunctionFactory`          | function factory | creates `AIFunction` from delegates      |
|  [05]   | `AIFunctionArguments`        | argument carrier | typed argument dictionary                |
|  [06]   | `AIFunctionFactoryOptions`   | factory options  | JSON schema and binding policy           |
|  [07]   | `ApprovalRequiredAIFunction` | approval gate    | requires explicit approval before invoke |

[PUBLIC_TYPE_SCOPE]: content and annotation contracts
- rail: model-client

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [RAIL]                             |
| :-----: | :---------------------- | :------------------ | :--------------------------------- |
|  [01]   | `AIContent`             | content base        | discriminated content part base    |
|  [02]   | `TextContent`           | text content        | plain text message part            |
|  [03]   | `DataContent`           | binary content      | base64-encoded or stream data part |
|  [04]   | `UriContent`            | URI content         | URI-referenced content part        |
|  [05]   | `TextReasoningContent`  | reasoning content   | model chain-of-thought text        |
|  [06]   | `FunctionCallContent`   | call request        | tool invocation request part       |
|  [07]   | `FunctionResultContent` | call result         | tool invocation result part        |
|  [08]   | `ErrorContent`          | error content       | structured error from model        |
|  [09]   | `UsageContent`          | usage content       | usage statistics content part      |
|  [10]   | `AIAnnotation`          | annotation base     | structured annotation on content   |
|  [11]   | `CitationAnnotation`    | citation annotation | source citation annotation         |

[PUBLIC_TYPE_SCOPE]: JSON schema utilities
- rail: model-client

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :-------------------------- | :------------- | :--------------------------------- |
|  [01]   | `AIJsonUtilities`           | schema utility | JSON schema generation and hashing |
|  [02]   | `AIJsonSchemaCreateOptions` | schema options | schema generation policy           |
|  [03]   | `AIJsonSchemaCreateContext` | schema context | per-type schema creation context   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chat client operations
- rail: model-client

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `IChatClient.GetResponseAsync(messages, options, ct)`   | request call   | returns `Task<ChatResponse>`                   |
|  [02]   | `IChatClient.GetStreamingResponseAsync(msgs, opts, ct)` | streaming call | returns `IAsyncEnumerable<ChatResponseUpdate>` |
|  [03]   | `IChatClient.GetService(serviceType, serviceKey?)`      | service call   | inner service discovery                        |
|  [04]   | `ChatClientExtensions.GetResponseAsync<T>` family       | extension call | typed structured-output overloads              |
|  [05]   | `ChatOptions.Clone()`                                   | copy call      | shallow clone of request options               |

[ENTRYPOINT_SCOPE]: embedding generator operations
- rail: model-client

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `IEmbeddingGenerator<TInput,TEmbedding>.GenerateAsync(values, options?, ct)` | generate       | embedding batch generation      |
|  [02]   | `IEmbeddingGenerator.GetService(serviceType, serviceKey?)`                   | service call   | inner service discovery         |
|  [03]   | `EmbeddingGeneratorExtensions` family                                        | extension call | scalar and enumerable overloads |

[ENTRYPOINT_SCOPE]: tool and function operations
- rail: model-client

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]  | [RAIL]                            |
| :-----: | :-------------------------------------------------------- | :-------------- | :-------------------------------- |
|  [01]   | `AIFunctionFactory.Create(Delegate, options?)`            | factory call    | creates function from delegate    |
|  [02]   | `AIFunctionFactory.Create(MethodInfo, target?, options?)` | factory call    | creates function from reflection  |
|  [03]   | `AIFunction.InvokeAsync(arguments?, ct)`                  | invoke call     | invokes the AI function           |
|  [04]   | `AIFunction.AsDeclarationOnly()`                          | projection call | returns non-invocable declaration |
|  [05]   | `AIFunction.UnderlyingMethod`                             | property        | backing `MethodInfo` or null      |
|  [06]   | `AIFunction.JsonSerializerOptions`                        | property        | serializer options for arguments  |

[ENTRYPOINT_SCOPE]: ChatOptions properties
- rail: model-client

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------- | :------------- | :----------------------------------- |
|  [01]   | `Temperature`              | option         | `float?` sampling temperature        |
|  [02]   | `MaxOutputTokens`          | option         | `int?` max tokens in response        |
|  [03]   | `TopP`                     | option         | `float?` nucleus sampling factor     |
|  [04]   | `TopK`                     | option         | `int?` top-K token count             |
|  [05]   | `FrequencyPenalty`         | option         | `float?` repetition penalty          |
|  [06]   | `PresencePenalty`          | option         | `float?` presence penalty            |
|  [07]   | `Seed`                     | option         | `long?` reproducibility seed         |
|  [08]   | `ResponseFormat`           | option         | `ChatResponseFormat?` output shape   |
|  [09]   | `Tools`                    | option         | `IList<AITool>?` tool list           |
|  [10]   | `ToolMode`                 | option         | `ChatToolMode?` tool invocation mode |
|  [11]   | `ModelId`                  | option         | `string?` model override             |
|  [12]   | `Reasoning`                | option         | `ReasoningOptions?` reasoning config |
|  [13]   | `AllowMultipleToolCalls`   | option         | `bool?` parallel tool calls gate     |
|  [14]   | `ConversationId`           | option         | `string?` stateful conversation ID   |
|  [15]   | `AllowBackgroundResponses` | option         | `bool?` background streaming gate    |

[ENTRYPOINT_SCOPE]: JSON schema and utility operations
- rail: model-client

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `AIJsonUtilities.CreateJsonSchema(type, options?)` | schema call    | generates JSON schema for a type |
|  [02]   | `AIJsonUtilities.DefaultOptions`                   | property       | shared `JsonSerializerOptions`   |
|  [03]   | `DataContent(string dataUri, mediaType?)`          | ctor call      | ingests a data-URI content part  |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- namespace: `Microsoft.Extensions.AI`
- chat contract: `IChatClient : IDisposable` with `GetResponseAsync` and `GetStreamingResponseAsync`
- embedding contract: `IEmbeddingGenerator : IDisposable` (non-generic); typed surface is `IEmbeddingGenerator<TInput,TEmbedding>`
- middleware: `DelegatingChatClient` and `DelegatingEmbeddingGenerator` wrap an inner client for pipeline composition
- service discovery: `GetService(Type, object?)` on each contract locates inner services such as metadata objects
- thread safety: all members of `IChatClient` and `IEmbeddingGenerator` are thread-safe for concurrent use

[FUNCTION_SURFACE]:
- `AIFunction` derives from `AIFunctionDeclaration` and adds `InvokeAsync`
- `AIFunctionFactory.Create` accepts a delegate or `MethodInfo` and derives a JSON schema from parameters via reflection
- `AIFunctionArguments` is a keyed dictionary; factory binding maps named JSON properties to typed parameters
- `AsDeclarationOnly()` returns a non-invocable `AIFunctionDeclaration` for tool manifests sent to models

[CONTENT_TOPOLOGY]:
- content parts: `TextContent`, `DataContent`, `UriContent`, `TextReasoningContent`, `FunctionCallContent`, `FunctionResultContent`, `ErrorContent`, `UsageContent`
- `AIContent` is the abstract base; all parts flow through `ChatMessage.Contents : IList<AIContent>`
- `FunctionCallContent.CallId` pairs with `FunctionResultContent.CallId` for tool round-trips
- `ToolCallContent` / `ToolResultContent` are hosted-tool variants for provider-managed tool surfaces

[LOCAL_ADMISSION]:
- Compute consumes `IChatClient` and `IEmbeddingGenerator` injected from DI; no direct provider construction inside model owners.
- `AIFunction` surfaces registered in `ChatOptions.Tools` are declared by compute owners and invoked by the middleware pipeline.
- `ChatOptions.Clone()` is the canonical copy path before per-request mutation; shared option instances must never be mutated.
- `AdditionalPropertiesDictionary` on options and responses carries provider-specific keys outside the strongly typed surface.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: provider-agnostic AI client contracts and content model
- Accept: chat and embedding via injected clients; tool registration via `AIFunctionFactory`
- Reject: provider SDK types, direct HTTP calls, and model-specific option objects inside compute owners
