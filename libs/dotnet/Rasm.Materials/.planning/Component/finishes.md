# [MATERIALS_FINISHES]

THE COVERING SEED PAGE owns TWO family rows over ONE algebra. A finish product and a fire-protection coat are the same five shapes — a kind axis, a payload union, a product row, a bag builder, a seed law — differing only in the `DetailLane` their family declares, so this page states each shape ONCE and the family column carries the split. The `finish` family (`ComponentClass.Minor`, `DetailLane.Product`) carries paint systems, ceramic tile, resilient flooring, carpet tile, acoustical ceiling tile with its suspension-grid duty, and stone cladding; the `fireproofing` family (`ComponentClass.Minor`, `DetailLane.Realization`) carries SFRM and intumescent coating. THE FIREPROOFING HOME LAW: `DetailSchema.FireproofingThickness`/`RatingMinutes`/`DensityClass` are Element-declared REALIZATION rows and a family holds ONE `DetailLane`, so a Product-lane seat stamps realization facts through a product bag — the distinct Realization-lane family is the only honest seat, and it is a FAMILY split, never a second algebra. THE INTUMESCENT SPLIT: `PaintBinder` excludes the intumescent binder — an intumescent coat is fire protection with a listing-owned thickness, so the fireproofing family owns it whole. THE CLADDING HOME LAW: a stone cladding panel is a laid covering whose cross-section is a monolithic `SectionProfile.Rectangle` — the panel family admits only ply-stacked `Layered` and `Corrugated` sheet goods, so granite and marble cladding seed here; the anchors a stone panel hangs on are `fastener` family rows under the `AnchorRole` body/restraint axis, and this page names no anchor geometry.

`CoveringKind` binds each product to its family, its covering leaf, its substance, its authority, and its admissible payload; `CoveringSpecification` keys the closed per-product payload across both lanes; `CoveringInstall` carries the bedded/laid/coated attachment token — `PanelFastening` stays the stationed board-fastening owner, and a thin-set bed, a lay-in panel, and a sprayed coat carry no station schedule, so the two axes are method-versus-schedule, never siblings. `GridDuty` carries the ASTM C635 direct-hung main-runner classes; `SfrmDensityClass` the published density bands; `BondTier` the IBC bond ladder over ASTM E736, now DERIVED at seed time from each SFRM row's own qualified height and stamped into its bag. `CoveringRows` mints the producer-scoped row names the Element schema does not declare. Substance physics read ONCE from the property library by `SubstanceId`; neither family publishes a structural resistance, so both capacity producers are typed refusals.

## [01]-[INDEX]

- [02]-[COVERING_FAMILY]: `PaintBinder`, `TileClass`, `ResilientClass`/`ResilientTexture`, `CeilingType`/`CeilingFacing`/`CeilingForm`, `GridDuty`/`SeismicCategory`, `StoneFinish`, `CoveringInstall`, `CoveringKind`, the `CoveringSpecification` union with its `Laid` form fold and `Certified` declared-value census, `CoveringRows`, `CoveringRow`, `CoveringDetail`, the ONE `Covering` roster with its lane-filtered projections and law mint, and `FinishSeed` (`Roster`/`Law`/`Capacity`).
- [03]-[FIREPROOFING_FAMILY]: `SfrmDensityClass`, `BondTier`, the listing-ownership law, and `FireproofingSeed` (`Roster`/`Law`/`Capacity`).

## [02]-[COVERING_FAMILY]

