# [WIRE_SEQUENCE]

Draw an ordered exchange across a wire or process boundary. Use `sequenceDiagram` with 3-4 participants, `autonumber`, one `alt` block splitting success from fault, and one `Note over` the wire naming the frame shape. `sequenceDiagram` supports no ELK; `look: neo` applies at 11.14.0+.

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    actorBkg: "#44475A"
    actorBorder: "#BD93F9"
    actorTextColor: "#F8F8F2"
    actorLineColor: "#6272A4"
    signalColor: "#FF79C6"
    signalTextColor: "#F8F8F2"
    noteBkgColor: "#44475A"
    noteTextColor: "#F8F8F2"
    noteBorderColor: "#6272A4"
    activationBkgColor: "#6272A4"
    activationBorderColor: "#BD93F9"
    loopTextColor: "#F8F8F2"
    labelBoxBkgColor: "#21222C"
    labelBoxBorderColor: "#6272A4"
    labelTextColor: "#F8F8F2"
---
sequenceDiagram
    accTitle: Wire exchange
    accDescr: Ordered request and response across a process boundary with a success versus fault alternative and the frame shape noted over the wire.
    autonumber
    participant C as Composer
    participant W as Wire
    participant R as Resolver
    Note over C,W: Frame { key, payload }
    C->>W: Request(Frame)
    W->>R: Dispatch(Frame)
    alt resolved
        R-->>W: Receipt
        W-->>C: Receipt
    else fault
        R-->>W: FaultRow
        W-->>C: FaultRow
    end
```
