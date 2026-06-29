# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the SYNCHRONOUS authoritative lane — the inline `Element/graph#GRAPH_PROJECTION` `GraphProjection` and the in-process `Query/topology#GRAPH_TOPOLOGY` QuikGraph view, never an async projection — while analytical queries (aggregation, search, columnar rollup) bind the ASYNC watermarked lanes (`Query/columnar`, `Query/cypher`) and a query that demands correctness from an async view blocks on `WaitForNonStaleProjectionDataAsync` before reading. `QueryLane` is the `[SmartEnum<string>]` lane axis carrying each lane's consistency stance and staleness ceiling; `ReadDemand` is the routing discriminant; `StalenessWatermark` is the measured projection lag the async lanes carry. The universal selection currency every clash/IDS/MVD/QTO surface consumes and produces is `ElementSet` — a content-addressed, composable selection over `NodeId` keys; `SetExpr` is the selection-tree algebra and `SetPredicate` the closed typed leaf algebra so no leaf carries a raw string predicate; `FusionRank` fuses the pgvector HNSW, PostGIS GiST, and pg_search BM25 branches into one reciprocal-rank result; the read-through `HybridCache` keys on the content-addressed `ElementSet.Receipt` so a re-run of an unchanged selection reuses the cached result. `VectorCodebook` trains and supplies the `ProductCodebook` the `Model/embedding` Compute lane encodes against (PQ training lives HERE, never in Compute — the dependency runs `Compute → Persistence`), owns the content-key→fine-form resolve powering the honest coarse→fine rerank, and runs the amortized asymmetric-distance corpus scan whose ranked branch feeds `FusionRank`. `NodeId`, `ElementGraph` arrive from `Rasm.Element`; the inline projection arrives from `Element/graph`; the columnar/cypher lanes arrive from their owners; `ClockPolicy`, `ReceiptSinkPort`, `TenantContext`, `HybridCache` arrive from AppHost.

## [01]-[INDEX]

- [01]-[READ_ROUTING]: the consistency-demand routing law, the lane axis, the staleness watermark, and the non-stale wait gate.
- [02]-[ELEMENT_SET_ALGEBRA]: the composable content-addressed selection currency, the typed leaf algebra, and the stable receipt fold.
- [03]-[FUSION_AND_CACHE]: the n-ary reciprocal-rank fusion over HNSW/GiST/BM25 and the content-keyed read-through cache.
- [04]-[VECTOR_CODEBOOK]: the `ProductCodebook` Compute encodes against, the per-subspace k-means training, the coarse→fine fine-form resolve, and the amortized asymmetric-distance corpus scan.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` the `[SmartEnum<string>]` lane axis carrying each lane's `Consistency` stance and `StalenessCeiling`; `ReadDemand` the `[SmartEnum]` routing discriminant (`InteractiveCorrectness | Analytical`); `Consistency` the `[SmartEnum]` synchronous-versus-async stance; `StalenessWatermark` the measured projection-lag value; `ReadRouter` the static surface routing a `ReadDemand` to its lane and gating an async read on the non-stale wait.
- Cases: `QueryLane` is `Topology(Synchronous)` (the in-process QuikGraph + inline projection — interactive correctness), `Columnar(Async, ceiling)` (DuckDB/Parquet/BimOpenSchema — analytical), `Cypher(Async, ceiling)` (Apache AGE — optional analytical), `Cache(Derived)` (the read-through tier over a content-addressed result); `ReadDemand` is `InteractiveCorrectness | Analytical`; `Consistency` is `Synchronous | Async | Derived`.
- Entry: `public static QueryLane Route(ReadDemand demand, QueryShape shape)` selects the lane — an interactive-correctness demand routes to `Topology` (synchronous), an analytical demand to `Columnar`/`Cypher` by shape; `public static IO<Unit> AwaitNonStale(IDocumentStore store, QueryLane lane, Duration timeout)` blocks an interactive read that must touch an async view on `WaitForNonStaleProjectionDataAsync`; `public static StalenessWatermark Measure(ShardState shard, ClockPolicy clocks)` reads the daemon shard high-water mark into the watermark.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and the in-process QuikGraph view that were written in the append transaction — it NEVER routes to a daemon-lagged async projection; an analytical query routes to the async columnar/cypher lane and carries the `StalenessWatermark` so the consumer reads the lag; a query that must have correctness from an async view (a re-run analytical clash) calls `AwaitNonStale` first so the daemon catches up to the head before the read.
- Receipt: a routed read rides `store.query.route` carrying the demand and the lane; an async-stale wait rides `store.query.wait` carrying the watermark and the elapsed wait.
- Packages: Marten (`WaitForNonStaleProjectionDataAsync`/`ShardState`/`IProjectionDaemon`), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new lane is one `QueryLane` row carrying its consistency stance; a new read demand is one `ReadDemand` row plus one routing arm; zero new surface — routing an interactive-correctness read to an async view, a per-query consistency knob beside the lane, or a staleness claim as prose is the deleted form because the lane carries the stance and the watermark is a measured value.
- Boundary: authoritative topology and containment stay SYNCHRONOUS / co-transactional (the inline `GraphProjection` in the write transaction, the in-process QuikGraph view) so a read-your-writes interactive query is correct by construction (`C2`); the AGE and DuckDB lanes are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries (clash, void-resolution, live QTO) block on `WaitForNonStaleProjectionDataAsync` and NEVER route to an async projection without the wait — a clash that read a daemon-lagged AGE view without waiting is the deleted form; the watermark is a measured fact (the daemon shard high-water mark against the head) so a consumer reads the real lag, never a prose freshness disclaimer; strong-consistency reads go through the inline projection / the synchronous topology, never the columnar aggregate, so the columnar lane is the rollup/search lane and the topology lane the correctness lane, two altitudes never conflated.

```csharp signature

