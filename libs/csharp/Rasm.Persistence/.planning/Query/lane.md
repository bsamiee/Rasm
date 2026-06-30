# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the SYNCHRONOUS authoritative lane — the inline `Element/graph#GRAPH_PROJECTION` `GraphProjection` and the in-process `Query/topology#GRAPH_TOPOLOGY` QuikGraph view, never an async projection — while analytical queries (aggregation, search, columnar rollup) bind the ASYNC watermarked lanes (`Query/columnar`, `Query/cypher`) and a query that demands correctness from an async view blocks on `WaitForNonStaleProjectionDataAsync` before reading. `QueryLane` is the `[SmartEnum<string>]` lane axis carrying each lane's consistency stance and staleness ceiling; `ReadDemand` is the routing discriminant; `StalenessWatermark` is the projection lag the async lanes carry, MEASURED as the event-log head sequence against the daemon shard's projected high-water mark — never a hardcoded zero. The universal selection currency every clash/IDS/MVD/QTO surface consumes and produces is `ElementSet` — a content-addressed, composable selection over `NodeId` keys whose length-framed receipt preimage is collision-free across runs and peers; `SetExpr` is the selection-tree algebra and `SetPredicate` the closed typed leaf algebra so no leaf carries a raw string predicate, and the `Closure` combinator is a genuine bounded transitive fold over the `Query/topology` incidence; `RetrievalBranch` is the typed fusion-branch axis (vector ANN / spatial GiST / lexical BM25) `FusionRank` fuses through one weighted reciprocal-rank fold with per-hit lineage; the read-through `HybridCache` keys on the content-addressed `ElementSet.Receipt` so a re-run of an unchanged selection reuses the cached result, a receipt-derived tag cutting it on a contributing-node change. `VectorCodebook` trains and supplies the `ProductCodebook` the `Model/embedding` Compute lane encodes against (PQ training lives HERE, never in Compute — the dependency runs `Compute → Persistence`), owns the content-key→fine-form resolve powering the honest coarse→fine rerank, and runs the amortized asymmetric-distance corpus scan whose ranked branch feeds `FusionRank`. `NodeId`, `ElementGraph` arrive from `Rasm.Element`; the inline projection arrives from `Element/graph`; the columnar/cypher lanes arrive from their owners; `ReceiptSinkPort`, `TenantContext`, `HybridCache` arrive from AppHost.

## [01]-[INDEX]

