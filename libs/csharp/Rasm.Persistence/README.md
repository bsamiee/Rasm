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
- `pocketken.H3` (managed Uber-H3 v4 hexagonal hierarchical geospatial indexing — the in-process counterpart to the `h3-pg` server extension so the same cell id is computed identically at ingest and in PostgreSQL; pure-managed AnyCPU over the admitted `NetTopologySuite`)
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`

[STORE_BACKENDS]:
Dedicated store-backend clients reaching beyond the single-PG / embedded-SQLite tier, each a distinct backend class with no overlap.
- `ClickHouse.Driver` (official high-ingest distributed columnar OLAP client — thread-safe primary API + ADO.NET, binary bulk insert, OpenTelemetry diagnostics; the scale-out billion-row aggregation lane beyond in-PG TimescaleDB + embedded DuckDB)
- `ScyllaDBCSharpDriver` (modern CQL wide-column client — shard/tablet-aware, prepared/batch, Linq2Cql + ADO.NET — driving both ScyllaDB and Apache Cassandra over the one CQL protocol)
- `Qdrant.Client` (official dedicated scale-out / distributed vector-store retrieval client — quantization, server-side payload filtering, named-vector multi-tenant collections, snapshots; the billion-scale ANN store class beyond the in-PG `pgvector`/`pgvectorscale` tier)
- `DeltaLake.Net` (delta-rs FFI Delta Lake managed read/write client — partitioning, S3/Azure/GCS storage, DataFusion query over the Arrow C Data Interface; ships osx-arm64 `delta_kernel`/`delta_rs` dylibs — external Delta-warehouse interop beside the self-hosted DuckLake catalog)

[EMBEDDED_KV]:
Embedded high-throughput KV/log engines beyond the SQLite relational B-tree floor; both ship osx-arm64 native dylibs.
- `rocksdb` (curiosity-ai line — embedded LSM-tree write-optimized KV/log engine: column families, WriteBatch, snapshots, merge operators, tiered compaction, transactions)
- `LightningDB` (LMDB — embedded memory-mapped B+tree read-optimized MVCC engine: ACID, zero-copy reads, named DBs, cursors, dupsort multi-value keys)

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
- `h3-pg` (Uber-H3 hex indexing inside PostgreSQL + the `h3_postgis` bridge; cell ids match the managed `pocketken.H3` pin)
- `apache-age` (openCypher graph database inside PostgreSQL — path queries over the federated entity graph; driven through raw Npgsql against the `agtype` result type)
- `pgrouting` (network/graph routing over PostGIS — `pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance`; the GEO_LANES routing capability via raw SQL)
- `pg_graphql` (in-Postgres GraphQL schema/resolver reflection via `graphql.resolve`)
- `pg_net` (asynchronous non-blocking HTTP/HTTPS from SQL — `net.http_get`/`net.http_post`)

[SQLITE]:
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Data.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlite3`

