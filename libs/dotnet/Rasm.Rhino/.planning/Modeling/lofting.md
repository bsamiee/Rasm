# [RASM_RHINO_MODELING_LOFTING]

`Lofts.Build` owns loft, one- and two-rail sweep, direct and variational patch, developable construction, and ruling evidence. `LoftOp` admits rails, profiles, constraints, and seed surfaces once through the spine's `ModelClaim` fold; `CurveFit` owns the shared rebuild/refit axis; the spine's `ModelRuntime` carries the `ProgressLease` governance band into the one paced native; and every geometry product exits through `Built<LoftSlot>`. `SweepFrameLaw` remains the frozen seam consumed by `SubDOp.FromSweepOne`.

## [01]-[INDEX]

- [02]-[SWEEP]: `SweepFrameLaw`, `CurveFit`, `RefitTarget`, `SweepOneMode`, `SweepTwoStations`, `SweepTwoShapeFeature`, `SweepClosure`, `DevelopableLaw`, `RulingSolve`, `SweepEnds`, `CurveCompatibility` — the sweep modality vocabularies and the two terminal carriers the curve rail also composes.
- [03]-[PATCH]: `PatchEdge`, `PatchBehavior`, `VariationalEdgePolicy`, `PatchLaw`, `VariationalLaw`, `LoftTangency` — the patch and variational solver policies.
- [04]-[ALGEBRA]: `VariationalThreading`, `LoftSlot`, `LoftOp`, and the `Lofts.Build` entry.

## [02]-[SWEEP]

- Owner: `CurveFit` `[Union]` — the ONE rebuild/refit discriminant collapsing three native rebuild families onto one `(SweepRebuild, points, tolerance, refitRail)` quadruple; `SweepFrameLaw` — the roadlike frame seam `SubDOp.FromSweepOne` also consumes; `SweepOneMode` and `SweepTwoStations` — the per-rail station modalities; `SweepTwoShapeFeature` — the two-rail shape grants; `RefitTarget` and `SweepClosure` — the refit-rail and closed-sweep rows; `DevelopableLaw` and `RulingSolve` — the developable source and solver modalities; `SweepEnds` and `CurveCompatibility` — the admitted terminal pair and the compatibility policy, both read by `Modeling/curves.md`'s `CurveOp.Compatible`.
- Law: every union owns its OWN evidence off the generated `Switch` — the four free `TLaw?`-typed predicates that switched over a union's cases from outside it are deleted, because each closed on `_ => false` and turned a new case from a compile break into a silent refusal. Validity is a member of the union it proves, and the roster's `Admitted` reads it.
- Law: `SweepFrameLaw` carries BOTH host projections — `Native` answers the static overloads' `(frame, normal)` pair and `Rig` seats the engine's roadlike members, so a caller never chooses between the two spellings and the SubD consumer reads the same owner.
- Law: `SweepEnds` is admitted at CONSTRUCTION — its generated factory runs the same fold `IsValid` reads, so an unset-bearing terminal cannot enter the rail and be caught one layer later; `StartOrUnset`/`EndOrUnset` lower `None` to the host's documented `Point3d.Unset` omit spelling at the call, which is the one place the sentinel is legal.
- Law: the shape grants are a `CapabilitySet`, so the partitioned two-rail station mode states its legal corner as ONE value comparison against `CapabilitySet.Of(AutoAdjust)` rather than a set-equality probe over a frozen set whose record equality compares by reference.
- Growth: a new station modality is one case on its rail's union; a new sweep grant is one vocabulary row.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `SweepOneRail`/`SweepTwoRail` `:39-40,50-51`, `Brep.CreateFromSweep`/`CreateFromSweepSegmented`/`CreateFromSweepInParts`, `Brep.CreateFromLoft`/`CreateFromLoftRebuild`/`CreateFromLoftRefit`, `DevelopableSrf` `:139-141`), RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — `NurbsCurve.MakeCompatible` `:193`), kernel `Domain/rails` (`Op`, `ValidityClaim`, `IValidityEvidence`, `Fin`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.Geometry;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepFrameLaw : IValidityEvidence {
    private SweepFrameLaw() { }
    public sealed record Freeform : SweepFrameLaw;
    public sealed record RoadlikeTop : SweepFrameLaw;
    public sealed record RoadlikeFront : SweepFrameLaw;
    public sealed record RoadlikeRight : SweepFrameLaw;
    public sealed record RoadlikeDirection(Vector3d Normal) : SweepFrameLaw;

    public bool IsValid => Switch(
        freeform: static _ => (ValidityClaim)true,
        roadlikeTop: static _ => (ValidityClaim)true,
        roadlikeFront: static _ => (ValidityClaim)true,
        roadlikeRight: static _ => (ValidityClaim)true,
        roadlikeDirection: static law => ValidityClaim.Direction(value: law.Normal));

    internal (SweepFrame Frame, Vector3d Normal) Native => Switch(
        freeform: static _ => (SweepFrame.Freeform, Vector3d.Unset),
        roadlikeTop: static _ => (SweepFrame.Roadlike, Vector3d.ZAxis),
        roadlikeFront: static _ => (SweepFrame.Roadlike, Vector3d.YAxis),
        roadlikeRight: static _ => (SweepFrame.Roadlike, Vector3d.XAxis),
        roadlikeDirection: static law => (SweepFrame.Roadlike, law.Normal));

    internal Unit Rig(SweepOneRail engine) => Switch(
        engine,
        freeform: static _ => unit,
        roadlikeTop: static sweep => { sweep.SetToRoadlikeTop(); return unit; },
        roadlikeFront: static sweep => { sweep.SetToRoadlikeFront(); return unit; },
        roadlikeRight: static sweep => { sweep.SetToRoadlikeRight(); return unit; },
        roadlikeDirection: static (sweep, law) => { sweep.SetRoadlikeUpDirection(up: law.Normal); return unit; });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveFit : IValidityEvidence {
    private CurveFit() { }
    public sealed record AsIs : CurveFit;
    public sealed record Rebuild(int Points) : CurveFit;
    public sealed record Refit(RefitTarget Target) : CurveFit;

    internal bool IncludesRails => Switch(
        asIs: static _ => false,
        rebuild: static _ => false,
        refit: static law => law.Target == RefitTarget.SectionsAndRails);

    public bool IsValid => Switch(
        asIs: static _ => (ValidityClaim)true,
        rebuild: static law => ValidityClaim.CountAtLeast(count: law.Points, floor: 2),
        refit: static law => (ValidityClaim)(law.Target is not null));

    internal (SweepRebuild Kind, int Points, double Tolerance, bool RefitRail) Native(Context domain) => Switch(
        domain,
        asIs: static _ => (SweepRebuild.None, 0, 0.0, false),
        rebuild: static (_, law) => (SweepRebuild.Rebuild, law.Points, 0.0, false),
        refit: static (model, law) => (SweepRebuild.Refit, 0, model.Absolute.Value, law.Target.Native));
}