- [01]-[READ_ROUTING]: the consistency-demand routing law, the lane axis, the staleness watermark, and the non-stale wait gate.
- [02]-[ELEMENT_SET_ALGEBRA]: the composable content-addressed selection currency, the typed leaf algebra, and the stable receipt fold.
- [03]-[FUSION_AND_CACHE]: the `EmbeddingArity` + `VectorMetric` pgvector SQL-binding axes, the typed `RetrievalBranch` axis, the weighted n-ary reciprocal-rank fusion over the vector/spatial/lexical branches, and the content-keyed read-through cache with receipt-derived invalidation.
- [04]-[VECTOR_CODEBOOK]: the `ProductCodebook` Compute encodes against, the per-subspace k-means training, the coarse→fine fine-form resolve, and the amortized asymmetric-distance corpus scan.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` the `[SmartEnum<string>]` lane axis carrying each lane's `Consistency` stance and `StalenessCeiling`; `ReadDemand` the `[SmartEnum]` routing discriminant (`InteractiveCorrectness | Analytical`); `Consistency` the `[SmartEnum]` synchronous-versus-async stance; `StalenessWatermark` the measured projection-lag value; `ReadRouter` the static surface routing a `ReadDemand` to its lane and gating an async read on the non-stale wait.
- Cases: `QueryLane` is `Topology(Synchronous)` (the in-process QuikGraph + inline projection — interactive correctness), `Columnar(Async, ceiling)` (DuckDB/Parquet/BimOpenSchema — analytical), `Cypher(Async, ceiling)` (Apache AGE — optional analytical), `Cache(Derived)` (the read-through tier over a content-addressed result); `ReadDemand` is `InteractiveCorrectness | Analytical`; `Consistency` is `Synchronous | Async | Derived`.
- Entry: `public static QueryLane Route(ReadDemand demand, QueryShape shape)` selects the lane — an interactive-correctness demand routes to `Topology` (synchronous), an analytical demand to `Cypher` (topological shape), `Cache` (a retrieval shape: vector/spatial/lexical, the fusion+reuse lane), `Columnar` (an explicit `Aggregate` shape), or back to the synchronous `Topology` (a no-evidence shape with every bit false, which has no async-lane warrant), every `QueryShape` field — `Aggregate` included — READ so no bool is a dead knob; `public static IO<Unit> AwaitNonStale(IDocumentStore store, QueryLane lane, Duration timeout)` blocks an interactive read that must touch an async view on `WaitForNonStaleProjectionDataAsync`; `public static StalenessWatermark Measure(EventStoreStatistics head, ShardState projection)` folds the event-log head sequence against the daemon shard's projected high-water `Sequence` into a measured sequence-gap watermark, and `public static IO<StalenessWatermark> Measure(IDocumentStore store, ShardName shard)` reads both off the live store (`FetchEventStoreStatistics` head, `AllProjectionProgress` shard, matched by the shard's string `ShardName`) so the watermark a consumer reads is a real daemon-lag fact built from the verified `EventSequenceNumber`/`ShardState.Sequence` members alone.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and the in-process QuikGraph view that were written in the append transaction — it NEVER routes to a daemon-lagged async projection; an analytical query routes to the async columnar/cypher lane and carries the `StalenessWatermark` so the consumer reads the lag; a query that must have correctness from an async view (a re-run analytical clash) calls `AwaitNonStale` first so the daemon catches up to the head before the read.
- Receipt: a routed read rides `store.query.route` carrying the demand and the lane; an async-stale wait rides `store.query.wait` carrying the watermark and the elapsed wait.
- Packages: Marten (`WaitForNonStaleProjectionDataAsync` — the async-lane staleness block whose production instance member is `IEventDatabase.WaitForNonStaleProjectionDataAsync(TimeSpan)` (Marten's own LINQ query-waiter path drives it through `IMartenDatabase`); the `IDocumentStore`/`IHost` overloads are `Marten.Events.TestingExtensions` convenience wrappers FORWARDING to that database member, so the dependency is on the production interface, not a test-only symbol — the `store.WaitForNonStaleProjectionDataAsync(timeout)` form below resolves through that extension to the database instance member; `ShardState` (the `ShardName` string + high-water `Sequence`)/`ShardName` (`Identity`)/`EventStoreStatistics` (`EventSequenceNumber`)/`AdvancedOperations.FetchEventStoreStatistics`/`AllProjectionProgress`/`IProjectionDaemon`), NodaTime (`Duration`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new lane is one `QueryLane` row carrying its consistency stance; a new read demand is one `ReadDemand` row plus one routing arm; zero new surface — routing an interactive-correctness read to an async view, a per-query consistency knob beside the lane, or a staleness claim as prose is the deleted form because the lane carries the stance and the watermark is a measured value.
- Boundary: authoritative topology and containment stay SYNCHRONOUS / co-transactional (the inline `GraphProjection` in the write transaction, the in-process QuikGraph view) so a read-your-writes interactive query is correct by construction (`C2`); the synchronous `Topology` lane is NOT infallible — `Route` selects the lane, but the `Query/topology#GRAPH_TOPOLOGY` `Traversals.Run` it binds returns `Fin<TopologyResult>` railing the typed `TopologyFault` band (`RootAbsent` 8370 for a query rooted at an absent node, `Cyclic` 8371 for an `Order` over a cyclic graph), so a router consumer composes the topology `Fin` into its OWN rail rather than assuming the topology lane always succeeds — an absent-root containment query is an honest typed fault, never a silent empty result; the AGE and DuckDB lanes are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries (clash, void-resolution, live QTO) block on `WaitForNonStaleProjectionDataAsync` and NEVER route to an async projection without the wait — a clash that read a daemon-lagged AGE view without waiting is the deleted form; the watermark is a MEASURED fact (`EventStoreStatistics.EventSequenceNumber` head against the `ShardState.Sequence` projected mark — the SEQUENCE GAP, since `ShardState.Timestamp` is a daemon-side RECORDING stamp (`DateTimeOffset.UtcNow` set at row construction), not a source-event clock, so it measures read-latency rather than producer→projection lag) so a consumer reads the real daemon lag, NEVER a hardcoded `Duration.Zero` on a trailing shard or a prose freshness disclaimer — a `Measure` that always returns zero lag is the illusory form this owner forbids, and a wall-clock lag mis-read off the recording `Timestamp` is the confound this owner rejects; strong-consistency reads go through the inline projection / the synchronous topology, never the columnar aggregate, so the columnar lane is the rollup/search lane and the topology lane the correctness lane, two altitudes never conflated.

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
    // Staleness is the SEQUENCE GAP first (the verified `EventSequenceNumber` vs `ShardState.Sequence` — the events the
    // async shard trails the log head), the `Lag` Duration a secondary wall-clock signal: a caught-up shard
    // (`Projected >= Head`) is never stale; a trailing shard is stale the moment its gap is non-zero OR its measured lag
    // exceeds the lane ceiling — so an interactive-correctness consumer blocks on ANY sequence gap, never trusting a
    // zero-lag reading on a behind shard.
    public long Gap => HeadSequence - ProjectedSequence;
    public bool Stale(Duration ceiling) => Gap > 0 || Lag > ceiling;
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
    // Every `QueryShape` field is load-bearing — no bool is a dead knob: an interactive-correctness demand binds
    // the synchronous `Topology` lane regardless of shape; an analytical demand routes by the shape's own evidence —
    // a topological shape to the `Cypher` graph lane, a retrieval shape (any of vector ANN / spatial GiST / lexical
    // BM25, the `#FUSION_AND_CACHE` `RetrievalBranch` set) to the content-addressed `Cache` lane that owns the
    // RRF fusion + read-through reuse, an `Aggregate` shape to the `Columnar` rollup lane, and a NO-evidence shape
    // (every bit false) back to the synchronous `Topology` lane because a shapeless analytical read has no
    // async-lane warrant and the correct default is the read-your-writes topology, not a silent rollup. `Aggregate`
    // is thus the explicit `Columnar` discriminant — a `Columnar` fall-through that fires regardless of `Aggregate`
    // would make it the dead knob the KNOB_TEST deletes, so it is READ, never inferred from the absence of the rest.
    public static QueryLane Route(ReadDemand demand, QueryShape shape) =>
        demand.Correctness ? QueryLane.Topology
        : shape.Topological ? QueryLane.Cypher
        : shape.Vector || shape.Spatial || shape.Lexical ? QueryLane.Cache
        : shape.Aggregate ? QueryLane.Columnar
        : QueryLane.Topology;

    public static IO<Unit> AwaitNonStale(IDocumentStore store, QueryLane lane, Duration timeout) =>
        lane.Consistency.Blocks
            ? IO.liftAsync(() => store.WaitForNonStaleProjectionDataAsync(timeout.ToTimeSpan())).Map(static _ => unit)
            : IO.pure(unit);

    // The watermark is a MEASURED fact, never a hardcoded zero — and it is built from the VERIFIED Marten members only:
    // the head is the event-log global sequence (`EventStoreStatistics.EventSequenceNumber` via
    // `store.Advanced.FetchEventStoreStatistics`) and the projected mark is the async shard's high-water
    // `ShardState.Sequence` (`store.Advanced.AllProjectionProgress`). `ShardState.Timestamp` IS a real member, but it is a
    // DAEMON-SIDE RECORDING stamp (`DateTimeOffset.UtcNow` set by the `ShardState(shardName, sequence)` constructor when the
    // progress row is read), NOT a source-event clock — the gap between when an event was APPENDED and when the projection
    // PROCESSED it is invisible to it, so subtracting it would report read-latency, never producer→projection lag. The
    // staleness signal is therefore the SEQUENCE GAP (`Head - Projected`), the events the projection has not yet folded:
    // a caught-up shard (`Projected >= Head`) carries `Duration.Zero` and a zero gap; a trailing shard carries a positive
    // gap and a conservatively-infinite `Lag` (`Duration.MaxValue`) so `Stale` is true on the gap alone — a wall-clock lag
    // mis-read off `Timestamp` would be the recording-clock confound, and a fabricated zero lag on a behind shard the
    // illusory form this owner forbids.
    public static StalenessWatermark Measure(EventStoreStatistics head, ShardState projection) =>
        new(head.EventSequenceNumber, projection.Sequence,
            projection.Sequence >= head.EventSequenceNumber ? Duration.Zero : Duration.MaxValue);

    public static IO<StalenessWatermark> Measure(IDocumentStore store, ShardName shard) =>
        from stats in IO.liftAsync(() => store.Advanced.FetchEventStoreStatistics())
        from progress in IO.liftAsync(() => store.Advanced.AllProjectionProgress())
        // `ShardState.ShardName` is the shard's string identity (`"Projection:All"`), so the progress row matches the
        // queried `ShardName.Identity` by string. A shard with NO progress row has never advanced — its high-water sequence
        // is UNKNOWN, not zero, so synthesizing a `ShardState(shard, 0L)` (the public `(string|ShardName, long)` ctor would
        // accept it) to fold through `Measure` would FABRICATE a measured daemon position the daemon never published and is
        // the deleted form; instead an absent shard rails the MAXIMALLY-stale watermark directly (projected sequence 0, an
        // infinite lag whenever the head carries any event) so an interactive-correctness consumer ALWAYS blocks on a
        // never-projected async view rather than reading a phantom zero-lag.
        select toSeq(progress).Find(s => s.ShardName == shard.Identity).Match(
            Some: state => Measure(stats, state),
            None: () => new StalenessWatermark(stats.EventSequenceNumber, 0L, stats.EventSequenceNumber > 0 ? Duration.MaxValue : Duration.Zero));
}
```

| [INDEX] | [POLICY]                  | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | interactive correctness   | the synchronous `Topology` lane        | inline projection + QuikGraph; never an async view (`C2`) |
|  [02]   | analytical                | the async `Columnar`/`Cypher` lane     | carries the `StalenessWatermark`                          |
|  [03]   | shape routing             | every `QueryShape` bit READ; `Aggregate` → `Columnar` | no-evidence shape → synchronous `Topology`; no dead knob |
|  [04]   | non-stale gate            | `WaitForNonStaleProjectionDataAsync`   | an interactive read on an async view blocks first         |
|  [05]   | watermark                 | `EventSequenceNumber` vs shard `Sequence` | the sequence gap (verified members), never the recording-clock `Timestamp` confound |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed receipt; `SetPredicate` the closed leaf-predicate algebra; `SetExpr` the selection-tree algebra; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Jsonpath | Classification | Containment | Material | Exists` on `SetPredicate` (the bounded operator within each typed — `SpatialOp` on `Spatial`, `JsonComparison` on `Jsonpath`); `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static ElementSet Evaluate(SetExpr expr, SetResolve resolve)` folds the expression tree into a stable key set through the two store-lowering ports the `SetResolve` carries (`Leaf` lowers a `Predicate`/`ByRule` to an index scan, `Expand` is the one-hop topology neighbour the `Closure` fold iterates); `public static UInt128 Receipt(Seq<NodeId> sortedKeys)` derives the content-addressed set identity over the length-framed distinct-sorted preimage so the same selection produces the same receipt across runs and peers; `public static ReadOnlyMemory<byte> Canonical(Seq<NodeId> sortedKeys)` is the length-framed preimage the receipt hashes and the parity corpus freezes.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the LENGTH-FRAMED distinct-sorted `NodeId` preimage (a LE `int32` key count, then per key a LE `int32` byte length and its UTF8 bytes) so two selections yielding the same elements share one receipt AND two different key sets can never collide on an unframed concatenation; the boolean combinators fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` lowers to the GiST predicate the TYPED `SpatialOp` `.Key` (`ST_Intersects`/`ST_Within`/`ST_DWithin`/…) names so a typo is a missing vocabulary row at compile time rather than a silent sequential scan, `Jsonpath` to a jsonb path predicate under the typed `JsonComparison` comparator, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms; the `Closure` arm is a GENUINE bounded transitive fold — it evaluates its `Seed` sub-expression then folds `Depth` one-hop `Expand` waves accumulating the reachable frontier to its fixpoint, never an opaque leaf identical to `Predicate`.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the cache key.
- Packages: System.IO.Hashing (`XxHash128`), System.Buffers (`ArrayBufferWriter`/`BinaryPrimitives`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
- Growth: a new selection primitive is one `SetPredicate` case (lowered by the `Predicate` leaf) or one `SetExpr` tree case; a new spatial operator is one `SpatialOp` row, a new jsonb comparator one `JsonComparison` row; a new combinator is one fold arm; zero new surface — a per-discipline selection class, a saved-search table, a string-query DSL, a raw-string leaf, or a free-string operator on a typed leaf is the deleted form because the algebra is one composable tree the planner lowers, every leaf predicate is a typed case, and every bounded operator within a leaf is a vocabulary row.
- Boundary: `ElementSet` is the one composable currency — every analysis surface takes an `ElementSet` and yields an `ElementSet` so results compose (a clash result intersected with a classification selection is one `SetExpr.Intersect`, never a join in application code); the receipt is content-addressed over the length-framed distinct-sorted preimage so it is stable across runs, peers, and tenants AND unambiguous — a positional or timestamp-keyed selection id, or an unframed byte concatenation two key sets collide on, is the deleted form; the `Closure` combinator is a real bounded transitive fold whose one-hop `Expand` is the `Query/topology#GRAPH_TOPOLOGY` incidence neighbour over the seam graph (the reachability owner stays the graph/topology owner, the bounded fold stays here), NEVER the `Version/ledger#CHANGEFEED` `Closure` — that ledger manifest is a representation-content-hash blob-transfer set keyed by `UInt128`, a DIFFERENT closure that cannot answer a `NodeId` reachability selection, so conflating the two is the deleted altitude error; every leaf predicate is a typed `SetPredicate` case and every bounded operator within it is a vocabulary row — the spatial operator is a `SpatialOp` smart-enum, the jsonb comparator a `JsonComparison` smart-enum — so a selection that promised a spatial intersection carries the typed `ST_*` operator the GiST index serves and the geometry, never a free string a typo degrades to a scan; selection evaluation pushes through the lane router so a `Spatial` leaf executes on the GiST index and a `Jsonpath` leaf on the jsonb index in the store, never client-side; the `ElementSet.Preimage` length-framed byte shape is what the `Version/commits#CRDT_WIRE` `ContentParityCorpus.Contribute(ParitySlot.ElementSet, set.Preimage)` freezes as the `elementset` parity vector (CONTRIBUTED by this owner, never reverse-imported into the Version owner).

