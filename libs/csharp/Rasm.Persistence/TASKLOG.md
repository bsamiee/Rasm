# [PERSISTENCE_TASKLOG]

Open work owned by this folder; closed items do not appear. Each row carries its owner, the `page#cluster` it gates, and the named environment that closes it. The tier-2 probe battery (Redis 7.4.9, MinIO/Azurite/fake-gcs, PG18.4 + 12 extensions on paradedb + timescaledb-ha) closed every owner whose proof is a live emulator/server roundtrip; the residue below is the native-SQLite host-load class, the live logical-replication/pgaudit-session class, and the unified-image packaging build — each blocked only by a host/asset/build this single-RID arm64-mac console cannot supply, never a capability or member-shape gap.

## [1]-[NATIVE_SQLITE_HOST_LOAD]

Native-SQLite extension load + at-rest encryption + multi-process file coordination — the e_sqlite3 host-load class the console probe cannot exercise; the owner member shape is fence-complete (`native-sqlite#EXTENSION_GATES`, `store-profiles#CROSS_PROCESS_LAW` SPIKE on the DENSITY_BAR).

| [INDEX] | [OWNER]             | [PAGE#CLUSTER]                   | [EXIT]                                                                                                                                                   |
| :-----: | ------------------- | -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ExtensionGate`     | native-sqlite#EXTENSION_GATES    | vec0 sourcing route chosen (package payload vs vendored tarball) and live load verified on osx-arm64                                                     |
|   [2]   | `EncryptionGate`    | native-sqlite#EXTENSION_GATES    | SQLCipher provider route with the external dylib confirmed on the target RID                                                                             |
|   [3]   | `ExtensionGate`     | native-sqlite#EXTENSION_GATES    | sqlean per-RID artifacts land as build content per RID                                                                                                   |
|   [4]   | `StoreLeaseRow`     | store-profiles#CROSS_PROCESS_LAW | two-process first-open race (racing `MigrateAsync` + busy_timeout, one WAL file) outcome documented; WAL-lock handling and `Claim` arbitration confirmed |
|   [5]   | `TabularExportSpec` | data-lanes#ANALYTICAL_LANE       | DuckDB `sqlite_scanner` ATTACH snapshot visibility under a concurrent WAL writer confirmed                                                               |
|   [6]   | `StoreLifecycle`    | store-profiles#STORE_LIFECYCLE   | APFS rename durability without directory fsync confirmed; migration-lock holder evidence captured                                                        |

## [2]-[LIVE_REPLICATION_AND_AUDIT]

The live PG18 logical-replication decode-fold and pgaudit session-audit class — extensions were `CREATE EXTENSION`'d in the battery but the running-stream and session-category semantics were not exercised; the owners are fence-complete (`sync-collaboration#TRANSPORT_AXIS`, `redaction-retention#AUDIT_BINDING` SPIKE on the DENSITY_BAR) and gate on a live PG18 root with a configured publication/subscription.

| [INDEX] | [OWNER]         | [PAGE#CLUSTER]                    | [EXIT]                                                                                                                                                                                                                                                                                                                  |
| :-----: | --------------- | --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `SyncTransport` | sync-collaboration#TRANSPORT_AXIS | live pgoutput stream emits the catalogued leaf set under `PgOutputProtocolVersion.V4` + `PgOutputStreamingMode.Parallel`; `publish_generated_columns`, idle-slot timeout, and subscription conflict-stat columns recorded; `SetReplicationStatus`/`SendStatusUpdate` LSN ack against the live slot ([LIVE_REPLICATION]) |
|   [2]   | `AuditBinding`  | redaction-retention#AUDIT_BINDING | pgaudit session-audit category semantics under `shared_preload_libraries=pgaudit` against the `Categories` rows, and the per-tenant `BindTenant` category emission verified against a per-tenant `CREATE POLICY` on a live PG18 server ([PGAUDIT_CATEGORIES])                                                           |

## [3]-[UNIFIED_IMAGE_PACKAGING]

No single off-the-shelf image carries the full admitted extension set (pg_search is AGPL pgrx shipping in paradedb; TSL timescaledb ships in timescaledb-ha; both halves PROVED on their respective images). The capability is FINALIZED; only the packaging build is open — a Gate-2C/implementation deliverable, never a capability or probe gap on `server-tier#CLUSTER_CONFIG`.

| [INDEX] | [OWNER]         | [PAGE#CLUSTER]             | [EXIT]                                                                                                                                                                                                                                                                                          |
| :-----: | --------------- | -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ClusterConfig` | server-tier#CLUSTER_CONFIG | custom Dockerfile `FROM timescale/timescaledb-ha:pg18` + apt partman/pgaudit/squeeze + build/copy pg_search + pgvectorscale, with the full `shared_preload_libraries` list, builds and boots; the `io_method=io_uring` vs `worker` triple's deploy-image kernel value observed in `pg_settings` |

## [4]-[HOST_BRIDGE]

| [INDEX] | [OWNER]         | [PAGE#CLUSTER]                | [EXIT]                                                                                                                                          |
| :-----: | --------------- | ----------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ExtensionGate` | native-sqlite#EXTENSION_GATES | hardened-runtime dlopen of extension dylibs inside the signed Rhino host (`scenarios/extension-load.verify.csx`) succeeds; verify script passes |

## [5]-[BIM_CURRENCY_LIVE_PROBES]

The BIM-currency owners (`version-control`, `federation`, `provenance`, `annotation`, `catalog-cost`, `schedule-interchange`) are fence-complete and FINALIZED on the DENSITY_BAR — they ride the existing op-log / content-addressed-snapshot / PostGIS-GiST substrate and admit no new engine. The residue below is the live-PG/live-format/host-credential class each owner names in its page RESEARCH cluster, each blocked only by a live server, a real interchange file, or an OS credential prompt this console arm cannot supply unattended, never a capability or member-shape gap.

| [INDEX] | [OWNER]                        | [PAGE#CLUSTER]                                     | [EXIT]                                                                                                                                                                                                     |
| :-----: | ------------------------------ | -------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `StructuralMerge`              | version-control#STRUCTURAL_DIFF                    | tree-edit-distance cost bound over a 10^6-node graph confirmed linear in changed-node count; brep/mesh canonical-adjacency `GeometryHash` distinguishes morph from topology break ([STRUCTURAL_DIFF_COST]) |
|   [2]   | `Crdt`                         | version-control#CRDT_ALGEBRA                       | multi-peer convergence harness proves the join-semilattice LUB holds under permuted op multisets + RGA tombstone-reclamation quiescence horizon ([CRDT_COMPACTION])                                        |
|   [3]   | `FederatedPlan`/`FusionRank`   | federation#FEDERATED_PLAN + #FUSION_RANK           | three-way RRF CTE planner route + cost-model engine selection + `ST_3DIntersects` clash pushdown against live PG18 + pgvectorscale + pg_search ([SPATIAL_CLASH_PUSHDOWN]/[FUSION_PLANNER_COST])            |
|   [4]   | `Provenance`/`LineageCdc`      | provenance#CAUSAL_DAG + #LINEAGE_CDC               | lineage slice cost over a 10^7-edge DAG + redaction-preserving chain verify against live PG18 pgaudit ([LINEAGE_SLICE_COST]/[REDACTION_PRESERVING_VERIFY])                                                 |
|   [5]   | `Bcf`/`CdeSync`                | annotation#BCF_PROTOCOL + #CDE_SYNC                | BCF 2.1/3.0 XML member shapes against the buildingSMART schema + BCF-API OAuth2 PKCE flow against a live CDE ([BCF_SCHEMA_MEMBERS]/[BCF_API_AUTH_FLOW])                                                    |
|   [6]   | `Catalog`/`CostRollup`         | catalog-cost#CLASSIFICATION_CATALOG + #COST_ROLLUP | published Uniclass/OmniClass/MasterFormat ltree load + DuckDB `GROUP BY ROLLUP` formula pushdown ([CLASSIFICATION_LTREE_LOAD]/[COST_FORMULA_PUSHDOWN])                                                     |
|   [7]   | `ScheduleImport`               | schedule-interchange#SCHEDULE_STORE                | P6 XER `%T`/`%F`/`%R` table grammar + MS-Project XML element shape against real exports ([XER_TABLE_GRAMMAR]/[MSPROJECT_XML_SHAPE])                                                                        |
|   [8]   | `CrsReconcile`                 | data-lanes#GEO_LANES                               | `ST_Transform` datum-shift accuracy + IFC `IfcMapConversion` member shapes + survey-point similarity-fit residual against live PG18 + postgis ([CRS_RECONCILE])                                            |
|   [9]   | `StandingQueries`/`ArrowPlane` | query-rail#STANDING_QUERY + #ARROW_PLANE           | IVM signed-delta sliding-window maintenance + watermark late-arrival retraction + DuckDB vector-chunk zero-copy carrier lifetime ([STANDING_QUERY_IVM]/[ARROW_ZERO_COPY])                                  |

## [6]-[OBJECT_AUTHORSHIP_SIGNING]

`Authz` (object-level ACL is fence-complete; the signed-authorship key is the SPIKE on DENSITY_BAR axis [39]). The signing-key seam reads an OS credential store and stays tier-3 — proven by tier-1 decompile member-shape only because an unattended keychain/DPAPI read prompts the operator.

| [INDEX] | [OWNER]            | [PAGE#CLUSTER]              | [EXIT]                                                                                                                                    |
| :-----: | ------------------ | --------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `SignedAuthorship` | schema-rail#IDENTITY_POLICY | AppHost-resolved signing-key handle + op-digest signature + public-key verify confirmed at the integrated host ([AUTHORSHIP_SIGNING_KEY]) |

## [7]-[LIVE_OBJECT_STORE_GC]

The object-store reachability garbage collector — the owner is fence-complete (`redaction-retention#RETENTION_SWEEPS` SPIKE on the DENSITY_BAR axis [20]); it gates on a live object-store residence listing crossed against a live sync `Closure` membership union, a cross-process integration the single-RID console probe cannot stand up unattended.

| [INDEX] | [OWNER]     | [PAGE#CLUSTER]                       | [EXIT]                                                                                                                                                                                                                                |
| :-----: | ----------- | ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ClosureGc` | redaction-retention#RETENTION_SWEEPS | `Collect` reachable-set-versus-residence eviction confirmed against a live object-store listing and a live `Closure` union; `LegalHold` exemption holds; eviction-delete round-trips through the residence-axis delete ([CLOSURE_GC]) |
