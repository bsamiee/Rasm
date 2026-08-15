# [MATERIALS_ALUMINUM]

THE ALUMINUM SEED FAMILY GROUNDED IN THE PUBLISHED EN 1999 ALLOY BANDS AND THE AUTHORED DIE ROSTER. No standardized structural-section catalogue exists for the metal — roughly ninety percent of extrusions run on customer-owned dies — so the family's section truth is the opposite of steel's: `AluminumGrade` transcribes the EN 1999-1-1 Table 3.2a/3.2b characteristic bands as PUBLISHED policy rows (fo/fu per alloy × temper × product form × thickness window, the buckling-class letter, the typed-absent HAZ cell), while `AluminumSeed` folds an AUTHORED die roster — curtain-wall mullion and transom, structural extrusions, marine sheet — through the parent `SectionProfile` arms under `Provenance.Authored`, the same posture the steel fabricated rows take. Every die proves its alloy band at seed time, so a section outside its grade's published thickness window never reaches the capacity rail unpriced, and the `ExtrusionRole` axis owns the complete per-role IFC pair the Gate-0 roster validates.

The page composes the parent `component#COMPONENT_OWNER` whole — `Component.Of`, `ComponentRow`, `SectionProfile.RectangleHollow`/`IShape`/`Angle`/`CircleHollow`/`Rectangle` railed factories, `IfcBinding`, `Coring.None`, `ComponentStandard`/`ComponentAuthority.En`, `ComponentDetail.ProductRows`, the band-2300 `ComponentFault` rail — and stamps the seam `DetailSchema` CURTAIN-WALL/ALUMINUM product rows (`MullionProfile`, `ThermalBreak`, `GlazingPocket`, the reused `PanelThickness`). Substance identities are the Properties catalogue's own `aluminium.6061t6`/`6063t5`/`6063t6`/`6082t6`/`5083` rows; the render slot is the measured `metal.aluminum` conductor. The EC9 partial-factor pair (γM1 1.10 covering cross-section resistance and member instability alike — EN 1999-1-1 declares no γM0 — and γM2 1.25 on the fracture rail) states once here, mirrored by the `capacity#SECTION_CAPACITY` `DesignBasis` EN 1999 row; the buckling CURVE constants per class letter are that member arm's own columns, never this page's.

## [01]-[INDEX]

- [02]-[ALUMINUM_FAMILY]: the `ExtrusionForm` EN 755/EN 485 product-form axis, the `BucklingClass` Table 3.2 letter, the `ThermalBreakClass` isolator vocabulary, the `ExtrusionRole` die-role axis with its per-role IFC pair, the `AluminumPartialFactor` EC9 γM pair, the `AlloyBand`/`HazRow`/`AluminumGrade` published Table 3.2a/3.2b band registry with the ONE `Strengths` banded read, the `DieRow` authored-die currency, the `AluminumDetail` product bag, and the `AluminumSeed.Rows` fold with its `CapacityReceipt.Aluminum` producer.

## [02]-[ALUMINUM_FAMILY]

