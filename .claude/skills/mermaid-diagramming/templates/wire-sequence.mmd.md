# [WIRE_SEQUENCE]

Draw an ordered exchange across a wire or process boundary. Use `sequenceDiagram` with 3-4 participants, `autonumber`, one `alt` block splitting success from fault, and one `Note over` the wire naming the frame shape. `sequenceDiagram` supports neither ELK nor `look` — keep only `theme: base`.

```mermaid
---
config:
  theme: base
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
