# [PERSISTENCE]

`Rasm.Persistence` is the APP-PLATFORM durable-state spine — the host-neutral C# backbone that owns every durable store, lane, and rail and consumes AppHost ports (clock, telemetry, receipts, drain, classification) and Compute artifact frames as settled vocabulary. It owns the store-profile engine axis, the lifecycle/lease/placement ceremony, cloud object-store residence, the self-provisioned PostgreSQL server tier, the schema/query rails, the embedded-SQLite floor, snapshot codecs, cache indexes, sync and collaboration transports, version control with CRDT and time-travel, the federated entity graph, provenance, annotation, catalog/cost, schedule interchange, and redaction/retention. This README routes the `.planning/` design pages and lists every external package the folder uses; the sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- stores: [profiles](.planning/stores/profiles.md)
- stores-remote: [object-store](.planning/stores-remote/object-store.md)
- stores-server: [provisioning](.planning/stores-server/provisioning.md)
- schema: [schema-rail](.planning/schema/schema-rail.md)
- modalities: [data-lanes](.planning/modalities/data-lanes.md)
- query: [query-rail](.planning/query/query-rail.md)
- engine: [sqlite](.planning/engine/sqlite.md)
- snapshots: [codecs](.planning/snapshots/codecs.md)
- cache: [indexes](.planning/cache/indexes.md)
- sync: [collaboration](.planning/sync/collaboration.md)
- versioning: [version-control](.planning/versioning/version-control.md)
- federation: [federation](.planning/federation/federation.md)
- provenance: [provenance](.planning/provenance/provenance.md)
- annotation: [annotation](.planning/annotation/annotation.md)
- schedule: [schedule-interchange](.planning/schedule/schedule-interchange.md)
- retention: [redaction-retention](.planning/retention/redaction-retention.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one C# manifest and never pinned here; admissions land here from the folder's ideas and tasks.

[EF_CORE_PROVIDERS]:
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL
- Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite
- Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime
- EFCore.NamingConventions
- linq2db.EntityFrameworkCore

[ENGINES_DRIVERS]:
- Microsoft.Data.Sqlite
- SQLitePCLRaw.bundle_e_sqlite3
- Npgsql
- Npgsql.OpenTelemetry
- DuckDB.NET.Data.Full

[SPATIAL_VECTOR_TEMPORAL]:
- Pgvector
- Pgvector.EntityFrameworkCore
- NetTopologySuite.IO.GeoJSON4STJ
- NetTopologySuite.IO.GeoPackage
- NodaTime
- NodaTime.Serialization.SystemTextJson

[SERIALIZATION_COMPRESSION_HASHING]:
- Thinktecture.Runtime.Extensions
- Thinktecture.Runtime.Extensions.Json
- Thinktecture.Runtime.Extensions.MessagePack
- Thinktecture.Runtime.Extensions.EntityFrameworkCore10
- MessagePack
- MessagePackAnalyzer
- K4os.Compression.LZ4
- System.IO.Hashing
- Sep

[CACHE_OBJECT_STORE]:
- Microsoft.Extensions.Caching.Hybrid
- Microsoft.Extensions.Caching.StackExchangeRedis
- StackExchange.Redis
- AWSSDK.S3
- Azure.Storage.Blobs
- Google.Cloud.Storage.V1

[COMPLIANCE_SYNC]:
- Microsoft.Extensions.Compliance.Redaction
- Microsoft.AspNetCore.JsonPatch.SystemTextJson

[FUNCTIONAL_CORE]:
- LanguageExt.Core

[ANALYTICAL_EGRESS]:
- Apache.Arrow
- Apache.Arrow.Flight
- Apache.Arrow.Adbc

[CHUNKING]:
- FastCDC.Net

[TESTING]:
- Verify.XunitV3
- BenchmarkDotNet
- SharpFuzz
- NodaTime.Testing
