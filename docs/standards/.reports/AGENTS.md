# [REPORTS_AGENTS]

Scope: `docs/standards/.reports/**`. Parent standards instructions remain in force for trusted owner routes; this folder stores source material only and is excluded from the active standards corpus unless a task explicitly names it.

## [1][SCOPE]

`.reports/` stores reusable agent outputs for this folder: source scans, repo scans, code-quality findings, navigation maps, bug/error/mistake/oversight reports, standards research, critique passes, synthesis notes, correction records, promotion maps, and candidate wording.

This folder does not own active instructions, documentation standards, architecture, roadmap state, command behavior, proof vocabulary, generated contracts, package facts, or validation gates. Promote durable findings to the nearest trusted owner.

When the user names another report path or asks for a one-off audit, use the named path and do not import this folder's session, track, manifest, or report-shape mechanics into that artifact.

## [2][READ_BEHAVIOR]

When a task names `.reports/**`, read the session manifest first, then prior synthesis, correction, promotion, or pruning records before individual lane reports. If no manifest exists, inventory folder names and report headings first, then treat the missing manifest as an archive-maintenance gap.

Before writing a new report in an existing lane, read prior passes in that lane and state the prior finding the new report extends, corrects, merges, drops, holds, or routes away. A later pass must add deeper source evidence, an adjacent owner route, a corrected contradiction, a narrower proof gap, exact candidate wording, or a new avenue the earlier pass missed.

When active standards work does not name this folder, treat `.reports/` as excluded source material. Do not include it in active-corpus counts, standards audits, root-file audits, routine link sweeps, generated-contract reads, or validation claims.

## [3][NAMING]

Name session folders as `<top-slug>-<ddmmyy>`. The `top-slug` is the active owner, standard, package, or durable research subject, not a phase, wave, worker role, prompt title, or process label. Use lowercase ASCII words joined with hyphens. Good examples: `agents-md-050626`, `code-documentation-050626`, `proof-060626`, `external-libs-060626`.

Do not use `session-`, dates before topics, worker names, wave names, or broad labels such as `misc`, `general-research`, `followup`, or `final`.

Use `track-<lane>` subfolders when a session has parallel concern lanes, more than 12 reports, or more than one durable subject lane. Track names are retrieval lanes, not chronology. Prefer owner or subject lanes such as `track-csharp`, `track-python`, `track-root-overlays`, `track-provider-docs`, `track-gap-critique`, or `track-promotion`.

Inside each track folder, name reports as `NN-<question-slug>.md`. The track path already carries the lane, so do not repeat it in every filename unless the file can move across tracks without losing meaning. `NN` is zero-padded, track-local, append-only, and assigned by merge order inside that track. Every track starts at `01`; sibling tracks in the same session may both contain `01-*` reports. Gaps are allowed after pruning; never renumber cited reports.

Parallel concern tracks collect independent questions under one owner or subject. Report numbers in those tracks do not encode cross-track chronology, priority, or dependency; the session manifest owns cross-track ordering, promotion state, and dependency notes.

Iterative push tracks extend an existing lane. A follow-up pass edits the earlier report when it corrects, retracts, or merges the same merge key; it adds a new report only for a stronger question, deeper evidence class, new owner route, or adjacent avenue that cannot be cleanly folded into the prior report.

## [4][SESSION_MANIFEST]

Every multi-report session folder has `README.md` as the session manifest. The manifest is the archive entrypoint and routing surface, not a ledger, narrative summary, or active standard.

Required sections:
- `[SESSION_CARD]`: definition block naming target owner, status, source set, read order, and boundary.
- `[TRACK_CARDS]`: grouped definition blocks, one per track, with `Direction`, `Facets`, `Use when`, `Extend by`, and `Route`.
- `[NEXT_PASS_RULE]`: one paragraph that tells a later agent how to build, correct, fold, or route without repeating the same pass.
- `[CORRECTIONS]`: only when a report has a corrected, superseded, or retracted claim.
- `[PROMOTION_LOG]`: durable rules promoted, target owner files, and promotion date.
- `[PRUNING_LOG]`: reports deleted, stubbed, or marked dropped, with the reason.

Allowed status values: `OPEN`, `PROMOTED`, `PARTIAL`, `SUPERSEDED`, `DROPPED`, and `RETRACTED`.

The manifest should answer these questions before it accounts for files: which lane should be read first, what direction each track took, what facet or source class each track owns, what a later pass may add, and where durable findings promote. Use filenames and `fd` for exhaustive file inventory; do not make the README a duplicate filesystem listing.

Use a report-index table only when a human or agent must compare homogeneous report rows by the same fields. Keep the table under the standards table ceiling, split it by track when needed, and omit it when track cards plus file names carry the retrieval job. A mega-table that only lists every file, source basis, and status is report accounting; replace it with track cards.

## [5][LANE_CONTRACT]

A report is useful only when it improves the lane-wide decision body. Each report must name its lane, merge key, owner route, unique evidence, recommendation state, and any contradiction it creates or resolves.

Use the merge key to make related findings collapsible across reports: `<owner-file-or-surface> :: <claim-or-rule> :: <action>`. Reports with the same merge key must converge, contradict with evidence, or mark the older finding stale; they must not restate the same validation in new prose.

Every report must leave one lane outcome:
- `PROMOTE`: durable rule or wording should move to an active owner.
- `CORRECT`: an existing report, route, claim, or recommendation is wrong.
- `MERGE`: this report strengthens or narrows an existing finding.
- `DROP`: the finding is transcript-only, duplicate, stale, or unsupported.
- `HOLD`: evidence is useful but proof is missing or the owner route is unresolved.

