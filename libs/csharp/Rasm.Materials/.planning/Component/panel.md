# [MATERIALS_PANEL]

THE PANEL SEED PAGE owns the `ComponentFamily.Panel` policy row, the product vocabularies, frozen standards rows, `FastenPattern`, the closed `PanelSpecification` payload, and the single `PanelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` fold. Board geometry is `SectionProfile.Layered` over the shared bounded `PlyRole`; deck geometry is `SectionProfile.Corrugated`, solved by the canonical `SectionSolver` arm. `Sectioned` follows structural kind, every row carries its kind-owned `IfcBinding`, and every product detail measurement remains on the `Fin` rail through catalogue construction.

## [01]-[INDEX]

- [02]-[PANEL_FAMILY]: the `PanelKind` board-type vocabulary; the edge, orientation, fastening, core, bond, foam, facer, and deck policies; the frozen deck/span tables; `FastenPattern`; the payload-timed `PanelSpecification` union; the `PanelRow` roster; `PanelDetail`; and `PanelSeed.Rows`.

## [02]-[PANEL_FAMILY]

- Owner: `PanelKind` carries the board-type axis, IFC leaf, structural flag, substance, authority, and `[UseDelegateFromConstructor]` layup; the shared `PlyRole` bounds every layer role across panel, glazing, timber, and masonry. `EdgeProfile`, `PanelOrientation`, `FastenerType`, `DeckForm`, `CoreType`, `BondClass`, `FoamType`, and `Facer` carry product policy; `DeckProfiles`, `SpanRatings`, `FastenPattern`, `PanelSpecification`, `PanelDetail`, and `PanelSeed` carry printed data, admission, payload timing, details, and construction.
- Cases: kind {fifteen rows over the tri-entity IFC spread; a fiberboard or magnesium-oxide board is one new row reusing `IfcCovering`/CLADDING and an existing layup} · edge {square/tapered/beveled/rounded/tongue-groove/shiplap/side-lap-interlock/lapped-seam} · orientation {strength-axis-perpendicular/parallel/unidirectional} · profile {`Layered` every covering/membrane board — the layup delegate's `Ply` stack; `Corrugated` the two steel-deck kinds — the `DeckProfiles` row + `GaugeRow` fill the six named dims}.
- Entry: `PanelSeed.Rows(Context)` traverses each row through dimension and fastening admission, resolves `PanelSpecification.DeckSheet` to `SectionProfile.Corrugated` and every other specification to `SectionProfile.Layered`, builds the detail bag from union projections, and seals the component. `PanelSpecification` keeps gypsum core/facer, wood span/bond, faced-board facer, deck form/profile/gauge, foam type/facer, and membrane absence in disjoint cases; no row carries irrelevant `None` policy values.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `PropertyBag`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` layup columns, `[ComplexValueObject]` + generated `ValidateFactoryArguments`/`Validate`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`), BCL inbox (`ImmutableArray`); NO sheet-goods external producer — the roster is `AUTHORED` under `SEED_ROW_LAW` (VividOrange owns structural-MEMBER catalogues and EN grades, not gypsum/sheathing/deck/insulation rosters); the VividOrange solve surface moves with the section machinery to `component#SECTION_SOLVER`; the deck base metal reuses the `connector#CONNECTOR_FAMILY` `GaugeRow`/`Gauges` table (one cold-formed gauge vocabulary — `Ga22`..`Ga10` named statics, PUBLISHED AISI base/design thickness and gauge-band yield; the deck rows reference `Ga22`/`Ga20`/`Ga18`/`Ga16` symbolically).
- Growth: a new board is one `PanelRow`; a new kind one `PanelKind` row binding its IFC leaf + layup delegate; a new edge/orientation/core/bond/foam/facer band one vocabulary row; a new deck profile one `DeckProfileRow`; a new span rating one `SpanRow` — ZERO type edits per `[DIFF_OF_NEXT_THING]` ("Panel (new board) — one `ComponentRow` in `PanelSeed.Rows` — thickness, width, IFC leaf, fastening all row values").
- Boundary: this page emits DATA — profiles, vocabulary rows, bags, and the seed fold; the section INTEGRAL is `component#SECTION_SOLVER`'s `corrugated` arm (one solver, compiler-forced per profile arm), the twenty-column `ComputedSection` lift is `SectionSolver.Admit`, and the per-coverage rib scaling reads `CoverWidthMm/RibPitchMm` off the `Corrugated` dims inside `Forms.ThinFold` — panel keeps only the `DeckProfiles`/`Gauges` DATA those dims read; the board substance physics (`gypsum.board`/`cement.board`/`wood.plywood`/`wood.osb`/`insulation.eps`/`xps`/`pir`/membranes) read ONCE from the property library by `SubstanceId`, never re-keyed here; `SubstanceId`/`AppearanceId` stay INDEPENDENT slots (a foil-faced polyiso keeps its foam substance while its appearance names the facer; a deck's substance is its gauge steel); the layup is the typed `Seq<Ply>` the seam `CompositionAuthor.LayerSet` coerces into `IfcMaterialLayerSet` (a deck is a `ProfileSet` — a ribbed sheet profiles, never layers); `IfcBinding` strings stay neutral (the generated `Rasm.Bim` roster validates composition-time and egress-time; the `IfcCoveringTypeEnum` has NO SHEATHING member so lining and sheathing are both CLADDING, the `IfcSlabTypeEnum` has NO COMPOSITE/DECK member so a composite floor deck is `IfcSlab`/FLOOR); the product bag rides the Type `Object` via `Assign.PropertyDefinition` and round-trips through the GENERAL Bim `Object`/property fold (a panel is an `IfcBuiltElement`, never a realizing element).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;  // ImmutableArray (the frozen printed-data row tables)
using LanguageExt;                   // Fin, Option, Seq, Traverse
using Rasm.Numerics;                  // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                   // Op, Context, AcceptValidated
using Rasm.Element.Composition;                  // MaterialId, PropertyBag, DetailSchema, Dimension, PropertyName, PropertyValue (the seam bag currencies PanelDetail composes)
using Rasm.Element.Properties;
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
    public static readonly PanelKind GypsumBoard        = new("gypsum-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.PaperFace, "paper.face", "gypsum.board", t));
    public static readonly PanelKind GypsumCeiling      = new("gypsum-ceiling",      ifcEntity: "IfcCovering", ifcPredefinedType: "CEILING",    substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.PaperFace, "paper.face", "gypsum.board", t));
    public static readonly PanelKind GypsumSheathing    = new("gypsum-sheathing",    ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "gypsum.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMatFacer, "glass.mat", "gypsum.board", t));
    public static readonly PanelKind PlywoodSheathing   = new("plywood-sheathing",   ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.plywood",   authority: ComponentAuthority.Apa, layup: static (f, t) => Mono(PlyRole.VeneerPly, "wood.plywood", t));
    public static readonly PanelKind OsbSheathing       = new("osb-sheathing",       ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "wood.osb",       authority: ComponentAuthority.Apa, layup: static (f, t) => Mono(PlyRole.StrandLayer, "wood.osb", t));
    public static readonly PanelKind CementBoard        = new("cement-board",        ifcEntity: "IfcCovering", ifcPredefinedType: "CLADDING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMeshScrim, "glass.scrim", "cement.board", t));
    public static readonly PanelKind CementUnderlayment = new("cement-underlayment", ifcEntity: "IfcCovering", ifcPredefinedType: "FLOORING",   substanceId: "cement.board",   authority: ComponentAuthority.Astm, layup: static (f, t) => FaceCoreFace(PlyRole.GlassMeshScrim, "glass.scrim", "cement.board", t));
    public static readonly PanelKind SteelDeckRoof      = new("steel-deck-roof",     ifcEntity: "IfcPlate",    ifcPredefinedType: "SHEET",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi, layup: static (f, t) => Seq<Ply>());
    public static readonly PanelKind SteelDeckFloor     = new("steel-deck-floor",    ifcEntity: "IfcSlab",     ifcPredefinedType: "FLOOR",      substanceId: "metal.steel",    authority: ComponentAuthority.Sdi, layup: static (f, t) => Seq<Ply>());
    public static readonly PanelKind RigidBoardEps      = new("rigid-board-eps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.eps", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.eps", t));
    public static readonly PanelKind RigidBoardXps      = new("rigid-board-xps",     ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.xps", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.xps", t));
    public static readonly PanelKind RigidBoardPoly     = new("rigid-board-poly",    ifcEntity: "IfcCovering", ifcPredefinedType: "INSULATION", substanceId: "insulation.pir", authority: ComponentAuthority.Astm, layup: static (f, t) => FacedFoam(f, "insulation.pir", t));
    public static readonly PanelKind MembraneEpdm       = new("membrane-epdm",       ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.epdm",  authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.epdm", t));
    public static readonly PanelKind MembranePvc        = new("membrane-pvc",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.pvc",   authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.pvc", t));
    public static readonly PanelKind MembraneTpo        = new("membrane-tpo",        ifcEntity: "IfcCovering", ifcPredefinedType: "ROOFING",    substanceId: "membrane.tpo",   authority: ComponentAuthority.Astm, layup: static (f, t) => Mono(PlyRole.MembraneCore, "membrane.tpo", t));
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public string SubstanceId { get; }
    public ComponentAuthority Authority { get; }   // the kind's OWN standards body — ASTM boards/foams/membranes, APA wood panels, SDI deck; never one blended authority
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    // The shared bounded PlyRole keeps layer policy typed through section and appearance projection.
    [UseDelegateFromConstructor]
    public partial Seq<Ply> Layup(Facer facer, PositiveMagnitude thickness);

    // Two thin faces over a core the remainder (the ~0.5 mm paper/glass-mat/scrim face; layup sums to the board
    // thickness so Component.Of's laminate guard holds). Derived-from-admitted mints ride PositiveMagnitude.Create.
    static Seq<Ply> FaceCoreFace(PlyRole face, string faceId, string coreId, PositiveMagnitude t) =>
        Seq(new Ply(MaterialId.Of(faceId), PositiveMagnitude.Create(0.5), face),
            new Ply(MaterialId.Of(coreId), PositiveMagnitude.Create(Math.Max(0.5, t.Value - 1.0)), face == PlyRole.GlassMeshScrim ? PlyRole.CementAggregateCore : PlyRole.GypsumCore),
            new Ply(MaterialId.Of(faceId), PositiveMagnitude.Create(0.5), face));

    // One homogeneous ply the host subdivides (veneer plies, strand mat, membrane sheet).
    static Seq<Ply> Mono(PlyRole role, string substanceId, PositiveMagnitude t) =>
        Seq(new Ply(MaterialId.Of(substanceId), t, role));

    // Facer/foam-core/facer rigid board: two-sided where the facer is, single-face else, bare core for Facer.None.
    static Seq<Ply> FacedFoam(Facer facer, string foamId, PositiveMagnitude t) {
        if (facer == Facer.None) { return Seq(new Ply(MaterialId.Of(foamId), t, PlyRole.FoamCore)); }
        PlyRole role = facer == Facer.Foil ? PlyRole.FoilFacer : PlyRole.GlassFiberMatFacer;
        double faces = facer.FacedBoth ? 2.0 : 1.0;
        Ply face = new(MaterialId.Of($"facer.{facer.Key}"), PositiveMagnitude.Create(0.2), role);
        Ply core = new(MaterialId.Of(foamId), PositiveMagnitude.Create(Math.Max(0.2, t.Value - 0.2 * faces)), PlyRole.FoamCore);
        return facer.FacedBoth ? Seq(face, core, face) : Seq(face, core);
    }
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
    public static readonly CoreType None              = new("none",               fireRated: false);
    public static readonly CoreType Regular           = new("regular",            fireRated: false);
    public static readonly CoreType TypeXFire         = new("type-x-fire",        fireRated: true);
    public static readonly CoreType TypeCFire         = new("type-c-fire",        fireRated: true);
    public static readonly CoreType MoistureResistant = new("moisture-resistant", fireRated: false);
    public static readonly CoreType AbuseResistant    = new("abuse-resistant",    fireRated: false);
    public static readonly CoreType GlassMat          = new("glass-mat",          fireRated: false);   // ASTM C1177 glass-mat exterior sheathing
    public static readonly CoreType WaterResistant    = new("water-resistant",    fireRated: false);
    public bool FireRated { get; }
}

