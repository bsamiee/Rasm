# [PROOF]

Proof is a claim-level obligation. Each command, contract, status, version, support statement, generated artifact, diagram, procedure, or provider claim must point to evidence strong enough to refresh it. This standard owns evidence strength, freshness, conflict resolution, and verification, and it does not own where a claim sits, which container presents it, or the words that phrase it.

## [1][USE_WHEN]

Apply this standard when documentation states:

- commands, flags, outputs, quality gates, or expected success signals;
- package, platform, runtime, language, host, API, or support versions;
- generated contracts, schemas, public API surfaces, diagrams, or codemaps;
- support status, deprecation, security posture, and end-of-life facts;
- operational procedures, rollback, escalation, and recovery checks;
- machine-facing indexes, retrieval metadata, generated mirrors, tool catalogs, or other agent surfaces whose behavior is the claim;
- current external-provider behavior.

Add metadata or proof fields only when the project has an actual need: a drift-prone claim, a reader trust need, or a named tool, renderer, generator, retrieval index, or review workflow that consumes the field. Never use evidence fields as page decoration.

## [2][EVIDENCE_HIERARCHY]

Use the strongest source that directly proves the claim:

1. Machine-readable repository truth: source, manifests, lockfiles, schemas, generated contracts, generated API reference, checked-in diagram models, or symbol documentation generated from source.
2. Executed local verification: the exact command, check, test, build, render, scenario run, link check, or docs build captured during the change.
3. Official primary documentation: vendor docs, specifications, release notes, support policies, API references, or standards documents.
4. Maintainer-controlled secondary material: project examples, migration guides, known limitations, issue records, or owning-project discussions.
5. Community material: discovery context only, never final proof when a stronger source exists.

Repository truth and generated contracts outrank prose. Local command output outranks a copied transcript. Current official docs outrank unofficial examples for actively changing tools.

## [3][CONFLICT_HANDLING]

When sources disagree, use the source closest to the executing system:

- generated contracts beat hand-written prose;
- manifests and lockfiles beat install instructions;
- source and generated symbol docs beat architecture summaries;
- local command output beats a copied result;
- official versioned docs beat community examples;
- newer primary sources beat older primary sources for changing tools.

If a lower source remains useful, cite it as background and state which higher source controls the claim.

## [4][PROOF_FIELDS]

This standard is the single owner of proof, source, freshness, and generated-artifact field labels. Use the smallest field set that keeps the claim maintainable:

- `Evidence:` names the source or command that proves the claim.
- `Last verified: YYYY-MM-DD` records observed behavior.
- `Review trigger:` names the event that makes the claim stale.
- `Generated from:` names the source model, contract, command, or workflow.
- `Source of truth:` names the owning contract, manifest, model, or path.

Use these labels exactly when a human-readable field is needed. Machine-consumed metadata may map to these facts only when the consuming tool, renderer, generator, retrieval index, or review workflow names the exact shape it reads; the machine schema owns its own field casing, and this standard owns the human-facing labels and proof meaning.

Prefer an event trigger over a calendar review date. Use a calendar date only when the external source changes on a schedule or no better trigger exists.

Attach the fields as a definition block beside the claim, one `label: value` per line, so each field is independently scannable and updatable:

```markdown template
`uv run python -m tools.quality static build`
Evidence: `tools/quality/__main__.py` `static build` verb; restore + build + analyzers, no tests.
Last verified: 2026-06-04
Review trigger: static rail verbs or routing change in `tools/quality`.
```

Carry only the fields the claim needs: a generated artifact adds `Generated from:` and `Source of truth:`, and a settled command may need only `Evidence:`. When several claims share one schema, group them per the definition-block rules [information-structure.md](information-structure.md) owns.

## [5][EVIDENCE_PLACEMENT]

Place evidence close to the claim it proves. A source inventory at the top or bottom of a page does not prove individual commands, versions, statuses, or provider behaviors unless each claim clearly maps back to that source. Use page-level proof only when every material claim shares one source and one freshness trigger; otherwise, attach claim-level evidence beside the drift-prone fact, table row, diagram caption, procedure, or generated artifact.

## [6][ASSERTION_UNCERTAINTY]

