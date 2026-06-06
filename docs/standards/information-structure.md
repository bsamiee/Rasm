# [INFORMATION_STRUCTURE]

This standard carries form: which container carries a piece of information and how that container supports scanning, retrieval, and maintenance. Choose the container after the document type is known and before drafting long sections. This standard does not decide salience, prose, visual styling, or evidence strength.

## [1][USE_WHEN]

Apply this standard to choose and shape containers:
- prose, bullets, numbered lists, and checklists
- definition blocks, status-tagged records, tables, decision tables, and lookup tables
- code blocks, intent labels, examples, monospace text structures, progress summaries, Mermaid diagrams, and C4 architecture handoffs
- callouts, collapsible blocks, footnotes, headings, section boundaries, line wrapping, retrieval chunks, and page anatomy.

Salience and ordering within a unit belong to the position standard, sentence mechanics to the craft standard, evidence strength to the proof standard, and visual styling to the formatting standard.

## [2][CONTAINER_CHOOSER]

Use the smallest container that preserves meaning. Change container when the reader's question shifts from explanation to lookup, ordered action, relationship, proof, or machine consumption. Structured containers are not decoration: bullets and key-value blocks outperform prose for option selection and field extraction, and tables outperform both for dense factual lookup.

A single record read by field belongs in a definition block, not a one-row table. Sparse data compared across rows still belongs in a table, not flattened into prose. When no two items share a comparison question, abandon the table and give each item its own record.

Choose the carrier by information signature and reader question:

| [INDEX] | [SIGNATURE]             | [QUESTION]       | [CARRIER]                           | [REJECT]                  |
| :-----: | :---------------------- | :--------------- | :---------------------------------- | :------------------------ |
|   [1]   | one claim               | meaning          | prose paragraph                     | labels; table restatement |
|   [2]   | peer facts              | equivalence      | bullet list                         | steps; state              |
|   [3]   | ordered path            | sequence         | numbered list or ordered record     | inventory                 |
|   [4]   | completion state        | checked result   | checklist                           | lifecycle record          |
|   [5]   | one item fields         | field scan       | definition block or per-item record | one-row table             |
|   [6]   | finite vocabulary       | closed values    | vocabulary card                     | loose definitions         |
|   [7]   | comparable rows         | lookup           | table-form chooser                  | heterogeneous records     |
|   [8]   | condition set           | resolved action  | decision table                      | sequential flow           |
|   [9]   | row detail              | row maintenance  | row-owned record                    | long cell                 |
|  [10]   | command or output       | run/read/prove   | command/output carrier              | transcript prose          |
|  [11]   | parser-owned Markdown   | exact shape      | machine-consumed record             | normalization             |
|  [12]   | topology or ownership   | connection       | visual/topology chooser             | decorative graphic        |
|  [13]   | literal source          | copy/study/avoid | code-fence chooser                  | unlabeled fence           |
|  [14]   | drift-prone claim       | proof/refresh    | proof record                        | hidden proof              |
|  [15]   | low-salience support    | defer reading    | `<details>`                         | proof; warnings           |
|  [16]   | interrupting constraint | interrupt        | GitHub alert                        | emphasis                  |

[TABLE_FORM_CHOOSER]:
- Comparison table: comparable items across shared attributes.
- Lookup table: closed keys to values or behavior.
- Decision table: finite conditions to resolved action, with hit policy declared first.
- Matrix: row category crossed with column category.
- Support matrix: versions, platforms, or features to support fields, caveats, and evidence.
- Dependency matrix: required, forbidden, optional, or missing relationships across two axes.

[VISUAL_TOPOLOGY_CHOOSER]:
- Codemap tree: path ownership, package shape, or generated-output placement.
- Matrix or dependency matrix: compact intersections across two axes.
- Mermaid: branching flow, state transition, sequence, relation, or topology that needs rendering.
- C4 or topology packet: system boundary, container, component, deployment, or resource topology.

[PACKET_PROMOTION_TEST]:
- Promote a packet only when repeated author decisions, independent field updates, proof closure, parser consumption, failure reading, or omission behavior exceeds ordinary prose, list, table, or definition-block carriers.
- Keep packet catalogs out of tables unless the reader truly compares two axes. Prefer compact definition blocks for packet families.
- Collapse a packet to prose or a short field block when fewer than 3 fields are independently scanned, updated, proved, or omitted.
- Omit absent fields. Never fill a packet to satisfy a template.

Narrative units still need structure. A standalone sentence is valid only when it carries a lead, transition, consequence, caption, route boundary, or explicit proof gap. If a sentence names a scannable value, convert it to a field line. If it qualifies a table row, move it to a note or row-owned record. If it warns, use an alert. If two loose sentences sit apart as separate paragraphs, join them into a paragraph pair with a lead and closing consequence, or turn them into list items or relation records.

