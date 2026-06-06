Question: How should `docs/standards/information-structure.md` become the practical answer to "I have this information; what structure should carry it?"
Type: repo-scan
Lane: track-information-structure
Merge key: docs/standards/information-structure.md :: container-selection toolbelt :: promote
Target owner: docs/standards/information-structure.md
Source basis: mandatory standards reads, current repository Markdown sample, GFM/CommonMark primary docs
Promotion target: docs/standards/information-structure.md
Outcome: PROMOTE

## [FINDINGS]

Finding 1
    File/line/heading: `docs/standards/information-structure.md:15` `## [2][CONTAINER_CHOOSER]`; `docs/standards/information-structure.md:41`; sample `README.md:13`, `README.md:50`, `README.md:103`.
    Evidence snippet: The target says "Choose containers by the reader question" and then lists reader questions, while the root README presents layout lookup, command blocks, and artifact flow as separate shapes.
    Weakness: The chooser is container-first after a short reader-question table. It does not give an input-first triage path for the author holding an unshaped packet such as "paths and purposes", "commands and expected signals", "one lifecycle", "conditional alternatives", or "source truth plus update trigger".
    Proposed correction: Add an early `INFORMATION_SIGNATURE_CHOOSER` before the current container table. Use compact rows such as `single claim -> prose or field`, `peer facts -> bullets`, `one item with fields -> definition block`, `homogeneous items -> table`, `independently updated items -> records`, `ordered operation -> numbered steps or command-step records`, `condition combinations -> decision table`, `sequence or topology -> Mermaid`, `small hierarchy -> monospace tree`, `literal copy surface -> fenced code block`, and `proof-bearing claim -> proof record`.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/README.md` only if its non-type route row for container choice needs a narrower label; `docs/standards/formatting.md` only if new labels require styling.
    Outcome: PROMOTE.
    Proof gap/question: No active edit ran. The candidate chooser should be checked against every active type standard before promotion so it does not restate type-owned structure.

Finding 2
    File/line/heading: `docs/standards/information-structure.md:69` paragraph islands; `docs/standards/style-guide.md:49` paragraph architecture; sample `tools/rhino-bridge/README.md:74`, `tools/rhino-bridge/README.md:82`, `tools/rhino-bridge/README.md:90`, `tools/rhino-bridge/README.md:102`, `tools/rhino-bridge/README.md:108`, `tools/quality/README.md:105`, `tools/quality/README.md:113`.
    Evidence snippet: Bridge and quality READMEs pair command fences with repeated "Expected result" prose and direct-dotnet equivalence bullets.
    Weakness: The standard lists `Command card` and `How-to command step record` later, but it never defines a promoted command-step packet. Authors therefore fall back to prose/fence/prose pairs even when the reusable structure is `Action`, `Command`, `Expected signal`, `Failure route`, and `Proof`.
    Proposed correction: Promote command-step and command-card templates out of the text-representation list into the code-block or task-packet section. Add a chooser rule: if a command is read for operation, use a step record; if it is read as a reusable contract, use a command card; if it is only a literal invocation in a larger record, keep it inline.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/task/how-to.md`, `docs/standards/task/runbook.md`, `docs/standards/proof.md`.
    Outcome: PROMOTE.
    Proof gap/question: Need a later promotion pass to decide whether `Expected signal` belongs to form or proof wording when the signal is a verified local output.

Finding 3
    File/line/heading: `docs/standards/information-structure.md:336` proof record packets; `docs/standards/proof.md:37` proof fields; sample `tools/quality/README.md:212`, `tools/quality/README.md:228`, `tools/rhino-bridge/README.md:155`, `tools/rhino-bridge/README.md:199`, `tools/rhino-bridge/README.md:219`.
    Evidence snippet: The sample docs describe JSON envelope fields, API payload keys, output blocks, and bridge marker kinds using bullets and tables.
    Weakness: `Field packet`, `Proof record`, `CLI envelope record`, and `API field card` are named but not specified. These are practical repo-native packets for structured output contracts, but the current standard leaves authors to infer their field order.
    Proposed correction: Add a `FIELD_PACKET_CHOOSER` with exact minimal schemas: `Field packet` for one record's facts, `CLI envelope record` for stdout/stderr/exit/artifacts/side effects/failure reading, `API field card` for field/source/type/allowed values/omission/proof/drift trigger, and `Proof record` delegating exact evidence-field order to `proof.md`.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/proof.md`, `docs/standards/reference/api.md`, `docs/standards/reference/reference.md`.
    Outcome: PROMOTE.
    Proof gap/question: If a named parser consumes any field order, proof must cite that parser; otherwise the packet should stay human-facing and omit absent fields.