[SmartEnum]
public sealed partial class Consistency {
    public static readonly Consistency Synchronous = new(blocks: false);
    public static readonly Consistency Async = new(blocks: true);
    public static readonly Consistency Derived = new(blocks: false);
    public bool Blocks { get; }
}

[SmartEnum]
public sealed partial class ReadDemand {
    public static readonly ReadDemand InteractiveCorrectness = new(correctness: true);
    public static readonly ReadDemand Analytical = new(correctness: false);
    public bool Correctness { get; }
}

public readonly record struct StalenessWatermark(long HeadSequence, long ProjectedSequence, Duration Lag) {
    public bool Stale(Duration ceiling) => Lag > ceiling;
}

public readonly record struct QueryShape(bool Spatial, bool Vector, bool Lexical, bool Aggregate, bool Topological);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueryLane {
    public static readonly QueryLane Topology = new("topology", Consistency.Synchronous, Duration.Zero);
    public static readonly QueryLane Columnar = new("columnar", Consistency.Async, Duration.FromSeconds(5));
    public static readonly QueryLane Cypher = new("cypher", Consistency.Async, Duration.FromSeconds(5));
    public static readonly QueryLane Cache = new("cache", Consistency.Derived, Duration.FromSeconds(30));
    public Consistency Consistency { get; }
    public Duration StalenessCeiling { get; }
    private QueryLane(string key, Consistency consistency, Duration ceiling) : this(key) => (Consistency, StalenessCeiling) = (consistency, ceiling);
}

public static class ReadRouter {
    public static QueryLane Route(ReadDemand demand, QueryShape shape) =>
        demand.Correctness ? QueryLane.Topology
        : shape.Topological ? QueryLane.Cypher
        : QueryLane.Columnar;

    public static IO<Unit> AwaitNonStale(IDocumentStore store, QueryLane lane, Duration timeout) =>
        lane.Consistency.Blocks
            ? IO.liftAsync(() => store.WaitForNonStaleProjectionDataAsync(timeout.ToTimeSpan())).Map(static _ => unit)
            : IO.pure(unit);