## [3][TABLES]

Use a table when row-and-column comparison or lookup across a homogeneous set is the point. Keep it within bounds that agent readers and split-pane readers handle.

[ELIGIBILITY]:
- Use a table for comparable items across shared attributes, closed-key lookups, finite condition-action rules, matrices, support policy, or dependency intersections.
- Avoid a table when the content is a sequence of actions, when the first column repeats one long phrase, when a single record is read by field, or when rows are heterogeneous records.
- The formatting standard carries table surface: enumerable Markdown tables carry `[INDEX]` first, bracketed uppercase rubrics in the header row, and `[1]` through `[n]` row identifiers.

[BOUNDS]:
- A table degrades past 15 columns or 20 rows.
- A table also degrades when rendered width forces horizontal scrolling in normal split-pane review or when more than one column is prose.
- A cell holds one atomic fact: a single value, short phrase, status token, compact marker, or Markdown inline such as a code span or link.
- Keep cells to 8 words or fewer. A column whose cells average more than 8 words is a prose column; a table may carry at most one prose column, and it must be the last column.
- Keep the stub column short, unique, and scannable: an identifier, command, proper noun, or status token, not a sentence.

[DECOMPOSITION]:
- Row count over 20 with 4 columns or fewer: split by a natural row axis such as status, phase, platform, or route into sibling tables under the ceiling; lead each sibling with one sentence naming the axis value it covers.
- Column count over 15 with 4 rows or fewer: pivot so subjects become rows; apply the row-split rule if the transposed table still exceeds 20 rows.
- Heterogeneous row records: abandon the single table for summarize-then-detail.
- Pivot outputs are transposed table, profile-split sibling tables, or key-value expansion.
- Summarize-then-detail uses a summary table of at most 5 columns with one row per axis value and one column carrying the detail-section anchor; detail tables or record sections immediately follow.

[PROSE_PAIRING]:
- Promote prose to a table when a paragraph compares 3 or more items across 2 or more attributes.
- Frame a table with 1 or 2 sentences only when the reader cannot act on the table alone: status context, applicable-row selection, or whole-set invariant.
- Follow-on prose may state a consequence or exception, but it must not restate cell values.
- In a mixed block, prose carries decision criteria or invariant; the table carries per-item values.
- A table followed by `[USE]`, `[DETAIL]`, `[NOTE]`, or another index-keyed block is valid only when the row set is identical, the secondary block is short, and no row needs independent proof, status, update, or removal fields. Otherwise use one complete table, row-owned records, grouped definition records, or subsection-per-record blocks.

[FORMS]:

| [INDEX] | [FORM]            | [ROW_AXIS]                    | [COLUMN_AXIS]                     | [USE]                                           |
| :-----: | :---------------- | :---------------------------- | :-------------------------------- | :---------------------------------------------- |
|   [1]   | Comparison table  | comparable items              | attributes                        | compare options, commands, sources, profiles    |
|   [2]   | Lookup table      | keys                          | value or behavior                 | map tokens, commands, codes, paths, owners      |
|   [3]   | Decision table    | condition combinations        | inputs plus resolved action       | choose one rule from finite overlapping facts   |
|   [4]   | Matrix            | row category                  | column category                   | show intersections, support, dependency, access |
|   [5]   | Support matrix    | versions, platforms, features | support fields, caveats, evidence | state compatibility or capability policy        |
|   [6]   | Dependency matrix | dependents or producers       | dependencies or consumers         | show allowed, required, forbidden relationships |

[DECISION_LOOKUP_RULES]:
- A decision table has one column per input condition on the left and the resolved action on the right, one row per enumerated combination.
- Declare hit policy before the table when rows can overlap.
- Use `first match wins` only when row order is part of the rule and rows are sorted from most specific to broadest.
- Use `most specific wins` when the row with the fewest wildcards controls.
- Use `all matching actions apply` only when actions compose without conflict.
- If no deterministic hit policy exists, the table is not safe; convert the rule to prose plus cases or redesign the rule.
- A lookup table maps a discrete key to a value, behavior, or next state and is read by key rather than compared across rows.

GFM tables are flat row-and-column structures. They do not support row spans, column spans, nested lists, multiline cells, or reliable embedded HTML. If a row needs nested facts, use definition records; if a heading needs column groups, split the table or use a matrix with explicit stub labels.

Validate table integrity before publication: every row in one table has the same cell count after escaped pipes are accounted for, literal pipes inside cells are escaped, and a table over the row or column ceiling decomposes by the dominant violation. A malformed lookup table is worse than prose because agents will parse the wrong columns confidently.

## [4][RECORDS_LISTS]

