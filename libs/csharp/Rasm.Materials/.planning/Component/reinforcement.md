# [MATERIALS_REINFORCEMENT]

THE REINFORCING-BAR COMPONENT FAMILY and THE HOST-NEUTRAL REINFORCED-CONCRETE-SECTION ASSEMBLER. The reinforcement vocabulary — the `RebarGrade` yield-strength axis (ASTM A615/A706, CSA G30.18, the EN-10080 grades bound to their `EnRebarGrade`), the `BarSize` nominal-bar axis (imperial #3..#18, CSA 10M..55M, the full EN-10080 H6..H50 series keyed to the `BarDiameter` catalogue), the `RebarUsage` `IfcReinforcingBarTypeEnum` structural-role axis and the `RebarSurface` `IfcReinforcingBarSurfaceEnum` bond axis, the `RebarRibGeometry` rib-deformation capture (ISO 6935-2 transverse-rib height/spacing, flank inclination α, rib-to-axis inclination β, the `RelativeRibArea` fR bond invariant, the ribless-perimeter fraction Σf_i over the `RibPattern` axis), the `RebarBend` ACI 318 §25.3 / EN 1992 §8.3 bend-and-mandrel schedule over the `HookKind` table discriminant and the BS 8666:2020 `ShapeCode` set, and the `RebarSection` cross-section receipt — is the realized reinforcing-bar vocabulary one `component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Reinforcement` case at `ComponentClass.Minor` (a bar is an `IfcElementComponent` standardized part, one Type, many placed pieces), and THIS owner is ALSO the host-neutral reinforced-concrete-section assembler: `RcSectionBuilder.Of` composes the `VividOrange.Sections` `ConcreteSection` from a FAMILY-AGNOSTIC `Component` concrete outline, an `EnConcreteMaterial`/`EnRebarMaterial` EN grade lowered through the `EnGrade` boundary, and the `RebarLayout` `[Union]` over the EN-10080 `BarDiameter` catalogue, lifting the `IConcreteSection` the `capacity#SECTION_CAPACITY` transformed-section and N-M-M solvers consume. A #5 rebar is a `Component` row, never a `Rebar` type: the bar size, the grade, the structural role, the bond surface, the rib geometry, the section receipt, and the bend template are reinforcement-`ComponentSection.Reinforcement` columns, and the `RebarSection` projection feeds the same `component#COMPONENT_OWNER` `Component.Of` admission and the same `ComponentCatalogue.Build` fold every sibling family drives — a reinforcing schedule places through the construction layout fold over one `Component`, never a per-family schedule owner. The reinforcement vocabulary grows by data — a new bar size is one `BarSize` row keyed to its `BarDiameter` catalogue entry, a new grade one `RebarGrade` row bound to its `EnRebarGrade`, a new structural role one `RebarUsage` row carrying its `IfcReinforcingBarTypeEnum` token, a new bend a `HookKind`/`ShapeCode` row — never a per-bar type. The bend geometry is the scalar `RebarBend` tuple the host materializes into a curve, NEVER a host curve here (the host-neutral scalar discipline `Construction/layout#ASSEMBLY_FOLD` keeps). The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentId`/`ComponentSection`/`ComponentFault`/`ParametricSection` shape, the `Rasm.Vectors` kernel `PositiveMagnitude` for every length/area/weight column, the `VividOrange.Sections` reinforced-section + reinforcement-layout owner, the `VividOrange.Materials` EN grade DATA + `EnRebarFactory` constitutive-law factory bound to its `VividOrange.Standards` `En1992` citation, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` appearance column each row carries, the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt read by `MaterialId` (never re-derived here), and the elastic transformed-section and ultimate N-M-M capacity of the RC section over the `IConcreteSection` this owner mints at `capacity#SECTION_CAPACITY`; the `fastener#FASTENER_FAMILY`, `connector#CONNECTOR_FAMILY`, and `joint#JOINT_FAMILY` families land their own sibling vocabularies, the `Projection/component#COMPONENT_PROJECTOR` lowers the reinforcement occurrence onto the seam graph, and `ProfileRef`/`ProfileSet`/`ComputedSection` stay seam-canonical (composed unchanged).

## [01]-[INDEX]

- [02]-[REINFORCEMENT_FAMILY]: the `RebarStandard` spec discriminant, the `RebarGrade` yield-strength axis bound to its `EnRebarGrade`, the `BarSize` nominal-bar axis keyed to the EN-10080 `BarDiameter`, the `RebarUsage` `IfcReinforcingBarTypeEnum` structural-role axis and the `RebarSurface` `IfcReinforcingBarSurfaceEnum` bond axis, the `RibPattern` rib-form axis and the `RebarRibGeometry` ISO 6935-2 deformation receipt, the `HookKind` ACI 318 §25.3 bend-table discriminant, the BS 8666:2020 `ShapeCode` 37-code schedule set, the `RebarHook` standard-hook angle axis, the `RebarBend` bend-and-mandrel receipt, the `RebarSection` cross-section payload (the `ComponentSection.Reinforcement` arm), and the `ComponentCatalogue.BuildRebarRows` ASTM/CSA/EN row table.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete-section owner — the `VividOrange.Sections` `ConcreteSection` admission boundary over a FAMILY-AGNOSTIC `Component` concrete outline + an `EnConcreteMaterial`/`EnRebarMaterial` EN grade + a `RebarLayout` `[Union]` (count/spacing × face/perimeter) over the `BarDiameter` catalogue, lifting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume, with the `EnGrade` admission lowering the `VividOrange.Materials` derivation throws onto the typed `ComponentFault` rail and the EC2 `MinimumReinforcementSpacing` clear-spacing rule composed.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: the reinforcement vocabulary (`RebarStandard` the spec discriminant, `RebarGrade` the ASTM/CSA/EN yield axis, `BarSize` the #3..#18 / 10M..55M / H6..H50 nominal-bar axis, `RebarUsage` the `IfcReinforcingBarTypeEnum` role axis, `RebarSurface` the `IfcReinforcingBarSurfaceEnum` bond axis, `RibPattern` the ISO 6935-2 rib-form axis, `HookKind` the ACI 318 §25.3 bend-table discriminant, `ShapeCode` the BS 8666:2020 schedule set, `RebarHook` the standard-hook angle axis); `RebarRibGeometry`/`RebarBend`/`RebarSection` the receipts; `ComponentCatalogue.BuildRebarRows` the registered-row seed `component#COMPONENT_OWNER` `ComponentCatalogue.Build` folds.
- Cases: grade {A615 Gr40/Gr60/Gr75/Gr80 (carbon-steel, non-weldable above Gr60 without supplemental S1) · A706 Gr60/Gr80 (low-alloy, weldable) · CSA G30.18 400W/500W (metric, weldable) · EN-10080 B500A/B500B/B500C (the EN-bodied ductility classes A/B/C the RC σ(ε) law reads)} · size {imperial #3..#11, #14, #18 · CSA 10M..55M · EN H6..H50 keyed to `BarDiameter.D6`..`D50`} · usage {main · ligature/shear stirrup · punching · edge · ring · anchoring · spacer · stud, each its verified `IfcReinforcingBarTypeEnum` token} · surface {textured deformed (ribbed, default) · plain round} · rib-pattern {uniform-height parallel ribs · crescent inclined two-series} · hook-kind {development (ACI Table 25.3.1) · stirrup-tie (Table 25.3.2) · seismic (Table 25.3.4)} · hook-angle {90° · 135° · 180°} · shape-code {the BS 8666:2020 set} — a bar is a `Component` row over one `RebarGrade`, one `BarSize`, one `RebarUsage`, and one `RebarSurface`, never a bar/role subtype.
- Entry: `public Fin<RebarBend> StandardHook(HookKind kind, RebarHook hook, Op key)` on `RebarSection` — the ACI 318 §25.3 standard-hook projection resolving the minimum inside bend diameter (`HookKind.MinInsideBendFactor(d)·d` — 6/8/10·d for development by bar band, 4/6·d for stirrup-tie and seismic), the straight hook extension (`hook.ExtensionFactor·d` floored at `hook.MinExtensionMm`), the EN 1992 §8.3 / BS 8666:2020 mandrel diameter (4·d for d ≤ 16 mm, 7·d above — the value `VividOrange.Sections` `Link.MinimumMandrelDiameter` computes for the actual link), AND the BS 8666:2020 `ShapeCode` (a `RebarUsage.Stirrup` bar schedules as a closed link `51`, a longitudinal bar as the hook-angle-keyed end-hook code) into the scalar `RebarBend` tuple the host curve-materializes and the `IfcReinforcingBar` `BendingShapeCode` reads; `ComponentCatalogue.BuildRebarRows(context)` folds the A615/A706/CSA/EN `RebarRow` table through `RebarOf` (resolving grade/size/usage/surface and admitting the dimensional columns) into the registered `Component` rows `ComponentCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBarBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` from `Rasm.Vectors` for the bar-diameter/area/weight columns, never an int-backed `Dimension` that truncates a fractional millimetre; `Op`/`Context`/`AcceptValidated` from `Rasm.Domain` the boundary-admission key), Rasm.Element (project — `MaterialId` the row's appearance/capacity identity), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue diameter the EN `BarSize` rows key, `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnRebarGrade` the EN-bodied grade the EN rows bind, `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` the registered yield + ductility-class ultimate the EN bar projects, `.api/api-vividorange-materials.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the grade/size/standard/usage/surface/pattern/hook/hook-kind/shape-code axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the reinforcement vocabulary grows by data — a new bar size is one `BarSize` row carrying its nominal diameter/area/weight (the EN rows keyed to a `BarDiameter` catalogue value), a new grade one `RebarGrade` row carrying its minimum yield bound to its `EnRebarGrade`, a new structural role one `RebarUsage` row carrying its `IfcReinforcingBarTypeEnum` token, a new rib form one `RibPattern` row, a new bend table one `HookKind` row, a new schedule shape one `ShapeCode` row — never a per-bar/per-role type, never a per-grade `Component` variant. The `ComponentFamily` axis stays CLOSED at nine (masonry/cmu/steel/timber/glazing/reinforcement/fastener/connector/joint); a welded reinforcement mesh grows as an `IfcReinforcingMesh` projection reading the same `RebarSection` columns, never a tenth family. The reinforced-concrete SECTION the rebar populates is the `[03]-[RC_SECTION]` owner block on this page, its capacity composed at `capacity#SECTION_CAPACITY`.
- Boundary: `RebarSection` composes the `Rasm.Vectors` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ASTM nominal diameter (`#5 = 15.875 mm`) admits without the truncation an int-backed `Dimension` count forces; the IFC structural ROLE is the `RebarUsage` `IfcReinforcingBarTypeEnum` token column (`main`→`MAIN`, `ligature`→`LIGATURE`, `shear`→`SHEAR`, `spacer`→`SPACEBAR`, `stud`→`STUD`) the `IfcReinforcingBar` `PredefinedType` carries and the BOND surface the `RebarSurface` `IfcReinforcingBarSurfaceEnum` token (`textured`→`TEXTURED`, `plain`→`PLAIN`), so a stirrup's IFC role and a deformed bar's bond are typed columns, never an inferred default; the RIB DEFORMATION is the `RebarRibGeometry` ISO 6935-2 receipt the `RebarSection.Ribs(pattern)` projection derives from the bar diameter — the `RelativeRibArea` fR (the bond invariant, the ISO 6935-2 / EN 10080 minimum 0.035 / 0.040 / 0.056 by the ≤6 / ≤12 / >12 mm diameter band) plus the transverse-rib height/spacing, the longitudinal-rib height, the flank inclination α (ISO 6935-2 §4.14, 45°), the rib-to-axis inclination β (ISO 6935-2 §4.15, the `RibPattern` sets it — 90° uniform-height, 60° crescent), and the ribless-perimeter fraction Σf_i (the ASTM A615 §7.4 gap limit 0.125), so a deformed bar carries its bond geometry the development-length and the federation read, a plain round `None`; the bend geometry is the scalar `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm, MandrelDiameterMm, ShapeCode)` `StandardHook` emits — the `MinInsideBendFactor` SPLIT by `HookKind` so a #5 stirrup bends at 4·d_b (Table 25.3.2) not the 6·d_b a development bar uses (Table 25.3.1), the `MandrelDiameterMm` the EN 1992 §8.3 former diameter (`VividOrange` `Link.MinimumMandrelDiameter` parity), the `ShapeCode` the BS 8666:2020 schedule designation the `IfcReinforcingBar` `BendingShapeCode` carries — NEVER a host `Curve`, the host boundary lofts the bar centreline and the bend arc from the scalar tuple exactly as the construction layout materializes a `Placement`, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.iron` for plain carbon A615, `metal.steel` for low-alloy A706 / weldable EN) the row's `AppearanceId` column carries, never a reinforcement-specific shade; the mechanical capacity crosses to `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by the GRADE `MaterialId` `RebarGrade.CapacityId` (the per-grade `steel.<designation>` reinforcing-steel row — the EN bands `steel.b500a`/`b500b`/`b500c`, the ASTM/CSA bands `steel.gr40`…`steel.500w` — carrying the grade's `f_yk` AND the ACI 318 §20.2.2.2 reinforcing modulus `E_s` 200 GPa, NEVER the generic `metal.steel` section baseline whose `E` 210 GPa / `f_y` 235 is the wrong physics for a 500/420 MPa bar), never re-derived here, so the `RebarGrade.MinimumYieldMpa` axis is the spec-nominal grade band, the `RebarSection.YieldForceKn` projection the spec-nominal `f_yk·A_s` total, and the EN-registered `EnYieldForceKn`/`EnUltimateForceKn` the `EnRebarFactory.CreateLinearElastic`/`CreateBiLinear` design yield + ductility-class `k·f_yk` ultimate the development/lap/overstrength capacity-design seam reads off the EN-bodied bar (the spec-nominal, the EN-registered, and the `CapacityId`-keyed `Mechanical` strengths disjoint by source, the last the `graph.MechanicalOf` row the structural runner reads off the projected bar); the `CapacityId` and `AppearanceId` are INDEPENDENT `MaterialId` slots (the two-slot law) — a coated bar's graded capacity steel and its epoxy finish stay distinct; `ComponentCatalogue.BuildRebarRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue` table with the A615/A706/G30.18/EN-10080 rows keyed `reinforcement.<designation>` (`reinforcement.rebar-no5-gr60`, `reinforcement.rebar-10m-400w`, `reinforcement.rebar-h16-b500c`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`; the placement of a rebar schedule reads `Construction/layout#ASSEMBLY_FOLD` over the SAME realized fold, never a parallel reinforcement-layout owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;                                // PositiveMagnitude (>0 finite magnitude — the bar-diameter/area/weight columns); the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                                 // Op (the boundary-admission key), AcceptValidated (the admission extension), Context
using Rasm.Element;                                // MaterialId (the seam-carried appearance/capacity identity the RebarSection AppearanceId/CapacityKey reference)
using Thinktecture;
using Rasm.Materials.Component;                    // Component/ComponentId/ComponentFamily/ComponentSection/ComponentFault/Coring/ComponentStandard/ComponentAuthority (the parent COMPONENT_OWNER)
using VividOrange.Sections.Reinforcement;          // BarDiameter (the EN-10080 D6..D50 catalogue diameter the EN BarSize rows key)
using VividOrange.Materials.StandardMaterials.En;  // EnRebarGrade (the EN-bodied grade the EN rows bind), EnRebarFactory (the registered yield + ductility-class ultimate)
using static LanguageExt.Prelude;                  // guard, Seq, Some/None, Optional

// Each Component family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the nine sibling
// `ComponentCatalogue` static classes are distinct types (a shared Rasm.Materials.Component collides CS0101 against
// component.md's own `ComponentCatalogue`); component#COMPONENT_OWNER stays the parent Rasm.Materials.Component
// and folds Reinforcement.ComponentCatalogue.BuildRebarRows by the sub-namespace-qualified name; parent types via the using above.
namespace Rasm.Materials.Component.Reinforcement;

// --- [TYPES] -------------------------------------------------------------------------------
// RebarStandard the spec discriminant carrying weldability + the citing authority; RebarUsage the IfcReinforcingBarTypeEnum
// role axis carrying its verified wire token; RebarSurface the IfcReinforcingBarSurfaceEnum bond axis carrying the ribbed
// flag; RibPattern the ISO 6935-2 rib-form axis setting the rib-to-axis inclination β; RebarGrade the yield band bound to
// its EnRebarGrade; BarSize the nominal-bar axis keyed to a BarDiameter; HookKind the ACI 318 §25.3 bend-table discriminant;
// ShapeCode the BS 8666:2020 schedule set; RebarHook the standard-hook angle axis — closed [SmartEnum<string>] vocabularies,
// every reinforcement variant a row, never a per-bar/per-grade/per-role subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615 = new("astm-a615", weldable: false, authority: "ASTM A615/A615M");
    public static readonly RebarStandard A706 = new("astm-a706", weldable: true, authority: "ASTM A706/A706M");
    public static readonly RebarStandard G30 = new("csa-g30.18", weldable: true, authority: "CSA G30.18");
    public static readonly RebarStandard En10080 = new("en-10080", weldable: true, authority: "EN 1992-1-1 / EN 10080 / ISO 6935-2");
    public bool Weldable { get; }
    public string Authority { get; }
}

// The bar's STRUCTURAL ROLE — each row carrying its verified IfcReinforcingBarTypeEnum member spelling
// (GeometryGym 25.7.30: MAIN/LIGATURE/SPACEBAR/PUNCHING/EDGE/RING/ANCHORING/SHEAR/STUD/USERDEFINED/NOTDEFINED). A stirrup
// is RebarUsage.Ligature, a longitudinal bar RebarUsage.Main, a shear leg RebarUsage.Shear — the IFC role is a column the
// Projection/component#COMPONENT_PROJECTOR lands on the seam Object node's PredefinedType, never a per-role rebar subtype.
// The Stirrup flag drives the RcSection link-vs-longitudinal placement (a Ligature row routes to the ConcreteSection Link)
// AND the StandardHook ShapeCode (a stirrup schedules as a closed link 51).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarUsage {
    public static readonly RebarUsage Main      = new("main",      ifcPredefinedType: "MAIN",      stirrup: false);
    public static readonly RebarUsage Ligature  = new("ligature",  ifcPredefinedType: "LIGATURE",  stirrup: true);
    public static readonly RebarUsage Shear     = new("shear",     ifcPredefinedType: "SHEAR",     stirrup: true);
    public static readonly RebarUsage Punching  = new("punching",  ifcPredefinedType: "PUNCHING",  stirrup: false);
    public static readonly RebarUsage Edge      = new("edge",      ifcPredefinedType: "EDGE",      stirrup: false);
    public static readonly RebarUsage Ring      = new("ring",      ifcPredefinedType: "RING",      stirrup: true);
    public static readonly RebarUsage Anchoring = new("anchoring", ifcPredefinedType: "ANCHORING", stirrup: false);
    public static readonly RebarUsage Spacer    = new("spacer",    ifcPredefinedType: "SPACEBAR",  stirrup: false);
    public static readonly RebarUsage Stud      = new("stud",      ifcPredefinedType: "STUD",      stirrup: false);   // cast-in headed-stud reinforcement (the IfcReinforcingBarTypeEnum STUD role) — distinct from the welded shear connector joint#JOINT_FAMILY owns
    public string IfcPredefinedType { get; }
    public bool Stirrup { get; }   // true -> the RcSection link / a transverse confinement bar; false -> a longitudinal layer bar
}

// The bar's BOND SURFACE — the verified IfcReinforcingBarSurfaceEnum {PLAIN, TEXTURED} the IfcReinforcingBar wire carries.
// Ribbed gates RebarSection.Ribs: a Textured bar projects its RebarRibGeometry, a Plain round (a tie/spacer or smooth dowel) None.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarSurface {
    public static readonly RebarSurface Textured = new("textured", ifcSurface: "TEXTURED", ribbed: true);    // deformed/ribbed (the default for A615/A706/EN-10080 hot-rolled bar)
    public static readonly RebarSurface Plain    = new("plain",    ifcSurface: "PLAIN",    ribbed: false);   // plain round (ties, spacers, smooth dowels)
    public string IfcSurface { get; }
    public bool Ribbed { get; }
}

