# [MATERIALS_CMU]

THE CMU SEED PAGE GROUNDED IN ASTM C90 + TMS 602-16/-22 + ACI 216.1 + NCMA TEK 6-2C. A concrete block is one `ComponentRow` minted by the ONE generator `CmuSeed.Rows : Context -> Fin<Seq<ComponentRow>>` — `ComponentFamily.Cmu` (`ComponentClass.Minor`, `DetailLane.None`, `admits: CellularRectangle`, `crossNominal: GrossRectangleMm.WidthMm`, `rows: CmuSeed.Rows`), never a `ConcreteBlock` type and never a bespoke payload record. The prior `CmuSection`/`CmuShape`/`CmuCell` records and the per-family `ComponentCatalogue.BuildCmuRows`/`CmuSections` folds are DELETED with zero column loss: the bed-plane geometry lands as the parent `SectionProfile.CellularRectangle` (`WidthMm` the through-wall thickness — the family cross nominal, `DepthMm` the along-wall unit length, `Seq<VoidCell>` the per-cell fill-state lattice the private `Lattice` generator lays from the ASTM face-shell/end-web/cross-web columns); the vocabulary stays on the six kept FORM-law SmartEnums (`CmuGrade`/`CmuStrength`/`CmuDensity`/`CmuAggregate`/`CmuSpecialUnit`/`CmuFinish`); the realization columns ride the typed `CmuRow` table with per-column provenance. `NetSection`/`GroutedSection` COLLAPSE onto the ONE `SectionSolver.Solve` `CellularRectangle` arm: the `VoidCell.Grouted` flag selects the result through one code path — the seeded lattice yields the AS-BUILT net (only ungrouted cells void, a fully-grouted unit the solid rectangle), and the DESIGN net is the same solve over `Cells.Map(c => c with { Grouted = false })` — so this page calls no solver and owns no perimeter builder.

Because `ComponentFamily.Cmu` is `DetailLane.None` (no `PropertyBag` — the brief widens only masonry and glazing), the fire/thermal/self-weight/equivalent-thickness receipts RE-HOME as the ONE `CmuPhysics` typed receipt over `(SectionProfile.CellularRectangle, CmuDensity, CmuAggregate)` — every quantity is a per-face or per-length ratio in which the unit height cancels exactly, so the receipt is computable at seed time and by any axis-holding consumer without a bag — `CmuSeed.Table` is the designation-keyed frozen join handing any consumer those axes off the M7-resolved `Component`. The prism strength stays the registered `CmuStrength` row (`f'm` + the two TMS 602 mortar columns), feeding `capacity#SECTION_CAPACITY` `MasonryCompression(FmMpa, NetAreaMm2, SectionModulusXMm3, SectionModulusYMm3, SlendernessReduction, FrMpa)` together with the M7-cached as-built section (both net moduli, so the biaxial unity folds each moment against ITS axis) — the `FrMpa` flexural-tension column rides the `masonry#MASONRY_FAMILY` `RuptureModulus` Table 9.1.9.2 row keyed by `MortarSystem`/`MortarType` beside this page's `f'm`; `CmuStrength.RequiredUnitMpa`/`Resolve` keep the `masonry#MASONRY_FAMILY` `MortarType` unit-strength-method seam. The coursing module survives as the `CmuSeed.Module` projection onto the parent `ComponentUnit` (height + the ASTM coordinating joint) — the seed-table datum the future Generation course row reads (`RASM-GENERATION-SPEC` `[05]`).

## [01]-[INDEX]

- [02]-[CMU_FAMILY]: the six kept SmartEnums (`CmuGrade` ASTM C90 unit grade + IFC profile subtype, `CmuStrength` TMS 602-16/-22 Table 2 with the `MortarType` inversion, `CmuDensity` oven-dry density + conductivity, `CmuAggregate` ACI 216.1 fire aggregate, `CmuSpecialUnit` molding geometry, `CmuFinish` architectural surface), the `CmuRow` provenance-columned seed table, the `CmuPhysics` fire/thermal/mass receipt with the `Coring` bucket, and the `CmuSeed` generator (`Rows` fold + `Module` coursing projection + the `Table` designation-keyed axis join + the private `Lattice`).
- [03]-[RESEARCH]: realized decisions.

## [02]-[CMU_FAMILY]

