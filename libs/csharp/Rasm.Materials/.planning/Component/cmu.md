# [MATERIALS_CMU]

THE CMU COMPONENTFAMILY GROUNDED IN ASTM C90 + TMS 602-16/-22 + ACI 216.1. The concrete-masonry-unit cross-section vocabulary — the ASTM C90 cell / face-shell / end-web / cross-web dimensional columns, the per-cell void/grout/reinforcement lattice, the demold draft + face-shell flare + special-unit + finish generative geometry, the grade / strength-class / density-class / aggregate-type discriminants, the exact per-cell net section, the grouted-cell augmentation, and the equivalent-thickness fire / isothermal-planes thermal-mass / self-weight receipts — is the cross-section a `component#COMPONENT_OWNER` `Component` carries in its `ComponentSection.Cmu` arm. A concrete block is a `Component` row of `ComponentClass.Minor` (`IfcElementComponent`: one standardized Type, many laid pieces), never a `ConcreteBlock` type: the per-cell geometry, the face-shell/end-web/cross-web columns, the void class, the ASTM C90 grade, the TMS 602 net-area strength class, the density class, the aggregate type, and the regional standard are cmu-`CmuSection` columns, and the `CmuSection.ToUnit` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry family drives — a CMU run is the same station-stepped course fold over one `Component`, never a per-family layout. The cmu vocabulary grows by DATA: a new unit is one `CmuRow` catalogue row, a new grade one `CmuGrade` case, a new strength one `CmuStrength` case, a new density one `CmuDensity` case, a new special unit one `CmuSpecialUnit` case, a new finish one `CmuFinish` case — never a per-block type. The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentUnit`/`ComponentStandard`/`ComponentSection`/`ComputedSection` shape, the shared `ParametricSection.Hollow` solver, and the relocated `Coring` void-class vocabulary; `masonry#MASONRY_FAMILY` `MortarType` for the unit-strength mortar band; `reinforcement#RC_SECTION` for the reinforced-grouted-cell transform; `capacity#SECTION_CAPACITY` for the masonry compressive-utilisation rail; and the `Rasm.Vectors` kernel `PositiveMagnitude` for every length column. Timber/glazing/steel land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[CMU_FAMILY]: the `CmuGrade` ASTM C90 unit-grade discriminant, the `CmuStrength` TMS 602-16/-22 Table 2 net-area-strength / `f'm` axis, the `CmuDensity` oven-dry-density + thermal-conductivity axis, the `CmuAggregate` ACI 216.1 fire-rating aggregate axis, the `CmuSpecialUnit` bond-beam/channel/lintel/sash/control-joint/open-end geometry axis, the `CmuFinish` split-face/scored/ribbed/ground surface axis, the `CmuCell` per-cell void/grout/rebar lattice, the `CmuSection` cell/face-shell/web record (its `NetSection`/`GroutedSection` exact per-cell solver, `EquivalentThicknessMm`/`FireRatingHours` ACI 216.1 power-law fire receipt, `ThermalResistanceM2KPerW` NCMA TEK 6-2C isothermal-planes receipt, `SelfWeightKnPerM2` mass receipt, `GroutedCellFraction`/`ToUnit`/`ToCoring` projections), the `CmuStrength.Resolve` TMS 602 unit-strength-method resolution, and the `ComponentCatalogue.BuildCmuRows` ASTM C90 row table plus the `ComponentCatalogue.CmuSections` `ComponentId`→`ComputedSection` map the `component#COMPONENT_OWNER` `ComponentCatalogue.Sections` fold + the `component#COMPONENT_RESOLUTION` M7 cache read.

## [02]-[CMU_FAMILY]

