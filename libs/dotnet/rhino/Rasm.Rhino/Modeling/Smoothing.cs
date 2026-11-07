namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SmoothFrame {
    public SmoothingCoordinateSystem System => Switch(world: static _ => SmoothingCoordinateSystem.World, objectUvn: static _ => SmoothingCoordinateSystem.Object, cPlane: static _ => SmoothingCoordinateSystem.CPlane);

    public Plane Plane => Switch(world: static _ => Plane.WorldXY, objectUvn: static _ => Plane.WorldXY, cPlane: static frame => frame.Frame);

    public sealed record World : SmoothFrame;

    public sealed record ObjectUvn : SmoothFrame;

    public sealed record CPlane(Plane Frame) : SmoothFrame;
}

public sealed record SmoothOptions(double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothFrame Frame) {
    public int Iterations { get; init; } = 1;

    public Option<Seq<int>> Vertices { get; init; }

    public Step<T> Step<T>() where T : GeometryBase =>
        Steps.Derived<T>(target => (GeometryBase)target switch {
            Curve curve => Products.Of<GeometryBase>(() => curve.Smooth(Factor, X, Y, Z, FixBoundaries, Frame.System, Frame.Plane)).Bind(static smoothed => IO.lift(Conversions.Cast<T>(smoothed))),
            Surface surface => Products.Of<GeometryBase>(() => surface.Smooth(Factor, X, Y, Z, FixBoundaries, Frame.System, Frame.Plane)).Bind(static smoothed => IO.lift(Conversions.Cast<T>(smoothed))),
            Mesh mesh => Vertices.Match(
                Some: vertices =>
                    from held in IO.lift(() => mesh.Within(ComponentIndexType.MeshVertex, vertices))
                    from smoothed in Products.Accepted(() => mesh.Smooth(held, Factor, Iterations, X, Y, Z, FixBoundaries, Frame.System, Frame.Plane))
                    select target,
                None: () => Products.Accepted(() => mesh.Smooth(Factor, Iterations, X, Y, Z, FixBoundaries, Frame.System, Frame.Plane)).Map(_ => target)),
            var other => IO.fail<T>(new RhinoFault(other.ObjectType, RhinoReason.Unsupported)),
        });
}
