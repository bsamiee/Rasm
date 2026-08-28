# [MATERIALS_ALUMINUM]

THE ALUMINUM SEED FAMILY GROUNDED IN THE PUBLISHED EN 1999 ALLOY BANDS AND THE AUTHORED DIE ROSTER. No standardized structural-section catalogue exists for the metal — roughly ninety percent of extrusions run on customer-owned dies — so the family's section truth is the opposite of steel's: the EN 1999-1-1 Table 3.2a/3.2b characteristic bands ride `component#MATERIAL_GRADE` `MaterialGrade` rows over the `GradeProperties.Aluminum` arm (fo/fu per alloy × temper × product form × thickness window, the buckling-class row, the typed-absent HAZ cell) with the ONE banded `Strengths` read landed HERE beside the family that owns it, while `AluminumSeed` folds an AUTHORED die roster — curtain-wall mullion and transom, structural extrusions, marine sheet — through the parent `SectionProfile` arms under `EvidenceGrade.User`, the same posture the steel fabricated rows take. Every die proves its alloy band at seed time, so a section outside its grade's published thickness window never reaches the capacity pipeline unpriced, and the `ExtrusionRole` axis owns the complete per-role IFC pair the Gate-0 roster validates.

The page composes the parent `component#COMPONENT_OWNER` whole — `Component.Of`, `ComponentRow`, `SectionProfile.RectangleHollow`/`IShape`/`Angle`/`CircleHollow`/`Rectangle` fallible factories, `IfcBinding`, `Coring.None`, `ComponentStandard`/`ComponentAuthority.En`, `ComponentDetail.ProductRows`, the band-2300 `ComponentFault` channel — and stamps the contract `DetailSchema` CURTAIN-WALL/ALUMINUM product rows (`MullionProfile`, `ThermalBreak`, `GlazingPocket`, the reused `PanelThickness`). Substance identities are the Properties catalogue's own `aluminium.6061t6`/`6063t5`/`6063t6`/`6082t6`/`5083` rows; the render slot is the measured `metal.aluminum` conductor. The EC9 partial-factor pair (γM1 1.10 covering cross-section resistance and member instability alike — EN 1999-1-1 declares no γM0 — and γM2 1.25 on the fracture check) states once here, mirrored by the `capacity#SECTION_CAPACITY` `DesignBasis` EN 1999 row; the §6.3.1 buckling CURVE is the `BucklingClass` row's own published pair, read by the `capacity#SECTION_CAPACITY` `AluminumMember` arm and its contract publication so neither end copies a constant.

## [01]-[INDEX]

- [02]-[ALUMINUM_FAMILY]: the `ExtrusionForm` EN 755/EN 485 product-form axis, the `BucklingClass` Table 6.6 curve row, the `ThermalBreakClass` isolator vocabulary, the `ExtrusionRole` die-role axis with its per-role IFC pair, the `AluminumPartialFactor` EC9 γM pair, the `AlloyBand`/`HazRow` published Table 3.2a/3.2b band currencies with the ONE `Strengths` banded read on the `GradeProperties.Aluminum` arm, the `DieRow` authored-die currency, the `AluminumDetail` product bag, and the `AluminumSeed.Roster`/`Law` set with its `CapacityLift.Aluminum` producer.

## [02]-[ALUMINUM_FAMILY]