```csharp signature
// The jsonb-predicate vocabulary (`@>`/`?`/`->>` comparisons the GIN `jsonb_ops` index serves) — one closed
// row set, never a free comparison string. `Jsonpath.Path` stays a string because a jsonpath expression is
// unbounded data, but the COMPARATOR is a bounded vocabulary, so it is typed exactly as the spatial OPERATOR below.
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

// The PostGIS spatial-predicate vocabulary — each row carries the `ST_*` function name the GiST index serves as
// its `.Key`, so a `Spatial` leaf binds a TYPED operator the index actually supports, never a free string a typo
// silently degrades to a sequential scan. The closed counterpart of `JsonComparison`: the page's "every leaf
// carries the operator" law holds for spatial because the operator IS a vocabulary item, not a string field.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialOp {
    public static readonly SpatialOp Intersects = new("ST_Intersects");
    public static readonly SpatialOp Contains = new("ST_Contains");
    public static readonly SpatialOp Within = new("ST_Within");
    public static readonly SpatialOp DWithin = new("ST_DWithin");
    public static readonly SpatialOp Overlaps = new("ST_Overlaps");
    public static readonly SpatialOp Touches = new("ST_Touches");
    public static readonly SpatialOp Covers = new("ST_Covers");
    public static readonly SpatialOp CoveredBy = new("ST_CoveredBy");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.Default)]
public abstract partial record SetPredicate {
    private SetPredicate() { }
    public sealed record Spatial(SpatialOp Op, Geometry Operand) : SetPredicate;
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

public readonly record struct ElementSet(UInt128 Receipt, Seq<NodeId> Keys, int Count, ReadOnlyMemory<byte> Preimage) {
    public static readonly ElementSet Empty = Of(Seq<NodeId>());
    // `Preimage` is the length-framed canonical byte shape `Receipt` hashes — exposed so the
    // `Version/commits#CRDT_WIRE` `ContentParityCorpus.Contribute(ParitySlot.ElementSet, set.Preimage)`
    // freezes the EXACT bytes the receipt derives from, never a re-spelled second projection.
    public static ElementSet Of(Seq<NodeId> keys) {
        var sorted = toSeq(keys.Distinct().OrderBy(static k => k.Value, StringComparer.Ordinal));
        var preimage = ElementSetAlgebra.Canonical(sorted);
        return new ElementSet(XxHash128.HashToUInt128(preimage.Span), sorted, sorted.Count, preimage);
    }
}

