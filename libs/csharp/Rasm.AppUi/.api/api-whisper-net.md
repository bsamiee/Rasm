# [RASM_APPUI_API_WHISPER_NET]

`Whisper.net` is the offline speech-to-text owner for the LiveCaption rail — managed bindings over `whisper.cpp` (the ggml Whisper inference engine). It carries the transcription surface under `Whisper.net`: `WhisperFactory` loads a ggml model into a reusable inference handle; `WhisperProcessorBuilder` is the one polymorphic build fold (every task, language, sampling, threshold, timestamp, and event-handler knob is a `With*` row on one builder); `WhisperProcessor.ProcessAsync` streams `SegmentData` as an `IAsyncEnumerable` for live caption emission; the built-in translate-to-English task (`WithTranslate`) folds recognition + translation into one pass; and a separate Silero-VAD pipeline (`WhisperVadFactory`/`WhisperVadProcessor`) segments speech before transcription. The core package ships NO native inference library and NO model weights — the `whisper.cpp` runtime is provisioned by the separate `Whisper.net.Runtime*` packages, and ggml weights download separately through `WhisperGgmlDownloader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Whisper.net`
- package: `Whisper.net` (core; NO runtime, NO weights)
- version: `1.9.1`
- license: MIT (expression)
- assembly: `Whisper.net`
- namespace: `Whisper.net` (factory/processor/builder/segment/VAD/client), `Whisper.net.Ggml` (downloader + `GgmlType`/`QuantizationType`/`SileroVadType`), `Whisper.net.LibraryLoader` (`RuntimeOptions`/`RuntimeLibrary`), `Whisper.net.Logger` (`LogProvider`/`WhisperLogLevel`), `Whisper.net.Wave` (`WaveParser`)
- target: `lib/net10.0` (bound) + `net9.0`/`net8.0`/`netstandard2.0`; `LangVersion 13`, `AllowUnsafeBlocks`
- depends: `Microsoft.Extensions.AI.Abstractions` (the `ISpeechToTextClient` surface); `netstandard2.0` adds `Microsoft.Bcl.AsyncInterfaces` + `System.Memory`
- runtime: the native `whisper.cpp` library is resolved at load time by `NativeLibraryLoader` per `RuntimeOptions.RuntimeLibraryOrder` — shipped by a separate `Whisper.net.Runtime*` package, never this core
- rail: caption

## [02]-[PUBLIC_TYPES]

[TRANSCRIPTION_CORE]: model handle + streaming processor — rail: caption

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `WhisperFactory`             | `sealed : IDisposable` — the loaded ggml model; `FromPath`/`FromBuffer` factories, `CreateBuilder()` |
|  [02]   | `WhisperFactoryOptions`      | `struct` — `UseGpu` (default true), `UseFlashAttention`, `UseDtwTimeStamps`, `HeadsPreset : WhisperAlignmentHeadsPreset`, `CustomAlignmentHeads`, `GpuDevice`, `DtwMemSize`, `DtwNTop`, `DelayInitialization` |
|  [03]   | `WhisperProcessorBuilder`    | the one polymorphic build fold — every task/language/sampling/threshold/timestamp/handler knob is a `With*` row, terminal `Build()` |
|  [04]   | `WhisperProcessor`           | `sealed : IAsyncDisposable, IDisposable` — `Process`/`ProcessAsync` (stream + sample forms), `DetectLanguage`, `ChangeLanguage` |
|  [05]   | `WhisperSpeechToTextClient`  | `sealed : ISpeechToTextClient` (Microsoft.Extensions.AI) — the LiveCaption streaming fit: `GetStreamingTextAsync`/`GetTextAsync` over a `WhisperFactory` |
|  [06]   | `WhisperModelLoadException` / `WhisperProcessingException` | typed load + processing failure rails                                |
|  [07]   | `IStringPool` / `IWhisperModelLoader`                      | pluggable string-pool (perf) + model-loader seams                    |

[SEGMENT_MODEL]: emitted transcription rows + event delegates — rail: caption

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `SegmentData`                | a caption segment — `Text`, `Start : TimeSpan`, `End : TimeSpan`, `Probability`/`MinProbability`/`MaxProbability`, `NoSpeechProbability`, `Language`, `Tokens : WhisperToken[]` |
|  [02]   | `WhisperToken`               | per-token detail (fields) — `Id`, `Text`, `Probability`, `Start`/`End`, `DtwTimestamp`, `VoiceLen`, … |
|  [03]   | `OnSegmentEventHandler` / `OnProgressHandler` / `OnEncoderBeginEventHandler` / `WhisperAbortEventHandler` | segment / progress / encoder-begin(cancel) / abort delegates |
|  [04]   | `WhisperAlignmentHeadsPreset` / `WhisperAlignmentHead` | DTW token-timestamp alignment-head preset + `(textLayer, head)` struct |

