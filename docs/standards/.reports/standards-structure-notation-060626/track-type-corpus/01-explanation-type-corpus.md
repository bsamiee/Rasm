Question: Which structure and notation systems in `docs/standards/explanation/*.md` should be preserved, normalized, or tightened?
Type: standards-research
Lane: track-type-corpus
Merge key: explanation type standards :: structure and notation systems :: normalize owner-routed findings
Target owner: `docs/standards/explanation/*.md`
Source basis: full repo read of explanation standards plus `information-structure.md`, `formatting.md`, `proof.md`, `style-guide.md`; current official Mermaid, C4, Structurizr, and arc42 docs for renderer and external architecture-documentation claims
Promotion target: `docs/standards/information-structure.md`, `docs/standards/formatting.md`, `docs/standards/proof.md`, and selected explanation type standards
Outcome: PROMOTE

## [FINDINGS]

### [CATALOGUE]

ADR systems:
- Code fences and intent labels: exact `mermaid`; `markdown template`; `markdown conceptual`.
- Statuses: lowercase ADR `Status` values `accepted`, `rejected`, `deprecated`, `superseded`; compact lifecycle marker mapping only for indexes.
- Bracket tokens and glyphs: bracketed headings, table rubrics, `[INDEX]`, Mermaid `accTitle` and `accDescr`; no compact glyph alphabet.
- Progress: none.
- Records and matrices: decision-log table, decision-class table, lifecycle maintenance table, status-evidence receipt, option comparison matrix, design-handoff relation record.
- Diagrams: one ADR lifecycle `stateDiagram-v2` with text equivalent.
- Co-location and examples: accepted/rejected lead contrast sits beside required structure; option matrix contrast sits beside option-comparison rule.
- Relation and proof fields: design-handoff record uses local provenance fields before shared relation fields; status evidence uses `Evidence`, `Generated from`, `Source of truth`, `Last verified`, and `Review trigger`.
- Isolated sentences: valid text equivalent, consequence, and rejection-reason sentences; no unsupported sentence island found.

Architecture systems:
- Code fences and intent labels: `markdown template`, `text conceptual`, exact `mermaid`.
- Statuses: architecture path-state markers `[ACTIVE]`, `[BLOCKED]`, `[PROVISIONAL]`, `[DEPRECATED]` with optional source key in the same brackets.
- Bracket tokens and glyphs: bracketed headings, group labels, table rubrics, path-state tokens, box-drawing codemap glyphs.
- Progress: none; roadmap status appears only as code-reading overlay.
- Records and matrices: code-scope placement table, section-action table, scope and project identity definition blocks, contract/generated-truth receipt, path-state table, entrypoint table, dependency edge list or matrix, invariant record, status relation record, representation proof record.
- Diagrams: flowchart examples with Mermaid frontmatter `config`, ELK layout, `look: neo`, `theme: base`, accessibility title/description, and text equivalent.
- Co-location and examples: codemap proof block sits beside codemap; path-state table sits beside path-state markers; diagram rejection sits beside diagram rule.
- Relation and proof fields: relation fields append after local identity/routing fields; proof fields follow the proof-local run and add `Element match` and `Result`.
- Isolated sentences: accepted/rejected contrast and diagram captions are valid; one source-line formatting defect appears in the accepted Mermaid flow.

Design-document systems:
- Code fences and intent labels: exact `mermaid`; `markdown template`; tables for profiles, lifecycle decisions, alternatives, slices, proof gates, final checks, and handoffs.
- Statuses: design `Status` values `DRAFT`, `DISCUSSION`, `FINAL-CHECK`, `ACCEPTED`, `IMPLEMENTED`, `DROPPED`; risk/question statuses `OPEN`, `ASSIGNED`, `DEFERRED`, `RESOLVED`, `ACCEPTED-AS-RISK`, `DROPPED`; final-check states `satisfied`, `pending`, `blocked`, `n/a`; handoff lifecycle uses the shared uppercase status set.
- Bracket tokens and glyphs: bracketed headings and table rubrics; stable local IDs `S<N>`, `R<N>`, `Q<N>`.
- Progress: none.
- Records and matrices: profile table, lifecycle/profile decision table, measurable-goal checklist, alternatives table, slice table or record, cross-cutting concern record, risk/open-question relation record, proof-plan table and receipt, final-check table, design lifecycle diagram, handoff record and decision table.
- Diagrams: one lifecycle `stateDiagram-v2` with text equivalent.
- Co-location and examples: record field order is close to first record template; proof receipt is close to proof-plan rule.
- Relation and proof fields: risk/open-question records prepend local lifecycle fields, then shared relation fields; proof receipt uses the proof-local run.
- Isolated sentences: lifecycle text equivalent and implementation-status boundary sentence are valid explanatory closes.

