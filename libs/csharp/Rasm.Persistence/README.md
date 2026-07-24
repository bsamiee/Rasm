# [PERSISTENCE]

`Rasm.Persistence` is the federation's durable truth: the content-addressed system of record for the `ElementGraph`; the version-control engine over it — commit-DAG, CRDT merge, AS-OF time travel, three-way merge, attested provenance, classification-driven retention, verified recovery; the consistency-split read lanes; the content-keyed geometry object store; and the fenced coordination substrate. Its bar: a Type re-key reads as a rename, a million-event model scrubs at the cost of its delta, and every cross-runtime reuse key resolves bit-identically against the kernel content-hash.

It persists the graph over a Marten append substrate, depends up on the `Rasm.Element` seam and the `Rasm` kernel content-hash, consumes the AppHost port vocabulary as settled contract, and references no sibling AEC-domain peer — alignment travels through seam contracts and the content-keyed wire.

## [01]-[ROUTER]

[ELEMENT]:
- [01]-[GRAPH](.planning/Element/graph.md): Stream-per-model event store and its inline authoritative `ElementGraph` projection.
- [02]-[CODEC](.planning/Element/codec.md): Content-address codec folding canonical bytes into chunked snapshot tiers.
- [03]-[IDENTITY](.planning/Element/identity.md): Identity-row tier owning tenancy, EF converters, spatial bounds, and KMS custody.
- [04]-[AUTHORITY](.planning/Element/authority.md): Deny-over-allow object-ACL grant algebra behind `Authority.Admit`.

[VERSION]:
- [05]-[LEDGER](.planning/Version/ledger.md): Op-log changefeed, HLC clock, and CRDT merge dispatch over the sync transports.
- [06]-[COMMITS](.planning/Version/commits.md): Content-addressed commit-DAG and convergent CRDT algebra.
- [07]-[TIMETRAVEL](.planning/Version/timetravel.md): AS-OF reconstruct, diff, blame, and bisect fold over the changefeed prefix.
- [08]-[MERGE](.planning/Version/merge.md): Three-way structural merge and RFC 6902 patch egress.
- [09]-[PROVENANCE](.planning/Version/provenance.md): W3C-PROV causal DAG and attested tamper-evidence ledger.
- [10]-[RETENTION](.planning/Version/retention.md): Retention-class sweep and full-history reachability GC.
- [11]-[RECOVERY](.planning/Version/recovery.md): Backup-substrate routes and verified PITR choreography.
- [12]-[EGRESS](.planning/Version/egress.md): CDC egress pump minting one CloudEvents envelope per sink with dedup and replay.
- [13]-[INGRESS](.planning/Version/ingress.md): Inbound CDC consume door — instrumented Kafka leg, CloudEvents decode, content-key dedup.

[QUERY]:
- [14]-[LANE](.planning/Query/lane.md): Read router discriminating authoritative from analytical over the selection algebra.
- [15]-[RETRIEVAL](.planning/Query/retrieval.md): ANN retrieval subsystem fusing the vector and text branches.
- [16]-[TOPOLOGY](.planning/Query/topology.md): In-process QuikGraph view owning default synchronous traversal.
- [17]-[COLUMNAR](.planning/Query/columnar.md): DuckDB analytical lane and its flat-table projection.
- [18]-[CYPHER](.planning/Query/cypher.md): Optional self-hosted openCypher and pgrouting lane.
- [19]-[CACHE](.planning/Query/cache.md): Compute-result reuse index with its benchmark gate and invalidation.
- [20]-[FEDERATION](.planning/Query/federation.md): Substrait federation router lowering portable plans onto the standing lanes.

[INGEST]:
- [21]-[TABULAR](.planning/Ingest/tabular.md): Delimited and spreadsheet source lane.
- [22]-[SCHEDULE](.planning/Ingest/schedule.md): Schedule-file codec and its durable task-relation DAG.
- [23]-[GEOSPATIAL](.planning/Ingest/geospatial.md): Geospatial feature source lane.
- [24]-[ISSUE](.planning/Ingest/issue.md): BCF issue-file codec — topics, viewpoints, and comments keyed to durable elements.

