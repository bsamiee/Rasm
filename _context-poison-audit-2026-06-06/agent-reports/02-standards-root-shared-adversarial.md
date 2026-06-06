# Standards Root Shared Adversarial Audit

Question: Which scoped standards rules could poison future `AGENTS.md`, prompts, or docs by causing over-instruction, meta-commentary, report leakage, or unusual coupling?
Type: gap-critique
Lane: standards-root-shared-adversarial
Merge key: docs/standards root shared :: context-poison risk :: audit
Target owner: docs/standards root and shared standards
Source basis: full line-numbered reread of scoped files only
Promotion target: none yet
Outcome: HOLD

## [1][READ_SCOPE]

Files read fully:
- `docs/standards/README.md` lines 1-231
- `docs/standards/AGENTS.md` lines 1-135
- `docs/standards/agents-md.md` lines 1-266
- `docs/standards/agentic-documentation.md` lines 1-288
- `docs/standards/information-structure.md` lines 1-503
- `docs/standards/style-guide.md` lines 1-214
- `docs/standards/proof.md` lines 1-316
- `docs/standards/formatting.md` lines 1-284
- `docs/standards/_reports/AGENTS.md` lines 1-160

Excluded:
- Other `docs/standards/_reports/**` content.
- Standards type-family files outside the requested pressure zone.
- Edits to any standards file.

## [2][SUMMARY]

The scoped standards already contain strong anti-poison controls: `_reports/**` is excluded from the active corpus, reports and prompt assets are source material only, task/session/process narration is banned from durable standards, proof fields are claim-level rather than decorative, and provider behavior must be proved or marked as a gap.

The remaining risks are not simple omissions. They come from places where the standards themselves are so broad or procedural that future agents may over-apply them: a report-session trigger that can hijack ordinary audits, an `AGENTS.md` grammar that imports code-owner language into every overlay, provider-specific current-behavior claims without adjacent proof records, an agentic catch-all that can pull MCP/retrieval/prompt machinery into ordinary docs, and a `_reports/AGENTS.md` report shape that may make source-material reports preserve process structure the active standards otherwise reject.

## [3][FINDINGS]

### [3.1][BROAD_REPORT_TRIGGER]

Severity: high
Axis: position
Risk class: report leakage and over-instruction

Evidence:
- `docs/standards/agents-md.md:203` says that when a task asks for investigation, critique, audit, planning, or multi-pass agent output that may be reused later, the agent should create or update a report session under `_reports/<top-slug>-<ddmmyy>/`, verify the nearest non-root `AGENTS.md`, and read `_reports/AGENTS.md` before writing.
- `docs/standards/AGENTS.md:9-11` says `_reports/**` is excluded unless named and that reports, critique passes, memory notes, prompt assets, session history, external research, and deprecated material are source material only.
- `docs/standards/_reports/AGENTS.md:17` says active standards work that does not name `_reports/` must treat `_reports/` as excluded source material.
- `docs/standards/README.md:23` and `docs/standards/README.md:162` make `_reports/**` non-corpus source material rather than an active standards body.

Issue:
The report trigger in `agents-md.md:203` is broader than the exclusion rule. Nearly every serious audit, planning pass, or critique "may be reused later," so a future agent can read that line as permission or obligation to create a standards `_reports/` session even when the user supplied a different output path, asked for a one-off report, or did not name `_reports/**`.

Correction task:
Tighten the trigger so `_reports/` session creation happens only when the user explicitly requests a reusable `docs/standards/_reports/**` archive, the task target is already inside that folder, or a trusted standards owner directs durable promotion work there. Add a counter-rule that user-specified output paths and one-off audit artifacts do not become `_reports/` sessions by default.

Rule or standard to tighten:
`docs/standards/agents-md.md` trust-boundary or report-companion section, with a cross-check in `docs/standards/_reports/AGENTS.md` if needed.

Proof gap:
No current repository consumer was checked for `_reports/` session creation. This is a standards-level contradiction risk, not a runtime behavior claim.

### [3.2][CODE_OWNER_GRAMMAR_OVERREACH]