- Owner: `CmuSeed` the ONE generator; `CmuPhysics` the ONE physical receipt; the six FORM-law SmartEnums (runtime key lookup + derivation columns, so they STAY per `SEED_ROW_LAW` tier 3); `CmuRow` the AUTHORED standards table (no admitted producer exists); `CmuSeed.Table` the designation-keyed frozen axis join a bag-free consumer pairs with the M7-resolved `Component.Profile` (the `DetailLane.None` landing surface — the brief forbids a cmu lane flip, so the seed join is the one legal axis path).
- Cases: grade {hollow/solid × load-bearing} (ASTM C90); strength {f2000..f3000} (TMS 602-16/-22 Table 2, `f'm` + the two mortar columns, PUBLISHED); density {lightweight <1680, medium 1680–2000, normal ≥2000 kg/m³}; aggregate {six rows over the four ACI 216.1 / IBC 722.3.2 categories, each its 1-hour equivalent thickness}; special {standard, bond-beam, knockout, channel, lintel, sash, control-joint, open-end}; finish {precision, split-face, scored, ground, ribbed}. A unit is one `CmuRow`; a new grade/strength/density/aggregate/special/finish is one case/row — never a per-block type.
- Entry: `CmuSeed.Rows(Context) : Fin<Seq<ComponentRow>>` — `Traverse`, never `Choose`: a grade/form mismatch faults `ComponentFault.Family` (the TYPED axis singletons make an unknown grade/strength/density/aggregate key UNREPRESENTABLE — no per-axis `TryGet` lift exists to miss), a degenerate lattice, out-of-bounds cell, or overlapping cell faults `ComponentFault.Dimension` inside `CellularRectangle.Of`, and any failure ABORTS `ComponentCatalogue.Of` loudly. Every row seeds `Sectioned: true` — an empty-lattice solid row solves as the full rectangle, a grouted row as its as-built net, so the M7 map carries every cmu unit.
- Packages: Rasm.Numerics (`PositiveMagnitude` via the parent profile factories), Rasm.Domain (`Context`/`Op`), Rasm.Element (`MaterialId`), Thinktecture.Runtime.Extensions (`[SmartEnum]`, generated `TryGet`/`Items`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Traverse`/`Fin`), the parent `component#COMPONENT_OWNER`/`#SECTION_PROFILE` owners. The cmu generative data is AUTHORED in-fence; ONLY the section integral crosses to VividOrange, through the parent solver (`.api/api-vividorange-sections-sectionproperties.md`).
- Growth: a new ASTM unit (metric A-series, half-high, architectural) is one `CmuRow`; a grouting/reinforcing variant is row columns (`GroutedCells`/`ReinforcedCells`/`RebarBarMm`); molding and finish variants are `CmuSpecialUnit`/`CmuFinish` rows the host extrudes — never a parallel section owner, never a solver edit.
- Boundary: column provenance per `SEED_ROW_LAW` — every dimensional and strength value PUBLISHED (ASTM C90-14 Table 1 face-shell/web minima, TMS 602 Table 2 strengths, ACI 216.1 aggregate thicknesses, ASTM C476 grout density); the web law is the SPLIT `2·EndWebMm + (cells−1)·CrossWebMm` span, never one uniform web; an open-end (`EndWebsPresent: false`) unit drops its end webs so the end cells run to the unit ends. The wire spelling stays `CmuGrade.IfcSubtype` row data (`IfcArbitraryProfileDefWithVoids` for a multi-cell hollow — the single-void `IfcRectangleHollowProfileDef` cannot carry two distinct cells — `IfcRectangleProfileDef` for a solid), resolved at the `Rasm.Bim` egress; the element stamp is `IfcBinding.Supertype(ComponentFamily.Cmu.Class)` (`IfcElementComponent` + `NOTDEFINED`). `DraftDegrees`/`FaceShellFlareMm` and the molding/finish fractions are captured generative columns the host materialization reads off the seed table and the SmartEnum rows — `VoidCell` carries fill-state only, and no bag exists on this family.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary — the CmuSeed.Table designation-keyed axis join
using LanguageExt;                                   // Fin, Option, Seq, Traverse
using Rasm.Domain;                                   // Context, Op
using Rasm.Element;                                  // MaterialId (the substance/appearance rows the seed assigns)
using Thinktecture;                                  // [SmartEnum], KeyMemberEqualityComparer, ComparerAccessors, TryGet/Items
using Rasm.Materials.Component;                      // Component/ComponentRow/ComponentFamily/ComponentFault/ComponentStandard/ComponentAuthority/ComponentUnit/Coring/IfcBinding/SectionProfile/VoidCell (the parent owners)
using static LanguageExt.Prelude;

// The cmu seed declares in the ONE Rasm.Materials.Component namespace; MortarType, CmuStrength, and every parent owner
// resolve by bare name, and the ComponentFamily.Cmu policy row folds CmuSeed.Rows. No per-family ComponentCatalogue
// exists — that owner died with the payload.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The unit grade: hollow/solid crossed with load-bearing (ASTM C90 loadbearing, C129 the non-loadbearing spec),
// carrying the IFC profile subtype the Bim egress resolves. A multi-cell HOLLOW unit serializes as
// IfcArbitraryProfileDefWithVoids (explicit per-cell voids); a SOLID or fully-grouted unit as the outer
// IfcRectangleProfileDef. A new grade is one case.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade HollowLoadBearing    = new("hollow-load-bearing",     loadBearing: true,  hollow: true,  ifcSubtype: "IfcArbitraryProfileDefWithVoids");
    public static readonly CmuGrade HollowNonLoadBearing = new("hollow-non-load-bearing", loadBearing: false, hollow: true,  ifcSubtype: "IfcArbitraryProfileDefWithVoids");
    public static readonly CmuGrade SolidLoadBearing     = new("solid-load-bearing",      loadBearing: true,  hollow: false, ifcSubtype: "IfcRectangleProfileDef");
    public static readonly CmuGrade SolidNonLoadBearing  = new("solid-non-load-bearing",  loadBearing: false, hollow: false, ifcSubtype: "IfcRectangleProfileDef");
    public bool LoadBearing { get; }
    public bool Hollow { get; }
    public string IfcSubtype { get; }
}

