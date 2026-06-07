Question: Which current or stable external sources should shape Rasm rules for lookup tables, decision tables, support matrices, row-owned details, table width, escaping, and table-to-record splits?
Type: standards-research
Lane: track-external-research
Merge key: docs/standards/information-structure.md :: tables matrices decision records :: promote source-backed decomposition rules
Target owner: `docs/standards/information-structure.md`, with routed findings for `docs/standards/formatting.md`, `docs/standards/reference/support-matrix.md`, `docs/standards/reference/reference.md`, and `docs/standards/explanation/roadmap.md`
Source basis: mandatory standards reads; prior lane report `track-external-research/01-gfm-github-markdown-capabilities.md`; official GitHub/GFM, Google, Microsoft, GitLab, W3C WAI, OMG DMN, Camunda, Kubernetes, and Node.js sources accessed 2026-06-06
Promotion target: `docs/standards/information-structure.md`
Outcome: PROMOTE

## [FINDINGS]

### [F1][TABLE_ELIGIBILITY]

Active owner/section: `docs/standards/information-structure.md` `[2][CONTAINER_CHOOSER]`, `[3][TABLES]`, `[10][DECISION_LOOKUP_TABLES]`; `docs/standards/reference/reference.md` `[5][FACT_ENTRIES]`.
External evidence: Google developer documentation style guide says lists and tables are both for similarly structured items, but single-unit items should be lists, term-definition pairs should usually be description lists, and three-or-more related data pieces belong in a table. Google also rejects one-column tables, layout tables, code-snippet layout tables, long one-dimensional lists split into table columns, and tables inside numbered procedures. Microsoft says tables are for two or more rows plus headers and two or more columns, and not for merely similar list items.
Rasm fit: The active Rasm rule already says a single record read by field belongs in a definition block, sparse shared data can stay tabular, and no-comparison row sets should become records. External sources support that direction and make the threshold easier to explain by input shape: one item -> record/list; one-dimensional set -> list; two-dimensional comparable data -> table.
Decision: PROMOTE. Add source-backed wording to the information-signature chooser: a table is justified by two-dimensional lookup or comparison, not by saving vertical space. Keep lookup tables for direct key-to-value maps; keep decision tables for finite condition combinations; route term-definition pairs and one-row tables to definition blocks unless a type standard requires consistent reference layout.
Proof gap: None for the source-backed design rule. Exact numeric Rasm ceilings remain local policy, not externally sourced thresholds.

### [F2][MARKDOWN_TABLE_SAFETY]

Active owner/section: `docs/standards/formatting.md` `[4][TABLE_STYLING]`; `docs/standards/information-structure.md` `[3][TABLES]`; `docs/standards/proof.md` renderer-support proof packet.
External evidence: The GFM spec defines tables as a leaf block with one header row, one delimiter row, and data rows. It allows inline content inside cells but not block-level elements; literal pipes inside cells must be escaped; header and delimiter rows must have the same cell count; later rows with too few cells get empty cells and rows with too many cells have excess cells ignored. GitHub Docs says tables use pipes and hyphens, require a blank line before the table for correct rendering, support left/center/right alignment markers, and escape literal pipes with backslash.
Rasm fit: `formatting.md` already requires escaped pipes, single-line cells, and alignment by semantic type. The GFM detail that excess body cells can be ignored is stronger risk evidence: malformed rows can silently produce a plausible but wrong table.
Decision: PROMOTE. Strengthen validation wording around escaped-pipe-aware cell counting: table checks must compare header/delimiter cells and data rows after escaped pipes are accounted for; a mismatch is not a cosmetic issue because a renderer can insert empty cells or drop excess cells.
Proof gap: Local automated table validation is not proved. If promoted wording claims a checker, `proof.md` needs tool-output proof; otherwise keep this as an author self-check.

### [F3][WIDTH_AND_CELL_COMPLEXITY]

