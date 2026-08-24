# [MATERIALS_FASTENER]

THE FASTENER SEED PAGE owns the `ComponentFamily.Fastener` roster and law, the thread-form algebra, and the EN 1993-1-8 single-fastener design values. `StockRow.Threaded` pairs a `ThreadRow` with a `MaterialGrade` fastener row; `StockRow.Plain` carries published nail, dowel, and rivet data — including its own PUBLISHED tensile strength — without a fake thread or bolt grade. Both cases project through ONE `StockFacts` read, so geometry, IFC binding, realization detail, the EC5 dowel check, and the seed law share one correspondence while case-specific admission stays total and ACCUMULATING. Every design value this page emits is a DESIGN resistance already divided by the partial factor its own `DesignBasis` row publishes, so a consumer folds demand against it directly and no arm re-divides — and no factor is spelled here.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerTrait`/`JointTrait` capability rosters, the `FastenerKind`/`ThreadSeries`/`BoltCategory`/`FayingSurface`/`HeadForm`/`ShearPlane` policy vocabularies, the `HexHardware` head-nut-washer dimension set, the `ThreadRow` ISO 68-1 form algebra with its `Threads` owner, the `GradeStep`/`SizeBand`/`FastenerBand` grade vocabulary and the `GradeProperties.Fastener` arm physics (`Admits`/`At`) beside the `MaterialGrade` fastener members, the `Fastening` EN 1993-1-8 shear/tension/punching design values with the ISO 4014 length bands and the EC5 §8.5 dowel-type algebra, the `FastenerDetail` realization bag, and the `FastenerSeed.Roster`/`Law`/`Capacity` triple.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — bolt + grip-plies + shear-planes + head + declared washer over one `(ThreadRow, MaterialGrade, GradeProperties.Fastener, BoltCategory, FayingSurface, HeadForm)` — the `BoltPosition`/`HoleShape`/`BearingDesign` EN 1993-1-8 Table 3.4 bearing geometry, the `PreloadKn` `Fp,C = 0.7·fub·As` projection under its yield ceiling, the `FastenerInstallation` admitted slip-and-torque factor set, the `SlipResistanceKn` §3.9 design value, and the ISO 7089/7090 washer-hardness selection.

## [02]-[FASTENER_FAMILY]

- Owner: `FastenerSeed` owns the `ComponentFamily.Fastener` roster, seed law, and capacity producer; `Threads` owns the thread table and `component#MATERIAL_GRADE` the grade rows; `FastenerKind` owns the complete IFC entity/token binding, the realization token, and its `CapabilitySet<FastenerTrait>` column; `BoltCategory`, `FayingSurface`, `ThreadSeries`, `ShearPlane`, and `HeadForm` own policy; `GradeProperties.Fastener` owns the size-banded grade physics; `Fastening` owns the design values; `FastenerAssembly` owns installed-bolt state; `FastenerDetail` owns the realization bag.
- Cases: kind {`bolt` · `nut` · `nail` · `screw` · `anchor` · `dowel` · `rivet` · `coupler` · `kerf` · `pin`} × stock form {threaded hardware over a `ThreadRow`/`MaterialGrade` pair · plain shank over its published designation, diameter, length, tensile strength, authority, and material pair}; the joint category is a `FastenerAssembly` decision, never a type-row column; the stone-cladding pair carries the `AnchorRole` body/restraint axis driving the seam `AnchorType` stamp.
- Entry: `ComponentSeed.Rows(context, FastenerSeed.Roster, FastenerSeed.Law)` — this page states the roster and the policy, never the fold. `FastenerSeed.Capacity` dispatches the `FastenerPlacement` the connection carries into the matching `CapacityReceipt`; `Fastening` owns the EN 1993-1-8 §3.6 resistances, the ISO 4014 length bands, and the EC5 §8.5 dowel-type check.
- Packages: Rasm.Numerics (`Dimension` aliased `Count` — the discrete grip-ply/shear-plane columns), Rasm.Domain (`Op`/`Context`/`AcceptValidated`, `ICapability`/`CapabilitySet`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `DetailSchema`, `PropertyBag`, `PropertyName`, `PropertyValue`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Circle.Of`/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`SeedLaw`/`ComponentSeed`, `MaterialGrade`+`GradeProperties`, the `capacity#SECTION_CAPACITY` `DesignBasis`/`SafetyFormat`/`CapacityPlacement`, and the sibling `TimberPartialFactor`/`ServiceClass`/`LoadDuration` the EC5 join reads), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]`, `[UseDelegateFromConstructor]`, `[Union]`, `[ComplexValueObject]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`.Apply`/`guard`/`Option`), BCL (`ImmutableArray`, `FrozenDictionary`).
- Growth: a new threaded combination is one `StockRow.Threaded`; a new plain-shank product one `StockRow.Plain`; a new kind one `FastenerKind` row with its trait set and stock case; a new thread one `Threads` entry carrying its own diameter and pitch; a new property class one `MaterialGrade` fastener row on `component#MATERIAL_GRADE`; a new connection category one `BoltCategory` row; a new head geometry one `HeadForm` row; a new bolt-group position one `BoltPosition` row; a new fastener or joint trait one roster row and its membership on the subjects that hold it.
- Boundary: every fastener uses `SectionProfile.Circle` and the seed-built realization bag. Thread semantics and grade payload exist only on `StockRow.Threaded`; `StockRow.Plain` carries its own published diameter, length, tensile strength, authority, and substance/appearance pair — so `StockFacts.UltimateMpa` is OPTIONAL, absent exactly where a threaded row's grade carries no fastener arm, a state the coherence census refuses before any row reaches a capacity read. `Fastening.TimberDowelShearKn` takes the SCALARS EC5 §8.5 consumes plus the two `GradeProperties.Timber` arms whose density and k90 intercept it reads, never a threaded currency a plain product does not carry. The stone-cladding `kerf`/`pin` kinds are CLOSED VOCABULARY without stock — no captured source prints their section dimensions to the two-source bar — so a proven product lands as one `StockRow.Plain` row.
- Boundary: this page emits EN 1993-1-8 and EN 1995-1-1 design resistances and NOTHING ELSE, and it spells NO partial factor. `Fastening.JointFactor` reads γM2 off the `capacity#SECTION_CAPACITY` `DesignBasis` row the placement declares (`en1993-1-8` is the joints row), and it REFUSES a resistance-factor basis: an AISC §J3 verdict divides no nominal by γM2, so a φ-format basis passed through publishes a resistance this page never computed. `GradeProperties.Fastener.EurocodeAlphaV` is `Some` only for the seven property classes EN 1993-1-8 Table 3.1 tabulates, so a SAE, ASTM, 9.8, or 12.9 grade RAILS out of the Eurocode resistances rather than borrowing an α_v the code never published for it. The published mechanical band, the preload, and the stock identity stay total over every grade, because those are each body's own specification data.
- Boundary: the retired `bool Metric` on the grade row DERIVES — the thread system is the authority's PRINT system, so `Admits` spells `(Authority == ComponentAuthority.En) == thread.Series.Metric` reading the owning `MaterialGrade` row's own authority column. `Admits`/`At` therefore land on `MaterialGrade` rather than on the arm: the arm carries no authority, and a member seated there takes one as an argument the call site supplies. A non-fastener grade answers `false` and `None` respectively, the arm mismatch stated at the refusal site.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

// --- [TYPES] -------------------------------------------------------------------------------
// The fastener FORM traits — the kernel capability vocabulary replacing the two bool columns the kind row carried.
// Threaded: a dowel/rivet has no thread, so its thread length resolves 0 and the body is all shank. Headed: a
// headless threaded part (nut/coupler) threads through its whole length. A third form fact is one row here plus its
// membership on the kinds that hold it, and no consumer signature moves.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerTrait : ICapability<FastenerTrait> {
    public static readonly FastenerTrait Threaded = new("threaded", rank: 0);
    public static readonly FastenerTrait Headed   = new("headed",   rank: 1);
    public int Rank { get; }
}

