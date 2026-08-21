# [RASM_COMPUTE_API_ONNXRUNTIMEGENAI]

`Microsoft.ML.OnnxRuntimeGenAI` owns the Compute model rail's generative token-streaming runtime over a LIFO native-handle chain, and `Microsoft.Extensions.AI.Abstractions` supplies the `IChatClient` contract the built-in `OnnxRuntimeGenAIChatClient` composes over that same chain for the M.E.AI streaming consumer. Structured output constrains in the native layer through `SetGuidance` rather than managed-side, generated token buffers stage through the `api-recyclable-stream` pool, and both feed the model rail's `GENERATIVE_RUN` cluster. This page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntimeGenAI`
- package: `Microsoft.ML.OnnxRuntimeGenAI` (MIT)
- assembly: transitive `Microsoft.ML.OnnxRuntimeGenAI.Managed`; the `net10.0` consumer binds `lib/net8.0/Microsoft.ML.OnnxRuntimeGenAI.dll`
- namespace: `Microsoft.ML.OnnxRuntimeGenAI`
- asset: native-only meta-package (`build/native` props/targets, `ort_genai.h`) with the managed facade
- depends: `Microsoft.ML.OnnxRuntime` — the genai native payload co-locates per-RID beside the base `libonnxruntime` payload; `api-onnxruntime` owns the base ABI matrix and EP roster
- depends: `Microsoft.Extensions.AI.Abstractions` — the managed facade's `OnnxRuntimeGenAIChatClient` implements its `IChatClient`; `libs/csharp/.api/api-extensions-ai.md` owns that contract surface whole
- rail: model
- verification: the package resolves NO assay `--key` (probe: `api query Generator` returns `status:unsupported`), exactly as the base `api-onnxruntime` key does. The MANAGED surface verifies by IL decompile of `Microsoft.ML.OnnxRuntimeGenAI.dll`; the managed layer holds no vocabulary at all — every `SetSearchOption` and `SetGuidance` string marshals straight through — so the NATIVE vocabularies verify only against the shipped `libonnxruntime-genai` payload's own parser, disassembled, and the string-typed key and guidance rosters below carry that evidence per table

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: handle chain and generation contracts

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `OgaHandle`                 | class         | native init/teardown, LIFO outermost                    |
|  [02]   | `Config`                    | class         | model dir, providers, and in-memory model data          |
|  [03]   | `Model`                     | class         | loads the generative model                              |
|  [04]   | `Tokenizer`                 | class         | encodes prompts, applies the chat template              |
|  [05]   | `TokenizerStream`           | class         | decodes one token per step                              |
|  [06]   | `Sequences`                 | class         | carries encoded/generated token sequences               |
|  [07]   | `GeneratorParams`           | class         | search options and structured-output guidance           |
|  [08]   | `Generator`                 | class         | runs the per-step token loop                            |
|  [09]   | `Adapters`                  | class         | `SafeHandle` registry of named LoRA adapters            |
|  [10]   | `OnnxRuntimeGenAIException` | class         | sole generative fault rail                              |
|  [11]   | `NamedTensors`              | class         | opaque named-tensor batch for multimodal injection      |
|  [12]   | `Tensor`                    | class         | native buffer with shape and `ElementType`              |
|  [13]   | `ElementType`               | enum          | `enum ElementType : long` ONNX element discriminant     |
|  [14]   | `Images`                    | class         | loads images from paths or byte buffers                 |
|  [15]   | `Audios`                    | class         | loads audios from paths or byte buffers                 |
|  [16]   | `MultiModalProcessor`       | class         | encodes image/audio/text batches into `NamedTensors`    |
|  [17]   | `StreamingProcessor`        | class         | incremental audio chunking to `NamedTensors`            |
|  [18]   | `Utils`                     | class         | static process-global GPU-device and native-log control |

[PUBLIC_TYPE_SCOPE]: M.E.AI projection
- note: the genai facade defines only these two types; `IChatClient`, `ChatResponse`, `ChatResponseUpdate`, `ChatMessage`, and `ChatOptions` resolve from `api-extensions-ai`.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :---------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `OnnxRuntimeGenAIChatClient`        | class         | `sealed : IChatClient`; streaming chat over an owned `Model` |
|  [02]   | `OnnxRuntimeGenAIChatClientOptions` | class         | `StopSequences`, `PromptFormatter`, `EnableCaching` policy   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process and model lifecycle

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new OgaHandle()`                                                    | ctor     | process-global init (`IDisposable`)   |
|  [02]   | `new Config(string)`                                                 | ctor     | reads `{modelPath}/genai_config.json` |
|  [03]   | `Config.ClearProviders()`                                            | instance | removes all configured providers      |
|  [04]   | `Config.AppendProvider(string)`                                      | instance | injects an execution provider         |
|  [05]   | `Config.SetProviderOption(string, string, string)`                   | instance | sets a provider option key            |
|  [06]   | `Config.Overlay(string)`                                             | instance | applies a JSON config overlay         |
|  [07]   | `Config.AddModelData(string, byte[])`                                | instance | injects in-memory model file data     |
|  [08]   | `Config.RemoveModelData(string)`                                     | instance | removes added in-memory model data    |
|  [09]   | `Config.SetDecoderProviderOptionsHardwareDeviceType(string, string)` | instance | sets decoder hardware device type     |
|  [10]   | `Config.SetDecoderProviderOptionsHardwareDeviceId(string, uint)`     | instance | sets decoder hardware device id       |
|  [11]   | `Config.SetDecoderProviderOptionsHardwareVendorId(string, uint)`     | instance | sets decoder hardware vendor id       |
|  [12]   | `Config.ClearDecoderProviderOptionsHardwareDeviceType(string)`       | instance | clears decoder hardware device type   |
|  [13]   | `Config.ClearDecoderProviderOptionsHardwareDeviceId(string)`         | instance | clears decoder hardware device id     |
|  [14]   | `Config.ClearDecoderProviderOptionsHardwareVendorId(string)`         | instance | clears decoder hardware vendor id     |
|  [15]   | `new Model(string)`                                                  | ctor     | loads the model from a path           |
|  [16]   | `new Model(Config)`                                                  | ctor     | loads the model from a config         |
|  [17]   | `Model.GetModelType() -> string`                                     | instance | reports the model type string         |

