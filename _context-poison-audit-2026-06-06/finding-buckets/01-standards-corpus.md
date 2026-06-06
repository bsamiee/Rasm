# Standards Corpus Findings

Source: `../final-report.md`
Scope: `docs/standards/**`, including `_reports/AGENTS.md` only.
Finding count: 26

The finding blocks below are copied verbatim from `../final-report.md`.

## Findings

#### F-STD-EXPL-01: Explanation standards reuse concrete EventPipeline-style examples as if they were source-backed project facts
- Severity: high
- Reports: `agent-reports/03-standards-explanation.md` F1, F5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:192`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:141`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:151`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/adr.md:52`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:173`
- Evidence:
  - `architecture.md` uses a full pseudo tree with `<package-root>/`, `Contracts/`, `Admission/`, `Execution/`, `Storage/`, `Legacy/`, `[ACTIVE M2]`, and `[DEPRECATED]`.
  - `roadmap.md` uses `# [EVENT_PIPELINE_ROADMAP]`, `src/EventPipeline/`, `EventPipeline.csproj`, project board `event-pipeline`, `issue/101`, `issue/112`, `M1`, and `M2`.
  - `design-doc.md` uses `# [FREEZE_EVENT_CONTRACT]`, a concrete date, and event-contract consumer language.
  - `adr.md` uses exact ADR IDs and dates such as `0001`, `0007`, `2026-01-12`, `2026-03-04`, and `0023`.
- Why it may poison context: concrete-looking examples in a generic standards corpus can be copied into unrelated docs as hidden canonical project truth or fake proof.
- Suggested disposition: replace repeated sample-system names, exact dates, checked proof phrasing, issue IDs, and concrete project files with neutral placeholders unless the exact value is the subject of the rule.

#### F-STD-REF-01: Reference standards encode Rasm-like support and route surfaces in generic examples
- Severity: high
- Reports: `agent-reports/04-standards-reference.md` F-01, F-03
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:179`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:252`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:322`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:351`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:368`
- Evidence:
  - `support-matrix.md` examples use `Library projects`, `Host runtime refs`, `Generated API lookup`, `host manifest`, `<api metadata command>`, host-specific collection semantics, and maintained metadata.
  - `readme.md` examples route to standard-library/runtime/package posture, external dependency facts, local adoption posture, test-tool API facts, and local gate routing.
- Why it may poison context: generic reference standards can make this repository's host/runtime/API-map posture look like a universal README/support-matrix model.
- Suggested disposition: replace with neutral surfaces such as `Client library`, `Managed runtime`, `Provider integration`, `API contracts`, `operations guide`, and `<compatibility-check output>`; keep Rasm-like support wording only in explicitly project-specific docs.

#### F-STD-EXPL-02: Test strategy standard publishes a C#/.NET tool catalog as generic strategy shape
- Severity: high
- Reports: `agent-reports/03-standards-explanation.md` F2
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:218`
- Evidence: example rows name `xUnit/MTP`, `CsCheck`, `coverlet`, `Stryker`, `Verify`, `ArchUnitNET`, `SharpFuzz`, `BenchmarkDotNet`, and `bridge runtime`, while the same file says repository truth should own gate names, commands, runners, artifacts, and release policy.
- Why it may poison context: agents can treat one ecosystem's testing stack as a default for all test strategies and invent unsupported rails in generic docs.
- Suggested disposition: replace concrete tool names with `<unit-test-runner>`, `<mutation-tool>`, `<snapshot-artifact>`, `<architecture-gate>`, and `<host-runtime-scenario>` in the generic standard; keep concrete mappings in project/tool references.

#### F-STD-REF-02: Code-documentation capsules import concrete library doctrine into a generic code-doc standard
- Severity: low
- Reports: `agent-reports/04-standards-reference.md` F-08
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:145`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:157`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:165`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:177`
- Evidence: language capsules name `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`, `IO<T>`, `K<F,T>`, Thinktecture value objects/smart enums/unions, and TypeScript `Effect`, `Exit`, `Cause`, `Stream`, and `Layer`.
- Why it may poison context: agents may treat LanguageExt, Thinktecture, and Effect-specific documentation doctrine as universal for unrelated projects.
- Suggested disposition: make the generic rule carrier-agnostic: document success, typed failure, runtime context, and resource contracts for the project's declared effect/value-object system; route concrete library names to language skills or project-specific code docs.

