# [PERSISTENCE_QUERY_LANE]

Rasm.Persistence routes every read by its consistency demand: interactive-correctness queries (clash, void-resolution, live QTO, containment) bind the synchronous authoritative lane — the inline `Element/graph` `GraphProjection` and the in-process `Query/topology` QuikGraph view — while analytical queries (aggregation, search, columnar rollup) bind the async watermarked columnar and cypher lanes. A query demanding correctness from an async view blocks on the projection daemon's non-stale wait before reading, so a read-your-writes interactive query is correct by construction and never touches a daemon-lagged projection.

`QueryLane` is the lane axis carrying each lane's consistency stance and staleness ceiling; `ReadDemand` discriminates the route; `StalenessWatermark` measures projection lag as the event-log head sequence against the daemon shard's high-water mark, never a hardcoded zero. `ElementSet` is the universal content-addressed selection currency every clash/IDS/MVD/QTO surface consumes and produces, `SetExpr` its selection-tree algebra and `SetPredicate` its closed typed leaf algebra so no leaf carries a raw string predicate, and `Closure` folds a bounded transitive walk over the `Query/topology` incidence. A retrieval-shaped read (vector ANN / spatial GiST / lexical BM25) routes to the `Query/retrieval` fusion lane, which read-through-caches on the `ElementSet.Receipt` this owner mints — the receipt currency stays here, the fusion and its cache live there. `NodeId`/`ElementGraph` arrive from `Rasm.Element`, the inline projection from `Element/graph`, the columnar/cypher lanes from their owners, and `ReceiptSinkPort` from AppHost as an injected port value on the Persistence-owned `ProjectionContext` frame.

## [01]-[INDEX]

- [01]-[READ_ROUTING]: the consistency-demand routing law, the lane axis, the staleness watermark, and the daemon non-stale wait gate.
- [02]-[ELEMENT_SET_ALGEBRA]: the composable content-addressed selection currency, the typed leaf algebra, and the stable receipt fold.

## [02]-[READ_ROUTING]

