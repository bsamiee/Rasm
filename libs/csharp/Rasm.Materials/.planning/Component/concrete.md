# [MATERIALS_CONCRETE]

THE CAST-IN-PLACE CONCRETE SEED FAMILY and THE EXPOSURE-DRIVEN COVER REGIME. A CIP concrete member TYPE is one `ComponentRow` minted by the ONE generator `ConcreteSeed.Rows -> Component.Of` over the concrete policy row (`ComponentClass.Primary`, `DetailLane.Realization`, admits `SectionProfile.Rectangle` or `SectionProfile.Circle`, cross-nominal the section depth) — never a `ConcreteBeam`/`ConcreteColumn` type and never a hand-keyed strength literal. The row axes are GRADE × MEMBER ROLE: `ConcreteGradeRow` binds each EN 1992-1-1 Table 3.1 strength class to its `VividOrange` `EnConcreteGrade` and its `concrete.<class>` substance row, and `ConcreteRole` carries each role's CONCRETE IFC4 leaf (`IfcBeam`/`IfcColumn`/`IfcWall`/`IfcSlab`/`IfcFooting`/`IfcPile`), its profile shape, and its ACI cover condition — so a new member type is one `MemberRow` over the two axes and a new grade or role is one row on its axis, never a central edit. The realization bag stamps the four Element-declared concrete rows (`ConcreteCover`/`MixDesignation`/`ExposureClass`/`CastMethod`) at seed time, and the durability cover that fills `ConcreteCover` is DERIVED from the member's declared exposure through the EN 1992-1-1 Table 4.4N grid this page owns — a cover is a computed regime read, never a per-row literal.

`[04]-[RC_ENTRY]` is where this family meets the capacity rail: `ConcreteRc.Assemble` resolves a seed member, derives its bond-governed nominal cover from the SAME regime that stamped its bag, gates the probe-proven engine limit (interaction FACE layers are polygon-only — a `Circle`-profiled member admits only the perimeter and placed layout cases, typed at THIS boundary, never an engine throw), and hands the member to `reinforcement#RC_SECTION` `RcSectionBuilder.Of`/`Capacity` — the ONE reinforced-section boundary, which already admits ANY `Component` as its concrete outline. The family's own `Capacity` column is therefore an EXPLICIT TYPED REFUSAL naming that route: a bare concrete section carries no `SectionCapacity` case of its own (`capacity#SECTION_CAPACITY` `RcInteraction`/`RcElastic` are built FROM an `RcSection`), so the refusal is what keeps the RC entry the only door. The page composes `component#COMPONENT_OWNER` (`Component`/`ComponentRow`/`SectionProfile`/`IfcBinding`/`ComponentStandard`/`SeedJoin`/`ComponentDetail`), the `Rasm.Element` `DetailSchema` concrete rows, and the `VividOrange.Materials` EN grade surface through the reinforcement page's `EnGrade` boundary — the `EnConcreteFactory` linear-elastic E is fck/0.00175 (a secant design line, decompile-verified), NOT Ecm, so seam stiffness always rides the substance catalogue row the grade links, never a factory read.

## [01]-[INDEX]

- [02]-[CONCRETE_FAMILY]: the `ConcreteGradeRow` EN-class table with its `EnConcreteGrade` and substance bindings, the `ConcreteRole` member-role axis with its per-role IFC4 leaf and ACI cover condition, the `MemberRow` realized selection, the seed-time `ConcreteDetail` realization bag, and the fail-loud `ConcreteSeed.Rows`/`Resolve`/`Capacity` fold the concrete policy row binds.
- [03]-[COVER_REGIME]: `StructuralClass` S1–S6 with the Table 4.3N adjustment, the `ExposureToken` EN 1992-1-1 Table 4.4N c_min,dur grid (XD3/XS3 rows S1–S5 typed-absent on a source conflict), `EnCover.Nominal` the §4.4.1 c_nom fold, the `AciCondition` ACI 318-19 Table 20.5.1.3.1 specified-cover table, and the `StrengthCorrespondence` advisory psi↔EN-class rows.
- [04]-[RC_ENTRY]: `ConcreteRc.Assemble`/`Capacity` — the seed-member-to-`RcSection` composition proven end to end over `reinforcement#RC_SECTION` `RcSectionBuilder`, with the circular-member perimeter-layout law.
- [05]-[SEISMIC_SYSTEMS]: the `AsceRcSystem` ASCE 7 Table 12.2-1 R/Ω0/Cd rows for the concrete systems and the `EnConcreteDuctility` EN 1998-1 behaviour-factor rows, proven cells only.

