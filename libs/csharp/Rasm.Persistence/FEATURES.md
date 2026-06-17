# [PERSISTENCE_FEATURES]

The realized capability list for the durable-state spine. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[ISOLATION_CONCEPTS]

The products this folder uniquely owns; each concept folds over one budgeted owner.

| [INDEX] | [FEATURE]                                                  | [PAGE#CLUSTER]                                 |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------- |
|   [1]   | Topology-as-rows store axis                                | store-profiles#PROFILE_AXIS                    |
|   [2]   | Receipted open/restore/drain ceremony                      | store-profiles#STORE_LIFECYCLE                 |
|   [3]   | Cross-process writer lease + epoch fence                   | store-profiles#CROSS_PROCESS_LAW               |
|   [4]   | Seven-lane durable shape axis                              | data-lanes#LANE_AXIS                           |
|   [5]   | Aggregate document store + per-column JSON index           | data-lanes#DOCUMENT_LANE                       |
|   [6]   | Vector + full-text search (pgvector)                       | data-lanes#SEARCH_LANES                        |
|   [7]   | Geospatial site-context lanes (PostGIS + GeoPackage)       | data-lanes#GEO_LANES                           |
|   [8]   | Analytical projection lane + HLC changefeed to parquet     | data-lanes#ANALYTICAL_LANE                     |
|   [9]   | One operation algebra + Kleisli composition                | query-rail#OPERATION_ALGEBRA                   |
|  [10]   | Keyset pagination + typed projection egress                | query-rail#PROJECTION_SHAPES                   |
|  [11]   | Bulk movement with self-emitted changefeed + delta         | query-rail#BULK_LANE                           |
|  [12]   | Four-hook interceptor spine + store observability          | query-rail#INTERCEPTOR_SPINE                   |
|  [13]   | Schema-drift gate + expand/contract migration law          | schema-rail#MIGRATION_LAW                      |
|  [14]   | Three-row identity axis (uuid-v7 / content / natural)      | schema-rail#IDENTITY_POLICY                    |
|  [15]   | Generated-converter admission + snake-case naming          | schema-rail#CONVERTER_RAIL                     |
|  [16]   | Embedded engine floor (PRAGMA, compile, maintenance)       | native-sqlite#PRAGMA_TABLE                     |
|  [17]   | Loadable-extension + encryption gate                       | native-sqlite#EXTENSION_GATES                  |
|  [18]   | Three-row sealed snapshot codec + atomic seal              | snapshot-codecs#CODEC_AXIS                     |
|  [19]   | Compression + hash policy axes                             | snapshot-codecs#COMPRESSION_HASHING            |
|  [20]   | Content-addressed snapshot identity + diff                 | snapshot-codecs#SNAPSHOT_PROTOCOL              |
|  [21]   | L2 cache contribution behind the AppHost port              | cache-indexes#L2_CONTRIBUTION                  |
|  [22]   | Model-result index (warm-start substrate selection)        | cache-indexes#MODEL_RESULT_INDEX               |
|  [23]   | Content-addressed artifact-blob catalog                    | cache-indexes#ARTIFACT_BLOB_INDEX              |
|  [24]   | Recency-gated benchmark route index                        | cache-indexes#BENCHMARK_INDEX                  |
|  [25]   | Op-log changefeed (one log: outbox, audit, feed, sync)     | sync-collaboration#OPLOG_CHANGEFEED            |
|  [26]   | LWW merge law + typed conflict receipts                    | sync-collaboration#MERGE_LAW                   |
|  [27]   | Three-transport sync axis (replication, http, diff)        | sync-collaboration#TRANSPORT_AXIS              |
|  [28]   | Object-graph sync (speckle-class diff transport)           | sync-collaboration#TRANSPORT_AXIS              |
|  [29]   | Presence + offline-queue collaboration                     | sync-collaboration#PRESENCE_AND_BLOB           |
|  [30]   | Streamed blob transfer (incremental frame hash)            | sync-collaboration#PRESENCE_AND_BLOB           |
|  [31]   | Classification enforcement at every store egress           | redaction-retention#CLASSIFICATION_ENFORCEMENT |
|  [32]   | Hold-first receipted retention sweep                       | redaction-retention#RETENTION_SWEEPS           |
|  [33]   | Support-bundle store evidence + hash-proved export         | redaction-retention#EXPORT_PROOF               |
|  [34]   | Classification to pgaudit category binding                 | redaction-retention#AUDIT_BINDING              |
|  [35]   | Operator-provisioning manifest + capability verdict        | store-profiles#PROVISIONING_ROWS               |
|  [36]   | Dashboard wire surface (snapshot + sync TS contracts)      | snapshot-codecs#TS_PROJECTION                  |
|  [37]   | Cloud object-store residence axis (S3/Azure/GCS)           | remote-stores#OBJECT_STORE                     |
|  [38]   | Chunked resumable multipart transfer + receipt             | remote-stores#MULTIPART_TRANSFER               |
|  [39]   | Content-key object residence + conditional-write           | remote-stores#OBJECT_RESIDENCE                 |
|  [40]   | Cloud object-store sync hub (content-key + op-log)         | remote-stores#ARTIFACT_SYNC_FEED               |
|  [41]   | Time-series telemetry product (hypertable + rollups)       | server-tier#TIMESCALE_PROVISIONING             |
|  [42]   | diskann + BM25 server search index provisioning            | server-tier#SEARCH_PROVISIONING                |
|  [43]   | Cluster GUC deploy fragments + read-only verify            | server-tier#CLUSTER_CONFIG                     |
|  [44]   | Multi-tenant data isolation (RLS) axis                     | server-tier#TENANCY_RLS                        |
|  [45]   | Migration-bundle service-deploy gate                       | server-tier#MIGRATION_BUNDLE                   |
|  [46]   | Tiered cache fabric (in-memory/sqlite/redis/pg)            | cache-indexes#L2_CONTRIBUTION                  |
|  [47]   | IFC semantic-ingest model-graph residence (managed)        | cache-indexes#ARTIFACT_BLOB_INDEX              |
|  [48]   | Object-store reachability GC by live Closure               | redaction-retention#RETENTION_SWEEPS           |
|  [49]   | Partial-graph checkout at a sub-root                       | sync-collaboration#TRANSPORT_AXIS              |
|  [50]   | Tenant provisioning lifecycle (create/destroy → RLS)       | server-tier#TENANCY_RLS                        |
|  [51]   | Per-tenant resource quota / rate bound                     | server-tier#TENANCY_RLS                        |
|  [52]   | Continuous-aggregate refresh push lane                     | query-rail#INTERCEPTOR_SPINE                   |
|  [53]   | Hybrid RAG retrieval (dense + sparse + BM25 RRF)           | data-lanes#SEARCH_LANES                        |
|  [54]   | Spatial op family (proximity/coverage/spatial-join)        | data-lanes#GEO_LANES                           |
|  [55]   | Spatial change-detection over the changefeed               | data-lanes#GEO_LANES                           |
|  [56]   | Spatial-analytical rollup to time-series series            | server-tier#TIMESCALE_PROVISIONING             |
|  [57]   | Content-addressed commit-DAG + named branches + merge-base | version-control#COMMIT_DAG                     |
|  [58]   | Convergent op-based CRDT object graph (RGA/OR-set/LWW)     | version-control#CRDT_ALGEBRA                   |
|  [59]   | AS-OF time-travel + range diff + blame + branch-from-past  | version-control#TIME_TRAVEL                    |
|  [60]   | Geometry-aware structural diff/merge + typed conflicts     | version-control#STRUCTURAL_DIFF                |
|  [61]   | Source-agnostic federated entity graph                     | federation#ENTITY_GRAPH                        |
|  [62]   | Element-set query algebra (universal BIM currency)         | federation#ELEMENT_SET_ALGEBRA                 |
|  [63]   | Cross-document reference resolver + transitive impact      | federation#CROSS_DOC_LINKS                     |
|  [64]   | Declarative rule engine (clash/IDS/MVD/QTO)                | federation#RULE_PLAN                           |
|  [65]   | Vector+spatial+FTS fusion-ranking with lineage             | federation#FUSION_RANK                         |
|  [66]   | Cross-engine federated query planner                       | federation#FEDERATED_PLAN                      |
|  [67]   | W3C-PROV lineage as a join dimension                       | provenance#CAUSAL_DAG                          |
|  [68]   | Tamper-evident hash-chained attested ledger                | provenance#ATTESTED_LEDGER                     |
|  [69]   | Lineage-scoped redaction-aware CDC                         | provenance#LINEAGE_CDC                         |
|  [70]   | Anchored-annotation anchor algebra (5-surface reuse)       | annotation#ANCHOR_ALGEBRA                      |
|  [71]   | BCF 2.1/3.0 coordination + viewpoint lifecycle             | annotation#BCF_PROTOCOL                        |
|  [72]   | Bidirectional CDE OAuth2 sync                              | annotation#CDE_SYNC                            |
|  [73]   | Multi-standard classification catalogs                     | catalog-cost#CLASSIFICATION_CATALOG            |
|  [74]   | Cost catalog + formula-evaluated 5D rollup                 | catalog-cost#COST_ROLLUP                       |
|  [75]   | P6/MS-Project schedule interchange + activity network      | schedule-interchange#SCHEDULE_STORE            |
|  [76]   | 4D construction state + planned/actual variance            | schedule-interchange#TASK_LINK_4D              |
|  [77]   | Continuous/incremental standing query (windows + IVM)      | query-rail#STANDING_QUERY                      |
|  [78]   | Columnar Arrow zero-copy analytics carrier                 | query-rail#ARROW_PLANE                         |
|  [79]   | Low-latency lossy awareness (cursor/selection/camera)      | sync-collaboration#PRESENCE_AND_BLOB           |
|  [80]   | Partial-replication subgraph checkout + working set        | sync-collaboration#PRESENCE_AND_BLOB           |
|  [81]   | Georeferencing/CRS reconciliation kernel                   | data-lanes#GEO_LANES                           |
|  [82]   | Object-level ACL + signed authorship                       | schema-rail#IDENTITY_POLICY                    |
|  [83]   | Schema + codec evolution registry (content-negotiation)    | snapshot-codecs#SCHEMA_EVOLUTION               |

## [2]-[CAPABILITY_EXPANSION_REGISTER]

Capabilities a downstream app hand-rolls in this domain absent the owner, each mapped to the `page#cluster` row/case/column that absorbs it.

| [INDEX] | [FEATURE]                                                      | [PAGE#CLUSTER]                      |
| :-----: | :------------------------------------------------------------- | :---------------------------------- |
|   [1]   | Live store attach snapshot coherence under one writer          | data-lanes#ANALYTICAL_LANE          |
|   [2]   | In-process managed sequence as a planner relation              | data-lanes#ANALYTICAL_LANE          |
|   [3]   | Vector-quantum chunk transfer for derived rollup staging       | data-lanes#ANALYTICAL_LANE          |
|   [4]   | Multi-statement fan-out read as one server round-trip          | query-rail#OPERATION_ALGEBRA        |
|   [5]   | Custom provider-type classification without per-type MapEnum   | store-profiles#PROFILE_AXIS         |
|   [6]   | PostgreSQL composite-type round-trip                           | schema-rail#EXTENSION_DDL           |
|   [7]   | Read-only embedded reader replica without writer contention    | store-profiles#PROFILE_AXIS         |
|   [8]   | Domain-fold aggregate registered into the embedded engine      | native-sqlite#MAINTENANCE_OPS       |
|   [9]   | Case-insensitive / locale collation in embedded queries        | native-sqlite#MAINTENANCE_OPS       |
|  [10]   | Paged hot-store backup with progress receipts                  | native-sqlite#MAINTENANCE_OPS       |
|  [11]   | Consistent multi-transaction read pin (monotone reader)        | native-sqlite#MAINTENANCE_OPS       |
|  [12]   | jsonb blob fast path beside the TEXT-json EF mapping           | native-sqlite#EXTENSION_GATES       |
|  [13]   | Per-connection defensive hardening before first statement      | native-sqlite#EXTENSION_GATES       |
|  [14]   | Streaming compression for payloads above 1 MiB                 | snapshot-codecs#COMPRESSION_HASHING |
|  [15]   | Zstandard compression swap keeping LZ4 archives readable       | snapshot-codecs#COMPRESSION_HASHING |
|  [16]   | AOT generated MessagePack resolver replacing runtime composite | snapshot-codecs#CODEC_AXIS          |
|  [17]   | Reader-to-writer fusion (no parse-materialize-rebuild)         | data-lanes#ANALYTICAL_LANE          |
|  [18]   | Parallel ordered tabular projection                            | data-lanes#ANALYTICAL_LANE          |
|  [19]   | Typed feature-attribute projection (no loose-table walking)    | data-lanes#GEO_LANES                |
|  [20]   | Constructive managed geometry refine over container candidates | data-lanes#GEO_LANES                |
|  [21]   | NodaTime range + multirange temporal columns                   | data-lanes#GEO_LANES                |
|  [22]   | Hierarchy path lane (assembly/part trees)                      | data-lanes#GEO_LANES                |
|  [23]   | DB-temporal daylight-transition policy                         | store-profiles#PROFILE_AXIS         |
|  [24]   | Idle replication-slot reclamation + conflict statistics        | sync-collaboration#TRANSPORT_AXIS   |
|  [25]   | Join-preserving HMAC redaction across artifacts                | redaction-retention#EXPORT_PROOF    |
|  [26]   | Native at-rest encryption for personal-class stores            | native-sqlite#EXTENSION_GATES       |
|  [27]   | Compiled hot-query delegates for proven hot shapes             | query-rail#PROJECTION_SHAPES        |
|  [28]   | pg_stat_statements slow-query evidence read view               | query-rail#INTERCEPTOR_SPINE        |
|  [29]   | Native PostgreSQL enum-type round-trip                         | schema-rail#EXTENSION_DDL           |
|  [30]   | Native enum/composite type-resolution open gate                | store-profiles#PROVISIONING_ROWS    |
|  [31]   | Bit-vector distance metrics (Hamming / Jaccard)                | data-lanes#SEARCH_LANES             |
|  [32]   | GeoJSON projection off the canonical wire geometry             | snapshot-codecs#CODEC_AXIS          |
|  [33]   | linq2db two-door bridge terminators + temporal aggregate       | query-rail#PROJECTION_SHAPES        |
|  [34]   | Npgsql meter histogram/cardinality posture column              | query-rail#INTERCEPTOR_SPINE        |
|  [35]   | Half/sparse/bit embedding precisions as one closed axis        | data-lanes#SEARCH_LANES             |
|  [36]   | TimescaleDB hyperfunction rollup projections (toolkit)         | query-rail#PROJECTION_SHAPES        |
|  [37]   | Resumable object-store transfer skip-on-committed-ETag         | remote-stores#MULTIPART_TRANSFER    |
|  [38]   | Per-tenant content-address cache-key partitioning              | server-tier#TENANCY_RLS             |
|  [39]   | Server-side filtered keyset re-query (no client filter)        | query-rail#PROJECTION_SHAPES        |
|  [40]   | Bulk-lane store-side backpressure shed fact                    | query-rail#BULK_LANE                |

## [3]-[FOLDER_CONCEPT_SEEDS]

Higher-order isolation concepts this folder enables, each a fold or combinator over already-budgeted owners.

- `StoreOp<T>` Kleisli combinator: `Then` sequences a producing op with a continuation through the generated total `Switch`, rewrapping into the source case so a receipt- or tag-bearing arm keeps its self-emitted invalidation — one composed op runs in one bracket inside one execution strategy. (query-rail#OPERATION_ALGEBRA)
- Deployment lattice: boot folds applied/known identifier sets and the fingerprint gate against the placement's migrate/read-ahead authority, so a store newer than the binary is a typed rejection and the apply arm is placement-derived, never a runtime branch. (schema-rail#MIGRATION_LAW)
- Content-addressed identity unification: the sealed snapshot `Hash` is one content address selecting cache tag, blob lookup, and diff identity at once — no parallel key mints, so the artifact-blob index, the L2 cache, and the snapshot diff share one content key. (snapshot-codecs#SNAPSHOT_PROTOCOL)
- Hold-first sweep transformer: `Sweep` filters holds before every eligibility evaluation so `LegalHold` dominates any bound by construction, and a new retained class composes with zero consumer change. (redaction-retention#RETENTION_SWEEPS)
- Interceptor-spine effect-transformer stack: the four EF hooks, the linq2db `AddInterceptor` altitude, and the Npgsql tracer/meter roots register as ordered transformers on one stack where registration order is execution order — a soft-delete audit or trace-depth cap is a new transformer row, never code in an operation body. (query-rail#INTERCEPTOR_SPINE)
- One-log four-fold projection: the single `op_log` table is simultaneously outbox, audit artifact, change feed, and sync feed — four folds over one structure committed inside the entity transaction, so a polled outbox, a CDC trigger, and a second changefeed are all structurally deleted. (sync-collaboration#OPLOG_CHANGEFEED)
- LWW join-semilattice convergence: `Adjudicate` over (HLC, origin) makes any partition of any permutation of the op multiset applied any number of times converge to identical materialized state — one statement subsuming idempotency, commutativity, reorder tolerance, and crash replay. (sync-collaboration#MERGE_LAW)
- Transport-invariant merge core: `SyncTransport` variance is case rows plus two fields (topology, direction); the LWW law and the op-log shape stay transport-invariant, so PgLogical replication, HTTP delta, and speckle-class diff all fold into one `Apply`, and a new transport touches zero entity or query surface. (sync-collaboration#TRANSPORT_AXIS)
- Incremental whole-artifact identity: `FrameHash` feeds each 64 KiB frame into one `XxHash128` instance via `Append` and reads `GetCurrentHash`, so a multi-gigabyte blob never materializes contiguously and the same content key threads streamed transfer, the blob catalog, and the sync `Closure` manifest. (sync-collaboration#PRESENCE_AND_BLOB)
- Capability-fold dispatch: `DataLane.Profiles` and `ExtensionRequirement.Verify` fold engine/server capability into a typed verdict the rail dispatches on, so a lane the engine lacks is an absent slot that never composes — exclusion at composition, never a runtime not-supported throw. (data-lanes#LANE_AXIS)
