---
description: Evidence strength, freshness, verification, and agent-surface evaluation
---

# Proof

Proof is a claim-level obligation. Each command, contract, status, version, support statement, generated artifact, diagram, procedure, or provider claim must point to evidence strong enough to refresh it. This standard owns evidence strength, freshness, conflict resolution, and verification. It does not own where a claim sits, which container presents it, or the words that phrase it.

## Use when

Apply this standard when documentation states:

- commands, flags, outputs, quality gates, or expected success signals;
- package, platform, runtime, language, host, API, or support versions;
- generated contracts, schemas, public API surfaces, diagrams, or codemaps;
- support status, deprecation, security posture, and end-of-life facts;
- operational procedures, rollback, escalation, and recovery checks;
- machine-facing indexes, retrieval metadata, generated mirrors, tool catalogs, or other agent surfaces whose behavior is the claim;
- current external-provider behavior.

Add proof fields only when a claim can drift or a reader needs the source to trust it. Do not use evidence fields as page decoration.

## Evidence hierarchy

Use the strongest source that directly proves the claim:

1. Machine-readable repository truth: source, manifests, lockfiles, schemas, generated contracts, generated API reference, checked-in diagram models, or symbol documentation generated from source.
2. Executed local verification: the exact command, check, test, build, render, scenario run, link check, or docs build captured during the change.
3. Official primary documentation: vendor docs, specifications, release notes, support policies, API references, or standards documents.
4. Maintainer-controlled secondary material: project examples, migration guides, known limitations, issue records, or owning-project discussions.
5. Community material: discovery context only, never final proof when a stronger source exists.

Repository truth and generated contracts outrank prose. Local command output outranks a copied transcript. Current official docs outrank unofficial examples for actively changing tools.

## Conflict handling

When sources disagree, use the source closest to the executing system:

- generated contracts beat hand-written prose;
- manifests and lockfiles beat install instructions;
- source and generated symbol docs beat architecture summaries;
- local command output beats a copied result;
- official versioned docs beat community examples;
- newer primary sources beat older primary sources for changing tools.

If a lower source is still useful, cite it as background and state which higher source controls the claim.

## Freshness fields

Use the smallest field that keeps the claim maintainable:

- `Evidence:` names the source or command that proves the claim.
- `Last verified: YYYY-MM-DD` records observed behavior.
- `Review trigger:` names the event that makes the claim stale.
- `Generated from:` names the source model, contract, command, or workflow.
- `Source of truth:` names the owning contract, manifest, model, or path.

Prefer an event trigger over a calendar review date. Use a calendar date only when the external source changes on a schedule or no better trigger exists.

Attach the fields as a definition block beside the claim, one `label: value` per line, so each field is independently scannable and updatable:

```markdown copy-safe
`uv run python -m tools.quality static build`
Evidence: `tools/quality/__main__.py` `static build` verb; restore + build + analyzers, no tests.
Last verified: 2026-06-04
Review trigger: static rail verbs or routing change in `tools/quality`.
```

Carry only the fields the claim needs: a generated artifact adds `Generated from:` and `Source of truth:`, a settled command may need only `Evidence:`. When several claims share one schema, group them per the definition-block rules information-structure.md owns.

## Evidence placement

Place evidence close to the claim it proves. A source inventory at the top or bottom of a page does not prove individual commands, versions, statuses, or provider behaviors unless each claim clearly maps back to that source. Use page-level proof only when every material claim shares one source and one freshness trigger; otherwise, attach claim-level evidence beside the drift-prone fact, table row, diagram caption, procedure, or generated artifact.

## Assertion and uncertainty

State a verified fact plainly, and mark a genuine gap explicitly. A qualifier is load-bearing only when evidence is actually uncertain; in that case keep it and name the missing source, the unrun check, or the provisional status. When there is no evidence basis for doubt, the hedge is noise and the phrasing standard removes it. An existence, optionality, or scope qualifier on a fact — `optional`, `if present`, `where supported` — is part of the fact; preserve it when you relocate the fact, because flattening a hedged claim into an unconditional one loses information. Do not present training-data recall or assumption as verified fact; if a claim cannot be checked against a current source during the change, say so and mark it provisional rather than asserting it.

## Preservation under refactor

A refactor relocates content; it never drops it. Restructuring a document, merging sections, or moving a rule to its owner must preserve every load-bearing fact — each command, version, flag, path, invariant, routing pointer, field, and qualifier survives somewhere in the result. Before replacing a document, diff the new version's content coverage against the prior one and confirm nothing material disappeared; a dropped fact is a regression, not a simplification. When a leaner rewrite would remove a concrete proof command, a dependency, or a non-derivable constraint, the rewrite is wrong, not the original. Treat a vanished load-bearing item as a blocker that fails verification, exactly as a broken link or an unrun gate does.

## External research

Use primary sources first:

- Prefer official docs, specifications, release notes, package manifests, and source repositories.
- Use current sources for changing tools, APIs, security guidance, support status, and provider behavior.
- Stable standards may cite canonical official docs without arbitrary recency churn when the rule is settled.
- Record version, date, commit, or page-update signal when the source exposes one, and replace third-party tutorials with primary docs before publication.

If current official docs are unavailable, state the gap and mark the source as provisional.

