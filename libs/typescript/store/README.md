# [STORE]

`store` is the W3 event-sourced durable persistence plane of `libs/typescript` — no migrations, by construction: the record of truth is an append-only journal with read-time upcasting, so the raw log is never rewritten and `PgMigrator` is banned branch-wide. One append surface keys streams `(appKey, tenantId, aggregate)`; typed extension-capability rows probe fail-closed; `StoreHandle` tenancy scopes make isolation a scope value, never a fork; the retrieval and object planes complete breadth-in-kind. DDL is idempotent declarative ensure with the split as law — `iac` applies at provision time, `store` verifies at startup, runtime never mutates schema. The folder imports `kernel`, `state`, `host`, and `security`, and publishes per-runtime subpath exports (`./server`, `./browser`, `./wasm`) with drivers banned on the neutral vocabulary subpaths. The domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[APPEND](.planning/journal/append.md): the ONE append surface — streams keyed `(appKey, tenantId, aggregate)`, OCC by expected version, events as closed `Schema.TaggedClass` families with app-authored `eventVersion`.
- [02]-[OUTBOX](.planning/journal/outbox.md): the transactional outbox atomic with the append plus the idempotency ledger (`ON CONFLICT DO UPDATE RETURNING (xmax = 0)` claim).
- [03]-[SNAPSHOT](.planning/journal/snapshot.md): the snapshot store keyed `snapshot_schema_version`.
- [04]-[UPCAST](.planning/journal/upcast.md): read-time `eventVersion` upcaster folds — total functions proven in `proof/law`; the raw log is never rewritten.
- [05]-[RETAIN](.planning/journal/retain.md): retention policy rows plus crypto-shredding via the `security/sign` `Shredder` — per-subject erasure is key destruction — with the per-subject DSAR export fold riding beside erasure.
- [06]-[INLINE](.planning/project/inline.md): same-transaction read-your-writes lanes binding `state` folds to durability.
- [07]-[ASYNC](.planning/project/async.md): checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED lanes.
- [08]-[REBUILD](.planning/project/rebuild.md): `pg_cron` rebuild/compaction plus `pg_ivm` in-database IVM rows.
- [09]-[ROW](.planning/capability/row.md): the closed `{extension, floor, probeSql, capabilities, layer}` vocabulary with fail-closed probes.
- [10]-[MATRIX](.planning/capability/matrix.md): the PG 18.4 extension matrix rows — deployment-image facts the `iac/kube` CNPG image satisfies, never JS deps.
- [11]-[HANDLE](.planning/scope/handle.md): the `StoreHandle` Layer family keyed `(appKey, tenancy policy)` with `LayerMap`-cached per-tenant Layers.
- [12]-[TENANT](.planning/scope/tenant.md): the RLS `app.current_tenant` GUC row set in-transaction; the schema-per-app / database-per-app law.
- [13]-[SQLITE](.planning/lane/sqlite.md): the sqlite node/bun lane under the same journal/projection contracts with the explicit capability-degradation table.
- [14]-[WASM](.planning/lane/wasm.md): the OPFS sqlite-wasm lane law `browser/persist` consumes.
- [15]-[HYBRID](.planning/retrieve/hybrid.md): hybrid RRF over the FTS | trigram | phonetic | fuzzy | semantic lane roster — embedding-fingerprint keys per vector row, a rerank row, facet/snippet/keyset-cursor families; the `Embedder` port `ai/embed` satisfies.
- [16]-[INDEX](.planning/retrieve/index.md): the vector/text index rows (pgvector/VectorChord, pg_search).
- [17]-[KEY](.planning/object/key.md): `ObjectKey` = kernel `ContentKey` — the kernel-delegating mint site — plus object lifecycle rows (reference-sweep GC by retention class).
- [18]-[PRESIGN](.planning/object/presign.md): presign plus codec fan-out (`sharp` derivative rows); conditional-put content-address idempotency — If-None-Match, 412 = idempotent noop.

## [2]-[DOMAIN_PACKAGES]

The folder-local packages this folder owns; versions live only in the `pnpm-workspace.yaml` catalog. The drivers are ban-fenced to this folder — `@effect/sql` core alone travels, as the `work` `SqlClient` port vocabulary.

[SQL_RAIL]:
- `@effect/sql` — the SQL rail core: the `SqlClient` Tag, statements, and transactions every journal, projection, and lane row composes.
- `@effect/sql-pg` — the PostgreSQL driver Layer: the pg journal spine, LISTEN/NOTIFY channelization, advisory-lock claims, COPY bulk lanes.

[SQLITE_LANES]:
- `@effect/sql-sqlite-node` — the node dialect row behind `lane/sqlite`.
- `@effect/sql-sqlite-bun` — the bun dialect row behind `lane/sqlite`.
- `@effect/sql-sqlite-wasm` — the OPFS browser dialect row behind `lane/wasm`.

[OBJECT_PLANE]:
- `@aws-sdk/client-s3` — the S3-compatible object client behind the content-addressed object store.
- `@aws-sdk/s3-request-presigner` — presigned-URL minting for the object presign rows.
- `sharp` — the image-derivative codec behind the presign fan-out rows.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry lives in `libs/typescript/.planning/README.md` with catalogues at `libs/typescript/.api/`.

- `effect` — rails, `Schema`, `Layer`, `LayerMap`, `Match`, `Stream`; the `StoreHandle` Layer cache and every rail ride it.
- `@effect/experimental` — the `EventLog` local-first sync overlay only; the record of truth never depends on it.
