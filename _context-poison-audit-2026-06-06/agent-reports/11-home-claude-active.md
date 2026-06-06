# Home Claude Active Instruction Audit

Auditor: K
Date: 2026-06-06
Scope: `/Users/bardiasamiee/.claude` active user-authored instruction surfaces.
Mode: read-only except this report.

## Executive Result

The active user-authored `.claude` instruction surface is small but high-impact:

- `/Users/bardiasamiee/.claude/CLAUDE.md`
- `/Users/bardiasamiee/.claude/rules/output-conventions.md`
- `/Users/bardiasamiee/.claude/rules/research-protocol.md`
- `/Users/bardiasamiee/.claude/settings.json`, read only for load/plugin evidence

No active top-level markdown/text files were found under `/Users/bardiasamiee/.claude/commands`, `/Users/bardiasamiee/.claude/agents`, or `/Users/bardiasamiee/.claude/hooks`; `/Users/bardiasamiee/.claude/prompts` and `/Users/bardiasamiee/.claude/settings` do not exist. `/Users/bardiasamiee/.claude/skills` contains benchmark artifacts, not a loadable `SKILL.md`, but the placement is still risky because project policy can treat `.claude/skills/*` as skill context.

Primary poisoning pattern: `.claude/CLAUDE.md` duplicates and sometimes conflicts with repo-local `CLAUDE.md` / `AGENTS.md` rules for output, plans, research freshness, quality gates, greenfield refactors, documentation hygiene, and delegation. The global file is acting like a repo policy mirror instead of a small user preference layer.

## Active Surfaces Read

| Surface | Status | Evidence |
| :-- | :-- | :-- |
| `/Users/bardiasamiee/.claude/CLAUDE.md` | Active user-authored instruction file read fully | 59 lines, global operator preferences |
| `/Users/bardiasamiee/.claude/rules/output-conventions.md` | Active user-authored rule read fully | 9 lines |
| `/Users/bardiasamiee/.claude/rules/research-protocol.md` | Active user-authored rule read fully | 9 lines |
| `/Users/bardiasamiee/.claude/settings.json` | Active config read for load evidence | enables skills, slash commands, agents, workflows, plugins at lines 13-64 |
| `/Users/bardiasamiee/.claude/skills/coding-pg-workspace/iteration-2/benchmark.md` | Read because `skills/**` exists; classified as benchmark/archive, not a skill definition | no `SKILL.md` under `.claude/skills`; benchmark lines 1-50 |

## Archival And Vendor Inventory

| Area | Inventory | Classification |
| :-- | :-- | :-- |
| `/Users/bardiasamiee/.claude/plans` | 345 files: 344 Markdown, 1 JS | archival/generated plan and prompt history; not exhaustively read |
| `/Users/bardiasamiee/.claude/plugins/cache` | 74 files, 59 dirs, 38 Markdown, 1 text | vendor plugin cache; not user-authored |
| `/Users/bardiasamiee/.claude/plugins/marketplaces` | 524 files, 285 dirs, 298 Markdown, 1 text | vendor marketplace clones; not user-authored |
| `/Users/bardiasamiee/.claude/cache` | `changelog.md`, `my-closed-issues.json` | cache; not active instruction by observed load evidence |
| `/Users/bardiasamiee/.claude/projects`, `tasks`, `paste-cache`, `file-history`, `session-env`, `telemetry` | large session/history/cache areas inventoried by counts only | archival/session material, excluded from active instruction audit |

Plugin load evidence:

- `/Users/bardiasamiee/.claude/settings.json:44-55` enables `skill-creator`, `claude-code-setup`, and several LSP plugins; disables `coderabbit`, `math-olympiad`, and `frontend-design`.
- `/Users/bardiasamiee/.claude/plugins/installed_plugins.json:4-110` lists installed cache paths.
- `/Users/bardiasamiee/.claude/plugins/known_marketplaces.json:2-26` lists marketplace clones and update timestamps.

## Findings

### F1. Global `.claude/CLAUDE.md` Duplicates Repo-Owned Policy

Severity: high

The global file restates rules already owned by the active Rasm repo instruction chain:

- Plan shape: `/Users/bardiasamiee/.claude/CLAUDE.md:43-47` mirrors `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:134-140`.
- Output structure: `/Users/bardiasamiee/.claude/CLAUDE.md:39-41` and `/Users/bardiasamiee/.claude/rules/output-conventions.md:3-9` overlap `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:89-95`.
- Research freshness: `/Users/bardiasamiee/.claude/CLAUDE.md:28-32` and `/Users/bardiasamiee/.claude/rules/research-protocol.md:3-9` overlap `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:34-38`.
- Documentation hygiene: `/Users/bardiasamiee/.claude/CLAUDE.md:34-37` duplicates `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:75-92`.
- Greenfield/refactor posture: `/Users/bardiasamiee/.claude/CLAUDE.md:49-56` overlaps `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:58-87` and `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:36-52`.
- Quality gates: `/Users/bardiasamiee/.claude/CLAUDE.md:58-59` mirrors the repo-owned detailed rails at `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:114-132`.

