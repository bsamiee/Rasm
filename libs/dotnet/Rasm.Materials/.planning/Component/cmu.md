# [MATERIALS_CMU]

THE CMU SEED PAGE GROUNDED IN ASTM C90-16a + TMS 602-16/-22 + ACI 216.1 + NCMA TEK 6-2B/7-1D. A concrete block is one `ComponentRow` the ONE `component#COMPONENT_SEED` generator mints from `CmuSeed.Roster` under `CmuSeed.Law` — `ComponentFamily.Cmu` (`ComponentClass.Minor`, `DetailLane.Realization`, `admits: CellularRectangle`, `crossNominal: GrossRectangleMm.WidthMm`), never a `ConcreteBlock` type and never a bespoke payload record. The bed-plane geometry is the parent `SectionProfile.CellularRectangle` (`WidthMm` the through-wall thickness — the family cross nominal, `DepthMm` the along-wall unit length, `Seq<VoidCell>` the per-cell fill-state lattice the private `Lattice` generator lays from the ASTM face-shell and web columns); the vocabulary is the five FORM-law SmartEnums (`CmuGrade`/`CmuDensity`/`CmuAggregate`/`CmuSpecialUnit`/`CmuFinish`) beside the `component#MATERIAL_GRADE` `MaterialGrade` cmu rows whose `GradeProperties.Cmu` arm carries the TMS 602 Table 2 strengths; the realization columns ride the typed `CmuRow` table with per-column provenance. `NetSection`/`GroutedSection` are ONE `SectionSolver.Solve` `CellularRectangle` arm, whose bed-plane solve IS the TMS 402 net cross-section (net area under axial compression, both-axis moduli for out-of-plane flexure): the `VoidCell.Grouted` flag selects the result through one code path — the seeded lattice yields the AS-BUILT net (only ungrouted cells void, a fully-grouted unit the solid rectangle), and the DESIGN net is the same solve over `CmuPhysics.DesignCells`, the manufactured-basis projection the `Coring` bucket reads — so this page calls no solver and owns no perimeter builder.

DIMENSIONS ARE DERIVED, NOT AUTHORED BESIDE THEIR MODULE. ASTM C90 is an INCH-POUND standard whose SI values are stated as mathematical conversions for information only, and it publishes no dimension table at all — it publishes tolerances about a *specified* dimension. A `CmuRow` therefore carries the COORDINATING MODULE (the nominal 4/6/8/10/12 in width, the 8 in course, the 16 in run) and every actual dimension is that module less the one ASTM `3/8 in` coordinating joint, which makes the module arithmetic exact by construction rather than a second authored series a reader must check against the first. `CmuSeed.Module` then re-adds the joint and lands back on the nominal, and the C90 §6.1 permissible deviation rides as the manufacturing tolerance band beside it.

`CmuPhysics` owns fire rating, thermal resistance, self-weight, equivalent thickness, solid fraction, and grout fraction over `(lattice, CmuDensity, CmuAggregate, CmuSpecialUnit)` as bag-free derivations, and the family's `DetailLane.Realization` bag carries the seed-computed `DetailSchema.ProfileSubtype` IFC profile-def token off `CmuPhysics.IfcSubtypeOf` beside its row's own `EvidenceGrade` — the wire datum the `Rasm.Bim` egress profile lane reads; the physics axes never land as bag rows. `CmuSeed.Table` is the `SeedJoin`-built `ComponentId`-keyed typed join from an M7-resolved component to its strength, density, aggregate, finish, and molding axes. The `GradeProperties.Cmu` arm carries the TMS 602 `f'm` and its optional Type-N unit-strength column with the mortar-keyed read and the unit-strength-method inversion landed here, and `masonry#MASONRY_FAMILY` `RuptureModulus.For` supplies the mortar-keyed flexural-tension row the capacity producer derives.

## [01]-[INDEX]

- [02]-[CMU_FAMILY]: the five SmartEnums (`CmuGrade` the ASTM C90 §5.3/§5.4 hollow-solid form, `CmuDensity` the C90 Table 2 class with its NCMA-derived conductivity, `CmuAggregate` the four ACI 216.1 fire categories keyed by rating period with the blended-aggregate rule, `CmuSpecialUnit` molding geometry, `CmuFinish` architectural surface), the `GradeProperties.Cmu` TMS 602-16/-22 Table 2 reads with the `MortarType` inversion, the `CmuRow` provenance-columned seed table over coordinating modules, the `CmuPhysics` fire/thermal/mass row with the geometry-derived `Coring` bucket and the `IfcSubtypeOf` wire token, and the `CmuSeed` set (`Roster`/`Law` seeding the `DetailSchema.Realization` bag + the `Properties` contract lowering + `Module` coursing projection + `Capacity` the basis-threaded producer + the `SeedJoin` axis join + the private `Lattice`).