Render a finite enumerable set whose items carry state as structured records, never as flat prose. Milestones, decisions, requirements, risks, tasks, and gates need status, dependency, and completion evidence. Use a table while items stay homogeneous and short-celled; switch to a per-item record block once any field needs more than a cell.

[STATUS_VOCABULARY]:

| [INDEX] | [STATUS]   | [MEANING]                                                                                      |
| :-----: | :--------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | `QUEUED`   | accepted for the record set or sequence, not yet executing                                     |
|   [2]   | `ACTIVE`   | actively executing inside the record's scope                                                   |
|   [3]   | `BLOCKED`  | unable to advance until a named dependency, decision, access grant, or proof gap closes        |
|   [4]   | `DEFERRED` | intentionally outside the current sequence, with a return event                                |
|   [5]   | `COMPLETE` | exit condition met and proof agrees where proof is required                                    |
|   [6]   | `DROPPED`  | removed before accepted execution or removed from the current corpus; no successor is required |
|   [7]   | `CANCELED` | accepted work stopped; successor, rollback, or retired need is required                        |

Use this closed default `Status` vocabulary so an agent can filter exact strings. A type standard may define a domain-specific status set in place of this default only when it defines exact casing, active states, blocked states, returnable states, terminal states, and deletion or removal behavior before the first example. A narrowed subset of the default status set is a domain-specific status set and must declare omitted states and removal behavior before examples.

[RECORD_FIELDS]:
- Grouping: scan groups only.
- Normative order: `[FIELD_ORDER]` remains the authoritative record order.

[IDENTITY_STATE]:
- `ID`: stable reference used only when another item, proof receipt, dependency edge, milestone, register, or adjacent document points to the record.
- `Status`: current lifecycle state from the closed set.
- `Changed fact`: path, command, contract, milestone, support row, gate, symptom, public symbol behavior, or other source fact consumed by the record.

[CONSUMPTION]:
- `Consumed by`: section, artifact, reader action, proof rule, route, or maintained document that uses the fact.
- `Use in this document`: how the fact changes reader action, proof, route, status interpretation, safe operation, or validation.
- `Exit`: single observable, falsifiable condition that moves the item to `COMPLETE`: a shipped artifact, merged path, or passing gate.
- `Depends`: item titles, anchors, IDs, or conditions that must hold first; `COMPLETE` is the default required state when no state is named.

[PROOF_MAINTENANCE]:
- `Evidence`: proof summary using the label defined by [proof.md](proof.md) when `Exit` is not self-evident.
- `Update when`: source event that requires this record or document to change.
- `Close when`: condition that removes the relation, closes the record, or proves the adjacent update is complete.
- `Route-away`: body of work that remains in another route.

[FIELD_ORDER]:
- Lifecycle records default to `ID`, `Status`, `Changed fact`, `Consumed by`, `Use in this document`, `Exit`, `Depends`, `Evidence`, `Update when`, `Close when`, `Route-away`, then domain-specific detail fields.
- Adjacent-document relation records use `Changed fact`, `Consumed by`, `Use in this document`, `Update when`, `Close when`, `Route-away`; `ID` and `Status` may precede that sequence only when the type standard declares the relation item itself has lifecycle.
- Type-local preamble fields may precede an adjacent-document relation record when they change reader action or identify the consuming surface. Once `Changed fact` appears, keep the shared relation fields contiguous and in order.
- A local exception must name every omitted, renamed, moved, or domain-mapped shared field before the first example. Once a shared field appears, preserve its relative order.
- Proof sub-blocks use the proof-local field run from [proof.md](proof.md) without interruption. Local identity or context fields such as `Gate`, `Surface`, `Representation`, or `Proof route` may precede `Evidence`; local result, match, disposition, or close fields follow `Review trigger`.

[VOCABULARY_CARD]:
- Vocabulary: exact vocabulary name.
- Applies to: field, table column, record set, type standard, or produced-document surface.
- Values: closed values with exact casing.
- Owner: shared standard or type standard that owns the value meanings.
- Default or initial value: only when the absence of a value has meaning.
- Removal behavior: how values leave the document or record.
- Lifecycle groups: active, blocked, returnable, terminal, omitted, or dropped groups only when lifecycle behavior matters.
- Machine/display split: only when the rendered value differs from the machine or source value.
- Unknown value rule: only when source unknowns can appear.
- Evidence, Proof gap, Last verified, Review trigger: only when the vocabulary claims current source, support, provider, generated, or drift-prone behavior.

Concrete status values stay in the type standard that validates the produced document. Type-local vocabularies also stay local when they control publication state, reader readiness, proof closure, or produced-document validity. A shared vocabulary card defines the carrier and field semantics; it does not import every type-local status into the default lifecycle vocabulary.