- Owner: `QueryLane` the lane axis carrying each lane's `Consistency` stance and `StalenessCeiling`; `ReadDemand` the routing discriminant; `Consistency` the synchronous-versus-async stance; `StalenessWatermark` the measured projection-lag value; `ReadRouter` the static surface routing a demand to its lane and gating an async read on the non-stale wait.
- Cases: `QueryLane` rows are `Topology(Synchronous)` (in-process QuikGraph + inline projection — interactive correctness), `Columnar(Async)` (DuckDB/Parquet/BimOpenSchema — analytical), `Cypher(Async)` (Apache AGE — optional analytical), `Cache(Derived)` (the `Query/retrieval` fusion + read-through reuse tier); `ReadDemand` is `InteractiveCorrectness | Analytical`; `Consistency` is `Synchronous | Async | Derived`.
- Entry: `Route` selects the lane — an interactive-correctness demand binds synchronous `Topology`, an analytical demand routes by shape (topological to `Cypher`, retrieval to `Cache`, an explicit `Aggregate` to `Columnar`, a no-evidence shape back to `Topology` for want of an async warrant) with every `QueryShape` bit read so no bool is a dead knob; `AwaitNonStale` blocks an interactive read that must touch an async view on `IProjectionDaemon.WaitForNonStaleData`, the production projection-runner member minted per store through `store.BuildProjectionDaemonAsync()` at the composition root, never the `Marten.Events` `TestingExtensions` store/host convenience overloads; `Measure` folds the event-log head sequence against the shard's projected high-water `Sequence` into a measured gap, its live-store arm reading both off `FetchEventStoreStatistics` and `AllProjectionProgress` matched by the shard's string identity, so a consumer reads real daemon lag built from the verified `EventSequenceNumber`/`ShardState.Sequence` members alone.
- Auto: an interactive-correctness query (clash narrow-phase, void-resolution, live QTO, containment ancestry) routes to the synchronous lane by construction so it reads the inline `GraphProjection` and QuikGraph view written in the append transaction, never a daemon-lagged async projection; an analytical query carries the `StalenessWatermark` so its consumer reads the lag; a re-run analytical clash demanding correctness from an async view calls `AwaitNonStale` first so the daemon catches up to the head before the read.
- Receipt: a routed read rides `store.query.route` carrying the demand and the lane; an async-stale wait rides `store.query.wait` carrying the watermark and the elapsed wait.
- Packages: Marten (`IProjectionDaemon.WaitForNonStaleData(TimeSpan)` the production non-stale block; `ShardState`/`ShardName`/`EventStoreStatistics`, `AdvancedOperations.FetchEventStoreStatistics`/`AllProjectionProgress`), NodaTime (`Duration`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new lane is one `QueryLane` row carrying its consistency stance; a new read demand is one `ReadDemand` row plus one routing arm; zero new surface — routing an interactive-correctness read to an async view, a per-query consistency knob beside the lane, or a staleness claim as prose is the deleted form because the lane carries the stance and the watermark is a measured value.
- Boundary: authoritative topology and containment stay synchronous and co-transactional (`C2`) — the inline `GraphProjection` in the write transaction, the in-process QuikGraph view — so a read-your-writes interactive query is correct by construction; that synchronous lane is NOT infallible, since the `Query/topology` `Traversals.Run` it binds returns `Fin<TopologyResult>` railing the typed `TopologyFault` band, so a router consumer composes the topology `Fin` into its OWN rail rather than assuming success and an absent-root containment query surfaces as an honest typed fault, never a silent empty result; AGE and DuckDB are ANALYTICAL ONLY with an explicit `StalenessWatermark`, and interactive-correctness queries block on `WaitForNonStaleData` and never route to an async projection without the wait — a clash reading a daemon-lagged AGE view is the deleted form, and the gate rides the production `IProjectionDaemon`, not a test-only symbol; staleness is a MEASURED sequence gap (`EventSequenceNumber` head against `ShardState.Sequence`), never `ShardState.Timestamp`, a daemon-side recording stamp (`DateTimeOffset.UtcNow` at row construction) that measures read-latency rather than producer-to-projection lag — a `Measure` returning `Duration.Zero` on a trailing shard is the illusory form this owner forbids; strong-consistency reads go through the inline projection and the synchronous topology, never the columnar aggregate, so the columnar lane stays the rollup/search lane and the topology lane the correctness lane.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using LanguageExt;
using Marten;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using NetTopologySuite.Geometries;
using NodaTime;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
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
    // BM25, the `Query/retrieval#FUSION_AND_REUSE` `RetrievalBranch` set) to the content-addressed `Cache` lane that
    // owns the RRF fusion + read-through reuse, an `Aggregate` shape to the `Columnar` rollup lane, and a NO-evidence
    // shape (every bit false) back to the synchronous `Topology` lane because a shapeless analytical read has no
    // async-lane warrant and the correct default is the read-your-writes topology, not a silent rollup. `Aggregate`
    // is thus the explicit `Columnar` discriminant — a `Columnar` fall-through that fires regardless of `Aggregate`
    // would make it the dead knob the KNOB_TEST deletes, so it is READ, never inferred from the absence of the rest.
    public static QueryLane Route(ReadDemand demand, QueryShape shape) =>
        demand.Correctness ? QueryLane.Topology
        : shape.Topological ? QueryLane.Cypher
        : shape.Vector || shape.Spatial || shape.Lexical ? QueryLane.Cache
        : shape.Aggregate ? QueryLane.Columnar
        : QueryLane.Topology;

    // The daemon IS the production non-stale surface: `IProjectionDaemon.WaitForNonStaleData` blocks until every
    // async shard has folded the head. The `store.WaitForNonStaleProjectionDataAsync` / `host.…` overloads are
    // `Marten.Events` TestingExtensions convenience wrappers — routing the gate through them would hang the lane's
    // consistency law off a test-support symbol, the dependency defect this signature deletes.
    public static IO<Unit> AwaitNonStale(IProjectionDaemon daemon, QueryLane lane, Duration timeout) =>
        lane.Consistency.Blocks
            ? IO.liftAsync(async () => { await daemon.WaitForNonStaleData(timeout.ToTimeSpan()).ConfigureAwait(false); return unit; })
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

| [INDEX] | [POLICY]                | [VALUE]                                             | [BINDING]                                               |
| :-----: | :---------------------- | :-------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | interactive correctness | the synchronous `Topology` lane                     | inline projection + QuikGraph; never an async view      |
|  [02]   | analytical              | the async `Columnar`/`Cypher` lane                  | carries the `StalenessWatermark`                        |
|  [03]   | shape routing           | every `QueryShape` bit READ; `Aggregate`→`Columnar` | no-evidence → sync `Topology`; no dead knob             |
|  [04]   | non-stale gate          | `IProjectionDaemon.WaitForNonStaleData`             | the production runner member; not `TestingExtensions`   |
|  [05]   | watermark               | `EventSequenceNumber` vs shard `Sequence`           | the sequence gap, never the recording-clock `Timestamp` |

## [03]-[ELEMENT_SET_ALGEBRA]

- Owner: `ElementSet` the polymorphic composable selection record carrying a stable content-addressed receipt; `SetPredicate` the closed leaf-predicate algebra; `SetExpr` the selection-tree algebra; `ElementSetAlgebra` the static surface owning literal selection, the boolean/spatial/property/classification combinators, and the stable-receipt fold.
- Cases: `Spatial | Jsonpath | Classification | Containment | Material | Exists` on `SetPredicate` (the bounded operator within each typed — `SpatialOp` on `Spatial`, `JsonComparison` on `Jsonpath`); `Literal | Predicate | ByRule | Union | Intersect | Difference | Closure` on `SetExpr`.
- Entry: `public static ElementSet Evaluate(SetExpr expr, SetResolve resolve)` folds the expression tree into a stable key set through the two store-lowering ports the `SetResolve` carries (`Leaf` lowers a `Predicate`/`ByRule` to an index scan, `Expand` is the one-hop topology neighbour the `Closure` fold iterates); `public static UInt128 Receipt(Seq<NodeId> sortedKeys)` derives the content-addressed set identity over the length-framed distinct-sorted preimage so the same selection produces the same receipt across runs and peers; `public static ReadOnlyMemory<byte> Canonical(Seq<NodeId> sortedKeys)` is the length-framed preimage the receipt hashes and the parity corpus freezes.
- Auto: an element set is the universal BIM currency — clash, IDS, MVD, QTO, and rule surfaces all consume and produce `ElementSet` values, so a clash result is an `ElementSet`, an IDS pass-set is an `ElementSet`, and a QTO subject is an `ElementSet`; the set receipt is `XxHash128` over the LENGTH-FRAMED distinct-sorted `NodeId` preimage (a LE `int32` key count, then per key a LE `int32` byte length and its UTF8 bytes) so two selections yielding the same elements share one receipt AND two different key sets can never collide on an unframed concatenation; the boolean combinators fold over evaluated leaf sets, and the one `Predicate` leaf carries a `SetPredicate` — `Spatial` lowers to the GiST predicate the TYPED `SpatialOp` `.Key` (`ST_Intersects`/`ST_Within`/`ST_DWithin`/…) names so a typo is a missing vocabulary row at compile time rather than a silent sequential scan, `Jsonpath` to a jsonb path predicate under the typed `JsonComparison` comparator, `Classification` to a tsvector/classification predicate, `Containment` to the containment-edge ancestry, `Material`/`Exists` to their jsonb existence forms; the `Closure` arm is a GENUINE bounded transitive fold — it evaluates its `Seed` sub-expression then folds `Depth` one-hop `Expand` waves accumulating the reachable frontier to its fixpoint, never an opaque leaf identical to `Predicate`.
- Receipt: an evaluation rides `store.elementset.eval` carrying the leaf count and the result cardinality; the stable receipt is the reuse key the `Query/retrieval#FUSION_AND_REUSE` read-through caches on.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the one federation hasher, seed-zero `XxHash128` value-identical), System.Buffers (`ArrayBufferWriter`/`BinaryPrimitives`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NetTopologySuite, NodaTime, BCL inbox.
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
    // `Eq`, never `Equals`: a static item field named `Equals` collides with the generated
    // `Equals(object?)`/`Equals(JsonComparison?)` members in the same partial type (CS0102); the wire key stays "eq".
    public static readonly JsonComparison Eq = new("eq");
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
        return new ElementSet(ContentHash.Of(preimage.Span), sorted, sorted.Count, preimage);
    }
}

