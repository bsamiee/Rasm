# [MATERIALS_INSULATION]

THE INSULATION SEED PAGE owns the `insulation` `ComponentFamily` row facts (`ComponentClass.Minor`, `DetailLane.Product`, `IfcBinding.Of("IfcCovering", "INSULATION")` — the Ifc2X3-valid covering token) over the NON-BOARD thermal-insulation forms: batt, roll, loose-fill, and spray. THE BOARD SPLIT LAW: a rigid insulation BOARD is a `panel` family fact — `panel#PANEL_FAMILY` owns the `rigid-board-eps`/`rigid-board-xps`/`rigid-board-poly` kinds, their `FoamType` chemistry policy, and their `Layered` facer stacks — so this page admits no board form and mints no foam-chemistry twin; the discriminant is the profile: a FORMED product here is a `SectionProfile.Rectangle` batt cross-section (width × thickness — no ply stack exists to layer), an UNFORMED product a `SectionProfile.Nominal` installed depth, and a ply-stacked board is the panel family's `Layered` alone.

`InsulationForm` carries the install modality the `DetailSchema.InstallMethod` token stamps, `InsulationProduct` the substance-keyed R-per-inch design band with the EN 13162 λ-declaration axis, and `InsulationSeed.Rows : Context -> Fin<Seq<ComponentRow>>` the ONE generator fold. Substance physics (conductivity, density, fire class) read ONCE from the property library by `SubstanceId` — `insulation.glasswool`/`stonewool`/`woodfibre`/`pur` — never re-keyed here; the R band is the PRODUCT rating axis beside that substance truth, exactly as `FoamType` holds it for boards. The `Facer` axis and the `DetailSchema.ThermalResistance`/`FacerClass`/`PanelThickness`/`BoardLength` product rows reuse their existing owners; the family publishes no structural resistance, so its capacity producer is the typed refusal.

## [01]-[INDEX]

- [02]-[INSULATION_FAMILY]: `InsulationForm` the install-modality axis, `InsulationProduct` the R-band product axis, `InsulationRow` the provenance-columned roster row, `InsulationDetail` the product-bag fold, and `InsulationSeed` (the `Rows` generator, the form-routed `ProfileOf`, and the `Capacity` refusal).

## [02]-[INSULATION_FAMILY]

