Question: Where does `information-structure.md` fail to give clean carrier choices for active type standards and current repo Markdown shapes?
Type: gap-critique
Lane: track-information-structure
Merge key: information-structure.md :: cross-type carrier choice defects :: tighten
Target owner: docs/standards/information-structure.md
Source basis: repo proof from `CLAUDE.md`, `AGENTS.md`, `docs/standards/README.md`, `docs/standards/AGENTS.md`, `.reports/AGENTS.md`, line-numbered reads of `docs/standards/information-structure.md`, all active type standards under `explanation`, `reference`, `task`, and `learning`, and sampled non-standards Markdown outside `docs/standards/**` and `.claude/**`
Promotion target: docs/standards/information-structure.md
Outcome: PROMOTE

## [FINDINGS]

### [1][CARRIER_CATALOGUE]

The corpus currently uses these information systems. `information-structure.md` names the broad families, but several recurring systems need a sharper carrier choice so type standards stop defining private mini-grammars.

| [INDEX] | [SYSTEM] | [CURRENT EVIDENCE] | [CARRIER GAP] |
| :-----: | :------- | :----------------- | :------------ |
|   [1]   | Prose paragraphs | `information-structure.md` lines 21-24, 69-71 | Covered. Needs cross-link only when used as lead, consequence, or route boundary. |
|   [2]   | Peer bullets | `information-structure.md` lines 21-25, 213-220 | Covered. |
|   [3]   | Numbered action lists | `information-structure.md` lines 21-25, 213-220 | Covered only for simple action order; not for numbered records with indented fields. |
|   [4]   | Checklists | `information-structure.md` lines 195-211 | Covered for checkbox gates; not enough to distinguish checkbox fields from step/checkpoint records. |
|   [5]   | Definition block | `information-structure.md` lines 221-240 | Covered. Needs clearer relation to ordered record continuations. |
|   [6]   | Grouped definition block | `information-structure.md` lines 229-240; `tools/assay/README.md` lines 20-30 | Covered, but current rule does not state when a grouped block may omit heading syntax and still remain scannable. |
|   [7]   | Subsection-per-record block | `information-structure.md` line 229; `how-to.md` lines 236-244 | Covered by threshold, but not named in the chooser table. |
|   [8]   | Status-tagged lifecycle record | `information-structure.md` lines 122-164 | Covered for default lifecycle, but not for type-local vocabularies and status projections. |
|   [9]   | Type-local status vocabulary | `adr.md` lines 57-69; `roadmap.md` lines 74-85; `support-matrix.md` lines 133-164; `onboarding.md` lines 73-79; `tutorial.md` lines 146-153 | No clean shared carrier for vocabulary definition, omitted-state declaration, projection to lifecycle, and removal behavior. |
|  [10]   | Profile selector | `api.md` lines 28-42; `design-doc.md` lines 131-140; `test-strategy.md` lines 97-144; `support-matrix.md` validation lines 399-410 | No named carrier for one-of profile choice plus obligations. |
|  [11]   | Authoring contract block | Every active type standard opens with `[AUTHORING_CONTRACT]`, for example `api.md` lines 20-27 and `contributing.md` lines 23-29 | No carrier rule says whether this is a definition block, bullet group, or contract record. |
|  [12]   | Page skeleton template | `information-structure.md` lines 498-563; active type standards `REQUIRED_STRUCTURE` sections | Covered, but shared standard anatomy and produced-document skeleton rules are mixed in one section. |
|  [13]   | Conditional-section insertion block | `roadmap.md` lines 115-139; `onboarding.md` lines 50-70 | Covered by page anatomy, but no reusable insertion table rule for trigger, placement, and omission. |
|  [14]   | Section-cardinality block | `information-structure.md` lines 524-536; type standards such as `design-doc.md` lines 142-147 | Covered by example only; carrier and field order are not explicit. |
|  [15]   | Adjacent-route relation record | `information-structure.md` lines 180-189; `design-doc.md` lines 359-372 | Covered. |
|  [16]   | Progress marker with calculation basis | `information-structure.md` lines 191-193; `roadmap.md` lines 195-207 | Covered at high level; roadmap-specific denominator and proof-map pattern needs a clear relation to the shared rule. |
|  [17]   | Comparison table | `information-structure.md` lines 73-106 | Covered. |
|  [18]   | Lookup table | `information-structure.md` lines 242-265 | Covered. |
|  [19]   | Decision table | `information-structure.md` lines 242-256 | Covered. |
|  [20]   | Matrix, support matrix, dependency matrix | `information-structure.md` lines 56-67, 94-105; `support-matrix.md` lines 221-271 | Covered as table forms, but no split rule for matrix row notes versus row-promoted records. |
|  [21]   | Row note or proof note after a table | `support-matrix.md` lines 244-256 | Not cleanly classified; footnote, note block, and row-promoted record boundaries need a decision rule. |
|  [22]   | Compact contrast record | `information-structure.md` line 288; `contributing.md` lines 128-137; `how-to.md` lines 174-177; `tutorial.md` lines 238-239 | Referenced but not owned as a carrier. |
|  [23]   | Command card | `information-structure.md` line 341; `api.md` lines 134-140 | Named only in a catalogue; field order and table-vs-record choice are local to API. |
|  [24]   | CLI envelope record and JSON output contract | `information-structure.md` line 342; `tools/quality/README.md` lines 12-13; `tools/assay/README.md` lines 172-218 | Named only in a catalogue; repo examples need stdout, stderr, exit, artifact, status, and failure-reading fields. |
|  [25]   | API field card | `information-structure.md` line 343; `api.md` lines 110-121 | Named only in a catalogue; no shared card field order. |
|  [26]   | Machine-consumed Markdown | `information-structure.md` lines 481-488; `tools/cs-analyzer/AnalyzerReleases.Shipped.md` lines 1-2 | Exception exists late, but parser-owned ledgers need a first-class route. |
|  [27]   | Code fences with intent labels | `information-structure.md` lines 266-305 | Covered. |
|  [28]   | Monospace codemap, tree, stack, and small matrix | `information-structure.md` lines 306-434; `architecture.md` lines 176-232 | Covered broadly; codemap proof packet and status table coupling need a named carrier. |
|  [29]   | Mermaid diagram with config and text equivalent | `information-structure.md` lines 409-454; `runbook.md` lines 62-91 | Covered. |
|  [30]   | C4 or topology packet | `information-structure.md` lines 326-328, 436-454 | Named, but the handoff from form to architecture type standard needs a carrier decision ladder. |
|  [31]   | GitHub alert callout | `information-structure.md` lines 456-464; `tools/rhino-bridge/README.md` lines 3-7 | Covered. |
|  [32]   | `<details>` collapsible block | `information-structure.md` lines 466-471 | Covered. |
|  [33]   | Footnote | `information-structure.md` lines 473-477 | Covered. |
|  [34]   | Hidden HTML source comment | `information-structure.md` lines 479-480 | Covered by route to formatting. |
|  [35]   | Runbook response profile and escalation packet | `runbook.md` lines 32-60, 280-292 | No shared carrier for incident/process profile records or domain-valid `none` values. |
|  [36]   | How-to procedure step record | `how-to.md` lines 165-179 | No shared carrier for numbered action plus fields. |
|  [37]   | Tutorial checkpoint record | `tutorial.md` lines 214-260 | No shared carrier for ordered checkpoint records with working-state proof. |
|  [38]   | Onboarding source, readiness, and stop records | `onboarding.md` lines 126-149, 177-219 | No shared carrier for preparation records with local availability. |
|  [39]   | PR body template | `contributing.md` lines 222-240 | No shared carrier rule for prose-plus-field packet templates. |
|  [40]   | Structured stdout marker table | `tools/rhino-bridge/README.md` lines 215-227 | Covered only as lookup table; no guidance for line-oriented machine markers. |
|  [41]   | Owner ladder and route tables | `docs/usage.md` lines 5-26; `AGENTS.md` lines 69-88 | Covered as lookup tables, but routing tables often carry authority and proof consequences that need one-row-table rejection and field clarity. |
|  [42]   | Command catalog table | `README.md` lines 50-101; `tools/rhino-bridge/README.md` lines 47-67; `tools/assay/README.md` lines 81-142 | Covered as lookup tables, but command cards and command rows split inconsistently. |

