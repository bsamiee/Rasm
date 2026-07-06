# [STRATA]

Draw which layer may depend on which. Use `flowchart TB` with 4-5 stratum subgraphs stacked top to bottom, edges permitted only downward, and one forbidden upward edge styled red and labeled prohibited. The red `linkStyle` on the offending edge is the law made visible: dependency flows down, never up.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
---
flowchart TB
    accTitle: Stratum dependency law
    accDescr: Four stacked strata with downward-only dependency edges and one forbidden upward edge styled red and marked prohibited.
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
    Kernel -->|forbidden: upward dep| App
    linkStyle 3 stroke:#c0392b,stroke-width:2px,color:#e06455
    classDef base fill:#2f5d8a,stroke:#7fb0d8,color:#ffffff
    class Kernel base
```
