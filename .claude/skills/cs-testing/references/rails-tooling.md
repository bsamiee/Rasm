# [H1][RAILS_TOOLING]
>**Dictum:** *Each rail has one command path and one owner.*

<br>

## [1][MANAGED]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `dotnet restore Workspace.slnx --locked-mode` | Package graph proof. |
| [2] | `uv run python -m tools.quality static full` | Build/analyzer/source-generation proof. |
| [3] | `uv run python -m tools.quality test run` | Default managed target: MTP unit tests only. |
| [4] | `uv run python -m tools.quality test run --all --mutation off` | All runnable MTP test projects. |
| [5] | `uv run python -m tools.quality test coverage` | Managed coverlet.MTP map. |
| [6] | `uv run python -m tools.quality test run --mutation changed` | Changed-file Stryker MTP mutation. |
| [7] | `uv run python -m tools.quality test run --mutation full` | Full managed Stryker MTP mutation. |
| [8] | `uv run python -m tools.quality test run --target <project>` | Focused MTP project run. |

`quality static` and MTP test runs isolate MSBuild output under `.artifacts/quality/<rail>/<run-id>/`. Mutation is explicit and fail-fast on `.artifacts/locks/mutation.lock`.

---
## [2][MUTATION]

Stryker is owned by the pure-managed `Rasm` project/test pair. Thresholds are `95/90/85`; reporters are `html/json/progress`. Zero discovery fails the rail.

```bash
uv run python -m tools.quality self-test
uv run python -m tools.quality test run --mutation changed
```

---
## [3][BRIDGE]

| [INDEX] | [COMMAND] | [USE] |
| :-----: | --------- | ----- |
| [1] | `uv run python -m tools.quality bridge doctor` | Bridge health and endpoint proof. |
| [2] | `uv run python -m tools.quality bridge verify <scenario-or-glob>` | Runtime verification against live RhinoWIP. |
| [3] | `uv run python -m tools.quality api doctor` | Host assembly and reference map. |

Bridge dotnet routes and live Rhino remain single-flight. Override Rhino bundle via `QUALITY_RHINO_APP` or `RHINO_WIP_APP_PATH`.
