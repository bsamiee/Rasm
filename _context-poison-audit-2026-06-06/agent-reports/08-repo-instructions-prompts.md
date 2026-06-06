# First-Wave Audit H - Repo Instructions And Prompts

## Scope

Audit target: repository instruction and prompt surfaces in `/Users/bardiasamiee/Documents/99.Github/Rasm`.

Included:
- `CLAUDE.md`
- root `AGENTS.md`
- nested project `AGENTS.md` files, excluding `docs/standards/_reports/**`
- `.claude/prompts/*.md`
- `.claude/settings/hooks/rules` files if present

Excluded:
- `docs/standards/_reports/**` by task instruction.
- third-party dependency instruction files under `node_modules/**`; hidden/ignored inventory found `node_modules/.pnpm/@zenuml+core@3.47.2_.../node_modules/@zenuml/core/AGENTS.md`, which is dependency material, not a repo instruction surface.
- `.claude/skills/**`, `.claude/agents/**`, `.claude/commands/**`, and `.claude/styles/**`; not in requested scope.

Adjacent read:
- `.claude/hooks/setup-env.sh` was read because a `.claude/hooks` directory exists, but no `.claude/settings` directory and no hook `rules` files were present.

No validation gate was run. This is a read-only audit except for this report.

## Files Read

Instruction files:
- `CLAUDE.md` - 195 lines
- `AGENTS.md` - 100 lines
- `docs/standards/AGENTS.md` - 135 lines
- `libs/csharp/AGENTS.md` - 60 lines
- `libs/csharp/Rasm/AGENTS.md` - 46 lines
- `libs/csharp/Rasm.AppHost/AGENTS.md` - 53 lines
- `libs/csharp/Rasm.AppUi/AGENTS.md` - 51 lines
- `libs/csharp/Rasm.Compute/AGENTS.md` - 50 lines
- `libs/csharp/Rasm.Grasshopper/AGENTS.md` - 54 lines
- `libs/csharp/Rasm.Materials/AGENTS.md` - 48 lines
- `libs/csharp/Rasm.Persistence/AGENTS.md` - 50 lines
- `libs/csharp/Rasm.Rhino/AGENTS.md` - 64 lines
- `tests/csharp/AGENTS.md` - 63 lines
- `tests/csharp/libs/AGENTS.md` - 61 lines
- `tools/assay/AGENTS.md` - 77 lines
- `tools/rhino-bridge/AGENTS.md` - 54 lines

Prompt files:
- `.claude/prompts/assay-ab-test-session.md` - 71 lines
- `.claude/prompts/assay-closeout-session.md` - 40 lines
- `.claude/prompts/assay-refine-session.md` - 61 lines
- `.claude/prompts/tool-rebuild.md` - 51 lines

Hook-adjacent files:
- `.claude/hooks/setup-env.sh` - 64 lines
- no `.claude/settings/hooks/rules` files present
- no `.claude/settings` directory present
- no `.claude/**/rules*` files found

## Executive Read

The nested `AGENTS.md` surfaces are mostly healthy. They are compact, local, owner-oriented, and usually avoid copied command ladders. Their repeated scope leads are not the main problem because they usually point to the parent owner and then add local rails.

The highest poisoning risk is in `.claude/prompts/*.md`. These files are labelled reusable but still carry session choreography, prompt-era artifact structures, A/B and stress ladders, local Assay/Rhino/tooling assumptions, and process provenance language. If future agents treat these as standing policy rather than task prompts, they can inject old workflow shape into unrelated work.

The second risk is `CLAUDE.md`: it is serving as both manifest and command catalog. Root `AGENTS.md` routes quality command behavior to `tools/quality/README.md`, but `CLAUDE.md` still carries exact static/test/bridge command ladders. That makes the highest-load file drift-prone and increases context pressure.

## Findings

### H1 - Reusable prompts still encode project/session workflow instead of portable task shape

Severity: High

Files:
- `.claude/prompts/assay-ab-test-session.md`
- `.claude/prompts/tool-rebuild.md`
- `.claude/prompts/assay-refine-session.md`
- `.claude/prompts/assay-closeout-session.md`

