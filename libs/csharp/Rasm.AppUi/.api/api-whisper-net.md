# [RASM_APPUI_API_WHISPER_NET]

`Whisper.net` is the offline speech-to-text owner for the LiveCaption rail — managed bindings over `whisper.cpp` (the ggml Whisper inference engine). It carries the transcription surface under `Whisper.net`: `WhisperFactory` loads a ggml model into a reusable inference handle; `WhisperProcessorBuilder` is the one polymorphic build fold (every task, language, sampling, threshold, timestamp, and event-handler knob is a `With*` row on one builder); `WhisperProcessor.ProcessAsync` streams `SegmentData` as an `IAsyncEnumerable` for live caption emission; the built-in translate-to-English task (`WithTranslate`) folds recognition + translation into one pass; and a separate Silero-VAD pipeline (`WhisperVadFactory`/`WhisperVadProcessor`) segments speech before transcription. The core package ships NO native inference library and NO model weights — the `whisper.cpp` runtime is provisioned by the separate `Whisper.net.Runtime*` packages, and ggml weights download separately through `WhisperGgmlDownloader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Whisper.net`

- package: `Whisper.net` (core; NO runtime, NO weights)
- license: MIT (expression)
- assembly: `Whisper.net`
- namespace: `Whisper.net` (factory/processor/builder/segment/VAD/client), `Whisper.net.Ggml` (downloader + `GgmlType`/`QuantizationType`/`SileroVadType`), `Whisper.net.LibraryLoader` (`RuntimeOptions`/`RuntimeLibrary`), `Whisper.net.Logger` (`LogProvider`/`WhisperLogLevel`), `Whisper.net.Wave` (`WaveParser`)
- target: `lib/net10.0` (bound) + `net9.0`/`net8.0`/`netstandard2.0`; `LangVersion 13`, `AllowUnsafeBlocks`
- depends: `Microsoft.Extensions.AI.Abstractions` (the `ISpeechToTextClient` surface); `netstandard2.0` adds `Microsoft.Bcl.AsyncInterfaces` + `System.Memory`
- runtime: the native `whisper.cpp` library is resolved at load time by `NativeLibraryLoader` per `RuntimeOptions.RuntimeLibraryOrder` — shipped by a separate `Whisper.net.Runtime*` package, never this core
- rail: caption

## [02]-[PUBLIC_TYPES]

[TRANSCRIPTION_CORE]: model handle + streaming processor — rail: caption

| [INDEX] | [SYMBOL]                     | [ROLE]             |
| :-----: | :--------------------------- | :----------------- |
|  [01]   | `WhisperFactory`             | loaded model       |
|  [02]   | `WhisperFactoryOptions`      | factory options    |
|  [03]   | `WhisperProcessorBuilder`    | build fold         |
|  [04]   | `WhisperProcessor`           | processor          |
|  [05]   | `WhisperSpeechToTextClient`  | speech client      |
|  [06]   | `WhisperModelLoadException`  | load failure       |
|  [07]   | `WhisperProcessingException` | processing failure |
|  [08]   | `IStringPool`                | string pool        |
|  [09]   | `IWhisperModelLoader`        | model loader       |

[WHISPER_FACTORY]:

- Contract: `IDisposable`
- Role: Loaded ggml model handle
- Surface: `FromPath`, `FromBuffer`, `CreateBuilder`

[WHISPER_FACTORY_OPTIONS]:

- Defaults: `UseGpu` is `true`
- Acceleration: `UseFlashAttention`, `UseDtwTimeStamps`, `GpuDevice`
- Alignment: `HeadsPreset : WhisperAlignmentHeadsPreset`, `CustomAlignmentHeads`
- DTW: `DtwMemSize`, `DtwNTop`
- Lifecycle: `DelayInitialization`

[WHISPER_PROCESSOR_BUILDER]:

- Role: One polymorphic build fold
- Surface: Every task, language, sampling, threshold, timestamp, and handler knob is a `With*` row; `Build()` is terminal.

[WHISPER_PROCESSOR]:

- Contract: `IAsyncDisposable`, `IDisposable`
- Surface: `Process`, `ProcessAsync`, `DetectLanguage`, `ChangeLanguage`
- Inputs: Stream and sample forms

[WHISPER_SPEECH_TO_TEXT_CLIENT]:

- Contract: `ISpeechToTextClient` from Microsoft.Extensions.AI
- Surface: `GetStreamingTextAsync`, `GetTextAsync`
- Input: `WhisperFactory`
- Role: LiveCaption streaming seam

[TRANSCRIPTION_SEAMS]:

- Failure: `WhisperModelLoadException` and `WhisperProcessingException` type the load and processing rails.
- Extension: `IStringPool` and `IWhisperModelLoader` expose the string-pool performance and model-loader seams.

[SEGMENT_MODEL]: emitted transcription rows + event delegates — rail: caption

| [INDEX] | [SYMBOL]                      | [ROLE]            |
| :-----: | :---------------------------- | :---------------- |
|  [01]   | `SegmentData`                 | caption segment   |
|  [02]   | `WhisperToken`                | token detail      |
|  [03]   | `OnSegmentEventHandler`       | segment delegate  |
|  [04]   | `OnProgressHandler`           | progress delegate |
|  [05]   | `OnEncoderBeginEventHandler`  | encoder delegate  |
|  [06]   | `WhisperAbortEventHandler`    | abort delegate    |
|  [07]   | `WhisperAlignmentHeadsPreset` | alignment preset  |
|  [08]   | `WhisperAlignmentHead`        | alignment head    |

[SEGMENT_DATA]:

- Content: `Text`, `Language`, `Tokens : WhisperToken[]`
- Timing: `Start : TimeSpan`, `End : TimeSpan`
- Confidence: `Probability`, `MinProbability`, `MaxProbability`, `NoSpeechProbability`

[WHISPER_TOKEN]:

- Fields: `Id`, `Text`, `Probability`, `Start`, `End`, `DtwTimestamp`, `VoiceLen`, …

[SEGMENT_EVENTS]:

- `OnSegmentEventHandler`: Segment callback
- `OnProgressHandler`: Progress callback
- `OnEncoderBeginEventHandler`: Encoder-begin callback with cancellation
- `WhisperAbortEventHandler`: Abort callback

[ALIGNMENT_HEADS]:

- `WhisperAlignmentHeadsPreset`: DTW token-timestamp alignment-head preset
- `WhisperAlignmentHead`: `(textLayer, head)` value

[VAD_PIPELINE]: Silero voice-activity detection (separate from transcription) — rail: caption

| [INDEX] | [SYMBOL]                     | [ROLE]       |
| :-----: | :--------------------------- | :----------- |
|  [01]   | `WhisperVadFactory`          | model handle |
|  [02]   | `WhisperVadProcessorBuilder` | tuning fold  |
|  [03]   | `WhisperVadProcessor`        | processor    |
|  [04]   | `VadSegmentData`             | speech span  |

[WHISPER_VAD_FACTORY]:

- Contract: `IDisposable`
- Role: Loaded Silero-VAD model handle
- Surface: `FromPath`, `CreateBuilder()`

[WHISPER_VAD_PROCESSOR_BUILDER]:

- Thresholds: `WithThreshold`, `WithMinSpeechDuration`, `WithMinSilenceDuration`, `WithMaxSpeechDuration`
- Windows: `WithSpeechPadding`, `WithSamplesOverlap`
- Execution: `WithThreads`, `WithUseGpu`, `WithGpuDevice`
- Terminal: `Build()`

[WHISPER_VAD_PROCESSOR]:

- Contract: `IAsyncDisposable`, `IDisposable`
- Detection: `DetectSpeech`, `DetectSpeechAsync`, and the `…NoReset` variants
- State: `ResetState`

[VAD_SEGMENT_DATA]:

- Span: `Start : TimeSpan`, `End : TimeSpan`

[GGML_RUNTIME_VOCABULARY]: model download + runtime selection — rail: caption

| [INDEX] | [SYMBOL]                | [ROLE]           |
| :-----: | :---------------------- | :--------------- |
|  [01]   | `WhisperGgmlDownloader` | model downloader |
|  [02]   | `GgmlType`              | model variant    |
|  [03]   | `QuantizationType`      | quantization     |
|  [04]   | `SileroVadType`         | VAD version      |
|  [05]   | `RuntimeOptions`        | runtime options  |
|  [06]   | `RuntimeLibrary`        | runtime selector |
|  [07]   | `LogProvider`           | logging API      |
|  [08]   | `WhisperLogLevel`       | log level        |

[WHISPER_GGML_DOWNLOADER]:

- Instance: `Default`
- Models: `GetGgmlModelAsync`, `GetGgmlSileroVadModelAsync`
- Encoders: `GetEncoderCoreMLModelAsync`, `GetEncoderOpenVinoModelAsync`
- Source: HuggingFace

[GGML_TYPE]:

- Values: `Tiny`, `TinyEn`, `Base`, `BaseEn`, `Small`, `SmallEn`, `Medium`, `MediumEn`, `LargeV1`, `LargeV2`, `LargeV3`, `LargeV3Turbo`

[QUANTIZATION_TYPE]:

- Values: `NoQuantization`, `Q4_0`, `Q4_1`, `Q5_0`, `Q5_1`, `Q8_0`

[SILERO_VAD_TYPE]:

- Values: `V5_1_2`, `V6_2_0`
- Default: `V6_2_0`

[RUNTIME_SELECTION]:

- Options: `RuntimeLibraryOrder`, `LibraryPath`, `LoadedLibrary`
- Default order: `[Cuda, Cuda12, Vulkan, CoreML, OpenVino, Cpu, CpuNoAvx]`
- Libraries: `Cpu`, `Cuda`, `Cuda12`, `Vulkan`, `CoreML`, `OpenVino`, `CpuNoAvx`

[LOGGING]:

- Surface: `AddConsoleLogging(minLevel)`, `AddLogger(Action<WhisperLogLevel,string?>)`
- Levels: `None`, `Error`, `Warning`, `Info`, `Cont`, `Debug`

## [03]-[ENTRYPOINTS]

[LOAD_BUILD_PROCESS]: the factory → builder → processor fold

- rail: caption
- `WhisperFactory` loads once and is reused; each `CreateBuilder()` configures one `WhisperProcessor`. `ProcessAsync` streams `SegmentData` as they finalize — the live-caption emit point.

| [INDEX] | [OPERATION] | [SURFACE_ROOT]     | [CAPABILITY]    |
| :-----: | :---------- | :----------------- | :-------------- |
|  [01]   | load        | `WhisperFactory`   | model load      |
|  [02]   | inspect     | `WhisperFactory`   | runtime probes  |
|  [03]   | stream      | `WhisperProcessor` | live segments   |
|  [04]   | batch       | `WhisperProcessor` | transcription   |
|  [05]   | language    | `WhisperProcessor` | language state  |
|  [06]   | teardown    | `WhisperProcessor` | resource return |

[MODEL_LOAD]:

- Path: `FromPath(string)`, `FromPath(string, WhisperFactoryOptions)`
- Memory: `FromBuffer(Memory<byte>)`, `FromBuffer(Memory<byte>, WhisperFactoryOptions)`

[FACTORY_INSPECTION]:

- Surface: `CreateBuilder()`, `GetSupportedLanguages()`, `GetRuntimeInfo()`
- Role: Open a build fold, probe languages, or probe native SIMD and acceleration.

[STREAM_TRANSCRIPTION]:

- Surface: `ProcessAsync(Stream, CancellationToken)`, `ProcessAsync(ReadOnlyMemory<float>, …)`, `ProcessAsync(float[], …)`
- Result: `IAsyncEnumerable<SegmentData>`

[BATCH_TRANSCRIPTION]:

- Surface: `Process(Stream)`, `Process(float[])`, `Process(ReadOnlySpan<float>)`

[LANGUAGE_STATE]:

- Surface: `DetectLanguage(float[])`, `DetectLanguageWithProbability(float[])`
- Constrained probe: `DetectLanguageWithProbability(ReadOnlySpan<float>, params ReadOnlySpan<string>)`
- Mutation: `ChangeLanguage(string?)`

[PROCESSOR_TEARDOWN]:

- Surface: `DisposeAsync()`, `Return(SegmentData)`
- Role: Tear down asynchronously or return pooled strings.

[BUILDER_KNOBS]: the polymorphic `With*` configuration surface on `WhisperProcessorBuilder`

- rail: caption
- One builder owns all variation; the LiveCaption path is `WithLanguage("auto")` (or `WithLanguageDetection()`) + `WithTranslate()` (recognize + translate-to-English in one pass) + `WithSegmentEventHandler`/streaming, tuned by the threshold and sampling rows.

