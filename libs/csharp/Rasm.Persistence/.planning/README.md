# [PERSISTENCE_PLANNING]

Rasm.Persistence has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns durable state: store profiles, data lanes, schema and query rails, native SQLite truth, snapshot codecs, cache indexes, sync and collaboration transports, and redaction/retention — consuming AppHost ports (clock, telemetry, receipts, drain, classification) as settled vocabulary.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                        | [OWNS]                                                       |  [STATE]  |
| :-----: | --------------------------------------------- | :----------------------------------------------------------- | :-------: |
|   [1]   | [store-profiles](store-profiles.md)           | engine axis, row records, cross-process law                  | finalized |
|   [2]   | [data-lanes](data-lanes.md)                   | lane map, geometry, analytical, extensions                   | finalized |
|   [3]   | [schema-rail](schema-rail.md)                 | identity, migrations, generated columns, DDL                 | finalized |
|   [4]   | [query-rail](query-rail.md)                   | operations, contexts, bulk lane, projection, interceptors    | finalized |
|   [5]   | [native-sqlite](native-sqlite.md)             | e_sqlite3, WAL, loadable extensions, encryption gate         | finalized |
|   [6]   | [snapshot-codecs](snapshot-codecs.md)         | codecs, compression, hashing, restore, wire contracts        | finalized |
|   [7]   | [cache-indexes](cache-indexes.md)             | cache contribution, serializers, result and artifact indexes | finalized |
|   [8]   | [sync-collaboration](sync-collaboration.md)   | sync transports, op-log, diffs, presence, conflicts          | finalized |
|   [9]   | [redaction-retention](redaction-retention.md) | retention, classification, audit binding                     | finalized |
|  [10]   | [remote-stores](remote-stores.md)             | object-store axis, multipart transfer, residence, sync feed  | finalized |
|  [11]   | [server-tier](server-tier.md)                 | time-series, search, cluster GUC, tenancy/RLS, migration bundle | finalized |

## [2]-[WIRE_PAGES]