// APA / EN 13986 wood-structural-panel exposure/bond durability.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondClass {
    public static readonly BondClass None      = new("none",       exterior: false);
    public static readonly BondClass Exposure1 = new("exposure-1", exterior: false);
    public static readonly BondClass Exterior  = new("exterior",   exterior: true);
    public bool Exterior { get; }
}

// Rigid-board foam chemistry (ASTM C578 EPS/XPS, C1289 polyiso): the design R-per-inch POLICY the seed-computed
// thermal receipt reads, and the compressive band. PUBLISHED design values; polyiso is the aged LTTR.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FoamType {
    public static readonly FoamType None    = new("none",    rValuePerInch: 0.0,  compressiveStrengthPsi: 0.0);
    public static readonly FoamType Eps     = new("eps",     rValuePerInch: 3.85, compressiveStrengthPsi: 15.0);
    public static readonly FoamType Xps     = new("xps",     rValuePerInch: 5.0,  compressiveStrengthPsi: 25.0);
    public static readonly FoamType Polyiso = new("polyiso", rValuePerInch: 5.7,  compressiveStrengthPsi: 20.0);
    public double RValuePerInch { get; }
    public double CompressiveStrengthPsi { get; }
    // DEFINED: R_IP·(t/25.4)·0.17611 — the SI thermal resistance (m2·K/W) the seed mints into the product bag.
    public double RValueSi(double thicknessMm) => RValuePerInch * (thicknessMm / 25.4) * 0.17611;
}

