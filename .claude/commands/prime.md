---
description: Ground a new planning session — topology, campaign state, planning law — then take the session objective
argument-hint: [target folder path(s) or a language (csharp|python|typescript); empty = branch-level grounding]
---

# [SESSION_PRIME]

Ground this session for planning-corpus work, then take the objective. TARGET: $ARGUMENTS — folder paths deepen those folders at prime; a bare language word deepens that branch; empty grounds at branch level and defers folder deepening to the objective. Grounding is read-only and plan-mode-compatible; no work, no proposals, no workflow launch before the objective arrives.

The frame: three peer branches — `libs/csharp`, `libs/python`, `libs/typescript` — building world-class, lib-grade foundations, and the entire operating surface is DESIGN DOCS: every capability is a transcription-complete code fence inside a `.planning/` design page, rebuilt root-up without hesitation; no session lands source files. CLAUDE.md and the root README are already loaded — never re-read them, never restate their content.

Budget law: [01] + [02] ground the session; [03] spends only on what the target or objective names; the whole prime stays under 100k tokens. Batch every multi-file read through one `tail -n +1` command, never per-file reads.

## [01]-[TOPOLOGY]

Run all four in one parallel block:

1. `tree -L 3 libs` — the full branch/package map with per-file size, modified-age, and git-modified columns: package rosters, index docs, thinness/staleness and uncommitted-pass signals in one view.
2. `fd -H -t d -d 3 '^\.(planning|api)$' libs` — the scaffold census: which folders carry a `.planning/` (the lifecycle signal) and which carry an `.api/` catalog tier (a planning folder without one is a gap signal).
3. `fd -d 1 -t f .` — root files, names only. `RASM-*` briefs/DECISIONs/SPECs are the standing campaign state; a brief is read IN FULL only when the objective names its campaign. Every other root file — central manifests, lockfiles, workspace/solution files, tool config — is an owner to know exists and never read at prime.
4. `ls .claude/workflows/` — the durable engine roster; a file beyond the durable roster is an ephemeral mid-campaign workflow, a live-campaign signal paired with its root brief. Read the session-start git status for uncommitted `.planning`/doc concentrations — a landed-but-uncommitted pass.

## [02]-[PLANNING_LAW]

READ 100%, one batch (`==> path <==` headers delimit files):

```bash
fd -H -t f -e md -E 'IDEAS.md' -E 'TASKLOG.md' . libs/.planning libs/csharp/.planning libs/python/.planning libs/typescript/.planning -X tail -n +1
```

This is the complete Tier-0 + branch law: `campaign-method.md` (the loop, the bar, the agent-role law, the two naivety axes, collapse-floor freedom), `architecture.md` (strata, dependency direction, wire seams), `README.md` (the authoring standard), `planning-targets.md` (the target index), each branch router + `ARCHITECTURE.md`, and the branch system pages (`component-system.md`, `composition-system.md`, `dataflow-system.md`). `IDEAS.md`/`TASKLOG.md` never open at prime — card pools enter context only when a dispatched rail works them. Cross-check `planning-targets.md` rows against the [01] scaffold census; a row disk contradicts is a finding for the readiness report.

## [03]-[TARGET_DEEPENING]

Spent only on what TARGET or the arrived objective names — folder cores are never read wholesale; the branch law already maps every folder:

- One folder: READ `<pkg>/README.md` + `<pkg>/ARCHITECTURE.md`, then `loc <pkg>/.planning` (the page inventory with the thinness/complexity signal a bare `ls` hides) and `ls <pkg>/.api/`.
- A bare language: batch the branch's folder cores in one command:

```bash
fd -H -t f --max-depth 3 -E '_tmp' -E '.planning' '^(README|ARCHITECTURE)\.md$' libs/<lang> -X tail -n +1
```

- Never warm-up reads: `docs/stacks/<lang>/` doctrine is composed in full by whichever agent writes or judges fences in that language, at that moment; `tools/assay/README.md` is read when a verification rail actually runs (`uv run python -m tools.assay ...`); a central manifest is scanned only when package work enters scope.

## [04]-[SESSION_DISPATCH]

The session shapes and their entry rails — selected when the objective arrives, never before:

| [SHAPE]                            | [ENTRY]                                                                                                            |
| :--------------------------------- | :------------------------------------------------------------------------------------------------------------------ |
| Major cross-folder campaign        | design workflow: survey->draft->judge->decide -> root DECISION brief -> `rebuild` legs against the brief, per-leg  |
| Campaign brief authoring           | `brief` with `{targets, upstream, deep, mandate}` — one brief or a dependency-ordered waterfall                    |
| Targeted rebuild / quality pass    | `rebuild` with `{targets, brief?}` (no brief = every targeted page hostile-rebuild)                                |
| Align / clean / hygiene            | `align-cards`, `hygiene-sweep`, `tidy-planning-docs`                                                               |
| Idea/task pool + realization       | `ideate` (pool generation), `implement-cs`/`implement-py`/`implement-ts` (cards -> fences)                         |
| Doctrine refining (`docs/stacks/`) | `stack-cs`/`stack-py`/`stack-ts`                                                                                   |
| Package roster / catalog work      | `survey-packages`, `survey-gaps`, `rebuild-api`                                                                    |
| Finalization / corrections pass    | `finalize` with a package root or folder subset (split-brain, phantoms, flow, collapse — post-build closure)      |

A shape with no fitting durable gets an ephemeral workflow authored via `.claude/skills/workflow-creator` and deleted after landing. Legs re-run across sessions against the same brief until cold passes find nothing.

## [05]-[CLOSE]

Return a compact readiness report — the live campaign state (root briefs and DECISIONs, ephemeral workflows, uncommitted planning concentrations), the topology signals worth acting on (scaffold gaps, thin or stale folders, any `planning-targets.md` row disk contradicts), the dispatch rail(s) that fit, and any blocker — then ask for the session objective, scope, and constraints. Maximum signal, zero restated law.
