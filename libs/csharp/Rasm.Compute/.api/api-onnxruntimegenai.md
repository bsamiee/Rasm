# [RASM_COMPUTE_API_ONNXRUNTIMEGENAI]

`Microsoft.ML.OnnxRuntimeGenAI` supplies the generative token-streaming runtime —
process-global init, model/config handles, tokenizer and chat-template assembly,
generator search options and structured-output guidance, the per-step token loop,
multimodal image/audio encoding via `MultiModalProcessor` and `StreamingProcessor`,
LoRA adapter hot-swap, GPU-device and native-log control via `Utils`, and the sole
generative fault rail; `Microsoft.Extensions.AI.Abstractions` supplies the `IChatClient`
contract that the built-in `OnnxRuntimeGenAIChatClient` composes over the same handle
chain for the M.E.AI streaming consumer. Both serve the Compute model rail's
`GENERATIVE_RUN` cluster; the generated tokens stage their byte buffers through the
`api-recyclable-stream` pool and the structured-output arm constrains generation in
the native layer rather than validating output managed-side. This page is HOST-LOCAL
and carries no TS_PROJECTION.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntimeGenAI`
- package: `Microsoft.ML.OnnxRuntimeGenAI` (direct pin; native meta-package)
- license: MIT (ONNX Runtime GenAI; nuspec `license type="file"`, `microsoft/onnxruntime-genai`)
- managed-assembly: transitive `Microsoft.ML.OnnxRuntimeGenAI.Managed` → the `net10.0` consumer binds `lib/net8.0/Microsoft.ML.OnnxRuntimeGenAI.dll` (the facade also ships `netstandard2.0` + `net9.0-{android,ios,maccatalyst}` variants; only `net8.0` is the bound desktop asset)
[NATIVE_FLOOR]:
The package depends on `Microsoft.ML.OnnxRuntime`; the central pin resolves it to the asset the inference engine `api-onnxruntime` owns. The genai native payload co-locates per-RID BESIDE the base `libonnxruntime` payload, and the per-RID base-runtime ABI matrix and EP/device roster stay owned by `api-onnxruntime#NATIVE_RUNTIME`, NOT restated here.
- native-rids: `linux-arm64`, `linux-x64`, `osx-arm64`, `win-arm64`, `win-x64` (+ `android`/`ios` archive forms) — the genai payload set, which is the subset of `api-onnxruntime`'s base-runtime RIDs that the genai meta-package also publishes; the `osx-arm64` runtime is the verified host asset, so a model run with no matching genai-AND-base RID payload faults at native init
- namespace: `Microsoft.ML.OnnxRuntimeGenAI`
- asset: native-only meta-package (`build/native` props/targets + `ort_genai.h`) plus the managed facade via the transitive `.Managed` package
- rail: model

[PACKAGE_SURFACE]: `Microsoft.Extensions.AI.Abstractions`
- package: `Microsoft.Extensions.AI.Abstractions` (direct pin)
- license: MIT (`dotnet/extensions`)
- assembly: `Microsoft.Extensions.AI.Abstractions` → the `net10.0` consumer binds `lib/net10.0`
- namespace: `Microsoft.Extensions.AI`
- asset: managed abstractions; the GenAI facade's transitive floor is but the central pin wins, so the `IChatClient` surface is the 10.x contract
- rail: model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handle chain and generation contracts
- rail: model

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]            | [CAPABILITY]                                           |
| :-----: | :-------------------------- | :------------------------ | :----------------------------------------------------- |
|  [01]   | `OgaHandle`                 | process-global handle     | owns native init/teardown (LIFO outermost)             |
|  [02]   | `Config`                    | model configuration       | configures model dir, providers, and model data        |
|  [03]   | `Model`                     | model root                | loads the generative model                             |
|  [04]   | `Tokenizer`                 | tokenizer root            | encodes prompts, applies chat template                 |
|  [05]   | `TokenizerStream`           | incremental decoder       | decodes one token per step                             |
|  [06]   | `Sequences`                 | token-sequence carrier    | carries encoded/generated token sequences              |
|  [07]   | `GeneratorParams`           | generation policy         | carries search options and guidance                    |
|  [08]   | `Generator`                 | generation engine         | runs the per-step token loop                           |
|  [09]   | `Adapters`                  | LoRA adapter registry     | loads/unloads named LoRA adapters (`SafeHandle`)       |
|  [10]   | `OnnxRuntimeGenAIException` | fault rail                | the sole generative exception type                     |
|  [11]   | `NamedTensors`              | named-tensor batch        | opaque batch of named tensors for multimodal injection |
|  [12]   | `Tensor`                    | native tensor carrier     | wraps a native buffer with shape and element type      |
|  [13]   | `ElementType`               | tensor element enum       | `enum ElementType : long` — ONNX element discriminant  |
|  [14]   | `Images`                    | image media loader        | loads images from paths or raw byte buffers            |
|  [15]   | `Audios`                    | audio media loader        | loads audios from paths or raw byte buffers            |
|  [16]   | `MultiModalProcessor`       | multimodal processor      | encodes image/audio/text batches into `NamedTensors`   |
|  [17]   | `StreamingProcessor`        | streaming audio processor | incremental audio chunking to `NamedTensors`           |
|  [18]   | `Utils`                     | static native control     | GPU device id + native log toggles (process-global)    |

