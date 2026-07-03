# [DOSSIER-API-TIERS] — Rasm.Persistence .api catalogs (both tiers) + manifests

Lane: api-tiers. Read FULLY: `libs/csharp/.api/*.md` (30 shared-substrate catalogs, 4574 LOC), `libs/csharp/Rasm.Persistence/.api/*.md` (77 per-package catalogs, 10302 LOC) — structurally fingerprinted all 107; deep-read the critical unmined + duplicate + EF-stack subsets; `Directory.Packages.props` (root), `Rasm.Persistence.csproj`, README/ARCHITECTURE/IDEAS/TASKLOG, `libs/.planning/architecture.md`. Owning pages read for mining judgment: Store/{blobstore,provisioning}, Query/cache, Element/identity (EF section) in full; the other 14 pages via corpus-a dossier + targeted demand-map/member greps. Stance: hostile, evidence-first. Every mined/unmined call carries a page:line or demand-map anchor. Sibling `dossier-corpus-a.md` owns the per-page fence verdicts + the fault-band registry finding (D1) — NOT re-derived here; this lane owns the PACKAGE↔PAGE mining ledger, the roster-vs-pageset divergence, and catalog-anchor drift.

Method note: demand map = `rg -l -F <token>` over all 18 `.planning` pages, per admitted package's signature token(s). "pages=0" = zero fence OR prose reference anywhere in the live page set. Catalog page-refs = `rg -o "(Store|Sync|Query|Version|Element|Ingest)/[A-Za-z_]+(#[A-Z_]+)?"` per catalog (the owning section each catalog claims to compose).

---

## [A] THE CENTRAL FINDING — ROSTER SIZED FOR A DELETED PAGE SET

The repeated "element rebuild" (TASKLOG closed cards) collapsed a ~30-page design to the current 18. The package roster (`.csproj` ~90 refs) + 77 per-package catalogs were authored against the PRE-collapse page set and never pruned or re-anchored. Result: a large orphan class + systemic catalog-anchor drift.

DELETED owner pages/sections still referenced by live catalogs (`rg` over `.planning` returns pages=0 for each page token; catalogs still point at them):
`Store/profiles`, `Store/remote`, `Store/server`, `Store/tenancy`, `Store/quality`, `Store/lifecycle` · `Query/federation`, `Query/pipeline`, `Query/transaction`, `Query/lanes` (plural; real page is `Query/lane`), `Query/rail` · `Sync/egress`, `Sync/schedule`, `Sync/coordination` · `Version/snapshots` · `Schema/ddl`.

The 18 SURVIVING pages: Element/{graph,codec,identity} · Version/{ledger,commits,timetravel,merge,provenance,retention,recovery} · Query/{lane,topology,columnar,cypher,cache} · Ingest/{tabular} · Store/{blobstore,provisioning}.

Corroboration the deletion is real, not a grep miss:
- `EgressSink`/`EgressPump`/`BulkPipeline`/`QualityRule`/`Substrait`/`PlanNode`/`FederatedEntity`/`FederatedPlan`/`SourceKind` → ZERO live pages (demand grep).
- `provisioning.md:37-43` `StoreProfile` is CLOSED to two engines (`server`=PG-18, `embedded`=SQLite, "never a third"); `provisioning.md:378` boundary rejects a third relational engine explicitly; `ARCHITECTURE.md:115` PROHIBITION "NEVER admit a new relational engine row — the sweep is closed (PostgreSQL + embedded SQLite only)."
- `api-clickhouse.md:3` charter: "composes the `Store/profiles` store-profile algebra as a distinct backend class" — `Store/profiles.md` does not exist.

---

## [B] MINED-VS-UNMINED LEDGER (per admitted Persistence package)

