# [MATERIALS_PRECAST]

THE PRECAST PRODUCT SEED FAMILY. A precast plank, tee, or panel is a CATALOGUED PRODUCT — one `ComponentRow` minted by the ONE generator `PrecastSeed.Rows -> Component.Of` over the precast policy row (`ComponentClass.Primary`, `DetailLane.Product`, admits `SectionProfile.Rectangle` or `SectionProfile.CellularRectangle`, cross-nominal the section depth) — never a `HollowcorePlank`/`DoubleTee` type. The row axes are PRODUCT KIND × the manufacturer-standard geometry ladder the evidence pack proves: `HollowcoreRow` the US depth ladder over the 48-in module (152/203/254/305/406 mm × 1219 mm — the two-sourced rungs; the 14-in and 15-in depths and the European 1200-mm family are single-sourced or answer-level only and stay OFF the roster), `DoubleTeeRow` the 8-ft and 10-ft PCI families (8DT14/16/18/24 with stems 4 ft o.c., 10DT24/26 with stems 5 ft o.c., both at the 2-in flange) — and every column a manufacturer table did not two-source is TYPED-ABSENT, never estimated: the hollowcore CORE LATTICE, the double-tee STEM WIDTH and taper, the sandwich-panel numeric wythe builds, and the entire per-depth SPAN-LOAD ladder are `Option` columns answering `None` today, the span-load ladder's owner named on the row (the PCI Design Handbook and each producer's own load table).

The absence law shapes the whole page: a seeded product's `SectionProfile` is its GROSS ENVELOPE (`Rectangle` over nominal width × depth) because the interior geometry a truthful net section needs — cores, stems, wythes — is exactly the geometry the pack failed to two-source, and inventing it lets a solved `ComputedSection` publish fabricated net properties into the section map. The Product bag therefore stamps `SectionBasis = "envelope"` beside every geometry row so a QTO or capacity reader knows the receipt is the shipping envelope, the proven interior facts (flange thickness, stem spacing) ride the bag as measured rows, and the family `Capacity` producer is an EXPLICIT TYPED REFUSAL naming the two absent routes — the PCI span-load ladder and the strand-designed `reinforcement#RC_SECTION` forward build over a future net profile. Sandwich wall panels land as the WYTHE CONVENTION (two concrete wythes over an insulation core, composite vs non-composite by connector) with no dimension rows at all — the numeric thickness ranges are single-sourced — and while `component#SECTION_PROFILE` now carries the `PlyRole.ConcreteWythe` row, the laminate arm stays closed to panels until a dimensioned wythe build two-sources and the family admission widens to `Layered`. IfcBindings are roster-exact against the generated GeometryGym IFC predefined-token census: `IfcSlab` publishes NO `HOLLOWCORE` token — `HOLLOWCORE` is an `IfcBeam` leaf — so the plank binds `IfcBeam`/`HOLLOWCORE`, the double tee `IfcBeam`/`T_BEAM`, the panel convention `IfcBuildingElementPart`/`PRECASTPANEL`, each the lawful exact token with the near-miss negative recorded on the row.

## [01]-[INDEX]

- [02]-[PRECAST_PRODUCTS]: the `PrecastKind` product roster with its roster-verified IFC leaves, the `HollowcoreRow`/`DoubleTeeRow` proven-geometry tables with their typed-absent interior and span-load columns, and the `WytheConvention` sandwich-panel facts that carry no dimensions.
- [03]-[PRECAST_SEED]: the seed-time `PrecastDetail` Product bag (`LiftingInsert`/`BearingLength`/`JointGrout` stamps, the `SectionBasis` envelope marker, the measured interior facts), the fail-loud `PrecastSeed.Rows`/`Resolve` fold, and the `PrecastSeed.Capacity` typed refusal naming the absent capacity owners.

## [02]-[PRECAST_PRODUCTS]

