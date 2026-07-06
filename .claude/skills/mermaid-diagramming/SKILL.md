---
name: mermaid-diagramming
description: >-
  Generates and validates Mermaid v11 diagrams with YAML frontmatter, ELK layout, Dracula theme
  tokens, and a bundled render-plus-graph-logic validator. Owns diagram methodology — when to
  diagram, node and edge selection, per-type construction, logical soundness — the standard
  archetype catalog (architecture spine, package seam graph, logic flow, state lifecycle, wire
  sequence, persistence schema, dependency strata), and the full v11 type roster from flowchart,
  sequence, state, class, and ER through gantt, mindmap, timeline, C4, architecture, kanban, and
  the chart family. Use when authoring or editing a mermaid fence, choosing a diagram type, or
  drawing any structural or relational diagram — nodes, states, sequences, entities — distinct
  from quantitative dataviz marks or an interactive HTML page.
---

# [MERMAID_DIAGRAMMING]

Every committed diagram answers one written question, instantiates one catalog archetype or admitted type, opens with frontmatter carrying the Dracula theme block, and ships only after the validator and the soundness audit pass. The reasoning discipline and the engine surface load on demand through the reference routes below.

## [01]-[QUESTION]

A diagram earns its fence only when the reader traces a relation across more marks than a clause holds: three or more nodes with at least one branch, cycle, or crossing relation. Below that threshold prose owns the fact, and one diagram owns one question — a diagram needing two legends is two diagrams. The full discipline — investigation, node and edge law, type selection, soundness, multi-diagram composition — is [references/methodology.md](references/methodology.md); what each type's marks must mean and how each type is built is [references/construction.md](references/construction.md).

## [02]-[CATALOG]

Select the archetype by intent, copy its template, and refill. An intent outside the catalog selects its type through the methodology decision table and the extended roster, under the same frontmatter, theming, and validation law.

| [INDEX] | [ARCHETYPE]   | [INTENT]                    | [DECLARATION]     |
| :-----: | :------------ | :-------------------------- | :---------------- |
|  [01]   | spine         | main path through owners    | `flowchart LR`    |
|  [02]   | seam-graph    | shapes across a boundary    | `flowchart LR`    |
|  [03]   | logic-flow    | one operation dispatch      | `flowchart LR`    |
|  [04]   | lifecycle     | guarded state transitions   | `stateDiagram-v2` |
|  [05]   | wire-sequence | ordered boundary exchange   | `sequenceDiagram` |
|  [06]   | schema        | persistent entity relations | `erDiagram`       |
|  [07]   | strata        | layer dependency direction  | `flowchart TB`    |

- spine: [templates/spine.mmd.md](templates/spine.mmd.md)
- seam-graph: [templates/seam-graph.mmd.md](templates/seam-graph.mmd.md)
- logic-flow: [templates/logic-flow.mmd.md](templates/logic-flow.mmd.md)
- lifecycle: [templates/lifecycle.mmd.md](templates/lifecycle.mmd.md)
- wire-sequence: [templates/wire-sequence.mmd.md](templates/wire-sequence.mmd.md)
- schema: [templates/schema.mmd.md](templates/schema.mmd.md)
- strata: [templates/strata.mmd.md](templates/strata.mmd.md)

## [03]-[VALIDATE]

A diagram is not done until its fence passes both stages: graph-logic checks over the source, then a render whose proof is an actual SVG artifact, never a zero exit alone.

```bash
python scripts/validate_mermaid.py <file.md ...>
```

Each fence emits `file:line: ok|FAIL <reason>|WARN <reason>` rows with check kinds `render`, `frontmatter`, `logic`, `setup`, and `read`; `--json` emits NDJSON for tooling. Logic failures block: orphan node, unreachable state, undefined class, unknown class target. Logic warns demand a split or a stated reason: duplicate edge, budget overrun. `--no-render` runs the logic and frontmatter checks alone for a fast loop; the process exits nonzero when any fence fails. A render failure splits `syntax` from `environment`, so a missing browser never masquerades as a broken diagram.

The renderer resolves as `--renderer CMD`, then a `pnpm exec mmdc` workspace, then `mmdc` on PATH, then `npx -y @mermaid-js/mermaid-cli`. The final branch reaches the network on every run; install the renderer once so validation stays offline:

```bash
pnpm add -D @mermaid-js/mermaid-cli
```

## [04]-[CONTRACT]

- Frontmatter opens every fence body before the diagram header, carrying `theme: base` with the Dracula variable subset the diagram type consumes; the token system, role map, and dual-host law are [references/theming.md](references/theming.md).
- `accTitle` and `accDescr` follow the header on every committed diagram, stating the relation the diagram encodes so the exported SVG stays usable outside its source.
- Node and edge labels carry concept names, never mechanism detail — the owning page carries the bytes.
- Semantic node classes and edge rails come from the canonical Dracula `classDef` set; an ad-hoc hex is a defect.

## [05]-[LEGIBILITY]

Legibility bounds a diagram, not syntax capacity: past its budget an archetype fragments into two diagrams by the split move, and a required legend is itself a split signal. The validator warns past each family ceiling; the archetype budget binds at review.

| [INDEX] | [ARCHETYPE]   | [BUDGET]       | [SPLIT_MOVE]                     |
| :-----: | :------------ | :------------- | :------------------------------- |
|  [01]   | spine         | 12 nodes       | split at the readiness gate      |
|  [02]   | seam-graph    | 12 edges       | partition by counterpart package |
|  [03]   | logic-flow    | 12 nodes       | extract an arm subflow           |
|  [04]   | lifecycle     | 12 states      | nest a composite state           |
|  [05]   | wire-sequence | 6 participants | split by interaction phase       |
|  [06]   | schema        | 8 entities     | split by aggregate root          |
|  [07]   | strata        | 6 strata       | collapse peer layers             |

A rendered diagram ships only after it passes every legibility check:

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
- Dracula palette, role map, base theme block, classDef and linkStyle rails, dual-host contrast, Alucard: [references/theming.md](references/theming.md)
- Frontmatter schema, secure keys, layout engines and ELK tuning, look system, accessibility, mmdc and CI, trap list: [references/config.md](references/config.md)
- Advanced flowchart, sequence, state, class, and ER — shape registry, edge IDs, markdown strings, KaTeX, version gates: [references/syntax-core.md](references/syntax-core.md)
- The full type roster beyond the core five, each with its minimal fence, version gate, and traps: [references/syntax-extended.md](references/syntax-extended.md)

## [07]-[GOTCHAS]

- `%%{init:...}%%` is deprecated — a fence opening with it is a defect; convert to frontmatter.
- Reserved words `end`, `default`, `subgraph`, `class` break node IDs — quote the label, rename the ID.
- `layout: elk` outside the flowchart family is a dead key — the type ignores or rejects it.
- Edge labels containing `[`, `]`, or `:` need double quotes: `A -->|"[WIRE]: shape"| B`.
- Beta types shift syntax between minors — validate after any version bump.

## [08]-[REPO_INTEGRATION]

When the host repo declares a docs gate, that gate consumes `scripts/validate_mermaid.py --json`; the bundled script is the same engine everywhere else.
