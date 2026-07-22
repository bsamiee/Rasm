# [RASM_APPUI_API_WHISPER_NET]

`Whisper.net` owns offline speech-to-text for the LiveCaption rail: managed bindings over the native `whisper.cpp` ggml engine that load a model into a reusable `WhisperFactory`, configure one `WhisperProcessor` through a single polymorphic `With*` builder, and stream `SegmentData` as an `IAsyncEnumerable` for live emission. One in-model task translates recognition to English, and a separate Silero-VAD pipeline gates audio to speech spans before transcription.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Whisper.net`
- package: `Whisper.net` (MIT, Sandro Hanea)
- assembly: `Whisper.net`
- namespace: `Whisper.net`, `Whisper.net.Ggml`, `Whisper.net.LibraryLoader`, `Whisper.net.Logger`, `Whisper.net.Wave`
- abi: managed bindings over the native `whisper.cpp` ggml engine; the native runtime and the ggml weights load out-of-package
- depends: `Microsoft.Extensions.AI.Abstractions` (the `ISpeechToTextClient` seam)
- rail: caption

## [02]-[PUBLIC_TYPES]

[TRANSCRIPTION_CORE]: model handle, build fold, streaming processor, and typed failure rails.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `WhisperFactory`             | sealed class  | loaded ggml model handle (`IDisposable`) |
|  [02]   | `WhisperFactoryOptions`      | struct        | GPU, DTW, and alignment load options     |
|  [03]   | `WhisperProcessorBuilder`    | class         | the polymorphic `With*` build fold       |
|  [04]   | `WhisperProcessor`           | sealed class  | streaming handle (`IAsyncDisposable`)    |
|  [05]   | `WhisperSpeechToTextClient`  | sealed class  | `ISpeechToTextClient` caption seam       |
|  [06]   | `WhisperModelLoadException`  | exception     | model-load failure rail                  |
|  [07]   | `WhisperProcessingException` | exception     | processing failure rail                  |
|  [08]   | `IStringPool`                | interface     | native-string pooling seam               |

[SEGMENT_MODEL]: emitted caption rows, event delegates, and DTW alignment types.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------- |
|  [01]   | `SegmentData`                 | class         | emitted caption segment             |
|  [02]   | `WhisperToken`                | class         | per-token detail                    |
|  [03]   | `EncoderBeginData`            | class         | encoder-begin callback payload      |
|  [04]   | `OnSegmentEventHandler`       | delegate      | segment callback                    |
|  [05]   | `OnProgressHandler`           | delegate      | progress callback                   |
|  [06]   | `OnEncoderBeginEventHandler`  | delegate      | encoder-begin gate returning `bool` |
|  [07]   | `WhisperAbortEventHandler`    | delegate      | abort gate returning `bool`         |
|  [08]   | `WhisperAlignmentHeadsPreset` | enum          | DTW alignment-head preset           |
|  [09]   | `WhisperAlignmentHead`        | struct        | `(TextLayer, Head)` alignment pair  |

[WHISPER_ALIGNMENT_HEADS_PRESET]: `None` `NTopMost` `Custom` `TinyEn` `Tiny` `BaseEn` `Base` `SmallEn` `Small` `MediumEn` `Medium` `LargeV1` `LargeV2` `LargeV3` `LargeV3Turbo`

[VAD_PIPELINE]: Silero voice-activity detection, separate from transcription.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `WhisperVadFactory`          | sealed class  | loaded Silero-VAD model handle (`IDisposable`) |
|  [02]   | `WhisperVadProcessorBuilder` | sealed class  | threshold and window tuning fold               |
|  [03]   | `WhisperVadProcessor`        | sealed class  | speech-span detector (`IAsyncDisposable`)      |
|  [04]   | `VadSegmentData`             | sealed class  | detected speech span                           |

[ASSETS_AND_RUNTIME]: model download, native-backend selection, log subscription, and PCM decoding.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :---------------------- | :------------ | :----------------------------------- |
|  [01]   | `WhisperGgmlDownloader` | class         | ggml, VAD, and encoder model fetch   |
|  [02]   | `GgmlType`              | enum          | model variant                        |
|  [03]   | `QuantizationType`      | enum          | weight quantization                  |
|  [04]   | `SileroVadType`         | enum          | VAD model version                    |
|  [05]   | `RuntimeOptions`        | static class  | native-runtime selection             |
|  [06]   | `RuntimeLibrary`        | enum          | native backend                       |
|  [07]   | `LogProvider`           | static class  | native-log subscription              |
|  [08]   | `WhisperLogLevel`       | enum          | log severity                         |
|  [09]   | `WaveParser`            | sealed class  | 16 kHz PCM wave to `float[]` samples |

[GGML_TYPE]: `Tiny` `TinyEn` `Base` `BaseEn` `Small` `SmallEn` `Medium` `MediumEn` `LargeV1` `LargeV2` `LargeV3` `LargeV3Turbo`
[QUANTIZATION_TYPE]: `NoQuantization` `Q4_0` `Q4_1` `Q5_0` `Q5_1` `Q8_0`
[SILERO_VAD_TYPE]: `V5_1_2` `V6_2_0` (default `V6_2_0`)
[RUNTIME_LIBRARY]: `Cpu` `Cuda` `Cuda12` `Vulkan` `CoreML` `OpenVino` `CpuNoAvx`
[WHISPER_LOG_LEVEL]: `None` `Error` `Warning` `Info` `Cont` `Debug`

## [03]-[ENTRYPOINTS]

[MODEL_AND_STREAM]: `WhisperFactory` loads one model and reuses it; each `CreateBuilder()` fold yields one `WhisperProcessor` whose `Process*` methods take `audio` as a `Stream`, `float[]`, `ReadOnlyMemory<float>`, or `ReadOnlySpan<float>`, and every async surface carries a trailing `CancellationToken`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `WhisperFactory.FromPath(string, WhisperFactoryOptions?)`         | factory  | load model from disk               |
|  [02]   | `WhisperFactory.FromBuffer(Memory<byte>, WhisperFactoryOptions?)` | factory  | load model from memory             |
|  [03]   | `WhisperFactory.CreateBuilder()`                                  | instance | open the builder fold              |
|  [04]   | `WhisperFactory.GetSupportedLanguages()`                          | static   | supported-language roster          |
|  [05]   | `WhisperFactory.GetRuntimeInfo()`                                 | static   | native SIMD and acceleration probe |
|  [06]   | `WhisperProcessor.ProcessAsync(audio)`                            | instance | stream `SegmentData` live          |
|  [07]   | `WhisperProcessor.Process(audio)`                                 | instance | batch transcription                |
|  [08]   | `WhisperProcessor.DetectLanguage(float[])`                        | instance | detect language                    |
|  [09]   | `WhisperProcessor.DetectLanguageWithProbability(float[])`         | instance | scored language detect             |
|  [10]   | `WhisperProcessor.ChangeLanguage(string?)`                        | instance | mid-stream language switch         |
|  [11]   | `WhisperProcessor.Return(SegmentData)`                            | instance | return pooled strings              |
|  [12]   | `WhisperProcessor.DisposeAsync()`                                 | instance | async native teardown              |

- `WhisperProcessor.ProcessAsync`: returns `IAsyncEnumerable<SegmentData>`; `GetSupportedLanguages` returns `IEnumerable<string>` and `GetRuntimeInfo` a `string?`.
- `WhisperProcessor.DetectLanguageWithProbability`: returns `(string? language, float probability)`; a `(ReadOnlySpan<float>, params ReadOnlySpan<string>)` overload constrains detection to candidate languages.

[BUILDER_KNOBS]: every knob is a `With*` fold returning `WhisperProcessorBuilder`, `Build()` terminal; the LiveCaption path folds `WithLanguage("auto")` or `WithLanguageDetection()`, `WithTranslate()`, and `WithSegmentEventHandler`, tuned by the threshold and sampling rows.

[TASK_LANGUAGE]: `WithLanguage` `WithLanguageDetection` `WithTranslate` `WithPrompt` `WithCarryInitialPrompt` `WithNoContext` `WithSingleSegment` `WithSuppressRegex` `WithoutSuppressBlank`
[WINDOW]: `WithThreads` `WithOffset` `WithDuration` `WithMaxLastTextTokens` `WithAudioContextSize`
[SEGMENTATION]: `WithMaxSegmentLength` `WithMaxTokensPerSegment` `SplitOnWord` `WithTokenTimestamps` `WithTokenTimestampsThreshold` `WithTokenTimestampsSumThreshold`
[THRESHOLDS]: `WithTemperature` `WithTemperatureInc` `WithMaxInitialTs` `WithLengthPenalty` `WithEntropyThreshold` `WithLogProbThreshold` `WithNoSpeechThreshold` `WithProbabilities`
[SAMPLING]: `WithGreedySamplingStrategy(Action<GreedySamplingStrategyBuilder>)` carries `WithBestOf(int)`; `WithBeamSearchSamplingStrategy(Action<BeamSearchSamplingStrategyBuilder>)` carries `WithBeamSize(int)` and `WithPatience(float)`
[HANDLERS]: `WithSegmentEventHandler` `WithProgressHandler` `WithEncoderBeginHandler`
[TRACING]: `WithStringPool` `WithoutStringPool` `WithOpenVinoEncoder` `WithPrintProgress` `WithPrintResults` `WithPrintTimestamps` `WithPrintSpecialTokens`
[TERMINAL]: `Build() -> WhisperProcessor`

[VAD_AND_ASSETS]: `WhisperVadProcessor.DetectSpeech*` returns `IReadOnlyList<VadSegmentData>` over the same `audio` union and every async surface carries a trailing `CancellationToken`; the `NoReset` variants keep detector state across calls, and `WhisperGgmlDownloader.Default` fetches every model from HuggingFace.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `WhisperVadFactory.FromPath(string, WhisperFactoryOptions?)`                    | factory  | load Silero-VAD model                |
|  [02]   | `WhisperVadFactory.CreateBuilder()`                                             | instance | open the VAD tuning fold             |
|  [03]   | `WhisperVadProcessor.DetectSpeech(audio)`                                       | instance | speech spans, resets state           |
|  [04]   | `WhisperVadProcessor.DetectSpeechNoReset(audio)`                                | instance | speech spans, keeps state            |
|  [05]   | `WhisperVadProcessor.DetectSpeechAsync(audio)`                                  | instance | async speech spans                   |
|  [06]   | `WhisperVadProcessor.ResetState()`                                              | instance | clear detector state                 |
|  [07]   | `WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType, QuantizationType)`           | instance | fetch ggml weights                   |
|  [08]   | `WhisperGgmlDownloader.GetGgmlSileroVadModelAsync(SileroVadType)`               | instance | fetch Silero-VAD model               |
|  [09]   | `WhisperGgmlDownloader.GetEncoderCoreMLModelAsync(GgmlType)`                    | instance | fetch CoreML encoder                 |
|  [10]   | `WhisperGgmlDownloader.GetEncoderOpenVinoModelAsync(GgmlType)`                  | instance | fetch OpenVINO encoder               |
|  [11]   | `RuntimeOptions.RuntimeLibraryOrder`                                            | property | native-backend fallback order        |
|  [12]   | `LogProvider.AddLogger(Action<WhisperLogLevel, string?>)`                       | static   | subscribe native logs                |
|  [13]   | `WhisperSpeechToTextClient.GetStreamingTextAsync(Stream, SpeechToTextOptions?)` | instance | streaming `ISpeechToTextClient` seam |

[VAD_KNOBS]: `WithThreshold` `WithMinSpeechDuration` `WithMinSilenceDuration` `WithMaxSpeechDuration` `WithSpeechPadding` `WithSamplesOverlap` `WithThreads` `WithUseGpu` `WithGpuDevice` `Build()`

[CAPTION_RECEIPT]: the read surface a caption consumer projects off each emitted row.
- SegmentData: `Text` `Language` `Tokens : WhisperToken[]`; `Start`/`End : TimeSpan`; `Probability` `MinProbability` `MaxProbability` `NoSpeechProbability : float`
- WhisperToken: `Id` `TimestampId : int`; `Text : string?`; `Start`/`End`/`DtwTimestamp : long`; `VoiceLen` `Probability` `ProbabilityLog` `TimestampProbability` `TimestampProbabilitySum : float`
- VadSegmentData: `Start`/`End : TimeSpan`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every caption folds through one loaded `WhisperFactory`: `CreateBuilder()` configures one `WhisperProcessor` on the single `With*` builder, `ProcessAsync` streams `SegmentData` off one native context that `IDisposable`/`IAsyncDisposable` teardown frees, and optional Silero VAD gates audio to speech spans before the processor reads it.

[STACKING]:
- `api-messageformat`(`.api/api-messageformat.md`): `WithTranslate` emits English caption text while `MessageFormat` owns the CLDR plural/select formatting of the surrounding UI chrome — recognition and translation versus message formatting is the seam.
- `api-libmpv`(`.api/api-libmpv.md`), `api-ffmpeg-autogen`(`.api/api-ffmpeg-autogen.md`): `libmpv` decodes the media audio track, `Whisper.net` transcribes it, `FFmpeg.AutoGen` encodes the deliverable — decode, transcribe, and encode are three owners over the media spine.
- `WaveParser` converts a 16 kHz PCM `Stream` to the `float[]` samples `ProcessAsync` and `DetectSpeech` consume, and each `SegmentData` is a receipt row on the AppUi telemetry spine where a low-confidence segment (`NoSpeechProbability` and `Probability` gates) is a counted caption-quality fact.

[LOCAL_ADMISSION]:
- Offline speech-to-text intent loads one `WhisperFactory` per session and streams through `ProcessAsync`; the native `whisper.cpp` runtime rides a separate `Whisper.net.Runtime*` package selected by `RuntimeOptions.RuntimeLibraryOrder` (`CoreML` on Apple silicon), and the ggml weights and the Silero-VAD model download once through `WhisperGgmlDownloader.Default` and cache on disk. A missing runtime or model surfaces through `WhisperModelLoadException` and the `LogProvider` rail.

[RAIL_LAW]:
- Package: `Whisper.net`
- Owns: offline speech-to-text for LiveCaption — model load, the one `WhisperProcessorBuilder` `With*` fold, streaming `ProcessAsync` to `IAsyncEnumerable<SegmentData>`, in-model translate-to-English (`WithTranslate`), language auto-detect, DTW token timestamps, and Silero-VAD segmentation.
- Accept: one `WhisperFactory` per session streaming through `ProcessAsync`; task and tuning on the single builder; `WhisperVadProcessor.DetectSpeech*` gating to speech spans; `SegmentData.Start`/`End` `TimeSpan`s driving caption timing; `WhisperSpeechToTextClient.GetStreamingTextAsync` as the `ISpeechToTextClient` seam.
- Reject: a cloud STT dependency where offline `whisper.cpp` is the mandate; a `Get`/`Transcribe`/`Translate` operation family where the one builder plus `ProcessAsync` discriminate task by `With*` row and input by overload; a hand-rolled VAD beside the Silero pipeline; an inline translation of caption text beside `WithTranslate`; leaking the factory, processor, or VAD handles past their `IDisposable`/`IAsyncDisposable` teardown.
