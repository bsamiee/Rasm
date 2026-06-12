# [PERSISTENCE_PLANNING]

Rasm.Persistence has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns durable state: store profiles, data lanes, schema and query rails, native SQLite truth, snapshot codecs, cache indexes, sync and collaboration transports, and redaction/retention — consuming AppHost ports (clock, telemetry, receipts, drain, classification) as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                        | [OWNS]                                                                        |  [STATE]  |
| :-----: | --------------------------------------------- | ----------------------------------------------------------------------------- | :-------: |
|   [1]   | [store-profiles](store-profiles.md)           | Six-row engine axis; widened row record; cross-process and provisioning law   | finalized |
|   [2]   | [data-lanes](data-lanes.md)                   | Seven-lane capability map incl. geometry lanes and extension capabilities     | finalized |
|   [3]   | [schema-rail](schema-rail.md)                 | Identity policy, migrations, generated columns, extension declarations        | finalized |
|   [4]   | [query-rail](query-rail.md)                   | StoreOp dispatch, pooled contexts, bulk lane, delta projection, interceptors  | finalized |
|   [5]   | [native-sqlite](native-sqlite.md)             | Verified e_sqlite3 surface, WAL law, loadable extensions, encryption gate     | finalized |
|   [6]   | [snapshot-codecs](snapshot-codecs.md)         | Codec axis, compression, hashing, restore choreography, wire contracts        | finalized |
|   [7]   | [cache-indexes](cache-indexes.md)             | L2 cache contribution, serializer rows, model-result and artifact indexes     | finalized |
|   [8]   | [sync-collaboration](sync-collaboration.md)   | SyncTransport axis, op-log changefeed, diff rows, presence, conflict receipts | finalized |
|   [9]   | [redaction-retention](redaction-retention.md) | Retention policies, classification enforcement, audit binding                 | finalized |

## [2]-[WIRE_PAGES]

