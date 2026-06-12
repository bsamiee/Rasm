# [RASM_PERSISTENCE_ROADMAP]

`Rasm.Persistence` implementation transcribes nine finalized planning pages in the
charter [BUILD_ORDER](.planning/README.md). Every task exits against named page
clusters and proves through the charter [PROOF_GATES](.planning/README.md);
nothing below re-designs a finalized page.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                     |
| :-----: | :---------------- | :------------------------------------------ |
|   [1]   | Project graph     | solution node present, AppHost referenced   |
|   [2]   | Planning corpus   | 9 of 9 pages finalized; charter complete    |
|   [3]   | Package graph     | 27 admissions restored and lock-tracked     |
|   [4]   | API catalogues    | 23 package pages maintained                 |
|   [5]   | Production source | absent — transcription not started          |

## [2]-[IMPLEMENTATION_TASKS]

Tasks run in charter BUILD_ORDER; the EXIT cell names the clusters the task transcribes; PROOF names the gate that flips the task done.

| [INDEX] | [TASK]                          | [EXITS_AGAINST]                                                                                  | [PROOF]                                       |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------------- | :--------------------------------------------- |
|   [1]   | Store profile axis + placement  | store-profiles#PROFILE_AXIS · #PLACEMENT_MATRIX · #CROSS_PROCESS_LAW                              | static build; profile/placement/locality specs |
|   [2]   | Lifecycle ceremony + provisioning | store-profiles#STORE_LIFECYCLE · #PROVISIONING_ROWS                                              | transition-law specs; open-receipt facts       |
|   [3]   | Schema rail                     | schema-rail#IDENTITY_POLICY · #MIGRATION_LAW · #GENERATED_COLUMNS · #EXTENSION_DDL · #CONVERTER_RAIL | migration-gate specs; `dotnet ef` probes     |
|   [4]   | Data lanes                      | data-lanes#LANE_AXIS · #DOCUMENT_LANE · #SEARCH_LANES · #GEO_LANES · #ANALYTICAL_LANE             | lane-admission specs                           |
|   [5]   | Native SQLite floor             | native-sqlite#PRAGMA_TABLE · #COMPILE_SURFACE · #MAINTENANCE_OPS · #EXTENSION_GATES               | compile-surface probe spec; bridge scenario    |
|   [6]   | Query rail                      | query-rail#OPERATION_ALGEBRA · #PROJECTION_SHAPES · #BULK_LANE · #INTERCEPTOR_SPINE               | dispatch-totality + fault-conversion specs     |
|   [7]   | Cache contribution + indexes    | cache-indexes#L2_CONTRIBUTION · #MODEL_RESULT_INDEX · #ARTIFACT_BLOB_INDEX · #BENCHMARK_INDEX     | paired closure gate with task [8]              |
|   [8]   | Snapshot codecs + protocol      | snapshot-codecs#CODEC_AXIS · #COMPRESSION_HASHING · #SNAPSHOT_PROTOCOL · #RESTORE_AND_DIFF        | round-trip, header, restore specs              |
|   [9]   | Sync + collaboration            | sync-collaboration#OPLOG_CHANGEFEED · #MERGE_LAW · #TRANSPORT_AXIS · #PRESENCE_AND_BLOB           | merge-law idempotency + adjudication specs     |
|  [10]   | Redaction + retention           | redaction-retention#CLASSIFICATION_ENFORCEMENT · #RETENTION_SWEEPS · #EXPORT_PROOF · #AUDIT_BINDING | sweep-fold, guard, audit-binding specs       |

TS_PROJECTION clusters on snapshot-codecs and sync-collaboration land in the TS workspace under the suite wire law, outside this task table.

## [3]-[IMPLEMENTATION_START_GATES]

Research-row resolution probes and bridge-proofed spikes; each gate unblocks the named clusters before or during its BUILD_ORDER task.

| [INDEX] | [GATE]                                                                  | [PROOF_ROUTE]                                                              | [UNBLOCKS]                                            |
| :-----: | :----------------------------------------------------------------------- | :--------------------------------------------------------------------------- | :----------------------------------------------------- |
|   [1]   | Provider option spellings: pooled factory, seeding hooks, pg/sqlite builders | `assay api query` rows on store-profiles#RESEARCH                          | store-profiles#PROFILE_AXIS                            |
|   [2]   | Two-process WAL first-open race under racing `MigrateAsync`               | `assay test run --target Rasm.Persistence.Tests`                             | store-profiles#CROSS_PROCESS_LAW                       |
|   [3]   | Migration-lock holder evidence outside `Internal` namespaces              | `assay api query efcore RelationalDatabaseFacadeExtensions`                  | store-profiles#STORE_LIFECYCLE · schema-rail#MIGRATION_LAW |
|   [4]   | uuidv7 double-generation precedence; snake-case × compiled-model output   | `dotnet ef migrations script` / `dbcontext optimize` spike entities          | schema-rail#IDENTITY_POLICY · #MIGRATION_LAW           |
|   [5]   | sqlite_scanner WAL visibility; GeoPackage R*Tree window; ToJson parity    | named specs + `assay api query` rows on data-lanes#RESEARCH                  | data-lanes#ANALYTICAL_LANE · #GEO_LANES · #DOCUMENT_LANE |
|   [6]   | Compile-flag receipt under the central bundle pin + Batteries_V2 round-trip | `assay test run --target Rasm.Persistence.Tests`                           | native-sqlite#COMPILE_SURFACE                          |
|   [7]   | vec0 sourcing decision + live load; SQLCipher provider + external dylib   | `assay test run --target Rasm.Persistence.Tests`                             | native-sqlite#EXTENSION_GATES                          |
|   [8]   | Hardened-runtime dlopen of unsigned extension dylibs inside the signed Rhino host | `assay bridge verify scenarios/extension-load.verify.csx`             | native-sqlite#EXTENSION_GATES                          |
|   [9]   | EF rail spellings: execution strategy, named filters, interceptor payloads | `assay api query` rows on query-rail#RESEARCH                               | query-rail#OPERATION_ALGEBRA · #PROJECTION_SHAPES · #INTERCEPTOR_SPINE |
|  [10]   | `MergeWithOutput` RETURNING old/new + `BulkCopyOptions` emission          | `assay api query linq2db MergeWithOutput`                                    | query-rail#BULK_LANE                                   |
|  [11]   | Hybrid cache contract spellings; buffer-cache zero-copy route             | `assay api query microsoft.extensions.caching.hybrid IDistributedCache`      | cache-indexes#L2_CONTRIBUTION                          |
|  [12]   | APFS rename durability; resolver coverage; GeoJSON factory precedence     | `assay test run --target Rasm.Persistence.Tests`                             | snapshot-codecs#SNAPSHOT_PROTOCOL · #CODEC_AXIS        |
|  [13]   | PgOutput message hierarchy; live PG18 replication + conflict-stat probes  | `assay api query npgsql LogicalReplicationConnection` + live-server spec     | sync-collaboration#TRANSPORT_AXIS                      |
|  [14]   | pgaudit category semantics under preload; `Redactor` span overloads       | live `pg_settings` capture spec + `assay api query compliance.redaction`     | redaction-retention#AUDIT_BINDING · #EXPORT_PROOF      |
|  [15]   | Deploy assets: postgresql.conf + pg_hba fragments, role grants            | physical asset rows at the first headless/web app root                       | store-profiles#PROVISIONING_ROWS                       |