Severity: high
Axis: craft
Risk class: unusual coupling and over-instruction

Evidence:
- `docs/standards/agents-md.md:14-16` asks every overlay draft to identify newest targets, local owner rails, values, algebras, folds, tables, receipts, scenarios, boundaries, and report hazards.
- `docs/standards/agents-md.md:56-69` says every action-changing rule names trigger, existing owner rail, extension action, rejected substitute, and route-away owner; it rejects generic rules unless the same bullet names a replacement owner and action.
- `docs/standards/agents-md.md:137-146` provides owner-rail chooser examples dominated by code/test concepts such as operation algebra, generated union, smart enum, value object, Effect service/layer, msgspec model, SQLSTATE rail, source-owned scenario, and receipt folds.
- `docs/standards/agents-md.md:244` applies the same maintenance shape to produced overlays, requiring source event, owning route, update action, cleanup condition, and stop rule.

Issue:
The overlay standard correctly rejects vague policy, but it couples all `AGENTS.md` authoring to code-owner vocabulary. For non-code folders, prompt assets, docs folders, and source-material archives, this can poison future overlays with artificial "owner rail" language, code architecture metaphors, and inflated route/action records. That is the same metadata and over-instruction problem the standards elsewhere reject.

Correction task:
Split the local-extension grammar by folder kind. Keep code/test/tool overlays on owner rails and extension actions. For docs, prompts, reports, generated source material, or reference-only folders, allow a smaller grammar: trigger, source route, reader action, proof gap, cleanup condition, and stop rule. State that code owner-rail terms are examples for code-bearing folders, not required vocabulary for every overlay.

Rule or standard to tighten:
`docs/standards/agents-md.md` sections `[4][PRODUCED_SLOTS]`, `[7][LOCAL_EXTENSION]`, and `[14][MAINTENANCE]`.

Proof gap:
No produced overlay corpus was audited here to count actual overuse. The risk is inferred from the normative wording in the scoped standard.

### [3.3][UNPROVED_PROVIDER_FACTS]

Severity: high
Axis: evidence
Risk class: stale provider behavior in prompts and instructions

Evidence:
- `docs/standards/agentic-documentation.md:212-240` gives provider-specific defaults for OpenAI Codex, Claude Code, and Gemini.
- `docs/standards/agentic-documentation.md:230` states that `AGENTS.md` affects Claude Code only when `CLAUDE.md` imports or links it.
- `docs/standards/agentic-documentation.md:244-246` says provider-specific rules are local authoring defaults, not proof of provider behavior, and that capability, loading, delimiter, schema, state, or output-control claims are provisional until current primary documentation or local output proves them.
- `docs/standards/proof.md:14-16` says current external-provider behavior needs proof fields only when needed and never as decoration.
- `docs/standards/proof.md:132` requires current sources for changing tools, APIs, security guidance, support status, and provider behavior.
- `docs/standards/proof.md:145-146` says `latest`, `current`, `supported`, `deprecated`, `obsolete`, `legacy`, `experimental`, `beta`, `soon`, and `future` require source-specific definitions and freshness when present behavior is described.

Issue:
The provider section warns that provider claims need proof, but the same section carries concrete provider behavior claims without adjacent `Evidence:`, `Last verified:`, or `Proof gap:` fields. Future prompt authors can copy those lines as current provider truth, especially the Claude Code loading statement, even though the proof standard classifies provider behavior as drift-prone.

Correction task:
Either remove current provider behavior claims and keep only source-agnostic authoring defaults, or attach claim-level proof records to each provider behavior statement. Treat loading behavior, schema enforcement, state handling, delimiter defaults, and output controls as dated provider claims unless rewritten as local conventions.

Rule or standard to tighten:
`docs/standards/agentic-documentation.md` `[13][PROVIDER_BEHAVIOR]`, using `docs/standards/proof.md` proof fields.

Proof gap:
No external provider documentation was fetched in this audit. The finding is that the scoped corpus itself requires proof for claims it currently states without local proof records.

### [3.4][AGENTIC_CATCHALL_WIDTH]