Finding 4
    File/line/heading: `docs/standards/information-structure.md:306` `## [12][MONOSPACE_TEXT]`; `docs/standards/information-structure.md:314`; sample `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:64`, `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:86`, `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:100`, `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:116`.
    Evidence snippet: AppHost architecture contains type shapes as prose, a short bare code block for records, a table of DU cases, and a field list for `ObservabilitySlot`.
    Weakness: The standard names `Type-shape block` and `Edge list` as source structures, but examples only cover file trees, stacked boxes, and small matrices. It does not show how to carry type relationships without a full Mermaid diagram or a wide table.
    Proposed correction: Add accepted examples for `type-shape block` and `edge list`. Use them for compact ownership/type/data-flow relationships, such as `Boot -> RasmRuntime -> DrainHandle` or `Producer -> Consumer: fact`, when rendering would add no information. State when to upgrade to Mermaid: multiple branches, rejoin points, lifecycles, or cross-cluster topology.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/explanation/architecture.md`, `docs/standards/formatting.md`.
    Outcome: PROMOTE.
    Proof gap/question: A promotion pass should decide whether the bare code block rule also needs a `text conceptual` example to avoid untagged fences.

Finding 5
    File/line/heading: `docs/standards/information-structure.md:436` `## [13][MERMAID_C4]`; `docs/standards/information-structure.md:450`; sample `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md:121`, `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md:141`, `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md:154`, `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md:162`, `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md:171`.
    Evidence snippet: The Vectors diagram spans build, rail, capability bands, spectral substrate, providers, result, styling, and class definitions in one diagram.
    Weakness: The standard has node and edge limits, but it does not give a decomposition recipe for an oversized diagram that mixes construction, dispatch, capability bands, substrate, and result surfaces.
    Proposed correction: Add a diagram-split rule: first extract proof/status/update facts to records or tables, then choose one remaining edge set per diagram. For oversized architecture diagrams, split by `construction flow`, `dispatch flow`, `dependency topology`, or `runtime sequence`; replace simple dependency-only edges with an edge list when no rendering value remains.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/explanation/architecture.md`, `docs/standards/style-guide.md`.
    Outcome: PROMOTE.
    Proof gap/question: Renderer validation is not needed for this report, but any active Mermaid edit would need local render proof or a `Proof gap:` per `proof.md`.

Finding 6
    File/line/heading: `docs/standards/information-structure.md:73` `## [3][TABLES]`; `docs/standards/information-structure.md:88`; sample `docs/system-api-map/packages.md:19`, `docs/system-api-map/packages.md:21`, `docs/system-api-map/packages.md:48`, `docs/host-libraries.md:24`.
    Evidence snippet: `packages.md` carries a current package table with 24 rows, then a candidate-band table; `host-libraries.md` carries a 12-row first-consumer candidate table.
    Weakness: The standard states table ceilings and summarize-then-detail, but the practical trigger still reads like a size rule rather than an author action. A maintainer with a growing package or support table needs an explicit "when this crosses the ceiling, create a summary table plus detail records or sibling tables" recipe.
    Proposed correction: Add a `large lookup decomposition` packet immediately after the table ceiling rule: `Summary row`, `Detail anchor`, `Detail container`, `Update trigger`, and `Split axis`. Name common repo axes: state, owner, package family, rail, platform, version, or support profile.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/reference/support-matrix.md`, `docs/standards/reference/reference.md`.
    Outcome: PROMOTE.
    Proof gap/question: Need current active type-standard review before deciding whether the detail container should be a table, grouped definition block, or subsection-per-record block for support matrices.

Finding 7
    File/line/heading: `docs/standards/information-structure.md:242` `## [10][DECISION_LOOKUP_TABLES]`; sample `docs/host-libraries.md:24`, `docs/host-libraries.md:26`, `docs/usage.md:28`, `docs/usage.md:39`, `tests/csharp/AGENTS.md:24`, `tests/csharp/AGENTS.md:26`, `tests/csharp/AGENTS.md:28`.
    Evidence snippet: Host-library candidates are trigger-to-owner rows; test instructions classify proof before assertion across static, bridge, architecture, tooling, fuzz, benchmark, survivor, or proof-gap routes.
    Weakness: The current decision-table section handles finite condition combinations but does not explicitly separate `classification table`, `decision table`, `lookup table`, and `flowchart`. Repo docs repeatedly carry classification routes where the result is a type or owner, not an action from a full truth table.
    Proposed correction: Add a subchooser: `classification table` for one observed signal to one class/owner; `lookup table` for key to value; `decision table` for multiple independent conditions plus hit policy; `flowchart` for ordered or short-circuit decisions; `prose cases` for non-enumerable alternatives.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/README.md`, `docs/standards/agents-md.md`, `docs/standards/task/runbook.md`.
    Outcome: PROMOTE.
    Proof gap/question: The target should not create a new table type if `classification table` can be expressed as a lookup table profile; promotion should choose the lowest-shape wording.

Finding 8
    File/line/heading: `docs/standards/information-structure.md:113` `## [5][TABLES_PROSE]`; `docs/standards/information-structure.md:452`; sample `tools/assay/README.md:40`, `tools/assay/README.md:42`, `tools/assay/README.md:79`, `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:7`, `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:17`.
    Evidence snippet: Assay flow carries a Mermaid diagram plus a text equivalent; AppHost build status carries a diagram plus a status table.
    Weakness: The standard says each representation must have a distinct reader job, but it does not provide a co-location test for diagram/table/prose triples. That leaves authors without a simple way to decide whether the text equivalent, table, and diagram are complementary or duplicative.
    Proposed correction: Add a `representation co-location test`: name each container's job in a lead or caption; keep both only if deleting one removes a unique reader action; put the proof/status/update carrier in records or tables; put topology/sequence in diagrams; put accessibility text equivalent beside the diagram but do not let it become a second source of status facts.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/style-guide.md`, `docs/standards/proof.md`.
    Outcome: PROMOTE.
    Proof gap/question: None for the rule. Active diagram edits would need renderer or accessibility proof only when claiming rendered behavior.