snapshot-codecs · sync-collaboration (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

linq2db core (LinqToDB.dll) merge/output surfaces ride the bridge package and get a page only if the core package is admitted; app-root JsonPatch catalogued at app-root creation.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                                                                                                                 | [CLOSED_BY]                                   | [STATE] |
| :-----: | ----------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- | :-----: |
|   [1]   | Execution-strategy rows (EnableRetryOnFailure / busy-retry); database retry excluded from the hop law                                                 | store-profiles + query-rail                   | CLOSED  |
|   [2]   | PooledDbContextFactory pooling in the query rail                                                                                                      | query-rail                                    | CLOSED  |
|   [3]   | Optimistic-concurrency tokens (pg xmin, sqlite version column) to typed store fault                                                                   | schema-rail + query-rail                      | CLOSED  |
|   [4]   | UseSeeding/UseAsyncSeeding reference-data law                                                                                                         | store-profiles                                | CLOSED  |
|   [5]   | PG maintenance/backup symmetry rows (ANALYZE, REINDEX, autovacuum posture)                                                                            | store-profiles                                | CLOSED  |
|   [6]   | Receipted restore choreography fencing all writers, deleting -wal/-shm                                                                                | snapshot-codecs + store-profiles              | CLOSED  |
|   [7]   | Cross-process law: WAL + busy-retry + first-opener-migrates + HLC maintenance lease; lease-handoff distinct from crash-reclaim with staleness timeout | store-profiles + native-sqlite                | CLOSED  |
|   [8]   | Clock-seam injection for all TTL/retention/HLC stamping                                                                                               | redaction-retention + sync-collaboration      | CLOSED  |
|   [9]   | IdentityPolicy axis (UuidV7Key, ContentHash, NaturalKey); sqlite uuidv7 via CreateFunction                                                            | schema-rail                                   | CLOSED  |
|  [10]   | Bulk-path invalidation: linq2db lane emits its own changefeed + tag invalidation                                                                      | query-rail                                    | CLOSED  |
|  [11]   | DataClassification consumed from AppHost; store-side enforcement rows only                                                                            | redaction-retention                           | CLOSED  |
|  [12]   | PG18 adoption rows: uuidv7, virtual generated columns (STORED when replicated/indexed), replication policy fields, MERGE/RETURNING delta              | schema-rail + query-rail + sync-collaboration | CLOSED  |
|  [13]   | Extensions axis: first-party ADOPT rows, PostGIS lanes via NTS chain, operator-provisioned rows, named REJECT rows                                    | data-lanes + schema-rail + store-profiles     | CLOSED  |
|  [14]   | Verified e_sqlite3 compile-flag table + loadable-extension law; sqlean/sqlite-vec gates with DuckDB vss alternative                                   | native-sqlite                                 | CLOSED  |
|  [15]   | M1-M5 sync rows: HasObjects diff, __closure manifest field, TransportBridge ownership here, fan-in/out topology field, capture-direction field        | sync-collaboration                            | CLOSED  |
|  [16]   | HttpDelta fallback = RFC 6902 JsonPatchDocument, subordinate to the op-log changefeed; merge-patch rejected                                           | sync-collaboration                            | CLOSED  |
|  [17]   | BlobRemote consumes the ArtifactSync frame constants as settled vocabulary                                                                            | sync-collaboration                            | CLOSED  |
|  [18]   | Presence rows (ephemeral changefeed + DropOldest)                                                                                                     | sync-collaboration                            | CLOSED  |
|  [19]   | Conflict-receipt projection consumed by the UI inspector                                                                                              | sync-collaboration                            | CLOSED  |
|  [20]   | Parquet schema-versioning policy row; DuckDB ATTACH sqlite_scanner row; postgres_scanner rejected                                                     | data-lanes                                    | CLOSED  |
|  [21]   | jsonb canonical law, ComplexProperty().ToJson(), jsonpath surface; OwnsOne().ToJson() rejected                                                        | schema-rail + query-rail                      | CLOSED  |
|  [22]   | GeoJSON4STJ wire projection + GeoPackage container rows; one canonical wire geometry (proto), NTS as PG boundary                                      | data-lanes + snapshot-codecs                  | CLOSED  |
|  [23]   | Snapshot-diff projection between two snapshots                                                                                                        | snapshot-codecs                               | CLOSED  |
|  [24]   | EP-context + profiling artifact blob routing (Compute consumes)                                                                                       | cache-indexes                                 | CLOSED  |
|  [25]   | Filesystem-locality admission guard (WAL needs local volume; network homes typed-rejected)                                                            | store-profiles                                | CLOSED  |
|  [26]   | SQLCipher encryption row research-gated on provider + external dylib spike                                                                            | native-sqlite                                 | CLOSED  |
|  [27]   | pgaudit classification binding research row; LegalHold-vs-sweep ordering                                                                              | redaction-retention                           | CLOSED  |
|  [28]   | Thinktecture EF value converters + MessagePack formatters as codec rows                                                                               | schema-rail + snapshot-codecs                 | CLOSED  |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of naive LOC. One owner per axis, one entrypoint family per rail; a new feature is a row, case, or policy value — never a new surface. The budget below is the complete public-owner set; a type outside it is a defect.

| [INDEX] | [AXIS/CONCERN]               | [OWNER]                                                                                                              | [KIND]                                                     |               [CASES]               |
| :-----: | ---------------------------- | -------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- | :---------------------------------: |
|   [1]   | Engine axis + blob contract  | `StoreProfile` + `StoreRows` + `BlobRemote`                                                                          | `[SmartEnum<string>]` widened rows + records               |               6 rows                |
|   [2]   | Store lifecycle              | `StoreLifecycle` + `StoreOpenReceipt` + `StoreCeremony`                                                              | `[SmartEnum<string>]` + fold surface                       |              5 states               |
|   [3]   | Placement                    | `StorePlacement`                                                                                                     | record + total fold                                        |               8 arms                |
|   [4]   | Cross-process law            | `StoreLeaseRow` + `StoreLocality`                                                                                    | record + admission guard                                   |            2 lease kinds            |
|   [5]   | Operator provisioning        | `ExtensionRequirement`                                                                                               | record table + verify fold                                 |               7 rows                |
|   [6]   | Lane axis                    | `DataLane` + `KvEntry`                                                                                               | `[Union]` + capability fold                                |               7 cases               |
|   [7]   | Document/search vocabulary   | `JsonIndex` · `VectorMetric` · `FullTextMode`                                                                        | `[SmartEnum<string>]`                                      |              3 · 4 · 4              |
|   [8]   | Geo + analytical policy      | `GeoLayer` + `TabularExportSpec` + `ParquetSchemaStamp`                                                              | policy records                                             |           row-per-concern           |
|   [9]   | Identity axis                | `IdentityPolicy`                                                                                                     | `[SmartEnum<string>]`                                      |               3 rows                |
|  [10]   | Schema law                   | `SchemaFault` + `SchemaFingerprint` + `DerivedColumn` + `SchemaDdl` + `ConverterRail`                                | `[Union]` fault + struct + DDL rows                        |       5 codes · 19 extensions       |
|  [11]   | Operation algebra            | `StoreOp<T>` + `StoreFault` + `StoreRail`                                                                            | `[Union]` × 2 + total dispatch                             |          8 ops · 6 faults           |
|  [12]   | Projection egress            | `KeysetPage<TRow>` + `ProjectionRail`                                                                                | record + extension fold                                    |            3 filter keys            |
|  [13]   | Bulk lane                    | `BulkRoute` + `BulkReceipt` + `BulkDelta<TRow>`                                                                      | `[SmartEnum]` + typed receipts                             |              3 routes               |
|  [14]   | Interceptor spine            | `StoreInterceptor` + `InterceptPolicy` + `StoreFact` + `StoreObservability`                                          | boundary capsule + policy + facts                          |          4 hooks · 7 kinds          |
|  [15]   | Native policy tables         | `SqlitePragma` + `SqliteFactKind` + `SqliteCompileSurface`                                                           | `[SmartEnum<string>]` tables + probe                       |        10 pragmas · 14 kinds        |
|  [16]   | Maintenance + gates          | `SqliteMaintenance` + `FunctionRegistration` + `ExtensionGate` + `EncryptionGate`                                    | verb surface + gate rows                                   |          9 verbs · 8 gates          |
|  [17]   | Snapshot codecs + protocol   | `SnapshotCodec` · `CompressionPolicy` · `HashPolicy` + `Snapshots` + `SnapshotRestoreOps` + `PersistenceWireContext` | `[SmartEnum<string>]` delegate rows + folds + wire context |           3 · 3 · 3 rows            |
|  [18]   | Cache contribution + indexes | `CacheContribution` + `ModelResultKey` + `ArtifactIndexRow` + `BenchmarkRow` + `IndexSurface`                        | boundary capsule + key shapes                              |     1 registration · 3 indexes      |
|  [19]   | Sync spine                   | `SyncOpKind` + `OpLogEntry` + `SyncMerge` + `ConflictOutcome` + `SyncTransport` + `SyncPump` + `Presence`            | vocab + `[Union]` × 2 + total dispatch                     | 3 kinds · 4 outcomes · 3 transports |
|  [20]   | Retention + classification   | `RetentionPolicy` + `ArtifactClasses` + `ClassificationGuard` + `StoreEvidence` + `AuditBinding`                     | axis + frozen tables + guards                              | 4 policies · 7 classes · 5 bindings |

Comparer accessors (`StoreKeyPolicy`, `SqliteKeyPolicy`, `SnapshotKeyPolicy`, `SyncKeyPolicy`, `RetentionKeyPolicy`) ride inside their owner files, one per axis family, package-local.

## [6]-[BUILD_ORDER]

| [INDEX] | [FILE]                   | [TRANSCRIBES]                                                                                        | [GATE]                                                               |
| :-----: | ------------------------ | ---------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
|   [1]   | `Stores/Profiles.cs`     | store-profiles#PROFILE_AXIS + #PLACEMENT_MATRIX + #CROSS_PROCESS_LAW                                 | static fix/build; profile, placement, locality specs                 |
|   [2]   | `Stores/Lifecycle.cs`    | store-profiles#STORE_LIFECYCLE + #PROVISIONING_ROWS                                                  | transition-law specs; two-process WAL race spec                      |
|   [3]   | `Schema/SchemaRail.cs`   | schema-rail#IDENTITY_POLICY + #MIGRATION_LAW + #GENERATED_COLUMNS + #EXTENSION_DDL + #CONVERTER_RAIL | gate specs; `dotnet ef` script/optimize probes                       |
|   [4]   | `Lanes/DataLanes.cs`     | data-lanes#LANE_AXIS + #DOCUMENT_LANE + #SEARCH_LANES + #GEO_LANES + #ANALYTICAL_LANE                | lane-admission specs; attach/window probe specs                      |
|   [5]   | `Native/Sqlite.cs`       | native-sqlite#PRAGMA_TABLE + #COMPILE_SURFACE + #MAINTENANCE_OPS + #EXTENSION_GATES                  | compile-surface probe spec; bridge scenario for the host dlopen seam |
|   [6]   | `Query/QueryRail.cs`     | query-rail#OPERATION_ALGEBRA + #PROJECTION_SHAPES + #BULK_LANE + #INTERCEPTOR_SPINE                  | dispatch-totality + fault-conversion specs                           |
|   [7]   | `Cache/Indexes.cs`       | cache-indexes#L2_CONTRIBUTION + #MODEL_RESULT_INDEX + #ARTIFACT_BLOB_INDEX + #BENCHMARK_INDEX        | paired closure with [8]                                              |
|   [8]   | `Snapshots/Codecs.cs`    | snapshot-codecs#CODEC_AXIS + #COMPRESSION_HASHING + #SNAPSHOT_PROTOCOL + #RESTORE_AND_DIFF           | round-trip, header, restore specs; gate closes [7]+[8]               |
|   [9]   | `Sync/Collaboration.cs`  | sync-collaboration#OPLOG_CHANGEFEED + #MERGE_LAW + #TRANSPORT_AXIS + #PRESENCE_AND_BLOB              | merge-law idempotency + adjudication specs                           |
|  [10]   | `Retention/Redaction.cs` | redaction-retention#CLASSIFICATION_ENFORCEMENT + #RETENTION_SWEEPS + #EXPORT_PROOF + #AUDIT_BINDING  | sweep-fold, guard, audit-binding specs                               |

Seam ordering law:
- Fingerprint slot: `StoreOpenReceipt.SchemaFingerprint` stays bare `ulong` (ledger seam), so [2] precedes [3] with zero forward reference; `SchemaFingerprint` in [3] is the typed owner.
- Wire-context pairing: [7] precedes [8] and the two files gate as one build closure — `PersistenceWireContext` declares the `CacheIndexFact` serializable row while `IndexSurface` consumes the generated context.
- Mutual placement: `StoreProfile` delegate columns consume `StorePlacement` and the placement record carries `StoreProfile` fields — both land in [1], never split.
- TS_PROJECTION clusters transcribe into the TS workspace under the suite wire law, never into `.cs` files.

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end, then read every page named in the file's TRANSCRIBES cell end-to-end before opening the file.
2. Transcribe every signature fence verbatim; add only file-organization scaffolding — namespace, usings, section separators per the repo file-organization law.
3. Run the collapse scan after every edit: 3+ parallel types, 3+ sibling factories, 3+ repeated switch arms, or 3+ single-call helpers triggers in-place polymorphic collapse, never a new file.
4. Run `uv run python -m tools.assay static fix` then `uv run python -m tools.assay static build` on the touched closure; a busy lease exits 5 and reruns.
5. Author specs per the `testing-cs` skill — law-matrix specs over the page cards; research-row proofs land as named specs in the same pass.
6. Host seams gate through bridge scenarios (`scenarios/extension-load.verify.csx`); a file with no host seam carries no scenario.
7. Pages stay finalized: a transcription conflict routes back to the page owner and the ledger, never into implementation-side redesign.

## [8]-[PROOF_GATES]

| [INDEX] | [GATE]     | [COMMAND]                                                                                                     | [EVIDENCE]                                                          |
| :-----: | ---------- | ------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
|   [1]   | restore    | `dotnet restore --force-evaluate`                                                                             | `packages.lock.json` closure regenerated and committed; zero NU1004 |
|   [2]   | catalogue  | `uv run python -m tools.assay api doctor --strict` + `api resolve`                                            | every ADMISSIONS_RECORD package resolves an assay asset             |
|   [3]   | routing    | `uv run python -m tools.assay static plan`                                                                    | Persistence closure owners and triggers land in notes               |
|   [4]   | compile    | `uv run python -m tools.assay static build`                                                                   | green leased build; `': error '` grep empty over the closure        |
|   [5]   | specs      | `uv run python -m tools.assay test run --target Rasm.Persistence.Tests`                                       | all specs green, research-row proof specs included                  |
|   [6]   | host seams | `uv run python -m tools.assay bridge verify libs/csharp/Rasm.Persistence/scenarios/extension-load.verify.csx` | scenario facts green inside live RhinoWIP                           |
|   [7]   | diagrams   | `npx -y @mermaid-js/mermaid-cli -i ARCHITECTURE.md -o /tmp/persistence-arch.svg`                              | local render exits zero; the MCP renderer stays permission-blocked  |

## [9]-[PROHIBITIONS]

- [NEVER] add a public surface beside the budgeted owners in [5]; a new capability is a row, case, or policy value.
- [NEVER] create wrappers, rename adapters, helper/utility files, or a layer over provider functions.
- [NEVER] introduce generic receipt or ledger abstractions; `StoreOpenReceipt`, `MigrationReceipt`, `BulkReceipt`, `SweepReceipt`, `ExportProof`, `SyncApplyReceipt`, `ConflictReceipt`, and `RestoreReceipt` stay typed.
- [NEVER] propagate sentinels — `DateTime` defaults, `Deleted`/`Inserted` nulls, and empty keys project to `Option<T>` at the boundary.
- [NEVER] call `DateTime.UtcNow`, `Stopwatch`, or direct timers; `ClockPolicy` is the only time seam.
- [NEVER] add a second cache, retry, or correlation owner — AppHost owns port, stampede, tags, and hop retry; `EnableRetryOnFailure` plus busy-retry are the only database retry owners and the database stays outside the hop law.
- [NEVER] write repository families, per-entity services, per-lane services, provider-twin query shapes, lazy loading, or offset pagination.
- [NEVER] hand-write converters, formatters, or migration code beside the generated rails — Thinktecture converters, EF-emitted migrations, and source-generated contexts own those forms.
- [NEVER] declare a second taxonomy: classification, redactor tables, blob framing constants, lease policy shapes, and profile-keyed tables compose from their settled owners.
- [NEVER] reference EF `Internal`-namespace types; migration-lock evidence reads from receipts.
- [NEVER] add a trigger-based second changefeed path; op-log rows commit with entity rows in one transaction.
- [NEVER] admit a new engine row — the sweep is closed (libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory rejected); PostgreSQL is never spawned or bundled by a Rasm process.
- [NEVER] execute runtime `ALTER SYSTEM`; provisioning is verification-only.
- [NEVER] treat CSP analyzer diagnostics as suppression targets; they are architecture pressure.

## [10]-[ADMISSIONS_RECORD]

[STORE_LANES]:

| [PACKAGE]                                              | [VERSION]     | [PAGE]          | [CATALOGUE]                                                                         |
| ------------------------------------------------------ | ------------- | --------------- | ----------------------------------------------------------------------------------- |
| DuckDB.NET.Data.Full                                   | 1.5.3         | data-lanes      | [api-duckdb](../.reports/api/api-duckdb.md)                                         |
| EFCore.NamingConventions                               | 10.0.1        | schema-rail     | [api-ef-naming](../.reports/api/api-ef-naming.md)                                   |
| linq2db.EntityFrameworkCore                            | 10.4.0        | query-rail      | [api-linq2db-ef](../.reports/api/api-linq2db-ef.md)                                 |
| Microsoft.Data.Sqlite                                  | 10.0.9        | native-sqlite   | [api-sqlite](../.reports/api/api-sqlite.md)                                         |
| Microsoft.EntityFrameworkCore.Design                   | 10.0.9        | schema-rail     | [api-ef-design](../.reports/api/api-ef-design.md)                                   |
| Microsoft.EntityFrameworkCore.Sqlite                   | 10.0.9        | store-profiles  | [api-ef-sqlite](../.reports/api/api-ef-sqlite.md)                                   |
| NetTopologySuite.IO.GeoJSON4STJ                        | 4.0.0         | snapshot-codecs | [api-nts-io](../.reports/api/api-nts-io.md)                                         |
| NetTopologySuite.IO.GeoPackage                         | 2.0.0         | data-lanes      | [api-nts-io](../.reports/api/api-nts-io.md)                                         |
| Npgsql                                                 | 10.0.3        | store-profiles  | [api-npgsql](../.reports/api/api-npgsql.md)                                         |
| Npgsql.EntityFrameworkCore.PostgreSQL                  | 10.0.2        | store-profiles  | [api-npgsql-ef](../.reports/api/api-npgsql-ef.md)                                   |
| Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | 10.0.2        | data-lanes      | [api-nts-ef](../.reports/api/api-nts-ef.md)                                         |
| Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime         | 10.0.2        | data-lanes      | [api-npgsql-ef-nodatime](../.reports/api/api-npgsql-ef-nodatime.md)                 |
| Npgsql.OpenTelemetry                                   | 10.0.3        | query-rail      | [api-npgsql-otel](../.reports/api/api-npgsql-otel.md)                               |
| Pgvector.EntityFrameworkCore                           | 0.3.0         | data-lanes      | [api-pgvector-ef](../.reports/api/api-pgvector-ef.md)                               |
| SQLitePCLRaw.bundle_e_sqlite3                          | 3.0.3         | native-sqlite   | [api-sqlitepcl](../.reports/api/api-sqlitepcl.md)                                   |
| Thinktecture.Runtime.Extensions.EntityFrameworkCore10  | 10.2.0-beta01 | schema-rail     | [api-thinktecture-serialization](../.reports/api/api-thinktecture-serialization.md) |

[SNAPSHOTS_AND_SUPPORT]:

| [PACKAGE]                                   | [VERSION] | [PAGE]              | [CATALOGUE]                                                                         |
| ------------------------------------------- | --------- | ------------------- | ----------------------------------------------------------------------------------- |
| K4os.Compression.LZ4                        | 1.3.8     | snapshot-codecs     | [api-lz4](../.reports/api/api-lz4.md)                                               |
| MessagePack                                 | 3.1.7     | snapshot-codecs     | [api-messagepack](../.reports/api/api-messagepack.md)                               |
| MessagePackAnalyzer                         | 3.1.7     | snapshot-codecs     | [api-messagepack-analyzer](../.reports/api/api-messagepack-analyzer.md)             |
| Microsoft.Extensions.Caching.Hybrid         | 10.7.0    | cache-indexes       | [api-hybrid-cache (AppHost)](../../Rasm.AppHost/.reports/api/api-hybrid-cache.md)   |
| Microsoft.Extensions.Compliance.Redaction   | 10.7.0    | redaction-retention | [api-redaction](../.reports/api/api-redaction.md)                                   |
| NodaTime                                    | 3.3.2     | store-profiles      | [api-nodatime](../.reports/api/api-nodatime.md)                                     |
| NodaTime.Serialization.SystemTextJson       | 1.4.0     | snapshot-codecs     | [api-nodatime-json](../.reports/api/api-nodatime-json.md)                           |
| Sep                                         | 0.14.1    | data-lanes          | [api-sep](../.reports/api/api-sep.md)                                               |
| System.IO.Hashing                           | 10.0.9    | schema-rail         | [api-hashing](../.reports/api/api-hashing.md)                                       |
| Thinktecture.Runtime.Extensions.Json        | 10.2.0    | snapshot-codecs     | [api-thinktecture-serialization](../.reports/api/api-thinktecture-serialization.md) |
| Thinktecture.Runtime.Extensions.MessagePack | 10.2.0    | snapshot-codecs     | [api-thinktecture-serialization](../.reports/api/api-thinktecture-serialization.md) |
