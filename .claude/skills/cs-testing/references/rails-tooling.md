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

---
## [2][MUTATION]

`scripts/mutate-cs.sh` restores local tools and runs `dotnet stryker` against the pure-managed `Rasm` project/test pair. It uses verified CLI options, VSTest, `Release`, `net10.0`, `html/json/progress` reporters, and thresholds `80/70/60`.

```bash
bash scripts/mutate-cs.sh --self-test
bash scripts/mutate-cs.sh
```

---
## [3][BRIDGE]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `bash scripts/rhino.sh verify apps/grasshopper/Radyab/Scenarios` | Run all `*.verify.csx` scenarios. |
| [2] | `bash scripts/rhino.sh bridge check-source <source.cs> --script <scenario.verify.csx>` | Prove scenario owns a source slice. |
| [3] | `bash scripts/rhino.sh bridge doctor` | Check active bridge session. |

Static specs must not fake this rail with skips, mocked Rhino native calls, or dylib copying.
