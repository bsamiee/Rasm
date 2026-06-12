# [PERSISTENCE_FEATURES]

Feature atlas for durable state. Every concept rides StoreProfile, DataLane, StoreOp, StoreLifecycle, SnapshotCodec, CompressionPolicy, SyncTransport, RetentionPolicy, HashPolicy, and IdentityPolicy rows plus the extension matrices — a new concept is a row, never a surface. Mechanics live in the finalized `.planning/` pages; anchors below name the owning clusters.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT]                                                     | [MODALITIES]                                       | [PAGES]                                                                                               |
| :-----: | ------------------------------------------------------------- | -------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
|   [1]   | Offline-first embedded store with field sync                  | plugin, standalone                                 | store-profiles#PROFILE_AXIS · native-sqlite#PRAGMA_TABLE · sync-collaboration#OPLOG_CHANGEFEED        |
|   [2]   | Dataset curation + tabular export (CSV/parquet)               | all                                                | data-lanes#ANALYTICAL_LANE · query-rail#PROJECTION_SHAPES                                             |
|   [3]   | Vector + full-text semantic search                            | service (pgvector), embedded (FTS5 + gated vector) | data-lanes#SEARCH_LANES · native-sqlite#EXTENSION_GATES                                               |
|   [4]   | Snapshot time-travel with receipted restore                   | all                                                | snapshot-codecs#SNAPSHOT_PROTOCOL · snapshot-codecs#RESTORE_AND_DIFF · store-profiles#STORE_LIFECYCLE |
|   [5]   | Geospatial site-context store (PostGIS lanes)                 | service, connected standalone                      | data-lanes#GEO_LANES · schema-rail#EXTENSION_DDL                                                      |
|   [6]   | Embedded geo container (GeoPackage over R*Tree)               | plugin, standalone                                 | data-lanes#GEO_LANES · native-sqlite#COMPILE_SURFACE                                                  |
|   [7]   | Analytical lane + benchmark/model-result index                | all                                                | data-lanes#ANALYTICAL_LANE · cache-indexes#BENCHMARK_INDEX · cache-indexes#MODEL_RESULT_INDEX         |
|   [8]   | Redaction + retention compliance engine                       | all                                                | redaction-retention#CLASSIFICATION_ENFORCEMENT · redaction-retention#RETENTION_SWEEPS                 |
|   [9]   | Document store + idempotent transaction log                   | all                                                | data-lanes#DOCUMENT_LANE · query-rail#OPERATION_ALGEBRA                                               |
|  [10]   | Encrypted-at-rest store (research-gated SQLCipher row)        | standalone, plugin                                 | native-sqlite#EXTENSION_GATES · redaction-retention#CLASSIFICATION_ENFORCEMENT                        |
|  [11]   | Object-graph sync transports (diff, delta, replication)       | hub, service                                       | sync-collaboration#TRANSPORT_AXIS · sync-collaboration#MERGE_LAW                                      |
|  [12]   | L2 cache contribution behind the AppHost port                 | all                                                | cache-indexes#L2_CONTRIBUTION · snapshot-codecs#CODEC_AXIS                                            |
|  [13]   | Receipted open/restore ceremony + cross-process lease law     | plugin, standalone                                 | store-profiles#STORE_LIFECYCLE · store-profiles#CROSS_PROCESS_LAW                                     |
|  [14]   | Operational fact stream + store observability                 | all                                                | query-rail#INTERCEPTOR_SPINE · native-sqlite#MAINTENANCE_OPS                                          |
|  [15]   | Presence + offline-queue collaboration                        | hub, standalone                                    | sync-collaboration#PRESENCE_AND_BLOB                                                                  |
|  [16]   | Bulk movement with self-emitted changefeed + delta projection | all                                                | query-rail#BULK_LANE · sync-collaboration#OPLOG_CHANGEFEED                                            |
|  [17]   | Support-bundle store evidence with hash-proved export         | all                                                | redaction-retention#EXPORT_PROOF · redaction-retention#AUDIT_BINDING                                  |
|  [18]   | Dashboard wire surface (snapshot + sync TS contracts)         | web                                                | snapshot-codecs#TS_PROJECTION · sync-collaboration#TS_PROJECTION                                      |
