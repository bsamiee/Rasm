namespace Rasm.Analysis;

public sealed record Query<TGeometry, TOut> where TGeometry : notnull {
    internal Op Key { get; }
    internal Requirement Requirement { get; }
    internal bool RequiresContext { get; }
    internal Option<Error> Rejection { get; }
    private Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>> Evaluate { get; }
    private Option<Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>>> AggregatePlan { get; }
    internal Query(
        Op key,
        Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>> effect,
        Requirement? requirement = null,
        bool requiresContext = false,
        Option<Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>>> aggregate = default,
        Option<Error> rejection = default) {
        Key = key;
        Requirement = requirement ?? Requirement.None;
        RequiresContext = requiresContext;
        Rejection = rejection;
        Evaluate = effect;
        AggregatePlan = aggregate;
    }
    public Eff<Analyze.Env, Seq<TOut>> Apply(TGeometry geometry) => Evaluate(arg: Seq(geometry));
    public Eff<Analyze.Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) => Evaluate(arg: geometry);
    internal Query<TIn, TOut> Contramap<TIn>(Func<TIn, TGeometry> map) where TIn : notnull =>
        new(
            key: Key,
            requirement: Requirement,
            requiresContext: RequiresContext,
            aggregate: AggregatePlan.Map<Func<Seq<TIn>, Eff<Analyze.Env, Seq<TOut>>>>(project => input => project(arg: input.Map(value => map(arg: value)))),
            rejection: Rejection,
            effect: input => Evaluate(arg: input.Map(value => map(arg: value))));
    public Query<TGeometry, TOut> Aggregate() =>
        AggregatePlan.Match(
            Some: project => new Query<TGeometry, TOut>(
                key: Key,
                requirement: Requirement,
                requiresContext: RequiresContext,
                aggregate: Some(project),
                effect: project),
            None: () => Reject(key: Key, fault: Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))));
    internal static Query<TGeometry, TOut> Build(
        Op key,
        Func<TGeometry, Eff<Analyze.Env, Seq<TOut>>> evaluator,
        Requirement? requirement = null,
        bool requiresContext = false,
        Option<Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>>> aggregate = default) {
        Requirement activeRequirement = requirement ?? Requirement.None;
        return new(
            key: key,
            requirement: activeRequirement,
            requiresContext: requiresContext,
            aggregate: aggregate.Map<Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TOut>>>>(project => geometry => from runtime in Analyze.EnvAsks
                                                                                                               from resolved in geometry.Traverse(item => Ready(geometry: item).Run(env: runtime)).As().ToEff()
                                                                                                               from result in project(arg: resolved)
                                                                                                               select result),
            effect: geometry => from runtime in Analyze.EnvAsks
                                from result in geometry.Traverse(item => (
                                        from resolved in Ready(geometry: item)
                                        from valid in Validate(geometry: resolved, requirement: activeRequirement)
                                        from value in evaluator(arg: valid)
                                        select value).Run(env: runtime))
                                    .Map(static chunks => chunks.Bind(static chunk => chunk))
                                    .As()
                                    .ToEff()
                                select result);
    }
    internal static Query<TGeometry, TOut> Build<TState>(
        Op key,
        TState state,
        Func<TState, TGeometry, Eff<Analyze.Env, Seq<TOut>>> evaluator,
        Requirement? requirement = null,
        bool requiresContext = false) =>
        Build(
            key: key,
            evaluator: geometry => evaluator(arg1: state, arg2: geometry),
            requirement: requirement,
            requiresContext: requiresContext);
    internal static Query<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(
            key: key,
            effect: _ => Fin.Fail<Seq<TOut>>(fault).ToEff(),
            rejection: Some(fault));
    private static Eff<Analyze.Env, TGeometry> Ready(TGeometry geometry) =>
        from runtime in Analyze.EnvAsks
        from resolved in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(new Fault.Cancelled()),
            false => Optional(geometry).ToFin(new Fault.MissingGeometry()),
        }).ToEff()
        select resolved;
    private static Eff<Analyze.Env, TGeometry> Validate(TGeometry geometry, Requirement requirement) =>
        (requirement.IsEmpty, geometry) switch {
            (false, GeometryBase native) => from context in Analyze.Asks
                                            from _ in context.Validate(geometry: native, requirement: requirement).ToEff()
                                            select geometry,
            _ => Fin.Succ(geometry).ToEff(),
        };
}
[SmartEnum<int>]
public sealed partial class MassKind {
    private delegate Eff<Analyze.Env, IDisposable> ComputeMass(object geometry, bool secondMoments, bool productMoments);
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None, compute: static (geometry, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationUnsupported(Label: nameof(None), GeometryType: geometry.GetType())).ToEff(), sum: static _ => Fin.Fail<IDisposable>(new Fault.ComputationFailed(Label: nameof(None))));
    public static readonly MassKind Length = new(
        key: 1,
        label: nameof(Length),
        requirement: Requirement.CurveLength,
        compute: static (geometry, secondMoments, productMoments) => (geometry switch {
            Curve curve => Optional(LengthMassProperties.Compute(curve: curve, length: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments))
                .ToFin(new Fault.ComputationFailed(Label: nameof(LengthMassProperties)))
                .Map(static props => (IDisposable)props),
            _ => Fin.Fail<IDisposable>(new Fault.ComputationUnsupported(Label: nameof(LengthMassProperties), GeometryType: geometry.GetType())),
        }).ToEff(),
        sum: static props => Optional(LengthMassProperties.WeightedSum(summands: props.AsIterable().Cast<LengthMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(new Fault.ComputationFailed(Label: nameof(LengthMassProperties)))
            .Map(static props => (IDisposable)props));
    public static readonly MassKind Area = new(
        key: 2,
        label: nameof(Area),
        requirement: Requirement.AreaMass,
        compute: static (geometry, secondMoments, productMoments) => from context in Analyze.Asks
                                                                     from props in Optional(geometry switch {
                                                                         Curve curve => AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: context.Absolute.Value),
                                                                         Mesh mesh => AreaMassProperties.Compute(mesh: mesh, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                                                                         Brep brep => AreaMassProperties.Compute(brep: brep, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: context.Relative.Value, absoluteTolerance: context.Absolute.Value),
                                                                         Surface surface => AreaMassProperties.Compute(surface: surface, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                                                                         _ => null,
                                                                     }).ToFin(geometry switch {
                                                                         Curve or Mesh or Brep or Surface => new Fault.ComputationFailed(Label: nameof(AreaMassProperties)),
                                                                         _ => new Fault.ComputationUnsupported(Label: nameof(AreaMassProperties), GeometryType: geometry.GetType()),
                                                                     }).Map(static props => (IDisposable)props).ToEff()
                                                                     select props,
        sum: static props => Optional(AreaMassProperties.WeightedSum(summands: props.AsIterable().Cast<AreaMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(new Fault.ComputationFailed(Label: nameof(AreaMassProperties)))
            .Map(static props => (IDisposable)props));
    public static readonly MassKind Volume = new(
        key: 3,
        label: nameof(Volume),
        requirement: Requirement.VolumeMass,
        compute: static (geometry, secondMoments, productMoments) => from context in Analyze.Asks
                                                                     from props in Optional(geometry switch {
                                                                         Mesh mesh => VolumeMassProperties.Compute(mesh: mesh, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                                                                         Brep brep => VolumeMassProperties.Compute(brep: brep, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: context.Relative.Value, absoluteTolerance: context.Absolute.Value),
                                                                         Surface surface => VolumeMassProperties.Compute(surface: surface, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                                                                         _ => null,
                                                                     }).ToFin(geometry switch {
                                                                         Mesh or Brep or Surface => new Fault.ComputationFailed(Label: nameof(VolumeMassProperties)),
                                                                         _ => new Fault.ComputationUnsupported(Label: nameof(VolumeMassProperties), GeometryType: geometry.GetType()),
                                                                     }).Map(static props => (IDisposable)props).ToEff()
                                                                     select props,
        sum: static props => Optional(VolumeMassProperties.WeightedSum(summands: props.AsIterable().Cast<VolumeMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(new Fault.ComputationFailed(Label: nameof(VolumeMassProperties)))
            .Map(static props => (IDisposable)props));
    public string Label { get; }
    internal Requirement Requirement { get; }
    private ComputeMass Compute { get; }
    private Func<Seq<IDisposable>, Fin<IDisposable>> Sum { get; }
    internal Query<TGeometry, TValue> Build<TGeometry, TValue>(Op key, Func<Op, IDisposable, Fin<Seq<TValue>>> project, bool secondMoments = false, bool productMoments = false) where TGeometry : notnull =>
        Query<TGeometry, TValue>.Build(
            key: key,
            requirement: Requirement,
            requiresContext: true,
            aggregate: Some<Func<Seq<TGeometry>, Eff<Analyze.Env, Seq<TValue>>>>(
                geometry => from props in ComputeAll(geometry: geometry, secondMoments: secondMoments, productMoments: productMoments)
                            from values in Query.BracketEach(
                                resources: props,
                                body: owned => from mass in Sum(arg: owned)
                                               from projected in Query.Bracket(factory: () => mass, body: disposable => project(arg1: key, arg2: disposable))
                                               select projected).ToEff()
                            select values),
            evaluator: geometry => from mass in Compute(geometry: geometry, secondMoments: secondMoments, productMoments: productMoments)
                                   from values in Query.Bracket(factory: () => mass, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                                   select values);
    private Eff<Analyze.Env, Seq<IDisposable>> ComputeAll<TGeometry>(Seq<TGeometry> geometry, bool secondMoments, bool productMoments) where TGeometry : notnull =>
        from runtime in Analyze.EnvAsks
        from props in geometry.Fold(
                initialState: Fin.Succ(Seq<IDisposable>()),
                f: (state, item) => state.Bind(owned => Compute(geometry: item, secondMoments: secondMoments, productMoments: productMoments)
                    .Run(env: runtime)
                    .Match(
                        Succ: resource => Fin.Succ(resource.Cons(owned)),
                        Fail: error => (Query.DisposeAll(resources: owned), Fin.Fail<Seq<IDisposable>>(error)).Item2)))
            .ToEff()
        select props;
}
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
public enum MeshFaceMetric { None = 0, AspectRatio = 1, Area = 2, Perimeter = 3, Skewness = 4, DihedralAngle = 5 }
public static class MeshFaceMetrics {
    public static Option<double> Sample(this MeshFaceMetric metric, Mesh mesh, int face) {
        ArgumentNullException.ThrowIfNull(argument: mesh);
        return metric switch {
            MeshFaceMetric.AspectRatio => Some(mesh.Faces.GetFaceAspectRatio(index: face)),
            MeshFaceMetric.Area => Some(FaceArea(mesh: mesh, face: face)),
            MeshFaceMetric.Perimeter => Some(FacePerimeter(mesh: mesh, face: face)),
            MeshFaceMetric.Skewness => Some(FaceSkewness(mesh: mesh, face: face)),
            MeshFaceMetric.DihedralAngle => Some(FaceMaxDihedral(mesh: mesh, face: face)),
            _ => Option<double>.None,
        };
    }
    internal static Vector3d ComputeFaceNormal(Mesh mesh, int face) {
        Seq<Point3d> verts = FaceVertices(mesh: mesh, face: face);
        Vector3d cross = Vector3d.CrossProduct(a: verts[1] - verts[0], b: verts[2] - verts[0]);
        _ = cross.Unitize();
        return cross;
    }
    internal static Seq<Point3d> FaceVertices(Mesh mesh, int face) {
        MeshFace mf = mesh.Faces[face];
        return mf.IsQuad switch {
            true => Seq((Point3d)mesh.Vertices[mf.A], (Point3d)mesh.Vertices[mf.B], (Point3d)mesh.Vertices[mf.C], (Point3d)mesh.Vertices[mf.D]),
            false => Seq((Point3d)mesh.Vertices[mf.A], (Point3d)mesh.Vertices[mf.B], (Point3d)mesh.Vertices[mf.C]),
        };
    }
    private static double FaceArea(Mesh mesh, int face) =>
        FaceVertices(mesh: mesh, face: face) switch {
            Seq<Point3d> v when v.Count == 4 => 0.5 * (Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length + Vector3d.CrossProduct(a: v[2] - v[0], b: v[3] - v[0]).Length),
            Seq<Point3d> v => 0.5 * Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length,
        };
    private static double FacePerimeter(Mesh mesh, int face) =>
        FaceVertices(mesh: mesh, face: face) switch {
            Seq<Point3d> v => v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d),
        };
    private static double FaceSkewness(Mesh mesh, int face) {
        Seq<Point3d> v = FaceVertices(mesh: mesh, face: face);
        double ideal = v.Count == 4 ? Math.PI / 2.0 : Math.PI / 3.0;
        return v.Map((vertex, i) => Vector3d.VectorAngle(a: v[(i + v.Count - 1) % v.Count] - vertex, b: v[(i + 1) % v.Count] - vertex))
            .Fold(initialState: 0.0, f: (acc, angle) => Math.Max(val1: acc, val2: Math.Max(val1: (angle - ideal) / (Math.PI - ideal), val2: (ideal - angle) / ideal)));
    }
    private static double FaceMaxDihedral(Mesh mesh, int face) {
        Vector3d normal = ComputeFaceNormal(mesh: mesh, face: face);
        return normal.IsValid switch {
            false => 0.0,
            true => toSeq(mesh.TopologyEdges.GetEdgesForFace(faceIndex: face))
                .Bind(edge => toSeq(mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge)).Filter(other => other != face))
                .Fold(initialState: 0.0, f: (max, other) => ComputeFaceNormal(mesh: mesh, face: other) switch {
                    { IsValid: true } neighbour => Math.Max(val1: max, val2: Vector3d.VectorAngle(a: normal, b: neighbour)),
                    _ => max,
                }),
        };
    }
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureProfile(CurvatureScalar Scalar, int Count, double Minimum, double Maximum, double Mean, double Variance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct ResidualProfile(int Count, double Minimum, double Maximum, double Mean, double Variance, double Rms, double Tolerance, bool WithinTolerance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceSample(int Face, double Value);
[StructLayout(LayoutKind.Auto)]
public readonly record struct Hit(int Id);
[StructLayout(LayoutKind.Auto)]
public readonly record struct Couple(int A, int B);
[StructLayout(LayoutKind.Auto)]
public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
[Union]
public partial record Bounds {
    public sealed record Box : Bounds; public sealed record Oriented(Plane Plane) : Bounds; public sealed record Transformed(Transform Transform) : Bounds; public sealed record Center : Bounds;
    public sealed record Corners : Bounds; public sealed record Edges : Bounds; public sealed record Area : Bounds; public sealed record Volume : Bounds;
}
[Union]
public partial record Measure {
    public sealed record Length : Measure; public sealed record Area : Measure; public sealed record Volume : Measure; public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure; public sealed record MassError(MassKind Mass) : Measure; public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure; public sealed record PrincipalAxes(MassKind Mass) : Measure;
}
[Union]
public partial record Location {
    public sealed record Midpoint : Location; public sealed record Tangent : Location; public sealed record Closest(Point3d Point) : Location;
    public sealed record PointAtCurve(double Parameter) : Location; public sealed record PointAtSurface(Point2d Uv) : Location; public sealed record PointAtLength(double Length) : Location;
    public sealed record FrameAtCurve(double Parameter) : Location; public sealed record FrameAtSurface(Point2d Uv) : Location; public sealed record PerpendicularFrameAt(double Parameter) : Location;
    public sealed record NormalAt(Point2d Uv) : Location; public sealed record CurvatureAtCurve(double Parameter) : Location; public sealed record CurvatureAtSurface(Point2d Uv) : Location;
    public sealed record CurvatureProfile(int Count, CurvatureScalar Scalar) : Location; public sealed record DerivativeAt(double Parameter, int Count) : Location;
    public sealed record DivideByCount(int Count) : Location; public sealed record DivideByLength(double Length) : Location; public sealed record Orientation(Plane Plane) : Location;
    public sealed record Contains(Point3d Point, Plane Plane) : Location; public sealed record ShortPath(Point2d Start, Point2d End) : Location;
    public sealed record ControlPoints : Location;
}
[Union]
public partial record Faces {
    public sealed record AllCase : Faces; public sealed record TopCase(Vector3d Axis) : Faces; public sealed record BottomCase(Vector3d Axis) : Faces; public sealed record AtCase(int? Value) : Faces;
    public static Faces All => new AllCase(); public static Faces Top(Vector3d axis) => new TopCase(Axis: axis); public static Faces Bottom(Vector3d axis) => new BottomCase(Axis: axis);
    public static Faces At(int? index = null) => new AtCase(Value: index);
}
public enum MeshCheckCount {
    None = 0,
    DegenerateFaces = 1, DisjointMeshes = 2, DuplicateFaces = 3, ExtremelyShortEdges = 4,
    InvalidNgons = 5, NakedEdges = 6, NonManifoldEdges = 7, NonUnitVectorNormals = 8,
    RandomFaceNormals = 9, SelfIntersectingPairs = 10, UnusedVertices = 11,
    VertexFaceNormalsDiffer = 12, ZeroLengthNormals = 13,
}
public static class MeshCheckCounts {
    public static int Get(this MeshCheckCount metric, MeshCheckParameters parameters) => metric switch {
        MeshCheckCount.DegenerateFaces => parameters.DegenerateFaceCount,
        MeshCheckCount.DisjointMeshes => parameters.DisjointMeshCount,
        MeshCheckCount.DuplicateFaces => parameters.DuplicateFaceCount,
        MeshCheckCount.ExtremelyShortEdges => parameters.ExtremelyShortEdgeCount,
        MeshCheckCount.InvalidNgons => parameters.InvalidNgonCount,
        MeshCheckCount.NakedEdges => parameters.NakedEdgeCount,
        MeshCheckCount.NonManifoldEdges => parameters.NonManifoldEdgeCount,
        MeshCheckCount.NonUnitVectorNormals => parameters.NonUnitVectorNormalCount,
        MeshCheckCount.RandomFaceNormals => parameters.RandomFaceNormalCount,
        MeshCheckCount.SelfIntersectingPairs => parameters.SelfIntersectingPairsCount,
        MeshCheckCount.UnusedVertices => parameters.UnusedVertexCount,
        MeshCheckCount.VertexFaceNormalsDiffer => parameters.VertexFaceNormalsDifferCount,
        MeshCheckCount.ZeroLengthNormals => parameters.ZeroLengthNormalCount,
        _ => 0,
    };
    internal static IEnumerable<MeshCheckCount> Defects => Enum.GetValues<MeshCheckCount>().Where(static m => m != MeshCheckCount.None);
}
[Union]
public partial record Curves {
    public sealed record AllCase : Curves; public sealed record SegmentsCase : Curves; public sealed record BoundaryCase : Curves; public sealed record NakedOuterCase : Curves; public sealed record NakedInnerCase : Curves; public sealed record InteriorCase : Curves; public sealed record NonManifoldCase : Curves; public sealed record OuterLoopCase : Curves; public sealed record InnerLoopCase : Curves; public sealed record IsoUCase : Curves; public sealed record IsoVCase : Curves; public sealed record SubCurvesCase : Curves; public sealed record SilhouetteCase(Vector3d? Direction) : Curves; public sealed record DraftCase(Vector3d? Direction, double? Angle) : Curves; public sealed record AtCase(int? Value) : Curves;
    public static Curves All => new AllCase(); public static Curves Segments => new SegmentsCase(); public static Curves Boundary => new BoundaryCase(); public static Curves NakedOuter => new NakedOuterCase();
    public static Curves NakedInner => new NakedInnerCase(); public static Curves Interior => new InteriorCase(); public static Curves NonManifold => new NonManifoldCase(); public static Curves OuterLoop => new OuterLoopCase();
    public static Curves InnerLoop => new InnerLoopCase(); public static Curves IsoU => new IsoUCase(); public static Curves IsoV => new IsoVCase(); public static Curves SubCurves => new SubCurvesCase();
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new DraftCase(Direction: direction, Angle: angle);
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal bool InputCurve => InputBoundary || this is SegmentsCase or SubCurvesCase;
    internal bool InputBoundary => this is AllCase or BoundaryCase;
    internal (CurveFeature Feature, Func<BrepEdge, bool> Brep, Func<Mesh, int, bool> Mesh)? Edge => this switch {
        AllCase => (CurveFeature.Edge, static _ => true, static (_, _) => true),
        SegmentsCase => (CurveFeature.Segment, static _ => true, static (_, _) => true),
        BoundaryCase => (CurveFeature.Boundary, BrepNakedEdge(nakedOuter: true, nakedInner: true), static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 1),
        NakedOuterCase => (CurveFeature.NakedOuter, BrepNakedEdge(nakedOuter: true, nakedInner: false), static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 1),
        NakedInnerCase => (CurveFeature.NakedInner, BrepNakedEdge(nakedOuter: false, nakedInner: true), static (_, _) => false),
        InteriorCase => (CurveFeature.Interior, static edge => edge.Valence == EdgeAdjacency.Interior, static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length == 2),
        NonManifoldCase => (CurveFeature.NonManifold, static edge => edge.Valence == EdgeAdjacency.NonManifold, static (mesh, index) => mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: index).Length > 2),
        _ => null,
    };
    private static Func<BrepEdge, bool> BrepNakedEdge(bool nakedOuter, bool nakedInner) =>
        edge => edge.Valence == EdgeAdjacency.Naked && toSeq(edge.TrimIndices()).Exists(trim => edge.Brep.Trims[trim].Loop.LoopType switch { BrepLoopType.Outer => nakedOuter, BrepLoopType.Inner => nakedInner, _ => false });
}
[Union]
public partial record Meshes {
    public sealed record ValidityBundleCase : Meshes; public sealed record StatsBundleCase : Meshes; public sealed record DefectsBundleCase : Meshes;
    public sealed record FaceQualityCase(MeshFaceMetric Metric) : Meshes; public sealed record AtFaceCase(int? Value) : Meshes;
    public static Meshes ValidityBundle => new ValidityBundleCase(); public static Meshes StatsBundle => new StatsBundleCase(); public static Meshes DefectsBundle => new DefectsBundleCase();
    public static Meshes FaceQuality(MeshFaceMetric metric) => new FaceQualityCase(Metric: metric);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
}
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record ProfileResidual(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
}
public static partial class Query {
    internal static readonly Op
        MidpointKey = Op.Create(value: "Midpoint"), BoundsKey = Op.Create(value: nameof(Bounds)), OrientedBoundsKey = Op.Create(value: "OrientedBounds"),
        TransformedBoundsKey = Op.Create(value: "TransformedBounds"), BoundsCenterKey = Op.Create(value: "BoundsCenter"), BoundsCornersKey = Op.Create(value: "BoundsCorners"),
        BoxEdgesKey = Op.Create(value: "BoxEdges"), BoxAreaKey = Op.Create(value: "BoxArea"), BoxVolumeKey = Op.Create(value: "BoxVolume"), MeasureKey = Op.Create(value: nameof(Measure)),
        LengthKey = Op.Create(value: "Length"), TangentKey = Op.Create(value: "Tangent"), ClosestKey = Op.Create(value: "Closest"),
        DomainKey = Op.Create(value: nameof(Domain)), PointAtKey = Op.Create(value: "PointAt"), PointAtLengthKey = Op.Create(value: "PointAtLength"),
        FrameAtKey = Op.Create(value: "FrameAt"), PerpendicularFrameAtKey = Op.Create(value: "PerpendicularFrameAt"), NormalAtKey = Op.Create(value: "NormalAt"),
        CurvatureAtKey = Op.Create(value: "CurvatureAt"), DerivativeAtKey = Op.Create(value: "DerivativeAt"), DivideByCountKey = Op.Create(value: "DivideByCount"),
        DivideByLengthKey = Op.Create(value: "DivideByLength"), OrientationKey = Op.Create(value: "Orientation"), ContainsKey = Op.Create(value: "Contains"),
        SegmentsKey = Op.Create(value: nameof(Segments)), EdgesKey = Op.Create(value: nameof(Edges)), NakedEdgesKey = Op.Create(value: nameof(NakedEdges)),
        EdgeMidpointsKey = Op.Create(value: "EdgeMidpoints"), SpatialMidpointKey = Op.Create(value: "SpatialMidpoint"),
        OutlinesKey = Op.Create(value: nameof(Outlines)), IsoKey = Op.Create(value: nameof(Iso)), PrimitiveKey = Op.Create(value: "Primitive"),
        ShortPathKey = Op.Create(value: "ShortPath"), SolidOrientationKey = Op.Create(value: nameof(SolidOrientation)), IsPointInsideKey = Op.Create(value: nameof(IsPointInside)),
        VerticesKey = Op.Create(value: nameof(Vertices)), ComponentsKey = Op.Create(value: "Components"), IsManifoldKey = Op.Create(value: nameof(IsManifold)),
        NakedPointStatusKey = Op.Create(value: nameof(NakedPointStatus)), MeshCheckKey = Op.Create(value: nameof(MeshCheck)), MeshCheckCountKey = Op.Create(value: "MeshCheckCount"), MeshFaceMetricKey = Op.Create(value: nameof(MeshFaceMetric)), SelfIntersectionsKey = Op.Create(value: nameof(SelfIntersections)), IntersectKey = Op.Create(value: nameof(Intersect)),
        ConformanceKey = Op.Create(value: nameof(Conformance)), DeviationKey = Op.Create(value: nameof(Deviation)), TreeKey = Op.Create(value: nameof(Tree)),
        ScopeKey = Op.Create(value: nameof(Analyze.Scope)), KindKey = Op.Create(value: nameof(Kind)), UniqueCornersKey = Op.Create(value: "UniqueCorners"),
        QuadrantsKey = Op.Create(value: nameof(Quadrants)), FacesKey = Op.Create(value: nameof(Faces)), CurvesKey = Op.Create(value: nameof(Curves)), MeshesKey = Op.Create(value: nameof(Meshes)),
        MeshValidityBundleKey = Op.Create(value: "MeshValidityBundle"), MeshStatsBundleKey = Op.Create(value: "MeshStatsBundle"), MeshDefectsBundleKey = Op.Create(value: "MeshDefectsBundle"), MeshAtFaceKey = Op.Create(value: "MeshAtFace"),
        ControlPointsKey = Op.Create(value: "ControlPoints"), AspectDispatchKey = Op.Create(value: "AspectDispatch");
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(Op key, object query) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> typed => typed,
            _ => Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
        };
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue>(
        Op key,
        Func<TNative, Eff<Analyze.Env, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, Func<TNative, Eff<Analyze.Env, Seq<TValue>>>>(
            key: key,
            state: project,
            project: static (nativeProject, native) => nativeProject(arg: native));
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(
        Op key,
        TState state,
        Func<TState, TNative, Eff<Analyze.Env, Seq<TValue>>> project,
        Requirement? requirement = null,
        bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key,
            requirement: requirement,
            requiresContext: requiresContext,
            state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }));
    internal static Fin<Seq<TValue>> One<TValue>(this Op key, TValue value) => key.RequireValid(value: value).Map(static candidate => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue>? values) => Optional(values).ToSeq().Bind(static value => value.AsIterable().ToSeq()).Traverse(value => key.RequireValid(value: value)).As();
    internal static Fin<Seq<TValue>> Solved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch { true => key.One(value: value), false => Fin.Fail<Seq<TValue>>(key.InvalidResult()) };
    internal static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    internal static Fin<TOut> BracketEach<TResource, TOut>(Seq<TResource> resources, Func<Seq<TResource>, Fin<TOut>> body) where TResource : class, IDisposable {
        Fin<TOut> result = body(arg: resources);
        _ = DisposeAll(resources: resources);
        return result;
    }
    internal static Unit DisposeAll<TResource>(Seq<TResource> resources) where TResource : class, IDisposable {
        _ = resources.Iter(static resource => resource.Dispose());
        return Unit.Default;
    }
    internal static Fin<Seq<TOut>> Results<TValue, TOut>(this Op key, IEnumerable<TValue>? values) => typeof(TValue).Equals(typeof(TOut)) switch {
        true => Many(key: key, values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
        false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
    };
    internal static Query<TGeometry, TOut> ClosestMatch<TGeometry, TOut, TSource, TValue>(
        Point3d point,
        Func<Point3d, TSource, Fin<Seq<TValue>>> project) where TGeometry : notnull where TSource : notnull =>
        Native<TGeometry, TOut, TSource, TValue, (Point3d Point, Func<Point3d, TSource, Fin<Seq<TValue>>> Project)>(
            key: ClosestKey,
            state: (Point: point, Project: project),
            project: static (state, source) => state.Project(arg1: state.Point, arg2: source).ToEff());
    internal static Fin<TOut> CurveAtNormalizedValue<TOut>(Curve curve, Context context, Op key, Func<Curve, double, TOut> project) =>
        curve.NormalizedLengthParameter(s: 0.5, t: out double parameter, fractionalTolerance: context.Relative.Value) switch {
            true => Fin.Succ(project(arg1: curve, arg2: parameter)),
            false => Fin.Fail<TOut>(key.InvalidResult()),
        };
    internal static Eff<Analyze.Env, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(
        TGeometry geometry,
        Op key,
        Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve => from context in Analyze.Asks
                           from validated in context.Validate(geometry: curve, requirement: Requirement.CurveLength).ToEff()
                           from value in CurveAtNormalizedValue(
                                   curve: validated, context: context, key: key, project: project)
                               .ToEff()
                           from result in One(key: key, value: value).ToEff()
                           select result,
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TOut))).ToEff(),
        };
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal<TMass>(
        this Op key,
        TMass mass) where TMass : class =>
        mass switch {
            LengthMassProperties length => key.PrincipalFromMoments(
                solved: length.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => key.PrincipalFromMoments(
                solved: area.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => key.PrincipalFromMoments(
                solved: volume.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalFromMoments(
        this Op key,
        bool solved,
        double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
}
