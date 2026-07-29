# [TS_DATA_ARCHITECTURE]

`data` owns the branch's durable-persistence surface: the `lane`, `journal`, `object`, and `read` sub-domains meet through the one journal write owner, the one capability rail, the one content identity, and the one tenancy contract. Backends land as semantic-guarantee rows on their owning lane, never sibling shapes; sub-domains align with the core, security, runtime, and iac peers by contract, never by reference.

## [01]-[DOMAIN_MAP]

```text codemap
data/
└── src/
    ├── lane/             # Guarantee-lane matrix: engines as rows under sealed capability vocabularies
    │   ├── postgres.ts   # First-party relational lane and its ruled extension matrix
    │   ├── sqlite.ts     # Embedded lane degrading one relational contract across its profile rows
    │   ├── olap.ts       # Analytical lane over DuckDB, ClickHouse, Flight, residence rows, and the Arrow-Parquet wire
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
    │   ├── file.ts       # Filesystem plane: gated content-addressed intake and the derivative spine
    │   ├── asset.ts      # category-general asset plane: category-gated admission, transform rows, container + ktx rows
    │   └── remote.ts     # Remote-origin plane: scheme-dispatched non-local sources
    └── read/             # Read side: typed queries, batching, projections, reactivity, retrieval
        ├── query.ts      # Typed CRUD with arity as combinator over Model codec pairs
        ├── batch.ts      # Request-batching engine: structural dedup and windowed resolvers
        ├── fold.ts       # Durable projection plane binding one Fold.Plan across staleness budgets
        ├── live.ts       # Reactivity-keyed reads: invalidation keys stamped at publish, read at query
        └── search.ts     # Retrieval lanes fused by reciprocal rank inside the database
```

## [02]-[STRATA]