Finding 9
    File/line/heading: `docs/standards/information-structure.md:266` `## [11][CODE_BLOCKS]`; `docs/standards/information-structure.md:272`; sample `libs/csharp/Rasm.AppHost/ARCHITECTURE.md:86`; `tools/rhino-bridge/README.md:205`.
    Evidence snippet: The AppHost type-shape block uses a bare fence, while bridge uses a `csharp conceptual` fence for a sample return line.
    Weakness: The standards corpus declares language-intent pairs, but the target's practical toolbelt does not say what to do with source-like snippets that are not copy-safe source and not ordinary output. This is where authors drift into bare fences.
    Proposed correction: Add a fence-selection note: `text conceptual` for source-neutral shape, `<language> conceptual` for language-shaped illustration, `<language> template` for copyable structure with placeholders, `<language> copy-safe` only for exact runnable input, and `text output-only` for observed or expected output. Mention CommonMark's info-string convention only as capability support, not a local renderer guarantee.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`, `docs/standards/reference/code-documentation.md`.
    Outcome: PROMOTE.
    Proof gap/question: Active promotion should verify whether `csharp conceptual` is already allowed implicitly by "other language tags only when genuinely that source language and still carries one intent label" or should be listed explicitly.

Finding 10
    File/line/heading: `docs/standards/information-structure.md:456` `## [14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; sample `tools/quality/README.md:3`, `tools/quality/README.md:12`, `tools/quality/README.md:87`, `tools/rhino-bridge/README.md:3`, `tools/rhino-bridge/README.md:6`, `libs/csharp/Rasm.AppHost/README.md:11`.
    Evidence snippet: Tool and AppHost docs use alert blocks for output contracts, mutation warnings, live bridge boundaries, and Generic Host risk.
    Weakness: The target says alerts interrupt the reading path, but it does not route alert content back to a durable container when the warning is really a command contract, failure-reading row, or boundary rule.
    Proposed correction: Add an alert demotion rule: keep an alert only for the interrupting risk; move repeatable output fields to a CLI envelope record, failure responses to a failure-reading table, and general boundaries to the document's boundary section.
    Owner file: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`, `docs/standards/task/runbook.md`, `docs/standards/reference/api.md`.
    Outcome: PROMOTE.
    Proof gap/question: None.