- Owner: `CoveringKind` the product-kind axis with per-row family, IFC leaf, substance, authority, and payload admission; `CoveringSpecification` the closed payload union spanning both lanes; the class vocabularies carry their published bounds as row columns; `CoveringRow` the product row; `CoveringDetail` the ONE bag builder; `Covering` the ONE roster and law mint; `FinishSeed` the Product-lane projection `ComponentFamily.Finish` binds.
- Cases: kind {paint, floor tile, wall tile, LVT, VCT, carpet tile, ceiling tile, granite cladding, marble cladding, SFRM, intumescent}; tile class {impervious ≤0.5 %, vitreous ≤3 %, semi-vitreous ≤7 %, non-vitreous unbounded — interior walls only} per ANSI A137.1 absorption over the ASTM C373 test; resilient class {F1700 I/II/III over monolithic/surface-decorated/printed-film, F1066 1/2/3 over solid/through-pattern/surface-pattern} — one axis, each row's own KEY the standard designation; resilient texture {embossed, smooth}; ceiling {Type III/IV/XII × Form 1/2 over the painted/membrane facing, the two-sourced ASTM E1264 members}; grid duty {light 5.0, intermediate 12.0, heavy 16.0 lbf/ft direct-hung}; seismic category {A–F, each carrying the ASTM E580 duty it requires}.
- Entry: `ComponentSeed.Rows(context, FinishSeed.Roster, FinishSeed.Law)` — this page states the roster and the policy, never the fold. The roster is ONE table and each family reads the slice its own kind rows claim, so a product cannot be authored into the wrong lane and no second roster can drift from the first.
- Packages: Rasm.Numerics (`PositiveMagnitude`), Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `PropertyBag`, `DetailSchema`/`PropertyCategory`/`PropertyName`/`PropertyValue`/`Dimension`), Rasm.Materials.Component (the parent owner plus `SeedLaw`/`ComponentSeed`; `panel#PANEL_FAMILY` `LateralShear.PlfToKnPerM` the ONE plf conversion), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` admission, `[Union]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Option`/`.Apply`/`guard`); NO external covering producer exists — the roster is `User` module policy under `SEED_ROW_LAW`, the class tables `Catalogue` transcriptions.
- Growth: a new covering product is one `CoveringRow`; a new kind one `CoveringKind` row binding family, leaf, substance, and admission; a new tile/resilient/ceiling class, grid duty, density class, or bond tier one vocabulary row; a proven E1264 Type or Form member one row — the full roster stays paywalled and lands member-by-member with proof; a proven NRC/CAC roster lands as measured rows on a product table, never as class columns, because ASTM E1264 declares both per product.
- Boundary: every published class bound reaches a reader or does not exist here. A bound the class STATES stamps into the bag beside its class token (the ANSI A137.1 absorption ceiling, the ASTM F1700 wear-layer floor, the IBC bond floor), a bound that gates a ROW fact proves at the seed census (the ASTM E580 grid duty against the declared seismic category, the ASTM E605 certified density against its class band), and a SERVICE bound rides the kind's own payload admission (a floor tile refuses a class ANSI A137.1 bars from wet service). The ANSI A326.3 wet-DCOF floor is the one bound with no reader and it is DELETED rather than declared: it judges a product's MEASURED coefficient, no captured pack declares one, and a bound with no operand governs nothing — it returns together with the measured column it judges, exactly as the bond ladder returned with the qualified height that selects it. The F1913 unbacked-sheet numerics and the C635 indirect-hung/furring rows each reached one source and stay absent. Grid duty converts its published plf ONCE through the `panel#PANEL_FAMILY` `LateralShear.PlfToKnPerM` constant, and that SI value is the currency the seismic gate compares in. Stone rows carry geometry and surface finish alone; a thickness class here is `User` estate policy until a published ladder proves out.
- Boundary: the module/coat correspondence is a fact of the PAYLOAD, not of the kind roster, so `CoveringSpecification.Laid` is a generated total `Switch` every arm answers for itself — a new arm cannot join without stating whether its product is laid, and the seed proves `ModuleMm.IsSome == Specification.Laid` before profile routing. The evidence grade is a REQUIRED positional column on every row: the retired default filled thirteen of fourteen rows with an assumption and let the one exception look like the deliberate case, which is exactly backwards. The thickness ROW is a lane fact — a Product-lane covering publishes its build on `DetailSchema.PanelThickness`, a Realization-lane one on `DetailSchema.FireproofingThickness` — so `CoveringDetail` reads the family's own lane rather than carrying a per-kind column mirroring it.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;
using Thinktecture;
using static LanguageExt.Prelude;

// Every family seed declares in the ONE Rasm.Materials.Component namespace (component#COMPONENT_OWNER); the owner
// binds FinishSeed and FireproofingSeed through their policy rows. This ONE page seeds the two families because they
// share the covering domain while splitting on lane — the eventual Finishes.cs carries both.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The paint binder-class axis. The intumescent binder is EXCLUDED by the fireproofing split: an intumescent coat's
// thickness is listing-owned fire protection, so it is a CoveringKind on the Realization lane, and a row minted here
// would fork one product across two lanes.
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
// because non-vitreous is UNBOUNDED above 7 % — a fabricated cap would gate products the class admits — and it
// stamps into the bag as the published ceiling the class means. WetAreas states the standard's own SERVICE bound,
// read by the floor-tile kind's own payload admission: a non-vitreous body serves interior walls only, so a floor
// row carrying one refuses at construction rather than seeding a product the standard bars from its own service.
// The ANSI A326.3 wet-DCOF floor is NOT here: it gates a product's MEASURED coefficient, no captured pack declares
// one, and a bound with no operand governs nothing — it returns together with the measured column it judges.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileClass {
    public static readonly TileClass Impervious   = new("impervious",    absorptionCeilingPct: Some(0.5), wetAreas: true);    // porcelain body — the PTCA certification basis
    public static readonly TileClass Vitreous     = new("vitreous",      absorptionCeilingPct: Some(3.0), wetAreas: true);
    public static readonly TileClass SemiVitreous = new("semi-vitreous", absorptionCeilingPct: Some(7.0), wetAreas: true);
    public static readonly TileClass NonVitreous  = new("non-vitreous",  absorptionCeilingPct: None,      wetAreas: false);
    public Option<double> AbsorptionCeilingPct { get; }
    public bool WetAreas { get; }
}

