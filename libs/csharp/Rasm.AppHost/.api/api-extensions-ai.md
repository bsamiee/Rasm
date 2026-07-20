# [RASM_APPHOST_API_EXTENSIONS_AI]

Full surface and stacking: `libs/csharp/.api/api-extensions-ai.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- namespace: `Microsoft.Extensions.AI`
- chat contract: `IChatClient : IDisposable` with `GetResponseAsync` and `GetStreamingResponseAsync`
- embedding contract: `IEmbeddingGenerator : IDisposable` (non-generic); typed surface is `IEmbeddingGenerator<TInput,TEmbedding>`
- modal contracts: `ISpeechToTextClient`, `ITextToSpeechClient`, `IImageGenerator`, `IRealtimeClient`, `IHostedFileClient` all carry `[Experimental("MEAI001")]`
- middleware: `DelegatingChatClient`, `DelegatingEmbeddingGenerator`, and modal delegating bases wrap an inner client for pipeline composition
- service discovery: `GetService(Type, object?)` on each contract locates inner services such as metadata objects
- thread safety: all members of primary contracts are thread-safe for concurrent use; implementations must not be disposed while in use

[FUNCTION_SURFACE]:
- `AITool` is the base; `AIFunctionDeclaration : AITool` adds `JsonSchema` and `ReturnJsonSchema`; `AIFunction : AIFunctionDeclaration` adds `InvokeAsync`
- `AIFunctionFactory.Create` accepts a delegate or `MethodInfo` and derives a JSON schema from parameters via reflection; `AIFunctionNameAttribute` and `AIParameterNameAttribute` on the agent owner's delegate override the reflected tool and parameter names
- `AIFunctionArguments` is a keyed `IDictionary<string,object?>` dictionary; factory binding maps named JSON properties to typed parameters
- `DelegatingAIFunction` wraps an `AIFunction` for per-tool middleware without re-authoring its schema
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
- Modal clients (`ISpeechToTextClient`, `ITextToSpeechClient`, `IImageGenerator`, `IRealtimeClient`, `IHostedFileClient`) require `[Experimental("MEAI001")]` suppression at call sites.
- `ReasoningOptions` is provider-transparent; implementations make best-effort mapping and may silently ignore unsupported fields.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: provider-agnostic AI client contracts, content model, and JSON schema utilities
- Accept: chat, embedding, and modal requests via injected clients; tool registration via `AIFunctionFactory`
- Reject: provider SDK types, direct HTTP calls, model-specific option objects, and hand-rolled JSON schema derivation inside capability-agent owners
