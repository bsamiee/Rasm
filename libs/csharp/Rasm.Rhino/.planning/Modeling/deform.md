# [RASM_RHINO_MODELING_DEFORM]

`Deforms.Apply` owns value-semantic morph, control, unroll, squish, inverse mapping, and mesh unwrap over geometry custody. `DeformOp` admits drivers once, generated policies own every native carrier, disposable engines remain inside borrow windows, and `Built<DeformSlot>` carries products plus axis-labelled evidence. Document transforms and kernel DEC flattening remain separate owners.

## [01]-[INDEX]

- [02]-[MORPH]: `MorphKind`, `MorphBehavior`, `BendBehavior`, `FlowBehavior`, and `MorphExtent`.
- [03]-[FLATTEN]: `Following`, `UnrollLaw`, `SquishLaw`, `SquishSpring`, and `SquishFollowers`.
- [04]-[ALGEBRA]: `DeformSlot`, `DeformOp`, and `Deforms.Apply`.
- [05]-[EXECUTION]: native lifetime, source correlation, and geometry custody.

## [02]-[MORPH]

`MorphKind` is the sole deformation discriminant. `BendBehavior`, `FlowBehavior`, `TaperBehavior`, `MorphExtent`, and `MorphBehavior` replace positional booleans with bounded policy. Every concrete engine enters the same duplicate-morph-own kernel, including `MorphControl`.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BendBehavior {
    public static readonly BendBehavior Straight = new(key: 0);
    public static readonly BendBehavior Symmetric = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class FlowBehavior {
    public static readonly FlowBehavior ReverseBase = new(key: 0);
    public static readonly FlowBehavior ReverseTarget = new(key: 1);
    public static readonly FlowBehavior PreventStretching = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TaperBehavior {
    public static readonly TaperBehavior Flat = new(key: 0);
    public static readonly TaperBehavior Infinite = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class MorphBehavior {
    public static readonly MorphBehavior QuickPreview = new(key: 0);
    public static readonly MorphBehavior PreserveStructure = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class MorphExtent {
    public static readonly MorphExtent Bounded = new(key: 0, native: false);
    public static readonly MorphExtent Infinite = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MorphKind {
    private MorphKind() { }
    public sealed record Bend(Point3d Start, Point3d End, Point3d Through, Option<double> Angle, FrozenSet<BendBehavior> Behavior) : MorphKind;
    public sealed record Flow(GeometryHandle BaseRail, GeometryHandle TargetRail, FrozenSet<FlowBehavior> Behavior) : MorphKind;
    public sealed record Maelstrom(Plane Frame, double Radius0, double Radius1, double AngleRadians) : MorphKind;
    public sealed record Splop(Plane Frame, GeometryHandle Surface, Point2d SurfaceUv, double Scale = 1.0, double AngleRadians = 0.0) : MorphKind;
    public sealed record Sporph(GeometryHandle BaseSurface, GeometryHandle TargetSurface, Option<(Point2d BaseUv, Point2d TargetUv)> Alignment, Option<Vector3d> ConstrainNormal = default) : MorphKind;
    public sealed record StretchToLength(Point3d Start, Point3d End, double Length) : MorphKind;
    public sealed record StretchToPoint(Point3d Start, Point3d End, Point3d Point) : MorphKind;
    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, FrozenSet<TaperBehavior> Behavior) : MorphKind;
    public sealed record Twist(Line Axis, double AngleRadians, MorphExtent Extent) : MorphKind;
    public sealed record Cage(GeometryHandle Reference, GeometryHandle Target) : MorphKind;
    public sealed record Control(GeometryHandle OriginCurve, GeometryHandle TargetCurve) : MorphKind;

    internal Fin<GeometryHandle> Morph(GeometryHandle target, FrozenSet<MorphBehavior> tuning, Context context, Op key) =>
        Switch(
            (Target: target, Tuning: tuning, Domain: context, Op: key),
            bend: static (ctx, kind) => Deformed(
                mint: () => kind.Angle.Case switch {
                    double angle => new Morphs.BendSpaceMorph(
                        start: kind.Start, end: kind.End, point: kind.Through, angle: angle,
                        straight: kind.Behavior.Contains(BendBehavior.Straight),
                        symmetric: kind.Behavior.Contains(BendBehavior.Symmetric)),
                    _ => new Morphs.BendSpaceMorph(
                        start: kind.Start, end: kind.End, point: kind.Through,
                        straight: kind.Behavior.Contains(BendBehavior.Straight),
                        symmetric: kind.Behavior.Contains(BendBehavior.Symmetric)),
                },
                ctx: ctx),
            flow: static (ctx, kind) => ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.BaseRail, key: ctx.Op, body: baseRail =>
                ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.TargetRail, key: ctx.Op, body: targetRail => Deformed(
                    mint: () => new Morphs.FlowSpaceMorph(
                        curve0: baseRail, curve1: targetRail,
                        reverseCurve0: kind.Behavior.Contains(FlowBehavior.ReverseBase),
                        reverseCurve1: kind.Behavior.Contains(FlowBehavior.ReverseTarget),
                        preventStretching: kind.Behavior.Contains(FlowBehavior.PreventStretching)),
                    ctx: ctx))),
            maelstrom: static (ctx, kind) => Deformed(
                mint: () => new Morphs.MaelstromSpaceMorph(plane: kind.Frame, radius0: kind.Radius0, radius1: kind.Radius1, angle: kind.AngleRadians),
                ctx: ctx),
            splop: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.Surface, key: ctx.Op, body: surface => Deformed(
                mint: () => new Morphs.SplopSpaceMorph(plane: kind.Frame, surface: surface, surfaceParam: kind.SurfaceUv, scale: kind.Scale, angle: kind.AngleRadians),
                ctx: ctx)),
            sporph: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.BaseSurface, key: ctx.Op, body: baseSurface =>
                ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.TargetSurface, key: ctx.Op, body: targetSurface => Deformed(
                    mint: () => {
                        Morphs.SporphSpaceMorph engine = kind.Alignment.Case switch {
                            (Point2d baseUv, Point2d targetUv) => new(surface0: baseSurface, surface1: targetSurface, surface0Param: baseUv, surface1Param: targetUv),
                            _ => new(surface0: baseSurface, surface1: targetSurface),
                        };
                        _ = kind.ConstrainNormal.Iter(normal => engine.ConstrainNormal = normal);
                        return engine;
                    },
                    ctx: ctx))),
            stretchToLength: static (ctx, kind) => Deformed(
                mint: () => new Morphs.StretchSpaceMorph(start: kind.Start, end: kind.End, length: kind.Length),
                ctx: ctx),
            stretchToPoint: static (ctx, kind) => Deformed(
                mint: () => new Morphs.StretchSpaceMorph(start: kind.Start, end: kind.End, point: kind.Point),
                ctx: ctx),
            taper: static (ctx, kind) => Deformed(
                mint: () => new Morphs.TaperSpaceMorph(
                    start: kind.Start, end: kind.End, startRadius: kind.StartRadius, endRadius: kind.EndRadius,
                    bFlat: kind.Behavior.Contains(TaperBehavior.Flat),
                    infiniteTaper: kind.Behavior.Contains(TaperBehavior.Infinite)),
                ctx: ctx),
            twist: static (ctx, kind) => Deformed(
                mint: () => new Morphs.TwistSpaceMorph {
                    TwistAxis = kind.Axis,
                    TwistAngleRadians = kind.AngleRadians,
                    InfiniteTwist = kind.Extent.Native,
                },
                ctx: ctx),
            cage: static (ctx, kind) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Reference, key: ctx.Op, body: reference =>
                ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Target, key: ctx.Op, body: cageTarget => Deformed(
                    mint: () => new Morphs.MeshCageMorph(referenceMesh: reference, targetMesh: cageTarget),
                    ctx: ctx))),
            control: static (ctx, kind) => ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.OriginCurve, key: ctx.Op, body: origin =>
                ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.TargetCurve, key: ctx.Op, body: driven =>
                    ctx.Op.Catch(() => {
                        using MorphControl driver = new(originCurve: origin, targetCurve: driven);
                        driver.SpaceMorphTolerance = ctx.Domain.Absolute.Value;
                        driver.QuickPreview = ctx.Tuning.Contains(MorphBehavior.QuickPreview);
                        driver.PreserveStructure = ctx.Tuning.Contains(MorphBehavior.PreserveStructure);
                        return Duplicated(target: ctx.Target, morph: working => ctx.Op.Confirm(success: driver.Morph(geometry: working)), key: ctx.Op);
                    }))));

    internal bool Admissible(FrozenSet<MorphBehavior>? tuning) =>
        Deforms.Declared(values: tuning, rows: MorphBehavior.Items) && Switch(
            bend: static kind => kind.Start.IsValid && kind.End.IsValid && kind.Through.IsValid
                && kind.Angle.ForAll(double.IsFinite)
                && Deforms.Declared(values: kind.Behavior, rows: BendBehavior.Items),
            flow: static kind => kind.BaseRail is not null && kind.TargetRail is not null
                && Deforms.Declared(values: kind.Behavior, rows: FlowBehavior.Items),
            maelstrom: static kind => kind.Frame.IsValid && double.IsFinite(kind.Radius0)
                && double.IsFinite(kind.Radius1) && double.IsFinite(kind.AngleRadians),
            splop: static kind => kind.Frame.IsValid && kind.Surface is not null && kind.SurfaceUv.IsValid
                && double.IsFinite(kind.Scale) && kind.Scale != 0.0 && double.IsFinite(kind.AngleRadians),
            sporph: static kind => kind.BaseSurface is not null && kind.TargetSurface is not null
                && kind.Alignment.ForAll(static pair => pair.BaseUv.IsValid && pair.TargetUv.IsValid)
                && kind.ConstrainNormal.ForAll(static normal => normal.IsValid && !normal.IsZero),
            stretchToLength: static kind => kind.Start.IsValid && kind.End.IsValid
                && double.IsFinite(kind.Length) && kind.Length > 0.0,
            stretchToPoint: static kind => kind.Start.IsValid && kind.End.IsValid && kind.Point.IsValid,
            taper: static kind => kind.Start.IsValid && kind.End.IsValid
                && double.IsFinite(kind.StartRadius) && kind.StartRadius >= 0.0
                && double.IsFinite(kind.EndRadius) && kind.EndRadius >= 0.0
                && Deforms.Declared(values: kind.Behavior, rows: TaperBehavior.Items),
            twist: static kind => kind.Axis.IsValid && double.IsFinite(kind.AngleRadians) && kind.Extent is not null,
            cage: static kind => kind.Reference is not null && kind.Target is not null,
            control: static kind => kind.OriginCurve is not null && kind.TargetCurve is not null);

    private static Fin<GeometryHandle> Deformed<TMorph>(Func<TMorph> mint, (GeometryHandle Target, FrozenSet<MorphBehavior> Tuning, Context Domain, Op Op) ctx)
        where TMorph : SpaceMorph, IDisposable =>
        ctx.Op.Catch(() => {
            using TMorph active = mint();
            active.Tolerance = ctx.Domain.Absolute.Value;
            active.QuickPreview = ctx.Tuning.Contains(MorphBehavior.QuickPreview);
            active.PreserveStructure = ctx.Tuning.Contains(MorphBehavior.PreserveStructure);
            return Duplicated(target: ctx.Target, morph: working => ctx.Op.Confirm(success: active.Morph(geometry: working)), key: ctx.Op);
        });

    private static Fin<GeometryHandle> Duplicated(GeometryHandle target, Func<GeometryBase, Fin<Unit>> morph, Op key) =>
        ModelGate.Borrow<GeometryBase, GeometryHandle>(handle: target, key: key, body: source =>
            from _ in guard(SpaceMorph.IsMorphable(geometry: source), key.Unsupported(geometryType: source.GetType(), outputType: typeof(GeometryBase)))
            from working in key.Catch(() => Optional(source.Duplicate()).ToFin(Fail: key.InvalidResult()))
            from morphed in morph(arg: working).Match(
                Succ: _ => ModelGate.Own(built: working, key: key),
                Fail: error => {
                    working.Dispose();
                    return Fin.Fail<GeometryHandle>(error: error);
                })
            select morphed);
}

