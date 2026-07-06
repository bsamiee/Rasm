---
name: mermaid-diagramming
description: >-
  Generates and validates Mermaid diagrams with YAML frontmatter, ELK layout, Dracula theme
  tokens, and a bundled render-plus-graph-logic validator. Owns diagram methodology — when to
  diagram, node and edge selection, per-type construction, logical soundness — the standard
  archetype catalog (architecture spine, package seam graph, logic flow, state lifecycle, wire
  sequence, persistence schema, dependency strata), and the admitted type registry: flowchart,
  sequence, state, class, ER, gantt, mindmap, timeline, kanban, gitGraph, requirement, C4, and
  architecture. Use when authoring, editing, or fixing any mermaid fence, choosing a diagram
  type, or whenever a task asks to draw, diagram, map, or visualize a system, flow, state
  machine, sequence exchange, database schema, dependency structure, schedule, hierarchy, or
  workflow board — even when mermaid is never named — distinct from quantitative dataviz marks
  (charts of data) and interactive HTML pages.
---

# [MERMAID_DIAGRAMMING]

Every committed diagram answers one written question, instantiates one catalog archetype or admitted type, opens with frontmatter carrying its type's Dracula subset, and ships only after the validator and the soundness audit pass. The reasoning discipline and the engine surface load on demand through the reference routes below.

## [01]-[QUESTION]

A diagram earns its fence only when the reader traces a relation across more marks than a clause holds: three or more nodes with at least one branch, cycle, or crossing relation. Below that threshold prose owns the fact, and one diagram owns one question — a diagram needing two legends is two diagrams. The full discipline — investigation traces per archetype, staged growth with the node-annotation-omission ladder, node and edge law, type selection, soundness, multi-diagram composition — is [references/methodology.md](references/methodology.md); what each type's marks must mean, its signal, and its master patterns is [references/construction.md](references/construction.md).

## [02]-[CATALOG]

Select the archetype by intent, copy its template, and refill — a catalog template is self-sufficient, carrying its archetype's construction law in its own prose. An intent outside the catalog selects its type through the methodology decision table and the extended registry, under the same frontmatter, theming, and validation law. The budget column is the legibility ceiling that binds at review; past it the split move fragments the subject into two diagrams, and a required legend is itself a split signal.

| [INDEX] | [ARCHETYPE]   | [INTENT]                    | [DECLARATION]     | [BUDGET]       | [SPLIT_MOVE]                     | [TEMPLATE]                                              |
| :-----: | :------------ | :-------------------------- | :---------------- | :------------- | :------------------------------- | :------------------------------------------------------ |
|  [01]   | spine         | main path through owners    | `flowchart LR`    | 12 nodes       | split at the readiness gate      | [spine](templates/spine.mmd.md)                         |
|  [02]   | seam-graph    | shapes across a boundary    | `flowchart LR`    | 12 edges       | partition by counterpart package | [seam-graph](templates/seam-graph.mmd.md)               |
|  [03]   | logic-flow    | one operation dispatch      | `flowchart LR`    | 12 nodes       | extract an arm subflow           | [logic-flow](templates/logic-flow.mmd.md)               |
|  [04]   | lifecycle     | guarded state transitions   | `stateDiagram-v2` | 12 states      | nest a composite state           | [lifecycle](templates/lifecycle.mmd.md)                 |
|  [05]   | wire-sequence | ordered boundary exchange   | `sequenceDiagram` | 6 participants | split by interaction phase       | [wire-sequence](templates/wire-sequence.mmd.md)         |
|  [06]   | schema        | persistent entity relations | `erDiagram`       | 8 entities     | split by aggregate root          | [schema](templates/schema.mmd.md)                       |
|  [07]   | strata        | layer dependency direction  | `flowchart TB`    | 6 strata       | collapse peer layers             | [strata](templates/strata.mmd.md)                       |

## [03]-[VALIDATE]

A diagram is not done until its fence passes both stages: graph-logic checks over the source, then a render whose proof is an actual SVG artifact, never a zero exit alone.

```bash
uv run scripts/validate_mermaid.py <file.md ...>
```