// The resilient-flooring class axis over BOTH live ASTM specs on one identity regime — the row KEY carries its own
// spec designation, so F1700 (solid vinyl, Class III printed film the product the market names LVT) and F1066
// (vinyl composition tile) stay one axis rather than sibling enums and the retired `Spec` column restated the key's
// own prefix. WearLayerFloorMm is the F1700 commercial wear-layer minimum (0.020 in), carried on the F1700 rows
// alone and stamped into the bag; F1066 publishes no wear-layer column, so its rows state absence.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResilientClass {
    public static readonly ResilientClass F1700Monolithic  = new("f1700-i",   wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700Decorated   = new("f1700-ii",  wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700PrintedFilm = new("f1700-iii", wearLayerFloorMm: Some(0.50));   // LVT — Class III wear-layer binder ≥ 90 %
    public static readonly ResilientClass F1066Solid       = new("f1066-1",   wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Through     = new("f1066-2",   wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Surface     = new("f1066-3",   wearLayerFloorMm: None);
    public Option<double> WearLayerFloorMm { get; }
}

// The resilient surface graining as a stamped token: a bag reader gets the product fact by name, and a third
// texture is one row instead of a second flag beside the first.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResilientTexture {
    public static readonly ResilientTexture Embossed = new("embossed");
    public static readonly ResilientTexture Smooth   = new("smooth");
}

// The ASTM E1264 panel facing — the manufactured surface a cleanability or IAQ read keys on, carried as a token the
// bag stamps rather than a bool stating only whether a facing existed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingFacing {
    public static readonly CeilingFacing Painted  = new("painted");
    public static readonly CeilingFacing Membrane = new("membrane");
}

// ASTM E1264 acoustical-panel Type — the TWO-SOURCED members alone; the full Type I–XX roster stays paywalled and
// grows member-by-member with proof. NRC and CAC are product-declared under the classification, never standard-fixed,
// so they land as measured product rows when a roster proves them — a class column here would fabricate declarations.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingType {
    public static readonly CeilingType MineralPainted     = new("type-iii", facing: CeilingFacing.Painted);
    public static readonly CeilingType MineralMembrane    = new("type-iv",  facing: CeilingFacing.Membrane);
    public static readonly CeilingType GlassFibreMembrane = new("type-xii", facing: CeilingFacing.Membrane);
    public CeilingFacing Facing { get; }
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
// tested per ASTM E3090 — the three PUBLISHED direct-hung classes. The indirect-hung and furring-bar rows reached one
// source and stay off the axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridDuty {
    public static readonly GridDuty Light        = new("light",        mainRunnerPlf: 5.0);
    public static readonly GridDuty Intermediate = new("intermediate", mainRunnerPlf: 12.0);
    public static readonly GridDuty Heavy        = new("heavy",        mainRunnerPlf: 16.0);
    public double MainRunnerPlf { get; }
    // Defined: the published plf through the ONE plf conversion owner — panel's lateral-shear constant, so the two
    // published-imperial tables convert on one factor. This SI value is the currency the seismic gate compares in,
    // so the E580 admission is never run in a unit the estate elsewhere refuses.
    public double MainRunnerKnPerM => MainRunnerPlf * LateralShear.PlfToKnPerM;
}

// The ASCE 7 seismic design category owning its OWN ASTM E580 grid requirement: A and B carry no grid rule (the
// light class is the published floor), C requires intermediate duty, D through F heavy. The requirement is a column
// HERE because the category is what varies — the retired form switched a FREE STRING on the duty vocabulary and
// answered absence for every token outside A–F, so a mistyped category and an unregulated one were indistinguishable
// and no caller could be forced to declare a real one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeismicCategory {
    public static readonly SeismicCategory A = new("A", required: Option<GridDuty>.None);
    public static readonly SeismicCategory B = new("B", required: Option<GridDuty>.None);
    public static readonly SeismicCategory C = new("C", required: Some(GridDuty.Intermediate));
    public static readonly SeismicCategory D = new("D", required: Some(GridDuty.Heavy));
    public static readonly SeismicCategory E = new("E", required: Some(GridDuty.Heavy));
    public static readonly SeismicCategory F = new("F", required: Some(GridDuty.Heavy));
    public Option<GridDuty> Required { get; }

    // The gate compares PUBLISHED CAPACITY, not roster position, so a duty row landing between two existing ones is
    // admitted correctly without touching this member; an unregulated category admits every duty.
    public bool Admits(GridDuty grid) =>
        Required.Match(Some: duty => grid.MainRunnerKnPerM >= duty.MainRunnerKnPerM, None: static () => true);
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

// The bedded/laid/coated attachment axis the DetailSchema.FasteningMethod token stamps from, spanning BOTH lanes: a
// sprayed fire-protection build and a brushed architectural coat are the same METHOD fact a thin-set bed is.
// PanelFastening stays the stationed board-fastening owner — none of these carries a station schedule, so this axis
// is METHOD where that one is SCHEDULE, never a sibling spelling of one concept.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoveringInstall {
    public static readonly CoveringInstall ThinSet  = new("thin-set");
    public static readonly CoveringInstall Adhesive = new("adhesive");
    public static readonly CoveringInstall LayIn    = new("lay-in");
    public static readonly CoveringInstall Anchored = new("anchored");
    public static readonly CoveringInstall Coated   = new("coated");
    public static readonly CoveringInstall Sprayed  = new("sprayed");
}

