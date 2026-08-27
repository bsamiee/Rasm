# 1. Remove the unconstructed stored-intent machine case

## From

`libs/dotnet/Rasm.Fabrication/.planning/Kinematics/machine.md:29-43`

```csharp
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.RootFinding;
using NodaTime;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rasm.Processing;
using Rasm.Solving;
using Rhino.Geometry;
using Thinktecture;
```

`libs/dotnet/Rasm.Fabrication/.planning/Kinematics/machine.md:315-348`

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToolAxisDemand : IValidityEvidence {
    private ToolAxisDemand() { }

    public sealed record Fixed(Vector3d Direction) : ToolAxisDemand;
    public sealed record Intent(Seq<VectorIntent> Rows, Context Context, Op Key) : ToolAxisDemand;
    public sealed record Cone(VectorCone Domain, Vector3d Preferred, Context Context, Op Key) : ToolAxisDemand;
    public sealed record Indexed(Arr<Vector3d> Directions) : ToolAxisDemand;

    public bool IsValid => Switch(
        fixedCase: static row => ValidityClaim.All(ValidityClaim.Finite(row.Direction), !row.Direction.IsTiny()),
        intent: static row => row.Context is not null && ValidityClaim.CountAtLeast(row.Rows.Count, 1),
        cone: static row => row.Context is not null && ValidityClaim.All(
            row.Domain.Axis.IsValid,
            ValidityClaim.Finite(row.Preferred),
            !row.Preferred.IsTiny()),
        indexed: static row => ValidityClaim.All(
            ValidityClaim.CountAtLeast(row.Directions.Count, 1),
            row.Directions.ForAll(static direction => ValidityClaim.Finite(direction) && !direction.IsTiny())));

    internal Option<Context> Projection => Switch(
        fixedCase: static _ => Option<Context>.None,
        intent: static row => Some(row.Context),
        cone: static row => Some(row.Context),
        indexed: static _ => Option<Context>.None);

    internal Fin<Vector3d> AxisAt(int index, Plane toolFrame, int coneSamples) => Switch(
        state: (Index: index, Frame: toolFrame, Samples: coneSamples),
        fixedCase: static (_, row) => Fin.Succ(row.Direction),
        intent: static (state, row) => row.Rows.Count == 1 || row.Rows.Count > state.Index
            ? row.Rows[Math.Min(state.Index, row.Rows.Count - 1)]
                .Project<Plane>(row.Context, row.Key)
                .Map(static frame => frame.ZAxis)
            : Fin.Fail<Vector3d>(new KernelFault.InvalidValue("machine", "machine-tool:intent-census")),
```

## To

```csharp
// using Rasm.Processing DELETED
// ToolAxisDemand.Intent DELETED
// ToolAxisDemand.IsValid intent arm DELETED
// ToolAxisDemand.Projection intent arm DELETED
// ToolAxisDemand.AxisAt intent arm DELETED
```

## Why

`ToolAxisDemand.Intent` has no construction site. It stores a heterogeneous relay only to project each row to `Plane`, discard everything except `ZAxis`, and reproduce the axis sequence already represented by `Indexed`. The case adds a second schedule representation and three dispatch obligations without a distinct machine capability.

## Change

Delete the `Intent` case and its three generated `Switch` arms. Remove the now-unused `Rasm.Processing` import. Keep `Fixed`, `Cone`, and `Indexed` unchanged.

## Delta

Code-fence LOC: -9. Module surface: -1 nested type and -4 members (one constructor and three positional properties), +0 types and +0 members; net -1 type and -4 members.

## Ripples

- `libs/dotnet/Rasm.Fabrication/.planning/Kinematics/machine.md`: remove the `Intent` ownership, context-equality, growth, and package claims; no producer migration exists.

# 2. Move cloud admission onto the cloud owners

## From

`libs/dotnet/Rasm/.planning/Processing/intent.md:120-129`

```csharp
public static Fin<VectorIntent> Cloud(VectorCloud cloud, VectorCloudMetric metric, Option<NeighborhoodPolicy> policy = default, Op? key = null) {
    Op op = key.OrDefault();
    return from validCloud in Admit.NotNull(value: cloud, key: op)
           from validMetric in Admit.NotNull(value: metric, key: op)
           from validPolicy in policy.Match(
               Some: candidate => candidate.Admit(key: op),
               None: () => NeighborhoodPolicy.Of(context: validCloud.Tolerance, key: op))
           from _ in guard(validMetric.AdmitsCase(cloud: validCloud), op.Unsupported(inputType: validCloud.GetType(), outputType: validMetric.Output))
           select (VectorIntent)new CloudCase(value: validCloud, metric: validMetric, policy: validPolicy);
}
```

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:241-251`

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud cloud, NeighborhoodPolicy policy, Op key) =>
    (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
        (false, _) => Fin.Fail<TOut>(error: key.Unsupported(inputType: cloud.GetType(), outputType: typeof(TOut))),
        (_, false) => Fin.Fail<TOut>(error: key.Unsupported(inputType: typeof(VectorCloudMetric), outputType: typeof(TOut))),
        _ => Measure(cloud: cloud, policy: policy, key: key).Bind(value => value switch {
            Seq<Vector3d> vs => ResultProjection.Values<Vector3d, TOut>(values: vs, key: key, owner: typeof(VectorCloudMetric)),
            Seq<double> ds => ResultProjection.Values<double, TOut>(values: ds, key: key, owner: typeof(VectorCloudMetric)),
            Seq<Plane> ps => ResultProjection.Values<Plane, TOut>(values: ps, key: key, owner: typeof(VectorCloudMetric)),
            _ => key.AcceptValue(value: value).Map(static v => (TOut)v),
        }),
    };
```

`libs/dotnet/Rasm/.planning/Processing/intent.md:213-222`

```csharp
public static Fin<VectorIntent> Voronoi(VectorCloud source, Option<PositiveMagnitude> tolerance = default, Op? key = null) {
    Op op = key.OrDefault();
    return from validSource in Admit.NotNull(value: source, key: op)
           from cluster in validSource is VectorCloud.ClusterCase c
               ? Fin.Succ(c)
               : Fin.Fail<VectorCloud.ClusterCase>(op.Unsupported(inputType: validSource.GetType(), outputType: typeof(CloudVoronoiResult)))
           from validTolerance in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance.Match(
               Some: static supplied => supplied.Value,
               None: () => cluster.Tolerance.For(lane: ToleranceLane.Deviation).Value))
           select (VectorIntent)new VoronoiCase(source: cluster, tolerance: validTolerance);
}
```

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:598-617`

