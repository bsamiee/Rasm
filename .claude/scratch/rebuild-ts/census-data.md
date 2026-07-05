# [CENSUS_TS_DATA]

Read-only truthful census of `libs/typescript/data`. 43 files, 5224 LOC total (`.planning` 3796, `.api` 1267, root 161). Every `.planning` page and `.api` catalog was read in full; nothing in this dossier is inferred from filenames alone.

## [01]-[FILE_REGISTER]

| [FILE] | [OWNED_CAPABILITY] | [ENTRY_SURFACE] | [FENCE_MASS] |
| :----- | :------------------ | :--------------- | :------------ |
| `README.md` | Router + domain/substrate package manifest | n/a (doc) | prose, 60 LOC |
| `ARCHITECTURE.md` | Codemap, seams, boundaries | n/a (doc) | prose, 51 LOC |
| `IDEAS.md` / `TASKLOG.md` | Forward pool / open work | n/a (doc) | empty templates only, no live cards |
| `.planning/lane/postgres.md` | `Pg` — spine capability rows (`uuidv7`/`returningOldNew`/`virtualGenerated`/`temporal`/`skipScan`), concurrency-primitive table (upholds/denies), ruled extension matrix (17 rows: pgvector/vchord/vchord_bm25/timescaledb/pg_partman/pg_cron/pg_ivm/pg_incremental/pg_duckdb/pg_parquet/pg_graphql/pg_jsonschema/pgaudit/postgis/h3/pg_trgm/fuzzystrmatch), 3 driver Layer mints | `Pg.client`, `Pg.fromPool`, `Pg.rows`, `Pg.image`, `Pg.core` | heavy, dense decision tables + 1 code fence (~200 LOC) |
| `.planning/lane/sqlite.md` | `Sqlite` — 5-profile degradation table (23 spine grants × 4 profile columns), 5 Layer constructors (node/bun/opfs/memory/libsql/d1), snapshot IO (export/backup/seed/dump/extend) | `Sqlite.node/bun/opfs/memory/libsql/d1`, `.snapshot/.backup/.extend` | heavy (~180 LOC) |
| `.planning/lane/olap.md` | `Olap` — 4-engine decision table (duckdbNode/duckdbWasm/pgDuckdb/clickhouse), scoped node+wasm wraps, lake attach rows (pg/sqlite/ducklake/httpfs), ClickHouse driver row, Arrow wire codec | `Olap.node/wasm/query/attach/clickhouse/wire` | heavy (~205 LOC) |
| `.planning/lane/cache.md` | `CacheLane` — 8-row escalation table (memo/keyed/request/persisted/pooled/external-gated/writeBehind-banned/redisClient-banned), keyed single-flight + request dedup, persisted cache over `KeyValueStore`, RcRef/RcMap pools | `CacheLane.keyed/dedup/persisted/handle/map` | medium (~107 LOC) |
| `.planning/lane/capability.md` | `Capability` — fail-closed extension/ensure probe service, one batched `RequestResolver` over `pg_extension`, `require`/`when` gates, one `CapabilityFault` family | `Capability.Default(rows, ensures, core)`, `.require`, `.when` | medium-heavy (~183 LOC) |
| `.planning/lane/tenant.md` | `Tenancy` tagged family (Rls/SchemaPerApp/DatabasePerApp) + locus fold, `Tenancy.within` (GUC/search-path transformer), `Stores` LayerMap (ScopeKey → SqlClient\|Capability), port-satisfaction contract (9 security ports) | `Tenancy.within/rls`, `Stores.get/invalidate/port` | heavy (~196 LOC) |
| `.planning/journal/append.md` | `Journal` — the one atomic write owner: `StreamKey`, OCC append (`Occ` Exact/None/Any), idempotency ledger claim (`(xmax=0)`), atomic publish (claim→append→outbox→slots→settle→NOTIFY), windowed read, relay claim/complete (SKIP LOCKED) | `Journal.of(spec).{append,head,read,publish}`, `Journal.claimBatch/complete` | heaviest page (~470 LOC) |
| `.planning/journal/evolve.md` | `Upcast` — total per-tag step-chain lift + decode, `Snapshot` — projection-as-latest-per-stream with monotonic upsert, `hydrate` (snapshot+tail) | `Upcast.plan/chain`, `Snapshot.of/due/hydrate` | heavy (~244 LOC) |
| `.planning/journal/fact.md` | `Fact` — polymorphic `AuditFact`/`MeterFact` union, one buffered drain rail (`Queue`+`groupedWithin`+retry), rollup+rating (`BigDecimal` exact arithmetic) | `Fact.record`, `Fact.rollup/rate`, `Fact.Default(identity)` | heavy (~302 LOC) |
| `.planning/journal/retain.md` | `Retain` — 4-class retention vocabulary + window table, frontier ledger, `Shredder`-backed crypto-shred ledger (seal/open/erase), per-subject DSAR export fold | `Retain.seal/open/erase/dsar` | heavy (~228 LOC) |
| `.planning/object/store.md` | `ObjectStore` — scoped S3 client, abort-bridged send, 412-as-replay fault fold, conditional put algebra (plain/multipart/streaming), verified get, reference-ledger GC (If-Match CAS sweep), presign grant mint, 8-engine conformance table | `ObjectStore` service: `.put/.putKeyed/.get/.head/.settled/.sweep/.grant/.refer/.release` | heaviest object page (~404 LOC) |
| `.planning/object/stream.md` | `Rail` — BYOB ingress, FastCDC content-defined chunking (owned wasm surface), incremental digest identity fold (checkpointed for resume), tus `Server`+`S3Store` resumable assembly, ranged reads | `Rail.bytes/chunked/identity/of/range` | heavy (~248 LOC) |
| `.planning/object/file.md` | `Disk` — content-addressed filesystem intake/watch/stage/egress over `@effect/platform` `FileSystem`, `Derive` — sharp derivative fan-out (gate→decode-once→clone-N→encode→mint→conditional-reput→grant) | `Disk.intake/watch/stage/egress`, `Derive.fanout` | heavy (~200 LOC) |
| `.planning/read/query.md` | `Query.table` — model field-family law (`Model.Class` 6-variant derivation), `SqlSchema` typed CRUD (findAll/findOne/single/void), `SqlResolver` rows (ordered/grouped/findById/void) assembled into one per-relation owner | `Query.table(model, spec)` | heavy (~180 LOC) |
| `.planning/read/batch.md` | `Batch` — general request-batching engine: request-class law, `Batch.of` resolver mint (batchN/aroundRequests/contextFromServices), 3 window geometries (traversal/dataLoader/persisted), served-lane census (probe/presence/descriptor/head) | `Batch.of/baked/tagged/windowed/durable` | medium-heavy (~145 LOC) |
| `.planning/read/fold.md` | `Lane` — projection binding at 3 staleness budgets: inline slot (in-transaction), drain daemon (SKIP LOCKED + LISTEN/NOTIFY + quarantine), maintenance rows (cron/ivm/incremental) + shadow-table rebuild w/ advisory lock | `Lane.of/inline/daemon/rebuild/replay/jobs` | heaviest read page (~360 LOC) |
| `.planning/read/live.md` | `Live` — reactivity-keyed invalidation: key-coordinate vocabulary (band/cells), `Live.of` (read/changes/mailbox), foreign-write edge (`mutation`/`invalidate`) | `Live.of/band/cells/mutation/invalidate` | medium (~112 LOC) |
| `.planning/read/search.md` | `Search` — 5-lane retrieval (fts/trigram/phonetic/fuzzy/semantic) fused by RRF, `Embedder`/`Reranker` ports, fingerprint-keyed embedding index, grant-gated index ensure, keyset cursor | `Search.of(corpus).{ensure,search,facets}` | heaviest — most complex query builder (~388 LOC) |

