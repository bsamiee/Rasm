# [TRACK_FORMATTING_NOTATION_02]

Question: Independent second pass on `docs/standards/formatting.md`, focused on unusual deviations from standardized field names, token families, status vocabularies, and formatting-vs-form boundary leaks.
Type: repo-scan
Lane: track-formatting-notation
Merge key: formatting-cross-corpus-usage
Target owner: `docs/standards/formatting.md`
Source basis: mandatory root and standards overlays, full active standards corpus excluding `.reports`, `.claude/**` notation scan, and non-standards repo Markdown notation scan.
Promotion target: `docs/standards/formatting.md`, with owner-routed corrections in `docs/standards/information-structure.md`, `docs/standards/reference/readme.md`, and type standards where noted.
Outcome: CORRECT

## [SOURCE_SET]

Read first:

| [INDEX] | [SOURCE] | [USE] |
| :---: | :--- | :--- |
| [1] | `CLAUDE.md` | Project skill route, repo notation baseline, `.claude/skills` authority context. |
| [2] | `AGENTS.md` | Root standards route, active instruction chain, `.reports` treatment. |
| [3] | `docs/standards/README.md` | Standards corpus route and shared owner map. |
| [4] | `docs/standards/AGENTS.md` | Active corpus read rule, formatting owner map, validation rule. |
| [5] | `.reports/AGENTS.md` | Report lane contract, compact record shape, no-retread rules. |

Full active standards corpus read line by line, excluding `.reports/**`:

| [INDEX] | [SOURCE] |
| :---: | :--- |
| [1] | `docs/standards/AGENTS.md` |
| [2] | `docs/standards/README.md` |
| [3] | `` |
| [4] | `docs/standards/agents-md.md` |
| [5] | `docs/standards/formatting.md` |
| [6] | `docs/standards/information-structure.md` |
| [7] | `docs/standards/proof.md` |
| [8] | `docs/standards/style-guide.md` |
| [9] | `docs/standards/explanation/adr.md` |
| [10] | `docs/standards/explanation/architecture.md` |
| [11] | `docs/standards/explanation/design-doc.md` |
| [12] | `docs/standards/explanation/roadmap.md` |
| [13] | `docs/standards/explanation/test-strategy.md` |
| [14] | `docs/standards/reference/api.md` |
| [15] | `docs/standards/reference/code-documentation.md` |
| [16] | `docs/standards/reference/readme.md` |
| [17] | `docs/standards/reference/reference.md` |
| [18] | `docs/standards/reference/support-matrix.md` |
| [19] | `docs/standards/task/contributing.md` |
| [20] | `docs/standards/task/how-to.md` |
| [21] | `docs/standards/task/runbook.md` |
| [22] | `docs/standards/learning/onboarding.md` |
| [23] | `docs/standards/learning/tutorial.md` |

Repo notation scans:

| [INDEX] | [SOURCE_SET] | [PATTERNS] |
| :---: | :--- | :--- |
| [1] | `.claude/**` | Code fences, bracket labels, invocation markers, legacy heading packets, compact table rubrics, prompt field packets. |
| [2] | Non-standards repo Markdown | GitHub alerts, table rubrics, group labels, status words, progress indicators, field packets, compact command records. |

External research:

No external Markdown/rendering research was used. The recommendations below are repo-owned notation and ownership corrections; none depends on a changing renderer capability beyond repository rules already captured in `proof.md`.

## [FINDINGS]

### [F1][COMPACT_GLYPH_ALPHABET_IS_UNMAPPED]

File: `docs/standards/formatting.md`
Path and line: `[2][STATUS_RESULT_MARKERS]`, lines 23-30 and 94-99.
Evidence snippet/source id: `formatting.md:27` lists compact glyphs `[o]`, `[x]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`, `[$]`; `formatting.md:48` says `[x]` means compact fail marker; `formatting.md:94-99` permits glyphs only in dense diagrams, matrices, terminal transcripts, and generated summaries.
Weakness/inconsistency: The compact glyph family is presented as a standardized token family, but only `[x]` receives a stable meaning. The other glyphs are described collectively as "dense result, change, attention, unknown, skipped, partial, or cached cells" without a one-to-one mapping. That leaves `[?]`, `[~]`, `[$]`, `[/]`, and `[o]` open to per-author interpretation while still looking standardized.
Proposed correction: Split compact glyphs into an explicit glyph map with one meaning per glyph, or demote the row to "local compact glyph alphabets must declare mapping before first use." If `[o]` and `[x]` are intended to be global pass/fail, state that directly and require all other glyphs to be type-local or artifact-local.
Active owner: `docs/standards/formatting.md`.
Ripple files: `docs/standards/information-structure.md` `[11][MARKERS_AND_MONOSPACE]`; any matrix/table examples that use compact glyphs; generated report templates under `.reports` if promoted.
Decision: CORRECT.
Proof gap/question: Which exact meanings should the repo reserve globally for `[o]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`, and `[$]`?