```csharp
internal static Fin<CloudVoronoiResult> ComputeVoronoiDetailed(
    VectorCloud.ClusterCase cluster, PositiveMagnitude tolerance, Op key) =>
    from _ in guard(cluster.Vertices.Count >= 4, key.InvalidInput()).ToFin()
    let sites = cluster.Vertices.Map(static (p, i) => new CloudVertex(index: i, x: p.X, y: p.Y, z: p.Z)).ToArray()
    from result in key.Catch(() => CensusOf(
            sites: sites, tolerance: tolerance.Value, key: key,
            complex: VoronoiMesh.Create<CloudVertex, CloudCell>(
                data: sites, PlaneDistanceTolerance: tolerance.Value)))
    select result;

internal static Fin<CloudVoronoiResult> CensusOf(
    CloudVertex[] sites, VoronoiMesh<CloudVertex, CloudCell, CloudEdge> complex, double tolerance, Op key) {
    CloudCell[] cells = [.. complex.Vertices];
    FrozenDictionary<CloudCell, (Point3d Center, double Radius)> spheres = cells
        .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: tolerance)))
        .Where(static row => row.Sphere.IsSome)
        .ToFrozenDictionary(static row => row.Cell,
            static row => row.Sphere.IfNone(default((Point3d Center, double Radius))));
```

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:617-631`

```csharp
return from admitted in key.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
       from hull in SolidOf(points: [.. sites.Select(PointOf)], tolerance: tolerance, key: key)
       let open = hull.Map(static solid => solid.Facets.Fold(Set<int>(),
           static (acc, facet) => facet.Vertices.Fold(acc, static (set, corner) => set.Add(corner)))).IfNone(Set<int>())
       from rows in CellRows(cells: cells, complex: complex, spheres: spheres, open: open, sites: sites, tolerance: tolerance, key: key)
       let tally = rows.Cells.Fold((Bounded: 0, Unbounded: 0, Degenerate: 0, Volume: 0.0), static (acc, cell) => cell.Measure.Switch(
           state: acc,
           bounded: static (t, measured) => (t.Bounded + 1, t.Unbounded, t.Degenerate, t.Volume + measured.Volume),
           unbounded: static (t, _) => (t.Bounded, t.Unbounded + 1, t.Degenerate, t.Volume),
           degenerate: static (t, _) => (t.Bounded, t.Unbounded, t.Degenerate + 1, t.Volume)))
       let census = new CloudVoronoiCensus(
           PlaneDistanceTolerance: admitted, InputCount: sites.Length, DualVertexCount: cells.Length, UnmeasuredVertexCount: cells.Length - rows.Vertices.Count,
```

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:672-676`

```csharp
internal static Fin<NaturalNeighborField> Of(Seq<Point3d> sites, PositiveMagnitude tolerance, Op key) {
    CloudVertex[] seeds = sites.Map(static (p, i) => new CloudVertex(index: i, x: p.X, y: p.Y, z: p.Z)).ToArray();
    return from _ in guard(sites.Count >= 4, key.InvalidInput()).ToFin()
           from dual in key.Catch(() => CloudKernel.CensusOf(sites: seeds, tolerance: tolerance.Value, key: key,
               complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: seeds, PlaneDistanceTolerance: tolerance.Value)))
```

`libs/dotnet/Rasm/.planning/Spatial/cloud.md:684-690`

```csharp
internal Fin<Seq<(int Site, double Weight)>> Weights(Point3d query, Op key) {
    CloudVertex[] after = [
        .. sites,
        new CloudVertex(index: sites.Length, x: query.X, y: query.Y, z: query.Z)];
    return from inserted in key.Catch(() => CloudKernel.CensusOf(sites: after, tolerance: tolerance.Value, key: key,
               complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: after, PlaneDistanceTolerance: tolerance.Value)))
```

## To

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud cloud, Option<NeighborhoodPolicy> policy, Op key) =>
    from active in policy.Match(
        Some: candidate => candidate.Admit(key: key),
        None: () => NeighborhoodPolicy.Of(context: cloud.Tolerance, key: key))
    from output in (AdmitsCase(cloud: cloud), Output == typeof(TOut)) switch {
        (false, _) => Fin.Fail<TOut>(key.Unsupported(inputType: cloud.GetType(), outputType: typeof(TOut))),
        (_, false) => Fin.Fail<TOut>(key.Unsupported(inputType: typeof(VectorCloudMetric), outputType: typeof(TOut))),
        _ => Measure(cloud: cloud, policy: active, key: key).Bind(value => value switch {
            Seq<Vector3d> values => ResultProjection.Values<Vector3d, TOut>(values: values, key: key, owner: typeof(VectorCloudMetric)),
            Seq<double> values => ResultProjection.Values<double, TOut>(values: values, key: key, owner: typeof(VectorCloudMetric)),
            Seq<Plane> values => ResultProjection.Values<Plane, TOut>(values: values, key: key, owner: typeof(VectorCloudMetric)),
            _ => key.AcceptValue(value: value).Map(static admitted => (TOut)admitted),
        })
    }
    select output;