### [2][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: CONTAINER_CHOOSER]

Line/heading: `docs/standards/information-structure.md` lines 15-72, `[2][CONTAINER_CHOOSER]`.

Evidence snippet: The chooser maps reader questions to broad shapes: `One item's fields?` to definition block, `Shared row values?` to table-form chooser, and `Conditional action?` to decision table.

Weakness/inconsistency: The active type corpus repeatedly needs a control-record chooser that is neither a generic definition block nor a table: authoring contracts, profile selectors, section-cardinality blocks, status-vocabulary definitions, API field cards, command cards, response profiles, and checkpoint records. Today those carriers are reinvented in type files.

Proposed correction: Add a `[CONTROL_RECORD_CHOOSER]` immediately after the main chooser. It should route `authoring contract`, `profile selector`, `closed vocabulary`, `field card`, `command/output contract`, `ordered checkpoint`, `section cardinality`, and `machine-consumed surface` to named carriers, with the table/record threshold for each.

Active owner: `docs/standards/information-structure.md`.

Ripple files: `docs/standards/explanation/*.md`, `docs/standards/reference/*.md`, `docs/standards/task/*.md`, `docs/standards/learning/*.md`.

Decision recommendation: `PROMOTE`. This is the highest-value structural fix because it gives type standards one shared vocabulary for forms they already use.