DEEP-MINED (composed as a load-bearing owner in a live page):
- `Marten` — the append substrate; `IDocumentSession`/`IDocumentStore`/`LightweightSession`/`QuerySession`/`SingleStreamProjection`/`AggregateStreamAsync`/`QueueSqlCommand` across graph/identity/cache/columnar/provisioning (demand pages=16). `api-marten.md` (253 LOC) is a full catalog.
- `Npgsql` — provisioning.md:240-251 the six-command `CreateBatch` catalog-read fold; identity/recovery. Deep.
- `Microsoft.Data.Sqlite` + `SQLitePCLRaw.bundle_e_sqlite3` — provisioning.md:369-434 EMBEDDED_FLOOR + ENGINE_OPERATIONS raw `Handle`/`sqlite3_db_config` bridge. Deep.
- `DuckDB.NET.Data.Full` — columnar.md COLUMNAR_LANE (demand pages=6). `api-duckdb.md` (269 LOC, the only catalog with an EXTENSION_PATTERN stacking section).
- `MessagePack`(+`MessagePackAnalyzer`) — codec/cache(L2 `SnapshotCodec.Binary`)/commits/ledger wire (pages=4). `cache.md:230-231` composes `MessagePackSerializer.Serialize/Deserialize`.
- `System.IO.Hashing` — `XxHash128` the one content-key hash everywhere (pages=12); substrate.
- `NodaTime`(+`.SystemTextJson`) — Instant/Duration everywhere (pages=17); substrate.
- `Thinktecture.Runtime.Extensions`(+`.Json`+`.MessagePack`) — SmartEnum/Union/ValueObject everywhere (pages=18); substrate.
- `QuikGraph` — topology.md the synchronous topology owner (pages=5); shared-substrate catalog.
- `AWSSDK.S3` + `Azure.Storage.Blobs` + `Google.Cloud.Storage.V1` + `Minio` — blobstore.md:148-388 the four-provider `ObjectClient` union + `ObjectLeg` nine-delegate transfer engine. Deep, exemplary.
- `AWSSDK.KeyManagementService` + `Azure.Security.KeyVault.Keys` + `Google.Cloud.Kms.V1` — identity.md:278 AUTHORITY signing/envelope + provenance. Mined.
- `CommunityToolkit.HighPerformance` — blobstore `ArrayPoolBufferWriter` + topology (pages=2).
- `Microsoft.Extensions.Caching.Hybrid` — cache.md:191-232 `IBufferDistributedCache`/`IHybridCacheSerializerFactory` L2. Deep.
- `pocketken.H3` — identity.md:141 `H3Index.FromPoint` + cypher (pages=3+).
- `System.Numerics.Tensors` — lane.md VECTOR_CODEBOOK `TensorPrimitives` (pages=1).
- `Sep`, `MiniExcel`, `Microsoft.Extensions.Compliance.Redaction` — tabular.md (Redaction=RedactionPlan). NOTE: `Ingest/` is a one-file folder (corpus-a D5); `Sep` is prose-named but shares tabular's page.
- `System.Formats.Cbor`, `ZstdSharp.Port`, `K4os.Compression.LZ4` — codec.md canonical CBOR + compression (Zstd also columnar).
- `Generator.Equals` — identity/merge/timetravel `[SetEquality]` (pages=3).
- `Apache.Arrow`(+`.Adbc`) + `ParquetSharp` — columnar ADBC Arrow bridge + tabular (pages=2-3).
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson` — merge.md RFC-6902 egress (pages=1).
- `Npgsql.EntityFrameworkCore.PostgreSQL`(+`.NodaTime`) + `Pgvector`(+`.EntityFrameworkCore`) — identity.md:76-170 IEntityTypeConfiguration + DbContext LINQ (see [E] for the PARTIAL/inconsistency caveat).
- `Speckle.Sdk`+`Speckle.Objects` — ledger.md#SYNC_TRANSPORTS one transport arm (pages=1). `api-speckle.md` correctly anchors `Version/ledger#SYNC_TRANSPORTS`.

WIRING-ONLY (mined at the app composition root, not in a page fence — acceptable, not a defect):
- `Npgsql.OpenTelemetry` — the ARCHITECTURE HEALTH_PROBE seam; DI-wired, no page fence.

