# [PERSISTENCE]

`Rasm.Persistence` is the content-addressed durable system of record for the `ElementGraph`: the version-control engine over it, the consistency-split read lanes, the content-keyed artifact object plane, and the fenced coordination substrate. Its bar: a Type re-key reads as a rename, a million-event model scrubs at the cost of its delta, and every cross-runtime reuse key resolves bit-identically against the kernel content-hash.

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
- [08]-[MERGE](.planning/Version/merge.md): Three-way structural merge and base-addressed `FieldMask` edit egress onto `Element.EntityEditWire`.
- [09]-[PROVENANCE](.planning/Version/provenance.md): W3C-PROV causal DAG and attested tamper-evidence ledger.
- [10]-[RETENTION](.planning/Version/retention.md): Retention-class sweep and full-history reachability GC.
- [11]-[RECOVERY](.planning/Version/recovery.md): Backup-substrate routes and verified PITR choreography.
- [12]-[EGRESS](.planning/Version/egress.md): CDC pump, atomic quarantine-and-advance, binding roster, and CESQL filter.
- [13]-[INGRESS](.planning/Version/ingress.md): Inbound CDC consume door — instrumented Kafka leg, rostered decode, `(source, id)` dedup.

[QUERY]:
- [14]-[LANE](.planning/Query/lane.md): Read router discriminating authoritative from analytical over the selection algebra.
- [15]-[RETRIEVAL](.planning/Query/retrieval.md): ANN retrieval fusing the vector and text branches beside the document full-text corpus lane.
- [16]-[TOPOLOGY](.planning/Query/topology.md): In-process QuikGraph view owning default synchronous traversal.
- [17]-[COLUMNAR](.planning/Query/columnar.md): DuckDB analytical lane over extension trust gates, secret residence, and ADBC warehouse reach.
- [18]-[LAKEHOUSE](.planning/Query/lakehouse.md): Co-transactional flat-table egress, Parquet generation codec, and the partitioned lake scan.
- [19]-[RESIDENCE](.planning/Query/residence.md): Column vocabulary, the analytics residence family, seam admission, and provisioning DDL.
- [20]-[SERVING](.planning/Query/serving.md): Residence read plan, Substrait lowering, transport reach, and the relational landing.
- [21]-[DATASETS](.planning/Query/datasets.md): Series hypertable roster and Fleet op-log rows.
- [22]-[CYPHER](.planning/Query/cypher.md): Optional self-hosted openCypher and pgrouting lane.
- [23]-[CACHE](.planning/Query/cache.md): Compute-result reuse index with its benchmark gate and invalidation.
- [24]-[FEDERATION](.planning/Query/federation.md): Substrait federation router lowering portable plans onto the standing lanes.

[INGEST]:
- [25]-[TABULAR](.planning/Ingest/tabular.md): `TabularSource` lane over the MiniExcel streaming codec; `TabularSpec` the modality discriminant.
- [26]-[SCHEDULE](.planning/Ingest/schedule.md): Schedule-file codec and its durable task-relation DAG.
- [27]-[GEOSPATIAL](.planning/Ingest/geospatial.md): `GeoSource` lane over the NTS-IO codec family; `GeoFormat` crossing four wire projections.
- [28]-[ISSUE](.planning/Ingest/issue.md): BCF issue rows — GlobalId correlation, cycle reconcile, and the typed-row seam to the container custodian.
- [29]-[POINTCLOUD](.planning/Ingest/pointcloud.md): Reality-capture codec — E57/LAS/LAZ scan headers, chunked blob residence, per-region H3 cells.

