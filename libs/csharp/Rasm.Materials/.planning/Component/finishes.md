# [MATERIALS_FINISHES]

THE FINISHES SEED PAGE owns TWO family rows, split by lane law. The `finish` family (`ComponentClass.Minor`, `DetailLane.Product`, default `IfcBinding.Of("IfcCovering", "FLOORING")` — every kind row overriding its own Ifc2X3-valid covering leaf) carries the architectural finish products: paint systems, ceramic tile, resilient flooring, carpet tile, acoustical ceiling tile with its suspension-grid duty, and stone cladding. The `fireproofing` family (`ComponentClass.Minor`, `DetailLane.Realization`, `IfcBinding.Named("IfcCovering", "Fireproofing")` — no published covering token names fire protection) carries SFRM and intumescent coating. THE FIREPROOFING HOME LAW: `DetailSchema.FireproofingThickness`/`RatingMinutes`/`DensityClass` are Element-declared REALIZATION rows and a family holds ONE `DetailLane`, so a Product-lane seat stamps realization facts through a product bag — the distinct Realization-lane family is the only honest seat. THE INTUMESCENT SPLIT: `PaintBinder` excludes the intumescent binder — an intumescent coat is fire protection with a listing-owned thickness, never a finish, so the fireproofing family owns it whole. THE CLADDING HOME LAW: a stone cladding panel is a laid Product whose cross-section is a monolithic `SectionProfile.Rectangle` — the panel family admits only ply-stacked `Layered` and `Corrugated` sheet goods, so granite and marble cladding seed here; the anchors a stone panel hangs on are `fastener` family rows — the `fastener#FASTENER_FAMILY` `kerf`/`pin` kinds under the `AnchorRole` body/restraint axis — and this page names no anchor geometry.

`FinishKind` binds each product to its covering leaf, substance, authority, and admissible payload; `FinishSpecification` keys the closed per-product payload; `FinishInstall` carries the bedded/laid attachment token — `PanelFastening` stays the stationed board-fastening owner, and a thin-set bed has no station schedule, so the two axes are method-versus-schedule, never siblings. `GridDuty` carries the ASTM C635 direct-hung main-runner classes; `BondTier` the IBC high-rise SFRM bond ladder over ASTM E736. `FinishRows` mints the producer-scoped row names the Element schema does not declare, through the owner-blessed `PropertyCategory.Materials.Row` scope. Substance physics read ONCE from the property library by `SubstanceId` — `coating.paint`/`ceramic.tile`/`flooring.resilient`/`flooring.carpet`/`ceiling.mineral`/`stone.granite`/`stone.marble`/`fireproofing.sfrm`/`fireproofing.intumescent`; neither family publishes a structural resistance, so both capacity producers are typed refusals.

## [01]-[INDEX]

- [02]-[FINISH_FAMILY]: `PaintBinder`, `TileClass`, `ResilientClass`, `CeilingType`/`CeilingForm`, `GridDuty`, `StoneFinish`, `FinishInstall`, `FinishKind`, the `FinishSpecification` union, `FinishRows`, `FinishRow`, `FinishDetail`, and `FinishSeed` (the `Rows` generator, the module-routed `ProfileOf`, and the `Capacity` refusal).
- [03]-[FIREPROOFING_FAMILY]: `SfrmDensityClass`, `BondTier`, `FireproofingKind`, the `FireproofingSpecification` union, `FireproofingRow`, `FireproofingDetail`, and `FireproofingSeed` (the `Rows` generator and the `Capacity` refusal).

## [02]-[FINISH_FAMILY]

