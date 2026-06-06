# [ROOT_STANDARDS_STRUCTURE_AUDIT_2026_06_05]

This report captures the read-only root-file audit requested on June 5, 2026. It excludes `docs/standards/agents-md.md` from target coverage. It covers `AGENTS.md`, `README.md`, `agentic-documentation.md`, `formatting.md`, `information-structure.md`, `proof.md`, and `style-guide.md`.

## [1][AGENT_COVERAGE]

| [FILE] | [AGENTS] |
| :----- | :------- |
| `information-structure.md` | Hume, Turing, Faraday, Einstein |
| `formatting.md` | Averroes, Hubble, Galileo, Kepler |
| `style-guide.md` | Goodall, Laplace, Maxwell, Schrodinger |
| `proof.md` | Socrates, Avicenna, Carson, Chandrasekhar |
| `agentic-documentation.md` | Pasteur, Aquinas, Arendt, Hypatia |
| `README.md` | Huygens, Poincare, Sagan, Halley |
| `AGENTS.md` | Dewey, Cicero, Mill, Mendel |

All agents reported read-only findings and no file edits.

## [2][COMBINED_TASK_LIST]

### [2.1][INFORMATION_STRUCTURE]

- [ ] Replace the oversized `[2][CONTAINER_CHOOSER]` table with narrower decision records or split chooser tables. A chooser row must resolve to one container or to the next chooser.
- [ ] Add rendered-width and prose-density limits to table eligibility, not only row and column counts.
- [ ] Split table-form guidance into generic forms and domain profiles; profile rows must name real row and column axes.
- [ ] Teach row-owned follow-on detail blocks beside the table rule and add accepted/rejected examples.
- [ ] Declare reusable record schemas with field order, required fields, conditional fields, and omission behavior before examples.
- [ ] Clarify checklist inline fields: fields stay inline only while the item remains a checklist; excess fields promote to records.
- [ ] Add a scope sentence to H2 sections that currently open directly with bullets when not machine-consumed.
- [ ] Move decision-table hit policy and wildcard semantics before decision-table examples.
- [ ] Mark illustrative rendered tables as conceptual examples before the table.
- [ ] Declare the complete code-fence language-intent vocabulary before examples, including current corpus uses such as `markdown conceptual`, `text template`, and `text rejected`, or change examples to declared values.
- [ ] Split the flat V1 representation catalog into grouped sets. Remove or define `V1`.
- [ ] Convert review-only representation prose into a closed structured list or lookup table.
- [ ] Move representation examples beside their selection rule; do not keep a gallery that hides container choice.
- [ ] Split Mermaid guidance into records for renderer config, type chooser, size limits, IDs, edge labels, and representation split.
- [ ] Convert alerts, `<details>`, and footnote guidance into one record per container with `Use`, `Renderer`, `Visible constraint`, `Spacing`, and `Reject`.
- [ ] Encode page-anatomy templates directly with required opening order. Do not rely on later prose to carry required slots.
- [ ] Add machine-consumed Markdown validation: declared consumer, exact shape, optional or omitted fields, and validation command or proof gap.
- [ ] Add validation checks for prose-heavy chooser tables, ungrouped representation catalogs, hit-policy placement, intent-label completeness, gallery examples, and page-anatomy mismatch.

Agent sources: Hume, Turing, Faraday, Einstein.

### [2.2][FORMATTING]

- [ ] Replace token-family prose with a lookup table that distinguishes invocation markers, group labels, GitHub alerts, compact glyphs, lifecycle tokens, result tokens, booleans, and table absence values.
- [ ] Co-locate absence and not-applicable notation: `—`, `n/a`, `[N/A]`, `[SKIP]`, `[UNKNOWN]`, and `[NULL]`.
- [ ] Add a marker-surface chooser for bracket-like notations before family-specific prose.
- [ ] Refactor progress-bar guidance into a basis record, edge-case table, Unicode example, ASCII fallback example, and rejected appended-metadata examples.
- [ ] Keep progress bar fallback co-located with the primary example instead of burying it in prose.
- [ ] Split glyph policy into allowed jobs, declaration rules, rejected forms, and text-equivalent requirements.
- [ ] Route GitHub alert selection and semantic eligibility to `information-structure.md`; keep syntax and alert-not-invocation distinction in `formatting.md`.
- [ ] Convert table alignment guidance to a semantic alignment lookup.
- [ ] Convert table safety guidance to a cell-safety decision table.
- [ ] Replace dense whitespace bullets with a boundary matrix for headings, structural gaps, fenced blocks, lists, group labels, field lines, and contrast records.
- [ ] Add accepted/rejected examples for group labels, field lines, compact contrast records, and heading-label misuse.
- [ ] Rename sample table `[STATUS]` to `[RESULT]` or use real lifecycle values; do not use `[o]` as boolean or active state.
- [ ] Convert anchor and hidden-comment field-line paragraphs into rendered bullets or records.
- [ ] Fix prose numeric ranges such as `1 to 3` and `1 to 2` when they appear with hyphen punctuation.
- [ ] Add validation for text equivalents, fence intent consistency, marker decision ladders, root audit representation, and anchor proof gaps.

