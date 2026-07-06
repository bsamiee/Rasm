# [SCHEMA]

Draw persistent entities and their relations. The template bakes in the schema discipline an unassisted attempt violates — every relationship edge has its FK attribute on the owning side and every FK has its edge, so the diagram and the storage constraint cannot disagree; cardinality states what storage enforces, never intended usage; and a many-to-many resolves through a visible junction entity carrying both FKs, because the crow's foot cannot express it directly. Use `erDiagram` with 4-7 entities around one aggregate root, typed attributes with `PK`/`FK` markers, and verb-labeled relations. `erDiagram` takes no ELK and no `look` — keep `theme: base` with its variable block.

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    nodeBorder: "#BD93F9"
    edgeLabelBackground: "#282A36"
    attributeBackgroundColorOdd: "#282A36"
    attributeBackgroundColorEven: "#21222C"
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
```

Refill law: rename entities to the real aggregate, keep FK-edge reciprocity on every relation, and resolve any many-to-many through a junction entity whose composite key is both FKs.