- Owner: `FinishKind` the product-kind axis with per-row IFC leaf, substance, authority, and payload admission; `FinishSpecification` the closed payload union; the class vocabularies carry their published bounds as row columns; `FinishSeed` the ONE generator.
- Cases: kind {paint, floor tile, wall tile, LVT, VCT, carpet tile, ceiling tile, granite cladding, marble cladding}; tile class {impervious ≤0.5 %, vitreous ≤3 %, semi-vitreous ≤7 %, non-vitreous unbounded — interior walls only} per ANSI A137.1 absorption over the ASTM C373 test; resilient class {F1700 I/II/III over monolithic/surface-decorated/printed-film, F1066 1/2/3 over solid/through-pattern/surface-pattern} — one axis, the `Spec` column the standard discriminant; ceiling {Type III/IV/XII × Form 1/2, the two-sourced ASTM E1264 members}; grid duty {light 5.0, intermediate 12.0, heavy 16.0 lbf/ft direct-hung}.
- Entry: `FinishSeed.Rows(Context)` traverses the roster through kind/payload admission, the module/coat coherence proof, module-routed profile construction, and the detail-bag fold onto the railed `Component.Of`; one malformed row aborts the catalogue.
- Packages: Rasm.Numerics (`PositiveMagnitude`), Rasm.Domain (`Op`/`Context`/`AcceptValidated`), Rasm.Element (`MaterialId`, `PropertyBag`, `DetailSchema`/`PropertyCategory`/`PropertyName`/`PropertyValue`/`Dimension`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` admission, `[Union]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`); NO external finish producer exists — the roster is `AUTHORED` module policy under `SEED_ROW_LAW`, the class tables PUBLISHED transcriptions.
- Growth: a new finish product is one `FinishRow`; a new kind one `FinishKind` row binding leaf, substance, and admission; a new tile/resilient/ceiling class or grid duty one vocabulary row; a proven E1264 Type or Form member one row — the full roster stays paywalled and lands member-by-member with proof; a proven NRC/CAC roster lands as measured rows on a product table, never as class columns, because ASTM E1264 declares both per product.
- Boundary: the wet-DCOF floor is `TileClass.WetDcofFloor` (ANSI A326.3, ≥ 0.42 for level interior floors walked on wet) — a placement gate the spec finish stage reads, never a per-row column, since the standard fixes the floor and products declare their own measured DCOF. The F1913 unbacked-sheet numerics and the C635 indirect-hung/furring rows each reached one source and stay absent. Grid duty converts its published plf ONCE through the `panel#PANEL_FAMILY` `LateralShear.PlfToKnPerM` constant — one conversion owner. Stone rows carry geometry and surface finish alone: the kerf/pin/dowel anchor systems a stone panel references are `fastener` family rows (`fastener#FASTENER_FAMILY` owns the `kerf`/`pin` kinds, the `AnchorRole` axis, and the `AnchorType` stamp from the anchor's own seat), and a thickness class here is AUTHORED estate policy until a published ladder proves out.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                   // Fin, Option, Seq, Traverse
using Rasm.Numerics;                  // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                   // Op, Context, AcceptValidated
using Rasm.Element.Composition;      // MaterialId, PropertyBag
using Rasm.Element.Properties;       // DetailSchema, PropertyCategory, PropertyName, PropertyValue, Dimension
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using Thinktecture;                  // [SmartEnum]/[Union]/[UseDelegateFromConstructor]/[KeyMemberEqualityComparer]/[KeyMemberComparer]
using static LanguageExt.Prelude;

// Every family seed declares in the ONE Rasm.Materials.Component namespace (component#COMPONENT_OWNER); the owner
// folds FinishSeed.Rows and FireproofingSeed.Rows through their policy rows, never by name. This ONE page seeds the
// two families because they share the covering domain while splitting on lane — the eventual Finishes.cs carries both.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The paint binder-class axis. The intumescent binder is EXCLUDED by the fireproofing split: an intumescent coat's
// thickness is listing-owned fire protection, so it is a FireproofingKind, and a row minted here would fork one
// product across two lanes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaintBinder {
    public static readonly PaintBinder Acrylic  = new("acrylic");
    public static readonly PaintBinder Alkyd    = new("alkyd");
    public static readonly PaintBinder Epoxy    = new("epoxy");
    public static readonly PaintBinder Urethane = new("urethane");
}

// ANSI A137.1 water-absorption classes over the ASTM C373 test (percent of dry weight). The ceiling is an Option
// because non-vitreous is UNBOUNDED above 7 % — a fabricated cap would gate products the class admits. WetAreas
// states the standard's own service bound: a non-vitreous body serves interior walls only, never wet areas.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileClass {
    // ANSI A326.3 wet dynamic coefficient of friction floor for level interior floors walked on when wet — the
    // standard fixes the FLOOR and each product declares its own measured DCOF, so the gate is a constant the spec
    // finish stage compares against a declared value, never a per-row column.
    public const double WetDcofFloor = 0.42;
    public static readonly TileClass Impervious   = new("impervious",    absorptionCeilingPct: Some(0.5), wetAreas: true);    // porcelain body — the PTCA certification basis
    public static readonly TileClass Vitreous     = new("vitreous",      absorptionCeilingPct: Some(3.0), wetAreas: true);
    public static readonly TileClass SemiVitreous = new("semi-vitreous", absorptionCeilingPct: Some(7.0), wetAreas: true);
    public static readonly TileClass NonVitreous  = new("non-vitreous",  absorptionCeilingPct: None,      wetAreas: false);
    public Option<double> AbsorptionCeilingPct { get; }
    public bool WetAreas { get; }
}