Proof gap/question: No renderer or parser command is needed for the rule itself. A later edit should run a shape scan for `AUTHORING_CONTRACT`, `SECTION_CARDINALITY`, `Profile:`, `Availability:`, `Command:`, `Field:`, and `Accepted`/`Rejected` examples to confirm the chooser covers all active uses.

### [3][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: STRUCTURED_RECORDS]

Line/heading: `docs/standards/information-structure.md` lines 122-164, `[6][STRUCTURED_RECORDS]`.

Evidence snippet: The standard defines a default `Status` set and allows type standards to define domain-specific status sets when they define exact casing, active states, blocked states, returnable states, terminal states, and removal behavior.

Weakness/inconsistency: The rule is correct but too buried inside structured records. Type standards need status vocabularies even before a record set exists: ADR lowercase dispositions, roadmap state plus milestone status, support display states, ramp availability, tutorial availability, design lifecycle, and test-strategy trigger/blocking vocabularies. The carrier is a vocabulary definition, not always a record item.

Proposed correction: Split a new `[CLOSED_VOCABULARIES]` section before structured records. Define the required vocabulary card fields: `Vocabulary`, `Applies to`, `Values`, `Active states`, `Blocked states`, `Returnable states`, `Terminal states`, `Omitted shared states`, `Projection rule`, `Removal behavior`, `Use before first record`.

Active owner: `docs/standards/information-structure.md`; domain meanings remain in each type standard.

Ripple files: `adr.md`, `design-doc.md`, `roadmap.md`, `test-strategy.md`, `support-matrix.md`, `onboarding.md`, `tutorial.md`, `runbook.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: Decide whether the vocabulary card should allow lowercase semantic values like ADR `accepted` and support labels like `End of support`, or whether it should require a `Display value` plus `Machine value` split when values are not uppercase tokens.

### [4][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: CHECKLISTS_LISTS_DEFINITION_BLOCKS]

Line/heading: `docs/standards/information-structure.md` lines 195-229, `[7][CHECKLISTS]`, `[8][LISTS]`, `[9][DEFINITION_BLOCKS]`.

Evidence snippet: Checklist item fields must trail the checkbox line; grouped definition blocks use four-space indentation; lists nest no deeper than two levels.

Weakness/inconsistency: Active task and learning standards use ordered records with indented fields, not checkboxes: `how-to.md` lines 165-172, `runbook.md` lines 175-188, and `tutorial.md` lines 229-257. The current standard does not say that a numbered step can own a definition-record continuation, nor whether those continuations count against list nesting.

Proposed correction: Add an `[ORDERED_RECORDS]` subsection. Define `numbered checkpoint record` as a numbered list item whose indented `label: value` continuation lines are record fields, not child list nesting. State when to use it: procedures, triage checks, tutorial checkpoints, and command-bearing task steps where order and per-step proof both matter.

Active owner: `docs/standards/information-structure.md`.

Ripple files: `task/how-to.md`, `task/runbook.md`, `learning/tutorial.md`, `task/contributing.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: Markdown rendering of indented continuation lines should be verified in the repo renderer if a future edit changes indentation width or mixes child bullets under step records.

### [5][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: CODE_BLOCKS]

Line/heading: `docs/standards/information-structure.md` lines 266-305, `[11][CODE_BLOCKS]`.

