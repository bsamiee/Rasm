Question: Does the collective standards task list still have proof gaps, weak correction shapes, unresolved choices, or report-frame leakage risk after the owner-order review?
Type: gap-critique
Lane: track-synthesis
Merge key: track-synthesis/00-collective-task-list.md :: review gap critique :: hold until corrected
Target owner: `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
Source basis: mandatory instruction chain, session manifest, every report file under `docs/standards/_reports/standards-structure-notation-060626/`, `track-synthesis/01-review-owner-order.md`, and active owner standards needed for proof, formatting, information-structure, and style verification.
Promotion target: `track-synthesis/00-collective-task-list.md`
Outcome: HOLD

Prior lane extension: extends `track-synthesis/01-review-owner-order.md` by checking accepted-task implementability, hidden choices, report-frame exclusion, and weak correction shapes after the owner-order pass.

## [SOURCE_SET_READ]

[INSTRUCTIONS]:
- `CLAUDE.md`
- root `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/_reports/AGENTS.md`

[SESSION_REPORTS]:
- `docs/standards/_reports/standards-structure-notation-060626/README.md`
- `track-adversarial-synthesis/01-owner-routing-critic.md`
- `track-adversarial-synthesis/02-report-boundary-critic.md`
- `track-adversarial-synthesis/03-information-structure-critic.md`
- `track-adversarial-synthesis/04-formatting-critic.md`
- `track-adversarial-synthesis/05-style-guide-critic.md`
- `track-adversarial-synthesis/06-proof-critic.md`
- `track-adversarial-synthesis/07-agentic-documentation-critic.md`
- `track-adversarial-synthesis/08-transcript-boundary-critic.md`
- `track-external-research/01-gfm-github-markdown-capabilities.md`
- `track-external-research/02-mermaid-renderer-proof.md`
- `track-external-research/03-information-architecture-structure-chooser-a.md`
- `track-external-research/04-information-architecture-structure-chooser-b.md`
- `track-external-research/05-tables-matrices-decision-records.md`
- `track-external-research/06-status-taxonomy-vocabularies.md`
- `track-external-research/07-proof-uncertainty-stale-sources.md`
- `track-external-research/08-compact-notation-sigils-glyphs.md`
- `track-external-research/09-code-fence-intent-labels.md`
- `track-external-research/10-machine-consumed-markdown-ledgers.md`
- `track-formatting-notation/01-formatting-token-systems.md`
- `track-formatting-notation/02-formatting-cross-corpus-usage.md`
- `track-information-structure/01-information-structure-container-toolbelt.md`
- `track-information-structure/02-information-structure-cross-type-fit.md`
- `track-repo-markdown/01-claude-markdown-patterns.md`
- `track-repo-markdown/02-non-standards-markdown-patterns.md`
- `track-root-corpus/01-root-shared-corpus.md`
- `track-synthesis/00-collective-task-list.md`
- `track-synthesis/01-review-owner-order.md`
- `track-type-corpus/01-explanation-type-corpus.md`
- `track-type-corpus/02-reference-type-corpus.md`
- `track-type-corpus/03-task-learning-type-corpus.md`

[ACTIVE_STANDARDS_CHECKED]:
- `docs/standards/README.md`: active authority and evidence split at lines 14-30; owner routing at lines 41-50.
- `docs/standards/information-structure.md`: current chooser lacks input-first signatures at lines 15-72; closed status defaults and record rules sit at lines 122-180; packet names are already present at lines 336-349; machine-consumed Markdown is still late at lines 481-488; page anatomy mixes jobs at lines 498-562.
- `docs/standards/formatting.md`: compact glyphs and bracket tokens remain ambiguous at lines 17-50 and 94-115; contrast-record shape still lives in formatting at lines 175-180; heading idiom is broad at lines 206-215.
- `docs/standards/proof.md`: proof gaps and gate claims route through proof fields and docs-code verification at lines 41-67 and 136-165.
- `docs/standards/style-guide.md`: prose-to-structure decisions and process-language rejection route through lines 43-58 and 183-206.
- `docs/standards/agentic-documentation.md`: accepted/rejected mini-table, provider-proof split, and prompt/process containment remain active concerns at lines 79-84, 120-133, and 211-247.
- `docs/standards/agents-md.md`: provider-loading behavior and report-boundary claims remain proof-sensitive at lines 38-52, 91, 123-127, and 195-203.
- Type-standard spot checks: `reference/readme.md` has the H1 contradiction at lines 41-51; `reference/support-matrix.md` has imported lifecycle field names at lines 117-131; `reference/code-documentation.md` has target language baselines at lines 68-72 and 169-305; `learning/tutorial.md` routes status vocabularies at lines 340-355; `explanation/architecture.md` has the Mermaid indentation defect at lines 260-280.

## [MISSING_FINDINGS]

[M1][ACCEPTED_TASKS_NEED_READY_MARKERS]:
Finding: `00-collective-task-list.md` still marks user-choice-bearing rows as `OPEN`, not blocked implementation rows. `Promote closed vocabulary cards` at lines 158-170 includes display-versus-machine fields, unknown-value behavior, compact glyph projection, and classification locality while its proof gap says user choices remain. `Clarify absence values across table cells, record fields, source unknowns, and domain none` at lines 412-424 is also `OPEN` while support `n/a` semantics remain undecided.
Correction: split each row into an implementable core and a `BLOCKED_USER_CHOICE` tail. An active-standard implementer should be able to run the list without deciding glyph policy, unknown cardinality, classification locality, or support-specific `n/a` semantics.

[M2][REPORT_FRAME_EXCLUSION_NEEDS_A_CLOSE_CHECK]:
Finding: the current task list has a good source-scan boundary task at lines 724-736, but it does not name the report-only headings that still exist in source reports. Examples include `[SOURCE_SET_READ]` in `track-external-research/02-mermaid-renderer-proof.md:10`, `[VALIDATION]` in `track-external-research/02-mermaid-renderer-proof.md:217` and `track-formatting-notation/02-formatting-cross-corpus-usage.md:221`, `[USER_CHOICES]` in `track-external-research/03-information-architecture-structure-chooser-a.md:184`, `[SYSTEM_CATALOG]` in `track-root-corpus/01-root-shared-corpus.md:78`, and `[CROSS_FILE_CATALOGUE]` in `track-type-corpus/03-task-learning-type-corpus.md:96`.
Correction: add an explicit close check that no active-standard edit copies report headings, report validation frames, source inventories, worker roles, confidence labels, or transcript/process names. `_reports/AGENTS.md:97-105` and `139-159` already own this rejection.

[M3][CURRENT_STALE_REPORT_CLAIMS_NEED_DISPOSITION]:
Finding: `track-adversarial-synthesis/02-report-boundary-critic.md:15-18` still cites old task-list and manifest wording with wave/review-agent terms that the current task list has already replaced at `00-collective-task-list.md:5-6`. This is not an active-standard defect, but it can mislead later agents if cited without the current task-list proof.
Correction: the task list or session manifest should mark those exact report-boundary findings `SUPERSEDED` for current line references while preserving the durable rule: do not promote wave, worker, transcript, or report-validation frames.

[M4][PROOF_CLOSE_FOR_REPORT_SESSION_TOO_BROAD]:
Finding: `Validate report-session files` at `00-collective-task-list.md:814-826` asks for `git diff --check -- docs/standards/_reports/standards-structure-notation-060626`. That is correct for a whole-session close, but a review agent with a single-file write scope cannot safely own unrelated report files.
Correction: add a scoped variant for review agents: run `git diff --check -- <written report path>`. Whole-session validation remains a main-agent or broader-scope task.

## [DUPLICATE_OR_OVER_BROAD_TASKS]

[D1][GLYPH_DUPLICATE_WITH_READY_LEAK]:
Tasks: `Resolve compact glyph semantics` at `00-collective-task-list.md:356-368` and `Resolve compact glyph alphabet` at lines 756-768.
Problem: these duplicate the same blocker, and `Promote closed vocabulary cards` at lines 158-170 still lets compact glyph projection remain inside a nominally `OPEN` information-structure task.
Correction: keep one glyph decision row. All ready rows must say glyph projection is held; none may define `[?]` as proof gap because `Proof gap:` is the only shared missing-evidence marker.

[D2][PACKET_CATALOG_PRESSURE]:
Tasks: `Add owner boundary and packet-promotion tests before packet catalogs` at lines 130-142, `Add no-table-for-packet-catalog and subchooser placement rules` at lines 144-156, `Promote field packet, proof record, API field card, and CLI envelope schemas` at lines 214-226, and `Add generated-ledger packet` at lines 298-310.
Problem: the first two tasks correctly resist packet sprawl, but later tasks can still create named packet peers. Active `information-structure.md:336-349` already lists packet names, so implementation should reduce and route them, not expand a catalog.
Correction: merge into one carrier-family correction: define a packet-promotion threshold, then specify only the packet families that pass it. Generated ledgers belong under the machine-consumed/generated exception, with proof semantics owned by `proof.md`.

[D3][TABLE_SIDE_CAR_AND_NON_STANDARDS_RIPPLE]:
Tasks: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules` at lines 242-254 and `Resolve row-sidecar label pattern` at lines 784-796.
Problem: the first row names sidecar constraints and non-standards ripple candidates while the second says sidecar syntax is still a user choice. This also conflicts with the sequence hold at lines 740-752.
Correction: implement table eligibility, decomposition triggers, and row-owned-record defaults first. Hold sidecar syntax and non-standards ripple.