#### F-STD-EXPL-03: Compatibility-window examples make stale-path preservation look normal
- Severity: medium
- Reports: `agent-reports/03-standards-explanation.md` F3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:123`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:180`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:11`
- Evidence: examples repeatedly use `legacy export retirement`, `compatibility window`, `support-controlled legacy path`, `[DEPRECATED]`, `migration`, `Legacy/`, and `<legacy-reader> [DEPRECATED]`.
- Why it may poison context: generic standards can train agents to preserve stale paths and compatibility overlays instead of deleting them or routing the exceptional case to a support owner.
- Suggested disposition: keep at most one support-controlled retirement example and make the positive rule deletion or support-matrix routing when live support proof exists.

#### F-STD-REF-03: Support-matrix deprecation example reads like a stale local command migration
- Severity: high
- Reports: `agent-reports/04-standards-reference.md` F-02
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:340`
- Evidence: deprecation record uses `former split inventory command`, `<replacement health-check command>`, `<tool> <replacement-command>`, `source inventory`, and `one declared response envelope`.
- Why it may poison context: the example can train support matrices to preserve command lineage and local tool-rebuild narratives instead of documenting current support status.
- Suggested disposition: replace with an abstract deprecated API or feature support example, or move command deprecation shape to a command/API reference standard with neutral names.

#### F-STD-EXPL-04: Explanation type standards repeat route catalogs across authoring contract, boundaries, and validation
- Severity: medium
- Reports: `agent-reports/03-standards-explanation.md` F4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/architecture.md:17`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/roadmap.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:24`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/adr.md:21`
- Evidence: the same neighbor-route obligations appear in opening route-away text, adjacent checks, boundary sections, and validation checklists.
- Why it may poison context: repeated route catalogs encourage future docs to paste neighboring-standard inventories rather than link only when reader action changes.
- Suggested disposition: keep one route-away rule and one compact boundary section per type standard; collapse validation to a general adjacent-link rule.

#### F-STD-REF-04: Reference type standards repeat proof packets and authoring-contract boilerplate
- Severity: medium
- Reports: `agent-reports/04-standards-reference.md` F-06, F-07; `agent-reports/06-standards-systemic-crosscut.md` F1, F4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:20`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:93`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md:119`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:117`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/readme.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:400`
- Evidence:
  - Type standards repeat maximum-field proof packets: `Evidence`, `Generated from`, `Controlling source`, `Proof gap`, `Last verified`, `Review trigger`, and sometimes `Refresh command`, `Imported fields`, or `Missing-value rule`.
  - Each reference type opens with `AUTHORING_CONTRACT` fields and closes with long `VALIDATION` checklists.
- Why it may poison context: future generated docs may mechanically fill provenance packets and authoring metadata as boilerplate, even when the reader needs a smaller fact record.
- Suggested disposition: keep only type-specific deltas in each type standard; move common field semantics to shared standards and mark full proof packets as maximum field sets with untriggered fields omitted.

#### F-STD-REF-05: Command-reference example uses quality-rail language and blurs subcommands with flags
- Severity: medium
- Reports: `agent-reports/04-standards-reference.md` F-04
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/reference.md:329`
- Evidence: command fact entry uses `Invocation: <tool> report <scope>`, `Flag: report`, and `Effect: reports diagnostics for the scoped project closure`.
- Why it may poison context: generic command references can inherit local quality-tool vocabulary and imprecise command-surface modeling.
- Suggested disposition: use a neutral flag example such as `<tool> export --format <format>` with `Flag: --format`, or separate `Subcommand:` from `Flag:`.

#### F-STD-REF-06: Exact version and lifecycle fields in reference standards may age into false authority
- Severity: medium
- Reports: `agent-reports/04-standards-reference.md` F-05
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/api.md:129`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:71`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:121`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:125`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/code-documentation.md:217`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/reference/support-matrix.md:130`
- Evidence: standards mention OpenAPI 3.2.0, language capsules for C# 14 / TypeScript 7 / Python 3.15 / Bash 5.3+ / PostgreSQL 18.4, Python PEP details, and exact lifecycle schema fields such as `isEoas`, `eoasFrom`, `isEol`, `eolFrom`, `isEoes`, and `isDiscontinued`.
- Why it may poison context: exact future/current-looking versions can be retrieved as authoritative facts without their target-standard caveat or current proof route.
- Suggested disposition: prefer `project-declared language baseline` or a clearly labeled `Target baselines` record with evidence/proof-gap routing; cite a maintained source route for OpenAPI/lifecycle schema claims.

#### F-STD-EXPL-05: Broad cross-cutting implication records can force `n/a` filler
- Severity: low
- Reports: `agent-reports/03-standards-explanation.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:171`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:192`
- Evidence: `design-doc.md` requires broad security/privacy/accessibility/internationalization/data/operational/compatibility/runtime records and permits `Applies: yes | no`; `test-strategy.md` requires at least High and Extreme risks currently local.
- Why it may poison context: authors may fill broad mandatory records with `n/a` metadata to satisfy the standard rather than documenting only action-changing concerns.
- Suggested disposition: trigger broad concern records only when a source, proof route, risk, or reader action changes.

