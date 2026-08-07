# [MATERIALS_PANEL]

THE PANEL SEED PAGE owns the `ComponentFamily.Panel` policy row, the product vocabularies, frozen standards rows, `FastenPattern`, the closed `PanelSpecification` payload, and the single `PanelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` fold. Board geometry is `SectionProfile.Layered` over the shared bounded `PlyRole`; deck geometry is `SectionProfile.Corrugated`, solved by the canonical `SectionSolver` arm. `Sectioned` follows structural kind, every row carries its kind-owned `IfcBinding`, and every product detail measurement remains on the `Fin` rail through catalogue construction.

## [01]-[INDEX]

- [02]-[PANEL_FAMILY]: the `PanelKind` board-type vocabulary; the edge, orientation, fastening, core, bond, foam, facer, and deck policies; the frozen deck/span tables; `FastenPattern`; the payload-timed `PanelSpecification` union; the `PanelRow` roster; `PanelDetail`; and `PanelSeed.Rows`.

## [02]-[PANEL_FAMILY]

- Owner: `PanelKind` carries the board-type axis, IFC leaf, structural flag, substance, authority, and `[UseDelegateFromConstructor]` layup; the shared `PlyRole` bounds every layer role across panel, glazing, timber, and masonry. `EdgeProfile`, `PanelOrientation`, `PanelFastening`, `DeckForm`, `CoreType`, `BondClass`, `FoamType`, and `Facer` carry product policy; `DeckProfiles`, `SpanRatings`, `FastenPattern`, `PanelSpecification`, `PanelDetail`, and `PanelSeed` carry printed data, admission, payload timing, details, and construction.
- Cases: kind {fifteen rows over the tri-entity IFC spread; a fiberboard or magnesium-oxide board is one new row reusing `IfcCovering`/CLADDING and an existing layup} · edge {square/tapered/beveled/rounded/tongue-groove/shiplap/side-lap-interlock/lapped-seam} · orientation {strength-axis-perpendicular/parallel/unidirectional} · profile {`Layered` every covering/membrane board — the layup delegate's `Ply` stack; `Corrugated` the two steel-deck kinds — the `DeckProfiles` row + `GaugeRow` fill the six named dims}.
- Entry: `PanelSeed.Rows(Context)` traverses each row through dimension and fastening admission, resolves `PanelSpecification.DeckSheet` to `SectionProfile.Corrugated` and every other specification to `SectionProfile.Layered`, builds the detail bag from union projections, and seals the component. `PanelSpecification` keeps gypsum core/facer, wood span/bond, faced-board facer, deck form/profile/gauge, foam type/facer, and membrane absence in disjoint cases; no row carries irrelevant `None` policy values.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `PropertyBag`, and the seam `DetailSchema`/`PropertyName`/`PropertyValue`/`Dimension` currencies `PanelDetail` composes; every `DetailSchema.Product` row a panel stamps is Element-declared at `property#DETAIL_SCHEMA`, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` layup columns, `[ComplexValueObject]` + generated `ValidateFactoryArguments`/`Validate`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`), BCL inbox (`ImmutableArray`); NO sheet-goods external producer — the roster is `AUTHORED` under `SEED_ROW_LAW` (VividOrange owns structural-MEMBER catalogues and EN grades, not gypsum/sheathing/deck/insulation rosters); the VividOrange solve surface moves with the section machinery to `component#SECTION_SOLVER`; the deck base metal reuses the `connector#CONNECTOR_FAMILY` `GaugeRow`/`Gauges` table (one cold-formed gauge vocabulary — `Ga22`..`Ga10` named statics, PUBLISHED AISI base/design thickness and gauge-band yield; the deck rows reference `Ga22`/`Ga20`/`Ga18`/`Ga16` symbolically).
- Growth: a new board is one `PanelRow`; a new kind one `PanelKind` row binding its IFC leaf + layup delegate; a new edge/orientation/core/bond/foam/facer band one vocabulary row; a new deck profile one `DeckProfileRow`; a new span rating one `SpanRow` — ZERO type edits per `[DIFF_OF_NEXT_THING]` ("Panel (new board) — one `ComponentRow` in `PanelSeed.Rows` — thickness, width, IFC leaf, fastening all row values").
- Boundary: this page emits DATA — profiles, vocabulary rows, bags, and the seed fold; the section INTEGRAL is `component#SECTION_SOLVER`'s `corrugated` arm (one solver, compiler-forced per profile arm), the twenty-column `ComputedSection` lift is `SectionSolver.Admit`, and the per-coverage rib scaling reads `CoverWidthMm/RibPitchMm` off the `Corrugated` dims inside `Forms.ThinFold` — panel keeps only the `DeckProfiles`/`Gauges` DATA those dims read; a deck row's solved `ComputedSection` is PRICED, not merely stored — the `steel#STEEL_FAMILY` AISI overload reads it at the `connector#CONNECTOR_FAMILY` `GaugeRow`'s own SS Grade 33/50 yield and the receipt lifts into `capacity#SECTION_CAPACITY` through `CapacityReceipt.DeckSheet`, so `GaugeRow.AxialSectionCapacityKnPerMm` is a CONSUMED datum rather than a one-directional seam declaration and a deck's flexural/shear verdict rides the one `Check(demand)` rail; the board substance physics (`gypsum.board`/`cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps`/`xps`/`pir`/membranes) read ONCE from the property library by `SubstanceId`, never re-keyed here; `SubstanceId`/`AppearanceId` stay INDEPENDENT slots (a foil-faced polyiso keeps its foam substance while its appearance names the facer; a deck's substance is its gauge steel); the layup is the typed `Seq<Ply>` the seam `CompositionAuthor.LayerSet` coerces into `IfcMaterialLayerSet` (a deck is a `ProfileSet` — a ribbed sheet profiles, never layers); `IfcBinding` strings stay neutral (the generated `Rasm.Bim` roster validates composition-time and egress-time; the `IfcCoveringTypeEnum` has NO SHEATHING member so lining and sheathing are both CLADDING, the `IfcSlabTypeEnum` has NO COMPOSITE/DECK member so a composite floor deck is `IfcSlab`/FLOOR); the product bag rides the Type `Object` via `Assign.PropertyDefinition` and round-trips through the GENERAL Bim `Object`/property fold (a panel is an `IfcBuiltElement`, never a realizing element).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary — the designation-keyed row join the capacity producer reads
using System.Collections.Immutable;  // ImmutableArray (the frozen printed-data row tables)
using LanguageExt;                   // Fin, Option, Seq, Traverse
using Rasm.Numerics;                  // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                   // Op, Context, AcceptValidated
using Rasm.Element.Composition;                  // MaterialId, PropertyBag, DetailSchema, Dimension, PropertyName, PropertyValue (the seam bag currencies PanelDetail composes)
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using Thinktecture;                  // [SmartEnum]/[ComplexValueObject]/[UseDelegateFromConstructor]/[KeyMemberEqualityComparer]/[KeyMemberComparer]
using static LanguageExt.Prelude;

// Every family seed declares in the ONE Rasm.Materials.Component namespace (component#COMPONENT_OWNER); the owner
// folds PanelSeed.Rows through the ComponentFamily.Panel policy row, never by name. The deck base-metal gauge reuses
// the connector family's Gauges row table — never a parallel deck gauge enum.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The board-type axis grown by DATA: each row carries the GeometryGym-verified IFC leaf the projector stamps
// (tri-entity spread — IfcCovering CLADDING/CEILING/FLOORING/INSULATION/ROOFING, IfcPlate SHEET, IfcSlab FLOOR;
// IfcDeck does NOT exist, so a roof/form deck is IfcPlate/SHEET and a composite floor deck IfcSlab/FLOOR), the
// The substance MaterialId key, the kind's ComponentAuthority standards body, and the kind's layup delegate
// (POLICY row — the face/core stack derivation rides the vocabulary, never an 8-arm ternary chain in the seed).
// Deck kinds bind the empty layup: their geometry is the Corrugated profile, not plies.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelKind {
    public static readonly PanelKind GypsumBoard        = new("gypsum-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.PaperFace, "paper.face", "gypsum.board", t),     admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer == Facer.None);
    public static readonly PanelKind GypsumCeiling      = new("gypsum-ceiling",      ifcEntity: "IfcCovering", ifcPredefinedType: "CEILING",    substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.PaperFace, "paper.face", "gypsum.board", t),     admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer == Facer.None);
    public static readonly PanelKind GypsumSheathing    = new("gypsum-sheathing",    ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMatFacer, "glass.mat", "gypsum.board", t), admits: static s => s is PanelSpecification.GypsumBoard { Facer: var facer } && facer != Facer.None);
    public static readonly PanelKind PlywoodSheathing   = new("plywood-sheathing",   ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.plywood",   authority: ComponentAuthority.Apa,  layup: static (f, t) => Mono(PlyRole.VeneerPly, "wood.plywood", t),                        admits: static s => s is PanelSpecification.WoodPanel);
    public static readonly PanelKind OsbSheathing       = new("osb-sheathing",       ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.osb",       authority: ComponentAuthority.Apa,  layup: static (f, t) => Mono(PlyRole.StrandLayer, "wood.osb", t),                          admits: static s => s is PanelSpecification.WoodPanel);
    public static readonly PanelKind CementBoard        = new("cement-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMeshScrim, "glass.scrim", "cement.board", t), admits: static s => s is PanelSpecification.FacedBoard);
    public static readonly PanelKind CementUnderlayment = new("cement-underlayment", ifcEntity: "IfcCovering", ifcPredefinedType: "FLOORING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMeshScrim, "glass.scrim", "cement.board", t), admits: static s => s is PanelSpecification.FacedBoard);
    public static readonly PanelKind SteelDeckRoof      = new("steel-deck-roof",     ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi,  layup: static (f, t) => Some(Seq<Ply>()),                                                  admits: static s => s is PanelSpecification.DeckSheet { Form.FloorDeck: false });
    public static readonly PanelKind SteelDeckFloor     = new("steel-deck-floor",    ifcEntity: "IfcSlab",     ifcPredefinedType: "FLOOR",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi,  layup: static (f, t) => Some(Seq<Ply>()),                                                  admits: static s => s is PanelSpecification.DeckSheet { Form.FloorDeck: true });
    public static readonly PanelKind RigidBoardEps      = new("rigid-board-eps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.eps", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.eps", t),                                 admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Eps);
    public static readonly PanelKind RigidBoardXps      = new("rigid-board-xps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.xps", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.xps", t),                                 admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Xps);
    public static readonly PanelKind RigidBoardPoly     = new("rigid-board-poly",    ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.pir", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.pir", t),                                 admits: static s => s is PanelSpecification.FoamBoard { Foam: var foam } && foam == FoamType.Polyiso);
    public static readonly PanelKind MembraneEpdm       = new("membrane-epdm",       ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.epdm",  authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.epdm", t),                     admits: static s => s is PanelSpecification.Membrane);
    public static readonly PanelKind MembranePvc        = new("membrane-pvc",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.pvc",   authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.pvc", t),                      admits: static s => s is PanelSpecification.Membrane);
    public static readonly PanelKind MembraneTpo        = new("membrane-tpo",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.tpo",   authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.tpo", t),                      admits: static s => s is PanelSpecification.Membrane);
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public string SubstanceId { get; }
    public ComponentAuthority Authority { get; }   // the kind's OWN standards body — ASTM boards/foams/membranes, APA wood panels, SDI deck; never one blended authority
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    // The shared bounded PlyRole keeps layer policy typed through section and appearance projection. A layup answers
    // ABSENCE where the board's own thickness cannot carry the stack its kind implies.
    [UseDelegateFromConstructor]
    public partial Option<Seq<Ply>> Layup(Facer facer, PositiveMagnitude thickness);

    // Which product payload this kind admits — a ROW COLUMN beside the layup it pairs with, so a new kind declares
    // its own admissible payload where it declares everything else about itself. The previous six-arm switch over
    // the specification family named PanelKind identities inside its arms, which meant the specification union knew
    // the whole kind roster by name and a new kind had to be added in two places that could disagree.
    [UseDelegateFromConstructor]
    public partial bool Admits(PanelSpecification specification);

    // The facing thicknesses are AUTHORED policy — Provenance.Authored, not published product data: no admitted
    // producer prints a paper, glass-mat, scrim, or foil facing thickness, and the layup needs a real number for the
    // ply stack to sum to the board. They are named constants so the value has ONE site and a later published read
    // replaces it once, rather than a bare literal recurring inside two layup builders.
    const double BoardFacingMm = 0.5;   // AUTHORED: paper / glass-mat / mesh-scrim board facing
    const double FoamFacingMm = 0.2;    // AUTHORED: foil / glass-fibre-mat rigid-board facing

    // Two thin faces over a core the remainder. The core is what the board has LEFT after its facings, so a board
    // thinner than its own facings has no core at all — a DEGENERATE build, and the layup says so rather than
    // clamping the remainder up to a facing thickness and returning a stack whose plies out-sum the board they
    // describe. That clamp broke the laminate-sum guarantee Component.Of exists to hold, silently, for every board
    // under 1.5 mm.
    static Option<Seq<Ply>> FaceCoreFace(PlyRole face, string faceId, string coreId, PositiveMagnitude t) =>
        Some(t.Value - 2.0 * BoardFacingMm)
            .Filter(static core => core > 0.0)
            .Map(core => Seq(
                new Ply(MaterialId.Of(faceId), PositiveMagnitude.Create(BoardFacingMm), face),
                new Ply(MaterialId.Of(coreId), PositiveMagnitude.Create(core), face == PlyRole.GlassMeshScrim ? PlyRole.CementAggregateCore : PlyRole.GypsumCore),
                new Ply(MaterialId.Of(faceId), PositiveMagnitude.Create(BoardFacingMm), face)));

    // One homogeneous ply the host subdivides (veneer plies, strand mat, membrane sheet) — always well-formed.
    static Option<Seq<Ply>> Mono(PlyRole role, string substanceId, PositiveMagnitude t) =>
        Some(Seq(new Ply(MaterialId.Of(substanceId), t, role)));

    // Facer/foam-core/facer rigid board. The facer's own Faces COUNT is the geometry, so every arm is reachable by
    // data: 0 is the bare core, 1 a single-faced board, 2 the mirrored layup. The core is again the remainder and
    // again refuses rather than fabricates.
    static Option<Seq<Ply>> FacedFoam(Facer facer, string foamId, PositiveMagnitude t) =>
        facer.Faces is 0
            ? Some(Seq(new Ply(MaterialId.Of(foamId), t, PlyRole.FoamCore)))
            : Some(t.Value - FoamFacingMm * facer.Faces)
                .Filter(static core => core > 0.0)
                .Map(core => {
                    Ply skin = new(MaterialId.Of($"facer.{facer.Key}"), PositiveMagnitude.Create(FoamFacingMm), facer.Role);
                    Ply centre = new(MaterialId.Of(foamId), PositiveMagnitude.Create(core), PlyRole.FoamCore);
                    return facer.Faces >= 2 ? Seq(skin, centre, skin) : Seq(skin, centre);
                });
}

// The board-edge axis the coursing reads to butt or lap adjacent boards: Lapped flags an interlocking edge the
// spec sheathing stage overlaps; GapMm is the board-to-board coursing gap (0 for a nesting edge, the ~3 mm
// control gap for a butt edge).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeProfile {
    public static readonly EdgeProfile Square           = new("square",             lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile Tapered          = new("tapered",            lapped: false, gapMm: 3.0);   // ASTM C1396 recessed long edge for the joint-compound feather
    public static readonly EdgeProfile Beveled          = new("beveled",            lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile Rounded          = new("rounded",            lapped: false, gapMm: 3.0);
    public static readonly EdgeProfile TongueGroove     = new("tongue-groove",      lapped: true,  gapMm: 0.0);
    public static readonly EdgeProfile Shiplap          = new("shiplap",            lapped: true,  gapMm: 0.0);
    public static readonly EdgeProfile SideLapInterlock = new("side-lap-interlock", lapped: true,  gapMm: 0.0);   // SDI deck nestable side-lap seam
    public static readonly EdgeProfile LappedSeam       = new("lapped-seam",        lapped: true,  gapMm: 0.0);
    public bool Lapped { get; }
    public double GapMm { get; }
}

// The strength-axis run direction the spec sheathing stage orients the run by; AcrossFrame flags the
// staggered-joint perpendicular lay.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelOrientation {
    public static readonly PanelOrientation StrengthAxisPerpendicular = new("strength-axis-perpendicular",    acrossFrame: true);
    public static readonly PanelOrientation StrengthAxisParallel      = new("strength-axis-parallel-to-span", acrossFrame: false);
    public static readonly PanelOrientation Unidirectional            = new("unidirectional",                 acrossFrame: false);
    public bool AcrossFrame { get; }
}

// ASTM C1396 gypsum core formulation; FireRated flags a Type-X/C core the fire-rating seam reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoreType {
    public static readonly CoreType Regular           = new("regular",            fireRated: false);
    public static readonly CoreType TypeXFire         = new("type-x-fire",        fireRated: true);
    public static readonly CoreType TypeCFire         = new("type-c-fire",        fireRated: true);
    public static readonly CoreType MoistureResistant = new("moisture-resistant", fireRated: false);
    public static readonly CoreType AbuseResistant    = new("abuse-resistant",    fireRated: false);
    public static readonly CoreType GlassMat          = new("glass-mat",          fireRated: false);   // ASTM C1177 glass-mat exterior sheathing
    public static readonly CoreType WaterResistant    = new("water-resistant",    fireRated: false);
    public bool FireRated { get; }
}