Roadmap systems:
- Code fences and intent labels: `markdown template`, `markdown conceptual`, `text template`.
- Statuses: roadmap document state `ACTIVE`, `SNAPSHOT`, `PAUSED`, `CLOSING`, `ARCHIVED`; milestone status `QUEUED`, `ACTIVE`, `BLOCKED`, `DEFERRED`, `COMPLETE`, `DROPPED`, `CANCELED`; register kinds define their own status sets.
- Bracket tokens and glyphs: stable IDs `M<N>`, `M<N>.T<N>`, `E-*`, `B-*`, `H-*`, `R-*`; progress bar glyphs use `█` and `░`.
- Progress: 20-cell progress marker with numerator, denominator, closure rule, and proof map.
- Records and matrices: planning-truth source order, maintenance table, sequence-type table, current-status table, milestone record, dependency edge table, blocker record, documentation-handoff record, handoff trigger table, work-register record, terminal work records.
- Diagrams: optional Mermaid dependency graph only when three or more edges are clearer visually than a table; no inline diagram example.
- Co-location and examples: progress rule appears before progress examples; milestone record carries proof map beside progress.
- Relation and proof fields: handoff and work-register records use shared relation fields; proof map carries completion basis.
- Isolated sentences: `No roadmap` verdict is a compact route-away record; accepted/rejected contrast is valid.

Test-strategy systems:
- Code fences and intent labels: `markdown template`, `markdown conceptual`, `text template`, exact `mermaid`.
- Statuses: quarantine status values `suspected`, `quarantined`, `repairing`, `re-enabled`, `deleted`; gate trigger and blocking values; no global lifecycle `Status` unless a record declares one.
- Bracket tokens and glyphs: bracketed headings and table rubrics; no progress glyphs.
- Progress: none.
- Records and matrices: conditional-section decision table, profile table, archetype table, risk-tier decision table, test-level definition block, rail-class checklist table, gate record, adjacent proof record, gate latency Mermaid flow, entry/exit criteria record, proof-by-change table, flaky-test policy record.
- Diagrams: one conceptual gate-latency flowchart with text equivalent.
- Co-location and examples: rejected level/proof/policy examples sit beside the rule they clarify.
- Relation and proof fields: gate record includes proof fields in local order; adjacent proof record follows shared relation fields.
- Isolated sentences: rejected-example reasons and gate-order text equivalent are valid.

### [FILE_FINDINGS]

`docs/standards/explanation/adr.md`
- Active owner: `adr.md` for ADR-specific statuses and proof slots; `formatting.md` for compact marker rendering; `proof.md` for evidence fields.
- Finding: ADR lowercase `Status` values are intentionally type-local and correctly protected from roadmap lifecycle markers, but the status-evidence examples use `markdown template` for pure field packets at lines 232, 247, 254, 260, and 267. `information-structure.md` says `text template` is the better fence for source-neutral label/value packets.
- Ripple: changing this pattern affects ADR, architecture, design, roadmap, and test-strategy field-packet examples; do not change only ADR.
- Decision: CHANGE. Promote a cross-type fence-normalization pass: use `text template` for pure definition packets and keep `markdown template` only when Markdown structure, headings, links, tables, or task lists are material.
- Proof gap: no renderer or Markdown-lint gate was run for this report; the finding is from source review against active standards.

`docs/standards/explanation/architecture.md`
- Active owner: `architecture.md` for codemap, C4, and path-state rules; `information-structure.md` for diagrams and code-fence intent; `proof.md` for renderer and model-source claims.
- Finding: the accepted flowchart has one unindented edge at line 277 while all surrounding Mermaid edges are indented. Mermaid may parse it, but the source shape violates the file's own source-readability and alignment discipline.
- Ripple: local to the accepted architecture Mermaid example; no semantic standard change needed.
- Decision: CHANGE. Indent `Validator --> Worker["Execution/EventWorker.cs [ACTIVE M2]"]` with the surrounding edge lines in a later active-standard edit.
- Proof gap: no Mermaid render command ran; this is a notation/source-readability defect, not a rendered-output claim.

`docs/standards/explanation/architecture.md`
- Active owner: `architecture.md` for C4/arc42/Structurizr posture; `proof.md` for maintained-source evidence; `information-structure.md` for C4 diagram handoffs.
- Finding: external architecture claims are directionally current but need closer proof placement if promoted. Official sources support the core claims: Mermaid frontmatter `config` and ELK layout are documented in Mermaid configuration/layout docs; `look: neo` appears in Mermaid config type docs; `accTitle` and `accDescr` are documented in Mermaid accessibility docs; C4 official docs define Context, Container, Component, and Code as diagram levels and define a Container as an application or data store that must run; Structurizr docs define Structurizr as model/diagram-as-code for C4; arc42 describes categories such as context, building block, runtime, and risks/technical debt.
- Ripple: affects architecture `VIEW_DISCIPLINE`, `MERMAID_C4` in `information-structure.md`, and proof language around renderer/model claims.
- Decision: CHANGE. Add or tighten a proof note only where the active standard makes an external claim: C4 level semantics, Mermaid config/accessibility support, and Structurizr-as-model-source behavior. Keep the note short and route local render proof to `proof.md`.
- Proof gap: maintained docs were checked, but no local `mmdc`, GitHub render, or docs build gate ran; repository-specific Mermaid support remains unproved.

