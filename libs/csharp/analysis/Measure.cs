using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

public static partial class Query {
    private delegate Fin<Seq<ResidualSample>> ResidualSampleCase<TGeometry, TPrimitive>(
        TGeometry geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context) where TGeometry : notnull where TPrimitive : notnull;
    private delegate Point3d ClosestPointCase<TPrimitive>(
        TPrimitive primitive,
        Point3d point) where TPrimitive : notnull;
    private delegate double DistanceCase<TPrimitive>(
        TPrimitive primitive,
        Point3d point) where TPrimitive : notnull;
    private delegate Fin<Seq<TValue>> ResidualProjection<TValue>(
        Seq<ResidualSample> samples,
        GeometryContext context);
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            BoundsKind.Box => Box<TGeometry, TOut>(),
            BoundsKind.Oriented => Oriented<TGeometry, TOut>(plane: aspect.Plane),
            BoundsKind.Transformed => Transformed<TGeometry, TOut>(transform: aspect.Transform),
            BoundsKind.Center when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCenterKey, query: Query<BoundingBox, Point3d>.Build(
                    key: BoundsCenterKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoundsCenterKey, value: geometry.Center))),
            BoundsKind.Corners when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCornersKey, query: Query<BoundingBox, Point3d>.Build(
                    key: BoundsCornersKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoundsCornersKey, values: geometry.GetCorners()))),
            BoundsKind.Edges when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line) =>
                Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                    key: BoxEdgesKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoxEdgesKey, values: geometry.GetEdges()))),
            BoundsKind.Area when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxAreaKey, query: Query<BoundingBox, double>.Build(
                    key: BoxAreaKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxAreaKey, value: geometry.Area))),
            BoundsKind.Volume when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxVolumeKey, query: Query<BoundingBox, double>.Build(
                    key: BoxVolumeKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxVolumeKey, value: geometry.Volume))),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        (aspect.Kind, aspect.Mass) switch {
            (_, MassKind.None) => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
            (MeasureKind.Scalar, MassKind.Length) => Length<TGeometry, TOut>(),
            (MeasureKind.Scalar, MassKind.Area) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Area)), query: AreaMass<TGeometry, double>(name: nameof(Analysis.Measure.Area), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Area))),
            (MeasureKind.Scalar, MassKind.Volume) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Volume)), query: VolumeMass<TGeometry, double>(name: nameof(Analysis.Measure.Volume), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Volume))),
            (MeasureKind.Error, MassKind.Length) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthError"), query: LengthMass<TGeometry, double>(name: "LengthError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.LengthError))),
            (MeasureKind.Error, MassKind.Area) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaError"), query: AreaMass<TGeometry, double>(name: "AreaError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.AreaError))),
            (MeasureKind.Error, MassKind.Volume) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeError"), query: VolumeMass<TGeometry, double>(name: "VolumeError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.VolumeError))),
            (MeasureKind.Centroid, MassKind.Length) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroid"), query: LengthMass<TGeometry, Point3d>(name: "LengthCentroid", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.Centroid, MassKind.Area) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroid"), query: AreaMass<TGeometry, Point3d>(name: "AreaCentroid", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.Centroid, MassKind.Volume) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroid"), query: VolumeMass<TGeometry, Point3d>(name: "VolumeCentroid", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.CentroidError, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroidError"), query: LengthMass<TGeometry, Vector3d>(name: "LengthCentroidError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.CentroidError, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroidError"), query: AreaMass<TGeometry, Vector3d>(name: "AreaCentroidError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.CentroidError, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroidError"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeCentroidError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.Radii, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthRadii"), query: LengthMass<TGeometry, Vector3d>(name: "LengthRadii", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Radii, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaRadii"), query: AreaMass<TGeometry, Vector3d>(name: "AreaRadii", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Radii, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeRadii"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeRadii", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Principal, MassKind.Length) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthPrincipal"), query: LengthMass<TGeometry, (double Moment, Vector3d Axis)>(name: "LengthPrincipal", project: static (OperationKey key, LengthMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (MeasureKind.Principal, MassKind.Area) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaPrincipal"), query: AreaMass<TGeometry, (double Moment, Vector3d Axis)>(name: "AreaPrincipal", project: static (OperationKey key, AreaMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (MeasureKind.Principal, MassKind.Volume) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumePrincipal"), query: VolumeMass<TGeometry, (double Moment, Vector3d Axis)>(name: "VolumePrincipal", project: static (OperationKey key, VolumeMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            _ => MeasureKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(
        Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        (aspect.Residual, aspect.Count, typeof(TGeometry), typeof(TPrimitive), typeof(TOut)) switch {
            (ConformanceResidual.None, _, _, _, _) or (_, <= 0, _, _, _) => Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(
                key: ConformanceKey,
                fault: ConformanceKey.InvalidInput()),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Line) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Line>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveLineSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Circle) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Circle>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveCircleSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Curve).IsAssignableFrom(c: geometry) && primitive == typeof(Arc) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Curve, Arc>(
                    aspect: aspect,
                    requirement: GeometryRequirement.CurveLength,
                    samples: CurveArcSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Plane) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Plane>(
                    aspect: aspect,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    samples: SurfacePlaneSamples),
            (_, _, Type geometry, Type primitive, _) when typeof(Surface).IsAssignableFrom(c: geometry) && primitive == typeof(Sphere) =>
                ConformanceCases<TGeometry, TPrimitive, TOut, Surface, Sphere>(
                    aspect: aspect,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    samples: SurfaceSphereSamples),
            _ => ConformanceKey.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceCases<TGeometry, TPrimitive, TOut, TNativeGeometry, TNativePrimitive>(
        Conformance aspect,
        GeometryRequirement requirement,
        ResidualSampleCase<TNativeGeometry, TNativePrimitive> samples) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        (aspect.Residual, typeof(TOut)) switch {
            (ConformanceResidual.Distance, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualDistance)),
            (ConformanceResidual.Rms, Type output) when output == typeof(double) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, double>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualRms)),
            (ConformanceResidual.WithinTolerance, Type output) when output == typeof(bool) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, bool>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualWithinTolerance)),
            (ConformanceResidual.Profile, Type output) when output == typeof(ResidualProfile) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualProfile>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualProfileProjection)),
            (ConformanceResidual.Maximum, Type output) when output == typeof(ResidualSample) =>
                Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: ConformanceKey, query: ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, ResidualSample>(
                    count: aspect.Count,
                    requirement: requirement,
                    samples: samples,
                    project: ResidualMaximum)),
            _ => ConformanceKey.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TNativeGeometry, TNativePrimitive, TValue>(
        int count,
        GeometryRequirement requirement,
        ResidualSampleCase<TNativeGeometry, TNativePrimitive> samples,
        ResidualProjection<TValue> project) where TGeometry : notnull where TPrimitive : notnull where TNativeGeometry : notnull where TNativePrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: ConformanceKey,
            requiresContext: true,
            state: (Count: count, Requirement: requirement, Samples: samples, Project: project),
            evaluator: static ((int Count, GeometryRequirement Requirement, ResidualSampleCase<TNativeGeometry, TNativePrimitive> Samples, ResidualProjection<TValue> Project) state, (TGeometry Geometry, TPrimitive Primitive) geometry, Fin<GeometryContext> context) => context
                .Bind((GeometryContext model) => model.ValidateOperands(
                        geometry: geometry,
                        a: state.Requirement,
                        b: GeometryRequirement.None)
                    .ToFin()
                    .Map(((TGeometry Geometry, TPrimitive Primitive) validated) => (
                        Context: model,
                        validated.Geometry,
                        validated.Primitive,
                        state.Count,
                        state.Samples,
                        state.Project)))
                .Bind(static ((GeometryContext Context, TGeometry Geometry, TPrimitive Primitive, int Count, ResidualSampleCase<TNativeGeometry, TNativePrimitive> Samples, ResidualProjection<TValue> Project) state) => (state.Geometry, state.Primitive) switch {
                    (TNativeGeometry geometry, TNativePrimitive primitive) => state.Samples(
                            geometry: geometry,
                            primitive: primitive,
                            count: state.Count,
                            context: state.Context)
                        .Bind((Seq<ResidualSample> values) => state.Project(
                            samples: values,
                            context: state.Context)),
                    _ => Fin.Fail<Seq<TValue>>(ConformanceKey.Unsupported(
                        geometryType: typeof((TGeometry Geometry, TPrimitive Primitive)),
                        outputType: typeof(TValue))),
                }));
    private static Fin<Seq<ResidualSample>> CurveLineSamples(Curve geometry, Line primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Line line, Point3d point) => line.ClosestPoint(
                testPoint: point,
                limitToFiniteSegment: false));
    private static Fin<Seq<ResidualSample>> CurveCircleSamples(Curve geometry, Circle primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Circle circle, Point3d point) => circle.ClosestPoint(testPoint: point));
    private static Fin<Seq<ResidualSample>> CurveArcSamples(Curve geometry, Arc primitive, int count, GeometryContext context) =>
        CurvePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            closest: static (Arc arc, Point3d point) => arc.ClosestPoint(testPoint: point));
    private static Fin<Seq<ResidualSample>> CurvePrimitiveSamples<TPrimitive>(
        Curve geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context,
        ClosestPointCase<TPrimitive> closest) where TPrimitive : notnull =>
        Fractions(count: count, key: ConformanceKey)
            .Bind((Seq<double> fractions) => fractions.Fold(
                    initialState: Fin.Succ(new ResidualState<Curve, TPrimitive>(
                        Samples: Seq<ResidualSample>(),
                        Geometry: geometry,
                        Primitive: primitive,
                        Context: context)),
                    f: (Fin<ResidualState<Curve, TPrimitive>> current, double fraction) => (
                            current,
                            Fin.Succ(fraction)
                        ).Apply((ResidualState<Curve, TPrimitive> state, double sample) => (
                            State: state,
                            Sample: sample))
                        .As()
                        .Bind(((ResidualState<Curve, TPrimitive> State, double Sample) pair) => pair.State.Geometry.NormalizedLengthParameter(
                            s: pair.Sample,
                            t: out double parameter,
                            fractionalTolerance: pair.State.Context.Relative.Value) switch {
                                true => pair.State.Geometry.PointAt(t: parameter) switch {
                                    Point3d point => closest(
                                            primitive: pair.State.Primitive,
                                            point: point) switch {
                                                Point3d closest => Fin.Succ(pair.State with {
                                                    Samples = pair.State.Samples.Add(new ResidualSample(
                                                        Index: pair.State.Samples.Count,
                                                        Location: point,
                                                        Distance: point.DistanceTo(other: closest),
                                                        Tolerance: pair.State.Context.Absolute.Value,
                                                        WithinTolerance: point.DistanceTo(other: closest) <= pair.State.Context.Absolute.Value)),
                                                }),
                                            },
                                },
                                false => Fin.Fail<ResidualState<Curve, TPrimitive>>(ConformanceKey.InvalidResult()),
                            }))
                .Map(static (ResidualState<Curve, TPrimitive> state) => state.Samples));
    private static Fin<Seq<ResidualSample>> SurfacePlaneSamples(Surface geometry, Plane primitive, int count, GeometryContext context) =>
        SurfacePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            distance: static (Plane plane, Point3d point) => Math.Abs(value: plane.DistanceTo(testPoint: point)));
    private static Fin<Seq<ResidualSample>> SurfaceSphereSamples(Surface geometry, Sphere primitive, int count, GeometryContext context) =>
        SurfacePrimitiveSamples(
            geometry: geometry,
            primitive: primitive,
            count: count,
            context: context,
            distance: static (Sphere sphere, Point3d point) => point.DistanceTo(other: sphere.ClosestPoint(testPoint: point)));
    private static Fin<Seq<ResidualSample>> SurfacePrimitiveSamples<TPrimitive>(
        Surface geometry,
        TPrimitive primitive,
        int count,
        GeometryContext context,
        DistanceCase<TPrimitive> distance) where TPrimitive : notnull =>
        (
            Samples(domain: geometry.Domain(direction: 0), count: count, key: ConformanceKey),
            Samples(domain: geometry.Domain(direction: 1), count: count, key: ConformanceKey)
        ).Apply(static (Seq<double> u, Seq<double> v) => (U: u, V: v)).As()
        .Bind(((Seq<double> U, Seq<double> V) samples) => samples.U
            .Bind((double u) => samples.V.Map((double v) => new Point2d(x: u, y: v)))
            .Fold(
                initialState: Fin.Succ(new ResidualState<Surface, TPrimitive>(
                    Samples: Seq<ResidualSample>(),
                    Geometry: geometry,
                    Primitive: primitive,
                    Context: context)),
                f: (Fin<ResidualState<Surface, TPrimitive>> current, Point2d uv) => current.Map((ResidualState<Surface, TPrimitive> state) => state.Geometry.PointAt(u: uv.X, v: uv.Y) switch {
                    Point3d point => state with {
                        Samples = state.Samples.Add(new ResidualSample(
                            Index: state.Samples.Count,
                            Location: point,
                            Distance: distance(primitive: state.Primitive, point: point),
                            Tolerance: state.Context.Absolute.Value,
                            WithinTolerance: distance(primitive: state.Primitive, point: point) <= state.Context.Absolute.Value)),
                    },
                })))
        .Map(static (ResidualState<Surface, TPrimitive> state) => state.Samples);
    private static Fin<Seq<double>> ResidualDistance(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples).Bind(static (Seq<double> residuals) => Many(key: ConformanceKey, values: residuals));
    private static Fin<Seq<double>> ResidualRms(Seq<ResidualSample> samples, GeometryContext _) =>
        ResidualDistances(samples: samples).Bind(static (Seq<double> residuals) => ResidualRmsDistances(residuals: residuals));
    private static Fin<Seq<bool>> ResidualWithinTolerance(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples).Bind((Seq<double> residuals) => ResidualWithinToleranceDistances(
            residuals: residuals,
            context: context));
    private static Fin<Seq<ResidualProfile>> ResidualProfileProjection(Seq<ResidualSample> samples, GeometryContext context) =>
        ResidualDistances(samples: samples).Bind((Seq<double> residuals) => ResidualProfileDistances(
            residuals: residuals,
            context: context));
    private static Fin<Seq<ResidualSample>> ResidualMaximum(Seq<ResidualSample> samples, GeometryContext _) =>
        samples.Fold(
            initialState: (Count: 0, Best: default, Valid: true),
            f: static ((int Count, ResidualSample Best, bool Valid) state, ResidualSample sample) => (
                Count: state.Count + 1,
                Best: (state.Count, sample.Distance > state.Best.Distance) switch {
                    (0, _) or (_, true) => sample,
                    _ => state.Best,
                },
                Valid: state.Valid && sample.Distance >= 0.0 && double.IsFinite(d: sample.Distance) && sample.Location.IsValid)) switch {
                    ( > 0, ResidualSample best, true) => Fin.Succ(Seq(best)),
                    _ => Fin.Fail<Seq<ResidualSample>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<double>> ResidualDistances(Seq<ResidualSample> samples) =>
        samples.Fold(
            initialState: Fin.Succ(Seq<double>()),
            f: static (Fin<Seq<double>> current, ResidualSample sample) => sample switch {
                { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && double.IsFinite(d: distance) =>
                    current.Map((Seq<double> values) => values.Add(distance)),
                _ => Fin.Fail<Seq<double>>(ConformanceKey.InvalidResult()),
            });
    private static Fin<Seq<double>> ResidualRmsDistances(Seq<double> residuals) =>
        residuals.Fold(
            initialState: (Count: 0, SumSquares: 0.0, Valid: true),
            f: static ((int Count, double SumSquares, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                SumSquares: state.SumSquares + (residual * residual),
                Valid: state.Valid && residual >= 0.0 && double.IsFinite(d: residual))) switch {
                    (int count, double sumSquares, true) when count > 0 && double.IsFinite(d: sumSquares) =>
                        One(key: ConformanceKey, value: Math.Sqrt(d: sumSquares / count)),
                    _ => Fin.Fail<Seq<double>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<bool>> ResidualWithinToleranceDistances(Seq<double> residuals, GeometryContext context) =>
        residuals.Fold(
            initialState: (Count: 0, Maximum: 0.0, Valid: true),
            f: static ((int Count, double Maximum, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                Maximum: Math.Max(val1: state.Maximum, val2: residual),
                Valid: state.Valid && residual >= 0.0 && double.IsFinite(d: residual))) switch {
                    (int count, double maximum, true) when count > 0 && double.IsFinite(d: maximum) =>
                        One(key: ConformanceKey, value: maximum <= context.Absolute.Value),
                    _ => Fin.Fail<Seq<bool>>(ConformanceKey.InvalidResult()),
                };
    private static Fin<Seq<ResidualProfile>> ResidualProfileDistances(Seq<double> residuals, GeometryContext context) {
        (int count, double minimum, double maximum, double sum, double sumSquares, bool valid) = residuals.Fold(
            initialState: (Count: 0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, Sum: 0.0, SumSquares: 0.0, Valid: true),
            f: static ((int Count, double Minimum, double Maximum, double Sum, double SumSquares, bool Valid) state, double residual) => (
                Count: state.Count + 1,
                Minimum: Math.Min(val1: state.Minimum, val2: residual),
                Maximum: Math.Max(val1: state.Maximum, val2: residual),
                Sum: state.Sum + residual,
                SumSquares: state.SumSquares + (residual * residual),
                Valid: state.Valid && residual >= 0.0 && double.IsFinite(d: residual)));
        double mean = count switch { > 0 => sum / count, _ => double.NaN };
        double variance = count switch {
            > 0 => residuals.Fold(
                initialState: (Mean: mean, Total: 0.0),
                f: static ((double Mean, double Total) state, double residual) => (
                    state.Mean,
                    state.Total + ((residual - state.Mean) * (residual - state.Mean)))).Total / count,
            _ => double.NaN,
        };
        double rms = count switch { > 0 => Math.Sqrt(d: sumSquares / count), _ => double.NaN };
        return (count, valid, double.IsFinite(d: minimum), double.IsFinite(d: maximum), double.IsFinite(d: mean), double.IsFinite(d: variance), double.IsFinite(d: rms), variance >= 0.0) switch {
            ( > 0, true, true, true, true, true, true, true) => Fin.Succ(Seq(new ResidualProfile(
                Count: count,
                Minimum: minimum,
                Maximum: maximum,
                Mean: mean,
                Variance: variance,
                Rms: rms,
                Tolerance: context.Absolute.Value,
                WithinTolerance: maximum <= context.Absolute.Value))),
            _ => Fin.Fail<Seq<ResidualProfile>>(ConformanceKey.InvalidResult()),
        };
    }
    private readonly record struct ResidualState<TGeometry, TPrimitive>(
        Seq<ResidualSample> Samples,
        TGeometry Geometry,
        TPrimitive Primitive,
        GeometryContext Context) where TGeometry : notnull where TPrimitive : notnull;
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, LengthMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, AreaMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, VolumeMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Query<TGeometry, TOut> Box<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(BoundingBox)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)) =>
                Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: BoundsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => One(key: BoundsKey, value: native.GetBoundingBox(accurate: true)),
                        Line line => One(key: BoundsKey, value: line.BoundingBox),
                        Polyline polyline => One(key: BoundsKey, value: polyline.BoundingBox),
                        BoundingBox box => One(key: BoundsKey, value: box),
                        _ => Fin.Fail<Seq<BoundingBox>>(BoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                    })),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Oriented<TGeometry, TOut>(Plane plane) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                Cast<TGeometry, TOut>(key: OrientedBoundsKey, query: Query<TGeometry, Box>.Build(
                    key: OrientedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => native.GetBoundingBox(plane: plane, worldBox: out Box box) switch {
                            BoundingBox local when local.IsValid => One(key: OrientedBoundsKey, value: box),
                            _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.InvalidResult()),
                        },
                        _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Box))),
                    })),
            _ => OrientedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Transformed<TGeometry, TOut>(Transform transform) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(BoundingBox) =>
                Cast<TGeometry, TOut>(key: TransformedBoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: TransformedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => One(key: TransformedBoundsKey, value: native.GetBoundingBox(xform: transform)),
                        _ => Fin.Fail<Seq<BoundingBox>>(TransformedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                    })),
            _ => TransformedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Line, double>.Build(
                    key: LengthKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Polyline, double>.Build(
                    key: LengthKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<TGeometry, double>.Build(
                    key: LengthKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> context) => (geometry, context) switch {
                        (Curve curve, Fin<GeometryContext> rail) => rail.Bind((GeometryContext model) => One(
                            key: LengthKey,
                            value: curve.GetLength(fractionalTolerance: model.Relative.Value))),
                        _ => Fin.Fail<Seq<double>>(LengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            _ => LengthKey.Unsupported<TGeometry, TOut>(),
        };
}
