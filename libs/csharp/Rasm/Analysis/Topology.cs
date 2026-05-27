using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record Topologies : IAspect {
    public sealed record KindCase : Topologies;
    public sealed record DomainsCase : Topologies;
    public sealed record SolidOrientationCase : Topologies;
    public sealed record ComponentsCase : Topologies;
    public sealed record ContainsPointCase(Point3d Point) : Topologies;
    public sealed record ScalarCase(TopologyScalar Scalar) : Topologies;
    public static Topologies Kind => new KindCase();
    public static Topologies Domains => new DomainsCase();
    public static Topologies SolidOrientation => new SolidOrientationCase();
    public static Topologies Components => new ComponentsCase();
    public static Topologies ContainsPoint(Point3d point) => new ContainsPointCase(Point: point);
    public static Topologies Manifold => new ScalarCase(Scalar: TopologyScalar.Manifold);
    public static Topologies Euler => new ScalarCase(Scalar: TopologyScalar.Euler);
    public static Topologies BoundaryLoops => new ScalarCase(Scalar: TopologyScalar.BoundaryLoops);
    public static Topologies Genus => new ScalarCase(Scalar: TopologyScalar.Genus);
    public static Topologies HoleCount => new ScalarCase(Scalar: TopologyScalar.HoleCount);
    public static Topologies FaceCount => new ScalarCase(Scalar: TopologyScalar.FaceCount);
    public static Topologies EdgeCount => new ScalarCase(Scalar: TopologyScalar.EdgeCount);
    public static Topologies VertexCount => new ScalarCase(Scalar: TopologyScalar.VertexCount);
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        kindCase: static _ => Analyze.Kind<TGeometry, TOut>(),
        domainsCase: static _ => Analyze.TopologyDomains<TGeometry, TOut>(),
        solidOrientationCase: static _ => Analyze.TopologySolidOrientation<TGeometry, TOut>(),
        componentsCase: static _ => Analyze.TopologyComponents<TGeometry, TOut>(),
        containsPointCase: static cp => Analyze.TopologyContains<TGeometry, TOut>(point: cp.Point),
        scalarCase: static scalar => Analyze.TopologyScalar<TGeometry, TOut>(scalar: scalar.Scalar));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class TopologyScalar {
    public static readonly TopologyScalar Manifold = new(key: 0, output: typeof(bool), extract: static (g, op) => Analyze.ManifoldOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar Euler = new(key: 1, output: typeof(int), extract: static (g, op) => Analyze.EulerOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar BoundaryLoops = new(key: 2, output: typeof(int), extract: static (g, op) => Analyze.BoundaryLoopsOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar Genus = new(key: 3, output: typeof(int), extract: static (g, op) => Analyze.GenusOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar HoleCount = new(key: 4, output: typeof(int), extract: static (g, op) => Analyze.HoleCountOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar FaceCount = new(key: 5, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.Faces.Count, brepCount: static b => b.Faces.Count).Map(static value => (object)value));
    public static readonly TopologyScalar EdgeCount = new(key: 6, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.TopologyEdges.Count, brepCount: static b => b.Edges.Count).Map(static value => (object)value));
    public static readonly TopologyScalar VertexCount = new(key: 7, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(static value => (object)value));
    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial Fin<object> Extract(GeometryBase geometry, Op op);
    internal Fin<int> IntegerOf(GeometryBase geometry, Op op) => Extract(geometry: geometry, op: op).Bind(value => value is int count ? Fin.Succ(count) : Fin.Fail<int>(op.InvalidResult()));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Topologies<TGeometry, TOut>(Topologies aspect) where TGeometry : notnull => Aspect<Topologies, TGeometry, TOut>(aspect: aspect);
    public static Operation<TGeometry, TOut> Coerce<TGeometry, TOut>() where TGeometry : notnull where TOut : notnull {
        Op key = Op.Of();
        return (Domain.Kind.Of(typeof(TOut)), GeometryKernel.CanCoerce(typeof(TGeometry), typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => KernelLift<TGeometry, TOut, Op>(key: key, state: key, requirement: Requirement.Basic, extract: static (op, g, ctx) => GeometryKernel.CoerceTo<TOut>(g, ctx, op).Bind(coerced => op.Accept(value: coerced))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    internal static Operation<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return GeometryKernel.Can(type: typeof(TGeometry), predicate: static _ => true)
            ? typeof(TOut) switch {
                Type t when t == typeof(Kind) => KernelLift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k))).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(string) => KernelLift<TGeometry, string, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.ToString(null, CultureInfo.InvariantCulture)))).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(Topology) => KernelLift<TGeometry, Topology, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.Topology))).As<TGeometry, TOut>(key: key),
                _ => key.Unsupported<TGeometry, TOut>(),
            }
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyDomains<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (GeometryKernel.CanCurveForm(type: typeof(TGeometry)) || GeometryKernel.CanSurfaceForm(type: typeof(TGeometry)))
            ? KernelLift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologySolidOrientation<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(BrepSolidOrientation) && GeometryKernel.CanEvaluateTopology(type: typeof(TGeometry))
            ? KernelLift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => SolidOrientationOf(geometry: g, op: op).Bind(o => op.Accept(value: o))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyComponents<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Mesh))
            ? KernelLift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(c => ProjectComponents<TOut>(components: c, op: op))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyContains<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid && typeof(TOut) == typeof(bool) && GeometryKernel.CanEvaluateSolidTopology(type: typeof(TGeometry))
            ? KernelLift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(c => s.Key.Accept(value: c))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyScalar<TGeometry, TOut>(TopologyScalar scalar) where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == scalar.Output && GeometryKernel.CanEvaluateTopology(type: typeof(TGeometry))
            ? KernelLift<TGeometry, TOut, (Op Key, TopologyScalar Scalar)>(key: key, state: (Key: key, Scalar: scalar), extract: static (s, g, _) =>
                ExtractTopologyScalar(geometry: g, scalar: s.Scalar, op: s.Key)
                    .Bind(value => value is TOut typed ? s.Key.Accept(value: typed) : Fin.Fail<Seq<TOut>>(s.Key.Unsupported(geometryType: value.GetType(), outputType: typeof(TOut)))))
            : key.Unsupported<TGeometry, TOut>();
    }
    private static Operation<TGeometry, TValue> KernelLift<TGeometry, TValue, TState>(Op key, TState state, Func<TState, TGeometry, Context, Fin<Seq<TValue>>> extract, Requirement? requirement = null, bool requiresContext = true) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (State: state, Extract: extract),
            evaluator: static (s, geometry) =>
                from context in Env.Asks
                from result in s.Extract(arg1: s.State, arg2: geometry, arg3: context).ToEff()
                select result);
    private static Fin<TResult> OnGeometry<TGeometry, TResult>(TGeometry geometry, Op op, Func<Mesh, Fin<TResult>> onMesh, Func<Brep, Fin<TResult>> onBrep) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => onMesh(arg: mesh),
            Brep brep => onBrep(arg: brep),
            GeometryBase { HasBrepForm: true } native => GeometryKernel.BrepForm(source: native, op: op).Bind(lease => lease.Use(project: onBrep)),
            object brepLike when GeometryKernel.CanCoerce(source: brepLike.GetType(), target: typeof(Brep)) => GeometryKernel.BrepForm(source: brepLike, op: op).Bind(lease => lease.Use(project: onBrep)),
            _ => Fin.Fail<TResult>(op.Unsupported(g.GetType(), typeof(TResult))),
        });
    private static Fin<object> ExtractTopologyScalar<TGeometry>(TGeometry geometry, TopologyScalar scalar, Op op) where TGeometry : notnull =>
        OnGeometry(
            geometry: geometry,
            op: op,
            onMesh: mesh => scalar.Extract(geometry: mesh, op: op),
            onBrep: brep => scalar.Extract(geometry: brep, op: op));
    internal static Fin<Seq<Interval>> DomainsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => Fin.Succ(Seq(curve.Domain)),
            Surface surface => Fin.Succ(Seq(surface.Domain(direction: 0), surface.Domain(direction: 1))),
            object surfaceLike when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) => GeometryKernel.SurfaceForm(source: surfaceLike, op: op).Bind(lease => lease.Use(surface => DomainsOf(geometry: surface, op: op))),
            _ => Fin.Fail<Seq<Interval>>(op.Unsupported(g.GetType(), typeof(Interval))),
        });
    internal static Fin<BrepSolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: mesh => Fin.Succ(mesh.SolidOrientation() switch {
                1 => BrepSolidOrientation.Outward,
                -1 => BrepSolidOrientation.Inward,
                _ => BrepSolidOrientation.None,
            }),
            onBrep: brep => Fin.Succ(brep.SolidOrientation));
    internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : notnull =>
        from _ in guard(target.IsValid, op.InvalidInput())
        from contained in OnGeometry(
            geometry: geometry,
            op: op,
            onMesh: mesh => Fin.Succ(mesh.IsPointInside(target, context.Absolute.Value, false)),
            onBrep: brep => Fin.Succ(brep.IsPointInside(target, context.Absolute.Value, false)))
        select contained;
    internal static Fin<Seq<GeometryBase>> ComponentsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => Fin.Succ(toSeq(mesh.SplitDisjointPieces().Cast<GeometryBase>())),
            Brep brep => BrepComponentsOf(brep: brep, op: op),
            GeometryBase { HasBrepForm: true } native => GeometryKernel.BrepForm(source: native, op: op).Bind(lease => lease.Use(brep => BrepComponentsOf(brep: brep, op: op))),
            _ => Fin.Fail<Seq<GeometryBase>>(op.Unsupported(g.GetType(), typeof(Seq<GeometryBase>))),
        });
    private static Fin<Seq<GeometryBase>> BrepComponentsOf(Brep brep, Op op) =>
        brep.GetConnectedComponents() switch {
            Brep[] cs when cs.Length > 0 => Fin.Succ(toSeq(cs.Cast<GeometryBase>())),
            _ when brep.IsValid => op.AcceptValue(brep).Map(static v => Seq((GeometryBase)v.DuplicateBrep())),
            _ => Fin.Fail<Seq<GeometryBase>>(op.InvalidResult()),
        };
    private static Fin<Seq<TOut>> ProjectComponents<TOut>(Seq<GeometryBase> components, Op op) =>
        components.TraverseM(component => component is TOut typed ? Fin.Succ(typed) : Fin.Fail<TOut>(op.Unsupported(geometryType: component.GetType(), outputType: typeof(TOut)))).As()
            .BindFail(error => components.Iter(static component => component.Dispose()) switch { _ => Fin.Fail<Seq<TOut>>(error) });
    internal static Fin<bool> ManifoldOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool _)),
            onBrep: static b => Fin.Succ(b.IsManifold));
    internal static Fin<int> EulerOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.TopologyVertices.Count - m.TopologyEdges.Count + m.Faces.Count),
            onBrep: b => b.IsManifold ? Fin.Succ(b.Vertices.Count - b.Edges.Count + b.Faces.Count) : Fin.Fail<int>(op.Unsupported(typeof(Brep), typeof(int))));
    internal static Fin<int> BoundaryLoopsOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(Optional(m.GetNakedEdges()).Map(static p => p.Length).IfNone(0)),
            onBrep: static b => Fin.Succ(BrepBoundaryCount(brep: b, predicate: static l => l.LoopType is BrepLoopType.Outer or BrepLoopType.Inner)));
    internal static Fin<int> GenusOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: m => m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented
                ? (EulerOf(geometry: m, op: op), BoundaryLoopsOf(geometry: m, op: op), ComponentCountOf(geometry: m, op: op)).Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
                : Fin.Fail<int>(op.Unsupported(typeof(Mesh), typeof(int))),
            onBrep: b => b.IsManifold
                ? (EulerOf(geometry: b, op: op), BoundaryLoopsOf(geometry: b, op: op), ComponentCountOf(geometry: b, op: op)).Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
                : Fin.Fail<int>(op.Unsupported(typeof(Brep), typeof(int))));
    internal static Fin<int> HoleCountOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: m => (BoundaryLoopsOf(geometry: m, op: op), ComponentCountOf(geometry: m, op: op)).Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As(),
            onBrep: b => (BoundaryLoopsOf(geometry: b, op: op), ComponentCountOf(geometry: b, op: op)).Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As());
    internal static Fin<int> ElementCountOf<TG>(TG geometry, Op op, Func<Mesh, int> meshCount, Func<Brep, int> brepCount) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op, onMesh: m => Fin.Succ(meshCount(arg: m)), onBrep: b => Fin.Succ(brepCount(arg: b)));
    private static int BrepBoundaryCount(Brep brep, Func<BrepLoop, bool> predicate) =>
        toSeq(brep.Loops).Filter(loop => predicate(arg: loop) && toSeq(loop.Trims).Exists(static trim => trim.Edge is { Valence: EdgeAdjacency.Naked })).Count;
    private static Fin<int> ComponentCountOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        ComponentsOf(geometry: geometry, op: op)
            .Map(static components => components.Iter(static component => component.Dispose()) switch { _ => Math.Max(val1: 1, val2: components.Count) });
}