// APA / EN 13986 wood-structural-panel exposure/bond durability. The all-false "none" row is DELETED: a wood panel
// has a bond class, and a board that is not a wood panel carries no BondClass at all — the specification union
// already says which is which, so the sentinel row modelled a state the payload timing makes unreachable.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondClass {
    public static readonly BondClass Exposure1 = new("exposure-1", exterior: false);
    public static readonly BondClass Exterior  = new("exterior",   exterior: true);
    public bool Exterior { get; }
}

// Rigid-board foam chemistry (ASTM C578 EPS/XPS, C1289 polyiso): the design R-per-inch POLICY the seed-computed
// thermal receipt reads, and the published compressive strength. PUBLISHED design values; polyiso is the aged LTTR.
// Both scalars convert to SI at the ROW, so the imperial constants live in one place each rather than inline at the
// call sites that consume them, and the all-zero "none" row is DELETED — a foam board has a chemistry.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FoamType {
    const double InchToMm = 25.4;
    const double RValueIpToSi = 0.17611;   // (h·ft²·°F/Btu) -> (m²·K/W)
    const double PsiToMpa = 0.00689476;
    public static readonly FoamType Eps     = new("eps",     rValuePerInch: 3.85, compressiveStrengthPsi: 15.0);
    public static readonly FoamType Xps     = new("xps",     rValuePerInch: 5.0,  compressiveStrengthPsi: 25.0);
    public static readonly FoamType Polyiso = new("polyiso", rValuePerInch: 5.7,  compressiveStrengthPsi: 20.0);
    public double RValuePerInch { get; }
    public double CompressiveStrengthPsi { get; }
    // DEFINED: the SI thermal resistance (m²·K/W) the seed mints into the product bag.
    public double RValueSi(double thicknessMm) => RValuePerInch * (thicknessMm / InchToMm) * RValueIpToSi;
    // DEFINED: the SI compressive strength (MPa) — the sandwich physics reads it as the core's own crushing bound,
    // and it crosses the product bag as the load-bearing datum a roof-assembly check needs before it prices foot
    // traffic or a ballast course over the board.
    public double CompressiveStrengthMpa => CompressiveStrengthPsi * PsiToMpa;
}