```

```csharp
public readonly record struct CloudVoronoiResult(
    Seq<CloudVoronoiCell> Cells, Arr<Point3d> Vertices, Arr<(int Tail, int Head)> Skeleton, CloudVoronoiCensus Census) : IValidityEvidence {
    // Existing IsValid and Project<TOut> remain.

    public static Fin<CloudVoronoiResult> Of(
        VectorCloud source, Option<PositiveMagnitude> tolerance = default, Op? key = null) {
        Op op = key.OrDefault();
        return from cloud in Admit.NotNull(value: source, key: op)
               from cluster in cloud is VectorCloud.ClusterCase active
                   ? Fin.Succ(active)
                   : Fin.Fail<VectorCloud.ClusterCase>(op.Unsupported(inputType: cloud.GetType(), outputType: typeof(CloudVoronoiResult)))
               from distance in tolerance.Match(
                   Some: static supplied => Fin.Succ(supplied),
                   None: () => op.AcceptValidated<PositiveMagnitude>(candidate: cluster.Tolerance.For(lane: ToleranceLane.Deviation).Value))
               from _ in guard(cluster.Vertices.Count >= 4, op.InvalidInput()).ToFin()
               let sites = cluster.Vertices.Map(static (point, index) => new CloudVertex(index: index, x: point.X, y: point.Y, z: point.Z)).ToArray()
               from result in op.Catch(() => CloudKernel.CensusOf(
                   sites: sites, tolerance: distance, key: op,
                   complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: sites, PlaneDistanceTolerance: distance.Value)))
               select result;
    }
}