- Owner: the `GradeProperties.Aluminum` partial member the ONE banded `Strengths` read lands on; `BucklingClass` the EN 1999-1-1 Table 6.6 curve row; `ExtrusionForm` the EN 755-2/EN 485 product-form axis every band keys on; `ExtrusionRole` the die-role axis owning the COMPLETE per-role IFC pair; `ThermalBreakClass` the closed isolator vocabulary the `DetailSchema.ThermalBreak` stamp reads; `AluminumPartialFactor` the EC9 γM pair; `AluminumSeed` the die roster, the seed law, and the capacity producer; `AluminumDetail` the product bag.
- Cases: grade {the five `ComponentFamily.Aluminum` `MaterialGrade` rows — 6061-T6 · 6063-T5 · 6063-T6 · 6082-T6 · 5083 O/H111} × form {ep · et · er-b · dt · sheet · plate} × role {mullion · transom · structural · panel} — a die is ONE `DieRow` over one fallible profile build delegate, its alloy band selected by the die's own governing element thickness, never a per-die type and never a catalogue impersonation.
- Entry: `ComponentSeed.Rows(context, AluminumSeed.Roster, AluminumSeed.Law)` — this page states the roster and the policy, never the fold; the law's coherence proves the (form, thickness) band against the grade's own printed window, so a die outside it names itself in the build verdict instead of reaching the capacity pipeline unpriced. `AluminumSeed.Capacity` resolves the die, re-reads the banded pair, and lifts `CapacityLift.Aluminum`.
- Packages: Rasm.Domain (`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `DetailSchema`, `PropertyBag`, `PropertyName`, `PropertyValue`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/the fallible `SectionProfile` arm factories/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`SeedJoin`; `component#MATERIAL_GRADE` `MaterialGrade`/`GradeProperties`; `component#COMPONENT_SEED` `SeedLaw`/`ComponentSeed`; `capacity#SECTION_CAPACITY` `CapacityLift.Aluminum`, `CapacityPlacement`, `SectionCapacity.Lift`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + comparer accessors for the form/role/isolator/curve vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Validation`/`.As()`/`ToFin`), BCL (`ImmutableArray`, `FrozenDictionary`). NO aluminum producer exists among admitted packages (reflection over the whole VividOrange train: `MaterialType.Aluminium` is one dead enum row, no grade, no factory, no extrusion family; `En1999` is a citation record over `En1999Part.Part1_1`–`Part1_5` with zero design members), so the grade bands are PUBLISHED under per-column provenance and the EN 1999 citation rides the capacity basis row.
- Growth: a new alloy/temper is one `MaterialGrade` aluminum row carrying its printed bands; a new printed band one `AlloyBand` entry on its arm; a new die one `DieRow` (the profile arm and its `component#SECTION_SOLVER` supplement already exist — the die is DATA); a new curtain-wall role one `ExtrusionRole` row carrying its Gate-0-validated IFC pair; a new isolator one `ThermalBreakClass` row; a new buckling curve one `BucklingClass` row carrying its own Table 6.6 pair; a HAZ capability is the `HazRow` cell filling on its grade — never a parallel reduction table.
- Boundary: the aluminum arm's columns and identity are `component#MATERIAL_GRADE`'s and its banded READ is this page's — `Strengths` selects the printed (form, thickness) cell and fails an uncovered pair typed rather than borrowing a neighbouring band, so no consumer re-walks the band list. Every numeric on this page is two-source-traced or typed-absent. The 6xxx HAZ pairs are single-sourced AND the simplified single-factor convention conflicts with the per-row EC9 pairs, so every 6xxx `Haz` cell is `None` and only the work-hardened 5083 O/H111 answers `Some(1.0, 1.0)`; the design modulus E = 70 GPa is single-sourced as a CODE value, so this page carries no E constant — the Properties catalogue substance rows own the modulus the member arm reads; the 6061-T6 sheet/plate band and the Table 3.2b Ramberg-Osgood exponent column are single-sourced and absent; standardized mullion-depth series and polyamide strut widths have no captured quantitative source, so pocket depth and isolator class are AUTHORED die facts on the roster, never standard claims. PROFILE REUSE is adjudicated closed: the parent `IShape`/`Channel`/`Tee`/`Angle`/`RectangleHollow`/`CircleHollow`/`ColdFormedC`/`Rectangle` arms cover the die space here, a free extruded silhouette rides `Outline` under its declared `SolidPolygon`/`OpenThin` topology, and the one unrepresentable die — a multi-chamber NON-rectangular hollow — is a voided-outline arm owned by `component#SECTION_PROFILE` growth, not minted here. IFC strings stay neutral; the Gate-0 roster proves `IfcMember` carries `MULLION` and `MEMBER` and NO transom token (the transom rides the `USERDEFINED` `ObjectType` discriminator IFC itself requires), and `IfcPlate`/`CURTAIN_PANEL` keeps the sheet row's triple disjoint from the glazing (`SHEET`) and panel (`NOTDEFINED`) family claims so the reverse `Claimant` read elects exactly one family. The capacity CURVE is not this page's: `AluminumSeed.Capacity` hands the member arm the banded (fo, fu) pair, the class letter, and the solved section whole, and the §6.3.1 α/λ̄0 constants per class letter live on the `capacity#SECTION_CAPACITY` EN 1999 arm.

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BucklingClass {
    public static readonly BucklingClass A = new("a", alpha: 0.20, plateau: 0.10);
    public static readonly BucklingClass B = new("b", alpha: 0.32, plateau: 0.00);
    public double Alpha { get; }
    public double Plateau { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThermalBreakClass {
    public static readonly ThermalBreakClass None           = new("none");
    public static readonly ThermalBreakClass PolyamideStrut = new("polyamide-strut");
    public static readonly ThermalBreakClass ResinPoured    = new("resin-poured");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtrusionRole {
    public static readonly ExtrusionRole Mullion    = new("mullion",    ifc: IfcBinding.Of("IfcMember", "MULLION"),      framing: true);
    public static readonly ExtrusionRole Transom    = new("transom",    ifc: IfcBinding.Named("IfcMember", "Transom"),   framing: true);
    public static readonly ExtrusionRole Structural = new("structural", ifc: IfcBinding.Of("IfcMember", "MEMBER"),       framing: false);
    public static readonly ExtrusionRole Panel      = new("panel",      ifc: IfcBinding.Of("IfcPlate", "CURTAIN_PANEL"), framing: false);
    public IfcBinding Ifc { get; }
    public bool Framing { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class AluminumPartialFactor {
    public const double Instability = 1.10;
    public const double Fracture    = 1.25;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AlloyBand(Seq<ExtrusionForm> Forms, double MinMm, double MaxMm, double FoMpa, double FuMpa) {
    public bool Covers(ExtrusionForm form, double thicknessMm) =>
        Forms.Contains(form) && thicknessMm > MinMm && thicknessMm <= MaxMm;
}

public readonly record struct HazRow(double RhoO, double RhoU);

public partial record GradeProperties {
    public sealed partial record Aluminum {
        public Fin<(double FoMpa, double FuMpa)> Strengths(ExtrusionForm form, double thicknessMm) =>
            Bands.Find(band => band.Covers(form, thicknessMm))
                .Map(static band => (band.FoMpa, band.FuMpa))
                .ToFin(new ComponentFault.GradeBandMissing(ComponentFamily.Aluminum, typeof(AlloyBand)));
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Aluminum> AluminumArm => Columns is GradeProperties.Aluminum arm ? Some(arm) : None;
}

// --- [TABLES] --------------------------------------------------------------------------
public readonly record struct DieRow(
    string Designation, ExtrusionRole Role, MaterialGrade Grade, ExtrusionForm Form, double ElementMm,
    ThermalBreakClass Break, Option<double> GlazingPocketMm, Func< Fin<SectionProfile>> Build);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AluminumDetail {
    public static Fin<PropertyBag> Of(DieRow die) =>
        from pocket in die.GlazingPocketMm.TraverseM(mm =>
            ComponentDetail.Measured(DetailSchema.GlazingPocket, Dimension.LengthDim, mm * 1e-3)).As()
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
            ComponentDetail.Sourced(EvidenceGrade.User),
            .. framing,
            .. isolator,
            .. pocket.ToSeq(),
            .. sheet.ToSeq(),
        ]);
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class AluminumSeed {
    static readonly MaterialId Render = MaterialId.Create("metal.aluminum");

    public static readonly Seq<DieRow> Roster = Seq(
        new DieRow("aluminum.mullion-50x120", ExtrusionRole.Mullion, MaterialGrade.A6063T5, ExtrusionForm.Profile, 3.0,
            ThermalBreakClass.PolyamideStrut, Some(25.0),
            static key => SectionProfile.RectangleHollow.Of(widthMm: 50.0, depthMm: 120.0, wallMm: 3.0, innerFilletMm: 2.0, outerFilletMm: 2.0)),
        new DieRow("aluminum.transom-50x80", ExtrusionRole.Transom, MaterialGrade.A6063T5, ExtrusionForm.Profile, 3.0,
            ThermalBreakClass.PolyamideStrut, Some(25.0),
            static key => SectionProfile.RectangleHollow.Of(widthMm: 50.0, depthMm: 80.0, wallMm: 3.0, innerFilletMm: 2.0, outerFilletMm: 2.0)),
        new DieRow("aluminum.i-120x80", ExtrusionRole.Structural, MaterialGrade.A6082T6, ExtrusionForm.Profile, 5.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.IShape.Of(depthMm: 120.0, widthMm: 80.0, webMm: 5.0, flangeMm: 5.0, filletMm: 6.0, flangeToeMm: 5.0)),
        new DieRow("aluminum.tube-60x6", ExtrusionRole.Structural, MaterialGrade.A6082T6, ExtrusionForm.Tube, 6.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.CircleHollow.Of(diameterMm: 60.0, wallMm: 6.0)),
        new DieRow("aluminum.angle-60x6", ExtrusionRole.Structural, MaterialGrade.A6061T6, ExtrusionForm.Profile, 6.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.Angle.Of(depthMm: 60.0, widthMm: 60.0, thicknessMm: 6.0, filletMm: 4.0, legToeMm: 6.0)),
        new DieRow("aluminum.sheet-3", ExtrusionRole.Panel, MaterialGrade.A5083, ExtrusionForm.Sheet, 3.0,
            ThermalBreakClass.None, None,
            static key => SectionProfile.Rectangle.Of(widthMm: 1200.0, depthMm: 3.0)));

    public static readonly SeedLaw<DieRow> Law = SeedLaw<DieRow>.Of(
        family: ComponentFamily.Aluminum,
        designation: static die => die.Designation,
        coherence: Coherence,
        profile: static (die, key) => die.Build(key),
        substance: static die => die.Grade.Substance,
        source: static _ => EvidenceGrade.User,
        standard: static die => new ComponentStandard(die.Grade.Authority.Region, StandardJointThicknessMm: 0.0, die.Grade.Authority),
        detail: Some<Func<DieRow, SectionProfile, Fin<PropertyBag>>>(static (die, _, _) => AluminumDetail.Of(die)),
        appearance: static _ => Render,
        ifc: static die => die.Role.Ifc);

    static Validation<Error, Unit> Coherence(DieRow die) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(die.Grade.Family == ComponentFamily.Aluminum,
                new ComponentFault.GradeFamilyMismatch(die.Grade, ComponentFamily.Aluminum)),
            AdmissionSlots.Gate(die.Grade.AluminumArm.IsSome,
                new ComponentFault.GradeBodyMissing(die.Grade, ComponentFamily.Aluminum)),
            die.Grade.AluminumArm
                .Traverse(arm => arm.Strengths(die.Form, die.ElementMm).ToValidation().Map(static _ => unit)).As()
                .Map(static _ => unit)));

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, DieRow>>> Table =
        SeedJoin.Of(Roster, static die => die.Designation);

    public static Fin<DieRow> Resolve(Component component) =>
        SeedJoin.Resolve(Table, component.Designation);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement) =>
        from solved in section.ToFin(new ComponentFault.SectionUnavailable(component.Designation))
        from based in guard(placement.Basis == DesignBasis.En1999,
            new ComponentFault.BasisUnsupported(placement.Basis, ComponentFamily.Aluminum))
        from die in Resolve(component, key)
        from arm in die.Grade.AluminumArm.ToFin(new ComponentFault.GradeBodyMissing(key, die.Grade, ComponentFamily.Aluminum))
        from banded in arm.Strengths(die.Form, die.ElementMm, key)
        from capacity in SectionCapacity.Lift(new CapacityLift.Aluminum(
            component.Designation, die.Grade, die.Form, banded.FoMpa, banded.FuMpa, solved, placement.Basis), key)
        select capacity;
}
```

## [03]-[RESEARCH]

(none)