[RECORD_FORMS]:
- Per-item record block: H3 item identifier plus fields, one `label: value` per line.
- Escalate from record table to per-item record blocks when any item has more than 5 fields, any field needs a list or code block, or items are updated independently over the document's life.
- Adjacent-document relation record: use only when another maintained document changes reader action, proof, status interpretation, validation, or maintenance; put it beside the section that consumes the adjacent fact and delete it when the fact no longer affects this document.
- Task ID: use only when another task, milestone, proof receipt, dependency edge, or adjacent document references the item. Put the stable ID at the front of the item, such as `[M2]` or `[ADR-0007]`; do not issue IDs merely because a list is numbered.
- Progress: represent progress only when the document states the numerator, denominator, closure rule, and proof surface before the marker. The rendered progress marker uses [formatting.md](formatting.md); counts, closure units, and proof details stay in surrounding fields. Percentages, bars, phases, and complexity values are valid only when the document defines the calculation or decision rule they measure.

Use this relation-record field order exactly:

```markdown template
Changed fact: <path, command, contract, milestone, support row, gate, symptom, or public symbol behavior>
Consumed by: <this document section or produced artifact>
Use in this document: <reader action, proof rule, route, status interpretation, safe operation, or validation>
Update when: <source event that requires this document to change>
Close when: <condition that removes the relation or proves the adjacent update is complete>
Route-away: <body of work that remains in the adjacent standard>
```

[CHECKLISTS]:
- Verification checklist: author self-check of observable, falsifiable conditions; item text only, no route or completion evidence. The validation section closing each standard uses this form.
- Acceptance checklist: external gate; each item carries an `Exit` condition. Completion evidence is added on completion through the field label defined by [proof.md](proof.md).
- Status checklist: living tracker; each item carries `Status` and, where they exist, `Depends` and proof fields.
- Checklist items use `- [ ]` or `- [x]`, never plain bullets, when completion is asserted and verified.
- Fields trail item text after an em dash. A checklist item may carry at most 3 trailing fields, all on the same line. When completion evidence needs several lines, an item needs a list-valued field, or fields are updated independently, promote the item to a structured record.

[LISTS]:
- Use bullets for equivalent items and numbered lists only when order is real.
- Keep items parallel in grammar and scope, and avoid single-item lists.
- Limit nesting to 2 levels; split deeper structure into subsections.
- Split a list past 7 items into named sets, each set introduced by a standalone bracketed `[X_Y_Z]:` label with formatting-standard spacing.
- Do not mix ordered and unordered items in one logical block.
- Use an ordered record when a numbered step has independently scanned fields such as `Action`, `Command`, `Expected signal`, `Failure route`, or `Proof`.
- Keep short ordered-record fields on the numbered item when they fit one line. Indent continuation fields 4 spaces under the step when more than one field is required.
- A fenced command or output block may appear inside an ordered record only as that step's command or expected signal; split commands and output into separate fences.
- Promote a step to a subsection-per-record block when it needs independent proof, rollback, escalation, or maintenance fields.

[DEFINITION_BLOCKS]:
- Use one label per line when a label carries meaning a reader will scan, quote, or update independently.
- For several records sharing one schema, use a grouped definition block: a plain record-name line, then shared `label: value` fields indented 4 spaces beneath it, with a blank line between records.
- A list-valued field keeps the label on its own line and indents child list items 4 spaces beneath the label; a wrapped prose continuation also indents 4 spaces.
- Once a record exceeds 5 fields, 2 or more fields need continuations, or any field needs a code block, move to a subsection-per-record block.
- Do not pack several labeled facts into one sentence, and do not widen a record into a one-row table.

[CONTRAST_RECORDS]:
- Use a compact contrast record when a rejected form is plausible, observed in source, or the positive form is easy to copy incorrectly.
- Fields are `Accepted:`, `Rejected:` or `Near miss:`, and `Reason:`. Formatting owns label spelling and spacing.
- Do not use a contrast record for every rule. Use a single positive example when the rejected form is not likely.
- Use fences only when line breaks, indentation, syntax highlighting, copyability, or renderer recognition is part of the distinction.

## [5][LITERAL_MACHINE_SURFACES]

Use literal and machine-consumed carriers only after deciding what the reader or parser needs to do with the surface. Parser-owned shape, copyable source, observed output, and executed proof are different carriers.

[MACHINE_CONSUMED_RECORD]:
- Consumer: parser, analyzer, release ledger, generator, or other named tool.
- Required shape: headings, fields, row order, or fence grammar the consumer reads.
- Fields validated: fields or rows the local consumer actually checks.
- Fields not validated: adjacent source shape that remains human convention or upstream behavior.
- Controlling source: source, manifest, generator, or maintained provider route.
- Validation command: exact command or `Proof gap:`.
- Exception: no-normalize rule that keeps the parser-owned shape intact.
- Review trigger: consumer, parser, generator, source schema, or upstream release-shape change.

