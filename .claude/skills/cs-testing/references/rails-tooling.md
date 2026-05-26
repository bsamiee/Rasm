# [H1][RAILS_TOOLING]
>**Dictum:** *Each rail has one command path and one owner.*

<br>

## [1][STATIC]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `dotnet restore Workspace.slnx --locked-mode` | Package graph proof. |
| [2] | `bash scripts/check-cs.sh full` | Build/analyzer/source-generation proof. |
| [3] | `bash scripts/test.sh` | Whole static test suite. |
| [4] | `TEST_TARGET=<project> bash scripts/test.sh` | Focused project run when supported by script. |
| [5] | `dotnet test tests/csharp/libs/Rasm/Rasm.Tests.csproj --configuration Release /p:CollectCoverage=true` | Managed coverage map. |
| [6] | `TEST_TARGET=tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj bash scripts/test.sh` | Focused GH2 managed specs. |
| [7] | `TEST_TARGET=tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj bash scripts/test.sh` | Focused Rhino managed specs. |
| [8] | `dotnet test tests/csharp/_architecture/Rasm.Architecture.Tests.csproj --configuration Debug` | Architecture boundary laws. |
| [9] | `dotnet test tests/csharp/_tooling/Rasm.TestingTools.Tests.csproj --configuration Release` | Stable testing-rail snapshots. |

---
## [2][MUTATION]

`scripts/mutate-cs.sh` restores local tools and runs `dotnet stryker` against the pure-managed `Rasm` project/test pair. It uses verified CLI options, VSTest, `Release`, `net10.0`, `html/json/progress` reporters, and thresholds `95/90/85`.

Current caveat: Stryker is diagnostic until it discovers non-zero tests under the current xUnit v3/VSTest rail. Use `1/1/1` thresholds for discovery proof because Stryker requires `threshold-low >= break-at`; keep the default managed target at `95/90/85` only after discovery is non-zero.

```bash
bash scripts/mutate-cs.sh --self-test
bash scripts/mutate-cs.sh
```

---
## [3][BRIDGE]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `bash scripts/rhino.sh verify tests/csharp/libs/Rasm/Vectors/scenarios` | Run vector-owned `*.verify.csx` scenarios. |
| [2] | `bash scripts/rhino.sh bridge check <source.cs> <scenario.verify.csx>` | Prove scenario owns a source slice. |
| [3] | `bash scripts/rhino.sh bridge doctor` | Check active bridge session. |
| [4] | `bash scripts/rhino.sh bridge clean <target>` | Clear generated reports for one target. |

Static specs must not fake this rail with skips, mocked Rhino native calls, dylib copying, scenario `#r` blocks, or hardcoded build-output paths.

---
## [4][ADJACENT_RAILS]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `dotnet run --project tests/csharp/_benchmarks/Rasm.Benchmarks.csproj --configuration Release -- --filter *` | BenchmarkDotNet hot-path measurements. |
| [2] | `dotnet run --project tests/csharp/_fuzz/Rasm.Fuzz.csproj --configuration Release` | SharpFuzz harness entrypoint after instrumentation/corpus setup. |

Benchmarks and fuzz harnesses are never default unit tests.
