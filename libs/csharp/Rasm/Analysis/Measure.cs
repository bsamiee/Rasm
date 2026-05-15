namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Measure : IAspect {
    public sealed record Length : Measure; public sealed record Area : Measure; public sealed record Volume : Measure; public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure; public sealed record MassError(MassKind Mass) : Measure; public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure; public sealed record PrincipalAxes(MassKind Mass) : Measure;
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        spatialMidpoint: static _ => typeof(TOut) == typeof(Point3d) ? Analyze.SpatialMidpoint<TGeometry, TOut>() : Op.Of(name: "SpatialMidpoint").Unsupported<TGeometry, TOut>(),
        length: static _ => Analyze.Length<TGeometry, TOut>(),
        area: static a => Analyze.MassMeasure<TGeometry, TOut>(mass: MassKind.Area, aspect: a),
        volume: static v => Analyze.MassMeasure<TGeometry, TOut>(mass: MassKind.Volume, aspect: v),
        massError: static e => Analyze.MassMeasure<TGeometry, TOut>(mass: e.Mass, aspect: e),
        centroid: static c => Analyze.MassMeasure<TGeometry, TOut>(mass: c.Mass, aspect: c),
        centroidError: static ce => Analyze.MassMeasure<TGeometry, TOut>(mass: ce.Mass, aspect: ce),
        radii: static r => Analyze.MassMeasure<TGeometry, TOut>(mass: r.Mass, aspect: r),
        principalAxes: static p => Analyze.MassMeasure<TGeometry, TOut>(mass: p.Mass, aspect: p));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull => Aspect<Measure, TGeometry, TOut>(aspect: aspect);
    public static Query<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(Point3d) && Dispatch.SupportsBounds(typeof(TGeometry), includeSphere: false))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Env.Asks
                                                    from centroid in Dispatch.Resolve<Point3d, (Context, Op)>(CapTag.Centroid, geometry, (context, op), op).ToEff()
                                                    from result in op.One(value: centroid).ToEff()
                                                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut) == typeof(double), KindLookup.For(typeof(TGeometry)).Case) switch {
            (true, Kind { Topology: Topology.Curve } kind) => Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, double>.Build(
                key: key,
                requirement: typeof(GeometryBase).IsAssignableFrom(kind.Type) ? Requirement.CurveLength : Requirement.None,
                requiresContext: true,
                state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from length in Dispatch.Resolve<double, (Context, Op)>(CapTag.Length, geometry, (context, op), op).ToEff()
                    from result in op.One(value: length).ToEff()
                    select result)),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    internal static Query<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return (aspect, typeof(TOut)) switch {
            (_, _) when mass.Equals(MassKind.None) => Query<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (Analysis.Measure.Area, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Area, suffix: string.Empty, project: static (k, props) => props switch {
                AreaMassProperties area => k.One(value: area.Area),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.Volume, Type output) when output == typeof(double) => MassCast<TGeometry, TOut, double>(mass: MassKind.Volume, suffix: string.Empty, project: static (k, props) => props switch {
                VolumeMassProperties volume => k.One(value: volume.Volume),
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
                            from aggregate in mass.Aggregate(geometry: native.AsIterable(), context: context, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: key).ToEff()
                            from values in Bracket(factory: () => aggregate, body: disposable => project(arg1: key, arg2: disposable)).ToEff()
                            select values),
            evaluator: geometry => from computed in mass.Compute(geometry: geometry, op: key, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)
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
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal<TMass>(this Op key, TMass mass) where TMass : class =>
        mass switch {
            LengthMassProperties length => PrincipalFromMoments(key: key,
                solved: length.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => PrincipalFromMoments(key: key,
                solved: area.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => PrincipalFromMoments(key: key,
                solved: volume.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalFromMoments(Op key, bool solved, double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
}