Machine-consumed Markdown may use a narrower shape when a parser, analyzer, release ledger, generator, or review tool consumes exact headings, fields, rows, or fence grammar. Declare the exception before applying ordinary heading, table, field, or fence normalization. Generated ledgers add `Generated from:`, `Generated artifact:`, `Expected value:` or `Expected output:`, and `Comparison:` or `Verification:`. Provenance proves origin; comparison proves the generated artifact agrees with source. Roslyn analyzer release ledgers may keep their machine shape rather than adopting ordinary-document records; do not refactor them into human-pretty prose unless the consumer changes.

[FENCE_RULES]:
- Every ordinary code fence carries a language tag in its info string, and the intent label follows the language.
- Fence standalone commands, literal files, configs, schemas, multiline examples, and snippets whose line breaks, indentation, syntax highlighting, copyability, or renderer recognition matter.
- Mark exactly one intent label so a reader knows whether the block is safe to run, study, fill in, or avoid.
- Renderer-local fences use the exact renderer tag and carry intent in nearby visible prose instead of in the info string; Mermaid fences are `mermaid`, not `mermaid conceptual`.
- Reference and API-heavy pages must not publish ordinary `bash`, `csharp`, `json`, or similar fences without an intent label.
- Use other language tags only when the block is genuinely that source language and still carries one intent label.

[FENCE_INTENTS]:
- `copy-safe`: run or paste as written. For a config or data block, use this only when the block is byte-equivalent to a named source-of-truth file and name that file in the label.
- `template`: copy the structure, then replace every placeholder before use. Use for section templates, lead contexts, and table shapes that contain placeholders.
- `conceptual`: illustrative or proposed shape, not a verbatim or runnable artifact.
- `test-only`: valid only in a test or fixture context.
- `generated`: produced by a generator; edit the source, not the block.
- `output-only`: sample output, not an input to run.
- `deprecated`: retained for recognition; do not adopt.
- `rejected`: counter-example shown to prevent misuse.

This standards corpus currently allows these language-intent pairs:

| [INDEX] | [FENCE]               | [USE]                                                  |
| :-----: | :-------------------- | :----------------------------------------------------- |
|   [1]   | `bash copy-safe`      | command a reader can run or paste as written           |
|   [2]   | `markdown template`   | copyable Markdown structure with placeholders          |
|   [3]   | `markdown conceptual` | illustrative Markdown shape, not current policy        |
|   [4]   | `markdown rejected`   | rendered Markdown counter-example                      |
|   [5]   | `text conceptual`     | source-neutral illustrative text shape                 |
|   [6]   | `text template`       | source-neutral label/value or terminal-shaped template |
|   [7]   | `text rejected`       | source-neutral counter-example                         |
|   [8]   | `diff output-only`    | bounded generated-diff proof                           |
|   [9]   | `text output-only`    | expected output signal                                 |

[COMMAND_OUTPUT_CHOOSER]:
- Copy-safe command: runnable command or exact invocation; use `bash copy-safe` or an inline `Command:` field.
- Expected signal: success text, short stdout or stderr cue, or state change the reader compares; use `text output-only` or an `Expected signal:` field.
- Observed output: command output captured as proof; route evidence fields through [proof.md](proof.md).
- Failure-reading rule: how stdout, stderr, exit status, artifacts, side effects, or logs classify a failure.
- Command card: command, purpose, precondition, effect, output, proof, and review trigger.
- CLI envelope record: stdout, stderr, exit status, artifacts, side effects, and failure reading.
- Generated comparison: generated artifact, expected value or output, comparison, and refresh route.
- Executed proof: exact command plus evidence, freshness, and proof-gap fields owned by [proof.md](proof.md).

Keep blocks short enough to review. Pair runnable commands and observed output as separate fences: `bash copy-safe` for the command a reader runs, then `text output-only` for the expected signal when the signal needs a block. Do not paste terminal transcripts into copy-safe fences. Use `diff output-only` for bounded generated-diff proof, and link the generated contract, manifest, or source file instead of pasting a long diff or machine output whole.

Do not fence short accepted/rejected, before/after, good/bad, or near-miss examples when each side is only a sentence, heading, command, field set, or compact value. Use the compact contrast record carried by [formatting.md](formatting.md) so paired values stay adjacent and the rejection reason remains visible.

Do not let command syntax imply execution proof. A copy-safe command tells the reader what to run; an executed proof record says it was run and what it proved.

## [6][VISUAL_TOPOLOGY]

Use visual and monospace topology only when arrangement, branch shape, relationship, hierarchy, state, progress, dependency, alignment, or comparison is the reader question. A visual is not a decoration, and one visual answers one reader question.

