Question: Which transcript and session-process shapes in the current synthesis would leak source-material mechanics into active standards if promoted?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: docs/standards/_reports boundary :: transcript and process leakage :: correct promotion boundary
Target owner: `docs/standards/_reports/AGENTS.md`; ``; `docs/standards/information-structure.md`; `docs/standards/proof.md`
Source basis: `docs/standards/_reports/AGENTS.md`; session `README.md`; `track-synthesis/00-collective-task-list.md`; `.claude/prompts/fix-standards-docs.md`; `track-external-research/09-code-fence-intent-labels.md`; active `agents-md.md`, `information-structure.md`, and `proof.md`
Promotion target: `docs/standards/_reports/AGENTS.md` only if the report-boundary rejection needs tighter wording; active standards should receive only the durable field-packet and fence-boundary rules named below.
Outcome: CORRECT

## [FINDINGS]

Finding 1
Path/section: `docs/standards/_reports/AGENTS.md:98-105`, `docs/standards/_reports/AGENTS.md:147-159`, `docs/standards/_reports/standards-structure-notation-060626/README.md:56-75`.
Axis: position.
Issue: the report boundary is mostly correct. `_reports/AGENTS.md` already bans top-level `TRANSCRIPT`, `READ_TRANSCRIPT`, `CONFIDENCE`, `VALIDATION`, `NO_CHANGE_CONFIRMATIONS`, `DRAFT_EDIT_MAP`, worker-role headings, and task-history sections. It also says post-promotion reports that only preserve transcript, wave structure, validation boilerplate, or duplicate synthesis should be deleted or dropped. The session manifest repeats the boundary by warning later agents not to repeat wave framing, transcript shape, or source inventories as durable standards.
Correction task: keep the existing report-only boundary as the controlling rule and do not copy its session mechanics into active standards. Later active edits should extract only reusable report field packets, proof-gap handling, and source-route rules.
Rule/standard to tighten: `_reports/AGENTS.md` only if a later pass finds an actual report using a forbidden heading; no active standard needs a transcript section.
Proof gap: no full line-by-line audit of every report body was run beyond targeted scans for transcript, wave, worker, validation, and code-fence leakage.

Finding 2
Path/section: `.claude/prompts/fix-standards-docs.md:68-134`, `track-synthesis/00-collective-task-list.md:668-680`.
Axis: craft.
Issue: the prompt's waves, named critique roles, and sub-agent choreography are valid task orchestration, not durable standards content. The collective task list correctly recognizes this by asking to promote only the source-scan record principle while excluding wave counts. The correction remains too easy to misapply because it names active owners and `_reports/AGENTS.md` together; a later implementation pass could accidentally standardize prompt process language instead of the field packet.
Correction task: treat this item as `CORRECT`, not a blanket `PROMOTE`, unless the accepted wording explicitly says the durable rule is the field packet only: source basis, exact span, weakness, correction route, ripple files, decision, and proof gap. Drop wave count, worker role, critique-role list, launch order, and "review agent" choreography from active standards.
Rule/standard to tighten: `` can own artifact separation; `docs/standards/information-structure.md` can own the report or source-scan record shape; `_reports/AGENTS.md` already owns report mechanics.
Proof gap: none for the boundary; the prompt and task-list lines are enough to prove the leakage risk.

Finding 3
Path/section: `track-external-research/09-code-fence-intent-labels.md:64-76`, `track-external-research/09-code-fence-intent-labels.md:104-123`, `track-synthesis/00-collective-task-list.md:284-296`, `docs/standards/information-structure.md:304-313`, `docs/standards/proof.md:26-32`, `docs/standards/proof.md:229-248`.
Axis: notation.
Issue: the proposed `text transcript` hold item should not become a global code-fence intent. The source report already says `text transcript` is syntactically viable but unsafe as a standard because a transcript can mix prompts, commands, terminal output, local paths, and proof claims. Active `information-structure.md` already separates runnable commands from observed output, and active `proof.md` says local command output outranks copied transcripts and long transcripts should be summarized rather than pasted.
Correction task: keep `transcript` out of the global intent vocabulary and out of ordinary active standards. If a literal transcript is ever retained inside `_reports/**`, describe the retention reason in visible prose and mark the report outcome as `HOLD` or `DROP`; do not add `text transcript` as a reusable fence label. Prefer `bash copy-safe` for commands, `text output-only` for bounded observed output or expected signals, and `text conceptual` for illustrative terminal-shaped text.
Rule/standard to tighten: `docs/standards/information-structure.md` should state that subtype words such as `schema`, `transcript`, `fixture`, and `contract` are prose/source-route labels unless a three-token grammar is explicitly selected. `docs/standards/proof.md` already owns the proof boundary.
Proof gap: no local fence validator is proven; any automated claim about catching `text transcript` needs tool proof.

