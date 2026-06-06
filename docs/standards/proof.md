# [PROOF]

Proof is a claim-level obligation. Each command, contract, status, version, support statement, generated artifact, diagram, procedure, or provider claim must point to reproducible evidence that lets the next maintainer refresh it. This standard carries evidence strength, freshness, conflict resolution, proof fields, proof gaps, preservation, and verification. It does not own where a claim sits, which container presents it, or the words that phrase it.

## [1][USE_WHEN]

Apply this standard when documentation states:
- commands, flags, outputs, quality gates, or expected success signals
- package, platform, runtime, language, host, API, or support versions
- generated contracts, schemas, public API surfaces, diagrams, or codemaps
- support status, deprecation, security posture, and end-of-life facts
- operational procedures, rollback, escalation, and recovery checks
- machine-facing indexes, retrieval metadata, generated mirrors, tool catalogs, or machine-facing surfaces whose behavior is the claim
- current external-provider behavior

Add proof fields only for a drift-prone claim, a named reader trust need, or a named tool, renderer, generator, retrieval index, or review workflow that consumes the field. Add non-proof machine-consumed fields only when the consumer names the exact schema it reads. Never use evidence fields as page decoration.

## [2][EVIDENCE_HIERARCHY]

Use the strongest source that directly proves the claim:
1. Machine-readable repository truth: source, manifests, lockfiles, schemas, generated contracts, generated API reference, checked-in diagram models, or symbol documentation generated from source.
2. Executed local verification: the exact command, check, test, build, render, scenario run, link check, or docs build captured during the change.
3. Maintained upstream source when repository truth is silent: versioned specifications, release notes, support policies, API references, or standards documents.
4. Source-controlled secondary material: project examples, migration guides, known limitations, issue records, or controlling-project discussions.

Repository truth and generated contracts outrank prose. Local command output outranks a copied transcript. Maintained upstream source outranks examples for actively changing tools.

## [3][CONFLICT_HANDLING]

When sources disagree, use the source closest to the executing system. The hierarchy above already decides most conflicts; this section states only disagreement behavior:
- If generated output and handwritten prose disagree, use the generated output and fix or route the prose.
- If local command output and copied transcripts disagree, use the local command output.
- If maintained upstream sources disagree, use the newer maintained source for changing tools and record the dated source.

If a lower source remains useful, cite it as background and state which higher source controls the claim.

## [4][PROOF_FIELDS]

This standard is the single route for proof, source, freshness, proof-gap, and generated-artifact field labels. Use the smallest field set that keeps the claim maintainable.

| [INDEX] | [FIELD]                     | [ADD_WHEN]                                                 | [OMIT_WHEN]                          |
| :-----: | :-------------------------- | :--------------------------------------------------------- | :----------------------------------- |
|   [1]   | `Evidence:`                 | source, command, output, render, or policy proves it       | no drift-prone proof field is needed |
|   [2]   | `Generated from:`           | artifact is generated or mirrored                          | artifact is handwritten source       |
|   [3]   | `Source of truth:`          | one contract, manifest, model, path, or route owns it      | `Evidence:` names the only source    |
|   [4]   | `Proof gap:`                | source, command, render, or validation is missing          | claim is proved                      |
|   [5]   | `Last verified: YYYY-MM-DD` | observed or external behavior can drift                    | event trigger is enough              |
|   [6]   | `Review trigger:`           | source, route, renderer, generator, or policy can stale it | claim cannot drift or is deleted     |

Use these labels exactly when a human-readable field is needed. Machine-consumed fields may map to these facts only when the consuming tool, renderer, generator, retrieval index, or review workflow names the exact shape it reads; the machine schema carries its own field casing, and this standard carries the human-facing labels and proof meaning.

Footnotes may attach short provenance to a sentence, but they never replace visible proof fields for drift-prone claims. If a command, renderer, package, support state, generated artifact, or provider behavior can drift, put the proof field beside the claim, table row, diagram caption, record, or procedure.

Local identity or context fields such as `Gate:`, `Surface:`, `Representation:`, or `Proof route:` may precede the proof fields. The proof-local labels then appear contiguously in this order: `Evidence:`, `Generated from:`, `Source of truth:`, `Proof gap:`, `Last verified:`, `Review trigger:`. Local result, match, disposition, or close fields follow that proof field run unless a named evaluation receipt declares its own order.

Prefer an event trigger over a calendar review date. Use a calendar date only when a maintained policy changes on a schedule or no better trigger exists.

Attach the fields as a definition block beside the claim, one `label: value` per line, so each field is independently scannable and updatable:

```markdown template
Command: `uv run python -m tools.quality static build`
Evidence: `tools/quality/__main__.py` `static build` verb; restore + build + analyzers, no tests.
Last verified: 2026-06-04
Review trigger: static rail verbs or routing change in `tools/quality`.
```

Carry only the fields the claim needs. A generated artifact adds `Generated from:` and `Source of truth:`. A settled command may need only `Evidence:`. A missing check uses `Proof gap:` plus `Review trigger:` when the gap can be closed. When several claims share one schema, follow the definition-block rules in [information-structure.md](information-structure.md).

## [5][EVIDENCE_PLACEMENT]

Place evidence close to the claim it proves.

| [INDEX] | [CLAIM]                   | [PLACE]                  | [REJECT]                    |
| :-----: | :------------------------ | :----------------------- | :-------------------------- |
|   [1]   | one drift-prone claim     | beside the claim         | page-level source inventory |
|   [2]   | table row                 | row-owned note or record | unrelated page footer       |
|   [3]   | diagram or renderer claim | caption or proof record  | hidden comment              |
|   [4]   | shared source and trigger | page-level proof record  | mixed-source page proof     |

[PAGE_LEVEL_PROOF]:
- Scope: claims covered by the shared source.
- Shared source: one source, command, generated contract, or maintained policy.
- Shared freshness: one `Last verified:` value or `Review trigger:`.
- Exclusions: commands, versions, statuses, provider behaviors, generated artifacts, or test-tool facts that do not share the source.
- Rule: any exception gets claim-level proof.

Legacy page-level labels such as `[SOURCE]` are not sufficient proof for drift-prone API, package, support, generated, or test-tool facts unless every material fact on the page shares that exact source and freshness trigger.

## [6][ASSERTION_UNCERTAINTY]

State a verified fact plainly. Mark a genuine gap explicitly.

[VERIFIED]:
- Use direct prose.
- Attach `Evidence:` where the fact can drift.

[PROOF_GAP]:
- Use `Proof gap:` to name the missing source, unrun check, unsupported renderer proof, or provisional status.
- Add `Review trigger:` when a future event can close the gap.
- Do not replace `Proof gap:` with `[?]`, `[UNKNOWN]`, `[SKIP]`, `n/a`, `—`, or any compact glyph. Those markers can describe source values, runtime states, skipped gate results, or absent table cells only under their declared families; missing evidence uses the proof field.

[LOAD_BEARING_QUALIFIER]:
- Preserve `optional`, `if present`, `where supported`, and similar scope qualifiers when they are part of the fact.
- Do not flatten a scoped claim into an unconditional one.

[UNSUPPORTED_RECALL]:
- Never present training-data recall or assumption as verified fact.
- If a claim cannot be checked against a current source during the change, state the proof gap and mark the claim provisional.

## [7][PRESERVATION_REFACTOR]

A refactor relocates content; it never drops load-bearing current facts.

1. Inventory commands, versions, flags, paths, invariants, routing pointers, fields, and qualifiers before replacement.
2. Map each load-bearing fact to its new location, route-away owner, or deletion proof.
3. Compare the replacement against the prior content coverage.
4. Treat a vanished load-bearing fact as a verification failure.

[PRESERVATION_RECEIPT]:
- Prior source: file or section replaced.
- Replacement scope: file or section that now carries the content.
- Preserved facts: load-bearing facts retained.
- Routed facts: facts moved to an owner standard or adjacent document.
- Deleted facts: stale facts removed with proof.
- Comparison method: diff, checklist, table, or reviewer pass.
- Result: preserved, routed, deleted with proof, or blocked.

## [8][SOURCE_RESEARCH]

Use maintained sources first:
- Prefer repository source, generated contracts, manifests, lockfiles, release notes, package manifests, and source repositories.
- Use current sources for changing tools, APIs, security guidance, support status, and provider behavior.
- Record version, date, commit, or page-update signal when the source exposes one.

If current source is unavailable, state the gap and mark the claim provisional.

[CLAIM_VERIFICATION]:
- Declaration: the exact claim being made.
- Provenance: where the claim came from.
- Expectation: value, behavior, output, support state, or contract the claim predicts.
- Verification: source comparison, command, render, generated-output comparison, or human review that checks the claim.
- Failure state: proof gap, stale-source marker, or correction route when verification is unavailable or fails.

[STALE_SOURCE_RULE]:
- Terms such as `latest`, `current`, `supported`, `deprecated`, `obsolete`, `legacy`, `experimental`, `beta`, `soon`, and `future` require a source-specific definition plus `Last verified:` or `Review trigger:` when they describe present behavior.
- Rewrite the claim as a source-independent target standard when current availability is not the point.

## [9][DOCS_CODE_VERIFICATION]

