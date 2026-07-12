# [RASM_PERSISTENCE_API_QDRANT]

`Qdrant.Client` is the official dedicated scale-out / distributed vector-store retrieval client — the billion-scale ANN store class beyond the in-PG `pgvector`/`pgvectorscale` tier (`api-pgvector-ef`, `api-pgvectorscale`). It is a thin async façade (`QdrantClient`) over a gRPC channel (`QdrantGrpcClient`) speaking the Qdrant protobuf model (`Qdrant.Client.Grpc`): named/sparse/multi-vector collections, server-side quantization (scalar/product/binary), the universal `QueryAsync` retrieval API with `PrefetchQuery` fusion and score-boosting `Formula` reranking, server-side payload `Filter` push-down, multitenant `ShardKey` partitioning, and collection/named-snapshot lifecycle. Every read/write is `Task`-returning over `Grpc.Net.Client`; the proto types are `protobuf-net`/`Grpc.Tools`-generated and shared with the admitted gRPC stack. This is the external scale-out vector lane; the in-Postgres vector tier stays the default residence, and Qdrant owns the case where ANN cardinality or recall tuning exceeds what a `pgvector` HNSW index serves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Qdrant.Client`
- package: `Qdrant.Client`
- license: Apache-2.0
- assembly: `Qdrant.Client`
- namespace: `Qdrant.Client` (façade), `Qdrant.Client.Grpc` (protobuf model + generated service clients)
- target: multi-target (`net6.0`, `netstandard2.0`, `net462`); the `net10.0` consumer binds `lib/net6.0` — pure-managed AnyCPU, no native runtime
- companion: transitive `Grpc.Net.Client` (channel), `Google.Protobuf` (wire model), `Grpc.Net.ClientFactory` (DI); all ride existing central gRPC rows (`Grpc.Net.Client`, `Google.Protobuf`)
- xml docs: `Qdrant.Client.xml` ships beside the assembly; member intent is doc-comment-sourced
- rail: vector-store-scaleout

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client roots
- rail: vector-store-scaleout

`QdrantClient` is the high-level async façade and the canonical surface; its ctors fan a `(string host, int port=6334, bool https=false, string? apiKey, TimeSpan grpcTimeout, ILoggerFactory?, IDictionary<string,string>? headers)` shape, a `(Uri address, …)` shape, and a `(QdrantGrpcClient grpcClient, …)` shape that wraps a pre-built channel (the DI-friendly seam where `Grpc.Net.ClientFactory` owns the channel lifecycle). `IQdrantClient` is the interface for substitution. `QdrantGrpcClient` exposes the raw generated service clients (`Collections`, `Points`, `Snapshots`, `Qdrant`) for protocol calls the façade does not surface.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :---------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `QdrantClient`          | async façade    | typed collection/point/snapshot ops     |
|  [02]   | `IQdrantClient`         | façade contract | substitution/abstraction surface        |
|  [03]   | `QdrantGrpcClient`      | gRPC root       | raw generated service clients + channel |
|  [04]   | `ClientConfiguration`   | channel config  | gRPC channel + retry/auth options       |
|  [05]   | `QdrantException`       | failure         | typed Qdrant gRPC failure wrapper       |
|  [06]   | `CertificateValidation` | TLS policy      | server certificate validation hook      |
|  [07]   | `RequestHeaders`        | header scope    | per-request gRPC metadata scope         |

[PUBLIC_TYPE_SCOPE]: collection and vector configuration model (`Qdrant.Client.Grpc`)
- rail: vector-store-scaleout

`VectorParams` configures a single named vector space (size + `Distance`); `VectorParamsMap` configures multiple named vectors per point; `SparseVectorConfig` configures sparse (term-weight) vectors; `MultiVectorConfig` configures multivector / late-interaction (ColBERT-style) spaces. `QuantizationConfig` selects `ScalarQuantization`, `ProductQuantization`, or `BinaryQuantization` for memory-compressed ANN. `HnswConfigDiff`/`OptimizersConfigDiff`/`WalConfigDiff` tune the index graph, background optimizers, and write-ahead log.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]      | [RAIL]                                |
| :-----: | :------------------------------------------------------------------ | :----------------- | :------------------------------------ |
|  [01]   | `VectorParams`                                                      | vector config      | size + `Distance` for one named space |
|  [02]   | `VectorParamsMap`                                                   | vector config      | multiple named vector spaces          |
|  [03]   | `SparseVectorConfig`                                                | sparse config      | sparse term-weight vector space       |
|  [04]   | `MultiVectorConfig`                                                 | multivector config | late-interaction (ColBERT) space      |
|  [05]   | `QuantizationConfig`                                                | quantization       | scalar/product/binary selector        |
|  [06]   | `ScalarQuantization` / `ProductQuantization` / `BinaryQuantization` | quantization       | memory-compressed ANN variants        |
|  [07]   | `HnswConfigDiff`                                                    | index tuner        | M / ef_construct / on-disk HNSW graph |
|  [08]   | `OptimizersConfigDiff`                                              | optimizer tuner    | segment/indexing/flush thresholds     |
|  [09]   | `Distance`                                                          | metric enum        | `Cosine`/`Euclid`/`Dot`/`Manhattan`   |
|  [10]   | `CollectionInfo`                                                    | collection state   | status, vector count, config snapshot |

[PUBLIC_TYPE_SCOPE]: point, payload, and filter model (`Qdrant.Client.Grpc`)
- rail: vector-store-scaleout

`PointStruct` is one upsertable point (`PointId` + `Vectors` + payload `Value` map); `Vectors`/`Vector`/`NamedVectors`/`SparseVector` carry the dense/named/sparse vector payloads; `Document` carries raw text/image for server-side inference (Qdrant computes the embedding). `Filter` composes `Condition`s (`must`/`should`/`must_not`) over payload fields for server-side push-down; `PayloadIncludeSelector`/`WithPayloadSelector`/`WithVectorsSelector` shape the returned payload/vectors. `PayloadSchemaType`/`FieldType`/`TokenizerType` drive payload-index creation (keyword/integer/float/geo/text/datetime/uuid).

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]    | [RAIL]                                    |
| :-----: | :-------------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `PointStruct`                                       | point            | id + vectors + payload to upsert          |
|  [02]   | `PointId`                                           | point id         | numeric or UUID point identity            |
|  [03]   | `Vectors` / `Vector` / `NamedVectors`               | vector payload   | dense/named dense vector data             |
|  [04]   | `SparseVector`                                      | sparse payload   | (indices, values) sparse vector           |
|  [05]   | `Document`                                          | inference input  | text/image for server-side embedding      |
|  [06]   | `Filter`                                            | query filter     | must/should/must_not condition tree       |
|  [07]   | `Condition`                                         | filter leaf      | field match/range/geo/has-id condition    |
|  [08]   | `WithPayloadSelector` / `PayloadIncludeSelector`    | payload selector | include/exclude returned payload          |
|  [09]   | `PayloadSchemaType` / `FieldType` / `TokenizerType` | index type       | payload-index field kind + text tokenizer |
|  [10]   | `ScoredPoint` / `UpdateResult` / `UpdateStatus`     | result           | scored hit / write ack + status           |

[PUBLIC_TYPE_SCOPE]: universal query and retrieval model (`Qdrant.Client.Grpc`)
- rail: vector-store-scaleout

`Query` is the universal retrieval discriminant (nearest, recommend, discover, context, order-by, fusion, formula, sample); `PrefetchQuery` carries a nested prefetch stage so a single `QueryAsync` runs a multi-stage hybrid pipeline (e.g. sparse prefetch → dense rerank). `Fusion` selects `Rrf`/`Dbsf` reciprocal/distribution-based fusion across prefetches; `Formula` is the score-boosting expression for payload-weighted reranking. `RecommendStrategy` selects the recommend scoring; `OrderBy` drives payload-ordered scroll.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]      | [RAIL]                                    |
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
- rail: vector-store-scaleout

`CreateCollectionAsync` fans `VectorParams` (single space) and `VectorParamsMap` (named spaces), each with an optional metadata `Dictionary<string,Value>` overload, and carries the full config: `shardNumber`/`replicationFactor`/`writeConsistencyFactor`, `onDiskPayload`, `HnswConfigDiff`, `OptimizersConfigDiff`, `WalConfigDiff`, `QuantizationConfig`, `SparseVectorConfig`, `ShardingMethod`, `StrictModeConfig`, `initFromCollection`, `timeout`.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]    | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `CreateCollectionAsync(name, VectorParams, …)`                             | async create    | creates a single-vector collection         |
|  [02]   | `CreateCollectionAsync(name, VectorParamsMap, …)`                          | async create    | creates a named-vector collection          |
|  [03]   | `RecreateCollectionAsync(name, …)`                                         | async create    | drops then recreates a collection          |
|  [04]   | `UpdateCollectionAsync(name, …)`                                           | async update    | mutates HNSW/optimizer/quantization config |
|  [05]   | `CollectionExistsAsync(name)` / `DeleteCollectionAsync(name)`              | async lifecycle | existence probe / drop                     |
|  [06]   | `GetCollectionInfoAsync(name)` / `ListCollectionsAsync()`                  | async read      | collection state / roster                  |
|  [07]   | `CreatePayloadIndexAsync(name, field, schemaType, …)`                      | async index     | builds a payload field index               |
|  [08]   | `CreateShardKeyAsync(name, shardKey, …)` / `ListShardKeysAsync(name)`      | async shard     | multitenant shard partition mgmt           |
|  [09]   | `CreateAliasAsync` / `UpdateAliasesAsync` / `ListAliasesAsync`             | async alias     | atomic collection alias swap               |
|  [10]   | `GetCollectionClusterSetupInfoAsync` / `UpdateCollectionClusterSetupAsync` | async cluster   | shard placement / replica mgmt             |

[ENTRYPOINT_SCOPE]: point write
- rail: vector-store-scaleout

`UpsertAsync(name, IReadOnlyList<PointStruct>, …)` is the canonical write; an overload takes an `updateFilter` for conditional upsert, and every write carries `wait` (block until indexed), `WriteOrderingType? ordering`, and `ShardKeySelector? shardKeySelector`. All writes return `UpdateResult` with `UpdateStatus`.

| [INDEX] | [SURFACE]                                                         | [CALL_SHAPE] | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------- | :----------- | :----------------------------------- |
|  [01]   | `UpsertAsync(name, points, wait, ordering, shardKeySelector)`     | async write  | inserts/replaces points              |
|  [02]   | `UpsertAsync(name, points, updateFilter, …)`                      | async write  | conditional upsert under a filter    |
|  [03]   | `UpdateVectorsAsync` / `DeleteVectorsAsync`                       | async write  | mutates/removes named vectors only   |
|  [04]   | `SetPayloadAsync` / `OverwritePayloadAsync` / `ClearPayloadAsync` | async write  | merge/replace/clear payload          |
|  [05]   | `DeletePayloadAsync` / `DeletePayloadIndexAsync`                  | async write  | removes payload keys / a field index |
|  [06]   | `DeleteAsync(name, PointsSelector, …)`                            | async write  | deletes points by id list or filter  |
|  [07]   | `UpdateBatchAsync(name, operations, …)`                           | async write  | one atomic multi-operation batch     |

[ENTRYPOINT_SCOPE]: retrieval
- rail: vector-store-scaleout

`QueryAsync` is the universal retrieval entry: `(name, Query?, IReadOnlyList<PrefetchQuery>?, usingVector, Filter?, scoreThreshold, SearchParams?, limit, offset, payloadSelector, vectorsSelector, ReadConsistency?, ShardKeySelector?, lookupFrom, timeout)`. The legacy `SearchAsync`/`RecommendAsync`/`DiscoverAsync` remain; `*GroupsAsync` group results by a payload field; `*BatchAsync` runs many queries in one request; `SearchMatrixOffsetsAsync`/`SearchMatrixPairsAsync` compute the pairwise similarity matrix; `FacetAsync` aggregates payload-value counts.

| [INDEX] | [SURFACE]                                                                                              | [CALL_SHAPE]    | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `QueryAsync(name, query, prefetch, …)`                                                                 | async query     | universal hybrid/fusion/formula retrieval |
|  [02]   | `QueryBatchAsync` / `QueryGroupsAsync`                                                                 | async query     | batched / payload-grouped universal query |
|  [03]   | `SearchAsync(name, ReadOnlyMemory<float>, filter, searchParams, limit, …)`                             | async search    | dense ANN search with filter              |
|  [04]   | `SearchBatchAsync` / `SearchGroupsAsync`                                                               | async search    | batched / grouped dense search            |
|  [05]   | `RecommendAsync` / `RecommendBatchAsync` / `RecommendGroupsAsync`                                      | async recommend | positive/negative example search          |
|  [06]   | `DiscoverAsync` / `DiscoverBatchAsync`                                                                 | async discover  | context-pair guided discovery search      |
|  [07]   | `RetrieveAsync(name, PointId \| Guid \| ulong \| IReadOnlyList<PointId>, withPayload, withVectors, …)` | async read      | fetches points by id                      |
|  [08]   | `ScrollAsync(name, filter, limit, orderBy, …)`                                                         | async scroll    | paged payload-filtered enumeration        |
|  [09]   | `CountAsync(name, filter, exact)`                                                                      | async count     | filtered point count                      |
|  [10]   | `FacetAsync(name, key, filter, …)`                                                                     | async facet     | payload-value aggregation                 |
|  [11]   | `SearchMatrixPairsAsync` / `SearchMatrixOffsetsAsync`                                                  | async matrix    | pairwise similarity matrix                |

[ENTRYPOINT_SCOPE]: snapshot and health
- rail: vector-store-scaleout

| [INDEX] | [SURFACE]                                                                                  | [CALL_SHAPE]   | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `CreateSnapshotAsync(name)` / `ListSnapshotsAsync(name)`                                   | async snapshot | per-collection snapshot create/list |
|  [02]   | `DeleteSnapshotAsync(name, snapshotName)`                                                  | async snapshot | drops a collection snapshot         |
|  [03]   | `CreateFullSnapshotAsync()` / `ListFullSnapshotsAsync()` / `DeleteFullSnapshotAsync(name)` | async snapshot | whole-storage snapshot              |
|  [04]   | `HealthAsync()`                                                                            | async probe    | server liveness + version           |

## [04]-[IMPLEMENTATION_LAW]

[QDRANT_TOPOLOGY]:
- two layers: `QdrantClient` (typed async façade, the canonical surface) over `QdrantGrpcClient` (raw `Grpc.Net.Client` channel + the generated `Collections`/`Points`/`Snapshots`/`Qdrant` service clients). The façade maps friendly arguments to the `Qdrant.Client.Grpc` proto request types.
- the wire model (`Qdrant.Client.Grpc`) is `Google.Protobuf`-generated; every model type carries the implicit conversions the façade overloads rely on (e.g. a `ulong`/`Guid`/`string` widens to `PointId`, a `float[]`/`ReadOnlyMemory<float>` widens to a `Vector`). These conversions are why the façade can accept primitive ids/vectors.
- gRPC port default is `6334` (binary), distinct from the `6333` REST port; `https`/`apiKey`/`headers` configure TLS and the `api-key` metadata. The `QdrantGrpcClient`-ctor overload is the DI seam where `Grpc.Net.ClientFactory` owns channel pooling.
- every façade method is `Task`-returning and `CancellationToken`-aware; there is no sync mirror — the client is async-only.

[LOCAL_ADMISSION]:
- the in-Postgres vector tier (`pgvector`/`pgvectorscale`) is the default residence; Qdrant enters behind the same `Query/retrieval` `VectorBackend` vocabulary only for the scale-out case (cardinality/recall/quantization beyond a `pgvector` HNSW index).
- a write enters through `UpsertAsync` with `wait` set per the durability profile; the `UpdateResult.Status` (`UpdateStatus`) is the write receipt that advances the `Query/retrieval` ingest ledger, never a fire-and-forget.
- retrieval enters through `QueryAsync` (the universal API), not the legacy `SearchAsync`, so hybrid prefetch + fusion + formula reranking are expressible in one round-trip; `SearchAsync`/`RecommendAsync` are admitted only for the single-stage dense case.
- payload `Filter` is the server-side push-down: the filter is built from the canonical query vocabulary and runs on the Qdrant node, never re-filtered client-side after a full fetch.
- multitenancy is `ShardKey`-based: a tenant's points carry a shard key and queries pass a `ShardKeySelector`, binding the collection to the `Element/identity` tenancy row.

[STACKING]:
- vector-tier discrimination: the `Query/retrieval` `VectorBackend` row discriminates `pgvector`/`pgvectorscale` (in-PG, default) from Qdrant (scale-out) on cardinality/recall; the canonical vector value object and `Distance` metric are shared, so the same embedding crosses both tiers — `api-pgvector-ef`/`api-pgvectorscale` own the in-PG leg, this catalog owns the scale-out leg.
- embedding ingest: a `[ValueObject]` embedding owner projects to a `Vector`/`SparseVector` through the implicit conversion; the payload map is the `Element/graph` element fact projected to a protobuf `Value` map. Server-side inference (`Document`) is the seam where Qdrant computes the embedding from raw text, bypassing a client-side embedding model.
- telemetry: the `QdrantClient(loggerFactory)` ctor wires `Microsoft.Extensions.Logging`; the gRPC channel's `Activity` spans feed the AppHost `telemetry` port through the admitted OpenTelemetry gRPC instrumentation — the query latency/recall is a span, not a bespoke logger. A `QdrantException` lifts at the façade edge onto the store-profile failure rail.
- retry/deadline: the gRPC `ClientConfiguration` carries the `grpcTimeout` deadline; transient gRPC status codes are retried by the channel's service config, aligning Qdrant's resilience with the `Polly`/`stamina`-shaped engine retry the other transports use rather than a hand-rolled retry loop.
- snapshot residence: `CreateSnapshotAsync` produces a Qdrant-native snapshot; its bytes land in the same object-store residence (`api-objectstore`/`Minio`) as the Parquet/Delta extracts, so the vector store's backup shares the `Store/blobstore` lane.

[RAIL_LAW]:
- Package: `Qdrant.Client`
- Owns: scale-out / distributed vector-store retrieval — named/sparse/multi-vector collections, server-side quantization, the universal `QueryAsync` hybrid/fusion/formula API, server-side payload filtering, multitenant sharding, and collection/snapshot lifecycle
- Accept: the typed `QdrantClient` façade, `UpsertAsync` with an `UpdateStatus` receipt, `QueryAsync` with `PrefetchQuery`/`Fusion`/`Formula`, server-side `Filter` push-down, and `ShardKey` multitenancy
- Reject: client-side payload filtering after a full fetch, the legacy `SearchAsync` where a hybrid `QueryAsync` is needed, a hand-rolled gRPC retry loop where the channel service config owns it, and treating Qdrant as the default vector residence over the in-PG tier
