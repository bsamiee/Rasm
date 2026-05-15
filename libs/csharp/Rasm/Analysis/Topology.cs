namespace Rasm.Analysis;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Domains<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) || typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)))
            ? Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, Interval>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from domains in DomainsOf(geometry: geometry, op: op).ToEff()
                    from result in op.Accept(values: domains).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Coerce<TGeometry, TOut>() where TGeometry : notnull where TOut : notnull {
        Op key = Op.Of();
        return (KindLookup.Resolve(typeof(TOut)), GeometryKernel.CanCoerce(typeof(TGeometry), typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => global::Rasm.Analysis.Operation<TGeometry, TOut>.Build(
                key: key, requirement: Requirement.Basic, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from coerced in GeometryKernel.Coerce<TOut>(geometry, context, op).ToEff()
                    from result in op.Accept(value: coerced).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Rasm.Domain.Kind) && GeometryKernel.CanKind(typeof(TGeometry))
            ? Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, Rasm.Domain.Kind>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(context: context).ToEff()
                    from result in op.Accept(value: kind).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static global::Rasm.Analysis.Operation<TGeometry, Rasm.Domain.SolidOrientation> SolidOrientation<TGeometry>() where TGeometry : GeometryBase {
        Op key = Op.Of();
        return global::Rasm.Analysis.Operation<TGeometry, Rasm.Domain.SolidOrientation>.Build(
            key: key, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from orientation in SolidOrientationOf(geometry: geometry, op: op).ToEff()
                from result in op.Accept(value: orientation).ToEff()
                select result);
    }
    public static global::Rasm.Analysis.Operation<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase {
        Op key = Op.Of();
        return point.IsValid switch {
            false => global::Rasm.Analysis.Operation<TGeometry, bool>.Reject(key: key, fault: key.InvalidInput()),
            true => Cast<TGeometry, bool>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, bool>.Build(
                key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, requiresContext: true,
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from contained in ContainsPoint(geometry: geometry, target: state.Target, context: context, op: state.Key).ToEff()
                    from result in state.Key.Accept(value: contained).ToEff()
                    select result)),
        };
    }
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Mesh))
            ? Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from components in ComponentsOf(geometry: geometry, op: op).ToEff()
                    from result in components.TraverseM(component => component is TOut typed ? Fin.Succ(typed) : Fin.Fail<TOut>(op.Unsupported(geometryType: component.GetType(), outputType: typeof(TOut)))).As().ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Fin<Seq<Interval>> DomainsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => Fin.Succ(Seq(curve.Domain)),
            Surface surface => Fin.Succ(Seq(surface.Domain(direction: 0), surface.Domain(direction: 1))),
            _ => Fin.Fail<Seq<Interval>>(op.Unsupported(g.GetType(), typeof(Interval))),
        });
    internal static Fin<SolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : GeometryBase =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Brep brep => Fin.Succ((SolidOrientation)(int)brep.SolidOrientation),
            Mesh mesh => Fin.Succ((SolidOrientation)mesh.SolidOrientation()),
            _ => Fin.Fail<SolidOrientation>(op.Unsupported(g.GetType(), typeof(SolidOrientation))),
        });
    internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : GeometryBase =>
        (Optional(geometry).ToFin(op.InvalidInput()), target.IsValid) switch {
            (_, false) => Fin.Fail<bool>(op.InvalidInput()),
            (Fin<TGeometry> source, true) => source.Bind(g => g switch {
                Brep brep => Fin.Succ(brep.IsPointInside(target, context.Absolute.Value, false)),
                Mesh mesh => Fin.Succ(mesh.IsPointInside(target, context.Absolute.Value, false)),
                _ => Fin.Fail<bool>(op.Unsupported(g.GetType(), typeof(bool))),
            }),
        };
    internal static Fin<Seq<GeometryBase>> ComponentsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => Fin.Succ(toSeq(mesh.SplitDisjointPieces().Cast<GeometryBase>())),
            Brep brep => BrepComponents(brep: brep, op: op),
            GeometryBase { HasBrepForm: true } native => Optional(Brep.TryConvertBrep(native)).ToFin(op.InvalidResult()).Bind(c => ReferenceEquals(native, c) ? BrepComponents(brep: c, op: op) : GeometryKernel.Borrowed(c, d => BrepComponents(brep: d, op: op))),
            _ => Fin.Fail<Seq<GeometryBase>>(op.Unsupported(g.GetType(), typeof(Seq<GeometryBase>))),
        });
    private static Fin<Seq<GeometryBase>> BrepComponents(Brep brep, Op op) =>
        brep.GetConnectedComponents() switch {
            Brep[] cs when cs.Length > 0 => Fin.Succ(toSeq(cs.Cast<GeometryBase>())),
            _ => Brep.SplitDisjointPieces(brep) switch {
                Brep[] ps when ps.Length > 0 => Fin.Succ(toSeq(ps.Cast<GeometryBase>())),
                _ => op.AcceptValue(brep).Map(static v => Seq((GeometryBase)v.DuplicateBrep())),
            },
        };
}