// `SetResolve` carries the TWO store-lowering ports the planner supplies: `Leaf` lowers a `Predicate`/`ByRule`
// leaf to its key set on the owning index, `Expand` is the ONE-HOP neighbour function (the `Query/topology`
// incidence over containment+connection edges) the `Closure` fold iterates. Threading the expander as a port
// keeps the reachability owner in `Query/topology`/`Element/graph` (the seam graph) and the algebra here.
public readonly record struct SetResolve(Func<SetExpr, Seq<NodeId>> Leaf, Func<Seq<NodeId>, Seq<NodeId>> Expand);

public static class ElementSetAlgebra {
    public static UInt128 Receipt(Seq<NodeId> sortedKeys) => XxHash128.HashToUInt128(Canonical(sortedKeys).Span);

    // The cross-runtime parity preimage (`Version/commits#CRDT_WIRE` `ContentParityCorpus` freezes this exact
    // byte shape as the `elementset` `ParityVector`): each key is LENGTH-FRAMED — a LE `int32` byte count then
    // the UTF8 bytes — so `["ab","c"]` and `["a","bc"]` can NEVER collide on an unframed concatenation; the
    // distinct-sorted order is the caller's contract, the frame is the preimage the Python/TS replicas reproduce.
    public static ReadOnlyMemory<byte> Canonical(Seq<NodeId> sortedKeys) {
        var buffer = new ArrayBufferWriter<byte>();
        BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), sortedKeys.Count);
        buffer.Advance(4);
        foreach (var key in sortedKeys) {
            int bytes = Encoding.UTF8.GetByteCount(key.Value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.GetSpan(4), bytes);
            buffer.Advance(4);
            Encoding.UTF8.GetBytes(key.Value, buffer.GetSpan(bytes));
            buffer.Advance(bytes);
        }
        return buffer.WrittenMemory;
    }

    public static ElementSet Evaluate(SetExpr expr, SetResolve resolve) => expr.Switch(
        state: resolve,
        literal: static (_, lit) => ElementSet.Of(lit.Keys),
        predicate: static (r, e) => ElementSet.Of(r.Leaf(e)),
        byRule: static (r, e) => ElementSet.Of(r.Leaf(e)),
        union: static (r, u) => ElementSet.Of(Evaluate(u.Left, r).Keys + Evaluate(u.Right, r).Keys),
        intersect: static (r, i) => ElementSet.Of(toSeq(Evaluate(i.Left, r).Keys.Intersect(Evaluate(i.Right, r).Keys))),
        difference: static (r, d) => ElementSet.Of(toSeq(Evaluate(d.Left, r).Keys.Except(Evaluate(d.Right, r).Keys))),
        // `Closure` is a GENUINE bounded transitive fold, never an opaque leaf: it evaluates its `Seed`
        // sub-expression ONCE (the seed leaf is a store scan — re-evaluating it for the Frontier seed would
        // double the scan, the deleted form), then folds `Depth` one-hop `Expand` waves accumulating the
        // reachable frontier — a `Closure(Seed, 0)` is the seed itself, each wave adds the next ring, and the
        // distinct fold is the fixpoint. The expander is the `Query/topology` incidence so the reachability
        // walk lives with the graph owner, never re-implemented as a second walk here.
        closure: static (r, c) => Closed(Evaluate(c.Seed, r).Keys, c.Depth, r.Expand));

    // The bounded transitive-closure fold the `Closure` arm composes: the seed key set (evaluated once by the
    // caller), `Depth` one-hop `Expand` waves, the distinct frontier accumulated to its fixpoint. A wave whose
    // ring is empty short-circuits to the accumulator so an early fixpoint costs no further `Expand` call.
    static ElementSet Closed(Seq<NodeId> seed, int depth, Func<Seq<NodeId>, Seq<NodeId>> expand) =>
        ElementSet.Of(Range(0, Math.Max(0, depth)).Fold(
            (Reached: seed, Frontier: seed),
            (acc, _) => toSeq(expand(acc.Frontier).Except(acc.Reached)) is var ring && ring.IsEmpty
                ? acc
                : (acc.Reached + ring, ring)).Reached);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | selection currency  | `ElementSet` in and out                | every analysis surface composes; never an application join |