Why this poisons context: the global file consumes budget on policy that the repo already owns and turns repo-specific norms into ambient user preference. When a non-Rasm repo is active, these rules travel incorrectly; when Rasm is active, they create multiple authority layers for the same decision.

Correction target: shrink global `.claude/CLAUDE.md` to stable user preferences only, and let repo `CLAUDE.md` / `AGENTS.md` own repo policy.

### F2. Research Freshness Windows Conflict

Severity: high

Conflicting active rules:

- Global latest-stable assumption: `/Users/bardiasamiee/.claude/CLAUDE.md:28-32`.
- Global rule file: sources must be `<=6 months old` and new technical content requires `2025+` sources at `/Users/bardiasamiee/.claude/rules/research-protocol.md:3-7`.
- Rasm repo rule: sources must be within `last 9 months` unless stable official docs are the only primary source at `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:34-35`.
- Current task prompt's global execution policy says research should source information within the last `3-4 months`.

Why this poisons context: an agent must spend reasoning budget reconciling 3-4 months, 6 months, 9 months, 2025+, latest stable, and project pins. That is not a useful policy stack; it is a freshness conflict.

Correction target: keep only one freshness rule per active authority layer. Global user preference can say "use current primary sources for freshness-sensitive facts"; repo manifests should own exact windows.

### F3. Global Response Footer Rule Conflicts With Concision

Severity: medium

Evidence:

- `/Users/bardiasamiee/.claude/CLAUDE.md:39-41` requires every substantive response to end with `Further Considerations`.
- `/Users/bardiasamiee/.claude/rules/output-conventions.md:7-9` says every paragraph must change action, no preamble, and no sign-off summaries.
- `/Users/bardiasamiee/.claude/CLAUDE.md:3` also demands minimized token waste.

Why this poisons context: a fixed footer is process-form boilerplate. It forces extra sections even when the answer is a one-line command result, a direct status update, or a final implementation summary.

Correction target: delete the mandatory footer. If advanced caveats matter, they should appear only when they change the decision.

### F4. Overbroad "No Control Flow" Rule Escapes Domain Logic

Severity: high

Evidence:

- `/Users/bardiasamiee/.claude/CLAUDE.md:9-15` bans `if` / `else` / `for` / `while` / `switch` / `try` / `catch` "across all languages" and requires functional programming exclusively.
- Repo policy is narrower: `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:60-64` forbids imperative branching, mutable accumulation, and exception-style control flow in domain logic.

Why this poisons context: the global rule makes adapters, CLIs, scripts, generated code, tests, glue code, and one-off operational logic appear invalid even when language skills or repo overlays allow pragmatic boundary exceptions. It also conflicts with the user's stated audit need, where shell control flow was useful for inventory.

Correction target: move language mechanics to actual skills and keep the global preference as a bias, not an absolute. Repo policy can keep the stricter domain-logic formulation.

### F5. Subagent Delegation Boilerplate Is Fixed Process Narration

Severity: medium

Evidence:

- `/Users/bardiasamiee/.claude/CLAUDE.md:21-26` requires subagents for bounded subtasks and mandates delegation when reading more than three files.
- `/Users/bardiasamiee/.claude/settings.json:26-33` allows `Agent`, `TaskCreate`, `TaskUpdate`, `TaskList`, `TaskGet`, `TaskOutput`, and `TaskStop`, but the instruction still encodes a fixed process rather than a result contract.

Why this poisons context: "more than 3 files" is an arbitrary workflow trigger. It can force fragmentation, fixed agent narration, and prompt boilerplate when direct parallel reads are faster and more verifiable.

Correction target: replace with a narrow preference such as "delegate only when the toolchain exposes a suitable agent and the task boundary is independently verifiable."

### F6. `.claude/skills` Contains Benchmark Reports In A Skill-Looking Path

Severity: medium

Evidence:

- `/Users/bardiasamiee/.claude/skills/coding-pg-workspace/iteration-2/benchmark.md:1-10` is a benchmark report, not a skill.
- `/Users/bardiasamiee/.claude/skills/coding-pg-workspace/iteration-2/benchmark.md:30-46` carries eval failures, older-pattern notes, and future iteration suggestions.
- No `SKILL.md` exists under `/Users/bardiasamiee/.claude/skills` in the inventory.
- Rasm repo policy says `.claude/skills/*` is project skill context at `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:27-30`.

Why this poisons context: benchmark evidence placed under `skills/` can be mistaken for skill instruction or loaded adjacent to actual skill work. It carries transient eval commentary, version claims, and performance numbers, not durable instruction.

