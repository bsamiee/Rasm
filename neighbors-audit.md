# 1. Make process-local vocabularies keyless

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:44-51`, anchor `NeighborMetric`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:300-314`, anchor `CurvatureRangeKind`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:329-340`, anchor `CurvatureAxis`

### From

```csharp
[SmartEnum<int>]
public sealed partial class NeighborMetric {
    public static readonly NeighborMetric Euclidean = new(key: 0, body: KDTree.EuclideanDistance, searchRadius: static r => r * r);
```

```csharp
[SmartEnum<int>]
public sealed partial class CurvatureRangeKind {
    public static readonly CurvatureRangeKind Plane = new(key: 1,
```

```csharp
[SmartEnum<int>]
public sealed partial class CurvatureAxis {
    public static readonly CurvatureAxis Principal = new(key: 0, project: static s => s.K1);
```

### To

```csharp
[SmartEnum]
public sealed partial class NeighborMetric {
    public static readonly NeighborMetric Euclidean = new(body: KDTree.EuclideanDistance, searchRadius: static r => r * r);
    // NeighborMetric.Key DELETED
```

```csharp
[SmartEnum]
public sealed partial class CurvatureRangeKind {
    public static readonly CurvatureRangeKind Plane = new(
    // CurvatureRangeKind.Key DELETED
```

```csharp
[SmartEnum]
public sealed partial class CurvatureAxis {
    public static readonly CurvatureAxis Principal = new(project: static s => s.K1);
    // CurvatureAxis.Key DELETED
```

Delete every surviving row's `key:` argument; task 13 deletes `CurvatureRangeKind.Empty` rather than re-keying it.

### Why

No `libs/dotnet/` consumer reads, admits, serializes, parses, persists, or converts these keys. Thinktecture's keyless smart enum retains singleton identity, `Items`, delegate columns, and generated dispatch while deleting unused key properties, lookup, parsing, conversions, and keyed-owner conformance.

# 2. Bind each metric tree at construction

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:46-50`, anchors `NeighborMetric.Euclidean` through `NeighborMetric.Body`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:167-170`, anchor `tree.Metric = row.Body`

### From

```csharp
public static readonly NeighborMetric Euclidean = new(key: 0, body: KDTree.EuclideanDistance, searchRadius: static r => r * r);
public static readonly NeighborMetric Manhattan = new(key: 1, body: KDTree.ManhattanDistance, searchRadius: static r => r);
public static readonly NeighborMetric Chebyshev = new(key: 2, body: KDTree.ChebyshevDistance, searchRadius: static r => r);
internal Func<IReadOnlyList<double>, IReadOnlyList<double>, double> Body { get; }
```

```csharp
KDTree<double, double, int> tree = KDTree.Create(coordinates, payloads, DistanceMetrics.EuclideanDistance);
tree.Metric = row.Body;
return tree;
```

### To

```csharp
public static readonly NeighborMetric Euclidean = new(metric: DistanceMetrics.EuclideanDistance, searchRadius: static r => r * r);
public static readonly NeighborMetric Manhattan = new(metric: DistanceMetrics.ManhattanDistance, searchRadius: static r => r);
public static readonly NeighborMetric Chebyshev = new(metric: DistanceMetrics.ChebyshevDistance, searchRadius: static r => r);
internal DistanceMetrics Metric { get; }
// NeighborMetric.Body DELETED
```

```csharp
KDTree.Create(coordinates, payloads, row.Metric)
```

### Why

`KDTree.Create` accepts `DistanceMetrics` and binds the corresponding metric delegate once. Creating every tree as Euclidean and mutating its public delegate immediately afterward is a second configuration path; the domain row should carry the package factory value directly.

# 3. Collapse point sources into one complete index case

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:78-85`, anchors `NeighborSource.PointsCase` and `NeighborSource.StaticCase`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:133-172`, anchors `NeighborIndex.PointsCase`, `NeighborIndex.StaticCase`, and their `Of` arms
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:221-230`, anchor `NeighborIndex.WithTree`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:401-415`, anchors the `pointsCase` and `staticCase` graph arms

### From

```csharp
public sealed record PointsCase(Seq<Point3d> Values) : NeighborSource;
public sealed record StaticCase(Seq<Point3d> Values) : NeighborSource;
```

```csharp
public sealed record PointsCase(Point3d[] Hay, RTree Tree) : NeighborIndex;
public sealed record StaticCase(FrozenDictionary<NeighborMetric, KDTree<double, double, int>> Trees, Point3d[] Points) : NeighborIndex;
```

```csharp
pointsCase: static (k, p) =>
    from hay in p.Values.TraverseM(v => k.AcceptValue(value: v)).As().Map(static vs => vs.ToArray())
    from tree in Optional(RTree.CreateFromPointArray(points: hay)).ToFin(k.InvalidResult())
    select (NeighborIndex)new PointsCase(Hay: hay, Tree: tree),
```

```csharp
pointsCase: static (s, p) => s.Run(p.Tree),
staticCase: static (s, t) => Optional(RTree.CreateFromPointArray(points: t.Points)).ToFin(s.Key.InvalidResult())
    .Bind(rtree => new Lease<RTree>.Owned(Value: rtree).Use(s.Run)));
```

### To

```csharp
public sealed record PointsCase(Seq<Point3d> Values) : NeighborSource;
// NeighborSource.StaticCase DELETED
```

```csharp
public sealed record PointsCase(
    Point3d[] Points,
    FrozenDictionary<NeighborMetric, KDTree<double, double, int>> Trees) : NeighborIndex;
// NeighborIndex.StaticCase DELETED
```

```csharp
pointsCase: static (k, p) =>
    from points in p.Values.TraverseM(v => k.AcceptValue(v)).As().Map(static vs => vs.ToArray())
    from _ in guard(points.Length > 0, k.InvalidInput()).ToFin()
    let coordinates = points.Select(IReadOnlyList<double> (v) => [v.X, v.Y, v.Z]).ToArray()
    let payloads = Enumerable.Range(0, points.Length).ToArray()
```

```csharp
from trees in k.Catch(() => Fin.Succ(NeighborMetric.Items.ToFrozenDictionary(
    static row => row, row => KDTree.Create(coordinates, payloads, row.Metric))))
select (NeighborIndex)new PointsCase(points, trees),
// NeighborSource.StaticCase DELETED
```

```csharp
pointsCase: static (s, p) => Optional(RTree.CreateFromPointArray(p.Points)).ToFin(s.Key.InvalidResult())
    .Bind(tree => new Lease<RTree>.Owned(tree).Use(s.Run)));
// NeighborIndex.StaticCase DELETED
```

Replace the two graph arms with one `pointsCase` arm selecting `p.Trees[search.Metric]`; box, sphere, and overlap execution builds its native RTree only inside the existing `Lease<RTree>.Owned` query window.

### Why

Both source cases admit the same frozen point identity and expose the same query algebra; only their eagerly selected backend differs. One point owner retains the exact per-metric trees used repeatedly by neighborhood folds and creates an RTree only for the box, sphere, or overlap call that needs it. This deletes one public source case, one public index case, one admission arm, one graph arm, one tree-dispatch arm, and the persistent native-tree field without deleting either capability or leaving native custody attached to a non-disposable union case.

### Ripples

- `libs/dotnet/Rasm/.planning/Parametric/surface.md:220`, `Processing/repair.md:283`, `Processing/sample.md:446,863,999,1302,1418`, and `Solving/fit.md:697`: construct `NeighborSource.PointsCase` instead of `NeighborSource.StaticCase`.
- `libs/dotnet/Rasm/.planning/Solving/fit.md:17,943`: replace the `StaticCase` package and diagram vocabulary with the surviving point case.
- Existing `PointsCase` consumers at `Analysis/query.md:267`, `Parametric/panelize.md:127`, and `Processing/sample.md:1428` keep their construction and gain the complete index.

# 4. Reduce backend evidence to its independent bit

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:35-42`, anchor `NeighborSearchBackend`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:103-119`, anchor `NeighborhoodCensus`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:485-510`, anchors `NeighborKernel.Batch` and `SearchBackend:`

### From

```csharp
[SmartEnum<int>]
public sealed partial class NeighborSearchBackend {
    public static readonly NeighborSearchBackend RTreeKnn = new(key: 0);
    public static readonly NeighborSearchBackend RTreeRadius = new(key: 1);
    public static readonly NeighborSearchBackend KdTreeKnn = new(key: 2);
    public static readonly NeighborSearchBackend KdTreeRadius = new(key: 3);
}
```

```csharp
int InputCount, int QueryCount, int RequestedNeighborCount, NeighborSearchBackend SearchBackend,
Option<double> Radius, Option<int> SelfNeighborCount,
```

```csharp
int hayCount, Func<int, Point3d> hayAt,
NeighborSearchBackend knnBackend, NeighborSearchBackend radiusBackend,
```

### To

```csharp
// NeighborSearchBackend DELETED
```

```csharp
int InputCount, int QueryCount, int RequestedNeighborCount, bool UsesKdTree,
Option<double> Radius, Option<int> SelfNeighborCount,
```

```csharp
int hayCount, Func<int, Point3d> hayAt, bool usesKdTree,
```

```csharp
UsesKdTree: usesKdTree, Radius: radius,
```

Pass `usesKdTree: false` from the cloud arm and `usesKdTree: true` from the unified point arm.

### Why

`Radius` already records k-nearest versus radial execution, so the four-row roster duplicates that query-mode fact. After the point collapse, the source fixes the remaining binary fact: clouds use RTree and point indexes use KDTree. A boolean column is the branch law's required form for a payloadless two-case fact and deletes a type, four rows, their generated surface, and one batch parameter.

# 5. Keep graph bounds admitted end to end

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:103-118`, anchors `NeighborhoodCensus.Radius` and its radius validity clause
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:388-395`, anchor `NeighborKernel.GraphOf` admission
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:485-500`, anchors `Batch` count/radius parameters and `requested`

### From

```csharp
Option<double> Radius, Option<int> SelfNeighborCount,
```

```csharp
internal static Fin<NeighborhoodGraph> GraphOf(
    NeighborIndex index, Point3d[] needles, Option<int> count, Option<double> radius, Op key,
    Option<NeighborMetric> metric = default) =>
```

```csharp
int requested = Math.Min(count.IfNone(hayCount), hayCount);
IEnumerable<int[]> batch = radius.Match(Some: r => radial(r, requested), None: () => knn(requested));
```

### To

```csharp
Option<double> Radius, Option<int> SelfNeighborCount,
```

```csharp
internal static Fin<NeighborhoodGraph> GraphOf(
    NeighborIndex index, Point3d[] needles, Option<Dimension> count, Option<PositiveMagnitude> radius, Op key,
    Option<NeighborMetric> metric = default) =>
    from _ in guard(needles.Length > 0 && (count.IsSome || radius.IsSome), key.InvalidInput()).ToFin()
```

```csharp
int requested = Math.Min(count.Map(static c => c.Value).IfNone(hayCount), hayCount);
IEnumerable<int[]> batch = radius.Match(
    Some: r => radial(r.Value, requested), None: () => knn(requested));
```

Thread `Option<Dimension>` and `Option<PositiveMagnitude>` through the `Switch` state and `Batch`; delete the `NeighborhoodCensus.IsValid` radius-positivity clause because the retained value object is its evidence.

### Why

`GraphOf` is an internal consumer of bounds already owned by Thinktecture values, not their raw boundary. Carrying those values through the switch and batch removes both per-call admission traversals and every repeated primitive positivity/finiteness check. The remaining guard states only the joint invariant that a graph query supplies a count or a radius.

### Ripples

- `libs/dotnet/Rasm/.planning/Parametric/panelize.md:128-129` and `Parametric/surface.md:221-222`: pass an admitted one-neighbor `Dimension` and typed absence.
- `libs/dotnet/Rasm/.planning/Processing/repair.md:284`, `Processing/sample.md:447,864,1030,1303,1429,1443`, and `Solving/fit.md:779,872`: retain already-proved count/radius facts as `Dimension`/`PositiveMagnitude`; where the local is still raw, admit it before `GraphOf` rather than reconstructing primitives in the target.

# 6. Type the nearest-query payload at admission

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:53-68`, anchors `NeighborQuery.NearestCase` and `Nearest`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:181-184`, anchor the nearest query arm
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:210-213`, anchor the nearest pair-probe arm

### From

```csharp
public sealed record NearestCase(int K, NeighborMetric Metric) : NeighborQuery;
```

```csharp
public static Fin<NeighborQuery> Nearest(int k, Option<NeighborMetric> metric = default, Op? key = null) =>
    guard(k > 0, key.OrDefault().InvalidInput()).ToFin()
        .Map(_ => (NeighborQuery)new NearestCase(K: k, Metric: metric.IfNone(NeighborMetric.Euclidean)));
```

### To

```csharp
public sealed record NearestCase(Dimension Count, NeighborMetric Metric) : NeighborQuery;
```

```csharp
public static Fin<NeighborQuery> Nearest(int k, Option<NeighborMetric> metric = default, Op? key = null) =>
    key.OrDefault().AcceptValidated<Dimension>(k)
        .Map(count => (NeighborQuery)new NearestCase(count, metric.IfNone(NeighborMetric.Euclidean)));
```

Use `Some(q.Count)` in the single-anchor arm and `Some(n.Count)` in the pair-probe arm after task 5 types the `GraphOf` bound.

### Why

Nearest and radius carry genuinely different evidence shapes, so their union cases remain distinct. `Dimension` admits nearest cardinality once at the factory and travels unchanged through every graph call. Do not enable `SwitchPartially`: this site routes a `Fin<NeighborhoodGraph>`, while the stack reserves partial dispatch for presence tests because a future query case would otherwise fall silently through `@default`.

# 7. Carry overlap tolerance as its admitted value

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:60`, anchor `NeighborQuery.OverlapsCase`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:201-206`, anchor `overlapsCase`

### From

```csharp
public sealed record OverlapsCase(NeighborIndex Other, double Tolerance) : NeighborQuery;
```

```csharp
from _ in guard(double.IsFinite(q.Tolerance) && q.Tolerance >= 0.0, s.Key.InvalidInput()).ToFin()
from pairs in s.Self.WithTree(key: s.Key, run: mine => q.Other.WithTree(key: s.Key, run: theirs => SearchCapsule<NeighborPair>(
    run: buffer => RTree.SearchOverlaps(treeA: mine, treeB: theirs, tolerance: q.Tolerance,
```

### To

```csharp
public sealed record OverlapsCase(NeighborIndex Other, Tolerance Band) : NeighborQuery;
// NeighborQuery.OverlapsCase.Tolerance DELETED
```

```csharp
from pairs in s.Self.WithTree(key: s.Key, run: mine => q.Other.WithTree(key: s.Key, run: theirs => SearchCapsule<NeighborPair>(
    run: buffer => RTree.SearchOverlaps(treeA: mine, treeB: theirs, tolerance: q.Band.Value,
```

### Why

The analysis boundary already owns an admitted `Tolerance`; projecting it to `double` and revalidating the primitive inside the query discards and reconstructs the same evidence. Carrying the value object removes the duplicate guard and leaves primitive projection at the Rhino call.

### Ripples

- `libs/dotnet/Rasm/.planning/Analysis/query.md:259-262`: pass `Band` directly to `NeighborQuery.OverlapsCase`.

# 8. Use generated factories at the native and proven boundaries

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:191-206`, anchors the `NeighborHit.Validate` and `NeighborPair.Validate` callbacks
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:215-217`, anchor pair materialization from `NeighborhoodGraph`

### From

```csharp
if (NeighborHit.Validate(args.Id, out NeighborHit? hit) is null) { buffer.Add(hit!.Value); }
```

```csharp
if (NeighborPair.Validate(args.Id, args.IdB, out NeighborPair? pair) is null) { buffer.Add(pair!); }
```

```csharp
.SelectMany(static (row, needle) => row.Select(id =>
    NeighborPair.Validate(needle, id, out NeighborPair? pair) is null
        ? Some(pair!) : Option<NeighborPair>.None))
.Somes()
```

### To

```csharp
if (NeighborHit.TryCreate(args.Id, out NeighborHit hit)) { buffer.Add(hit); }
```

```csharp
if (NeighborPair.TryCreate(args.Id, args.IdB, out NeighborPair? pair)) { buffer.Add(pair!); }
```

```csharp
.SelectMany(static (row, needle) => row.Select(id => NeighborPair.Create(needle, id)))
```

### Why

Thinktecture already generates boolean `TryCreate` for callback-shaped admission, so reading the validation error only to compare it with null and then null-forgiving the output is hand-rolled factory logic. Pair materialization from an accepted graph is stronger: the graph census has proved every hit ordinal nonnegative and in range, while `SelectMany` supplies a nonnegative needle ordinal, so the generated `Create` consumes already-proved values without an impossible `Option`/`Somes` filter.

# 9. Delete the unused probe gate

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:70-74`, anchor `NeighborQuery.SearchProbe`

### From

```csharp
internal Fin<(NeighborQuery Query, Point3d Anchor)> SearchProbe(Op key) => this switch {
    BoxCase { Bounds: var bounds } when bounds.IsValid => Fin.Succ((this, bounds.Center)),
    BallCase { Ball: var ball } when ball.IsValid => Fin.Succ((this, ball.Center)),
    _ => Fin.Fail<(NeighborQuery, Point3d)>(key.InvalidInput()),
};
```

### To

```csharp
// NeighborQuery.SearchProbe DELETED
```

### Why

No `libs/dotnet/` consumer calls this member. It duplicates the box and sphere validity gates in the executing arms and adds a second hand-written dispatch over a generated closed union without owning a capability.

# 10. Delete unprovable self-neighbor evidence

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:103-119`, anchors `NeighborhoodCensus.SelfNeighborCount` and `IsValid`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:497-505`, anchor `SelfNeighborCount:`

### From

```csharp
Option<PositiveMagnitude> Radius, Option<int> SelfNeighborCount,
```

```csharp
SelfNeighborCount.Map(count => count >= 0 && count <= QueryCount).IfNone(true),
```

```csharp
SelfNeighborCount: needles.Length == hayCount
    ? Some(ids.Where(static (row, i) => row.Contains(i)).Count())
    : Option<int>.None,
```

### To

```csharp
Option<PositiveMagnitude> Radius,
// NeighborhoodCensus.SelfNeighborCount DELETED
```

```csharp
// NeighborhoodCensus.SelfNeighborCount DELETED
```

```csharp
// NeighborhoodCensus.SelfNeighborCount DELETED
```

### Why

Equal hay and needle counts do not prove that needle ordinal `i` denotes hay ordinal `i`; unrelated same-sized clouds receive false self-neighbor evidence. `Batch` has no identity correspondence from which to derive this fact. Deleting the unprovable field also removes the per-row `Contains` scan; a future genuine self census must enter with explicit correspondence evidence rather than a count heuristic.

# 11. Delete derived neighborhood aliases

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:108`, anchor `NeighborhoodCensus.RadiusLimited`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:419-421`, anchor `NeighborKernel.CensusOf`

### From

```csharp
public bool RadiusLimited => Radius.IsSome;
```

```csharp
internal static Fin<NeighborhoodCensus> CensusOf(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
    GraphOf(index: new NeighborIndex.CloudCase(Source: cluster), needles: [.. cluster.Vertices.AsIterable()], policy: policy, key: key)
        .Map(static graph => graph.Census);
```

### To

```csharp
// NeighborhoodCensus.RadiusLimited DELETED
```

```csharp
// NeighborKernel.CensusOf DELETED
```

### Why

`RadiusLimited` only renames the retained option's presence bit. `CensusOf` has one consumer and only forwards `GraphOf(...).Map(graph => graph.Census)`. Both capabilities remain directly expressible after deleting two module members.

### Ripples

- `libs/dotnet/Rasm/.planning/Spatial/cloud.md:258`: inline the existing `GraphOf(...).Map(static graph => graph.Census)` expression in the `VectorCloudMetric.Neighborhood` row.

# 12. Remove census fields already owned by nested evidence

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:286-296`, anchor `PcaCensus`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:342-361`, anchor `CurvatureCensus`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:466-472`, anchor `new CurvatureCensus`

### From

```csharp
public readonly record struct PcaCensus(
    int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
```

```csharp
int InputCount, int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
int RankRejectedCount, int ResidualRejectedCount, int SolveRejectedCount, Option<Stat<Scalar>> Residuals,
```

```csharp
AcceptedSampleCount: census.Accepted.Count,
RejectedSampleCount: census.Rank + census.Residual + census.Solve,
```

### To

```csharp
// PcaCensus.InputCount DELETED
public readonly record struct PcaCensus(
    int RequestedNeighborCount, int AcceptedSampleCount, int RejectedSampleCount,
```

```csharp
// CurvatureCensus.InputCount DELETED
// CurvatureCensus.AcceptedSampleCount DELETED
// CurvatureCensus.RejectedSampleCount DELETED
int RequestedNeighborCount, int RankRejectedCount, int ResidualRejectedCount, int SolveRejectedCount,
Option<Stat<Scalar>> Residuals,
```

```csharp
// CurvatureCensus.AcceptedSampleCount DELETED
// CurvatureCensus.RejectedSampleCount DELETED
```

Use `Neighborhood.InputCount` in both total-count checks and `Range.AcceptedSampleCount` in the curvature residual presence/count checks.

### Why

Both censuses carry `NeighborhoodCensus`, which already owns the input count. `CurvatureRange` owns the accepted count, and the three typed refusal counts derive the rejected total. Their requested-neighbor fields remain because `NeighborhoodCensus.RequestedNeighborCount` is capped to the hay count while each outer census records the caller's original request. Removing four duplicated fields leaves one authority for each fact and shortens construction and validity without deleting evidence.

# 13. Derive empty and aggregate curvature classification from the tally

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:300-314`, anchors `CurvatureRangeKind.Empty` and `CurvatureRangeKind.Of`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:368-378`, anchor `CurvatureRange.Kind`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:545-562`, anchors the tally fold and `Kind:` construction

### From

```csharp
public static readonly CurvatureRangeKind Empty = new(key: 0, admits: static (_, _) => false);
[UseDelegateFromConstructor] internal partial bool Admits(CurvatureSample sample, double band);
internal static CurvatureRangeKind Of(CurvatureSample sample, double band) =>
    Items.First(row => row.Admits(sample: sample, band: band));
```

```csharp
int AcceptedSampleCount, CurvatureRangeKind Kind, int PlaneLikeCount, int SphereLikeCount,
```

```csharp
Kind: samples.IsEmpty
    ? CurvatureRangeKind.Empty
    : toSeq(CurvatureRangeKind.Items).Find(row => Counted(row) == samples.Count).IfNone(CurvatureRangeKind.Mixed),
```

### To

```csharp
// CurvatureRangeKind.Empty DELETED
[UseDelegateFromConstructor] internal partial bool Admits(CurvatureSample sample, double band);
// CurvatureRangeKind.Of DELETED
```

```csharp
int AcceptedSampleCount, int PlaneLikeCount, int SphereLikeCount,
```

```csharp
public Option<CurvatureRangeKind> Kind => AcceptedSampleCount switch {
    0 => None,
    int n when PlaneLikeCount == n => Some(CurvatureRangeKind.Plane),
    int n when SphereLikeCount == n => Some(CurvatureRangeKind.Sphere),
    int n when SaddleLikeCount == n => Some(CurvatureRangeKind.Saddle),
    _ => Some(CurvatureRangeKind.Mixed),
};
```

Inline `CurvatureRangeKind.Items.First(row => row.Admits(sample, band))` in the sole tally site; delete the `Kind:` constructor argument and its stored-field validity clause.

### Why

`Empty` is not a curvature classification: its delegate rejects every sample and the row exists only as an absence sentinel. The aggregate kind is already fully determined by the accepted count and four classification counts, so storing it creates a second authority that can disagree with the tally. A computed `Option` retains the query capability while deleting the fake row, one helper, and one stored field.

# 14. Collapse the one-site curvature band carrier

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:363-378`, anchors `CurvatureBand` and `CurvatureRange.Bands`
- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:549-554`, anchor `new CurvatureBand`

### From

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureBand(CurvatureAxis Axis, Stat<Scalar> Spread) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Evidence(Some(Spread));
}
```

```csharp
Option<Arr<CurvatureBand>> Bands, double Tolerance) : IValidityEvidence {
```

```csharp
.Map(spread => new CurvatureBand(Axis: axis, Spread: spread)))
.Map(bands => Some(new Arr<CurvatureBand>([.. bands]))))
```

### To

```csharp
// CurvatureBand DELETED
```

```csharp
Option<Arr<(CurvatureAxis Axis, Stat<Scalar> Spread)>> Bands, double Tolerance) : IValidityEvidence {
```

```csharp
.Map(spread => (Axis: axis, Spread: spread)))
.Map(bands => Some(new Arr<(CurvatureAxis Axis, Stat<Scalar> Spread)>([.. bands]))))
```

Update the bands validity fold to read `ValidityClaim.Evidence(Some(band.Spread))` directly.

### Why

`CurvatureBand` is a two-cell pair constructed only inside `RangeOf`, stored only in `CurvatureRange`, and never consumed by nominal type. A named tuple preserves both axis and spread while deleting a public module type, its validity member, its layout attribute, and its constructor surface.

# 15. Collapse curvature projection wrappers onto the axis

### Location

- `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:478-483`, anchors `NeighborKernel.Curvedness`, `ShapeIndex`, and `Projected`

### From

```csharp
internal static Fin<Seq<double>> Curvedness(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
    Projected(axis: CurvatureAxis.Curvedness, cluster: cluster, policy: policy, key: key);
internal static Fin<Seq<double>> ShapeIndex(VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
    Projected(axis: CurvatureAxis.Shape, cluster: cluster, policy: policy, key: key);
private static Fin<Seq<double>> Projected(CurvatureAxis axis, VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
    PrincipalCurvatures(cluster: cluster, policy: policy, key: key).Map(r => r.Samples.Map(axis.Project));
```

### To

```csharp
internal static Fin<Seq<double>> Project(
    CurvatureAxis axis, VectorCloud.ClusterCase cluster, NeighborhoodPolicy policy, Op key) =>
    PrincipalCurvatures(cluster, policy, key).Map(r => r.Samples.Map(axis.Project));
// NeighborKernel.Curvedness DELETED
// NeighborKernel.ShapeIndex DELETED
// NeighborKernel.Projected DELETED
```

### Why

The projection vocabulary already lives on `CurvatureAxis`; the two module methods only hard-code rows before forwarding to the same one-call helper. One parameterized operation replaces three members, retains both projections, and lets every future axis use the same path without adding another wrapper.

### Ripples

- `libs/dotnet/Rasm/.planning/Spatial/cloud.md:255-256`: call `NeighborKernel.Project(CurvatureAxis.Curvedness, ...)` and `NeighborKernel.Project(CurvatureAxis.Shape, ...)`.