// The resilient-flooring class axis over BOTH live ASTM specs on one identity regime — the Spec column is the
// standard discriminant, so F1700 (solid vinyl, Class III printed film the product the market names LVT) and F1066
// (vinyl composition tile) stay one axis rather than sibling enums. WearLayerFloorMm is the F1700 commercial
// wear-layer minimum (0.020 in), carried on the F1700 rows alone; F1066 publishes no wear-layer column. The F1913
// unbacked-sheet numerics reached one source and stay off this axis until proven.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResilientClass {
    public static readonly ResilientClass F1700Monolithic  = new("f1700-i",   spec: "F1700", wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700Decorated   = new("f1700-ii",  spec: "F1700", wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700PrintedFilm = new("f1700-iii", spec: "F1700", wearLayerFloorMm: Some(0.50));   // LVT — Class III wear-layer binder ≥ 90 %
    public static readonly ResilientClass F1066Solid       = new("f1066-1",   spec: "F1066", wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Through     = new("f1066-2",   spec: "F1066", wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Surface     = new("f1066-3",   spec: "F1066", wearLayerFloorMm: None);
    public string Spec { get; }
    public Option<double> WearLayerFloorMm { get; }
}

// ASTM E1264 acoustical-panel Type — the TWO-SOURCED members alone; the full Type I–XX roster stays paywalled and
// grows member-by-member with proof. NRC and CAC are product-declared under the classification, never standard-fixed,
// so they land as measured product rows when a roster proves them — a class column here would fabricate declarations.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingType {
    public static readonly CeilingType MineralPainted    = new("type-iii", faced: false);   // mineral base, painted finish
    public static readonly CeilingType MineralMembrane   = new("type-iv",  faced: true);    // mineral base, membrane-faced
    public static readonly CeilingType GlassFibreMembrane = new("type-xii", faced: true);   // glass-fibre base, membrane-faced
    public bool Faced { get; }
}

// ASTM E1264 Form — manufacture route; the two-sourced members.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingForm {
    public static readonly CeilingForm Nodular     = new("form-1");
    public static readonly CeilingForm WaterFelted = new("form-2");
}

// ASTM C635 suspension-grid structural duty: the main runner's simple-span capacity over the 4 ft span at L/360,
// tested per ASTM E3090 — the three PUBLISHED direct-hung classes. The indirect-hung and furring-bar rows reached
// one source and stay off the axis. ForSdc is the ASTM E580 seismic coupling: SDC C requires intermediate duty,
// SDC D–F heavy.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridDuty {
    public static readonly GridDuty Light        = new("light",        mainRunnerPlf: 5.0);
    public static readonly GridDuty Intermediate = new("intermediate", mainRunnerPlf: 12.0);
    public static readonly GridDuty Heavy        = new("heavy",        mainRunnerPlf: 16.0);
    public double MainRunnerPlf { get; }
    // DEFINED: the published plf through the ONE plf conversion owner — panel's lateral-shear constant, so the two
    // published-imperial tables convert on one factor.
    public double MainRunnerKnPerM => MainRunnerPlf * LateralShear.PlfToKnPerM;

    // Total over the closed A–F category vocabulary and ABSENT past it: A/B carry no E580 grid requirement (the
    // light class is the published floor), and an unrecognized token answers None rather than silently earning the
    // lightest duty — a catch-all Light over a free string certified garbage as a seismic selection.
    public static Option<GridDuty> ForSdc(string sdc) => sdc switch {
        "A" or "B" => Some(Light),
        "C" => Some(Intermediate),
        "D" or "E" or "F" => Some(Heavy),
        _ => None,
    };
}

// The dimension-stone surface finish vocabulary — a fabrication token the appearance and slip posture read.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoneFinish {
    public static readonly StoneFinish Polished = new("polished");
    public static readonly StoneFinish Honed    = new("honed");
    public static readonly StoneFinish Flamed   = new("flamed");
}

