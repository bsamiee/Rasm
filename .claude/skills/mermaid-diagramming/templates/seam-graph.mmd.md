# [SEAM_GRAPH]

Draw who exchanges what shape across a package boundary. The template bakes in what a seam actually is — every edge is one contracted shape crossing the boundary, labeled `"[KIND]: shape-name"` from the closed KIND vocabulary `[WIRE] [SHAPE] [PORT] [BOUNDARY] [RECEIPT] [CONTENT_KEY] [GRADUATION] [TESSELLATION] [FAULT] [PROJECTION] [TRANSPORT]`, so an unkinded edge is an unowned contract; a bidirectional edge exists only where a real inverse contract exists, never as label-dodging; and the counterpart mirrors the same edge verbatim in its own seam graph, so the label text is shared law, not local prose. The KIND also selects the edge rail — data-bearing kinds (`[WIRE] [CONTENT_KEY] [GRADUATION]`) ride Orange, `[RECEIPT]` rides Green, `[FAULT]` rides Red, boundary-crossing delivery (`[TRANSPORT] [PROJECTION]`) rides Cyan, and control kinds keep the Pink default — so the label's law and the stroke's law agree. Use `flowchart LR` with one `subgraph` for the home package holding its sub-domain owners, counterpart packages outside it, 8-12 edges, and `elk.mergeEdges: true` once edges cross. Node `classDef` encodes seam direction — bidirectional counterparts classed `external` against one-way sinks classed `annotation`. Layer permission questions are strata, never a seam registry.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  elk:
    mergeEdges: true
  flowchart:
    padding: 16
  themeCSS: ".nodeLabel{font-size:14px;font-weight:500}.edgeLabel{font-size:12.5px;font-weight:500}.cluster-label .nodeLabel{font-size:13px;font-weight:600}.cluster rect{stroke-width:1.5px}.edgePaths path{stroke-width:1.5px}"
  themeVariables:
    darkMode: true
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#6272A4"
    edgeLabelBackground: "#44475A"
    titleColor: "#F8F8F2"
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
---
flowchart LR
    accTitle: Package seam registry
    accDescr: Home package sub-domains exchanging kinded shapes with external counterparts, edge rails colored by kind and nodes classed by seam direction.
    subgraph core[CORE PACKAGE]
        Resolver[Resolver]
        Registry[Registry]
        Composer[Composer]
    end
    DataStore[(data)]
    UiShell([ui shell])
    Transport{{transport}}
    Registry -->|"[SHAPE]: Descriptor"| Composer
    Composer -->|"[PORT]: ResolvePort"| Resolver
    Resolver <-->|"[WIRE]: RowBatch"| DataStore
    DataStore -->|"[CONTENT_KEY]: Hash128"| Registry
    DataStore -->|"[GRADUATION]: Snapshot"| Composer
    Composer <-->|"[BOUNDARY]: PortMap"| Transport
    Transport -->|"[TRANSPORT]: Frame"| UiShell
    Resolver -->|"[PROJECTION]: ViewModel"| UiShell
    Resolver -->|"[RECEIPT]: RunReceipt"| UiShell
    Resolver -->|"[FAULT]: FaultRow"| UiShell
    linkStyle 2,3,4 stroke:#FFB86C,color:#F8F8F2
    linkStyle 6,7 stroke:#8BE9FD,color:#F8F8F2
    linkStyle 8 stroke:#50FA7B,color:#F8F8F2
    linkStyle 9 stroke:#FF5555,stroke-width:2px,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FD,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    class Resolver,Registry,Composer primary
    class DataStore,Transport external
    class UiShell annotation
```

Refill by renaming owners and counterparts to the real packages, keep every label `[KIND]: shape-name` with the shape's exact wire name, keep each edge on its kind's rail — `linkStyle` indices are declaration positions, recounted after any edge insertion; a seam registry that grows under edits moves its rails to edge-id classes (`Resolver f1@-->|"[FAULT]: FaultRow"| UiShell` with `class f1 edgeError`), which survive insertions without recounts — and land the mirrored edge in the counterpart's graph in the same change. The frontmatter micro-scale `themeCSS` stamp, the ruled mono stack, and the `#44475A` edge-label backing are fixed law — a refill renames content, never strips the fidelity surface.