[PUBLIC_TYPE_SCOPE]: M.E.AI projection
- rail: model
- note: `OnnxRuntimeGenAIChatClient` and `OnnxRuntimeGenAIChatClientOptions` are the only M.E.AI types defined IN the GenAI facade; `IChatClient`/`ChatResponse`/`ChatResponseUpdate`/`ChatMessage`/`ChatOptions` are owned by `Microsoft.Extensions.AI.Abstractions` (`api-extensions-ai`).

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                                                    |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------------------------------- |
|  [01]   | `OnnxRuntimeGenAIChatClient`      | `IChatClient` impl  | `sealed`; streaming chat over the GenAI handle chain, owns its `Model`/`Config` lifetime |
|  [02]   | `OnnxRuntimeGenAIChatClientOptions` | client policy     | `sealed`; `StopSequences`, `PromptFormatter`, `EnableCaching`  |
|  [03]   | `IChatClient`                     | M.E.AI contract     | response and streaming-response surface (abstractions package) |
|  [04]   | `ChatResponse`                    | M.E.AI response     | non-streaming response carrier (abstractions package)          |
|  [05]   | `ChatResponseUpdate`              | M.E.AI update       | streaming incremental update carrier (abstractions package)    |
|  [06]   | `ChatMessage` / `ChatOptions`     | M.E.AI message/opts | role-tagged message + per-call options (abstractions package)  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process and model lifecycle
- rail: model

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]                                                                                     | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `new OgaHandle()`                                      | `OgaHandle()`                                                                                    | process-global init (`IDisposable`)              |
|  [02]   | `new Config(string)`                                   | `Config(string modelPath)`                                                                       | reads `{modelPath}/genai_config.json`            |
|  [03]   | `Config.ClearProviders`                                | `void ClearProviders()`                                                                          | removes all configured providers                 |
|  [04]   | `Config.AppendProvider`                                | `void AppendProvider(string provider)`                                                           | injects an execution provider                    |
|  [05]   | `Config.SetProviderOption`                             | `void SetProviderOption(string provider, string option, string value)`                           | sets a provider option key                       |
|  [06]   | `Config.Overlay`                                       | `void Overlay(string json)`                                                                      | applies a JSON config overlay                    |
|  [07]   | `Config.AddModelData`                                  | `void AddModelData(string modelFilename, byte[] modelData)`                                      | injects in-memory model file data                |
|  [08]   | `Config.RemoveModelData`                               | `void RemoveModelData(string modelFilename)`                                                     | removes a previously added in-memory model file  |
|  [09]   | `Config.SetDecoderProviderOptionsHardwareDeviceType`   | `void SetDecoderProviderOptionsHardwareDeviceType(string provider, string hardware_device_type)` | sets hardware device type for decoder provider   |
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

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                               | [CAPABILITY]                           |
| :-----: | :---------------------------- | :--------------------------------------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `new Tokenizer(Model)`        | `Tokenizer(Model model)`                                                                                   | builds the tokenizer over a model      |
|  [02]   | `Tokenizer.CreateStream`      | `TokenizerStream CreateStream()`                                                                           | sole `TokenizerStream` source          |
|  [03]   | `Tokenizer.ApplyChatTemplate` | `string ApplyChatTemplate(string template_str, string messages, string tools, bool add_generation_prompt)` | assembles the prompt natively          |
|  [04]   | `Tokenizer.Encode`            | `Sequences Encode(string str)`                                                                             | encodes one string to a `Sequences`    |
|  [05]   | `Tokenizer.EncodeBatch`       | `Sequences EncodeBatch(string[] strings)`                                                                  | encodes a batch of strings             |
|  [06]   | `Tokenizer.Decode`            | `string Decode(ReadOnlySpan<int> sequence)`                                                                | decodes a full token span to string    |
|  [07]   | `Tokenizer.DecodeBatch`       | `string[] DecodeBatch(Sequences sequences)`                                                                | decodes all sequences in a `Sequences` |
|  [08]   | `Tokenizer.UpdateOptions`     | `void UpdateOptions(Dictionary<string, string> options)`                                                   | sets tokenizer option key-value pairs  |
|  [09]   | `Tokenizer.GetBosTokenId`     | `int GetBosTokenId()`                                                                                      | returns the beginning-of-sequence ID   |
|  [10]   | `Tokenizer.GetEosTokenIds`    | `ReadOnlySpan<int> GetEosTokenIds()`                                                                       | returns all end-of-sequence IDs        |
|  [11]   | `Tokenizer.GetPadTokenId`     | `int GetPadTokenId()`                                                                                      | returns the padding token ID           |
|  [12]   | `TokenizerStream.Decode`      | `string Decode(int token)`                                                                                 | decodes one token incrementally        |

