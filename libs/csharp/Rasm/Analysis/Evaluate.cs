namespace Rasm.Analysis;

public static partial class Query {
    public static Query<TGeometry, TOut> Quadrants<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: QuadrantsKey, query: Query<TGeometry, Point3d>.Build(
                key: QuadrantsKey,
                requirement: Requirement.CurveLength,
                evaluator: static geom => from ctx in Analyze.Asks
                                          from result in QuadrantsFromGeom(geom: geom, tolerance: ctx.Absolute.Value).ToEff()
                                          select result)),
            _ => QuadrantsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<Point3d>> QuadrantsFromGeom<TGeometry>(TGeometry geom, double tolerance) where TGeometry : notnull =>
        geom switch {
            Curve curve when curve.IsValid => ExtractCardinals(curve: curve, tolerance: tolerance),
            Polyline polyline when polyline.IsValid => Bracket(factory: polyline.ToPolylineCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Line line when line.IsValid => Bracket(factory: () => new LineCurve(line: line), body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Circle circle when circle.IsValid => Bracket(factory: circle.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Arc arc when arc.IsValid => Bracket(factory: arc.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            _ => Fin.Fail<Seq<Point3d>>(QuadrantsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    private static Fin<Seq<Point3d>> ExtractCardinals(Curve curve, double tolerance) =>
        Seq((Direction: Vector3d.XAxis, Maximize: false), (Direction: Vector3d.XAxis, Maximize: true), (Direction: Vector3d.YAxis, Maximize: false), (Direction: Vector3d.YAxis, Maximize: true), (Direction: Vector3d.ZAxis, Maximize: false), (Direction: Vector3d.ZAxis, Maximize: true))
            .Take(curve.IsPlanar(tolerance: tolerance) switch { true => 4, false => 6 })
            .TraverseM(state => ExtremeAlongDirection(curve: curve, direction: state.Direction, maximize: state.Maximize))
            .As();
    private static Fin<Point3d> ExtremeAlongDirection(Curve curve, Vector3d direction, bool maximize) =>
        toSeq(curve.ExtremeParameters(direction: direction)).Map(curve.PointAt) switch {
            Seq<Point3d> points => points
                .Maxima(projection: p => (Vector3d)p * (maximize switch { true => direction, false => -direction }), tolerance: 0.0)
                .Head
                .ToFin(QuadrantsKey.InvalidResult()),
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
                pointAtCurve: static pac => Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, query: CurveAt<TGeometry, Point3d>(key: PointAtKey, parameter: pac.Parameter, project: static (curve, parameter) => One(key: PointAtKey, value: curve.PointAt(t: parameter)))),
                pointAtLength: static pal => Located<TGeometry, TOut, Curve, Point3d>(
                    key: PointAtLengthKey,
                    query: Query<TGeometry, Point3d>.Build(
                        key: PointAtLengthKey,
                        requirement: Requirement.CurveLength,
                        state: pal.Length,
                        evaluator: static (segmentLength, geometry) => geometry switch {
                            Curve curve =>
                                from ctx in Analyze.Asks
                                from result in (curve.LengthParameter(segmentLength: segmentLength, t: out double parameter, fractionalTolerance: ctx.Relative.Value) switch {
                                    true => One(key: PointAtLengthKey, value: curve.PointAt(t: parameter)),
                                    false => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.InvalidResult()),
                                }).ToEff()
                                select result,
                            _ => Fin.Fail<Seq<Point3d>>(PointAtLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))).ToEff(),
                        })),
                frameAtCurve: static fac => Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, query: CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
                perpendicularFrameAt: static pfa => Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, query: CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
                curvatureAtCurve: static cac => Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, query: CurveAt<TGeometry, Vector3d>(key: CurvatureAtKey, parameter: cac.Parameter, project: static (curve, parameter) => One(key: CurvatureAtKey, value: curve.CurvatureAt(t: parameter)))),
                derivativeAt: static da => Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, query: CurveAt<TGeometry, Vector3d>(key: DerivativeAtKey, parameter: da.Parameter, project: (curve, parameter) => Many(key: DerivativeAtKey, values: curve.DerivativeAt(t: parameter, derivativeCount: da.Count)))),
                divideByCount: static dbc => Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, query: DividePoly<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
                divideByLength: static dbl => Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, query: DividePoly<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
                orientation: static o => Located<TGeometry, TOut, Curve, CurveOrientation>(
                    key: OrientationKey,
                    query: Query<TGeometry, CurveOrientation>.Build(
                        key: OrientationKey,
                        state: o.Plane,
                        evaluator: static (plane, geometry) => geometry switch {
                            Curve curve => One(key: OrientationKey, value: curve.ClosedCurveOrientation(plane: plane)).ToEff(),
                            _ => Fin.Fail<Seq<CurveOrientation>>(OrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
                        })),
                contains: static cnt => Located<TGeometry, TOut, Curve, PointContainment>(
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
                pointAtSurface: static pas => Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, query: SurfaceUv<TGeometry, Point3d>(key: PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
                frameAtSurface: static fas => Located<TGeometry, TOut, Surface, Plane>(
                    key: FrameAtKey,
                    query: SurfaceUv<TGeometry, Plane>(
                        key: FrameAtKey,
                        uv: fas.Uv,
                        project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                            true => One(key: FrameAtKey, value: frame),
                            false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                        })),
                normalAt: static na => Located<TGeometry, TOut, Surface, Vector3d>(
                    key: NormalAtKey,
                    query: SurfaceUv<TGeometry, Vector3d>(
                        key: NormalAtKey,
                        uv: na.Uv,
                        project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                            Vector3d normal when normal.IsValid && !normal.IsTiny() => One(key: NormalAtKey, value: normal),
                            _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
                        })),
                curvatureAtSurface: static cas => Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, query: SurfaceUv<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
                shortPath: static sp => Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, query: ShortPath<TGeometry>(start: sp.Start, end: sp.End)),
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
    private static Query<TGeometry, TOut>? Located<TGeometry, TOut, TNative, TValue>(Op key, Query<TGeometry, TValue> query) where TGeometry : notnull => typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(TValue) ? Cast<TGeometry, TOut>(key: key, query: query) : null;
    private static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Point3d>(
            key: MidpointKey,
            line: static line => line.PointAt(t: 0.5),
            curve: static (curve, parameter) => curve.PointAt(t: parameter));
    private static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Vector3d>(
            key: TangentKey,
            line: static line => line.UnitTangent,
            curve: static (curve, parameter) => curve.TangentAt(t: parameter));
    private static Query<TGeometry, TOut> Middle<TGeometry, TOut, TValue>(
        Op key,
        Func<Line, TValue> line,
        Func<Curve, double, TValue> curve) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(TValue) =>
                Cast<TGeometry, TOut>(key: key, query: Query<Line, TValue>.Build(
                    key: key,
                    state: (Key: key, Project: line),
                    evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(TValue) =>
                Cast<TGeometry, TOut>(key: key, query: Query<Polyline, TValue>.Build(
                    key: key,
                    state: (Key: key, Project: curve),
                    evaluator: static (state, geometry) => Bracket(factory: geometry.ToPolylineCurve, body: polyCurve => polyCurve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                        true => One(key: state.Key, value: state.Project(arg1: polyCurve, arg2: parameter)),
                        false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(TValue) =>
                Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
                    key: key,
                    requirement: Requirement.CurveLength,
                    state: (Key: key, Project: curve),
                    evaluator: static (state, geometry) => CurveAtNormalized<TGeometry, TValue>(
                        geometry: geometry,
                        key: state.Key,
                        project: state.Project))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> CurvatureProfile<TGeometry, TOut>(int count, CurvatureScalar scalar) where TGeometry : notnull =>
        (count, scalar, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) =>
                Query<TGeometry, TOut>.Reject(key: CurvatureAtKey, fault: CurvatureAtKey.InvalidInput()),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                CurvatureQuery<TGeometry, TOut, Curve, Vector3d>(requirement: Requirement.CurveLength, count: count, project: static (curve, sampleCount, ctx) => CurveCurvatures(curve: curve, count: sampleCount, model: ctx)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                ScalarCurvature<TGeometry, TOut, Curve>(requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (curve, sampleCount, ctx) => CurveMagnitudes(curve: curve, count: sampleCount, model: ctx)),
            (_, CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) =>
                ScalarCurvature<TGeometry, TOut, Curve>(requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (curve, sampleCount, ctx) => CurveMagnitudes(curve: curve, count: sampleCount, model: ctx)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) =>
                CurvatureQuery<TGeometry, TOut, Surface, SurfaceCurvature>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, ctx) => SurfaceCurvatures(surface: surface, count: sampleCount, model: ctx)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) =>
                CurvatureQuery<TGeometry, TOut, Surface, CurvatureProfile>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, ctx) => SurfaceCurvatures(surface: surface, count: sampleCount, model: ctx).Bind(static curvatures => (SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Gaussian).Bind(static values => Profile(scalar: CurvatureScalar.Gaussian, values: values)), SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Mean).Bind(static values => Profile(scalar: CurvatureScalar.Mean, values: values))).Apply(static (gaussian, mean) => Seq(gaussian, mean)).As())),
            (_, CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) =>
                ScalarCurvature<TGeometry, TOut, Surface>(requirement: Requirement.SurfaceEvaluation, count: count, scalar: scalar, project: (surface, sampleCount, ctx) => SurfaceCurvatures(surface: surface, count: sampleCount, model: ctx).Bind(curvatures => SurfaceScalars(curvatures: curvatures, scalar: scalar))),
            _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> ScalarCurvature<TGeometry, TOut, TNative>(Requirement requirement, int count, CurvatureScalar scalar, Func<TNative, int, Context, Fin<Seq<double>>> project) where TGeometry : notnull where TNative : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(double) =>
                CurvatureQuery<TGeometry, TOut, TNative, double>(requirement: requirement, count: count, project: (native, sampleCount, ctx) => project(arg1: native, arg2: sampleCount, arg3: ctx).Bind(static values => Many(key: CurvatureAtKey, values: values))),
            Type output when output == typeof(CurvatureProfile) =>
                CurvatureQuery<TGeometry, TOut, TNative, CurvatureProfile>(requirement: requirement, count: count, project: (native, sampleCount, ctx) => project(arg1: native, arg2: sampleCount, arg3: ctx).Bind(values => Profile(scalar: scalar, values: values).Map(static profile => Seq(profile)))),
            _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> CurvatureQuery<TGeometry, TOut, TNative, TValue>(Requirement requirement, int count, Func<TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, (int Count, Func<TNative, int, Context, Fin<Seq<TValue>>> Project)>(
            key: CurvatureAtKey,
            state: (Count: count, Project: project),
            requirement: requirement,
            requiresContext: true,
            project: static (state, native) =>
                from ctx in Analyze.Asks
                from result in state.Project(arg1: native, arg2: state.Count, arg3: ctx).ToEff()
                select result);
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
    private static Fin<Seq<double>> CurveMagnitudes(Curve curve, int count, Context model) => CurveCurvatures(curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
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
    private static Fin<CurvatureProfile> Profile(CurvatureScalar scalar, Seq<double> values) => Stats.From(values: values, key: CurvatureAtKey).Map(s => new CurvatureProfile(Scalar: scalar, Count: s.Count, Minimum: s.Minimum, Maximum: s.Maximum, Mean: s.Mean, Variance: s.Variance));
    private static Fin<Seq<double>> Samples(Interval domain, int count, Op key) =>
        domain.IsValid switch {
            true => Fractions(count: count, key: key).Map(fractions => fractions.Map(f => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    private static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        point.IsValid switch {
            false => Query<TGeometry, TOut>.Reject(key: ClosestKey, fault: ClosestKey.InvalidInput()),
            true => (typeof(TGeometry), typeof(TOut)) switch {
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
            },
        };
    private static Query<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(
            key: key,
            parameter: parameter,
            project: (curve, t) => perpendicular switch {
                true => key.Solved(
                    isSolved: curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame),
                    value: perpendicularFrame),
                false => key.Solved(
                    isSolved: curve.FrameAt(t: t, plane: out Plane frame),
                    value: frame),
            });
    private static Query<TGeometry, TOut> CurveAt<TGeometry, TOut>(
        Op key,
        double parameter,
        Func<Curve, double, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Native<TGeometry, TOut, Curve, TOut, (Op Key, double Parameter, Func<Curve, double, Fin<Seq<TOut>>> Project)>(
            key: key,
            state: (Key: key, Parameter: parameter, Project: project),
            project: static (state, curve) => (curve.Domain.IncludesParameter(t: state.Parameter) switch {
                true => state.Project(arg1: curve, arg2: state.Parameter),
                false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
            }).ToEff());
    private static Query<TGeometry, Point3d> DividePoly<TGeometry>(
        Op key,
        Requirement? requirement,
        Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: key,
            requirement: requirement,
            state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => (geometry switch {
                Curve curve => state.Divide(arg: curve)
                    .ToFin(state.Key.InvalidResult())
                    .Bind(points => Many(key: state.Key, values: points)),
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
        Native<TGeometry, TOut, Surface, TOut, (Op Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project)>(
            key: key,
            requirement: Requirement.SurfaceEvaluation,
            state: (Key: key, Uv: uv, Project: project),
            project: static (state, surface) =>
                from ctx in Analyze.Asks
                from parameter in Uv(surface: surface, uv: state.Uv, context: ctx, key: state.Key).ToEff()
                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                select result);
    private static Fin<Point2d> Uv(Surface surface, Point2d uv, Context context, Op key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y) && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
}
