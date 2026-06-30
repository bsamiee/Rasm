# [PERSISTENCE]

`Rasm.Persistence` is the APP-PLATFORM durable-state spine: a host-neutral C# backbone that persists the `Rasm.Element` `ElementGraph` as its system of record. It depends UP on the `Rasm.Element` seam (the `ElementGraph`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`Header`/`ContentAddress` contracts) and the `Rasm` kernel content-hash, and it consumes AppHost ports (`clock`, `telemetry`, `receipts`, `drain`, `classification`) as settled vocabulary — it never references a sibling AEC-domain peer. Marten is the APPEND SUBSTRATE: each model is one event stream whose `GraphDelta` event bodies fold into the whole `ElementGraph` through an inline projection (read-your-writes) and `AggregateStreamAsync` AS-OF, while the preserved op-log/CRDT/time-travel/`StructuralMerge`/causal-DAG engine PROJECTS from those events. The package owns the ElementGraph store-load roundtrip (`Element/`), the version-control engine over Marten — commit-DAG, convergent CRDT, AS-OF time-travel, three-way structural merge, W3C-PROV provenance, classification/retention with full-history reachability GC, and verified backup/PITR recovery (`Version/`) — the read lanes (synchronous in-process QuikGraph topology, async DuckDB/BimOpenSchema columnar, optional Apache AGE openCypher, and the compute-result reuse index — artifact-blob index, model-result recency horizon, benchmark-claim gate; `Query/`), the tabular ingest codec (`Ingest/`), and the content-keyed geometry object store plus the self-provisioned PostgreSQL server tier and embedded-SQLite floor (`Store/`). The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[ELEMENT_GRAPH](.planning/Element/graph.md)
- [02]-[ELEMENT_CODEC](.planning/Element/codec.md)
- [03]-[ELEMENT_IDENTITY](.planning/Element/identity.md)
- [04]-[VERSION_LEDGER](.planning/Version/ledger.md)
- [05]-[VERSION_COMMITS](.planning/Version/commits.md)
- [06]-[VERSION_TIMETRAVEL](.planning/Version/timetravel.md)
- [07]-[VERSION_MERGE](.planning/Version/merge.md)
- [08]-[VERSION_PROVENANCE](.planning/Version/provenance.md)
- [09]-[VERSION_RETENTION](.planning/Version/retention.md)
- [10]-[VERSION_RECOVERY](.planning/Version/recovery.md)
- [11]-[QUERY_LANE](.planning/Query/lane.md)
- [12]-[QUERY_TOPOLOGY](.planning/Query/topology.md)
- [13]-[QUERY_COLUMNAR](.planning/Query/columnar.md)
- [14]-[QUERY_CYPHER](.planning/Query/cypher.md)
- [15]-[QUERY_CACHE](.planning/Query/cache.md)
- [16]-[INGEST_TABULAR](.planning/Ingest/tabular.md)
- [17]-[STORE_BLOBSTORE](.planning/Store/blobstore.md)
- [18]-[STORE_PROVISIONING](.planning/Store/provisioning.md)

## [02]-[DOMAIN_PACKAGES]

Every Persistence-domain library the folder uses, planned or implemented. Versions are centralized in the one C# manifest and never pinned here; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below.

