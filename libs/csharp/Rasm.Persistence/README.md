# [PERSISTENCE]

`Rasm.Persistence` is the APP-PLATFORM durable-state spine: a host-neutral C# backbone that persists the `Rasm.Element` `ElementGraph` as its system of record. It depends UP on the `Rasm.Element` seam (the `ElementGraph`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`Header`/`ContentAddress` contracts) and the `Rasm` kernel content-hash, and it consumes AppHost ports (`clock`, `telemetry`, `receipts`, `drain`, `classification`) as settled vocabulary — it never references a sibling AEC-domain peer. Marten is the APPEND SUBSTRATE: each model is one event stream whose `GraphDelta` event bodies fold into the whole `ElementGraph` through an inline projection (read-your-writes) and `AggregateStreamAsync` AS-OF, while the preserved op-log/CRDT/time-travel/`StructuralMerge`/causal-DAG engine PROJECTS from those events. The package owns the ElementGraph store-load roundtrip plus the `Authority.Admit` deny-over-allow object-ACL authorization set-algebra — `authority.md`, split from `identity.md`, owns no fault band and composes `IdentityFault` 8340 — with `graph.md` hosting the `[FAULT_TABLES]` `FaultBand` band registry (`Element/`), the version-control engine over Marten — commit-DAG, convergent CRDT, AS-OF time-travel, three-way structural merge, W3C-PROV provenance, classification/retention with full-history reachability GC, verified backup/PITR recovery, and the CloudEvents CDC egress pump draining the per-sink outbox cursor (`Version/`) — the read lanes (routing + `ElementSet` algebra, the fusion/PQ-codebook retrieval subsystem, synchronous in-process QuikGraph topology, async DuckDB/BimOpenSchema columnar, optional Apache AGE openCypher, the compute-result reuse index with its wide-column residency axis, and the Substrait federation router lowering portable plans onto the standing lanes; `Query/`), the file-codec ingress axis (tabular, MPXJ schedule, GeoPackage/GeoJSON/WKB geospatial; `Ingest/`), and the content-keyed geometry object store plus the verification-first PostgreSQL server tier, embedded-SQLite floor, and the token-validating fenced-lease coordination store behind the four AppHost PORT contracts (`Store/`). The sub-domain map and domain law live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[ELEMENT_GRAPH](.planning/Element/graph.md)
- [02]-[ELEMENT_CODEC](.planning/Element/codec.md)
- [03]-[ELEMENT_IDENTITY](.planning/Element/identity.md)
- [04]-[ELEMENT_AUTHORITY](.planning/Element/authority.md)
- [05]-[VERSION_LEDGER](.planning/Version/ledger.md)
- [06]-[VERSION_COMMITS](.planning/Version/commits.md)
- [07]-[VERSION_TIMETRAVEL](.planning/Version/timetravel.md)
- [08]-[VERSION_MERGE](.planning/Version/merge.md)
- [09]-[VERSION_PROVENANCE](.planning/Version/provenance.md)
- [10]-[VERSION_RETENTION](.planning/Version/retention.md)
- [11]-[VERSION_RECOVERY](.planning/Version/recovery.md)
- [12]-[VERSION_EGRESS](.planning/Version/egress.md)
- [13]-[QUERY_LANE](.planning/Query/lane.md)
- [14]-[QUERY_RETRIEVAL](.planning/Query/retrieval.md)
- [15]-[QUERY_TOPOLOGY](.planning/Query/topology.md)
- [16]-[QUERY_COLUMNAR](.planning/Query/columnar.md)
- [17]-[QUERY_CYPHER](.planning/Query/cypher.md)
- [18]-[QUERY_CACHE](.planning/Query/cache.md)
- [19]-[QUERY_FEDERATION](.planning/Query/federation.md)
- [20]-[INGEST_TABULAR](.planning/Ingest/tabular.md)
- [21]-[INGEST_SCHEDULE](.planning/Ingest/schedule.md)
- [22]-[INGEST_GEOSPATIAL](.planning/Ingest/geospatial.md)
- [23]-[STORE_BLOBSTORE](.planning/Store/blobstore.md)
- [24]-[STORE_PROVISIONING](.planning/Store/provisioning.md)
- [25]-[STORE_COORDINATION](.planning/Store/coordination.md)

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
- `pocketken.H3` — managed Uber-H3 v4 hex geospatial indexing; the in-process `h3-pg` counterpart, same cell id at ingest and in PostgreSQL
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`

[STORE_BACKENDS]:
Dedicated store-backend clients reaching beyond the single-PG / embedded-SQLite tier, each a distinct backend class with no overlap.
- `ClickHouse.Driver` — distributed columnar OLAP client, binary bulk insert + ADO.NET; the scale-out billion-row lane beyond in-PG TimescaleDB + DuckDB
- `ScyllaDBCSharpDriver` — CQL wide-column client, shard/tablet-aware, prepared/batch, Linq2Cql + ADO.NET; drives ScyllaDB and Cassandra over one CQL protocol
- `Qdrant.Client` — dedicated scale-out vector-store client, quantization, payload filtering, snapshots; the billion-scale ANN class beyond in-PG `pgvector`
- `DeltaLake.Net` — delta-rs FFI Delta Lake read/write client, S3/Azure/GCS, DataFusion over Arrow C Data; osx-arm64 dylibs, external-warehouse interop

[EMBEDDED_KV]:
Embedded high-throughput KV/log engines beyond the SQLite relational B-tree floor; both ship osx-arm64 native dylibs.
- `rocksdb` — curiosity-ai line, embedded LSM-tree write-optimized KV/log engine: column families, WriteBatch, snapshots, merge operators, transactions
- `LightningDB` — LMDB embedded memory-mapped B+tree read-optimized MVCC engine: ACID, zero-copy reads, named DBs, cursors, dupsort multi-value keys

[SERVER_EXTENSIONS]:
PostgreSQL 18 server-tier extensions: no managed assembly, provisioned through raw SQL, preload-gated or type/index/standalone-registered per the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` row — the AUTHORITATIVE provisioning roster supersets this consumer-facing list with the base-type rows a dependency chain gates on, and the two MUST agree. Each carries a folder `.api/` catalogue of its SQL surface.
- `timescaledb` — hypertable, continuous-aggregate, retention, columnstore; bgworker scheduler
- `timescaledb_toolkit` — hyperfunction / time-weighted-aggregate layer over the `timescaledb` base
- `pg_duckdb` — in-PG DuckDB analytical bridge, distinct from the in-process `DuckDB.NET` columnar lane, meeting at the columnar SQL surface
- `postgis` — operator classes over the built-in GiST AM, the geospatial base the raster/3D/routing rows extend
- `postgis_raster` — PostGIS raster over the `postgis` base
- `postgis_sfcgal` — PostGIS exact 3D geometry over the `postgis` base
- `pgvector` — the `hnsw` access-method ANN tier, the `vector` base `pgvectorscale` gates on
- `pgvectorscale` — diskann access method over a pgvector column
- `pg_search` — ParadeDB bm25 access method; pdb query-builder schema
- `pg_cron` — database-local cron for SQL maintenance jobs
- `pg_partman` — declarative range/list partition maintenance
- `pg_squeeze` — lock-light table-bloat reclamation
- `pg_jsonschema` — server-side JSON Schema CHECK validation
- `pgaudit` — session/object audit logging
- `h3-pg` — Uber-H3 hex indexing inside PostgreSQL + the `h3_postgis` bridge over the `h3` base; cell ids match the `pocketken.H3` pin
- `apache-age` — openCypher graph database inside PostgreSQL, path queries over `agtype` via raw Npgsql; demoted beneath QuikGraph, disabled by default
- `pgrouting` — network/graph routing over the `postgis` base, `pgr_dijkstra`/`pgr_drivingDistance`; the `Query/cypher#GRAPH_QUERY` routing cases via raw SQL
- `pg_graphql` — in-Postgres GraphQL schema/resolver reflection via `graphql.resolve`
- `pg_net` — asynchronous non-blocking HTTP/HTTPS from SQL, `net.http_get`/`net.http_post`