// The TMS 602-16/-22 Table 2 specified-masonry-strength class: FmMpa IS the specified f'm (the assemblage strength the
// design seam reads, NOT the unit strength); the two PUBLISHED net-area UNIT-strength columns key the mortar band
// (Type M/S the lower, Type N the higher; the empty Type-N cells for f2750/f3000 carry +Infinity so a Type-N unit
// never qualifies). The five rows are the standard's published classes — registered DATA, never a caller double.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuStrength {
    public static readonly CmuStrength F2000 = new("f2000", fmMpa: 13.79, netUnitMsMpa: 13.79, netUnitNMpa: 18.27);
    public static readonly CmuStrength F2250 = new("f2250", fmMpa: 15.51, netUnitMsMpa: 17.93, netUnitNMpa: 23.44);
    public static readonly CmuStrength F2500 = new("f2500", fmMpa: 17.24, netUnitMsMpa: 22.41, netUnitNMpa: 28.96);
    public static readonly CmuStrength F2750 = new("f2750", fmMpa: 18.96, netUnitMsMpa: 26.89, netUnitNMpa: double.PositiveInfinity);
    public static readonly CmuStrength F3000 = new("f3000", fmMpa: 20.69, netUnitMsMpa: 31.03, netUnitNMpa: double.PositiveInfinity);
    public double FmMpa { get; }              // the specified masonry compressive strength f'm (MPa) — the capacity#SECTION_CAPACITY MasonryCompression input
    public double NetUnitMsMpa { get; }       // required net-area unit strength with Type M or S mortar (MPa), PUBLISHED
    public double NetUnitNMpa { get; }        // required net-area unit strength with Type N mortar (MPa); +Infinity where the column is empty

    // The mortar-band column read through the generated exhaustive Switch — a new MortarType row breaks HERE at
    // compile time, never an ==-chain a new row silently falls past. Type O/K sit below the TMS 602 loadbearing
    // floor and qualify for no class.
    public double RequiredUnitMpa(MortarType mortar) => mortar.Switch(
        m: () => NetUnitMsMpa, s: () => NetUnitMsMpa, n: () => NetUnitNMpa,
        o: static () => double.PositiveInfinity, k: static () => double.PositiveInfinity);

    // The unit-strength method, inverted: the HIGHEST f'm class the supplied net-area unit strength qualifies for
    // under the mortar band — the descending-by-f'm scan's first passing row; a below-floor unit rails None.
    public static Option<CmuStrength> Resolve(double netUnitStrengthMpa, MortarType mortar) =>
        toSeq(Items.OrderByDescending(static c => c.FmMpa))
            .Filter(c => netUnitStrengthMpa >= c.RequiredUnitMpa(mortar))
            .Head;
}

// The ASTM C90 oven-dry-density classification (published 1360–2320 kg/m³ banded at 1680 / 2000): the self-weight and
// thermal-mass driver; ConductivityWPerMK the density-class concrete conductivity the isothermal-planes R reads;
// MaxAbsorptionKgPerM3 the ASTM C90 water-absorption cap per class (288/240/208 = 18/15/13 pcf) — the submittal
// quality gate a spec lane reads off the same row, PUBLISHED.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuDensity {
    public static readonly CmuDensity Lightweight = new("lightweight", ovenDryKgPerM3: 1600.0, conductivityWPerMK: 0.50, maxAbsorptionKgPerM3: 288.0);   // < 1680 kg/m³ (105 pcf)
    public static readonly CmuDensity Medium      = new("medium",      ovenDryKgPerM3: 1840.0, conductivityWPerMK: 0.90, maxAbsorptionKgPerM3: 240.0);   // 1680–2000 kg/m³
    public static readonly CmuDensity Normal      = new("normal",      ovenDryKgPerM3: 2160.0, conductivityWPerMK: 1.40, maxAbsorptionKgPerM3: 208.0);   // ≥ 2000 kg/m³ (125 pcf)
    public double OvenDryKgPerM3 { get; }
    public double ConductivityWPerMK { get; }
    public double MaxAbsorptionKgPerM3 { get; }
}

