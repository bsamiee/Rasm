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

## [2]-[CAPABILITY_ROWS]

- Engines: SqliteEmbedded, SqliteMemory, PostgresServer (PG 18 dialect pin), FileSnapshot, DuckDbAnalytical, BlobRemote seam — six rows, zero engine growth admitted this cycle (store-profiles#PROFILE_AXIS).
- PG extensions: 19 declared `SchemaDdl.Extensions` rows (pg_trgm, btree_gin/gist, citext, hstore, ltree, pgcrypto, unaccent, fuzzystrmatch, cube, intarray, tablefunc, amcheck, pg_prewarm, pg_visibility, pg_logicalinspect, postgres_fdw, vector, postgis); PostGIS first-class through the NetTopologySuite chain; built-in ranges/multiranges and native uuidv7 ride with zero extension entry; seven operator-provisioned preload rows; pg_cron rejected — the AppHost schedule port owns cadence (schema-rail#EXTENSION_DDL · store-profiles#PROVISIONING_ROWS).
- SQLite: verified e_sqlite3 surface (FTS5, FTS4, JSON1, R*Tree, math, snapshot API, column metadata, FK-on); loadable-extension route via EnableExtensions/LoadExtension; vec0 gated with brute-force fallback, sqlean deferred, SQLCipher research-gated with DuckDB vss as the evaluation alternative (native-sqlite#COMPILE_SURFACE · native-sqlite#EXTENSION_GATES).
- DuckDB: parquet, json, icu, sqlite_scanner ATTACH over the live store; postgres_scanner rejected — the server boundary is parquet-export-only (data-lanes#ANALYTICAL_LANE).
- Codecs: STJ source-gen JSON + MessagePack (Thinktecture formatters + LZ4 block compression) + GeoJSON projection; jsonb canonical on PG; ComplexProperty().ToJson() mapping; HttpDelta fallback = RFC 6902 JsonPatchDocument subordinate to the HLC op-log changefeed (snapshot-codecs#CODEC_AXIS · data-lanes#DOCUMENT_LANE · sync-collaboration#TRANSPORT_AXIS).
- Bulk + delta: linq2db bulk lane emitting its own changefeed + tag invalidation; MERGE/RETURNING old/new delta projection behind the ReturningOldNew capability column; pgoutput V4 parallel-streaming replication rows (query-rail#BULK_LANE · sync-collaboration#TRANSPORT_AXIS).
- Cross-process: WAL + busy-retry + first-opener-migrates + HLC maintenance lease; single-writer epoch fencing; restore choreography fencing all writers and deleting -wal/-shm sidecars; filesystem-locality admission guard (store-profiles#CROSS_PROCESS_LAW · store-profiles#STORE_LIFECYCLE).
- Identity: uuid-v7 default via Guid.CreateVersion7 with the sqlite uuid7 function leg, content-hash XxHash128, natural-key admission (schema-rail#IDENTITY_POLICY).

## [3]-[RESEARCH_GATES]

Every former atlas gap is closed on its named cluster: conflict-receipt projection (sync-collaboration#MERGE_LAW), snapshot-diff projection (snapshot-codecs#RESTORE_AND_DIFF), parquet schema-versioning (data-lanes#ANALYTICAL_LANE), presence rows (sync-collaboration#PRESENCE_AND_BLOB), pgaudit binding (redaction-retention#AUDIT_BINDING). The remaining unknowns are the per-page RESEARCH rows, gathered as the [implementation-start gates](ROADMAP.md) table.
