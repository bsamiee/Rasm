# [MATERIALS_PANEL]

THE PANEL SEED PAGE owns the `ComponentFamily.Panel` row facts, the product vocabularies, the frozen standards rows, `FastenPattern`, the closed `PanelSpecification` payload, and the `PanelSeed.Roster`/`Law` pair the ONE `component#COMPONENT_SEED` generator folds. Board geometry is `SectionProfile.Layered` over the shared bounded `PlyRole`; deck geometry is `SectionProfile.Corrugated`, solved by the canonical `SectionSolver` arm. `Sectioned` follows structural kind, every row carries its kind-owned `IfcBinding`, and every product detail measurement remains on the `Fin` result through catalogue construction.

The LAYUP is the SPECIFICATION's, not the kind's: the twenty per-kind layup delegates were four bodies written twenty times, so the ply stack now dispatches once on the payload arm and reads two data columns off the kind — its core `PlyRole` and its board facing — while the kind keeps the ONE policy column that is genuinely per-kind, the payload it `Admits`. The membrane option-coherence those admissions each restated (a vapour class rides the vapour-retarder duty alone, a flashing form the flashing duty alone) is now one identity-derived read on `MembraneDuty` lifted as a seed coherence conjunct, so an incoherent pairing names itself instead of failing an admission that cannot say why.

## [01]-[INDEX]

- [02]-[PANEL_FAMILY]: the `PanelKind` board-type vocabulary; the edge, orientation, fastening, core, bond, foam, facer, deck, membrane-duty, vapour-class, and flashing policies; the frozen deck/span tables and the SDPWS lateral table; `FastenPattern`; the payload-timed `PanelSpecification` union with its layup, coherence, facing, and detail projections; the `PanelRow` roster row; `PanelDetail`; and the `PanelSeed` roster, law, and capacity producer.

## [02]-[PANEL_FAMILY]