## Docs-as-code verification

This is the canonical docs-as-code gate ladder for the standard; the external-research, evidence-format, and agent-surface sections defer here for which gate a changed claim requires. Match the changed-claim condition to the required gate:

| Changed claim | Required gate |
| --- | --- |
| Markdown content only, no structure or links | `git diff --check` on the changed Markdown, at minimum. |
| Structure, tables, examples, or generated Markdown | the configured formatter or linter. |
| Links added, removed, renamed, or generated | the configured link checker. |
| Navigation, generated reference output, examples, diagrams, or configuration | the docs build. |
| A documented contract that is generated (API, schema, CLI, diagram) | regenerate or compare the generated output against source. |
| An operational procedure | run the documented steps, or state why a step is a review gate rather than an executable command. |
| Visual layout is the claim | render screenshots, diagrams, PDFs, or pages. |

Do not claim a gate passed unless it ran in the current change or a current status check proves it — recall that a gate would pass is not proof it did. If no configured gate exists, state that rather than inventing one.

## Agent-surface evaluation

Treat a machine-facing surface as a contract when it affects retrieval, generated mirrors, tool use, or structured output, and prove it with evaluation rather than assertion. Useful evaluation includes:

- representative questions or tasks drawn from real maintenance failures, not invented happy paths;
- a baseline comparison against the previous surface, manual route, or known failure;
- repeated trials when the claim is about stochastic output, retrieval ranking, or tool selection;
- exact checks for format and link correctness, plus a judge or source-trace review for retrieved or generated answers;
- a transcript or trace, with model or provider version, configured tool set, token or context budget, latency, and tool errors when the surface owns them;
- an unsupported-claim review and a tool-call failure review.

Record the evaluation as a definition block beside the surface it proves, so every required field is present and an author cannot collapse the receipt into a bare assertion:

```markdown copy-safe
Surface: `docs/standards/_index.json` retrieval index.
Questions: 12 drawn from real "which standard owns X" maintenance misses.
Baseline: prior flat README link list; new index resolves 11/12 vs 6/12.
Trials: 3 runs per question; ranking stable across runs.
Checks: exact link + heading-anchor validity; judge review of top-1 source trace.
Trace: model `claude-opus-4-8`, tool set `{search, read}`, token budget 8k, p50 latency 1.4s, 0 tool errors.
Reviews: unsupported-claim review clean; tool-call-failure review clean.
Last verified: 2026-06-04
```

Omit a field only when the surface does not own it — a non-stochastic, deterministic surface drops `Trials`, a surface with no model in the loop drops the model line of `Trace`. State the proof gap when a contract is reviewed by a human rather than enforced by tooling.

## Evidence format

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

```markdown copy-safe
Claim: `static full` parity covers `Workspace.slnx`.
Evidence: `uv run python -m tools.quality static full` — restore/build/analyzers green across the solution closure.
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

## Proof by document type

The evidence hierarchy, freshness fields, and docs-as-code gates above govern every document; a README, Architecture page, Reference page, How-to, or Code-documentation comment carries no proof obligation those rules do not already derive, so do not restate the generic obligation per type. This section lists only the proof surfaces that are genuinely type-distinct — the obligation a type carries that the hierarchy alone does not produce:

| Document type | Type-distinct proof surface |
| --- | --- |
| ADR | Confirmation evidence states how the decision is observable after acceptance; supersession links are current. |
| Design document | A validation plan names the commands, contracts, runtime checks, owner reviews, or risks that will prove the proposal before it ships. |
| Roadmap | Every milestone carries exit criteria, dependencies, and a proof surface; see [explanation/roadmap.md](explanation/roadmap.md). |
| API documentation | The generated contract or reference is the source of truth; prose links to it instead of duplicating it. |
| Runbook | Triage, mitigation, rollback, escalation, and recovery verification are executable or explicitly marked as review gates. |
| Onboarding | Readiness criteria include observable exercises, review tasks, or owner sign-off rather than narrative completion. |

## Boundaries

- [agentic-documentation.md](agentic-documentation.md) owns where evidence sits in a unit and the metadata fields that carry proof to machines.
- [information-structure.md](information-structure.md) owns the container that presents an evidence table, caption, or labeled block.
- [style-guide.md](style-guide.md) owns the phrasing of a claim and the removal of filler hedging.
- [formatting.md](formatting.md) owns the markers and styling that present an evidence table or status field.
- [README.md](README.md) owns document-type routing and cross-standard links.

## Review checklist

- [ ] Drift-prone claims have claim-level evidence.
- [ ] Evidence sits close enough to the claim to support maintenance.
- [ ] Repository truth and generated contracts outrank prose.
- [ ] External facts use primary sources where available.
- [ ] A freshness trigger or `Last verified` exists where a claim can drift.
- [ ] Commands and checks are exact and reproducible.
- [ ] Generated content is linked or regenerated, not manually forked.
- [ ] Provider-specific behavior has current official proof.
- [ ] Agent surfaces are evaluated with baseline, trials, and source trace where the behavior is the claim.
- [ ] Genuine uncertainty is marked; unrun gates and proof gaps are stated.
- [ ] A refactor preserved every load-bearing fact; no command, version, field, or invariant was dropped.