### [F2][CONTRAST_RECORD_SHAPE_LEAKS_INTO_FORMATTING]

File: `docs/standards/formatting.md`
Path and line: `[5][LIST_WHITESPACE_DISCIPLINE]`, lines 175-180.
Evidence snippet/source id: `formatting.md:179` defines contrast labels `Accepted:`, `Rejected:`, `Before:`, `After:`, `Near miss:`, and `Reason:` and says to prefer a compact record instead of a table; `information-structure.md:15-17` says information structure owns whether content belongs in a table, list, section, details block, callout, code fence, or record; `information-structure.md:288` routes compact contrast records back to formatting.
Weakness/inconsistency: Formatting says it does not choose containers, decompose tables, write prose, or weigh evidence, but it currently defines a record type and makes a table-vs-record choice. Information structure then points back to formatting for that record, creating a circular boundary.
Proposed correction: Move contrast-record eligibility and field names to `information-structure.md`. Leave only rendering mechanics in `formatting.md`: label spelling, colon spacing, indentation, blank-line rules, and compact field-line layout.
Active owner: `docs/standards/information-structure.md` for form; `docs/standards/formatting.md` for surface rendering.
Ripple files: `docs/standards/style-guide.md`, `docs/standards/task/contributing.md`, `docs/standards/task/how-to.md`, `docs/standards/task/runbook.md`.
Decision: CORRECT.
Proof gap/question: Should contrast records be a shared record type, or should each task/reference standard declare its own contrast fields?

### [F3][HEADING_RULE_OVERSTATES_PUBLIC_README_SURFACE]

File: `docs/standards/formatting.md`
Path and line: `[6][HEADING_IDIOM]`, lines 206-215.
Evidence snippet/source id: `formatting.md:208` says to use one bracketed heading format throughout documentation and instruction files; `docs/standards/reference/readme.md:33-39` allows plain public/registry README headings and bracketed repo-internal README headings; `docs/standards/reference/readme.md:71` says public README section headings stay plain.
Weakness/inconsistency: Formatting states the bracketed heading idiom too broadly. The README standard deliberately preserves public and registry README heading surfaces where plain headings are part of the reader contract.
Proposed correction: Narrow the formatting rule to repository-internal standards-controlled documentation and instruction files, then explicitly route public/registry README heading exceptions to `docs/standards/reference/readme.md`.
Active owner: `docs/standards/formatting.md`.
Ripple files: `docs/standards/reference/readme.md`; public package README files; registry-facing README generation or review checklists.
Decision: CORRECT.
Proof gap/question: Should "documentation files" in formatting always mean repo-internal docs unless a type standard declares an external-reader exception?

### [F4][README_HEADING_EXAMPLE_IS_SELF_CONTRADICTORY]

File: `docs/standards/reference/readme.md`
Path and line: `[3][README_BASELINES]`, lines 41-51.
Evidence snippet/source id: `readme.md:43-45` rejects a repo-internal H1 example that is `# [ASSAY_OPERATOR]`; `readme.md:48-51` then accepts the same H1 shape for the same repo-internal label.
Weakness/inconsistency: The accepted and rejected examples are textually identical. This blocks a clean merge between formatting's rejected `# [H1][NAME]` heading shape and README's public-vs-internal heading exception.
Proposed correction: Replace the rejected example with the actual forbidden shape, likely `# [H1][ASSAY_OPERATOR]`, if the intended rule is "do not include the level token in the heading." If the intended rule is public plain headings, change the accepted example and context accordingly.
Active owner: `docs/standards/reference/readme.md`.
Ripple files: `docs/standards/formatting.md` heading idiom; README review examples in any report lane that cites this section.
Decision: CORRECT.
Proof gap/question: Is the rejected README example meant to reject level-tagged headings, public/internal mixing, or a different bracket packet entirely?

### [F5][RUNBOOK_CONTRAST_LABEL_DRIFTS_FROM_SHARED_LABEL]

