# [SPINE]

Draw the main path through an owner set: the boot to compose to ready to run to drain shape a runtime walks once. Use `flowchart LR` with 8-12 nodes on a single dominant rail and exactly one branch off a readiness gate onto the fault rail. Terminals are stadium nodes; the fault rail rejoins the drain so cleanup is unconditional.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
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
    classDef entry fill:#3b6ea5,stroke:#7fb0d8,color:#ffffff
    classDef rail fill:#a65c3a,stroke:#d68a5c,color:#ffffff
    class Boot,Stop entry
    class Fault rail
```