## [02]-[CONCRETE_FAMILY]

- Owner: `ConcreteGradeRow` the tier-3 strength-class row currency with `ConcreteGrades` the frozen table; `ConcreteRole` the member-role `[SmartEnum<string>]` policy axis; `MemberRow` the realized-selection row; `ConcreteDetail` the seed-time realization-bag constructor; `ConcreteSeed` the `Rows` fold, the `SeedJoin`-backed `Resolve`, and the typed `Capacity` refusal the concrete `ComponentFamily` policy row binds.
- Cases: grade {c25_30 · c30_37 · c35_45 · c40_50 · c45_55 · c50_60 · c55_67 · c60_75 · c70_85 — the EN 1992-1-1 Table 3.1 rows the evidence pack confirms, each bound to its `EnConcreteGrade` and its `concrete.<class>` substance id} × role {beam · column · round-column · wall · slab · footing · pile — each carrying its CONCRETE IFC4 leaf from the verified IFC leaf roster, its round/rectangular profile shape, its ACI cover condition, and its cast-method token} — a member type is one `MemberRow` naming both axes with its gross dimensions and exposure; wall and slab rows state the one-metre design strip (`WidthMm = 1000`), so their cross-section is the strip the role convention fixes and the cross-nominal depth reads the thickness.
- Entry: `public static Fin<Seq<ComponentRow>> ConcreteSeed.Rows(Context context)` traverses `Members` through the common `Component.Of` rail: each row derives its durability cover through `EnCover.Nominal` off its declared exposure and structural class, builds its railed `Rectangle`/`Circle` profile, stamps its role's IFC leaf, and constructs the bag — one malformed row ABORTS the catalogue. `ConcreteSeed.Resolve(Component, Op)` is the `SeedJoin` designation join restoring the typed `MemberRow` axes for the RC entry.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`), Rasm.Element (project — `MaterialId`, `PropertyBag`, the `DetailSchema.ConcreteCover`/`MixDesignation`/`ExposureClass`/`CastMethod` realization rows this bag stamps — Element-declared at `Rasm.Element/Properties/property#DETAIL_SCHEMA`, never minted here), VividOrange.Materials (`EnConcreteGrade` the grade binding; the factory E divergence stated on the row — `.api/api-vividorange-materials.md`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` owners and the `reinforcement#RC_SECTION` entry), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`ImmutableArray`, collection expressions).
- Growth: a new strength class is one `ConcreteGradeRow` bound to its `EnConcreteGrade` and substance id (a class the enum lacks stays out until the package publishes it); a new role is one `ConcreteRole` row carrying its OWN roster-verified IFC leaf and cover condition; a new member type is one `MemberRow`; a new cast method is a token on the row — never a per-role type, never a `ComponentFamily` edit, never a hand-keyed Ecm beside the substance row that owns it.
- Boundary: the grade row carries fck and the printed cube twin and derives fcm by the standard's own generator — it carries NO Ecm column, because the `concrete.<class>` substance row (`Properties/properties#MATERIAL_PROPERTY_CATALOGUE`) owns the mean modulus and the `EnConcreteFactory` linear-elastic E is the fck/0.00175 secant design line (decompile-verified), so a stiffness read here shadows one owner or imports the wrong one. US strength grades ride the `[03]` correspondence rows as ADVISORY data — `ACI318ConcreteGrade` enum rows exist but every non-EN factory arm throws (probe-confirmed), so no US grade reaches a factory. Member dimensions are `Provenance.Authored` (this estate's realized selection); the grade columns transcribe the print. `ComponentAuthority` publishes no ACI row, so the US cover table cites its clause in place and the seed rows stand EN-bodied under `ComponentAuthority.En`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                   // FrozenDictionary — the SeedJoin Resolve table
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                               // PositiveMagnitude — the kernel >0 finite magnitude atom
using Rasm.Domain;                                 // Op, Context, AcceptValidated
using Rasm.Element.Composition;                    // MaterialId, PropertyBag
using Rasm.Element.Properties;                     // DetailSchema, Dimension, PropertyName, PropertyValue
using Thinktecture;
using VividOrange.Materials.StandardMaterials.En;  // EnConcreteGrade — the EN grade binding RcSectionBuilder lowers
using VividOrange.Standards.Eurocode;              // NationalAnnex (the RC-entry passthrough)
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

