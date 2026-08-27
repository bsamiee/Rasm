# [MATERIALS_PRECAST]

THE PRECAST PRODUCT SEED FAMILY. A precast plank, tee, or panel is a CATALOGUED PRODUCT — one `ComponentRow` the ONE `component#COMPONENT_SEED` generator mints from `PrecastSeed.Roster` under `PrecastSeed.Law` over the precast policy row (`ComponentClass.Primary`, `DetailLane.Product`, admits `SectionProfile.Rectangle` or `SectionProfile.CellularRectangle`, cross-nominal the section depth) — never a `HollowcorePlank`/`DoubleTee` type. The row axes are PRODUCT KIND × the closed `PrecastInterior` payload the evidence pack proves: the US hollowcore depth ladder over the 48-in module (152/203/254/305/406 mm × 1219 mm — the two-sourced rungs; the 14-in and 15-in depths and the European 1200-mm family are single-sourced or answer-level only and stay OFF the roster) and the 8-ft and 10-ft PCI double-tee families (8DT14/16/18/24 with stems 4 ft o.c., 10DT24/26 with stems 5 ft o.c., both at the 2-in flange) — and every column a manufacturer table did not two-source is TYPED-ABSENT, never estimated: the hollowcore CORE PATTERN, the double-tee STEM WIDTH and taper, the sandwich-panel numeric wythe builds, and the entire per-depth SPAN-LOAD ladder are `Option` columns answering `None` today, the span-load ladder's owner named on the row (the PCI Design Handbook and each producer's own load table).

The absence law shapes the whole page: a seeded product's `SectionProfile` is its GROSS ENVELOPE (`Rectangle` over nominal width × depth) because the interior geometry a truthful net section needs — cores, stems, wythes — is exactly the geometry the pack failed to two-source, and inventing it lets a solved `ComputedSection` publish fabricated net properties into the section map. The Product bag therefore stamps `SectionBasis` off the ADMITTED profile so a QTO or capacity reader knows whether the solved section is the shipping envelope or a real net, the proven interior facts (flange thickness, stem spacing) ride the bag as measured rows, and the family `Capacity` producer is an EXPLICIT TYPED REFUSAL naming the two absent routes — the PCI span-load ladder and the strand-designed `reinforcement#RC_SECTION` forward build over a future net profile. Sandwich wall panels ride the `Wythes` interior arm as the WYTHE CONVENTION (two concrete wythes over an insulation core, composite vs non-composite by connector) with no dimension columns at all — the numeric thickness ranges are single-sourced — and while `component#SECTION_PROFILE` now carries the `PlyRole.ConcreteWythe` row, the laminate arm stays closed to panels until a dimensioned wythe build two-sources and the family admission widens to `Layered`. IfcBindings are roster-exact against the generated GeometryGym IFC predefined-token census: `IfcSlab` publishes NO `HOLLOWCORE` token — `HOLLOWCORE` is an `IfcBeam` leaf — so the plank binds `IfcBeam`/`HOLLOWCORE`, the double tee `IfcBeam`/`T_BEAM`, the panel convention `IfcBuildingElementPart`/`PRECASTPANEL`, each the lawful exact token with the near-miss negative recorded on the row.

## [01]-[INDEX]

- [02]-[PRECAST_PRODUCTS]: the `PrecastKind` product roster with its roster-verified IFC leaves and its interior-admission column, the closed `PrecastInterior` payload carrying each form's proven geometry beside its typed-absent interior, and the `PrecastRow` roster row with its erection declaration.
- [03]-[PRECAST_SEED]: the `PrecastSeed` roster and seed law (the accumulating coherence, the envelope-or-net profile route, the Product bag), the `SeedJoin`-backed `Resolve`, and the `Capacity` typed refusal naming the absent capacity owners.

## [02]-[PRECAST_PRODUCTS]

