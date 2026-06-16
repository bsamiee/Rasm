# [RASM_COMPUTE_API_ONNXRUNTIMEGENAI]

`Microsoft.ML.OnnxRuntimeGenAI` supplies the generative token-streaming runtime —
process-global init, model/config handles, tokenizer and chat-template assembly,
generator search options and structured-output guidance, the per-step token loop,
LoRA adapter hot-swap, and the sole generative fault rail; `Microsoft.Extensions.AI.Abstractions` supplies
the `IChatClient` projection that composes the same handle chain for the M.E.AI
streaming consumer. Both serve the Compute model rail's `GENERATIVE_RUN` cluster.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntimeGenAI`
- package: `Microsoft.ML.OnnxRuntimeGenAI` (native runtime meta-package)
- managed-assembly: `Microsoft.ML.OnnxRuntimeGenAI.Managed` → `lib/net8.0/Microsoft.ML.OnnxRuntimeGenAI.dll`
- namespace: `Microsoft.ML.OnnxRuntimeGenAI`
- asset: native-only meta-package (`libonnxruntime-genai`) + managed facade via transitive `Microsoft.ML.OnnxRuntimeGenAI.Managed`
- rail: model
- decompile-source: `~/.nuget/packages/microsoft.ml.onnxruntimegenai.managed/0.14.1/lib/net8.0/Microsoft.ML.OnnxRuntimeGenAI.dll`

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions`
- assembly: `Microsoft.Extensions.AI.Abstractions`
- namespace: `Microsoft.Extensions.AI`
- asset: managed abstractions
- rail: model

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handle chain and generation contracts
- rail: model
- source: decompile-verified against `Microsoft.ML.OnnxRuntimeGenAI.Managed` 0.14.1

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
|   [9]   | `Adapters`                  | LoRA adapter registry  | loads/unloads named LoRA adapters          |
|  [10]   | `OnnxRuntimeGenAIException` | fault rail             | the sole generative exception type         |

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
- source: decompile-verified

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                           | [CAPABILITY]                          |
| :-----: | :------------------------- | :--------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `new OgaHandle()`          | `OgaHandle()`                                                          | process-global init (`IDisposable`)   |
|   [2]   | `new Config(string)`       | `Config(string modelPath)`                                             | reads `{modelPath}/genai_config.json` |
|   [3]   | `Config.ClearProviders`    | `void ClearProviders()`                                                | removes all configured providers      |
|   [4]   | `Config.AppendProvider`    | `void AppendProvider(string provider)`                                 | injects an execution provider         |
|   [5]   | `Config.SetProviderOption` | `void SetProviderOption(string provider, string option, string value)` | sets a provider option key            |
|   [6]   | `Config.Overlay`           | `void Overlay(string json)`                                            | applies a JSON config overlay         |
|   [7]   | `new Model(string)`        | `Model(string modelPath)`                                              | loads the model directly from path    |
|   [8]   | `new Model(Config)`        | `Model(Config config)`                                                 | loads the model from config           |
|   [9]   | `Model.GetModelType`       | `string GetModelType()`                                                | reports the model type string         |

[ENTRYPOINT_SCOPE]: tokenization and chat template
- rail: model
- source: decompile-verified

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                               | [CAPABILITY]                           |
| :-----: | :---------------------------- | :--------------------------------------------------------------------------------------------------------- | :------------------------------------- |
|   [1]   | `new Tokenizer(Model)`        | `Tokenizer(Model model)`                                                                                   | builds the tokenizer over a model      |
|   [2]   | `Tokenizer.CreateStream`      | `TokenizerStream CreateStream()`                                                                           | sole `TokenizerStream` source          |
|   [3]   | `Tokenizer.ApplyChatTemplate` | `string ApplyChatTemplate(string template_str, string messages, string tools, bool add_generation_prompt)` | assembles the prompt natively          |
|   [4]   | `Tokenizer.Encode`            | `Sequences Encode(string str)`                                                                             | encodes one string to a `Sequences`    |
|   [5]   | `Tokenizer.EncodeBatch`       | `Sequences EncodeBatch(string[] strings)`                                                                  | encodes a batch of strings             |
|   [6]   | `Tokenizer.Decode`            | `string Decode(ReadOnlySpan<int> sequence)`                                                                | decodes a full token span to string    |
|   [7]   | `Tokenizer.DecodeBatch`       | `string[] DecodeBatch(Sequences sequences)`                                                                | decodes all sequences in a `Sequences` |
|   [8]   | `TokenizerStream.Decode`      | `string Decode(int token)`                                                                                 | decodes one token incrementally        |

[ENTRYPOINT_SCOPE]: generation loop and search options
- rail: model
- source: decompile-verified

`SetSearchOption` admits numeric (`double`) and bool values only; `SetGuidance` carries type, data, and FFTokens policy. `GetSearchNumber`/`GetSearchBool` read back any previously-set option.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                              | [CAPABILITY]                           |
| :-----: | :--------------------- | :------------------------------------------------------------------------ | :------------------------------------- |
|   [1]   | `new GeneratorParams`  | `GeneratorParams(Model model)`                                            | builds generation params               |
|   [2]   | `SetSearchOption`      | `void SetSearchOption(string searchOption, double value)`                 | numeric search option                  |
|   [3]   | `SetSearchOption`      | `void SetSearchOption(string searchOption, bool value)`                   | flag search option                     |
|   [4]   | `GetSearchNumber`      | `double GetSearchNumber(string searchOption)`                             | reads back a numeric search option     |
|   [5]   | `GetSearchBool`        | `bool GetSearchBool(string searchOption)`                                 | reads back a bool search option        |
|   [6]   | `SetGuidance`          | `void SetGuidance(string type, string data, bool enableFFTokens = false)` | structured-output constraint           |
|   [7]   | `new Generator`        | `Generator(Model model, GeneratorParams generatorParams)`                 | builds the generator                   |
|   [8]   | `AppendTokenSequences` | `void AppendTokenSequences(Sequences sequences)`                          | seeds the prompt tokens                |
|   [9]   | `AppendTokens`         | `void AppendTokens(ReadOnlySpan<int> inputIDs)`                           | re-feeds token span                    |
|  [10]   | `RewindTo`             | `void RewindTo(ulong newLength)`                                          | rewinds sequence to given length       |
|  [11]   | `GenerateNextToken`    | `void GenerateNextToken()`                                                | advances one step                      |
|  [12]   | `IsDone`               | `bool IsDone()`                                                           | terminates generation                  |
|  [13]   | `GetSequence`          | `ReadOnlySpan<int> GetSequence(ulong index)`                              | view over native token memory          |
|  [14]   | `GetNextTokens`        | `ReadOnlySpan<int> GetNextTokens()`                                       | span of most-recently generated tokens |
|  [15]   | `TokenCount`           | `ulong TokenCount()`                                                      | current sequence token count           |
|  [16]   | `SetActiveAdapter`     | `void SetActiveAdapter(Adapters adapters, string adapterName)`            | hot-swaps the active LoRA adapter      |
|  [17]   | `SetModelInput`        | `void SetModelInput(string name, Tensor value)`                           | injects a named model input tensor     |
|  [18]   | `SetInputs`            | `void SetInputs(NamedTensors namedTensors)`                               | injects a named-tensor batch           |
|  [19]   | `GetInput`             | `Tensor GetInput(string inputName)`                                       | retrieves a named input tensor         |
|  [20]   | `GetOutput`            | `Tensor GetOutput(string outputName)`                                     | retrieves a named output tensor        |
|  [21]   | `SetRuntimeOption`     | `void SetRuntimeOption(string key, string value)`                         | sets a generator runtime option        |

[ENTRYPOINT_SCOPE]: LoRA adapter management
- rail: model
- source: decompile-verified

`Adapters : SafeHandle`; created per `Model`, lives for the adapter set's lifetime.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                               | [CAPABILITY]                         |
| :-----: | :-------------------- | :--------------------------------------------------------- | :----------------------------------- |
|   [1]   | `new Adapters(Model)` | `Adapters(Model model)`                                    | creates the adapter registry         |
|   [2]   | `LoadAdapter`         | `void LoadAdapter(string adapterPath, string adapterName)` | loads a named LoRA adapter from path |
|   [3]   | `UnloadAdapter`       | `void UnloadAdapter(string adapterName)`                   | unloads a named LoRA adapter         |

[ENTRYPOINT_SCOPE]: recognized `SetSearchOption` key strings
- source: doc-sourced (ORT-GenAI 0.14.x public documentation); key strings pass through to native `OgaGeneratorParamsSetSearchNumber`/`OgaGeneratorParamsSetSearchBool` without managed validation — no managed string registry exists in the binary
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [KEY_STRING]         | [VALUE_TYPE] | [CAPABILITY]                               |
| :-----: | :------------------- | :----------- | :----------------------------------------- |
|   [1]   | `num_beams`          | `double`     | beam count for beam-search decoding        |
|   [2]   | `length_penalty`     | `double`     | penalty applied to sequence length         |
|   [3]   | `repetition_penalty` | `double`     | penalty for repeated tokens                |
|   [4]   | `top_k`              | `double`     | top-K sampling limit                       |
|   [5]   | `top_p`              | `double`     | nucleus sampling probability mass          |
|   [6]   | `temperature`        | `double`     | logit temperature scaling                  |
|   [7]   | `do_sample`          | `bool`       | enables stochastic sampling                |
|   [8]   | `max_length`         | `double`     | maximum generated sequence length          |
|   [9]   | `min_length`         | `double`     | minimum generated sequence length          |
|  [10]   | `early_stopping`     | `bool`       | halts beam search when all beams reach EOS |

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
- `Model` admits both `Model(string modelPath)` (direct path) and `Model(Config config)` (config-driven); provider selection rides `Config.AppendProvider`/`SetProviderOption` before `new Model(config)`, never per generation.
- `Adapters : SafeHandle` — `ReleaseHandle` calls `OgaDestroyAdapters`; the managed GC boundary is `SafeHandle`, not `IDisposable`.

[GENERATION_LOOP]:
- Numeric search options pass as `double` and flags as `bool`; there is no string-valued `SetSearchOption` overload. `TrySetSearchOption` does not exist in this binary.
- Key strings are not validated at the managed boundary; an unrecognized key propagates to native and faults `OnnxRuntimeGenAIException` from the native layer.
- `GetSequence(ulong)` returns a `ReadOnlySpan<int>` view over native memory owned by the live `Generator`; the newest token (`sequence[^1]`) copies out before the next iteration and never retains past the loop.
- `GetNextTokens()` returns the most recently generated token span; distinct from `GetSequence(ulong)` which returns a full sequence view.
- `TokenizerStream` is obtained ONLY through `Tokenizer.CreateStream()`; `TokenizerStream` has no public constructor; `TokenizerStream.Decode(int)` takes a single `int`, not a `ReadOnlySpan<int>`.
- `Tokenizer.Decode(ReadOnlySpan<int>)` decodes a full span; `TokenizerStream.Decode(int)` decodes one token incrementally — these are distinct operations.
- `Tokenizer.ApplyChatTemplate` takes four arguments: `(string template_str, string messages, string tools, bool add_generation_prompt)`.
- The tool-call arm re-feeds typed results through `Generator.AppendTokens`/`AppendTokenSequences` or rewinds a partial turn through `RewindTo(ulong)`.
- LoRA hot-swap calls `Generator.SetActiveAdapter(Adapters adapters, string adapterName)` mid-generation; `Adapters` is loaded per-name via `LoadAdapter(string path, string name)`.

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
