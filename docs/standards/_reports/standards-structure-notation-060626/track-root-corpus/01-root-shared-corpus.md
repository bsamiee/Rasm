Question: Which root and shared documentation standards need structural, formatting, notation, prose, evidence, or representation improvements before promotion?
Type: gap-critique
Lane: track-root-corpus
Merge key: root-shared standards corpus :: structure notation evidence representation :: audit
Target owner: docs/standards/README.md; docs/standards/AGENTS.md; docs/standards/agents-md.md; docs/standards/proof.md; docs/standards/style-guide.md
Source basis: full local source read plus bounded renderer-source research where renderer capability claims are directly at issue
Promotion target: active owner files named per finding
Outcome: PROMOTE

## [FINDINGS]

[F1][RENDERER_PROOF_PACKET]
Path and line/heading: docs/standards/proof.md:154-163, docs/standards/information-structure.md:105, docs/standards/information-structure.md:438-440, docs/standards/information-structure.md:458-479, docs/standards/formatting.md:107-114.
Axis: evidence and notation.
Evidence snippet/source id: proof.md lists renderer claims that require proof, while information-structure.md and formatting.md assert GFM table limits, Mermaid `config:`, GitHub alerts, `<details>`, and footnotes. Source R1: GitHub Basic writing and formatting syntax, accessed 2026-06-06, documents task lists, footnotes, and five alert types at https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax. Source R2: GitHub collapsed sections, accessed 2026-06-06, documents `<details>` and `<summary>` at https://docs.github.com/github/writing-on-github/working-with-advanced-formatting/organizing-information-with-collapsed-sections. Source R3: GitHub Flavored Markdown Spec 0.29-gfm, accessed 2026-06-06, says GFM tables are rows and columns and do not contain block-level elements at https://github.github.com/gfm/#tables-extension-. Source R4: Mermaid configuration and layouts docs, accessed 2026-06-06, document diagram-frontmatter `config:` and `layout: elk` at https://mermaid.js.org/config/configuration.html and https://mermaid.js.org/config/layouts.html.
Weakness or inconsistency: the active corpus has the correct proof rule but no compact renderer-support proof packet, so the same renderer facts are asserted in form and notation pages without a nearby source, local render result, or explicit gap. This makes the rule self-inconsistent: proof.md demands renderer proof while the renderer grammar examples rely on unstated proof.
Proposed correction: add one renderer-support proof packet under proof.md or a short proof-owned subsection that lists each supported renderer claim, its maintained source, local repository support route, and stale trigger. Keep form rules in information-structure.md and surface grammar in formatting.md, but route their renderer capability proof back to proof.md instead of repeating source URLs in both files.
Active owner file: docs/standards/proof.md.
Ripple files: docs/standards/information-structure.md; docs/standards/formatting.md.
Decision recommendation: PROMOTE.
Proof gap or question: local `mmdc` render was not run during this report; GitHub-rendered behavior was checked through maintained GitHub docs only, not through a local rendered artifact.

[F2][PROVIDER_LOADING_PROOF]
Path and line/heading: docs/standards/agents-md.md:38-52 and :213-247.
Axis: evidence and position.
Evidence snippet/source id: agents-md.md lists Codex loading facts such as root-to-current-directory loading, `AGENTS.override.md` precedence, opt-in fallback filenames, empty-file skipping, and project-doc budget truncation. names OpenAI Codex, Claude Code, and Gemini authoring defaults and states that provider-specific rules are local defaults, not proof of current behavior.
Weakness or inconsistency: the files correctly warn that provider-loading facts need proof, but the current behavior list is presented without a proof field, source route, last-verified date, or explicit proof gap. The result is a high-value current-state claim embedded in a target standard.
Proposed correction: keep the authoring defaults, but attach a provider-surface proof record or mark the unverified items as `Proof gap:`. The record should separate local Codex loading behavior from Claude Code and Gemini authoring defaults so each claim can refresh independently.
Active owner file: docs/standards/agents-md.md for `AGENTS.md` load semantics; for provider authoring defaults; docs/standards/proof.md for field meanings.
Ripple files: root AGENTS.md; docs/standards/AGENTS.md.
Decision recommendation: HOLD.
Proof gap or question: no current provider documentation or local provider-output proof was collected for this report because the task limited current-source research to renderer or Markdown capability claims.