// The JOINT traits of an EN 1993-1-8 Table 3.2 category. Shear selects WHICH resistance triple the capacity fold
// reports as governing, so a category-D tension connection never reports a shear verdict; Preloaded gates the [03]
// slip projection and requires a preloadable grade plus a named faying class.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class JointTrait : ICapability<JointTrait> {
    public static readonly JointTrait Shear     = new("shear",     rank: 0);
    public static readonly JointTrait Preloaded = new("preloaded", rank: 1);
    public int Rank { get; }
}

// The stone-cladding anchor role: the kerf bar is the BODY anchor carrying panel gravity in the panel edge, the
// restraint pin the LATERAL-only dowel into the back face. A role-carrying kind stamps the seam DetailSchema.AnchorType
// row beside FastenerType, so a cladding bag names its anchor system.
public enum AnchorRole : byte { None = 0, Body = 1, Restraint = 2 }

// The kind axis of the seed roster: the member-type vocabulary owning the COMPLETE entity-token binding (POLICY_VALUES
// — entity selection is a row read, never reconstructed at the seed). The verified GeometryGym
// IfcMechanicalFastenerTypeEnum carries BOLT/SCREW/NAIL/ANCHORBOLT/DOWEL/RIVET/COUPLER and NO NUT member, so the nut
// ROW binds IfcDiscreteAccessory/USERDEFINED. Because USERDEFINED is the schema's own catch-all, the wire token alone
// leaves a nut indistinguishable from any other owner-labelled accessory, so DetailToken is the row's SEPARATE
// realization identity and the bag stamps THAT.
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

// The pitch family. Metric and unified threads share ONE 60° form, so the series carries only what genuinely differs:
// the SYSTEM bit a grade admits against, and the ISO 898-1 versus ASME B1.1 tensile-stress-area coefficient — the two
// standards subtract 0.9382·P and 0.9743·P from the major diameter respectively, and that single coefficient IS the
// ~3% disagreement between their printed area tables. A fine row is one Threads entry naming this series with its own
// finer pitch: both columns are read the moment such a row exists.
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

// The EN 1993-1-8 Table 3.2 joint category — the bearing-vs-preloaded axis a CONNECTION selects, never a type-row
// column. The clause citation rides the capacity#SECTION_CAPACITY SectionCapacity.Code column on the lifted verdict,
// so no inert static sits beside the rows. A non-preloadable grade in a B/C/E joint rails at FastenerAssembly.Of.
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

// The EN 1993-1-8 §3.9 / RCSC slip-factor class μ a preloaded joint relies on; None (μ = 0) is the bearing-joint row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FayingSurface {
    public static readonly FayingSurface None   = new("none",    slipFactor: 0.00);
    public static readonly FayingSurface ClassA = new("class-a", slipFactor: 0.50);   // blasted, loose rust removed
    public static readonly FayingSurface ClassB = new("class-b", slipFactor: 0.40);   // blasted + alkali-zinc-silicate coat
    public static readonly FayingSurface ClassC = new("class-c", slipFactor: 0.30);   // wire-brushed / galvanized + roughened
    public static readonly FayingSurface ClassD = new("class-d", slipFactor: 0.20);   // untreated
    public double SlipFactor { get; }
}

// The head geometry as the TWO published EN 1993-1-8 Table 3.4 corrections it drives: k2, the tension-resistance
// coefficient (0.9 for every head the table names, 0.63 countersunk), and the bearing-thickness deduction, because a
// countersink removes half its own depth from the ply and the code sizes that depth at half the bolt diameter. One
// row carries both, so a countersunk connection cannot pick up one correction and miss the other.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeadForm {
    public static readonly HeadForm Hexagon     = new("hexagon",     tensionFactor: 0.90, thicknessDeductionRatio: 0.00);
    public static readonly HeadForm Countersunk = new("countersunk", tensionFactor: 0.63, thicknessDeductionRatio: 0.25);
    public double TensionFactor { get; }
    public double ThicknessDeductionRatio { get; }
}

// The shear plane as its TWO independent published columns rather than one fused scalar: the AREA the plane cuts (the
// ISO/ASME tensile stress area through the thread, the gross shank area through the plain body) and the α_v the code
// tabulates for that plane. Table 3.4 gives the shank plane 0.6 for EVERY class while the threaded plane splits per
// class, so the shank arm answers a constant and the threaded arm reads the grade arm's own column — and a grade the
// Eurocode does not tabulate answers None on both, which is what makes the refusal reachable instead of implicit.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShearPlane {
    const double ShankAlphaV = 0.60;
    public static readonly ShearPlane Threaded = new("threaded", static thread => thread.StressAreaMm2,  static arm => arm.EurocodeAlphaV);
    public static readonly ShearPlane Shank    = new("shank",    static thread => thread.NominalAreaMm2, static arm => arm.EurocodeAlphaV.Map(static _ => ShankAlphaV));
    [UseDelegateFromConstructor] public partial double ResistanceAreaMm2(ThreadRow thread);
    [UseDelegateFromConstructor] public partial Option<double> ShearFactor(GradeProperties.Fastener arm);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-size hex hardware envelope, carried for BOTH thread systems: head height, nut height, and the plain-washer