## [02]-[CMU_FAMILY]

- Owner: `CmuSeed` the roster, the seed law, and the capacity producer; `CmuPhysics` the ONE physical-property row; the five FORM-law SmartEnums (runtime key lookup + derivation columns, so they STAY per `SEED_ROW_LAW` tier 3); the `GradeProperties.Cmu` partial members the Table 2 columns mean; `CmuRow` the AUTHORED standards table (no admitted producer exists); `CmuSeed.Table` the fallible `ComponentId`-keyed axis join an axis consumer pairs with the M7-resolved `Component.Profile` (the ONE legal axis path — the `DetailLane.Realization` bag carries the wire and evidence rows, never an axis column).
- Cases: grade {hollow, solid} (ASTM C90 §5.3/§5.4, the form alone — the solid net-area floor has its own owner); strength {the five `ComponentFamily.Cmu` `MaterialGrade` rows f2000..f3000, TMS 602-16/-22 Table 2 `f'm` + the two mortar columns, PUBLISHED}; density {lightweight <1680, medium 1680–2000, normal ≥2000 kg/m³, each carrying its C90 absorption caps and its NCMA-band-derived conductivity}; aggregate {the four ACI 216.1 / IBC `722.3.2` categories, each its published equivalent-thickness cells keyed by RATING PERIOD}; special {standard, bond-beam, knockout, channel, lintel, sash, control-joint, open-end}; finish {precision, split-face, scored, ribbed}. A unit is one `CmuRow`; a new grade/strength/density/aggregate/special/finish is one case/row — never a per-block type.
- Entry: `ComponentSeed.Rows(context, CmuSeed.Roster, CmuSeed.Law)` — this page states the roster and the policy, never the fold. The law's coherence proves grade/form, fill/reinforcement correspondence, the C90 Table 1 face-shell floor with its footnote-B split residual, the normalized-web-area floor, and the C90 NOTE 5 density population TOGETHER, so a malformed row names every column it broke in one verdict instead of the first hiding the rest; degenerate, out-of-bounds, or overlapping cells fault inside `CellularRectangle.Of`, and every failure aborts the catalogue. `ComponentRow.Sectioned` reads the constructed profile's own topology, so the M7 map carries the admitted as-built section by derivation rather than by assertion.
- Packages: Rasm.Numerics (`PositiveMagnitude` via the parent profile factories), Rasm.Domain (`Context`/`Op`, the kernel `Tolerance`/`ToleranceLane` the blend closure admits through), Rasm.Element (`MaterialId`, `EvidenceGrade`, the contract `DetailSchema`/`PropertyCategory` currencies), Thinktecture.Runtime.Extensions (`[SmartEnum]`, generated `TryGet`/`Items`/`Switch`; `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Validation`/`Fin`), the parent `component#COMPONENT_OWNER`/`#SECTION_PROFILE`/`#MATERIAL_GRADE`/`#COMPONENT_SEED` owners and `masonry#MASONRY_FAMILY` for the shared `RuptureModulus`, `WallAcoustics`, and `RatingPeriod`. The cmu generative data is AUTHORED in-fence; ONLY the section integral crosses to VividOrange, through the parent solver (`.api/api-vividorange-sections-sectionproperties.md`).
- Growth: a new ASTM unit (metric A-series, half-high, architectural) is one `CmuRow`; a grouting/reinforcing variant is row columns (`GroutedCells`/`ReinforcedCells`/`RebarBarMm`); molding and finish variants are `CmuSpecialUnit`/`CmuFinish` rows the host extrudes and the lattice reads; a further published fire period is one `RatingPeriod` row and one cell per aggregate that publishes it — never a parallel section owner, never a solver edit.
- Boundary: column provenance per `SEED_ROW_LAW`. PUBLISHED: the C90 Table 1 face-shell minima and the single web minimum, the C90 Table 1 normalized-web-area floor, the C90 Table 2 density bands, absorption caps, and net-area compressive floors, the C90 §5.4.1 solid net-area floor, the C90 §6.1 permissible deviation, the TMS 602 Table 2 strengths, the ACI 216.1 equivalent thicknesses and blended-aggregate rule, the NCMA TEK 6-2B density-resistivity bands, and the ASTM C476 grout density. DEFINED: every actual dimension, which is its coordinating module less the ASTM joint by the standard's own coordination. AUTHORED: the per-class representative oven-dry density inside each published band, and the molding fractions. C90 publishes ONE web thickness minimum rather than a per-width end/cross split, so both web columns seed at that minimum while the SPLIT itself stays real geometry (an open-end unit drops its end webs so the end cells run to the unit ends) — and the normalized web area, whose C140 measurement convention this repo does not possess, rides a DECLARED `Option` column gated against the published floor only where a row declares one, never a computed claim. The wire spelling is the AS-BUILT occupancy derivation `CmuPhysics.IfcSubtypeOf(cell)` (`IfcArbitraryProfileDefWithVoids` iff any UNGROUTED cell remains — the single-void `IfcRectangleHollowProfileDef` cannot carry two distinct cells — `IfcRectangleProfileDef` for a solid or fully-grouted lattice), the derived token seeded as the `DetailSchema.ProfileSubtype` realization-bag row — Bim references no AEC peer, so the wire spelling is carried row data, never a cross-package call; the manufacturing grade never contradicts the grouted state; the element stamp is the `ComponentFamily.Cmu.Ifc` concrete leaf, whose object-type discriminator keeps it distinct from the clay sibling's leaf under the reverse type-candidate read. `DraftDegrees`/`FaceShellFlareMm` are captured generative columns the host materialization reads off the seed table — `VoidCell` carries fill-state only.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade Hollow = new("hollow");
    public static readonly CmuGrade Solid  = new("solid");
}