[F3][ACTIVE_CORPUS_BOUNDARY]
Path and line/heading: docs/standards/README.md:142-160 and docs/standards/AGENTS.md:5-11.
Axis: position and craft.
Evidence snippet/source id: README.md includes `AGENTS.md` in the folder layout, then says active standards are the files in the layout except `_reports/**` and deprecated folders, and finally says `AGENTS.md` is an instruction overlay, not a standards body. AGENTS.md says the active corpus is the current root, shared, and type standards listed by README.md and excludes `_reports/**`.
Weakness or inconsistency: the wording is technically recoverable but easy to misread during source-set selection. A future audit can count `AGENTS.md` as an active standards body because it is listed inside the active layout before the later exclusion clarifies its role.
Proposed correction: split the README folder layout into `Standards bodies` and `Instruction and report overlays`, or replace the sentence with `Active standards bodies are the standards files in this layout; AGENTS.md and _reports/** are instruction or source-material overlays, not standards bodies.` Keep AGENTS.md in read-order rules, but not in active-standards counts.
Active owner file: docs/standards/README.md.
Ripple files: docs/standards/AGENTS.md; docs/standards/_reports/AGENTS.md.
Decision recommendation: CORRECT.
Proof gap or question: none; this is a local wording and boundary issue proven by current repository files.

[F4][DOCS_GATE_SELECTOR]
Path and line/heading: docs/standards/proof.md:136-165, docs/standards/AGENTS.md:133-136, tools/assay/rails/docs.py:1-102, tools/assay/composition/catalog.py:279-280.
Axis: evidence and representation.
Evidence snippet/source id: proof.md lists docs-as-code gate classes including link checker, anchor validation, docs build, renderer proof, and configured formatter or linter. AGENTS.md requires unsupported renderer, link, anchor, or docs-build claims to be reported as proof gaps. Current repo source shows the configured docs rail validates Mermaid through `mmdc`; tools/assay/composition/catalog.py registers `mmdc`, and tools/assay/rails/docs.py fans routed Markdown files into `mmdc -i`.
Weakness or inconsistency: the proof ladder is useful, but it does not separate configured gates from desired-but-unconfigured gate classes. A maintainer can read the table as an available validation catalog even though the local docs rail currently proves only Mermaid rendering from the inspected source.
Proposed correction: add a small `Configured standards gates` record in proof.md or route it to the tool README: `Whitespace: git diff --check`, `Mermaid: tools/assay docs check or pnpm exec mmdc`, `Links or anchors: proof gap unless a local checker is named`, `Docs build: proof gap unless a build target is named`. Keep the abstract ladder, but put configured availability beside it.
Active owner file: docs/standards/proof.md.
Ripple files: docs/standards/AGENTS.md; tools/assay/README.md or tools/assay source if the tool route becomes documented.
Decision recommendation: PROMOTE.
Proof gap or question: no link or anchor checker was found during this bounded inspection; a wider tool inventory could confirm whether another route exists outside tools/assay.

[F5][STRICT_SCHEMA_CLAIM]
Path and line/heading: :120-123.
Axis: evidence and form.
Evidence snippet/source id: says strict JSON object schemas use `additionalProperties: false`, every property required, and explicit nullable values only where the provider or validator supports that shape, then says schema proves shape, not truth.
Weakness or inconsistency: the rule is directionally strong but packs provider mechanics, schema shape, tool-schema behavior, and semantic validation into one paragraph. The sentence can be misapplied as a universal JSON Schema requirement outside a strict provider or validator surface.
Proposed correction: convert the paragraph into a short decision table or field packet that separates `Strict JSON-schema surface`, `Typed tool input`, `Prompt-only JSON`, and `Human-reviewed field list`. Each row should name shape proof, semantic proof, and proof gap behavior.
Active owner file: .
Ripple files: docs/standards/proof.md; docs/standards/information-structure.md.
Decision recommendation: HOLD.
Proof gap or question: no current provider schema documentation was collected for this report; promote only after provider-specific mechanics are source-backed or the wording is reduced to local authoring convention.