// The rigid-board/sheathing facer form; FacedBoth flags a two-sided facer the layup mirrors.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Facer {
    public static readonly Facer None          = new("none",            facedBoth: false);
    public static readonly Facer Foil          = new("foil",            facedBoth: true);
    public static readonly Facer GlassFiberMat = new("glass-fiber-mat", facedBoth: true);
    public static readonly Facer CoatedGlass   = new("coated-glass",    facedBoth: true);
    public bool FacedBoth { get; }
}

// The board-fastener axis the FastenPattern carries; Welded flags a puddle/heat weld the host renders as a seam,
// AppearanceId the fastener render material the spec sheathing stage tags each fastener placement with.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerType {
    public static readonly FastenerType DrywallScrew   = new("drywall-screw",   welded: false, appearanceId: "metal.steel");     // ASTM C1002 bugle-head
    public static readonly FastenerType StructuralNail = new("structural-nail", welded: false, appearanceId: "metal.steel");     // 8d common, APA edge/field
    public static readonly FastenerType RoofingNail    = new("roofing-nail",    welded: false, appearanceId: "metal.steel");
    public static readonly FastenerType DeckWeld       = new("deck-weld",       welded: true,  appearanceId: "metal.steel");     // SDI arc-spot puddle weld
    public static readonly FastenerType DeckScrew      = new("deck-screw",      welded: false, appearanceId: "metal.steel");     // SDI self-drilling support screw
    public static readonly FastenerType PlateAndScrew  = new("plate-and-screw", welded: false, appearanceId: "metal.steel");
    public static readonly FastenerType Adhesive       = new("adhesive",        welded: false, appearanceId: "adhesive.bead");
    public static readonly FastenerType HeatWeld       = new("heat-weld",       welded: true,  appearanceId: "membrane.seam");
    public static readonly FastenerType SeamAdhesive   = new("seam-adhesive",   welded: false, appearanceId: "adhesive.bead");
    public bool Welded { get; }
    public string AppearanceId { get; }
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
// SDI rib geometry as a FROZEN printed-data row (SEED_ROW_LAW FORM law: pure standards data converts off the
// SmartEnum form). Depth/pitch/coverage and the flange columns are PUBLISHED SDI profile geometry; TopFlatMm is
// the DEFINED pitch-fraction derivation; BottomFlatMm the printed bottom-bearing flat the Corrugated arm's sixth
// named dim reads. A new profile is one row.
public readonly record struct DeckProfileRow(string Key, double RibDepthMm, double RibPitchMm, double CoverageMm, double TopFlangeFraction, double BottomFlatMm) {
    public double TopFlatMm => RibPitchMm * TopFlangeFraction;   // DEFINED: pitch x published fraction
}

