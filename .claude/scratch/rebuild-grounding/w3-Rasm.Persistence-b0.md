# w3 · Rasm.Persistence/Query batch b0 — grounding dossier

VERIFIED PRIMARY EXTRACTS ONLY. Every claim carries a `file:line` anchor. Scope: `Query/{lane[rebuild], retrieval[new], topology[improve], columnar[improve]}`.

## [00]-[INVENTORIES] (real `ls`)

Shared substrate `.api` tier — `libs/csharp/.api/` (31 catalogs): api-csparse, api-extensions-ai, api-generator-equals, api-grpc-*, api-hashing, api-highperformance, api-hybrid-cache, api-jsonpatch, api-languageext, api-mapperly, api-mathnet-numerics, api-mathnet-providers, api-nodatime(+stj/protobuf), api-protobuf, api-quikgraph, api-redaction, api-system-configuration, api-tensors, api-thinktecture-{json,messagepack,runtime-extensions}, api-unicolour, api-unitsnet.

Folder `.api` tier — `libs/csharp/Rasm.Persistence/.api/` (78 catalogs): the retrieval/lane-relevant set includes api-pgvector-ef, api-pgvectorscale, api-pg-search, api-qdrant, api-marten, api-duckdb, api-arrow, api-adbc-apache, api-ara3d-bimopenschema, api-parquetsharp, api-flowtide-substrait, api-npgsql(+ef/nts), api-nts-*, api-h3(+pg), api-clickhouse, api-deltalake. Folder stubs pointing to shared tier: api-hashing.md (`:3` → `libs/csharp/.api/api-hybrid-cache.md`-style redirect), api-hybrid-cache.md:3, api-nodatime*.md, api-redaction.md, api-thinktecture-json.md.

Doctrine root — `docs/stacks/csharp/` (7 root pages: README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) + `docs/stacks/csharp/domain/` (14 shards incl. persistence, postgres, data-interchange, concurrency, durability).

Analyzer law: `dotnet_style_namespace_match_folder = true:error` — all four pages sit in `Query/` folder → namespace `Rasm.Persistence.Query` (confirmed topology.md:32, columnar.md:38).

## [01]-[DECISION / BRIEF SCOPE ANCHORS]

`RASM-CS-PERSISTENCE-DECISION.md`:
- `:106` lane.md `SPLIT · rebuild` — "Drops to routing + set-algebra: `ReadRouter` … `StalenessWatermark` sequence-gap, `ElementSet`/`SetExpr`/`ElementSetAlgebra.Evaluate` + length-framed `Canonical`. The `FUSION_AND_CACHE`+`VECTOR_CODEBOOK` ANN subsystem leaves to `retrieval`. Route `WaitForNonStaleProjectionDataAsync` through `IProjectionDaemon`/`IMartenDatabase`, not `TestingExtensions`. Band NONE (set-algebra total post-split)."
- `:107` retrieval.md `NEW · new · SPLIT-from Query/lane.md#[04]-[FUSION_AND_CACHE]+#[05]-[VECTOR_CODEBOOK] (:267-520)` — "`FusionRank.Fuse` … + `VectorCodebook.Train`/`AdcScan` … MINT `RetrievalFault : Expected` (kills the lane codebook's bare `Error.New(8360..8363)` :432-441). `[V6]` pgvector `<->`/`<=>` LINQ server-side ANN row beside the in-process PQ/ADC hot-set lane (residency is row data on the one retrieval axis…). Band Retrieval 8410. Entry `Retrieval.Fuse` + `VectorCodebook.Train`/`AdcScan` (`RetrievalOp` `[Union]`). Seams: `⇄ Rasm.Compute/Model/embedding` (VectorRow.ContentKey ↔ EmbeddingVector.ContentKey); `← pgvector` column map; `→ cache` fusion result."
- `:108` topology.md `KEEP · improve` — "`[V10]/NEW` `Lca` DAG-gate (`Lca`/`Ancestor` pre-gate `tree.IsDirectedAcyclicGraph()` and rail `TopologyFault.Cyclic`, symmetric with the already-gated `Order`; `api-quikgraph:26` flags `OfflineLeastCommonAncestor` unsound over cyclic input). `[V10]` `TypedEdge.IsContainment`/`Kind` dead-accessor prune."
- `:109` columnar.md `KEEP · improve` — "`[V10]/E14` typed trust gate (`Identifier`/`StorePath`/`SecretName` value-object family gates `Mount`/`Secret`/`Egress {projection}`…). `[V10]` posture constant (the four `80%`/`90%` literals `:80-83` collapse to one). `[V10]` phantom-spellings closed (`ExecuteQueryAsync`→`ExecuteQuery`; `BimData.WriteDuckDB`→`frames.ToDataSet().WriteToDuckDB`). ADD `ColumnarExtension.Substrait` row (DuckDB `from_substrait(blob)`, V1 lane)."
- `:154` fault band — "8410 `RetrievalFault` `Query/retrieval` 841x MINT (kills the lane codebook bare `Error.New(8360..8363)` :432-441 — the only un-banded lane owner)."
- `:177` "ARCH:55 reconciliation GeometryHash RETARGET `Query/topology` → `Version/merge#STRUCTURAL_DIFF` (topology zero reconciliation refs)."
- `:260` roster — "Vector search | `Query/retrieval` | pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · qdrant-scaleout | `VectorBackend` axis | `Qdrant.Client`…".

