# [TS_DATA_ARCHITECTURE]

The domain map of `data` — the wave-2 durable-persistence package. Four sub-domains (`lane`, `journal`, `object`, `read`) meet through the one journal write owner, the one capability rail, the one content identity, and the one tenancy contract; a backend is a semantic-guarantee row, never a sibling shape.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
data/
└── src/
    ├── lane/             # The guarantee-lane matrix: engines as rows under sealed capability vocabularies
    │   ├── postgres.ts   # Pg — first-party capability rows, the concurrency-primitive table, the ruled extension matrix
    │   ├── sqlite.ts     # ONE sqlite lane, five profile rows (node/bun/wasm-OPFS/libsql/d1) with the per-profile degradation table
    │   ├── olap.ts       # The analytical lane: DuckDB node/wasm engine rows + the clickhouse row behind two scoped wraps
    │   ├── cache.ts      # The latency lane: Tier-0 keyed single-flight, request dedup, restart-surviving PersistedCache rows
    │   ├── capability.ts # The fail-closed capability rail: batched roster probes at Layer construction, one probe fault family
    │   └── tenant.ts     # Tenancy — RLS/schema/database cases, Tenancy.within TENANT_GUC write path, the security port-satisfaction rows
    ├── journal/          # The record of truth: atomic writes, evolution, facts, lawful aging
    │   ├── append.ts     # The ONE write owner: journal + outbox + idempotency ledger atomic, LEDGER_CLAIM/ATOMIC_PUBLISH/RELAY_ROWS
    │   ├── evolve.ts     # Read-time upcasting: total per-tag eventVersion chains + snapshot as a projection over Upcast.chain
    │   ├── fact.ts       # The durable fact journal: audit + meter rows of ONE fact family through ONE buffered rail
    │   └── retain.ts     # Retention classes, crypto-shredding via the security Shredder, DSAR portability folds
    ├── object/           # The content-addressed object plane over the one ContentKey
    │   ├── store.ts      # The S3-conditional object store: command union, engine conformance table, reference-CAS GC, presign grants
    │   ├── stream.ts     # The resumable rail: BYOB ingress, checkpointed identity fold, tus server over the S3 staging store
    │   └── file.ts       # The filesystem plane: gated content-addressed intake, watch admission, the sharp derivative codec
    └── read/             # The read side: typed queries, batching, projections, reactivity, retrieval
        ├── query.ts      # SqlSchema/SqlResolver typed CRUD — arity as combinator, Model codec pairs
        ├── batch.ts      # The request-batching engine: structural dedup, windowed resolvers, request caching
        ├── fold.ts       # The projection plane: one Fold.Plan-bound Lane at three staleness budgets (inline/async/rebuild)
        ├── live.ts       # Reactivity-keyed reactive reads: invalidation keys stamped at publish, consumed at query
        └── search.ts     # Five-lane retrieval (FTS, trigram, phonetic, fuzzy, semantic) fused by reciprocal-rank inside the database
```

## [02]-[SEAMS]

```text seams
read/fold      ←  typescript:core/state       # [SHAPE]: Fold.Plan (key/lift/merge) bound at three staleness budgets
object/store   ⇄  typescript:core/value       # [CONTENT_KEY]: ObjectKey IS ContentKey — a delegating mint site, never a second hash
lane/tenant    ←  typescript:security/access  # [BOUNDARY]: app.current_tenant RLS + ambient TenantScope read
lane/tenant    →  typescript:security/authn   # [PORT]: SessionStore/IdentityJournal/ClaimStore/RelationStore Layers
lane/tenant    →  typescript:security/authn   # [PORT]: RateLimiterStore Layer backing the credential-verify throttle budgets
journal/retain ←  typescript:security/crypt   # [SHAPE]: Shredder five-verb envelope, WrappedKey per-subject ledger
journal/append ⇄  typescript:runtime/work     # [BOUNDARY]: outbox claim-lease/urgency/park statements composed by the queue lane
read/live      →  typescript:runtime/serve    # [SHAPE]: reactivity-keyed feeds served under the resume-token law
object/stream  →  typescript:runtime/serve    # [BOUNDARY]: tus dispatchers mounted at the serving route
read/search    ←  typescript:runtime/ai       # [PORT]: Embedder/Reranker satisfied by the embedding plane
lane/postgres  →  typescript:iac/kube         # [SHAPE]: Pg.image/Pg.rows extension roster the CNPG image realizes
lane/tenant    →  typescript:iac/kube         # [BOUNDARY]: Tenancy.rls ensure roster applied by the in-cluster provision job
```

## [03]-[ORGANIZATION]

`lane` prices what each engine guarantees: `postgres` is the spine whose extension matrix is ruled data, `sqlite` degrades one relational contract across five profiles, `olap` and `cache` are guarantee tiers distinct from durability, `capability` refuses to boot against an engine that cannot prove its rows, and `tenant` is the single write path that pins the tenancy GUC. `journal` is the record of truth: `append` is the one atomic write owner (journal, outbox, idempotency in the same commit), `evolve` lifts old payloads at read time so the log is never rewritten, `fact` carries audit and metering as one family, and `retain` ages data lawfully without rewriting. `object` binds every byte plane to the core content identity — `store` conditional-puts against the conformance table, `stream` resumes at verified offsets, `file` gates filesystem intake through the same identity fold. `read` composes the lanes into consumption: `query` proves shapes, `batch` collapses N lookups structurally, `fold` binds the core fold algebra durably, `live` makes read-your-writes a coordinate, and `search` fuses five retrieval lanes in one round trip.

## [04]-[BOUNDARIES]

- DDL is declarative additive ensure with the split as law: iac applies at provision, this folder verifies at startup through the capability rail, the runtime never mutates schema.
- The folder holds no keys and makes no authorization decisions; it enforces the security-declared tenancy contract and stores only wrapped key material.
- Engine names never leak upward: consumers bind guarantee lanes; a new engine is a row on the owning lane page.
- The object plane's conformance table refuses engines that cannot honor `If-None-Match: *` conditional put; the refused rows are recorded so the argument is never re-had.
