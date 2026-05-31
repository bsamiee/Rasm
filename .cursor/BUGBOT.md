# Rasm Bugbot Guidance

PR review only — behavioral regressions, host-boundary mistakes, missing validation, and command drift. Defer formatting and CSP violations to `tools.quality static check|build`; do not nitpick grammar on docs.

## Context

RhinoWIP on macOS, `.NET 10`, C# 14, GH2, Eto, central package management. Three orthogonal quality rails:

| [INDEX] | [RAIL]          |
| :-----: | --------------- |
|   [1]   | Static C#       |
|   [2]   | Unit + mutation |
|   [3]   | Rhino runtime   |

[COMMAND]
- [1] `uv run python -m tools.quality static check|build`
- [2] `uv run python -m tools.quality test run`
- [3] `uv run python -m tools.quality bridge verify <scenario-or-glob>`

Full gate policy: [`CLAUDE.md`](../CLAUDE.md) §5.2, [`docs/usage.md`](../docs/usage.md).

## Severity

**Blocking (request changes):**

- Non-canonical bridge or quality commands — see [stale-rejections.md](bugbot/stale-rejections.md)
- `#r`, `#load`, or absolute paths in `*.verify.csx`
- Conflating static, test, and bridge rails in one invocation
- Secrets or credentials in committed config

**Advisory:**

- Rhino/GH/Eto surface change without touched `*.verify.csx` or scenario when runtime behavior is claimed
- feat/fix without regression spec or scenario where appropriate
- Reference doc agent-routing strings outside `docs/standards/**`

## What to leave alone

- `.artifacts/**`, Verify snapshots, Stryker reports, mutation corpora
- Lockfile-only PRs without dependency semantic change
- `docs/standards/**` voice authority
- Manifest routing in `AGENTS.md` / `CLAUDE.md`

## Tone

Name file and line. Suggest the proving command (`static check`, `static build`, `test run`, `bridge verify`). No vague “consider adding tests” without naming the rail.

## Scoped review (nested)

| [INDEX] | [AREA]                      |
| :-----: | --------------------------- |
|   [1]   | Bridge operator + scenarios |
|   [2]   | Specs + testkit             |
|   [3]   | Rhino host boundary         |
|   [4]   | GH2 boundary                |

[FILE]
- [1] [tools/rhino-bridge/.cursor/BUGBOT.md](../tools/rhino-bridge/.cursor/BUGBOT.md)
- [2] [tests/csharp/.cursor/BUGBOT.md](../tests/csharp/.cursor/BUGBOT.md)
- [3] [libs/csharp/Rasm.Rhino/.cursor/BUGBOT.md](../libs/csharp/Rasm.Rhino/.cursor/BUGBOT.md)
- [4] [libs/csharp/Rasm.Grasshopper/.cursor/BUGBOT.md](../libs/csharp/Rasm.Grasshopper/.cursor/BUGBOT.md)

## Rule owners (link only — Bugbot does not auto-load `.mdc`)

- [Bridge scenarios](rules/rasm-bridge-scenarios.mdc)
- [Bridge operator](rules/rasm-rhino-bridge.mdc)
- [Quality operator](rules/rasm-quality-operator.mdc)

## Canonical Guidance

Canonical list: [bugbot/stale-rejections.md](bugbot/stale-rejections.md)

## Cross-cutting checks

- C# changes preserve polymorphic collapse, typed rails, and category owners — no helpers, wrappers, shims, or parallel APIs
- Dependencies follow central package management and approved docs before local reinvention