// bore/outside/thickness triple are dimensioned by ISO 4014/4032/7089 and by ASME B18.2.1/B18.2.2/B18.22.1 alike, so
// they are plain columns. The BEARING-FACE and UNDER-HEAD FILLET diameters are optional because only the ISO product
// declares them: an inch head is dimensioned across flats and corners, and a dw/da column on a UNC row could only be
// an ISO shape transplanted onto a product that never published one. Presence of this envelope on a placement IS the
// washer declaration — the retired bool said a washer was fitted while the geometry it implied lived elsewhere.
public readonly record struct HexHardware(
    double HeadHeightMm, Option<double> BearingDiameterMm, Option<double> FilletDiameterMm,
    double NutHeightMm, double WasherInnerMm, double WasherOuterMm, double WasherThicknessMm);

// ISO 261/724 + ASME B1.1 thread row. Only the three columns the standards genuinely PRINT independently are stored —
// major diameter, pitch, and across-flats — because every remaining thread dimension is the ISO 68-1 60° form's own
// algebra over those three. The basic minor d1 = d − 1.25·H, the pitch diameter d2 = d − 0.75·H, the rounded root
// d3 = d − (17/12)·H, and the fundamental triangle height H = P/(2·tan(α/2)) all descend from the ONE flank angle.
// StressAreaMm2 reproduces the printed tensile stress area EXACTLY by its own standard's formula over the series
// coefficient, so one derivation serves both tables and a transcription slip in either is unrepresentable. Tag is the
// designation token for inch rows ("3/8" -> "0375"); Key doubles as the token for metric rows.
public readonly record struct ThreadRow(
    string Key, ThreadSeries Series, double MajorMm, double PitchMm, double AcrossFlatsMm,
    Option<HexHardware> Hardware = default, Option<string> Tag = default) {

    public const double InchToMm = 25.4;        // the ONE inch basis both the thread table and the grade bands convert on
    public const double FlankAngleDeg = 60.0;   // ISO 68-1 / ASME B1.1 included angle — the ONE form constant below

    public string Designation => Tag.IfNone(Key);
    public double FundamentalHeightMm => PitchMm / (2.0 * Math.Tan(FlankAngleDeg * Math.PI / 360.0));   // H
    public double MinorMm => MajorMm - 1.25 * FundamentalHeightMm;                                      // ISO 724 / ASME basic minor d1
    public double PitchDiameterMm => MajorMm - 0.75 * FundamentalHeightMm;                              // ISO 724 d2
    public double RootMinorMm => MajorMm - 17.0 / 12.0 * FundamentalHeightMm;                           // ISO 898-1 rounded-root d3
    public double AcrossCornersMm => AcrossFlatsMm * 2.0 / Math.Sqrt(3.0);                              // e = s·2/√3
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorMm * MajorMm;                                  // gross shank area
    public double StressAreaMm2 => Math.PI / 4.0 * Math.Pow(MajorMm - Series.StressAreaCoefficient * PitchMm, 2.0);
    public double RunoutMm => 2.5 * PitchMm;                                                            // ISO 3508 incomplete-thread allowance x
    // EN 1993-1-8 §3.6.1(4) d_m: the mean of the across-points and across-flats dimensions of the head or nut,
    // whichever is smaller — the punching-shear diameter, distinct from the ISO 4014 washer-face dw the shop reads.
    public double PunchingDiameterMm => 0.5 * (AcrossFlatsMm + AcrossCornersMm);
}