// CloudKernel.ComputeVoronoiDetailed DELETED
```

```csharp
internal static Fin<CloudVoronoiResult> CensusOf(
    CloudVertex[] sites, VoronoiMesh<CloudVertex, CloudCell, CloudEdge> complex, PositiveMagnitude tolerance, Op key) {
    double epsilon = tolerance.Value;
    CloudCell[] cells = [.. complex.Vertices];
    FrozenDictionary<CloudCell, (Point3d Center, double Radius)> spheres = cells
        .Select(cell => (Cell: cell, Sphere: Circumsphere(cell: cell, tolerance: epsilon)))
        .Where(static row => row.Sphere.IsSome)
        .ToFrozenDictionary(static row => row.Cell,
            static row => row.Sphere.IfNone(default((Point3d Center, double Radius))));
    return from hull in SolidOf(points: [.. sites.Select(PointOf)], tolerance: epsilon, key: key)
           let open = hull.Map(static solid => solid.Facets.Fold(Set<int>(),
               static (acc, facet) => facet.Vertices.Fold(acc, static (set, corner) => set.Add(corner)))).IfNone(Set<int>())
           from rows in CellRows(cells: cells, complex: complex, spheres: spheres, open: open, sites: sites, tolerance: epsilon, key: key)
           let tally = rows.Cells.Fold((Bounded: 0, Unbounded: 0, Degenerate: 0, Volume: 0.0), static (acc, cell) => cell.Measure.Switch(
               state: acc,
               bounded: static (t, measured) => (t.Bounded + 1, t.Unbounded, t.Degenerate, t.Volume + measured.Volume),
               unbounded: static (t, _) => (t.Bounded, t.Unbounded + 1, t.Degenerate, t.Volume),
               degenerate: static (t, _) => (t.Bounded, t.Unbounded, t.Degenerate + 1, t.Volume)))
           let census = new CloudVoronoiCensus(
               PlaneDistanceTolerance: tolerance, InputCount: sites.Length, DualVertexCount: cells.Length, UnmeasuredVertexCount: cells.Length - rows.Vertices.Count,
```

```csharp
from dual in key.Catch(() => CloudKernel.CensusOf(sites: seeds, tolerance: tolerance, key: key,
    complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: seeds, PlaneDistanceTolerance: tolerance.Value)))

from inserted in key.Catch(() => CloudKernel.CensusOf(sites: after, tolerance: tolerance, key: key,
    complex: VoronoiMesh.Create<CloudVertex, CloudCell>(data: after, PlaneDistanceTolerance: tolerance.Value)))
```

## Why

The relay owns no cloud behavior. `CloudCase` resolves the optional neighborhood policy only to invoke `VectorCloudMetric.Project`, while `VoronoiCase` demotes `PositiveMagnitude` to `double` and `CloudKernel.CensusOf` immediately reconstructs it. Deleting the relay without moving these gates would duplicate admission at callers or expose the internal kernel as a cross-package entry.

## Change

Make `VectorCloudMetric.Project` resolve and admit its optional policy before its existing compatibility and output gates. Add `CloudVoronoiResult.Of` as the public cluster-only 3D-dual entry. Delete `CloudKernel.ComputeVoronoiDetailed`; type `CensusOf` on `PositiveMagnitude`, derive one `epsilon` only for MIConvexHull and numeric kernels, and pass the admitted value from both natural-neighbor calls unchanged.

## Delta

Code-fence LOC: +15. Module surface: +1 public result-owner method and -1 internal kernel method, +0 types; net 0 members and 0 types. Tolerance admission inside `CensusOf`: 1 to 0 per call.

## Ripples

- `libs/dotnet/Rasm/.planning/Spatial/cloud.md`: update metric and Voronoi ownership, entry, package, and growth statements; retain MIConvexHull's verified `PlaneDistanceTolerance` spelling.
- `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/partition.md`: call `CloudVoronoiResult.Of` and retain the concrete result; remove obsolete relay package and diagram claims.
- `libs/dotnet/Rasm/.planning/Analysis/inspect.md`, `libs/dotnet/Rasm/.planning/Drawing/pack.md`, and `libs/dotnet/Rasm/.planning/Processing/decimate.md`: call the reshaped metric entry directly with `Option<NeighborhoodPolicy>`.

# 3. Delete the cross-domain projection facade

## From

`libs/dotnet/Rasm/.planning/Processing/intent.md:45-57`

```csharp
[Union]
public abstract partial record VectorIntent {
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Basis) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, Dimension Rank) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, Option<AnglePivot> Pivot) : VectorIntent;
    public sealed record SupportCase : VectorIntent { internal SupportCase(SupportSpace space, Point3d query, SupportProjection projection) { Space = space; Query = query; Projection = projection; } public SupportSpace Space { get; } public Point3d Query { get; } public SupportProjection Projection { get; } }
    public sealed record ExtractionCase : VectorIntent { internal ExtractionCase(Extraction value) => Value = value; public Extraction Value { get; } }
    public sealed record RayCase(Point3d Origin, Direction RayDirection, RayPolicy Policy) : VectorIntent;
    public sealed record FrameCase(Point3d Origin, Vector3d Normal, Option<Vector3d> XHint) : VectorIntent;
    public sealed record CurveCase : VectorIntent { internal CurveCase(Curve source, double parameter, CurveProjection mode) { Source = source; Parameter = parameter; Mode = mode; } public Curve Source { get; } public double Parameter { get; } public CurveProjection Mode { get; } }
    public sealed record CloudCase : VectorIntent { internal CloudCase(VectorCloud value, VectorCloudMetric metric, NeighborhoodPolicy policy) { Value = value; Metric = metric; Policy = policy; } public VectorCloud Value { get; } public VectorCloudMetric Metric { get; } public NeighborhoodPolicy Policy { get; } }
    public sealed record WindingCase : VectorIntent { internal WindingCase(VectorCloud.RingCase value, Point3d query) { Value = value; Query = query; } public VectorCloud.RingCase Value { get; } public Point3d Query { get; } }
