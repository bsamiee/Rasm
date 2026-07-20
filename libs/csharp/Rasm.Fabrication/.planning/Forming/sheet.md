# [RASM_FABRICATION_FLAT_PATTERN]

`FlatPattern` owns sheet development from admitted formed-panel or kernel-surface evidence to one neutral-axis-correct flat, bend topology, feature evidence, relief census, and content-keyed result. `FormPolicy` admits material, thickness, calibration, development, grain, relief, and feature limits once; every interior length is machining-canonical millimeters.

`FlatPattern.Unfold`, `UnfoldResult`, and `FlatPattern.Formed` preserve the `FabricationPolicy.Form` wire. Kernel `Development.Apply` owns surface isometry, `PolygonAlgebra.Apply` owns region topology and relief subtraction, and `ContentKey.Of` owns artifact identity.

## [01]-[INDEX]

- [01]-[SHEET_DEVELOPMENT]: Generated admission, parameterized bend physics, panel and surface development, neutral-axis placement, sheet-feature evidence, relief topology, and result projection.

## [02]-[SHEET_DEVELOPMENT]

- Owner: `FormPolicy` owns admitted sheet intent; `SheetSource` owns formed-panel, component, and kernel-surface ingress; `SheetLink` owns panel and surface adjacency; `SheetForm` owns line-form and local-feature evidence; `FlatPattern` owns development and projection.
- Cases: `KSource` resolves table, measured-coupon, `DIN 6935`, and material-physics neutral-axis positions; `HemKind` carries per-row sweep and inside-radius laws; `ReliefKind` sizes and generates rectangular, obround, tear, and circular reliefs; `SheetForm` carries each feature's distinct evidence and its tooling demand.
- Entry: `FlatPattern.Unfold(FormPolicy, FabricationInput)` is the frozen development seam, and `FlatPattern.Formed(UnfoldResult, Seq<BendStep>)` is the frozen result projection.
- Auto: Panel links derive a topological placement order; the generated grain field gates bend radius and loop-feature strain; surface links shift kernel islands by neutral-axis deltas; every bend endpoint enters one relief-seat census that folds co-terminating bends into one corner seat sized against the formed radius; one `PolygonOp.Boolean` subtracts admitted reliefs.
- Receipt: `UnfoldResult` preserves flat regions, bend topology, forming physics, kernel isometry, neutral-axis displacement, feature evidence, relief evidence, and material identity.
- Packages: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `CommunityToolkit.HighPerformance`, `QuikGraph`, `UnitsNet`, `Rasm`, and the `Geometry2D` owner compose the surface.
- Growth: A K convention is one `KSource` row, a hem geometry is one `HemKind` row, a relief geometry is one `ReliefKind` row, a sheet feature is one `SheetForm` case, a link modality is one `SheetLink` case, and a new source is one `SheetSource` case with one total dispatch arm.
- Boundary: Forming owns neutral-axis and feature development; kernel isometry, planar topology, process physics, and content identity remain at their canonical owners.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
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
        .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:k-table").ToError())
        .Bind(table => table.Resolve(query)));
    public static readonly KSource Coupon = new("coupon", static query => query.Coupon
        .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:k-coupon").ToError())
        .Bind(coupon => coupon.Calibrate(query)));
    public static readonly KSource Din6935 = new("din-6935", static query =>
        Fin.Succ(Math.Clamp(0.65 + (0.5 * Math.Log10(query.RadiusMm / query.ThicknessMm)), 0.5, 1.0) / 2.0));
    public static readonly KSource Physics = new("physics", static query => Fin.Succ(query.Forming.KFactor));

    [UseDelegateFromConstructor]
    private partial Fin<double> ResolveAdmitted(KQuery query);

    public Fin<double> Resolve(KQuery? query) => Optional(query)
        .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:k-query").ToError())
        .Bind(ResolveAdmitted)
        .Bind(static k => double.IsFinite(k) && k is > 0.0 and < 1.0
            ? Fin.Succ(k)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:k-result").ToError()));
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

    // A line form whose geometry demands dedicated tooling overrides the policy default at candidate admission;
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
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:k-coupon-applicability").ToError());
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
            .HeadOrNone()
            .Map(band => band.KLow + ((band.KHigh - band.KLow)
                * ((query.RadiusMm / query.ThicknessMm) - band.RtLow) / (band.RtHigh - band.RtLow)))
            .Filter(static k => double.IsFinite(k) && k is > 0.0 and < 1.0)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:k-band:{query.Material.Family.Key}:{query.Method.Key}").ToError());
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
            return Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:grain-axis").ToError());
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
            ? Fin.Fail<UnfoldResult>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:input").ToError())
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
            : Fin.Fail<ProcessBudget.Formed>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:forming-budget").ToError()));

    private static Fin<double> BendLength(SheetSource source) => source.Switch(
        panels: static row => Total(row.Links.Map(static link => link.ParentEdge.A.DistanceTo(link.ParentEdge.B))),
        component: static row => Total(row.Links.Map(static link => link.ParentEdge.A.DistanceTo(link.ParentEdge.B))),
        surface: static row => SurfaceLength(row));

    private static Fin<double> SurfaceLength(SheetSource.Surface source) {
        Mesh mesh = source.Value.Mesh.DuplicateNative();
        return source.Links.Traverse(link => (link.SourceA < mesh.Vertices.Count && link.SourceB < mesh.Vertices.Count
                ? Fin.Succ(mesh.Vertices.Point3dAt(link.SourceA).DistanceTo(mesh.Vertices.Point3dAt(link.SourceB)))
                : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:surface-bend-length").ToError()))
            .ToValidation()).As().ToFin().Bind(Total);
    }

    private static Fin<double> Total(Seq<double> lengths) {
        double total = lengths.Fold(0.0, static (sum, length) => sum + length);
        return double.IsFinite(total) && total > 0.0
            ? Fin.Succ(total)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:bend-length").ToError());
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
            : Fin.Fail<DevelopmentResult.Unrolled>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:surface-result").ToError())
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
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:surface-parent").ToError())
        from child in islands.Map((island, index) => (Island: island, Index: index)).Find(row => row.Island.Chart == link.Child)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:surface-child").ToError())
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
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:bend").ToError())
               from query in KQuery.Validate(
                   policy.Material,
                   policy.Method,
                   resolvedRadius,
                   policy.ThicknessMm,
                   policy.KFactors,
                   policy.Coupon,
                   forming,
                   out KQuery admitted) is { } error
                    ? Fin.Fail<KQuery>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
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
            bead: static (_, _) => Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:line-bead").ToError()),
            louver: static (_, _) => Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:line-louver").ToError()),
            emboss: static (_, _) => Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:line-emboss").ToError()),
            dimple: static (_, _) => Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:line-dimple").ToError()));
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
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:parent:{link.Parent}").ToError())
        from resolved in bends.Find(row => row.Child == link.Child)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:bend:{link.Child}").ToError())
        from child in PlanarPlacement.Between(link.ChildEdge, parent.Apply(link.ParentEdge), resolved.Projection.AllowanceMm)
        from loop in Transform(state.Panels[link.Child], child.Apply)
        select state with {
            Flat = state.Flat.SetItem(link.Child, loop),
            Placement = state.Placement.Add(link.Child, child),
        };

    private static Fin<Seq<PanelRegion>> Neutralize(Seq<UvIsland> islands, Seq<BendLine> bends, Context tolerance) =>
        islands.Map((island, index) => (Island: island, Index: index))
            .Traverse(row => Boundaries(row.Island, tolerance)
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
            : Fin.Fail<Seq<PanelRegion>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:neutral-axis").ToError());
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
        bend: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-bend").ToError()),
        hem: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-hem").ToError()),
        jog: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-jog").ToError()),
        curl: static (_, _) => Fin.Fail<SheetFeatureEvidence>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-curl").ToError()),
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
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-measure").ToError())
        from diagonal in bounded is ProfileResult.Bound bound
            ? Fin.Succ(bound.Box.Diagonal.Length)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:feature-bound").ToError())
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
                : Fin.Fail<TopologyReceipt>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:topology-trace").ToError()));

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
                        .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:relief-side").ToError())
                        .Map(hit => hit ? left : -left)
                    : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:relief-side").ToError())
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
        PolygonAlgebra.Apply(new PolygonOp.Boolean(flat.ToSeq(), cuts, PolygonBoolean.Difference, PolygonFill.NonZero))
            .Bind(static trace => trace is PolygonTrace.Regions regions
                ? Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary).ToArr())
                : Fin.Fail<Arr<Loop>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:relief-trace").ToError()));

    internal static Fin<Loop> Rectangular(ReliefSeat seat, Context tolerance) =>
        ReliefPolygon(seat, tolerance, static (center, along, normal, halfWidth, halfDepth) => [
            center - (along * halfWidth),
            center + (along * halfWidth),
            center + (along * halfWidth) + (normal * halfDepth * 2.0),
            center - (along * halfWidth) + (normal * halfDepth * 2.0),
        ], Arr<double>());

    // A slot closed by one semicircle across its far edge: `Bulges[i]` owns the span opening at `Vertices[i]`,
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
            : Fin.Fail<Vector3d>(new GeometryFault.DegenerateInput(Kind.Brep, -1, locus).ToError());

    private static Fin<UnfoldResult> AdmitResult(
        Arr<Loop> flat,
        Seq<BendLine> bends,
        double thicknessMm,
        MaterialSpec material,
        ProcessBudget.Formed forming,
        UnfoldEvidence evidence) =>
        UnfoldResult.Validate(flat, bends, thicknessMm, material, forming, evidence, out UnfoldResult result) is { } error
            ? Fin.Fail<UnfoldResult>(new GeometryFault.DegenerateInput(Kind.Brep, -1, error.Message).ToError())
            : Fin.Succ(result);

    private static Option<int> Local(UvIsland island, int source) =>
        toSeq(Enumerable.Range(0, island.Vertices.Count)).Find(index => island.Vertices[index] == source);

    private static Fin<Seq<Loop>> Boundaries(UvIsland island, Context tolerance) {
        Seq<(int A, int B)> boundary = toSeq(island.Faces)
            .Bind(static face => Seq(
                (A: face.A, B: face.B),
                (A: face.B, B: face.C),
                (A: face.C, B: face.A)))
            .GroupBy(static edge => (Math.Min(edge.A, edge.B), Math.Max(edge.A, edge.B)))
            .Filter(static group => group.Count() == 1)
            .Map(static group => group.First())
            .ToSeq();
        return boundary.Count >= 3
            ? BoundaryRegions(boundary, island, tolerance, Seq<Loop>())
            : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:island:{island.Chart}").ToError());
    }

    private static Fin<Seq<Loop>> BoundaryRegions(
        Seq<(int A, int B)> remaining,
        UvIsland island,
        Context tolerance,
        Seq<Loop> regions) =>
        remaining.IsEmpty
            ? Fin.Succ(regions)
            : from cycle in BoundaryCycle(remaining, island.Chart)
              from loop in Loop.Admit(cycle.Vertices.Map(index => Planar(island.Uv[index])).ToArr(), closed: true, Arr<double>(), tolerance)
              from result in BoundaryRegions(
                  remaining.Filter(edge => !cycle.Edges.Contains(Normalize(edge))),
                  island,
                  tolerance,
                  regions.Add(loop))
              select result;

    private static Fin<(Seq<int> Vertices, Set<(int A, int B)> Edges)> BoundaryCycle(
        Seq<(int A, int B)> boundary,
        ChartId chart) =>
        boundary.Head
            .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:island-cycle:{chart}").ToError())
            .Bind(seed => AdvanceBoundary(boundary, chart, seed.A, seed.B, Seq(seed.A), Set(Normalize(seed))));

    private static Fin<(Seq<int> Vertices, Set<(int A, int B)> Edges)> AdvanceBoundary(
        Seq<(int A, int B)> boundary,
        ChartId chart,
        int origin,
        int current,
        Seq<int> vertices,
        Set<(int A, int B)> used) =>
        current == origin
            ? vertices.Count >= 3
                ? Fin.Succ((vertices, used))
                : Fin.Fail<(Seq<int>, Set<(int, int)>)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:island-cycle:{chart}").ToError())
            : used.Count > boundary.Count
                ? Fin.Fail<(Seq<int>, Set<(int, int)>)>(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:island-cycle:{chart}").ToError())
                : from edge in boundary
                      .Filter(edge => !used.Contains(Normalize(edge)) && (edge.A == current || edge.B == current))
                      .OrderBy(edge => (edge.A == current ? edge.B : edge.A) == origin ? 1 : 0)
                      .HeadOrNone()
                      .ToFin(new GeometryFault.DegenerateInput(Kind.Brep, -1, $"flat:island-open:{chart}").ToError())
                  from cycle in AdvanceBoundary(
                      boundary,
                      chart,
                      origin,
                      edge.A == current ? edge.B : edge.A,
                      vertices.Add(current),
                      used.Add(Normalize(edge)))
                  select cycle;

    private static (int A, int B) Normalize((int A, int B) edge) =>
        edge.A <= edge.B ? edge : (edge.B, edge.A);

    private static Fin<Loop> Transform(Loop loop, Func<Point3d, Point3d> apply) =>
        Loop.Admit(loop.Vertices.Map(apply).ToArr(), loop.Closed, loop.Bulges, loop.Tolerance);

    private static Point3d Planar(Point2d point) => new(point.X, point.Y, 0.0);

    private static byte[] Canonical(UnfoldResult unfold, Seq<BendStep> bends) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, unfold.ThicknessMm);
        Write(writer, unfold.Material.Family.Key);
        Write(writer, unfold.Material.Identity.Grade);
        Write(writer, unfold.Forming.TensileRm);
        Write(writer, unfold.Forming.KFactor);
        Write(writer, unfold.Forming.SpringbackRatio);
        Write(writer, unfold.Forming.MinBendRadiusFactor);
        Write(writer, unfold.Forming.FlowStressMpa);
        Write(writer, unfold.Forming.LimitStrain);
        Write(writer, unfold.Forming.Evidence);
        Write(writer, unfold.Flat.Count);
        unfold.Flat.Iter(loop => Write(writer, loop));
        Write(writer, unfold.Bends.Count);
        unfold.Bends.Iter(bend => {
            Write(writer, bend.Index);
            Write(writer, bend.Line.A); Write(writer, bend.Line.B);
            Write(writer, bend.Parent); Write(writer, bend.Child);
            Write(writer, bend.AngleDeg); Write(writer, bend.InsideRadiusMm); Write(writer, bend.K);
            Write(writer, bend.Projection.AllowanceMm); Write(writer, bend.Projection.SetbackMm);
            Write(writer, bend.Projection.DeductionMm); Write(writer, bend.Projection.NeutralShiftMm);
            Write(writer, bend.Form);
            Write(writer, bend.Prerequisites.Count);
            bend.Prerequisites.Order().Iter(prerequisite => Write(writer, prerequisite));
        });
        Write(writer, unfold.Evidence.Features.Count);
        unfold.Evidence.Features.Iter(feature => {
            Write(writer, feature.Form);
            Write(writer, feature.DevelopedMm);
            Write(writer, feature.PeakStrain);
        });
        Write(writer, unfold.Evidence.Reliefs.Count);
        unfold.Evidence.Reliefs.Iter(relief => {
            Write(writer, relief.At); Write(writer, relief.Along.X); Write(writer, relief.Along.Y); Write(writer, relief.Along.Z);
            Write(writer, relief.Inward.X); Write(writer, relief.Inward.Y); Write(writer, relief.Inward.Z);
            Write(writer, relief.WidthMm); Write(writer, relief.DepthMm); Write(writer, relief.InsideRadiusMm);
            Write(writer, relief.Meeting.Count);
            relief.Meeting.Order().Iter(bend => Write(writer, bend));
            Write(writer, relief.ExistingClearance ? 1 : 0);
        });
        Write(writer, unfold.Evidence.Isometry.IsSome ? 1 : 0);
        unfold.Evidence.Isometry.Iter(receipt => {
            Write(writer, receipt.Strips); Write(writer, receipt.Rulings);
            Write(writer, receipt.MaxIsometry); Write(writer, receipt.MeanIsometry); Write(writer, receipt.MaxTorsal);
            Write(writer, receipt.Components);
        });
        Write(writer, unfold.Evidence.Panels.Count);
        unfold.Evidence.Panels.Iter(panel => {
            Write(writer, panel.Panel); Write(writer, panel.Boundary);
        });
        Write(writer, unfold.Evidence.NeutralAxis.Count);
        unfold.Evidence.NeutralAxis.Iter(row => {
            Write(writer, row.Bend); Write(writer, row.ShiftMm);
        });
        Write(writer, unfold.Evidence.Topology.Fill.Key);
        Write(writer, unfold.Evidence.Topology.Tolerance.Absolute.Value);
        Write(writer, unfold.Evidence.Topology.Plane);
        Write(writer, unfold.Evidence.Topology.Nodes.Count);
        unfold.Evidence.Topology.Nodes.Iter(node => {
            Write(writer, node.Index);
            Write(writer, node.Parent.IsSome ? 1 : 0);
            node.Parent.Iter(parent => Write(writer, parent));
            Write(writer, node.Depth); Write(writer, node.IsHole ? 1 : 0);
            Write(writer, node.Boundary); Write(writer, node.SignedArea);
        });
        Write(writer, bends.Count);
        bends.Iter(bend => {
            Write(writer, bend.Line.A); Write(writer, bend.Line.B);
            Write(writer, bend.Order); Write(writer, bend.AngleDeg); Write(writer, bend.RadiusMm);
            Write(writer, bend.KFactor); Write(writer, bend.OverbendDeg); Write(writer, bend.TonnageKn);
            Write(writer, bend.Orientation.Key);
        });
        return writer.WrittenSpan.ToArray();
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, string value) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
        Write(writer, bytes.Length);
        bytes.CopyTo(writer.GetSpan(bytes.Length));
        writer.Advance(bytes.Length);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Point3d point) {
        Write(writer, point.X); Write(writer, point.Y); Write(writer, point.Z);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Loop loop) {
        Write(writer, loop.Count);
        Write(writer, loop.Closed ? 1 : 0);
        Write(writer, loop.Tolerance.Absolute.Value);
        loop.Vertices.Iter(point => Write(writer, point));
        loop.Bulges.Iter(bulge => Write(writer, bulge));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, BudgetEvidence evidence) {
        Write(writer, evidence.State.TemperatureC); Write(writer, evidence.State.Hardness);
        Write(writer, evidence.State.StrainRate); Write(writer, evidence.State.Strain);
        Write(writer, evidence.State.MoistureFraction); Write(writer, evidence.State.GrainSizeUm);
        Write(writer, evidence.PowerW);
        Write(writer, evidence.Energy.Joules.IsSome ? 1 : 0);
        evidence.Energy.Joules.Iter(value => Write(writer, value));
        Write(writer, evidence.Energy.Seconds.IsSome ? 1 : 0);
        evidence.Energy.Seconds.Iter(value => Write(writer, value));
        Write(writer, evidence.Material.Family.Key); Write(writer, evidence.Material.Identity.Grade);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, SheetForm form) {
        _ = form.Switch<ArrayPoolBufferWriter<byte>, Unit>(
            state: writer,
            bend: static (target, _) => {
                Write(target, nameof(SheetForm.Bend)); return unit;
            },
            hem: static (target, value) => {
                Write(target, nameof(SheetForm.Hem)); Write(target, value.Kind.Key); Write(target, value.Gap.Millimeters); return unit;
            },
            jog: static (target, value) => {
                Write(target, nameof(SheetForm.Jog)); Write(target, value.Offset.Millimeters); Write(target, value.Spacing.Millimeters); return unit;
            },
            curl: static (target, value) => {
                Write(target, nameof(SheetForm.Curl)); Write(target, value.InsideRadius.Millimeters); Write(target, value.Sweep.Radians); return unit;
            },
            bead: static (target, value) => {
                Write(target, nameof(SheetForm.Bead)); Write(target, value.Path); Write(target, value.Width.Millimeters); Write(target, value.Depth.Millimeters); return unit;
            },
            louver: static (target, value) => {
                Write(target, nameof(SheetForm.Louver)); Write(target, value.Aperture); Write(target, value.Height.Millimeters); Write(target, value.Opening.Radians); return unit;
            },
            emboss: static (target, value) => {
                Write(target, nameof(SheetForm.Emboss)); Write(target, value.Footprint); Write(target, value.Height.Millimeters); Write(target, value.Draft.Radians); return unit;
            },
            dimple: static (target, value) => {
                Write(target, nameof(SheetForm.Dimple)); Write(target, value.Footprint); Write(target, value.Depth.Millimeters); Write(target, value.ToolRadius.Millimeters); return unit;
            });
    }

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
                return Fin.Fail<PlanarPlacement>(new GeometryFault.DegenerateInput(Kind.Brep, -1, "flat:panel-edge").ToError());
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