|  [02]   | receipt             | `XxHash128` over length-framed preimage | stable + collision-free; the cache key + parity preimage  |
|  [03]   | typed leaves        | `SetPredicate` cases + `SpatialOp`/`JsonComparison` operators | no raw-string predicate or operator; lowered to a store index |
|  [04]   | closure             | bounded transitive fold over topology  | one-hop `Expand` is `Query/topology`; never the ledger manifest |

## [04]-[FUSION_AND_CACHE]

- Owner: `EmbeddingArity` the `[SmartEnum<string>]` CLR-to-store vector arity axis (`vector`/`halfvec`/`sparsevec`/`bit`); `VectorMetric` the `[SmartEnum<string>]` closed six-metric pgvector distance axis carrying each metric's raw-CTE `Op` literal and EF-translator `Fn` member; `RetrievalBranch` the `[SmartEnum<string>]` typed branch axis (vector ANN / spatial GiST / lexical BM25) carrying each branch's in-PG `Index` name and RRF `Weight`; `FusionRank` the weighted n-ary reciprocal-rank-fusion surface over those branches with per-hit lineage; `FusionHit` the per-element fused rank carrying its typed branch contributions; `ResultCache` the read-through `HybridCache` tier keyed on the content-addressed `ElementSet.Receipt`.
- Entry: `public static Seq<FusionHit> Fuse(int k, Seq<(RetrievalBranch Branch, Seq<NodeId> Ranked)> branches)` folds the per-branch ranked lists into one weighted reciprocal-rank result whose lineage names the typed branch; `public static IO<ElementSet> CachedEvaluate(SetExpr expr, SetResolve resolve, HybridCache cache, Duration ttl)` resolves a selection ONCE through the cache keyed on its receipt with a receipt-derived invalidation tag.
- Auto: the fusion is the one weighted n-ary RRF fold `Score(e) = Σ_b branch.Weight / (k + rank_b(e))` over each TYPED branch's ranked list so a vector, spatial, and lexical retrieval combine into one ranking with per-branch lineage that names which index ranked the hit — the `Vector` branch orders by a `VectorMetric` (`Op` `<->`/`<=>`/`<#>`/`<+>`/`<~>`/`<%>` for the raw-CTE leg, `Fn` `nameof(VectorDbFunctionsExtensions.L2Distance)` … for the EF-translated `ORDER BY` leg) over an `EmbeddingArity` column (`vector(N)`/`halfvec(N)`/`sparsevec(N)`/`bit(N)`, the `Bit` arity the `binary_quantize(emb)::bit(N)` coarse-Hamming gate), the `Spatial` branch by a PostGIS GiST `ST_*` predicate, the `Lexical` branch by `pg_search` `pdb.score(<key_field>)` BM25 degrading to native `ts_rank` when the extension is absent — the same fold a two-branch and a three-branch retrieval both ride; the selection is evaluated ONCE (its `ElementSet` carries its own receipt), the cache keys on `ElementSet.Receipt` so a re-run of an unchanged selection against an unchanged graph reuses the result, and the receipt-derived `tags` feed `RemoveByTagAsync` so an op-log change to a contributing node invalidates the cache through the `Version/ledger#CHANGEFEED` tag-cut.
- Receipt: a fusion rides `store.fusion.rank` carrying the branch count and the fused cardinality; a cache hit rides `store.cache.hit`, a miss `store.cache.produce`.
- Packages: Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync`/`HybridCacheEntryOptions`/`RemoveByTagAsync`), Pgvector.EntityFrameworkCore (`VectorDbFunctionsExtensions` distance members + `Pgvector.Vector` probe), System.IO.Hashing (`XxHash128`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new retrieval branch is one `RetrievalBranch` row carrying its index name and weight plus one ranked list into the same `Fuse` fold; a new vector arity is one `EmbeddingArity` row, a new distance one `VectorMetric` row; zero new surface — a per-branch-pair fusion, a bespoke score blend, a positional `branch-{b}` string lineage, a parallel column-type enum duplicating `EmbeddingArity`, a free-string distance operator, or a free-string cache tag is the deleted form because the RRF is one weighted n-ary fold over the typed branch axis, the arity and metric are closed axes, and the cache tag derives from the content-addressed receipt.
- Boundary: this `FUSION_AND_CACHE` owner is the search-lane binding the pgvector/pg_search/pgvectorscale `.api` catalogs compose against — `EmbeddingArity` (the CLR-to-store arity), `VectorMetric` (the six distance metrics), and `FusionRank.Fuse` (the RRF stack) are the typed C# projections of those catalogues' server surface, so a catalogue's `VectorMetric`/`EmbeddingArity`/RRF reference resolves here, never a parallel saved-search owner; the fusion is the one weighted n-ary RRF fold over the typed `RetrievalBranch` axis so a two-branch and a three-branch retrieval share one ranking algebra and a hit's lineage names the index that ranked it — a bespoke per-pair blend or a positional string branch label is the deleted form; the cache is the AppHost `HybridCache` port keyed on the content-addressed `ElementSet.Receipt` with a receipt-derived tag, so a free-string tag rejects at admission because it is uninvalidatable by construction and the logical tag-cut (the changefeed `RemoveByTagAsync` invalidation) and the physical delete are different lifetimes; spatial→PG GiST and ANN→pgvector are the index owners (DuckDB spatial/vss being the columnar aggregator only, not the transactional index), so the fusion branches read the federated row's GiST/HNSW/tsvector columns and never duplicate the index, the vector branch resolving through the `#VECTOR_CODEBOOK` `VectorRow.Subject`-mapped ranked rows.