// The >threshold mechanical step a size-banded class carries, in the units its own body prints. ISO 898-1 bands class
// 8.8 above M16; SAE J429 bands grade 2 above 3/4 in and grade 5 above 1 in; ASTM F3125 unified the legacy A325
// over-1-in reduction AWAY, so no F3125 row steps and a step transcribed onto one contradicts the specification that
// removed it.
public readonly record struct GradeStep(double AboveMm, double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

// The diameter range a grade's own specification covers. It is REQUIRED on the arm rather than optional because every
// body scopes its classes, and the scope is what makes an unplaced roster row honest: class 9.8 exists only to M16,
// SAE 5.2 and 8.2 only to 1 in, the twist-off F3125 grades only to 1-1/4 in.
public readonly record struct SizeBand(double MinMm, double MaxMm) {
    public bool Covers(double diameterMm) => diameterMm >= MinMm && diameterMm <= MaxMm;
}

// The EFFECTIVE mechanical band at a thread size — the ONE band read every projection routes through, so an M20 8.8
// reads 600/830/660 and an M12 8.8 580/800/640, never a hybrid row. ProofStressMpa is OPTIONAL because F3125
// acceptance is tensile and yield alone: the proof-load stresses circulate in one secondary reproduction, and typed
// absence is the honest carrier for a cell no second source confirms.
public readonly record struct FastenerBand(Option<double> ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

// The FASTENER arm's physics, co-located with the family that owns it: component#MATERIAL_GRADE declares the columns
// and this page states what they mean. SpecifiedUltimateMpa is the ultimate each body designates for DESIGN and
// preload, distinct from the acceptance minimum — an ISO class designates Rm,nom (the leading number × 100, the
// EN 1993-1-8 Table 3.1 f_ub exactly) while the Table 3 acceptance minimum sits above it for the classes that round
// up. EurocodeAlphaV is the Table 3.4 THREADED-plane α_v, Some ONLY for the seven classes Table 3.1 tabulates, so
// inventing one for 9.8, 12.9, or any SAE/ASTM grade is the failure the Option exists to make impossible.
public partial record GradeProperties {
    public sealed partial record Fastener {
        // The size-banded read: a stepped class answers its own >threshold row above the step diameter and its base
        // columns below, so no consumer ever pairs a base proof stress with a stepped tensile minimum.
        public FastenerBand At(ThreadRow thread) =>
            Step.Filter(step => thread.MajorMm > step.AboveMm)
                .Map(static step => new FastenerBand(Some(step.ProofStressMpa), step.TensileStrengthMpa, step.MinimumYieldMpa))
                .IfNone(new FastenerBand(ProofStressMpa, TensileStrengthMpa, MinimumYieldMpa));

        // The SIZE SCOPE alone — the SYSTEM exclusion needs the owning row's authority and therefore lands on
        // MaterialGrade, which is the only surface that holds both halves of the law.
        public bool Covers(ThreadRow thread) => Sizes.Covers(thread.MajorMm);
    }
}

// The fastener members of the one grade identity. Admits is the SYSTEM exclusion and the SIZE SCOPE in one law: an
// ISO class pairs metric threads and an SAE/ASTM grade inch threads — the retired `bool Metric` column is exactly the
// authority's own print system, so it derives here rather than being stored and kept in step by hand. A grade with no
// fastener arm answers false and None, which is what makes a rebar or timber row reaching a bolt path a typed refusal
// instead of an arm nobody matched.
public sealed partial class MaterialGrade {
    public Option<GradeProperties.Fastener> FastenerArm => Columns is GradeProperties.Fastener arm ? Some(arm) : None;
    public bool Admits(ThreadRow thread) =>
        FastenerArm.Exists(arm => (Authority == ComponentAuthority.En) == thread.Series.Metric && arm.Covers(thread));
    public Option<FastenerBand> At(ThreadRow thread) => FastenerArm.Map(arm => arm.At(thread));
}

// --- [TABLES] ------------------------------------------------------------------------------
// 17 thread rows: 9 ISO 261 metric coarse + 8 ASME B1.1 UNC (Tag the decimal token), each carrying its own system's
// published hex envelope. NAMED statics so the roster references rows SYMBOLICALLY — a typo'd size is a compile miss,
// never a runtime key.
public static class Threads {
    // The ISO envelope in the millimetres ISO 4014/4032/7089 print, bearing face and fillet included.
    static Option<HexHardware> Iso(double headHeight, double bearing, double fillet, double nutHeight, double washerInner, double washerOuter, double washerThickness) =>
        Some(new HexHardware(headHeight, Some(bearing), Some(fillet), nutHeight, washerInner, washerOuter, washerThickness));

    // The ASME envelope in the INCHES B18.2.1 (head height), B18.2.2 (nut thickness), and B18.22.1 (Type A narrow
    // washer) print, converted once — a pre-rounded millimetre column would hide which digits the standard published
    // behind a conversion the reader has to invert. The heavy-hex and wide-washer series are DIFFERENT products with
    // their own tables, so a row here never blends them.
    static Option<HexHardware> Asme(double headHeightIn, double nutHeightIn, double washerInnerIn, double washerOuterIn, double washerThicknessIn) =>
        Some(new HexHardware(headHeightIn * ThreadRow.InchToMm, None, None, nutHeightIn * ThreadRow.InchToMm,
            washerInnerIn * ThreadRow.InchToMm, washerOuterIn * ThreadRow.InchToMm, washerThicknessIn * ThreadRow.InchToMm));

    // An inch row states the FRACTION and the threads per inch its own designation IS, plus the across-flats fraction
    // B18.2.1 prints, and the mint derives every millimetre column — so the 25.4-multiples and 25.4/n reciprocals
    // cannot drift from the size they name.
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

// --- [OPERATIONS] --------------------------------------------------------------------------
// The single-fastener DESIGN values over (ThreadRow, GradeProperties.Fastener) — every projection here is already
// divided by the partial factor its own DesignBasis row publishes, so the receipts Rasm.Compute reads off the seam
// fold demand directly and the group resistance and combined-action interaction compose them without re-dividing.
public static class Fastening {
    // The joint partial factor, READ from the capacity#SECTION_CAPACITY DesignBasis row the placement declares —
    // `en1993-1-8` is the joints row and carries γM2 = 1.25 in the slot EN 1993 puts it in. This page spells no
    // factor of its own, so an annex moving the recommended value moves ONE cell. The read is GATED on the format
    // because an EN 1993-1-8 resistance under a φ-format basis would divide a nominal by unity and publish an AISC
    // §J3 verdict this page never computed; the refusal names the basis that would have to own that arm.
    public static Fin<double> JointFactor(DesignBasis basis, Op key) =>
        basis.Format == SafetyFormat.LimitState
            ? Fin.Succ(basis.GammaM2)
            : new ComponentFault.BasisUnsupported(key, basis, ComponentFamily.Fastener);

    // F_v,Rd = α_v·f_ub·A/γM2. The α_v and the area both ride the ShearPlane row, and a grade EN 1993-1-8 does not
    // tabulate refuses HERE rather than borrowing a neighbouring class's factor.
    public static Fin<double> ShearResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, ShearPlane plane, DesignBasis basis, Op key) =>
        from gamma in JointFactor(basis, key)
        from alphaV in plane.ShearFactor(arm).ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Fastener, typeof(GradeProperties.Fastener)))
        select alphaV * arm.SpecifiedUltimateMpa * plane.ResistanceAreaMm2(thread) / gamma * 1e-3;

    // F_t,Rd = k2·f_ub·A_s/γM2 over the head's own k2.
    public static Fin<double> TensionResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, HeadForm head, DesignBasis basis, Op key) =>
        from gamma in JointFactor(basis, key)
        from tabulated in arm.EurocodeAlphaV.ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Fastener, typeof(GradeProperties.Fastener)))
        select head.TensionFactor * arm.SpecifiedUltimateMpa * thread.StressAreaMm2 / gamma * 1e-3;

    // B_p,Rd = 0.6·π·d_m·t_p·f_u/γM2 — the EN 1993-1-8 §3.6.1(4) punching shear of the ply under the head or nut,
    // read off the head envelope's own across-flats/across-corners mean. It is a PLY resistance, so it takes the ply
    // scalars and no grade column at all.
    public static Fin<double> PunchingResistanceKn(ThreadRow thread, double plyThicknessMm, double plyUltimateMpa, DesignBasis basis, Op key) =>
        JointFactor(basis, key).Map(gamma => 0.6 * Math.PI * thread.PunchingDiameterMm * plyThicknessMm * plyUltimateMpa / gamma * 1e-3);

    // ISO 4014 reference thread length: b = 2d plus a per-band constant, the band keyed on the bolt's OWN nominal
    // length. The three additions were a four-level ternary whose numeric thresholds and constants were interleaved;
    // as rows the table is what the standard prints, and a revised band is one row. The top band's ceiling is
    // unbounded, so the fallback names that same row rather than a fabricated zero.
    public readonly record struct ReferenceLengthBand(double LengthCeilingMm, double AdditionMm);
    static readonly ImmutableArray<ReferenceLengthBand> ReferenceLengths = [new(125.0, 6.0), new(200.0, 12.0), new(double.PositiveInfinity, 25.0)];

    // A dowel/rivet is all shank; a headless threaded part (nut/coupler) threads its whole length; a headed threaded
    // part takes the ISO 4014 reference clamped to its own length for a short fully-threaded bolt.
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

    // EC5 §8.5 dowel-type TIMBER connection — the cross-material composition of the fastener and timber vocabularies.
    // Embedment enters ANGLED: f_h,0,k = 0.082·(1 − 0.01·d)·ρk is the parallel-to-grain value, and the §8.5.1.1
    // reduction f_h,α,k = f_h,0,k/(k90·sin²α + cos²α) carries it to the actual load-to-grain angle, k90 riding each
    // side's own material class through its GradeProperties.Timber intercept (softwood 1.35 + 0.015d, LVL 1.30, D
    // classes 0.90). A bolt loaded across the grain embeds at roughly two thirds of its parallel value on a softwood
    // side, so an angle-free embedment over-states every non-parallel connection the clause is written for. The
    // fastener yield moment is My,Rk = 0.3·fu,b·d^2.6, the per-shear-plane timber-to-timber single-shear
    // characteristic Fv,Rk is the MINIMUM over the six Johansen modes (the rope-effect Fax/4 term taken 0 — the
    // withdrawal capacity is hardware-specific data), and the design value is kmod·Fv,Rk/γM at the timber owner's own
    // CONNECTION partial factor, which EN 1995-1-1 Table 2.3 sets independently of any member form.
    // The FIVE input admissions are INDEPENDENT, so they accumulate: a call with a zero thickness AND a negative
    // diameter names both, where one conjunction behind one message named neither.
    public static Fin<double> TimberDowelShearKn(
        double diameterMm, double fastenerUltimateMpa, double loadToGrainDeg,
        GradeProperties.Timber side1, double t1Mm, GradeProperties.Timber side2, double t2Mm,
        ServiceClass service, LoadDuration duration, Op key) =>
        from admitted in (
            Positive(diameterMm, "d", key),
            Positive(fastenerUltimateMpa, "fu", key),
            Positive(t1Mm, "t1", key),
            Positive(t2Mm, "t2", key),
            guard(double.IsFinite(loadToGrainDeg), new KernelFault.OutOfRange(nameof(loadToGrainDeg), loadToGrainDeg, "finite", Some(key))).ToValidation())
            .Apply(static (_, _, _, _, _) => unit).As().ToFin()
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

    static Validation<Error, Unit> Positive(double value, string label, Op key) =>
        guard(double.IsFinite(value) && value > 0.0,
            new KernelFault.OutOfRange(label, value, "finite and positive", Some(key))).ToValidation();
}