[STORE]:
- [30]-[BLOBSTORE](.planning/Store/blobstore.md): Content-keyed artifact object plane — client dispatch, grant plane, multipart transfer sessions.
- [31]-[RESIDENCE](.planning/Store/residence.md): Stored-byte form and write stance — checksum, codec, seal, storage tier, WORM lock.
- [32]-[REDRIVE](.planning/Store/redrive.md): Process-seam fault band, the retriability discriminant, the re-offer route, and the re-drive port.
- [33]-[BLOBGC](.planning/Store/blobgc.md): Write-blob-first protocol, lifecycle arming, and the full-history reachability sweep.
- [34]-[SCHEMA](.planning/Store/schema.md): One immutable `SchemaContract` — generation identity minted canonically, the two-proof verdict.
- [35]-[PROVISIONING](.planning/Store/provisioning.md): Verify-only extension tier and provider materializer rows.
- [36]-[COORDINATION](.planning/Store/coordination.md): Fenced budget, CAS, lease, membership, and typed per-sink outbox cursor.
- [37]-[OBSERVABILITY](.planning/Store/observability.md): Store telemetry over harvests, hook rail, chargeback residence, and contributor port.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[RELATIONAL_TIER]: PostgreSQL/EF managed stack and the embedded-SQLite floor — the closed relational system-of-record tier.
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
- `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- `Npgsql.NetTopologySuite`
- `Npgsql.NodaTime` — ADO temporal codec the raw lanes need, since the EF plugin places codecs only on mapped connections.
- `Npgsql.OpenTelemetry`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore` — Trace-only ORM-layer command spans complementing the ADO-layer `Npgsql.OpenTelemetry` spans.
- `EFCore.NamingConventions`
- `linq2db.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Pgvector`
- `Pgvector.EntityFrameworkCore`
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlite3`
- `SQLitePCLRaw.bundle_e_sqlite3mc` — Multiple Ciphers bundle; encrypted embedded floor under a KMS-custodied key.

[SCALEOUT_BACKENDS]: Dedicated scale-out store clients and embedded KV engines beyond the relational tier, each a distinct backend class.
- `ClickHouse.Driver` — Distributed columnar OLAP client; the billion-row lane beyond in-PG TimescaleDB and DuckDB.
- `ScyllaDBCSharpDriver` — CQL wide-column client driving ScyllaDB and Cassandra over one protocol.
- `Qdrant.Client` — Scale-out vector store; the billion-scale ANN class beyond in-PG `pgvector`.
- `DeltaLake.Net` — delta-rs Delta Lake read/write over S3/Azure/GCS for external-warehouse interop.
- `rocksdb` — Embedded LSM-tree write-optimized KV/log engine.
- `LightningDB` — LMDB memory-mapped B+tree read-optimized MVCC engine.

[COLUMNAR_QUERY]: In-process columnar engine, the Flight and ADBC transports, and the Substrait plan IR.
- `DuckDB.NET.Data.Full` — Drives the in-process DuckDB columnar lane, distinct from the `pg_duckdb` server bridge.
- `Apache.Arrow.Flight`
- `Apache.Arrow.Flight.AspNetCore` — Binds a `FlightServer` subclass onto an ASP.NET Core gRPC endpoint; sole holder of the server-adapter grant.
- `Apache.Arrow.Flight.Sql` — Flight SQL dialect over the Flight transport; `FlightSqlClient` verbs and the `FlightSqlServer` served-node base.
- `Apache.Arrow.Adbc`
- `Apache.Arrow.Adbc.Drivers.Apache` — Pure-managed Thrift+Arrow ADBC over Hive, Impala, and Spark.
- `Apache.Arrow.Adbc.Drivers.BigQuery` — Pure-managed BigQuery ADBC cloud-warehouse lane.
- `FlowtideDotNet.Substrait` — Substrait portable query-plan IR backing the federation rail and the one residence lowering.

[COLUMNAR_FORMATS]: Columnar file formats, the lake scanner, and content-defined chunking.
- `Apache.Arrow.Compression` — Arrow-IPC `Lz4Frame`/`Zstd` codec factory.
- `ParquetSharp` — Native libparquet Parquet read and write.
- `ParquetSharp.Dataset` — Partitioned multi-file Parquet lake scanner streaming `Apache.Arrow` batches with predicate and column pushdown.
- `FastCDC.Net`
- `Ara3D.BimOpenSchema`
- `Ara3D.BimOpenSchema.IO`
- `Parquet.Net` — Pure-managed Parquet codec under the BimOpenSchema Parquet-zip leg, version-governed as a central transitive pin.

[WIRE_SNAPSHOT_CODECS]: Row, snapshot, and schema codecs with their compression belt.
- `Chr.Avro` — Avro schema derivation, resolution, evolution, and POCO mapping for the registry codec profile.
- `Chr.Avro.Binary`
- `System.Formats.Cbor` — BCL CBOR / RFC 8949 self-describing snapshot codec.
- `JsonSchema.Net` — JSON Schema 2020-12 evaluator; the in-process `pg_jsonschema` fallback.
- `ZstdSharp.Port` — Standalone Zstandard snapshot and blob compression.
- `K4os.Compression.LZ4`
- `K4os.Compression.LZ4.Streams` — Separate distribution carrying the `Stream` frame adapters the object-plane codec row composes.

[FILE_CODECS]: Spreadsheet, schedule, and scan file codecs.
- `MiniExcel` — Streaming `.xlsx`/`.csv` codec; the spreadsheet lane `Sep` cannot reach.
- `MPXJ.Net` — MS-Project, P6, and Asta schedule-file codec the `Sep`/`MiniExcel` lanes lack.
- `Aardvark.Data.E57` — ASTM E57 scan decode behind the `Ingest/pointcloud` residence leg: header, `Data3D` setup, CompressedVector stream.
- `Sep`

[APPEND_EGRESS]: Marten append substrate, the out-of-Rhino sync transports, and the CDC change-egress pipeline.
- `Marten` — PostgreSQL event store; `GraphDelta` bodies fold `ElementGraph` via `AggregateStreamAsync` AS-OF.
- `Confluent.Kafka`
- `OpenTelemetry.Instrumentation.ConfluentKafka` — Instrumented producer/consumer builders carrying messaging spans and meters.
- `Confluent.SchemaRegistry` — Schema Registry REST client, subject compatibility and evolution.
- `Confluent.SchemaRegistry.Serdes.Avro`
- `Confluent.SchemaRegistry.Serdes.Json`
- `AMQPNetLite.Core` — `AMQP 1.0` protocol transport beneath the CloudEvents binding: connection, session, links, framing.
- `CloudNative.CloudEvents.Amqp` — CloudEvents AMQP 1.0 binding; the AMQP-native egress path distinct from the `RabbitMQ.Client` 0-9-1 leg.
- `CloudNative.CloudEvents.Kafka` — Binary-mode `ce_` header binding onto `Confluent.Kafka`; backs the `kafka` binding row.
- `RabbitMQ.Client` — AMQP 0-9-1 with publisher confirms; backs the `rabbitmq` binding row.
- `DotPulsar` — Apache Pulsar binary-protocol client; backs the `pulsar` binding row.
- `Pidgin` — Allocation-light parser combinators; the table-driven CESQL grammar behind the `sql` dialect at `Version/egress#SUBSCRIPTION_FILTER`.