// Every family page declares in the ONE Rasm.Materials.Component namespace; component#COMPONENT_OWNER binds
// ConcreteSeed.Rows by bare name on the concrete ComponentFamily policy row.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The member-role axis: each role carries its CONCRETE IFC4 leaf as ROW DATA (every (Entity, Predefined) pair
// verified against the generated IFC predefined-token roster (GeometryGym 25.7.30) — IfcBeam/BEAM, IfcColumn/COLUMN, IfcWall/SOLIDWALL,
// IfcSlab/FLOOR, IfcFooting/PAD_FOOTING, IfcPile/BORED), its profile shape (Round mints Circle, else Rectangle),
// its ACI 318-19 cover condition, its cast-method token, and Strip — true where the role's section is the
// one-metre design strip convention (wall/slab) rather than a discrete member outline. The role overrides the
// family-default IfcBinding PER SEED ROW exactly as the ComponentFamily.Ifc column licenses for a
// kind-determined leaf; the reverse AdmitImported read elects the family off the family default.
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

// --- [MODELS] ------------------------------------------------------------------------------
// SEED_ROW_LAW tier-3 currency: the EN 1992-1-1 Table 3.1 strength-class row. Fck and FckCube PUBLISHED (the
// printed class pair); Fcm DEFINED by the standard's own generator fcm = fck + 8 — a stored column beside the
// generator would be a second answer. NO Ecm column: the concrete.<key> substance row owns the mean modulus
// (c30_37 -> 33 GPa), and the EnConcreteFactory linear-elastic E is fck/0.00175 — the secant design line, NOT
// Ecm (decompile-verified) — so a fence reading stiffness anywhere but the substance row imports a wrong law.
// En is the VividOrange grade the RC entry lowers through reinforcement#RC_SECTION EnGrade.Concrete; the
// MixToken is the printed class designation the DetailSchema.MixDesignation bag row carries verbatim.
public readonly record struct ConcreteGradeRow(string Key, EnConcreteGrade En, double FckMpa, double FckCubeMpa, string MixToken) {
    public double FcmMpa => FckMpa + 8.0;                       // DEFINED: EN 1992-1-1 Table 3.1 generator
    public MaterialId Substance => MaterialId.Of($"concrete.{Key}");
}

// One realized CIP member type: role and grade as TYPED refs (a typo'd axis is a compile miss), the gross
// cross-section (WidthMm = 1000 on a Strip role — the design-strip convention the role row states), the declared
// EN 206 exposure token the cover regime and the bag both read, and the structural class the cover grid indexes.
// The designation derives from the row's own currencies, so a name cannot disagree with the thing it names.
public readonly record struct MemberRow(ConcreteRole Role, ConcreteGradeRow Grade, double WMm, double DMm, ExposureToken Exposure, StructuralClass Class) {
    public string Designation =>
        Role.Round
            ? $"concrete.{Grade.Key}-{Role.Key}-d{DMm:0}"
            : $"concrete.{Grade.Key}-{Role.Key}-{WMm:0}x{DMm:0}";
}