| [INDEX] | [FAMILY]                | [CAPABILITY]       |
| :-----: | :---------------------- | :----------------- |
|  [01]   | task and language       | prompt policy      |
|  [02]   | threading and window    | audio window       |
|  [03]   | segmentation and timing | caption chunks     |
|  [04]   | decoding thresholds     | confidence gates   |
|  [05]   | sampling strategy       | decoder selection  |
|  [06]   | event handlers          | callback routing   |
|  [07]   | performance and tracing | runtime tuning     |
|  [08]   | terminal                | processor creation |

[TASK_AND_LANGUAGE]:

- Task: `WithLanguage(string)`, `WithLanguageDetection()`, `WithTranslate()`
- Context: `WithNoContext()`, `WithSingleSegment()`, `WithPrompt(string)`, `WithCarryInitialPrompt(bool)`
- Suppression: `WithSuppressRegex(string)`, `WithoutSuppressBlank()`

[THREADING_AND_WINDOW]:

- Surface: `WithThreads(int)`, `WithOffset(TimeSpan)`, `WithDuration(TimeSpan)`
- Context: `WithMaxLastTextTokens(int)`, `WithAudioContextSize(int)`

[SEGMENTATION_AND_TIMING]:

- Segments: `WithMaxSegmentLength(int)`, `WithMaxTokensPerSegment(int)`, `SplitOnWord()`
- Timestamps: `WithTokenTimestamps()`, `WithTokenTimestampsThreshold(float)`, `WithTokenTimestampsSumThreshold(float)`

[DECODING_THRESHOLDS]:

- Temperature: `WithTemperature(float)`, `WithTemperatureInc(float)`, `WithMaxInitialTs(float)`
- Quality: `WithLengthPenalty(float)`, `WithEntropyThreshold(float)`, `WithLogProbThreshold(float)`, `WithNoSpeechThreshold(float)`
- Output: `WithProbabilities()`

[SAMPLING_STRATEGY]:

- Greedy: `WithGreedySamplingStrategy(Action<GreedySamplingStrategyBuilder>)` with `WithBestOf(int)`
- Beam: `WithBeamSearchSamplingStrategy(Action<BeamSearchSamplingStrategyBuilder>)` with `WithBeamSize(int)` and `WithPatience(float)`

[EVENT_HANDLERS]:

- Segment: `WithSegmentEventHandler(OnSegmentEventHandler)`
- Progress: `WithProgressHandler(OnProgressHandler)`
- Encoder: `WithEncoderBeginHandler(OnEncoderBeginEventHandler)`

[PERFORMANCE_AND_TRACING]:

- Pooling: `WithStringPool(IStringPool?)`, `WithoutStringPool()`
- Encoder: `WithOpenVinoEncoder(string?, string?, string?)`
- Tracing: `WithPrintProgress()`, `WithPrintResults()`, `WithPrintTimestamps(bool)`, `WithPrintSpecialTokens()`

[BUILD_TERMINAL]:

- Surface: `Build()`
- Result: `WhisperProcessor`

[VAD_AND_ASSETS]: Silero VAD pipeline + ggml download + runtime/log

- rail: caption

| [INDEX] | [OPERATION] | [SURFACE_ROOT]              | [CAPABILITY]    |
| :-----: | :---------- | :-------------------------- | :-------------- |
|  [01]   | configure   | `WhisperVadFactory`         | VAD load        |
|  [02]   | detect      | `WhisperVadProcessor`       | speech spans    |
|  [03]   | fetch       | `WhisperGgmlDownloader`     | model assets    |
|  [04]   | select      | `RuntimeOptions`            | native runtime  |
|  [05]   | log         | `LogProvider`               | native logs     |
|  [06]   | transcribe  | `WhisperSpeechToTextClient` | caption streams |

[VAD_CONFIGURATION]:

- Surface: `FromPath(string)` → `CreateBuilder()` → `Build()`

[SPEECH_DETECTION]:

- Sync: `DetectSpeech(Stream/float[]/ReadOnlySpan<float>)`
- Async: `DetectSpeechAsync(Stream/float[]/ReadOnlyMemory<float>, CancellationToken)`
- State: `…NoReset`, `ResetState()`
- Result: `VadSegmentData` spans before transcription

[MODEL_ASSETS]:

- Instance: `Default`
- Model: `GetGgmlModelAsync(GgmlType, QuantizationType, CancellationToken)`
- VAD: `GetGgmlSileroVadModelAsync(SileroVadType, CancellationToken)`
- Source: HuggingFace

[NATIVE_RUNTIME]:

- Surface: `RuntimeLibraryOrder`, `LibraryPath`, `LoadedLibrary`

[NATIVE_LOGGING]:

- Surface: `AddConsoleLogging(WhisperLogLevel)`, `AddLogger(Action<WhisperLogLevel,string?>)`

[SPEECH_TO_TEXT_CLIENT]:

- Stream: `GetStreamingTextAsync(Stream, SpeechToTextOptions?, CancellationToken)`
- Batch: `GetTextAsync(...)`
- Role: Microsoft.Extensions.AI streaming-caption seam

## [04]-[IMPLEMENTATION_LAW]

[CAPTION_LAW]:

- Package: `Whisper.net`
- Owns: offline speech-to-text for LiveCaption — model load (`WhisperFactory.FromPath`/`FromBuffer`), the one polymorphic `WhisperProcessorBuilder` `With*` fold, streaming transcription (`WhisperProcessor.ProcessAsync` → `IAsyncEnumerable<SegmentData>`), the built-in translate-to-English task (`WithTranslate`), language auto-detect (`DetectLanguage`/`WithLanguageDetection`), token timestamps (DTW alignment heads), and the separate Silero-VAD segmentation pipeline.
- Accept: the LiveCaption rail loads one `WhisperFactory` per session and streams captions through `ProcessAsync`, configured on the single builder (`WithLanguage`/`WithTranslate`/thresholds/sampling/`WithSegmentEventHandler`); `WhisperVadProcessor.DetectSpeech*` gates transcription to speech spans; `SegmentData.Start`/`End` `TimeSpan`s drive caption timing; the Microsoft.Extensions.AI `WhisperSpeechToTextClient.GetStreamingTextAsync` is the abstraction seam where the caption UI consumes an `ISpeechToTextClient`; runtime selection is `RuntimeOptions.RuntimeLibraryOrder` (CoreML on Apple silicon).
- Reject: a cloud STT dependency where offline `whisper.cpp` inference is the mandate; a `Get`/`GetMany`/`Transcribe`/`Translate` operation family where the one builder + `ProcessAsync` discriminate task by `With*` row and input by overload; a hand-rolled VAD where the Silero pipeline owns it; a hand-rolled ICU/plural translation of caption text where `WithTranslate` performs recognition+translation in the model pass; leaking the `WhisperFactory`/`WhisperProcessor`/VAD handles where `IDisposable`/`IAsyncDisposable` teardown frees the native context.
- Provisioning is honest: the ggml model file and the Silero-VAD model are fetched once through `WhisperGgmlDownloader` and cached on disk (never embedded), and the native `whisper.cpp` runtime is a separate `Whisper.net.Runtime*` package — a missing model or runtime is a host-provisioning fault surfaced through `WhisperModelLoadException` and the `LogProvider` rail.

[STACKING]:

- Complements `api-messageformat.md`: `Whisper.net.WithTranslate` produces English caption text; `MessageFormat` owns the CLDR plural/select localization of the surrounding UI chrome — recognition/translation versus message formatting is the seam, never overlapped.
- Complements `api-libmpv.md` / `api-ffmpeg-autogen.md`: `libmpv` decodes the media whose audio track feeds captions; `Whisper.net` transcribes; `FFmpeg.AutoGen` encodes the deliverable — decode, transcribe, encode are three distinct owners over the media spine.
- The `SegmentData` stream is a receipt row on the AppUi telemetry spine exactly as the wgpu/libmpv native log streams are — a low-confidence segment (`NoSpeechProbability`, `Probability` gates) is a counted caption-quality fact, not a swallowed print.

> [!IMPORTANT]
> The core `Whisper.net` assembly ships NEITHER the native inference library NOR model weights. The native `whisper.cpp` runtime is provisioned by a separate `Whisper.net.Runtime*` package (`Whisper.net.Runtime.CoreML` on Apple silicon, `.Runtime.Cuda`/`.Runtime.Cuda12` on NVIDIA, `.Runtime.Vulkan`/`.Runtime.OpenVino`, `.Runtime` CPU, `.Runtime.NoAvx`) and selected at load time by `RuntimeOptions.RuntimeLibraryOrder`. The ggml model weights (and the Silero-VAD model) download separately at first use through `WhisperGgmlDownloader.Default` from the HuggingFace repository and are cached on disk — the app-host distribution layer owns both provisioning steps, exactly as the `FFmpeg`/`libmpv` natives are provisioned.