[SmartEnum<int>]
public sealed partial class RefitTarget {
    public static readonly RefitTarget Sections = new(key: 0, native: false);
    public static readonly RefitTarget SectionsAndRails = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepTwoStations : IValidityEvidence {
    private SweepTwoStations() { }
    public sealed record Static : SweepTwoStations;
    public sealed record Engine(Seq<double> Rail1, Seq<double> Rail2) : SweepTwoStations;
    public sealed record Partitioned(Seq<Point2d> RailParameters) : SweepTwoStations;

    public bool IsValid => Switch(
        @static: static _ => (ValidityClaim)true,
        engine: static law => ValidityClaim.All(
            ModelClaim.Rows(rows: law.Rail1, claim: static value => ValidityClaim.Finite(value: value), allowEmpty: true),
            ModelClaim.Rows(rows: law.Rail2, claim: static value => ValidityClaim.Finite(value: value), allowEmpty: true)),
        partitioned: static law => ModelClaim.Rows(
            rows: law.RailParameters, claim: static parameter => (ValidityClaim)parameter.IsValid, allowEmpty: true));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepOneMode : IValidityEvidence {
    private SweepOneMode() { }
    public sealed record Static : SweepOneMode;
    public sealed record Segmented : SweepOneMode;
    public sealed record Parameterized(Seq<double> ShapeParameters) : SweepOneMode;

    public bool IsValid => Switch(
        @static: static _ => (ValidityClaim)true,
        segmented: static _ => (ValidityClaim)true,
        parameterized: static law => ModelClaim.Rows(
            rows: law.ShapeParameters, claim: static parameter => ValidityClaim.Finite(value: parameter), allowEmpty: true));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SweepTwoShapeFeature : ICapability<SweepTwoShapeFeature> {
    public static readonly SweepTwoShapeFeature MaintainHeight = new(key: "maintain-height");
    public static readonly SweepTwoShapeFeature AutoAdjust = new(key: "auto-adjust");
}

[SmartEnum<int>]
public sealed partial class SweepClosure {
    public static readonly SweepClosure Open = new(key: 0, native: false);
    public static readonly SweepClosure Closed = new(key: 1, native: true);

    internal bool Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevelopableLaw : IValidityEvidence {
    private DevelopableLaw() { }
    public sealed record ByDensity(PairPosture Reverse, int Density) : DevelopableLaw;
    public sealed record ByRulings(Seq<Point2d> FixedRulings) : DevelopableLaw;

    public bool IsValid => Switch(
        byDensity: static law => ValidityClaim.All(
            law.Reverse is not null, ValidityClaim.CountAtLeast(count: law.Density, floor: 1)),
        byRulings: static law => ModelClaim.Rows(
            rows: law.FixedRulings, claim: static ruling => (ValidityClaim)ruling.IsValid));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RulingSolve : IValidityEvidence {
    private RulingSolve() { }
    public sealed record Local(Interval Domain0, Interval Domain1) : RulingSolve;
    public sealed record MinTwistSecond(Interval Domain1) : RulingSolve;
    public sealed record MinTwistBoth(Interval Domain0, Interval Domain1) : RulingSolve;

    public bool IsValid => Switch(
        local: static law => ValidityClaim.All(law.Domain0.IsValid, law.Domain1.IsValid),
        minTwistSecond: static law => (ValidityClaim)law.Domain1.IsValid,
        minTwistBoth: static law => ValidityClaim.All(law.Domain0.IsValid, law.Domain1.IsValid));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct SweepEnds : IValidityEvidence {
    public Option<Point3d> Start { get; }
    public Option<Point3d> End { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<Point3d> start,
        ref Option<Point3d> end) =>
        validationError = Admits(start: start, end: end)
            ? null
            : new ValidationError("A present sweep terminal must be a valid point.");

    public bool IsValid => Admits(start: Start, end: End);

    internal Point3d StartOrUnset => Start.IfNone(Point3d.Unset);
    internal Point3d EndOrUnset => End.IfNone(Point3d.Unset);

    private static ValidityClaim Admits(Option<Point3d> start, Option<Point3d> end) =>
        ValidityClaim.All(
            ValidityClaim.WhenPresent(facet: start, claim: static point => ValidityClaim.Finite(value: point)),
            ValidityClaim.WhenPresent(facet: end, claim: static point => ValidityClaim.Finite(value: point)));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CurveCompatibility : IValidityEvidence {
    public int SimplifyMethod { get; }
    public int PointCount { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int simplifyMethod,
        ref int pointCount) =>
        validationError = Admits(simplifyMethod: simplifyMethod, pointCount: pointCount)
            ? null
            : new ValidationError("Curve compatibility requires a valid simplifier and point count.");

    public bool IsValid => Admits(simplifyMethod: SimplifyMethod, pointCount: PointCount);

    private static ValidityClaim Admits(int simplifyMethod, int pointCount) =>
        ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: simplifyMethod, floor: 0),
            ValidityClaim.CountAtLeast(count: pointCount, floor: 2));
}
```

## [03]-[PATCH]

- Owner: `PatchLaw`, `VariationalLaw`, and `LoftTangency` — the three solver policies, each admitted at construction; `PatchEdge` and `PatchBehavior` — the patch grant vocabularies; `VariationalEdgePolicy` — the preserve-edges row.
- Law: the fixed-edge argument is an ORDERED projection off the roster — `PatchEdge`'s declaration order IS the host's `bool[4]` order (north, east, south, west), so `FixedEdges` folds the roster once and a fifth edge row extends the projection instead of adding a fifth hand `Contains`.
- Law: the tangency ends are ONE `PairPosture` value, and the empty corner is refused at admission — the host reads a start-tangent and end-tangent bool pair, `(false, false)` names a tangency constraint that constrains nothing, and a row-valued pair makes the two bools untransposable at the call.
- Law: `VariationalLaw.Rig` stays hand-seated — five of its seventeen target slots are regime bindings off `Context`, `InitialSurface` takes the BORROWED native rather than the policy's handle, and `PreserveEdges` reads a row projection, so a source-complete mapping cannot express the member set and a generated mapper carrying the residual ten beside them is the split form `Exchange/options.md` carves against.
- Law: the initial surface lowers through the ONE host-slot spelling — `Op.ToHostSlot` is where an absent optional becomes `null` for a host write, so `ValueUnsafe` (which throws on `None`) never stands in for it.
- Growth: a new solver weight is one column with its claim; a new patch grant is one vocabulary row.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `Brep.CreatePatch`, `Brep.VariationalPatchSettings` `:47`, `Brep.CreateVariationalPatch`, `Brep.CurveConstraint`/`PointConstraint`, `Brep.VariationalPatchResult`, `RhinoVariationalDomain`), kernel `Domain/rails` (`Op.ToHostSlot`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context.Absolute`, `Context.Angle`, `Context.Fractional`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatchEdge : ICapability<PatchEdge> {
    public static readonly PatchEdge North = new(key: "north");
    public static readonly PatchEdge East = new(key: "east");
    public static readonly PatchEdge South = new(key: "south");
    public static readonly PatchEdge West = new(key: "west");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatchBehavior : ICapability<PatchBehavior> {
    public static readonly PatchBehavior Trim = new(key: "trim");
    public static readonly PatchBehavior Tangency = new(key: "tangency");
}

[SmartEnum<int>]
public sealed partial class VariationalEdgePolicy {
    public static readonly VariationalEdgePolicy Free = new(key: 0, native: false);
    public static readonly VariationalEdgePolicy Preserve = new(key: 1, native: true);

    internal bool Native { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct PatchLaw : IValidityEvidence {
    public int USpans { get; }
    public int VSpans { get; }
    public CapabilitySet<PatchBehavior> Behavior { get; }
    public double PointSpacing { get; }
    public double Flexibility { get; }
    public double SurfacePull { get; }
    public CapabilitySet<PatchEdge> Edges { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int uSpans,
        ref int vSpans,
        ref CapabilitySet<PatchBehavior> behavior,
        ref double pointSpacing,
        ref double flexibility,
        ref double surfacePull,
        ref CapabilitySet<PatchEdge> edges) =>
        validationError = Admits(
                uSpans: uSpans, vSpans: vSpans, pointSpacing: pointSpacing,
                flexibility: flexibility, surfacePull: surfacePull)
            ? null
            : new ValidationError("Patch spans and solver weights are outside the admitted range.");

    public bool IsValid => Admits(
        uSpans: USpans, vSpans: VSpans, pointSpacing: PointSpacing,
        flexibility: Flexibility, surfacePull: SurfacePull);

    internal bool[] FixedEdges => [.. toSeq(PatchEdge.Items).Map(edge => Edges.Admits(capability: edge))];

    private static ValidityClaim Admits(
        int uSpans, int vSpans, double pointSpacing, double flexibility, double surfacePull) =>
        ValidityClaim.All(
            ValidityClaim.CountAtLeast(count: uSpans, floor: 1), ValidityClaim.CountAtLeast(count: vSpans, floor: 1),
            ValidityClaim.Positive(value: pointSpacing),
            ValidityClaim.Nonnegative(value: flexibility), ValidityClaim.Nonnegative(value: surfacePull));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct VariationalLaw : IValidityEvidence {
    public RhinoVariationalDomain Domain { get; }
    public int DegreeU { get; }
    public int DegreeV { get; }
    public int SpanCountU { get; }
    public int SpanCountV { get; }
    public double Stretching { get; }
    public double Bending { get; }
    public double RocBending { get; }
    public double UVRotation { get; }
    public int MaxRefinements { get; }
    public VariationalEdgePolicy Edges { get; }
    public Option<GeometryHandle> InitialSurface { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref RhinoVariationalDomain domain,
        ref int degreeU,
        ref int degreeV,
        ref int spanCountU,
        ref int spanCountV,
        ref double stretching,
        ref double bending,
        ref double rocBending,
        ref double uvRotation,
        ref int maxRefinements,
        ref VariationalEdgePolicy edges,
        ref Option<GeometryHandle> initialSurface) =>
        validationError = Admits(
                domain: domain, degreeU: degreeU, degreeV: degreeV, spanCountU: spanCountU, spanCountV: spanCountV,
                stretching: stretching, bending: bending, rocBending: rocBending, uvRotation: uvRotation,
                maxRefinements: maxRefinements, edges: edges, initialSurface: initialSurface)
            ? null
            : new ValidationError("Variational domain, degree, spans, weights, rotation, and refinements are outside the admitted range.");

    public bool IsValid => Admits(
        domain: Domain, degreeU: DegreeU, degreeV: DegreeV, spanCountU: SpanCountU, spanCountV: SpanCountV,
        stretching: Stretching, bending: Bending, rocBending: RocBending, uvRotation: UVRotation,
        maxRefinements: MaxRefinements, edges: Edges, initialSurface: InitialSurface);

    internal Fin<Brep.VariationalPatchSettings> Rig(Context domain, Option<Surface> initial, Op key) =>
        key.Catch(() => Fin.Succ(value: new Brep.VariationalPatchSettings {
            Tolerance = domain.Absolute.Value,
            AngleToleranceRadians = domain.Angle.Value,
            InternalTolerance = domain.Absolute.Value,
            CurvatureRelativeTolerance = domain.Fractional,
            CurvatureZeroTolerance = domain.Absolute.Value,
            DegreeU = DegreeU,
            DegreeV = DegreeV,
            SpanCountU = SpanCountU,
            SpanCountV = SpanCountV,
            Domain = Domain,
            Stretching = Stretching,
            Bending = Bending,
            RocBending = RocBending,
            UVRotation = UVRotation,
            MaxRefinements = MaxRefinements,
            InitialSurface = Op.ToHostSlot(value: initial),
            PreserveEdges = Edges.Native,
        }));

    private static ValidityClaim Admits(
        RhinoVariationalDomain domain,
        int degreeU,
        int degreeV,
        int spanCountU,
        int spanCountV,
        double stretching,
        double bending,
        double rocBending,
        double uvRotation,
        int maxRefinements,
        VariationalEdgePolicy? edges,
        Option<GeometryHandle> initialSurface) =>
        ValidityClaim.All(
            Enum.IsDefined(domain),
            ValidityClaim.CountAtLeast(count: degreeU, floor: 1), ValidityClaim.CountAtLeast(count: degreeV, floor: 1),
            ValidityClaim.CountAtLeast(count: spanCountU, floor: 1), ValidityClaim.CountAtLeast(count: spanCountV, floor: 1),
            edges is not null,
            ValidityClaim.Nonnegative(value: stretching), ValidityClaim.Nonnegative(value: bending),
            ValidityClaim.Nonnegative(value: rocBending), ValidityClaim.Finite(value: uvRotation),
            ValidityClaim.CountAtLeast(count: maxRefinements, floor: 0),
            ValidityClaim.WhenPresent(facet: initialSurface, claim: static handle => ModelClaim.Handle(handle: handle)));
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct LoftTangency : IValidityEvidence {
    public GeometryHandle StartOwner { get; }
    public int StartTrim { get; }
    public GeometryHandle EndOwner { get; }
    public int EndTrim { get; }
    public PairPosture Ends { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref GeometryHandle startOwner,
        ref int startTrim,
        ref GeometryHandle endOwner,
        ref int endTrim,
        ref PairPosture ends) =>
        validationError = Admits(
                startOwner: startOwner, startTrim: startTrim, endOwner: endOwner, endTrim: endTrim, ends: ends)
            ? null
            : new ValidationError("Loft tangency requires owners, non-negative trim indices, and at least one constrained end.");

    public bool IsValid => Admits(
        startOwner: StartOwner, startTrim: StartTrim, endOwner: EndOwner, endTrim: EndTrim, ends: Ends);

    private static ValidityClaim Admits(
        GeometryHandle? startOwner, int startTrim, GeometryHandle? endOwner, int endTrim, PairPosture? ends) =>
        ValidityClaim.All(
            ModelClaim.Handle(handle: startOwner), ModelClaim.Handle(handle: endOwner),
            ValidityClaim.CountAtLeast(count: startTrim, floor: 0), ValidityClaim.CountAtLeast(count: endTrim, floor: 0),
            ends is not null && ends != PairPosture.Neither);
}
```

## [04]-[ALGEBRA]

- Owner: `LoftSlot` `[SmartEnum<int>]` — the consequence vocabulary; `LoftOp` `[Union]` `[GenerateUnionOps]` — the sole construction algebra, each case carrying its generated `SelfOp`; `VariationalThreading` — the solver parallelism row; `Lofts` — the one entry, and the folder's runtime-bound variant of the spine.
- Law: the governance band is CONSUMED, never minted — the spine's `ModelRuntime` carries the regime, the cancellation token, and the optional fraction reporter a `ProgressLease` produces (`HostUi/shell.md` is the package's ONE producer), so an `IProgress` shim or a `CancellationTokenSource` minted beside a lease is the forked form. With no lease the token is `CancellationToken.None` and the reporter lowers to `null`, which is exactly what `Brep.CreateVariationalPatch` reads as an unpaced run.
- Law: the spine's `Context` is authoritative — `Apply` takes the regime the fold hands it AND the runtime that carries the band, so no arm reads a second context off the runtime and no parameter is discarded. Only the variational patch takes the band, so the token and reporter reach exactly one native and every other arm runs unpaced by the host's own shape.
- Law: `Lofts.Build` materializes the operation span ahead of the runtime bind — a span cannot cross the `Eff.runtime<ModelRuntime>()` lambda — then runs the spine's `ModelGate.Entry` over the sequence, so capture, the non-empty guard, accumulating admission, the fold, and the bench stamp are the spine's and `Built<LoftSlot>.Bench` carries harvest evidence.
- Law: admission NAMES its axis — `Admitted` dispatches the generated `Switch` into `ModelClaim.Admits`, so a sweep breaching station congruence AND rail refit reports both, and each nested `guard` inside an arm names the axis it gates instead of one shared refusal.
- Law: compatibility is the CURVE rail's — `NurbsCurve.MakeCompatible` has one call site in the folder (`CurveOp.Compatible`), and the loft verb that wrapped the same native behind a second slot is deleted; a caller batching compatibility before a loft composes two operations in one `Build` spread, which is what this rail's own flow already described.
- Law: variational evidence encodes native absence structurally — a nullable host channel lands a fact only through `ModelFact.Channel`, so an empty `Project` over its slot is the unknown verdict, a present `Flag`/`Text` fact is the answer, and an empty warning string stays distinct from a missing one.
- Law: ruling solves split by what the host RETURNS — `DevelopableSrf.RulingMinTwist` answers `bool` and folds through `Op.Confirm`, while `DevelopableSrf.GetLocalDevopableRuling` answers `int` and lands that count/status as a `Code` fact beside its `UvRows` pair, because discarding it drops the only qualification the solved pair carries.
- Growth: a new construction verb is one `LoftOp` case with its arm; a new solver channel is one `LoftSlot` row.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `SweepOneRail`/`SweepTwoRail` `:39-40,50-51`, `Brep.CreateFromSweep*`, `Brep.CreateFromLoft*`, `Brep.CreatePatch`, `Brep.CreateVariationalPatch` `:94-100`, `Brep.CreateDevelopableLoft`, `DevelopableSrf.GetLocalDevopableRuling`/`RulingMinTwist`/`UntwistRulings` `:139-141`), kernel `Domain/rails` (`Op`, `[GenerateUnionOps]` + generated `SelfOp`, `Fin`), `Modeling/curves.md` (`ModelClaim`, `ModelFact`), `Modeling/solids.md` (`ModelGate`, `ModelRuntime`, `Built<TSlot>`, `BuildReceipt<TSlot>`, `BuildBody`), LanguageExt.Core (`Eff.runtime`, `Zip`, `Seq`), Thinktecture.Runtime.Extensions.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VariationalThreading {
    public static readonly VariationalThreading Serial = new(key: 0, native: false);
    public static readonly VariationalThreading Parallel = new(key: 1, native: true);

    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class LoftSlot {
    public static readonly LoftSlot Swept = new(key: 0);
    public static readonly LoftSlot Lofted = new(key: 1);
    public static readonly LoftSlot Patched = new(key: 2);
    public static readonly LoftSlot Solved = new(key: 3);
    public static readonly LoftSlot Developed = new(key: 4);
    public static readonly LoftSlot Rulings = new(key: 5);
    public static readonly LoftSlot Warning = new(key: 6);
    public static readonly LoftSlot Error = new(key: 7);
    public static readonly LoftSlot G0Interior = new(key: 8);
    public static readonly LoftSlot G0 = new(key: 9);
    public static readonly LoftSlot G1 = new(key: 10);
    public static readonly LoftSlot G2 = new(key: 11);
}

[GenerateUnionOps]
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoftOp {
    private LoftOp() { }
    public sealed record SweepOne(
        GeometryHandle Rail, Seq<GeometryHandle> Shapes, SweepEnds Ends, SweepFrameLaw Frame,
        SweepClosure Closure, SweepBlend Blend, SweepMiter Miter, CurveFit Fit, SweepOneMode Mode) : LoftOp;
    public sealed record SweepTwo(
        GeometryHandle Rail1, GeometryHandle Rail2, Seq<GeometryHandle> Shapes, SweepEnds Ends,
        SweepClosure Closure, CurveFit Fit, CapabilitySet<SweepTwoShapeFeature> Shape, SweepTwoStations Stations) : LoftOp;
    public sealed record Loft(
        Seq<GeometryHandle> Shapes, SweepEnds Ends, LoftType Kind, SweepClosure Closure,
        CurveFit Fit, Option<LoftTangency> Tangency = default) : LoftOp;
    public sealed record Patch(Seq<GeometryHandle> Geometry, Option<GeometryHandle> StartingSurface, PatchLaw Law) : LoftOp;
    public sealed record Variational(
        Seq<(GeometryHandle Curve, Continuity Continuity)> Edges,
        Seq<(GeometryHandle Curve, Continuity Continuity)> InternalCurves,
        Seq<Point3d> Points, VariationalLaw Law,
        VariationalThreading Threading) : LoftOp;
    public sealed record Developable(GeometryHandle Rail0, GeometryHandle Rail1, DevelopableLaw Law) : LoftOp;
    public sealed record SolveRuling(GeometryHandle Rail0, GeometryHandle Rail1, Point2d Seed, RulingSolve Law) : LoftOp;
    public sealed record AdjustRulings(GeometryHandle Rail0, GeometryHandle Rail1, Seq<Point2d> Rulings) : LoftOp;

    internal Fin<LoftOp> Admitted(Op key) =>
        Switch(
            context: key,
            sweepOne: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Blend), Enum.IsDefined(row.Blend)),
                (nameof(row.Miter), Enum.IsDefined(row.Miter)),
                (nameof(row.Mode), row.Mode is { IsValid: true })),
            sweepTwo: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Rail2), ModelClaim.Handle(handle: row.Rail2)),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Stations), row.Stations is { IsValid: true })),
            loft: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Kind), Enum.IsDefined(row.Kind) && row.Kind != LoftType.Developable),
                (nameof(row.Tangency), ValidityClaim.Evidence(evidence: row.Tangency))),
            patch: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Geometry), ModelClaim.Handles(handles: row.Geometry)),
                (nameof(row.StartingSurface), ValidityClaim.WhenPresent(
                    facet: row.StartingSurface, claim: static handle => ModelClaim.Handle(handle: handle))),
                (nameof(row.Law), row.Law.IsValid)),
            variational: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Edges), Constraints(rows: row.Edges)),
                (nameof(row.InternalCurves), Constraints(rows: row.InternalCurves, allowEmpty: true)),
                (nameof(row.Points), ModelClaim.Points(points: row.Points, allowEmpty: true)),
                (nameof(row.Law), row.Law.IsValid),
                (nameof(row.Threading), row.Threading is not null)),
            developable: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail0), ModelClaim.Handle(handle: row.Rail0)),
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Law), row.Law is { IsValid: true })),
            solveRuling: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail0), ModelClaim.Handle(handle: row.Rail0)),
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Seed), row.Seed.IsValid),
                (nameof(row.Law), row.Law is { IsValid: true })),
            adjustRulings: static (op, row) => ModelClaim.Admits(row, op,
                (nameof(row.Rail0), ModelClaim.Handle(handle: row.Rail0)),
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Rulings), ModelClaim.Rows(rows: row.Rulings, claim: static ruling => (ValidityClaim)ruling.IsValid))));

    private static ValidityClaim Constraints(
        Seq<(GeometryHandle Curve, Continuity Continuity)> rows, bool allowEmpty = false) =>
        ModelClaim.Rows(rows: rows, allowEmpty: allowEmpty, claim: static row => ValidityClaim.All(
            ModelClaim.Handle(handle: row.Curve), Enum.IsDefined(row.Continuity)));

    internal Fin<Built<LoftSlot>> Apply(Context domain, ModelRuntime runtime) =>
        Switch(
            (Domain: domain, Runtime: runtime),
            sweepOne: static (model, edit) => {
                Op op = SweepOne.SelfOp;
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail, key: op, body: rail =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                        edit.Mode.Switch(
                            parameterized: parameterized =>
                                from _ in guard(parameterized.ShapeParameters.Count == shapes.Count, op.InvalidInput(axis: nameof(SweepOneMode.Parameterized.ShapeParameters)))
                                from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, op.InvalidInput(axis: nameof(edit.Ends)))
                                from ___ in guard(!edit.Fit.IncludesRails, op.InvalidInput(axis: nameof(edit.Fit)))
                                from built in op.Catch(() => {
                                    SweepOneRail engine = new() {
                                        SweepTolerance = model.Domain.Absolute.Value,
                                        AngleToleranceRadians = model.Domain.Angle.Value,
                                        ClosedSweep = edit.Closure.Native,
                                        GlobalShapeBlending = edit.Blend == SweepBlend.Global,
                                        MiterType = (int)edit.Miter,
                                    };
                                    _ = edit.Frame.Rig(engine: engine);
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => kind switch {
                                        SweepRebuild.Rebuild => engine.PerformSweepRebuild(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), points),
                                        SweepRebuild.Refit => engine.PerformSweepRefit(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), refit),
                                        _ => engine.PerformSweep(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable()),
                                    });
                                })
                                select built,
                            segmented: _ =>
                                from _ in guard(!edit.Fit.IncludesRails, op.InvalidInput(axis: nameof(edit.Fit)))
                                from built in op.Catch(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweepSegmented(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit));
                                })
                                select built,
                            @static: _ => op.Catch(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, bool refitRail) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweep(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit, refitRail: refitRail));
                                }))));
            },
            sweepTwo: static (model, edit) => {
                Op op = SweepTwo.SelfOp;
                return ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                    ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: edit.Rail2, key: op, body: rail2 =>
                        ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                            edit.Stations.Switch(
                                engine: stations =>
                                    from _ in guard(
                                        stations.Rail1.Count == shapes.Count && stations.Rail2.Count == shapes.Count,
                                        op.InvalidInput(axis: nameof(SweepTwoStations.Engine)))
                                    from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, op.InvalidInput(axis: nameof(edit.Ends)))
                                    from ___ in guard(!edit.Fit.IncludesRails, op.InvalidInput(axis: nameof(edit.Fit)))
                                    from built in op.Catch(() => {
                                        SweepTwoRail engine = new() {
                                            SweepTolerance = model.Domain.Absolute.Value,
                                            AngleToleranceRadians = model.Domain.Angle.Value,
                                            ClosedSweep = edit.Closure.Native,
                                            MaintainHeight = edit.Shape.Admits(capability: SweepTwoShapeFeature.MaintainHeight),
                                            AutoAdjust = edit.Shape.Admits(capability: SweepTwoShapeFeature.AutoAdjust),
                                        };
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                        return ModelGate.Many(op, LoftSlot.Swept, () => kind switch {
                                            SweepRebuild.Rebuild => engine.PerformSweepRebuild(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), points),
                                            SweepRebuild.Refit => engine.PerformSweepRefit(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), refit),
                                            _ => engine.PerformSweep(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable()),
                                        });
                                    })
                                    select built,
                                partitioned: stations =>
                                    from _ in guard(stations.RailParameters.Count == shapes.Count, op.InvalidInput(axis: nameof(SweepTwoStations.Partitioned.RailParameters)))
                                    from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, op.InvalidInput(axis: nameof(edit.Ends)))
                                    from ___ in guard(edit.Fit is CurveFit.AsIs, op.InvalidInput(axis: nameof(edit.Fit)))
                                    from ____ in guard(
                                        edit.Shape == CapabilitySet<SweepTwoShapeFeature>.Of(SweepTwoShapeFeature.AutoAdjust),
                                        op.InvalidInput(axis: nameof(edit.Shape)))
                                    from built in op.Catch(() => ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweepInParts(
                                        rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                        rail_params: stations.RailParameters.AsIterable(), closed: edit.Closure.Native, tolerance: model.Domain.Absolute.Value)))
                                    select built,
                                @static: _ =>
                                    from _ in guard(!edit.Fit.IncludesRails, op.InvalidInput(axis: nameof(edit.Fit)))
                                    from built in op.Catch(() => {
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                        return ModelGate.Many(op, LoftSlot.Swept, () => Brep.CreateFromSweep(
                                            rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                            start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset, closed: edit.Closure.Native,
                                            tolerance: model.Domain.Absolute.Value, rebuild: kind, rebuildPointCount: points, refitTolerance: refit,
                                            preserveHeight: edit.Shape.Admits(capability: SweepTwoShapeFeature.MaintainHeight),
                                            autoAdjust: edit.Shape.Admits(capability: SweepTwoShapeFeature.AutoAdjust)));
                                    })
                                    select built))));
            },
            loft: static (model, edit) => {
                Op op = Loft.SelfOp;
                return from _ in guard(!edit.Fit.IncludesRails, op.InvalidInput(axis: nameof(edit.Fit)))
                       from built in ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Shapes, key: op, body: shapes =>
                           edit.Tangency.Case switch {
                        LoftTangency tangency => ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.StartOwner, key: op, body: startOwner =>
                            ModelGate.Borrow<Brep, Built<LoftSlot>>(handle: tangency.EndOwner, key: op, body: endOwner =>
                                from _ in guard(tangency.StartTrim < startOwner.Trims.Count, op.InvalidInput(axis: nameof(tangency.StartTrim)))
                                from __ in guard(tangency.EndTrim < endOwner.Trims.Count, op.InvalidInput(axis: nameof(tangency.EndTrim)))
                                from ___ in guard(edit.Fit is CurveFit.AsIs, op.InvalidInput(axis: nameof(edit.Fit)))
                                from built in op.Catch(() => ModelGate.Many(op, LoftSlot.Lofted, () => Brep.CreateFromLoft(
                                    curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                    StartTangent: tangency.Ends.First,
                                    EndTangent: tangency.Ends.Second,
                                    StartTrim: startOwner.Trims[tangency.StartTrim], EndTrim: endOwner.Trims[tangency.EndTrim],
                                    loftType: edit.Kind, closed: edit.Closure.Native)))
                                select built)),
                        _ => op.Catch(() => ModelGate.Many(op, LoftSlot.Lofted, () => edit.Fit switch {
                            CurveFit.Rebuild fit => Brep.CreateFromLoftRebuild(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, rebuildPointCount: fit.Points),
                            CurveFit.Refit => Brep.CreateFromLoftRefit(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, refitTolerance: model.Domain.Absolute.Value),
                            _ => Brep.CreateFromLoft(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value),
                        })),
                    })
                       select built;
            },
            patch: static (model, edit) => {
                Op op = Patch.SelfOp;
                return ModelGate.BorrowMany<GeometryBase, Built<LoftSlot>>(handles: edit.Geometry, key: op, body: constraints =>
                    edit.StartingSurface.Case switch {
                        GeometryHandle starting => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: starting, key: op,
                            body: surface => Patched(op: op, edit: edit, constraints: constraints, starting: Some(surface), model: model.Domain)),
                        _ => Patched(op: op, edit: edit, constraints: constraints, starting: Option<Surface>.None, model: model.Domain),
                    });
            },
            variational: static (model, edit) => {
                Op op = Variational.SelfOp;
                return ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.Edges.Map(static row => row.Curve), key: op, body: edgeCurves =>
                    ModelGate.BorrowMany<Curve, Built<LoftSlot>>(handles: edit.InternalCurves.Map(static row => row.Curve), key: op, allowEmpty: true, body: interiorCurves => {
                        Fin<Built<LoftSlot>> Solve(Option<Surface> initial) =>
                            from settings in edit.Law.Rig(domain: model.Domain, initial: initial, key: op)
                            from built in op.Catch(() => {
                                Brep patch = Brep.CreateVariationalPatch(
                                    edges: edgeCurves.Zip(edit.Edges.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    internalCurves: interiorCurves.Zip(edit.InternalCurves.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    points: edit.Points.Map(static point => new Brep.PointConstraint(point: point)).AsIterable(),
                                    settings: settings, multiThreading: edit.Threading.Native,
                                    cancelToken: model.Runtime.Cancellation,
                                    progress: Op.ToHostSlot(value: model.Runtime.ScalarProgress),
                                    results: out Brep.VariationalPatchResult verdict);
                                return ModelGate.Own(built: patch, key: op).Map(owned => Built<LoftSlot>.Of(
                                    operation: op,
                                    Products: Seq(owned),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Solved, body: new BuildBody.Tally(Count: 1))
                                        + ModelFact.Channel(slot: LoftSlot.Warning, value: Optional(verdict.Warning).Map(static detail => (BuildBody)new BuildBody.Text(Value: detail)))
                                        + ModelFact.Channel(slot: LoftSlot.Error, value: Optional(verdict.Error).Map(static detail => (BuildBody)new BuildBody.Text(Value: detail)))
                                        + ModelFact.Channel(slot: LoftSlot.G0Interior, value: Optional(verdict.G0Int).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + ModelFact.Channel(slot: LoftSlot.G0, value: Optional(verdict.G0).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + ModelFact.Channel(slot: LoftSlot.G1, value: Optional(verdict.G1).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))
                                        + ModelFact.Channel(slot: LoftSlot.G2, value: Optional(verdict.G2).Map(static held => (BuildBody)new BuildBody.Flag(Value: held)))));
                            }, token: model.Runtime.Cancellation)
                            select built;
                        return edit.Law.InitialSurface.Case switch {
                            GeometryHandle seed => ModelGate.Borrow<Surface, Built<LoftSlot>>(handle: seed, key: op, body: surface => Solve(initial: Some(surface))),
                            _ => Solve(initial: Option<Surface>.None),
                        };
                    }));
            },
            developable: static (_, edit) => edit.Law.Switch(
                (Edit: edit, Op: Developable.SelfOp),
                byDensity: static (ctx, law) => ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                    ModelGate.Borrow<Curve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                        ModelGate.Many(ctx.Op, LoftSlot.Developed, () => Brep.CreateDevelopableLoft(
                            crv0: rail0,
                            crv1: rail1,
                            reverse0: law.Reverse.First,
                            reverse1: law.Reverse.Second,
                            density: law.Density)))),
                byRulings: static (ctx, law) => ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail0, key: ctx.Op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: ctx.Edit.Rail1, key: ctx.Op, body: rail1 =>
                        ModelGate.Many(ctx.Op, LoftSlot.Developed, () => Brep.CreateDevelopableLoft(
                            rail0: rail0, rail1: rail1, fixedRulings: law.FixedRulings.AsIterable()))))),
            solveRuling: static (_, edit) => {
                Op op = SolveRuling.SelfOp;
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        edit.Law.Switch(
                            (Rail0: rail0, Rail1: rail1, Seed: edit.Seed, Op: op),
                            local: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                int verdict = DevelopableSrf.GetLocalDevopableRuling(
                                    rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                    rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                    t0_out: ref t0, t1_out: ref t1);
                                return Fin.Succ(value: RulingBuilt(operation: ctx.Op, t0: t0, t1: t1, code: Some(verdict)));
                            }),
                            minTwistSecond: static (ctx, law) => ctx.Op.Catch(() => {
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, rail1: ctx.Rail1, t1: ctx.Seed.Y,
                                        dom1: law.Domain1, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(operation: ctx.Op, t0: ctx.Seed.X, t1: t1, cosine: Some(cosine)));
                            }),
                            minTwistBoth: static (ctx, law) => ctx.Op.Catch(() => {
                                double t0 = ctx.Seed.X;
                                double t1 = ctx.Seed.Y;
                                double cosine = 0.0;
                                return ctx.Op.Confirm(success: DevelopableSrf.RulingMinTwist(
                                        rail0: ctx.Rail0, t0: ctx.Seed.X, dom0: law.Domain0,
                                        rail1: ctx.Rail1, t1: ctx.Seed.Y, dom1: law.Domain1,
                                        t0_out: ref t0, t1_out: ref t1, cos_twist_out: ref cosine))
                                    .Map(_ => RulingBuilt(operation: ctx.Op, t0: t0, t1: t1, cosine: Some(cosine)));
                            }))));
            },
            adjustRulings: static (_, edit) => {
                Op op = AdjustRulings.SelfOp;
                return ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail0, key: op, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Built<LoftSlot>>(handle: edit.Rail1, key: op, body: rail1 =>
                        op.Catch(() => {
                            System.Collections.Generic.IEnumerable<Point2d> rulings = edit.Rulings.AsIterable();
                            return op.Confirm(success: DevelopableSrf.UntwistRulings(rail0: rail0, rail1: rail1, rulings: ref rulings))
                                .Map(_ => Built<LoftSlot>.Of(
                                    operation: op,
                                    Products: Seq<GeometryHandle>(),
                                    Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: toSeq(rulings)))));
                        })));
            });

    private static Fin<Built<LoftSlot>> Patched(
        Op op, Patch edit, Seq<GeometryBase> constraints, Option<Surface> starting, Context model) =>
        ModelGate.Single(op, LoftSlot.Patched, () => Brep.CreatePatch(
            geometry: constraints.AsIterable(), startingSurface: Op.ToHostSlot(value: starting),
            uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans,
            trim: edit.Law.Behavior.Admits(capability: PatchBehavior.Trim),
            tangency: edit.Law.Behavior.Admits(capability: PatchBehavior.Tangency),
            pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
            fixEdges: edit.Law.FixedEdges, tolerance: model.Absolute.Value));

    private static Built<LoftSlot> RulingBuilt(
        Op operation, double t0, double t1, Option<double> cosine = default, Option<int> code = default) =>
        Built<LoftSlot>.Of(
            operation: operation,
            Products: Seq<GeometryHandle>(),
            Evidence: BuildReceipt<LoftSlot>.Of(slot: LoftSlot.Rulings, body: new BuildBody.UvRows(Rows: Seq(new Point2d(t0, t1))))
                + ModelFact.Channel(slot: LoftSlot.Rulings, value: cosine.Map(static value => (BuildBody)new BuildBody.Measure(Value: value)))
                + ModelFact.Channel(slot: LoftSlot.Rulings, value: code.Map(static value => (BuildBody)new BuildBody.Code(Value: value))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lofts {
    public static Eff<ModelRuntime, Built<LoftSlot>> Build(params ReadOnlySpan<LoftOp> operations) {
        Seq<LoftOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(key: key),
                apply: (operation, model) => operation.Apply(domain: model, runtime: runtime)).ToEff());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
