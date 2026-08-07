# [PERSISTENCE]

`Rasm.Persistence` is the content-addressed durable system of record for the `ElementGraph`; the version-control engine over it — commit-DAG, CRDT merge, AS-OF time travel, three-way merge, attested provenance, classification-driven retention, verified recovery; the consistency-split read lanes; the content-keyed artifact object plane; and the fenced coordination substrate. Its bar: a Type re-key reads as a rename, a million-event model scrubs at the cost of its delta, and every cross-runtime reuse key resolves bit-identically against the kernel content-hash.

It persists the graph over a Marten append substrate and depends up on the `Rasm.Element` seam for the `ElementGraph` and the `Rasm` kernel alone for the content hash and the signal capsule's causal frame, instrument mechanism, and hook vocabulary, each a settled contract. Its instrument roster and its lifecycle points contribute as `TelemetryContributorPort` and `HookPoint` values the app-platform root binds, so no app-platform package is referenced.

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
- [15]-[RETRIEVAL](.planning/Query/retrieval.md): ANN retrieval fusing the vector and text branches beside the document full-text corpus lane.
- [16]-[TOPOLOGY](.planning/Query/topology.md): In-process QuikGraph view owning default synchronous traversal.
- [17]-[COLUMNAR](.planning/Query/columnar.md): DuckDB analytical lane, flat-table projection, and the analytics residence family.
- [18]-[CYPHER](.planning/Query/cypher.md): Optional self-hosted openCypher and pgrouting lane.
- [19]-[CACHE](.planning/Query/cache.md): Compute-result reuse index with its benchmark gate and invalidation.
- [20]-[FEDERATION](.planning/Query/federation.md): Substrait federation router lowering portable plans onto the standing lanes.

[INGEST]:
- [21]-[TABULAR](.planning/Ingest/tabular.md): Delimited and spreadsheet source lane.
- [22]-[SCHEDULE](.planning/Ingest/schedule.md): Schedule-file codec and its durable task-relation DAG.
- [23]-[GEOSPATIAL](.planning/Ingest/geospatial.md): Geospatial feature source lane.
- [24]-[ISSUE](.planning/Ingest/issue.md): BCF issue rows — GlobalId correlation, cycle reconcile, and the typed-row seam to the container custodian.
- [25]-[POINTCLOUD](.planning/Ingest/pointcloud.md): Reality-capture codec — E57/LAS/LAZ scan headers, chunked blob residence, per-region H3 cells.

