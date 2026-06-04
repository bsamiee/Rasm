---
description: Standard for architecture documentation
---

# Architecture standards

Architecture documents explain current structure, boundaries, invariants, and
quality trade-offs. They are explanation documents, not installation guides,
task plans, incident procedures, or API catalogs.

## Use when

Use an architecture document when readers need to understand:

- system, package, owner, host, or runtime boundaries;
- building blocks and their relationships;
- invariants, constraints, quality trade-offs, and rejected shapes;
- current topology and the proof that keeps it accurate.

Do not use architecture docs for durable decision rationale, implementation
sequence, operational response, or generated API reference. Link ADRs, roadmaps,
runbooks, and API docs instead.

## External basis

Use external architecture standards for semantics:

- arc42 owns architecture explanation categories.
- C4 owns software architecture abstractions, diagram levels, and relationship
  semantics.
- Structurizr DSL owns authored model consistency when current repository
  tooling or an existing architecture corpus uses it.
- Mermaid owns Markdown-friendly rendering syntax only.

Do not redefine these standards locally. A smaller local profile must preserve
the external standard's meaning.

## Profiles

- Landscape architecture: system or tool-level document at `ARCHITECTURE.md`.
- Owner-contract architecture: package or sub-concern document at
  `_ARCHITECTURE.md`.
- README-only architecture: allowed only for small scopes with obvious
  structure.

Use one architecture file per scope. Do not keep both `ARCHITECTURE.md` and
`_ARCHITECTURE.md` in the same directory.

## Landscape structure

Use an arc42-lite structure:

1. Scope, goals, and constraints.
2. Context and scope.
3. Solution strategy.
4. Building block view and codemap.
5. Runtime scenarios.
6. Deployment view.
7. Cross-cutting concepts and quality invariants.
8. Decisions, risks, and glossary.

Required sections are scope, context, solution strategy, building blocks,
invariants, and risks. Runtime and deployment sections are required when the
scope has non-obvious flows, host behavior, or deployed topology.

## Owner-contract structure

Use the compact profile for package or sub-concern boundaries:

1. Purpose and boundary.
2. Build or support status.
3. Public contracts and owned building blocks.
4. Invariants and rejected shapes.
5. Proof and evidence.
6. ADR and roadmap links.

Owner-contract files are a compact arc42 subset. They still need scope,
contracts, invariants, and proof.

## C4 and diagrams

C4 owns diagram semantics. Rendering syntax does not.

- Landscape architecture requires C4 Context and Container views unless the
  scope is explicitly README-only.
- Component views are allowed only when stable component boundaries matter.
- Dynamic and Deployment views are allowed when runtime sequence or deployed
  topology explains a current invariant.
- Code-level diagrams are rare; use them only when they add value beyond the
  codemap.
- Keep each top-level view to roughly 3-7 meaningful elements.
- Caption each diagram with C4 view type, authored source, renderer, and
  repository source of truth.

Use a checked-in architecture model when repository tooling, manifests, or the
existing architecture corpus configure one. When no model tool is configured,
authored diagram source is acceptable if the caption names the source, renderer,
repository source of truth, and verification evidence. Introduce Structurizr
only when current repository tooling or the architecture owner makes it the
modeling surface.

Treat generated SVG, PNG, PlantUML, Mermaid, and static-site outputs as
generated artifacts.

Use Mermaid only as a renderer or local sketch format:

- a single lightweight inline `flowchart` that follows C4 terminology;
- generated Mermaid exported from a checked-in architecture model.

Do not use Mermaid syntax as the architecture model.

## Codemap

The codemap must come from the repository, not from intent. Include real paths
at two or three levels, explain each owner boundary, and omit leaf files unless
the leaf is a public contract or central algorithm.

## Proof

Architecture proof must name how the document was refreshed:

- source paths or manifests used to derive the codemap;
- authored diagram model path, when one exists;
- renderer or export command, when a diagram is generated;
- ADR, roadmap, reference, or support links that carry related truth.

Do not claim a diagram reflects the system unless model elements and codemap
were checked against current repository paths.

## Boundaries

- README owns entry and adoption links.
- Architecture owns current structure and invariants.
- ADR owns why a durable choice was made.
- Roadmap owns phase sequence and exit criteria.
- Design documents own pre-implementation proposals.
- Runbooks own operational recovery.

## Review checklist

- [ ] Profile is named.
- [ ] Scope, goals, and constraints are explicit.
- [ ] Required views exist for the selected profile.
- [ ] Authored diagram source, renderer, source of truth, and verification
      evidence are named.
- [ ] Mermaid is used only as renderer syntax or a lightweight sketch.
- [ ] Codemap matches current repository paths.
- [ ] ADRs, risks, and roadmap links are honest.
- [ ] Proof states how diagrams and codemap were verified.