Evidence snippet: The standard says not to fence short accepted/rejected, before/after, good/bad, or near-miss examples; it routes compact contrast records to `formatting.md`.

Weakness/inconsistency: Type standards use contrast records as a semantic carrier, not merely notation: `contributing.md` lines 128-137, `how-to.md` lines 174-177, `onboarding.md` lines 151-153, and `tutorial.md` lines 238-239. `formatting.md` can own labels, but `information-structure.md` must own when contrast records replace prose, tables, and fences.

Proposed correction: Add `[CONTRAST_RECORDS]` under examples or code blocks. Field order should be `Accepted`, `Rejected` or `Near miss`, then `Reason`. Use it only when the contrast prevents a likely wrong author action; promote to subsection-per-record if either side needs multiline evidence.

Active owner: `docs/standards/information-structure.md` for carrier choice; `docs/standards/formatting.md` for exact label notation.

Ripple files: all type standards with accepted/rejected examples, especially `contributing.md`, `how-to.md`, `onboarding.md`, `tutorial.md`, `architecture.md`, `roadmap.md`, and `test-strategy.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: Confirm whether `Accepted fields:` and `Accepted lead:` in `adr.md` lines 168-180 should become one grouped contrast record or remain multiple compact field lines.

### [6][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: MONOSPACE_TEXT_AND_MERMAID_C4]

Line/heading: `docs/standards/information-structure.md` lines 306-454, `[12][MONOSPACE_TEXT]` and `[13][MERMAID_C4]`.

Evidence snippet: The standard lists codemap trees, matrices, Mermaid diagrams, C4 packets, proof record packets, and task record packets, and says each table/record/diagram must carry a distinct reader job.

Weakness/inconsistency: The architecture standard already has a richer representation pattern: codemap tree, path-state table, and representation proof packet live together in `architecture.md` lines 176-232. `information-structure.md` lacks a carrier for a representation packet that states what each visual owns and how proof attaches without duplicating the diagram.

Proposed correction: Add `[REPRESENTATION_PACKETS]` before or inside the visual section. Define fields: `Representation`, `Carries`, `Does not carry`, `Companion representation`, `Evidence`, `Generated from`, `Source of truth`, `Review trigger`, `Text equivalent required`. Use it for codemap trees, topology diagrams, C4 packets, and rendered diagrams with companion tables.

Active owner: `docs/standards/information-structure.md`; proof field semantics remain in `proof.md`; architecture-specific C4 scope remains in `explanation/architecture.md`.

Ripple files: `explanation/architecture.md`, `explanation/test-strategy.md`, `task/runbook.md`, `learning/tutorial.md`, `tools/quality/README.md`, `tools/rhino-bridge/README.md`, `tools/assay/README.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: No Mermaid renderer claim is made here. If promoted wording changes Mermaid examples, run the repo's current Mermaid check before claiming render validity.

### [7][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: MACHINE_CONSUMED_RECORD]

Line/heading: `docs/standards/information-structure.md` lines 481-488, `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`.

Evidence snippet: Machine-consumed Markdown may use a narrower shape when a named parser, analyzer, release ledger, or generator consumes exact headings, fields, or row order.

Weakness/inconsistency: This rule is placed after details and footnotes, but it governs high-authority exceptions. Repo samples include Roslyn analyzer release ledgers at `tools/cs-analyzer/AnalyzerReleases.Shipped.md` lines 1-2, JSON-envelope output contracts in `tools/quality/README.md` lines 12-13, and line-oriented bridge markers in `tools/rhino-bridge/README.md` lines 215-227. These should be classified before ordinary prettification rules can rewrite them.

Proposed correction: Move or duplicate the rule as a first-class `[MACHINE_CONSUMED_MARKDOWN]` section near code blocks and records. Require `Consumer`, `Parsed shape`, `Allowed deviation`, `Validation command or proof gap`, and `Do not normalize unless consumer changes`.

Active owner: `docs/standards/information-structure.md`.

Ripple files: `reference/api.md`, `tools/cs-analyzer/AnalyzerReleases.*.md`, `tools/quality/README.md`, `tools/rhino-bridge/README.md`, `tools/assay/README.md`, generated contract/reference docs.

Decision recommendation: `PROMOTE`.

Proof gap/question: If this is promoted, verify the exact Roslyn release-tracking shape from current local tool/source or official analyzer docs before changing any release ledger prose.

