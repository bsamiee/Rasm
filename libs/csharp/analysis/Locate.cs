using System.Linq;
using Core;
using Core.Domain;
using Core.Runtime;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static partial class Query {
    public static Query<Curve, Point3d> WorldCardinalPoints() =>
        Query<Curve, Point3d>.Build(
            key: WorldCardinalPointsKey,
            requirement: GeometryRequirement.CurveLength,
            evaluator: static (Curve curve) =>
                from rt in Analyze.Asks
                from validated in rt.Context.Validate(geometry: curve, requirement: GeometryRequirement.CurveLength).ToEff()
                from result in ExtractCardinals(curve: validated, tolerance: rt.Context.Absolute.Value).ToEff()
                select result);
    public static Query<TGeometry, TOut> Quadrants<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: WorldCardinalPointsKey, query: Query<TGeometry, Point3d>.Build(
                key: WorldCardinalPointsKey,
                requirement: GeometryRequirement.CurveLength,
                evaluator: static (TGeometry geom) =>
                    from rt in Analyze.Asks
                    from result in QuadrantsFromGeom(geom: geom, tolerance: rt.Context.Absolute.Value).ToEff()
                    select result)),
            _ => WorldCardinalPointsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Point3d>> QuadrantsFromGeom<TGeometry>(TGeometry geom, double tolerance) where TGeometry : notnull =>
        geom switch {
            Curve curve when curve.IsValid => ExtractCardinals(curve: curve, tolerance: tolerance),
            Polyline polyline when polyline.IsValid => WithOwnedCurve(owned: polyline.ToPolylineCurve(), tolerance: tolerance),
            Line line when line.IsValid => WithOwnedCurve(owned: new LineCurve(line: line), tolerance: tolerance),
            Circle circle when circle.IsValid => WithOwnedCurve(owned: circle.ToNurbsCurve(), tolerance: tolerance),
            Arc arc when arc.IsValid => WithOwnedCurve(owned: arc.ToNurbsCurve(), tolerance: tolerance),
            _ => Fin.Fail<Seq<Point3d>>(WorldCardinalPointsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    private static Fin<Seq<Point3d>> WithOwnedCurve(Curve owned, double tolerance) {
        using Curve disposable = owned;
        return ExtractCardinals(curve: disposable, tolerance: tolerance);
    }
    private static Fin<Seq<Point3d>> ExtractCardinals(Curve curve, double tolerance) =>
        (
            ExtremeAlongDirection(curve: curve, direction: Vector3d.XAxis, isMax: false),
            ExtremeAlongDirection(curve: curve, direction: Vector3d.XAxis, isMax: true),
            ExtremeAlongDirection(curve: curve, direction: Vector3d.YAxis, isMax: false),
            ExtremeAlongDirection(curve: curve, direction: Vector3d.YAxis, isMax: true),
            ExtremeAlongDirection(curve: curve, direction: Vector3d.ZAxis, isMax: false),
            ExtremeAlongDirection(curve: curve, direction: Vector3d.ZAxis, isMax: true)
        ).Apply(static (Point3d xMin, Point3d xMax, Point3d yMin, Point3d yMax, Point3d zMin, Point3d zMax) =>
            (XMin: xMin, XMax: xMax, YMin: yMin, YMax: yMax, ZMin: zMin, ZMax: zMax))
        .As()
        .Map(((Point3d XMin, Point3d XMax, Point3d YMin, Point3d YMax, Point3d ZMin, Point3d ZMax) state) =>
            curve.IsPlanar(tolerance: tolerance)
                ? Seq(state.XMin, state.XMax, state.YMin, state.YMax)
                : Seq(state.XMin, state.XMax, state.YMin, state.YMax, state.ZMin, state.ZMax));
    private static Fin<Point3d> ExtremeAlongDirection(Curve curve, Vector3d direction, bool isMax) {
        double[] parameters = curve.ExtremeParameters(direction: direction);
        return parameters.Length switch {
            0 => Fin.Fail<Point3d>(WorldCardinalPointsKey.InvalidResult()),
            _ => Fin.Succ(parameters
                .Aggregate(
                    seed: (Param: parameters[0], Score: Dot(point: curve.PointAt(t: parameters[0]), direction: direction)),
                    func: (state, t) => Choose(
                        current: state,
                        candidate: (Param: t, Score: Dot(point: curve.PointAt(t: t), direction: direction)),
                        isMax: isMax))
                switch { (double param, _) => curve.PointAt(t: param) }),
        };
    }
    private static (double Param, double Score) Choose(
        (double Param, double Score) current,
        (double Param, double Score) candidate,
        bool isMax) =>
        (isMax, candidate.Score > current.Score, candidate.Score < current.Score) switch {
            (true, true, _) => candidate,
            (false, _, true) => candidate,
            _ => current,
        };
    private static double Dot(Point3d point, Vector3d direction) =>
        (point.X * direction.X) + (point.Y * direction.Y) + (point.Z * direction.Z);
    public static Query<TGeometry, TOut> Locate<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        aspect switch {
            Location.Midpoint => Mid<TGeometry, TOut>(),
            Location.Tangent => TangentAtMiddle<TGeometry, TOut>(),
            Location.Closest c => Closest<TGeometry, TOut>(point: c.Point),
            Location.CurvatureProfile cp => CurvatureProfile<TGeometry, TOut>(count: cp.Count, scalar: cp.Scalar),
            Location.PointAtCurve pac when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtKey, query: CurveAt<TGeometry, Point3d>(
                    key: PointAtKey,
                    parameter: pac.Parameter,
                    project: static (Curve curve, double parameter) => One(key: PointAtKey, value: curve.PointAt(t: parameter)))),
            Location.PointAtLength pal when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtLengthKey, query: Query<TGeometry, Point3d>.Build(
                    key: PointAtLengthKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: pal.Length,
                    evaluator: static (double segmentLength, TGeometry geometry) => CurveAtLengthValue(
                        segmentLength: segmentLength,
                        geometry: geometry))),
            Location.FrameAtCurve fac when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: FrameAtKey, query: CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
            Location.PerpendicularFrameAt pfa when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: PerpendicularFrameAtKey, query: CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
            Location.CurvatureAtCurve cac when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: CurveAt<TGeometry, Vector3d>(
                    key: CurvatureAtKey,
                    parameter: cac.Parameter,
                    project: static (Curve curve, double parameter) => One(key: CurvatureAtKey, value: curve.CurvatureAt(t: parameter)))),
            Location.DerivativeAt da when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: DerivativeAtKey, query: CurveAt<TGeometry, Vector3d>(
                    key: DerivativeAtKey,
                    parameter: da.Parameter,
                    project: (Curve curve, double parameter) => Many(key: DerivativeAtKey, values: curve.DerivativeAt(t: parameter, derivativeCount: da.Count)))),
            Location.DivideByCount dbc when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: DivideByCountKey, query: Divide<TGeometry>(count: dbc.Count)),
            Location.DivideByLength dbl when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: DivideByLengthKey, query: Divide<TGeometry>(length: dbl.Length)),
            Location.Orientation o when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(CurveOrientation) =>
                Cast<TGeometry, TOut>(key: OrientationKey, query: Query<TGeometry, CurveOrientation>.Build(
                    key: OrientationKey,
                    state: o.Plane,
                    evaluator: static (Plane plane, TGeometry geometry) => geometry switch {
                        Curve curve => One(key: OrientationKey, value: curve.ClosedCurveOrientation(plane: plane)).ToEff(),
                        _ => Eff<AnalysisRuntime, Seq<CurveOrientation>>.Fail(error: OrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))),
                    })),
            Location.Contains cnt when typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(PointContainment) =>
                Cast<TGeometry, TOut>(key: ContainsKey, query: Query<TGeometry, PointContainment>.Build(
                    key: ContainsKey,
                    requiresContext: true,
                    state: (Probe: cnt.Point, Frame: cnt.Plane),
                    evaluator: static ((Point3d Probe, Plane Frame) probe, TGeometry geometry) => geometry switch {
                        Curve curve =>
                            from rt in Analyze.Asks
                            from result in (curve.Contains(testPoint: probe.Probe, plane: probe.Frame, tolerance: rt.Context.Absolute.Value) switch {
                                PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(ContainsKey.InvalidResult()),
                                PointContainment containment => One(key: ContainsKey, value: containment),
                            }).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<PointContainment>>.Fail(error: ContainsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))),
                    })),
            Location.PointAtSurface pas when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: PointAtKey, query: SurfaceUv<TGeometry, Point3d>(
                    key: PointAtKey,
                    uv: pas.Uv,
                    project: static (Surface geometry, Point2d parameter) => One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
            Location.FrameAtSurface fas when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Plane) =>
                Cast<TGeometry, TOut>(key: FrameAtKey, query: SurfaceUv<TGeometry, Plane>(
                    key: FrameAtKey,
                    uv: fas.Uv,
                    project: static (Surface geometry, Point2d parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                        true => One(key: FrameAtKey, value: frame),
                        false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                    })),
            Location.NormalAt na when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: NormalAtKey, query: SurfaceUv<TGeometry, Vector3d>(
                    key: NormalAtKey,
                    uv: na.Uv,
                    project: static (Surface geometry, Point2d parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                        Vector3d normal when normal.IsValid && !normal.IsTiny() => One(key: NormalAtKey, value: normal),
                        _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
                    })),
            Location.CurvatureAtSurface cas when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(SurfaceCurvature) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: SurfaceUv<TGeometry, SurfaceCurvature>(
                    key: CurvatureAtKey,
                    uv: cas.Uv,
                    project: static (Surface geometry, Point2d parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y))
                        .ToFin(CurvatureAtKey.InvalidResult())
                        .Map(static (SurfaceCurvature curvature) => Seq(curvature)))),
            Location.ShortPath sp when typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(Curve) =>
                Cast<TGeometry, TOut>(key: ShortPathKey, query: ShortPath<TGeometry>(start: sp.Start, end: sp.End)),
            _ => PointAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Line, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Line geometry) => (geometry.IsValid switch {
                        true => One(key: MidpointKey, value: geometry.PointAt(t: 0.5)),
                        false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                    }).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Polyline, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Polyline geometry) => PolylineMidpoint(geometry: geometry).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<TGeometry, Point3d>.Build(
                    key: MidpointKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry) => CurveAtNormalized<TGeometry, Point3d>(
                        geometry: geometry,
                        key: MidpointKey,
                        project: static (Curve curve, double parameter) => curve.PointAt(t: parameter)))),
            _ => MidpointKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Point3d>> PolylineMidpoint(Polyline geometry) {
        using PolylineCurve curve = geometry.ToPolylineCurve();
        return curve.IsValid switch {
            true => One(key: MidpointKey, value: curve.PointAtNormalizedLength(length: 0.5)),
            false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
        };
    }
    private static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Line, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Line geometry) => One(key: TangentKey, value: geometry.UnitTangent).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Polyline, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Polyline geometry) => PolylineTangent(geometry: geometry).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<TGeometry, Vector3d>.Build(
                    key: TangentKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry) => CurveAtNormalized<TGeometry, Vector3d>(
                        geometry: geometry,
                        key: TangentKey,
                        project: static (Curve curve, double parameter) => curve.TangentAt(t: parameter)))),
            _ => TangentKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Vector3d>> PolylineTangent(Polyline geometry) {
        using PolylineCurve curve = geometry.ToPolylineCurve();
        return curve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
            true => One(key: TangentKey, value: curve.TangentAt(t: parameter)),
            false => Fin.Fail<Seq<Vector3d>>(TangentKey.InvalidResult()),
        };
    }
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
                    evaluator: static (int sampleCount, TGeometry geometry) => geometry switch {
                        Curve curve =>
                            from rt in Analyze.Asks
                            from result in CurveCurvatures(
                                curve: curve,
                                count: sampleCount,
                                model: rt.Context).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<Vector3d>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Vector3d))),
                    })),
            (CurvatureScalar.None or CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry) => geometry switch {
                        Curve curve =>
                            from rt in Analyze.Asks
                            from values in CurveScalarProfile(curve: curve, count: sampleCount, model: rt.Context, scalar: CurvatureScalar.Magnitude).ToEff()
                            from profile in Profile(scalar: CurvatureScalar.Magnitude, values: values).ToEff()
                            select Seq(profile),
                        _ => Eff<AnalysisRuntime, Seq<CurvatureProfile>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, double>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.CurveLength,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry) => geometry switch {
                        Curve curve =>
                            from rt in Analyze.Asks
                            from values in CurveScalarProfile(curve: curve, count: sampleCount, model: rt.Context, scalar: CurvatureScalar.Magnitude).ToEff()
                            from result in Many(key: CurvatureAtKey, values: values).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<double>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, SurfaceCurvature>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry) => geometry switch {
                        Surface surface =>
                            from rt in Analyze.Asks
                            from result in SurfaceCurvatures(
                                surface: surface,
                                count: sampleCount,
                                model: rt.Context).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<SurfaceCurvature>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(SurfaceCurvature))),
                    })),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: count,
                    evaluator: static (int sampleCount, TGeometry geometry) => geometry switch {
                        Surface surface =>
                            from rt in Analyze.Asks
                            from curvatures in SurfaceCurvatures(
                                surface: surface,
                                count: sampleCount,
                                model: rt.Context).ToEff()
                            from result in (
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
                            ).Apply(static (CurvatureProfile gaussian, CurvatureProfile mean) => Seq(gaussian, mean)).As().ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<CurvatureProfile>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, CurvatureProfile>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: (Count: count, Scalar: scalar),
                    evaluator: static ((int Count, CurvatureScalar Scalar) state, TGeometry geometry) => geometry switch {
                        Surface surface =>
                            from rt in Analyze.Asks
                            from values in SurfaceScalarProfile(surface: surface, count: state.Count, model: rt.Context, scalar: state.Scalar).ToEff()
                            from profile in Profile(scalar: state.Scalar, values: values).ToEff()
                            select Seq(profile),
                        _ => Eff<AnalysisRuntime, Seq<CurvatureProfile>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurvatureProfile))),
                    })),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, double>.Build(
                    key: CurvatureAtKey,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    state: (Count: count, Scalar: scalar),
                    evaluator: static ((int Count, CurvatureScalar Scalar) state, TGeometry geometry) => geometry switch {
                        Surface surface =>
                            from rt in Analyze.Asks
                            from values in SurfaceScalarProfile(surface: surface, count: state.Count, model: rt.Context, scalar: state.Scalar).ToEff()
                            from result in Many(key: CurvatureAtKey, values: values).ToEff()
                            select result,
                        _ => Eff<AnalysisRuntime, Seq<double>>.Fail(error: CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
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
            > 1 => Fin.Succ(toSeq(Enumerable
                .Range(start: 0, count: count)
                .Select(i => i / (count - 1.0)))),
            _ => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Fin<CurvatureProfile> Profile(CurvatureScalar scalar, Seq<double> values) =>
        values.StatsOf(key: CurvatureAtKey)
            .Map((Stats s) => new CurvatureProfile(
                Scalar: scalar,
                Count: s.Count,
                Minimum: s.Minimum,
                Maximum: s.Maximum,
                Mean: s.Mean,
                Variance: s.Variance));
    private static Fin<Seq<double>> Samples(Interval domain, int count, OperationKey key) =>
        domain.IsValid switch {
            true => Fractions(count: count, key: key).Map((Seq<double> fractions) => fractions.Map((double f) => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
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
            state: (Key: key, Parameter: parameter, Project: project),
            evaluator: static ((OperationKey Key, double Parameter, Func<Curve, double, Fin<Seq<TOut>>> Project) state, TGeometry geometry) => (geometry switch {
                Curve curve => curve.Domain.IncludesParameter(t: state.Parameter) switch {
                    true => state.Project(arg1: curve, arg2: state.Parameter),
                    false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
                },
                _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
            }).ToEff());
    private static Query<TGeometry, Point3d> Divide<TGeometry>(int count) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: DivideByCountKey,
            state: count,
            evaluator: static (int amount, TGeometry geometry) => (geometry switch {
                Curve curve => curve.DivideByCount(segmentCount: amount, includeEnds: true, points: out Point3d[] points) switch {
                    double[] => Many(key: DivideByCountKey, values: points),
                    _ => Fin.Fail<Seq<Point3d>>(DivideByCountKey.InvalidResult()),
                },
                _ => Fin.Fail<Seq<Point3d>>(DivideByCountKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            }).ToEff());
    private static Query<TGeometry, Point3d> Divide<TGeometry>(double length) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: DivideByLengthKey,
            requirement: GeometryRequirement.CurveLength,
            state: length,
            evaluator: static (double segmentLength, TGeometry geometry) => (geometry switch {
                Curve curve => curve.DivideByLength(segmentLength: segmentLength, includeEnds: true, points: out Point3d[] points) switch {
                    double[] => Many(key: DivideByLengthKey, values: points),
                    _ => Fin.Fail<Seq<Point3d>>(DivideByLengthKey.InvalidResult()),
                },
                _ => Fin.Fail<Seq<Point3d>>(DivideByLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            }).ToEff());
    private static Query<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull =>
        Query<TGeometry, Curve>.Build(
            key: ShortPathKey,
            requirement: GeometryRequirement.SurfaceEvaluation,
            state: (Start: start, End: end),
            evaluator: static ((Point2d Start, Point2d End) endpoints, TGeometry geometry) => geometry switch {
                Surface surface =>
                    from rt in Analyze.Asks
                    from uvStart in Uv(surface: surface, uv: endpoints.Start, context: rt.Context, key: ShortPathKey).ToEff()
                    from uvEnd in Uv(surface: surface, uv: endpoints.End, context: rt.Context, key: ShortPathKey).ToEff()
                    from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: rt.Context.Absolute.Value))
                        .ToFin(ShortPathKey.InvalidResult())
                        .ToEff()
                    select Seq(path),
                _ => Eff<AnalysisRuntime, Seq<Curve>>.Fail(error: ShortPathKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))),
            });
    private static Query<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(
        OperationKey key,
        Point2d uv,
        Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: key,
            requirement: GeometryRequirement.SurfaceEvaluation,
            state: (Key: key, Uv: uv, Project: project),
            evaluator: static ((OperationKey Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project) state, TGeometry geometry) => geometry switch {
                Surface surface =>
                    from rt in Analyze.Asks
                    from parameter in Uv(surface: surface, uv: state.Uv, context: rt.Context, key: state.Key).ToEff()
                    from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                    select result,
                _ => Eff<AnalysisRuntime, Seq<TOut>>.Fail(error: state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
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
    private static Eff<AnalysisRuntime, Seq<Point3d>> CurveAtLengthValue<TGeometry>(double segmentLength, TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Curve curve =>
                from rt in Analyze.Asks
                from result in (curve.LengthParameter(
                    segmentLength: segmentLength,
                    t: out double parameter,
                    fractionalTolerance: rt.Context.Relative.Value) switch {
                        true => One(key: PointAtLengthKey, value: curve.PointAt(t: parameter)),
                        false => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.InvalidResult()),
                    }).ToEff()
                select result,
            _ => Eff<AnalysisRuntime, Seq<Point3d>>.Fail(error: PointAtLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    public static Query<TGeometry, TOut> FaceFrame<TGeometry, TOut>(Faces aspect) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Plane)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry) || geometry == typeof(object)) =>
                Cast<TGeometry, TOut>(key: FaceFrameKey, query: Query<TGeometry, Plane>.Build<Faces>(
                    key: FaceFrameKey,
                    state: aspect,
                    requirement: GeometryRequirement.SurfaceEvaluation,
                    evaluator: static (Faces selector, TGeometry geometry) =>
                        from rt in Analyze.Asks
                        from faces in DecomposeFaces(geometry: geometry).ToEff()
                        from selected in SelectFaces(faces: faces, selector: selector, runtime: rt).ToEff()
                        from frames in selected.Traverse((Brep face) => FrameAtCentroid(face: face, runtime: rt)).As().ToEff()
                        from result in Many(key: FaceFrameKey, values: frames).ToEff()
                        select result)),
            _ => FaceFrameKey.Unsupported<TGeometry, TOut>(),
        };
    // Invariant: face is the single-face Brep produced by DuplicateFace; face.Faces[0] is the BrepFace.
    // The frame's Z is reoriented when OrientationIsReversed so that Z always points outward on closed solids.
    private static Fin<Plane> FrameAtCentroid(Brep face, AnalysisRuntime runtime) =>
        Optional(AreaMassProperties.Compute(
                brep: face,
                area: true,
                firstMoments: true,
                secondMoments: false,
                productMoments: false,
                relativeTolerance: runtime.Context.Relative.Value,
                absoluteTolerance: runtime.Context.Absolute.Value))
            .ToFin(FaceFrameKey.InvalidResult())
            .Bind((AreaMassProperties mass) => {
                using AreaMassProperties disposable = mass;
                BrepFace brepFace = face.Faces[0];
                return (
                    brepFace.ClosestPoint(testPoint: disposable.Centroid, u: out double u, v: out double v),
                    brepFace.FrameAt(u: u, v: v, frame: out Plane frame)
                ) switch {
                    (true, true) when brepFace.OrientationIsReversed =>
                        Fin.Succ(new Plane(origin: frame.Origin, xDirection: frame.XAxis, yDirection: -frame.YAxis)),
                    (true, true) => Fin.Succ(frame),
                    _ => Fin.Fail<Plane>(FaceFrameKey.InvalidResult()),
                };
            });
}
