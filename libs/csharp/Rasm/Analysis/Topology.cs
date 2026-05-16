using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Topologies : IAspect {
    public sealed record DomainsCase : Topologies;
    public sealed record SolidOrientationCase : Topologies;
    public sealed record ComponentsCase : Topologies;
    public sealed record ContainsPointCase(Point3d Point) : Topologies;
    public sealed record ManifoldCase : Topologies;
    public sealed record EulerCharacteristicCase : Topologies;
    public sealed record BoundaryLoopsCase : Topologies;
    public sealed record GenusCase : Topologies;
    public sealed record HoleCountCase : Topologies;
    public static Topologies Domains => new DomainsCase();
    public static Topologies SolidOrientation => new SolidOrientationCase();
    public static Topologies Components => new ComponentsCase();
    public static Topologies ContainsPoint(Point3d point) => new ContainsPointCase(Point: point);
    public static Topologies Manifold => new ManifoldCase();
    public static Topologies Euler => new EulerCharacteristicCase();
    public static Topologies BoundaryLoops => new BoundaryLoopsCase();
    public static Topologies Genus => new GenusCase();
    public static Topologies HoleCount => new HoleCountCase();
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        domainsCase: static _ => Analyze.TopologyDomains<TGeometry, TOut>(),
        solidOrientationCase: static _ => Analyze.TopologySolidOrientation<TGeometry, TOut>(),
        componentsCase: static _ => Analyze.TopologyComponents<TGeometry, TOut>(),
        containsPointCase: static cp => Analyze.TopologyContains<TGeometry, TOut>(point: cp.Point),
        manifoldCase: static _ => Analyze.TopologyManifold<TGeometry, TOut>(),
        eulerCharacteristicCase: static _ => Analyze.TopologyEuler<TGeometry, TOut>(),
        boundaryLoopsCase: static _ => Analyze.TopologyBoundaryLoops<TGeometry, TOut>(),
        genusCase: static _ => Analyze.TopologyGenus<TGeometry, TOut>(),
        holeCountCase: static _ => Analyze.TopologyHoleCount<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Topologies<TGeometry, TOut>(Topologies aspect) where TGeometry : notnull => Aspect<Topologies, TGeometry, TOut>(aspect: aspect);
    public static Operation<TGeometry, TOut> Coerce<TGeometry, TOut>() where TGeometry : notnull where TOut : notnull {
        Op key = Op.Of();
        return (KindLookup.Resolve(typeof(TOut)), GeometryKernel.CanCoerce(typeof(TGeometry), typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => KernelLift<TGeometry, TOut, Op>(key: key, state: key, requirement: Requirement.Basic, extract: static (op, g, ctx) => GeometryKernel.CoerceTo<TOut>(g, ctx, op).Bind(coerced => op.Accept(value: coerced))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Operation<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Kind) && GeometryKernel.CanKind(typeof(TGeometry))
            ? KernelLift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => ((object)g).KindOf(context: ctx).Bind(k => op.Accept(value: k))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyDomains<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase) || typeof(Curve).IsAssignableFrom(typeof(TGeometry)) || typeof(Surface).IsAssignableFrom(typeof(TGeometry)))
            ? KernelLift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologySolidOrientation<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(BrepSolidOrientation) && (typeof(TGeometry) == typeof(object) || typeof(GeometryBase).IsAssignableFrom(typeof(TGeometry)))
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
        return point.IsValid && typeof(TOut) == typeof(bool) && (typeof(TGeometry) == typeof(object) || typeof(GeometryBase).IsAssignableFrom(typeof(TGeometry)))
            ? KernelLift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(c => s.Key.Accept(value: c))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TG, TO> TopologyManifold<TG, TO>() where TG : notnull => TopologyScalarOp<TG, TO, bool>(key: Op.Of(), extract: static (g, op) => ManifoldOf(geometry: g, op: op));
    internal static Operation<TG, TO> TopologyEuler<TG, TO>() where TG : notnull => TopologyScalarOp<TG, TO, int>(key: Op.Of(), extract: static (g, op) => EulerOf(geometry: g, op: op));
    internal static Operation<TG, TO> TopologyBoundaryLoops<TG, TO>() where TG : notnull => TopologyScalarOp<TG, TO, int>(key: Op.Of(), extract: static (g, op) => BoundaryLoopsOf(geometry: g, op: op));
    internal static Operation<TG, TO> TopologyGenus<TG, TO>() where TG : notnull => TopologyScalarOp<TG, TO, int>(key: Op.Of(), extract: static (g, op) => GenusOf(geometry: g, op: op));
    internal static Operation<TG, TO> TopologyHoleCount<TG, TO>() where TG : notnull => TopologyScalarOp<TG, TO, int>(key: Op.Of(), extract: static (g, op) => HoleCountOf(geometry: g, op: op));
    private static Operation<TGeometry, TOut> TopologyScalarOp<TGeometry, TOut, TValue>(Op key, Func<TGeometry, Op, Fin<TValue>> extract) where TGeometry : notnull =>
        typeof(TOut) == typeof(TValue) && (typeof(TGeometry) == typeof(object) || typeof(GeometryBase).IsAssignableFrom(typeof(TGeometry)))
            ? KernelLift<TGeometry, TValue, (Op Key, Func<TGeometry, Op, Fin<TValue>> Extract)>(key: key, state: (Key: key, Extract: extract), extract: static (s, g, _) => s.Extract(arg1: g, arg2: s.Key).Bind(v => s.Key.Accept(value: v))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
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
            _ => Fin.Fail<TResult>(op.Unsupported(g.GetType(), typeof(TResult))),
        });
    internal static Fin<Seq<Interval>> DomainsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => Fin.Succ(Seq(curve.Domain)),
            Surface surface => Fin.Succ(Seq(surface.Domain(direction: 0), surface.Domain(direction: 1))),
            _ => Fin.Fail<Seq<Interval>>(op.Unsupported(g.GetType(), typeof(Interval))),
        });
    internal static Fin<BrepSolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Brep brep => Fin.Succ(brep.SolidOrientation),
            Mesh mesh => mesh.SolidOrientation() switch {
                1 => Fin.Succ(BrepSolidOrientation.Outward),
                -1 => Fin.Succ(BrepSolidOrientation.Inward),
                0 => Fin.Succ(BrepSolidOrientation.None),
                _ => Fin.Fail<BrepSolidOrientation>(op.InvalidResult()),
            },
            _ => Fin.Fail<BrepSolidOrientation>(op.Unsupported(g.GetType(), typeof(BrepSolidOrientation))),
        });
    internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : notnull =>
        from _ in guard(target.IsValid, op.InvalidInput())
        from g in Optional(geometry).ToFin(op.InvalidInput())
        from contained in g switch {
            Brep brep => Fin.Succ(brep.IsPointInside(target, context.Absolute.Value, false)),
            Mesh mesh => Fin.Succ(mesh.IsPointInside(target, context.Absolute.Value, false)),
            _ => Fin.Fail<bool>(op.Unsupported(g.GetType(), typeof(bool))),
        }
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
        OnGeometry<TG, bool>(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool _)),
            onBrep: static b => Fin.Succ(b.IsManifold));
    internal static Fin<int> EulerOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry<TG, int>(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.TopologyVertices.Count - m.TopologyEdges.Count + m.Faces.Count),
            onBrep: b => b.IsManifold ? Fin.Succ(b.Vertices.Count - b.Edges.Count + b.Faces.Count) : Fin.Fail<int>(op.Unsupported(typeof(Brep), typeof(int))));
    internal static Fin<int> BoundaryLoopsOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry<TG, int>(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(Optional(m.GetNakedEdges()).Map(static p => p.Length).IfNone(0)),
            onBrep: static b => Fin.Succ(BrepLoopCount(brep: b, predicate: static l => l.LoopType is BrepLoopType.Outer or BrepLoopType.Inner or BrepLoopType.Slit)));
    private static int BrepLoopCount(Brep brep, Func<BrepLoop, bool> predicate) => toSeq(brep.Loops).Filter(predicate).Count;
    internal static Fin<int> GenusOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry<TG, int>(geometry: geometry, op: op,
            onMesh: m => m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented
                ? Fin.Succ((2 - (m.TopologyVertices.Count - m.TopologyEdges.Count + m.Faces.Count) - Optional(m.GetNakedEdges()).Map(static p => p.Length).IfNone(0)) / 2)
                : Fin.Fail<int>(op.Unsupported(typeof(Mesh), typeof(int))),
            onBrep: b => b.IsManifold && b.SolidOrientation != BrepSolidOrientation.None
                ? Fin.Succ((2 - (b.Vertices.Count - b.Edges.Count + b.Faces.Count) - BrepLoopCount(brep: b, predicate: static l => l.LoopType is BrepLoopType.Outer or BrepLoopType.Inner or BrepLoopType.Slit)) / 2)
                : Fin.Fail<int>(op.Unsupported(typeof(Brep), typeof(int))));
    internal static Fin<int> HoleCountOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry<TG, int>(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(Math.Max(0, Optional(m.GetNakedEdges()).Map(static p => p.Length).IfNone(0) - 1)),
            onBrep: static b => Fin.Succ(BrepLoopCount(brep: b, predicate: static l => l.LoopType == BrepLoopType.Inner)));
}
