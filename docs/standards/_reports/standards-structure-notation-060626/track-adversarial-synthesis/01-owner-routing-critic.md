Question: Which collective task-list items have wrong owner routing, overpromote source material, widen type-local rules, or miss ripple owners?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: track-synthesis/00-collective-task-list.md :: owner routing :: correct false promotions
Target owner: `docs/standards/_reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
Source basis: mandatory instruction chain, session manifest, `track-synthesis/00-collective-task-list.md`, active shared owners, and cited source reports named below
Promotion target: `track-synthesis/00-collective-task-list.md`
Outcome: CORRECT

## [FINDINGS]

### [F1][RELATION_PREAMBLE_OWNER]

Task reference: `track-synthesis/00-collective-task-list.md:54-66`.
Cited reports read: `track-type-corpus/01-explanation-type-corpus.md`; `track-type-corpus/02-reference-type-corpus.md`; `track-type-corpus/03-task-learning-type-corpus.md`.
Problem: The task gives the owner as `docs/standards/README.md` or `docs/standards/information-structure.md`. `README.md` owns split/link routing and reader-need placement, not relation-record field order. `information-structure.md` already owns adjacent-document relation records and local field preambles through its record-order rule.
Stronger owner route: `docs/standards/information-structure.md` owns the shared rule: local context fields may precede the shared relation run only when they change reader action, proof, status interpretation, validation, or maintenance; once `Changed fact:` appears, the shared field run stays contiguous. Type standards own only their local preamble declarations.
Decision: CORRECT.
Missing ripple files: remove the wildcard family ripple as an edit mandate. Ripple only to specific type standards where an active-source scan proves the local exception is missing before the first example.
Proof gap: The cited reports prove the cross-type pattern, but the task-list wildcard edit set still needs a current active-file scan before any type-standard edit.

### [F2][CLOSED_VOCABULARY_ROUTE]

Task reference: `track-synthesis/00-collective-task-list.md:116-128`.
Cited reports read: `track-information-structure/02-information-structure-cross-type-fit.md`; `track-formatting-notation/02-formatting-cross-corpus-usage.md`; `track-type-corpus/01-explanation-type-corpus.md`; `track-type-corpus/03-task-learning-type-corpus.md`; `track-external-research/06-status-taxonomy-vocabularies.md`; `track-external-research/03-information-architecture-structure-chooser-a.md`.
Problem: The task is routed only to `information-structure.md` and labels the axis as notation, but the proposed vocabulary card includes proof fields, projection rules, machine/display value split, glyph projection, and domain status values. That is a cross-owner packet, not a single notation task.
Stronger owner route: `information-structure.md` owns the vocabulary-card carrier and declaration threshold. `formatting.md` owns bracketed marker and compact glyph rendering. `proof.md` owns `Evidence:` and `Review trigger:` semantics. Each type standard owns its actual ADR, roadmap, support, onboarding, tutorial, design, test, or runbook values.
Decision: CORRECT.
Missing ripple files: add `docs/standards/formatting.md` and `docs/standards/proof.md` as required ripples for projection and proof-field semantics. Treat listed type standards as candidate checks, not automatic edits, unless their local values are currently undeclared or conflict with the shared card.
Proof gap: User choices remain open for display-versus-machine field cardinality, compact glyph policy, and unknown-value cardinality.

### [F3][COMMAND_OUTPUT_RIPPLE_OVERREACH]

Task reference: `track-synthesis/00-collective-task-list.md:158-170`.
Cited reports read: `track-information-structure/01-information-structure-container-toolbelt.md`; `track-information-structure/02-information-structure-cross-type-fit.md`; `track-repo-markdown/02-non-standards-markdown-patterns.md`.
Problem: The task correctly routes packet shape to `information-structure.md`, but its ripple list includes root and tool READMEs as if source-shape evidence already authorizes non-standards doc rewrites. The later targeted-repo task explicitly holds broad repo Markdown ripple until standards wording lands.
Stronger owner route: `information-structure.md` defines command/output carriers. `proof.md` defines evidence for command behavior, output claims, unrun gates, and freshness. Root and tool READMEs own their command truth and should be later ripple targets only after the active standard is promoted and each local command claim is proved from source or execution.
Decision: HOLD.
Missing ripple files: add `docs/standards/proof.md` to the standards ripple. Move `README.md`, `tools/quality/README.md`, `tools/assay/README.md`, and `tools/rhino-bridge/README.md` to the later targeted-ripple sequence.
Proof gap: No command behavior was validated in the cited repo-scan reports; they are source-shape evidence only.

### [F4][PAGE_ANATOMY_RIPPLE_SCOPE]

Task reference: `track-synthesis/00-collective-task-list.md:270-282`.
Cited report read: `track-information-structure/02-information-structure-cross-type-fit.md`.
Problem: The owner route is right, but the ripple files are too broad. Splitting standard-file anatomy, type-standard opening, and produced-document skeletons belongs in `information-structure.md`; it does not automatically require editing every active type standard.
Stronger owner route: `information-structure.md` owns the split. `docs/standards/AGENTS.md` is a valid ripple because it repeats the type-standard opening contract. Type standards become ripple targets only where they currently rely on ambiguous page-anatomy wording or duplicate the shared opening rule incorrectly.
Decision: CORRECT.
Missing ripple files: none. Reduce `every active type standard` to a post-edit conformance scan with path-specific edits only for conflicts.
Proof gap: After the shared split lands, run an active-corpus heading/opening scan before naming type files for edit.

### [F5][TUTORIAL_STATUS_ROUTE]

Task references: `track-synthesis/00-collective-task-list.md:624-636` and `track-synthesis/00-collective-task-list.md:638-650`.
Cited report read: `track-type-corpus/03-task-learning-type-corpus.md`.
Problem: The first task says `tutorial.md` or shared routing language and ripples `reference/reference.md`. The second task names onboarding/tutorial as positive examples but then lists those same files as ripple targets. This risks moving type-local publication/proof statuses into reference lookup standards or editing type files merely to restate examples.
Stronger owner route: `information-structure.md` owns the shared boundary: type-local status vocabularies stay in the type standard when they determine produced-document validity, publication state, readiness, or proof closure. `learning/tutorial.md` needs a local edit only if its route-away sentence currently reads as moving its own `Availability` or `Execution` vocabularies to reference. `reference/reference.md` owns standalone lookup status vocabularies only.
Decision: CORRECT.
Missing ripple files: remove `reference/reference.md` unless a current line there claims ownership over type-local validity/publication statuses. Treat `learning/onboarding.md` and `learning/tutorial.md` as source examples for the shared rule, not edit targets.
Proof gap: Need a current source check of the exact tutorial boundary sentence before deciding whether `tutorial.md` itself needs a local wording correction.

### [F6][SOURCE_SCAN_RECORD_PROMOTION]

Task reference: `track-synthesis/00-collective-task-list.md:668-680`.
Cited report read: `track-repo-markdown/01-claude-markdown-patterns.md`.
Problem: The task promotes a `.claude` prompt report contract into `agentic-documentation.md`, `information-structure.md`, and `_reports/AGENTS.md`. The durable report-shape mechanics already have a trusted owner in `_reports/AGENTS.md`; copying prompt/report fields into active standards would promote source material and report structure beyond its route.
Stronger owner route: `_reports/AGENTS.md` owns report-specific field order, lane contract, merge keys, outcomes, corrections, promotion maps, and pruning. `agentic-documentation.md` may merge only the generic source-trace principle for task-output contracts. `information-structure.md` should receive only a generic source-scan carrier if produced active documentation needs it outside reports.
Decision: MERGE.
Missing ripple files: none. The current task should split into a report-mechanics correction for `_reports/AGENTS.md` and a separate HOLD item for any non-report source-scan carrier.
Proof gap: No active non-report consumer for the full source-scan field packet is proved. Keep wave counts and prompt choreography out of active standards.

### [F7][REPO_MARKDOWN_SEQUENCE_CONFLICT]

Task references: `track-synthesis/00-collective-task-list.md:200-212`, `track-synthesis/00-collective-task-list.md:684-696`, and `track-synthesis/00-collective-task-list.md:714-726`.
Cited reports read: `track-information-structure/01-information-structure-container-toolbelt.md`; `track-repo-markdown/02-non-standards-markdown-patterns.md`; `track-external-research/03-information-architecture-structure-chooser-a.md`.
Problem: The large lookup/matrix task lists `docs/system-api-map/**` and `docs/external-libs/**` as ripple files while the targeted-repo task says broad repo Markdown ripple is blocked until standards wording lands. The row-sidecar user-choice task also keeps sidecar labels unresolved.
Stronger owner route: `information-structure.md` owns table decomposition, row-owned records, matrix profiles, and row-sidecar eligibility. `formatting.md` owns sidecar label rendering only after the sidecar carrier is chosen. `docs/system-api-map/**`, `docs/external-libs/**`, and tool README files remain later targeted ripple candidates after standards wording and user choice settle.
Decision: HOLD.
Missing ripple files: add `docs/standards/formatting.md` when row-sidecar labels or marker spelling are part of the accepted rule. Remove non-standards docs from the immediate active-standards ripple list.
Proof gap: User choice is still open for source-scannable sidecars versus Markdown footnotes versus row-owned records.

## [EVIDENCE]

Owner boundaries:
- `docs/standards/README.md:18-22` separates README routing from shared standards and type-standard local vocabulary.
- `docs/standards/README.md:90-96` maps form to `information-structure.md`, evidence to `proof.md`, and notation to `formatting.md`.
- `docs/standards/AGENTS.md:29-42` repeats the same owner table and names type standards as owners for artifact-specific status vocabulary and local proof slots.
- `docs/standards/information-structure.md:160-164` already allows local identity/context fields before proof or relation runs when the local exception is declared.
- `docs/standards/formatting.md:3-15` states that formatting renders containers and status semantics after another standard defines them.
- `_reports/AGENTS.md:82-105` defines the report shape; `_reports/AGENTS.md:139-145` says durable findings promote to nearest trusted owners and reports remain source material.

## [RECOMMENDATIONS]

1. Update the collective task list, not active standards, to narrow owner routes before implementation.
2. Keep source-shape evidence from `.claude/**`, tool READMEs, and non-standards docs as source material until the active standard owner accepts the general rule and any user-choice blocker closes.
3. Replace broad wildcard ripple lists with path-specific active-source checks after the shared owner rule is edited.
4. Add missing `proof.md` and `formatting.md` ripples where tasks define proof-field semantics, command-output proof, status projections, compact glyphs, or sidecar rendering.

## [PROOF_GAPS]

- No active standards were edited.
- No non-standards Markdown ripple should run from this report alone.
- No command behavior, renderer behavior, link checker, anchor checker, or docs build was verified.
- The target task list still needs an implementation-owner pass to apply these corrections into `track-synthesis/00-collective-task-list.md`.