[D4][CLAUDE_GOVERNANCE_ROWS]:
Tasks: `.claude/skills/**` invocation markers at lines 398-410, `.claude/prompts/**` and `.claude/skills/**` governance at lines 710-722, and `.claude` boundary resolution at lines 798-810.
Problem: three rows carry one user choice plus one provider-proof gate.
Correction: merge into one `HOLD_USER_CHOICE` row with two independent blockers: user governance scope and maintained provider-format proof.

## [INCORRECT_OWNER_ROUTES]

[O1][CLOSED_VOCAB_OWNER_SPLIT]:
Task: `Promote closed vocabulary cards` at `00-collective-task-list.md:158-170`.
Incorrect route: owner is only `information-structure.md`.
Correct route: `information-structure.md` owns declaration threshold and card form; `formatting.md` owns rendered markers and glyphs; `proof.md` owns evidence fields, proof gaps, freshness, and review triggers; type standards own concrete vocabularies.

[O2][ABSENCE_VALUES_SHOULD_NOT_ROUTE_TO_AGENTS]:
Task: `Clarify absence values across table cells, record fields, source unknowns, and domain none` at lines 412-424.
Incorrect route: includes `docs/standards/AGENTS.md` as an owner.
Correct route: `formatting.md` owns table-cell rendering; `information-structure.md` owns omitted fields and record shape; `proof.md` owns proof uncertainty; type standards own domain values like runbook `Safe mutation: none` and support-matrix `n/a`.

