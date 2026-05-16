namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record BoundsShape {
    public sealed record AxisAligned : BoundsShape;
    public sealed record InPlane(Plane Plane) : BoundsShape;
    public sealed record Transformed(Transform Xform) : BoundsShape;
    public sealed record PrincipalFrame : BoundsShape;
}

[Union]
public partial record BoxDerivation {
    public sealed record Center : BoxDerivation;
    public sealed record Corners(bool Unique) : BoxDerivation;
    public sealed record Edges : BoxDerivation;
    public sealed record Area : BoxDerivation;
    public sealed record Volume : BoxDerivation;
    public sealed record Diagonal : BoxDerivation;
    public sealed record AspectRatio : BoxDerivation;
    public sealed record Tightness : BoxDerivation;
}

[Union]
public partial record EnclosingShape {
    public sealed record SphereCase : EnclosingShape;
    public sealed record CircleCase(Plane Plane) : EnclosingShape;
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Bounds : IAspect {
    public sealed record BoxCase(BoundsShape Shape) : Bounds;
    public sealed record DerivedCase(BoxDerivation Derivation) : Bounds;
    public sealed record EnclosingCase(EnclosingShape Shape) : Bounds;
    public static Bounds AxisAligned => new BoxCase(Shape: new BoundsShape.AxisAligned());
    public static Bounds Oriented(Plane plane) => new BoxCase(Shape: new BoundsShape.InPlane(Plane: plane));
    public static Bounds Transformed(Transform transform) => new BoxCase(Shape: new BoundsShape.Transformed(Xform: transform));
    public static Bounds Principal => new BoxCase(Shape: new BoundsShape.PrincipalFrame());
    public static Bounds Center => new DerivedCase(Derivation: new BoxDerivation.Center());
    public static Bounds Corners(bool unique = false) => new DerivedCase(Derivation: new BoxDerivation.Corners(Unique: unique));
    public static Bounds Edges => new DerivedCase(Derivation: new BoxDerivation.Edges());
    public static Bounds Area => new DerivedCase(Derivation: new BoxDerivation.Area());
    public static Bounds Volume => new DerivedCase(Derivation: new BoxDerivation.Volume());
    public static Bounds Diagonal => new DerivedCase(Derivation: new BoxDerivation.Diagonal());
    public static Bounds AspectRatio => new DerivedCase(Derivation: new BoxDerivation.AspectRatio());
    public static Bounds Tightness => new DerivedCase(Derivation: new BoxDerivation.Tightness());
    public static Bounds Enclosing(EnclosingShape shape) => new EnclosingCase(Shape: shape);
    internal static readonly Op BoundsKey = Op.Of(name: nameof(Bounds));
    internal static readonly Op OrientedKey = Op.Of(name: "OrientedBounds");
    internal static readonly Op TransformedKey = Op.Of(name: "TransformedBounds");
    internal static readonly Op PrincipalKey = Op.Of(name: "PrincipalBounds");
    internal static readonly Op CenterKey = Op.Of(name: "BoundsCenter");
    internal static readonly Op CornersKey = Op.Of(name: "BoundsCorners");
    internal static readonly Op BoxEdgesKey = Op.Of(name: "BoxEdges");
    internal static readonly Op BoxAreaKey = Op.Of(name: "BoxArea");
    internal static readonly Op BoxVolumeKey = Op.Of(name: "BoxVolume");
    internal static readonly Op BoxDiagonalKey = Op.Of(name: "BoxDiagonal");
    internal static readonly Op BoxAspectRatioKey = Op.Of(name: "BoxAspectRatio");
    internal static readonly Op BoxTightnessKey = Op.Of(name: "BoxTightness");
    internal static readonly Op EnclosingSphereKey = Op.Of(name: "EnclosingSphere");
    internal static readonly Op EnclosingCircleKey = Op.Of(name: "EnclosingCircle");
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        boxCase: static b => b.Shape.Switch(
            axisAligned: static _ => (typeof(TOut) == typeof(BoundingBox) && GeometryKernel.CanBound(typeof(TGeometry), includeSphere: true))
                ? Analysis.Operation<TGeometry, BoundingBox>.Build(
                    key: BoundsKey, state: BoundsKey,
                    evaluator: static (op, geometry) => geometry.BoundsOf(op: op).Bind(b => op.Accept(value: b)).ToEff()).As<TGeometry, TOut>(key: BoundsKey)
                : BoundsKey.Unsupported<TGeometry, TOut>(),
            inPlane: static p => (typeof(TOut) == typeof(Box) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, Box, (Op Key, Plane Plane)>(
                    key: OrientedKey, state: (OrientedKey, p.Plane),
                    project: static (state, native) => new Box(state.Plane, native) switch {
                        { IsValid: true } box => state.Key.Accept(value: box).ToEff(),
                        _ => Fin.Fail<Seq<Box>>(state.Key.InvalidResult()).ToEff(),
                    })
                : OrientedKey.Unsupported<TGeometry, TOut>(),
            transformed: static t => (typeof(TOut) == typeof(BoundingBox) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, BoundingBox, (Op Key, Transform Xform)>(
                    key: TransformedKey, state: (Key: TransformedKey, t.Xform),
                    project: static (state, native) => state.Key.Accept(value: native.GetBoundingBox(xform: state.Xform)).ToEff())
                : TransformedKey.Unsupported<TGeometry, TOut>(),
            principalFrame: static _ => (typeof(TOut) == typeof(Box) && GeometryKernel.CanPrincipal(type: typeof(TGeometry)))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, Box, Op>(
                    key: PrincipalKey, state: PrincipalKey, requirement: Requirement.Basic, requiresContext: true,
                    project: static (state, native) =>
                        from context in Env.Asks
                        from frame in MassKind.PrincipalFrameOf(geometry: native, context: context, key: state).ToEff()
                        from box in (new Box(frame, native) switch {
                            { IsValid: true } b => Fin.Succ(b),
                            _ => Fin.Fail<Box>(state.InvalidResult()),
                        }).ToEff()
                        from result in state.Accept(value: box).ToEff()
                        select result)
                : PrincipalKey.Unsupported<TGeometry, TOut>()),
        derivedCase: static d => d.Derivation.Switch(
            center: static _ => typeof(TOut) == typeof(Point3d)
                ? Analysis.Operation<TGeometry, Point3d>.Build(
                    key: CenterKey, state: CenterKey,
                    evaluator: static (op, geometry) => geometry.BoundsOf(op: op).Bind(b => op.Accept(value: b.Center)).ToEff()).As<TGeometry, TOut>(key: CenterKey)
                : CenterKey.Unsupported<TGeometry, TOut>(),
            corners: static c => typeof(TOut) == typeof(Point3d)
                ? Analysis.Operation<TGeometry, Point3d>.Build(
                    key: CornersKey, requiresContext: c.Unique, state: (Key: CornersKey, c.Unique),
                    evaluator: static (state, geometry) =>
                        from runtime in Env.EnvAsks
                        from bbox in geometry.BoundsOf(op: state.Key).ToEff()
                        from result in state.Key.Accept(values: state.Unique ? Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: runtime.Context.Absolute.Value) : bbox.GetCorners()).ToEff()
                        select result).As<TGeometry, TOut>(key: CornersKey)
                : CornersKey.Unsupported<TGeometry, TOut>(),
            edges: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
                ? Analysis.Operation<BoundingBox, Line>.Build(
                    key: BoxEdgesKey, state: BoxEdgesKey,
                    evaluator: static (op, geometry) => op.Accept(values: geometry.GetEdges()).ToEff()).As<TGeometry, TOut>(key: BoxEdgesKey)
                : BoxEdgesKey.Unsupported<TGeometry, TOut>(),
            area: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxAreaKey, boundingBox: static g => g.Area, box: static g => g.Area),
            volume: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxVolumeKey, boundingBox: static g => g.Volume, box: static g => g.Volume),
            diagonal: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxDiagonalKey, boundingBox: static g => g.Diagonal.Length, box: static g => g.BoundingBox.Diagonal.Length),
            aspectRatio: static _ => Analyze.BoxMetric<TGeometry, TOut>(key: BoxAspectRatioKey, boundingBox: static g => AspectOf(g.Diagonal), box: static g => AspectOf(new Vector3d(g.X.Length, g.Y.Length, g.Z.Length))),
            tightness: static _ => (typeof(TOut) == typeof(double) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)) && GeometryKernel.CanPrincipal(type: typeof(TGeometry)))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, double, Op>(
                    key: BoxTightnessKey, state: BoxTightnessKey, requirement: Requirement.Basic, requiresContext: true,
                    project: static (state, native) =>
                        from context in Env.Asks
                        from frame in MassKind.PrincipalFrameOf(geometry: native, context: context, key: state).ToEff()
                        from obb in (new Box(frame, native) switch { { IsValid: true } b => Fin.Succ(b), _ => Fin.Fail<Box>(state.InvalidResult()) }).ToEff()
                        from aabb in ((object)native).BoundsOf(op: state).ToEff()
                        from result in state.Accept(value: aabb.Volume / Math.Max(val1: obb.Volume, val2: RhinoMath.ZeroTolerance)).ToEff()
                        select result)
                : BoxTightnessKey.Unsupported<TGeometry, TOut>()),
        enclosingCase: static e => e.Shape.Switch(
            sphereCase: static _ => (typeof(TOut) == typeof(Sphere) && GeometryKernel.CanBound(typeof(TGeometry), includeSphere: true))
                ? Analysis.Operation<TGeometry, Sphere>.Build(
                    key: EnclosingSphereKey, state: EnclosingSphereKey,
                    evaluator: static (op, geometry) =>
                        from bbox in ((object)geometry).BoundsOf(op: op).ToEff()
                        from sphere in (new Sphere(center: bbox.Center, radius: bbox.Diagonal.Length * 0.5) switch {
                            { IsValid: true } s => Fin.Succ(s),
                            _ => Fin.Fail<Sphere>(op.InvalidResult()),
                        }).ToEff()
                        from result in op.Accept(value: sphere).ToEff()
                        select result).As<TGeometry, TOut>(key: EnclosingSphereKey)
                : EnclosingSphereKey.Unsupported<TGeometry, TOut>(),
            circleCase: static c => (typeof(TOut) == typeof(Circle) && GeometryKernel.CanReadVertices(type: typeof(TGeometry)))
                ? Analysis.Operation<TGeometry, Circle>.Build(
                    key: EnclosingCircleKey, requiresContext: true, state: (Key: EnclosingCircleKey, c.Plane),
                    evaluator: static (state, geometry) =>
                        from context in Env.Asks
                        from vertices in Analyze.VerticesOf(geometry: geometry, context: context, op: state.Key).ToEff()
                        from projected in Fin.Succ(toSeq(vertices.AsIterable().Select(point => state.Plane.ClosestPoint(testPoint: point)).ToArray())).ToEff()
                        from center in (projected.IsEmpty
                            ? Fin.Fail<Point3d>(state.Key.InvalidResult())
                            : Fin.Succ((Point3d)(projected.AsIterable().Aggregate(Vector3d.Zero, static (sum, point) => sum + (Vector3d)point) / projected.Count))).ToEff()
                        from circle in (new Circle(plane: new Plane(origin: center, normal: state.Plane.Normal), radius: projected.AsIterable().Max(point => point.DistanceTo(other: center))) switch {
                            { IsValid: true } fit => Fin.Succ(fit),
                            _ => Fin.Fail<Circle>(state.Key.InvalidResult()),
                        }).ToEff()
                        from result in state.Key.Accept(value: circle).ToEff()
                        select result).As<TGeometry, TOut>(key: EnclosingCircleKey)
                : EnclosingCircleKey.Unsupported<TGeometry, TOut>()));
    private static double AspectOf(Vector3d extents) {
        double ax = Math.Abs(extents.X), ay = Math.Abs(extents.Y), az = Math.Abs(extents.Z);
        return Math.Max(Math.Max(ax, ay), az) / Math.Max(Math.Min(Math.Min(ax, ay), az), RhinoMath.ZeroTolerance);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull => Aspect<Bounds, TGeometry, TOut>(aspect: aspect);
    internal static Operation<TGeometry, TOut> BoxMetric<TGeometry, TOut>(Op key, Func<BoundingBox, double> boundingBox, Func<Box, double> box) where TGeometry : notnull =>
        (typeof(TOut) == typeof(double), typeof(TGeometry)) switch {
            (true, Type geometry) when geometry == typeof(BoundingBox) => Operation<BoundingBox, double>.Build(
                key: key, state: (Key: key, Project: boundingBox),
                evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff()).As<TGeometry, TOut>(key: key),
            (true, Type geometry) when geometry == typeof(Box) => Operation<Box, double>.Build(
                key: key, state: (Key: key, Project: box),
                evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff()).As<TGeometry, TOut>(key: key),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
}