// The rigid-board/sheathing facer form. Faces is the COUNT of faced sides the layup mirrors and Role the ply role
// the facer IS — both row columns, so a new facer names its own role instead of being compared against a sibling,
// and a genuinely single-faced product lands as a row rather than as an arm nothing can select.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Facer {
    public static readonly Facer None          = new("none",            faces: 0, role: PlyRole.FoamCore);
    public static readonly Facer Foil          = new("foil",            faces: 2, role: PlyRole.FoilFacer);
    public static readonly Facer GlassFiberMat = new("glass-fiber-mat", faces: 2, role: PlyRole.GlassFiberMatFacer);
    public static readonly Facer CoatedGlass   = new("coated-glass",    faces: 2, role: PlyRole.CoatedGlassFacer);
    public int Faces { get; }
    public PlyRole Role { get; }
}

// The board-fastening POLICY axis: each row carries the panel-specific policy (Welded flags a puddle/heat weld the
// host renders as a seam, AppearanceId the fastener render material the spec sheathing stage tags each placement
// with) BESIDE the canonical fastener#FASTENER_FAMILY FastenerKind the mechanical rows ARE — a drywall or deck screw
// is FastenerKind.Screw, a structural or roofing nail FastenerKind.Nail, so the wire token stamps from
// Kind.IfcPredefinedType exactly as connector#CONNECTOR_FAMILY ConnectorInstall does and the two vocabularies read as
// policy-versus-kind rather than as sibling spellings of one concept. The prior nine-row parallel FastenerType in this
// same Rasm.Materials.Component namespace was the duplicated token roster the connector page's own settled law
// forbids. A welded or bonded row carries NO FastenerKind — an arc-spot weld, a heat weld, and an adhesive bead are
// not mechanical fasteners — so FastenerToken falls back to the row's own canonical key rather than fabricating an
// IfcMechanicalFastener predefined value the schema does not carry.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelFastening {
    public static readonly PanelFastening DrywallScrew   = new("drywall-screw",   kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(3.5),   appearanceId: "metal.steel");   // ASTM C1002 bugle-head #6
    public static readonly PanelFastening StructuralNail = new("structural-nail", kind: Some(FastenerKind.Nail),  welded: false, shankMm: Some(3.33),  appearanceId: "metal.steel");   // 8d common, the APA edge/field schedule
    public static readonly PanelFastening StructuralNail10d = new("structural-nail-10d", kind: Some(FastenerKind.Nail), welded: false, shankMm: Some(3.76), appearanceId: "metal.steel");
    public static readonly PanelFastening RoofingNail    = new("roofing-nail",    kind: Some(FastenerKind.Nail),  welded: false, shankMm: Some(3.05),  appearanceId: "metal.steel");
    public static readonly PanelFastening DeckWeld       = new("deck-weld",       kind: None,                     welded: true,  shankMm: None,        appearanceId: "metal.steel");   // SDI arc-spot puddle weld
    public static readonly PanelFastening DeckScrew      = new("deck-screw",      kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(4.83),  appearanceId: "metal.steel");   // SDI self-drilling #12
    public static readonly PanelFastening PlateAndScrew  = new("plate-and-screw", kind: Some(FastenerKind.Screw), welded: false, shankMm: Some(4.83),  appearanceId: "metal.steel");
    public static readonly PanelFastening Adhesive       = new("adhesive",        kind: None,                     welded: false, shankMm: None,        appearanceId: "adhesive.bead");
    public static readonly PanelFastening HeatWeld       = new("heat-weld",       kind: None,                     welded: true,  shankMm: None,        appearanceId: "membrane.seam");
    public static readonly PanelFastening SeamAdhesive   = new("seam-adhesive",   kind: None,                     welded: false, shankMm: None,        appearanceId: "adhesive.bead");
    public Option<FastenerKind> Kind { get; }
    public bool Welded { get; }
    // The published shank diameter — the SIZE datum that makes a nailing schedule a design input rather than a
    // drawing note. A shear-wall unit shear is keyed on panel grade, panel thickness, NAIL SIZE, and edge spacing;
    // with the size living only in a comment, three of those four keys were expressible and the fourth was not, so
    // no lateral capacity could be formed from a row at all. A welded or bonded fastening carries no shank.
    public Option<double> ShankDiameterMm { get; }
    public string AppearanceId { get; }

    // The IFC predefined value where a canonical mechanical kind exists — and NOTHING where one does not. The
    // previous projection fell back to this row's own key, which put "heat-weld" and "adhesive" into a field whose
    // vocabulary is IfcMechanicalFastenerTypeEnum: a Bim reader recovering that token reads a mechanical fastener
    // type the schema never defined. A weld and a bond are not mechanical fasteners, and absence says so.
    public Option<string> FastenerToken => Kind.Map(static kind => kind.IfcPredefinedType);
}