[VAD_PIPELINE]: Silero voice-activity detection (separate from transcription) — rail: caption

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `WhisperVadFactory`          | `sealed : IDisposable` — the loaded Silero-VAD model; `FromPath`, `CreateBuilder()` |
|  [02]   | `WhisperVadProcessorBuilder` | VAD tuning fold — `WithThreshold`/`WithMinSpeechDuration`/`WithMinSilenceDuration`/`WithMaxSpeechDuration`/`WithSpeechPadding`/`WithSamplesOverlap`/`WithThreads`/`WithUseGpu`/`WithGpuDevice`, `Build()` |
|  [03]   | `WhisperVadProcessor`        | `sealed : IAsyncDisposable, IDisposable` — `DetectSpeech`/`DetectSpeechAsync` (+ `…NoReset` variants), `ResetState` |
|  [04]   | `VadSegmentData`             | a detected speech span — `Start : TimeSpan`, `End : TimeSpan`                       |

[GGML_RUNTIME_VOCABULARY]: model download + runtime selection — rail: caption

| [INDEX] | [SYMBOL]                     | [KIND]                                                                             |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `WhisperGgmlDownloader`      | `Default` singleton + `GetGgmlModelAsync`/`GetGgmlSileroVadModelAsync`/`GetEncoderCoreMLModelAsync`/`GetEncoderOpenVinoModelAsync` (HuggingFace fetch) |
|  [02]   | `GgmlType`                   | model size/variant — `Tiny`/`TinyEn`/`Base`/`BaseEn`/`Small`/`SmallEn`/`Medium`/`MediumEn`/`LargeV1`/`LargeV2`/`LargeV3`/`LargeV3Turbo` |
|  [03]   | `QuantizationType`           | weight quantization — `NoQuantization`/`Q4_0`/`Q4_1`/`Q5_0`/`Q5_1`/`Q8_0`          |
|  [04]   | `SileroVadType`              | VAD model version — `V5_1_2`/`V6_2_0` (default `V6_2_0`)                           |
|  [05]   | `RuntimeOptions` / `RuntimeLibrary` | `RuntimeLibraryOrder` (default `[Cuda, Cuda12, Vulkan, CoreML, OpenVino, Cpu, CpuNoAvx]`), `LibraryPath`, `LoadedLibrary`; the `Cpu`/`Cuda`/`Cuda12`/`Vulkan`/`CoreML`/`OpenVino`/`CpuNoAvx` selector |
|  [06]   | `LogProvider` / `WhisperLogLevel` | `AddConsoleLogging(minLevel)` / `AddLogger(Action<WhisperLogLevel,string?>)`; `None`/`Error`/`Warning`/`Info`/`Cont`/`Debug` |

## [03]-[ENTRYPOINTS]

[LOAD_BUILD_PROCESS]: the factory → builder → processor fold
- rail: caption
- `WhisperFactory` loads once and is reused; each `CreateBuilder()` configures one `WhisperProcessor`. `ProcessAsync` streams `SegmentData` as they finalize — the live-caption emit point.

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT]         | [RAIL]  |
| :-----: | :------------------------------------------------------------------------ | :--------------------- | :------ |
|  [01]   | `FromPath(string)` / `FromPath(string, WhisperFactoryOptions)` / `FromBuffer(Memory<byte>)` / `FromBuffer(Memory<byte>, WhisperFactoryOptions)` | `WhisperFactory` | load the ggml model (file / in-memory) |
|  [02]   | `CreateBuilder()` / `GetSupportedLanguages()` / `GetRuntimeInfo()`         | `WhisperFactory`       | open a build fold / probe languages / probe native SIMD+accel |
|  [03]   | `ProcessAsync(Stream, CancellationToken)` / `ProcessAsync(ReadOnlyMemory<float>, …)` / `ProcessAsync(float[], …)` → `IAsyncEnumerable<SegmentData>` | `WhisperProcessor` | STREAM caption segments |
|  [04]   | `Process(Stream)` / `Process(float[])` / `Process(ReadOnlySpan<float>)`    | `WhisperProcessor`     | synchronous batch transcription |
|  [05]   | `DetectLanguage(float[])` / `DetectLanguageWithProbability(float[])` / `DetectLanguageWithProbability(ReadOnlySpan<float>, params ReadOnlySpan<string>)` / `ChangeLanguage(string?)` | `WhisperProcessor` | language auto-detect + mid-stream language switch |
|  [06]   | `DisposeAsync()` / `Return(SegmentData)`                                   | `WhisperProcessor`     | async teardown / return pooled strings |

