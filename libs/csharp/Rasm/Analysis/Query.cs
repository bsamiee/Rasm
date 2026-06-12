namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public abstract partial record RelationQuery {
    private RelationQuery() { }
    public sealed record IntersectionCase : RelationQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationIntersection<TA, TB, TOut>(key: key); }
    public sealed record ClassificationCase : RelationQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationClassification<TA, TB, TOut>(key: key); }
    public sealed record DeviationCase : RelationQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationDeviation<TA, TB, TOut>(key: key); }
    public sealed record SelfIntersectionCase : RelationQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Analyze.RelationSelfIntersection<TGeometry, TOut>(key: key); }
    public sealed record RayCase(RayQuery Query) : RelationQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Analyze.RelationRay<TGeometry, TOut>(query: Query, key: key); }
    public sealed record ConformanceCase(ConformanceMetric Metric, int Count, Seq<double> Percentiles) : RelationQuery { internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Analyze.RelationConformance<TA, TB, TOut>(metric: Metric, count: Count, percentiles: Percentiles, key: key); }
    public static RelationQuery Intersections => new IntersectionCase();
    public static RelationQuery Classification => new ClassificationCase();
    public static RelationQuery CurveDeviation => new DeviationCase();
    public static RelationQuery SelfIntersection => new SelfIntersectionCase();
    public static RelationQuery Ray(RayQuery query) => new RayCase(Query: query);
    public static RelationQuery Conformance(ConformanceMetric metric, int count, params double[] percentiles) =>
        new ConformanceCase(Metric: metric, Count: count, Percentiles: Optional(metric).Bind(m => m.Equals(ConformanceMetric.Distribution) ? Some(toSeq(percentiles)) : Option<Seq<double>>.None).IfNone(Seq<double>()));
    internal virtual Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull => key.Unsupported<TGeometry, TOut>();
    internal virtual Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) where TA : notnull where TB : notnull where TOut : notnull => key.Unsupported<(TA A, TB B), TOut>();
}

[SkipUnionOps]
[Union]
public abstract partial record SpatialQuery {
    private SpatialQuery() { }
    public sealed record SearchBoxCase(Tree Index, BoundingBox Box) : SpatialQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialSearch<TOut>(index: Index, box: Box, key: key); }
    public sealed record SearchSphereCase(Tree Index, Sphere Sphere) : SpatialQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialSearch<TOut>(index: Index, sphere: Sphere, key: key); }
    public sealed record OverlapCase(Tree Left, Tree Right, double Tolerance) : SpatialQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialOverlaps<TOut>(left: Left, right: Right, tolerance: Tolerance, key: key); }
    public sealed record PointPairsCase(Seq<Point3d> Points, Seq<Point3d> Needles, SpatialProbe Probe) : SpatialQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Analyze.SpatialPointPairs<TOut>(points: Points, needles: Needles, probe: Probe, key: key); }
    public static SpatialQuery Search(Tree index, BoundingBox box) => new SearchBoxCase(Index: index, Box: box);
    public static SpatialQuery Search(Tree index, Sphere sphere) => new SearchSphereCase(Index: index, Sphere: sphere);
    public static SpatialQuery Overlaps(Tree left, Tree right, double tolerance = 0.0) => new OverlapCase(Left: left, Right: right, Tolerance: tolerance);
    public static SpatialQuery PointPairs(ReadOnlySpan<Point3d> points, ReadOnlySpan<Point3d> needles, SpatialProbe probe) => new PointPairsCase(Points: Seq(points), Needles: Seq(needles), Probe: probe);
    internal abstract Operation<Unit, TOut> Service<TOut>(Op key) where TOut : notnull;
}

