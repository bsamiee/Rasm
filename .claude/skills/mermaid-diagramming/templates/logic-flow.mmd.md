# [LOGIC_FLOW]

Draw one operation's dispatch structure: input discrimination fanning to arms, arms folding into one merge, the merge yielding a receipt. The template bakes in the polymorphic-collapse law — variation lives in the arms and never in parallel exits, so every arm folds back to the single receipt path — plus two supports an unassisted attempt omits: the discriminator reads its arms from a policy store, making dispatch table-driven rather than hardcoded branching, and evidence feeds the fold on a dotted trace so the receipt is auditable. Use `flowchart LR` with 8-12 nodes, one rhombus discriminator whose out-labels are exhaustive and disjoint, and three-or-more arms; the receipt is classed `success`, stores are classed `data`.

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
    accDescr: One operation discriminating input shape into three policy-selected dispatch arms that fold into a merge yielding one receipt, with a dotted evidence trace feeding the fold.
    In([Input]) --> Shape{Shape?}
    Policy[(Policy rows)] -.->|selects arms| Shape
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
    class Policy,Evidence data
```

Refill law: the arm labels are the input-shape vocabulary — rename them to the real discriminants and keep them exhaustive; a new capability is a new arm row, never a second exit after the fold.
