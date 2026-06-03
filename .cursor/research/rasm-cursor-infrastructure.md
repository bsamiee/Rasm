# Rasm Cursor Infrastructure Synthesis

This document captures the source-grounded decisions behind the Cursor cleanup. It summarizes completed research waves without storing private transcript paths or raw logs.

## Source Categories

| [INDEX] | [CATEGORY]              |
| :-----: | ----------------------- |
|   [1]   | Cursor product behavior |
|   [2]   | Repo authority          |
|   [3]   | Build and host config   |
|   [4]   | Bridge truth            |

[SOURCES]
- [1] [Rules](https://cursor.com/docs/rules.md), [Bugbot](https://cursor.com/docs/bugbot.md), [Plan Mode](https://cursor.com/docs/agent/plan-mode.md); subagents/hooks/MCP docs deferred for repo
- [2] `CLAUDE.md`, root `AGENTS.md`, nested `AGENTS.md`, `.claude/skills/`, `docs/` corpora
- [3] `Directory.Build.props`, `Directory.Packages.props`, `tools/quality/`, `package.json`
- [4] `tools/rhino-bridge/README.md`, `tools/rhino-bridge/AGENTS.md`, `tests/csharp/AGENTS.md`

## Instruction stack

```text
CLAUDE.md (always on in Cursor)
  → root AGENTS.md
    → nearest nested AGENTS.md
      → matched .cursor/rules/*.mdc (Agent chat only)
        → .claude/skills/ by file type
PR review: .cursor/BUGBOT.md (separate merge order; does not read .mdc unless linked)
```

## Rule pack decisions

- **One** `alwaysApply: true` router (`rasm-core.mdc` ~18 body lines); **18** glob-scoped domain rules; **zero** intelligent (description-only) rules.
- `rasm-prose.mdc` owns reference-doc prose boundaries; `docs/standards/voice.md` is canonical voice.
- Procedures live in `.claude/skills/` only — no `.cursor/skills/`.
- Flat `.cursor/rules/` only — nested `rules/` subdirectories are not scanned by Cursor.

## Bugbot architecture (2026)

Bugbot is Cursor’s **PR review product** — not IDE Agent rules.

| [INDEX] | [SURFACE]                 |
| :-----: | ------------------------- |
|   [1]   | `tools/cs-analyzer` (CSP) |
|   [2]   | `tools.quality` / CI      |
|   [3]   | `.cursor/BUGBOT.md`       |

[OWNS]
- [1] Style, FP law, analyzer violations
- [2] Build, test, bridge verify
- [3] Behavioral regressions, stale vocabulary, rail misuse, host mistakes

**Merge order:** Team Rules → dashboard learned/manual → `.cursor/BUGBOT.md` → User Rules.

**Repo layout:**

```text
.cursor/BUGBOT.md                    # Hub: severity, tone, scoped owners, links
.cursor/bugbot/stale-rejections.md    # Single canonical stale list
```

Scoped PR areas map to nearest `AGENTS.md` and glob `.mdc` owners listed in `BUGBOT.md`; do not duplicate full rule bodies.

## Explicitly deferred

| [INDEX] | [CAPABILITY]                                   |
| :-----: | ---------------------------------------------- |
|   [1]   | Root `.cursorignore` / `.cursorindexingignore` |
|   [2]   | `hooks.json`, `permissions.json`               |
|   [3]   | `.cursor/agents/`, `.cursor/commands/`         |
|   [4]   | MCP / worktrees / `environment.json`           |

[RATIONALE]
- [1] Rely on existing `.gitignore`; no extra root ignore files
- [2] Analyzers + `tools.quality` + BUGBOT cover enforcement posture
- [3] Procedures in `.claude/skills/`; no duplicate Cursor skills
- [4] No current team need; avoids stale or secret-bearing config

## Plans convention

Plan Mode saves to user home by default. **Save to workspace** commits artifacts under `.cursor/plans/` for team-visible architecture notes. Keep plans ≤1–2 pages per `CLAUDE.md` §5.3.

## Stale guidance routing

| [INDEX] | [CONCERN]                      |
| :-----: | ------------------------------ |
|   [1]   | IDE Agent routing + invariants |
|   [2]   | PR behavioral + stale patterns |
|   [3]   | Bridge operator law            |

[OWNER]
- [1] `.cursor/rules/*.mdc`
- [2] `.cursor/BUGBOT.md` + `bugbot/stale-rejections.md`
- [3] `tools/rhino-bridge/AGENTS.md`, `rasm-rhino-bridge.mdc`, `rasm-bridge-scenarios.mdc`

## Research waves (historical)

- Cursor docs wave: `.mdc` frontmatter, Team > Project > User precedence, no Cursor skills in repo.
- Bridge audit: `uv run python -m tools.quality bridge` as sole operator; batched `facts={json}` via `BridgeMarker.Scan`.
- Code audit: glob rules per host-risk surface (Vectors, Materials, Grasshopper, Rhino categories, specs, bridge scenarios, docs corpora).