This is the canonical docs-as-code gate ladder for this standards library. Source research, evidence format, and agent-surface sections use this ladder to choose the required gate.

Hit policy: run every configured gate whose row matches the changed claim. If no configured gate exists, state `Proof gap: no configured <gate class> exists` rather than inventing one. A final validation line uses one of 3 states: `ran <exact command>`, `not applicable because <claim class did not change>`, or `Proof gap: no configured <gate class> command named`.

| [INDEX] | [CLAIM_CHANGED]                           | [GATE]                                           |
| :-----: | :---------------------------------------- | :----------------------------------------------- |
|   [1]   | Markdown content only                     | `git diff --check` on changed Markdown           |
|   [2]   | Structure, tables, examples, generated MD | configured formatter or linter                   |
|   [3]   | Links or anchors                          | link checker or local path/anchor validation     |
|   [4]   | Navigation, diagrams, config, docs output | docs build                                       |
|   [5]   | Generated contract claim                  | regenerate or compare generated output to source |
|   [6]   | Operational procedure                     | run steps or state proof gap                     |
|   [7]   | Visual layout claim                       | render screenshots, diagrams, PDFs, or pages     |

The link-or-anchor row includes added, removed, renamed, or generated links and heading anchors.

[RENDERER_CLAIMS]:
- Mermaid render behavior.
- Mermaid `config:` support.
- GitHub alert support.
- `<details>` rendering.
- Footnote rendering.
- Diagram accessibility text.
- Generated diagram output.

Renderer-dependent documentation claims require proof from the renderer or `Proof gap:`. Mermaid, GitHub, and `mmdc` support claims need local render proof when the repository depends on that rendering. Tool availability proves a route exists; it does not prove a changed diagram rendered.

[CONFIGURED_GATE_RECORD]:
- Gate class: whitespace, link, anchor, docs build, renderer, generated contract, procedure, visual layout, provider behavior, or custom class.
- Configured command or source: exact command, status check, maintained source, or `Proof gap:`.
- Applies to: changed claim class.
- Proves: what the gate verifies.
- Does not prove: nearby claim class the gate cannot verify.
- Review trigger: command, renderer, source, or claim-class change.

[RENDERER_SOURCE_RECORD]:
- Source class: CommonMark/GFM syntax, GitHub product behavior, official Mermaid syntax/config, local `mmdc` availability, local render output, or generated SVG accessibility output.
- Applies to: syntax, renderer support, local output, accessibility, or visual layout claim.
- Configured command or source: exact command or maintained source.
- Proves: the bounded renderer behavior checked.
- Does not prove: unrendered local diagrams, GitHub-hosted behavior, SVG accessibility, or visual layout unless the row names that proof.
- Proof gap: missing renderer proof where applicable.
- Last verified: date when observed behavior can drift.
- Review trigger: renderer, Mermaid version, GitHub behavior, diagram source, or command route change.

Do not claim a gate passed unless it ran in the current change or a current status check proves it. Knowing a gate would pass is not proof it did. A local validation script is a local check, not a configured repository gate.

## [10][AGENT_SURFACE_EVALUATION]

Treat a machine-facing surface as a contract when it affects retrieval, generated mirrors, tool use, or structured output. Prove that contract with evaluation rather than assertion.

Machine-facing contract semantics are defined in [agentic-documentation.md](agentic-documentation.md): shape enforcement, source provenance, semantic validation, and runtime safety. This section defines the evidence and receipt fields for those checks.

Declare evaluation receipt fields before examples.

[DETERMINISTIC_RECEIPT]:
- Surface: machine-facing surface being proved.
- Baseline: prior behavior, manual route, or known failure.
- Checks: exact checks run.
- Evidence: source, command, trace, or review proof.
- Result: observed outcome.
- Last verified: date of observed behavior.
- Review trigger: event that makes the receipt stale.

[RIGOR_FIELDS]:
- Questions: 20–50 representative questions or tasks drawn from real maintenance failures, not invented happy paths.
- Trials: 3–5 runs per case when the claim is stochastic output, retrieval ranking, or tool selection.
- Statistics: Wilson score 95% confidence interval for binary rates and paired comparison against the baseline on the same task set.
- Trace: model or provider version, configured tool set, token or context budget, latency, and tool errors when the surface carries them.
- Reviews: format, link correctness, source-trace, unsupported-claim, and tool-call-failure reviews.

Record the evaluation as a definition block beside the surface it proves. Keep the minimum receipt visible for deterministic surfaces:

```markdown template
Surface: `<retrieval-index-path>` retrieval index.
Baseline: prior flat README link list.
Checks: exact file links and heading-anchor validity.
Evidence: local link and anchor validation.
Result: index links resolve and no orphan target remains.
Last verified: 2026-06-04
Review trigger: standard filename, heading label, route map, or index-generation change.
```

