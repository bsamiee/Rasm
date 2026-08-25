# [MATERIALS_INSULATION]

THE INSULATION SEED PAGE owns the `insulation` `ComponentFamily` row facts (`ComponentClass.Minor`, `DetailLane.Product`, `IfcBinding.Of("IfcCovering", "INSULATION")` — the Ifc2X3-valid covering token) over the NON-BOARD thermal-insulation forms: batt, roll, loose-fill, and spray. THE BOARD SPLIT LAW: a rigid insulation BOARD is a `panel` family fact — `panel#PANEL_FAMILY` owns the `rigid-board-eps`/`rigid-board-xps`/`rigid-board-poly` kinds, their `FoamType` chemistry policy, and their `Layered` facer stacks — so this page admits no board form and mints no foam-chemistry twin; the discriminant is the profile: a FORMED product here is a `SectionProfile.Rectangle` batt cross-section (width × thickness — no ply stack exists to layer), an UNFORMED product a `SectionProfile.Nominal` installed depth, and a ply-stacked board is the panel family's `Layered` alone.

`InsulationForm` carries the install modality the `DetailSchema.InstallMethod` token stamps, `InsulationProduct` the substance-keyed R-per-inch design band with the EN 13162 λ-declaration axis, and `InsulationSeed.Roster`/`Law` the policy value the ONE `component#COMPONENT_SEED` generator folds. Substance physics (conductivity, density, fire class) read ONCE from the property library by `SubstanceId` — `insulation.glasswool`/`stonewool`/`woodfibre`/`pur` — never re-keyed here; the R band is the PRODUCT rating axis beside that substance truth, exactly as `FoamType` holds it for boards. The `Facer` axis and the `DetailSchema.ThermalResistance`/`FacerClass`/`PanelThickness`/`BoardLength` product rows reuse their existing owners; the family publishes no structural resistance, so its capacity producer is the typed refusal.

## [01]-[INDEX]

- [02]-[INSULATION_FAMILY]: `InsulationForm` the install-modality axis, `InsulationProduct` the R-band product axis, `InsulationRow` the evidence-columned roster row, and `InsulationSeed` (`Roster`, `Law` with its accumulating `Coherence`, the form-routed profile, the product bag, and the `Capacity` refusal).

## [02]-[INSULATION_FAMILY]