[SQLITE]:
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Data.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlite3`

[COLUMNAR_ARROW]:
- `DuckDB.NET.Data.Full`
- `Apache.Arrow`
- `Apache.Arrow.Flight`
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Adbc.Drivers.Apache` — pure-managed Thrift+Arrow ADBC driver over Hive/Impala/Spark Thrift SQL, the osx-arm64-viable concrete ADBC driver
- `Apache.Arrow.Adbc.Drivers.BigQuery` — pure-managed Google BigQuery ADBC driver, the cloud-warehouse lane over the same ADBC API, Arrow record-batch results
- `Apache.Arrow.Compression` — `ICompressionCodecFactory` for `Lz4Frame`/`Zstd` Arrow-IPC compression, distinct from the `K4os.Compression.LZ4` snapshot codec
- `ParquetSharp` — native libparquet-cpp Parquet file read/write, the direct columnar-file codec the managed `Apache.Arrow` stack lacks; ships osx-arm64 dylib
- `FlowtideDotNet.Substrait` — Substrait portable query-plan IR, cross-backend plan backing the Query federation rail; SQL-text ingest via `SqlParserCS`
- `FastCDC.Net`

[BIM_ANALYTICS_EXCHANGE]:
- `Ara3D.BimOpenSchema`
- `Ara3D.BimOpenSchema.IO`