#### F-STD-ROOT-01: `agentic-documentation.md` embeds provider-specific prompt behavior as durable standards prose
- Severity: high
- Reports: `agent-reports/01-standards-root-shared.md` F01
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md:208`
- Evidence: provider-specific sections define `OPENAI_CODEX`, `CLAUDE_CODE`, and `GEMINI` defaults for structure, delimiters, long context, structured output, state, caching, output control, and loading behavior; the file warns that provider rules need proof but does not attach claim-level proof records beside each provider claim.
- Why it may poison context: standards become a provider manual, and future prompts/docs can copy stale provider behavior as durable truth.
- Suggested disposition: collapse to provider-agnostic artifact rules, or move provider-specific claims into a maintained provider reference with evidence, freshness trigger, and proof route.

#### F-STD-ROOT-02: Proof evaluation receipt machinery is too heavy for ordinary docs and prompts
- Severity: medium
- Reports: `agent-reports/01-standards-root-shared.md` F06; `agent-reports/02-standards-root-shared-adversarial.md` 3.6
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md:197`
- Evidence: `proof.md` defines agent-surface evaluation receipts with stochastic rigor fields such as 20-50 questions, 3-5 trials, Wilson score confidence intervals, traces, latency, tool errors, token budget, and provider/model version.
- Why it may poison context: ordinary docs/prompts/reports can overproduce evaluation metadata or copy sample values as pseudo-proof when no evaluation ran.
- Suggested disposition: keep a minimal deterministic receipt in shared proof guidance and move stochastic evaluation rigor behind a strict trigger for retrieval quality, ranking, tool selection, latency, or provider behavior claims.

#### F-STD-ROOT-07: `agentic-documentation.md` is broad enough to leak MCP/retrieval/prompt machinery into ordinary docs
- Severity: medium
- Reports: `agent-reports/02-standards-root-shared-adversarial.md` 3.4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agentic-documentation.md:168`
- Evidence: one standard owns salience, artifact separation, task output contracts, provider posture, generated mirrors, retrieval stores, machine-readable indexes, MCP resources, MCP prompts, MCP tools, and structured-output contracts.
- Why it may poison context: ordinary READMEs, standards, or prompts can import MCP/retrieval/provider machinery simply because they are "agentic", even when no maintained surface exists.
- Suggested disposition: add an eligibility gate: apply MCP, retrieval, generated mirror, and structured-output sections only when a real deployed/maintained surface exists or the user explicitly asks to design one.

#### F-STD-ROOT-03: `agents-md.md` imports language and repo implementation rails into generic overlay guidance
- Severity: high
- Reports: `agent-reports/01-standards-root-shared.md` F02; `agent-reports/02-standards-root-shared-adversarial.md` 3.2
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:129`
- Evidence: `agents-md.md` prescribes trigger-owner-action wording, then defines C#, TypeScript, Python, Bash, SQL, tests, and bridge owner rails including C# `Fin`/`Validation`/`Eff`, TypeScript Effect layers, Python `msgspec`, host-facing assertions, and host-runtime scenario proof.
- Why it may poison context: an `AGENTS.md` authoring standard can cause generic overlays to carry project/language implementation rail names already owned by root policy, skills, and local subtree overlays.
- Suggested disposition: replace the rail chooser with carrier-neutral overlay guidance: name the local canonical owner and extension action only when the folder has one; route language-specific rail examples to skills or local overlays.