// The covering product-kind axis over BOTH families: each row carries the family that owns it, its GeometryGym-verified
// Ifc2X3 covering leaf, its substance key, its standards body, and its payload admission — a new kind declares its own
// admissible payload where it declares everything else about itself. The FAMILY column is what lets one roster serve
// two ComponentFamily rows: a product cannot be authored into the wrong lane, because the lane is read off the kind's
// own family rather than off the table a row happened to sit in. Tile splits floor from wall on the covering token;
// stone splits granite from marble on the substance; the paint and fire-protection leaves ride USERDEFINED, because
// IfcCoveringTypeEnum publishes neither a coating nor a fire-protection token and the object-type discriminator
// carries the product name per the component IfcBinding law. The name CoveringKind is this page's, NOT a merge: the
// Appearance-namespace FinishKind is a different sense with its own behavior rows and consumers, and two senses on one
// spelling in one flat namespace is what the rename closes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoveringKind {
    public static readonly CoveringKind Paint       = new("paint",        family: ComponentFamily.Finish, ifc: IfcBinding.Named("IfcCovering", "PaintSystem"),   substanceId: "coating.paint",             authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Paint);
    // The floor row carries the ANSI A137.1 SERVICE bound in its own admission: a class the standard bars from wet
    // service cannot seed a floor, so the bound gates at construction instead of standing beside the class unread.
    public static readonly CoveringKind TileFloor   = new("tile-floor",   family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "FLOORING"),         substanceId: "ceramic.tile",              authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Tile tile && tile.Class.WetAreas);
    public static readonly CoveringKind TileWall    = new("tile-wall",    family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "CLADDING"),         substanceId: "ceramic.tile",              authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Tile);
    public static readonly CoveringKind Resilient   = new("resilient",    family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "FLOORING"),         substanceId: "flooring.resilient",        authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Resilient);
    public static readonly CoveringKind CarpetTile  = new("carpet-tile",  family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "FLOORING"),         substanceId: "flooring.carpet",           authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Carpet);
    public static readonly CoveringKind CeilingTile = new("ceiling-tile", family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "CEILING"),          substanceId: "ceiling.mineral",           authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Ceiling);
    public static readonly CoveringKind Granite     = new("granite",      family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "CLADDING"),         substanceId: "stone.granite",             authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Stone);
    public static readonly CoveringKind Marble      = new("marble",       family: ComponentFamily.Finish, ifc: IfcBinding.Of("IfcCovering", "CLADDING"),         substanceId: "stone.marble",              authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Stone);
    public static readonly CoveringKind Sfrm        = new("sfrm",         family: ComponentFamily.Fireproofing, ifc: IfcBinding.Named("IfcCovering", "Fireproofing"), substanceId: "fireproofing.sfrm",        authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Sfrm);
    public static readonly CoveringKind Intumescent = new("intumescent",  family: ComponentFamily.Fireproofing, ifc: IfcBinding.Named("IfcCovering", "Fireproofing"), substanceId: "fireproofing.intumescent", authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Intumescent);

    public ComponentFamily Family { get; }
    public IfcBinding Ifc { get; }
    public string SubstanceId { get; }
    public ComponentAuthority Authority { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public ComponentStandard Receipt => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);

    [UseDelegateFromConstructor]
    public partial bool Admits(CoveringSpecification specification);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Product payload as ONE closed family across both lanes: each case carries only the axes its product form admits, so
// a paint row carries no tile class, a carpet row carries nothing but its module, and an intumescent row carries no
// density class. The FIRE arms sit here rather than in a sibling union because the eight cases are one algebra the
// bag builder and the profile route both fold — two unions meant two generators and two bag folds for one shape.
[Union]
public abstract partial record CoveringSpecification {
    private CoveringSpecification() { }
    public sealed record Paint(PaintBinder Binder) : CoveringSpecification;
    public sealed record Tile(TileClass Class) : CoveringSpecification;
    public sealed record Resilient(ResilientClass Class, ResilientTexture Texture) : CoveringSpecification;
    public sealed record Carpet : CoveringSpecification;
    // Seismic is the ASCE 7 category the ceiling assembly is DECLARED for; the seed proves the grid against that
    // category's own ASTM E580 duty requirement, and an undeclared category leaves the grid unregulated rather than
    // inventing one. RatingMinutes-style listing facts never ride a type row (see [03]).
    public sealed record Ceiling(CeilingType Type, CeilingForm Form, GridDuty Grid, Option<SeismicCategory> Seismic) : CoveringSpecification;
    public sealed record Stone(StoneFinish Finish) : CoveringSpecification;
    // QualifiedHeightM is the building height the estate reference product is qualified to — an ordering fact a
    // specifier declares — and the IBC bond tier DERIVES from it at seed time, so no row stores a tier a height could
    // contradict. DensityKgM3 is the ASTM E605 CERTIFICATE slot the class band judges: absent until a certificate
    // fills it, and gated the moment one does.
    public sealed record Sfrm(SfrmDensityClass Density, double QualifiedHeightM, Option<double> DensityKgM3) : CoveringSpecification;
    public sealed record Intumescent : CoveringSpecification;

    // An APPLIED coat has no module — its only dimension is its dry-film or sprayed build — and a LAID product always
    // has one. The correspondence is a CASE fact, so every arm answers for itself and a ninth arm cannot join without
    // stating which it is; the seed proves the row's module against this answer before routing a profile.
    public bool Laid => Switch(
        paint:       static _ => false,
        tile:        static _ => true,
        resilient:   static _ => true,
        carpet:      static _ => true,
        ceiling:     static _ => true,
        stone:       static _ => true,
        sfrm:        static _ => false,
        intumescent: static _ => false);

    // The DECLARED-value census the seed coherence folds: each arm proves the declarations it carries against the
    // bands its own class publishes, and an arm declaring none proves vacuously. A certificate slot nobody filled is
    // an unmeasured product, never a failure, so no roster row is refused for a measurement no one took.
    public Validation<Error, Unit> Certified(Op key) => Switch(
        paint:       static _ => Success<Error, Unit>(unit),
        tile:        static _ => Success<Error, Unit>(unit),
        resilient:   static _ => Success<Error, Unit>(unit),
        carpet:      static _ => Success<Error, Unit>(unit),
        ceiling:     row => Prove(
            row.Seismic.Match(Some: category => category.Admits(row.Grid), None: true),
            new KernelFault.InvalidValue(nameof(row.Grid), "a grid admitted by the seismic category", Some(key))),
        stone:       static _ => Success<Error, Unit>(unit),
        sfrm:        row => Prove(
            row.DensityKgM3.Match(Some: row.Density.Admits, None: true),
            new KernelFault.InvalidValue(nameof(row.DensityKgM3), "a measurement inside its SFRM density class", Some(key))),
        intumescent: static _ => Success<Error, Unit>(unit));

