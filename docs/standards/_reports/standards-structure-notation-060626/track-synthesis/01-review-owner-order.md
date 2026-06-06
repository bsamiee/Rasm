Question: Should the collective standards task list proceed to active-standard implementation, and which owner-order corrections must happen first?
Type: synthesis
Lane: track-synthesis
Merge key: track-synthesis/00-collective-task-list.md :: owner order and implementation sequence :: hold until corrected
Target owner: `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
Source basis: mandatory instruction chain, session manifest, every report file under `docs/standards/_reports/standards-structure-notation-060626/`, active owner standards needed for route verification, and current repo file inventory.
Promotion target: `track-synthesis/00-collective-task-list.md`
Outcome: HOLD

Prior lane extension: extends `track-adversarial-synthesis/01-owner-routing-critic.md`, `track-adversarial-synthesis/02-report-boundary-critic.md`, and `track-adversarial-synthesis/03-information-structure-critic.md` by turning their owner-route findings into a task-list acceptance decision and implementation order.

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
- `track-type-corpus/01-explanation-type-corpus.md`
- `track-type-corpus/02-reference-type-corpus.md`
- `track-type-corpus/03-task-learning-type-corpus.md`

[ACTIVE_OWNER_STANDARDS]:
- `docs/standards/README.md`: active-corpus boundary, route map, split/link, lifecycle, prompt/process anti-patterns at lines 14-30, 86-99, 114-140, 160, and 174-176.
- `docs/standards/AGENTS.md`: report-source boundary, owner table, proof close, and process-language rejections at lines 9-11, 29-42, 56, 78-105, and 114-136.
- `docs/standards/_reports/AGENTS.md`: report-only status, lane mechanics, promotion, pruning, and report-frame rejections at lines 3-17, 51-63, 82-105, 139-153, and 155-160.
- `docs/standards/information-structure.md`: carrier choice, record field order, relation records, machine-consumed Markdown, representation job split, and page anatomy at lines 15-71, 122-180, 438-454, 481-488, and 498-562.
- `docs/standards/formatting.md`: marker rendering, invocation markers, field-line contrast records, heading idiom, and notation boundaries at lines 1-15, 17-50, 101-115, 175-180, 206-215, and 229-257.
- `docs/standards/proof.md`: proof fields, proof gaps, evidence hierarchy, docs-code gate selection, and renderer/provider proof at lines 1-16, 18-67, 136-165, and 262-284.
- `docs/standards/agentic-documentation.md`: artifact separation, task-output contracts, provider-proof split, and prompt/process containment at lines 90-133, 209-247, and 260-289.
- `docs/standards/agents-md.md`: `_reports/**` trust boundary, report companion route, provider-loading proof, and no process metadata at lines 38-52, 91, 123-127, 195-203, 238-245, and 263-266.
- `docs/standards/style-guide.md`: transient process language and example/prose ownership at lines 43-47, 145-157, and 183-206.
- Type standards checked for current route examples: `reference/readme.md` lines 33-51 and 405-457; `learning/tutorial.md` lines 146-153 and 340-360; `task/runbook.md` lines 32-49, 209-220, and 296-310.

## [MISSING_FINDINGS]

[M1][IMPLEMENTABLE_QUEUE_MISSING]:
Finding: `track-synthesis/00-collective-task-list.md` is open at line 3 and contains ready tasks, held tasks, dropped tasks, research blockers, sequence blockers, and validation tasks in one list. It does not provide a ready implementation queue that excludes `BLOCKED_*`, `DROPPED_*`, and later-ripple rows before active edits begin.
Correction: add a synthesis-only queue with groups `READY_NOW`, `MERGE_FIRST`, `HOLD_USER_CHOICE`, `HOLD_RESEARCH`, `DROP_NO_EDIT`, and `LATER_RIPPLE`. This is an update to `00-collective-task-list.md`, not an active-standard change.
Evidence: session manifest says promotion happens only after task-list review and user choices at `README.md:73-75`; `_reports/AGENTS.md` defines `HOLD` and `DROP` outcomes at lines 57-63.

[M2][ACTIVE_SOURCE_SCAN_GATE_MISSING]:
Finding: several tasks correctly say exact ripple files require source-span verification, but no single task owns the post-shared-edit scan that decides which type standards are touched. Examples: relation preambles at `00-collective-task-list.md:68-80`, page anatomy at `00-collective-task-list.md:312-324`, tutorial status route-away at `00-collective-task-list.md:680-692`, and code-fence labels at `00-collective-task-list.md:326-338`.
Correction: add a source-scan task after shared-owner edits and before any type-standard ripple. The task should name `rg` patterns and state that the scan is evidence for path selection only, not a renderer, link, command, or provider proof.
Owner route: `track-synthesis/00-collective-task-list.md`; proof semantics route to `docs/standards/proof.md`.

[M3][PROMPT_PROCESS_LEAK_SCAN_MISSING]:
Finding: the task list now avoids most wave and worker wording, but it still references `track-adversarial-synthesis/08-transcript-boundary-critic.md` at `00-collective-task-list.md:345` and carries prompt-process source material in the source-scan task at `00-collective-task-list.md:724-736`. There is no close check that active standards remain free of prompt/process terms after implementation.
Correction: add a final active-standard source scan for `wave`, `review-agent`, `sub-agent`, `transcript-boundary`, `prompt choreography`, `worker role`, and `confidence` under `docs/standards --glob '!_reports/**'`. Any hit must be a report path, rejected archival example, or explicit source-material boundary.
Owner route: `style-guide.md` carries transient process-language rejection; `agentic-documentation.md` carries artifact separation; `_reports/AGENTS.md` carries report-frame rejection.

[M4][PROVIDER_SPLIT_IS_NOT_SEQUENCED]:
Finding: `Split provider-independent contract guidance from provider-specific schema mechanics` at `00-collective-task-list.md:100-112` combines an implementable contract-strength boundary with held provider-specific mechanics. Its status is `OPEN_WITH_HELD_PROVIDER_MECHANICS`, which is not enough to stop a later edit from inserting unproved provider behavior.
Correction: split into two tasks: one `READY_NOW` task for contract-strength separation without provider-current claims, and one `HOLD_RESEARCH` task for supported schema keywords, delimiter behavior, provider loading, and enforcement availability.
Owner route: `agentic-documentation.md` owns the contract split; `proof.md` owns provider proof and freshness; `agents-md.md` owns `AGENTS.md` loading claims only when current proof exists.

## [DUPLICATE_OR_OVER_BROAD_TASKS]

[D1][COMMAND_PACKET_FAMILY_OVERLAP]:
Tasks: `Define command/output surfaces and callable contract packets` at `00-collective-task-list.md:200-212`, `Promote field packet, proof record, API field card, and CLI envelope schemas` at `00-collective-task-list.md:214-226`, and `Add generated-ledger packet` at `00-collective-task-list.md:298-310`.
Problem: these split one carrier family into several peer packet tasks. The active `information-structure.md` owns carrier choice, while `proof.md` owns evidence labels and command-output proof.
Correction: merge them into one `COMMAND_OUTPUT_AND_GENERATED_SURFACES` subchooser task. Keep generated-ledger wording under the machine-consumed/generated exception, not as an ordinary packet catalog.

[D2][TABLE_SIDE_CAR_RIPPLE_OVERREACH]:
Tasks: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules` at `00-collective-task-list.md:242-254` and `Resolve row-sidecar label pattern` at `00-collective-task-list.md:784-796`.
Problem: the first task names non-standards ripple files and sidecar work while the second states sidecar syntax is still a user choice. This conflicts with `Hold broad repo Markdown ripple until standards wording lands` at `00-collective-task-list.md:740-752`.
Correction: implement table decomposition and row-owned-record rules first. Hold sidecar rendering and all non-standards Markdown ripple until the row-sidecar user choice closes.

[D3][CLAUDE_GOVERNANCE_DUPLICATION]:
Tasks: `Add .claude/skills/** to invocation-marker surface only if governed as agent instructions` at `00-collective-task-list.md:398-410`, `Decide whether .claude/prompts/** and .claude/skills/** are standards-governed ripple surfaces` at `00-collective-task-list.md:710-722`, and `Resolve .claude governance boundary` at `00-collective-task-list.md:798-810`.
Problem: these are the same governance decision expressed in 3 locations. Keeping all 3 invites partial implementation of `.claude/**` before the user choice and provider-format proof close.
Correction: merge into one `HOLD_USER_CHOICE` task. If accepted, route the governing boundary to `.claude/README.md`, marker rendering to `formatting.md`, provider mechanics to `proof.md`, and report-source evidence to the reports only.

[D4][GLYPH_CHOICE_DUPLICATION]:
Tasks: `Resolve compact glyph semantics` at `00-collective-task-list.md:356-368` and `Resolve compact glyph alphabet` at `00-collective-task-list.md:756-768`.
Problem: these duplicate the same blocker with slightly different wording. The task list also lets vocabulary-card work mention compact glyph projection at `00-collective-task-list.md:158-170`, so glyph policy can leak into a supposedly ready information-structure task.
Correction: keep one `HOLD_USER_CHOICE` task for compact glyph policy. Ready tasks may say glyph projection is held; they must not define `[?]`, `[x]`, or any compact alphabet semantics.

[D5][TYPE_EXAMPLE_RESTATEMENT]:
Task: `Use onboarding and tutorial as positive narrowed-status examples` at `00-collective-task-list.md:694-706`.
Problem: the task has no required active edit unless a shared example is deliberately added. It should not touch type standards only to restate already-valid local vocabularies.
Correction: mark as evidence for the shared closed-vocabulary boundary or drop as an implementation task.

## [INCORRECT_OWNER_ROUTES]

[O1][CLOSED_VOCABULARY_CROSS_OWNER]:
Task: `Promote closed vocabulary cards` at `00-collective-task-list.md:158-170`.
Problem: the owner is only `information-structure.md` and the axis is `notation`, but the correction includes proof fields, machine/display value split, glyph projection, unknown-value behavior, lifecycle groups, and type-local status terms.
Correct owner route: `information-structure.md` owns the vocabulary-card carrier and declaration threshold; `formatting.md` owns marker rendering; `proof.md` owns `Evidence:`, `Proof gap:`, `Last verified:`, and `Review trigger:` semantics; type standards own their actual status values.

[O2][SOURCE_SCAN_RECORD_OVERPROMOTES_REPORT_SHAPE]:
Task: `Correct source-scan record principles without prompt process choreography` at `00-collective-task-list.md:724-736`.
Problem: it names `agentic-documentation.md`, `information-structure.md`, and `_reports/AGENTS.md` together as owners. That can copy a `.claude` prompt/report field contract into active standards.
Correct owner route: `_reports/AGENTS.md` owns report identity fields, merge keys, outcomes, corrections, promotion maps, and pruning. `agentic-documentation.md` may receive generic source-provenance guidance. `information-structure.md` should receive a generic field-packet carrier only if a non-report active consumer is proved.

[O3][INVOCATION_MARKER_SURFACE_DEPENDS_ON_GOVERNANCE]:
Task: `Add .claude/skills/** to invocation-marker surface only if governed as agent instructions` at `00-collective-task-list.md:398-410`.
Problem: `formatting.md` owns invocation marker rendering, not the governance decision that skill files become standards-governed instruction surfaces.
Correct owner route: `.claude/README.md` or a trusted instruction owner must first decide local governance; `proof.md` must carry provider-format proof; then `formatting.md` may render invocation markers for accepted surfaces.

[O4][ABSENCE_VALUES_SHOULD_NOT_BECOME_AGENTS_POLICY]:
Task: `Clarify absence values across table cells, record fields, source unknowns, and domain none` at `00-collective-task-list.md:412-424`.
Problem: the task includes `docs/standards/AGENTS.md` as an owner. `AGENTS.md` can reject filler fields during standards work, but it should not become the global owner for absence rendering or domain `none` semantics.
Correct owner route: `formatting.md` owns table-cell absence rendering; `information-structure.md` owns omitted record fields; `proof.md` owns unknown/unverified claim gaps; type standards own domain-specific `none` values such as runbook `Safe mutation: none`.

## [WEAK_CORRECTION_SHAPES]

[W1][INPUT_CHOOSER_SHAPE_TOO_OPEN]:
Task: `Add an input-first information signature chooser` at `00-collective-task-list.md:116-128`.
Weakness: `table or grouped definition block` still permits a wide table or packet catalog. `track-adversarial-synthesis/03-information-structure-critic.md:14-44` narrows this to a compact chooser with subchoosers.
Correction shape: grouped definition block with short input-signature rows; each row chooses one carrier or routes to one subchooser.

[W2][TABLE_DECOMPOSITION_PACKETS_TOO_BROAD]:
Task: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules` at `00-collective-task-list.md:242-254`.
Weakness: `table-form chooser expansion plus decomposition packets` can become more table profiles instead of a table-spam guard.
Correction shape: table eligibility rule first, then decomposition thresholds; row-owned records are the default for proof, lifecycle, update, and removal facts until row-sidecar user choice closes.

[W3][RENDERER_SUPPORT_PACKET_SHAPE_UNDECIDED]:
Task: `Add renderer-support proof packet` at `00-collective-task-list.md:442-454`.
Weakness: `proof record or compact table` risks adding a new proof packet when `proof.md` already owns human-facing proof fields and docs-code gate classes.
Correction shape: proof-owned gate record keyed by `Gate class`, `Configured command or source`, `Applies to`, `Proves`, `Does not prove`, `Proof gap`, and `Review trigger`; use a table only when comparing several configured gates.

[W4][VALIDATION_TASKS_NEED STATES_NOT_PROMISES]:
Task: `Validate active standards after implementation` at `00-collective-task-list.md:828-840`.
Weakness: the task is correct in direction but should not be part of ready implementation until changed files are known. It also cannot imply a link, anchor, docs-build, renderer, or provider gate exists.
Correction shape: final close records use exactly one of `ran <exact command>`, `not applicable because <claim class did not change>`, or `Proof gap: no configured <gate class> exists`.

## [SEQUENCING_PROBLEMS]

[S1][FULL_LIST_IMPLEMENTATION_SHOULD_HOLD]:
Problem: the session manifest says durable promotion waits for source research, adversarial critique, task-list review, user choices, active-standard edits, and validation at `README.md:73-75`. The current task list still contains multiple user-choice and research blockers, so full-list implementation should not start.
Required order: correct `00-collective-task-list.md` first; do not edit active standards from the current mixed list.

[S2][READY_SHARED_OWNER_ORDER]:
Recommended sequence after task-list correction:
1. Report-session corrections only: current file and any task-list owner-order updates.
2. `README.md` active-corpus boundary only if the exact current sentence remains ambiguous.
3. `information-structure.md` carrier chooser, packet-promotion test, control-record subchooser, command/output subchooser, table-spam guard, machine-consumed exception, and page-anatomy split, excluding blocked glyph, sidecar, and provider-specific semantics.
4. `proof.md` proof-field, gate-class, renderer/provider proof, stale-source, and command-output evidence rules.
5. `formatting.md` rendering-only edits for headings, contrast labels, absence values, and invocation markers, excluding compact glyph and `.claude` skill governance until user choice closes.
6. `agentic-documentation.md` and `agents-md.md` only for artifact separation, contract-strength split, and provider-proof gaps without unverified provider behavior.
7. Type-standard exact fixes: README H1 contradiction, runbook `Reason:` label, architecture Mermaid indentation, pure field-packet fence labels, and only path-specific relation/status edits proved by active-source scan.
8. Non-standards repo Markdown ripple only after shared standards land and user choices close.

[S3][BLOCKED_TASKS_MUST_NOT_BE_CO_IMPLEMENTED]:
Problem: blocked tasks are interleaved with ready ones: provider mechanics at `00-collective-task-list.md:100-112` and `530-542`, glyphs at `356-368` and `756-768`, `.claude` governance at `398-410`, `710-722`, and `798-810`, language baselines at `620-632`, support imported fields at `634-646`, row sidecars at `784-796`, and broad repo ripple at `740-752`.
Correction: copy those rows into a `HOLD` group before active edits begin.

## [TASKS_NEEDING_USER_CHOICE_BEFORE_IMPLEMENTATION]

[U1][COMPACT_GLYPH_POLICY]:
Task refs: `Resolve compact glyph semantics` at `00-collective-task-list.md:356-368`; `Resolve compact glyph alphabet` at `00-collective-task-list.md:756-768`.
Choice needed: minimal global `[x]` plus local declared alphabets, full global glyph map, or ASCII-first local policy.

[U2][BRACKET_TOKEN_SUFFIX_GRAMMAR]:
Task refs: `Add type-local bracket token and status registry pointer` at `00-collective-task-list.md:370-382`; `Resolve bracket-token suffix grammar` at `00-collective-task-list.md:770-782`.
Choice needed: codemap/source-key-only suffixes, generally allowed declared type-local markers with base-token projection, or no shared suffix grammar.

[U3][ROW_SIDECAR_PATTERN]:
Task refs: `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules` proof gap at `00-collective-task-list.md:242-254`; `Resolve row-sidecar label pattern` at `00-collective-task-list.md:784-796`.
Choice needed: durable source-scannable sidecars, Markdown footnotes, or row-owned records. Until chosen, use row-owned records for proof/update/removal details.

[U4][CLAUDE_GOVERNANCE]:
Task refs: `.claude/skills/**` invocation marker blocker at `00-collective-task-list.md:398-410`; `.claude` ripple boundary at `00-collective-task-list.md:710-722`; `.claude` governance boundary at `00-collective-task-list.md:798-810`.
Choice needed: normalize `.claude/prompts/**` and `.claude/skills/**` under standards in this implementation, or keep them as source material only. Provider skill-format proof is still separate from the user governance choice.

[U5][README_LATE_EXAMPLES]:
Task ref: `Move or reframe README late examples` at `00-collective-task-list.md:606-618`.
Choice needed: move local misuse examples beside controlling rules, or preserve a late combined section only if it is explicitly a reader-route example set.

[U6][SUPPORT_NA_DOMAIN]:
Task ref: `Clarify absence values across table cells, record fields, source unknowns, and domain none` at `00-collective-task-list.md:412-424`.
Choice needed: whether support-matrix `n/a` shares the domain-`none` rule or stays support-specific.

## [TASKS_TO_DROP_WITH_REASON]

[DROP1][TEXT_TRANSCRIPT_GLOBAL_INTENT]:
Task: `Drop text transcript as a global fence intent` at `00-collective-task-list.md:340-352`.
Reason: already correctly marked `DROPPED_ACTIVE_STANDARD_INTENT`; keep as false-positive guard only, not an implementation task.

[DROP2][ROADMAP_PROGRESS_FALSE_POSITIVE]:
Task: `Preserve roadmap progress as canonical progress example` at `00-collective-task-list.md:576-588`.
Reason: already marked `DROPPED_NO_DEFECT`; no active edit should follow.

[DROP3][TASK_LOCAL_STEP_MARKERS_GLOBALIZATION]:
Task: `Keep task-local step markers local` at `00-collective-task-list.md:664-676`.
Reason: already marked `DROPPED_TYPE_LOCAL`; no global notation promotion should follow.

[DROP4][ONBOARDING_TUTORIAL_POSITIVE_EXAMPLES_AS_EDIT]:
Task: `Use onboarding and tutorial as positive narrowed-status examples` at `00-collective-task-list.md:694-706`.
Reason: useful evidence, but no edit target unless the shared rule intentionally adds an example. Drop as an implementation task or merge into the closed-vocabulary evidence basis.

[DROP5][NON_STANDARDS_RIPPLE_BEFORE_STANDARDS]:
Task surface: non-standards files named in `00-collective-task-list.md:242-254`, `284-310`, `740-752`, and `784-796`.
Reason: source material only until shared standards land and user-choice blockers close. Keep as later ripple candidates, not immediate tasks.

## [FINAL_RECOMMENDATION]

Recommendation: HOLD.

Reason: the report corpus is strong enough to proceed after correction, but the current collective list is not an implementation contract yet. It still mixes ready tasks with user choices, pending provider/source research, duplicate carrier families, dropped false positives, and broad non-standards ripple. The next safe action is to revise `track-synthesis/00-collective-task-list.md` into an owner-ordered queue, split held research from ready active-standard edits, merge duplicate command/packet/table/glyph/`.claude` tasks, and explicitly keep `_reports/**`, prompt/process labels, worker roles, and report frames out of active standards.

Top findings:
- `00-collective-task-list.md` needs an implementation queue before active edits because open, blocked, dropped, and later-ripple rows are interleaved.
- Command/output, field-packet, generated-ledger, table-sidecar, compact-glyph, and `.claude` tasks overlap and should merge before promotion.
- Closed vocabulary cards, source-scan records, invocation-marker governance, and absence values cross owner boundaries that need splitting across `information-structure.md`, `formatting.md`, `proof.md`, type standards, `.claude/README.md`, and `_reports/AGENTS.md`.
- User choices block compact glyphs, bracket suffix grammar, row sidecars, `.claude` governance, README late examples, and support `n/a` semantics.
- The ready implementation order starts with task-list correction, then shared owner standards, then path-specific type fixes, and only later non-standards Markdown ripple.

Close proof:
- `git diff --check -- docs/standards/_reports/standards-structure-notation-060626/track-synthesis/01-review-owner-order.md` passed after this report was written.

Proof gaps:
- No active standards were edited.
- No Mermaid render, link check, anchor check, docs build, provider lookup, or command-behavior proof ran in this review.
