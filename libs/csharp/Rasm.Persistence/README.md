# [PERSISTENCE]

`Rasm.Persistence` is the APP-PLATFORM durable-state spine: a host-neutral C# backbone that owns every durable store, lane, and rail. It consumes AppHost ports (`clock`, `telemetry`, `receipts`, `drain`, `classification`) and Compute artifact frames as settled vocabulary. The package owns the store-profile engine axis, the lifecycle/lease/placement ceremony, cloud object-store residence, the self-provisioned PostgreSQL server tier, the schema/query rails, the embedded-SQLite floor, snapshot codecs, cache indexes, sync and collaboration transports, version control with CRDT and time-travel, the federated entity graph, provenance, annotation, catalog/cost, schedule interchange, and redaction/retention. The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[ENGINE](.planning/Store/engine.md)
- [02]-[PROFILES](.planning/Store/profiles.md)
- [03]-[REMOTE](.planning/Store/remote.md)
- [04]-[PROVISIONING](.planning/Store/provisioning.md)
- [05]-[TENANCY](.planning/Store/tenancy.md)
- [06]-[ENCRYPTION](.planning/Store/encryption.md)
- [07]-[QUALITY](.planning/Store/quality.md)
- [08]-[IDENTITY](.planning/Schema/identity.md)
- [09]-[MIGRATION](.planning/Schema/migration.md)
- [10]-[DDL](.planning/Schema/ddl.md)
- [11]-[CONVERTERS](.planning/Schema/converters.md)
- [12]-[RAIL](.planning/Query/rail.md)
- [13]-[LANES](.planning/Query/lanes.md)
- [14]-[CACHE](.planning/Query/cache.md)
- [15]-[FEDERATION](.planning/Query/federation.md)
- [16]-[TRANSACTION](.planning/Query/transaction.md)
- [17]-[PIPELINE](.planning/Query/pipeline.md)
- [18]-[COMMITS](.planning/Version/commits.md)
- [19]-[TIMETRAVEL](.planning/Version/timetravel.md)
- [20]-[DIFF](.planning/Version/diff.md)
- [21]-[PROVENANCE](.planning/Version/provenance.md)
- [22]-[SNAPSHOTS](.planning/Version/snapshots.md)
- [23]-[RETENTION](.planning/Version/retention.md)
- [24]-[RECOVERY](.planning/Version/recovery.md)
- [25]-[COLLABORATION](.planning/Sync/collaboration.md)
- [26]-[ANNOTATION](.planning/Sync/annotation.md)
- [27]-[SCHEDULE](.planning/Sync/schedule.md)
- [28]-[EGRESS](.planning/Sync/egress.md)
- [29]-[COORDINATION](.planning/Sync/coordination.md)

## [02]-[DOMAIN_PACKAGES]

Every Persistence-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below.

[POSTGRES_EF]:
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- `Npgsql.OpenTelemetry`
- `EFCore.NamingConventions`
- `linq2db.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Pgvector`
- `Pgvector.EntityFrameworkCore`
- `NetTopologySuite.IO.GeoJSON4STJ`
- `NetTopologySuite.IO.GeoPackage`
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`

[SERVER_EXTENSIONS]:
PostgreSQL 18 server-tier extensions: no managed assembly, provisioned through raw SQL, preload-gated or type/index-registered per the `Store/server#CLUSTER_CONFIG` row. Each carries a folder `.api/` catalogue of its SQL surface.
- `timescaledb` (hypertable, continuous-aggregate, retention, columnstore; bgworker scheduler)
- `pgvectorscale` (diskann access method over a pgvector column)
- `pg_search` (ParadeDB bm25 access method; pdb query-builder schema)
- `pg_cron` (database-local cron for SQL maintenance jobs)
- `pg_partman` (declarative range/list partition maintenance)
- `pg_squeeze` (lock-light table-bloat reclamation)
- `pg_jsonschema` (server-side JSON Schema CHECK validation)
- `pgaudit` (session/object audit logging)

[SQLITE]:
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Data.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlite3`

[COLUMNAR_ARROW]:
- `DuckDB.NET.Data.Full`
- `Apache.Arrow`
- `Apache.Arrow.Flight`
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Compression` (admitted concrete `ICompressionCodecFactory` for `Lz4Frame`/`Zstd` Arrow-IPC compression; pure-managed, transitives `K4os.Compression.LZ4.Streams` + `ZstdSharp.Port` — distinct from the `K4os.Compression.LZ4` snapshot codec)
- `FastCDC.Net`

[OBJECT_STORE]:
- `AWSSDK.S3`
- `Azure.Storage.Blobs`
- `Google.Cloud.Storage.V1`

[ENCRYPTION_KMS]:
- `AWSSDK.KeyManagementService`
- `Azure.Security.KeyVault.Keys`
- `Google.Cloud.Kms.V1`

[REDIS]:
- `StackExchange.Redis`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.Extensions.Caching.Hybrid`

[VERSIONING_SYNC]:
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- `Speckle.Sdk`
- `Speckle.Objects`

[STREAMING_EGRESS]:
- `Confluent.Kafka`
- `CloudNative.CloudEvents`
- `CloudNative.CloudEvents.Kafka`
- `CloudNative.CloudEvents.SystemTextJson`

[WIRE_SERIALIZATION]:
- `MessagePack`
- `MessagePackAnalyzer`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `K4os.Compression.LZ4`
- `Sep`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries Persistence consumes; these are owned at the monorepo substrate layer. Package charters and API evidence live in `libs/csharp/.planning/README.md` and the adjacent `.api/` folder.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
- `BenchmarkDotNet`
- `SharpFuzz`
- `NodaTime.Testing`