File: `docs/standards/task/runbook.md`
Path and line: `[12][FORMAT_CHOICES]`, lines 306-309.
Evidence snippet/source id: `runbook.md:306-309` uses `Command:`, `Rejected near-miss:`, and `Why rejected:`; `formatting.md:179`, `contributing.md:137`, and `how-to.md:72` standardize `Reason:` for contrast explanation.
Weakness/inconsistency: `Why rejected:` is a synonym for `Reason:` in the same field-packet family. It creates a local field-name variant without declaring why runbooks need a distinct name.
Proposed correction: Rename `Why rejected:` to `Reason:` in the runbook standard, unless runbooks require a distinct incident-response field. If distinct, declare it as a runbook-local field before first use and explain why `Reason:` is insufficient.
Active owner: `docs/standards/task/runbook.md`.
Ripple files: `docs/standards/formatting.md` if contrast fields remain there; `docs/standards/information-structure.md` if contrast fields move there.
Decision: CORRECT.
Proof gap/question: None for the inconsistency; only the owner decision on shared versus runbook-local field naming remains.

### [F6][INVOCATION_MARKER_SURFACE_OMITS_SKILL_FILES]

File: `docs/standards/formatting.md`
Path and line: `[3][INVOCATION_MARKERS]`, lines 101-115.
Evidence snippet/source id: `formatting.md:103` reserves invocation markers for instruction files such as `AGENTS.md`, `CLAUDE.md`, and prompt files; `CLAUDE.md:27-30` treats `.claude/skills/*` as project skill context; `.claude/skills/coding-bash/SKILL.md:10` uses `[CRITICAL]`; `.claude/skills/coding-csharp/references/concurrency.md:321` uses `[CRITICAL]` and later `[IMPORTANT]`, `[ALWAYS]`, and `[NEVER]` markers.
Weakness/inconsistency: The repo uses `.claude/skills/**` as agent instruction material, but formatting's allowed invocation-marker surface does not name skill files or skill references. The omission makes existing skill instructions look like violations even when they are semantically instruction surfaces.
Proposed correction: Add skill files and skill reference files that govern agent behavior to the allowed invocation-marker surface, or route `.claude/skills/**` out of invocation markers and into plain modal prose. Promotion is preferable because the project manifest already treats skill context as load-bearing.
Active owner: `docs/standards/formatting.md`, with instruction-surface context in `` and `docs/standards/agents-md.md`.
Ripple files: `.claude/skills/**`; prompt and skill review guidance; future instruction-surface scans.
Decision: PROMOTE.
Proof gap/question: Should generated or third-party skill reference files inherit the same marker allowance, or only repo-authored skill files?

### [F7][LEGACY_HEADING_AND_RUBRIC_PACKETS_PERSIST_OUTSIDE_STANDARDS]

File: `.claude/**`
Path and line: `.claude/prompts/assay-ab-test-session.md:5`, `.claude/prompts/assay-closeout-session.md:6`, `.claude/prompts/assay-refine-session.md:6`, `.claude/prompts/tool-rebuild.md:1`, `.claude/skills/coding-bash/SKILL.md:1`, `.claude/skills/coding-bash/references/array-operations.md:1`, `.claude/skills/coding-bash/references/array-operations.md:5`.
Evidence snippet/source id: Prompt files use headings such as `## [1] WHERE / WHAT / STATE`; skill files use `# [H1][CODING-BASH]` and tables with `[IDX]` or `[S]` instead of `[INDEX]` or declared compact headers.
Weakness/inconsistency: Active standards reject `# [H1][NAME]`, prefer `## [INDEX][NAME]` for bracketed headings, and set `[INDEX]` as the first enumerable table column. The older prompt and skill corpus still uses the previous packet families.
Proposed correction: Do not change `formatting.md` to bless the old packets. Treat this as downstream migration work for prompt and skill files, with a possible short "legacy prompt/skill migration" route if the repo wants those surfaces governed by docs standards.
Active owner: `.claude` prompt and skill owners, not active standards.
Ripple files: `.claude/prompts/*.md`, `.claude/skills/**/*.md`.
Decision: HOLD.
Proof gap/question: Are `.claude/prompts/**` and `.claude/skills/**` intended to be brought under `docs/standards/formatting.md`, or are they external instruction assets with their own style contract?

### [F8][GROUP_LABEL_REFERENCES_NEED_A_FIELD_PACKET_OWNER]