Severity: medium
Axis: position
Risk class: prompt and tool machinery leaking into ordinary docs

Evidence:
- `docs/standards/agentic-documentation.md:7-14` applies the standard to placing controlling content, `llms.txt`, generated mirrors, machine-readable indexes, retrieval stores, MCP catalogs, structured-output contracts, and task/durable artifact separation.
- `docs/standards/agentic-documentation.md:18-25` routes unit ordering, artifact separation, task prompts, `AGENTS.md`, indexes, retrieval, MCP catalogs, generated mirrors, and provider behavior inside one standard.
- `docs/standards/agentic-documentation.md:168-192` defines MCP resources, prompts, and tools as record families with authorization and canonical-reference fields.
- `docs/standards/README.md:89-99` says each cross-cutting rule routes to exactly one owner and root standards audits use only position, form, craft, evidence, and notation.
- `docs/standards/AGENTS.md:87-89` routes prose, position, direct present-tense wording, exact source names, no prompt/session/process narration, and `AGENTS.md` semantic slots across `information-structure.md`, `style-guide.md`, `agentic-documentation.md`, and `agents-md.md`.

Issue:
`agentic-documentation.md` is doing several jobs: salience, artifact separation, provider posture, task output contracts, retrieval provenance, MCP catalogs, generated mirrors, and provider defaults. It repeatedly warns against copying active bodies and manuals, but its breadth makes it easy for future docs to pull MCP/tool/retrieval/prompt mechanics into ordinary standards, READMEs, or prompts merely because they are "agentic."

Correction task:
Add an eligibility gate near the lead: apply MCP, retrieval, generated mirror, and structured-output sections only when a real deployed or maintained surface exists or the task explicitly asks to design one. Otherwise route ordinary documentation to README/type standards, proof fields, or prompt assets without adding catalog machinery.

Rule or standard to tighten:
`docs/standards/agentic-documentation.md` lead and `[1][USE_WHEN]`.

Proof gap:
No downstream documents were audited for actual leakage. This is a pressure-zone risk from owner width and cross-routing.

### [3.5][REPORT_SHAPE_CAN_PRESERVE_PROCESS]

Severity: medium
Axis: form
Risk class: meta-commentary and report leakage

Evidence:
- `docs/standards/_reports/AGENTS.md:53-57` says every report must name lane, merge key, owner route, unique evidence, recommendation state, contradictions, and one lane outcome.
- `docs/standards/_reports/AGENTS.md:66-80` requires exactly one report type and rejects worker roles, waves, confidence levels, or implementation phases.
- `docs/standards/_reports/AGENTS.md:84-95` requires each report to start with a compact record containing question, type, lane, merge key, target owner, source basis, promotion target, and outcome.
- `docs/standards/_reports/AGENTS.md:105` bans top-level transcript, confidence, validation, no-change confirmations, draft edit maps, worker-role headings, and task-history sections.
- `docs/standards/_reports/AGENTS.md:157-160` says the folder must not become a second corpus, standards library, README index, command catalog, provider manual, validation ledger, transcript store, or instruction source.

Issue:
The report leaf rejects process leakage, but its own required report packet is process-heavy. For a single adversarial report, lane, merge key, promotion target, outcome, and report type can become metadata scaffolding rather than reader action. That scaffolding is safer inside reusable `_reports` archives than in user-directed one-off reports, but the standard does not name a single-report or user-specified-output exception.

Correction task:
Add a narrow exception: if the user supplies a one-off report path outside `docs/standards/_reports/**`, do not import `_reports/AGENTS.md` packet mechanics. If the report is inside `_reports/**`, allow the compact record but require each field to change later promotion, correction, or pruning action; omit fields that do not.

Rule or standard to tighten:
`docs/standards/_reports/AGENTS.md` `[7][REPORT_SHAPE]` and `docs/standards/agents-md.md` report trigger.

Proof gap:
This audit did not read any existing `_reports` report bodies, by instruction. The finding is about required report shape only.

### [3.6][EVALUATION_RIGOR_CAN_OVERFIT]