// The bedded/laid attachment axis the DetailSchema.FasteningMethod token stamps from. PanelFastening stays the
// stationed board-fastening owner — a thin-set bed, a lay-in panel, and a brushed coat carry no station schedule,
// so this axis is METHOD where that one is SCHEDULE, never a sibling spelling of one concept.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FinishInstall {
    public static readonly FinishInstall ThinSet  = new("thin-set");
    public static readonly FinishInstall Adhesive = new("adhesive");
    public static readonly FinishInstall LayIn    = new("lay-in");
    public static readonly FinishInstall Anchored = new("anchored");
    public static readonly FinishInstall Coated   = new("coated");
}

// The finish product-kind axis: each row carries its GeometryGym-verified Ifc2X3 covering leaf, its substance key,
// its standards body, and its payload admission — a new kind declares its own admissible payload where it declares
// everything else about itself. Tile splits floor from wall on the covering token; stone splits granite from marble
// on the substance. The paint leaf rides USERDEFINED: IfcCoveringTypeEnum publishes no coating token, so the
// object-type discriminator carries the product name per the component IfcBinding law.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FinishKind {
    public static readonly FinishKind Paint       = new("paint",        ifc: IfcBinding.Named("IfcCovering", "PaintSystem"), substanceId: "coating.paint",       authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Paint);
    public static readonly FinishKind TileFloor   = new("tile-floor",   ifc: IfcBinding.Of("IfcCovering", "FLOORING"),       substanceId: "ceramic.tile",        authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Tile);
    public static readonly FinishKind TileWall    = new("tile-wall",    ifc: IfcBinding.Of("IfcCovering", "CLADDING"),       substanceId: "ceramic.tile",        authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Tile);
    public static readonly FinishKind Resilient   = new("resilient",    ifc: IfcBinding.Of("IfcCovering", "FLOORING"),       substanceId: "flooring.resilient",  authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Resilient);
    public static readonly FinishKind CarpetTile  = new("carpet-tile",  ifc: IfcBinding.Of("IfcCovering", "FLOORING"),       substanceId: "flooring.carpet",     authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Carpet);
    public static readonly FinishKind CeilingTile = new("ceiling-tile", ifc: IfcBinding.Of("IfcCovering", "CEILING"),        substanceId: "ceiling.mineral",     authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Ceiling);
    public static readonly FinishKind Granite     = new("granite",      ifc: IfcBinding.Of("IfcCovering", "CLADDING"),       substanceId: "stone.granite",       authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Stone);
    public static readonly FinishKind Marble      = new("marble",       ifc: IfcBinding.Of("IfcCovering", "CLADDING"),       substanceId: "stone.marble",        authority: ComponentAuthority.Astm, admits: static s => s is FinishSpecification.Stone);
    public IfcBinding Ifc { get; }
    public string SubstanceId { get; }
    public ComponentAuthority Authority { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    [UseDelegateFromConstructor]
    public partial bool Admits(FinishSpecification specification);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Product payload as a closed family: each case carries only the axes its product form admits, so a paint row
// carries no tile class and a carpet row carries nothing but its module.
[Union]
public abstract partial record FinishSpecification {
    private FinishSpecification() { }
    public sealed record Paint(PaintBinder Binder) : FinishSpecification;
    public sealed record Tile(TileClass Class) : FinishSpecification;
    public sealed record Resilient(ResilientClass Class, bool Embossed) : FinishSpecification;
    public sealed record Carpet : FinishSpecification;
    public sealed record Ceiling(CeilingType Type, CeilingForm Form, GridDuty Grid) : FinishSpecification;
    public sealed record Stone(StoneFinish Finish) : FinishSpecification;
}

// ModuleMm is Some exactly where the product is a LAID module (a cut tile, board, or slab with a width × length),
// None for an applied coat whose only dimension is its dry-film build — the insulation ExtentMm posture, so a coat
// can never carry a fabricated module and a laid product can never omit its real one; the generator proves the
// correspondence against the payload before profile routing.
public readonly record struct FinishRow(
    string Designation, FinishKind Kind, Option<(double WidthMm, double LengthMm)> ModuleMm, double ThicknessMm,
    FinishInstall Install, FinishSpecification Specification) {
    // Module dimensions are estate policy unless a row states a published basis — the VCT gauge row overrides.
    public Provenance Source { get; init; } = Provenance.Authored;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The producer-scoped row names the Element schema does not declare, minted ONCE through the owner-blessed
// PropertyCategory.Materials scope (the property page's declared growth arm for producer-local row families) — a
// call-site Row mint per stamp would fork the spelling per arm.
public static class FinishRows {
    public static readonly PropertyName AbsorptionClass = PropertyCategory.Materials.Row(nameof(AbsorptionClass));
    public static readonly PropertyName BinderClass     = PropertyCategory.Materials.Row(nameof(BinderClass));
    public static readonly PropertyName CeilingClass    = PropertyCategory.Materials.Row(nameof(CeilingClass));
    public static readonly PropertyName CeilingForm     = PropertyCategory.Materials.Row(nameof(CeilingForm));
    public static readonly PropertyName GridDuty        = PropertyCategory.Materials.Row(nameof(GridDuty));
    public static readonly PropertyName ResilientClass  = PropertyCategory.Materials.Row(nameof(ResilientClass));
    public static readonly PropertyName StoneFinish     = PropertyCategory.Materials.Row(nameof(StoneFinish));
}

// The seed-built PRODUCT bag: the FasteningMethod install token, thickness, the module length WHERE a module
// exists (a coat has no BoardLength to state — absence, never a placeholder row), and the dissolved payload's
// class tokens — each spec arm stamping its own axes and nothing else.
public static class FinishDetail {
    public static Fin<PropertyBag> Of(FinishRow row, PositiveMagnitude thicknessMm, Op key) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from length in row.ModuleMm.Match(
            Some: module => ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, module.LengthMm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.ProductRows([
            ComponentDetail.Sourced(row.Source),
            ComponentDetail.Token(DetailSchema.FasteningMethod, row.Install.Key),
            thickness,
            .. length.ToSeq(),
            .. PayloadRows(row.Specification),
        ]);

    static Seq<(PropertyName Name, PropertyValue Value)> PayloadRows(FinishSpecification specification) => specification.Switch(
        paint: static p => Seq(ComponentDetail.Token(FinishRows.BinderClass, p.Binder.Key)),
        tile: static t => Seq(ComponentDetail.Token(FinishRows.AbsorptionClass, t.Class.Key)),
        resilient: static r => Seq(ComponentDetail.Token(FinishRows.ResilientClass, r.Class.Key)),
        carpet: static _ => Seq<(PropertyName, PropertyValue)>(),
        ceiling: static c => Seq(
            ComponentDetail.Token(FinishRows.CeilingClass, c.Type.Key),
            ComponentDetail.Token(FinishRows.CeilingForm, c.Form.Key),
            ComponentDetail.Token(FinishRows.GridDuty, c.Grid.Key)),
        stone: static s => Seq(ComponentDetail.Token(FinishRows.StoneFinish, s.Finish.Key)));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED finish roster: metric tile/board modules, the imperial-module rows carrying their inch basis in the
// designation. The VCT row alone states Published — its 12 × 12 in × 1/8 in gauge transcribes the two-sourced
// spec-practice convention; every other module is estate policy. Stone thickness classes (30 mm exterior granite,
// 20 mm interior marble) are AUTHORED until a published ladder proves out.
public static class FinishSeed {
    static readonly Seq<FinishRow> Roster = Seq(
        // --- paint systems — one row per binder class, module NONE: the Nominal profile is the AUTHORED reference
        //     dry-film build (0.1 mm architectural, 0.3 mm high-build epoxy), the coat's ONE dimension
        new FinishRow("finish.paint-acrylic",  FinishKind.Paint, None, 0.1, FinishInstall.Coated, new FinishSpecification.Paint(PaintBinder.Acrylic)),
        new FinishRow("finish.paint-alkyd",    FinishKind.Paint, None, 0.1, FinishInstall.Coated, new FinishSpecification.Paint(PaintBinder.Alkyd)),
        new FinishRow("finish.paint-epoxy",    FinishKind.Paint, None, 0.3, FinishInstall.Coated, new FinishSpecification.Paint(PaintBinder.Epoxy)),
        new FinishRow("finish.paint-urethane", FinishKind.Paint, None, 0.1, FinishInstall.Coated, new FinishSpecification.Paint(PaintBinder.Urethane)),
        // --- ceramic tile (ANSI A137.1 classes over ASTM C373)
        new FinishRow("finish.tile-porcelain-600",  FinishKind.TileFloor, Some((600.0, 600.0)), 10.0, FinishInstall.ThinSet, new FinishSpecification.Tile(TileClass.Impervious)),
        new FinishRow("finish.tile-porcelain-300",  FinishKind.TileFloor, Some((300.0, 300.0)), 9.0,  FinishInstall.ThinSet, new FinishSpecification.Tile(TileClass.Impervious)),
        new FinishRow("finish.tile-wall-200",       FinishKind.TileWall,  Some((200.0, 200.0)), 8.0,  FinishInstall.ThinSet, new FinishSpecification.Tile(TileClass.NonVitreous)),
        // --- resilient flooring (ASTM F1700 LVT; ASTM F1066 VCT)
        new FinishRow("finish.lvt-iii-152x1219", FinishKind.Resilient, Some((152.4, 1219.2)), 5.0, FinishInstall.Adhesive, new FinishSpecification.Resilient(ResilientClass.F1700PrintedFilm, Embossed: true)),
        new FinishRow("finish.vct-305",          FinishKind.Resilient, Some((304.8, 304.8)),  3.2, FinishInstall.Adhesive, new FinishSpecification.Resilient(ResilientClass.F1066Solid, Embossed: false)) { Source = Provenance.Published },
        // --- carpet tile
        new FinishRow("finish.carpet-tile-610", FinishKind.CarpetTile, Some((609.6, 609.6)), 6.4, FinishInstall.Adhesive, new FinishSpecification.Carpet()),
        // --- acoustical ceiling tile (ASTM E1264 classification; ASTM C635 grid duty)
        new FinishRow("finish.ceil-mineral-610x610",  FinishKind.CeilingTile, Some((609.6, 609.6)),  15.9, FinishInstall.LayIn, new FinishSpecification.Ceiling(CeilingType.MineralPainted, CeilingForm.WaterFelted, GridDuty.Intermediate)),
        new FinishRow("finish.ceil-mineral-610x1219", FinishKind.CeilingTile, Some((609.6, 1219.2)), 15.9, FinishInstall.LayIn, new FinishSpecification.Ceiling(CeilingType.MineralPainted, CeilingForm.WaterFelted, GridDuty.Intermediate)),
        // --- stone cladding — anchors are fastener family rows (kerf/pin); this roster carries slab geometry and finish alone
        new FinishRow("finish.stone-granite-30", FinishKind.Granite, Some((1219.2, 609.6)), 30.0, FinishInstall.Anchored, new FinishSpecification.Stone(StoneFinish.Flamed)),
        new FinishRow("finish.stone-marble-20",  FinishKind.Marble,  Some((914.4, 609.6)),  20.0, FinishInstall.Anchored, new FinishSpecification.Stone(StoneFinish.Honed)));

    // The module-routed profile: a coat row proves its module ABSENCE and lands the Nominal dry-film build — an
    // applied coat has no module to rectangle — and every laid product proves its module and lands the Rectangle
    // cross-section (width × thickness, the family cross nominal reading DepthMm); the invariant is proven ONCE
    // here, the insulation form/extent posture, so a coat carrying a fabricated module is unrepresentable.
    static Fin<SectionProfile> ProfileOf(FinishRow r, PositiveMagnitude thickness, Op key) =>
        from coherent in guard(r.ModuleMm.IsNone == r.Specification is FinishSpecification.Paint,
            ComponentFault.Family(key, $"<finish-module-coat-mismatch:{r.Designation}:{r.Kind.Key}>"))
        from profile in r.ModuleMm.Match(
            Some: module => SectionProfile.Rectangle.Of(widthMm: module.WidthMm, depthMm: thickness.Value, key),
            None: () => SectionProfile.Nominal.Of(thickness.Value, key))
        select profile;

    // The ONE generator fold (RAIL law): Traverse accumulates every failing row's fault, then the build aborts. The
    // appearance slot is the substance — a finish IS its visible surface, so the two MaterialId slots coincide by
    // construction rather than by omission.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Roster.Traverse(r =>
            from admitted in guard(r.Kind.Admits(r.Specification),
                ComponentFault.Family(context.Key, $"<finish-kind-spec-mismatch:{r.Designation}:{r.Kind.Key}>"))
            from thickness in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
            from profile in ProfileOf(r, thickness, context.Key)
            from detail in FinishDetail.Of(r, thickness, context.Key)
            from item in Component.Of(
                ComponentFamily.Finish, r.Designation, profile, r.Kind.Ifc,
                Coring.None, new ComponentStandard("us", StandardJointThicknessMm: 0.0, r.Kind.Authority),
                substanceId: r.Kind.Substance,
                appearanceId: r.Kind.Substance,
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, r.Source)).As();

    // The ComponentFamily.Finish CAPACITY producer: the typed total refusal — no finish product publishes a
    // structural resistance, and a stone panel's hang rides its fastener kerf/pin anchors' own verdicts.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<finish-publishes-no-resistance:{component.Designation.Value}>");
}
```

## [03]-[FIREPROOFING_FAMILY]

- Owner: `FireproofingKind` the form axis over the two substance keys; `SfrmDensityClass` the published density bands; `BondTier` the IBC bond-strength ladder over ASTM E736; `FireproofingSeed` the ONE generator on `DetailLane.Realization`.
- Cases: kind {sfrm, intumescent}; density class {low 240–336 kg/m³ gypsum-bound concealed-only, medium 352–625 exposed no-contact, high ≥ 640 cement-bound contact-prone}; bond tier {low-rise 150 psf under 22.9 m, mid-rise 430 psf to 128 m, high-rise 1000 psf above}.
- Entry: `FireproofingSeed.Rows(Context)` traverses the roster through kind/payload admission and the realization-bag fold onto the railed `Component.Of`.
- Growth: a new product is one `FireproofingRow`; a new density class or bond tier one vocabulary row; a board fire-protection system (shaft liner, calcium silicate) is a `panel#PANEL_FAMILY` board row with its tested-assembly listing, never a family here — the board split law holds across trades.
- Boundary: THE LISTING-OWNERSHIP LAW — no generic thickness-per-rating table exists for either form: an intumescent DFT is fixed by the specific UL 263 listing, the member's W/D section factor, and beam-versus-column orientation, and an SFRM thickness by the approved fire-resistance design, so `DetailSchema.RatingMinutes` stamps ONLY at the applied-system seat that knows its listing, never from a type row here — a type-level rating stamp certifies a rating no listing granted. `FireproofingThickness` stamps the row's own nominal build (the fact the type does standardize); `DensityClass` stamps on SFRM rows alone. ASTM E605 owns field thickness/density and ASTM E736 cohesion/adhesion; the E736 base-case product minimum varies by TDS and stays absent — the IBC ladder is the one published floor, independent of density.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// SFRM density classes — the two-sourced bands. The high-class ceiling is Option-absent because the band is
// unbounded above 640 kg/m³; Binder and Exposure carry the band's published service posture as tokens the spec
// stage reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SfrmDensityClass {
    public static readonly SfrmDensityClass Low    = new("low",    densityFloorKgM3: 240.0, densityCeilingKgM3: Some(336.0), binder: "gypsum",        exposure: "concealed");
    public static readonly SfrmDensityClass Medium = new("medium", densityFloorKgM3: 352.0, densityCeilingKgM3: Some(625.0), binder: "cement-gypsum", exposure: "exposed-no-contact");
    public static readonly SfrmDensityClass High   = new("high",   densityFloorKgM3: 640.0, densityCeilingKgM3: None,        binder: "cement",        exposure: "contact-prone");
    public double DensityFloorKgM3 { get; }
    public Option<double> DensityCeilingKgM3 { get; }
    public string Binder { get; }
    public string Exposure { get; }
}

// The IBC bond-strength ladder over ASTM E736 (introduced with the 2009 high-rise provisions): building height
// selects the minimum SFRM cohesion/adhesion, independent of density class. The published values are psf; kPa is
// the one conversion at this owner. For() selects by height — the fire-assessment seam's read.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondTier {
    const double PsfToKPa = 0.0478803;
    public static readonly BondTier LowRise  = new("low-rise",  heightFloorM: 0.0,   heightCeilingM: Some(22.9),  bondPsf: 150.0);
    public static readonly BondTier MidRise  = new("mid-rise",  heightFloorM: 22.9,  heightCeilingM: Some(128.0), bondPsf: 430.0);
    public static readonly BondTier HighRise = new("high-rise", heightFloorM: 128.0, heightCeilingM: None,        bondPsf: 1000.0);
    public double HeightFloorM { get; }
    public Option<double> HeightCeilingM { get; }
    public double BondPsf { get; }
    public double BondKPa => BondPsf * PsfToKPa;   // DEFINED

    public static BondTier For(double heightM) =>
        heightM > 128.0 ? HighRise : heightM > 22.9 ? MidRise : LowRise;
}

// The fireproofing form axis: substance key + the family's own USERDEFINED covering stamp (IfcCoveringTypeEnum
// publishes no fire-protection token). The intumescent form owns the intumescent binder whole per the paint split.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FireproofingKind {
    public static readonly FireproofingKind Sfrm        = new("sfrm",        substanceId: "fireproofing.sfrm",        admits: static s => s is FireproofingSpecification.Sfrm);
    public static readonly FireproofingKind Intumescent = new("intumescent", substanceId: "fireproofing.intumescent", admits: static s => s is FireproofingSpecification.Intumescent);
    public string SubstanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    [UseDelegateFromConstructor]
    public partial bool Admits(FireproofingSpecification specification);
}

