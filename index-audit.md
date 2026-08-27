# 1. Make `NodeVerdict` a keyless internal vocabulary

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:39-48`, anchor `[SmartEnum<int>] public sealed partial class NodeVerdict`; `:542-553`, anchor `verdict.Equals(NodeVerdict.Absorb)`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class NodeVerdict {
    public static readonly NodeVerdict Prune = new(key: 0, offersChildren: false, visits: false);
    public static readonly NodeVerdict Absorb = new(key: 1, offersChildren: false, visits: true);
    public static readonly NodeVerdict Descend = new(key: 2, offersChildren: true, visits: true);
```

```csharp
return total + (verdict.Equals(NodeVerdict.Absorb)
```

### To

```csharp
[SmartEnum]
internal sealed partial class NodeVerdict {
    internal static readonly NodeVerdict Prune = new(offersChildren: false, visits: false);
    internal static readonly NodeVerdict Absorb = new(offersChildren: false, visits: true);
    internal static readonly NodeVerdict Descend = new(offersChildren: true, visits: true);
```

```csharp
return total + (verdict == NodeVerdict.Absorb
```

### Why

The three-state pruning owner remains intact, but no boundary reads, orders, parses, serializes, or looks up an integer key. Keyless Thinktecture generation retains the behavior columns and generated equality while removing public visibility, three meaningless ordinals, and the generated keyed lookup, parsing, formatting, comparison, and conversion surface.

# 2. Make `SpatialKind` a behavior-only Thinktecture row set

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:98-108`, anchor `[SmartEnum<string>] public sealed partial class SpatialKind`.

### From

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new("bvh", SpatialIndex.BuildBvh);
    public static readonly SpatialKind Octree = new("octree", SpatialIndex.BuildOctree);
    public static readonly SpatialKind Agglomerative = new("agglomerative", SpatialIndex.BuildAgglomerative);
```

```csharp
[UseDelegateFromConstructor]
public partial Fin<SpatialIndex> Build(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy);
```

### To

```csharp
[SmartEnum]
public sealed partial class SpatialKind {
    public static readonly SpatialKind Bvh = new(SpatialIndex.BuildBvh);
    public static readonly SpatialKind Octree = new(SpatialIndex.BuildOctree);
    public static readonly SpatialKind Agglomerative = new(SpatialIndex.BuildAgglomerative);
```

```csharp
[UseDelegateFromConstructor]
internal partial Fin<SpatialIndex> Build(BoundingBox[] boxes, Point3d[] centroids, BuildPolicy policy);
```

### Why

Consumers select one of the declared behavior rows directly; none admits or emits a string key. The keyless declaration keeps the kernel-selection type and its generated behavior dispatch while removing duplicated string literals, comparer attributes, keyed lookup/conversion, and the public raw-array builder member.

# 3. Consume the established squared winding-separation policy

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:196`, anchor `SpatialQuery.Winding`; `:512-515`, anchor `static double[] Winding`; `:542-561`, anchors `WindingAt`, `Multipole`, and `betaSquared * radius * radius`.

### From

```csharp
public sealed record Winding(Arr<Point3d> Queries,
    Arr<(Point3d A, Point3d B, Point3d C)> Triangles,
    PositiveMagnitude Beta) : SpatialQuery;