[O3][SOURCE_SCAN_RECORD_REPORT_OWNER]:
Task: `Correct source-scan record principles without prompt process choreography` at lines 724-736.
Incorrect route: active owners and `_reports/AGENTS.md` are grouped together.
Correct route: `_reports/AGENTS.md` owns report identity fields and report-only frames. Active standards may receive only generic source-provenance or field-packet rules after a non-report active consumer is proved.

## [WEAK_CORRECTION_SHAPES]

[W1][INPUT_CHOOSER_NEEDS_AN_INPUT_SIGNATURE_NOT_A_TABLE_CHOICE]:
Task: `Add an input-first information signature chooser` at lines 116-128.
Weakness: `table or grouped definition block` leaves the implementer to choose the container and can recreate the wide chooser problem the reports reject.
Correction shape: use a grouped definition block keyed by raw input signature, with each entry routing to one common carrier or one local subchooser.

[W2][VOCABULARY_CARD_TOO_LARGE]:
Task: `Promote closed vocabulary cards` at lines 158-170.
Weakness: the correction lists many fields but does not identify the minimum required card. It risks forcing lifecycle, machine/display, unknown, projection, evidence, and review fields onto every small local vocabulary.
Correction shape: require `Vocabulary`, `Applies to`, `Values`, `Default or initial value when needed`, `Removal behavior`, and `Owner`; add lifecycle groups, machine/display split, unknown-value rule, evidence, and review trigger only when source proof or type behavior requires them.