- Owner: the cmu unit vocabulary (`CmuGrade` the hollow/solid + load-bearing discriminant and IFC profile-subtype carrier, `CmuStrength` the TMS 602-16/-22 Table 2 net-area compressive-strength class resolving the specified masonry strength `f'm`, `CmuDensity` the lightweight/medium/normal oven-dry-density + thermal-conductivity class, `CmuAggregate` the ACI 216.1 aggregate type the fire power-law reads, `CmuSpecialUnit` the bond-beam/channel/lintel/sash/control-joint/open-end molding geometry, `CmuFinish` the split-face/scored/ribbed/ground surface geometry, `CmuCell` the per-cell void/grout/rebar descriptor, `CmuSection` the ASTM C90 cell/face-shell/web record); `ComponentCatalogue.BuildCmuRows` the registered-row seed `component#COMPONENT_OWNER` composes and `ComponentCatalogue.CmuSections` the `ComponentId`→`ComputedSection` section map `component#COMPONENT_OWNER` `ComponentCatalogue.Sections` folds (the realized sibling to `steel#STEEL_FAMILY` `SteelSections` / `timber#TIMBER_FAMILY` `TimberSections`), both deriving from the one `Shapes` seed; the `CmuSection.ToUnit` projection bridging a section to the canonical `ComponentUnit`.
- Cases: grade {hollow-load-bearing, hollow-non-load-bearing, solid-load-bearing, solid-non-load-bearing} — the ASTM C90 unit-grade set; strength {f2000, f2250, f2500, f2750, f3000} — the TMS 602-16/-22 Table 2 specified-masonry-strength classes the net-area unit strength resolves (`f'm` 2000–3000 psi, the standard published rows); density {lightweight (<1680 kg/m³), medium (1680–2000), normal (≥2000)} — the ASTM C90 oven-dry-density set; aggregate {siliceous, calcareous, normal-weight, expanded-clay, expanded-slag, lightweight-blend} — the ACI 216.1 / IBC 722.3.2 four fire categories over six rows; special {standard, bond-beam, knockout, channel, lintel, sash, control-joint, open-end}; finish {precision, split-face, scored, ground, ribbed}; a section is a `CmuSection` row over these axes plus its per-cell `CmuCell` lattice, never a section subtype.
- Entry: `public Fin<ComponentUnit> ToUnit(Context context, Op key)` on `CmuSection` — the section→`ComponentUnit` projection (`WidthMm` = actual unit width, `HeightMm` = actual unit height, `CourseHeightMm` = actual unit height plus the bed joint, `LengthMm` = actual unit length) so a CMU member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public Fin<ComputedSection> NetSection(Op key)` the EXACT ungrouted net cross-section (the shared `component#COMPONENT_OWNER` `ParametricSection.Hollow` solver over EVERY cell footprint returning net Area AND net inertia AND net plastic moduli, the design seam's true net section); `public Fin<ComputedSection> GroutedSection(Op key)` the as-built partially/fully-grouted net section the TMS 402 reinforced-masonry consumer reads (only the UNGROUTED cells emit a void, a grouted cell integrating as solid, so a fully-grouted unit returns the solid rectangle); `public double SelfWeightKnPerM2()` the wall self-weight from the density-class oven-dry density over the net solid plus the per-cell grout; `public CmuStrength PrismStrength()` the TMS 602 net-area-strength receipt the masonry `capacity#SECTION_CAPACITY` `MasonryCompression` utilisation reads; `public double EquivalentThicknessMm()` the ACI 216.1 net-volume/face-area equivalent thickness; `public double FireRatingHours()` the aggregate-dependent ACI 216.1 power-law fire-resistance rating; `public double ThermalResistanceM2KPerW()` the NCMA TEK 6-2C isothermal-planes multi-cell R-value; `public double GroutedCellFraction` the per-cell grout ratio; `public double SolidFraction()` the quick ungrouted net-area ratio the `Coring` bucket reads; `public Coring ToCoring()` the void-class bridge.
- Packages: Rasm.Vectors (kernel — `PositiveMagnitude` for every fractional-millimeter section column), VividOrange.Sections.SectionProperties (via the shared `component#COMPONENT_OWNER` `ParametricSection.Hollow` bridge — the hollow `Perimeter` net-section Green's-theorem integral over the per-cell voids; `.api/api-vividorange-sections-sectionproperties.md`), Thinktecture.Runtime.Extensions (`.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core, BCL inbox (`FrozenDictionary`). The cmu generative data (cell lattice, draft, flare, special units, finishes, fire power-law, isothermal-planes thermal, self-weight) is hand-rolled in-fence; ONLY the section-property integral crosses to VividOrange through `ParametricSection.Hollow`.
- Growth: the cmu vocabulary grows by data — a new ASTM C90 unit is one `CmuRow` catalogue row keyed by its nominal designation, a new grade one `CmuGrade` case, a new TMS 602 strength one `CmuStrength` row, a new density class one `CmuDensity` row, a new fire aggregate one `CmuAggregate` row, a new special unit one `CmuSpecialUnit` row, a new architectural finish one `CmuFinish` row — never a per-block type, never a per-family `Component` variant. A precise per-cell draft taper, a flared face shell, a scored/ground-face/ribbed architectural finish, or a metric A-series unit is a `CmuCell`/`CmuRow` column growth, never a parallel section owner. A timber/glazing family lands its own vocabulary on its own page the way cmu carries `CmuGrade`/`CmuSection`.
- Boundary: the cmu vocabulary is a realized `ComponentFamily.Cmu` of `ComponentClass.Minor` — a per-block class is the deleted form; `CmuSection` composes the `Rasm.Vectors` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the ASTM C90-14 minimum face-shell (19/25/32 mm by nominal width) and 19 mm minimum web thickness admitting as fractional millimeters, the web column SPLIT into the `EndWebMm` (the two unit-end webs) and the `CrossWebMm` (the interior webs between cells) per ASTM C90-14 Table 1 so the net-section web span is `2·EndWebMm + (cells−1)·CrossWebMm`, never one uniform web; the structural-design input is the typed `CmuStrength` net-area-strength receipt (the TMS 602-16/-22 Table 2 unit-strength method resolving the specified masonry strength `f'm` from the unit net-area strength and the `MortarType` band, the Type-M-or-S column distinct from the higher Type-N column), never a raw double, the sibling of the `steel#STEEL_FAMILY` `SteelGrade.YieldMpa` and the `timber#TIMBER_FAMILY` `TimberGrade.FmkMpa` design strengths; the cell void/grout/reinforcement is the per-cell `Seq<CmuCell>` lattice (each cell its own centre, width, length, demold draft, grout boolean, reinforced boolean, and rebar bar diameter) the row layout materializes from the ASTM columns, REPLACING the homogeneous void-shrink — `CmuSection.NetSection` subtracts EVERY cell footprint for the EXACT ungrouted net section through the ONE `ParametricSection.Hollow` solver, `CmuSection.GroutedSection` subtracts only the UNGROUTED cells (a grouted cell integrates as solid) for the TMS 402 partially-grouted member the `reinforcement#RC_SECTION` reinforced-masonry consumer reads, and `GroutedCellFraction` is the per-cell grout ratio derived from the lattice rather than a homogeneous smear; the fire-resistance rating is the ACI 216.1 / IBC 722.3.2 equivalent-thickness POWER LAW `R = clamp((te/cn)^1.7, 0, 4)` (the exponent 1/0.59, `cn` the per-aggregate equivalent thickness for a 1-hour rating the `CmuAggregate.EqThick1HrMm` carries), never a linear cap; the self-weight is the `CmuDensity` oven-dry density over the net solid plus the grouted-cell grout at the ASTM C476 `GroutDensityKgPerM3` 2243 kg/m³ (140 pcf); the multi-cell steady-state R is the NCMA TEK 6-2C isothermal-planes method (the two face shells in series with the core's parallel web/cell paths, the cell path conducting through grout when grouted and through the trapped-air cavity when ungrouted), all realized receipts the thermal/fire/structural seam reads off the section rather than a Pset string; the demold draft, the face-shell flare, the `CmuSpecialUnit` trough/slot, and the `CmuFinish` split/score/rib are the captured generative geometry the `Construction/layout#ASSEMBLY_FOLD` host solid extrudes and the `Rasm.Bim` surface-style egress reads, the `CmuSpecialUnit.EndWebsPresent` flag the only one the on-page cell lattice consumes (an open-end unit omits its end webs); `CmuSection.ToUnit` is the ONE bridge from the cell/face-shell vocabulary to the canonical `ComponentUnit` the `Resolve` fold consumes, and `CmuSection.ToCoring` maps the ungrouted net-area solid fraction to the relocated `component#COMPONENT_OWNER` `Coring` void class so the cmu unit shares the masonry course algebra; a CMU run is a `LayerSet`/`ProfileSet` assignment along the `RunPath` extruded through one `Component`, never a masonry-special-case; the section maps to the `CmuGrade.IfcSubtype` on the wire — a SOLID/fully-grouted unit the `IfcRectangleProfileDef` rectangle, a multi-cell HOLLOW unit the `IfcArbitraryProfileDefWithVoids` carrying the explicit per-cell voids (NOT the single-uniform-void `IfcRectangleHollowProfileDef`), the spelling resolved at the `Rasm.Bim` egress boundary; `ComponentCatalogue.BuildCmuRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue` registry with the ASTM C90 rows keyed `cmu.<designation>`, the realized cross-section grounded in the published ASTM C90 dimensional values and the TMS 602-16/-22 strength table.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                                   // Fin, Option, Seq
using Rasm.Vectors;                                  // PositiveMagnitude (every length column lives in the kernel atoms owner)
using Rasm.Domain;                                   // Context, Op, AcceptValidated
using Rasm.Element;                                  // MaterialId (the seam appearance handle BuildCmuRows assigns)
using Thinktecture;                                  // [SmartEnum]/KeyMemberEqualityComparer, ComparerAccessors, TryGet
using Op = Rasm.Domain.Op;
using Rasm.Materials.Component;                       // Component/ComponentClass/ComponentFamily/ComponentUnit/ComponentStandard/ComponentAuthority/ComponentId/ComponentFault/ComponentSection/ComputedSection/ParametricSection/Coring (the parent COMPONENT_OWNER + the relocated Coring void class)
using Rasm.Materials.Component.Masonry;              // MortarType (the masonry#MASONRY_FAMILY unit-strength mortar band CmuStrength.Resolve reads)
using static LanguageExt.Prelude;                    // toSeq, Seq

// Each family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the sibling `ComponentCatalogue`
// static classes are distinct types (one shared namespace collides as CS0101); component#COMPONENT_OWNER stays
// the parent Rasm.Materials.Component and folds Cmu.ComponentCatalogue.BuildCmuRows / Cmu.ComponentCatalogue.CmuSections
// by the sub-namespace-qualified name. The parent owner types and the masonry MortarType are composed via the usings above.
namespace Rasm.Materials.Component.Cmu;

// --- [TYPES] -------------------------------------------------------------------------------
// The ASTM C90 unit grade: the hollow/solid void form crossed with the load-bearing classification, the IFC profile
// the grade maps to. A multi-cell HOLLOW unit carries explicit per-cell voids the wire serializes as
// IfcArbitraryProfileDefWithVoids (the single-uniform-void IfcRectangleHollowProfileDef cannot represent two or three
// distinct cells); a SOLID or fully-grouted unit is the outer IfcRectangleProfileDef rectangle. A new grade is one case.
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

// The TMS 602-16/-22 Table 2 specified-masonry-strength class: the structural-design input f'm (the masonry assemblage
// strength, NOT the unit strength), with the published net-area UNIT strength the unit-strength method requires per the
// two mortar columns the table tabulates — Type M or S (the lower required unit strength) and Type N (the higher) — so a
// CMU's design strength is the registered DATA, never a caller-supplied raw double. The five rows are the standard
// published f'm classes (2000/2250/2500/2750/3000 psi). FmMpa IS the specified f'm; the Type-N cell is empty (no Type-N
// mortar reaches f'm 2750/3000), carried as +Infinity so a Type-N unit never qualifies for those classes. The static
// Resolve inverts the table (unit net-area strength + mortar type → the richest f'm class the unit qualifies for).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuStrength {
    public static readonly CmuStrength F2000 = new("f2000", fmMpa: 13.79, netUnitMsMpa: 13.79, netUnitNMpa: 18.27);
    public static readonly CmuStrength F2250 = new("f2250", fmMpa: 15.51, netUnitMsMpa: 17.93, netUnitNMpa: 23.44);
    public static readonly CmuStrength F2500 = new("f2500", fmMpa: 17.24, netUnitMsMpa: 22.41, netUnitNMpa: 28.96);
    public static readonly CmuStrength F2750 = new("f2750", fmMpa: 18.96, netUnitMsMpa: 26.89, netUnitNMpa: double.PositiveInfinity);
    public static readonly CmuStrength F3000 = new("f3000", fmMpa: 20.69, netUnitMsMpa: 31.03, netUnitNMpa: double.PositiveInfinity);
    public double FmMpa { get; }              // the specified masonry compressive strength f'm (MPa)
    public double NetUnitMsMpa { get; }       // the net-area unit strength required with Type M or S mortar (MPa)
    public double NetUnitNMpa { get; }        // the net-area unit strength required with Type N mortar (MPa); +Infinity where the column is empty

    // The required net-area unit strength for this f'm class by mortar band: Type M or S read the lower column, Type N
    // the higher, Type O/K (below the TMS 602 loadbearing-CMU floor) qualify for no class. MortarType singletons compare
    // by identity, so the band selection is the owner-vocabulary read the table column keys, never a string probe.
    public double RequiredUnitMpa(MortarType mortar) =>
        mortar == MortarType.N ? NetUnitNMpa
        : mortar == MortarType.M || mortar == MortarType.S ? NetUnitMsMpa
        : double.PositiveInfinity;

    // The TMS 602-16/-22 unit-strength method, inverted: the HIGHEST f'm class whose required net-area unit strength
    // (by the mortar band) the supplied unit net-area strength meets — a stronger unit qualifies for MORE classes and is
    // assigned the richest, so a stronger unit + stronger mortar yields a higher f'm. The descending-by-f'm scan's FIRST
    // passing row IS that ceiling. A unit below F2000's Type-M/S floor satisfies no row and rails None (the caller's
    // PrismStrength stays the registered grade rather than a fabricated below-floor class).
    public static Option<CmuStrength> Resolve(double netUnitStrengthMpa, MortarType mortar) =>
        toSeq(Items.OrderByDescending(static c => c.FmMpa))
            .Filter(c => netUnitStrengthMpa >= c.RequiredUnitMpa(mortar))
            .Head;
}

// The ASTM C90 oven-dry-density classification (the published 1360–2320 kg/m³ unit range banded at 1680 / 2000): the
// self-weight and thermal-mass driver. ConductivityWPerMK is the density-class thermal conductivity of the concrete the
// isothermal-planes R-value reads (NCMA TEK 6-2C: a lightweight aggregate concrete conducts far less than a normal-weight).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuDensity {
    public static readonly CmuDensity Lightweight = new("lightweight", ovenDryKgPerM3: 1600.0, conductivityWPerMK: 0.50);   // < 1680 kg/m³ (105 pcf)
    public static readonly CmuDensity Medium      = new("medium",      ovenDryKgPerM3: 1840.0, conductivityWPerMK: 0.90);   // 1680–2000 kg/m³
    public static readonly CmuDensity Normal      = new("normal",      ovenDryKgPerM3: 2160.0, conductivityWPerMK: 1.40);   // ≥ 2000 kg/m³ (125 pcf)
    public double OvenDryKgPerM3 { get; }
    public double ConductivityWPerMK { get; }
}

