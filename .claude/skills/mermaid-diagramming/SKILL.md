---
name: mermaid-diagramming
description: >-
  Generates and validates Mermaid v11 diagrams with YAML frontmatter, ELK layout, and theme
  tokens. Owns the standard diagram catalog — architecture spine, package seam graph, logic flow,
  state lifecycle, wire sequence, persistence schema, dependency strata — one template per
  archetype, per-archetype legibility budgets, and a bundled render validator that must pass
  before a diagram is done. Use when authoring or editing a mermaid fence, choosing a diagram
  type, or rendering a structural or relational diagram — nodes, states, sequences, entities —
  distinct from quantitative dataviz marks or an interactive HTML page.
---

# [MERMAID_DIAGRAMMING]

Each diagram instantiates one catalog archetype, opens with frontmatter and accessibility directives, and renders clean before the task that produced it closes.

## [01]-[WHEN_NOT]

A diagram carries structure a sentence cannot; below that threshold prose owns the fact and the fence is noise.

- Minimum-information threshold: three or more nodes with at least one branch, cycle, or crossing relation. A two-node single-edge graph is a sentence — write the sentence.
- Prose-owns-it test: a relation a reader retains from one clause routes to prose, never a fence. A diagram earns its slot only where the reader traces a path, a state transition, or a cardinality across more marks than a clause holds.
- One diagram owns one question; a diagram needing two legends is two diagrams.

## [02]-[CATALOG]

Select the archetype by intent, copy its template, and refill. An intent outside the catalog — timeline, hierarchy, infrastructure — selects its type from the syntax references under the same frontmatter and validation law.

| [INDEX] | [ARCHETYPE]   | [INTENT]                 | [DECLARATION]     |
| :-----: | :------------ | :----------------------- | :---------------- |
|  [01]   | spine         | main path through owners | `flowchart LR`    |
|  [02]   | seam-graph    | shapes across a boundary | `flowchart LR`    |
|  [03]   | logic-flow    | one operation dispatch   | `flowchart LR`    |
|  [04]   | lifecycle     | guarded state transitions| `stateDiagram-v2` |
|  [05]   | wire-sequence | ordered boundary exchange| `sequenceDiagram` |
|  [06]   | schema        | persistent entity relations | `erDiagram`    |
|  [07]   | strata        | layer dependency direction | `flowchart TB`  |

- spine: [templates/spine.mmd.md](templates/spine.mmd.md)
- seam-graph: [templates/seam-graph.mmd.md](templates/seam-graph.mmd.md)
- logic-flow: [templates/logic-flow.mmd.md](templates/logic-flow.mmd.md)
- lifecycle: [templates/lifecycle.mmd.md](templates/lifecycle.mmd.md)
- wire-sequence: [templates/wire-sequence.mmd.md](templates/wire-sequence.mmd.md)
- schema: [templates/schema.mmd.md](templates/schema.mmd.md)
- strata: [templates/strata.mmd.md](templates/strata.mmd.md)

## [03]-[CONTRACT]

- Frontmatter opens every fence body before the diagram header, carrying the layout, look, and theme keys the diagram type admits.
- `accTitle` and `accDescr` follow the header on every committed diagram, stating the relation the diagram encodes so the exported SVG stays usable outside its source.
- Node and edge labels carry concept names, never mechanism detail — the owning page carries the bytes.

## [04]-[LEGIBILITY]

Legibility bounds a diagram, not syntax capacity: past its budget an archetype fragments into two diagrams by the split move, and a required legend is itself a split signal.

| [INDEX] | [ARCHETYPE]   | [BUDGET]        | [SPLIT_MOVE]                 |
| :-----: | :------------ | :-------------- | :--------------------------- |
|  [01]   | spine         | 12 nodes        | split at the readiness gate  |
|  [02]   | seam-graph    | 12 edges        | partition by counterpart package |
|  [03]   | logic-flow    | 12 nodes        | extract an arm subflow       |
|  [04]   | lifecycle     | 12 states       | nest a composite state       |
|  [05]   | wire-sequence | 6 participants  | split by interaction phase   |
|  [06]   | schema        | 8 entities      | split by aggregate root      |
|  [07]   | strata        | 6 strata        | collapse peer layers         |

A rendered diagram ships only after it passes every legibility check:

- Labels render untruncated.
- Edges do not visually dominate nodes.
- Orientation matches reading order.
- Node groups stay visually separable.
- Contrast survives both light and dark hosts.
- The diagram type matches the subject.

A faulted fence converges on its own source across at most five render-inspect-edit rounds; each correction is a minimal text edit to the fence itself, never a sibling file.

## [05]-[VALIDATE]

A diagram is not done until its fence renders. Validation separates two failure classes: a syntax fault — the fence parses to nothing — is distinct from an environment fault, where no renderer resolves.

```bash
python scripts/validate_mermaid.py <file.md ...>
```

Each fence emits `file:line: ok|FAIL <stage>: <reason>|WARN no-frontmatter`, with `<stage>` splitting `syntax` from `environment`; `--json` emits NDJSON for tooling; the process exits nonzero when any fence fails or no renderer resolves. Render proof is an actual SVG artifact, not a zero exit alone.

The renderer resolves as `--renderer CMD`, then a `pnpm exec mmdc` workspace, then `mmdc` on PATH, then `npx -y @mermaid-js/mermaid-cli`. The final branch reaches the network on every run; install the renderer once so validation stays offline:

```bash
pnpm add -D @mermaid-js/mermaid-cli
```

## [06]-[REFERENCES]

- Frontmatter, layout, themes, classDef, accessibility: [references/config-theming.md](references/config-theming.md)
- Flowchart, sequence, state, class, ER syntax: [references/syntax-core.md](references/syntax-core.md)
- Charts, C4, timeline, gitGraph, kanban, beta types: [references/syntax-extended.md](references/syntax-extended.md)

## [07]-[GOTCHAS]

- `%%{init:...}%%` is deprecated — a fence opening with it is a defect; convert to frontmatter.
- Reserved words `end`, `default`, `subgraph`, `class` break node IDs — quote the label, rename the ID.
- ELK on non-flowchart types fails or is ignored — drop `layout:` there rather than ship a dead key.
- Beta types shift syntax between minors — validate after any version bump.
- Edge labels containing `[`, `]`, or `:` need double quotes: `A -->|"[WIRE]: shape"| B`.
- `classDef` declared above the nodes it styles renders unstyled — declare it at diagram root, after the nodes.

## [08]-[REPO_INTEGRATION]

When the host repo declares a docs gate, that gate consumes `scripts/validate_mermaid.py --json`; the bundled script is the same engine everywhere else.
