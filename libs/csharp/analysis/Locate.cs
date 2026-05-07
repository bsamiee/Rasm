using System.Linq;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static partial class Query {
    public static Query<TGeometry, TOut> Locate<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            LocationKind.Midpoint => Mid<TGeometry, TOut>(),
            LocationKind.Tangent => TangentAtMiddle<TGeometry, TOut>(),
            LocationKind.Closest => Closest<TGeometry, TOut>(point: aspect.Point),
            LocationKind.CurvatureProfile => CurvatureProfile<TGeometry, TOut>(count: aspect.Count, scalar: aspect.Scalar),
            LocationKind.PointAtCurve or LocationKind.PointAtLength or LocationKind.FrameAtCurve
                or LocationKind.PerpendicularFrameAt or LocationKind.CurvatureAtCurve
                or LocationKind.DerivativeAt or LocationKind.DivideByCount
                or LocationKind.DivideByLength or LocationKind.Orientation
                or LocationKind.Contains => CurveLocation<TGeometry, TOut>(aspect: aspect),
            LocationKind.PointAtSurface or LocationKind.FrameAtSurface or LocationKind.NormalAt
                or LocationKind.CurvatureAtSurface or LocationKind.ShortPath =>
                SurfaceLocation<TGeometry, TOut>(aspect: aspect),
            _ => PointAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> CurveLocation<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            LocationKind.PointAtCurve when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtKey, query: CurveAt<TGeometry, Point3d>(
                    key: PointAtKey,
                    parameter: aspect.Parameter,
                    project: static (Curve curve, double parameter) => One(key: PointAtKey, value: curve.PointAt(t: parameter)))),
            LocationKind.PointAtLength when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtLengthKey, query: Query<TGeometry, Point3d>.Build(
                    key: PointAtLengthKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> context) => context.Bind((GeometryContext model) => geometry switch {
                        Curve curve => curve.LengthParameter(
                            segmentLength: aspect.Parameter,
                            t: out double parameter,
                            fractionalTolerance: model.Relative.Value) switch {
                                true => One(key: PointAtLengthKey, value: curve.PointAt(t: parameter)),
                                false => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.InvalidResult()),
                            },
                        _ => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    }))),
            LocationKind.FrameAtCurve when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: FrameAtKey, query: CurveFrame<TGeometry>(key: FrameAtKey, parameter: aspect.Parameter, perpendicular: false)),
            LocationKind.PerpendicularFrameAt when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: PerpendicularFrameAtKey, query: CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: aspect.Parameter, perpendicular: true)),
            LocationKind.CurvatureAtCurve when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: CurveAt<TGeometry, Vector3d>(
                    key: CurvatureAtKey,
                    parameter: aspect.Parameter,
                    project: static (Curve curve, double parameter) => One(key: CurvatureAtKey, value: curve.CurvatureAt(t: parameter)))),
            LocationKind.DerivativeAt when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: DerivativeAtKey, query: CurveAt<TGeometry, Vector3d>(
                    key: DerivativeAtKey,
                    parameter: aspect.Parameter,
                    project: (Curve curve, double parameter) => Many(key: DerivativeAtKey, values: curve.DerivativeAt(t: parameter, derivativeCount: aspect.Count)))),
            _ => CurveDerivedLocation<TGeometry, TOut>(aspect: aspect),
        };
    private static Query<TGeometry, TOut> CurveDerivedLocation<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            LocationKind.DivideByCount when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: DivideByCountKey, query: Divide<TGeometry>(count: aspect.Count)),
            LocationKind.DivideByLength when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: DivideByLengthKey, query: Divide<TGeometry>(length: aspect.Parameter)),
            LocationKind.Orientation when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(CurveOrientation) =>
                Cast<TGeometry, TOut>(key: OrientationKey, query: Query<TGeometry, CurveOrientation>.Build(
                    key: OrientationKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => One(key: OrientationKey, value: curve.ClosedCurveOrientation(plane: aspect.Plane)),
                        _ => Fin.Fail<Seq<CurveOrientation>>(OrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))),
                    })),
            LocationKind.Contains when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(PointContainment) =>
                Cast<TGeometry, TOut>(key: ContainsKey, query: Query<TGeometry, PointContainment>.Build(
                    key: ContainsKey,
                    requiresContext: true,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> context) => context.Bind((GeometryContext model) => geometry switch {
                        Curve curve => curve.Contains(testPoint: aspect.Point, plane: aspect.Plane, tolerance: model.Absolute.Value) switch {
                            PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(ContainsKey.InvalidResult()),
                            PointContainment containment => One(key: ContainsKey, value: containment),
                        },
                        _ => Fin.Fail<Seq<PointContainment>>(ContainsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))),
                    }))),
            _ => PointAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> SurfaceLocation<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            LocationKind.PointAtSurface when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtKey, query: SurfaceUv<TGeometry, Point3d>(
                    key: PointAtKey,
                    uv: aspect.Uv,
                    project: static (Surface geometry, Point2d parameter) => One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
            LocationKind.FrameAtSurface when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: FrameAtKey, query: SurfaceUv<TGeometry, Plane>(
                    key: FrameAtKey,
                    uv: aspect.Uv,
                    project: static (Surface geometry, Point2d parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                        true => One(key: FrameAtKey, value: frame),
                        false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                    })),
            LocationKind.NormalAt when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: NormalAtKey, query: SurfaceUv<TGeometry, Vector3d>(
                    key: NormalAtKey,
                    uv: aspect.Uv,
                    project: static (Surface geometry, Point2d parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                        Vector3d normal when normal.IsValid && !normal.IsTiny() => One(key: NormalAtKey, value: normal),
                        _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
                    })),
            LocationKind.CurvatureAtSurface when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(SurfaceCurvature) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: SurfaceUv<TGeometry, SurfaceCurvature>(
                    key: CurvatureAtKey,
                    uv: aspect.Uv,
                    project: static (Surface geometry, Point2d parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y))
                        .ToFin(CurvatureAtKey.InvalidResult())
                        .Map(static (SurfaceCurvature curvature) => Seq(curvature)))),
            LocationKind.ShortPath when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: ShortPathKey, query: ShortPath<TGeometry>(start: aspect.Uv, end: aspect.End)),
            _ => PointAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Line, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => geometry.IsValid switch {
                        true => One(key: MidpointKey, value: geometry.PointAt(t: 0.5)),
                        false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                    })),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Polyline, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => {
                        using PolylineCurve curve = geometry.ToPolylineCurve();
                        return curve.IsValid switch {
                            true => One(key: MidpointKey, value: curve.PointAtNormalizedLength(length: 0.5)),
                            false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                        };
                    })),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<TGeometry, Point3d>.Build(
                    key: MidpointKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> context) => CurveAtNormalized<TGeometry, Point3d>(
                        geometry: geometry,
                        context: context,
                        key: MidpointKey,
                        project: static (Curve curve, double parameter) => curve.PointAt(t: parameter)))),
            _ => MidpointKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Line, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => One(key: TangentKey, value: geometry.UnitTangent))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Polyline, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => {
                        using PolylineCurve curve = geometry.ToPolylineCurve();
                        return curve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                            true => One(key: TangentKey, value: curve.TangentAt(t: parameter)),
                            false => Fin.Fail<Seq<Vector3d>>(TangentKey.InvalidResult()),
                        };
                    })),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<TGeometry, Vector3d>.Build(
                    key: TangentKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> context) => CurveAtNormalized<TGeometry, Vector3d>(
                        geometry: geometry,
                        context: context,
                        key: TangentKey,
                        project: static (Curve curve, double parameter) => curve.TangentAt(t: parameter)))),
            _ => TangentKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> CurvatureProfile<TGeometry, TOut>(int count, CurvatureScalar scalar) where TGeometry : notnull =>
        count switch {
            <= 0 => Query<TGeometry, TOut>.Reject(key: CurvatureAtKey, fault: CurvatureAtKey.InvalidInput()),
            _ => CurvatureProfileReady<TGeometry, TOut>(count: count, scalar: scalar),
        };
    private static Query<TGeometry, TOut> CurvatureProfileReady<TGeometry, TOut>(int count, CurvatureScalar scalar) where TGeometry : notnull =>
        (scalar, typeof(TGeometry), typeof(TOut)) switch {
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, Vector3d>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry, Fin<GeometryContext> context) => (geometry, context) switch {
                        (Curve curve, Fin<GeometryContext> rail) => rail.Bind((GeometryContext model) => CurveCurvatures(
                            curve: curve,
                            count: sampleCount,
                            model: model)),
                        _ => Fin.Fail<Seq<Vector3d>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Vector3d))),
                    })),
            (CurvatureScalar.None or CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry, Fin<GeometryContext> context) => (geometry, context) switch {
                        (Curve curve, Fin<GeometryContext> rail) => rail
                            .Bind((GeometryContext model) => CurveScalarProfile(curve: curve, count: sampleCount, model: model, scalar: CurvatureScalar.Magnitude))
                            .Bind(static (Seq<double> values) => Profile(
                                    scalar: CurvatureScalar.Magnitude,
                                    values: values)
                                .Map(static (CurvatureProfile profile) => Seq(profile))),
                        _ => Fin.Fail<Seq<CurvatureProfile>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, double>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry, Fin<GeometryContext> context) => (geometry, context) switch {
                        (Curve curve, Fin<GeometryContext> rail) => rail
                            .Bind((GeometryContext model) => CurveScalarProfile(curve: curve, count: sampleCount, model: model, scalar: CurvatureScalar.Magnitude))
                            .Bind(static (Seq<double> values) => Many(
                                key: CurvatureAtKey,
                                values: values)),
                        _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, SurfaceCurvature>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Surface surface => context.Bind((GeometryContext model) => SurfaceCurvatures(
                            surface: surface,
                            count: sampleCount,
                            model: model)),
                        _ => Fin.Fail<Seq<SurfaceCurvature>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(SurfaceCurvature))),
                    })),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Surface surface => context
                            .Bind((GeometryContext model) => SurfaceCurvatures(
                                surface: surface,
                                count: sampleCount,
                                model: model))
                            .Bind(static (Seq<SurfaceCurvature> curvatures) => (
                                SurfaceScalars(
                                    curvatures: curvatures,
                                    scalar: CurvatureScalar.Gaussian).Bind(static (Seq<double> values) => Profile(
                                    scalar: CurvatureScalar.Gaussian,
                                    values: values)),
                                SurfaceScalars(
                                    curvatures: curvatures,
                                    scalar: CurvatureScalar.Mean).Bind(static (Seq<double> values) => Profile(
                                    scalar: CurvatureScalar.Mean,
                                    values: values))
                            ).Apply(static (CurvatureProfile gaussian, CurvatureProfile mean) => Seq(gaussian, mean)).As()),
                        _ => Fin.Fail<Seq<CurvatureProfile>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: (Count: count, Scalar: scalar),
                    evaluator: static ((int Count, CurvatureScalar Scalar) state, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Surface surface => context
                            .Bind((GeometryContext model) => SurfaceScalarProfile(surface: surface, count: state.Count, model: model, scalar: state.Scalar))
                            .Bind((Seq<double> values) => Profile(
                                    scalar: state.Scalar,
                                    values: values)
                                .Map(static (CurvatureProfile profile) => Seq(profile))),
                        _ => Fin.Fail<Seq<CurvatureProfile>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, double>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: (Count: count, Scalar: scalar),
                    evaluator: static ((int Count, CurvatureScalar Scalar) state, TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                        Surface surface => context
                            .Bind((GeometryContext model) => SurfaceScalarProfile(surface: surface, count: state.Count, model: model, scalar: state.Scalar))
                            .Bind(static (Seq<double> values) => Many(
                                key: CurvatureAtKey,
                                values: values)),
                        _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<double>> CurveScalarProfile(Curve curve, int count, GeometryContext model, CurvatureScalar scalar) =>
        scalar switch {
            CurvatureScalar.Magnitude => CurveCurvatures(curve: curve, count: count, model: model)
                .Map(static (Seq<Vector3d> values) => values.Map(static (Vector3d value) => value.Length)),
            _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(Curve), outputType: typeof(double))),
        };
    private static Fin<Seq<double>> SurfaceScalarProfile(Surface surface, int count, GeometryContext model, CurvatureScalar scalar) =>
        SurfaceCurvatures(surface: surface, count: count, model: model).Bind((Seq<SurfaceCurvature> curvatures) => SurfaceScalars(curvatures: curvatures, scalar: scalar));
    private static Fin<Seq<double>> SurfaceScalars(Seq<SurfaceCurvature> curvatures, CurvatureScalar scalar) =>
        scalar switch {
            CurvatureScalar.Gaussian => Fin.Succ(curvatures.Map(static (SurfaceCurvature curvature) => curvature.Gaussian)),
            CurvatureScalar.Mean => Fin.Succ(curvatures.Map(static (SurfaceCurvature curvature) => curvature.Mean)),
            _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Vector3d>> CurveCurvatures(Curve curve, int count, GeometryContext model) =>
        CurveSamples(
                curve: curve,
                count: count,
                model: model,
                key: CurvatureAtKey)
            .Bind((Seq<double> parameters) => Many(
                key: CurvatureAtKey,
                values: parameters.Map((double parameter) => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Surface surface, int count, GeometryContext model) =>
        (
            Samples(domain: surface.Domain(direction: 0), count: count, key: CurvatureAtKey),
            Samples(domain: surface.Domain(direction: 1), count: count, key: CurvatureAtKey)
        ).Apply(static (Seq<double> u, Seq<double> v) => (U: u, V: v)).As()
        .Bind(((Seq<double> U, Seq<double> V) samples) =>
            samples.U
                .Bind((double u) => samples.V.Map((double v) => new Point2d(x: u, y: v)))
                .Fold(
                    Fin.Succ(Seq<SurfaceCurvature>()),
                    (Fin<Seq<SurfaceCurvature>> current, Point2d uv) => (
                        current,
                        Uv(surface: surface, uv: uv, context: model, key: CurvatureAtKey)
                            .Bind((Point2d parameter) => Optional(surface.CurvatureAt(u: parameter.X, v: parameter.Y))
                                .ToFin(CurvatureAtKey.InvalidResult()))
                    ).Apply(static (Seq<SurfaceCurvature> previous, SurfaceCurvature next) => previous.Add(next)).As()));
    private static Fin<Seq<double>> CurveSamples(Curve curve, int count, GeometryContext model, OperationKey key) =>
        Fractions(count: count, key: key)
            .Bind((Seq<double> fractions) => fractions.Fold(
                Fin.Succ(Seq<double>()),
                (Fin<Seq<double>> current, double fraction) => (
                    current,
                    curve.NormalizedLengthParameter(
                        s: fraction,
                        t: out double parameter,
                        fractionalTolerance: model.Relative.Value) switch {
                            true => Fin.Succ(parameter),
                            false => Fin.Fail<double>(key.InvalidResult()),
                        }
                ).Apply(static (Seq<double> previous, double next) => previous.Add(next)).As()));
    private static Fin<Seq<double>> Fractions(int count, OperationKey key) =>
        count switch {
            1 => Fin.Succ(Seq(0.5)),
            > 1 => Fin.Succ(Enumerable
                .Range(start: 0, count: count)
                .Aggregate(
                    seed: (Samples: Seq<double>(), Denominator: count - 1.0),
                    func: static ((Seq<double> Samples, double Denominator) state, int index) => (
                        Samples: state.Samples.Add(index / state.Denominator),
                        state.Denominator)).Samples),
            _ => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Fin<CurvatureProfile> Profile(CurvatureScalar scalar, Seq<double> values) {
        (int count, double minimum, double maximum, double sum, bool valid) = values.Fold(
            initialState: (Count: 0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, Sum: 0.0, Valid: true),
            f: static ((int Count, double Minimum, double Maximum, double Sum, bool Valid) state, double value) => (
                Count: state.Count + 1,
                Minimum: Math.Min(val1: state.Minimum, val2: value),
                Maximum: Math.Max(val1: state.Maximum, val2: value),
                Sum: state.Sum + value,
                Valid: state.Valid && double.IsFinite(d: value)));
        double mean = count switch { > 0 => sum / count, _ => double.NaN };
        double variance = count switch {
            > 0 => values.Fold(
                initialState: (Mean: mean, Total: 0.0),
                f: static ((double Mean, double Total) state, double value) => (
                    state.Mean,
                    state.Total + ((value - state.Mean) * (value - state.Mean)))).Total / count,
            _ => double.NaN,
        };
        return (count > 0, valid, double.IsFinite(d: mean), double.IsFinite(d: variance), variance >= 0.0) switch {
            (true, true, true, true, true) => Fin.Succ(new CurvatureProfile(Scalar: scalar, Count: count, Minimum: minimum, Maximum: maximum, Mean: mean, Variance: variance)),
            _ => Fin.Fail<CurvatureProfile>(CurvatureAtKey.InvalidResult()),
        };
    }
    private static Fin<Seq<double>> Samples(Interval domain, int count, OperationKey key) =>
        (domain.IsValid, count) switch {
            (true, 1) => Fin.Succ(Seq(domain.Mid)),
            (true, > 1) => Fin.Succ(Enumerable
                .Range(start: 0, count: count)
                .Aggregate(
                    seed: Seq<double>(),
                    func: (Seq<double> samples, int index) => samples.Add(domain.ParameterAt(index / (count - 1.0))))),
            _ => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => ClosestPoint<TGeometry, TOut>(point: point),
            _ => ClosestDetail<TGeometry, TOut>(point: point),
        };
    private static Query<TGeometry, TOut> ClosestPoint<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Line, Point3d>(point: point, project: static (Point3d target, Line geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target, limitToFiniteSegment: true))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Polyline, Point3d>(point: point, project: static (Point3d target, Polyline geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Curve, Point3d>(point: point, project: static (Point3d target, Curve geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(t: parameter)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Surface, Point3d>(point: point, project: static (Point3d target, Surface geometry) => geometry.ClosestPoint(testPoint: target, u: out double u, v: out double v) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(u: u, v: v)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Point3d>(point: point, project: static (Point3d target, Brep geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Point3d>(point: point, project: static (Point3d target, Mesh geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            _ => ClosestKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> ClosestDetail<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                ClosestMatch<TGeometry, TOut, Curve, double>(point: point, project: static (Point3d target, Curve geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: parameter),
                    false => Fin.Fail<Seq<double>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Vector3d>(point: point, project: static (Point3d target, Brep geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex _, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d normal) switch {
                    true => One(key: ClosestKey, value: normal),
                    false => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Vector3d>(point: point, project: static (Point3d target, Mesh geometry) => geometry.ClosestPoint(testPoint: target, pointOnMesh: out Point3d _, normalAtPoint: out Vector3d normal, maximumDistance: 0.0) switch {
                    >= 0 => One(key: ClosestKey, value: normal),
                    _ => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                ClosestMatch<TGeometry, TOut, Brep, ComponentIndex>(point: point, project: static (Point3d target, Brep geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex component, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d _) switch {
                    true => One(key: ClosestKey, value: component),
                    false => Fin.Fail<Seq<ComponentIndex>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(MeshPoint) =>
                ClosestMatch<TGeometry, TOut, Mesh, MeshPoint>(point: point, project: static (Point3d target, Mesh geometry) => Optional(geometry.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0))
                    .ToFin(ClosestKey.InvalidResult())
                    .Map(static (MeshPoint meshPoint) => Seq(meshPoint))),
            _ => ClosestKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, Plane> CurveFrame<TGeometry>(OperationKey key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(
            key: key,
            parameter: parameter,
            project: (Curve curve, double t) => perpendicular switch {
                true => key.Solved(
                    solved: curve.PerpendicularFrameAt(t: t, plane: out Plane frame),
                    value: frame),
                false => key.Solved(
                    solved: curve.FrameAt(t: t, plane: out Plane frame),
                    value: frame),
            });
    private static Query<TGeometry, TOut> CurveAt<TGeometry, TOut>(
        OperationKey key,
        double parameter,
        Func<Curve, double, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: key,
            evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                Curve curve => curve.Domain.IncludesParameter(t: parameter) switch {
                    true => project(arg1: curve, arg2: parameter),
                    false => Fin.Fail<Seq<TOut>>(key.InvalidInput()),
                },
                _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
            });
    private static Query<TGeometry, Point3d> Divide<TGeometry>(int count) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: DivideByCountKey,
            evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                Curve curve => curve.DivideByCount(segmentCount: count, includeEnds: true, points: out Point3d[] points) switch {
                    double[] => Many(key: DivideByCountKey, values: points),
                    _ => Fin.Fail<Seq<Point3d>>(DivideByCountKey.InvalidResult()),
                },
                _ => Fin.Fail<Seq<Point3d>>(DivideByCountKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            });
    private static Query<TGeometry, Point3d> Divide<TGeometry>(double length) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: DivideByLengthKey,
            requirement: GeometryRequirement.CurveLength,
            evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                Curve curve => curve.DivideByLength(segmentLength: length, includeEnds: true, points: out Point3d[] points) switch {
                    double[] => Many(key: DivideByLengthKey, values: points),
                    _ => Fin.Fail<Seq<Point3d>>(DivideByLengthKey.InvalidResult()),
                },
                _ => Fin.Fail<Seq<Point3d>>(DivideByLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            });
    private static Query<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull =>
        Query<TGeometry, Curve>.Build(
            key: ShortPathKey,
            requirement: GeometryRequirement.SurfaceEvaluation,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                Surface surface => context
                    .Bind((GeometryContext model) => (
                        Uv(surface: surface, uv: start, context: model, key: ShortPathKey),
                        Uv(surface: surface, uv: end, context: model, key: ShortPathKey)
                    ).Apply(static (Point2d a, Point2d b) => (Start: a, End: b)).As()
                    .Bind((ValueTuple<Point2d, Point2d> uv) => Optional(surface.ShortPath(start: uv.Item1, end: uv.Item2, tolerance: model.Absolute.Value))
                        .ToFin(ShortPathKey.InvalidResult())))
                    .Map(static (Curve path) => Seq(path)),
                _ => Fin.Fail<Seq<Curve>>(ShortPathKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
            });
    private static Query<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(
        OperationKey key,
        Point2d uv,
        Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: key,
            requirement: GeometryRequirement.SurfaceEvaluation,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                Surface surface => context
                    .Bind((GeometryContext model) => Uv(surface: surface, uv: uv, context: model, key: key))
                    .Bind((Point2d parameter) => project(arg1: surface, arg2: parameter)),
                _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
            });
    private static Fin<Point2d> Uv(Surface surface, Point2d uv, GeometryContext context, OperationKey key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid
                && v.IsValid
                && u.IncludesParameter(t: uv.X)
                && v.IncludesParameter(t: uv.Y)
                && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
}
