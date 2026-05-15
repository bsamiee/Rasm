namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Boundaries : IAspect {
    public sealed record NakedCase : Boundaries; public sealed record OutlineCase(Plane Plane) : Boundaries; public sealed record SelfIntersectionCase : Boundaries; public sealed record AllCase : Boundaries;
    public static Boundaries Naked => new NakedCase(); public static Boundaries Outline(Plane plane) => new OutlineCase(Plane: plane);
    public static Boundaries SelfIntersection => new SelfIntersectionCase(); public static Boundaries All => new AllCase();
    private static readonly Op NakedKey = Op.Of(name: "NakedEdges");
    private static readonly Op OutlineKey = Op.Of(name: "Outlines");
    private static readonly Op SelfIntersectionKey = Op.Of(name: "SelfIntersections");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        nakedCase: static _ => (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Analyze.Curves<TGeometry, TOut>(aspect: Curves.Boundary),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) => Analyze.Native<TGeometry, TOut, Mesh, Polyline>(key: NakedKey, project: static mesh => NakedKey.ManyOrEmpty(values: mesh.GetNakedEdges()).ToEff()),
            _ => NakedKey.Unsupported<TGeometry, TOut>(),
        },
        outlineCase: static o => (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline) && o.Plane.IsValid)
            ? Analyze.Cast<TGeometry, TOut>(key: OutlineKey, query: Query<Mesh, Polyline>.Build(key: OutlineKey, state: (Op: OutlineKey, o.Plane), evaluator: static (state, geometry) => state.Op.Many(values: geometry.GetOutlines(plane: state.Plane)).ToEff()))
            : (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline))
                ? Analyze.Cast<TGeometry, TOut>(key: OutlineKey, query: Query<Mesh, Polyline>.Reject(key: OutlineKey, fault: OutlineKey.InvalidInput()))
                : OutlineKey.Unsupported<TGeometry, TOut>(),
        selfIntersectionCase: static _ => (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline))
            ? Analyze.Cast<TGeometry, TOut>(key: SelfIntersectionKey, query: Query<Mesh, Polyline>.Build(
                key: SelfIntersectionKey, requirement: Requirement.Basic, state: SelfIntersectionKey,
                evaluator: static (op, geometry) => from runtime in Env.EnvAsks
                                                    from result in Analyze.SelfIntersectionsValue(op: op, geometry: geometry, runtime: runtime).ToEff()
                                                    select result))
            : SelfIntersectionKey.Unsupported<TGeometry, TOut>(),
        allCase: static _ => Dispatch.Supports(CapTag.Curves, typeof(TGeometry)) && typeof(TOut) == typeof(Curve)
            ? Analyze.Curves<TGeometry, TOut>(aspect: Curves.All)
            : Curves.Key.Unsupported<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Boundaries<TGeometry, TOut>(Boundaries aspect) where TGeometry : notnull => Aspect<Boundaries, TGeometry, TOut>(aspect: aspect);
}
