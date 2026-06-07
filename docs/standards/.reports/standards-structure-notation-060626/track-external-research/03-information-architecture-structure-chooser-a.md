Question: Which current or stable information-architecture and technical-writing sources should shape `information-structure.md` into a practical "I have this information; what structure should carry it?" chooser?
Type: standards-research
Lane: track-external-research
Merge key: docs/standards/information-structure.md :: input-to-structure chooser :: promote
Target owner: `docs/standards/information-structure.md`
Source basis: active standards and report-session reads; Diataxis, DITA 2.0, Microsoft Writing Style Guide, Google developer documentation style guide, GitHub Flavored Markdown Spec, and Nielsen Norman Group sources accessed 2026-06-06
Promotion target: `docs/standards/information-structure.md`, with proof-owned source packets in `docs/standards/proof.md` and notation-only ripples in `docs/standards/formatting.md`
Outcome: PROMOTE

Prior lane extension: extends `track-external-research/01-gfm-github-markdown-capabilities.md` by holding renderer syntax as prior proof work and adding information-architecture, information-typing, structure-selection, and prose-to-carrier source evidence for the same standards session.

## [FINDINGS]

Finding 1
    Active owner/section: `docs/standards/information-structure.md` `[2][CONTAINER_CHOOSER]`.
    Evidence source URLs: https://dita-lang.org/dita/archspec/base/information-typing; https://diataxis.fr/_/downloads/en/latest/pdf/.
    Finding: Information typing is the strongest external justification for an input-first chooser. DITA frames information typing as matching different reader questions to different structures, including retrieval-oriented structures such as tables for reference and step sequences for tasks. Diataxis makes the same split at document-type level: reference describes, how-to guides action, tutorials teach, and explanation builds understanding.
    Weakness/inconsistency: The active chooser starts with reader questions, but the collective task list correctly notes that authors often arrive with raw material first: command output, a field set, a condition grid, a proof-bearing claim, a type relation, a machine contract, or a warning. The standard needs a small input signature layer before the existing reader-question table so agents can convert raw material without first writing prose.
    Proposed correction: Add an `INFORMATION_SIGNATURE_CHOOSER` before or at the top of `[2][CONTAINER_CHOOSER]`. The rows should map raw input signatures to carriers: one concept to prose; peer facts to bullets; one scannable item to definition block; several homogeneous comparable items to table; independent lifecycle items to records; condition combinations to decision table; ordered action checkpoints to ordered records; literal source to code fence; topology to codemap, edge list, or Mermaid; command output to command/output packet; proof-bearing claim to proof record.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/README.md`; type standards only where they currently refer to container choice in a way that conflicts with the new signature layer.
    Decision: PROMOTE.
    Proof gap/question: No user choice is required for the signature layer; exact row count and row labels should be reviewed against the active corpus before promotion.

Finding 2
    Active owner/section: `docs/standards/information-structure.md` `[6][STRUCTURED_RECORDS]`.
    Evidence source URLs: https://dita-lang.org/dita/archspec/base/information-typing; https://dita-lang.org/dita/archspec/base/topicstructure.
    Finding: Structured authoring supports small, self-contained units with permitted body structures and information-type-specific content. DITA's topic model separates topic identity, short description, body content, links, and maps, and it warns that nesting can reduce usability and reuse when it creates complex dependent documents.
    Weakness/inconsistency: The active standards already use many local status vocabularies, but `information-structure.md` only gives the shared lifecycle default and says type standards may define local sets. That leaves authors without a reusable way to declare a closed vocabulary before first use.
    Proposed correction: Promote a shared `VOCABULARY_CARD` shape before structured records. Required fields should include `Vocabulary`, `Applies to`, `Values`, `Active states`, `Blocked states`, `Returnable states`, `Terminal states`, `Omitted shared states`, `Projection rule`, `Removal behavior`, and `Display or machine token`. Type standards keep their local vocabulary bodies; the shared card only standardizes declaration shape.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/explanation/adr.md`; `docs/standards/explanation/architecture.md`; `docs/standards/explanation/design-doc.md`; `docs/standards/explanation/roadmap.md`; `docs/standards/reference/support-matrix.md`; `docs/standards/learning/onboarding.md`; `docs/standards/learning/tutorial.md`.
    Decision: PROMOTE.
    Proof gap/question: User choice remains open for whether lowercase or display-value status sets need extra handling beyond the shared card.