// The seed-time DetailLane.Realization bag. The FastenerForm complex row carries the ISO 68-1 thread algebra and the
// ISO 4014/4032/7089 hex envelope the shop cuts and turns from — the derived geometry has exactly one consumer and it
// is the fabrication document, so the form columns are published rather than computed and discarded.
public static class FastenerDetail {
    public static Fin<PropertyBag> Of(FastenerKind kind, StockFacts facts, Option<ThreadRow> thread, EvidenceGrade source) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, facts.DiameterMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.NominalLength, Dimension.LengthDim, facts.LengthMm * 1e-3)
        from form in thread.Match(Some: t => FormRow(kind, t, facts.LengthMm).Map(Some), None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        // A role-carrying cladding kind stamps the seam AnchorType row beside FastenerType — the anchor SYSTEM a
        // stone bag names, derived off the kind row, never a per-seed literal.
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

    // The columns every hex product declares ride the map unconditionally; the two only the ISO product dimensions
    // are OMITTED where absent rather than written as a zero, so a UNC bag content-keys on the envelope its standards
    // actually publish and a reader never mistakes a missing dimension for a measured one.
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

// --- [COMPOSITION] -------------------------------------------------------------------------
// The face BOTH stock cases answer, minted by ONE dispatch. Eight separate two-arm Switches over the same union were
// eight copies of one correspondence, so a ninth shared column meant a ninth dispatch; here it is one more field.
// UltimateMpa is OPTIONAL because a threaded row's ultimate lives on its grade's fastener arm: the seed coherence
// census refuses a grade carrying none, so the absence is the type stating what the roster proves rather than a
// measure anyone takes.
public readonly record struct StockFacts(
    FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, Option<double> UltimateMpa,
    ComponentAuthority Authority, MaterialId Substance, MaterialId Appearance) {
    public ComponentStandard Standard => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);
}

// Threaded rows reference thread and grade currencies symbolically; plain rows carry only shank facts.
[Union]
public abstract partial record StockRow {
    private StockRow() { }
    public sealed record Threaded(FastenerKind Kind, ThreadRow Thread, MaterialGrade Grade, double LengthMm) : StockRow;
    // UltimateMpaColumn is the PUBLISHED tensile strength of the plain shank — ASTM F1667 common nail 690, EN 10025
    // dowel bar 400, ASTM A502 rivet 415 — the one datum the EC5 §8.5 yield-moment relation needs and no thread/grade
    // pair carries for a plain product.
    public sealed record Plain(
        FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, double UltimateMpaColumn,
        ComponentAuthority Authority, MaterialId Substance, MaterialId Appearance) : StockRow;

    // The ONE projection: a threaded row reads its grade's tensile strength at its own thread band, a plain row its
    // published column, and every downstream consumer — geometry, IFC binding, the detail bag, the EC5 dowel check,
    // the seed law selectors — reads the same record. The designation drops the class separator so an 8.8 bolt keys
    // as `m16-88`, which is the token the shop document and the ComponentId share.
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

    // The row census, ACCUMULATING: the kind's thread trait, the grade's system-and-size admission, the arm's
    // presence, and the length are INDEPENDENT columns, so a row wrong in three ways names all three in ONE verdict.
    // The first-refusal chain this replaced reported one defect per build and hid the rest behind a fix.
    public Validation<Error, Unit> Coherence(Op key) => Switch(
        threaded: row => (
            Prove(row.Kind.Traits.Admits(FastenerTrait.Threaded), new KernelFault.InvalidValue(nameof(row.Kind), "a threaded stock kind", Some(key))),
            Prove(row.Grade.Admits(row.Thread), new KernelFault.InvalidValue(nameof(row.Thread), "a thread admitted by its grade", Some(key))),
            Prove(row.Grade.FastenerArm.IsSome, new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Fastener)),
            Prove(double.IsFinite(row.LengthMm) && row.LengthMm > 0.0, new KernelFault.OutOfRange(nameof(row.LengthMm), row.LengthMm, "finite and positive", Some(key))))
            .Apply(static (_, _, _, _) => unit).As(),
        plain: row => (
            Prove(!row.Kind.Traits.Admits(FastenerTrait.Threaded), new KernelFault.InvalidValue(nameof(row.Kind), "a plain stock kind", Some(key))),
            Prove(double.IsFinite(row.DiameterMm) && row.DiameterMm > 0.0, new KernelFault.OutOfRange(nameof(row.DiameterMm), row.DiameterMm, "finite and positive", Some(key))),
            Prove(double.IsFinite(row.LengthMm) && row.LengthMm > 0.0, new KernelFault.OutOfRange(nameof(row.LengthMm), row.LengthMm, "finite and positive", Some(key))))
            .Apply(static (_, _, _) => unit).As());

    static Validation<Error, Unit> Prove(bool held, Error fault) => guard(held, fault).ToValidation();
}