- Owner: `PrecastKind` the product-kind `[SmartEnum<string>]` carrying each kind's CONCRETE IFC4 leaf and the `PrecastInterior` arm it admits; `PrecastInterior` the closed per-form payload; `PrecastRow` the tier-3 roster currency; `Erection` the declared-absent site stamps.
- Cases: kind {hollowcore · double-tee · sandwich-panel · solid-slab · beam · column · stair — the closed product vocabulary; only the first two carry seeded rows, because only their geometry two-sources, and the last five are roster rows a proven ladder can populate without a type edit} × interior {cored · stemmed · wythes · solid} × hollowcore depth {6 · 8 · 10 · 12 · 16 in} × double-tee family {8DT14 · 8DT16 · 8DT18 · 8DT24 · 10DT24 · 10DT26}.
- Law: TWO-SOURCED OR ABSENT — the 14/15-in hollowcore depths (single/no source), the European 1200-mm width and 150–500 depth family (convention/answer-level only), the 12DT/15DT families (PCI-handbook-owned), the core pattern, stem width, wythe thickness ranges, and every span-load cell are ABSENT with the absence typed; a row landing a `Some` in one of those columns must name two agreeing captures, and the seeded profile stays the gross envelope until the interior columns fill.
- Boundary: IFC pairs are roster-exact with the negatives recorded — `IfcSlab` carries {APPROACH_SLAB, BASESLAB, FLOOR, LANDING, PAVING, ROOF, SIDEWALK, TRACKSLAB, WEARING} and NO `HOLLOWCORE`, so the plank rides the `IfcBeam` `HOLLOWCORE[Ifc4]` leaf; the double tee rides `IfcBeam`/`T_BEAM`; the panel rides `IfcBuildingElementPart`/`PRECASTPANEL[Ifc4]` — a REAL predefined token, so the triple is disjoint from the masonry/cmu `USERDEFINED` + ObjectType claims on the same entity. The beam/column/stair roster rows deliberately carry NO `IfcBeam`/`BEAM` or `IfcColumn`/`COLUMN` binding today: those pairs are the CIP concrete family's claims, and a second family on one pair voids the `component#CATALOGUE` reverse-election for both — a precast beam ladder lands with its own discriminated leaf when its geometry two-sources. A kind whose leaf is absent cannot seed, which the coherence census states as its own conjunct rather than leaving to a downstream lift.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record PrecastInterior {
    private PrecastInterior() { }

    public sealed record Cored(Option<Seq<VoidCell>> Cores) : PrecastInterior;

    public sealed record Stemmed(double FlangeMm, double StemSpacingMm, Option<double> StemWidthMm) : PrecastInterior;

    public sealed record Wythes(bool Composite) : PrecastInterior;

    public sealed record Solid : PrecastInterior;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PrecastKind {
    public static readonly PrecastKind Hollowcore    = new("hollowcore",     Some(IfcBinding.Of("IfcBeam", "HOLLOWCORE")),                     admits: static i => i is PrecastInterior.Cored);
    public static readonly PrecastKind DoubleTee     = new("double-tee",     Some(IfcBinding.Of("IfcBeam", "T_BEAM")),                         admits: static i => i is PrecastInterior.Stemmed);
    public static readonly PrecastKind SandwichPanel = new("sandwich-panel", Some(IfcBinding.Of("IfcBuildingElementPart", "PRECASTPANEL")),    admits: static i => i is PrecastInterior.Wythes);
    public static readonly PrecastKind SolidSlab     = new("solid-slab",     Option<IfcBinding>.None,                                          admits: static i => i is PrecastInterior.Solid);
    public static readonly PrecastKind Beam          = new("beam",           Option<IfcBinding>.None,                                          admits: static i => i is PrecastInterior.Solid);
    public static readonly PrecastKind Column        = new("column",         Option<IfcBinding>.None,                                          admits: static i => i is PrecastInterior.Solid);
    public static readonly PrecastKind Stair         = new("stair",          Option<IfcBinding>.None,                                          admits: static i => i is PrecastInterior.Solid);
    public Option<IfcBinding> Ifc { get; }
    [UseDelegateFromConstructor] public partial bool Admits(PrecastInterior interior);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Erection(Option<string> LiftingInsert, Option<double> BearingLengthMm, Option<string> JointGrout) {
    public static readonly Erection Undeclared = new(None, None, None);

    public Fin<Seq<(PropertyName, PropertyValue)>> Rows() =>
        from bearing in BearingLengthMm
            .TraverseM(mm => ComponentDetail.Measured(DetailSchema.BearingLength, Dimension.LengthDim, mm * 1e-3))
            .As()
        select LiftingInsert.Map(static token => ComponentDetail.Token(DetailSchema.LiftingInsert, token)).ToSeq()
            + bearing.ToSeq()
            + JointGrout.Map(static token => ComponentDetail.Token(DetailSchema.JointGrout, token)).ToSeq();
}

public readonly record struct PrecastRow(
    string Key, PrecastKind Kind, double WidthMm, double DepthMm, PrecastInterior Interior,
    Option<ImmutableArray<(double SpanM, double LoadKnM2)>> SpanLoad) {
    public string Designation => $"precast.{Key}";
    public Erection Erection { get; init; } = Erection.Undeclared;
    public EvidenceGrade Source { get; init; } = EvidenceGrade.Catalogue;
}
```

## [03]-[PRECAST_SEED]

- Owner: `PrecastSeed` the roster, the seed law (coherence, profile route, Product bag), the `SeedJoin`-backed `Resolve` restoring the typed product row, and the typed `Capacity` refusal the precast `ComponentFamily` policy row binds.
- Entry: `ComponentSeed.Rows(context, PrecastSeed.Roster, PrecastSeed.Law)`. The coherence proves kind/interior correspondence, leaf presence, and dimensional sanity TOGETHER, so a roster with three malformed rows names all three in one verdict; the gross-envelope `Rectangle` (or the true `CellularRectangle` the moment a pattern lands) then admits through the fallible factory and one malformed row ABORTS the catalogue.
- Auto: `SectionBasis` stamps off the ADMITTED profile on every seeded bag — the one marker that keeps a gross-envelope `ComputedSection` from being read as a net section by a QTO or capacity consumer. When a row's `Cores` column fills, the profile becomes `CellularRectangle` and the marker flips to `"net"` off the section that was actually built, with no consumer edit and no second reading of the column.
- Packages: Rasm.Domain (project — `Context`), Rasm.Element (project — `MaterialId`, `EvidenceGrade`, `PropertyBag`, the `DetailSchema.LiftingInsert`/`BearingLength`/`JointGrout` rows — Element-declared, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum]`/`[Union]`/`[UseDelegateFromConstructor]`), LanguageExt.Core (`Validation`/`Fin`/`Seq`), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners, BCL inbox (`FrozenDictionary`, `ImmutableArray`); NO `VividOrange` — precast product geometry is manufacturer-standard data with no admitted producer.
- Growth: a new depth rung, tee family, or panel build is one row ONCE two-sourced; a core pattern or stem width landing flips that row's profile to its true net arm with no type edit; a producer span-load table landing fills `SpanLoad` and retires the capacity refusal for its rows; a new erection stamp is one `Erection` column over an Element-declared row — never a per-product type, never a fabricated interior, never a `ComponentFamily` edit.
- Boundary: the ENVELOPE LAW is this cluster's spine — a seeded plank or tee solves to its gross rectangle, so its section-map entry (area, inertia, takeoff volume) states the SHIPPING ENVELOPE and the bag says so; the true net section is PCI/producer-owned data the `Cored.Cores`/`Stemmed.StemWidthMm` columns await. `PrecastSeed.Capacity` refuses typed on BOTH absent routes by name: the flexural/shear capacity of a prestressed plank is its producer's span-load ladder (`SpanLoad = None` today), and the first-principles route is a strand-designed `reinforcement#RC_SECTION` build that needs the net profile — so the refusal is the honest verdict and a fabricated envelope-based capacity is unrepresentable. `ComponentAuthority` publishes no PCI row, so the seed rows stand under `ComponentAuthority.Astm` with its own region column supplying the standard and the true body stated here — the PCI authority row is `component#COMPONENT_OWNER` growth. Substance is `concrete.c50_60` on every row — the plant-cured strength band precast producers publish product data against is itself uncaptured, so the id is this repo's selection consistent with the row evidence, revisited when a producer sheet two-sources the design strength.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
public static class PrecastSeed {
    static readonly ComponentStandard Us =
        new(ComponentAuthority.Astm.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Astm);
    static readonly MaterialId Substance = MaterialId.Create("concrete.c50_60");
    static readonly PropertyName SectionBasis    = PropertyCategory.Materials.Row("SectionBasis");
    static readonly PropertyName FlangeThickness = PropertyCategory.Materials.Row("FlangeThickness");
    static readonly PropertyName StemSpacing     = PropertyCategory.Materials.Row("StemSpacing");

