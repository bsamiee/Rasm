# [RASM_APPHOST_API_EXTENSIONS_AI_MIDDLEWARE]

`Microsoft.Extensions.AI` mints the concrete middleware pipeline over the abstractions contracts: a `ChatClientBuilder`/`EmbeddingGeneratorBuilder` folds provider-agnostic decorators into one `DelegatingChatClient` chain the capability-agent governance fold composes once, so every model call is metered, content-cached, traced, and context-bounded without provider coupling. Abstractions live at `libs/csharp/.api/api-extensions-ai.md`; this catalog carries the concrete builder, decorator, reducer, and registration surface alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI`
- package: `Microsoft.Extensions.AI`
- assembly: `Microsoft.Extensions.AI`
- namespace: `Microsoft.Extensions.AI`, `Microsoft.Extensions.DependencyInjection`
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat-client builder, decorators, and reducers

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]     | [CAPABILITY]                              |
| :-----: | :------------------------------ | :---------------- | :---------------------------------------- |
|  [01]   | `ChatClientBuilder`             | builder           | folds decorators into the pipeline        |
|  [02]   | `FunctionInvokingChatClient`    | delegating client | the `AIFunction` tool-call loop           |
|  [03]   | `CachingChatClient`             | abstract base     | response-cache decorator base             |
|  [04]   | `DistributedCachingChatClient`  | delegating client | content-keyed `IDistributedCache` cache   |
|  [05]   | `OpenTelemetryChatClient`       | delegating client | `gen_ai.*` span and metric telemetry      |
|  [06]   | `LoggingChatClient`             | delegating client | structured request/response logs          |
|  [07]   | `ConfigureOptionsChatClient`    | delegating client | per-request `ChatOptions` mutation        |
|  [08]   | `ReducingChatClient`            | delegating client | `IChatReducer` history reduction          |
|  [09]   | `ImageGeneratingChatClient`     | delegating client | folds `IImageGenerator` into the pipeline |
|  [10]   | `AnonymousDelegatingChatClient` | delegating client | delegate-backed inline middleware         |
|  [11]   | `MessageCountingChatReducer`    | reducer           | truncate to a message count               |
|  [12]   | `SummarizingChatReducer`        | reducer           | summarize overflow through a model        |
|  [13]   | `ChatResponse<T>`               | typed response    | structured-output carrier                 |

- `CachingChatClient.GetCacheKey`/`EnableCaching`: a `protected abstract` key derivation and a `protected virtual` per-call cacheability gate a subclass overrides.
- `AnonymousDelegatingChatClient`: `internal sealed`, reachable only through the `ChatClientBuilder.Use(...)` delegate overloads.

[PUBLIC_TYPE_SCOPE]: embedding-generator builder and decorators

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]     | [CAPABILITY]                             |
| :-----: | :--------------------------------------------- | :---------------- | :--------------------------------------- |
|  [01]   | `EmbeddingGeneratorBuilder<TInput,TEmbedding>` | builder           | composes the embedding pipeline          |
|  [02]   | `CachingEmbeddingGenerator<TIn,TE>`            | abstract base     | embedding-cache decorator base           |
|  [03]   | `DistributedCachingEmbeddingGenerator<TIn,TE>` | delegating client | content-keyed embedding cache            |
|  [04]   | `OpenTelemetryEmbeddingGenerator<TIn,TE>`      | delegating client | embedding-call OTel instrumentation      |
|  [05]   | `LoggingEmbeddingGenerator<TIn,TE>`            | delegating client | structured embedding logs                |
|  [06]   | `ConfigureOptionsEmbeddingGenerator<TIn,TE>`   | delegating client | per-request `EmbeddingGenerationOptions` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder composition, DI registration, and typed reads

Every `Use*`/`ConfigureOptions`/`AsBuilder` returns its builder for chaining; a load-bearing non-builder return rides the signature.

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `IChatClient.AsBuilder()`                                                                | instance | open a builder over a client       |
|  [02]   | `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)`                                   | instance | weave a decorator factory          |
|  [03]   | `ChatClientBuilder.Use(Func<IChatClient,IServiceProvider,IChatClient>)`                  | instance | weave a DI-aware decorator         |
|  [04]   | `ChatClientBuilder.Use(sharedFunc)`                                                      | instance | inline pre/post middleware         |
|  [05]   | `ChatClientBuilder.Use(getResponseFunc, getStreamingResponseFunc)`                       | instance | inline per-verb middleware         |
|  [06]   | `ChatClientBuilder.UseFunctionInvocation(ILoggerFactory?, Action<...>?)`                 | instance | add the tool-call loop             |
|  [07]   | `ChatClientBuilder.UseDistributedCache(IDistributedCache?, Action<...>?)`                | instance | add the response cache             |
|  [08]   | `ChatClientBuilder.UseOpenTelemetry(ILoggerFactory?, string?, Action<...>?)`             | instance | add `gen_ai` telemetry             |
|  [09]   | `ChatClientBuilder.UseLogging(ILoggerFactory?, Action<...>?)`                            | instance | add request/response logs          |
|  [10]   | `ChatClientBuilder.ConfigureOptions(Action<ChatOptions>)`                                | instance | mutate per-request options         |
|  [11]   | `ChatClientBuilder.UseChatReducer(IChatReducer?, Action<...>?)`                          | instance | add history reduction              |
|  [12]   | `ChatClientBuilder.Build(IServiceProvider?) -> IChatClient`                              | instance | materialize the pipeline           |
|  [13]   | `IServiceCollection.AddChatClient(IChatClient, ServiceLifetime)`                         | static   | register the pipeline in DI        |
|  [14]   | `IServiceCollection.AddKeyedChatClient(object?, IChatClient, ServiceLifetime)`           | static   | keyed DI registration              |
|  [15]   | `IChatClient.GetResponseAsync<T>(...) -> Task<ChatResponse<T>>`                          | instance | typed structured-output round-trip |
|  [16]   | `ChatResponse<T>.Result -> T`                                                            | property | typed read, throws on parse miss   |
|  [17]   | `ChatResponse<T>.TryGetResult(out T?)`                                                   | instance | non-throwing typed read            |
|  [18]   | `IEmbeddingGenerator<TIn,TE>.AsBuilder()`                                                | instance | open an embedding builder          |
|  [19]   | `IServiceCollection.AddEmbeddingGenerator<TIn,TE>(IEmbeddingGenerator, ServiceLifetime)` | static   | register the embedding pipeline    |
|  [20]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseDistributedCache(...)`                             | instance | add the embedding cache            |
|  [21]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseOpenTelemetry(...)`                                | instance | add embedding telemetry            |
|  [22]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseLogging(...)`                                      | instance | add embedding logs                 |
|  [23]   | `EmbeddingGeneratorBuilder<TIn,TE>.ConfigureOptions(Action<EmbeddingGenerationOptions>)` | instance | mutate per-request options         |