public partial record GradeProperties {
    public sealed partial record Cmu {
        public Option<double> RequiredUnitMpa(MortarType mortar) => mortar.Switch(
            state: this,
            m: static strength => Some(strength.NetUnitMsMpa),
            s: static strength => Some(strength.NetUnitMsMpa),
            n: static strength => strength.NetUnitNMpa,
            o: static _ => Option<double>.None,
            k: static _ => Option<double>.None);
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Cmu> CmuArm => Columns is GradeProperties.Cmu arm ? Some(arm) : None;
}

public sealed partial class MaterialGrade {
    public static Option<MaterialGrade> CmuOf(double netUnitStrengthMpa, MortarType mortar) =>
        !double.IsFinite(netUnitStrengthMpa) || netUnitStrengthMpa <= 0.0
            ? Option<MaterialGrade>.None
            : toSeq(toSeq(Items)
                    .Choose(static row => row.Columns is GradeProperties.Cmu arm ? Some((Row: row, Arm: arm)) : None)
                    .Filter(pair => pair.Arm.RequiredUnitMpa(mortar).Exists(required => netUnitStrengthMpa >= required))
                    .OrderByDescending(static pair => pair.Arm.FmMpa))
                .Map(static pair => pair.Row)
                .Head;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuDensity {
    public static readonly CmuDensity Lightweight = new("lightweight", ovenDryKgPerM3: 1600.0, lowerBoundKgPerM3: Option<double>.None, upperBoundKgPerM3: Some(1680.0),
        maxAbsorptionKgPerM3: 288.0, maxAbsorptionIndividualKgPerM3: 320.0);
    public static readonly CmuDensity Medium = new("medium", ovenDryKgPerM3: 1840.0, lowerBoundKgPerM3: Some(1680.0), upperBoundKgPerM3: Some(2000.0),
        maxAbsorptionKgPerM3: 240.0, maxAbsorptionIndividualKgPerM3: 272.0);
    public static readonly CmuDensity Normal = new("normal", ovenDryKgPerM3: 2160.0, lowerBoundKgPerM3: Some(2000.0), upperBoundKgPerM3: Option<double>.None,
        maxAbsorptionKgPerM3: 208.0, maxAbsorptionIndividualKgPerM3: 240.0);

    public double OvenDryKgPerM3 { get; }
    public Option<double> LowerBoundKgPerM3 { get; }
    public Option<double> UpperBoundKgPerM3 { get; }
    public double MaxAbsorptionKgPerM3 { get; }
    public double MaxAbsorptionIndividualKgPerM3 { get; }

    public const double MinNetAreaCompressiveMpa = 13.8;
    public const double MinNetAreaCompressiveIndividualMpa = 12.4;
    public const double PopulationFloorKgPerM3 = 1360.0;
    public const double PopulationCeilingKgPerM3 = 2320.0;

    public bool Holds(double densityKgPerM3) =>
        LowerBoundKgPerM3.Match(Some: lo => densityKgPerM3 >= lo, None: () => true)
        && UpperBoundKgPerM3.Match(Some: hi => densityKgPerM3 < hi, None: () => true);

    public double ConductivityWPerMK => ConcreteResistivity.ConductivityAt(OvenDryKgPerM3);
    public (double LowWPerMK, double HighWPerMK) ConductivityBandWPerMK => ConcreteResistivity.BandAt(OvenDryKgPerM3);
}

public static class ConcreteResistivity {
    const double BtuInchPerHrFt2FToWPerMK = 0.1442279;
    const double PoundPerFt3ToKgPerM3 = 16.018463;

