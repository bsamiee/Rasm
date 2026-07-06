# [SCHEMA]

Draw persistent entities and their relations. Use `erDiagram` with 4-5 entities, typed attributes carrying `PK`/`FK` markers, and relationship cardinalities with verb labels. `erDiagram` supports no ELK and no `look` — keep `theme: base` with its variable block.

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
