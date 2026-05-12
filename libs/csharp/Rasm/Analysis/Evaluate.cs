namespace Rasm.Analysis;

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Stats {
    private Stats(int count, double minimum, double maximum, double mean, double variance, double rms) {
        Count = count;
        Minimum = minimum;
        Maximum = maximum;
        Mean = mean;
        Variance = variance;
        Rms = rms;
    }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms { get; }
    internal static Fin<Stats> From(Seq<double> values, Op key) =>
        values.Fold(
            initialState: (Count: 0, Mean: 0.0, M2: 0.0, SumSquares: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static (state, value) => (Count: state.Count + 1, Delta: value - state.Mean, Square: value * value) switch {
                (int count, double delta, double square) => (
                    Count: count, Mean: state.Mean + (delta / count), M2: state.M2 + (delta * (value - (state.Mean + (delta / count)))), SumSquares: state.SumSquares + square, Minimum: Math.Min(val1: state.Minimum, val2: value), Maximum: Math.Max(val1: state.Maximum, val2: value), AllFinite: state.AllFinite && RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: square)),
            }) switch {
                (0, _, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                (_, _, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                (int count, double mean, double m2, double sumSquares, double minimum, double maximum, _) => Fin.Succ(new Stats(
                    count: count, minimum: minimum, maximum: maximum, mean: mean, variance: Math.Max(val1: 0.0, val2: m2 / count), rms: Math.Sqrt(d: sumSquares / count))),
            };
}

public static partial class Query {
    public static Query<TGeometry, TOut> Quadrants<TGeometry, TOut>() where TGeometry : notnull => typeof(TOut) switch {
        Type output when output == typeof(Point3d) => Cast<TGeometry, TOut>(key: QuadrantsKey, query: Query<TGeometry, Point3d>.Build(
            key: QuadrantsKey,
            requirement: Requirement.CurveLength,
            evaluator: static geometry => from context in Analyze.Asks
                                          from result in QuadrantsFromGeom(geometry: geometry, tolerance: context.Absolute.Value).ToEff()
                                          select result)),
        _ => QuadrantsKey.Unsupported<TGeometry, TOut>(),
    };
    internal static Fin<Seq<Point3d>> QuadrantsFromGeom<TGeometry>(TGeometry geometry, double tolerance) where TGeometry : notnull =>
        geometry switch {
            Curve curve when curve.IsValid => ExtractCardinals(curve: curve, tolerance: tolerance),
            Polyline polyline when polyline.IsValid => Bracket(factory: polyline.ToPolylineCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Line line when line.IsValid => Bracket(factory: () => new LineCurve(line: line), body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Circle circle when circle.IsValid => Bracket(factory: circle.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            Arc arc when arc.IsValid => Bracket(factory: arc.ToNurbsCurve, body: (Curve curve) => ExtractCardinals(curve: curve, tolerance: tolerance)),
            _ => Fin.Fail<Seq<Point3d>>(QuadrantsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
        };
    internal static Fin<Seq<Point3d>> ExtractCardinals(Curve curve, double tolerance) =>
        Seq((Direction: Vector3d.XAxis, Maximize: false), (Direction: Vector3d.XAxis, Maximize: true), (Direction: Vector3d.YAxis, Maximize: false), (Direction: Vector3d.YAxis, Maximize: true), (Direction: Vector3d.ZAxis, Maximize: false), (Direction: Vector3d.ZAxis, Maximize: true))
            .Take(curve.IsPlanar(tolerance: tolerance) switch { true => 4, false => 6 })
            .TraverseM(state => ExtremeAlongDirection(curve: curve, direction: state.Direction, maximize: state.Maximize))
            .As();
    internal static Fin<Point3d> ExtremeAlongDirection(Curve curve, Vector3d direction, bool maximize) =>
        toSeq(curve.ExtremeParameters(direction: direction)).Map(curve.PointAt) switch {
            Seq<Point3d> points => points
                .Maxima(projection: p => (Vector3d)p * (maximize switch { true => direction, false => -direction }), tolerance: 0.0)
                .Head
                .ToFin(QuadrantsKey.InvalidResult()),
        };
    public static Query<TGeometry, TOut> Locate<TGeometry, TOut>(Location aspect) where TGeometry : notnull {
        ArgumentNullException.ThrowIfNull(argument: aspect);
        return aspect.Apply<TGeometry, TOut>();
    }
    internal static Query<TGeometry, TOut> ControlPoints<TGeometry, TOut>() where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when output == typeof(Point3d) && (typeof(Curve).IsAssignableFrom(c: geometry) || typeof(Surface).IsAssignableFrom(c: geometry) || typeof(Brep).IsAssignableFrom(c: geometry) || geometry == typeof(object)) => Cast<TGeometry, TOut>(key: ControlPointsKey, query: Query<TGeometry, Point3d>.Build(
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
    internal static Fin<Seq<Point3d>> CurveControlPoints(NurbsCurve curve) =>
        Many(
            key: ControlPointsKey,
            values: Enumerable.Range(start: 0, count: curve.Points.Count)
                .Select(index => curve.Points[index].Location));
    internal static Fin<Seq<Point3d>> SurfaceControlPoints(NurbsSurface surface) =>
        Many(
            key: ControlPointsKey,
            values: Enumerable.Range(start: 0, count: surface.Points.CountU)
                .SelectMany(u => Enumerable.Range(start: 0, count: surface.Points.CountV)
                    .Select(v => surface.Points.GetControlPoint(u: u, v: v).Location)));
    internal static Fin<Seq<Point3d>> BrepControlPoints(Brep brep) =>
        toSeq(brep.Faces)
            .TraverseM(face => Optional(face.ToNurbsSurface())
                .ToFin(ControlPointsKey.InvalidResult())
                .Bind(static nurbs => { using NurbsSurface disposable = nurbs; return SurfaceControlPoints(surface: disposable); }))
            .As()
            .Map(static nested => nested.Bind(static points => points));
    internal static Query<TGeometry, TOut>? Located<TGeometry, TOut, TNative, TValue>(Op key, Query<TGeometry, TValue> query) where TGeometry : notnull => typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) && typeof(TOut) == typeof(TValue) ? Cast<TGeometry, TOut>(key: key, query: query) : null;
    internal static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Point3d>(
            key: MidpointKey,
            line: static line => line.PointAt(t: 0.5),
            curve: static (curve, parameter) => curve.PointAt(t: parameter));
    internal static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Vector3d>(
            key: TangentKey,
            line: static line => line.UnitTangent,
            curve: static (curve, parameter) => curve.TangentAt(t: parameter));
    internal static Query<TGeometry, TOut> Middle<TGeometry, TOut, TValue>(
        Op key,
        Func<Line, TValue> line,
        Func<Curve, double, TValue> curve) where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<Line, TValue>.Build(
                    key: key, state: (Key: key, Project: line), evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<Polyline, TValue>.Build(
                    key: key, state: (Key: key, Project: curve), evaluator: static (state, geometry) => Bracket(factory: geometry.ToPolylineCurve, body: polyCurve => polyCurve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                        true => One(key: state.Key, value: state.Project(arg1: polyCurve, arg2: parameter)),
                        false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                    }).ToEff())),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
                    key: key, requirement: Requirement.CurveLength, state: (Key: key, Project: curve), evaluator: static (state, geometry) => CurveAtNormalized<TGeometry, TValue>(
                        geometry: geometry, key: state.Key, project: state.Project))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    internal static Query<TGeometry, TOut> CurvatureProfile<TGeometry, TOut>(int count, CurvatureScalar scalar) where TGeometry : notnull =>
        (count, scalar, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) => Query<TGeometry, TOut>.Reject(key: CurvatureAtKey, fault: CurvatureAtKey.InvalidInput()),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => CurvatureQuery<TGeometry, TOut, Curve, Vector3d>(requirement: Requirement.CurveLength, count: count, project: static (curve, sampleCount, context) => CurveCurvatures(curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) => ScalarCurvature<TGeometry, TOut, Curve>(requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (curve, sampleCount, context) => CurveMagnitudes(curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) => ScalarCurvature<TGeometry, TOut, Curve>(requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (curve, sampleCount, context) => CurveMagnitudes(curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) => CurvatureQuery<TGeometry, TOut, Surface, SurfaceCurvature>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, context) => SurfaceCurvatures(surface: surface, count: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) => CurvatureQuery<TGeometry, TOut, Surface, CurvatureProfile>(requirement: Requirement.SurfaceEvaluation, count: count, project: static (surface, sampleCount, context) => SurfaceCurvatures(surface: surface, count: sampleCount, model: context).Bind(static curvatures => (SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Gaussian).Bind(static values => Profile(scalar: CurvatureScalar.Gaussian, values: values)), SurfaceScalars(curvatures: curvatures, scalar: CurvatureScalar.Mean).Bind(static values => Profile(scalar: CurvatureScalar.Mean, values: values))).Apply(static (gaussian, mean) => Seq(gaussian, mean)).As())),
            (_, CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) => ScalarCurvature<TGeometry, TOut, Surface>(requirement: Requirement.SurfaceEvaluation, count: count, scalar: scalar, project: (surface, sampleCount, context) => SurfaceCurvatures(surface: surface, count: sampleCount, model: context).Bind(curvatures => SurfaceScalars(curvatures: curvatures, scalar: scalar))),
            _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        };
    internal static Query<TGeometry, TOut> ScalarCurvature<TGeometry, TOut, TNative>(Requirement requirement, int count, CurvatureScalar scalar, Func<TNative, int, Context, Fin<Seq<double>>> project) where TGeometry : notnull where TNative : notnull => typeof(TOut) switch {
        Type output when output == typeof(double) => CurvatureQuery<TGeometry, TOut, TNative, double>(requirement: requirement, count: count, project: (native, sampleCount, context) => project(arg1: native, arg2: sampleCount, arg3: context).Bind(static values => Many(key: CurvatureAtKey, values: values))),
        Type output when output == typeof(CurvatureProfile) => CurvatureQuery<TGeometry, TOut, TNative, CurvatureProfile>(requirement: requirement, count: count, project: (native, sampleCount, context) => project(arg1: native, arg2: sampleCount, arg3: context).Bind(values => Profile(scalar: scalar, values: values).Map(static profile => Seq(profile)))),
        _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
    };
    internal static Query<TGeometry, TOut> CurvatureQuery<TGeometry, TOut, TNative, TValue>(Requirement requirement, int count, Func<TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, (int Count, Func<TNative, int, Context, Fin<Seq<TValue>>> Project)>(
            key: CurvatureAtKey,
            state: (Count: count, Project: project),
            requirement: requirement,
            requiresContext: true,
            project: static (state, native) => from context in Analyze.Asks
                                               from result in state.Project(arg1: native, arg2: state.Count, arg3: context).ToEff()
                                               select result);
    internal static Fin<Seq<double>> SurfaceScalars(Seq<SurfaceCurvature> curvatures, CurvatureScalar scalar) =>
        scalar switch {
            CurvatureScalar.Gaussian => Fin.Succ(curvatures.Map(static curvature => curvature.Gaussian)),
            CurvatureScalar.Mean => Fin.Succ(curvatures.Map(static curvature => curvature.Mean)),
            _ => Fin.Fail<Seq<double>>(CurvatureAtKey.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    internal static Fin<Seq<Vector3d>> CurveCurvatures(Curve curve, int count, Context model) =>
        CurveSamples(
                curve: curve,
                count: count,
                model: model,
                key: CurvatureAtKey)
            .Bind(parameters => Many(
                key: CurvatureAtKey,
                values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    internal static Fin<Seq<double>> CurveMagnitudes(Curve curve, int count, Context model) => CurveCurvatures(curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    internal static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Surface surface, int count, Context model) =>
        (
            Samples(domain: surface.Domain(direction: 0), count: count, key: CurvatureAtKey),
            Samples(domain: surface.Domain(direction: 1), count: count, key: CurvatureAtKey)
        ).Apply(static (u, v) => (U: u, V: v)).As()
        .Bind(samples => samples.U
            .Bind(u => samples.V.Map(v => new Point2d(x: u, y: v)))
            .TraverseM(uv => Uv(surface: surface, uv: uv, context: model, key: CurvatureAtKey)
                .Bind(parameter => Optional(surface.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult())))
            .As());
    internal static Fin<Seq<double>> CurveSamples(Curve curve, int count, Context model, Op key) =>
        Fractions(count: count, key: key)
            .Bind(fractions => Optional(curve.NormalizedLengthParameters(
                    s: [.. fractions.AsIterable()], absoluteTolerance: model.Absolute.Value, fractionalTolerance: model.Relative.Value))
                .ToFin(key.InvalidResult())
                .Map(static parameters => toSeq(parameters)));
    internal static Fin<Seq<double>> Fractions(int count, Op key) =>
        count switch {
            1 => Fin.Succ(Seq(0.5)),
            > 1 => Fin.Succ(toSeq(Enumerable.Range(start: 0, count: count).Select(i => i / (count - 1.0)))),
            _ => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    internal static Fin<CurvatureProfile> Profile(CurvatureScalar scalar, Seq<double> values) => Stats.From(values: values, key: CurvatureAtKey).Map(s => new CurvatureProfile(Scalar: scalar, Count: s.Count, Minimum: s.Minimum, Maximum: s.Maximum, Mean: s.Mean, Variance: s.Variance));
    internal static Fin<Seq<double>> Samples(Interval domain, int count, Op key) =>
        domain.IsValid switch {
            true => Fractions(count: count, key: key).Map(fractions => fractions.Map(f => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    internal static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        point.IsValid switch {
            false => Query<TGeometry, TOut>.Reject(key: ClosestKey, fault: ClosestKey.InvalidInput()),
            true => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Line, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target, limitToFiniteSegment: true))),
                (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Polyline, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
                (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Curve, Point3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(t: parameter)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Surface, Point3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, u: out double u, v: out double v) switch {
                    true => One(key: ClosestKey, value: geometry.PointAt(u: u, v: v)),
                    false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Brep, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
                (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) => ClosestMatch<TGeometry, TOut, Mesh, Point3d>(point: point, project: static (target, geometry) => One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
                (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) => ClosestMatch<TGeometry, TOut, Curve, double>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                    true => One(key: ClosestKey, value: parameter),
                    false => Fin.Fail<Seq<double>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => ClosestMatch<TGeometry, TOut, Brep, Vector3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex _, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d normal) switch {
                    true => One(key: ClosestKey, value: normal),
                    false => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => ClosestMatch<TGeometry, TOut, Mesh, Vector3d>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, pointOnMesh: out Point3d _, normalAtPoint: out Vector3d normal, maximumDistance: 0.0) switch {
                    >= 0 => One(key: ClosestKey, value: normal),
                    _ => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) => ClosestMatch<TGeometry, TOut, Brep, ComponentIndex>(point: point, project: static (target, geometry) => geometry.ClosestPoint(testPoint: target, closestPoint: out Point3d _, ci: out ComponentIndex component, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d _) switch {
                    true => One(key: ClosestKey, value: component),
                    false => Fin.Fail<Seq<ComponentIndex>>(ClosestKey.InvalidResult()),
                }),
                (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(MeshPoint) => ClosestMatch<TGeometry, TOut, Mesh, MeshPoint>(point: point, project: static (target, geometry) => Optional(geometry.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0))
                        .ToFin(ClosestKey.InvalidResult())
                        .Map(static meshPoint => Seq(meshPoint))),
                _ => ClosestKey.Unsupported<TGeometry, TOut>(),
            },
        };
    internal static Query<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(
            key: key,
            parameter: parameter,
            project: (curve, t) => perpendicular switch {
                true => key.Solved(
                    isSolved: curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame), value: perpendicularFrame),
                false => key.Solved(
                    isSolved: curve.FrameAt(t: t, plane: out Plane frame), value: frame),
            });
    internal static Query<TGeometry, TOut> CurveAt<TGeometry, TOut>(
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
    internal static Query<TGeometry, Point3d> DividePoly<TGeometry>(
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
    internal static Query<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull =>
        Query<TGeometry, Curve>.Build(
            key: ShortPathKey,
            requirement: Requirement.SurfaceEvaluation,
            state: (Start: start, End: end),
            evaluator: static (endpoints, geometry) => geometry switch {
                Surface surface => from context in Analyze.Asks
                                   from uvStart in Uv(surface: surface, uv: endpoints.Start, context: context, key: ShortPathKey).ToEff()
                                   from uvEnd in Uv(surface: surface, uv: endpoints.End, context: context, key: ShortPathKey).ToEff()
                                   from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: context.Absolute.Value))
                                       .ToFin(ShortPathKey.InvalidResult())
                                       .ToEff()
                                   select Seq(path),
                _ => Fin.Fail<Seq<Curve>>(ShortPathKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))).ToEff(),
            });
    internal static Query<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(
        Op key,
        Point2d uv,
        Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Native<TGeometry, TOut, Surface, TOut, (Op Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project)>(
            key: key,
            requirement: Requirement.SurfaceEvaluation,
            state: (Key: key, Uv: uv, Project: project),
            project: static (state, surface) => from context in Analyze.Asks
                                                from parameter in Uv(surface: surface, uv: state.Uv, context: context, key: state.Key).ToEff()
                                                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                                                select result);
    internal static Fin<Point2d> Uv(Surface surface, Point2d uv, Context context, Op key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y) && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
}

// --- [LOCATION_ROLE] ---------------------------------------------------------------------
internal static class LocationRole {
    internal static Query<TGeometry, TOut> Apply<TGeometry, TOut>(this Location aspect) where TGeometry : notnull => aspect.Switch(
        midpoint: static _ => Query.Mid<TGeometry, TOut>(),
        tangent: static _ => Query.TangentAtMiddle<TGeometry, TOut>(),
        closest: static c => Query.Closest<TGeometry, TOut>(point: c.Point),
        curvatureProfile: static cp => Query.CurvatureProfile<TGeometry, TOut>(count: cp.Count, scalar: cp.Scalar),
        pointAtCurve: static pac => Query.Located<TGeometry, TOut, Curve, Point3d>(key: Query.PointAtKey, query: Query.CurveAt<TGeometry, Point3d>(key: Query.PointAtKey, parameter: pac.Parameter, project: static (curve, parameter) => Query.One(key: Query.PointAtKey, value: curve.PointAt(t: parameter)))) ?? Query.PointAtKey.Unsupported<TGeometry, TOut>(),
        pointAtLength: static pal => Query.Located<TGeometry, TOut, Curve, Point3d>(
            key: Query.PointAtLengthKey, query: Query<TGeometry, Point3d>.Build(
                key: Query.PointAtLengthKey, requirement: Requirement.CurveLength, state: pal.Length, evaluator: static (segmentLength, geometry) => geometry switch {
                    Curve curve => from context in Analyze.Asks
                                   from result in (curve.LengthParameter(segmentLength: segmentLength, t: out double parameter, fractionalTolerance: context.Relative.Value) switch {
                                       true => Query.One(key: Query.PointAtLengthKey, value: curve.PointAt(t: parameter)),
                                       false => Fin.Fail<Seq<Point3d>>(Query.PointAtLengthKey.InvalidResult()),
                                   }).ToEff()
                                   select result,
                    _ => Fin.Fail<Seq<Point3d>>(Query.PointAtLengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))).ToEff(),
                })) ?? Query.PointAtLengthKey.Unsupported<TGeometry, TOut>(),
        frameAtCurve: static fac => Query.Located<TGeometry, TOut, Curve, Plane>(key: Query.FrameAtKey, query: Query.CurveFrame<TGeometry>(key: Query.FrameAtKey, parameter: fac.Parameter, perpendicular: false)) ?? Query.FrameAtKey.Unsupported<TGeometry, TOut>(),
        perpendicularFrameAt: static pfa => Query.Located<TGeometry, TOut, Curve, Plane>(key: Query.PerpendicularFrameAtKey, query: Query.CurveFrame<TGeometry>(key: Query.PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)) ?? Query.PerpendicularFrameAtKey.Unsupported<TGeometry, TOut>(),
        curvatureAtCurve: static cac => Query.Located<TGeometry, TOut, Curve, Vector3d>(key: Query.CurvatureAtKey, query: Query.CurveAt<TGeometry, Vector3d>(key: Query.CurvatureAtKey, parameter: cac.Parameter, project: static (curve, parameter) => Query.One(key: Query.CurvatureAtKey, value: curve.CurvatureAt(t: parameter)))) ?? Query.CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        derivativeAt: static da => Query.Located<TGeometry, TOut, Curve, Vector3d>(key: Query.DerivativeAtKey, query: Query.CurveAt<TGeometry, Vector3d>(key: Query.DerivativeAtKey, parameter: da.Parameter, project: (curve, parameter) => Query.Many(key: Query.DerivativeAtKey, values: curve.DerivativeAt(t: parameter, derivativeCount: da.Count)))) ?? Query.DerivativeAtKey.Unsupported<TGeometry, TOut>(),
        divideByCount: static dbc => Query.Located<TGeometry, TOut, Curve, Point3d>(key: Query.DivideByCountKey, query: Query.DividePoly<TGeometry>(key: Query.DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })) ?? Query.DivideByCountKey.Unsupported<TGeometry, TOut>(),
        divideByLength: static dbl => Query.Located<TGeometry, TOut, Curve, Point3d>(key: Query.DivideByLengthKey, query: Query.DividePoly<TGeometry>(key: Query.DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })) ?? Query.DivideByLengthKey.Unsupported<TGeometry, TOut>(),
        orientation: static o => Query.Located<TGeometry, TOut, Curve, CurveOrientation>(
            key: Query.OrientationKey, query: Query<TGeometry, CurveOrientation>.Build(
                key: Query.OrientationKey, state: o.Plane, evaluator: static (plane, geometry) => geometry switch {
                    Curve curve => Query.One(key: Query.OrientationKey, value: curve.ClosedCurveOrientation(plane: plane)).ToEff(),
                    _ => Fin.Fail<Seq<CurveOrientation>>(Query.OrientationKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
                })) ?? Query.OrientationKey.Unsupported<TGeometry, TOut>(),
        contains: static cnt => Query.Located<TGeometry, TOut, Curve, PointContainment>(
            key: Query.ContainsKey, query: Query<TGeometry, PointContainment>.Build(
                key: Query.ContainsKey, requiresContext: true, state: (Probe: cnt.Point, Frame: cnt.Plane), evaluator: static (probe, geometry) => geometry switch {
                    Curve curve => from context in Analyze.Asks
                                   from result in (curve.Contains(testPoint: probe.Probe, plane: probe.Frame, tolerance: context.Absolute.Value) switch {
                                       PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(Query.ContainsKey.InvalidResult()),
                                       PointContainment containment => Query.One(key: Query.ContainsKey, value: containment),
                                   }).ToEff()
                                   select result,
                    _ => Fin.Fail<Seq<PointContainment>>(Query.ContainsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))).ToEff(),
                })) ?? Query.ContainsKey.Unsupported<TGeometry, TOut>(),
        pointAtSurface: static pas => Query.Located<TGeometry, TOut, Surface, Point3d>(key: Query.PointAtKey, query: Query.SurfaceUv<TGeometry, Point3d>(key: Query.PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => Query.One(key: Query.PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))) ?? Query.PointAtKey.Unsupported<TGeometry, TOut>(),
        frameAtSurface: static fas => Query.Located<TGeometry, TOut, Surface, Plane>(
            key: Query.FrameAtKey, query: Query.SurfaceUv<TGeometry, Plane>(
                key: Query.FrameAtKey, uv: fas.Uv, project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                    true => Query.One(key: Query.FrameAtKey, value: frame),
                    false => Fin.Fail<Seq<Plane>>(Query.FrameAtKey.InvalidResult()),
                })) ?? Query.FrameAtKey.Unsupported<TGeometry, TOut>(),
        normalAt: static na => Query.Located<TGeometry, TOut, Surface, Vector3d>(
            key: Query.NormalAtKey, query: Query.SurfaceUv<TGeometry, Vector3d>(
                key: Query.NormalAtKey, uv: na.Uv, project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                    Vector3d normal when normal.IsValid && !normal.IsTiny() => Query.One(key: Query.NormalAtKey, value: normal),
                    _ => Fin.Fail<Seq<Vector3d>>(Query.NormalAtKey.InvalidResult()),
                })) ?? Query.NormalAtKey.Unsupported<TGeometry, TOut>(),
        curvatureAtSurface: static cas => Query.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: Query.CurvatureAtKey, query: Query.SurfaceUv<TGeometry, SurfaceCurvature>(key: Query.CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(Query.CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))) ?? Query.CurvatureAtKey.Unsupported<TGeometry, TOut>(),
        shortPath: static sp => Query.Located<TGeometry, TOut, Surface, Curve>(key: Query.ShortPathKey, query: Query.ShortPath<TGeometry>(start: sp.Start, end: sp.End)) ?? Query.ShortPathKey.Unsupported<TGeometry, TOut>(),
        controlPoints: static _ => Query.ControlPoints<TGeometry, TOut>());
}
