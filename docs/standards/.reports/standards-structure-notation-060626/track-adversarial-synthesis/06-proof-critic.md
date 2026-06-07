Question: Which proof-related synthesis tasks still risk overclaiming current behavior, configured gates, renderer output, generated ledgers, stale sources, or validation?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: docs/standards/proof.md :: proof correctness and validation overclaims :: correct task-list promotion
Target owner: docs/standards/proof.md
Source basis: active proof, information-structure, and formatting standards; session manifest; collective task list; root-corpus report; external research reports 01, 02, 07, and 10
Promotion target: docs/standards/proof.md; docs/standards/information-structure.md; docs/standards/formatting.md; track-synthesis/00-collective-task-list.md
Outcome: CORRECT

## [FINDINGS]

### [1][RENDERER_SOURCE_CLASS]

Task-list target: `track-synthesis/00-collective-task-list.md:386-398`.
Axis: evidence.
Issue: the renderer-support task is correct, but the final packet can still overclaim if it records maintained renderer docs, installed `mmdc`, and rendered-output evidence in one undifferentiated `Evidence:` field. `proof.md:154-165` requires proof for renderer claims, while `information-structure.md:438-440` and `formatting.md:107-114` state renderer-sensitive syntax. External reports prove source support and local CLI availability, but `track-external-research/02-mermaid-renderer-proof.md:78-91` and `:93-113` explicitly leave local visual layout and SVG accessibility output unproved.
Correction task: require the renderer packet to split each row into `Source class`, `Applies to`, `Configured command or source`, `Proves`, `Does not prove`, `Proof gap`, `Last verified`, and `Review trigger`. Rows must separate CommonMark/GFM syntax, GitHub product behavior, official Mermaid syntax/config, local `mmdc` availability, local render output, and generated SVG accessibility output.
Rule or standard to tighten: `docs/standards/proof.md` renderer claims and configured-gate records.
Proof gap: no active standards diagram render, SVG inspection, GitHub-render proof, or visual screenshot proof was produced by the named reports.
Disposition: MERGE with the existing renderer-support task, but add `Does not prove` as non-optional.

### [2][MMDC_ROUTE_VERSUS_RENDER_PASS]

Task-list target: `track-synthesis/00-collective-task-list.md:400-412`; validation target `:758-768`.
Axis: evidence.
Issue: the task list says Mermaid uses locally installed `pnpm exec mmdc` when render behavior is claimed, and the Mermaid report proves `mmdc` is configured and usable at `track-external-research/02-mermaid-renderer-proof.md:139-172`. That proves a gate route exists. It does not prove any future changed diagram rendered unless the command runs on the changed Markdown and the report names the generated artifacts.
Correction task: phrase the configured Mermaid gate as available route plus proof class, not as standing render proof. A final validation note may say `Mermaid gate not run because no diagram or renderer claim changed`; it may not say renderer proof passed from package availability alone.
Rule or standard to tighten: `docs/standards/proof.md` `[9][DOCS_CODE_VERIFICATION]`; `docs/standards/AGENTS.md` proof-close checklist.
Proof gap: no full active-corpus Mermaid render gate ran in the Mermaid report, and no generated artifacts exist for this proof-critic report.
Disposition: CORRECT any promotion wording that collapses `mmdc` availability into render success.

### [3][CONFIGURED_GATES_MUST_STAY_NEGATIVE_WHEN_ABSENT]

Task-list target: `track-synthesis/00-collective-task-list.md:400-412` and `:748-768`.
Axis: evidence.
Issue: `proof.md:140-165` already says to run every configured matching gate and to state `Proof gap:` when no configured gate exists. The synthesis task should not soften that into a broad validation plan that implies link checking, anchor validation, docs builds, generated-output comparison, or visual layout proof are configured. The bounded source set found `git diff --check` and Mermaid tooling only; `track-external-research/07-proof-uncertainty-stale-sources.md:45-54` supports explicit configured-gate records.
Correction task: for every final validation line, require exact wording from one of three states: `ran <exact command>`, `not applicable because <claim class did not change>`, or `Proof gap: no configured <gate class> command named`. Avoid `link/anchor check` shorthand unless the command or source file is named.
Rule or standard to tighten: `docs/standards/proof.md` configured-gate record and final validation wording.
Proof gap: link checker, anchor checker, docs build, table validator, fence-label validator, and generated-ledger parser availability remain unproved in the bounded read.
Disposition: PROMOTE with stricter negative wording.

### [4][GENERATED_LEDGER_PACKET_NEEDS_EXPECTATION_AND_COMPARISON]