[BUILDER_KNOBS]: the polymorphic `With*` configuration surface on `WhisperProcessorBuilder`
- rail: caption
- One builder owns all variation; the LiveCaption path is `WithLanguage("auto")` (or `WithLanguageDetection()`) + `WithTranslate()` (recognize + translate-to-English in one pass) + `WithSegmentEventHandler`/streaming, tuned by the threshold and sampling rows.

| [INDEX] | [SURFACE]                                                                                     | [FAMILY]              | [RAIL]  |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------------- | :------ |
|  [01]   | `WithLanguage(string)` / `WithLanguageDetection()` / `WithTranslate()` / `WithNoContext()` / `WithSingleSegment()` / `WithPrompt(string)` / `WithCarryInitialPrompt(bool)` / `WithSuppressRegex(string)` / `WithoutSuppressBlank()` | task/language | recognition task + prompt policy |
|  [02]   | `WithThreads(int)` / `WithOffset(TimeSpan)` / `WithDuration(TimeSpan)` / `WithMaxLastTextTokens(int)` / `WithAudioContextSize(int)` | threading/window | thread count + audio window |
|  [03]   | `WithMaxSegmentLength(int)` / `WithMaxTokensPerSegment(int)` / `SplitOnWord()` / `WithTokenTimestamps()` / `WithTokenTimestampsThreshold(float)` / `WithTokenTimestampsSumThreshold(float)` | segmentation/timestamps | caption chunking + per-token timing |
|  [04]   | `WithTemperature(float)` / `WithTemperatureInc(float)` / `WithMaxInitialTs(float)` / `WithLengthPenalty(float)` / `WithEntropyThreshold(float)` / `WithLogProbThreshold(float)` / `WithNoSpeechThreshold(float)` / `WithProbabilities()` | decoding thresholds | quality/confidence gates |
|  [05]   | `WithGreedySamplingStrategy(Action<GreedySamplingStrategyBuilder>)` (`WithBestOf(int)`) / `WithBeamSearchSamplingStrategy(Action<BeamSearchSamplingStrategyBuilder>)` (`WithBeamSize(int)`/`WithPatience(float)`) | sampling strategy | greedy vs beam-search sub-builders |
|  [06]   | `WithSegmentEventHandler(OnSegmentEventHandler)` / `WithProgressHandler(OnProgressHandler)` / `WithEncoderBeginHandler(OnEncoderBeginEventHandler)` | event handlers | push segment/progress/cancel callbacks |
|  [07]   | `WithStringPool(IStringPool?)` / `WithoutStringPool()` / `WithOpenVinoEncoder(string?, string?, string?)` / `WithPrintProgress()`/`WithPrintResults()`/`WithPrintTimestamps(bool)`/`WithPrintSpecialTokens()` | perf/debug | pooling, OpenVINO encoder, stdout tracing |
|  [08]   | `Build()`                                                                                      | terminal              | materialize the `WhisperProcessor` |

[VAD_AND_ASSETS]: Silero VAD pipeline + ggml download + runtime/log
- rail: caption

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT]              | [RAIL]  |
| :-----: | :------------------------------------------------------------------------ | :-------------------------- | :------ |
|  [01]   | `FromPath(string)` → `CreateBuilder()` → `Build()`                        | `WhisperVadFactory`         | load Silero-VAD + configure |
|  [02]   | `DetectSpeech(Stream/float[]/ReadOnlySpan<float>)` / `DetectSpeechAsync(Stream/float[]/ReadOnlyMemory<float>, CancellationToken)` / `…NoReset` / `ResetState()` | `WhisperVadProcessor` | segment speech spans (`VadSegmentData`) before transcription |
|  [03]   | `Default` / `GetGgmlModelAsync(GgmlType, QuantizationType, CancellationToken)` / `GetGgmlSileroVadModelAsync(SileroVadType, CancellationToken)` | `WhisperGgmlDownloader` | fetch model + VAD weights from HuggingFace |
|  [04]   | `RuntimeLibraryOrder` / `LibraryPath` / `LoadedLibrary`                    | `RuntimeOptions`            | native-runtime selection + probe |
|  [05]   | `AddConsoleLogging(WhisperLogLevel)` / `AddLogger(Action<WhisperLogLevel,string?>)` | `LogProvider`      | route native log lines |
|  [06]   | `GetStreamingTextAsync(Stream, SpeechToTextOptions?, CancellationToken)` / `GetTextAsync(...)` | `WhisperSpeechToTextClient` | the Microsoft.Extensions.AI streaming-caption fit |

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
