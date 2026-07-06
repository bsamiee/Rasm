# [SEAM_GRAPH]

Draw who exchanges what shape across a package boundary. Use `flowchart LR` with one `subgraph` for the home package holding its sub-domain owners, counterpart packages outside it, and 8-12 edges each labeled `"[KIND]: shape-name"`. The KIND vocabulary is closed: `[WIRE] [SHAPE] [PORT] [BOUNDARY] [RECEIPT] [CONTENT_KEY] [GRADUATION] [TESSELLATION] [FAULT] [PROJECTION] [TRANSPORT]`. Node `classDef` encodes seam direction — bidirectional counterparts against one-way sinks.

```mermaid
---
config:
  layout: elk
  look: neo
  theme: base
---
flowchart LR
    accTitle: Package seam registry
    accDescr: Home package sub-domains exchanging kinded shapes with external counterparts, classed by bidirectional versus one-way seam direction.
    subgraph core[core package]
        Resolver[Resolver]
        Registry[Registry]
        Composer[Composer]
    end
    DataStore[(data)]
    UiShell[ui]
    Transport{{transport}}
    Registry -->|"[SHAPE]: Descriptor"| Composer
    Composer -->|"[PORT]: ResolvePort"| Resolver
    Resolver <-->|"[WIRE]: RowBatch"| DataStore
    DataStore -->|"[CONTENT_KEY]: Hash128"| Registry
    DataStore -->|"[GRADUATION]: Snapshot"| Composer
    Composer <-->|"[BOUNDARY]: PortMap"| Transport
    Transport -->|"[TRANSPORT]: Frame"| UiShell
    Resolver -->|"[PROJECTION]: ViewModel"| UiShell
    Resolver -->|"[RECEIPT]: RunReceipt"| UiShell
    Resolver -->|"[FAULT]: FaultRow"| UiShell
    classDef home fill:#44506b,stroke:#8b9bc4,color:#ffffff
    classDef biseam fill:#2f7d5b,stroke:#5cc79a,color:#ffffff
    classDef oneway fill:#8a5a2b,stroke:#d19a5c,color:#ffffff
    class Resolver,Registry,Composer home
    class DataStore,Transport biseam
    class UiShell oneway
```