`docs/standards/explanation/design-doc.md`
- Active owner: `design-doc.md` for proposal lifecycle and risk records; `information-structure.md` for relation-record ordering.
- Finding: design has the most status-family density in the explanation corpus. It correctly declares separate vocabularies before use, but the handoff record at lines 361-373 uses the shared lifecycle vocabulary while risk/open-question records at lines 250-278 use design-specific statuses. The current text is correct, but future agents can easily confuse these field families because both use `Status:`.
- Ripple: roadmap also uses multiple status families; architecture uses bracketed path states; ADR uses lowercase lifecycle values.
- Decision: PROMOTE. Add a small cross-type status-family map in the report synthesis or in `information-structure.md` if active standards need a shared lookup. Do not collapse vocabularies; each family carries different lifecycle behavior.
- Proof gap: no active edit made; this is a navigation/normalization recommendation from source review.

`docs/standards/explanation/roadmap.md`
- Active owner: `roadmap.md` for progress and milestone records; `formatting.md` for progress bar rendering; `information-structure.md` for progress eligibility.
- Finding: roadmap is the only explanation standard with a progress bar. It correctly defines calculation basis before examples and keeps count/proof details in `Proof map`, not on the progress line.
- Ripple: do not generalize progress bars into ADR, architecture, design, or test-strategy without a visible numerator, denominator, closure rule, and proof surface.
- Decision: KEEP. Preserve roadmap as the canonical type-standard example of progress notation.
- Proof gap: no arithmetic/tool check was run; source review shows the local examples align with the shared rule.

`docs/standards/explanation/roadmap.md`
- Active owner: `roadmap.md` for dependency edge records and terminal work; `information-structure.md` for when table versus diagram carries an edge set.
- Finding: roadmap says Mermaid appears only when three or more edges are easier to scan than the table, while the table keeps source route, live source, and go/no-go fields. This is a strong representation split and should become the cross-type reference pattern for "diagram plus table" decisions.
- Ripple: architecture and test-strategy already use the same split but could cross-reference the rule rather than restating it.
- Decision: PROMOTE. Route the durable shared rule to `information-structure.md`: diagrams own topology or sequence shape; records/tables own source, status, proof, live source, update, and removal fields.
- Proof gap: no diagram render was run; finding is about representation ownership.

`docs/standards/explanation/test-strategy.md`
- Active owner: `test-strategy.md` for gate taxonomy and flaky-test policy; `proof.md` for proof-by-change evidence strength; `style-guide.md` for terminology.
- Finding: test strategy properly separates testing vocabulary from local executable truth, but many template proof cells use generic values by design. The document warns produced strategies must replace them with local gates; this is correct but depends on agents noticing both `LOCAL_TRUTH` and table notes.
- Ripple: contributing/how-to/runbook routes must not copy these generic proof cells as commands or gate names.
- Decision: KEEP with synthesis emphasis. In any promotion summary, call this a "template-only vocabulary" system so generic cells do not become active local proof.
- Proof gap: no repo gate inventory was run; this report did not validate current local test commands.

Cross-file relation records
- Active owner: `information-structure.md` for shared relation fields; each type standard for local field prefixes.
- Finding: relation records are present in all five explanation standards, but they have justified local prefixes: ADR uses `Origin design` and `Accepted direction`; architecture uses `Path or surface`, `Carries`, and `Does not own`; design and roadmap prepend lifecycle fields when the relation itself is tracked; test strategy uses gate-specific adjacent proof records. This is not drift, but it needs a single normalization statement to prevent false cleanup that removes local fields.
- Ripple: affects all explanation type standards and any future synthesis report.
- Decision: PROMOTE. Preserve local prefixes when they change reader action, then require the shared relation field run to remain in order once it begins.
- Proof gap: source review only.