## [EVIDENCE]

Mandatory instruction and standards reads:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/_reports/AGENTS.md`
- `docs/standards/information-structure.md`
- `docs/standards/formatting.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`

Sampled non-standards Markdown outside `docs/standards/**` and `.claude/**`:
- `README.md`
- `docs/usage.md`
- `docs/host-libraries.md`
- `docs/system-api-map/README.md`
- `docs/system-api-map/packages.md`
- `docs/external-libs/README.md`
- `docs/external-libs/languageext/rasm.md`
- `docs/testing-libs/README.md`
- `tools/quality/README.md`
- `tools/rhino-bridge/README.md`
- `tools/assay/README.md`
- `libs/csharp/Rasm.AppHost/ARCHITECTURE.md`
- `libs/csharp/Rasm.AppHost/README.md`
- `libs/csharp/Rasm.AppHost/ROADMAP.md`
- `libs/csharp/Rasm/Vectors/_ARCHITECTURE.md`
- `tests/csharp/AGENTS.md`

Current-source notes used only for Markdown capability claims:
- [GitHub Flavored Markdown Spec](https://github.github.com/gfm/?ref=scriptorium): tables are a GFM leaf-block extension made of header, delimiter, and data rows; this supports the existing rule that table cells should stay flat row/column data.
- [GitHub Docs, organizing information with tables](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-tables): GitHub documents inline formatting, alignment markers, and pipe escaping inside tables; this supports table-cell safety rules without widening tables into nested structures.
- [CommonMark 0.30 fenced code blocks](https://spec.commonmark.org/0.30/#fenced-code-blocks): fenced blocks carry an optional info string, content is literal, and the first word is conventionally used as the language; this supports explicit language-plus-intent fence labels while avoiding claims that CommonMark itself enforces local intent labels.

## [RECOMMENDATIONS]

Promotion order:
1. Add `INFORMATION_SIGNATURE_CHOOSER` near the top so the standard answers the author's raw input question before it teaches container families.
2. Promote currently named but underspecified packets into templates: command step, command card, CLI envelope record, API field card, field packet, type-shape block, and edge list.
3. Add decomposition recipes for oversized tables and oversized diagrams.
4. Add co-location tests for prose/table/diagram/text-equivalent combinations.
5. Add alert demotion and fence-selection notes so warnings and snippets route to durable containers instead of becoming decoration.

Candidate merge wording:

```markdown template
[INFORMATION_SIGNATURE_CHOOSER]:
- One claim, consequence, or boundary -> prose paragraph; use a paragraph pair when the next unit states consequence, proof, or route-away.
- One item's fields -> definition block; move to subsection-per-record when fields need lists, code blocks, or independent updates.
- Homogeneous items -> table; split to summary-plus-detail when the row, column, width, or prose-cell ceiling fails.
- Independently updated items -> status-tagged records.
- Runnable action -> command-step record; reusable command contract -> command card.
- One observed signal to one owner or class -> classification/lookup table.
- Multiple independent conditions to one action -> decision table with hit policy.
- Ordered branch or lifecycle -> Mermaid flowchart or state diagram.
- Small hierarchy, type shape, or directed edge set -> monospace tree, type-shape block, or edge list.
- Literal input, output, config, schema, or source snippet -> fenced code block with one language and one intent label.
- Drift-prone fact -> proof record beside the claim.
```

## [PROOF_GAPS]

- No active standards were edited in this pass, so no docs validation gate ran.
- The target session folder had no manifest or prior lane report at read time. Per `_reports/AGENTS.md`, missing `README.md` is an archive-maintenance gap, but this task required exactly one report at the named path.
- Mermaid renderer behavior was not validated locally; recommendations avoid claiming a rendered output passed.
- Candidate wording needs an active-corpus promotion pass before insertion so the final rule does not duplicate type-standard-owned page anatomy or proof-field order.
