# [LOGIC_FLOW]

Draw one operation's branch and dispatch structure: input discrimination fanning to dispatch arms, arms folding into a merge, and the merge yielding a receipt. Use `flowchart LR` with 8-12 nodes, one rhombus discriminator, three-or-more parallel arms collapsing to a single fold, and exactly one dotted evidence edge feeding the fold. Variation lives in the arms, never in parallel exits; the receipt is classed `success`, the store `data`.

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
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef success fill:#50FA7B,stroke:#50FA7B,color:#282A36
    classDef data fill:#FFB86C,stroke:#FFB86C,color:#282A36
    class ArmA,ArmB,ArmC primary
    class Receipt success
    class Evidence data
```
