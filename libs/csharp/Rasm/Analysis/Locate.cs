using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Rasm.Analysis;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class Query {
    public static Query<Curve, Point3d> WorldCardinalPoints() =>
        Query<Curve, Point3d>.Build(
            key: WorldCardinalPointsKey,
            requirement: Requirement.CurveLength,
            evaluator: static curve => from ctx in Analyze.Asks
                                       from result in ExtractCardinals(curve: curve, tolerance: ctx.Absolute.Value).ToEff()
                                       select result);
    public static Query<TGeometry, TOut> Quadrants<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: WorldCardinalPointsKey, query: Query<TGeometry, Point3d>.Build(
                key: WorldCardinalPointsKey,
                requirement: Requirement.CurveLength,
                evaluator: static geom => from ctx in Analyze.Asks
                                          from result in QuadrantsFromGeom(geom: geom, tolerance: ctx.Absolute.Value).ToEff()
                                          select result)),
            _ => WorldCardinalPointsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Point3d>> QuadrantsFromGeom<TGeometry>(TGeometry geom, double tolerance) where TGeometry : notnull =>
        geom switch {
            Curve curve when curve.IsValid => ExtractCardinals(curve: curve, tolerance: tolerance),
            Polyline polyline when polyline.IsValid => Bracket(factory: polyline.ToPolylineCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Line line when line.IsValid => Bracket(factory: () => new LineCurve(line: line), body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Circle circle when circle.IsValid => Bracket(factory: circle.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Arc arc when arc.IsValid => Bracket(factory: arc.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            _ => Fin.Fail<Seq<Point3d>>(WorldCardinalPointsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    private static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    private static Fin<Seq<Point3d>> ExtractCardinals(Curve curve, double tolerance) =>
        (ExtremeAlongDirection(curve: curve, direction: Vector3d.XAxis, maximize: false), ExtremeAlongDirection(curve: curve, direction: Vector3d.XAxis, maximize: true), ExtremeAlongDirection(curve: curve, direction: Vector3d.YAxis, maximize: false), ExtremeAlongDirection(curve: curve, direction: Vector3d.YAxis, maximize: true), ExtremeAlongDirection(curve: curve, direction: Vector3d.ZAxis, maximize: false), ExtremeAlongDirection(curve: curve, direction: Vector3d.ZAxis, maximize: true))
            .Apply(static (xMin, xMax, yMin, yMax, zMin, zMax) => (XMin: xMin, XMax: xMax, YMin: yMin, YMax: yMax, ZMin: zMin, ZMax: zMax)).As()
            .Map(state => curve.IsPlanar(tolerance: tolerance) ? Seq(state.XMin, state.XMax, state.YMin, state.YMax) : Seq(state.XMin, state.XMax, state.YMin, state.YMax, state.ZMin, state.ZMax));
    private static Fin<Point3d> ExtremeAlongDirection(Curve curve, Vector3d direction, bool maximize) =>
        toSeq(curve.ExtremeParameters(direction: direction)).Map(curve.PointAt) switch {
            Seq<Point3d> points => (maximize switch {
                true => points.Maxima(projection: p => (Vector3d)p * direction, tolerance: 0.0),
                false => points.Minima(projection: p => (Vector3d)p * direction, tolerance: 0.0),
            }).Head.ToFin(WorldCardinalPointsKey.InvalidResult()),
        };
    public static Query<TGeometry, TOut> Locate<TGeometry, TOut>(Location aspect) where TGeometry : notnull =>
        Aspect<TGeometry, TOut, Location>(
            aspect: aspect,
            key: PointAtKey,
            dispatch: static candidate => candidate.Switch<Query<TGeometry, TOut>?>(
                midpoint: static _ => Mid<TGeometry, TOut>(),
                tangent: static _ => TangentAtMiddle<TGeometry, TOut>(),
                closest: static c => Closest<TGeometry, TOut>(point: c.Point),
                curvatureProfile: static cp => CurvatureProfile<TGeometry, TOut>(count: cp.Count, scalar: cp.Scalar),
                pointAtCurve: static pac => CurveLocated<TGeometry, TOut, Point3d>(
                    key: PointAtKey,
                    query: CurveAt<TGeometry, Point3d>(
                        key: PointAtKey,
                        parameter: pac.Parameter,
                        project: static (curve, parameter) => One(key: PointAtKey, value: curve.PointAt(t: parameter)))),
                pointAtLength: static pal => CurveLocated<TGeometry, TOut, Point3d>(
                    key: PointAtLengthKey,
                    query: Query<TGeometry, Point3d>.Build(
                        key: PointAtLengthKey,
                        requirement: Requirement.CurveLength,
                        state: pal.Length,
                        evaluator: static (segmentLength, geometry) => CurveAtLengthValue(
                            segmentLength: segmentLength,
                            geometry: geometry))),
                frameAtCurve: static fac => CurveLocated<TGeometry, TOut, Plane>(key: FrameAtKey, query: CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
                perpendicularFrameAt: static pfa => CurveLocated<TGeometry, TOut, Plane>(key: PerpendicularFrameAtKey, query: CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
                curvatureAtCurve: static cac => CurveLocated<TGeometry, TOut, Vector3d>(
                    key: CurvatureAtKey,
                    query: CurveAt<TGeometry, Vector3d>(
                        key: CurvatureAtKey,
                        parameter: cac.Parameter,
                        project: static (curve, parameter) => One(key: CurvatureAtKey, value: curve.CurvatureAt(t: parameter)))),
                derivativeAt: static da => CurveLocated<TGeometry, TOut, Vector3d>(
                    key: DerivativeAtKey,
                    query: CurveAt<TGeometry, Vector3d>(
                        key: DerivativeAtKey,
                        parameter: da.Parameter,
                        project: (curve, parameter) => Many(key: DerivativeAtKey, values: curve.DerivativeAt(t: parameter, derivativeCount: da.Count)))),
                divideByCount: static dbc => CurveLocated<TGeometry, TOut, Point3d>(key: DivideByCountKey, query: Divide<TGeometry>(count: dbc.Count)),
                divideByLength: static dbl => CurveLocated<TGeometry, TOut, Point3d>(key: DivideByLengthKey, query: Divide<TGeometry>(length: dbl.Length)),
                orientation: static o => CurveLocated<TGeometry, TOut, CurveOrientation>(
                    key: OrientationKey,
                    query: Query<TGeometry, CurveOrientation>.Build(
                        key: OrientationKey,
                        state: o.Plane,
                        evaluator: static (plane, geometry) => geometry switch {
                            Curve curve => One(key: OrientationKey, value: curve.ClosedCurveOrientation(plane: plane)).ToEff(),
                            _ => Fin.Fail<Seq<CurveOrientation>>(OrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
                        })),
                contains: static cnt => CurveLocated<TGeometry, TOut, PointContainment>(
                    key: ContainsKey,
                    query: Query<TGeometry, PointContainment>.Build(
                        key: ContainsKey,
                        requiresContext: true,
                        state: (Probe: cnt.Point, Frame: cnt.Plane),
                        evaluator: static (probe, geometry) => geometry switch {
                            Curve curve =>
                                from ctx in Analyze.Asks
                                from result in (curve.Contains(testPoint: probe.Probe, plane: probe.Frame, tolerance: ctx.Absolute.Value) switch {
                                    PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(ContainsKey.InvalidResult()),
                                    PointContainment containment => One(key: ContainsKey, value: containment),
                                }).ToEff()
                                select result,
                            _ => Fin.Fail<Seq<PointContainment>>(ContainsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))).ToEff(),
                        })),
                pointAtSurface: static pas => SurfaceLocated<TGeometry, TOut, Point3d>(
                    key: PointAtKey,
                    query: SurfaceUv<TGeometry, Point3d>(
                        key: PointAtKey,
                        uv: pas.Uv,
                        project: static (geometry, parameter) => One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
                frameAtSurface: static fas => SurfaceLocated<TGeometry, TOut, Plane>(
                    key: FrameAtKey,
                    query: SurfaceUv<TGeometry, Plane>(
                        key: FrameAtKey,
                        uv: fas.Uv,
                        project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                            true => One(key: FrameAtKey, value: frame),
                            false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                        })),
                normalAt: static na => SurfaceLocated<TGeometry, TOut, Vector3d>(
                    key: NormalAtKey,
                    query: SurfaceUv<TGeometry, Vector3d>(
                        key: NormalAtKey,
                        uv: na.Uv,
                        project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                            Vector3d normal when normal.IsValid && !normal.IsTiny() => One(key: NormalAtKey, value: normal),
                            _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
                        })),
                curvatureAtSurface: static cas => SurfaceLocated<TGeometry, TOut, SurfaceCurvature>(
                    key: CurvatureAtKey,
                    query: SurfaceUv<TGeometry, SurfaceCurvature>(
                        key: CurvatureAtKey,
                        uv: cas.Uv,
                        project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y))
                            .ToFin(CurvatureAtKey.InvalidResult())
                            .Map(static curvature => Seq(curvature)))),
                shortPath: static sp => SurfaceLocated<TGeometry, TOut, Curve>(key: ShortPathKey, query: ShortPath<TGeometry>(start: sp.Start, end: sp.End)),
                controlPoints: static _ => ControlPoints<TGeometry, TOut>()));
    private static Query<TGeometry, TOut> ControlPoints<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(Point3d) && (typeof(Curve).IsAssignableFrom(c: geometry) || typeof(Surface).IsAssignableFrom(c: geometry) || typeof(Brep).IsAssignableFrom(c: geometry) || geometry == typeof(object)) =>
                Cast<TGeometry, TOut>(key: ControlPointsKey, query: Query<TGeometry, Point3d>.Build(
                    key: ControlPointsKey,
                    evaluator: static geometry => (geometry switch {
                        NurbsCurve curve => CurveControlPoints(curve: curve),
                        Curve curve => Optional(curve.ToNurbsCurve())
                            .ToFin(ControlPointsKey.InvalidResult())
                            .Bind(static nurbs => { using NurbsCurve disposable = nurbs; return CurveControlPoints(curve: disposable); }),
                        NurbsSurface surface => SurfaceControlPoints(surface: surface),
                        Surface surface => Optional(surface.ToNurbsSurface())
                            .ToFin(ControlPointsKey.InvalidResult())
                            .Bind(static nurbs => { using NurbsSurface disposable = nurbs; return SurfaceControlPoints(surface: disposable); }),
                        Brep brep => BrepControlPoints(brep: brep),
                        _ => Fin.Fail<Seq<Point3d>>(ControlPointsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    }).ToEff())),
            _ => ControlPointsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Point3d>> CurveControlPoints(NurbsCurve curve) =>
        Many(
            key: ControlPointsKey,
            values: Enumerable.Range(start: 0, count: curve.Points.Count)
                .Select(index => curve.Points[index].Location));
    private static Fin<Seq<Point3d>> SurfaceControlPoints(NurbsSurface surface) =>
        Many(
            key: ControlPointsKey,
            values: Enumerable.Range(start: 0, count: surface.Points.CountU)
                .SelectMany(u => Enumerable.Range(start: 0, count: surface.Points.CountV)
                    .Select(v => surface.Points.GetControlPoint(u: u, v: v).Location)));
    private static Fin<Seq<Point3d>> BrepControlPoints(Brep brep) =>
        toSeq(brep.Faces)
            .TraverseM(face => Optional(face.ToNurbsSurface())
                .ToFin(ControlPointsKey.InvalidResult())
                .Bind(static nurbs => { using NurbsSurface disposable = nurbs; return SurfaceControlPoints(surface: disposable); }))
            .As()
            .Map(static nested => nested.Bind(static points => points));
    private static Query<TGeometry, TOut>? CurveLocated<TGeometry, TOut, TValue>(Op key, Query<TGeometry, TValue> query) where TGeometry : notnull =>
        typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(TValue) ? Cast<TGeometry, TOut>(key: key, query: query) : null;
    private static Query<TGeometry, TOut>? SurfaceLocated<TGeometry, TOut, TValue>(Op key, Query<TGeometry, TValue> query) where TGeometry : notnull =>
        typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(TValue) ? Cast<TGeometry, TOut>(key: key, query: query) : null;
    private static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Line, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static geometry => (geometry.IsValid switch {
                        true => One(key: MidpointKey, value: geometry.PointAt(t: 0.5)),
                        false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                    }).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<Polyline, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static geometry => Bracket(factory: geometry.ToPolylineCurve, body: static curve => curve.IsValid switch {
                        true => One(key: MidpointKey, value: curve.PointAtNormalizedLength(length: 0.5)),
                        false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: MidpointKey, query: Query<TGeometry, Point3d>.Build(
                    key: MidpointKey,
                    requirement: Requirement.CurveLength,
                    evaluator: static geometry => CurveAtNormalized<TGeometry, Point3d>(
                        geometry: geometry,
                        key: MidpointKey,
                        project: static (curve, parameter) => curve.PointAt(t: parameter)))),
            _ => MidpointKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Line, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static geometry => One(key: TangentKey, value: geometry.UnitTangent).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<Polyline, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static geometry => Bracket(factory: geometry.ToPolylineCurve, body: static curve => curve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                        true => One(key: TangentKey, value: curve.TangentAt(t: parameter)),
                        false => Fin.Fail<Seq<Vector3d>>(TangentKey.InvalidResult()),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(key: TangentKey, query: Query<TGeometry, Vector3d>.Build(
                    key: TangentKey,
                    requirement: Requirement.CurveLength,
                    evaluator: static geometry => CurveAtNormalized<TGeometry, Vector3d>(
                        geometry: geometry,
                        key: TangentKey,
                        project: static (curve, parameter) => curve.TangentAt(t: parameter)))),
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
                CurvatureQuery<TGeometry, TOut, Curve, Vector3d>(requirement: Requirement.CurveLength, count: count, project: static (curve, sampleCount, ctx) => CurveCurvatures(curve: curve, count: sampleCount, model: ctx)),
            (CurvatureScalar.None or CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                CurvatureQuery<TGeometry, TOut, Curve, CurvatureProfile>(requirement: Requirement.CurveLength, count: count, project: static (curve, sampleCount, ctx) => CurveMagnitudes(curve: curve, count: sampleCount, model: ctx).Bind(static values => Profile(scalar: CurvatureScalar.Magnitude, values: values).Map(static profile => Seq(profile)))),
            (CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                CurvatureQuery<TGeometry, TOut, Curve, double>(requirement: Requirement.CurveLength, count: count, project: static (curve, sampleCount, ctx) => CurveMagnitudes(curve: curve, count: sampleCount, model: ctx).Bind(static values => Many(key: CurvatureAtKey, values: values))),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) =>
                CurvatureQuery<TGeometry, TOut, Surface, SurfaceCurvature>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, ctx) => SurfaceCurvatures(surface: surface, count: sampleCount, model: ctx)),
            (CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                CurvatureQuery<TGeometry, TOut, Surface, CurvatureProfile>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, ctx) => SurfaceCurvatures(surface: surface, count: sampleCount, model: ctx).Bind(static curvatures => (SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Gaussian).Bind(static values => Profile(scalar: CurvatureScalar.Gaussian, values: values)), SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Mean).Bind(static values => Profile(scalar: CurvatureScalar.Mean, values: values))).Apply(static (gaussian, mean) => Seq(gaussian, mean)).As())),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                CurvatureQuery<TGeometry, TOut, Surface, CurvatureProfile>(requirement: Requirement.SurfaceEvaluation, count: count, project: (surface, sampleCount, ctx) => SurfaceScalarProfile(surface: surface, count: sampleCount, model: ctx, scalar: scalar).Bind(values => Profile(scalar: scalar, values: values).Map(static profile => Seq(profile)))),
            (CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                CurvatureQuery<TGeometry, TOut, Surface, double>(requirement: Requirement.SurfaceEvaluation, count: count, project: (surface, sampleCount, ctx) => SurfaceScalarProfile(surface: surface, count: sampleCount, model: ctx, scalar: scalar).Bind(static values => Many(key: CurvatureAtKey, values: values))),
            _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> CurvatureQuery<TGeometry, TOut, TNative, TValue>(Requirement requirement, int count, Func<TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: CurvatureAtKey, query: Query<TGeometry, TValue>.Build(
            key: CurvatureAtKey,
            requirement: requirement,
            requiresContext: true,
            state: (Count: count, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => from ctx in Analyze.Asks
                                  from result in state.Project(arg1: native, arg2: state.Count, arg3: ctx).ToEff()
                                  select result,
                _ => Fin.Fail<Seq<TValue>>(CurvatureAtKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TValue))).ToEff(),
            }));
    private static Fin<Seq<double>> SurfaceScalarProfile(Surface surface, int count, Context model, CurvatureScalar scalar) =>
        SurfaceCurvatures(surface: surface, count: count, model: model).Bind(curvatures => SurfaceScalars(curvatures: curvatures, scalar: scalar));
    private static Fin<Seq<double>> SurfaceScalars(Seq<SurfaceCurvature> curvatures, CurvatureScalar scalar) =>
        scalar switch {
            CurvatureScalar.Gaussian => Fin.Succ(curvatures.Map(static curvature => curvature.Gaussian)),
            CurvatureScalar.Mean => Fin.Succ(curvatures.Map(static curvature => curvature.Mean)),
            _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Vector3d>> CurveCurvatures(Curve curve, int count, Context model) =>
        CurveSamples(
                curve: curve,
                count: count,
                model: model,
                key: CurvatureAtKey)
            .Bind(parameters => Many(
                key: CurvatureAtKey,
                values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudes(Curve curve, int count, Context model) =>
        CurveCurvatures(curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Surface surface, int count, Context model) =>
        (
            Samples(domain: surface.Domain(direction: 0), count: count, key: CurvatureAtKey),
            Samples(domain: surface.Domain(direction: 1), count: count, key: CurvatureAtKey)
        ).Apply(static (u, v) => (U: u, V: v)).As()
        .Bind(samples => samples.U
            .Bind(u => samples.V.Map(v => new Point2d(x: u, y: v)))
            .TraverseM(uv => Uv(surface: surface, uv: uv, context: model, key: CurvatureAtKey)
                .Bind(parameter => Optional(surface.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult())))
            .As());
    private static Fin<Seq<double>> CurveSamples(Curve curve, int count, Context model, Op key) =>
        Fractions(count: count, key: key)
            .Bind(fractions => Optional(curve.NormalizedLengthParameters(
                    s: [.. fractions.AsIterable()],
                    absoluteTolerance: model.Absolute.Value,
                    fractionalTolerance: model.Relative.Value))
                .ToFin(key.InvalidResult())
                .Map(static parameters => toSeq(parameters)));
    private static Fin<Seq<double>> Fractions(int count, Op key) =>
        count switch {
            1 => Fin.Succ(Seq(0.5)),
            > 1 => Fin.Succ(toSeq(Enumerable.Range(start: 0, count: count).Select(i => i / (count - 1.0)))),
            _ => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Fin<CurvatureProfile> Profile(CurvatureScalar scalar, Seq<double> values) =>
        Stats.From(values: values, key: CurvatureAtKey).Map(s => new CurvatureProfile(Scalar: scalar, Count: s.Count, Minimum: s.Minimum, Maximum: s.Maximum, Mean: s.Mean, Variance: s.Variance));
    private static Fin<Seq<double>> Samples(Interval domain, int count, Op key) =>
        domain.IsValid switch {
            true => Fractions(count: count, key: key).Map(fractions => fractions.Map(f => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Line, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target, limitToFiniteSegment: true))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Polyline, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Curve, Point3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(t: parameter)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Surface, Point3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, u: out double u, v: out double v) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(u: u, v: v)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                ClosestMatch<TGeometry, TOut, Curve, double>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: parameter),
                    false => Fin.Fail<Seq<double>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Vector3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex _, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d normal) switch {
                    true => One(key: ClosestKey, value: normal),
                    false => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Vector3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, pointOnMesh: out Point3d _, normalAtPoint: out Vector3d normal, maximumDistance: 0.0) switch {
                    >= 0 => One(key: ClosestKey, value: normal),
                    _ => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                ClosestMatch<TGeometry, TOut, Brep, ComponentIndex>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex component, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d _) switch {
                    true => One(key: ClosestKey, value: component),
                    false => Fin.Fail<Seq<ComponentIndex>>(ClosestKey.InvalidResult()),
                }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(MeshPoint) =>
                ClosestMatch<TGeometry, TOut, Mesh, MeshPoint>(point: point, project: static (target, geometry) => Optional(geometry.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0))
                    .ToFin(ClosestKey.InvalidResult())
                    .Map(static meshPoint => Seq(meshPoint))),
            _ => ClosestKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(
            key: key,
            parameter: parameter,
            project: (curve, t) => perpendicular switch {
                true => OpResult<Plane>.Solved(
                    isSolved: curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame),
                    value: perpendicularFrame).Reduce(key: key),
                false => OpResult<Plane>.Solved(
                    isSolved: curve.FrameAt(t: t, plane: out Plane frame),
                    value: frame).Reduce(key: key),
            });
    private static Query<TGeometry, TOut> CurveAt<TGeometry, TOut>(
        Op key,
        double parameter,
        Func<Curve, double, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: key,
            state: (Key: key, Parameter: parameter, Project: project),
            evaluator: static (state, geometry) => (geometry switch {
                Curve curve => curve.Domain.IncludesParameter(t: state.Parameter) switch {
                    true => state.Project(arg1: curve, arg2: state.Parameter),
                    false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
                },
                _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
            }).ToEff());
    private static Query<TGeometry, Point3d> Divide<TGeometry>(int count) where TGeometry : notnull =>
        DividePoly<TGeometry>(
            key: DivideByCountKey,
            requirement: null,
            divide: curve => curve.DivideByCount(segmentCount: count, includeEnds: true, points: out Point3d[] points) switch {
                double[] => Optional(points),
                _ => Option<Point3d[]>.None,
            });
    private static Query<TGeometry, Point3d> Divide<TGeometry>(double length) where TGeometry : notnull =>
        DividePoly<TGeometry>(
            key: DivideByLengthKey,
            requirement: Requirement.CurveLength,
            divide: curve => curve.DivideByLength(segmentLength: length, includeEnds: true, points: out Point3d[] points) switch {
                double[] => Optional(points),
                _ => Option<Point3d[]>.None,
            });
    private static Query<TGeometry, Point3d> DividePoly<TGeometry>(
        Op key,
        Requirement? requirement,
        Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: key,
            requirement: requirement,
            state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => (geometry switch {
                Curve curve => state.Divide(arg: curve).Match(
                    Some: points => Many(key: state.Key, values: points),
                    None: () => Fin.Fail<Seq<Point3d>>(state.Key.InvalidResult())),
                _ => Fin.Fail<Seq<Point3d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            }).ToEff());
    private static Query<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull =>
        Query<TGeometry, Curve>.Build(
            key: ShortPathKey,
            requirement: Requirement.SurfaceEvaluation,
            state: (Start: start, End: end),
            evaluator: static (endpoints, geometry) => geometry switch {
                Surface surface =>
                    from ctx in Analyze.Asks
                    from uvStart in Uv(surface: surface, uv: endpoints.Start, context: ctx, key: ShortPathKey).ToEff()
                    from uvEnd in Uv(surface: surface, uv: endpoints.End, context: ctx, key: ShortPathKey).ToEff()
                    from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: ctx.Absolute.Value))
                        .ToFin(ShortPathKey.InvalidResult())
                        .ToEff()
                    select Seq(path),
                _ => Fin.Fail<Seq<Curve>>(ShortPathKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))).ToEff(),
            });
    private static Query<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(
        Op key,
        Point2d uv,
        Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: key,
            requirement: Requirement.SurfaceEvaluation,
            state: (Key: key, Uv: uv, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                Surface surface =>
                    from ctx in Analyze.Asks
                    from parameter in Uv(surface: surface, uv: state.Uv, context: ctx, key: state.Key).ToEff()
                    from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                    select result,
                _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))).ToEff(),
            });
    private static Fin<Point2d> Uv(Surface surface, Point2d uv, Context context, Op key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y) && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
    private static Eff<Context, Seq<Point3d>> CurveAtLengthValue<TGeometry>(double segmentLength, TGeometry geometry) where TGeometry : notnull =>
        geometry switch {
            Curve curve =>
                from ctx in Analyze.Asks
                from result in (curve.LengthParameter(
                    segmentLength: segmentLength,
                    t: out double parameter,
                    fractionalTolerance: ctx.Relative.Value) switch {
                        true => One(key: PointAtLengthKey, value: curve.PointAt(t: parameter)),
                        false => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.InvalidResult()),
                    }).ToEff()
                select result,
            _ => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))).ToEff(),
        };
}