- Owner: `AluminumGrade` owns the EN 1999-1-1 characteristic bands, the buckling-class letter, the HAZ cell, and the substance identity; `ExtrusionForm` the EN 755-2/EN 485 product-form axis every band keys on; `ExtrusionRole` the die-role axis owning the COMPLETE per-role IFC pair; `ThermalBreakClass` the closed isolator vocabulary the `DetailSchema.ThermalBreak` stamp reads; `AluminumPartialFactor` the EC9 γM pair; `AluminumSeed` the die roster, the `ComponentFamily.Aluminum` row fold, and the capacity producer; `AluminumDetail` the product bag.
- Cases: grade {6061-T6 · 6063-T5 · 6063-T6 · 6082-T6 · 5083 O/H111} × form {ep · et · er-b · dt · sheet · plate} × role {mullion · transom · structural · panel} — a die is ONE `DieRow` over one railed profile build delegate, its alloy band selected by the die's own governing element thickness, never a per-die type and never a catalogue impersonation.
- Entry: `AluminumSeed.Rows(context)` traverses the die roster: `Strengths` proves the (form, thickness) band, the build delegate lands the parametric `SectionProfile` arm through the arm's own railed `Of`, `AluminumDetail` builds the product bag, and `Component.Of` constructs under the role's IFC pair — `Traverse` is the rail, a rejected die ABORTS the build. `AluminumSeed.Capacity` resolves the die, re-reads the banded pair, and lifts `CapacityReceipt.Aluminum`.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `DetailSchema`, `PropertyBag`, `PropertyName`, `PropertyValue`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/the railed `SectionProfile` arm factories/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`Provenance`; `capacity#SECTION_CAPACITY` `CapacityReceipt.Aluminum`, `CapacityPlacement`, `SectionCapacity.Lift`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + comparer accessors for the form/role/isolator/grade vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`/`.As()`/`ToFin`), BCL (`ImmutableArray`, `FrozenDictionary`). NO aluminum producer exists among admitted packages (reflection over the whole VividOrange train: `MaterialType.Aluminium` is one dead enum row, no grade, no factory, no extrusion family; `En1999` is a citation record over `En1999Part.Part1_1`–`Part1_5` with zero design members), so the grade bands are PUBLISHED here under per-column provenance and the EN 1999 citation rides the capacity basis row.
- Growth: a new alloy/temper is one `AluminumGrade` row carrying its printed bands; a new printed band one `AlloyBand` entry on its grade; a new die one `DieRow` (the profile arm and its `component#SECTION_SOLVER` supplement already exist — the die is DATA); a new curtain-wall role one `ExtrusionRole` row carrying its Gate-0-validated IFC pair; a new isolator one `ThermalBreakClass` row; a HAZ capability is the `HazRow` cell filling on its grade — never a parallel reduction table.
- Boundary: every numeric on this page is two-source-traced or typed-absent. The 6xxx HAZ pairs are single-sourced AND the simplified single-factor convention conflicts with the per-row EC9 pairs, so every 6xxx `Haz` cell is `None` and only the work-hardened 5083 O/H111 answers `Some(1.0, 1.0)`; the design modulus E = 70 GPa is single-sourced as a CODE value, so this page carries no E constant — the Properties catalogue substance rows own the modulus the member arm reads; the 6061-T6 sheet/plate band and the Table 3.2b Ramberg-Osgood exponent column are single-sourced and absent; standardized mullion-depth series and polyamide strut widths have no captured quantitative source, so pocket depth and isolator class are AUTHORED die facts on the roster, never standard claims. PROFILE REUSE is adjudicated closed: the parent `IShape`/`Channel`/`Tee`/`Angle`/`RectangleHollow`/`CircleHollow`/`ColdFormedC`/`Rectangle` arms cover the die space here, a free extruded silhouette rides `Outline` under its declared `SolidPolygon`/`OpenThin` topology, and the one unrepresentable die — a multi-chamber NON-rectangular hollow — is a voided-outline arm owned by `component#SECTION_PROFILE` growth, not minted here. IFC strings stay neutral; the Gate-0 roster proves `IfcMember` carries `MULLION` and `MEMBER` and NO transom token (the transom rides the `USERDEFINED` `ObjectType` discriminator IFC itself requires), and `IfcPlate`/`CURTAIN_PANEL` keeps the sheet row's triple disjoint from the glazing (`SHEET`) and panel (`NOTDEFINED`) family claims so the reverse `Claimant` read elects exactly one family. The capacity CURVE is not this page's: `AluminumSeed.Capacity` hands the member arm the banded (fo, fu) pair, the class letter, and the section receipt whole, and the §6.3.1 α/λ̄0 constants per class letter live on the `capacity#SECTION_CAPACITY` EN 1999 arm.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;        // FrozenDictionary (the designation-keyed die join the capacity producer reads)
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Domain;                      // Op, Context
using Rasm.Element.Composition;                     // MaterialId, DetailSchema, PropertyBag, PropertyName, PropertyValue
using Rasm.Element.Properties;
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride — disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The EN 755-2 / EN 485 product-form axis every EN 1999-1-1 Table 3.2a/3.2b band keys on: EP extruded profile, ET
// extruded tube, ER/B extruded rod and bar, DT drawn tube (the EN 755 forms), Sheet and Plate the EN 485 flat
// forms Table 3.2a covers. EN 755-1 owns delivery and inspection, EN 755-2 the mechanical bands mirrored here,
// EN 755-3..-9 the per-product dimensional tolerances — the parts are ROLES of one standard family, so no
// per-part row exists.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtrusionForm {
    public static readonly ExtrusionForm Profile = new("ep");
    public static readonly ExtrusionForm Tube    = new("et");
    public static readonly ExtrusionForm Rod     = new("er-b");
    public static readonly ExtrusionForm Drawn   = new("dt");
    public static readonly ExtrusionForm Sheet   = new("sheet");
    public static readonly ExtrusionForm Plate   = new("plate");
}