Severity: low
Axis: evidence
Risk class: over-instruction

Evidence:
- `docs/standards/proof.md:215-218` defines rigor fields for agent-surface evaluation: 20-50 representative questions, 3-5 trials per case for stochastic output, Wilson score 95% confidence interval, paired baseline comparison, and traces.
- `docs/standards/proof.md:233` narrows those fields to stochastic output, ranking, tool selection, latency, or provider behavior.
- `docs/standards/proof.md:246` says `Proof gap:` applies when a contract is reviewed by a human rather than enforced by tooling.

Issue:
The narrowing sentence at `proof.md:233` helps, but the field values are still exact enough to become cargo-cult benchmark requirements for lightweight prompt, retrieval, or provider notes. That would poison future docs with expensive evaluation scaffolding even when a claim only needs source provenance, semantic validation, or a human proof gap.

Correction task:
Clarify that the 20-50 question set, 3-5 trials, and Wilson interval are required only when the document claims measured retrieval/ranking/tool-selection quality or provider-output performance, not when it merely documents a machine-facing surface or local authoring default.

Rule or standard to tighten:
`docs/standards/proof.md` `[10][AGENT_SURFACE_EVALUATION]`.

Proof gap:
No evaluation receipts were audited for actual overuse. This is a preventive tightening.

## [4][LOW_RISK_CONTROLS]

These rules actively reduce context poisoning and should be preserved:
- `docs/standards/README.md:23` and `docs/standards/README.md:162` keep `_reports/**` outside the active corpus.
- `docs/standards/AGENTS.md:11` blocks report transcripts, roles, confidence, task framing, wave counts, and report structure from active standards.
- `docs/standards/AGENTS.md:93-94` forbids people/process metadata fields unless a local tool consumes them.
- `docs/standards/AGENTS.md:99-105` rejects live task instructions, chat excerpts, prompt-source narration, fixed sub-agent counts, unsupported provider claims, empty headings, decorative diagrams, metadata whitelists, and hand-maintained generated catalogs.
- `docs/standards/agentic-documentation.md:87-102` separates durable docs, prompt assets, task instructions, and state artifacts.
- `docs/standards/agentic-documentation.md:242` rejects exposed chain-of-thought in published prompt standards.
- `docs/standards/information-structure.md:56-60` limits packet promotion and forbids filling absent fields.
- `docs/standards/style-guide.md:43-47` removes filler, draft notes, transient interaction language, and prompt-era process vocabulary.
- `docs/standards/proof.md:16` forbids decorative evidence fields.
- `docs/standards/proof.md:259` rejects long transcripts as evidence.
- `docs/standards/formatting.md:130-132` keeps invocation markers out of ordinary documentation.
- `docs/standards/_reports/AGENTS.md:141-143` says to promote rules, not report frames.

## [5][GAPS]

- I did not read auditor A's report or any other audit output, so this report is independent but cannot compare agreement or divergence.
- I did not read any existing `docs/standards/_reports/**` report bodies, per scope.
- I did not inspect produced `AGENTS.md` files outside the scoped standards zone, so findings about downstream poisoning are pressure-zone risks, not counted corpus defects.
- I did not run link, anchor, renderer, or docs-build validation; this was a read-only standards audit plus the requested report write.

## [6][RECOMMENDATION]

Priority corrections:
1. Narrow `agents-md.md:203` so ordinary audits do not default into `_reports/` session creation.
2. Split `agents-md.md` local-extension grammar into code-bearing and non-code overlay forms.
3. Add proof records or remove current provider behavior claims in `agentic-documentation.md` provider sections.
4. Add an eligibility gate to `agentic-documentation.md` so MCP/retrieval/generated-mirror machinery applies only to real maintained surfaces.
5. Add a single-report or user-specified-output exception to `_reports/AGENTS.md` and the report trigger path.

Net assessment: the corpus is not broadly poisoned. The highest-risk rules are narrow but important because they live in standards that future agents will treat as authoring templates. Tightening those triggers would preserve the anti-leak posture without weakening the standards.
