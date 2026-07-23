# [RASM_PERSISTENCE_API_QDRANT]

`Qdrant.Client` owns the scale-out distributed vector-store lane, the billion-scale ANN residence past the in-Postgres `pgvector` tier. `QdrantClient` fronts a `QdrantGrpcClient` gRPC channel speaking the `Qdrant.Client.Grpc` protobuf model, folding named, sparse, and multi-vector collections, server-side quantization, the universal `QueryAsync` fusion/formula retrieval API, payload push-down, and multitenant sharding onto one async surface. In-Postgres `pgvector` holds the default residence; Qdrant enters where ANN cardinality or recall tuning exceeds an HNSW index.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Qdrant.Client`
- package: `Qdrant.Client` (Apache-2.0)
- assembly: `Qdrant.Client`
- namespace: `Qdrant.Client` façade, `Qdrant.Client.Grpc` protobuf model with the generated service clients
- target: `net6.0`/`netstandard2.0`/`net462`; a `net10.0` consumer binds `lib/net6.0` — pure-managed AnyCPU, no native runtime
- depends: `Grpc.Net.Client` channel, `Google.Protobuf` wire model, `Grpc.Net.ClientFactory` DI, each riding an existing central gRPC row
- rail: vector-store-scaleout

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client roots

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                 |
| :-----: | :---------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `QdrantClient`          | async façade    | typed collection/point/snapshot ops          |
|  [02]   | `IQdrantClient`         | façade contract | substitution/abstraction surface             |
|  [03]   | `QdrantGrpcClient`      | gRPC root       | raw generated service clients + channel      |
|  [04]   | `ClientConfiguration`   | client options  | api key, TLS thumbprint, per-request headers |
|  [05]   | `QdrantException`       | failure         | typed Qdrant gRPC failure wrapper            |
|  [06]   | `CertificateValidation` | TLS policy      | server certificate validation hook           |
|  [07]   | `RequestHeaders`        | header scope    | per-request gRPC metadata scope              |

[PUBLIC_TYPE_SCOPE]: collection and vector configuration model (`Qdrant.Client.Grpc`)

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]      | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------ | :----------------- | :------------------------------------ |
|  [01]   | `VectorParams`                                                      | vector config      | size + `Distance` for one named space |
|  [02]   | `VectorParamsMap`                                                   | vector config      | multiple named vector spaces          |
|  [03]   | `SparseVectorConfig`                                                | sparse config      | sparse term-weight vector space       |
|  [04]   | `MultiVectorConfig`                                                 | multivector config | late-interaction (ColBERT) space      |
|  [05]   | `QuantizationConfig`                                                | quantization       | scalar/product/binary selector        |
|  [06]   | `ScalarQuantization` / `ProductQuantization` / `BinaryQuantization` | quantization       | memory-compressed ANN variants        |
|  [07]   | `HnswConfigDiff`                                                    | index tuner        | M / ef_construct / on-disk HNSW graph |
|  [08]   | `OptimizersConfigDiff`                                              | optimizer tuner    | segment/indexing/flush thresholds     |
|  [09]   | `WalConfigDiff`                                                     | wal tuner          | capacity, segments-ahead, retention   |
|  [10]   | `ShardingMethod`                                                    | shard enum         | `Auto` id-based / `Custom` key-based  |
|  [11]   | `StrictModeConfig`                                                  | collection limits  | per-collection query and limit caps   |
|  [12]   | `Distance`                                                          | metric enum        | `Cosine`/`Euclid`/`Dot`/`Manhattan`   |
|  [13]   | `CollectionInfo`                                                    | collection state   | status, vector count, config snapshot |

[PUBLIC_TYPE_SCOPE]: point, payload, and filter model (`Qdrant.Client.Grpc`)

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `PointStruct`                                       | point            | id + vectors + payload to upsert          |
|  [02]   | `PointId`                                           | point id         | numeric or UUID point identity            |
|  [03]   | `Vectors` / `Vector` / `NamedVectors`               | vector payload   | dense/named dense vector data             |
|  [04]   | `SparseVector`                                      | sparse payload   | (indices, values) sparse vector           |
|  [05]   | `Document`                                          | inference input  | text/image for server-side embedding      |
|  [06]   | `Filter`                                            | query filter     | `must`/`should`/`must_not` condition tree |
|  [07]   | `Condition`                                         | filter leaf      | field match/range/geo/has-id condition    |
|  [08]   | `WithPayloadSelector` / `PayloadIncludeSelector`    | payload selector | include/exclude returned payload          |
|  [09]   | `WithVectorsSelector`                               | vector selector  | all vectors or a named-vector subset      |
|  [10]   | `PayloadSchemaType` / `FieldType` / `TokenizerType` | index type       | payload-index field kind + text tokenizer |
|  [11]   | `ScoredPoint` / `UpdateResult` / `UpdateStatus`     | result           | scored hit / write ack + status           |

[PUBLIC_TYPE_SCOPE]: universal query and retrieval model (`Qdrant.Client.Grpc`)

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]      | [CAPABILITY]                              |
| :-----: | :---------------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `Query`                             | query discriminant | nearest/recommend/discover/fusion/formula |
|  [02]   | `PrefetchQuery`                     | prefetch stage     | nested multi-stage hybrid prefetch        |
|  [03]   | `Fusion`                            | fusion enum        | `Rrf` / `Dbsf` cross-prefetch fusion      |
|  [04]   | `Formula`                           | score expression   | payload-weighted score boosting/reranking |
|  [05]   | `RecommendStrategy`                 | recommend enum     | average-vector vs. best-score strategy    |
|  [06]   | `OrderBy`                           | scroll order       | payload-field ordered scroll              |
|  [07]   | `SearchParams`                      | search tuning      | hnsw_ef / exact / quantization params     |
|  [08]   | `ReadConsistency` / `WriteOrdering` | consistency        | read-quorum / write-ordering selector     |
|  [09]   | `ShardKey`                          | shard partition    | multitenant shard-key partitioning        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: collection lifecycle

| [INDEX] | [SURFACE]                                                                  | [SHAPE]         | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `CreateCollectionAsync(name, VectorParams, …)`                             | async create    | creates a single-vector collection  |
|  [02]   | `CreateCollectionAsync(name, VectorParamsMap, …)`                          | async create    | creates a named-vector collection   |
|  [03]   | `RecreateCollectionAsync(name, …)`                                         | async create    | drops then recreates a collection   |
|  [04]   | `UpdateCollectionAsync(name, …)`                                           | async update    | mutates HNSW/optimizer/quantization |
|  [05]   | `CollectionExistsAsync(name)` / `DeleteCollectionAsync(name)`              | async lifecycle | existence probe / drop              |
|  [06]   | `GetCollectionInfoAsync(name)` / `ListCollectionsAsync()`                  | async read      | collection state / roster           |
|  [07]   | `CreatePayloadIndexAsync(name, field, schemaType, …)`                      | async index     | builds a payload field index        |
|  [08]   | `CreateShardKeyAsync(name, shardKey, …)` / `ListShardKeysAsync(name)`      | async shard     | multitenant shard partition mgmt    |
|  [09]   | `CreateAliasAsync` / `UpdateAliasesAsync` / `ListAliasesAsync`             | async alias     | atomic collection alias swap        |
|  [10]   | `GetCollectionClusterSetupInfoAsync` / `UpdateCollectionClusterSetupAsync` | async cluster   | shard placement / replica mgmt      |

[ENTRYPOINT_SCOPE]: point write

| [INDEX] | [SURFACE]                                                         | [SHAPE]     | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------- | :---------- | :----------------------------------- |
|  [01]   | `UpsertAsync(name, points, wait, ordering, shardKeySelector)`     | async write | inserts/replaces points              |
|  [02]   | `UpsertAsync(name, points, updateFilter, …)`                      | async write | conditional upsert under a filter    |
|  [03]   | `UpdateVectorsAsync` / `DeleteVectorsAsync`                       | async write | mutates/removes named vectors only   |
|  [04]   | `SetPayloadAsync` / `OverwritePayloadAsync` / `ClearPayloadAsync` | async write | merge/replace/clear payload          |
|  [05]   | `DeletePayloadAsync` / `DeletePayloadIndexAsync`                  | async write | removes payload keys / a field index |
|  [06]   | `DeleteAsync(name, PointsSelector, …)`                            | async write | deletes points by id list or filter  |
|  [07]   | `UpdateBatchAsync(name, operations, …)`                           | async write | one atomic multi-operation batch     |

[ENTRYPOINT_SCOPE]: retrieval

| [INDEX] | [SURFACE]                                                                  | [SHAPE]         | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `QueryAsync(name, query, prefetch, …)`                                     | async query     | universal hybrid/fusion/formula retrieval |
|  [02]   | `QueryBatchAsync` / `QueryGroupsAsync`                                     | async query     | batched / payload-grouped universal query |
|  [03]   | `SearchAsync(name, ReadOnlyMemory<float>, filter, searchParams, limit, …)` | async search    | dense ANN search with filter              |
|  [04]   | `SearchBatchAsync` / `SearchGroupsAsync`                                   | async search    | batched / grouped dense search            |
|  [05]   | `RecommendAsync` / `RecommendBatchAsync` / `RecommendGroupsAsync`          | async recommend | positive/negative example search          |
|  [06]   | `DiscoverAsync` / `DiscoverBatchAsync`                                     | async discover  | context-pair guided discovery search      |
|  [07]   | `RetrieveAsync(name, ids, withPayload, withVectors, …)`                    | async read      | fetches points by id                      |
|  [08]   | `ScrollAsync(name, filter, limit, orderBy, …)`                             | async scroll    | paged payload-filtered enumeration        |
|  [09]   | `CountAsync(name, filter, exact)`                                          | async count     | filtered point count                      |
|  [10]   | `FacetAsync(name, key, filter, …)`                                         | async facet     | payload-value aggregation                 |
|  [11]   | `SearchMatrixPairsAsync` / `SearchMatrixOffsetsAsync`                      | async matrix    | pairwise similarity matrix                |

[ENTRYPOINT_SCOPE]: snapshot and health

| [INDEX] | [SURFACE]                                                | [SHAPE]        | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `CreateSnapshotAsync(name)` / `ListSnapshotsAsync(name)` | async snapshot | per-collection snapshot create/list |
|  [02]   | `DeleteSnapshotAsync(name, snapshotName)`                | async snapshot | drops a collection snapshot         |
|  [03]   | `CreateFullSnapshotAsync()` / `ListFullSnapshotsAsync()` | async snapshot | whole-storage snapshot create/list  |
|  [04]   | `DeleteFullSnapshotAsync(name)`                          | async snapshot | drops a whole-storage snapshot      |
|  [05]   | `HealthAsync()`                                          | async probe    | server liveness + version           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `QdrantClient` maps friendly arguments onto `QdrantGrpcClient`, the raw `Grpc.Net.Client` channel carrying the generated `Collections`/`Points`/`Snapshots`/`Qdrant` service clients.
- Implicit conversions on the `Google.Protobuf` model widen a `ulong`/`Guid`/`string` to `PointId` and a `float[]`/`ReadOnlyMemory<float>` to `Vector`, so the façade takes primitive ids and vectors directly.
- Binary gRPC binds port `6334`, distinct from the `6333` REST port; `https`/`apiKey`/`headers` configure TLS and the `api-key` metadata, and the `QdrantGrpcClient` ctor overload is the DI seam where `Grpc.Net.ClientFactory` owns channel pooling.
- Every façade method is `Task`-returning and `CancellationToken`-aware; the client is async-only.

[STACKING]:
- `api-pgvector-ef` / `api-pgvectorscale`: the `Query/retrieval` `VectorBackend` row discriminates the in-Postgres leg from this scale-out leg on cardinality and recall, and the canonical vector value object with its `Distance` metric crosses both, so one embedding serves either tier.
- `api-objectstore`: `CreateSnapshotAsync` bytes land in the object-store residence holding the Parquet and Delta extracts, so vector backup shares the `Store/blobstore` lane.
- embedding ingest: a `[ValueObject]` embedding owner projects onto `Vector`/`SparseVector` through the implicit conversion and the payload map carries the `Element/graph` element fact as a protobuf `Value` map; `Document` hands raw text to server-side inference, so Qdrant computes the embedding.
- telemetry: the `QdrantClient(loggerFactory)` ctor wires `Microsoft.Extensions.Logging` and the channel's `Activity` spans feed the AppHost `telemetry` port through the admitted OpenTelemetry gRPC instrumentation, making query latency and recall a span; `QdrantException` lifts at the façade edge onto the store-profile failure rail.
- resilience: the `grpcTimeout` ctor argument sets the per-call deadline and the channel's service config retries transient gRPC status codes, aligning Qdrant with the `Polly`-shaped engine retry the other transports carry.

[LOCAL_ADMISSION]:
- A write enters through `UpsertAsync` with `wait` set per the durability profile, and `UpdateResult.Status` is the receipt advancing the `Query/retrieval` ingest ledger.
- Retrieval enters through the universal `QueryAsync`, so hybrid prefetch, fusion, and formula reranking compose in one round-trip; `SearchAsync` and `RecommendAsync` serve the single-stage dense and example-guided cases.
- Payload `Filter` builds from the canonical query vocabulary and runs on the Qdrant node as server-side push-down.
- Multitenancy rides `ShardKey`: a tenant's points carry the shard key and queries pass a `ShardKeySelector`, binding the collection to the `Element/identity` tenancy row.

[RAIL_LAW]:
- Package: `Qdrant.Client`
- Owns: scale-out distributed vector-store retrieval — named/sparse/multi-vector collections, server-side quantization, the universal `QueryAsync` hybrid/fusion/formula API, server-side payload filtering, multitenant sharding, and collection/snapshot lifecycle
- Accept: the typed `QdrantClient` façade, `UpsertAsync` under an `UpdateStatus` receipt, `QueryAsync` with `PrefetchQuery`/`Fusion`/`Formula`, server-side `Filter` push-down, and `ShardKey` multitenancy
- Reject: client-side payload filtering after a full fetch, single-stage `SearchAsync` where `QueryAsync` expresses the hybrid pipeline, a hand-rolled gRPC retry loop the channel service config already owns, and Qdrant as the default vector residence over the in-Postgres tier
