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

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]        | [CAPABILITY]                              |
| :-----: | :------------------------ | :-------------------- | :---------------------------------------- |
|   [1]   | `OgaHandle`               | process-global handle  | owns native init/teardown (LIFO outermost) |
|   [2]   | `Config`                  | model configuration    | configures model dir and providers        |
|   [3]   | `Model`                   | model root             | loads the generative model                |
|   [4]   | `Tokenizer`               | tokenizer root         | encodes prompts, applies chat template    |
|   [5]   | `TokenizerStream`         | incremental decoder    | decodes one token per step                |
|   [6]   | `Sequences`               | token-sequence carrier | carries encoded/generated token sequences |
|   [7]   | `GeneratorParams`         | generation policy      | carries search options and guidance       |
|   [8]   | `Generator`               | generation engine      | runs the per-step token loop              |
|   [9]   | `OnnxRuntimeGenAIException`| fault rail             | the sole generative exception type        |

[PUBLIC_TYPE_SCOPE]: M.E.AI projection
- rail: model

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]                              |
| :-----: | :----------------------------- | :------------------ | :---------------------------------------- |
|   [1]   | `OnnxRuntimeGenAIChatClient`   | `IChatClient` impl  | streaming chat over the GenAI handle chain |
|   [2]   | `IChatClient`                  | M.E.AI contract     | response and streaming-response surface    |
|   [3]   | `ChatResponse`                 | M.E.AI response     | non-streaming response carrier             |
|   [4]   | `ChatResponseUpdate`           | M.E.AI update       | streaming incremental update carrier       |
|   [5]   | `ChatMessage`                  | M.E.AI message      | role-tagged message carrier                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process and model lifecycle
- rail: model

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                  | [CAPABILITY]                          |
| :-----: | :-------------------------------- | :-------------------------------------------- | :------------------------------------ |
|   [1]   | `new OgaHandle()`                 | `OgaHandle()`                                 | process-global init (`IDisposable`)   |
|   [2]   | `new Config(string)`              | `Config(string modelPath)`                    | reads `{modelPath}/genai_config.json` |
|   [3]   | `Config.AppendProvider`           | `void AppendProvider(string provider)`        | injects an execution provider         |
|   [4]   | `Config.SetProviderOption`        | `void SetProviderOption(string provider, string option, string value)` | sets a provider option key |
|   [5]   | `new Model(Config)`               | `Model(Config config)`                        | loads the model from config           |
|   [6]   | `Model.GetModelType`              | `string GetModelType()`                       | reports the model type string         |

[ENTRYPOINT_SCOPE]: tokenization and chat template
- rail: model

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                              | [CAPABILITY]                         |
| :-----: | :---------------------------- | :----------------------------------------------------------------------- | :----------------------------------- |
|   [1]   | `new Tokenizer(Model)`        | `Tokenizer(Model model)`                                                  | builds the tokenizer over a model    |
|   [2]   | `Tokenizer.CreateStream`      | `TokenizerStream CreateStream()`                                          | sole `TokenizerStream` source        |
|   [3]   | `Tokenizer.ApplyChatTemplate` | `string ApplyChatTemplate(string template_str, string messages, string tools, bool add_generation_prompt)` | assembles the prompt natively |
|   [4]   | `Tokenizer.Encode`            | `Sequences Encode(string text)`                                          | encodes text to token sequences      |
|   [5]   | `TokenizerStream.Decode`      | `string Decode(int token)`                                               | decodes one token incrementally      |

[ENTRYPOINT_SCOPE]: generation loop and search options
- rail: model

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                       | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :---------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `new GeneratorParams(Model)`       | `GeneratorParams(Model model)`                                    | builds generation params              |
|   [2]   | `GeneratorParams.SetSearchOption`  | `void SetSearchOption(string name, double value)`                | numeric search option                 |
|   [3]   | `GeneratorParams.SetSearchOption`  | `void SetSearchOption(string name, bool value)`                  | flag search option (no string overload) |
|   [4]   | `GeneratorParams.SetGuidance`      | `void SetGuidance(string type, string data, bool enableFFTokens)`| structured-output constraint          |
|   [5]   | `new Generator(Model, GeneratorParams)` | `Generator(Model model, GeneratorParams generatorParams)`   | builds the generator                  |
|   [6]   | `Generator.AppendTokenSequences`   | `void AppendTokenSequences(Sequences sequences)`                 | seeds the prompt tokens               |
|   [7]   | `Generator.AppendTokens`           | `void AppendTokens(ReadOnlySpan<int> tokens)`                    | re-feeds typed tool-result tokens     |
|   [8]   | `Generator.RewindTo`               | `void RewindTo(ulong newLength)`                                 | rewinds a partial turn                 |
|   [9]   | `Generator.GenerateNextToken`      | `void GenerateNextToken()`                                       | advances one step                     |
|  [10]   | `Generator.IsDone`                 | `bool IsDone()`                                                  | loop terminator                       |
|  [11]   | `Generator.GetSequence`            | `ReadOnlySpan<int> GetSequence(ulong index)`                    | native-memory view of generated tokens |

[ENTRYPOINT_SCOPE]: M.E.AI chat client
- rail: model

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                                                                 | [CAPABILITY]                       |
| :-----: | :--------------------------------------- | :-------------------------------------------------------------------------- | :--------------------------------- |
|   [1]   | `new OnnxRuntimeGenAIChatClient(Model)`  | `OnnxRuntimeGenAIChatClient(Model model, OnnxRuntimeGenAIChatClientOptions? options = null)` | wraps a `Model` as `IChatClient` |
|   [2]   | `IChatClient.GetResponseAsync`           | `Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken)` | non-streaming response |
|   [3]   | `IChatClient.GetStreamingResponseAsync`  | `IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken)` | streaming response |

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
