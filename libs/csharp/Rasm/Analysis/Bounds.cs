namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Bounds : IAspect {
    public sealed record AxisAligned : Bounds; public sealed record Oriented(Plane Plane) : Bounds; public sealed record Transformed(Transform Transform) : Bounds; public sealed record Center : Bounds;
    public sealed record Corners(bool Unique = false) : Bounds; public sealed record Edges : Bounds; public sealed record Area : Bounds; public sealed record Volume : Bounds;
    private static readonly Op BoundsKey = Op.Of(name: nameof(Bounds));
    private static readonly Op OrientedKey = Op.Of(name: "OrientedBounds");
    private static readonly Op TransformedKey = Op.Of(name: "TransformedBounds");
    private static readonly Op CenterKey = Op.Of(name: "BoundsCenter");
    private static readonly Op CornersKey = Op.Of(name: "BoundsCorners");
    private static readonly Op BoxEdgesKey = Op.Of(name: "BoxEdges");
    private static readonly Op BoxAreaKey = Op.Of(name: "BoxArea");
    private static readonly Op BoxVolumeKey = Op.Of(name: "BoxVolume");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        axisAligned: static _ => (typeof(TOut) == typeof(BoundingBox) && Dispatch.SupportsBounds(typeof(TGeometry), includeSphere: true))
            ? Analyze.Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                key: BoundsKey, state: BoundsKey,
                evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(b => Analyze.One(key: op, value: b)).ToEff()))
            : BoundsKey.Unsupported<TGeometry, TOut>(),
        oriented: static o => (typeof(TOut) == typeof(Rhino.Geometry.Box) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Analyze.Native<TGeometry, TOut, GeometryBase, Rhino.Geometry.Box, (Op Key, Plane Plane)>(
                key: OrientedKey, state: (OrientedKey, o.Plane),
                project: static (state, native) => native.GetBoundingBox(plane: state.Plane, worldBox: out Rhino.Geometry.Box box) switch {
                    { IsValid: true } when box.IsValid => Analyze.One(key: state.Key, value: box).ToEff(),
                    _ => Fin.Fail<Seq<Rhino.Geometry.Box>>(state.Key.InvalidResult()).ToEff(),
                })
            : OrientedKey.Unsupported<TGeometry, TOut>(),
        transformed: static t => (typeof(TOut) == typeof(BoundingBox) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Analyze.Native<TGeometry, TOut, GeometryBase, BoundingBox, (Op Key, Transform Xform)>(
                key: TransformedKey, state: (Key: TransformedKey, Xform: t.Transform),
                project: static (state, native) => Analyze.One(key: state.Key, value: native.GetBoundingBox(xform: state.Xform)).ToEff())
            : TransformedKey.Unsupported<TGeometry, TOut>(),
        center: static _ => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: CenterKey, query: Query<TGeometry, Point3d>.Build(
                key: CenterKey, state: CenterKey,
                evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(b => Analyze.One(key: op, value: b.Center)).ToEff()))
            : CenterKey.Unsupported<TGeometry, TOut>(),
        corners: static c => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: CornersKey, query: Query<TGeometry, Point3d>.Build(
                key: CornersKey, requiresContext: c.Unique, state: (Key: CornersKey, c.Unique),
                evaluator: static (state, geometry) =>
                    from runtime in Env.EnvAsks
                    from bbox in geometry.Bounds(op: state.Key).ToEff()
                    from result in Analyze.Many(key: state.Key, values: state.Unique ? Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: runtime.Context.Absolute.Value) : bbox.GetCorners()).ToEff()
                    select result))
            : CornersKey.Unsupported<TGeometry, TOut>(),
        edges: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
            ? Analyze.Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                key: BoxEdgesKey, state: BoxEdgesKey,
                evaluator: static (op, geometry) => Analyze.Many(key: op, values: geometry.GetEdges()).ToEff()))
            : BoxEdgesKey.Unsupported<TGeometry, TOut>(),
        area: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxAreaKey, boundingBox: static g => g.Area, box: static g => g.Area),
        volume: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxVolumeKey, boundingBox: static g => g.Volume, box: static g => g.Volume));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull => Aspect<Bounds, TGeometry, TOut>(aspect: aspect);
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
}