[INTERCHANGE_CODECS]:
Row-oriented and self-describing ingress/egress codecs the columnar set lacked.
- `Chr.Avro` — abstract Avro schema model + resolution/evolution + GenericRecord/POCO mapping + code generation
- `Chr.Avro.Binary` — Avro binary serializer over the abstract schema model
- `Chr.Avro.Confluent` — first-class Confluent Schema Registry serdes leg of `Chr.Avro`
- `System.Formats.Cbor` — BCL CBOR / RFC 8949 reader/writer, Ctap2Canonical/Strict modes; the IETF self-describing snapshot codec, orthogonal to MessagePack
- `MiniExcel` — streaming `.xlsx`/`.csv` codec, typed/`IDataReader` ingress+egress; the spreadsheet lane `Sep` cannot reach, retires `Sylvan.Data.Excel`
- `ZstdSharp.Port` — standalone Zstandard snapshot/blob compression owner, promoted from the Arrow-IPC transitive; managed port, streaming + dictionary training

[OBJECT_STORE]:
- `AWSSDK.S3`
- `Azure.Storage.Blobs`
- `Google.Cloud.Storage.V1`
- `Minio` — endpoint-agnostic S3-compatible object client, MinIO/R2/Wasabi/B2/Ceph, multipart, presigned URLs; the self-hosted lane AWS/Azure/GCS SDK rows lack

[ENCRYPTION_KMS]:
- `AWSSDK.KeyManagementService`
- `Azure.Security.KeyVault.Keys`
- `Google.Cloud.Kms.V1`

