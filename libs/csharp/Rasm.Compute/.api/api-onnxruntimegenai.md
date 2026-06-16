# [RASM_COMPUTE_API_ONNXRUNTIMEGENAI]

`Microsoft.ML.OnnxRuntimeGenAI` supplies the generative token-streaming runtime —
process-global init, model/config handles, tokenizer and chat-template assembly,
generator search options and structured-output guidance, the per-step token loop,
and the sole generative fault rail; `Microsoft.Extensions.AI.Abstractions` supplies
the `IChatClient` projection that composes the same handle chain for the M.E.AI
streaming consumer. Both serve the Compute model rail's `GENERATIVE_RUN` cluster.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntimeGenAI`
- package: `Microsoft.ML.OnnxRuntimeGenAI`
- assembly: `Microsoft.ML.OnnxRuntimeGenAI`
- namespace: `Microsoft.ML.OnnxRuntimeGenAI`
- asset: managed wrapper plus `libonnxruntime-genai` native runtime asset
- rail: model

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- asset: managed abstractions
- rail: model

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handle chain and generation contracts
- rail: model

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]         | [CAPABILITY]                               |
| :-----: | :-------------------------- | :--------------------- | :----------------------------------------- |
|   [1]   | `OgaHandle`                 | process-global handle  | owns native init/teardown (LIFO outermost) |
|   [2]   | `Config`                    | model configuration    | configures model dir and providers         |
|   [3]   | `Model`                     | model root             | loads the generative model                 |
|   [4]   | `Tokenizer`                 | tokenizer root         | encodes prompts, applies chat template     |
|   [5]   | `TokenizerStream`           | incremental decoder    | decodes one token per step                 |
|   [6]   | `Sequences`                 | token-sequence carrier | carries encoded/generated token sequences  |
|   [7]   | `GeneratorParams`           | generation policy      | carries search options and guidance        |
|   [8]   | `Generator`                 | generation engine      | runs the per-step token loop               |
|   [9]   | `OnnxRuntimeGenAIException` | fault rail             | the sole generative exception type         |

[PUBLIC_TYPE_SCOPE]: M.E.AI projection
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]     | [CAPABILITY]                               |
| :-----: | :--------------------------- | :----------------- | :----------------------------------------- |
|   [1]   | `OnnxRuntimeGenAIChatClient` | `IChatClient` impl | streaming chat over the GenAI handle chain |
|   [2]   | `IChatClient`                | M.E.AI contract    | response and streaming-response surface    |
|   [3]   | `ChatResponse`               | M.E.AI response    | non-streaming response carrier             |
|   [4]   | `ChatResponseUpdate`         | M.E.AI update      | streaming incremental update carrier       |
|   [5]   | `ChatMessage`                | M.E.AI message     | role-tagged message carrier                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process and model lifecycle
- rail: model

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                           | [CAPABILITY]                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `new OgaHandle()`          | `OgaHandle()`                                                          | process-global init (`IDisposable`)   |
|   [2]   | `new Config(string)`       | `Config(string modelPath)`                                             | reads `{modelPath}/genai_config.json` |
|   [3]   | `Config.AppendProvider`    | `void AppendProvider(string provider)`                                 | injects an execution provider         |
|   [4]   | `Config.SetProviderOption` | `void SetProviderOption(string provider, string option, string value)` | sets a provider option key            |
|   [5]   | `new Model(Config)`        | `Model(Config config)`                                                 | loads the model from config           |
|   [6]   | `Model.GetModelType`       | `string GetModelType()`                                                | reports the model type string         |

[ENTRYPOINT_SCOPE]: tokenization and chat template
- rail: model

Tokenizer calls keep exact return and parameter shapes in package topology; the table keeps the operation inventory.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]            | [CAPABILITY]                      |
| :-----: | :---------------------------- | :---------------------- | :-------------------------------- |
|   [1]   | `Tokenizer`                   | model constructor       | builds the tokenizer over a model |
|   [2]   | `Tokenizer.CreateStream`      | stream factory          | sole `TokenizerStream` source     |
|   [3]   | `Tokenizer.ApplyChatTemplate` | chat-template operation | assembles the prompt natively     |
|   [4]   | `Tokenizer.Encode`            | text encode             | encodes text to token sequences   |
|   [5]   | `TokenizerStream.Decode`      | token decode            | decodes one token incrementally   |

