# [H1][RAILS_TOOLING]
>**Dictum:** *Each rail has one command path and one owner.*

<br>

## [1][STATIC]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `dotnet restore Workspace.slnx --locked-mode` | Package graph proof. |
| [2] | `uv run python -m tools.quality static full` | Build/analyzer/source-generation proof. |
| [3] | `uv run python -m tools.quality test run` | Default managed target: VSTest then Stryker. Focused `--target`: VSTest only. |
| [4] | `uv run python -m tools.quality test run --target <project>` | Focused project run (VSTest only; skips mutation). |
| [5] | `dotnet test tests/csharp/libs/Rasm/Rasm.Tests.csproj --configuration Release /p:CollectCoverage=true` | Managed coverage map. |
| [6] | `uv run python -m tools.quality test run --target tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj` | Focused GH2 managed specs. |
| [7] | `uv run python -m tools.quality test run --target tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj` | Focused Rhino managed specs. |
| [8] | `dotnet test tests/csharp/_architecture/Rasm.Architecture.Tests.csproj --configuration Debug` | Architecture boundary laws. |
| [9] | `dotnet test tests/csharp/_tooling/Rasm.TestingTools.Tests.csproj --configuration Release` | Stable testing-rail snapshots. |

`quality static` and focused `--target` test runs isolate MSBuild output under `.artifacts/agents/<pid>/` per invocation. Default `tools.quality test run` adds Stryker after VSTest in one process.

---
## [2][MUTATION]

Default `tools.quality test run` runs VSTest, then `dotnet stryker` on the pure-managed `Rasm` project/test pair. Focused `--target` runs execute VSTest only. Thresholds are `95/90/85`; reporters are `html/json/progress`.

Current caveat: Stryker is diagnostic until it discovers non-zero tests under the current xUnit v3/VSTest rail. Stryker uses project `obj/`/`bin/` after the isolated VSTest pass — do not parallelize two default test runs with other dotnet gates. Use `1/1/1` thresholds only for Stryker discovery diagnostics when avoiding threshold failure.

```bash
uv run python -m tools.quality self-test
uv run python -m tools.quality test run
```

---
## [3][BRIDGE]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `uv run python -m tools.quality bridge doctor` | Bridge health and endpoint proof. |
| [2] | `uv run python -m tools.quality bridge verify <scenario-or-glob>` | Runtime verification against live RhinoWIP. |
| [3] | `uv run python -m tools.quality api doctor` | Host assembly and reference map. |

Bridge dotnet routes and live Rhino remain single-flight. Override Rhino bundle via `QUALITY_RHINO_APP` (quality operator) or `RHINO_WIP_APP_PATH` (bridge client launch contract).
