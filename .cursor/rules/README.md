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
|  [10]   | `docs/external-libs/**/*.md`             |
|  [11]   | `Directory.Packages.props`               |

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
- [10] `rasm-docs-reference`, `rasm-prose`
- [11] `rasm-dependencies`

## Intentional glob overlap

| [INDEX] | [OVERLAP]                                                    |
| :-----: | ------------------------------------------------------------ |
|   [1]   | `rasm-csharp-production` + domain rules                      |
|   [2]   | `tools/rhino-bridge/` (recursive) + `rasm-csharp-production` |
|   [3]   | `docs/` (recursive) + `rasm-prose` + `rasm-docs-*`           |

[REASON]
- [1] Production C# baseline plus folder-specific invariants
- [2] Bridge C# shares production C# law
- [3] Prose boundaries plus doc-type routing

Rules merge; they are not deduplicated. Keep each file focused to limit token load.

## Rule Index

| [INDEX] | [FILE]                       |
| :-----: | ---------------------------- |
|   [1]   | `rasm-core.mdc`              |
|   [2]   | `rasm-prose.mdc`             |
|   [3]   | `rasm-csharp-production.mdc` |
|   [4]   | `rasm-csharp-specs.mdc`      |
|   [5]   | `rasm-csharp-tests.mdc`      |
|   [6]   | `rasm-testkit.mdc`           |
|   [7]   | `rasm-bridge-scenarios.mdc`  |
|   [8]   | `rasm-rhino-bridge.mdc`      |
|   [9]   | `rasm-quality-operator.mdc`  |
|  [10]   | `rasm-docs-markdown.mdc`     |
|  [11]   | `rasm-docs-reference.mdc`    |
|  [12]   | `rasm-docs-architecture.mdc` |
|  [13]   | `rasm-dependencies.mdc`      |
|  [14]   | `rasm-analysis-domain.mdc`   |
|  [15]   | `rasm-vectors.mdc`           |
|  [16]   | `rasm-materials.mdc`         |
|  [17]   | `rasm-grasshopper.mdc`       |
|  [18]   | `rasm-rhino-ui.mdc`          |
|  [19]   | `rasm-rhino-categories.mdc`  |

[SCOPE]
- [1] Always-on router (~18 lines).
- [2] Prose boundaries for docs, manifests, `.mdc`.
- [3] Production C# under `libs/`, `apps/`, `tools/`.
- [4] xUnit/CsCheck law-matrix specs.
- [5] Test project and spec-folder guidance.
- [6] Shared `Rasm.TestKit` generators, oracles, and scenario harness.
- [7] Rhino/GH2 `*.verify.csx` runtime scenarios.
- [8] Bridge operator and runtime command routes.
- [9] `tools/quality` operator rails.
- [10] Markdown and Cursor docs hygiene.
- [11] System, external library, testing library, usage, and host reference docs.
- [12] Architecture, ADR, README, and changelog docs.
- [13] Package manifests and build props.
- [14] `Rasm/Analysis` plus `Rasm/Domain`.
- [15] `Rasm/Vectors`.
- [16] Host-free material catalogues.
- [17] GH2 components and UI boundary.
- [18] Rhino/GH2/Eto UI rails.
- [19] Rhino Commands, Capture, Construction, Exchange, Camera, Blocks.

**Count:** 19 `.mdc` files — 1 `alwaysApply`, 18 glob-scoped, 0 intelligent (description-only) rules.

## Unscoped paths (intentional)

No dedicated `.mdc` for: root `package.json`, `pyproject.toml`, `Workspace.slnx`, `.github/**` — use `CLAUDE.md`, root `AGENTS.md`, and domain rules when editing adjacent code. Add a glob rule only when repeated agent mistakes justify it.

## Non-Rule Artifacts

| [INDEX] | [PATH]                                           |
| :-----: | ------------------------------------------------ |
|   [1]   | `.cursor/BUGBOT.md`                              |
|   [2]   | `.cursor/bugbot/stale-rejections.md`             |
|   [3]   | Nested `.cursor/BUGBOT.md`                       |
|   [4]   | `.cursor/plans/`                                 |
|   [5]   | `.cursor/research/rasm-cursor-infrastructure.md` |

[ROLE]
- [1] PR review hub — severity, tone, links
- [2] Canonical stale-pattern list
- [3] Walk-up merge under `tools/rhino-bridge/`, `tests/csharp/`, `libs/csharp/Rasm.Rhino/`, `libs/csharp/Rasm.Grasshopper/`
- [4] Optional Plan Mode artifacts after Save to workspace
- [5] Source-grounded synthesis for this rule pack

Bugbot does **not** auto-load `.mdc` rules — link from `BUGBOT.md` when PR review needs them.

## Deferred (by design)

No root `.cursorignore` (rely on `.gitignore`), no `hooks.json`, `permissions.json`, `.cursor/agents/`, or `.cursor/commands/`.