- S0 floor — independent mints, none importing a data sibling; `capability` is the fail-closed rail fed by argument, never import.
- S1 `lane/tenant` — pins the tenancy write path over `Capability` and `Pg`.
- S1 `lane/sqlite` — degrades the `Pg` contract through the grant-key type read, harvesting query evidence into `Pg.Profile` — its one value read.
- S2 `journal` — `append` commits journal, outbox, and idempotency in one transaction; `retain` ages and `fact` meters inside the stratum.
- S2 `append` mints the CloudEvents relay envelope and owns the core-brand `Hook` vocabulary; `retain` fans its erase tombstone through it.
- S3 `object` — every byte plane binds `Journal` custody under the one content identity; `store` roots, `stream` and `file` tap `Hook` at admission.
- S4 `read` — consumption over everything below; `lane/olap` sits beside it composing `ObjectStore` and the `Pg.Profile` harvest band.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Data interior import strata
    accDescr: Five interior strata onto the mint floor; imports downward, remote alone reaching the cache lane, olap and sqlite reading the Pg band.
    subgraph S4["S4 READ"]
        Olap[olap]
        Read["query · batch · search · fold"]
    end
    subgraph S3["S3 OBJECT"]
        Object["store · stream · file · asset"]
        Remote[remote]
    end
    subgraph S2["S2 JOURNAL"]
        Journal["append · retain · fact"]
    end
    subgraph S1["S1 TENANT + SQLITE"]
        Tenant[tenant]
        Sqlite[sqlite]
    end
    subgraph S0["S0 FLOOR"]
        Postgres[postgres]
        Capability[capability]
        Evolve[evolve]
        Live[live]
        Cache[cache]
    end
    Tenant e1@-->|"[IMPORT]: Capability"| Capability
    Tenant e2@-->|"[IMPORT]: Pg"| Postgres
    Journal e3@-->|"[IMPORT]: Tenancy"| Tenant
    Journal e4@-->|"[IMPORT]: Upcast"| Evolve
    Journal e5@-->|"[IMPORT]: Live"| Live
    Journal e6@--> Capability
    Object e7@-->|"[IMPORT]: Journal"| Journal
    Remote e8@-->|"[IMPORT]: CacheLane"| Cache
    Object e9@--> Capability
    Read e10@-->|"[IMPORT]: ObjectStore"| Object
    Read e11@-->|"[IMPORT]: Journal"| Journal
    Read e12@-->|"[IMPORT]: Live"| Live
    Read e13@--> Capability
    Read e14@-->|"[IMPORT]: Snapshot"| Evolve
    Olap e15@-->|"[IMPORT]: ObjectStore"| Object
    Olap e17@-->|"[IMPORT]: Pg"| Postgres
    Sqlite e16@-->|"[IMPORT]: Pg"| Postgres
    Object e18@-->|"[IMPORT]: Hook"| Journal
    S0 f1@-->|"forbidden: upward import"| S4
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Data package seam registry
    accDescr: Data owners exchanging content keys, tenancy, custody, reactive shapes, and the analytics-residence door with the core, security, runtime, and iac peers.
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
        Capability[Capability admission]
        Fact[Fact journal]
        Batch[Request batching]
        Cache[Cache lane]
        Olap[Analytical lane]
        Asset[Asset pipeline]
    end
    Core{{core}}
    Security{{security}}
    Runtime{{runtime}}
    Iac{{iac}}
    Persistence[(Rasm.Persistence)]
    Core e1@-->|"[SHAPE]: Fold.Plan"| Fold
    Core e2@-->|"[CONTENT_KEY]: ContentKey"| Store
    Core e3@-->|"[SHAPE]: TenantContext"| Tenant
    Tenant e4@-->|"[PORT]: SessionStore"| Security
    Security e5@-->|"[BOUNDARY]: TenantScope"| Tenant
    Security e6@-->|"[SHAPE]: SealedEnvelope"| Retain
    Append e7@<-->|"[BOUNDARY]: Journal.claimBatch"| Runtime
    Live e8@-->|"[SHAPE]: Live.changes"| Runtime
    Stream e9@-->|"[BOUNDARY]: Rail"| Runtime
    Runtime e10@-->|"[PORT]: Embedder"| Search
    Postgres e11@-->|"[SHAPE]: Pg.rows"| Iac
    Tenant e12@-->|"[BOUNDARY]: Tenancy.rls"| Iac
    Core e13@-->|"[SHAPE]: Convention"| Append
    Core e14@-->|"[SHAPE]: Convention"| Fold
    Append e15@-->|"[PORT]: Journal.census"| Runtime
    Tenant e16@-->|"[PORT]: ClaimStore"| Security
    Core e17@-->|"[SHAPE]: Convention"| Fact
    Core e18@-->|"[SHAPE]: Convention"| Batch
    Core e19@-->|"[SHAPE]: Convention"| Store
    Core e20@-->|"[SHAPE]: Convention"| Stream
    Core e21@-->|"[SHAPE]: Convention"| Cache
    Core e22@-->|"[SHAPE]: Convention"| Olap
    Append e23@-->|"[SHAPE]: Journal.envelope"| Runtime
    Core e24@-->|"[SHAPE]: Tap.Point"| Append
    Append e25@-->|"[SHAPE]: Tap.Registry"| Runtime
    Capability e26@-->|"[PROJECTION]: Backend.Projection"| Iac
    Capability e27@-->|"[SHAPE]: Backend.Generation"| Runtime
    Persistence e28@<-->|"[CONTRACT]: BackendContract"| Capability
    Append e29@-->|"[PORT]: AuditJournal"| Security
    Core e30@-->|"[SHAPE]: Query.Residence"| Olap
    Core e32@-->|"[SHAPE]: Hops"| Olap
    Olap e31@-->|"[SHAPE]: Query.Target"| Core
    Core e33@-->|"[PROJECTION]: DashboardModel.Signal"| Olap
    Iac e34@-->|"[PORT]: analytics residence"| Olap
    Core e35@-->|"[SHAPE]: Convention"| Asset
```

## [04]-[INTERNAL]

`lane` prices guarantees, never durability tiers: `postgres` is the spine, the embedded, analytical, and latency lanes sit beside it, `capability` refuses to boot an engine that cannot prove its rows, and `tenant` is the single write path pinning the tenancy GUC. `journal` is the record of truth — `append` commits journal, outbox, and idempotency together so a replay returns the stored receipt, and read-time upcasting keeps the log append-only. `object` binds every byte plane to the one content identity through a single admission fold.

`read` composes the lanes into consumption, from proven-shape CRUD to reciprocal-rank fusion. One pool and one code path serve a fleet-scale consumer with tenancy carried as a scope value; an artifact hashed in any runtime is reusable by every other; and `retain` makes erasure cryptographically total — destroying the sole wrapped key folds every sealed read to a redaction marker.

## [05]-[BOUNDARIES]

- Generated framework artifacts apply through IaC convergence; this folder verifies realized state and never mutates schema at runtime.
- Generated backend projections drive admission and provider adapters without transferring contract authority into this package.
- Operator rebuild materializes, verifies, and publishes a fresh target outside every request path.
- Key custody stays out: no authorization decision here, the security-declared tenancy contract enforced, only wrapped key material stored.
- Engine names never leak upward: consumers bind guarantee lanes, and a new engine is a row on its owning lane page.
- Object-plane conformance refuses any engine that cannot honor `If-None-Match: *` conditional put; refused rows are recorded once.
