# [MATERIALS_FASTENER]

THE FASTENER SEED PAGE owns the `ComponentFamily.Fastener` roster and law, the thread-form algebra, and the EN 1993-1-8 single-fastener design values. `StockRow.Threaded` pairs a `ThreadRow` with a `MaterialGrade` fastener row; `StockRow.Plain` carries published nail, dowel, and rivet data — including its own PUBLISHED tensile strength — without a fake thread or bolt grade. Both cases project through ONE `StockFacts` read, so geometry, IFC binding, realization detail, the EC5 dowel check, and the seed law share one correspondence while case-specific admission stays total and ACCUMULATING. Every design value this page emits is a DESIGN resistance already divided by the partial factor its own `DesignBasis` row publishes, so a consumer folds demand against it directly and no arm re-divides — and no factor is spelled here.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerTrait`/`JointTrait` capability rosters, the `FastenerKind`/`ThreadSeries`/`BoltCategory`/`FayingSurface`/`HeadForm`/`ShearPlane` policy vocabularies, the `HexHardware` head-nut-washer dimension set, the `ThreadRow` ISO 68-1 form algebra with its `Threads` owner, the `GradeStep`/`SizeBand`/`FastenerBand` grade vocabulary and the `GradeProperties.Fastener` arm physics (`Admits`/`At`) beside the `MaterialGrade` fastener members, the `Fastening` EN 1993-1-8 shear/tension/punching design values with the ISO 4014 length bands and the EC5 §8.5 dowel-type algebra, the `FastenerDetail` realization bag, and the `FastenerSeed.Roster`/`Law`/`Capacity` triple.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — bolt + grip-plies + shear-planes + head + declared washer over one `(ThreadRow, MaterialGrade, GradeProperties.Fastener, BoltCategory, FayingSurface, HeadForm)` — the `BoltPosition`/`HoleShape`/`BearingDesign` EN 1993-1-8 Table 3.4 bearing geometry, the `PreloadKn` `Fp,C = 0.7·fub·As` projection under its yield ceiling, the `FastenerInstallation` admitted slip-and-torque factor set, the `SlipResistanceKn` §3.9 design value, and the ISO 7089/7090 washer-hardness selection.

## [02]-[FASTENER_FAMILY]

- Owner: `FastenerSeed` owns the `ComponentFamily.Fastener` roster, seed law, and capacity producer; `Threads` owns the thread table and `component#MATERIAL_GRADE` the grade rows; `FastenerKind` owns the complete IFC entity/token binding, the realization token, and its `CapabilitySet<FastenerTrait>` column; `BoltCategory`, `FayingSurface`, `ThreadSeries`, `ShearPlane`, and `HeadForm` own policy; `GradeProperties.Fastener` owns the size-banded grade physics; `Fastening` owns the design values; `FastenerAssembly` owns installed-bolt state; `FastenerDetail` owns the realization bag.
- Cases: kind {`bolt` · `nut` · `nail` · `screw` · `anchor` · `dowel` · `rivet` · `coupler` · `kerf` · `pin`} × stock form {threaded hardware over a `ThreadRow`/`MaterialGrade` pair · plain shank over its published designation, diameter, length, tensile strength, authority, and material pair}; the joint category is a `FastenerAssembly` decision, never a type-row column; the stone-cladding pair carries the `AnchorRole` body/restraint axis driving the contract `AnchorType` stamp.
- Entry: `ComponentSeed.Rows(context, FastenerSeed.Roster, FastenerSeed.Law)` — this page states the roster and the policy, never the fold. `FastenerSeed.Capacity` dispatches the `FastenerPlacement` the connection carries into the matching `CapacityLift`; `Fastening` owns the EN 1993-1-8 §3.6 resistances, the ISO 4014 length bands, and the EC5 §8.5 dowel-type check.
- Packages: Rasm.Numerics (`Dimension` aliased `Count` — the discrete grip-ply/shear-plane columns), Rasm.Domain (`Context`/`FactoryBridge.Accept`, `ICapability`/`CapabilitySet`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `DetailSchema`, `PropertyBag`, `PropertyName`, `PropertyValue`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Circle.Of`/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`SeedLaw`/`ComponentSeed`, `MaterialGrade`+`GradeProperties`, the `capacity#SECTION_CAPACITY` `DesignBasis`/`SafetyFormat`/`CapacityPlacement`, and the sibling `TimberPartialFactor`/`ServiceClass`/`LoadDuration` the EC5 join reads), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]`, `[UseDelegateFromConstructor]`, `[Union]`, `[ComplexValueObject]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`.Apply`/`guard`/`Option`), BCL (`ImmutableArray`, `FrozenDictionary`).
- Growth: a new threaded combination is one `StockRow.Threaded`; a new plain-shank product one `StockRow.Plain`; a new kind one `FastenerKind` row with its trait set and stock case; a new thread one `Threads` entry carrying its own diameter and pitch; a new property class one `MaterialGrade` fastener row on `component#MATERIAL_GRADE`; a new connection category one `BoltCategory` row; a new head geometry one `HeadForm` row; a new bolt-group position one `BoltPosition` row; a new fastener or joint trait one roster row and its membership on the subjects that hold it.
- Boundary: every fastener uses `SectionProfile.Circle` and the seed-built realization bag. Thread semantics and grade payload exist only on `StockRow.Threaded`; `StockRow.Plain` carries its own published diameter, length, tensile strength, authority, and substance/appearance pair — so `StockFacts.UltimateMpa` is OPTIONAL, absent exactly where a threaded row's grade carries no fastener arm, a state the coherence census refuses before any row reaches a capacity read. `Fastening.TimberDowelShearKn` takes the SCALARS EC5 §8.5 consumes plus the two `GradeProperties.Timber` arms whose density and k90 intercept it reads, never a threaded currency a plain product does not carry. The stone-cladding `kerf`/`pin` kinds are CLOSED VOCABULARY without stock — no captured source prints their section dimensions to the two-source bar — so a proven product lands as one `StockRow.Plain` row.
- Boundary: this page emits EN 1993-1-8 and EN 1995-1-1 design resistances and NOTHING ELSE, and it spells NO partial factor. `Fastening.JointFactor` reads γM2 off the `capacity#SECTION_CAPACITY` `DesignBasis` row the placement declares (`en1993-1-8` is the joints row), and it REFUSES a resistance-factor basis: an AISC §J3 verdict divides no nominal by γM2, so a φ-format basis passed through publishes a resistance this page never computed. `GradeProperties.Fastener.EurocodeAlphaV` is `Some` only for the seven property classes EN 1993-1-8 Table 3.1 tabulates, so a SAE, ASTM, 9.8, or 12.9 grade FAILS out of the Eurocode resistances rather than borrowing an α_v the code never published for it. The published mechanical band, the preload, and the stock identity stay total over every grade, because those are each body's own specification data.
- Boundary: the retired `bool Metric` on the grade row DERIVES — the thread system is the authority's PRINT system, so `Admits` spells `(Authority == ComponentAuthority.En) == thread.Series.Metric` reading the owning `MaterialGrade` row's own authority column. `Admits`/`At` therefore land on `MaterialGrade` rather than on the arm: the arm carries no authority, and a member seated there takes one as an argument the call site supplies. A non-fastener grade answers `false` and `None` respectively, the arm mismatch stated at the refusal site.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using Count = Rasm.Numerics.Dimension;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerTrait : ICapability<FastenerTrait> {
    public static readonly FastenerTrait Threaded = new("threaded", rank: 0);
    public static readonly FastenerTrait Headed   = new("headed",   rank: 1);
    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointTrait : ICapability<JointTrait> {
    public static readonly JointTrait Shear     = new("shear",     rank: 0);
    public static readonly JointTrait Preloaded = new("preloaded", rank: 1);
    public int Rank { get; }
}

public enum AnchorRole : byte { None = 0, Body = 1, Restraint = 2 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    static readonly CapabilitySet<FastenerTrait> HeadedThread = CapabilitySet<FastenerTrait>.Of(FastenerTrait.Threaded, FastenerTrait.Headed);
    static readonly CapabilitySet<FastenerTrait> BareThread   = CapabilitySet<FastenerTrait>.Of(FastenerTrait.Threaded);
    static readonly CapabilitySet<FastenerTrait> HeadedShank  = CapabilitySet<FastenerTrait>.Of(FastenerTrait.Headed);

    public static readonly FastenerKind Bolt    = new("bolt",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "BOLT",        detailToken: "BOLT",    traits: HeadedThread);
    public static readonly FastenerKind Nut     = new("nut",     ifcEntity: "IfcDiscreteAccessory",  ifcPredefinedType: "USERDEFINED", detailToken: "NUT",     traits: BareThread);
    public static readonly FastenerKind Nail    = new("nail",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "NAIL",        detailToken: "NAIL",    traits: HeadedShank);
    public static readonly FastenerKind Screw   = new("screw",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "SCREW",       detailToken: "SCREW",   traits: HeadedThread);
    public static readonly FastenerKind Anchor  = new("anchor",  ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "ANCHORBOLT",  detailToken: "ANCHOR",  traits: HeadedThread);
    public static readonly FastenerKind Dowel   = new("dowel",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "DOWEL",       detailToken: "DOWEL",   traits: CapabilitySet<FastenerTrait>.None);
    public static readonly FastenerKind Rivet   = new("rivet",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "RIVET",       detailToken: "RIVET",   traits: HeadedShank);
    public static readonly FastenerKind Coupler = new("coupler", ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "COUPLER",     detailToken: "COUPLER", traits: BareThread);
    public static readonly FastenerKind Kerf    = new("kerf",    ifcEntity: "IfcDiscreteAccessory",  ifcPredefinedType: "USERDEFINED", detailToken: "KERF-ANCHOR",   traits: CapabilitySet<FastenerTrait>.None, role: AnchorRole.Body);
    public static readonly FastenerKind Pin     = new("pin",     ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "DOWEL",       detailToken: "RESTRAINT-PIN", traits: CapabilitySet<FastenerTrait>.None, role: AnchorRole.Restraint);
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public string DetailToken { get; }
    public CapabilitySet<FastenerTrait> Traits { get; }
    public AnchorRole Role { get; }
    public IfcBinding Ifc => IfcBinding.Of(IfcEntity, IfcPredefinedType);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSeries {
    public static readonly ThreadSeries MetricCoarse  = new("metric-coarse", metric: true,  stressAreaCoefficient: 0.9382);
    public static readonly ThreadSeries MetricFine    = new("metric-fine",   metric: true,  stressAreaCoefficient: 0.9382);
    public static readonly ThreadSeries UnifiedCoarse = new("unc",           metric: false, stressAreaCoefficient: 0.9743);
    public static readonly ThreadSeries UnifiedFine   = new("unf",           metric: false, stressAreaCoefficient: 0.9743);
    public bool Metric { get; }
    public double StressAreaCoefficient { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltCategory {
    public static readonly BoltCategory A = new("A", traits: CapabilitySet<JointTrait>.Of(JointTrait.Shear));
    public static readonly BoltCategory B = new("B", traits: CapabilitySet<JointTrait>.Of(JointTrait.Shear, JointTrait.Preloaded));
    public static readonly BoltCategory C = new("C", traits: CapabilitySet<JointTrait>.Of(JointTrait.Shear, JointTrait.Preloaded));
    public static readonly BoltCategory D = new("D", traits: CapabilitySet<JointTrait>.None);
    public static readonly BoltCategory E = new("E", traits: CapabilitySet<JointTrait>.Of(JointTrait.Preloaded));
    public CapabilitySet<JointTrait> Traits { get; }
    public bool Preloaded => Traits.Admits(JointTrait.Preloaded);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FayingSurface {
    public static readonly FayingSurface None   = new("none",    slipFactor: 0.00);
    public static readonly FayingSurface ClassA = new("class-a", slipFactor: 0.50);
    public static readonly FayingSurface ClassB = new("class-b", slipFactor: 0.40);
    public static readonly FayingSurface ClassC = new("class-c", slipFactor: 0.30);
    public static readonly FayingSurface ClassD = new("class-d", slipFactor: 0.20);
    public double SlipFactor { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeadForm {
    public static readonly HeadForm Hexagon     = new("hexagon",     tensionFactor: 0.90, thicknessDeductionRatio: 0.00);
    public static readonly HeadForm Countersunk = new("countersunk", tensionFactor: 0.63, thicknessDeductionRatio: 0.25);
    public double TensionFactor { get; }
    public double ThicknessDeductionRatio { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShearPlane {
    const double ShankAlphaV = 0.60;
    public static readonly ShearPlane Threaded = new("threaded", static thread => thread.StressAreaMm2,  static arm => arm.EurocodeAlphaV);
    public static readonly ShearPlane Shank    = new("shank",    static thread => thread.NominalAreaMm2, static arm => arm.EurocodeAlphaV.Map(static _ => ShankAlphaV));
    [UseDelegateFromConstructor] public partial double ResistanceAreaMm2(ThreadRow thread);
    [UseDelegateFromConstructor] public partial Option<double> ShearFactor(GradeProperties.Fastener arm);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct HexHardware(
    double HeadHeightMm, Option<double> BearingDiameterMm, Option<double> FilletDiameterMm,
    double NutHeightMm, double WasherInnerMm, double WasherOuterMm, double WasherThicknessMm);

public readonly record struct ThreadRow(
    string Key, ThreadSeries Series, double MajorMm, double PitchMm, double AcrossFlatsMm,
    Option<HexHardware> Hardware = default, Option<string> Tag = default) {

    public const double InchToMm = 25.4;
    public const double FlankAngleDeg = 60.0;

    public string Designation => Tag.IfNone(Key);
    public double FundamentalHeightMm => PitchMm / (2.0 * Math.Tan(FlankAngleDeg * Math.PI / 360.0));
    public double MinorMm => MajorMm - 1.25 * FundamentalHeightMm;
    public double PitchDiameterMm => MajorMm - 0.75 * FundamentalHeightMm;
    public double RootMinorMm => MajorMm - 17.0 / 12.0 * FundamentalHeightMm;
    public double AcrossCornersMm => AcrossFlatsMm * 2.0 / Math.Sqrt(3.0);
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorMm * MajorMm;
    public double StressAreaMm2 => Math.PI / 4.0 * Math.Pow(MajorMm - Series.StressAreaCoefficient * PitchMm, 2.0);
    public double RunoutMm => 2.5 * PitchMm;
    public double PunchingDiameterMm => 0.5 * (AcrossFlatsMm + AcrossCornersMm);
}

public readonly record struct GradeStep(double AboveMm, double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

public readonly record struct SizeBand(double MinMm, double MaxMm) {
    public bool Covers(double diameterMm) => diameterMm >= MinMm && diameterMm <= MaxMm;
}

public readonly record struct FastenerBand(Option<double> ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

public partial record GradeProperties {
    public sealed partial record Fastener {
        public FastenerBand At(ThreadRow thread) =>
            Step.Filter(step => thread.MajorMm > step.AboveMm)
                .Map(static step => new FastenerBand(Some(step.ProofStressMpa), step.TensileStrengthMpa, step.MinimumYieldMpa))
                .IfNone(new FastenerBand(ProofStressMpa, TensileStrengthMpa, MinimumYieldMpa));

        public bool Covers(ThreadRow thread) => Sizes.Covers(thread.MajorMm);
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Fastener> FastenerArm => Columns is GradeProperties.Fastener arm ? Some(arm) : None;
    public bool Admits(ThreadRow thread) =>
        FastenerArm.Exists(arm => (Authority == ComponentAuthority.En) == thread.Series.Metric && arm.Covers(thread));
    public Option<FastenerBand> At(ThreadRow thread) => FastenerArm.Map(arm => arm.At(thread));
}

// --- [TABLES] --------------------------------------------------------------------------
public static class Threads {
    static Option<HexHardware> Iso(double headHeight, double bearing, double fillet, double nutHeight, double washerInner, double washerOuter, double washerThickness) =>
        Some(new HexHardware(headHeight, Some(bearing), Some(fillet), nutHeight, washerInner, washerOuter, washerThickness));

    static Option<HexHardware> Asme(double headHeightIn, double nutHeightIn, double washerInnerIn, double washerOuterIn, double washerThicknessIn) =>
        Some(new HexHardware(headHeightIn * ThreadRow.InchToMm, None, None, nutHeightIn * ThreadRow.InchToMm,
            washerInnerIn * ThreadRow.InchToMm, washerOuterIn * ThreadRow.InchToMm, washerThicknessIn * ThreadRow.InchToMm));

    static ThreadRow Unc(string key, string tag, double inches, double threadsPerInch, double acrossFlatsIn, Option<HexHardware> hardware) =>
        new(key, ThreadSeries.UnifiedCoarse, inches * ThreadRow.InchToMm, ThreadRow.InchToMm / threadsPerInch, acrossFlatsIn * ThreadRow.InchToMm, hardware, tag);

    public static readonly ThreadRow M6     = new("m6",  ThreadSeries.MetricCoarse,  6.0, 1.00, 10.0, Iso(4.0,  8.74,  6.8,  5.2,  6.4,  12.0, 1.6));
    public static readonly ThreadRow M8     = new("m8",  ThreadSeries.MetricCoarse,  8.0, 1.25, 13.0, Iso(5.3,  11.47, 9.2,  6.8,  8.4,  16.0, 1.6));
    public static readonly ThreadRow M10    = new("m10", ThreadSeries.MetricCoarse, 10.0, 1.50, 16.0, Iso(6.4,  14.47, 11.2, 8.4,  10.5, 20.0, 2.0));
    public static readonly ThreadRow M12    = new("m12", ThreadSeries.MetricCoarse, 12.0, 1.75, 18.0, Iso(7.5,  16.47, 13.7, 10.8, 13.0, 24.0, 2.5));
    public static readonly ThreadRow M16    = new("m16", ThreadSeries.MetricCoarse, 16.0, 2.00, 24.0, Iso(10.0, 22.00, 17.7, 14.8, 17.0, 30.0, 3.0));
    public static readonly ThreadRow M20    = new("m20", ThreadSeries.MetricCoarse, 20.0, 2.50, 30.0, Iso(12.5, 27.70, 22.4, 18.0, 21.0, 37.0, 3.0));
    public static readonly ThreadRow M24    = new("m24", ThreadSeries.MetricCoarse, 24.0, 3.00, 36.0, Iso(15.0, 33.25, 26.4, 21.5, 25.0, 44.0, 4.0));
    public static readonly ThreadRow M30    = new("m30", ThreadSeries.MetricCoarse, 30.0, 3.50, 46.0, Iso(18.7, 42.75, 33.4, 25.6, 31.0, 56.0, 4.0));
    public static readonly ThreadRow M36    = new("m36", ThreadSeries.MetricCoarse, 36.0, 4.00, 55.0, Iso(22.5, 51.11, 39.4, 31.0, 37.0, 66.0, 5.0));
    public static readonly ThreadRow In0250 = Unc("1/4",   "0250", 0.250,  20.0, 0.4375, Asme(0.1719, 0.2188, 0.281, 0.625, 0.065));
    public static readonly ThreadRow In0375 = Unc("3/8",   "0375", 0.375,  16.0, 0.5625, Asme(0.2500, 0.3281, 0.406, 0.812, 0.065));
    public static readonly ThreadRow In0500 = Unc("1/2",   "0500", 0.500,  13.0, 0.7500, Asme(0.3438, 0.4375, 0.531, 1.062, 0.095));
    public static readonly ThreadRow In0625 = Unc("5/8",   "0625", 0.625,  11.0, 0.9375, Asme(0.4219, 0.5469, 0.656, 1.312, 0.095));
    public static readonly ThreadRow In0750 = Unc("3/4",   "0750", 0.750,  10.0, 1.1250, Asme(0.5000, 0.6406, 0.812, 1.469, 0.134));
    public static readonly ThreadRow In0875 = Unc("7/8",   "0875", 0.875,   9.0, 1.3125, Asme(0.5781, 0.7500, 0.938, 1.750, 0.134));
    public static readonly ThreadRow In1000 = Unc("1",     "1000", 1.000,   8.0, 1.5000, Asme(0.6719, 0.8594, 1.062, 2.000, 0.134));
    public static readonly ThreadRow In1500 = Unc("1-1/2", "1500", 1.500,   6.0, 2.2500, Asme(1.0000, 1.2813, 1.625, 3.000, 0.165));
    public static readonly ImmutableArray<ThreadRow> Rows = [M6, M8, M10, M12, M16, M20, M24, M30, M36, In0250, In0375, In0500, In0625, In0750, In0875, In1000, In1500];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fastening {
    public static Fin<double> JointFactor(DesignBasis basis) =>
        basis.Format == SafetyFormat.LimitState
            ? Fin.Succ(basis.GammaM2)
            : new ComponentFault.BasisUnsupported(basis, ComponentFamily.Fastener);

    public static Fin<double> ShearResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, ShearPlane plane, DesignBasis basis) =>
        from gamma in JointFactor(basis)
        from alphaV in plane.ShearFactor(arm).ToFin(new ComponentFault.GradeBandMissing(ComponentFamily.Fastener, typeof(GradeProperties.Fastener)))
        select alphaV * arm.SpecifiedUltimateMpa * plane.ResistanceAreaMm2(thread) / gamma * 1e-3;

    public static Fin<double> TensionResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, HeadForm head, DesignBasis basis) =>
        from gamma in JointFactor(basis)
        from tabulated in arm.EurocodeAlphaV.ToFin(new ComponentFault.GradeBandMissing(ComponentFamily.Fastener, typeof(GradeProperties.Fastener)))
        select head.TensionFactor * arm.SpecifiedUltimateMpa * thread.StressAreaMm2 / gamma * 1e-3;

    public static Fin<double> PunchingResistanceKn(ThreadRow thread, double plyThicknessMm, double plyUltimateMpa, DesignBasis basis) =>
        JointFactor(basis).Map(gamma => 0.6 * Math.PI * thread.PunchingDiameterMm * plyThicknessMm * plyUltimateMpa / gamma * 1e-3);

    public readonly record struct ReferenceLengthBand(double LengthCeilingMm, double AdditionMm);
    static readonly ImmutableArray<ReferenceLengthBand> ReferenceLengths = [new(125.0, 6.0), new(200.0, 12.0), new(double.PositiveInfinity, 25.0)];

    public static double ThreadLengthMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        !kind.Traits.Admits(FastenerTrait.Threaded) ? 0.0
        : !kind.Traits.Admits(FastenerTrait.Headed) ? lengthMm
        : Math.Min(lengthMm, 2.0 * thread.MajorMm + ReferenceAdditionMm(lengthMm));

    static double ReferenceAdditionMm(double lengthMm) =>
        toSeq(ReferenceLengths).Find(band => lengthMm <= band.LengthCeilingMm)
            .Map(static band => band.AdditionMm)
            .IfNone(ReferenceLengths[^1].AdditionMm);

    public static double UnthreadedShankMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        lengthMm - ThreadLengthMm(kind, thread, lengthMm);

    public static Fin<double> TimberDowelShearKn(
        double diameterMm, double fastenerUltimateMpa, double loadToGrainDeg,
        GradeProperties.Timber side1, double t1Mm, GradeProperties.Timber side2, double t2Mm,
        ServiceClass service, LoadDuration duration) =>
        from admitted in AdmissionSlots.Accumulate(Seq(
            Positive(diameterMm, "d"),
            Positive(fastenerUltimateMpa, "fu"),
            Positive(t1Mm, "t1"),
            Positive(t2Mm, "t2"),
            AdmissionSlots.Gate(double.IsFinite(loadToGrainDeg),
                new KernelFault.OutOfRange(nameof(loadToGrainDeg), loadToGrainDeg, "finite")))).ToFin()
        let d = diameterMm
        let alpha = loadToGrainDeg * Math.PI / 180.0
        let sin2 = Math.Pow(Math.Sin(alpha), 2.0)
        let cos2 = Math.Pow(Math.Cos(alpha), 2.0)
        let fh1 = 0.082 * (1.0 - 0.01 * d) * side1.DensityK / ((side1.K90Base + 0.015 * d) * sin2 + cos2)
        let fh2 = 0.082 * (1.0 - 0.01 * d) * side2.DensityK / ((side2.K90Base + 0.015 * d) * sin2 + cos2)
        let beta = fh2 / fh1
        let my = 0.3 * fastenerUltimateMpa * Math.Pow(d, 2.6)
        let ratio = t2Mm / t1Mm
        let modeC = fh1 * t1Mm * d / (1.0 + beta)
            * (Math.Sqrt(beta + 2.0 * beta * beta * (1.0 + ratio + ratio * ratio) + beta * beta * beta * ratio * ratio) - beta * (1.0 + ratio))
        let modeD = 1.05 * fh1 * t1Mm * d / (2.0 + beta)
            * (Math.Sqrt(2.0 * beta * (1.0 + beta) + 4.0 * beta * (2.0 + beta) * my / (fh1 * d * t1Mm * t1Mm)) - beta)
        let modeE = 1.05 * fh1 * t2Mm * d / (1.0 + 2.0 * beta)
            * (Math.Sqrt(2.0 * beta * beta * (1.0 + beta) + 4.0 * beta * (1.0 + 2.0 * beta) * my / (fh1 * d * t2Mm * t2Mm)) - beta)
        let modeF = 1.15 * Math.Sqrt(2.0 * beta / (1.0 + beta)) * Math.Sqrt(2.0 * my * fh1 * d)
        let fvk = Seq(fh1 * t1Mm * d, fh2 * t2Mm * d, modeC, modeD, modeE, modeF).Min(double.PositiveInfinity)
        select duration.KmodFor(service) * fvk / TimberPartialFactor.Connection * 1e-3;

    static Validation<Error, Unit> Positive(double value, string label) =>
        AdmissionSlots.Gate(
            double.IsFinite(value) && value > 0.0,
            new KernelFault.OutOfRange(label, value, "finite and positive"));
}

public static class FastenerDetail {
    public static Fin<PropertyBag> Of(FastenerKind kind, StockFacts facts, Option<ThreadRow> thread, EvidenceGrade source) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, facts.DiameterMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.NominalLength, Dimension.LengthDim, facts.LengthMm * 1e-3)
        from form in thread.TraverseM(t => FormRow(kind, t, facts.LengthMm)).As()
        select ComponentDetail.RealizationRows([
            ComponentDetail.Token(DetailSchema.FastenerType, kind.DetailToken),
            .. kind.Role == AnchorRole.None ? Seq<(PropertyName, PropertyValue)>() : Seq(ComponentDetail.Token(DetailSchema.AnchorType, kind.DetailToken)),
            ComponentDetail.Sourced(source),
            diameter,
            length,
            .. form.ToSeq(),
        ]);

    static Fin<(PropertyName, PropertyValue)> FormRow(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        from pitch in Si(thread.PitchMm)
        from minor in Si(thread.MinorMm)
        from pitchDiameter in Si(thread.PitchDiameterMm)
        from root in Si(thread.RootMinorMm)
        from corners in Si(thread.AcrossCornersMm)
        from runout in Si(thread.RunoutMm)
        from threaded in Si(Fastening.ThreadLengthMm(kind, thread, lengthMm))
        from shank in Si(Fastening.UnthreadedShankMm(kind, thread, lengthMm))
        select (DetailSchema.FastenerForm, (PropertyValue)new PropertyValue.Complex("fastener-form", Map(
            (DetailSchema.FlankAngle, (PropertyValue)new PropertyValue.Text($"{ThreadRow.FlankAngleDeg:R}")),
            (DetailSchema.Pitch, pitch),
            (DetailSchema.MinorDiameter, minor),
            (DetailSchema.PitchDiameter, pitchDiameter),
            (DetailSchema.RootDiameter, root),
            (DetailSchema.AcrossCorners, corners),
            (DetailSchema.ThreadRunout, runout),
            (DetailSchema.ThreadLength, threaded),
            (DetailSchema.UnthreadedShank, shank))
            + thread.Hardware.Map(HexEnvelope).IfNone(Map<PropertyName, PropertyValue>())));

    static Map<PropertyName, PropertyValue> HexEnvelope(HexHardware hex) => Map(
        (DetailSchema.HeadHeight, (PropertyValue)new PropertyValue.Text($"{hex.HeadHeightMm:R}")),
        (DetailSchema.NutHeight, new PropertyValue.Text($"{hex.NutHeightMm:R}")),
        (DetailSchema.WasherInner, new PropertyValue.Text($"{hex.WasherInnerMm:R}")),
        (DetailSchema.WasherOuter, new PropertyValue.Text($"{hex.WasherOuterMm:R}")),
        (DetailSchema.WasherThickness, new PropertyValue.Text($"{hex.WasherThicknessMm:R}")))
        + Declared(DetailSchema.BearingDiameter, hex.BearingDiameterMm)
        + Declared(DetailSchema.FilletDiameter, hex.FilletDiameterMm);

    static Map<PropertyName, PropertyValue> Declared(PropertyName name, Option<double> mm) =>
        mm.Match(
            Some: value => Map((name, (PropertyValue)new PropertyValue.Text($"{value:R}"))),
            None: static () => Map<PropertyName, PropertyValue>());

    static Fin<PropertyValue> Si(double mm) =>
        MeasureValue.OfSi(Dimension.LengthDim, mm * 1e-3).Map(static value => (PropertyValue)new PropertyValue.Measure(value));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public readonly record struct StockFacts(
    FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, Option<double> UltimateMpa,
    ComponentAuthority Authority, MaterialId Substance, MaterialId Appearance) {
    public ComponentStandard Standard => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);
}

[Union]
public abstract partial record StockRow {
    private StockRow() { }
    public sealed record Threaded(FastenerKind Kind, ThreadRow Thread, MaterialGrade Grade, double LengthMm) : StockRow;
    public sealed record Plain(
        FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, double UltimateMpaColumn,
        ComponentAuthority Authority, MaterialId Substance, MaterialId Appearance) : StockRow;

    public StockFacts Facts => Switch(
        threaded: static row => new StockFacts(
            row.Kind, $"{row.Thread.Designation}-{row.Grade.Key.Replace(".", string.Empty)}", row.Thread.MajorMm, row.LengthMm,
            row.Grade.At(row.Thread).Map(static band => band.TensileStrengthMpa),
            row.Grade.Authority, row.Grade.Substance, row.Grade.Appearance.IfNone(row.Grade.Substance)),
        plain: static row => new StockFacts(
            row.Kind, row.Designation, row.DiameterMm, row.LengthMm, Some(row.UltimateMpaColumn),
            row.Authority, row.Substance, row.Appearance));

    public Option<ThreadRow> Thread => Switch(threaded: static row => Some(row.Thread), plain: static _ => Option<ThreadRow>.None);
    public Option<MaterialGrade> Grade => Switch(threaded: static row => Some(row.Grade), plain: static _ => Option<MaterialGrade>.None);
    public Option<GradeProperties.Fastener> Arm => Grade.Bind(static grade => grade.FastenerArm);

    public Validation<Error, Unit> Coherence() => Switch(
        threaded: row => AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(row.Kind.Traits.Admits(FastenerTrait.Threaded), new KernelFault.InvalidValue(nameof(row.Kind), "a threaded stock kind")),
            AdmissionSlots.Gate(row.Grade.Admits(row.Thread), new KernelFault.InvalidValue(nameof(row.Thread), "a thread admitted by its grade")),
            AdmissionSlots.Gate(row.Grade.FastenerArm.IsSome, new ComponentFault.GradeBodyMissing(row.Grade, ComponentFamily.Fastener)),
            AdmissionSlots.Gate(double.IsFinite(row.LengthMm) && row.LengthMm > 0.0, new KernelFault.OutOfRange(nameof(row.LengthMm), row.LengthMm, "finite and positive")))),
        plain: row => AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(!row.Kind.Traits.Admits(FastenerTrait.Threaded), new KernelFault.InvalidValue(nameof(row.Kind), "a plain stock kind")),
            AdmissionSlots.Gate(double.IsFinite(row.DiameterMm) && row.DiameterMm > 0.0, new KernelFault.OutOfRange(nameof(row.DiameterMm), row.DiameterMm, "finite and positive")),
            AdmissionSlots.Gate(double.IsFinite(row.LengthMm) && row.LengthMm > 0.0, new KernelFault.OutOfRange(nameof(row.LengthMm), row.LengthMm, "finite and positive")))));
}

[Union]
public abstract partial record FastenerPlacement {
    private FastenerPlacement() { }
    public sealed record Bearing(BoltCategory Category, HeadForm Head, ShearPlane Plane, int GripPlies, int ShearPlanes, Option<HexHardware> Washer, BearingDesign Ply) : FastenerPlacement;
    public sealed record SlipCritical(BoltCategory Category, FayingSurface Faying, HeadForm Head, int GripPlies, int ShearPlanes, Option<HexHardware> Washer, FastenerInstallation Install) : FastenerPlacement;
    public sealed record TimberDowel(MaterialGrade Side1, double Thickness1Mm, MaterialGrade Side2, double Thickness2Mm, double LoadToGrainDeg, int ShearPlanes, ServiceClass Service, LoadDuration Duration) : FastenerPlacement;
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class FastenerSeed {
    public static readonly Seq<StockRow> Roster = Seq<StockRow>(
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M12,    MaterialGrade.G88,  60.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M16,    MaterialGrade.G88,  80.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M16,    MaterialGrade.G109, 80.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M20,    MaterialGrade.G88,  90.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M20,    MaterialGrade.G109, 90.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M24,    MaterialGrade.G109, 110.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M30,    MaterialGrade.G129, 140.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0375, MaterialGrade.Gr5,  63.5),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0500, MaterialGrade.Gr5,  76.2),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0750, MaterialGrade.Gr8,  101.6),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0875, MaterialGrade.A325, 114.3),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0875, MaterialGrade.A490, 114.3),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0625, MaterialGrade.F1852, 88.9),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0750, MaterialGrade.F2280, 101.6),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M16,    MaterialGrade.G88,  14.8),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M20,    MaterialGrade.G109, 18.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.M8,     MaterialGrade.G88,  40.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.M6,     MaterialGrade.G98,  30.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.In0250, MaterialGrade.Gr2,  31.8),
        new StockRow.Threaded(FastenerKind.Coupler, Threads.M20,    MaterialGrade.G88,  60.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M16,    MaterialGrade.G88,  200.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M20,    MaterialGrade.G88,  250.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In0750, MaterialGrade.A325, 304.8),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In0750, MaterialGrade.F155436,  304.8),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In1000, MaterialGrade.F155455,  457.2),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In1500, MaterialGrade.F1554105, 609.6),
        new StockRow.Plain(FastenerKind.Nail,  "8d-common",  3.33, 63.5,  690.0, ComponentAuthority.Astm, MaterialId.Create("steel.fastener-nail"),  MaterialId.Create("metal.iron")),
        new StockRow.Plain(FastenerKind.Nail,  "10d-common", 3.76, 76.2,  690.0, ComponentAuthority.Astm, MaterialId.Create("steel.fastener-nail"),  MaterialId.Create("metal.iron")),
        new StockRow.Plain(FastenerKind.Dowel, "dowel-20",  20.00, 100.0, 400.0, ComponentAuthority.En,   MaterialId.Create("steel.fastener-dowel"), MaterialId.Create("metal.steel")),
        new StockRow.Plain(FastenerKind.Rivet, "rivet-0500", 12.70, 38.1, 415.0, ComponentAuthority.Astm, MaterialId.Create("steel.fastener-rivet"), MaterialId.Create("metal.iron")));

    static readonly EvidenceGrade Stock = EvidenceGrade.Catalogue;

    public static readonly SeedLaw<StockRow> Law = SeedLaw<StockRow>.Of(
        family: ComponentFamily.Fastener,
        designation: static row => $"fastener.{row.Facts.Kind.Key}-{row.Facts.Designation}",
        coherence: static (row, key) => row.Coherence(),
        profile: static (row, key) => SectionProfile.Circle.Of(row.Facts.DiameterMm),
        substance: static row => row.Facts.Substance,
        source: static _ => Stock,
        standard: static row => row.Facts.Standard,
        detail: Some<Func<StockRow, SectionProfile, Fin<PropertyBag>>>(
            static (row, _, _) => FastenerDetail.Of(row.Facts.Kind, row.Facts, row.Thread, Stock)),
        appearance: static row => row.Facts.Appearance,
        ifc: static row => row.Facts.Kind.Ifc);

    static readonly FrozenDictionary<ComponentId, StockRow> Table =
        Roster.ToFrozenDictionary(static row => ComponentId.Create($"fastener.{row.Facts.Kind.Key}-{row.Facts.Designation}"), static row => row);

    public static Fin<StockRow> Resolve(Component component) =>
        Table.TryGetValue(component.Designation, out StockRow row)
            ? Fin.Succ(row)
            : new ComponentFault.ComponentMissing(ProfileRef.Of(component.Designation.Value));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        from row in Resolve(component)
        from connection in placement.Fastener.ToFin(
            new ComponentFault.ConnectionMissing(key, component.Designation))
        from lift in connection.Switch(
            bearing: state =>
                from assembly in Assembly(row, state.Category, FayingSurface.None, state.Head, state.GripPlies, state.ShearPlanes, state.Washer, key)
                select (CapacityLift)new CapacityLift.Bolt(component.Designation, assembly, state.Ply, state.Plane),
            slipCritical: state =>
                from assembly in Assembly(row, state.Category, state.Faying, state.Head, state.GripPlies, state.ShearPlanes, state.Washer, key)
                select (CapacityLift)new CapacityLift.SlipCritical(component.Designation, assembly, state.Install),
            timberDowel: state =>
                from ultimate in row.Facts.UltimateMpa.ToFin(
                    new ComponentFault.GradeBandMissing(key, ComponentFamily.Fastener, typeof(StockFacts)))
                from side1 in TimberArm(state.Side1, key)
                from side2 in TimberArm(state.Side2, key)
                from perPlane in Fastening.TimberDowelShearKn(
                    row.Facts.DiameterMm, ultimate, state.LoadToGrainDeg,
                    side1, state.Thickness1Mm, side2, state.Thickness2Mm, state.Service, state.Duration, key)
                select (CapacityLift)new CapacityLift.TimberDowel(component.Designation, perPlane, state.ShearPlanes))
        from capacity in SectionCapacity.Lift(lift, key)
        select capacity;

    static Fin<FastenerAssembly> Assembly(StockRow row, BoltCategory category, FayingSurface faying, HeadForm head, int gripPlies, int shearPlanes, Option<HexHardware> washer) =>
        from thread in row.Thread.ToFin(new KernelFault.InvalidValue(nameof(row.Thread), "a threaded stock row"))
        from grade in row.Grade.ToFin(new KernelFault.InvalidValue(nameof(row.Grade), "a threaded stock grade"))
        from assembly in FastenerAssembly.Of(thread, grade, category, faying, head, gripPlies, shearPlanes, washer)
        select assembly;

    static Fin<GradeProperties.Timber> TimberArm(MaterialGrade grade) =>
        grade.Columns is GradeProperties.Timber arm
            ? Fin.Succ(arm)
            : new ComponentFault.GradeBodyMissing(grade, ComponentFamily.Timber);
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` owns the installed bolt state and its own resistance projections; `BearingDesign` owns the ply the shank bears against and derives its EN 1993-1-8 Table 3.4 factors from the bolt-group geometry; `BoltPosition` and `HoleShape` own the published position and hole-form policy; `FastenerInstallation` admits the shared `(ks, γM3, km)` slip-and-torque policy.
- Cases: one assembly shape for every modality — a non-preloaded (A/D) assembly resolves `FayingSurface.None` and returns `None` for preload, slip, and tightening torque; a preloaded (B/C/E) assembly requires a named slip class and returns `Some` design values — never a numeric absence sentinel and never a `PreloadedBolt`/`BearingBolt` pair. `BoltPosition` closes the four-cell product of the two independent Table 3.4 discriminants: end-versus-inner along the load path selects α_d, edge-versus-inner across it selects k1.
- Entry: `FastenerAssembly.Of(thread, grade, category, faying, head, gripPlies, shearPlanes, washer)` ACCUMULATES its four independent admissions — a system- or size-mismatched thread/grade pair, a missing fastener arm, a preloaded category over a non-preloadable grade, and a preloaded category with `FayingSurface.None` — then admits the two discrete counts, and carries the PROVED arm onto the assembly so no projection re-unwraps it. `BearingDesign.Of` admits the ply and its bolt-group distances once.
- Growth: a new connection modality is a `BoltCategory`/`FayingSurface` row the assembly reads; a new hole form one `HoleShape` row; a new bolt-group position one `BoltPosition` row; the multi-bolt group `ΣFs,Rd`, the long-joint `β`, and the `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` interaction are `Rasm.Compute` consumers over these single-bolt design values.
- Boundary: `Count` admits the discrete grip and shear-plane columns. `BearingDesign` takes the DISTANCES the code's own formulas consume and derives `k1` and `α_b` from them, so a caller cannot hand the resistance one opaque scalar in which a transposed edge and end distance is invisible; the hole-shape reduction and the countersink thickness deduction are rows the same derivation reads. Every resistance takes the placement's `DesignBasis` and reads γM2 through `Fastening.JointFactor` — this section spells no partial factor either. The preload is bounded by the grade's own yield load, because a pretension above the elastic limit is a tightening method the assembly cannot represent. A washer's ABSENCE is the absence of a washer, so its hardness, outer diameter, and thickness are all `None` together rather than a bool guarding three separate reads.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltPosition {
    public static readonly BoltPosition EndEdge    = new("end-edge",    static (l, d0) => l / (3.0 * d0),        static (t, d0) => 2.8 * t / d0 - 1.7);
    public static readonly BoltPosition EndInner   = new("end-inner",   static (l, d0) => l / (3.0 * d0),        static (t, d0) => 1.4 * t / d0 - 1.7);
    public static readonly BoltPosition InnerEdge  = new("inner-edge",  static (l, d0) => l / (3.0 * d0) - 0.25, static (t, d0) => 2.8 * t / d0 - 1.7);
    public static readonly BoltPosition InnerInner = new("inner-inner", static (l, d0) => l / (3.0 * d0) - 0.25, static (t, d0) => 1.4 * t / d0 - 1.7);
    [UseDelegateFromConstructor] public partial double AlphaD(double loadwiseMm, double holeMm);
    [UseDelegateFromConstructor] public partial double K1Raw(double transverseMm, double holeMm);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HoleShape {
    public static readonly HoleShape Normal               = new("normal",                bearingFactor: 1.0);
    public static readonly HoleShape Oversize             = new("oversize",              bearingFactor: 0.8);
    public static readonly HoleShape SlottedPerpendicular = new("slotted-perpendicular", bearingFactor: 0.6);
    public double BearingFactor { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct BearingDesign {
    public double PlyThicknessMm { get; }
    public double PlyUltimateMpa { get; }
    public double LoadwiseDistanceMm { get; }
    public double TransverseDistanceMm { get; }
    public double HoleDiameterMm { get; }
    public HoleShape Hole { get; }
    public BoltPosition Position { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double plyThicknessMm, ref double plyUltimateMpa,
        ref double loadwiseDistanceMm, ref double transverseDistanceMm, ref double holeDiameterMm,
        ref HoleShape hole, ref BoltPosition position) =>
        validationError = hole is not null && position is not null
            && double.IsFinite(plyThicknessMm) && plyThicknessMm > 0.0
            && double.IsFinite(plyUltimateMpa) && plyUltimateMpa > 0.0
            && double.IsFinite(loadwiseDistanceMm) && loadwiseDistanceMm > 0.0
            && double.IsFinite(transverseDistanceMm) && transverseDistanceMm > 0.0
            && double.IsFinite(holeDiameterMm) && holeDiameterMm > 0.0
            ? null
            : new ValidationError($"Bearing design requires positive finite thickness, strength, distances, and hole diameter; received {plyThicknessMm:R}, {plyUltimateMpa:R}, {loadwiseDistanceMm:R}, {transverseDistanceMm:R}, {holeDiameterMm:R}.");

    public static Fin<BearingDesign> Of(
        double plyThicknessMm, double plyUltimateMpa, double loadwiseDistanceMm, double transverseDistanceMm,
        double holeDiameterMm, HoleShape hole, BoltPosition position) =>
        FactoryBridge.Accept<BearingDesign>(
            Validate(plyThicknessMm, plyUltimateMpa, loadwiseDistanceMm, transverseDistanceMm, holeDiameterMm, hole, position, out BearingDesign design), design);

    public double K1 => Math.Min(Position.K1Raw(TransverseDistanceMm, HoleDiameterMm), 2.5);
    public double AlphaB(GradeProperties.Fastener arm) =>
        Math.Min(Math.Min(Position.AlphaD(LoadwiseDistanceMm, HoleDiameterMm), arm.SpecifiedUltimateMpa / PlyUltimateMpa), 1.0);

    public Fin<double> ResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, HeadForm head, DesignBasis basis) =>
        Fastening.JointFactor(basis).Map(gamma =>
            Hole.BearingFactor * K1 * AlphaB(arm) * PlyUltimateMpa * thread.MajorMm
                * (PlyThicknessMm - head.ThicknessDeductionRatio * thread.MajorMm) / gamma * 1e-3);
}

[ComplexValueObject]
public readonly partial struct FastenerInstallation {
    public double Ks { get; }
    public double GammaM3 { get; }
    public double Km { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double ks, ref double gammaM3, ref double km) =>
        validationError = double.IsFinite(ks) && ks > 0.0 && double.IsFinite(gammaM3) && gammaM3 > 0.0 && double.IsFinite(km) && km > 0.0
            ? null
            : new ValidationError($"Fastener installation factors must be finite and positive; received {ks:R}, {gammaM3:R}, {km:R}.");

    public static Fin<FastenerInstallation> Of(double ks, double gammaM3, double km) =>
        FactoryBridge.Accept<FastenerInstallation>(Validate(ks, gammaM3, km, out FastenerInstallation design), design);
}

public readonly record struct FastenerAssembly(
    ThreadRow Thread, MaterialGrade Grade, GradeProperties.Fastener Arm, BoltCategory Category,
    FayingSurface Faying, HeadForm Head, Count GripPlies, Count ShearPlanes, Option<HexHardware> Washer) {

    public static Fin<FastenerAssembly> Of(
        ThreadRow thread, MaterialGrade grade, BoltCategory category, FayingSurface faying, HeadForm head,
        int gripPlies, int shearPlanes, Option<HexHardware> washer) =>
        from proven in AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(grade.Admits(thread), new KernelFault.InvalidValue(nameof(thread), "a thread admitted by its grade")),
            AdmissionSlots.Gate(grade.FastenerArm.IsSome, new ComponentFault.GradeBodyMissing(grade, ComponentFamily.Fastener)),
            AdmissionSlots.Gate(!category.Preloaded || grade.FastenerArm.Exists(static a => a.Preloadable), new KernelFault.InvalidValue(nameof(grade), "a preloadable grade for a preloaded connection")),
            AdmissionSlots.Gate(!category.Preloaded || faying != FayingSurface.None, new KernelFault.InvalidValue(nameof(faying), "a faying class for a preloaded connection"))))
            .ToFin()
        from arm in grade.FastenerArm.ToFin(new ComponentFault.GradeBodyMissing(grade, ComponentFamily.Fastener))
        from plies in FactoryBridge.Accept<Count>(candidate: gripPlies)
        from planes in FactoryBridge.Accept<Count>(candidate: shearPlanes)
        select new FastenerAssembly(thread, grade, arm, category, category.Preloaded ? faying : FayingSurface.None, head, plies, planes, washer);

    public FastenerBand Band => Arm.At(Thread);

    public double YieldLoadKn => Band.MinimumYieldMpa * Thread.StressAreaMm2 * 1e-3;
    public Option<double> ProofLoadKn => Band.ProofStressMpa.Map(stress => stress * Thread.StressAreaMm2 * 1e-3);
    public double PreloadCeilingKn => ProofLoadKn.Map(proof => Math.Min(proof, YieldLoadKn)).IfNone(YieldLoadKn);

    public Option<double> PreloadKn =>
        Category.Preloaded
            ? Some(0.7 * Arm.SpecifiedUltimateMpa * Thread.StressAreaMm2 * 1e-3).Filter(preload => preload <= PreloadCeilingKn)
            : None;

    public Option<double> SlipResistanceKn(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Ks * ShearPlanes.Value * Faying.SlipFactor * preload / design.GammaM3);

    public Option<double> TighteningTorqueNm(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Km * (Thread.MajorMm * 1e-3) * (preload * 1e3));

    public Fin<double> ShearResistanceKn(ShearPlane plane, DesignBasis basis) =>
        Fastening.ShearResistanceKn(Thread, Arm, plane, basis).Map(perPlane => perPlane * ShearPlanes.Value);
    public Fin<double> TensionResistanceKn(DesignBasis basis) => Fastening.TensionResistanceKn(Thread, Arm, Head, basis);
    public Fin<double> BearingResistanceKn(BearingDesign ply, DesignBasis basis) => ply.ResistanceKn(Thread, Arm, Head, basis);
    public Fin<double> PunchingResistanceKn(BearingDesign ply, DesignBasis basis) =>
        Fastening.PunchingResistanceKn(Thread, ply.PlyThicknessMm, ply.PlyUltimateMpa, basis);

    public Option<double> WasherHardnessHv => Washer.Map(_ => Arm.Preloadable ? 300.0 : 200.0);
    public Option<double> WasherOuterMm => Washer.Map(static h => h.WasherOuterMm);
    public Option<double> WasherThicknessMm => Washer.Map(static h => h.WasherThicknessMm);
    public Option<double> NutHeightMm => Thread.Hardware.Map(static h => h.NutHeightMm);
}
```

## [04]-[RESEARCH]

(none)