Cross-file code-fence intent
- Active owner: `information-structure.md` for allowed language-intent pairs; `formatting.md` for fence spacing.
- Finding: the explanation corpus follows the renderer-local Mermaid rule, but field-packet examples are split between `markdown template` and `text template`. The allowed-pair table already distinguishes Markdown structure from source-neutral label/value packets; the type corpus has not applied that distinction consistently.
- Ripple: ADR status evidence, architecture proof packets, design proof receipts, roadmap constraint and blocker packets, and test-strategy gate packets should use the same fence decision rule.
- Decision: CHANGE. Normalize in a later active edit by shape, not by file: pure field packet -> `text template`; heading/table/checklist/link-bearing Markdown shape -> `markdown template`; Mermaid -> exact `mermaid` with intent in visible prose.
- Proof gap: no automated fence validator found or run.

Cross-file isolated sentences
- Active owner: `style-guide.md` for isolated-sentence validity; `information-structure.md` for sentence islands between containers.
- Finding: isolated sentences in the explanation corpus mostly serve allowed jobs: leads, text equivalents, rejected-example reasons, route boundaries, or closing consequences. No sentence island needs promotion from prose to field line in the mandatory target set.
- Ripple: do not run a blanket "remove one-line paragraphs" cleanup on these files.
- Decision: KEEP. Treat sentence-island cleanup as source-specific, not corpus-wide.
- Proof gap: source review only.

## [EVIDENCE]

Mandatory local reads:
- `CLAUDE.md`, root `AGENTS.md`, `docs/standards/README.md`, `docs/standards/AGENTS.md`, and `.reports/AGENTS.md`.
- Full target files: `docs/standards/explanation/adr.md`, `architecture.md`, `design-doc.md`, `roadmap.md`, and `test-strategy.md`.
- Shared owners: `docs/standards/information-structure.md`, `formatting.md`, `proof.md`, and `style-guide.md`.

Local owner evidence:
- `information-structure.md` controls containers, records, relation fields, code-block intent labels, progress eligibility, Mermaid/C4 use, and isolated sentence rules.
- `formatting.md` controls status/result markers, progress glyph grammar, table styling, whitespace, group labels, and heading idiom.
- `proof.md` controls proof fields, renderer claims, maintained-source evidence, proof gaps, and docs-as-code gate selection.
- `style-guide.md` controls isolated sentence validity, direct prose, terminology, punctuation, code-safe Markdown, links, and text equivalents.

Current-source research used only for renderer or external architecture-documentation claims:
- [Mermaid configuration](https://mermaid.js.org/config/configuration.html): frontmatter `config` is a supported per-diagram configuration source.
- [Mermaid layouts](https://mermaid.js.org/config/layouts.html): `layout: elk` is a supported layout value in YAML config or initialization options.
- [Mermaid config interface](https://mermaid.js.org/config/setup/mermaid/interfaces/MermaidConfig.html): `look` includes `neo`, `classic`, and `handDrawn`.
- [Mermaid accessibility options](https://mermaid.js.org/config/accessibility.html): `accTitle` and `accDescr` are documented accessibility hooks.
- [C4 diagrams](https://c4model.com/diagrams): C4 uses Context, Container, Component, and Code diagram levels and says not every level is required.
- [C4 container abstraction](https://c4model.com/abstractions/container): a C4 Container is an application or data store that must be running for the software system to work.
- [C4 component abstraction](https://c4model.com/abstractions/component): a Component is related functionality inside a Container behind a defined interface.
- [Structurizr DSL](https://docs.structurizr.com/dsl): Structurizr DSL defines architecture models based on C4 as text.
- [Structurizr home](https://docs.structurizr.com/): Structurizr presents itself as a models-as-code tool for C4.
- [arc42 overview](https://arc42.org/overview): arc42 provides architecture-documentation categories such as context, building block, runtime, and risks/technical debt.

## [RECOMMENDATIONS]

1. Promote a cross-type status-family map before editing individual statuses. The map should show ADR lowercase lifecycle, architecture path-state markers, design proposal/risk/final-check/handoff statuses, roadmap document and milestone statuses, and test-strategy gate/quarantine field values.
2. Normalize fence intent by block shape across all explanation standards. Use `text template` for pure field packets and `markdown template` for Markdown structure.
3. Fix the architecture Mermaid source indentation in the next active-standard edit.
4. Keep roadmap progress as the canonical progress example and do not copy progress bars into other type standards without calculation and proof basis.
5. Preserve local prefixes on relation records when the prefix changes reader action, but keep the shared relation field run ordered once it begins.
6. Add claim-level proof or proof-gap wording near external renderer/model claims only where the active standard depends on that behavior. Do not turn current-source links into a provider manual.

## [PROOF_GAPS]

- No active standards were edited.
- No local Mermaid render, `mmdc`, docs build, Markdown formatter, link checker, or anchor checker ran for this report.
- Current-source research used maintained official docs, but repository-specific renderer support remains unproved.
- The target session folder appears to lack a session `README.md`; `.reports/AGENTS.md` treats that as an archive-maintenance gap when no manifest exists.