Correction target: move benchmark artifacts out of `.claude/skills` into an archive/evals path, or add a clear non-loadable archive marker if Claude tooling supports it.

### F7. `.claude/plans` Is A Large Prompt/Agent Archive With No Active Boundary Marker

Severity: medium

Evidence:

- Inventory found 345 plan files: 344 Markdown and 1 JS.
- Filename patterns include many generated prompt/agent/process artifacts, for example `planning-phase-use-gleaming-book-PROMPT.md`, `planning-phase-use-gleaming-book-HANDOFF.md`, many `*-agent-*` files, many `create-use-a-workflow-*` files, and many Rasm-coupled targets such as `target-libs-csharp-rasm-grasshopper-ui-*`.

Why this poisons context: no evidence showed the plans directory is currently loaded, so it was treated as archival. The risk is accidental promotion: if future tooling or a broad prompt says "read plans", this folder contains stale local-project coupling, fixed process narration, workflow boilerplate, and agent fanout records.

Correction target: keep it explicitly archival, or split active plans from generated history.

### F8. Documentation Hygiene Rule Is Correct But Misplaced As A Global Mirror

Severity: low

Evidence:

- `/Users/bardiasamiee/.claude/CLAUDE.md:34-37` warns against project-coupled reusable docs, duplicated validation ladders, load-order instructions, and source/provenance boilerplate.
- Rasm root owns the same rule more concretely at `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:75-92`.

Why this poisons context: the rule itself is good, but global duplication adds another instruction source and includes Rasm-shaped phrases such as "host SDKs" and "local quality tools" in a user-home file.

Correction target: keep only the universal user preference globally; let repo docs standards own the detailed anti-pattern list.

## Non-Findings

- No active top-level `.claude/prompts` directory exists.
- No active top-level `.claude/commands` markdown/text files exist.
- No active top-level `.claude/agents` markdown/text files exist.
- No active top-level `.claude/hooks` markdown/text files exist.
- Plugin cache and marketplace areas contain many markdown instruction surfaces, but they are vendor/plugin files, not user-authored surfaces. Settings prove some plugins are enabled, but no user-authored poisoning finding was assigned to vendor files in this pass.
- `/Users/bardiasamiee/.claude/cache/changelog.md` was inventoried as cache, not active instruction.

## Gaps

- Did not exhaustively read `/Users/bardiasamiee/.claude/plans/**` because the scope classified plans as archival unless load evidence exists. I inventoried counts and filename patterns only.
- Did not exhaustively read plugin cache or marketplace markdown because these are vendor surfaces and the audit target was active user-authored instruction.
- Did not read project session transcripts, paste cache, file-history, task JSON, telemetry, or session-env content because no evidence showed they are active instruction surfaces.
- Did not verify Claude Code's exact runtime loader implementation; classification is based on observed files, settings, plugin manifests, and the audit scope.

## Recommended Cleanup Order

1. Reduce `/Users/bardiasamiee/.claude/CLAUDE.md` to stable user preferences: tone, no emojis, concise output, verify claims, avoid local coupling in reusable artifacts.
2. Delete or soften the mandatory `Further Considerations` footer.
3. Resolve freshness windows by removing numeric windows from global `.claude` rules.
4. Move absolute language mechanics and quality gates out of global `.claude`; leave them to skills and repo manifests.
5. Move `.claude/skills/coding-pg-workspace/iteration-2` out of `skills/` or mark it archival.
6. Add an archival boundary for `.claude/plans`.

## Validation

Read/inventory commands used from repo root:

- `fd -a -t f '\\.(md|txt)$|CLAUDE\\.md$' /Users/bardiasamiee/.claude/CLAUDE.md /Users/bardiasamiee/.claude/skills /Users/bardiasamiee/.claude/commands /Users/bardiasamiee/.claude/prompts /Users/bardiasamiee/.claude/agents /Users/bardiasamiee/.claude/rules /Users/bardiasamiee/.claude/hooks /Users/bardiasamiee/.claude/settings`
- `fd -a -t f . /Users/bardiasamiee/.claude/plans`
- `fd -a -t f . /Users/bardiasamiee/.claude/plugins/cache -d 5`
- `fd -a -t f '\\.(md|txt)$' /Users/bardiasamiee/.claude/plugins/cache /Users/bardiasamiee/.claude/plugins/marketplaces -d 6`
- `nl -ba /Users/bardiasamiee/.claude/CLAUDE.md`
- `nl -ba /Users/bardiasamiee/.claude/rules/output-conventions.md`
- `nl -ba /Users/bardiasamiee/.claude/rules/research-protocol.md`
- `nl -ba /Users/bardiasamiee/.claude/settings.json`
- `nl -ba /Users/bardiasamiee/.claude/plugins/installed_plugins.json`
- `nl -ba /Users/bardiasamiee/.claude/plugins/known_marketplaces.json`

No executable project quality rail was run; this is a read-only audit plus report write.