Finding 3
    Active owner/section: `docs/standards/information-structure.md` near `[7][CHECKLISTS]`, `[8][LISTS]`, and `[9][DEFINITION_BLOCKS]`.
    Evidence source URLs: https://learn.microsoft.com/en-us/style-guide/procedures-instructions/writing-step-by-step-instructions; https://learn.microsoft.com/en-us/style-guide/scannable-content/lists.
    Finding: Microsoft separates list forms by action: numbered lists for sequential procedures or priorities, bullets for unordered sets, and term lists for term-definition pairs. Its procedure guidance also puts location before action when needed, uses one step per instruction unless short same-place actions combine, and avoids unnecessary introductory text.
    Weakness/inconsistency: Rasm task and learning standards use numbered items with indented field lines, but `information-structure.md` does not name this as a first-class ordered record. Without the term, authors can treat step records as ordinary nested lists and lose field discipline.
    Proposed correction: Add `ORDERED_RECORDS` as a shared carrier. Define it as a numbered action or checkpoint whose continuation lines carry fields such as `Operation`, `Expected result`, `If wrong`, `Working state`, `Verify`, or type-local equivalents. State that ordered records are for dependency order, triage order, learner checkpoints, and procedure steps. A fenced command is allowed only when multiline copyability or syntax highlighting matters; otherwise the command stays in the `Operation` field.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/task/how-to.md`; `docs/standards/task/runbook.md`; `docs/standards/learning/tutorial.md`.
    Decision: PROMOTE.
    Proof gap/question: The open task-list question is whether ordered records may contain fenced blocks. External guidance supports keeping short commands in the step; Rasm can allow fenced companion blocks only when the form standard already permits the fence.

Finding 4
    Active owner/section: `docs/standards/information-structure.md` `[3][TABLES]`, `[4][TABLE_CONTENT_DISCIPLINE]`, and `[10][DECISION_LOOKUP_TABLES]`.
    Evidence source URLs: https://learn.microsoft.com/en-us/style-guide/scannable-content/tables; https://github.github.com/gfm/.
    Finding: Microsoft treats tables as useful for complex information with multiple attributes, direct key lookup, and categories with examples, but rejects tables for mere similar-item lists. It also stresses row identifiers in the left column, parallel entries, precise headers, limited columns, and brief cells. GFM gives the renderer constraint: Markdown tables are flat row-and-column structures.
    Weakness/inconsistency: The active standard has good row/column ceilings, but the report-session task list asks for more decomposition guidance around large lookup tables, gate matrices, row notes, and row-owned records. External guidance supports this split: tables carry comparison or lookup; records carry nested, long, proof-heavy, or independently updated row detail.
    Proposed correction: Expand the table chooser with a decomposition rule keyed by the first failing condition: many short rows become summary-plus-detail; long row detail becomes row-owned records; several independent condition columns become decision table with hit policy; one key maps to one value as lookup; two axes become matrix; proof-heavy gate rows become gate records. Keep GFM renderer limits in the proof packet rather than repeating renderer details here.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/reference/reference.md`; `docs/standards/reference/support-matrix.md`; `docs/standards/explanation/roadmap.md`.
    Decision: PROMOTE.
    Proof gap/question: User choice remains open for row-sidecar labels versus footnotes versus row-owned records. External table guidance favors row-owned records once qualifications stop fitting in cells.