[ENTRYPOINT_SCOPE]: Sequences token carrier
- rail: model

`Sequences` is created by `Tokenizer.Encode`/`EncodeBatch` or returned by `Generator.GetSequence`; direct construction is internal only.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                  | [CAPABILITY]                                         |
| :-----: | :----------------------- | :-------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Sequences.NumSequences` | `ulong NumSequences { get; }`                 | count of sequences in the batch                      |
|  [02]   | `Sequences[ulong]`       | `ReadOnlySpan<int> this[ulong sequenceIndex]` | span view over one sequence by index                 |
|  [03]   | `Sequences.Append`       | `void Append(int token, ulong sequenceIndex)` | appends one token to the sequence at `sequenceIndex` |

[ENTRYPOINT_SCOPE]: generation loop and search options
- rail: model

`SetSearchOption` admits numeric (`double`) and bool values only; `SetGuidance` carries type, data, and FFTokens policy. `GetSearchNumber`/`GetSearchBool` read back any previously-set option.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                              | [CAPABILITY]                           |
| :-----: | :--------------------- | :------------------------------------------------------------------------ | :------------------------------------- |
|  [01]   | `new GeneratorParams`  | `GeneratorParams(Model model)`                                            | builds generation params               |
|  [02]   | `SetSearchOption`      | `void SetSearchOption(string searchOption, double value)`                 | numeric search option                  |
|  [03]   | `SetSearchOption`      | `void SetSearchOption(string searchOption, bool value)`                   | flag search option                     |
|  [04]   | `GetSearchNumber`      | `double GetSearchNumber(string searchOption)`                             | reads back a numeric search option     |
|  [05]   | `GetSearchBool`        | `bool GetSearchBool(string searchOption)`                                 | reads back a bool search option        |
|  [06]   | `SetGuidance`          | `void SetGuidance(string type, string data, bool enableFFTokens = false)` | structured-output constraint           |
|  [07]   | `new Generator`        | `Generator(Model model, GeneratorParams generatorParams)`                 | builds the generator                   |
|  [08]   | `AppendTokenSequences` | `void AppendTokenSequences(Sequences sequences)`                          | seeds the prompt tokens                |
|  [09]   | `AppendTokens`         | `void AppendTokens(ReadOnlySpan<int> inputIDs)`                           | re-feeds token span                    |
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

`Adapters : SafeHandle`; created per `Model`, lives for the adapter set's lifetime.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                               | [CAPABILITY]                         |
| :-----: | :-------------------- | :--------------------------------------------------------- | :----------------------------------- |
|  [01]   | `new Adapters(Model)` | `Adapters(Model model)`                                    | creates the adapter registry         |
|  [02]   | `LoadAdapter`         | `void LoadAdapter(string adapterPath, string adapterName)` | loads a named LoRA adapter from path |
|  [03]   | `UnloadAdapter`       | `void UnloadAdapter(string adapterName)`                   | unloads a named LoRA adapter         |

[ENTRYPOINT_SCOPE]: GenAI Tensor and ElementType
- rail: model

`Tensor` in this namespace is `Microsoft.ML.OnnxRuntimeGenAI.Tensor` — a native-handle carrier distinct from `System.Numerics.Tensors.Tensor<T>`. It wraps an OGA-owned buffer with a shape and `ElementType` discriminant.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                        | [CAPABILITY]                                     |
| :-----: | :------------------------- | :-------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `new Tensor`               | `Tensor(nint data, long[] shape, ElementType type)` | wraps a raw pointer buffer as a named OGA tensor |
|  [02]   | `Tensor.Type`              | `ElementType Type()`                                | returns the element type discriminant            |
|  [03]   | `Tensor.Shape`             | `long[] Shape()`                                    | returns the dimension sizes                      |
|  [04]   | `Tensor.NumElements`       | `long NumElements()`                                | returns the total element count                  |
|  [05]   | `Tensor.GetData<T>`        | `ReadOnlySpan<T> GetData<T>()`                      | exposes the native buffer as a typed span        |
|  [06]   | `Tensor.ElementsFromShape` | `static long ElementsFromShape(long[] shape)`       | computes element count from a shape array        |

`ElementType` values: `undefined`, `float32`, `uint8`, `int8`, `uint16`, `int16`, `int32`, `int64`, `string_t`, `bool_t`, `float16`, `float64`, `uint32`, `uint64`.

[ENTRYPOINT_SCOPE]: image and audio media loaders
- rail: model

`Images` and `Audios` are `IDisposable`; both expose only static `Load` factories — no public constructor.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                | [CAPABILITY]                           |
| :-----: | :---------------------- | :------------------------------------------ | :------------------------------------- |
|  [01]   | `Images.Load(string[])` | `static Images Load(string[] imagePaths)`   | loads images from file paths           |
|  [02]   | `Images.Load(byte[])`   | `static Images Load(byte[] imageBytesData)` | loads one image from a raw byte buffer |
|  [03]   | `Audios.Load(string[])` | `static Audios Load(string[] audioPaths)`   | loads audios from file paths           |
|  [04]   | `Audios.Load(byte[])`   | `static Audios Load(byte[] audioBytesData)` | loads one audio from a raw byte buffer |

[ENTRYPOINT_SCOPE]: MultiModalProcessor
- rail: model

`MultiModalProcessor : IDisposable` is constructed per `Model`; it encodes prompt+media batches into `NamedTensors` that feed `Generator.SetInputs`. It also exposes a `CreateStream` factory mirroring `Tokenizer.CreateStream`.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                                                                          | [CAPABILITY]                                                |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `new MultiModalProcessor(Model)`             | `MultiModalProcessor(Model model)`                                                    | creates the processor for a model                           |
|  [02]   | `MultiModalProcessor.ProcessImages`          | `NamedTensors ProcessImages(string prompt, Images images)`                            | encodes one prompt + images into named tensors              |
|  [03]   | `MultiModalProcessor.ProcessImages`          | `NamedTensors ProcessImages(string[] prompts, Images images)`                         | encodes a batch of prompts + images into named tensors      |
|  [04]   | `MultiModalProcessor.ProcessAudios`          | `NamedTensors ProcessAudios(string prompt, Audios audios)`                            | encodes one prompt + audios into named tensors              |
|  [05]   | `MultiModalProcessor.ProcessAudios`          | `NamedTensors ProcessAudios(string[] prompts, Audios audios)`                         | encodes a batch of prompts + audios into named tensors      |
|  [06]   | `MultiModalProcessor.ProcessImagesAndAudios` | `NamedTensors ProcessImagesAndAudios(string prompt, Images images, Audios audios)`    | encodes one prompt + images + audios                        |
|  [07]   | `MultiModalProcessor.ProcessImagesAndAudios` | `NamedTensors ProcessImagesAndAudios(string[] prompts, Images images, Audios audios)` | encodes batch of prompts + images + audios                  |
|  [08]   | `MultiModalProcessor.Decode`                 | `string Decode(ReadOnlySpan<int> sequence)`                                           | decodes a full token span to string                         |
|  [09]   | `MultiModalProcessor.CreateStream`           | `TokenizerStream CreateStream()`                                                      | creates an incremental `TokenizerStream` from the processor |

[ENTRYPOINT_SCOPE]: StreamingProcessor
- rail: model

`StreamingProcessor : IDisposable` handles incremental audio chunk delivery. `Process` returns `null` until enough context is accumulated; `Flush` drains remaining state.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                               | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :----------------------------------------- | :---------------------------------------------------- |
|  [01]   | `new StreamingProcessor(Model)` | `StreamingProcessor(Model model)`          | creates the streaming processor for a model           |
|  [02]   | `StreamingProcessor.Process`    | `NamedTensors? Process(float[] audioData)` | feeds an audio chunk; returns tensors when ready      |
|  [03]   | `StreamingProcessor.Flush`      | `NamedTensors? Flush()`                    | drains remaining state; returns final tensors or null |
|  [04]   | `StreamingProcessor.SetOption`  | `void SetOption(string key, string value)` | sets a processor option                               |
|  [05]   | `StreamingProcessor.GetOption`  | `string GetOption(string key)`             | gets a processor option value                         |

[ENTRYPOINT_SCOPE]: recognized `SetSearchOption` key strings
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [KEY_STRING]         | [VALUE_TYPE] | [CAPABILITY]                               |
| :-----: | :------------------- | :----------- | :----------------------------------------- |
|  [01]   | `num_beams`          | `double`     | beam count for beam-search decoding        |
|  [02]   | `length_penalty`     | `double`     | penalty applied to sequence length         |
|  [03]   | `repetition_penalty` | `double`     | penalty for repeated tokens                |
|  [04]   | `top_k`              | `double`     | top-K sampling limit                       |
|  [05]   | `top_p`              | `double`     | nucleus sampling probability mass          |
|  [06]   | `temperature`        | `double`     | logit temperature scaling                  |
|  [07]   | `do_sample`          | `bool`       | enables stochastic sampling                |
|  [08]   | `max_length`         | `double`     | maximum generated sequence length          |
|  [09]   | `min_length`         | `double`     | minimum generated sequence length          |
|  [10]   | `early_stopping`     | `bool`       | halts beam search when all beams reach EOS |

[ENTRYPOINT_SCOPE]: GPU device and native-log control (`Utils`)
- rail: model
- note: `Utils` is a static process-global control surface; the GPU device id selects the CUDA/DML device before model load and the log toggles drive the native ORT logger.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                      | [CAPABILITY]                                  |
| :-----: | :------------------------------ | :------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `Utils.SetCurrentGpuDeviceId`   | `static void SetCurrentGpuDeviceId(int device_id)` | selects the active GPU device for model load  |
|  [02]   | `Utils.GetCurrentGpuDeviceId`   | `static int GetCurrentGpuDeviceId()`              | reads the active GPU device id                |
|  [03]   | `Utils.SetLogBool`              | `static void SetLogBool(string name, bool value)` | toggles a native ORT log flag                 |
|  [04]   | `Utils.SetLogString`            | `static void SetLogString(string name, string value)` | sets a native ORT log string option           |

[ENTRYPOINT_SCOPE]: M.E.AI chat client
- rail: model
- note: the ctor admits three handle forms with an `owns*` lifetime flag; the `string`/`Config` forms build the `Model` internally. The streaming/response/`GetService` surface comes from `IChatClient` in `api-extensions-ai`.

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                                                                       | [CAPABILITY]                                            |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `new OnnxRuntimeGenAIChatClient(string)` | `OnnxRuntimeGenAIChatClient(string modelPath, OnnxRuntimeGenAIChatClientOptions? options = null)` | builds the `Model` from a path and owns it             |
|  [02]   | `new OnnxRuntimeGenAIChatClient(Model)`  | `OnnxRuntimeGenAIChatClient(Model model, bool ownsModel = true, OnnxRuntimeGenAIChatClientOptions? options = null)` | wraps an existing `Model`; `ownsModel` gates disposal  |
|  [03]   | `new OnnxRuntimeGenAIChatClient(Config)` | `OnnxRuntimeGenAIChatClient(Config config, bool ownsConfig = true, OnnxRuntimeGenAIChatClientOptions? options = null)` | builds the `Model` from a `Config` (provider-tuned)    |
|  [04]   | `IChatClient.GetResponseAsync`          | `Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage>, ChatOptions? = null, CancellationToken = default)` | non-streaming response                                 |
|  [05]   | `IChatClient.GetStreamingResponseAsync` | `IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage>, ChatOptions? = null, CancellationToken = default)` | streaming response                                     |

[ENTRYPOINT_SCOPE]: `OnnxRuntimeGenAIChatClientOptions`
- rail: model
- note: the client-policy bag; `PromptFormatter` overrides the default chat-template assembly and `StopSequences`/`EnableCaching` tune the M.E.AI streaming loop.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                | [CAPABILITY]                                  |
| :-----: | :------------------------------ | :-------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `StopSequences`                 | `IList<string>? StopSequences { get; set; }`                                | halts generation on any listed sequence       |
|  [02]   | `PromptFormatter`               | `Func<IEnumerable<ChatMessage>, ChatOptions?, string>? PromptFormatter { get; set; }` | replaces the default chat-template assembly   |
|  [03]   | `EnableCaching`                 | `bool EnableCaching { get; set; }`                                          | reuses generator state across turns           |

## [04]-[IMPLEMENTATION_LAW]

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

[MEAI_PROJECTION]:
- `OnnxRuntimeGenAIChatClient` is `sealed : IChatClient, IDisposable`; the ctor admits `(string modelPath, …)`, `(Model, bool ownsModel = true, …)`, and `(Config, bool ownsConfig = true, …)` — there is NO `(Model, options)` two-arg form, and the `owns*` flag governs whether the client disposes the underlying handle.
- `OnnxRuntimeGenAIChatClientOptions` is the only policy bag — `StopSequences`/`PromptFormatter`/`EnableCaching`; `PromptFormatter` (`Func<IEnumerable<ChatMessage>, ChatOptions?, string>?`) is the override seam for `Tokenizer.ApplyChatTemplate` when the M.E.AI consumer owns prompt assembly.
- `IChatClient`/`ChatResponse`/`ChatResponseUpdate`/`ChatMessage`/`ChatOptions` resolve from `api-extensions-ai`; the GenAI facade defines only the client and its options, so a second managed chat-message model beside `ChatMessage` is the rejected form.

[NATIVE_CONTROL]:
- `Utils` is a static process-global control: `SetCurrentGpuDeviceId(int)` selects the device BEFORE model load (provider device selection is `Config.SetDecoderProviderOptionsHardwareDeviceId` per-decoder; `Utils` is the global default), and `SetLogBool`/`SetLogString` drive the native ORT logger — there is no managed log abstraction over them.
- The native floor is `Microsoft.ML.OnnxRuntime >= 1.23.0` (central-pinned to `1.27.0`); the genai runtime composes the inference engine `api-onnxruntime` owns. The provider strings `Config.AppendProvider` accepts, the option keys `Config.SetProviderOption` sets, and the device selectors `Config.SetDecoderProviderOptionsHardware{DeviceType,DeviceId,VendorId}` bind are the EP roster + `OrtHardwareDevice` (`Type`/`VendorId`/`DeviceId`) discovery owned by `api-onnxruntime#EXECUTION_PROVIDER_SELECTION`; the per-RID native payload availability (which accelerated provider exists on which RID) is owned by `api-onnxruntime`'s `[PACKAGE_ASSET_SCOPE]: per-RID native ABI matrix`. This lane restates neither — a decoder provider selected here that has no native payload on the host RID faults at native init.

