# Context Poisoning Audit Final Report

Created: 2026-06-06

## Scope

Audit active project documentation, instruction files, skills, prompts, and code comments across:

- `/Users/bardiasamiee/Documents/99.Github/Rasm`
- `/Users/bardiasamiee/.codex`
- `/Users/bardiasamiee/.claude`

Exclude `docs/standards/_reports/**` content from findings except for reading its `AGENTS.md`.

## Method

This report is built incrementally from agent report files. Each imported report should be recorded in `Report Intake Log`, then findings should be merged into the categorized sections below. Duplicate findings are combined, with distinct evidence preserved.

## Report Intake Log

- Imported `agent-reports/08-repo-instructions-prompts.md`: read root `CLAUDE.md`, root/nested `AGENTS.md`, `.claude/prompts/*.md`, and hook-adjacent files; produced findings about reusable prompts encoding fixed session choreography, Assay/Rasm coupling, copied validation/stress ladders, process/provenance chatter, `CLAUDE.md` command catalogs, duplicated global mechanics, and a brittle provider-budget fact.
- Imported `agent-reports/09-project-claude-skills.md`: inventoried 126 project-local skill Markdown files under `.claude/skills/**`; produced findings about static GitHub Actions version/SHA catalogs, visible reasoning handling in Perplexity tooling, Rasm-coupled `testing-cs`, duplicated validation ladders, contradictory templates, skill-eval prompt sections, local environment leakage, defensive version wording, and project/brand coupling in language skills.
- Imported `agent-reports/10-home-codex-skills.md`: audited `~/.codex/AGENTS.md` and active `~/.codex/skills/**` excluding memories/caches/vendor/tmp; produced findings about global policy duplication, Rasm-specific analyzer rulecraft in a global skill, Parametric Portal leakage in `testing-ts`, Rasm-style `testing-cs`, duplicated command/validation catalogs, frozen version/model claims, orchestration meta, and local package/path examples.
- Imported `agent-reports/11-home-claude-active.md`: read active user-authored `~/.claude` instruction/rule/config surfaces and inventoried archives/vendor areas; produced findings about global policy duplication, conflicting research windows, mandatory footer, overbroad no-control-flow rule, fixed delegation trigger, benchmark artifacts in a skill-looking path, unmarked plan archives, and duplicated documentation hygiene.

## Findings By Category

### Repo Prompt Assets Encode Session Process

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

### Root Instructions Duplicate Command Or Agent Mechanics

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

### Project-Local Skills Contain Stale Catalogs Or Reusable-Surface Coupling

#### F-SKILL-PROJ-01: GitHub Actions skill contradicts live-resolution policy with static version/SHA catalogs
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:22`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:137`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/version-discovery.md:76`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/common_errors.md:84`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/examples/docker-build-push.yml:26`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/examples/dependency-review.yml:24`
- Evidence: skill says to resolve action versions live and never embed static SHAs, but references/examples include current-version tables, exact action versions, exact SHAs, Node runtime timing, and pinned example SHAs.
- Why it may poison context: agents may copy stale SHAs/majors as current truth instead of resolving actions at generation time.
- Suggested disposition: delete static current-version/SHA tables; keep resolution protocol, pinning format, and validation shape; use placeholder SHAs or mark static SHAs unusable.

#### F-SKILL-PROJ-02: Perplexity tool skill normalizes visible reasoning and `<think>` handling
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F2
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:56`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:93`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/scripts/perplexity.py:11`
- Evidence: user-facing skill docs describe visible reasoning, `strip` for research/reason commands, and `<think>` tags by default; wrapper usage repeats "strip thinking".
- Why it may poison context: generated research workflows may request, preserve, or strip reasoning traces instead of treating reasoning as unavailable/private.
- Suggested disposition: remove visible chain-of-thought wording; expose only summarized answer content; make any stripping internal or reject reasoning traces at the wrapper boundary.

#### F-SKILL-PROJ-03: Project-local `testing-cs` skill is heavily Rasm-coupled while still acting reusable
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:40`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:73`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:117`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/bridge-runtime.md:17`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/rails-tooling.md:8`
- Evidence: skill embeds repo paths, `_testkit`, `_architecture`, `_tooling`, `_benchmarks`, `_fuzz`, repo scenario roots, harness variables, fact channels, retired bridge flow, runtime health command, artifact paths, and `docs/testing-libs/*` facts.
- Why it may poison context: generic C# test generation can inherit Rasm runtime/quality-router assumptions and tool behavior becomes owned by both skill and repo docs.
- Suggested disposition: split reusable testing law/oracle material from Rasm-local scenario and quality-router overlay; route command catalogs to the quality owner.