    static readonly Seq<(double Pcf, double RLow, double RHigh)> Bands = Seq(
        (85.0,  0.23, 0.34),
        (95.0,  0.18, 0.28),
        (105.0, 0.14, 0.23),
        (115.0, 0.11, 0.19),
        (125.0, 0.08, 0.15),
        (135.0, 0.07, 0.12),
        (140.0, 0.06, 0.11));

    public static readonly double MortarConductivityWPerMK = BtuInchPerHrFt2FToWPerMK / 0.10;

    public static (double LowWPerMK, double HighWPerMK) BandAt(double densityKgPerM3) {
        double pcf = Math.Clamp(densityKgPerM3 / PoundPerFt3ToKgPerM3, Bands.Head.Map(static b => b.Pcf).IfNone(85.0), Bands.Last.Map(static b => b.Pcf).IfNone(140.0));
        (double Pcf, double RLow, double RHigh) lo = Bands.Filter(b => b.Pcf <= pcf).Last.IfNone(Bands.Head.IfNone((85.0, 0.23, 0.34)));
        (double Pcf, double RLow, double RHigh) hi = Bands.Filter(b => b.Pcf >= pcf).Head.IfNone(Bands.Last.IfNone((140.0, 0.06, 0.11)));
        double t = hi.Pcf > lo.Pcf ? (pcf - lo.Pcf) / (hi.Pcf - lo.Pcf) : 0.0;
        return (BtuInchPerHrFt2FToWPerMK / (lo.RHigh + (hi.RHigh - lo.RHigh) * t),
                BtuInchPerHrFt2FToWPerMK / (lo.RLow + (hi.RLow - lo.RLow) * t));
    }

    public static double ConductivityAt(double densityKgPerM3) =>
        BandAt(densityKgPerM3) is var band ? (band.LowWPerMK + band.HighWPerMK) / 2.0 : 0.0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate CalcareousOrSiliceousGravel = new("calcareous-or-siliceous-gravel", oneHourMm: 71.1, further: Seq((RatingPeriod.ThreeHour, 135.0)));
    public static readonly CmuAggregate LimestoneCindersOrSlag      = new("limestone-cinders-or-slag",      oneHourMm: 68.6, further: Seq<(RatingPeriod, double)>());
    public static readonly CmuAggregate ExpandedClayShaleOrSlate    = new("expanded-clay-shale-or-slate",   oneHourMm: 66.0, further: Seq((RatingPeriod.ThreeHour, 112.0)));
    public static readonly CmuAggregate ExpandedSlagOrPumice        = new("expanded-slag-or-pumice",        oneHourMm: 53.3, further: Seq<(RatingPeriod, double)>());
    public double OneHourMm { get; }
    public Seq<(RatingPeriod Period, double Mm)> Further { get; }

    public Option<double> RequiredThicknessMm(RatingPeriod period) =>
        period == RatingPeriod.OneHour
            ? Some(OneHourMm)
            : Further.Find(cell => cell.Period == period).Map(static cell => cell.Mm);

    public static Fin<double> BlendedThicknessMm(RatingPeriod period, Op key, params ReadOnlySpan<(CmuAggregate Aggregate, double Fraction)> mix) {
        Seq<(CmuAggregate Aggregate, double Fraction)> blend = toSeq([.. mix]);
        return
            from closure in Tolerance.Of(ToleranceLane.Conservation, VolumeClosureBand, key)
            from unitVolume in guard(!blend.IsEmpty && Math.Abs(blend.Sum(static m => m.Fraction) - 1.0) <= closure.Value,
                new KernelFault.OutOfRange(nameof(blend), blend.Sum(static m => m.Fraction), "fractions summing to one", Some(key)))
            from weighted in blend.Traverse(m => m.Aggregate.RequiredThicknessMm(period)
                .ToFin(new ComponentFault.FireThicknessMissing(key, period))
                .Map(required => required * m.Fraction)).As()
            select weighted.Sum();
    }

