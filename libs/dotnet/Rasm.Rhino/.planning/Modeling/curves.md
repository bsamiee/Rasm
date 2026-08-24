# [RASM_RHINO_MODELING_CURVES]

`Rasm.Rhino.Modeling` owns curve host operations. One `CurveOp` union carries offset, refinement, extension, shortening, splitting, pulling, projection, joining, booleans, planar regions, blend/fillet/tween/match/mean construction, interpolation, analytic and freeform seeding, text outlines, and subd-friendly rebuilds through `HostCurves.Build`. Policy unions discriminate native overloads from payload shape; `Context` supplies every tolerance and angle.

This page also seats the Modeling spine's shared vocabulary — `ModelClaim`, `ModelFact`, `FitPosture`, `PairPosture` — that every sibling composes. Kernel parametric evaluation, division, curvature, contours, and predicate-exact offsets remain kernel-owned.

## [01]-[INDEX]

- [02]-[MODEL_CLAIM]: `ModelClaim`, `ModelFact`, `FitPosture`, `PairPosture` — the folder-wide admission fold, the absent-channel fact gate, and the two shared policy rows.
- [03]-[OFFSET_POLICY]: `CurveScalar`, `OffsetFrame`, `SurfaceOffsetTarget`, `SurfaceLift`, `RibbonRefit`, `RibbonLaw` — finite scalar admission, offset discriminants, and the ribbon carrier.
- [04]-[SHAPE_POLICY]: `CurveEdit`, `ExtendLaw`, `ShortenLaw`, `SplitCutter`, `PullTarget`, `ProjectTarget`, `CurveBooleanLaw`, `BlendLaw`, `ArcBlendLaw`, `TweenLaw`, `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, `CatenaryLaw`, `TextFace`, `FitGrant`, `PointCountLaw`, `FitLaw`, `JoinGrant`, `FilletGrant`, `FilletRailDegree`, `FilletArcDegree`, `RailFilletLaw` — the modality vocabularies.
- [05]-[OPERATION_RAIL]: `CurveSlot`, `CurveOp`, and the `HostCurves.Build` entry.

## [02]-[MODEL_CLAIM]

- Owner: `ModelClaim` — the Modeling spine's admission vocabulary: the handle, spread, and row shape claims every policy owner and every `*Op.Admitted` reads, and `Admits`, the ONE accumulating axis fold each operation roster dispatches into; `ModelFact` — the absent-channel gate every host side channel lands through; `FitPosture` and `PairPosture` — the two host modality rows more than one Modeling page instantiates.
- Law: a refusal NAMES its axis. `Admits` folds `(axis, claim)` pairs through the `Validation` applicative, so an operation violating four constraints answers four `KernelFault.InvalidInput(Key, Axis)` errors on one rail instead of one indistinguishable refusal; a caller repairing a request reads the whole rejection set at once, exactly as `ModelGate.Entry` reports the whole operation set. One composite `guard` over a case's constraints is the deleted form.
- Law: every shape claim answers `ValidityClaim`, never a bare `bool` — the claims fold through `ValidityClaim.All` beside the kernel rows an owner already reads, and the implicit `bool` conversion in both directions means a comparison, a nested owner's own `IsValid`, and a claim compose at one arity. Scalar, coordinate, count, and direction predicates are the KERNEL's rows (`Domain/rails.md#[08]`) and are never re-spelled here; this owner adds only what the kernel does not model — handle liveness and the spread arms over the folder's own carriers.
- Law: `Rows` is the one spread arm — a page proving a per-element invariant over a `Seq` supplies its element claim and re-spells no `ForAll`; `Handles` and `Points` are its two named instantiations, kept because they are read at forty-odd sites and the element claim is fixed.
- Law: `FitPosture` and `PairPosture` are rows because the host projection is a COLUMN, not a name. `Loose` was a case-name suffix on four `PullTarget` cases and one `OffsetFrame` case, and the reversal pair was six positional bools across three `CurveOp` cases and two `lofting.md` vocabularies; both now travel as one value whose host projection the row carries, so a call site cannot transpose the pair and a new corner is a row. Row status needs a host column, a writer, or a correlated sibling; without one a two-state modality stays a named `bool` on its owning case — `PreserveTangents`, `Simple`, `FixEnds`, `Interpolate`, `Average` are that form, and the sibling pages' two-row `Native` vocabularies are the opposite form and correct as they stand.
- Law: a host side channel that answered NOTHING lands no fact — `ModelFact.Channel` is the one gate, so an empty spread and an absent one stay two readings and a `?? []` coalesce (which certifies absence as an empty answer) is the deleted form corpus-wide on this spine. Consumers reading an empty `Project` over the slot read that the host stayed silent.
- Law: rank is the kernel's own derivation — `ICapability<TSelf>.Rank` projects declaration ordinal through the per-constructed-type index (`Domain/validation.md` [06]), so no roster on this spine carries a hand-kept mirror column and none re-spells the projection through a folder-local helper. Vocabularies mirroring a HOST flag word (`TextFace`, `SilhouetteKind`) OVERRIDE the member and rank on the bit, because the bit IS the host's declared order and `CapabilitySet.Mask` already reads it.
- Growth: a new shape claim is one `ModelClaim` member; a new shared modality corner is one `FitPosture`/`PairPosture` row with every consumer's projection unchanged.
- Packages: kernel `Domain/rails` (`Op`, `KernelFault.InvalidInput(Key, Axis)`, `ValidityClaim`, `Fin`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Rasm.Rhino.Document` (`GeometryHandle`), LanguageExt.Core (`Validation`, `Seq`, `Traverse` — `libs/dotnet/.api/api-languageext.md`), Thinktecture.Runtime.Extensions (`[SmartEnum]` — `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FitPosture : ICapability<FitPosture> {
    public static readonly FitPosture Fitted = new(key: "fitted", native: false);
    public static readonly FitPosture Loose = new(key: "loose", native: true);

    internal bool Native { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PairPosture : ICapability<PairPosture> {
    public static readonly PairPosture Neither = new(key: "neither", first: false, second: false);
    public static readonly PairPosture FirstOnly = new(key: "first", first: true, second: false);
    public static readonly PairPosture SecondOnly = new(key: "second", first: false, second: true);
    public static readonly PairPosture Both = new(key: "both", first: true, second: true);

    internal bool First { get; }
    internal bool Second { get; }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Absence is a MISSING fact, never a zero: a host out-channel that answered nothing contributes no receipt row,
// so a consumer's empty projection over the slot reads as "the host stayed silent" rather than "the host answered
// none" — the two are different answers and a coalesced empty erases the difference.
internal static class ModelFact {
    internal static BuildReceipt<TSlot> Channel<TSlot>(TSlot slot, Option<BuildBody> value) where TSlot : notnull =>
        value.Map(body => BuildReceipt<TSlot>.Of(slot: slot, body: body)).IfNone(BuildReceipt<TSlot>.Empty);

    // `IEnumerable<T>?` and not `T[]?`: the host publishes side channels as arrays, jagged arrays, and `List<T>`
    // alike, and one arity covers all three without a call site converting first.
    internal static Option<Seq<T>> Answered<T>(IEnumerable<T>? channel) => Optional(channel).Map(static rows => toSeq(rows));
}

internal static class ModelClaim {
    internal static ValidityClaim Handle(GeometryHandle? handle) => handle is not null;

    internal static ValidityClaim Handles(Seq<GeometryHandle> handles, bool allowEmpty = false) =>
        Rows(rows: handles, claim: static handle => Handle(handle: handle), allowEmpty: allowEmpty);

    internal static ValidityClaim Points(Seq<Point3d> points, bool allowEmpty = false) =>
        Rows(rows: points, claim: static point => ValidityClaim.Finite(value: point), allowEmpty: allowEmpty);

    internal static ValidityClaim Rows<T>(Seq<T> rows, Func<T, ValidityClaim> claim, bool allowEmpty = false) =>
        ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: rows.Count, floor: allowEmpty ? 0 : 1),
            rows.ForAll(row => claim(arg: row)));

    // One refusal per violated axis. The applicative accumulates, so a request breaching four constraints answers
    // four keyed faults on one rail; the operation surfaces unaltered on the success arm, so the roster's `Admitted`
    // stays a total projection rather than a predicate the caller re-reads the value through.
    internal static Fin<TOp> Admits<TOp>(TOp operation, Op key, params ReadOnlySpan<(string Axis, ValidityClaim Holds)> axes) =>
        toSeq(axes.ToArray())
            .Traverse(axis => axis.Holds
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(key.InvalidInput(axis: axis.Axis)))
            .As()
            .Map(_ => operation)
            .ToFin();
}
```

## [03]-[OFFSET_POLICY]

- Owner: `CurveScalar` admits every finite policy scalar once; `OffsetFrame` closes planar and normal framing as explicit cases with the fit posture as a column; `SurfaceOffsetTarget` closes each catalogued host-and-law pair; `SurfaceLift` closes normal and tangent lifting; `RibbonRefit` resolves refit tolerance from a behavior-bearing row; `RibbonLaw` carries the native ribbon policy as one admitted value.
- Law: varying distances are admitted `CurveScalar` rows — the on-surface arm splits `(Parameter, Distance)` rows into the two parallel native arrays at the call, so finiteness and cardinality are proven by construction.
- Law: `RibbonLaw.Rig` is the one site naming `RibbonOffsetParameters` — offset distance, location, plane vector, blend radius, rebuild and refit knobs, cross-section alignment, and the `RibbonOffsetSurfaceMethod` row bake in one member with the tolerance slot reading the regime; the ribbon's rails, cross-sections, and breps cross as products behind per-class tallies. Generated transcription does not reach this member: four of its nine slots are regime derivations (`OffsetTolerance`, `RefitTolerance`) or `Option`-to-sentinel lowerings (`BlendRadius`, `OffsetPlaneVector3d`) that no source-to-target mapping expresses, and a mapper carrying the residual four beside them is the split form `Exchange/options.md` already carves against.
- Law: an absent optional lowers to the host's own sentinel, never a zero — `BlendRadius` is `Option<CurveScalar>` mapped to `RhinoMath.UnsetValue` and `PlaneVector` is `Option<Vector3d>` mapped to `Vector3d.Unset`, so "no blend" and "a zero-radius blend" stay distinct requests instead of collapsing on a default-constructed slot.
- Law: `RibbonLaw` is unconstructible when invalid — its generated factory runs the same fold its `IsValid` reads, so the ribbon arm never receives a policy whose location, rebuild count, plane vector, or surface method is unsound. NAMED LOSS: the five defaulted columns the record form carried must now be supplied at the call; bought back by an invalid instance being unrepresentable rather than caught one layer later at `Admitted`.
- Packages: RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — `RibbonOffsetParameters` `[04]`, `RibbonOffsetSurfaceMethod` `[09]`, `Curve.RibbonOffset` `:214`, `Curve.OffsetOnSurface`/`OffsetNormalToSurface`/`OffsetTangentToSurface`), kernel `Domain/rails` (`Op`, `ValidityClaim`, `IValidityEvidence`, `Fin`), kernel `Domain/context` (`Context`, `Tolerance`), `Rasm.Rhino.Document` (`GeometryHandle`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct CurveScalar {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = ValidityClaim.Finite(value: value)
            ? null
            : new ValidationError(message: string.Create(CultureInfo.InvariantCulture, $"CurveScalar must be finite (got {value:R})."));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OffsetFrame : IValidityEvidence {
    private OffsetFrame() { }
    public sealed record InPlane(Plane Value, CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp) : OffsetFrame;
    public sealed record ByNormal(
        Point3d DirectionPoint, Vector3d Normal, FitPosture Posture,
        CurveOffsetCornerStyle Corner = CurveOffsetCornerStyle.Sharp,
        CurveOffsetEndStyle End = CurveOffsetEndStyle.None) : OffsetFrame;

    public bool IsValid => Switch(
        inPlane: static frame => ValidityClaim.All(frame.Value.IsValid, Enum.IsDefined(frame.Corner)),
        byNormal: static frame => ValidityClaim.All(
            ValidityClaim.Finite(value: frame.DirectionPoint), ValidityClaim.Direction(value: frame.Normal),
            frame.Posture is not null, Enum.IsDefined(frame.Corner), Enum.IsDefined(frame.End)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceOffsetTarget : IValidityEvidence {
    private SurfaceOffsetTarget() { }
    public sealed record FaceDistance(GeometryHandle Host, int Face, CurveScalar Distance) : SurfaceOffsetTarget;
    public sealed record FacePoint(GeometryHandle Host, int Face, Point2d Point) : SurfaceOffsetTarget;
    public sealed record FaceVarying(GeometryHandle Host, int Face, Seq<(CurveScalar Parameter, CurveScalar Distance)> Rows) : SurfaceOffsetTarget;
    public sealed record SurfaceDistance(GeometryHandle Host, CurveScalar Distance) : SurfaceOffsetTarget;
    public sealed record SurfacePoint(GeometryHandle Host, Point2d Point) : SurfaceOffsetTarget;

    public bool IsValid => Switch(
        faceDistance: static target => ValidityClaim.All(
            ModelClaim.Handle(handle: target.Host), ValidityClaim.CountAtLeast(count: target.Face, floor: 0)),
        facePoint: static target => ValidityClaim.All(
            ModelClaim.Handle(handle: target.Host), ValidityClaim.CountAtLeast(count: target.Face, floor: 0), target.Point.IsValid),
        faceVarying: static target => ValidityClaim.All(
            ModelClaim.Handle(handle: target.Host), ValidityClaim.CountAtLeast(count: target.Face, floor: 0),
            ValidityClaim.CountAtLeast(count: target.Rows.Count, floor: 1)),
        surfaceDistance: static target => ModelClaim.Handle(handle: target.Host),
        surfacePoint: static target => ValidityClaim.All(ModelClaim.Handle(handle: target.Host), target.Point.IsValid));

    internal Fin<Built<CurveSlot>> Apply(Curve curve, Context model, Op op) => Switch(
        state: (Curve: curve, Model: model, Op: op),
        faceDistance: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face < brep.Faces.Count, ctx.Op.InvalidInput(axis: nameof(target.Face)))
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(brep.Faces[target.Face], target.Distance.Value, ctx.Model.Absolute.Value))
            select built),
        facePoint: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face < brep.Faces.Count, ctx.Op.InvalidInput(axis: nameof(target.Face)))
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(brep.Faces[target.Face], target.Point, ctx.Model.Absolute.Value))
            select built),
        faceVarying: static (ctx, target) => ModelGate.Borrow<Brep, Built<CurveSlot>>(target.Host, ctx.Op, brep =>
            from _ in guard(target.Face < brep.Faces.Count, ctx.Op.InvalidInput(axis: nameof(target.Face)))
            from built in ModelGate.Many(ctx.Op, CurveSlot.OnSurface, () => ctx.Curve.OffsetOnSurface(
                    face: brep.Faces[target.Face],
                    curveParameters: target.Rows.Map(static row => row.Parameter.Value).ToArray(),
                    offsetDistances: target.Rows.Map(static row => row.Distance.Value).ToArray(),
                    fittingTolerance: ctx.Model.Absolute.Value))
            select built),
        surfaceDistance: static (ctx, target) => ModelGate.Borrow<Surface, Built<CurveSlot>>(target.Host, ctx.Op, surface =>
            ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(surface, target.Distance.Value, ctx.Model.Absolute.Value))),
        surfacePoint: static (ctx, target) => ModelGate.Borrow<Surface, Built<CurveSlot>>(target.Host, ctx.Op, surface =>
            ModelGate.Many(ctx.Op, CurveSlot.OnSurface,
                () => ctx.Curve.OffsetOnSurface(surface, target.Point, ctx.Model.Absolute.Value))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceLift {
    private SurfaceLift() { }
    public sealed record Normal : SurfaceLift;
    public sealed record Tangent : SurfaceLift;
}

[SmartEnum<int>]
public sealed partial class RibbonRefit {
    public static readonly RibbonRefit None = new(key: 0, resolve: static _ => 0.0);
    public static readonly RibbonRefit AtTolerance = new(key: 1, resolve: static domain => domain.Absolute.Value);

    [UseDelegateFromConstructor]
    internal partial double Resolve(Context domain);
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct RibbonLaw : IValidityEvidence {
    public CurveScalar Distance { get; }
    public Point3d Location { get; }
    public RibbonRefit Refit { get; }
    public Option<CurveScalar> BlendRadius { get; }
    public Option<Vector3d> PlaneVector { get; }
    public int RebuildPointCount { get; }
    public bool AlignCrossSections { get; }
    public RibbonOffsetSurfaceMethod SurfaceMethod { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CurveScalar distance,
        ref Point3d location,
        ref RibbonRefit refit,
        ref Option<CurveScalar> blendRadius,
        ref Option<Vector3d> planeVector,
        ref int rebuildPointCount,
        ref bool alignCrossSections,
        ref RibbonOffsetSurfaceMethod surfaceMethod) {
        validationError = Admits(location: location, refit: refit, planeVector: planeVector,
                rebuildPointCount: rebuildPointCount, surfaceMethod: surfaceMethod)
            ? null
            : new ValidationError("Ribbon policy requires a valid location, a declared refit row, a non-degenerate plane vector, a non-negative rebuild count, and a declared surface method.");
    }

    public bool IsValid => Admits(
        location: Location, refit: Refit, planeVector: PlaneVector,
        rebuildPointCount: RebuildPointCount, surfaceMethod: SurfaceMethod);

    // `AlignCrossSections` stays a named bool: it is one independent host knob with no sibling in the record, no
    // host projection column, and no legal-corner law a row could carry.
    internal Fin<RibbonOffsetParameters> Rig(Context domain, Op key) =>
        key.Catch(() => Fin.Succ(value: new RibbonOffsetParameters {
            OffsetDistance = Distance.Value,
            OffsetLocation = Location,
            OffsetTolerance = domain.Absolute.Value,
            OffsetPlaneVector3d = PlaneVector.IfNone(Vector3d.Unset),
            BlendRadius = BlendRadius.Map(static row => row.Value).IfNone(RhinoMath.UnsetValue),
            RebuildPointCount = RebuildPointCount,
            RefitTolerance = Refit.Resolve(domain),
            AlignCrossSections = AlignCrossSections,
            RibbonSurfaceGenerationMethod = SurfaceMethod,
        }));

    private static ValidityClaim Admits(
        Point3d location, RibbonRefit? refit, Option<Vector3d> planeVector,
        int rebuildPointCount, RibbonOffsetSurfaceMethod surfaceMethod) =>
        ValidityClaim.All(
            ValidityClaim.Finite(value: location),
            refit is not null,
            ValidityClaim.WhenPresent(facet: planeVector, claim: static vector => ValidityClaim.Direction(value: vector)),
            ValidityClaim.CountAtLeast(count: rebuildPointCount, floor: 0),
            Enum.IsDefined(surfaceMethod));
}
```

## [04]-[SHAPE_POLICY]

- Owner: the modality vocabularies — `CurveBooleanLaw` carries exactly the source arity consumed by union, intersection, or difference; `ExtendLaw` fuses the extension side, style, and terminal; `ShortenLaw` fuses domain and end trimming; `SplitCutter` fuses parameter, brep, surface, and plane splitting; `PullTarget`/`ProjectTarget` fuse sources with their destinations; `BlendLaw` and `ArcBlendLaw` close blend construction; `TweenLaw` fuses plain, matched, and sampled tweening; `SpiralLaw`, `ParabolaSeed`, `AnalyticCurve`, and `CatenaryLaw` fuse the construction seed families; `PointCountLaw` closes the three legal control-point modes; `FitLaw` carries the advanced `NurbsCurveFitParameters` surface as one policy value; `TextFace`, `FitGrant`, `JoinGrant`, and `FilletGrant` are the page's capability rosters.
- Law: the discriminant is the value's shape — every native overload family resolves from the case the caller constructed, so no arm reads a mode flag and no verb family grows a `ByX` sibling. `PullTarget` carries its fit posture as a column rather than doubling its case count, and every arm composes the host's own `loose`-bearing overload with the row's projection.
- Law: `FitAxis` distinguishes fixed intensity from coefficient-bearing custom intensity; `FitGrant` carries orthogonal fit grants as a `CapabilitySet` column; `PointCountLaw` closes automatic, fixed, and variable control-point modes as cases, so the three local corner bools `FitLaw` folded by hand are one total `Switch` and a fourth mode is one case that breaks every reader.
- Law: a capability column is a `CapabilitySet`, never a `FrozenSet` — the kernel column carries `Admits`, `AdmitsAll`, `Mask`, rank-ordered `Wire`, and the `[UnorderedEquality]` membership comparison a raw `FrozenSet` member of a value object cannot give (record equality compares a frozen set by REFERENCE, so two identical policies read unequal), and membership outside the roster is unrepresentable rather than probed.
- Law: catenary construction is one case over four shape terminals — through-point, length, parameter, and apex select the native static, and the apex, parameter, length, and deviation out-channels land as facts on every form.
- Law: no native mode int crosses a case payload — `ArcDegree` and `ArcSlider` carry the non-rational arc-bezier degree and sliders off the folder's shared owners, `CurveCompatibility` carries the simplify method with its rebuild count, and `TextFace` folds the additive font-style bits through `CapabilitySet.Mask`, so the empty set IS the host's `0` Normal and no arm re-derives an encoding.
- Law: the rail fillet declares three host rosters the arc-bezier owners do not share — `FilletRailDegree` closes the rail direction at 3 or 5, `FilletArcDegree` closes the arc direction at 2 through 5 where 2 alone yields a rational surface, and the slider pair is a two-slot tuple of `ArcSlider` because the host takes exactly two; a `Seq` of sliders lets the wrong arity reach a native that reads two.
- Law: `FitLaw.Rig` stays hand-seated for the reason `RibbonLaw.Rig` does, and harder — three `FitAxis` members each fan to a target intensity AND coefficient slot, the grant column fans to four target bools, and `KinkAngleRadians` reads the regime, so six of nine source members are one-to-many fans no source-complete mapping admits.
- Packages: RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — `NurbsCurveFitParameters` `[05]`, `Curve.CreateFilletCurves` `:170`, `Curve.JoinCurves` `:209`, `Curve.CreateTextOutlines` `:213`, `NurbsCurve.MakeCompatible` `:193`, `Curve.PullToBrepFace`/`PullToMesh` `:112-118`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), kernel `Domain/rails` (`ValidityClaim`, `IValidityEvidence`, `Op`), `Modeling/lofting.md` (`CurveCompatibility`, `SweepEnds`), `Modeling/meshing.md` (`SmoothLaw`), `Modeling/solids.md` (`ArcDegree`, `ArcSlider`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveEdit : IValidityEvidence {
    private CurveEdit() { }
    public sealed record RemoveShort : CurveEdit;
    public sealed record CloseGap : CurveEdit;
    public sealed record TrimDomain(Interval Value) : CurveEdit;

    public bool IsValid => Switch(
        removeShort: static _ => (ValidityClaim)true,
        closeGap: static _ => (ValidityClaim)true,
        trimDomain: static edit => (ValidityClaim)edit.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtendLaw : IValidityEvidence {
    private ExtendLaw() { }
    public sealed record ByLength(CurveEnd Side, double Length, CurveExtensionStyle Style) : ExtendLaw;
    public sealed record ToGeometry(CurveEnd Side, CurveExtensionStyle Style, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record ToPoint(CurveEnd Side, CurveExtensionStyle Style, Point3d Terminal) : ExtendLaw;
    public sealed record ByLine(CurveEnd Side, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record ByArc(CurveEnd Side, Seq<GeometryHandle> Bounds) : ExtendLaw;
    public sealed record OnSurface(CurveEnd Side, GeometryHandle Target, Option<int> Face = default) : ExtendLaw;

    public bool IsValid => Switch(
        byLength: static law => ValidityClaim.All(
            Enum.IsDefined(law.Side), ValidityClaim.Positive(value: law.Length), Enum.IsDefined(law.Style)),
        toGeometry: static law => ValidityClaim.All(
            Enum.IsDefined(law.Side), Enum.IsDefined(law.Style), ModelClaim.Handles(handles: law.Bounds)),
        toPoint: static law => ValidityClaim.All(
            Enum.IsDefined(law.Side), Enum.IsDefined(law.Style), ValidityClaim.Finite(value: law.Terminal)),
        byLine: static law => ValidityClaim.All(Enum.IsDefined(law.Side), ModelClaim.Handles(handles: law.Bounds)),
        byArc: static law => ValidityClaim.All(Enum.IsDefined(law.Side), ModelClaim.Handles(handles: law.Bounds)),
        onSurface: static law => ValidityClaim.All(
            Enum.IsDefined(law.Side), ModelClaim.Handle(handle: law.Target),
            ValidityClaim.WhenPresent(facet: law.Face, claim: static face => ValidityClaim.CountAtLeast(count: face, floor: 0))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShortenLaw : IValidityEvidence {
    private ShortenLaw() { }
    public sealed record ToDomain(Interval Value) : ShortenLaw;
    public sealed record AtEnd(CurveEnd Side, double Length) : ShortenLaw;

    public bool IsValid => Switch(
        toDomain: static law => (ValidityClaim)law.Value.IsValid,
        atEnd: static law => ValidityClaim.All(Enum.IsDefined(law.Side), ValidityClaim.Positive(value: law.Length)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SplitCutter : IValidityEvidence {
    private SplitCutter() { }
    public sealed record AtParameters(Seq<double> Values) : SplitCutter;
    public sealed record ByBrep(GeometryHandle Value) : SplitCutter;
    public sealed record BySurface(GeometryHandle Value) : SplitCutter;
    public sealed record ByPlane(Plane Value) : SplitCutter;

    public bool IsValid => Switch(
        atParameters: static cutter => ModelClaim.Rows(
            rows: cutter.Values, claim: static value => ValidityClaim.Finite(value: value)),
        byBrep: static cutter => ModelClaim.Handle(handle: cutter.Value),
        bySurface: static cutter => ModelClaim.Handle(handle: cutter.Value),
        byPlane: static cutter => (ValidityClaim)cutter.Value.IsValid);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PullTarget : IValidityEvidence {
    private PullTarget() { }
    public sealed record ToFace(GeometryHandle Brep, int Face, FitPosture Posture) : PullTarget;
    public sealed record ToMesh(GeometryHandle Mesh, FitPosture Posture) : PullTarget;

    public bool IsValid => Switch(
        toFace: static target => ValidityClaim.All(
            ModelClaim.Handle(handle: target.Brep), ValidityClaim.CountAtLeast(count: target.Face, floor: 0),
            target.Posture is not null),
        toMesh: static target => ValidityClaim.All(ModelClaim.Handle(handle: target.Mesh), target.Posture is not null));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectTarget : IValidityEvidence {
    private ProjectTarget() { }
    public sealed record ToBreps(Seq<GeometryHandle> Curves, Seq<GeometryHandle> Breps, Vector3d Direction) : ProjectTarget;
    public sealed record ToMeshes(Seq<GeometryHandle> Curves, Seq<GeometryHandle> Meshes, Vector3d Direction) : ProjectTarget;
    public sealed record ToPlane(GeometryHandle Curve, Plane Plane) : ProjectTarget;

    public bool IsValid => Switch(
        toBreps: static target => ValidityClaim.All(
            ModelClaim.Handles(handles: target.Curves), ModelClaim.Handles(handles: target.Breps),
            ValidityClaim.Direction(value: target.Direction)),
        toMeshes: static target => ValidityClaim.All(
            ModelClaim.Handles(handles: target.Curves), ModelClaim.Handles(handles: target.Meshes),
            ValidityClaim.Direction(value: target.Direction)),
        toPlane: static target => ValidityClaim.All(ModelClaim.Handle(handle: target.Curve), target.Plane.IsValid));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveBooleanLaw : IValidityEvidence {
    private CurveBooleanLaw() { }
    public sealed record Union(Seq<GeometryHandle> Curves) : CurveBooleanLaw;
    public sealed record Intersection(GeometryHandle First, GeometryHandle Second) : CurveBooleanLaw;
    public sealed record Difference(GeometryHandle First, Seq<GeometryHandle> Subtractors) : CurveBooleanLaw;

    public bool IsValid => Switch(
        union: static law => ModelClaim.Handles(handles: law.Curves),
        intersection: static law => ValidityClaim.All(
            ModelClaim.Handle(handle: law.First), ModelClaim.Handle(handle: law.Second)),
        difference: static law => ValidityClaim.All(
            ModelClaim.Handle(handle: law.First), ModelClaim.Handles(handles: law.Subtractors)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlendLaw : IValidityEvidence {
    private BlendLaw() { }
    public sealed record EndToEnd(BlendContinuity Continuity, Option<(double BulgeA, double BulgeB)> Bulge = default) : BlendLaw;
    public sealed record AtParameters(double T0, BlendContinuity Continuity0, double T1, BlendContinuity Continuity1, PairPosture Reverse) : BlendLaw;

    public bool IsValid => Switch(
        endToEnd: static law => ValidityClaim.All(
            Enum.IsDefined(law.Continuity),
            ValidityClaim.WhenPresent(facet: law.Bulge, claim: static bulge => ValidityClaim.All(
                ValidityClaim.Finite(value: bulge.BulgeA), ValidityClaim.Finite(value: bulge.BulgeB)))),
        atParameters: static law => ValidityClaim.All(
            ValidityClaim.Finite(value: law.T0), Enum.IsDefined(law.Continuity0),
            ValidityClaim.Finite(value: law.T1), Enum.IsDefined(law.Continuity1), law.Reverse is not null));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArcBlendLaw : IValidityEvidence {
    private ArcBlendLaw() { }
    public sealed record ControlPointRatio(CurveScalar Ratio) : ArcBlendLaw;
    public sealed record LineArcRadius(CurveScalar Radius) : ArcBlendLaw;

    public bool IsValid => Switch(
        controlPointRatio: static law => ValidityClaim.Positive(value: law.Ratio.Value),
        lineArcRadius: static law => ValidityClaim.Positive(value: law.Radius.Value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TweenLaw : IValidityEvidence {
    private TweenLaw() { }
    public sealed record Plain : TweenLaw;
    public sealed record Matched : TweenLaw;
    public sealed record Sampled(int Samples) : TweenLaw;

    public bool IsValid => Switch(
        plain: static _ => (ValidityClaim)true,
        matched: static _ => (ValidityClaim)true,
        sampled: static law => ValidityClaim.CountAtLeast(count: law.Samples, floor: 1));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpiralLaw : IValidityEvidence {
    private SpiralLaw() { }
    public sealed record AboutAxis(Point3d AxisStart, Vector3d AxisDirection) : SpiralLaw;
    public sealed record AlongRail(GeometryHandle Rail, double T0, double T1, int PointsPerTurn) : SpiralLaw;

    public bool IsValid => Switch(
        aboutAxis: static law => ValidityClaim.All(
            ValidityClaim.Finite(value: law.AxisStart), ValidityClaim.Direction(value: law.AxisDirection)),
        alongRail: static law => ValidityClaim.All(
            ModelClaim.Handle(handle: law.Rail), ValidityClaim.Finite(value: law.T0), ValidityClaim.Finite(value: law.T1),
            ValidityClaim.CountAtLeast(count: law.PointsPerTurn, floor: 1)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParabolaSeed : IValidityEvidence {
    private ParabolaSeed() { }
    public sealed record FromVertex(Point3d Vertex, Point3d Start, Point3d End) : ParabolaSeed;
    public sealed record FromFocus(Point3d Focus, Point3d Start, Point3d End) : ParabolaSeed;
    public sealed record FromPoints(Point3d Start, Point3d Inner, Point3d End) : ParabolaSeed;

    public bool IsValid => Switch(
        fromVertex: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.Vertex), ValidityClaim.Finite(value: seed.Start), ValidityClaim.Finite(value: seed.End)),
        fromFocus: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.Focus), ValidityClaim.Finite(value: seed.Start), ValidityClaim.Finite(value: seed.End)),
        fromPoints: static seed => ValidityClaim.All(
            ValidityClaim.Finite(value: seed.Start), ValidityClaim.Finite(value: seed.Inner), ValidityClaim.Finite(value: seed.End)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticCurve : IValidityEvidence {
    private AnalyticCurve() { }
    public sealed record OfLine(Line Value) : AnalyticCurve;
    public sealed record OfArc(Arc Value, Option<(int Degree, int CvCount)> Structure = default) : AnalyticCurve;
    public sealed record OfCircle(Circle Value, Option<(int Degree, int CvCount)> Structure = default) : AnalyticCurve;
    public sealed record OfEllipse(Ellipse Value) : AnalyticCurve;

    public bool IsValid => Switch(
        ofLine: static seed => (ValidityClaim)seed.Value.IsValid,
        ofArc: static seed => ValidityClaim.All(seed.Value.IsValid, Structured(structure: seed.Structure)),
        ofCircle: static seed => ValidityClaim.All(seed.Value.IsValid, Structured(structure: seed.Structure)),
        ofEllipse: static seed => (ValidityClaim)seed.Value.IsValid);

    private static ValidityClaim Structured(Option<(int Degree, int CvCount)> structure) =>
        ValidityClaim.WhenPresent(facet: structure, claim: static shape => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: shape.Degree, floor: 1),
            ValidityClaim.CountAtLeast(count: shape.CvCount, floor: shape.Degree + 1)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CatenaryLaw : IValidityEvidence {
    private CatenaryLaw() { }
    public sealed record ThroughPoint(Point3d Value) : CatenaryLaw;
    public sealed record FromLength(double Value) : CatenaryLaw;
    public sealed record FromParameter(double Value) : CatenaryLaw;
    public sealed record FromApex(Point3d Value) : CatenaryLaw;

    public bool IsValid => Switch(
        throughPoint: static law => ValidityClaim.Finite(value: law.Value),
        fromLength: static law => ValidityClaim.Positive(value: law.Value),
        fromParameter: static law => ValidityClaim.Finite(value: law.Value),
        fromApex: static law => ValidityClaim.Finite(value: law.Value));
}

// RhinoCommon reads `textStyle` additively — 0 Normal, 1 Bold, 2 Italic, "any number of the following" — so the
// empty set IS Normal, the host bit is the row's rank, and `CapabilitySet.Mask` is the whole encoding.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextFace : ICapability<TextFace> {
    public static readonly TextFace Bold = new(key: "bold", bit: 1);
    public static readonly TextFace Italic = new(key: "italic", bit: 2);

    public int Rank => Bit;
    internal int Bit { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JoinGrant : ICapability<JoinGrant> {
    public static readonly JoinGrant PreserveDirection = new(key: "preserve-direction");
    public static readonly JoinGrant SimpleJoin = new(key: "simple-join");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilletGrant : ICapability<FilletGrant> {
    public static readonly FilletGrant JoinResult = new(key: "join-result");
    public static readonly FilletGrant TrimInputs = new(key: "trim-inputs");
    public static readonly FilletGrant ArcExtension = new(key: "arc-extension");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FitGrant : ICapability<FitGrant> {
    public static readonly FitGrant SubDFriendly = new(key: "subd-friendly");
    public static readonly FitGrant Closed = new(key: "closed");
    public static readonly FitGrant TangentsAtKinks = new(key: "tangents-at-kinks");
    public static readonly FitGrant Optimize = new(key: "optimize");
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitAxis {
    private FitAxis() { }
    public sealed record Preset(FitPreset Value) : FitAxis;
    public sealed record Custom(CurveScalar Coefficient) : FitAxis;

    internal (NurbsCurveFitParameters.Intensity Intensity, double Coefficient) Native() => Switch(
        preset: static value => (value.Value.Native, 0.0),
        custom: static value => (NurbsCurveFitParameters.Intensity.Custom, value.Coefficient.Value));

    internal bool Resolved => Switch(
        preset: static axis => axis.Value is not null,
        custom: static _ => true);
}

[SmartEnum<int>]
public sealed partial class FitPreset {
    public static readonly FitPreset None = new(key: 0, native: NurbsCurveFitParameters.Intensity.None);
    public static readonly FitPreset Low = new(key: 1, native: NurbsCurveFitParameters.Intensity.Low);
    public static readonly FitPreset Moderate = new(key: 2, native: NurbsCurveFitParameters.Intensity.Moderate);
    public static readonly FitPreset Medium = new(key: 3, native: NurbsCurveFitParameters.Intensity.Medium);
    public static readonly FitPreset High = new(key: 4, native: NurbsCurveFitParameters.Intensity.High);
    public static readonly FitPreset Extreme = new(key: 5, native: NurbsCurveFitParameters.Intensity.Extreme);

    public NurbsCurveFitParameters.Intensity Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PointCountLaw {
    private PointCountLaw() { }
    public sealed record Automatic : PointCountLaw;
    public sealed record Fixed(int Count) : PointCountLaw;
    public sealed record Variable(int Count, int Ceiling) : PointCountLaw;

    // Mode reads off the (count, range) PAIR at the host — automatic is `(0, [0,0])`, fixed pins the range shut,
    // variable opens it upward — so the pair projects here once and no consumer re-derives which mode it named.
    internal (int PointCount, IndexPair Range) Native => Switch(
        automatic: static _ => (0, new IndexPair(0, 0)),
        @fixed: static law => (law.Count, new IndexPair(0, 0)),
        variable: static law => (law.Count, new IndexPair(law.Count, law.Ceiling)));

    internal ValidityClaim Admits(int degree) => Switch(
        state: degree,
        automatic: static _ => (ValidityClaim)true,
        @fixed: static (floor, law) => ValidityClaim.CountAtLeast(count: law.Count, floor: floor + 1),
        variable: static (floor, law) => ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: law.Count, floor: floor + 1),
            ValidityClaim.CountAtLeast(count: law.Ceiling, floor: law.Count)));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct FitLaw : IValidityEvidence {
    public FitAxis Smoothing { get; }
    public FitAxis Uniformity { get; }
    public FitAxis CurvatureBias { get; }
    public CapabilitySet<FitGrant> Grants { get; }
    public NurbsCurveFitParameters.TangentMatch TangentMatching { get; }
    public NurbsCurveFitParameters.KinkSplit KinkSplitting { get; }
    public int Degree { get; }
    public PointCountLaw PointCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FitAxis smoothing,
        ref FitAxis uniformity,
        ref FitAxis curvatureBias,
        ref CapabilitySet<FitGrant> grants,
        ref NurbsCurveFitParameters.TangentMatch tangentMatching,
        ref NurbsCurveFitParameters.KinkSplit kinkSplitting,
        ref int degree,
        ref PointCountLaw pointCount) {
        validationError = Admits(
                smoothing: smoothing, uniformity: uniformity, curvatureBias: curvatureBias,
                tangentMatching: tangentMatching, kinkSplitting: kinkSplitting, degree: degree, pointCount: pointCount)
            ? null
            : new ValidationError("Curve fit policy is inconsistent.");
    }

    public bool IsValid => Admits(
        smoothing: Smoothing, uniformity: Uniformity, curvatureBias: CurvatureBias,
        tangentMatching: TangentMatching, kinkSplitting: KinkSplitting, degree: Degree, pointCount: PointCount);

    internal Fin<NurbsCurveFitParameters> Rig(Context domain, Op key) =>
        key.Catch(() => (Smoothing.Native(), Uniformity.Native(), CurvatureBias.Native(), PointCount.Native) switch {
            var (smoothing, uniformity, curvature, count) => Fin.Succ(value: new NurbsCurveFitParameters {
                TangentMatching = TangentMatching, KinkSplitting = KinkSplitting,
                SmoothingIntensity = smoothing.Intensity,
                UniformityIntensity = uniformity.Intensity,
                CurvatureBiasIntensity = curvature.Intensity,
                Degree = Degree,
                PointCount = count.PointCount,
                KinkAngleRadians = domain.Angle.Value,
                SmoothingCoefficient = smoothing.Coefficient,
                UniformityCoefficient = uniformity.Coefficient,
                CurvatureBiasCoefficient = curvature.Coefficient,
                SubDFriendly = Grants.Admits(capability: FitGrant.SubDFriendly),
                Closed = Grants.Admits(capability: FitGrant.Closed),
                ApplyTangentMatchingAtKinks = Grants.Admits(capability: FitGrant.TangentsAtKinks),
                OptimizeCurve = Grants.Admits(capability: FitGrant.Optimize),
                PointCountRange = count.Range,
            }),
        });

    private static ValidityClaim Admits(
        FitAxis? smoothing,
        FitAxis? uniformity,
        FitAxis? curvatureBias,
        NurbsCurveFitParameters.TangentMatch tangentMatching,
        NurbsCurveFitParameters.KinkSplit kinkSplitting,
        int degree,
        PointCountLaw? pointCount) =>
        ValidityClaim.All(
            smoothing is { Resolved: true }, uniformity is { Resolved: true }, curvatureBias is { Resolved: true },
            Enum.IsDefined(tangentMatching), Enum.IsDefined(kinkSplitting),
            ValidityClaim.CountAtLeast(count: degree, floor: 1),
            pointCount is not null && pointCount.Admits(degree: degree));
}

[SmartEnum<int>]
public sealed partial class FilletRailDegree {
    public static readonly FilletRailDegree Cubic = new(key: 3);
    public static readonly FilletRailDegree Quintic = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class FilletArcDegree {
    public static readonly FilletArcDegree Rational = new(key: 2);
    public static readonly FilletArcDegree Cubic = new(key: 3);
    public static readonly FilletArcDegree Quartic = new(key: 4);
    public static readonly FilletArcDegree Quintic = new(key: 5);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct RailFilletLaw : IValidityEvidence {
    public FilletRailDegree Rail { get; }
    public FilletArcDegree Arc { get; }
    public (ArcSlider Tangent, ArcSlider Inner) Sliders { get; }
    public int BezierSurfaceCount { get; }
    public bool Extend { get; }
    public FilletSurfaceSplitType Split { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FilletRailDegree rail,
        ref FilletArcDegree arc,
        ref (ArcSlider Tangent, ArcSlider Inner) sliders,
        ref int bezierSurfaceCount,
        ref bool extend,
        ref FilletSurfaceSplitType split) {
        validationError = Admits(rail: rail, arc: arc, bezierSurfaceCount: bezierSurfaceCount, split: split)
            ? null
            : new ValidationError("Rail fillet requires declared rail and arc degrees, a non-negative bezier count, and a declared split type.");
    }

    public bool IsValid => Admits(rail: Rail, arc: Arc, bezierSurfaceCount: BezierSurfaceCount, split: Split);

    // `numBezierSrfs` is documented "if > 0, …", so zero is the legal do-not-subdivide value, never a floor breach.
    private static ValidityClaim Admits(
        FilletRailDegree? rail, FilletArcDegree? arc, int bezierSurfaceCount, FilletSurfaceSplitType split) =>
        ValidityClaim.All(
            rail is not null, arc is not null,
            ValidityClaim.CountAtLeast(count: bezierSurfaceCount, floor: 0), Enum.IsDefined(split));
}
```

## [05]-[OPERATION_RAIL]

- Owner: `CurveSlot` `[SmartEnum<int>]` — the consequence vocabulary; `CurveOp` `[Union]` `[GenerateUnionOps]` — the whole verified curve host-op roster, each case carrying its own generated `SelfOp`; `HostCurves` — the one entry folding any operation spread into one `Built<CurveSlot>`.
- Law: the entry family renames at the boundary — the kernel owns `Curves` (`Analysis/select.md`), so this host roster is `HostCurves` under the branch rule that a boundary declaration whose simple name matches a kernel owner renames on the host side.
- Law: refinement is value-semantic — fair, fit, rebuild, smooth, and simplify run the instance member on the borrowed curve and own the returned refinement; the boolean tolerance-less and tween tolerance-less overloads are obsolete, so every arm runs the tolerance form off the regime.
- Law: correspondence maps survive — projection folds its curve and brep source indices, join folds its key map, curve-boolean union folds its index map, and the planar-region product folds its per-region partition as `SourceGroups`, so a consumer never re-derives which input produced which output. Mesh-target projection is the declared exception: `Curve.ProjectToMesh` exposes no index channel on any overload, so the `ToMeshes` arm records the product tally alone and carries no correspondence guarantee.
- Law: the boolean-regions carrier dies at the seam — `CurveBooleanRegions` is read inside the arm, its per-region curves cross as products partitioned by a `SourceGroups` fact, and point assignments carry the `SourceAxis.Region` discriminant. Region walking is one generated cross-product over the three host counters, and the per-region offset partition is one scan, so no index arithmetic is repeated per boundary.
- Law: `CurveOp.Admitted` closes the whole roster before dispatch through the generated `Switch` — every handle, index, finite scalar, degree-versus-count relation, host enum, direction vector, and nested policy payload admits at the spine, each constraint NAMING its axis through `ModelClaim.Admits`, so a request breaching several constraints reports all of them and a new case breaks the compile instead of falling to a silent refusal.
- Law: admission is owner-local — every policy union answers its own `IsValid` off its generated `Switch` and every policy value object answers the same fold its generated factory ran, so a new case breaks its owning union's evidence at compile; `Admitted` composes those answers over case-local shape read through `ModelClaim`, and one `object?`-typed predicate switching over every policy type is the deleted form that let a new case pass unchecked.
- Law: end reconciliation answers pairs — `MakeEndsMeet` duplicates both curves, reconciles the duplicates under one `PairPosture`, and crosses both as products, so the mutating native never touches an input handle; the staged harvest rolls both duplicates back on the failure path, because a throwing or refusing native orphans two live copies otherwise.
- Law: a two-state modality is a named `bool` on its owning case only where it is the WHOLE fact — one independent host argument with no correlated sibling, no host projection column, and no legal-corner law (`PreserveTangents`, `FixEnds`, `Interpolate`, `PeriodicClosed`, `Average`, `Smooth`, `CloseLoops`, `Combine`). Every modality carrying a host projection column, a writer delegate, a native factory, or a correlated partner is a ROW instead: `FitPosture` carries `loose`, `PairPosture` carries the operand pair, `JoinGrant`/`FilletGrant`/`TextFace` carry adjacent host arguments a call site transposes silently, and the sibling Modeling pages' two-row `Native` vocabularies hold that same form.
- Law: smoothing seats once — `Curve.Smooth` and `Mesh.Smooth` take the IDENTICAL five knobs (factor, three axis bits, boundary bit, coordinate system, frame), so `Smooth` consumes the meshing rail's `SmoothLaw` rather than respelling them as five payload fields; the mesh-only pass count and vertex selection stay on that page's cases, and the law's valid-frame admission means this arm composes the long host overload alone.
- Law: compatibility seats once and reconciles once — `CurveCompatibility` is the lofting rail's `[ComplexValueObject]` over the simplify method and rebuild count, `SweepEnds` is that rail's admitted terminal pair whose `StartOrUnset`/`EndOrUnset` lower `None` to the host's documented `Point3d.Unset` omit spelling, and `CurveOp.Compatible` is the ONE `NurbsCurve.MakeCompatible` call site in the folder. Lofting's second compatibility verb wrapped the same host member behind a second slot and is deleted; a caller batching compatibility before a loft composes two operations in one `Build` spread.
- Law: direction agreement is evidence — `DirectionsMatch` borrows both curves and lands the host `Curve.DoDirectionsMatch` verdict as a `Flag` fact with no product, so join, sweep, and loft preparation reads the verdict off the receipt rail; the kernel's typed direction-relation vocabulary stays the analysis-altitude owner, and this host boolean serves construction preparation only.
- Growth: a new curve host verb is one case with its arm; a new modality is one case on the owning policy union.
- Packages: RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — the `Curve`/`NurbsCurve` construction, boolean, blend, fillet, tween, match, and outline rosters `:160-225`), RhinoCommon geometry (`.api/api-rhinocommon-geometry.md` — `CurveBooleanRegions`, `CurveSimplifyOptions`, `IndexPair`), kernel `Domain/rails` (`Op`, `Fault`, `ValidityClaim`, `[GenerateUnionOps]` + generated `SelfOp`, `Fin`), kernel `Domain/context` (`Context`), `Modeling/solids.md` (`ModelGate`, `Built<TSlot>`, `BuildReceipt<TSlot>`, `BuildBody`, `SourceAxis`), `Modeling/lofting.md` (`SweepEnds`, `CurveCompatibility`), `Modeling/meshing.md` (`SmoothLaw`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveSlot {
    public static readonly CurveSlot Offset = new(key: 0);
    public static readonly CurveSlot OnSurface = new(key: 1);
    public static readonly CurveSlot Lifted = new(key: 2);
    public static readonly CurveSlot Ribboned = new(key: 3);
    public static readonly CurveSlot Rails = new(key: 4);
    public static readonly CurveSlot Sections = new(key: 5);
    public static readonly CurveSlot Surfaced = new(key: 6);
    public static readonly CurveSlot Refined = new(key: 7);
    public static readonly CurveSlot FitEvidence = new(key: 8);
    public static readonly CurveSlot Extended = new(key: 9);
    public static readonly CurveSlot Shortened = new(key: 10);
    public static readonly CurveSlot SplitApart = new(key: 11);
    public static readonly CurveSlot Pulled = new(key: 12);
    public static readonly CurveSlot Projected = new(key: 13);
    public static readonly CurveSlot Joined = new(key: 14);
    public static readonly CurveSlot Booled = new(key: 15);
    public static readonly CurveSlot Regions = new(key: 16);
    public static readonly CurveSlot Blended = new(key: 17);
    public static readonly CurveSlot Filleted = new(key: 18);
    public static readonly CurveSlot Tweened = new(key: 19);
    public static readonly CurveSlot Matched = new(key: 20);
    public static readonly CurveSlot Constructed = new(key: 21);
    public static readonly CurveSlot Reconciled = new(key: 22);
    public static readonly CurveSlot Outlined = new(key: 23);
    public static readonly CurveSlot Trimmed0 = new(key: 24);
    public static readonly CurveSlot Trimmed1 = new(key: 25);
    public static readonly CurveSlot DirectionVerdict = new(key: 26);
}

[GenerateUnionOps]
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveOp {
    private CurveOp() { }
    public sealed record Offset(GeometryHandle Curve, OffsetFrame Frame, CurveScalar Distance) : CurveOp;
    public sealed record OffsetOnSurface(GeometryHandle Curve, SurfaceOffsetTarget Target) : CurveOp;
    public sealed record OffsetLift(GeometryHandle Curve, GeometryHandle Surface, CurveScalar Height, SurfaceLift Lift) : CurveOp;
    public sealed record Ribbon(GeometryHandle Curve, RibbonLaw Law) : CurveOp;
    public sealed record Fair(GeometryHandle Curve, int ClampStart, int ClampEnd, int Iterations) : CurveOp;
    public sealed record Fit(GeometryHandle Curve, int Degree) : CurveOp;
    public sealed record Rebuild(GeometryHandle Curve, int PointCount, int Degree, bool PreserveTangents) : CurveOp;
    public sealed record Smooth(GeometryHandle Curve, SmoothLaw Law) : CurveOp;
    public sealed record Simplify(GeometryHandle Curve, CurveSimplifyOptions Options, Option<CurveEnd> EndOnly = default) : CurveOp;
    public sealed record Edit(GeometryHandle Curve, CurveEdit Verb) : CurveOp;
    public sealed record NurbsFit(GeometryHandle Curve, Interval Domain, FitLaw Law) : CurveOp;
    public sealed record Extend(GeometryHandle Curve, ExtendLaw Law) : CurveOp;
    public sealed record Shorten(GeometryHandle Curve, ShortenLaw Law) : CurveOp;
    public sealed record Split(GeometryHandle Curve, SplitCutter Cutter) : CurveOp;
    public sealed record Pull(GeometryHandle Curve, PullTarget Target) : CurveOp;
    public sealed record Project(ProjectTarget Target) : CurveOp;
    public sealed record Join(Seq<GeometryHandle> Curves, CapabilitySet<JoinGrant> Grants) : CurveOp;
    public sealed record Boolean(CurveBooleanLaw Law) : CurveOp;
    public sealed record Regions(Seq<GeometryHandle> Curves, Plane Frame, Seq<Point3d> Points, bool Combine) : CurveOp;
    public sealed record Blend(GeometryHandle First, GeometryHandle Second, BlendLaw Law) : CurveOp;
    public sealed record ArcBlend(Point3d Start, Vector3d StartDirection, Point3d End, Vector3d EndDirection, ArcBlendLaw Law) : CurveOp;
    public sealed record FilletCurves(
        GeometryHandle First, Point3d NearFirst, GeometryHandle Second, Point3d NearSecond,
        double Radius, CapabilitySet<FilletGrant> Grants) : CurveOp;
    public sealed record FilletCorners(GeometryHandle Curve, double Radius) : CurveOp;
    public sealed record Tween(GeometryHandle First, GeometryHandle Second, int Count, TweenLaw Law) : CurveOp;
    public sealed record MatchCurve(
        GeometryHandle First, GeometryHandle Second, PairPosture Reverse,
        BlendContinuity Continuity, PreserveEnd Preserve, bool Average) : CurveOp;
    public sealed record Mean(GeometryHandle First, GeometryHandle Second) : CurveOp;
    public sealed record TwoView(GeometryHandle First, GeometryHandle Second, Vector3d FirstDirection, Vector3d SecondDirection) : CurveOp;
    public sealed record Interpolated(Seq<Point3d> Points, int Degree, Option<CurveKnotStyle> Knots = default, Option<(Vector3d Start, Vector3d End)> Tangents = default) : CurveOp;
    public sealed record ControlPoints(Seq<Point3d> Points, int Degree = 3) : CurveOp;
    public sealed record FitPoints(Seq<Point3d> Points, bool Periodic = false, Option<(int Degree, Vector3d Start, Vector3d End)> Constrained = default) : CurveOp;
    public sealed record HSpline(Seq<Point3d> Points, Option<(Vector3d Start, Vector3d End)> Tangents = default) : CurveOp;
    public sealed record SoftEdit(GeometryHandle Curve, double T, Vector3d Delta, double Length, bool FixEnds = true) : CurveOp;
    public sealed record PeriodicClose(GeometryHandle Curve, bool Smooth = true) : CurveOp;
    public sealed record SubDFriendly(GeometryHandle Curve, Option<(int PointCount, bool PeriodicClosed)> Structure = default) : CurveOp;
    public sealed record SubDFriendlyPoints(Seq<Point3d> Points, bool Interpolate, bool PeriodicClosed) : CurveOp;
    public sealed record Compatible(Seq<GeometryHandle> Curves, SweepEnds Ends, CurveCompatibility Law) : CurveOp;
    public sealed record Spiral(SpiralLaw Law, Point3d RadiusPoint, double Pitch, double TurnCount, double Radius0, double Radius1) : CurveOp;
    public sealed record Parabola(ParabolaSeed Seed) : CurveOp;
    public sealed record ArcBezier(ArcDegree Degree, Point3d Center, Point3d Start, Point3d End, double Radius, ArcSlider TanSlider, ArcSlider MidSlider) : CurveOp;
    public sealed record Analytic(AnalyticCurve Seed) : CurveOp;
    public sealed record Catenary(Point3d Start, Point3d End, Vector3d AxisDirection, CatenaryLaw Law, bool Smooth, int PointCount) : CurveOp;
    public sealed record MakeEndsMeet(GeometryHandle First, GeometryHandle Second, PairPosture AdjustStart) : CurveOp;
    public sealed record RailFillet(
        GeometryHandle Rail, GeometryHandle First, int FirstFace, GeometryHandle Second, int SecondFace,
        double U, double V, RailFilletLaw Law) : CurveOp;
    public sealed record TextOutlines(
        string Text, string Font, double Height, CapabilitySet<TextFace> Faces,
        bool CloseLoops, Plane Frame, double SmallCapsScale = 1.0) : CurveOp;
    public sealed record DirectionsMatch(GeometryHandle First, GeometryHandle Second) : CurveOp;

    internal Fin<CurveOp> Admitted(Op key) =>
        Switch(
            context: key,
            offset: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Frame), row.Frame is { IsValid: true })),
            offsetOnSurface: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Target), row.Target is { IsValid: true })),
            offsetLift: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Surface), ModelClaim.Handle(handle: row.Surface)), (nameof(row.Lift), row.Lift is not null)),
            ribbon: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Law), row.Law.IsValid)),
            fair: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.ClampStart), ValidityClaim.CountAtLeast(count: row.ClampStart, floor: 0)),
                (nameof(row.ClampEnd), ValidityClaim.CountAtLeast(count: row.ClampEnd, floor: 0)),
                (nameof(row.Iterations), ValidityClaim.CountAtLeast(count: row.Iterations, floor: 1))),
            fit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Degree), ValidityClaim.CountAtLeast(count: row.Degree, floor: 1))),
            rebuild: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Degree), ValidityClaim.CountAtLeast(count: row.Degree, floor: 1)),
                (nameof(row.PointCount), ValidityClaim.CountAtLeast(count: row.PointCount, floor: row.Degree + 1))),
            smooth: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Law), row.Law.IsValid)),
            // `CurveSimplifyOptions` is a host flag mask, so every bit combination is legal material and only the
            // optional end selector carries a roster to gate.
            simplify: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.EndOnly), ValidityClaim.WhenPresent(facet: row.EndOnly, claim: static side => Enum.IsDefined(side)))),
            edit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Verb), row.Verb is { IsValid: true })),
            nurbsFit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Domain), row.Domain.IsValid), (nameof(row.Law), row.Law.IsValid)),
            extend: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Law), row.Law is { IsValid: true })),
            shorten: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Law), row.Law is { IsValid: true })),
            split: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Cutter), row.Cutter is { IsValid: true })),
            pull: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)), (nameof(row.Target), row.Target is { IsValid: true })),
            project: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Target), row.Target is { IsValid: true })),
            join: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves))),
            boolean: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Law), row.Law is { IsValid: true })),
            regions: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves)), (nameof(row.Frame), row.Frame.IsValid),
                (nameof(row.Points), ModelClaim.Points(points: row.Points, allowEmpty: true))),
            blend: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)), (nameof(row.Law), row.Law is { IsValid: true })),
            arcBlend: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Start), ValidityClaim.Finite(value: row.Start)),
                (nameof(row.StartDirection), ValidityClaim.Direction(value: row.StartDirection)),
                (nameof(row.End), ValidityClaim.Finite(value: row.End)),
                (nameof(row.EndDirection), ValidityClaim.Direction(value: row.EndDirection)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            filletCurves: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.NearFirst), ValidityClaim.Finite(value: row.NearFirst)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.NearSecond), ValidityClaim.Finite(value: row.NearSecond)),
                (nameof(row.Radius), ValidityClaim.Positive(value: row.Radius))),
            filletCorners: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Radius), ValidityClaim.Positive(value: row.Radius))),
            tween: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Count), ValidityClaim.CountAtLeast(count: row.Count, floor: 1)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            matchCurve: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.Reverse), row.Reverse is not null),
                (nameof(row.Continuity), Enum.IsDefined(row.Continuity)), (nameof(row.Preserve), Enum.IsDefined(row.Preserve))),
            mean: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second))),
            twoView: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.FirstDirection), ValidityClaim.Direction(value: row.FirstDirection)),
                (nameof(row.SecondDirection), ValidityClaim.Direction(value: row.SecondDirection))),
            interpolated: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ValidityClaim.All(
                    ModelClaim.Points(points: row.Points),
                    ValidityClaim.CountAtLeast(count: row.Points.Count, floor: row.Degree + 1))),
                (nameof(row.Degree), ValidityClaim.CountAtLeast(count: row.Degree, floor: 1)),
                (nameof(row.Knots), ValidityClaim.WhenPresent(facet: row.Knots, claim: static knots => Enum.IsDefined(knots))),
                (nameof(row.Tangents), ValidityClaim.WhenPresent(facet: row.Tangents, claim: static ends => ValidityClaim.All(
                    ValidityClaim.Direction(value: ends.Start), ValidityClaim.Direction(value: ends.End))))),
            controlPoints: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ValidityClaim.All(
                    ModelClaim.Points(points: row.Points),
                    ValidityClaim.CountAtLeast(count: row.Points.Count, floor: row.Degree + 1))),
                (nameof(row.Degree), ValidityClaim.CountAtLeast(count: row.Degree, floor: 1))),
            fitPoints: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ModelClaim.Points(points: row.Points)),
                (nameof(row.Constrained), ValidityClaim.WhenPresent(facet: row.Constrained, claim: static ends => ValidityClaim.All(
                    ValidityClaim.CountAtLeast(count: ends.Degree, floor: 1),
                    ValidityClaim.Direction(value: ends.Start), ValidityClaim.Direction(value: ends.End))))),
            hSpline: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ModelClaim.Points(points: row.Points)),
                (nameof(row.Tangents), ValidityClaim.WhenPresent(facet: row.Tangents, claim: static ends => ValidityClaim.All(
                    ValidityClaim.Direction(value: ends.Start), ValidityClaim.Direction(value: ends.End))))),
            softEdit: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.T), ValidityClaim.Finite(value: row.T)),
                (nameof(row.Delta), ValidityClaim.Finite(value: row.Delta)),
                (nameof(row.Length), ValidityClaim.Positive(value: row.Length))),
            periodicClose: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve))),
            subDFriendly: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curve), ModelClaim.Handle(handle: row.Curve)),
                (nameof(row.Structure), ValidityClaim.WhenPresent(
                    facet: row.Structure, claim: static shape => ValidityClaim.CountAtLeast(count: shape.PointCount, floor: 1)))),
            subDFriendlyPoints: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Points), ModelClaim.Points(points: row.Points))),
            compatible: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Curves), ModelClaim.Handles(handles: row.Curves)),
                (nameof(row.Ends), row.Ends.IsValid), (nameof(row.Law), row.Law.IsValid)),
            spiral: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Law), row.Law is { IsValid: true }),
                (nameof(row.RadiusPoint), ValidityClaim.Finite(value: row.RadiusPoint)),
                (nameof(row.Pitch), ValidityClaim.Finite(value: row.Pitch)),
                (nameof(row.TurnCount), ValidityClaim.All(ValidityClaim.Finite(value: row.TurnCount), row.TurnCount != 0.0)),
                (nameof(row.Radius0), ValidityClaim.Nonnegative(value: row.Radius0)),
                (nameof(row.Radius1), ValidityClaim.Nonnegative(value: row.Radius1))),
            parabola: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Seed), row.Seed is { IsValid: true })),
            arcBezier: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Degree), row.Degree is not null),
                (nameof(row.Center), ValidityClaim.Finite(value: row.Center)),
                (nameof(row.Start), ValidityClaim.Finite(value: row.Start)),
                (nameof(row.End), ValidityClaim.Finite(value: row.End)),
                (nameof(row.Radius), ValidityClaim.Positive(value: row.Radius))),
            analytic: static (op, row) => ModelClaim.Admits(row, op, (nameof(row.Seed), row.Seed is { IsValid: true })),
            catenary: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Start), ValidityClaim.Finite(value: row.Start)),
                (nameof(row.End), ValidityClaim.Finite(value: row.End)),
                (nameof(row.AxisDirection), ValidityClaim.Direction(value: row.AxisDirection)),
                (nameof(row.Law), row.Law is { IsValid: true }),
                (nameof(row.PointCount), ValidityClaim.CountAtLeast(count: row.PointCount, floor: 2))),
            makeEndsMeet: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.AdjustStart), row.AdjustStart is not null)),
            railFillet: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.FirstFace), ValidityClaim.CountAtLeast(count: row.FirstFace, floor: 0)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second)),
                (nameof(row.SecondFace), ValidityClaim.CountAtLeast(count: row.SecondFace, floor: 0)),
                (nameof(row.U), ValidityClaim.Finite(value: row.U)), (nameof(row.V), ValidityClaim.Finite(value: row.V)),
                (nameof(row.Law), row.Law.IsValid)),
            textOutlines: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Height), ValidityClaim.Positive(value: row.Height)),
                (nameof(row.Frame), row.Frame.IsValid),
                (nameof(row.SmallCapsScale), ValidityClaim.Positive(value: row.SmallCapsScale))),
            directionsMatch: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.First), ModelClaim.Handle(handle: row.First)),
                (nameof(row.Second), ModelClaim.Handle(handle: row.Second))));

    internal Fin<Built<CurveSlot>> Apply(Context domain) =>
        Switch(
            context: domain,
            offset: static (model, edit) => Borrowed(edit.Curve, Offset.SelfOp, (curve, op) =>
                edit.Frame.Switch(
                    state: (Curve: curve, Model: model, Edit: edit, Op: op),
                    inPlane: static (ctx, frame) => ModelGate.Many(ctx.Op, CurveSlot.Offset, () => ctx.Curve.Offset(
                        plane: frame.Value, distance: ctx.Edit.Distance.Value, tolerance: ctx.Model.Absolute.Value, cornerStyle: frame.Corner)),
                    byNormal: static (ctx, frame) => ModelGate.Many(ctx.Op, CurveSlot.Offset, () => ctx.Curve.Offset(
                        directionPoint: frame.DirectionPoint, normal: frame.Normal, distance: ctx.Edit.Distance.Value,
                        tolerance: ctx.Model.Absolute.Value, angleTolerance: ctx.Model.Angle.Value, loose: frame.Posture.Native,
                        cornerStyle: frame.Corner, endStyle: frame.End)))),
            offsetOnSurface: static (model, edit) => Borrowed(edit.Curve, OffsetOnSurface.SelfOp,
                (curve, op) => edit.Target.Apply(curve, model, op)),
            offsetLift: static (_, edit) => Borrowed(edit.Curve, OffsetLift.SelfOp, (curve, op) =>
                ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: edit.Surface, key: op, body: surface =>
                    edit.Lift.Switch(
                        state: (Curve: curve, Surface: surface, Edit: edit, Op: op),
                        normal: static ctx => ModelGate.Single(ctx.Op, CurveSlot.Lifted, () => ctx.Curve.OffsetNormalToSurface(
                            surface: ctx.Surface, height: ctx.Edit.Height.Value)),
                        tangent: static ctx => ModelGate.Single(ctx.Op, CurveSlot.Lifted, () => ctx.Curve.OffsetTangentToSurface(
                            surface: ctx.Surface, height: ctx.Edit.Height.Value))))),
            ribbon: static (model, edit) => Borrowed(edit.Curve, Ribbon.SelfOp, (curve, op) =>
                from parameters in edit.Law.Rig(domain: model, key: op)
                from built in op.Catch(() => {
                    Curve ribbon = curve.RibbonOffset(
                        ribbonParameters: parameters, railCurves: out Curve[] rails,
                        crossSectionCurves: out Curve[] sections, brepSurfaces: out Brep[] breps);
                    return ModelGate.Staged(op: op,
                        (CurveSlot.Ribboned, (GeometryBase[])[ribbon], false),
                        (CurveSlot.Rails, rails, true),
                        (CurveSlot.Sections, sections, true),
                        (CurveSlot.Surfaced, breps, true));
                })
                select built),
            fair: static (model, edit) => Borrowed(edit.Curve, Fair.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Fair(
                    distanceTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value,
                    clampStart: edit.ClampStart, clampEnd: edit.ClampEnd, iterations: edit.Iterations))),
            fit: static (model, edit) => Borrowed(edit.Curve, Fit.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Fit(degree: edit.Degree, fitTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            rebuild: static (_, edit) => Borrowed(edit.Curve, Rebuild.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => curve.Rebuild(pointCount: edit.PointCount, degree: edit.Degree, preserveTangents: edit.PreserveTangents))),
            smooth: static (_, edit) => Borrowed(edit.Curve, Smooth.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.Law.Apply(target: curve))),
            simplify: static (model, edit) => Borrowed(edit.Curve, Simplify.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.EndOnly.Case switch {
                    CurveEnd end => curve.SimplifyEnd(end: end, options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                    _ => curve.Simplify(options: edit.Options,
                        distanceTolerance: model.Absolute.Value, angleToleranceRadians: model.Angle.Value),
                })),
            edit: static (model, edit) => Borrowed(edit.Curve, Edit.SelfOp, (curve, op) =>
                op.Catch(() => {
                    Curve working = (Curve)curve.Duplicate();
                    Fin<Unit> changed = edit.Verb.Switch(
                        state: (Working: working, Domain: model, Op: op),
                        removeShort: static ctx => ctx.Op.Confirm(success: ctx.Working.RemoveShortSegments(tolerance: ctx.Domain.Absolute.Value)),
                        closeGap: static ctx => ctx.Op.Confirm(success: ctx.Working.MakeClosed(tolerance: ctx.Domain.Absolute.Value)),
                        trimDomain: static (ctx, law) => ctx.Op.Confirm(success: ctx.Working.TrimInterval(domain: law.Value)));
                    return changed.Bind(_ => ModelGate.Kept(op, CurveSlot.Refined, working)).Rollback(working);
                })),
            nurbsFit: static (model, edit) => Borrowed(edit.Curve, NurbsFit.SelfOp, (curve, op) =>
                edit.Law.Rig(domain: model, key: op).Bind(parameters => {
                    using (parameters) {
                        return op.Catch(() => {
                            NurbsCurve fitted = Curve.CreateNurbsCurveFit(
                                curve: curve, domain: edit.Domain, rebuildOptions: parameters,
                                maximumSeparation: out Line separation, thisSeparationParameter: out double atSource, nurbsSeparationParameter: out double atFit);
                            return ModelGate.Own(built: fitted, key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
                                Products: Seq(owned),
                                Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Refined, body: new BuildBody.Tally(Count: 1))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: separation.Length))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Marks(Points: Seq(separation.From, separation.To)))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: atSource))
                                    + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: atFit))));
                        });
                    }
                })),
            extend: static (_, edit) => Borrowed(edit.Curve, Extend.SelfOp, (curve, op) =>
                edit.Law.Switch(
                    state: (Curve: curve, Op: op),
                    byLength: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                        side: law.Side, length: law.Length, style: law.Style)),
                    toGeometry: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                            side: law.Side, style: law.Style, geometry: bounds.AsIterable()))),
                    toPoint: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.Extend(
                        side: law.Side, style: law.Style, endPoint: law.Terminal)),
                    byLine: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendByLine(
                            side: law.Side, geometry: bounds.AsIterable()))),
                    byArc: static (ctx, law) => ModelGate.BorrowMany<GeometryBase, Built<CurveSlot>>(handles: law.Bounds, key: ctx.Op,
                        body: bounds => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendByArc(
                            side: law.Side, geometry: bounds.AsIterable()))),
                    onSurface: static (ctx, law) => law.Face.Case switch {
                        int face => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Target, key: ctx.Op, body: host =>
                            from _ in guard(face < host.Faces.Count, ctx.Op.InvalidInput(axis: nameof(law.Face)))
                            from built in ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendOnSurface(side: law.Side, face: host.Faces[face]))
                            select built),
                        _ => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Target, key: ctx.Op,
                            body: host => ModelGate.Single(ctx.Op, CurveSlot.Extended, () => ctx.Curve.ExtendOnSurface(side: law.Side, surface: host))),
                    })),
            shorten: static (_, edit) => Borrowed(edit.Curve, Shorten.SelfOp, (curve, op) =>
                edit.Law.Switch(
                    state: (Curve: curve, Op: op),
                    toDomain: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(domain: law.Value)),
                    atEnd: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Shortened, () => ctx.Curve.Trim(side: law.Side, length: law.Length)))),
            split: static (model, edit) => Borrowed(edit.Curve, Split.SelfOp, (curve, op) =>
                edit.Cutter.Switch(
                    state: (Curve: curve, Model: model, Op: op),
                    atParameters: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(t: law.Values.AsIterable())),
                    byBrep: static (ctx, law) => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Value, key: ctx.Op, body: cutter =>
                        ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                            cutter: cutter, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value))),
                    bySurface: static (ctx, law) => ModelGate.Borrow<Surface, Built<CurveSlot>>(handle: law.Value, key: ctx.Op, body: cutter =>
                        ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                            cutter: cutter, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value))),
                    byPlane: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.SplitApart, () => ctx.Curve.Split(
                        plane: law.Value, tolerance: ctx.Model.Absolute.Value, angleToleranceRadians: ctx.Model.Angle.Value)))),
            // One arm per destination: the fit posture is a COLUMN the host's own `loose`-bearing overload reads, so
            // brep and mesh targets each reach one native instead of two arms differing by a literal.
            pull: static (model, edit) => Borrowed(edit.Curve, Pull.SelfOp, (curve, op) =>
                edit.Target.Switch(
                    state: (Curve: curve, Model: model, Op: op),
                    toFace: static (ctx, law) => ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: law.Brep, key: ctx.Op, body: host =>
                        from _ in guard(law.Face < host.Faces.Count, ctx.Op.InvalidInput(axis: nameof(law.Face)))
                        from built in ModelGate.Many(ctx.Op, CurveSlot.Pulled, () => Curve.PullToBrepFace(
                            curve: ctx.Curve, face: host.Faces[law.Face], tolerance: ctx.Model.Absolute.Value, loose: law.Posture.Native))
                        select built),
                    toMesh: static (ctx, law) => ModelGate.Borrow<Mesh, Built<CurveSlot>>(handle: law.Mesh, key: ctx.Op, body: mesh =>
                        ModelGate.Single(ctx.Op, CurveSlot.Pulled, () => ctx.Curve.PullToMesh(
                            mesh: mesh, tolerance: ctx.Model.Absolute.Value, loose: law.Posture.Native))))),
            project: static (model, edit) => edit.Target.Switch(
                state: (Model: model, Op: Project.SelfOp),
                toBreps: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                    ModelGate.BorrowMany<Brep, Built<CurveSlot>>(handles: law.Breps, key: ctx.Op, body: breps =>
                        ctx.Op.Catch(() => {
                            Curve[] projected = Curve.ProjectToBrep(
                                curves: curves.AsIterable(), breps: breps.AsIterable(), direction: law.Direction,
                                tolerance: ctx.Model.Absolute.Value,
                                curveIndices: out int[] curveMap, brepIndices: out int[] brepMap);
                            return ModelGate.Mapped(
                                op: ctx.Op,
                                slot: CurveSlot.Projected,
                                built: projected,
                                mapLength: projected.Length,
                                maps: [(SourceAxis.Curve, curveMap), (SourceAxis.Brep, brepMap)]);
                        }))),
                toMeshes: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                    ModelGate.BorrowMany<Mesh, Built<CurveSlot>>(handles: law.Meshes, key: ctx.Op, body: meshes =>
                        ModelGate.Many(ctx.Op, CurveSlot.Projected, () => Curve.ProjectToMesh(
                            curves: curves.AsIterable(), meshes: meshes.AsIterable(), direction: law.Direction,
                            tolerance: ctx.Model.Absolute.Value)))),
                toPlane: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Curve, key: ctx.Op, body: curve =>
                    ModelGate.Single(ctx.Op, CurveSlot.Projected, () => Curve.ProjectToPlane(curve: curve, plane: law.Plane)))),
            join: static (model, edit) => {
                Op op = Join.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    op.Catch(() => {
                        Curve[] joined = Curve.JoinCurves(
                            inputCurves: curves.AsIterable(), joinTolerance: model.Absolute.Value,
                            preserveDirection: edit.Grants.Admits(capability: JoinGrant.PreserveDirection),
                            simpleJoin: edit.Grants.Admits(capability: JoinGrant.SimpleJoin), key: out int[] map);
                        return ModelGate.Mapped(
                            op: op, slot: CurveSlot.Joined, built: joined, mapLength: curves.Count,
                            maps: [(SourceAxis.Input, map)]);
                    }));
            },
            boolean: static (model, edit) => edit.Law.Switch(
                state: (Model: model, Op: Boolean.SelfOp),
                union: static (ctx, law) => ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Curves, key: ctx.Op, body: curves =>
                    ctx.Op.Catch(() => {
                        Curve[] fused = Curve.CreateBooleanUnion(
                            curves: curves.AsIterable(), tolerance: ctx.Model.Absolute.Value, indexMap: out int[] map);
                        return ModelGate.Mapped(
                            op: ctx.Op, slot: CurveSlot.Booled, built: fused, mapLength: curves.Count,
                            maps: [(SourceAxis.Input, map)]);
                    })),
                intersection: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.First, key: ctx.Op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Second, key: ctx.Op, body: second =>
                        ModelGate.Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanIntersection(
                            curveA: first, curveB: second, tolerance: ctx.Model.Absolute.Value)))),
                difference: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.First, key: ctx.Op, body: first =>
                    ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: law.Subtractors, key: ctx.Op, body: subtractors =>
                        ModelGate.Many(ctx.Op, CurveSlot.Booled, () => Curve.CreateBooleanDifference(
                            curveA: first, subtractors: subtractors.AsIterable(), tolerance: ctx.Model.Absolute.Value))))),
            regions: static (model, edit) => {
                Op op = Regions.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    op.Catch(() => {
                        CurveBooleanRegions? acquired = edit.Points.IsEmpty
                            ? Curve.CreateBooleanRegions(curves: curves.AsIterable(), plane: edit.Frame, combineRegions: edit.Combine, tolerance: model.Absolute.Value)
                            : Curve.CreateBooleanRegions(curves: curves.AsIterable(), plane: edit.Frame, points: edit.Points.AsIterable(), combineRegions: edit.Combine, tolerance: model.Absolute.Value);
                        return Optional(acquired).ToFin(Fail: op.InvalidResult()).Bind(live => {
                            using (live) {
                                Seq<Seq<Curve>> regions = Counter(count: live.RegionCount)
                                    .Map(region => toSeq(live.RegionCurves(regionIndex: region)));
                                Seq<int> pointRegions = Counter(count: live.PointCount)
                                    .Map(point => live.RegionPointIndex(pointIndex: point));
                                Seq<(int Region, int Boundary, int Segment, int PlanarCurve, Interval Domain, bool Reversed)> segments =
                                    Counter(count: live.RegionCount).Bind(region =>
                                        Counter(count: live.BoundaryCount(region)).Bind(boundary =>
                                            Counter(count: live.SegmentCount(region, boundary)).Map(segment => {
                                                int planarCurve = live.SegmentDetails(region, boundary, segment, out Interval domain, out bool reversed);
                                                return (Region: region, Boundary: boundary, Segment: segment, PlanarCurve: planarCurve, Domain: domain, Reversed: reversed);
                                            })));
                                // Products concatenate the regions in order, so each region's slice is the running
                                // offset and its own count — one pass, never a per-region re-count of the prefix.
                                Seq<Seq<int>> groups = regions.Fold(
                                    state: (Offset: 0, Groups: Seq<Seq<int>>()),
                                    folder: static (state, region) => (
                                        Offset: state.Offset + region.Count,
                                        Groups: state.Groups + Seq(Counter(count: region.Count).Map(row => row + state.Offset))))
                                    .Groups;
                                return ModelGate.OwnMany(built: regions.Bind(static region => region), key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
                                    Products: owned,
                                    Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceGroups(
                                        Axis: SourceAxis.Region, Groups: groups))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.SourceMap(Axis: SourceAxis.Region, Rows: pointRegions))
                                        + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Regions, body: new BuildBody.RegionSegments(Rows: segments))));
                            }
                        });
                    }));
            },
            blend: static (_, edit) => {
                Op op = Blend.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Op: op),
                            endToEnd: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => law.Bulge.Case switch {
                                (double bulgeA, double bulgeB) => Curve.CreateBlendCurve(
                                    curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity, bulgeA: bulgeA, bulgeB: bulgeB),
                                _ => Curve.CreateBlendCurve(curveA: ctx.First, curveB: ctx.Second, continuity: law.Continuity),
                            }),
                            atParameters: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateBlendCurve(
                                curve0: ctx.First, t0: law.T0, reverse0: law.Reverse.First, continuity0: law.Continuity0,
                                curve1: ctx.Second, t1: law.T1, reverse1: law.Reverse.Second, continuity1: law.Continuity1)))));
            },
            arcBlend: static (_, edit) => edit.Law.Switch(
                state: (Edit: edit, Op: ArcBlend.SelfOp),
                controlPointRatio: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateArcBlend(
                    startPt: ctx.Edit.Start, startDir: ctx.Edit.StartDirection, endPt: ctx.Edit.End,
                    endDir: ctx.Edit.EndDirection, controlPointLengthRatio: law.Ratio.Value)),
                lineArcRadius: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Blended, () => Curve.CreateArcLineArcBlend(
                    startPt: ctx.Edit.Start, startDir: ctx.Edit.StartDirection, endPt: ctx.Edit.End,
                    endDir: ctx.Edit.EndDirection, radius: law.Radius.Value))),
            filletCurves: static (model, edit) => {
                Op op = FilletCurves.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Filleted, () => Curve.CreateFilletCurves(
                            curve0: first, point0: edit.NearFirst, curve1: second, point1: edit.NearSecond,
                            radius: edit.Radius,
                            join: edit.Grants.Admits(capability: FilletGrant.JoinResult),
                            trim: edit.Grants.Admits(capability: FilletGrant.TrimInputs),
                            arcExtension: edit.Grants.Admits(capability: FilletGrant.ArcExtension),
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            filletCorners: static (model, edit) => Borrowed(edit.Curve, FilletCorners.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Filleted, () => Curve.CreateFilletCornersCurve(
                    curve: curve, radius: edit.Radius, tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))),
            tween: static (model, edit) => {
                Op op = Tween.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        edit.Law.Switch(
                            state: (First: first, Second: second, Edit: edit, Model: model, Op: op),
                            plain: static ctx => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurves(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count, tolerance: ctx.Model.Absolute.Value)),
                            matched: static ctx => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurvesWithMatching(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count, tolerance: ctx.Model.Absolute.Value)),
                            sampled: static (ctx, law) => ModelGate.Many(ctx.Op, CurveSlot.Tweened, () => Curve.CreateTweenCurvesWithSampling(
                                curve0: ctx.First, curve1: ctx.Second, numCurves: ctx.Edit.Count,
                                numSamples: law.Samples, tolerance: ctx.Model.Absolute.Value)))));
            },
            matchCurve: static (_, edit) => {
                Op op = MatchCurve.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Matched, () => Curve.CreateMatchCurve(
                            curve0: first, reverse0: edit.Reverse.First, continuity: edit.Continuity,
                            curve1: second, reverse1: edit.Reverse.Second, preserve: edit.Preserve, average: edit.Average))));
            },
            mean: static (model, edit) => {
                Op op = Mean.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Single(op, CurveSlot.Constructed, () => Curve.CreateMeanCurve(curveA: first, curveB: second, angleToleranceRadians: model.Angle.Value))));
            },
            twoView: static (model, edit) => {
                Op op = TwoView.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        ModelGate.Many(op, CurveSlot.Constructed, () => Curve.CreateCurve2View(
                            curveA: first, curveB: second, vectorA: edit.FirstDirection, vectorB: edit.SecondDirection,
                            tolerance: model.Absolute.Value, angleTolerance: model.Angle.Value))));
            },
            interpolated: static (_, edit) => ModelGate.Single(Interpolated.SelfOp, CurveSlot.Constructed,
                () => (edit.Knots.Case, edit.Tangents.Case) switch {
                    (CurveKnotStyle knots, (Vector3d start, Vector3d end)) => Curve.CreateInterpolatedCurve(
                        points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots, startTangent: start, endTangent: end),
                    (CurveKnotStyle knots, _) => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree, knots: knots),
                    _ => Curve.CreateInterpolatedCurve(points: edit.Points.AsIterable(), degree: edit.Degree),
                }),
            controlPoints: static (_, edit) => ModelGate.Single(ControlPoints.SelfOp, CurveSlot.Constructed,
                () => Curve.CreateControlPointCurve(points: edit.Points.AsIterable(), degree: edit.Degree)),
            fitPoints: static (model, edit) => ModelGate.Single(FitPoints.SelfOp, CurveSlot.Constructed,
                () => edit.Constrained.Case switch {
                    (int degree, Vector3d start, Vector3d end) => NurbsCurve.CreateFromFitPoints(
                        points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, degree: degree,
                        periodic: edit.Periodic, startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateFromFitPoints(points: edit.Points.AsIterable(), tolerance: model.Absolute.Value, periodic: edit.Periodic),
                }),
            hSpline: static (_, edit) => ModelGate.Single(HSpline.SelfOp, CurveSlot.Constructed,
                () => edit.Tangents.Case switch {
                    (Vector3d start, Vector3d end) => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable(), startTangent: start, endTangent: end),
                    _ => NurbsCurve.CreateHSpline(points: edit.Points.AsIterable()),
                }),
            softEdit: static (_, edit) => Borrowed(edit.Curve, SoftEdit.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => Curve.CreateSoftEditCurve(
                    curve: curve, t: edit.T, delta: edit.Delta, length: edit.Length, fixEnds: edit.FixEnds))),
            periodicClose: static (_, edit) => Borrowed(edit.Curve, PeriodicClose.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => Curve.CreatePeriodicCurve(curve: curve, smooth: edit.Smooth))),
            subDFriendly: static (_, edit) => Borrowed(edit.Curve, SubDFriendly.SelfOp, (curve, op) =>
                ModelGate.Single(op, CurveSlot.Refined, () => edit.Structure.Case switch {
                    (int points, bool periodic) => NurbsCurve.CreateSubDFriendly(curve: curve, pointCount: points, periodicClosedCurve: periodic),
                    _ => NurbsCurve.CreateSubDFriendly(curve: curve),
                })),
            subDFriendlyPoints: static (_, edit) => ModelGate.Single(SubDFriendlyPoints.SelfOp, CurveSlot.Refined,
                () => NurbsCurve.CreateSubDFriendly(
                    points: edit.Points.AsIterable(), interpolatePoints: edit.Interpolate, periodicClosedCurve: edit.PeriodicClosed)),
            compatible: static (model, edit) => {
                Op op = Compatible.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<CurveSlot>>(handles: edit.Curves, key: op, body: curves =>
                    ModelGate.Many(op, CurveSlot.Refined, () => NurbsCurve.MakeCompatible(
                        curves: curves.AsIterable(), startPt: edit.Ends.StartOrUnset, endPt: edit.Ends.EndOrUnset,
                        simplifyMethod: edit.Law.SimplifyMethod, numPoints: edit.Law.PointCount,
                        refitTolerance: model.Absolute.Value, angleTolerance: model.Angle.Value)));
            },
            spiral: static (_, edit) => edit.Law.Switch(
                state: (Edit: edit, Op: Spiral.SelfOp),
                aboutAxis: static (ctx, law) => ModelGate.Single(ctx.Op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                    axisStart: law.AxisStart, axisDir: law.AxisDirection, radiusPoint: ctx.Edit.RadiusPoint,
                    pitch: ctx.Edit.Pitch, turnCount: ctx.Edit.TurnCount, radius0: ctx.Edit.Radius0, radius1: ctx.Edit.Radius1)),
                alongRail: static (ctx, law) => ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: law.Rail, key: ctx.Op, body: rail =>
                    ModelGate.Single(ctx.Op, CurveSlot.Constructed, () => NurbsCurve.CreateSpiral(
                        railCurve: rail, t0: law.T0, t1: law.T1, radiusPoint: ctx.Edit.RadiusPoint,
                        pitch: ctx.Edit.Pitch, turnCount: ctx.Edit.TurnCount, radius0: ctx.Edit.Radius0,
                        radius1: ctx.Edit.Radius1, pointsPerTurn: law.PointsPerTurn)))),
            parabola: static (_, edit) => edit.Seed.Switch(
                state: Parabola.SelfOp,
                fromVertex: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromVertex(
                    vertex: seed.Vertex, startPoint: seed.Start, endPoint: seed.End)),
                fromFocus: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromFocus(
                    focus: seed.Focus, startPoint: seed.Start, endPoint: seed.End)),
                fromPoints: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateParabolaFromPoints(
                    startPoint: seed.Start, innerPoint: seed.Inner, endPoint: seed.End))),
            arcBezier: static (_, edit) => ModelGate.Single(ArcBezier.SelfOp, CurveSlot.Constructed,
                () => NurbsCurve.CreateNonRationalArcBezier(
                    degree: edit.Degree.Key, center: edit.Center, start: edit.Start, end: edit.End,
                    radius: edit.Radius, tanSlider: edit.TanSlider.Value, midSlider: edit.MidSlider.Value)),
            analytic: static (_, edit) => edit.Seed.Switch(
                state: Analytic.SelfOp,
                ofLine: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromLine(line: seed.Value)),
                ofArc: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                    (int degree, int cvCount) => NurbsCurve.CreateFromArc(arc: seed.Value, degree: degree, cvCount: cvCount),
                    _ => NurbsCurve.CreateFromArc(arc: seed.Value),
                }),
                ofCircle: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => seed.Structure.Case switch {
                    (int degree, int cvCount) => NurbsCurve.CreateFromCircle(circle: seed.Value, degree: degree, cvCount: cvCount),
                    _ => NurbsCurve.CreateFromCircle(circle: seed.Value),
                }),
                ofEllipse: static (key, seed) => ModelGate.Single(key, CurveSlot.Constructed, () => NurbsCurve.CreateFromEllipse(ellipse: seed.Value))),
            catenary: static (_, edit) => {
                Op op = Catenary.SelfOp;
                return op.Catch(() => {
                    (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) result = edit.Law.Switch(
                        state: edit,
                        throughPoint: static (request, law) => Hung(native: Curve.CreateCatenaryCurveThroughPoint, request: request, shape: law.Value),
                        fromLength: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromLength, request: request, shape: law.Value),
                        fromParameter: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromParameter, request: request, shape: law.Value),
                        fromApex: static (request, law) => Hung(native: Curve.CreateCatenaryCurveFromApex, request: request, shape: law.Value));
                    return ModelGate.Own(built: result.Hung, key: op).Map(owned => Built<CurveSlot>.Of(operation: op,
                        Products: Seq(owned),
                        Evidence: BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Marks(Points: Seq(result.Apex)))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Parameter))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Length))
                            + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.Constructed, body: new BuildBody.Measure(Value: result.Deviation))));
                });
            },
            makeEndsMeet: static (_, edit) => {
                Op op = MakeEndsMeet.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() => {
                            Curve workingFirst = (Curve)first.Duplicate();
                            Curve workingSecond = (Curve)second.Duplicate();
                            return ModelGate.Staged(op: op, success: Curve.MakeEndsMeet(
                                curveA: workingFirst, adjustStartCurveA: edit.AdjustStart.First,
                                curveB: workingSecond, adjustStartCurveB: edit.AdjustStart.Second),
                                (CurveSlot.Reconciled, (GeometryBase[])[workingFirst, workingSecond], false))
                                .Rollback(workingFirst, workingSecond);
                        })));
            },
            railFillet: static (model, edit) => {
                Op op = RailFillet.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Rail, key: op, body: rail =>
                    ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                        ModelGate.Borrow<Brep, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                            from _ in guard(edit.FirstFace < first.Faces.Count, op.InvalidInput(axis: nameof(edit.FirstFace)))
                            from __ in guard(edit.SecondFace < second.Faces.Count, op.InvalidInput(axis: nameof(edit.SecondFace)))
                            from built in op.Catch(() => {
                                System.Collections.Generic.List<Brep> fillets = [];
                                System.Collections.Generic.List<Brep> trimmed0 = [];
                                System.Collections.Generic.List<Brep> trimmed1 = [];
                                return op.Confirm(success: rail.FilletSurfaceToRail(
                                        faceWithCurve: first.Faces[edit.FirstFace], secondFace: second.Faces[edit.SecondFace],
                                        u1: edit.U, v1: edit.V, railDegree: edit.Law.Rail.Key, arcDegree: edit.Law.Arc.Key,
                                        arcSliders: Seq(edit.Law.Sliders.Tangent.Value, edit.Law.Sliders.Inner.Value).AsIterable(),
                                        numBezierSrfs: edit.Law.BezierSurfaceCount,
                                        extend: edit.Law.Extend, split_type: edit.Law.Split, tolerance: model.Absolute.Value,
                                        out_fillets: fillets, out_breps0: trimmed0, out_breps1: trimmed1, fitResults: out double[] fit))
                                    .Bind(_ => ModelGate.Staged(op: op,
                                        extra: ModelFact.Answered(channel: fit).Match(
                                            Some: static values => values.Fold(BuildReceipt<CurveSlot>.Empty, static (receipt, value) =>
                                                receipt + BuildReceipt<CurveSlot>.Of(slot: CurveSlot.FitEvidence, body: new BuildBody.Measure(Value: value))),
                                            None: static () => BuildReceipt<CurveSlot>.Empty),
                                        (CurveSlot.Filleted, fillets, false),
                                        (CurveSlot.Trimmed0, trimmed0, true),
                                        (CurveSlot.Trimmed1, trimmed1, true)));
                            })
                            select built)));
            },
            textOutlines: static (model, edit) => {
                Op op = TextOutlines.SelfOp;
                return
                    from text in op.AcceptText(value: edit.Text)
                    from font in op.AcceptText(value: edit.Font)
                    from built in ModelGate.Many(op, CurveSlot.Outlined, () => Curve.CreateTextOutlines(
                        text: text, font: font, textHeight: edit.Height,
                        textStyle: edit.Faces.Mask(bit: static face => face.Bit),
                        closeLoops: edit.CloseLoops,
                        plane: edit.Frame, smallCapsScale: edit.SmallCapsScale, tolerance: model.Absolute.Value))
                    select built;
            },
            directionsMatch: static (_, edit) => {
                Op op = DirectionsMatch.SelfOp;
                return ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.First, key: op, body: first =>
                    ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: edit.Second, key: op, body: second =>
                        op.Catch(() => Fin.Succ(value: Built<CurveSlot>.Of(operation: op,
                            Products: Seq<GeometryHandle>(),
                            Evidence: BuildReceipt<CurveSlot>.Of(
                                slot: CurveSlot.DirectionVerdict,
                                body: new BuildBody.Flag(Value: Curve.DoDirectionsMatch(curveA: first, curveB: second))))))));
            });

    private static Fin<Built<CurveSlot>> Borrowed(GeometryHandle handle, Op op, Func<Curve, Op, Fin<Built<CurveSlot>>> body) =>
        ModelGate.Borrow<Curve, Built<CurveSlot>>(handle: handle, key: op, body: curve => body(curve, op));

    // `CurveBooleanRegions` publishes counts, not sequences, so its three nested rosters generate from the count
    // alone; an empty count yields an empty spread and no arm carries a bounds branch of its own.
    private static Seq<int> Counter(int count) => toSeq(Enumerable.Range(start: 0, count: count));

    private delegate Curve CatenaryNative<in TShape>(
        Point3d catenary_start, Point3d catenary_end, Vector3d axis_dir, TShape shape, bool bSmooth, int point_count,
        out Point3d apex_out, out double parameter_out, out double length_out, out double max_deviation_out);

    private static (Curve Hung, Point3d Apex, double Parameter, double Length, double Deviation) Hung<TShape>(
        CatenaryNative<TShape> native, Catenary request, TShape shape) {
        Curve hung = native(
            request.Start, request.End, request.AxisDirection, shape, request.Smooth, request.PointCount,
            out Point3d apex, out double parameter, out double length, out double deviation);
        return (Hung: hung, Apex: apex, Parameter: parameter, Length: length, Deviation: deviation);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class HostCurves {
    public static Fin<Built<CurveSlot>> Build(ModelRuntime runtime, params ReadOnlySpan<CurveOp> operations) =>
        ModelGate.Entry(
            runtime: runtime,
            operations: operations,
            admit: static (operation, key) => operation.Admitted(key: key),
            apply: static (operation, model) => operation.Apply(domain: model));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