```csharp signature
// The CLR-to-store arity axis the `pgvector` plugin maps (`api-pgvector-ef`): one row per store type carrying
// its `vector`/`halfvec`/`sparsevec`/`bit` base name. `Bit` is the `binary_quantize(emb)::bit(N)` expression
// column the coarse Hamming gate reads; the other three map `Pgvector.Vector`/`HalfVector`/`SparseVector`. A
// new arity is one row, never a parallel column-type enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EmbeddingArity {
    public static readonly EmbeddingArity Dense = new("dense", "vector");
    public static readonly EmbeddingArity Half = new("half", "halfvec");
    public static readonly EmbeddingArity Sparse = new("sparse", "sparsevec");
    public static readonly EmbeddingArity Bit = new("bit", "bit");
    public string StoreType { get; }
    public string Column(int n) => $"{StoreType}({n})";
    private EmbeddingArity(string key, string storeType) : this(key) => StoreType = storeType;
}

// The six pgvector distance metrics as one closed axis (`api-pgvector-ef` `VectorDbFunctionsExtensions`): the
// `Op` column carries the raw-CTE operator literal (`<->`/`<#>`/`<=>`/`<+>`/`<~>`/`<%>`), the `Fn` column the
// EF-translator member name (`nameof(VectorDbFunctionsExtensions.L2Distance)` …) the typed `ORDER BY` leg binds
// by reference. A `Bit` arity routes through `Hamming`/`Jaccard`, a dense arity through the four float metrics —
// the planner picks HNSW/ivfflat/diskann over the always-present exact scan, the metric never a free string.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorMetric {
    public static readonly VectorMetric L2 = new("l2", "<->", nameof(VectorDbFunctionsExtensions.L2Distance));
    public static readonly VectorMetric InnerProduct = new("ip", "<#>", nameof(VectorDbFunctionsExtensions.MaxInnerProduct));
    public static readonly VectorMetric Cosine = new("cosine", "<=>", nameof(VectorDbFunctionsExtensions.CosineDistance));
    public static readonly VectorMetric L1 = new("l1", "<+>", nameof(VectorDbFunctionsExtensions.L1Distance));
    public static readonly VectorMetric Hamming = new("hamming", "<~>", nameof(VectorDbFunctionsExtensions.HammingDistance));
    public static readonly VectorMetric Jaccard = new("jaccard", "<%>", nameof(VectorDbFunctionsExtensions.JaccardDistance));
    public string Op { get; }
    public string Fn { get; }
    private VectorMetric(string key, string op, string fn) : this(key) => (Op, Fn) = (op, fn);
}