- Owner: `PrecastKind` the product-kind `[SmartEnum<string>]` carrying each kind's CONCRETE IFC4 leaf; `HollowcoreRow`/`DoubleTeeRow` the tier-3 row currencies with `Hollowcores`/`DoubleTees` the frozen tables; `WytheConvention` the sandwich-panel structural convention row.
- Cases: kind {hollowcore · double-tee · sandwich-panel · solid-slab · beam · column · stair — the closed product vocabulary; only the first two carry seeded rows, because only their geometry two-sources, and the last five are roster rows a proven ladder can populate without a type edit} × hollowcore depth {6 · 8 · 10 · 12 · 16 in} × double-tee family {8DT14 · 8DT16 · 8DT18 · 8DT24 · 10DT24 · 10DT26}.
- Law: TWO-SOURCED OR ABSENT — the 14/15-in hollowcore depths (single/no source), the European 1200-mm width and 150–500 depth family (convention/answer-level only), the 12DT/15DT families (PCI-handbook-owned), the core lattice, stem width, wythe thickness ranges, and every span-load cell are ABSENT with the absence typed; a row landing a `Some` in one of those columns must name two agreeing captures, and the seeded profile stays the gross envelope until the interior columns fill.
- Boundary: IFC pairs are roster-exact with the negatives recorded — `IfcSlab` carries {APPROACH_SLAB, BASESLAB, FLOOR, LANDING, PAVING, ROOF, SIDEWALK, TRACKSLAB, WEARING} and NO `HOLLOWCORE`, so the plank rides the `IfcBeam` `HOLLOWCORE[Ifc4]` leaf; the double tee rides `IfcBeam`/`T_BEAM`; the panel rides `IfcBuildingElementPart`/`PRECASTPANEL[Ifc4]` — a REAL predefined token, so the triple is disjoint from the masonry/cmu `USERDEFINED` + ObjectType claims on the same entity. The beam/column/stair roster rows deliberately carry NO `IfcBeam`/`BEAM` or `IfcColumn`/`COLUMN` binding today: those pairs are the CIP concrete family's claims, and a second family on one pair voids the `component#CATALOGUE` reverse-election for both — a precast beam ladder lands with its own discriminated leaf when its geometry two-sources.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                   // FrozenDictionary — the SeedJoin Resolve table
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                               // PositiveMagnitude
using Rasm.Domain;                                 // Op, Context
using Rasm.Element.Composition;                    // MaterialId, PropertyBag
using Rasm.Element.Properties;                     // DetailSchema, Dimension, PropertyCategory, PropertyName, PropertyValue
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