// The ACI 216.1 / IBC 722.3.2 aggregate type the fire equivalent-thickness power law reads: EqThick1HrMm is the
// equivalent thickness (mm) required for a 1-hour fire-resistance rating (the cn in R = (te/cn)^1.7). A lightweight /
// expanded aggregate reaches a rating at a SMALLER equivalent thickness, so its cn is smaller and the same te yields a
// higher rating. Six rows over the four IBC categories: calcareous-or-siliceous gravel (71.1 mm), limestone/cinders/
// air-cooled slag (68.6 mm), expanded clay/shale/slate (66.0 mm), expanded slag or pumice (53.3 mm).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuAggregate {
    public static readonly CmuAggregate Siliceous        = new("siliceous",         eqThick1HrMm: 71.1);   // siliceous gravel — least fire-efficient
    public static readonly CmuAggregate Calcareous       = new("calcareous",        eqThick1HrMm: 71.1);   // calcareous gravel (same IBC category as siliceous)
    public static readonly CmuAggregate NormalWeight     = new("normal-weight",     eqThick1HrMm: 68.6);   // limestone / cinders / air-cooled slag
    public static readonly CmuAggregate ExpandedClay     = new("expanded-clay",     eqThick1HrMm: 66.0);   // expanded clay / shale / slate
    public static readonly CmuAggregate ExpandedSlag     = new("expanded-slag",     eqThick1HrMm: 53.3);   // expanded slag (lightweight) — most fire-efficient tier
    public static readonly CmuAggregate LightweightBlend = new("lightweight-blend", eqThick1HrMm: 53.3);   // expanded slag or pumice blend
    public double EqThick1HrMm { get; }      // ACI 216.1 equivalent thickness (mm) for a 1-hour rating — the power-law cn
}