// --- [MODELS] ------------------------------------------------------------------------------
[Union]
public abstract partial record FireproofingSpecification {
    private FireproofingSpecification() { }
    public sealed record Sfrm(SfrmDensityClass Density) : FireproofingSpecification;
    public sealed record Intumescent : FireproofingSpecification;
}

public readonly record struct FireproofingRow(
    string Designation, FireproofingKind Kind, double ThicknessMm, FireproofingSpecification Specification) {
    public Provenance Source { get; init; } = Provenance.Authored;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-built REALIZATION bag: FireproofingThickness the row's own nominal build, DensityClass on the SFRM arm
// alone. RatingMinutes never stamps here — the listing-ownership law seats it at the applied system.
public static class FireproofingDetail {
    public static Fin<PropertyBag> Of(FireproofingRow row, PositiveMagnitude thicknessMm, Op key) =>
        from thickness in ComponentDetail.Measured(DetailSchema.FireproofingThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        select ComponentDetail.RealizationRows([
            ComponentDetail.Sourced(row.Source),
            thickness,
            .. row.Specification is FireproofingSpecification.Sfrm sfrm
                ? Seq(ComponentDetail.Token(DetailSchema.DensityClass, sfrm.Density.Key))
                : Seq<(PropertyName, PropertyValue)>(),
        ]);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED fireproofing roster: nominal reference builds — thickness-per-rating stays with the listing, so a
// row standardizes the product form at an estate reference thickness and every rated application re-derives its own.
public static class FireproofingSeed {
    static readonly Seq<FireproofingRow> Roster = Seq(
        new FireproofingRow("fireproofing.sfrm-low-25",    FireproofingKind.Sfrm,        25.0, new FireproofingSpecification.Sfrm(SfrmDensityClass.Low)),
        new FireproofingRow("fireproofing.sfrm-medium-25", FireproofingKind.Sfrm,        25.0, new FireproofingSpecification.Sfrm(SfrmDensityClass.Medium)),
        new FireproofingRow("fireproofing.sfrm-high-25",   FireproofingKind.Sfrm,        25.0, new FireproofingSpecification.Sfrm(SfrmDensityClass.High)),
        new FireproofingRow("fireproofing.intumescent-3",  FireproofingKind.Intumescent,  3.0, new FireproofingSpecification.Intumescent()));

    // The ONE generator fold: every row is the Nominal applied build — fireproofing has no manufactured
    // cross-section — stamped with the family's USERDEFINED covering leaf.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Roster.Traverse(r =>
            from admitted in guard(r.Kind.Admits(r.Specification),
                ComponentFault.Family(context.Key, $"<fireproofing-kind-spec-mismatch:{r.Designation}:{r.Kind.Key}>"))
            from thickness in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
            from profile in SectionProfile.Nominal.Of(thickness.Value, context.Key)
            from detail in FireproofingDetail.Of(r, thickness, context.Key)
            from item in Component.Of(
                ComponentFamily.Fireproofing, r.Designation, profile,
                IfcBinding.Named("IfcCovering", "Fireproofing"),
                Coring.None, new ComponentStandard("us", StandardJointThicknessMm: 0.0, ComponentAuthority.Astm),
                substanceId: r.Kind.Substance,
                appearanceId: r.Kind.Substance,
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, r.Source)).As();

    // The ComponentFamily.Fireproofing CAPACITY producer: the typed total refusal — fireproofing protects a priced
    // member and prices nothing itself.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<fireproofing-publishes-no-resistance:{component.Designation.Value}>");
}
```

## [04]-[RESEARCH]

(none)