`RASM-CS-PERSISTENCE-BRIEF.md`:
- `:147` "E14 Parameterization unevenness | `columnar.md:256,356-361,391` raw-interpolated identifiers/paths beside `:225-227,271` bound values, no trust gate."
- `:178` "`Pgvector.EntityFrameworkCore` — `[V6]` ruled default: the `<->`/`<=>` LINQ distance path as the server-side ANN row on the retrieval owner; column-mapping-only is the demotion the retrieval draft must prove."
- `:188` (README substrate) `System.Numerics.Tensors` `TensorPrimitives.Distance/Add/Divide` back the PQ k-means with the SAME reduction Compute's nearest-centroid encode assigns with.

## [02]-[VERIFIED .API MEMBER BLOCKS]

### api-marten.md — watermark surface (lane rebuild)
- `:60` "`AdvancedOperations` class (`store.Advanced`) | `ResetAllData`, `FetchEventStoreStatistics`, `AllProjectionProgress`, `RebuildSingleStream…`".
- `:106` "`IProjectionDaemon` interface — the async-projection runner: `StartAllAsync`, `RebuildProjectionAsync`, `WaitForNonStaleData`…".
- `:108` "`ShardState` / `ShardName` record/struct — per-shard projection progress + the projection-shard identity".
- `:109` "`EventStoreStatistics` class — `FetchEventStoreStatistics` result (event/stream counts, sequence + projection hi…)".
- `:205` "`daemon.StartAllAsync()` / `RebuildProjectionAsync<TView>(…)` / `WaitForNonStaleData(TimeSpan)`…".
- `:219` "`host.WaitForNonStaleProjectionDataAsync(TimeSpan)` / `store.WaitForNonStaleProjectionDataAsync([tenantIdOrDatabaseName], TimeSpan)` (`Marten.Events`) | block until async projections are caught up". → the store/host overloads are the `Marten.Events` (TestingExtensions) convenience form; `IProjectionDaemon.WaitForNonStaleData` (`:106,205`) is the production path DECISION:106 mandates. Current lane uses the store overload at lane.md:84,124.

### api-hashing.md — XxHash128 (lane, retrieval)
- `:35` "`XxHash128` hash algorithm — 128-bit; `Low64`/`High64` fields; `GetCurrentHashAsUInt128`".
- `:50` "`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed = 0)` static `UInt128` — one-shot 128-bit value".
- `:63-65` "`Append(ReadOnlySpan<byte>)` / `Append(byte[])`; `Append(Stream)`; `AppendAsync(Stream, CancellationToken)`".
- `:83` "content-identity rail composes `XxHash128.HashToUInt128` at SEED ZERO as the one `ContentHash.Of` entry". (lane.md ElementSetAlgebra.Receipt/Canonical + retrieval ProductCodebook.KeyOf compose these; sound.)

### api-tensors.md — PQ float paths (retrieval)
- `:99` "`TensorPrimitives.Add` — computes elementwise op".
- `:102` "`Divide` — computes elementwise op".
- `:155` "`Distance` — computes Euclidean distance". (VectorCodebook.Train/AdcScan/Lloyd at lane.md:456,463,488,491,495 compose these; SAME reduction Compute assigns with — sound.)