snapshot-codecs · sync-collaboration (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

linq2db core (LinqToDB.dll) merge/output surfaces ride the bridge package and get a page only if the core package is admitted; app-root JsonPatch catalogued at app-root creation.

## [4]-[GAP_LEDGER]

Every row is CLOSED; `[CLOSED_BY]` names the page that absorbed the gap. A row is present only when closed, so a present row reads CLOSED by definition.

| [INDEX] | [GAP]                                       | [CLOSED_BY (page#cluster)]                    |
| :-----: | :------------------------------------------ | :-------------------------------------------- |
|   [1]   | database execution-strategy rows            | store-profiles + query-rail                   |
|   [2]   | pooled context factory                      | query-rail                                    |
|   [3]   | optimistic concurrency tokens               | schema-rail + query-rail                      |
|   [4]   | reference-data seeding law                  | store-profiles                                |
|   [5]   | PostgreSQL maintenance symmetry             | store-profiles                                |
|   [6]   | receipted restore choreography              | snapshot-codecs + store-profiles              |
|   [7]   | WAL, busy retry, opener, HLC lease law      | store-profiles + native-sqlite                |
|   [8]   | clock seam for durable stamping             | redaction-retention + sync-collaboration      |
|   [9]   | identity axis and SQLite uuidv7             | schema-rail                                   |
|  [10]   | bulk invalidation changefeed                | query-rail                                    |
|  [11]   | store-side classification enforcement       | redaction-retention                           |
|  [12]   | PostgreSQL 18 adoption rows                 | schema-rail + query-rail + sync-collaboration |
|  [13]   | extension axis and PostGIS lanes            | data-lanes + schema-rail + store-profiles     |
|  [14]   | e_sqlite3 compile flags and extension gates | native-sqlite                                 |
|  [15]   | M1-M5 sync rows                             | sync-collaboration                            |
|  [16]   | RFC 6902 HTTP delta fallback                | sync-collaboration                            |
|  [17]   | BlobRemote frame constants                  | sync-collaboration                            |
|  [18]   | ephemeral presence rows                     | sync-collaboration                            |
|  [19]   | UI conflict receipt projection              | sync-collaboration                            |
|  [20]   | Parquet and DuckDB analytical rows          | data-lanes                                    |
|  [21]   | jsonb canonical law                         | schema-rail + query-rail                      |
|  [22]   | GeoJSON, GeoPackage, canonical geometry     | data-lanes + snapshot-codecs                  |
|  [23]   | snapshot diff projection                    | snapshot-codecs                               |
|  [24]   | compute artifact blob routing               | cache-indexes                                 |
|  [25]   | filesystem-locality admission guard         | store-profiles                                |
|  [26]   | SQLCipher research gate                     | native-sqlite                                 |
|  [27]   | pgaudit and legal-hold ordering             | redaction-retention                           |
|  [28]   | Thinktecture codec rows                     | schema-rail + snapshot-codecs                 |
|  [29]   | cloud object-store residence topology       | remote-stores#OBJECT_STORE                    |
|  [30]   | chunked resumable multipart transfer        | remote-stores#MULTIPART_TRANSFER              |
|  [31]   | cloud object-store sync hub seam            | remote-stores#ARTIFACT_SYNC_FEED              |
|  [32]   | self-provisioned PG18 time-series tier      | server-tier#TIMESCALE_PROVISIONING            |
|  [33]   | diskann + BM25 index provisioning           | server-tier#SEARCH_PROVISIONING               |
|  [34]   | cluster GUC deploy fragments + verify       | server-tier#CLUSTER_CONFIG                    |
|  [35]   | multi-tenancy + RLS isolation axis          | server-tier#TENANCY_RLS                       |
|  [36]   | migration-bundle service-deploy gate        | server-tier#MIGRATION_BUNDLE                  |
|  [37]   | Redis L2 cache residence                    | cache-indexes#L2_CONTRIBUTION                 |
|  [38]   | bit-vector columns + Hamming/Jaccard        | data-lanes#SEARCH_LANES                       |
|  [39]   | DuckDB in-process table-function relation   | data-lanes#ANALYTICAL_LANE                    |
|  [40]   | OLD/NEW RETURNING bulk emission             | query-rail#BULK_LANE                          |
|  [41]   | Sep header-range column projection          | data-lanes#ANALYTICAL_LANE                    |
|  [42]   | pg_jsonschema document-shape invariant      | schema-rail#EXTENSION_DDL + data-lanes#DOCUMENT_LANE |
|  [43]   | tenant provisioning lifecycle (create/destroy → RLS + seed) | server-tier#TENANCY_RLS                |
|  [44]   | per-tenant resource quota / rate bound      | server-tier#TENANCY_RLS                       |
|  [45]   | PG18 WITHOUT OVERLAPS temporal-key shape    | schema-rail#EXTENSION_DDL                     |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface. The budget below is the complete public-owner set; a type outside it is a defect. `[OWNER]` cells fold every extension block and mapping descriptor under the axis owner — `StoreOpCompose` rides axis [11], `TabularDirection`/`TabularSpec`/`AnalyticalTraversal`/`DuckDBOpLogMap`/`RelationSource`/`RelationSchema`/`RelationLane` ride axis [8], `EmbeddingArity`/`EmbeddingIdentity`/`VectorQuery`/`FullTextQuery` ride axis [7], `DbConfig` rides axis [16], `Composite`/`MapComposites`/`Enum`/`MapEnums`/`SqlitePatterns` ride axis [10], `GeoJsonProjection`/`PersistenceResolver`/`GeneratedMessagePackResolver` ride axis [17], `CacheResidence` rides axis [18], `ObjectClient`/`TransferReceipt`/`ObjectTransferFact` ride axis [21], `RlsPolicy`/`TenantProvision`/`TenantQuota`/`TenantReceipt`/`MigrationPlan` ride axis [22], `TemporalShape`/`TemporalKey` ride axis [10] — so the complete-owner claim holds. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped now, never a deferred surface.

| [INDEX] | [AXIS]               | [OWNER]                                          | [KIND]            | [CASES]           |  [STATE]  |
| :-----: | :------------------- | :----------------------------------------------- | :---------------- | :---------------- | :-------: |
|   [1]   | engine + blob        | StoreProfile, StoreRows, BlobRemote              | enum + records    | 6 rows            | FINALIZED |
|   [2]   | lifecycle            | StoreLifecycle, receipts                         | enum + fold       | 5 states          | FINALIZED |
|   [3]   | placement            | StorePlacement                                   | record + fold     | 8 arms            | FINALIZED |
|   [4]   | cross-process        | StoreLeaseRow, StoreLocality                     | record + guard    | 2 lease kinds     |   SPIKE   |
|   [5]   | provisioning         | ExtensionRequirement                             | table + verify    | 11 rows           | FINALIZED |
|   [6]   | lane axis            | DataLane, KvEntry                                | union + fold      | 7 cases           | FINALIZED |
|   [7]   | document/search      | JsonIndex, VectorMetric, FullTextMode            | enums             | 4 · 6 · 4         | FINALIZED |
|   [8]   | geo + analytical     | GeoLayer, TabularExportSpec, TabularDirection    | policy + enum     | concern rows      | FINALIZED |
|   [9]   | identity             | IdentityPolicy                                   | enum              | 3 rows            | FINALIZED |
|  [10]   | schema law           | faults, fingerprint, columns, SchemaDdl, TemporalShape | fault + DDL | 5 codes · 19 ext. · 3 temporal shapes | FINALIZED |
|  [11]   | operation algebra    | StoreOp, StoreFault, StoreRail, StoreOpCompose   | unions + dispatch | 8 ops · 6 faults  | FINALIZED |
|  [12]   | projection egress    | KeysetPage, ProjectionRail                       | record + fold     | 3 filter keys     | FINALIZED |
|  [13]   | bulk lane            | BulkRoute, receipts, deltas                      | enum + receipts   | 3 routes          | FINALIZED |
|  [14]   | interceptor spine    | interceptors, policies, facts                    | capsule + policy  | 4 hooks · 7 kinds | FINALIZED |
|  [15]   | native policy tables | pragmas, facts, compile surface                  | tables + probe    | 10 · 14           | FINALIZED |
|  [16]   | maintenance + gates  | maintenance, functions, extensions, DbConfig     | verbs + gates     | 9 verbs · 8 gates | FINALIZED |
|  [17]   | snapshot protocol    | codecs, compression, hashing, restore            | rows + wire       | 3 · 3 · 5         | FINALIZED |
|  [18]   | cache + indexes      | contribution, result, artifact, benchmark        | capsule + keys    | 1 + 3 indexes     | FINALIZED |
|  [19]   | sync spine           | op kind, log, merge, conflicts                   | vocab + dispatch  | 3 · 4 · 3         |   SPIKE   |
|  [20]   | retention + classes  | policies, classes, guards, evidence              | axes + guards     | 4 · 7 · 5         |   SPIKE   |
|  [21]   | object-store         | ObjectStore, MultipartTransfer, ObjectResidence, ArtifactSyncFeed, RemoteStoreFault | SmartEnum + Union + records | 3 providers · 5 faults | FINALIZED |
|  [22]   | server-tier          | TimescaleProvisioning, SearchProvisioning, ClusterConfig, TenancyModel, TenantProvision, TenantQuota, MigrationBundle | static folds + SmartEnum | 5 clusters · 4 tenancy · 2 lifecycle | FINALIZED |

Comparer accessors (`StoreKeyPolicy`, `SqliteKeyPolicy`, `SnapshotKeyPolicy`, `SyncKeyPolicy`, `RetentionKeyPolicy`, `CacheResidenceKeyPolicy`) ride inside their owner files, one per axis family, package-local. `ObjectStore`, `TenancyModel`, and the data-lanes string-keyed axes (`JsonIndex`/`VectorMetric`/`FullTextMode`/`EmbeddingArity`) reuse the package-wide `StoreKeyPolicy` accessor.

## [6]-[BUILD_ORDER]

Cluster cells use page-local anchor names; proof cells name evidence beyond the standard static/spec gate.

| [INDEX] | [FILE]                   | [CLUSTERS]                                   | [PROOF]                         |
| :-----: | :----------------------- | :------------------------------------------- | :------------------------------ |
|   [1]   | `Stores/Profiles.cs`     | profiles, placement, cross-process law       | profile, placement, locality    |
|   [2]   | `Stores/Lifecycle.cs`    | lifecycle, provisioning                      | transitions, two-process WAL    |
|   [3]   | `Schema/SchemaRail.cs`   | identity, migrations, columns, DDL, codecs   | gates, EF script/optimize       |
|   [4]   | `Lanes/DataLanes.cs`     | lanes, document, search, geo, analytical     | lane admission and attach probe |
|   [5]   | `Native/Sqlite.cs`       | pragmas, compile surface, maintenance, gates | compile probe and bridge seam   |
|   [6]   | `Query/QueryRail.cs`     | operations, projections, bulk, interceptors  | dispatch and fault conversion   |
|   [7]   | `Cache/Indexes.cs`       | L2, result, artifact, benchmark indexes      | paired closure with [8]         |
|   [8]   | `Snapshots/Codecs.cs`    | codecs, compression, protocol, restore       | round-trip, header, restore     |
|   [9]   | `Sync/Collaboration.cs`  | op-log, merge, transport, presence/blob      | merge idempotency, adjudication |
|  [10]   | `Retention/Redaction.cs` | classification, sweeps, export, audits       | sweep, guard, audit binding     |
|  [11]   | `Stores/ServerTier.cs`   | timescale, search, cluster GUC, tenancy/RLS, migration bundle | live-PG18 provisioning roundtrip |
|  [12]   | `Stores/RemoteStores.cs` | object-store, multipart transfer, residence, sync feed | object-store emulator roundtrip |

Seam ordering law:
- Fingerprint slot: `StoreOpenReceipt.SchemaFingerprint` stays bare `ulong` (ledger seam), so [2] precedes [3] with zero forward reference; `SchemaFingerprint` in [3] is the typed owner.
- Wire-context pairing: [7] precedes [8] and the two files gate as one build closure — `PersistenceWireContext` declares the `CacheIndexFact` serializable row while `IndexSurface` consumes the generated context.
- Mutual placement: `StoreProfile` delegate columns consume `StorePlacement` and the placement record carries `StoreProfile` fields — both land in [1], never split.
- Server-tier ordering: `Stores/ServerTier.cs` consumes `ExtensionRequirement` (from [2]) and `SchemaDdl` (from [3]) as settled vocabulary, so [11] follows both with zero forward reference; the `TenancyModel` RLS row consumes the AppHost `TenantContext` and the `redaction-retention#AUDIT_BINDING` category binding as settled.
- Remote-stores ordering: `Stores/RemoteStores.cs` consumes the `BlobRemote`/`BlobRemote.Descriptor` contract (from [1]) and the Compute `ARTIFACT_FRAMES` 64-KiB/Crc32/XxHash128 frame constants plus the `sync-collaboration#OPLOG_CHANGEFEED` op-log as settled, so [12] follows [1] and the snapshot/sync closure with zero re-declared frame width.
- TS_PROJECTION clusters transcribe into the TS workspace under the suite wire law, never into `.cs` files.

## [7]-[FILE_PROCESS]

1. Read this charter end-to-end, then read every page named in the file's TRANSCRIBES cell end-to-end before opening the file.
2. Transcribe every signature fence verbatim; add only file-organization scaffolding — namespace, usings, section separators per the repo file-organization law.
3. Run the collapse scan after every edit: 3+ parallel types, 3+ sibling factories, 3+ repeated switch arms, or 3+ single-call helpers triggers in-place polymorphic collapse, never a new file.
4. Run `uv run python -m tools.assay static fix` then `uv run python -m tools.assay static build` on the touched closure; a busy lease exits 5 and reruns.
5. Author specs per the `testing-cs` skill — law-matrix specs over the page cards; research-row proofs land as named specs in the same pass.
6. Host seams gate through bridge scenarios (`scenarios/extension-load.verify.csx`); a file with no host seam carries no scenario.
7. Transcription conflicts route back to the page owner and the ledger, never into implementation-side redesign.

## [8]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the proof gate, not after each edit.

| [GATE] | [RAIL]                                | [EVIDENCE]                                |
| :----: | :------------------------------------ | :---------------------------------------- |
|  [G1]  | `dotnet restore --force-evaluate`     | lockfile closure regenerates; zero NU1004 |
|  [G2]  | `api doctor --strict` + `api resolve` | admissions resolve assay assets           |
|  [G3]  | `static plan`                         | closure owners and triggers land in notes |
|  [G4]  | `static build`                        | leased build green; error grep empty      |
|  [G5]  | `test run` Persistence target         | specs and research-row proofs pass        |
|  [G6]  | `bridge verify` extension scenario    | live RhinoWIP scenario facts pass         |
|  [G7]  | `mmdc` architecture render            | local diagram render exits zero           |

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

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. `[CATALOGUE]` keys omit the `api-` prefix and `.md` suffix; `[STATUS]` is `catalogue-pending`, `app-root-pending`, `tests-only`, or `admitted`.

| [INDEX] | [PACKAGE]                                              | [PAGE]              | [CATALOGUE]                | [STATUS]          |
| :-----: | :----------------------------------------------------- | :------------------ | :------------------------- | :---------------- |
|   [1]   | DuckDB.NET.Data.Full                                   | data-lanes          | duckdb                     | admitted          |
|   [2]   | EFCore.NamingConventions                               | schema-rail         | ef-naming                  | admitted          |
|   [3]   | K4os.Compression.LZ4                                   | snapshot-codecs     | lz4                        | admitted          |
|   [4]   | MessagePack                                            | snapshot-codecs     | messagepack                | admitted          |
|   [5]   | MessagePackAnalyzer                                    | snapshot-codecs     | messagepack-analyzer       | admitted          |
|   [6]   | Microsoft.Data.Sqlite                                  | native-sqlite       | sqlite                     | admitted          |
|   [7]   | Microsoft.EntityFrameworkCore.Design                   | schema-rail         | ef-design                  | admitted          |
|   [8]   | Microsoft.EntityFrameworkCore.Sqlite                   | store-profiles      | ef-sqlite                  | admitted          |
|   [9]   | Microsoft.Extensions.Caching.Hybrid                   | cache-indexes       | hybrid-cache AppHost       | admitted          |
|  [10]   | Microsoft.Extensions.Compliance.Redaction             | redaction-retention | redaction                  | admitted          |
|  [11]   | NetTopologySuite.IO.GeoJSON4STJ                        | snapshot-codecs     | nts-io                     | admitted          |
|  [12]   | NetTopologySuite.IO.GeoPackage                         | data-lanes          | nts-io                     | admitted          |
|  [13]   | NodaTime                                              | store-profiles      | nodatime                   | admitted          |
|  [14]   | NodaTime.Serialization.SystemTextJson                 | snapshot-codecs     | nodatime-json              | admitted          |
|  [15]   | Npgsql                                                 | store-profiles      | npgsql                     | admitted          |
|  [16]   | Npgsql.EntityFrameworkCore.PostgreSQL                  | store-profiles      | npgsql-ef                  | admitted          |
|  [17]   | Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | data-lanes          | nts-ef                     | admitted          |
|  [18]   | Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime         | data-lanes          | npgsql-ef-nodatime         | admitted          |
|  [19]   | Npgsql.OpenTelemetry                                   | query-rail          | npgsql-otel                | admitted          |
|  [20]   | Pgvector.EntityFrameworkCore                           | data-lanes          | pgvector-ef                | admitted          |
|  [21]   | Sep                                                   | data-lanes          | sep                        | admitted          |
|  [22]   | SQLitePCLRaw.bundle_e_sqlite3                          | native-sqlite       | sqlitepcl                  | admitted          |
|  [23]   | System.IO.Hashing                                     | schema-rail         | hashing                    | admitted          |
|  [24]   | Thinktecture.Runtime.Extensions.EntityFrameworkCore10  | schema-rail         | thinktecture-serialization | admitted          |
|  [25]   | Thinktecture.Runtime.Extensions.Json                  | snapshot-codecs     | thinktecture-serialization | admitted          |
|  [26]   | Thinktecture.Runtime.Extensions.MessagePack            | snapshot-codecs     | thinktecture-serialization | admitted          |
|  [27]   | AWSSDK.S3                                              | remote-stores       | aws-s3                     | admitted          |
|  [28]   | Azure.Storage.Blobs                                    | remote-stores       | azure-blobs                | admitted          |
|  [29]   | Google.Cloud.Storage.V1                               | remote-stores       | gcs-storage                | admitted          |
|  [30]   | StackExchange.Redis                                    | cache-indexes       | stackexchange-redis        | admitted          |
|  [31]   | Microsoft.Extensions.Caching.StackExchangeRedis        | cache-indexes       | caching-redis              | admitted          |
|  [32]   | BenchmarkDotNet                                        | data-lanes          | pending                    | catalogue-pending |
|  [33]   | Verify.XunitV3                                         | snapshot-codecs     | pending                    | catalogue-pending |
|  [34]   | SharpFuzz                                              | snapshot-codecs     | pending                    | catalogue-pending |

## [11]-[REFINEMENT_HORIZON]

Deepening targets, each open against a named page RESEARCH probe: the GIS lanes pushed to full pipeline capability — heterogeneous ingestion through GeoPackage/GeoJSON/PostGIS rows into the analytical lane — gated by data-lanes#RESEARCH `[MANAGED_REFINE_OPS]` constructive-geometry forms and `[REGISTER_TABLE_FUNCTION]` chunk-fill; sync topologies rehearsed against the hub and collaboration concepts, gated by sync-collaboration#RESEARCH `[LIVE_REPLICATION]` publication-parameter and conflict-stat facts on a live PG18 server; the extension gates — vec0, SQLCipher, sqlean — resolved from native-sqlite#RESEARCH `[EXTENSION_LOADING]` live-load and hardened-runtime dlopen probes into settled rows; server self-provisioning rows exercised against store-profiles#PROVISIONING_ROWS — postgresql.conf preload fragments, pg_hba fragments, role grants, and server-side extension enablement — when the first server root lands, with the redaction-retention#RESEARCH `[PGAUDIT_CATEGORIES]` session-audit category semantics verified against the `AuditBinding.Categories` rows under `shared_preload_libraries=pgaudit` on that same live PG18 root; the server tier exercised against server-tier#RESEARCH `[SERVER_PROVISIONING_PROBE]` (create_hypertable/continuous-aggregate/columnstore apply contract, diskann-over-`halfvec` `storage_layout=memory_optimized`, pg_search BM25 `@@@`/`paradedb.score`, and the RLS `current_setting('rasm.tenant')` per-tenant isolation), `[CLUSTER_CONFIG_PORTABILITY]` (io_uring vs worker on the deploy-image kernel), and `[MIGRATION_BUNDLE_PROBE]` (the self-contained bundle executable and the NOT-VALID/NOT-ENFORCED lock-light apply-then-validate contract) on the colima timescaledb/paradedb PG18 image; the cloud object-store tier exercised against remote-stores#RESEARCH `[OBJECT_ROUNDTRIP]` (the multipart-resume skip-on-committed-ETag and conditional-write `412` surface), `[OBJECT_MEMBER_SPELLINGS]` (the get/head/list/delete member set decompile-verified at admission), and `[REMOTE_FAULT_LIFT]` (the three boundary-exception types) against the MinIO/Azurite/fake-gcs-server emulators; the Redis L2 residence exercised against the local redis-server roundtrip behind `cache-indexes#L2_CONTRIBUTION`. The bar: any data product — offline-first field store, telemetry lake, geospatial sync hub, multi-tenant SaaS, cloud object-store sync hub — composes from rows with zero app-side persistence code.

Testing-infrastructure horizon: the snapshot-codec decode and sealed-artifact rejection ladder ride `SharpFuzz` `Fuzzer.OutOfProcess.Run` over the restore lane's untrusted-data boundary (`NEEDS-ADMISSION`); the `StoreProfile.SqliteMemory` placement is the deterministic in-memory store row and the DuckDB analytical lane the live-engine round-trip; bulk-write lane throughput lands on the BenchmarkDotNet rail.