### [8][DOCS/STANDARDS/INFORMATION-STRUCTURE.MD :: PAGE_ANATOMY]

Line/heading: `docs/standards/information-structure.md` lines 498-563, `[17][PAGE_ANATOMY]`.

Evidence snippet: The section first gives a generic template with `Lead`, `Use when`, rules, `Boundaries`, and `Validation`; then it gives a type-standard opening order and a second template using placeholder `SECTION_A` and `SECTION_B`.

Weakness/inconsistency: Three different jobs are collapsed: shared standard page anatomy, type standard authoring contract order, and produced-document skeletons. Active type standards have their own `REQUIRED_STRUCTURE` sections, but the shared standard does not name which template is for the standard file itself versus the document being prescribed.

Proposed correction: Split the section into three subsections: `[STANDARD_FILE_ANATOMY]`, `[TYPE_STANDARD_OPENING]`, and `[PRODUCED_DOCUMENT_SKELETON]`. Keep the authoring-contract order in the type-standard opening rule, and keep produced-document templates as examples that type standards own locally.

Active owner: `docs/standards/information-structure.md`.

Ripple files: every active type standard, plus `docs/standards/AGENTS.md` lines 4-5 because its artifact contract repeats the opening-order requirement.

Decision recommendation: `PROMOTE`.

Proof gap/question: After promotion, check whether type standards should still carry `[AUTHORING_CONTRACT]` as bullets or switch to a shared contract-record carrier.

### [9][DOCS/STANDARDS/REFERENCE/API.MD :: PROFILES_AND_CONTRACT_RECORDS]

Line/heading: `docs/standards/reference/api.md` lines 28-42, `[2][PROFILES]`; lines 89-121, `[5][CONTRACT_RECORDS]`.

Evidence snippet: API pages choose one profile, then use contract records, field cards, command cards, and CLI envelope records.

Weakness/inconsistency: `information-structure.md` mentions command cards, CLI envelopes, and API field cards only as catalogue items at lines 341-343. It does not give a clean carrier choice between a command table, command card, API contract record, CLI envelope record, and field card.

Proposed correction: Add a shared `[CALLABLE_CONTRACT_RECORDS]` chooser. Use a lookup table for command inventory; use a command card when one command family needs preconditions, channels, side effects, proof, and review trigger; use a CLI envelope record when stdout, stderr, exit, artifacts, diagnostics, and failure reading are parsed independently; use API field cards when individual fields have parser/default/failure semantics.

Active owner: `docs/standards/information-structure.md`; API-specific field semantics stay in `reference/api.md`.

Ripple files: `reference/api.md`, `reference/reference.md`, `task/how-to.md`, `task/runbook.md`, `tools/quality/README.md`, `tools/assay/README.md`, `tools/rhino-bridge/README.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: Decide whether CLI envelope records belong under `reference/api.md` only, or whether `information-structure.md` should own the minimum universal fields.

### [10][DOCS/STANDARDS/TASK/RUNBOOK.MD :: RESPONSE_PROFILE_AND_DOMAIN_NONE]

Line/heading: `docs/standards/task/runbook.md` lines 32-60, `[3][RESPONSE_PROFILE]`.

Evidence snippet: Runbooks render impact class, response clock, mutation permission, escalation threshold, communication requirement, and evidence requirement as a definition block; the literal `none` is valid only for domain fields where no obligation exists.

Weakness/inconsistency: `docs/standards/AGENTS.md` forbids placeholder fields and filler `none`, while runbooks need domain-valid `none` for safe mutation, rollback, communication, or local profile fields. `information-structure.md` does not define a "domain-none" rule for field packets, so future agents may either over-delete valid `none` or preserve filler `none`.

Proposed correction: Add a field-value rule under definition blocks: `none` is valid only when the field's domain defines no obligation as a semantic value; otherwise omit the field or state a proof gap. Require the local standard to name the fields where `none` is allowed before examples.

Active owner: `docs/standards/information-structure.md`; forbidden metadata posture stays in `docs/standards/AGENTS.md`.

Ripple files: `task/runbook.md`, `task/contributing.md`, `reference/support-matrix.md`, `learning/onboarding.md`, `learning/tutorial.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: Confirm whether support matrix `n/a` and table `--` values should share the same field-value rule or remain support-specific because source null/false/omitted distinctions are stronger there.