// The rib-deformation FORM — the ISO 6935-2 §4.15 angle β between a transverse rib and the bar axis the RebarRibGeometry
// reads: a UniformHeight pattern (the classic parallel-rib bar) sets perpendicular transverse ribs, a Crescent pattern
// (the modern hot-rolled sichel two-series bar) sets inclined ribs. The pattern is the RebarSection.Ribs argument; modern
// deformed bar defaults to Crescent.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RibPattern {
    public static readonly RibPattern UniformHeight = new("uniform-height", inclinationDeg: 90.0);
    public static readonly RibPattern Crescent      = new("crescent",       inclinationDeg: 60.0);
    public double InclinationDeg { get; }   // β — the ISO 6935-2 §4.15 transverse-rib-to-axis angle
}

// EnGrade is the VividOrange.Materials EnRebarGrade an EN-bodied grade maps to (None for the ASTM/CSA bands that carry no
// EN equivalent), so RcSectionBuilder.Of can lower the matched EN grade to its EnRebarMaterial DATA and the EnYieldForceKn/
// EnUltimateForceKn projections can read EnRebarFactory.CreateLinearElastic/CreateBiLinear without re-keying f_yk. The
// RebarGrade.MinimumYieldMpa band stays the spec-nominal yield the schedule reads; the EN factory carries the RC strength.
// capacityId is the GRADED properties#MATERIAL_PROPERTY_CATALOGUE row whose Mechanical (YieldStrengthMpa = the grade's f_yk,
// YoungsModulusMpa = the ACI 318 §20.2.2.2 reinforcing-steel E_s 200 GPa) the structural-design seam reads by MaterialId — a
// per-grade steel.<designation> row (the EN bands -> steel.b500a/b/c, the ASTM/CSA bands -> their own steel.gr*/steel.*w row),
// NEVER the generic metal.steel structural-section baseline (E 210 GPa, f_y 235) a bar is not made of: the reinforcing-steel
// row carries the grade's real yield and the 200 GPa rebar modulus, so a #5 Gr60 bar reads 420 MPa / 200 GPa not 235 / 210.
// capacityId and appearanceId are INDEPENDENT MaterialId slots (the component#COMPONENT_OWNER two-slot law): the capacity row
// is the structural steel, the appearance row the rendered finish (metal.iron plain carbon, metal.steel weldable, an epoxy
// row for a coated bar) — neither derived from the other.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarGrade {
    public static readonly RebarGrade Gr40  = new("gr40",  minimumYieldMpa: 280.0, standard: RebarStandard.A615,    capacityId: "steel.gr40",  appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60  = new("gr60",  minimumYieldMpa: 420.0, standard: RebarStandard.A615,    capacityId: "steel.gr60",  appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr75  = new("gr75",  minimumYieldMpa: 520.0, standard: RebarStandard.A615,    capacityId: "steel.gr75",  appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr80  = new("gr80",  minimumYieldMpa: 550.0, standard: RebarStandard.A615,    capacityId: "steel.gr80",  appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60W = new("gr60w", minimumYieldMpa: 420.0, standard: RebarStandard.A706,    capacityId: "steel.gr60w", appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade Gr80W = new("gr80w", minimumYieldMpa: 550.0, standard: RebarStandard.A706,    capacityId: "steel.gr80w", appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G400W = new("400w",  minimumYieldMpa: 400.0, standard: RebarStandard.G30,     capacityId: "steel.400w",  appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G500W = new("500w",  minimumYieldMpa: 500.0, standard: RebarStandard.G30,     capacityId: "steel.500w",  appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade B500A = new("b500a", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, capacityId: "steel.b500a", appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500A));
    public static readonly RebarGrade B500B = new("b500b", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, capacityId: "steel.b500b", appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500B));
    public static readonly RebarGrade B500C = new("b500c", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, capacityId: "steel.b500c", appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500C));
    public double MinimumYieldMpa { get; }
    public RebarStandard Standard { get; }
    public string CapacityId { get; }   // the graded steel.<designation> Mechanical-row key (E_s 200 GPa + the grade f_yk) the structural seam reads — NEVER metal.steel
    public string AppearanceId { get; }
    public Option<EnRebarGrade> EnGrade { get; }   // Some only for the EN-bodied B500A/B/C bands the RC σ(ε) law + the EnRebarFactory projections read
    public bool Weldable => Standard.Weldable;
}

// The nominal-bar axis. Catalogue binds ONLY the EN-10080 H-series to its published BarDiameter (the diameter the
// VividOrange Rebar(IMaterial, BarDiameter) ctor consumes); the imperial/CSA bands carry None so RcSectionBuilder feeds a
// raw Length(NominalDiameterMm) (the exact non-EN nominal, never a catalogue approximation). The EN H6..H50 area/mass
// columns transcribe ISO 6935-2 Table 2 (A = 0.7854·d², w = 0.00617·d²); the imperial #3..#18 transcribe ASTM A615
// soft-metric; the CSA 10M..55M transcribe CSA G30.18.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BarSize {
    public static readonly BarSize No3  = new("#3",  nominalDiameterMm: 9.525,  nominalAreaMm2: 71.0,   unitWeightKgM: 0.560,  catalogue: None);
    public static readonly BarSize No4  = new("#4",  nominalDiameterMm: 12.700, nominalAreaMm2: 129.0,  unitWeightKgM: 0.994,  catalogue: None);
    public static readonly BarSize No5  = new("#5",  nominalDiameterMm: 15.875, nominalAreaMm2: 199.0,  unitWeightKgM: 1.552,  catalogue: None);
    public static readonly BarSize No6  = new("#6",  nominalDiameterMm: 19.050, nominalAreaMm2: 284.0,  unitWeightKgM: 2.235,  catalogue: None);
    public static readonly BarSize No7  = new("#7",  nominalDiameterMm: 22.225, nominalAreaMm2: 387.0,  unitWeightKgM: 3.042,  catalogue: None);
    public static readonly BarSize No8  = new("#8",  nominalDiameterMm: 25.400, nominalAreaMm2: 510.0,  unitWeightKgM: 3.973,  catalogue: None);
    public static readonly BarSize No9  = new("#9",  nominalDiameterMm: 28.651, nominalAreaMm2: 645.0,  unitWeightKgM: 5.060,  catalogue: None);
    public static readonly BarSize No10 = new("#10", nominalDiameterMm: 32.258, nominalAreaMm2: 819.0,  unitWeightKgM: 6.404,  catalogue: None);
    public static readonly BarSize No11 = new("#11", nominalDiameterMm: 35.814, nominalAreaMm2: 1006.0, unitWeightKgM: 7.907,  catalogue: None);
    public static readonly BarSize No14 = new("#14", nominalDiameterMm: 43.002, nominalAreaMm2: 1452.0, unitWeightKgM: 11.380, catalogue: None);
    public static readonly BarSize No18 = new("#18", nominalDiameterMm: 57.328, nominalAreaMm2: 2581.0, unitWeightKgM: 20.240, catalogue: None);
    public static readonly BarSize M10  = new("10M", nominalDiameterMm: 11.300, nominalAreaMm2: 100.0,  unitWeightKgM: 0.785,  catalogue: None);
    public static readonly BarSize M15  = new("15M", nominalDiameterMm: 16.000, nominalAreaMm2: 200.0,  unitWeightKgM: 1.570,  catalogue: None);
    public static readonly BarSize M20  = new("20M", nominalDiameterMm: 19.500, nominalAreaMm2: 300.0,  unitWeightKgM: 2.355,  catalogue: None);
    public static readonly BarSize M25  = new("25M", nominalDiameterMm: 25.200, nominalAreaMm2: 500.0,  unitWeightKgM: 3.925,  catalogue: None);
    public static readonly BarSize M30  = new("30M", nominalDiameterMm: 29.900, nominalAreaMm2: 700.0,  unitWeightKgM: 5.495,  catalogue: None);
    public static readonly BarSize M35  = new("35M", nominalDiameterMm: 35.700, nominalAreaMm2: 1000.0, unitWeightKgM: 7.850,  catalogue: None);
    public static readonly BarSize M45  = new("45M", nominalDiameterMm: 43.700, nominalAreaMm2: 1500.0, unitWeightKgM: 11.775, catalogue: None);
    public static readonly BarSize M55  = new("55M", nominalDiameterMm: 56.400, nominalAreaMm2: 2500.0, unitWeightKgM: 19.625, catalogue: None);
    // EN-10080 / ISO 6935-2 Table 2 — the H-bar series the B500A/B/C grades roll at, each keyed straight to its BarDiameter
    // catalogue value (D6..D50, the full enum the consumer binds) so an EN bar resolves its Rebar diameter from the published
    // catalogue, not a literal. D8/D14/D28/D32/D40/D50 are the catalogue members the prior H10/H12/H16/H20/H25-only set left unexploited.
    public static readonly BarSize H6   = new("H6",  nominalDiameterMm: 6.000,  nominalAreaMm2: 28.3,   unitWeightKgM: 0.222,  catalogue: Some(BarDiameter.D6));
    public static readonly BarSize H8   = new("H8",  nominalDiameterMm: 8.000,  nominalAreaMm2: 50.3,   unitWeightKgM: 0.395,  catalogue: Some(BarDiameter.D8));
    public static readonly BarSize H10  = new("H10", nominalDiameterMm: 10.000, nominalAreaMm2: 78.5,   unitWeightKgM: 0.617,  catalogue: Some(BarDiameter.D10));
    public static readonly BarSize H12  = new("H12", nominalDiameterMm: 12.000, nominalAreaMm2: 113.0,  unitWeightKgM: 0.888,  catalogue: Some(BarDiameter.D12));
    public static readonly BarSize H14  = new("H14", nominalDiameterMm: 14.000, nominalAreaMm2: 154.0,  unitWeightKgM: 1.210,  catalogue: Some(BarDiameter.D14));
    public static readonly BarSize H16  = new("H16", nominalDiameterMm: 16.000, nominalAreaMm2: 201.0,  unitWeightKgM: 1.580,  catalogue: Some(BarDiameter.D16));
    public static readonly BarSize H20  = new("H20", nominalDiameterMm: 20.000, nominalAreaMm2: 314.0,  unitWeightKgM: 2.470,  catalogue: Some(BarDiameter.D20));
    public static readonly BarSize H25  = new("H25", nominalDiameterMm: 25.000, nominalAreaMm2: 491.0,  unitWeightKgM: 3.850,  catalogue: Some(BarDiameter.D25));
    public static readonly BarSize H28  = new("H28", nominalDiameterMm: 28.000, nominalAreaMm2: 616.0,  unitWeightKgM: 4.840,  catalogue: Some(BarDiameter.D28));
    public static readonly BarSize H32  = new("H32", nominalDiameterMm: 32.000, nominalAreaMm2: 804.0,  unitWeightKgM: 6.310,  catalogue: Some(BarDiameter.D32));
    public static readonly BarSize H40  = new("H40", nominalDiameterMm: 40.000, nominalAreaMm2: 1257.0, unitWeightKgM: 9.860,  catalogue: Some(BarDiameter.D40));
    public static readonly BarSize H50  = new("H50", nominalDiameterMm: 50.000, nominalAreaMm2: 1964.0, unitWeightKgM: 15.420, catalogue: Some(BarDiameter.D50));
    public double NominalDiameterMm { get; }
    public double NominalAreaMm2 { get; }
    public double UnitWeightKgM { get; }
    public Option<BarDiameter> Catalogue { get; }   // Some -> RcSectionBuilder feeds the catalogued BarDiameter to a Rebar; None -> a raw Length(NominalDiameterMm)
}

// The ACI 318-19 §25.3 bend-table discriminant — the FIX for the prior single BarSize.BendFactor that conflated Table
// 25.3.1 (development of deformed bars in tension) with 25.3.2 (stirrups/ties). MinInsideBendFactor returns the minimum
// inside bend-diameter multiple (×d_b) by table × bar band: development is 6·d_b for #3..#8 (d_b <= 25.4 mm), 8·d_b for
// #9..#11 (<= 36 mm), 10·d_b for #14/#18; stirrup-tie and seismic are 4·d_b for #3..#5 (d_b <= 16 mm) and 6·d_b for #6..#8 —
// so a #5 stirrup bends at 4·d_b, never the 6·d_b a development bar uses. The band thresholds (25.4/36/16 mm) are the
// bar-diameter boundaries of the ACI size groups.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HookKind {
    public static readonly HookKind Development = new("development", aciTable: "ACI 318-19 Table 25.3.1");
    public static readonly HookKind StirrupTie  = new("stirrup-tie", aciTable: "ACI 318-19 Table 25.3.2");
    public static readonly HookKind Seismic     = new("seismic",     aciTable: "ACI 318-19 Table 25.3.4");
    public string AciTable { get; }
    public double MinInsideBendFactor(double barDiameterMm) =>
        this == Development ? (barDiameterMm <= 25.4 ? 6.0 : barDiameterMm <= 36.0 ? 8.0 : 10.0)
                            : (barDiameterMm <= 16.0 ? 4.0 : 6.0);
}

// The BS 8666:2020 schedule SHAPE-CODE set (the bar-bending-schedule designation the IfcReinforcingBar BendingShapeCode wire
// carries). Legs is the count of straight legs (the A..F dimension letters in the BS 8666 cutting-length formula) and Link
// is true for the closed-perimeter shapes (the rectangular/circular stirrups + the helix) — the topology the host shape-table
// lofts the polyline from; the cutting-length formula itself is the host shape-table's, this owner carrying the token + topology.
// Code00 straight, Code11 single 90° bend (L-bar), Code12 180° semicircular hook, Code13 angled end hook, Code51 the standard
// closed rectangular link, Code67 a radiused arc, Code75 a circular link, Code77 a helix (C turns), Code99 the non-standard
// fully-dimensioned shape (Legs 0 signalling a drawn sketch).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShapeCode {
    public static readonly ShapeCode Code00 = new("00", legs: 1, link: false);
    public static readonly ShapeCode Code01 = new("01", legs: 1, link: false);
    public static readonly ShapeCode Code11 = new("11", legs: 2, link: false);
    public static readonly ShapeCode Code12 = new("12", legs: 2, link: false);
    public static readonly ShapeCode Code13 = new("13", legs: 3, link: false);
    public static readonly ShapeCode Code14 = new("14", legs: 2, link: false);
    public static readonly ShapeCode Code15 = new("15", legs: 2, link: false);
    public static readonly ShapeCode Code21 = new("21", legs: 3, link: false);
    public static readonly ShapeCode Code22 = new("22", legs: 4, link: false);
    public static readonly ShapeCode Code23 = new("23", legs: 3, link: false);
    public static readonly ShapeCode Code24 = new("24", legs: 3, link: false);
    public static readonly ShapeCode Code25 = new("25", legs: 3, link: false);
    public static readonly ShapeCode Code26 = new("26", legs: 3, link: false);
    public static readonly ShapeCode Code27 = new("27", legs: 3, link: false);
    public static readonly ShapeCode Code28 = new("28", legs: 3, link: false);
    public static readonly ShapeCode Code29 = new("29", legs: 3, link: false);
    public static readonly ShapeCode Code31 = new("31", legs: 4, link: false);
    public static readonly ShapeCode Code32 = new("32", legs: 4, link: false);
    public static readonly ShapeCode Code33 = new("33", legs: 3, link: true);
    public static readonly ShapeCode Code34 = new("34", legs: 4, link: false);
    public static readonly ShapeCode Code35 = new("35", legs: 4, link: false);
    public static readonly ShapeCode Code36 = new("36", legs: 4, link: false);
    public static readonly ShapeCode Code41 = new("41", legs: 5, link: false);
    public static readonly ShapeCode Code44 = new("44", legs: 5, link: false);
    public static readonly ShapeCode Code46 = new("46", legs: 5, link: false);
    public static readonly ShapeCode Code47 = new("47", legs: 4, link: true);
    public static readonly ShapeCode Code48 = new("48", legs: 4, link: true);
    public static readonly ShapeCode Code51 = new("51", legs: 4, link: true);
    public static readonly ShapeCode Code52 = new("52", legs: 4, link: true);
    public static readonly ShapeCode Code56 = new("56", legs: 5, link: false);
    public static readonly ShapeCode Code63 = new("63", legs: 5, link: true);
    public static readonly ShapeCode Code64 = new("64", legs: 6, link: false);
    public static readonly ShapeCode Code67 = new("67", legs: 1, link: false);
    public static readonly ShapeCode Code75 = new("75", legs: 2, link: true);
    public static readonly ShapeCode Code77 = new("77", legs: 1, link: true);
    public static readonly ShapeCode Code98 = new("98", legs: 5, link: false);
    public static readonly ShapeCode Code99 = new("99", legs: 0, link: false);
    public int Legs { get; }
    public bool Link { get; }
}

// The ACI 318-19 standard end-hook angles, each carrying its straight-extension factor (×d_b) and the absolute floor the
// host applies (Table 25.3.1: a 180° development-hook tail >= 65 mm; Table 25.3.2/25.3.4: a 135° stirrup/seismic-hook tail
// >= 75 mm; the 90° development hook has no floor), plus the BS 8666:2020 ShapeCode a longitudinal bar with that hook schedules
// as (90°->11 L-bar, 135°->13 angled hook, 180°->12 semicircular). A RebarUsage.Stirrup bar overrides to Code51 (closed link).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarHook {
    public static readonly RebarHook Ninety        = new("90",  bendDegrees: 90.0,  extensionFactor: 12.0, minExtensionMm: 0.0,  shape: ShapeCode.Code11);
    public static readonly RebarHook OneThirtyFive = new("135", bendDegrees: 135.0, extensionFactor: 6.0,  minExtensionMm: 75.0, shape: ShapeCode.Code13);
    public static readonly RebarHook OneEighty     = new("180", bendDegrees: 180.0, extensionFactor: 4.0,  minExtensionMm: 65.0, shape: ShapeCode.Code12);
    public double BendDegrees { get; }
    public double ExtensionFactor { get; }
    public double MinExtensionMm { get; }
    public ShapeCode Shape { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ISO 6935-2 rib-deformation receipt the host materializes onto the bar surface and the IfcReinforcingBar bond reads.
// RelativeRibArea fR is the GOVERNING bond invariant (the ISO 6935-2 / EN 10080 minimum by diameter band); the transverse-rib
// height/spacing + longitudinal-rib height + the flank inclination α + the rib-to-axis inclination β are the rib form;
// RiblessPerimeterFraction Σf_i is the ASTM A615 §7.4 gap fraction (the un-ribbed perimeter the longitudinal ribs span).
public readonly record struct RebarRibGeometry(
    double TransverseRibHeightMm,
    double TransverseRibSpacingMm,
    double LongitudinalRibHeightMm,
    double FlankInclinationDeg,
    double RibInclinationDeg,
    double RelativeRibArea,
    double RiblessPerimeterFraction,
    RibPattern Pattern);

// The host-neutral bend receipt: the ACI 318 §25.3 minimum inside bend diameter + straight hook extension + the EN 1992 §8.3
// mandrel (former) diameter the host curve-materializes, PLUS the BS 8666:2020 ShapeCode the IfcReinforcingBar BendingShapeCode
// wire reads. MandrelDiameterMm is the bending-former diameter (the value VividOrange Link.MinimumMandrelDiameter computes for
// the link); InsideBendDiameterMm is the ACI development/stirrup hook bend diameter (the two distinct because the EN former rule
// and the ACI hook rule differ — the former 4·d/7·d, the ACI bend 4..10·d by HookKind × band).
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm, double MandrelDiameterMm, ShapeCode Shape);

public readonly record struct RebarSection(
    BarSize Size,
    RebarGrade Grade,
    RebarUsage Usage,
    RebarSurface Surface,
    PositiveMagnitude DiameterMm,
    PositiveMagnitude NominalAreaMm2,
    PositiveMagnitude UnitWeightKgM) {

    // The FINISH the appearance projection reads (metal.iron for plain carbon A615, metal.steel for low-alloy/weldable),
    // INDEPENDENT from CapacityKey: a coated bar keeps its graded CapacityKey while its AppearanceId names the epoxy-coat row.
    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);

    // The CAPACITY material whose Properties/properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row (YieldStrengthMpa/YoungsModulusMpa)
    // the structural-design seam reads — the bar's GRADED reinforcing STEEL, the per-grade steel.<designation> row (the EN bands
    // steel.b500a/b/c, the ASTM/CSA bands steel.gr*/steel.*w) carrying the grade's f_yk AND the ACI 318 §20.2.2.2 reinforcing
    // modulus E_s 200 GPa, sourced INDEPENDENTLY from the AppearanceId finish (the component#COMPONENT_OWNER two-slot independence
    // law: a coated bar's capacity steel and its coat finish are distinct MaterialIds, neither derived from the other). NEVER
    // metal.steel — the generic structural-section baseline (E 210 GPa, f_y 235) is the wrong physics for a 500/420 MPa rebar,
    // so a graph.MechanicalOf read off the projected bar resolves the grade row, not the section baseline.
    public MaterialId CapacityKey => MaterialId.Of(Grade.CapacityId);

    // SPEC-NOMINAL bar tensile-yield force — a TOTAL double projection over the section's OWN spec-nominal grade band, NEVER a
    // base-metal re-derivation. The MaterialId-keyed Mechanical receipt (the measured YieldStrengthMpa) is the design seam's,
    // read once at the federation, never threaded into the section.
    public double YieldForceKn => Grade.MinimumYieldMpa * NominalAreaMm2.Value * 1e-3;

    // EN-REGISTERED design yield + ductility-class ultimate (Some only for the EN-bodied B500A/B/C bands) — the development/lap/
    // seismic-overstrength capacity-design seam reads the f_yd from EnRebarFactory.CreateLinearElastic(grade).Strength and the
    // k·f_yk ultimate from CreateBiLinear(grade).UltimateStrength (the EN ductility k = 1.05 class A / 1.08 class B / 1.15 class C),
    // lowered from the registered grade rather than a hand-keyed f_yk/f_u. The spec-nominal YieldForceKn stays the schedule receipt.
    public Option<double> EnYieldForceKn => Grade.EnGrade.Map(g => EnRebarFactory.CreateLinearElastic(g).Strength.Megapascals * NominalAreaMm2.Value * 1e-3);
    public Option<double> EnUltimateForceKn => Grade.EnGrade.Map(g => EnRebarFactory.CreateBiLinear(g).UltimateStrength.Megapascals * NominalAreaMm2.Value * 1e-3);

    // The ISO 6935-2 rib geometry the host materializes — Some for a Textured (ribbed) bar, None for a Plain round. fR is the
    // ISO 6935-2 / EN 10080 minimum relative rib area by diameter band (the bond invariant); the spacing cap 0.7·d and the
    // ribless-perimeter fraction 0.125 are ASTM A615 §7.4 maxima; the rib height 0.05·d the ASTM A615 Table 1 min-avg-height/d
    // ratio; the flank inclination 45° the ISO 6935-2 §4.14 minimum; β the RibPattern's ISO 6935-2 §4.15 rib-to-axis angle.
    public Option<RebarRibGeometry> Ribs(RibPattern pattern) {
        if (!Surface.Ribbed) return None;
        double d = DiameterMm.Value;
        double fR = d <= 6.0 ? 0.035 : d <= 12.0 ? 0.040 : 0.056;
        return Some(new RebarRibGeometry(
            TransverseRibHeightMm:    0.05 * d,
            TransverseRibSpacingMm:   0.7 * d,
            LongitudinalRibHeightMm:  0.05 * d,
            FlankInclinationDeg:      45.0,
            RibInclinationDeg:        pattern.InclinationDeg,
            RelativeRibArea:          fR,
            RiblessPerimeterFraction: 0.125,
            Pattern:                  pattern));
    }

    // The ACI 318 §25.3 standard hook PLUS the EN 1992 §8.3 mandrel PLUS the BS 8666:2020 ShapeCode — the Usage selects a closed
    // link (Code51) over an end hook so a Ligature stirrup schedules as a closed perimeter, a Main bar as the angle-keyed end hook.
    // The bend diameter splits by HookKind (a #5 stirrup at 4·d_b, a #5 development bar at 6·d_b); the mandrel diameter is the
    // EN former rule (4·d for d <= 16 mm, 7·d above — VividOrange Link.MinimumMandrelDiameter parity); the extension is floored
    // at hook.MinExtensionMm (65 mm for the 180° development hook, 75 mm for the 135° seismic/stirrup hook).
    public Fin<RebarBend> StandardHook(HookKind kind, RebarHook hook, Op key) =>
        from valid in guard(hook.ExtensionFactor > 0.0, ComponentFault.Dimension(key, $"<degenerate-hook:{Grade.Key}:{Size.Key}>"))
        let d = DiameterMm.Value
        let insideBendDiameterMm = kind.MinInsideBendFactor(d) * d
        let hookExtensionMm = Math.Max(hook.ExtensionFactor * d, hook.MinExtensionMm)
        let mandrelDiameterMm = (d <= 16.0 ? 4.0 : 7.0) * d
        select new RebarBend(hook.BendDegrees, insideBendDiameterMm, hookExtensionMm, mandrelDiameterMm, Usage.Stirrup ? ShapeCode.Code51 : hook.Shape);
}

