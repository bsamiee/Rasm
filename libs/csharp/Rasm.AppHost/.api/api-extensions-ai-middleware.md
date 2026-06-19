# [RASM_APPHOST_API_EXTENSIONS_AI_MIDDLEWARE]

`Microsoft.Extensions.AI` is the concrete middleware package over the `Microsoft.Extensions.AI.Abstractions` contracts: a fluent `ChatClientBuilder`/`EmbeddingGeneratorBuilder` composes provider-agnostic decorators — function-invocation, distributed response caching, OpenTelemetry GenAI instrumentation, logging, and option configuration — into one delegating pipeline the AppHost model-governance fold consumes so every model call is metered, content-cached, and traced without coupling to a provider. The abstractions surface (`IChatClient`, `IEmbeddingGenerator`, `AIFunction`, the modal clients) lives at `api-extensions-ai.md`; this catalogue carries only the concrete builder and decorator types the middleware composition reaches for.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI`
- package: `Microsoft.Extensions.AI`
- assembly: `Microsoft.Extensions.AI`
- namespace: `Microsoft.Extensions.AI`
- asset: runtime library
- rail: capability-agent

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat-client builder and decorators
- rail: capability-agent

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                                  |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------------- |
|   [1]   | `ChatClientBuilder`            | builder            | composes the `IChatClient` delegating pipeline         |
|   [2]   | `FunctionInvokingChatClient`   | delegating client  | runs `AIFunction` tool calls in the response loop      |
|   [3]   | `DistributedCachingChatClient` | delegating client  | content-keyed response cache over `IDistributedCache`  |
|   [4]   | `CachingChatClient`            | abstract base      | caching-decorator base of `DistributedCachingChatClient` |
|   [5]   | `OpenTelemetryChatClient`      | delegating client  | GenAI `gen_ai.*` span/metric instrumentation           |
|   [6]   | `LoggingChatClient`            | delegating client  | request/response structured logging                    |
|   [7]   | `ConfigureOptionsChatClient`   | delegating client  | mutates `ChatOptions` per request                      |

[PUBLIC_TYPE_SCOPE]: embedding-generator builder and decorators
- rail: capability-agent

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]     | [RAIL]                                              |
| :-----: | :--------------------------------------------- | :---------------- | :------------------------------------------------- |
|   [1]   | `EmbeddingGeneratorBuilder<TInput,TEmbedding>` | builder           | composes the `IEmbeddingGenerator` pipeline        |
|   [2]   | `DistributedCachingEmbeddingGenerator<TIn,TE>` | delegating gen.   | content-keyed embedding cache                      |
|   [3]   | `OpenTelemetryEmbeddingGenerator<TIn,TE>`      | delegating gen.   | embedding-call OTel instrumentation                |

[PUBLIC_TYPE_SCOPE]: builder extension surfaces
- rail: capability-agent

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [RAIL]                                  |
| :-----: | :----------------------------------------------- | :---------------- | :-------------------------------------- |
|   [1]   | `FunctionInvokingChatClientBuilderExtensions`    | extension methods | `UseFunctionInvocation`                 |
|   [2]   | `DistributedCachingChatClientBuilderExtensions`  | extension methods | `UseDistributedCache`                   |
|   [3]   | `OpenTelemetryChatClientBuilderExtensions`       | extension methods | `UseOpenTelemetry`                      |
|   [4]   | `LoggingChatClientBuilderExtensions`             | extension methods | `UseLogging`                            |
|   [5]   | `ConfigureOptionsChatClientBuilderExtensions`    | extension methods | `ConfigureOptions`                      |
|   [6]   | `ChatClientBuilderChatClientExtensions`          | extension methods | `AsBuilder`                             |
|   [7]   | `DistributedCachingEmbeddingGeneratorBuilderExtensions` | extension methods | `UseDistributedCache` (embeddings) |
|   [8]   | `OpenTelemetryEmbeddingGeneratorBuilderExtensions`      | extension methods | `UseOpenTelemetry` (embeddings)    |
|   [9]   | `EmbeddingGeneratorBuilderEmbeddingGeneratorExtensions` | extension methods | `AsBuilder` (embeddings)           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder composition and decorator wiring
- rail: capability-agent

| [INDEX] | [MEMBER]                                                          | [KIND]         | [RETURN]                          |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `IChatClient.AsBuilder()`                                        | extension      | `ChatClientBuilder`               |
|   [2]   | `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)`           | builder call   | `ChatClientBuilder`               |
|   [3]   | `ChatClientBuilder.Build(IServiceProvider?)`                     | builder call   | `IChatClient`                     |
|   [4]   | `ChatClientBuilder.UseFunctionInvocation(ILoggerFactory?, Action<FunctionInvokingChatClient>?)` | extension | `ChatClientBuilder` |
|   [5]   | `ChatClientBuilder.UseDistributedCache(IDistributedCache?, Action<DistributedCachingChatClient>?)` | extension | `ChatClientBuilder` |
|   [6]   | `ChatClientBuilder.UseOpenTelemetry(ILoggerFactory?, string?, Action<OpenTelemetryChatClient>?)` | extension | `ChatClientBuilder` |
|   [7]   | `ChatClientBuilder.UseLogging(ILoggerFactory?, Action<LoggingChatClient>?)` | extension   | `ChatClientBuilder`               |
|   [8]   | `ChatClientBuilder.ConfigureOptions(Action<ChatOptions>)`        | extension      | `ChatClientBuilder`               |
|   [9]   | `IEmbeddingGenerator<TIn,TE>.AsBuilder()`                        | extension      | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [10]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseDistributedCache(...)`     | extension      | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [11]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseOpenTelemetry(...)`        | extension      | `EmbeddingGeneratorBuilder<TIn,TE>` |

[ENTRYPOINT_SCOPE]: decorator-tuning properties
- rail: capability-agent

| [INDEX] | [MEMBER]                                                       | [KIND]    | [SEMANTICS]                                          |
| :-----: | :------------------------------------------------------------- | :-------- | :-------------------------------------------------- |
|   [1]   | `FunctionInvokingChatClient.MaximumIterationsPerRequest`      | property  | caps tool-call loop iterations                       |
|   [2]   | `FunctionInvokingChatClient.MaximumConsecutiveErrorsPerRequest` | property | caps consecutive tool-call errors                    |
|   [3]   | `FunctionInvokingChatClient.AllowConcurrentInvocation`       | property  | gates parallel tool execution                        |
|   [4]   | `FunctionInvokingChatClient.AdditionalTools`                 | property  | tools merged into every request                      |
|   [5]   | `FunctionInvokingChatClient.IncludeDetailedErrors`          | property  | surfaces tool-error detail to the model              |
|   [6]   | `FunctionInvokingChatClient.TerminateOnUnknownCalls`        | property  | halts on an unresolved tool name                     |
|   [7]   | `CachingChatClient.CacheKeyAdditionalValues`               | property  | extends the content-cache key                        |

## [4]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: middleware composition
- rail: capability-agent

- pipeline shape: every decorator is a `DelegatingChatClient` wrapping the inner `IChatClient`; `ChatClientBuilder.Use`/`Build` composes them outermost-last so `UseOpenTelemetry().UseDistributedCache().UseFunctionInvocation()` reads outermost-to-innermost in call order, the OTel span enclosing the cache lookup enclosing the function-invocation loop.
- caching: `DistributedCachingChatClient` keys the response by the serialized request (messages + options) over an injected `IDistributedCache`; an identical request-under-key replays the cached `ChatResponse` rather than re-calling the provider, and `CacheKeyAdditionalValues` folds extra discriminants into the key.
- token usage: a completed `ChatResponse` carries `ChatResponse.Usage` (`UsageDetails` with `InputTokenCount`/`OutputTokenCount`/`TotalTokenCount`) the governance fold projects onto a cost vector.
- instrumentation: `UseOpenTelemetry` emits the OTel GenAI `gen_ai.*` conventions; the source name argument names the `ActivitySource`/`Meter` the observability composition root registers.
- function invocation: `FunctionInvokingChatClient` runs the tool-call loop over `ChatOptions.Tools`, invoking each `AIFunction`; the governance fold supplies the brokered `CommandAIFunction` instances so every tool call routes through the command algebra.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: capability-agent

- the model-governance fold composes the four decorators once at the capability-agent composition edge: `client.AsBuilder().UseOpenTelemetry(...).UseDistributedCache(hybridCache-backed IDistributedCache).UseFunctionInvocation(...).Build(sp)`.
- the cache is the resources-lane `HybridCache` exposed as `IDistributedCache`, so the model response cache and the suite content cache share one store, never a second cache owner.
- `MaximumIterationsPerRequest` and `MaximumConsecutiveErrorsPerRequest` are policy values read off the agent options row, never literals; the function-invocation loop bound traces to the agent's deadline class.
- the concrete decorators are the one model-governance owner; a hand-rolled retry loop, a per-call OTel span, or a second response cache beside the decorators is the deleted form.