Agent sources: Averroes, Hubble, Galileo, Kepler.

### [2.3][STYLE_GUIDE]

- [ ] Remove semicolon-ended bullets in `[USE_WHEN]`; add validation for bullet terminal punctuation.
- [ ] Split `[WORDING_PRECEDENCE]` so conflict resolution, sentence composition fallback, and punctuation fallback do not share one paragraph.
- [ ] Protect canonical terms such as `route` from being treated as noise words unless the text means generic routing language.
- [ ] Recast the sentence-island rule so it allows imperatives and captions while rejecting labels disguised as sentences.
- [ ] Define FANBOYS before using “coordinating conjunction,” and add concrete criteria for comma omission in joined imperatives.
- [ ] Add examples for conjunctive adverbs such as `however`, `therefore`, and `otherwise`.
- [ ] Replace slash grammar examples with exact patterns such as `both ... and`.
- [ ] Clarify number rules for operational counts versus editorial prose counts.
- [ ] Add examples for open-compound modifiers and suspended hyphens.
- [ ] Fix hyphenated numeric ranges to en dashes or rewrite as prose.
- [ ] Narrow dash guidance to prose dash mechanics and route field rendering and table absence to `formatting.md`.
- [ ] Move bracketed label and field-line format ownership to `formatting.md`; keep `style-guide.md` focused on wording.
- [ ] Narrow link guidance to link text and canonical destination wording; route freshness and generated/mirrored targets to `proof.md` and `agentic-documentation.md`.
- [ ] Keep accessibility wording in `style-guide.md`, but route container eligibility, glyph alphabets, progress basis, and diagram placement to `information-structure.md` and `formatting.md`.
- [ ] Rename final proofing pass as craft proofing, or split by shared-standard owner.
- [ ] Add body guidance for root-audit prose if validation keeps that check.
- [ ] Add validation checks for colon capitalization, semicolon bullets, prose slashes, hyphenated ranges, number-word drift, and colon-lead/list adjacency.

Agent sources: Goodall, Laplace, Maxwell, Schrodinger.

### [2.4][PROOF]

- [ ] Collapse duplicated source-ranking facts between `[EVIDENCE_HIERARCHY]` and `[CONFLICT_HANDLING]`.
- [ ] Replace generic “metadata” language with exact proof fields or exact machine-consumed schema fields with named consumers.
- [ ] Convert proof-field vocabulary into a schema or lookup table with `Use when`, `Omit when`, and field order.
- [ ] Make `proof.md` the proof-field-order owner; have `information-structure.md` reference it without re-enumerating the order.
- [ ] Use exact field labels with colons in field-order prose.
- [ ] Label the first proof-field template line as `Claim:` or `Command:`.
- [ ] Teach single-claim proof, grouped proof records, table-row proof notes, subsection proof records, and page-level proof records.
- [ ] Add a visible `Proof gap:` field or an exact proof-gap encoding.
- [ ] Split evidence-placement, assertion-uncertainty, and preservation-refactor paragraphs into decision tables, state records, ordered steps, and failure checklists.
- [ ] Add a preservation receipt shape for replacement work.
- [ ] State docs-as-code gate hit policy: overlapping rows run all matching gates unless the document names a stricter configured gate.
- [ ] Make gate rows conditional on configured commands; state explicit gaps when no configured gate exists.
- [ ] Move renderer-dependent examples into row-owned notes or a renderer-claim list.
- [ ] Keep machine-facing contract taxonomy in `agentic-documentation.md`; keep proof receipt fields and evidence requirements in `proof.md`.
- [ ] Declare agent-surface evaluation receipt fields before examples, including required and conditional fields.
- [ ] Reconcile evaluation receipt order with proof-field order, or declare the evaluation receipt as a named exception.
- [ ] Merge or clearly mark partial rigor templates as additive fragments.
- [ ] Replace nonexistent concrete example paths with placeholders or proved existing paths.
- [ ] Remove or define the orphan `1–10 validity score` validation item.
- [ ] Remove semicolon-ended bullets, fix FANBOYS punctuation, replace slash shorthand such as `restore/build/analyzers green`, and normalize `[GATE]` cell grammar.

