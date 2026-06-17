# [PERSISTENCE]

`Rasm.Persistence` is one durable-state spine with zero consumers; the implementation is full-capability with no holding back. The `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The spine owns store profiles, data lanes, schema and query rails, native SQLite truth, snapshot codecs, cache indexes, sync and collaboration transports, redaction and retention, the cloud object-store and self-provisioned server tiers, and the BIM-currency legs — version control, federation, provenance, annotation, catalog/cost, and schedule interchange — each one rail per concern consuming AppHost ports (clock, telemetry, receipts, drain, classification) as settled vocabulary. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                    | [OWNS]                                                               |
| :-----: | :-------------------------------------------------------- | :------------------------------------------------------------------- |
|   [1]   | [store-profiles](Stores/.planning/store-profiles.md)             | engine axis, row records, cross-process law                          |
|   [2]   | [data-lanes](Lanes/.planning/data-lanes.md)                      | lane map, geometry, analytical, extensions                           |
|   [3]   | [schema-rail](Schema/.planning/schema-rail.md)                   | identity, migrations, generated columns, DDL                         |
|   [4]   | [query-rail](Query/.planning/query-rail.md)                      | operations, contexts, bulk lane, projection, interceptors            |
|   [5]   | [native-sqlite](Native/.planning/native-sqlite.md)              | e_sqlite3, WAL, loadable extensions, encryption gate                 |
|   [6]   | [snapshot-codecs](Snapshots/.planning/snapshot-codecs.md)       | codecs, compression, hashing, restore, wire contracts                |
|   [7]   | [cache-indexes](Cache/.planning/cache-indexes.md)               | cache contribution, serializers, result and artifact indexes         |
|   [8]   | [sync-collaboration](Sync/.planning/sync-collaboration.md)      | sync transports, op-log, diffs, presence, conflicts                  |
|   [9]   | [redaction-retention](Retention/.planning/redaction-retention.md)   | retention, classification, audit binding                         |
|  [10]   | [remote-stores](Stores/.planning/remote-stores.md)               | object-store axis, multipart transfer, residence, sync feed          |
|  [11]   | [server-tier](Stores/.planning/server-tier.md)                   | time-series, search, cluster GUC, tenancy/RLS, migration bundle      |
|  [12]   | [version-control](Versioning/.planning/version-control.md)       | commit-DAG, CRDT algebra, time-travel, structural diff/merge         |
|  [13]   | [federation](Federation/.planning/federation.md)                 | entity graph, element-set algebra, links, rule plan, fusion, planner |
|  [14]   | [provenance](Provenance/.planning/provenance.md)                 | causal DAG, attested ledger, lineage-scoped CDC                      |
|  [15]   | [annotation](Annotation/.planning/annotation.md)                 | anchor algebra, BCF protocol, CDE OAuth2 sync                        |
|  [16]   | [catalog-cost](Catalog/.planning/catalog-cost.md)                 | classification catalogs, cost-code, formula rollup                   |
|  [17]   | [schedule-interchange](Schedule/.planning/schedule-interchange.md) | P6/MSP import, task-element link, 4D state                           |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. `[CATALOGUE]` keys omit the `api-` prefix and `.md` suffix. `[STATUS]` is one of `admitted`, `catalogue-pending`, `app-root-pending`, `tests-only`. The linq2db core merge/output surface rides the bridge package and earns a catalogue only on core-package admission; app-root JsonPatch catalogues at app-root creation.

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
|   [9]   | Microsoft.Extensions.Caching.Hybrid                    | cache-indexes       | hybrid-cache AppHost       | admitted          |
|  [10]   | Microsoft.Extensions.Compliance.Redaction              | redaction-retention | redaction                  | admitted          |
|  [11]   | NetTopologySuite.IO.GeoJSON4STJ                        | snapshot-codecs     | nts-io                     | admitted          |
|  [12]   | NetTopologySuite.IO.GeoPackage                         | data-lanes          | nts-io                     | admitted          |
|  [13]   | NodaTime                                               | store-profiles      | nodatime                   | admitted          |
|  [14]   | NodaTime.Serialization.SystemTextJson                  | snapshot-codecs     | nodatime-json              | admitted          |
|  [15]   | Npgsql                                                 | store-profiles      | npgsql                     | admitted          |
|  [16]   | Npgsql.EntityFrameworkCore.PostgreSQL                  | store-profiles      | npgsql-ef                  | admitted          |
|  [17]   | Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | data-lanes          | nts-ef                     | admitted          |
|  [18]   | Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime         | data-lanes          | npgsql-ef-nodatime         | admitted          |
|  [19]   | Npgsql.OpenTelemetry                                   | query-rail          | npgsql-otel                | admitted          |
|  [20]   | Pgvector.EntityFrameworkCore                           | data-lanes          | pgvector-ef                | admitted          |
|  [21]   | Sep                                                    | data-lanes          | sep                        | admitted          |
|  [22]   | SQLitePCLRaw.bundle_e_sqlite3                          | native-sqlite       | sqlitepcl                  | admitted          |
|  [23]   | System.IO.Hashing                                      | schema-rail         | hashing                    | admitted          |
|  [24]   | Thinktecture.Runtime.Extensions.EntityFrameworkCore10  | schema-rail         | thinktecture-serialization | admitted          |
|  [25]   | Thinktecture.Runtime.Extensions.Json                   | snapshot-codecs     | thinktecture-serialization | admitted          |
|  [26]   | Thinktecture.Runtime.Extensions.MessagePack            | snapshot-codecs     | thinktecture-serialization | admitted          |
|  [27]   | AWSSDK.S3                                              | remote-stores       | aws-s3                     | admitted          |
|  [28]   | Azure.Storage.Blobs                                    | remote-stores       | azure-blobs                | admitted          |
|  [29]   | Google.Cloud.Storage.V1                                | remote-stores       | gcs-storage                | admitted          |
|  [30]   | StackExchange.Redis                                    | cache-indexes       | stackexchange-redis        | admitted          |
|  [31]   | Microsoft.Extensions.Caching.StackExchangeRedis        | cache-indexes       | caching-redis              | admitted          |
|  [32]   | BenchmarkDotNet                                        | data-lanes          | —                          | catalogue-pending |
|  [33]   | Verify.XunitV3                                         | snapshot-codecs     | —                          | catalogue-pending |
|  [34]   | SharpFuzz                                              | snapshot-codecs     | —                          | tests-only        |
|  [35]   | linq2db core (LinqToDB.dll)                            | query-rail          | —                          | catalogue-pending |
|  [36]   | Microsoft.AspNetCore.JsonPatch.SystemTextJson          | sync-collaboration  | —                          | app-root-pending  |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                    | [RAIL]                               | [EVIDENCE]                                                 |
| :-----: | :------------------------ | :----------------------------------- | :--------------------------------------------------------- |
|  [G1]   | locked restore            | Assay restore rail                   | lockfile closure regenerates; zero NU1004                  |
|  [G2]   | API catalogue resolve     | `assay api` doctor/resolve           | admissions resolve in `.api` or doctrine pages             |
|  [G3]   | static plan + build       | Assay static rail                    | leased build green; zero `': error '` lines                |
|  [G4]   | spec law-matrix           | Assay test rail (Persistence target) | `testing-cs` law-matrix specs and research-row proofs pass |
|  [G5]   | host-seam bridge scenario | Assay bridge rail                    | extension-load scenario passes under live RhinoWIP         |
|  [G6]   | page diagram render       | local mermaid-cli                    | page diagrams render through the local renderer            |
