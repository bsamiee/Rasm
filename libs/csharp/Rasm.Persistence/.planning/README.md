# [PERSISTENCE_PLANNING]

Rasm.Persistence has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns durable state: store profiles, data lanes, schema and query rails, native SQLite truth, snapshot codecs, cache indexes, sync and collaboration transports, and redaction/retention — consuming AppHost ports (clock, telemetry, receipts, drain, classification) as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] | [STATE] |
| :-----: | ------ | ------ | :-----: |
| [1] | [store-profiles](store-profiles.md) | Six-row engine axis; widened row record; cross-process and provisioning law | planned |
| [2] | [data-lanes](data-lanes.md) | Seven-lane capability map incl. geometry lanes and extension capabilities | planned |
| [3] | [schema-rail](schema-rail.md) | Identity policy, migrations, generated columns, extension declarations | planned |
| [4] | [query-rail](query-rail.md) | StoreOp dispatch, pooled contexts, bulk lane, delta projection, interceptors | planned |
| [5] | [native-sqlite](native-sqlite.md) | Verified e_sqlite3 surface, WAL law, loadable extensions, encryption gate | planned |
| [6] | [snapshot-codecs](snapshot-codecs.md) | Codec axis, compression, hashing, restore choreography, wire contracts | planned |
| [7] | [cache-indexes](cache-indexes.md) | L2 cache contribution, serializer rows, model-result and artifact indexes | planned |
| [8] | [sync-collaboration](sync-collaboration.md) | SyncTransport axis, op-log changefeed, diff rows, presence, conflict receipts | planned |
| [9] | [redaction-retention](redaction-retention.md) | Retention policies, classification enforcement, audit binding | planned |

## [2]-[WIRE_PAGES]

snapshot-codecs · sync-collaboration (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

linq2db core (LinqToDB.dll) merge/output surfaces ride the bridge package and get a page only if the core package is admitted; app-root JsonPatch catalogued at app-root creation.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP] | [CLOSED_BY] | [STATE] |
| :-----: | ----- | ----------- | :-----: |
| [1] | Execution-strategy rows (EnableRetryOnFailure / busy-retry); database retry excluded from the hop law | store-profiles + query-rail | OPEN |
| [2] | PooledDbContextFactory pooling in the query rail | query-rail | OPEN |
| [3] | Optimistic-concurrency tokens (pg xmin, sqlite version column) to typed store fault | schema-rail + query-rail | OPEN |
| [4] | UseSeeding/UseAsyncSeeding reference-data law | store-profiles | OPEN |
| [5] | PG maintenance/backup symmetry rows (ANALYZE, REINDEX, autovacuum posture) | store-profiles | OPEN |
| [6] | Receipted restore choreography fencing all writers, deleting -wal/-shm | snapshot-codecs + store-profiles | OPEN |
| [7] | Cross-process law: WAL + busy-retry + first-opener-migrates + HLC maintenance lease; lease-handoff distinct from crash-reclaim with staleness timeout | store-profiles + native-sqlite | OPEN |
| [8] | Clock-seam injection for all TTL/retention/HLC stamping | redaction-retention + sync-collaboration | OPEN |
| [9] | IdentityPolicy axis (UuidV7Key, ContentHash, NaturalKey); sqlite uuidv7 via CreateFunction | schema-rail | OPEN |
| [10] | Bulk-path invalidation: linq2db lane emits its own changefeed + tag invalidation | query-rail | OPEN |
| [11] | DataClassification consumed from AppHost; store-side enforcement rows only | redaction-retention | OPEN |
| [12] | PG18 adoption rows: uuidv7, virtual generated columns (STORED when replicated/indexed), replication policy fields, MERGE/RETURNING delta | schema-rail + query-rail + sync-collaboration | OPEN |
| [13] | Extensions axis: first-party ADOPT rows, PostGIS lanes via NTS chain, operator-provisioned rows, named REJECT rows | data-lanes + schema-rail + store-profiles | OPEN |
| [14] | Verified e_sqlite3 compile-flag table + loadable-extension law; sqlean/sqlite-vec gates with DuckDB vss alternative | native-sqlite | OPEN |
| [15] | M1-M5 sync rows: HasObjects diff, __closure manifest field, TransportBridge ownership here, fan-in/out topology field, capture-direction field | sync-collaboration | OPEN |
| [16] | HttpDelta fallback = RFC 6902 JsonPatchDocument, subordinate to the op-log changefeed; merge-patch rejected | sync-collaboration | OPEN |
| [17] | BlobRemote consumes the ArtifactSync frame constants as settled vocabulary | sync-collaboration | OPEN |
| [18] | Presence rows (ephemeral changefeed + DropOldest) | sync-collaboration | OPEN |
| [19] | Conflict-receipt projection consumed by the UI inspector | sync-collaboration | OPEN |
| [20] | Parquet schema-versioning policy row; DuckDB ATTACH sqlite_scanner row; postgres_scanner rejected | data-lanes | OPEN |
| [21] | jsonb canonical law, ComplexProperty().ToJson(), jsonpath surface; OwnsOne().ToJson() rejected | schema-rail + query-rail | OPEN |
| [22] | GeoJSON4STJ wire projection + GeoPackage container rows; one canonical wire geometry (proto), NTS as PG boundary | data-lanes + snapshot-codecs | OPEN |
| [23] | Snapshot-diff projection between two snapshots | snapshot-codecs | OPEN |
| [24] | EP-context + profiling artifact blob routing (Compute consumes) | cache-indexes | OPEN |
| [25] | Filesystem-locality admission guard (WAL needs local volume; network homes typed-rejected) | store-profiles | OPEN |
| [26] | SQLCipher encryption row research-gated on provider + external dylib spike | native-sqlite | OPEN |
| [27] | pgaudit classification binding research row; LegalHold-vs-sweep ordering | redaction-retention | OPEN |
| [28] | Thinktecture EF value converters + MessagePack formatters as codec rows | schema-rail + snapshot-codecs | OPEN |

Sections [DENSITY_BAR], [BUILD_ORDER], [FILE_PROCESS], [PROOF_GATES], [PROHIBITIONS], [ADMISSIONS_RECORD] complete after page finalization.