// Every family page declares in the ONE Rasm.Materials.Component namespace; component#COMPONENT_OWNER binds
// PrecastSeed.Rows by bare name on the precast ComponentFamily policy row.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The product-kind axis. Ifc is Some for the kinds whose roster-exact leaf exists AND is unclaimed by a sibling
// family; the beam/column/stair rows carry None because their natural leaves (IfcBeam/BEAM, IfcColumn/COLUMN,
// IfcStairFlight/STRAIGHT) either belong to the CIP concrete family's claims or await a proven product ladder —
// a kind with no leaf cannot seed, which is the same two-sourced-or-absent law the geometry columns obey.
// The plank's leaf is IfcBeam/HOLLOWCORE — IfcSlab publishes no HOLLOWCORE token (roster negative, recorded);
// the tee's is IfcBeam/T_BEAM; the panel's IfcBuildingElementPart/PRECASTPANEL (a REAL Ifc4 predefined token,
// disjoint by triple from masonry/cmu's USERDEFINED ObjectType claims on the same entity).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PrecastKind {
    public static readonly PrecastKind Hollowcore    = new("hollowcore",     Some(IfcBinding.Of("IfcBeam", "HOLLOWCORE")));
    public static readonly PrecastKind DoubleTee     = new("double-tee",     Some(IfcBinding.Of("IfcBeam", "T_BEAM")));
    public static readonly PrecastKind SandwichPanel = new("sandwich-panel", Some(IfcBinding.Of("IfcBuildingElementPart", "PRECASTPANEL")));
    public static readonly PrecastKind SolidSlab     = new("solid-slab",     Option<IfcBinding>.None);
    public static readonly PrecastKind Beam          = new("beam",           Option<IfcBinding>.None);
    public static readonly PrecastKind Column        = new("column",         Option<IfcBinding>.None);
    public static readonly PrecastKind Stair         = new("stair",          Option<IfcBinding>.None);
    public Option<IfcBinding> Ifc { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// SEED_ROW_LAW tier-3 currency: one hollowcore plank product. DepthMm/WidthMm PUBLISHED (two-sourced manufacturer
// standards over the PCI 4-ft module). Cores is the interior void lattice as VoidCell rows in the profile's
// corner frame — TYPED-ABSENT today (no manufacturer core geometry two-sourced); when it fills, ProfileOf mints
// the true CellularRectangle net and the envelope law retires for that row with no type edit. SpanLoad is the
// per-depth span-load ladder — TYPED-ABSENT with its owner stated: the PCI Design Handbook and the producer's
// own load table publish it, and a family-level "up to ~15.2 m at the deep end" claim is not a ladder.
public readonly record struct HollowcoreRow(string Key, double DepthMm, double WidthMm,
    Option<Seq<VoidCell>> Cores, Option<ImmutableArray<(double SpanM, double LoadKnM2)>> SpanLoad) {
    public string Designation => $"precast.{Key}";
}

// One double-tee product: nominal width, depth, flange thickness, and stem spacing PUBLISHED (Sweets/GPRM and
// Precast Specialties/GPRM load-table sheets agree); StemWidthMm — the stem's own breadth and taper — is
// TYPED-ABSENT (no sheet two-sourced it), which is exactly what blocks a truthful BuiltUp net section and keeps
// the seeded profile the gross envelope. SpanLoad as on the hollowcore row.
public readonly record struct DoubleTeeRow(string Key, double WidthMm, double DepthMm, double FlangeMm, double StemSpacingMm,
    Option<double> StemWidthMm, Option<ImmutableArray<(double SpanM, double LoadKnM2)>> SpanLoad) {
    public string Designation => $"precast.{Key}";
}

// The sandwich-panel STRUCTURAL CONVENTION — the two-sourced facts, which are qualitative: two concrete wythes
// over an insulation core; composite action via the wythe connectors; a non-composite panel carries one thick
// structural wythe and a thinner architectural wythe. Every NUMERIC column (wythe/core thicknesses, the 3-2-3
// notation's inch triple, panel width range) is single-sourced and ABSENT, so this row seeds NO component and
// exists to pin the vocabulary a future dimensioned row instantiates. The PlyRole.ConcreteWythe row is landed at
// component#SECTION_PROFILE, so the Layered arm can REPRESENT a wythe stack — the missing input is the DIMENSIONS,
// and a dimensioned panel ladder still has to two-source before any row seeds or the admission widens.
public readonly record struct WytheConvention(string Key, bool Composite);

// --- [TABLES] ------------------------------------------------------------------------------
// The five two-sourced US depth rungs × the 48-in module (6/8/10/12/16 in = 152/203/254/305/406 mm × 1219 mm).
// The 14-in (single-source, Canadian line) and 15-in (no capture) rungs and the European 1200-mm family
// (width convention-only, depth family answer-level) are OFF the roster — a rung lands with two captures.
public static class Hollowcores {
    public static readonly HollowcoreRow Hc6  = new("hc6-48",  152.0, 1219.0, None, None);
    public static readonly HollowcoreRow Hc8  = new("hc8-48",  203.0, 1219.0, None, None);
    public static readonly HollowcoreRow Hc10 = new("hc10-48", 254.0, 1219.0, None, None);
    public static readonly HollowcoreRow Hc12 = new("hc12-48", 305.0, 1219.0, None, None);
    public static readonly HollowcoreRow Hc16 = new("hc16-48", 406.0, 1219.0, None, None);
    public static readonly ImmutableArray<HollowcoreRow> Rows = [Hc6, Hc8, Hc10, Hc12, Hc16];
}

// The 8-ft family (2438 mm nominal, stems 4 ft = 1219 mm o.c., 2-in = 50.8 mm flange, depths 14/16/18/24 in) and
// the 10-ft family (3048 mm nominal, stems 5 ft = 1524 mm o.c., depths 24/26 in). The 12DT/15DT families are
// PCI-handbook-owned and OFF the roster; the composite-topping variants are erection-state facts, not products.
public static class DoubleTees {
    public static readonly DoubleTeeRow Dt8x14  = new("8dt14",  2438.0, 355.6, 50.8, 1219.0, None, None);
    public static readonly DoubleTeeRow Dt8x16  = new("8dt16",  2438.0, 406.4, 50.8, 1219.0, None, None);
    public static readonly DoubleTeeRow Dt8x18  = new("8dt18",  2438.0, 457.2, 50.8, 1219.0, None, None);
    public static readonly DoubleTeeRow Dt8x24  = new("8dt24",  2438.0, 609.6, 50.8, 1219.0, None, None);
    public static readonly DoubleTeeRow Dt10x24 = new("10dt24", 3048.0, 609.6, 50.8, 1524.0, None, None);
    public static readonly DoubleTeeRow Dt10x26 = new("10dt26", 3048.0, 660.4, 50.8, 1524.0, None, None);
    public static readonly ImmutableArray<DoubleTeeRow> Rows = [Dt8x14, Dt8x16, Dt8x18, Dt8x24, Dt10x24, Dt10x26];
}

// The two wythe-convention rows — vocabulary, never dimensions.
public static class WytheConventions {
    public static readonly ImmutableArray<WytheConvention> Rows = [
        new("composite", Composite: true), new("non-composite", Composite: false)];
}
```

## [03]-[PRECAST_SEED]

- Owner: `PrecastDetail` the seed-time Product-bag constructor; `PrecastSeed` the `Rows` fold, the `SeedJoin`-backed `Resolve` join restoring the typed product row, and the typed `Capacity` refusal the precast `ComponentFamily` policy row binds.
- Entry: `public static Fin<Seq<ComponentRow>> PrecastSeed.Rows(Context context)` traverses both product tables through the common `Component.Of` rail — each row builds its gross-envelope `Rectangle` through the railed factory, binds its kind's roster-verified leaf, and stamps its Product bag; one malformed row ABORTS the catalogue. The erection stamps are DECLARED, absent-honest: `PrecastDetail.Of(row..., lifting, bearingMm, grout)` stamps `LiftingInsert`/`BearingLength`/`JointGrout` only where the caller's product declaration supplies them — the pack proves no lifting-insert class, seat length, or grout spec, so the seed passes `None` and the bag carries no row, never a placeholder token.
- Auto: `SectionBasis = "envelope"` stamps on EVERY seeded bag unconditionally — the one marker that keeps a gross-envelope `ComputedSection` from being read as a net section by a QTO or capacity consumer; when a row's interior columns fill and `ProfileOf` mints the true `CellularRectangle`, the marker flips to `"net"` off the same `Cores.IsSome` fact, no consumer edit.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`), Rasm.Element (project — `MaterialId`, `PropertyBag`, the `DetailSchema.LiftingInsert`/`BearingLength`/`JointGrout` rows — Element-declared, never minted here), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` owners), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `ImmutableArray`, collection expressions); NO `VividOrange` — precast product geometry is manufacturer-standard data with no admitted producer.
- Growth: a new depth rung, tee family, or panel build is one row ONCE two-sourced; a core lattice or stem width landing flips that row's profile to its true net arm with no type edit; a producer span-load table landing fills `SpanLoad` and retires the capacity refusal for its rows; a new erection stamp is one `Erection` column over an Element-declared row — never a per-product type, never a fabricated interior, never a `ComponentFamily` edit.
- Boundary: the ENVELOPE LAW is this cluster's spine — a seeded plank or tee solves to its gross rectangle, so its section-map receipt (area, inertia, takeoff volume) states the SHIPPING ENVELOPE and the bag says so; the true net section is PCI/producer-owned data the `Cores`/`StemWidthMm` columns await. `PrecastSeed.Capacity` refuses typed on BOTH absent routes by name: the flexural/shear capacity of a prestressed plank is its producer's span-load ladder (`SpanLoad = None` today), and the first-principles route is a strand-designed `reinforcement#RC_SECTION` build that needs the net profile — so the refusal is the honest verdict and a fabricated envelope-based capacity is unrepresentable. `ComponentAuthority` publishes no PCI row, so the seed rows stand under `ComponentAuthority.Astm` (`us`) with the true body stated here — the PCI authority row is `component#COMPONENT_OWNER` growth. Substance is `concrete.c50_60` on every row — the plant-cured strength band precast producers publish product data against is itself uncaptured, so the id is this estate's AUTHORED selection consistent with the row provenance, revisited when a producer sheet two-sources the design strength.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The Product-lane bag: geometry rows the sheets prove, the envelope/net basis marker, and the three
// Element-declared erection stamps present only where declared. FlangeThickness/StemSpacing mint through the
// owner-blessed PropertyCategory.Materials producer scope (Element's DetailSchema declares no double-tee rows,
// and a Materials-scoped row is the sanctioned in-package mint the Sourced row already uses).
public static class PrecastDetail {
    static readonly PropertyName SectionBasis    = PropertyCategory.Materials.Row("SectionBasis");
    static readonly PropertyName FlangeThickness = PropertyCategory.Materials.Row("FlangeThickness");
    static readonly PropertyName StemSpacing     = PropertyCategory.Materials.Row("StemSpacing");