    public static readonly Seq<PrecastRow> Roster = Seq(
        new PrecastRow("hc6-48",  PrecastKind.Hollowcore, 1219.0, 152.0, new PrecastInterior.Cored(None), None),
        new PrecastRow("hc8-48",  PrecastKind.Hollowcore, 1219.0, 203.0, new PrecastInterior.Cored(None), None),
        new PrecastRow("hc10-48", PrecastKind.Hollowcore, 1219.0, 254.0, new PrecastInterior.Cored(None), None),
        new PrecastRow("hc12-48", PrecastKind.Hollowcore, 1219.0, 305.0, new PrecastInterior.Cored(None), None),
        new PrecastRow("hc16-48", PrecastKind.Hollowcore, 1219.0, 406.0, new PrecastInterior.Cored(None), None),
        new PrecastRow("8dt14",  PrecastKind.DoubleTee, 2438.0, 355.6, new PrecastInterior.Stemmed(50.8, 1219.0, None), None),
        new PrecastRow("8dt16",  PrecastKind.DoubleTee, 2438.0, 406.4, new PrecastInterior.Stemmed(50.8, 1219.0, None), None),
        new PrecastRow("8dt18",  PrecastKind.DoubleTee, 2438.0, 457.2, new PrecastInterior.Stemmed(50.8, 1219.0, None), None),
        new PrecastRow("8dt24",  PrecastKind.DoubleTee, 2438.0, 609.6, new PrecastInterior.Stemmed(50.8, 1219.0, None), None),
        new PrecastRow("10dt24", PrecastKind.DoubleTee, 3048.0, 609.6, new PrecastInterior.Stemmed(50.8, 1524.0, None), None),
        new PrecastRow("10dt26", PrecastKind.DoubleTee, 3048.0, 660.4, new PrecastInterior.Stemmed(50.8, 1524.0, None), None));

