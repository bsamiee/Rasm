# [<architecture-title-token>]

<domain-map-lead-2-3-sentences: the unit's charter in owning voice — what it owns, the one invariant band it lowers onto, and its boundary to the peers it aligns with by contract, never by reference.>

## [01]-[DOMAIN_MAP]

The codemap is the unit's file index as a fenced representation: one node per eventual source file, named in the language's folder and file casing. Each comment carries the concept the file owns in one aligned tail — the `#` column aligns within a block, the whole line stays under 150 columns, and no method chain, type roster, or design-decision detail rides a tree comment. A comment that cannot fit aligned under the cap is trimmed to its load-bearing concept, never spaced around.

```text codemap
core/
├── resolver.py       # mints content keys; owns the resolve dispatch
├── registry.py       # holds the descriptor registry and admission law
└── shape/            # the shape sub-domain owners
    ├── fold.py       # folds shape ops through one entry
    └── codec.py      # decodes shape wire bytes at the seam
```

## [02]-[SEAMS]

The seam map is cross-boundary by construction and partner-package grain: home sub-domain owners in the `subgraph`, one node per counterpart package, one edge per contract labeled `[KIND]: shape-name`, rails colored by kind. An in-package relation is never a seam — it lives in the codemap or the `[03]-[INTERNAL]` diagram. Construction, the closed KIND vocabulary, the kind-to-rail-class map, node shape-by-role, and the hub-split rule are the mermaid-diagramming seam-graph archetype's; a unit whose cross-boundary seams overflow one clean fence splits into two fences by counterpart group, each with its own `accTitle`.

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Core unit seam registry
    accDescr: Core sub-domain owners exchanging kinded shapes with a data store and a consuming shell, edge rails colored by kind and nodes classed by seam direction.
    subgraph core[CORE UNIT]
        Resolver[Resolver]
        Registry[Registry]
    end
    DataStore[(data store)]
    Shell([app shell])
    Registry e1@-->|"[SHAPE]: Descriptor"| Resolver
    Resolver e2@<-->|"[WIRE]: RowBatch"| DataStore
    Resolver e3@-->|"[RECEIPT]: ResolveReceipt"| Shell
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Resolver,Registry primary
    class DataStore data
    class Shell annotation
    class e1 edgeControl
    class e2 edgeData
    class e3 edgeSuccess
```

## [03]-[INTERNAL]

Optional, present only where internal composition or a boot, lifecycle, or dispatch flow carries signal a codemap cannot: the internal wiring the seam map excludes graduates here as a `flowchart LR` spine or `stateDiagram-v2` lifecycle under the mermaid-diagramming spine or lifecycle archetype, one diagram answering one question. A unit whose interior reads fully from the codemap omits this section.

Package-specific prose sections — organization, boundaries, prohibitions — follow as the sanctioned extension point, each owning one decision cluster its heading names.
