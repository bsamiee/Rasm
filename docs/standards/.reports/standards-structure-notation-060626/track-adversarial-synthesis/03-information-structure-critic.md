Question: Do the proposed `information-structure.md` changes make a dense carrier-selection toolbelt, or do they create table spam, broad packet catalogs, duplicated type-standard semantics, and unclear owner boundaries?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: docs/standards/information-structure.md :: carrier toolbelt versus packet catalog sprawl :: correct
Target owner: docs/standards/information-structure.md
Source basis: `CLAUDE.md`; root `AGENTS.md`; `docs/standards/README.md`; `docs/standards/AGENTS.md`; `.reports/AGENTS.md`; session `README.md`; `track-synthesis/00-collective-task-list.md`; `docs/standards/information-structure.md`; `style-guide.md`; `formatting.md`; `proof.md`; `track-information-structure/01-information-structure-container-toolbelt.md`; `track-information-structure/02-information-structure-cross-type-fit.md`; `track-external-research/03-information-architecture-structure-chooser-a.md`; `04-information-architecture-structure-chooser-b.md`; `05-tables-matrices-decision-records.md`
Promotion target: docs/standards/information-structure.md
Outcome: CORRECT

Prior lane extension: corrects the information-structure lane by preserving its input-first chooser direction while rejecting the parts that would turn the file into a broad packet catalog or a second proof/type-standard registry.

## [FINDINGS]

### [1][KEEP_THE_TOOLBELT_DROP_THE_CATALOG]

Finding: The proposed `INFORMATION_SIGNATURE_CHOOSER` is the right top-level correction, but several downstream tasks inflate it into a catalog of every shape in the corpus.

Evidence: The active `information-structure.md` already has a reader-question chooser, table chooser, visual chooser, record rules, code-block rules, and page anatomy. The collective task list then proposes input signatures, closed vocabulary cards, ordered records, contrast records, command/output surfaces, field packets, API field cards, CLI envelopes, type-shape blocks, edge lists, topology bundles, table decomposition packets, generated-ledger packets, and page-anatomy splits.

Risk: If every named shape becomes a peer top-level section, the standard stops being a chooser and becomes a shopping list. Agents will select a fashionable packet name instead of asking the decisive question: what job does this information need to do?

Correction task: Add one compact input-first chooser, then route broad rows to a small number of subchoosers. Do not add a mega-table or a flat catalog of packet names.

Exact correction shape:

```markdown template
[INFORMATION_SIGNATURES]:
- One claim, caveat, consequence, or route boundary -> prose paragraph.
- Peer facts or unordered requirements -> bullets.
- One item with scannable fields -> definition block.
- Repeated homogeneous items with short shared fields -> table.
- Trackable items with status, dependency, proof, or update triggers -> structured records.
- Ordered action with per-step fields -> ordered records.
- One key to one owner, class, value, or behavior -> lookup table profile.
- Independent conditions to one action -> decision table with hit policy.
- Callable behavior or parsed output -> command/output subchooser.
- Relation, topology, branch, sequence, lifecycle, or boundary crossing -> topology subchooser.
- Literal source, config, command, output, or source-shaped example -> code fence with language and intent.
- Drift-prone claim -> proof record using `proof.md` field semantics.
```

Rule to tighten: each chooser row must answer one author input signature and either choose one carrier or name one subchooser. Rows that only list possible future packets are rejected.

Outcome: PROMOTE with correction.

### [2][MERGE_CONTROL_RECORDS]

Finding: Closed vocabulary cards, profile selectors, section-cardinality blocks, authoring contracts, field packets, and contrast records are all control records with different local fields, not separate standard families.

Evidence: `track-information-structure/02-information-structure-cross-type-fit.md` identifies authoring contracts, profile selectors, section cardinality, status vocabularies, API field cards, command cards, response profiles, and checkpoint records as gaps. The report's conclusion is right that these need shared carrier choice, but the proposed task list risks promoting each as its own schema.

Risk: Separate schemas duplicate type-standard semantics and make `information-structure.md` decide domain vocabulary, publication state, support state, API semantics, and response behavior. Those meanings belong to type standards and proof owners.

Correction task: Add one `CONTROL_RECORDS` subchooser. It should name when a definition block, grouped definition block, subsection-per-record block, ordered record, or contrast record is the carrier. It should not define every domain field.

Exact correction shape:

```markdown template
[CONTROL_RECORDS]:
- Authoring contract: definition block or bullets at the top of a type standard; field names stay local.
- Profile selector: definition block when one profile controls obligations; table only when profiles are compared across shared attributes.
- Closed vocabulary: vocabulary card before first use; meanings stay in the owning type standard.
- Section cardinality: compact grouped definition block; no table unless several document profiles compare the same sections.
- Field packet: definition block for one item's fields; subsection-per-record when fields need lists, code, proof sub-blocks, or independent updates.
- Contrast record: `Accepted`, `Rejected` or `Near miss`, then `Reason`; use only to prevent a likely wrong author action.
```

