# [MATERIALS_CMU]

THE CMU PROFILEFAMILY GROUNDED IN ASTM C90 + TMS 402. The concrete-masonry-unit cross-section vocabulary — the ASTM C90 cell / face-shell / web dimensional columns, the grade / strength-class / density-class / aggregate-type discriminants, the net cell geometry, the grouted-cell and reinforced-cell augmentation, and the equivalent-thickness fire / thermal-mass / self-weight receipts — is the cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Cmu` case. A concrete block is a `Profile` row, never a `ConcreteBlock` type: the cell geometry, the face-shell/web columns, the void class, the ASTM C90 grade, the TMS 602 net-area strength class, the density class, the aggregate type, and the regional standard are cmu-`Profile` columns, and the `CmuSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry family drives — a CMU run is the same station-stepped course fold over one `Profile`, never a per-family layout. The cmu vocabulary grows by DATA: a new unit is one `CmuRow` catalogue row, a new grade one `CmuGrade` case, a new strength one `CmuStrength` case, a new density one `CmuDensity` case — never a per-block type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard`/`ComputedSection` shape and the shared `ParametricSection.Hollow` solver, `masonry#PROFILE_FAMILY` `Coring`/`MortarType` for the course/joint algebra, `Connection/reinforcement#RC_SECTION` for the reinforced-grouted-cell path, `Profiles/capacity#SECTION_CAPACITY` for the masonry compressive utilisation rail, and the `Rasm` kernel `PositiveMagnitude` for every length column; timber/glazing/steel land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[CMU_FAMILY]: the `CmuGrade` ASTM C90 unit-grade discriminant, the `CmuStrength` TMS 602 net-area-strength / `f'm` axis, the `CmuDensity` oven-dry-density axis, the `CmuAggregate` fire-rating aggregate axis, the `CmuSection` cell/face-shell/grout record (its `NetSection`/`GroutedSection` exact-net solver, `EquivalentThicknessMm`/`FireRatingHours` fire receipt, `SelfWeightKnPerM2` mass receipt, `ToUnit`/`ToCoring` projections), the `CmuStrength.Resolve` TMS 602 unit-strength-method resolution, and the `ProfileCatalogue.BuildCmuRows` ASTM C90 row table plus the `ProfileCatalogue.CmuSections` `ProfileId`→`ComputedSection` map the `profile#PROFILE_OWNER` `ProfileCatalogue.Sections` fold + the `[PROFILE_RESOLUTION]` M7 cache read.

## [02]-[CMU_FAMILY]

