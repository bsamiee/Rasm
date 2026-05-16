namespace Rasm.Analysis;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Domains<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase) || typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) || typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)))
            ? Cast<TGeometry, TOut>(key: key, operation: KernelLift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Coerce<TGeometry, TOut>() where TGeometry : notnull where TOut : notnull {
        Op key = Op.Of();
        return (KindLookup.Resolve(typeof(TOut)), GeometryKernel.CanCoerce(typeof(TGeometry), typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => KernelLift<TGeometry, TOut, Op>(key: key, state: key, requirement: Requirement.Basic, extract: static (op, g, ctx) => GeometryKernel.CoerceTo<TOut>(g, ctx, op).Bind(coerced => op.Accept(value: coerced))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Rasm.Domain.Kind) && GeometryKernel.CanKind(typeof(TGeometry))
            ? Cast<TGeometry, TOut>(key: key, operation: KernelLift<TGeometry, Rasm.Domain.Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => ((object)g).KindOf(context: ctx).Bind(k => op.Accept(value: k))))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static global::Rasm.Analysis.Operation<TGeometry, BrepSolidOrientation> SolidOrientation<TGeometry>() where TGeometry : GeometryBase {
        Op key = Op.Of();
        return KernelLift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => SolidOrientationOf(geometry: g, op: op).Bind(o => op.Accept(value: o)));
    }
    public static global::Rasm.Analysis.Operation<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase {
        Op key = Op.Of();
        return point.IsValid switch {
            false => global::Rasm.Analysis.Operation<TGeometry, bool>.Reject(key: key, fault: key.InvalidInput()),
            true => Cast<TGeometry, bool>(key: key, operation: KernelLift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(c => s.Key.Accept(value: c)))),
        };
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Mesh))
            ? Cast<TGeometry, TOut>(key: key, operation: KernelLift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(c => ProjectComponents<TOut>(components: c, op: op))))
            : key.Unsupported<TGeometry, TOut>();
    }
    private static global::Rasm.Analysis.Operation<TGeometry, TValue> KernelLift<TGeometry, TValue, TState>(Op key, TState state, Func<TState, TGeometry, Context, Fin<Seq<TValue>>> extract, Requirement? requirement = null, bool requiresContext = true) where TGeometry : notnull =>
        global::Rasm.Analysis.Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (State: state, Extract: extract),
            evaluator: static (s, geometry) =>
                from context in Env.Asks
                from result in s.Extract(arg1: s.State, arg2: geometry, arg3: context).ToEff()
                select result);
    internal static Fin<Seq<Interval>> DomainsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => Fin.Succ(Seq(curve.Domain)),
            Surface surface => Fin.Succ(Seq(surface.Domain(direction: 0), surface.Domain(direction: 1))),
            _ => Fin.Fail<Seq<Interval>>(op.Unsupported(g.GetType(), typeof(Interval))),
        });
    internal static Fin<BrepSolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : GeometryBase =>
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
    internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : GeometryBase =>
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
}