[VISUAL_CARRIERS]:
- Grouping: reader job.
- Rule: each item preserves the carrier, its use, and its rejection.

[SOURCE_STRUCTURE]:
- Codemap tree: path ownership, package shape, or generated-output placement. Reject generic folder inventories.
- Type-shape block: compact relationship between interfaces, records, unions, carriers, or data flow. Reject full architecture diagrams.
- Edge list: directed relation where a full diagram adds no clarity. Reject multibranch workflows.

[INTERSECTION]:
- Small matrix: state, capability, support, dependency, or gate intersections that fit in source. Reject wide support policy.
- Dependency matrix: required, forbidden, optional, or missing relationships across 2 axes. Reject prose dependency lists.

[STATE_SIGNAL]:
- Gate or risk map: gate, risk, signal, action, and evidence. Reject unproved status dashboards.
- Glyph legend: closed local alphabet for compact state, result, change, or risk. Reject decorative symbols.
- Progress bar: 20-cell completion marker backed by numerator, denominator, closure rule, and proof surface. Reject duplicate checklist state.
- Alert: one interrupting note, tip, invariant, warning, or caution. Reject ordinary emphasis.

[RENDERED_TOPOLOGY]:
- Mermaid: branching flow, state transition, sequence, class/entity relation, or topology. Reject decorative renders.
- C4 or topology packet: system boundary, container, component, deployment, or resource topology. Reject type-standard replacements.

[REVIEW_ONLY]:
- DOT-like shorthand.
- Formal finite-state-machine grammar.
- Sparklines or microplots.
- Heatmaps.
- Mini timelines.
- Swimlane text grammar.
- Generated visual indexes.

Use monospace text when raw-Markdown inspection matters more than a rendered image: file trees, repository layout, artifact placement, progress summaries, small stacks or matrices, and tiny flows embedded in code comments where no render step exists. UTF-8 box drawing, block glyphs, and bitmap-style text are allowed when the repository and renderer preserve them and the glyphs encode load-bearing hierarchy, order, state, progress, dependency, alignment, or column comparison. Use plain ASCII only when the target surface cannot reliably render those glyphs.

Define a text-graphic alphabet only when the reader needs it to decode the structure. A glyph legend is valid when it is closed, local, and changes reader action; it is rejected when it only decorates the page. Do not use FIGlet banners, ornamental separators, decorative frames, copied terminal animations, ANSI color output, photo-to-ASCII art, or box art whose alignment is not load-bearing.

A file tree uses box-drawing connectors, with `├──` on every child but the last and `└──` on the last, and a `│` riser carrying down through each open branch:

```text conceptual
project/
├── README.md
└── reference/
    └── api.md
```

Alignment is the whole point. Connectors, progress cells, and columns must line up exactly in a monospace font, because misaligned text graphics read harder than the prose they replace. A small stack, box, or matrix must align to a single width so values read down a column, not just across a row.

Use a tiny matrix only when the crossings matter:

```text conceptual
          state  proof  update
Lifecycle yes    yes    yes
Note      no     no     no
```

Text equivalent: lifecycle records carry state, proof, and update fields; notes do not.

Accepted: use a short aligned tree, stack, box, or matrix when source inspection is the reader job.
Rejected: draw a multibranch workflow with ASCII arrows.
Reason: branching flows with multiple decisions, actors, or states have outgrown monospace text and belong in Mermaid.

Use Mermaid when rendered structure adds value beyond bullets or monospace text. Mermaid source is compact, text-editable, and renderer-backed, so prefer it over embedded images for any diagram an agent may need to read or revise. Use an exact `mermaid` fence, not an intent-labeled fence, because Markdown renderers detect Mermaid by the language tag. State conceptual, template, generated, or rejected intent in the lead-in sentence or caption.

Mermaid source may use the renderer `config:` block inside the diagram fence; this is diagram configuration, not page fact storage. Prefer `layout: elk` when the repository has Mermaid ELK support, set `look: neo`, and use `theme: base` when theme variables are needed. Place `accTitle` and `accDescr` immediately after the diagram declaration when the diagram type supports them, and keep a visible text equivalent nearby.

[MERMAID_TYPES]:
- `flowchart`: branching workflow or data movement.
- `sequenceDiagram`: actor-to-actor interaction over time.
- `stateDiagram-v2`: lifecycle, statuses, or transitions.
- `erDiagram`: entities and relationships.
- `classDiagram`: type relationships when names alone are insufficient.
- `quadrantChart`, `sankey-beta`, `architecture-beta`, and C4 views: comparative positioning, flow volume, deployment or resource topology, or system structure when a simpler type loses meaning.

