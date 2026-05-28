# Rasm Cursor Infrastructure Synthesis

This document captures the source-grounded decisions behind the final Cursor cleanup. It summarizes completed research waves without storing private transcript paths or raw logs.

## Source Categories

| Category | Sources Used |
| --- | --- |
| Cursor product behavior | Current Cursor docs synthesis for project rules, user rules, team rules, skills, subagents, hooks, MCP, and Bugbot. |
| Repo authority | `CLAUDE.md`, root `AGENTS.md`, nested `AGENTS.md`, `.claude/skills/`, `docs/system-api-map`, `docs/external-libs`, `docs/testing-libs`. |
| Build and host config | `Directory.Build.props`, `Directory.Packages.props`, `README.md`, `tools/quality/`, `package.json` (`test:cs`, `verify:rhino`). |
| Bridge truth | `tools/rhino-bridge/README.md`, `tools/rhino-bridge/AGENTS.md`, `tests/csharp/AGENTS.md`, `Rasm.TestKit.Scenarios`, `BridgeMarker.Scan`. |
| Code audits | `libs/csharp/Rasm`, `Rasm.Rhino`, `Rasm.Grasshopper`, tests, and bridge operator surfaces. |
| Host research | Local RhinoWIP app-bundle XML/decompile, GH2 SDK notes, Eto/macOS UI constraints, RhinoCode diagnostic lane, Yak packaging lane. |

## Research Waves

- **Cursor docs wave:** Confirmed `.cursor/rules/*.mdc` project rules with `description`, `globs`, and `alwaysApply`; User Rules live in Settings as plain text; Team Rules outrank Project Rules, which outrank User Rules. Skills are multi-step procedures, but this repo should not define Cursor skills because `.claude/skills/` owns procedures.
- **Claude/Codex synthesis wave:** Durable Rasm guidance is greenfield polymorphic collapse, OOP native boundaries with FP internals, LanguageExt/Thinktecture typed rails, external-lib-first policy, and separated quality gates. Codex had stale bridge memory and secret-bearing config, so none of that was imported.
- **Repo config wave:** Rasm currently targets RhinoWIP on macOS, `.NET 10`, C# 14, GH2, Eto, central package management, and app-bundle host references. `global.json` is absent and should only be treated as a future trigger if added.
- **Bridge audit wave:** `uv run python -m tools.quality bridge` is the only operator. Current routes are `verify`, `bridge check`, `bridge doctor`, `api doctor|path|xml|types|decompile`, and `package|deploy|publish`. Scenarios use `Scenario.Run` plus `facts.Add`; the batched `facts={json}` marker parsed by `BridgeMarker.Scan` is the durable evidence channel.
- **Code audit wave:** Focused rules are useful for `Rasm/Analysis` + `Domain`, `Rasm/Vectors`, `Rasm.Materials`, `Rasm.Grasshopper`, Rhino UI, Rhino category folders, C# specs, testkit, bridge scenarios, and reference/architecture docs because each has specific ownership and host-risk patterns.
- **Host research wave:** Local RhinoWIP XML/decompile is the highest-confidence API source. GH2 is WIP and local XML wins over older examples. Eto UI on macOS must be document-parented and UI-thread marshaled; `RhinoDoc.ActiveDoc` is not acceptable when command or UI context supplies a document.

## Decisions

- Replaced broad or stale project rules with one always-on router plus focused auto-attached rules.
- Normalized Cursor activation semantics: always-loaded rules use `alwaysApply: true`; file-scoped rules use concrete `globs`; intelligent rules omit `globs` and rely on `description`.
- Removed Rasm Cursor skill guidance and kept `.claude/skills/` as the only procedural source.
- Added Materials, Rhino category, C# spec, testkit, bridge scenario, reference-doc, and architecture-doc rule surfaces.
- Added `.cursor/BUGBOT.md` for PR-review-specific checks because it is deterministic, non-executable, and supported by Cursor docs.
- Did not add hooks because no source-grounded low-risk deterministic guardrail exceeded existing repo gates and analyzer checks.
- Did not add MCP, env, permissions, or worktree config because no current task required them, and copying external config risks stale settings or secrets.

## Stale Guidance Removed

Canonical stale-bridge rejections live in `rasm-rhino-bridge.mdc`, bridge scenario emission bans live in `rasm-bridge-scenarios.mdc`, and PR-review checks live in `.cursor/BUGBOT.md`. Other rules reference current owners instead of copying reject lists.