// The SDI structural form; FloorDeck flags the composite deck modeled as a slab (the IfcSlab/FLOOR row).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeckForm {
    public static readonly DeckForm Form      = new("form",      floorDeck: false);   // ANSI/SDI stay-in-place form deck
    public static readonly DeckForm Composite = new("composite", floorDeck: true);    // ANSI/SDI C-2017 embossed-rib floor deck
    public static readonly DeckForm Roof      = new("roof",      floorDeck: false);   // ANSI/SDI RD-2017 roof deck
    public bool FloorDeck { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// Deck rib geometry as a FROZEN printed-data row. The six columns are the profile as its publisher prints it: depth,
// pitch, coverage, both FLANGE FLATS, and the STEEL GRADE the profile is rolled in. Both flats are stored because
// both are printed — the top flat was previously derived from an authored pitch FRACTION invented to stand in for a
// printed dimension, which made a published number look derived and put the fiction one column away from the data.
// The grade is a row column for the same reason it is a published fact: the roof-deck series is rolled across the SS
// grade band while the composite series is capped at Grade 50, a limit the standard sets so the shear-transfer
// embossments do not crack, and reading that off a yield threshold rather than the row inverted the dependency.
// Provenance is per-ROW because the two series do not share it: SDI standardizes only the roof-deck profiles, so
// their flats are PUBLISHED, while every composite profile is proprietary and its flats reach one publisher only.
public readonly record struct DeckProfileRow(
    string Key, double RibDepthMm, double RibPitchMm, double CoverageMm,
    double TopFlatMm, double BottomFlatMm, SteelGrade Grade, Provenance Source);

// The roof-deck series is the SDI standard profile set — narrow, intermediate, wide, and deep rib — whose flats
// satisfy the standard's own identity that the top flat and the rib opening sum to the pitch. The composite series is
// proprietary geometry: a manufacturer names it, no standard standardizes it, and the roster says so on the row.
public static class DeckProfiles {
    public static readonly DeckProfileRow NarrowRibA    = new("narrow-rib-a",   38.1, 152.4, 914.4, 127.0,  9.5,  SteelGrade.A653Gr33, Provenance.Published);   // SDI NR: 5in top, 3/8in bottom
    public static readonly DeckProfileRow IntermediateF = new("intermediate-f", 38.1, 152.4, 914.4, 108.0, 12.7,  SteelGrade.A653Gr33, Provenance.Published);   // SDI IR: 4-1/4in top, 1/2in bottom
    public static readonly DeckProfileRow WideRibB      = new("wide-rib-b",     38.1, 152.4, 914.4,  88.9, 39.7,  SteelGrade.A653Gr50, Provenance.Published);   // SDI WR: 3-1/2in top, 1-9/16in bottom
    public static readonly DeckProfileRow DeepN         = new("deep-n",         76.2, 203.2, 609.6, 133.4, 38.1,  SteelGrade.A653Gr50, Provenance.Published);   // SDI 3DR: 5-1/4in top, 1-1/2in bottom
    public static readonly DeckProfileRow Composite15   = new("composite-15",   38.1, 152.4, 914.4,  88.9, 44.5,  SteelGrade.A653Gr50, Provenance.Authored);
    public static readonly DeckProfileRow Composite2Vli = new("composite-2vli", 50.8, 304.8, 914.4, 127.0, 127.0, SteelGrade.A653Gr50, Provenance.Authored);
    public static readonly DeckProfileRow Composite3Vli = new("composite-3vli", 76.2, 304.8, 914.4, 120.7, 120.7, SteelGrade.A653Gr50, Provenance.Authored);
    public static readonly ImmutableArray<DeckProfileRow> Rows = [NarrowRibA, IntermediateF, WideRibB, DeepN, Composite15, Composite2Vli, Composite3Vli];
}

// APA PRP-108 span rating as a FROZEN printed-data row. Roof spacing is published TWICE — once with panel edge
// support and once without — and the two differ by up to a third on the same rating, so a single roof column would
// have credited every unblocked roof with the blocked spacing. Floor spacing is an Option because a roof-only rating
// genuinely has none, where the previous zero asserted a floor span of nothing.
public readonly record struct SpanRow(string Key, int RoofEdgeSupportedIn, int RoofUnsupportedIn, Option<int> FloorSpanIn) {
    public double RoofEdgeSupportedMm => RoofEdgeSupportedIn * 25.4;   // DEFINED
    public double RoofUnsupportedMm => RoofUnsupportedIn * 25.4;       // DEFINED
    public Option<double> FloorSpanMm => FloorSpanIn.Map(static span => span * 25.4);
}

// The APA Rated Sheathing series and the Sturd-I-Floor series in ONE roster: a Sturd-I-Floor rating IS a span rating
// whose single published number is its joist spacing, so it is a row here rather than a parallel vocabulary. The
// nominal "20 oc" and "40/20" ratings both mean 19.2 in of actual spacing, which the standard footnotes and this
// roster carries as the number rather than the label.
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
// SDPWS wood-structural-panel NOMINAL unit shear as frozen printed data. ONE nominal column per configuration: the
// current edition publishes a single vn applicable to both wind and seismic, and the hazard distinction is the
// capacity#SECTION_CAPACITY LateralHazard reduction applied downstream. Earlier editions tabulated a wind column
// beside a seismic one, so seeding a second column here would fork the table this transcribes and re-import a
// distinction the standard deleted. Values are the PUBLISHED plf converted once — a pre-rounded kN/m column would
// hide which digits the standard printed behind a conversion the reader has to invert.
// Every cell is Option because coverage is genuinely partial: a configuration the standard tabulates but this corpus
// could not corroborate carries absence rather than a number resting on one reading, and the resolution then reports
// not-applicable instead of pricing an assembly off an unverified cell.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WspGrade {
    public static readonly WspGrade StructuralI = new("structural-i");
    public static readonly WspGrade Sheathing   = new("sheathing");     // the Sheathing and Single-Floor block
}

// The sheathing nail as its published product row: the reference fastener is a carbon-steel smooth-shank COMMON nail
// and BearingLengthMm is lm, the minimum penetration into framing or blocking the table requires — the column that
// makes a listed vn conditional on the nail actually reaching the framing.
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

// How the sheathed assembly carries its in-plane shear — the axis selecting WHICH published table prices it. A shear
// wall and a blocked diaphragm are different tables at the same nailing, and an unblocked diaphragm is a third whose
// columns are load CASES rather than edge spacings, so one row cannot serve all three.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralAssembly {
    public static readonly LateralAssembly ShearWall          = new("shear-wall");
    public static readonly LateralAssembly BlockedDiaphragm   = new("blocked-diaphragm");
    public static readonly LateralAssembly UnblockedDiaphragm = new("unblocked-diaphragm");
}

// A shear-wall row: one grade/thickness/nail configuration across the four published panel-edge nail spacings. Field
// spacing is 12 in o.c. throughout the table, so it is the table's law rather than a per-row column.
public readonly record struct ShearWallRow(
    WspGrade Grade, double PanelThicknessIn, SheathingNail Nail,
    Option<double> At6In, Option<double> At4In, Option<double> At3In, Option<double> At2In, Provenance Source);

// A blocked-diaphragm row: the four published boundary/other-edge spacing PAIRS at one grade, nail, thickness, and
// minimum framing width. Framing width is a real column here (it is not one in the shear-wall table, whose 3 in
// requirement rides a footnote instead), so a 2 in and a 3 in row are distinct rows rather than one row with a factor.
public readonly record struct BlockedDiaphragmRow(
    WspGrade Grade, SheathingNail Nail, double PanelThicknessIn, double FramingWidthIn,
    Option<double> At6And6, Option<double> At4And6, Option<double> At2Half4, Option<double> At2And3, Provenance Source);

// An unblocked-diaphragm row: nails at 6 in o.c. throughout, the two columns being the load CASE — case 1 (framing
// and continuous panel joints parallel to load) against the remaining cases.
public readonly record struct UnblockedDiaphragmRow(
    WspGrade Grade, SheathingNail Nail, double PanelThicknessIn, double FramingWidthIn,
    Option<double> Case1, Option<double> Cases2To6, Provenance Source);

public static class LateralShear {
    public const double InchToMm = 25.4;
    // The published unit is pounds per linear foot; one conversion carries every cell to the kN/m the capacity rail
    // and the Demand column both speak.
    public const double PlfToKnPerM = 0.0145939;

    static Option<double> Plf(double plf) => Some(plf * PlfToKnPerM);
    static readonly Option<double> Uncorroborated = None;