// `SetResolve` carries the TWO store-lowering ports the planner supplies: `Leaf` lowers a `Predicate`/`ByRule`
// leaf to its key set on the owning index, `Expand` is the ONE-HOP neighbour function (the `Query/topology`
// incidence over containment+connection edges) the `Closure` fold iterates. Threading the expander as a port
// keeps the reachability owner in `Query/topology`/`Element/graph` (the seam graph) and the algebra here.
public readonly record struct SetResolve(Func<SetExpr, Seq<NodeId>> Leaf, Func<Seq<NodeId>, Seq<NodeId>> Expand);

public static class ElementSetAlgebra {
    // The receipt composes the kernel `ContentHash.Of` seed-zero entry ([B] — value-identical to the raw
    // `XxHash128.HashToUInt128` the parity corpus froze, so the cross-runtime `elementset` ParityVector bytes
    // are untouched; the call-path collapse deletes the second mint site, never the identity).
    public static UInt128 Receipt(Seq<NodeId> sortedKeys) => ContentHash.Of(Canonical(sortedKeys).Span);

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

| [INDEX] | [POLICY]           | [VALUE]                                                 | [BINDING]                                                |
| :-----: | :----------------- | :------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | selection currency | `ElementSet` in and out                                 | every analysis surface composes; never an app join       |
|  [02]   | receipt            | `ContentHash.Of` over length-framed preimage            | stable + collision-free; the reuse key + parity preimage |
|  [03]   | typed leaves       | `SetPredicate` + `SpatialOp`/`JsonComparison` operators | no raw-string predicate/op; lowered to a store index     |
|  [04]   | closure            | bounded transitive fold over topology                   | one-hop `Expand` is `Query/topology`; not the manifest   |
