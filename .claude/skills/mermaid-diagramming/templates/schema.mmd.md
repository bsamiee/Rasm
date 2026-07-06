# [SCHEMA]

Draw persistent entities and their relations. The template bakes in the schema discipline an unassisted attempt violates — every relationship edge has its FK attribute on the owning side and every FK has its edge, so the diagram and the storage constraint cannot disagree; cardinality states what storage enforces, never intended usage; and a many-to-many resolves through a visible junction entity carrying both FKs, because the crow's foot cannot express it directly. Use `erDiagram` with 4-7 entities around one aggregate root, typed attributes with `PK`/`FK` markers, and verb-labeled relations; the aggregate root is classed `primary`, the junction `recessed`, and an externally-owned registry `external` in its ER stroke-encoded form — dark fill, Cyan stroke and title — because a bright fill floods the attribute rows; the hierarchy the prose claims is the hierarchy the render shows. `erDiagram` takes no ELK, and this template holds the classic look — keep `theme: base` with its variable block. In-memory type relations are a class diagram, never a persistence schema.

```mermaid
---
config:
  theme: base
  themeCSS: ".nodeLabel{font-size:12px}.name .nodeLabel{font-size:14px;font-weight:600}.edgeLabel .nodeLabel{font-size:12.5px;font-weight:500}.relationshipLine{stroke-width:1.5px}"
  themeVariables:
    darkMode: true
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    nodeBorder: "#BD93F9"
    edgeLabelBackground: "#44475A"
    attributeBackgroundColorOdd: "#282A36"
    attributeBackgroundColorEven: "#21222C"
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
---
erDiagram
    accTitle: Persistent entity schema
    accDescr: Typed entities around one aggregate root, every relationship backed by its foreign key and a junction entity resolving the many-to-many.
    OWNER ||--o{ RUN : issues
    RUN ||--|{ RECEIPT : yields
    RUN }o--|| REGISTRY : binds
    RECEIPT ||--o{ FAULT : records
    RUN ||--o{ RUN_ARTIFACT : consumes
    ARTIFACT ||--o{ RUN_ARTIFACT : feeds
    OWNER {
        uuid id PK
        string name
    }
    RUN {
        uuid id PK
        uuid owner_id FK
        uuid registry_id FK
        string state
    }
    RECEIPT {
        uuid id PK
        uuid run_id FK
        int code
    }
    REGISTRY {
        uuid id PK
        string version
    }
    FAULT {
        uuid id PK
        uuid receipt_id FK
        string reason
    }
    ARTIFACT {
        uuid id PK
        string content_key UK
    }
    RUN_ARTIFACT {
        uuid run_id PK, FK
        uuid artifact_id PK, FK
    }
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef external fill:#21222C,stroke:#8BE9FD,color:#8BE9FD
    class RUN primary
    class RUN_ARTIFACT recessed
    class REGISTRY external
```

Refill by renaming entities to the real aggregate, keep FK-edge reciprocity on every relation, resolve any many-to-many through a junction entity whose composite key is both FKs, and keep the root/junction/external classes on the entities that carry those roles. The frontmatter micro-scale `themeCSS` stamp, the ruled mono stack, and the `#44475A` label backing are fixed law — a refill renames entities, never strips the fidelity surface.