### [11][DOCS/STANDARDS/LEARNING/TUTORIAL.MD :: CHECKPOINT_RECORDS]

Line/heading: `docs/standards/learning/tutorial.md` lines 214-260, `[8][STEP_RECORDS]` and `[9][EXECUTION_VOCABULARY]`.

Evidence snippet: Tutorial steps are numbered checkpoint records with `Operation`, `Expected`, `Working state`, `Execution`, `Notice`, and `If wrong` fields.

Weakness/inconsistency: `information-structure.md` treats numbered lists and definition blocks as separate choices. Tutorial checkpoints prove that some ordered lists are record carriers. Without a shared carrier, how-to, tutorial, and runbook standards each carry slightly different wording for the same structural idea.

Proposed correction: Fold tutorial checkpoint, how-to command step, and runbook triage step under the `[ORDERED_RECORDS]` correction from finding 4. Add a subtype table: `procedure step`, `triage check`, `learner checkpoint`, with required fields and route-away when the step becomes a full record.

Active owner: `docs/standards/information-structure.md`; field names remain local to each type standard.

Ripple files: `learning/tutorial.md`, `task/how-to.md`, `task/runbook.md`.

Decision recommendation: `MERGE` with finding 4.

Proof gap/question: If promoted, decide whether ordered records may contain fenced blocks or whether any fenced command forces the command out of the record into adjacent prose.

### [12][NON_STANDARDS_MARKDOWN_SAMPLE :: COMMAND_AND_OUTPUT_SHAPES]

Line/heading: sampled files outside `docs/standards/**` and `.claude/**`.

Evidence snippet: Root `README.md` uses layout/build-property tables and copy-safe command fences at lines 13-101. `docs/usage.md` uses owner ladders and proof-source tables at lines 5-69. `tools/quality/README.md` uses alerts, Mermaid, module tables, command tables, status tables, and JSON Envelope prose at lines 3-119. `tools/rhino-bridge/README.md` uses callouts, diagrams, command tables, output-contract fields, status policy, marker tables, and failure-reading tables at lines 1-67, 155-188, 215-227, and 259-271. `tools/assay/README.md` uses a status definition block, grouped migration records, command subheading records, and output channel records at lines 5-31, 81-142, and 172-218.

Weakness/inconsistency: Current repo Markdown relies heavily on operator-facing command/output surfaces. `information-structure.md` covers tables and code fences, but not the combined pattern: command inventory table plus per-command record plus output envelope/channel record plus failure-reading table.

Proposed correction: Add a `[COMMAND_OUTPUT_SURFACES]` decision rule: use command inventory tables for selecting a surface; per-command records for command family semantics; output envelope records when machines parse channels; failure-reading decision/lookup tables when signal drives next action; copy-safe fences only for runnable commands.

Active owner: `docs/standards/information-structure.md`; command truth remains in each tool README or API standard.

Ripple files: root `README.md`, `tools/quality/README.md`, `tools/assay/README.md`, `tools/rhino-bridge/README.md`, `reference/api.md`, `reference/reference.md`, `task/how-to.md`, `task/runbook.md`.

Decision recommendation: `PROMOTE`.

Proof gap/question: No current command behavior was validated in this pass; all command/output examples are source-shape evidence only.

## [RECOMMENDATIONS]

1. Add one cross-cutting carrier chooser after `[2][CONTAINER_CHOOSER]` rather than scattering fixes through type standards.
2. Promote closed vocabulary, profile selector, ordered record, compact contrast record, representation packet, machine-consumed Markdown, and command/output surface as named carriers.
3. Split page anatomy into standard-file anatomy, type-standard opening, and produced-document skeleton rules.
4. Keep domain field names in type standards. `information-structure.md` should own the carrier, escalation threshold, omission rule, and table-vs-record choice only.
5. Do not edit active type standards until the shared carrier vocabulary is tightened; otherwise each type file will keep inventing local synonyms.

## [PROOF_GAPS]

- No external current-source research was needed. The findings rely on current repository source and active standards, and make no new provider or renderer claim.
- No Markdown renderer, link checker, Mermaid renderer, or docs build was run. Any future promotion that changes fenced diagrams, HTML details, table syntax, or machine-consumed ledgers must run the matching proof gate or record the gap.
- The target session folder lacks a session manifest. Because this task requested exactly one report, this report records the archive-maintenance gap instead of creating `README.md`.
