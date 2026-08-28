# [RASM_RHINO_MODELING_LOFTING]

`Lofts.Build` owns loft, one- and two-rail sweep, direct and variational patch, and developable construction. `LoftOp` admits rails, profiles, constraints, and seed surfaces once through the spine's `ModelClaim` fold; `CurveFit` owns the shared rebuild/refit axis; and the spine's `ModelRuntime` carries the `ProgressLease` governance band into the one paced native. `SweepFrameLaw` remains the frozen contract consumed by `SubDOp.FromSweepOne`.

## [01]-[INDEX]

- [02]-[SWEEP]: `SweepFrameLaw`, `CurveFit`, `RefitTarget`, `SweepOneMode`, `SweepTwoStations`, `SweepTwoShapeFeature`, `SweepClosure`, `DevelopableLaw`, `SweepEnds`, `CurveCompatibility` — the sweep modality vocabularies and the two terminal carriers the curve pipeline also composes.
- [03]-[PATCH]: `PatchEdge`, `PatchBehavior`, `VariationalEdgePolicy`, `PatchLaw`, `VariationalLaw`, `LoftTangency` — the patch and variational solver policies.
- [04]-[ALGEBRA]: `VariationalThreading`, `LoftOp`, and the `Lofts.Build` entry.

## [02]-[SWEEP]

