# [SPINE]

Draw the main path a runtime walks once: boot, compose, resolve, a readiness gate, run, drain, stop. The spine captures three decisions an unassisted attempt misses — the gate is the only branch point, since a spine with two gates is two spines; every stage before the gate can fault and every fault converges on one rail; the rail rejoins drain, so cleanup is unconditional rather than a happy-path privilege. Use `flowchart LR` with 8-12 nodes on one dominant rail; terminals are stadium nodes classed `boundary`, stores take the cylinder form classed `data`, processes take the subroutine form classed `primary`, and the fault rail is classed `error` with every fault edge riding the Red `linkStyle` rail — the node and its edges state the same law. A cycle anywhere is a defect — a runtime that loops back is a lifecycle, not a spine.

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
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:12.5px;font-weight:600;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path,.marker circle{transform:scale(.8);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Owner-set spine
    accDescr: The once-walked main path from boot through compose, readiness gate, run, and drain, with every stage fault converging on one red rail that rejoins drain.
    Boot([Boot]) --> Registry[(Registry)]
    Registry --> Composer[[Composer]]
    Composer --> Resolver[[Resolver]]
    Resolver --> Ready{Ready?}
    Ready -->|composed| Run[Run]
    Ready -->|missing dep| Fault[/Fault rail/]
    Composer -.->|compose fault| Fault
    Run --> Drain[Drain]
    Fault -->|unconditional cleanup| Drain
    Drain --> Stop([Stop])
    linkStyle 5,6,8 stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef error fill:#FF555580,stroke:#FF5555,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    class Boot,Stop boundary
    class Fault error
    class Composer,Resolver,Run,Drain primary
    class Registry data
```

Refill by renaming stages to the real owner set, keep the single gate, and route every stage that can fail onto the one rail — the gate's fault exit and the unconditional cleanup rejoin stay solid because the runtime walks them, a mid-stage fault hop rides a dotted edge, and every fault edge stays on the Red rail: its `linkStyle` indices are declaration positions, so recount after any edge insertion. The frontmatter micro-scale `themeCSS` stamp, the ruled mono stack, and the `#21222C` edge-label backing are fixed law — a refill renames content, never strips the fidelity surface.
