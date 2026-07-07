# [STRATA]

Draw which layer may depend on which. The template bakes in the full dependency law, not just the stack — downward edges are legal including skips, so one dashed skip edge shows transitive reach is permitted on the Comment rail; exactly one upward edge exists, styled Dracula Red and labeled forbidden, making the violated law visible instead of implicit; and each stratum is a subgraph so membership, not position, carries the layer fact, with each stratum node filled from the ordinal accents so altitude reads as color. Use `flowchart TB` with 4-5 stratum subgraphs, solid adjacent-layer edges, at most one dashed legal skip, and the one red forbidden edge. The 6-stratum ceiling binds at review — the validator's flowchart family ceiling cannot see strata. A runtime walk order is a spine, never a stratum stack.

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 22
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
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:12.5px;font-weight:600;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path,.marker circle{transform:scale(.8);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart TB
    accTitle: Stratum dependency law
    accDescr: Four stacked strata with downward-only dependency edges, one dashed legal skip edge, and one forbidden upward edge styled red and marked prohibited.
    subgraph L4[APP]
        App[App]
    end
    subgraph L3[HOST BOUNDARY]
        Host[Host]
    end
    subgraph L2[PLATFORM]
        Platform[Platform]
    end
    subgraph L1[KERNEL]
        Kernel[Kernel]
    end
    App --> Host
    Host --> Platform
    Platform --> Kernel
    App -.->|legal skip| Platform
    Kernel -->|"forbidden: upward dep"| App
    linkStyle 3 stroke:#6272A4,color:#F8F8F2,stroke-width:1.5px,stroke-dasharray:4 6
    linkStyle 4 stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    classDef stratumApp fill:#BD93F980,stroke:#BD93F9,color:#F8F8F2
    classDef stratumHost fill:#8BE9FD99,stroke:#8BE9FD,color:#282A36
    classDef stratumPlatform fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class App stratumApp
    class Host stratumHost
    class Platform stratumPlatform
    class Kernel boundary
```

Refill by renaming strata to the real layer roster, keep edges downward with at most one demonstrative skip on the Comment rail, keep the single forbidden edge red, and keep one distinct ordinal fill per stratum — the ordinal `stratum*` classes are this archetype's stated exception to the canonical-class floor, so the validator's class warns are the accepted receipt — every `linkStyle` index is the edge's declaration position, so recount after any edge insertion. The frontmatter micro-scale `themeCSS` stamp, the ruled mono stack, and the `#21222C` edge-label backing are fixed law — a refill renames content, never strips the fidelity surface.
