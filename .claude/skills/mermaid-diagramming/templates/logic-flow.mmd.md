# [LOGIC_FLOW]

Draw one operation's branch and dispatch structure: input discrimination fanning to dispatch arms, arms folding into a merge, and the merge yielding a receipt. Use `flowchart LR` with 8-12 nodes, one rhombus discriminator, three-or-more parallel arms collapsing to a single fold, and exactly one dotted evidence edge feeding the fold. Variation lives in the arms, never in parallel exits.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
---
flowchart LR
    accTitle: Dispatch and fold
    accDescr: One operation discriminating input shape into three dispatch arms that fold into a merge, yielding a receipt, with a dotted evidence trace.
    In([Input]) --> Shape{Shape?}
    Shape -->|single| ArmA[Resolve one]
    Shape -->|batch| ArmB[Resolve batch]
    Shape -->|stream| ArmC[Resolve stream]
    ArmA --> Fold[Fold]
    ArmB --> Fold
    ArmC --> Fold
    Fold --> Receipt[/Receipt/]
    Receipt --> Out([Out])
    Evidence[(Evidence)] -.->|trace| Fold
    classDef arm fill:#44506b,stroke:#8b9bc4,color:#ffffff
    classDef sink fill:#2f7d5b,stroke:#5cc79a,color:#ffffff
    class ArmA,ArmB,ArmC arm
    class Receipt,Evidence sink
```