[OBJECT_CUSTODY]: Cloud object stores and the KMS key custody beneath the content-keyed object plane.
- `AWSSDK.S3`
- `OpenTelemetry.Instrumentation.AWS` — One root registration spanning the S3 and KMS legs through the shared `AWSSDK.Core` pipeline.
- `Azure.Storage.Blobs`
- `Azure.Storage.Blobs.Batch` — Separate distribution carrying the blob batch client the object plane's page-at-a-time erase folds.
- `Google.Cloud.Storage.V1`
- `Minio` — Endpoint-agnostic S3-compatible client for the self-hosted lane.
- `AWSSDK.KeyManagementService`
- `Azure.Security.KeyVault.Keys`
- `Google.Cloud.Kms.V1`

[CACHE_BACKPLANE]: Redis backplane serving the read-lane cache tier and the egress binding row.
- `StackExchange.Redis` — Backs the `Query/cache` L2 backplane and the `Version/egress` `redis` binding row.
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `OpenTelemetry.Instrumentation.StackExchangeRedis` — Trace-only command spans hooking the cache and egress multiplexers into the root trace.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/dotnet/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `JetBrains.Annotations`
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `System.IO.Hashing`
- `QuikGraph` — Models the in-process topology the synchronous `Query/topology` lane composes.
- `Riok.Mapperly` — Generated boundary transcription for the package's structural seams; `Store/schema` composes generated parity messages directly.
- `Generator.Equals` — Generated structural equality; `Version/commits` payload-true CRDT state equality; content keys stay `XxHash128`.
- `CommunityToolkit.HighPerformance` — Spans, memory pools, and bit primitives on the cache and object-store path.
- `System.Numerics.Tensors` — SIMD `TensorPrimitives` backing the `VECTOR_CODEBOOK` PQ k-means and ADC scan.