```

`libs/dotnet/Rasm/.planning/Processing/intent.md:80-85`

```csharp
public sealed record TopologyCase : VectorIntent { internal TopologyCase(MeshSpace space) => Space = space; public MeshSpace Space { get; } }
public sealed record FeaturesCase : VectorIntent { internal FeaturesCase(MeshSpace space, MeshFeaturePolicy policy) { Space = space; Policy = policy; } public MeshSpace Space { get; } public MeshFeaturePolicy Policy { get; } }
public sealed record DescriptorCase : VectorIntent { internal DescriptorCase(MeshSpace space, MeshDescriptor kind, Dimension pairs) { Space = space; Kind = kind; Pairs = pairs; } public MeshSpace Space { get; } public MeshDescriptor Kind { get; } public Dimension Pairs { get; } }
public sealed record DiscreteCalculusCase : VectorIntent { internal DiscreteCalculusCase(MeshSpace space, MeshLaplacian kind) { Space = space; Kind = kind; } public MeshSpace Space { get; } public MeshLaplacian Kind { get; } }
public sealed record SegmentationCase : VectorIntent { internal SegmentationCase(MeshSpace space, MeshSegmentation kind) { Space = space; Kind = kind; } public MeshSpace Space { get; } public MeshSegmentation Kind { get; } }
private VectorIntent() { }
```

`libs/dotnet/Rasm/.planning/Processing/intent.md:89-99`

```csharp
// --- [CONSTRUCTION]
public static Fin<VectorIntent> Axis(SignedAxis axis, Option<Plane> frame = default, Op? key = null) {
    Op op = key.OrDefault();
    return from active in Admit.NotNull(value: axis, key: op)
           from basis in frame.TraverseM(plane => Admit.Plane(basis: plane, key: op)).As()
           select (VectorIntent)new AxisCase(Value: active, Basis: basis);
}
public static VectorIntent Direction(Vector3d value) => new DirectionCase(Value: value);
public static VectorIntent Axes(Option<Seq<Vector3d>> values = default, Option<Dimension> rank = default) =>
    new AxesCase(Values: values, Rank: rank.IfNone(SpatialRank));