Active owner/section: `docs/standards/information-structure.md` `[3][TABLES]`, `[4][TABLE_CONTENT_DISCIPLINE]`; `docs/standards/formatting.md` `[4][TABLE_STYLING]`.
External evidence: Microsoft says to keep responsive design in mind, limit columns, keep cell text brief, and ideally keep entries to one line. GitLab says a `Description` column should be rightmost where possible, gives special handling for very wide tables, and says table-wide realignment can make diffs hard to review. Google says not to merge cells with `colspan` or `rowspan`, and W3C WAI says complex tables with multi-level headers require explicit header associations.
Rasm fit: Rasm already sets local ceilings of 15 columns, 20 rows, at most one prose column, and 8-word cells. External sources do not supply those exact numbers, but they strongly support the underlying constraints: brief cells, left row identity, rightmost description/prose, no merged cells, no complex headers in simple Markdown, and maintenance-aware formatting.
Decision: MERGE. Keep Rasm numeric ceilings as local policy, but cite external support for the qualitative triggers. Add "complex-header or merged-cell need" as a split trigger: because GFM cannot encode accessible multi-level relationships, split into sibling tables, a matrix with explicit stub labels, or row-owned records instead of simulating spans.
Proof gap: None for the qualitative rule. Numeric limits remain repo convention and should not be described as a standard-library or provider limit.

### [F4][ROW_OWNED_DETAILS]

Active owner/section: `docs/standards/information-structure.md` `[5][TABLES_PROSE]`, `[6][STRUCTURED_RECORDS]`; `docs/standards/reference/support-matrix.md` `[8][MATRIX]`; `docs/standards/reference/reference.md` `[5][FACT_ENTRIES]`.
External evidence: Google requires a complete introductory sentence for tables because screen readers may not preannounce them, places footnotes immediately after a table when needed, and advises avoiding footnotes when possible. GitLab says to avoid empty cells where possible and keep tables accessible and scannable. Microsoft says row-identifying information belongs in the leftmost column, entries should be parallel, and cells should not be blank.
Rasm fit: Rasm's active standards already use visible notes after tables, proof-bearing fact cards, support-matrix row records, and adjacent-document relation records. The missing part is a source-backed row-sidecar decision: when a note qualifies the whole table, it can be a visible note; when a detail changes one row's source, proof, update trigger, replacement, migration, or removal behavior, it belongs to that row as a record.
Decision: PROMOTE. Define row-owned details as record promotion triggers, not decorative sidecars. A table note may qualify the whole table or a small set of cells; row-owned facts with independent proof or lifecycle fields must become a per-row definition block or subsection record. Preserve the open user-choice item about `[GROUP] [INDEX]` sidecars versus footnotes versus row-owned records; this report recommends row-owned records for durable proof and lifecycle, but does not choose the notation.
Proof gap: User choice remains open for sidecar label syntax in `track-synthesis/00-collective-task-list.md`.

### [F5][DECISION_TABLE_HIT_POLICY]

Active owner/section: `docs/standards/information-structure.md` `[10][DECISION_LOOKUP_TABLES]`; `docs/standards/formatting.md` table-marker notation only if a compact hit-policy label is added.
External evidence: OMG publishes DMN as the standard for precise decision and business-rule specification; its current page lists DMN 1.6 beta and formal 1.5, and states beta is informational while formal versions are followed for compliance. Camunda/CIB seven DMN docs state that a decision table hit policy specifies which matching rules are included in the result, with `Unique`, `Any`, and `First` returning at most one satisfied rule and `Rule Order` and `Collect` returning multiple. Camunda best-practice docs distinguish `Unique`, `First`, `Priority`, `Any`, aggregated `Collect`, `Collect`, `Rule order`, and `Output order`, and note that hit policy affects readability and maintainability.
Rasm fit: Rasm already requires a hit policy before overlapping decision-table rows and rejects nondeterministic policy. External DMN sources support making hit policy a first-class field, but Rasm should not import the full DMN vocabulary unless a produced document is actually modeling executable DMN.
Decision: PROMOTE. Keep a local lightweight hit-policy vocabulary for documentation decision tables: `unique` for disjoint rows, `first match wins` only when row order is the rule, `most specific wins` only when wildcard count defines priority, and `all matching actions apply` only when actions compose. Add a route note: executable/business-rule DMN tables must use DMN source vocabulary and evidence, not the local shorthand.
Proof gap: The report used official OMG current-version status plus Camunda implementation guidance; exact normative DMN PDF text was not extracted. If Rasm later claims DMN compliance, cite the formal OMG specification directly, not only implementation docs.

### [F6][SUPPORT_MATRIX_SOURCE_MODEL]