// The EN 1999-1-1 Table 3.2 buckling-class letter the §6.3.1 curve selection keys on. The generator behind the
// column: standardized fo ≤ 230 MPa classes B except a precipitation-hardened T6 temper, which classes A; fo above
// 230 classes A. The per-class α/λ̄0 curve constants are the capacity#SECTION_CAPACITY EN 1999 member arm's own
// columns, never this page's.
public enum BucklingClass : byte { A = 0, B = 1 }

// The curtain-wall isolator class the DetailSchema.ThermalBreak product row stamps — a closed fabrication
// vocabulary, never a free string on a bag.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThermalBreakClass {
    public static readonly ThermalBreakClass None           = new("none");
    public static readonly ThermalBreakClass PolyamideStrut = new("polyamide-strut");
    public static readonly ThermalBreakClass ResinPoured    = new("resin-poured");
}

// The die-role axis owning the COMPLETE per-role IFC pair — entity selection is a row read, never reconstructed at
// the seed, and a role whose leaf differs from the ComponentFamily.Aluminum default overrides per seed row under
// the component#COMPONENT_OWNER law. Gate-0: IfcMember carries MULLION and MEMBER and NO transom token, so the
// transom rides the USERDEFINED ObjectType discriminator IFC itself requires; the panel sheet is IfcPlate
// CURTAIN_PANEL — IfcPlate/SHEET and IfcPlate/NOTDEFINED stay the glazing and panel families' claims, so the
// reverse Claimant read elects exactly one family per triple. Framing drives the MullionProfile stamp.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtrusionRole {
    public static readonly ExtrusionRole Mullion    = new("mullion",    ifc: IfcBinding.Of("IfcMember", "MULLION"),      framing: true);
    public static readonly ExtrusionRole Transom    = new("transom",    ifc: IfcBinding.Named("IfcMember", "Transom"),   framing: true);
    public static readonly ExtrusionRole Structural = new("structural", ifc: IfcBinding.Of("IfcMember", "MEMBER"),       framing: false);
    public static readonly ExtrusionRole Panel      = new("panel",      ifc: IfcBinding.Of("IfcPlate", "CURTAIN_PANEL"), framing: false);
    public IfcBinding Ifc { get; }
    public bool Framing { get; }   // a framing role stamps DetailSchema.MullionProfile; the panel role stamps PanelThickness
}

// --- [CONSTANTS] ---------------------------------------------------------------------------
// EN 1999-1-1 declares NO γM0: γM1 = 1.10 covers cross-section resistance whatever the class AND member
// instability, γM2 = 1.25 the net-section/ultimate fracture rail. The capacity#SECTION_CAPACITY DesignBasis row
// for EN 1999 mirrors this pair, so the invariant states at both owners and moves as one.
public static class AluminumPartialFactor {
    public const double Instability = 1.10;   // γM1
    public const double Fracture    = 1.25;   // γM2
}

