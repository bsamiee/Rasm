# [RASM_PERSISTENCE_ROADMAP]

Implementation proceeds in the charter BUILD_ORDER: start gates resolve before or during the clusters they unblock, every task exits against named planning-page clusters, and every exit proves through a charter PROOF_GATES row. The corpus is design-finalized; this roadmap carries the implementation sequence and its open start gates, never a restatement of settled design. Owner realization state lives on the charter DENSITY_BAR `[STATE]` column; this roadmap routes to it.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                                                        |
| :-----: | :---------------- | :----------------------------------------------------------------------------- |
|   [1]   | Project graph     | solution node present, AppHost referenced                                      |
|   [2]   | Planning corpus   | 11 pages authored (9 finalized + remote-stores, server-tier); charter complete |
|   [3]   | Package graph     | admissions restored and lock-tracked                                           |
|   [4]   | API catalogues    | admitted-package pages maintained                                              |
|   [5]   | Production source | absent — transcription not started                                             |

## [2]-[START_GATES]

Each gate resolves before or during its task's BUILD_ORDER position; the named cluster holds its final shape until the gate answers. The PROBE column names the page RESEARCH cluster or charter gate that owns the open surface; the open-work rows themselves live in the package `TASKLOG.md` (native and server probes) and the suite `TASKLOG.md` (cross-folder server-root deploy).

| [INDEX] | [GATE]                                                           | [PROBE]                                                                                                          | [UNBLOCKS]                                                            |
| :-----: | :--------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | Loadable-extension sourcing and live load (vec0, sqlean)         | native-sqlite#RESEARCH `[EXTENSION_LOADING]`; charter `bridge` gate                                              | native-sqlite#EXTENSION_GATES                                         |
|   [2]   | SQLCipher provider route on osx-arm64; keying surface            | native-sqlite#RESEARCH `[EXTENSION_LOADING]`                                                                     | native-sqlite#EXTENSION_GATES                                         |
|   [3]   | Hardened-runtime dlopen inside the signed Rhino host             | native-sqlite#RESEARCH `[EXTENSION_LOADING]`; charter `bridge` gate                                              | native-sqlite#EXTENSION_GATES                                         |
|   [4]   | Two-process first-open race on one WAL at the `Claim` boundary   | store-profiles#RESEARCH `[FIRST_OPEN_RACE]`                                                                      | store-profiles#CROSS_PROCESS_LAW                                      |
|   [5]   | uuidv7 client-vs-column precedence; naming compiled-model parity | schema-rail#RESEARCH `[NAMING_COMPILED_MODEL]`                                                                   | schema-rail#IDENTITY_POLICY · #MIGRATION_LAW                          |
|   [6]   | DuckDB `sqlite_scanner` ATTACH visibility under a WAL writer     | data-lanes#RESEARCH `[LIVE_ATTACH]`                                                                              | data-lanes#ANALYTICAL_LANE                                            |
|   [7]   | `MergeWithOutputAsync` RETURNING old/new on the pg provider      | query-rail#RESEARCH `[BULK_EMISSION]`                                                                            | query-rail#BULK_LANE                                                  |
|   [8]   | APFS rename durability; migration-lock holder evidence           | snapshot-codecs#RESEARCH `[RENAME_DURABILITY]`                                                                   | snapshot-codecs#SNAPSHOT_PROTOCOL                                     |
|   [9]   | Live-PG18 replication and audit probes                           | sync-collaboration#RESEARCH `[LIVE_REPLICATION]`; redaction-retention#RESEARCH `[PGAUDIT_CATEGORIES]`            | sync-collaboration#TRANSPORT_AXIS · redaction-retention#AUDIT_BINDING |
|  [10]   | Server-root deploy assets; provisioning is verification-only     | suite `TASKLOG.md` cross-folder row (first server app root)                                                      | store-profiles#PROVISIONING_ROWS                                      |
|  [11]   | Live-PG18 server-tier provisioning + RLS + bundle apply          | server-tier#RESEARCH `[SERVER_PROVISIONING_PROBE]` · `[CLUSTER_CONFIG_PORTABILITY]` · `[MIGRATION_BUNDLE_PROBE]` | server-tier (all clusters)                                            |
|  [12]   | S3/Azure/GCS multipart-resume + member-spelling verify           | remote-stores#RESEARCH `[OBJECT_ROUNDTRIP]` · `[OBJECT_MEMBER_SPELLINGS]` · `[REMOTE_FAULT_LIFT]`                | remote-stores (all clusters)                                          |
|  [13]   | StackExchange.Redis L2 residence roundtrip                       | cache-indexes#L2_CONTRIBUTION residence axis                                                                     | cache-indexes#L2_CONTRIBUTION                                         |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER. The EXITS_AGAINST column names the planning-page clusters each task closes; the PROOF column names the charter BUILD_ORDER proof beyond the standard static and spec gate.