public static VectorIntent Angular(Vector3d a, Vector3d b, Option<AnglePivot> pivot = default) => new AngularCase(A: a, B: b, Pivot: pivot);
```

`libs/dotnet/Rasm/.planning/Processing/intent.md:277-285`

```csharp
public Fin<TOut> Project<TOut>(Context context, Op? key = null) {
    Op op = key.OrDefault();
    return from model in Admit.NotNull(value: context, error: op.MissingContext())
           from result in Dispatch<TOut>(context: model, op: op)
           select result;
}
private Fin<TOut> Dispatch<TOut>(Context context, Op op) => Switch(
    state: (Context: context, Key: op),
    axisCase: static (state, axis) =>
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:250-254`

```csharp
internal static Fin<ManifoldStatus> Status(MeshSpace space, Context context, Op key) =>
    VectorIntent.Topology(space, key)
        .Bind(intent => intent.Project<(int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)>(context: context, key: key))
        .Map(ManifoldStatus.Of);
```

`libs/dotnet/Rasm/.planning/Processing/session.md:34-38`

```csharp
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);
```

`libs/dotnet/Rasm.Rhino/.planning/Viewport/operations.md:620-621`

```csharp
return from intent in VectorIntent.Pose(from: self.From.Frame.Value, to: self.To.Frame.Value, t: progress.Value, mode: self.Interpolation, key: op)
       from plane in intent.Project<Plane>(context: self.Context, key: op)
```

`libs/dotnet/Rasm.Fabrication/.planning/Toolpath/partition.md:420-422`

```csharp
: from cloud in VectorCloud.Cluster(accepted, request.Boundary.Tolerance, key: Op.Of(name: nameof(Solids)))
  from intent in VectorIntent.Voronoi(cloud, key: Op.Of(name: nameof(Solids)))
  from dual in intent.Project<CloudVoronoiResult>(request.Boundary.Tolerance, Op.Of(name: nameof(Solids)))
```

## To

```csharp
// VectorIntent DELETED
```

```csharp
internal static Fin<ManifoldStatus> Status(MeshSpace space, Context context, Op key) =>
    MeshKernel.TopologyDetailed(space).Map(topology => new ManifoldStatus(
        EulerCharacteristic: topology.EulerCharacteristic,
        BoundaryComponents: topology.BoundaryComponents,
        IsManifold: topology.Traits.Admits(MeshTrait.Manifold),
        IsOriented: topology.Traits.Admits(MeshTrait.Oriented),
        NonManifoldEdges: topology.NonManifoldEdges,
        Genus: topology.Genus));
```

```csharp
// ManifoldStatus.Of DELETED
```

```csharp
return from plane in self.Interpolation.Interpolate(
    a: self.From.Frame.Value, b: self.To.Frame.Value, t: progress, context: self.Context, key: op)
```

```csharp
: from cloud in VectorCloud.Cluster(accepted, request.Boundary.Tolerance, key: Op.Of(name: nameof(Solids)))
  from dual in CloudVoronoiResult.Of(cloud, key: Op.Of(name: nameof(Solids)))
