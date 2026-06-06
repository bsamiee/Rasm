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

- Imported `agent-reports/03-standards-explanation.md`: read `docs/standards/explanation/{adr,architecture,design-doc,roadmap,test-strategy}.md`; produced six findings about concrete sample-system leakage, C#/.NET gate leakage, compatibility examples, repeated route catalogs, exact example proof values, and broad cross-cutting records.
- Imported `agent-reports/04-standards-reference.md`: read `docs/standards/reference/{api,code-documentation,readme,reference,support-matrix}.md`; produced eight findings about Rasm-like support examples, stale command-migration examples, local route taxonomy leakage, quality-rail command examples, exact version claims, repeated proof packets, authoring-contract boilerplate, and library-specific code-doc doctrine.
- Imported `agent-reports/01-standards-root-shared.md`: read standards root/shared files plus `docs/standards/_reports/AGENTS.md`; produced eight findings about provider-specific prompt rules, implementation rail leakage in `agents-md.md`, anonymous implementation-proof routing, validation/read-order boilerplate, overbroad evaluation receipts, and report-process leakage.
- Imported `agent-reports/05-standards-task-learning.md`: read `docs/standards/task/{contributing,how-to,runbook}.md` and `docs/standards/learning/{onboarding,tutorial}.md`; produced six findings about Markdown/Git validation examples, proof/gate over-specification, onboarding provenance chatter, rigid tutorial thresholds, decorative runbook diagrams, and adjacent-check repetition.
- Imported `agent-reports/02-standards-root-shared-adversarial.md`: independently read standards root/shared files plus `docs/standards/_reports/AGENTS.md`; confirmed provider-claim, owner-rail, report-shape, and evaluation-rigor risks; added sharper findings about the broad `_reports/` trigger and `agentic-documentation.md` catch-all width.
- Imported `agent-reports/06-standards-systemic-crosscut.md`: read active `docs/standards` files systemically; confirmed authoring-contract duplication, root-audit mechanics leakage, proof-packet duplication, and `_reports` quarantine risk; added sticky produced-skeleton and status/progress-notation risks.
- Imported `agent-reports/07-docs-non-standards.md`: read 38 non-standards docs across `docs/external-libs`, `docs/testing-libs`, `docs/system-api-map`, `docs/usage.md`, and `docs/host-libraries.md`; produced findings about instruction markers in ordinary docs, generic API docs leaking project context, weak provenance labels, duplicated route/proof posture, defensive version wording, local examples, and bracketed group-label overuse.
- Imported `agent-reports/08-repo-instructions-prompts.md`: read root `CLAUDE.md`, root/nested `AGENTS.md`, `.claude/prompts/*.md`, and hook-adjacent files; produced findings about reusable prompts encoding fixed session choreography, Assay/Rasm coupling, copied validation/stress ladders, process/provenance chatter, `CLAUDE.md` command catalogs, duplicated global mechanics, and a brittle provider-budget fact.
- Imported `agent-reports/09-project-claude-skills.md`: inventoried 126 project-local skill Markdown files under `.claude/skills/**`; produced findings about static GitHub Actions version/SHA catalogs, visible reasoning handling in Perplexity tooling, Rasm-coupled `testing-cs`, duplicated validation ladders, contradictory templates, skill-eval prompt sections, local environment leakage, defensive version wording, and project/brand coupling in language skills.
- Imported `agent-reports/10-home-codex-skills.md`: audited `~/.codex/AGENTS.md` and active `~/.codex/skills/**` excluding memories/caches/vendor/tmp; produced findings about global policy duplication, Rasm-specific analyzer rulecraft in a global skill, Parametric Portal leakage in `testing-ts`, Rasm-style `testing-cs`, duplicated command/validation catalogs, frozen version/model claims, orchestration meta, and local package/path examples.
- Imported `agent-reports/11-home-claude-active.md`: read active user-authored `~/.claude` instruction/rule/config surfaces and inventoried archives/vendor areas; produced findings about global policy duplication, conflicting research windows, mandatory footer, overbroad no-control-flow rule, fixed delegation trigger, benchmark artifacts in a skill-looking path, unmarked plan archives, and duplicated documentation hygiene.
- Imported `agent-reports/12-code-comments-tools.md`: searched 290 scoped source/prose files and 34 scoped Markdown/instruction files outside `docs/` and `.claude`; deep-read high-hit operator docs and representative comments; produced findings about tool README migration/process chatter, bridge agent guidance and legacy wire notes, AppUi/Compute/Persistence transient evidence, test metadata docstrings, repeated `BRIDGE-DEFERRED`, local examples, and mostly clean source comments.