[SkipUnionOps]
[Union]
public abstract partial record AnalysisQuery {
    private AnalysisQuery() { }
    public sealed record GeometryCase(GeometryRequest Request) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Analyze.GeometryRequestOp<TGeometry, TOut>(request: Request, key: key); }
    public sealed record BoundsCase(Bounds Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record MeasureCase(Measure Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record LocationCase(Location Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record CurvesCase(Curves Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record FacesCase(Faces Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record TopologyCase(Topologies Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record MeshesCase(Meshes Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record PointsCase(Points Query) : AnalysisQuery { internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Operation<TGeometry, TOut>(); }
    public sealed record RelationCase(RelationQuery Query) : AnalysisQuery {
        internal override Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) => Query.Single<TGeometry, TOut>(key: key);
        internal override Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) => Query.Pair<TA, TB, TOut>(key: key);
    }
    public sealed record SpatialCase(SpatialQuery Query) : AnalysisQuery { internal override Operation<Unit, TOut> Service<TOut>(Op key) => Query.Service<TOut>(key: key); }
    public static AnalysisQuery Geometry(GeometryRequest request) => new GeometryCase(Request: request);
    public static AnalysisQuery Measure(Bounds query) => new BoundsCase(Query: query);
    public static AnalysisQuery Measure(Measure query) => new MeasureCase(Query: query);
    public static AnalysisQuery Location(Location query) => new LocationCase(Query: query);
    public static AnalysisQuery Selection(Curves query) => new CurvesCase(Query: query);
    public static AnalysisQuery Selection(Faces query) => new FacesCase(Query: query);
    public static AnalysisQuery Selection(Topologies query) => new TopologyCase(Query: query);
    public static AnalysisQuery MeshPointSpatial(Meshes query) => new MeshesCase(Query: query);
    public static AnalysisQuery MeshPointSpatial(Points query) => new PointsCase(Query: query);
    public static AnalysisQuery Relation(RelationQuery query) => new RelationCase(Query: query);
    public static AnalysisQuery Spatial(SpatialQuery query) => new SpatialCase(Query: query);
    internal virtual Operation<TGeometry, TOut> Single<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull => key.Unsupported<TGeometry, TOut>();
    internal virtual Operation<(TA A, TB B), TOut> Pair<TA, TB, TOut>(Op key) where TA : notnull where TB : notnull where TOut : notnull => key.Unsupported<(TA A, TB B), TOut>();
    internal virtual Operation<Unit, TOut> Service<TOut>(Op key) where TOut : notnull => key.Unsupported<Unit, TOut>();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Query<TGeometry, TOut>(AnalysisQuery? query, Op? key = null) where TGeometry : notnull where TOut : notnull {
        Op active = key ?? Op.Of(name: nameof(Query));
        return Optional(query).Map(q => q.Single<TGeometry, TOut>(key: active)).IfNone(Operation<TGeometry, TOut>.Reject(key: active, fault: active.InvalidInput()));
    }

    public static Operation<(TA A, TB B), TOut> Query<TA, TB, TOut>(AnalysisQuery? query, Op? key = null) where TA : notnull where TB : notnull where TOut : notnull {
        Op active = key ?? Op.Of(name: nameof(Query));
        return Optional(query).Map(q => q.Pair<TA, TB, TOut>(key: active)).IfNone(Operation<(TA A, TB B), TOut>.Reject(key: active, fault: active.InvalidInput()));
    }

    public static Operation<Unit, TOut> Query<TOut>(AnalysisQuery? query, Op? key = null) where TOut : notnull {
        Op active = key ?? Op.Of(name: nameof(Query));
        return Optional(query).Map(q => q.Service<TOut>(key: active)).IfNone(Operation<Unit, TOut>.Reject(key: active, fault: active.InvalidInput()));
    }

    public static Validation<Error, Seq<TOut>> Run<TGeometry, TOut>(AnalysisQuery query, params ReadOnlySpan<TGeometry> input) where TGeometry : notnull where TOut : notnull =>
        Run(operation: Query<TGeometry, TOut>(query: query), input: input);

    public static Validation<Error, Seq<TOut>> Run<TA, TB, TOut>(AnalysisQuery query, params ReadOnlySpan<(TA A, TB B)> input) where TA : notnull where TB : notnull where TOut : notnull =>
        Run(operation: Query<TA, TB, TOut>(query: query), input: input);

    public static Validation<Error, Seq<TOut>> Run<TOut>(AnalysisQuery query) where TOut : notnull =>
        Run(operation: Query<TOut>(query: query), input: Unit.Default);

    internal static Operation<TGeometry, TOut> GeometryRequestOp<TGeometry, TOut>(GeometryRequest request, Op key) where TGeometry : notnull where TOut : notnull =>
        request switch {
            GeometryRequest.KindCase => Kind<TGeometry, TOut>(),
            GeometryRequest.CoerceCase coerce when coerce.Output == typeof(TOut) => GeometryCoerce<TGeometry, TOut>(key: key),
            GeometryRequest.BoundsCase => Analysis.Bounds.AxisAligned.Operation<TGeometry, TOut>(),
            GeometryRequest.CurveFormCase when typeof(TOut) == typeof(CurveForm) => GeometryCurveForm<TGeometry, TOut>(key: key),
            GeometryRequest.SurfaceFormCase when typeof(TOut) == typeof(Surface) => GeometryCoerce<TGeometry, TOut>(key: key),
            GeometryRequest.BrepFormCase when typeof(TOut) == typeof(Brep) => GeometryCoerce<TGeometry, TOut>(key: key),
            GeometryRequest.VerticesCase when typeof(TOut) == typeof(Point3d) => GeometryVertices<TGeometry, TOut>(key: key),
            GeometryRequest.SamplePointsCase samples when typeof(TOut) == typeof(Point3d) => GeometrySamples<TGeometry, TOut>(count: samples.Count, key: key),
            GeometryRequest.SurfaceUvCase uv when typeof(TOut) == typeof(Point2d) => GeometrySurfaceUv<TGeometry, TOut>(uv: uv.Uv, key: key),
            GeometryRequest.ClosestCase closest when typeof(TOut) == typeof(ClosestHit) => GeometryClosest<TGeometry, TOut>(target: closest.Target, key: key),
            GeometryRequest.SignedDistanceCase distance when typeof(TOut) == typeof(double) => GeometrySignedDistance<TGeometry, TOut>(sample: distance.Sample, hit: distance.Hit, key: key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };

    private static Operation<TGeometry, TOut> GeometryCoerce<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, TOut>.Build(key: key, requirement: Requirement.Basic, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Env.Asks
                from value in GeometryKernel.CoerceTo<TOut>(source: geometry, context: context, op: op).ToEff()
                from output in new AnalysisOutput<TOut>(Key: op).One(value: value).ToEff()
                select output);

    private static Operation<TGeometry, TOut> GeometryCurveForm<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, CurveForm>.Build(key: key, requirement: Requirement.Basic, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Env.Asks
                from form in GeometryKernel.CurveForm(source: geometry, op: op)
                    .Bind(lease => lease.Use(curve => GeometryKernel.CurveFormOf(curve: curve, context: context, op: op))).ToEff()
                from output in new AnalysisOutput<CurveForm>(Key: op).One(value: form).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    private static Operation<TGeometry, TOut> GeometryVertices<TGeometry, TOut>(Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point3d>.Build(key: key, state: key,
            evaluator: static (op, geometry) =>
                from points in GeometryKernel.VerticesOf(source: geometry, key: op).ToEff()
                from output in new AnalysisOutput<Point3d>(Key: op).Many(values: points).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    private static Operation<TGeometry, TOut> GeometrySamples<TGeometry, TOut>(int count, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point3d>.Build(key: key, requiresContext: true, state: (Key: key, Count: count),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from points in GeometryKernel.SamplePoints(source: geometry, count: state.Count, context: context, key: state.Key).ToEff()
                from output in new AnalysisOutput<Point3d>(Key: state.Key).Many(values: points).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    private static Operation<TGeometry, TOut> GeometrySurfaceUv<TGeometry, TOut>(Point2d uv, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, Point2d>.Build(key: key, requirement: Requirement.SurfaceEvaluation, requiresContext: true, state: (Key: key, Uv: uv),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in GeometryKernel.SurfaceForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(surface => GeometryKernel.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key))).ToEff()
                from output in new AnalysisOutput<Point2d>(Key: state.Key).One(value: result).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    private static Operation<TGeometry, TOut> GeometryClosest<TGeometry, TOut>(Point3d target, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, ClosestHit>.Build(key: key, requiresContext: true, state: (Key: key, Target: target),
            evaluator: static (state, geometry) =>
                from hit in GeometryKernel.ClosestOf(geometry: geometry, target: state.Target, key: state.Key).ToEff()
                from output in new AnalysisOutput<ClosestHit>(Key: state.Key).One(value: hit).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);

    private static Operation<TGeometry, TOut> GeometrySignedDistance<TGeometry, TOut>(Point3d sample, ClosestHit hit, Op key) where TGeometry : notnull where TOut : notnull =>
        Operation<TGeometry, double>.Build(key: key, state: (Key: key, Sample: sample, Hit: hit),
            evaluator: static (state, geometry) =>
                from distance in GeometryKernel.SignedDistanceOf(geometry: geometry, hit: state.Hit, sample: state.Sample, key: state.Key).ToEff()
                from output in new AnalysisOutput<double>(Key: state.Key).One(value: distance).ToEff()
                select output)
            .As<TGeometry, TOut>(key: key);
}