[POSTGRES_EF]:
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- `Npgsql.NetTopologySuite`
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
PostgreSQL 18 server-tier extensions: no managed assembly, provisioned through raw SQL, preload-gated or type/index/standalone-registered per the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` row — the AUTHORITATIVE provisioning roster supersets this consumer-facing list with the base-type rows a dependency chain gates on, and the two MUST agree. Each carries a folder `.api/` catalogue of its SQL surface.
- `timescaledb` (hypertable, continuous-aggregate, retention, columnstore; bgworker scheduler)
- `timescaledb_toolkit` (hyperfunction / time-weighted-aggregate layer over the `timescaledb` base)
- `pg_duckdb` (in-PG DuckDB analytical bridge — distinct from the in-process `DuckDB.NET` columnar lane, the two meeting at the columnar SQL surface)
- `postgis` (operator classes over the built-in GiST AM — the geospatial base the raster/3D/routing rows extend)
- `postgis_raster` (PostGIS raster over the `postgis` base)
- `postgis_sfcgal` (PostGIS exact 3D geometry over the `postgis` base)
- `pgvector` (the `hnsw` access-method ANN tier — the `vector` base `pgvectorscale` gates on)
- `pgvectorscale` (diskann access method over a pgvector column)
- `pg_search` (ParadeDB bm25 access method; pdb query-builder schema)
- `pg_cron` (database-local cron for SQL maintenance jobs)
- `pg_partman` (declarative range/list partition maintenance)
- `pg_squeeze` (lock-light table-bloat reclamation)
- `pg_jsonschema` (server-side JSON Schema CHECK validation)
- `pgaudit` (session/object audit logging)
- `h3-pg` (Uber-H3 hex indexing inside PostgreSQL + the `h3_postgis` bridge over the `h3` base; cell ids match the managed `pocketken.H3` pin)
- `apache-age` (openCypher graph database inside PostgreSQL — path queries over the federated entity graph; driven through raw Npgsql against the `agtype` result type; demoted beneath the in-process QuikGraph, disabled by default)
- `pgrouting` (network/graph routing over the `postgis` base — `pgr_dijkstra`/`pgr_aStar`/`pgr_drivingDistance`; the GEO_LANES routing capability via raw SQL)
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

[BIM_ANALYTICS_EXCHANGE]:
- `Ara3D.BimOpenSchema`
- `Ara3D.BimOpenSchema.IO`

[INTERCHANGE_CODECS]:
Row-oriented and self-describing ingress/egress codecs the columnar set lacked.
- `Chr.Avro` (abstract Avro schema model + resolution/evolution + GenericRecord/POCO mapping + code generation)
- `Chr.Avro.Binary` (Avro binary serializer over the abstract schema model)
- `Chr.Avro.Confluent` (first-class Confluent Schema Registry serdes leg of `Chr.Avro`)
- `System.Formats.Cbor` (first-party BCL CBOR / RFC 8949 reader/writer — Strict/Canonical/Ctap2Canonical conformance modes; the IETF self-describing binary snapshot/blob codec, orthogonal to the schemaless MessagePack wire format)
- `MiniExcel` (zero-template streaming `.xlsx`/`.csv` codec — lazy `dynamic`/typed/`IDataReader`/`DataTable` ingress with cell-range windowing, attribute + runtime `DynamicExcelColumn` mapping, `IEnumerable`/`IDataReader` egress, `.xlsx` template rendering, merged-cell folding, embedded pictures, and CSV↔XLSX transcode; the spreadsheet boundary the `Sep` delimited-only lane cannot reach, retiring `Sylvan.Data.Excel`)
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
- `Marten` (PostgreSQL event store + document database — the append substrate beneath the `Version/` engine: per-model event streams whose `GraphDelta` bodies fold into the whole `ElementGraph` via `AggregateStreamAsync` AS-OF by version/timestamp; inline `SingleStreamProjection` for read-your-writes topology, async daemon `MultiStreamProjection`/`FlatTableProjection` for the AGE/DuckDB/BimOpenSchema analytical lanes; identity row + event commit atomically in one `IDocumentSession`)
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- `Speckle.Sdk`
- `Speckle.Objects`
- `PollinationSDK`

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
- `K4os.Compression.LZ4`
- `MPXJ.Net` (binary MS-Project .mpp / Primavera P6 XER/PMXML / Asta / Phoenix schedule-file codec via IKVM Java->IL bridge — the schedule-ingress lane the row-oriented `Sep`/`MiniExcel` codecs lack)
- `Sep`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries Persistence consumes; package charters live in `libs/csharp/.planning/README.md` and shared API evidence lives in `libs/csharp/.api/`.

[SEAM_REFERENCES]:
Upward ProjectReferences (alignment by contract, never a sibling peer reference). Persistence persists any `ElementGraph` and composes the kernel content-hash; it never references `Rasm.Materials`/`Rasm.Bim`/`Rasm.Fabrication`.
- `Rasm.Element` (the AEC-DOMAIN seam — `ElementGraph`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`Header`/`ContentAddress`/`Discipline` contracts the durable spine persists)
- `Rasm` (the KERNEL — the seed-zero `XxHash128` content-hash entry the codec and the content address compose; geometry referenceable by content-hash only)

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`

[CODE_GENERATION]:
- `Riok.Mapperly` (compile-time graph↔DTO/proto mapper — the source-generated marshal between the seam `ElementGraph`/`Node` shapes and the wire/columnar projections, never a hand-rolled mapper; shared substrate, `libs/csharp/.api/api-mapperly.md`)
- `Generator.Equals` (source-generated structural equality for node/edge value carriers and the content-key preimage, never a hand-rolled `Equals`/`GetHashCode`; shared substrate, `libs/csharp/.api/api-generator-equals.md`)
- `QuikGraph` (the in-process topology graph + traversal/shortest-path/components/topological-sort algorithms the synchronous authoritative `Query/topology` lane composes; shared substrate, `libs/csharp/.api/api-quikgraph.md`)

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[PERF]:
- `CommunityToolkit.HighPerformance` (cross-cutting high-performance BCL substrate co-consumed with `Rasm.Compute`; spans/memory-pool/bit primitives behind the Cache And Object Store path)
- `System.Numerics.Tensors` (cross-cutting SIMD numeric substrate co-consumed with `Rasm.Compute`; `TensorPrimitives.Distance`/`Add`/`Divide` back the `Query/lane#VECTOR_CODEBOOK` product-quantization k-means training and asymmetric-distance scan with the SAME reduction Compute's nearest-centroid encode assigns with; shared substrate, `libs/csharp/.api/api-tensors.md`)

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
- `BenchmarkDotNet`
- `NodaTime.Testing`
