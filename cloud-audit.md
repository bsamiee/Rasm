# 1. Make process-local cloud vocabularies keyless

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:35-45,228-270,395-404` — `CloudDedup`, `VectorCloudMetric`, and `CloudHullKind`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class CloudDedup {
    public static readonly CloudDedup Merge = new(key: 0,
```

```csharp
[SmartEnum<int>]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(key: 0,
```

```csharp
[SmartEnum<int>]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new(key: 0);
```

### To

```csharp
[SmartEnum]
public sealed partial class CloudDedup {
    public static readonly CloudDedup Merge = new(
```

```csharp
[SmartEnum]
public sealed partial class VectorCloudMetric {
    public static readonly VectorCloudMetric Normal = Ring(
```

```csharp
[SmartEnum]
public sealed partial class CloudHullKind {
    public static readonly CloudHullKind Convex3D = new();
```

Remove the remaining `key:` arguments from these rosters and `int key` from the five metric row builders.

### Why

No `libs/dotnet/` consumer reads, persists, parses, converts, or looks up these integer keys. Keyless Thinktecture rows retain item identity, total dispatch, and constructor delegates without inventing process-local wire identity. `CloudHullRejection` and `HullRoute` remain keyed because their keys carry MIConvexHull and capability-set identity.

# 2. Make the native point-cloud index reference-owned

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:26,47-112` — the `RuntimeHelpers` import, `VectorCloud`, its cases, `ClusterCase` copy constructor, `IndexHandle`, and `SharedIndex`.

### From

```csharp
[Union]
public abstract partial record VectorCloud : IDisposable {
```

```csharp
public sealed record RingCase : VectorCloud {
public sealed record PolylineCase : VectorCloud {
public sealed record ClusterCase : VectorCloud {
```

```csharp
private ClusterCase(ClusterCase original) : base(original) {
    Vertices = original.Vertices; Tolerance = original.Tolerance; Mass = original.Mass;
    Index = original.Index.Copy(); Admission = original.Admission;
}
```

```csharp
internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass,
    Lease<PointCloud> Indexed, CloudAdmission Admission) {
    this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass;
    Index = new IndexHandle(lease: Indexed); this.Admission = Admission;
}
private IndexHandle Index { get; }
```

```csharp
private sealed class IndexHandle : IEquatable<IndexHandle> {
    private readonly SharedIndex owner;
```

```csharp
private sealed class SharedIndex(Lease<PointCloud> lease) {
    private int references = 1;
```

```csharp
using System.Runtime.CompilerServices;
```

### To

```csharp
[Union]
public abstract partial class VectorCloud : IDisposable {
```

```csharp
public sealed class RingCase : VectorCloud {
public sealed class PolylineCase : VectorCloud {
public sealed class ClusterCase : VectorCloud {
```

```csharp
private readonly Lock gate = new();
private readonly Lease<PointCloud> index;
private bool disposed;
```

```csharp
internal ClusterCase(Seq<Point3d> Vertices, Context Tolerance, Option<Arr<double>> Mass,
    Lease<PointCloud> Indexed, CloudAdmission Admission) {
    this.Vertices = Vertices; this.Tolerance = Tolerance; this.Mass = Mass;
    index = Indexed; this.Admission = Admission;
}
```

```csharp
internal Fin<T> UseIndex<T>(Op key, Func<PointCloud, Fin<T>> project) {
    lock (gate) return disposed ? Fin.Fail<T>(key.InvalidContext()) : project(index.Resource);
}
```

```csharp
public void Dispose() => Switch(
    ringCase: static _ => { },
    polylineCase: static _ => { },
    clusterCase: static cluster => {
        lock (cluster.gate) {
            if (!cluster.disposed) { cluster.disposed = true; _ = cluster.index.Dispose(); }
        }
    });
```

```csharp
// ClusterCase(ClusterCase original) DELETED
// ClusterCase.Release DELETED
// IndexHandle DELETED
// SharedIndex DELETED
// System.Runtime.CompilerServices import DELETED
```

### Why

Consumers use clouds only by reference, case, generated dispatch, and `IDisposable`; none uses equality, `with`, or cloning. Removing record-copy semantics lets `ClusterCase` own the lease directly. One lock preserves use-versus-dispose exclusion and idempotent disposal while deleting two types, the copy path, its one-call release wrapper, reference counting, equality/hash plumbing, and `RuntimeHelpers`.

# 3. Pass neighborhood policy without a cloud wrapper

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:253-293` — neighborhood rows, `Measure`, both `Project` overloads, and `CloudMetricPolicy`.

### From

```csharp
[UseDelegateFromConstructor]
private partial Fin<object> Measure(VectorCloud cloud, CloudMetricPolicy policy, Op key);
```

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud cloud, Op key) =>
    CloudMetricPolicy.AdmitOrDefault(policy: None, context: cloud.Tolerance, key: key)
        .Bind(policy => Project<TOut>(cloud: cloud, policy: policy, key: key));
```

```csharp
public readonly record struct CloudMetricPolicy(NeighborhoodPolicy Neighborhood) {
    internal static Fin<CloudMetricPolicy> AdmitOrDefault(Option<CloudMetricPolicy> policy, Context context, Op key) =>
        policy.Match(Some: p => p.Neighborhood.Admit(key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)),
                     None: () => NeighborhoodPolicy.Of(context: context, key: key).Map(static n => new CloudMetricPolicy(Neighborhood: n)));
}
```

### To

```csharp
[UseDelegateFromConstructor]
private partial Fin<object> Measure(VectorCloud cloud, NeighborhoodPolicy policy, Op key);
```

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud cloud, NeighborhoodPolicy policy, Op key) =>
```

Change the six neighborhood-backed rows from `p.Neighborhood` to `p`.

```csharp
// VectorCloudMetric.Project<TOut>(VectorCloud, Op) DELETED
// CloudMetricPolicy DELETED
```

### Why

The wrapper adds no invariant, axis, or behavior, and every read immediately unwraps its only field. The no-policy `Project` overload has no consumer; `VectorIntent.Cloud` already resolves policy before dispatch. Passing `NeighborhoodPolicy` directly removes one module-level type, one admission helper, one field hop, and one unused member.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/intent.md:56,120-126,311` — carry `NeighborhoodPolicy`; accept `Option<NeighborhoodPolicy>`; admit `Some` with `Admit` and derive `None` with `NeighborhoodPolicy.Of`; pass it to `Metric.Project`.
- `libs/dotnet/Rasm/.planning/Drawing/pack.md:240-248,597-599` and `libs/dotnet/Rasm/.planning/Processing/decimate.md:531-535` — replace `Option<CloudMetricPolicy>` with `Option<NeighborhoodPolicy>`.

# 4. Collapse duplicate metric row builders

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:245-270` — `TangentFlow` through `OpenLength`, `Poly`, and both `Cluster` builders.

### From

```csharp
TangentFlow = Poly(key: 16, measure: static (pts, k) => CloudKernel.TangentFlowOf(points: pts, key: k)),
CumulativeArcLength = Poly(key: 17, measure: static (pts, k) => CloudKernel.CumulativeArcLengthOf(points: pts, key: k)),
```

```csharp
private static VectorCloudMetric Poly<TValue>(int key, Func<Seq<Point3d>, Op, Fin<TValue>> measure);
private static VectorCloudMetric Cluster<TValue>(int key, Func<VectorCloud.ClusterCase, Op, Fin<TValue>> measure);
private static VectorCloudMetric Cluster<TValue>(int key, Func<VectorCloud.ClusterCase, CloudMetricPolicy, Op, Fin<TValue>> measure);
```

### To

```csharp
TangentFlow = Chain(measure: static (cloud, k) => CloudKernel.TangentFlowOf(points: cloud.Vertices, key: k)),
CumulativeArcLength = Chain(measure: static (cloud, k) => CloudKernel.CumulativeArcLengthOf(points: cloud.Vertices, key: k)),
```

Apply `Chain` to `EdgeCurvatures` and `OpenLength`; give policy-independent cluster rows the three-argument builder with `_` for policy.

```csharp
// VectorCloudMetric.Poly<TValue> DELETED
// VectorCloudMetric.Cluster<TValue>(Func<VectorCloud.ClusterCase, Op, Fin<TValue>>) DELETED
```

### Why

`Poly` and `Chain` admit the same ring and polyline cases; `Poly` only forwards `Vertices`. The cluster overloads differ only by whether the row ignores resolved policy. Reusing the broader delegates deletes two private members and their adapters without duplicating case logic.

# 5. Return principal statistics as a named tuple

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:250-252,341-351` — `Spread`, `PrincipalStats`, and `PrincipalStatsOf`.

### From

```csharp
Spread = Cluster(key: 22, measure: static (c, k) =>
    CloudKernel.PrincipalStatsOf(cluster: c, key: k).Bind(s => k.AcceptValue(value: s.Spread))),
```

```csharp
internal sealed record PrincipalStats(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen) {
    internal Seq<(double Moment, Vector3d Axis)> Axes => Eigen.Map(static p => (Moment: p.Eigenvalue, Axis: AsVector3d(v: p.Eigenvector)));
    internal Vector3d Spread => new(Eigen[0].Eigenvalue, Eigen[1].Eigenvalue, Eigen[2].Eigenvalue);
}
```

```csharp
internal static Fin<PrincipalStats> PrincipalStatsOf(VectorCloud.ClusterCase cluster, Op key) =>
```

### To

```csharp
Spread = Cluster(measure: static (c, _, k) => CloudKernel.PrincipalStatsOf(cluster: c, key: k)
    .Bind(s => k.AcceptValue(value: new Vector3d(s.Eigen[0].Eigenvalue, s.Eigen[1].Eigenvalue, s.Eigen[2].Eigenvalue)))),
```

```csharp
internal static Fin<(Vector3d Mean, Seq<(double Eigenvalue, Arr<double> Eigenvector)> Eigen)>
    PrincipalStatsOf(VectorCloud.ClusterCase cluster, Op key) =>
```

```csharp
from full in eigen.Count >= 3
    ? Fin.Succ((Mean: stats.Mean, Eigen: eigen))
    : Fin.Fail<(Vector3d, Seq<(double, Arr<double>)>)>(key.InvalidResult())
```

```csharp
// PrincipalStats DELETED
```

### Why

`PrincipalStats` has no identity or invariant beyond the pair the fold computes. Its `Axes` projection is unread and its `Spread` projection has one call site. A named tuple removes one nested type and two members while retaining the mean and complete eigenbasis for the other PCA folds.

# 6. Carry mass evidence as one optional state

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:190-213` — `CloudAdmission` mass columns, helper members, and validity arm.

### From

```csharp
Option<double> MassInputTotal, Option<double> MassMergedTotal, Option<double> MassOutputTotal) : IValidityEvidence {
    internal static bool MassConserved(double input, double output, double tolerance) =>
        Math.Abs(input - output) <= tolerance * Math.Max(1.0, Math.Abs(input));
    internal static bool MassNormalized(double output, double tolerance) =>
        Math.Abs(1.0 - output) <= tolerance;
    internal static bool MassAdmitted(double total) => double.IsFinite(total) && total >= 0.0;
```

```csharp
(MassInputTotal.Case, MassMergedTotal.Case, MassOutputTotal.Case) switch {
    (double input, double merged, double output) =>
```

### To

```csharp
Option<(double Input, double Merged, double Output)> Mass) : IValidityEvidence {
```

```csharp
Mass.Match(
    Some: totals => ValidityClaim.All(
        ValidityClaim.Nonnegative(totals.Input),
        ValidityClaim.Nonnegative(totals.Merged),
        ValidityClaim.Nonnegative(totals.Output),
        Math.Abs(totals.Input - totals.Merged) <= ConservationTolerance.Value * Math.Max(1.0, Math.Abs(totals.Input)),
        Math.Abs(1.0 - totals.Output) <= ConservationTolerance.Value),
    None: static () => true));
```

```csharp
// CloudAdmission.MassConserved DELETED
// CloudAdmission.MassNormalized DELETED
// CloudAdmission.MassAdmitted DELETED
```

### Why

The totals are one all-or-none fact, but three `Option` fields permit six partial states. One optional tuple makes presence structural, deletes two fields and three one-site helpers, and states conservation once. `AdmitCluster` constructs the tuple in this target; no outside consumer reads an individual total.

# 7. Derive merged count from the admission cardinalities

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:190-205` — `CloudAdmission.MergedCoordinateCount` and its cardinality claims.

### From

```csharp
int InputCount, int OutputCount, int InputDuplicateCoordinateCount, int MergedCoordinateCount,
```

```csharp
InputDuplicateCoordinateCount >= 0 && MergedCoordinateCount >= 0,
ValidityClaim.CountExactly(count: OutputCount + MergedCoordinateCount, expected: InputCount),
MergedCoordinateCount == 0 || Dedup.Equals(CloudDedup.Merge),
```

### To

```csharp
int InputCount, int OutputCount, int InputDuplicateCoordinateCount,
```

```csharp
InputDuplicateCoordinateCount >= 0 && OutputCount <= InputCount,
OutputCount == InputCount || Dedup.Equals(CloudDedup.Merge),
```

### Why

`MergedCoordinateCount` is exactly `InputCount - OutputCount`; storing it creates a second authority for a value the cardinalities already determine. Removing the field and equality claim lowers result surface while preserving exact-duplicate evidence as the genuinely distinct count.

# 8. Let rejection presence carry hull completion state

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:406-410,477-497` — `CloudFoldStatus` and `CloudHullResult`.

### From

```csharp
[SmartEnum<int>]
public sealed partial class CloudFoldStatus {
    public static readonly CloudFoldStatus Completed = new(key: 0);
    public static readonly CloudFoldStatus Rejected = new(key: 1);
}
```

```csharp
CloudHullKind Kind, CloudFoldStatus Status, PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
```

```csharp
Rejection.IsNone || Status.Equals(CloudFoldStatus.Rejected),
!Status.Equals(CloudFoldStatus.Completed) || Route.Admits(capability: HullRoute.Native),
!Status.Equals(CloudFoldStatus.Completed) || OutputVertexCount >= 3,
Mesh.IsSome || Solid.IsSome || Status.Equals(CloudFoldStatus.Rejected));
```

### To

```csharp
// CloudFoldStatus DELETED
```

```csharp
CloudHullKind Kind, PositiveMagnitude Tolerance, VectorAngle AngleTolerance,
```

```csharp
Rejection.IsSome || Route.Admits(capability: HullRoute.Native),
Rejection.IsSome || OutputVertexCount >= 3,
Mesh.IsSome || Solid.IsSome || Rejection.IsSome);
```

### Why

`Status` is a second payload-free spelling of `Rejection.IsSome`, and no consumer reads it. Substituting that presence test preserves rejected fallback geometry as well as geometry-free refusal while deleting one module-level type and one field.

# 9. Use route evidence instead of a fallback-only hull kind

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:395-404` — `CloudHullKind.FootprintWrapper`.

### From

```csharp
public static readonly CloudHullKind AlphaShape = new(key: 3);
public static readonly CloudHullKind FootprintWrapper = new(key: 4);
public static readonly CloudHullKind Faceted3D = new(key: 5);
```

### To

```csharp
public static readonly CloudHullKind AlphaShape = new();
public static readonly CloudHullKind Faceted3D = new();
// CloudHullKind.FootprintWrapper DELETED
```

Return `CloudHullKind.ConvexFootprint2D` from the existing 3D-to-2D fallback arm and retain `HullRoute.Fallback` on that result.

### Why

`FootprintWrapper` executes the same fitted-plane hull and emits the same geometry as `ConvexFootprint2D`; only the route differs. `HullRoute.Fallback` already carries that provenance, so a second kind row makes callers distinguish identical products by execution history. Reusing the geometric kind removes one row and one duplicate dispatch arm without losing the fallback fact.

# 10. Use one indexed vertex carrier in both hull dimensions

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:513-525` — `CloudVertex` and `CloudPlanarVertex`.

### From

```csharp
internal sealed class CloudVertex(int index, double[] position) : IVertex {
    public int Index { get; } = index;
    public double[] Position { get; } = position;
}
```

```csharp
internal sealed class CloudPlanarVertex : IVertex2D {
    internal int Index { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
}
```

### To

```csharp
internal sealed class CloudVertex : IVertex, IVertex2D {
    public CloudVertex() : this(index: 0, x: 0.0, y: 0.0) { }
    internal CloudVertex(int index, double x, double y, double z = 0.0) {
        Index = index; Position = [x, y, z];
    }
    public int Index { get; }
    public double[] Position { get; }
    public double X => Position[0];
    public double Y => Position[1];
}
```

Construct 3D rows with `(index, point.X, point.Y, point.Z)` and planar rows with `(index, x, y)`.

```csharp
// CloudPlanarVertex DELETED
```

### Why

Both package contracts carry the same identity and coordinates; the planar carrier is the first two coordinates of the N-dimensional carrier. MIConvexHull preserves caller vertex instances and imposes `new()` only on the `Create2D` declaration, so the parameterless constructor satisfies the bound. This removes one module-level class and repeated storage while retaining the genuine cluster index.

# 11. Use MIConvexHull topology carriers directly

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:23-32,514-529,542-549,597,659-666,674-707,727-742` — imports, `CloudFace`, `CloudCell`, `CloudVoronoiEdge`, and MIConvexHull generic closures.

### From

```csharp
internal sealed class CloudFace : ConvexFace<CloudVertex, CloudFace>;
internal sealed class CloudCell : TriangulationCell<CloudVertex, CloudCell> {
    internal bool Boundary => Array.Exists(array: Adjacency, match: static face => face is null);
}
internal sealed class CloudVoronoiEdge : VoronoiEdge<CloudVertex, CloudCell>;
```

```csharp
ConvexHullCreationResult<CloudVertex, CloudFace> hull =
    ConvexHull.Create<CloudVertex, CloudFace>(
        data: [.. points.Select(static (p, i) => new CloudVertex(index: i, position: [p.X, p.Y, p.Z]))],
        tolerance: tolerance);
```

```csharp
VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(
    data: sites, PlaneDistanceTolerance: tolerance)
```

### To

```csharp
using CloudCell = MIConvexHull.DefaultTriangulationCell<Rasm.Spatial.CloudVertex>;
using CloudEdge = MIConvexHull.VoronoiEdge<Rasm.Spatial.CloudVertex, MIConvexHull.DefaultTriangulationCell<Rasm.Spatial.CloudVertex>>;
using CloudFace = MIConvexHull.DefaultConvexFace<Rasm.Spatial.CloudVertex>;
```

```csharp
ConvexHullCreationResult<CloudVertex, CloudFace> hull =
    ConvexHull.Create<CloudVertex>(
        data: [.. points.Select(static (p, i) => new CloudVertex(index: i, x: p.X, y: p.Y, z: p.Z))],
        tolerance: tolerance);
```

```csharp
VoronoiMesh.Create<CloudVertex, CloudCell>(
    data: sites, PlaneDistanceTolerance: tolerance)
```

Replace the planned `CloudCell.Boundary` read with `Array.Exists(cell.Adjacency, static face => face is null)` and close Voronoi signatures over `CloudEdge`.

```csharp
// CloudFace DELETED
// CloudCell DELETED
// CloudVoronoiEdge DELETED
```

### Why

The package defaults expose the same `Vertices`, `Adjacency`, `Normal`, `Source`, and `Target` surfaces. These subclasses add no payload; `CloudCell` adds only a one-read predicate. File aliases preserve readable signatures without runtime types, deleting three module-level classes while `CloudVertex` retains the index payload the defaults cannot supply.

### Ripples

- `libs/dotnet/Rasm/.api/api-miconvexhull.md:81` — replace the `CloudVertex`/`CloudPlanarVertex` split and custom `CloudFace`/`CloudCell`/`CloudVoronoiEdge` ownership claim with the unified `CloudVertex` plus the package default face, cell, and edge carriers.

# 12. Inline the one-call solid accumulator

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:542-572` — `CloudKernel.SolidOf` and private `Accumulate`.

### From

```csharp
return CloudHullRejection.Of(outcome: hull.Outcome, key: key).Bind(rejection => rejection.IsSome
    ? Fin.Succ(Option<CloudSolid>.None)
    : Accumulate(anchor: points.Aggregate(Point3d.Origin, static (sum, p) => sum + p) / points.Length,
        faces: toSeq(hull.Result.Faces), tolerance: tolerance, key: key));
```

```csharp
private static Fin<Option<CloudSolid>> Accumulate(
    Point3d anchor, Seq<CloudFace> faces, double tolerance, Op key) {
```

### To

```csharp
return CloudHullRejection.Of(outcome: hull.Outcome, key: key).Bind(rejection => {
    if (rejection.IsSome) return Fin.Succ(Option<CloudSolid>.None);
    Point3d anchor = points.Aggregate(Point3d.Origin, static (sum, p) => sum + p) / points.Length;
    Seq<CloudFace> faces = toSeq(hull.Result.Faces);
    (double volume, Vector3d moment) = faces.Fold(
        (Volume: 0.0, Moment: Vector3d.Zero), (acc, face) => {
            (Vector3d u, Vector3d v, Vector3d w) = (PointOf(face.Vertices[0]) - anchor,
                PointOf(face.Vertices[1]) - anchor, PointOf(face.Vertices[2]) - anchor);
            double tet = Math.Abs(Vector3d.CrossProduct(a: v - u, b: w - u) * u) / 6.0;
            return (acc.Volume + tet, acc.Moment + (tet * 0.25 * (u + v + w)));
        });
    if (volume <= tolerance * tolerance * tolerance) return Fin.Succ(Option<CloudSolid>.None);
```

Continue with the existing tetrahedral-volume, facet, and validity statements in the same lambda.

```csharp
// CloudKernel.Accumulate DELETED
```

### Why

`Accumulate` has exactly one caller and only continues `SolidOf` after the successful hull outcome. Moving its body into that branch removes one private module member and four forwarded arguments while leaving the volume, facet, and validity folds unchanged.

# 13. Pass measured Voronoi spheres without a wrapper

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:531-536,674-680,705-707` — `CellSpheres`, the measured dictionary in `CensusOf`, and `CellRows`.

### From

```csharp
internal readonly record struct CellSpheres(FrozenDictionary<CloudCell, (Point3d Center, double Radius)> Measured) {
    internal Option<Line> Dual(CloudVoronoiEdge edge) =>
        from tail in Measured.TryGetValue(key: edge.Source, value: out (Point3d Center, double Radius) from) ? Some(from.Center) : Option<Point3d>.None
        from head in Measured.TryGetValue(key: edge.Target, value: out (Point3d Center, double Radius) to) ? Some(to.Center) : Option<Point3d>.None
        select new Line(from: tail, to: head);
}
```

```csharp
CellSpheres spheres = new(Measured: cells
    .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: tolerance)))
```

### To

```csharp
FrozenDictionary<CloudCell, (Point3d Center, double Radius)> spheres = cells
    .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: tolerance)))
    .Where(static row => row.Sphere.IsSome)
    .ToFrozenDictionary(static row => row.Cell,
        static row => row.Sphere.IfNone(default((Point3d Center, double Radius))));
```

Change `CellRows` to receive that dictionary. Inline its dual-edge projection:

```csharp
from tail in spheres.TryGetValue(edge.Source, out (Point3d Center, double Radius) source) ? Some(source.Center) : Option<Point3d>.None
from head in spheres.TryGetValue(edge.Target, out (Point3d Center, double Radius) target) ? Some(target.Center) : Option<Point3d>.None
select new Line(from: tail, to: head)
```

```csharp
// CellSpheres DELETED
```

### Why

`CellSpheres` contains only a dictionary and one forwarding projection. Passing the dictionary directly removes one module-level record and one member while retaining centers and radii for `CellRows`. Explicit tuple types keep the proposal within the no-`var` law.

# 14. Give Voronoi only its admitted tolerance

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:659-667` — `CloudKernel.ComputeVoronoiDetailed`.

### From

```csharp
internal static Fin<CloudVoronoiResult> ComputeVoronoiDetailed(
    VectorCloud.ClusterCase cluster, CloudHullPolicy policy, Op key) =>
```

```csharp
complex: VoronoiMesh.Create<CloudVertex, CloudCell, CloudVoronoiEdge>(
    data: sites, PlaneDistanceTolerance: policy.Tolerance.Value)))
```

### To

```csharp
internal static Fin<CloudVoronoiResult> ComputeVoronoiDetailed(
    VectorCloud.ClusterCase cluster, PositiveMagnitude tolerance, Op key) =>
```

```csharp
complex: VoronoiMesh.Create<CloudVertex, CloudCell>(
    data: sites, PlaneDistanceTolerance: tolerance.Value)))
```

Pass `tolerance.Value` to `CensusOf`.

### Why

Voronoi reads only `CloudHullPolicy.Tolerance`; angle, alpha, and lambda describe hull algorithms this fold cannot execute. Passing the exact admitted lever removes false coupling and prevents unrelated hull settings from appearing meaningful.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/intent.md:73,211-218,373-375` — make `VoronoiCase` carry `PositiveMagnitude`; accept `Option<PositiveMagnitude>`; admit supplied `.Value` with `AcceptValidated<PositiveMagnitude>` or derive absence from `ToleranceLane.Deviation`; pass the magnitude to `ComputeVoronoiDetailed`.

# 15. Put bounded-cell measures on the bounded case

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:590-615,686-695` — `CloudCellBound`, `CloudVoronoiCell`, and the census fold over bound and volume columns.

### From

```csharp
[SmartEnum<int>]
public sealed partial class CloudCellBound {
    public static readonly CloudCellBound Bounded = new(key: 0);
    public static readonly CloudCellBound Unbounded = new(key: 1);
    public static readonly CloudCellBound Degenerate = new(key: 2);
}
```

```csharp
int Site, Point3d Seed, CloudCellBound Bound, Arr<int> Vertices, Arr<int> Neighbors,
Option<double> Volume, Option<Point3d> Centroid, Option<double> Extent)
```

### To

```csharp
public readonly partial record struct CloudVoronoiCell(
    int Site, Point3d Seed, Bound Measure, Arr<int> Vertices, Arr<int> Neighbors) : IValidityEvidence {
    [Union]
    public abstract partial record Bound {
        private Bound() { }
        public sealed record Bounded(double Volume, Point3d Centroid, double Extent) : Bound;
        public sealed record Unbounded : Bound;
        public sealed record Degenerate : Bound;
    }
```

Replace the option-presence validity branch with exhaustive generated `Measure.Switch`, and let census counts and volume totals project from the three cases.

```csharp
// CloudCellBound DELETED
// CloudVoronoiCell.Volume DELETED
// CloudVoronoiCell.Centroid DELETED
// CloudVoronoiCell.Extent DELETED
```

### Why

Boundedness decides the payload shape: only a bounded cell can carry volume, centroid, and extent. The smart enum plus three independent options admits impossible partial measurements and makes every reader repeat the relation. A nested union removes one module-level vocabulary and three nullable columns while generated exhaustive dispatch preserves all three genuine outcomes.

### Ripples

- `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/partition.md:23-25,417-424` — update the described crossing and map `CloudVoronoiCell.Bound.Bounded` to present `PartitionSolid` measures, with `Unbounded` and `Degenerate` mapping to absent measures through generated `Switch`.

# 16. Retain only baseline volumes and vertices for natural neighbors

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:669-672,727-750` — `CloudKernel.VolumeLoss` and `NaturalNeighborField`.

### From

```csharp
public sealed record NaturalNeighborField(CloudVoronoiResult Base, Arr<Point3d> Sites, double Tolerance) {
```

```csharp
internal static Option<(int Site, double Loss)> VolumeLoss(
    CloudVoronoiResult first, CloudVoronoiResult second, int site) =>
    from was in first.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
    from now in second.Cells.Find(cell => cell.Site == site).Bind(static cell => cell.Volume)
    select (Site: site, Loss: was - now);
```

```csharp
CloudVertex[] after = [
    .. self.Sites.AsIterable().Select(static (p, i) =>
        new CloudVertex(index: i, position: [p.X, p.Y, p.Z])),
    new CloudVertex(index: self.Sites.Count, position: [query.X, query.Y, query.Z])];
```

### To

```csharp
public sealed class NaturalNeighborField {
    private readonly FrozenDictionary<int, double> volumes;
    private readonly CloudVertex[] sites;
    private readonly PositiveMagnitude tolerance;
    private NaturalNeighborField(FrozenDictionary<int, double> volumes,
        CloudVertex[] sites, PositiveMagnitude tolerance) {
        this.volumes = volumes; this.sites = sites; this.tolerance = tolerance;
    }
```

```csharp
let volumes = dual.Cells.Bind(cell => cell.Measure.Switch(
    bounded: measured => Seq((Site: cell.Site, Volume: measured.Volume)),
    unbounded: static _ => Seq<(int Site, double Volume)>(),
    degenerate: static _ => Seq<(int Site, double Volume)>()))
    .ToFrozenDictionary(static row => row.Site, static row => row.Volume)
select new NaturalNeighborField(volumes: volumes, sites: seeds, tolerance: tolerance)
```

```csharp
CloudVertex[] after = [
    .. sites,
    new CloudVertex(index: sites.Length, x: query.X, y: query.Y, z: query.Z)];
```

```csharp
from _support in cell.Measure.Switch(
    bounded: static _ => Fin.Succ(unit),
    unbounded: _ => Fin.Fail<Unit>(key.InvalidInput()),
    degenerate: _ => Fin.Fail<Unit>(key.InvalidInput()))
from losses in toSeq(cell.Neighbors.AsIterable().Filter(site => site != sites.Length))
    .TraverseM(site =>
        (from was in volumes.TryGetValue(site, out double volume) ? Some(volume) : Option<double>.None
         from now in inserted.Cells.Find(row => row.Site == site).Bind(row => row.Measure.Switch(
             bounded: static measured => Some(measured.Volume),
             unbounded: static _ => Option<double>.None,
             degenerate: static _ => Option<double>.None))
         select (Site: site, Loss: was - now)).ToFin(key.InvalidInput())).As()
```

Use `PositiveMagnitude tolerance` in `Of`; use `.Value` only at `VoronoiMesh.Create` and `CensusOf`.

```csharp
// CloudKernel.VolumeLoss DELETED
```

### Why

The public positional record and `with` surface can split state that must be minted together. `Weights` reads only baseline volumes, yet the record retains the full dual and rebuilds every vertex per query. A private-constructor class retains admitted vertices and a site-keyed volume table, deleting three public properties, copy/equality surface, the retained graph, repeated reconstruction, and one helper without losing the Sibson fold.

### Ripples

- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:408-413` — pass `c.Policy.Tolerance.IfNone(noneValue: PositiveMagnitude.Create(value: context.Absolute.Value))` directly to `NaturalNeighborField.Of` instead of projecting `.Value`.

# 17. Delete the unread measured-vertex projection

### Location

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:617-634` — `CloudVoronoiCensus.MeasuredVertexCount`.

### From

```csharp
internal int MeasuredVertexCount => DualVertexCount - UnmeasuredVertexCount;
```

### To

```csharp
// CloudVoronoiCensus.MeasuredVertexCount DELETED
```

### Why

No target or `libs/dotnet/` consumer reads this derived convenience member. Both source counts remain on the census, so the subtraction is still available if a future consumer needs it. Removing the unused projection reduces member surface without discarding evidence.
