using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Measure : IAspect {
    public sealed record LengthCase : Measure; public sealed record AreaCase : Measure; public sealed record VolumeCase : Measure; public sealed record SpatialMidpointCase : Measure;
    public sealed record CentroidCase(MassKind Mass) : Measure; public sealed record MassErrorCase(MassKind Mass) : Measure; public sealed record CentroidErrorCase(MassKind Mass) : Measure;
    public sealed record RadiiCase(MassKind Mass) : Measure; public sealed record PrincipalAxesCase(MassKind Mass) : Measure;
    public static Measure Length => new LengthCase();
    public static Measure Area => new AreaCase();
    public static Measure Volume => new VolumeCase();
    public static Measure SpatialMidpoint => new SpatialMidpointCase();
    public static Measure Centroid(MassKind mass) => new CentroidCase(Mass: mass);
    public static Measure MassError(MassKind mass) => new MassErrorCase(Mass: mass);
    public static Measure CentroidError(MassKind mass) => new CentroidErrorCase(Mass: mass);
    public static Measure Radii(MassKind mass) => new RadiiCase(Mass: mass);
    public static Measure PrincipalAxes(MassKind mass) => new PrincipalAxesCase(Mass: mass);
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        spatialMidpointCase: static _ => typeof(TOut) == typeof(Point3d) ? Analyze.SpatialMidpoint<TGeometry, TOut>() : Op.Of(name: "SpatialMidpoint").Unsupported<TGeometry, TOut>(),
        lengthCase: static _ => Analyze.Length<TGeometry, TOut>(),
        areaCase: static a => Analyze.MassMeasure<TGeometry, TOut>(mass: MassKind.Area, aspect: a),
        volumeCase: static v => Analyze.MassMeasure<TGeometry, TOut>(mass: MassKind.Volume, aspect: v),
        massErrorCase: static e => Analyze.MassMeasure<TGeometry, TOut>(mass: e.Mass, aspect: e),
        centroidCase: static c => Analyze.MassMeasure<TGeometry, TOut>(mass: c.Mass, aspect: c),
        centroidErrorCase: static ce => Analyze.MassMeasure<TGeometry, TOut>(mass: ce.Mass, aspect: ce),
        radiiCase: static r => Analyze.MassMeasure<TGeometry, TOut>(mass: r.Mass, aspect: r),
        principalAxesCase: static p => Analyze.MassMeasure<TGeometry, TOut>(mass: p.Mass, aspect: p));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MassKind {
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None, compute: static (_, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))), aggregate: static (_, _, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))));
    public static readonly MassKind Length = new(key: 1, label: nameof(Length), requirement: Requirement.CurveLength, compute: LengthOf, aggregate: LengthAggregate);
    public static readonly MassKind Area = new(key: 2, label: nameof(Area), requirement: Requirement.AreaMass, compute: AreaOf, aggregate: static (self, geom, ctx, fm, sm, pm, op) => SumAggregate<AreaMassProperties>(geom, ctx, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)));
    public static readonly MassKind Volume = new(key: 3, label: nameof(Volume), requirement: Requirement.VolumeMass, compute: VolumeOf, aggregate: static (self, geom, ctx, fm, sm, pm, op) => SumAggregate<VolumeMassProperties>(geom, ctx, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)));
    private readonly Func<object, Context, bool, bool, bool, Op, Fin<IDisposable>> compute;
    private readonly Func<MassKind, IEnumerable<GeometryBase>, Context, bool, bool, bool, Op, Fin<IDisposable>> aggregate;
    public string Label { get; }
    internal Requirement Requirement { get; }
    internal Fin<IDisposable> Compute(object geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => compute(geometry, context, firstMoments, secondMoments, productMoments, op);
    internal Fin<IDisposable> Aggregate(IEnumerable<GeometryBase> geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => aggregate(this, geometry, context, firstMoments, secondMoments, productMoments, op);
    public Eff<Env, IDisposable> Compute(object? geometry, Op op, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) => Optional(geometry).ToFin(op.InvalidInput()).ToEff().Bind(g => Env.Asks.Bind(context => Compute(g, context, firstMoments, secondMoments, productMoments, op).ToEff()));
    private static Fin<IDisposable> Done<TMass>(TMass? mass) where TMass : class, IDisposable => Optional(mass).ToFin(new Fault.ComputationFailed(typeof(TMass).Name)).Map(static p => (IDisposable)p);
    private static Fin<IDisposable> LengthOf(object geometry, Context _, bool fm, bool sm, bool pm, Op op) =>
        GeometryKernel.CurveForm(source: geometry, op: op)
            .Bind(lease => lease.Use(curve => Done(LengthMassProperties.Compute(curve, length: true, firstMoments: fm, secondMoments: sm, productMoments: pm))));
    private static Fin<IDisposable> AreaOf(object geometry, Context context, bool fm, bool sm, bool pm, Op op) => geometry switch {
        Curve curve => Done(AreaMassProperties.Compute(curve, context.Absolute.Value)),
        Mesh mesh => Done(AreaMassProperties.Compute(mesh, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm)),
        Brep brep => Done(AreaMassProperties.Compute(brep, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value)),
        Surface surface => Done(AreaMassProperties.Compute(surface, area: true, firstMoments: fm, secondMoments: sm, productMoments: pm)),
        Box or BoundingBox or Sphere or Cylinder or Cone or Torus => GeometryKernel.BrepForm(source: geometry, op: op).Bind(lease => lease.Use(brep => AreaOf(geometry: brep, context: context, fm: fm, sm: sm, pm: pm, op: op))),
        _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(AreaMassProperties))),
    };
    private static Fin<IDisposable> VolumeOf(object geometry, Context context, bool fm, bool sm, bool pm, Op op) => geometry switch {
        Mesh mesh => Done(VolumeMassProperties.Compute(mesh, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm)),
        Brep brep => Done(VolumeMassProperties.Compute(brep, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value)),
        Surface surface => Done(VolumeMassProperties.Compute(surface, volume: true, firstMoments: fm, secondMoments: sm, productMoments: pm)),
        Box or BoundingBox or Sphere or Cylinder or Cone or Torus => GeometryKernel.BrepForm(source: geometry, op: op).Bind(lease => lease.Use(brep => VolumeOf(geometry: brep, context: context, fm: fm, sm: sm, pm: pm, op: op))),
        _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(VolumeMassProperties))),
    };
    private static Fin<IDisposable> LengthAggregate(MassKind self, IEnumerable<GeometryBase> geometry, Context context, bool fm, bool sm, bool pm, Op op) => toSeq(geometry) switch { Seq<GeometryBase> items when items.ForAll(static i => i is Curve) => Done(LengthMassProperties.Compute(curves: items.AsIterable().Cast<Curve>(), length: true, firstMoments: fm, secondMoments: sm, productMoments: pm)), Seq<GeometryBase> items => SumAggregate<LengthMassProperties>(items.AsIterable(), context, self, fm, sm, pm, op, static (t, s) => t.Sum(s, true)) };
    private static Fin<IDisposable> SumAggregate<TMass>(IEnumerable<GeometryBase> geometry, Context context, MassKind mass, bool fm, bool sm, bool pm, Op op, Func<TMass, IEnumerable<TMass>, bool> sum) where TMass : class, IDisposable =>
        toSeq(geometry).Fold(Fin.Succ(Seq<IDisposable>()), (state, item) => state.Bind(owned =>
            mass.compute(item, context, fm, sm, pm, op)
                .Match(Succ: r => Fin.Succ(r.Cons(owned)), Fail: e => (owned.Iter(static r => r.Dispose()), Fin.Fail<Seq<IDisposable>>(e)).Item2)))
            .Bind(owned => {
                TMass[] masses = [.. owned.AsIterable().Cast<TMass>()];
                Fin<IDisposable> result = masses.Length switch {
                    1 => Fin.Succ<IDisposable>(masses[0]),
                    > 1 when sum(masses[0], Enumerable.Skip(masses, 1)) => Fin.Succ<IDisposable>(masses[0]),
                    _ => Fin.Fail<IDisposable>(new Fault.ComputationFailed(typeof(TMass).Name)),
                };
                _ = toSeq(Enumerable.Skip(masses, result.IsSucc ? 1 : 0)).Iter(static r => r.Dispose());
                return result;
            });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull => Aspect<Measure, TGeometry, TOut>(aspect: aspect);
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut), typeof(TGeometry)) switch {
            (Type output, Type geometry) when output == typeof(Point3d)
                && (geometry == typeof(object) || geometry == typeof(GeometryBase) || geometry == typeof(Point3d) || geometry == typeof(Point) || geometry == typeof(Line) || geometry == typeof(Polyline) || geometry == typeof(BoundingBox) || geometry == typeof(Box) || typeof(Curve).IsAssignableFrom(geometry) || typeof(Brep).IsAssignableFrom(geometry) || typeof(Mesh).IsAssignableFrom(geometry) || typeof(Surface).IsAssignableFrom(geometry) || typeof(SubD).IsAssignableFrom(geometry)) =>
                Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Env.Asks
                                                    from centroid in CentroidOf(geometry: geometry, context: context, op: op).ToEff()
                                                    from result in op.Accept(value: centroid).ToEff()
                                                    select result)),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        Option<Requirement> requirement = (typeof(TOut) == typeof(double), typeof(TGeometry), KindLookup.Resolve(typeof(TGeometry)).Case) switch {
            (true, Type geometry, _) when geometry == typeof(object) || geometry == typeof(GeometryBase) => Some(Requirement.CurveLength),
            (true, _, Kind { Topology: Topology.Curve } kind) => Some(typeof(GeometryBase).IsAssignableFrom(kind.Type) ? Requirement.CurveLength : Requirement.None),
            _ => Option<Requirement>.None,
        };
        return requirement.Match(
            Some: active => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, double>.Build(
                key: key,
                requirement: active,
                requiresContext: true,
                state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from length in LengthOf(geometry: geometry, context: context, op: op).ToEff()
                    from result in op.Accept(value: length).ToEff()
                    select result)),
            None: () => key.Unsupported<TGeometry, TOut>());
    }
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> MassMeasure<TGeometry, TOut>(MassKind mass, Measure aspect) where TGeometry : notnull {
        Op key = Op.Of();
        return (aspect, typeof(TOut)) switch {
            (_, _) when mass.Equals(MassKind.None) => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (Analysis.Measure.AreaCase, Type output) when output == typeof(double) => MassOperation<TGeometry, TOut, double>(mass: MassKind.Area, suffix: string.Empty, project: static (k, props) => props switch {
                AreaMassProperties area => k.Accept(value: area.Area),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.VolumeCase, Type output) when output == typeof(double) => MassOperation<TGeometry, TOut, double>(mass: MassKind.Volume, suffix: string.Empty, project: static (k, props) => props switch {
                VolumeMassProperties volume => k.Accept(value: volume.Volume),
                _ => Fin.Fail<Seq<double>>(k.InvalidResult()),
            }),
            (Analysis.Measure.MassErrorCase, Type output) when output == typeof(double) => MassOperation<TGeometry, TOut, double>(mass: mass, suffix: "Error", project: (k, props) => MassValue<double>(aspect: aspect, key: k, props: props), secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.CentroidCase, Type output) when output == typeof(Point3d) => MassOperation<TGeometry, TOut, Point3d>(mass: mass, suffix: "Centroid", project: (k, props) => MassValue<Point3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.CentroidErrorCase, Type output) when output == typeof(Vector3d) => MassOperation<TGeometry, TOut, Vector3d>(mass: mass, suffix: "CentroidError", project: (k, props) => MassValue<Vector3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: mass.Equals(MassKind.Length)),
            (Analysis.Measure.RadiiCase, Type output) when output == typeof(Vector3d) => MassOperation<TGeometry, TOut, Vector3d>(mass: mass, suffix: "Radii", project: (k, props) => MassValue<Vector3d>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: true),
            (Analysis.Measure.PrincipalAxesCase, Type output) when output == typeof(ValueTuple<double, Vector3d>) => MassOperation<TGeometry, TOut, (double Moment, Vector3d Axis)>(mass: mass, suffix: "Principal", project: (k, props) => MassValue<(double Moment, Vector3d Axis)>(aspect: aspect, key: k, props: props), firstMoments: true, secondMoments: true, productMoments: true),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static global::Rasm.Analysis.Operation<TGeometry, TOut> MassOperation<TGeometry, TOut, TValue>(MassKind mass, string suffix, Func<Op, IDisposable, Fin<Seq<TValue>>> project, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) where TGeometry : notnull {
        Op key = Op.Of(name: $"{mass.Label}{suffix}");
        return Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, TValue>.Build(
            key: key, requirement: mass.Requirement, requiresContext: true,
            aggregate: Some<Func<Seq<TGeometry>, Eff<Env, Seq<TValue>>>>(
                geometry => from context in Env.Asks
                            from native in geometry.Traverse(item => item switch {
                                GeometryBase gb => Fin.Succ(gb),
                                _ => Fin.Fail<GeometryBase>(key.Unsupported(geometryType: item.GetType(), outputType: typeof(GeometryBase))),
                            }).As().ToEff()
                            from aggregate in mass.Aggregate(geometry: native.AsIterable(), context: context, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: key).ToEff()
                            from values in new Lease<IDisposable>.Owned(Value: aggregate).Use(disposable => project(arg1: key, arg2: disposable)).ToEff()
                            select values),
            evaluator: geometry => from computed in mass.Compute(geometry: geometry, op: key, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)
                                   from values in new Lease<IDisposable>.Owned(Value: computed).Use(disposable => project(arg1: key, arg2: disposable)).ToEff()
                                   select values));
    }
    private static Fin<Seq<TValue>> MassValue<TValue>(Measure aspect, Op key, IDisposable props) =>
        aspect switch {
            Analysis.Measure.MassErrorCase => key.MassProperty<double, TValue>(props: props, length: static l => l.LengthError, area: static a => a.AreaError, volume: static v => v.VolumeError),
            Analysis.Measure.CentroidCase => key.MassProperty<Point3d, TValue>(props: props, length: static l => l.Centroid, area: static a => a.Centroid, volume: static v => v.Centroid),
            Analysis.Measure.CentroidErrorCase => key.MassProperty<Vector3d, TValue>(props: props, length: static l => l.CentroidError, area: static a => a.CentroidError, volume: static v => v.CentroidError),
            Analysis.Measure.RadiiCase => key.MassProperty<Vector3d, TValue>(props: props, length: static l => l.CentroidCoordinatesRadiiOfGyration, area: static a => a.CentroidCoordinatesRadiiOfGyration, volume: static v => v.CentroidCoordinatesRadiiOfGyration),
            Analysis.Measure.PrincipalAxesCase => key.PrincipalAxesOf(mass: props).Bind(values => key.AcceptResults<(double Moment, Vector3d Axis), TValue>(values: values)),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    private static Fin<Seq<TValue>> MassProperty<TProp, TValue>(this Op key, IDisposable props, Func<LengthMassProperties, TProp> length, Func<AreaMassProperties, TProp> area, Func<VolumeMassProperties, TProp> volume) =>
        props switch {
            LengthMassProperties l => key.AcceptResults<TProp, TValue>(values: Seq(length(arg: l))),
            AreaMassProperties a => key.AcceptResults<TProp, TValue>(values: Seq(area(arg: a))),
            VolumeMassProperties v => key.AcceptResults<TProp, TValue>(values: Seq(volume(arg: v))),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalAxesOf<TMass>(this Op key, TMass mass) where TMass : class =>
        mass switch {
            LengthMassProperties length => PrincipalAxesFromMoments(key: key,
                solved: length.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => PrincipalAxesFromMoments(key: key,
                solved: area.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => PrincipalAxesFromMoments(key: key,
                solved: volume.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalAxesFromMoments(Op key, bool solved, double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    internal static Fin<double> LengthOf<TGeometry>(TGeometry geometry, Context context, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Line line => Fin.Succ(line.Length),
            Polyline polyline => Fin.Succ(polyline.Length),
            Circle circle => Fin.Succ(circle.Circumference),
            Arc arc => Fin.Succ(arc.Length),
            Ellipse ellipse => Optional(ellipse.ToNurbsCurve()).ToFin(op.InvalidResult()).Bind(curve => new Lease<Curve>.Owned(Value: curve).Use(native => LengthOf(geometry: native, context: context, op: op))),
            Curve curve => curve.GetLength(context.Fractional) switch { double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => Fin.Succ(length), _ => Fin.Fail<double>(op.InvalidResult()) },
            _ => Fin.Fail<double>(op.Unsupported(g.GetType(), typeof(double))),
        });
    internal static Fin<Point3d> CentroidOf<TGeometry>(TGeometry geometry, Context context, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Point3d point => Fin.Succ(point),
            Point point => Fin.Succ(point.Location),
            Line line => Fin.Succ(line.PointAt(t: 0.5)),
            Polyline polyline => Fin.Succ(polyline.CenterPoint()),
            BoundingBox box => Fin.Succ(box.Center),
            Box box => Fin.Succ(box.Center),
            Brep brep => MassCentroidOf(geometry: brep, isSolid: brep.IsSolid, context: context, op: op),
            Mesh mesh => MassCentroidOf(geometry: mesh, isSolid: mesh.IsSolid, context: context, op: op),
            Surface surface => MassCentroidOf(geometry: surface, isSolid: surface.IsSolid, context: context, op: op),
            Curve curve => (curve.IsClosed, curve.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value)) switch {
                (false, _) => Optional(LengthMassProperties.Compute(curve)).ToFin(op.InvalidResult()).Map(static m => new Lease<LengthMassProperties>.Owned(Value: m).Use(static d => d.Centroid)),
                (true, true) => Optional(AreaMassProperties.Compute(curve, context.Absolute.Value)).ToFin(op.InvalidResult()).Map(static m => new Lease<AreaMassProperties>.Owned(Value: m).Use(static d => d.Centroid)),
                _ => Fin.Fail<Point3d>(op.InvalidResult()),
            },
            SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Bind(brep => new Lease<Brep>.Owned(Value: brep).Use(d => MassCentroidOf(geometry: d, isSolid: d.IsSolid, context: context, op: op))),
            _ => Fin.Fail<Point3d>(op.Unsupported(g.GetType(), typeof(Point3d))),
        });
    private static Fin<Point3d> MassCentroidOf(object geometry, bool isSolid, Context context, Op op) =>
        (isSolid ? MassKind.Volume : MassKind.Area).Compute(geometry, context, true, false, false, op)
            .Bind(d => new Lease<IDisposable>.Owned(Value: d).Use(owned => owned switch { LengthMassProperties l => Fin.Succ(l.Centroid), AreaMassProperties a => Fin.Succ(a.Centroid), VolumeMassProperties v => Fin.Succ(v.Centroid), _ => Fin.Fail<Point3d>(op.InvalidResult()) }));
}