[REDIS]:
- `StackExchange.Redis` — direct driver, the `Query/cache` L2 invalidation backplane and the `Version/egress` `EgressSink.RedisStream` sink row
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.Extensions.Caching.Hybrid`

[VERSIONING_SYNC]:
- `Marten` — PostgreSQL event store + document DB, append substrate beneath `Version/`; `GraphDelta` bodies fold `ElementGraph` via `AggregateStreamAsync` AS-OF
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- `Speckle.Sdk`
- `Speckle.Objects`
- `PollinationSDK` — cloud-run transport, sidecar-only; durable half `Version/provenance` `CloudRunFact` via `CausalDag.Derive`, fork closure never in-Rhino

[STREAMING_EGRESS]:
- `Confluent.Kafka`
- `Confluent.SchemaRegistry` — Schema Registry REST client, subject compatibility, evolution, references; drives Confluent/Karapace/Apicurio/AWS Glue/Redpanda
- `Confluent.SchemaRegistry.Serdes.Avro` — registry-governed Avro serde for evolution-safe binary Avro topics
- `Confluent.SchemaRegistry.Serdes.Protobuf` — registry-governed Protobuf serde composing the admitted `Google.Protobuf` wire seam
- `Confluent.SchemaRegistry.Serdes.Json` — registry-governed JSON-Schema serde, server-side validation, distinct from the CloudEvents JSON envelope formatter
- `CloudNative.CloudEvents`
- `CloudNative.CloudEvents.Kafka`
- `CloudNative.CloudEvents.SystemTextJson`

[MESSAGING_PROTOCOLS]:
Full-featured production messaging-protocol clients backing the egress sink rows, each a distinct wire protocol.
- `NATS.Net` — full NATS protocol, Core pub/sub, JetStream durable streams/acks for the egress Settle, plus JetStream KV + Object Store; backs `EgressSink.Nats`
- `RabbitMQ.Client` — AMQP 0-9-1 client, fully-async TAP API, publisher confirms, consumer, queue/exchange/binding admin; backs `EgressSink.RabbitMq`
- `DotPulsar` — Apache Pulsar binary-protocol client, durable subscriptions/acks, JSON/Protobuf schema; the `EgressSink.Pulsar` sink + log-streaming ingress

[WIRE_SERIALIZATION]:
- `MessagePack`
- `MessagePackAnalyzer`
- `JsonSchema.Net` — JSON Schema 2020-12 evaluator, the in-process `pg_jsonschema` fallback when the extension is absent, same schema validated off the wire
- `K4os.Compression.LZ4`
- `MPXJ.Net` — MS-Project .mpp / P6 XER/PMXML / Asta / Phoenix schedule-file codec via IKVM Java→IL bridge; the schedule-ingress lane `Sep`/`MiniExcel` lack
- `Sep`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting C# substrate libraries Persistence consumes; package charters live in `libs/csharp/.planning/README.md` and shared API evidence lives in `libs/csharp/.api/`.

[SEAM_REFERENCES]:
Upward ProjectReferences (alignment by contract, never a sibling peer reference). Persistence persists any `ElementGraph` and composes the kernel content-hash; it never references `Rasm.Materials`/`Rasm.Bim`/`Rasm.Fabrication`.
- `Rasm.Element` — the AEC-DOMAIN seam: `ElementGraph`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`Header`/`ContentAddress`/`Discipline` contracts
- `Rasm` — the KERNEL: the seed-zero `XxHash128` content-hash entry the codec and the content address compose; geometry referenceable by content-hash only

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`

[CODE_GENERATION]:
- `Riok.Mapperly` — compile-time graph↔DTO/proto mapper, generated marshal between seam and wire/columnar projections, never hand-rolled; shared substrate
- `Generator.Equals` — generated structural equality for node/edge carriers + content-key preimage, never hand-rolled `Equals`/`GetHashCode`; shared substrate
- `QuikGraph` — in-process topology graph + traversal/shortest-path/components/topological-sort the synchronous `Query/topology` lane composes; shared substrate

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[PERF]:
- `CommunityToolkit.HighPerformance` — high-perf BCL substrate co-consumed with `Rasm.Compute`, spans/memory-pool/bit primitives, cache + object-store path
- `System.Numerics.Tensors` — SIMD numeric substrate with `Rasm.Compute`; `TensorPrimitives` backs `VECTOR_CODEBOOK` PQ k-means + ADC scan; shared substrate

[TEST_SUBSTRATE]:
- `Verify.XunitV3`
- `BenchmarkDotNet`