| [INDEX] | [TASK]                   | [EXITS_AGAINST]                                                                                                        | [PROOF]                                                         |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------- |
|   [1]   | `Stores/Profiles.cs`     | store-profiles#PROFILE_AXIS, #PLACEMENT_MATRIX, #CROSS_PROCESS_LAW (gate [4])                                          | static + spec (profile, placement, locality)                    |
|   [2]   | `Stores/Lifecycle.cs`    | store-profiles#STORE_LIFECYCLE, #PROVISIONING_ROWS (gate [10])                                                         | static + spec (transitions, two-process WAL)                    |
|   [3]   | `Schema/SchemaRail.cs`   | schema-rail#IDENTITY_POLICY (gate [5]), #MIGRATION_LAW (gate [5]), #GENERATED_COLUMNS, #EXTENSION_DDL, #CONVERTER_RAIL | static + spec (gates, EF script/optimize)                       |
|   [4]   | `Lanes/DataLanes.cs`     | data-lanes#LANE_AXIS, #DOCUMENT_LANE, #SEARCH_LANES, #GEO_LANES, #ANALYTICAL_LANE (gate [6])                           | static + spec (lane admission and attach probe)                 |
|   [5]   | `Native/Sqlite.cs`       | native-sqlite#PRAGMA_TABLE, #COMPILE_SURFACE, #MAINTENANCE_OPS, #EXTENSION_GATES (gates [1]-[3])                       | static + spec + bridge (compile probe, extension-load scenario) |
|   [6]   | `Query/QueryRail.cs`     | query-rail#OPERATION_ALGEBRA, #PROJECTION_SHAPES, #BULK_LANE (gate [7]), #INTERCEPTOR_SPINE                            | static + spec (dispatch and fault conversion)                   |
|   [7]   | `Cache/Indexes.cs`       | cache-indexes#L2_CONTRIBUTION, #MODEL_RESULT_INDEX, #ARTIFACT_BLOB_INDEX, #BENCHMARK_INDEX                             | static + spec (paired closure with [8])                         |
|   [8]   | `Snapshots/Codecs.cs`    | snapshot-codecs#CODEC_AXIS, #COMPRESSION_HASHING, #SNAPSHOT_PROTOCOL (gate [8]), #RESTORE_AND_DIFF                     | static + spec (round-trip, header, restore)                     |
|   [9]   | `Sync/Collaboration.cs`  | sync-collaboration#OPLOG_CHANGEFEED, #MERGE_LAW, #TRANSPORT_AXIS (gate [9]), #PRESENCE_AND_BLOB                        | static + spec (merge idempotency, adjudication)                 |
|  [10]   | `Retention/Redaction.cs` | redaction-retention#CLASSIFICATION_ENFORCEMENT, #RETENTION_SWEEPS, #EXPORT_PROOF, #AUDIT_BINDING (gate [9])            | static + spec (sweep, guard, audit binding)                     |
|  [11]   | `Stores/ServerTier.cs`   | server-tier#TIMESCALE_PROVISIONING, #SEARCH_PROVISIONING, #CLUSTER_CONFIG, #TENANCY_RLS, #MIGRATION_BUNDLE (gate [11]) | static + spec + live-PG18 provisioning roundtrip                |
|  [12]   | `Stores/RemoteStores.cs` | remote-stores#OBJECT_STORE, #MULTIPART_TRANSFER, #OBJECT_RESIDENCE, #ARTIFACT_SYNC_FEED (gate [12])                    | static + spec + object-store emulator roundtrip                 |

Seam ordering follows the charter BUILD_ORDER: [2] precedes [3] so `StoreOpenReceipt.SchemaFingerprint` stays a bare `ulong` ledger seam until `SchemaFingerprint` owns it; [7] precedes [8] as one build closure across the shared `PersistenceWireContext`; `StoreProfile` and `StorePlacement` land together in [1]; `Stores/ServerTier.cs` follows [2] and [3] (consumes `ExtensionRequirement` + `SchemaDdl` settled); `Stores/RemoteStores.cs` follows [1] and the snapshot/sync closure (consumes `BlobRemote` + ArtifactSync frame constants + op-log settled).