    // Table 4.3A, wood-based panel shear walls. Footnote law this roster does NOT fold into its cells, because each
    // is conditional on member facts a panel row cannot see: a 3/8 or 7/16 panel may take the 15/32 value at the same
    // nailing where studs are at 16 in o.c. or the panel spans across studs; a framing species other than
    // Douglas-Fir-Larch or Southern Pine scales by the specific-gravity adjustment factor; a 10d row scales by 0.92
    // where overturning tension is resisted by a hold-down on the inside face of the end post.
    public static readonly ImmutableArray<ShearWallRow> ShearWalls = [
        new(WspGrade.StructuralI, 0.3125, SheathingNail.Sixd,   Plf(560),  Plf(840),  Plf(1090), Plf(1430), Provenance.Published),
        new(WspGrade.StructuralI, 0.3750, SheathingNail.Eightd, Plf(645),  Plf(1010), Plf(1290), Plf(1710), Provenance.Published),
        new(WspGrade.StructuralI, 0.4375, SheathingNail.Eightd, Plf(715),  Plf(1105), Plf(1415), Plf(1875), Provenance.Published),
        new(WspGrade.StructuralI, 0.4688, SheathingNail.Eightd, Plf(785),  Plf(1205), Plf(1540), Plf(2045), Provenance.Published),
        new(WspGrade.StructuralI, 0.4688, SheathingNail.Tend,   Plf(950),  Plf(1430), Plf(1860), Plf(2435), Provenance.Published),
        new(WspGrade.Sheathing,   0.3125, SheathingNail.Sixd,   Plf(505),  Plf(755),  Plf(980),  Plf(1260), Provenance.Published),
        new(WspGrade.Sheathing,   0.3750, SheathingNail.Sixd,   Plf(560),  Plf(840),  Plf(1090), Plf(1430), Provenance.Published),
        new(WspGrade.Sheathing,   0.3750, SheathingNail.Eightd, Plf(615),  Plf(895),  Plf(1150), Plf(1485), Provenance.Published),
        new(WspGrade.Sheathing,   0.4375, SheathingNail.Eightd, Plf(670),  Plf(980),  Plf(1260), Plf(1640), Provenance.Published),
        new(WspGrade.Sheathing,   0.4688, SheathingNail.Eightd, Plf(730),  Plf(1065), Plf(1370), Plf(1790), Provenance.Published),
        new(WspGrade.Sheathing,   0.4688, SheathingNail.Tend,   Plf(870),  Plf(1290), Plf(1680), Plf(2155), Provenance.Published),
        new(WspGrade.Sheathing,   0.5938, SheathingNail.Tend,   Plf(950),  Plf(1430), Plf(1860), Plf(2435), Provenance.Published)];

    // Table 4.2A, blocked diaphragms. The Sheathing 5/16 rows and the four 6d/8d rows below them reached one
    // corroborating reading each and therefore carry absence: the 5/16 pair reproduces nowhere outside the standard,
    // and the others corroborate only through a municipal amendment printing an allowable derived from a prior
    // edition, which is a re-derivation rather than a reproduction of the nominal.
    public static readonly ImmutableArray<BlockedDiaphragmRow> BlockedDiaphragms = [
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 2.0, Plf(520),  Plf(700),  Plf(1050), Plf(1175), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 3.0, Plf(590),  Plf(785),  Plf(1175), Plf(1330), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 2.0, Plf(755),  Plf(1010), Plf(1485), Plf(1680), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 3.0, Plf(840),  Plf(1120), Plf(1680), Plf(1890), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 2.0, Plf(895),  Plf(1190), Plf(1790), Plf(2045), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 3.0, Plf(1010), Plf(1345), Plf(2015), Plf(2295), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 3.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 3.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 3.0, Plf(755),  Plf(1010), Plf(1510), Plf(1710), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 2.0, Uncorroborated, Uncorroborated, Uncorroborated, Uncorroborated, Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 3.0, Plf(800),  Plf(1065), Plf(1595), Plf(1805), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 2.0, Plf(755),  Plf(1010), Plf(1485), Plf(1680), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 3.0, Plf(840),  Plf(1120), Plf(1680), Plf(1890), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 2.0, Plf(810),  Plf(1080), Plf(1610), Plf(1835), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 3.0, Plf(910),  Plf(1205), Plf(1820), Plf(2060), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 2.0, Plf(895),  Plf(1190), Plf(1790), Plf(2045), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 3.0, Plf(1010), Plf(1345), Plf(2015), Plf(2295), Provenance.Published)];

    // Table 4.2C, unblocked diaphragms.
    public static readonly ImmutableArray<UnblockedDiaphragmRow> UnblockedDiaphragms = [
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 2.0, Plf(460), Plf(350), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Sixd,   0.3125, 3.0, Plf(520), Plf(390), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 2.0, Plf(670), Plf(505), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Eightd, 0.3750, 3.0, Plf(740), Plf(560), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 2.0, Plf(800), Plf(600), Provenance.Published),
        new(WspGrade.StructuralI, SheathingNail.Tend,   0.4688, 3.0, Plf(895), Plf(670), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 2.0, Plf(420), Plf(310), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3125, 3.0, Plf(475), Plf(350), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 2.0, Plf(460), Plf(350), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Sixd,   0.3750, 3.0, Plf(520), Plf(390), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 2.0, Plf(600), Plf(450), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.3750, 3.0, Plf(670), Plf(505), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 2.0, Plf(645), Plf(475), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4375, 3.0, Plf(715), Plf(530), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 2.0, Plf(670), Plf(505), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Eightd, 0.4688, 3.0, Plf(740), Plf(560), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 2.0, Plf(715), Plf(530), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.4688, 3.0, Plf(810), Plf(600), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 2.0, Plf(800), Plf(600), Provenance.Published),
        new(WspGrade.Sheathing,   SheathingNail.Tend,   0.5938, 3.0, Plf(895), Plf(670), Provenance.Published)];

    // The ONE resolution over all three tables: the assembly selects the table, the configuration selects the row, and
    // the schedule or load case selects the column. Every miss is TYPED and names what missed — an unlisted
    // configuration and an uncorroborated cell are different faults, because one means the standard tabulates nothing
    // and the other means this corpus declined to trust its single reading.
    public static Fin<double> Nominal(
        WspGrade grade, double thicknessIn, SheathingNail nail, LateralAssembly assembly,
        double edgeSpacingIn, double framingWidthIn, int loadCase, Op key) =>
        assembly.Key switch {
            "shear-wall" => Row(toSeq(ShearWalls).Find(r => r.Grade == grade && Same(r.PanelThicknessIn, thicknessIn) && r.Nail == nail),
                r => Column(edgeSpacingIn, [(6.0, r.At6In), (4.0, r.At4In), (3.0, r.At3In), (2.0, r.At2In)]), grade, thicknessIn, nail, key),
            "blocked-diaphragm" => Row(toSeq(BlockedDiaphragms).Find(r => r.Grade == grade && Same(r.PanelThicknessIn, thicknessIn) && r.Nail == nail && Same(r.FramingWidthIn, framingWidthIn)),
                r => Column(edgeSpacingIn, [(6.0, r.At6And6), (4.0, r.At4And6), (2.5, r.At2Half4), (2.0, r.At2And3)]), grade, thicknessIn, nail, key),
            _ => Row(toSeq(UnblockedDiaphragms).Find(r => r.Grade == grade && Same(r.PanelThicknessIn, thicknessIn) && r.Nail == nail && Same(r.FramingWidthIn, framingWidthIn)),
                r => loadCase <= 1 ? r.Case1 : r.Cases2To6, grade, thicknessIn, nail, key),
        };

    static Fin<double> Row<TRow>(Option<TRow> found, Func<TRow, Option<double>> cell, WspGrade grade, double thicknessIn, SheathingNail nail, Op key) =>
        found.Match(
            Some: row => cell(row).Match(
                Some: Fin.Succ,
                None: () => Fin.Fail<double>(ComponentFault.Capacity(key, $"<lateral-cell-uncorroborated:{grade.Key}:{thicknessIn:R}:{nail.Key}>"))),
            None: () => Fin.Fail<double>(ComponentFault.Capacity(key, $"<lateral-configuration-unlisted:{grade.Key}:{thicknessIn:R}:{nail.Key}>")));

    // Column selection is nearest-listed-at-or-tighter: a schedule tighter than the tightest published spacing reads
    // the tightest published cell rather than extrapolating past the tested nailing, and a looser schedule than the
    // loosest listed answers nothing at all.
    static Option<double> Column(double spacingIn, ReadOnlySpan<(double SpacingIn, Option<double> Value)> columns) {
        Option<double> chosen = None;
        foreach ((double listed, Option<double> value) in columns) {
            if (spacingIn <= listed) { chosen = value; }
        }
        return chosen;
    }

    // Published thicknesses are exact binary fractions of an inch, so equality is a tolerance read rather than a
    // float compare — a 15/32 panel written 0.469 and one written 0.4688 are the same product.
    static bool Same(double a, double b) => Math.Abs(a - b) < 5e-4;
}