- Owner: the cmu unit vocabulary (`CmuGrade` the hollow/solid + load-bearing discriminant, `CmuStrength` the ASTM C90 / TMS 602 net-area compressive-strength class resolving the specified masonry strength `f'm`, `CmuDensity` the lightweight/medium/normal oven-dry-density class, `CmuAggregate` the aggregate type the equivalent-thickness fire rating reads, `CmuSection` the ASTM C90 cell/face-shell/grout record); `ProfileCatalogue.BuildCmuRows` the registered-row seed `profile#PROFILE_OWNER` composes and `ProfileCatalogue.CmuSections` the `ProfileId`→`ComputedSection` section map `profile#PROFILE_OWNER` `ProfileCatalogue.Sections` folds (the realized sibling to `steel#STEEL_FAMILY` `SteelSections` / `timber#TIMBER_FAMILY` `TimberSections`), both deriving from the one `Shapes` seed; the `CmuSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`.
- Cases: grade {hollow-load-bearing, hollow-non-load-bearing, solid-load-bearing, solid-non-load-bearing} — the ASTM C90 unit-grade set; strength {f1350, f1500, f2000, f2500, f3000} — the TMS 602 specified-masonry-strength classes the net-area unit strength resolves; density {lightweight (<1680 kg/m³), medium (1680–2000), normal (≥2000)} — the ASTM C90 oven-dry-density set; aggregate {normal-weight, expanded-slag, expanded-clay, calcareous, siliceous, lightweight-blend} — the ACI 216.1 fire equivalent-thickness aggregate set; a section is a `CmuSection` row over these axes, never a section subtype.
- Entry: `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `CmuSection` — the section→`ProfileUnit` projection (`WidthMm` = actual unit width, `HeightMm` = actual unit height, `CourseHeightMm` = actual unit height plus the bed joint, `LengthMm` = actual unit length) so a CMU member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public Fin<ComputedSection> NetSection(Op key)` the EXACT ungrouted net cross-section (the shared `profile#PROFILE_OWNER` `ParametricSection.Hollow` solver over the cell voids returning net Area AND net inertia AND net plastic moduli, the design seam's true net section); `public Fin<ComputedSection> GroutedSection(double groutFraction, Op key)` the partially/fully-grouted net section the TMS 402 reinforced-masonry consumer reads (the cells re-filled by `groutFraction ∈ [0,1]`, the homogenized transformed section); `public double SelfWeightKnPerM2()` the wall self-weight from the density-class oven-dry density over the net solid plus grout; `public CmuStrength PrismStrength()` the TMS 602 net-area-strength receipt the masonry `Profiles/capacity#SECTION_CAPACITY` `MasonryCompression` utilisation reads; `public double EquivalentThicknessMm()` the ACI 216.1 net-volume/face-area equivalent thickness and `public double FireRatingHours()` the aggregate-dependent fire-resistance rating; `public double SolidFraction()` the quick net-area ratio the `Coring` bucket reads, and `public Coring ToCoring()` the masonry void-class bridge.
- Packages: Rasm (project — `PositiveMagnitude`/`UnitInterval` for every fractional-millimeter section column and the grout fraction), VividOrange.Sections.SectionProperties (via the shared `profile#PROFILE_OWNER` `ParametricSection.Hollow` bridge — the hollow `Perimeter` net-section Green's-theorem integral; `.api/api-vividorange-sections-sectionproperties.md`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the cmu vocabulary grows by data — a new ASTM C90 unit is one `CmuRow` catalogue row keyed by its nominal designation, a new grade one `CmuGrade` case, a new TMS 602 strength one `CmuStrength` row, a new density class one `CmuDensity` row, a new fire aggregate one `CmuAggregate` row — never a per-block type, never a per-family `Profile` variant. A precise per-cell draft taper, a scored/ground-face architectural finish, or a metric A-series unit is a `CmuRow`/`CellVoids` column growth, never a parallel section owner. A timber/glazing family lands its own vocabulary on its own page the way cmu carries `CmuGrade`/`CmuSection`.
- Boundary: the cmu vocabulary is a realized `ProfileFamily` — a per-block class is the deleted form; `CmuSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the ASTM C90 minimum face-shell (19/25/32 mm by nominal width, 16 mm where solid-grouted) and 19 mm minimum web thickness admitting as fractional millimeters; the structural-design input is the typed `CmuStrength` net-area-strength receipt (the TMS 602 unit-strength method resolving the specified masonry strength `f'm` from the unit strength and the `MortarType`), never a raw double, the sibling of the `steel#STEEL_FAMILY` `SteelGrade.YieldMpa` and the `timber#TIMBER_FAMILY` `TimberGrade.FmkMpa` design strengths; `CmuSection.NetSection` is the EXACT ungrouted net section through the ONE `ParametricSection.Hollow` solver over the built cell voids (net Area AND net inertia AND net plastic moduli), and `CmuSection.GroutedSection` re-fills the cells by a grout fraction for the TMS 402 partially-grouted member the `Connection/reinforcement#RC_SECTION` reinforced-masonry consumer reads — the gross-minus-cell-area scalar `SolidFraction` survives only as the coarse `Coring`-bucket ratio; the fire-resistance rating is the ACI 216.1 equivalent-thickness method (the net solid volume over the exposed face area, the rating an aggregate-dependent step the `CmuAggregate` carries), and the self-weight the `CmuDensity` oven-dry density over the net solid plus grout, both realized receipts the thermal/fire/structural seam reads off the section rather than a Pset string; `CmuSection.ToUnit` is the ONE bridge from the cell/face-shell vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes, and `CmuSection.ToCoring` maps the net-area solid fraction to the `masonry#PROFILE_FAMILY` `Coring` void class so the cmu unit shares the masonry course algebra; a CMU run is a `LayerSet`/`ProfileSet` assignment along the `RunPath` extruded through one `Profile`, never a masonry-special-case; the `CmuGrade`/`CmuStrength` carry the load-bearing + design-strength discriminants the design seam reads, and the section maps to the `IfcRectangleProfileDef` rectangle outer face on the wire (a hollow unit's cell voids the `IfcRectangleHollowProfileDef` true-net option or the `Coring` void fraction, a grouted unit the solid rectangle); `ProfileCatalogue.BuildCmuRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the ASTM C90 rows keyed `cmu.<designation>`, the realized cross-section grounded in the published ASTM C90 dimensional values and the TMS 602 strength table.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                                   // Fin, Option, Seq
using Rasm.Domain;                                   // PositiveMagnitude, UnitInterval, Context, Op
using Rasm.Element;                                  // MaterialId (the seam appearance handle BuildCmuRows assigns)
using Thinktecture;                                  // [SmartEnum]/KeyMemberEqualityComparer, ComparerAccessors
using Op = Rasm.Domain.Op;
using Rasm.Materials.Profiles;                       // Profile/ProfileUnit/ProfileStandard/ProfileId/ProfileFault/ProfileFamily/ComputedSection/ParametricSection (the parent PROFILE_OWNER)
using Rasm.Materials.Profiles.Masonry;               // Coring/MortarType (the masonry#PROFILE_FAMILY void-class + mortar-band this cmu family composes)
using static LanguageExt.Prelude;                    // toSeq, Some, None

// Each family page is its OWN Rasm.Materials.Profiles.<Family> sub-namespace so the six sibling `ProfileCatalogue`
// static classes are distinct types (one shared namespace would be a CS0101 collision); profile#PROFILE_OWNER stays the
// parent Rasm.Materials.Profiles and folds Cmu.ProfileCatalogue.BuildCmuRows / Cmu.ProfileCatalogue.CmuSections by the
// sub-namespace-qualified name. The parent owner types and the masonry Coring/MortarType are composed via the usings above.
namespace Rasm.Materials.Profiles.Cmu;

// --- [TYPES] -------------------------------------------------------------------------------
// The ASTM C90 unit grade: the hollow/solid void form crossed with the load-bearing classification, the IFC profile
// the grade maps to (a hollow unit the true-net IfcRectangleHollowProfileDef, a solid/grouted unit the outer
// IfcRectangleProfileDef rectangle). A new grade (e.g. an open-end bond-beam unit) is one case, never a per-block type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade HollowLoadBearing    = new("hollow-load-bearing",     loadBearing: true,  hollow: true,  ifcSubtype: "IfcRectangleHollowProfileDef");
    public static readonly CmuGrade HollowNonLoadBearing = new("hollow-non-load-bearing", loadBearing: false, hollow: true,  ifcSubtype: "IfcRectangleHollowProfileDef");
    public static readonly CmuGrade SolidLoadBearing     = new("solid-load-bearing",      loadBearing: true,  hollow: false, ifcSubtype: "IfcRectangleProfileDef");
    public static readonly CmuGrade SolidNonLoadBearing  = new("solid-non-load-bearing",  loadBearing: false, hollow: false, ifcSubtype: "IfcRectangleProfileDef");
    public bool LoadBearing { get; }
    public bool Hollow { get; }
    public string IfcSubtype { get; }
}

// The TMS 602 specified-masonry-strength class: the structural-design input f'm (the masonry assemblage strength, NOT
// the unit strength), with the published net-area UNIT strength the unit-strength method requires per mortar type so a
// CMU's design strength is the registered DATA, never a caller-supplied raw double. The FmMpa property IS the specified
// f'm; the static Resolve inverts the TMS 602 Table 2 unit-strength method (unit net-area strength + mortar type → f'm).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuStrength {
    public static readonly CmuStrength F1350 = new("f1350", fmMpa:  9.31, netUnitTypeMnMpa: 10.34, netUnitTypeSMpa: 12.41);
    public static readonly CmuStrength F1500 = new("f1500", fmMpa: 10.34, netUnitTypeMnMpa: 12.07, netUnitTypeSMpa: 13.79);
    public static readonly CmuStrength F2000 = new("f2000", fmMpa: 13.79, netUnitTypeMnMpa: 17.24, netUnitTypeSMpa: 18.96);
    public static readonly CmuStrength F2500 = new("f2500", fmMpa: 17.24, netUnitTypeMnMpa: 22.41, netUnitTypeSMpa: 24.13);
    public static readonly CmuStrength F3000 = new("f3000", fmMpa: 20.69, netUnitTypeMnMpa: 27.58, netUnitTypeSMpa: 31.03);
    public double FmMpa { get; }              // the specified masonry compressive strength f'm (MPa)
    public double NetUnitTypeMnMpa { get; }   // the net-area unit strength required with Type M or N mortar (MPa)
    public double NetUnitTypeSMpa { get; }    // the net-area unit strength required with Type S mortar (MPa)

    // The TMS 602 unit-strength method, inverted: the HIGHEST f'm class whose required net-area unit strength (by the
    // grout/mortar's MortarType band) the supplied unit net-area strength meets — a stronger unit qualifies for MORE
    // classes and is assigned the richest, so a stronger unit + stronger mortar yields a higher f'm. The descending-by-
    // f'm scan's FIRST passing row IS that ceiling. MortarType M/S/N gate the column (the table tabulates M-or-N vs S);
    // a unit below F1350's Type-M/N floor satisfies no row and rails None (the caller's PrismStrength stays the
    // registered grade rather than a fabricated below-floor class).
    public static Option<CmuStrength> Resolve(double netUnitStrengthMpa, MortarType mortar) {
        bool typeS = mortar == MortarType.S;
        return toSeq(Items.OrderByDescending(static c => c.FmMpa))
            .Filter(c => netUnitStrengthMpa >= (typeS ? c.NetUnitTypeSMpa : c.NetUnitTypeMnMpa))
            .Head;
    }
}

// The ASTM C90 oven-dry-density classification (the published 1360–2320 kg/m³ unit range banded at 1680 / 2000): the
// self-weight and the thermal-mass driver, and the lightweight bias the fire equivalent-thickness rating reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuDensity {
    public static readonly CmuDensity Lightweight = new("lightweight", ovenDryKgPerM3: 1600.0);   // < 1680 kg/m³ (105 pcf)
    public static readonly CmuDensity Medium      = new("medium",      ovenDryKgPerM3: 1840.0);   // 1680–2000 kg/m³
    public static readonly CmuDensity Normal      = new("normal",      ovenDryKgPerM3: 2160.0);   // ≥ 2000 kg/m³ (125 pcf)
    public double OvenDryKgPerM3 { get; }
}

// The ACI 216.1 aggregate type the fire equivalent-thickness → rating step reads: a lightweight/expanded aggregate
// reaches a given fire rating at a SMALLER equivalent thickness than a siliceous/calcareous normal-weight aggregate,
// so the same equivalent thickness yields a higher rating for a lightweight aggregate. RatingFactor scales the
// equivalent thickness onto the ACI 216.1 rating curve (the per-aggregate required-thickness-for-1-hour reciprocal).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate Siliceous       = new("siliceous",        ratingFactor: 0.0157);   // siliceous gravel — least fire-efficient
    public static readonly CmuAggregate Calcareous      = new("calcareous",       ratingFactor: 0.0172);   // limestone / calcareous gravel
    public static readonly CmuAggregate NormalWeight    = new("normal-weight",    ratingFactor: 0.0172);   // normal-weight blend
    public static readonly CmuAggregate ExpandedSlag    = new("expanded-slag",    ratingFactor: 0.0212);   // air-cooled / expanded slag
    public static readonly CmuAggregate ExpandedClay    = new("expanded-clay",    ratingFactor: 0.0212);   // expanded clay / shale / slate
    public static readonly CmuAggregate LightweightBlend = new("lightweight-blend", ratingFactor: 0.0259);  // pumice / expanded — most fire-efficient
    public double RatingFactor { get; }      // fire-resistance hours per mm equivalent thickness (ACI 216.1 curve slope)
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ASTM C90 unit section: the outer L×W×H module, the face-shell/web thicknesses, the cell count, the bed joint,
// the typed grade/strength/density/aggregate receipts, and the grout fraction (0 ungrouted, 1 fully solid-grouted, a
// partial value for a TMS 402 partially-grouted wall). Every fractional-millimeter column rides the kernel
// PositiveMagnitude; GroutFraction rides the kernel UnitInterval so a malformed fraction rails the value-object Fin.
public readonly record struct CmuSection(
    CmuGrade Grade,
    CmuStrength Strength,
    CmuDensity Density,
    CmuAggregate Aggregate,
    PositiveMagnitude ActualWidthMm,
    PositiveMagnitude ActualHeightMm,
    PositiveMagnitude ActualLengthMm,
    PositiveMagnitude FaceShellMm,
    PositiveMagnitude WebThicknessMm,
    int CellCount,
    double BedJointMm,
    UnitInterval GroutFraction) {

    public double GrossAreaMm2 => ActualWidthMm.Value * ActualLengthMm.Value;
    public double NetAreaMm2 => Grade.Hollow
        ? GrossAreaMm2 - Math.Max(0, CellCount) * CellAreaMm2()
        : GrossAreaMm2;
    // The grouted net area: the ungrouted net solid plus the grout fraction of the cell void area — the area the
    // TMS 402 reinforced/grouted-masonry self-weight and net-section design read for a partially-grouted wall.
    public double GroutedAreaMm2 => Grade.Hollow
        ? NetAreaMm2 + GroutFraction.Value * Math.Max(0, CellCount) * CellAreaMm2()
        : GrossAreaMm2;
    public double SolidFraction() => GrossAreaMm2 > 0.0 ? Math.Clamp(NetAreaMm2 / GrossAreaMm2, 0.0, 1.0) : 1.0;
    public double GroutedSolidFraction() => GrossAreaMm2 > 0.0 ? Math.Clamp(GroutedAreaMm2 / GrossAreaMm2, 0.0, 1.0) : 1.0;

    private double CellAreaMm2() {
        double cellWidth = ActualWidthMm.Value - 2.0 * FaceShellMm.Value;
        double webSpan = Math.Max(0, CellCount + 1) * WebThicknessMm.Value;
        double cellLength = (ActualLengthMm.Value - webSpan) / Math.Max(1, CellCount);
        return Math.Max(0.0, cellWidth) * Math.Max(0.0, cellLength);
    }

    // The cell voids in the horizontal L×W cross-section: CellCount equal cells separated by webs, each cell inset by
    // the face shell across the width and centred along the length, the void list profile#PROFILE_OWNER ParametricSection
    // subtracts from the outer rectangle for the EXACT net section. An UNGROUTED hollow unit emits its cells; a fully
    // solid-grouted (GroutFraction == 1) or a solid grade emits none, so the solver returns the gross rectangle.
    private Seq<(double X, double Y, double W, double H)> CellVoids(double effectiveGroutFraction) {
        if (!Grade.Hollow || CellCount <= 0 || effectiveGroutFraction >= 1.0) { return Seq<(double, double, double, double)>(); }
        double cellWid = Math.Max(0.0, ActualWidthMm.Value - 2.0 * FaceShellMm.Value);
        double cellLen = Math.Max(0.0, (ActualLengthMm.Value - (CellCount + 1) * WebThicknessMm.Value) / CellCount);
        double pitch = cellLen + WebThicknessMm.Value;
        // A partial grout fraction shrinks every cell void homogeneously by scale = sqrt(1 - f) so the REMAINING void
        // AREA is exactly (1 - f) of the ungrouted cells (area-exact: W·s·H·s = (1-f)·W·H) and the net section is
        // monotone in f — exact at f=0 (the ungrouted hollow) and f=1 (CellVoids empties to the solid rectangle), an
        // area-honest approximation between (the void's net-inertia contribution scales s⁴=(1-f)², bounded by the two
        // limits but not the exact transformed-section inertia the TMS 402 reinforced design re-derives over the grout
        // modulus). This is the geometric net-area-equivalent the gross-rectangle ParametricSection.Hollow subtracts.
        double scale = Math.Sqrt(Math.Max(0.0, 1.0 - effectiveGroutFraction));
        return toSeq(Enumerable.Range(0, CellCount))
            .Map(i => (X: -ActualLengthMm.Value / 2 + WebThicknessMm.Value + cellLen / 2 + i * pitch, Y: 0.0, W: cellLen * scale, H: cellWid * scale));
    }

    // The coarse masonry#PROFILE_FAMILY Coring void-class bucket (the exact net section is NetSection/GroutedSection): a
    // solid-fraction band, the low-solid hollow bucket discriminating the CELL COUNT so the 12-in 3-cell unit reads the
    // faithful Hollow3Cell row rather than forcing onto the 2-cell label (CellCount is the unit's own column, not a
    // re-derived geometry), the shared clay/concrete-masonry void vocabulary covering both.
    public Coring ToCoring() => SolidFraction() switch {
        >= 0.95 => Coring.None,
        >= 0.75 => Coring.Cored3Hole,
        >= 0.60 => Coring.Perforated10Cell,
        _       => CellCount >= 3 ? Coring.Hollow3Cell : Coring.Hollow2Cell,
    };

    // The EXACT ungrouted net section the structural design seam reads — net Area AND net moment-of-inertia AND net
    // plastic/torsion/shear columns with the cells subtracted, not the gross-minus-cell-area scalar SolidFraction
    // approximates; the ONE profile#PROFILE_OWNER ParametricSection.Hollow solver runs over the built hollow Perimeter.
    public Fin<ComputedSection> NetSection(Op key) =>
        ParametricSection.Hollow(ActualLengthMm.Value, ActualWidthMm.Value, CellVoids(0.0), key);

    // The partially/fully-grouted net section a TMS 402 reinforced-masonry consumer reads — the cells re-filled by the
    // section's own GroutFraction (overridden by the caller's request for an as-built fraction), so a fully-grouted
    // wall integrates as the solid rectangle and a partially-grouted wall as the interpolated net section; the
    // Connection/reinforcement#RC_SECTION owner adds the reinforcement transform over this grouted concrete outline.
    public Fin<ComputedSection> GroutedSection(double groutFraction, Op key) =>
        ParametricSection.Hollow(ActualLengthMm.Value, ActualWidthMm.Value, CellVoids(Math.Clamp(double.IsFinite(groutFraction) ? groutFraction : GroutFraction.Value, 0.0, 1.0)), key);

    // The TMS 602 net-area-strength receipt the masonry compressive-utilisation rail reads — the section's specified
    // masonry strength f'm class, the structural-design input (NOT the unit strength), carried as typed data.
    public CmuStrength PrismStrength() => Strength;

    // The ACI 216.1 equivalent thickness (mm): the net solid volume — including the grout — divided by the exposed
    // face area (the L×H face), the single fire-design geometric parameter. A solid/fully-grouted unit's equivalent
    // thickness IS its actual width; a hollow unit's is reduced by the void fraction.
    public double EquivalentThicknessMm() => ActualWidthMm.Value * GroutedSolidFraction();

    // The ACI 216.1 fire-resistance rating (hours) from the equivalent thickness and the aggregate type: the rating
    // is the equivalent thickness scaled by the per-aggregate CmuAggregate.RatingFactor (a lightweight aggregate
    // reaches a rating at a smaller thickness), capped at 4 hours (the ACI 216.1 tabulated maximum).
    public double FireRatingHours() => Math.Clamp(EquivalentThicknessMm() * Aggregate.RatingFactor, 0.0, 4.0);

    // The wall self-weight (kN/m²): the oven-dry density over the grouted net solid through the unit height projected
    // onto the L×H face — the dead-load receipt the structural/seismic seam reads, heavier for a normal-weight grouted
    // unit, lightest for a hollow lightweight unit. The grout fills the grouted cell fraction at a normal-weight
    // 2240 kg/m³; mass(kg) = solid-area(mm²)·density(kg/m³)·height(mm)·1e-9, weight(kN) = mass·g/1000, pressure =
    // weight / face(m²). The 1e-9 carries mm²·mm → m³ so area·density·height·1e-9 is the unit solid mass in kilograms.
    public double SelfWeightKnPerM2() {
        double cellVoidMm2 = Math.Max(0, CellCount) * CellAreaMm2();
        double unitMassKg = (NetAreaMm2 * Density.OvenDryKgPerM3 + GroutFraction.Value * cellVoidMm2 * 2240.0) * ActualHeightMm.Value * 1e-9;
        double faceAreaM2 = ActualLengthMm.Value * ActualHeightMm.Value * 1e-6;
        return faceAreaM2 > 0.0 ? unitMassKg * 9.80665 / 1000.0 / faceAreaM2 : 0.0;
    }

    public Fin<ProfileUnit> ToUnit(Context context, Op key) =>
        ProfileUnit.Of(ActualWidthMm.Value, ActualHeightMm.Value, ActualLengthMm.Value, ActualHeightMm.Value + BedJointMm, context, key);
}

// The raw catalogue row: plain doubles + the typed-axis keys, admitted ONCE through CmuOf into the kernel value-objects
// and the closed CmuGrade/CmuStrength/CmuDensity/CmuAggregate axes so a non-positive dimension or an unknown axis key
// drops the row through Choose rather than seeding a degenerate Profile. GroutFraction defaults 0 (an ungrouted unit).
public readonly record struct CmuRow(
    string Designation, string Grade, string Strength, string Density, string Aggregate,
    double WMm, double HMm, double LMm, double FaceShellMm, double WebMm, int Cells, double GroutFraction = 0.0);

public sealed record CmuShape(ProfileId Id, CmuSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    // The bounded standards body is ProfileAuthority.Astm (the ASTM C90 unit-dimension authority, region "us"); the
    // TMS 602 strength method is the CmuStrength net-area-strength DATA, not a ProfileStandard column.
    static readonly ProfileStandard AstmC90 = new("us", StandardJointThicknessMm: 9.5, Authority: ProfileAuthority.Astm);

    // The ASTM C90 nominal-width set: 4/6/8/10/12-inch hollow load-bearing units (actual width 90/140/190/240/290 mm,
    // the 190×390 mm 8-inch module), the minimum face-shell by width (19 mm ≤4 in, 25 mm at 6 in, 32 mm ≥8 in), the
    // 19 mm minimum web, the 4/8-inch solid units, the architectural splitface, the half-high unit, and an 8-inch
    // fully-grouted reinforced unit. A new finish or metric A-series unit is one further row, never a new type.
    static readonly Seq<CmuRow> AstmRows = Seq(
        new CmuRow("cmu.4in-hollow",        "hollow-load-bearing",     "f1350", "normal",      "normal-weight", 90.0,  190.0, 390.0, 19.0, 19.0, 2),
        new CmuRow("cmu.6in-hollow",        "hollow-load-bearing",     "f1500", "normal",      "normal-weight", 140.0, 190.0, 390.0, 25.0, 19.0, 2),
        new CmuRow("cmu.8in-hollow",        "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 19.0, 2),
        new CmuRow("cmu.8in-hollow-lw",     "hollow-load-bearing",     "f1500", "lightweight", "expanded-clay", 190.0, 190.0, 390.0, 32.0, 19.0, 2),
        new CmuRow("cmu.10in-hollow",       "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 240.0, 190.0, 390.0, 32.0, 19.0, 2),
        new CmuRow("cmu.12in-hollow",       "hollow-load-bearing",     "f2500", "normal",      "normal-weight", 290.0, 190.0, 390.0, 32.0, 19.0, 3),
        new CmuRow("cmu.4in-solid",         "solid-load-bearing",      "f1350", "normal",      "normal-weight", 90.0,  190.0, 390.0, 45.0, 19.0, 0),
        new CmuRow("cmu.8in-solid",         "solid-load-bearing",      "f2500", "normal",      "calcareous",    190.0, 190.0, 390.0, 95.0, 19.0, 0),
        new CmuRow("cmu.8in-grouted",       "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 19.0, 2, 1.0),
        new CmuRow("cmu.8in-splitface",     "hollow-load-bearing",     "f2000", "medium",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 19.0, 2),
        new CmuRow("cmu.8in-halfhigh",      "hollow-non-load-bearing", "f1350", "lightweight", "expanded-slag", 190.0, 90.0,  390.0, 32.0, 19.0, 2));

    static Fin<CmuShape> CmuOf(CmuRow r, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: r.HMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: r.LMm)
        from fs in key.AcceptValidated<PositiveMagnitude>(candidate: r.FaceShellMm)
        from web in key.AcceptValidated<PositiveMagnitude>(candidate: r.WebMm)
        from grout in key.AcceptValidated<UnitInterval>(candidate: r.GroutFraction)
        from grade in CmuGrade.TryGet(r.Grade, out CmuGrade? g) ? Fin.Succ(g!) : Fin.Fail<CmuGrade>(ProfileFault.Family(key, $"<unknown-cmu-grade:{r.Grade}>"))
        from strength in CmuStrength.TryGet(r.Strength, out CmuStrength? s) ? Fin.Succ(s!) : Fin.Fail<CmuStrength>(ProfileFault.Family(key, $"<unknown-cmu-strength:{r.Strength}>"))
        from density in CmuDensity.TryGet(r.Density, out CmuDensity? d) ? Fin.Succ(d!) : Fin.Fail<CmuDensity>(ProfileFault.Family(key, $"<unknown-cmu-density:{r.Density}>"))
        from aggregate in CmuAggregate.TryGet(r.Aggregate, out CmuAggregate? a) ? Fin.Succ(a!) : Fin.Fail<CmuAggregate>(ProfileFault.Family(key, $"<unknown-cmu-aggregate:{r.Aggregate}>"))
        select new CmuShape(ProfileId.Of(r.Designation), new CmuSection(grade, strength, density, aggregate, w, h, l, fs, web, r.Cells, AstmC90.StandardJointThicknessMm, grout), AstmC90);

    // Every realized CMU unit resolved to its CmuShape ONCE — the seed BuildCmuRows and CmuSections both derive from
    // (DERIVED_LOGIC, one source), the SAME shape steel#STEEL_FAMILY Shapes / timber#TIMBER_FAMILY Shapes take. A
    // malformed row (a non-positive dimension, an unknown axis key) drops through Choose rather than seeding a partial.
    static readonly Seq<CmuShape> Shapes =
        AstmRows.Choose(row => CmuOf(row, default, default).ToOption());

    public static FrozenDictionary<ProfileId, Profile> BuildCmuRows(Context context) =>
        Shapes
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Cmu, unit, shape.Section.ToCoring(), shape.Standard, MaterialId.Of("concrete.cmu")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);

    // The ProfileId → ComputedSection map the profile#PROFILE_OWNER ProfileCatalogue.Sections folds and the
    // [03]-[PROFILE_RESOLUTION] ProfileResolution.Build caches by ProfileRef (the realized sibling to steel#STEEL_FAMILY
    // SteelSections / timber#TIMBER_FAMILY TimberSections) — CMU is a profiled structural family, so a Rasm.Compute
    // MasonryCompression check reads the unit's net section off the seam without re-running the integral. The cached
    // section is the AS-BUILT GroutedSection over the row's own GroutFraction (a hollow row its exact net section, the
    // fully-grouted row its solid rectangle) through the shared profile#PROFILE_OWNER ParametricSection.Hollow solver;
    // a section whose integral fails drops through Choose (a build-time ProfileFault.Section surfaced in THIS page,
    // never a swallowed gap the resolver mis-reports as "unregistered").
    public static FrozenDictionary<ProfileId, ComputedSection> CmuSections(Context context) =>
        Shapes
            .Choose(shape => shape.Section.GroutedSection(shape.Section.GroutFraction.Value, default).ToOption()
                .Map(section => (shape.Id, Section: section)))
            .ToFrozenDictionary(static r => r.Id, static r => r.Section, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [CMU_ROW_TRANSCRIPTION]: REALIZED — the ASTM C90 standard carries the hollow/solid load-bearing concrete-masonry-unit grades with the actual 190×190×390 mm 8-inch module, the minimum face-shell thickness by nominal width (19 mm at 4 in, 25 mm at 6 in, 32 mm at 8 in and above; 16 mm where solid-grouted), and the 19 mm minimum web thickness; the catalogue carries the 4/6/8/10/12-inch hollow nominal widths, the 4/8-inch solid grades, the 190-mm splitface architectural unit, the 90-mm half-high unit, an 8-inch lightweight expanded-clay unit, and an 8-inch fully-grouted unit, the standard nominal-width set keyed `cmu.<designation>`, a new architectural finish (ground-face, scored, ribbed) or metric A-series unit one further `CmuRow` data addition, never a new type. The raw `CmuRow` carries plain doubles plus the typed-axis keys and admits once through `CmuOf` into the kernel value-objects (`PositiveMagnitude` length columns, `UnitInterval` grout fraction) and the closed `CmuGrade`/`CmuStrength`/`CmuDensity`/`CmuAggregate` axes, a non-positive dimension or an unknown axis key dropping the row through `Choose` rather than seeding a degenerate `Profile`.
- [CMU_STRENGTH_GRADE]: REALIZED — the structural-design input is the typed `CmuStrength` `[SmartEnum]` (the TMS 602 specified-masonry-strength classes f1350/f1500/f2000/f2500/f3000 carrying the assemblage strength `f'm` in MPa and the published net-area UNIT strength the unit-strength method requires per `MortarType` band), the sibling of the `steel#STEEL_FAMILY` `SteelGrade.YieldMpa` and the `timber#TIMBER_FAMILY` `TimberGrade.FmkMpa` design strengths — so a CMU member's design strength is the registered grade DATA, never a caller-supplied raw double. `CmuStrength.Resolve(netUnitStrengthMpa, mortar)` inverts the TMS 602 Table 2 unit-strength method (the HIGHEST `f'm` class whose required net-area unit strength the supplied unit meets — a stronger unit qualifies for more classes and takes the richest, the Type-S column distinct from the Type-M/N column), so a stronger unit + stronger mortar yields a higher `f'm`. The `CmuSection.PrismStrength` receipt feeds the `Profiles/capacity#SECTION_CAPACITY` masonry compressive utilisation through the REALIZED `SectionCapacityResolver.MasonryCompression(double fmMpa, ComputedSection section, double slendernessReduction)` lift onto the realized `SectionCapacity.MasonryCompression` case (the TMS 402 axial-flexural `max(P/φPn, M/φMn)` unreinforced/reinforced check `f'm` + the grouted net `ComputedSection` area/modulus scale), the realized design input the prior naive page entirely lacked. The TMS 402 §9.3.4.1.1 member-stability slenderness reduction `R = [1 - (h/140r)²]` is passed IN as the `slendernessReduction` argument by the `Rasm.Compute`/cmu CALLER — this owner is a unit-section vocabulary and NEVER computes member height `h` (the wall slenderness is a placement-level input, not a `CmuSection` column), so `PrismStrength().FmMpa` + a `GroutedSection(GroutFraction)` + the caller's `R` close the lift. Ripple counterpart: `Profiles/capacity` `[SECTION_CAPACITY]` (the realized `MasonryCompression` case + `SectionCapacityResolver.MasonryCompression` lift the `f'm` + grouted net section feed) and `masonry#PROFILE_FAMILY` `MortarType` (the unit-strength-method mortar band).
- [CMU_DENSITY_FIRE_MASS]: REALIZED — the `CmuDensity` `[SmartEnum]` (ASTM C90 lightweight <1680 / medium 1680–2000 / normal ≥2000 kg/m³ oven-dry density, the published 1360–2320 kg/m³ unit range banded) and the `CmuAggregate` `[SmartEnum]` (the ACI 216.1 fire equivalent-thickness aggregate types — siliceous/calcareous normal-weight through expanded-slag/clay/lightweight, each carrying its rating-curve slope) realize the two physical receipts the prior page omitted: `CmuSection.SelfWeightKnPerM2` is the wall dead load (the oven-dry density over the grouted net solid through the unit height — heaviest for a normal-weight grouted unit, lightest for a hollow lightweight unit), and `CmuSection.FireRatingHours` is the ACI 216.1 equivalent-thickness fire-resistance rating (`EquivalentThicknessMm` the net solid volume over the exposed face area, the rating that thickness scaled by the per-aggregate `RatingFactor` so a lightweight aggregate reaches a rating at a smaller thickness, capped at the 4-hour tabulated maximum). Both are realized receipts the thermal/fire/structural seam reads off the section, never a Pset string — the typical 8-inch lightweight hollow unit reads its ~2-hour rating and the normal-weight grouted unit its higher self-weight directly. The remaining multi-cell steady-state R-value is a `CmuSection` thermal-resistance column growth over the same cell geometry, never a parallel owner.
- [CMU_GROUT_REINFORCEMENT]: REALIZED — the `CmuSection.GroutFraction` (`UnitInterval`, 0 ungrouted, 1 fully solid-grouted, a partial value for a TMS 402 partially-grouted wall) and the `GroutedSection`/`GroutedAreaMm2` projections realize the dominant structural CMU form the prior page omitted: a reinforced-masonry shear wall or pilaster is grouted at the reinforced cells, and `GroutedSection(groutFraction, key)` re-fills the cell voids by the as-built fraction (the `CellVoids` homogeneous void-shrink interpolating the ungrouted-hollow and fully-grouted net-inertia bounds) so the `Connection/reinforcement#RC_SECTION` reinforced-masonry consumer adds the steel transform over the grouted concrete outline through the SAME `ParametricSection.Hollow` solver, never a per-cell literal. A fully-grouted unit (`GroutFraction == 1`) emits no cell voids and integrates as the solid rectangle; the `SelfWeightKnPerM2` adds the grout mass over the grouted cell fraction. Ripple counterpart: `Connection/reinforcement` `[RC_SECTION]` (the grouted-concrete + reinforcement transformed section the `GroutedSection` outline feeds) and `Profiles/capacity` `[SECTION_CAPACITY]` (the grouted net section the TMS 402 reinforced-masonry capacity reads).
- [IFCPROFILEDEF_CMU_ALIGNMENT]: the cmu family maps to the `IfcRectangleProfileDef` rectangle subtype for a solid/grouted unit and the `IfcRectangleHollowProfileDef` (the rectangular hollow section, the true single-cell net) or the outer rectangle plus the `Coring` void fraction for a hollow unit — the `CmuGrade.IfcSubtype` carries the spelling, the outer 190×390 mm face the `XDim`/`YDim`. A multi-cell unit's exact voids are the `CmuSection.NetSection` `ParametricSection.Hollow` concern (read at the structural seam), never a per-cell `IfcArbitraryProfileDefWithVoids` on the wire; a CMU member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` carrying the rectangle/hollow profile plus the `Coring` receipt, the splitface/ground-face finish a surface-style on the element rather than a profile variant, the grout state a `Pset_ConcreteElementGeneral`-class property the `Rasm.Bim` egress derives from the `GroutFraction`. The probe is the per-finish surface-style mapping at the `Rasm.Bim` boundary, the rectangle/hollow profile the realized base case.
- [CMU_CELL_GEOMETRY]: REALIZED — the exact net section is the `CmuSection.NetSection` (ungrouted) and `GroutedSection` (grouted) projections composing the shared `profile#PROFILE_OWNER` `ParametricSection.Hollow` solver over the `CellVoids` cell rectangles (the `VividOrange.Sections.SectionProperties` Green's-theorem integral subtracting the cells from BOTH the area AND the second moment, the `ParametricSection.HollowPlastics` superposition yielding the net plastic/torsion/shear columns), so the structural design seam reads the true net `Area`/`MomentOfInertiaYy`/`Zz`/`Zx`/`Zy`/`J` rather than the gross-minus-cell-area scalar the `SolidFraction` approximation yields — the `NetAreaMm2`/`CellAreaMm2` fast path survives only as the coarse `Coring`-bucket ratio the `ToCoring` reads (its bands corrected for CMU's 2-cell/3-cell geometry: solid fraction ≥0.95 `None`, ≥0.75 `Cored3Hole`, ≥0.60 `Perforated10Cell`, and the low-solid hollow bucket discriminating the `CellCount` so the 8-in 2-cell unit reads `Hollow2Cell` and the 12-in 3-cell unit reads the faithful `masonry#PROFILE_FAMILY` `Hollow3Cell` row rather than forcing onto the 2-cell label — the shared clay/concrete-masonry void vocabulary covering both). The cell voids are computed from the face-shell/web/cell-count columns (`CellVoids`: `CellCount` equal cells inset by the face shell, separated by webs, centred along the length, homogeneously shrunk by the grout fraction), never a per-cell profile literal; a precise per-cell draft taper is a `CellVoids` column growth, never a parallel section owner. The realized `ProfileCatalogue.CmuSections` `FrozenDictionary<ProfileId, ComputedSection>` is the M7 section-map contribution `profile#PROFILE_OWNER` `ProfileCatalogue.Sections` requires (the sibling to `steel#STEEL_FAMILY` `SteelSections` and `timber#TIMBER_FAMILY` `TimberSections`) — it caches each unit's AS-BUILT `GroutedSection(GroutFraction)` once over the one `Shapes` seed (a hollow row its exact net section, the fully-grouted row its solid rectangle), so the `[03]-[PROFILE_RESOLUTION]` `ProfileResolution.Build` joins a CMU `ProfileRef` to `Some(section)` and the `Profiles/capacity#SECTION_CAPACITY` `MasonryCompression` case reads the grouted net area/modulus off the seam without re-running the `ParametricSection.Hollow` integral per call; a section whose integral fails drops through `Choose` as a build-time `ProfileFault.Section` surfaced here, never the silent-drop the resolver would mis-report as "unregistered".
