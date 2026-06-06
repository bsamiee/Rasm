# [SHARPFUZZ_API]

[IMPORTANT] `SharpFuzz` is isolated in `tests/csharp/_fuzz` behind harness proof and does not run in the normal managed test gate.

## [1][SURFACE]

| [INDEX] | [API]                                          | [PROJECT_USE]                                                           |
| :-----: | ---------------------------------------------- | ----------------------------------------------------------------------- |
|   [1]   | `Fuzzer.OutOfProcess.Run(Action<string>, int)` | String grammar/token harnesses with explicit input length bounds.       |
|   [2]   | `Fuzzer.OutOfProcess.Run(Action<Stream>)`      | Binary/stream decoder harnesses when string normalization is wrong.     |
|   [3]   | `Fuzzer.LibFuzzer.Run`                         | Span-based harnesses when native setup exists.                          |
|   [4]   | `RunAndIgnoreExceptions`                       | Only for explicitly non-crashing parsers.                               |
|   [5]   | corpus/crash dirs                              | File-based artifacts under `.artifacts/fuzz`.                           |
|   [6]   | instrumentation CLI                            | Not in the local tool manifest until a first-class fuzz rail needs it. |

## [2][PROJECT_SCOPE]

Use SharpFuzz for pure managed parsers, token readers, import decoders, and resilient command-token surfaces. Do not fuzz host-native documents, UI surfaces, viewport state, or app-bundle assembly loading.

Run the local harness directly after instrumentation/corpus setup:

```bash
dotnet run --project <fuzz-project> --configuration Release
```