    static Validation<Error, Unit> Prove(bool held, Error fault) => guard(held, fault).ToValidation();
}

// ModuleMm is Some exactly where the product is a LAID module (a cut tile, board, or slab with a width × length),
// None for an applied coat. Source is a REQUIRED positional column: a defaulted evidence grade filled every row with
// an assumption and made the one honest exception look like the deliberate case.
public readonly record struct CoveringRow(
    string Designation, CoveringKind Kind, Option<(double WidthMm, double LengthMm)> ModuleMm, double ThicknessMm,
    CoveringInstall Install, CoveringSpecification Specification, EvidenceGrade Source);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The producer-scoped row names the Element schema does not declare, minted ONCE through the owner-blessed
// PropertyCategory.Materials scope (the property page's declared growth arm for producer-local row families) — a
// call-site Row mint per stamp would fork the spelling per arm.
public static class CoveringRows {
    public static readonly PropertyName AbsorptionCeiling = PropertyCategory.Materials.Row(nameof(AbsorptionCeiling));
    public static readonly PropertyName AbsorptionClass   = PropertyCategory.Materials.Row(nameof(AbsorptionClass));
    public static readonly PropertyName BinderClass       = PropertyCategory.Materials.Row(nameof(BinderClass));
    public static readonly PropertyName BondFloor         = PropertyCategory.Materials.Row(nameof(BondFloor));
    public static readonly PropertyName BondTier          = PropertyCategory.Materials.Row(nameof(BondTier));
    public static readonly PropertyName CeilingClass      = PropertyCategory.Materials.Row(nameof(CeilingClass));
    public static readonly PropertyName CeilingFacing     = PropertyCategory.Materials.Row(nameof(CeilingFacing));
    public static readonly PropertyName CeilingForm       = PropertyCategory.Materials.Row(nameof(CeilingForm));
    public static readonly PropertyName GridDuty          = PropertyCategory.Materials.Row(nameof(GridDuty));
    public static readonly PropertyName ResilientClass    = PropertyCategory.Materials.Row(nameof(ResilientClass));
    public static readonly PropertyName ResilientTexture  = PropertyCategory.Materials.Row(nameof(ResilientTexture));
    public static readonly PropertyName SeismicCategory   = PropertyCategory.Materials.Row(nameof(SeismicCategory));
    public static readonly PropertyName SfrmBinder        = PropertyCategory.Materials.Row(nameof(SfrmBinder));
    public static readonly PropertyName SfrmDensity       = PropertyCategory.Materials.Row(nameof(SfrmDensity));
    public static readonly PropertyName SfrmExposure      = PropertyCategory.Materials.Row(nameof(SfrmExposure));
    public static readonly PropertyName StoneFinish       = PropertyCategory.Materials.Row(nameof(StoneFinish));
    public static readonly PropertyName WearLayerFloor    = PropertyCategory.Materials.Row(nameof(WearLayerFloor));
}