Add rigor fields to the same receipt only when the surface carries stochastic output, ranking, tool selection, latency, or provider behavior. This fragment extends the deterministic receipt; it does not replace it:

```markdown template
Questions: 24 drawn from real "which standard carries X" maintenance misses.
Baseline: prior flat README link list; new index resolves 22/24 vs 14/24.
Trials: 3 runs per question; ranking stable across runs.
Checks: exact link and heading-anchor validity; judge review of top-1 source trace.
Evidence: trace and reviewed source spans.
Trace: model or provider version, tool set `{search, read}`, token budget 8k, p50 latency 1.4s, 0 tool errors.
Reviews: unsupported-claim review clean; tool-call-failure review clean.
Last verified: 2026-06-04
```

State `Proof gap:` when a contract is reviewed by a human rather than enforced by tooling.

## [11][EVIDENCE_FORMAT]

Keep evidence short and reproducible:
- the exact command as run
- the source path plus field, heading, symbol, or contract name
- the versioned source, specification, or maintained policy link
- the generated contract path and generation command
- the rendered artifact path when visual output matters
- the status-check name and result when CI is the proof
- the known gap when proof is intentionally unavailable

Do not paste long transcripts. Summarize the result and keep enough source detail for the next maintenance route to reproduce the proof.

Source provenance is not semantic validation. Provenance proves origin and refresh route, shape enforcement proves the container, semantic validation proves the claim matches the source, evaluation receipts prove retrieval quality, ranking, tool choice, latency, or provider behavior, and runtime safety proves authorization and downstream suitability.

A compliant note names the command, the source path, and a freshness marker and summarizes the outcome rather than reproducing it:

```markdown template
Claim: `static full` parity covers `Workspace.slnx`.
Evidence: `uv run python -m tools.quality static full`; restore, build, and analyzer checks passed across the solution closure.
Source of truth: `Workspace.slnx`; routing in `tools/quality/__main__.py`.
Last verified: 2026-06-04
```

The next note is `rejected`: it pastes an unbounded transcript, names no command or source, and asserts success the next maintenance route cannot reproduce.

```markdown rejected
Ran the build and it works.
> Restoring... Building... 0 Warning(s) 0 Error(s)
> [hundreds of lines of MSBuild output]
```

The contrast is the rule: a note is reproducible when a maintenance route can re-run the exact command against the named source, not when it preserves only a past transcript.

## [12][PROOF_DOCUMENT_TYPE]

The evidence hierarchy, freshness fields, proof labels, preservation rule, and docs-as-code gates govern every document. Type standards may name artifact-specific proof slots, examples, and route-away rules where the shared hierarchy is not specific enough. Do not restate generic proof per type.

## [13][BOUNDARIES]

- [agentic-documentation.md](agentic-documentation.md) carries machine-facing placement and contract separation; this standard carries evidence strength, receipt fields, freshness, and evaluation proof.
- [information-structure.md](information-structure.md) carries the container that presents an evidence table, caption, or labeled block.
- [style-guide.md](style-guide.md) carries the phrasing of a claim and the removal of filler hedging.
- [formatting.md](formatting.md) carries the markers and styling that present an evidence table or status field.
- [README.md](README.md) carries document-type routing and cross-standard links.

## [14][VALIDATION]

Use this verification checklist by group:

[EVIDENCE_FRESHNESS]:
- [ ] Drift-prone claims have claim-level evidence.
- [ ] Evidence sits close enough to the claim to support maintenance.
- [ ] Repository truth and generated contracts outrank prose.
- [ ] Drift-prone facts use maintained sources where available.
- [ ] A freshness trigger or `Last verified` exists where a claim can drift.
- [ ] Commands and checks are exact and reproducible.

[GENERATED_AGENT]:
- [ ] Generated content is linked or regenerated, not manually forked.
- [ ] Provider-specific behavior has current maintained proof.
- [ ] Renderer-dependent claims have local render proof, maintained renderer proof, or an explicit gap.
- [ ] Schema or structured-output proof is separated from semantic source-trace, validation, application, and runtime-safety proof.
- [ ] Agent surfaces carry deterministic receipt fields and add rigor fields where stochastic, ranking, tool-selection, latency, or provider behavior is the claim.

[GAPS_PRESERVATION]:
- [ ] Genuine uncertainty is marked; unrun gates and proof gaps are stated.
- [ ] A refactor preserved every load-bearing fact; no command, version, field, or invariant was dropped.
- [ ] A refactor has a preservation receipt when content was replaced or relocated.