[STORE]:
- [26]-[BLOBSTORE](.planning/Store/blobstore.md): Content-keyed artifact object plane with its write-blob-first seal.
- [27]-[SCHEMA](.planning/Store/schema.md): Owns the canonical backend contract and generation algebra.
- [28]-[PROVISIONING](.planning/Store/provisioning.md): Verify-only extension tier and provider materializer rows.
- [29]-[COORDINATION](.planning/Store/coordination.md): Token-fenced lease store owning budget, CAS, lease, membership, and outbox.
- [30]-[OBSERVABILITY](.planning/Store/observability.md): Store telemetry over harvests, hook rail, chargeback residence, and contributor port.

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
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- `Microsoft.EntityFrameworkCore.Sqlite`
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
- `Apache.Arrow.Flight`
- `Apache.Arrow.Flight.Sql` — Flight SQL dialect over the Flight transport; `FlightSqlClient` verbs and the `FlightSqlServer` served-node base.
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Adbc.Drivers.Apache` — pure-managed Thrift+Arrow ADBC over Hive, Impala, and Spark.
- `Apache.Arrow.Adbc.Drivers.BigQuery` — pure-managed BigQuery ADBC cloud-warehouse lane.
- `Apache.Arrow.Compression` — Arrow-IPC `Lz4Frame`/`Zstd` codec factory.
- `ParquetSharp` — native libparquet Parquet read and write.
- `ParquetSharp.Dataset` — partitioned multi-file Parquet lake scanner streaming `Apache.Arrow` batches with predicate and column pushdown.
- `FlowtideDotNet.Substrait` — Substrait portable query-plan IR backing the federation rail and the one residence lowering.
- `FastCDC.Net`
- `Ara3D.BimOpenSchema`
- `Ara3D.BimOpenSchema.IO`
- `Parquet.Net` — pure-managed Parquet codec under the BimOpenSchema Parquet-zip leg, version-governed as a central transitive pin.
- `Chr.Avro` — Avro schema model, resolution, evolution, and POCO mapping.
- `Chr.Avro.Binary`
- `Chr.Avro.Confluent` — binds the Confluent Schema Registry serdes leg.
- `System.Formats.Cbor` — BCL CBOR / RFC 8949 self-describing snapshot codec.
- `MiniExcel` — streaming `.xlsx`/`.csv` codec; the spreadsheet lane `Sep` cannot reach.
- `ZstdSharp.Port` — standalone Zstandard snapshot and blob compression.
- `JsonSchema.Net` — JSON Schema 2020-12 evaluator; the in-process `pg_jsonschema` fallback.
- `K4os.Compression.LZ4`
- `MPXJ.Net` — MS-Project, P6, and Asta schedule-file codec the `Sep`/`MiniExcel` lanes lack.
- `Sep`

[REALITY_CAPTURE]: Managed scan decode behind the `Ingest/pointcloud` residence leg.
- `Aardvark.Data.E57` — ASTM E57 read decode: file header, `Data3D` scan-setup metadata, and the CompressedVector point stream.

[APPEND_AND_EGRESS]: Marten append substrate, the out-of-Rhino sync transports, and the CDC change-egress pipeline.
- `Marten` — PostgreSQL event store; `GraphDelta` bodies fold `ElementGraph` via `AggregateStreamAsync` AS-OF.
- `Confluent.Kafka`
- `OpenTelemetry.Instrumentation.ConfluentKafka` — instrumented producer/consumer builders carrying messaging spans and meters.
- `Confluent.SchemaRegistry` — Schema Registry REST client, subject compatibility and evolution.
- `Confluent.SchemaRegistry.Serdes.Avro`
- `Confluent.SchemaRegistry.Serdes.Protobuf` — registry-governed Protobuf serde over `Google.Protobuf`.
- `Confluent.SchemaRegistry.Serdes.Json`
- `CloudNative.CloudEvents.Amqp` — CloudEvents AMQP 1.0 binding; the AMQP-native egress path distinct from the `RabbitMQ.Client` 0-9-1 leg.
- `CloudNative.CloudEvents.Kafka` — binary-mode `ce_` header binding onto `Confluent.Kafka`; backs `EgressSink.Kafka`.
- `RabbitMQ.Client` — AMQP 0-9-1 with publisher confirms; backs `EgressSink.RabbitMq`.
- `DotPulsar` — Apache Pulsar binary-protocol client; backs `EgressSink.Pulsar`.

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

[MACHINE_CONNECTIVITY]:
- `MQTTnet` — QoS-1 `PublishAsync` PUBACK evidence and the v5 UserProperties tracing carrier; backs `EgressSink.Mqtt`.

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

[GEOMETRY_INTERCHANGE]:
- `Speckle.Sdk` — the send half: serialiser, transports, and client behind `SyncTransport.SpeckleLikeDiff`.
- `Speckle.Objects` — the `Base`-derived geometry and `DataObject` shapes a sync marshal targets.
- `Unofficial.laszip.netstandard` — one LAS/LAZ engine for two consumers: Bim decodes for scan-to-BIM, this folder for chunked residence and `.lax` windowed reads.

[ENERGY_SIMULATION]:
- `PollinationSDK` — cloud-run transport, sidecar-only; the durable `Version/provenance` `CloudRunFact` half.

[DATA_SUBSTRATE]:
- `Apache.Arrow` — columnar format and Arrow IPC wire; the ADBC/Flight/compression egress train rides folder-side (`api-arrow-egress.md`).
- `Microsoft.Data.Sqlite` — embedded ADO.NET transport and the `Handle` raw bridge beneath the store-profile rail.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — the `Egress.Envelope` projection: one `CloudEvent` per `OpLogEntry` on the changefeed wire.
- `CloudNative.CloudEvents.SystemTextJson` — the one shared `JsonEventFormatter` codec identity across every sink.
- `CloudNative.CloudEvents.Mqtt` — structured-mode MQTT v5 binding; backs `EgressSink.Mqtt`, payload-body-only with no binary mode.
- `NATS.Net` — JetStream publish-ack egress backing `EgressSink.Nats`; KV and Object Store as distributed store-backend rows.

[GEOSPATIAL_INDEX]:
- `pocketken.H3` — managed Uber-H3 v4 hex indexing; the same cell id at ingest and in PostgreSQL as `h3-pg`.

[GRAPH_ALGORITHM]:
- `QuikGraph` — models the in-process topology the synchronous `Query/topology` lane composes.

[PLANAR_GEOMETRY]:
- `NetTopologySuite` — `Geometry` currency and WKB/WKT core codecs behind every spatial column, satellite codec, and geometry content key.
- `NetTopologySuite.IO.GeoJSON4STJ` — GeoJSON text on the `Ingest/geospatial#FEATURE_ROWS` seam and the web egress projection.
- `NetTopologySuite.IO.GeoPackage` — GeoPackage geometry-BLOB coding on the same feature-rows seam.

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — L2-store and serializer half of the AppHost-owned two-tier cache.

[DATA_CLASSIFICATION]:
- `Microsoft.Extensions.Compliance.Redaction` — classification attributes on egressed members; redactor binding stays at the app root.

[TELEMETRY_CONTRACT]:
- `Microsoft.Extensions.Telemetry.Abstractions` — pooled per-operation latency ledger the `Query/lane` read phases stamp; activation stays app-root.

[WIRE_CODEGEN]:
- `Riok.Mapperly` — generated seam-to-wire and columnar marshal.
- `Generator.Equals` — generated structural equality and content-key preimage.
- `MessagePack` — the snapshot-axis codec profile: framed ingest, content-identity encoding, LZ4 posture.
- `MessagePackAnalyzer` — build-only generator and `MsgPack###` gate behind the AOT resolver chain.
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson` — RFC 6902 document mutation over the STJ wire.

[RUNTIME_INBOX]:
- `System.Net.Http` — blob-store client, ranged reads, and multipart upload legs of the object plane.
- `System.Text.Json` — generated wire contexts and the `JsonDocument`/`JsonElement` payload plane.

[TEST_SUBSTRATE]: Rows bind in branch test and benchmark projects, never the package csproj.
- `Verify.XunitV3`
- `BenchmarkDotNet`
