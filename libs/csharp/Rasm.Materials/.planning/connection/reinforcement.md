# [MATERIALS_REINFORCEMENT]

THE FIRST REALIZED CONNECTIONFAMILY and THE REINFORCED-CONCRETE SECTION OWNER. The reinforcement vocabulary — the `RebarGrade` yield-strength axis (ASTM A615/A706, CSA G30.18, EN-10080, the EN-bodied grades bound to their `EnRebarGrade`), the `BarSize` nominal-bar axis (imperial #3..#18, CSA 10M..55M, EN H10..H25), the `RebarUsage` `IfcReinforcingBarTypeEnum` structural-role axis (main/ligature/shear/punching/edge/ring/anchoring/spacer) and the `RebarSurface` `IfcReinforcingBarSurfaceEnum` bond axis, the `RebarSection` cross-section receipt (nominal bar diameter / cross-sectional area / unit weight), and the ACI 318 standard bend/hook geometry carried as a scalar bend-angle/radius/hook-extension tuple plus its BS 8666 `ShapeCode` — is the realized reinforcing-bar vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Reinforcement` case, and THIS owner is ALSO the host-neutral reinforced-concrete-section assembler: the `RcSection.Of` boundary composes the `VividOrange.Sections` `ConcreteSection` from a `profile#PROFILE_OWNER` `Profile` concrete outline, a `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` EN grade, and the `FaceReinforcementLayer`/`PerimeterReinforcementLayer` rebar-layout engines over the EN-10080 `BarDiameter` catalogue, lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` transformed-section/N-M-M solvers consume. A #5 rebar is a `ConnectionItem` row, never a `Rebar` type: the bar size, the grade, the structural role, the bond surface, the section receipt, and the bend template are reinforcement-`ConnectionItem` columns, and the `RebarSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the fastener and hanger families drive — a reinforcing schedule places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The reinforcement vocabulary grows by data — a new bar size is one `BarSize` row keyed to its `BarDiameter` catalogue entry, a new grade one `RebarGrade` row bound to its `EnRebarGrade`, a new structural role one `RebarUsage` row carrying its `IfcReinforcingBarTypeEnum` token, a new designation one `RebarRow` catalogue entry — never a per-bar type. The bend geometry is a scalar bend-angle/radius/hook-extension tuple the host materializes into a curve, NEVER a host curve here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Placement>` of scalar tuples the host boundary materializes). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every length/area/weight column, the `VividOrange.Sections` reinforced-section + reinforcement-layout owner (`ConcreteSection`/`Rebar`/`Link`/`FaceReinforcementLayer`/`PerimeterReinforcementLayer`/`ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`/`MinimumReinforcementSpacing`/`BarDiameter`), the `VividOrange.Materials` EN grade DATA (`EnConcreteMaterial`/`EnRebarMaterial`/`EnRebarFactory`) bound to its `VividOrange.Standards` `En1992` citation, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.iron`/`metal.steel`) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the fastener, hanger, and joint families land their own sibling vocabularies on `Connection/fastener#FASTENER_FAMILY`, `Connection/hanger#HANGER_FAMILY`, and `Connection/joint#JOINT_FAMILY`, and the elastic transformed-section and ultimate N-M-M capacity of the RC section land on `Profiles/capacity#SECTION_CAPACITY` over the `IConcreteSection` THIS owner mints.

## [01]-[INDEX]

- [01]-[REINFORCEMENT_FAMILY]: the `RebarGrade` yield-strength axis bound to its `EnRebarGrade`, the `BarSize` nominal-bar axis keyed to the EN-10080 `BarDiameter`, the `RebarUsage` `IfcReinforcingBarTypeEnum` structural-role axis and the `RebarSurface` `IfcReinforcingBarSurfaceEnum` bond axis, the `RebarBend` ACI 318 bend/hook scalar tuple carrying its BS 8666 `ShapeCode`, the `RebarSection` cross-section receipt, and the `ConnectionCatalogue.BuildRebarRows` ASTM A615/A706/CSA G30.18/EN-10080 row table.
- [03]-[RC_SECTION]: the `RcSection` reinforced-concrete-section owner — the `VividOrange.Sections` `ConcreteSection` admission boundary over a `Profile` concrete outline + an `EnConcreteMaterial`/`EnRebarMaterial` EN grade + a `RebarLayout` `[Union]` (count/spacing × face/perimeter) over the `BarDiameter` catalogue, lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` solvers consume, with the EN grade admission lowering the `VividOrange.Materials` derivation throws onto the typed `ConnectionFault` rail and the EC2 `MinimumReinforcementSpacing` clear-spacing rule composed.

## [02]-[REINFORCEMENT_FAMILY]

- Owner: the reinforcement vocabulary (`RebarGrade` the ASTM A615/A706/CSA/EN yield-strength axis, `BarSize` the #3..#18 / 10M..55M / H10..H25 nominal-bar axis, `RebarStandard` the spec discriminant, `RebarUsage` the `IfcReinforcingBarTypeEnum` structural-role axis, `RebarSurface` the `IfcReinforcingBarSurfaceEnum` bond axis, `RebarBend` the ACI 318 bend/hook scalar tuple, `RebarSection` the cross-section receipt); `ConnectionCatalogue.BuildRebarRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `RebarSection.StandardHook` projection emitting the ACI 318 hook geometry plus the BS 8666 `ShapeCode` the host materializes and the wire reads.
- Cases: grade {A615 Gr40 / Gr60 / Gr75 / Gr80 (carbon-steel, non-weldable above Gr60 without supplemental S1) · A706 Gr60 / Gr80 (low-alloy, weldable) · CSA G30.18 400W / 500W (metric, weldable) · EN-10080 B500B / B500C (the EN-bodied grades the RC σ(ε) law reads)} · size {imperial #3..#11, #14, #18 · metric 10M..55M · EN H10..H25} · usage {main longitudinal · ligature/shear stirrup · punching · edge · ring · anchoring · spacer, each its verified `IfcReinforcingBarTypeEnum` token} · surface {textured deformed (default) · plain round} · hook {ninety / one-thirty-five / one-eighty standard bend angles, the ACI 318 Table 25.3.1/25.3.2 minimum-bend-diameter and hook-extension rule, each its BS 8666 `ShapeCode`} — a bar is a `ConnectionItem` row over one `RebarGrade`, one `BarSize`, one `RebarUsage`, and one `RebarSurface`, never a bar/role subtype.
- Entry: `public Fin<RebarBend> StandardHook(RebarHook hook, Op key)` on `RebarSection` — the ACI 318 standard-hook projection resolving the bend angle, the minimum inside bend diameter (a `BarSize`-dependent multiple of the bar diameter), the straight hook extension, AND the BS 8666 `ShapeCode` (a `RebarUsage.Stirrup` bar schedules as a closed link `51`, a longitudinal bar as the hook-angle-keyed end-hook code) into the scalar `RebarBend` tuple the host curve-materializes and the `IfcReinforcingBar` `BendingShapeCode` reads; `ConnectionCatalogue.BuildRebarRows(context)` folds the A615/A706/CSA/EN `RebarRow` table through `RebarOf` (resolving grade/size/usage/surface and admitting the dimensional columns) into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBarBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the bar-diameter/area/weight/bend-geometry length columns, never an int-backed `Dimension` that truncates a fractional millimeter; `Op`/`Context` the kernel admission key), Rasm.Element (project — `MaterialId` the row's appearance/capacity identity), VividOrange.Sections (`BarDiameter` the EN-10080 D6..D50 catalogue diameter the metric/EN `BarSize` rows key, `.api/api-vividorange-sections.md`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the grade/size/standard/usage/surface/hook axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the reinforcement vocabulary grows by data — a new bar size is one `BarSize` row carrying its nominal diameter/area/weight (the metric/EN rows keyed to a `BarDiameter` catalogue value), a new grade one `RebarGrade` row carrying its minimum yield bound to its `EnRebarGrade`, a new structural role one `RebarUsage` row carrying its `IfcReinforcingBarTypeEnum` token, a new designation one `RebarRow` catalogue entry, a new hook one `RebarHook` row carrying its `ShapeCode` — never a per-bar/per-role type, never a per-grade `ConnectionItem` variant. A fastener/hanger/joint family lands its own vocabulary on its own page the way reinforcement carries `RebarGrade`/`BarSize`/`RebarUsage`/`RebarSection`; `anchor` folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint, `joint` the deliberate continuous weld/adhesive/stud widening at `Connection/joint#JOINT_FAMILY`), never a fifth sibling family or a per-bar type. The reinforced-concrete SECTION the rebar populates (the `RcSection` `ConcreteSection` assembler) is the `[03]-[RC_SECTION]` owner block on this page, its elastic transformed-section and ultimate N-M-M capacity composed at `Profiles/capacity#SECTION_CAPACITY`.
- Boundary: the reinforcement vocabulary is the first realized `ConnectionFamily` — a per-bar `Rebar` class is the deleted form; `RebarSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ASTM nominal diameter (`#5 = 15.875 mm`, `10M = 11.3 mm`) admits without the truncation an int-backed `Dimension` count would force, the `NominalAreaMm2` and `UnitWeightKgM` columns carried as `PositiveMagnitude` receipts the `IfcReinforcingBar` wire and the structural-design seam read; the IFC structural ROLE is the `RebarUsage` `IfcReinforcingBarTypeEnum` token column (the `FastenerKind.IfcPredefinedType` mirror — `main`→`MAIN`, `ligature`→`LIGATURE`, `shear`→`SHEAR`, `spacer`→`SPACEBAR`) the `IfcReinforcingBar` `PredefinedType` carries and the BOND surface the `RebarSurface` `IfcReinforcingBarSurfaceEnum` token (`textured`→`TEXTURED`, `plain`→`PLAIN`), so a stirrup's IFC role and a deformed bar's bond are typed columns the wire reads, never an inferred default; the ACI 318 bend geometry is the scalar `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm, ShapeCode)` tuple `StandardHook` emits — the `ShapeCode` the BS 8666 schedule designation (`13`/`14`/`15` end hooks, `51` closed link) the `IfcReinforcingBar` `BendingShapeCode` carries — NEVER a host `Curve`, the host boundary lofts the bar centerline and the bend arc from the scalar tuple exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.iron` for plain carbon A615, `metal.steel` for low-alloy A706 / weldable EN) the row's `ConnectionItem.AppearanceId` column carries, never a reinforcement-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `RebarGrade.MinimumYieldMpa` axis is the spec-nominal grade band, the `RebarSection.YieldForceKn` projection the spec-nominal `f_yk·A_s` total (the `FastenerSection.ProofLoadKn`/`JointSection.DesignShearKn` mirror), and the measured capacity is the property-library receipt; `ConnectionCatalogue.BuildRebarRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the A615/A706/G30.18/EN-10080 rows keyed `connection.<designation>` (`connection.rebar-no5-gr60`, `connection.rebar-10m-400w`, `connection.rebar-h16-b500c`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a rebar schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel reinforcement-layout owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;                                // PositiveMagnitude (>0 finite magnitude — the bar-diameter/area/weight/bend columns) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                                 // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                                // MaterialId (the seam-carried appearance/capacity identity the ConnectionItem AppearanceId references)
using Thinktecture;
using Rasm.Materials.Connection;                   // ConnectionFamily/ConnectionId/ConnectionItem/ConnectionSection/ConnectionFault (the parent CONNECTION_OWNER)
using VividOrange.Sections.Reinforcement;          // BarDiameter (the EN-10080 D6..D50 catalogue diameter the metric/EN BarSize rows key)
using VividOrange.Materials.StandardMaterials.En;  // EnRebarGrade (the EN-bodied grade the EN-10080 rows bind for the RC σ(ε) law)
using static LanguageExt.Prelude;                  // guard, Seq, Some/None, Optional

// Each Connection family page is its OWN Rasm.Materials.Connection.<Family> sub-namespace so the four sibling
// `ConnectionCatalogue` static classes are distinct types (a shared Rasm.Materials.Connection would be a CS0101 collision
// with connection.md's own `ConnectionCatalogue`); connection#CONNECTION_OWNER stays the parent Rasm.Materials.Connection
// and folds Reinforcement.ConnectionCatalogue.BuildRebarRows by the sub-namespace-qualified name; parent types via the using above.
namespace Rasm.Materials.Connection.Reinforcement;

// --- [TYPES] -------------------------------------------------------------------------------
// RebarStandard the spec discriminant; RebarUsage the IfcReinforcingBarTypeEnum structural-role axis carrying its
// verified wire token (the FastenerKind.IfcPredefinedType mirror); RebarSurface the IfcReinforcingBarSurfaceEnum
// bond-surface axis; RebarGrade the yield band bound to its EnRebarGrade; BarSize the nominal-bar axis keyed to a
// BarDiameter; RebarHook the ACI 318 hook angle/extension axis — six closed [SmartEnum<string>] vocabularies, every
// reinforcement variant a row, never a per-bar/per-grade/per-role subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615 = new("astm-a615", weldable: false, authority: "ASTM A615/A615M");
    public static readonly RebarStandard A706 = new("astm-a706", weldable: true, authority: "ASTM A706/A706M");
    public static readonly RebarStandard G30 = new("csa-g30.18", weldable: true, authority: "CSA G30.18");
    public static readonly RebarStandard En10080 = new("en-10080", weldable: true, authority: "EN 1992-1-1 / EN 10080");
    public bool Weldable { get; }
    public string Authority { get; }
}

// The bar's STRUCTURAL ROLE — the FastenerKind.IfcPredefinedType mirror, each row carrying its verified
// IfcReinforcingBarTypeEnum member spelling (the Rasm.Bim Semantics/connection [REALIZING_ELEMENT_SURFACE]
// GeometryGym 25.7.30 decompile: MAIN/LIGATURE/SPACEBAR/PUNCHING/EDGE/RING/ANCHORING/SHEAR/STUD/USERDEFINED/
// NOTDEFINED). A stirrup is RebarUsage.Ligature, a longitudinal bar RebarUsage.Main, a shear leg RebarUsage.Shear —
// the IFC role is a column the connection#CONNECTION_PROJECTOR ConnectionProjector lands on the seam Object node's
// PredefinedType (the BarType detail-bag row analogue), never a per-role rebar subtype. The
// Stirrup flag drives the RcSection link-vs-longitudinal placement so a Ligature row routes to the ConcreteSection
// Link rather than an AddRebarLayer longitudinal.
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
    public static readonly RebarUsage Stud      = new("stud",      ifcPredefinedType: "STUD",      stirrup: false);   // cast-in headed-stud reinforcement (the IfcReinforcingBarTypeEnum STUD role) — distinct from the welded shear connector the joint#JOINT_FAMILY StudClass owns
    public string IfcPredefinedType { get; }
    public bool Stirrup { get; }   // true -> the RcSection link / a transverse confinement bar, false -> a longitudinal layer bar
}