// The typed board-fastening schedule — GENERATED admission ([ComplexValueObject]): the validation partial owns
// the positive-finite spacing guard and the non-negative edge distance (a welded deck carries 0 inset), the ONE
// railed Of lifts the generated outcome onto ComponentFault.Dimension. EdgeStations/FieldStations derive the
// per-board-axis station counts the spec sheathing stage places (a welded deck reads EdgeStations as its
// side-lap weld count).
[ComplexValueObject]
public readonly partial struct FastenPattern {
    public double FieldSpacingMm { get; }
    public double EdgeSpacingMm { get; }
    public double EdgeDistanceMm { get; }
    public PanelFastening Fastener { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double fieldSpacingMm, ref double edgeSpacingMm, ref double edgeDistanceMm, ref PanelFastening fastener) =>
        validationError = fastener is not null && double.IsFinite(fieldSpacingMm) && fieldSpacingMm > 0.0 && double.IsFinite(edgeSpacingMm) && edgeSpacingMm > 0.0 && double.IsFinite(edgeDistanceMm) && edgeDistanceMm >= 0.0
            ? null
            : new ValidationError($"<fasten-schedule-invalid:field={fieldSpacingMm}:edge={edgeSpacingMm}:inset={edgeDistanceMm}>");

    public static Fin<FastenPattern> Of(double fieldMm, double edgeMm, double edgeDistMm, PanelFastening fastener, Op key) =>
        Validate(fieldMm, edgeMm, edgeDistMm, fastener, out FastenPattern pattern) is { } error
            ? Fin.Fail<FastenPattern>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(pattern);

    public int EdgeStations(PositiveMagnitude axisLengthMm) => Math.Max(2, (int)Math.Floor(axisLengthMm.Value / EdgeSpacingMm) + 1);
    public int FieldStations(PositiveMagnitude axisLengthMm) => Math.Max(2, (int)Math.Floor(axisLengthMm.Value / FieldSpacingMm) + 1);
}

// Product-specific payload is a closed family: each row carries only the axes its product form admits.
[Union]
public abstract partial record PanelSpecification {
    private PanelSpecification() { }
    public sealed record GypsumBoard(CoreType Core, Facer Facer) : PanelSpecification;
    // Grade and Nail are the SDPWS lateral-table KEYS: the published unit-shear roster is cut by panel grade and by
    // the reference common nail, neither of which the span rating or the bond class determines. Nail is distinct from
    // the row's PanelFastening, which is the install token the detail bag stamps — the table keys on the reference
    // fastener it was tested with, and a row installed with something else is outside the tabulated configuration.
    public sealed record WoodPanel(SpanRow Span, BondClass Bond, WspGrade Grade, SheathingNail Nail) : PanelSpecification;
    public sealed record FacedBoard(Facer Facer) : PanelSpecification;
    public sealed record DeckSheet(DeckForm Form, DeckProfileRow Rib, GaugeRow Gauge) : PanelSpecification;
    public sealed record FoamBoard(FoamType Foam, Facer Facer) : PanelSpecification;
    public sealed record Membrane : PanelSpecification;

    public Facer LayupFacer => Switch(
        gypsumBoard: static specification => specification.Facer,
        facedBoard: static specification => specification.Facer,
        foamBoard: static specification => specification.Facer,
        woodPanel: static _ => Facer.None, deckSheet: static _ => Facer.None, membrane: static _ => Facer.None);
    // One case out of six answers, so the read is the pattern that asks for it — five arms spelling absence were
    // five lines restating that the other cases are not this one.
    public Option<DeckSheet> Deck => this is DeckSheet deck ? Some(deck) : None;

