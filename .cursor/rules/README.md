# Rasm Cursor Rules

Project rules for the Rasm monorepo live in `.cursor/rules/*.mdc`. They are concise routing and invariant files; canonical procedures remain in `CLAUDE.md`, `AGENTS.md`, nested `AGENTS.md`, `.claude/skills/`, `docs/`, and bridge docs.

## Activation

| [INDEX] | [MODE]         | [WHERE]                             |
| :-----: | -------------- | ----------------------------------- |
|   [1]   | Always         | `rasm-core.mdc`                     |
|   [2]   | Auto attached  | Rule `globs`                        |
|   [3]   | Apply manually | Omit both `description` and `globs` |
|   [4]   | User Rules     | Cursor Settings                     |

[USE]
- [1] Slim router: read ladder, skill location, quality pointer.
- [2] File-family guidance when matching files are open.
- [3] Load only by explicit `@rule` mention.
- [4] Optional personal bootstrap from `user-rules-bootstrap.txt`.

Cursor precedence is Team > Project > User. This repo does not define Cursor skills because `.claude/skills/` owns full procedures.

**Verify rule packs:** Cmd+Shift+P → **Show Active Rules** with no files open — expect slim `rasm-core` plus `CLAUDE.md` context, not duplicated skill tables.

## Show Active Rules matrix

After editing `.mdc` frontmatter, open the fixture file and confirm active glob rules (plus always-on `rasm-core`).

| [INDEX] | [FIXTURE]                                |
| :-----: | ---------------------------------------- |
|   [1]   | *(no files open)*                        |
|   [2]   | `libs/csharp/Rasm/Domain/*.cs`           |
|   [3]   | `libs/csharp/Rasm/Vectors/*.cs`          |
|   [4]   | `libs/csharp/Rasm.Rhino/Commands/*.cs`   |
|   [5]   | `libs/csharp/Rasm.Grasshopper/UI/*.cs`   |
|   [6]   | `tests/csharp/_testkit/*.cs`             |
|   [7]   | `tests/csharp/**/scenarios/*.verify.csx` |
|   [8]   | `tools/rhino-bridge/**/*.cs`             |
|   [9]   | `tools/quality/**/*.py`                  |
|  [10]   | `Directory.Packages.props`               |

[RULES]
- [1] `rasm-core` only
- [2] `rasm-csharp-production`, `rasm-analysis-domain`
- [3] `rasm-csharp-production`, `rasm-vectors`
- [4] `rasm-csharp-production`, `rasm-rhino-categories`
- [5] `rasm-csharp-production`, `rasm-grasshopper`, `rasm-rhino-ui`
- [6] `rasm-csharp-tests`, `rasm-testkit`
- [7] `rasm-csharp-tests`, `rasm-bridge-scenarios`
- [8] `rasm-csharp-production`, `rasm-rhino-bridge`
- [9] `rasm-quality-operator`
- [10] `rasm-dependencies`

## Intentional glob overlap

| [INDEX] | [OVERLAP]                                                    |
| :-----: | ------------------------------------------------------------ |
|   [1]   | `rasm-csharp-production` + domain rules                      |
|   [2]   | `tools/rhino-bridge/` (recursive) + `rasm-csharp-production` |

[REASON]
- [1] Production C# baseline plus folder-specific invariants
- [2] Bridge C# shares production C# law

Rules merge; they are not deduplicated. Keep each file focused to limit token load.

## Rule Index

| [INDEX] | [FILE]                       |
| :-----: | ---------------------------- |
|   [1]   | `rasm-core.mdc`              |
|   [2]   | `rasm-csharp-production.mdc` |
|   [3]   | `rasm-csharp-specs.mdc`      |
|   [4]   | `rasm-csharp-tests.mdc`      |
|   [5]   | `rasm-testkit.mdc`           |
|   [6]   | `rasm-bridge-scenarios.mdc`  |
|   [7]   | `rasm-rhino-bridge.mdc`      |
|   [8]   | `rasm-quality-operator.mdc`  |
|   [9]   | `rasm-dependencies.mdc`      |
|  [10]   | `rasm-analysis-domain.mdc`   |
|  [11]   | `rasm-vectors.mdc`           |
|  [12]   | `rasm-materials.mdc`         |
|  [13]   | `rasm-grasshopper.mdc`       |
|  [14]   | `rasm-rhino-ui.mdc`          |
|  [15]   | `rasm-rhino-categories.mdc`  |

[SCOPE]
- [1] Always-on router (~18 lines).
- [2] Production C# under `libs/`, `apps/`, `tools/`.
- [3] xUnit/CsCheck law-matrix specs.
- [4] Test project and spec-folder guidance.
- [5] Shared `Rasm.TestKit` generators, oracles, and scenario harness.
- [6] Rhino/GH2 `*.verify.csx` runtime scenarios.
- [7] Bridge operator and runtime command routes.
- [8] `tools/quality` operator rails.
- [9] Package manifests and build props.
- [10] `Rasm/Analysis` plus `Rasm/Domain`.
- [11] `Rasm/Vectors`.
- [12] Host-free material catalogues.
- [13] GH2 components and UI boundary.
- [14] Rhino/GH2/Eto UI rails.
- [15] Rhino Commands, Capture, Construction, Exchange, Camera, Blocks.

**Count:** 15 `.mdc` files — 1 `alwaysApply`, 14 glob-scoped, 0 intelligent (description-only) rules.

## Unscoped paths (intentional)

No dedicated `.mdc` for: `docs/**`, `**/*.md`, root `package.json`, `pyproject.toml`, `Workspace.slnx`, `.github/**` — use `CLAUDE.md`, root `AGENTS.md` §5, `docs/standards/README.md`, and domain rules when editing adjacent code. Add a glob rule only when repeated agent mistakes justify it.

## Non-Rule Artifacts

| [INDEX] | [PATH]              |
| :-----: | ------------------- |
|   [1]   | `.cursor/BUGBOT.md` |

[ROLE]
- [1] PR review hub — severity, tone, stale rejections, scoped `AGENTS.md` / `.mdc` owners

Bugbot does **not** auto-load `.mdc` rules — link from `BUGBOT.md` when PR review needs them.

## Deferred (by design)

No root `.cursorignore` (rely on `.gitignore`), no `hooks.json`, `permissions.json`, `.cursor/agents/`, or `.cursor/commands/`.