Finding 5
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]` and command/output packet references under `[PROOF_RECORD_PACKETS]`.
    Evidence source URLs: https://developers.google.com/style/; https://learn.microsoft.com/en-us/style-guide/procedures-instructions/writing-step-by-step-instructions.
    Finding: Google makes project-specific style the first reference, then its own guide, then third-party references. Microsoft procedure guidance keeps instructions easy to follow and places action context where the reader needs it. Together, they support Rasm's local source-first command policy: a command carrier should not be generic prose copied from outside examples; it should name invocation, expected signal, side effect, and proof route from local truth.
    Weakness/inconsistency: `information-structure.md` names `Command card`, `CLI envelope record`, and `How-to command step record`, but it does not yet define the chooser between them. This leaves API, reference, README, how-to, and runbook standards to duplicate packet shapes.
    Proposed correction: Add a `COMMAND_OUTPUT_CHOOSER` that distinguishes six carriers: inline command field for one short operation inside a step; fenced command block for multiline or copy-safe syntax; command card for callable families; CLI envelope for stdout, stderr, exit status, artifacts, side effects, and failure reading; output record for observed signals; and command inventory table for short comparable command families. Route behavior proof to `proof.md`.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/reference/api.md`; `docs/standards/reference/reference.md`; `docs/standards/reference/readme.md`; `docs/standards/task/how-to.md`; `docs/standards/task/runbook.md`; tool READMEs only after active-standard wording lands.
    Decision: PROMOTE.
    Proof gap/question: A packet can define fields without proving any command behavior. Produced documents still need command source or run proof beside behavior claims.

Finding 6
    Active owner/section: `docs/standards/information-structure.md` `[12][MONOSPACE_TEXT]` and `[13][MERMAID_C4]`.
    Evidence source URLs: https://media.nngroup.com/media/articles/attachments/Heuristic_6_A4_compressed.pdf; https://media.nngroup.com/media/articles/attachments/Heuristic_Summary1_A4_compressed.pdf.
    Finding: NN/g's recognition-over-recall heuristic supports visible comparison, in-context help, and reducing information a reader must remember across a surface. Its minimalist-design heuristic supports deleting irrelevant or rarely needed information because every extra unit competes with relevant units.
    Weakness/inconsistency: The active standard names codemap trees, type-shape blocks, edge lists, Mermaid, and topology packets, but the upgrade rule is scattered: when should a raw text structure stay text, when should it become a table, and when should it become a rendered diagram?
    Proposed correction: Add a `TOPOLOGY_STRUCTURE_CHOOSER` with carrier jobs: codemap tree for path ownership and generated placement; type-shape block for records, unions, interfaces, and carriers; edge list for tiny dependency sets; matrix for comparable relationships; Mermaid for branching, sequence, lifecycle, or topology that would require recall if left in prose. Add the co-location test: diagrams own topology or sequence; records and tables own status, proof, updates, and removal triggers.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/explanation/architecture.md`; `docs/standards/formatting.md`.
    Decision: PROMOTE.
    Proof gap/question: Mermaid examples require renderer proof only when active edits claim rendering. This report makes no renderer claim beyond source-backed carrier logic.

Finding 7
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`.
    Evidence source URLs: https://media.nngroup.com/media/articles/attachments/Heuristic_6_A4_compressed.pdf; https://media.nngroup.com/media/reports/free/Site_Map_Usability_2nd_Edition.pdf.
    Finding: Recognition-over-recall supports keeping required information visible or easily retrievable. NN/g site-map guidance is a useful caution for progressive disclosure: when disclosure is needed, the mechanism must be intuitive and not hide information readers expect up front.
    Weakness/inconsistency: The active standard says `<details>` is for low-salience support and alerts interrupt the path, but it can make the progressive-disclosure test sharper. Authors need a quick demotion rule: required constraints, proof, warnings, first-read procedures, and decision inputs remain visible.
    Proposed correction: Add a `PROGRESSIVE_DISCLOSURE_TEST`: hide only material that is optional after the reader has the decision, proof, or action; keep load-bearing constraints, proof, warnings, first-run procedures, and branch conditions visible. If hidden material is needed to choose the next action, it is not low salience.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/proof.md`; `docs/standards/formatting.md`; `docs/standards/task/runbook.md`.
    Decision: PROMOTE.
    Proof gap/question: No user choice required. Exact GitHub `<details>` rendering proof belongs to the existing renderer-support packet from `01-gfm-github-markdown-capabilities.md`.

Finding 8
    Active owner/section: `docs/standards/information-structure.md` `[17][PAGE_ANATOMY]` and `[18][HEADINGS_CHUNKS]`.
    Evidence source URLs: https://dita-lang.org/dita/archspec/base/topicdefined; https://dita-lang.org/dita/archspec/base/topicstructure; https://developers.google.com/style/.
    Finding: DITA's topic model separates authoring units from delivery maps, and Google makes project-specific style the first authority. These support Rasm's existing owner routes: document anatomy should help standalone retrieval, but the active standard should not blur shared-standard anatomy, type-standard opening contracts, and produced-document skeletons into one undifferentiated template.
    Weakness/inconsistency: The collective task list is right that `[17][PAGE_ANATOMY]` mixes 3 jobs. That makes it harder for an agent to know whether it is shaping a standards file, a type standard, or a produced user document.
    Proposed correction: Split page anatomy into `STANDARD_FILE_ANATOMY`, `TYPE_STANDARD_OPENING`, and `PRODUCED_DOCUMENT_SKELETON`. Keep all existing templates, but put each under the job it controls. Add one sentence that a produced document uses the type standard skeleton first, then shared carrier rules inside sections.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: every active type standard only if they currently rely on ambiguous page-anatomy wording.
    Decision: PROMOTE.
    Proof gap/question: Review the exact type-standard opening contract after active-source critique before editing.

Finding 9
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; `docs/standards/proof.md` `[4][PROOF_FIELDS]`.
    Evidence source URLs: https://learn.microsoft.com/en-us/style-guide/scannable-content/tables; https://learn.microsoft.com/en-us/style-guide/scannable-content/lists; https://media.nngroup.com/media/articles/attachments/Heuristic_Summary1_A4_compressed.pdf.
    Finding: External sources converge on a practical rule: structure should reduce work, not display author effort. Lists improve scanning for same-rank items, tables help comparison and lookup, and minimalist design rejects irrelevant or rarely needed material that competes with the relevant units.
    Weakness/inconsistency: Rasm has strong anti-decoration rules, but the report-session task list correctly asks for representation co-location and alert-demotion tests. Authors need a rule that asks what unique job each representation carries before publishing both a table and a diagram, or an alert and a record.
    Proposed correction: Add a `REPRESENTATION_JOB_TEST`: before publishing a second carrier for the same fact set, state the job each carrier owns. Delete the second carrier if removing it loses no reader action. Demote repeatable boundaries, output contracts, failure responses, and proof from alerts into records or tables; keep alerts only for interrupting risk.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/style-guide.md`; `docs/standards/proof.md`; `docs/standards/reference/api.md`; `docs/standards/task/runbook.md`.
    Decision: PROMOTE.
    Proof gap/question: None for the shared rule. Renderer or accessibility proof applies only when a produced diagram, alert, or visual claim is changed.