State a verified fact plainly, and mark a genuine gap explicitly. A qualifier is load-bearing only when evidence is genuinely uncertain; in that case, keep it and name the missing source, the unrun check, or the provisional status. When no evidence basis for doubt exists, the hedge is noise and the phrasing standard removes it. An existence, optionality, or scope qualifier on a fact — `optional`, `if present`, `where supported` — is part of the fact, so preserve it when you relocate the fact, because flattening a hedged claim into an unconditional one loses information. Never present training-data recall or assumption as verified fact; if a claim cannot be checked against a current source during the change, state that gap and mark the claim provisional rather than asserting it.

## [7][PRESERVATION_REFACTOR]

A refactor relocates content; it never drops it. Restructuring a document, merging sections, or moving a rule to its owner must preserve every load-bearing fact — each command, version, flag, path, invariant, routing pointer, field, and qualifier survives somewhere in the result. Before replacing a document, diff the new version's content coverage against the prior one and confirm nothing material disappeared; a dropped fact is a regression, not a simplification. When a leaner rewrite would remove a concrete proof command, a dependency, or a non-derivable constraint, the rewrite is wrong, not the original. Treat a vanished load-bearing item as a blocker that fails verification, exactly as a broken link or an unrun gate does.

## [8][EXTERNAL_RESEARCH]

Use primary sources first:

- Prefer official docs, specifications, release notes, package manifests, and source repositories.
- Use current sources for changing tools, APIs, security guidance, support status, and provider behavior.
- Stable standards may cite canonical official docs without arbitrary recency churn when the rule is settled.
- Record version, date, commit, or page-update signal when the source exposes one, and replace third-party tutorials with primary docs before publication.

If current official docs are unavailable, state the gap and mark the source as provisional.

## [9][DOCS_CODE_VERIFICATION]

This is the canonical docs-as-code gate ladder for the standard; the external-research, evidence-format, and agent-surface sections defer here for which gate a changed claim requires. Match the changed-claim condition to the required gate:

| [INDEX] | [CLAIM_CHANGED]                           | [GATE]                                           |
| :-----: | :---------------------------------------- | :----------------------------------------------- |
|   [1]   | Markdown content only                     | `git diff --check` on changed Markdown           |
|   [2]   | Structure, tables, examples, generated MD | configured formatter or linter                   |
|   [3]   | Links added, removed, renamed, generated  | link checker or local path/anchor validation     |
|   [4]   | Navigation, diagrams, config, docs output | docs build                                       |
|   [5]   | Generated contract claim                  | regenerate or compare generated output to source |
|   [6]   | Operational procedure                     | run steps or mark review-only                    |
|   [7]   | Visual layout claim                       | render screenshots, diagrams, PDFs, or pages     |

Do not claim a gate passed unless it ran in the current change or a current status check proves it — knowing a gate would pass is not proof it did. If no configured gate exists, state that rather than inventing one; a local validation script is a local check, not a configured repository gate.

## [10][AGENT_SURFACE_EVALUATION]

Treat a machine-facing surface as a contract when it affects retrieval, generated mirrors, tool use, or structured output. Prove that contract with evaluation rather than assertion.

A deterministic surface needs a minimum receipt: surface, baseline or prior behavior, checks, result, and freshness trigger. Add the rigor fields below only when the claim depends on stochastic output, ranking, tool selection, latency, or provider behavior:

- 20–50 representative questions or tasks drawn from real maintenance failures, not invented happy paths;
- a baseline comparison against the previous surface, manual route, or known failure;
- 3–5 trials per case when the claim is about stochastic output, retrieval ranking, or tool selection;
- a Wilson score 95% confidence interval for binary rates — task pass, valid JSON, instruction-following — and a paired comparison against the baseline on the same task set;
- exact checks for format and link correctness, plus a judge or source-trace review for retrieved or generated answers;
- a transcript or trace, with model or provider version, configured tool set, token or context budget, latency, and tool errors when the surface owns them;
- an unsupported-claim review and a tool-call failure review.

Record the evaluation as a definition block beside the surface it proves. Keep the minimum receipt visible for deterministic surfaces:

```markdown template
Surface: `docs/standards/_index.json` retrieval index.
Baseline: prior flat README link list.
Checks: exact file links and heading-anchor validity.
Result: index links resolve and no orphan target remains.
Last verified: 2026-06-04
Review trigger: standard filename, heading label, route map, or index-generation change.
```

Add rigor fields only when the surface owns stochastic output, ranking, tool selection, latency, or provider behavior:

```markdown template
Questions: 24 drawn from real "which standard owns X" maintenance misses.
Baseline: prior flat README link list; new index resolves 22/24 vs 14/24.
Trials: 3 runs per question; ranking stable across runs.
Checks: exact link and heading-anchor validity; judge review of top-1 source trace.
Trace: model or provider version, tool set `{search, read}`, token budget 8k, p50 latency 1.4s, 0 tool errors.
Reviews: unsupported-claim review clean; tool-call-failure review clean.
Last verified: 2026-06-04
```

State the proof gap when a contract is reviewed by a human rather than enforced by tooling.

## [11][EVIDENCE_FORMAT]

Keep evidence short and reproducible:

- the exact command as run;
- the source path plus field, heading, symbol, or contract name;
- the versioned specification or official documentation link;
- the generated contract path and generation command;
- the rendered artifact path when visual output matters;
- the status-check name and result when CI is the proof;
- the known gap when proof is intentionally unavailable.

Do not paste long transcripts. Summarize the result and keep enough source detail for the next maintainer to reproduce the proof.

A compliant note names the command, the source path, and a freshness marker, and summarizes the outcome rather than reproducing it:

```markdown template
Claim: `static full` parity covers `Workspace.slnx`.
Evidence: `uv run python -m tools.quality static full`; restore/build/analyzers green across the solution closure.
Source of truth: `Workspace.slnx`; routing in `tools/quality/__main__.py`.
Last verified: 2026-06-04
```

The next note is `rejected`: it pastes an unbounded transcript, names no command or source, and asserts success the next maintainer cannot reproduce.

```markdown rejected
Ran the build and it works.
> Restoring... Building... 0 Warning(s) 0 Error(s)
> [hundreds of lines of MSBuild output]
```

The contrast is the rule: a note is reproducible when a maintainer can re-run the exact command against the named source, not when it shows that it once ran.

## [12][PROOF_DOCUMENT_TYPE]

The evidence hierarchy, freshness fields, and docs-as-code gates above govern every document. Do not restate generic proof per type. This section lists only type-distinct proof surfaces — obligations the hierarchy alone does not make specific enough for an authoring agent:

| [INDEX] | [TYPE]       | [DISTINCT_PROOF]                          |
| :-----: | :----------- | :---------------------------------------- |
|   [1]   | ADR          | confirmation and supersession evidence    |
|   [2]   | Design doc   | validation plan evidence                  |
|   [3]   | Roadmap      | milestone exit and dependency proof       |
|   [4]   | API doc      | generated contract or reference truth     |
|   [5]   | Contributing | workflow, PR, and sign-off evidence       |
|   [6]   | How-to       | executed path or stated proof gap         |
|   [7]   | Runbook      | triage-to-recovery evidence capture       |
|   [8]   | Onboarding   | exercises, review tasks, or owner signoff |

The table names the distinct proof surface only. The type standard owns the full field set: contributing preserves workflow evidence, PR proof fields, unrun gates, and enforced sign-off or commit-format proof; how-to preserves executed path or proof gap, outcome verification, and rollback proof when state-changing; runbook preserves triage, mitigation, rollback, escalation, recovery verification, and evidence capture.

## [13][BOUNDARIES]

- [agentic-documentation.md](agentic-documentation.md) owns machine-facing surface placement and evaluation posture; this standard owns proof field labels and proof meaning.
- [information-structure.md](information-structure.md) owns the container that presents an evidence table, caption, or labeled block.
- [style-guide.md](style-guide.md) owns the phrasing of a claim and the removal of filler hedging.
- [formatting.md](formatting.md) owns the markers and styling that present an evidence table or status field.
- [README.md](README.md) owns document-type routing and cross-standard links.

## [14][REVIEW_CHECKLIST]

**Evidence and freshness**
- [ ] Drift-prone claims have claim-level evidence.
- [ ] Evidence sits close enough to the claim to support maintenance.
- [ ] Repository truth and generated contracts outrank prose.
- [ ] External facts use primary sources where available.
- [ ] A freshness trigger or `Last verified` exists where a claim can drift.
- [ ] Commands and checks are exact and reproducible.

**Generated and agent surfaces**
- [ ] Generated content is linked or regenerated, not manually forked.
- [ ] Provider-specific behavior has current official proof.
- [ ] Agent surfaces carry deterministic receipt fields, and add baseline trials, source trace, and review fields where stochastic, ranking, tool-selection, latency, or provider behavior is the claim.

**Gaps and preservation**
- [ ] Genuine uncertainty is marked; unrun gates and proof gaps are stated.
- [ ] A refactor preserved every load-bearing fact; no command, version, field, or invariant was dropped.