Keep diagrams small enough to review in source. Prefer roughly 5 to 9 nodes and no more than about 12 meaningful edges for hand-maintained diagrams. Split a diagram when it has more than 2 decision nodes, more than 1 lifecycle, multiple unrelated subgraphs, or labels that repeat table cells. Use stable semantic node IDs such as `Request`, `Quota`, or `Recovery` instead of one-letter IDs except in tiny examples where the rendered label is the whole subject. Keep IDs ASCII-safe and distinct from Mermaid keywords. Quote labels containing punctuation, parentheses, or reserved words inside the node label, not by making the node ID a sentence. Edge labels must add a condition, status, or action that is not obvious from the node names.

Use at most one controlling representation for one decision, edge set, branch, lifecycle, or status fact. Pick the carrier by job: codemaps show current structure and path routes, Mermaid shows flow, state, dependency, or boundary crossing, tables compare comparable fields, and records hold independently updated facts. Publish both a table or record and a diagram only when each carries a distinct reader job: tables and records own source, status, proof, live source, update, and removal triggers; diagrams own topology, sequence, branch/rejoin shape, dependency shape, or lifecycle transition shape. Add an explicit job-split sentence only when adjacent representations could duplicate the same fact set. Delete one representation if removing it loses no unique reader action.

For architecture, use [explanation/architecture.md](explanation/architecture.md) to choose the C4 profile floor, static Context and Container semantics, Component drill-down rules, and deployment or resource-topology cases. Choosing whether a diagram is needed is this standard's call; how an architecture model is structured belongs to the architecture type standard.

## [7][SECONDARY_CONTAINERS]

Use secondary containers only when they change reading path, salience, portability, or economy. They do not replace proof, safety, first-read procedures, or ordinary structure.

| [INDEX] | [FORM]   | [USE]                                                  | [SHAPE]                      | [REJECT]                              |
| :-----: | :------- | :----------------------------------------------------- | :--------------------------- | :------------------------------------ |
|   [1]   | Callout  | interrupting note, tip, invariant, warning, or caution | GFM alert                    | decoration; nesting; stacking         |
|   [2]   | Details  | low-salience trace, option dump, or long output        | `<details>` plus `<summary>` | hidden proof or first-read procedure  |
|   [3]   | Footnote | short claim-local provenance or qualification          | `[^label]`                   | proof replacement; long qualification |

[SECONDARY_RULES]:
- Callouts use one of `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, or `> [!CAUTION]`; use one callout per concern, and use `TIP` only for real user-facing efficiency payoff.
- Details keep required constraints, proof, safety warnings, and first-read procedures visible. Put a blank line after `<summary>...</summary>` and before `</details>` so nested Markdown renders predictably.
- Footnotes use local, monotonic labels. Drift-prone claim evidence stays in the visible proof block, and table qualifications stay beside the table.

Hidden HTML comments are source-only notation, not a reader-facing container. Use the formatting standard's `<!-- source-only: ... -->` shape for author or generator hints, and never use a comment as the only intent label for a table, example, generated block, or safety constraint. Machine-consumed Markdown exceptions are declared in `[5][LITERAL_MACHINE_SURFACES]` before ordinary formatting rules apply.

Do not hard-wrap Markdown prose. Write each paragraph as one logical line and let the renderer and editor soft-wrap it. Insert a manual line break only where it is structural: between list items, table rows, definition-block fields, or inside fenced blocks. Do not use trailing spaces for hard breaks; if a hard break is truly part of rendered content, prefer a list, table row, definition field, or fence over inline hard-break syntax.

Carry meaning with structure, not ornament. Markdown structure is cheap, readable, and portable, so spend tokens on signal: meaningful headings, lists, records, and tables, not nested decoration. Avoid stacked emphasis and redundant rules, and let one structure own each section.

## [8][PAGE_SHAPE]

Render page shape as 3 separate jobs: standard file anatomy, type-standard opening order, and produced-document skeleton. The template is the structure prescription; conditional sections appear only when their condition holds so agents do not publish empty headings.

```markdown template
# [TITLE]

<Lead: scope and promise in one short paragraph.>

## [1][USE_WHEN]

## [2][<REQUIRED_SHAPE>]

## [3][BOUNDARIES]

## [4][VALIDATION]
```

[SECTION_CARDINALITY]:
- `Lead`, `Use when`, rules section or sections, `Boundaries`, and `Validation`: required.
- `Examples`: conditional; include only where misuse is likely.
- Conditional sections: add with `## [N][<CONDITIONAL_SECTION>]` only when the condition holds.
- Every long standard needs a chooser, boundaries, and a validation section.

[TYPE_STANDARD_OPENING]:
1. Purpose and boundary in the lead.
2. `Use when`.
3. Route-away rule.
4. Agent use: how the document changes editing, proof, routing, or maintenance.
5. Required produced-document structure.
6. Section cardinality.
7. Adjacent checks and relation-record rule.
8. Maintenance triggers.

