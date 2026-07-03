---
description: Ground a new planning session — core law, scoped corpus maps, workflow dispatch — then take the session objective
argument-hint: [target folder path(s) or a language (csharp|python|typescript); empty = cross-libs]
---

# [SESSION_PRIME]

Ground this session for planning-corpus work, then take the objective. TARGET: $ARGUMENTS — one or more folder paths scope to those folders and their branches; a bare language word scopes to that branch; empty scopes cross-libs. Grounding is read-only and plan-mode-compatible; no work, no proposals, and no workflow launch before the objective arrives.

## [01]-[CORE_LAW]

Always, in full, regardless of target:

1. `ls libs/.planning/`, then READ 100%: `planning-targets.md` (the target index), `campaign-method.md` (the loop, the bar, the agent-role law — discovery/critique/red-team/verify, the two naivety axes, collapse-floor freedom), `architecture.md` (strata, dependency direction, wire seams), `README.md` (the authoring standard: doc-set tiers, index-doc templates, page grammar). CLAUDE.md and the root README are already loaded — never re-read them, never restate their content back.
2. READ `tools/assay/README.md` — the repo operator every verification claim routes through (`uv run python -m tools.assay ...`).
3. `ls .claude/workflows/` — the durable engine roster. The workflow paradigm, run-ledger, and resume law are the CLAUDE.md `[01]` `[WORKFLOW_ENGINE]` card; `.claude/skills/workflow-creator/` is read only when the session authors or edits a workflow file. A file beyond the durable roster is an ephemeral mid-campaign workflow — a live-campaign signal, paired with its root brief.
4. Census the campaign state: `fd -d 1 'RASM-.*\.md'` at the repo root — the standing briefs and DECISION records — and read the session-start git status for uncommitted `.planning`/doc diffs (a landed-but-uncommitted pass). Names only at prime; a brief is read IN FULL only when the objective names its campaign.

## [02]-[TARGET_SCOPE]

For each targeted language `<lang>` (all three when cross-libs):

1. READ `docs/stacks/<lang>/README.md` in full + `ls docs/stacks/<lang>/` (and `ls docs/stacks/csharp/domain/` for csharp). The remaining doctrine pages are composed IN FULL by any agent that writes fences in that language — the session reads them itself only when it writes fences itself, never as warm-up.
2. READ the branch column: `libs/<lang>/.planning/README.md` + `libs/<lang>/.planning/ARCHITECTURE.md`. Then `tree -a -L 2 libs/<lang>` for the general branch topology — package roots, doc surfaces, and which folders carry a `.planning/` scaffold (the lifecycle signal); never a file-by-file walk.
3. `ls libs/<lang>/.api/` — the shared substrate-catalog tier; each planning folder carries its own domain tier at `<pkg>/.api/`.
4. Scan the central manifest for the package landscape (names and groups, never memorized versions): `Directory.Packages.props` (csharp), `pyproject.toml` (python), `pnpm-workspace.yaml` (typescript).

For each targeted FOLDER: READ its two core index docs (`README.md` + `ARCHITECTURE.md`), then `loc <pkg>/.planning` — the page inventory with the thinness/complexity signal a bare `ls` hides — and `ls <pkg>/.api/`. A bare-language target reads the same two cores for every planning folder on that branch. `IDEAS.md`/`TASKLOG.md` are never opened at prime — card pools enter context only when a dispatched rail works them.

HOMING EXCEPTION — `libs/csharp/Rasm`: the only irregular planning folder. Design pages live at `Rasm/Geometry/.planning/`; the Rasm-root `README.md`/`ARCHITECTURE.md` and `Rasm/.api/` govern the Geometry effort; `Analysis/`, `Domain/`, `Vectors/` are mature source and never planning targets; one `.csproj` is shared by all of `Rasm/`.

## [03]-[SESSION_DISPATCH]

The session shapes and their entry rails — selected when the objective arrives, never before:

| [SHAPE]                            | [ENTRY]                                                                                                           |
| :--------------------------------- | :---------------------------------------------------------------------------------------------------------------- |
| Major cross-folder campaign        | design workflow: survey->draft->judge->decide -> root DECISION brief -> `rebuild` legs against the brief, per-leg |
| Targeted rebuild / quality pass    | `rebuild` with `{targets, brief?}` (no brief = every targeted page hostile-rebuild)                               |
| Align / clean / hygiene            | `align-cards`, `hygiene-sweep`, `tidy-planning-docs`                                                              |
| Idea/task pool + realization       | `ideate` (pool generation), `implement-cs`/`implement-py`/`implement-ts` (cards -> fences)                        |
| Doctrine refining (`docs/stacks/`) | `stack-cs`/`stack-py`/`stack-ts`                                                                                  |
| Package roster / catalog work      | `survey-packages`, `survey-gaps`, `rebuild-api`                                                                   |
| Cross-file residual cleanup        | `resolve-residuals`                                                                                               |

A shape with no fitting durable gets an ephemeral workflow authored via `.claude/skills/workflow-creator` and deleted after landing. Legs re-run across sessions against the same brief until cold passes find nothing.

## [04]-[CLOSE]

Return a compact readiness report — the live campaign state (root briefs, ephemeral workflows, uncommitted planning diffs), targets and their observed state (page inventories, staleness or drift signals between the core docs and disk, any `planning-targets.md` row disk contradicts), the dispatch rail(s) that fit, and any blocker — then ask for the session objective, scope, and constraints. Maximum signal, zero restated law.