#### F-SKILL-PROJ-04: Project-local skills duplicate validation ladders and command catalogs from owners
- Severity: high
- Reports: `agent-reports/09-project-claude-skills.md` F4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/SKILL.md:143`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-cs/references/rails-tooling.md:8`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/SKILL.md:48`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/references/validation.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:161`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/SKILL.md:155`
- Evidence: skills embed static/test/runtime commands, mutation/coverage commands, Bash validation pipelines, GitHub Actions validation pipelines, best-practice checks, and troubleshooting tables.
- Why it may poison context: stale skill validation text becomes a second owner for tool behavior and can cause false proof or over-validation.
- Suggested disposition: replace command ladders with owner routes and selection criteria; keep only invariant skill-local checks.

#### F-SKILL-PROJ-05: Skill templates/examples encode contradictory or placeholder artifact patterns
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/SKILL.md:166`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/testing-ts/references/validation.md:37`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/templates/docker-action.template.yml:72`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/templates/standard.template.md:120`
- Evidence: `testing-ts` says pass schemas directly but template/reference still use `Arbitrary.make(Schema)`; action templates and Bash templates include executable placeholder/wrapper behavior.
- Why it may poison context: generated artifacts can copy contradictory or placeholder code into production output.
- Suggested disposition: fix the `testing-ts` schema contradiction and remove placeholder behavior from executable templates.

#### F-SKILL-PROJ-06: Skill-eval prompt/meta sections are embedded in runtime skill context
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:167`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/SKILL.md:155`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/references/validation.md:93`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-ts/SKILL.md:167`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-pg/references/validation.md:176`
- Evidence: skills include explicit invocation, implicit invocation, noisy context, negative control, and compliance check examples.
- Why it may poison context: runtime skills become prompt-test transcripts and steer agents toward meta compliance.
- Suggested disposition: move eval prompts to private evaluation fixtures outside runtime skill context.

#### F-SKILL-PROJ-07: Tool skills leak local environment details into reusable surfaces
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F7
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/context7-tools/SKILL.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/exa-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/perplexity-tools/SKILL.md:10`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/tavily-tools/SKILL.md:12`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/sonarcloud-tools/SKILL.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/hostinger-tools/SKILL.md:55`
- Evidence: skills mention `$CLAUDE_HOME`, `$CODEX_HOME`, 1Password API-key injection, `SONAR_TOKEN`, repo-root `sonar-project.properties`, coverage paths, and concrete VPS/firewall IDs.
- Why it may poison context: generated instructions may depend on one workstation/runtime.
- Suggested disposition: use `<skill-root>/scripts/...`, keep secret injection in local install notes, and replace concrete IDs with placeholders.

#### F-SKILL-PROJ-08: Skills preserve defensive version/caveat manuals and project/brand coupling
- Severity: medium
- Reports: `agent-reports/09-project-claude-skills.md` F8, F9
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/SKILL.md:4`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-bash/references/version-features.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/github-actions/references/version-discovery.md:65`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/mermaid-diagramming/SKILL.md:99`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-python/SKILL.md:128`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/coding-csharp/references/validation.md:91`
- Evidence: skills carry version gates/fallback chains/history, paused rollout history, beta caveats, "Noesis", CSP analyzer mappings, and project proof language.
- Why it may poison context: reusable skills generate compatibility chatter or local analyzer assumptions instead of current owner truth.
- Suggested disposition: keep only version boundaries that change generation behavior; remove project/brand/analyzer specifics from reusable layers or rename them as project overlays.