// --- [TABLES] ------------------------------------------------------------------------------
// The nine pack-confirmed Table 3.1 rows (fck / fck,cube printed verbatim; the substance catalogue carries the
// full C12/15..C90/105 span — a grade row lands here only where the component axis realizes members over it).
public static class ConcreteGrades {
    public static readonly ConcreteGradeRow C25_30 = new("c25_30", EnConcreteGrade.C25_30, 25.0, 30.0,  "C25/30");
    public static readonly ConcreteGradeRow C30_37 = new("c30_37", EnConcreteGrade.C30_37, 30.0, 37.0,  "C30/37");
    public static readonly ConcreteGradeRow C35_45 = new("c35_45", EnConcreteGrade.C35_45, 35.0, 45.0,  "C35/45");
    public static readonly ConcreteGradeRow C40_50 = new("c40_50", EnConcreteGrade.C40_50, 40.0, 50.0,  "C40/50");
    public static readonly ConcreteGradeRow C45_55 = new("c45_55", EnConcreteGrade.C45_55, 45.0, 55.0,  "C45/55");
    public static readonly ConcreteGradeRow C50_60 = new("c50_60", EnConcreteGrade.C50_60, 50.0, 60.0,  "C50/60");
    public static readonly ConcreteGradeRow C55_67 = new("c55_67", EnConcreteGrade.C55_67, 55.0, 67.0,  "C55/67");
    public static readonly ConcreteGradeRow C60_75 = new("c60_75", EnConcreteGrade.C60_75, 60.0, 75.0,  "C60/75");
    public static readonly ConcreteGradeRow C70_85 = new("c70_85", EnConcreteGrade.C70_85, 70.0, 85.0,  "C70/85");
    public static readonly ImmutableArray<ConcreteGradeRow> Rows = [C25_30, C30_37, C35_45, C40_50, C45_55, C50_60, C55_67, C60_75, C70_85];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The seed-time realization bag (DetailLane.Realization): the four Element-declared concrete rows plus the
// shared Joint("Cast") and Sourced rows. ConcreteCover is the MEASURED durability c_nom the regime derived —
// the seed-time value carries the 10 mm minimum bond floor; the RC entry re-derives with the chosen bar so a
// large bar can only RAISE it. The seam SectionProperties AxisDistance stays the FIRE cover — a different fact.
public static class ConcreteDetail {
    public static Fin<PropertyBag> Of(MemberRow row, double coverMm, Provenance source, Op key) =>
        from joint in ComponentDetail.Joint("Cast", key)
        from cover in ComponentDetail.Measured(DetailSchema.ConcreteCover, Dimension.LengthDim, coverMm * 1e-3)
        select ComponentDetail.RealizationRows(
            joint,
            cover,
            ComponentDetail.Token(DetailSchema.MixDesignation, row.Grade.MixToken),
            ComponentDetail.Token(DetailSchema.ExposureClass, row.Exposure.Key),
            ComponentDetail.Token(DetailSchema.CastMethod, row.Role.Cast),
            ComponentDetail.Sourced(source));
}

public static class ConcreteSeed {
    // Member DIMENSIONS are this estate's realized selection — AUTHORED under SEED_ROW_LAW — while every grade
    // column transcribes the print; the row provenance states the values' origin, so Authored is the honest column
    // and AdmitImported refuses to hand a vendor type these dimensions as published data.
    static readonly Provenance Selected = Provenance.Authored;

    // EN-bodied: the EN 1992-1-1/EN 206 span; a cast member has no mortar joint. ComponentAuthority publishes no
    // ACI row, so a US-bodied concrete selection waits on that vocabulary (the [03] ACI cover table cites its
    // clause in place).
    static readonly ComponentStandard En = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);

