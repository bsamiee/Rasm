# Rasm Cursor Infrastructure Synthesis

This document captures the source-grounded decisions behind the final Cursor cleanup. It summarizes completed research waves without storing private transcript paths or raw logs.

## Source Categories

| Category | Sources Used |
| --- | --- |
| Cursor product behavior | Current Cursor docs synthesis for project rules, user rules, team rules, skills, subagents, hooks, MCP, and Bugbot. |
| Repo authority | `CLAUDE.md`, root `AGENTS.md`, nested `AGENTS.md`, `.claude/skills/`, `docs/system-api-map`, `docs/external-libs`, `docs/testing-libs`. |
| Build and host config | `Directory.Build.props`, `Directory.Packages.props`, `README.md`, `scripts/check-cs.sh`, `scripts/test.sh`, `scripts/rhino.sh`. |
| Bridge truth | `tools/rhino-bridge/README.md`, `tools/rhino-bridge/AGENTS.md`, `tests/csharp/AGENTS.md`, `Rasm.TestKit.Scenarios`, `BridgeMarker.Scan`. |
| Code audits | `libs/csharp/Rasm`, `Rasm.Rhino`, `Rasm.Grasshopper`, tests, and bridge operator surfaces. |
| Host research | Local RhinoWIP app-bundle XML/decompile, GH2 SDK notes, Eto/macOS UI constraints, RhinoCode diagnostic lane, Yak packaging lane. |

## Research Waves

- **Cursor docs wave:** Confirmed `.cursor/rules/*.mdc` project rules with `description`, `globs`, and `alwaysApply`; User Rules live in Settings as plain text; Team Rules outrank Project Rules, which outrank User Rules. Skills are multi-step procedures, but this repo should not define Cursor skills because `.claude/skills/` owns procedures.
- **Claude/Codex synthesis wave:** Durable Rasm guidance is greenfield polymorphic collapse, OOP native boundaries with FP internals, LanguageExt/Thinktecture typed rails, external-lib-first policy, and separated quality gates. Codex had stale bridge memory and secret-bearing config, so none of that was imported.
- **Repo config wave:** Rasm currently targets RhinoWIP on macOS, `.NET 10`, C# 14, GH2, Eto, central package management, and app-bundle host references. `global.json` is absent and should only be treated as a future trigger if added.
- **Bridge audit wave:** `scripts/rhino.sh` is the only operator. Current routes are `verify`, `bridge check`, `bridge doctor`, `api doctor|path|xml|types|decompile`, and `package|deploy|publish`. Scenarios use `Scenario.Run` plus `facts.Add`; the batched `facts={json}` marker parsed by `BridgeMarker.Scan` is the durable evidence channel.
- **Code audit wave:** Focused rules are useful for `Rasm/Analysis` + `Domain`, `Rasm/Vectors`, `Rasm.Grasshopper`, Rhino UI, and Rhino Exchange/Camera/Blocks because each has category-specific ownership and host-risk patterns.
- **Host research wave:** Local RhinoWIP XML/decompile is the highest-confidence API source. GH2 is WIP and local XML wins over older examples. Eto UI on macOS must be document-parented and UI-thread marshaled; `RhinoDoc.ActiveDoc` is not acceptable when command or UI context supplies a document.

## Decisions

- Replaced broad or stale project rules with one always-on router plus focused auto-attached rules.
- Removed Rasm Cursor skill guidance and kept `.claude/skills/` as the only procedural source.
- Added `.cursor/BUGBOT.md` for PR-review-specific checks because it is deterministic, non-executable, and supported by Cursor docs.
- Did not add hooks because no source-grounded low-risk deterministic guardrail exceeded existing repo gates and analyzer checks.
- Did not add MCP, env, permissions, or worktree config because no current task required them, and copying external config risks stale settings or secrets.

## Stale Guidance Removed

The rule pack rejects `Console.WriteLine("key=value")` scenario facts, `BridgeMarker.EmitFact`, `BridgeMarker.EmitScenarioHeader`, `load-smoke`, `StartScriptServer`, job JSON, `tests/rhino`, `pnpm check:cs`, user skill paths to `~/.cursor/rules/rasm`, GH1, Rhino 8, Windows assumptions, and `RhinoDoc.ActiveDoc` fallback when a command document exists.
