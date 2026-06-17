# [RASM_COMPUTE_API_ONNXRUNTIMEGENAI]

`Microsoft.ML.OnnxRuntimeGenAI` supplies the generative token-streaming runtime —
process-global init, model/config handles, tokenizer and chat-template assembly,
generator search options and structured-output guidance, the per-step token loop,
multimodal image/audio encoding via `MultiModalProcessor` and `StreamingProcessor`,
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

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]            | [CAPABILITY]                                           |
| :-----: | :-------------------------- | :------------------------ | :----------------------------------------------------- |
|   [1]   | `OgaHandle`                 | process-global handle     | owns native init/teardown (LIFO outermost)             |
|   [2]   | `Config`                    | model configuration       | configures model dir, providers, and model data        |
|   [3]   | `Model`                     | model root                | loads the generative model                             |
|   [4]   | `Tokenizer`                 | tokenizer root            | encodes prompts, applies chat template                 |
|   [5]   | `TokenizerStream`           | incremental decoder       | decodes one token per step                             |
|   [6]   | `Sequences`                 | token-sequence carrier    | carries encoded/generated token sequences              |
|   [7]   | `GeneratorParams`           | generation policy         | carries search options and guidance                    |
|   [8]   | `Generator`                 | generation engine         | runs the per-step token loop                           |
|   [9]   | `Adapters`                  | LoRA adapter registry     | loads/unloads named LoRA adapters (`SafeHandle`)       |
|  [10]   | `OnnxRuntimeGenAIException` | fault rail                | the sole generative exception type                     |
|  [11]   | `NamedTensors`              | named-tensor batch        | opaque batch of named tensors for multimodal injection |
|  [12]   | `Tensor`                    | native tensor carrier     | wraps a native buffer with shape and element type      |
|  [13]   | `ElementType`               | tensor element enum       | discriminates ONNX element types (float32, int32, …)   |
|  [14]   | `Images`                    | image media loader        | loads images from paths or raw byte buffers            |
|  [15]   | `Audios`                    | audio media loader        | loads audios from paths or raw byte buffers            |
|  [16]   | `MultiModalProcessor`       | multimodal processor      | encodes image/audio/text batches into `NamedTensors`   |
|  [17]   | `StreamingProcessor`        | streaming audio processor | incremental audio chunking to `NamedTensors`           |

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

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]                                                                                     | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|   [1]   | `new OgaHandle()`                                      | `OgaHandle()`                                                                                    | process-global init (`IDisposable`)              |
|   [2]   | `new Config(string)`                                   | `Config(string modelPath)`                                                                       | reads `{modelPath}/genai_config.json`            |
|   [3]   | `Config.ClearProviders`                                | `void ClearProviders()`                                                                          | removes all configured providers                 |
|   [4]   | `Config.AppendProvider`                                | `void AppendProvider(string provider)`                                                           | injects an execution provider                    |
|   [5]   | `Config.SetProviderOption`                             | `void SetProviderOption(string provider, string option, string value)`                           | sets a provider option key                       |
|   [6]   | `Config.Overlay`                                       | `void Overlay(string json)`                                                                      | applies a JSON config overlay                    |
|   [7]   | `Config.AddModelData`                                  | `void AddModelData(string modelFilename, byte[] modelData)`                                      | injects in-memory model file data                |
|   [8]   | `Config.RemoveModelData`                               | `void RemoveModelData(string modelFilename)`                                                     | removes a previously added in-memory model file  |
|   [9]   | `Config.SetDecoderProviderOptionsHardwareDeviceType`   | `void SetDecoderProviderOptionsHardwareDeviceType(string provider, string hardware_device_type)` | sets hardware device type for decoder provider   |
|  [10]   | `Config.SetDecoderProviderOptionsHardwareDeviceId`     | `void SetDecoderProviderOptionsHardwareDeviceId(string provider, uint hardware_device_id)`       | sets hardware device ID for decoder provider     |
|  [11]   | `Config.SetDecoderProviderOptionsHardwareVendorId`     | `void SetDecoderProviderOptionsHardwareVendorId(string provider, uint hardware_vendor_id)`       | sets hardware vendor ID for decoder provider     |
|  [12]   | `Config.ClearDecoderProviderOptionsHardwareDeviceType` | `void ClearDecoderProviderOptionsHardwareDeviceType(string provider)`                            | clears hardware device type for decoder provider |
|  [13]   | `Config.ClearDecoderProviderOptionsHardwareDeviceId`   | `void ClearDecoderProviderOptionsHardwareDeviceId(string provider)`                              | clears hardware device ID for decoder provider   |
|  [14]   | `Config.ClearDecoderProviderOptionsHardwareVendorId`   | `void ClearDecoderProviderOptionsHardwareVendorId(string provider)`                              | clears hardware vendor ID for decoder provider   |
|  [15]   | `new Model(string)`                                    | `Model(string modelPath)`                                                                        | loads the model directly from path               |
|  [16]   | `new Model(Config)`                                    | `Model(Config config)`                                                                           | loads the model from config                      |
|  [17]   | `Model.GetModelType`                                   | `string GetModelType()`                                                                          | reports the model type string                    |

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
|   [8]   | `Tokenizer.UpdateOptions`     | `void UpdateOptions(Dictionary<string, string> options)`                                                   | sets tokenizer option key-value pairs  |
|   [9]   | `Tokenizer.GetBosTokenId`     | `int GetBosTokenId()`                                                                                      | returns the beginning-of-sequence ID   |
|  [10]   | `Tokenizer.GetEosTokenIds`    | `ReadOnlySpan<int> GetEosTokenIds()`                                                                       | returns all end-of-sequence IDs        |
|  [11]   | `Tokenizer.GetPadTokenId`     | `int GetPadTokenId()`                                                                                      | returns the padding token ID           |
|  [12]   | `TokenizerStream.Decode`      | `string Decode(int token)`                                                                                 | decodes one token incrementally        |