public static class DeckProfiles {
    public static readonly DeckProfileRow WideRibB      = new("wide-rib-b",     38.1, 152.4, 914.4, 0.45, 25.4);   // 1.5in B, 6in pitch, 36in coverage
    public static readonly DeckProfileRow NarrowRibA    = new("narrow-rib-a",   38.1, 152.4, 914.4, 0.30, 38.1);   // 1.5in A (narrow top, wide bearing)
    public static readonly DeckProfileRow IntermediateF = new("intermediate-f", 38.1, 152.4, 914.4, 0.38, 31.8);   // 1.5in F
    public static readonly DeckProfileRow DeepN         = new("deep-n",         76.2, 203.2, 609.6, 0.40, 38.1);   // 3in N, 8in pitch, 24in coverage
    public static readonly DeckProfileRow Composite2Vli = new("composite-2vli", 50.8, 152.4, 914.4, 0.42, 44.5);   // 2in composite, steep embossed webs
    public static readonly DeckProfileRow Composite3Vli = new("composite-3vli", 76.2, 203.2, 914.4, 0.42, 50.8);   // 3in composite
    public static readonly DeckProfileRow Dovetail      = new("dovetail",       50.8, 152.4, 914.4, 0.55, 19.1);   // re-entrant acoustic/cellular
    public static readonly ImmutableArray<DeckProfileRow> Rows = [WideRibB, NarrowRibA, IntermediateF, DeepN, Composite2Vli, Composite3Vli, Dovetail];
}

// APA PRP-108 span rating as a FROZEN printed-data row: the dual roof/floor maximum support spacing. Absence is
// the roster's Option<SpanRow> column (a non-wood board carries no rating) — the prior all-zero None sentinel row
// is deleted.
public readonly record struct SpanRow(string Key, int RoofSpanIn, int FloorSpanIn) {
    public double RoofSpanMm => RoofSpanIn * 25.4;    // DEFINED
    public double FloorSpanMm => FloorSpanIn * 25.4;  // DEFINED
}

public static class SpanRatings {
    public static readonly SpanRow S24_0  = new("24/0",  24, 0);
    public static readonly SpanRow S24_16 = new("24/16", 24, 16);
    public static readonly SpanRow S32_16 = new("32/16", 32, 16);
    public static readonly SpanRow S40_20 = new("40/20", 40, 20);
    public static readonly SpanRow S48_24 = new("48/24", 48, 24);
    public static readonly ImmutableArray<SpanRow> Rows = [S24_0, S24_16, S32_16, S40_20, S48_24];
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
    public FastenerType Fastener { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double fieldSpacingMm, ref double edgeSpacingMm, ref double edgeDistanceMm, ref FastenerType fastener) =>
        validationError = fastener is not null && double.IsFinite(fieldSpacingMm) && fieldSpacingMm > 0.0 && double.IsFinite(edgeSpacingMm) && edgeSpacingMm > 0.0 && double.IsFinite(edgeDistanceMm) && edgeDistanceMm >= 0.0
            ? null
            : new ValidationError($"<fasten-schedule-invalid:field={fieldSpacingMm}:edge={edgeSpacingMm}:inset={edgeDistanceMm}>");

    public static Fin<FastenPattern> Of(double fieldMm, double edgeMm, double edgeDistMm, FastenerType fastener, Op key) =>
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
    public sealed record WoodPanel(SpanRow Span, BondClass Bond) : PanelSpecification;
    public sealed record FacedBoard(Facer Facer) : PanelSpecification;
    public sealed record DeckSheet(DeckForm Form, DeckProfileRow Rib, GaugeRow Gauge) : PanelSpecification;
    public sealed record FoamBoard(FoamType Foam, Facer Facer) : PanelSpecification;
    public sealed record Membrane : PanelSpecification;

    public bool AdmittedBy(PanelKind kind) => Switch(
        gypsumBoard: specification => (kind == PanelKind.GypsumBoard || kind == PanelKind.GypsumCeiling || kind == PanelKind.GypsumSheathing)
            && (kind == PanelKind.GypsumSheathing ? specification.Facer != Facer.None : specification.Facer == Facer.None),
        woodPanel: _ => kind == PanelKind.PlywoodSheathing || kind == PanelKind.OsbSheathing,
        facedBoard: _ => kind == PanelKind.CementBoard || kind == PanelKind.CementUnderlayment,
        deckSheet: specification => kind == PanelKind.SteelDeckFloor
            ? specification.Form.FloorDeck
            : kind == PanelKind.SteelDeckRoof && !specification.Form.FloorDeck,
        foamBoard: specification =>
            (kind == PanelKind.RigidBoardEps && specification.Foam == FoamType.Eps)
            || (kind == PanelKind.RigidBoardXps && specification.Foam == FoamType.Xps)
            || (kind == PanelKind.RigidBoardPoly && specification.Foam == FoamType.Polyiso),
        membrane: _ => kind == PanelKind.MembraneEpdm || kind == PanelKind.MembranePvc || kind == PanelKind.MembraneTpo);

    public Facer LayupFacer => Switch(
        gypsumBoard: static specification => specification.Facer,
        facedBoard: static specification => specification.Facer,
        foamBoard: static specification => specification.Facer,
        woodPanel: static _ => Facer.None, deckSheet: static _ => Facer.None, membrane: static _ => Facer.None);
    public Option<DeckSheet> Deck => Switch(
        deckSheet: static specification => Some(specification),
        gypsumBoard: static _ => None, woodPanel: static _ => None,
        facedBoard: static _ => None, foamBoard: static _ => None, membrane: static _ => None);

