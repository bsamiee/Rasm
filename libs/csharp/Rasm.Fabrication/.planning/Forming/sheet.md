# [RASM_FABRICATION_FLAT_PATTERN]

`FlatPattern` owns sheet development from admitted formed-panel or kernel-surface evidence to one neutral-axis-correct flat, bend topology, feature evidence, relief census, and content-keyed result. `FormPolicy` admits material, thickness, calibration, development, grain, relief, and feature limits once; every interior length is machining-canonical millimeters.

`FlatPattern.Unfold`, `UnfoldResult`, and `FlatPattern.Formed` preserve the `FabricationPolicy.Form` wire. Kernel `Development.Apply` owns surface isometry, `PolygonAlgebra.Apply` owns region topology and relief subtraction, and `ContentKey.Of` owns artifact identity.

## [01]-[INDEX]

- [02]-[SHEET_DEVELOPMENT]: Generated admission, parameterized bend physics, panel and surface development, neutral-axis placement, sheet-feature evidence, relief topology, and result projection.

## [02]-[SHEET_DEVELOPMENT]

- Owner: `FormPolicy` owns admitted sheet intent; `SheetSource` owns formed-panel, component, and kernel-surface ingress; `SheetLink` owns panel and surface adjacency; `SheetForm` owns line-form and local-feature evidence; `FlatPattern` owns development and projection.
- Cases: `KSource` resolves table, measured-coupon, `DIN 6935`, and material-physics neutral-axis positions; `HemKind` carries per-row sweep and inside-radius laws; `ReliefKind` sizes and generates rectangular, obround, tear, and circular reliefs; `SheetForm` carries each feature's distinct evidence and its tooling demand.
- Entry: `FlatPattern.Unfold(FormPolicy, FabricationInput)` is the frozen development seam, and `FlatPattern.Formed(UnfoldResult, Seq<BendStep>)` is the frozen result projection.
- Auto: Panel links derive a topological placement order; the generated grain field gates bend radius and loop-feature strain; surface links shift kernel islands by neutral-axis deltas; every bend endpoint enters one relief-seat census that folds co-terminating bends into one corner seat sized against the formed radius; one `PolygonOp.Boolean` subtracts admitted reliefs.
- Receipt: `UnfoldResult` preserves flat regions, bend topology, forming physics, kernel isometry, neutral-axis displacement, feature evidence, relief evidence, and material identity.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `QuikGraph`, `UnitsNet`, `Rasm`, the `Rasm.Element` `CanonicalWriter` codec, and the `Geometry2D` owner compose the surface.
- Growth: each K convention is one `KSource` row, a hem geometry is one `HemKind` row, a relief geometry is one `ReliefKind` row, a sheet feature is one `SheetForm` case, a link modality is one `SheetLink` case, and a new source is one `SheetSource` case with one total dispatch arm.
- Boundary: Forming owns neutral-axis and feature development; kernel isometry, planar topology, process physics, and content identity remain at their canonical owners.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using Rasm.Element.Projection;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Forming;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class KSource {
    public static readonly KSource Table = new("table", static query => query.Table
        .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:k-table"))
        .Bind(table => table.Resolve(query)));
    public static readonly KSource Coupon = new("coupon", static query => query.Coupon
        .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:k-coupon"))
        .Bind(coupon => coupon.Calibrate(query)));
    // DIN 6935 fixes the correction factor at `k = 0.65 + 0.5·lg(r/s)`, held at 0.65 below `r/s = 0.65` and capped
    // at 1.0 above; the floor is the standard's own lower branch, and dropping it to 0.5 understates the neutral
    // axis by a fifth on exactly the tight-radius bends this row exists to answer.
    public static readonly KSource Din6935 = new("din-6935", static query =>
        Fin.Succ(Math.Clamp(0.65 + (0.5 * Math.Log10(query.RadiusMm / query.ThicknessMm)), 0.65, 1.0) / 2.0));
    public static readonly KSource Physics = new("physics", static query => Fin.Succ(query.Forming.KFactor));

    [UseDelegateFromConstructor]
    private partial Fin<double> ResolveAdmitted(KQuery query);

    public Fin<double> Resolve(KQuery? query) => Optional(query)
        .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:k-query"))
        .Bind(ResolveAdmitted)
        .Bind(static k => double.IsFinite(k) && k is > 0.0 and < 1.0
            ? Fin.Succ(k)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:k-result")));
}

[SmartEnum<string>]
public sealed partial class ReliefKind {
    public static readonly ReliefKind Rectangular = new("rectangular", widthFactor: 1.0, radiusFactor: 1.0, depthFactor: 1.0, FlatPattern.Rectangular);
    public static readonly ReliefKind Obround = new("obround", widthFactor: 1.0, radiusFactor: 1.0, depthFactor: 1.5, FlatPattern.Obround);
    public static readonly ReliefKind Tear = new("tear", widthFactor: 0.75, radiusFactor: 1.0, depthFactor: 0.5, FlatPattern.Tear);
    public static readonly ReliefKind Circular = new("circular", widthFactor: 1.5, radiusFactor: 0.5, depthFactor: 0.75, FlatPattern.Circular);

    public double WidthFactor { get; }
    public double RadiusFactor { get; }
    public double DepthFactor { get; }

    // Relief must clear the formed radius, so depth carries the bend's inside radius beside the thickness term.
    public double Width(double thicknessMm) => WidthFactor * thicknessMm;
    public double Depth(double thicknessMm, double insideRadiusMm) => (RadiusFactor * insideRadiusMm) + (DepthFactor * thicknessMm);

    [UseDelegateFromConstructor]
    public partial Fin<Loop> Cut(ReliefSeat seat, Context tolerance);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SheetForm {
    private SheetForm() { }

    public sealed record Bend : SheetForm;
    public sealed record Hem(HemKind Kind, Length Gap) : SheetForm;
    public sealed record Jog(Length Offset, Length Spacing) : SheetForm;
    public sealed record Curl(Length InsideRadius, Angle Sweep) : SheetForm;
    public sealed record Bead(Loop Path, Length Width, Length Depth) : SheetForm;
    public sealed record Louver(Loop Aperture, Length Height, Angle Opening) : SheetForm;
    public sealed record Emboss(Loop Footprint, Length Height, Angle Draft) : SheetForm;
    public sealed record Dimple(Loop Footprint, Length Depth, Length ToolRadius) : SheetForm;

    public bool IsValid => Switch(
        bend: static _ => true,
        hem: static row => row.Kind is not null && double.IsFinite(row.Gap.Millimeters) && row.Gap.Millimeters >= 0.0,
        jog: static row => double.IsFinite(row.Offset.Millimeters) && row.Offset > Length.Zero
            && double.IsFinite(row.Spacing.Millimeters) && row.Spacing > Length.Zero,
        curl: static row => double.IsFinite(row.InsideRadius.Millimeters) && row.InsideRadius > Length.Zero
            && double.IsFinite(row.Sweep.Radians) && row.Sweep > Angle.Zero && row.Sweep <= Angle.FromDegrees(360.0),
        bead: static row => row.Path is { Closed: true } && row.Width > Length.Zero && row.Depth > Length.Zero,
        louver: static row => row.Aperture is { Closed: true } && row.Height > Length.Zero
            && row.Opening > Angle.Zero && row.Opening <= Angle.FromDegrees(180.0),
        emboss: static row => row.Footprint is { Closed: true } && row.Height > Length.Zero
            && row.Draft >= Angle.Zero && row.Draft < Angle.FromDegrees(90.0),
        dimple: static row => row.Footprint is { Closed: true } && row.Depth > Length.Zero && row.ToolRadius > Length.Zero);

    public bool IsLine => Switch(
        bend: static _ => true,
        hem: static _ => true,
        jog: static _ => true,
        curl: static _ => true,
        bead: static _ => false,
        louver: static _ => false,
        emboss: static _ => false,
        dimple: static _ => false);

    public bool IsFeature => IsValid && !IsLine;

    // Line forms whose geometry demands dedicated tooling override the policy default at candidate admission;
    // None defers to FormPolicy, so one part mixes hemmed, curled, and ordinary bends under one policy.
    public Option<(BendMethod Method, PunchKind Punch)> Tooling => Switch(
        bend: static _ => Option<(BendMethod, PunchKind)>.None,
        hem: static row => Some((row.Kind == HemKind.Closed ? BendMethod.Coin : BendMethod.Hem, PunchKind.Hemming)),
        jog: static _ => Option<(BendMethod, PunchKind)>.None,
        curl: static _ => Some((BendMethod.Fold, PunchKind.Radius)),
        bead: static _ => Option<(BendMethod, PunchKind)>.None,
        louver: static _ => Option<(BendMethod, PunchKind)>.None,
        emboss: static _ => Option<(BendMethod, PunchKind)>.None,
        dimple: static _ => Option<(BendMethod, PunchKind)>.None);
}

[SmartEnum<string>]
public sealed partial class HemKind {
    public static readonly HemKind Open = new("open", Angle.FromDegrees(180.0), static (_, radius, gap) => Math.Max(radius, gap / 2.0));
    public static readonly HemKind Closed = new("closed", Angle.FromDegrees(180.0), static (thickness, radius, _) => Math.Max(radius, thickness / 2.0));
    public static readonly HemKind Teardrop = new("teardrop", Angle.FromDegrees(210.0), static (thickness, radius, gap) => Math.Max(radius, Math.Max(gap, thickness) / 2.0));
    public static readonly HemKind Rolled = new("rolled", Angle.FromDegrees(270.0), static (thickness, radius, gap) => Math.Max(radius, Math.Max(gap, thickness)));

    public Angle Sweep { get; }

    [UseDelegateFromConstructor]
    public partial double InsideRadius(double thicknessMm, double radiusMm, double gapMm);

    public double Allowance(double thicknessMm, double radiusMm, double k, double gapMm) =>
        Sweep.Radians * (InsideRadius(thicknessMm, radiusMm, gapMm) + (k * thicknessMm));
}

[ComplexValueObject]
public sealed partial class BendCoupon {
    public Material Material { get; }
    public Option<string> Grade { get; }
    public BendMethod Method { get; }
    public double ThicknessMm { get; }
    public double InsideRadiusMm { get; }
    public double BendAngleDeg { get; }
    public double DevelopedAllowanceMm { get; }
    public double RadiusThicknessTolerance { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Material material,
        ref Option<string> grade,
        ref BendMethod method,
        ref double thicknessMm,
        ref double insideRadiusMm,
        ref double bendAngleDeg,
        ref double developedAllowanceMm,
        ref double radiusThicknessTolerance) =>
        validationError = material is not null && method is not null
            && grade.ForAll(static value => !string.IsNullOrWhiteSpace(value))
            && double.IsFinite(thicknessMm) && thicknessMm > 0.0
            && double.IsFinite(insideRadiusMm) && insideRadiusMm >= 0.0
            && double.IsFinite(bendAngleDeg) && Math.Abs(bendAngleDeg) is > 0.0 and <= 180.0
            && double.IsFinite(developedAllowanceMm) && developedAllowanceMm > 0.0
            && double.IsFinite(radiusThicknessTolerance) && radiusThicknessTolerance >= 0.0
                ? null
                : new ValidationError(message: "Bend coupon must carry material, method, measured geometry, and an R/T applicability band.");

    public Fin<double> Calibrate(KQuery query) {
        double couponRatio = InsideRadiusMm / ThicknessMm;
        double queryRatio = query.RadiusMm / query.ThicknessMm;
        double radians = Angle.FromDegrees(Math.Abs(BendAngleDeg)).Radians;
        double k = ((DevelopedAllowanceMm / radians) - InsideRadiusMm) / ThicknessMm;
        return Material == query.Material.Family && Method == query.Method
            && Grade.ForAll(grade => grade == query.Material.Identity.Grade)
            && Math.Abs(queryRatio - couponRatio) <= RadiusThicknessTolerance
            && double.IsFinite(k) && k is > 0.0 and < 1.0
                ? Fin.Succ(k)
                : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:k-coupon-applicability"));
    }
}

[ComplexValueObject]
public sealed partial class KFactorBand {
    public Material Material { get; }
    public Option<string> Grade { get; }
    public BendMethod Method { get; }
    public double RtLow { get; }
    public double RtHigh { get; }
    public double KLow { get; }
    public double KHigh { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Material material,
        ref Option<string> grade,
        ref BendMethod method,
        ref double rtLow,
        ref double rtHigh,
        ref double kLow,
        ref double kHigh) =>
        validationError = material is not null && method is not null
            && grade.ForAll(static value => !string.IsNullOrWhiteSpace(value))
            && double.IsFinite(rtLow) && rtLow >= 0.0 && double.IsFinite(rtHigh) && rtHigh > rtLow
            && double.IsFinite(kLow) && double.IsFinite(kHigh)
            && kLow is > 0.0 and < 1.0 && kHigh is > 0.0 and < 1.0
                ? null
                : new ValidationError(message: "K-factor bands require an admitted material-method interval and bounded neutral-axis factors.");
}

[ComplexValueObject]
public sealed partial class KFactorTable {
    public Arr<KFactorBand> Bands { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Arr<KFactorBand> bands) =>
        validationError = !bands.IsEmpty
            && bands.GroupBy(static band => (band.Material, band.Grade, band.Method)).ForAll(static series =>
                series.OrderBy(static band => band.RtLow)
                    .Zip(series.OrderBy(static band => band.RtLow).Skip(1))
                    .All(static pair => pair.First.RtHigh <= pair.Second.RtLow))
                ? null
                : new ValidationError(message: "K-factor bands must form finite non-overlapping material and method series.");

    public Fin<double> Resolve(KQuery query) {
        Arr<KFactorBand> exact = Bands.Filter(band => band.Material == query.Material.Family && band.Method == query.Method
            && band.Grade.Exists(grade => grade == query.Material.Identity.Grade));
        Arr<KFactorBand> series = exact.IsEmpty
            ? Bands.Filter(band => band.Material == query.Material.Family && band.Method == query.Method && band.Grade.IsNone)
            : exact;
        return series.Filter(band => query.RadiusMm / query.ThicknessMm >= band.RtLow && query.RadiusMm / query.ThicknessMm < band.RtHigh)
            .Head
            .Map(band => band.KLow + ((band.KHigh - band.KLow)
                * ((query.RadiusMm / query.ThicknessMm) - band.RtLow) / (band.RtHigh - band.RtLow)))
            .Filter(static k => double.IsFinite(k) && k is > 0.0 and < 1.0)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"flat:k-band:{query.Material.Family.Key}:{query.Method.Key}"));
    }
}