Active owner/section: `docs/standards/reference/support-matrix.md` `[5][LIFECYCLE_BASELINES]`, `[8][MATRIX]`, `[9][READING_RULE]`, `[10][COMPATIBILITY_BOUNDS]`; `docs/standards/explanation/roadmap.md` `[16][BOUNDARIES]`.
External evidence: Kubernetes' current version-skew policy names the support question, supported versions, component-specific skew rules, narrowing notes, and upgrade order; it states the release branches currently maintained and names the Release Managers group as the decision owner. Node.js release documentation separates release status from dates and explains that LTS usually guarantees critical bug fixes over a defined window while production applications should use Active LTS or Maintenance LTS releases.
Rasm fit: `support-matrix.md` already requires scope, support regime, source model, status vocabulary, matrix, lifecycle dates, reading rule, compatibility bounds, migration anchors, and roadmap/how-to/runbook route-away. Kubernetes and Node show why those fields are not metadata spam: support claims need source phase, grant class, scope, owner, and update trigger because support policy changes over time.
Decision: PROMOTE. Use support matrices only when support policy is the reader question. A matrix row should carry status, scope/version, condition/bound, key date when applicable, and basis; detailed fix classes, support channel, source phase, upgrade order, and migration detail move to row-owned records or adjacent roadmap/how-to routes. Upgrade order belongs to roadmap/how-to when it becomes a sequence; the support matrix may link it but should not embed the procedure.
Proof gap: Current Rasm support examples still need local source proof when they name live versions, lifecycle dates, or source-field names.

### [F7][MATRIX_VS_RECORD_SPLIT]

Active owner/section: `docs/standards/information-structure.md` `[3][TABLES]`, `[6][STRUCTURED_RECORDS]`, `[10][DECISION_LOOKUP_TABLES]`; `docs/standards/reference/reference.md` `[7][KEYED_MAPPINGS]`; `docs/standards/reference/support-matrix.md` `[8][MATRIX]`.
External evidence: W3C WAI distinguishes simple tables from complex tables requiring explicit header associations; Google rejects merged cells and layout tables; GitLab warns about wide tables and makes the description column rightmost; Microsoft says row identity belongs left and cells should be parallel and brief.
Rasm fit: This maps cleanly to a split rule: tables are for homogeneous rows with one shared question; records are for items whose proof, status, update trigger, source model, migration, replacement, exception, or nested fields differ independently.
Decision: PROMOTE. Add a compact table-to-record split test: keep a table while rows share one schema and cells remain atomic; split to grouped definition records when fields are sparse or independently updated; split to subsection-per-record blocks when any row needs lists, code fences, proof sub-blocks, migration explanation, or multiple update/removal fields. For large homogeneous tables, use summary-plus-detail with the summary table as retrieval and details as records or sibling tables.
Proof gap: None for the design rule. Active promotion should still review examples across reference, support matrix, and roadmap before selecting final candidate wording.

## [EVIDENCE]