    // The realized selection spans every role once and the grade ladder where members realize it; strip roles
    // carry the 1000 mm design strip. Exposure per row is the member's declared service environment; every row
    // holds S4 (the §4.4.1 recommended 50-year class) so the cover read is the recommended ladder.
    static readonly ImmutableArray<MemberRow> Members = [
        new(ConcreteRole.Beam,        ConcreteGrades.C25_30, 300.0,  500.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Beam,        ConcreteGrades.C30_37, 300.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Beam,        ConcreteGrades.C35_45, 400.0,  800.0, ExposureToken.Xc4, StructuralClass.S4),
        new(ConcreteRole.Column,      ConcreteGrades.C30_37, 400.0,  400.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Column,      ConcreteGrades.C50_60, 500.0,  500.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Column,      ConcreteGrades.C70_85, 600.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.RoundColumn, ConcreteGrades.C40_50, 600.0,  600.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Wall,        ConcreteGrades.C30_37, 1000.0, 250.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Wall,        ConcreteGrades.C35_45, 1000.0, 300.0, ExposureToken.Xc4, StructuralClass.S4),
        new(ConcreteRole.Slab,        ConcreteGrades.C25_30, 1000.0, 200.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Slab,        ConcreteGrades.C30_37, 1000.0, 250.0, ExposureToken.Xc1, StructuralClass.S4),
        new(ConcreteRole.Footing,     ConcreteGrades.C25_30, 1000.0, 500.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new(ConcreteRole.Footing,     ConcreteGrades.C30_37, 1000.0, 700.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new(ConcreteRole.Pile,        ConcreteGrades.C30_37, 600.0,  600.0, ExposureToken.Xc2Xc3, StructuralClass.S4),
        new(ConcreteRole.Pile,        ConcreteGrades.C35_45, 900.0,  900.0, ExposureToken.Xc2Xc3, StructuralClass.S4)];

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, MemberRow>>> Table =
        SeedJoin.Of(toSeq(Members), static row => row.Designation);

    public static Fin<MemberRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    // The ONE generator arm: the durability cover derives from the SAME regime the RC entry re-reads (seed-time
    // bar-diameter floor = the c_min 10 mm term — the bond term re-derives when a bar is chosen), the profile is
    // the role-shaped railed arm, the IFC leaf the role's own row, and the bag seed-computed — every concrete row
    // flows Component.Of -> ComponentRow under the roster's own provenance, no second construction path.
    static Fin<ComponentRow> MemberOf(MemberRow row) {
        Op key = Op.Of(name: row.Designation);
        return
            from cover in EnCover.Nominal(row.Exposure, row.Class, barDiameterMm: 0.0, key)
            from profile in row.Role.Round
                ? SectionProfile.Circle.Of(row.DMm, key)
                : SectionProfile.Rectangle.Of(row.WMm, row.DMm, key)
            from detail in ConcreteDetail.Of(row, cover, Selected, key)
            from item in Component.Of(
                ComponentFamily.Concrete, row.Designation, profile,
                row.Role.Ifc,
                Coring.None, En, row.Grade.Substance, row.Grade.Substance,
                detail: Some(detail), key)
            select new ComponentRow(item, Selected);
    }

    // Fail-loud: the Context parameter is the ComponentFamily.Rows delegate contract; the seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        toSeq(Members).Traverse(MemberOf).As();

    // The concrete CAPACITY producer is an EXPLICIT TYPED REFUSAL, not silence: a bare concrete section carries no
    // SectionCapacity case — RcInteraction and RcElastic are built FROM an RcSection, and ConcreteRc.Assemble is
    // that route, deriving cover and layout law before handing this same Component to RcSectionBuilder. Binding
    // the refusal keeps the RC entry the ONLY door and the family axis compiler-forced.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<concrete-capacity-rides-rc-entry:{component.Designation.Value}>");
}
```

## [03]-[COVER_REGIME]

- Owner: `StructuralClass` the EN 1992-1-1 §4.4.1 S1–S6 axis with the Table 4.3N adjustment; `ExposureToken` the Table 4.4N COLUMN axis carrying the c_min,dur grid (each row naming the EN 206 classes it covers); `EnCover` the §4.4.1 c_nom fold; `AciCondition` the ACI 318-19 Table 20.5.1.3.1 specified-cover rows; `StrengthCorrespondence` the advisory psi↔EN-class map.
- Cases: EN grid {S1..S6} × {X0 · XC1 · XC2/XC3 · XC4 · XD1/XS1 · XD2/XS2 · XD3/XS3} — every cell the primary text's printed value, the XD3/XS3 column carrying S6 = 55 corroborated and S1–S5 TYPED-ABSENT (the primary conflicts with the SCIA variant grid, so those five cells refuse rather than pick a side); XA classes carry NO cover column by §4.4.1.2(12) — chemical attack is handled through concrete composition (`Properties/properties#MIX_PROPORTION` `ExposureClass` owns those EN 206 Annex F floors), so an XA read is a typed refusal naming that owner. ACI rows {cast-against-ground 75 · exposed 50/38 by bar band · interior slab/wall 19/38 · interior beam/column 38 · shell 19/13} — the SI-edition exact-conversion values.
- Law: `c_nom = c_min + Δc_dev` with `c_min = max(c_min,b; c_min,dur; 10)` (Exprs. 4.1/4.2) — Δc_dev the code-default 10 mm, Δc_dur,γ the code-default 0, c_min,b the bar diameter the caller supplies (0 at seed time, so the bond term can only raise a cover later, never lower it). This axis is NOT the `Properties/properties#MIX_PROPORTION` `ExposureClass` axis: that owner keys EN 206 Annex F durability FLOORS (w/c, cement, strength) per class; this owner keys the Table 4.4N cover COLUMNS, which group classes — the bag token is the join between them.
- Boundary: every numeric cell transcribes the pack's two-sourced print; the correspondence rows are ADVISORY BY CONSTRUCTION (f'c ~9% fractile vs fck 5% fractile — both sources flag it), seeded as guidance data no admission consumes; the 8000/10000 psi mappings are single-sourced and stay off the roster.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// EN 1992-1-1 §4.4.1: recommended class for 50-year design life S4, minimum S1; Table 4.3N adjusts — 100-year
// design life +2 classes, a higher strength class −1 — the two pack-proven modifiers, clamped to the S1..S6 span.
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