[COLUMNAR_ARROW]:
- `DuckDB.NET.Data.Full`
- `Apache.Arrow`
- `Apache.Arrow.Flight`
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Adbc.Drivers.Apache` (pure-managed Thrift+Arrow ADBC driver binding Hive/Impala/Spark Thrift SQL endpoints — the osx-arm64-viable concrete ADBC driver; the Interop FlightSql/Snowflake drivers ship no osx-arm64 native and are rejected)
- `Apache.Arrow.Adbc.Drivers.BigQuery` (pure-managed Google BigQuery ADBC driver — the cloud-warehouse lane over the same ADBC API, Arrow record-batch results)
- `Apache.Arrow.Compression` (admitted concrete `ICompressionCodecFactory` for `Lz4Frame`/`Zstd` Arrow-IPC compression; pure-managed, transitives `K4os.Compression.LZ4.Streams` + `ZstdSharp.Port` — distinct from the `K4os.Compression.LZ4` snapshot codec)
- `ParquetSharp` (native libparquet-cpp Parquet file read/write — the direct columnar-file codec the managed `Apache.Arrow` C# stack lacks; osx-arm64 native dylib ships in-package, distinct from the DuckDB SQL parquet path)
- `FlowtideDotNet.Substrait` (Substrait portable query-plan IR — cross-backend relational-algebra plan format backing the Query federation/pipeline rail; pure-managed, SQL-text ingest via the `SqlParserCS` transitive)
- `FastCDC.Net`

[INTERCHANGE_CODECS]:
Row-oriented and self-describing ingress/egress codecs the columnar set lacked.
- `Chr.Avro` (abstract Avro schema model + resolution/evolution + GenericRecord/POCO mapping + code generation)
- `Chr.Avro.Binary` (Avro binary serializer over the abstract schema model)
- `Chr.Avro.Confluent` (first-class Confluent Schema Registry serdes leg of `Chr.Avro`)
- `System.Formats.Cbor` (first-party BCL CBOR / RFC 8949 reader/writer — Strict/Canonical/Ctap2Canonical conformance modes; the IETF self-describing binary snapshot/blob codec, orthogonal to the schemaless MessagePack wire format)
- `Sylvan.Data.Excel` (`DbDataReader`-shaped streaming xlsx/xlsb/xls ingress + xlsx/xlsb egress codec — the spreadsheet boundary the `Sep` delimited-only lane cannot stream into managed ADO.NET rows)
- `ZstdSharp.Port` (first-class standalone Zstandard snapshot/blob compression owner — promoted from the Arrow-IPC transitive floor; pure-managed zstd v1.5.7 port with streaming + dictionary training)

[OBJECT_STORE]:
- `AWSSDK.S3`
- `Azure.Storage.Blobs`
- `Google.Cloud.Storage.V1`
- `Minio` (endpoint-agnostic S3-compatible object client — MinIO/R2/Wasabi/B2/Ceph/any S3 API; bucket lifecycle, object put/get/stat/remove, multipart, presigned URLs, SSE, notifications — the self-hosted lane the cloud-native AWS/Azure/GCS SDK rows lack)

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
- `Confluent.SchemaRegistry` (Schema Registry REST client — subject compatibility, schema evolution, schema references; drives Confluent/Karapace/Apicurio/AWS Glue/Redpanda registries)
- `Confluent.SchemaRegistry.Serdes.Avro` (registry-governed Avro serde for evolution-safe binary Avro topics)
- `Confluent.SchemaRegistry.Serdes.Protobuf` (registry-governed Protobuf serde composing the admitted `Google.Protobuf` wire seam)
- `Confluent.SchemaRegistry.Serdes.Json` (registry-governed JSON-Schema serde with server-side schema validation — distinct from the CloudEvents JSON envelope formatter)
- `CloudNative.CloudEvents`
- `CloudNative.CloudEvents.Kafka`
- `CloudNative.CloudEvents.SystemTextJson`

[MESSAGING_PROTOCOLS]:
Full-featured production messaging-protocol clients backing the egress sink rows, each a distinct wire protocol.
- `NATS.Net` (full NATS protocol — Core pub/sub + request/reply, JetStream durable streams/consumers/acks/replay for the egress Settle ceremony, plus JetStream KV + Object Store as distinct store-backend capability; backs `EgressSink.Nats`)
- `RabbitMQ.Client` (official AMQP 0-9-1 client, fully-async v7 TAP API — publisher confirms, consumer, queue/exchange/binding admin; backs `EgressSink.RabbitMq`)
- `DotPulsar` (official Apache Pulsar binary-protocol client — durable subscriptions, acks, JSON/Protobuf schema; the `EgressSink.Pulsar` sink + distinct log-streaming ingress backend with separated compute/storage and tiered storage)

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

[PERF]:
- `CommunityToolkit.HighPerformance` (cross-cutting high-performance BCL substrate co-consumed with `Rasm.Compute`; spans/memory-pool/bit primitives behind the Cache And Object Store path)

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
- `BenchmarkDotNet`
- `SharpFuzz`
- `NodaTime.Testing`