- Owner: `InsulationForm` binds each install modality to its `InstallMethod` token and its FORMED discriminant — the one column that routes profile construction; `InsulationProduct` binds each non-board product to its substance id, its published R-per-inch band, and its EN 13162 λD band; `InsulationRow` carries the roster row; `InsulationSeed` the generator and the capacity refusal.
- Cases: form {batt, roll, loose-fill, spray} — formed forms carry a manufactured width × length module, unformed forms an installed depth alone; product {glasswool-batt, stonewool-batt, woodfibre-batt, glasswool-loose, spray-open-cell, spray-closed-cell} — each row one substance key with its rating bands, absence typed where no two-sourced band exists.
- Entry: `InsulationSeed.Rows(Context)` traverses the roster through form/extent coherence, profile admission, and the detail-bag fold onto the railed `Component.Of`; one malformed row aborts the catalogue under the fail-loud CATALOGUE law.
- Packages: Rasm.Numerics (`PositiveMagnitude`), Rasm.Domain (`Op`/`Context`/`AcceptValidated`), Rasm.Element (`MaterialId`, `PropertyBag`, the seam `DetailSchema`/`PropertyName`/`PropertyValue`/`Dimension` currencies — every stamped row is Element-declared at `Rasm.Element/Properties/property#PROPERTY_BAG`, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`); NO external insulation producer exists — the roster is `AUTHORED` module policy under `SEED_ROW_LAW` while the R bands transcribe their published ranges on the product axis.
- Growth: a new insulation product is one `InsulationRow`; a new chemistry or form-chemistry pairing one `InsulationProduct` row; a new install modality one `InsulationForm` row; a faced product is one row selecting its `Facer` — the panel-owned axis, never a facing twin; a proven λD or R cell fills its `Option` in place. A BOARD product is a `panel#PANEL_FAMILY` row, never a row here.
- Boundary: the R design read is the band FLOOR — the conservative published minimum — so a stamped `ThermalResistance` never exceeds what every product in the band delivers; a product with no two-sourced band stamps NO thermal row (`woodfibre-batt`, `glasswool-loose`), and a single declared λD is structurally absent — EN 13162 λD is a per-product declaration, so only the typical band is representable. `Facer` reuses the panel axis (`Facer.Kraft` the single-faced kraft row); the ASTM C665 facing-type/flame-class roster reached no second source, so faced rows carry the facer token alone and the C665 class axis lands only with its proof. IfcBinding claim ambiguity is BY DESIGN: the panel rigid-board rows and every row here stamp `IfcCovering`/`INSULATION`, so `ComponentCatalogue.AdmitImported` elects NOTHING for that pair — an imported insulation type cannot be told board from batt off its IFC stamp, and the typed skip is the honest verdict. The spray substance rides `insulation.pur`; open-cell field density sits far below that substance row's rigid-foam density, a divergence the property library resolves at its own owner, never by a density column here.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                   // Fin, Option, Seq, Traverse
using Rasm.Numerics;                  // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                   // Op, Context, AcceptValidated
using Rasm.Element.Composition;      // MaterialId, PropertyBag
using Rasm.Element.Properties;       // DetailSchema, PropertyName, PropertyValue, Dimension (the seam bag currencies InsulationDetail composes)
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using Thinktecture;                  // [SmartEnum]/[KeyMemberEqualityComparer]/[KeyMemberComparer]
using static LanguageExt.Prelude;

// Every family seed declares in the ONE Rasm.Materials.Component namespace (component#COMPONENT_OWNER); the owner
// folds InsulationSeed.Rows through the ComponentFamily.Insulation policy row, never by name. Facer and the shared
// detail constructors bind by bare name from their panel and component owners.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The install-modality axis: Install is the DetailSchema.InstallMethod token verbatim (the Element-declared
// placement-mode vocabulary), and Formed is the profile route — a formed product carries a manufactured
// width × length module and lands as SectionProfile.Rectangle (width × thickness), an unformed one carries an
// installed depth alone and lands as SectionProfile.Nominal. Adhered placement is a growth row, landed when a
// product that adheres (a faced batt in a rainscreen, a mineral-wool board twin excluded by the board split) seeds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InsulationForm {
    public static readonly InsulationForm Batt      = new("batt",       install: "friction-fit", formed: true);
    public static readonly InsulationForm Roll      = new("roll",       install: "friction-fit", formed: true);
    public static readonly InsulationForm LooseFill = new("loose-fill", install: "blown",        formed: false);
    public static readonly InsulationForm Spray     = new("spray",      install: "sprayed",      formed: false);
    public string Install { get; }
    public bool Formed { get; }
}

