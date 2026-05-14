namespace Rasm.Analysis;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Domain<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (typeof(Curve).IsAssignableFrom(c: typeof(TGeometry)) || typeof(Surface).IsAssignableFrom(c: typeof(TGeometry)))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Interval>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(context: context).ToEff()
                    from domains in kind.Domains(geometry: geometry, op: op).ToEff()
                    from result in Many(key: op, values: domains).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (typeof(TOut).AsKind(), Dispatch.SupportsCoercion(source: typeof(TGeometry), target: typeof(TOut))) switch {
            (Option<Kind> someKind, true) when someKind.IsSome => Query<TGeometry, TOut>.Build(
                key: key, requirement: Requirement.Basic, requiresContext: true,
                state: (Key: key, Kind: someKind.IfNone(() => Rasm.Domain.Kind.Surface)),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from coerced in state.Kind.Coerce<TOut>(geometry: geometry, context: context, op: state.Key).ToEff()
                    from result in state.Key.One(value: coerced).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Query<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Rasm.Domain.Kind) && Dispatch.SupportsKind(source: typeof(TGeometry))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, Rasm.Domain.Kind>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(context: context).ToEff()
                    from result in One(key: op, value: kind).ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Query<TGeometry, Rasm.Domain.SolidOrientation> SolidOrientation<TGeometry>() where TGeometry : GeometryBase {
        Op key = Op.Of();
        return Query<TGeometry, Rasm.Domain.SolidOrientation>.Build(
            key: key, requiresContext: true, state: key,
            evaluator: static (op, geometry) =>
                from context in Env.Asks
                from kind in ((object)geometry).Kind(context: context).ToEff()
                from orientation in kind.SolidOrientation(geometry: geometry, op: op).ToEff()
                from result in One(key: op, value: orientation).ToEff()
                select result);
    }
    public static Query<TGeometry, bool> IsPointInside<TGeometry>(Point3d point) where TGeometry : GeometryBase {
        Op key = Op.Of();
        return point.IsValid switch {
            false => Query<TGeometry, bool>.Reject(key: key, fault: key.InvalidInput()),
            true => Cast<TGeometry, bool>(key: key, query: Query<TGeometry, bool>.Build(
                key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology, requiresContext: true,
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(context: context).ToEff()
                    from contained in kind.Contains(geometry: geometry, target: state.Target, context: context, op: state.Key).ToEff()
                    from result in One(key: state.Key, value: contained).ToEff()
                    select result)),
        };
    }
    public static Query<TGeometry, TOut> Components<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Mesh))
            ? Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(context: context).ToEff()
                    from components in kind.Components(geometry: geometry, context: context, op: op).ToEff()
                    from result in components.TraverseM(component => component is TOut typed ? Fin.Succ(typed) : Fin.Fail<TOut>(op.Unsupported(geometryType: component.GetType(), outputType: typeof(TOut)))).As().ToEff()
                    select result))
            : key.Unsupported<TGeometry, TOut>();
    }
}