[ComplexValueObject]
public sealed partial class KQuery {
    public MaterialSpec Material { get; }
    public BendMethod Method { get; }
    public double RadiusMm { get; }
    public double ThicknessMm { get; }
    public Option<KFactorTable> Table { get; }
    public Option<BendCoupon> Coupon { get; }
    public ProcessBudget.Formed Forming { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref MaterialSpec material,
        ref BendMethod method,
        ref double radiusMm,
        ref double thicknessMm,
        ref Option<KFactorTable> table,
        ref Option<BendCoupon> coupon,
        ref ProcessBudget.Formed forming) =>
        validationError = material is not null && method is not null && forming is not null
            && double.IsFinite(radiusMm) && radiusMm > 0.0
            && double.IsFinite(thicknessMm) && thicknessMm > 0.0
                ? null
                : new ValidationError(message: "K-factor query must carry admitted material, method, forming, radius, and thickness.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SheetLink {
    private SheetLink() { }

    public sealed record Panel(
        int Parent,
        int Child,
        Edge3 ParentEdge,
        Edge3 ChildEdge,
        double AngleDeg,
        Option<double> RadiusMm,
        SheetForm Form,
        Set<int> Prerequisites) : SheetLink;

    public sealed record Surface(
        ChartId Parent,
        ChartId Child,
        int SourceA,
        int SourceB,
        double ReferenceArcMm,
        double AngleDeg,
        Option<double> RadiusMm,
        SheetForm Form,
        Set<int> Prerequisites) : SheetLink;

    public bool IsValid => Switch(
        panel: static link => link.ParentEdge.A.IsValid && link.ParentEdge.B.IsValid && link.ParentEdge.A != link.ParentEdge.B
            && link.ChildEdge.A.IsValid && link.ChildEdge.B.IsValid && link.ChildEdge.A != link.ChildEdge.B
            && double.IsFinite(link.AngleDeg) && Math.Abs(link.AngleDeg) is > 0.0 and <= 180.0
            && link.RadiusMm.ForAll(static radius => double.IsFinite(radius) && radius >= 0.0)
            && link.Form is { IsValid: true, IsLine: true },
        surface: static link => link.SourceA >= 0 && link.SourceB >= 0 && link.SourceA != link.SourceB
            && double.IsFinite(link.ReferenceArcMm) && link.ReferenceArcMm >= 0.0
            && double.IsFinite(link.AngleDeg) && Math.Abs(link.AngleDeg) is > 0.0 and <= 180.0
            && link.RadiusMm.ForAll(static radius => double.IsFinite(radius) && radius >= 0.0)
            && link.Form is { IsValid: true, IsLine: true });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SheetSource {
    private SheetSource() { }

    public sealed record Panels(Seq<SheetLink.Panel> Links, Seq<SheetForm> Features) : SheetSource;
    public sealed record Component(AdmittedComponent Value, Seq<SheetLink.Panel> Links, Seq<SheetForm> Features) : SheetSource;
    public sealed record Surface(SurfaceResult.UvTessellation Value, Seq<SheetLink.Surface> Links, Seq<SheetForm> Features) : SheetSource;

    public bool IsValid => Switch(
        panels: static row => row.Links.ForAll(static link => link.IsValid) && row.Features.ForAll(static feature => feature is { IsFeature: true }),
        component: static row => row.Value is not null && row.Links.ForAll(static link => link.IsValid)
            && row.Features.ForAll(static feature => feature is { IsFeature: true }),
        surface: static row => row.Value is not null && row.Links.ForAll(static link => link.IsValid)
            && row.Features.ForAll(static feature => feature is { IsFeature: true }));
}

[ComplexValueObject]
public sealed partial class GrainLaw {
    public double RollingDeg { get; }
    public double Parallel { get; }
    public double Transverse { get; }
    public double Exponent { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double rollingDeg,
        ref double parallel,
        ref double transverse,
        ref double exponent) =>
        validationError = double.IsFinite(rollingDeg)
            && double.IsFinite(parallel) && parallel > 0.0
            && double.IsFinite(transverse) && transverse > 0.0
            && double.IsFinite(exponent) && exponent >= 1.0
                ? null
                : new ValidationError(message: "Grain law must carry a rolling axis, positive directional limits, and a finite interpolation exponent.");

    public Fin<double> At(Vector3d direction) {
        Vector3d projected = new(direction.X, direction.Y, 0.0);
        if (!projected.Unitize())
            return Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Line, None, "flat:grain-axis").ToError());
        double radians = Angle.FromDegrees(RollingDeg).Radians;
        Vector3d rolling = new(Math.Cos(radians), Math.Sin(radians), 0.0);
        double parallel = Math.Abs(projected * rolling);
        double transverse = Math.Sqrt(Math.Max(0.0, 1.0 - (parallel * parallel)));
        return Fin.Succ(Math.Pow(
            Math.Pow(Parallel * parallel, Exponent) + Math.Pow(Transverse * transverse, Exponent),
            1.0 / Exponent));
    }
}