    public static readonly Lazy<Fin<FrozenDictionary<ComponentId, PrecastRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static Fin<PrecastRow> Resolve(Component component) =>
        SeedJoin.Resolve(Table, component.Designation);

    public static readonly SeedLaw<PrecastRow> Law = SeedLaw<PrecastRow>.Of(
        family: ComponentFamily.Precast,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: Profile,
        substance: static _ => Substance,
        source: static r => r.Source,
        standard: static _ => Us,
        detail: Some<Func<PrecastRow, SectionProfile, Fin<PropertyBag>>>(Detail),
        ifc: static r => r.Kind.Ifc.IfNone(ComponentFamily.Precast.Ifc));
    static Validation<Error, Unit> Coherence(PrecastRow r) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                r.Kind.Admits(r.Interior),
                new KernelFault.InvalidValue(nameof(r.Interior), "an interior admitted by the precast kind")),
            AdmissionSlots.Gate(
                r.Kind.Ifc.IsSome,
                new KernelFault.InvalidValue(nameof(r.Kind.Ifc), "a bound IFC precast kind")),
            AdmissionSlots.Gate(
                double.IsFinite(r.WidthMm) && r.WidthMm > 0.0 && double.IsFinite(r.DepthMm) && r.DepthMm > 0.0,
                new KernelFault.InvalidValue(nameof(PrecastRow), "positive finite width and depth"))));

    static Fin<SectionProfile> Profile(PrecastRow r) =>
        (r.Interior is PrecastInterior.Cored cored ? cored.Cores : Option<Seq<VoidCell>>.None).Match(
            Some: cells => SectionProfile.CellularRectangle.Of(r.WidthMm, r.DepthMm, cells),
            None: () => SectionProfile.Rectangle.Of(r.WidthMm, r.DepthMm));

    static Fin<PropertyBag> Detail(PrecastRow r, SectionProfile profile) =>
        from interior in InteriorRows(r.Interior)
        from stamps in r.Erection.Rows()
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(SectionBasis, profile is SectionProfile.CellularRectangle ? "net" : "envelope"),
            ComponentDetail.Sourced(r.Source),
            .. interior,
            .. stamps]);

    static Fin<Seq<(PropertyName, PropertyValue)>> InteriorRows(PrecastInterior interior) => interior.Switch(
        cored: static _ => Fin.Succ(Seq<(PropertyName, PropertyValue)>()),
        stemmed: static stems =>
            from flange in ComponentDetail.Measured(FlangeThickness, Dimension.LengthDim, stems.FlangeMm * 1e-3)
            from spacing in ComponentDetail.Measured(StemSpacing, Dimension.LengthDim, stems.StemSpacingMm * 1e-3)
            select Seq(flange, spacing),
        wythes: static wythes => Fin.Succ(Seq(ComponentDetail.Token(
            PropertyCategory.Materials.Row("WytheAction"), wythes.Composite ? "composite" : "non-composite"))),
        solid: static _ => Fin.Succ(Seq<(PropertyName, PropertyValue)>()));

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        new ComponentFault.CapacityUnavailable(component.Designation);
}
```

## [04]-[RESEARCH]

(none)