[STORE]:
- [25]-[BLOBSTORE](.planning/Store/blobstore.md): Content-keyed geometry object store with its write-blob-first seal.
- [26]-[PROVISIONING](.planning/Store/provisioning.md): Verification-first extension tier and provider-binding rows.
- [27]-[COORDINATION](.planning/Store/coordination.md): Token-fenced lease store owning budget, CAS, lease, membership, and outbox.
- [28]-[OBSERVABILITY](.planning/Store/observability.md): Engine-stat and plan-shape harvest receipts, the hook rail, and the instrument contributor.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[RELATIONAL_TIER]: PostgreSQL/EF managed stack and the embedded-SQLite floor — the closed relational system-of-record tier.
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- `Npgsql.NetTopologySuite`
- `Npgsql.OpenTelemetry`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore` — trace-only ORM-layer command spans complementing the ADO-layer `Npgsql.OpenTelemetry` spans.
- `EFCore.NamingConventions`
- `linq2db.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Pgvector`
- `Pgvector.EntityFrameworkCore`
- `NetTopologySuite.IO.GeoJSON4STJ`
- `NetTopologySuite.IO.GeoPackage`
- `pocketken.H3` — managed Uber-H3 v4 hex indexing; the same cell id at ingest and in PostgreSQL as `h3-pg`.
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.Data.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlite3`
- `SQLitePCLRaw.bundle_e_sqlite3mc` — Multiple Ciphers bundle; encrypted embedded floor under a KMS-custodied key.

[SERVER_EXTENSIONS]: PostgreSQL SQL-provisioned extensions carrying no managed assembly. `Store/provisioning`'s `ServerExtension` roster is authoritative — its keys carry the server `CREATE EXTENSION` spelling and its base-bridge rows land only through the verification fold; rows here carry the package spelling, each with a folder `.api/` catalogue of its SQL surface.
- `timescaledb` — hypertable, continuous-aggregate, retention, and columnstore.
- `timescaledb_toolkit` — hyperfunction and time-weighted-aggregate layer over `timescaledb`.
- `pg_duckdb` — in-PG DuckDB analytical bridge, distinct from the in-process `DuckDB.NET` lane.
- `postgis` — carries the geospatial base the raster, 3D, and routing rows extend.
- `postgis_raster` — PostGIS raster over `postgis`.
- `postgis_sfcgal` — PostGIS exact 3D geometry over `postgis`.
- `pgvector` — ships the `hnsw` ANN access method and the `vector` base `pgvectorscale` gates on.
- `pgvectorscale` — diskann access method over a pgvector column.
- `pg_search` — ParadeDB bm25 access method.
- `pg_cron` — database-local cron for SQL maintenance jobs.
- `pg_partman` — declarative range and list partition maintenance.
- `pg_squeeze` — lock-light table-bloat reclamation.
- `pg_jsonschema` — server-side JSON Schema CHECK validation.
- `pgaudit` — session and object audit logging.
- `h3-pg` — Uber-H3 hex indexing in PostgreSQL; cell ids match the `pocketken.H3` pin.
- `apache-age` — openCypher over `agtype`, demoted beneath QuikGraph and disabled by default.
- `pgrouting` — network routing over `postgis` backing the `Query/cypher` routing cases.
- `pg_graphql` — in-Postgres GraphQL schema and resolver reflection.
- `pg_net` — asynchronous non-blocking HTTP/HTTPS from SQL.

[SCALEOUT_BACKENDS]: Dedicated scale-out store clients and embedded KV engines beyond the relational tier, each a distinct backend class.
- `ClickHouse.Driver` — distributed columnar OLAP client; the billion-row lane beyond in-PG TimescaleDB and DuckDB.
- `ScyllaDBCSharpDriver` — CQL wide-column client driving ScyllaDB and Cassandra over one protocol.
- `Qdrant.Client` — scale-out vector store; the billion-scale ANN class beyond in-PG `pgvector`.
- `DeltaLake.Net` — delta-rs Delta Lake read/write over S3/Azure/GCS for external-warehouse interop.
- `rocksdb` — embedded LSM-tree write-optimized KV/log engine.
- `LightningDB` — LMDB memory-mapped B+tree read-optimized MVCC engine.