[ComplexValueObject]
public sealed partial class FormPolicy {
    public SheetSource Source { get; }
    public MaterialSpec Material { get; }
    public ConstitutiveState State { get; }
    public double ThicknessMm { get; }
    public BendMethod Method { get; }
    public PunchKind Punch { get; }
    public BrakePolicy Brake { get; }
    public KSource KSource { get; }
    public Option<KFactorTable> KFactors { get; }
    public Option<BendCoupon> Coupon { get; }
    public Option<double> DieWidthFactor { get; }
    public Option<GrainLaw> Grain { get; }
    public ReliefKind Relief { get; }
    public DevelopPolicy Development { get; }
    public double IsometryBudget { get; }
    public double TorsalBudget { get; }
    public double FeatureStrainLimit { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SheetSource source,
        ref MaterialSpec material,
        ref ConstitutiveState state,
        ref double thicknessMm,
        ref BendMethod method,
        ref PunchKind punch,
        ref BrakePolicy brake,
        ref KSource kSource,
        ref Option<KFactorTable> kFactors,
        ref Option<BendCoupon> coupon,
        ref Option<double> dieWidthFactor,
        ref Option<GrainLaw> grain,
        ref ReliefKind relief,
        ref DevelopPolicy development,
        ref double isometryBudget,
        ref double torsalBudget,
        ref double featureStrainLimit) =>
        validationError = source is { IsValid: true } && material is not null && state is not null
            && method is not null && punch is not null && brake is not null
            && kSource is not null && kFactors.ForAll(static value => value is not null) && relief is not null && development is { IsValid: true }
            && (kSource != KSource.Table || kFactors.IsSome) && (kSource != KSource.Coupon || coupon.IsSome)
            && double.IsFinite(thicknessMm) && thicknessMm > 0.0
            && coupon.ForAll(static value => value is not null)
            && dieWidthFactor.ForAll(static value => double.IsFinite(value) && value > 0.0)
            && double.IsFinite(isometryBudget) && isometryBudget >= 0.0
            && double.IsFinite(torsalBudget) && torsalBudget >= 0.0
            && double.IsFinite(featureStrainLimit) && featureStrainLimit > 0.0
                ? null
                : new ValidationError(message: "Sheet policy must carry admitted material, geometry, calibration, tooling, and budgets.");

}

public readonly record struct BendProjection(double AllowanceMm, double SetbackMm, double DeductionMm, double NeutralShiftMm);

public sealed record BendLine(
    int Index,
    Edge3 Line,
    int Parent,
    int Child,
    double AngleDeg,
    double InsideRadiusMm,
    double K,
    BendProjection Projection,
    SheetForm Form,
    Set<int> Prerequisites);

public sealed record ReliefSeat(
    Point3d At,
    Vector3d Along,
    Vector3d Inward,
    double WidthMm,
    double DepthMm,
    double InsideRadiusMm,
    Set<int> Meeting,
    bool ExistingClearance);

public sealed record SheetFeatureEvidence(SheetForm Form, double DevelopedMm, double PeakStrain);
public sealed record PanelRegion(int Panel, Loop Boundary);

public sealed record UnfoldEvidence(
    Option<DevelopmentReceipt> Isometry,
    Seq<PanelRegion> Panels,
    Seq<(int Bend, double ShiftMm)> NeutralAxis,
    Seq<SheetFeatureEvidence> Features,
    Seq<ReliefSeat> Reliefs,
    TopologyReceipt Topology);