    // The per-product rows, plus the ONE membrane-seam token: a lapped single-ply roll's seam METHOD is its fastening
    // (heat weld or seam adhesive), so DetailSchema.MembraneSeam emits from the membrane arm alone — the prior
    // unconditional stamp put "drywall-screw" in a membrane-seam field on every gypsum board, which a Rasm.Bim reader
    // then recovered as a seam fact. Every panel's fastening rides DetailSchema.FastenerType instead, the row
    // connector#CONNECTOR_FAMILY already uses for the same concept.
    public Fin<Seq<(PropertyName Name, PropertyValue Value)>> DetailRows(PositiveMagnitude thicknessMm, PanelFastening fastening) => Switch(
        gypsumBoard: specification => Fin.Succ(
            Seq(ComponentDetail.Token(DetailSchema.CoreClass, specification.Core.Key))
            + FacerRow(specification.Facer)),
        // The SpanRow conversions gain their consumer: both published roof spans and the floor span cross as MEASURED
        // SI rows beside the token, so a downstream framing check reads the maximum support spacing as a quantity
        // instead of parsing "24/16" out of a string — and reads the EDGE-SUPPORTED and UNSUPPORTED roof spacings as
        // the two different numbers the rating publishes. A roof-only rating carries no floor row at all.
        woodPanel: specification =>
            from supported in ComponentDetail.Measured(DetailSchema.RoofSpan, Dimension.LengthDim, specification.Span.RoofEdgeSupportedMm * 1e-3)
            from unsupported in ComponentDetail.Measured(DetailSchema.RoofSpanUnsupported, Dimension.LengthDim, specification.Span.RoofUnsupportedMm * 1e-3)
            from floor in specification.Span.FloorSpanMm.Match(
                Some: span => ComponentDetail.Measured(DetailSchema.FloorSpan, Dimension.LengthDim, span * 1e-3).Map(Some),
                None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
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
        membrane: _ => Fin.Succ(Seq(ComponentDetail.Token(DetailSchema.MembraneSeam, fastening.Key))));

    // A bare-core board has no facer to name — one absence read shared by the three arms that can carry one.
    static Seq<(PropertyName Name, PropertyValue Value)> FacerRow(Facer facer) =>
        facer == Facer.None ? Empty : Seq(ComponentDetail.Token(DetailSchema.FacerClass, facer.Key));
}

public readonly record struct PanelRow(
    string Designation, PanelKind Kind, double WidthMm, double LengthMm, double ThicknessMm,
    EdgeProfile Edge, PanelOrientation Orientation, PanelFastening Fastener, double FieldMm, double EdgeMm, double EdgeDistMm,
    PanelSpecification Specification) {
    // Board dimensions and fastening schedules are PUBLISHED manufacturer/ANSI data; a row extended off a nominal
    // sheet module states Defined here rather than under a table-wide banner the extension would silently outgrow.
    public Provenance Source { get; init; } = Provenance.Published;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-built PRODUCT bag (DetailLane.Product): the edge token, the FASTENING token, thickness, field/edge
// spacing (the dimension-only MeasureValue.OfSi mints preserved), the Corrugated rib depth/pitch rows for a deck, and
// the dissolved payload's product columns with no other landing — board length, orientation, core/span/bond/foam/
// facer tokens, the deck form, the seed-computed SI thermal resistance for a foam board, and the membrane seam on the
// membrane arm alone. Every panel stamps DetailSchema.FastenerType off the fastening's own canonical token (the
// FastenerKind IFC predefined value where a mechanical kind exists, the fastening key for a weld or bond) — the row
// connector#CONNECTOR_FAMILY stamps for the same concept — so a Rasm.Bim reader recovers a gypsum board's screw from
// a FASTENER field and a single-ply roll's weld from a SEAM field. Token/Measured/ProductRows are the relocated
// component#COMPONENT_DETAIL constructors.
public static class PanelDetail {
    public static Fin<PropertyBag> Of(
        PositiveMagnitude lengthMm, PositiveMagnitude thicknessMm, EdgeProfile edge, PanelOrientation orientation,
        FastenPattern fastening, PanelSpecification specification, Provenance source) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from field in ComponentDetail.Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, fastening.FieldSpacingMm * 1e-3)
        from edgeSpacing in ComponentDetail.Measured(DetailSchema.EdgeSpacing, Dimension.LengthDim, fastening.EdgeSpacingMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, lengthMm.Value * 1e-3)
        from payloadRows in specification.DetailRows(thicknessMm, fastening.Fastener)
        from shank in fastening.Fastener.ShankDiameterMm.Match(
            Some: mm => ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, mm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
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

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED board roster: ASTM C1396/C1396M-24 + EN 520 gypsum (incl. the CEILING-leaf board), ASTM C1177/C1178
// glass-mat sheathing + water-resistant tile backer, APA PRP-108 / PS 1-19 / PS 2-18 wood structural panels, ASTM C1325 cement board (incl.
// the FLOORING-leaf underlayment), ANSI/SDI RD-2017 / C-2017 steel deck, ASTM C578/C1289 rigid board, and the
// single-ply roof membranes. Dimensions/schedules PUBLISHED verbatim; the board-product standards carry no
// regional mortar joint (joint 0.0), and the authority rides the KIND's own standards body (ASTM/APA/SDI).
public static class PanelSeed {
    // The nominal sheet MODULE and the schedules built on it. Every board in the roster is cut from one of a handful
    // of standard sheet sizes and fastened on one of a handful of standard schedules, so those dimensions are named
    // policy values the rows reference: the 4-foot width and 8-foot length recurred across twenty rows and the
    // 12-inch field / 8-inch edge / 3/8-inch inset triple across ten, each spelled as a raw millimetre conversion
    // that a reader had to recognize before knowing two rows agreed.
    const double Sheet4FtMm = 1219.2;
    const double Sheet8FtMm = 2438.4;
    const double FieldPitch12InMm = 304.8;
    const double EdgePitch8InMm = 203.2;
    const double EdgePitch6InMm = 152.4;
    const double EdgeInset38InMm = 9.5;
    // A deck row's printed sheet width and thickness must AGREE with the rib row it references — the same profile
    // print authored twice must not diverge silently. The comparison is toleranced rather than exact: both sides are
    // inch dimensions carried as millimetre doubles, so an exact equality gate passes only while both spellings
    // happen to round identically and turns a re-derived conversion into a build abort.
    const double DeckDriftToleranceMm = 0.1;

    static readonly Seq<PanelRow> Roster = Seq(
        // --- gypsum board (ASTM C1396; EN 520) — tapered/square edge, drywall-screw 12in field / 8in edge
        new PanelRow("panel.gyp-reg-050-4x8",   PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-reg-038-4x8",   PanelKind.GypsumBoard,   1219.2, 2438.4, 9.5,  EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-x-050-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-54x12",   PanelKind.GypsumBoard,   1371.6, 3657.6, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-c-625-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeCFire, Facer.None)),
        new PanelRow("panel.gyp-mr-050-4x8",    PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.MoistureResistant, Facer.None)),
        new PanelRow("panel.gyp-abuse-625-4x8", PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.AbuseResistant, Facer.None)),
        new PanelRow("panel.gyp-025-4x8",       PanelKind.GypsumBoard,   1219.2, 2438.4, 6.4,  EdgeProfile.Square,  PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-ceil-050-4x8",  PanelKind.GypsumCeiling, 1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        // --- gypsum sheathing (ASTM C1396 gypsum-sheathing; ASTM C1177 glass-mat) — square edge, glass-mat facer
        new PanelRow("panel.gypsheath-x-050-4x8",   PanelKind.GypsumSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-x-625-4x8",   PanelKind.GypsumSheathing, 1219.2, 2438.4, 15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-gm-625-4x10", PanelKind.GypsumSheathing, 1219.2, 3048.0, 15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.GlassMat, Facer.GlassFiberMat)),
        new PanelRow("panel.gyp-wr-backer-050-3x5", PanelKind.GypsumSheathing, 914.4,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.WaterResistant, Facer.GlassFiberMat)),
        // --- plywood sheathing (APA PRP-108 / PS 1-19; EN 13986/636) — span-rated, 8d nail edge 6in / field 12in
        new PanelRow("panel.ply-rated-038-4x8-240",   PanelKind.PlywoodSheathing, 1219.2, 2438.4, 9.5,  EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-1532-4x8-2416", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-050-4x8-3216",  PanelKind.PlywoodSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-1932-4x8-4020", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 15.1, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-2332-4x8-4824", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior, WspGrade.Sheathing, SheathingNail.Tend)),
        new PanelRow("panel.ply-str1-1932-4x8",       PanelKind.PlywoodSheathing, 1219.2, 2438.4, 15.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exterior, WspGrade.StructuralI, SheathingNail.Eightd)),
        new PanelRow("panel.ply-rated-075-4x8-4824",  PanelKind.PlywoodSheathing, 1219.2, 2438.4, 19.0, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior, WspGrade.Sheathing, SheathingNail.Tend)),
        // --- osb sheathing (APA PRP-108 / PS 2-18; EN 13986/300)
        new PanelRow("panel.osb-rated-716-4x8-240",   PanelKind.OsbSheathing, 1219.2, 2438.4, 11.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-1532-4x8-2416", PanelKind.OsbSheathing, 1219.2, 2438.4, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-050-4x8-3216",  PanelKind.OsbSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        new PanelRow("panel.osb-rated-2332-4x8-4824", PanelKind.OsbSheathing, 1219.2, 2438.4, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Tend)),
        new PanelRow("panel.osb-rated-1532-4x24",     PanelKind.OsbSheathing, 1219.2, 7315.2, 11.9, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, PanelFastening.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1, WspGrade.Sheathing, SheathingNail.Eightd)),
        // --- cement board (ASTM C1325; ANSI A118.9) — glass-mesh scrim, edge-dist 3/4in
        new PanelRow("panel.cbu-025-3x5",          PanelKind.CementBoard,        914.4,  1524.0, 6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-3x5",          PanelKind.CementBoard,        914.4,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-4x8",          PanelKind.CementBoard,        1219.2, 2438.4, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-625-4x8",          PanelKind.CementBoard,        1219.2, 2438.4, 15.9, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-32x60",        PanelKind.CementBoard,        812.8,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-underlay-014-3x5", PanelKind.CementUnderlayment, 914.4,  1524.0, 6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, PanelFastening.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        // --- steel deck (ANSI/SDI RD-2017 roof; C-2017 composite) — coverage/pitch/depth from the referenced DeckProfiles row, base metal from the connector GaugeRow key
        new PanelRow("panel.deck-b-22ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-b-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga20)),
        new PanelRow("panel.deck-a-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.NarrowRibA, Gauges.Ga20)),
        new PanelRow("panel.deck-f-18ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.IntermediateF, Gauges.Ga18)),
        new PanelRow("panel.deck-n-18ga-roof", PanelKind.SteelDeckRoof,  609.6, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.DeepN, Gauges.Ga18)),
        new PanelRow("panel.deck-bform-22ga",  PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckScrew, 304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Form, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-15vl-20ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite15, Gauges.Ga20)),
        new PanelRow("panel.deck-2vli-18ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 50.8, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite2Vli, Gauges.Ga18)),
        new PanelRow("panel.deck-3vli-16ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, PanelFastening.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite3Vli, Gauges.Ga16)),
        // --- rigid-board insulation (ASTM C578 EPS/XPS; C1289 polyiso)
        new PanelRow("panel.eps-1in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-2in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 50.8,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-4in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 101.6, EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.Adhesive,      406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.xps-1in-2x8",      PanelKind.RigidBoardXps,  609.6,  2438.4, 25.4,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.xps-2in-2x8",      PanelKind.RigidBoardXps,  609.6,  2438.4, 50.8,  EdgeProfile.TongueGroove, PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.polyiso-1in-4x8",  PanelKind.RigidBoardPoly, 1219.2, 2438.4, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.GlassFiberMat)),
        new PanelRow("panel.polyiso-2in-foil", PanelKind.RigidBoardPoly, 1219.2, 2438.4, 50.8,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.Foil)),
        new PanelRow("panel.polyiso-3in-4x8",  PanelKind.RigidBoardPoly, 1219.2, 2438.4, 76.2,  EdgeProfile.Square,       PanelOrientation.Unidirectional, PanelFastening.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.CoatedGlass)),
        // --- single-ply roof membranes — roof/wall/floor remains a spec layout role; panel is the product form
        new PanelRow("panel.epdm-060-roll", PanelKind.MembraneEpdm, 3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.SeamAdhesive, 304.8, 152.4, 0.0, new PanelSpecification.Membrane()),
        new PanelRow("panel.pvc-060-roll",  PanelKind.MembranePvc,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.HeatWeld,     304.8, 152.4, 0.0, new PanelSpecification.Membrane()),
        new PanelRow("panel.tpo-060-roll",  PanelKind.MembraneTpo,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, PanelFastening.HeatWeld,     304.8, 152.4, 0.0, new PanelSpecification.Membrane()));

    // The kind-routed profile: a deck row fills the six Corrugated named dims from its REFERENCED DeckProfileRow +
    // the SYMBOLICALLY referenced connector GaugeRow base metal (GaugeMm = gauge.BaseThicknessMm — the prior
    // duplicated BaseMetalMm column and the prior runtime-railed gauge Find are both deleted: the Gauges roster
    // carries the deck bands 22ga..16ga as named statics, so nothing resolves and nothing can miss); every other
    // row builds Layered from the kind's own layup delegate. The drift guard proves the roster's printed
    // width/thickness agree with the rib row's coverage/depth — the same SDI print authored twice must not diverge
    // silently. Profile construction is the railed SectionProfile Of INSIDE the Traverse; the section INTEGRAL for
    // Sectioned rows runs once in ComponentCatalogue.Of through SectionSolver.Solve's corrugated arm.
    static Fin<(SectionProfile Profile, MaterialId Substance)> ProfileOf(
        PanelRow r, PositiveMagnitude width, PositiveMagnitude thickness, Op key) =>
        r.Specification.Deck.Match(
            Some: deck =>
                from aligned in guard(
                    Math.Abs(thickness.Value - deck.Rib.RibDepthMm) <= DeckDriftToleranceMm
                        && Math.Abs(width.Value - deck.Rib.CoverageMm) <= DeckDriftToleranceMm,
                    ComponentFault.Dimension(key, $"<deck-row-dims-drift:{r.Designation}>"))
                from corrugated in SectionProfile.Corrugated.Of(
                    coverWidthMm: deck.Rib.CoverageMm, ribDepthMm: deck.Rib.RibDepthMm, ribPitchMm: deck.Rib.RibPitchMm,
                    gaugeMm: deck.Gauge.BaseThicknessMm, topFlatMm: deck.Rib.TopFlatMm, bottomFlatMm: deck.Rib.BottomFlatMm, key)
                select (corrugated, deck.Gauge.Substance),
            None: () =>
                from plies in r.Kind.Layup(r.Specification.LayupFacer, thickness)
                    .ToFin(ComponentFault.Dimension(key, $"<board-thinner-than-its-own-facings:{r.Designation}>"))
                from layered in SectionProfile.Layered.Of(plies, overallMm: thickness.Value, widthMm: width.Value, key)
                select (layered, r.Kind.Substance));

    // The appearance slot: the outermost facing ply's material (probed through the bounded PlyRole), the substance for a bare-core board or a deck — INDEPENDENT of SubstanceId per the
    // two-slot law (a foil-faced polyiso keeps foam substance + facer appearance).
    static MaterialId AppearanceOf(SectionProfile profile, MaterialId substance) =>
        profile is SectionProfile.Layered layered
            ? layered.Plies.Find(static ply => ply.Role.Facing)
                .Map(static ply => ply.Material).IfNone(substance)
            : substance;

    // The ONE generator fold (RAIL law): Traverse accumulates EVERY failing row's fault (applicative Fin), then the
    // build aborts — never Choose/ToOption; the prior THREE swallow sites (Shapes/BuildPanelRows/PanelSections) are
    // DELETED. A deck-presence mismatch, a malformed dimension, or a bad schedule faults its row; EVERY vocabulary
    // axis — the deck gauge included — is a typed row REFERENCE, so nothing resolves at runtime and nothing can
    // miss. Sectioned derives from the admitted DeckSheet case; ComponentCatalogue.Of
    // traverses SectionSolver.Solve over exactly those rows.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Roster.Traverse(r =>
            from admitted in guard(r.Kind.Admits(r.Specification),
                ComponentFault.Family(context.Key, $"<panel-kind-spec-mismatch:{r.Designation}:{r.Kind.Key}>"))
            from width in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
            from length in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
            from thickness in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
            from fastening in FastenPattern.Of(r.FieldMm, r.EdgeMm, r.EdgeDistMm, r.Fastener, context.Key)
            from routed in ProfileOf(r, width, thickness, context.Key)
            from detail in PanelDetail.Of(length, thickness, r.Edge, r.Orientation, fastening, r.Specification, Source(r))
            from item in Component.Of(
                ComponentFamily.Panel, r.Designation, routed.Profile,
                IfcBinding.Of(r.Kind.IfcEntity, r.Kind.IfcPredefinedType),
                Coring.None, new ComponentStandard("us", StandardJointThicknessMm: 0.0, r.Kind.Authority),
                substanceId: routed.Substance,
                appearanceId: AppearanceOf(routed.Profile, routed.Substance),
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, Source(r))).As();

    // A deck row inherits its rib profile's OWN provenance — the SDI roof series is standardized and the composite
    // series proprietary, so the two do not share one label — while a board row transcribes its product standard's
    // published dimensions and schedules whole.
    static Provenance Source(PanelRow r) => r.Specification.Deck.Match(
        Some: static deck => deck.Rib.Source,
        None: static () => Provenance.Published);

    static readonly FrozenDictionary<ComponentId, PanelRow> Table =
        Roster.ToFrozenDictionary(static row => ComponentId.Create(row.Designation), static row => row);

    // The ComponentFamily.Panel CAPACITY producer, over the two board forms that carry a published resistance. A DECK
    // sheet is priced out of plane: its solved Corrugated section runs the AISI body at the gauge band's own yield and
    // lifts as CapacityReceipt.DeckSheet, which is what makes GaugeRow.AxialSectionCapacityKnPerMm a consumed datum. A
    // WOOD STRUCTURAL PANEL is priced IN PLANE off its own tabulated unit shear, reduced ONCE here — the rail and the
    // placement's hazard both exist at this seat and nowhere downstream, which is why the receipt carries a finished
    // design value exactly as the connector receipts do. Every other board form refuses TYPED and names itself: a
    // gypsum, faced, foam, or membrane board publishes no in-plane table, and returning some other family's verdict
    // for one would be worse than returning none.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Table.TryGetValue(component.Designation, out PanelRow found)
            ? Fin.Succ(found)
            : Fin.Fail<PanelRow>(ComponentFault.Family(key, $"<panel-row-unregistered:{component.Designation.Value}>"))
        from capacity in row.Specification.Switch(
            deckSheet: deck =>
                from solved in section.ToFin(ComponentFault.Section(key, $"<deck-section-unresolved:{row.Designation}>"))
                from design in SteelDesign.Capacity(component.Profile, deck.Rib.Grade, solved, placement, key)
                select SectionCapacity.Lift(new CapacityReceipt.DeckSheet(component.Designation, deck.Gauge, deck.Rib, design)),
            woodPanel: wood =>
                from nominal in LateralShear.Nominal(
                    wood.Grade, row.ThicknessMm / LateralShear.InchToMm, wood.Nail, placement.Assembly,
                    row.EdgeMm / LateralShear.InchToMm, placement.FramingWidthMm / LateralShear.InchToMm,
                    placement.DiaphragmCase, key)
                from design in placement.Hazard.Design(nominal, DesignBasis.Sdpws.Format, key)
                select SectionCapacity.Lift(new CapacityReceipt.LateralPanel(component.Designation, design, placement.Hazard)),
            gypsumBoard: _ => Unpriced(row, key), facedBoard: _ => Unpriced(row, key),
            foamBoard: _ => Unpriced(row, key), membrane: _ => Unpriced(row, key))
        select capacity;

    static Fin<SectionCapacity> Unpriced(PanelRow row, Op key) =>
        ComponentFault.Capacity(key, $"<panel-form-publishes-no-resistance:{row.Designation}:{row.Kind.Key}>");
}
```

## [03]-[RESEARCH]

(none)