[ENTRYPOINT_SCOPE]: tokenization and chat template

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `new Tokenizer(Model)`                                                | ctor     | builds the tokenizer            |
|  [02]   | `Tokenizer.CreateStream() -> TokenizerStream`                         | instance | sole `TokenizerStream` source   |
|  [03]   | `Tokenizer.ApplyChatTemplate(string, string, string, bool) -> string` | instance | assembles the prompt natively   |
|  [04]   | `Tokenizer.Encode(string) -> Sequences`                               | instance | encodes one string              |
|  [05]   | `Tokenizer.EncodeBatch(string[]) -> Sequences`                        | instance | encodes a batch of strings      |
|  [06]   | `Tokenizer.Decode(ReadOnlySpan<int>) -> string`                       | instance | decodes a full token span       |
|  [07]   | `Tokenizer.DecodeBatch(Sequences) -> string[]`                        | instance | decodes all sequences           |
|  [08]   | `Tokenizer.UpdateOptions(Dictionary<string, string>)`                 | instance | sets tokenizer option pairs     |
|  [09]   | `Tokenizer.GetBosTokenId() -> int`                                    | instance | beginning-of-sequence id        |
|  [10]   | `Tokenizer.GetEosTokenIds() -> ReadOnlySpan<int>`                     | instance | all end-of-sequence ids         |
|  [11]   | `Tokenizer.GetPadTokenId() -> int`                                    | instance | padding token id                |
|  [12]   | `Tokenizer.GetBotTokenId() -> int`                                    | instance | beginning-of-text id            |
|  [13]   | `Tokenizer.GetEotTokenId() -> int`                                    | instance | end-of-turn id                  |
|  [14]   | `Tokenizer.GetBorTokenId() -> int`                                    | instance | beginning-of-response id        |
|  [15]   | `Tokenizer.GetEorTokenId() -> int`                                    | instance | end-of-response id              |
|  [16]   | `TokenizerStream.Decode(int) -> string`                               | instance | decodes one token incrementally |