Evidence:
- `.claude/prompts/assay-ab-test-session.md:3` says the next session must work in a fixed order: A/B parity, stress, non-parity exercise, hardening, tests.
- `.claude/prompts/assay-ab-test-session.md:31-57` carries five named phases, including A/B parity, concurrency stress, non-parity exercise, adversarial hardening, and test suite construction.
- `.claude/prompts/assay-ab-test-session.md:67-71` repeats the fixed sequence under `START HERE`.
- `.claude/prompts/tool-rebuild.md:26-37` defines a nine-step multi-stage methodology: deep-read, research wave, architecture authoring, per-module design docs, critique waves, `_TMP` implementation, holistic critique, fold-back, completeness sweep.
- `.claude/prompts/assay-refine-session.md:52-60` prescribes a methodology and fixed start order.
- `.claude/prompts/assay-closeout-session.md:10-39` encodes a two-pass process, then repeats it in `START HERE`.

Why this poisons context:
- The prompts are stored as reusable prompts, but they contain session choreography that can override a future task's actual scope.
- The files recreate process instructions agents already receive from higher-priority system/developer/repo guidance.
- The prompts are likely to cause phase narration and task-history carryover, a pattern the repo standards explicitly reject for active documentation and instruction surfaces.

Recommended correction:
- Convert each prompt from a process script into a bounded task contract: goal, owned inputs, non-owned boundaries, allowed output, and proof ceiling.
- Remove `PHASE`, `PASS`, `DIMENSION`, `START HERE`, and ordered-session choreography unless the order is truly semantic and tool-specific.
- Replace broad "run this whole ladder" wording with "select proof through the active repo instruction chain and tool owner."

### H2 - Prompt files contain local project/tool coupling despite being reusable templates

Severity: High

Files:
- `.claude/prompts/assay-ab-test-session.md`
- `.claude/prompts/assay-refine-session.md`
- `.claude/prompts/tool-rebuild.md`

Evidence:
- `.claude/prompts/assay-ab-test-session.md:7-11` describes the new tool as "the agent-first polyglot quality keychain" and names optional automation arms, host boundary, and test scaffold.
- `.claude/prompts/assay-ab-test-session.md:28` names "the existing assay test scaffold" inside a nominally placeholder-based template.
- `.claude/prompts/assay-ab-test-session.md:39` names build commands, mutation runs, rewrite apply, package stage, and read-only fans as stress targets.
- `.claude/prompts/assay-refine-session.md:7-9` bakes in keychain, predecessor, host boundary, test scaffold, one Engine, one Envelope, catalog-driven tool selection, and aspect behavior.
- `.claude/prompts/tool-rebuild.md:21-24` requires `<tool>/_design/` and `<tool>/_TMP/` staging with specific docs and sync behavior.
- `.claude/prompts/tool-rebuild.md:49-51` requires `_TMP/` and `_design/` as the deliverable end state.

Why this poisons context:
- The prompt filenames and placeholder syntax imply portability, but the content still carries Assay-style architecture, temporary implementation conventions, and Rasm toolchain assumptions.
- A future agent can wrongly introduce `_design/`, `_TMP/`, Engine/Envelope, aspect stack, automation, or host-boundary concepts into a tool that does not own those shapes.
- This directly conflicts with root guidance to keep generated documentation, prompts, skills, standards, examples, templates, and reusable guidance project-agnostic by default (`AGENTS.md:79`) and to avoid project-coupled examples in generic prompts (`AGENTS.md:89`).

Recommended correction:
- Either rename these as Rasm/Assay-specific prompts and route them as source material, or make them truly generic by replacing local tool architecture with abstract owner categories.
- Remove `_design/` and `_TMP/` as required universal folder names unless a specific active owner proves those are current repo policy.
- Replace "Engine", "Envelope", "aspect", "assay", "host boundary", "mutation", and package-stage language with parameterized owner slots that require the agent to bind the terms from the target repo.

### H3 - `CLAUDE.md` carries a command ladder that belongs to tool owners

Severity: Medium-High