Finding 4
Path/section: targeted scan results across `docs/standards/_reports/standards-structure-notation-060626/**`; examples include `track-external-research/02-mermaid-renderer-proof.md:217-221`, `track-formatting-notation/02-formatting-cross-corpus-usage.md:221-224`, and repeated "No active standards were edited" notes in source reports.
Axis: evidence.
Issue: report-only close notes are useful for archive accountability but should not become a standards rule. `_reports/AGENTS.md` explicitly rejects no-change confirmations that only say a file was read, a report was written, active files were untouched, or expected validation was not needed. Active standards should require proof gaps and configured gate selection, not boilerplate `VALIDATION` sections in every report or no-op claims.
Correction task: when promoting evidence rules, preserve only the durable distinction: a report can name the gate it ran or the proof gap it leaves; active standards should not ask reports to carry top-level validation ledgers, no-change confirmations, or report-written receipts.
Rule/standard to tighten: `docs/standards/proof.md` for gate/proof-gap semantics; `_reports/AGENTS.md` for report close-shape pruning if future reports keep adding table-stakes no-op close notes.
Proof gap: the scan was targeted to leakage terms, not an exhaustive report-quality review.

## [EVIDENCE]

- Root and standards overlays make `_reports/**` source material only and exclude it from active standards authority unless durable findings are promoted through trusted owners.
- `_reports/AGENTS.md` already defines allowed report records and forbids top-level transcript, validation, confidence, draft-edit, worker-role, and task-history sections.
- The session manifest tells later passes to start from the collective task list and not repeat wave framing, transcript shape, or source inventories as durable standards.
- `agents-md.md` treats transcripts, prompt assets, memory notes, reports, critique passes, fixed sub-agent counts, local paths, session plans, and process commentary as non-authoritative source material that must be stripped before active wording.
- `proof.md` treats copied transcripts as weaker than local command output and rejects long transcript pastes as proof.

## [RECOMMENDATIONS]

[CORRECT]:
- Reword the source-scan task-list item from "promote" posture to a corrected boundary: promote the field packet only; drop prompt choreography.
- Treat `text transcript` as a rejected global intent, not merely a held general convention. The only hold is whether `_reports/**` may retain literal transcripts as source material under visible prose, not whether active standards should standardize a transcript fence label.
- Convert report-only validation boilerplate into proof-gap language when a report finding depends on an unrun command, renderer, link check, or source scan.

[KEEP]:
- Keep the existing `_reports/AGENTS.md` report-shape ban on top-level transcript, validation, no-change, draft-edit, confidence, worker-role, and task-history sections.
- Keep session README process notes inside the report session only; they are not active standards.

[DROP]:
- Drop any future active-standard wording that names wave counts, worker roles, critique-role lists, transcript-retention sections, or no-op report completion receipts.
- Drop `transcript` from the global code-fence intent vocabulary unless the user explicitly chooses a report-local exception and the exception stays out of active produced-document examples.

## [CANDIDATE_WORDING]

For `track-synthesis/00-collective-task-list.md`, replace the risky correction shape with:

```text template
Correction: promote only the durable source-scan field packet: source basis, exact source span, weakness, correction route, ripple files, decision, and proof gap. Do not promote wave count, worker role, launch order, critique-role names, transcript sections, or no-change validation receipts into active standards.
```

For `docs/standards/information-structure.md` if the fence grammar task lands:

```text template
Do not use `transcript` as a global code-fence intent. A transcript is source material, not a produced-document block type. Split commands and observed output into separate fences, or summarize the transcript through a proof record that names the command, source, result, and proof gap.
```

For `_reports/AGENTS.md` only if a later report needs a report-local exception:

```text template
Literal transcripts may remain in `_reports/**` only when they are bounded source evidence for a correction, carry a visible retention reason, and are not promoted as an active-standard section or global code-fence intent. Prefer summarized proof records.
```

## [PROOF_GAPS]

- No exhaustive all-report prose review was run; this pass used the required source reads plus targeted scans for transcript, wave, worker, validation, no-change, draft-edit, session-process, and `text transcript` leakage.
- No local Markdown fence validator is proven. Any future automated enforcement of `text transcript` rejection needs a configured tool source or an explicit proof gap.
