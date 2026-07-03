# [STORE_ARCHITECTURE]

The domain map of `store` — the W3 event-sourced durable persistence plane of `libs/typescript`. Seven sub-domains own the no-migration store: `journal` the append-only record of truth and its ledger, `project` the two read lanes plus rebuild, `capability` the typed PG extension rows, `scope` the tenancy Layer family, `lane` the sqlite dialect lanes, `retrieve` the hybrid retrieval plane, `object` the content-addressed object plane. The folder imports `kernel`, `state`, `host`, and `security`; `security`, `telemetry`, and `work` reach it only through port Tags the app root satisfies, `iac` consumes its capability vocabulary, and the host-free fold algebra stays in `state` — `store/project` binds that algebra to durability.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
store/src/           # imports kernel, state, host, security (W3); runtime-spanning — per-runtime subpaths ./server, ./browser, ./wasm; drivers banned on the neutral vocabulary subpaths
├── journal/         # The append-only record of truth
│   ├── append.ts    # the ONE append surface: streams (appKey, tenantId, aggregate); OCC by expected version; events are closed Schema.TaggedClass families with app-authored eventVersion
│   ├── outbox.ts    # transactional outbox atomic with the append + idempotency ledger (ON CONFLICT DO UPDATE RETURNING (xmax = 0))
│   ├── snapshot.ts  # snapshot store keyed snapshot_schema_version
│   ├── upcast.ts    # read-time eventVersion upcaster folds — total functions, proven in proof/law; the raw log is never rewritten
│   └── retain.ts    # retention policy rows + crypto-shredding via the security/sign Shredder; per-subject erasure = key destruction, the log never rewritten; the per-subject DSAR export fold (portability read over journal + object rows) rides beside erasure
├── project/         # The read side: two lanes plus rebuild
│   ├── inline.ts    # same-transaction read-your-writes lanes (binds state folds to durability)
│   ├── async.ts     # checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED lanes
│   └── rebuild.ts   # pg_cron rebuild/compaction + pg_ivm in-database IVM rows
├── capability/      # Typed PG extension capability
│   ├── row.ts       # closed {extension, floor, probeSql, capabilities, layer} vocabulary; fail-closed probes
│   └── matrix.ts    # the PG 18.4 extension matrix rows (pgvector/VectorChord, pg_search, timescaledb, pg_partman, pgmq, pg_cron, pg_ivm, pg_duckdb, pg_graphql, pg_jsonschema, pgaudit, postgis, pg_uuidv7, h3; in-core trgm + LISTEN/NOTIFY channelization + advisory-lock claims + COPY bulk lanes)
├── scope/           # Tenancy as a scope value
│   ├── handle.ts    # StoreHandle Layer family keyed (appKey, tenancy policy); LayerMap-cached per-tenant Layers
│   └── tenant.ts    # RLS app.current_tenant GUC row set in-transaction; schema-per-app / database-per-app law
├── lane/            # Dialect lanes under the same contracts
│   ├── sqlite.ts    # sqlite node/bun lane + the explicit capability-degradation table (no RLS → file-per-app; no pg_ivm → in-process folds)
│   └── wasm.ts      # OPFS sqlite-wasm lane law — browser/persist consumes
├── retrieve/        # The retrieval plane
│   ├── hybrid.ts    # hybrid RRF over the FTS | trigram | phonetic | fuzzy | semantic lane roster; embedding-fingerprint keys per vector row; rerank row; facet/snippet/keyset-cursor families; Embedder port satisfied by ai/embed
│   └── index.ts     # vector/text index rows (pgvector/VectorChord, pg_search)
└── object/          # The content-addressed object plane
    ├── key.ts       # ObjectKey = kernel ContentKey; content-addressed store (kernel-delegating mint site); object lifecycle rows (reference-sweep GC by retention class)
    └── presign.ts   # presign + codec fan-out (sharp derivative rows); conditional-put content-address idempotency (If-None-Match; 412 = idempotent noop)
```

`journal` is the spine: one append surface, OCC by expected version, the idempotency ledger and outbox atomic in the same commit; schema evolution is read-time upcasting, so a new event version is one upcast fold and the log is never rewritten. `project` binds the `state` fold algebra to durability — inline lanes for read-your-writes, async lanes checkpointed and LISTEN/NOTIFY-woken. `capability` keeps PG power typed and fail-closed: extensions are deployment-image facts the `iac/kube` CNPG image satisfies, never JS deps. A new extension is one `capability/matrix.ts` row realized by the image; a new tenancy shape is one `scope` policy value; a new retrieval lane is one roster row; a new dialect lane is one row under the same journal/projection contracts.

## [02]-[SEAMS]

```text seams
journal/snapshot   ←  csharp:Rasm.Persistence/Element  # [WIRE]: SnapshotHeader + canonical-CBOR content-stable bytes
journal/append     ←  csharp:Rasm.Persistence/Version  # [WIRE]: CrdtOpWire MessagePack union + Hlc 16-byte cell
journal/snapshot   ←  typescript:wire/codec            # [WIRE]: SnapshotHeader decode transits the wire codec/snapshot row — store/journal consumes the decoded value, never re-decodes
journal/append     ←  typescript:wire/codec            # [WIRE]: CrdtOpWire decode transits the wire codec/crdt row — store/journal consumes the decoded ops, never re-decodes
journal/retain     ←  typescript:security/sign         # [SHAPE]: the AES-GCM envelope Shredder primitive — the one direct store → security edge; per-subject erasure = key destruction
journal/append     →  typescript:security/session      # [PORT]: SessionStore/IdentityJournal port Tags — store journal Layers satisfy them at the app root
journal/append     →  typescript:telemetry/signal      # [PORT]: audit + meter journal port Tags — store journal Layers satisfy them at the app root
retrieve/hybrid    ←  typescript:ai/embed              # [PORT]: the Embedder port declared here, satisfied by ai/embed at the app root
capability/matrix  →  typescript:iac/kube              # [BOUNDARY]: extension rows as CNPG deployment-image facts; DDL split — iac applies at provision time, store verifies at startup, runtime never mutates schema
```

The two `Rasm.Persistence` rows are the C#-inbound seams: C# mints `SnapshotHeader` and `CrdtOpWire`, `wire` decodes them through its `codec/snapshot` and `codec/crdt` rows, and `store/journal` consumes the decoded values — app-authored journal events never cross the C# wire. The port rows run outward: `security`, `telemetry`, and `work` never import `store`; each declares port Tags against its own models, and the app root satisfies them with `store` journal and driver Layers. `store → security` is the one direct edge — `journal/retain` imports the `security/sign` AES-GCM envelope primitive as its crypto-shredding `Shredder`, never a port. `retrieve` declares the `Embedder` port `ai/embed` satisfies at app composition. The DDL split closes the loop with `iac`: `iac` applies the idempotent declarative ensure at provision time, `store` verifies at startup, runtime never mutates schema.