    public Fin<Seq<(PropertyName Name, PropertyValue Value)>> DetailRows(PositiveMagnitude thicknessMm) => Switch(
        gypsumBoard: static specification => Fin.Succ(toSeq(new Option<(PropertyName, PropertyValue)>[] {
            specification.Core == CoreType.None ? None : Some(ComponentDetail.Token(DetailSchema.CoreClass, specification.Core.Key)),
            specification.Facer == Facer.None ? None : Some(ComponentDetail.Token(DetailSchema.FacerClass, specification.Facer.Key)),
        }).Somes()),
        woodPanel: static specification => Fin.Succ(Seq(
            ComponentDetail.Token(DetailSchema.SpanRating, specification.Span.Key),
            ComponentDetail.Token(DetailSchema.BondClass, specification.Bond.Key))),
        facedBoard: static specification => Fin.Succ(Seq(
            ComponentDetail.Token(DetailSchema.FacerClass, specification.Facer.Key))),
        deckSheet: specification =>
            from depth in ComponentDetail.Measured(DetailSchema.RibDepth, Dimension.LengthDim, specification.Rib.RibDepthMm * 1e-3)
            from pitch in ComponentDetail.Measured(DetailSchema.RibPitch, Dimension.LengthDim, specification.Rib.RibPitchMm * 1e-3)
            select Seq(depth, pitch, ComponentDetail.Token(DetailSchema.DeckForm, specification.Form.Key)),
        foamBoard: specification =>
            from thermal in ComponentDetail.Measured(
                DetailSchema.ThermalResistance, Dimension.Create(0, -1, 3, 0, 1, 0, 0), specification.Foam.RValueSi(thicknessMm.Value))
            select toSeq(new Option<(PropertyName, PropertyValue)>[] {
                Some(ComponentDetail.Token(DetailSchema.FoamClass, specification.Foam.Key)),
                Some(thermal),
                specification.Facer == Facer.None ? None : Some(ComponentDetail.Token(DetailSchema.FacerClass, specification.Facer.Key)),
            }).Somes(),
        membrane: static _ => Fin.Succ(Seq<(PropertyName, PropertyValue)>()));
}