// The non-board product axis: substance key + the PUBLISHED R-per-inch band + the EN 13162 typical λD band. The
// bands are Options because absence is typed, never zero-filled: wood-fibre batt and loose-fill glasswool reached no
// two-sourced R band this estate accepts, and a SINGLE declared λD is structurally absent — EN 13162 declares λD per
// product at 10 °C, so only the typical range (stone wool 0.034–0.038, glass wool 0.034–0.039 W/(m·K)) is
// representable and a point value would fabricate a declaration no producer made. Spray rows both key insulation.pur:
// open/closed cell is the product axis' own discriminant, and cell chemistry beyond the R band (air-barrier and
// vapour-barrier qualifying thicknesses) reached one source, so it stays off the row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InsulationProduct {
    const double InchToMm = 25.4;
    const double RValueIpToSi = 0.17611;   // (h·ft²·°F/Btu) -> (m²·K/W) — the panel FoamType conversion, restated at this owner
    public static readonly InsulationProduct GlassWoolBatt   = new("glasswool-batt",   substanceId: "insulation.glasswool", rPerInch: Some((Lo: 3.1, Hi: 3.8)), lambdaWMK: Some((Lo: 0.034, Hi: 0.039)));
    public static readonly InsulationProduct StoneWoolBatt   = new("stonewool-batt",   substanceId: "insulation.stonewool", rPerInch: Some((Lo: 3.7, Hi: 4.3)), lambdaWMK: Some((Lo: 0.034, Hi: 0.038)));
    public static readonly InsulationProduct WoodFibreBatt   = new("woodfibre-batt",   substanceId: "insulation.woodfibre", rPerInch: None, lambdaWMK: None);
    public static readonly InsulationProduct GlassWoolLoose  = new("glasswool-loose",  substanceId: "insulation.glasswool", rPerInch: None, lambdaWMK: None);
    public static readonly InsulationProduct SprayOpenCell   = new("spray-open-cell",  substanceId: "insulation.pur",       rPerInch: Some((Lo: 3.5, Hi: 3.8)), lambdaWMK: None);
    public static readonly InsulationProduct SprayClosedCell = new("spray-closed-cell", substanceId: "insulation.pur",      rPerInch: Some((Lo: 6.0, Hi: 7.0)), lambdaWMK: None);
    public string SubstanceId { get; }
    public Option<(double Lo, double Hi)> RPerInch { get; }
    public Option<(double Lo, double Hi)> LambdaWMK { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    // DEFINED: the SI thermal resistance (m²·K/W) off the band FLOOR — the conservative published minimum, so the
    // stamped value never exceeds what the weakest product in the band delivers; absence propagates whole.
    public Option<double> RValueSi(double thicknessMm) =>
        RPerInch.Map(band => band.Lo * (thicknessMm / InchToMm) * RValueIpToSi);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The roster row: ExtentMm is Some exactly where the form is FORMED — the width × length module of a cut product —
// and None for an installed depth, an invariant the generator proves before profile routing. Module dimensions are
// AUTHORED estate policy (no admitted producer prints them); the R band on the product axis stays the published fact.
public readonly record struct InsulationRow(
    string Designation, InsulationForm Form, InsulationProduct Product, Facer Facer,
    Option<(double WidthMm, double LengthMm)> ExtentMm, double ThicknessMm) {
    public Provenance Source { get; init; } = Provenance.Authored;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-built PRODUCT bag (DetailLane.Product): the InstallMethod token, thickness, the formed module's board
// length, the band-floor ThermalResistance where a band exists, and the FacerClass token where the row is faced —
// each row Element-declared, the constructors the relocated component#COMPONENT_DETAIL owners.
public static class InsulationDetail {
    public static Fin<PropertyBag> Of(InsulationRow row, PositiveMagnitude thicknessMm, Op key) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, thicknessMm.Value * 1e-3)
        from length in row.ExtentMm.Match(
            Some: extent => ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, extent.LengthMm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        from thermal in row.Product.RValueSi(thicknessMm.Value).Match(
            Some: si => ComponentDetail.Measured(DetailSchema.ThermalResistance, Dimension.Create(0, -1, 3, 0, 1, 0, 0), si).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.ProductRows([
            ComponentDetail.Sourced(row.Source),
            ComponentDetail.Token(DetailSchema.InstallMethod, row.Form.Install),
            thickness,
            .. length.ToSeq(),
            .. thermal.ToSeq(),
            .. FacerRow(row.Facer),
        ]);

    // A bare product has no facer to name — one absence read, the panel arm's own idiom.
    static Seq<(PropertyName Name, PropertyValue Value)> FacerRow(Facer facer) =>
        facer == Facer.None ? Empty : Seq(ComponentDetail.Token(DetailSchema.FacerClass, facer.Key));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED non-board roster: batt/roll modules on the 16/24 in stud-bay widths (15/23 in actual) and the 47 in
// batt length, loose-fill at conventional installed depths, spray at conventional pass thicknesses. Every dimension
// is estate module policy — Provenance.Authored, so AdmitImported never hands a vendor type this geometry as
// published — and the board-product standards carry no regional mortar joint (joint 0.0).
public static class InsulationSeed {
    const double Bay16Mm = 381.0;      // 15 in actual width, the 16 in o.c. stud bay
    const double Bay24Mm = 584.2;      // 23 in actual width, the 24 in o.c. stud bay
    const double BattLengthMm = 1193.8;   // 47 in
    const double RollLengthMm = 7620.0;   // 25 ft

    static readonly Seq<InsulationRow> Roster = Seq(
        new InsulationRow("insulation.batt-fg-89-16in",       InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-fg-89-16in-kraft", InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.Kraft, Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-fg-140-16in",      InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 140.0),
        new InsulationRow("insulation.batt-fg-89-24in",       InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay24Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-mw-89-16in",       InsulationForm.Batt,      InsulationProduct.StoneWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-mw-140-16in",      InsulationForm.Batt,      InsulationProduct.StoneWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 140.0),
        new InsulationRow("insulation.batt-wf-100",           InsulationForm.Batt,      InsulationProduct.WoodFibreBatt,   Facer.None,  Some((575.0, 1220.0)), 100.0),   // metric wood-fibre module
        new InsulationRow("insulation.roll-fg-89-16in",       InsulationForm.Roll,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, RollLengthMm)), 89.0),
        new InsulationRow("insulation.loose-fg-250",          InsulationForm.LooseFill, InsulationProduct.GlassWoolLoose,  Facer.None,  None, 250.0),
        new InsulationRow("insulation.loose-fg-400",          InsulationForm.LooseFill, InsulationProduct.GlassWoolLoose,  Facer.None,  None, 400.0),
        new InsulationRow("insulation.spray-oc-89",           InsulationForm.Spray,     InsulationProduct.SprayOpenCell,   Facer.None,  None, 89.0),
        new InsulationRow("insulation.spray-cc-50",           InsulationForm.Spray,     InsulationProduct.SprayClosedCell, Facer.None,  None, 50.0));

    // The form-routed profile: a FORMED row proves its module and lands the Rectangle batt cross-section
    // (width × thickness — the family cross nominal reads DepthMm, the thickness); an UNFORMED row proves its
    // absence and lands the Nominal installed depth. A row whose extent contradicts its form faults typed — the
    // invariant is proven here once, so no consumer re-derives which forms carry a module.
    static Fin<SectionProfile> ProfileOf(InsulationRow r, PositiveMagnitude thickness, Op key) =>
        from coherent in guard(r.ExtentMm.IsSome == r.Form.Formed,
            ComponentFault.Family(key, $"<insulation-form-extent-mismatch:{r.Designation}:{r.Form.Key}>"))
        from profile in r.ExtentMm.Match(
            Some: extent => SectionProfile.Rectangle.Of(widthMm: extent.WidthMm, depthMm: thickness.Value, key),
            None: () => SectionProfile.Nominal.Of(thickness.Value, key))
        select profile;

    // The appearance slot: the facer material where the row is faced, the substance otherwise — INDEPENDENT of
    // SubstanceId per the two-slot law (a kraft-faced batt keeps glasswool substance + kraft appearance).
    static MaterialId AppearanceOf(InsulationRow r) =>
        r.Facer == Facer.None ? r.Product.Substance : MaterialId.Of($"facer.{r.Facer.Key}");

    // The ONE generator fold (RAIL law): Traverse accumulates every failing row's fault, then the build aborts.
    // Every row stamps the family's own IfcCovering/INSULATION pair; Sectioned derives per profile (a Rectangle batt
    // solves, a Nominal depth answers unsectioned membership from its own topology).
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Roster.Traverse(r =>
            from thickness in context.Key.AcceptValidated<PositiveMagnitude>(candidate: r.ThicknessMm)
            from profile in ProfileOf(r, thickness, context.Key)
            from detail in InsulationDetail.Of(r, thickness, context.Key)
            from item in Component.Of(
                ComponentFamily.Insulation, r.Designation, profile,
                IfcBinding.Of("IfcCovering", "INSULATION"),
                Coring.None, new ComponentStandard("us", StandardJointThicknessMm: 0.0, ComponentAuthority.Astm),
                substanceId: r.Product.Substance,
                appearanceId: AppearanceOf(r),
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, r.Source)).As();

    // The ComponentFamily.Insulation CAPACITY producer: the typed total refusal. No insulation form publishes a
    // structural resistance — the batt's section solve is geometry bookkeeping, never a priced member — so every
    // designation refuses by name rather than borrowing another family's verdict.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<insulation-publishes-no-resistance:{component.Designation.Value}>");
}
```

## [03]-[RESEARCH]

(none)