// The CONNECTION a catalogued fastener sits in — the state no stock row can carry, as one closed family whose cases
// each hold EXACTLY the evidence their receipt arm consumes. It rides ONE Option column on the capacity placement, so
// a bolted verdict costs the placement one decision rather than a per-family argument tail, and the modality is
// recoverable from the value alone. The washer is an Option<HexHardware>: presence IS the declaration and the value
// is the declared product envelope, so an ISO 7090 chamfered washer and an ISO 7089 plain one are two values rather
// than one bool plus a lookup that could not tell them apart. The timber sides are MaterialGrade rows, the EC5 arm
// extracted once at the capacity producer.
[Union]
public abstract partial record FastenerPlacement {
    private FastenerPlacement() { }
    public sealed record Bearing(BoltCategory Category, HeadForm Head, ShearPlane Plane, int GripPlies, int ShearPlanes, Option<HexHardware> Washer, BearingDesign Ply) : FastenerPlacement;
    public sealed record SlipCritical(BoltCategory Category, FayingSurface Faying, HeadForm Head, int GripPlies, int ShearPlanes, Option<HexHardware> Washer, FastenerInstallation Install) : FastenerPlacement;
    public sealed record TimberDowel(MaterialGrade Side1, double Thickness1Mm, MaterialGrade Side2, double Thickness2Mm, double LoadToGrainDeg, int ShearPlanes, ServiceClass Service, LoadDuration Duration) : FastenerPlacement;
}

