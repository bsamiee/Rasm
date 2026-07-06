# [SPINE]

Draw the main path through an owner set: the boot to compose to ready to run to drain shape a runtime walks once. Use `flowchart LR` with 8-12 nodes on a single dominant rail and exactly one branch off a readiness gate onto the fault rail. Terminals are stadium nodes classed `boundary`; the fault rail is classed `error` and rejoins the drain so cleanup is unconditional.

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
    accDescr: The main path through an owner set from boot through compose, ready, run, and drain, with one fault-rail branch that rejoins drain.
    Boot([Boot]) --> Registry[Registry]
    Registry --> Composer[Composer]
    Composer --> Resolver[Resolver]
    Resolver --> Ready{Ready?}
    Ready -->|composed| Run[Run]
    Ready -->|missing dep| Fault[/Fault rail/]
    Run --> Drain[Drain]
    Fault --> Drain
    Drain --> Stop([Stop])
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef error fill:#FF5555,stroke:#FF5555,color:#282A36
    class Boot,Stop boundary
    class Fault error
```