File: `tools/rhino-bridge/README.md`
Path and line: lines 51-70 and 141-151.
Evidence snippet/source id: `tools/rhino-bridge/README.md:54-62` uses table cells such as `[INTENT] [2]`; `tools/rhino-bridge/README.md:68` renders `[INTENT]` as a standalone group label without a colon; `tools/rhino-bridge/README.md:150` renders `[ENV]` the same way; `formatting.md:168` says standalone group labels use `[X_Y_Z]:` with a colon.
Weakness/inconsistency: The bridge README uses bracket labels as source-key packets and local group anchors, but formatting only defines colon-terminated group labels and hidden anchor comments. This pattern is useful and repeated, but it is not currently standardized as either a note, source key, footnote substitute, or field packet.
Proposed correction: If source-key packets in table cells are allowed, define them in `information-structure.md` as a form pattern and in `formatting.md` as a rendering pattern. If not allowed, bridge README should convert `[INTENT]` and `[ENV]` labels to colon-terminated group labels or normal footnotes/notes.
Active owner: `docs/standards/information-structure.md` for source-key packet form; `docs/standards/formatting.md` for bracket-label rendering.
Ripple files: `tools/rhino-bridge/README.md`; any command catalog or runbook that uses compact bracket references from table cells to later notes.
Decision: HOLD.
Proof gap/question: Is the `[GROUP] [INDEX]` table-cell reference pattern considered a durable Rasm documentation affordance or just local bridge README notation?

### [F9][TYPE_LOCAL_STATUS_VOCABULARIES_ARE_MOSTLY_DECLARED_BUT_NEED_A_REGISTRY_POINTER]

File: active standards corpus
Path and line: `information-structure.md:128-136`, `architecture.md:182-188`, `design-doc.md:43-62`, `roadmap.md:67-85`, `reference.md:141-144`, `support-matrix.md:137-165`, `onboarding.md:73-80`, `tutorial.md:146-153`.
Evidence snippet/source id: Information structure defines default lifecycle states and allows type standards to declare exact domain-specific status sets. The type standards do declare many local vocabularies, including lowercase ADR states, architecture path markers, design states, roadmap states, support labels, onboarding availability, and tutorial availability.
Weakness/inconsistency: The local vocabularies are mostly legal, but there is no single registry pointer that tells an agent where to compare type-local states against global formatting tokens. This makes it easy to mistake support labels or path markers for global lifecycle tokens.
Proposed correction: Add a short cross-reference in `formatting.md` from status/result markers to `information-structure.md`'s lifecycle vocabulary and type-local status declaration rule. Keep the detailed type-local vocabulary in each type standard.
Active owner: `docs/standards/formatting.md` for token-family distinction; `docs/standards/information-structure.md` for lifecycle declaration contract; each type standard for its own state vocabulary.
Ripple files: All type standards with local states.
Decision: MERGE.
Proof gap/question: Should `formatting.md` include a small "not a registry" note to prevent it from accumulating all type-local states?

## [EVIDENCE]