    public StructuralClass Adjusted(bool designLife100Years, bool higherStrengthClass) =>
        Items[Math.Clamp(Ordinal + (designLife100Years ? 2 : 0) - (higherStrengthClass ? 1 : 0), 0, Items.Count - 1)];
}

// The Table 4.4N COLUMN axis: each token names the EN 206 classes its column covers, so the bag's declared class
// string joins to exactly one band and the grouped columns (XC2/XC3, XD1/XS1, ...) are one row, never re-split.
// Cover carries the six c_min,dur cells S1..S6 in ordinal order; the XD3/XS3 row's first five cells are
// TYPED-ABSENT — the primary text (30/35/40/45/50) conflicts with the SCIA variant grid (25/30/35/40/45), only
// the S6 = 55 cell carries an independent corroboration, and an absent cell refuses rather than picks a side.
// XA classes appear on NO band: §4.4.1.2(12) routes chemical attack to concrete composition — the
// Properties/properties#MIX_PROPORTION ExposureClass Annex F floors — so an XA cover read is a typed refusal
// naming that owner, never a borrowed XC number presented as published.
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

// The ACI 318-19 Table 20.5.1.3.1 specified-cover rows (cast-in-place nonprestressed), the SI edition's
// exact-conversion millimetres (75/50/38/19/13). Each condition carries its large-bar and small-bar cells and
// the band threshold as ROW DATA: the exposed and interior-slab rows split at the printed bar bounds (No.5/No.6
// = 15.875/19.05 mm; No.11/No.14 = 35.814/43.002 mm off the reinforcement#REINFORCEMENT_FAMILY Bars table), the
// single-value rows carry both cells equal — so SpecifiedMm is one lattice read, never a branch ladder.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AciCondition {
    public static readonly AciCondition CastAgainstGround  = new("cast-against-ground",  largeMm: 75.0, smallMm: 75.0, thresholdMm: 0.0);
    public static readonly AciCondition ExposedToWeather   = new("exposed-to-weather",   largeMm: 50.0, smallMm: 38.0, thresholdMm: 19.05);   // No.6 and up 2 in; No.5 and smaller 1-1/2 in
    public static readonly AciCondition InteriorSlabWall   = new("interior-slab-wall",   largeMm: 38.0, smallMm: 19.0, thresholdMm: 43.002);  // No.14/No.18 1-1/2 in; No.11 and smaller 3/4 in
    public static readonly AciCondition InteriorBeamColumn = new("interior-beam-column", largeMm: 38.0, smallMm: 38.0, thresholdMm: 0.0);
    public static readonly AciCondition Shell              = new("shell",                largeMm: 19.0, smallMm: 13.0, thresholdMm: 19.05);   // No.6 and larger 3/4 in; No.5 and smaller 1/2 in
    public double LargeMm { get; }
    public double SmallMm { get; }
    public double ThresholdMm { get; }

    public double SpecifiedMm(double barDiameterMm) => barDiameterMm >= ThresholdMm && ThresholdMm > 0.0 ? LargeMm : ThresholdMm > 0.0 ? SmallMm : LargeMm;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ADVISORY psi↔EN-class correspondence — f'c (~9% fractile cylinder) equated to fck (5% fractile) is
// approximate BY CONSTRUCTION and both sources flag it, so these rows are guidance a specifier reads, never an
// identity any admission consumes; the exact MPa column is arithmetic (1 psi = 0.00689476 MPa). The two-sourced
// five; the 8000/10000 psi rows are single-sourced and stay off the roster.
public readonly record struct StrengthCorrespondence(int Psi, double Mpa, string NearestEnClass);