Files:
- `CLAUDE.md`
- `AGENTS.md`
- `libs/csharp/AGENTS.md`
- `tools/rhino-bridge/AGENTS.md`

Evidence:
- `CLAUDE.md:99-112` lists exact TypeScript and C# dependency workflow commands.
- `CLAUDE.md:114-132` lists exact static, test, API, bridge, and parallel-agent quality gate commands.
- `AGENTS.md:66-67` routes quality command behavior to `tools/quality/README.md` and Rhino bridge operator behavior to `tools/rhino-bridge/README.md`.
- `AGENTS.md:87` says root `AGENTS.md` carries no command catalogs or validation ladders and routes them to owners.
- `libs/csharp/AGENTS.md:51` routes command syntax to `CLAUDE.md` and `tools/quality/README.md`, showing command ownership is split across two surfaces.
- `tools/rhino-bridge/AGENTS.md:3` says `README.md` owns architecture, command catalog, output contract, environment variables, failure reading, and update rules.

Why this poisons context:
- `CLAUDE.md` is a high-priority always-read manifest. Exact command catalogs in that file become sticky and drift-prone.
- Command behavior now has split ownership: `CLAUDE.md` owns the ladder in practice, while root routing and tool overlays claim tool READMEs own command syntax and operator behavior.
- Agents may over-run broad rails because a high-priority command list is present, even when local proof owners would select a smaller gate.

Recommended correction:
- Keep only gate taxonomy and route selectors in `CLAUDE.md`.
- Move exact command syntax, output paths, and command-specific behavior to `tools/quality/README.md`, `tools/rhino-bridge/README.md`, or the owning tool surface.
- Update `libs/csharp/AGENTS.md:51` so it routes command syntax to a single current owner instead of both `CLAUDE.md` and tool README.

### H4 - Prompt files copy validation and stress ladders into task text

Severity: Medium-High

Files:
- `.claude/prompts/assay-ab-test-session.md`
- `.claude/prompts/tool-rebuild.md`

Evidence:
- `.claude/prompts/assay-ab-test-session.md:15` requires running both tools against the same isolated tree and diffing fields, shapes, exit codes, statuses, truncation, locks, ordering, artifacts, and diagnostics.
- `.claude/prompts/assay-ab-test-session.md:35` blocks stress/tests until every overlapping cell is non-regressive or fixed.
- `.claude/prompts/assay-ab-test-session.md:39` requires a stress harness launching many concurrent invocations across lease-bearing and build-bearing paths.
- `.claude/prompts/assay-ab-test-session.md:51-57` prescribes exact law families for the test suite.
- `.claude/prompts/tool-rebuild.md:28-37` prescribes design, critique, implementation, fold-back, and completeness sweeps as universal rebuild gates.

Why this poisons context:
- Validation ladder selection should come from active repo instruction and tool owners, not from prompt assets.
- These prompts can force expensive or stale checks even when a future target has a different risk profile.
- The wording encourages agents to preserve prompt proof structure instead of discovering the target's owner-defined gates.

Recommended correction:
- Replace copied ladders with proof intents: parity proof, stress proof, adversarial proof, and test proof.
- Require agents to bind each proof intent to the current owner surface before running or claiming it.
- Move any Assay-specific A/B matrix or stress harness details into an Assay owner document if still current.

### H5 - Process/provenance chatter remains concentrated in prompt assets

Severity: Medium

Files:
- `.claude/prompts/tool-rebuild.md`
- `.claude/prompts/assay-refine-session.md`

Evidence:
- `.claude/prompts/tool-rebuild.md:28` asks for "Deep-read and verdict harvest" and preserving contradictions as decision-ledger inputs.
- `.claude/prompts/tool-rebuild.md:29` prescribes a "Research wave" and returned signatures, levers, seams, and constraints.
- `.claude/prompts/tool-rebuild.md:32` prescribes "Critique waves" until actionable defects stop.
- `.claude/prompts/tool-rebuild.md:40-41` speaks in provenance terms: architectural authority, research fan-out, and owner-source evidence.
- `.claude/prompts/assay-refine-session.md:52-54` prescribes comprehension, critique, synthesis, implementation, adversarial review, and targeted fixing.

