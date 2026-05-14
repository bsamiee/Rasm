namespace Rasm.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
internal readonly record struct Stats {
    private Stats(int count, double minimum, double maximum, double mean, double variance) {
        Count = count;
        Minimum = minimum;
        Maximum = maximum;
        Mean = mean;
        Variance = variance;
    }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms => Math.Sqrt(d: (Mean * Mean) + Variance);
    internal static Fin<Stats> From(Seq<double> values, Op key) =>
        values.Fold(
            initialState: (Count: 0, Mean: 0.0, M2: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static (state, value) => (Count: state.Count + 1, Delta: value - state.Mean) switch {
                (int count, double delta) => (
                    Count: count, Mean: state.Mean + (delta / count), M2: state.M2 + (delta * (value - (state.Mean + (delta / count)))), Minimum: Math.Min(val1: state.Minimum, val2: value), Maximum: Math.Max(val1: state.Maximum, val2: value), AllFinite: state.AllFinite && RhinoMath.IsValidDouble(x: value)),
            }) switch {
                (0, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                (_, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                (int count, double mean, double m2, double minimum, double maximum, _) => Fin.Succ(new Stats(
                    count: count, minimum: minimum, maximum: maximum, mean: mean, variance: Math.Max(val1: 0.0, val2: m2 / count))),
            };
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static partial class Query {
    internal static Fin<Seq<Point3d>> ExtractCardinals(Op op, Curve curve, double tolerance) =>
        Seq((Direction: Vector3d.XAxis, Maximize: false), (Direction: Vector3d.XAxis, Maximize: true), (Direction: Vector3d.YAxis, Maximize: false), (Direction: Vector3d.YAxis, Maximize: true), (Direction: Vector3d.ZAxis, Maximize: false), (Direction: Vector3d.ZAxis, Maximize: true))
            .Take(curve.IsPlanar(tolerance: tolerance) switch { true => 4, false => 6 })
            .TraverseM(state => toSeq(curve.ExtremeParameters(direction: state.Direction)).Map(curve.PointAt)
                .Maxima(projection: p => (Vector3d)p * (state.Maximize switch { true => state.Direction, false => -state.Direction }), tolerance: 0.0)
                .Head.ToFin(op.InvalidResult()))
            .As();
    public static Query<TGeometry, TOut> Locate<TGeometry, TOut>(Location aspect) where TGeometry : notnull => Aspect<Location, TGeometry, TOut>(aspect: aspect);
    internal static Query<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<Query<TGeometry, TValue>> query) where TGeometry : notnull =>
        (typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue)
            ? Cast<TGeometry, TOut>(key: key, query: query())
            : key.Unsupported<TGeometry, TOut>();
    internal static Query<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Point3d>(key: Op.Of(name: "Midpoint"), line: static line => line.PointAt(t: 0.5), curve: static (curve, parameter) => curve.PointAt(t: parameter));
    internal static Query<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Vector3d>(key: Op.Of(name: "Tangent"), line: static line => line.UnitTangent, curve: static (curve, parameter) => curve.TangentAt(t: parameter));
    internal static Query<TGeometry, TOut> Middle<TGeometry, TOut, TValue>(Op key, Func<Line, TValue> line, Func<Curve, double, TValue> curve) where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<Line, TValue>.Build(
                key: key, state: (Key: key, Project: line), evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
        (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<Polyline, TValue>.Build(
                key: key, state: (Key: key, Project: curve), evaluator: static (state, geometry) => Bracket(factory: geometry.ToPolylineCurve, body: polyCurve => polyCurve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                    true => One(key: state.Key, value: state.Project(arg1: polyCurve, arg2: parameter)),
                    false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                }).ToEff())),
        (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
                key: key, requirement: Requirement.CurveLength, state: (Key: key, Project: curve), evaluator: static (state, geometry) => CurveAtNormalized<TGeometry, TValue>(geometry: geometry, key: state.Key, project: state.Project))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    internal static Query<TGeometry, TOut> CurvatureProfile<TGeometry, TOut>(int count, CurvatureScalar scalar) where TGeometry : notnull {
        Op key = Op.Of(name: "CurvatureAt");
        return (count, scalar, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => CurvatureQuery<TGeometry, TOut, Curve, Vector3d>(key: key, requirement: Requirement.CurveLength, count: count, project: static (op, curve, sampleCount, context) => CurveCurvatures(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.Magnitude, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, scalar: CurvatureScalar.Magnitude, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) => CurvatureQuery<TGeometry, TOut, Surface, SurfaceCurvature>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context)),
            (_, CurvatureScalar.None, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(CurvatureProfile) => CurvatureQuery<TGeometry, TOut, Surface, CurvatureProfile>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => (SurfaceScalars(key: op, curvatures: curvatures, scalar: CurvatureScalar.Gaussian).Bind(values => Profile(key: op, scalar: CurvatureScalar.Gaussian, values: values)), SurfaceScalars(key: op, curvatures: curvatures, scalar: CurvatureScalar.Mean).Bind(values => Profile(key: op, scalar: CurvatureScalar.Mean, values: values))).Apply(static (gaussian, mean) => Seq(gaussian, mean)).As())),
            (_, CurvatureScalar.Gaussian or CurvatureScalar.Mean, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(CurvatureProfile)) => ScalarCurvature<TGeometry, TOut, Surface>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, scalar: scalar, project: (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => SurfaceScalars(key: op, curvatures: curvatures, scalar: scalar))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static Query<TGeometry, TOut> ScalarCurvature<TGeometry, TOut, TNative>(Op key, Requirement requirement, int count, CurvatureScalar scalar, Func<Op, TNative, int, Context, Fin<Seq<double>>> project) where TGeometry : notnull where TNative : notnull => typeof(TOut) switch {
        Type output when output == typeof(double) => CurvatureQuery<TGeometry, TOut, TNative, double>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => Many(key: op, values: values))),
        Type output when output == typeof(CurvatureProfile) => CurvatureQuery<TGeometry, TOut, TNative, CurvatureProfile>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => Profile(key: op, scalar: scalar, values: values).Map(static profile => Seq(profile)))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    private static Query<TGeometry, TOut> CurvatureQuery<TGeometry, TOut, TNative, TValue>(Op key, Requirement requirement, int count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, (Op Key, int Count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> Project)>(
            key: key, state: (Key: key, Count: count, Project: project), requirement: requirement, requiresContext: true,
            project: static (state, native) => from context in Env.Asks
                                               from result in state.Project(arg1: state.Key, arg2: native, arg3: state.Count, arg4: context).ToEff()
                                               select result);
    private static Fin<Seq<double>> SurfaceScalars(Op key, Seq<SurfaceCurvature> curvatures, CurvatureScalar scalar) =>
        scalar switch {
            CurvatureScalar.Gaussian => Fin.Succ(curvatures.Map(static curvature => curvature.Gaussian)),
            CurvatureScalar.Mean => Fin.Succ(curvatures.Map(static curvature => curvature.Mean)),
            _ => Fin.Fail<Seq<double>>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Vector3d>> CurveCurvatures(Op key, Curve curve, int count, Context model) =>
        CurveSamples(curve: curve, count: count, model: model, key: key)
            .Bind(parameters => Many(key: key, values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudes(Op key, Curve curve, int count, Context model) =>
        CurveCurvatures(key: key, curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Op key, Surface surface, int resolution, Context model) =>
        (Samples(domain: surface.Domain(direction: 0), resolution: resolution, key: key),
         Samples(domain: surface.Domain(direction: 1), resolution: resolution, key: key))
        .Apply(static (u, v) => (U: u, V: v)).As()
        .Bind(samples => samples.U
            .Bind(u => samples.V.Map(v => new Point2d(x: u, y: v)))
            .TraverseM(uv => Uv(surface: surface, uv: uv, context: model, key: key)
                .Bind(parameter => Optional(surface.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(key.InvalidResult())))
            .As());
    private static Fin<Seq<double>> CurveSamples(Curve curve, int count, Context model, Op key) =>
        Dispatch.Fractions(count: count, op: key)
            .Bind(fractions => Optional(curve.NormalizedLengthParameters(s: [.. fractions.AsIterable()], absoluteTolerance: model.Absolute.Value, fractionalTolerance: model.Relative.Value))
                .ToFin(key.InvalidResult())
                .Map(static parameters => toSeq(parameters)));
    private static Fin<CurvatureProfile> Profile(Op key, CurvatureScalar scalar, Seq<double> values) =>
        Stats.From(values: values, key: key).Map(s => new CurvatureProfile(Scalar: scalar, Count: s.Count, Minimum: s.Minimum, Maximum: s.Maximum, Mean: s.Mean, Variance: s.Variance));
    internal static Fin<Seq<double>> Samples(Interval domain, int resolution, Op key) =>
        domain.IsValid switch {
            true => Dispatch.Fractions(count: resolution, op: key).Map(fractions => fractions.Map(f => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    internal static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid switch {
            false => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => Query<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: point), requiresContext: true,
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from hit in kind.Closest(value: geometry, target: state.Target, ctx: context, op: state.Key).ToEff()
                    from result in (typeof(TOut) switch {
                        Type t when t == typeof(Point3d) => state.Key.Results<Point3d, TOut>(values: Seq(hit.Point)),
                        Type t when t == typeof(double) => hit.Distance.ToFin(Fail: state.Key.InvalidResult()).Bind(d => state.Key.Results<double, TOut>(values: Seq(d))),
                        Type t when t == typeof(Vector3d) => hit.Normal.ToFin(Fail: state.Key.InvalidResult()).Bind(n => state.Key.Results<Vector3d, TOut>(values: Seq(n))),
                        Type t when t == typeof(ComponentIndex) => hit.Component.ToFin(Fail: state.Key.InvalidResult()).Bind(c => state.Key.Results<ComponentIndex, TOut>(values: Seq(c))),
                        Type t when t == typeof(MeshPoint) => hit.MeshPoint.ToFin(Fail: state.Key.InvalidResult()).Bind(mp => state.Key.Results<MeshPoint, TOut>(values: Seq(mp))),
                        _ => Fin.Fail<Seq<TOut>>(error: state.Key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                    }).ToEff()
                    select result),
        };
    }
    internal static Query<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(key: key, parameter: parameter, project: (curve, t) => perpendicular switch {
            true => key.Solved(isSolved: curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame), value: perpendicularFrame),
            false => key.Solved(isSolved: curve.FrameAt(t: t, plane: out Plane frame), value: frame),
        });
    internal static Query<TGeometry, TOut> CurveAt<TGeometry, TOut>(Op key, double parameter, Func<Curve, double, Fin<Seq<TOut>>> project, Requirement? requirement = null) where TGeometry : notnull =>
        Native<TGeometry, TOut, Curve, TOut, (Op Key, double Parameter, Func<Curve, double, Fin<Seq<TOut>>> Project)>(
            key: key, requirement: requirement ?? Requirement.Basic, state: (Key: key, Parameter: parameter, Project: project),
            project: static (state, curve) => (curve.Domain.IncludesParameter(t: state.Parameter) switch {
                true => state.Project(arg1: curve, arg2: state.Parameter),
                false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
            }).ToEff());
    internal static Query<TGeometry, Point3d> DividePoly<TGeometry>(Op key, Requirement? requirement, Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Query<TGeometry, Point3d>.Build(
            key: key, requirement: requirement, state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => (geometry switch {
                Curve curve => state.Divide(arg: curve)
                    .ToFin(state.Key.InvalidResult())
                    .Bind(points => Many(key: state.Key, values: points)),
                _ => Fin.Fail<Seq<Point3d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            }).ToEff());
    internal static Query<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull {
        Op key = Op.Of();
        return Query<TGeometry, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation,
            state: (Key: key, Start: start, End: end),
            evaluator: static (state, geometry) => geometry switch {
                Surface surface => from context in Env.Asks
                                   from uvStart in Uv(surface: surface, uv: state.Start, context: context, key: state.Key).ToEff()
                                   from uvEnd in Uv(surface: surface, uv: state.End, context: context, key: state.Key).ToEff()
                                   from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: context.Absolute.Value))
                                       .ToFin(state.Key.InvalidResult())
                                       .ToEff()
                                   select Seq(path),
                _ => Fin.Fail<Seq<Curve>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))).ToEff(),
            });
    }
    internal static Query<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(Op key, Point2d uv, Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Native<TGeometry, TOut, Surface, TOut, (Op Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project)>(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Uv: uv, Project: project),
            project: static (state, surface) => from context in Env.Asks
                                                from parameter in Uv(surface: surface, uv: state.Uv, context: context, key: state.Key).ToEff()
                                                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                                                select result);
    private static Fin<Point2d> Uv(Surface surface, Point2d uv, Context context, Op key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y) && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
}
