---
description: Diataxis map, document-type routing, placement, and lifecycle discipline
---

# Documentation system

Every durable document has one primary reader need, one owning place, one
source-of-truth relationship, and one refresh trigger. This standard routes a
draft to the right document type and prevents blended documents from becoming
hard to maintain.

## Use when

Use this standard when:

- creating a new document;
- deciding whether to split, merge, delete, or move documentation;
- choosing between tutorial, how-to, reference, explanation, README, ADR,
  design, roadmap, runbook, support matrix, test strategy, contributing, API,
  code documentation, or onboarding;
- resolving duplicate claims across documentation owners.

Do not use it for sentence mechanics, table shape, evidence fields, or
agent-facing metadata. Use the owning shared standard after the document type is
clear.

## Reader-need map

Diataxis is the corpus map:

- Tutorial: teach a learner through a complete, tested path.
- How-to: help a competent reader complete one real task.
- Reference: let a working reader look up accurate facts.
- Explanation: help a reader understand context, trade-offs, structure, or
  decisions.

Classify edge cases by action and cognition:

- Action plus learning is tutorial.
- Action plus work is how-to.
- Cognition plus work is reference.
- Cognition plus learning is explanation.

Do not create empty quadrant folders to look complete. Publish only documents
that answer a real reader need.

## Choose the type

Choose by reader outcome first, then artifact type:

- Entry point or local hub: [readme.md](reference/readme.md).
- Current system structure, boundaries, invariants, and trade-offs:
  [architecture.md](explanation/architecture.md).
- Durable decision with consequences and confirmation:
  [adr.md](explanation/adr.md).
- Pre-implementation proposal or RFC-style review:
  [design-doc.md](explanation/design-doc.md).
- Build sequence, milestone intent, and exit proof:
  [roadmap.md](explanation/roadmap.md).
- Test levels, gate ownership, evidence policy, or flaky-test handling:
  [test-strategy.md](explanation/test-strategy.md).
- One normal task for a competent reader: [how-to.md](task/how-to.md).
- Complete learning path with fixed inputs and observable result:
  [tutorial.md](learning/tutorial.md).
- Operational trigger, triage, mitigation, rollback, and escalation:
  [runbook.md](task/runbook.md).
- Curated lookup facts, glossaries, commands, host facts, or data dictionaries:
  [reference.md](reference/reference.md).
- HTTP contract, generated library reference, or public API surface:
  [api.md](reference/api.md).
- Public symbol intent, constraints, effect channels, or failure channels:
  [code-documentation.md](reference/code-documentation.md).
- Support truth for runtimes, platforms, versions, or features:
  [support-matrix.md](reference/support-matrix.md).
- Contribution workflow and pull request evidence:
  [contributing.md](task/contributing.md).
- Role ramp for contributors, maintainers, or operators:
  [onboarding.md](learning/onboarding.md).

If no type fits, reduce the scope until one reader outcome is primary.

## Placement

Place documentation where the reader or tool will first look:

- Repository root for corpus-wide entry documents.
- Owner-local directory for package, product, tool, or subsystem truth.
- `docs/` for shared cross-owner material, public documentation corpora, and
  generated documentation entrypoints.
- `docs/standards/` for authoring standards only.
- Source files for public symbol documentation and inline rationale that cannot
  be expressed by names or types.

Prefer one owner for a claim. Link across owners instead of copying the same
claim into multiple pages.

## Split and link

Split a draft when it tries to satisfy more than one primary reader need:

- Move background from tutorials and how-to guides into explanation.
- Move API catalogs, option tables, and command inventories into reference or
  API documentation.
- Move step-by-step work from reference leaves into how-to guides.
- Move operational response from how-to guides into runbooks.
- Move durable decision rationale from architecture into ADRs.
- Move implementation sequence and exit criteria from architecture into
  roadmaps or design documents.
- Move contribution workflow from README files into contributing guides.

After splitting, add the smallest useful cross-link: `Related`, `See also`,
`Decision`, `Reference`, or `Next steps`. Do not duplicate moved content as a
summary that can drift.

## Lifecycle

Documentation is maintained like code:

- Create the smallest useful document that answers the reader need.
- Update docs in the same change that alters the source truth they describe.
- Delete or replace dead documentation when it is known to be wrong.
- Add review triggers for drift-prone claims.
- Prefer explicit stale markers over silent uncertainty when a claim cannot be
  verified during maintenance.

Do not preserve old paths, terminology, commands, or product claims as
compatibility notes unless live product support and evidence justify them.

## Boundaries

- [style-guide.md](style-guide.md) owns prose, terminology, notation, links,
  examples, and accessibility.
- [information-structure.md](information-structure.md) owns page shape, lists,
  tables, diagrams, labeled blocks, and retrieval chunks.
- [proof.md](proof.md) owns evidence strength, freshness, source conflicts, and
  verification claims.
- [agentic-documentation.md](agentic-documentation.md) owns instruction files,
  machine-facing indexes, generated mirrors, retrieval, metadata, and catalogs.
- Type standards own placement, required sections, lifecycle, forbidden
  content, and type-specific proof.

## Anti-patterns

- README files that carry design history, tutorials, or API catalogs.
- Architecture documents that carry task plans or incident response steps.
- ADRs that include execution procedures instead of decision confirmation.
- Tutorials that branch into production variants.
- Reference leaves that hide procedures in dense tables.
- Runbooks without observable trigger, owner, review date, verification, or
  escalation path.
- Generated API documentation forked by hand-written endpoint or symbol tables.
- Standards that mention authoring interactions, obsolete workflow names, or
  temporary task labels instead of durable document behavior.

## Review checklist

- [ ] The document has one primary reader need.
- [ ] The document type matches that need.
- [ ] Placement is near the owner that can refresh the truth.
- [ ] Adjacent reader needs are linked, not embedded.
- [ ] Drift-prone claims route to proof rules.
- [ ] Shared standards are linked only where they own the concern.
- [ ] Dead paths, old terms, and stale commands are removed rather than kept as
      aliases.