[STAGING_INTEGRATION]:
- Token/tensor byte buffers stage through the `api-recyclable-stream` pool, not ad hoc arrays; `Tensor.GetData<T>()` and `GetSequence(ulong)` return native-backed `ReadOnlySpan<T>` views that copy into a rented stream only at the edge.
- A structured generative artifact crossing the wire is a `Runtime/wire#PROTO_VOCABULARY` `api-protobuf` message, not a managed DTO; the model-rejection fault lifts to `ComputeFault.ModelRejected` at the boundary.

[LOCAL_ADMISSION]:
- Token-streaming is a run mode on the owned model lane; a `GenerativeService`, `ChatClient`, `Conversation`, or `PromptService` wrapper is the rejected form.
- The built-in `OnnxRuntimeGenAIChatClient : IChatClient` composes the same handle chain when the M.E.AI streaming abstraction is the consumer — it is the admitted projection, never a hand-rolled one.
- Grammar-constrained structured output is enforced at generation through `SetGuidance`; a managed JSON-schema validator over the output is the rejected form.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntimeGenAI` (MIT, native + `.Managed` net8.0) + `Microsoft.Extensions.AI.Abstractions` (MIT, net10.0)
- Owns: generative token-streaming runtime, multimodal encoding, streaming audio, GPU-device/native-log control via `Utils`, and the `OnnxRuntimeGenAIChatClient` `IChatClient` projection
- Accept: model-dir generative runs over the LIFO handle chain; multimodal `Images`/`Audios` + `MultiModalProcessor` pipelines; incremental `StreamingProcessor` audio paths; M.E.AI streaming via the three admitted ctors stacked onto the `api-recyclable-stream` staging pool
- Reject: chat-client/conversation/prompt service families; managed output validators; a second managed chat-message model beside `ChatMessage`; the phantom `(Model, options)` ctor; `System.Numerics.Tensors.Tensor<T>` confused with `Microsoft.ML.OnnxRuntimeGenAI.Tensor`; a model run with no matching native RID payload
