# [PERSISTENCE_FEATURES]

Isolation-concept atlas for durable state. Every concept rides one budgeted DENSITY_BAR owner so a new product is a row, case, or policy value, never a surface. The `[OWNER]` cell names that owner; the `[PAGE#CLUSTER]` cell routes to the mechanics; owner realization state is read from the charter DENSITY_BAR by following the owner. Mechanics live in the finalized `.planning/` pages.

## [1]-[ISOLATION_CONCEPTS]

The products this folder uniquely owns; each concept folds over one budgeted owner.

| [INDEX] | [FEATURE]                                              | [OWNER]              | [PAGE#CLUSTER]                          |
| :-----: | :----------------------------------------------------- | :------------------- | :-------------------------------------- |
|   [1]   | Topology-as-rows store axis                            | `StoreProfile`       | store-profiles#PROFILE_AXIS             |
|   [2]   | Receipted open/restore/drain ceremony                  | `StoreLifecycle`     | store-profiles#STORE_LIFECYCLE          |
|   [3]   | Cross-process writer lease + epoch fence               | `StoreLeaseRow`      | store-profiles#CROSS_PROCESS_LAW        |
|   [4]   | Seven-lane durable shape axis                          | `DataLane`           | data-lanes#LANE_AXIS                     |
|   [5]   | Aggregate document store + per-column JSON index       | `DataLane`           | data-lanes#DOCUMENT_LANE                 |
|   [6]   | Vector + full-text semantic search                     | `DataLane`           | data-lanes#SEARCH_LANES                  |
|   [7]   | Geospatial site-context lanes (PostGIS + GeoPackage)   | `GeoLayer`           | data-lanes#GEO_LANES                     |
|   [8]   | Analytical projection lane + HLC changefeed to parquet | `TabularExportSpec`  | data-lanes#ANALYTICAL_LANE               |
|   [9]   | One operation algebra + Kleisli composition            | `StoreOp<T>`         | query-rail#OPERATION_ALGEBRA            |
|  [10]   | Keyset pagination + typed projection egress            | `KeysetPage<TRow>`   | query-rail#PROJECTION_SHAPES            |
|  [11]   | Bulk movement with self-emitted changefeed + delta     | `BulkRoute`          | query-rail#BULK_LANE                     |
|  [12]   | Four-hook interceptor spine + store observability      | `StoreInterceptor`   | query-rail#INTERCEPTOR_SPINE            |
|  [13]   | Schema-drift gate + expand/contract migration law      | `SchemaFault`        | schema-rail#MIGRATION_LAW               |
|  [14]   | Three-row identity axis (uuid-v7 / content / natural)  | `IdentityPolicy`     | schema-rail#IDENTITY_POLICY             |
|  [15]   | Generated-converter admission + snake-case naming      | `ConverterRail`      | schema-rail#CONVERTER_RAIL              |
|  [16]   | Embedded engine floor (PRAGMA, compile, maintenance)   | `SqlitePragma`       | native-sqlite#PRAGMA_TABLE              |
|  [17]   | Loadable-extension + encryption gate                   | `ExtensionGate`      | native-sqlite#EXTENSION_GATES           |
|  [18]   | Three-row sealed snapshot codec + atomic seal          | `SnapshotCodec`      | snapshot-codecs#CODEC_AXIS              |
|  [19]   | Compression + hash policy axes                         | `CompressionPolicy`  | snapshot-codecs#COMPRESSION_HASHING     |
|  [20]   | Content-addressed snapshot identity + diff             | `SnapshotCodec`      | snapshot-codecs#SNAPSHOT_PROTOCOL       |
|  [21]   | L2 cache contribution behind the AppHost port          | `CacheContribution`  | cache-indexes#L2_CONTRIBUTION           |
|  [22]   | Model-result index (warm-start substrate selection)    | `ModelResultKey`     | cache-indexes#MODEL_RESULT_INDEX        |
|  [23]   | Content-addressed artifact-blob catalog                | `ArtifactIndexRow`   | cache-indexes#ARTIFACT_BLOB_INDEX       |
|  [24]   | Recency-gated benchmark route index                    | `BenchmarkRow`       | cache-indexes#BENCHMARK_INDEX           |
|  [25]   | Op-log changefeed (one log: outbox, audit, feed, sync) | `OpLogEntry`         | sync-collaboration#OPLOG_CHANGEFEED     |
|  [26]   | LWW merge law + typed conflict receipts                | `ConflictOutcome`    | sync-collaboration#MERGE_LAW            |
|  [27]   | Three-transport sync axis (replication, http, diff)    | `SyncTransport`      | sync-collaboration#TRANSPORT_AXIS       |
|  [28]   | Object-graph sync (speckle-class diff transport)       | `SyncTransport`      | sync-collaboration#TRANSPORT_AXIS       |
|  [29]   | Presence + offline-queue collaboration                 | `PresenceRow`        | sync-collaboration#PRESENCE_AND_BLOB    |
|  [30]   | Streamed blob transfer (incremental frame hash)        | `PresenceRow`        | sync-collaboration#PRESENCE_AND_BLOB    |
|  [31]   | Classification enforcement at every store egress       | `ArtifactClasses`    | redaction-retention#CLASSIFICATION_ENFORCEMENT |
|  [32]   | Hold-first receipted retention sweep                   | `RetentionPolicy`    | redaction-retention#RETENTION_SWEEPS    |
|  [33]   | Support-bundle store evidence + hash-proved export     | `ExportProof`        | redaction-retention#EXPORT_PROOF        |
|  [34]   | Classification to pgaudit category binding             | `AuditBinding`       | redaction-retention#AUDIT_BINDING       |
|  [35]   | Operator-provisioning manifest + capability verdict    | `ExtensionRequirement` | store-profiles#PROVISIONING_ROWS      |
|  [36]   | Dashboard wire surface (snapshot + sync TS contracts)  | `SnapshotCodec`      | snapshot-codecs#TS_PROJECTION           |

## [2]-[CAPABILITY_EXPANSION_REGISTER]

Capabilities a downstream app hand-rolls in this domain absent the owner, each mapped to the budgeted owner and the `page#cluster` row/case/column that absorbs it. Owner realization state is read from the charter DENSITY_BAR via the owner.

| [INDEX] | [FEATURE]                                                      | [OWNER]                | [PAGE#CLUSTER]                          |
| :-----: | :------------------------------------------------------------- | :--------------------- | :-------------------------------------- |
|   [1]   | Live store attach snapshot coherence under one writer          | `TabularExportSpec`    | data-lanes#ANALYTICAL_LANE              |
|   [2]   | In-process managed sequence as a planner relation             | `TabularExportSpec`    | data-lanes#ANALYTICAL_LANE              |
|   [3]   | Vector-quantum chunk transfer for derived rollup staging       | `TabularExportSpec`    | data-lanes#ANALYTICAL_LANE              |
|   [4]   | Multi-statement fan-out read as one server round-trip          | `StoreOp<T>`           | query-rail#OPERATION_ALGEBRA           |
|   [5]   | Custom provider-type classification without per-type MapEnum    | `StoreProfile`         | store-profiles#PROFILE_AXIS            |
|   [6]   | PostgreSQL composite-type round-trip                          | `SchemaDdl`            | schema-rail#EXTENSION_DDL              |
|   [7]   | Read-only embedded reader replica without writer contention    | `StoreProfile`         | store-profiles#PROFILE_AXIS            |
|   [8]   | Domain-fold aggregate registered into the embedded engine      | `SqliteMaintenance`    | native-sqlite#MAINTENANCE_OPS         |
|   [9]   | Case-insensitive / locale collation in embedded queries        | `SqliteMaintenance`    | native-sqlite#MAINTENANCE_OPS         |
|  [10]   | Paged hot-store backup with progress receipts                  | `SqliteMaintenance`    | native-sqlite#MAINTENANCE_OPS         |
|  [11]   | Consistent multi-transaction read pin (monotone reader)        | `SqliteMaintenance`    | native-sqlite#MAINTENANCE_OPS         |
|  [12]   | jsonb blob fast path beside the TEXT-json EF mapping            | `ExtensionGate`        | native-sqlite#EXTENSION_GATES         |
|  [13]   | Per-connection defensive hardening before first statement      | `ExtensionGate`        | native-sqlite#EXTENSION_GATES         |
|  [14]   | Streaming compression for payloads above 1 MiB                 | `CompressionPolicy`    | snapshot-codecs#COMPRESSION_HASHING   |
|  [15]   | Zstandard compression swap keeping LZ4 archives readable        | `CompressionPolicy`    | snapshot-codecs#COMPRESSION_HASHING   |
|  [16]   | AOT generated MessagePack resolver replacing runtime composite | `SnapshotCodec`        | snapshot-codecs#CODEC_AXIS            |
|  [17]   | Reader-to-writer fusion (no parse-materialize-rebuild)         | `TabularExportSpec`    | data-lanes#ANALYTICAL_LANE            |
|  [18]   | Parallel ordered tabular projection                           | `TabularExportSpec`    | data-lanes#ANALYTICAL_LANE            |
|  [19]   | Typed feature-attribute projection (no loose-table walking)    | `GeoLayer`             | data-lanes#GEO_LANES                  |
|  [20]   | Constructive managed geometry refine over container candidates | `GeoLayer`             | data-lanes#GEO_LANES                  |
|  [21]   | NodaTime range + multirange temporal columns                   | `GeoLayer`             | data-lanes#GEO_LANES                  |
|  [22]   | Hierarchy path lane (assembly/part trees)                      | `GeoLayer`             | data-lanes#GEO_LANES                  |
|  [23]   | DB-temporal daylight-transition policy                         | `StoreProfile`         | store-profiles#PROFILE_AXIS           |
|  [24]   | Idle replication-slot reclamation + conflict statistics        | `SyncTransport`        | sync-collaboration#TRANSPORT_AXIS     |
|  [25]   | Join-preserving HMAC redaction across artifacts                | `ExportProof`          | redaction-retention#EXPORT_PROOF      |
|  [26]   | Native at-rest encryption for personal-class stores            | `ExtensionGate`        | native-sqlite#EXTENSION_GATES         |
|  [27]   | Compiled hot-query delegates for proven hot shapes             | `KeysetPage<TRow>`     | query-rail#PROJECTION_SHAPES          |
|  [28]   | pg_stat_statements slow-query evidence read view               | `StoreInterceptor`     | query-rail#INTERCEPTOR_SPINE          |
|  [29]   | Native PostgreSQL enum-type round-trip                         | `SchemaDdl`            | schema-rail#EXTENSION_DDL             |
|  [30]   | Native enum/composite type-resolution open gate                | `ExtensionRequirement` | store-profiles#PROVISIONING_ROWS      |
|  [31]   | Bit-vector distance metrics (Hamming / Jaccard)                | `DataLane`             | data-lanes#SEARCH_LANES               |
|  [32]   | GeoJSON projection off the canonical wire geometry             | `GeoJsonProjection`    | snapshot-codecs#CODEC_AXIS            |
|  [33]   | linq2db two-door bridge terminators + temporal aggregate       | `KeysetPage<TRow>`     | query-rail#PROJECTION_SHAPES          |
|  [34]   | Npgsql meter histogram/cardinality posture column              | `StoreInterceptor`     | query-rail#INTERCEPTOR_SPINE          |

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

## [4]-[ROUTING]

- Concept mechanics: `.planning/<page>.md` at the named `cluster` anchor.
- Owners, budgets, gaps, admissions, and owner state: `.planning/README.md` (DENSITY_BAR, GAP_LEDGER, ADMISSIONS_RECORD).
- Implementation-start gates and research probes: `ROADMAP.md` and each page RESEARCH cluster.
- External-API grounding: `.api/<package>.md` per the catalogue README rail map.
- Cross-package topologies and concert concepts: `../.planning/FEATURES.md`.
- TS web-layer consumption of the wire surfaces: `libs/typescript/.planning/`.