[LOCAL_READ_SET]:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `.reports/AGENTS.md`
- `.reports/standards-structure-notation-060626/README.md`
- `docs/standards/information-structure.md`
- `docs/standards/formatting.md`
- `docs/standards/reference/support-matrix.md`
- `docs/standards/reference/reference.md`
- `docs/standards/explanation/roadmap.md`
- `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
- `.reports/standards-structure-notation-060626/track-external-research/01-gfm-github-markdown-capabilities.md`
- `.reports/standards-structure-notation-060626/track-information-structure/01-information-structure-container-toolbelt.md`
- `.reports/standards-structure-notation-060626/track-type-corpus/02-reference-type-corpus.md`

[EXTERNAL_SOURCE_SET_ACCESSED_2026_06_06]:
- GitHub Flavored Markdown Spec: https://github.github.com/gfm/. Used for GFM table grammar, inline-only cells, escaped pipes, alignment delimiters, header/delimiter cell-count matching, and body-row mismatch behavior.
- GitHub Docs, "Organizing information with tables": https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-tables. Used for GitHub table creation, blank-line-before-table guidance, alignment markers, inline formatting, and escaped pipes.
- Google developer documentation style guide, "Tables": https://developers.google.cn/style/tables. Used for list-versus-table selection, one-row and one-column table rejection, no layout/code-snippet/list-column tables, introductory sentences, placement, footnote placement, no styling-only headers, no merged cells, and logical row ordering.
- Microsoft Style Guide, "Tables": https://learn.microsoft.com/en-us/style-guide/scannable-content/tables. Used for table purpose, leftmost row identity, parallel entries, no blank cells, responsive design, brief cells, precise headers, and sentence-style capitalization.
- GitLab Docs, "Documentation Style Guide - Tables": https://docs.gitlab.com/development/documentation/styleguide/. Used for matrix suitability, avoiding empty cells, rightmost description column, large-table formatting, header/delimiter row length, wide-table maintenance, and diff-aware realignment.
- W3C WAI, "Tables Tutorial": https://w3c.github.io/wai-website/tutorials/tables/. Used for data-grid table accessibility, header/data cell relationships, complex-header requirements, captions/summaries, no layout tables, and visual-cue insufficiency.
- OMG, "Decision Model and Notation": https://www.omg.org/spec/DMN/. Used for current DMN version/source status and the beta-versus-formal compliance distinction.
- CIB seven / Camunda DMN hit policy documentation: https://docs.cibseven.org/manual/2.0/reference/dmn/decision-table/hit-policy/. Used for hit-policy role, single-hit versus multiple-hit behavior, default `UNIQUE`, rule order, and collect behavior.
- Camunda 8 best practice, "Choosing the DMN hit policy": https://unsupported.docs.camunda.io/8.2/docs/components/best-practices/modeling/choosing-the-dmn-hit-policy/. Used for hit-policy readability/maintainability consequences and single-result versus multiple-result distinction.
- Kubernetes, "Version Skew Policy": https://kubernetes.io/releases/version-skew-policy/. Used as a current support-policy example with support scope, component-specific bounds, narrowing notes, support branch policy, decision owner, and upgrade-order route.
- Node.js, "Node.js Releases": https://nodejs.org/en/about/releases/. Used as a current lifecycle example that separates release status, LTS meaning, production-use guidance, and branch status/date table.

## [RECOMMENDATIONS]

[PROMOTE]:
- Add source-backed table eligibility wording to `information-structure.md`: table only for two-dimensional lookup/comparison across homogeneous rows; records/lists for one-dimensional, one-row, or independently updated facts.
- Add a table-to-record split packet near the current table ceilings: `Keep as table`, `Summary-plus-detail`, `Grouped definition records`, `Subsection-per-record`, and `Route-away`.
- Add a decision-table hit-policy note that keeps Rasm's lightweight policies local and routes real DMN compliance claims to OMG/DMN proof.
- Add support-matrix wording that separates support policy rows from upgrade-order procedures and future sequence.

[MERGE]:
- Merge GFM/GitHub parser safety with the prior renderer-capability report instead of duplicating the renderer proof packet.
- Merge qualitative width/accessibility support into Rasm's existing local row/column/cell ceilings without implying the numbers are external standards.
- Merge row-owned-detail guidance with the open row-sidecar user-choice item; do not choose notation in this report.

[HOLD]:
- Hold exact lifecycle field-name claims such as imported API fields until a current source schema proves them.
- Hold automated table-validation claims until a local checker or command output proves the configured gate.

## [CANDIDATE_WORDING]

Candidate merge wording for `docs/standards/information-structure.md`:

```markdown template
[TABLE_TO_RECORD_SPLIT]:
- Keep a table when rows are homogeneous, the reader scans one shared comparison or lookup question, and cells stay atomic.
- Use summary-plus-detail when the row set is too large but the summary rows share one retrieval axis.
- Use grouped definition records when rows share a subject family but fields become sparse or independently maintained.
- Use subsection-per-record blocks when any row needs lists, code fences, proof sub-blocks, migration detail, replacement detail, source-model fields, or multiple update/removal fields.
- Do not simulate row spans, column spans, nested tables, or multi-level headers in GFM; split the table or move the complex row to a record.
```

Candidate merge wording for `docs/standards/information-structure.md` decision tables:

```markdown template
Hit policy: `<unique | first match wins | most specific wins | all matching actions apply>`
Rule order: `<required only when first match wins>`
Fallback: `<action when no row matches, or proof gap if undefined>`
```

Candidate merge wording for `docs/standards/reference/support-matrix.md`:

```text template
Surface: `<runtime, platform, feature, component, API, or plan>`
Version or scope: `<release line, version range, component pair, entitlement, or environment>`
Status: `<support vocabulary term>`
Condition or bound: `<short support condition, compatibility bound, or lifecycle rule>`
Key date: `<date, not announced, still supported, or n/a>`
Basis: `<source path, generated check, maintained policy, or proof gap>`
Detail: `<row-owned record anchor when proof, source phase, migration, replacement, or update fields exceed cells>`
```

## [PROOF_GAPS]

- Local automated validation for escaped-pipe-aware table cell counting was not found or run.
- The exact OMG DMN normative PDF text was not extracted; DMN compliance claims must cite the formal specification directly.
- No active standards were edited in this pass.
- Row-sidecar notation remains a user-choice item in the collective task list.