// The NCMA TEK 6-2C special-unit geometry: EndWebsPresent is the ONE column the on-page cell lattice reads (an open-end
// A/H-block omits its end webs so the unit drops over vertical reinforcement without lifting); CrossWebFraction (the
// retained cross-web depth a bond-beam/channel/lintel knocks down), TroughDepthFraction (the open-top channel/beam
// trough), and ControlSlotFraction (the sash groove / control-joint shear key) are the captured generative columns the
// Construction host solid extrudes. A new special unit is one row carrying its modifier, never a per-shape subtype.
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
    public bool EndWebsPresent { get; }          // false drops the end webs (open-end) — the cell lattice reads it
    public double CrossWebFraction { get; }      // cross-web retained depth fraction — host trough geometry
    public double TroughDepthFraction { get; }   // open-top trough depth as a fraction of unit height — host solid
    public double ControlSlotFraction { get; }   // vertical control-joint / sash slot depth as a fraction of unit width — host solid
}

// The ASTM C90 architectural surface finish geometry — the captured columns the Construction host solid relief-models and
// the Rasm.Bim surface-style egress reads, never a structural-section modifier (a finish is a face texture, not a profile
// variant). Split-face carries its fractured-aggregate relief depth, scored its vertical-score count + pitch, ribbed its
// flute count + depth; precision and ground (burnished/honed) carry no relief. A new finish is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuFinish {
    public static readonly CmuFinish Precision = new("precision",  splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish SplitFace = new("split-face", splitDepthMm: 6.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Scored    = new("scored",     splitDepthMm: 0.0, scoreCount: 2, scoreSpacingMm: 130.0, ribCount: 0, ribDepthMm: 0.0);
    public static readonly CmuFinish Ground    = new("ground",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 0, ribDepthMm: 0.0);   // burnished / honed — surface polish, no relief
    public static readonly CmuFinish Ribbed    = new("ribbed",     splitDepthMm: 0.0, scoreCount: 0, scoreSpacingMm: 0.0,   ribCount: 8, ribDepthMm: 10.0);
    public double SplitDepthMm { get; }      // split-face fractured-aggregate relief depth
    public int ScoreCount { get; }           // vertical scores dividing the face into sub-units
    public double ScoreSpacingMm { get; }    // score pitch along the unit length
    public int RibCount { get; }             // fluted/ribbed vertical profiles across the face
    public double RibDepthMm { get; }        // rib relief depth
}

// --- [MODELS] ------------------------------------------------------------------------------
// One cell of the ASTM C90 unit, materialized from the face-shell / end-web / cross-web columns by CmuCell.Lattice: its
// centre offset along the unit length, its in-bed width (across the unit width, between the face shells) and length
// (along the unit length, between the webs), its demold draft (the per-cell taper the host solid extrudes), and its
// as-built grout/reinforcement state (a grouted cell integrates as solid; a reinforced cell carries the rebar bar). The
// cell dimensions are DERIVED geometry over the already-admitted unit columns, so they ride plain doubles; a degenerate
// lattice (web span exceeding the unit length) rails ComponentFault.Dimension at the layout, never a negative void.
public readonly record struct CmuCell(
    double CentreXMm, double WidthMm, double LengthMm, double DraftDegrees,
    bool Grouted, bool Reinforced, double RebarBarMm) {

    public double AreaMm2 => Math.Max(0.0, WidthMm) * Math.Max(0.0, LengthMm);
    // The horizontal-plane void the ParametricSection.Hollow solver subtracts: centred at (CentreXMm, 0) with the
    // length-axis extent W = LengthMm and the width-axis extent H = WidthMm (the solver's first axis is the unit length).
    public (double X, double Y, double W, double H) Footprint => (CentreXMm, 0.0, LengthMm, WidthMm);

    // The CellCount cells laid from the ASTM columns: CellCount equal cells inset by the face shell across the width,
    // separated by the cross webs, bounded by the end webs (omitted for an open-end unit so the end cells run to the
    // unit ends), centred along the length. The first GroutedCells cells carry grout and the first ReinforcedCells of
    // those carry the rebar bar — the per-cell lattice the partially-grouted/reinforced design reads, replacing the
    // homogeneous void-shrink. A solid grade (CellCount 0) lays no cells; a degenerate span rails the Dimension slot.
    public static Fin<Seq<CmuCell>> Lattice(
        double lengthMm, double widthMm, double faceShellMm, double endWebMm, double crossWebMm, int cellCount,
        int groutedCells, int reinforcedCells, double rebarBarMm, double draftDegrees, bool endWebsPresent, Op key) {
        if (cellCount <= 0) { return Fin.Succ(Seq<CmuCell>()); }
        double endWeb = endWebsPresent ? endWebMm : 0.0;
        double cellWid = widthMm - 2.0 * faceShellMm;
        double webSpan = 2.0 * endWeb + (cellCount - 1) * crossWebMm;
        double cellLen = (lengthMm - webSpan) / cellCount;
        double pitch = cellLen + crossWebMm;
        return cellWid > 0.0 && cellLen > 0.0
            ? Fin.Succ(toSeq(Enumerable.Range(0, cellCount)).Map(i => new CmuCell(
                CentreXMm: -lengthMm / 2.0 + endWeb + cellLen / 2.0 + i * pitch,
                WidthMm: cellWid, LengthMm: cellLen, DraftDegrees: draftDegrees,
                Grouted: i < groutedCells || i < reinforcedCells, Reinforced: i < reinforcedCells,
                RebarBarMm: i < reinforcedCells ? rebarBarMm : 0.0)))
            : Fin.Fail<Seq<CmuCell>>(ComponentFault.Dimension(key, $"<cmu-cell-degenerate:width={cellWid:R}:length={cellLen:R}>"));
    }
}

// The ASTM C90 unit section: the typed grade/strength/density/aggregate/special/finish axes, the outer L×W×H module, the
// face-shell + end-web + cross-web thicknesses, the per-cell void/grout/reinforcement lattice, the demold face-shell
// flare, and the bed + head joints. Every fractional-millimeter structural column rides the kernel PositiveMagnitude;
// the grout state is the per-cell Seq<CmuCell> grout booleans (GroutedCellFraction derived), replacing the prior scalar.
public readonly record struct CmuSection(
    CmuGrade Grade,
    CmuStrength Strength,
    CmuDensity Density,
    CmuAggregate Aggregate,
    CmuSpecialUnit Special,
    CmuFinish Finish,
    PositiveMagnitude ActualWidthMm,
    PositiveMagnitude ActualHeightMm,
    PositiveMagnitude ActualLengthMm,
    PositiveMagnitude FaceShellMm,
    double FaceShellFlareMm,
    PositiveMagnitude EndWebMm,
    PositiveMagnitude CrossWebMm,
    Seq<CmuCell> Cells,
    double BedJointMm,
    double HeadJointMm) {

    public const double GroutDensityKgPerM3 = 2243.0;        // ASTM C476 grout — 140 pcf (the grouted-cell fill density)
    public const double GroutConductivityWPerMK = 1.40;      // grout thermal conductivity — a grouted cell conducts like normal-weight concrete
    public const double CellAirResistanceM2KPerW = 0.18;     // ASHRAE vertical air-cavity resistance — the ungrouted-cell thermal path

    public double GrossAreaMm2 => ActualWidthMm.Value * ActualLengthMm.Value;
    public double CellVoidAreaMm2 => Cells.Sum(static c => c.AreaMm2);
    public double UngroutedCellAreaMm2 => Cells.Filter(static c => !c.Grouted).Sum(static c => c.AreaMm2);
    public double GroutedCellAreaMm2 => Cells.Filter(static c => c.Grouted).Sum(static c => c.AreaMm2);
    public double NetAreaMm2 => GrossAreaMm2 - CellVoidAreaMm2;            // ungrouted net — every cell open
    public double GroutedAreaMm2 => GrossAreaMm2 - UngroutedCellAreaMm2;   // as-built net — only the ungrouted cells open
    public double SolidFraction() => GrossAreaMm2 > 0.0 ? Math.Clamp(NetAreaMm2 / GrossAreaMm2, 0.0, 1.0) : 1.0;
    public double GroutedSolidFraction() => GrossAreaMm2 > 0.0 ? Math.Clamp(GroutedAreaMm2 / GrossAreaMm2, 0.0, 1.0) : 1.0;
    // The fraction of the cell void that is grout-filled, derived from the per-cell Grouted booleans — 0 ungrouted, 1
    // fully grouted — the lattice-honest grout ratio the partially-grouted/reinforced design reads off the cells.
    public double GroutedCellFraction => CellVoidAreaMm2 > 0.0 ? GroutedCellAreaMm2 / CellVoidAreaMm2 : 0.0;
    // The per-unit run advance the layout reads — the unit length plus the 3D head joint (distinct from the bed joint the
    // ComponentUnit CourseHeight folds), so the head joint is captured geometry rather than an implicit coursing scalar.
    public double RunModuleMm => ActualLengthMm.Value + HeadJointMm;

    // The EXACT ungrouted net section the structural design seam reads — net Area AND net moment-of-inertia AND net
    // plastic/torsion/shear columns with EVERY cell subtracted, not a gross-minus-cell-area scalar; the ONE
    // component#COMPONENT_OWNER ParametricSection.Hollow solver runs over the per-cell footprints (the solver's first
    // axis is the unit length, so the call passes Length then Width).
    public Fin<ComputedSection> NetSection(Op key) =>
        ParametricSection.Hollow(ActualLengthMm.Value, ActualWidthMm.Value, Cells.Map(static c => c.Footprint), key);

    // The as-built partially/fully-grouted net section a TMS 402 reinforced-masonry consumer reads — only the UNGROUTED
    // cells emit a void (a grouted cell integrates as solid), so a fully-grouted unit integrates as the solid rectangle
    // and a partially-grouted unit as the exact per-cell net section; the reinforcement#RC_SECTION owner adds the steel
    // transform over this grouted concrete outline through the SAME ParametricSection.Hollow solver, never a per-cell literal.
    public Fin<ComputedSection> GroutedSection(Op key) =>
        ParametricSection.Hollow(ActualLengthMm.Value, ActualWidthMm.Value, Cells.Filter(static c => !c.Grouted).Map(static c => c.Footprint), key);

    // The coarse component#COMPONENT_OWNER Coring void-class bucket (the exact net section is NetSection/GroutedSection):
    // an ungrouted solid-fraction band, the low-solid hollow bucket discriminating the CELL COUNT so a 3-cell unit reads
    // the faithful Hollow3Cell row rather than forcing onto the 2-cell label (the cell count is the lattice's own length).
    public Coring ToCoring() => SolidFraction() switch {
        >= 0.95 => Coring.None,
        >= 0.75 => Coring.Cored3Hole,
        >= 0.60 => Coring.Perforated10Cell,
        _       => Cells.Count >= 3 ? Coring.Hollow3Cell : Coring.Hollow2Cell,
    };

    // The TMS 602 net-area-strength receipt the masonry compressive-utilisation rail reads — the section's specified
    // masonry strength f'm class, the structural-design input (NOT the unit strength), carried as typed data.
    public CmuStrength PrismStrength() => Strength;

    // The ACI 216.1 equivalent thickness (mm): the net solid volume — grout counts as solid — over the exposed L×H face,
    // reducing to (GrossArea − ungrouted-cell void) / Length. A solid/fully-grouted unit's te is its actual width; a
    // hollow unit's is reduced by the ungrouted void fraction.
    public double EquivalentThicknessMm() =>
        ActualLengthMm.Value > 0.0 ? (GrossAreaMm2 - UngroutedCellAreaMm2) / ActualLengthMm.Value : ActualWidthMm.Value;

    // The ACI 216.1 / IBC 722.3.2 fire-resistance rating (hours): the POWER LAW R = (te / cn)^1.7 (the exponent 1/0.59),
    // cn the per-aggregate equivalent thickness for a 1-hour rating (CmuAggregate.EqThick1HrMm), capped at the 4-hour
    // tabulated maximum — a lightweight aggregate reaches a rating at a smaller te, never the prior linear scaling.
    public double FireRatingHours() =>
        Math.Clamp(Math.Pow(EquivalentThicknessMm() / Aggregate.EqThick1HrMm, 1.7), 0.0, 4.0);

    // The wall self-weight (kN/m²): the oven-dry density over the ungrouted net solid plus the per-cell grout at the
    // ASTM C476 GroutDensityKgPerM3, projected onto the L×H face — heaviest for a normal-weight grouted unit, lightest
    // for a hollow lightweight unit. mass(kg) = (net-solid·density + grouted-cell·grout-density)·height·1e-9 (mm²·mm→m³),
    // weight(kN) = mass·g/1000, pressure = weight / face(m²).
    public double SelfWeightKnPerM2() {
        double unitMassKg = (NetAreaMm2 * Density.OvenDryKgPerM3 + GroutedCellAreaMm2 * GroutDensityKgPerM3) * ActualHeightMm.Value * 1e-9;
        double faceAreaM2 = ActualLengthMm.Value * ActualHeightMm.Value * 1e-6;
        return faceAreaM2 > 0.0 ? unitMassKg * 9.80665 / 1000.0 / faceAreaM2 : 0.0;
    }

    // The NCMA TEK 6-2C isothermal-planes (parallel-path) steady-state R (m²·K/W), material-only — the assembly seam adds
    // the surface-film resistances. The two face shells are layers in SERIES with the core; the core is a PARALLEL
    // combination of the solid-web path (the concrete between cells) and the per-cell path (a grouted cell conducts
    // through grout, an ungrouted cell through the trapped-air cavity), the conductances area-weighted by the length
    // fraction each occupies. A solid block (no core, or no cells) is the one homogeneous width/k layer.
    public double ThermalResistanceM2KPerW() {
        double k = Density.ConductivityWPerMK;
        double widthM = ActualWidthMm.Value / 1000.0;
        double coreWidthM = Math.Max(0.0, ActualWidthMm.Value - 2.0 * FaceShellMm.Value) / 1000.0;
        if (coreWidthM <= 0.0 || Cells.IsEmpty) { return widthM / k; }
        double length = ActualLengthMm.Value;
        double faceShellR = FaceShellMm.Value / 1000.0 / k;
        double solidLength = length - Cells.Sum(static c => c.LengthMm);
        double webConductance = solidLength / length * (k / coreWidthM);
        double cellConductance = Cells.Sum(c => c.LengthMm / length * (c.Grouted ? GroutConductivityWPerMK / coreWidthM : 1.0 / CellAirResistanceM2KPerW));
        return 2.0 * faceShellR + 1.0 / (webConductance + cellConductance);
    }

    public Fin<ComponentUnit> ToUnit(Context context, Op key) =>
        ComponentUnit.Of(ActualWidthMm.Value, ActualHeightMm.Value, ActualLengthMm.Value, ActualHeightMm.Value + BedJointMm, context, key);
}

// The raw catalogue row: plain doubles + the typed-axis keys, admitted ONCE through CmuOf into the kernel value-objects
// and the closed CmuGrade/CmuStrength/CmuDensity/CmuAggregate/CmuSpecialUnit/CmuFinish axes so a non-positive dimension
// or an unknown axis key drops the row through Choose rather than seeding a degenerate Component. The grout/reinforcement
// count + bar size, the demold draft, the face-shell flare, and the special/finish keys default to the plain ungrouted
// precision unit, so a standard row sets only its structural columns and a grouted/reinforced/special/finish row names its extras.
public readonly record struct CmuRow(
    string Designation, string Grade, string Strength, string Density, string Aggregate,
    double WMm, double HMm, double LMm, double FaceShellMm, double EndWebMm, double CrossWebMm, int Cells,
    int GroutedCells = 0, int ReinforcedCells = 0, double RebarBarMm = 0.0,
    double FaceShellFlareMm = 0.0, double DraftDegrees = 1.5,
    string Special = "standard", string Finish = "precision");

public sealed record CmuShape(ComponentId Id, CmuSection Section, ComponentStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The bounded standards body is ComponentAuthority.Astm (the ASTM C90 unit-dimension authority, region "us"); the
    // TMS 602-16/-22 strength method is the CmuStrength net-area-strength DATA, not a ComponentStandard column. Both the
    // bed joint and the head joint default to the ASTM standard coordinating thickness the CmuSection carries.
    static readonly ComponentStandard AstmC90 = new("us", StandardJointThicknessMm: 9.5, Authority: ComponentAuthority.Astm);

    // The ASTM C90 nominal-width set: 4/6/8/10/12-inch hollow load-bearing units (actual width 90/140/190/240/290 mm, the
    // 190×390 mm 8-inch module), the minimum face-shell by width (19 mm at 4 in, 25 mm at 6 in, 32 mm at ≥8 in), the
    // end-web/cross-web split (the unit-end webs thicker than the interior webs, both above the 19 mm minimum), the 4/8-in
    // solid units, an 8-inch lightweight expanded-clay unit, an 8-inch fully-grouted unit, an 8-inch single-cell
    // reinforced unit (a #5 / 15.9 mm bar), an 8-inch bond-beam unit, an 8-inch open-end reinforced unit, an 8-inch
    // split-face architectural unit, and a half-high unit. A new finish or metric A-series unit is one further row.
    static readonly Seq<CmuRow> AstmRows = Seq(
        new CmuRow("cmu.4in-hollow",     "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 90.0,  190.0, 390.0, 19.0, 19.0, 19.0, 2),
        new CmuRow("cmu.6in-hollow",     "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 140.0, 190.0, 390.0, 25.0, 25.0, 25.0, 2),
        new CmuRow("cmu.8in-hollow",     "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2),
        new CmuRow("cmu.8in-hollow-lw",  "hollow-load-bearing",     "f2000", "lightweight", "expanded-clay", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2),
        new CmuRow("cmu.10in-hollow",    "hollow-load-bearing",     "f2500", "normal",      "normal-weight", 240.0, 190.0, 390.0, 32.0, 38.0, 25.0, 2),
        new CmuRow("cmu.12in-hollow",    "hollow-load-bearing",     "f2500", "normal",      "normal-weight", 290.0, 190.0, 390.0, 32.0, 38.0, 25.0, 3),
        new CmuRow("cmu.4in-solid",      "solid-load-bearing",      "f2000", "normal",      "normal-weight", 90.0,  190.0, 390.0, 45.0, 19.0, 19.0, 0),
        new CmuRow("cmu.8in-solid",      "solid-load-bearing",      "f2500", "normal",      "calcareous",    190.0, 190.0, 390.0, 95.0, 19.0, 19.0, 0),
        new CmuRow("cmu.8in-grouted",    "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, GroutedCells: 2),
        new CmuRow("cmu.8in-reinforced", "hollow-load-bearing",     "f2500", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, ReinforcedCells: 1, RebarBarMm: 15.9),
        new CmuRow("cmu.8in-bondbeam",   "hollow-load-bearing",     "f2000", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, GroutedCells: 2, ReinforcedCells: 1, RebarBarMm: 12.7, Special: "bond-beam"),
        new CmuRow("cmu.8in-openend",    "hollow-load-bearing",     "f2500", "normal",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, ReinforcedCells: 1, RebarBarMm: 15.9, Special: "open-end"),
        new CmuRow("cmu.8in-splitface",  "hollow-load-bearing",     "f2000", "medium",      "normal-weight", 190.0, 190.0, 390.0, 32.0, 32.0, 25.0, 2, Finish: "split-face"),
        new CmuRow("cmu.8in-halfhigh",   "hollow-non-load-bearing", "f2000", "lightweight", "expanded-slag", 190.0, 90.0,  390.0, 32.0, 32.0, 25.0, 2));

    static Fin<CmuShape> CmuOf(CmuRow r, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: r.HMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: r.LMm)
        from fs in key.AcceptValidated<PositiveMagnitude>(candidate: r.FaceShellMm)
        from endWeb in key.AcceptValidated<PositiveMagnitude>(candidate: r.EndWebMm)
        from crossWeb in key.AcceptValidated<PositiveMagnitude>(candidate: r.CrossWebMm)
        from grade in CmuGrade.TryGet(r.Grade, out CmuGrade? g) ? Fin.Succ(g!) : Fin.Fail<CmuGrade>(ComponentFault.Family(key, $"<unknown-cmu-grade:{r.Grade}>"))
        from strength in CmuStrength.TryGet(r.Strength, out CmuStrength? s) ? Fin.Succ(s!) : Fin.Fail<CmuStrength>(ComponentFault.Family(key, $"<unknown-cmu-strength:{r.Strength}>"))
        from density in CmuDensity.TryGet(r.Density, out CmuDensity? d) ? Fin.Succ(d!) : Fin.Fail<CmuDensity>(ComponentFault.Family(key, $"<unknown-cmu-density:{r.Density}>"))
        from aggregate in CmuAggregate.TryGet(r.Aggregate, out CmuAggregate? a) ? Fin.Succ(a!) : Fin.Fail<CmuAggregate>(ComponentFault.Family(key, $"<unknown-cmu-aggregate:{r.Aggregate}>"))
        from special in CmuSpecialUnit.TryGet(r.Special, out CmuSpecialUnit? sp) ? Fin.Succ(sp!) : Fin.Fail<CmuSpecialUnit>(ComponentFault.Family(key, $"<unknown-cmu-special:{r.Special}>"))
        from finish in CmuFinish.TryGet(r.Finish, out CmuFinish? fn) ? Fin.Succ(fn!) : Fin.Fail<CmuFinish>(ComponentFault.Family(key, $"<unknown-cmu-finish:{r.Finish}>"))
        from cells in CmuCell.Lattice(l.Value, w.Value, fs.Value, endWeb.Value, crossWeb.Value, r.Cells, r.GroutedCells, r.ReinforcedCells, r.RebarBarMm, r.DraftDegrees, special.EndWebsPresent, key)
        select new CmuShape(
            ComponentId.Of(r.Designation),
            new CmuSection(grade, strength, density, aggregate, special, finish, w, h, l, fs, r.FaceShellFlareMm, endWeb, crossWeb, cells, AstmC90.StandardJointThicknessMm, AstmC90.StandardJointThicknessMm),
            AstmC90);

    // Every realized CMU unit resolved to its CmuShape ONCE — the seed BuildCmuRows and CmuSections both derive from
    // (DERIVED_LOGIC, one source), the SAME shape steel#STEEL_FAMILY Shapes / timber#TIMBER_FAMILY Shapes take. A
    // malformed row (a non-positive dimension, an unknown axis key, a degenerate cell lattice) drops through Choose.
    static readonly Seq<CmuShape> Shapes =
        AstmRows.Choose(row => CmuOf(row, default, default).ToOption());

    // The ComponentId → Component rows component#COMPONENT_OWNER ComponentCatalogue.Build folds: a CMU is a
    // ComponentClass.Minor part (IfcElementComponent — one standardized Type, many laid pieces), its cross-section the
    // ComponentSection.Cmu arm carrying the rich CmuSection, its canonical dimensional projection the ComponentUnit the
    // layout fold reads, its void class the relocated Coring, its appearance the concrete.cmu MaterialId row.
    public static FrozenDictionary<ComponentId, Component> BuildCmuRows(Context context) =>
        Shapes
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Component: new Component(
                    ComponentClass.Minor, ComponentFamily.Cmu, unit, shape.Section.ToCoring(), shape.Standard,
                    new ComponentSection.Cmu(shape.Section), MaterialId.Of("concrete.cmu")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Component, ComparerAccessors.StringOrdinal.EqualityComparer);

    // The ComponentId → ComputedSection map the component#COMPONENT_OWNER ComponentCatalogue.Sections folds and the
    // component#COMPONENT_RESOLUTION ComponentResolution.Build caches by ProfileRef (the realized sibling to
    // steel#STEEL_FAMILY SteelSections / timber#TIMBER_FAMILY TimberSections) — CMU is a profiled structural family, so a
    // Rasm.Compute MasonryCompression check reads the unit's net section off the seam without re-running the integral.
    // The cached section is the AS-BUILT GroutedSection (a hollow row its exact per-cell net section, the fully-grouted
    // row its solid rectangle) through the shared component#COMPONENT_OWNER ParametricSection.Hollow solver; a section
    // whose integral fails drops through Choose (a build-time ComponentFault.Section surfaced in THIS page, never a
    // swallowed gap the resolver mis-reports as "unregistered").
    public static FrozenDictionary<ComponentId, ComputedSection> CmuSections(Context context) =>
        Shapes
            .Choose(shape => shape.Section.GroutedSection(default).ToOption()
                .Map(section => (shape.Id, Section: section)))
            .ToFrozenDictionary(static r => r.Id, static r => r.Section, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [CMU_ROW_TRANSCRIPTION]: REALIZED — the ASTM C90 standard carries the hollow/solid load-bearing concrete-masonry-unit grades with the actual 190×190×390 mm 8-inch module, the minimum face-shell thickness by nominal width (19 mm at 4 in, 25 mm at 6 in, 32 mm at 8 in and above), and the 19 mm minimum web thickness now SPLIT into the `EndWebMm` (the two unit-end webs) and the `CrossWebMm` (the interior webs) per ASTM C90-14 Table 1, so the net-section web span is `2·EndWebMm + (cells−1)·CrossWebMm` rather than the prior `(cells+1)` uniform-web inference. The catalogue carries the 4/6/8/10/12-inch hollow nominal widths, the 4/8-inch solid grades, an 8-inch lightweight expanded-clay unit, an 8-inch fully-grouted unit, an 8-inch single-cell reinforced unit, an 8-inch bond-beam unit, an 8-inch open-end reinforced unit, the 190-mm split-face architectural unit, and the 90-mm half-high unit, keyed `cmu.<designation>`; a new architectural finish, special unit, or metric A-series unit is one further `CmuRow` data addition, never a new type. The raw `CmuRow` admits once through `CmuOf` into the kernel value-objects (`PositiveMagnitude` length columns) and the closed `CmuGrade`/`CmuStrength`/`CmuDensity`/`CmuAggregate`/`CmuSpecialUnit`/`CmuFinish` axes, a non-positive dimension, an unknown axis key, or a degenerate cell lattice dropping the row through `Choose`.
- [CMU_CELL_LATTICE]: REALIZED — the per-cell void/grout/reinforcement model is the `Seq<CmuCell>` lattice `CmuCell.Lattice` materializes from the ASTM face-shell/end-web/cross-web columns: each cell carries its centre offset along the length, its in-bed width and length, its demold draft, and its as-built grout/reinforced state plus rebar bar diameter, REPLACING the prior uniform-cell inference and the homogeneous `sqrt(1−f)` void-shrink. `CmuSection.NetSection` subtracts EVERY cell footprint (the pure ungrouted net), `GroutedSection` subtracts only the UNGROUTED cells (a grouted cell integrates as solid), and `GroutedCellFraction` is the per-cell grout ratio derived from the lattice — so a partially-grouted or single-cell reinforced unit integrates as the exact per-cell net section rather than an area-shrunk approximation. The cell dimensions are derived geometry over the already-admitted unit columns (plain doubles); a degenerate lattice (web span exceeding the unit length) rails `ComponentFault.Dimension` at the layout, never a negative void. An open-end (`CmuSpecialUnit.OpenEnd`) unit omits its end webs through the `EndWebsPresent` flag the lattice reads, extending the end cells to the unit ends. Ripple counterpart: `reinforcement#RC_SECTION` (the grouted-concrete + reinforcement transformed section the `GroutedSection` outline + the per-cell `RebarBarMm` feed).
- [CMU_STRENGTH_GRADE]: REALIZED — the structural-design input is the typed `CmuStrength` `[SmartEnum]` (the TMS 602-16/-22 Table 2 specified-masonry-strength classes f2000/f2250/f2500/f2750/f3000 carrying the assemblage strength `f'm` in MPa and the published net-area UNIT strength the unit-strength method requires in the two mortar columns the table tabulates — Type M or S, and the higher Type N), the sibling of the `steel#STEEL_FAMILY` `SteelGrade.YieldMpa` and the `timber#TIMBER_FAMILY` `TimberGrade.FmkMpa` design strengths — so a CMU member's design strength is the registered grade DATA, never a caller-supplied raw double. The two mortar columns are CORRECT to the TMS grouping (Type N mortar requires the HIGHER unit strength than Type M or S for the same `f'm`), and the Type-N cell is empty for f2750/f3000 (no Type-N mortar reaches those classes), carried as `+Infinity` so a Type-N unit never qualifies. `CmuStrength.Resolve(netUnitStrengthMpa, mortar)` inverts the table (the HIGHEST `f'm` class whose required unit strength the supplied unit meets, the descending-by-`f'm` scan's first passing row), so a stronger unit + stronger mortar yields a higher `f'm`; a unit below the F2000 floor rails `None`. The `CmuSection.PrismStrength` receipt feeds the `capacity#SECTION_CAPACITY` masonry compressive utilisation through the `SectionCapacityResolver.MasonryCompression(double fmMpa, ComputedSection section, double slendernessReduction)` lift onto the `SectionCapacity.MasonryCompression` case (the TMS 402 axial-flexural unreinforced/reinforced check), the TMS 402 §9.3.4.1.1 member-stability slenderness reduction passed IN by the `Rasm.Compute`/cmu CALLER (this owner is a unit-section vocabulary and NEVER computes member height — wall slenderness is a placement-level input), so `PrismStrength().FmMpa` + `GroutedSection` + the caller's `R` close the lift. Ripple counterpart: `capacity#SECTION_CAPACITY` (the `MasonryCompression` case + `SectionCapacityResolver.MasonryCompression` lift) and `masonry#MASONRY_FAMILY` `MortarType` (the unit-strength-method mortar band).
- [CMU_DENSITY_FIRE_THERMAL_MASS]: REALIZED — the `CmuDensity` `[SmartEnum]` (ASTM C90 lightweight <1680 / medium 1680–2000 / normal ≥2000 kg/m³ oven-dry density, each carrying the concrete thermal conductivity) and the `CmuAggregate` `[SmartEnum]` (the ACI 216.1 / IBC 722.3.2 fire aggregate types over six rows in four categories, each carrying its 1-hour equivalent thickness `EqThick1HrMm`) realize the three physical receipts: `CmuSection.SelfWeightKnPerM2` is the wall dead load (the oven-dry density over the ungrouted net solid plus the per-cell grout at the ASTM C476 `GroutDensityKgPerM3` 2243 kg/m³, REPLACING the prior hardcoded 2240); `CmuSection.FireRatingHours` is the ACI 216.1 equivalent-thickness POWER LAW `R = clamp((te/cn)^1.7, 0, 4)` (the exponent 1/0.59, `te = (GrossArea − ungrouted-cell void)/Length`, `cn` the per-aggregate `EqThick1HrMm`), REPLACING the prior linear scaling so a fully-grouted normal-weight 8-inch unit reads its 4-hour rating and a hollow lightweight unit its lower rating from the curve; and `CmuSection.ThermalResistanceM2KPerW` is the NCMA TEK 6-2C isothermal-planes multi-cell R (the two face shells in series with the core's parallel web/cell paths, a grouted cell conducting through `GroutConductivityWPerMK`, an ungrouted cell through the `CellAirResistanceM2KPerW` cavity), the multi-cell steady-state R the prior page lacked. All three are realized receipts the thermal/fire/structural seam reads off the section, never a Pset string.
- [CMU_GENERATIVE_GEOMETRY]: REALIZED — the captured generative geometry the `Construction/layout#ASSEMBLY_FOLD` host solid extrudes and the `Rasm.Bim` surface-style egress reads: the per-cell `DraftDegrees` (the demold taper), the `FaceShellFlareMm` (the flared bearing face shell), the `CmuSpecialUnit` molding axis (bond-beam knocked-down cross webs + channel/lintel troughs via `CrossWebFraction`/`TroughDepthFraction`, sash/control-joint slots via `ControlSlotFraction`, the open-end `EndWebsPresent` the cell lattice consumes), the `HeadJointMm` 3D head joint (the `RunModuleMm` run advance), and the `CmuFinish` surface axis (split-face relief depth, scored count + pitch, ribbed flute count + depth, ground polish). The structural net section reads only the cell/web/face-shell columns (the minimum-net basis); the molding/finish columns are the generative parameters the host materializes, a finish a surface-style on the element rather than a profile variant. A new special unit or finish is one `CmuSpecialUnit`/`CmuFinish` row, never a parallel section owner.
- [IFCPROFILEDEF_CMU_ALIGNMENT]: REALIZED — the `CmuGrade.IfcSubtype` carries the wire spelling: a SOLID or fully-grouted unit maps to the `IfcRectangleProfileDef` rectangle (the outer 190×390 mm face the `XDim`/`YDim`), and a multi-cell HOLLOW unit maps to `IfcArbitraryProfileDefWithVoids` carrying the EXPLICIT per-cell voids the `CmuSection.NetSection` `CellVoids` describe — NOT the single-uniform-void `IfcRectangleHollowProfileDef`, which cannot represent two or three distinct cells. The cmu member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` carrying the rectangle / voided profile plus the `Coring` receipt, the split-face/ground/ribbed finish a surface-style on the element, the grout state a `Pset_ConcreteElementGeneral`-class property the `Rasm.Bim` egress derives from the per-cell grout lattice. The probe is the per-finish surface-style mapping at the `Rasm.Bim` boundary; the rectangle / `IfcArbitraryProfileDefWithVoids` profile is the realized base case the egress resolves at the seam.
- [CMU_SECTION_MAP]: REALIZED — the `ComponentCatalogue.CmuSections` `FrozenDictionary<ComponentId, ComputedSection>` is the M7 section-map contribution `component#COMPONENT_OWNER` `ComponentCatalogue.Sections` folds (the sibling to `steel#STEEL_FAMILY` `SteelSections` and `timber#TIMBER_FAMILY` `TimberSections`) — it caches each unit's AS-BUILT `GroutedSection` once over the one `Shapes` seed (a hollow row its exact per-cell net section, the fully-grouted row its solid rectangle), so the `component#COMPONENT_RESOLUTION` `ComponentResolution.Build` joins a CMU `ProfileRef` to `Some(section)` and the `capacity#SECTION_CAPACITY` `MasonryCompression` case reads the grouted net area/modulus off the seam without re-running the `ParametricSection.Hollow` integral per call; a section whose integral fails drops through `Choose` as a build-time `ComponentFault.Section` surfaced here, never a silent-drop a resolver mis-reports as "unregistered". The elastic columns + fire-exposed perimeter come from the one `VividOrange.Sections.SectionProperties` Green's-theorem integral over the per-cell `Perimeter`, the plastic/torsion/shear columns from the `ParametricSection.HollowPlastics` superposition, so the cmu net section fills the SAME seventeen-field `ComputedSection` receipt every family fills.
