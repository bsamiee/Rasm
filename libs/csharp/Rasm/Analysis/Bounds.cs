namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Bounds : IAspect {
    public sealed record AxisAlignedCase : Bounds; public sealed record OrientedCase(Plane Plane) : Bounds; public sealed record TransformedCase(Transform Transform) : Bounds; public sealed record CenterCase : Bounds;
    public sealed record CornersCase(bool Unique = false) : Bounds; public sealed record EdgesCase : Bounds; public sealed record AreaCase : Bounds; public sealed record VolumeCase : Bounds;
    public sealed record PrincipalCase : Bounds;
    public static Bounds AxisAligned => new AxisAlignedCase();
    public static Bounds Oriented(Plane plane) => new OrientedCase(Plane: plane);
    public static Bounds Transformed(Transform transform) => new TransformedCase(Transform: transform);
    public static Bounds Center => new CenterCase();
    public static Bounds Corners(bool unique = false) => new CornersCase(Unique: unique);
    public static Bounds Edges => new EdgesCase();
    public static Bounds Area => new AreaCase();
    public static Bounds Volume => new VolumeCase();
    public static Bounds Principal => new PrincipalCase();
    internal static readonly Op BoundsKey = Op.Of(name: nameof(Bounds));
    internal static readonly Op OrientedKey = Op.Of(name: "OrientedBounds");
    internal static readonly Op TransformedKey = Op.Of(name: "TransformedBounds");
    internal static readonly Op CenterKey = Op.Of(name: "BoundsCenter");
    internal static readonly Op CornersKey = Op.Of(name: "BoundsCorners");
    internal static readonly Op BoxEdgesKey = Op.Of(name: "BoxEdges");
    internal static readonly Op BoxAreaKey = Op.Of(name: "BoxArea");
    internal static readonly Op BoxVolumeKey = Op.Of(name: "BoxVolume");
    internal static readonly Op PrincipalKey = Op.Of(name: "PrincipalBounds");
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        axisAlignedCase: static _ => (typeof(TOut) == typeof(BoundingBox) && GeometryKernel.CanBound(typeof(TGeometry), includeSphere: true))
            ? Analyze.Cast<TGeometry, TOut>(key: BoundsKey, operation: global::Rasm.Analysis.Operation<TGeometry, BoundingBox>.Build(
                key: BoundsKey, state: BoundsKey,
                evaluator: static (op, geometry) => geometry.BoundsOf(op: op).Bind(b => op.Accept(value: b)).ToEff()))
            : BoundsKey.Unsupported<TGeometry, TOut>(),
        orientedCase: static o => (typeof(TOut) == typeof(Rhino.Geometry.Box) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Analyze.Native<TGeometry, TOut, GeometryBase, Rhino.Geometry.Box, (Op Key, Plane Plane)>(
                key: OrientedKey, state: (OrientedKey, o.Plane),
                project: static (state, native) => new Rhino.Geometry.Box(state.Plane, native) switch {
                    { IsValid: true } box => state.Key.Accept(value: box).ToEff(),
                    _ => Fin.Fail<Seq<Rhino.Geometry.Box>>(state.Key.InvalidResult()).ToEff(),
                })
            : OrientedKey.Unsupported<TGeometry, TOut>(),
        transformedCase: static t => (typeof(TOut) == typeof(BoundingBox) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Analyze.Native<TGeometry, TOut, GeometryBase, BoundingBox, (Op Key, Transform Xform)>(
                key: TransformedKey, state: (Key: TransformedKey, Xform: t.Transform),
                project: static (state, native) => state.Key.Accept(value: native.GetBoundingBox(xform: state.Xform)).ToEff())
            : TransformedKey.Unsupported<TGeometry, TOut>(),
        centerCase: static _ => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: CenterKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: CenterKey, state: CenterKey,
                evaluator: static (op, geometry) => geometry.BoundsOf(op: op).Bind(b => op.Accept(value: b.Center)).ToEff()))
            : CenterKey.Unsupported<TGeometry, TOut>(),
        cornersCase: static c => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: CornersKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: CornersKey, requiresContext: c.Unique, state: (Key: CornersKey, c.Unique),
                evaluator: static (state, geometry) =>
                    from runtime in Env.EnvAsks
                    from bbox in geometry.BoundsOf(op: state.Key).ToEff()
                    from result in state.Key.Accept(values: state.Unique ? Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: runtime.Context.Absolute.Value) : bbox.GetCorners()).ToEff()
                    select result))
            : CornersKey.Unsupported<TGeometry, TOut>(),
        edgesCase: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
            ? Analyze.Cast<TGeometry, TOut>(key: BoxEdgesKey, operation: global::Rasm.Analysis.Operation<BoundingBox, Line>.Build(
                key: BoxEdgesKey, state: BoxEdgesKey,
                evaluator: static (op, geometry) => op.Accept(values: geometry.GetEdges()).ToEff()))
            : BoxEdgesKey.Unsupported<TGeometry, TOut>(),
        areaCase: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxAreaKey, boundingBox: static g => g.Area, box: static g => g.Area),
        volumeCase: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxVolumeKey, boundingBox: static g => g.Volume, box: static g => g.Volume),
        principalCase: static _ => (typeof(TOut) == typeof(Rhino.Geometry.Box) && GeometryKernel.CanPrincipal(type: typeof(TGeometry)))
            ? Analyze.Native<TGeometry, TOut, GeometryBase, Rhino.Geometry.Box, Op>(
                key: PrincipalKey, state: PrincipalKey, requirement: Requirement.Basic, requiresContext: true,
                project: static (state, native) =>
                    from context in Env.Asks
                    from box in BoundsDispatch.PrincipalBoxOf(geometry: native, context: context, key: state).ToEff()
                    from result in state.Accept(value: box).ToEff()
                    select result)
            : PrincipalKey.Unsupported<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull => Aspect<Bounds, TGeometry, TOut>(aspect: aspect);
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> BoxMetric<TGeometry, TOut>(Op key, Func<BoundingBox, double> boundingBox, Func<Box, double> box) where TGeometry : notnull =>
        (typeof(TOut) == typeof(double), typeof(TGeometry)) switch {
            (true, Type geometry) when geometry == typeof(BoundingBox) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<BoundingBox, double>.Build(
                key: key, state: (Key: key, Project: boundingBox),
                evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff())),
            (true, Type geometry) when geometry == typeof(Box) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<Box, double>.Build(
                key: key, state: (Key: key, Project: box),
                evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff())),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
}