[COLUMNAR_AND_CODECS]: In-process columnar analytics stack and the serialization, interchange, and compression codec belt.
- `DuckDB.NET.Data.Full` — drives the in-process DuckDB columnar lane, distinct from the `pg_duckdb` server bridge.
- `Apache.Arrow`
- `Apache.Arrow.Flight`
- `Apache.Arrow.Flight.Sql` — Flight SQL dialect over the Flight transport; `FlightSqlClient` verbs and the `FlightSqlServer` served-node base.
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Adbc.Drivers.Apache` — pure-managed Thrift+Arrow ADBC over Hive, Impala, and Spark.
- `Apache.Arrow.Adbc.Drivers.BigQuery` — pure-managed BigQuery ADBC cloud-warehouse lane.
- `Apache.Arrow.Compression` — Arrow-IPC `Lz4Frame`/`Zstd` codec factory.
- `ParquetSharp` — native libparquet Parquet read and write.
- `ParquetSharp.Dataset` — partitioned multi-file Parquet lake scanner streaming `Apache.Arrow` batches with predicate and column pushdown.
- `FlowtideDotNet.Substrait` — Substrait portable query-plan IR backing the federation rail.
- `FastCDC.Net`
- `Ara3D.BimOpenSchema`
- `Ara3D.BimOpenSchema.IO`
- `Chr.Avro` — Avro schema model, resolution, evolution, and POCO mapping.
- `Chr.Avro.Binary`
- `Chr.Avro.Confluent` — binds the Confluent Schema Registry serdes leg.
- `System.Formats.Cbor` — BCL CBOR / RFC 8949 self-describing snapshot codec.
- `MiniExcel` — streaming `.xlsx`/`.csv` codec; the spreadsheet lane `Sep` cannot reach.
- `ZstdSharp.Port` — standalone Zstandard snapshot and blob compression.
- `MessagePack`
- `MessagePackAnalyzer`
- `JsonSchema.Net` — JSON Schema 2020-12 evaluator; the in-process `pg_jsonschema` fallback.
- `K4os.Compression.LZ4`
- `MPXJ.Net` — MS-Project, P6, and Asta schedule-file codec the `Sep`/`MiniExcel` lanes lack.
- `Sep`

[APPEND_AND_EGRESS]: Marten append substrate, the out-of-Rhino sync transports, and the CDC change-egress pipeline.
- `Marten` — PostgreSQL event store; `GraphDelta` bodies fold `ElementGraph` via `AggregateStreamAsync` AS-OF.
- `Microsoft.Extensions.Compliance.Redaction`
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson`
- `Speckle.Sdk`
- `Speckle.Objects`
- `PollinationSDK` — cloud-run transport, sidecar-only; the durable `Version/provenance` `CloudRunFact` half.
- `Confluent.Kafka`
- `OpenTelemetry.Instrumentation.ConfluentKafka` — instrumented producer/consumer builders carrying messaging spans and meters.
- `Confluent.SchemaRegistry` — Schema Registry REST client, subject compatibility and evolution.
- `Confluent.SchemaRegistry.Serdes.Avro`
- `Confluent.SchemaRegistry.Serdes.Protobuf` — registry-governed Protobuf serde over `Google.Protobuf`.
- `Confluent.SchemaRegistry.Serdes.Json`
- `CloudNative.CloudEvents`
- `CloudNative.CloudEvents.Amqp` — CloudEvents AMQP 1.0 binding; the AMQP-native egress path distinct from the `RabbitMQ.Client` 0-9-1 leg.
- `CloudNative.CloudEvents.Kafka`
- `CloudNative.CloudEvents.Mqtt` — structured-mode CloudEvents MQTT v5 binding; backs `EgressSink.Mqtt`, payload-body-only with no binary mode.
- `CloudNative.CloudEvents.SystemTextJson`
- `NATS.Net` — Core pub/sub and JetStream durable streams; backs `EgressSink.Nats`.
- `RabbitMQ.Client` — AMQP 0-9-1 with publisher confirms; backs `EgressSink.RabbitMq`.
- `DotPulsar` — Apache Pulsar binary-protocol client; backs `EgressSink.Pulsar`.
- `MQTTnet` — MQTT v5 client with QoS-1 `PublishAsync` PUBACK evidence and the v5 UserProperties tracing carrier; backs `EgressSink.Mqtt`.

[OBJECT_CACHE_KMS]: Cloud object stores, the Redis cache backplane, and KMS custody.
- `AWSSDK.S3`
- `OpenTelemetry.Instrumentation.AWS` — one root registration spanning the S3 and KMS legs through the shared `AWSSDK.Core` pipeline.
- `Azure.Storage.Blobs`
- `Google.Cloud.Storage.V1`
- `Minio` — endpoint-agnostic S3-compatible client for the self-hosted lane.
- `StackExchange.Redis` — backs the `Query/cache` L2 backplane and the `Version/egress` `EgressSink.RedisStream` sink.
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `OpenTelemetry.Instrumentation.StackExchangeRedis` — trace-only command spans hooking the cache and egress multiplexers into the root trace.
- `AWSSDK.KeyManagementService`
- `Azure.Security.KeyVault.Keys`
- `Google.Cloud.Kms.V1`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `CommunityToolkit.HighPerformance` — spans, memory pools, and bit primitives on the cache and object-store path.
- `System.Numerics.Tensors` — SIMD `TensorPrimitives` backing the `VECTOR_CODEBOOK` PQ k-means and ADC scan.

[GRAPH_ALGORITHM]:
- `QuikGraph` — models the in-process topology the synchronous `Query/topology` lane composes.

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — L2-store and serializer half of the AppHost-owned two-tier cache.

[WIRE_CODEGEN]:
- `Riok.Mapperly` — generated seam-to-wire and columnar marshal.
- `Generator.Equals` — generated structural equality and content-key preimage.

[TEST_SUBSTRATE]: Rows bind in branch test and benchmark projects, never the package csproj.
- `Verify.XunitV3`
- `BenchmarkDotNet`