## [4]-[TESTING_APPROACH]

Universal rails share the legend in the package ROADMAP corpus — owner and resolved member identical across the four packages. Versions live in `Directory.Packages.props`.

Universal-rail concept differentiator:

| [RAIL]                   | [CONCEPT PROVEN]                                                                                                                                                                         |
| :----------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law     | schema-verdict six-case fold, migration-wave classification table, identity-row axis, store-op `Switch` shaping, op-log merge-receipt conservation; `EnsureCreated` ephemeral test row   |
| CsCheck PBT              | durability convergence law (join-semilattice over `(stamp, origin)`), keyset-cursor monotonicity, bulk-write round-trip, single-writer / multi-reader WAL contention                     |
| coverlet.MTP coverage    | managed reachability of fold / gate / adjudication surfaces; native SQLite / DuckDB engine path classified out                                                                           |
| dotnet-stryker mutation  | killing oracle over migration physical-class arms, LWW guard `(excluded.stamp, excluded.origin) > (stamp, origin)`, seal / restore rejection-ladder tiers, retention-sweep verdict union |
| Verify.XunitV3 snapshot  | MessagePack / sealed-header artifact projection (fixed-offset fields) + generated-migration SQL / contract-resolver manifest text; scrub epoch / timestamps                              |
| ArchUnitNET architecture | provider-invariance (interior never branches on provider), one-op-family reachability (no repository-per-aggregate), converter-admission monopoly                                        |

Package-specific rails:

| [RAIL]                                         | [OWNER]                          | [CONCEPT PROVEN]                                                                                                                  | [RESOLVED MEMBER / TOKEN]                                                               |
| :--------------------------------------------- | :------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------- |
| BenchmarkDotNet                                | data-lanes#ANALYTICAL_LANE       | bulk-write lanes, codec encode / decode size scaling, seal / restore pass, DuckDB analytical-lane round-trip durable-throughput   | `ManualConfig`, `MemoryDiagnoser`, `BenchmarkSwitcher`; rail `tests/csharp/_benchmarks` |
| SharpFuzz                                      | snapshot-codecs#RESTORE_AND_DIFF | snapshot-codec decode under `WithSecurity(UntrustedData)` + sealed-artifact rejection ladder across the restore rest boundary     | `Fuzzer.OutOfProcess.Run(Action<Stream>)`; rail `tests/csharp/_fuzz`                    |
| host/runtime scenarios (SqliteMemory / DuckDB) | store-profiles#PLACEMENT_MATRIX  | live store, WAL sidecar, cross-process `data_version` probe; deterministic in-memory store row; live-engine analytical round-trip | `StoreProfile.SqliteMemory` placement; DuckDB analytical lane (scenario rail)           |

## [5]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed, every PROOF_GATES row is green, the charter GAP_LEDGER stays fully CLOSED, and the charter `spec` gate passes on the full suite. Residual host-bridge work is the charter DENSITY_BAR `SPIKE` rows resolving against their page RESEARCH clusters — the native loadable-extension and encryption gates (native-sqlite#RESEARCH `[EXTENSION_LOADING]`), the cross-process and provisioning probes (store-profiles#RESEARCH `[FIRST_OPEN_RACE]`), the live-PG18 replication and audit probes (sync-collaboration#RESEARCH `[LIVE_REPLICATION]`, redaction-retention#RESEARCH `[PGAUDIT_CATEGORIES]`), the snapshot rename-durability probe (snapshot-codecs#RESEARCH `[RENAME_DURABILITY]`), the live-PG18 server-tier provisioning probes (server-tier#RESEARCH `[SERVER_PROVISIONING_PROBE]`, `[CLUSTER_CONFIG_PORTABILITY]`, `[MIGRATION_BUNDLE_PROBE]`), the object-store emulator roundtrip and admission-decompile probes (remote-stores#RESEARCH `[OBJECT_ROUNDTRIP]`, `[OBJECT_MEMBER_SPELLINGS]`, `[REMOTE_FAULT_LIFT]`), and the Redis L2 residence roundtrip (cache-indexes#L2_CONTRIBUTION). The open rows for each spike live in the package `TASKLOG.md`; the cross-folder server-root deploy spike lives in the suite `TASKLOG.md`.