- Rows [12]-[15] THROW when the model defines no such id, so each is a probe rather than a read; `GetBosTokenId`, `GetEosTokenIds`, and `GetPadTokenId` answer for every model.

[ENTRYPOINT_SCOPE]: Sequences token carrier
- note: created by `Tokenizer.Encode`/`EncodeBatch` or `Generator.GetSequence`; direct construction is internal.

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Sequences.NumSequences -> ulong`       | property | count of sequences in the batch      |
|  [02]   | `Sequences[ulong] -> ReadOnlySpan<int>` | property | span view over one sequence by index |
|  [03]   | `Sequences.Append(int, ulong)`          | instance | appends one token at `sequenceIndex` |

[ENTRYPOINT_SCOPE]: generation loop and search options

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `new GeneratorParams(Model)`                        | ctor     | builds generation params           |
|  [02]   | `GeneratorParams.SetSearchOption(string, double)`   | instance | numeric search option              |
|  [03]   | `GeneratorParams.SetSearchOption(string, bool)`     | instance | flag search option                 |
|  [04]   | `GeneratorParams.GetSearchNumber(string) -> double` | instance | reads a numeric option             |
|  [05]   | `GeneratorParams.GetSearchBool(string) -> bool`     | instance | reads a bool option                |
|  [06]   | `GeneratorParams.SetGuidance(string, string, bool)` | instance | structured-output constraint       |
|  [07]   | `new Generator(Model, GeneratorParams)`             | ctor     | builds the generator               |
|  [08]   | `Generator.AppendTokenSequences(Sequences)`         | instance | seeds prompt tokens                |
|  [09]   | `Generator.AppendTokens(ReadOnlySpan<int>)`         | instance | re-feeds a token span              |
|  [10]   | `Generator.RewindTo(ulong)`                         | instance | rewinds sequence to a length       |
|  [11]   | `Generator.GenerateNextToken()`                     | instance | advances one step                  |
|  [12]   | `Generator.IsDone() -> bool`                        | instance | terminates generation              |
|  [13]   | `Generator.GetSequence(ulong) -> ReadOnlySpan<int>` | instance | view over native token memory      |
|  [14]   | `Generator.GetNextTokens() -> ReadOnlySpan<int>`    | instance | most-recently generated tokens     |
|  [15]   | `Generator.TokenCount() -> ulong`                   | instance | current sequence token count       |
|  [16]   | `Generator.SetActiveAdapter(Adapters, string)`      | instance | hot-swaps the active LoRA adapter  |
|  [17]   | `Generator.SetModelInput(string, Tensor)`           | instance | injects a named model input tensor |
|  [18]   | `Generator.SetInputs(NamedTensors)`                 | instance | injects a named-tensor batch       |
|  [19]   | `Generator.GetInput(string) -> Tensor`              | instance | retrieves a named input tensor     |
|  [20]   | `Generator.GetOutput(string) -> Tensor`             | instance | retrieves a named output tensor    |
|  [21]   | `Generator.SetRuntimeOption(string, string)`        | instance | sets a generator runtime option    |

[ENTRYPOINT_SCOPE]: LoRA adapter management
- note: `Adapters : SafeHandle`, created per `Model` and living for the adapter set's lifetime.

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `new Adapters(Model)`                  | ctor     | creates the adapter registry         |
|  [02]   | `Adapters.LoadAdapter(string, string)` | instance | loads a named LoRA adapter from path |
|  [03]   | `Adapters.UnloadAdapter(string)`       | instance | unloads a named LoRA adapter         |

[ENTRYPOINT_SCOPE]: GenAI Tensor and ElementType
- note: `Tensor` here is `Microsoft.ML.OnnxRuntimeGenAI.Tensor`, an OGA-owned buffer distinct from `System.Numerics.Tensors.Tensor<T>`.

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `new Tensor(nint, long[], ElementType)`    | ctor     | wraps a raw pointer buffer as a tensor |
|  [02]   | `Tensor.Type() -> ElementType`             | instance | element type discriminant              |
|  [03]   | `Tensor.Shape() -> long[]`                 | instance | dimension sizes                        |
|  [04]   | `Tensor.NumElements() -> long`             | instance | total element count                    |
|  [05]   | `Tensor.GetData<T>() -> ReadOnlySpan<T>`   | instance | native buffer as a typed span          |
|  [06]   | `Tensor.ElementsFromShape(long[]) -> long` | static   | element count from a shape array       |

`ElementType`: `undefined` `float32` `uint8` `int8` `uint16` `int16` `int32` `int64` `string_t` `bool_t` `float16` `float64` `uint32` `uint64`

[ENTRYPOINT_SCOPE]: image and audio media loaders
- note: `Images` and `Audios` are `IDisposable` with static `Load` factories only, no public constructor.

| [INDEX] | [SURFACE]                         | [SHAPE] | [CAPABILITY]                       |
| :-----: | :-------------------------------- | :------ | :--------------------------------- |
|  [01]   | `Images.Load(string[]) -> Images` | static  | loads images from file paths       |
|  [02]   | `Images.Load(byte[]) -> Images`   | static  | loads one image from a byte buffer |
|  [03]   | `Audios.Load(string[]) -> Audios` | static  | loads audios from file paths       |
|  [04]   | `Audios.Load(byte[]) -> Audios`   | static  | loads one audio from a byte buffer |

[ENTRYPOINT_SCOPE]: MultiModalProcessor
- note: `MultiModalProcessor : IDisposable`, built per `Model`; every `Process*` returns `NamedTensors` that feed `Generator.SetInputs`.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new MultiModalProcessor(Model)`                                       | ctor     | creates the processor                 |
|  [02]   | `MultiModalProcessor.ProcessImages(string, Images)`                    | instance | one prompt + images                   |
|  [03]   | `MultiModalProcessor.ProcessImages(string[], Images)`                  | instance | batch prompts + images                |
|  [04]   | `MultiModalProcessor.ProcessAudios(string, Audios)`                    | instance | one prompt + audios                   |
|  [05]   | `MultiModalProcessor.ProcessAudios(string[], Audios)`                  | instance | batch prompts + audios                |
|  [06]   | `MultiModalProcessor.ProcessImagesAndAudios(string, Images, Audios)`   | instance | one prompt + images + audios          |
|  [07]   | `MultiModalProcessor.ProcessImagesAndAudios(string[], Images, Audios)` | instance | batch + images + audios               |
|  [08]   | `MultiModalProcessor.Decode(ReadOnlySpan<int>) -> string`              | instance | decodes a full token span             |
|  [09]   | `MultiModalProcessor.CreateStream() -> TokenizerStream`                | instance | incremental stream from the processor |