UNMINED — admitted + fully cataloged + ZERO live-page consumer (the orphan class, grouped by dead owner):
- Store-backend tier (dead `Store/profiles`/`Store/remote`/`Store/tenancy`/`Store/quality`/`Store/lifecycle`): `ClickHouse.Driver` (pages=0), `ScyllaDBCSharpDriver` (pages=0, catalog has NO page-ref at all), `Qdrant.Client` (pages=0), `DeltaLake.Net` (columnar prose-mention only; catalog→Store/profiles+lifecycle+timetravel).
- Embedded KV (dead — provisioning EMBEDDED_FLOOR is SQLite-only + closed): `rocksdb` (pages=0), `LightningDB` (pages=0).
- Streaming egress (dead `Sync/egress`/`Sync/coordination`): `Confluent.Kafka`, `Confluent.SchemaRegistry`(+`.Serdes.Avro/Json/Protobuf`), `Chr.Avro`(+`.Binary`+`.Confluent`), `CloudNative.CloudEvents`(+`.Kafka`+`.SystemTextJson`), `NATS.Net`, `RabbitMQ.Client`, `DotPulsar` — 14 packages, all pages=0.
- Federation (dead `Query/federation`/`Query/pipeline`): `FlowtideDotNet.Substrait` (pages=0; IDEAS BLOCKED, but owner page gone — see [G]), `Apache.Arrow.Adbc.Drivers.BigQuery` (pages=0), `Apache.Arrow.Adbc.Drivers.Apache` (columnar ADBC generic; specific Thrift driver un-named), `Apache.Arrow.Flight` (TASKLOG defers to `Query/pipeline`=gone; blobstore/columnar prose-mention only).
- Schedule (dead `Sync/schedule`, relocated to Rasm.Bim): `MPXJ.Net` (pages=0).
- Sync-outside-rhino (dead `Store/remote`): `PollinationSDK` (pages=0).
- EF sub-stack (see [E]): `EFCore.NamingConventions`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Sqlite`, `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`.
- Cache swap (named as growth-path, not composed): `StackExchange.Redis` + `Microsoft.Extensions.Caching.StackExchangeRedis` — cache.md:185 "a redis swap is the `Microsoft.Extensions.Caching.StackExchangeRedis` `RedisCache`… selected by the lane's `Store` key"; the DEFAULT L2 is the Marten `CacheL2Store` (cache.md:191). The raw `StackExchange.Redis` driver (`ConnectionMultiplexer`/`IDatabase`) is composed nowhere.

PARTIAL / BYPASSED: `Pgvector.EntityFrameworkCore` (column-type map at identity.md:87, but the pgvector EF distance operators `<->`/`<=>` are NOT shown — lane.md's vector search is an in-process PQ/ADC scan, not a pgvector EF query); `linq2db.EntityFrameworkCore` (tabular BulkCopy prose ref, corpus-a D7 "referenced, not homed"); `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` (see [E] inconsistency); `Apache.Arrow.Compression` (Arrow-IPC codec, likely composed via Arrow).

Count: ~30 of ~90 admitted Persistence packages have NO live-page consumer. The dominant defect is over-provisioning (naivety-of-coverage in reverse: admitted capability the target never demands), NOT a coverage gap.

Server-extension SQL catalogs (`api-timescaledb`/`pg-search`/`pgrouting`/`apache-age`/`h3-pg`/`pg-graphql`/`pg-net`/`pgvectorscale`/`pg-server-bgworkers`) are MINED as `ServerExtension` roster rows (provisioning.md:119-138), even though several catalog headers mis-anchor to dead sections (see [D]). These are SQL-surface catalogs (no managed assembly) — correctly per README `[SERVER_EXTENSIONS]`.

---

## [C] TIER STRUCTURE, DUPLICATION, HYGIENE

Two tiers: `libs/csharp/.api/` = whole-BRANCH substrate (H1 owner-tags reveal cross-package ownership: `RASM_COMPUTE_API_*` = grpc*/protobuf/extensions-ai/highperformance; `RASM_APPHOST_API_*` = nodatime/jsonpatch/redaction/hybrid-cache; `RASM_APPUI_API_*` = unicolour/unitsnet; `RASM_API_*` = hashing/mapperly/quikgraph/generator-equals/tensors/thinktecture*/csparse/mathnet). Persistence consumes only a subset of the shared tier (mapperly/generator-equals/quikgraph/hashing/tensors/thinktecture*/highperformance/hybrid-cache/jsonpatch/redaction/nodatime*/messagepack); the grpc/protobuf/mathnet/csparse/unicolour/unitsnet/extensions-ai shared catalogs are other-owner (not a Persistence concern).

DEFECTS:
- `api-messagepack.md` is BYTE-IDENTICAL across both tiers, BOTH tagged `# [RASM_PERSISTENCE_API_MESSAGEPACK]` (`diff -q` = IDENTICAL). The shared-tier copy is MISFILED: a `RASM_PERSISTENCE_*` catalog belongs only in `Rasm.Persistence/.api/` (CLAUDE.md [04]: shared substrate → `libs/csharp/.api/`; package domain → package `.api/`). Delete the shared-tier copy or re-scope it to a neutral `RASM_API_MESSAGEPACK` substrate catalog.
- DOUBLE-CATALOGING: 9 packages carry BOTH a shared catalog (owned by another package) AND a Persistence overlay: `nodatime` (shared APPHOST + pkg PERSISTENCE), `jsonpatch`, `redaction`, `hybrid-cache`, `highperformance` (shared COMPUTE), `nodatime-stj`, `thinktecture-json`, `hashing`, `messagepack`. The overlay pattern is allowed ("folder-specific overlay", CLAUDE.md [04]) but is a drift surface — the shared `api-hashing` (96 LOC, `RASM_API`) and pkg `api-hashing` (102 LOC, `RASM_PERSISTENCE`) already diverge.
- PROVENANCE-TAG inconsistency: some catalogs open with `<!-- catalog:Name@version -->` (api-clickhouse, api-deltalake, api-dotpulsar), most do NOT (api-mapperly/marten/messagepack). No uniform tag → no machine-checkable version-drift audit.
- STACKING-section inconsistency: only `api-duckdb`#EXTENSION_PATTERN and `api-nats`#STACKING_AND_RAIL carry an explicit integration section; the other 75 fold stacking into `[04]-[IMPLEMENTATION_LAW]`. The rebuild-api charter (document how packages STACK into single dense rails) is unevenly met.
- All 107 catalogs are AUTHORED (54-480 LOC); NO one-line placeholder stubs exist. Catalog QUALITY (member depth) is high across the tier — the defect is anchor drift + orphan admission, not thin catalogs.