- Owner: `PanelKind` carries the board-type axis, IFC leaf, structural authority, substance, core `PlyRole`, board facing, and the `[UseDelegateFromConstructor]` payload admission; the shared `PlyRole` bounds every layer role across panel, glazing, timber, and masonry. `EdgeProfile`, `PanelOrientation`, `PanelFastening`, `DeckForm`, `CoreType`, `BondClass`, `FoamType`, `Facer`, `MembraneDuty`, `VaporClass`, and `FlashingKind` carry product policy; `DeckProfiles`, `SpanRatings`, `LateralShear`, `FastenPattern`, `PanelSpecification`, `PanelDetail`, and `PanelSeed` carry printed data, admission, payload timing, details, and construction.
- Cases: kind {the tri-entity IFC spread; a fiberboard or magnesium-oxide board is one new row reusing `IfcCovering`/CLADDING and an existing core role} · edge {square/tapered/beveled/rounded/tongue-groove/shiplap/side-lap-interlock/lapped-seam} · orientation {strength-axis-perpendicular/parallel/unidirectional} · duty {roofing/air-barrier/vapour-retarder/waterproofing/flashing — the membrane service axis, each barrier duty stamping its `BarrierClass` token} · profile {`Layered` every covering/membrane board — the specification's own ply stack; `Corrugated` the two steel-deck kinds — the `DeckProfiles` row + `GaugeRow` fill the six named dims}.
- Entry: `ComponentSeed.Rows(context, PanelSeed.Roster, PanelSeed.Law)`. The law's coherence proves kind/payload admission, payload coherence, dimensional sanity, the fastening schedule, and the deck-row dimensional agreement TOGETHER, so a malformed board names every column it broke in one verdict; the profile route then resolves `PanelSpecification.DeckSheet` to `Corrugated` and every other payload to `Layered`, and the detail builds the bag from the union's own projections.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Context`/`FactoryBridge.Accept`, the kernel `Tolerance`/`ToleranceLane` the deck-drift band admits through), Rasm.Element (project — `MaterialId`, `EvidenceGrade`, `PropertyBag`, and the contract `DetailSchema`/`PropertyName`/`PropertyValue`/`Dimension` currencies `PanelDetail` composes; the one producer-scoped mint is `PanelRows.FlashingKind` through the owner-blessed `PropertyCategory.Materials` scope), the parent `component#COMPONENT_OWNER`/`#MATERIAL_GRADE`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]`, `[Union]`, `[ComplexValueObject]` + generated `ValidateFactoryArguments`/`Validate`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Validation`/`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`, `ImmutableArray`); NO sheet-goods external producer — the roster is `SEED_ROW_LAW` data (VividOrange owns structural-MEMBER catalogues and EN grades, not gypsum/sheathing/deck/insulation rosters); the deck base metal reuses the `connector#CONNECTOR_FAMILY` `GaugeRow`/`Gauges` table (one cold-formed gauge vocabulary — `Ga22`..`Ga10` named statics, PUBLISHED AISI base/design thickness and gauge-band yield).
- Growth: a new board is one `PanelRow`; a new kind one `PanelKind` row binding its IFC leaf, core role, facing, and admitted payload; a new edge/orientation/core/bond/foam/facer/duty/vapour-class/flashing band one vocabulary row; a new deck profile one `DeckProfileRow`; a new span rating one `SpanRow`; a new layup SHAPE is one `PanelSpecification` arm and one `Layup` case the compiler forces — ZERO type edits for a new board. A NON-BOARD insulation form (batt, roll, loose-fill, spray) is an `insulation` family row at `insulation#INSULATION_FAMILY` — the board split law — and an APPLIED liquid membrane seeds no sheet row here.
- Boundary: this page emits DATA — profiles, vocabulary rows, bags, and the seed law; the section INTEGRAL is `component#SECTION_SOLVER`'s `corrugated` arm, the twenty-column `ComputedSection` lift is `SectionSolver.Admit`, and the per-coverage rib scaling reads `CoverWidthMm`/`RibPitchMm` off the `Corrugated` dims inside `Forms.ThinFold` — panel keeps only the `DeckProfiles`/`Gauges` DATA those dims read. A deck row's solved `ComputedSection` is PRICED, not merely stored — the `steel#STEEL_FAMILY` AISI overload reads it at the rib row's own `MaterialGrade` and the design capacity lifts into `capacity#SECTION_CAPACITY` through `CapacityLift.DeckSheet`, so `GaugeRow.AxialSectionCapacityKnPerMm` is a CONSUMED datum and a deck's flexural/shear verdict rides the one `Check(demand)` result. The board substance physics read ONCE from the property library by `SubstanceId`, never re-keyed here; `SubstanceId`/`AppearanceId` stay INDEPENDENT slots (a foil-faced polyiso keeps its foam substance while its appearance names the facer; a deck's substance is its gauge steel), and the appearance derives from the ONE `FacingMaterial` owner the layup itself reads rather than from a second probe over the built plies. The layup is the typed `Seq<Ply>` the contract `CompositionAuthor.LayerSet` coerces into `IfcMaterialLayerSet` (a deck is a `ProfileSet` — a ribbed sheet profiles, never layers); `IfcBinding` strings stay neutral (the `IfcCoveringTypeEnum` has NO SHEATHING member so lining and sheathing are both CLADDING, the `IfcSlabTypeEnum` has NO COMPOSITE/DECK member so a composite floor deck is `IfcSlab`/FLOOR); the product bag rides the Type `Object` via `Assign.PropertyDefinition` and round-trips through the GENERAL Bim `Object`/property fold.

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
using Dimension = Rasm.Element.Properties.Dimension;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelKind {
    public static readonly PanelKind GypsumBoard        = new("gypsum-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.GypsumCore),            facing: Some((PlyRole.PaperFace, "paper.face")),      admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer == Facer.None);
    public static readonly PanelKind GypsumCeiling      = new("gypsum-ceiling",      ifcEntity: "IfcCovering", ifcPredefinedType: "CEILING",    substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.GypsumCore),            facing: Some((PlyRole.PaperFace, "paper.face")),      admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer == Facer.None);
    public static readonly PanelKind GypsumSheathing    = new("gypsum-sheathing",    ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.GypsumCore),            facing: Some((PlyRole.GlassMatFacer, "glass.mat")),   admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer != Facer.None);
    public static readonly PanelKind PlywoodSheathing   = new("plywood-sheathing",   ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.plywood",   authority: ComponentAuthority.Apa,  coreRole: Some(PlyRole.VeneerPly),             facing: None,                                         admits: static s => s is PanelSpecification.WoodPanel);
    public static readonly PanelKind OsbSheathing       = new("osb-sheathing",       ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.osb",       authority: ComponentAuthority.Apa,  coreRole: Some(PlyRole.StrandLayer),           facing: None,                                         admits: static s => s is PanelSpecification.WoodPanel);
    public static readonly PanelKind CementBoard        = new("cement-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.CementAggregateCore),   facing: Some((PlyRole.GlassMeshScrim, "glass.scrim")), admits: static s => s is PanelSpecification.FacedBoard);
    public static readonly PanelKind CementUnderlayment = new("cement-underlayment", ifcEntity: "IfcCovering", ifcPredefinedType: "FLOORING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.CementAggregateCore),   facing: Some((PlyRole.GlassMeshScrim, "glass.scrim")), admits: static s => s is PanelSpecification.FacedBoard);
    public static readonly PanelKind SteelDeckRoof      = new("steel-deck-roof",     ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi,  coreRole: None,                                facing: None,                                         admits: static s => s is PanelSpecification.DeckSheet { Form.FloorDeck: false });
    public static readonly PanelKind SteelDeckFloor     = new("steel-deck-floor",    ifcEntity: "IfcSlab",     ifcPredefinedType: "FLOOR",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi,  coreRole: None,                                facing: None,                                         admits: static s => s is PanelSpecification.DeckSheet { Form.FloorDeck: true });
    public static readonly PanelKind RigidBoardEps      = new("rigid-board-eps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.eps", authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.FoamCore),              facing: None,                                         admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Eps);
    public static readonly PanelKind RigidBoardXps      = new("rigid-board-xps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.xps", authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.FoamCore),              facing: None,                                         admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Xps);
    public static readonly PanelKind RigidBoardPoly     = new("rigid-board-poly",    ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.pir", authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.FoamCore),              facing: None,                                         admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Polyiso);
    public static readonly PanelKind MembraneEpdm       = new("membrane-epdm",       ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.epdm",  authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.Roofing);
    public static readonly PanelKind MembranePvc        = new("membrane-pvc",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.pvc",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.Roofing);
    public static readonly PanelKind MembraneTpo        = new("membrane-tpo",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.tpo",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.Roofing);
    public static readonly PanelKind AirBarrier         = new("air-barrier",         ifcEntity: "IfcCovering", ifcPredefinedType: "MEMBRANE",   substanceId: "membrane.wrap",  authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.AirBarrier);
    public static readonly PanelKind VapourRetarder     = new("vapour-retarder",     ifcEntity: "IfcCovering", ifcPredefinedType: "MEMBRANE",   substanceId: "membrane.pe",    authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.VapourRetarder);
    public static readonly PanelKind Waterproofing      = new("waterproofing",       ifcEntity: "IfcCovering", ifcPredefinedType: "MEMBRANE",   substanceId: "membrane.sbs",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.Waterproofing);
    public static readonly PanelKind FlashingMembrane   = new("flashing-membrane",   ifcEntity: "IfcCovering", ifcPredefinedType: "MEMBRANE",   substanceId: "membrane.sbs",   authority: ComponentAuthority.Astm, coreRole: Some(PlyRole.MembraneCore),          facing: None,                                         admits: static s => s is PanelSpecification.Membrane { Duty: var duty } && duty == MembraneDuty.Flashing);

    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public string SubstanceId { get; }
    public ComponentAuthority Authority { get; }
    public Option<PlyRole> CoreRole { get; }
    public Option<(PlyRole Role, string MaterialId)> Facing { get; }
    public MaterialId Substance => MaterialId.Create(SubstanceId);

    [UseDelegateFromConstructor]
    public partial bool Admits(PanelSpecification specification);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeProfile {
    public static readonly EdgeProfile Square           = new("square",             lapped: false);
    public static readonly EdgeProfile Tapered          = new("tapered",            lapped: false);
    public static readonly EdgeProfile Beveled          = new("beveled",            lapped: false);
    public static readonly EdgeProfile Rounded          = new("rounded",            lapped: false);
    public static readonly EdgeProfile TongueGroove     = new("tongue-groove",      lapped: true);
    public static readonly EdgeProfile Shiplap          = new("shiplap",            lapped: true);
    public static readonly EdgeProfile SideLapInterlock = new("side-lap-interlock", lapped: true);
    public static readonly EdgeProfile LappedSeam       = new("lapped-seam",        lapped: true);
    public bool Lapped { get; }

    const double ButtControlGapMm = 3.0;
    public double GapMm => Lapped ? 0.0 : ButtControlGapMm;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelOrientation {
    public static readonly PanelOrientation StrengthAxisPerpendicular = new("strength-axis-perpendicular");
    public static readonly PanelOrientation StrengthAxisParallel      = new("strength-axis-parallel-to-span");
    public static readonly PanelOrientation Unidirectional            = new("unidirectional");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoreType {
    public static readonly CoreType Regular           = new("regular",            fireRated: false);
    public static readonly CoreType TypeXFire         = new("type-x-fire",        fireRated: true);
    public static readonly CoreType TypeCFire         = new("type-c-fire",        fireRated: true);
    public static readonly CoreType MoistureResistant = new("moisture-resistant", fireRated: false);
    public static readonly CoreType AbuseResistant    = new("abuse-resistant",    fireRated: false);
    public static readonly CoreType GlassMat          = new("glass-mat",          fireRated: false);
    public static readonly CoreType WaterResistant    = new("water-resistant",    fireRated: false);
    public bool FireRated { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondClass {
    public static readonly BondClass Exposure1 = new("exposure-1");
    public static readonly BondClass Exterior  = new("exterior");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FoamType {
    const double InchToMm = 25.4;
    const double RValueIpToSi = 0.17611;
    const double PsiToMpa = 0.00689476;
    public static readonly FoamType Eps     = new("eps",     rValuePerInch: 3.85, compressiveStrengthPsi: 15.0);
    public static readonly FoamType Xps     = new("xps",     rValuePerInch: 5.0,  compressiveStrengthPsi: 25.0);
    public static readonly FoamType Polyiso = new("polyiso", rValuePerInch: 5.7,  compressiveStrengthPsi: 20.0);
    public double RValuePerInch { get; }
    public double CompressiveStrengthPsi { get; }
    public double RValueSi(double thicknessMm) => RValuePerInch * (thicknessMm / InchToMm) * RValueIpToSi;
    public double CompressiveStrengthMpa => CompressiveStrengthPsi * PsiToMpa;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Facer {
    public static readonly Facer None          = new("none",            faces: 0, role: PlyRole.FoamCore);
    public static readonly Facer Kraft         = new("kraft",           faces: 1, role: PlyRole.PaperFace);
    public static readonly Facer Foil          = new("foil",            faces: 2, role: PlyRole.FoilFacer);
    public static readonly Facer GlassFiberMat = new("glass-fiber-mat", faces: 2, role: PlyRole.GlassFiberMatFacer);
    public static readonly Facer CoatedGlass   = new("coated-glass",    faces: 2, role: PlyRole.CoatedGlassFacer);
    public int Faces { get; }
    public PlyRole Role { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MembraneDuty {
    public const double AirPermeanceCeilingLpsM2 = 0.02;
    public static readonly MembraneDuty Roofing        = new("roofing",         barrier: false);
    public static readonly MembraneDuty AirBarrier     = new("air-barrier",     barrier: true);
    public static readonly MembraneDuty VapourRetarder = new("vapour-retarder", barrier: true);
    public static readonly MembraneDuty Waterproofing  = new("waterproofing",   barrier: true);
    public static readonly MembraneDuty Flashing       = new("flashing",        barrier: false);
    public bool Barrier { get; }

    public bool Coherent(Option<VaporClass> vapor, Option<FlashingKind> flashing) =>
        vapor.IsSome == (this == VapourRetarder) && flashing.IsSome == (this == Flashing);

    public Option<string> BarrierToken(Option<VaporClass> vapor) =>
        Barrier ? Some(vapor.Map(static v => v.Key).IfNone(Key)) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VaporClass {
    const double UsPermToNgPaSM2 = 57.2;
    public static readonly VaporClass ClassI   = new("vapor-class-i",   permFloorUsPerm: 0.0, permCeilingUsPerm: 0.1);
    public static readonly VaporClass ClassII  = new("vapor-class-ii",  permFloorUsPerm: 0.1, permCeilingUsPerm: 1.0);
    public static readonly VaporClass ClassIII = new("vapor-class-iii", permFloorUsPerm: 1.0, permCeilingUsPerm: 10.0);
    public double PermFloorUsPerm { get; }
    public double PermCeilingUsPerm { get; }
    public double PermCeilingNgPaSM2 => PermCeilingUsPerm * UsPermToNgPaSM2;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlashingKind {
    public static readonly FlashingKind DripEdge         = new("drip-edge");
    public static readonly FlashingKind Step             = new("step");
    public static readonly FlashingKind Counter          = new("counter");
    public static readonly FlashingKind Valley           = new("valley");
    public static readonly FlashingKind ThroughWall      = new("through-wall");
    public static readonly FlashingKind SelfAdheredStrip = new("self-adhered-strip");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelFastening {
    public static readonly PanelFastening DrywallScrew      = new("drywall-screw",        kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(3.5),   appearanceId: "metal.steel");
    public static readonly PanelFastening StructuralNail    = new("structural-nail",      kind: Some(FastenerKind.Nail),  welded: false, shankMm: Some(3.33),  appearanceId: "metal.steel");
    public static readonly PanelFastening StructuralNail10d = new("structural-nail-10d",  kind: Some(FastenerKind.Nail),  welded: false, shankMm: Some(3.76),  appearanceId: "metal.steel");
    public static readonly PanelFastening RoofingNail       = new("roofing-nail",         kind: Some(FastenerKind.Nail),  welded: false, shankMm: Some(3.05),  appearanceId: "metal.steel");
    public static readonly PanelFastening DeckWeld          = new("deck-weld",            kind: None,                     welded: true,  shankMm: None,        appearanceId: "metal.steel");
    public static readonly PanelFastening DeckScrew         = new("deck-screw",           kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(4.83),  appearanceId: "metal.steel");
    public static readonly PanelFastening PlateAndScrew     = new("plate-and-screw",      kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(4.83),  appearanceId: "metal.steel");
    public static readonly PanelFastening Adhesive          = new("adhesive",             kind: None,                     welded: false, shankMm: None,        appearanceId: "adhesive.bead");
    public static readonly PanelFastening HeatWeld          = new("heat-weld",            kind: None,                     welded: true,  shankMm: None,        appearanceId: "membrane.seam");
    public static readonly PanelFastening SeamAdhesive      = new("seam-adhesive",        kind: None,                     welded: false, shankMm: None,        appearanceId: "adhesive.bead");
    public Option<FastenerKind> Kind { get; }
    public bool Welded { get; }
    public Option<double> ShankDiameterMm { get; }
    public string AppearanceId { get; }

    public Option<string> FastenerToken => Kind.Map(static kind => kind.IfcPredefinedType);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeckForm {
    public static readonly DeckForm Form      = new("form",      floorDeck: false);
    public static readonly DeckForm Composite = new("composite", floorDeck: true);
    public static readonly DeckForm Roof      = new("roof",      floorDeck: false);
    public bool FloorDeck { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DeckProfileRow(
    string Key, double RibDepthMm, double RibPitchMm, double CoverageMm,
    double TopFlatMm, double BottomFlatMm, MaterialGrade Grade, EvidenceGrade Source);

public static class DeckProfiles {
    public static readonly DeckProfileRow NarrowRibA    = new("narrow-rib-a",   38.1, 152.4, 914.4, 127.0,  9.5,  MaterialGrade.A653Gr33, EvidenceGrade.Catalogue);
    public static readonly DeckProfileRow IntermediateF = new("intermediate-f", 38.1, 152.4, 914.4, 108.0, 12.7,  MaterialGrade.A653Gr33, EvidenceGrade.Catalogue);
    public static readonly DeckProfileRow WideRibB      = new("wide-rib-b",     38.1, 152.4, 914.4,  88.9, 39.7,  MaterialGrade.A653Gr50, EvidenceGrade.Catalogue);
    public static readonly DeckProfileRow DeepN         = new("deep-n",         76.2, 203.2, 609.6, 133.4, 38.1,  MaterialGrade.A653Gr50, EvidenceGrade.Catalogue);
    public static readonly DeckProfileRow Composite15   = new("composite-15",   38.1, 152.4, 914.4,  88.9, 44.5,  MaterialGrade.A653Gr50, EvidenceGrade.User);
    public static readonly DeckProfileRow Composite2Vli = new("composite-2vli", 50.8, 304.8, 914.4, 127.0, 127.0, MaterialGrade.A653Gr50, EvidenceGrade.User);
    public static readonly DeckProfileRow Composite3Vli = new("composite-3vli", 76.2, 304.8, 914.4, 120.7, 120.7, MaterialGrade.A653Gr50, EvidenceGrade.User);
    public static readonly ImmutableArray<DeckProfileRow> Rows = [NarrowRibA, IntermediateF, WideRibB, DeepN, Composite15, Composite2Vli, Composite3Vli];
}

public readonly record struct SpanRow(string Key, int RoofEdgeSupportedIn, int RoofUnsupportedIn, Option<int> FloorSpanIn) {
    public double RoofEdgeSupportedMm => RoofEdgeSupportedIn * 25.4;
    public double RoofUnsupportedMm => RoofUnsupportedIn * 25.4;
    public Option<double> FloorSpanMm => FloorSpanIn.Map(static span => span * 25.4);
}

public static class SpanRatings {
    public static readonly SpanRow S12_0  = new("12/0",  12, 12, None);
    public static readonly SpanRow S16_0  = new("16/0",  16, 16, None);
    public static readonly SpanRow S20_0  = new("20/0",  20, 20, None);
    public static readonly SpanRow S24_0  = new("24/0",  24, 20, None);
    public static readonly SpanRow S24_16 = new("24/16", 24, 24, Some(16));
    public static readonly SpanRow S32_16 = new("32/16", 32, 28, Some(16));
    public static readonly SpanRow S40_20 = new("40/20", 40, 32, Some(19));
    public static readonly SpanRow S48_24 = new("48/24", 48, 36, Some(24));
    public static readonly SpanRow S54_32 = new("54/32", 54, 40, Some(32));
    public static readonly SpanRow S60_32 = new("60/32", 60, 48, Some(32));
    public static readonly SpanRow Floor16 = new("16oc", 24, 24, Some(16));
    public static readonly SpanRow Floor20 = new("20oc", 32, 32, Some(19));
    public static readonly SpanRow Floor24 = new("24oc", 48, 36, Some(24));
    public static readonly SpanRow Floor32 = new("32oc", 48, 40, Some(32));
    public static readonly SpanRow Floor48 = new("48oc", 60, 48, Some(48));
    public static readonly ImmutableArray<SpanRow> Rows = [
        S12_0, S16_0, S20_0, S24_0, S24_16, S32_16, S40_20, S48_24, S54_32, S60_32,
        Floor16, Floor20, Floor24, Floor32, Floor48];
}

// --- [LATERAL_UNIT_SHEAR]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WspGrade {
    public static readonly WspGrade StructuralI = new("structural-i");
    public static readonly WspGrade Sheathing   = new("sheathing");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheathingNail {
    public static readonly SheathingNail Sixd   = new("6d",  lengthIn: 2.000, shankIn: 0.113, headIn: 0.266, bearingIn: 1.250);
    public static readonly SheathingNail Eightd = new("8d",  lengthIn: 2.500, shankIn: 0.131, headIn: 0.281, bearingIn: 1.375);
    public static readonly SheathingNail Tend   = new("10d", lengthIn: 3.000, shankIn: 0.148, headIn: 0.312, bearingIn: 1.500);
    public double LengthIn { get; }
    public double ShankIn { get; }
    public double HeadIn { get; }
    public double BearingIn { get; }
    public double ShankMm => ShankIn * LateralShear.InchToMm;
    public double BearingLengthMm => BearingIn * LateralShear.InchToMm;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralAssembly {
    public static readonly LateralAssembly ShearWall          = new("shear-wall");
    public static readonly LateralAssembly BlockedDiaphragm   = new("blocked-diaphragm");
    public static readonly LateralAssembly UnblockedDiaphragm = new("unblocked-diaphragm");
}

public readonly record struct ShearWallRow(
    WspGrade Grade, double PanelThicknessIn, SheathingNail Nail,
    Option<double> At6In, Option<double> At4In, Option<double> At3In, Option<double> At2In, EvidenceGrade Source);

public readonly record struct BlockedDiaphragmRow(
    WspGrade Grade, SheathingNail Nail, double PanelThicknessIn, double FramingWidthIn,
    Option<double> At6And6, Option<double> At4And6, Option<double> At2Half4, Option<double> At2And3, EvidenceGrade Source);

public readonly record struct UnblockedDiaphragmRow(
    WspGrade Grade, SheathingNail Nail, double PanelThicknessIn, double FramingWidthIn,
    Option<double> Case1, Option<double> Cases2To6, EvidenceGrade Source);

public static class LateralShear {
    public const double InchToMm = 25.4;
    public const double PlfToKnPerM = 0.0145939;

    static Option<double> Plf(double plf) => Some(plf * PlfToKnPerM);
    static readonly Option<double> Uncorroborated = None;

    public static readonly ImmutableArray<ShearWallRow> ShearWalls = [
        new(WspGrade.StructuralI, 0.3125, SheathingNail.Sixd,   Plf(560),  Plf(840),  Plf(1090), Plf(1430), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, 0.3750, SheathingNail.Eightd, Plf(645),  Plf(1010), Plf(1290), Plf(1710), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, 0.4375, SheathingNail.Eightd, Plf(715),  Plf(1105), Plf(1415), Plf(1875), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, 0.4688, SheathingNail.Eightd, Plf(785),  Plf(1205), Plf(1540), Plf(2045), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, 0.4688, SheathingNail.Tend,   Plf(950),  Plf(1430), Plf(1860), Plf(2435), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.3125, SheathingNail.Sixd,   Plf(505),  Plf(755),  Plf(980),  Plf(1260), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.3750, SheathingNail.Sixd,   Plf(560),  Plf(840),  Plf(1090), Plf(1430), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.3750, SheathingNail.Eightd, Plf(615),  Plf(895),  Plf(1150), Plf(1485), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.4375, SheathingNail.Eightd, Plf(670),  Plf(980),  Plf(1260), Plf(1640), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.4688, SheathingNail.Eightd, Plf(730),  Plf(1065), Plf(1370), Plf(1790), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.4688, SheathingNail.Tend,   Plf(870),  Plf(1290), Plf(1680), Plf(2155), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   0.5938, SheathingNail.Tend,   Plf(950),  Plf(1430), Plf(1860), Plf(2435), EvidenceGrade.Catalogue)];

    public static readonly ImmutableArray<BlockedDiaphragmRow> BlockedDiaphragms = [
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 2.0, Plf(520),  Plf(700),  Plf(1050), Plf(1175), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 3.0, Plf(590),  Plf(785),  Plf(1175), Plf(1330), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 2.0, Plf(755),  Plf(1010), Plf(1485), Plf(1680), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 3.0, Plf(840),  Plf(1120), Plf(1680), Plf(1890), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 2.0, Plf(895),  Plf(1190), Plf(1790), Plf(2045), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 3.0, Plf(1010), Plf(1345), Plf(2015), Plf(2295), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 3.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 3.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 3.0, Plf(755),  Plf(1010), Plf(1510), Plf(1710), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 3.0, Plf(800),  Plf(1065), Plf(1595), Plf(1805), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 2.0, Plf(755),  Plf(1010), Plf(1485), Plf(1680), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 3.0, Plf(840),  Plf(1120), Plf(1680), Plf(1890), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 2.0, Plf(810),  Plf(1080), Plf(1610), Plf(1835), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 3.0, Plf(910),  Plf(1205), Plf(1820), Plf(2060), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 2.0, Plf(895),  Plf(1190), Plf(1790), Plf(2045), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 3.0, Plf(1010), Plf(1345), Plf(2015), Plf(2295), EvidenceGrade.Catalogue)];

    public static readonly ImmutableArray<UnblockedDiaphragmRow> UnblockedDiaphragms = [
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 2.0, Plf(460), Plf(350), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 3.0, Plf(520), Plf(390), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 2.0, Plf(670), Plf(505), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 3.0, Plf(740), Plf(560), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 2.0, Plf(800), Plf(600), EvidenceGrade.Catalogue),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 3.0, Plf(895), Plf(670), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 2.0, Plf(420), Plf(310), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 3.0, Plf(475), Plf(350), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 2.0, Plf(460), Plf(350), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 3.0, Plf(520), Plf(390), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 2.0, Plf(600), Plf(450), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 3.0, Plf(670), Plf(505), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 2.0, Plf(645), Plf(475), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 3.0, Plf(715), Plf(530), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 2.0, Plf(670), Plf(505), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 3.0, Plf(740), Plf(560), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 2.0, Plf(715), Plf(530), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 3.0, Plf(810), Plf(600), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 2.0, Plf(800), Plf(600), EvidenceGrade.Catalogue),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 3.0, Plf(895), Plf(670), EvidenceGrade.Catalogue)];

    public static Fin<double> Nominal(
        WspGrade grade, double thicknessIn, SheathingNail nail, LateralAssembly assembly,
        double edgeSpacingIn, double framingWidthIn, int loadCase) =>
        assembly.Switch(
            state: (Grade: grade, ThicknessIn: thicknessIn, Nail: nail, EdgeIn: edgeSpacingIn, FramingIn: framingWidthIn, LoadCase: loadCase),
            shearWall: static x => Row(
                toSeq(ShearWalls).Find(r => r.Grade == x.Grade && Same(r.PanelThicknessIn, x.ThicknessIn) && r.Nail == x.Nail),
                r => Column(x.EdgeIn, [(6.0, r.At6In), (4.0, r.At4In), (3.0, r.At3In), (2.0, r.At2In)]), x),
            blockedDiaphragm: static x => Row(
                toSeq(BlockedDiaphragms).Find(r => r.Grade == x.Grade && Same(r.PanelThicknessIn, x.ThicknessIn) && r.Nail == x.Nail && Same(r.FramingWidthIn, x.FramingIn)),
                r => Column(x.EdgeIn, [(6.0, r.At6And6), (4.0, r.At4And6), (2.5, r.At2Half4), (2.0, r.At2And3)]), x),
            unblockedDiaphragm: static x => Row(
                toSeq(UnblockedDiaphragms).Find(r => r.Grade == x.Grade && Same(r.PanelThicknessIn, x.ThicknessIn) && r.Nail == x.Nail && Same(r.FramingWidthIn, x.FramingIn)),
                r => x.LoadCase <= 1 ? r.Case1 : r.Cases2To6, x));

    static Fin<double> Row<TRow>(
        Option<TRow> found, Func<TRow, Option<double>> cell,
        (WspGrade Grade, double ThicknessIn, SheathingNail Nail, double EdgeIn, double FramingIn, int LoadCase) x) =>
        found.Bind(cell).ToFin(new ComponentFault.LateralCellMissing(x.Key, x.Grade, x.Nail, x.ThicknessIn));

    static Option<double> Column(double spacingIn, ReadOnlySpan<(double SpacingIn, Option<double> Value)> columns) {
        Option<double> chosen = None;
        foreach ((double listed, Option<double> value) in columns) {
            if (spacingIn <= listed) { chosen = value; }
        }
        return chosen;
    }

    static bool Same(double a, double b) => Math.Abs(a - b) < 5e-4;
}

[ComplexValueObject]
public readonly partial struct FastenPattern {
    public double FieldSpacingMm { get; }
    public double EdgeSpacingMm { get; }
    public double EdgeDistanceMm { get; }
    public PanelFastening Fastener { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double fieldSpacingMm, ref double edgeSpacingMm, ref double edgeDistanceMm, ref PanelFastening fastener) =>
        validationError = fastener is not null && double.IsFinite(fieldSpacingMm) && fieldSpacingMm > 0.0 && double.IsFinite(edgeSpacingMm) && edgeSpacingMm > 0.0 && double.IsFinite(edgeDistanceMm) && edgeDistanceMm >= 0.0
            ? null
            : new ValidationError($"Fastening spacings must be finite and positive and edge distance finite and non-negative; received {fieldSpacingMm:R}, {edgeSpacingMm:R}, {edgeDistanceMm:R}.");

    public static Fin<FastenPattern> Of(double fieldMm, double edgeMm, double edgeDistMm, PanelFastening fastener) =>
        FactoryBridge.Accept<FastenPattern>(Validate(fieldMm, edgeMm, edgeDistMm, fastener, out FastenPattern pattern), pattern);

    public int EdgeStations(PositiveMagnitude axisLengthMm) => Math.Max(2, (int)Math.Floor(axisLengthMm.Value / EdgeSpacingMm) + 1);
    public int FieldStations(PositiveMagnitude axisLengthMm) => Math.Max(2, (int)Math.Floor(axisLengthMm.Value / FieldSpacingMm) + 1);
}

[Union]
public abstract partial record PanelSpecification {
    private PanelSpecification() { }
    public sealed record GypsumBoard(CoreType Core, Facer Facer) : PanelSpecification;
    public sealed record WoodPanel(SpanRow Span, BondClass Bond, WspGrade Grade, SheathingNail Nail) : PanelSpecification;
    public sealed record FacedBoard(Facer Facer) : PanelSpecification;
    public sealed record DeckSheet(DeckForm Form, DeckProfileRow Rib, GaugeRow Gauge) : PanelSpecification;
    public sealed record FoamBoard(FoamType Foam, Facer Facer) : PanelSpecification;
    public sealed record Membrane(MembraneDuty Duty, Option<VaporClass> Vapor, Option<FlashingKind> Flashing) : PanelSpecification;

    public Option<DeckSheet> Deck => this is DeckSheet deck ? Some(deck) : None;

    public bool Coherent() => Switch(
        membrane: static specification => specification.Duty.Coherent(specification.Vapor, specification.Flashing),
        gypsumBoard: static _ => true, woodPanel: static _ => true, facedBoard: static _ => true,
        deckSheet: static _ => true, foamBoard: static _ => true);

    public Option<Seq<Ply>> Layup(PanelKind kind, PositiveMagnitude thickness) => Switch(
        state: (Kind: kind, Thickness: thickness),
        gypsumBoard: static (x, _) => FaceCoreFace(x.Kind, x.Thickness),
        facedBoard:  static (x, _) => FaceCoreFace(x.Kind, x.Thickness),
        woodPanel:   static (x, _) => Mono(x.Kind, x.Thickness),
        membrane:    static (x, _) => Mono(x.Kind, x.Thickness),
        deckSheet:   static (_, _) => Some(Seq<Ply>()),
        foamBoard:   static (x, s) => FacedFoam(x.Kind, s.Facer, x.Thickness));

    public Option<MaterialId> FacingMaterial(PanelKind kind) => Switch(
        state: kind,
        gypsumBoard: static (k, _) => k.Facing.Map(static face => MaterialId.Create(face.MaterialId)),
        facedBoard:  static (k, _) => k.Facing.Map(static face => MaterialId.Create(face.MaterialId)),
        foamBoard:   static (_, s) => s.Facer.Faces > 0 ? Some(MaterialId.Create($"facer.{s.Facer.Key}")) : None,
        woodPanel:   static (_, _) => Option<MaterialId>.None,
        deckSheet:   static (_, _) => Option<MaterialId>.None,
        membrane:    static (_, _) => Option<MaterialId>.None);

    const double BoardFacingMm = 0.5;
    const double FoamFacingMm = 0.2;

    static Option<Seq<Ply>> FaceCoreFace(PanelKind kind, PositiveMagnitude thickness) =>
        from face in kind.Facing
        from core in kind.CoreRole
        from remainder in Some(thickness.Value - 2.0 * BoardFacingMm).Filter(static left => left > 0.0)
        select Seq(
            new Ply(MaterialId.Create(face.MaterialId), PositiveMagnitude.Create(BoardFacingMm), face.Role),
            new Ply(kind.Substance, PositiveMagnitude.Create(remainder), core),
            new Ply(MaterialId.Create(face.MaterialId), PositiveMagnitude.Create(BoardFacingMm), face.Role));

    static Option<Seq<Ply>> Mono(PanelKind kind, PositiveMagnitude thickness) =>
        kind.CoreRole.Map(role => Seq(new Ply(kind.Substance, thickness, role)));

    static Option<Seq<Ply>> FacedFoam(PanelKind kind, Facer facer, PositiveMagnitude thickness) =>
        facer.Faces is 0
            ? Mono(kind, thickness)
            : from role in kind.CoreRole
              from remainder in Some(thickness.Value - FoamFacingMm * facer.Faces).Filter(static left => left > 0.0)
              let skin = new Ply(MaterialId.Create($"facer.{facer.Key}"), PositiveMagnitude.Create(FoamFacingMm), facer.Role)
              let centre = new Ply(kind.Substance, PositiveMagnitude.Create(remainder), role)
              select facer.Faces >= 2 ? Seq(skin, centre, skin) : Seq(skin, centre);

    public Fin<Seq<(PropertyName Name, PropertyValue Value)>> DetailRows(PositiveMagnitude thicknessMm, PanelFastening fastening) => Switch(
        gypsumBoard: specification => Fin.Succ(
            Seq(ComponentDetail.Token(DetailSchema.CoreClass, specification.Core.Key))
            + FacerRow(specification.Facer)),
        woodPanel: specification =>
            from supported in ComponentDetail.Measured(DetailSchema.RoofSpan, Dimension.LengthDim, specification.Span.RoofEdgeSupportedMm * 1e-3)
            from unsupported in ComponentDetail.Measured(DetailSchema.RoofSpanUnsupported, Dimension.LengthDim, specification.Span.RoofUnsupportedMm * 1e-3)
        from floor in specification.Span.FloorSpanMm.TraverseM(span =>
            ComponentDetail.Measured(DetailSchema.FloorSpan, Dimension.LengthDim, span * 1e-3)).As()
            select Seq(
                ComponentDetail.Token(DetailSchema.SpanRating, specification.Span.Key),
                ComponentDetail.Token(DetailSchema.BondClass, specification.Bond.Key),
                supported, unsupported) + floor.ToSeq(),
        facedBoard: specification => Fin.Succ(FacerRow(specification.Facer)),
        deckSheet: specification =>
            from depth in ComponentDetail.Measured(DetailSchema.RibDepth, Dimension.LengthDim, specification.Rib.RibDepthMm * 1e-3)
            from pitch in ComponentDetail.Measured(DetailSchema.RibPitch, Dimension.LengthDim, specification.Rib.RibPitchMm * 1e-3)
            select Seq(depth, pitch, ComponentDetail.Token(DetailSchema.DeckForm, specification.Form.Key)),
        foamBoard: specification =>
            from thermal in ComponentDetail.Measured(
                DetailSchema.ThermalResistance, Dimension.Create(0, -1, 3, 0, 1, 0, 0), specification.Foam.RValueSi(thicknessMm.Value))
            from crushing in ComponentDetail.Measured(
                DetailSchema.CompressiveStrength, Dimension.PressureDim, specification.Foam.CompressiveStrengthMpa * 1e6)
            select Seq(ComponentDetail.Token(DetailSchema.FoamClass, specification.Foam.Key), thermal, crushing)
                + FacerRow(specification.Facer),
        membrane: specification => Fin.Succ(
            Seq(ComponentDetail.Token(DetailSchema.MembraneSeam, fastening.Key))
            + specification.Duty.BarrierToken(specification.Vapor).Map(static token => ComponentDetail.Token(DetailSchema.BarrierClass, token)).ToSeq()
            + specification.Flashing.Map(static kind => ComponentDetail.Token(PanelRows.FlashingKind, kind.Key)).ToSeq()));

    static Seq<(PropertyName Name, PropertyValue Value)> FacerRow(Facer facer) =>
        facer == Facer.None ? Empty : Seq(ComponentDetail.Token(DetailSchema.FacerClass, facer.Key));
}

public readonly record struct PanelRow(
    string Designation, PanelKind Kind, double WidthMm, double LengthMm, double ThicknessMm,
    EdgeProfile Edge, PanelOrientation Orientation, PanelFastening Fastener, double FieldMm, double EdgeMm, double EdgeDistMm,
    PanelSpecification Specification) {
    public EvidenceGrade Declared { get; init; } = EvidenceGrade.Catalogue;
    public EvidenceGrade Source => Specification.Deck.Match(Some: static deck => deck.Rib.Source, None: () => Declared);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PanelRows {
    public static readonly PropertyName FlashingKind = PropertyCategory.Materials.Row(nameof(FlashingKind));
}

public static class PanelDetail {
    public static Fin<PropertyBag> Of(
        PositiveMagnitude lengthMm, PositiveMagnitude thicknessMm, EdgeProfile edge, PanelOrientation orientation,
        FastenPattern fastening, PanelSpecification specification, EvidenceGrade source) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from field in ComponentDetail.Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, fastening.FieldSpacingMm * 1e-3)
        from edgeSpacing in ComponentDetail.Measured(DetailSchema.EdgeSpacing, Dimension.LengthDim, fastening.EdgeSpacingMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, lengthMm.Value * 1e-3)
        from payloadRows in specification.DetailRows(thicknessMm, fastening.Fastener)
        from shank in fastening.Fastener.ShankDiameterMm.TraverseM(mm =>
            ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, mm * 1e-3)).As()
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.EdgeProfile, edge.Key),
            ComponentDetail.Sourced(source),
            .. fastening.Fastener.FastenerToken.Map(token => ComponentDetail.Token(DetailSchema.FastenerType, token)).ToSeq(),
            ComponentDetail.Token(DetailSchema.FasteningMethod, fastening.Fastener.Key),
            thickness, field, edgeSpacing,
            length,
            ComponentDetail.Token(DetailSchema.PanelOrientation, orientation.Key),
            .. shank.ToSeq(),
            .. payloadRows,
        ]);
}

// --- [TABLES] --------------------------------------------------------------------------
public static class PanelSeed {
    const double Sheet4FtMm = 1219.2;
    const double Sheet8FtMm = 2438.4;
    const double FieldPitch12InMm = 304.8;
    const double EdgePitch8InMm = 203.2;
    const double EdgePitch6InMm = 152.4;
    const double EdgeInset38InMm = 9.5;
    const double DeckDriftMm = 0.1;

    public static readonly Seq<PanelRow> Roster = Seq(
        new PanelRow("panel.gyp-reg-050-4x8",   PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-reg-038-4x8",   PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 9.5,  EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-x-050-4x8",     PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-4x8",     PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-54x12",   PanelKind.GypsumBoard,   1371.6, 3657.6, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-c-625-4x8",     PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeCFire, Facer.None)),
        new PanelRow("panel.gyp-mr-050-4x8",    PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.MoistureResistant, Facer.None)),
        new PanelRow("panel.gyp-abuse-625-4x8", PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.AbuseResistant, Facer.None)),
        new PanelRow("panel.gyp-025-4x8",       PanelKind.GypsumBoard,   Sheet4FtMm, Sheet8FtMm, 6.4,  EdgeProfile.Square,  PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-ceil-050-4x8",  PanelKind.GypsumCeiling, Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, FieldPitch12InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gypsheath-x-050-4x8",   PanelKind.GypsumSheathing, Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, EdgePitch8InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-x-625-4x8",   PanelKind.GypsumSheathing, Sheet4FtMm, Sheet8FtMm, 15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, EdgePitch8InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-gm-625-4x10", PanelKind.GypsumSheathing, Sheet4FtMm, 3048.0,     15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, EdgePitch8InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.GlassMat, Facer.GlassFiberMat)),
        new PanelRow("panel.gyp-wr-backer-050-3x5", PanelKind.GypsumSheathing, 914.4,  1524.0,         12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, EdgePitch8InMm, EdgePitch8InMm, EdgeInset38InMm, new PanelSpecification.GypsumBoard(CoreType.WaterResistant, Facer.GlassFiberMat)),
        new PanelRow("panel.ply-rated-038-4x8-240",   PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 9.5,  EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-1532-4x8-2416", PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-050-4x8-3216",  PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-1932-4x8-4020", PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 15.1, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-2332-4x8-4824", PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior, WspGrade.Sheathing, SheathingNail.Tend)),
        new PanelRow("panel.ply-str1-1932-4x8",       PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 15.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exterior, WspGrade.StructuralI, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-075-4x8-4824",  PanelKind.PlywoodSheathing, Sheet4FtMm, Sheet8FtMm, 19.0, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior, WspGrade.Sheathing, SheathingNail.Tend)),
        new PanelRow("panel.osb-rated-716-4x8-240",   PanelKind.OsbSheathing, Sheet4FtMm, Sheet8FtMm, 11.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-1532-4x8-2416", PanelKind.OsbSheathing, Sheet4FtMm, Sheet8FtMm, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-050-4x8-3216",  PanelKind.OsbSheathing, Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-2332-4x8-4824", PanelKind.OsbSheathing, Sheet4FtMm, Sheet8FtMm, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Tend)),
        new PanelRow("panel.osb-rated-1532-4x24",     PanelKind.OsbSheathing, Sheet4FtMm, 7315.2,     11.9, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, FieldPitch12InMm, EdgePitch6InMm, EdgeInset38InMm, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.cbu-025-3x5",          PanelKind.CementBoard,        914.4,      1524.0,     6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-3x5",          PanelKind.CementBoard,        914.4,      1524.0,     12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-4x8",          PanelKind.CementBoard,        Sheet4FtMm, Sheet8FtMm, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-625-4x8",          PanelKind.CementBoard,        Sheet4FtMm, Sheet8FtMm, 15.9, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-32x60",        PanelKind.CementBoard,        812.8,      1524.0,     12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-underlay-014-3x5", PanelKind.CementUnderlayment, 914.4,      1524.0,     6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, EdgePitch8InMm, EdgePitch8InMm, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.deck-b-22ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-b-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga20)),
        new PanelRow("panel.deck-a-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.NarrowRibA, Gauges.Ga20)),
        new PanelRow("panel.deck-f-18ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.IntermediateF, Gauges.Ga18)),
        new PanelRow("panel.deck-n-18ga-roof", PanelKind.SteelDeckRoof,  609.6, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.DeepN, Gauges.Ga18)),
        new PanelRow("panel.deck-bform-22ga",  PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckScrew, FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Form, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-15vl-20ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite15, Gauges.Ga20)),
        new PanelRow("panel.deck-2vli-18ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 50.8, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite2Vli, Gauges.Ga18)),
        new PanelRow("panel.deck-3vli-16ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  FieldPitch12InMm, FieldPitch12InMm, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite3Vli, Gauges.Ga16)),
        new PanelRow("panel.eps-1in-4x8",      PanelKind.RigidBoardEps,  Sheet4FtMm, Sheet8FtMm, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-2in-4x8",      PanelKind.RigidBoardEps,  Sheet4FtMm, Sheet8FtMm, 50.8,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-4in-4x8",      PanelKind.RigidBoardEps,  Sheet4FtMm, Sheet8FtMm, 101.6, EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.Adhesive,      406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.xps-1in-2x8",      PanelKind.RigidBoardXps,  609.6,      Sheet8FtMm, 25.4,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.xps-2in-2x8",      PanelKind.RigidBoardXps,  609.6,      Sheet8FtMm, 50.8,  EdgeProfile.TongueGroove, PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.polyiso-1in-4x8",  PanelKind.RigidBoardPoly, Sheet4FtMm, Sheet8FtMm, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.GlassFiberMat)),
        new PanelRow("panel.polyiso-2in-foil", PanelKind.RigidBoardPoly, Sheet4FtMm, Sheet8FtMm, 50.8,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.Foil)),
        new PanelRow("panel.polyiso-3in-4x8",  PanelKind.RigidBoardPoly, Sheet4FtMm, Sheet8FtMm, 76.2,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.CoatedGlass)),
        new PanelRow("panel.epdm-060-roll", PanelKind.MembraneEpdm, 3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.SeamAdhesive, FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.Roofing, None, None)),
        new PanelRow("panel.pvc-060-roll",  PanelKind.MembranePvc,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.HeatWeld,     FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.Roofing, None, None)),
        new PanelRow("panel.tpo-060-roll",  PanelKind.MembraneTpo,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.HeatWeld,     FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.Roofing, None, None)),
        new PanelRow("panel.wrap-ab-roll",      PanelKind.AirBarrier,       3048.0, 30480.0, 0.2,  EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.SeamAdhesive, FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.AirBarrier, None, None)) { Declared = EvidenceGrade.User },
        new PanelRow("panel.pe-vr-6mil-roll",   PanelKind.VapourRetarder,   3048.0, 30480.0, 0.15, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.SeamAdhesive, FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.VapourRetarder, Some(VaporClass.ClassI), None)) { Declared = EvidenceGrade.User },
        new PanelRow("panel.sbs-bg-60mil-roll", PanelKind.Waterproofing,    914.4,  20320.0, 1.5,  EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.Adhesive,     FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.Waterproofing, None, None)),
        new PanelRow("panel.flash-sa-9in-roll", PanelKind.FlashingMembrane, 228.6,  22860.0, 0.64, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.Adhesive,     FieldPitch12InMm, EdgePitch6InMm, 0.0, new PanelSpecification.Membrane(MembraneDuty.Flashing, None, Some(FlashingKind.SelfAdheredStrip))) { Declared = EvidenceGrade.User });

    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, PanelRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static readonly SeedLaw<PanelRow> Law = SeedLaw<PanelRow>.Of(
        family: ComponentFamily.Panel,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: Substance,
        source: static r => r.Source,
        standard: static r => new ComponentStandard(r.Kind.Authority.Region, StandardJointThicknessMm: 0.0, r.Kind.Authority),
        detail: Some<Func<PanelRow, SectionProfile, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.Specification.FacingMaterial(r.Kind).IfNone(Substance(r)),
        ifc: static r => IfcBinding.Of(r.Kind.IfcEntity, r.Kind.IfcPredefinedType));

    static Validation<Error, Unit> Coherence(PanelRow r) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(r.Kind.Admits(r.Specification),
                new KernelFault.InvalidValue(nameof(r.Specification), "a specification admitted by the panel kind")),
            AdmissionSlots.Gate(r.Specification.Coherent(),
                new KernelFault.InvalidValue(nameof(r.Specification), "a coherent panel payload")),
            AdmissionSlots.Gate(
                double.IsFinite(r.WidthMm) && r.WidthMm > 0.0 && double.IsFinite(r.LengthMm) && r.LengthMm > 0.0
                && double.IsFinite(r.ThicknessMm) && r.ThicknessMm > 0.0,
                new KernelFault.InvalidValue(nameof(PanelRow), "positive finite width, length, and thickness")),
            FastenPattern.Of(r.FieldMm, r.EdgeMm, r.EdgeDistMm, r.Fastener).ToValidation().Map(static _ => unit),
            DeckDrift(r)));

    static Validation<Error, Unit> DeckDrift(PanelRow r) =>
        Tolerance.Of(ToleranceLane.Match, DeckDriftMm).ToValidation()
            .Bind(band => AdmissionSlots.Gate(r.Specification.Deck.ForAll(deck =>
                    Math.Abs(r.ThicknessMm - deck.Rib.RibDepthMm) <= band.Value
                    && Math.Abs(r.WidthMm - deck.Rib.CoverageMm) <= band.Value),
                new KernelFault.InvalidValue(nameof(r.ThicknessMm), "deck gauge thickness and rib coverage")));

    static MaterialId Substance(PanelRow r) =>
        r.Specification.Deck.Map(static deck => deck.Gauge.Substance).IfNone(r.Kind.Substance);

    static Fin<SectionProfile> Profile(PanelRow r) =>
        r.Specification.Deck.Match(
            Some: deck => SectionProfile.Corrugated.Of(
                coverWidthMm: deck.Rib.CoverageMm, ribDepthMm: deck.Rib.RibDepthMm, ribPitchMm: deck.Rib.RibPitchMm,
                gaugeMm: deck.Gauge.BaseThicknessMm, topFlatMm: deck.Rib.TopFlatMm, bottomFlatMm: deck.Rib.BottomFlatMm),
            None: () =>
                from thickness in FactoryBridge.Accept<PositiveMagnitude>(candidate: r.ThicknessMm)
                from plies in r.Specification.Layup(r.Kind, thickness)
                    .ToFin(new KernelFault.InvalidValue(nameof(r.ThicknessMm), "at least the built facing thickness"))
                from layered in SectionProfile.Layered.Of(plies, overallMm: r.ThicknessMm, widthMm: r.WidthMm)
                select layered);

    static Fin<PropertyBag> Detail(PanelRow r, SectionProfile profile) =>
        from length in FactoryBridge.Accept<PositiveMagnitude>(candidate: r.LengthMm)
        from thickness in FactoryBridge.Accept<PositiveMagnitude>(candidate: r.ThicknessMm)
        from fastening in FastenPattern.Of(r.FieldMm, r.EdgeMm, r.EdgeDistMm, r.Fastener)
        from bag in PanelDetail.Of(length, thickness, r.Edge, r.Orientation, fastening, r.Specification, r.Source)
        select bag;

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        from row in SeedJoin.Resolve(Table, component.Designation)
        from capacity in row.Specification.Switch(
            deckSheet: deck =>
                from solved in section.ToFin(new ComponentFault.SectionUnavailable(component.Designation))
                from design in SteelDesign.Capacity(component.Profile, deck.Rib.Grade, solved, placement)
                from lifted in SectionCapacity.Lift(new CapacityLift.DeckSheet(component.Designation, deck.Gauge, deck.Rib, design))
                select lifted,
            woodPanel: wood =>
                from nominal in LateralShear.Nominal(
                    wood.Grade, row.ThicknessMm / LateralShear.InchToMm, wood.Nail, placement.Assembly,
                    row.EdgeMm / LateralShear.InchToMm, placement.FramingWidthMm / LateralShear.InchToMm,
                    placement.DiaphragmCase)
                from design in placement.Hazard.Design(nominal, placement.Basis.Format)
                from lifted in SectionCapacity.Lift(new CapacityLift.LateralPanel(component.Designation, design, placement.Hazard))
                select lifted,
            gypsumBoard: _ => Unpriced(component.Designation), facedBoard: _ => Unpriced(component.Designation),
            foamBoard: _ => Unpriced(component.Designation), membrane: _ => Unpriced(component.Designation))
        select capacity;

    static Fin<SectionCapacity> Unpriced(ComponentId subject) =>
        new ComponentFault.CapacityUnavailable(subject);
}
```

## [03]-[RESEARCH]

(none)