Why this poisons context:
- The language invites agents to reproduce process reports, wave summaries, and provenance narration instead of writing the durable artifact or implementing the requested change.
- The repo standards repeatedly reject prompt-source narration, critique summaries, task framing, fixed wave counts, and process chatter in active surfaces (`docs/standards/AGENTS.md:11`, `docs/standards/AGENTS.md:99`).

Recommended correction:
- Keep research/provenance as internal method only when the task explicitly asks for it.
- For reusable prompts, express only the artifact contract and proof ceiling.
- Delete "wave", "harvest", "critique until", and "decision ledger" wording unless the target artifact itself is a report or design ledger.

### H6 - `CLAUDE.md` repeats global agent mechanics already supplied above repo level

Severity: Medium

File:
- `CLAUDE.md`

Evidence:
- `CLAUDE.md:34-38` repeats general behavior: use new sources, tools over internal knowledge, parallelize aggressively, reference symbols by name.
- `CLAUDE.md:91-95` repeats response-format behavior: use backticks, avoid large code blocks, use Markdown, keep responses actionable.
- `CLAUDE.md:136-140` prescribes general plan shape.
- `CLAUDE.md:60-87` carries broad functional-programming and anti-wrapper rules across all domain logic.

Why this poisons context:
- Some of this is repo-specific posture, but much of it duplicates global/developer agent behavior.
- High-priority duplicated rules increase context mass and make conflicts harder to resolve.
- The broadest global rules could be split between repo-specific engineering doctrine and language skills, reducing repetition in every task.

Recommended correction:
- Keep only Rasm-specific deltas in `CLAUDE.md`: skill routing, repo-level target posture, core architectural constraints, and owner routes.
- Move language mechanics into the language skills where they are not already present.
- Move chat/format behavior out unless it has a repo-specific reason.

### H7 - Load-order wording is mostly useful, but one provider-budget fact is brittle

Severity: Low-Medium

File:
- `AGENTS.md`

Evidence:
- `AGENTS.md:9` states Codex load order, one instruction file per directory, `AGENTS.override.md` precedence, and a 32 KiB project-doc budget.
- `AGENTS.md:11` correctly warns that fallback names and provider-loading behavior are configuration facts, not repo policy, unless local config proves them.

Why this poisons context:
- The 32 KiB budget is a provider behavior claim embedded in root policy. If the provider changes the budget, a root instruction file becomes stale.
- The same paragraph is otherwise action-changing and useful because nested overlay discovery depends on it.

Recommended correction:
- Keep load-order behavior that changes author action.
- Route the numeric budget to a provider-behavior proof owner or phrase it without an exact number unless current local config proves it.

## Lower-Risk Observations

### Nested C# overlays are mostly local and action-changing

The C# parent and project overlays usually name local owners, route package facts away, and reject local bad shapes without copying full command catalogs.

Examples:
- `libs/csharp/AGENTS.md:25-30` names extension grammar for project capability, folders, receipt families, and package adoption.
- `libs/csharp/Rasm.Grasshopper/AGENTS.md:23-29` names GH2 component, UI, document mutation, wire, motion, host fact, and app behavior owners.
- `libs/csharp/Rasm.Rhino/AGENTS.md:22-29` names Rhino concern owners and native conversion rules.
- `libs/csharp/Rasm.Persistence/AGENTS.md:21-27` names lifecycle, query, app state, snapshot, store receipt, and package-backed storage owner routes.

Smell level: low. These are not generic global repeats; they are local rail contracts.

### Test overlays avoid generic validation ladders

The test overlays select proof classes and oracle policy rather than listing broad runner commands.

Examples:
- `tests/csharp/AGENTS.md:24-28` defines expected-value sources, proof classification, bridge scenario ownership, and mutation survivor triage.
- `tests/csharp/libs/AGENTS.md:33-40` defines extension grammar for shared rail proof, `Option` proof, input families, expected-value engines, dense case families, and bridge-owned behavior.

Smell level: low. These should remain instruction surfaces.

### Tool overlays generally route command catalogs away

