namespace Rasm.Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------
public static partial class Query {
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        Aspect<Bounds, TGeometry, TOut>(aspect: aspect, key: Op.Of());
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        Aspect<Measure, TGeometry, TOut>(aspect: aspect, key: Op.Of());
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        aspect?.ToQuery<TGeometry, TPrimitive, TOut>() ?? Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
    public static Query<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(Point3d) && typeof(TGeometry).SupportsBounds(includeSphere: false))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Env.Asks
                                                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                                                    from centroid in kind.Centroid(value: geometry, ctx: context, op: op).ToEff()
                                                    from result in One(key: op, value: centroid).ToEff()
                                                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return (aspect, typeof(TOut)) switch {
            (_, _) when mass.Equals(MassKind.None) => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (Analysis.Measure.Area, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Area, suffix: string.Empty, project: static (k, props) => props switch {
                AreaMassProperties area => One(key: k, value: area.Area),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.Volume, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Volume, suffix: string.Empty, project: static (k, props) => props switch {
                VolumeMassProperties volume => One(key: k, value: volume.Volume),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.MassError, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: mass, suffix: "Error", project: (k, props) => MassProject<double>(aspect: aspect, key: k, props: props), secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.Centroid, Type output) when output == typeof(Point3d) => MassCast<TGeometry, TOut, Point3d>(mass: mass, suffix: "Centroid", project: (k, props) => MassProject<Point3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.CentroidError, Type output) when output == typeof(Vector3d) => MassCast<TGeometry, TOut, Vector3d>(mass: mass, suffix: "CentroidError", project: (k, props) => MassProject<Vector3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.Radii, Type output) when output == typeof(Vector3d) => MassCast<TGeometry, TOut, Vector3d>(mass: mass, suffix: "Radii", project: (k, props) => MassProject<Vector3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: true),
            (Analysis.Measure.PrincipalAxes, Type output) when output == typeof(ValueTuple<double, Vector3d>) => MassCast<TGeometry, TOut, (double Moment, Vector3d Axis)>(mass: mass, suffix: "Principal", project: (k, props) => MassProject<(double Moment, Vector3d Axis)>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: true, productMoments: true),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static Query<TGeometry, TOut> MassCast<TGeometry, TOut, TValue>(MassKind mass, string suffix, Func<Op, IDisposable, Fin<Seq<TValue>>> project, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) where TGeometry : notnull {
        Op key = Op.Of(name: $"{mass.Label}{suffix}");
        return Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, requirement: mass.Requirement, requiresContext: true,
            aggregate: Some<Func<Seq<TGeometry>, Eff<Env, Seq<TValue>>>>(
                geometry => from context in Env.Asks
                            from native in geometry.Traverse(item => item switch {
                                GeometryBase gb => Fin.Succ(gb),
                                _ => Fin.Fail<GeometryBase>(key.Unsupported(geometryType: item.GetType(), outputType: typeof(GeometryBase))),
                            }).As().ToEff()
                            from aggregate in mass.Aggregate(arg1: native.AsIterable(), arg2: context, arg3: firstMoments, arg4: secondMoments, arg5: productMoments, arg6: key).ToEff()
                            from values in Bracket(factory: () => aggregate, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                            select values),
            evaluator: geometry => from computed in mass.Compute(value: geometry, op: key, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)
                                   from values in Bracket(factory: () => computed, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                                   select values));
    }
    private static Fin<Seq<TValue>> MassProject<TValue>(Measure aspect, Op key, IDisposable props) =>
        aspect switch {
            Analysis.Measure.MassError => key.PickMass<double, TValue>(props: props, length: static l => l.LengthError, area: static a => a.AreaError, volume: static v => v.VolumeError),
            Analysis.Measure.Centroid => key.PickMass<Point3d, TValue>(props: props, length: static l => l.Centroid, area: static a => a.Centroid, volume: static v => v.Centroid),
            Analysis.Measure.CentroidError => key.PickMass<Vector3d, TValue>(props: props, length: static l => l.CentroidError, area: static a => a.CentroidError, volume: static v => v.CentroidError),
            Analysis.Measure.Radii => key.PickMass<Vector3d, TValue>(props: props, length: static l => l.CentroidCoordinatesRadiiOfGyration, area: static a => a.CentroidCoordinatesRadiiOfGyration, volume: static v => v.CentroidCoordinatesRadiiOfGyration),
            Analysis.Measure.PrincipalAxes => key.Principal(mass: props).Bind(values => key.Results<(double Moment, Vector3d Axis), TValue>(values: values)),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    private static Fin<Seq<TValue>> PickMass<TProp, TValue>(this Op key, IDisposable props, Func<LengthMassProperties, TProp> length, Func<AreaMassProperties, TProp> area, Func<VolumeMassProperties, TProp> volume) =>
        props switch {
            LengthMassProperties l => key.Results<TProp, TValue>(values: Seq(length(arg: l))),
            AreaMassProperties a => key.Results<TProp, TValue>(values: Seq(area(arg: a))),
            VolumeMassProperties v => key.Results<TProp, TValue>(values: Seq(volume(arg: v))),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    internal static Query<TGeometry, TOut> BoxMetric<TGeometry, TOut>(Op key, Func<BoundingBox, double> boundingBox, Func<Box, double> box) where TGeometry : notnull =>
        (typeof(TOut) == typeof(double), typeof(TGeometry)) switch {
            (true, Type geometry) when geometry == typeof(BoundingBox) => Cast<TGeometry, TOut>(key: key, query: Query<BoundingBox, double>.Build(
                key: key, state: (Key: key, Project: boundingBox),
                evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            (true, Type geometry) when geometry == typeof(Box) => Cast<TGeometry, TOut>(key: key, query: Query<Box, double>.Build(
                key: key, state: (Key: key, Project: box),
                evaluator: static (state, geometry) => state.Key.RequireValid(value: geometry).Bind(validated => One(key: state.Key, value: state.Project(arg: validated))).ToEff())),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    internal static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(double), typeof(TGeometry).AsKind().Case) switch {
            (true, Kind { Topology: Topology.Curve } kind) => Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, double>.Build(
                key: key,
                requirement: kind.IsGeometryBaseDerived() ? Requirement.CurveLength : Requirement.None,
                requiresContext: true,
                state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kindAt in ((object)geometry).Kind(ctx: context).ToEff()
                    from length in kindAt.Length(value: geometry, ctx: context, op: op).ToEff()
                    from result in One(key: op, value: length).ToEff()
                    select result)),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    internal static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceProject<TGeometry, TPrimitive, TOut>(Conformance aspect, Requirement requirement) where TGeometry : notnull where TPrimitive : notnull =>
        (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, double>(
                count: item.Count, requirement: requirement, project: static (residuals, _) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Many(key: Rasm.Analysis.Conformance.Key, values: values)))),
            (Conformance.Rms item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, double>(
                count: item.Count, requirement: requirement, project: static (residuals, _) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key)).Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: stats.Rms)))),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, bool>(
                count: item.Count, requirement: requirement, project: static (residuals, context) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key)).Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: stats.Maximum <= context.Absolute.Value)))),
            (Conformance.ProfileResidual item, Type output) when output == typeof(ResidualProfile) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, ResidualProfile>(
                count: item.Count, requirement: requirement, project: static (residuals, context) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals)
                    .Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key))
                    .Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: new ResidualProfile(Count: stats.Count, Minimum: stats.Minimum, Maximum: stats.Maximum, Mean: stats.Mean, Variance: stats.Variance, Rms: stats.Rms, Tolerance: context.Absolute.Value, WithinTolerance: stats.Maximum <= context.Absolute.Value))))),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, ResidualSample>(
                count: item.Count, requirement: requirement, project: static (residuals, _) => residuals
                    .TraverseM(static sample => sample switch {
                        { Distance: double d, Location.IsValid: true } when d >= 0.0 && RhinoMath.IsValidDouble(x: d) => Fin.Succ(sample),
                        _ => Fin.Fail<ResidualSample>(Rasm.Analysis.Conformance.Key.InvalidResult()),
                    })
                    .As()
                    .Bind(values => values.Maxima(projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(Rasm.Analysis.Conformance.Key.InvalidResult()).Map(static best => Seq(best))))),
            _ => Rasm.Analysis.Conformance.Key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TValue>(int count, Requirement requirement, Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: Rasm.Analysis.Conformance.Key, requiresContext: true,
            state: (Op: Rasm.Analysis.Conformance.Key, Count: count, Requirement: requirement, Project: project),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from validated in runtime.Context.ValidatePair(a: pair.Geometry, b: pair.Primitive, requirementA: state.Requirement, requirementB: Requirement.None, cancel: runtime.Cancellation).ToEff()
                from kindG in ((object)validated.A).Kind(ctx: runtime.Context).ToEff()
                from kindP in ((object)validated.B).Kind(ctx: runtime.Context).ToEff()
                from residuals in kindG.Conformance(kindP: kindP, geometry: validated.A, primitive: validated.B, count: state.Count, ctx: runtime.Context, op: state.Op).ToEff()
                from result in state.Project(arg1: residuals, arg2: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<double>> ResidualDistances(Op key, Seq<ResidualSample> samples) =>
        samples.TraverseM(sample => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(distance),
            _ => Fin.Fail<double>(key.InvalidResult()),
        }).As();
}