Agent sources: Socrates, Avicenna, Carson, Chandrasekhar.

### [2.5][AGENTIC_DOCUMENTATION]

- [ ] Add an early surface chooser for placement work, task prompts, artifact separation, retrieval, indexes, MCP catalogs, structured outputs, generated mirrors, and provider profiles.
- [ ] Convert scale records (`Document`, `Section`, `Paragraph and sentence`) into a table or records with shared fields.
- [ ] Merge repeated serial-position framing and render artifact-lifetime differences as comparison records.
- [ ] Render ranked constraints as an ordered list or rank table.
- [ ] Extract the positive-imperative rewrite example into a compact contrast record.
- [ ] Replace artifact-separation bullets with artifact-family records that expose lifetime, contents, authority, promotion route, and rejection.
- [ ] Split task instruction order from structured-output contracts, either with H3 subsections or separate H2 sections.
- [ ] Qualify schema-closure mechanics to surfaces that support them.
- [ ] Convert the four machine-facing checks into a lookup table: `Check`, `Proves`, `Examples`, `Does not prove`.
- [ ] Collapse `AGENTS.md` authoring to the local salience/provider delta and route structure elsewhere.
- [ ] Replace copied README type inventory in `llms.txt` guidance with a route to README-selected canonical documents.
- [ ] Render `llms.txt`, `llms-full.txt`, and mirrors as artifact records with source, generation, allowed use, and rejection.
- [ ] Convert retrieval provenance into an exact field packet with labels, order, optionality, and omission behavior.
- [ ] Create separate MCP record-family templates for `Resources`, `Prompts`, and `Tools`.
- [ ] Convert generated-mirror requirements into a field packet and route access-class policy consistently.
- [ ] Split provider behavior into portable contract, local defaults, provider profile schema, and proof-gated capability claims.
- [ ] Add proof fields or convert provider profiles to local authoring conventions for drift-prone provider claims.
- [ ] Split multi-fact provider fields such as `State and caching` and `Claude Code files`.
- [ ] Move source-map and reference-catalog retrieval guidance to retrieval provenance.
- [ ] Expand validation so every body rule is checked; split validation groups over 7 items and remove the single-item `[SAFETY]` group.
- [ ] Fix dense prose, vague terms, comma splices, misplaced modifiers, and abstract trust/safety language.

Agent sources: Pasteur, Aquinas, Arendt, Hypatia.

### [2.6][README]

- [ ] Split rule authority from claim evidence in `[SOURCE_PRECEDENCE]`.
- [ ] Condition `docs/usage.md` use to cross-stack owner precedence, implementation proof, host-library routing, or command/tooling claims outside this corpus.
- [ ] Replace type-standard-before-shared-standard ranking with owner resolution.
- [ ] Add standards-library edit read-order branch that routes through `docs/standards/AGENTS.md`.
- [ ] Replace undefined “shared route” with a non-type route lookup table.
- [ ] Convert reader-need mapping into a 2-axis decision table or matrix.
- [ ] Make route-away cells canonical labels or linked owner standards.
- [ ] Replace long shared-standard bullets with a lookup table using axis, owner, controls, and route-away.
- [ ] Move instruction-surface routing out of the five-axis shared-standards section.
- [ ] Convert placement rules into a condition-to-location table.
- [ ] Convert split/link bullets into a split matrix with content smell, remove-from, destination owner, and close condition.
- [ ] Co-locate lifecycle and maintenance decisions or make maintenance README-local only.
- [ ] Replace folder-layout prose inventory with a compact codemap or folder table.
- [ ] Convert mixed-module anti-pattern into a decomposition matrix.
- [ ] Collapse boundary section to exclusions and conflict-resolution routes not already covered earlier.
- [ ] Group validation by the five shared axes and state that command/link/anchor gates are selected through `proof.md`.
- [ ] Link actionable router targets such as `docs/usage.md` and `AGENTS.md`; keep code spans only for literal path facts.
- [ ] Standardize the shared-standard count as `5 shared standards`.
- [ ] Remove semicolon-ended bullets and clean overloaded routing prose.
- [ ] Define active corpus versus instruction overlays and `_reports/` exclusions.

