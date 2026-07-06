# [SCHEMA]

Draw persistent entities and their relations. Use `erDiagram` with 4-5 entities, typed attributes carrying `PK`/`FK` markers, and relationship cardinalities with verb labels. `erDiagram` supports neither ELK nor `look` — keep only `theme: base`.

```mermaid
---
config:
  theme: base
---
erDiagram
    accTitle: Persistent entity schema
    accDescr: Typed entities linked by primary and foreign keys, each relation carrying a crow's-foot cardinality.
    OWNER ||--o{ RUN : issues
    RUN ||--|{ RECEIPT : yields
    RUN }o--|| REGISTRY : binds
    RECEIPT ||--o{ FAULT : records
    OWNER {
        uuid id PK
        string name
    }
    RUN {
        uuid id PK
        uuid owner_id FK
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
```