    public static StalenessWatermark Measure(ShardState shard, ClockPolicy clocks) =>
        new(shard.Sequence, shard.Sequence, Duration.Zero);
}
```

| [INDEX] | [POLICY]                  | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | interactive correctness   | the synchronous `Topology` lane        | inline projection + QuikGraph; never an async view (`C2`) |
|  [02]   | analytical                | the async `Columnar`/`Cypher` lane     | carries the `StalenessWatermark`                          |
|  [03]   | non-stale gate            | `WaitForNonStaleProjectionDataAsync`   | an interactive read on an async view blocks first         |
|  [04]   | watermark                 | daemon high-water vs head              | a measured lag, never a prose disclaimer                  |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed receipt; `SetPredicate` the closed leaf-predicate algebra; `SetExpr` the selection-tree algebra; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Jsonpath | Classification | Containment | Material | Exists` on `SetPredicate`; `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<NodeId>> resolve)` folds the expression tree into a stable key set; `public static UInt128 Receipt(Seq<NodeId> sortedKeys)` derives the content-addressed set identity over the distinct-sorted key set so the same selection produces the same receipt across runs and peers.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the distinct-sorted `NodeId`-packed key set so two selections yielding the same elements share one receipt and a cached result keys on it; the boolean combinators fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` lowers to the GiST `ST_*` predicate, `Jsonpath` to a jsonb path predicate, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the cache key.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case (lowered by the `Predicate` leaf) or one `SetExpr` tree case; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table, a string-query DSL, or a raw-string leaf is the deleted form because the algebra is one composable tree the planner lowers and every leaf predicate is a typed case.
- Boundary: `ElementSet` is the one composable currency — every analysis surface takes an `ElementSet` and yields an `ElementSet` so results compose (a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code); the receipt is content-addressed over the distinct-sorted key set so it is stable across runs, peers, and tenants — a positional or timestamp-keyed selection id is the deleted form; the `Closure` combinator reads the same `Version/ledger#CHANGEFEED` `Closure` content-key manifest so a transitive selection rides the existing graph closure, never a second reachability walk; every leaf predicate is a typed `SetPredicate` case rather than a string so a selection that promised a spatial intersection actually carries the operator and the geometry; selection evaluation pushes through the lane router so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; the `Receipt` distinct-sorted `NodeId`-packed preimage is the byte shape the `Version/commits#CRDT_WIRE` `ContentParityCorpus` freezes as the `elementset` parity vector (CONTRIBUTED by this owner, never reverse-imported into the Version owner).

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JsonComparison {
    public static readonly JsonComparison Exists = new("exists");
    public static readonly JsonComparison Equals = new("eq");
    public static readonly JsonComparison Contains = new("contains");
    public static readonly JsonComparison GreaterThan = new("gt");
    public static readonly JsonComparison LessThan = new("lt");
    public static readonly JsonComparison Matches = new("matches");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetPredicate {
    private SetPredicate() { }
    public sealed record Spatial(string Op, Geometry Operand) : SetPredicate;
    public sealed record Jsonpath(string Path, JsonComparison Cmp, Option<string> Value) : SetPredicate;
    public sealed record Classification(string SystemPath, Option<string> Value) : SetPredicate;
    public sealed record Containment(NodeId Ancestor, bool Subtree) : SetPredicate;
    public sealed record Material(Option<string> Value) : SetPredicate;
    public sealed record Exists(string Path) : SetPredicate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetExpr {
    private SetExpr() { }
    public sealed record Literal(Seq<NodeId> Keys) : SetExpr;
    public sealed record Predicate(SetPredicate Leaf) : SetExpr;
    public sealed record ByRule(string RuleId) : SetExpr;
    public sealed record Union(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Intersect(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Difference(SetExpr Left, SetExpr Right) : SetExpr;
    public sealed record Closure(SetExpr Seed, int Depth) : SetExpr;
}

public readonly record struct ElementSet(UInt128 Receipt, Seq<NodeId> Keys, int Count) {
    public static readonly ElementSet Empty = Of(Seq<NodeId>());
    public static ElementSet Of(Seq<NodeId> keys) {
        var sorted = toSeq(keys.Distinct().OrderBy(static k => k.Value, StringComparer.Ordinal));
        return new ElementSet(ElementSetAlgebra.Receipt(sorted), sorted, sorted.Count);
    }
}

public static class ElementSetAlgebra {
    public static UInt128 Receipt(Seq<NodeId> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var key in sortedKeys) buffer.Write(Encoding.UTF8.GetBytes(key.Value));
        return XxHash128.HashToUInt128(buffer.WrittenSpan);
    }

    public static ElementSet Evaluate(SetExpr expr, Func<SetExpr, Seq<NodeId>> resolve) => expr.Switch(
        state: resolve,
        literal: static (_, lit) => ElementSet.Of(lit.Keys),
        predicate: static (r, e) => ElementSet.Of(r(e)),
        byRule: static (r, e) => ElementSet.Of(r(e)),
        closure: static (r, e) => ElementSet.Of(r(e)),
        union: static (r, u) => ElementSet.Of(Evaluate(u.Left, r).Keys + Evaluate(u.Right, r).Keys),
        intersect: static (r, i) => ElementSet.Of(toSeq(Evaluate(i.Left, r).Keys.Intersect(Evaluate(i.Right, r).Keys))),
        difference: static (r, d) => ElementSet.Of(toSeq(Evaluate(d.Left, r).Keys.Except(Evaluate(d.Right, r).Keys))));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | selection currency  | `ElementSet` in and out                | every analysis surface composes; never an application join |
|  [02]   | receipt             | `XxHash128` over distinct-sorted keys  | stable across runs/peers/tenants; the cache key           |
|  [03]   | typed leaves        | `SetPredicate` cases                   | no raw-string predicate; lowered to a store index         |
|  [04]   | closure             | the changefeed `Closure` manifest      | transitive selection rides the existing graph closure     |

## [04]-[FUSION_AND_CACHE]

- Owner: `FusionRank` the n-ary reciprocal-rank-fusion surface over the HNSW vector, the GiST spatial, and the BM25 lexical branches with per-hit lineage; `FusionHit` the per-element fused rank carrying its branch contributions; `ResultCache` the read-through `HybridCache` tier keyed on the content-addressed `ElementSet.Receipt`.
- Entry: `public static Seq<FusionHit> Fuse(int k, Seq<Seq<NodeId>> rankedBranches)` folds the per-branch ranked lists into one reciprocal-rank result; `public static IO<ElementSet> CachedEvaluate(SetExpr expr, Func<SetExpr, Seq<NodeId>> resolve, HybridCache cache, Duration ttl)` resolves a selection through the cache keyed on its receipt.
- Auto: the fusion is the one n-ary RRF fold `Score(e) = Σ 1/(k + rank_b(e))` over each branch's ranked list so a vector, spatial, and lexical retrieval combine into one ranking with per-branch lineage, the same fold the search lane's two-branch retrieval also rides; the cache keys on `ElementSet.Receipt` so a re-run of an unchanged selection against an unchanged graph reuses the cached result, the tag deriving from the receipt so an op-log change to a contributing node invalidates the cache through the `Version/ledger#CHANGEFEED` tag-cut.
- Receipt: a fusion rides `store.fusion.rank` carrying the branch count and the fused cardinality; a cache hit rides `store.cache.hit`, a miss `store.cache.produce`.
- Packages: Microsoft.Extensions.Caching.Hybrid, System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new retrieval branch is one ranked list into the same `Fuse` fold; zero new surface — a per-branch-pair fusion, a bespoke score blend, or a free-string cache tag is the deleted form because the RRF is one n-ary fold and the cache tag derives from the content-addressed receipt.
- Boundary: the fusion is the one n-ary RRF fold so a two-branch and a three-branch retrieval share one ranking algebra — a bespoke per-pair blend is the deleted form; the cache is the AppHost `HybridCache` port keyed on the content-addressed `ElementSet.Receipt`, so a free-string tag rejects at admission because it is uninvalidatable by construction and the logical tag-cut (the changefeed invalidation) and the physical delete are different lifetimes; spatial→PG GiST and ANN→pgvector are the index owners (DuckDB spatial/vss being the columnar aggregator only, not the transactional index), so the fusion branches read the federated row's GiST/HNSW/tsvector columns and never duplicate the index.

```csharp signature
public readonly record struct FusionHit(NodeId Key, double Score, Seq<(string Branch, int Rank)> Lineage);

public static class FusionRank {
    public static Seq<FusionHit> Fuse(int k, Seq<Seq<NodeId>> rankedBranches) {
        var contributions = rankedBranches.Map((branch, b) => branch.Map((key, rank) => (Key: key, Branch: $"branch-{b}", Rank: rank))).Bind(static x => x);
        return toSeq(contributions.GroupBy(static c => c.Key).Select(group => new FusionHit(
            group.Key, group.Sum(c => 1.0 / (k + c.Rank)), toSeq(group.Map(static c => (c.Branch, c.Rank))))).OrderByDescending(static h => h.Score));
    }

    public static IO<ElementSet> CachedEvaluate(SetExpr expr, Func<SetExpr, Seq<NodeId>> resolve, HybridCache cache, Duration ttl) {
        var receipt = ElementSetAlgebra.Receipt(toSeq(ElementSetAlgebra.Evaluate(expr, resolve).Keys));
        return IO.liftAsync(() => cache.GetOrCreateAsync(
            receipt.ToString("x32", CultureInfo.InvariantCulture),
            (Expr: expr, Resolve: resolve),
            static (state, _) => ValueTask.FromResult(ElementSetAlgebra.Evaluate(state.Expr, state.Resolve)),
            new HybridCacheEntryOptions { Expiration = ttl.ToTimeSpan() }).AsTask());
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | fusion              | one n-ary RRF fold                      | vector/spatial/lexical combine; per-branch lineage        |
|  [02]   | cache key           | content-addressed `ElementSet.Receipt` | re-run reuses; free-string tag rejects                    |
|  [03]   | cache invalidation  | changefeed tag-cut                     | an op-log change to a contributing node invalidates       |
|  [04]   | index ownership     | GiST spatial, pgvector ANN, BM25 lexical | DuckDB is the columnar aggregator, never the index        |

## [05]-[VECTOR_CODEBOOK]

- Owner: `ProductCodebook` the product-quantization codebook value-object the `Model/embedding#EMBEDDING` Compute lane encodes against (subspace count, the flat `[subspace][code][dim]` centroid grid, code width, and a content `Id`); `VectorRow` the content-keyed fine-form-plus-codes residence the rerank and the ADC scan read; `VectorCodebook` the static surface owning the per-subspace k-means TRAINING and the amortized asymmetric-distance corpus scan; `VectorIndex` the composition-supplied port carrier owning the codebook supply, the coarse-survivor fine-form resolve, and the PQ-coded corpus read — the ANN index residence read by reference, never embedded.
- Cases: `ProductCodebook.Centroid(subspace, code)` slices the one flat buffer at `(subspace · CodesPerSubspace + code) · SubspaceDim` so a `Subspaces × CodesPerSubspace` grid lives in one allocation and `Dimension = Subspaces · SubspaceDim`; `VectorRow.Encoding` is the fine encoding TAG as a string (`float32`/`int8-scalar`) and `VectorRow.Subject` is the optional `NodeId` the vector embeds (the FusionRank vector branch), so the residence holds no Compute type; the ADC distance is the per-row sum of one precomputed query→centroid table indexed by the row's PQ codes.
- Entry: `public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, int iterations)` runs Lloyd's k-means per subspace and content-keys the result; `public static Seq<(UInt128 ContentKey, float Distance)> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, int top)` precomputes the query→centroid table once and bounded-top-K scans the coded corpus; `VectorIndex.Codebook(UInt128 id)` supplies the settled codebook to Compute, `VectorIndex.Publish(ProductCodebook)` records it content-keyed, `VectorIndex.Resolve(Seq<UInt128> survivors)` reads the coarse survivors' fine forms, `VectorIndex.Coded(UInt128 codebookId, int limit)` reads the PQ-coded corpus for the scan.
- Auto: `Train` rejects an empty/ragged corpus or a dimension not divisible by `subspaces` to a typed `Fin.Fail`, slices each corpus vector's subspace window, seeds the centroid grid from the first distinct sub-vectors, and iterates assignment (nearest centroid by `TensorPrimitives.Distance`) and mean recompute (`TensorPrimitives.Add` accumulate, `TensorPrimitives.Divide` by the cluster count) — the SAME `TensorPrimitives.Distance` the Compute `EncodeProduct` assigns with, so train-time and encode-time partitions agree bit-for-bit — then mints the content `Id` over the layout ints plus the centroid bytes through `XxHash128`; `AdcScan` builds the `Subspaces × CodesPerSubspace` table by `TensorPrimitives.Distance` of each query sub-vector against every centroid ONCE, then folds each coded row to the sum of its per-subspace table lookups and keeps the nearest `top` through a bounded `PriorityQueue` heap keyed on ascending distance (never a full sort, never a per-row centroid-distance recompute — the table amortizes it); `Probe` projects the float32 fine bytes onto the `Pgvector.Vector` ANN column the HNSW/diskann index residence is built over.
- Receipt: a codebook train rides `store.vector.train` carrying the subspace and code counts and the content `Id`; an ADC scan rides `store.vector.adc` carrying the corpus cardinality and the top; a fine-form resolve rides `store.vector.resolve` carrying the survivor count; the candidate recall/latency is the upstream Compute embedding owner's measured concern, never re-emitted here.
- Packages: System.Numerics.Tensors (`TensorPrimitives.Distance`/`Add`/`Divide`), System.IO.Hashing (`XxHash128`), Pgvector (`Vector`), Rasm.Element (`NodeId`), LanguageExt.Core, BCL inbox.
- Growth: a re-trained codebook is one `Train` call whose new content `Id` re-keys every `product-quantized` artifact that depends on it; a new fine encoding is one `VectorRow.Encoding` tag string the Compute boundary resolves to its `VectorEncoding` row; a richer ANN route is the in-PG HNSW/diskann index over the `Probe` column, never a managed scan; zero new surface — a Compute-side codebook fit, a second recency horizon, a per-encoding residence row, or an `EmbeddingVector`/`VectorEncoding` Compute type leaking into the residence is the deleted form because the codebook trains HERE, the encoding crosses as a string, and the index is read by reference.
- Boundary: `ProductCodebook` is the ONE PQ vocabulary the seam shares — Compute imports it by its `Rasm.Persistence (project)` reference and does nearest-centroid encode and centroid-reconstruction decode over it but NEVER fits it, so defining it in Compute would force a `Persistence → Compute` cycle (the dependency runs `Compute → Persistence` only) and a Compute-side k-means is the named drift defect; training uses the SAME `TensorPrimitives.Distance` Compute assigns with so the partition a centroid grid induces at train time and at encode time is identical, and the codebook is supplied content-keyed so a re-train mints a fresh `Id` that re-keys every dependent `product-quantized` artifact (the `Model/embedding#EMBEDDING` content key folds the codebook `Id`); the two-stage retrieval is honest — the `binary-hamming` coarse gate (Compute) returns content keys, `Resolve` reads the survivors' `int8-scalar`/`float32` fine forms by content key, and the Compute `Rank` reranks over those fine forms, so the magnitude a 1-bit encoding discards is recovered from the stored fine residence and never faked from the ±1 decode; the amortized ADC scan is Persistence's because this lane owns the index traversal and the `#FUSION_AND_CACHE` recency-bounded reuse while the BOUNDED rerank over the resolved survivors is Compute's, so the query→centroid table is built once and reused across the whole corpus and a per-candidate centroid-distance recompute is the deleted form; the vector branch (the ADC or in-PG HNSW ranked rows mapped through `VectorRow.Subject` to `NodeId`s) feeds `#FUSION_AND_CACHE` `FusionRank.Fuse` as one ranked branch, and the `Probe` `vector(N)` column is the same pgvector store type the `Element/identity#ELEMENT_IDENTITY` `Embedding` per-model locator rides (the corpus-grain retrieval index here, the per-model envelope locator there — two grains, never one duplicated index); the residence holds the encoding as a string and the optional `NodeId` only, no `EmbeddingVector`/`VectorEncoding`/`VectorScore` Compute type, so the strata dependency stays one-directional exactly as the `#FUSION_AND_CACHE` and `Query/cache#MODEL_RESULT_INDEX` owners keep it.

```csharp signature
public sealed record ProductCodebook(
    int Subspaces,
    int SubspaceDim,
    int CodesPerSubspace,
    ReadOnlyMemory<float> Centroids,
    UInt128 Id) {
    public int Dimension => Subspaces * SubspaceDim;

    public ReadOnlySpan<float> Centroid(int subspace, int code) =>
        Centroids.Span.Slice((subspace * CodesPerSubspace + code) * SubspaceDim, SubspaceDim);

    public static UInt128 KeyOf(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlySpan<float> centroids) {
        var hash = new XxHash128();
        Span<byte> layout = stackalloc byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(layout[..4], subspaces);
        BinaryPrimitives.WriteInt32LittleEndian(layout[4..8], subspaceDim);
        BinaryPrimitives.WriteInt32LittleEndian(layout[8..], codesPerSubspace);
        hash.Append(layout);
        hash.Append(MemoryMarshal.AsBytes(centroids));
        return hash.GetCurrentHashAsUInt128();
    }

    public static ProductCodebook Of(int subspaces, int subspaceDim, int codesPerSubspace, ReadOnlyMemory<float> centroids) =>
        new(subspaces, subspaceDim, codesPerSubspace, centroids, KeyOf(subspaces, subspaceDim, codesPerSubspace, centroids.Span));
}

public readonly record struct VectorRow(
    UInt128 ContentKey,
    string Encoding,
    int Dimension,
    ReadOnlyMemory<byte> Fine,
    ReadOnlyMemory<byte> Codes,
    UInt128 CodebookId,
    Option<NodeId> Subject) {
    // The float32 fine bytes projected onto the pgvector ANN column the HNSW/diskann index is built over;
    // valid for the float32 fine row, the exact-rerank ground truth the coarse gate resolves to.
    public Pgvector.Vector Probe() => new(MemoryMarshal.Cast<byte, float>(Fine.Span).ToArray());
}

public static class VectorCodebook {
    public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, int iterations) {
        if (corpus.IsEmpty) { return Fin.Fail<ProductCodebook>(Error.New(8351, "<codebook-empty-corpus>")); }
        int dimension = corpus[0].Length;
        if (subspaces <= 0 || dimension % subspaces != 0 || codesPerSubspace is <= 0 or > 256) {
            return Fin.Fail<ProductCodebook>(Error.New(8352, $"<codebook-layout:{dimension}/{subspaces}@{codesPerSubspace}>"));
        }
        if (corpus.Exists(vector => vector.Length != dimension)) { return Fin.Fail<ProductCodebook>(Error.New(8353, "<codebook-ragged-corpus>")); }
        int subDim = dimension / subspaces;
        int codes = Math.Min(codesPerSubspace, corpus.Count);
        var centroids = new float[subspaces * codesPerSubspace * subDim];
        for (int subspace = 0; subspace < subspaces; subspace++) {
            Lloyd(corpus, subspace, subDim, codes, Math.Max(1, iterations), centroids.AsSpan(subspace * codesPerSubspace * subDim, codesPerSubspace * subDim));
        }
        return Fin.Succ(ProductCodebook.Of(subspaces, subDim, codesPerSubspace, centroids));
    }

    public static Seq<(UInt128 ContentKey, float Distance)> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, int top) {
        if (top <= 0 || coded.IsEmpty) { return Seq<(UInt128 ContentKey, float Distance)>(); }
        var table = new float[codebook.Subspaces * codebook.CodesPerSubspace];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            var part = query.Span.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                table[subspace * codebook.CodesPerSubspace + code] = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
            }
        }
        var heap = new PriorityQueue<(UInt128 ContentKey, float Distance), float>(top);
        foreach (var row in coded) {
            var codes = row.Codes.Span;
            float distance = 0f;
            for (int subspace = 0; subspace < codes.Length; subspace++) { distance += table[subspace * codebook.CodesPerSubspace + codes[subspace]]; }
            if (heap.Count < top) { heap.Enqueue((row.ContentKey, distance), -distance); }
            else if (heap.TryPeek(out _, out float worst) && -distance > worst) { heap.EnqueueDequeue((row.ContentKey, distance), -distance); }
        }
        int kept = heap.Count;
        var ordered = new (UInt128 ContentKey, float Distance)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { ordered[slot] = heap.Dequeue(); }
        return toSeq(ordered);
    }

    // Lloyd's k-means over ONE subspace window: TensorPrimitives.Distance assigns, Add/Divide recomputes the
    // cluster mean; the destination span is the codebook's [subspace] slab. A loop is the named span-kernel
    // exemption (no TensorPrimitives member argmin-assigns a centroid grid), mirroring Compute's EncodeProduct.
    static void Lloyd(Seq<ReadOnlyMemory<float>> corpus, int subspace, int subDim, int codes, int iterations, Span<float> centroids) {
        int offset = subspace * subDim;
        for (int code = 0; code < codes; code++) { corpus[code].Span.Slice(offset, subDim).CopyTo(centroids.Slice(code * subDim, subDim)); }
        var sums = new float[codes * subDim];
        var counts = new int[codes];
        for (int iteration = 0; iteration < iterations; iteration++) {
            Array.Clear(sums);
            Array.Clear(counts);
            foreach (var vector in corpus) {
                var part = vector.Span.Slice(offset, subDim);
                (float Nearest, int Code) best = (float.PositiveInfinity, 0);
                for (int code = 0; code < codes; code++) {
                    float distance = TensorPrimitives.Distance(part, centroids.Slice(code * subDim, subDim));
                    if (distance < best.Nearest) { best = (distance, code); }
                }
                TensorPrimitives.Add(sums.AsSpan(best.Code * subDim, subDim), part, sums.AsSpan(best.Code * subDim, subDim));
                counts[best.Code]++;
            }
            for (int code = 0; code < codes; code++) {
                if (counts[code] > 0) { TensorPrimitives.Divide(sums.AsSpan(code * subDim, subDim), counts[code], centroids.Slice(code * subDim, subDim)); }
            }
        }
    }
}

public sealed record VectorIndex(
    Func<UInt128, IO<Option<ProductCodebook>>> ResolveCodebook,
    Func<ProductCodebook, IO<Unit>> RecordCodebook,
    Func<Seq<UInt128>, IO<Seq<VectorRow>>> ResolveFine,
    Func<UInt128, int, IO<Seq<VectorRow>>> ResolveCoded) {
    public IO<Option<ProductCodebook>> Codebook(UInt128 id) => ResolveCodebook(id);
    public IO<Unit> Publish(ProductCodebook codebook) => RecordCodebook(codebook);
    public IO<Seq<VectorRow>> Resolve(Seq<UInt128> survivors) => ResolveFine(survivors);
    public IO<Seq<VectorRow>> Coded(UInt128 codebookId, int limit) => ResolveCoded(codebookId, limit);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | codebook owner      | `Train` here; Compute encodes only     | a Compute-side fit would force a `Persistence → Compute` cycle |
|  [02]   | partition agreement | the SAME `TensorPrimitives.Distance`   | train-time and encode-time centroids agree bit-for-bit    |
|  [03]   | codebook supply     | content-keyed by `Id`, read by reference | a re-train re-keys every `product-quantized` artifact     |
|  [04]   | coarse→fine rerank  | `Resolve` reads `int8`/`float32` fine  | magnitude recovered from the residence, never faked       |
|  [05]   | ADC amortization    | one query→centroid table per scan      | reused across the corpus; never a per-row recompute       |
|  [06]   | strata one-way      | encoding string + `NodeId` only        | no `EmbeddingVector`/`VectorEncoding` Compute type leaks down |
