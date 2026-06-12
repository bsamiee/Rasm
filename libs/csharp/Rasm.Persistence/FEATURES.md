# [PERSISTENCE_FEATURES]

Feature atlas for durable state. Every concept rides StoreProfile, DataLane, StoreOp, StoreLifecycle, SnapshotCodec, CompressionPolicy, SyncTransport, RetentionPolicy, HashPolicy, and IdentityPolicy rows plus the extension matrices — a new concept is a row, never a surface. Mechanics live in `.planning/` pages.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT] | [MODALITIES] | [PAGES] |
| :-----: | --------- | ------------ | ------- |
| [1] | Offline-first embedded store with field sync | plugin, standalone | store-profiles, sync-collaboration, native-sqlite |
| [2] | Dataset curation + tabular export (CSV/parquet) | all | data-lanes, query-rail |
| [3] | Vector + full-text semantic search | service (pgvector), embedded (FTS5 + gated vector) | data-lanes, native-sqlite |
| [4] | Snapshot time-travel with receipted restore | all | snapshot-codecs, store-profiles |
| [5] | Geospatial site-context store (PostGIS lanes) | service, connected standalone | data-lanes, schema-rail |
| [6] | Embedded geo container (GeoPackage over R*Tree) | plugin, standalone | data-lanes, native-sqlite |
| [7] | Analytical lane + benchmark/model-result index | all | data-lanes, cache-indexes |
| [8] | Redaction + retention compliance engine | all | redaction-retention |
| [9] | Document store + idempotent transaction log | all | data-lanes, query-rail |
| [10] | Encrypted-at-rest store (research-gated SQLCipher row) | standalone, plugin | native-sqlite, redaction-retention |
| [11] | Object-graph sync transports (diff, delta, replication) | hub, service | sync-collaboration |

## [2]-[CAPABILITY_ROWS]

- Engines: SqliteEmbedded, SqliteMemory, PostgresServer (18.4 target), FileSnapshot, DuckDbAnalytical, BlobRemote seam — six rows, zero engine growth admitted this cycle.
- PG extensions: 20+ first-party ADOPT rows (pg_trgm, btree_gin/gist, citext, hstore, ltree, pgcrypto, unaccent, fuzzystrmatch, cube, intarray, tablefunc, amcheck, ranges, uuidv7 native); PostGIS first-class through the NetTopologySuite chain; operator-provisioned rows for preload-gated extensions; pg_cron rejected (AppHost schedule port owns scheduling).
- SQLite: verified e_sqlite3 surface (FTS5, FTS4, JSON1, R*Tree, math, snapshot API, column metadata, FK-on); loadable-extension route via EnableExtensions/LoadExtension; sqlean vendoring and sqlite-vec stay implementation-start gates with DuckDB vss as the evaluation alternative.
- DuckDB: parquet, json, icu, sqlite_scanner ATTACH over the live store; postgres_scanner rejected (parquet-export-only).
- Codecs: STJ source-gen JSON + MessagePack (Thinktecture formatters + LZ4 block compression) + GeoJSON projection; jsonb canonical on PG; ComplexProperty().ToJson() mapping; HttpDelta fallback = RFC 6902 JsonPatchDocument subordinate to the HLC op-log changefeed.
- Bulk + delta: linq2db bulk lane emitting its own changefeed + tag invalidation; MERGE/RETURNING old/new delta projection on PG 18; logical replication V4 parallel streaming rows.
- Cross-process: WAL + busy-retry + first-opener-migrates + HLC maintenance lease; single-writer storeEpoch; restore choreography fencing all writers; filesystem-locality admission guard.

## [3]-[GAPS_TRACKED]

- Conflict-receipt projection to the UI inspector; snapshot-diff projection; pgaudit classification binding; parquet schema-versioning row; presence rows for collaboration — each closes on its named page or charter ledger row.