Taxonomies, baselines, examples, graphics, and provider notes follow only after this contract unless the type standard is itself one of the shared standards. A visually complete page that buries structure or field order below examples is non-conforming.

```markdown template
# [SCOPE_TYPE]

<Lead.>

## [1][<SECTION_A>]

## [2][<SECTION_B>]

## [3][BOUNDARIES]

## [4][VALIDATION]
```

Tag each produced-document heading `required | conditional | optional | repeatable` in a cardinality block beneath the template. `required` sections always appear, `conditional` sections appear only when their condition holds, `optional` sections appear at author discretion, and `repeatable` records appear once per item. Put conditional sections in a separate addition block unless every instance of the type needs the heading. Put record-field order beside the first record template, not in a late validation section.

[HEADINGS_CHUNKS]:
- Use one H1 and do not skip heading levels.
- Treat H2 sections as primary retrievable units that stand alone.
- Use H3 only to refine one H2 concern; avoid H4 and deeper unless a renderer or generated format requires them.
- Use the bracketed heading idiom the formatting standard carries, and do not put links in headings.
- Keep heading labels short: 1 or 2 semantic words by default, 3 words when needed, and more only when a source name or command family would become ambiguous.
- Each H2 should carry enough context to be read out of order.
- When a section could be reused as a generated mirror, task template, or state artifact, state that artifact type where the distinction changes how an agent uses it.

[EXAMPLES]:
- Include an example only when the rule is easy to misapply.
- Put the example beside the rule it clarifies and keep its data realistic.
- Mark placeholders and omitted sections explicitly.
- Label any block a reader could copy, run, or mistake for current policy with its intent.
- Do not publish interaction excerpts, nonpublic local paths, or local task notes as reusable patterns.

## [9][BOUNDARIES]

- [agentic-documentation.md](agentic-documentation.md) carries salience and the placement of content within the containers this standard shapes.
- [formatting.md](formatting.md) carries visual styling of these containers: table alignment, status and invocation markers, whitespace, and the heading-label idiom.
- [style-guide.md](style-guide.md) carries the words inside every container.
- [proof.md](proof.md) carries evidence strength and freshness for the facts a table, record, diagram, or block presents.
- [README.md](README.md) carries document-type routing and links to type standards such as the architecture standard.

## [10][VALIDATION]

Use this verification checklist by group:

[PAGE_SHAPE]:
- [ ] The page follows the prescribed anatomy: lead, use when, rules, boundaries, validation, and examples only where misuse is likely.
- [ ] One primary container carries each section, and mixed blocks assign disjoint routes.
- [ ] Lists nest no deeper than 2 levels and sets past 7 items use standalone bracketed set labels that do not render inside the preceding paragraph.
- [ ] Root-file audits can be recorded against the 5 shared-standard axes without adding a one-off grading container.
- [ ] Chooser tables stay narrow, avoid prose-heavy cells, and route broad rows to a named subchooser.
- [ ] Prose is not hard-wrapped; manual breaks are structural only.
- [ ] Headings form standalone retrievable H2 units and carry no links.
- [ ] Examples sit beside the rule they clarify.

[RECORDS_TABLES]:
- [ ] Tables stay within column and row bounds, hold no paragraph cells beyond one trailing prose column, and decompose by the dominant violation when over.
- [ ] Table eligibility checks rendered width and prose density, not only row and column count.
- [ ] A finite enumerable set of trackable items uses status-tagged records with `Status`, `Exit`, and applicable dependency or completion-evidence details, never flat prose.
- [ ] Checklists use the checkbox form and carry the fields their checklist form requires.
- [ ] A single record uses a definition block; record clusters use grouped or subsection-per-record blocks.
- [ ] Decision and lookup tables are used for condition-action and key-value content respectively, and decision tables declare hit policy before examples.

[LITERAL_MACHINE]:
- [ ] Ordinary code blocks carry exactly one intent label, renderer-local fences use exact language tags, and placeholder templates use `template`, not `copy-safe`.
- [ ] Code-fence language-intent pairs are declared before examples use them.
- [ ] Machine-consumed Markdown declares consumer, exact shape, omitted fields, and validation command or proof gap.
- [ ] Command syntax, expected output, observed output, and executed proof stay in separate carriers.

[VISUAL_SECONDARY]:
- [ ] Monospace text is short and raw-Markdown readable; glyphs carry load-bearing meaning; Mermaid is used only when rendering adds value.
- [ ] Diagrams, alerts, glyph-heavy graphics, progress bars, and monospace structures have text equivalents where adjacent text does not already carry the meaning.
- [ ] Callouts, collapsible blocks, and footnotes are used for their purpose and the renderer supports them.
- [ ] Hidden comments are source-only hints, and any reader-facing safety, proof, or intent text is visible.