[DATA_SUBSTRATE]:
- `Apache.Arrow` — Columnar format and Arrow IPC wire; the ADBC/Flight/compression egress train rides folder-side (`api-arrow-egress.md`).
- `Microsoft.Data.Sqlite` — Embedded ADO.NET transport and the `Handle` raw bridge beneath the store-profile rail.

[EXCHANGE_SUBSTRATE]:
- `Speckle.Sdk` — Send half: serialiser, transports, and client behind `SyncTransport.SpeckleLikeDiff`.
- `Speckle.Objects` — `Base`-derived geometry and `DataObject` shapes a sync marshal targets.
- `Unofficial.laszip.netstandard` — One LAS/LAZ engine behind chunked residence and `.lax` windowed reads.
- `PollinationSDK` — Cloud-run transport, sidecar-only; the durable `Version/provenance` `CloudRunFact` half.

[PLANAR_GEOSPATIAL]:
- `NetTopologySuite` — `Geometry` currency and WKB/WKT core codecs behind every spatial column, satellite codec, and geometry content key.
- `NetTopologySuite.IO.GeoJSON4STJ` — GeoJSON text on the `Ingest/geospatial#FEATURE_ROWS` seam and the web egress projection.
- `NetTopologySuite.IO.GeoPackage` — GeoPackage geometry-BLOB coding on the same feature-rows seam.
- `pocketken.H3` — Managed Uber-H3 v4 hex indexing; the same cell id at ingest and in PostgreSQL as `h3-pg`.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — `CloudEvent` values `Version/egress` mints per `OpLogEntry` through the kernel envelope owner holding codec identity.
- `NATS.Net` — JetStream publish-ack egress backing the `nats` binding row; KV and Object Store as distributed store-backend rows.
- `MQTTnet` — QoS-1 `PublishAsync` PUBACK evidence and the v5 User Property carrier the branch-owned MQTT binding writes unprefixed attributes onto.

[WIRE_CODEGEN]:
- `Celly.Protovalidate` — Validates completed generated event extensions and CRDT operation payloads at authoring and ingress boundaries.
- `Google.Protobuf` — Field masks, bounded parsing, foreign Substrait descriptors, and generated CRDT payload runtime.
- `Grpc.Core.Api` — `SyncService.SyncServiceBase`, `SyncServiceClient`, `CallInvoker`, `IServerStreamWriter<T>`, `ServerCallContext`; both ends.
- `MessagePack` — Snapshot codec and the uncompressed thirteen-slot op-log envelope; generated protobuf fills `[Key(6)] Payload` alone.
- `MessagePackAnalyzer` — Build-only generator and `MsgPack###` gate behind the AOT resolver chain.

[HOST_SERVICES]:
- `Microsoft.Extensions.Caching.Hybrid` — L2-store and serializer half of the AppHost-owned two-tier cache.
- `Microsoft.Extensions.Compliance.Redaction` — Classification attributes on egressed members; redactor binding stays at the app root.
- `Microsoft.Extensions.Telemetry.Abstractions` — Pooled per-operation latency ledger the `Query/lane` read phases stamp; activation stays app-root.

[RUNTIME_INBOX]:
- `System.Net.Http` — Blob-store client, ranged reads, and multipart upload legs of the object plane.
- `System.Text.Json` — Generated wire contexts and the `JsonDocument`/`JsonElement` payload plane.
- `System.Threading.Channels` — Bounded fan-out lanes behind the changefeed, outbox, and egress pump, and the AMQP leg's in-flight bound.

[TEST_SUBSTRATE]: Rows bind in branch test projects, never the package csproj.
- `Verify.XunitV3`
