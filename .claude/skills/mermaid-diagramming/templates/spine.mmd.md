# [SPINE]

Draw the main path a runtime walks once: boot, compose, resolve, a readiness gate, run, drain, stop. The spine captures three decisions an unassisted attempt misses — the gate is the only branch point, since a spine with two gates is two spines; every stage before the gate can fault and every fault converges on one rail; the rail rejoins drain, so cleanup is unconditional rather than a happy-path privilege. Use `flowchart LR` with 8-12 nodes on one dominant rail; terminals are stadium nodes classed `boundary`, the fault rail is classed `error`, and a cycle anywhere is a defect — a runtime that loops back is a lifecycle, not a spine.

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
    edgeLabelBackground: "#282A36"
---
flowchart LR
    accTitle: Owner-set spine
    accDescr: The once-walked main path from boot through compose, readiness gate, run, and drain, with every stage fault converging on one rail that rejoins drain.
    Boot([Boot]) --> Registry[Registry]
    Registry --> Composer[Composer]
    Composer --> Resolver[Resolver]
    Resolver --> Ready{Ready?}
    Ready -->|composed| Run[Run]
    Ready -->|missing dep| Fault[/Fault rail/]
    Composer -.->|compose fault| Fault
    Run --> Drain[Drain]
    Fault -->|unconditional cleanup| Drain
    Drain --> Stop([Stop])
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef error fill:#FF5555,stroke:#FF5555,color:#282A36
    class Boot,Stop boundary
    class Fault error
```

Refill by renaming stages to the real owner set, keep the single gate, and route every stage that can fail onto the one rail with a dotted edge — solid edges carry the walked path, dotted edges carry the fault hops.
