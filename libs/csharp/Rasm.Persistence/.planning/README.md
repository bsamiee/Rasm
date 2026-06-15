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

## [2]-[WIRE_PAGES]

snapshot-codecs · sync-collaboration (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

linq2db core (LinqToDB.dll) merge/output surfaces ride the bridge package and get a page only if the core package is admitted; app-root JsonPatch catalogued at app-root creation.

## [4]-[GAP_LEDGER]

Every ledger row is closed; `[OWNER]` names the page that absorbed the gap and `[STATE]` carries the `CLOSED` queryable lens — a gap that is not `CLOSED` is an open defect, never silent.

| [INDEX] | [GAP]                                       | [OWNER]                                       | [STATE] |
| :-----: | :------------------------------------------ | :-------------------------------------------- | :-----: |
|   [1]   | database execution-strategy rows            | store-profiles + query-rail                   | CLOSED  |
|   [2]   | pooled context factory                      | query-rail                                    | CLOSED  |
|   [3]   | optimistic concurrency tokens               | schema-rail + query-rail                      | CLOSED  |
|   [4]   | reference-data seeding law                  | store-profiles                                | CLOSED  |
|   [5]   | PostgreSQL maintenance symmetry             | store-profiles                                | CLOSED  |
|   [6]   | receipted restore choreography              | snapshot-codecs + store-profiles              | CLOSED  |
|   [7]   | WAL, busy retry, opener, HLC lease law      | store-profiles + native-sqlite                | CLOSED  |
|   [8]   | clock seam for durable stamping             | redaction-retention + sync-collaboration      | CLOSED  |
|   [9]   | identity axis and SQLite uuidv7             | schema-rail                                   | CLOSED  |
|  [10]   | bulk invalidation changefeed                | query-rail                                    | CLOSED  |
|  [11]   | store-side classification enforcement       | redaction-retention                           | CLOSED  |
|  [12]   | PostgreSQL 18 adoption rows                 | schema-rail + query-rail + sync-collaboration | CLOSED  |
|  [13]   | extension axis and PostGIS lanes            | data-lanes + schema-rail + store-profiles     | CLOSED  |
|  [14]   | e_sqlite3 compile flags and extension gates | native-sqlite                                 | CLOSED  |
|  [15]   | M1-M5 sync rows                             | sync-collaboration                            | CLOSED  |
|  [16]   | RFC 6902 HTTP delta fallback                | sync-collaboration                            | CLOSED  |
|  [17]   | BlobRemote frame constants                  | sync-collaboration                            | CLOSED  |
|  [18]   | ephemeral presence rows                     | sync-collaboration                            | CLOSED  |
|  [19]   | UI conflict receipt projection              | sync-collaboration                            | CLOSED  |
|  [20]   | Parquet and DuckDB analytical rows          | data-lanes                                    | CLOSED  |
|  [21]   | jsonb canonical law                         | schema-rail + query-rail                      | CLOSED  |
|  [22]   | GeoJSON, GeoPackage, canonical geometry     | data-lanes + snapshot-codecs                  | CLOSED  |
|  [23]   | snapshot diff projection                    | snapshot-codecs                               | CLOSED  |
|  [24]   | compute artifact blob routing               | cache-indexes                                 | CLOSED  |
|  [25]   | filesystem-locality admission guard         | store-profiles                                | CLOSED  |
|  [26]   | SQLCipher research gate                     | native-sqlite                                 | CLOSED  |
|  [27]   | pgaudit and legal-hold ordering             | redaction-retention                           | CLOSED  |
|  [28]   | Thinktecture codec rows                     | schema-rail + snapshot-codecs                 | CLOSED  |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of naive LOC. One owner per axis, one entrypoint family per rail; a new feature is a row, case, or policy value — never a new surface. The budget below is the complete public-owner set; a type outside it is a defect. `[OWNER]` cells fold every extension block and mapping descriptor under the axis owner — `StoreOpCompose` rides axis [11], `TabularDirection`/`TabularSpec`/`AnalyticalTraversal`/`DuckDBOpLogMap` ride axis [8], `DbConfig` rides axis [16], `Composite`/`MapComposites` ride axis [10] — so the complete-owner claim holds. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual native, bridge, or live-server probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped now, never a deferred surface.

| [INDEX] | [AXIS]               | [OWNER]                                          | [KIND]            | [CASES]           |  [STATE]  |
| :-----: | :------------------- | :----------------------------------------------- | :---------------- | :---------------- | :-------: |
|   [1]   | engine + blob        | StoreProfile, StoreRows, BlobRemote              | enum + records    | 6 rows            | FINALIZED |
|   [2]   | lifecycle            | StoreLifecycle, receipts                         | enum + fold       | 5 states          | FINALIZED |
|   [3]   | placement            | StorePlacement                                   | record + fold     | 8 arms            | FINALIZED |
|   [4]   | cross-process        | StoreLeaseRow, StoreLocality                     | record + guard    | 2 lease kinds     |   SPIKE   |
|   [5]   | provisioning         | ExtensionRequirement                             | table + verify    | 7 rows            |   SPIKE   |
|   [6]   | lane axis            | DataLane, KvEntry                                | union + fold      | 7 cases           | FINALIZED |
|   [7]   | document/search      | JsonIndex, VectorMetric, FullTextMode            | enums             | 4 · 4 · 4         | FINALIZED |
|   [8]   | geo + analytical     | GeoLayer, TabularExportSpec, TabularDirection    | policy + enum     | concern rows      |   SPIKE   |
|   [9]   | identity             | IdentityPolicy                                   | enum              | 3 rows            | FINALIZED |
|  [10]   | schema law           | faults, fingerprint, columns, SchemaDdl          | fault + DDL       | 5 codes · 19 ext. |   SPIKE   |
|  [11]   | operation algebra    | StoreOp, StoreFault, StoreRail, StoreOpCompose   | unions + dispatch | 8 ops · 6 faults  | FINALIZED |
|  [12]   | projection egress    | KeysetPage, ProjectionRail                       | record + fold     | 3 filter keys     | FINALIZED |
|  [13]   | bulk lane            | BulkRoute, receipts, deltas                      | enum + receipts   | 3 routes          |   SPIKE   |
|  [14]   | interceptor spine    | interceptors, policies, facts                    | capsule + policy  | 4 hooks · 7 kinds |   SPIKE   |
|  [15]   | native policy tables | pragmas, facts, compile surface                  | tables + probe    | 10 · 14           |   SPIKE   |
|  [16]   | maintenance + gates  | maintenance, functions, extensions, DbConfig     | verbs + gates     | 9 verbs · 8 gates |   SPIKE   |
|  [17]   | snapshot protocol    | codecs, compression, hashing, restore            | rows + wire       | 3 · 3 · 5         |   SPIKE   |
|  [18]   | cache + indexes      | contribution, result, artifact, benchmark        | capsule + keys    | 1 + 3 indexes     | FINALIZED |
|  [19]   | sync spine           | op kind, log, merge, conflicts                   | vocab + dispatch  | 3 · 4 · 3         |   SPIKE   |
|  [20]   | retention + classes  | policies, classes, guards, evidence              | axes + guards     | 4 · 7 · 5         |   SPIKE   |

Comparer accessors (`StoreKeyPolicy`, `SqliteKeyPolicy`, `SnapshotKeyPolicy`, `SyncKeyPolicy`, `RetentionKeyPolicy`) ride inside their owner files, one per axis family, package-local.

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
7. Transcription conflicts route back to the page owner and the ledger, never into implementation-side redesign.

## [8]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit.

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

[STORE_LANES]:

API keys omit the `api-` prefix and `.md` suffix. `[VERSION]` records the props/lock-resolved pin so the charter is the one place version truth lives; a value disagreeing with `Directory.Packages.props` plus the lockfile is a doc-truth drift.

| [INDEX] | [PACKAGE]                                              | [VERSION] | [OWNER]         | [API]                      |
| :-----: | :----------------------------------------------------- | :-------- | :-------------- | :------------------------- |
|   [1]   | DuckDB.NET.Data.Full                                   | 1.5.3     | data-lanes      | duckdb                     |
|   [2]   | EFCore.NamingConventions                               | 10.0.1    | schema-rail     | ef-naming                  |
|   [3]   | linq2db.EntityFrameworkCore                            | 10.4.0    | query-rail      | linq2db-ef                 |
|   [4]   | Microsoft.Data.Sqlite                                  | 10.0.9    | native-sqlite   | sqlite                     |
|   [5]   | Microsoft.EntityFrameworkCore.Design                   | 10.0.9    | schema-rail     | ef-design                  |
|   [6]   | Microsoft.EntityFrameworkCore.Sqlite                   | 10.0.9    | store-profiles  | ef-sqlite                  |
|   [7]   | NetTopologySuite.IO.GeoJSON4STJ                        | 4.0.0     | snapshot-codecs | nts-io                     |
|   [8]   | NetTopologySuite.IO.GeoPackage                         | 2.0.0     | data-lanes      | nts-io                     |
|   [9]   | Npgsql                                                 | 10.0.3    | store-profiles  | npgsql                     |
|  [10]   | Npgsql.EntityFrameworkCore.PostgreSQL                  | 10.0.2    | store-profiles  | npgsql-ef                  |
|  [11]   | Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | 10.0.2    | data-lanes      | nts-ef                     |
|  [12]   | Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime         | 10.0.2    | data-lanes      | npgsql-ef-nodatime         |
|  [13]   | Npgsql.OpenTelemetry                                   | 10.0.3    | query-rail      | npgsql-otel                |
|  [14]   | Pgvector.EntityFrameworkCore                           | 0.3.0     | data-lanes      | pgvector-ef                |
|  [15]   | SQLitePCLRaw.bundle_e_sqlite3                          | 3.0.3     | native-sqlite   | sqlitepcl                  |
|  [16]   | Thinktecture.Runtime.Extensions.EntityFrameworkCore10  | 10.2.0    | schema-rail     | thinktecture-serialization |

[SNAPSHOTS_AND_SUPPORT]:

| [INDEX] | [PACKAGE]                                   | [VERSION] | [OWNER]             | [API]                      |
| :-----: | :------------------------------------------ | :-------- | :------------------ | :------------------------- |
|   [1]   | K4os.Compression.LZ4                        | 1.3.8     | snapshot-codecs     | lz4                        |
|   [2]   | MessagePack                                 | 3.1.7     | snapshot-codecs     | messagepack                |
|   [3]   | MessagePackAnalyzer                         | 3.1.7     | snapshot-codecs     | messagepack-analyzer       |
|   [4]   | Microsoft.Extensions.Caching.Hybrid         | 10.7.0    | cache-indexes       | hybrid-cache AppHost       |
|   [5]   | Microsoft.Extensions.Compliance.Redaction   | 10.7.0    | redaction-retention | redaction                  |
|   [6]   | NodaTime                                    | 3.3.2     | store-profiles      | nodatime                   |
|   [7]   | NodaTime.Serialization.SystemTextJson       | 1.4.0     | snapshot-codecs     | nodatime-json              |
|   [8]   | Sep                                         | 0.14.1    | data-lanes          | sep                        |
|   [9]   | System.IO.Hashing                           | 10.0.9    | schema-rail         | hashing                    |
|  [10]   | Thinktecture.Runtime.Extensions.Json        | 10.2.0    | snapshot-codecs     | thinktecture-serialization |
|  [11]   | Thinktecture.Runtime.Extensions.MessagePack | 10.2.0    | snapshot-codecs     | thinktecture-serialization |

## [11]-[REFINEMENT_HORIZON]

Deepening targets: the GIS lanes pushed to full pipeline capability (heterogeneous source ingestion through GeoPackage/GeoJSON/PostGIS rows into the analytical lane); sync topologies rehearsed against the hub and collaboration concepts; the extension gates (vec0, SQLCipher, sqlean) resolved from probes into settled rows; server self-provisioning rows exercised when the first server root lands. The bar: any data product — offline-first field store, telemetry lake, geospatial sync hub — composes from rows with zero app-side persistence code.
