# [RASM_RHINO_MODELING_DEFORM]

`Deforms.Build` owns value-semantic morph, control, unroll, squish, inverse mapping, and mesh unwrap over geometry custody. `DeformOp` admits drivers once through the spine's `ModelClaim` fold, generated policies own every native carrier, disposable engines ride `Lease` windows inside their borrow scope, and every operation returns its owned geometry handles directly. Document transforms and kernel DEC flattening remain separate owners.

## [01]-[INDEX]

- [02]-[MORPH]: `BendBehavior`, `FlowBehavior`, `TaperBehavior`, `MorphBehavior`, `MorphExtent`, `MorphKind` — the deformation discriminant and its grant vocabularies.
- [03]-[FLATTEN]: `SquishBehavior`, `UnrollOutput`, `UnrollFollowers`, `SquishFollowers`, `UnrollLaw`, `SquishSpring`, `SquishLaw` — the flattener policies and their carried geometry.
- [04]-[ALGEBRA]: `DeformOp` and the `Deforms.Build` entry.

## [02]-[MORPH]

- Owner: `MorphKind` `[Union]` — the sole deformation discriminant, eleven engine forms behind one mint; `BendBehavior`, `FlowBehavior`, `TaperBehavior`, and `MorphBehavior` — the grant vocabularies replacing every positional host bool; `MorphExtent` — the infinite-twist row.
- Law: every concrete engine enters the same duplicate-morph-own kernel — `Deformed` mints the engine inside `Try.lift` under a `using`, seats tolerance and the two tuning grants, and hands the morph body to `Duplicated`; `MorphControl` is not a `SpaceMorph`, so its arm builds the driver and re-enters `Duplicated` directly, and that carve is the only reason a second mint exists.
- Law: morphability gates before duplication — `Duplicated` refuses a non-morphable source through `Unsupported` with both types named, duplicates only after that verdict, and disposes the working copy on the morph's failure edge, so a refused morph never strands a live duplicate.
- Law: grants are a `CapabilitySet`, never a `FrozenSet` — membership outside the roster is unrepresentable rather than probed at admission, and the runtime `Declared` membership check three Modeling pages spelled by hand deletes with it. `CapabilitySet`'s `[UnorderedEquality]` comparison also fixes the reference-equality reading a raw frozen set gives every value object holding one.
- Law: a two-state modality carrying a HOST projection column is a row — `MorphExtent` renders `InfiniteTwist`, so the bit lives on the row that names both corners and a caller cannot pass a bare `true` whose meaning the call site alone carries. Grants with no host column and no correlated partner stay `CapabilitySet` members; a solitary independent bit stays a named bool on its owning case.
- Growth: a new morph engine is one `MorphKind` case with its mint; a new grant is one vocabulary row.
- Packages: RhinoCommon deform (`.api/api-rhinocommon-deform.md` — `Morphs.BendSpaceMorph`/`FlowSpaceMorph`/`MaelstromSpaceMorph`/`SplopSpaceMorph`/`SporphSpaceMorph`/`StretchSpaceMorph`/`TaperSpaceMorph`/`TwistSpaceMorph`/`MeshCageMorph`, `MorphControl`, `SpaceMorph.IsMorphable`), kernel `Domain/results` (`Fin`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BendBehavior : ICapability<BendBehavior> {
    public static readonly BendBehavior Straight = new(key: "straight");
    public static readonly BendBehavior Symmetric = new(key: "symmetric");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlowBehavior : ICapability<FlowBehavior> {
    public static readonly FlowBehavior ReverseBase = new(key: "reverse-base");
    public static readonly FlowBehavior ReverseTarget = new(key: "reverse-target");
    public static readonly FlowBehavior PreventStretching = new(key: "prevent-stretching");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TaperBehavior : ICapability<TaperBehavior> {
    public static readonly TaperBehavior Flat = new(key: "flat");
    public static readonly TaperBehavior Infinite = new(key: "infinite");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MorphBehavior : ICapability<MorphBehavior> {
    public static readonly MorphBehavior QuickPreview = new(key: "quick-preview");
    public static readonly MorphBehavior PreserveStructure = new(key: "preserve-structure");
}

[SmartEnum<int>]
public sealed partial class MorphExtent {
    public static readonly MorphExtent Bounded = new(key: 0, native: false);
    public static readonly MorphExtent Infinite = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MorphKind : IValidityEvidence {
    private MorphKind() { }
    public sealed record Bend(Point3d Start, Point3d End, Point3d Through, Option<double> Angle, CapabilitySet<BendBehavior> Behavior) : MorphKind;
    public sealed record Flow(GeometryHandle BaseRail, GeometryHandle TargetRail, CapabilitySet<FlowBehavior> Behavior) : MorphKind;
    public sealed record Maelstrom(Plane Frame, double Radius0, double Radius1, double AngleRadians) : MorphKind;
    public sealed record Splop(Plane Frame, GeometryHandle Surface, Point2d SurfaceUv, double Scale = 1.0, double AngleRadians = 0.0) : MorphKind;
    public sealed record Sporph(GeometryHandle BaseSurface, GeometryHandle TargetSurface, Option<(Point2d BaseUv, Point2d TargetUv)> Alignment, Option<Vector3d> ConstrainNormal = default) : MorphKind;
    public sealed record StretchToLength(Point3d Start, Point3d End, double Length) : MorphKind;
    public sealed record StretchToPoint(Point3d Start, Point3d End, Point3d Point) : MorphKind;
    public sealed record Taper(Point3d Start, Point3d End, double StartRadius, double EndRadius, CapabilitySet<TaperBehavior> Behavior) : MorphKind;
    public sealed record Twist(Line Axis, double AngleRadians, MorphExtent Extent) : MorphKind;
    public sealed record Cage(GeometryHandle Reference, GeometryHandle Target) : MorphKind;
    public sealed record Control(GeometryHandle OriginCurve, GeometryHandle TargetCurve) : MorphKind;

    public bool IsValid => Switch(
        bend: static kind => ValidityClaim.All(
            ValidityClaim.Finite(value: kind.Start), ValidityClaim.Finite(value: kind.End), ValidityClaim.Finite(value: kind.Through),
            ValidityClaim.WhenPresent(facet: kind.Angle, claim: static angle => ValidityClaim.Finite(value: angle))),
        flow: static kind => ValidityClaim.All(
            ModelClaim.Handle(handle: kind.BaseRail), ModelClaim.Handle(handle: kind.TargetRail)),
        maelstrom: static kind => ValidityClaim.All(
            kind.Frame.IsValid, ValidityClaim.Finite(value: kind.Radius0),
            ValidityClaim.Finite(value: kind.Radius1), ValidityClaim.Finite(value: kind.AngleRadians)),
        splop: static kind => ValidityClaim.All(
            kind.Frame.IsValid, ModelClaim.Handle(handle: kind.Surface), kind.SurfaceUv.IsValid,
            ValidityClaim.Finite(value: kind.Scale), kind.Scale != 0.0, ValidityClaim.Finite(value: kind.AngleRadians)),
        sporph: static kind => ValidityClaim.All(
            ModelClaim.Handle(handle: kind.BaseSurface), ModelClaim.Handle(handle: kind.TargetSurface),
            ValidityClaim.WhenPresent(facet: kind.Alignment, claim: static pair => ValidityClaim.All(pair.BaseUv.IsValid, pair.TargetUv.IsValid)),
            ValidityClaim.WhenPresent(facet: kind.ConstrainNormal, claim: static normal => ValidityClaim.Direction(value: normal))),
        stretchToLength: static kind => ValidityClaim.All(
            ValidityClaim.Finite(value: kind.Start), ValidityClaim.Finite(value: kind.End),
            ValidityClaim.Positive(value: kind.Length)),
        stretchToPoint: static kind => ValidityClaim.All(
            ValidityClaim.Finite(value: kind.Start), ValidityClaim.Finite(value: kind.End), ValidityClaim.Finite(value: kind.Point)),
        taper: static kind => ValidityClaim.All(
            ValidityClaim.Finite(value: kind.Start), ValidityClaim.Finite(value: kind.End),
            ValidityClaim.Nonnegative(value: kind.StartRadius), ValidityClaim.Nonnegative(value: kind.EndRadius)),
        twist: static kind => ValidityClaim.All(
            kind.Axis.IsValid, ValidityClaim.Finite(value: kind.AngleRadians), kind.Extent is not null),
        cage: static kind => ValidityClaim.All(
            ModelClaim.Handle(handle: kind.Reference), ModelClaim.Handle(handle: kind.Target)),
        control: static kind => ValidityClaim.All(
            ModelClaim.Handle(handle: kind.OriginCurve), ModelClaim.Handle(handle: kind.TargetCurve)));

    internal Fin<GeometryHandle> Morph(GeometryHandle target, CapabilitySet<MorphBehavior> tuning, Context context) =>
        Switch(
            (Target: target, Tuning: tuning, Domain: context),
            bend: static (ctx, kind) => Deformed(
                mint: () => kind.Angle.Case switch {
                    double angle => new Morphs.BendSpaceMorph(
                        start: kind.Start, end: kind.End, point: kind.Through, angle: angle,
                        straight: kind.Behavior.Admits(capability: BendBehavior.Straight),
                        symmetric: kind.Behavior.Admits(capability: BendBehavior.Symmetric)),
                    _ => new Morphs.BendSpaceMorph(
                        start: kind.Start, end: kind.End, point: kind.Through,
                        straight: kind.Behavior.Admits(capability: BendBehavior.Straight),
                        symmetric: kind.Behavior.Admits(capability: BendBehavior.Symmetric)),
                },
                ctx: ctx),
            flow: static (ctx, kind) => ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.BaseRail, body: baseRail =>
                ModelGate.Borrow<Curve, GeometryHandle>(handle: kind.TargetRail, body: targetRail => Deformed(
                    mint: () => new Morphs.FlowSpaceMorph(
                        curve0: baseRail, curve1: targetRail,
                        reverseCurve0: kind.Behavior.Admits(capability: FlowBehavior.ReverseBase),
                        reverseCurve1: kind.Behavior.Admits(capability: FlowBehavior.ReverseTarget),
                        preventStretching: kind.Behavior.Admits(capability: FlowBehavior.PreventStretching)),
                    ctx: ctx))),
            maelstrom: static (ctx, kind) => Deformed(
                mint: () => new Morphs.MaelstromSpaceMorph(plane: kind.Frame, radius0: kind.Radius0, radius1: kind.Radius1, angle: kind.AngleRadians),
                ctx: ctx),
            splop: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.Surface, body: surface => Deformed(
                mint: () => new Morphs.SplopSpaceMorph(plane: kind.Frame, surface: surface, surfaceParam: kind.SurfaceUv, scale: kind.Scale, angle: kind.AngleRadians),
                ctx: ctx)),
            sporph: static (ctx, kind) => ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.BaseSurface, body: baseSurface =>
                ModelGate.Borrow<Surface, GeometryHandle>(handle: kind.TargetSurface, body: targetSurface => Deformed(
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
                    bFlat: kind.Behavior.Admits(capability: TaperBehavior.Flat),
                    infiniteTaper: kind.Behavior.Admits(capability: TaperBehavior.Infinite)),
                ctx: ctx),
            twist: static (ctx, kind) => Deformed(
                mint: () => new Morphs.TwistSpaceMorph {
                    TwistAxis = kind.Axis,
                    TwistAngleRadians = kind.AngleRadians,
                    InfiniteTwist = kind.Extent.Native,
                },
                ctx: ctx),
            cage: static (ctx, kind) => ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Reference, body: reference =>
                ModelGate.Borrow<Mesh, GeometryHandle>(handle: kind.Target, body: cageTarget => Deformed(
                    mint: () => new Morphs.MeshCageMorph(referenceMesh: reference, targetMesh: cageTarget),
                    ctx: ctx))),
            control: static (ctx, kind) => ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.OriginCurve, body: origin =>
                ModelGate.Borrow<NurbsCurve, GeometryHandle>(handle: kind.TargetCurve, body: driven =>
                    Try.lift(() => {
                        using MorphControl driver = new(originCurve: origin, targetCurve: driven);
                        driver.SpaceMorphTolerance = ctx.Domain.Absolute.Value;
                        driver.QuickPreview = ctx.Tuning.Admits(capability: MorphBehavior.QuickPreview);
                        driver.PreserveStructure = ctx.Tuning.Admits(capability: MorphBehavior.PreserveStructure);
                        return Duplicated(target: ctx.Target, morph: working => Admit.Confirm(success: driver.Morph(geometry: working)));
                    }).Run().Bind(static inner => inner))));

    private static Fin<GeometryHandle> Deformed<TMorph>(
        Func<TMorph> mint, (GeometryHandle Target, CapabilitySet<MorphBehavior> Tuning, Context Domain) ctx)
        where TMorph : SpaceMorph, IDisposable =>
        Try.lift(() => {
            using TMorph active = mint();
            active.Tolerance = ctx.Domain.Absolute.Value;
            active.QuickPreview = ctx.Tuning.Admits(capability: MorphBehavior.QuickPreview);
            active.PreserveStructure = ctx.Tuning.Admits(capability: MorphBehavior.PreserveStructure);
            return Duplicated(target: ctx.Target, morph: working => Admit.Confirm(success: active.Morph(geometry: working)));
        }).Run().Bind(static inner => inner);

    private static Fin<GeometryHandle> Duplicated(GeometryHandle target, Func<GeometryBase, Fin<Unit>> morph) =>
        ModelGate.Borrow<GeometryBase, GeometryHandle>(handle: target, body: source =>
            from _ in guard(SpaceMorph.IsMorphable(geometry: source), new KernelFault.Unsupported(InputType: source.GetType(), OutputType: typeof(GeometryBase)))
            from working in Try.lift(() => Optional(source.Duplicate()).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from morphed in morph(arg: working).Match(
                Succ: _ => ModelGate.Own(built: working),
                Fail: error => {
                    working.Dispose();
                    return Fin.Fail<GeometryHandle>(error: error);
                })
            select morphed);
}
```

## [03]-[FLATTEN]

- Owner: `UnrollFollowers` and `SquishFollowers` — the carried-geometry rows of the two flatteners; `UnrollLaw`, `SquishSpring`, and `SquishLaw` — the flattener policies; `SquishBehavior` — the squish grant vocabulary; `UnrollOutput` — the explode row.
- Law: each follower row carries only geometry whose flattened form returns through the operation result. `UnrollFollowers` admits curve handles, while `SquishFollowers` keeps curve and live `TextDot` handles distinct because the native entrypoints require different geometry classes.
- Law: each generated factory hook and `IsValid` read the same owner admission expression, so an invalid instance is unconstructible and no consumer re-tests what construction already proved; the spread claims are the spine's `ModelClaim` rows, so a follower spread's element invariant is stated once.
- Law: `SquishLaw.Rig` hands back a LEASE, never a bare native — `SquishParameters.Default` mints a fresh instance on every read (`ReferenceEquals(Default, Default)` is false) and the type is `IDisposable`, so the rig acquires through `Lease.Acquire`, configures inside a rolled-back `Catch` that disposes on a mid-configure throw, and the consuming arm's `Lease.Use` closes it with the cleanup fault aggregated into the primary. Both the `= null` custody sentinel and the hand `try`/`finally` delete: custody is a case, never a nulled local.
- Law: the spring pair is one value — `SetSpringConstants` takes both biases together and `GetSpringConstants` answers both, so `SquishSpring` carries the pair and an absent spring is `None` rather than two defaulted zeros.
- Growth: a new flattener grant is one `SquishBehavior` row; a new solver weight is one `SquishLaw` column with its claim.
- Packages: RhinoCommon deform (`.api/api-rhinocommon-deform.md` — `Unroller` `:37` (`ExplodeOutput`, `ExplodeSpacing`, `AbsoluteTolerance`, `RelativeTolerance`, `AddFollowingGeometry`, `PerformUnroll`), `Squisher`, `SquishParameters`, `SquishFlatteningAlgorithm`, `SquishDeformation`), kernel `Domain/results` (`Lease<T>.Acquire`/`Use`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context.Absolute`, `Context.Fractional`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SquishBehavior : ICapability<SquishBehavior> {
    public static readonly SquishBehavior PreserveBoundary = new(key: "preserve-boundary");
    public static readonly SquishBehavior PreserveTopology = new(key: "preserve-topology");
    public static readonly SquishBehavior SaveMapping = new(key: "save-mapping");
    public static readonly SquishBehavior CaptureNets = new(key: "capture-nets");
}

[SmartEnum<int>]
public sealed partial class UnrollOutput {
    public static readonly UnrollOutput Joined = new(key: 0, native: false);
    public static readonly UnrollOutput Exploded = new(key: 1, native: true);

    internal bool Native { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct UnrollFollowers : IValidityEvidence {
    public Seq<GeometryHandle> Curves { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<GeometryHandle> curves) =>
        validationError = ModelClaim.Handles(handles: curves, allowEmpty: true)
            ? null
            : new ValidationError("Unroll followers require live curve handles.");

    public bool IsValid => ModelClaim.Handles(handles: Curves, allowEmpty: true);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishFollowers : IValidityEvidence {
    public Seq<GeometryHandle> Curves { get; }
    public Seq<GeometryHandle> Dots { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<GeometryHandle> curves,
        ref Seq<GeometryHandle> dots) =>
        validationError = Admits(curves: curves, dots: dots)
            ? null
            : new ValidationError("Squish followers require live curve and dot handles.");

    public bool IsValid => Admits(curves: Curves, dots: Dots);

    private static ValidityClaim Admits(Seq<GeometryHandle> curves, Seq<GeometryHandle> dots) =>
        ValidityClaim.All(
            ModelClaim.Handles(handles: curves, allowEmpty: true),
            ModelClaim.Handles(handles: dots, allowEmpty: true));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct UnrollLaw : IValidityEvidence {
    public UnrollOutput Output { get; }
    public double Spacing { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UnrollOutput output,
        ref double spacing) =>
        validationError = Admits(output: output, spacing: spacing)
            ? null
            : new ValidationError("Unroll output must be declared, and spacing must be finite and non-negative.");

    public bool IsValid => Admits(output: Output, spacing: Spacing);

    private static ValidityClaim Admits(UnrollOutput? output, double spacing) =>
        ValidityClaim.All(output is not null, ValidityClaim.Nonnegative(value: spacing));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishSpring : IValidityEvidence {
    public double Boundary { get; }
    public double Deformation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double boundary,
        ref double deformation) =>
        validationError = Admits(boundary: boundary, deformation: deformation)
            ? null
            : new ValidationError("Squish spring constants must be finite and non-negative.");

    public bool IsValid => Admits(boundary: Boundary, deformation: Deformation);

    private static ValidityClaim Admits(double boundary, double deformation) =>
        ValidityClaim.All(ValidityClaim.Nonnegative(value: boundary), ValidityClaim.Nonnegative(value: deformation));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SquishLaw : IValidityEvidence {
    public SquishFlatteningAlgorithm Algorithm { get; }
    public SquishDeformation Mode { get; }
    public CapabilitySet<SquishBehavior> Behavior { get; }
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
        ref CapabilitySet<SquishBehavior> behavior,
        ref double boundaryStretch,
        ref double boundaryCompress,
        ref double interiorStretch,
        ref double interiorCompress,
        ref double absoluteLimit,
        ref Option<SquishSpring> spring) =>
        validationError = Admits(
                algorithm: algorithm, mode: mode, boundaryStretch: boundaryStretch, boundaryCompress: boundaryCompress,
                interiorStretch: interiorStretch, interiorCompress: interiorCompress, absoluteLimit: absoluteLimit, spring: spring)
            ? null
            : new ValidationError("Squish policies must be declared, weights must be finite and non-negative, and the limit must be finite and positive.");

    public bool IsValid => Admits(
        algorithm: Algorithm, mode: Mode, boundaryStretch: BoundaryStretch, boundaryCompress: BoundaryCompress,
        interiorStretch: InteriorStretch, interiorCompress: InteriorCompress, absoluteLimit: AbsoluteLimit, spring: Spring);

    internal Fin<Lease<SquishParameters>> Rig() =>
        Lease<SquishParameters>.Acquire(mint: static () => SquishParameters.Default)
            .Bind(lease => Try.lift(() => {
                SquishParameters parameters = lease.Resource;
                parameters.Algorithm = Algorithm;
                parameters.PreserveTopology = Behavior.Admits(capability: SquishBehavior.PreserveTopology);
                parameters.SaveMapping = Behavior.Admits(capability: SquishBehavior.SaveMapping);
                parameters.AbsoluteLimit = AbsoluteLimit;
                parameters.SetDeformation(
                    deformation: Mode,
                    bPreserveBoundary: Behavior.Admits(capability: SquishBehavior.PreserveBoundary),
                    boundaryStretchConstant: BoundaryStretch,
                    boundaryCompressConstant: BoundaryCompress,
                    interiorStretchConstant: InteriorStretch,
                    interiorCompressConstant: InteriorCompress);
                _ = Spring.Iter(spring => parameters.SetSpringConstants(
                    boundaryBias: spring.Boundary,
                    deformationBias: spring.Deformation));
                return Fin.Succ(value: lease);
            }).Run().Bind(static inner => inner).Rollback(lease.Resource));

    private static ValidityClaim Admits(
        SquishFlatteningAlgorithm algorithm,
        SquishDeformation mode,
        double boundaryStretch,
        double boundaryCompress,
        double interiorStretch,
        double interiorCompress,
        double absoluteLimit,
        Option<SquishSpring> spring) =>
        ValidityClaim.All(
            Enum.IsDefined(algorithm), Enum.IsDefined(mode),
            ValidityClaim.Nonnegative(value: boundaryStretch), ValidityClaim.Nonnegative(value: boundaryCompress),
            ValidityClaim.Nonnegative(value: interiorStretch), ValidityClaim.Nonnegative(value: interiorCompress),
            ValidityClaim.Positive(value: absoluteLimit),
            ValidityClaim.Evidence(evidence: spring));
}
```

## [04]-[ALGEBRA]

- Owner: `DeformOp` `[Union]` — the sole operation algebra; `Deforms` — the one entry folding any operation spread into one owned geometry sequence.
- Law: the entry is `Build`, as on every sibling — one concept, one spelling across the eight Modeling rosters, and the `Apply` name stays where it means "run this operation" on `DeformOp` itself.
- Law: admission NAMES its axis — `Admitted` dispatches the generated `Switch` into the spine's `ModelClaim.Admits`, so a request breaching several constraints answers one keyed fault per breached axis and a new case breaks the compile rather than falling to a silent refusal.
- Law: every arm returns only owned geometry handles through `ModelGate`; unconsumed native side channels close inside the producing call.
- Law: engine custody splits on the host and is stated where it bites — `Squisher` and `MeshUnwrapper` are `IDisposable` and release after every geometry product has detached; `Unroller` holds no native handle and needs no release. `ModelGate.OwnEach` owns each direct squish result before producing the next, so a later refusal releases the complete prefix, and every `Rollback` in the squish chain releases the exact accumulated prefix at its own edge.
- Growth: a new deformation verb is one `DeformOp` case with its arm.
- Packages: RhinoCommon deform (`.api/api-rhinocommon-deform.md` — `Unroller`, `Squisher` (`SquishSurface`, `SquishMesh`, `SquishCurve`, `SquishTextDot`, `Get2dMesh`, `Get3dMesh`, `Is2dPatternSquished`, `SquishBack2dMarks`), `MeshUnwrapper`, `MeshUnwrapMethod`), kernel `Domain/results` (`Try.lift`, `Admit.Confirm`, `Lease<T>.Use`, ``), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`), LanguageExt.Core (`Seq`), Thinktecture.Runtime.Extensions.

```csharp
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeformOp {
    private DeformOp() { }
    public sealed record Morph(GeometryHandle Target, MorphKind Kind, CapabilitySet<MorphBehavior> Behavior) : DeformOp;
    public sealed record Unroll(GeometryHandle Target, UnrollFollowers Followers, UnrollLaw Law) : DeformOp;
    public sealed record Squish(GeometryHandle Target, SquishLaw Law, Seq<GeometryHandle> Marks, SquishFollowers Followers) : DeformOp;
    public sealed record SquishBack(GeometryHandle Pattern, Seq<GeometryHandle> Marks) : DeformOp;
    public sealed record Unwrap(Seq<GeometryHandle> Meshes, MeshUnwrapMethod Method, Option<Plane> Symmetry = default) : DeformOp;

    internal Fin<DeformOp> Admitted() =>
        Switch(
            morph: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Kind), row.Kind is { IsValid: true })),
            unroll: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Followers), row.Followers.IsValid), (nameof(row.Law), row.Law.IsValid)),
            squish: static (row) => ModelClaim.Admits(row,
                (nameof(row.Target), ModelClaim.Handle(handle: row.Target)),
                (nameof(row.Law), row.Law.IsValid),
                (nameof(row.Marks), ModelClaim.Handles(handles: row.Marks, allowEmpty: true)),
                (nameof(row.Followers), row.Followers.IsValid)),
            squishBack: static (row) => ModelClaim.Admits(row,
                (nameof(row.Pattern), ModelClaim.Handle(handle: row.Pattern)),
                (nameof(row.Marks), ModelClaim.Handles(handles: row.Marks))),
            unwrap: static (row) => ModelClaim.Admits(row,
                (nameof(row.Meshes), ModelClaim.Handles(handles: row.Meshes)),
                (nameof(row.Method), Enum.IsDefined(row.Method)),
                (nameof(row.Symmetry), ValidityClaim.WhenPresent(facet: row.Symmetry, claim: static symmetry => symmetry.IsValid))));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain) =>
        Switch(
            domain,
            morph: static (model, edit) => {
                return edit.Kind.Morph(target: edit.Target, tuning: edit.Behavior, context: model)
                    .Map(product => Seq(product));
            },
            unroll: static (model, edit) => {
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Target, body: source =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Followers.Curves, allowEmpty: true, body: followers =>
                        Try.lift(() => {
                            Fin<Unroller> admitted = source switch {
                                Brep brep => Fin.Succ(value: new Unroller(brep: brep)),
                                Surface surface => Fin.Succ(value: new Unroller(surface: surface)),
                                _ => Fin.Fail<Unroller>(error: new KernelFault.Unsupported(InputType: source.GetType(), OutputType: typeof(Unroller))),
                            };
                            return admitted.Bind(active => {
                                active.ExplodeOutput = edit.Law.Output.Native;
                                active.ExplodeSpacing = edit.Law.Spacing;
                                active.AbsoluteTolerance = model.Absolute.Value;
                                active.RelativeTolerance = model.Fractional;
                                if (!followers.IsEmpty) { active.AddFollowingGeometry(curves: followers.AsIterable()); }
                                Brep[] flatBreps = active.PerformUnroll(
                                    unrolledCurves: out Curve[] flatCurves,
                                    unrolledPoints: out _,
                                    unrolledDots: out TextDot[] flatDots);
                                Seq<Curve> carried = Optional(flatCurves).Map(static rows => toSeq(rows)).IfNone(Seq<Curve>());
                                Seq<TextDot> dots = Optional(flatDots).Map(static rows => toSeq(rows)).IfNone(Seq<TextDot>());
                                return
                                    from flat in ModelGate.OwnMany(built: flatBreps)
                                        .Rollback([.. dots])
                                    from carriedHandles in ModelGate.OwnMany(
                                            built: carried, allowEmpty: true)
                                        .Rollback([.. flat])
                                        .Rollback([.. dots])
                                    let _ = dots.Iter(static dot => dot.Dispose())
                                    select flat + carriedHandles;
                            });
                        }).Run().Bind(static inner => inner)));
            },
            squish: static (_, edit) => {
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Target, body: source =>
                    ModelGate.BorrowMany<GeometryBase, Seq<GeometryHandle>>(handles: edit.Marks, allowEmpty: true, body: marks =>
                        from parameters in edit.Law.Rig()
                        from flattened in parameters.Use(body: sp => Try.lift(() => {
                            using Squisher engine = new();
                            System.Collections.Generic.List<GeometryBase> mapped = [];
                            Fin<GeometryHandle> flat = source switch {
                                Surface surface => ModelGate.Own(
                                    built: engine.SquishSurface(sp: sp, surface: surface, marks: marks.AsIterable(), squished_marks_out: mapped)),
                                Mesh mesh => ModelGate.Own(
                                    built: engine.SquishMesh(sp: sp, mesh3d: mesh, marks: marks.AsIterable(), squished_marks_out: mapped)),
                                _ => Fin.Fail<GeometryHandle>(error: new KernelFault.Unsupported(InputType: source.GetType(), OutputType: typeof(Squisher))),
                            };
                            return flat.Rollback([.. mapped]).Bind(primary => (
                                from crossed in ModelGate.OwnMany(built: mapped, allowEmpty: true)
                                from directCurves in ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(
                                    handles: edit.Followers.Curves,
                                    allowEmpty: true,
                                    body: curves => ModelGate.OwnEach(
                                        sources: curves,
                                        run: engine.SquishCurve,
                                        allowEmpty: true)).Rollback([.. crossed])
                                from directDots in ModelGate.BorrowMany<TextDot, Seq<GeometryHandle>>(
                                    handles: edit.Followers.Dots,
                                    allowEmpty: true,
                                    body: dots => ModelGate.OwnEach(
                                        sources: dots,
                                        run: engine.SquishTextDot,
                                        allowEmpty: true)).Rollback([.. crossed + directCurves])
                                from nets in (edit.Law.Behavior.Admits(capability: SquishBehavior.CaptureNets)
                                    ? from flat2d in ModelGate.Own(built: engine.Get2dMesh())
                                      from flat3d in ModelGate.Own(built: engine.Get3dMesh()).Rollback(flat2d)
                                      select Seq(flat2d, flat3d)
                                    : Fin.Succ(value: Seq<GeometryHandle>()))
                                    .Rollback([.. crossed + directCurves + directDots])
                                select Seq(primary) + crossed + directCurves + directDots + nets)
                                .Rollback(primary));
                        }).Run().Bind(static inner => inner))
                        select flattened));
            },
            squishBack: static (_, edit) => {
                return ModelGate.Borrow<GeometryBase, Seq<GeometryHandle>>(handle: edit.Pattern, body: pattern =>
                    ModelGate.BorrowMany<GeometryBase, Seq<GeometryHandle>>(handles: edit.Marks, body: marks =>
                        from _ in Admit.Confirm(success: Squisher.Is2dPatternSquished(geometry: pattern))
                        from restored in Try.lift(() => ModelGate.OwnMany(
                            built: Squisher.SquishBack2dMarks(squishedGeometry: pattern, marks: marks.AsIterable()))).Run().Bind(static inner => inner)
                        select restored));
            },
            unwrap: static (_, edit) => {
                return ModelGate.BorrowMany<Mesh, Seq<GeometryHandle>>(handles: edit.Meshes, body: sources =>
                    from working in sources.FoldM<Fin, Seq<Mesh>>(Seq<Mesh>(), (held, source) =>
                        Try.lift(() => Optional(source.Duplicate() as Mesh).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                            .Map(copy => held.Add(value: copy))
                            .Rollback([.. held]))
                    from unwrapped in Try.lift(() => {
                            using MeshUnwrapper engine = new(meshes: working.AsIterable());
                            _ = edit.Symmetry.Iter(symmetry => engine.SymmetryPlane = symmetry);
                            return Admit.Confirm(success: engine.Unwrap(method: edit.Method))
                                .Bind(_ => ModelGate.Many(op, () => working.AsEnumerable()));
                        }).Run().Bind(static inner => inner)
                        .Rollback([.. working])
                    select unwrapped);
            });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Deforms {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<DeformOp> operations) {
        Seq<DeformOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(),
                apply: static (operation, model) => operation.Apply(domain: model)).ToEff());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