[ComplexValueObject]
public sealed partial class UnfoldResult {
    public Arr<Loop> Flat { get; }
    public Seq<BendLine> Bends { get; }
    public double ThicknessMm { get; }
    public MaterialSpec Material { get; }
    public ProcessBudget.Formed Forming { get; }
    public UnfoldEvidence Evidence { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<Loop> flat,
        ref Seq<BendLine> bends,
        ref double thicknessMm,
        ref MaterialSpec material,
        ref ProcessBudget.Formed forming,
        ref UnfoldEvidence evidence) =>
        validationError = !flat.IsEmpty && flat.ForAll(static loop => loop is not null && loop.Closed)
            && flat.ForAll(loop => loop.Tolerance == flat[0].Tolerance)
            && bends.ForAll(static bend => bend.Line.A.IsValid && bend.Line.B.IsValid && bend.Line.A != bend.Line.B
                && bend.Parent >= 0 && bend.Child >= 0 && bend.Parent != bend.Child
                && double.IsFinite(bend.AngleDeg) && Math.Abs(bend.AngleDeg) is > 0.0 and <= 180.0
                && double.IsFinite(bend.InsideRadiusMm) && bend.InsideRadiusMm >= 0.0
                && double.IsFinite(bend.K) && bend.K is > 0.0 and < 1.0
                && double.IsFinite(bend.Projection.AllowanceMm) && bend.Projection.AllowanceMm > 0.0
                && double.IsFinite(bend.Projection.SetbackMm)
                && double.IsFinite(bend.Projection.DeductionMm)
                && double.IsFinite(bend.Projection.NeutralShiftMm)
                && bend.Form is { IsValid: true, IsLine: true })
            && bends.Map(static bend => bend.Index).Distinct().Count() == bends.Count
            && double.IsFinite(thicknessMm) && thicknessMm > 0.0 && material is not null && forming is not null && evidence is not null
            && evidence.Topology is not null
            && !evidence.Panels.IsEmpty && evidence.Panels.ForAll(panel => panel.Panel >= 0
                && panel.Boundary is { Closed: true } && panel.Boundary.Tolerance == flat[0].Tolerance)
            && evidence.NeutralAxis.Count == bends.Count
            && evidence.NeutralAxis.Map(static row => row.Bend).Distinct().Count == bends.Count
            && bends.ForAll(bend => evidence.Panels.Exists(panel => panel.Panel == bend.Parent)
                && evidence.Panels.Exists(panel => panel.Panel == bend.Child)
                && bend.Prerequisites.ForAll(prerequisite => prerequisite != bend.Index
                    && bends.Exists(candidate => candidate.Index == prerequisite))
                && evidence.NeutralAxis.Exists(row => row.Bend == bend.Index
                    && double.IsFinite(row.ShiftMm)
                    && row.ShiftMm.Equals(bend.Projection.NeutralShiftMm)))
            && evidence.Features.ForAll(static feature => feature.Form is { IsValid: true, IsFeature: true }
                && double.IsFinite(feature.DevelopedMm) && feature.DevelopedMm > 0.0
                && double.IsFinite(feature.PeakStrain) && feature.PeakStrain >= 0.0)
            && evidence.Reliefs.ForAll(relief => Valid(relief, bends, evidence.Topology, flat[0].Tolerance.Absolute.Value))
                ? null
                : new ValidationError(message: "Unfold evidence must carry closed compatible flats and fully admitted bend rows.");