- `ChatClientBuilder.Use(sharedFunc)`: `Func<IEnumerable<ChatMessage>,ChatOptions?,Func<…,Task>,CancellationToken,Task>` wrapped in the `internal sealed AnonymousDelegatingChatClient`; pre/post only, no `ChatResponse` handle.
- `GetResponseAsync<T>`: accepts `IEnumerable<ChatMessage>`/`string`/`ChatMessage` with optional `JsonSerializerOptions`; `useJsonSchemaResponseFormat: true` forces `ChatResponseFormat.ForJsonSchema`, and it is the sole typed surface with no streaming twin.

[ENTRYPOINT_SCOPE]: decorator-tuning properties

| [INDEX] | [SURFACE]                                                       | [SHAPE]         | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `FunctionInvokingChatClient.MaximumIterationsPerRequest`        | property        | caps tool-call loop iterations                  |
|  [02]   | `FunctionInvokingChatClient.MaximumConsecutiveErrorsPerRequest` | property        | caps consecutive tool-call errors               |
|  [03]   | `FunctionInvokingChatClient.AllowConcurrentInvocation`          | property        | gates parallel tool execution                   |
|  [04]   | `FunctionInvokingChatClient.AdditionalTools`                    | property        | tools merged into every request                 |
|  [05]   | `FunctionInvokingChatClient.IncludeDetailedErrors`              | property        | surfaces tool-error detail to the model         |
|  [06]   | `FunctionInvokingChatClient.TerminateOnUnknownCalls`            | property        | halts on an unresolved tool name                |
|  [07]   | `FunctionInvokingChatClient.FunctionInvoker`                    | property        | `Func` hook intercepting each `AIFunction` call |
|  [08]   | `FunctionInvokingChatClient.CurrentContext`                     | static property | ambient `FunctionInvocationContext` in flight   |
|  [09]   | `CachingChatClient.CoalesceStreamingUpdates`                    | property        | merges streamed updates before caching          |
|  [10]   | `DistributedCachingChatClient.CacheKeyAdditionalValues`         | property        | extends the content-cache key                   |
|  [11]   | `DistributedCachingChatClient.JsonSerializerOptions`            | property        | serializer for cached payloads                  |
|  [12]   | `OpenTelemetryChatClient.EnableSensitiveData`                   | property        | includes prompt/response content in spans       |
|  [13]   | `OpenTelemetryChatClient.JsonSerializerOptions`                 | property        | serializer for telemetry payloads               |

