---
description: Index for documentation standards
---

# Documentation standards

This folder is the active standards library for Rasm documentation. Start here
to choose a document type, then apply the shared rules and the matching type
standard.

## Use when

Use this index when:

- creating or rewriting Markdown documentation;
- deciding which standard owns a rule;
- routing a draft to the right document type;
- checking whether a standard is active.

Do not use inactive material in `_TMP/` unless the user explicitly asks to
integrate it.

## Owner precedence

Use the strongest applicable owner:

1. Current repository source, manifests, generated contracts, and runnable tool
   output.
2. The document-type standard in this folder.
3. Shared standards for style, information structure, proof, and agent-facing
   surfaces.
4. External standards cited by the active local standard.

When a claim can drift, prove it with repository truth before web examples or
prior notes.

## Read order

Read order is workflow order. Owner precedence still decides conflicts.

1. Choose the reader intent with
   [documentation-system.md](documentation-system.md).
2. Apply the shared standards:
   [style-guide.md](style-guide.md),
   [information-structure.md](information-structure.md),
   [proof.md](proof.md), and
   [agentic-documentation.md](agentic-documentation.md).
3. Apply exactly one type standard. If a draft satisfies two reader intents,
   split it and link between the files.

## Shared standards

- [documentation-system.md](documentation-system.md): document type, placement,
  lifecycle, split/link rules, and boundaries.
- [style-guide.md](style-guide.md): prose, terminology, notation, punctuation,
  links, examples, code-safe Markdown, and accessibility.
- [information-structure.md](information-structure.md): page anatomy, headings,
  lists, tables, diagrams, labeled blocks, examples, and chunks.
- [proof.md](proof.md): evidence hierarchy, source conflicts, freshness,
  verification claims, and proof gaps.
- [agentic-documentation.md](agentic-documentation.md): instruction files,
  machine-facing indexes, generated mirrors, retrieval, metadata, MCP catalogs,
  state artifacts, and structured outputs.

## Choose by need

- Entry point or local hub: [readme.md](reference/readme.md).
- Current structure, boundaries, and invariants:
  [architecture.md](explanation/architecture.md).
- Durable architectural decision: [adr.md](explanation/adr.md).
- Pre-implementation proposal or RFC-style review:
  [design-doc.md](explanation/design-doc.md).
- Build sequence, milestones, and exit proof:
  [roadmap.md](explanation/roadmap.md).
- Test levels, gates, evidence, or flake policy:
  [test-strategy.md](explanation/test-strategy.md).
- Tested learning path: [tutorial.md](learning/tutorial.md).
- Role ramp for contributors, maintainers, or operators:
  [onboarding.md](learning/onboarding.md).
- Curated lookup truth: [reference.md](reference/reference.md).
- HTTP contract or generated API surface: [api.md](reference/api.md).
- Public symbol intent, constraints, and failure channels:
  [code-documentation.md](reference/code-documentation.md).
- Supported versions, platforms, runtimes, or features:
  [support-matrix.md](reference/support-matrix.md).
- One repeatable task for a competent reader: [how-to.md](task/how-to.md).
- Operational symptom-to-fix procedure: [runbook.md](task/runbook.md).
- Contributor workflow: [contributing.md](task/contributing.md).

## Folder layout

- Root files are shared standards and this index.
- `explanation/` holds decision, design, architecture, roadmap, and test
  strategy standards.
- `reference/` holds entry, lookup, API, code documentation, and support matrix
  standards.
- `task/` holds normal task, operational recovery, and contribution workflow
  standards.
- `learning/` holds tutorial and onboarding standards.
- `_TMP/` holds inactive source material that is not part of the active
  standard set.

## Maintenance rules

- Keep this README as a route map. Put detailed rules in the owning shared or
  type-specific standard.
- Prefer restructuring, deletion, and owner links over duplicated guidance.
- Use tables only when comparison or lookup value is stronger than a list.
- Remove stale standards instead of retaining compatibility aliases.
- Keep release history in the release mechanism used by the project, not in
  this index.

## Review checklist

- [ ] Active standards are linked by current filename.
- [ ] Each shared standard has one clear owner role.
- [ ] Each type standard appears in exactly one family.
- [ ] `_TMP/` is described as inactive.
- [ ] The index routes readers instead of restating standard bodies.
