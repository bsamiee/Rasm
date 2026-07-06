# [STRATA]

Draw which layer may depend on which. Use `flowchart TB` with 4-5 stratum subgraphs stacked top to bottom, edges permitted only downward, and one forbidden upward edge styled Dracula Red and labeled prohibited. The red `linkStyle` on the offending edge is the law made visible: dependency flows down, never up.

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
    linkStyle 3 stroke:#FF5555,stroke-width:2px,color:#FF5555
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class Kernel boundary
```
