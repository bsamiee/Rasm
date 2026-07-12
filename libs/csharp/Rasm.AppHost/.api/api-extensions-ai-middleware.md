# [RASM_APPHOST_API_EXTENSIONS_AI_MIDDLEWARE]

`Microsoft.Extensions.AI` is the concrete middleware package over the `Microsoft.Extensions.AI.Abstractions` contracts: a fluent `ChatClientBuilder`/`EmbeddingGeneratorBuilder` composes provider-agnostic decorators — function-invocation, distributed response caching, OpenTelemetry GenAI instrumentation, logging, and option configuration — into one delegating pipeline the AppHost model-governance fold consumes so every model call is metered, content-cached, and traced without coupling to a provider. The abstractions surface (`IChatClient`, `IEmbeddingGenerator`, `AIFunction`, the modal clients) lives at `api-extensions-ai.md`; this catalogue carries only the concrete builder and decorator types the middleware composition reaches for.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI`

- package: `Microsoft.Extensions.AI`
- assembly: `Microsoft.Extensions.AI`
- namespace: `Microsoft.Extensions.AI`
- asset: runtime library
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: chat-client builder and decorators

- rail: capability-agent

| [INDEX] | [SYMBOL]                        | [FAMILY]          | [ROLE]                             |
| :-----: | :------------------------------ | :---------------- | :--------------------------------- |
|  [01]   | `ChatClientBuilder`             | builder           | `IChatClient` pipeline composer    |
|  [02]   | `FunctionInvokingChatClient`    | delegating client | `AIFunction` tool invocation       |
|  [03]   | `DistributedCachingChatClient`  | delegating client | content-keyed response cache       |
|  [04]   | `CachingChatClient`             | abstract base     | cache decorator                    |
|  [05]   | `OpenTelemetryChatClient`       | delegating client | GenAI span/metric telemetry        |
|  [06]   | `LoggingChatClient`             | delegating client | structured request/response logs   |
|  [07]   | `ConfigureOptionsChatClient`    | delegating client | per-request `ChatOptions` mutation |
|  [08]   | `DelegatingChatClient`          | middleware base   | custom middleware                  |
|  [09]   | `AnonymousDelegatingChatClient` | delegating client | delegate-backed middleware         |

[DELEGATING_CHAT_CLIENT]:

- Surface: public custom-middleware base.
- Dispatch: `virtual` verbs delegate through `protected InnerClient`.

[CACHING_CHAT_CLIENT]:

- Base: caching decorator for `DistributedCachingChatClient` over `IDistributedCache`.

[ANONYMOUS_DELEGATING_CHAT_CLIENT]:

- Visibility: `internal sealed` and uninstantiable from a consumer package.
- Binding: backs the public `Use(sharedFunc)` and `Use(getResponseFunc,getStreamingResponseFunc)` overloads.

[PUBLIC_TYPE_SCOPE]: embedding-generator builder and decorators

- rail: capability-agent

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [RAIL]                                      |
| :-----: | :--------------------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `EmbeddingGeneratorBuilder<TInput,TEmbedding>` | builder         | composes the `IEmbeddingGenerator` pipeline |
|  [02]   | `DistributedCachingEmbeddingGenerator<TIn,TE>` | delegating gen. | content-keyed embedding cache               |
|  [03]   | `OpenTelemetryEmbeddingGenerator<TIn,TE>`      | delegating gen. | embedding-call OTel instrumentation         |

[PUBLIC_TYPE_SCOPE]: builder extension surfaces

- rail: capability-agent

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [RAIL]                             |
| :-----: | :------------------------------------------------------ | :---------------- | :--------------------------------- |
|  [01]   | `FunctionInvokingChatClientBuilderExtensions`           | extension methods | `UseFunctionInvocation`            |
|  [02]   | `DistributedCachingChatClientBuilderExtensions`         | extension methods | `UseDistributedCache`              |
|  [03]   | `OpenTelemetryChatClientBuilderExtensions`              | extension methods | `UseOpenTelemetry`                 |
|  [04]   | `LoggingChatClientBuilderExtensions`                    | extension methods | `UseLogging`                       |
|  [05]   | `ConfigureOptionsChatClientBuilderExtensions`           | extension methods | `ConfigureOptions`                 |
|  [06]   | `ChatClientBuilderChatClientExtensions`                 | extension methods | `AsBuilder`                        |
|  [07]   | `ChatClientStructuredOutputExtensions`                  | extension methods | typed `GetResponseAsync<T>`        |
|  [08]   | `ChatResponse<T>`                                       | typed response    | structured-output carrier          |
|  [09]   | `DistributedCachingEmbeddingGeneratorBuilderExtensions` | extension methods | `UseDistributedCache` (embeddings) |
|  [10]   | `OpenTelemetryEmbeddingGeneratorBuilderExtensions`      | extension methods | `UseOpenTelemetry` (embeddings)    |
|  [11]   | `EmbeddingGeneratorBuilderEmbeddingGeneratorExtensions` | extension methods | `AsBuilder` (embeddings)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder composition and decorator wiring

- rail: capability-agent

`ChatClientBuilder.Use` and `Build` are builder calls; extension rows compose builders, and typed reads project `ChatResponse<T>`.

| [INDEX] | [MEMBER]                                                                                           | [RESULT]                            |
| :-----: | :------------------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `IChatClient.AsBuilder()`                                                                          | `ChatClientBuilder`                 |
|  [02]   | `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)`                                             | `ChatClientBuilder`                 |
|  [03]   | `ChatClientBuilder.Build(IServiceProvider?)`                                                       | `IChatClient`                       |
|  [04]   | `ChatClientBuilder.Use(Func<IChatClient,IServiceProvider,IChatClient>)`                            | `ChatClientBuilder`                 |
|  [05]   | `ChatClientBuilder.Use(sharedFunc)`                                                                | `ChatClientBuilder`                 |
|  [06]   | `ChatClientBuilder.Use(getResponseFunc, getStreamingResponseFunc)`                                 | `ChatClientBuilder`                 |
|  [07]   | `ChatClientBuilder.UseFunctionInvocation(ILoggerFactory?, Action<FunctionInvokingChatClient>?)`    | `ChatClientBuilder`                 |
|  [08]   | `ChatClientBuilder.UseDistributedCache(IDistributedCache?, Action<DistributedCachingChatClient>?)` | `ChatClientBuilder`                 |
|  [09]   | `ChatClientBuilder.UseOpenTelemetry(ILoggerFactory?, string?, Action<OpenTelemetryChatClient>?)`   | `ChatClientBuilder`                 |
|  [10]   | `ChatClientBuilder.UseLogging(ILoggerFactory?, Action<LoggingChatClient>?)`                        | `ChatClientBuilder`                 |
|  [11]   | `ChatClientBuilder.ConfigureOptions(Action<ChatOptions>)`                                          | `ChatClientBuilder`                 |
|  [12]   | `IChatClient.GetResponseAsync<T>(...)`                                                             | `Task<ChatResponse<T>>`             |
|  [13]   | `ChatResponse<T>.Result`                                                                           | `T`                                 |
|  [14]   | `ChatResponse<T>.TryGetResult(out T?)`                                                             | non-throwing result                 |
|  [15]   | `IEmbeddingGenerator<TIn,TE>.AsBuilder()`                                                          | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [16]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseDistributedCache(...)`                                       | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [17]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseOpenTelemetry(...)`                                          | `EmbeddingGeneratorBuilder<TIn,TE>` |

[SHARED_FUNC_OVERLOAD]:

- Signature: `Func<IEnumerable<ChatMessage>,ChatOptions?,Func<…,Task>,CancellationToken,Task> sharedFunc`.
- Binding: wraps `AnonymousDelegatingChatClient` for pre/post handling without a response handle.

[PER_VERB_OVERLOAD]:

- Binding: wraps `AnonymousDelegatingChatClient` with per-verb delegates.

[TYPED_RESPONSE_READS]:

- Source: response text deserialized against the generated JSON schema.
- Result: `.Result` returns the bound `T`; `TryGetResult(out T?)` is the non-throwing read.

`GetResponseAsync<T>` accepts `IEnumerable<ChatMessage>` / `string` / `ChatMessage`, serializer options, chat options, optional JSON-schema response formatting, and a cancellation token. It is the typed structured-output round-trip and has no streaming twin.

[ENTRYPOINT_SCOPE]: decorator-tuning properties

- rail: capability-agent

| [INDEX] | [MEMBER]                                                        | [KIND]   | [SEMANTICS]                             |
| :-----: | :-------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `FunctionInvokingChatClient.MaximumIterationsPerRequest`        | property | caps tool-call loop iterations          |
|  [02]   | `FunctionInvokingChatClient.MaximumConsecutiveErrorsPerRequest` | property | caps consecutive tool-call errors       |
|  [03]   | `FunctionInvokingChatClient.AllowConcurrentInvocation`          | property | gates parallel tool execution           |
|  [04]   | `FunctionInvokingChatClient.AdditionalTools`                    | property | tools merged into every request         |
|  [05]   | `FunctionInvokingChatClient.IncludeDetailedErrors`              | property | surfaces tool-error detail to the model |
|  [06]   | `FunctionInvokingChatClient.TerminateOnUnknownCalls`            | property | halts on an unresolved tool name        |
|  [07]   | `CachingChatClient.CacheKeyAdditionalValues`                    | property | extends the content-cache key           |

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: middleware composition

- rail: capability-agent

- pipeline shape: every decorator is a `DelegatingChatClient` wrapping the inner `IChatClient`; `ChatClientBuilder.Use`/`Build` composes them outermost-last so `UseOpenTelemetry().UseDistributedCache().UseFunctionInvocation()` reads outermost-to-innermost in call order, the OTel span enclosing the cache lookup enclosing the function-invocation loop.
- caching: `DistributedCachingChatClient` keys the response by the serialized request (messages + options) over an injected `IDistributedCache`; an identical request-under-key replays the cached `ChatResponse` rather than re-calling the provider, and `CacheKeyAdditionalValues` folds extra discriminants into the key.
- token usage: a completed `ChatResponse` carries `ChatResponse.Usage` (`UsageDetails` with `InputTokenCount`/`OutputTokenCount`/`TotalTokenCount`) the governance fold projects onto a cost vector.
- instrumentation: `UseOpenTelemetry` emits the OTel GenAI `gen_ai.*` conventions; the source name argument names the `ActivitySource`/`Meter` the observability composition root registers.
- function invocation: `FunctionInvokingChatClient` runs the tool-call loop over `ChatOptions.Tools`, invoking each `AIFunction`; the governance fold supplies the brokered `CommandAIFunction` instances so every tool call routes through the command algebra.
- custom middleware: response-mutating governance arms subclass `DelegatingChatClient` and override its `virtual` `GetResponseAsync`, `GetStreamingResponseAsync`, or `GetService` verbs over `protected InnerClient`.
- custom binding: `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)` weaves the subclass into the pipeline; the `internal sealed AnonymousDelegatingChatClient` behind `Use(sharedFunc)` exposes only a pre/post `Func<…,Task>` without a `ChatResponse` handle.
- response mutation: middleware that rewrites or redacts a response uses the subclass surface, never the `sharedFunc` overload.
- structured output: `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` derives the response-format JSON schema from `AIJsonUtilities.CreateJsonSchema(typeof(T))`, calls the inner `IChatClient`, and wraps the result in `ChatResponse<T>`.
- response format: `useJsonSchemaResponseFormat: true` forces `ChatResponseFormat.ForJsonSchema` over a textual prompt.
- typed read: `.Result` deserializes the bound `T` and throws on a parse miss; `TryGetResult(out T?)` is the non-throwing read.
- typed streaming: `GetResponseAsync<T>` is the sole typed-output surface; `GetStreamingResponseAsync<T>` has no corresponding surface.
- schema ownership: structured output and `AIFunction` arguments share the `AIJsonUtilities` schema generator owned by `api-extensions-ai.md`.

[IMPLEMENTATION_LAW]: AppHost usage

- rail: capability-agent

- the model-governance fold composes the four decorators once at the capability-agent composition edge: `client.AsBuilder().UseOpenTelemetry(...).UseDistributedCache(hybridCache-backed IDistributedCache).UseFunctionInvocation(...).Build(sp)`.
- the cache is the resources-lane `HybridCache` exposed as `IDistributedCache`, so the model response cache and the suite content cache share one store, never a second cache owner.
- `MaximumIterationsPerRequest` and `MaximumConsecutiveErrorsPerRequest` are policy values read off the agent options row, never literals; the function-invocation loop bound traces to the agent's deadline class.
- the concrete decorators are the one model-governance owner; a hand-rolled retry loop, a per-call OTel span, or a second response cache beside the decorators is the deleted form.