// The ACI 216.1 / IBC 722.3.2 fire aggregate: EqThick1HrMm the equivalent thickness (mm) for a 1-hour rating (the cn
// in R = (te/cn)^1.7). Six rows over the four IBC categories — a lighter aggregate rates at a smaller te.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate Siliceous        = new("siliceous",         eqThick1HrMm: 71.1);   // siliceous gravel — least fire-efficient
    public static readonly CmuAggregate Calcareous       = new("calcareous",        eqThick1HrMm: 71.1);   // calcareous gravel (same IBC category)
    public static readonly CmuAggregate NormalWeight     = new("normal-weight",     eqThick1HrMm: 68.6);   // limestone / cinders / air-cooled slag
    public static readonly CmuAggregate ExpandedClay     = new("expanded-clay",     eqThick1HrMm: 66.0);   // expanded clay / shale / slate
    public static readonly CmuAggregate ExpandedSlag     = new("expanded-slag",     eqThick1HrMm: 53.3);   // expanded slag — most fire-efficient tier
    public static readonly CmuAggregate LightweightBlend = new("lightweight-blend", eqThick1HrMm: 53.3);   // expanded slag or pumice blend
    public double EqThick1HrMm { get; }
}

// The NCMA special-unit molding geometry: EndWebsPresent is the ONE column the lattice reads (an open-end A/H-block
// drops over vertical rebar without lifting); the three fractions are captured generative columns the host solid
// extrudes off the seed table. A new special unit is one row, never a per-shape subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuSpecialUnit {
    public static readonly CmuSpecialUnit Standard     = new("standard",      endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);
    public static readonly CmuSpecialUnit BondBeam     = new("bond-beam",     endWebsPresent: true,  crossWebFraction: 0.25, troughDepthFraction: 0.55, controlSlotFraction: 0.00);   // knocked-down cross webs form the horizontal-bar channel
    public static readonly CmuSpecialUnit Knockout     = new("knockout",      endWebsPresent: true,  crossWebFraction: 0.50, troughDepthFraction: 0.00, controlSlotFraction: 0.00);   // pre-scored webs for field removal
    public static readonly CmuSpecialUnit Channel      = new("channel",       endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.70, controlSlotFraction: 0.00);   // continuous U-trough
    public static readonly CmuSpecialUnit Lintel       = new("lintel",        endWebsPresent: true,  crossWebFraction: 0.00, troughDepthFraction: 0.85, controlSlotFraction: 0.00);   // deep beam trough
    public static readonly CmuSpecialUnit Sash         = new("sash",          endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.50);   // jamb groove for a frame / sealant
    public static readonly CmuSpecialUnit ControlJoint = new("control-joint", endWebsPresent: true,  crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 1.00);   // full-depth vertical shear key
    public static readonly CmuSpecialUnit OpenEnd      = new("open-end",      endWebsPresent: false, crossWebFraction: 1.00, troughDepthFraction: 0.00, controlSlotFraction: 0.00);   // A/H-block — end webs omitted to drop over rebar
    public bool EndWebsPresent { get; }
    public double CrossWebFraction { get; }
    public double TroughDepthFraction { get; }
    public double ControlSlotFraction { get; }
}