- Owner: `InsulationForm` binds each install modality to its `InstallMethod` token and its FORMED discriminant — the one column that routes profile construction; `InsulationProduct` binds each non-board product to its substance id, its published R-per-inch band, and its EN 13162 λD band; `InsulationRow` carries the roster row; `InsulationSeed` the roster, the seed law, and the capacity refusal.
- Cases: form {batt, roll, loose-fill, spray} — formed forms carry a manufactured width × length module, unformed forms an installed depth alone; product {glasswool-batt, stonewool-batt, woodfibre-batt, glasswool-loose, spray-open-cell, spray-closed-cell} — each row one substance key with its rating bands, absence typed where no two-sourced band exists.
- Entry: `ComponentSeed.Rows(context, InsulationSeed.Roster, InsulationSeed.Law)` — this page states the roster and the policy, never the fold. The law's coherence proves form/extent correspondence and dimensional sanity TOGETHER, so a malformed row names every column it broke in one verdict instead of the first hiding the rest; geometry admits after, inside the `SectionProfile` factories' own rail, and every failure aborts the catalogue under the fail-loud CATALOGUE law.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, the seam `DetailSchema`/`PropertyName`/`PropertyValue`/`Dimension` currencies — every stamped row is Element-declared at `Rasm.Element/Properties/property#PROPERTY_BAG`, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`), LanguageExt.Core (`Validation`/`Fin`/`Seq`/`Option`), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners; NO external insulation producer exists — the roster is `EvidenceGrade.User` module policy under `SEED_ROW_LAW` while the R bands transcribe their published ranges on the product axis.
- Growth: a new insulation product is one `InsulationRow`; a new chemistry or form-chemistry pairing one `InsulationProduct` row; a new install modality one `InsulationForm` row; a faced product is one row selecting its `Facer` — the panel-owned axis, never a facing twin; a proven λD or R cell fills its `Option` in place. A BOARD product is a `panel#PANEL_FAMILY` row, never a row here.
- Boundary: the R design read is the band FLOOR — the conservative published minimum — so a stamped `ThermalResistance` never exceeds what every product in the band delivers; a product with no two-sourced band stamps NO thermal row (`woodfibre-batt`, `glasswool-loose`), and a single declared λD is structurally absent — EN 13162 λD is a per-product declaration, so only the typical band is representable. `Facer` reuses the panel axis (`Facer.Kraft` the single-faced kraft row); the ASTM C665 facing-type/flame-class roster reached no second source, so faced rows carry the facer token alone and the C665 class axis lands only with its proof. IfcBinding claim ambiguity is BY DESIGN: the panel rigid-board rows and every row here stamp `IfcCovering`/`INSULATION`, so `ComponentCatalogue.AdmitImported` elects NOTHING for that pair — an imported insulation type cannot be told board from batt off its IFC stamp, and the typed skip is the honest verdict. The spray substance rides `insulation.pur`; open-cell field density sits far below that substance row's rigid-foam density, a divergence the property library resolves at its own owner, never by a density column here.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
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
public sealed partial class InsulationForm {
    public static readonly InsulationForm Batt      = new("batt",       install: "friction-fit", formed: true);
    public static readonly InsulationForm Roll      = new("roll",       install: "friction-fit", formed: true);
    public static readonly InsulationForm LooseFill = new("loose-fill", install: "blown",        formed: false);
    public static readonly InsulationForm Spray     = new("spray",      install: "sprayed",      formed: false);
    public string Install { get; }
    public bool Formed { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InsulationProduct {
    const double InchToMm = 25.4;
    const double RValueIpToSi = 0.17611;
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

    public Option<double> RValueSi(double thicknessMm) =>
        RPerInch.Map(band => band.Lo * (thicknessMm / InchToMm) * RValueIpToSi);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct InsulationRow(
    string Designation, InsulationForm Form, InsulationProduct Product, Facer Facer,
    Option<(double WidthMm, double LengthMm)> ExtentMm, double ThicknessMm) {
    public EvidenceGrade Source { get; init; } = EvidenceGrade.User;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class InsulationSeed {
    const double Bay16Mm = 381.0;
    const double Bay24Mm = 584.2;
    const double BattLengthMm = 1193.8;
    const double RollLengthMm = 7620.0;
    static readonly ComponentStandard Astm =
        new(ComponentAuthority.Astm.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Astm);

    public static readonly Seq<InsulationRow> Roster = Seq(
        new InsulationRow("insulation.batt-fg-89-16in",       InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-fg-89-16in-kraft", InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.Kraft, Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-fg-140-16in",      InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 140.0),
        new InsulationRow("insulation.batt-fg-89-24in",       InsulationForm.Batt,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay24Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-mw-89-16in",       InsulationForm.Batt,      InsulationProduct.StoneWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 89.0),
        new InsulationRow("insulation.batt-mw-140-16in",      InsulationForm.Batt,      InsulationProduct.StoneWoolBatt,   Facer.None,  Some((Bay16Mm, BattLengthMm)), 140.0),
        new InsulationRow("insulation.batt-wf-100",           InsulationForm.Batt,      InsulationProduct.WoodFibreBatt,   Facer.None,  Some((575.0, 1220.0)), 100.0),
        new InsulationRow("insulation.roll-fg-89-16in",       InsulationForm.Roll,      InsulationProduct.GlassWoolBatt,   Facer.None,  Some((Bay16Mm, RollLengthMm)), 89.0),
        new InsulationRow("insulation.loose-fg-250",          InsulationForm.LooseFill, InsulationProduct.GlassWoolLoose,  Facer.None,  None, 250.0),
        new InsulationRow("insulation.loose-fg-400",          InsulationForm.LooseFill, InsulationProduct.GlassWoolLoose,  Facer.None,  None, 400.0),
        new InsulationRow("insulation.spray-oc-89",           InsulationForm.Spray,     InsulationProduct.SprayOpenCell,   Facer.None,  None, 89.0),
        new InsulationRow("insulation.spray-cc-50",           InsulationForm.Spray,     InsulationProduct.SprayClosedCell, Facer.None,  None, 50.0));

    public static readonly SeedLaw<InsulationRow> Law = SeedLaw<InsulationRow>.Of(
        family: ComponentFamily.Insulation,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: static r => r.Product.Substance,
        source: static r => r.Source,
        standard: static _ => Astm,
        detail: Some<Func<InsulationRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.Facer == Facer.None ? r.Product.Substance : MaterialId.Of($"facer.{r.Facer.Key}"));

    static Validation<Error, Unit> Coherence(InsulationRow r, Op key) =>
        (guard(r.ExtentMm.IsSome == r.Form.Formed,
             new KernelFault.InvalidValue(nameof(r.ExtentMm), "present exactly for formed insulation", Some(key))).ToValidation(),
         guard(double.IsFinite(r.ThicknessMm) && r.ThicknessMm > 0.0
                 && r.ExtentMm.ForAll(static e => double.IsFinite(e.WidthMm) && e.WidthMm > 0.0 && double.IsFinite(e.LengthMm) && e.LengthMm > 0.0),
             new KernelFault.InvalidValue(nameof(InsulationRow), "positive finite thickness and formed extents", Some(key))).ToValidation())
            .Apply(static (_, _) => unit).As();

    static Fin<SectionProfile> Profile(InsulationRow r, Op key) =>
        r.ExtentMm.Match(
            Some: extent => SectionProfile.Rectangle.Of(widthMm: extent.WidthMm, depthMm: r.ThicknessMm, key),
            None: () => SectionProfile.Nominal.Of(r.ThicknessMm, key));

    static Fin<PropertyBag> Detail(InsulationRow r, SectionProfile profile, Op key) =>
        from thickness in ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, r.ThicknessMm * 1e-3)
        from length in r.ExtentMm.Match(
            Some: extent => ComponentDetail.Measured(DetailSchema.BoardLength, Dimension.LengthDim, extent.LengthMm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        from thermal in r.Product.RValueSi(r.ThicknessMm).Match(
            Some: si => ComponentDetail.Measured(DetailSchema.ThermalResistance, Dimension.Create(0, -1, 3, 0, 1, 0, 0), si).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.ProductRows([
            ComponentDetail.Sourced(r.Source),
            ComponentDetail.Token(DetailSchema.InstallMethod, r.Form.Install),
            thickness,
            .. length.ToSeq(),
            .. thermal.ToSeq(),
            .. FacerRow(r.Facer),
        ]);

    static Seq<(PropertyName Name, PropertyValue Value)> FacerRow(Facer facer) =>
        facer == Facer.None ? Empty : Seq(ComponentDetail.Token(DetailSchema.FacerClass, facer.Key));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [03]-[RESEARCH]

(none)
