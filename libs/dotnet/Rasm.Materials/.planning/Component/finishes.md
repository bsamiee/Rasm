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
- Packages: Rasm.Numerics (`PositiveMagnitude`), Rasm.Domain (`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `PropertyBag`, `DetailSchema`/`PropertyCategory`/`PropertyName`/`PropertyValue`/`Dimension`), Rasm.Materials.Component (the parent owner plus `SeedLaw`/`ComponentSeed`; `panel#PANEL_FAMILY` `LateralShear.PlfToKnPerM` the ONE plf conversion), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` with `[UseDelegateFromConstructor]` admission, `[Union]`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Option`/`.Apply`/`guard`); NO external covering producer exists — the roster is `User` module policy under `SEED_ROW_LAW`, the class tables `Catalogue` transcriptions.
- Growth: a new covering product is one `CoveringRow`; a new kind one `CoveringKind` row binding family, leaf, substance, and admission; a new tile/resilient/ceiling class, grid duty, density class, or bond tier one vocabulary row; a proven E1264 Type or Form member one row — the full roster stays paywalled and lands member-by-member with proof; a proven NRC/CAC roster lands as measured rows on a product table, never as class columns, because ASTM E1264 declares both per product.
- Boundary: every published class bound reaches a reader or does not exist here. A bound the class STATES stamps into the bag beside its class token (the ANSI A137.1 absorption ceiling, the ASTM F1700 wear-layer floor, the IBC bond floor), a bound that gates a ROW fact proves at the seed census (the ASTM E580 grid duty against the declared seismic category, the ASTM E605 certified density against its class band), and a SERVICE bound rides the kind's own payload admission (a floor tile refuses a class ANSI A137.1 bars from wet service). The ANSI A326.3 wet-DCOF floor is the one bound with no reader and it is DELETED rather than declared: it judges a product's MEASURED coefficient, no captured pack declares one, and a bound with no operand governs nothing — it returns together with the measured column it judges, exactly as the bond ladder returned with the qualified height that selects it. The F1913 unbacked-sheet numerics and the C635 indirect-hung/furring rows each reached one source and stay absent. Grid duty converts its published plf ONCE through the `panel#PANEL_FAMILY` `LateralShear.PlfToKnPerM` constant, and that SI value is the currency the seismic gate compares in. Stone rows carry geometry and surface finish alone; a thickness class here is `User` repo policy until a published ladder proves out.
- Boundary: the module/coat correspondence is a fact of the PAYLOAD, not of the kind roster, so `CoveringSpecification.Laid` is a generated total `Switch` every arm answers for itself — a new arm cannot join without stating whether its product is laid, and the seed proves `ModuleMm.IsSome == Specification.Laid` before profile routing. The evidence grade is a REQUIRED positional column on every row: the retired default filled thirteen of fourteen rows with an assumption and let the one exception look like the deliberate case, which is exactly backwards. The thickness ROW is a lane fact — a Product-lane covering publishes its build on `DetailSchema.PanelThickness`, a Realization-lane one on `DetailSchema.FireproofingThickness` — so `CoveringDetail` reads the family's own lane rather than carrying a per-kind column mirroring it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
public sealed partial class PaintBinder {
    public static readonly PaintBinder Acrylic  = new("acrylic");
    public static readonly PaintBinder Alkyd    = new("alkyd");
    public static readonly PaintBinder Epoxy    = new("epoxy");
    public static readonly PaintBinder Urethane = new("urethane");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileClass {
    public static readonly TileClass Impervious   = new("impervious",    absorptionCeilingPct: Some(0.5), wetAreas: true);
    public static readonly TileClass Vitreous     = new("vitreous",      absorptionCeilingPct: Some(3.0), wetAreas: true);
    public static readonly TileClass SemiVitreous = new("semi-vitreous", absorptionCeilingPct: Some(7.0), wetAreas: true);
    public static readonly TileClass NonVitreous  = new("non-vitreous",  absorptionCeilingPct: None,      wetAreas: false);
    public Option<double> AbsorptionCeilingPct { get; }
    public bool WetAreas { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResilientClass {
    public static readonly ResilientClass F1700Monolithic  = new("f1700-i",   wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700Decorated   = new("f1700-ii",  wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1700PrintedFilm = new("f1700-iii", wearLayerFloorMm: Some(0.50));
    public static readonly ResilientClass F1066Solid       = new("f1066-1",   wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Through     = new("f1066-2",   wearLayerFloorMm: None);
    public static readonly ResilientClass F1066Surface     = new("f1066-3",   wearLayerFloorMm: None);
    public Option<double> WearLayerFloorMm { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResilientTexture {
    public static readonly ResilientTexture Embossed = new("embossed");
    public static readonly ResilientTexture Smooth   = new("smooth");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingFacing {
    public static readonly CeilingFacing Painted  = new("painted");
    public static readonly CeilingFacing Membrane = new("membrane");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingType {
    public static readonly CeilingType MineralPainted     = new("type-iii", facing: CeilingFacing.Painted);
    public static readonly CeilingType MineralMembrane    = new("type-iv",  facing: CeilingFacing.Membrane);
    public static readonly CeilingType GlassFibreMembrane = new("type-xii", facing: CeilingFacing.Membrane);
    public CeilingFacing Facing { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CeilingForm {
    public static readonly CeilingForm Nodular     = new("form-1");
    public static readonly CeilingForm WaterFelted = new("form-2");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GridDuty {
    public static readonly GridDuty Light        = new("light",        mainRunnerPlf: 5.0);
    public static readonly GridDuty Intermediate = new("intermediate", mainRunnerPlf: 12.0);
    public static readonly GridDuty Heavy        = new("heavy",        mainRunnerPlf: 16.0);
    public double MainRunnerPlf { get; }
    public double MainRunnerKnPerM => MainRunnerPlf * LateralShear.PlfToKnPerM;
}

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

    public bool Admits(GridDuty grid) =>
        Required.Match(Some: duty => grid.MainRunnerKnPerM >= duty.MainRunnerKnPerM, None: static () => true);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoneFinish {
    public static readonly StoneFinish Polished = new("polished");
    public static readonly StoneFinish Honed    = new("honed");
    public static readonly StoneFinish Flamed   = new("flamed");
}

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoveringKind {
    public static readonly CoveringKind Paint       = new("paint",        family: ComponentFamily.Finish, ifc: IfcBinding.Named("IfcCovering", "PaintSystem"),   substanceId: "coating.paint",             authority: ComponentAuthority.Astm, admits: static s => s is CoveringSpecification.Paint);
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
    public MaterialId Substance => MaterialId.Create(SubstanceId);
    public ComponentStandard Standard => new(Authority.Region, StandardJointThicknessMm: 0.0, Authority);

    [UseDelegateFromConstructor]
    public partial bool Admits(CoveringSpecification specification);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record CoveringSpecification {
    private CoveringSpecification() { }
    public sealed record Paint(PaintBinder Binder) : CoveringSpecification;
    public sealed record Tile(TileClass Class) : CoveringSpecification;
    public sealed record Resilient(ResilientClass Class, ResilientTexture Texture) : CoveringSpecification;
    public sealed record Carpet : CoveringSpecification;
    public sealed record Ceiling(CeilingType Type, CeilingForm Form, GridDuty Grid, Option<SeismicCategory> Seismic) : CoveringSpecification;
    public sealed record Stone(StoneFinish Finish) : CoveringSpecification;
    public sealed record Sfrm(SfrmDensityClass Density, double QualifiedHeightM, Option<double> DensityKgM3) : CoveringSpecification;
    public sealed record Intumescent : CoveringSpecification;

    public bool Laid => Switch(
        paint:       static _ => false,
        tile:        static _ => true,
        resilient:   static _ => true,
        carpet:      static _ => true,
        ceiling:     static _ => true,
        stone:       static _ => true,
        sfrm:        static _ => false,
        intumescent: static _ => false);

    public Validation<Error, Unit> Certified() => Switch(
        paint:       static _ => Success<Error, Unit>(unit),
        tile:        static _ => Success<Error, Unit>(unit),
        resilient:   static _ => Success<Error, Unit>(unit),
        carpet:      static _ => Success<Error, Unit>(unit),
        ceiling:     row => AdmissionSlots.Gate(
            row.Seismic.Match(Some: category => category.Admits(row.Grid), None: true),
            new KernelFault.InvalidValue(nameof(row.Grid), "a grid admitted by the seismic category")),
        stone:       static _ => Success<Error, Unit>(unit),
        sfrm:        row => AdmissionSlots.Gate(
            row.DensityKgM3.Match(Some: row.Density.Admits, None: true),
            new KernelFault.InvalidValue(nameof(row.DensityKgM3), "a measurement inside its SFRM density class")),
        intumescent: static _ => Success<Error, Unit>(unit));
}

public readonly record struct CoveringRow(
    string Designation, CoveringKind Kind, Option<(double WidthMm, double LengthMm)> ModuleMm, double ThicknessMm,
    CoveringInstall Install, CoveringSpecification Specification, EvidenceGrade Source);

// --- [OPERATIONS] ----------------------------------------------------------------------
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

public static class CoveringDetail {
    public static Fin<PropertyBag> Of(CoveringRow row, PositiveMagnitude thicknessMm) =>
        from thickness in ComponentDetail.Measured(ThicknessRow(row.Kind.Family), Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from length in row.ModuleMm.TraverseM(module =>
            ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, module.LengthMm * 1e-3)).As()
        from payload in PayloadRows(row.Specification)
        select Bag(row.Kind.Family.Lane, [
            ComponentDetail.Sourced(row.Source),
            ComponentDetail.Token(DetailSchema.FasteningMethod, row.Install.Key),
            thickness,
            .. length.ToSeq(),
            .. payload,
        ]);

    static PropertyName ThicknessRow(ComponentFamily family) =>
        family.Lane == DetailLane.Product ? DetailSchema.PanelThickness : DetailSchema.FireproofingThickness;

    static PropertyBag Bag(DetailLane lane, params (PropertyName Name, PropertyValue Value)[] rows) =>
        lane == DetailLane.Product ? ComponentDetail.ProductRows(rows) : ComponentDetail.RealizationRows(rows);

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

    static Fin<Seq<(PropertyName Name, PropertyValue Value)>> Bounded(PropertyName name, Dimension dim, double toSi, Option<double> value) =>
        value.TraverseM(bound => ComponentDetail.Measured(name, dim, bound * toSi))
            .As()
            .Map(static row => row.ToSeq());
}

// --- [TABLES] --------------------------------------------------------------------------
public static class Covering {
    public static readonly Seq<CoveringRow> Roster = Seq(
        new CoveringRow("finish.paint-acrylic",  CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Acrylic),  EvidenceGrade.User),
        new CoveringRow("finish.paint-alkyd",    CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Alkyd),    EvidenceGrade.User),
        new CoveringRow("finish.paint-epoxy",    CoveringKind.Paint, None, 0.3, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Epoxy),    EvidenceGrade.User),
        new CoveringRow("finish.paint-urethane", CoveringKind.Paint, None, 0.1, CoveringInstall.Coated, new CoveringSpecification.Paint(PaintBinder.Urethane), EvidenceGrade.User),
        new CoveringRow("finish.tile-porcelain-600", CoveringKind.TileFloor, Some((600.0, 600.0)), 10.0, CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.Impervious),  EvidenceGrade.User),
        new CoveringRow("finish.tile-porcelain-300", CoveringKind.TileFloor, Some((300.0, 300.0)), 9.0,  CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.Impervious),  EvidenceGrade.User),
        new CoveringRow("finish.tile-wall-200",      CoveringKind.TileWall,  Some((200.0, 200.0)), 8.0,  CoveringInstall.ThinSet, new CoveringSpecification.Tile(TileClass.NonVitreous), EvidenceGrade.User),
        new CoveringRow("finish.lvt-iii-152x1219", CoveringKind.Resilient, Some((152.4, 1219.2)), 5.0, CoveringInstall.Adhesive, new CoveringSpecification.Resilient(ResilientClass.F1700PrintedFilm, ResilientTexture.Embossed), EvidenceGrade.User),
        new CoveringRow("finish.vct-305",          CoveringKind.Resilient, Some((304.8, 304.8)),  3.2, CoveringInstall.Adhesive, new CoveringSpecification.Resilient(ResilientClass.F1066Solid, ResilientTexture.Smooth),         EvidenceGrade.Catalogue),
        new CoveringRow("finish.carpet-tile-610", CoveringKind.CarpetTile, Some((609.6, 609.6)), 6.4, CoveringInstall.Adhesive, new CoveringSpecification.Carpet(), EvidenceGrade.User),
        new CoveringRow("finish.ceil-mineral-610x610",  CoveringKind.CeilingTile, Some((609.6, 609.6)),  15.9, CoveringInstall.LayIn, new CoveringSpecification.Ceiling(CeilingType.MineralPainted, CeilingForm.WaterFelted, GridDuty.Intermediate, Some(SeismicCategory.C)), EvidenceGrade.User),
        new CoveringRow("finish.ceil-mineral-610x1219", CoveringKind.CeilingTile, Some((609.6, 1219.2)), 15.9, CoveringInstall.LayIn, new CoveringSpecification.Ceiling(CeilingType.MineralMembrane, CeilingForm.WaterFelted, GridDuty.Heavy, Some(SeismicCategory.D)), EvidenceGrade.User),
        new CoveringRow("finish.stone-granite-30", CoveringKind.Granite, Some((1219.2, 609.6)), 30.0, CoveringInstall.Anchored, new CoveringSpecification.Stone(StoneFinish.Flamed), EvidenceGrade.User),
        new CoveringRow("finish.stone-marble-20",  CoveringKind.Marble,  Some((914.4, 609.6)),  20.0, CoveringInstall.Anchored, new CoveringSpecification.Stone(StoneFinish.Honed),  EvidenceGrade.User),
        new CoveringRow("fireproofing.sfrm-low-25",    CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.Low, QualifiedHeightM: 18.0, DensityKgM3: None),    EvidenceGrade.User),
        new CoveringRow("fireproofing.sfrm-medium-25", CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.Medium, QualifiedHeightM: 90.0, DensityKgM3: None),  EvidenceGrade.User),
        new CoveringRow("fireproofing.sfrm-high-25",   CoveringKind.Sfrm,        None, 25.0, CoveringInstall.Sprayed, new CoveringSpecification.Sfrm(SfrmDensityClass.High, QualifiedHeightM: 180.0, DensityKgM3: None),   EvidenceGrade.User),
        new CoveringRow("fireproofing.intumescent-3",  CoveringKind.Intumescent, None,  3.0, CoveringInstall.Coated,  new CoveringSpecification.Intumescent(), EvidenceGrade.User));

    public static Seq<CoveringRow> RosterFor(ComponentFamily family) => Roster.Filter(row => row.Kind.Family == family);

    public static SeedLaw<CoveringRow> LawFor(ComponentFamily family) => SeedLaw<CoveringRow>.Of(
        family: family,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: ProfileOf,
        substance: static row => row.Kind.Substance,
        source: static row => row.Source,
        standard: static row => row.Kind.Standard,
        detail: Some<Func<CoveringRow, SectionProfile, Fin<PropertyBag>>>(Detail),
        appearance: static row => row.Kind.Substance,
        ifc: static row => row.Kind.Ifc);

    static Validation<Error, Unit> Coherence(CoveringRow row) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                row.Kind.Admits(row.Specification),
                new KernelFault.InvalidValue(nameof(row.Specification), "a specification admitted by the covering kind")),
            AdmissionSlots.Gate(
                row.ModuleMm.IsSome == row.Specification.Laid,
                new KernelFault.InvalidValue(nameof(row.ModuleMm), "present exactly for laid coverings")),
            row.Specification.Certified()));

    static Fin<SectionProfile> ProfileOf(CoveringRow row) =>
        row.ModuleMm.Match(
            Some: module => SectionProfile.Rectangle.Of(widthMm: module.WidthMm, depthMm: row.ThicknessMm),
            None: () => SectionProfile.Nominal.Of(row.ThicknessMm));

    static Fin<PropertyBag> Detail(CoveringRow row, SectionProfile profile) =>
        CoveringDetail.Of(row, profile.GrossRectangleMm.DepthMm);
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class FinishSeed {
    public static readonly Seq<CoveringRow> Roster = Covering.RosterFor(ComponentFamily.Finish);
    public static readonly SeedLaw<CoveringRow> Law = Covering.LawFor(ComponentFamily.Finish);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        new ComponentFault.CapacityUnavailable(component.Designation);
}
```

## [03]-[FIREPROOFING_FAMILY]

- Owner: `SfrmDensityClass` the published density bands; `BondTier` the IBC bond-strength ladder over ASTM E736; `FireproofingSeed` the Realization-lane projection of the ONE covering roster.
- Cases: kind {sfrm, intumescent} on the `CoveringKind` axis; density class {low 240–336 kg/m³ gypsum-bound concealed-only, medium 352–625 exposed no-contact, high ≥ 640 cement-bound contact-prone}; bond tier {low-rise 150 psf under 22.9 m, mid-rise 430 psf to 128 m, high-rise 1000 psf above}.
- Entry: `ComponentSeed.Rows(context, FireproofingSeed.Roster, FireproofingSeed.Law)` over the same roster and law mint the finish family reads.
- Growth: a new product is one `CoveringRow`; a new density class or bond tier one vocabulary row; a board fire-protection system (shaft liner, calcium silicate) is a `panel#PANEL_FAMILY` board row with its tested-assembly listing, never a family here — the board split law holds across trades.
- Boundary: THE LISTING-OWNERSHIP LAW — no generic thickness-per-rating table exists for either form: an intumescent DFT is fixed by the specific UL 263 listing, the member's W/D section factor, and beam-versus-column orientation, and an SFRM thickness by the approved fire-resistance design, so `DetailSchema.RatingMinutes` stamps ONLY at the applied-system seat that knows its listing, never from a type row — a type-level rating stamp certifies a rating no listing granted. The row's own nominal build stamps `FireproofingThickness` (the fact the type does standardize), `DensityClass` stamps on SFRM rows alone, and the BOND TIER stamps DERIVED from the row's qualified height. That derivation is what makes the ladder READ: the tier is derived rather than stored, so no height contradicts it, and the tier row is the resolver a fire-assessment stage dereferences for the cohesion/adhesion floor it verifies against. ASTM E605 owns field thickness/density and ASTM E736 cohesion/adhesion; the E736 base-case product minimum varies by TDS and stays absent — the IBC ladder is the one published floor, independent of density.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    public bool Admits(double densityKgM3) =>
        densityKgM3 >= DensityFloorKgM3 && DensityCeilingKgM3.Match(Some: ceiling => densityKgM3 <= ceiling, None: static () => true);
}

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
    public double BondKPa => BondPsf * PsfToKPa;

    public bool Covers(double heightM) =>
        heightM >= HeightFloorM && HeightCeilingM.Match(Some: ceiling => heightM < ceiling, None: static () => true);

    public static BondTier For(double heightM) =>
        toSeq(Items).Find(row => row.Covers(heightM)).IfNone(HighRise);
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class FireproofingSeed {
    public static readonly Seq<CoveringRow> Roster = Covering.RosterFor(ComponentFamily.Fireproofing);
    public static readonly SeedLaw<CoveringRow> Law = Covering.LawFor(ComponentFamily.Fireproofing);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        new ComponentFault.CapacityUnavailable(component.Designation);
}
```

## [04]-[RESEARCH]

(none)