---

## [D] CATALOG ANCHOR DRIFT (systemic — affects mined AND orphaned packages)

Nearly every per-package catalog's `#SECTION` anchors point at renamed/deleted pages. This is INDEPENDENT of pruning: even KEPT/mined catalogs are mis-anchored and must be re-pointed to the live 18-page set. Evidence (catalog → the dead anchor it claims):
- `api-duckdb` → `Store/profiles` (DEAD; real owner columnar.md). DuckDB is deeply mined — anchor is simply stale.
- `api-arrow` → `Query/rail#ARROW_EGRESS` + `Sync/egress#EGRESS_SINK` (both DEAD; real owner columnar.md).
- `api-cbor` → `Sync/egress` (DEAD; real owner codec.md).
- `api-zstd`/`api-lz4`/`api-nodatime`/`api-thinktecture-serialization`/`api-messagepack-analyzer` → `Version/snapshots#CODEC_AXIS` (DEAD; real owner `Element/codec#CODEC_AXIS`).
- `api-highperformance` → `Store/remote#BLOB_REMOTE`/`#OBJECT_IO` (DEAD; real owner `Store/blobstore`).
- `api-messagepack` → `Sync/egress` (DEAD; real owners codec/cache/commits/ledger).
- `api-h3`/`api-sep`/`api-pgvectorscale`/`api-timescaledb`/`api-h3-pg` → `Query/lanes…` (DEAD plural; real page `Query/lane`).
- `api-pg-search` → `Store/server#SEARCH_PROVISIONING` (DEAD; real owner `Store/provisioning`).
- `api-apache-age`/`api-pgrouting`/`api-pg-graphql` → `Query/federation…` (DEAD; real owners cypher.md + provisioning.md).
- `api-hashing` → `Store/quality` (DEAD) + `Query/cache` (alive).
- `api-ef-sqlite`/`api-parquetsharp`/`api-pg-server-bgworkers` → `Store/profiles`/`Store/tenancy` (DEAD).

RULING: a rebuild-api pass must re-anchor all 77 catalogs to the live page set. Sixteen catalogs carry NO page anchor at all (`api-marten`/`npgsql`/`npgsql-ef`/`npgsql-ef-nodatime`/`npgsql-nts`/`npgsql-otel`/`ef-design`/`ef-naming`/`nts-ef`/`nts-io`/`thinktecture-ef`/`objectstore`/`redis`/`hybrid-cache`/`nodatime-stj`/`thinktecture-json`) — those need an anchor authored, not just corrected.