### Home Codex Instructions And Skills Leak Repo-Specific Policy

#### F-HOME-CODEX-01: `~/.codex/AGENTS.md` duplicates repo-level policy in global scope
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J1
- Locations:
  - `/Users/bardiasamiee/.codex/AGENTS.md:4`
  - `/Users/bardiasamiee/.codex/AGENTS.md:15`
  - `/Users/bardiasamiee/.codex/AGENTS.md:28`
  - `/Users/bardiasamiee/.codex/AGENTS.md:42`
  - `/Users/bardiasamiee/.codex/AGENTS.md:65`
- Evidence: global `AGENTS.md` defines file-type skill routing, greenfield refactor doctrine, polymorphism/helper rules, planning shape, navigation, research provider order, and language baselines, then says repository routes/command catalogs belong in active repo instructions.
- Why it may poison context: Rasm-style defaults pre-load into every repository before local repo instructions can define their own routes, versions, and planning policy.
- Suggested disposition: keep only portable prompt/document hygiene and safety in home `AGENTS.md`; move skill routing, language baselines, command ownership, and planning shape to repo owners.

#### F-HOME-CODEX-02: `cs-analyzer-rulecraft` is Rasm-specific but stored as a global reusable skill
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J2
- Locations:
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:3`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:15`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:41`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:75`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:117`
- Evidence: skill names `tools/cs-analyzer`, `docs/system-api-map`, analyzer output locking, C# 14, LanguageExt, Thinktecture, CSP-style rules, and broad Rasm-like production proof loops.
- Why it may poison context: unrelated analyzer repos inherit Rasm analyzer topology, docs routes, and proof habits.
- Suggested disposition: move to Rasm-scoped project skills or rewrite as generic analyzer-rule guidance using repo-declared analyzer/project/proof routes.

#### F-HOME-CODEX-03: Global `testing-ts` skill contains Parametric Portal imports, topology, hooks, and app ports
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J3
- Locations:
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:47`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:85`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/references/categories.md:136`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/references/guardrails.md:16`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/templates/unit-pbt.spec.template.md:24`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/templates/contract.spec.template.md:25`
- Evidence: skill names `packages/server`, `@parametric-portal/*`, `.claude/hooks/validate-spec.sh`, Playwright agent pipeline, triple-app bootstrap `api (:4000)`, `parametric_icons (:3001)`, and `test_harness (:3002)`.
- Why it may poison context: all TypeScript test generation can inherit Parametric Portal package names, hooks, ports, import order, and topology.
- Suggested disposition: move examples/templates into a project-scoped skill; replace imports with neutral placeholders and route app bootstrap to active repo test instructions.

#### F-HOME-CODEX-04: Global `testing-cs` carries Rasm-style test rails and runtime scenario ownership
- Severity: high
- Reports: `agent-reports/10-home-codex-skills.md` J4
- Locations:
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:16`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:24`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:40`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:117`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/references/rails-tooling.md:8`
  - `/Users/bardiasamiee/.codex/skills/testing-cs/references/bridge-runtime.md:17`
- Evidence: skill defines active repo testkit, `docs/testing-libs`, exact spec/testkit/runtime/mutation/architecture/tooling/benchmark/fuzz locations, quality-router validation ladder, and runtime scenario rules.
- Why it may poison context: global C# testing guidance imports Rasm host/runtime separation and quality-router behavior into other C# repos.
- Suggested disposition: keep generic law/oracle/test-shape guidance global; move Rasm rails to repo instructions or project-scoped skills.

#### F-HOME-CODEX-05: Home skills duplicate command catalogs, validation ladders, frozen versions, orchestration meta, and local examples
- Severity: medium
- Reports: `agent-reports/10-home-codex-skills.md` J5, J6, J7, J8
- Locations:
  - `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:20`
  - `/Users/bardiasamiee/.codex/skills/testing-ts/SKILL.md:55`
  - `/Users/bardiasamiee/.codex/skills/github-actions/references/act_usage.md:12`
  - `/Users/bardiasamiee/.codex/skills/speech/SKILL.md:9`
  - `/Users/bardiasamiee/.codex/skills/cs-analyzer-rulecraft/SKILL.md:43`
  - `/Users/bardiasamiee/.codex/skills/nx-tools/SKILL.md:45`
- Evidence: skills contain Nx command catalogs, `pnpm exec nx test -- --coverage`, `npx stryker run`, current actionlint/act versions, hard-coded OpenAI model names/behavior, explicit 8-10 sub-agent orchestration, `@parametric-portal/types`, and `CLAUDE.md` examples.
- Why it may poison context: home skills become stale source owners and inject local examples/orchestration into unrelated work.
- Suggested disposition: route commands to tool help or active repo owners; replace current-version/model claims with live lookup or fallback-snapshot labels; gate orchestration on explicit user request; neutralize local examples.

### Home Claude Active Surfaces Mirror Repo Policy Or Preserve Archives

#### F-HOME-CLAUDE-01: Global `.claude/CLAUDE.md` duplicates repo-owned policy
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F1, F8
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:34`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:39`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:43`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:49`
  - `/Users/bardiasamiee/.claude/CLAUDE.md:58`
  - `/Users/bardiasamiee/.claude/rules/output-conventions.md:3`
  - `/Users/bardiasamiee/.claude/rules/research-protocol.md:3`
- Evidence: global `.claude` repeats repo plan shape, output structure, research freshness, documentation hygiene, greenfield/refactor posture, and quality gate reminders.
- Why it may poison context: repo-specific norms become ambient user preferences for non-Rasm repos and create multiple authority layers for Rasm.
- Suggested disposition: shrink global `.claude/CLAUDE.md` to stable user preferences; let repo `CLAUDE.md`/`AGENTS.md` own repo policy and detailed anti-patterns.

#### F-HOME-CLAUDE-02: Active research freshness windows conflict
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F2
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:28`
  - `/Users/bardiasamiee/.claude/rules/research-protocol.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:34`
- Evidence: active rules mention latest-stable assumptions, `<=6 months`, `2025+`, repo `last 9 months`, and the current global execution policy uses `3-4 months`.
- Why it may poison context: agents spend reasoning budget reconciling conflicting freshness windows and may pick the wrong one.
- Suggested disposition: keep only "use current primary sources for freshness-sensitive facts" globally; let repo manifests own exact windows.

#### F-HOME-CLAUDE-03: Mandatory `Further Considerations` footer conflicts with concision
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F3
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:39`
  - `/Users/bardiasamiee/.claude/rules/output-conventions.md:7`
- Evidence: global file requires every substantive response to end with `Further Considerations` while also demanding minimized token waste, no preamble, no sign-off summaries, and every paragraph changing action.
- Why it may poison context: fixed footer sections become process-form boilerplate even for direct status updates or concise final answers.
- Suggested disposition: delete the mandatory footer; include caveats only when they change the decision.

#### F-HOME-CLAUDE-04: Global no-control-flow rule is broader than repo domain-logic policy
- Severity: high
- Reports: `agent-reports/11-home-claude-active.md` F4
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:9`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/CLAUDE.md:60`
- Evidence: global rule bans `if`/`else`/`for`/`while`/`switch`/`try`/`catch` across all languages, while repo policy scopes the strict rule to domain logic with boundary exceptions.
- Why it may poison context: adapters, scripts, CLIs, generated code, tests, and operational glue can appear invalid despite skill/repo exceptions.
- Suggested disposition: move language mechanics to skills and keep global preference as a bias; preserve repo's narrower domain-logic formulation.

#### F-HOME-CLAUDE-05: Fixed subagent delegation trigger is process narration
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F5
- Locations:
  - `/Users/bardiasamiee/.claude/CLAUDE.md:21`
  - `/Users/bardiasamiee/.claude/settings.json:26`
- Evidence: global rule mandates delegation when reading more than three files, despite this being an arbitrary process trigger.
- Why it may poison context: agents may fragment simple work and produce unnecessary delegation boilerplate.
- Suggested disposition: replace with "delegate only when a suitable agent exists and the task boundary is independently verifiable."

#### F-HOME-CLAUDE-06: Benchmark artifacts and plan archives sit in paths that can be mistaken for active context
- Severity: medium
- Reports: `agent-reports/11-home-claude-active.md` F6, F7
- Locations:
  - `/Users/bardiasamiee/.claude/skills/coding-pg-workspace/iteration-2/benchmark.md:1`
  - `/Users/bardiasamiee/.claude/plans`
- Evidence: `.claude/skills/.../benchmark.md` is a benchmark report with eval failures/future iteration suggestions and no `SKILL.md`; `.claude/plans` contains 345 generated plan/prompt/agent/history files with no active boundary marker.
- Why it may poison context: benchmark evidence under `skills/` or unmarked generated plans can be mistaken for skill instructions or active planning context.
- Suggested disposition: move benchmark artifacts out of `.claude/skills` or mark non-loadable; add an archival boundary for `.claude/plans`.

## Findings By Location

- `docs/standards/explanation/architecture.md`: sample-system codemap, compatibility markers, and repeated route/proof scaffolding.
- `docs/standards/explanation/roadmap.md`: EventPipeline-style roadmap examples, exact issues/milestones, and compatibility-window records.
- `docs/standards/explanation/design-doc.md`: concrete event-contract examples and broad cross-cutting implication records.
- `docs/standards/explanation/test-strategy.md`: C#/.NET testing tool catalog and broad risk/gate records.
- `docs/standards/explanation/adr.md`: exact ADR IDs/dates in examples and repeated route checks.
- `docs/standards/reference/support-matrix.md`: Rasm-like host/runtime/generated-API examples, command migration example, exact lifecycle field names, and repeated proof packets.
- `docs/standards/reference/readme.md`: local route taxonomy in generic README examples and repeated authoring-contract scaffolding.
- `docs/standards/reference/reference.md`: proof/source templates, quality-rail command example, and authoring/validation repetition.
- `docs/standards/reference/api.md`: OpenAPI exact-version claim and repeated contract/proof packet.
- `docs/standards/reference/code-documentation.md`: exact language baselines and concrete LanguageExt/Thinktecture/Effect doctrine.
- `docs/standards/agentic-documentation.md`: provider-specific prompt behavior claims without adjacent proof.
- `docs/standards/proof.md`: overbroad agent-surface evaluation receipt machinery.
- `docs/standards/agents-md.md`: language/repo owner-rail examples inside generic `AGENTS.md` authoring rules.
- `docs/standards/AGENTS.md` and `docs/standards/README.md`: repeated standards read-order/routing mechanics.
- `docs/standards/_reports/AGENTS.md`: process-heavy report mechanics and date-coded archive examples.

## Deferred Or Excluded Material

- `docs/standards/_reports/**` bodies were excluded by request. `docs/standards/_reports/AGENTS.md` was read and audited.
- `~/.codex/memories/**`, rollout summaries, plugin caches, vendor imports, and tmp folders were excluded from the active `~/.codex` findings. Agent `10` included hidden `.system` skills where they appeared under active `~/.codex/skills`.
- `~/.claude/plugins/cache/**` and `~/.claude/plugins/marketplaces/**` were inventoried as vendor/plugin material, not active user-authored instruction.
- `~/.claude/plans/**` was inventoried as a 345-file archival/generated plan area, not read exhaustively because no load evidence promoted it to active instruction. It remains a risk if future tooling or prompts treat it as active.
- `node_modules/**`, generated output, build output, caches, artifacts, dependency folders, `bin/`, and `obj/` were excluded from code/comment findings.
- The code/comment pass did not line-read all 290 scoped files; it used broad candidate search plus deep reads of high-hit operator docs and representative comment/docstring clusters.
- No external provider/API freshness verification was performed in this first wave; version/currentness findings flag the storage of drift-prone claims, not whether every claim is factually stale on 2026-06-06.

## Open Questions

- Should standards examples use one shared neutral toy domain at all, or should every example be placeholder-only unless a real source route owns it?
- Should exact target baselines live in generic type standards, or in a single project/tool baseline owner that type standards reference abstractly?
- Should provider-specific behavior be removed from the standards corpus entirely and treated as a maintained provider-reference artifact?
