# [MATERIALS_CONCRETE]

THE CAST-IN-PLACE CONCRETE SEED FAMILY and THE EXPOSURE-DRIVEN COVER REGIME. A CIP concrete member TYPE is one `ComponentRow` the ONE `component#COMPONENT_SEED` generator mints from `ConcreteSeed.Roster` under `ConcreteSeed.Law` over the concrete policy row (`ComponentClass.Primary`, `DetailLane.Realization`, admits `SectionProfile.Rectangle` or `SectionProfile.Circle`, cross-nominal the section depth) — never a `ConcreteBeam`/`ConcreteColumn` type and never a hand-keyed strength literal. The row axes are GRADE × MEMBER ROLE: the strength class is a `component#MATERIAL_GRADE` `MaterialGrade` row whose `GradeProperties.Concrete` arm binds the EN 1992-1-1 Table 3.1 cylinder/cube pair, its `VividOrange` `EnConcreteGrade`, and its printed mix designation, and `ConcreteRole` carries each role's CONCRETE IFC4 leaf (`IfcBeam`/`IfcColumn`/`IfcWall`/`IfcSlab`/`IfcFooting`/`IfcPile`), its profile shape, and its ACI cover condition — so a new member type is one `MemberRow` over the two axes and a new grade or role is one row on its axis, never a central edit. The realization bag stamps the four Element-declared concrete rows (`ConcreteCover`/`MixDesignation`/`ExposureClass`/`CastMethod`) at seed time, and the durability cover that fills `ConcreteCover` is DERIVED from the member's declared exposure through the EN 1992-1-1 Table 4.4N grid this page owns — a cover is a computed regime read, never a per-row literal.

`[04]-[RC_ENTRY]` is where this family meets the capacity pipeline: `ConcreteRc.Assemble` resolves a seed member, derives its bond-governed nominal cover from the SAME regime that stamped its bag, gates the probe-proven engine limit (interaction FACE layers are polygon-only — a `Circle`-profiled member admits only the perimeter and placed layout cases, typed at THIS boundary, never an engine throw), and hands the member to `reinforcement#RC_SECTION` `RcSectionBuilder.Of`/`Capacity` — the ONE reinforced-section boundary, which already admits ANY `Component` as its concrete outline. The family's own `Capacity` column is therefore an EXPLICIT TYPED REFUSAL naming that route: a bare concrete section carries no `SectionCapacity` case of its own (`capacity#SECTION_CAPACITY` `RcInteraction`/`RcElastic` are built FROM an `RcSection`), so the refusal is what keeps the RC entry the only door. The page composes `component#COMPONENT_OWNER` (`Component`/`ComponentRow`/`SectionProfile`/`IfcBinding`/`ComponentStandard`/`SeedJoin`/`ComponentDetail`), the `Rasm.Element` `DetailSchema` concrete rows, and the `VividOrange.Materials` EN grade surface through the reinforcement page's `EnGrade` boundary — the `EnConcreteFactory` linear-elastic E is fck/0.00175 (a secant design line, decompile-verified), NOT Ecm, so contract stiffness always rides the substance catalogue row the grade links, never a factory read.

## [01]-[INDEX]

- [02]-[CONCRETE_FAMILY]: the `GradeProperties.Concrete` Table 3.1 generator, the `ConcreteRole` member-role axis with its per-role IFC4 leaf and ACI cover condition, the `MemberRow` realized selection, the seed-time `ConcreteDetail` realization bag, and the `ConcreteSeed.Roster`/`Law`/`Resolve`/`Capacity` set the concrete policy row binds.
- [03]-[COVER_REGIME]: `StructuralClass` S1–S6 with the Table 4.3N adjustment, the `ExposureToken` EN 1992-1-1 Table 4.4N c_min,dur grid (XD3/XS3 rows S1–S5 typed-absent on a source conflict), `EnCover.Nominal` the §4.4.1 c_nom fold, the `AciCondition` ACI 318-19 Table 20.5.1.3.1 specified-cover table, and the `StrengthCorrespondence` advisory psi↔EN-class rows.
- [04]-[RC_ENTRY]: `ConcreteRc.Assemble`/`Capacity` — the seed-member-to-`RcSection` composition proven end to end over `reinforcement#RC_SECTION` `RcSectionBuilder`, with the circular-member perimeter-layout law.
- [05]-[SEISMIC_SYSTEMS]: the `AsceRcSystem` ASCE 7 Table 12.2-1 R/Ω0/Cd rows for the concrete systems and the `EnConcreteDuctility` EN 1998-1 behaviour-factor rows, proven cells only.