[ENTRYPOINT_SCOPE]: StreamingProcessor
- note: `StreamingProcessor : IDisposable`; `Process` returns `null` until enough context accumulates, `Flush` drains remaining state.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new StreamingProcessor(Model)`                        | ctor     | creates the streaming processor   |
|  [02]   | `StreamingProcessor.Process(float[]) -> NamedTensors?` | instance | feeds a chunk; tensors when ready |
|  [03]   | `StreamingProcessor.Flush() -> NamedTensors?`          | instance | drains remaining state            |
|  [04]   | `StreamingProcessor.SetOption(string, string)`         | instance | sets a processor option           |
|  [05]   | `StreamingProcessor.GetOption(string) -> string`       | instance | gets a processor option value     |

[ENTRYPOINT_SCOPE]: recognized `SetSearchOption` key strings — COMPLETE
- note: both overloads funnel into ONE native parser (`Generators::SetSearchNumber` and `SetSearchBool` each tag a variant and call `Generators::Search_Element::OnValue`), which is the same element that parses the `search` block of `genai_config.json`. The roster is therefore shared: a key is not numeric-only or bool-only at dispatch, and the VALUE type is checked afterward by the native `JSON::Get<double>`/`JSON::Get<bool>` selector the table's `[VALUE_TYPE]` column names. Defaults are the runtime's own, not a consumer's policy seed.
- evidence: disassembly of the shipped `libonnxruntime-genai` native payload for the host RID, corroborated 18-for-18 against the upstream `struct Search` at the matching source tag. Half the keys carry NO `.cstring` literal — they compile to inline immediate compares — so a `strings` sweep UNDER-REPORTS this roster and is not a usable probe.

| [INDEX] | [KEY_STRING]                | [VALUE_TYPE] | [DEFAULT]             | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :----------- | :-------------------- | :-------------------------------------------------- |
|  [01]   | `max_length`                | `double`     | `0` -> context length | total sequence length; `0` faults at search start   |
|  [02]   | `min_length`                | `double`     | `0`                   | minimum generated sequence length                   |
|  [03]   | `batch_size`                | `double`     | `1`                   | staged batch width; MANDATORY on every run          |
|  [04]   | `num_beams`                 | `double`     | `1`                   | beam count; `1` disables beam search                |
|  [05]   | `num_return_sequences`      | `double`     | `1`                   | finished sequences a beam search returns            |
|  [06]   | `top_k`                     | `double`     | `50`                  | top-K sampling limit; `0` or greater, `<= vocab`    |
|  [07]   | `top_p`                     | `double`     | `0`                   | nucleus mass in `[0.0, 1.0]`; `0` disables          |
|  [08]   | `temperature`               | `double`     | `1.0`                 | logit temperature scaling                           |
|  [09]   | `repetition_penalty`        | `double`     | `1.0`                 | repeated-token penalty; `1.0` disables              |
|  [10]   | `length_penalty`            | `double`     | `1.0`                 | `> 0` favors longer, `< 0` shorter                  |
|  [11]   | `no_repeat_ngram_size`      | `double`     | `0`                   | bans a repeated n-gram; `0` disables                |
|  [12]   | `diversity_penalty`         | `double`     | `0`                   | RECOGNIZED AND INERT — upstream marks it unused     |
|  [13]   | `random_seed`               | `double`     | `-1`                  | `-1` seeds from the random device; else seeds RNG   |
|  [14]   | `chunk_size`                | `double`     | unset                 | prefill chunking; active when present and positive  |
|  [15]   | `blank_penalty`             | `double`     | `0`                   | CTC/RNNT blank-token logit penalty                  |
|  [16]   | `do_sample`                 | `bool`       | `false`               | enables stochastic sampling                         |
|  [17]   | `early_stopping`            | `bool`       | `true`                | halts beam search when all beams reach EOS          |
|  [18]   | `past_present_share_buffer` | `bool`       | `false`               | KV in-place reuse; the runtime may force either way |

- Row [03] is not optional: staging without it faults `input sequences count does not match batch size`, and its value is the staged sequence count — one for a single prompt, the `EncodeBatch` sequence count for a batch.
- Rows typed `double` but integral natively (`max_length`, `min_length`, `batch_size`, `num_beams`, `num_return_sequences`, `top_k`, `no_repeat_ngram_size`, `random_seed`, `chunk_size`) cross `Generators::SafeDoubleToInt`, which throws `<key> is not an integer` on a fractional value; the float-typed rows do not.
- Row [13] settles replayability: a seeded sampled run reproduces, and `GetSearchNumber("random_seed")` round-trips the value. The spellings `seed`, `min_p`, and `num_hypotheses` are NOT recognized.
- An unrecognized key raises `JSON::unknown_value_error` from the parser tail. That exception carries NO message and enumerates nothing, so a literal key is undiagnosable at the call site — the consumer's key vocabulary must be closed. The only related strings belong to the GETTERS: `<name> is an invalid name for GetSearchNumber.` and `<name> is an invalid name for GetSearchBool.`

[ENTRYPOINT_SCOPE]: recognized `SetGuidance` type strings — COMPLETE

| [INDEX] | [TYPE_STRING]  | [CAPABILITY]                                 |
| :-----: | :------------- | :------------------------------------------- |
|  [01]   | `json_schema`  | constrains decoding to a JSON schema         |
|  [02]   | `regex`        | constrains decoding to a regular expression  |
|  [03]   | `lark_grammar` | constrains decoding to a Lark-syntax grammar |

- evidence: the runtime's own refusal closes the set — `Unsupported guidance type: <type> (only json_schema, regex, and lark_grammar are supported)`. There is no `choice` type; an enumerated choice rides a `json_schema` enum or a `regex` alternation.
- `SetGuidance` VALIDATES NEITHER the type nor the pairing. It stores type and data and screens one model type (`whisper`); the type check fires later, at `Generator` construction, inside the native LLGuidance constraint initializer. A bad type therefore fails a whole acquire chain rather than its own call.
- Type and data are inseparable: supplying one alone raises `Guidance type and data must be provided together`.
- The `llguidance` crate's own vocabulary (`lark`, `substring_words`, `substring_chars`, and the `TopLevelGrammar`/`GrammarWithLexer` serde field names) also appears in the payload but is INTERNAL to the vendored constraint engine and unreachable through the `type` parameter — a fourth row transcribed from it names something no call site can select.

[ENTRYPOINT_SCOPE]: GPU device and native-log control (`Utils`)
- note: `Utils` is static process-global; the GPU device id selects the CUDA/DML device before model load, and the log toggles drive the native ORT logger.

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Utils.SetCurrentGpuDeviceId(int)`     | static  | selects the active GPU device       |
|  [02]   | `Utils.GetCurrentGpuDeviceId() -> int` | static  | reads the active GPU device id      |
|  [03]   | `Utils.SetLogBool(string, bool)`       | static  | toggles a native ORT log flag       |
|  [04]   | `Utils.SetLogString(string, string)`   | static  | sets a native ORT log string option |
|  [05]   | `Utils.EnableTelemetryEvents()`        | static  | enables native telemetry emission   |
|  [06]   | `Utils.DisableTelemetryEvents()`       | static  | disables native telemetry emission  |

