# Repo Instructions And Prompts Findings

Source: `../final-report.md`
Scope: Repository `CLAUDE.md`, `AGENTS.md`, nested overlays, and `.claude/prompts`.
Finding count: 8

The finding blocks below are copied verbatim from `../final-report.md`.

## Findings

#### F-PROMPT-01: `.claude/prompts/*.md` encode fixed session choreography instead of portable task contracts
- Severity: high
- Reports: `agent-reports/08-repo-instructions-prompts.md` H1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:31`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:26`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-refine-session.md:52`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-closeout-session.md:10`
- Evidence: prompts prescribe fixed phases/passes such as A/B parity, stress, non-parity exercise, hardening, tests, deep-read, research wave, architecture authoring, per-module design docs, critique waves, `_TMP` implementation, holistic critique, fold-back, completeness sweep, and `START HERE` order.
- Why it may poison context: reusable prompts can override the future task's real scope and recreate process narration agents already receive from higher-priority guidance.
- Suggested disposition: convert prompts to bounded task contracts: goal, owned inputs, non-owned boundaries, allowed output, and proof ceiling; remove phase/pass/wave choreography unless semantically required.

#### F-PROMPT-02: Reusable prompt files still carry Assay/Rasm tool architecture assumptions
- Severity: high
- Reports: `agent-reports/08-repo-instructions-prompts.md` H2
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-refine-session.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:21`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:49`
- Evidence: prompts mention agent-first polyglot quality keychain, existing assay test scaffold, build/mutation/rewrite/package-stage stress paths, one Engine, one Envelope, catalog-driven tool selection, aspect behavior, `<tool>/_design/`, and `<tool>/_TMP/`.
- Why it may poison context: nominally reusable prompts can introduce `_design/`, `_TMP/`, Engine/Envelope, aspect stack, automation, host-boundary, or Assay concepts into tools that do not own those shapes.
- Suggested disposition: either mark these as Rasm/Assay-specific source material, or make them truly generic by replacing local architecture with abstract owner slots bound from the target repo.

#### F-PROMPT-03: Prompt files copy validation/stress ladders into task text
- Severity: medium-high
- Reports: `agent-reports/08-repo-instructions-prompts.md` H4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:35`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:39`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-ab-test-session.md:51`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:28`
- Evidence: prompt text requires A/B diffing exact fields/shapes/exit codes/statuses/truncation/locks/artifacts/diagnostics, stress harnesses across concurrent invocations, exact law families, critique ladders, fold-back, and completeness sweeps.
- Why it may poison context: validation selection moves from owner instructions/tool docs into reusable prompt prose, forcing expensive or stale checks even when the target risk profile differs.
- Suggested disposition: replace copied ladders with proof intents and require binding each proof intent to the active owner before running or claiming it.

#### F-PROMPT-04: Prompt assets retain process/provenance chatter
- Severity: medium
- Reports: `agent-reports/08-repo-instructions-prompts.md` H5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:29`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:32`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/tool-rebuild.md:40`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/prompts/assay-refine-session.md:52`
- Evidence: prompts use "verdict harvest", "research wave", "critique waves", "decision-ledger inputs", "research fan-out", "owner-source evidence", comprehension/critique/synthesis/implementation/adversarial review process wording.
- Why it may poison context: agents may reproduce process reports and wave summaries instead of producing the durable artifact or implementation.
- Suggested disposition: keep research/provenance internal unless the artifact is a report/design ledger; remove wave/harvest/critique-until wording from reusable prompts.

#### F-INSTR-01: `CLAUDE.md` carries exact command ladders that belong to tool owners
- Severity: medium-high
- Reports: `agent-reports/08-repo-instructions-prompts.md` H3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:99`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:114`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:66`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:87`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/AGENTS.md:51`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/AGENTS.md:3`
- Evidence: `CLAUDE.md` lists exact dependency/static/test/API/bridge/parallel-agent commands while root `AGENTS.md` routes quality command behavior and bridge operator behavior to tool READMEs; `libs/csharp/AGENTS.md` routes command syntax to both `CLAUDE.md` and `tools/quality/README.md`.
- Why it may poison context: high-priority always-read instructions become drift-prone command catalogs and can push agents to over-run broad rails.
- Suggested disposition: keep gate taxonomy/selectors in `CLAUDE.md`; move exact command syntax/output paths/behavior to `tools/quality/README.md`, `tools/rhino-bridge/README.md`, or owning tool surfaces; make `libs/csharp/AGENTS.md` point to one current command owner.

#### F-INSTR-02: `CLAUDE.md` repeats general agent mechanics already supplied above repo level
- Severity: medium
- Reports: `agent-reports/08-repo-instructions-prompts.md` H6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:34`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:60`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:91`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:136`
- Evidence: repo `CLAUDE.md` repeats using fresh sources/tools, parallelization, response formatting, plan shape, and broad functional-programming/anti-wrapper behavior.
- Why it may poison context: duplicated high-priority global rules increase context mass and create conflict risk; broad language mechanics belong in language skills unless they are repo-specific deltas.
- Suggested disposition: keep only Rasm-specific deltas in `CLAUDE.md`; move language mechanics to skills and remove generic response/planning mechanics unless the repo needs a stricter local delta.

#### F-INSTR-03: Root `AGENTS.md` numeric provider-budget fact is brittle
- Severity: low-medium
- Reports: `agent-reports/08-repo-instructions-prompts.md` H7
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/AGENTS.md:9`
- Evidence: root instruction text names Codex load behavior and an exact 32 KiB project-doc budget, while also warning provider-loading facts are configuration facts unless local config proves them.
- Why it may poison context: exact provider budget can drift and turn root policy stale.
- Suggested disposition: keep action-changing load behavior; route numeric budget to provider-behavior proof or remove the exact number unless local config proves it.

#### F-INSTR-04: Most nested `AGENTS.md` files are relatively healthy
- Severity: note
- Reports: `agent-reports/08-repo-instructions-prompts.md` lower-risk observations
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/AGENTS.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Grasshopper/AGENTS.md:23`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Rhino/AGENTS.md:22`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/AGENTS.md:24`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/AGENTS.md:3`
- Evidence: nested overlays generally name local owner rails, route command catalogs away, and avoid broad runner commands.
- Suggested disposition: leave most nested overlays intact during cleanup; focus on prompts and root command duplication first.