Tasks to merge: `Promote closed vocabulary cards`; `Promote field packet, proof record, API field card, and CLI envelope schemas`; `Move contrast-record form ownership to information structure`; the profile/authoring/cardinality gaps from `track-information-structure/02`.

Tasks to hold: user-choice details for lowercase/display-value status handling. The shared card can include `Display value` or `Machine value` only as optional carrier fields; exact state semantics remain type-local.

Outcome: MERGE.

### [3][COMMAND_OUTPUT_NEEDS_ONE_SUBCHOOSER]

Finding: Command cards, command-step records, CLI envelope records, output records, API field cards, failure-reading tables, and command inventories should not become independent peer packets.

Evidence: The task list separately proposes command/output surfaces, callable contract packets, CLI envelope records, API field cards, generated ledgers, and command behavior proof. The external reports agree that command truth remains in tool READMEs, API standards, or local proof; `information-structure.md` only chooses the carrier.

Risk: A broad command packet catalog will encourage authors to create one record for every command even when a lookup table or one inline command field is enough. It will also tempt `information-structure.md` to assert behavior proof that belongs in `proof.md`.

Correction task: Add one `COMMAND_OUTPUT_SURFACES` subchooser with table-versus-record thresholds.

Exact correction shape:

```markdown template
[COMMAND_OUTPUT_SURFACES]:
- Inline command field: one short invocation inside a step or record.
- `bash copy-safe` fence: command line breaks, quoting, or copyability matter.
- Command inventory table: several commands compared by purpose, owner, precondition, or output.
- Command card: one reusable command family needs precondition, effect, expected signal, failure route, and proof route.
- CLI envelope record: stdout, stderr, exit status, artifacts, side effects, and failure reading are parsed independently.
- Failure-reading table: observed signal selects the next action.
```

Boundary rule: packet fields may name `Proof route`, but proof labels, freshness, command evidence, generated output, and unrun checks route to `proof.md`.

Tasks to merge: `Define command/output surfaces and callable contract packets`; `Promote field packet, proof record, API field card, and CLI envelope schemas`; `Add generated-ledger packet` for command-generated output surfaces only.

Tasks to hold: any local command behavior claim until the owning README, source file, generated contract, or executed command proves it.

Outcome: MERGE.

### [4][TABLE_RULES_NEED_A_SPAM_GUARD]

Finding: The large lookup, matrix, gate-matrix, row-sidecar, and decision-table tasks are useful only if they strengthen table rejection and decomposition. They should not add new table types for every row shape.

Evidence: `information-structure.md` already rejects one-row tables, prose columns, malformed GFM tables, oversized rows/columns, and nested table facts. External report `05-tables-matrices-decision-records.md` supports a two-dimensional lookup/comparison test, row-owned records for independent proof/lifecycle facts, and a lightweight hit policy.

Risk: Adding classification tables, gate matrices, package/API matrix profiles, row sidecars, and support bundles as peer forms will normalize table spam. Authors will add tables to save vertical space rather than because the reader compares rows and columns.

Correction task: Tighten the table eligibility rule before adding more table profiles.

Exact correction shape:

```markdown template
[TABLE_SPAM_TEST]:
- Use a table only for two-dimensional lookup or comparison across homogeneous rows.
- Reject a table used only to save vertical space, align prose, list one-dimensional facts, or display one record.
- Keep a table while every row shares one schema and cells stay atomic.
- Use grouped definition records when fields become sparse or independently maintained.
- Use subsection-per-record blocks when any row needs lists, code fences, proof sub-blocks, migration detail, replacement detail, source-model fields, or multiple update/removal fields.
- Use summary-plus-detail when the full row set is too large but a short retrieval table can point to detail records or sibling tables.
```

