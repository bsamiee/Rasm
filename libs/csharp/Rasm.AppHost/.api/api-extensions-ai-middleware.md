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

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                                                   |
| :-----: | :----------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `ChatClientBuilder`            | builder           | composes the `IChatClient` delegating pipeline           |
|  [02]   | `FunctionInvokingChatClient`   | delegating client | runs `AIFunction` tool calls in the response loop        |
|  [03]   | `DistributedCachingChatClient` | delegating client | content-keyed response cache over `IDistributedCache`    |
|  [04]   | `CachingChatClient`            | abstract base     | caching-decorator base of `DistributedCachingChatClient` |
|  [05]   | `OpenTelemetryChatClient`      | delegating client | GenAI `gen_ai.*` span/metric instrumentation             |
|  [06]   | `LoggingChatClient`            | delegating client | request/response structured logging                      |
|  [07]   | `ConfigureOptionsChatClient`   | delegating client | mutates `ChatOptions` per request                        |
|  [08]   | `DelegatingChatClient`         | middleware base   | public custom-middleware base; `virtual` verb pass-throughs over `protected InnerClient` |
|  [09]   | `AnonymousDelegatingChatClient`| delegating client | `internal sealed` delegate-backed client behind the public `Use(sharedFunc)`/`Use(getResponseFunc,getStreamingResponseFunc)` overloads; uninstantiable from a consumer package |

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
|  [07]   | `ChatClientStructuredOutputExtensions`                  | extension methods | typed `GetResponseAsync<T>` → `ChatResponse<T>` |
|  [08]   | `ChatResponse<T>`                                       | typed response    | strongly-typed structured-output response carrier |
|  [09]   | `DistributedCachingEmbeddingGeneratorBuilderExtensions` | extension methods | `UseDistributedCache` (embeddings) |
|  [10]   | `OpenTelemetryEmbeddingGeneratorBuilderExtensions`      | extension methods | `UseOpenTelemetry` (embeddings)    |
|  [11]   | `EmbeddingGeneratorBuilderEmbeddingGeneratorExtensions` | extension methods | `AsBuilder` (embeddings)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder composition and decorator wiring
- rail: capability-agent

| [INDEX] | [MEMBER]                                                                                           | [KIND]       | [RETURN]                            |
| :-----: | :------------------------------------------------------------------------------------------------- | :----------- | :---------------------------------- |
|  [01]   | `IChatClient.AsBuilder()`                                                                          | extension    | `ChatClientBuilder`                 |
|  [02]   | `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)`                                             | builder call | `ChatClientBuilder`                 |
|  [03]   | `ChatClientBuilder.Build(IServiceProvider?)`                                                       | builder call | `IChatClient`                       |
|  [04]  | `ChatClientBuilder.Use(Func<IChatClient,IServiceProvider,IChatClient>)`                            | builder call | `ChatClientBuilder`                 |
|  [05]  | `ChatClientBuilder.Use(Func<IEnumerable<ChatMessage>,ChatOptions?,Func<…,Task>,CancellationToken,Task> sharedFunc)` | builder call | `ChatClientBuilder` (wraps `AnonymousDelegatingChatClient`; pre/post only, no response handle) |
|  [06]  | `ChatClientBuilder.Use(getResponseFunc, getStreamingResponseFunc)`                                 | builder call | `ChatClientBuilder` (wraps `AnonymousDelegatingChatClient`; per-verb delegates) |
|  [07]   | `ChatClientBuilder.UseFunctionInvocation(ILoggerFactory?, Action<FunctionInvokingChatClient>?)`    | extension    | `ChatClientBuilder`                 |
|  [08]   | `ChatClientBuilder.UseDistributedCache(IDistributedCache?, Action<DistributedCachingChatClient>?)` | extension    | `ChatClientBuilder`                 |
|  [09]   | `ChatClientBuilder.UseOpenTelemetry(ILoggerFactory?, string?, Action<OpenTelemetryChatClient>?)`   | extension    | `ChatClientBuilder`                 |
|  [10]   | `ChatClientBuilder.UseLogging(ILoggerFactory?, Action<LoggingChatClient>?)`                        | extension    | `ChatClientBuilder`                 |
|  [11]   | `ChatClientBuilder.ConfigureOptions(Action<ChatOptions>)`                                          | extension    | `ChatClientBuilder`                 |
|  [12]   | `IChatClient.GetResponseAsync<T>(...)`                                                            | extension    | typed `Task<ChatResponse<T>>` |
|  [13]   | `ChatResponse<T>.Result` / `ChatResponse<T>.TryGetResult(out T?)`                                  | typed read   | bound `T` deserialized from the response text against the generated JSON schema |
|  [14]   | `IEmbeddingGenerator<TIn,TE>.AsBuilder()`                                                          | extension    | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [15]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseDistributedCache(...)`                                       | extension    | `EmbeddingGeneratorBuilder<TIn,TE>` |
|  [16]   | `EmbeddingGeneratorBuilder<TIn,TE>.UseOpenTelemetry(...)`                                          | extension    | `EmbeddingGeneratorBuilder<TIn,TE>` |

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
- custom middleware: a response-mutating governance arm subclasses the public `DelegatingChatClient` base (`virtual` `GetResponseAsync`/`GetStreamingResponseAsync`/`GetService` over the `protected InnerClient`) woven through `ChatClientBuilder.Use(Func<IChatClient,IChatClient>)`; the `internal sealed AnonymousDelegatingChatClient` behind the `Use(sharedFunc)` overload exposes only a pre/post `Func<…,Task>` with no `ChatResponse` handle, so a middleware that rewrites the response or redacts content is the subclass, never the `sharedFunc` overload.
- structured output: `ChatClientStructuredOutputExtensions.GetResponseAsync<T>` derives the response-format JSON schema from `AIJsonUtilities.CreateJsonSchema(typeof(T))` (set `useJsonSchemaResponseFormat: true` to force `ChatResponseFormat.ForJsonSchema` over a textual prompt), calls the inner `IChatClient`, and wraps the result in `ChatResponse<T>` — `.Result` deserializes the bound `T` (throwing on a parse miss) and `TryGetResult(out T?)` is the non-throwing read. This is the ONLY typed-output surface; there is no `GetStreamingResponseAsync<T>`, so a typed streaming round-trip is the rejected form. The schema flows from the same `AIJsonUtilities` owner the tool-arguments path uses, so structured output and `AIFunction` arguments share one schema generator (`api-extensions-ai.md`).

[IMPLEMENTATION_LAW]: AppHost usage
- rail: capability-agent

- the model-governance fold composes the four decorators once at the capability-agent composition edge: `client.AsBuilder().UseOpenTelemetry(...).UseDistributedCache(hybridCache-backed IDistributedCache).UseFunctionInvocation(...).Build(sp)`.
- the cache is the resources-lane `HybridCache` exposed as `IDistributedCache`, so the model response cache and the suite content cache share one store, never a second cache owner.
- `MaximumIterationsPerRequest` and `MaximumConsecutiveErrorsPerRequest` are policy values read off the agent options row, never literals; the function-invocation loop bound traces to the agent's deadline class.
- the concrete decorators are the one model-governance owner; a hand-rolled retry loop, a per-call OTel span, or a second response cache beside the decorators is the deleted form.