| [ID] | [PATH] | [LINE_OR_HEADING] | [SNIPPET] | [USE] |
| :---: | :--- | :--- | :--- | :--- |
| [E1] | `docs/standards/formatting.md` | 3 | Formatting owns presentation layer only. | Boundary baseline for F2. |
| [E2] | `docs/standards/formatting.md` | 23-30 | Token family table includes result, change, lifecycle, compact glyph, and explicit state rows. | Token-family baseline for F1 and F9. |
| [E3] | `docs/standards/formatting.md` | 44 | Type-local marker valid only when type standard declares closed vocabulary. | Local status legality for F9. |
| [E4] | `docs/standards/formatting.md` | 48 | Checkbox is completion assertion; `[x]` is compact fail marker. | Only explicit compact glyph mapping for F1. |
| [E5] | `docs/standards/formatting.md` | 175-180 | Contrast labels and compact record preference. | Boundary leak for F2 and field-label drift for F5. |
| [E6] | `docs/standards/formatting.md` | 208-215 | Bracketed heading idiom and rejected `# [H1][NAME]`. | Heading rule for F3 and F4. |
| [E7] | `docs/standards/information-structure.md` | 15-17 | Information structure owns table/list/record/container choice. | Boundary owner for F2. |
| [E8] | `docs/standards/information-structure.md` | 128-136 | Default lifecycle vocabulary and type-local status declaration rule. | Status owner for F9. |
| [E9] | `docs/standards/information-structure.md` | 288 | Compact contrast record routes back to formatting. | Circular owner for F2. |
| [E10] | `docs/standards/reference/readme.md` | 33-39, 71 | README heading modes allow plain public/registry headings and bracketed internal headings. | Exception for F3. |
| [E11] | `docs/standards/reference/readme.md` | 41-51 | Rejected and accepted repo-internal H1 examples are both `# [ASSAY_OPERATOR]`. | Contradiction for F4. |
| [E12] | `docs/standards/task/runbook.md` | 306-309 | Runbook contrast record uses `Why rejected:`. | Field drift for F5. |
| [E13] | `docs/standards/task/contributing.md` | 137 | Contrast labels use `Accepted`, `Rejected`, `Near miss`, `Reason`. | Shared label evidence for F5. |
| [E14] | `docs/standards/task/how-to.md` | 72 | Contrast labels use `Accepted`, `Rejected`, `Near miss`, `Reason`. | Shared label evidence for F5. |
| [E15] | `CLAUDE.md` | 27-30 | `.claude/skills/*` loaded as project skill context. | Skill instruction surface for F6. |
| [E16] | `.claude/skills/coding-bash/SKILL.md` | 1, 10 | `# [H1][CODING-BASH]`; `[CRITICAL]` marker. | Legacy heading and invocation marker evidence for F6 and F7. |
| [E17] | `.claude/skills/coding-csharp/references/concurrency.md` | 321, 342, 348-365 | `[CRITICAL]`, `[IMPORTANT]`, `[ALWAYS]`, `[NEVER]` markers. | Invocation marker evidence for F6. |
| [E18] | `.claude/prompts/assay-ab-test-session.md` | 5 | `## [1] WHERE / WHAT / STATE`. | Legacy prompt heading evidence for F7. |
| [E19] | `.claude/skills/coding-bash/references/array-operations.md` | 5 | Table headers include `[IDX]` and `[S]`. | Legacy compact table rubric evidence for F7. |
| [E20] | `tools/rhino-bridge/README.md` | 54-68, 150 | Table-cell references `[INTENT] [2]`; standalone `[INTENT]` and `[ENV]` labels without colon. | Field-packet/source-key evidence for F8. |

## [RECOMMENDATIONS]

| [ORDER] | [ACTION] | [OWNER] | [DECISION] |
| :---: | :--- | :--- | :---: |
| [1] | Add or demote the compact glyph map so glyphs cannot carry implicit per-author semantics. | `docs/standards/formatting.md` | CORRECT |
| [2] | Move contrast-record form rules out of formatting and into information structure; leave rendering rules in formatting. | `docs/standards/information-structure.md` and `docs/standards/formatting.md` | CORRECT |
| [3] | Narrow heading idiom wording to internal controlled docs and route README external-reader exceptions. | `docs/standards/formatting.md` | CORRECT |
| [4] | Fix the contradictory README heading example. | `docs/standards/reference/readme.md` | CORRECT |
| [5] | Rename runbook `Why rejected:` to `Reason:` or declare the runbook-local alias. | `docs/standards/task/runbook.md` | CORRECT |
| [6] | Promote `.claude/skills/**` as an allowed invocation-marker surface if those files remain agent instructions. | `docs/standards/formatting.md` | PROMOTE |
| [7] | Keep legacy prompt/skill packet migration out of `formatting.md` unless the repo decides `.claude` assets are active standards-governed docs. | `.claude` prompt and skill owners | HOLD |
| [8] | Decide whether `[GROUP] [INDEX]` source-key packets are a shared documentation pattern; standardize or route away. | `docs/standards/information-structure.md` and `docs/standards/formatting.md` | HOLD |
| [9] | Add a status-vocabulary registry pointer from formatting to information-structure and type standards. | `docs/standards/formatting.md` | MERGE |

## [PROOF_GAPS]

| [INDEX] | [GAP] | [OWNER] |
| :---: | :--- | :--- |
| [1] | Target report lane had no pre-existing lane manifest to read before this file was created. | `.reports` archive owner |
| [2] | Compact glyph meanings require an owner decision before correction can be exact. | `docs/standards/formatting.md` |
| [3] | Skill file governance needs a repo decision: active instruction surface versus external imported material. | `CLAUDE.md`, `docs/standards/formatting.md`, `.claude` owners |
| [4] | `[GROUP] [INDEX]` source-key packets need a form owner before formatting can define rendering without leaking structure. | `docs/standards/information-structure.md` |

## [VALIDATION]

Status: COMPLETE
Command: `git diff --check -- .reports/standards-structure-notation-060626/track-formatting-notation/02-formatting-cross-corpus-usage.md`
Result: [PASS]
