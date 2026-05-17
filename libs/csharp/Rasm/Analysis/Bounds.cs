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
    public sealed record SphereCase(int Count = 64) : EnclosingShape;
    public sealed record CircleCase(Plane Plane, int Count = 64) : EnclosingShape;
    public sealed record CylinderCase(Vector3d Axis, int Count = 64) : EnclosingShape;
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
    public static Bounds EnclosingCylinder(Vector3d axis, int count = 64) => new EnclosingCase(Shape: new EnclosingShape.CylinderCase(Axis: axis, Count: count));
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
    internal static readonly Op EnclosingCylinderKey = Op.Of(name: "EnclosingCylinder");
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
            principalFrame: static _ => (typeof(TOut) == typeof(Box) && GeometryKernel.Can(type: typeof(TGeometry), predicate: static k => k.CanPrincipal))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, Box, Op>(
                    key: PrincipalKey, state: PrincipalKey, requirement: Requirement.Basic,
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
            tightness: static _ => (typeof(TOut) == typeof(double) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)) && GeometryKernel.Can(type: typeof(TGeometry), predicate: static k => k.CanPrincipal))
                ? Analyze.Native<TGeometry, TOut, GeometryBase, double, Op>(
                    key: BoxTightnessKey, state: BoxTightnessKey, requirement: Requirement.Basic,
                    project: static (state, native) =>
                        from context in Env.Asks
                        from frame in MassKind.PrincipalFrameOf(geometry: native, context: context, key: state).ToEff()
                        from obb in (new Box(frame, native) switch { { IsValid: true } b => Fin.Succ(b), _ => Fin.Fail<Box>(state.InvalidResult()) }).ToEff()
                        from aabb in ((object)native).BoundsOf(op: state).ToEff()
                        from result in (obb.Volume > RhinoMath.ZeroTolerance ? state.Accept(value: aabb.Volume / obb.Volume) : Fin.Fail<Seq<double>>(state.InvalidResult())).ToEff()
                        select result)
                : BoxTightnessKey.Unsupported<TGeometry, TOut>()),
        enclosingCase: static e => e.Shape.Switch(
            sphereCase: static s => (typeof(TOut) == typeof(Sphere) && GeometryKernel.CanBound(typeof(TGeometry), includeSphere: true))
                ? Analysis.Operation<TGeometry, Sphere>.Build(
                    key: EnclosingSphereKey, requiresContext: true, state: (Key: EnclosingSphereKey, s.Count),
                    evaluator: static (state, geometry) =>
                        from context in Env.Asks
                        from samples in EnclosingSamples(geometry: geometry, context: context, key: state.Key, count: state.Count).ToEff()
                        from result in RitterFit(samples: samples, key: state.Key, construct: static (c, r) => new Sphere(center: c, radius: r), isValid: static s => s.IsValid).ToEff()
                        from accepted in state.Key.Accept(value: result).ToEff()
                        select accepted).As<TGeometry, TOut>(key: EnclosingSphereKey)
                : EnclosingSphereKey.Unsupported<TGeometry, TOut>(),
            circleCase: static c => (typeof(TOut) == typeof(Circle) && GeometryKernel.Can(type: typeof(TGeometry), predicate: static k => k.CanReadVertices))
                ? Analysis.Operation<TGeometry, Circle>.Build(
                    key: EnclosingCircleKey, requiresContext: true, state: (Key: EnclosingCircleKey, c.Plane, c.Count),
                    evaluator: static (state, geometry) =>
                        from context in Env.Asks
                        from samples in EnclosingSamples(geometry: geometry, context: context, key: state.Key, count: state.Count).ToEff()
                        from projected in Fin.Succ(samples.Choose(p => state.Plane.ClosestParameter(testPoint: p, s: out double s, t: out double t) ? Some(new Point2d(x: s, y: t)) : Option<Point2d>.None)).ToEff()
                        from result in ((projected.Count, Circle.TrySmallestEnclosingCircle(points: projected.AsIterable(), tolerance: context.Absolute.Value, circle: out Circle circle, indicesOnCircle: out int[] _), circle) switch {
                            ( > 0, true, { IsValid: true } c) => Fin.Succ(new Circle(plane: new Plane(origin: state.Plane.PointAt(u: c.Center.X, v: c.Center.Y), xDirection: state.Plane.XAxis, yDirection: state.Plane.YAxis), radius: c.Radius)),
                            _ => Fin.Fail<Circle>(state.Key.InvalidResult()),
                        }).ToEff()
                        from accepted in state.Key.Accept(value: result).ToEff()
                        select accepted).As<TGeometry, TOut>(key: EnclosingCircleKey)
                : EnclosingCircleKey.Unsupported<TGeometry, TOut>(),
            cylinderCase: static cy => (typeof(TOut) == typeof(Cylinder) && GeometryKernel.CanBound(typeof(TGeometry), includeSphere: true))
                ? Analysis.Operation<TGeometry, Cylinder>.Build(
                    key: EnclosingCylinderKey, requiresContext: true, state: (Key: EnclosingCylinderKey, cy.Axis, cy.Count),
                    evaluator: static (state, geometry) =>
                        from context in Env.Asks
                        from axis in (state.Axis is { IsValid: true } a && !a.IsTiny() && a.Unitize() ? Fin.Succ(a) : Fin.Fail<Vector3d>(state.Key.InvalidInput())).ToEff()
                        from samples in EnclosingSamples(geometry: geometry, context: context, key: state.Key, count: state.Count).ToEff()
                        let plane = new Plane(origin: Point3d.Origin, normal: axis)
                        from projected in Fin.Succ(samples.Map(plane.ClosestPoint)).ToEff()
                        from disc in RitterFit(samples: projected, key: state.Key, construct: static (c, r) => (Center: c, Radius: r), isValid: static d => d.Radius >= 0.0).ToEff()
                        let extent = samples.Fold(initialState: (Min: double.PositiveInfinity, Max: double.NegativeInfinity), f: (s, p) => ((p - Point3d.Origin) * axis) switch { double d => (Min: Math.Min(val1: s.Min, val2: d), Max: Math.Max(val1: s.Max, val2: d)) })
                        from result in (new Cylinder(baseCircle: new Circle(plane: new Plane(origin: disc.Center + (axis * extent.Min), normal: axis), radius: disc.Radius), height: extent.Max - extent.Min) switch {
                            { IsValid: true } cyl => state.Key.Accept(value: cyl),
                            _ => Fin.Fail<Seq<Cylinder>>(state.Key.InvalidResult()),
                        }).ToEff()
                        select result).As<TGeometry, TOut>(key: EnclosingCylinderKey)
                : EnclosingCylinderKey.Unsupported<TGeometry, TOut>()));
    private static Fin<Seq<Point3d>> EnclosingSamples<TGeometry>(TGeometry geometry, Context context, Op key, int count) where TGeometry : notnull =>
        GeometryKernel.SamplePoints(source: geometry, count: count, context: context, key: key).Match(
            Succ: Fin.Succ,
            Fail: error => error is Fault.Unsupported ? Analyze.VerticesOf(geometry: geometry, context: context, op: key) : Fin.Fail<Seq<Point3d>>(error));
    private static Fin<T> RitterFit<T>(Seq<Point3d> samples, Op key, Func<Point3d, double, T> construct, Func<T, bool> isValid) =>
        (samples.Count switch {
            0 => Fin.Fail<(Point3d Center, double Radius)>(key.InvalidResult()),
            1 => Fin.Succ((Center: samples[0], Radius: 0.0)),
            _ => Fin.Succ(FarthestFrom(samples: samples, anchor: samples[0]) switch {
                Point3d p1 => FarthestFrom(samples: samples, anchor: p1) switch {
                    Point3d p2 => samples.Fold(
                        initialState: (Center: new Point3d(x: (p1.X + p2.X) * 0.5, y: (p1.Y + p2.Y) * 0.5, z: (p1.Z + p2.Z) * 0.5), Radius: p1.DistanceTo(other: p2) * 0.5),
                        f: static (state, p) => p.DistanceTo(other: state.Center) switch {
                            double d when d <= state.Radius => state,
                            double d => (Center: state.Center + ((p - state.Center) * ((d - state.Radius) * 0.5 / d)), Radius: (state.Radius + d) * 0.5),
                        }),
                },
            }),
        }).Bind(result => construct(arg1: result.Center, arg2: result.Radius) switch {
            T fit when isValid(arg: fit) => Fin.Succ(fit),
            _ => Fin.Fail<T>(key.InvalidResult()),
        });
    private static Point3d FarthestFrom(Seq<Point3d> samples, Point3d anchor) =>
        samples.Fold(
            initialState: (Best: anchor, Anchor: anchor, SqDist: 0.0),
            f: static (state, p) => ((p - state.Anchor) * (p - state.Anchor)) switch {
                double sq when sq > state.SqDist => state with { Best = p, SqDist = sq },
                _ => state,
            }).Best;
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
