Question: Which proposed structure and notation tasks risk damaging prose craft, readability, examples, captions, or non-formulaic agent-facing wording?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: docs/standards/style-guide.md :: prose-to-structure conversion and example placement :: refine
Target owner: docs/standards/style-guide.md; docs/standards/information-structure.md
Source basis: mandatory instruction files, session manifest, collective task list, active `style-guide.md`, `information-structure.md`, and style/prose-cited reports
Promotion target: docs/standards/style-guide.md; docs/standards/information-structure.md
Outcome: MERGE

## [FINDINGS]

Finding 1
    Task-list target: `Add or cross-link prose-to-structure conversion tests`.
    Prior finding extended: `track-information-structure/01-information-structure-container-toolbelt.md` and `track-repo-markdown/02-non-standards-markdown-patterns.md` both ask for isolated-sentence triage; `track-type-corpus/01-explanation-type-corpus.md` warns against blanket one-line-paragraph cleanup.
    Critique: The task is directionally correct, but it must not become a conversion mandate. `style-guide.md` already allows isolated sentences for leads, transitions, captions, closing consequences, route boundaries, and explicit proof gaps. If the new chooser tells agents to convert every short sentence into a field line, it will erase useful rhythm and make the standards read like forms instead of executable prose.
    Correction route: Promote a "keep or convert" test, not a "convert" table. A sentence stays prose when it has a predicate and changes reader interpretation, sequence, proof, or route. It becomes a field only when the reader scans or updates the value independently.
    Outcome: MERGE.

Finding 2
    Task-list target: `Add an input-first information signature chooser`.
    Prior finding extended: `track-external-research/03-information-architecture-structure-chooser-a.md` and `04-information-architecture-structure-chooser-b.md` support input-first routing but caution against one mega-table.
    Critique: The chooser will help authors, but the candidate rows already risk becoming a registry of every carrier name. A long chooser with command cards, CLI envelopes, API field cards, source-proof packets, topology bundles, gate matrices, row sidecars, and codemap trees can train agents to pick jargon before they understand the sentence job.
    Correction route: Keep the top chooser short and plain: prose, list, definition block, table, record, ordered record, decision table, visual/topology, code fence, proof record. Put rare packet names in subchoosers only when the common carrier is not enough.
    Outcome: MERGE.

Finding 3
    Task-list target: `Promote representation co-location and alert demotion tests`.
    Prior finding extended: `track-information-structure/01-information-structure-container-toolbelt.md`, `track-type-corpus/01-explanation-type-corpus.md`, `track-repo-markdown/02-non-standards-markdown-patterns.md`, and external structure reports.
    Critique: The proposed job split is strong: diagrams own topology or sequence; tables and records own status, proof, update, and removal facts. The style risk is mandatory captioning. If every diagram/table pair must carry a formulaic "diagram job/table job" caption, ordinary sections will accumulate mechanical throat-clearing.
    Correction route: Require a job-split sentence only when duplication risk is visible or when both carriers sit adjacent to the same fact set. Otherwise let the lead sentence or surrounding paragraph do the work.
    Outcome: MERGE.

Finding 4
    Task-list target: `Move or reframe README late examples`.
    Prior finding extended: `track-type-corpus/02-reference-type-corpus.md` flags the late README examples and asks for critique on whether combined examples justify staying late.
    Critique: "Examples beside rules" is a good default, but combined examples sometimes teach the interaction among several rules better than scattered micro-examples. A late example section is wrong when it becomes a gallery. It is valid when its lead names the reader route it demonstrates and each example has a tight caption or reason sentence.
    Correction route: Preserve late combined examples only under a named route such as `READER_ROUTE_EXAMPLES`, with one sentence saying which rules combine. Move local misuse examples beside their rules. Do not make all examples local or all examples late.
    Outcome: HOLD for the README file; PROMOTE the style distinction.

Finding 5
    Task-list target: `Move contrast-record form ownership to information structure` and `Replace the accepted/rejected mini-table with a compact contrast record`.
    Prior finding extended: `track-formatting-notation/01-formatting-token-systems.md` through the collective task list.
    Critique: Compact contrast records improve sentence examples, but they can become repetitive if every rule has `Accepted`, `Rejected`, and `Reason`. The craft rule should require contrast records only where the rejected form is plausible or recurring. Otherwise a single positive example beside the rule is enough.
    Correction route: Add a misuse threshold: use a contrast record when an agent is likely to copy the rejected form, when a current source shows the defect, or when the positive shape is not self-explanatory.
    Outcome: MERGE.

Finding 6
    Task-list target: `Promote field packet, proof record, API field card, and CLI envelope schemas`; `Define command/output surfaces and callable contract packets`; `Add generated-ledger packet`.
    Prior finding extended: `track-information-structure/01-information-structure-container-toolbelt.md`, `track-repo-markdown/02-non-standards-markdown-patterns.md`, and `track-external-research/03-information-architecture-structure-chooser-a.md`.
    Critique: Packet definitions are useful for command and proof surfaces, but the writing standard needs an anti-formulaic guard. A packet with 8 fields can be worse than a paragraph when only 2 fields carry independent value. Over-specified templates invite filler fields, even though `docs/standards/AGENTS.md` forbids absent-field placeholders.
    Correction route: Pair every new packet with an omission and collapse rule: omit absent fields; collapse the packet to prose or a short field block when fewer than 3 fields are independently scanned, updated, or proved.
    Outcome: MERGE.