### api-quikgraph.md — topology (improve) + set-algebra Expand seam
- `:26-27` "`Rasm.Bim` … `IsDirectedAcyclicGraph` pre-gated — Tarjan `OfflineLeastCommonAncestor` is unsound over the multi-parent commit DAG (rooted-tree contract), so the BFS closure IS the merge-base." → the catalog's own law that offline LCA needs a DAG/rooted-tree gate. topology.md:257-264 `Lca` runs `OfflineLeastCommonAncestor` with NO gate; topology.md:303 `Order` IS gated by `IsDirectedAcyclicGraph`.
- `:56` "`IEdge<out TVertex>` … a consumer-defined edge (the Persistence `TypedEdge` carrying a `Relationship` kind) is a first-class edge".
- `:58` "`SEquatableEdge<TVertex>` … the edge `OfflineLeastCommonAncestor` keys its query pairs on".
- `:8-9` facade owns "topological sort, BFS/DFS … offline least-common-ancestor, strongly/weakly-connected components and condensation, transitive closure/reduction, shortest paths (Dijkstra/A*/Bellman-Ford/DAG), minimum spanning tree, and maximum flow" — roster wider than topology.md's 13 cases.

### api-pgvector-ef.md — server-side ANN (retrieval)
- `:154-164` `VectorDbFunctionsExtensions` six methods `L2Distance`(`<->`)/`MaxInnerProduct`(`<#>`)/`CosineDistance`(`<=>`)/`L1Distance`(`<+>`)/`HammingDistance`(`<~>`)/`JaccardDistance`(`<%>`). (lane.md:302-307 binds all six by `nameof` — sound.)
- `:174` "`VectorMetric` rows bind each method by `nameof(VectorDbFunctionsExtensions.L2Distance)` and build the `ORDER BY` projection with `Expression.Call(typeof(VectorDbFunctionsExtensions), Fn, …, columnExpr, Expression.Constant(new Vector(probe)))`". → the server-side LINQ Order-leg the current owner does NOT realize.
- `:178-183` index builder `HasMethod`/`HasOperators`/`HasStorageParameter` (hnsw/ivfflat/opclass/`m`/`ef_construction`/`lists`).
- `:210-211` query GUCs — hnsw: `hnsw.ef_search`(40)/`hnsw.iterative_scan`/`hnsw.max_scan_tuples`; ivfflat: `ivfflat.probes`(1)/`ivfflat.max_probes`. → the SET-LOCAL binder surface the current owner lacks.
- `:217` "`binary_quantize(vector)` → `bit` … backs the `EmbeddingArity.Bit` `binary_quantize(emb)::bit(N)`". (lane.md:288 EmbeddingArity.Bit — sound.)

### api-pgvectorscale.md — diskann (retrieval)
- `:30` "held under `Query/retrieval#SEARCH_PROVISIONING_PROBE`" — the catalog ANTICIPATES a retrieval page section for halfvec/sparsevec-over-diskann + the probe binder.
- `:32-40` `DiskAnnOps` `[SmartEnum<string>]` Cosine/L2/InnerProduct carrying ops-class + operator.
- `:70-77` query GUCs `diskann.query_search_list_size`(100)/`diskann.query_rescore`(50) — "`SET LOCAL` per session/transaction by the `Query/lane#FUSION_AND_CACHE` binder".
- `:89-90` "the `search.vector.route` fact discriminates exact-scan vs HNSW vs IVFFlat vs diskann … `FusionRank.Fuse` … `SUM(1.0 / (rrfConstant + rank))` with `rrfConstant=60`". → canonical RRF constant; current lane.md:339 `Fuse(int k, …)` takes k as a param with no named 60 default and no ANN-route fact.

### api-pg-search.md — BM25 lexical (retrieval)
- `:7` "the `Query/lane#FUSION_AND_CACHE` `FusionRank.Fuse` BM25 branch matches through … Only the `pdb.*` builders and the bare column operators are emitted".
- `:39-47` `@@@` + 7 match operators (`|||`/`&&&`/`===`/`###`/`##`/`##>`).
- `:55-67` 11 `pdb.*` builders (parse/match/range_term/phrase_prefix/more_like_this/regex/all + `::pdb.fuzzy`/`boost`/`const`/`slop`).
- `:80-86` `SearchProjection` functions `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg`.
- `:90-94` "the `Bm25Predicate` `[Union]` is the C# projection of section `[04]` — one union case per builder/operator/cast, `Bm25Predicate.Sql()` switching … the `SearchProjection` static surface is the C# projection of section `[05]`". → the catalog names a typed `Bm25Predicate` union + `SearchProjection` surface the current lexical branch (lane.md:271,317,323 — ONE `RetrievalBranch.Lexical` row + prose) never realizes.

### api-qdrant.md — scale-out vector (retrieval forward roster)
- head "`QdrantClient` … `QueryAsync` retrieval API with `PrefetchQuery` fusion and score-boosting `Formula` reranking, server-side payload `Filter` push-down … `QuantizationConfig` selects `ScalarQuantization`, `ProductQuantization`, or `BinaryQuantization`". → the `VectorBackend.QdrantScaleout` provider row (DECISION:260).