[F6][REPORT_SESSION_MANIFEST_GAP]
Path and line/heading: docs/standards/_reports/AGENTS.md:11-17, docs/standards/_reports/standards-structure-notation-060626/README.md.
Axis: evidence.
Evidence snippet/source id: _reports/AGENTS.md says a named `_reports/**` task reads the session manifest first and treats a missing manifest as an archive-maintenance gap. The session folder `docs/standards/_reports/standards-structure-notation-060626/` exists, but `README.md` is absent.
Weakness or inconsistency: the requested report can be written, but the session lacks the manifest that should route tracks and later passes. That increases retread risk across the existing track folders.
Proposed correction: create the session README in a separate archive-maintenance change, using _reports/AGENTS.md section [4][SESSION_MANIFEST]. Do not create it in this pass because the user required exactly one report file.
Active owner file: docs/standards/_reports/AGENTS.md.
Ripple files: docs/standards/_reports/standards-structure-notation-060626/README.md.
Decision recommendation: HOLD.
Proof gap or question: this report intentionally does not create the manifest to preserve the exact one-file write contract.

## [SYSTEM_CATALOG]

[CODE_FENCES_AND_INTENT_LABELS]:
- Ordinary fences: language tag plus exactly one intent label, declared before examples use it.
- Allowed language-intent pairs seen: `bash copy-safe`, `markdown template`, `markdown conceptual`, `markdown rejected`, `text conceptual`, `text template`, `text rejected`, `diff output-only`, and `text output-only`.
- Renderer-local fences: exact renderer tag, especially `mermaid`; conceptual, template, generated, or rejected intent moves to the lead-in sentence or caption.
- Reuse classes: `copy-safe`, `template`, `conceptual`, `test-only`, `generated`, `output-only`, `deprecated`, and `rejected`.

[STATUS_LIFECYCLE_VOCABULARIES]:
- Default structured-record statuses: `QUEUED`, `ACTIVE`, `BLOCKED`, `DEFERRED`, `COMPLETE`, `DROPPED`, and `CANCELED`.
- Report manifest status values: `OPEN`, `PROMOTED`, `PARTIAL`, `SUPERSEDED`, `DROPPED`, and `RETRACTED`.
- Report lane outcomes: `PROMOTE`, `CORRECT`, `MERGE`, `DROP`, and `HOLD`.
- Inline result markers: `[PASS]`, `[FAIL]`, `[SKIP]`, `[PARTIAL]`, and `[N/A]`.
- Inline change markers: `[ADDED]`, `[REMOVED]`, `[CHANGED]`, and `[UNCHANGED]`.
- Inline lifecycle markers mirror the default structured-record statuses in brackets.
- Explicit state markers: `[OK]`, `[ERROR]`, `[WARNING]`, `[CAUTION]`, `[PENDING]`, `[UNKNOWN]`, `[NEW]`, `[DELETED]`, `[SAME]`, `[NULL]`, `[APPROX]`, `[CACHED]`, and `[SAVED]`.
- Absence ladder: `—`, `n/a`, `[N/A]`, `[SKIP]`, `[UNKNOWN]`, and `[NULL]`.

