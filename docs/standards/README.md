---
description: Index and router for the documentation standards library
---

# Documentation standards

This folder is the active standards library and router for Rasm documentation. Start here to find the reader need, pick the document type, and route to the shared standard that owns each rule. This index is the one file that links across the library; the standards it points to name their neighbors by topic and link back only at their boundaries.

## Use when

Use this index when:

- creating or rewriting Markdown documentation;
- deciding which reader need a draft serves and which type fits;
- deciding which standard owns a rule;
- splitting, merging, moving, or deleting a document;
- checking whether a standard is active.

Unless the user explicitly asks to integrate it, do not use inactive material in `_TMP/`.

## Owner precedence

Use the strongest applicable owner:

1. Current repository source, manifests, generated contracts, and runnable tool output.
2. The document-type standard in this folder.
3. The shared standard for position, form, craft, evidence, or notation.
4. External standards cited by the active local standard.

When a claim can drift, prove it with repository truth before web examples or prior notes.

## Read order

Read order is workflow order; owner precedence still decides conflicts.

1. Use the reader-need map and type chooser below to pick one document type.
2. Apply the five shared standards: position, form, craft, evidence, and notation.
3. Apply exactly one type standard. If a draft serves two reader needs, split it and link between the files.

## Reader-need map

Diátaxis is the corpus map. Classify a document by what the reader does:

- Tutorial: a tested path that teaches a learner.
- How-to: one real task for a competent reader.
- Reference: accurate facts a working reader looks up.
- Explanation: context, trade-offs, structure, or decisions a reader wants to understand.

Resolve edge cases by action and cognition: action plus learning is tutorial, action plus work is how-to, cognition plus work is reference, and cognition plus learning is explanation. Publish only documents that answer a real reader need; do not create empty quadrant folders to look complete.

## Choose the type

Map the reader need to its Diátaxis quadrant first, then to the artifact. The four quadrants below own the four type families in [Folder layout](#folder-layout) one-to-one, so the route reads need → quadrant → type in one pass.

Explanation (cognition + learning):

- Current structure, boundaries, and invariants: [architecture.md](explanation/architecture.md).
- Durable decision with consequences and confirmation: [adr.md](explanation/adr.md).
- Pre-implementation proposal or RFC-style review: [design-doc.md](explanation/design-doc.md).
- Build sequence, milestones, and exit proof: [roadmap.md](explanation/roadmap.md).
- Test levels, gates, evidence, or flake policy: [test-strategy.md](explanation/test-strategy.md).

Reference (cognition + work):

- Entry point or local hub: [readme.md](reference/readme.md).
- Curated lookup truth: [reference.md](reference/reference.md).
- HTTP contract or generated API surface: [api.md](reference/api.md).
- Public symbol intent, constraints, and failure channels: [code-documentation.md](reference/code-documentation.md).
- Supported versions, platforms, runtimes, or features: [support-matrix.md](reference/support-matrix.md).

How-to (action + work):

- One repeatable task for a competent reader: [how-to.md](task/how-to.md).
- Operational symptom-to-fix procedure: [runbook.md](task/runbook.md).
- Contributor workflow and pull-request evidence: [contributing.md](task/contributing.md).

Tutorial (action + learning):

- Tested learning path: [tutorial.md](learning/tutorial.md).
- Role ramp for contributors, maintainers, or operators: [onboarding.md](learning/onboarding.md).

If no quadrant fits cleanly, reduce the scope until one reader outcome is primary.

## Shared standards

Every document obeys five shared owners; each rule has exactly one of them:

- [agentic-documentation.md](agentic-documentation.md): position and agent cognition — salience and ordering within a unit, artifact separation, provider behavior, instruction files, indexes, retrieval, metadata, and catalogs.
- [information-structure.md](information-structure.md): form — container selection, tables, structured records, lists, diagrams, code blocks, page anatomy, line wrapping, and chunks.
- [style-guide.md](style-guide.md): craft — sentences, terminology, punctuation, links, examples, code-safe Markdown, and accessibility.
- [proof.md](proof.md): evidence — strength, freshness, source conflicts, verification, agent-surface evaluation, and preservation under refactor.
- [formatting.md](formatting.md): notation — status and invocation markers, table styling, whitespace, and the heading idiom.

## Placement

Place documentation where the reader or tool first looks:

- Repository root for corpus-wide entry documents.
- Owner-local directory for package, product, tool, or subsystem truth.
- `docs/` for shared cross-owner material and generated documentation entrypoints.
- `docs/standards/` for authoring standards only.
- Source files for public symbol documentation and rationale that names and types cannot express.

Prefer one owner for a claim, and link across owners instead of copying the same claim into multiple pages.

## Split and link

When a draft serves more than one primary reader need, split it:

- Move background from tutorials and how-to guides into explanation.
- Move catalogs, option tables, and command inventories into reference or API documentation.
- Move step-by-step work from reference leaves into how-to guides.
- Move operational response from how-to guides into runbooks.
- Move durable decision rationale from architecture into ADRs.
- Move implementation sequence and exit criteria into roadmaps or design docs.
- Move contribution workflow from README files into contributing guides.

After splitting, add the smallest useful cross-link and do not leave a summary copy that can drift.

## Lifecycle

Maintain documentation like code:

- Create the smallest useful document that answers the reader need.
- Update docs in the same change that alters the source truth they describe.
- Delete or replace dead documentation when it is known to be wrong.
- Add a review trigger for a drift-prone claim, and prefer an explicit stale marker over silent uncertainty when a claim cannot be verified during maintenance.

Unless live product support and evidence justify them, do not preserve old paths, terminology, commands, or product claims as compatibility notes.

## Folder layout

- Root files are the five shared standards, this index, and `AGENTS.md`, the agent instruction file for `docs/standards/**` (instruction-file rules owned by [agentic-documentation.md](agentic-documentation.md)).
- `explanation/` holds decision, design, architecture, roadmap, and test-strategy standards.
- `reference/` holds entry, lookup, API, code-documentation, and support-matrix standards.
- `task/` holds normal-task, operational-recovery, and contribution standards.
- `learning/` holds tutorial and onboarding standards.
- `_TMP/` holds inactive source material outside the active standard set.

## Anti-patterns

- README files that carry design history, tutorials, or API catalogs.
- Architecture documents that carry task plans or incident response.
- ADRs that include execution procedures instead of decision confirmation.
- Tutorials that branch into production variants.
- Reference leaves that hide procedures in dense tables.
- Runbooks without an observable trigger, owner, verification, or escalation.
- Generated API documentation forked by hand-written endpoint or symbol tables.
- Standards that mention authoring interactions, obsolete workflow names, or temporary task labels instead of durable document behavior.

## Maintenance rules

- Keep this README a route map; put detailed rules in the owning standard.
- Prefer restructuring, deletion, and owner links over duplicated guidance.
- Restructure without dropping load-bearing facts; diff content coverage before replacing a document, per the preservation rule in proof.
- Use a table only when comparison or lookup value beats a list.
- Remove a stale standard instead of keeping a compatibility alias.
- Keep release history in the project's release mechanism, not in this index.

## Review checklist

- [ ] Active standards are linked by current filename.
- [ ] Each shared standard has one clear owner role.
- [ ] Each type standard appears in exactly one family.
- [ ] The reader-need map and type chooser route to a single primary type.
- [ ] `_TMP/` is described as inactive.
- [ ] This index routes readers instead of restating standard bodies.
