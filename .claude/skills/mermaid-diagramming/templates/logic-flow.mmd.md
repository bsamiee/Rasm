# [LOGIC_FLOW]

Draw one operation's dispatch structure: input discrimination fanning to arms, arms folding into one merge, the merge yielding a receipt. The template bakes in the polymorphic-collapse law — variation lives in the arms and never in parallel exits, so every arm folds back to the single receipt path — plus two supports an unassisted attempt omits: the discriminator reads its arms from a policy store, making dispatch table-driven rather than hardcoded branching, and evidence feeds the fold on a dotted trace so the receipt is auditable. Use `flowchart LR` with 8-12 nodes, one rhombus discriminator whose out-labels are exhaustive and disjoint, and three-or-more arms; the receipt is classed `success` with its delivery edges on the Green rail, stores are classed `data`, and every dotted trace rides the Comment rail — a trace annotates the fold, so its stroke leaves the Pink control default. Ordered steps across a boundary are a wire-sequence, never a dispatch fold.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
  flowchart:
    padding: 16
  themeCSS: ".nodeLabel{font-size:14px;font-weight:500}.edgeLabel{font-size:12.5px;font-weight:500}.edgePaths path{stroke-width:1.5px}"
  themeVariables:
    darkMode: true
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#44475A"
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
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
    linkStyle 1,10 stroke:#6272A4,color:#F8F8F2
    linkStyle 8,9 stroke:#50FA7B,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef success fill:#50FA7B,stroke:#50FA7B,color:#282A36
    classDef data fill:#FFB86C,stroke:#FFB86C,color:#282A36
    class ArmA,ArmB,ArmC primary
    class Receipt success
    class Policy,Evidence data
```

The arm labels are the input-shape vocabulary — rename them to the real discriminants and keep them exhaustive; a new capability is a new arm row, never a second exit after the fold. Trace edges keep the Comment rail: recount the `linkStyle` indices after any edge insertion. The frontmatter micro-scale `themeCSS` stamp, the ruled mono stack, and the `#44475A` edge-label backing are fixed law — a refill renames content, never strips the fidelity surface.