// --- [MODELS] ------------------------------------------------------------------------------
// One EN 1999-1-1 Table 3.2a/3.2b band: the product forms the printed row groups, its thickness window
// (exclusive-min, inclusive-max — the "3 < t ≤ 25" print), and the characteristic pair. Forms ride the band
// because the standard prints one row per form GROUP (EP/ET/ER/B share a cell on most alloys and split on
// 6082-T6), so a per-form copy would fork the printed row.
public readonly record struct AlloyBand(Seq<ExtrusionForm> Forms, double MinMm, double MaxMm, double FoMpa, double FuMpa) {
    public bool Covers(ExtrusionForm form, double thicknessMm) =>
        Forms.Contains(form) && thicknessMm > MinMm && thicknessMm <= MaxMm;
}

// The HAZ reduction pair ρo,haz/ρu,haz — PRESENT only where two independent sources agree. The EC9 per-row 6xxx
// pairs are single-sourced AND the simplified single-factor convention (one factor applied to fu) CONFLICTS with
// them numerically (190 against the per-row 185 on 6082-T6), so every 6xxx cell is typed-absent and a welded-zone
// check over a 6xxx die is unpriceable rather than mis-priced; 5083 O/H111 answers 1.0/1.0 on both sources
// because a work-hardened O-temper has no precipitation structure for a weld to dissolve.
public readonly record struct HazRow(double RhoO, double RhoU);

// The EN 1999-1-1 alloy-temper grade band: SEED_ROW_LAW rows transcribed from Tables 3.2a/3.2b (Published — the
// standard's own print corroborated by the independent EN 755-2/EN 485 producer datasheets), the buckling-class
// letter, the typed-absent HAZ cell, and the substance identity the Properties catalogue prices (the catalogue
// row also owns the 70 GPa design modulus — this page states no second E). Strengths is the ONE banded read every
// consumer routes through: the die's own governing element thickness and product form select the printed band,
// and an uncovered (form, thickness) rails typed rather than borrowing a neighbouring band. The 6063-T5 split is
// the EC9 print (≤ 3 / 3–25); the later EN 755-2 revision splits the SAME value pairs at ≤ 10 / 10–25, recorded
// as a variant, and the EC9 split seeds. The 6061-T6 sheet/plate band is single-sourced and absent — extrusion is
// its seeded form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AluminumGrade {
    public static readonly AluminumGrade A6061T6 = new("6061-t6", BucklingClass.A, "aluminium.6061t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 20.0, 240.0, 260.0)),
        haz: Option<HazRow>.None);
    public static readonly AluminumGrade A6063T5 = new("6063-t5", BucklingClass.B, "aluminium.6063t5",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 3.0, 130.0, 175.0),
            new AlloyBand(Seq(ExtrusionForm.Profile), 3.0, 25.0, 110.0, 160.0)),
        haz: Option<HazRow>.None);
    public static readonly AluminumGrade A6063T6 = new("6063-t6", BucklingClass.A, "aluminium.6063t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 25.0, 160.0, 195.0)),
        haz: Option<HazRow>.None);
    public static readonly AluminumGrade A6082T6 = new("6082-t6", BucklingClass.A, "aluminium.6082t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile), 0.0, 5.0, 250.0, 290.0),
            new AlloyBand(Seq(ExtrusionForm.Tube), 5.0, 15.0, 260.0, 310.0),
            new AlloyBand(Seq(ExtrusionForm.Rod), 0.0, 20.0, 250.0, 295.0)),
        haz: Option<HazRow>.None);
    public static readonly AluminumGrade A5083 = new("5083", BucklingClass.B, "aluminium.5083",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 200.0, 110.0, 270.0),
            new AlloyBand(Seq(ExtrusionForm.Sheet), 0.0, 50.0, 125.0, 275.0),
            new AlloyBand(Seq(ExtrusionForm.Plate), 50.0, 80.0, 115.0, 270.0)),
        haz: Some(new HazRow(1.0, 1.0)));
    public BucklingClass Class { get; }
    public string SubstanceId { get; }
    public Seq<AlloyBand> Bands { get; }
    public Option<HazRow> Haz { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    public Fin<(double FoMpa, double FuMpa)> Strengths(ExtrusionForm form, double thicknessMm, Op key) =>
        Bands.Find(band => band.Covers(form, thicknessMm))
            .Map(static band => (band.FoMpa, band.FuMpa))
            .ToFin(ComponentFault.Grade(key, $"<aluminum-band-unpublished:{Key}:{form.Key}:{thicknessMm:R}>"));
}

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED die currency — aluminum's section truth: no standardized structural-section catalogue exists for
// the metal (roughly ninety percent of extrusions run on customer-owned dies), so every roster row is this
// estate's own die under Provenance.Authored, the steel fabricated-row posture, and a project die is one more
// row. Build lands the die on its parametric SectionProfile arm through the arm's own railed Of, so a malformed
// die aborts the catalogue typed on the one fold; ElementMm is the governing element thickness the Table 3.2 band
// selects on (the wall of a hollow, the flange of an open section, the sheet's own thickness), proven against the
// grade band at seed time so a die outside its alloy's published window never seeds. Pocket depth and isolator
// class are die facts a shop states — the STANDARD mullion-depth series has no captured quantitative source, so
// no row claims one.
public readonly record struct DieRow(
    string Designation, ExtrusionRole Role, AluminumGrade Grade, ExtrusionForm Form, double ElementMm,
    ThermalBreakClass Break, Option<double> GlazingPocketMm, Func<Op, Fin<SectionProfile>> Build);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time DetailLane.Product bag: the seam CURTAIN-WALL/ALUMINUM rows stamped from the die's own facts —