Examples:
- `tools/assay/AGENTS.md:3` says `README.md` owns command surface, operator workflow, and user-facing tool reference.
- `tools/rhino-bridge/AGENTS.md:3` says `README.md` owns architecture, command catalog, output contract, environment variables, failure reading, and update rules.

Smell level: low. The problem is not these overlays; it is the split with `CLAUDE.md` command ladders.

### Standards overlay actively rejects report and prompt leakage

`docs/standards/AGENTS.md` is doing useful anti-poisoning work:
- `docs/standards/AGENTS.md:11` rejects copying report transcripts, roles, confidence, task framing, fixed wave counts, or report structure into active standards.
- `docs/standards/AGENTS.md:55` requires read-only audits to state no files were edited and no validation gate ran.
- `docs/standards/AGENTS.md:96-105` forbids task instructions, chat excerpts, critique summaries, prompt-source narration, fixed sub-agent counts, unsupported gate claims, and generated catalogs.

Smell level: low. Keep this surface, but use it to clean `.claude/prompts`.

## Smell Matrix

| Smell | Present | Main Location | Notes |
| :-- | :-- | :-- | :-- |
| Repeated global rules | Yes | `CLAUDE.md` | Broad mechanics and response behavior repeat agent-level defaults. |
| Meta instructions agents already have | Yes | `CLAUDE.md`, prompts | Parallelization, Markdown response style, phase workflow, plan shape. |
| Local project coupling inside reusable prompts | Yes | `.claude/prompts/*.md` | Assay/keychain/Engine/Envelope/host-boundary/_TMP/_design assumptions. |
| Validation ladders copied into prompts | Yes | `assay-ab-test-session.md`, `tool-rebuild.md` | Exact A/B, stress, law, critique, and completeness ladders. |
| Fixed sub-agent counts | No direct count found | None | "Waves" and process narration exist, but no fixed numeric agent count in scoped files. |
| Process narration | Yes | `.claude/prompts/*.md` | Phase/pass/dimension/wave language. |
| Source/provenance chatter | Yes | `tool-rebuild.md`, `assay-refine-session.md` | Research waves, verdict harvest, decision ledger, owner-source evidence. |
| Defensive/version wording | Light | `CLAUDE.md`, `AGENTS.md` | Freshness/budget facts are exact and drift-prone; little compatibility caveat wording found. |
| Brittle load-order instructions | Light | `AGENTS.md` | Mostly useful; exact 32 KiB provider budget is brittle. |

## Recommended Cleanup Order

1. Clean `.claude/prompts/*.md` first.
   - Highest poisoning risk.
   - Most direct conflict with root guidance on reusable prompts and project coupling.
   - Likely easiest to fix without changing active repo behavior.

2. Split command ladders out of `CLAUDE.md`.
   - Keep gate taxonomy and triggers.
   - Route exact commands to `tools/quality/README.md`, `tools/rhino-bridge/README.md`, and package/tool owners.
   - Then repair `libs/csharp/AGENTS.md:51` if it still routes command syntax to two owners.

3. Trim general agent mechanics from `CLAUDE.md`.
   - Keep repo-specific engineering constraints.
   - Move language mechanics to skills where needed.
   - Remove generic response/planning instructions unless the repo needs a stricter local delta.

4. Leave most nested overlays intact.
   - They mostly pass the delta-only test.
   - Do not flatten local owner rails into root policy.

## Gaps

- I did not read `docs/standards/_reports/**` because the task explicitly excluded that subtree.
- I did not read `.claude/skills/**`, `.claude/agents/**`, `.claude/commands/**`, or `.claude/styles/**`; these were outside the requested instruction/prompt surface.
- I did not audit third-party `node_modules/**` instruction files. Hidden/ignored inventory found one dependency `AGENTS.md`, but it is not a repo instruction surface.
- No `.claude/settings/hooks/rules` files were present. `.claude/hooks/setup-env.sh` exists and was read as adjacent context; it is a hook script, not a rules file.
- I did not run link, Markdown, docs, static, test, or bridge validation; this was a read-only audit plus report write.