[BRACKETED_TOKENS_GLYPHS_ALERTS_PROGRESS]:
- Heading idiom: `# [DOCUMENT_TITLE]`, `## [N][SECTION_LABEL]`, and `### [N.M][SUBSECTION_LABEL]`.
- Group labels: `[X_Y_Z]:` for standalone set labels before lists or tables.
- Table rubrics: bracketed uppercase headers, with enumerable tables starting on `[INDEX]`.
- Compact glyphs: `[o]`, `[x]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`, and `[$]`.
- Invocation markers: `[IMPORTANT]`, `[CRITICAL]`, `[ALWAYS]`, and `[NEVER]`, reserved for instruction surfaces.
- GitHub alerts: `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, and `> [!CAUTION]`.
- Progress bars: exactly 20 cells inside brackets plus an integer percentage; numerator, denominator, closure rule, and proof surface belong beside the marker.
- Hidden comments: `<!-- source-only: <short reason> -->`, never the only carrier of proof, safety, intent, or required constraints.

[CO_LOCATION_RULES_AND_DEFECTS]:
- Evidence sits beside the claim, row, caption, record, procedure, or generated artifact it proves.
- Proof fields stay contiguous in proof.md order: `Evidence:`, `Generated from:`, `Source of truth:`, `Proof gap:`, `Last verified:`, and `Review trigger:`.
- Examples sit beside the rule they clarify; galleries and detached examples are rejected.
- Adjacent-document relation records sit beside the consuming section and are deleted when the adjacent fact no longer changes action, proof, or maintenance.
- Details blocks may hide low-salience support, but not proof, warnings, safety constraints, or first-read procedures.
- Comments are source-only hints, not reader-facing containers.

[DIAGRAMS_TABLES_MATRICES_RECORDS_FIELD_PACKETS_CHECKLISTS]:
- Narrative containers: prose, bullets, numbered lists, and checklists.
- Record and lookup containers: definition blocks, grouped definition blocks, status-tagged records, per-item H3 record blocks, comparison tables, lookup tables, decision tables, matrices, support matrices, and dependency matrices.
- Literal and visual containers: code blocks, monospace file trees, small stacks, small matrices, Mermaid diagrams, C4 or topology packets, callouts, collapsible blocks, and footnotes.
- Record packets: proof records, command cards, CLI envelope records, API field cards, adjacent-route records, how-to command step records, and runbook packets.
- Checklist forms: verification checklist, acceptance checklist with `Exit`, and status checklist with `Status`, optional `Depends`, and proof fields.
- Mermaid diagram jobs: flow, state, sequence, class/entity relation, topology, branch/rejoin shape, dependency shape, and lifecycle transition shape.
- Monospace representation jobs: codemap tree, type-shape block, edge list, small matrix, dependency matrix, lookup or support matrix, gate or risk map, progress summary, and tiny source-readable flow.

[ISOLATED_SENTENCES_PARAGRAPHS_CAPTIONS_EXAMPLES_ROUTE_RECORDS_GENERATED_BLOCKS]:
- Isolated sentences are allowed only as a lead, transition, caption, closing consequence, route boundary, or explicit proof gap.
- Paragraph pairs carry setup then consequence, exception, proof route, or route-away.
- Captions and text equivalents state the relation, state, or proof a visual encodes, not visual appearance alone.
- Compact contrast records use `Accepted:`, `Rejected:`, `Before:`, `After:`, `Near miss:`, and `Reason:` when fences are unnecessary.
- Route records use `Changed fact`, `Consumed by`, `Use in this document`, `Update when`, `Close when`, and `Route-away`.
- Generated mirror packets use `Canonical source`, `Generated from`, `Hierarchy`, `Omissions`, `Exclusions`, and `Edit rule`.
- Machine-consumed Markdown records declare `Consumer`, `Required shape`, `Optional fields`, `Validation command`, and `Exception`.

## [EVIDENCE]

Source set read line by line:
- CLAUDE.md.
- AGENTS.md.
- docs/standards/README.md.
- docs/standards/AGENTS.md.
- docs/standards/_reports/AGENTS.md.
- .
- docs/standards/agents-md.md.
- docs/standards/proof.md.
- docs/standards/style-guide.md.
- docs/standards/information-structure.md.
- docs/standards/formatting.md.

Existing report state:
- Requested report path did not exist before this pass.
- Session folder exists without README.md; recorded as [F6] instead of creating a second file.

Renderer and Markdown sources used:
- GitHub Basic writing and formatting syntax, accessed 2026-06-06: footnotes and alerts.
- GitHub Organizing information with collapsed sections, accessed 2026-06-06: `<details>` and `<summary>`.
- GitHub Flavored Markdown Spec 0.29-gfm, accessed 2026-06-06: table model and block-level table limits.
- Mermaid Configuration and Layouts docs, accessed 2026-06-06: diagram frontmatter `config:` and `layout: elk`.

Local repository proof used:
- package.json:26 declares `@mermaid-js/mermaid-cli`.
- pnpm-workspace.yaml:30 pins `@mermaid-js/mermaid-cli` to 11.14.0.
- tools/assay/composition/catalog.py:279-280 registers `mmdc`.
- tools/assay/rails/docs.py:1-102 validates Mermaid diagrams by routing Markdown files into `mmdc`.

## [RECOMMENDATIONS]

Promote first:
- [F1] Add proof-owned renderer-support packet.
- [F4] Separate configured docs gates from desired proof classes.
- [F3] Tighten README active-corpus wording so instruction overlays cannot be counted as standards bodies.

Hold until sourced:
- [F2] Provider-loading and provider-default proof.
- [F5] Strict schema mechanics by provider or validator surface.
- [F6] Session manifest creation, because this pass had an exact one-report-file write contract.

## [PROOF_GAPS]

- No local renderer command ran in this pass.
- No link or anchor validation command was run in this pass.
- No current provider documentation was researched for Codex, Claude Code, or Gemini behavior.
- No active standards file was edited.