Tasks to merge: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules`; `Add lightweight decision-table hit policy`; the `classification table` proposals.

Tasks to drop: standalone `classification table` as a named peer form. Fold it into `lookup table profile` unless a later active corpus edit proves a distinct carrier is necessary.

Tasks to hold: row-sidecar label syntax. Until the user chooses sidecars versus footnotes, use row-owned records for proof, lifecycle, update, and removal facts.

Outcome: CORRECT.

### [5][TOPOLOGY_SHAPES_NEED_JOB_BOUNDARIES]

Finding: Codemap trees, type-shape blocks, edge lists, Mermaid diagrams, C4/topology packets, topology-status bundles, representation packets, and text equivalents need a job test, not a larger visual catalog.

Evidence: The current visual sections already name codemap trees, type-shape blocks, edge lists, matrices, Mermaid, and C4 or topology packets. The reports correctly ask for examples and upgrade rules, but `topology-status bundle` blurs the boundary between diagram topology and status/proof records.

Risk: A bundled topology/status packet duplicates architecture-type semantics and proof-record semantics. It also encourages publishing both a diagram and a table that carry the same facts.

Correction task: Add a `TOPOLOGY_STRUCTURE_CHOOSER` plus `REPRESENTATION_JOB_TEST`. Keep architecture-specific C4 scope in `explanation/architecture.md`; keep proof/status/update facts in records or tables.

Exact correction shape:

```markdown template
[TOPOLOGY_STRUCTURE_CHOOSER]:
- Codemap tree: path ownership, package shape, or generated-output placement.
- Type-shape block: compact relationship between records, unions, interfaces, carriers, or data flow.
- Edge list: tiny directed dependency set where rendering adds no information.
- Matrix: relationship intersections across 2 axes.
- Mermaid: branch, sequence, lifecycle, dependency, or topology that would require recall if left in prose.

[REPRESENTATION_JOB_TEST]:
- Name what each representation carries before publishing both.
- Diagrams carry topology, sequence, branch/rejoin shape, dependency shape, or lifecycle transition shape.
- Records and tables carry source, status, proof, update, review, and removal facts.
- Delete the second representation if removing it loses no unique reader action.
```

Tasks to merge: `Add type-shape block, edge list, codemap tree, and topology-status bundle guidance`; `Promote representation co-location and alert demotion tests`; `Add text-graphic and ASCII fallback rules` only for the representation job and fallback boundary.

Tasks to drop: `topology-status bundle` as a named carrier. Replace it with a topology representation plus adjacent status/proof record when both are needed.

Outcome: CORRECT.

### [6][MACHINE_CONSUMED_MARKDOWN_IS_AN_EARLY_EXCEPTION]

Finding: Moving machine-consumed Markdown earlier is necessary, but generated ledgers and parser-owned ledgers should not become ordinary packet templates.

Evidence: The current machine-consumed rule sits inside callouts/collapsible/footnotes, after ordinary cleanup rules. The reports cite analyzer release ledgers, generated references, line-oriented markers, and parser-sensitive tables as cases that must resist normal prettification.

Risk: If generated-ledger packets are treated as ordinary human-facing records, agents may hand-maintain generated rows, normalize parser-required fields, or duplicate proof labels into generated surfaces.

Correction task: Promote an early `MACHINE_CONSUMED_MARKDOWN` exception with a no-normalize rule and proof route.

Exact correction shape:

```markdown template
[MACHINE_CONSUMED_MARKDOWN]:
- Consumer: <parser, analyzer, generator, release ledger, or retrieval tool>
- Parsed shape: <required headings, fields, rows, fences, or order>
- Source of truth: <source file, generator, contract, or proof gap>
- Validation command: <exact command, or proof gap>
- No-normalize rule: ordinary container cleanup stops unless the consumer changes.
- Review trigger: <parser, generator, ledger, or schema change>
```

Tasks to merge: `Move machine-consumed Markdown earlier and name parser-owned exceptions`; `Add generated-ledger packet`.

Tasks to hold: any Roslyn, parser, or generator-specific field order until current local source or maintained docs prove the consumed shape.

Outcome: PROMOTE with boundary.

### [7][PAGE_ANATOMY_SPLIT_IS_VALID_BUT_NOT_A_PACKET]

Finding: Splitting page anatomy into standard-file anatomy, type-standard opening, and produced-document skeleton is correct. It should not add another record packet family.

Evidence: Current `[17][PAGE_ANATOMY]` mixes shared-standard page shape, type-standard opening contract order, and produced-document skeleton examples. The task list and external research both identify this as an ambiguity.

Risk: If the split becomes a schema registry, type standards will duplicate page skeletons and `information-structure.md` will prescribe artifact-specific sections that belong in type standards.

Correction task: Split the section by job and route produced-document skeletons back to type standards.

Exact correction shape:

```markdown template
## [N][PAGE_ANATOMY]

[STANDARD_FILE_ANATOMY]:
- Lead, `Use when`, rules, `Boundaries`, and `Validation` are required.
- `Examples` appears only where misuse is likely.

[TYPE_STANDARD_OPENING]:
- Lead, `Use when`, route-away, agent use, required produced structure, section cardinality, adjacent checks, and maintenance triggers appear before taxonomies or examples.