## Findings By Category

### Generic Standards Coupled To Concrete Sample Systems

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

### Generic Standards Leak Toolchain-Specific Defaults

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

### Compatibility And Legacy Normalized In Examples

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

### Repeated Route And Proof Boilerplate

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

### Fragile Exact Version Or Lifecycle Claims

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

### Overbroad Required Records

#### F-STD-EXPL-05: Broad cross-cutting implication records can force `n/a` filler
- Severity: low
- Reports: `agent-reports/03-standards-explanation.md` F6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/design-doc.md:171`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/standards/explanation/test-strategy.md:192`
- Evidence: `design-doc.md` requires broad security/privacy/accessibility/internationalization/data/operational/compatibility/runtime records and permits `Applies: yes | no`; `test-strategy.md` requires at least High and Extreme risks currently local.
- Why it may poison context: authors may fill broad mandatory records with `n/a` metadata to satisfy the standard rather than documenting only action-changing concerns.
- Suggested disposition: trigger broad concern records only when a source, proof route, risk, or reader action changes.

### Provider And Agent-Surface Claims Without Tight Proof

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

### Generic Overlay Standards Leak Implementation Doctrine

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

### Shared Standards Duplicate Read And Validation Mechanics

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

### Report Mechanics Leaking Into Active Guidance

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

### Prompt Or Session Provenance In Durable Standards

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

### Sticky Skeletons And Decorative Structures

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

### Non-Standards Docs Carry Instruction Syntax Or Project Posture