// The bar's BOND SURFACE — the verified IfcReinforcingBarSurfaceEnum {PLAIN, TEXTURED} the IfcReinforcingBar wire
// carries, the deformed-rib vs plain-round distinction the bond-development length reads; a modern deformed bar is
// RebarSurface.Textured, a plain round (a tie/spacer or pre-1960 stock) RebarSurface.Plain.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarSurface {
    public static readonly RebarSurface Textured = new("textured", ifcSurface: "TEXTURED");   // deformed/ribbed (the default for A615/A706/EN-10080 hot-rolled bar)
    public static readonly RebarSurface Plain    = new("plain",    ifcSurface: "PLAIN");      // plain round (ties, spacers, smooth dowels)
    public string IfcSurface { get; }
}

// EnGrade is the VividOrange.Materials EnRebarGrade an EN-bodied grade maps to (None for the ASTM/CSA bands that
// carry no EN equivalent), so RcSection.Of can lower the matched EN grade to its EnRebarMaterial fck/Es DATA without
// re-keying f_yk; the RebarGrade band stays the spec-nominal yield the connection schedule reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarGrade {
    public static readonly RebarGrade Gr40  = new("gr40",  minimumYieldMpa: 280.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60  = new("gr60",  minimumYieldMpa: 420.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr75  = new("gr75",  minimumYieldMpa: 520.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr80  = new("gr80",  minimumYieldMpa: 550.0, standard: RebarStandard.A615,   appearanceId: "metal.iron",  enGrade: None);
    public static readonly RebarGrade Gr60W = new("gr60w", minimumYieldMpa: 420.0, standard: RebarStandard.A706,   appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade Gr80W = new("gr80w", minimumYieldMpa: 550.0, standard: RebarStandard.A706,   appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G400W = new("400w",  minimumYieldMpa: 400.0, standard: RebarStandard.G30,    appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade G500W = new("500w",  minimumYieldMpa: 500.0, standard: RebarStandard.G30,    appearanceId: "metal.steel", enGrade: None);
    public static readonly RebarGrade B500B = new("b500b", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500B));
    public static readonly RebarGrade B500C = new("b500c", minimumYieldMpa: 500.0, standard: RebarStandard.En10080, appearanceId: "metal.steel", enGrade: Some(EnRebarGrade.B500C));
    public double MinimumYieldMpa { get; }
    public RebarStandard Standard { get; }
    public string AppearanceId { get; }
    public Option<EnRebarGrade> EnGrade { get; }   // Some only for the EN-bodied B500B/B500C the RC σ(ε) law reads
    public bool Weldable => Standard.Weldable;
}

// Catalogue is the EN-10080 BarDiameter the metric rows resolve a Rebar diameter from rather than a raw Length —
// None for the ASTM/CSA bands the catalogue does not enumerate, so RcSection.Of feeds a catalogued BarDiameter when
// present and a raw Length(nominalDiameterMm) otherwise; the BarSize band stays the spec-nominal section receipt.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BarSize {
    public static readonly BarSize No3  = new("#3",  nominalDiameterMm: 9.525,  nominalAreaMm2: 71.0,   unitWeightKgM: 0.560,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No4  = new("#4",  nominalDiameterMm: 12.700, nominalAreaMm2: 129.0,  unitWeightKgM: 0.994,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No5  = new("#5",  nominalDiameterMm: 15.875, nominalAreaMm2: 199.0,  unitWeightKgM: 1.552,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No6  = new("#6",  nominalDiameterMm: 19.050, nominalAreaMm2: 284.0,  unitWeightKgM: 2.235,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No7  = new("#7",  nominalDiameterMm: 22.225, nominalAreaMm2: 387.0,  unitWeightKgM: 3.042,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No8  = new("#8",  nominalDiameterMm: 25.400, nominalAreaMm2: 510.0,  unitWeightKgM: 3.973,  bendFactor: 6.0,  catalogue: None);
    public static readonly BarSize No9  = new("#9",  nominalDiameterMm: 28.651, nominalAreaMm2: 645.0,  unitWeightKgM: 5.060,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No10 = new("#10", nominalDiameterMm: 32.258, nominalAreaMm2: 819.0,  unitWeightKgM: 6.404,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No11 = new("#11", nominalDiameterMm: 35.814, nominalAreaMm2: 1006.0, unitWeightKgM: 7.907,  bendFactor: 8.0,  catalogue: None);
    public static readonly BarSize No14 = new("#14", nominalDiameterMm: 43.002, nominalAreaMm2: 1452.0, unitWeightKgM: 11.380, bendFactor: 10.0, catalogue: None);
    public static readonly BarSize No18 = new("#18", nominalDiameterMm: 57.328, nominalAreaMm2: 2581.0, unitWeightKgM: 20.240, bendFactor: 10.0, catalogue: None);
    public static readonly BarSize M10  = new("10M", nominalDiameterMm: 11.300, nominalAreaMm2: 100.0,  unitWeightKgM: 0.785,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D12));
    public static readonly BarSize M15  = new("15M", nominalDiameterMm: 16.000, nominalAreaMm2: 200.0,  unitWeightKgM: 1.570,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D16));
    public static readonly BarSize M20  = new("20M", nominalDiameterMm: 19.500, nominalAreaMm2: 300.0,  unitWeightKgM: 2.355,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D20));
    public static readonly BarSize M25  = new("25M", nominalDiameterMm: 25.200, nominalAreaMm2: 500.0,  unitWeightKgM: 3.925,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D25));
    public static readonly BarSize M30  = new("30M", nominalDiameterMm: 29.900, nominalAreaMm2: 700.0,  unitWeightKgM: 5.495,  bendFactor: 8.0,  catalogue: Some(BarDiameter.D32));
    public static readonly BarSize M35  = new("35M", nominalDiameterMm: 35.700, nominalAreaMm2: 1000.0, unitWeightKgM: 7.850,  bendFactor: 8.0,  catalogue: Some(BarDiameter.D40));
    public static readonly BarSize M45  = new("45M", nominalDiameterMm: 43.700, nominalAreaMm2: 1500.0, unitWeightKgM: 11.775, bendFactor: 10.0, catalogue: Some(BarDiameter.D50));
    public static readonly BarSize M55  = new("55M", nominalDiameterMm: 56.400, nominalAreaMm2: 2500.0, unitWeightKgM: 19.625, bendFactor: 10.0, catalogue: None);
    // EN-10080 nominal diameters (the H-bar series the B500B/B500C grades are rolled at) keyed straight to their
    // BarDiameter catalogue value, so an EN bar resolves its Rebar diameter from the published catalogue, not a literal.
    public static readonly BarSize H10  = new("H10", nominalDiameterMm: 10.000, nominalAreaMm2: 78.5,   unitWeightKgM: 0.617,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D10));
    public static readonly BarSize H12  = new("H12", nominalDiameterMm: 12.000, nominalAreaMm2: 113.1,  unitWeightKgM: 0.888,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D12));
    public static readonly BarSize H16  = new("H16", nominalDiameterMm: 16.000, nominalAreaMm2: 201.1,  unitWeightKgM: 1.578,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D16));
    public static readonly BarSize H20  = new("H20", nominalDiameterMm: 20.000, nominalAreaMm2: 314.2,  unitWeightKgM: 2.466,  bendFactor: 6.0,  catalogue: Some(BarDiameter.D20));
    public static readonly BarSize H25  = new("H25", nominalDiameterMm: 25.000, nominalAreaMm2: 490.9,  unitWeightKgM: 3.854,  bendFactor: 8.0,  catalogue: Some(BarDiameter.D25));
    public double NominalDiameterMm { get; }
    public double NominalAreaMm2 { get; }
    public double UnitWeightKgM { get; }
    public double BendFactor { get; }
    public Option<BarDiameter> Catalogue { get; }   // Some -> RcSection feeds the catalogued BarDiameter to a Rebar
}

// The ACI 318-19 Table 25.3.2 standard end-hook angles, each carrying its straight-extension factor (×dᵦ) and its
// BS 8666 schedule ShapeCode the RebarBend emits for a non-stirrup bar (13 = 90° end hook, 14 = 135°, 15 = 180°).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RebarHook {
    public static readonly RebarHook Ninety        = new("90",  bendDegrees: 90.0,  extensionFactor: 12.0, shapeCode: "13");
    public static readonly RebarHook OneThirtyFive = new("135", bendDegrees: 135.0, extensionFactor: 6.0,  shapeCode: "14");
    public static readonly RebarHook OneEighty     = new("180", bendDegrees: 180.0, extensionFactor: 4.0,  shapeCode: "15");
    public double BendDegrees { get; }
    public double ExtensionFactor { get; }
    public string ShapeCode { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The host-neutral bend receipt: the ACI 318 angle + minimum-inside-bend-diameter + straight hook extension the host
// curve-materializes, PLUS the ISO 3766 / BS 8666 BendingShapeCode the IfcReinforcingBar BendingShapeCode wire reads —
// closing the [IFC_REINFORCING_WIRE] schedule-shape promise. ShapeCode is the standard bar-bending schedule designation
// (BS 8666: "00" straight, "13" single 90° end hook, "14" single 135° end hook, "15" single 180° end hook, "51" closed
// link/stirrup) the host materializes the polyline shape from and the federation reads as the BendingShapeCode token.
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm, string ShapeCode);

public readonly record struct RebarSection(
    BarSize Size,
    RebarGrade Grade,
    RebarUsage Usage,
    RebarSurface Surface,
    PositiveMagnitude DiameterMm,
    PositiveMagnitude NominalAreaMm2,
    PositiveMagnitude UnitWeightKgM) {

    // The FINISH the appearance projection reads (metal.iron for plain carbon A615, metal.steel for low-alloy/weldable),
    // INDEPENDENT from CapacityKey: a coated bar keeps the steel CapacityKey while its AppearanceId names the epoxy-coat row.
    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);

    // The CAPACITY material whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row (YieldStrengthMpa/YoungsModulusMpa)
    // the structural-connection-design seam reads — the bar's structural STEEL, sourced INDEPENDENTLY from the AppearanceId
    // finish (connection#CONNECTION_OWNER's two-slot independence law: a coated bar's capacity steel and its coat finish
    // are distinct MaterialIds, neither derived from the other). The reinforcing steel is metal.steel for every grade.
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    // SPEC-NOMINAL bar tensile-yield force — the FastenerSection.ProofLoadKn / JointSection.DesignShearKn mirror: a
    // TOTAL double projection over the section's OWN spec-nominal grade band, NEVER a base-metal re-derivation. The
    // MaterialId-keyed properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt (the measured YieldStrengthMpa read by
    // the ConnectionItem CapacityKey) is the design seam's, read once at the federation, never threaded into the section.
    public double YieldForceKn => Grade.MinimumYieldMpa * NominalAreaMm2.Value * 1e-3;

    // The ACI 318-19 standard hook (Table 25.3.1 minimum inside bend diameter + Table 25.3.2 straight extension) PLUS the
    // BS 8666 schedule ShapeCode the IfcReinforcingBar BendingShapeCode carries — the Usage selects a closed link (51)
    // over an end hook so a Ligature stirrup schedules as a closed perimeter, a Main bar as the angle-keyed end hook.
    public Fin<RebarBend> StandardHook(RebarHook hook, Op key) =>
        from valid in guard(hook.ExtensionFactor > 0.0 && Size.BendFactor > 0.0, ConnectionFault.Capacity(key, $"<degenerate-hook:{Grade.Key}:{Size.Key}>"))
        let diameter = DiameterMm.Value
        let insideBendDiameterMm = Size.BendFactor * diameter
        let hookExtensionMm = hook.ExtensionFactor * diameter
        select new RebarBend(hook.BendDegrees, insideBendDiameterMm, hookExtensionMm, Usage.Stirrup ? "51" : hook.ShapeCode);
}

public readonly record struct RebarRow(string Designation, string Size, string Grade, string Usage, string Surface);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    // designation · size · grade · usage · surface — the ASTM A615/A706, CSA G30.18, and EN-10080 reinforcing-bar seed.
    // The usage column keys the IfcReinforcingBarTypeEnum role (main longitudinal vs ligature stirrup vs shear leg) and
    // routes the RcSection placement; the surface column the IfcReinforcingBarSurfaceEnum bond surface (deformed default,
    // plain for ties). The EN-10080 B500B/B500C rows close the previously-phantom En10080 standard + B500B/B500C grades.
    static readonly Seq<RebarRow> RebarRows = Seq(
        new RebarRow("connection.rebar-no3-gr60",     "#3",  "gr60",  "main",     "textured"),
        new RebarRow("connection.rebar-no4-gr60",     "#4",  "gr60",  "main",     "textured"),
        new RebarRow("connection.rebar-no4-gr60-tie", "#4",  "gr60",  "ligature", "textured"),
        new RebarRow("connection.rebar-no5-gr60",     "#5",  "gr60",  "main",     "textured"),
        new RebarRow("connection.rebar-no6-gr60",     "#6",  "gr60",  "main",     "textured"),
        new RebarRow("connection.rebar-no7-gr75",     "#7",  "gr75",  "main",     "textured"),
        new RebarRow("connection.rebar-no8-gr75",     "#8",  "gr75",  "main",     "textured"),
        new RebarRow("connection.rebar-no9-gr80",     "#9",  "gr80",  "main",     "textured"),
        new RebarRow("connection.rebar-no11-gr80",    "#11", "gr80",  "main",     "textured"),
        new RebarRow("connection.rebar-no5-gr60w",    "#5",  "gr60w", "main",     "textured"),
        new RebarRow("connection.rebar-no8-gr80w",    "#8",  "gr80w", "main",     "textured"),
        new RebarRow("connection.rebar-10m-400w",     "10M", "400w",  "main",     "textured"),
        new RebarRow("connection.rebar-10m-400w-tie", "10M", "400w",  "ligature", "textured"),
        new RebarRow("connection.rebar-15m-400w",     "15M", "400w",  "main",     "textured"),
        new RebarRow("connection.rebar-25m-500w",     "25M", "500w",  "main",     "textured"),
        new RebarRow("connection.rebar-35m-500w",     "35M", "500w",  "main",     "textured"),
        new RebarRow("connection.rebar-h12-b500b",      "H12", "b500b", "main",     "textured"),
        new RebarRow("connection.rebar-h16-b500c",      "H16", "b500c", "main",     "textured"),
        new RebarRow("connection.rebar-h25-b500c",      "H25", "b500c", "main",     "textured"),
        new RebarRow("connection.rebar-h10-b500b-link", "H10", "b500b", "ligature", "plain"));

    static Fin<(ConnectionId Id, ConnectionItem Item)> RebarOf(RebarRow r, Context context, Op key) =>
        from size in BarSize.TryGet(r.Size, out BarSize? s) ? Fin.Succ(s!) : Fin.Fail<BarSize>(ConnectionFault.Designation(key, $"<unknown-bar-size:{r.Size}>"))
        from grade in RebarGrade.TryGet(r.Grade, out RebarGrade? g) ? Fin.Succ(g!) : Fin.Fail<RebarGrade>(ConnectionFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from usage in RebarUsage.TryGet(r.Usage, out RebarUsage? u) ? Fin.Succ(u!) : Fin.Fail<RebarUsage>(ConnectionFault.Designation(key, $"<unknown-usage:{r.Usage}>"))
        from surface in RebarSurface.TryGet(r.Surface, out RebarSurface? f) ? Fin.Succ(f!) : Fin.Fail<RebarSurface>(ConnectionFault.Designation(key, $"<unknown-surface:{r.Surface}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalDiameterMm)
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalAreaMm2)
        from weight in key.AcceptValidated<PositiveMagnitude>(candidate: size.UnitWeightKgM)
        let section = new RebarSection(size, grade, usage, surface, diameter, area, weight)
        from item in ConnectionItem.Of(ConnectionFamily.Reinforcement, r.Designation, new ConnectionSection.Reinforcement(section), section.CapacityKey, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildRebarRows(Context context) =>
        RebarRows
            .Choose(row => RebarOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RC_SECTION]

- Owner: `RcSection` the host-neutral reinforced-concrete-section assembler over the `VividOrange.Sections` `ConcreteSection`; `RebarLayout` `[Union]` the closed rebar-arrangement axis (`FaceCount`/`FaceSpacing`/`PerimeterCount`/`PerimeterSpacing`) collapsing the four `VividOrange.Sections` layout-engine constructors; `EnGrade` the static EN-grade admission boundary lowering `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` derivation throws onto the typed `ConnectionFault` rail; `RcSection.Of` the one builder lifting the `IConcreteSection` the `Profiles/capacity#SECTION_CAPACITY` solvers consume.
- Cases: layout {`FaceCount` (n bars on a named `SectionFace` — `ReinforcementLayoutByCount` + `FaceReinforcementLayer`), `FaceSpacing` (max-spacing bars on a face — `ReinforcementLayoutBySpacing` + `FaceReinforcementLayer`), `PerimeterCount` (n bars round the whole section — `ReinforcementLayoutByCount` + `PerimeterReinforcementLayer`, no face), `PerimeterSpacing` (max-spacing perimeter bars — `ReinforcementLayoutBySpacing` + `PerimeterReinforcementLayer`, no face)} — the face cases over the `VividOrange.Sections` `SectionFace` floor enum (`Top`/`Left`/`Right`/`Bottom`/`Sides`, NO `Perimeter` member — perimeter distribution is the separate `PerimeterReinforcementLayer` engine, never a `SectionFace` value), the perimeter cases carrying no face; a rebar arrangement is a `RebarLayout` case, never a per-engine constructor scattered at the call site; a stirrup is the `Link` admitted once with its `MinimumMandrelDiameter`, the EC2 clear spacing the one `MinimumReinforcementSpacing` rule.
- Entry: `public static Fin<RcSection> Of(Profile concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key)` — the ONE reinforced-section boundary: it lowers the `concreteGrade` through `EnGrade.Concrete(concreteGrade, annex, key)` to an `EnConcreteMaterial`, the `barGrade.EnGrade` through `EnGrade.Rebar(...)` to an `EnRebarMaterial` (a non-EN `barGrade.EnGrade == None` railing `ConnectionFault.Grade`), builds the `ConcreteSection` from the `Profile`'s `IProfile` perimeter (the `profile#PROFILE_OWNER` `ParametricSection`/`SectionReader` `IProfile` the section solver already consumes) + the concrete `IMaterial` + a `Link` (the `linkSize.Catalogue` `BarDiameter` over the rebar `IMaterial`) + the `coverMm` `Length`, folds each `RebarLayout` case to its `FaceReinforcementLayer`/`PerimeterReinforcementLayer` and `AddRebarLayer`s it, and reads back `section.Rebars` — the `IConcreteSection` the elastic transformed-section (`ConcreteSectionProperties`) and ultimate N-M-M (`InteractionDiagram`) solvers at `Profiles/capacity#SECTION_CAPACITY` consume; `public Fin<double> MinimumBarSpacingMm(NationalAnnex annex, BarSize size, double maxAggregateMm, Op key)` reads the EC2 `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing` clear-spacing rule for the annex, one polymorphic boundary, never a `BuildRcByCount`/`BuildRcBySpacing` family.
- Packages: VividOrange.Sections (`ConcreteSection`/`Section`, `Rebar`/`Link`/`LongitudinalReinforcement`, `FaceReinforcementLayer`/`PerimeterReinforcementLayer`, `ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing`, `MinimumReinforcementSpacing`, `SectionFace`, `BarDiameter`; the `InvalidMaterialTypeException`/`InvalidProfileTypeException` boundary throws trapped here; `.api/api-vividorange-sections.md`), VividOrange.Materials (`EnConcreteMaterial`/`EnRebarMaterial` grade DATA, `EnConcreteFactory`/`EnRebarFactory`; the `ArgumentException`/`MissingNationalAnnexException` derivation throws trapped here; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1992`/`NationalAnnex` the grade cites; `.api/api-vividorange-standards.md`), VividOrange.Profiles + Profiles.Perimeter + Geometry (the `IProfile`/`IPerimeter`/`ILocalPoint2d` the `ConcreteSection` consumes, via the `profile#PROFILE_OWNER` `ParametricSection`), UnitsNet (`Length` cover/diameter at the edge), Rasm (project — `PositiveMagnitude`), LanguageExt.Core (`Fin`/`Seq`/`Fold`), Thinktecture.Runtime.Extensions (`[Union]` for `RebarLayout`, `[SmartEnum]` for the `SectionFace` band).
- Growth: a new rebar arrangement is one `RebarLayout` `[Union]` case binding its `VividOrange.Sections` layout engine, a new section face one `SectionFace` band row, a new constitutive concrete law a `Profiles/capacity#SECTION_CAPACITY` concern reading the same `EnConcreteMaterial` — never a per-arrangement RC builder, never a hand-keyed `f_yk`/`f_ck` where the EN grade carries it, never a raw-`Length` bar diameter where the `BarDiameter` catalogue enumerates it; the EN grade DATA admission is the ONE boundary, the capacity computation the `Profiles/capacity` owner over the `IConcreteSection` minted here.
- Boundary: `RcSection.Of` is the BOUNDARY_ADMISSION point where the `VividOrange.Sections`/`Materials` exception-throwing surface is admitted EXACTLY ONCE — the `EnConcreteMaterial`/`EnRebarMaterial` ctors throw `ArgumentException` (unknown grade) / `MissingNationalAnnexException` (untabulated annex) and the `ConcreteSection`/layout construction throws `InvalidMaterialTypeException`/`InvalidProfileTypeException`, all trapped at THIS boundary and lowered onto the typed `ConnectionFault.Grade`/`ConnectionFault.Capacity` rail (the `.api` `[BOUNDARY_EXCEPTION_LAW]`), so no `VividOrange` throw and no `null` reaches an interior signature and the `IConcreteSection` egress carries only validated DATA; the `barGrade` binds to its `EnRebarGrade` through `RebarGrade.EnGrade` so a `Rebar` carries a registered EN grade rather than a hand-keyed `f_yk` (the spec-nominal `MinimumYieldMpa` band stays the connection-schedule receipt, the EN grade DATA the RC-capacity strength), and a metric `BarSize.Catalogue` resolves a `Rebar(IMaterial, BarDiameter)` over the EN-10080 catalogue rather than a raw `Length`; the four `VividOrange.Sections` layout-engine constructors (`FaceReinforcementLayer`/`PerimeterReinforcementLayer` × count/spacing) collapse into the one `RebarLayout` `[Union]` the `Of` fold dispatches — the 4+-parallel-constructor collapse trigger — so a bar arrangement is a `RebarLayout` case never a scattered `new FaceReinforcementLayer(...)`; the EC2 clear spacing reads the `MinimumReinforcementSpacing(annex)` rule, never an inline EC2 constant; the assembled `IConcreteSection` is host-neutral DATA (the `VividOrange.Geometry` `ILocalPoint2d` bar positions, never a `Rhino.Geometry` type) the `Profiles/capacity#SECTION_CAPACITY` elastic transformed-section + ultimate N-M-M solvers consume and the `VividOrange.Serialization` round-trip persists inside the C# layer (`.api/api-vividorange-serialization.md`, the C#-internal cache wire, distinct from the canonical Thinktecture wire); the RC section is NOT a `ConnectionItem` — a `ConnectionItem` is one discrete rebar in the schedule, the `RcSection` the populated concrete member the rebar reinforces, the two meeting at the `BarSize`/`RebarGrade` vocabulary this page already owns.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
// Same Rasm.Materials.Connection.Reinforcement sub-namespace as the section-02 fence (RcSection/RebarLayout/EnGrade are
// reinforcement-family types); composes the section-02 prelude (LanguageExt, Rasm.Vectors, Rasm.Domain, Thinktecture,
// static Prelude) plus the VividOrange RC surface below and the parent Rasm.Materials.Connection ConnectionFault rail.
using Rasm.Materials.Connection;                     // ConnectionFault (the parent CONNECTION_OWNER rail RcSection.Of lowers the VividOrange throws onto)
using Rasm.Materials.Profiles;                       // Profile, ParametricSection (the profile#PROFILE_OWNER concrete-outline owner ProfileOf lowers to IProfile)
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

// The reinforced-concrete section receipt: the assembled VividOrange.Sections IConcreteSection plus the resolved
// gross steel area the Profiles/capacity solvers and the QTO seam read, never a re-derived bar-area sum.
public sealed record RcSection(IConcreteSection Section, EnConcreteMaterial Concrete, EnRebarMaterial Rebar, double GrossSteelAreaMm2, double CoverMm) {
    public Profile ConcreteProfile { get; init; }   // the Materials Profile the concrete outline came from (QTO key)
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN-grade admission boundary: the VividOrange.Materials grade ctors throw on an unknown grade / untabulated annex
// (the .api [BOUNDARY_EXCEPTION_LAW]); EnGrade traps the throw ONCE and lowers it onto the typed ConnectionFault rail,
// so no VividOrange throw and no non-EN grade reaches the RcSection.Of interior.
public static class EnGrade {
    public static Fin<EnConcreteMaterial> Concrete(EnConcreteGrade grade, NationalAnnex annex, Op key) =>
        Try(() => new EnConcreteMaterial(grade, annex)).ToFin()
            .MapFail(e => ConnectionFault.Grade(key, $"<en-concrete-grade:{grade}:{annex}:{e.Message}>"));

    public static Fin<EnRebarMaterial> Rebar(Option<EnRebarGrade> grade, NationalAnnex annex, Op key) =>
        grade.Match(
            Some: g => Try(() => new EnRebarMaterial(g, annex)).ToFin()
                .MapFail(e => ConnectionFault.Grade(key, $"<en-rebar-grade:{g}:{annex}:{e.Message}>")),
            None: () => Fin.Fail<EnRebarMaterial>(ConnectionFault.Grade(key, "<rebar-grade-not-en-bodied-for-rc-section>")));
}

public static class RcSectionBuilder {
    // The ONE reinforced-section boundary: lower the EN grades, build the ConcreteSection from the Profile's IProfile +
    // concrete IMaterial + a Link + cover, fold each RebarLayout to its placement engine and AddRebarLayer it, read back
    // section.Rebars. Every VividOrange throw (InvalidMaterialTypeException/InvalidProfileTypeException/ArgumentException)
    // is trapped here onto ConnectionFault — the IConcreteSection egress carries only validated DATA.
    public static Fin<RcSection> Of(Profile concrete, EnConcreteGrade concreteGrade, RebarGrade barGrade, BarSize linkSize, Seq<RebarLayout> layout, double coverMm, NationalAnnex annex, Op key) =>
        from concreteMaterial in EnGrade.Concrete(concreteGrade, annex, key)
        from rebarMaterial in EnGrade.Rebar(barGrade.EnGrade, annex, key)
        from profile in ParametricSection.ProfileOf(concrete, key)   // profile#PROFILE_OWNER IProfile perimeter
        from built in Try(() => Build(profile, concreteMaterial, rebarMaterial, linkSize, layout, coverMm)).ToFin()
            .MapFail(e => ConnectionFault.Capacity(key, $"<rc-section-build:{concrete.Family.Key}:{e.Message}>"))
        let area = built.Rebars.Sum(r => Math.PI * 0.25 * r.Rebar.Diameter.Millimeters * r.Rebar.Diameter.Millimeters * r.CountPerBundle)
        select new RcSection(built, concreteMaterial, rebarMaterial, area, coverMm) { ConcreteProfile = concrete };

    static ConcreteSection Build(IProfile profile, EnConcreteMaterial concrete, EnRebarMaterial rebar, BarSize linkSize, Seq<RebarLayout> layout, double coverMm) {
        Link link = new(rebar, LinkDiameter(linkSize, rebar));
        ConcreteSection section = new(profile, concrete, link, Length.FromMillimeters(coverMm));
        layout.Iter(l => section.AddRebarLayer(LayerOf(l, rebar)));
        return section;
    }

    // Each RebarLayout case -> its VividOrange.Sections placement engine; a metric BarSize feeds the catalogued
    // BarDiameter ctor, an imperial/CSA BarSize a raw Length(nominalDiameterMm) — one Rebar per layout, never a literal.
    // The generated [Union] Switch is the totality proof: a fifth RebarLayout case breaks this arm at compile time,
    // never a runtime-silent `_` (the deleted `layout switch { … _ => throw }` form).
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
            .MapFail(e => ConnectionFault.Capacity(key, $"<min-bar-spacing:{annex}:{size.Key}:{e.Message}>"));
}
```

## [04]-[RESEARCH]

- [REBAR_ROW_TRANSCRIPTION]: REALIZED — the ASTM A615/A706 + CSA G30.18 + EN-10080 reinforcing-bar catalogue (the #3..#18 imperial, 10M..55M CSA-metric, and H10..H25 EN nominal columns, the A615 Gr40/60/75/80, A706 Gr60/80 weldable, CSA 400W/500W weldable, and EN B500B/B500C grades) seeds through `ConnectionCatalogue.BuildRebarRows(context)` over the `RebarRow` designation/size/grade/usage/surface table, the nominal-diameter/area/weight columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the grade/size/usage/surface/hook algebra realized as the reinforcement vocabulary; the bar rows are the seed the connection-layout fold consumes and a new bar is one `RebarRow` data addition plus, if novel, one `BarSize`/`RebarGrade`/`RebarUsage` row. The nominal diameter/area/unit-weight values transcribe the published ASTM A615/A706 (US-customary soft-metric), CSA G30.18 (metric 10M..55M), and EN-10080 (H10..H25, `A = πd²/4`, `w = 0.00617·d²` kg/m) dimensions; a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`. The `BarSize.BendFactor` is the ACI 318-19 Table 25.3.1 minimum-inside-bend-diameter multiple (6dᵦ for #3..#8, 8dᵦ for #9..#11, 10dᵦ for #14/#18 and the matching metric bands); the `RebarHook.ExtensionFactor` is the ACI 318-19 Table 25.3.2 standard-hook straight extension (12dᵦ for a 90° hook, 6dᵦ for 135°, 4dᵦ for 180°, the latter floored at 65 mm at the host materialization, never here).
- [REBAR_BEND_HOST_MATERIALIZATION]: REALIZED — the `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm, ShapeCode)` scalar tuple is the host-neutral bend receipt — the host boundary lofts the bar centerline polyline and fillets each bend by the inside-bend-diameter radius into the host `Curve`, exactly as `Construction/layout#ASSEMBLY_FOLD` materializes a `Seq<Placement>` of scalar tuples into host geometry. The `ShapeCode` is the BS 8666 / ISO 3766 bar-bending schedule designation the host shape-table selects the bent polyline from (`13`/`14`/`15` single end hooks, `51` closed link) and the `IfcReinforcingBar` `BendingShapeCode` wire carries; a `RebarUsage.Stirrup` bar emits `51` so its schedule is the closed perimeter, a longitudinal bar the hook-angle-keyed code. This owner NEVER constructs a host curve: the scalar tuple is the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the reinforcement catalogue stays a leaf below the host boundary. A bent-bar schedule (a stirrup, a column tie, a development hook) is a `RebarBend` per bend plus a `Construction/layout` station-stepped centerline, never a per-shape rebar curve type.
- [IFC_REINFORCING_WIRE]: REALIZED — a reinforcing bar round-trips to IFC 4.3 as an `IfcReinforcingBar` carrying the GeometryGym-public scalars `NominalDiameter` (the `RebarSection.DiameterMm`), `CrossSectionArea` (`NominalAreaMm2`), `BarLength`, and `PredefinedType` (the `RebarUsage.IfcPredefinedType` token, the GeometryGym-verified `IfcReinforcingBarTypeEnum` ∈ {MAIN, LIGATURE, SPACEBAR, PUNCHING, EDGE, RING, ANCHORING, SHEAR, STUD, USERDEFINED, NOTDEFINED} per `api-geometrygym-ifc.md`); the `BarSurface` (`RebarSurface.IfcSurface`, the IFC4.3 `IfcReinforcingBarSurfaceEnum` ∈ {PLAIN, TEXTURED}), the `BendingShapeCode` (`RebarBend.ShapeCode`), and the `SteelGrade` (`RebarGrade.Key`) ride the `Rasm.Bim` egress's IFC mapping (the bar attributes the egress sets), this page carrying only the portable token columns. A reinforcement mesh round-trips as an `IfcReinforcingMesh` over the GeometryGym-public scalars `MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarCrossSectionArea` (`api-geometrygym-ifc.md`) reading the same `RebarSection` columns. The wire mapping is the `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrengthMpa` the `SteelGrade` asserts), host-neutral here — this page emits the typed `RebarUsage`/`RebarSurface`/`RebarBend.ShapeCode` columns the wire reads through the verified enum tokens, never an IFC entity.
- [RC_SECTION_COMPOSITION]: REALIZED — the `[03]-[RC_SECTION]` `RcSection` owner is the host-neutral reinforced-concrete-section assembler over the `VividOrange.Sections` `ConcreteSection`: `RcSectionBuilder.Of` lowers an `EnConcreteGrade`/`EnRebarGrade` through `EnGrade.Concrete`/`Rebar` to the `VividOrange.Materials` `EnConcreteMaterial`/`EnRebarMaterial` DATA (trapping the `ArgumentException`/`MissingNationalAnnexException` derivation throws onto `ConnectionFault.Grade`), builds the `ConcreteSection` from the `profile#PROFILE_OWNER` `ParametricSection.ProfileOf` `IProfile` concrete outline + the concrete `IMaterial` + a `Link` over the EN-10080 `BarDiameter` + the `coverMm` `Length`, and folds the closed `RebarLayout` `[Union]` (the four `FaceReinforcementLayer`/`PerimeterReinforcementLayer` × count/spacing constructors collapsed into one) through `AddRebarLayer` — replacing the hand-rolled RC-section assembly the `.api/api-vividorange-sections.md` `[RAIL_LAW]` `Reject` clause names. The `IConcreteSection` egress is the input the `Profiles/capacity#SECTION_CAPACITY` `ConcreteSectionProperties` transformed-section solver and the `VividOrange.InteractionDiagram` N-M-M capacity engine consume; the EC2 clear bar spacing reads `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing` rather than an inline constant. A metric `BarSize.Catalogue` resolves a `Rebar(IMaterial, BarDiameter)` over the published catalogue, an imperial/CSA size a raw `Length(nominalDiameterMm)`; the `RcSection` is NOT a `ConnectionItem` (a discrete schedule bar) — it is the populated concrete member, the two meeting at the `BarSize`/`RebarGrade` vocabulary. `RcSection.Of` admits ANY `profile#PROFILE_OWNER` `Profile` as its concrete outline because `ParametricSection.ProfileOf(concrete, key)` is FAMILY-AGNOSTIC — it reads `concrete.Unit.WidthMm`/`HeightMm` into the rectangle `IProfile` regardless of `ProfileFamily` — so a `cmu#CMU_GROUT_REINFORCEMENT` grouted CMU unit (a `ProfileFamily.Cmu` `Profile` whose fully-grouted `GroutedSection` net solid IS its `Unit` width×height rectangle) admits as the reinforced-masonry concrete input the SAME way the parametric concrete outline does, so a TMS 402 reinforced CMU shear wall resolves its reinforcement transform through this ONE `RcSection.Of` boundary — no cmu-specific RC builder, the grouted rectangle the concrete outline the `ConcreteSection`/`RebarLayout` reinforcement transform composes over (the EN concrete-grade admission still applies, so a CMU grouted wall carries its grout `EnConcreteGrade` as the section concrete). Ripple counterpart: `Profiles/capacity` `[SECTION_CAPACITY]` (the elastic transformed-section + ultimate N-M-M capacity solvers over the `IConcreteSection` minted here) and `Profiles/cmu` `[CMU_GROUT_REINFORCEMENT]` (the grouted CMU `Profile` outline `RcSection.Of` admits for the reinforced-masonry transform).
- [EN_GRADE_BINDING]: REALIZED — `RebarGrade` carries an `Option<EnRebarGrade> EnGrade` (`Some` for the EN-bodied `B500B`/`B500C` bands bound to the verified `EnRebarGrade.B500B`/`B500C`, `None` for the ASTM/CSA spec-nominal bands), so the RC σ(ε)-law path reads a registered EN grade through `RcSectionBuilder.Of` rather than a hand-keyed `f_yk`, while the spec-nominal `MinimumYieldMpa` band stays the connection-schedule receipt — the two strengths disjoint by source (spec-nominal for the schedule, EN-registered for the RC capacity); the `ConnectionCatalogue.RebarRows` seed carries the EN H12/H16/H25 `B500B`/`B500C` rows (and an H10 `B500B` plain link) so the `En10080` standard + the `B500B`/`B500C` grades are exercised catalogue rows, never a declared-but-unseeded phantom. `BarSize` carries an `Option<BarDiameter> Catalogue` binding the metric/EN rows to the EN-10080 D6..D50 catalogue the `VividOrange.Sections` `Rebar(IMaterial, BarDiameter)` ctor consumes (the H10..H25 EN sizes keyed straight to `BarDiameter.D10`..`D25`), so a metric/EN bar resolves its diameter from the published catalogue rather than a raw millimetre literal at the `Rebar` boundary; the EN-bodied `RebarStandard.En10080` band cites `En1992` at the `RcSection` `Concrete`/`Rebar` admission.