[PRODUCED_DOCUMENT_SKELETON]:
- The active type standard owns the skeleton.
- This standard only requires conditional sections to be separated from the base template and omitted when absent.
```

Tasks to merge: `Split page anatomy into three jobs`.

Tasks to hold: converting every type standard's `[AUTHORING_CONTRACT]` into a new shared contract record until the shared control-record rule lands.

Outcome: PROMOTE.

### [8][MISSING_TASKS]

Missing task 1: Add a packet-promotion threshold.

Correction shape:

```markdown template
[PACKET_PROMOTION_TEST]:
- Promote a named carrier only when 2 or more active standards use the same structural job, or 1 parser, generator, or machine consumer requires exact shape.
- Otherwise keep the case as an example under an existing carrier.
- Do not add a packet that only renames a type-standard field set.
```

Missing task 2: Add an owner-boundary paragraph before subchoosers.

Correction shape:

```markdown template
This standard owns carrier choice, carrier escalation, decomposition thresholds, and minimum human-facing carrier fields. It does not own proof label meanings, marker spelling, command behavior, API semantics, support lifecycle states, or type-local publication/status vocabularies.
```

Missing task 3: Add a no-table-for-packet-catalog rule.

Correction shape:

```markdown template
Do not use a table merely to list every possible carrier. Use a grouped chooser when rows need prose, examples, or route-away notes. Use a table only when the rows are homogeneous and the reader compares the same columns.
```

Missing task 4: Add a subchooser placement rule.

Correction shape:

```markdown template
Place a subchooser beside the first top-level chooser row that routes to it. Do not collect unrelated packets in a late catalogue section.
```

Missing task 5: Add an active-corpus shape scan before promotion.

Correction shape:

```bash copy-safe
rg -n "AUTHORING_CONTRACT|SECTION_CARDINALITY|Accepted:|Rejected:|Near miss:|Command:|Expected|Proof gap:|Source of truth:|Availability:|Execution:|Profile:" docs/standards --glob '*.md' --glob '!.reports/**'
```

Use the scan as source-shape evidence only. It does not prove rendered Markdown, command behavior, or links.

## [RECOMMENDATIONS]

[PROMOTE]:
- `INFORMATION_SIGNATURE_CHOOSER`, but as a grouped chooser with short rows and named subchoosers, not a wide table.
- `CONTROL_RECORDS`, `ORDERED_RECORDS`, `COMMAND_OUTPUT_SURFACES`, `TABLE_SPAM_TEST`, `TOPOLOGY_STRUCTURE_CHOOSER`, `REPRESENTATION_JOB_TEST`, early `MACHINE_CONSUMED_MARKDOWN`, and the 3-way `PAGE_ANATOMY` split.

[MERGE]:
- Merge vocabulary cards, profile selectors, section cardinality, field packets, and contrast records into `CONTROL_RECORDS`.
- Merge command cards, command steps, CLI envelopes, output records, API field cards, generated-ledger needs, and failure-reading tables into `COMMAND_OUTPUT_SURFACES`.
- Merge large lookup, matrix, gate matrix, row-owned detail, and hit-policy work into table decomposition and decision-table subsections.
- Merge topology-status bundle work into representation job tests plus topology chooser.

[DROP]:
- Drop standalone `classification table` unless later proof shows lookup-table profiles cannot carry it.
- Drop `topology-status bundle` as a carrier name.
- Drop any late broad packet catalogue that only lists possible structures without selection rules.

[HOLD]:
- Hold row-sidecar notation until the user chooses sidecars, footnotes, or row-owned records.
- Hold compact glyph semantics in formatting until the glyph user choice resolves.
- Hold lowercase/display-value status semantics as type-local unless a shared carrier field is enough.
- Hold parser, Roslyn, generator, or command-behavior specifics until local source, maintained docs, generated contracts, or executed commands prove them.

## [CANDIDATE_WORDING]

Candidate boundary lead for the revised `information-structure.md` chooser:

```markdown template
Choose the carrier by the information's job before drafting prose. This standard owns carrier choice, escalation, decomposition, and minimum human-facing carrier fields. It routes proof label meanings to `proof.md`, marker spelling to `formatting.md`, words to `style-guide.md`, document-type skeletons to type standards, and current command or API behavior to the source that owns the behavior.
```

Candidate compact subchooser rule:

```markdown template
Use a subchooser only when one top-level signature has several legitimate carriers. The subchooser must start with the deciding input condition and end with a rejection rule. Do not create a peer packet section for a case that can be a row in an existing subchooser.
```

Candidate anti-spam close:

```markdown template
A carrier is valid only when deleting or replacing it would lose a reader action: lookup, comparison, ordered execution, proof refresh, parser compatibility, topology recognition, or independent maintenance. If the carrier only makes the page look more structured, remove it.
```

## [PROOF_GAPS]

- No active standards were edited in this pass.
- No renderer, link checker, Markdown table validator, docs build, Mermaid render, command run, parser validation, or provider source check was performed.
- `git diff --check -- .reports/standards-structure-notation-060626/track-adversarial-synthesis/03-information-structure-critic.md` passed after this report was created.
