# [RASM_WORKSPACE]

Rasm is a polyglot monorepo whose `libs/` estates hold independently adoptable C#, Python, and TypeScript capability, and whose apps, plugins, and services compose those estates exactly as they take an external package.

## [01]-[MAP]

| [INDEX] | [TREE]  | [HOLDS]                                                                                  |
| :-----: | :------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `libs`  | Three independently adoptable language estates beside the cross-`libs/` planning core    |
|  [02]   | `apps`  | App-keyed islands; each `apps/<app-name>/` holds one app's projects across any languages |
|  [03]   | `tools` | Repo operators and architecture-pressure surfaces, each charter-owned by its own README  |
|  [04]   | `tests` | C#, Python, and TypeScript suites beside the cross-language contract corpus              |
|  [05]   | `docs`  | Durable doctrine: standards and per-language stacks                                      |

## [02]-[TOOL_OWNERS]

| [INDEX] | [TOOL]               | [ROLE]                                                                        |
| :-----: | :------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `tools/assay`        | Typed operator running every quality claim; its `--help` owns the roster.     |
|  [02]   | `tools/rhino-bridge` | Live Rhino owner: host lifecycle, scenario execution, cargo, spool, evidence. |
|  [03]   | `tools/cs-analyzer`  | Local Roslyn diagnostics carrying repeated C# source-shape laws.              |
|  [04]   | `tools/biome`        | Promoted GritQL lint rules the root `biome.json` registers at error.          |
|  [05]   | `tools/yak`          | Tracked Yak package manifests, one per package slug.                          |
|  [06]   | `Parametric_Forge`   | Sibling repo owning machine composition, executables, and credential policy.  |

[OUTPUT_ROUTING]:
- Every root, tool, and library routes generated output through an owned store — `.artifacts`, `.cache`, staging roots, or owner-declared state files.
- Root scratch output is a defect repairing at the writing tool's own configuration, never through a wrapper relocating the write after the fact.

## [03]-[OPERATOR]

`uv run assay <claim> <verb>` is the one runnable form: one JSON `Envelope` lands on stdout and diagnostics ride stderr. Claims, verbs, and flags live in `uv run assay --help` and each claim's own help.

```bash copy-safe
uv run assay self-test
```

- `uv run --no-sync assay <claim> <verb>` is the interactive fast path; the gate form resyncs first.
- `tools/assay/README.md` owns claim scope, flag semantics, the automation arm, and the per-claim evidence contract.
- Every claim runs from the repo root, since a wrong working directory fabricates a verdict instead of failing.

## [04]-[NEW_PROJECT]

`apps/README.md` owns island layout, host admission, the substrate every app composes, and the blessed per-project file set.

- `uv run assay init python-app apps/<app-name>/<project>` mints a Python app project and appends its explicit workspace member row.
- `uv run assay init python-lib libs/python/<name>` mints a library member with its `rasm` namespace seat and its suite conftest.
- `uv run assay init check` proves the workspace member census both ways across every governed tree.
- C# and TypeScript projects mint by hand from root-composed presets, under the guards `apps/README.md` names.

## [05]-[ROUTERS]

| [INDEX] | [SURFACE]                        | [OWNS]                                                                            |
| :-----: | :------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `CLAUDE.md`                      | Agent constitution: doc topology, implementation standards, tool and lane routing |
|  [02]   | `AGENTS.md`                      | Session load order and the read-only planning-corpus engineering contract         |
|  [03]   | `docs/README.md`                 | Doctrine router across standards and stacks                                       |
|  [04]   | `libs/.planning/ARCHITECTURE.md` | Stratification law, dependency direction, consumption model, admission ladder     |
|  [05]   | `libs/.planning/README.md`       | Planning doc-set per tier, index-doc contracts, and design-page grammar           |
|  [06]   | `tests/README.md`                | Proof-estate law across every language suite and the contract corpus              |
|  [07]   | `apps/README.md`                 | App-keyed island layout, open host roster, and the substrate an app composes      |
|  [08]   | Reviewer configs                 | `.coderabbit.yaml`, `.greptile/`, `.macroscope/` carry reviewer tone and scope    |