## [02]-[.API_ROSTER]

| [CATALOG] | [PACKAGE] | [DEPTH_SIGNAL] |
| :--- | :--- | :--- |
| `apache-arrow.md` | `apache-arrow` | shallow overlay (41 LOC) — explicitly defers full catalogue to `ui/.api/apache-arrow.md`, records only the interchange seam |
| `aws-sdk-client-s3.md` | `@aws-sdk/client-s3` | deep (117 LOC) — command inventory, 412-noop pattern, abort-bridge idiom fully specified |
| `aws-sdk-lib-storage.md` | `@aws-sdk/lib-storage` | medium (68 LOC) — `Upload` param-spread mechanics for conditional survival under streaming |
| `aws-sdk-s3-request-presigner.md` | `@aws-sdk/s3-request-presigner` | medium (79 LOC) |
| `basic-ftp.md` | `basic-ftp` | medium (65 LOC) — full dial/resume/TLS surface documented |
| `chokidar.md` | `chokidar` | medium (60 LOC) — v5 API, glob-removal note, awaitWriteFinish levers |
| `duckdb-node-api.md` | `@duckdb/node-api` | shallow-medium (51 LOC) |
| `duckdb-wasm.md` | `@duckdb/duckdb-wasm` | shallow-medium (53 LOC) |
| `effect-sql-clickhouse.md` | `@effect/sql-clickhouse` | shallow-medium (51 LOC) |
| `effect-sql-d1.md` | `@effect/sql-d1` | shallow (48 LOC) |
| `effect-sql-libsql.md` | `@effect/sql-libsql` | shallow (56 LOC) |
| `effect-sql-pg.md` | `@effect/sql-pg` | deep (115 LOC) — three-member driver surface plus the whole SQL-as-data law explained |
| `effect-sql-sqlite-bun.md` | `@effect/sql-sqlite-bun` | deep (106 LOC) — cross-references all three sqlite driver peers |
| `effect-sql-sqlite-node.md` | `@effect/sql-sqlite-node` | medium (72 LOC) |
| `effect-sql-sqlite-wasm.md` | `@effect/sql-sqlite-wasm` | medium (75 LOC) |
| `effect-sql.md` | `@effect/sql` | deepest (168 LOC) — full neutral-rail contract: SqlSchema, SqlResolver, Model, transaction spine, overlay Storage tags |
| `sharp.md` | `sharp` | deep (130 LOC) |
| `tus-s3-store.md` | `@tus/s3-store` | medium (70 LOC) |
| `tus-server.md` | `@tus/server` | medium (77 LOC) |
| `webdav.md` | `webdav` | medium (66 LOC) |