```

```csharp
static double[] Winding(NodeStore store, SpatialQuery.Winding query) {
    (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(store, query.Triangles);
    double betaSquared = query.Beta.Value * query.Beta.Value;
    return [.. query.Queries.AsIterable().Select(point =>
        WindingAt(store, query, betaSquared, dipole, weighted, area, point))];
}
```

### To

```csharp
public sealed record Winding(Arr<Point3d> Queries,
    Arr<(Point3d A, Point3d B, Point3d C)> Triangles,
    PositiveMagnitude BetaSquared) : SpatialQuery;
```

```csharp
static double[] Winding(NodeStore store, SpatialQuery.Winding query) {
    (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(store, query.Triangles);
    return [.. query.Queries.AsIterable().Select(point =>
        WindingAt(store, query, query.BetaSquared.Value, dipole, weighted, area, point))];
}
```

### Why

`BetaSquared` is already the established winding-separation coordinate on `ArrangementPolicy` and `SdfMeshPolicy`, both canonically `4.0`, and `Multipole` consumes that coordinate directly in the squared-distance comparison. Renaming the Spatial payload to that existing owner language and deleting the second squaring removes one derived step, fixes the current `ArrangementPolicy.BetaSquared.Value` carrier mismatch, and prevents a correctly typed arrangement call from turning the established `4.0` threshold into `16.0`.

### Ripples

In `libs/dotnet/Rasm/.planning/Meshing/arrangement.md`, pass the existing `ArrangementPolicy.BetaSquared` value directly instead of `.Value` and retain its canonical `4.0`. In `libs/dotnet/Rasm/.planning/Drawing/view.md`, rename `ViewPolicy.Beta` and the `beta` admission option to `BetaSquared`/`betaSquared`, change the default raw value from `2.0` to the established squared value `4.0`, and pass that value directly to the winding query. Align the winding-field declarations and prose in `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md` with its already-established `SdfMeshPolicy.WindingCase.BetaSquared` and `betaSquared = 4.0` coordinate.

# 4. Replace erased request/result protocols with typed index operations

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:110-121`, anchor `[SmartEnum<string>] public sealed partial class QueryKind`; `:176-227`, anchors `QueryResult`, `SpatialQuery`, `SpatialOp`, and `SpatialAnswer`; `:251-261`, anchors `Admit` and `Centroids`; `:458-515`, anchors `Query`, `RangeHits`, `SlabHits`, and `Winding`; `:542-608`, anchors `WindingAt`, `RayNearest`, and `KNearest`; `:655-659`, anchor `Refit`; `:699-722`, anchor `NodeLinkProjection`; `:803-806`, anchor `UnitDirection`; `:856-875`, anchor `Spatial.Apply`.

### From

```csharp
[SmartEnum<string>]
public sealed partial class QueryKind {
    public static readonly QueryKind Range = new("range");
    public static readonly QueryKind Ray = new("ray");
    public static readonly QueryKind Nearest = new("nearest");
    public static readonly QueryKind Overlap = new("overlap");
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueryResult {
    public sealed record Hits(Seq<int> Ids) : QueryResult;
    public sealed record RayHit(Option<int> Id, double T) : QueryResult;
    public sealed record Nearest(Seq<int> Ordered) : QueryResult;
    public sealed record Pairs(Seq<(int Left, int Right)> Overlaps) : QueryResult;
    public sealed record Field(double[] Values) : QueryResult;
}
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialQuery {
    public sealed record Range(BoundingBox Box, Option<Sphere> Ball) : SpatialQuery;
    public sealed record Ray(Ray3d Probe, double MaxT) : SpatialQuery;
    public sealed record Nearest(Point3d Query, int K) : SpatialQuery;
    public sealed record Overlap(SpatialIndex Other, double Tolerance) : SpatialQuery;
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialOp {
    public sealed record Build(SpatialKind Kind, BoundingBox[] Primitives, BuildPolicy Policy) : SpatialOp;
    public sealed record Refit(SpatialIndex Index, BoundingBox[] Updated) : SpatialOp;
    public sealed record Query(SpatialIndex Index, SpatialQuery Probe) : SpatialOp;
    public sealed record Wire(SpatialIndex Index) : SpatialOp;
}
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialAnswer {
    public sealed record Index(SpatialIndex Value) : SpatialAnswer;
    public sealed record Result(QueryResult Value) : SpatialAnswer;
    public sealed record Wire(float[] Bounds, long[] Nodes) : SpatialAnswer;
}
```

```csharp
public static class Spatial {
    public static Fin<SpatialAnswer> Apply(SpatialOp op, Op? key = null) {
```

### To

```csharp
// QueryKind DELETED
// QueryResult DELETED
// SpatialQuery DELETED
// SpatialOp DELETED
// SpatialAnswer DELETED
// Spatial DELETED
```

```csharp
public static Fin<SpatialIndex> Build(
    SpatialKind kind, BoundingBox[] primitives, BuildPolicy policy, Op? key = null) {
    Op op = key.OrDefault();
    return from boxes in Admit(primitives)
           from built in kind.Build(boxes, Centroids(boxes), policy)
           from _ in guard(built.IsValid, op.InvalidResult()).ToFin()
           select built;
}
```

```csharp
public Fin<SpatialIndex> Refit(BoundingBox[] revised, Op? key = null) {
    Op op = key.OrDefault();
    Fin<SpatialIndex> result = revised.Length != Primitives.Length
        ? Fin.Fail<SpatialIndex>(new GeometryFault.IndexMismatch(EntityKind.Face, Primitives.Length, revised.Length))
        : Admit(revised).Bind(Rebound);
    return result.Bind(index => guard(index.IsValid, op.InvalidResult()).ToFin().Map(_ => index));
}
```

```csharp
public Fin<Seq<int>> Query(BoundingBox box, Option<Sphere> ball = default, Op? key = null) {
    Op op = key.OrDefault();
    return guard(box.IsValid && ball.Match(static sphere => sphere.IsValid, static () => true), op.InvalidInput()).ToFin()
        .Map(_ => LeafHits(Store, node => Intersects(Store.Bound(node), box) ? NodeVerdict.Descend : NodeVerdict.Prune,
            primitive => Intersects(Primitives[primitive], box)
                && ball.Match(sphere => SphereHits(Primitives[primitive], sphere), static () => true)));
}
```

```csharp
public Fin<(Option<int> Id, double T)> Query(Ray3d ray, double maxT, Op? key = null) {
    Vector3d direction = ray.Direction;
    Op op = key.OrDefault();
    return guard(ray.Position.IsValid && direction.IsValid && direction.Unitize()
            && double.IsFinite(maxT) && maxT > 0.0, op.InvalidInput()).ToFin()
        .Map(_ => RayNearest(Store, Primitives, new Ray3d(ray.Position, direction), maxT));
}
```

```csharp
public Fin<Seq<int>> Query(Point3d point, int count, Op? key = null) {
    Op op = key.OrDefault();
    return guard(point.IsValid && count > 0, op.InvalidInput()).ToFin()
        .Map(_ => KNearest(Store, Primitives, point, count));
}
```

```csharp
public Fin<Seq<(int Left, int Right)>> Query(SpatialIndex other, double tolerance, Op? key = null) {
    Op op = key.OrDefault();
    return guard(double.IsFinite(tolerance) && tolerance >= 0.0, op.InvalidInput()).ToFin()
        .Map(_ => OverlapPairs(this, other, tolerance, static (_, _) => true));
}
```

```csharp
public Fin<Seq<(int Left, int Right)>> Query(double tolerance, Op? key = null) {
    Op op = key.OrDefault();
    return guard(double.IsFinite(tolerance) && tolerance >= 0.0, op.InvalidInput()).ToFin()
        .Map(_ => OverlapPairs(this, this, tolerance, static (left, right) => left < right));
}
```

```csharp
public Fin<double[]> Query(Arr<Point3d> points, Arr<(Point3d A, Point3d B, Point3d C)> triangles,
    PositiveMagnitude betaSquared, Op? key = null) {
    Op op = key.OrDefault();
    return triangles.Count != Primitives.Length
        ? Fin.Fail<double[]>(new GeometryFault.IndexMismatch(EntityKind.Face, Primitives.Length, triangles.Count))
        : guard(points.Count > 0, op.InvalidInput()).ToFin().Map(_ => {
            (Vector3d[] dipole, Point3d[] weighted, double[] area) = Moments(Store, triangles);
            return [.. points.AsIterable().Select(point =>
                WindingAt(Store, triangles, betaSquared.Value, dipole, weighted, area, point))];
        });
}
```

```csharp
public Fin<Seq<int>> Query(CellLattice grid, int layer, Op? key = null) {
    Op op = key.OrDefault();
    return guard(layer >= 0 && layer < grid.Layers.Value, op.InvalidInput()).ToFin()
        .Map(_ => LeafHits(Store, node => CrossesLayer(Store.Bound(node), grid, layer)
                ? NodeVerdict.Descend : NodeVerdict.Prune,
            primitive => CrossesLayer(Primitives[primitive], grid, layer)));
}
```

```csharp
public Fin<(float[] Bounds, long[] Nodes)> Wire(Op? key = null) =>
    NodeLinkProjection(Store, key.OrDefault());
```

```csharp
// SpatialIndex.Query(SpatialQuery, Op) DELETED
// SpatialIndex.RangeHits DELETED
// SpatialIndex.SlabHits DELETED
// SpatialIndex.Winding DELETED
// SpatialIndex.UnitDirection DELETED
```

```csharp
static (Option<int> Id, double T) RayNearest(
    NodeStore store, BoundingBox[] primitives, Ray3d ray, double maxT) {
    (double best, Option<int> hit) = NodeWalk.Descend(
        root: 0, seed: (Best: maxT, Hit: Option<int>.None), step: (state, node, frontier) => {
            if (!Slab(store.Bound(node), ray, state.Best, out _)) { return state; }
```

```csharp
static Seq<int> KNearest(NodeStore store, BoundingBox[] primitives, Point3d point, int count) {
    Ranked<int, double> nearest = new(count, ExtremumDirection.Minimum);
    _ = NodeWalk.Descend(root: 0, seed: unit, step: (state, node, frontier) => {
        double lower = store.Bound(node).ClosestPoint(point).DistanceTo(point);
```

```csharp
static double WindingAt(NodeStore store, Arr<(Point3d A, Point3d B, Point3d C)> triangles,
    double betaSquared, Vector3d[] dipole, Point3d[] weighted, double[] area, Point3d point) =>
```

### Why

Every caller knows the operation and query shape it constructs, but the current surface erases that fact twice: first into request unions and then into unrelated result unions that each caller runtime-checks back to its only possible answer. Typed operations on the index preserve one `Query` name with input-shape dispatch, delete five module-level owners, twenty nested case types, the seven-row mirrored query vocabulary, two generated dispatch planes, and every impossible-answer branch. Build, refit, and wire retain their admission and validity gates; ray misses retain `(None, maxT)`; winding retains the existing `Beta` semantics and squares it only at the multipole comparison.

### Ripples

Replace `Spatial.Apply` construction and `SpatialAnswer`/`QueryResult` unwrapping with `SpatialIndex.Build`, instance `Refit`, typed `Query`, and instance `Wire` in `libs/dotnet/Rasm/.planning/Drawing/hatch.md`, `Drawing/view.md`, `Meshing/arrangement.md`, `Meshing/intersect.md`, `Meshing/offset.md`, `Processing/decimate.md`, `Processing/remesh.md`, `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md`, `Render/reality.md`, `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md`, `Solver/clash.md`, `libs/dotnet/Rasm.Fabrication/.planning/Additive/scanpath.md`, `Additive/support.md`, `Geometry2D/algebra.md`, `Nesting/linking.md`, `Spec/manufacturability.md`, `Toolpath/guard.md`, and `Verify/audit.md`; delete consumer helpers whose only work is erased-answer unwrapping. Update the retired vocabulary and direct producer wording in `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md`, `Meshing/slice.md`, `libs/dotnet/Rasm/.api/api-rhino.md`, `libs/dotnet/Rasm.AppUi/.planning/Render/immersive.md`, `libs/dotnet/Rasm.Compute/ARCHITECTURE.md`, `libs/dotnet/Rasm/README.md`, and `libs/dotnet/Rasm/ARCHITECTURE.md`. In `Spec/manufacturability.md`, read the ray tuple directly; wire consumers accept `(float[] Bounds, long[] Nodes)` directly.

# 4. Collapse `SpatialIndex` cases onto their optional refit baseline

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:131-136`, anchors `BuildPolicy.RefitGrowth` and `RefitDegradationLimit`; `:230-240`, anchor `[Union] public abstract partial record SpatialIndex`; `:294-295`, `:323-324`, and `:360-361`, builder-result anchors; `:679-685`, anchor `return Switch<Fin<SpatialIndex>>`.

### From

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpatialIndex : IValidityEvidence {
    public sealed record Bvh(NodeStore Store, BoundingBox[] Primitives, double BuildCost,
        BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;
    public sealed record LinearOctree(NodeStore Store, BoundingBox[] Primitives,
        BuildPolicy Policy, SpatialKind Builder) : SpatialIndex;
```

```csharp
double cost = AggregateSahCost(refitted);
return Switch<Fin<SpatialIndex>>(
    bvh: b => cost > b.Policy.RefitDegradationLimit * b.BuildCost
        ? b.Builder.Build(updated, Centroids(updated), b.Policy)
        : Fin.Succ((SpatialIndex)(b with { Primitives = updated, Store = refitted })),
    linearOctree: o => Fin.Succ((SpatialIndex)(o with { Primitives = updated, Store = refitted })));
```

```csharp
public double RefitDegradationLimit => 1.0 + RefitGrowth.Value;
```

### To

```csharp
public sealed class SpatialIndex : IValidityEvidence {
    private SpatialIndex(NodeStore store, BoundingBox[] primitives, BuildPolicy policy,
        SpatialKind kind, Option<double> baseline) =>
        (Store, Primitives, Policy, Kind, Baseline) = (store, primitives, policy, kind, baseline);
    public NodeStore Store { get; }
    BoundingBox[] Primitives { get; }
    BuildPolicy Policy { get; }
    SpatialKind Kind { get; }
    Option<double> Baseline { get; }

// SpatialIndex.Bvh DELETED
// SpatialIndex.LinearOctree DELETED
```

```csharp
return Baseline.Match(
    Some: prior => AggregateSahCost(refitted) > (1.0 + Policy.RefitGrowth.Value) * prior
        ? Kind.Build(updated, Centroids(updated), Policy)
        : Fin.Succ(new SpatialIndex(refitted, updated, Policy, Kind, Baseline)),
    None: () => Fin.Succ(new SpatialIndex(refitted, updated, Policy, Kind, None)));
```

```csharp
return Fin.Succ(new SpatialIndex(store, boxes, policy, SpatialKind.Bvh,
    Some(AggregateSahCost(store))));
```

```csharp
return Fin.Succ(new SpatialIndex(arena.Freeze(next, order), boxes, policy,
    SpatialKind.Octree, None));
```

```csharp
return Fin.Succ(new SpatialIndex(store, boxes, policy, SpatialKind.Agglomerative,
    Some(AggregateSahCost(store))));
```

```csharp
// BuildPolicy.RefitDegradationLimit DELETED
```

### Why

Both cases share identity, admission, consumer, lifecycle, and all stored coordinates; only BVH-shaped builds carry a baseline SAH value that can trigger rebuild on refit. One class with optional evidence deletes two nested case types and the generated union dispatch/equality surface, prevents the octree from computing an unused SAH cost, and makes primitives, policy, kind, and baseline implementation state. The one-use degradation alias adds no authority beyond admitted `RefitGrowth`, so refit reads the policy column directly.

### Ripples

Replace the `[Union]` and polymorphic-case wording in `libs/dotnet/Rasm/ARCHITECTURE.md` and `libs/dotnet/Rasm/README.md`. In `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md`, replace the invalid `BuildPolicy.Canonical with { RefitDegradationLimit = ... }` and nonexistent `IsAdmitted` probe with `BuildPolicy.Of`, passing `refitGrowth: RefitDegradationLimit - 1.0` and the unchanged canonical values.

# 5. Accumulate independent `BuildPolicy` admissions before their relation

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:138-147`, anchor `public static Fin<BuildPolicy> Of`.

### From

```csharp
return from leaf in op.AcceptValidated<Dimension>(candidate: leafSize)
       from depth in op.AcceptValidated<Dimension>(candidate: maxDepth)
       from buckets in op.AcceptValidated<Dimension>(candidate: sahBuckets)
       from growth in op.AcceptValidated<PositiveMagnitude>(candidate: refitGrowth)
       from floor in op.AcceptValidated<Dimension>(candidate: parallelFloor)
```

```csharp
       from _ in guard(leaf.Value <= PackedCountMax && buckets.Value > 1, op.InvalidInput()).ToFin()
       select new BuildPolicy(leafSize: leaf, maxDepth: depth, sahBuckets: buckets,
           refitGrowth: growth, parallelFloor: floor);
```

### To

```csharp
return (op.AcceptValidated<Dimension>(leafSize).ToValidation(),
        op.AcceptValidated<Dimension>(maxDepth).ToValidation(),
        op.AcceptValidated<Dimension>(sahBuckets).ToValidation(),
        op.AcceptValidated<PositiveMagnitude>(refitGrowth).ToValidation(),
        op.AcceptValidated<Dimension>(parallelFloor).ToValidation())
```

```csharp
    .Apply((leaf, depth, buckets, growth, floor) =>
        new BuildPolicy(leaf, depth, buckets, growth, floor)).As().ToFin()
    .Bind(policy => guard(policy.LeafSize.Value <= PackedCountMax
            && policy.SahBuckets.Value > 1, op.InvalidInput()).ToFin()
        .Map(_ => policy));
```

### Why

The five generated value-object admissions are independent; only the packed-count and bucket-floor relation depends on their admitted values. LanguageExt tuple `Apply` accumulates all independent refusals, `.As().ToFin()` crosses back once, and `Bind` then sequences the dependent relation. The current monadic chain discards every unrelated refusal after the first.

### Ripples

Route `BuildPolicy.Canonical with` through `BuildPolicy.Of` in `libs/dotnet/Rasm.AppUi/.planning/Render/pathtrace.md`, `Render/reality.md`, and both occurrences in `libs/dotnet/Rasm.Fabrication/.planning/Documentation/projection.md`; compose the returned `Fin<BuildPolicy>` and delete the nonexistent `IsAdmitted` probes.

# 6. Derive `NodeStore.Count` and make structural validation total

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:154-174`, anchors `public sealed record NodeStore` and `public BoundingBox Bound`; `:242-248`, anchor `public bool IsValid`; `:748-751`, anchor `Arena.Freeze`; `:762-768`, anchor `Links`.

### From

```csharp
public sealed record NodeStore(
    int Count,
    float[] BoundsMin,
    float[] BoundsMax,
    int[] FirstChild,
```

```csharp
internal NodeStore Freeze(int count, int[] order) => new(count,
    mins.Span[..(3 * count)].ToArray(), maxs.Span[..(3 * count)].ToArray(),
```

```csharp
public bool IsValid => ValidityClaim.All(
    ValidityClaim.CountAtLeast(count: Store.Count, floor: 1),
    ValidityClaim.CountExactly(count: Store.BoundsMin.Length, expected: 3 * Store.Count),
    ValidityClaim.CountExactly(count: Store.BoundsMax.Length, expected: 3 * Store.Count),
    ValidityClaim.CountExactly(count: Store.Order.Length, expected: Primitives.Length),
```

```csharp
static bool Links(NodeStore store) {
    for (int node = 0; node < store.Count; node++) {
        bool leaf = store.Leaf(node);
```

### To

```csharp
public sealed record NodeStore(
    float[] BoundsMin,
    float[] BoundsMax,
    int[] FirstChild,
    int[] ChildCount,
```

```csharp
public int Count => FirstChild.Length;

internal NodeStore Freeze(int count, int[] order) => new(
    mins.Span[..(3 * count)].ToArray(), maxs.Span[..(3 * count)].ToArray(),
```

```csharp
ValidityClaim.CountExactly(count: Store.ChildCount.Length, expected: Store.Count),
ValidityClaim.CountExactly(count: Store.LeafStart.Length, expected: Store.Count),
ValidityClaim.CountExactly(count: Store.LeafCount.Length, expected: Store.Count),
ValidityClaim.CountExactly(count: Store.Order.Length, expected: Primitives.Length),
Store.Order.All(primitive => (uint)primitive < (uint)Primitives.Length)
    && Store.Order.Distinct().Count() == Primitives.Length,
```

```csharp
static bool Links(NodeStore store) {
    if (store.ChildCount.Length != store.Count || store.LeafStart.Length != store.Count
        || store.LeafCount.Length != store.Count) { return false; }
    for (int node = 0; node < store.Count; node++) {
        bool leaf = store.LeafCount[node] > 0;
```

```csharp
if (leaf
        ? store.ChildCount[node] != 0 || store.FirstChild[node] != 0
        : store.ChildCount[node] == 0 || store.LeafStart[node] != -1) { return false; }
if (leaf && (store.LeafStart[node] < 0
        || store.LeafCount[node] > store.Order.Length - store.LeafStart[node])) { return false; }
```

```csharp
if (!leaf && (store.FirstChild[node] <= node
        || store.ChildCount[node] > store.Count - store.FirstChild[node])) { return false; }
```

```csharp
internal BoundingBox Bound(int node) {
    ReadOnlySpan<float> lo = Lower.GetRowSpan(node), hi = Upper.GetRowSpan(node);
    return new(new Point3d(lo[0], lo[1], lo[2]), new Point3d(hi[0], hi[1], hi[2]));
}
```

### Why

`FirstChild` already owns exactly one slot per node, so stored `Count` is a duplicate authority. `Bound` remains the real internal reconstruction operation while the public crossing stays `Wire`. The current eager validity fold omits three parallel lane lengths and can index a short lane inside `Links`, converting invalid evidence into an exception. The total proof guards every indexed lane first, establishes canonical leaf/internal rows, and proves `Order` is an in-range primitive permutation.

# 7. Seat wire projection on its only public entrypoint

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:699-722`, anchor `NodeLinkProjection`; `:872-873`, anchor `SpatialIndex.NodeLinkProjection(w.Index.Store, k)`.

### From

```csharp
internal static Fin<(float[] Bounds, long[] Nodes)> NodeLinkProjection(NodeStore store, Op key) {
    for (int node = 0; node < store.Count; node++)
        if (store.LeafCount[node] > BuildPolicy.PackedCountMax || store.ChildCount[node] > BuildPolicy.PackedCountMax)
            return Fin.Fail<(float[] Bounds, long[] Nodes)>(key.InvalidInput());
```

```csharp
public Fin<(float[] Bounds, long[] Nodes)> Wire(Op? key = null) =>
    NodeLinkProjection(Store, key.OrDefault());
```

### To

```csharp
public Fin<(float[] Bounds, long[] Nodes)> Wire(Op? key = null) {
    Op op = key.OrDefault();
    for (int node = 0; node < Store.Count; node++)
        if (Store.LeafCount[node] > BuildPolicy.PackedCountMax || Store.ChildCount[node] > BuildPolicy.PackedCountMax)
            return Fin.Fail<(float[] Bounds, long[] Nodes)>(op.InvalidInput());
```

```csharp
int count = Store.Count;
float[] bounds = new float[6 * count];
long[] nodes = new long[count + Store.Order.Length];
int tail = count;
Span2D<float> wire = bounds.AsSpan2D(height: count, width: 6);
```

```csharp
Store.Lower.GetRowSpan(node).CopyTo(row[..3]);
Store.Upper.GetRowSpan(node).CopyTo(row[3..]);
```

```csharp
// SpatialIndex.NodeLinkProjection DELETED
```

### Why

Once the erased operation protocol is gone, `NodeLinkProjection` has one caller that only forwards `Store` and the operation key. Seating the unchanged packing loop on `Wire` removes the forwarding member and keeps packed-layout validation, projection, and egress on their one owner.

# 8. Use RhinoCommon AABB intersection and inline sphere refinement

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:502-505`, anchor `RangeHits`; `:638-643`, intersection reads in `OverlapPairs`; `:808-811`, anchors `Intersects` and `SphereHits`.

### From

```csharp
Intersects(store.Bound(node), range.Box)
```

```csharp
Intersects(primitives[prim], range.Box)
    && range.Ball.Match(ball => SphereHits(primitives[prim], ball), static () => true)
```

```csharp
static bool Intersects(BoundingBox a, BoundingBox b) =>
    a.Min.X <= b.Max.X && a.Max.X >= b.Min.X && a.Min.Y <= b.Max.Y
    && a.Max.Y >= b.Min.Y && a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;
```

```csharp
static bool SphereHits(BoundingBox box, Sphere ball) =>
    box.ClosestPoint(ball.Center).DistanceTo(ball.Center) <= ball.Radius;
```

### To

```csharp
BoundingBox.Intersection(Store.Bound(node), box).IsValid
```

```csharp
BoundingBox.Intersection(Primitives[primitive], box).IsValid
    && ball.Match(sphere => Primitives[primitive].ClosestPoint(sphere.Center)
        .DistanceTo(sphere.Center) <= sphere.Radius, static () => true)
```

```csharp
if (!BoundingBox.Intersection(Inflate(ls.Bound(l), tolerance), rs.Bound(r)).IsValid) { return pairs; }
```

```csharp
// SpatialIndex.Intersects DELETED
// SpatialIndex.SphereHits DELETED
```

### Why

RhinoCommon already owns expression-shaped AABB intersection, including touching extents, so the six-comparison wrapper hand-rolls an admitted package member. The spherical refinement has one caller and merely renames `ClosestPoint` plus `DistanceTo`; inlining it at leaf admission removes a second private member without obscuring reusable logic.

# 9. Inline the one-call leaf-bound fold into `LeafRefit`

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:688-697`, anchor `readonly struct LeafRefit`; `:789-793`, anchor `LeafBound`.

### From

```csharp
public void Invoke(int node) {
    if (!store.Leaf(node)) { return; }
    BoundingBox bound = LeafBound(store, boxes, node);
```

```csharp
static BoundingBox LeafBound(NodeStore store, BoundingBox[] boxes, int node) {
    BoundingBox box = BoundingBox.Empty;
    foreach (int prim in store.Primitives(node)) box.Union(boxes[prim]);
    return box;
}
```

### To

```csharp
public void Invoke(int node) {
    if (!store.Leaf(node)) { return; }
    BoundingBox bound = BoundingBox.Empty;
    foreach (int primitive in store.Primitives(node)) { bound.Union(boxes[primitive]); }
```

```csharp
// SpatialIndex.LeafBound DELETED
```

### Why

The three-line fold belongs to the only action that consumes it and shares no state with another kernel. Inlining removes one private member while retaining `LeafRefit` as the required `IAction` capsule for `ParallelHelper.For`.

# 10. Reuse the octree cell bound already carried by recursion

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:304-307`, anchor local function `bool Cell` and its leaf arm.

### From

```csharp
bool Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
    int count = hi - lo;
    if (count <= policy.LeafSize.Value || depth >= ceiling) {
        return arena.Write(node, Union(boxes, order, lo, hi), 0, 0, lo, count);
    }
```

### To

```csharp
bool Cell(int node, int lo, int hi, int depth, BoundingBox bound) {
    int count = hi - lo;
    if (count <= policy.LeafSize.Value || depth >= ceiling) {
        return arena.Write(node, bound, 0, 0, lo, count);
    }
```

### Why

The root and every recursive call already compute the exact primitive union before passing `bound`. Reading that value at the terminal cell removes a duplicate primitive traversal and makes the parameter's purpose explicit without adding a symbol.

# 11. Inline the one-call centroid-bound fold

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:272-279`, anchor local function `Partition` and `CentroidBound`; `:783-787`, anchor `static BoundingBox CentroidBound`.

### From

```csharp
BoundingBox centroidBound = CentroidBound(centroids, order, lo, hi);
```

```csharp
static BoundingBox CentroidBound(Point3d[] centroids, int[] order, int lo, int hi) {
    BoundingBox box = BoundingBox.Empty;
    for (int i = lo; i < hi; i++) box.Union(centroids[order[i]]);
    return box;
}
```

### To

```csharp
BoundingBox centroidBound = BoundingBox.Empty;
for (int i = lo; i < hi; i++) { centroidBound.Union(centroids[order[i]]); }
```

```csharp
// SpatialIndex.CentroidBound DELETED
```

### Why

The centroid extent is consumed only by the enclosing BVH partition decision, and its fold is three lines over state that decision already owns. Keeping it local removes a one-call private member without duplicating an independently reusable algorithm.

# 12. Inline the sole stable partition and remove its delegate

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:283-286`, anchor `int mid = StablePartition`; `:847-853`, anchor `static int StablePartition`.

### From

```csharp
int mid = StablePartition(order, lo, hi, scratch.Span, index =>
    (int)(buckets * (Axis(centroids[index], axis) - Axis(centroidBound.Min, axis))
        / Math.Max(extent, EpsilonPolicy.ZeroTolerance)) <= splitBucket);
```

```csharp
static int StablePartition(int[] order, int lo, int hi, Span<int> scratch, Func<int, bool> onLeft) {
    Span<int> buffer = scratch[..(hi - lo)];
    int write = lo, b = 0;
```

### To

```csharp
Span<int> partition = scratch.Span[..count];
int mid = lo, right = 0;
for (int i = lo; i < hi; i++) {
    int primitive = order[i];
    int bucket = (int)(buckets * (Axis(centroids[primitive], axis) - Axis(centroidBound.Min, axis))
        / Math.Max(extent, EpsilonPolicy.ZeroTolerance));
```

```csharp
    if (bucket <= splitBucket) { order[mid++] = primitive; }
    else { partition[right++] = primitive; }
}
partition[..right].CopyTo(order.AsSpan(mid, right));
```

```csharp
// SpatialIndex.StablePartition DELETED
```

### Why

`StablePartition` has one caller and its predicate closes over the BVH decision's buckets, axis, extent, and centroid bound. Keeping the stable left-in-place/right-in-scratch walk at that decision removes one private method and one `Func<int, bool>` allocation boundary while preserving both partitions' order.

# 13. Fold the one-call Morton combiner into `MortonOrder`

### Location

`libs/dotnet/Rasm/.planning/Spatial/index.md:402-408`, anchor `static (uint[] Codes, int[] Order) MortonOrder`; `:833`, anchor `static uint Morton`.

### From

```csharp
uint[] codes = Array.ConvertAll(centroids, c => Morton(
    Normalize(c.X, root.Min.X, span.X), Normalize(c.Y, root.Min.Y, span.Y),
    Normalize(c.Z, root.Min.Z, span.Z)));
```

```csharp
static uint Morton(uint x, uint y, uint z) =>
    Expand10(x) | (Expand10(y) << 1) | (Expand10(z) << 2);
```

### To

```csharp
uint[] codes = Array.ConvertAll(centroids, c =>
    Expand10(Normalize(c.X, root.Min.X, span.X))
    | (Expand10(Normalize(c.Y, root.Min.Y, span.Y)) << 1)
    | (Expand10(Normalize(c.Z, root.Min.Z, span.Z)) << 2));
```

```csharp
// SpatialIndex.Morton DELETED
```

### Why

`Morton` is a one-line name used only while `MortonOrder` converts centroids. Folding its three interleaves into that conversion removes one private member while preserving `Expand10` and `Normalize`, the two primitives shared by all axes.
