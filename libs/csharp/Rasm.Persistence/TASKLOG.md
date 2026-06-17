# [PERSISTENCE_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`. The tier-2 probe battery (Redis 7.4.9, MinIO/Azurite/fake-gcs, PG18.4 + 12 extensions on paradedb + timescaledb-ha) closed every owner whose proof is a live emulator/server roundtrip; the residue below is the native-SQLite host-load class, the live logical-replication/pgaudit-session class, the BIM-currency live-format class, and the unified-image packaging build gated on a first server root.

## [1]-[NATIVE_SQLITE_HOST_LOAD]

The e_sqlite3 host-load class the console probe cannot exercise; the owner member shape is fence-complete.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | vec0 sourcing route chosen (package payload vs vendored tarball) and live load verified on osx-arm64 | native-sqlite#EXTENSION_GATES | SPIKE |
| [2] | SQLCipher provider route with the external dylib confirmed on the target RID; keying surface verified | native-sqlite#EXTENSION_GATES | SPIKE |
| [3] | sqlean per-RID artifacts land as build content per RID | native-sqlite#EXTENSION_GATES | SPIKE |
| [4] | Two-process first-open race (racing `MigrateAsync` + busy_timeout, one WAL file) outcome documented; WAL-lock handling and `Claim` arbitration confirmed | store-profiles#CROSS_PROCESS_LAW | SPIKE |
| [5] | DuckDB `sqlite_scanner` ATTACH snapshot visibility under a concurrent WAL writer confirmed | data-lanes#ANALYTICAL_LANE | SPIKE |
| [6] | APFS rename durability without directory fsync confirmed; migration-lock holder evidence captured | store-profiles#STORE_LIFECYCLE | SPIKE |
| [7] | Hardened-runtime dlopen of extension dylibs inside the signed Rhino host (`scenarios/extension-load.verify.csx`) succeeds | native-sqlite#EXTENSION_GATES | SPIKE |

## [2]-[LIVE_REPLICATION_AND_AUDIT]

The live PG18 logical-replication decode-fold and pgaudit session-audit class; the owners are fence-complete and gate on a live PG18 root with a configured publication/subscription.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Live pgoutput stream emits the catalogued leaf set under `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel`; `publish_generated_columns`, idle-slot timeout, and subscription conflict-stat columns recorded; `SetReplicationStatus`/`SendStatusUpdate` LSN ack against the live slot | sync-collaboration#TRANSPORT_AXIS | SPIKE |
| [2] | pgaudit session-audit category semantics under `shared_preload_libraries=pgaudit` against the `Categories` rows; per-tenant `BindTenant` category emission verified against a per-tenant `CREATE POLICY` on a live PG18 server | redaction-retention#AUDIT_BINDING | SPIKE |

## [3]-[UNIFIED_IMAGE_PACKAGING]

No single off-the-shelf image carries the full admitted extension set (pg_search is AGPL pgrx shipping in paradedb; TSL timescaledb ships in timescaledb-ha; both halves proved on their respective images). The capability is finalized; only the packaging build is open — a deploy-asset deliverable gated on the first server app root.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Custom Dockerfile `FROM timescale/timescaledb-ha:pg18` + apt partman/pgaudit/squeeze + build/copy pg_search + pgvectorscale, with the full `shared_preload_libraries` list, builds and boots; the `io_method=io_uring` vs `worker` triple's deploy-image kernel value observed in `pg_settings` | server-tier#CLUSTER_CONFIG | BLOCKED |
| [2] | Server self-provisioning rows (postgresql.conf preload fragments, pg_hba fragments, role grants, server-side extension enablement) exercised against the first live server root | store-profiles#PROVISIONING_ROWS | BLOCKED |

## [4]-[BIM_CURRENCY_LIVE_PROBES]

The BIM-currency owners ride the existing op-log / content-addressed-snapshot / PostGIS-GiST substrate and admit no new engine. The residue is the live-PG/live-format/host-credential class each owner names in its page RESEARCH cluster.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Tree-edit-distance cost bound over a 10^6-node graph confirmed linear in changed-node count; brep/mesh canonical-adjacency `GeometryHash` distinguishes morph from topology break | version-control#STRUCTURAL_DIFF | SPIKE |
| [2] | Multi-peer convergence harness proves the join-semilattice LUB holds under permuted op multisets + RGA tombstone-reclamation quiescence horizon | version-control#CRDT_ALGEBRA | SPIKE |
| [3] | Three-way RRF CTE planner route + cost-model engine selection + `ST_3DIntersects` clash pushdown against live PG18 + pgvectorscale + pg_search | federation#FEDERATED_PLAN · federation#FUSION_RANK | SPIKE |
| [4] | Lineage slice cost over a 10^7-edge DAG + redaction-preserving chain verify against live PG18 pgaudit | provenance#CAUSAL_DAG · provenance#LINEAGE_CDC | SPIKE |
| [5] | BCF 2.1/3.0 XML member shapes against the buildingSMART schema + BCF-API OAuth2 PKCE flow against a live CDE | annotation#BCF_PROTOCOL · annotation#CDE_SYNC | SPIKE |
| [6] | Published Uniclass/OmniClass/MasterFormat ltree load + DuckDB `GROUP BY ROLLUP` formula pushdown | catalog-cost#CLASSIFICATION_CATALOG · catalog-cost#COST_ROLLUP | SPIKE |
| [7] | P6 XER `%T`/`%F`/`%R` table grammar + MS-Project XML element shape against real exports | schedule-interchange#SCHEDULE_STORE | SPIKE |
| [8] | `ST_Transform` datum-shift accuracy + IFC `IfcMapConversion` member shapes + survey-point similarity-fit residual against live PG18 + postgis | data-lanes#GEO_LANES | SPIKE |
| [9] | IVM signed-delta sliding-window maintenance + watermark late-arrival retraction + DuckDB vector-chunk zero-copy carrier lifetime | query-rail#STANDING_QUERY · query-rail#ARROW_PLANE | SPIKE |
| [10] | AppHost-resolved signing-key handle + op-digest signature + public-key verify confirmed at the integrated host | schema-rail#IDENTITY_POLICY | SPIKE |
| [11] | `Collect` reachable-set-versus-residence eviction confirmed against a live object-store listing and a live `Closure` union; `LegalHold` exemption holds; eviction-delete round-trips through the residence-axis delete | redaction-retention#RETENTION_SWEEPS | SPIKE |

## [5]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (vocabulary owners before consumers, `Stores/Profiles.cs` through `Schedule/ScheduleInterchange.cs`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`; the test project `Rasm.Persistence.Tests` node is present and empty | store-profiles#PROFILE_AXIS | QUEUED |
