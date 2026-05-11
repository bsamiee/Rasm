namespace Rasm.Analysis;

public sealed record Query<TGeometry, TOut> where TGeometry : notnull {
    internal Op Key { get; }
    internal Requirement Requirement { get; }
    internal bool RequiresContext { get; }
    internal Option<Error> Rejection { get; }
    private Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>> Evaluate { get; }
    private Option<Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>>> AggregatePlan { get; }
    internal Query(
        Op key,
        Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>> effect,
        Requirement? requirement = null,
        bool requiresContext = false,
        Option<Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>>> aggregate = default,
        Option<Error> rejection = default) {
        Key = key;
        Requirement = requirement ?? Requirement.None;
        RequiresContext = requiresContext;
        Rejection = rejection;
        Evaluate = effect;
        AggregatePlan = aggregate;
    }
    internal Eff<Analyze.Runtime, Seq<TOut>> Apply(TGeometry geometry) => Evaluate(arg: Seq(geometry));
    internal Eff<Analyze.Runtime, Seq<TOut>> Apply(Seq<TGeometry> geometry) => Evaluate(arg: geometry);
    internal Query<TIn, TOut> Contramap<TIn>(Func<TIn, TGeometry> map) where TIn : notnull =>
        new(
            key: Key,
            requirement: Requirement,
            requiresContext: RequiresContext,
            aggregate: AggregatePlan.Map<Func<Seq<TIn>, Eff<Analyze.Runtime, Seq<TOut>>>>(project => input => project(arg: input.Map(value => map(arg: value)))),
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
        Func<TGeometry, Eff<Analyze.Runtime, Seq<TOut>>> evaluator,
        Requirement? requirement = null,
        bool requiresContext = false,
        Option<Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>>> aggregate = default) {
        Requirement activeRequirement = requirement ?? Requirement.None;
        return new(
            key: key,
            requirement: activeRequirement,
            requiresContext: requiresContext,
            aggregate: aggregate.Map<Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TOut>>>>(project => geometry =>
                from runtime in Analyze.RuntimeAsks
                from resolved in geometry.Traverse(item => Ready(geometry: item).Run(env: runtime)).As().ToEff()
                from result in project(arg: resolved)
                select result),
            effect: geometry =>
                from runtime in Analyze.RuntimeAsks
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
        Func<TState, TGeometry, Eff<Analyze.Runtime, Seq<TOut>>> evaluator,
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
    private static Eff<Analyze.Runtime, TGeometry> Ready(TGeometry geometry) =>
        from runtime in Analyze.RuntimeAsks
        from resolved in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(OpFault.Cancelled()),
            false => Optional(geometry).ToFin(ValidationFault.MissingGeometry()),
        }).ToEff()
        select resolved;
    private static Eff<Analyze.Runtime, TGeometry> Validate(TGeometry geometry, Requirement requirement) =>
        (requirement.IsEmpty, geometry) switch {
            (false, GeometryBase native) =>
                from ctx in Analyze.Asks
                from _ in ctx.Validate(geometry: native, requirement: requirement).ToEff()
                select geometry,
            _ => Fin.Succ(geometry).ToEff(),
        };
}
[SmartEnum<int>]
public sealed partial class MassKind {
    private delegate Eff<Analyze.Runtime, IDisposable> ComputeMass(object geometry, bool secondMoments, bool productMoments);
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None, compute: static (geometry, _, _) => Fin.Fail<IDisposable>(OpFault.ComputationUnsupported(label: nameof(None), geometryType: geometry.GetType())).ToEff(), sum: static _ => Fin.Fail<IDisposable>(OpFault.ComputationFailed(label: nameof(None))));
    public static readonly MassKind Length = new(
        key: 1,
        label: nameof(Length),
        requirement: Requirement.CurveLength,
        compute: static (geometry, secondMoments, productMoments) => (geometry switch {
            Curve curve => Optional(LengthMassProperties.Compute(curve: curve, length: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments))
                .ToFin(OpFault.ComputationFailed(label: nameof(LengthMassProperties)))
                .Map(static props => (IDisposable)props),
            _ => Fin.Fail<IDisposable>(OpFault.ComputationUnsupported(label: nameof(LengthMassProperties), geometryType: geometry.GetType())),
        }).ToEff(),
        sum: static props => Optional(LengthMassProperties.WeightedSum(summands: props.AsIterable().Cast<LengthMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(OpFault.ComputationFailed(label: nameof(LengthMassProperties)))
            .Map(static props => (IDisposable)props));
    public static readonly MassKind Area = new(
        key: 2,
        label: nameof(Area),
        requirement: Requirement.AreaMass,
        compute: static (geometry, secondMoments, productMoments) =>
            from ctx in Analyze.Asks
            from props in Optional(geometry switch {
                Curve curve => AreaMassProperties.Compute(closedPlanarCurve: curve, planarTolerance: ctx.Absolute.Value),
                Mesh mesh => AreaMassProperties.Compute(mesh: mesh, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                Brep brep => AreaMassProperties.Compute(brep: brep, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: ctx.Relative.Value, absoluteTolerance: ctx.Absolute.Value),
                Surface surface => AreaMassProperties.Compute(surface: surface, area: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                _ => null,
            }).ToFin(geometry switch {
                Curve or Mesh or Brep or Surface => OpFault.ComputationFailed(label: nameof(AreaMassProperties)),
                _ => OpFault.ComputationUnsupported(label: nameof(AreaMassProperties), geometryType: geometry.GetType()),
            }).Map(static props => (IDisposable)props).ToEff()
            select props,
        sum: static props => Optional(AreaMassProperties.WeightedSum(summands: props.AsIterable().Cast<AreaMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(OpFault.ComputationFailed(label: nameof(AreaMassProperties)))
            .Map(static props => (IDisposable)props));
    public static readonly MassKind Volume = new(
        key: 3,
        label: nameof(Volume),
        requirement: Requirement.VolumeMass,
        compute: static (geometry, secondMoments, productMoments) =>
            from ctx in Analyze.Asks
            from props in Optional(geometry switch {
                Mesh mesh => VolumeMassProperties.Compute(mesh: mesh, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                Brep brep => VolumeMassProperties.Compute(brep: brep, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: ctx.Relative.Value, absoluteTolerance: ctx.Absolute.Value),
                Surface surface => VolumeMassProperties.Compute(surface: surface, volume: true, firstMoments: true, secondMoments: secondMoments, productMoments: productMoments),
                _ => null,
            }).ToFin(geometry switch {
                Mesh or Brep or Surface => OpFault.ComputationFailed(label: nameof(VolumeMassProperties)),
                _ => OpFault.ComputationUnsupported(label: nameof(VolumeMassProperties), geometryType: geometry.GetType()),
            }).Map(static props => (IDisposable)props).ToEff()
            select props,
        sum: static props => Optional(VolumeMassProperties.WeightedSum(summands: props.AsIterable().Cast<VolumeMassProperties>(), weights: Enumerable.Repeat(element: 1.0, count: props.Count)))
            .ToFin(OpFault.ComputationFailed(label: nameof(VolumeMassProperties)))
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
            aggregate: Some<Func<Seq<TGeometry>, Eff<Analyze.Runtime, Seq<TValue>>>>(geometry =>
                from props in geometry.Traverse(item => Compute(geometry: item, secondMoments: secondMoments, productMoments: productMoments)).As()
                from mass in Sum(arg: props).ToEff()
                from values in Query.Bracket(factory: () => mass, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                select values),
            evaluator: geometry =>
                from mass in Compute(geometry: geometry, secondMoments: secondMoments, productMoments: productMoments)
                from values in Query.Bracket(factory: () => mass, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                select values);
}
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
[SmartEnum<int>]
public sealed partial class MeshFaceMetric {
    public static readonly MeshFaceMetric None = new(key: 0, project: Option<Func<Mesh, int, double>>.None);
    public static readonly MeshFaceMetric AspectRatio = new(key: 1, project: Some<Func<Mesh, int, double>>(static (mesh, face) => mesh.Faces.GetFaceAspectRatio(index: face)));
    public Option<Func<Mesh, int, double>> Project { get; }
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
    public sealed record AllCase : Faces; public sealed record TopCase : Faces; public sealed record BottomCase : Faces; public sealed record AtCase(int? Value) : Faces;
    public static Faces All => new AllCase(); public static Faces Top => new TopCase(); public static Faces Bottom => new BottomCase();
    public static Faces At(int? index = null) => new AtCase(Value: index);
}
[SmartEnum<int>]
public sealed partial class MeshCheckCount {
    public static readonly MeshCheckCount None = new(key: 0, project: Option<Func<MeshCheckParameters, int>>.None);
    public static readonly MeshCheckCount DegenerateFaces = new(key: 1, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.DegenerateFaceCount)), DisjointMeshes = new(key: 2, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.DisjointMeshCount));
    public static readonly MeshCheckCount DuplicateFaces = new(key: 3, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.DuplicateFaceCount)), ExtremelyShortEdges = new(key: 4, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.ExtremelyShortEdgeCount));
    public static readonly MeshCheckCount InvalidNgons = new(key: 5, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.InvalidNgonCount)), NakedEdges = new(key: 6, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.NakedEdgeCount));
    public static readonly MeshCheckCount NonManifoldEdges = new(key: 7, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.NonManifoldEdgeCount)), NonUnitVectorNormals = new(key: 8, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.NonUnitVectorNormalCount));
    public static readonly MeshCheckCount RandomFaceNormals = new(key: 9, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.RandomFaceNormalCount)), SelfIntersectingPairs = new(key: 10, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.SelfIntersectingPairsCount));
    public static readonly MeshCheckCount UnusedVertices = new(key: 11, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.UnusedVertexCount)), VertexFaceNormalsDiffer = new(key: 12, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.VertexFaceNormalsDifferCount));
    public static readonly MeshCheckCount ZeroLengthNormals = new(key: 13, project: Some<Func<MeshCheckParameters, int>>(static parameters => parameters.ZeroLengthNormalCount));
    public Option<Func<MeshCheckParameters, int>> Project { get; }
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
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record ProfileResidual(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
}
public static partial class Query {
    internal delegate bool PrimitiveCase<TSource, TValue>(
        TSource geometry,
        Context context,
        out TValue value) where TSource : GeometryBase;
    internal static readonly Op
        MidpointKey = new(name: "Midpoint"), BoundsKey = new(name: nameof(Bounds)), OrientedBoundsKey = new(name: "OrientedBounds"),
        TransformedBoundsKey = new(name: "TransformedBounds"), BoundsCenterKey = new(name: "BoundsCenter"), BoundsCornersKey = new(name: "BoundsCorners"),
        BoxEdgesKey = new(name: "BoxEdges"), BoxAreaKey = new(name: "BoxArea"), BoxVolumeKey = new(name: "BoxVolume"), MeasureKey = new(name: nameof(Measure)),
        LengthKey = new(name: "Length"), TangentKey = new(name: "Tangent"), ClosestKey = new(name: "Closest"),
        DomainKey = new(name: nameof(Domain)), PointAtKey = new(name: "PointAt"), PointAtLengthKey = new(name: "PointAtLength"),
        FrameAtKey = new(name: "FrameAt"), PerpendicularFrameAtKey = new(name: "PerpendicularFrameAt"), NormalAtKey = new(name: "NormalAt"),
        CurvatureAtKey = new(name: "CurvatureAt"), DerivativeAtKey = new(name: "DerivativeAt"), DivideByCountKey = new(name: "DivideByCount"),
        DivideByLengthKey = new(name: "DivideByLength"), OrientationKey = new(name: "Orientation"), ContainsKey = new(name: "Contains"),
        SegmentsKey = new(name: nameof(Segments)), EdgesKey = new(name: nameof(Edges)), NakedEdgesKey = new(name: nameof(NakedEdges)),
        EdgeMidpointsKey = new(name: "EdgeMidpoints"), SpatialMidpointKey = new(name: "SpatialMidpoint"),
        OutlinesKey = new(name: nameof(Outlines)), IsoKey = new(name: nameof(Iso)), PrimitiveKey = new(name: "Primitive"),
        ShortPathKey = new(name: "ShortPath"), SolidOrientationKey = new(name: nameof(SolidOrientation)), IsPointInsideKey = new(name: nameof(IsPointInside)),
        VerticesKey = new(name: nameof(Vertices)), ComponentsKey = new(name: "Components"), IsManifoldKey = new(name: nameof(IsManifold)),
        NakedPointStatusKey = new(name: nameof(NakedPointStatus)), MeshCheckKey = new(name: nameof(MeshCheck)), MeshCheckCountKey = new(name: "MeshCheckCount"), MeshFaceMetricKey = new(name: nameof(MeshFaceMetric)), SelfIntersectionsKey = new(name: nameof(SelfIntersections)), IntersectKey = new(name: nameof(Intersect)),
        ConformanceKey = new(name: nameof(Conformance)), DeviationKey = new(name: nameof(Deviation)), TreeKey = new(name: nameof(Tree)),
        ScopeKey = new(name: nameof(Analyze.Scope)),
        KindKey = new(name: nameof(Kind)),
        UniqueCornersKey = new(name: "UniqueCorners"),
        QuadrantsKey = new(name: nameof(Quadrants)),
        FacesKey = new(name: nameof(Faces)),
        CurvesKey = new(name: nameof(Curves)),
        ControlPointsKey = new(name: "ControlPoints");
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(
            key: key,
            fault: key.Unsupported(
                geometryType: typeof(TGeometry),
                outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Aspect<TGeometry, TOut, TAspect>(
        TAspect aspect,
        Op key,
        Func<TAspect, Query<TGeometry, TOut>?> dispatch) where TGeometry : notnull where TAspect : notnull =>
        Optional(dispatch(arg: aspect))
            .IfNone(() => key.Unsupported<TGeometry, TOut>());
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(
        Op key,
        object query) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> typed => typed,
            _ => Query<TGeometry, TOut>.Reject(
                key: key,
                fault: key.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TOut))),
        };
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue>(
        Op key,
        Func<TNative, Eff<Analyze.Runtime, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, Func<TNative, Eff<Analyze.Runtime, Seq<TValue>>>>(
            key: key,
            state: project,
            project: static (nativeProject, native) => nativeProject(arg: native));
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(
        Op key,
        TState state,
        Func<TState, TNative, Eff<Analyze.Runtime, Seq<TValue>>> project,
        Requirement? requirement = null,
        bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key,
            requirement: requirement,
            requiresContext: requiresContext,
            state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TValue))).ToEff(),
            }));
    internal static Fin<Seq<TValue>> One<TValue>(this Op key, TValue value) => key.RequireValid(value: value).Map(static candidate => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue>? values) =>
        Optional(values)
            .ToSeq()
            .Bind(static value => value.AsIterable().ToSeq())
            .Traverse(value => key.RequireValid(value: value))
            .As();
    internal static Fin<Seq<TValue>> Solved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch {
            true => key.One(value: value),
            false => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    internal static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    internal static Fin<Seq<TOut>> IntersectionOutput<TOut>(
        this Op key,
        IEnumerable<Curve>? curves = null,
        IEnumerable<Line>? lines = null,
        IEnumerable<Circle>? circles = null,
        IEnumerable<Point3d>? points = null,
        IEnumerable<Polyline>? polylines = null,
        IEnumerable<Interval>? intervals = null,
        IEnumerable<IntersectionKind>? kinds = null,
        CurveIntersections? intersections = null) =>
        typeof(TOut) switch {
            Type output when output == typeof(Curve) => key.Results<Curve, TOut>(values: curves),
            Type output when output == typeof(Line) => key.Results<Line, TOut>(values: lines),
            Type output when output == typeof(Circle) => key.Results<Circle, TOut>(values: circles),
            Type output when output == typeof(Interval) => key.Results<Interval, TOut>(values: intervals),
            Type output when output == typeof(Point3d) => key.Results<Point3d, TOut>(values: points ?? Optional(intersections).ToSeq().Bind(static events => events).Where(static intersection => intersection.IsPoint).Select(static intersection => intersection.PointA)),
            Type output when output == typeof(IntersectionEvent) => key.Results<IntersectionEvent, TOut>(values: Optional(intersections).ToSeq().Bind(static events => events)),
            Type output when output == typeof(Polyline) => key.Results<Polyline, TOut>(values: polylines),
            Type output when output == typeof(IntersectionKind) =>
                key.Results<IntersectionKind, TOut>(values: Optional(intersections).ToSeq().Bind(static events => events)
                    .Select(static intersection => intersection switch {
                        IntersectionEvent candidate when candidate.IsOverlap => IntersectionKind.Overlap,
                        IntersectionEvent candidate when candidate.IsPoint => IntersectionKind.Point,
                        _ => IntersectionKind.Unknown,
                    })
                    .Concat(second: Classify(values: curves, kind: IntersectionKind.Overlap))
                    .Concat(second: Classify(values: lines, kind: IntersectionKind.Curve))
                    .Concat(second: Classify(values: circles, kind: IntersectionKind.Curve))
                    .Concat(second: Classify(values: points, kind: IntersectionKind.Point))
                    .Concat(second: Classify(values: intervals, kind: IntersectionKind.Overlap))
                    .Concat(second: Optional(kinds).IfNone(() => Classify(values: polylines, kind: IntersectionKind.Overlap)))),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        };
    private static Seq<IntersectionKind> Classify<TValue>(IEnumerable<TValue>? values, IntersectionKind kind) =>
        toSeq(Optional(values).ToSeq().Bind(static source => source).Select(_ => kind));
    internal static Fin<Seq<TOut>> Results<TValue, TOut>(
        this Op key,
        IEnumerable<TValue>? values) =>
        typeof(TValue).Equals(typeof(TOut)) switch {
            true => Many(key: key, values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
            false => Fin.Fail<Seq<TOut>>(key.Unsupported(
                geometryType: typeof(void),
                outputType: typeof(TOut))),
        };
    internal static Query<TGeometry, TOut> PrimitiveMatch<TGeometry, TOut, TSource, TValue>(
        PrimitiveCase<TSource, TValue> project) where TGeometry : notnull where TSource : GeometryBase =>
        Native<TGeometry, TOut, TSource, TValue, PrimitiveCase<TSource, TValue>>(
            key: PrimitiveKey,
            state: project,
            requiresContext: true,
            project: static (extract, source) =>
                from ctx in Analyze.Asks
                from validated in ctx.Validate(geometry: source, requirement: Requirement.Basic).ToEff()
                from result in (extract(geometry: validated, context: ctx, value: out TValue value) switch {
                    bool solved => PrimitiveKey.Solved(isSolved: solved, value: value),
                }).ToEff()
                select result);
    internal static Query<TGeometry, TOut> ClosestMatch<TGeometry, TOut, TSource, TValue>(
        Point3d point,
        Func<Point3d, TSource, Fin<Seq<TValue>>> project) where TGeometry : notnull where TSource : notnull =>
        Native<TGeometry, TOut, TSource, TValue, (Point3d Point, Func<Point3d, TSource, Fin<Seq<TValue>>> Project)>(
            key: ClosestKey,
            state: (Point: point, Project: project),
            project: static (state, source) => state.Project(arg1: state.Point, arg2: source).ToEff());
    internal static Fin<TOut> CurveAtNormalizedValue<TOut>(
        Curve curve,
        Context context,
        Op key,
        Func<Curve, double, TOut> project) =>
        curve.NormalizedLengthParameter(
            s: 0.5,
            t: out double parameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => Fin.Succ(project(arg1: curve, arg2: parameter)),
                false => Fin.Fail<TOut>(key.InvalidResult()),
            };
    internal static Eff<Analyze.Runtime, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(
        TGeometry geometry,
        Op key,
        Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve =>
                from ctx in Analyze.Asks
                from validated in ctx.Validate(geometry: curve, requirement: Requirement.CurveLength).ToEff()
                from value in CurveAtNormalizedValue(
                        curve: validated,
                        context: ctx,
                        key: key,
                        project: project)
                    .ToEff()
                from result in One(key: key, value: value).ToEff()
                select result,
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))).ToEff(),
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