Task-list target: `track-synthesis/00-collective-task-list.md:256-268`.
Axis: evidence.
Issue: the generated-ledger packet lists source, refresh command, generated artifact or expected output, proof gap, freshness, and trigger. That is necessary but still lets provenance pass for validation. `track-external-research/07-proof-uncertainty-stale-sources.md:12-21` says generated/provider/renderer/support records must name the claim, controlling source, verification or comparison performed, and failure state. `proof.md:20-26` also ranks generated contracts and executed local verification separately.
Correction task: add `Expected value:` or `Expected output:` plus `Comparison:` or `Verification:` to the generated-ledger packet. `Generated from:` proves origin, `Source of truth:` proves ownership, and `Comparison:` proves the checked artifact agrees. If no local generator or comparison is inventoried, the packet stays generic and carries `Proof gap: local generated-documentation command not inventoried`.
Rule or standard to tighten: `docs/standards/information-structure.md` generated-ledger carrier; `docs/standards/proof.md` generated-artifact proof fields.
Proof gap: no local generated-documentation command or generated-ledger parser was inventoried by the named reports.
Disposition: CORRECT before active wording promotes.

### [5][MACHINE_LEDGER_VALIDATION_SCOPE]

Task-list target: `track-synthesis/00-collective-task-list.md:242-254`.
Axis: evidence.
Issue: the machine-consumed Markdown task correctly records that official Roslyn release-tracking activation is not locally proven. The active promotion can still overclaim if it says local release-discipline tests validate "the Roslyn ledger" as a whole. `track-external-research/10-machine-consumed-markdown-ledgers.md:23-31` narrows local proof to row parse shape and descriptor/category/severity consistency, not semicolon comments, `### New Rules`, release-section movement, official `AdditionalFiles` wiring, or upstream release workflow.
Correction task: require every parser-owned ledger record to list `Consumer`, `Fields validated`, `Fields not validated`, `Validation command`, and `Proof gap`. Do not place `tools/cs-analyzer/AnalyzerReleases.*.md` in ripple files as a normalization target; it is an exception source, not a standards rewrite target, unless the consumer changes.
Rule or standard to tighten: `docs/standards/information-structure.md` machine-consumed Markdown exception; `docs/standards/formatting.md` no-normalize exception; `docs/standards/proof.md` validation-scope wording.
Proof gap: no C# test rail ran, and official release-tracking analyzer activation remains unproved from the bounded local project read.
Disposition: MERGE with the machine-ledger task and correct ripple wording.

### [6][PROOF_GAP_IS_NOT_A_GLYPH_OR_UNKNOWN_VALUE]

Task-list target: `track-synthesis/00-collective-task-list.md:300-312` and `:356-368`.
Axis: notation and evidence.
Issue: the compact-glyph task leaves open whether `[?]` may ever mean proof gap. That should not be a user choice. `proof.md:97-107` requires explicit `Proof gap:` for missing source, unrun check, unsupported renderer proof, or provisional status. `formatting.md:31-40` already distinguishes table absence, skipped gates, unknown values, and literal nulls. A compact glyph can mark a local unknown value only after a local vocabulary declares it; it must not replace a proof field.
Correction task: resolve proof-gap notation now: `Proof gap:` is the only shared marker for missing evidence. `[?]`, `[UNKNOWN]`, `[SKIP]`, and the table absence token may describe source values, runtime states, skipped gate results, or absent table cells only under their declared families. This should move out of the blocked glyph-choice bucket.
Rule or standard to tighten: `docs/standards/proof.md` assertion uncertainty; `docs/standards/formatting.md` status/result marker boundary.
Proof gap: remaining uncertainty is user choice for the non-proof glyph alphabet, not evidence.
Disposition: CORRECT.

### [7][STALE_SOURCE_WORDING_MUST_CONTROL]

Task-list target: `track-synthesis/00-collective-task-list.md:428-440`, `:566-576`, and `:582-590`.
Axis: evidence.
Issue: the stale-source task is correct, but it must explicitly govern language-baseline and imported lifecycle-field tasks. `track-external-research/07-proof-uncertainty-stale-sources.md:23-32` says freshness and lifecycle terms require source-specific definition plus `Last verified:` or `Review trigger:`. Therefore claims such as TypeScript beta status, C# release state, Python docs beta state, support lifecycle fields, `latest`, `current`, `legacy`, `deprecated`, and `obsolete` cannot remain in type standards as unsourced examples.
Correction task: require future baseline and lifecycle tasks to choose one of two forms: target-standard invariant without current availability wording, or current-behavior claim with source-specific definition, current source, and freshness field. Do not use "baseline", "latest", or imported field names as current facts without the proof packet.
Rule or standard to tighten: `docs/standards/proof.md` stale-source rule; `docs/standards/reference/code-documentation.md`; `docs/standards/reference/support-matrix.md`.
Proof gap: C# 14 release-state proof and exact imported lifecycle schema proof are still uncollected in the task list.
Disposition: PROMOTE as a blocking condition on those type-standard edits.