// The ONE seed-built bag both lanes share. The install token, the thickness, and the evidence grade are stated ONCE;
// the LANE selects the schema and the thickness row, and the payload's own Switch supplies the class tokens. A second
// bag builder for the fire lane would have been the same four lines plus one dispatch.
public static class CoveringDetail {
    public static Fin<PropertyBag> Of(CoveringRow row, PositiveMagnitude thicknessMm, Op key) =>
        from thickness in ComponentDetail.Measured(ThicknessRow(row.Kind.Family), Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from length in row.ModuleMm.Match(
            Some: module => ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, module.LengthMm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        from payload in PayloadRows(row.Specification)
        select Bag(row.Kind.Family.Lane, [
            ComponentDetail.Sourced(row.Source),
            ComponentDetail.Token(DetailSchema.FasteningMethod, row.Install.Key),
            thickness,
            .. length.ToSeq(),
            .. payload,
        ]);

    // The thickness row is a LANE fact: two Element-declared rows for two lanes, so the bag reads the family's own
    // lane rather than a per-kind column that would mirror it.
    static PropertyName ThicknessRow(ComponentFamily family) =>
        family.Lane == DetailLane.Product ? DetailSchema.PanelThickness : DetailSchema.FireproofingThickness;

    static PropertyBag Bag(DetailLane lane, params (PropertyName Name, PropertyValue Value)[] rows) =>
        lane == DetailLane.Product ? ComponentDetail.ProductRows(rows) : ComponentDetail.RealizationRows(rows);

    // Each spec arm stamps its OWN axes and nothing else, and every arm carries its class's PUBLISHED bounds beside
    // its class token — an absorption ceiling, a wear-layer floor, a bond floor — because a specifier reading the bag
    // needs the number the class means, not a key to dereference elsewhere. The SFRM arm stamps the density class,
    // its binder and exposure posture, the IBC bond tier its qualified height selects with that tier's own E736
    // cohesion/adhesion floor, and the E605 certified density where one has landed. An unbounded band and an unfilled
    // certificate each contribute NO row, so absence never reads as a measured zero.
    static Fin<Seq<(PropertyName Name, PropertyValue Value)>> PayloadRows(CoveringSpecification specification) => specification.Switch(
        paint:       static p => Fin.Succ(Seq(ComponentDetail.Token(CoveringRows.BinderClass, p.Binder.Key))),
        tile:        static t =>
            from ceiling in Bounded(CoveringRows.AbsorptionCeiling, Dimension.Dimensionless, 1e-2, t.Class.AbsorptionCeilingPct)
            select Seq(ComponentDetail.Token(CoveringRows.AbsorptionClass, t.Class.Key)) + ceiling,
        resilient:   static r =>
            from wear in Bounded(CoveringRows.WearLayerFloor, Dimension.LengthDim, 1e-3, r.Class.WearLayerFloorMm)
            select Seq(
                ComponentDetail.Token(CoveringRows.ResilientClass, r.Class.Key),
                ComponentDetail.Token(CoveringRows.ResilientTexture, r.Texture.Key)) + wear,
        carpet:      static _ => Fin.Succ(Seq<(PropertyName, PropertyValue)>()),
        ceiling:     static c => Fin.Succ(Seq(
                ComponentDetail.Token(CoveringRows.CeilingClass, c.Type.Key),
                ComponentDetail.Token(CoveringRows.CeilingFacing, c.Type.Facing.Key),
                ComponentDetail.Token(CoveringRows.CeilingForm, c.Form.Key),
                ComponentDetail.Token(CoveringRows.GridDuty, c.Grid.Key))
            + c.Seismic.Map(static category => ComponentDetail.Token(CoveringRows.SeismicCategory, category.Key)).ToSeq()),
        stone:       static s => Fin.Succ(Seq(ComponentDetail.Token(CoveringRows.StoneFinish, s.Finish.Key))),
        sfrm:        static s =>
            from density in Bounded(CoveringRows.SfrmDensity, Dimension.DensityDim, 1.0, s.DensityKgM3)
            let tier = BondTier.For(s.QualifiedHeightM)
            from bond in ComponentDetail.Measured(CoveringRows.BondFloor, Dimension.PressureDim, tier.BondKPa * 1e3)
            select Seq(
                ComponentDetail.Token(DetailSchema.DensityClass, s.Density.Key),
                ComponentDetail.Token(CoveringRows.SfrmBinder, s.Density.Binder),
                ComponentDetail.Token(CoveringRows.SfrmExposure, s.Density.Exposure),
                ComponentDetail.Token(CoveringRows.BondTier, tier.Key),
                bond) + density,
        intumescent: static _ => Fin.Succ(Seq<(PropertyName, PropertyValue)>()));

    // A stated bound crosses as a MEASURED row in the estate's own SI — a percent as a fraction, a millimetre as a
    // metre — and an absent bound contributes no row at all, so an unbounded class states its openness by omission
    // rather than by a fabricated cap a reader would take for the standard's.
    static Fin<Seq<(PropertyName Name, PropertyValue Value)>> Bounded(PropertyName name, Dimension dim, double toSi, Option<double> value) =>
        value.Match(
            Some: bound => ComponentDetail.Measured(name, dim, bound * toSi).Map(static row => Seq(row)),
            None: static () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The ONE covering roster over both lanes: metric tile/board modules, the imperial-module rows carrying their inch
// basis in the designation, and the fire-protection reference builds. The VCT row alone states Catalogue — its
// 12 × 12 in × 1/8 in gauge transcribes the two-sourced spec-practice convention; every other module and every
// fireproofing build is estate policy, so every row states its own grade positionally and none inherits one.
public static class Covering {
    public static readonly Seq<CoveringRow> Roster = Seq(
        // --- paint systems — module NONE: the Nominal profile is the reference dry-film build (0.1 mm architectural,
        //     0.3 mm high-build epoxy), the coat's ONE dimension
        new CoveringRow("finish.paint-acrylic",  CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Acrylic),  EvidenceGrade.User),
        new CoveringRow("finish.paint-alkyd",    CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Alkyd),    EvidenceGrade.User),
        new CoveringRow("finish.paint-epoxy",    CoveringKind.Paint, None, 0.3, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Epoxy),    EvidenceGrade.User),
        new CoveringRow("finish.paint-urethane", CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Urethane), EvidenceGrade.User),
        // --- ceramic tile (ANSI A137.1 classes over ASTM C373)
        new CoveringRow("finish.tile-porcelain-600", CoveringKind.TileFloor, Some((600.0, 600.0)), 10.0, CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.Impervious),  EvidenceGrade.User),
        new CoveringRow("finish.tile-porcelain-300", CoveringKind.TileFloor, Some((300.0, 300.0)), 9.0,  CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.Impervious),  EvidenceGrade.User),
        new CoveringRow("finish.tile-wall-200",      CoveringKind.TileWall,  Some((200.0, 200.0)), 8.0,  CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.NonVitreous), EvidenceGrade.User),
        // --- resilient flooring (ASTM F1700 LVT; ASTM F1066 VCT)
        new CoveringRow("finish.lvt-iii-152x1219", CoveringKind.Resilient, Some((152.4, 1219.2)), 5.0, CoveringInstall.Adhesive, new CoveringSpecification.Resilient(ResilientClass.F1700PrintedFilm, ResilientTexture.Embossed), EvidenceGrade.User),
        new CoveringRow("finish.vct-305",          CoveringKind.Resilient, Some((304.8, 304.8)),  3.2, CoveringInstall.Adhesive, new CoveringSpecification.Resilient(ResilientClass.F1066Solid, ResilientTexture.Smooth),         EvidenceGrade.Catalogue),
        // --- carpet tile
        new CoveringRow("finish.carpet-tile-610", CoveringKind.CarpetTile, Some((609.6, 609.6)), 6.4, CoveringInstall.Adhesive, new CoveringSpecification.Carpet(), EvidenceGrade.User),
        // --- acoustical ceiling tile (ASTM E1264 classification; ASTM C635 grid duty)
        new CoveringRow("finish.ceil-mineral-610x610",  CoveringKind.CeilingTile, Some((609.6, 609.6)),  15.9, CoveringInstall.LayIn, new CoveringSpecification.Ceiling(CeilingType.MineralPainted, CeilingForm.WaterFelted, GridDuty.Intermediate, Some(SeismicCategory.C)), EvidenceGrade.User),
        new CoveringRow("finish.ceil-mineral-610x1219", CoveringKind.CeilingTile, Some((609.6, 1219.2)), 15.9, CoveringInstall.LayIn, new CoveringSpecification.Ceiling(CeilingType.MineralMembrane, CeilingForm.WaterFelted, GridDuty.Heavy, Some(SeismicCategory.D)), EvidenceGrade.User),
        // --- stone cladding — anchors are fastener family rows (kerf/pin); this roster carries slab geometry and finish alone
        new CoveringRow("finish.stone-granite-30", CoveringKind.Granite, Some((1219.2, 609.6)), 30.0, CoveringInstall.Anchored, new CoveringSpecification.Stone(StoneFinish.Flamed), EvidenceGrade.User),
        new CoveringRow("finish.stone-marble-20",  CoveringKind.Marble,  Some((914.4, 609.6)),  20.0, CoveringInstall.Anchored, new CoveringSpecification.Stone(StoneFinish.Honed),  EvidenceGrade.User),
        // --- fire protection — nominal reference builds at three qualified heights, one per density class, so each
        //     band's IBC bond tier is reached; thickness-per-rating stays with the listing ([03])
        new CoveringRow("fireproofing.sfrm-low-25",    CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.Low, QualifiedHeightM: 18.0, DensityKgM3: None),    EvidenceGrade.User),
        new CoveringRow("fireproofing.sfrm-medium-25", CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.Medium, QualifiedHeightM: 90.0, DensityKgM3: None),  EvidenceGrade.User),
        new CoveringRow("fireproofing.sfrm-high-25",   CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.High, QualifiedHeightM: 180.0, DensityKgM3: None),   EvidenceGrade.User),
        new CoveringRow("fireproofing.intumescent-3",  CoveringKind.Intumescent, None,  3.0, CoveringInstall.Coated,  new CoveringSpecification.Intumescent(), EvidenceGrade.User));

    // Each family reads the slice its own kind rows claim. One roster and one law mint keep the two ComponentFamily
    // rows from drifting: a product's lane is the kind's own family column, so an SFRM row cannot be authored into
    // the finish slice and no filter predicate restates a fact the row already carries.
    public static Seq<CoveringRow> RosterFor(ComponentFamily family) => Roster.Filter(row => row.Kind.Family == family);

    // The seed POLICY value, one mint for both families: the roster and the coherence census are shared, and the
    // family is the only column that varies. The regional receipt derives from the kind's own authority row.
    public static SeedLaw<CoveringRow> LawFor(ComponentFamily family) => SeedLaw<CoveringRow>.Of(
        family: family,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: ProfileOf,
        substance: static row => row.Kind.Substance,
        source: static row => row.Source,
        standard: static row => row.Kind.Receipt,
        detail: Some<Func<CoveringRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        // A covering IS its visible surface, so the two MaterialId slots coincide by construction rather than by
        // omission.
        appearance: static row => row.Kind.Substance,
        ifc: static row => row.Kind.Ifc);

    // The row census, ACCUMULATING: the kind/payload admission, the module/coat correspondence, and the payload's own
    // declared-value proofs are INDEPENDENT authoring facts, so a paint row given a tile payload AND a module AND an
    // out-of-band certificate names all three in ONE verdict.
    static Validation<Error, Unit> Coherence(CoveringRow row, Op key) =>
        (guard(row.Kind.Admits(row.Specification),
             new KernelFault.InvalidValue(nameof(row.Specification), "a specification admitted by the covering kind", Some(key))).ToValidation(),
         guard(row.ModuleMm.IsSome == row.Specification.Laid,
             new KernelFault.InvalidValue(nameof(row.ModuleMm), "present exactly for laid coverings", Some(key))).ToValidation(),
         row.Specification.Certified(key))
            .Apply(static (_, _, _) => unit).As();

    // The module-routed profile: a coat lands the Nominal build — an applied coat has no module to rectangle — and
    // every laid product the Rectangle cross-section (width × thickness, the family cross nominal reading DepthMm).
    // The coherence census already proved the correspondence, so this fold reads the module and nothing else, and the
    // thickness admits ONCE at the profile factory's own PositiveMagnitude rail rather than being lifted here and
    // re-admitted there.
    static Fin<SectionProfile> ProfileOf(CoveringRow row, Op key) =>
        row.ModuleMm.Match(
            Some: module => SectionProfile.Rectangle.Of(widthMm: module.WidthMm, depthMm: row.ThicknessMm, key),
            None: () => SectionProfile.Nominal.Of(row.ThicknessMm, key));

    // The bag reads the thickness back off the ADMITTED profile rather than re-admitting the raw scalar, so the value
    // the bag publishes and the value the geometry carries are one number by construction.
    static Fin<PropertyBag> Detail(CoveringRow row, SectionProfile profile, Op key) =>
        CoveringDetail.Of(row, profile.GrossRectangleMm.DepthMm, key);
}