```

## [03]-[FLATTEN]

`Following`, `UnrollLaw`, `SquishLaw`, `SquishSpring`, and `SquishFollowers` validate before native engines exist. `SquishBehavior` replaces topology, mapping, boundary, and diagnostic booleans with one policy value. `FollowingGeometryIndex` preserves source correlation for carried curves.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SquishBehavior {
    public static readonly SquishBehavior PreserveBoundary = new(key: 0);
    public static readonly SquishBehavior PreserveTopology = new(key: 1);
    public static readonly SquishBehavior SaveMapping = new(key: 2);
    public static readonly SquishBehavior CaptureNets = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class UnrollOutput {
    public static readonly UnrollOutput Joined = new(key: 0, native: false);
    public static readonly UnrollOutput Exploded = new(key: 1, native: true);

    internal bool Native { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Following {
    public Seq<GeometryHandle> Curves { get; }
    public Seq<Point3d> Points { get; }
    public Seq<(Point3d Location, string Text)> Dots { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<GeometryHandle> curves,
        ref Seq<Point3d> points,
        ref Seq<(Point3d Location, string Text)> dots) {
        if (!IsAdmissible(curves: curves, points: points, dots: dots)) {
            validationError = new ValidationError("Following geometry requires live curve handles, valid points, and located dot labels with text.");
        }
    }

    internal bool Admissible => IsAdmissible(curves: Curves, points: Points, dots: Dots);

    private static bool IsAdmissible(
        Seq<GeometryHandle> curves,
        Seq<Point3d> points,
        Seq<(Point3d Location, string Text)> dots) =>
        curves.ForAll(static handle => handle is not null)
        && points.ForAll(static point => point.IsValid)
        && dots.ForAll(static row => row.Location.IsValid && !string.IsNullOrWhiteSpace(row.Text));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishFollowers {
    public Seq<GeometryHandle> Curves { get; }
    public Seq<GeometryHandle> Dots { get; }
    public Seq<Point3d> Points { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<GeometryHandle> curves,
        ref Seq<GeometryHandle> dots,
        ref Seq<Point3d> points) {
        if (!IsAdmissible(curves: curves, dots: dots, points: points)) {
            validationError = new ValidationError("Squish followers require live curve and dot handles plus valid points.");
        }
    }

    internal bool Admissible => IsAdmissible(curves: Curves, dots: Dots, points: Points);

    private static bool IsAdmissible(Seq<GeometryHandle> curves, Seq<GeometryHandle> dots, Seq<Point3d> points) =>
        curves.ForAll(static handle => handle is not null)
        && dots.ForAll(static handle => handle is not null)
        && points.ForAll(static point => point.IsValid);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct UnrollLaw {
    public UnrollOutput Output { get; }
    public double Spacing { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UnrollOutput output,
        ref double spacing) {
        if (!IsAdmissible(output: output, spacing: spacing)) {
            validationError = new ValidationError("Unroll output must be declared, and spacing must be finite and non-negative.");
        }
    }

    internal bool Admissible => IsAdmissible(output: Output, spacing: Spacing);

    private static bool IsAdmissible(UnrollOutput? output, double spacing) =>
        output is not null && double.IsFinite(spacing) && spacing >= 0.0;
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishSpring {
    public double Boundary { get; }
    public double Deformation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double boundary,
        ref double deformation) {
        if (!double.IsFinite(boundary) || boundary < 0.0 || !double.IsFinite(deformation) || deformation < 0.0) {
            validationError = new ValidationError("Squish spring constants must be finite and non-negative.");
        }
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishLaw {
    public SquishFlatteningAlgorithm Algorithm { get; }
    public SquishDeformation Mode { get; }
    public FrozenSet<SquishBehavior> Behavior { get; }
    public double BoundaryStretch { get; }
    public double BoundaryCompress { get; }
    public double InteriorStretch { get; }
    public double InteriorCompress { get; }
    public double AbsoluteLimit { get; }
    public Option<SquishSpring> Spring { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SquishFlatteningAlgorithm algorithm,
        ref SquishDeformation mode,
        ref FrozenSet<SquishBehavior> behavior,
        ref double boundaryStretch,
        ref double boundaryCompress,
        ref double interiorStretch,
        ref double interiorCompress,
        ref double absoluteLimit,
        ref Option<SquishSpring> spring) {
        if (!IsAdmissible(
            algorithm: algorithm,
            mode: mode,
            behavior: behavior,
            boundaryStretch: boundaryStretch,
            boundaryCompress: boundaryCompress,
            interiorStretch: interiorStretch,
            interiorCompress: interiorCompress,
            absoluteLimit: absoluteLimit)) {
            validationError = new ValidationError("Squish policies must be declared, weights must be finite and non-negative, and the limit must be finite and positive.");
        }
    }

    internal bool Admissible => IsAdmissible(
        algorithm: Algorithm,
        mode: Mode,
        behavior: Behavior,
        boundaryStretch: BoundaryStretch,
        boundaryCompress: BoundaryCompress,
        interiorStretch: InteriorStretch,
        interiorCompress: InteriorCompress,
        absoluteLimit: AbsoluteLimit);

    private static bool IsAdmissible(
        SquishFlatteningAlgorithm algorithm,
        SquishDeformation mode,
        FrozenSet<SquishBehavior>? behavior,
        double boundaryStretch,
        double boundaryCompress,
        double interiorStretch,
        double interiorCompress,
        double absoluteLimit) =>
        Enum.IsDefined(algorithm)
        && Enum.IsDefined(mode)
        && Deforms.Declared(values: behavior, rows: SquishBehavior.Items)
        && double.IsFinite(boundaryStretch) && boundaryStretch >= 0.0
        && double.IsFinite(boundaryCompress) && boundaryCompress >= 0.0
        && double.IsFinite(interiorStretch) && interiorStretch >= 0.0
        && double.IsFinite(interiorCompress) && interiorCompress >= 0.0
        && double.IsFinite(absoluteLimit) && absoluteLimit > 0.0;

    internal Fin<SquishParameters> Rig(Op key) =>
        key.Catch(() => {
            SquishParameters parameters = SquishParameters.Default;
            parameters.Algorithm = Algorithm;
            parameters.PreserveTopology = Behavior.Contains(SquishBehavior.PreserveTopology);
            parameters.SaveMapping = Behavior.Contains(SquishBehavior.SaveMapping);
            parameters.AbsoluteLimit = AbsoluteLimit;
            parameters.SetDeformation(
                deformation: Mode,
                bPreserveBoundary: Behavior.Contains(SquishBehavior.PreserveBoundary),
                boundaryStretchConstant: BoundaryStretch,
                boundaryCompressConstant: BoundaryCompress,
                interiorStretchConstant: InteriorStretch,
                interiorCompressConstant: InteriorCompress);
            _ = Spring.Iter(spring => parameters.SetSpringConstants(
                boundaryBias: spring.Boundary,
                deformationBias: spring.Deformation));
            return Fin.Succ(value: parameters);
        });
}
```