Agent sources: Huygens, Poincare, Sagan, Halley.

### [2.7][AGENTS]

- [ ] Add a root-file audit contract for every root standards audit or edit.
- [ ] Require broader context inspection before every `docs/standards/**` edit: target file, routed owner standard, linked adjacent standards, and affected folder or type family.
- [ ] Require agents to identify poor information representation before rewriting: prose hiding lookup data, prose-like tables, one-row tables, duplicated table/diagram bodies, decorative diagrams, missing relation records, empty conditional headings, stale links, and proof fields used as decoration.
- [ ] Use exactly the five audit axes: position, form, craft, evidence, notation.
- [ ] Require audit findings to include path plus line or section, axis, issue, correction task, rule or standard to tighten, and proof gap when applicable.
- [ ] Co-locate `_reports/` behavior into one decision table or record.
- [ ] Convert read order into a task-mode table for root audit, shared/root edit, narrow type edit, named `_reports` task, and `docs/usage.md` escalation.
- [ ] Narrow the rule-owner table so it is action-selecting and does not overclaim `README.md` placement or `agentic-documentation.md` instruction-file structure.
- [ ] Align type-standard required order with `information-structure.md`: purpose, use when, route-away, agent use, produced structure, cardinality, adjacent checks, maintenance triggers, then examples.
- [ ] Reconcile preservation with stale deletion: preserve load-bearing current facts; delete stale facts only with current proof or owner routing.
- [ ] Split local-gate proof from provider-behavior proof.
- [ ] Narrow people/process metadata bans so route-owner labels remain allowed.
- [ ] Group heterogeneous forbidden-pattern lists.
- [ ] Add final boundaries and validation so the file does not close on a negative list.
- [ ] Fix slash alternatives, dense prose, predicate mismatch, and vague target-standard language.

Agent sources: Dewey, Cicero, Mill, Mendel.

## [3][IMPLEMENTATION_PRIORITY]

[PASS_1_ROOT_GUARDRAILS]:
- [x] Update `docs/standards/AGENTS.md` with broader-context representation requirements, root-file audit contract, and final validation.
- [x] Update `README.md` routing, active corpus, and root audit axis guidance.

[PASS_2_CORE_STRUCTURE]:
- [x] Update `information-structure.md` and `formatting.md` so chooser, table, progress, glyph, fence, representation, and validation rules prevent the recurring defects.

[PASS_3_PROOF_AND_AGENTIC]:
- [x] Update `proof.md` field schemas, proof gaps, evaluation receipts, and docs-as-code hit policy.
- [x] Update `agentic-documentation.md` provider, retrieval, artifact, MCP, and validation structures.

[PASS_4_CRAFT]:
- [x] Update `style-guide.md` with prose mechanics, owner boundaries, root-audit prose, punctuation validation, and example placement.

## [4][AGENT_REPORT_INDEX]

The individual reports were captured in the conversation stream and normalized into the task list above. No agent edited files. Jason was not part of this root-file batch.

## [5][IMPLEMENTATION_CONFIRMATION]

[CHANGED_ROOT_STANDARDS]:
- [x] `docs/standards/AGENTS.md`
- [x] `docs/standards/README.md`
- [x] `docs/standards/agentic-documentation.md`
- [x] `docs/standards/formatting.md`
- [x] `docs/standards/information-structure.md`
- [x] `docs/standards/proof.md`
- [x] `docs/standards/style-guide.md`

[VALIDATION]:
- [x] `git diff --check -- docs/standards`
- [x] Local Markdown link and anchor validation for changed `docs/standards/**/*.md`
- [x] `pnpm exec mmdc` for 9 changed Mermaid fences
- [x] Structural scan for semicolon-ended bullets and prose numeric ranges in changed root standards

[RENDERER_SUPPORT]:
- Mermaid renderer support for changed fences is proved by `pnpm exec mmdc`.
- Markdown link and anchor support is proved by local path and heading validation.
- No docs-build, C# static, C# test, or bridge rail claim is made by this report.
