# [MATERIALS_STEEL]

THE STEEL PROFILEFAMILY. The steel cross-section vocabulary — the AISC Shapes Database v16.0 section-property columns (depth / flange-width / web-thickness / flange-thickness / fillet, the `Ix`/`Sx`/`Zx` strong-axis stiffness columns) and the `IfcProfileDef` I-shape/U-shape/L-shape/HSS subtype discriminant — is the second realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Steel` case. A wide-flange section is a `Profile` row, never a `WSection` type: the section shape, the dimensional columns, and the stiffness receipt are steel-`Profile` columns, and the `SteelSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry family drives — a steel member extrudes through one `Profile` over the `RunPath`, never a per-family layout. The steel vocabulary grows by data — a new section is one `SteelShape` catalogue row, a new subtype one `SteelClass` case — never a per-section type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape and the `Rasm` kernel `Dimension` value-object for every length column; cmu/timber/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[STEEL_FAMILY]: the `SteelClass` subtype axis, the `SteelSection` section-property record, the `CompactnessClass` slenderness verdict, the `DesignCapacity` LRFD projection, the `ProfileUnit` projection that flows a steel section through the masonry-shaped `Resolve` fold, and the `ProfileCatalogue.BuildSteelRows` AISC v16.0 row tables (W/M/S/HP · C/MC · L · HSS · WT/MT/ST).

## [02]-[STEEL_FAMILY]

- Owner: the steel section vocabulary (`SteelClass` the `IfcProfileDef` subtype discriminant, `SteelSection` the AISC section-property record, `CompactnessClass` the Table B4.1 verdict, `DesignCapacity` the LRFD receipt); `ProfileCatalogue.BuildSteelRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `SteelSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`, the `SteelSection.Capacity` projection the structural-design seam reads.
- Cases: class {i-shape (W/M/S/HP), u-shape (C/MC channel), l-shape (L angle), hss-rect, hss-round, tee (WT/MT/ST)} — the closed `IfcProfileDef` parameterized-profile subtype set; a section is a `SteelSection` row over one `SteelClass`, never a section subtype.
- Entry: `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `SteelSection` — the section→`ProfileUnit` projection (`WidthMm` = flange width or HSS breadth, `HeightMm`/`CourseHeightMm` = section depth, `LengthMm` = a unit-segment placeholder the `RunPath` length overrides) so a steel member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public CompactnessClass Classify(double yieldStressMpa)` the AISC Table B4.1 flange-AND-web slenderness verdict (compact/noncompact/slender), and `public DesignCapacity Capacity(double yieldMpa, double unbracedLengthMm, double effectiveLengthMm)` the `[BoundaryAdapter]` LRFD projection emitting `φMn` (flexural — yielding/LTB/FLB governing), `φPn` (compression — flexural buckling over the slenderness input), and `φVn` (shear) from the `Zx`/`Sx`/`Ix`/`Area` columns already stored — every capacity a derived projection, never a re-minted dimension.
- Packages: Rasm (project — `PositiveMagnitude` for every fractional-millimeter section column), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the steel vocabulary grows by data — a new AISC section is one `SteelShape` catalogue row keyed by its EDI designation, a new shape family one `SteelClass` case carrying its `IfcProfileDef` subtype mapping — never a per-section type, never a per-family `Profile` variant. A cmu/timber/glazing family lands its own vocabulary on its own page the way steel carries `SteelClass`/`SteelSection`; the named cost is stated at `profile#STRUCTURAL_FAMILY_VOCABULARY` and queued in `TASKLOG.md`.
- Boundary: the steel vocabulary is the second realized `ProfileFamily` — a per-section class is the deleted form; `SteelSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive and a fractional AISC web thickness admits without truncation, and the `Ix`/`Sx`/`Zx` stiffness columns are `PositiveMagnitude` receipts the `IfcProfileDef` wire and the `DesignCapacity` LRFD projection read, never raw doubles; the `IsCompact` bool is the deleted form — `SteelSection.Classify` returns the 3-state `CompactnessClass` (compact/noncompact/slender) over AISC Table B4.1 flange-AND-web limits and `SteelSection.Capacity` derives the LRFD `φMn`/`φPn`/`φVn` from the stored stiffness columns, so a beam/column/brace reports its design strength directly; `SteelSection.ToUnit` is the ONE bridge from the section-property vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes — a steel run is the same station-stepped fold over a single-orientation course, so a beam/column is a `ProfileSet` extrusion (`assembly#MATERIAL_ASSIGNMENT` `ProfileSet`) along the `RunPath`, never a masonry-style multi-unit course; `SteelClass` maps each shape to its `IfcProfileDef` subtype (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef`) so a steel member round-trips to IFC 4.3 as an `IfcMaterialProfileSet`; `ProfileCatalogue.BuildSteelRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the AISC rows keyed `steel.<designation>`, the realized cross-section grounded in the published AISC Shapes Database v16.0 section-property values.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class SteelClass {
    public static readonly SteelClass IShape   = new("i-shape", ifcSubtype: "IfcIShapeProfileDef");
    public static readonly SteelClass UShape   = new("u-shape", ifcSubtype: "IfcUShapeProfileDef");
    public static readonly SteelClass LShape   = new("l-shape", ifcSubtype: "IfcLShapeProfileDef");
    public static readonly SteelClass HssRect  = new("hss-rect", ifcSubtype: "IfcRectangleHollowProfileDef");
    public static readonly SteelClass HssRound = new("hss-round", ifcSubtype: "IfcCircleHollowProfileDef");
    public static readonly SteelClass Tee      = new("tee", ifcSubtype: "IfcTShapeProfileDef");
    public string IfcSubtype { get; }
}