#### F-DOC-01: Ordinary docs use instruction-only invocation markers
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.1
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/packages.md:59`
- Evidence: all 38 target non-standards docs matched `[IMPORTANT]` or `[CRITICAL]`.
- Why it may poison context: reference docs can be mistaken for higher-ranked instructions, and future artifacts may reproduce agent-weighting syntax in ordinary reader-facing prose.
- Suggested disposition: replace invocation markers with ordinary lead prose or GitHub alerts only when the container genuinely interrupts the reader; reserve `[IMPORTANT]`/`[CRITICAL]` for instruction surfaces.

#### F-DOC-02: Generic-looking library/test API docs carry Rasm, host, analyzer, testkit, and local-path context
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.2, 2.6
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/api.md:35`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/mathnet/api.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/thinktecture/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/archunit/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/verify/api.md:42`
- Evidence: generic `api.md` leaves mention workspace imports, local XML proof, project usage owners, package-state docs, architecture test project, local analyzer, project testkit, and `tests/csharp/_tooling/ModuleInitializers.cs`; examples use `DomainRoot`, `BoundaryAdapterAttribute`, `AdmissionRegistry`, `Vectors.Dimension.TryCreate`, "Yuksel WSE", and "Cholesky vs LU".
- Why it may poison context: portable API references become project instruction carriers and can import Rasm/Rhino/testkit assumptions into unrelated work.
- Suggested disposition: keep portable package API facts in `api.md`; move local graph/XML/testkit/analyzer/path facts into explicit project posture files such as `rasm.md`, `docs/usage.md`, `docs/system-api-map/**`, or test overlays.

#### F-DOC-03: Non-standards docs use ad hoc source/provenance labels without proof-standard fields
- Severity: high
- Reports: `agent-reports/07-docs-non-standards.md` 2.3
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/xunit/api.md:15`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:64`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:120`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/stryker/api.md:52`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/coverlet/api.md:14`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/mathnet/api.md:26`
- Evidence: docs use `[SOURCE]`, "local proof showed", and "Verified control surfaces include..." without claim-local `Evidence:`, `Last verified:`, `Review trigger:`, or `Proof gap:`.
- Why it may poison context: provenance-looking text appears authoritative but is not refreshable and teaches source-label chatter instead of claim-level proof.
- Suggested disposition: convert only drift-prone claims to proof-standard fields; remove decorative source labels and route stable facts to owners.

#### F-DOC-04: Project graph, proof order, and local command routing are duplicated across non-standards docs
- Severity: medium
- Reports: `agent-reports/07-docs-non-standards.md` 2.4
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/usage.md:58`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:24`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/README.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/packages.md:76`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/README.md:18`
- Evidence: package graph, host policy, cross-stack usage, proof routing, local XML/decompile proof, boundary adapters, testing package injection, mutation tooling, and direct test rails repeat across multiple docs.
- Why it may poison context: generated docs can copy route boilerplate and agent-only context, and owners drift when proof order changes.
- Suggested disposition: keep proof/order routing in `docs/usage.md`, `docs/system-api-map/**`, and owner-local instructions; reduce generic indexes to short owner links.

#### F-DOC-05: Non-standards docs retain defensive version/migration wording without consistent freshness triggers
- Severity: medium
- Reports: `agent-reports/07-docs-non-standards.md` 2.5
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/csharp/language.md:6`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/csharp/language.md:101`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/languageext/effects.md:46`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/thinktecture/sourcegen.md:31`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:155`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/host-libraries.md:41`
- Evidence: docs warn against `LangVersion=preview`/`latest`, list out-of-scope statuses, carry v4/v5 and v10 migration deltas, reject legacy spans, and instruct refreshing latest package versions before first consumer.
- Why it may poison context: stale baseline caveats can become active doctrine and encourage defensive "old-vs-new" prose in durable docs.
- Suggested disposition: keep version facts only in explicit support/API delta references with freshness fields; rewrite target guidance as direct rules where current availability is not the point.

#### F-DOC-06: Bracketed group-label overuse reinforces template-style prose in ordinary docs
- Severity: low
- Reports: `agent-reports/07-docs-non-standards.md` 2.7
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/external-libs/README.md:16`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/bcl.md:28`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/system-api-map/replacements.md:25`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/cscheck/api.md:48`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/docs/testing-libs/verify/api.md:29`
- Evidence: repeated `[FILES]`, `[USE]`, `[OWNER]`, `[DETAIL]`, `[WHY_VERIFY_FITS]`, and `[EXTRAS]` labels behave like heading surrogates or template residue.
- Why it may poison context: future generated docs may emit agent-facing label packets instead of normal prose or real headings.
- Suggested disposition: keep group labels only when they introduce a real set; convert repeated local labels to H3 sections or prose.

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

### Tool And Code Prose Preserve Transitional Process State

#### F-TOOL-01: Assay README carries transitional adoption state as durable operator truth
- Severity: high
- Reports: `agent-reports/12-code-comments-tools.md` L-01
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:7`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:218`
- Evidence: README repeats Assay as intended successor to `tools.quality`, not root-canonical yet, use-now/update-when guidance, command replacement intent, and old payload-location migration notes.
- Why it may poison context: agents reason from an in-between migration state and preserve compatibility language instead of following the current root owner route.
- Suggested disposition: collapse to one short status fact or remove after root routing settles; keep command behavior in command tables and migration policy in the root quality owner.

#### F-TOOL-02: Assay README embeds proof/source process schema in operator manual
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-02
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:300`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:302`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:337`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:350`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:374`
- Evidence: operator manual uses `Current truth`, `Caveat`, `Reader action`, "how agents consume proof", "task System Information", source-owner tables, maintainer-flow instructions, README validation commands, and blocker recording.
- Why it may poison context: tool manual becomes a second docs standard and reinforces task-process vocabulary.
- Suggested disposition: keep operator-facing contract only; move proof/source-owner grammar to overlays or docs standards; remove task-process references.

#### F-TOOL-03: `tools/quality` README mixes operator reference with agent instruction and validation boilerplate
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-03
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:13`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:262`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:286`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/README.md:295`
- Evidence: README calls tool an "agent-only CLI", tells readers how to report evidence, includes "no World-A/World-B duality", has `AGENT_ROUTING`, tells readers to load `.claude/skills/coding-python/SKILL.md`, and includes README/Mermaid/dependency-currency validation commands.
- Why it may poison context: README acts as standing instruction authority instead of just tool reference.
- Suggested disposition: reduce `AGENT_ROUTING` to operator rail selection if needed; move skill-loading and validation ladders to overlays or root quality route.

#### F-TOOL-04: Rhino bridge README preserves agent guidance, legacy wire migration, and local WIP evidence
- Severity: high
- Reports: `agent-reports/12-code-comments-tools.md` L-04
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:3`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:18`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:32`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:203`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:235`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/rhino-bridge/README.md:241`
- Evidence: README instructs coding agents, names `Coding Agent` in diagram, describes "structured agent evidence", has `Agent guidance`, `Wire format migration`, legacy `key=value` parsing warning, and current local XML/WIP evidence.
- Why it may poison context: runtime operator reference becomes agent memo, migration note, and evidence transcript; legacy wire behavior stays alive in retrieval.
- Suggested disposition: keep current marker/output/scenario contract; move bridge-use policy to `tools/rhino-bridge/AGENTS.md`; move transient WIP/XML evidence to API/source owner or dated report.

#### F-TOOL-05: AppUi/Compute/Persistence docs carry transient host gates, version-scared caveats, and `.claude` source anchors
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-05, L-06
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:54`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:112`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppUi/ROADMAP.md:75`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Compute/_ARCHITECTURE.md:180`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:35`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:165`
- Evidence: docs include `[DEFERRED]` GH2-Avalonia embedding research, decompiled DLL evidence, "per WIP drop" commands, settled/still-open native gates, "needs an agent with the macOS host", CoreML/ORT/Rhino crash caveats, deprecated package choices, no-version-pin language, and `.claude/skills` source anchors.
- Why it may poison context: architecture and roadmap docs preserve investigation transcripts and version-sensitive package fear instead of stable decisions plus proof routes.
- Suggested disposition: split durable constraints from evidence chronology; route command gates/dates to runbook/report; remove `.claude` skill paths from architecture unless they are active instruction sources.

#### F-TOOL-06: Test modules embed audit metadata, placeholders, and repeated `BRIDGE-DEFERRED` posture
- Severity: medium
- Reports: `agent-reports/12-code-comments-tools.md` L-07, L-08
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_engine.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_model.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/core/test_rail_package.py:1`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/tools/assay/conftest.py:269`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:9`
- Evidence: tests include `Source surface`, `Laws`, `xfail strict`, `bedrock: coverage pending`, methodology-hole comments, `foundation/W3 law`, and repeated all-caps `BRIDGE-DEFERRED` comments across static specs.
- Why it may poison context: retrieval can treat law-matrix metadata and deferred proof tokens as current proof inventory or accepted incomplete state.
- Suggested disposition: keep executable test names/assertions as truth; shorten or remove module docstrings that restate ownership; replace `bedrock` skips with issue/xfail conditions or tests; keep bridge-deferred comments only where they change behavior.

#### F-TOOL-07: Project-coupled examples appear in reusable layout/tool docs
- Severity: low
- Reports: `agent-reports/12-code-comments-tools.md` L-09
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md:13`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/README.md:41`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/README.md:141`
- Evidence: docs use `apps/grasshopper/Radyab/`, `apps/grasshopper/Radyab/Radyab.csproj`, exact folder layout, `tests/csharp/libs/Rasm.Rhino`, and `rasm-bridge` as examples.
- Why it may poison context: if treated as reusable conventions, future agents may copy Radyab or `rasm-bridge` paths into unrelated plugin/package work.
- Suggested disposition: keep only if intentionally repo-specific; otherwise use placeholders or label as current exemplar.

#### F-TOOL-08: Source comments are mostly clean; a few retain narrative/provenance phrasing
- Severity: note
- Reports: `agent-reports/12-code-comments-tools.md` L-10
- Locations:
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/composition/registry.py:564`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/assay/composition/registry.py:613`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/tools/quality/rails/api.py:658`
  - `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Grasshopper/UI/Motion.cs:2169`
- Evidence: sampled source comments mostly explain invariants, but a few use provenance/history phrasing such as probe-cache token provenance and "stale lie".
- Suggested disposition: leave invariant comments unless touching the file; phrase as current invariant rather than history if edited.

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