[ENTRYPOINT_SCOPE]: M.E.AI chat client
- note: each ctor takes a trailing `OnnxRuntimeGenAIChatClientOptions?`; the `string`/`Config` ctors build the `Model` internally, and the response and streaming surface is `IChatClient` from `api-extensions-ai`.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new OnnxRuntimeGenAIChatClient(string)`       | ctor     | builds `Model` from a path, owns it              |
|  [02]   | `new OnnxRuntimeGenAIChatClient(Model, bool)`  | ctor     | wraps a `Model`; `ownsModel` gates disposal      |
|  [03]   | `new OnnxRuntimeGenAIChatClient(Config, bool)` | ctor     | builds `Model` from a `Config`                   |
|  [04]   | `IChatClient.GetResponseAsync(...)`            | instance | non-streaming `Task<ChatResponse>`               |
|  [05]   | `IChatClient.GetStreamingResponseAsync(...)`   | instance | streaming `IAsyncEnumerable<ChatResponseUpdate>` |

[ENTRYPOINT_SCOPE]: `OnnxRuntimeGenAIChatClientOptions`

| [INDEX] | [SURFACE]         | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :---------------- | :------- | :------------------------------------------- |
|  [01]   | `StopSequences`   | property | halts generation on any listed sequence      |
|  [02]   | `PromptFormatter` | property | overrides the default chat-template assembly |
|  [03]   | `EnableCaching`   | property | reuses generator state across turns          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every GenAI type wraps a native handle and disposes; `using` order is LIFO with `OgaHandle` outermost for process-global init/teardown. `Adapters : SafeHandle` releases through `OgaDestroyAdapters` at the GC boundary AND answers `Dispose()`, so an adapter set is released deterministically with the model it was created against rather than left to finalization.
- `Config(modelDir)` opens `{modelDir}/genai_config.json`; provider selection rides `Config.AppendProvider`/`SetProviderOption` before `new Model(config)`, never per generation.
- `OnnxRuntimeGenAIException` is the sole fault rail at `Model`/`Config` construction and across the generation loop; a missing or malformed model directory faults it at construction.
- `SetSearchOption` takes `double` or `bool` only, with no string overload and no `TrySetSearchOption`; both overloads reach ONE native parser, so `[03]`'s key roster is shared between them and the overload choice selects only how the value is read. An unrecognized key is unvalidated managed-side and surfaces as an `OnnxRuntimeGenAIException` wrapping a MESSAGELESS native `unknown_value_error` — no key name, no roster — so a consumer that cannot name the recognized set has no diagnostic at all.
- `Generator.GetSequence(ulong)` and `GetNextTokens()` return `ReadOnlySpan<int>` views over native memory owned by the live `Generator`; the newest token copies out before the next step and never retains past the loop.
- `TokenizerStream` comes only from `Tokenizer.CreateStream()`; `TokenizerStream.Decode(int)` decodes one token, distinct from `Tokenizer.Decode(ReadOnlySpan<int>)` over a full span.
- `Generator.AppendTokens`/`AppendTokenSequences` re-feed the tool-call arm's typed results, `RewindTo(ulong)` rewinds a partial turn, and `Generator.SetActiveAdapter` hot-swaps a LoRA adapter mid-generation over an `Adapters` set loaded per name.
- `MultiModalProcessor` encodes vision/audio prompts into `NamedTensors` that feed `Generator.SetInputs`; `StreamingProcessor.Process(float[])` returns `null` until a VAD boundary and `Flush()` drains the final fragment.
- `Tensor` is `Microsoft.ML.OnnxRuntimeGenAI.Tensor`, qualified on any boundary shared with `System.Numerics.Tensors.Tensor<T>`, and `Tensor.GetData<T>()` spans native memory owned by the live `Tensor` that copies out before disposal.
- `OnnxRuntimeGenAIChatClient` is `sealed : IChatClient, IDisposable` over three ctors — path, `(Model, ownsModel)`, `(Config, ownsConfig)` — with no `(Model, options)` two-arg form, the `owns*` flag governing whether the client disposes the handle, and `OnnxRuntimeGenAIChatClientOptions.PromptFormatter` the override seam for `Tokenizer.ApplyChatTemplate` when the M.E.AI consumer owns prompt assembly.
- `Utils` is the static process-global control: `SetCurrentGpuDeviceId(int)` sets the global device before model load ahead of the per-decoder `Config.SetDecoderProviderOptionsHardwareDeviceId`, and `SetLogBool`/`SetLogString` are the sole handle to the native ORT logger.
- `SetRuntimeOption` validates nothing managed-side: an unrecognized key is accepted SILENTLY, and `terminate_session` = `1` mid-drain ABORTS the process — an uncatchable native abort no managed rail observes — so a consumer admits runtime keys against a banned set rather than passing them through.
- `Generator.GetOutput(name)` and `GetInput(name)` SIGSEGV on a name the live graph does not carry; there is no managed name check, so a read is only safe for a name the model class is known to publish.
- `Generator.GetSequence(ulong)` performs NO range check — an index past the batch width returns sequence 0 rather than throwing — so every read gates the index against the staged width first.
- `Generator.IsDone()` answers the whole BATCH: a sequence emitting EOS leaves it false while any other sequence runs, and a finished sequence then emits the pad token at full batch width on every remaining step, so per-sequence stop is caller-owned state.
- `Generator.TokenCount()` is one batch-wide scalar, and `RewindTo` on a batch wider than one admits only `0` while a restart faults, so a multi-sequence run has no rewind or restart rail at all.
- `GeneratorParams.SetGuidance(type, data, enableFFTokens)` with fast-forward ENABLED commits tokens `GetNextTokens()` never surfaces, so a streamed decode loses spans the committed sequence holds; under any guidance, `Generator.AppendTokens` admits only spans the grammar derives and rejects free text with a parser error. The call itself validates NEITHER the type against `[03]`'s roster nor the type-plus-data pairing — both refuse at `Generator` construction instead — so a guidance defect surfaces one acquire chain later than the call that caused it.
- Structured output is a SEARCH-TIME constraint, never a post-hoc validation: `random_seed` fixes the sampler's draw and guidance fixes the admissible token set, and the two compose — a seeded, guided run is reproducible token for token.
- `Tokenizer.ApplyChatTemplate` takes the tools argument as raw JSON text and the native template pass rejects malformed input; a template carrying no tools block ignores the argument entirely, so a roster passed to such a template is silently inert rather than faulted.
- `Images.Load`/`Audios.Load` FAULT on an empty path array, so a media-side call dispatches on which side carries paths; `NamedTensors` is opaque — it publishes `Dispose()` alone and no element or shape read.
- `MultiModalProcessor` binds only a model type registered as multimodal and `StreamingProcessor` binds only the speech-stream type; either constructed against another model type faults, so a consumer gates on `Model.GetModelType()` before construction.
- `Tokenizer.EncodeBatch` LEFT-PADS a ragged batch to its longest member, so the returned `NumSequences` equals the prompt count and the padded prefix is the pad token.
- Provider names passed to `Config.AppendProvider` are case-sensitive native strings resolved at `Model` construction: `CoreML` builds and generates on this package version, and `XNNPACK` refuses `not supported in this build` on `osx-arm64`.

[STACKING]:
- `api-onnxruntime`(`.api/api-onnxruntime.md`): the genai runtime composes the base inference engine; the EP strings `Config.AppendProvider` accepts, the `Config.SetProviderOption` keys, and the `Config.SetDecoderProviderOptionsHardware{DeviceType,DeviceId,VendorId}` selectors bind that catalog's EP roster and `OrtHardwareDevice` discovery, and its per-RID native ABI matrix owns payload availability; the genai payload ships the subset of those RIDs it publishes (`osx-arm64` the host asset), so a decoder provider with no matching genai-and-base payload faults at native init.
- `api-recyclable-stream`(`.api/api-recyclable-stream.md`): `Tensor.GetData<T>()` and `Generator.GetSequence(ulong)` native-backed `ReadOnlySpan<T>` views copy into a rented `RecyclableMemoryStream` at the edge, never ad hoc arrays.
- `api-extensions-ai`(`.api/api-extensions-ai.md`): `IChatClient`, `ChatResponse`, `ChatResponseUpdate`, `ChatMessage`, and `ChatOptions` resolve there, and `OnnxRuntimeGenAIChatClient` implements `IChatClient` over the same handle chain, so a second managed chat-message model beside `ChatMessage` never enters.
- `api-protobuf`(`.api/api-protobuf.md`): a structured generative artifact crossing the wire is a `Runtime/wire#PROTO_VOCABULARY` message, never a managed DTO.
- `Model` rail `GENERATIVE_RUN`: token streaming folds the LIFO handle chain as a run mode, and the model-rejection fault lifts to `ComputeFault.ModelRejected` at the boundary rather than a per-call catch. The `SearchKey` and `GuidanceKind` rosters on that page are the typed projection of `[03]`'s two verified vocabularies — key strings, value types, and integrality gates read HERE and are never re-transcribed there — while each row's policy SEED is that page's own product posture and deliberately diverges from this catalog's `[DEFAULT]` column where stated.