// The TYPED retrieval-branch axis — the fusion's branch identity is a vocabulary value, never a positional
// `branch-{b}` string. Each row names the in-PG index it reads (`Index`) and its RRF `Weight`: the `Vector`
// branch orders by a `VectorMetric` `Op`/`Fn` over an `EmbeddingArity` column, the `Spatial` branch by a PostGIS
// GiST `ST_*` predicate, the `Lexical` branch by `pg_search` `pdb.score(<key_field>)` BM25 (degrading to native
// `ts_rank` when the extension is absent). A new branch is one row carrying its index name and weight.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetrievalBranch {
    public static readonly RetrievalBranch Vector = new("vector", "pgvector-hnsw", 1.0);
    public static readonly RetrievalBranch Spatial = new("spatial", "postgis-gist", 1.0);
    public static readonly RetrievalBranch Lexical = new("lexical", "pg_search-bm25", 1.0);
    public string Index { get; }
    public double Weight { get; }
    private RetrievalBranch(string key, string index, double weight) : this(key) => (Index, Weight) = (index, weight);
}

public readonly record struct FusionHit(NodeId Key, double Score, Seq<(RetrievalBranch Branch, int Rank)> Lineage);

public static class FusionRank {
    // Weighted n-ary reciprocal-rank fusion `Score(e) = Σ_b branch.Weight / (k + rank_b(e))` over the TYPED
    // branches: each branch carries its `RetrievalBranch` identity so a hit's lineage names which index ranked
    // it (vector ANN, spatial GiST, lexical BM25), never an anonymous positional index; the same fold serves a
    // two-branch and a three-branch retrieval, the weight a per-branch policy value rather than a blend knob.
    // The per-branch rank is the 1-BASED list position (the canonical RRF rank, so the top hit scores `Weight/(k+1)`
    // and `k+rank` never divides by `k` alone) — projected with the indexed `Select` (`Seq<NodeId>.Map` carries no
    // (value, index) overload, so the index rides the LINQ indexed projection the surrounding fold already composes).
    public static Seq<FusionHit> Fuse(int k, Seq<(RetrievalBranch Branch, Seq<NodeId> Ranked)> branches) =>
        toSeq(branches
            .Bind(static b => toSeq(b.Ranked.AsEnumerable().Select((key, index) => (Key: key, b.Branch, Rank: index + 1))))
            .GroupBy(static c => c.Key)
            .Select(group => new FusionHit(
                group.Key,
                group.Sum(c => c.Branch.Weight / (k + c.Rank)),
                toSeq(group.Select(static c => (c.Branch, c.Rank)))))
            .OrderByDescending(static h => h.Score));