## [02]-[CONCRETE_FAMILY]

- Owner: the `GradeProperties.Concrete` partial member the Table 3.1 mean-strength generator lands on; `ConcreteRole` the member-role `[SmartEnum<string>]` policy axis; `MemberRow` the realized-selection row; `ConcreteDetail` the seed-time realization-bag constructor; `ConcreteSeed` the roster, the seed law, the `SeedJoin`-backed `Resolve`, and the typed `Capacity` refusal the concrete `ComponentFamily` policy row binds.
- Cases: grade {the nine `ComponentFamily.Concrete` `MaterialGrade` rows — the EN 1992-1-1 Table 3.1 classes the evidence pack confirms, each carrying its `EnConcreteGrade`, its printed cylinder/cube pair, its mix token, and its `concrete.<class>` substance id} × role {beam · column · round-column · wall · slab · footing · pile — each carrying its CONCRETE IFC4 leaf from the verified IFC leaf roster, its round/rectangular profile shape, its ACI cover condition, and its cast-method token} — a member type is one `MemberRow` naming both axes with its gross dimensions and exposure; wall and slab rows state the one-metre design strip (`WidthMm = 1000`), so their cross-section is the strip the role convention fixes and the cross-nominal depth reads the thickness.
- Entry: `ComponentSeed.Rows(context, ConcreteSeed.Roster, ConcreteSeed.Law)` — this page states the roster and the policy, never the fold; the law's detail arm derives each row's durability cover through `EnCover.Nominal` off its declared exposure and structural class, so the bag is built where the regime is read. `ConcreteSeed.Resolve(Component, Op)` is the `SeedJoin` designation join restoring the typed `MemberRow` axes for the RC entry.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`), Rasm.Element (project — `MaterialId`, `EvidenceGrade`, `PropertyBag`, the `DetailSchema.ConcreteCover`/`MixDesignation`/`ExposureClass`/`CastMethod` realization rows this bag stamps — Element-declared at `Rasm.Element/Properties/property#DETAIL_SCHEMA`, never minted here), VividOrange.Materials (`EnConcreteGrade` the grade binding; the factory E divergence stated on the arm — `.api/api-vividorange-materials.md`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` owners, `component#MATERIAL_GRADE` `MaterialGrade`/`GradeProperties`, `component#COMPONENT_SEED` `SeedLaw`/`ComponentSeed`, and the `reinforcement#RC_SECTION` entry), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`ImmutableArray`, collection expressions).
- Growth: a new strength class is one `MaterialGrade` concrete row bound to its `EnConcreteGrade` and substance id (a class the enum lacks stays out until the package publishes it); a new role is one `ConcreteRole` row carrying its OWN roster-verified IFC leaf and cover condition; a new member type is one `MemberRow`; a new cast method is a token on the row — never a per-role type, never a `ComponentFamily` edit, never a hand-keyed Ecm beside the substance row that owns it.
- Boundary: the grade arm carries fck and the printed cube twin and derives fcm by the standard's own generator — it carries NO Ecm column, because the `concrete.<class>` substance row (`Properties/properties#MATERIAL_PROPERTY_CATALOGUE`) owns the mean modulus and the `EnConcreteFactory` linear-elastic E is the fck/0.00175 secant design line (decompile-verified), so a stiffness read here shadows one owner or imports the wrong one. US strength grades ride the `[03]` correspondence rows as ADVISORY data — `ACI318ConcreteGrade` enum rows exist but every non-EN factory arm throws (probe-confirmed), so no US grade reaches a factory. Member dimensions are this repo's realized selection and seed under `EvidenceGrade.User`; the grade columns transcribe the print. `ComponentAuthority` publishes no ACI row, so the US cover table cites its clause in place and the seed rows stand EN-bodied under `ComponentAuthority.En`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Standards.Eurocode;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConcreteRole {
    public static readonly ConcreteRole Beam        = new("beam",         IfcBinding.Of("IfcBeam", "BEAM"),           round: false, strip: false, AciCondition.InteriorBeamColumn, cast: "cast-in-place");
    public static readonly ConcreteRole Column      = new("column",       IfcBinding.Of("IfcColumn", "COLUMN"),       round: false, strip: false, AciCondition.InteriorBeamColumn, cast: "cast-in-place");
    public static readonly ConcreteRole RoundColumn = new("round-column", IfcBinding.Of("IfcColumn", "COLUMN"),       round: true,  strip: false, AciCondition.InteriorBeamColumn, cast: "cast-in-place");
    public static readonly ConcreteRole Wall        = new("wall",         IfcBinding.Of("IfcWall", "SOLIDWALL"),      round: false, strip: true,  AciCondition.InteriorSlabWall,   cast: "cast-in-place");
    public static readonly ConcreteRole Slab        = new("slab",         IfcBinding.Of("IfcSlab", "FLOOR"),          round: false, strip: true,  AciCondition.InteriorSlabWall,   cast: "cast-in-place");
    public static readonly ConcreteRole Footing     = new("footing",      IfcBinding.Of("IfcFooting", "PAD_FOOTING"), round: false, strip: true,  AciCondition.CastAgainstGround,  cast: "cast-in-place");
    public static readonly ConcreteRole Pile        = new("pile",         IfcBinding.Of("IfcPile", "BORED"),          round: true,  strip: false, AciCondition.CastAgainstGround,  cast: "tremie");
    public IfcBinding Ifc { get; }
    public bool Round { get; }
    public bool Strip { get; }
    public AciCondition Aci { get; }
    public string Cast { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public partial record GradeProperties {
    public sealed partial record Concrete {
        public double FcmMpa => FckMpa + 8.0;
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Concrete> ConcreteArm => Columns is GradeProperties.Concrete arm ? Some(arm) : None;
}

public readonly record struct MemberRow(ConcreteRole Role, MaterialGrade Grade, double WMm, double DMm, ExposureToken Exposure, StructuralClass Class) {
    public string Designation =>
        Role.Round
            ? $"concrete.{Grade.Key}-{Role.Key}-d{DMm:0}"
            : $"concrete.{Grade.Key}-{Role.Key}-{WMm:0}x{DMm:0}";
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ConcreteDetail {
    public static Fin<PropertyBag> Of(MemberRow row, GradeProperties.Concrete grade, double coverMm, EvidenceGrade source, Op key) =>
        from joint in ComponentDetail.Joint("Cast", key)
        from cover in ComponentDetail.Measured(DetailSchema.ConcreteCover, Dimension.LengthDim, coverMm * 1e-3)
        select ComponentDetail.RealizationRows(
            joint,
            cover,
            ComponentDetail.Token(DetailSchema.MixDesignation, grade.MixToken),
            ComponentDetail.Token(DetailSchema.ExposureClass, row.Exposure.Key),
            ComponentDetail.Token(DetailSchema.CastMethod, row.Role.Cast),
            ComponentDetail.Sourced(source));
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class ConcreteSeed {
    public static readonly Seq<MemberRow> Roster = Seq(
        new MemberRow(ConcreteRole.Beam,        MaterialGrade.C25_30, 300.0,  500.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Beam,        MaterialGrade.C30_37, 300.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Beam,        MaterialGrade.C35_45, 400.0,  800.0, ExposureToken.Xc4, StructuralClass.S4),
        new MemberRow(ConcreteRole.Column,      MaterialGrade.C30_37, 400.0,  400.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Column,      MaterialGrade.C50_60, 500.0,  500.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Column,      MaterialGrade.C70_85, 600.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.RoundColumn, MaterialGrade.C40_50, 600.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Wall,        MaterialGrade.C30_37, 1000.0, 250.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Wall,        MaterialGrade.C35_45, 1000.0, 300.0, ExposureToken.Xc4, StructuralClass.S4),
        new MemberRow(ConcreteRole.Slab,        MaterialGrade.C25_30, 1000.0, 200.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Slab,        MaterialGrade.C30_37, 1000.0, 250.0, ExposureToken.Xc1, StructuralClass.S4),
        new MemberRow(ConcreteRole.Footing,     MaterialGrade.C25_30, 1000.0, 500.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new MemberRow(ConcreteRole.Footing,     MaterialGrade.C30_37, 1000.0, 700.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new MemberRow(ConcreteRole.Pile,        MaterialGrade.C30_37, 600.0,  600.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new MemberRow(ConcreteRole.Pile,        MaterialGrade.C35_45, 900.0,  900.0, ExposureToken.Xc2Xc3, StructuralClass.S4));

    public static readonly SeedLaw<MemberRow> Law = SeedLaw<MemberRow>.Of(
        family: ComponentFamily.Concrete,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: static (row, key) => row.Role.Round
            ? SectionProfile.Circle.Of(row.DMm, key)
            : SectionProfile.Rectangle.Of(row.WMm, row.DMm, key),
        substance: static row => row.Grade.Substance,
        source: static _ => EvidenceGrade.User,
        standard: static row => new ComponentStandard(row.Grade.Authority.Region, StandardJointThicknessMm: 0.0, row.Grade.Authority),
        detail: Some<Func<MemberRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        ifc: static row => row.Role.Ifc);

    static Validation<Error, Unit> Coherence(MemberRow row, Op key) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(
                row.Grade.Family == ComponentFamily.Concrete,
                new ComponentFault.GradeFamilyMismatch(key, row.Grade, ComponentFamily.Concrete)),
            AdmissionSlots.Gate(
                row.Grade.ConcreteArm.IsSome,
                new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Concrete)),
            AdmissionSlots.Gate(
                !row.Role.Strip || row.WMm == DesignStripMm,
                new KernelFault.InvalidValue(nameof(row.WMm), "the declared one-metre design strip width", Some(key)))));

    const double DesignStripMm = 1000.0;

    static Fin<PropertyBag> Detail(MemberRow row, SectionProfile profile, Op key) =>
        from grade in row.Grade.ConcreteArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Concrete))
        from cover in EnCover.Nominal(row.Exposure, row.Class, barDiameterMm: 0.0, key)
        from bag in ConcreteDetail.Of(row, grade, cover, EvidenceGrade.User, key)
        select bag;

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, MemberRow>>> Table =
        SeedJoin.Of(Roster, static row => row.Designation);

    public static Fin<MemberRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [03]-[COVER_REGIME]

- Owner: `StructuralClass` the EN 1992-1-1 §4.4.1 S1–S6 axis with the Table 4.3N adjustment; `ExposureToken` the Table 4.4N COLUMN axis carrying the c_min,dur grid (each row naming the EN 206 classes it covers); `EnCover` the §4.4.1 c_nom fold; `AciCondition` the ACI 318-19 Table 20.5.1.3.1 specified-cover rows; `StrengthCorrespondence` the advisory psi↔EN-class map.
- Cases: EN grid {S1..S6} × {X0 · XC1 · XC2/XC3 · XC4 · XD1/XS1 · XD2/XS2 · XD3/XS3} — every cell the primary text's printed value, the XD3/XS3 column carrying S6 = 55 corroborated and S1–S5 TYPED-ABSENT (the primary conflicts with the SCIA variant grid, so those five cells refuse rather than pick a side); XA classes carry NO cover column by §4.4.1.2(12) — chemical attack is handled through concrete composition (`Properties/properties#MIX_PROPORTION` `ExposureClass` owns those EN 206 Annex F floors), so an XA read is a typed refusal naming that owner. ACI rows {cast-against-ground 75 · exposed 50/38 by bar band · interior slab/wall 19/38 · interior beam/column 38 · shell 19/13} — the SI-edition exact-conversion values.
- Law: `c_nom = c_min + Δc_dev` with `c_min = max(c_min,b; c_min,dur; 10)` (Exprs. 4.1/4.2) — Δc_dev the code-default 10 mm, Δc_dur,γ the code-default 0, c_min,b the bar diameter the caller supplies (0 at seed time, so the bond term can only raise a cover later, never lower it). This axis is NOT the `Properties/properties#MIX_PROPORTION` `ExposureClass` axis: that owner keys EN 206 Annex F durability FLOORS (w/c, cement, strength) per class; this owner keys the Table 4.4N cover COLUMNS, which group classes — the bag token is the join between them.
- Boundary: every numeric cell transcribes the pack's two-sourced print; the correspondence rows are ADVISORY BY CONSTRUCTION (f'c ~9% fractile vs fck 5% fractile — both sources flag it), seeded as guidance data no admission consumes; the 8000/10000 psi mappings are single-sourced and stay off the roster.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StructuralClass {
    public static readonly StructuralClass S1 = new("s1", ordinal: 0);
    public static readonly StructuralClass S2 = new("s2", ordinal: 1);
    public static readonly StructuralClass S3 = new("s3", ordinal: 2);
    public static readonly StructuralClass S4 = new("s4", ordinal: 3);
    public static readonly StructuralClass S5 = new("s5", ordinal: 4);
    public static readonly StructuralClass S6 = new("s6", ordinal: 5);
    public int Ordinal { get; }

    static readonly Seq<StructuralClass> Ladder = toSeq(Items.OrderBy(static row => row.Ordinal));
    static readonly int FloorOrdinal = Items.Min(static row => row.Ordinal);
    static readonly int CeilingOrdinal = Items.Max(static row => row.Ordinal);

    public StructuralClass Adjusted(bool designLife100Years, bool higherStrengthClass) =>
        Ladder.Filter(row => row.Ordinal <= Math.Clamp(
                Ordinal + (designLife100Years ? 2 : 0) - (higherStrengthClass ? 1 : 0), FloorOrdinal, CeilingOrdinal))
            .Last.IfNone(this);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExposureToken {
    public static readonly ExposureToken X0     = new("X0",      classes: ["X0"],           cover: [Some(10.0), Some(10.0), Some(10.0), Some(10.0), Some(15.0), Some(20.0)]);
    public static readonly ExposureToken Xc1    = new("XC1",     classes: ["XC1"],          cover: [Some(10.0), Some(10.0), Some(10.0), Some(15.0), Some(20.0), Some(25.0)]);
    public static readonly ExposureToken Xc2Xc3 = new("XC2/XC3", classes: ["XC2", "XC3"],   cover: [Some(10.0), Some(15.0), Some(20.0), Some(25.0), Some(30.0), Some(35.0)]);
    public static readonly ExposureToken Xc4    = new("XC4",     classes: ["XC4"],          cover: [Some(15.0), Some(20.0), Some(25.0), Some(30.0), Some(35.0), Some(40.0)]);
    public static readonly ExposureToken Xd1Xs1 = new("XD1/XS1", classes: ["XD1", "XS1"],   cover: [Some(20.0), Some(25.0), Some(30.0), Some(35.0), Some(40.0), Some(45.0)]);
    public static readonly ExposureToken Xd2Xs2 = new("XD2/XS2", classes: ["XD2", "XS2"],   cover: [Some(25.0), Some(30.0), Some(35.0), Some(40.0), Some(45.0), Some(50.0)]);
    public static readonly ExposureToken Xd3Xs3 = new("XD3/XS3", classes: ["XD3", "XS3"],   cover: [None, None, None, None, None, Some(55.0)]);
    public ImmutableArray<string> Classes { get; }
    public ImmutableArray<Option<double>> Cover { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AciCondition {
    public static readonly AciCondition CastAgainstGround  = new("cast-against-ground",  largeMm: 75.0, smallMm: 75.0, thresholdMm: 0.0);
    public static readonly AciCondition ExposedToWeather   = new("exposed-to-weather",   largeMm: 50.0, smallMm: 38.0, thresholdMm: 19.05);
    public static readonly AciCondition InteriorSlabWall   = new("interior-slab-wall",   largeMm: 38.0, smallMm: 19.0, thresholdMm: 43.002);
    public static readonly AciCondition InteriorBeamColumn = new("interior-beam-column", largeMm: 38.0, smallMm: 38.0, thresholdMm: 0.0);
    public static readonly AciCondition Shell              = new("shell",                largeMm: 19.0, smallMm: 13.0, thresholdMm: 19.05);
    public double LargeMm { get; }
    public double SmallMm { get; }
    public double ThresholdMm { get; }

    public double SpecifiedMm(double barDiameterMm) => barDiameterMm >= ThresholdMm && ThresholdMm > 0.0 ? LargeMm : ThresholdMm > 0.0 ? SmallMm : LargeMm;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StrengthCorrespondence(int Psi, double Mpa, string NearestEnClass);

public static class StrengthMap {
    public static readonly ImmutableArray<StrengthCorrespondence> Rows = [
        new(2500, 17.2, "C16/20"), new(3000, 20.7, "C20/25"), new(4000, 27.6, "C28/35"),
        new(5000, 34.5, "C35/45"), new(6000, 41.4, "C40/50")];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EnCover {
    const double DeviationMm = 10.0;
    const double FloorMm = 10.0;

    public static Fin<double> Nominal(ExposureToken exposure, StructuralClass structural, double barDiameterMm, Op key) =>
        from bar in guard(double.IsFinite(barDiameterMm) && barDiameterMm >= 0.0,
            new KernelFault.OutOfRange(nameof(barDiameterMm), barDiameterMm, "finite and non-negative", Some(key)))
        from dur in exposure.Cover[structural.Ordinal].ToFin(
            new ComponentFault.CoverCellMissing(key, exposure, structural))
        select Math.Max(Math.Max(barDiameterMm, dur), FloorMm) + DeviationMm;
}
```

## [04]-[RC_ENTRY]

- Owner: `ConcreteRc` — the concrete-side composition of the `reinforcement#RC_SECTION` boundary: `Assemble` resolves a SEED member and hands it to `RcSectionBuilder.Of` under the cover this page's regime derives, and `Capacity` the same composition through `RcSectionBuilder.Capacity` onto `capacity#SECTION_CAPACITY` `SectionCapacity.Resolve` — the end-to-end proof that a catalogued CIP member reaches `RcInteraction`/`RcElastic` with zero bespoke assembly.
- Law: the probe-proven engine limit gates HERE, typed: `FaceReinforcementLayer` is POLYGON-ONLY — fed a `Circle` profile it throws `InvalidProfileTypeException` — while perimeter and placed layouts work on circles; a face case fails `FaceLayoutUnsupported` here.
- Entry: `ConcreteRc.Assemble(member, barGrade, link, layout, annex, key)` derives cover from the member's exposure; `Capacity(..., intent, ...)` accepts only `RcCapacityIntent.Hull` or `.Elastic`, so non-RC build variants cannot enter this route.
- Boundary: this owner derives NO section math and admits NO VividOrange surface — grade lowering, layer construction, the transformed-section carrier, and the capacity solve all stay behind `RcSectionBuilder`/`SectionCapacity`; the concrete contribution is exactly the member, the cover law, and the layout admissibility its own profile decides.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ConcreteRc {
    public static Fin<RcSection> Assemble(Component member, MaterialGrade barGrade, BarRow link, Seq<RebarLayout> layout, NationalAnnex annex, Op key) =>
        from row in ConcreteSeed.Resolve(member, key)
        from admitted in Admissible(member, layout, key)
        from grade in row.Grade.ConcreteArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Concrete))
        from cover in EnCover.Nominal(row.Exposure, row.Class, LargestBarMm(link, layout), key)
        from section in RcSectionBuilder.Of(member, grade.En, barGrade, link, layout, cover, annex, key)
        select section;

    public static Fin<SectionCapacity> Capacity(Component member, MaterialGrade barGrade, BarRow link, Seq<RebarLayout> layout, RcCapacityIntent intent, CapacityPlacement placement, Op key) =>
        from row in ConcreteSeed.Resolve(member, key)
        from admitted in Admissible(member, layout, key)
        from grade in row.Grade.ConcreteArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Concrete))
        from cover in EnCover.Nominal(row.Exposure, row.Class, LargestBarMm(link, layout), key)
        from capacity in RcSectionBuilder.Capacity(member, grade.En, barGrade, link, layout, cover, intent, placement, key)
        select capacity;

    static Fin<Unit> Admissible(Component member, Seq<RebarLayout> layout, Op key) =>
        guard(member.Profile is not SectionProfile.Circle
                || layout.ForAll(static item => item is not (RebarLayout.FaceCount or RebarLayout.FaceSpacing)),
            new ComponentFault.FaceLayoutUnsupported(key, typeof(SectionProfile.Circle))).ToFin();

    static double LargestBarMm(BarRow link, Seq<RebarLayout> layout) =>
        Math.Max(link.NominalDiameterMm, layout.Map(static item => item.Switch(
            faceCount:        static c => c.Bar.NominalDiameterMm,
            faceSpacing:      static s => s.Bar.NominalDiameterMm,
            perimeterCount:   static c => c.Bar.NominalDiameterMm,
            perimeterSpacing: static s => s.Bar.NominalDiameterMm,
            placed:           static p => p.Bar.NominalDiameterMm)).Fold(0.0, Math.Max));
}
```

## [05]-[SEISMIC_SYSTEMS]

- Owner: `AsceRcSystem` the ASCE 7 Table 12.2-1 seismic design-coefficient rows for the CONCRETE force-resisting systems; `EnConcreteDuctility` the EN 1998-1 Table 5.1 behaviour-factor rows — both under `SEED_ROW_LAW` with only the pack's TWO-SOURCED cells carried and every unproven cell typed-absent.
- Cases: ASCE {special/ordinary RC bearing walls · special/ordinary RC building-frame walls · special/intermediate/ordinary RC moment frames · the two SMF-dual wall rows} each carrying its R/Ω0/Cd triple verbatim; EN {DCL the q ≤ 1.5 overstrength-only cap · DCM frame/dual/coupled-wall q0 = 3.0} — the DCH column, the uncoupled-wall and torsionally-flexible rows, and every αu/α1 default are standard-text-only in the pack and land ABSENT, so a DCH read refuses rather than transcribes an uncorroborated cell; the elevation-irregularity reduction (q0 × 0.8) is the one two-sourced modifier and rides the owner as its constant.
- Boundary: these are SYSTEM-level design coefficients a lateral-system selection reads — no member check consumes them and no `SectionCapacity` case carries them; the DEMAND-side consumer is the `Rasm.Compute` `Analysis/capacity#SEISMIC_ROUTE` `SpectrumPolicy.Behavior` divisor, which takes the EN q (the `EnConcreteDuctility` q0 under its αu/α1 and elevation modifiers, floored at `QFloor`) or the ASCE R over Ie (the `AsceRcSystem.R` column) as a SCALAR at composition because the branch strata forbid a reference in either direction — the lateral-system selection resolves the row once per engagement and threads the number. Ω0 and Cd are the row's amplification companions with NO Compute arm yet: Ω0 amplifies connected-element force demands and Cd elastic drifts, and no overstrength-combination or drift-amplification fold exists at the member route — the columns stand as system-selection data until that consumer lands, never silently folded into `Behavior`. The SDC permission/height columns of Table 12.2-1 stay untranscribed (out of pack scope); member-level seismic DETAILING (hoop spacing, confinement ratios) has no proven row in the pack and lands nowhere on this page.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
public readonly record struct AsceRcSystem(string Key, double R, double Omega0, double Cd);

public static class AsceRcSystems {
    public static readonly ImmutableArray<AsceRcSystem> Rows = [
        new("bearing-special-rc-shear-wall",  5.0,  2.5, 5.0),
        new("bearing-ordinary-rc-shear-wall", 4.0,  2.5, 4.0),
        new("frame-special-rc-shear-wall",    6.0,  2.5, 5.0),
        new("frame-ordinary-rc-shear-wall",   5.0,  2.5, 4.5),
        new("special-rc-moment-frame",        8.0,  3.0, 5.5),
        new("intermediate-rc-moment-frame",   5.0,  3.0, 4.5),
        new("ordinary-rc-moment-frame",       3.0,  3.0, 2.5),
        new("dual-smf-special-rc-wall",       7.0,  2.5, 5.5),
        new("dual-smf-ordinary-rc-wall",      6.0,  2.5, 5.0)];
}

public readonly record struct EnConcreteDuctility(string Key, Option<double> Q0, bool AlphaRatioApplies);

public static class EnConcreteDuctilities {
    public const double IrregularElevationFactor = 0.8;
    public const double QFloor = 1.5;

    public static readonly ImmutableArray<EnConcreteDuctility> Rows = [
        new("dcl",             Some(1.5), AlphaRatioApplies: false),
        new("dcm-frame-dual",  Some(3.0), AlphaRatioApplies: true),
        new("dch-frame-dual",  None,      AlphaRatioApplies: true)];
}
```

## [06]-[RESEARCH]

(none)