- Owner: `CurveFit` `[Union]` — the ONE rebuild/refit discriminant collapsing three native rebuild families onto one `(SweepRebuild, points, tolerance, refitRail)` quadruple; `SweepFrameLaw` — the roadlike frame contract `SubDOp.FromSweepOne` also consumes; `SweepOneMode` and `SweepTwoStations` — the per-rail station modalities; `SweepTwoShapeFeature` — the two-rail shape grants; `RefitTarget` and `SweepClosure` — the refit-rail and closed-sweep rows; `DevelopableLaw` — the developable source modality; `SweepEnds` and `CurveCompatibility` — the admitted terminal pair and the compatibility policy, both read by `Modeling/curves.md`'s `CurveOp.Compatible`.
- Law: every union owns its OWN evidence off the generated `Switch` — the four free `TLaw?`-typed predicates that switched over a union's cases from outside it are deleted, because each closed on `_ => false` and turned a new case from a compile break into a silent refusal. Validity is a member of the union it proves, and the roster's `Admitted` reads it.
- Law: `SweepFrameLaw` carries BOTH host projections — `Native` answers the static overloads' `(frame, normal)` pair and `Rig` seats the engine's roadlike members, so a caller never chooses between the two spellings and the SubD consumer reads the same owner.
- Law: `SweepEnds` is admitted at CONSTRUCTION — its generated factory runs the same fold `IsValid` reads, so an unset-bearing terminal cannot enter the pipeline and be caught one layer later; `StartOrUnset`/`EndOrUnset` lower `None` to the host's documented `Point3d.Unset` omit spelling at the call, which is the one place the sentinel is legal.
- Law: the shape grants are a `CapabilitySet`, so the partitioned two-rail station mode states its legal corner as ONE value comparison against `CapabilitySet.Of(AutoAdjust)` rather than a set-equality probe over a frozen set whose record equality compares by reference.
- Growth: a new station modality is one case on its rail's union; a new sweep grant is one vocabulary row.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `SweepOneRail`/`SweepTwoRail` `:39-40,50-51`, `Brep.CreateFromSweep`/`CreateFromSweepSegmented`/`CreateFromSweepInParts`, `Brep.CreateFromLoft`/`CreateFromLoftRebuild`/`CreateFromLoftRefit`, `DevelopableSrf` `:139-141`), RhinoCommon surfacing (`.api/api-rhinocommon-surfacing.md` — `NurbsCurve.MakeCompatible` `:193`), kernel `Domain/results` (`ValidityClaim`, `IValidityEvidence`, `Fin`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

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
- Law: the initial surface lowers through the ONE host-slot spelling — `HostEdge.Slot` is where an absent optional becomes `null` for a host write, so `ValueUnsafe` (which throws on `None`) never stands in for it.
- Growth: a new solver weight is one column with its claim; a new patch grant is one vocabulary row.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `Brep.CreatePatch`, `Brep.VariationalPatchSettings` `:47`, `Brep.CreateVariationalPatch`, `Brep.CurveConstraint`/`PointConstraint`, `Brep.VariationalPatchResult`, `RhinoVariationalDomain`), kernel `Domain/results` (`HostEdge.Slot`, `ValidityClaim`, `IValidityEvidence`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), kernel `Domain/context` (`Context.Absolute`, `Context.Angle`, `Context.Fractional`), `Modeling/curves.md` (`ModelClaim`, `PairPosture`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
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

    internal Fin<Brep.VariationalPatchSettings> Rig(Context domain, Option<Surface> initial) =>
        Try.lift(() => Fin.Succ(value: new Brep.VariationalPatchSettings {
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
            InitialSurface = HostEdge.Slot(value: initial),
            PreserveEdges = Edges.Native,
        })).Run().Bind(static inner => inner);

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

- Owner: `LoftOp` `[Union]` — the sole construction algebra; `VariationalThreading` — the solver parallelism row; `Lofts` — the one entry, and the folder's runtime-bound variant of the spine.
- Law: the governance band is CONSUMED, never minted — the spine's `ModelRuntime` carries the regime, the cancellation token, and the optional fraction reporter a `ProgressLease` produces (`HostUi/shell.md` is the package's ONE producer), so an `IProgress` shim or a `CancellationTokenSource` minted beside a lease is the forked form. With no lease the token is `CancellationToken.None` and the reporter lowers to `null`, which is exactly what `Brep.CreateVariationalPatch` reads as an unpaced run.
- Law: the spine's `Context` is authoritative — `Apply` takes the regime the fold hands it AND the runtime that carries the band, so no arm reads a second context off the runtime and no parameter is discarded. Only the variational patch takes the band, so the token and reporter reach exactly one native and every other arm runs unpaced by the host's own shape.
- Law: `Lofts.Build` materializes the operation span ahead of the runtime bind — a span cannot cross the `Eff.runtime<ModelRuntime>()` lambda — then runs the spine's `ModelGate.Entry` over the sequence, so capture, the non-empty guard, accumulating admission, and custody-safe product fold remain spine-owned.
- Law: admission NAMES its axis — `Admitted` dispatches the generated `Switch` into `ModelClaim.Admits`, so a sweep breaching station congruence AND rail refit reports both, and each nested `guard` inside an arm names the axis it gates instead of one shared refusal.
- Law: compatibility is the CURVE pipeline's — `NurbsCurve.MakeCompatible` has one call site in the folder (`CurveOp.Compatible`), and the loft verb that wrapped the same native behind a second slot is deleted; a caller batching compatibility before a loft composes two operations in one `Build` spread, which is what this pipeline's own flow already described.
- Growth: a new construction verb is one `LoftOp` case with its arm.
- Packages: RhinoCommon solids (`.api/api-rhinocommon-solids.md` — `SweepOneRail`/`SweepTwoRail` `:39-40,50-51`, `Brep.CreateFromSweep*`, `Brep.CreateFromLoft*`, `Brep.CreatePatch`, `Brep.CreateVariationalPatch` `:94-100`, `Brep.CreateDevelopableLoft`), kernel `Domain/results` (`Fin`), `Modeling/curves.md` (`ModelClaim`), `Modeling/solids.md` (`ModelGate`, `ModelRuntime`), LanguageExt.Core (`Eff.runtime`, `Zip`, `Seq`), Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VariationalThreading {
    public static readonly VariationalThreading Serial = new(key: 0, native: false);
    public static readonly VariationalThreading Parallel = new(key: 1, native: true);

    internal bool Native { get; }
}

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

    internal Fin<LoftOp> Admitted() =>
        Switch(
            sweepOne: static (row) => ModelClaim.Admits(row,
                (nameof(row.Rail), ModelClaim.Handle(handle: row.Rail)),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Frame), row.Frame is { IsValid: true }),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Blend), Enum.IsDefined(row.Blend)),
                (nameof(row.Miter), Enum.IsDefined(row.Miter)),
                (nameof(row.Mode), row.Mode is { IsValid: true })),
            sweepTwo: static (row) => ModelClaim.Admits(row,
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Rail2), ModelClaim.Handle(handle: row.Rail2)),
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Stations), row.Stations is { IsValid: true })),
            loft: static (row) => ModelClaim.Admits(row,
                (nameof(row.Shapes), ModelClaim.Handles(handles: row.Shapes)),
                (nameof(row.Ends), row.Ends.IsValid),
                (nameof(row.Closure), row.Closure is not null),
                (nameof(row.Fit), row.Fit is { IsValid: true }),
                (nameof(row.Kind), Enum.IsDefined(row.Kind) && row.Kind != LoftType.Developable),
                (nameof(row.Tangency), ValidityClaim.Evidence(evidence: row.Tangency))),
            patch: static (row) => ModelClaim.Admits(row,
                (nameof(row.Geometry), ModelClaim.Handles(handles: row.Geometry)),
                (nameof(row.StartingSurface), ValidityClaim.WhenPresent(
                    facet: row.StartingSurface, claim: static handle => ModelClaim.Handle(handle: handle))),
                (nameof(row.Law), row.Law.IsValid)),
            variational: static (row) => ModelClaim.Admits(row,
                (nameof(row.Edges), Constraints(rows: row.Edges)),
                (nameof(row.InternalCurves), Constraints(rows: row.InternalCurves, allowEmpty: true)),
                (nameof(row.Points), ModelClaim.Points(points: row.Points, allowEmpty: true)),
                (nameof(row.Law), row.Law.IsValid),
                (nameof(row.Threading), row.Threading is not null)),
            developable: static (row) => ModelClaim.Admits(row,
                (nameof(row.Rail0), ModelClaim.Handle(handle: row.Rail0)),
                (nameof(row.Rail1), ModelClaim.Handle(handle: row.Rail1)),
                (nameof(row.Law), row.Law is { IsValid: true })));

    private static ValidityClaim Constraints(
        Seq<(GeometryHandle Curve, Continuity Continuity)> rows, bool allowEmpty = false) =>
        ModelClaim.Rows(rows: rows, allowEmpty: allowEmpty, claim: static row => ValidityClaim.All(
            ModelClaim.Handle(handle: row.Curve), Enum.IsDefined(row.Continuity)));

    internal Fin<Seq<GeometryHandle>> Apply(Context domain, ModelRuntime runtime) =>
        Switch(
            (Domain: domain, Runtime: runtime),
            sweepOne: static (model, edit) => {
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Rail, body: rail =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Shapes, body: shapes =>
                        edit.Mode.Switch(
                            parameterized: parameterized =>
                                from _ in guard(parameterized.ShapeParameters.Count == shapes.Count, new KernelFault.InvalidInput(Axis: Some(nameof(SweepOneMode.Parameterized.ShapeParameters))))
                                from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Ends))))
                                from ___ in guard(!edit.Fit.IncludesRails, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                from built in Try.lift(() => {
                                    SweepOneRail engine = new() {
                                        SweepTolerance = model.Domain.Absolute.Value,
                                        AngleToleranceRadians = model.Domain.Angle.Value,
                                        ClosedSweep = edit.Closure.Native,
                                        GlobalShapeBlending = edit.Blend == SweepBlend.Global,
                                        MiterType = (int)edit.Miter,
                                    };
                                    _ = edit.Frame.Rig(engine: engine);
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, () => kind switch {
                                        SweepRebuild.Rebuild => engine.PerformSweepRebuild(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), points),
                                        SweepRebuild.Refit => engine.PerformSweepRefit(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable(), refit),
                                        _ => engine.PerformSweep(rail, shapes.AsIterable(), parameterized.ShapeParameters.AsIterable()),
                                    });
                                }).Run().Bind(static inner => inner)
                                select built,
                            segmented: _ =>
                                from _ in guard(!edit.Fit.IncludesRails, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                from built in Try.lift(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, () => Brep.CreateFromSweepSegmented(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit));
                                }).Run().Bind(static inner => inner)
                                select built,
                            @static: _ => Try.lift(() => {
                                    (SweepFrame frame, Vector3d normal) = edit.Frame.Native;
                                    (SweepRebuild kind, int points, double refit, bool refitRail) = edit.Fit.Native(domain: model.Domain);
                                    return ModelGate.Many(op, () => Brep.CreateFromSweep(
                                        rail: rail, shapes: shapes.AsIterable(), startPoint: edit.Ends.StartOrUnset, endPoint: edit.Ends.EndOrUnset,
                                        frameType: frame, roadlikeNormal: normal, closed: edit.Closure.Native, blendType: edit.Blend, miterType: edit.Miter,
                                        tolerance: model.Domain.Absolute.Value, rebuildType: kind, rebuildPointCount: points, refitTolerance: refit, refitRail: refitRail));
                                }).Run().Bind(static inner => inner))));
            },
            sweepTwo: static (model, edit) => {
                return ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Rail1, body: rail1 =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: edit.Rail2, body: rail2 =>
                        ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Shapes, body: shapes =>
                            edit.Stations.Switch(
                                engine: stations =>
                                    from _ in guard(
                                        stations.Rail1.Count == shapes.Count && stations.Rail2.Count == shapes.Count,
                                        new KernelFault.InvalidInput(Axis: Some(nameof(SweepTwoStations.Engine))))
                                    from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Ends))))
                                    from ___ in guard(!edit.Fit.IncludesRails, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                    from built in Try.lift(() => {
                                        SweepTwoRail engine = new() {
                                            SweepTolerance = model.Domain.Absolute.Value,
                                            AngleToleranceRadians = model.Domain.Angle.Value,
                                            ClosedSweep = edit.Closure.Native,
                                            MaintainHeight = edit.Shape.Admits(capability: SweepTwoShapeFeature.MaintainHeight),
                                            AutoAdjust = edit.Shape.Admits(capability: SweepTwoShapeFeature.AutoAdjust),
                                        };
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                        return ModelGate.Many(op, () => kind switch {
                                            SweepRebuild.Rebuild => engine.PerformSweepRebuild(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), points),
                                            SweepRebuild.Refit => engine.PerformSweepRefit(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable(), refit),
                                            _ => engine.PerformSweep(
                                                rail1, rail2, shapes.AsIterable(), stations.Rail1.AsIterable(), stations.Rail2.AsIterable()),
                                        });
                                    }).Run().Bind(static inner => inner)
                                    select built,
                                partitioned: stations =>
                                    from _ in guard(stations.RailParameters.Count == shapes.Count, new KernelFault.InvalidInput(Axis: Some(nameof(SweepTwoStations.Partitioned.RailParameters))))
                                    from __ in guard(edit.Ends.Start.IsNone && edit.Ends.End.IsNone, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Ends))))
                                    from ___ in guard(edit.Fit is CurveFit.AsIs, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                    from ____ in guard(
                                        edit.Shape == CapabilitySet<SweepTwoShapeFeature>.Of(SweepTwoShapeFeature.AutoAdjust),
                                        new KernelFault.InvalidInput(Axis: Some(nameof(edit.Shape))))
                                    from built in Try.lift(() => ModelGate.Many(() => Brep.CreateFromSweepInParts(
                                        rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                        rail_params: stations.RailParameters.AsIterable(), closed: edit.Closure.Native, tolerance: model.Domain.Absolute.Value))).Run().Bind(static inner => inner)
                                    select built,
                                @static: _ =>
                                    from _ in guard(!edit.Fit.IncludesRails, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                    from built in Try.lift(() => {
                                        (SweepRebuild kind, int points, double refit, _) = edit.Fit.Native(domain: model.Domain);
                                        return ModelGate.Many(op, () => Brep.CreateFromSweep(
                                            rail1: rail1, rail2: rail2, shapes: shapes.AsIterable(),
                                            start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset, closed: edit.Closure.Native,
                                            tolerance: model.Domain.Absolute.Value, rebuild: kind, rebuildPointCount: points, refitTolerance: refit,
                                            preserveHeight: edit.Shape.Admits(capability: SweepTwoShapeFeature.MaintainHeight),
                                            autoAdjust: edit.Shape.Admits(capability: SweepTwoShapeFeature.AutoAdjust)));
                                    }).Run().Bind(static inner => inner)
                                    select built))));
            },
            loft: static (model, edit) => {
                return from _ in guard(!edit.Fit.IncludesRails, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                       from built in ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Shapes, body: shapes =>
                           edit.Tangency.Case switch {
                        LoftTangency tangency => ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: tangency.StartOwner, body: startOwner =>
                            ModelGate.Borrow<Brep, Seq<GeometryHandle>>(handle: tangency.EndOwner, body: endOwner =>
                                from _ in guard(tangency.StartTrim < startOwner.Trims.Count, new KernelFault.InvalidInput(Axis: Some(nameof(tangency.StartTrim))))
                                from __ in guard(tangency.EndTrim < endOwner.Trims.Count, new KernelFault.InvalidInput(Axis: Some(nameof(tangency.EndTrim))))
                                from ___ in guard(edit.Fit is CurveFit.AsIs, new KernelFault.InvalidInput(Axis: Some(nameof(edit.Fit))))
                                from built in Try.lift(() => ModelGate.Many(() => Brep.CreateFromLoft(
                                    curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                    StartTangent: tangency.Ends.First,
                                    EndTangent: tangency.Ends.Second,
                                    StartTrim: startOwner.Trims[tangency.StartTrim], EndTrim: endOwner.Trims[tangency.EndTrim],
                                    loftType: edit.Kind, closed: edit.Closure.Native))).Run().Bind(static inner => inner)
                                select built)),
                        _ => Try.lift(() => ModelGate.Many(() => edit.Fit switch {
                            CurveFit.Rebuild fit => Brep.CreateFromLoftRebuild(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, rebuildPointCount: fit.Points),
                            CurveFit.Refit => Brep.CreateFromLoftRefit(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value, refitTolerance: model.Domain.Absolute.Value),
                            _ => Brep.CreateFromLoft(
                                curves: shapes.AsIterable(), start: edit.Ends.StartOrUnset, end: edit.Ends.EndOrUnset,
                                loftType: edit.Kind, closed: edit.Closure.Native, angleTol: model.Domain.Angle.Value),
                        })).Run().Bind(static inner => inner),
                    })
                       select built;
            },
            patch: static (model, edit) => {
                return ModelGate.BorrowMany<GeometryBase, Seq<GeometryHandle>>(handles: edit.Geometry, body: constraints =>
                    edit.StartingSurface.Case switch {
                        GeometryHandle starting => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: starting,
                            body: surface => Patched(edit: edit, constraints: constraints, starting: Some(surface), model: model.Domain)),
                        _ => Patched(edit: edit, constraints: constraints, starting: Option<Surface>.None, model: model.Domain),
                    });
            },
            variational: static (model, edit) => {
                return ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.Edges.Map(static row => row.Curve), body: edgeCurves =>
                    ModelGate.BorrowMany<Curve, Seq<GeometryHandle>>(handles: edit.InternalCurves.Map(static row => row.Curve), allowEmpty: true, body: interiorCurves => {
                        Fin<Seq<GeometryHandle>> Solve(Option<Surface> initial) =>
                            from settings in edit.Law.Rig(domain: model.Domain, initial: initial)
                            from built in ModelGate.Single(op, () => Brep.CreateVariationalPatch(
                                    edges: edgeCurves.Zip(edit.Edges.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    internalCurves: interiorCurves.Zip(edit.InternalCurves.Map(static row => row.Continuity))
                                        .Map(static pair => new Brep.CurveConstraint(curve: pair.First, continuity: pair.Second)).AsIterable(),
                                    points: edit.Points.Map(static point => new Brep.PointConstraint(point: point)).AsIterable(),
                                    settings: settings, multiThreading: edit.Threading.Native,
                                    cancelToken: model.Runtime.Cancellation,
                                    progress: HostEdge.Slot(value: model.Runtime.ScalarProgress),
                                    results: out _), token: model.Runtime.Cancellation)
                            select built;
                        return edit.Law.InitialSurface.Case switch {
                            GeometryHandle seed => ModelGate.Borrow<Surface, Seq<GeometryHandle>>(handle: seed, body: surface => Solve(initial: Some(surface))),
                            _ => Solve(initial: Option<Surface>.None),
                        };
                    }));
            },
            developable: static (_, edit) => edit.Law.Switch(
                edit,
                byDensity: static (ctx, law) => ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: ctx.Rail0, body: rail0 =>
                    ModelGate.Borrow<Curve, Seq<GeometryHandle>>(handle: ctx.Rail1, body: rail1 =>
                        ModelGate.Many(() => Brep.CreateDevelopableLoft(
                            crv0: rail0,
                            crv1: rail1,
                            reverse0: law.Reverse.First,
                            reverse1: law.Reverse.Second,
                            density: law.Density)))),
                byRulings: static (ctx, law) => ModelGate.Borrow<NurbsCurve, Seq<GeometryHandle>>(handle: ctx.Rail0, body: rail0 =>
                    ModelGate.Borrow<NurbsCurve, Seq<GeometryHandle>>(handle: ctx.Rail1, body: rail1 =>
                        ModelGate.Many(() => Brep.CreateDevelopableLoft(
                            rail0: rail0, rail1: rail1, fixedRulings: law.FixedRulings.AsIterable()))))));

    private static Fin<Seq<GeometryHandle>> Patched(Patch edit, Seq<GeometryBase> constraints, Option<Surface> starting, Context model) =>
        ModelGate.Single(() => Brep.CreatePatch(
            geometry: constraints.AsIterable(), startingSurface: HostEdge.Slot(value: starting),
            uSpans: edit.Law.USpans, vSpans: edit.Law.VSpans,
            trim: edit.Law.Behavior.Admits(capability: PatchBehavior.Trim),
            tangency: edit.Law.Behavior.Admits(capability: PatchBehavior.Tangency),
            pointSpacing: edit.Law.PointSpacing, flexibility: edit.Law.Flexibility, surfacePull: edit.Law.SurfacePull,
            fixEdges: edit.Law.FixedEdges, tolerance: model.Absolute.Value));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lofts {
    public static Eff<ModelRuntime, Seq<GeometryHandle>> Build(params ReadOnlySpan<LoftOp> operations) {
        Seq<LoftOp> captured = toSeq(operations.ToArray());
        return Eff.runtime<ModelRuntime>().Bind(runtime =>
            ModelGate.Entry(
                runtime: runtime,
                operations: captured,
                admit: static (operation, key) => operation.Admitted(),
                apply: (operation, model) => operation.Apply(domain: model, runtime: runtime)).ToEff());
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