    // The cache is read-through on the content-addressed receipt — the selection is evaluated EXACTLY ONCE (its
    // `ElementSet` carries its own `Receipt`, the receipt hex is the `HybridCache` key), and the already-evaluated
    // `set` is the factory state itself, so a miss STORES the value the key was derived from rather than re-running
    // `Evaluate` a second time (a factory that re-evaluated `(Expr, Resolve)` would double the resolve on every
    // miss, the defect this captures away); the receipt-derived tag feeds `RemoveByTagAsync` when an op-log change
    // to a contributing node invalidates the logical cut. Computing the key needs the receipt, which needs the set —
    // so the one unavoidable evaluation produces both the key and the cached value, the stampede gate folding any
    // concurrent miss onto this one already-computed result.
    public static IO<ElementSet> CachedEvaluate(SetExpr expr, SetResolve resolve, HybridCache cache, Duration ttl) {
        var set = ElementSetAlgebra.Evaluate(expr, resolve);
        var key = set.Receipt.ToString("x32", CultureInfo.InvariantCulture);
        return IO.liftAsync(() => cache.GetOrCreateAsync(
            key,
            set,
            static (computed, _) => ValueTask.FromResult(computed),
            new HybridCacheEntryOptions { Expiration = ttl.ToTimeSpan() },
            tags: [$"elementset:{key}"]).AsTask());
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | fusion              | weighted n-ary RRF over `RetrievalBranch` | typed branch lineage; never a positional `branch-{b}`   |
|  [02]   | vector arity        | `EmbeddingArity` row → `<store>(N)`     | `vector`/`halfvec`/`sparsevec`/`bit`; no parallel column enum |
|  [03]   | distance metric     | `VectorMetric` `Op` + `Fn`             | the six pgvector ops; never a free-string operator        |
|  [04]   | cache key           | content-addressed `ElementSet.Receipt` | evaluated once; re-run reuses; free-string tag rejects    |
|  [05]   | cache invalidation  | receipt-derived tag → `RemoveByTagAsync` | a changefeed op-log change to a contributing node cuts it |
|  [06]   | index ownership     | GiST spatial, pgvector ANN, BM25 lexical | DuckDB is the columnar aggregator, never the index        |

## [05]-[VECTOR_CODEBOOK]

- Owner: `ProductCodebook` the product-quantization codebook value-object the `Model/embedding#EMBEDDING` Compute lane encodes against (subspace count, the flat `[subspace][code][dim]` centroid grid, code width, and a content `Id`); `VectorRow` the content-keyed fine-form-plus-codes residence the rerank and the ADC scan read; `VectorCodebook` the static surface owning the per-subspace k-means TRAINING and the amortized asymmetric-distance corpus scan; `VectorIndex` the composition-supplied port carrier owning the codebook supply, the coarse-survivor fine-form resolve, and the PQ-coded corpus read — the ANN index residence read by reference, never embedded.
- Cases: `ProductCodebook.Centroid(subspace, code)` slices the one flat buffer at `(subspace · CodesPerSubspace + code) · SubspaceDim` so a `Subspaces × CodesPerSubspace` grid lives in one allocation and `Dimension = Subspaces · SubspaceDim`; `VectorRow.Encoding` is the fine encoding TAG as a string (`float32`/`int8-scalar`) and `VectorRow.Subject` is the optional `NodeId` the vector embeds (the FusionRank vector branch), so the residence holds no Compute type; the ADC distance is the per-row sum of one precomputed query→centroid table indexed by the row's PQ codes.
- Entry: `public static Fin<ProductCodebook> Train(Seq<ReadOnlyMemory<float>> corpus, int subspaces, int codesPerSubspace, int iterations)` runs Lloyd's k-means per subspace and content-keys the result; `public static Seq<(UInt128 ContentKey, float Distance)> AdcScan(ReadOnlyMemory<float> query, ProductCodebook codebook, Seq<VectorRow> coded, int top)` precomputes the query→centroid table once and bounded-top-K scans the coded corpus; `VectorIndex.Codebook(UInt128 id)` supplies the settled codebook to Compute, `VectorIndex.Publish(ProductCodebook)` records it content-keyed, `VectorIndex.Resolve(Seq<UInt128> survivors)` reads the coarse survivors' fine forms, `VectorIndex.Coded(UInt128 codebookId, int limit)` reads the PQ-coded corpus for the scan.
- Auto: `Train` rejects an empty/ragged corpus, a dimension not divisible by `subspaces`, and a corpus smaller than `codesPerSubspace` (which would leave trailing centroid slots untrained at zero) to a typed `Fin.Fail`, slices each corpus vector's subspace window, seeds the centroid grid from the first `codesPerSubspace` sub-vectors (Forgy initialization), and iterates assignment (nearest centroid by `TensorPrimitives.Distance`) and mean recompute (`TensorPrimitives.Add` accumulate, `TensorPrimitives.Divide` by the cluster count) — the SAME `TensorPrimitives.Distance` the Compute `EncodeProduct` assigns with, so train-time and encode-time partitions agree bit-for-bit — then mints the content `Id` over the layout ints plus the centroid bytes through `XxHash128`; `AdcScan` builds the `Subspaces × CodesPerSubspace` table by `TensorPrimitives.Distance` of each query sub-vector against every centroid ONCE, then folds each coded row to the sum of its per-subspace table lookups and keeps the nearest `top` through a bounded `PriorityQueue` heap keyed on ascending distance (never a full sort, never a per-row centroid-distance recompute — the table amortizes it); `Probe` projects the float32 fine bytes onto the `Pgvector.Vector` ANN column the HNSW/diskann index residence is built over.
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
        if (corpus.IsEmpty) { return Fin.Fail<ProductCodebook>(Error.New(8360, "<codebook-empty-corpus>")); }
        int dimension = corpus[0].Length;
        if (subspaces <= 0 || dimension % subspaces != 0 || codesPerSubspace is <= 0 or > 256) {
            return Fin.Fail<ProductCodebook>(Error.New(8361, $"<codebook-layout:{dimension}/{subspaces}@{codesPerSubspace}>"));
        }
        if (corpus.Exists(vector => vector.Length != dimension)) { return Fin.Fail<ProductCodebook>(Error.New(8362, "<codebook-ragged-corpus>")); }
        // A corpus smaller than the codebook leaves trailing centroid slots untrained at zero, which the ADC
        // table would then treat as real centroids — a silent-quality defect. Reject it rather than ship a
        // half-trained codebook: every one of the `codesPerSubspace` slots must be seeded from a real vector.
        if (corpus.Count < codesPerSubspace) { return Fin.Fail<ProductCodebook>(Error.New(8363, $"<codebook-undersized:{corpus.Count}<{codesPerSubspace}>")); }
        int subDim = dimension / subspaces;
        var centroids = new float[subspaces * codesPerSubspace * subDim];
        for (int subspace = 0; subspace < subspaces; subspace++) {
            Lloyd(corpus, subspace, subDim, codesPerSubspace, Math.Max(1, iterations), centroids.AsSpan(subspace * codesPerSubspace * subDim, codesPerSubspace * subDim));
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