// --- [POLICIES] ----------------------------------------------------------------------------
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
        new StockRow.Plain(FastenerKind.Nail,  "8d-common",  3.33, 63.5,  690.0, ComponentAuthority.Astm, MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Nail,  "10d-common", 3.76, 76.2,  690.0, ComponentAuthority.Astm, MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Dowel, "dowel-20",  20.00, 100.0, 400.0, ComponentAuthority.En,   MaterialId.Of("steel.fastener-dowel"), MaterialId.Of("metal.steel")),
        new StockRow.Plain(FastenerKind.Rivet, "rivet-0500", 12.70, 38.1, 415.0, ComponentAuthority.Astm, MaterialId.Of("steel.fastener-rivet"), MaterialId.Of("metal.iron")));

    // Every stocked row transcribes a product standard whole — the thread geometry off ISO 68-1/261, the strength
    // band off the grade row, and the length off the product standard's own length series — so one evidence grade
    // covers the roster rather than a per-case selector whose arms would agree.
    static readonly EvidenceGrade Stock = EvidenceGrade.Catalogue;

    // The seed POLICY value: this page states the roster and the law, component#COMPONENT_SEED owns the traverse.
    // The regional receipt derives from the row's own authority, so an EN class seeds `eu` and an SAE grade `us`
    // without a per-body ComponentStandard static.
    public static readonly SeedLaw<StockRow> Law = SeedLaw<StockRow>.Of(
        family: ComponentFamily.Fastener,
        designation: static row => $"fastener.{row.Facts.Kind.Key}-{row.Facts.Designation}",
        coherence: static (row, key) => row.Coherence(key),
        profile: static (row, key) => SectionProfile.Circle.Of(row.Facts.DiameterMm, key),
        substance: static row => row.Facts.Substance,
        source: static _ => Stock,
        standard: static row => row.Facts.Standard,
        detail: Some<Func<StockRow, SectionProfile, Op, Fin<PropertyBag>>>(
            static (row, _, _) => FastenerDetail.Of(row.Facts.Kind, row.Facts, row.Thread, Stock)),
        appearance: static row => row.Facts.Appearance,
        ifc: static row => row.Facts.Kind.Ifc);

    static readonly FrozenDictionary<ComponentId, StockRow> Table =
        Roster.ToFrozenDictionary(static row => ComponentId.Create($"fastener.{row.Facts.Kind.Key}-{row.Facts.Designation}"), static row => row);

    public static Fin<StockRow> Resolve(Component component, Op key) =>
        Table.TryGetValue(component.Designation, out StockRow row)
            ? Fin.Succ(row)
            : new ComponentFault.ComponentMissing(key, ProfileRef.Of(component.Designation.Value));

    // The ComponentFamily.Fastener CAPACITY producer. A single fastener's design values are meaningless without the
    // CONNECTION it sits in, so the placement's FastenerPlacement column is the input and its case selects the
    // receipt: a bearing connection lifts the assembly its own Of already admitted, a preloaded connection the slip
    // state of that same assembly, and a dowel-type timber connection the EC5 per-plane value this page computes. The
    // refusal survives ONLY where the placement declares no fastener connection at all — the one state a catalogue
    // row genuinely cannot price, and it names the column that fixes it.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Resolve(component, key)
        from connection in placement.Fastener.ToFin(
            new ComponentFault.ConnectionMissing(key, component.Designation))
        from receipt in connection.Switch(
            bearing: state =>
                from assembly in Assembly(row, state.Category, FayingSurface.None, state.Head, state.GripPlies, state.ShearPlanes, state.Washer, key)
                select (CapacityReceipt)new CapacityReceipt.Bolt(component.Designation, assembly, state.Ply, state.Plane),
            slipCritical: state =>
                from assembly in Assembly(row, state.Category, state.Faying, state.Head, state.GripPlies, state.ShearPlanes, state.Washer, key)
                select (CapacityReceipt)new CapacityReceipt.SlipCritical(component.Designation, assembly, state.Install),
            timberDowel: state =>
                from ultimate in row.Facts.UltimateMpa.ToFin(
                    new ComponentFault.GradeBandMissing(key, ComponentFamily.Fastener, typeof(StockFacts)))
                from side1 in TimberArm(state.Side1, key)
                from side2 in TimberArm(state.Side2, key)
                from perPlane in Fastening.TimberDowelShearKn(
                    row.Facts.DiameterMm, ultimate, state.LoadToGrainDeg,
                    side1, state.Thickness1Mm, side2, state.Thickness2Mm, state.Service, state.Duration, key)
                select (CapacityReceipt)new CapacityReceipt.TimberDowel(component.Designation, perPlane, state.ShearPlanes))
        from capacity in SectionCapacity.Lift(receipt, key)
        select capacity;

    // The bolt-assembly admission both bearing arms share: a plain stock row carries no thread and no grade arm, so
    // the refusal names the stock rather than letting a nail reach a bolt resistance.
    static Fin<FastenerAssembly> Assembly(StockRow row, BoltCategory category, FayingSurface faying, HeadForm head, int gripPlies, int shearPlanes, Option<HexHardware> washer, Op key) =>
        from thread in row.Thread.ToFin(new KernelFault.InvalidValue(nameof(row.Thread), "a threaded stock row", Some(key)))
        from grade in row.Grade.ToFin(new KernelFault.InvalidValue(nameof(row.Grade), "a threaded stock grade", Some(key)))
        from assembly in FastenerAssembly.Of(thread, grade, category, faying, head, gripPlies, shearPlanes, washer, key)
        select assembly;

    // The EC5 clause reads DENSITY and the k90 intercept, both columns of the timber grade payload, so a grade from
    // another family reaching a dowel connection refuses here instead of embedding on a column it does not carry.
    static Fin<GradeProperties.Timber> TimberArm(MaterialGrade grade, Op key) =>
        grade.Columns is GradeProperties.Timber arm
            ? Fin.Succ(arm)
            : new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Timber);
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` owns the installed bolt state and its own resistance projections; `BearingDesign` owns the ply the shank bears against and derives its EN 1993-1-8 Table 3.4 factors from the bolt-group geometry; `BoltPosition` and `HoleShape` own the published position and hole-form policy; `FastenerInstallation` admits the shared `(ks, γM3, km)` slip-and-torque policy.
- Cases: one assembly shape for every modality — a non-preloaded (A/D) assembly resolves `FayingSurface.None` and returns `None` for preload, slip, and tightening torque; a preloaded (B/C/E) assembly requires a named slip class and returns `Some` design values — never a numeric absence sentinel and never a `PreloadedBolt`/`BearingBolt` pair. `BoltPosition` closes the four-cell product of the two independent Table 3.4 discriminants: end-versus-inner along the load path selects α_d, edge-versus-inner across it selects k1.
- Entry: `FastenerAssembly.Of(thread, grade, category, faying, head, gripPlies, shearPlanes, washer, key)` ACCUMULATES its four independent admissions — a system- or size-mismatched thread/grade pair, a missing fastener arm, a preloaded category over a non-preloadable grade, and a preloaded category with `FayingSurface.None` — then admits the two discrete counts, and carries the PROVED arm onto the assembly so no projection re-unwraps it. `BearingDesign.Of` admits the ply and its bolt-group distances once.
- Growth: a new connection modality is a `BoltCategory`/`FayingSurface` row the assembly reads; a new hole form one `HoleShape` row; a new bolt-group position one `BoltPosition` row; the multi-bolt group `ΣFs,Rd`, the long-joint `β`, and the `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` interaction are `Rasm.Compute` consumers over these single-bolt design values.
- Boundary: `Count` admits the discrete grip and shear-plane columns. `BearingDesign` takes the DISTANCES the code's own formulas consume and derives `k1` and `α_b` from them, so a caller cannot hand the resistance one opaque scalar in which a transposed edge and end distance is invisible; the hole-shape reduction and the countersink thickness deduction are rows the same derivation reads. Every resistance takes the placement's `DesignBasis` and reads γM2 through `Fastening.JointFactor` — this section spells no partial factor either. The preload is bounded by the grade's own yield load, because a pretension above the elastic limit is a tightening method the assembly cannot represent. A washer's ABSENCE is the absence of a washer, so its hardness, outer diameter, and thickness are all `None` together rather than a bool guarding three separate reads.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The EN 1993-1-8 Table 3.4 bolt position as the FLATTENED PRODUCT of its two independent discriminants: the
// load-path position selects α_d (an end bolt reads its end distance e1, an inner bolt its pitch p1 less a quarter),
// and the transverse position selects k1 (an edge bolt reads 2.8·e2/d0 − 1.7, an inner bolt 1.4·p2/d0 − 1.7). One row
// per cell keeps both reads at one level and makes a mis-paired rule unrepresentable.
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

// The EN 1993-1-8 Table 3.4 hole-form reduction on the bearing resistance.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HoleShape {
    public static readonly HoleShape Normal               = new("normal",                bearingFactor: 1.0);
    public static readonly HoleShape Oversize             = new("oversize",              bearingFactor: 0.8);
    public static readonly HoleShape SlottedPerpendicular = new("slotted-perpendicular", bearingFactor: 0.6);
    public double BearingFactor { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ply the shank bears against, carrying the GEOMETRY EN 1993-1-8 Table 3.4 consumes rather than a pre-collapsed
// scalar: the loadwise distance (e1 for an end bolt, p1 for an inner one), the transverse distance (e2 or p2), the
// hole diameter, its form, and the bolt-group position. k1 and α_b are DERIVED here, so the two published factors
// stay separable on the receipt and an edge distance transposed into the end slot changes the answer visibly instead
// of disappearing into one number. The generated validation owns the positive-finite guard.
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
        double holeDiameterMm, HoleShape hole, BoltPosition position, Op key) =>
        key.AcceptValidated<BearingDesign>(
            Validate(plyThicknessMm, plyUltimateMpa, loadwiseDistanceMm, transverseDistanceMm, holeDiameterMm, hole, position, out BearingDesign design), design);

    public double K1 => Math.Min(Position.K1Raw(TransverseDistanceMm, HoleDiameterMm), 2.5);
    public double AlphaB(GradeProperties.Fastener arm) =>
        Math.Min(Math.Min(Position.AlphaD(LoadwiseDistanceMm, HoleDiameterMm), arm.SpecifiedUltimateMpa / PlyUltimateMpa), 1.0);

    // F_b,Rd = k1·α_b·f_u·d·t/γM2 over the BOLT's nominal diameter and the thinnest connected ply, the countersink
    // removing its own half-depth — sized at a quarter of the bolt diameter — from that thickness.
    public Fin<double> ResistanceKn(ThreadRow thread, GradeProperties.Fastener arm, HeadForm head, DesignBasis basis, Op key) =>
        Fastening.JointFactor(basis, key).Map(gamma =>
            Hole.BearingFactor * K1 * AlphaB(arm) * PlyUltimateMpa * thread.MajorMm
                * (PlyThicknessMm - head.ThicknessDeductionRatio * thread.MajorMm) / gamma * 1e-3);
}

// The EN 1993-1-8 §3.9 / EN 1090-2 §8.5 installation design set admitted ONCE: ks the hole-tolerance factor, γM3 the
// slip partial factor, km the manufacturer-declared EN 14399-2 k-class torque factor. γM3 is an INSTALLATION column
// rather than a DesignBasis read because EN 1993-1-8 keys it on the slip limit state a project declares (1.10 at
// ultimate, 1.25 for an oversize or slotted hole), which is a connection decision the basis row cannot carry.
[ComplexValueObject]
public readonly partial struct FastenerInstallation {
    public double Ks { get; }
    public double GammaM3 { get; }
    public double Km { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double ks, ref double gammaM3, ref double km) =>
        validationError = double.IsFinite(ks) && ks > 0.0 && double.IsFinite(gammaM3) && gammaM3 > 0.0 && double.IsFinite(km) && km > 0.0
            ? null
            : new ValidationError($"Fastener installation factors must be finite and positive; received {ks:R}, {gammaM3:R}, {km:R}.");

    public static Fin<FastenerInstallation> Of(double ks, double gammaM3, double km, Op key) =>
        key.AcceptValidated<FastenerInstallation>(Validate(ks, gammaM3, km, out FastenerInstallation design), design);
}

// The complete bolt-connection receipt over the standards rows: preload, slip, and washer projections are the
// EN 1993-1-8 §3.9 single-bolt design values the Rasm.Compute slip-critical and combined-action checks read. The
// PROVED fastener arm rides on the record because Of already admitted it — every projection below reads a column
// rather than re-unwrapping the union, which is what makes the whole surface Option-free except where a value is
// genuinely absent.
public readonly record struct FastenerAssembly(
    ThreadRow Thread, MaterialGrade Grade, GradeProperties.Fastener Arm, BoltCategory Category,
    FayingSurface Faying, HeadForm Head, Count GripPlies, Count ShearPlanes, Option<HexHardware> Washer) {

    // The four admissions are INDEPENDENT — a wrong-system thread, a non-fastener grade, a non-preloadable grade in a
    // preloaded joint, and a preloaded joint with no faying class are four separate authoring defects — so they
    // accumulate and a caller learns everything wrong with one connection in one verdict.
    public static Fin<FastenerAssembly> Of(
        ThreadRow thread, MaterialGrade grade, BoltCategory category, FayingSurface faying, HeadForm head,
        int gripPlies, int shearPlanes, Option<HexHardware> washer, Op key) =>
        from proven in (
            Prove(grade.Admits(thread), new KernelFault.InvalidValue(nameof(thread), "a thread admitted by its grade", Some(key))),
            Prove(grade.FastenerArm.IsSome, new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Fastener)),
            Prove(!category.Preloaded || grade.FastenerArm.Exists(static a => a.Preloadable), new KernelFault.InvalidValue(nameof(grade), "a preloadable grade for a preloaded connection", Some(key))),
            Prove(!category.Preloaded || faying != FayingSurface.None, new KernelFault.InvalidValue(nameof(faying), "a faying class for a preloaded connection", Some(key))))
            .Apply(static (_, _, _, _) => unit).As().ToFin()
        from arm in grade.FastenerArm.ToFin(new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Fastener))
        from plies in key.AcceptValidated<Count>(candidate: gripPlies)
        from planes in key.AcceptValidated<Count>(candidate: shearPlanes)
        select new FastenerAssembly(thread, grade, arm, category, category.Preloaded ? faying : FayingSurface.None, head, plies, planes, washer);

    static Validation<Error, Unit> Prove(bool held, Error fault) => guard(held, fault).ToValidation();

    public FastenerBand Band => Arm.At(Thread);

    // The two published loads a tightening must stay under, and the governing one. The PROOF load is the stress the
    // specification requires the bolt to sustain with no permanent set, and where a body prints it, it binds tighter
    // than yield — an ISO 8.8 proofs at 580 MPa against a 640 MPa yield — so the ceiling is the lesser of the two and
    // a grade whose body prints no proof load is bounded by its yield alone.
    public double YieldLoadKn => Band.MinimumYieldMpa * Thread.StressAreaMm2 * 1e-3;
    public Option<double> ProofLoadKn => Band.ProofStressMpa.Map(stress => stress * Thread.StressAreaMm2 * 1e-3);
    public double PreloadCeilingKn => ProofLoadKn.Map(proof => Math.Min(proof, YieldLoadKn)).IfNone(YieldLoadKn);

    // Fp,C = 0.7·fub·As over the size-banded read. None IS a snug-tight non-preloaded connection, the absence the
    // Rasm.Compute consumer reads through the Option — never numeric zero, which would price a preload the joint has
    // not. A pretension above the ceiling answers None as well: that is not a weaker preload, it is a tightening the
    // assembly does not represent.
    public Option<double> PreloadKn =>
        Category.Preloaded
            ? Some(0.7 * Arm.SpecifiedUltimateMpa * Thread.StressAreaMm2 * 1e-3).Filter(preload => preload <= PreloadCeilingKn)
            : None;

    public Option<double> SlipResistanceKn(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Ks * ShearPlanes.Value * Faying.SlipFactor * preload / design.GammaM3);

    public Option<double> TighteningTorqueNm(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Km * (Thread.MajorMm * 1e-3) * (preload * 1e3));

    // The group shear over every plane, the tension under the head's own k2, and the bearing against the ply — the
    // three columns the Connection verdict folds, each already a design resistance under the declared basis.
    public Fin<double> ShearResistanceKn(ShearPlane plane, DesignBasis basis, Op key) =>
        Fastening.ShearResistanceKn(Thread, Arm, plane, basis, key).Map(perPlane => perPlane * ShearPlanes.Value);
    public Fin<double> TensionResistanceKn(DesignBasis basis, Op key) => Fastening.TensionResistanceKn(Thread, Arm, Head, basis, key);
    public Fin<double> BearingResistanceKn(BearingDesign ply, DesignBasis basis, Op key) => ply.ResistanceKn(Thread, Arm, Head, basis, key);
    public Fin<double> PunchingResistanceKn(BearingDesign ply, DesignBasis basis, Op key) =>
        Fastening.PunchingResistanceKn(Thread, ply.PlyThicknessMm, ply.PlyUltimateMpa, basis, key);

    // ISO 7090 300 HV (chamfered, preloaded high-strength) vs ISO 7089 200 HV (plain). A connection with no washer
    // has no washer hardness — absence, not the hardness of a part that is not there — and the declared envelope
    // carries the outer diameter and thickness the shop reads, so all three answer together.
    public Option<double> WasherHardnessHv => Washer.Map(_ => Arm.Preloadable ? 300.0 : 200.0);
    public Option<double> WasherOuterMm => Washer.Map(static h => h.WasherOuterMm);
    public Option<double> WasherThicknessMm => Washer.Map(static h => h.WasherThicknessMm);
    public Option<double> NutHeightMm => Thread.Hardware.Map(static h => h.NutHeightMm);
}
```

## [04]-[RESEARCH]

(none)