public static class StrengthMap {
    public static readonly ImmutableArray<StrengthCorrespondence> Rows = [
        new(2500, 17.2, "C16/20"), new(3000, 20.7, "C20/25"), new(4000, 27.6, "C28/35"),
        new(5000, 34.5, "C35/45"), new(6000, 41.4, "C40/50")];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The §4.4.1 nominal-cover fold: c_nom = max(c_min,b; c_min,dur; 10) + Δc_dev. Recommended values live as the
// owner's constants (Δc_dev = 10, Δc_dur,γ = 0 — the additive-safety, stainless, and coating deductions are
// national-annex terms this fold does not model); a typed-absent grid cell (XD3/XS3 at S1..S5) and the XA
// refusal both rail rather than fabricate a ladder value.
public static class EnCover {
    const double DeviationMm = 10.0;   // recommended Δc_dev
    const double FloorMm = 10.0;       // Expr. 4.2 absolute floor

    public static Fin<double> Nominal(ExposureToken exposure, StructuralClass structural, double barDiameterMm, Op key) =>
        from bar in guard(double.IsFinite(barDiameterMm) && barDiameterMm >= 0.0,
            ComponentFault.Dimension(key, $"<cover-bar-diameter-rejected:{barDiameterMm:R}>"))
        from dur in exposure.Cover[structural.Ordinal].ToFin(
            ComponentFault.Dimension(key, $"<cover-cell-typed-absent:{exposure.Key}:{structural.Key}>"))
        select Math.Max(Math.Max(barDiameterMm, dur), FloorMm) + DeviationMm;
}
```

## [04]-[RC_ENTRY]

- Owner: `ConcreteRc` — the concrete-side composition of the `reinforcement#RC_SECTION` boundary: `Assemble` resolves a SEED member and hands it to `RcSectionBuilder.Of` under the cover this page's regime derives, and `Capacity` the same composition through `RcSectionBuilder.Capacity` onto `capacity#SECTION_CAPACITY` `SectionCapacity.Resolve` — the end-to-end proof that a catalogued CIP member reaches `RcInteraction`/`RcElastic` with zero bespoke assembly.
- Law: the probe-proven engine limit gates HERE, typed: `FaceReinforcementLayer` is POLYGON-ONLY — fed a `Circle` profile it throws `InvalidProfileTypeException` — while `PerimeterReinforcementLayer` and the placed bar work on circles, so a `Circle`-profiled member (round column, bored pile) admits only the `PerimeterCount`/`PerimeterSpacing`/`Placed` layout cases and a face case on a round member is a typed `ComponentFault.Family` refusal at THIS boundary, never an engine throw surfacing as a build fault.
- Entry: `ConcreteRc.Assemble(member, barGrade, link, layout, annex, key)` — cover re-derives from the member's OWN declared exposure and class with the chosen layout's largest bar as c_min,b, so the bond term is live exactly where the bar is known; `ConcreteRc.Capacity(member, barGrade, link, layout, build, placement, key)` threads `RcSectionBuilder.Capacity` whole. Both entries take the `Component` — `RcSectionBuilder.Of` is already family-agnostic and PROFILE-FAITHFUL over `SectionSolver.ProfileOf`, so the round member feeds its true `ICircle`.
- Boundary: this owner derives NO section math and admits NO VividOrange surface — grade lowering, layer construction, the transformed-section carrier, and the capacity solve all stay behind `RcSectionBuilder`/`SectionCapacity`; the concrete contribution is exactly the member, the cover law, and the layout admissibility its own profile decides.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The END-TO-END rail: seed member -> exposure-derived cover -> RcSectionBuilder -> RcSection -> SectionCapacity.
// A catalogued concrete.c30_37-beam-300x600 plus a face layout of H16 B500B bars and an H10 link reaches the
// RcInteraction hull and the RcElastic screen through exactly this fold — no consumer assembles a concrete
// section for itself, and the two RC capacity cases have the producer chain this family owes them.
public static class ConcreteRc {
    public static Fin<RcSection> Assemble(Component member, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, NationalAnnex annex, Op key) =>
        from row in ConcreteSeed.Resolve(member, key)
        from admitted in Admissible(member, layout, key)
        from cover in EnCover.Nominal(row.Exposure, row.Class, LargestBarMm(link, layout), key)
        from section in RcSectionBuilder.Of(member, row.Grade.En, barGrade, link, layout, cover, annex, key)
        select section;