---

## [E] EF-CORE STACK — 80% UNMINED + INTERNALLY INCONSISTENT

identity.md is the SOLE EF Core consumer (the read-projection side: `IEntityTypeConfiguration<ElementIdentity>` + `<NodeCell>`, `DbContext` LINQ for H3/pgvector/GiST lanes "Marten LINQ cannot serve", identity.md:177). Everything else in the 9-package EF stack is unmined:
- `EFCore.NamingConventions` — UNMINED. identity.md:79-124 hand-names EVERY column (`HasColumnName("tenant")`/`"h3_cell"`/`"embedding"`/`"global_ids"`…) instead of `UseSnakeCaseNamingConvention()`. Demand grep: `ToSnakeCase`=0, `EFCore.NamingConventions`=0.
- `Microsoft.EntityFrameworkCore.Design` — UNMINED. `MigrationBuilder`/`migrationBuilder`=0 across all pages; the migration home `Schema/ddl.md` (TASKLOG `SCHEMADDL_SQL_COLLAPSE` references `Schema/ddl#EXTENSION_DDL` over `MigrationBuilder.Sql`) was deleted. The element_identity/node_cell tables have no authored DDL owner.
- `Microsoft.EntityFrameworkCore.Sqlite` — UNMINED. The embedded floor (provisioning.md:369-434) is raw `Microsoft.Data.Sqlite` ADO + SQLitePCLRaw, not EF. No page runs EF-over-SQLite. NOTE: this exposes an unspecified architectural seam — Marten is PG-only, so the `StoreProfile.Embedded` (SQLite) profile has NO shown ElementGraph-persistence path (the graph store rides Marten, which the embedded floor cannot host).
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` + `Npgsql.NetTopologySuite` + `NetTopologySuite.IO.GeoJSON4STJ` + `NetTopologySuite.IO.GeoPackage` — UNMINED. NTS is used ONLY for in-memory H3 cell derivation (identity.md:141 `new Point(...) { SRID = 4326 }`); NO persisted NTS geometry column, NO GeoJSON/GeoPackage codec in any page. The coverage raster rides blobstore as content-keyed bytes (blobstore.md:3), not GeoPackage.
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` — BYPASSED + INCONSISTENT. identity.md:82 `identity.Property(e => e.Cell).HasColumnName("h3_cell")` with NO explicit converter (relies on Thinktecture.EF `UseValueObjectValueConverter()` to map `H3Cell`→long), YET identity.md:120 hand-writes `node.Property(n => n.Node).HasConversion(static n => n.Value, static s => NodeId.Create(s))` for `NodeId` (also a Thinktecture ValueObject). Either the auto-converter is registered (the hand NodeId converter is dead double-registration) or it is not (H3Cell/ModelId/Tenant columns are unmapped). `ARCHITECTURE.md:113` PROHIBITION forbids "hand-written converters… beside the generated rails; Thinktecture converters… own those forms."

RULING: identity.md must either (a) fully commit to the EF read-projection — register `UseSnakeCaseNamingConvention()` + `UseValueObjectValueConverter()`, delete the hand `NodeId`/`ModelId` converters, author the migration owner (restore a Schema/ddl owner or fold DDL into identity.md), and resolve the embedded-profile persistence path — or (b) prune the unmined EF sub-stack (naming-conventions/design/sqlite/nts-ef/npgsql-nts/nts-io) and keep only the npgsql-ef + pgvector-ef + npgsql-ef-nodatime the read-projection actually composes. The LanguageExt-type converters (`Option<Vector>`/`Seq<NodeId>`/`HashMap`, identity.md:87-105) are legitimately hand-written (Thinktecture.EF does not own LanguageExt types) — those are NOT the defect.

---

## [F] DOMAIN GAPS — FEW; ROSTER IS OVER-PROVISIONED