public readonly record struct RebarRow(string Designation, string Size, string Grade, string Usage, string Surface);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // designation · size · grade · usage · surface — the ASTM A615/A706, CSA G30.18, and EN-10080 reinforcing-bar seed.
    // The usage column keys the IfcReinforcingBarTypeEnum role (main longitudinal vs ligature stirrup vs shear leg) and routes
    // the RcSection placement; the surface column the IfcReinforcingBarSurfaceEnum bond surface (deformed default, plain for ties).
    // The EN-10080 rows exercise the B500A/B/C ductility classes (the CreateBiLinear k = 1.05/1.08/1.15 branches) and the new
    // EN H8/H14/H28/H32/H40/H50 sizes keyed to BarDiameter.D8/D14/D28/D32/D40/D50.
    static readonly Seq<RebarRow> RebarRows = Seq(
        new RebarRow("reinforcement.rebar-no3-gr60",      "#3",  "gr60",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no4-gr60",      "#4",  "gr60",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no4-gr60-tie",  "#4",  "gr60",  "ligature", "textured"),
        new RebarRow("reinforcement.rebar-no5-gr60",      "#5",  "gr60",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no6-gr60",      "#6",  "gr60",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no7-gr75",      "#7",  "gr75",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no8-gr75",      "#8",  "gr75",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no9-gr80",      "#9",  "gr80",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no11-gr80",     "#11", "gr80",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no18-gr80",     "#18", "gr80",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-no5-gr60w",     "#5",  "gr60w", "main",     "textured"),
        new RebarRow("reinforcement.rebar-no8-gr80w",     "#8",  "gr80w", "main",     "textured"),
        new RebarRow("reinforcement.rebar-10m-400w",      "10M", "400w",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-10m-400w-tie",  "10M", "400w",  "ligature", "textured"),
        new RebarRow("reinforcement.rebar-15m-400w",      "15M", "400w",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-25m-500w",      "25M", "500w",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-35m-500w",      "35M", "500w",  "main",     "textured"),
        new RebarRow("reinforcement.rebar-h8-b500a-link", "H8",  "b500a", "ligature", "plain"),
        new RebarRow("reinforcement.rebar-h12-b500b",     "H12", "b500b", "main",     "textured"),
        new RebarRow("reinforcement.rebar-h14-b500b",     "H14", "b500b", "main",     "textured"),
        new RebarRow("reinforcement.rebar-h16-b500c",     "H16", "b500c", "main",     "textured"),
        new RebarRow("reinforcement.rebar-h25-b500c",     "H25", "b500c", "main",     "textured"),
        new RebarRow("reinforcement.rebar-h32-b500c",     "H32", "b500c", "main",     "textured"),
        new RebarRow("reinforcement.rebar-h40-b500c",     "H40", "b500c", "main",     "textured"));

    // The grade's spec body lowered to the component#COMPONENT_OWNER ComponentStandard the Component.Of admission requires —
    // the bounded ComponentAuthority + region, StandardJointThicknessMm 0.0 (a reinforcing bar has no mortar joint, the SAME
    // as the steel/timber profiled members). ASTM A615/A706 -> Astm/us, CSA G30.18 -> Csa/ca, EN-10080 -> En/eu.
    static ComponentStandard StandardOf(RebarGrade grade) => grade.Standard == RebarStandard.En10080
        ? new ComponentStandard("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En)
        : grade.Standard == RebarStandard.G30
            ? new ComponentStandard("ca", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Csa)
            : new ComponentStandard("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    static Fin<Component> RebarOf(RebarRow r, Context context, Op key) =>
        from size in BarSize.TryGet(r.Size, out BarSize? s) ? Fin.Succ(s!) : Fin.Fail<BarSize>(ComponentFault.Designation(key, $"<unknown-bar-size:{r.Size}>"))
        from grade in RebarGrade.TryGet(r.Grade, out RebarGrade? g) ? Fin.Succ(g!) : Fin.Fail<RebarGrade>(ComponentFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from usage in RebarUsage.TryGet(r.Usage, out RebarUsage? u) ? Fin.Succ(u!) : Fin.Fail<RebarUsage>(ComponentFault.Designation(key, $"<unknown-usage:{r.Usage}>"))
        from surface in RebarSurface.TryGet(r.Surface, out RebarSurface? f) ? Fin.Succ(f!) : Fin.Fail<RebarSurface>(ComponentFault.Designation(key, $"<unknown-surface:{r.Surface}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalDiameterMm)
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalAreaMm2)
        from weight in key.AcceptValidated<PositiveMagnitude>(candidate: size.UnitWeightKgM)
        let section = new RebarSection(size, grade, usage, surface, diameter, area, weight)
        from item in Component.Of(ComponentFamily.Reinforcement, r.Designation, new ComponentSection.Reinforcement(section), Coring.None, StandardOf(grade), section.CapacityKey, section.AppearanceId, key)
        select item;

    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO explicit
    // comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a type mismatch
    // on the ComponentId Component.Designation key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildRebarRows(Context context) =>
        RebarRows
            .Choose(row => RebarOf(row, context, default).ToOption())
            .ToFrozenDictionary(static c => c.Designation, static c => c);
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the host-neutral reinforced-concrete-section assembler over the `VividOrange.Sections` `ConcreteSection`; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the static EN-grade admission boundary lowering `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` derivation throws onto the typed `ComponentFault` rail; `RcSectionBuilder.Of` the one builder lifting the `IConcreteSection` the `capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`), `FaceSpacing` (max-spacing bars on a face — `ReinforcementLayoutBySpacing` + `FaceReinforcementLayer`), `PerimeterCount` (n bars round the whole section — `ReinforcementLayoutByCount` + `PerimeterReinforcementLayer`, no face), `PerimeterSpacing` (max-spacing perimeter bars — `ReinforcementLayoutBySpacing` + `PerimeterReinforcementLayer`, no face)} — the face cases over the `VividOrange.Sections` `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`, NO `Perimeter` member — perimeter distribution is the separate `PerimeterReinforcementLayer` engine, never a `SectionFace` value), the perimeter cases carrying no face; a rebar arrangement is a `RebarLayout` case, never a per-engine constructor scattered at the call site; a stirrup is the `Link` admitted once with its `MinimumMandrelDiameter`, the EC2 clear spacing the one `MinimumReinforcementSpacing` rule.
- Entry: `public static Fin<RcSection> Of(Component concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the `concreteGrade` through `EnGrade.Concrete(concreteGrade, annex, key)` to an `EnConcreteMaterial`, the `barGrade.EnGrade` through `EnGrade.Rebar(...)` to an `EnRebarMaterial` (a non-EN `barGrade.EnGrade == None` railing `ComponentFault.Grade`), builds the `ConcreteSection` from the FAMILY-AGNOSTIC `component#COMPONENT_OWNER` `ParametricSection.ProfileOf(concrete.GrossRectangleMm.WidthMm.Value, concrete.GrossRectangleMm.DepthMm.Value, key)` `IProfile` perimeter (the decoupled `(widthMm, depthMm)` admission over the Component's gross bounding rectangle) + the concrete `IMaterial` + a `Link` (the `linkSize.Catalogue` `BarDiameter` over the rebar `IMaterial`) + the `coverMm` `Length`, folds each `RebarLayout` case to its `FaceReinforcementLayer`/`PerimeterReinforcementLayer` and `AddRebarLayer`s it, and reads back `section.Rebars` — the `IConcreteSection` the elastic transformed-section (`ConcreteSectionProperties`) and ultimate N-M-M (`InteractionDiagram`) solvers at `capacity#SECTION_CAPACITY` consume; `public Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarSize size, double maxAggregateMm, Op key)` reads the EC2 `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing` clear-spacing rule, one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`/`Section`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`, `MinimumReinforcementSpacing`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial` grade DATA, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` derivation throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex` the grade cites; `.api/api-vividorange-standards.md`), VividOrange.Profiles + Geometry (the `IProfile` the `ConcreteSection` consumes, via the `component#COMPONENT_OWNER` `ParametricSection`), UnitsNet (`Length` cover/diameter at the edge), Rasm (project — `PositiveMagnitude` from `Rasm.Vectors`), LanguageExt.Core (`Fin`/`Seq`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `RebarLayout`).
- Growth: a new rebar arrangement is one `RebarLayout` `[Union]` case binding its `VividOrange.Sections` layout engine, a new section face one `SectionFace` band the `VividOrange` floor enumerates, a new constitutive concrete law a `capacity#SECTION_CAPACITY` concern reading the same `EnConcreteMaterial` — never a per-arrangement RC builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a raw-`Length` bar diameter where the `BarDiameter` catalogue enumerates it; the EN grade DATA admission is the ONE boundary, the capacity computation the `capacity#SECTION_CAPACITY` owner over the `IConcreteSection` minted here.
- Boundary: `RcSectionBuilder.Of` is the BOUNDARY_ADMISSION point where the `VividOrange.Sections`/`Materials` exception-throwing surface is admitted EXACTLY ONCE — the `EnConcreteMaterial`/`EnRebarMaterial` ctors throw `ArgumentException` (unknown grade) / `MissingNationalAnnexException` (untabulated annex) and the `ConcreteSection`/layout construction throws `InvalidMaterialTypeException`/`InvalidProfileTypeException`, all trapped at THIS boundary and lowered onto the typed `ComponentFault.Grade` (the EN-grade derivation) / `ComponentFault.Section` (the section assembly) rail (the `.api` `[BOUNDARY_EXCEPTION_LAW]`), so no `VividOrange` throw and no `null` reaches an interior signature and the `IConcreteSection` egress carries only validated DATA — `ComponentFault.Capacity` is RESERVED for the `capacity#SECTION_CAPACITY` N-M-M/elastic SOLVE, the section-assembly fault `ComponentFault.Section`; the `barGrade` binds to its `EnRebarGrade` through `RebarGrade.EnGrade` so a `Rebar` carries a registered EN grade rather than a hand-keyed `f_yk` (the spec-nominal `MinimumYieldMpa` band stays the schedule receipt, the EN grade DATA the RC-capacity strength), and a metric `BarSize.Catalogue` resolves a `Rebar(IMaterial, BarDiameter)` over the EN-10080 catalogue rather than a raw `Length`; the four `VividOrange.Sections` layout-engine constructors collapse into the one `RebarLayout` `[Union]` the `Of` fold dispatches — the 4+-parallel-constructor collapse trigger — so a bar arrangement is a `RebarLayout` case never a scattered `new FaceReinforcementLayer(...)`, the generated `[Union]` `Switch` the totality proof a fifth case breaks at compile time; the EC2 clear spacing reads the `MinimumReinforcementSpacing(annex)` rule, never an inline EC2 constant; the assembled `IConcreteSection` is host-neutral DATA (the `VividOrange.Geometry` `ILocalPoint2d` bar positions, never a `Rhino.Geometry` type) the `capacity#SECTION_CAPACITY` solvers consume and the `VividOrange.Serialization` round-trip persists inside the C# layer; `RcSectionBuilder.Of` admits ANY `component#COMPONENT_OWNER` `Component` as its concrete outline because `ParametricSection.ProfileOf` is FAMILY-AGNOSTIC — it reads the `Component.GrossRectangleMm` `(WidthMm, DepthMm)` bounding pair into the rectangle `IProfile` regardless of `ComponentFamily` — so a `cmu#CMU_CELL_LATTICE` grouted CMU unit (a `ComponentFamily.Cmu` `Component` whose fully-grouted net solid IS its gross width×height rectangle) admits as the reinforced-masonry concrete input the SAME way the parametric concrete outline does, a TMS 402 reinforced CMU shear wall resolving its reinforcement transform through this ONE `RcSectionBuilder.Of` boundary (its grout `EnConcreteGrade` the section concrete) — no cmu-specific RC builder; the RC section is NOT a `Component` — a `Component` is one discrete rebar in the schedule, the `RcSection` the populated concrete member the rebar reinforces, the two meeting at the `BarSize`/`RebarGrade` vocabulary this page owns; the `RcSection.ConcreteProfile` field carries the source `Component` (the QTO key + the gross depth/width the `capacity#SECTION_CAPACITY` `RcElastic` fibre lever reads off `Component.GrossRectangleMm`).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
// Same Rasm.Materials.Component.Reinforcement sub-namespace as the section-02 fence (RcSection/RebarLayout/EnGrade are
// reinforcement-family types); composes the section-02 prelude (LanguageExt, Rasm.Vectors, Rasm.Domain, Thinktecture,
// static Prelude) plus the VividOrange RC surface below and the parent Rasm.Materials.Component ComponentFault rail.
using Rasm.Materials.Component;                      // Component, ComponentFault, ParametricSection (the COMPONENT_OWNER concrete-outline owner ProfileOf lowers to IProfile)
using VividOrange.Sections;                          // ConcreteSection, Section, SectionFace, IConcreteSection
using VividOrange.Sections.Reinforcement;            // Rebar, Link, FaceReinforcementLayer, PerimeterReinforcementLayer, MinimumReinforcementSpacing, BarDiameter, IReinforcementLayer
using VividOrange.Materials.StandardMaterials.En;    // EnConcreteMaterial, EnRebarMaterial, EnConcreteGrade
using VividOrange.Profiles;                          // IProfile (the concrete-outline perimeter)
using VividOrange.Standards.Eurocode;                // NationalAnnex citation context (En1992 the grade carries)
using UnitsNet;                                      // Length (cover / diameter at the edge)

// --- [MODELS] ------------------------------------------------------------------------------
// One RebarLayout [Union] collapses the four VividOrange.Sections layout-engine constructors — face/perimeter ×
// count/spacing — each case carrying its own layout-strategy arguments, never four scattered `new ...Layer(...)` sites.
[Union]
public abstract partial record RebarLayout {
    private RebarLayout() { }
    public sealed record FaceCount(SectionFace Face, BarSize Bar, int Count) : RebarLayout;
    public sealed record FaceSpacing(SectionFace Face, BarSize Bar, double MaxSpacingMm) : RebarLayout;
    public sealed record PerimeterCount(BarSize Bar, int Count) : RebarLayout;
    public sealed record PerimeterSpacing(BarSize Bar, double MaxSpacingMm) : RebarLayout;
}

// The reinforced-concrete section receipt: the assembled VividOrange.Sections IConcreteSection plus the resolved EN
// concrete/rebar grade DATA, the gross steel area the capacity#SECTION_CAPACITY solvers and the QTO seam read (never a
// re-derived bar-area sum), and the source Component the concrete outline came from (the QTO key + the gross-dimension lever).
public sealed record RcSection(IConcreteSection Section, EnConcreteMaterial Concrete, EnRebarMaterial Rebar, double GrossSteelAreaMm2, double CoverMm) {
    public Component ConcreteProfile { get; init; }   // the source concrete-outline Component (QTO key; capacity#SECTION_CAPACITY reads its GrossRectangleMm gross depth/width)
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary: the VividOrange.Materials grade ctors throw on an unknown grade / untabulated annex
// (the .api [BOUNDARY_EXCEPTION_LAW]); EnGrade traps the throw ONCE and lowers it onto the typed ComponentFault.Grade rail,
// so no VividOrange throw and no non-EN grade reaches the RcSectionBuilder.Of interior.
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        Try(() => new EnConcreteMaterial(grade, annex)).ToFin()
            .MapFail(e => ComponentFault.Grade(key, $"<en-concrete-grade:{grade}:{annex}:{e.Message}>"));

    public static Fin<EnRebarMaterial> Rebar(Option<EnRebarGrade> grade, NationalAnnex annex, Op key) =>
        grade.Match(
            Some: g => Try(() => new EnRebarMaterial(g, annex)).ToFin()
                .MapFail(e => ComponentFault.Grade(key, $"<en-rebar-grade:{g}:{annex}:{e.Message}>")),
            None: () => Fin.Fail<EnRebarMaterial>(ComponentFault.Grade(key, "<rebar-grade-not-en-bodied-for-rc-section>")));
}

public static class RcSectionBuilder {
    // The ONE reinforced-section boundary: lower the EN grades, build the ConcreteSection from the Component's IProfile +
    // concrete IMaterial + a Link + cover, fold each RebarLayout to its placement engine and AddRebarLayer it, read back
    // section.Rebars. Every VividOrange throw (InvalidMaterialTypeException/InvalidProfileTypeException/ArgumentException)
    // is trapped here onto ComponentFault.Section — the IConcreteSection egress carries only validated DATA.
    public static Fin<RcSection> Of(Component concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade.EnGrade, annex, key)
        from profile in ParametricSection.ProfileOf(concrete.GrossRectangleMm.WidthMm.Value, concrete.GrossRectangleMm.DepthMm.Value, key)   // component#COMPONENT_OWNER gross-rectangle IProfile perimeter (the decoupled (widthMm, depthMm) admission)
        from built in Try(() => Build(profile, concreteMaterial, rebarMaterial, linkSize, layout, coverMm)).ToFin()
            .MapFail(e => ComponentFault.Section(key, $"<rc-section-build:{concrete.Family.Key}:{e.Message}>"))
        let area = built.Rebars.Sum(r => Math.PI * 0.25 * r.Rebar.Diameter.Millimeters * r.Rebar.Diameter.Millimeters * r.CountPerBundle)
        select new RcSection(built, concreteMaterial, rebarMaterial, area, coverMm) { ConcreteProfile = concrete };

    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarSize linkSize, Seq<RebarLayout> layout, double coverMm) {
        Link link = new(rebar, LinkDiameter(linkSize, rebar));
        ConcreteSection section = new(profile, concrete, link, Length.FromMillimeters(coverMm));
        layout.Iter(l => section.AddRebarLayer(LayerOf(l, rebar)));
        return section;
    }

    // Each RebarLayout case -> its VividOrange.Sections placement engine; an EN BarSize feeds the catalogued BarDiameter ctor,
    // an imperial/CSA BarSize a raw Length(NominalDiameterMm) — one Rebar per layout, never a literal. The generated [Union]
    // Switch is the totality proof: a fifth RebarLayout case breaks this arm at compile time, never a runtime-silent `_`.
    static IReinforcementLayer LayerOf(RebarLayout layout, EnRebarMaterial rebar) => layout.Switch(
        faceCount:        c => new FaceReinforcementLayer(c.Face, RebarOf(c.Bar, rebar), c.Count),
        faceSpacing:      s => new FaceReinforcementLayer(s.Face, RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)),
        perimeterCount:   c => (IReinforcementLayer)new PerimeterReinforcementLayer(RebarOf(c.Bar, rebar), c.Count),
        perimeterSpacing: s => new PerimeterReinforcementLayer(RebarOf(s.Bar, rebar), Length.FromMillimeters(s.MaxSpacingMm)));

    static Rebar RebarOf(BarSize size, EnRebarMaterial rebar) =>
        size.Catalogue.Match(Some: d => new Rebar(rebar, d), None: () => new Rebar(rebar, Length.FromMillimeters(size.NominalDiameterMm)));

    static BarDiameter LinkDiameter(BarSize size, EnRebarMaterial rebar) => size.Catalogue.IfNone(BarDiameter.D8);

    // The EC2 clear bar-spacing rule for the annex + bar diameter, read from MinimumReinforcementSpacing rather than an
    // inline EC2 constant; the maxAggregateMm tunes the +(d_g) term the EC2 rule adds to the bar-diameter governed spacing.
    public static Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarSize size, double maxAggregateMm, Op key) =>
        Try(() => new MinimumReinforcementSpacing(annex).GetMinimumReinforcementSpacing(Length.FromMillimeters(size.NominalDiameterMm)).Millimeters).ToFin()
            .MapFail(e => ComponentFault.Section(key, $"<min-bar-spacing:{annex}:{size.Key}:{e.Message}>"));
}
```

## [04]-[RESEARCH]

- [REBAR_ROW_TRANSCRIPTION]: REALIZED — the ASTM A615/A706 + CSA G30.18 + EN-10080 reinforcing-bar catalogue (the #3..#18 imperial, 10M..55M CSA-metric, and H6..H50 EN nominal columns, the A615 Gr40/60/75/80, A706 Gr60/80 weldable, CSA 400W/500W weldable, and EN B500A/B500B/B500C ductility-class grades) seeds through `ComponentCatalogue.BuildRebarRows(context)` over the `RebarRow` designation/size/grade/usage/surface table, the nominal-diameter/area/weight columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the grade/size/usage/surface algebra realized as the reinforcement vocabulary; a new bar is one `RebarRow` data addition plus, if novel, one `BarSize`/`RebarGrade`/`RebarUsage` row. The EN H6..H50 nominal diameter/area/unit-weight columns transcribe ISO 6935-2 Table 2 (`A = 0.7854·d²`, `w = 0.00617·d²` kg/m); the imperial transcribe ASTM A615 soft-metric, the CSA-metric CSA G30.18. A non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`. The `BarSize.Catalogue` binds ONLY the EN-10080 H-series to its published `BarDiameter` (the `VividOrange.Sections` `Rebar(IMaterial, BarDiameter)` ctor diameter); the imperial/CSA bands carry `None` so the exact non-EN nominal feeds a raw `Length`, never a catalogue approximation. The id prefix is the family token `reinforcement.<designation>` (the unified `ComponentId` `family.designation` format, the literal `connection.` prefix retired).
- [REBAR_BEND_SCHEDULE_SPLIT]: REALIZED — the prior single `BarSize.BendFactor` conflated ACI 318-19 Table 25.3.1 (development of deformed bars in tension) with Table 25.3.2 (stirrups, ties, hoops); the `HookKind` `[SmartEnum]` discriminant SPLITS them — `HookKind.MinInsideBendFactor(d)` returns 6·d_b for #3..#8 development bars (8·d_b for #9..#11, 10·d_b for #14/#18) and 4·d_b for #3..#5 stirrup-tie/seismic bars (6·d_b for #6..#8), so a #5 stirrup bends at 4·d_b not the 6·d_b a development bar uses (the band thresholds 25.4/36/16 mm the ACI size-group boundaries). `RebarSection.StandardHook(kind, hook, key)` emits the `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm, MandrelDiameterMm, ShapeCode)` scalar tuple — the inside bend diameter `kind.MinInsideBendFactor(d)·d`, the straight extension `hook.ExtensionFactor·d` floored at `hook.MinExtensionMm` (65 mm for the 180° development hook tail, 75 mm for the 135° seismic/stirrup hook tail), AND the EN 1992 §8.3 / BS 8666:2020 Table 2 mandrel (bending-former) diameter `(d <= 16 ? 4 : 7)·d` — the value the `VividOrange.Sections` `Link.MinimumMandrelDiameter` getter computes (`4·φ` for `φ <= 16 mm`, `7·φ` above) for the actual link, surfaced on the schedule so the bender reads the former diameter. The host boundary lofts the bar centreline polyline and fillets each bend by the inside-bend-diameter radius into the host `Curve`, exactly as `Construction/layout#ASSEMBLY_FOLD` materializes scalar tuples into host geometry — this owner NEVER constructs a host curve.
- [SHAPE_CODE_SET]: REALIZED — the `ShapeCode` `[SmartEnum]` carries the full BS 8666:2020 schedule set (`00`/`01` straight, `11`..`15` single-bend/hook bars, `21`..`36` multi-bend bars, `41`..`56` complex bent bars, `47`/`48`/`51`/`52`/`63` closed links, `64` the six-leg bar, `67` the radiused arc, `75` the circular link, `77` the helix, `98` the chair, `99` the non-standard fully-dimensioned shape) — the prior `{00,13,14,15,51}` subset widened to the 37-code standard set. Each code carries `Legs` (the A..F dimension-letter count the host shape-table lofts the polyline from) and `Link` (true for the closed-perimeter stirrup/circular/helix shapes); the cutting-length formula is the host shape-table's, this owner carrying the schedule token the `IfcReinforcingBar` `BendingShapeCode` wire reads. `RebarSection.StandardHook` selects the code from the bar: a `RebarUsage.Stirrup` bar → `ShapeCode.Code51` (closed link), a longitudinal bar → the `RebarHook.Shape` (90°→`Code11` L-bar, 135°→`Code13` angled hook, 180°→`Code12` semicircular), `Code99` the fully-dimensioned fallback (`Legs 0`).
- [RIB_GEOMETRY_CAPTURE]: REALIZED — the prior `RebarSurface` carried only the `Textured`/`Plain` IFC bond token with NO rib geometry; the `RebarRibGeometry` record + the `RebarSection.Ribs(RibPattern)` projection now capture the ISO 6935-2 rib deformation a bond/anchorage model and the host materialization read. The GOVERNING bond invariant is `RelativeRibArea` fR — the ISO 6935-2 / EN 10080 minimum relative rib area `0.035` for `d <= 6 mm`, `0.040` for `6 < d <= 12 mm`, `0.056` for `d > 12 mm` (the bond index the development-length / `fib` Model Code bond-stress law reads). The transverse-rib spacing `0.7·d` and the ribless-perimeter fraction `0.125` (the un-ribbed perimeter the longitudinal ribs span) are the ASTM A615 §7.4 maxima (max average spacing `0.7·d`, gap `<= 12.5%` of nominal perimeter); the transverse/longitudinal rib height `0.05·d` the ASTM A615 Table 1 minimum-average-height-to-diameter ratio; the flank inclination α `45°` the ISO 6935-2 §4.14 minimum transverse-rib flank inclination; the rib-to-axis inclination β the `RibPattern` `InclinationDeg` (ISO 6935-2 §4.15 — `90°` for the `UniformHeight` parallel-rib bar, `60°` for the `Crescent` inclined two-series bar). A `Textured` bar projects `Some` rib geometry, a `Plain` round `None`; modern hot-rolled deformed bar defaults to `RibPattern.Crescent`.
- [IFC_REINFORCING_WIRE]: REALIZED — a reinforcing bar round-trips to IFC 4.3 as an `IfcReinforcingBar` carrying the GeometryGym-public scalars `NominalDiameter` (the `RebarSection.DiameterMm`), `CrossSectionArea` (`NominalAreaMm2`), `BarLength`, and `PredefinedType` (the `RebarUsage.IfcPredefinedType` token, the GeometryGym-verified `IfcReinforcingBarTypeEnum` ∈ {MAIN, LIGATURE, SPACEBAR, PUNCHING, EDGE, RING, ANCHORING, SHEAR, STUD, USERDEFINED, NOTDEFINED}); the `BarSurface` (`RebarSurface.IfcSurface`, the `IfcReinforcingBarSurfaceEnum` ∈ {PLAIN, TEXTURED}), the `BendingShapeCode` (`RebarBend.Shape.Key`, the BS 8666:2020 designation), and the `SteelGrade` (`RebarGrade.Key`) ride the `Rasm.Bim` egress's IFC mapping. A reinforcement mesh round-trips as an `IfcReinforcingMesh` over the GeometryGym-public scalars `MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarCrossSectionArea` reading the same `RebarSection` columns. The wire mapping is the `Projection/component#COMPONENT_PROJECTOR` + `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrengthMpa` the `SteelGrade` asserts), host-neutral here — this page emits the typed `RebarUsage`/`RebarSurface`/`RebarBend.Shape` columns through the verified enum tokens, never an IFC entity.
- [RC_SECTION_COMPOSITION]: REALIZED — `RcSectionBuilder.Of` lowers an `EnConcreteGrade`/`EnRebarGrade` through `EnGrade.Concrete`/`Rebar` to the `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` DATA (trapping the `ArgumentException`/`MissingNationalAnnexException` derivation throws onto `ComponentFault.Grade`), builds the `ConcreteSection` from the `component#COMPONENT_OWNER` `ParametricSection.ProfileOf` `IProfile` concrete outline + the concrete `IMaterial` + a `Link` over the EN-10080 `BarDiameter` + the `coverMm` `Length`, and folds the closed `RebarLayout` `[Union]` (the four `FaceReinforcementLayer`/`PerimeterReinforcementLayer` × count/spacing constructors collapsed into one) through `AddRebarLayer`. The `IConcreteSection` egress is the input the `capacity#SECTION_CAPACITY` `ConcreteSectionProperties` transformed-section solver and the `VividOrange.InteractionDiagram` N-M-M capacity engine consume; the EC2 clear bar spacing reads `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing`. `RcSectionBuilder.Of` admits ANY `component#COMPONENT_OWNER` `Component` as its concrete outline because `ParametricSection.ProfileOf` is FAMILY-AGNOSTIC (reads the `Component.GrossRectangleMm` `(WidthMm, DepthMm)` bounding pair into the rectangle `IProfile` regardless of `ComponentFamily`), so a `cmu#CMU_CELL_LATTICE` grouted CMU unit admits as the reinforced-masonry concrete input the SAME way — no cmu-specific RC builder, the grouted rectangle the concrete outline the reinforcement transform composes over (the grout `EnConcreteGrade` the section concrete). The `RcSection.ConcreteProfile` field carries the source `Component` so the `capacity#SECTION_CAPACITY` `RcElastic` arm reads its `GrossRectangleMm` gross depth/width for the fibre lever. The section-build fault rails `ComponentFault.Section` (`ComponentFault.Capacity` reserved for the capacity SOLVE). Ripple counterparts: `capacity#SECTION_CAPACITY` (the elastic transformed-section + N-M-M capacity over the `IConcreteSection` minted here) and `cmu#CMU_CELL_LATTICE` (the grouted CMU outline this boundary admits).
- [GRADE_CAPACITY_ROW]: REALIZED — `RebarGrade` carries a `CapacityId` `MaterialId`-key column resolving the GRADED `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` reinforcing-steel row (`steel.b500a`/`b500b`/`b500c` for the EN bands, `steel.gr40`/`gr60`/`gr75`/`gr80`/`gr60w`/`gr80w`/`400w`/`500w` for the ASTM/CSA bands), each row carrying the grade's `f_yk` (the ASTM A615/A706 / CSA G30.18 / EN 10080 spec yield) AND the ACI 318 §20.2.2.2 reinforcing modulus `E_s` 200 GPa, so `RebarSection.CapacityKey => Grade.CapacityId` and the `Projection/component#COMPONENT_PROJECTOR`-projected bar's seam `Mechanical` (the `graph.MechanicalOf` the `Rasm.Compute/structural` runner reads) is the bar's REAL grade strength — NEVER the generic `metal.steel` section baseline (`E` 210 GPa, `f_y` 235) that under-reads a 500/420 MPa bar by 2× and the modulus by 5%. `CapacityId` and `AppearanceId` are INDEPENDENT slots (the two-slot law); the prior single `metal.steel`-for-every-grade `CapacityKey` is the deleted form.
- [EN_GRADE_BINDING]: REALIZED — `RebarGrade` carries an `Option<EnRebarGrade> EnGrade` (`Some` for the EN-bodied `B500A`/`B500B`/`B500C` ductility-class bands bound to the verified `EnRebarGrade.B500A`/`B500B`/`B500C`, `None` for the ASTM/CSA spec-nominal bands), so the RC σ(ε)-law path reads a registered EN grade through `RcSectionBuilder.Of` rather than a hand-keyed `f_yk`, while the spec-nominal `MinimumYieldMpa` band stays the schedule receipt — the two strengths disjoint by source. The `RebarSection.EnYieldForceKn`/`EnUltimateForceKn` projections compose `EnRebarFactory.CreateLinearElastic(grade).Strength` (the registered `f_yk`, `E_s = 200 GPa`) and `EnRebarFactory.CreateBiLinear(grade).UltimateStrength` (the ductility-class `k·f_yk` ultimate, `k = 1.05` class A / `1.08` class B / `1.15` class C) into the EN-registered design yield/ultimate FORCE (`Some` for the EN-bodied grades) the development/lap/seismic-overstrength capacity-design seam reads off the bar — the VividOrange EN rebar factory exploited for the registered strengths, never a hand-keyed `f_u`. The `ComponentCatalogue.RebarRows` seed carries the EN H8/H12/H14/H16/H25/H32/H40 `B500A`/`B500B`/`B500C` rows so the `B500A`/`B/C` ductility classes (the three `CreateBiLinear` branches) and the EN H8/H14/H32/H40 sizes (keyed to the `BarDiameter.D8`/`D14`/`D32`/`D40` catalogue members the prior set left unexploited) are exercised catalogue rows, never declared-but-unseeded phantoms. `BarSize` binds the EN rows to the EN-10080 D6..D50 catalogue so a metric/EN bar resolves its diameter from the published catalogue; the EN-bodied `RebarStandard.En10080` band cites `En1992` at the `RcSectionBuilder` admission.