// --- [POLICIES] ----------------------------------------------------------------------------
public static class FinishSeed {
    public static readonly Seq<CoveringRow> Roster = Covering.RosterFor(ComponentFamily.Finish);
    public static readonly SeedLaw<CoveringRow> Law = Covering.LawFor(ComponentFamily.Finish);

    // The ComponentFamily.Finish CAPACITY producer: the typed total refusal — no finish product publishes a
    // structural resistance, and a stone panel's hang rides its fastener kerf/pin anchors' own verdicts.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [03]-[FIREPROOFING_FAMILY]

- Owner: `SfrmDensityClass` the published density bands; `BondTier` the IBC bond-strength ladder over ASTM E736; `FireproofingSeed` the Realization-lane projection of the ONE covering roster.
- Cases: kind {sfrm, intumescent} on the `CoveringKind` axis; density class {low 240–336 kg/m³ gypsum-bound concealed-only, medium 352–625 exposed no-contact, high ≥ 640 cement-bound contact-prone}; bond tier {low-rise 150 psf under 22.9 m, mid-rise 430 psf to 128 m, high-rise 1000 psf above}.
- Entry: `ComponentSeed.Rows(context, FireproofingSeed.Roster, FireproofingSeed.Law)` over the same roster and law mint the finish family reads.
- Growth: a new product is one `CoveringRow`; a new density class or bond tier one vocabulary row; a board fire-protection system (shaft liner, calcium silicate) is a `panel#PANEL_FAMILY` board row with its tested-assembly listing, never a family here — the board split law holds across trades.
- Boundary: THE LISTING-OWNERSHIP LAW — no generic thickness-per-rating table exists for either form: an intumescent DFT is fixed by the specific UL 263 listing, the member's W/D section factor, and beam-versus-column orientation, and an SFRM thickness by the approved fire-resistance design, so `DetailSchema.RatingMinutes` stamps ONLY at the applied-system seat that knows its listing, never from a type row — a type-level rating stamp certifies a rating no listing granted. The row's own nominal build stamps `FireproofingThickness` (the fact the type does standardize), `DensityClass` stamps on SFRM rows alone, and the BOND TIER stamps DERIVED from the row's qualified height. That derivation is what makes the ladder READ: the tier is derived rather than stored, so no height contradicts it, and the tier row is the resolver a fire-assessment stage dereferences for the cohesion/adhesion floor it verifies against. ASTM E605 owns field thickness/density and ASTM E736 cohesion/adhesion; the E736 base-case product minimum varies by TDS and stays absent — the IBC ladder is the one published floor, independent of density.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// SFRM density classes — the two-sourced bands. The high-class ceiling is Option-absent because the band is unbounded
// above 640 kg/m³; Binder and Exposure carry the band's published service posture as tokens the spec stage reads.
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