Each fence emits `file:line: STATUS check detail` rows with check kinds `render`, `frontmatter`, `contract`, `logic`, `setup`, `read`, and `collect`; `--json` emits identical-key NDJSON for tooling. Contract, logic, and frontmatter rows fire only on findings — a clean fence prints its render row alone, and silence from a check is a pass. Graph-logic analysis covers flowchart, state, sequence, ER, class, gantt, requirement, architecture, and C4; any other family emits a `logic-unimplemented` warn instead of silent approval. Logic failures block: orphan node, unreachable state, undefined or unknown class targets, dangling task, group, service, or relation references. Logic warns demand a split or a stated reason: duplicate same-label edges, orphan participants, entities, services, and requirements, budget overruns. Contract warnings cover accessibility presence and order, `theme: base`, the mono-stack floors, `clusterBkg`, label backing, canonical classes, linkStyle index drift, semantic edge rails, sequence grouping, and palette drift. `--no-render` runs the logic and frontmatter checks alone for a fast loop; the process exits nonzero when any fence fails. A render failure splits `syntax` from `environment`, so a missing browser never masquerades as a broken diagram.

The renderer resolves as `--renderer CMD`, then a `pnpm exec mmdc` workspace, then `mmdc` on PATH, then `npx -y @mermaid-js/mermaid-cli`. The final branch reaches the network on every run; install the renderer once so validation stays offline:

```bash
pnpm add -D @mermaid-js/mermaid-cli
```

## [04]-[CONTRACT]

- Frontmatter opens every fence body before the diagram header, carrying `theme: base` with the Dracula variable subset the diagram type consumes; the token system, role map, dual-host law, and the types and hosts that carry a local style law instead — packet, C4 element surfaces, host-themed docs — are [references/theming.md](references/theming.md).
- `accTitle` and `accDescr` follow the header on every committed diagram, stating the relation the diagram encodes so the exported SVG stays usable outside its source; `block`, `mindmap`, `sankey`, and `venn` refuse the directives, so there the relation sentence sits beside the fence.
- Node and edge labels carry concept names, never mechanism detail — the owning page carries the bytes.
- Semantic node classes and edge rails come from the canonical Dracula `classDef` set; an ad-hoc hex is a defect.
- The ruled mono stack `fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"` and the Selection `#44475A` label backing reach every themed fence; per-element sizes ride the theming micro-scale `themeCSS` stamps, and no canvas text renders below 12px.

## [05]-[LEGIBILITY]

Legibility bounds a diagram, not syntax capacity: the validator warns past each family ceiling, and the catalog's archetype budget binds at review. A rendered diagram ships only after it passes every legibility check:

- Labels render untruncated.
- Edges do not visually dominate nodes.
- Orientation matches reading order.
- Node groups stay visually separable.
- Contrast survives both light and dark hosts.
- The diagram type matches the subject.

A faulted fence converges on its own source across at most five render-inspect-edit rounds; each correction is a minimal text edit to the fence itself, never a sibling file.

## [06]-[REFERENCES]

- When to diagram, investigation, node and edge law, type selection, soundness audit, multi-diagram composition: [references/methodology.md](references/methodology.md)
- Per-type construction — the question each type answers, what its marks assert, failure modes, truth tests: [references/construction.md](references/construction.md)
- Dracula palette, role map, the base theme block, canonical classDef rails, dual-host contrast, Alucard: [references/theming.md](references/theming.md)
- The full styling grammar — every link and arrow form for every family, link lengths, the complete shape registry, containers and subgraphs, style precedence with its interaction traps, the per-type styling matrix: [references/styling.md](references/styling.md)
- Frontmatter schema, secure keys, layout engines and ELK tuning, look system, accessibility, mmdc and CI, trap list: [references/config.md](references/config.md)
- Advanced flowchart, sequence, state, class, and ER — node metadata, edge IDs, markdown strings, KaTeX, per-type traps: [references/syntax-core.md](references/syntax-core.md)
- The type registry beyond the core five — admitted rows with working fences and traps, registered rows named: [references/syntax-extended.md](references/syntax-extended.md)

## [07]-[GOTCHAS]

- `%%{init:...}%%` is deprecated — a fence opening with it is a defect; convert to frontmatter.
- Reserved words `end`, `default`, `subgraph`, `class` break node IDs — quote the label, rename the ID.
- `layout: elk` outside the flowchart family is a dead key — the type ignores or rejects it.
- Edge labels containing `[`, `]`, or `:` need double quotes: `A -->|"[WIRE]: shape"| B`.
- Beta types shift syntax between minors — validate after any version bump.

## [08]-[REPO_INTEGRATION]

When the host repo declares a docs gate, that gate consumes `scripts/validate_mermaid.py --json`; the bundled script is the same engine everywhere else.
