# [RASM_PERSISTENCE_ROADMAP]

Implementation proceeds in the charter BUILD_ORDER: start gates and open research items resolve before the clusters they unblock, every task exits against named planning-page clusters, and every exit proves through a charter PROOF_GATES row. Open research items live in each page's RESEARCH section and bind their resolved facts into the fences they gate. The charter is `.planning/README.md`; pages are transcribed, never re-designed.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                   |
| :-----: | :---------------- | :---------------------------------------- |
|   [1]   | Project graph     | solution node present, AppHost referenced |
|   [2]   | Planning corpus   | 9 of 9 pages finalized; charter complete  |
|   [3]   | Package graph     | 27 admissions restored and lock-tracked   |
|   [4]   | API catalogues    | 23 package pages maintained               |
|   [5]   | Production source | absent — transcription not started        |

## [2]-[START_GATES]

Bridge, native, and live-server probes; each gate resolves before or during its page's BUILD_ORDER task, and the named clusters hold their final shape until the gate answers. Every gate names the page RESEARCH cluster that owns its open surface.

| [INDEX] | [GATE]                                                                                                              | [PROBE]                                                                                                                | [UNBLOCKS]                                                            |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|   [1]   | vec0 live load + package-payload-versus-vendored-tarball sourcing                                                  | `vec_version()` fact under live load (native-sqlite#RESEARCH `[EXTENSION_LOADING]`); charter `bridge` gate              | native-sqlite#EXTENSION_GATES                                        |
|   [2]   | SQLCipher provider route with external dylib on osx-arm64; `SQLite3Provider_sqlcipher` keying surface              | `EncryptionGate.Sqlcipher` ceremony resolves before the row leaves `ExtensionGateState.Research` (native-sqlite#RESEARCH `[EXTENSION_LOADING]`) | native-sqlite#EXTENSION_GATES                                        |
|   [3]   | sqlean per-RID loadable vendored as build content                                                                 | per-RID native payload under the OS loader path (native-sqlite#RESEARCH `[EXTENSION_LOADING]`)                         | native-sqlite#EXTENSION_GATES                                        |
|   [4]   | hardened-runtime dlopen of unsigned extension dylibs inside the signed Rhino host                                 | `scenarios/extension-load.verify.csx` (native-sqlite#RESEARCH `[EXTENSION_LOADING]`); charter `bridge` gate            | native-sqlite#EXTENSION_GATES                                        |
|   [5]   | `DbConfig.Hardened` ordering against pooled physical opens; `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` arming path    | db_config application point versus the migration ladder (native-sqlite#RESEARCH `[DB_CONFIG_OPS]`); compile-options receipt under the bundle-line override (native-sqlite#RESEARCH `[ENGINE_FLOOR]`) | native-sqlite#MAINTENANCE_OPS · native-sqlite#COMPILE_SURFACE |
|   [6]   | two-process first-open race: losing reader attach versus the winner's `MigrateAsync` commit boundary on one WAL   | atomicity under `busy_timeout` at the `Claim` conflict boundary (store-profiles#RESEARCH `[FIRST_OPEN_RACE]`)          | store-profiles#CROSS_PROCESS_LAW                                     |
|   [7]   | uuidv7 double-generation precedence: client `Guid.CreateVersion7` versus pg column default                        | `ClientGenerated` precedence under `ValueGeneratedNever`; `UseSnakeCaseNamingConvention` compiled-model parity (schema-rail#RESEARCH `[NAMING_COMPILED_MODEL]`) | schema-rail#IDENTITY_POLICY · schema-rail#MIGRATION_LAW       |
|   [8]   | DuckDB `sqlite_scanner` ATTACH snapshot visibility under a concurrent WAL writer                                  | `ATTACH (TYPE sqlite, READ_ONLY)` page-cache coherence across the `Duplicate` lane (data-lanes#RESEARCH `[LIVE_ATTACH]`) | data-lanes#ANALYTICAL_LANE                                          |
|   [9]   | `MergeWithOutputAsync` RETURNING old/new emission on the pg provider                                              | ReturningOldNew capability column; `BulkCopyOptions` ProviderSpecific emission (query-rail#RESEARCH `[BULK_EMISSION]`) | query-rail#BULK_LANE                                                 |
|  [10]   | APFS rename durability without parent-directory fsync; migration-lock holder evidence                            | directory-entry survival on power loss; managed fsync route if absent (snapshot-codecs#RESEARCH `[RENAME_DURABILITY]`) | snapshot-codecs#SNAPSHOT_PROTOCOL                                    |
|  [11]   | live-PG18 replication probes: `publish_generated_columns`, idle-slot timeout, subscription conflict-stat columns  | live PG18 server facts (sync-collaboration#RESEARCH `[LIVE_REPLICATION]`)                                              | sync-collaboration#TRANSPORT_AXIS                                    |
|  [12]   | pgaudit category binding: `shared_preload_libraries=pgaudit` session-audit semantics against the `Categories` rows | live PG18 server facts (redaction-retention#RESEARCH `[PGAUDIT_CATEGORIES]`)                                           | redaction-retention#AUDIT_BINDING                                   |
|  [13]   | server-root deploy assets: postgresql.conf preload fragments, pg_hba fragments, role grants, server extension enablement | first server app root; provisioning is verification-only, runtime `ALTER SYSTEM` rejected                       | store-profiles#PROVISIONING_ROWS                                    |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER; the EXITS_AGAINST column names planning-page clusters and the PROOF column names the charter BUILD_ORDER proof beyond the standard static/spec gate.

| [INDEX] | [TASK]                   | [EXITS_AGAINST]                                                                                                                                            | [PROOF]                                            |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|   [1]   | `Stores/Profiles.cs`     | store-profiles#PROFILE_AXIS, store-profiles#PLACEMENT_MATRIX, store-profiles#CROSS_PROCESS_LAW (gate [6])                                                  | static + spec (profile, placement, locality)       |
|   [2]   | `Stores/Lifecycle.cs`    | store-profiles#STORE_LIFECYCLE, store-profiles#PROVISIONING_ROWS (gate [13])                                                                              | static + spec (transitions, two-process WAL)       |
|   [3]   | `Schema/SchemaRail.cs`   | schema-rail#IDENTITY_POLICY (gate [7]), schema-rail#MIGRATION_LAW (gate [7]), schema-rail#GENERATED_COLUMNS, schema-rail#EXTENSION_DDL, schema-rail#CONVERTER_RAIL | static + spec (gates, EF script/optimize)   |
|   [4]   | `Lanes/DataLanes.cs`     | data-lanes#LANE_AXIS, data-lanes#DOCUMENT_LANE, data-lanes#SEARCH_LANES, data-lanes#GEO_LANES, data-lanes#ANALYTICAL_LANE (gate [8])                       | static + spec (lane admission and attach probe)    |
|   [5]   | `Native/Sqlite.cs`       | native-sqlite#PRAGMA_TABLE, native-sqlite#COMPILE_SURFACE (gate [5]), native-sqlite#MAINTENANCE_OPS (gate [5]), native-sqlite#EXTENSION_GATES (gates [1]-[4]) | static + spec + bridge (compile probe, extension-load scenario) |
|   [6]   | `Query/QueryRail.cs`     | query-rail#OPERATION_ALGEBRA, query-rail#PROJECTION_SHAPES, query-rail#BULK_LANE (gate [9]), query-rail#INTERCEPTOR_SPINE                                  | static + spec (dispatch and fault conversion)      |
|   [7]   | `Cache/Indexes.cs`       | cache-indexes#L2_CONTRIBUTION, cache-indexes#MODEL_RESULT_INDEX, cache-indexes#ARTIFACT_BLOB_INDEX, cache-indexes#BENCHMARK_INDEX                          | static + spec (paired closure with [8])            |
|   [8]   | `Snapshots/Codecs.cs`    | snapshot-codecs#CODEC_AXIS, snapshot-codecs#COMPRESSION_HASHING, snapshot-codecs#SNAPSHOT_PROTOCOL (gate [10]), snapshot-codecs#RESTORE_AND_DIFF           | static + spec (round-trip, header, restore)        |
|   [9]   | `Sync/Collaboration.cs`  | sync-collaboration#OPLOG_CHANGEFEED, sync-collaboration#MERGE_LAW, sync-collaboration#TRANSPORT_AXIS (gate [11]), sync-collaboration#PRESENCE_AND_BLOB     | static + spec (merge idempotency, adjudication)    |
|  [10]   | `Retention/Redaction.cs` | redaction-retention#CLASSIFICATION_ENFORCEMENT, redaction-retention#RETENTION_SWEEPS, redaction-retention#EXPORT_PROOF, redaction-retention#AUDIT_BINDING (gate [12]) | static + spec (sweep, guard, audit binding) |

Seam ordering per the charter BUILD_ORDER: [2] precedes [3] so `StoreOpenReceipt.SchemaFingerprint` stays a bare `ulong` ledger seam until `SchemaFingerprint` owns it; [7] precedes [8] as one build closure across the shared `PersistenceWireContext`; `StoreProfile` and `StorePlacement` land together in [1].

## [4]-[TESTING_APPROACH]

Universal rails share the legend in the package ROADMAP corpus (owner + resolved member identical across the four packages); versions live in `Directory.Packages.props` `ItemGroup Label="Test Stack"`.

Universal-rail concept differentiator:

| [RAIL]                  | [CONCEPT PROVEN]                                                                                                                                  |
| :---------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law    | schema-verdict six-case fold, migration-wave classification table, identity-row axis, store-op `Switch` shaping, op-log merge-receipt conservation; `EnsureCreated` ephemeral test row |
| CsCheck PBT             | durability convergence law (join-semilattice over `(stamp, origin)` — any partition of any permutation applied any number of times converges to identical state), keyset-cursor monotonicity, bulk-write round-trip, single-writer / multi-reader WAL contention |
| coverlet.MTP coverage   | managed reachability of fold / gate / adjudication surfaces; native SQLite / DuckDB engine path classified out                                    |
| dotnet-stryker mutation | killing oracle over migration physical-class arms, LWW guard `(excluded.stamp, excluded.origin) > (stamp, origin)`, seal / restore rejection-ladder tiers, retention-sweep verdict union |
| Verify.XunitV3 snapshot | MessagePack / sealed-header artifact projection (fixed-offset fields) + generated-migration SQL / contract-resolver manifest text; scrub epoch / timestamps |
| ArchUnitNET architecture | provider-invariance (interior never branches on provider), one-op-family reachability (no repository-per-aggregate), converter-admission monopoly |

Package-specific rails:

| [RAIL]                                  | [OWNER]                  | [CONCEPT PROVEN]                                                                                  | [RESOLVED MEMBER / TOKEN]                                                                              |
| :-------------------------------------- | :----------------------- | :----------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------- |
| BenchmarkDotNet                         | `specialized-rails.md [2]` | bulk-write lanes (set-based / `BulkCopyAsync` / merge), codec encode / decode size scaling, seal / restore pass, DuckDB analytical-lane round-trip durable-throughput | `ManualConfig`, `MemoryDiagnoser`, `BenchmarkSwitcher`; rail `tests/csharp/_benchmarks`              |
| SharpFuzz                               | `specialized-rails.md [3]` | snapshot-codec decode (`MessagePackSerializer.Deserialize` under `WithSecurity(UntrustedData)`) + sealed-artifact rejection ladder (raw-byte gates) — the restore lane reads untrusted data across a rest boundary | `Fuzzer.OutOfProcess.Run(Action<Stream>)`; rail `tests/csharp/_fuzz`. `SharpFuzz` `NEEDS-ADMISSION` |
| host/runtime scenarios (SqliteMemory / DuckDB) | `durability.md`   | live store, WAL sidecar, cross-process `data_version` probe; `SqliteMemory` is the deterministic in-memory store row; the DuckDB analytical lane is the live-engine round-trip scenario | `StoreProfile.SqliteMemory` placement; DuckDB analytical lane (scenario rail)                         |

## [5]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed, every PROOF_GATES row is green, the GAP_LEDGER stays fully CLOSED, and the charter `spec` gate passes on the full suite.
