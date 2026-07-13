# [TS_DATA_ARCHITECTURE]

`data` owns the branch's durable-persistence surface: four sub-domains — `lane`, `journal`, `object`, `read` — meet through the one journal write owner, the one capability rail, the one content identity, and the one tenancy contract. A backend is a semantic-guarantee row on its owning lane, never a sibling shape; sub-domains align with the core, security, runtime, and iac peers by contract, never by reference.

## [01]-[DOMAIN_MAP]

```text codemap
data/
└── src/
    ├── lane/             # Guarantee-lane matrix: engines as rows under sealed capability vocabularies
    │   ├── postgres.ts   # First-party relational lane and its ruled extension matrix
    │   ├── sqlite.ts     # Embedded lane degrading one relational contract across its profile rows
    │   ├── olap.ts       # Analytical lane over DuckDB and ClickHouse engine rows
    │   ├── cache.ts      # Latency lane: single-flight, dedup, restart-surviving cache rows
    │   ├── capability.ts # Fail-closed capability rail probed at Layer construction
    │   └── tenant.ts     # Tenancy write path pinning the TENANT_GUC across RLS, schema, and database cases
    ├── journal/          # Record of truth: atomic writes, evolution, facts, lawful aging
    │   ├── append.ts     # One atomic write owner: journal, outbox, and idempotency ledger in one commit
    │   ├── evolve.ts     # Read-time upcasting: per-tag version chains, snapshot as a projection
    │   ├── fact.ts       # Durable fact journal: audit and metering as one buffered family
    │   └── retain.ts     # Retention classes, crypto-shredding, and DSAR portability folds
    ├── object/           # Content-addressed object plane over the one ContentKey
    │   ├── store.ts      # S3-conditional content-addressed object store
    │   ├── stream.ts     # Resumable rail: BYOB ingress, checkpointed identity fold, tus server
    │   ├── file.ts       # Filesystem plane: gated content-addressed intake and derivative codec
    │   └── remote.ts     # Remote-origin plane: scheme-dispatched non-local sources
    └── read/             # Read side: typed queries, batching, projections, reactivity, retrieval
        ├── query.ts      # Typed CRUD with arity as combinator over Model codec pairs
        ├── batch.ts      # Request-batching engine: structural dedup and windowed resolvers
        ├── fold.ts       # Durable projection plane binding one Fold.Plan across staleness budgets
        ├── live.ts       # Reactivity-keyed reads: invalidation keys stamped at publish, read at query
        └── search.ts     # Five-lane retrieval fused by reciprocal rank inside the database
```

## [02]-[SEAMS]

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Data package seam registry
    accDescr: Data sub-domain owners exchanging content keys, tenancy, custody, and reactive shapes with the core, security, runtime, and IaC packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph data[DATA]
        Fold[Projection fold]
        Store[Object store]
        Tenant[Tenancy write path]
        Retain[Retention]
        Append[Journal write owner]
        Live[Reactive reads]
        Stream[Resumable stream]
        Search[Retrieval fusion]
        Postgres[Pg lane]
    end
    Core{{core}}
    Security{{security}}
    Runtime{{runtime}}
    Iac([iac])
    Core e1@-->|"[SHAPE]: Fold.Plan"| Fold
    Core e2@<-->|"[CONTENT_KEY]: ContentKey"| Store
    Core e3@-->|"[SHAPE]: TenantContext"| Tenant
    Tenant e4@-->|"[PORT]: SessionStore"| Security
    Security e5@-->|"[BOUNDARY]: TenantScope"| Tenant
    Security e6@-->|"[SHAPE]: Shredder envelope"| Retain
    Append e7@<-->|"[BOUNDARY]: Outbox claim"| Runtime
    Live e8@-->|"[SHAPE]: Reactivity feed"| Runtime
    Stream e9@-->|"[BOUNDARY]: Tus dispatcher"| Runtime
    Runtime e10@-->|"[PORT]: Embedder"| Search
    Postgres e11@-->|"[SHAPE]: Pg.rows"| Iac
    Tenant e12@-->|"[SHAPE]: Tenancy locus"| Iac
    Tenant e13@-->|"[BOUNDARY]: Tenancy.rls"| Iac
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Fold,Store,Tenant,Retain,Append,Live,Stream,Search,Postgres primary
    class Core,Security,Runtime external
    class Iac annotation
    class e2 edgeData
    class e1,e3,e4,e5,e6,e7,e8,e9,e10,e11,e12,e13 edgeControl
```

## [03]-[ORGANIZATION]

`lane` prices guarantees, never durability tiers: `postgres` is the spine, the embedded, analytical, and latency lanes sit beside it, `capability` refuses to boot an engine that cannot prove its rows, and `tenant` is the single write path pinning the tenancy GUC. `journal` is the record of truth — `append` commits journal, outbox, and idempotency together, and read-time upcasting keeps the log append-only. `object` binds every byte plane to the one content identity through a single admission fold. `read` composes the lanes into consumption, from proven-shape CRUD to reciprocal-rank fusion.

## [04]-[BOUNDARIES]

- DDL is declarative additive ensure: iac applies at provision, this folder verifies at startup through the capability rail, and runtime never mutates schema. Sole carve-out is the operator rebuild verb — `read/fold`'s session-locked shadow swap — never scheduled, never reachable from a request path.
- Key custody stays out: this folder makes no authorization decision, enforces the security-declared tenancy contract, and stores only wrapped key material.
- Engine names never leak upward: consumers bind guarantee lanes, and a new engine is a row on its owning lane page.
- Object-plane conformance refuses any engine that cannot honor `If-None-Match: *` conditional put, and refused rows are recorded so the argument is never re-had.