Finding 10
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; `docs/standards/reference/code-documentation.md`.
    Evidence source URLs: https://dita-lang.org/dita/archspec/base/topicdefined; https://github.github.com/gfm/.
    Finding: Structured authoring distinguishes ordinary human-readable topic content from processing-aware content. GFM and HTML block behavior also show that Markdown can become parser-sensitive in ways ordinary prose rules do not safely normalize.
    Weakness/inconsistency: The active machine-consumed Markdown rule appears late, after many ordinary-document rules. The collective task list correctly says parser-owned exceptions must move earlier so agents do not prettify analyzer ledgers, generated ledgers, or parser-required Markdown into ordinary documentation.
    Proposed correction: Promote `MACHINE_CONSUMED_MARKDOWN` nearer the chooser. Required fields: `Consumer`, `Parsed shape`, `Required headings or fields`, `Optional fields`, `Validation command`, `Proof gap`, and `No-normalize rule`. State that ordinary container cleanup stops where a named parser consumes exact shape.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/reference/code-documentation.md`; parser-owned ledgers only after local source proof.
    Decision: PROMOTE.
    Proof gap/question: Confirm current analyzer release ledger shape before naming a Roslyn-specific example.

## [EVIDENCE]

[SOURCE_SET_READ]:
- `CLAUDE.md`.
- `AGENTS.md`.
- `docs/standards/README.md`.
- `docs/standards/AGENTS.md`.
- `.reports/AGENTS.md`.
- `.reports/standards-structure-notation-060626/README.md`.
- `docs/standards/information-structure.md`.
- `docs/standards/formatting.md`.
- `docs/standards/style-guide.md`.
- `docs/standards/proof.md`.
- Active explanation standards: `adr.md`, `architecture.md`, `design-doc.md`, `roadmap.md`, and `test-strategy.md`.
- Active reference standards: `api.md`, `code-documentation.md`, `readme.md`, `reference.md`, and `support-matrix.md`.
- Active task standards: `contributing.md`, `how-to.md`, and `runbook.md`.
- Active learning standards: `onboarding.md` and `tutorial.md`.
- `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`.
- Existing report `track-external-research/01-gfm-github-markdown-capabilities.md` to avoid retreading renderer findings.

[PRIMARY_SOURCE_SET_ACCESSED_2026_06_06]:
- Diataxis PDF, downloaded page: https://diataxis.fr/_/downloads/en/latest/pdf/.
- DITA 2.0, topic as basic unit: https://dita-lang.org/dita/archspec/base/topicdefined.
- DITA 2.0, information typing: https://dita-lang.org/dita/archspec/base/information-typing.
- DITA 2.0, topic structure: https://dita-lang.org/dita/archspec/base/topicstructure.
- Google developer documentation style guide, About this guide: https://developers.google.com/style/.
- Microsoft Writing Style Guide, Writing step-by-step instructions: https://learn.microsoft.com/en-us/style-guide/procedures-instructions/writing-step-by-step-instructions.
- Microsoft Writing Style Guide, Tables: https://learn.microsoft.com/en-us/style-guide/scannable-content/tables.
- Microsoft Writing Style Guide, Lists: https://learn.microsoft.com/en-us/style-guide/scannable-content/lists.
- GitHub Flavored Markdown Spec: https://github.github.com/gfm/.
- Nielsen Norman Group, Recognition rather than recall heuristic PDF: https://media.nngroup.com/media/articles/attachments/Heuristic_6_A4_compressed.pdf.
- Nielsen Norman Group, Ten usability heuristics summary PDF: https://media.nngroup.com/media/articles/attachments/Heuristic_Summary1_A4_compressed.pdf.
- Nielsen Norman Group, Site Map Usability report PDF: https://media.nngroup.com/media/reports/free/Site_Map_Usability_2nd_Edition.pdf.

## [RECOMMENDATIONS]

[PROMOTE_TO_INFORMATION_STRUCTURE]:
- Add `INFORMATION_SIGNATURE_CHOOSER` before the current reader-question chooser. It should answer "I have this raw material; what carrier should hold it?" before prose is drafted.
- Add `VOCABULARY_CARD` as a shared declaration shape, while leaving type-local vocabulary semantics in the type standards.
- Add `ORDERED_RECORDS` for numbered procedure, triage, and learner-checkpoint records with continuation field lines.
- Add `COMMAND_OUTPUT_CHOOSER` covering inline command fields, fenced command blocks, command cards, CLI envelopes, output records, and command inventory tables.
- Expand table decomposition with row-owned records, summary-plus-detail, gate-record promotion, and decision-table hit-policy triggers.
- Add `TOPOLOGY_STRUCTURE_CHOOSER` for codemap trees, type-shape blocks, edge lists, matrices, and Mermaid.
- Add `PROGRESSIVE_DISCLOSURE_TEST` so hidden details never carry required constraints, proof, warnings, first-read procedures, or branch conditions.
- Add `REPRESENTATION_JOB_TEST` so tables, records, diagrams, alerts, and prose each own a distinct reader job.
- Move or duplicate `MACHINE_CONSUMED_MARKDOWN` earlier as a first-class exception to ordinary cleanup.
- Split page anatomy into `STANDARD_FILE_ANATOMY`, `TYPE_STANDARD_OPENING`, and `PRODUCED_DOCUMENT_SKELETON`.

[ROUTE_TO_PROOF]:
- Put source freshness and renderer/source-class proof in `proof.md`, not in the form standard. This includes GFM table flatness, GitHub rendering behavior, and any future local validator claims.
- Keep command behavior proof out of packet definitions. Packet definitions can name fields; produced command claims require local source, generated output, or executed command evidence.

[ROUTE_TO_FORMATTING]:
- Keep marker spelling, table alignment, heading idiom, field-line spacing, and compact glyph rendering in `formatting.md`.
- Do not move status vocabulary semantics into formatting. Formatting may point to type-local declarations, but it is not the registry.

[ROUTE_TO_TYPE_STANDARDS]:
- Keep ADR, roadmap, design, support, onboarding, tutorial, and runbook vocabularies type-local when they determine document validity, publication state, readiness, proof closure, or response behavior.
- Update type standards only where they currently duplicate a packet that the shared form standard will own, or where they need a local exception after the shared rule lands.

## [USER_CHOICES]

User choice: compact glyph alphabet.
    Current task-list status: `BLOCKED_USER_CHOICE`.
    Recommendation: hold. This report does not choose a global glyph map. `information-structure.md` can still define when a glyph legend is a carrier; `formatting.md` and user choice decide whether glyph meanings are global or locally declared.

User choice: row-sidecar label pattern.
    Current task-list status: `BLOCKED_USER_CHOICE`.
    Recommendation: prefer row-owned records when a row needs proof, update, or removal fields; use footnotes only for short non-load-bearing provenance; hold durable `[GROUP] [INDEX]` sidecars until the user chooses.

User choice: `.claude` governance boundary.
    Current task-list status: `BLOCKED_USER_CHOICE`.
    Recommendation: no action in this report. The sources support project-specific style first, but they do not decide whether `.claude/**` is inside the active standards-governed ripple set.

User choice: lowercase/display-value status handling.
    Current task-list status: open under closed vocabulary cards.
    Recommendation: use the proposed `VOCABULARY_CARD` field `Display or machine token` so lowercase ADR statuses and title-case support statuses can stay valid local vocabularies without becoming shared lifecycle tokens.

## [CANDIDATE_WORDING]

Candidate input-first lead for `information-structure.md`:

```markdown template
[INFORMATION_SIGNATURES]:
- One concept, consequence, caveat, or transition -> prose.
- Peer facts or unordered requirements -> bullets.
- One scannable item with independent fields -> definition block.
- Several comparable items with shared short attributes -> table.
- Trackable items with status, proof, dependencies, or update triggers -> structured records.
- Independent conditions that choose one action -> decision table with hit policy.
- Ordered action checkpoints -> ordered records.
- Literal source, command, config, or output whose line breaks matter -> code fence.
- Path, type, dependency, lifecycle, or flow shape -> codemap tree, type-shape block, edge list, matrix, or Mermaid.
- Drift-prone or proof-bearing claim -> proof record beside the claim.
```

Candidate vocabulary-card template:

```text template
Vocabulary: <name>
Applies to: <record family, type standard, or local surface>
Values: <closed values with exact casing>
Active states: <values that remain in current use>
Blocked states: <values that cannot advance without a named gap; omit when absent>
Returnable states: <values with a return trigger; omit when absent>
Terminal states: <values that close or retire the record>
Omitted shared states: <shared lifecycle values intentionally not used>
Projection rule: <how values map to shared lifecycle or markers; omit when no projection exists>
Removal behavior: <when records stay, route away, or delete>
Display or machine token: <display label | machine token | source literal>
```

Candidate ordered-record rule:

```markdown template
1. <Imperative checkpoint title.>
    Operation: <command, edit, query, UI action, or judgment input>
    Expected result: <observable signal needed before the next step>
    If wrong: <local recovery or route-away; omit when absent>
```

## [PROOF_GAPS]

- This report uses maintained or stable external sources accessed 2026-06-06. It does not claim local renderer output, local command behavior, or configured validator behavior.
- No active standards were edited. Active-corpus validation is intentionally out of scope.
- The requested report path still needs `git diff --check` after file creation.