Finding 7
    Task-list target: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules`.
    Prior finding extended: `track-external-research/03-information-architecture-structure-chooser-a.md` and `04-information-architecture-structure-chooser-b.md`.
    Critique: Decomposition rules protect tables from becoming unreadable, but sidecar labels can damage flow when they force readers to jump from a row to a separate mini-ledger for ordinary qualifications. Row sidecars should stay rare because they split one thought across two containers.
    Correction route: Prefer row-owned records when the sidecar needs proof, update, removal, or more than a short note. Prefer a consequence sentence after the table when the qualification governs the whole table. Use row sidecars only for short, stable notes that would otherwise bloat every row.
    Outcome: MERGE.

Finding 8
    Task-list target: `Promote source-scan record principles without prompt wave choreography`.
    Prior finding extended: `track-repo-markdown/01-claude-markdown-patterns.md`.
    Critique: The durable report fields are good, but report-shaped language should not leak into active standards prose. Words such as `Decision`, `Correction`, `Ripple files`, and `Proof gap` belong in reports, task lists, and proof records. They should not become the default texture of a standard's explanatory paragraphs.
    Correction route: Keep report packets in `_reports/AGENTS.md` and task-output surfaces. Active standards can use the same reasoning internally but should publish the resulting rule in direct prose, compact records, or examples.
    Outcome: MERGE.

## [EVIDENCE]

Active standards:
- `style-guide.md` permits isolated sentences only for specific reader jobs and rejects sentence-to-label conversion when the sentence remains a complete thought.
- `information-structure.md` already says sentence islands must introduce the next container, close the previous one, or become structured data.
- requires controlling content at unit edges but still routes sentence craft to `style-guide.md`.

Cited reports:
- `track-information-structure/01-information-structure-container-toolbelt.md` identifies real missing carriers: command packets, field packets, type-shape blocks, decomposition recipes, co-location tests, and alert demotion.
- `track-repo-markdown/02-non-standards-markdown-patterns.md` supplies repo evidence for command clusters, topology-status bundles, row sidecars, source-proof packets, and isolated sentence packets.
- `track-external-research/03-information-architecture-structure-chooser-a.md` and `04-information-architecture-structure-chooser-b.md` support input-first choice, arity rules, and representation job tests while warning against overlarge selectors.
- `track-type-corpus/01-explanation-type-corpus.md` says many isolated sentences in explanation standards are valid leads, text equivalents, rejected-example reasons, route boundaries, or consequences.
- `track-type-corpus/02-reference-type-corpus.md` flags the README late-example issue as a decision point, not an automatic move-all-examples rule.

## [RECOMMENDATIONS]

Promote these refinements:
- Add a `KEEP_OR_CONVERT` prose test beside paragraph architecture. It should distinguish lead, consequence, caption, field line, alert, note, footnote, and row-owned record without forcing all short sentences into fields.
- Add an anti-formulaic guard to `information-structure.md`: choose the smallest carrier that preserves the reader job, and collapse packets when fields are not independently scanned, updated, or proved.
- Keep the input-first chooser compact. Route uncommon forms to subchoosers rather than putting every packet name in the first table.
- Require examples beside rules by default, but allow a late combined-example section when the lead names the combined route and the examples teach rule interaction.
- Use job-split captions only when adjacent representations could duplicate the same fact set.

Hold or narrow these tasks:
- Hold blanket relocation of README examples until the README standard distinguishes local misuse examples from combined route examples.
- Hold any rule that makes row sidecars a default alternative to row-owned records.
- Narrow contrast-record promotion so `Accepted`, `Rejected`, and `Reason` appear only when the rejected form is plausible or observed.

## [CANDIDATE_WORDING]

Candidate style-guide wording:

```markdown template
[KEEP_OR_CONVERT]:
- Keep a standalone sentence when it leads, transitions, captions, closes, states a consequence, names a route boundary, or marks a proof gap.
- Convert it to `Label: value` only when the value is scanned, quoted, or updated independently.
- Convert it to a note, footnote, alert, or row-owned record only when its scope is local to a row, claim, warning, or provenance point.
- Delete or fold it when it only repeats emphasis.
```

Candidate form-standard wording:

```markdown template
[FORMULAIC_PACKET_CHECK]:
- Use a packet when 3 or more fields carry independent scanning, proof, update, failure-reading, or omission value.
- Collapse to prose or a short definition block when the fields are only labels for one sentence.
- Omit absent fields; never fill a packet to satisfy a template.
```

## [PROOF_GAPS]

- No active standards were edited in this pass.
- No Markdown link, anchor, renderer, or docs-build gate ran because the requested output is a report with no changed links, headings outside the new report, or diagrams.
- The critique uses source-report evidence and active-standard text; it does not re-run every repo sample line cited by the source reports.