    const double VolumeClosureBand = 1e-9;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuSpecialUnit {
    public static readonly CmuSpecialUnit Standard     = new("standard",      endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit BondBeam     = new("bond-beam",     endWebsPresent: true,  crossWebFraction: 0.25, troughDepthFraction: 0.55, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit Knockout     = new("knockout",      endWebsPresent: true,  crossWebFraction: 0.50, troughDepthFraction: 0.00, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit Channel      = new("channel",       endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.70, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit Lintel       = new("lintel",        endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.85, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit Sash         = new("sash",          endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.50);
    public static readonly CmuSpecialUnit ControlJoint = new("control-joint", endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 1.00);
    public static readonly CmuSpecialUnit OpenEnd      = new("open-end",      endWebsPresent: false, crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);
    public bool EndWebsPresent { get; }
    public double CrossWebFraction { get; }
    public double TroughDepthFraction { get; }
    public double ControlSlotFraction { get; }

    public int SeparatedCells(int mouldedCells) => CrossWebFraction > 0.0 ? Math.Max(1, mouldedCells) : 1;
    public double CrossWebMm(double mouldedCrossWebMm) => mouldedCrossWebMm * CrossWebFraction;
    public double TroughVoidShare => Math.Clamp(TroughDepthFraction, 0.0, 1.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuFinish {
    public static readonly CmuFinish Precision = new("precision",  splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish SplitFace = new("split-face", splitDepthMm: 6.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Scored    = new("scored",     splitDepthMm: 0.0, scoreCount: 2, scoreSpacingMm: 130.0, ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Ribbed    = new("ribbed",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 8, ribDepthMm: 10.0);
    public double SplitDepthMm { get; }
    public int ScoreCount { get; }
    public double ScoreSpacingMm { get; }
    public int RibCount { get; }
    public double RibDepthMm { get; }

    public const double SplitResidualFloorMm = 19.1;

    public double EffectiveFaceShellMm(double mouldedFaceShellMm) => mouldedFaceShellMm - SplitDepthMm;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct CmuRow(
    string Designation, CmuGrade Grade, MaterialGrade Strength, CmuDensity Density, CmuAggregate Aggregate,
    double WModuleMm, double HModuleMm, double LModuleMm,
    double FaceShellMm, double EndWebMm, double CrossWebMm, int Cells,
    int GroutedCells = 0, int ReinforcedCells = 0, double RebarBarMm = 0.0,
    double FaceShellFlareMm = 0.0, double DraftDegrees = 1.5) {
    public CmuSpecialUnit Special { get; init; } = CmuSpecialUnit.Standard;
    public CmuFinish Finish { get; init; } = CmuFinish.Precision;
    public Option<double> NormalizedWebAreaMm2PerM2 { get; init; } = Option<double>.None;
    public EvidenceGrade Source { get; init; } = EvidenceGrade.Defined;

    public double WMm => WModuleMm - CmuSeed.CoordinatingJointMm;
    public double HMm => HModuleMm - CmuSeed.CoordinatingJointMm;
    public double LMm => LModuleMm - CmuSeed.CoordinatingJointMm;

    public double EffectiveFaceShellMm => Finish.EffectiveFaceShellMm(FaceShellMm);
}

public readonly record struct CmuPhysics(
    double EquivalentThicknessMm,
    Option<RatingPeriod> FireRating,
    double SelfWeightKnPerM2,
    double ThermalResistanceM2KPerW,
    double SolidFraction,
    double GroutedSolidFraction,
    double GroutedCellFraction) {

    public const double GroutDensityKgPerM3 = 2243.0;
    public const double GroutConductivityWPerMK = 1.40;
    public const double CellAirResistanceM2KPerW = 0.18;
    const double GravityMPerS2 = 9.80665;

    public static CmuPhysics Of(SectionProfile.CellularRectangle cell, CmuDensity density, CmuAggregate aggregate, CmuSpecialUnit special) {
        double w = cell.WidthMm.Value, len = cell.DepthMm.Value, gross = w * len;
        double depthShare = special.TroughDepthFraction > 0.0 ? special.TroughVoidShare : 1.0;
        double allVoid = cell.Cells.Sum(static c => c.WidthMm * c.HeightMm);
        double openVoid = cell.Cells.Filter(static c => !c.Grouted).Sum(static c => c.WidthMm * c.HeightMm);
        double net = gross - allVoid;
        double te = (gross - openVoid * depthShare) / len;
        return new(
            EquivalentThicknessMm: te,
            FireRating: RatingPeriod.Floor(Math.Pow(te / aggregate.OneHourMm, 1.7)),
            SelfWeightKnPerM2: ((gross - allVoid * depthShare) * density.OvenDryKgPerM3 + (allVoid - openVoid) * depthShare * GroutDensityKgPerM3) * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: IsothermalPlanes(cell, density),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0,
            GroutedSolidFraction: gross > 0.0 ? Math.Clamp((gross - openVoid) / gross, 0.0, 1.0) : 1.0,
            GroutedCellFraction: allVoid > 0.0 ? (allVoid - openVoid) / allVoid : 0.0);
    }

    public double ArealMassKgPerM2 => SelfWeightKnPerM2 * 1000.0 / GravityMPerS2;

    public static Seq<VoidCell> DesignCells(Seq<VoidCell> cells) =>
        cells.Map(static c => c with { Grouted = false, Reinforced = false });

    static double IsothermalPlanes(SectionProfile.CellularRectangle cell, CmuDensity density) {
        double k = density.ConductivityWPerMK, widthM = cell.WidthMm.Value / 1000.0, len = cell.DepthMm.Value;
        if (cell.Cells.IsEmpty) { return widthM / k; }
        double coreWidthM = cell.Cells.Max(static c => c.WidthMm) / 1000.0;
        double webConductance = (len - cell.Cells.Sum(static c => c.HeightMm)) / len * (k / coreWidthM);
        double cellConductance = cell.Cells.Sum(c => c.HeightMm / len / CellPathResistance(c, coreWidthM, k));
        return (widthM - coreWidthM) / k + 1.0 / (webConductance + cellConductance);
    }

    static double CellPathResistance(VoidCell cell, double coreWidthM, double k) {
        double cellWidthM = cell.WidthMm / 1000.0;
        return (cell.Grouted ? cellWidthM / GroutConductivityWPerMK : CellAirResistanceM2KPerW)
            + Math.Max(0.0, coreWidthM - cellWidthM) / k;
    }

    const double CoredNetFloor = 0.60;

    public static Coring CoringOf(double widthMm, double lengthMm, Seq<VoidCell> cells) {
        double gross = widthMm * lengthMm;
        Seq<VoidCell> design = DesignCells(cells);
        double solid = gross > 0.0 ? (gross - design.Sum(static c => c.WidthMm * c.HeightMm)) / gross : 1.0;
        return (solid, design.Count) switch {
            (>= RuptureModulus.SolidNetFloor, _) => Coring.None,
            (>= CoredNetFloor, _)                => Coring.Perforated10Cell,
            (_, >= 3)                            => Coring.Hollow3Cell,
            _                                    => Coring.Hollow2Cell,
        };
    }

    public static string IfcSubtypeOf(Seq<VoidCell> cells) =>
        cells.Exists(static c => !c.Grouted) ? "IfcArbitraryProfileDefWithVoids" : "IfcRectangleProfileDef";
}

// --- [TABLES] --------------------------------------------------------------------------
public static class CmuSeed {
    public const double CoordinatingJointMm = 9.5;
    public const double PermissibleDeviationMm = 3.2;
    public const double MouldedFeatureDeviationMm = 1.6;
    public const double WebFloorMm = 19.0;
    public const double NormalizedWebAreaFloorMm2PerM2 = 45140.0;

    static readonly MaterialId ConcreteCmu = MaterialId.Of("concrete.cmu");

    static readonly Seq<(double NominalWidthMm, double FloorMm)> FaceShellFloors = Seq(
        (102.0, 19.0),
        (152.0, 25.0),
        (double.PositiveInfinity, 32.0));

    public static double FaceShellFloorMm(double nominalWidthMm) =>
        FaceShellFloors.Filter(f => nominalWidthMm <= f.NominalWidthMm).Head.Map(static f => f.FloorMm).IfNone(32.0);

    public static readonly Seq<CmuRow> Roster = Seq(
        new CmuRow("cmu.4in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 19.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.6in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   152.4, 203.2, 406.4, 25.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow",     CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.8in-hollow-lw",  CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedClayShaleOrSlate, 203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.10in-hollow",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   254.0, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2),
        new CmuRow("cmu.12in-hollow",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   304.8, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 3),
        new CmuRow("cmu.4in-solid",      CmuGrade.Solid,  MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   101.6, 203.2, 406.4, 46.1, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-solid",      CmuGrade.Solid,  MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.CalcareousOrSiliceousGravel, 203.2, 203.2, 406.4, 96.9, WebFloorMm, WebFloorMm, 0),
        new CmuRow("cmu.8in-grouted",    CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2),
        new CmuRow("cmu.8in-reinforced", CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9),
        new CmuRow("cmu.8in-bondbeam",   CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, GroutedCells: 2, ReinforcedCells: 1, RebarBarMm: 12.7) { Special = CmuSpecialUnit.BondBeam },
        new CmuRow("cmu.8in-openend",    CmuGrade.Hollow, MaterialGrade.F2500, CmuDensity.Normal,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2, ReinforcedCells: 1, RebarBarMm: 15.9) { Special = CmuSpecialUnit.OpenEnd },
        new CmuRow("cmu.8in-splitface",  CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Medium,      CmuAggregate.LimestoneCindersOrSlag,   203.2, 203.2, 406.4, 32.0, WebFloorMm, WebFloorMm, 2) { Finish = CmuFinish.SplitFace },
        new CmuRow("cmu.8in-halfhigh",   CmuGrade.Hollow, MaterialGrade.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedSlagOrPumice,     203.2, 101.6, 406.4, 32.0, WebFloorMm, WebFloorMm, 2));

    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, CmuRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static readonly SeedLaw<CmuRow> Law = SeedLaw<CmuRow>.Of(
        family: ComponentFamily.Cmu,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: static (r, key) => SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, Lattice(r), key),
        substance: static _ => ConcreteCmu,
        source: static r => r.Source,
        standard: static r => new ComponentStandard(r.Strength.Authority.Region, CoordinatingJointMm, r.Strength.Authority),
        detail: Some<Func<CmuRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        voids: static r => CmuPhysics.CoringOf(r.WMm, r.LMm, Lattice(r)));

    static Validation<Error, Unit> Coherence(CmuRow r, Op key) =>
        (guard(r.Strength.Family == ComponentFamily.Cmu,
             new ComponentFault.GradeFamilyMismatch(key, r.Strength, ComponentFamily.Cmu)).ToValidation(),
         guard(r.Strength.Columns is GradeProperties.Cmu,
             new ComponentFault.GradeBodyMissing(key, r.Strength, ComponentFamily.Cmu)).ToValidation(),
         guard((r.Grade == CmuGrade.Hollow) == (r.Cells > 0),
             new KernelFault.InvalidValue(nameof(r.Grade), "hollow exactly when the row declares cells", Some(key))).ToValidation(),
         guard(r.Cells >= 0
                 && r.GroutedCells >= 0 && r.GroutedCells <= r.Cells
                 && r.ReinforcedCells >= 0 && r.ReinforcedCells <= r.Cells
                 && double.IsFinite(r.RebarBarMm)
                 && (r.ReinforcedCells == 0 ? r.RebarBarMm == 0.0 : r.RebarBarMm > 0.0),
             new KernelFault.InvalidValue(nameof(r.ReinforcedCells), "cell, fill, and bar declarations that agree", Some(key))).ToValidation(),
         guard(r.FaceShellMm >= FaceShellFloorMm(r.WModuleMm)
                 && r.EndWebMm >= WebFloorMm && r.CrossWebMm >= WebFloorMm
                 && r.EffectiveFaceShellMm >= CmuFinish.SplitResidualFloorMm,
             new KernelFault.InvalidValue(nameof(r.FaceShellMm), "published face-shell and effective-shell floors", Some(key))).ToValidation(),
         guard(r.NormalizedWebAreaMm2PerM2.ForAll(static anw => anw >= NormalizedWebAreaFloorMm2PerM2),
             new KernelFault.InvalidValue(nameof(r.NormalizedWebAreaMm2PerM2), "the published normalized web-area floor", Some(key))).ToValidation(),
         guard(r.Density.Holds(r.Density.OvenDryKgPerM3)
                 && r.Density.OvenDryKgPerM3 >= CmuDensity.PopulationFloorKgPerM3
                 && r.Density.OvenDryKgPerM3 <= CmuDensity.PopulationCeilingKgPerM3,
             new KernelFault.InvalidValue(nameof(r.Density), "a density inside the published population band", Some(key))).ToValidation())
            .Apply(static (_, _, _, _, _, _, _) => unit).As();

    static Fin<PropertyBag> Detail(CmuRow r, SectionProfile profile, Op key) =>
        profile is SectionProfile.CellularRectangle lattice
            ? Fin.Succ(ComponentDetail.RealizationRows(
                ComponentDetail.Token(DetailSchema.ProfileSubtype, CmuPhysics.IfcSubtypeOf(lattice.Cells)),
                ComponentDetail.Sourced(r.Source)))
            : new ComponentFault.ProfileMismatch(key, ComponentFamily.Cmu, profile.GetType());

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in SeedJoin.Resolve(Table, component.Designation, key)
        from solved in section.ToFin(new ComponentFault.SectionUnavailable(key, component.Designation))
        from lattice in component.Profile is SectionProfile.CellularRectangle cell
            ? Fin.Succ(cell)
            : Fin.Fail<SectionProfile.CellularRectangle>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Cmu, component.Profile.GetType()))
        from strength in row.Strength.CmuArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Strength, ComponentFamily.Cmu))
        from capacity in SectionCapacity.Lift(row.ReinforcedCells > 0
            ? new CapacityLift.ReinforcedMasonry(component.Designation, strength, solved, placement.HeightMm, placement.Basis, row, placement.BarGrade)
            : new CapacityLift.Masonry(
                component.Designation, strength, solved, placement.HeightMm, placement.Basis,
                RuptureModulus.For(component.Profile, placement.Rupture,
                    CmuPhysics.Of(lattice, row.Density, row.Aggregate, row.Special).GroutedCellFraction),
                placement.Flexural, placement.System, placement.Mortar),
            key)
        select capacity;

    const double ConcreteSpecificHeatJKgK = 1000.0;
    const double ConcreteVapourMu = 6.0;

    public static Fin<Seq<MaterialPropertySet>> Properties(CmuRow row, SectionProfile.CellularRectangle cell, Op key) =>
        from physics in Fin.Succ(CmuPhysics.Of(cell, row.Density, row.Aggregate, row.Special))
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: row.Density.ConductivityWPerMK,
            specificHeat: ConcreteSpecificHeatJKgK,
            uValue: 1.0 / physics.ThermalResistanceM2KPerW,
            vapourResistanceFactor: ConcreteVapourMu, key)
        from spectrum in WallAcoustics.Of(physics.ArealMassKgPerM2, key)
        from fire in physics.FireRating
            .Map(period => FireResistance.I(period.Key, key).Map(static r => Seq(MaterialPropertySet.OfFire(FireRating.A1, r))))
            .IfNone(Fin.Succ(Seq<MaterialPropertySet>()))
        select Seq(thermal, MaterialPropertySet.OfAcoustic(spectrum)) + fire;

    public static Fin<ComponentUnit> Module(CmuRow row, Op key) =>
        ComponentUnit.Of(row.WMm, row.HMm, row.LMm, row.HMm + CoordinatingJointMm, key);

    static Seq<VoidCell> Lattice(CmuRow r) {
        if (r.Cells <= 0) { return Seq<VoidCell>(); }
        double faceShell = r.EffectiveFaceShellMm;
        double endWeb = r.Special.EndWebsPresent ? r.EndWebMm : 0.0;
        int cells = r.Special.SeparatedCells(r.Cells);
        double crossWeb = r.Special.CrossWebMm(r.CrossWebMm);
        double slotMm = r.Special.ControlSlotFraction * faceShell;
        double usable = r.LMm - slotMm;
        double cellLen = (usable - (2.0 * endWeb + (cells - 1) * crossWeb)) / cells;
        Seq<VoidCell> cores = toSeq(Enumerable.Range(0, cells)).Map(i => new VoidCell(
            XMm: faceShell, YMm: endWeb + i * (cellLen + crossWeb),
            WidthMm: r.WMm - 2.0 * faceShell, HeightMm: cellLen,
            Grouted: i < Math.Max(r.GroutedCells, r.ReinforcedCells) || cells < r.Cells && r.GroutedCells > 0,
            Reinforced: i < r.ReinforcedCells));
        return slotMm > 0.0
            ? cores.Add(new VoidCell(XMm: 0.0, YMm: r.LMm - slotMm, WidthMm: r.WMm, HeightMm: slotMm))
            : cores;
    }
}
```

## [03]-[RESEARCH]

(none)
