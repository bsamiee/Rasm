# [STRATA]

Draw which layer may depend on which. The template bakes in the full dependency law, not just the stack — downward edges are legal including skips, so one dashed skip edge shows transitive reach is permitted; exactly one upward edge exists, styled Dracula Red and labeled forbidden, making the violated law visible instead of implicit; and each stratum is a subgraph so membership, not position, carries the layer fact. Use `flowchart TB` with 4-5 stratum subgraphs, solid adjacent-layer edges, at most one dashed legal skip, and the one red forbidden edge. The 6-stratum ceiling binds at review — the validator's flowchart family ceiling cannot see strata. A runtime walk order is a spine, never a stratum stack.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
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
    edgeLabelBackground: "#282A36"
    titleColor: "#F8F8F2"
---
flowchart TB
    accTitle: Stratum dependency law
    accDescr: Four stacked strata with downward-only dependency edges, one dashed legal skip edge, and one forbidden upward edge styled red and marked prohibited.
    subgraph L4[App]
        App[App]
    end
    subgraph L3[Host boundary]
        Host[Host]
    end
    subgraph L2[Platform]
        Platform[Platform]
    end
    subgraph L1[Kernel]
        Kernel[Kernel]
    end
    App --> Host
    Host --> Platform
    Platform --> Kernel
    App -.->|legal skip| Platform
    Kernel -->|"forbidden: upward dep"| App
    linkStyle 4 stroke:#FF5555,stroke-width:2px,color:#FF5555
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class Kernel boundary
```

Refill by renaming strata to the real layer roster, keep edges downward with at most one demonstrative skip, and keep the single forbidden edge red — its `linkStyle` index is the edge's declaration position, so recount after any edge insertion.