The scope's concerns are covered or over-covered; there is no material gap where the 18-page design needs a concern no admitted package owns. Assessed candidates:
- CRDT engine (commits.md CRDT_ALGEBRA hand-rolls the six-CDT `CrdtField`/`Merge` join-semilattice): `LoroCs` 1.13.6 IS in the manifest (App-UI group, Directory.Packages.props:354) — a mature Rust-binding CRDT. CONSIDERED-AND-REJECTED: the ARCHITECTURE [07] wire law requires the CRDT op payload be reproduced BIT-IDENTICALLY across C#/Python/TS (`CrdtOpWire` MessagePack union, commits.md:266 the `[Key]` sequence IS the wire schema), which a Rust-binding CRDT whose wire is Loro's cannot satisfy. LoroCs is a distinct App-UI concern (live collaborative editing). Record so the author does NOT naively propose LoroCs as the Persistence CRDT owner.
- W3C-PROV (provenance.md): hand-built causal DAG — correctly a vocabulary, not a package gap (KMS SignAsync + System.Formats.Cbor + hash-chain suffice).
- Structural three-way merge / Merkle diff (merge.md): hand-built — a simple algorithm + JsonPatch RFC-6902 egress; not a package gap.
- DuckLake catalog (referenced in `api-deltalake.md` charter "beside the self-hosted DuckLake catalog"): if the self-hosted lakehouse catalog is in-scope, DuckLake (a DuckDB extension) has NO row in the `ServerExtension` roster nor a DuckDB-extension entry. Minor named candidate (not researched).

---

## [G] SCOPE-MANDATE CENSUS — api-tier angle

(1) Count-prefix wire law: NO api-tier impact — the amended `Node.ToCanonicalBytes` is seam-owned; codec.md composes `ContentAddress.Of` verbatim (corpus-a A1). No catalog change.
(2) 25xx→27xx Fabrication re-band: NO api-tier impact — no Fabrication package is (or may be) referenced by Persistence (`ARCHITECTURE.md:95,116`).
(3) PERSISTENCE_GRAPH_REBUILD: the `Marten` catalog (`api-marten.md`) is the load-bearing evidence surface; deeply mined by graph.md (corpus-a A3). No api defect.
(4) Materials-leg derived Type NodeId re-key: identity-migration concern; no package gap (`IdentityPolicy` owns it, identity.md:184-189).
(5) Streaming/incremental `ContentAddress.OfGraph`: kernel-owner boundary; `System.IO.Hashing` XxHash128 is the kernel's, composed not re-homed. No api defect.

IDEAS/TASKLOG REALIZATION PHANTOMS (api-tier relevant): IDEAS `[REUSE_WIRE]`/`[SUBSTRAIT_FEDERATION_SEAM]` + TASKLOG `[ARTIFACT_CONTENT_KEY_FEDERATION]` all assert "the Persistence half — `Query/Federation.cs` `FederatedEntity`/`FederatedPlan`/`SetExpr` — is realized." `Query/federation.md` does NOT exist; `FederatedEntity`/`FederatedPlan`/`PlanNode`/`SourceKind` appear in ZERO pages. The `FlowtideDotNet.Substrait` admission rides these phantom claims. The BLOCKED status is legitimate (waits on python:data), but the "C# half realized" clause is false.

---

## [H] VERDICT CANDIDATES (strongest first)

VC1 ★★★ ROSTER-VS-PAGESET DIVERGENCE. The element-rebuild collapse (30→18 pages) orphaned ~30 admitted packages whose owner pages/sections were deleted; the roster was never pruned. The campaign must dispose each orphan cluster: RESTORE the owner (if the capability is genuinely in-scope) or PRUNE the package (license-gate + one-owner-per-concern). This is the defining structural ruling — it decides ~1/3 of the manifest. Evidence: [A]/[B] demand-map pages=0 for the whole store-backend + egress + schedule + federation clusters; provisioning.md:378 + ARCHITECTURE.md:115 close the engine sweep.

VC2 ★★★ CATALOG ANCHOR DRIFT is systemic and independent of pruning. Nearly all 77 catalogs mis-anchor to renamed/deleted pages (`Store/profiles`/`Store/remote`/`Store/server`/`Query/lanes`/`Query/rail`/`Query/federation`/`Sync/egress`/`Version/snapshots`), INCLUDING deeply-mined packages (duckdb, arrow, cbor, zstd, lz4, highperformance, messagepack). A rebuild-api pass must re-anchor every kept catalog to the live 18-page set; 16 catalogs have no anchor at all. Evidence: [D].