    public static Fin<PropertyBag> Hollowcore(HollowcoreRow row, Erection erection, Provenance source, Op key) =>
        from stamps in erection.Rows(key)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(SectionBasis, row.Cores.IsSome ? "net" : "envelope"),
            ComponentDetail.Sourced(source),
            .. stamps]);

    public static Fin<PropertyBag> DoubleTee(DoubleTeeRow row, Erection erection, Provenance source, Op key) =>
        from flange in ComponentDetail.Measured(FlangeThickness, Dimension.LengthDim, row.FlangeMm * 1e-3)
        from stems in ComponentDetail.Measured(StemSpacing, Dimension.LengthDim, row.StemSpacingMm * 1e-3)
        from stamps in erection.Rows(key)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(SectionBasis, "envelope"),
            flange,
            stems,
            ComponentDetail.Sourced(source),
            .. stamps]);
}

// The erection declaration: the three Element-declared precast stamps as OPTION columns a product declaration
// supplies — the pack proves none, so the seed's reference state is all-None and the bag carries only what is
// declared, never a placeholder. A vendor import or a project declaration fills them per row.
public readonly record struct Erection(Option<string> LiftingInsert, Option<double> BearingLengthMm, Option<string> JointGrout) {
    public static readonly Erection Undeclared = new(None, None, None);

    public Fin<Seq<(PropertyName, PropertyValue)>> Rows(Op key) =>
        from bearing in BearingLengthMm.Match(
            Some: mm => ComponentDetail.Measured(DetailSchema.BearingLength, Dimension.LengthDim, mm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select LiftingInsert.Map(static token => ComponentDetail.Token(DetailSchema.LiftingInsert, token)).ToSeq()
            + bearing.ToSeq()
            + JointGrout.Map(static token => ComponentDetail.Token(DetailSchema.JointGrout, token)).ToSeq();
}

public static class PrecastSeed {
    // Manufacturer-standard geometry transcribed from two agreeing sheets — Published; the substance selection
    // is estate-authored (stated on the cluster boundary), and the row provenance follows the GEOMETRY, the
    // bag's Sourced row carrying the same column across the seam.
    static readonly Provenance Tabulated = Provenance.Published;

    // PCI is the products' true standards body; ComponentAuthority publishes no PCI row, so Astm ("us") is the
    // lawful nearest and the PCI row is component#COMPONENT_OWNER growth. A precast product has no mortar joint.
    static readonly ComponentStandard Us = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    static readonly MaterialId Substance = MaterialId.Of("concrete.c50_60");

    // The gross-envelope profile — CellularRectangle the moment a core lattice lands (Cores.IsSome), Rectangle
    // envelope until then; both routes railed, and the bag's SectionBasis marker derives from the same fact.
    static Fin<SectionProfile> HollowcoreProfile(HollowcoreRow row, Op key) =>
        row.Cores.Match(
            Some: cells => SectionProfile.CellularRectangle.Of(row.WidthMm, row.DepthMm, cells, key),
            None: () => SectionProfile.Rectangle.Of(row.WidthMm, row.DepthMm, key));

    static Fin<ComponentRow> HollowcoreOf(HollowcoreRow row) {
        Op key = Op.Of(name: row.Designation);
        return
            from ifc in PrecastKind.Hollowcore.Ifc.ToFin(ComponentFault.Family(key, $"<precast-kind-unbound:{PrecastKind.Hollowcore.Key}>"))
            from profile in HollowcoreProfile(row, key)
            from detail in PrecastDetail.Hollowcore(row, Erection.Undeclared, Tabulated, key)
            from item in Component.Of(
                ComponentFamily.Precast, row.Designation, profile, ifc,
                Coring.None, Us, Substance, Substance,
                detail: Some(detail), key)
            select new ComponentRow(item, Tabulated);
    }

    static Fin<ComponentRow> DoubleTeeOf(DoubleTeeRow row) {
        Op key = Op.Of(name: row.Designation);
        return
            from ifc in PrecastKind.DoubleTee.Ifc.ToFin(ComponentFault.Family(key, $"<precast-kind-unbound:{PrecastKind.DoubleTee.Key}>"))
            from profile in SectionProfile.Rectangle.Of(row.WidthMm, row.DepthMm, key)
            from detail in PrecastDetail.DoubleTee(row, Erection.Undeclared, Tabulated, key)
            from item in Component.Of(
                ComponentFamily.Precast, row.Designation, profile, ifc,
                Coring.None, Us, Substance, Substance,
                detail: Some(detail), key)
            select new ComponentRow(item, Tabulated);
    }

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, (string Designation, PrecastKind Kind)>>> Kinds =
        SeedJoin.Of(
            toSeq(Hollowcores.Rows).Map(static r => (r.Designation, Kind: PrecastKind.Hollowcore))
                + toSeq(DoubleTees.Rows).Map(static r => (r.Designation, Kind: PrecastKind.DoubleTee)),
            static row => row.Designation);

    // The typed kind join the capacity refusal and a product reader compose — one designation-keyed lookup over
    // the same SeedJoin rail every family uses.
    public static Fin<PrecastKind> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Kinds, component.Designation, key).Map(static row => row.Kind);

    // Fail-loud, both product tables in ONE applicative pass so a malformed plank and a malformed tee name
    // themselves in one build abort. The Context parameter is the ComponentFamily.Rows delegate contract.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        (toSeq(Hollowcores.Rows).Traverse(HollowcoreOf).As(), toSeq(DoubleTees.Rows).Traverse(DoubleTeeOf).As())
            .Apply(static (planks, tees) => planks + tees).As();

    // The precast CAPACITY producer is an EXPLICIT TYPED REFUSAL naming BOTH absent owners: the product's own
    // span-load ladder (SpanLoad — PCI Design Handbook / producer load table, typed-absent) and the
    // first-principles strand-designed RcSectionBuilder route, which needs the net profile the Cores/StemWidthMm
    // columns await. An envelope-section capacity would price a solid rectangle a plank is not; refusing typed
    // is the honest verdict, and either route landing retires this refusal for its rows with no consumer edit.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<precast-capacity-awaits-span-ladder-or-net-profile:{component.Designation.Value}>");
}
```

## [04]-[RESEARCH]

(none)
