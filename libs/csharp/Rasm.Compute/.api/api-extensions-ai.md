# [RASM_COMPUTE_API_EXTENSIONS_AI]

`Microsoft.Extensions.AI.Abstractions` supplies provider-agnostic contracts for chat,
embedding generation, image generation, speech-to-text, text-to-speech, realtime sessions,
tool definition and invocation, content part modeling, and JSON schema utilities that Compute
model owners consume through `IChatClient`, `IEmbeddingGenerator`, and `AIFunction` without
coupling to a specific AI provider.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- asset: runtime library
- rail: model-client

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat client contracts
- rail: model-client

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                |
| :-----: | :--------------------- | :--------------- | :------------------------------------ |
|   [1]   | `IChatClient`          | client contract  | provider-agnostic chat interface      |
|   [2]   | `DelegatingChatClient` | delegating base  | middleware composition base           |
|   [3]   | `ChatMessage`          | message model    | input/output message carrier          |
|   [4]   | `ChatRole`             | role value       | `System`, `User`, `Assistant`, `Tool` |
|   [5]   | `ChatOptions`          | request options  | temperature, tools, format, seed      |
|   [6]   | `ChatResponse`         | response model   | completed chat response carrier       |
|   [7]   | `ChatResponseUpdate`   | streaming update | incremental streaming response unit   |
|   [8]   | `ChatFinishReason`     | finish reason    | stop condition vocabulary             |
|   [9]   | `ChatToolMode`         | tool mode        | auto, none, or required               |
|  [10]   | `ChatResponseFormat`   | response format  | text or JSON schema constraint        |
|  [11]   | `ChatClientMetadata`   | client metadata  | provider name and model identity      |
|  [12]   | `UsageDetails`         | usage model      | input/output/total token counts       |

[PUBLIC_TYPE_SCOPE]: embedding generator contracts
- rail: model-client

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                             |
| :-----: | :----------------------------- | :----------------- | :--------------------------------- |
|   [1]   | `IEmbeddingGenerator`          | non-generic base   | non-generic service discovery root |
|   [2]   | `DelegatingEmbeddingGenerator` | delegating base    | middleware composition base        |
|   [3]   | `Embedding<TEmbedding>`        | embedding carrier  | typed embedding payload            |
|   [4]   | `GeneratedEmbeddings<T>`       | batch result       | embedding batch response           |
|   [5]   | `EmbeddingGenerationOptions`   | request options    | model and additional properties    |
|   [6]   | `EmbeddingGeneratorMetadata`   | generator metadata | provider name and model identity   |
|   [7]   | `BinaryEmbedding`              | binary embedding   | bit-packed embedding vector        |

[PUBLIC_TYPE_SCOPE]: tool and function contracts
- rail: model-client

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [RAIL]                                   |
| :-----: | :--------------------------- | :--------------- | :--------------------------------------- |
|   [1]   | `AITool`                     | tool base        | tool description base class              |
|   [2]   | `AIFunction`                 | function tool    | invocable tool with schema               |
|   [3]   | `AIFunctionDeclaration`      | declaration base | non-invocable tool description           |
|   [4]   | `AIFunctionFactory`          | function factory | creates `AIFunction` from delegates      |
|   [5]   | `AIFunctionArguments`        | argument carrier | typed argument dictionary                |
|   [6]   | `AIFunctionFactoryOptions`   | factory options  | JSON schema and binding policy           |
|   [7]   | `ApprovalRequiredAIFunction` | approval gate    | requires explicit approval before invoke |

[PUBLIC_TYPE_SCOPE]: content and annotation contracts
- rail: model-client

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]       | [RAIL]                             |
| :-----: | :---------------------- | :------------------ | :--------------------------------- |
|   [1]   | `AIContent`             | content base        | discriminated content part base    |
|   [2]   | `TextContent`           | text content        | plain text message part            |
|   [3]   | `DataContent`           | binary content      | base64-encoded or stream data part |
|   [4]   | `UriContent`            | URI content         | URI-referenced content part        |
|   [5]   | `TextReasoningContent`  | reasoning content   | model chain-of-thought text        |
|   [6]   | `FunctionCallContent`   | call request        | tool invocation request part       |
|   [7]   | `FunctionResultContent` | call result         | tool invocation result part        |
|   [8]   | `ErrorContent`          | error content       | structured error from model        |
|   [9]   | `UsageContent`          | usage content       | usage statistics content part      |
|  [10]   | `AIAnnotation`          | annotation base     | structured annotation on content   |
|  [11]   | `CitationAnnotation`    | citation annotation | source citation annotation         |