    public static Fin<SectionCapacity> Capacity(Component member, RebarGradeRow barGrade, BarRow link, Seq<RebarLayout> layout, CapacityBuild build, CapacityPlacement placement, Op key) =>
        from row in ConcreteSeed.Resolve(member, key)
        from admitted in Admissible(member, layout, key)
        from cover in EnCover.Nominal(row.Exposure, row.Class, LargestBarMm(link, layout), key)
        from capacity in RcSectionBuilder.Capacity(member, row.Grade.En, barGrade, link, layout, cover, build, placement, key)
        select capacity;

    // The polygon-only engine limit as a TYPED gate: face layers throw on a Circle (probe-confirmed), perimeter and
    // placed layers do not — so the round member's admissible layout subset is decided here from the profile the
    // member itself carries, and the refusal names the case that cannot exist.
    static Fin<Unit> Admissible(Component member, Seq<RebarLayout> layout, Op key) =>
        guard(member.Profile is not SectionProfile.Circle
                || layout.ForAll(static item => item is not (RebarLayout.FaceCount or RebarLayout.FaceSpacing)),
            ComponentFault.Family(key, $"<face-layout-on-circular-member:{member.Designation.Value}>")).ToFin();

    // c_min,b reads the largest bar in the assembly — the link and every layout bar — so the bond term of
    // Expr. 4.2 is derived from what is actually placed rather than a caller-remembered diameter.
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
- Boundary: these are SYSTEM-level design coefficients a lateral-system selection reads — no member check consumes them and no `SectionCapacity` case carries them; the DEMAND-side consumer is the `Rasm.Compute` `Analysis/structural#SEISMIC_ROUTE` `SpectrumPolicy.Behavior` divisor, which takes the EN q (the `EnConcreteDuctility` q0 under its αu/α1 and elevation modifiers, floored at `QFloor`) or the ASCE R over Ie (the `AsceRcSystem.R` column) as a SCALAR at composition because the branch strata forbid a reference in either direction — the lateral-system selection resolves the row once per engagement and threads the number. Ω0 and Cd are the row's amplification companions with NO Compute arm yet: Ω0 amplifies connected-element force demands and Cd elastic drifts, and no overstrength-combination or drift-amplification fold exists at the member route — the columns stand as system-selection data until that consumer lands, never silently folded into `Behavior`. The SDC permission/height columns of Table 12.2-1 stay untranscribed (out of pack scope); member-level seismic DETAILING (hoop spacing, confinement ratios) has no proven row in the pack and lands nowhere on this page.

```csharp signature
// --- [TABLES] ------------------------------------------------------------------------------
// ASCE 7 Table 12.2-1, concrete systems, the pack's two-sourced rows verbatim (response modification R,
// overstrength Ω0, deflection amplification Cd). The permitted-SDC and height-limit columns are out of pack
// scope and untranscribed; a system the roster lacks is one row once two-sourced.
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

// EN 1998-1 concrete behaviour factors, proven cells only: DCL is the q ≤ 1.5 overstrength-only cap (two-sourced),
// DCM frame/dual/coupled-wall q0 = 3.0 (two-sourced; the ×(αu/α1) multiplier's default ladder is standard-text-only
// and ABSENT, so Q0 states the base and AlphaRatio is a caller input a pushover or a corroborated default supplies).
// Every DCH cell is ABSENT — a DCH row lands when a second source corroborates the 4.5/4.0/3.0/2.0 column.
// q = q0·kw ≥ 1.5 and the kw wall formula are standard-text-only and not transcribed.
public readonly record struct EnConcreteDuctility(string Key, Option<double> Q0, bool AlphaRatioApplies);

public static class EnConcreteDuctilities {
    public const double IrregularElevationFactor = 0.8;   // non-regular in elevation: q0 reduced 20% (two-sourced)
    public const double QFloor = 1.5;                     // the DCL/absolute floor (two-sourced)

    public static readonly ImmutableArray<EnConcreteDuctility> Rows = [
        new("dcl",             Some(1.5), AlphaRatioApplies: false),
        new("dcm-frame-dual",  Some(3.0), AlphaRatioApplies: true),
        new("dch-frame-dual",  None,      AlphaRatioApplies: true)];
}
```

## [06]-[RESEARCH]

(none)