// MullionProfile the framing-profile token on the framing roles, ThermalBreak the isolator class where one
// exists, GlazingPocket the authored pocket depth where the die carries one, PanelThickness the sheet's own cross
// thickness on the panel role — beside the Sourced provenance every Product bag carries.
public static class AluminumDetail {
    public static Fin<PropertyBag> Of(DieRow die) =>
        from pocket in die.GlazingPocketMm.Match(
            Some: static mm => ComponentDetail.Measured(DetailSchema.GlazingPocket, Dimension.LengthDim, mm * 1e-3).Map(Some),
            None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        from sheet in die.Role == ExtrusionRole.Panel
            ? ComponentDetail.Measured(DetailSchema.PanelThickness, Dimension.LengthDim, die.ElementMm * 1e-3).Map(Some)
            : Fin.Succ(Option<(PropertyName, PropertyValue)>.None)
        let framing = die.Role.Framing
            ? Seq(ComponentDetail.Token(DetailSchema.MullionProfile, die.Designation))
            : Seq<(PropertyName, PropertyValue)>()
        let isolator = die.Break == ThermalBreakClass.None
            ? Seq<(PropertyName, PropertyValue)>()
            : Seq(ComponentDetail.Token(DetailSchema.ThermalBreak, die.Break.Key))
        select ComponentDetail.ProductRows([
            ComponentDetail.Sourced(Provenance.Authored),
            .. framing,
            .. isolator,
            .. pocket.ToSeq(),
            .. sheet.ToSeq(),
        ]);
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The ONE catalogue fold and the family capacity producer the ComponentFamily.Aluminum policy row binds.
public static class AluminumSeed {
    static readonly ComponentStandard En = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly MaterialId Render = MaterialId.Of("metal.aluminum");

    // Six authored dies spanning the role axis: the 6063-T5 curtain-wall mullion/transom pair (polyamide-strut
    // break, authored 25 mm glazing pocket), the 6082-T6 structural I and tube, the 6061-T6 bracket angle, and
    // the 5083 sheet. Each ElementMm sits inside its grade's printed band, which RowOf proves rather than assumes.
    static readonly ImmutableArray<DieRow> Dies = [
        new("mullion-50x120", ExtrusionRole.Mullion, AluminumGrade.A6063T5, ExtrusionForm.Profile, 3.0,
            ThermalBreakClass.PolyamideStrut, Some(25.0),
            static key => SectionProfile.RectangleHollow.Of(widthMm: 50.0, depthMm: 120.0, wallMm: 3.0, innerFilletMm: 2.0, outerFilletMm: 2.0, key)),
        new("transom-50x80", ExtrusionRole.Transom, AluminumGrade.A6063T5, ExtrusionForm.Profile, 3.0,
            ThermalBreakClass.PolyamideStrut, Some(25.0),
            static key => SectionProfile.RectangleHollow.Of(widthMm: 50.0, depthMm: 80.0, wallMm: 3.0, innerFilletMm: 2.0, outerFilletMm: 2.0, key)),
        new("i-120x80", ExtrusionRole.Structural, AluminumGrade.A6082T6, ExtrusionForm.Profile, 5.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.IShape.Of(depthMm: 120.0, widthMm: 80.0, webMm: 5.0, flangeMm: 5.0, filletMm: 6.0, flangeToeMm: 5.0, key)),
        new("tube-60x6", ExtrusionRole.Structural, AluminumGrade.A6082T6, ExtrusionForm.Tube, 6.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.CircleHollow.Of(diameterMm: 60.0, wallMm: 6.0, key)),
        new("angle-60x6", ExtrusionRole.Structural, AluminumGrade.A6061T6, ExtrusionForm.Profile, 6.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.Angle.Of(depthMm: 60.0, widthMm: 60.0, thicknessMm: 6.0, filletMm: 4.0, legToeMm: 6.0, key)),
        new("sheet-3", ExtrusionRole.Panel, AluminumGrade.A5083, ExtrusionForm.Sheet, 3.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.Rectangle.Of(widthMm: 1200.0, depthMm: 3.0, key))];

    // Fail-loud: ONE Traverse over the roster — a band miss, a profile rejection, or a Component.Of refusal ABORTS
    // the catalogue build. The Context parameter is the ComponentFamily.Rows delegate contract; this seed reads no
    // context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        toSeq(Dies).Traverse(RowOf).As();

    static Fin<ComponentRow> RowOf(DieRow die) {
        Op key = Op.Of(name: $"aluminum.{die.Designation}");
        return
            from banded in die.Grade.Strengths(die.Form, die.ElementMm, key)
            from profile in die.Build(key)
            from detail in AluminumDetail.Of(die)
            from item in Component.Of(
                ComponentFamily.Aluminum, $"aluminum.{die.Designation}", profile,
                die.Role.Ifc, Coring.None, En,
                substanceId: die.Grade.Substance, appearanceId: Render, detail: Some(detail), key)
            select new ComponentRow(item, Provenance.Authored);
    }

    // The railed designation-keyed die join — SeedJoin per the component#CATALOGUE law, so a malformed die
    // designation lands typed on the ComponentFault rail instead of a TypeInitializationException from a throwing
    // static ComponentId.Create the composition root cannot attribute.
    static readonly Lazy<Fin<FrozenDictionary<ComponentId, DieRow>>> Table =
        SeedJoin.Of(toSeq(Dies), static die => $"aluminum.{die.Designation}");

    public static Fin<DieRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    // The ComponentFamily.Aluminum CAPACITY producer: every die is Sectioned, so an unresolved section is a
    // catalogue defect railed here. The banded (fo, fu) pair, the buckling-class letter riding the grade, and the
    // section receipt cross WHOLE onto the capacity#SECTION_CAPACITY EN 1999 member arm — the curve constants and
    // the verdict are that arm's, the data this page's, so no aluminum die is ever priced through a steel curve.
    // The jurisdiction is PROVEN, not merely threaded: en1999 is the one basis whose aluminium bands exist (the seed
    // proved the EN band before any lift), so a placement declaring any other basis refuses typed here — the
    // AnchorBed posture, a mis-jurisdictioned die unrepresentable rather than mis-priced through a foreign kernel.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from solved in section.ToFin(ComponentFault.Section(key, $"<aluminum-section-unresolved:{component.Designation.Value}>"))
        from based in guard(placement.Basis == DesignBasis.En1999,
            ComponentFault.Capacity(key, $"<aluminum-basis-unrealized:{placement.Basis.Key}:{component.Designation.Value}>"))
        from die in Resolve(component, key)
        from banded in die.Grade.Strengths(die.Form, die.ElementMm, key)
        select SectionCapacity.Lift(new CapacityReceipt.Aluminum(
            component.Designation, die.Grade, die.Form, banded.FoMpa, banded.FuMpa, solved, placement.Basis));
}
```

## [03]-[RESEARCH]

(none)