[W3][COMMAND_OUTPUT_SURFACE_NEEDS_PROOF_SPLIT]:
Task: `Define command/output surfaces and callable contract packets` at lines 200-212.
Weakness: the correction defines shapes but does not force the split between illustrative command syntax, expected signal, observed output, and executed-command proof.
Correction shape: every command/output carrier should name whether it is `copy-safe command`, `expected signal`, `observed output`, `failure-reading rule`, or `executed proof`. Executed proof routes to `proof.md`; illustrative command syntax stays in information structure/style.

[W4][VALIDATION_ROWS_NEED_SCOPE]:
Tasks: `Validate report-session files` at lines 814-826 and `Validate active standards after implementation` at lines 828-840.
Weakness: both are correct but too broad for worker-scoped report edits. A single-file report pass must not imply whole-session or active-standard validation.
Correction shape: validation row should require exact command, scope, and claim class. For this report, the valid close command is only `git diff --check -- docs/standards/_reports/standards-structure-notation-060626/track-synthesis/02-review-gap-critique.md`.

## [SEQUENCING_PROBLEMS]

[S1][READY_ROWS_BEFORE_BLOCKED_ROWS]:
Problem: implementation should not begin from the mixed list at `00-collective-task-list.md:1-840`. `track-synthesis/01-review-owner-order.md:67-85` already finds that the ready queue is missing, and this pass confirms several nominally `OPEN` rows still hide choices.
Required order: revise the task list into ready, held, dropped, later-ripple, and validation groups before active standards are edited.

[S2][MACHINE_CONSUMED_EXCEPTION_MUST_PRECEDE_NORMALIZATION]:
Problem: `Move machine-consumed Markdown earlier and name parser-owned exceptions` at lines 284-296 is sequenced after multiple record, table, packet, and topology tasks. Active `information-structure.md:481-488` currently puts machine-consumed Markdown late under callouts/details/footnotes.
Required order: promote the machine-consumed exception before code-fence, heading, table, field-packet, generated-ledger, glyph, and proof-field normalization.

[S3][PROOF_BEFORE_FORMATTING_RIPPLE]:
Problem: formatting tasks depend on proof semantics. Compact glyphs, absence values, renderer support, configured gates, proof-bearing templates, and stale-source rules cross `formatting.md` and `proof.md`.
Required order: land `proof.md` changes for proof gaps, configured gates, stale-source terms, and renderer/source-class packets before formatting examples claim renderer behavior, proof closure, unknown states, or validation gates.

## [TASKS_NEEDING_USER_CHOICE_BEFORE_IMPLEMENTATION]

[U1][COMPACT_GLYPH_POLICY]:
Task refs: `Resolve compact glyph semantics` at lines 356-368 and `Resolve compact glyph alphabet` at lines 756-768.
Choice needed: minimal global `[x]` plus local declarations, full global glyph map, or ASCII-first local policy. `[?]` may not mean proof gap.

[U2][BRACKET_SUFFIX_GRAMMAR]:
Task refs: `Add type-local bracket token and status registry pointer` at lines 370-382 and `Resolve bracket-token suffix grammar` at lines 770-782.
Choice needed: codemap/source-key-only suffixes, declared type-local suffixes with projection, or no shared suffix grammar.