[LOCAL_ADMISSION]:
- Token streaming is a run mode on the owned model lane, and grammar-constrained structured output is enforced at generation through `SetGuidance`; a `GenerativeService`/`ChatClient`/`Conversation`/`PromptService` wrapper or a managed JSON-schema output validator is the rejected form.
- `OnnxRuntimeGenAIChatClient : IChatClient` is the admitted M.E.AI projection over the shared handle chain, never a hand-rolled one.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntimeGenAI`
- Owns: generative token-streaming runtime, multimodal encoding, streaming audio, `Utils` GPU-device/native-log control, and the `OnnxRuntimeGenAIChatClient` `IChatClient` projection
- Accept: model-dir generative runs over the LIFO handle chain; `Images`/`Audios` + `MultiModalProcessor` multimodal pipelines; incremental `StreamingProcessor` audio; M.E.AI streaming through the three admitted ctors staged onto the `api-recyclable-stream` pool
- Reject: chat-client/conversation/prompt service families; managed output validators; a second managed chat-message model beside `ChatMessage`; the phantom `(Model, options)` ctor; `System.Numerics.Tensors.Tensor<T>` confused with the GenAI `Tensor`; a model run with no matching native RID payload; a search-option key or guidance type outside `[03]`'s verified rosters, and a `strings`-only re-derivation of either (half the key roster carries no literal)