    private static bool Valid(ReliefSeat relief, Seq<BendLine> bends, TopologyReceipt topology, double toleranceMm) {
        double unitTolerance = Math.Sqrt(double.BitIncrement(1.0) - 1.0);
        return relief is not null && relief.At.IsValid
            && Finite(relief.Along) && Finite(relief.Inward)
            && Math.Abs(relief.Along.Length - 1.0) <= unitTolerance
            && Math.Abs(relief.Inward.Length - 1.0) <= unitTolerance
            && Math.Abs(relief.Along * relief.Inward) <= unitTolerance
            && double.IsFinite(relief.WidthMm) && relief.WidthMm > 0.0
            && double.IsFinite(relief.DepthMm) && relief.DepthMm > 0.0
            && double.IsFinite(relief.InsideRadiusMm) && relief.InsideRadiusMm >= 0.0
            && !relief.Meeting.IsEmpty
            && relief.Meeting.ForAll(index => bends.Find(bend => bend.Index == index)
                .Exists(bend => bend.Line.A.DistanceTo(relief.At) <= toleranceMm
                    || bend.Line.B.DistanceTo(relief.At) <= toleranceMm))
            && topology.Nodes.Count > 0;

        static bool Finite(Vector3d value) => value.IsValid
            && double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class FlatPattern {
    public static Fin<UnfoldResult> Unfold(FormPolicy policy, FabricationInput input) =>
        policy is null || input is null
            ? Fin.Fail<UnfoldResult>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:input"))
            : from bendLength in BendLength(policy.Source)
              from forming in FormedRow(policy.Material, input.Process, policy.State, policy.ThicknessMm, bendLength)
              from assembly in policy.Source.Switch(
                  state: (Policy: policy, Input: input, Forming: forming),
                  panels: static (state, source) => DevelopPanels(state.Input.Profiles, source.Links, source.Features, state.Policy, state.Forming),
                  component: static (state, source) => DevelopPanels(source.Value.Profiles, source.Links, source.Features, state.Policy, state.Forming),
                  surface: static (state, source) => DevelopSurface(source, state.Policy, state.Forming))
              from result in Finish(assembly, policy, forming)
              select result;

    public static FabricationResult Formed(UnfoldResult unfold, Seq<BendStep> bends) =>
        new FabricationResult.FormedResult(
            unfold.Flat,
            bends,
            bends.Map(static bend => bend.OverbendDeg).Fold(0.0, Math.Max),
            ContentKey.Of(EgressKind.FlatPattern, Canonical(unfold, bends)));

    internal static Fin<ProcessBudget.Formed> FormedRow(
        MaterialSpec material,
        ProcessKind process,
        ConstitutiveState state,
        double thicknessMm,
        double bendLengthMm) =>
        ProcessPhysics.Budget(new PhysicsRequest.Forming(process, material, state, thicknessMm, bendLengthMm))
            .Bind(static budget => budget is ProcessBudget.Formed formed
            ? Fin.Succ(formed)
            : Fin.Fail<ProcessBudget.Formed>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:forming-budget")));

    private static Fin<double> BendLength(SheetSource source) => source.Switch(
        panels: static row => Total(row.Links.Map(static link => link.ParentEdge.A.DistanceTo(link.ParentEdge.B))),
        component: static row => Total(row.Links.Map(static link => link.ParentEdge.A.DistanceTo(link.ParentEdge.B))),
        surface: static row => SurfaceLength(row));

    private static Fin<double> SurfaceLength(SheetSource.Surface source) {
        Mesh mesh = source.Value.Mesh.DuplicateNative();
        return source.Links.Traverse(link => (link.SourceA < mesh.Vertices.Count && link.SourceB < mesh.Vertices.Count
                ? Fin.Succ(mesh.Vertices.Point3dAt(link.SourceA).DistanceTo(mesh.Vertices.Point3dAt(link.SourceB)))
                : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:surface-bend-length")))
            .ToValidation()).As().ToFin().Bind(Total);
    }

    private static Fin<double> Total(Seq<double> lengths) {
        double total = lengths.Fold(0.0, static (sum, length) => sum + length);
        return double.IsFinite(total) && total > 0.0
            ? Fin.Succ(total)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:bend-length"));
    }

    private static Fin<SheetAssembly> DevelopPanels(
        Arr<Loop> panels,
        Seq<SheetLink.Panel> links,
        Seq<SheetForm> features,
        FormPolicy policy,
        ProcessBudget.Formed forming) =>
        from schedule in LinkOrder(panels.Count, links)
        from bends in schedule.Order.Traverse(link => BendOf(link, policy, forming).ToValidation()).As().ToFin()
        from placed in schedule.Order.FoldM<Fin, PanelState>(PanelState.Start(panels, schedule.Root), (state, link) => Place(state, link, bends)).As()
        select new SheetAssembly(
            placed.Flat,
            bends,
            features,
            None,
            placed.Flat.Map((loop, panel) => new PanelRegion(panel, loop)).ToSeq());

    private static Fin<SheetAssembly> DevelopSurface(SheetSource.Surface source, FormPolicy policy, ProcessBudget.Formed forming) =>
        from development in Development.Apply(new DevelopOp.Unroll(source.Value, policy.Development))
        from unrolled in development is DevelopmentResult.Unrolled value
            ? Fin.Succ(value)
            : Fin.Fail<DevelopmentResult.Unrolled>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:surface-result"))
        from _ in unrolled.Receipt.MaxIsometry <= policy.IsometryBudget && unrolled.Receipt.MaxTorsal <= policy.TorsalBudget
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(FabricationFault.UnfoldInfeasible(unrolled.Atlas.Islands.Count, unrolled.Field.RailOffsets.Count))
        from bends in source.Links.Traverse(link => SurfaceBendOf(link, unrolled.Atlas.Islands, policy, forming).ToValidation()).As().ToFin()
        from panels in Neutralize(unrolled.Atlas.Islands, bends, source.Value.Mesh.Tolerance)
        select new SheetAssembly(
            panels.Map(static panel => panel.Boundary).ToArr(),
            bends,
            source.Features,
            Some(unrolled.Receipt),
            panels);

    private static Fin<UnfoldResult> Finish(SheetAssembly assembly, FormPolicy policy, ProcessBudget.Formed forming) =>
        from topology in Regions(assembly.Flat)
        from features in assembly.Features.Traverse(feature => FeatureOf(feature, policy, forming).ToValidation()).As().ToFin()
        from seats in ReliefSeats(assembly.Flat, assembly.Bends, topology, policy)
        from cuts in seats.Filter(static seat => !seat.ExistingClearance)
            .Traverse(seat => policy.Relief.Cut(seat, assembly.Flat[0].Tolerance).ToValidation()).As().ToFin()
        from relieved in cuts.IsEmpty ? Fin.Succ(assembly.Flat) : Difference(assembly.Flat, cuts)
        from panels in cuts.IsEmpty
            ? Fin.Succ(assembly.Panels)
            : assembly.Panels.Traverse(panel => Difference(Arr(panel.Boundary), cuts)
                .Map(loops => loops.Map(loop => new PanelRegion(panel.Panel, loop)).ToSeq()).ToValidation()).As().ToFin()
                .Map(static regions => regions.Bind(static region => region))
        from finalTopology in Regions(relieved)
        from result in AdmitResult(
            relieved.ToArr(),
            assembly.Bends,
            policy.ThicknessMm,
            policy.Material,
            forming,
            new UnfoldEvidence(
                assembly.Isometry,
                panels,
                assembly.Bends.Map(static bend => (bend.Index, bend.Projection.NeutralShiftMm)),
                features,
                seats,
                finalTopology))
        select result;

    private static Fin<BendLine> BendOf(SheetLink.Panel link, FormPolicy policy, ProcessBudget.Formed forming) =>
        Annotate(link.Child, link.ParentEdge, link.Parent, link.Child, link.AngleDeg, link.RadiusMm, link.Form, link.Prerequisites, policy, forming, 0.0);

    private static Fin<BendLine> SurfaceBendOf(
        SheetLink.Surface link,
        Seq<UvIsland> islands,
        FormPolicy policy,
        ProcessBudget.Formed forming) =>
        from parent in islands.Map((island, index) => (Island: island, Index: index)).Find(row => row.Island.Chart == link.Parent)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:surface-parent"))
        from child in islands.Map((island, index) => (Island: island, Index: index)).Find(row => row.Island.Chart == link.Child)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:surface-child"))
        from a in Local(parent.Island, link.SourceA)
        from b in Local(parent.Island, link.SourceB)
        from bend in Annotate(
            child.Index,
            new Edge3(Planar(parent.Island.Uv[a]), Planar(parent.Island.Uv[b])),
            parent.Index,
            child.Index,
            link.AngleDeg,
            link.RadiusMm,
            link.Form,
            link.Prerequisites,
            policy,
            forming,
            link.ReferenceArcMm)
        select bend;

    private static Fin<BendLine> Annotate(
        int index,
        Edge3 line,
        int parent,
        int child,
        double angleDeg,
        Option<double> radius,
        SheetForm form,
        Set<int> prerequisites,
        FormPolicy policy,
        ProcessBudget.Formed forming,
        double referenceArcMm) {
        return from grainFactor in policy.Grain.Map(grain => grain.At(line.B - line.A)).IfNone(Fin.Succ(1.0))
               let minimumRadius = forming.MinBendRadiusFactor * policy.ThicknessMm * grainFactor
               let resolvedRadius = radius.IfNone(minimumRadius)
               from _ in double.IsFinite(angleDeg) && Math.Abs(angleDeg) is > 0.0 and <= 180.0
            && double.IsFinite(resolvedRadius) && resolvedRadius >= minimumRadius
            && line.A.IsValid && line.B.IsValid && line.A != line.B
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:bend"))
               from query in KQuery.Validate(
                   policy.Material,
                   policy.Method,
                   resolvedRadius,
                   policy.ThicknessMm,
                   policy.KFactors,
                   policy.Coupon,
                   forming,
                   out KQuery admitted) is { } error
                    ? Fin.Fail<KQuery>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, error.Message))
                    : Fin.Succ(admitted)
               from k in policy.KSource.Resolve(query)
               from projection in Project(form, angleDeg, resolvedRadius, policy.ThicknessMm, k, referenceArcMm)
               select new BendLine(index, line, parent, child, angleDeg, resolvedRadius, k, projection, form, prerequisites);
    }

    private static Fin<BendProjection> Project(SheetForm form, double angleDeg, double radiusMm, double thicknessMm, double k, double referenceArcMm) {
        double angle = Math.Abs(angleDeg);
        double radians = angle * Math.PI / 180.0;
        Fin<double> allowance = form.Switch(
            state: (Radians: radians, Radius: radiusMm, Thickness: thicknessMm, K: k),
            bend: static (state, _) => Fin.Succ(state.Radians * (state.Radius + (state.K * state.Thickness))),
            hem: static (state, value) => Fin.Succ(value.Kind.Allowance(state.Thickness, state.Radius, state.K, value.Gap.Millimeters)),
            jog: static (state, value) => Fin.Succ((2.0 * state.Radians * (state.Radius + (state.K * state.Thickness)))
                + value.Spacing.Millimeters),
            curl: static (state, value) => Fin.Succ(
                value.Sweep.Radians * (value.InsideRadius.Millimeters + (state.K * state.Thickness))),
            bead: static (_, _) => Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:line-bead")),
            louver: static (_, _) => Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:line-louver")),
            emboss: static (_, _) => Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:line-emboss")),
            dimple: static (_, _) => Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:line-dimple")));
        double setback = Math.Tan(angle * Math.PI / 360.0) * (radiusMm + thicknessMm);
        return allowance.Map(value => new BendProjection(value, setback, (2.0 * setback) - value, value - referenceArcMm));
    }

    private static Fin<(int Root, Seq<SheetLink.Panel> Order)> LinkOrder(int panels, Seq<SheetLink.Panel> links) {
        Set<int> children = links.Map(static link => link.Child).ToSet();
        bool tree = panels > 0 && links.Count == panels - 1 && children.Count == links.Count
            && links.ForAll(link => link.Parent >= 0 && link.Parent < panels && link.Child >= 0 && link.Child < panels
                && link.Parent != link.Child && link.Prerequisites.ForAll(prerequisite => prerequisite >= 0 && prerequisite < panels));
        if (!tree)
            return Fin.Fail<(int, Seq<SheetLink.Panel>)>(FabricationFault.UnfoldInfeasible(panels, links.Count));
        BidirectionalGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, panels));
        links.Iter(link => {
            graph.AddEdge(new SEdge<int>(link.Parent, link.Child));
            link.Prerequisites.Iter(prerequisite => graph.AddEdge(new SEdge<int>(prerequisite, link.Child)));
        });
        Seq<int> roots = graph.Roots().ToSeq();
        return roots.Count == 1 && graph.IsDirectedAcyclicGraph()
            ? Fin.Succ((roots[0], graph.SourceFirstTopologicalSort().ToSeq().Bind(vertex => links.Filter(link => link.Child == vertex))))
            : Fin.Fail<(int, Seq<SheetLink.Panel>)>(FabricationFault.UnfoldInfeasible(panels, links.Count));
    }

    private static Fin<PanelState> Place(PanelState state, SheetLink.Panel link, Seq<BendLine> bends) =>
        from parent in state.Placement.Find(link.Parent)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"flat:parent:{link.Parent}"))
        from resolved in bends.Find(row => row.Child == link.Child)
            .ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Forming, $"flat:bend:{link.Child}"))
        // The flat gap between two untrimmed panel outlines is the bend DEDUCTION, not the allowance: the allowance
        // is the developed length along the neutral axis, and seating the panels at that distance oversizes every
        // developed flat by twice the setback per bend. `DeductionMm` is the signed gap the placement wants.
        from child in PlanarPlacement.Between(link.ChildEdge, parent.Apply(link.ParentEdge), resolved.Projection.DeductionMm)
        from loop in Transform(state.Panels[link.Child], child.Apply)
        select state with {
            Flat = state.Flat.SetItem(link.Child, loop),
            Placement = state.Placement.Add(link.Child, child),
        };

    private static Fin<Seq<PanelRegion>> Neutralize(Seq<UvIsland> islands, Seq<BendLine> bends, Context tolerance) =>
        islands.Map((island, index) => (Island: island, Index: index))
            .Traverse(row => row.Island.Boundary(tolerance, Op.Of(name: nameof(Neutralize)))
                .Bind(chains => Rings(chains, tolerance))
                .Map(loops => loops.Map(loop => new PanelRegion(row.Index, loop))).ToValidation()).As().ToFin()
            .Map(static regions => regions.Bind(static region => region))
            .Bind(regions => bends.FoldM<Fin, Seq<PanelRegion>>(regions,
                (state, bend) => ShiftSurface(state, bend, bends)).As());

    private static Fin<Seq<PanelRegion>> ShiftSurface(
        Seq<PanelRegion> regions,
        BendLine bend,
        Seq<BendLine> bends) {
        Vector3d edge = bend.Line.B - bend.Line.A;
        Vector3d normal = new(-edge.Y, edge.X, 0.0);
        Set<int> descendants = Descendants(bends, Set(bend.Child));
        return normal.Unitize()
            ? regions.Traverse(region => (descendants.Contains(region.Panel)
                ? Transform(region.Boundary, point => point + (normal * bend.Projection.NeutralShiftMm))
                    .Map(loop => region with { Boundary = loop })
                : Fin.Succ(region)).ToValidation()).As().ToFin()
            : Fin.Fail<Seq<PanelRegion>>(new GeometryFault.DegenerateInput(Kind.Line, None, "flat:neutral-axis").ToError());
    }

    private static Set<int> Descendants(Seq<BendLine> bends, Set<int> seed) {
        Set<int> closure = seed.Union(bends.Filter(bend => seed.Contains(bend.Parent)).Map(static bend => bend.Child).ToSet());
        return closure.Count > seed.Count ? Descendants(bends, closure) : closure;
    }

    private static Fin<SheetFeatureEvidence> FeatureOf(
        SheetForm feature,
        FormPolicy policy,
        ProcessBudget.Formed forming) => feature.Switch(
        state: (Policy: policy, Forming: forming),
        bend: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-bend")),
        hem: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-hem")),
        jog: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-jog")),
        curl: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-curl")),
        bead: static (state, value) => FeatureFromLoop(value, value.Path, value.Width.Millimeters, value.Depth.Millimeters, state.Policy, state.Forming),
        louver: static (state, value) => FeatureFromLoop(value, value.Aperture, 0.0, value.Height.Millimeters, state.Policy, state.Forming),
        emboss: static (state, value) => FeatureFromLoop(value, value.Footprint, 0.0, value.Height.Millimeters, state.Policy, state.Forming),
        dimple: static (state, value) => FeatureFromLoop(value, value.Footprint, value.ToolRadius.Millimeters, value.Depth.Millimeters, state.Policy, state.Forming));

    private static Fin<SheetFeatureEvidence> Feature(
        SheetForm form,
        double developedMm,
        double strain,
        FormPolicy policy,
        ProcessBudget.Formed forming) =>
        double.IsFinite(developedMm) && developedMm >= 0.0
            && double.IsFinite(strain) && strain >= 0.0
            && strain <= Math.Min(policy.FeatureStrainLimit, forming.LimitStrain)
                ? Fin.Succ(new SheetFeatureEvidence(form, developedMm, strain))
                : Fin.Fail<SheetFeatureEvidence>(FabricationFault.UnfoldInfeasible(1, 1));

    private static Fin<SheetFeatureEvidence> FeatureFromLoop(
        SheetForm form,
        Loop loop,
        double widthMm,
        double heightMm,
        FormPolicy policy,
        ProcessBudget.Formed forming) =>
        from measured in loop.Apply(new ProfileOp.Measure())
        from bounded in loop.Apply(new ProfileOp.Bound())
        from path in measured is ProfileResult.Measure metric
            ? Fin.Succ(metric.Path.Millimeters)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-measure"))
        from diagonal in bounded is ProfileResult.Bound bound
            ? Fin.Succ(bound.Box.Diagonal.Length)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:feature-bound"))
        from grainFactors in policy.Grain.Map(grain => toSeq(Enumerable.Range(0, loop.Count))
            .Traverse(index => grain.At(loop.At(index + 1) - loop.At(index)).ToValidation()).As().ToFin())
            .IfNone(Fin.Succ(Seq(1.0)))
        let grainFactor = grainFactors.Fold(0.0, Math.Max)
        from evidence in Feature(form, path + widthMm, grainFactor * heightMm / Math.Max(widthMm, diagonal), policy, forming)
        select evidence;

    private static Fin<TopologyReceipt> Regions(Arr<Loop> flat) =>
        PolygonAlgebra.Apply(new PolygonOp.Inspect(flat.ToSeq(), new PolygonQuery.Topology(PolygonFill.NonZero)))
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result)
                : Fin.Fail<TopologyReceipt>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:topology-trace")));

    private static Fin<Seq<ReliefSeat>> ReliefSeats(Arr<Loop> flat, Seq<BendLine> bends, TopologyReceipt topology, FormPolicy policy) {
        double probe = Math.Max(policy.ThicknessMm, flat[0].Tolerance.Absolute.Value * 2.0);
        return bends.Bind(static bend => Seq((Bend: bend, At: bend.Line.A), (Bend: bend, At: bend.Line.B)))
            .Traverse(pair => (
                from axis in Unit(pair.Bend.Line.B - pair.Bend.Line.A, "flat:relief-axis")
                let left = new Vector3d(-axis.Y, axis.X, 0.0)
                from contained in Seq(left, -left).Traverse(direction => flat
                    .Traverse(loop => loop.Apply(new ProfileOp.Contains(pair.At + (direction * probe)))
                        .Map(static result => result is ProfileResult.Contains inside && inside.Value)
                        .ToValidation()).As().ToFin()
                    .Map(static hits => hits.Exists(identity)).ToValidation()).As().ToFin()
                from inward in contained.Filter(identity).Count == 1
                    ? contained.Head
                        .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "flat:relief-side").ToError())
                        .Map(hit => hit ? left : -left)
                    : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "flat:relief-side").ToError())
                from holeClear in topology.Nodes.Filter(static node => node.IsHole)
                    .Traverse(node => node.Boundary.Apply(new ProfileOp.Contains(pair.At))
                        .Map(static result => result is ProfileResult.Contains inside && inside.Value)
                        .ToValidation()).As().ToFin()
                select new ReliefSeat(
                    pair.At,
                    axis,
                    inward,
                    policy.Relief.Width(policy.ThicknessMm),
                    policy.Relief.Depth(policy.ThicknessMm, pair.Bend.InsideRadiusMm),
                    pair.Bend.InsideRadiusMm,
                    Set(pair.Bend.Index),
                    holeClear.Exists(identity))).ToValidation()).As().ToFin()
            .Map(seats => Corners(seats, probe));
    }

    // Bend terminations sharing one point are one corner seat sized for the deepest meeting bend, so a corner
    // takes a single relief instead of overlapping cuts and a lone bend against free boundary still seats alone.
    private static Seq<ReliefSeat> Corners(Seq<ReliefSeat> seats, double probeMm) =>
        seats.Fold(Seq<ReliefSeat>(), (held, seat) => held
            .Map(static (row, index) => (Row: row, Index: index))
            .Find(row => row.Row.At.DistanceTo(seat.At) <= probeMm)
            .Match(
                Some: found => held.Map((row, index) => index != found.Index ? row : row with {
                    WidthMm = Math.Max(row.WidthMm, seat.WidthMm),
                    DepthMm = Math.Max(row.DepthMm, seat.DepthMm),
                    InsideRadiusMm = Math.Max(row.InsideRadiusMm, seat.InsideRadiusMm),
                    Meeting = row.Meeting.Union(seat.Meeting),
                    ExistingClearance = row.ExistingClearance && seat.ExistingClearance,
                }),
                None: () => held.Add(seat)));

    private static Fin<Arr<Loop>> Difference(Arr<Loop> flat, Seq<Loop> cuts) =>
        PolygonAlgebra.Apply(new PolygonOp.Boolean(flat.ToSeq(), cuts, BooleanOp.Difference, PolygonFill.NonZero))
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary).ToArr())
                : Fin.Fail<Arr<Loop>>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, "flat:relief-trace")));

    internal static Fin<Loop> Rectangular(ReliefSeat seat, Context tolerance) =>
        ReliefPolygon(seat, tolerance, static (center, along, normal, halfWidth, halfDepth) => [
            center - (along * halfWidth),
            center + (along * halfWidth),
            center + (along * halfWidth) + (normal * halfDepth * 2.0),
            center - (along * halfWidth) + (normal * halfDepth * 2.0),
        ], Arr<double>());

    // Slots close with one semicircle across the far edge: `Bulges[i]` owns the span opening at `Vertices[i]`,
    // so only index 2 — the returning width edge — carries the half turn.
    internal static Fin<Loop> Obround(ReliefSeat seat, Context tolerance) =>
        ReliefPolygon(seat, tolerance, static (center, along, normal, halfWidth, halfDepth) => [
            center - (along * halfWidth),
            center + (along * halfWidth),
            center + (along * halfWidth) + (normal * halfDepth * 2.0),
            center - (along * halfWidth) + (normal * halfDepth * 2.0),
        ], Arr(0.0, 0.0, 1.0, 0.0));

    internal static Fin<Loop> Tear(ReliefSeat seat, Context tolerance) =>
        ReliefPolygon(seat, tolerance, static (center, along, normal, halfWidth, halfDepth) => [
            center,
            center + (along * halfWidth) + (normal * halfDepth),
            center + (normal * halfDepth * 2.0),
            center - (along * halfWidth) + (normal * halfDepth),
        ], Arr(0.0, Math.Tan(Math.PI / 8.0), Math.Tan(Math.PI / 8.0), 0.0));

    // Four cardinal points at one radius about a seated centre keep the cut circular at every width-to-depth
    // ratio; sharing the width and depth half-extents as two radii would emit an ellipse under a bulge quarter turn.
    internal static Fin<Loop> Circular(ReliefSeat seat, Context tolerance) =>
        ReliefPolygon(seat, tolerance, static (center, along, normal, halfWidth, halfDepth) => [
            center + (normal * Math.Max(halfDepth, halfWidth)) - (along * halfWidth),
            center + (normal * (Math.Max(halfDepth, halfWidth) - halfWidth)),
            center + (normal * Math.Max(halfDepth, halfWidth)) + (along * halfWidth),
            center + (normal * (Math.Max(halfDepth, halfWidth) + halfWidth)),
        ], Arr(Math.Tan(Math.PI / 8.0), Math.Tan(Math.PI / 8.0), Math.Tan(Math.PI / 8.0), Math.Tan(Math.PI / 8.0)));

    private static Fin<Loop> ReliefPolygon(
        ReliefSeat seat,
        Context tolerance,
        Func<Point3d, Vector3d, Vector3d, double, double, Arr<Point3d>> vertices,
        Arr<double> bulges) {
        return Loop.Admit(
            vertices(seat.At, seat.Along, seat.Inward, seat.WidthMm / 2.0, seat.DepthMm / 2.0),
            closed: true,
            bulges,
            tolerance);
    }

    private static Fin<Vector3d> Unit(Vector3d vector, string locus) =>
        vector.Unitize()
            ? Fin.Succ(vector)
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Line, None, locus).ToError());

    private static Fin<UnfoldResult> AdmitResult(
        Arr<Loop> flat,
        Seq<BendLine> bends,
        double thicknessMm,
        MaterialSpec material,
        ProcessBudget.Formed forming,
        UnfoldEvidence evidence) =>
        UnfoldResult.Validate(flat, bends, thicknessMm, material, forming, evidence, out UnfoldResult result) is { } error
            ? Fin.Fail<UnfoldResult>(new FabricationFault.PolicyInadmissible(FabConcern.Forming, error.Message))
            : Fin.Succ(result);

    private static Option<int> Local(UvIsland island, int source) =>
        toSeq(Enumerable.Range(0, island.Vertices.Count)).Find(index => island.Vertices[index] == source);

    // Kernel `UvIsland.Boundary` owns the island walk — once-counted edges, face-inherited winding, outer CCW and
    // holes CW, branching typed — so this owner only terminates its Chain carrier into the Loop atom.
    private static Fin<Seq<Loop>> Rings(Seq<Chain> chains, Context tolerance) =>
        chains.TraverseM(chain => Loop.Admit(
            toSeq(chain.Points).ToArr(), chain.Closed, Arr<double>(), tolerance)).As();

    private static Fin<Loop> Transform(Loop loop, Func<Point3d, Point3d> apply) =>
        Loop.Admit(loop.Vertices.Map(apply).ToArr(), loop.Closed, loop.Bulges, loop.Tolerance);

    private static Point3d Planar(Point2d point) => new(point.X, point.Y, 0.0);

    // Every preimage on this page composes the ONE `Rasm.Element` `CanonicalWriter`: a page-local `double` framing is
    // what let two artifacts with identical content mint different keys depending on which page keyed them, and
    // `Double` additionally folds `-0.0` and every NaN payload to one pattern the raw IEEE write leaves forked.
    private static byte[] Canonical(UnfoldResult unfold, Seq<BendStep> bends) {
        CanonicalWriter writer = new(unfold.Evidence.Topology.Tolerance.Absolute.Value);
        _ = writer.Double(unfold.ThicknessMm)
            .String(unfold.Material.Family.Key)
            .String(unfold.Material.Identity.Grade)
            .Double(unfold.Forming.TensileRm)
            .Double(unfold.Forming.KFactor)
            .Double(unfold.Forming.SpringbackRatio)
            .Double(unfold.Forming.MinBendRadiusFactor)
            .Double(unfold.Forming.FlowStressMpa)
            .Double(unfold.Forming.LimitStrain);
        Write(writer, unfold.Forming.Evidence);
        _ = writer.Ordinal(unfold.Flat.Count);
        unfold.Flat.Iter(loop => Write(writer, loop));
        _ = writer.Ordinal(unfold.Bends.Count);
        unfold.Bends.Iter(bend => {
            _ = writer.Ordinal(bend.Index);
            Write(writer, bend.Line.A); Write(writer, bend.Line.B);
            _ = writer.Ordinal(bend.Parent).Ordinal(bend.Child)
                .Double(bend.AngleDeg).Double(bend.InsideRadiusMm).Double(bend.K)
                .Double(bend.Projection.AllowanceMm).Double(bend.Projection.SetbackMm)
                .Double(bend.Projection.DeductionMm).Double(bend.Projection.NeutralShiftMm);
            Write(writer, bend.Form);
            _ = writer.Ordinal(bend.Prerequisites.Count);
            bend.Prerequisites.Order().Iter(prerequisite => writer.Ordinal(prerequisite));
        });
        _ = writer.Ordinal(unfold.Evidence.Features.Count);
        unfold.Evidence.Features.Iter(feature => {
            Write(writer, feature.Form);
            _ = writer.Double(feature.DevelopedMm).Double(feature.PeakStrain);
        });
        _ = writer.Ordinal(unfold.Evidence.Reliefs.Count);
        unfold.Evidence.Reliefs.Iter(relief => {
            Write(writer, relief.At);
            _ = writer.Double(relief.Along.X).Double(relief.Along.Y).Double(relief.Along.Z)
                .Double(relief.Inward.X).Double(relief.Inward.Y).Double(relief.Inward.Z)
                .Double(relief.WidthMm).Double(relief.DepthMm).Double(relief.InsideRadiusMm)
                .Ordinal(relief.Meeting.Count);
            relief.Meeting.Order().Iter(bend => writer.Ordinal(bend));
            _ = writer.Bool(relief.ExistingClearance);
        });
        _ = writer.Bool(unfold.Evidence.Isometry.IsSome);
        unfold.Evidence.Isometry.Iter(receipt => writer
            .Ordinal(receipt.Strips).Ordinal(receipt.Rulings)
            .Double(receipt.MaxIsometry).Double(receipt.MeanIsometry).Double(receipt.MaxTorsal)
            .Ordinal(receipt.Components));
        _ = writer.Ordinal(unfold.Evidence.Panels.Count);
        unfold.Evidence.Panels.Iter(panel => {
            _ = writer.Ordinal(panel.Panel);
            Write(writer, panel.Boundary);
        });
        _ = writer.Ordinal(unfold.Evidence.NeutralAxis.Count);
        unfold.Evidence.NeutralAxis.Iter(row => writer.Ordinal(row.Bend).Double(row.ShiftMm));
        _ = writer.String(unfold.Evidence.Topology.Fill.Key)
            .Double(unfold.Evidence.Topology.Tolerance.Absolute.Value)
            .Double(unfold.Evidence.Topology.Plane)
            .Ordinal(unfold.Evidence.Topology.Nodes.Count);
        unfold.Evidence.Topology.Nodes.Iter(node => {
            _ = writer.Ordinal(node.Index).Bool(node.Parent.IsSome);
            node.Parent.Iter(parent => writer.Ordinal(parent));
            _ = writer.Ordinal(node.Depth).Bool(node.IsHole);
            Write(writer, node.Boundary);
            _ = writer.Double(node.SignedArea);
        });
        _ = writer.Ordinal(bends.Count);
        bends.Iter(bend => {
            Write(writer, bend.Line.A); Write(writer, bend.Line.B);
            _ = writer.Ordinal(bend.Order).Double(bend.AngleDeg).Double(bend.RadiusMm)
                .Double(bend.KFactor).Double(bend.OverbendDeg).Double(bend.TonnageKn)
                .String(bend.Orientation.Key);
        });
        return writer.ToBytes().ToArray();
    }

    private static void Write(CanonicalWriter writer, Point3d point) =>
        _ = writer.Double(point.X).Double(point.Y).Double(point.Z);

    private static void Write(CanonicalWriter writer, Loop loop) {
        _ = writer.Ordinal(loop.Count).Bool(loop.Closed).Double(loop.Tolerance.Absolute.Value);
        loop.Vertices.Iter(point => Write(writer, point));
        loop.Bulges.Iter(bulge => writer.Double(bulge));
    }

    private static void Write(CanonicalWriter writer, BudgetEvidence evidence) {
        _ = writer.Double(evidence.State.TemperatureC).Double(evidence.State.Hardness)
            .Double(evidence.State.StrainRate).Double(evidence.State.Strain)
            .Double(evidence.State.MoistureFraction).Double(evidence.State.GrainSizeUm)
            .Double(evidence.PowerW)
            .Bool(evidence.Energy.Joules.IsSome);
        evidence.Energy.Joules.Iter(value => writer.Double(value));
        _ = writer.Bool(evidence.Energy.Seconds.IsSome);
        evidence.Energy.Seconds.Iter(value => writer.Double(value));
        _ = writer.String(evidence.Material.Family.Key).String(evidence.Material.Identity.Grade);
    }

    private static void Write(CanonicalWriter writer, SheetForm form) => _ = form.Switch(
        state: writer,
        bend: static (target, _) => target.String(nameof(SheetForm.Bend)),
        hem: static (target, value) => target.String(nameof(SheetForm.Hem)).String(value.Kind.Key).Double(value.Gap.Millimeters),
        jog: static (target, value) => target.String(nameof(SheetForm.Jog)).Double(value.Offset.Millimeters).Double(value.Spacing.Millimeters),
        curl: static (target, value) => target.String(nameof(SheetForm.Curl)).Double(value.InsideRadius.Millimeters).Double(value.Sweep.Radians),
        bead: static (target, value) => { _ = target.String(nameof(SheetForm.Bead)); Write(target, value.Path); return target.Double(value.Width.Millimeters).Double(value.Depth.Millimeters); },
        louver: static (target, value) => { _ = target.String(nameof(SheetForm.Louver)); Write(target, value.Aperture); return target.Double(value.Height.Millimeters).Double(value.Opening.Radians); },
        emboss: static (target, value) => { _ = target.String(nameof(SheetForm.Emboss)); Write(target, value.Footprint); return target.Double(value.Height.Millimeters).Double(value.Draft.Radians); },
        dimple: static (target, value) => { _ = target.String(nameof(SheetForm.Dimple)); Write(target, value.Footprint); return target.Double(value.Depth.Millimeters).Double(value.ToolRadius.Millimeters); });

    private sealed record SheetAssembly(
        Arr<Loop> Flat,
        Seq<BendLine> Bends,
        Seq<SheetForm> Features,
        Option<DevelopmentReceipt> Isometry,
        Seq<PanelRegion> Panels);

    private sealed record PanelState(Arr<Loop> Panels, Arr<Loop> Flat, HashMap<int, PlanarPlacement> Placement) {
        public static PanelState Start(Arr<Loop> panels, int root) => new(panels, panels, HashMap((root, PlanarPlacement.Identity)));
    }

    private readonly record struct PlanarPlacement(double Cos, double Sin, double Tx, double Ty) {
        public static readonly PlanarPlacement Identity = new(1.0, 0.0, 0.0, 0.0);

        public Point3d Apply(Point3d point) => new(
            (Cos * point.X) - (Sin * point.Y) + Tx,
            (Sin * point.X) + (Cos * point.Y) + Ty,
            point.Z);

        public Edge3 Apply(Edge3 edge) => new(Apply(edge.A), Apply(edge.B));

        public static Fin<PlanarPlacement> Between(Edge3 source, Edge3 target, double gap) {
            Vector3d from = source.B - source.A;
            Vector3d to = target.A - target.B;
            if (!from.Unitize() || !to.Unitize())
                return Fin.Fail<PlanarPlacement>(new GeometryFault.DegenerateInput(Kind.Line, None, "flat:panel-edge").ToError());
            double cos = (from * to) / (from.Length * to.Length);
            double sin = Vector3d.CrossProduct(from, to).Z;
            Vector3d normal = new(-to.Y, to.X, 0.0);
            Point3d rotated = new((cos * source.A.X) - (sin * source.A.Y), (sin * source.A.X) + (cos * source.A.Y), source.A.Z);
            Vector3d shift = target.B - rotated + (normal * gap);
            return Fin.Succ(new PlanarPlacement(cos, sin, shift.X, shift.Y));
        }
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