[ENTRYPOINT_SCOPE]: generation loop and search options
- rail: model

`SetSearchOption` admits numeric and bool values only; `SetGuidance` carries type, data, and FFTokens policy.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]      | [CAPABILITY]                      |
| :-----: | :--------------------- | :---------------- | :-------------------------------- |
|   [1]   | `GeneratorParams`      | model constructor | builds generation params          |
|   [2]   | `SetSearchOption`      | numeric option    | sets numeric search option        |
|   [3]   | `SetSearchOption`      | flag option       | sets bool search option           |
|   [4]   | `SetGuidance`          | guidance option   | sets structured-output constraint |
|   [5]   | `Generator`            | model plus params | builds the generator              |
|   [6]   | `AppendTokenSequences` | sequence append   | seeds the prompt tokens           |
|   [7]   | `AppendTokens`         | token span append | re-feeds typed tool-result tokens |
|   [8]   | `RewindTo`             | sequence rewind   | rewinds a partial turn            |
|   [9]   | `GenerateNextToken`    | generation step   | advances one step                 |
|  [10]   | `IsDone`               | loop predicate    | terminates generation             |
|  [11]   | `GetSequence`          | sequence view     | exposes generated token memory    |

[ENTRYPOINT_SCOPE]: M.E.AI chat client
- rail: model

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]       | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :----------------- | :------------------------------- |
|   [1]   | `OnnxRuntimeGenAIChatClient`            | model wrapper      | wraps a `Model` as `IChatClient` |
|   [2]   | `IChatClient.GetResponseAsync`          | async response     | non-streaming response           |
|   [3]   | `IChatClient.GetStreamingResponseAsync` | streaming response | streaming response               |

```csharp generated
OnnxRuntimeGenAIChatClient(Model model, OnnxRuntimeGenAIChatClientOptions? options = null)
Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken)
IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken)
```

## [4]-[IMPLEMENTATION_LAW]

[HANDLE_CHAIN]:
- Every GenAI type is `IDisposable` wrapping a native handle; the `using` order is LIFO with `OgaHandle` outermost (process-global init/teardown).
- `Config(modelDir)` opens `{modelDir}/genai_config.json`; a missing or malformed model directory faults `OnnxRuntimeGenAIException` at construction.
- Provider selection rides `Config.AppendProvider`/`SetProviderOption` before `new Model(config)`, never per generation.

[GENERATION_LOOP]:
- Numeric search options pass as `double` and flags as `bool`; there is no string-valued `SetSearchOption` overload.
- `GetSequence(ulong)` returns a `ReadOnlySpan<int>` view over native memory owned by the live `Generator`; the newest token (`sequence[^1]`) copies out before the next iteration and never retains past the loop.
- `TokenizerStream` is obtained ONLY through `Tokenizer.CreateStream()`; `TokenizerStream` has no public constructor.
- The tool-call arm re-feeds typed results through `Generator.AppendTokens`/`AppendTokenSequences` or rewinds a partial turn through `RewindTo(ulong)`.

[FAULT_LAW]:
- `OnnxRuntimeGenAIException` is the sole fault rail at `Model`/`Config` construction and across the generation loop; it lifts to `ComputeFault.ModelRejected` at the boundary, never a per-call catch.

[LOCAL_ADMISSION]:
- Token-streaming is a run mode on the owned model lane; a `GenerativeService`, `ChatClient`, `Conversation`, or `PromptService` wrapper is the rejected form.
- The built-in `OnnxRuntimeGenAIChatClient : IChatClient` composes the same handle chain when the M.E.AI streaming abstraction is the consumer.
- Grammar-constrained structured output is enforced at generation through `SetGuidance`; a managed JSON-schema validator over the output is the rejected form.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntimeGenAI` + `Microsoft.Extensions.AI.Abstractions`
- Owns: generative token-streaming runtime and its `IChatClient` projection
- Accept: model-dir generative runs over the LIFO handle chain
- Reject: chat-client/conversation/prompt service families and managed output validators