[ENTRYPOINT_SCOPE]: Sequences token carrier
- rail: model
- source: decompile-verified

`Sequences` is created by `Tokenizer.Encode`/`EncodeBatch` or returned by `Generator.GetSequence`; direct construction is internal only.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                  | [CAPABILITY]                                         |
| :-----: | :----------------------- | :-------------------------------------------- | :--------------------------------------------------- |
|   [1]   | `Sequences.NumSequences` | `ulong NumSequences { get; }`                 | count of sequences in the batch                      |
|   [2]   | `Sequences[ulong]`       | `ReadOnlySpan<int> this[ulong sequenceIndex]` | span view over one sequence by index                 |
|   [3]   | `Sequences.Append`       | `void Append(int token, ulong sequenceIndex)` | appends one token to the sequence at `sequenceIndex` |

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

[ENTRYPOINT_SCOPE]: GenAI Tensor and ElementType
- rail: model
- source: decompile-verified

`Tensor` in this namespace is `Microsoft.ML.OnnxRuntimeGenAI.Tensor` — a native-handle carrier distinct from `System.Numerics.Tensors.Tensor<T>`. It wraps an OGA-owned buffer with a shape and `ElementType` discriminant.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                        | [CAPABILITY]                                     |
| :-----: | :------------------------- | :-------------------------------------------------- | :----------------------------------------------- |
|   [1]   | `new Tensor`               | `Tensor(nint data, long[] shape, ElementType type)` | wraps a raw pointer buffer as a named OGA tensor |
|   [2]   | `Tensor.Type`              | `ElementType Type()`                                | returns the element type discriminant            |
|   [3]   | `Tensor.Shape`             | `long[] Shape()`                                    | returns the dimension sizes                      |
|   [4]   | `Tensor.NumElements`       | `long NumElements()`                                | returns the total element count                  |
|   [5]   | `Tensor.GetData<T>`        | `ReadOnlySpan<T> GetData<T>()`                      | exposes the native buffer as a typed span        |
|   [6]   | `Tensor.ElementsFromShape` | `static long ElementsFromShape(long[] shape)`       | computes element count from a shape array        |

`ElementType` values: `undefined`, `float32`, `uint8`, `int8`, `uint16`, `int16`, `int32`, `int64`, `string_t`, `bool_t`, `float16`, `float64`, `uint32`, `uint64`.

[ENTRYPOINT_SCOPE]: image and audio media loaders
- rail: model
- source: decompile-verified

`Images` and `Audios` are `IDisposable`; both expose only static `Load` factories — no public constructor.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                | [CAPABILITY]                           |
| :-----: | :---------------------- | :------------------------------------------ | :------------------------------------- |
|   [1]   | `Images.Load(string[])` | `static Images Load(string[] imagePaths)`   | loads images from file paths           |
|   [2]   | `Images.Load(byte[])`   | `static Images Load(byte[] imageBytesData)` | loads one image from a raw byte buffer |
|   [3]   | `Audios.Load(string[])` | `static Audios Load(string[] audioPaths)`   | loads audios from file paths           |
|   [4]   | `Audios.Load(byte[])`   | `static Audios Load(byte[] audioBytesData)` | loads one audio from a raw byte buffer |

[ENTRYPOINT_SCOPE]: MultiModalProcessor
- rail: model
- source: decompile-verified