### api-duckdb.md / api-arrow.md — columnar (improve)
- api-duckdb.md:239 "`substrait` `community` — `get_substrait('⟨sql⟩')`/`get_substrait_json` → binary/JSON Substrait plan; `from_substrait(⟨blob⟩)`".
- api-duckdb.md:265 "the `substrait` COMMUNITY extension (`INSTALL substrait FROM community; LOAD substrait;`)". → the Substrait row needs a community-repo-aware Bootstrap; columnar.md:69 `Bootstrap` emits only `INSTALL {Key}; LOAD {Key};` (no `FROM community`).
- api-duckdb.md:61,119,124-125 `DuckDBMappedAppender<T,TMap>`/`CreateAppender<T,TMap>`/`.AppendRecords`/`.Close` (columnar.md:238-244 — sound).
- api-arrow.md:239-242 `AdbcStatement.SqlQuery`/`SubstraitPlan`; `ExecuteQuery()`/`ExecuteQueryAsync()` → `QueryResult`; `ExecutePartitioned()`. (columnar.md:284,289 ArrowStream binds only `SqlQuery`+`ExecuteQueryAsync`; `SubstraitPlan`/`ExecutePartitioned` unexploited.)
- api-arrow.md:124,135-136 `IArrowArrayStream` (`Schema`+`ReadNextRecordBatchAsync`); `AdbcStatement`; `QueryResult` (`long RowCount` + `IArrowArrayStream? Stream`).

### api-ara3d-bimopenschema.md — flat-table egress (columnar improve)
- `:97` "`data.WriteDuckDB` — `void WriteDuckDB(this BimData, FilePath)`".
- `:108` "`set.WriteToDuckDB` — `void WriteToDuckDB(this IDataSet, FilePath)`". → BOTH `BimData.WriteDuckDB` (columnar.md:468) and the `ToDataSet().WriteToDuckDB` path are real; DECISION:16 prefers the explicit `frames.ToDataSet().WriteToDuckDB` spelling (alignment, not a hard phantom).
- `:124-126` `[DATASET_BRIDGE]` — `ToDataSet()` projects eleven `IDataTable`s; the DuckDB writer suffixes each with the projection ordinal (`Points_0`…`Entities_4`…). (columnar.md:406,410 `<Name>_<n>` ordinal — sound.)

### api-hybrid-cache.md (shared) — result cache (retrieval absorbs FUSION ResultCache)
- shared `libs/csharp/.api/api-hybrid-cache.md:32-45` `HybridCache.GetOrCreateAsync` polymorphic family, `SetAsync`, `RemoveAsync`, `RemoveByTagAsync`; `HybridCacheEntryOptions` (`Expiration`/`LocalCacheExpiration`/`Flags`). (lane.md:357-366 `CachedEvaluate` composes `GetOrCreateAsync`+`HybridCacheEntryOptions`+tags — sound; moves to retrieval with the fusion per DECISION:107 `→ cache` seam.)

## [03]-[FOLDER-CONTEXT ANCHORS]

- ARCHITECTURE.md:25 codemap `Lane.cs` = "ReadRouter … the ElementSet selection algebra, FusionRank, the HybridCache read-through" (pre-split); `:64` seam `Query/lane ⇄ python:data/tabular/query [WIRE] ElementSet receipt currency + Substrait portable plan`.
- README.md:18-22 router rows `[12]-[QUERY_LANE]`…`[16]-[QUERY_CACHE]`; README.md:52 `Pgvector`+`Pgvector.EntityFrameworkCore`, `:68-70` server extensions `pgvector`/`pgvectorscale`/`pg_search`, `:52` `Qdrant.Client` STORE_BACKENDS forward row.
- cache.md (sibling, distinct owner) = compute-result reuse index: `ArtifactIndexRow`/`ModelResultIndex`/`BenchmarkRow`/`CacheL2Store` (cache.md:14,71,140,179) — NOT the fusion ResultCache; the retrieval `→ cache fusion result` seam is the selection-result HybridCache, keyed on `ElementSet.Receipt` (lane.md:359), which stays a lane-owned identity.
- cypher.md (sibling) = OPTIONAL AGE/pgrouting async lane demoted beneath topology (cypher.md:2); owns `GraphFault` 836x, the `H3Cell` node space, the analytical metric-route counterpart to topology's unit-weight `ShortestPathsDijkstra`.
- topology.md incidence is the `Query/lane#ELEMENT_SET_ALGEBRA` `SetResolve.Expand` one-hop expander (lane.md:206-209) — the `Closure` transitive fold's reachability owner.