```

## Why

`VectorIntent` groups 38 unrelated capabilities that share neither one domain identity nor a live stored-program consumer. Every construction site immediately projects the case, so the union duplicates 38 payloads, 38 factories, 38 dispatch arms, and the real owners' admission and projection surfaces. Twenty-six factories have no consumer. The topology route also requests a tuple containing two booleans that `Topology.Project<TOut>` does not publish, so it returns `Unsupported` instead of a `ManifoldStatus`. Thinktecture exhaustive dispatch adds value to a real closed family, not a cross-domain forwarding switch.

## Change

Delete `Processing/intent.md` whole and add no replacement union, facade, extension wrapper, alias, or relay. Route every live construction directly to the owner named by its former dispatch arm: `Direction.Of`, `VectorAngle.Of`, `VectorSpan.Of(...).Components`, `VectorRelation.Of`, `SignedAxis.Cardinal` plus `TraverseM`, `SupportSpace.Closest` plus `SupportProjection.Project`, `CurveProjection.Project`, `VectorCloudMetric.Project`, `MotionInterpolation.Interpolate`, `SegmentKernel.DetectFeatureEdgesDetailed`, `MeshKernel.TopologyDetailed`, and `CloudVoronoiResult.Of`. Inline one-step axes/component arithmetic only at its consumer. Replace the invalid topology tuple projection with a direct `Topology` field map and delete the one-call `ManifoldStatus.Of` wrapper.

## Delta

Code-fence LOC: -385 in the target and -2 from `ManifoldStatus.Of`; direct consumer rewrites add no module members or types. Module surface: -39 types and -175 members (38 cases, their 94 properties and 38 constructors, 38 factories, root constructor, `SpatialRank`, `Project`, `Dispatch`, and `ManifoldStatus.Of`), +0 types and +0 members; net -39 types and -175 members.

## Ripples

- `libs/dotnet/Rasm/.planning/Parametric/locate.md`: call `CurveProjection.Project` for the four curve rows; call `SupportSpace.Closest` then `SupportProjection.Project` for closest-point rows; remove relay prose and diagram nodes.
- `libs/dotnet/Rasm/.planning/Analysis/inspect.md`: call `VectorCloudMetric.Project`, `Rasm.Numerics.Direction.Of`, and `VectorAngle.Of` directly; preserve the existing `Fin` and `TraverseM` chains.
- `libs/dotnet/Rasm/.planning/Analysis/measure.md`: call `Rasm.Numerics.Direction.Of` and the support owner pair directly; remove the `Rasm.Processing` package claim.
- `libs/dotnet/Rasm/.planning/Analysis/relations.md`: call `VectorRelation.Of` directly and retain the unsupported-result recovery.
- `libs/dotnet/Rasm/.planning/Analysis/select.md`: call `Rasm.Numerics.Direction.Of`, `VectorSpan.Of(...).Components`, and `SignedAxis.Cardinal` directly; preserve `TraverseM` for collection sequencing.
- `libs/dotnet/Rasm/.planning/Drawing/pack.md`: call `VectorCloudMetric.Project` directly with `Some(policy.Cloud)`.
- `libs/dotnet/Rasm/.planning/Drawing/view.md`, `libs/dotnet/Rasm/.planning/Processing/decimate.md`, and `libs/dotnet/Rasm/.planning/Processing/flatten.md`: pass the `MeshFeaturePolicy.Of` result directly to `SegmentKernel.DetectFeatureEdgesDetailed`; remove each construct/project pair and do not re-admit the policy in the kernel.
- `libs/dotnet/Rasm/.planning/Processing/repair.md` and `libs/dotnet/Rasm/.planning/Processing/session.md`: map `Topology` directly to `ManifoldStatus` and delete the tuple factory.
- `libs/dotnet/Rasm.Rhino/.planning/Viewport/operations.md`: pass the existing `UnitInterval` directly to `MotionInterpolation.Interpolate`; remove the Processing claim and relay prose.
- `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/partition.md`: call `CloudVoronoiResult.Of` and update its owner, package, boundary, and diagram statements.
- `libs/dotnet/Rasm.AppUi/.planning/Render/pipeline.md`, `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/surface.md`, `libs/dotnet/Rasm/.planning/Parametric/projections.md`, `libs/dotnet/Rasm/.planning/Processing/flow.md`, `libs/dotnet/Rasm/.planning/Processing/geodesics.md`, `libs/dotnet/Rasm/.planning/Processing/register.md`, `libs/dotnet/Rasm/.planning/Processing/remesh.md`, and `libs/dotnet/Rasm/.planning/Processing/session.md`: remove obsolete `VectorIntent` ownership/package references and name the actual owner already carried by each statement.
- `libs/dotnet/Rasm/README.md`: delete the `INTENT` router row and renumber the following rows.
- `libs/dotnet/Rasm/ARCHITECTURE.md`, `libs/dotnet/Rasm.Fabrication/ARCHITECTURE.md`, and `libs/dotnet/Rasm.Rhino/ARCHITECTURE.md`: remove `Intent.cs` and every `VectorIntent` import, wire, and boundary edge; retain direct edges only for shapes consumed by a live fence.