`MultiModalProcessor : IDisposable` is constructed per `Model`; it encodes prompt+media batches into `NamedTensors` that feed `Generator.SetInputs`. It also exposes a `CreateStream` factory mirroring `Tokenizer.CreateStream`.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                                                                          | [CAPABILITY]                                                |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------ | :---------------------------------------------------------- |
|   [1]   | `new MultiModalProcessor(Model)`             | `MultiModalProcessor(Model model)`                                                    | creates the processor for a model                           |
|   [2]   | `MultiModalProcessor.ProcessImages`          | `NamedTensors ProcessImages(string prompt, Images images)`                            | encodes one prompt + images into named tensors              |
|   [3]   | `MultiModalProcessor.ProcessImages`          | `NamedTensors ProcessImages(string[] prompts, Images images)`                         | encodes a batch of prompts + images into named tensors      |
|   [4]   | `MultiModalProcessor.ProcessAudios`          | `NamedTensors ProcessAudios(string prompt, Audios audios)`                            | encodes one prompt + audios into named tensors              |
|   [5]   | `MultiModalProcessor.ProcessAudios`          | `NamedTensors ProcessAudios(string[] prompts, Audios audios)`                         | encodes a batch of prompts + audios into named tensors      |
|   [6]   | `MultiModalProcessor.ProcessImagesAndAudios` | `NamedTensors ProcessImagesAndAudios(string prompt, Images images, Audios audios)`    | encodes one prompt + images + audios                        |
|   [7]   | `MultiModalProcessor.ProcessImagesAndAudios` | `NamedTensors ProcessImagesAndAudios(string[] prompts, Images images, Audios audios)` | encodes batch of prompts + images + audios                  |
|   [8]   | `MultiModalProcessor.Decode`                 | `string Decode(ReadOnlySpan<int> sequence)`                                           | decodes a full token span to string                         |
|   [9]   | `MultiModalProcessor.CreateStream`           | `TokenizerStream CreateStream()`                                                      | creates an incremental `TokenizerStream` from the processor |

[ENTRYPOINT_SCOPE]: StreamingProcessor
- rail: model
- source: decompile-verified

`StreamingProcessor : IDisposable` handles incremental audio chunk delivery. `Process` returns `null` until enough context is accumulated; `Flush` drains remaining state.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                               | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :----------------------------------------- | :---------------------------------------------------- |
|   [1]   | `new StreamingProcessor(Model)` | `StreamingProcessor(Model model)`          | creates the streaming processor for a model           |
|   [2]   | `StreamingProcessor.Process`    | `NamedTensors? Process(float[] audioData)` | feeds an audio chunk; returns tensors when ready      |
|   [3]   | `StreamingProcessor.Flush`      | `NamedTensors? Flush()`                    | drains remaining state; returns final tensors or null |
|   [4]   | `StreamingProcessor.SetOption`  | `void SetOption(string key, string value)` | sets a processor option                               |
|   [5]   | `StreamingProcessor.GetOption`  | `string GetOption(string key)`             | gets a processor option value                         |

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

[MULTIMODAL_LAW]:
- `Images.Load` and `Audios.Load` have no public constructor; both are produced via the static factory family only.
- `MultiModalProcessor` is the encoding path for vision/audio models; it produces `NamedTensors` that feed `Generator.SetInputs`.
- `StreamingProcessor` handles incremental audio: `Process(float[])` returns `null` until a VAD boundary; `Flush()` drains the final fragment.
- `Tensor` in this namespace is `Microsoft.ML.OnnxRuntimeGenAI.Tensor`; qualify on any boundary where `System.Numerics.Tensors.Tensor<T>` also appears.
- `GetData<T>()` returns a `ReadOnlySpan<T>` backed by native memory owned by the live `Tensor`; copy out before disposal.

[FAULT_LAW]:
- `OnnxRuntimeGenAIException` is the sole fault rail at `Model`/`Config` construction and across the generation loop; it lifts to `ComputeFault.ModelRejected` at the boundary, never a per-call catch.

[LOCAL_ADMISSION]:
- Token-streaming is a run mode on the owned model lane; a `GenerativeService`, `ChatClient`, `Conversation`, or `PromptService` wrapper is the rejected form.
- The built-in `OnnxRuntimeGenAIChatClient : IChatClient` composes the same handle chain when the M.E.AI streaming abstraction is the consumer.
- Grammar-constrained structured output is enforced at generation through `SetGuidance`; a managed JSON-schema validator over the output is the rejected form.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntimeGenAI` + `Microsoft.Extensions.AI.Abstractions`
- Owns: generative token-streaming runtime, multimodal encoding, streaming audio, and the `IChatClient` projection
- Accept: model-dir generative runs over the LIFO handle chain; multimodal `Images`/`Audios` + `MultiModalProcessor` pipelines; incremental `StreamingProcessor` audio paths
- Reject: chat-client/conversation/prompt service families; managed output validators; `System.Numerics.Tensors.Tensor<T>` confused with `Microsoft.ML.OnnxRuntimeGenAI.Tensor`