## [04]-[ALGEBRA]

`DeformOp` is the sole operation algebra. Geometry products cross through `ModelGate`, while unrolled points, source indices, squished points, topology diagnostics, and labelled spring axes remain receipt evidence. `MeshUnwrapper`, `Unroller`, `Squisher`, and every concrete morph stay inside their consuming arm.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DeformSlot {
    public static readonly DeformSlot Morphed = new(key: 0);
    public static readonly DeformSlot Unrolled = new(key: 1);
    public static readonly DeformSlot Followed = new(key: 2);
    public static readonly DeformSlot Squished = new(key: 3);
    public static readonly DeformSlot MarkMapped = new(key: 4);
    public static readonly DeformSlot Netted = new(key: 5);
    public static readonly DeformSlot Restored = new(key: 6);
    public static readonly DeformSlot Unwrapped = new(key: 7);
    public static readonly DeformSlot Mesh2dEdges = new(key: 8);
    public static readonly DeformSlot Mesh3dEdges = new(key: 9);
    public static readonly DeformSlot AreaConstraints = new(key: 10);
    public static readonly DeformSlot BoundarySpring = new(key: 11);
    public static readonly DeformSlot DeformationSpring = new(key: 12);
    public static readonly DeformSlot FollowingIndex = new(key: 13);
    public static readonly DeformSlot SquishCurves = new(key: 14);
    public static readonly DeformSlot SquishDots = new(key: 15);
    public static readonly DeformSlot SquishPoints = new(key: 16);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeformOp {
    private DeformOp() { }
    public sealed record Morph(GeometryHandle Target, MorphKind Kind, FrozenSet<MorphBehavior> Behavior) : DeformOp;
    public sealed record Unroll(GeometryHandle Target, Following Followers, UnrollLaw Law) : DeformOp;
    public sealed record Squish(GeometryHandle Target, SquishLaw Law, Seq<GeometryHandle> Marks, SquishFollowers Followers) : DeformOp;
    public sealed record SquishBack(GeometryHandle Pattern, Seq<GeometryHandle> Marks) : DeformOp;
    public sealed record Unwrap(Seq<GeometryHandle> Meshes, MeshUnwrapMethod Method, Option<Plane> Symmetry = default) : DeformOp;

    internal Fin<DeformOp> Admitted(Op key) =>
        guard(this switch {
            Morph edit => edit.Target is not null && edit.Kind is not null && edit.Kind.Admissible(tuning: edit.Behavior),
            Unroll edit => edit.Target is not null && edit.Followers.Admissible && edit.Law.Admissible,
            Squish edit => edit.Target is not null && edit.Law.Admissible
                && Handles(edit.Marks, allowEmpty: true) && edit.Followers.Admissible,
            SquishBack edit => edit.Pattern is not null && Handles(edit.Marks),
            Unwrap edit => Handles(edit.Meshes) && Enum.IsDefined(edit.Method)
                && edit.Symmetry.ForAll(static symmetry => symmetry.IsValid),
            _ => false,
        }, key.InvalidInput()).ToFin().Map(_ => this);

    private static bool Handles(Seq<GeometryHandle> handles, bool allowEmpty = false) =>
        (allowEmpty || !handles.IsEmpty) && handles.ForAll(static handle => handle is not null);

    internal Fin<Built<DeformSlot>> Apply(Context domain) =>
        Switch(
            domain,
            morph: static (model, edit) => {
                Op op = Op.Of(name: nameof(Morph));
                return edit.Kind.Morph(target: edit.Target, tuning: edit.Behavior, context: model, key: op)
                    .Map(product => Built<DeformSlot>.Of(
                        operation: op,
                        Products: Seq(product),
                        Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Morphed, body: new BuildBody.Tally(Count: 1))));
            },
            unroll: static (model, edit) => {
                Op op = Op.Of(name: nameof(Unroll));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.BorrowMany<Curve, Built<DeformSlot>>(handles: edit.Followers.Curves, key: op, allowEmpty: true, body: followers =>
                        op.Catch(() => {
                            Fin<Unroller> admitted = source switch {
                                Brep brep => Fin.Succ(value: new Unroller(brep: brep)),
                                Surface surface => Fin.Succ(value: new Unroller(surface: surface)),
                                _ => Fin.Fail<Unroller>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Unroller))),
                            };
                            return admitted.Bind(active => {
                                active.ExplodeOutput = edit.Law.Output.Native;
                                active.ExplodeSpacing = edit.Law.Spacing;
                                active.AbsoluteTolerance = model.Absolute.Value;
                                active.RelativeTolerance = model.Fractional;
                                _ = Op.SideWhen(!followers.IsEmpty, () => active.AddFollowingGeometry(curves: followers.AsIterable()));
                                _ = Op.SideWhen(!edit.Followers.Points.IsEmpty, () => active.AddFollowingGeometry(points: edit.Followers.Points.AsIterable()));
                                _ = edit.Followers.Dots.Iter(row => active.AddFollowingGeometry(dotLocation: row.Location, dotText: row.Text));
                                Brep[] flatBreps = active.PerformUnroll(
                                    unrolledCurves: out Curve[] flatCurves,
                                    unrolledPoints: out Point3d[] flatPoints,
                                    unrolledDots: out TextDot[] flatDots);
                                Seq<Brep> breps = toSeq(flatBreps ?? []);
                                Seq<Curve> curves = toSeq(flatCurves ?? []);
                                Seq<Point3d> points = toSeq(flatPoints ?? []);
                                Seq<TextDot> dots = toSeq(flatDots ?? []);
                                Seq<int> sourceIndices = curves.Map(active.FollowingGeometryIndex);
                                return
                                    from flat in ModelGate.OwnMany(built: breps, key: op)
                                        .Rollback([.. dots])
                                    from carried in ModelGate.OwnMany(built: curves, key: op, allowEmpty: true)
                                        .Rollback(flat)
                                        .Rollback([.. dots])
                                    let rows = dots.Map(static dot => (dot.Point, dot.Text))
                                    let _ = dots.Iter(static dot => dot.Dispose())
                                    select Built<DeformSlot>.Of(
                                        operation: op,
                                        Products: flat + carried,
                                        Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Unrolled, body: new BuildBody.Tally(Count: flat.Count))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Tally(Count: carried.Count))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.FollowingIndex, body: new BuildBody.Components(Indices: sourceIndices))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Marks(Points: points))
                                            + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Followed, body: new BuildBody.Labels(Rows: rows)));
                            });
                        })));
            },
            squish: static (_, edit) => {
                Op op = Op.Of(name: nameof(Squish));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Target, key: op, body: source =>
                    ModelGate.BorrowMany<GeometryBase, Built<DeformSlot>>(handles: edit.Marks, key: op, allowEmpty: true, body: marks =>
                        from parameters in edit.Law.Rig(key: op)
                        from flattened in op.Catch(() => {
                            using SquishParameters sp = parameters;
                            using Squisher engine = new();
                            _ = sp.GetSpringConstants(boundaryBias: out double boundaryBias, deformationBias: out double deformationBias);
                            System.Collections.Generic.List<GeometryBase> mapped = [];
                            Fin<GeometryHandle> flat = source switch {
                                Surface surface => ModelGate.Own(
                                    built: engine.SquishSurface(sp: sp, surface: surface, marks: marks.AsIterable(), squished_marks_out: mapped), key: op),
                                Mesh mesh => ModelGate.Own(
                                    built: engine.SquishMesh(sp: sp, mesh3d: mesh, marks: marks.AsIterable(), squished_marks_out: mapped), key: op),
                                _ => Fin.Fail<GeometryHandle>(error: op.Unsupported(geometryType: source.GetType(), outputType: typeof(Squisher))),
                            };
                            return flat.Rollback([.. mapped]).Bind(primary => (
                                from crossed in ModelGate.OwnMany(built: mapped, key: op, allowEmpty: true)
                                from directCurves in ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(
                                    handles: edit.Followers.Curves,
                                    key: op,
                                    allowEmpty: true,
                                    body: curves => ModelGate.OwnEach(
                                        sources: curves,
                                        key: op,
                                        run: engine.SquishCurve,
                                        allowEmpty: true)).Rollback(crossed)
                                from directDots in ModelGate.BorrowMany<TextDot, Seq<GeometryHandle>>(
                                    handles: edit.Followers.Dots,
                                    key: op,
                                    allowEmpty: true,
                                    body: dots => ModelGate.OwnEach(
                                        sources: dots,
                                        key: op,
                                        run: engine.SquishTextDot,
                                        allowEmpty: true)).Rollback(crossed + directCurves)
                                from directPoints in edit.Followers.Points.TraverseM(point =>
                                        op.Catch(() => engine.SquishPoint(point: point, squishedPoint: out Point3d squished)
                                            ? Fin.Succ(value: squished)
                                            : Fin.Fail<Point3d>(error: op.InvalidResult()))).As()
                                    .Rollback(crossed + directCurves + directDots)
                                from nets in (edit.Law.Behavior.Contains(SquishBehavior.CaptureNets)
                                    ? from flat2d in ModelGate.Own(built: engine.Get2dMesh(), key: op)
                                      from flat3d in ModelGate.Own(built: engine.Get3dMesh(), key: op).Rollback(flat2d)
                                      select Seq(flat2d, flat3d)
                                    : Fin.Succ(value: Seq<GeometryHandle>()))
                                    .Rollback(crossed + directCurves + directDots)
                                select Built<DeformSlot>.Of(
                                    operation: op,
                                    Products: Seq(primary) + crossed + directCurves + directDots + nets,
                                    Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Squished, body: new BuildBody.Tally(Count: 1))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.MarkMapped, body: new BuildBody.Tally(Count: crossed.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.SquishCurves, body: new BuildBody.Tally(Count: directCurves.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.SquishDots, body: new BuildBody.Tally(Count: directDots.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.SquishPoints, body: new BuildBody.Marks(Points: directPoints))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Netted, body: new BuildBody.Tally(Count: nets.Count))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Mesh2dEdges, body: new BuildBody.Segments(Lines: toSeq(engine.GetLengthConstrained2dLines() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Mesh3dEdges, body: new BuildBody.Segments(Lines: toSeq(engine.GetLengthConstrained3dLines() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.AreaConstraints, body: new BuildBody.Faces(Rows: toSeq(engine.GetAreaConstrainedTrianglesIndices() ?? [])))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.BoundarySpring, body: new BuildBody.Measure(Value: boundaryBias))
                                        + BuildReceipt<DeformSlot>.Of(slot: DeformSlot.DeformationSpring, body: new BuildBody.Measure(Value: deformationBias))))
                                .Rollback(primary));
                        })
                        select flattened));
            },
            squishBack: static (_, edit) => {
                Op op = Op.Of(name: nameof(SquishBack));
                return ModelGate.Borrow<GeometryBase, Built<DeformSlot>>(handle: edit.Pattern, key: op, body: pattern =>
                    ModelGate.BorrowMany<GeometryBase, Built<DeformSlot>>(handles: edit.Marks, key: op, body: marks =>
                        from _ in op.Confirm(success: Squisher.Is2dPatternSquished(geometry: pattern))
                        from restored in op.Catch(() => ModelGate.OwnMany(
                            built: Squisher.SquishBack2dMarks(squishedGeometry: pattern, marks: marks.AsIterable()), key: op))
                        select Built<DeformSlot>.Of(
                            operation: op,
                            Products: restored,
                            Evidence: BuildReceipt<DeformSlot>.Of(slot: DeformSlot.Restored, body: new BuildBody.Tally(Count: restored.Count)))));
            },
            unwrap: static (_, edit) => {
                Op op = Op.Of(name: nameof(Unwrap));
                return ModelGate.BorrowMany<Mesh, Built<DeformSlot>>(handles: edit.Meshes, key: op, body: sources =>
                    from unwrapped in sources.Fold(Fin.Succ(value: Seq<Mesh>()), (state, source) => state.Bind(held =>
                            op.Catch(() => Optional(source.Duplicate() as Mesh).ToFin(Fail: op.InvalidResult())).Match(
                                Succ: copy => Fin.Succ(value: held.Add(value: copy)),
                                Fail: error => {
                                    _ = held.Iter(static mesh => mesh.Dispose());
                                    return Fin.Fail<Seq<Mesh>>(error: error);
                                })))
                        .Bind(working => op.Catch(() => {
                            using MeshUnwrapper engine = new(meshes: working.AsIterable());
                            _ = edit.Symmetry.Iter(symmetry => engine.SymmetryPlane = symmetry);
                            return op.Confirm(success: engine.Unwrap(method: edit.Method))
                                .Bind(_ => ModelGate.Many(op, DeformSlot.Unwrapped, () => working.AsEnumerable()));
                        }).Rollback([.. working]))
                    select unwrapped);
            });
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Deforms {
    internal static bool Declared<T>(FrozenSet<T>? values, IReadOnlyList<T> rows) where T : class =>
        values is not null && values.All(row => row is not null && rows.Contains(row));

    public static Fin<Built<DeformSlot>> Apply(Context context, params ReadOnlySpan<DeformOp> operations) {
        Op op = Op.Of();
        Seq<DeformOp> captured = toSeq(operations.ToArray());
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from _ in guard(!captured.IsEmpty, op.InvalidInput())
               from admitted in captured.TraverseM(operation =>
                       Optional(operation).ToFin(Fail: op.InvalidInput())
                           .Bind(active => active.Admitted(key: op)))
                   .As()
               from built in ModelGate.Folded(
                   context: domain,
                   operations: admitted,
                   apply: static (operation, model) => operation.Apply(domain: model))
               select built;
    }
}
```

## [05]-[EXECUTION]

`Deforms.Apply` admits every operation payload before `ModelGate.Folded`. Morph and unwrap operations duplicate before mutation; unroll and squish engines die after all primary, carried, mapped, and diagnostic projections are detached.

`FollowingGeometryIndex` records one source position per normalized flattened curve. `ModelGate.OwnEach` owns each direct squish result before producing the next, so a later refusal releases the complete prefix.

`BoundarySpring` and `DeformationSpring` preserve the two `SquishParameters.GetSpringConstants` axes as distinct facts rather than position-dependent repeated measures.