#### F-STD-ROOT-08: Broad `_reports/` session trigger can override user-specified one-off audit paths
- Severity: high
- Reports: `agent-reports/02-standards-root-shared-adversarial.md` 3.1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/agents-md.md:203`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md:9`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:17`
- Evidence: `agents-md.md` says investigations, critiques, audits, planning, or multi-pass agent output that may be reused later should create/update a `_reports/<top-slug>-<ddmmyy>/` session, while other standards say `_reports/**` is excluded unless named.
- Why it may poison context: nearly every serious audit may be reusable later, so agents can interpret the rule as an obligation to create standards `_reports/` sessions even when the user supplied a different output path or asked for a one-off report.
- Suggested disposition: restrict `_reports/` session creation to explicit user requests, existing `_reports/**` targets, or trusted owner-directed durable promotion work; add a counter-rule for user-specified output paths.

#### F-STD-ROOT-04: Root/shared standards repeat read-order and validation mechanics enough to inflate narrow tasks
- Severity: medium
- Reports: `agent-reports/01-standards-root-shared.md` F04, F05; `agent-reports/06-standards-systemic-crosscut.md` F2
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/AGENTS.md:13`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md:33`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md:413`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/proof.md:152`
- Evidence: `AGENTS.md` and `README.md` both define standards read order; each shared standard closes with a validation checklist; `information-structure.md` requires long standards to have a validation section while `proof.md` already owns gate selection.
- Why it may poison context: every standards edit can become a full-corpus audit, and future docs/prompts can inherit validation sections as ceremony rather than owner-specific proof selectors.
- Suggested disposition: keep local read deltas only in overlays; keep proof gate selection in `proof.md`; shorten per-standard validation to owner-specific checks.

#### F-STD-ROOT-05: Anonymous `project-declared implementation-proof owner` is repeated without naming the route
- Severity: medium
- Reports: `agent-reports/01-standards-root-shared.md` F03
- Location: `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/README.md:28`
- Evidence: `README.md` repeats `project-declared implementation-proof owner` at lines 28, 205, and 227 without naming the concrete owner file.
- Why it may poison context: agents can copy the phrase as provenance chatter instead of routing to a real owner.
- Suggested disposition: name the concrete active route or rewrite as a generic fallback to the active repository instruction route for implementation proof outside the standards corpus.

#### F-STD-TASK-01: Task and learning standards use Markdown/Git validation mini-artifacts as reusable examples
- Severity: high
- Reports: `agent-reports/05-standards-task-learning.md` F1, F2
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:80`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:167`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md:190`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:107`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:251`
- Evidence: examples use `# [VALIDATE_MARKDOWN_CHANGE]`, `# [VALIDATE_MARKDOWN_DIFF]`, `git status --short -- <changed-paths>`, `git diff --check -- <changed-paths>`, local path/anchor validation, `[PASS] no whitespace drift`, `[SKIP] validator access needed`, and a copyable `Last verified: 2026-06-04`.
- Why it may poison context: generic how-to/tutorial/contributing standards can train unrelated docs to center repository Markdown validation ladders and proof fields rather than task-specific reader outcomes.
- Suggested disposition: replace docs-only Git validation examples with neutral artifacts; route docs-as-code gate selection back to `proof.md`; remove fixed dates or use `YYYY-MM-DD`.

#### F-STD-ROOT-06: `_reports/AGENTS.md` is correctly quarantined but process-heavy enough to leak if promoted
- Severity: low
- Reports: `agent-reports/01-standards-root-shared.md` F07, F08; `agent-reports/02-standards-root-shared-adversarial.md` 3.5; `agent-reports/06-standards-systemic-crosscut.md` F5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:21`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:64`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:82`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/_reports/AGENTS.md:155`
- Evidence: the report overlay defines session folders, track names, report types, report shapes, merge keys, archive operations, and examples such as `agents-md-050626`, `code-documentation-050626`, and `proof-060626`.
- Why it may poison context: if agents promote mechanics instead of findings, active standards can inherit report-process scaffolding, date-coded examples, and archive vocabulary.
- Suggested disposition: keep the file as source-material mechanics; sharpen the lead that report vocabulary must not be copied into active standards or non-report artifacts; add a one-off/user-specified-output exception so `_reports/AGENTS.md` packet mechanics do not leak into arbitrary reports.

#### F-STD-TASK-02: Onboarding standard leaks chat-history and instruction-source provenance
- Severity: high
- Reports: `agent-reports/05-standards-task-learning.md` F3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:68`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:293`
- Evidence: the standard says ramps help agents prepare "without replaying chat history", bans metadata through a long provenance-aware section, and validates against the "strict agent-only metadata ban in [AGENTS.md](../AGENTS.md)".
- Why it may poison context: durable reusable standards can teach generated ramps to mention chat history, instruction-source bans, or provenance chatter rather than producing the ramp.
- Suggested disposition: remove chat-history phrasing and `AGENTS.md` citation from produced-ramp validation; state the durable rule directly: ramps omit role/team/owner/progress scaffolding unless a named consumer reads it.

#### F-STD-TASK-03: Tutorial threshold rules may force arbitrary generated lesson shapes
- Severity: medium
- Reports: `agent-reports/05-standards-task-learning.md` F4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:57`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:90`
- Evidence: `tutorial.md` repeats a three-lesson threshold, `3 to 12` checkpoint steps, an "under about 15 minutes" compactness rule, and "local rule" phrasing.
- Why it may poison context: generated tutorials may force-fit counts, steps, and duration estimates instead of shaping lessons around learner proof.
- Suggested disposition: keep one cardinality rule only where it prevents bad artifacts, remove repeated "local rule" phrasing, and avoid time estimates as structural triggers unless published duration is part of the artifact.

#### F-STD-TASK-04: Runbook response diagram mostly duplicates required section order
- Severity: low
- Reports: `agent-reports/05-standards-task-learning.md` F5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:62`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:102`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:232`
- Evidence: optional Mermaid diagram renders `Trigger -> Impact -> Safety -> Triage -> Decision -> Mitigation/Rollback/Escalation -> Verification/Evidence`, mirroring the required structure below.
- Why it may poison context: agents may copy a decorative lifecycle diagram into produced runbooks even when the section spine or a decision table is clearer.
- Suggested disposition: remove the generic diagram or replace it with a branch-specific example that cannot be represented well by the required structure alone.

#### F-STD-SYS-01: Exact produced skeletons are sticky enough to generate over-structured docs
- Severity: medium
- Reports: `agent-reports/06-standards-systemic-crosscut.md` F3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md:53`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:131`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:31`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:42`
- Evidence: multiple type standards publish exact produced spines with many mandatory sections, conditional sections, section-order rules, heading mechanics, and field packets.
- Why it may poison context: produced docs can become standards-shaped instead of reader-shaped, especially lightweight docs that only need one goal, one path, and one proof statement.
- Suggested disposition: rephrase spines as minimum valid contracts plus allowed local sections; keep exact order only where safety-critical.

#### F-STD-SYS-02: Status vocabularies and progress notation can become default decoration
- Severity: low
- Reports: `agent-reports/06-standards-systemic-crosscut.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md:120`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/information-structure.md:157`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/formatting.md:77`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/formatting.md:128`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:146`
- Evidence: the corpus defines lifecycle vocabularies, relation-record field order, marker families, 20-cell progress bar mechanics, and type-local availability vocabularies.
- Why it may poison context: future docs may add lifecycle fields, availability terms, progress bars, or bracketed tokens because the standards expose attractive state machinery.
- Suggested disposition: add a "state earns status" rule: use status only when a maintained actor filters, updates, or removes the item by state; use progress bars only when numerator, denominator, closure rule, and proof surface are visible.

#### F-STD-TASK-05: Adjacent-check lines repeat across task and learning standards but are mostly conditional
- Severity: note
- Reports: `agent-reports/05-standards-task-learning.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/contributing.md:23`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/how-to.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/task/runbook.md:17`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/onboarding.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/learning/tutorial.md:19`
- Evidence: each file uses `AUTHORING_CONTRACT` with `Agent use`, produced structure, cardinality, adjacent checks, and maintenance triggers.
- Why it may poison context: broad adjacent-check inventories can train agents to perform or paste cross-doc checklists, but the report notes these are mostly intentional and conditionally worded.
- Suggested disposition: do not treat as a primary cleanup target; if tightening, shorten broad inventories while preserving conditional triggers.