// The ASTM C90 architectural surface finish — face texture the host relief-models and the Bim surface-style egress
// reads, never a structural-section modifier. A new finish is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuFinish {
    public static readonly CmuFinish Precision = new("precision",  splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish SplitFace = new("split-face", splitDepthMm: 6.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Scored    = new("scored",     splitDepthMm: 0.0, scoreCount: 2, scoreSpacingMm: 130.0, ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Ground    = new("ground",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);   // burnished / honed — polish, no relief
    public static readonly CmuFinish Ribbed    = new("ribbed",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 8, ribDepthMm: 10.0);
    public double SplitDepthMm { get; }
    public int ScoreCount { get; }
    public double ScoreSpacingMm { get; }
    public int RibCount { get; }
    public double RibDepthMm { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The AUTHORED seed row: TYPED axis singletons + plain published doubles — an unknown grade/strength/density/aggregate
// key is UNREPRESENTABLE at compile time, so the fold carries no per-axis TryGet lift (the brief exemplar row form:
// ThreadSeries.MetricCoarse, never "metric-coarse"). Dimensional columns PUBLISHED (ASTM C90-14 Table 1); HMm is the
// coursing-module datum the Module projection reads; DraftDegrees/FaceShellFlareMm the captured demold geometry the
// host materialization reads off this table. Special/Finish are init columns defaulting to the Standard/Precision
// singletons (a SmartEnum row is no compile-time constant, so they cannot ride the positional default slots).
public readonly record struct CmuRow(
    string Designation, CmuGrade Grade, CmuStrength Strength, CmuDensity Density, CmuAggregate Aggregate,
    double WMm, double HMm, double LMm, double FaceShellMm, double EndWebMm, double CrossWebMm, int Cells,
    int GroutedCells = 0, int ReinforcedCells = 0, double RebarBarMm = 0.0,
    double FaceShellFlareMm = 0.0, double DraftDegrees = 1.5) {
    public CmuSpecialUnit Special { get; init; } = CmuSpecialUnit.Standard;
    public CmuFinish Finish { get; init; } = CmuFinish.Precision;
}

// The ONE physical receipt over (CellularRectangle, CmuDensity, CmuAggregate) — the re-homed fire/thermal/mass columns
// the deleted CmuSection carried, now a bag-free derivation (ComponentFamily.Cmu is DetailLane.None): every quantity is
// a per-face or per-length ratio in which the unit HEIGHT cancels exactly, so seed time and any axis-holding consumer
// (capacity, a Compute fire/thermal runner, the host) compute the identical receipt off the profile fill-state.
public readonly record struct CmuPhysics(
    double EquivalentThicknessMm,        // ACI 216.1 te: (gross − ungrouted void) / length — grout counts as solid
    double FireRatingHours,              // ACI 216.1 / IBC 722.3.2 POWER LAW clamp((te/cn)^1.7, 0, 4), cn per aggregate
    double SelfWeightKnPerM2,            // oven-dry net solid + grouted-cell grout, per wall-face m²
    double ThermalResistanceM2KPerW,     // NCMA TEK 6-2C isothermal planes, material-only (films are the assembly's)
    double SolidFraction,                // design net (every cell open) / gross — the manufactured basis
    double GroutedSolidFraction,         // as-built net / gross
    double GroutedCellFraction) {        // grout-filled share of the cell void, lattice-honest

    public const double GroutDensityKgPerM3 = 2243.0;        // ASTM C476 grout — 140 pcf
    public const double GroutConductivityWPerMK = 1.40;      // a grouted cell conducts like normal-weight concrete
    public const double CellAirResistanceM2KPerW = 0.18;     // ASHRAE vertical air-cavity — the ungrouted-cell path
    const double GravityMPerS2 = 9.80665;

    public static CmuPhysics Of(SectionProfile.CellularRectangle cell, CmuDensity density, CmuAggregate aggregate) {
        double w = cell.WidthMm.Value, len = cell.DepthMm.Value, gross = w * len;
        double allVoid = cell.Cells.Sum(static c => c.WidthMm * c.HeightMm);
        double openVoid = cell.Cells.Filter(static c => !c.Grouted).Sum(static c => c.WidthMm * c.HeightMm);
        double net = gross - allVoid;
        double te = (gross - openVoid) / len;
        return new(
            EquivalentThicknessMm: te,
            FireRatingHours: Math.Clamp(Math.Pow(te / aggregate.EqThick1HrMm, 1.7), 0.0, 4.0),
            SelfWeightKnPerM2: (net * density.OvenDryKgPerM3 + (allVoid - openVoid) * GroutDensityKgPerM3) * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: IsothermalPlanes(cell, density),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0,
            GroutedSolidFraction: gross > 0.0 ? Math.Clamp((gross - openVoid) / gross, 0.0, 1.0) : 1.0,
            GroutedCellFraction: allVoid > 0.0 ? (allVoid - openVoid) / allVoid : 0.0);
    }

    // Two face shells in SERIES with the core; the core a PARALLEL combination of the solid-web path and the per-cell
    // paths (grout k or trapped-air R), conductances weighted by depth-axis length fraction. The face shell and core
    // width DERIVE from the widest cell — a cell-free solid is one homogeneous width/k layer.
    static double IsothermalPlanes(SectionProfile.CellularRectangle cell, CmuDensity density) {
        double k = density.ConductivityWPerMK, widthM = cell.WidthMm.Value / 1000.0, len = cell.DepthMm.Value;
        if (cell.Cells.IsEmpty) { return widthM / k; }
        double coreWidthM = cell.Cells.Max(static c => c.WidthMm) / 1000.0;
        double webConductance = (len - cell.Cells.Sum(static c => c.HeightMm)) / len * (k / coreWidthM);
        double cellConductance = cell.Cells.Sum(c => c.HeightMm / len * (c.Grouted ? GroutConductivityWPerMK / coreWidthM : 1.0 / CellAirResistanceM2KPerW));
        return (widthM - coreWidthM) / k + 1.0 / (webConductance + cellConductance);
    }

    // The coarse Coring bucket on the MANUFACTURED basis (design net — every cell open); the cell count picks the
    // faithful hollow row so a 12-in 3-cell unit never forces onto the 2-cell label.
    public static Coring CoringOf(SectionProfile.CellularRectangle cell) {
        double gross = cell.WidthMm.Value * cell.DepthMm.Value;
        double solid = gross > 0.0 ? (gross - cell.Cells.Sum(static c => c.WidthMm * c.HeightMm)) / gross : 1.0;
        return solid switch {
            >= 0.95 => Coring.None,
            >= 0.75 => Coring.Cored3Hole,
            >= 0.60 => Coring.Perforated10Cell,
            _       => cell.Cells.Count >= 3 ? Coring.Hollow3Cell : Coring.Hollow2Cell,
        };
    }
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class CmuSeed {
    // ASTM C90 dimensional authority (region "us"); the 9.5 mm coordinating joint serves BOTH the bed joint (the
    // Module coursing rise) and the head joint (the run advance) — the retired RunModuleMm datum is derivable, never a twin.
    static readonly ComponentStandard AstmC90 = new("us", StandardJointThicknessMm: 9.5, Authority: ComponentAuthority.Astm);
    static readonly MaterialId ConcreteCmu = MaterialId.Of("concrete.cmu");   // substance and appearance coincide for a plain CMU

    // The ASTM C90 roster, columns PUBLISHED: 4/6/8/10/12-in hollow (face-shell minima 19/25/32 mm by width, end-web/
    // cross-web SPLIT per Table 1), the 4/8-in solids, an 8-in lightweight expanded-clay unit, an 8-in fully-grouted
    // unit, an 8-in single-cell reinforced unit (#5 / 15.9 mm), an 8-in bond-beam, an 8-in open-end reinforced unit,
    // an 8-in split-face architectural unit, and the half-high. A new metric A-series or finish is one further row.
    // PUBLIC: the Generation course fold (Module) and the host materialization read the published columns by designation.
    public static readonly Seq<CmuRow> AstmRows = Seq(
        new CmuRow("cmu.4in-hollow",     CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 90.0,  190.0, 390.0, 19.0, 19.0, 19.0, 2),
        new CmuRow("cmu.6in-hollow",     CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 140.0, 190.0, 390.0, 25.0, 25.0, 25.0, 2),
        new CmuRow("cmu.8in-hollow",     CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2),
        new CmuRow("cmu.8in-hollow-lw",  CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedClay, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2),
        new CmuRow("cmu.10in-hollow",    CmuGrade.HollowLoadBearing,    CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.NormalWeight, 240.0, 190.0, 390.0, 32.0, 38.0, 25.0, 2),
        new CmuRow("cmu.12in-hollow",    CmuGrade.HollowLoadBearing,    CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.NormalWeight, 290.0, 190.0, 390.0, 32.0, 38.0, 25.0, 3),
        new CmuRow("cmu.4in-solid",      CmuGrade.SolidLoadBearing,     CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 90.0,  190.0, 390.0, 45.0, 19.0, 19.0, 0),
        new CmuRow("cmu.8in-solid",      CmuGrade.SolidLoadBearing,     CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.Calcareous,   190.0, 190.0, 390.0, 95.0, 19.0, 19.0, 0),
        new CmuRow("cmu.8in-grouted",    CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, GroutedCells: 2),
        new CmuRow("cmu.8in-reinforced", CmuGrade.HollowLoadBearing,    CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, ReinforcedCells: 1, RebarBarMm: 15.9),
        new CmuRow("cmu.8in-bondbeam",   CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Normal,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, GroutedCells: 2, ReinforcedCells: 1, RebarBarMm: 12.7) { Special = CmuSpecialUnit.BondBeam },
        new CmuRow("cmu.8in-openend",    CmuGrade.HollowLoadBearing,    CmuStrength.F2500, CmuDensity.Normal,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, ReinforcedCells: 1, RebarBarMm: 15.9) { Special = CmuSpecialUnit.OpenEnd },
        new CmuRow("cmu.8in-splitface",  CmuGrade.HollowLoadBearing,    CmuStrength.F2000, CmuDensity.Medium,      CmuAggregate.NormalWeight, 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2) { Finish = CmuFinish.SplitFace },
        new CmuRow("cmu.8in-halfhigh",   CmuGrade.HollowNonLoadBearing, CmuStrength.F2000, CmuDensity.Lightweight, CmuAggregate.ExpandedSlag, 190.0, 90.0,  390.0, 32.0, 32.0, 25.0, 2));

    // The designation-keyed axis join for bag-free consumers (DetailLane.None carries no PropertyBag): a Compute
    // fire/thermal runner or the host resolves the Component off the graph, joins THIS row by designation for the
    // CmuDensity/CmuAggregate/CmuStrength axes and the demold/molding columns, then CmuPhysics.Of(profile,
    // row.Density, row.Aggregate) computes the identical receipt anywhere — one frozen map, never a per-consumer scan.
    public static readonly FrozenDictionary<string, CmuRow> Table =
        AstmRows.ToFrozenDictionary(static r => r.Designation, static r => r);

    // The ONE generator fold (RAIL law): Traverse, never Choose — a grade/lattice form mismatch, an out-of-bounds or
    // overlapping cell, or a Component.Of rejection ABORTS ComponentCatalogue.Of loudly. The per-axis TryGet lifts are
    // the DELETED form: the row columns are the typed singletons, so an unknown axis key never reaches runtime. Every
    // row is Sectioned: true — SectionSolver.Solve nets the AS-BUILT section (empty lattice -> the solid rectangle,
    // grouted cells filled), the M7 map the capacity#SECTION_CAPACITY MasonryCompression check reads.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        AstmRows.Traverse(r =>
            from formed in guard(r.Grade.Hollow == (r.Cells > 0), ComponentFault.Family(context.Key, $"<cmu-grade-form-mismatch:{r.Designation}>"))
            from profile in SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, Lattice(r, r.Special.EndWebsPresent), context.Key)
            let cell = (SectionProfile.CellularRectangle)profile
            from item in Component.Of(
                ComponentFamily.Cmu, r.Designation, profile,
                IfcBinding.Supertype(ComponentFamily.Cmu.Class), CmuPhysics.CoringOf(cell),
                AstmC90, substanceId: ConcreteCmu, appearanceId: ConcreteCmu,
                detail: None, context.Key)
            select new ComponentRow(item, Sectioned: true)).As();

    // The coursing module the Generation course row reads off the seed table (spec [05]): actual height + the ASTM
    // coordinating joint = the 200/100 mm module. The parent ComponentUnit re-admits nothing downstream.
    public static Fin<ComponentUnit> Module(CmuRow row, Op key) =>
        ComponentUnit.Of(row.WMm, row.HMm, row.LMm, row.HMm + AstmC90.StandardJointThicknessMm, key);

    // The ASTM lattice, PURE: cellCount equal cells inset by the face shell across the WIDTH axis, bounded by the end
    // webs and separated by the cross webs along the DEPTH (length) axis — min-corner VoidCells in the profile corner
    // frame, web span 2·EndWeb + (cells−1)·CrossWeb. An open-end unit drops its end webs so the end cells run to the
    // unit ends. The first GroutedCells carry grout, the first ReinforcedCells the bar. A degenerate span yields an
    // out-of-bounds/non-positive cell the CellularRectangle.Of containment gate faults — the boundary owns the rail.
    static Seq<VoidCell> Lattice(CmuRow r, bool endWebsPresent) {
        if (r.Cells <= 0) { return Seq<VoidCell>(); }
        double endWeb = endWebsPresent ? r.EndWebMm : 0.0;
        double cellLen = (r.LMm - (2.0 * endWeb + (r.Cells - 1) * r.CrossWebMm)) / r.Cells;
        return toSeq(Enumerable.Range(0, r.Cells)).Map(i => new VoidCell(
            XMm: r.FaceShellMm, YMm: endWeb + i * (cellLen + r.CrossWebMm),
            WidthMm: r.WMm - 2.0 * r.FaceShellMm, HeightMm: cellLen,
            Grouted: i < r.GroutedCells || i < r.ReinforcedCells, Reinforced: i < r.ReinforcedCells));
    }
}
```

## [03]-[RESEARCH]

- [SEED_PARADIGM]: REALIZED — the bespoke `CmuSection`/`CmuShape`/`CmuCell` payload records, the `ComponentSection.Cmu` arm, and the per-family `ComponentCatalogue.BuildCmuRows`/`CmuSections` folds are DELETED with zero column loss: geometry rides `SectionProfile.CellularRectangle` + the `VoidCell` fill-state lattice, vocabulary the six kept SmartEnums, realization columns the provenance-columned `CmuRow` table. `CmuSeed.Rows` is the ONE `Traverse` generator (the prior `.Choose(...ToOption())` fault-swallow DELETED — a malformed row aborts the catalogue loudly); every row is `Sectioned: true`, and the `hollow == (Cells > 0)` grade/form guard rejects a transcription error the old fold silently seeded.
- [BED_PLANE_ORIENTATION]: REALIZED — the cmu `CellularRectangle` is the BED-PLANE section: `WidthMm` the through-wall thickness (the family `crossNominal` read, preserving the prior 190 mm cross dimension), `DepthMm` the along-wall unit length, cells min-corner-stationed along the depth axis between the end/cross webs. `SectionSolver.Solve` over this plane IS the TMS 402 net cross-section (net area under axial compression, both-axis moduli for out-of-plane flexure) — the receipt `DepthMm`/`WidthMm` columns now carry length/thickness where the deleted per-family solver carried thickness/length, a value-level re-orientation inside the frozen column set, membership `Sectioned`-pinned.
- [FILL_STATE_ONE_PASS]: REALIZED — `NetSection`/`GroutedSection` COLLAPSE onto the ONE `CellularRectangle` solve arm: `Curves.RectWithVoids` voids ONLY ungrouted cells, so the seeded lattice yields the AS-BUILT net the M7 map caches (a fully-grouted unit the solid rectangle, the open-end reinforced unit its exact per-cell net) and the DESIGN net is the same solve over `Cells.Map(c => c with { Grouted = false })` — a data transform, never a second solver path or a page-local perimeter builder.
- [PHYSICS_REHOME]: REALIZED — `ComponentFamily.Cmu` is `DetailLane.None` (the brief widens only masonry + glazing), so the fire/thermal/self-weight/equivalent-thickness receipts re-home as the ONE `CmuPhysics` typed receipt over `(CellularRectangle, CmuDensity, CmuAggregate)`: `te = (gross − ungrouted void)/length`, `R = clamp((te/cn)^1.7, 0, 4)` (ACI 216.1 power law, `cn` the aggregate row), self-weight `(net·ρ_dry + grout·ρ_C476)·g/L`, and the NCMA TEK 6-2C isothermal-planes R whose face-shell and core width DERIVE from the widest cell — every term a per-face/per-length ratio in which the unit height cancels exactly, so no bag is needed and the receipt computes identically at seed and downstream. The axes LAND on `CmuSeed.Table` — the designation-keyed frozen row join a bag-free consumer pairs with the M7-resolved `Component.Profile` (`CmuPhysics.Of(profile, row.Density, row.Aggregate)`), zero bag, zero lane widening; the Compute runner that reads it stays next-campaign forward work per `[CONSUMER_RIPPLE_RULES]`.
- [STRENGTH_SEAM]: REALIZED — `CmuStrength` stays the registered TMS 602-16/-22 Table 2 data (`f'm` + the two mortar columns, `+Infinity` for the empty Type-N cells); `RequiredUnitMpa(mortar)` keys the `masonry#MASONRY_FAMILY` `MortarType` band by identity and `Resolve` inverts the table to the richest qualifying class; the widened `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CmuStrength, ComputedSection, double, RuptureModulus, MortarSystem, MortarType)` reads `FmMpa` plus the M7-cached as-built section (net area AND both net moduli — the biaxial unity's per-axis `φMnx`/`φMny` inputs) and resolves the `MasonryCompression.FrMpa` flexural-tension column on the `masonry#MASONRY_FAMILY` `RuptureModulus` Table 9.1.9.2 row — the grouted-lattice fact selects the hollow-grouted vs hollow-ungrouted row, `CmuPhysics.GroutedCellFraction` is the TMS partial-grout interpolation fraction the footnote path (`RuptureModulus.PartialGrout`) consumes — while the TMS 402 member-stability bracket (`[1 - (h/140r)²]` at `h/r <= 99`, `(70r/h)²` above) stays a placement-level caller input — this unit-section vocabulary never computes member height.
- [COURSING_MODULE]: REALIZED — `CmuSeed.Module(row, key)` projects the seed row onto the parent `ComponentUnit` (width/height/length + `height + 9.5` coursing rise), the datum the future Generation course row reads (`RASM-GENERATION-SPEC` `[05]`); the head-joint run advance is `LengthMm + StandardJointThicknessMm` over the same `ComponentStandard` column, so the retired `RunModuleMm`/`ToUnit` carry no lost data. The half-high row's 90 mm height stays live through this projection.
- [GENERATIVE_COLUMNS]: REALIZED — `DraftDegrees`, `FaceShellFlareMm`, the `CmuSpecialUnit` molding fractions, and the `CmuFinish` relief columns are captured generative geometry the host materialization and the `Rasm.Bim` surface-style egress read off the seed table and the SmartEnum rows; `VoidCell` carries fill-state only (the brief-frozen shape), and the wire spelling stays `CmuGrade.IfcSubtype` row data resolved at the Bim boundary (`IfcArbitraryProfileDefWithVoids` multi-cell, `IfcRectangleProfileDef` solid/fully-grouted).
- [SOURCING]: no admitted producer for the ASTM C90/TMS 602/ACI 216.1 rosters exists, so every column stays AUTHORED/PUBLISHED. The `CmuPhysics` axes land on `CmuSeed.Table` — the designation-keyed seed join is the ONE legal landing, `component.md` lane-law unedited.