// --- [COMPOSITION] ------------------------------------------------------------------------
file static class BoundsDispatch {
    public static Fin<Box> PrincipalBoxOf(GeometryBase geometry, Context context, Op key) =>
        ResolveMass(geometry: geometry) switch {
            MassKind kind when kind.Equals(MassKind.None) => Fin.Fail<Box>(key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Box))),
            MassKind kind => kind.Compute(geometry: geometry, context: context, firstMoments: true, secondMoments: true, productMoments: true, op: key)
                .Bind(disposable => new Lease<IDisposable>.Owned(Value: disposable).Use(owned =>
                    PrincipalFrame(mass: owned, key: key)
                        .Map(frame => new Box(frame, geometry))
                        .Bind(box => box.IsValid ? Fin.Succ(box) : Fin.Fail<Box>(key.InvalidResult())))),
        };
    private static Fin<Plane> PrincipalFrame(IDisposable mass, Op key) =>
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
    private static MassKind ResolveMass(GeometryBase geometry) =>
        geometry switch {
            Curve => MassKind.Length,
            Brep brep => brep.IsSolid ? MassKind.Volume : MassKind.Area,
            Mesh mesh => mesh.IsSolid ? MassKind.Volume : MassKind.Area,
            Extrusion extrusion => extrusion.IsSolid ? MassKind.Volume : MassKind.Area,
            Surface surface => surface.IsSolid ? MassKind.Volume : MassKind.Area,
            _ => MassKind.None,
        };
}