[PUBLIC_TYPE_SCOPE]: JSON schema utilities
- rail: model-client

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :-------------------------- | :------------- | :--------------------------------- |
|   [1]   | `AIJsonUtilities`           | schema utility | JSON schema generation and hashing |
|   [2]   | `AIJsonSchemaCreateOptions` | schema options | schema generation policy           |
|   [3]   | `AIJsonSchemaCreateContext` | schema context | per-type schema creation context   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: chat client operations
- rail: model-client

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------ | :------------- | :--------------------------------------------- |
|   [1]   | `IChatClient.GetResponseAsync(messages, options, ct)`   | request call   | returns `Task<ChatResponse>`                   |
|   [2]   | `IChatClient.GetStreamingResponseAsync(msgs, opts, ct)` | streaming call | returns `IAsyncEnumerable<ChatResponseUpdate>` |
|   [3]   | `IChatClient.GetService(serviceType, serviceKey?)`      | service call   | inner service discovery                        |
|   [4]   | `ChatClientExtensions.GetResponseAsync<T>` family       | extension call | typed structured-output overloads              |
|   [5]   | `ChatOptions.Clone()`                                   | copy call      | shallow clone of request options               |

[ENTRYPOINT_SCOPE]: embedding generator operations
- rail: model-client

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `IEmbeddingGenerator<TInput,TEmbedding>.GenerateAsync(values, options?, ct)` | generate       | embedding batch generation      |
|   [2]   | `IEmbeddingGenerator.GetService(serviceType, serviceKey?)`                   | service call   | inner service discovery         |
|   [3]   | `EmbeddingGeneratorExtensions` family                                        | extension call | scalar and enumerable overloads |

[ENTRYPOINT_SCOPE]: tool and function operations
- rail: model-client

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]  | [RAIL]                            |
| :-----: | :-------------------------------------------------------- | :-------------- | :-------------------------------- |
|   [1]   | `AIFunctionFactory.Create(Delegate, options?)`            | factory call    | creates function from delegate    |
|   [2]   | `AIFunctionFactory.Create(MethodInfo, target?, options?)` | factory call    | creates function from reflection  |
|   [3]   | `AIFunction.InvokeAsync(arguments?, ct)`                  | invoke call     | invokes the AI function           |
|   [4]   | `AIFunction.AsDeclarationOnly()`                          | projection call | returns non-invocable declaration |
|   [5]   | `AIFunction.UnderlyingMethod`                             | property        | backing `MethodInfo` or null      |
|   [6]   | `AIFunction.JsonSerializerOptions`                        | property        | serializer options for arguments  |

[ENTRYPOINT_SCOPE]: ChatOptions properties
- rail: model-client

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Temperature`              | option         | `float?` sampling temperature        |
|   [2]   | `MaxOutputTokens`          | option         | `int?` max tokens in response        |
|   [3]   | `TopP`                     | option         | `float?` nucleus sampling factor     |
|   [4]   | `TopK`                     | option         | `int?` top-K token count             |
|   [5]   | `FrequencyPenalty`         | option         | `float?` repetition penalty          |
|   [6]   | `PresencePenalty`          | option         | `float?` presence penalty            |
|   [7]   | `Seed`                     | option         | `long?` reproducibility seed         |
|   [8]   | `ResponseFormat`           | option         | `ChatResponseFormat?` output shape   |
|   [9]   | `Tools`                    | option         | `IList<AITool>?` tool list           |
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
|   [1]   | `AIJsonUtilities.CreateJsonSchema(type, options?)` | schema call    | generates JSON schema for a type |
|   [2]   | `AIJsonUtilities.DefaultOptions`                   | property       | shared `JsonSerializerOptions`   |
|   [3]   | `DataContent(string dataUri, mediaType?)`          | ctor call      | ingests a data-URI content part  |

## [4]-[IMPLEMENTATION_LAW]

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