[U3][ROW_SIDE_CAR_PATTERN]:
Task refs: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules` at lines 242-254 and `Resolve row-sidecar label pattern` at lines 784-796.
Choice needed: source-scannable sidecars, Markdown footnotes, or row-owned records. Until chosen, use row-owned records for proof/update/removal detail.

[U4][CLAUDE_GOVERNANCE]:
Task refs: `.claude/skills/**` invocation markers at lines 398-410, `.claude` ripple boundary at lines 710-722, and `.claude` governance at lines 798-810.
Choice needed: govern `.claude/prompts/**` and `.claude/skills/**` under standards, or keep them as source material only. Provider skill-format proof remains separate.

[U5][README_LATE_EXAMPLES]:
Task ref: `Move or reframe README late examples` at lines 606-618.
Choice needed: move local misuse examples beside controlling rules, or preserve late examples as explicitly named reader-route examples.

[U6][SUPPORT_NA_DOMAIN]:
Task ref: `Clarify absence values across table cells, record fields, source unknowns, and domain none` at lines 412-424.
Choice needed: whether support `n/a` shares the domain-`none` rule or stays support-specific.

## [TASKS_TO_DROP_WITH_REASON]

[DROP1][TEXT_TRANSCRIPT_GLOBAL_INTENT]:
Task: `Drop text transcript as a global fence intent` at lines 340-352.
Reason: already correctly dropped for active standards. Keep only as a false-positive guard and optional `_reports/**` exception question, not as an active edit.

[DROP2][ROADMAP_PROGRESS_FALSE_POSITIVE]:
Task: `Preserve roadmap progress as canonical progress example` at lines 576-588.
Reason: already `DROPPED_NO_DEFECT`; no active edit.

[DROP3][TASK_LOCAL_STEP_MARKERS]:
Task: `Keep task-local step markers local` at lines 664-676.
Reason: already `DROPPED_TYPE_LOCAL`; no global notation promotion.

[DROP4][STANDALONE_CLASSIFICATION_TABLE]:
Source report task family: classification-table promotion from `track-information-structure/01-information-structure-container-toolbelt.md:75-80` and `track-external-research/04-information-architecture-structure-chooser-b.md:89-107`.
Reason: fold into lookup-table profile unless later active-corpus proof shows a separate carrier is necessary.

[DROP5][TOPOLOGY_STATUS_BUNDLE_AS_CARRIER]:
Source report task family: topology-status bundle from `track-repo-markdown/02-non-standards-markdown-patterns.md:29` and adversarial drop recommendation at `track-adversarial-synthesis/03-information-structure-critic.md:161-163`.
Reason: replace with topology representation plus adjacent status/proof record when both are needed; do not promote a named carrier.

[DROP6][REPORT_VALIDATION_FRAME_PROMOTION]:
Source reports: report-local headings and validation boilerplate named under `[M2]`.
Reason: reports may keep source evidence, but active standards must not inherit report `VALIDATION`, source-set inventory, confidence, system-catalog, worker, wave, transcript, or no-change frames.

## [FINAL_RECOMMENDATION]

Recommendation: HOLD.

Reason: the synthesis is close but not yet implementable as an active-standard edit contract. It still contains nominally open tasks with hidden user choices, duplicate rows for glyphs, packets, `.claude`, and sidecars, weak correction shapes that can expand packet/table catalogs, stale report-boundary line references, and validation scopes wider than a single review agent can own.

Top findings:
- The list needs ready/held/drop/later-ripple groups before active standards are edited.
- Machine-consumed Markdown and proof-owner rules must land before ordinary formatting, fence, table, glyph, generated-ledger, and validation normalization.
- Compact glyphs, bracket suffixes, row sidecars, `.claude` governance, README late examples, and support `n/a` semantics require user choices.
- Report frames must remain source-only; do not promote `[SOURCE_SET_READ]`, `[VALIDATION]`, worker roles, wave/process language, transcript sections, or no-change receipts.
- Drop standalone classification-table and topology-status-bundle carrier names; fold them into lookup/table profiles and topology-plus-record job splits.

Close proof:
- `git diff --check -- docs/standards/_reports/standards-structure-notation-060626/track-synthesis/02-review-gap-critique.md` passed.

Proof gaps:
- No active standards were edited.
- No Mermaid render, link check, anchor check, docs build, provider lookup, command-behavior proof, or whole-session report validation ran in this review.