20 catalogs total; every README-listed domain package (relational, analytical, object-plane, filesystem, remote-transfer) has a matching `.api` file. No stub/placeholder catalogs — every file carries real narrative depth (40+ LOC minimum).

## [03]-[CAPABILITY_MAP_AND_MISMATCHES]

**Genuinely owned today** (design-page-realized, cross-referenced consistently across pages):
- Guarantee-lane matrix: postgres spine, 5-profile sqlite degradation, OLAP 4-engine table, cache 8-row escalation table, fail-closed capability probe, tenancy (RLS/schema/database-per-app) — all six lane pages exist, cross-cite each other correctly, and the README's lane router list (`[01]`-`[06]`) matches exactly.
- Journal: atomic append/publish/idempotency/relay (`append.md`), schema evolution + snapshotting (`evolve.md`), audit+meter fact rail (`fact.md`), retention/crypto-shred/DSAR (`retain.md`) — four pages, README router `[07]`-`[10]` matches.
- Object plane: content-addressed store with GC (`store.md`), resumable tus rail with FastCDC (`stream.md`), filesystem intake + sharp derivatives (`file.md`) — README router `[11]`-`[13]` matches.
- Read side: typed CRUD/batching fusion (`query.md`), general batch engine (`batch.md`), projection at 3 staleness budgets (`fold.md`), reactivity (`live.md`), 5-lane fused search (`search.md`) — README router `[14]`-`[18]` matches.
- ARCHITECTURE.md's domain map (`lane/journal/object/read` four-way split) and its 11 named cross-folder seams are each traceable to a real page section (e.g. `read/fold ← core/state`, `object/store ⇄ core/value`, `lane/tenant → security/access`, `journal/append ⇄ runtime/work`).

**Named mismatches**:
1. **README domain-package list vs. realized code — filesystem/remote-transfer packages are catalogued but NOT wired into any page.** README's `[FILESYSTEM]` row lists `chokidar` and `[REMOTE_TRANSFER]` lists `basic-ftp`/`webdav`, and all three have full `.api` catalogs (each explicitly self-describing as "a remote-origin backend row of the `object/file` plane beside the SFTP and DAV rows"). But `object/file.md` — the only page that could consume them — implements `Disk.watch` exclusively over `@effect/platform`'s `FileSystem.watch` (platform-native, not chokidar) and has zero mention of FTP, WebDAV, or SFTP ingestion anywhere in its five clusters. The three catalogs also reference an "SFTP row" that has no corresponding `.api` file and no package entry in README at all — a third, wholly undocumented remote-origin class the catalogs assume exists.
2. **RESEARCH-flagged incompleteness inside otherwise-settled pages** (self-declared, not a finding but worth flagging as real open surface): `olap.md` defers the unbounded incremental-stream reader-continuation spelling; `object/stream.md` defers the chunk-level Merkle proof-tree row; `read/fold.md` defers the `pg_incremental` pipeline-registration statement spelling; `read/search.md` defers the `vchord_bm25` index DDL and rank-fragment spellings. Four RESEARCH tags total, each scoped to one row/clause, none blocking the page's settled majority.
3. **IDEAS.md / TASKLOG.md are empty templates** — both files contain only the source-only card-authoring instructions with `(none)` in every OPEN/CLOSED section. No live forward-pool or open-work signal exists for this folder despite the domain being deep and RESEARCH-flagged in four places above; the deferred work is documented only inline in the design pages, not distilled into tracked cards.
4. **`project.json` scope tag `plane:runtime`** — ARCHITECTURE.md and README frame `data` as the wave-2 durable-persistence package (a data/storage plane), while the Nx project tag says `plane:runtime`; the tag vocabulary and the domain's self-description diverge (no `plane:data` or `plane:storage` tag exists in this file to check against).

No stub `.planning` pages exist — all 18 are fully authored with real code fences, laws, and cross-references. No orphaned `.api` catalogs (unreferenced-by-any-page) beyond the chokidar/basic-ftp/webdav trio above.