// The AISC Table B4.1 width-to-thickness verdict — a 3-state design class, never a 2-state IsCompact flag.
[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new();
    public static readonly CompactnessClass Noncompact = new();
    public static readonly CompactnessClass Slender     = new();
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct SteelSection(
    SteelClass Class,
    PositiveMagnitude DepthMm,
    PositiveMagnitude FlangeWidthMm,
    PositiveMagnitude WebThicknessMm,
    PositiveMagnitude FlangeThicknessMm,
    PositiveMagnitude FilletMm,
    PositiveMagnitude AreaMm2,
    PositiveMagnitude IxMm4,
    PositiveMagnitude SxMm3,
    PositiveMagnitude ZxMm3) {

    public double FlangeSlenderness => FlangeWidthMm.Value / (2.0 * FlangeThicknessMm.Value);
    public double WebSlenderness => (DepthMm.Value - 2.0 * FlangeThicknessMm.Value) / WebThicknessMm.Value;
    public double RadiusOfGyrationMm => Math.Sqrt(IxMm4.Value / AreaMm2.Value);

    public Fin<ProfileUnit> ToUnit(Context context, Op key) =>
        ProfileUnit.Of(FlangeWidthMm.Value, DepthMm.Value, DepthMm.Value, DepthMm.Value, context, key);

    // AISC 360 Table B4.1 flange λpf/λrf and web λpw/λrw limits over √(E/Fy); the governing axis is the slenderer of flange and web.
    [BoundaryAdapter]
    public CompactnessClass Classify(double yieldMpa) {
        double r = Math.Sqrt(200_000.0 / yieldMpa);
        (double flangeP, double flangeR) = (0.38 * r, 1.0 * r);
        (double webP, double webR) = (3.76 * r, 5.70 * r);
        bool slender = FlangeSlenderness > flangeR || WebSlenderness > webR;
        bool compact = FlangeSlenderness <= flangeP && WebSlenderness <= webP;
        return slender ? CompactnessClass.Slender : compact ? CompactnessClass.Compact : CompactnessClass.Noncompact;
    }

    // AISC 360 Chapters F/E/G LRFD: φMn = φb·Fy·Zx with the LTB/FLB reduction, φPn = φc·Fcr·A over the flexural-buckling stress, φVn = φv·0.6·Fy·Aw.
    [BoundaryAdapter]
    public DesignCapacity Capacity(double yieldMpa, double unbracedLengthMm, double effectiveLengthMm) {
        const double φb = 0.90, φc = 0.90, φv = 0.90, E = 200_000.0;
        double r = RadiusOfGyrationMm, λc = effectiveLengthMm / r;
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        double Fcr = Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe;
        double Lb = unbracedLengthMm, Lp = 1.76 * r * Math.Sqrt(E / yieldMpa);
        double Mp = yieldMpa * ZxMm3.Value;
        double Mn = Lb <= Lp ? Mp : Math.Max(yieldMpa * SxMm3.Value, Mp - (Mp - 0.7 * yieldMpa * SxMm3.Value) * Math.Clamp((Lb - Lp) / (3.0 * Lp), 0.0, 1.0));
        double Aw = DepthMm.Value * WebThicknessMm.Value;
        return new DesignCapacity(
            FlexuralNmm: φb * Mn,
            CompressionN: φc * Fcr * AreaMm2.Value,
            ShearN: φv * 0.6 * yieldMpa * Aw,
            Classification: Classify(yieldMpa),
            Slenderness: λc);
    }
}

public readonly record struct DesignCapacity(double FlexuralNmm, double CompressionN, double ShearN, CompactnessClass Classification, double Slenderness);

public readonly record struct SteelRow(SteelClass Class, string Designation, double DMm, double BfMm, double TwMm, double TfMm, double KMm, double AMm2, double IxMm4, double SxMm3, double ZxMm3);

public sealed record SteelShape(ProfileId Id, SteelSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly ProfileStandard Aisc = new("us", StandardJointThicknessMm: 0.0, Authority: "AISC v16.0");

    // AISC Shapes Database v16.0 SI columns; HSS wall thickness rides WebThicknessMm, tee stem thickness rides WebThicknessMm per HSS_COLUMN_REUSE.
    static readonly Seq<SteelRow> SteelRows = Seq(
        new SteelRow(SteelClass.IShape, "steel.w12x26", 310.0, 165.0, 5.84, 9.65, 17.8, 4940.0, 204.0e6, 314.0e3, 353.0e3),
        new SteelRow(SteelClass.IShape, "steel.w14x90", 356.0, 369.0, 11.2, 18.0, 28.7, 17100.0, 416.0e6, 1730.0e3, 1920.0e3),
        new SteelRow(SteelClass.IShape, "steel.w18x50", 457.0, 190.0, 9.02, 14.5, 24.4, 9480.0, 333.0e6, 1410.0e3, 1610.0e3),
        new SteelRow(SteelClass.IShape, "steel.w21x68", 537.0, 210.0, 10.9, 17.4, 27.7, 12900.0, 616.0e6, 2300.0e3, 2620.0e3),
        new SteelRow(SteelClass.IShape, "steel.w24x76", 607.0, 228.0, 11.2, 17.3, 28.4, 14400.0, 874.0e6, 2880.0e3, 3290.0e3),
        new SteelRow(SteelClass.UShape, "steel.c15x33.9", 381.0, 86.4, 10.2, 16.0, 22.4, 6450.0, 180.0e6, 945.0e3, 1100.0e3),
        new SteelRow(SteelClass.UShape, "steel.mc18x42.7", 457.0, 95.2, 11.9, 16.7, 26.9, 8130.0, 282.0e6, 1230.0e3, 1490.0e3),
        new SteelRow(SteelClass.LShape, "steel.l6x6x3/4", 152.0, 152.0, 19.1, 19.1, 19.1, 5500.0, 11.6e6, 113.0e3, 204.0e3),
        new SteelRow(SteelClass.LShape, "steel.l4x4x1/2", 102.0, 102.0, 12.7, 12.7, 12.7, 2420.0, 2.27e6, 31.0e3, 56.5e3),
        new SteelRow(SteelClass.HssRect, "steel.hss8x8x1/2", 203.0, 203.0, 11.8, 11.8, 11.8, 8390.0, 50.3e6, 495.0e3, 595.0e3),
        new SteelRow(SteelClass.HssRect, "steel.hss6x4x3/8", 152.0, 102.0, 8.86, 8.86, 8.86, 3690.0, 11.2e6, 147.0e3, 187.0e3),
        new SteelRow(SteelClass.HssRound, "steel.hss6.625x0.500", 168.0, 168.0, 11.8, 11.8, 11.8, 5810.0, 18.4e6, 219.0e3, 296.0e3),
        new SteelRow(SteelClass.HssRound, "steel.hss8.625x0.375", 219.0, 219.0, 8.84, 8.84, 8.84, 5710.0, 32.6e6, 298.0e3, 393.0e3),
        new SteelRow(SteelClass.Tee, "steel.wt9x38", 230.0, 311.0, 8.13, 16.3, 16.3, 7160.0, 36.6e6, 188.0e3, 339.0e3),
        new SteelRow(SteelClass.Tee, "steel.wt6x25", 152.0, 204.0, 7.11, 11.8, 11.8, 4730.0, 9.99e6, 78.7e3, 142.0e3));

    static Fin<SteelShape> SteelShapeOf(SteelRow r, Context context, Op key) =>
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.DMm)
        from bf in key.AcceptValidated<PositiveMagnitude>(candidate: r.BfMm)
        from tw in key.AcceptValidated<PositiveMagnitude>(candidate: r.TwMm)
        from tf in key.AcceptValidated<PositiveMagnitude>(candidate: r.TfMm)
        from k in key.AcceptValidated<PositiveMagnitude>(candidate: r.KMm)
        from a in key.AcceptValidated<PositiveMagnitude>(candidate: r.AMm2)
        from ix in key.AcceptValidated<PositiveMagnitude>(candidate: r.IxMm4)
        from sx in key.AcceptValidated<PositiveMagnitude>(candidate: r.SxMm3)
        from zx in key.AcceptValidated<PositiveMagnitude>(candidate: r.ZxMm3)
        select new SteelShape(ProfileId.Of(r.Designation), new SteelSection(r.Class, depth, bf, tw, tf, k, a, ix, sx, zx), Aisc);

    public static FrozenDictionary<ProfileId, Profile> BuildSteelRows(Context context) =>
        SteelRows
            .Choose(row => SteelShapeOf(row, context, default).ToOption())
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Steel, unit, Coring.None, shape.Standard, MaterialId.Of("metal.iron")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ProfileKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [AISC_ROW_TRANSCRIPTION]: the AISC Shapes Database v16.0 carries all standard US steel shapes (1873-2016) with the depth/flange-width/web/flange-thickness/fillet dimensional columns and the `Ix`/`Sx`/`Zx`/`A` section-property columns in both US-customary and SI units; the realized seed spans all six `SteelClass` cases — W-shape (`IShape`), C/MC channel (`UShape`), L angle (`LShape`), rectangular and round HSS (`HssRect`/`HssRound`), and WT tee (`Tee`) — and the remaining rows in every family are pure `SteelRow` data additions to `SteelRows`, each carrying its `SteelClass` discriminant, never a new type. The transcription reads the SI-unit AISC columns directly; a designation keys `steel.<edi-designation>`. The raw `SteelRow` carries plain doubles and admits once through `SteelShapeOf` into the kernel value-objects, so the catalogue seed validates every column rather than trusting a literal.
- [POSITIVE_MAGNITUDE_ADMISSION]: the kernel value-objects admit through `key.AcceptValidated<TVO>(candidate:)` (the `Op` extension returning `Fin<TVO>` for any `IObjectFactory<TVO, double/int, ValidationError>`), not a `PositiveMagnitude.Of(value, context, key)` overload — no such `.Of(double, Context, Op)` exists on `Dimension`, `PositiveMagnitude`, or `UnitInterval`; the already-valid-literal route is the Thinktecture total `PositiveMagnitude.Create(value:)`. AISC v16.0 dimensional columns are fractional millimeters (`TwMm = 9.02`, `KMm = 24.4`), so every section column — depth/flange/web/fillet and the `Ix`/`Sx`/`Zx`/`A` stiffness columns — admits as the double-backed `PositiveMagnitude` (`> 0`, finite); the int-backed `Dimension` (`>= 1` discrete count) would truncate a fractional web thickness and corrupt the `FlangeSlenderness`/`WebSlenderness`/`Classify` slenderness verdicts and the `Capacity` LRFD projection, so it is never the carrier for a continuous measured dimension. A non-positive or non-finite column rails the value-object's own `Fin`, so a malformed AISC row drops from `BuildSteelRows` through `Choose` rather than seeding a degenerate `Profile`.
- [COMPOSITE_AND_BUILTUP]: the realized roster covers the AISC rolled shapes across all six `SteelClass` cases with the `DesignCapacity` LRFD receipt derived per section; built-up plate girders, composite steel-concrete sections (AISC 360 Chapter I, referencing the `JOINT_CONNECTION_FAMILY` shear-stud `StudClass` for the interface), and cold-formed AISI S100 members are the remaining `SteelClass` growth axis (one case per `IfcProfileDef` subtype) plus their own catalogue rows, never a parallel section owner — the `SteelSection` stiffness columns already carry the design receipt a composite section computes from its plate geometry. This growth is the open `STEEL_COMPOSITE_AND_COLDFORMED` task.
- [HSS_COLUMN_REUSE]: `Ix`/`Sx`/`Zx` admit as `PositiveMagnitude` so a zero-stiffness section is unrepresentable; an HSS-round section whose `FlangeThicknessMm`/`FilletMm` columns are inapplicable carries the wall thickness in `WebThicknessMm` and a small positive-finite `FilletMm` the `PositiveMagnitude` adapter admits (a zero fillet is mapped to the wall radius, never to zero, since the column is `> 0`), the `SteelClass.HssRound` discriminant routing the `IfcProfileDef` wire to the circle-hollow subtype.
```
