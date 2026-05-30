# [H1][SHARPFUZZ_API]
>**Dictum:** *Fuzz pure parsers and decoders, not Rhino runtime.*

<br>

[IMPORTANT] `SharpFuzz 2.2.0` is the latest observed package but is older than the preferred freshness window. Keep it isolated in `tests/csharp/_fuzz` behind harness proof and do not make it a normal test gate.

---
## [1][PACKAGE]
>**Dictum:** *Fuzzing is a separate executable rail.*

<br>

| [INDEX] | [PACKAGE]               | [PIN]   | [USE]                                   |
| :-----: | ----------------------- | ------- | --------------------------------------- |
|   [1]   | `SharpFuzz`             | `2.2.0` | In-process harness API.                 |
|   [2]   | `SharpFuzz.CommandLine` | `2.2.0` | Local `sharpfuzz` instrumentation tool. |

[SOURCE] NuGet package page: https://www.nuget.org/packages/SharpFuzz/2.2.0

---
## [2][SURFACE]
>**Dictum:** *Harnesses own corpus and crash semantics.*

<br>

| [INDEX] | [API]                                          | [RASM_USE]                                                              |
| :-----: | ---------------------------------------------- | ----------------------------------------------------------------------- |
|   [1]   | `Fuzzer.OutOfProcess.Run(Action<string>, int)` | String grammar/token harnesses with explicit input length bounds.       |
|   [2]   | `Fuzzer.OutOfProcess.Run(Action<Stream>)`      | Binary/stream decoder harnesses when string normalization is wrong.     |
|   [3]   | `Fuzzer.LibFuzzer.Run`                         | Span-based harnesses when native setup exists.                          |
|   [4]   | `RunAndIgnoreExceptions`                       | Only for explicitly non-crashing parsers.                               |
|   [5]   | corpus/crash dirs                              | File-based artifacts under `.artifacts/fuzz`.                           |
|   [6]   | instrumentation CLI                            | Opt-in script/harness flow; never required by `tools.quality test run`. |

---
## [3][RASM_SCOPE]
>**Dictum:** *Fuzz input grammars, not live hosts.*

<br>

Use SharpFuzz for pure managed parsers, token readers, import decoders, and resilient command-token surfaces. Do not fuzz RhinoDoc, GH document/canvas, viewport state, or app-bundle assembly loading.

Run the local harness directly after instrumentation/corpus setup:

```bash
dotnet run --project tests/csharp/_fuzz/Rasm.Fuzz.csproj --configuration Release
```