### [8][SOURCE_TRACE_IS_NOT_AGENT_EVALUATION]

Task-list target: `track-synthesis/00-collective-task-list.md:442-454`.
Axis: evidence.
Issue: the source-trace versus semantic-pass task is good, but its ripple back to `proof.md` should prevent a second overclaim: a cited source trace also does not prove retrieval quality, tool-selection quality, or agent-surface effectiveness. `proof.md:167-216` requires deterministic or rigor receipts for machine-facing surfaces, including checks, result, trace, and review fields where stochastic output or ranking is claimed. `track-external-research/07-proof-uncertainty-stale-sources.md:56-65` already warns not to imply probabilistic retrieval quality without evaluation receipts.
Correction task: add a negative sentence to the promotion packet: source provenance can prove origin, and semantic validation can prove claim agreement, but retrieval/ranking/tool-selection claims require the evaluation receipt in `proof.md`; otherwise mark `Proof gap:`.
Rule or standard to tighten: `` source trace wording; `docs/standards/proof.md` agent-surface evaluation.
Proof gap: no retrieval evaluation, ranking trial set, or provider/tool-selection trace was collected in this session.
Disposition: MERGE.

## [EVIDENCE]

[SOURCE_FILES_READ]:
- `CLAUDE.md`.
- `AGENTS.md`.
- `docs/standards/README.md`.
- `docs/standards/AGENTS.md`.
- `.reports/AGENTS.md`.
- `.reports/standards-structure-notation-060626/README.md`.
- `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`.
- `docs/standards/proof.md`.
- `docs/standards/information-structure.md`.
- `docs/standards/formatting.md`.
- `.reports/standards-structure-notation-060626/track-root-corpus/01-root-shared-corpus.md`.
- `.reports/standards-structure-notation-060626/track-external-research/01-gfm-github-markdown-capabilities.md`.
- `.reports/standards-structure-notation-060626/track-external-research/02-mermaid-renderer-proof.md`.
- `.reports/standards-structure-notation-060626/track-external-research/07-proof-uncertainty-stale-sources.md`.
- `.reports/standards-structure-notation-060626/track-external-research/10-machine-consumed-markdown-ledgers.md`.

[PRIOR_FINDINGS_EXTENDED]:
- `track-root-corpus/01-root-shared-corpus.md`: renderer-support packet, configured gate selector, active proof gaps.
- `track-external-research/02-mermaid-renderer-proof.md`: configured `mmdc` route and local-versus-upstream Mermaid split.
- `track-external-research/07-proof-uncertainty-stale-sources.md`: provenance versus verification, generated-ledger packet, stale-source rule, proof-gap templates.
- `track-external-research/10-machine-consumed-markdown-ledgers.md`: parser-owned ledgers, local parser scope, no-normalize rule, generated versus hand-maintained ledger split.

## [RECOMMENDATIONS]

[CORRECT_BEFORE_PROMOTION]:
- Add `Does not prove:` to renderer-support and configured-gate packets.
- Keep `mmdc` availability separate from actual changed-diagram render proof.
- Add comparison or verification fields to generated-ledger records.
- Add validated and unvalidated field scope to machine-consumed ledger records.
- Declare that `Proof gap:` cannot be replaced by `[?]`, `[UNKNOWN]`, `[SKIP]`, the table absence token, or any compact glyph.

[PROMOTE_AS_BLOCKERS]:
- Any active edit that claims link, anchor, docs-build, table, fence-label, generated-contract, visual layout, or renderer proof must name the configured command or carry `Proof gap:`.
- Any current-version, support, lifecycle, beta, deprecated, latest, current, or imported-schema claim must carry source-specific definition plus freshness, or be rewritten as target-standard invariant.
- Any source-trace or generated-source claim must state what it proves and what validation remains unproved.

## [PROOF_GAPS]

- No active standards file was edited in this pass.
- No Mermaid render, GitHub render, SVG accessibility inspection, link checker, anchor checker, docs build, generated-output comparison, table validator, fence-label validator, or C# test rail ran in this pass.
- This report relies on the named reports' local and external source evidence; it does not add new web research.
- The only close gate for this report is `git diff --check -- .reports/standards-structure-notation-060626`.
