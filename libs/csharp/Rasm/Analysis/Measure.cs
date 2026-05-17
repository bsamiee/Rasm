using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Measure : IAspect {
    public sealed record LengthCase : Measure;
    public sealed record SpatialMidpointCase : Measure;
    public sealed record MassPropertyCase(MassKind Mass, MassProperty Property) : Measure;
    public static Measure Length => new LengthCase();
    public static Measure SpatialMidpoint => new SpatialMidpointCase();
    public static Measure Area => new MassPropertyCase(Mass: MassKind.Area, Property: MassProperty.Magnitude);
    public static Measure Volume => new MassPropertyCase(Mass: MassKind.Volume, Property: MassProperty.Magnitude);
    public static Measure Centroid(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Centroid);
    public static Measure MassError(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.MagnitudeError);
    public static Measure CentroidError(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.CentroidError);
    public static Measure Radii(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Radii);
    public static Measure PrincipalAxes(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.PrincipalAxes);
    public static Measure Inertia(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.Inertia);
    public static Measure InertiaProducts(MassKind mass) => new MassPropertyCase(Mass: mass, Property: MassProperty.InertiaProducts);
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        lengthCase: static _ => Analyze.Length<TGeometry, TOut>(),
        spatialMidpointCase: static _ => typeof(TOut) == typeof(Point3d) ? Analyze.SpatialMidpoint<TGeometry, TOut>() : Op.Of(name: "SpatialMidpoint").Unsupported<TGeometry, TOut>(),
        massPropertyCase: static p => Analyze.MassPropertyMeasure<TGeometry, TOut>(mass: p.Mass, property: p.Property));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MassProperty {
    public static readonly MassProperty Magnitude = new(key: 0, suffix: string.Empty, output: typeof(double), first: static _ => false, second: static _ => false, product: static _ => false, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.Length, area: static a => a.Area, volume: static v => v.Volume));
    public static readonly MassProperty MagnitudeError = new(key: 1, suffix: "Error", output: typeof(double), first: static _ => false, second: static m => m.Equals(MassKind.Length), product: static _ => false, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.LengthError, area: static a => a.AreaError, volume: static v => v.VolumeError));
    public static readonly MassProperty Centroid = new(key: 2, suffix: nameof(Centroid), output: typeof(Point3d), first: static _ => true, second: static m => m.Equals(MassKind.Length), product: static _ => false, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.Centroid, area: static a => a.Centroid, volume: static v => v.Centroid));
    public static readonly MassProperty CentroidError = new(key: 3, suffix: nameof(CentroidError), output: typeof(Vector3d), first: static _ => true, second: static m => m.Equals(MassKind.Length), product: static _ => false, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.CentroidError, area: static a => a.CentroidError, volume: static v => v.CentroidError));
    public static readonly MassProperty Radii = new(key: 4, suffix: nameof(Radii), output: typeof(Vector3d), first: static _ => true, second: static _ => true, product: static _ => false, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.CentroidCoordinatesRadiiOfGyration, area: static a => a.CentroidCoordinatesRadiiOfGyration, volume: static v => v.CentroidCoordinatesRadiiOfGyration));
    public static readonly MassProperty PrincipalAxes = new(key: 5, suffix: "Principal", output: typeof(ValueTuple<double, Vector3d>), first: static _ => true, second: static _ => true, product: static _ => true, extract: static (k, p) => k.PrincipalAxesOf(mass: p).Map(static axes => axes.Map(static axis => (object)axis)));
    public static readonly MassProperty Inertia = new(key: 6, suffix: nameof(Inertia), output: typeof(Vector3d), first: static _ => true, second: static _ => true, product: static _ => true, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.WorldCoordinatesMomentsOfInertia, area: static a => a.WorldCoordinatesMomentsOfInertia, volume: static v => v.WorldCoordinatesMomentsOfInertia));
    public static readonly MassProperty InertiaProducts = new(key: 7, suffix: "Products", output: typeof(Vector3d), first: static _ => true, second: static _ => true, product: static _ => true, extract: static (k, p) => k.MassPropertyExtract(props: p, length: static l => l.WorldCoordinatesProductMoments, area: static a => a.WorldCoordinatesProductMoments, volume: static v => v.WorldCoordinatesProductMoments));
    private readonly Func<MassKind, bool> first;
    private readonly Func<MassKind, bool> second;
    private readonly Func<MassKind, bool> product;
    private readonly Func<Op, IDisposable, Fin<Seq<object>>> extract;
    public string Suffix { get; }
    public Type Output { get; }
    internal bool FirstMoments(MassKind mass) => first(arg: mass);
    internal bool SecondMoments(MassKind mass) => second(arg: mass);
    internal bool ProductMoments(MassKind mass) => product(arg: mass);
    internal Fin<Seq<TValue>> Extract<TValue>(Op key, IDisposable mass) =>
        typeof(TValue) == Output
            ? extract(arg1: key, arg2: mass).Bind(values => values.TraverseM(value => value is TValue typed ? key.AcceptValue(value: typed) : Fin.Fail<TValue>(key.Unsupported(geometryType: value.GetType(), outputType: typeof(TValue)))).As())
            : Fin.Fail<Seq<TValue>>(key.Unsupported(geometryType: typeof(IDisposable), outputType: typeof(TValue)));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MassKind {
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None, compute: static (_, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))), aggregate: static (_, _, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(nameof(None))));
    public static readonly MassKind Length = new(key: 1, label: nameof(Length), requirement: Requirement.CurveLength, compute: LengthOf, aggregate: LengthAggregate);
    public static readonly MassKind Area = new(key: 2, label: nameof(Area), requirement: Requirement.AreaMass, compute: AreaOf, aggregate: static (self, geom, ctx, firstMoments, secondMoments, productMoments, op) => SumAggregate<AreaMassProperties>(geom, ctx, self, firstMoments, secondMoments, productMoments, op, static (t, s) => t.Sum(s, true)));
    public static readonly MassKind Volume = new(key: 3, label: nameof(Volume), requirement: Requirement.VolumeMass, compute: VolumeOf, aggregate: static (self, geom, ctx, firstMoments, secondMoments, productMoments, op) => SumAggregate<VolumeMassProperties>(geom, ctx, self, firstMoments, secondMoments, productMoments, op, static (t, s) => t.Sum(s, true)));
    private readonly Func<object, Context, bool, bool, bool, Op, Fin<IDisposable>> compute;
    private readonly Func<MassKind, IEnumerable<object>, Context, bool, bool, bool, Op, Fin<IDisposable>> aggregate;
    public string Label { get; }
    internal Requirement Requirement { get; }
    internal Fin<IDisposable> Compute(object geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => compute(geometry, context, firstMoments, secondMoments, productMoments, op);
    internal Fin<IDisposable> Aggregate(IEnumerable<object> geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => aggregate(this, geometry, context, firstMoments, secondMoments, productMoments, op);
    public Eff<Env, IDisposable> Compute(object? geometry, Op op, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) => Optional(geometry).ToFin(op.InvalidInput()).ToEff().Bind(g => Env.Asks.Bind(context => Compute(g, context, firstMoments, secondMoments, productMoments, op).ToEff()));
    internal static MassKind KindOf(GeometryBase geometry) => geometry switch {
        Curve => Length,
        Brep brep => brep.IsSolid ? Volume : Area,
        Mesh mesh => mesh.IsSolid ? Volume : Area,
        Extrusion extrusion => extrusion.IsSolid ? Volume : Area,
        Surface surface => surface.IsSolid ? Volume : Area,
        _ => None,
    };
    internal static Fin<Plane> PrincipalFrameOf(GeometryBase geometry, Context context, Op key) =>
        KindOf(geometry: geometry) switch {
            MassKind kind when kind.Equals(None) => Fin.Fail<Plane>(key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Plane))),
            MassKind kind => kind.Compute(geometry: geometry, context: context, firstMoments: true, secondMoments: true, productMoments: true, op: key)
                .Bind(disposable => new Lease<IDisposable>.Owned(Value: disposable).Use(owned => PrincipalFrameOf(mass: owned, key: key))),
        };
    internal static Fin<Plane> PrincipalFrameOf(IDisposable mass, Op key) =>
        (mass switch {
            LengthMassProperties l => Some(l.Centroid),
            AreaMassProperties a => Some(a.Centroid),
            VolumeMassProperties v => Some(v.Centroid),
            _ => Option<Point3d>.None,
        }).ToFin(key.InvalidResult()).Bind(centroid =>
            key.PrincipalAxesOf(mass: mass).Bind(axes => (axes.Count, centroid.IsValid) switch {
                ( >= 2, true) => new Plane(origin: centroid, xDirection: axes[0].Axis, yDirection: axes[1].Axis) switch {
                    { IsValid: true } plane => Fin.Succ(plane),
                    _ => Fin.Fail<Plane>(key.InvalidResult()),
                },
                _ => Fin.Fail<Plane>(key.InvalidResult()),
            }));
    private static Fin<IDisposable> Done<TMass>(TMass? mass) where TMass : class, IDisposable => Optional(mass).ToFin(new Fault.ComputationFailed(typeof(TMass).Name)).Map(static p => (IDisposable)p);
    private static Fin<IDisposable> LengthOf(object geometry, Context _, bool firstMoments, bool secondMoments, bool productMoments, Op op) =>
        GeometryKernel.CurveForm(source: geometry, op: op)
            .Bind(lease => lease.Use(curve => Done(LengthMassProperties.Compute(curve, length: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments))));
    private static Fin<IDisposable> AreaOf(object geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => geometry switch {
        Mesh mesh => Done(AreaMassProperties.Compute(mesh, area: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)),
        Curve curve => Done(AreaMassProperties.Compute(curve, context.Absolute.Value)),
        object curveLike when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => GeometryKernel.CurveForm(source: curveLike, op: op).Bind(lease => lease.Use(curve => AreaOf(geometry: curve, context: context, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op))),
        Brep brep => Done(AreaMassProperties.Compute(brep, area: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value)),
        Surface surface => Done(AreaMassProperties.Compute(surface, area: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)),
        GeometryBase { HasBrepForm: true } or Box or BoundingBox or Sphere or Cylinder or Cone or Torus => GeometryKernel.BrepForm(source: geometry, op: op).Bind(lease => lease.Use(brep => AreaOf(geometry: brep, context: context, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op))),
        _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(AreaMassProperties))),
    };
    private static Fin<IDisposable> VolumeOf(object geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) => geometry switch {
        Mesh mesh => Done(VolumeMassProperties.Compute(mesh, volume: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)),
        Brep brep => Done(VolumeMassProperties.Compute(brep, volume: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value)),
        Surface surface => Done(VolumeMassProperties.Compute(surface, volume: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)),
        GeometryBase { HasBrepForm: true } or Box or BoundingBox or Sphere or Cylinder or Cone or Torus => GeometryKernel.BrepForm(source: geometry, op: op).Bind(lease => lease.Use(brep => VolumeOf(geometry: brep, context: context, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op))),
        _ => Fin.Fail<IDisposable>(op.Unsupported(geometry.GetType(), typeof(VolumeMassProperties))),
    };
    private static Fin<IDisposable> LengthAggregate(MassKind self, IEnumerable<object> geometry, Context context, bool firstMoments, bool secondMoments, bool productMoments, Op op) =>
        toSeq(geometry) switch {
            Seq<object> items when items.ForAll(static item => item is Curve) => Done(LengthMassProperties.Compute(curves: items.AsIterable().Cast<Curve>(), length: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments)),
            Seq<object> items => SumAggregate<LengthMassProperties>(items.AsIterable(), context, self, firstMoments, secondMoments, productMoments, op, static (t, s) => t.Sum(s, true)),
        };
    private static Fin<IDisposable> SumAggregate<TMass>(IEnumerable<object> geometry, Context context, MassKind mass, bool firstMoments, bool secondMoments, bool productMoments, Op op, Func<TMass, IEnumerable<TMass>, bool> sum) where TMass : class, IDisposable =>
        toSeq(geometry).Fold(Fin.Succ(Seq<IDisposable>()), (state, item) => state.Bind(owned =>
            mass.compute(item, context, firstMoments, secondMoments, productMoments, op)
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
    public static Operation<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull => Aspect<Measure, TGeometry, TOut>(aspect: aspect);
    internal static Operation<TGeometry, TOut> SpatialMidpoint<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut), typeof(TGeometry)) switch {
            (Type output, Type geometry) when output == typeof(Point3d)
                && (geometry == typeof(object) || geometry == typeof(GeometryBase) || geometry == typeof(Point3d) || geometry == typeof(Point) || geometry == typeof(BoundingBox) || geometry == typeof(Box) || GeometryKernel.CanCurveForm(type: geometry) || typeof(Brep).IsAssignableFrom(geometry) || typeof(Mesh).IsAssignableFrom(geometry) || GeometryKernel.CanSurfaceForm(type: geometry) || typeof(SubD).IsAssignableFrom(geometry)) =>
                Operation<TGeometry, Point3d>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) => from context in Env.Asks
                                                    from centroid in CentroidOf(geometry: geometry, context: context, op: op).ToEff()
                                                    from result in op.Accept(value: centroid).ToEff()
                                                    select result).As<TGeometry, TOut>(key: key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    internal static Operation<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        Option<Requirement> requirement = (typeof(TOut) == typeof(double), typeof(TGeometry), Rasm.Domain.Kind.Of(typeof(TGeometry)).Case) switch {
            (true, Type geometry, _) when geometry == typeof(object) || geometry == typeof(GeometryBase) => Some(Requirement.CurveLength),
            (true, _, Kind kind) when kind.Topology == Topology.Curve => Some(Requirement.CurveLength),
            _ => Option<Requirement>.None,
        };
        return requirement.Match(
            Some: active => Operation<TGeometry, double>.Build(
                key: key,
                requirement: active,
                requiresContext: true,
                state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from length in LengthOf(geometry: geometry, context: context, op: op).ToEff()
                    from result in op.Accept(value: length).ToEff()
                    select result).As<TGeometry, TOut>(key: key),
            None: () => key.Unsupported<TGeometry, TOut>());
    }
    internal static Operation<TGeometry, TOut> MassPropertyMeasure<TGeometry, TOut>(MassKind mass, MassProperty property) where TGeometry : notnull {
        Op key = Op.Of(name: $"{mass.Label}{property.Suffix}");
        return (mass.Equals(MassKind.None), typeof(TOut) == property.Output) switch {
            (true, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (false, true) => MassOperation<TGeometry, TOut>(mass: mass, property: property),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static Operation<TGeometry, TOut> MassOperation<TGeometry, TOut>(MassKind mass, MassProperty property) where TGeometry : notnull {
        Op key = Op.Of(name: $"{mass.Label}{property.Suffix}");
        return Operation<TGeometry, TOut>.Build(
            key: key, requirement: mass.Requirement, requiresContext: true,
            aggregate: Some<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>>(
                geometry => from context in Env.Asks
                            from aggregate in mass.Aggregate(geometry: geometry.Map(static item => (object)item).AsIterable(), context: context, firstMoments: property.FirstMoments(mass: mass), secondMoments: property.SecondMoments(mass: mass), productMoments: property.ProductMoments(mass: mass), op: key).ToEff()
                            from values in new Lease<IDisposable>.Owned(Value: aggregate).Use(disposable => property.Extract<TOut>(key: key, mass: disposable)).ToEff()
                            select values),
            evaluator: geometry => from computed in mass.Compute(geometry: geometry, op: key, firstMoments: property.FirstMoments(mass: mass), secondMoments: property.SecondMoments(mass: mass), productMoments: property.ProductMoments(mass: mass))
                                   from values in new Lease<IDisposable>.Owned(Value: computed).Use(disposable => property.Extract<TOut>(key: key, mass: disposable)).ToEff()
                                   select values).As<TGeometry, TOut>(key: key);
    }
    internal static Fin<Seq<object>> MassPropertyExtract<TProp>(this Op key, IDisposable props, Func<LengthMassProperties, TProp> length, Func<AreaMassProperties, TProp> area, Func<VolumeMassProperties, TProp> volume) =>
        props switch {
            LengthMassProperties l => key.Accept(value: length(arg: l)).Map(static values => values.Map(static value => (object)value!)),
            AreaMassProperties a => key.Accept(value: area(arg: a)).Map(static values => values.Map(static value => (object)value!)),
            VolumeMassProperties v => key.Accept(value: volume(arg: v)).Map(static values => values.Map(static value => (object)value!)),
            _ => Fin.Fail<Seq<object>>(key.InvalidResult()),
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
            BrepFace face => MassCentroidOf(geometry: face, isSolid: false, context: context, op: op),
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