public readonly record struct PanelRow(
    string Designation, PanelKind Kind, double WidthMm, double LengthMm, double ThicknessMm,
    EdgeProfile Edge, PanelOrientation Orientation, FastenerType Fastener, double FieldMm, double EdgeMm, double EdgeDistMm,
    PanelSpecification Specification);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-built PRODUCT bag (DetailLane.Product): the five prior rows byte-identical (edge token, seam token,
// thickness, field/edge spacing — the dimension-only MeasureValue.OfSi mints preserved), the Corrugated rib
// depth/pitch rows for a deck, and the dissolved payload's product columns with no other landing — board length,
// orientation, core/span/bond/foam/facer tokens, the deck form, and the seed-computed SI thermal resistance for
// a foam board. Token/Measured/ProductRows are the relocated component#COMPONENT_DETAIL constructors.
public static class PanelDetail {
    public static Fin<PropertyBag> Of(
        PositiveMagnitude lengthMm, PositiveMagnitude thicknessMm, EdgeProfile edge, PanelOrientation orientation,
        FastenPattern fastening, PanelSpecification specification) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from field in ComponentDetail.Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, fastening.FieldSpacingMm * 1e-3)
        from edgeSpacing in ComponentDetail.Measured(DetailSchema.EdgeSpacing, Dimension.LengthDim, fastening.EdgeSpacingMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, lengthMm.Value * 1e-3)
        from payloadRows in specification.DetailRows(thicknessMm)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.EdgeProfile, edge.Key),
            ComponentDetail.Token(DetailSchema.MembraneSeam, fastening.Fastener.Key),
            thickness, field, edgeSpacing,
            length,
            ComponentDetail.Token(DetailSchema.PanelOrientation, orientation.Key),
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

    static readonly Seq<PanelRow> Roster = Seq(
        // --- gypsum board (ASTM C1396; EN 520) — tapered/square edge, drywall-screw 12in field / 8in edge
        new PanelRow("panel.gyp-reg-050-4x8",   PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-reg-038-4x8",   PanelKind.GypsumBoard,   1219.2, 2438.4, 9.5,  EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-x-050-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-x-625-54x12",   PanelKind.GypsumBoard,   1371.6, 3657.6, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.None)),
        new PanelRow("panel.gyp-c-625-4x8",     PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeCFire, Facer.None)),
        new PanelRow("panel.gyp-mr-050-4x8",    PanelKind.GypsumBoard,   1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.MoistureResistant, Facer.None)),
        new PanelRow("panel.gyp-abuse-625-4x8", PanelKind.GypsumBoard,   1219.2, 2438.4, 15.9, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.AbuseResistant, Facer.None)),
        new PanelRow("panel.gyp-025-4x8",       PanelKind.GypsumBoard,   1219.2, 2438.4, 6.4,  EdgeProfile.Square,  PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        new PanelRow("panel.gyp-ceil-050-4x8",  PanelKind.GypsumCeiling, 1219.2, 2438.4, 12.7, EdgeProfile.Tapered, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 304.8, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.Regular, Facer.None)),
        // --- gypsum sheathing (ASTM C1396 gypsum-sheathing; ASTM C1177 glass-mat) — square edge, glass-mat facer
        new PanelRow("panel.gypsheath-x-050-4x8",   PanelKind.GypsumSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-x-625-4x8",   PanelKind.GypsumSheathing, 1219.2, 2438.4, 15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.TypeXFire, Facer.GlassFiberMat)),
        new PanelRow("panel.gypsheath-gm-625-4x10", PanelKind.GypsumSheathing, 1219.2, 3048.0, 15.9, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.GlassMat, Facer.GlassFiberMat)),
        new PanelRow("panel.gyp-wr-backer-050-3x5", PanelKind.GypsumSheathing, 914.4,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.StrengthAxisPerpendicular, FastenerType.DrywallScrew, 203.2, 203.2, 9.5, new PanelSpecification.GypsumBoard(CoreType.WaterResistant, Facer.GlassFiberMat)),
        // --- plywood sheathing (APA PRP-108 / PS 1-19; EN 13986/636) — span-rated, 8d nail edge 6in / field 12in
        new PanelRow("panel.ply-rated-038-4x8-240",   PanelKind.PlywoodSheathing, 1219.2, 2438.4, 9.5,  EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1)),
        new PanelRow("panel.ply-rated-1532-4x8-2416", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1)),
        new PanelRow("panel.ply-rated-050-4x8-3216",  PanelKind.PlywoodSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1)),
        new PanelRow("panel.ply-rated-1932-4x8-4020", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 15.1, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exposure1)),
        new PanelRow("panel.ply-rated-2332-4x8-4824", PanelKind.PlywoodSheathing, 1219.2, 2438.4, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior)),
        new PanelRow("panel.ply-str1-1932-4x8",       PanelKind.PlywoodSheathing, 1219.2, 2438.4, 15.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S40_20, BondClass.Exterior)),
        new PanelRow("panel.ply-rated-075-4x8-4824",  PanelKind.PlywoodSheathing, 1219.2, 2438.4, 19.0, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exterior)),
        // --- osb sheathing (APA PRP-108 / PS 2-18; EN 13986/300)
        new PanelRow("panel.osb-rated-716-4x8-240",   PanelKind.OsbSheathing, 1219.2, 2438.4, 11.1, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_0, BondClass.Exposure1)),
        new PanelRow("panel.osb-rated-1532-4x8-2416", PanelKind.OsbSheathing, 1219.2, 2438.4, 11.9, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1)),
        new PanelRow("panel.osb-rated-050-4x8-3216",  PanelKind.OsbSheathing, 1219.2, 2438.4, 12.7, EdgeProfile.Square,       PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S32_16, BondClass.Exposure1)),
        new PanelRow("panel.osb-rated-2332-4x8-4824", PanelKind.OsbSheathing, 1219.2, 2438.4, 18.3, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S48_24, BondClass.Exposure1)),
        new PanelRow("panel.osb-rated-1532-4x24",     PanelKind.OsbSheathing, 1219.2, 7315.2, 11.9, EdgeProfile.TongueGroove, PanelOrientation.StrengthAxisPerpendicular, FastenerType.StructuralNail, 304.8, 152.4, 9.5, new PanelSpecification.WoodPanel(SpanRatings.S24_16, BondClass.Exposure1)),
        // --- cement board (ASTM C1325; ANSI A118.9) — glass-mesh scrim, edge-dist 3/4in
        new PanelRow("panel.cbu-025-3x5",          PanelKind.CementBoard,        914.4,  1524.0, 6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-3x5",          PanelKind.CementBoard,        914.4,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-4x8",          PanelKind.CementBoard,        1219.2, 2438.4, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-625-4x8",          PanelKind.CementBoard,        1219.2, 2438.4, 15.9, EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-050-32x60",        PanelKind.CementBoard,        812.8,  1524.0, 12.7, EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        new PanelRow("panel.cbu-underlay-014-3x5", PanelKind.CementUnderlayment, 914.4,  1524.0, 6.4,  EdgeProfile.Square, PanelOrientation.Unidirectional, FastenerType.RoofingNail, 203.2, 203.2, 19.0, new PanelSpecification.FacedBoard(Facer.GlassFiberMat)),
        // --- steel deck (ANSI/SDI RD-2017 roof; C-2017 composite) — coverage/pitch/depth from the referenced DeckProfiles row, base metal from the connector GaugeRow key
        new PanelRow("panel.deck-b-22ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-b-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.WideRibB, Gauges.Ga20)),
        new PanelRow("panel.deck-a-20ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.NarrowRibA, Gauges.Ga20)),
        new PanelRow("panel.deck-f-18ga-roof", PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.IntermediateF, Gauges.Ga18)),
        new PanelRow("panel.deck-n-18ga-roof", PanelKind.SteelDeckRoof,  609.6, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Roof, DeckProfiles.DeepN, Gauges.Ga18)),
        new PanelRow("panel.deck-bform-22ga",  PanelKind.SteelDeckRoof,  914.4, 6096.0, 38.1, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckScrew, 304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Form, DeckProfiles.WideRibB, Gauges.Ga22)),
        new PanelRow("panel.deck-2vli-18ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 50.8, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite2Vli, Gauges.Ga18)),
        new PanelRow("panel.deck-3vli-16ga",   PanelKind.SteelDeckFloor, 914.4, 9144.0, 76.2, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Composite3Vli, Gauges.Ga16)),
        new PanelRow("panel.deck-dt-16ga",     PanelKind.SteelDeckFloor, 914.4, 9144.0, 50.8, EdgeProfile.SideLapInterlock, PanelOrientation.StrengthAxisParallel, FastenerType.DeckWeld,  304.8, 304.8, 0.0, new PanelSpecification.DeckSheet(DeckForm.Composite, DeckProfiles.Dovetail, Gauges.Ga16)),
        // --- rigid-board insulation (ASTM C578 EPS/XPS; C1289 polyiso)
        new PanelRow("panel.eps-1in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-2in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 50.8,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.eps-4in-4x8",      PanelKind.RigidBoardEps,  1219.2, 2438.4, 101.6, EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, FastenerType.Adhesive,      406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Eps, Facer.None)),
        new PanelRow("panel.xps-1in-2x8",      PanelKind.RigidBoardXps,  609.6,  2438.4, 25.4,  EdgeProfile.Shiplap,      PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.xps-2in-2x8",      PanelKind.RigidBoardXps,  609.6,  2438.4, 50.8,  EdgeProfile.TongueGroove, PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Xps, Facer.None)),
        new PanelRow("panel.polyiso-1in-4x8",  PanelKind.RigidBoardPoly, 1219.2, 2438.4, 25.4,  EdgeProfile.Square,       PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.GlassFiberMat)),
        new PanelRow("panel.polyiso-2in-foil", PanelKind.RigidBoardPoly, 1219.2, 2438.4, 50.8,  EdgeProfile.Square,       PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.Foil)),
        new PanelRow("panel.polyiso-3in-4x8",  PanelKind.RigidBoardPoly, 1219.2, 2438.4, 76.2,  EdgeProfile.Square,       PanelOrientation.Unidirectional, FastenerType.PlateAndScrew, 406.4, 406.4, 0.0, new PanelSpecification.FoamBoard(FoamType.Polyiso, Facer.CoatedGlass)),
        // --- single-ply roof membranes — roof/wall/floor remains a spec layout role; panel is the product form
        new PanelRow("panel.epdm-060-roll", PanelKind.MembraneEpdm, 3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, FastenerType.SeamAdhesive, 304.8, 152.4, 0.0, new PanelSpecification.Membrane()),
        new PanelRow("panel.pvc-060-roll",  PanelKind.MembranePvc,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, FastenerType.HeatWeld,     304.8, 152.4, 0.0, new PanelSpecification.Membrane()),
        new PanelRow("panel.tpo-060-roll",  PanelKind.MembraneTpo,  3048.0, 30480.0, 1.52, EdgeProfile.LappedSeam, PanelOrientation.Unidirectional, FastenerType.HeatWeld,     304.8, 152.4, 0.0, new PanelSpecification.Membrane()));

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
                from aligned in guard(thickness.Value == deck.Rib.RibDepthMm && width.Value == deck.Rib.CoverageMm,
                    ComponentFault.Dimension(key, $"<deck-row-dims-drift:{r.Designation}>"))
                from corrugated in SectionProfile.Corrugated.Of(
                    coverWidthMm: deck.Rib.CoverageMm, ribDepthMm: deck.Rib.RibDepthMm, ribPitchMm: deck.Rib.RibPitchMm,
                    gaugeMm: deck.Gauge.BaseThicknessMm, topFlatMm: deck.Rib.TopFlatMm, bottomFlatMm: deck.Rib.BottomFlatMm, key)
                select (corrugated, deck.Gauge.Substance),
            None: () =>
                from layered in SectionProfile.Layered.Of(r.Kind.Layup(r.Specification.LayupFacer, thickness), overallMm: thickness.Value, widthMm: width.Value, key)
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
            from admitted in guard(r.Specification.AdmittedBy(r.Kind),
                ComponentFault.Family(context.Key, $"<panel-kind-spec-mismatch:{r.Designation}:{r.Kind.Key}>"))
            from width in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.WidthMm)
            from length in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
            from thickness in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
            from fastening in FastenPattern.Of(r.FieldMm, r.EdgeMm, r.EdgeDistMm, r.Fastener, context.Key)
            from routed in ProfileOf(r, width, thickness, context.Key)
            from detail in PanelDetail.Of(length, thickness, r.Edge, r.Orientation, fastening, r.Specification)
            from item in Component.Of(
                ComponentFamily.Panel, r.Designation, routed.Profile,
                IfcBinding.Of(r.Kind.IfcEntity, r.Kind.IfcPredefinedType),
                Coring.None, new ComponentStandard("us", StandardJointThicknessMm: 0.0, r.Kind.Authority),
                substanceId: routed.Substance,
                appearanceId: AppearanceOf(routed.Profile, routed.Substance),
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, Sectioned: r.Specification.Deck.IsSome)).As();
}
```

## [03]-[RESEARCH]

- [SEED_FOLD_RAIL]: REALIZED — `PanelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` is ONE `Traverse`; the prior THREE `.Choose(...ToOption())` swallow sites (`Shapes`, `BuildPanelRows`, `PanelSections`) are DELETED. The connector `Gauges` roster carries the thin `Ga22`/`Ga20` deck bands, and every deck row references its `GaugeRow` SYMBOLICALLY — the prior string-keyed gauge column, its runtime `Find`, and the `<unknown-gauge>` fault case are all deleted, so no gauge read can miss and no roster residual remains. The deck-rib section-solve moves out of the seed entirely: `ComponentCatalogue.Of` traverses `SectionSolver.Solve` over the `Sectioned` rows, so a degenerate rib aborts the catalogue build loud, in one place, for every family.
- [PAYLOAD_DISSOLUTION]: REALIZED — `PanelSection`, `PanelShape`, and `DeckRib` are DELETED with zero column loss: board geometry rides `SectionProfile.Layered(Plies, OverallMm, WidthMm)` (the `GrossRectangleMm` (width, thickness) pair value-identical to the prior projection; the laminate sum guard holds by construction), deck geometry rides the six `SectionProfile.Corrugated` named dims (`CoverWidthMm`/`RibDepthMm`/`RibPitchMm`/`GaugeMm`/`TopFlatMm`/`BottomFlatMm` filled from `DeckProfiles` + `GaugeRow.BaseThicknessMm` — the prior per-row `BaseMetalMm` column deleted as a duplicated spelling of the gauge row's own base-metal fact), the vocabulary columns ride the kept SmartEnums, and the product columns with no other landing (board length, orientation, core/span/bond/foam/facer, deck form, the seed-computed SI thermal resistance) ride the seed-built `PanelDetail` bag; `ToUnit`/`CrossNominalMm`/`GrossRectangleMm`/`SubstanceId`/`AppearanceId`/`RValue` projections are subsumed by the family `crossNominal` delegate, the profile base-constructor gross facts, `ProfileOf`'s substance routing, `AppearanceOf`'s facing-ply probe, and `FoamType.RValueSi`.
- [DECK_SECTION_RELOCATION]: CROSS-FILE — the bespoke `DeckSection.RibSection`/`RibPerimeter`/`RibPlastics`/`Scale`/`Band` machinery DISSOLVES into the ONE `component#SECTION_SOLVER` `corrugated` arm: `Curves.Deck` builds the per-pitch thin-band trapezoid `Perimeter` (centre-line offset by `GaugeMm/2`, the bottom flat now first-class), `Forms.ThinFold` supplies the thin-walled plastic moduli, the open-section `J = Σ b·t³/3`, the sloped-web shear areas, and the per-coverage scaling (`ribs = CoverWidthMm/RibPitchMm` — the deleted `RibCountPerCoverage`), and `SectionSolver.Admit` lifts the twenty columns once for all arms; the `.Utility.Parts` `EllipseQuarterPart` additionally admits exact curved-corner integration for the `dovetail` re-entrant profile the straight-only `TrapezoidalPart` band approximates. Panel keeps only the DATA those dims read.
- [FORM_LAW_CONVERSIONS]: REALIZED — `DeckProfile` (7 rows) and `SpanRating` (5 rows + the deleted all-zero `None` sentinel, absence now `Option<SpanRow>`) convert 1:1 to frozen `readonly record struct` row tables with identical printed columns per `SEED_ROW_LAW`; `DeckProfileRow` gains the `BottomFlatMm` PUBLISHED column the `Corrugated` sixth dim requires (values geometrically consistent with SDI web slopes — verify against the SDI catalog figures before realization) and the DEFINED `TopFlatMm` pitch-fraction derivation; `SpanRow` gains the DEFINED `RoofSpanMm`/`FloorSpanMm` SI reads. The kept vocabularies stack `[KeyMemberComparer]` beside `[KeyMemberEqualityComparer]` per the campaign row convention.
- [GENERATED_ADMISSION_AND_SYMBOLIC_ROSTER]: `FastenPattern` admits the schedule and derives endpoint-inclusive station counts. `PanelSpecification` collapses the flat optional/default tail into disjoint product cases, while every case references policy and standards rows symbolically. `DeckProfiles`, `SpanRatings`, and `Gauges` remain named row tables with no runtime key resolution.
- [LAYUP_POLICY_ROWS]: `PanelKind.Layup` emits the shared `Ply` currency with a bounded `PlyRole`; every facer maps to its canonical facing/core role pair, and deck kinds emit no plies because their structural composition is a `ProfileSet`.
- [KIND_COVERAGE]: EXTENDED — `GypsumCeiling` (`IfcCovering`/CEILING, the ASTM C1396 ceiling board) and `CementUnderlayment` (`IfcCovering`/FLOORING, the 1/4in underlayment) land as kind + seed rows, closing the two leaves the prior prose promised but never seeded; the tri-entity spread facts stand assay-confirmed (`IfcCoveringTypeEnum` has NO SHEATHING member; `IfcSlabTypeEnum` has NO COMPOSITE/DECK member; `GeometryGym.Ifc.IfcDeck` does NOT exist). `ComponentAuthority` rides the KIND (the owner's `Apa`/`Sdi` rows — authored for wood panels and steel deck — now composed instead of one blended `Astm` standard). `panel.deck-dt-16ga` (the re-entrant dovetail composite) and `panel.gyp-wr-backer-050-3x5` (the ASTM C1178 water-resistant tile backer) instantiate the previously DEAD `DeckProfiles.Dovetail` row and `CoreType.WaterResistant` band as data — the dovetail row is the `EllipseQuarterPart` curved-corner arm's exercising datum, so the [DECK_SECTION_RELOCATION] claim carries a live row, never dead vocabulary.
- [DETAIL_SCHEMA_ROWS]: REALIZED — `PanelDetail` preserves the five prior product rows byte-identical plus the conditional rib rows, and composes NEW `DetailSchema` `PropertyName` rows (`BoardLength`, `PanelOrientation`, `CoreClass`, `SpanRating`, `BondClass`, `FoamClass`, `FacerClass`, `ThermalResistance`, `DeckForm`) now declared on the seam `DetailSchema.Product` vocabulary (Element `property#DETAIL_SCHEMA`), reconciled with the `component.md` `[COMPONENT_DETAIL]` relocation.
- [DECK_BOTTOM_FLAT_VERIFY]: RESEARCH — the `DeckProfileRow.BottomFlatMm` authored values (`25.4`/`38.1`/`31.8`/`38.1`/`44.5`/`50.8`/`19.1`) sit within the SDI web-slope band but await verification against the printed SDI RD-2017/C-2017 profile figures before realization (no machine-readable SDI profile source is admitted, so the values stay `AUTHORED`).