## [6][REPORT_TYPES]

Use exactly one report type per report:
- `source-scan`: primary external or local source evidence.
- `repo-scan`: current repository, source, manifest, or tool-output evidence.
- `code-quality`: concrete defect, risk, bad pattern, missed invariant, or owner-rail quality finding.
- `navigation-map`: owner maps, route graphs, false-route lists, missing-owner gaps, and next-read decisions.
- `bug-oversight`: observed symptom, reproduction or source evidence, cause, affected owner, correction route, and proof gap.
- `standards-research`: findings mapped to `ADD`, `CHANGE`, `REMOVE`, `KEEP`, or `NO_CHANGE` plus the active owner file.
- `gap-critique`: adversarial critique of existing or proposed rules.
- `synthesis`: cross-report consolidation that removes duplicates and resolves contradictions.
- `wording-packet`: exact candidate language for one owner file.
- `promotion-map`: maps findings to active standard owners.
- `correction`: fixes a prior report claim with stronger proof.
- `prune-log`: records archive cleanup when deletion would otherwise hide why evidence disappeared.

Do not create report types for worker roles, waves, confidence levels, or implementation phases.

## [7][REPORT_SHAPE]

Each report starts with this compact record:

```text
Question: <one concrete question>
Type: <one report type>
Lane: <track or lane name>
Merge key: <owner-file-or-surface :: claim-or-rule :: action>
Target owner: <active standard or overlay path>
Source basis: <repo proof, primary docs, prior report paths, or tool output>
Promotion target: <owner path or "none yet">
Outcome: <PROMOTE|CORRECT|MERGE|DROP|HOLD>
```

Use these sections when applicable:
- `[FINDINGS]`
- `[EVIDENCE]`
- `[RECOMMENDATIONS]`
- `[CANDIDATE_WORDING]`
- `[PROOF_GAPS]`
- `[CORRECTIONS]`

Do not use top-level `TRANSCRIPT`, `READ_TRANSCRIPT`, `CONFIDENCE`, `VALIDATION`, `NO_CHANGE_CONFIRMATIONS`, `DRAFT_EDIT_MAP`, worker-role headings, or task-history sections. Preserve source lists and proof gaps; remove process narration.

## [8][NO_RETREAD]

Before adding a report, search the session manifest and existing report headings for the same question, source, target owner, and recommendation. Add a new report only when it contributes at least one of:
- a new primary source or repo proof;
- a materially different finding;
- a correction to a prior claim;
- a promotion map for an unpromoted finding;
- candidate wording that replaces weaker wording;
- a new adjacent owner route or avenue of concern.

If the new pass only confirms an existing finding, update the manifest status or promotion log instead of adding another report.

## [9][CORRECTIONS]

When a later report finds an earlier report wrong, stale, duplicated, overbroad, or unsupported, it must name the earlier report path, the exact claim being corrected, the stronger evidence, and the lane outcome: `CORRECT`, `DROP`, `MERGE`, or `HOLD`.

When current repository source, active standards, maintained documentation, or runnable tool output conflicts with a report, current proof wins. Mark the report finding stale instead of preserving both versions.

Never silently rewrite a cited report claim. Correct it through the session manifest and, when needed, a `correction` report.

A correction record names:
- `Original report`
- `Claim`
- `Problem`
- `Replacing proof`
- `Disposition`: `CORRECTED`, `SUPERSEDED`, or `RETRACTED`
- `Affected promotion targets`

When a corrected report is not externally cited, prefer updating the original file with a short `[CORRECTIONS]` section. When it is cited or already promoted, leave the original body intact and add a correction report.

No-change confirmations are allowed only when they resolve a real decision: keeping a rule, rejecting an edit, preserving an owner route, or blocking false proof. Delete or mark `DROPPED` no-change confirmations that only say a file was read, a report was written, active files were untouched, or expected validation was not needed.

## [10][PROMOTION_AND_PRUNING]

Promote rules, not report frames. A promoted rule must name the active owner file and the report paths that supplied evidence.

Promote only durable findings. Map each finding to the nearest trusted owner: parent `AGENTS.md` for behavior deltas, README for entry/workflow, architecture for code shape, roadmap for future sequence, source/manifests/generated contracts for current truth, tool README for command behavior, and `docs/standards/*` only when the promoted rule is a documentation standard.

When a promoted rule changes owner routing, overlay behavior, source-material status, or archive cleanup, update or verify the root read order, parent route table, affected sibling overlays, session manifest, and active-corpus exclusion in the same change.

After promotion:
- keep reports with unique source evidence, exact findings, proof gaps, or reusable candidate wording;
- delete reports that only preserve transcript, wave structure, validation boilerplate, or duplicated synthesis;
- mark `DROPPED` in the manifest instead of keeping tombstone files;
- keep a stub only when an external or active document cites the old path.

A report kept after promotion must still answer why a future agent would read it instead of the active standard.

## [11][REJECTIONS]

- Do not turn this folder into a second docs corpus, standards library, README index, command catalog, provider manual, validation ledger, transcript store, or instruction source.
- Do not copy report headings, confidence labels, wave names, worker roles, prompt wording, task narration, local machine paths, worktree status chatter, draft patch plans, fixed agent counts, session folders, track names, report types, report fields, or archive vocabulary into active owners or non-report artifacts.
- Do not publish table-stakes reports that only say the folder was read, the task was performed, active files were untouched, or no validation was needed.
- Do not copy active folder policy, command gates, README content, architecture summaries, or standards bodies into `.reports/AGENTS.md`.