- `FunctionInvokingChatClient.InvokeFunctionAsync(FunctionInvocationContext, ct)`/`CreateResponseMessages(ReadOnlySpan<FunctionInvocationResult>)`: `protected virtual` loop overrides; `FunctionInvoker` overrides dispatch without subclassing.
- `FunctionInvocationContext`: carries the mutable `Function`, `Arguments`, `CallContent`, and `Messages` for the in-flight call, exposed ambiently through `CurrentContext`.
- `FunctionInvocationResult`: `Status` (`FunctionInvocationStatus.RanToCompletion`/`NotFound`/`Exception`), `CallContent`, `Result`, `Exception`, `Terminate`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every decorator is a `DelegatingChatClient` wrapping the inner `IChatClient`; `Build` composes them outermost-last, so `UseOpenTelemetry().UseDistributedCache().UseFunctionInvocation()` nests the span over the cache lookup over the tool-call loop.
- `DistributedCachingChatClient` keys a response by the serialized request over `IDistributedCache` and replays the cached `ChatResponse` on a key hit; `CacheKeyAdditionalValues` folds extra discriminants and `EnableCaching` gates per-call cacheability.
- `ChatResponse.Usage` (`UsageDetails`: `InputTokenCount`, `OutputTokenCount`, `TotalTokenCount`, `CachedInputTokenCount`, `ReasoningTokenCount`, `AdditionalCounts`) carries the token vector the governance fold projects to a cost vector, a cache hit crediting `CachedInputTokenCount`.
- Custom middleware subclasses `DelegatingChatClient` and overrides its `virtual` `GetResponseAsync`/`GetStreamingResponseAsync`/`GetService` over `protected InnerClient`; response mutation uses the subclass surface, never the `sharedFunc` overload.
- Structured output derives the response-format schema from `AIJsonUtilities` and wraps the result in `ChatResponse<T>`.
- `SpeechToTextClientBuilder`, `TextToSpeechClientBuilder`, `ImageGeneratorBuilder`, `RealtimeClientBuilder`, and `HostedFileClientBuilder` repeat the same builder-decorator-`Add*` shape under `[Experimental("MEAI001")]`.

[STACKING]:
- `Microsoft.Extensions.AI.Abstractions`(`libs/csharp/.api/api-extensions-ai.md`): the abstractions tier owns `IChatClient`, `IEmbeddingGenerator`, `AIFunction`, `DelegatingChatClient`, `IChatReducer`, and `AIJsonUtilities`; every decorator subclasses `DelegatingChatClient`, `Build` emits an `IChatClient`, and structured output reads its schema from `AIJsonUtilities`.
- `api-otel.md`(`.api/api-otel.md`): `UseOpenTelemetry(source)` emits the `gen_ai.*` GenAI span and metric conventions on the `ActivitySource`/`Meter` the OTel composition root admits through `AddSource`/`AddMeter`.
- `api-hybrid-cache.md`(`libs/csharp/.api/api-hybrid-cache.md`): `UseDistributedCache` binds the resources-lane `HybridCache` surfaced as `IDistributedCache`, so the model response cache and the suite content cache share one store.
- `api-mcp.md`(`.api/api-mcp.md`): `McpClientTool : AIFunction` registers in `ChatOptions.Tools`, and `FunctionInvokingChatClient` runs the tool-call loop over each, routing through the brokered `CommandAIFunction` the governance fold supplies.
- within-fold: the governance fold composes the stack once at the capability-agent edge — `AddChatClient(sp => provider, lifetime).UseOpenTelemetry(...).UseDistributedCache(...).UseChatReducer(...).UseFunctionInvocation(...)` — reading every bound off the agent options row.

[LOCAL_ADMISSION]:
- `MaximumIterationsPerRequest`, `MaximumConsecutiveErrorsPerRequest`, the `IChatReducer` target count, and `EnableSensitiveData` are agent-options policy values, never literals; sensitive-data capture is opt-in per agent, never global.
- `DistributedCachingChatClient` binds the one resources-lane store, and the decorator stack is the one model-governance owner.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI`
- Owns: the provider-agnostic chat and embedding middleware pipeline — function invocation, response caching, GenAI telemetry, logging, option configuration, history reduction, and DI registration.
- Accept: decorators composed through `ChatClientBuilder`/`EmbeddingGeneratorBuilder` and registered via `AddChatClient`/`AddEmbeddingGenerator`.
- Reject: a hand-rolled retry loop, a per-call OTel span, a hand-rolled history trim, or a second response cache beside the decorators.
