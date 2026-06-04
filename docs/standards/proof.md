---
description: Cross-cutting evidence, freshness, and verification standard
---

# Proof

Proof is a claim-level obligation. Each command, contract, status, version,
support statement, generated artifact, diagram, procedure, or provider claim
must point to evidence strong enough to refresh it.

## Use when

Use this standard when documentation states:

- commands, flags, outputs, quality gates, or expected success signals;
- package, platform, runtime, language, host, API, or support versions;
- generated contracts, schemas, public API surfaces, diagrams, or codemaps;
- support status, deprecation, security posture, and end-of-life facts;
- operational procedures, rollback, escalation, and recovery checks;
- machine-facing indexes, retrieval metadata, generated mirrors, or tool
  catalogs;
- current external-provider behavior.

Do not use proof fields as page decoration. Add them only when the claim can
drift or the reader needs the source to trust the claim.

## Evidence hierarchy

Use the strongest source that directly proves the claim:

1. Machine-readable repository truth: source code, manifests, lockfiles,
   schemas, generated contracts, generated API reference, checked-in diagram
   models, or public symbol documentation generated from source.
2. Executed local verification: exact command, check, test, build, render,
   scenario run, link check, or docs build captured during the change.
3. Official primary documentation: vendor docs, specifications, release notes,
   support policies, API references, or standards documents.
4. Maintainer-controlled secondary material: project examples, migration guides,
   known limitations, issue records, or discussions from the owning project.
5. Community material: discovery context only, not final proof when a stronger
   source exists.

Repository truth and generated contracts outrank prose. Local command output
outranks copied transcripts. Current official docs outrank unofficial examples
for actively changing tools.

## Conflict handling

When sources disagree, use the source closest to the executing system:

- generated contracts beat hand-written prose;
- manifests and lockfiles beat install instructions;
- source and generated symbol docs beat architecture summaries;
- local command output beats a copied result;
- official versioned docs beat community examples;
- newer primary sources beat older primary sources for changing tools.

If a lower source is still useful, cite it as background and state which higher
source controls the claim.

## Freshness fields

Use the smallest field that keeps the claim maintainable:

- `Evidence:` names the source or command that proves the claim.
- `Last verified: YYYY-MM-DD` records observed behavior.
- `Review trigger:` names the event that makes the claim stale.
- `Generated from:` names the source model, contract, command, or workflow.
- `Source of truth:` names the owning contract, manifest, model, or path.

Prefer event triggers over calendar review dates. Use a calendar date when the
external source changes on a schedule or no better trigger exists.

## Evidence placement

Place evidence close to the claim it proves. A source inventory at the top or
bottom of a page does not prove individual commands, versions, statuses, or
provider behaviors unless each claim clearly maps back to that source.

Use page-level proof only when every material claim in the page shares the same
source and freshness trigger. Otherwise, add claim-level evidence beside the
drift-prone fact, table row, diagram caption, procedure, or generated artifact.

## External research

Research uses primary sources first:

- Prefer official docs, specifications, release notes, package manifests, and
  source repositories.
- Use current sources for changing tools, APIs, security guidance, support
  status, and provider behavior.
- Stable standards may cite canonical official docs without arbitrary recency
  churn when the rule is settled.
- Record version, date, commit, or page update signal when the source exposes
  one.
- Replace third-party tutorials with primary docs before publication.

If current official docs are unavailable, state the gap and mark the source as
provisional.

## Docs-as-code verification

Choose the gate that proves the changed claim:

- Markdown-only standards edits: run `git diff --check -- docs/standards` at
  minimum.
- Markdown validity: run the configured formatter or linter when structure,
  tables, examples, or generated Markdown changed.
- Link integrity: run the configured link checker when links are added,
  removed, renamed, or generated.
- Site or reference build: run the docs build when navigation, generated
  reference output, examples, diagrams, or site configuration changed.
- Contract generation: regenerate or compare generated API, schema, CLI, or
  diagram output when the documented contract is generated.
- Procedure proof: run the documented steps, or state why the step is a review
  gate instead of an executable command.
- Visual proof: render screenshots, diagrams, PDFs, or pages when visual layout
  is the claim.

Do not claim a gate passed unless it ran in the current change or a current
status check proves it. If no configured gate exists, state that rather than
inventing one.

## Evidence format

Keep evidence short and reproducible:

- exact command as run;
- source path plus field, heading, symbol, or contract name;
- versioned specification or official documentation link;
- generated contract path and generation command;
- rendered artifact path when visual output matters;
- status-check name and result when CI is the proof;
- known gap when proof is intentionally unavailable.

Do not paste long transcripts. Summarize the result and keep enough source
detail for the next maintainer to reproduce the proof.

## Proof by document type

- README: entry links resolve, advertised setup or usage is current, and status
  claims point to the owning source.
- Architecture: diagrams, codemap, contracts, and invariants match repository
  truth or generated models.
- ADR: confirmation evidence states how the decision can be observed after
  acceptance; supersession links are current.
- Design document: validation plan names the commands, contracts, runtime
  checks, owner reviews, or risks that will prove the proposal.
- Roadmap: every milestone has exit criteria, dependencies, and a proof surface.
- Reference and support: drift-prone facts cite source, command, generated
  metadata, or official primary documentation.
- API documentation: generated contract or generated reference is the source of
  truth; prose links to it instead of duplicating it.
- Code documentation: public symbol comments match source behavior and
  generated reference output.
- How-to and tutorial: the documented path was run or proof gaps are explicit.
- Runbook: triage, mitigation, rollback, escalation, and recovery verification
  are executable or explicitly marked as review gates.
- Contributing and test strategy: workflow and gate names map to current
  runnable commands or maintained automation.
- Onboarding: readiness criteria include observable exercises, review tasks, or
  owner sign-off.
- Agentic documentation: indexes point to canonical docs, generated mirrors are
  marked, and provider claims use primary sources.

## Review checklist

- [ ] Drift-prone claims have claim-level evidence.
- [ ] Evidence appears close enough to the claim to support maintenance.
- [ ] Repository truth and generated contracts outrank prose.
- [ ] External facts use primary sources where available.
- [ ] Freshness trigger or `Last verified` exists where the claim can drift.
- [ ] Commands and checks are exact and reproducible.
- [ ] Generated content is linked or regenerated, not manually forked.
- [ ] Provider-specific behavior has current official proof.
- [ ] Unrun gates and proof gaps are stated.