VC3 ★★★ EF-CORE STACK is 80% unmined + internally inconsistent. identity.md is the sole EF consumer; naming-conventions/design/sqlite/nts-ef/npgsql-nts/nts-io are all unmined; identity.md simultaneously relies on Thinktecture.EF auto-conversion (H3Cell, no converter) and hand-writes a NodeId converter (identity.md:82 vs :120) — a PROHIBITION violation either way. Also surfaces an unspecified seam: the SQLite `StoreProfile.Embedded` profile has no ElementGraph-persistence path (Marten is PG-only). RULING: commit-or-prune the EF sub-stack; resolve the embedded-profile store path. Evidence: [E].

VC4 ★★ SCALE-OUT STORE-BACKEND TIER contradicts the sealed design law. ClickHouse/ScyllaDB/Qdrant/rocksdb/LightningDB/DeltaLake are admitted as "backends beyond single-PG/embedded-SQLite," but the only store-selection owner (StoreProfile) is closed to two engines and the in-PG tier already owns these concerns (TimescaleDB=OLAP, pgvector/pgvectorscale=ANN, DuckDB=columnar). RULING: dead admissions against the current design — prune unless the design workflow re-charters a multi-backend `Store/profiles` owner (reversing a sealed prohibition — a design decision, not a build-leg change). Evidence: provisioning.md:37-43,378; ARCHITECTURE.md:115; [B].

VC5 ★★ STREAMING EGRESS fully orphaned — 14 packages (Kafka + SchemaRegistry+3 Serdes + Chr.Avro+2 + CloudEvents+2 + NATS + RabbitMQ + DotPulsar) admitted for `Sync/egress#EGRESS_SINK` (TASKLOG CDC_STREAMING_EGRESS-COMPLETE), but the whole Sync/ folder is deleted and EgressSink/EgressPump=0 pages. RULING: if CDC egress is in-scope, restore ONE `Sync/egress` owner (the `pg_net` server-extension egress + CloudEvents envelope are the minimal spine — four parallel broker clients is not one-owner-per-concern); else prune all 14. Evidence: [A]/[B]; TASKLOG:61.

VC6 ★★ TIER-HYGIENE — dedup + standardize. `api-messagepack.md` is a byte-identical misfiled duplicate across tiers (both `RASM_PERSISTENCE`); 9 packages are double-cataloged (shared other-owner + Persistence overlay, already drifting); the `catalog:` provenance tag and the STACKING section are both inconsistently applied. RULING: delete the misfiled shared messagepack, standardize the provenance tag + a uniform stacking section across the kept tier. Evidence: [C].

VC7 ★ FEDERATION REALIZATION PHANTOM — IDEAS/TASKLOG claim `Query/Federation.cs` (`FederatedEntity`/`FederatedPlan`/`SetExpr`) is realized; the page and every symbol are absent (pages=0). `FlowtideDotNet.Substrait` + the ADBC BigQuery/Apache drivers + Arrow.Flight ride these phantom claims. RULING: author the real `Query/federation` lowering-target owner (the C# half the BLOCKED cross-language seams need) or correct the cards + drop Substrait/BigQuery/Flight until the owner lands. Evidence: [G]; IDEAS:34-47; TASKLOG:20-26.

VC8 ★ DOMAIN GAPS ARE FEW (roster over-provisioned, not under). No material capability the 18-page scope needs is unowned. Considered-and-rejected: LoroCs is NOT the Persistence CRDT owner (cross-runtime bit-parity wire law forecloses it); provenance/merge are correctly hand-built vocabularies. Minor named candidate: a DuckLake server/DuckDB-extension row if the self-hosted lakehouse catalog is in-scope. Evidence: [F].

WATERFALL-RIPPLE (upstream `RASM-CS-GEOMETRY-BRIEF.md`): NONE owed from this lane. The api-tier survey surfaces no capability Persistence demands that the geometry brief lacks; the parametric content-identity seam (geometry [V2]) already routes through codec.md's `ContentAddress.Of(UInt128)` wrap (corpus-a B). Ripple authority stays with the author agent; recorded here, none owed.