    // The band read a field ASTM E605 density verifies against — Some for a bounded band, and the floor alone above
    // 640, so an unbounded class states its openness instead of a fabricated ceiling.
    public bool Admits(double densityKgM3) =>
        densityKgM3 >= DensityFloorKgM3 && DensityCeilingKgM3.Match(Some: ceiling => densityKgM3 <= ceiling, None: static () => true);
}

// The IBC bond-strength ladder over ASTM E736 (introduced with the 2009 high-rise provisions): building height
// selects the minimum SFRM cohesion/adhesion, independent of density class. The published values are psf; kPa is the
// one conversion at this owner. `For` reads the ROWS' own height columns — the retired form re-typed the 22.9 and
// 128.0 thresholds in a ternary beside the columns that already carried them, so a revised band moved one number and
// left the other, and the selector and the roster could disagree with nothing to raise it.
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
    public double BondKPa => BondPsf * PsfToKPa;   // Defined

    // Half-open on the ceiling so the bands PARTITION: a building exactly at 22.9 m reads mid-rise once and once only,
    // which is what a shared threshold between two closed bands could not promise.
    public bool Covers(double heightM) =>
        heightM >= HeightFloorM && HeightCeilingM.Match(Some: ceiling => heightM < ceiling, None: static () => true);

    // The unbounded top row is what makes the fold total, so the fallback names that same row rather than a
    // fabricated tier.
    public static BondTier For(double heightM) =>
        toSeq(Items).Find(row => row.Covers(heightM)).IfNone(HighRise);
}

// --- [POLICIES] ----------------------------------------------------------------------------
public static class FireproofingSeed {
    public static readonly Seq<CoveringRow> Roster = Covering.RosterFor(ComponentFamily.Fireproofing);
    public static readonly SeedLaw<CoveringRow> Law = Covering.LawFor(ComponentFamily.Fireproofing);

    // The ComponentFamily.Fireproofing CAPACITY producer: the typed total refusal — fireproofing protects a priced
    // member and prices nothing itself.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [04]-[RESEARCH]

(none)
