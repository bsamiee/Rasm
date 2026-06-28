# [MATERIALS_STEEL]

THE STEEL PROFILEFAMILY GROUNDED IN THE PUBLISHED SECTION DATABASE. The steel cross-section vocabulary — the `VividOrange.Profiles.Catalogue` AISC Shapes Database v16.0 (2299 American) and EN 10365:2017 (558 European) published sections as typed sealed-singleton profile classes carrying real `UnitsNet.Length` geometry, the `VividOrange.Sections.SectionProperties` Green's-theorem polygon-integral solver computing every section property from that geometry, and the `IfcProfileDef` I-shape/U-shape/L-shape/double-angle/HSS/tee/composite/cold-formed subtype discriminant — is the second realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Steel` case, spanning the full structural-steel product range: rolled shapes, AISC 360 Chapter I composite steel-concrete sections, and AISI S100 cold-formed light-gauge members. A wide-flange section is a `Profile` row keyed by a `CatalogueFactory.CreateAmerican(American)` / `CreateEuropean(European)` identity, never a `WSection` type and never a hand-keyed dimension literal: the section shape, the published geometry, the computed stiffness receipt, and the composite/cold-formed detail are steel-`Profile` columns sourced from the registered database, and the `SteelSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry family drives — a steel member extrudes through one `Profile` over the `RunPath`, never a per-family layout. The steel vocabulary grows by data — a new section is one `American`/`European` identity in the seed, a new subtype one `SteelClass` case — never a per-section type and never a transcribed section-property literal. A composite floor beam references the `Connection/joint#JOINT_FAMILY` shear-stud `StudClass` for its steel-concrete shear interface, the one stud vocabulary, never a parallel stud owner. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape, `VividOrange.Profiles.Catalogue` + `VividOrange.Sections.SectionProperties` for the published geometry and its computed properties, the in-folder `Appearance/photometric#PHOTOMETRIC` `MaterialUnits` boundary for the `UnitsNet`→SI-millimetre admission, and the `Rasm` kernel `PositiveMagnitude` for every admitted length/property column; cmu/timber/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[STEEL_FAMILY]: the `SteelClass` subtype axis (i-shape · u-shape · l-shape · double-angle · hss-rect · hss-round · tee · composite · cold-formed) folded onto the catalogue `AmericanShape`/`EuropeanShape` family taxonomy, the `SteelSection` section-property record with strong-AND-weak-axis stiffness columns and the `CompositeDetail`/`ColdFormedDetail` augmentation, the `SectionReader` `VividOrange` catalogue+solver admission boundary, the `CompactnessClass` slenderness verdict, the `DesignCapacity` LRFD projection (rolled F/E/G · AISC 360 Ch I composite · AISI S100 cold-formed), the `ProfileUnit` projection that flows a steel section through the masonry-shaped `Resolve` fold, and the `ProfileCatalogue.BuildSteelRows` catalogue-seeded row table.

## [02]-[STEEL_FAMILY]

- Owner: the steel section vocabulary (`SteelClass` the `IfcProfileDef` subtype discriminant folded onto the catalogue `AmericanShape`/`EuropeanShape` family enum, `SteelSection` the computed section-property record, `CompactnessClass` the Table B4.1 verdict, `DesignCapacity` the LRFD receipt); `SectionReader` the `VividOrange.Profiles.Catalogue`+`VividOrange.Sections.SectionProperties` admission boundary that reads one `ICatalogue` into one `SteelSection`; `ProfileCatalogue.BuildSteelRows` the catalogue-seeded registered-row seed `profile#PROFILE_OWNER` composes; the `SteelSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`, the `SteelSection.Capacity` projection the structural-design seam reads.
- Cases: class {i-shape (W/M/S/HP), u-shape (C/MC channel), l-shape (L angle), double-angle (2L from `AmericanShape.DoubleL`), hss-rect, hss-round (round-HSS + Pipe), tee (WT/MT/ST), composite (AISC 360 Ch I steel-concrete), cold-formed (AISI S100 light-gauge)} — the `IfcProfileDef` parameterized-profile subtype set folded onto the published `AmericanShape` (13) / `EuropeanShape` (25) family taxonomy by `SteelClass.OfShape`; a section is a `SteelSection` row over one `SteelClass` (the composite/cold-formed rows carrying their `CompositeDetail`/`ColdFormedDetail` `Option`), never a section subtype, a parallel composite owner, or a parallel section-family enum duplicating `AmericanShape`/`EuropeanShape`.
- Entry: `public static Fin<SteelSection> SectionReader.Read(ICatalogue catalogue, Op key)` — the ONE `VividOrange` admission boundary: it dispatches the `IAmericanCatalogue.Shape`/`IEuropeanCatalogue.Shape` family onto `SteelClass`, reads the dimensional columns from the family geometry interface (`II`/`IIParallelFlange`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`+`IHollowStructuralSection`) as `UnitsNet.Length`, runs `new SectionProperties(catalogue)` over the polymorphic `IProfile` for `Area`/`MomentOfInertiaYy`/`Zz`/`ElasticSectionModulusYy`/`Zz`/`RadiusOfGyrationYy`/`Zz`, and admits every quantity to its SI-millimetre scalar through the in-folder `MaterialUnits` boundary into the kernel `PositiveMagnitude` columns once. `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `SteelSection` is the section→`ProfileUnit` projection (`WidthMm` = flange width or HSS breadth, `HeightMm`/`CourseHeightMm` = section depth, `LengthMm` = a unit-segment placeholder the `RunPath` length overrides) so a steel member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public CompactnessClass Classify(double yieldStressMpa)` the AISC Table B4.1 flange-AND-web slenderness verdict (compact/noncompact/slender), and `public DesignCapacity Capacity(double yieldMpa, double unbracedLengthMm, double effectiveLengthMm)` the `[BoundaryAdapter]` LRFD projection emitting `φMn` (flexural — yielding/LTB/FLB governing for a rolled shape, the AISC 360 Ch I plastic composite moment for a `Composite` section over its slab + `StudClass` horizontal shear, the AISI S100 effective-section `Seff·Fy` for a `ColdFormed` section), `φPn` (compression — flexural buckling over the WEAK-axis radius `min(rx, ry)` the solver supplies both axes for), and `φVn` (shear) from the computed `Zx`/`Sx`/`Ix`/`Iy`/`Area` columns plus the `CompositeDetail`/`ColdFormedDetail` `Option` when present — every capacity a derived projection over the computed columns, never a re-minted dimension or a parallel composite owner.
- Packages: VividOrange.Profiles.Catalogue (`CatalogueFactory.CreateAmerican`/`CreateEuropean` → `ICatalogue`/`IProfile`, the `American`/`European` identity enums, the `AmericanShape`/`EuropeanShape` family enums, the `II`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`/`IHollowStructuralSection` `UnitsNet.Length` geometry interfaces; `.api/api-vividorange-profiles-catalogue.md`), VividOrange.Sections.SectionProperties (`new SectionProperties(IProfile)` + the `Area`/`MomentOfInertiaYy`/`Zz`/`ElasticSectionModulusYy`/`Zz`/`RadiusOfGyrationYy`/`Zz`/`Perimeter` `UnitsNet` properties; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Materials (`EnSteelMaterial`/`EnSteelFactory.CreateLinearElastic` the Table 3.1 `f_y` by grade × `EnSteelSpecification` × thickness-band — the `SteelGrade.YieldMpa(thicknessMm)` source; the `ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException` derivation throws trapped at the grade admission; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1993` the grade cites; `.api/api-vividorange-standards.md`), UnitsNet (the `MaterialUnits` in-folder SI-millimetre coercion, the `Pressure`/`Length` from the EN factory; `.api/api-unitsnet.md`), Rasm (project — `PositiveMagnitude` for every admitted column), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the steel vocabulary grows by data — a new AISC/EN section is one `American`/`European` identity added to `AmericanSeed`/`EuropeanSeed` (the catalogue carries all 2299 + 558 published sections; the seed selects the realized subset, every other section already present in the database behind its enum value), a new shape family one `SteelClass` case carrying its `IfcProfileDef` subtype mapping and its `SteelClass.OfShape` fold arm, a new built-up section one parametric `Perimeter`-backed `SteelSection` over the matching class — never a per-section type, never a transcribed section-property literal, never a parallel composite/cold-formed owner. A cmu/timber/glazing family lands its own vocabulary on its own page the way steel carries `SteelClass`/`SteelSection`.
- Boundary: the steel vocabulary is the second realized `ProfileFamily` — a per-section class AND a hand-keyed section-property literal table are the deleted forms; `SectionReader.Read` is the BOUNDARY_ADMISSION point where raw `VividOrange` `UnitsNet` geometry is admitted EXACTLY ONCE — the catalogue's published dimensions (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the quantity) and the solver's computed `Area`/`AreaMomentOfInertia`/`Volume`/`Length` properties coerce to SI-millimetre scalars through the in-folder `MaterialUnits` boundary and admit into the kernel `PositiveMagnitude` (double-backed `> 0` finite), so the section never re-mints a length primitive, a fractional AISC web thickness admits without truncation, and the interior carries raw SI doubles never a `UnitsNet` type in a signature; the `Ix`/`Iy`/`Sx`/`Sy`/`Zx` stiffness columns are computed `PositiveMagnitude` receipts the `IfcProfileDef` wire and the `DesignCapacity` LRFD projection read, never hand-keyed, never raw doubles; the `RadiusOfGyrationMm` hand-`Math.Sqrt(Ix/A)` is the deleted form — both `RxMm` and `RyMm` come from the solver `RadiusOfGyrationYy`/`Zz`, so the `Capacity` flexural-buckling check governs about the WEAK axis (`min(rx, ry)`) the way real column design does, never the strong-axis-only approximation; the plastic `Zx` the polygon integral cannot yield is the one DERIVED column — `PlasticModulus.Yy` computes it from the admitted dimensional columns per `SteelClass` (the I/channel/tee rectangular-component sum, the HSS-rect/round closed form, the angle shape-factor), grounding it in geometry not a literal; the `IsCompact` bool is the deleted form — `SteelSection.Classify` returns the 3-state `CompactnessClass` (compact/noncompact/slender) over AISC Table B4.1 flange-AND-web limits and `SteelSection.Capacity` derives the LRFD `φMn`/`φPn`/`φVn` from the computed stiffness columns; `SteelSection.ToUnit` is the ONE bridge from the section-property vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes — a steel run is the same station-stepped fold over a single-orientation course, so a beam/column is a `ProfileSet` extrusion (`assembly#MATERIAL_COMPOSITION` `ProfileSet`) along the `RunPath`, never a masonry-style multi-unit course; `SteelClass.OfShape` folds the published `AmericanShape`/`EuropeanShape` family taxonomy onto the `SteelClass` discriminant so the catalogue's own 13/25-family axis IS the section discriminant and each shape maps to its `IfcProfileDef` subtype (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef`, the `DoubleL`/`Composite` sections to `IfcArbitraryClosedProfileDef` since neither has a single parametric form, the `ColdFormed` channel-stud to `IfcUShapeProfileDef`) so a steel member round-trips to IFC 4.3 as an `IfcMaterialProfileSet`; the `Composite` section reads the `Connection/joint#JOINT_FAMILY` `StudClass` for its shear interface (the composite `Qn` per stud the `joint#JOINT_FAMILY` `JointSection.AllowableForceKn` develops, the horizontal shear `ΣQn` the composite plastic-moment couple caps), and the `ColdFormed` section reads its AISI S100 `EffectiveSectionModulusRatio` for the local-buckling flexural reduction, both derived projections over the computed columns; `ProfileCatalogue.BuildSteelRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table by folding `AmericanSeed`/`EuropeanSeed` through `CatalogueFactory` and `SectionReader.Read`, keyed `steel.<designation>`, the realized cross-section grounded in the registered published database rather than a hand-transcribed table.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using VividOrange.Profiles;                          // CatalogueFactory, American/European, AmericanShape/EuropeanShape, ICatalogue, II/IChannel/...
using VividOrange.Sections.SectionProperties;        // SectionProperties polygon-integral solver over IProfile
using VividOrange.Materials.StandardMaterials.En;    // EnSteelGrade, EnSteelMaterial, EnSteelFactory (the Table 3.1 f_y source)

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class SteelClass {
    public static readonly SteelClass IShape      = new("i-shape", ifcSubtype: "IfcIShapeProfileDef");
    public static readonly SteelClass UShape      = new("u-shape", ifcSubtype: "IfcUShapeProfileDef");
    public static readonly SteelClass LShape      = new("l-shape", ifcSubtype: "IfcLShapeProfileDef");
    public static readonly SteelClass DoubleAngle = new("double-angle", ifcSubtype: "IfcArbitraryClosedProfileDef");
    public static readonly SteelClass HssRect     = new("hss-rect", ifcSubtype: "IfcRectangleHollowProfileDef");
    public static readonly SteelClass HssRound    = new("hss-round", ifcSubtype: "IfcCircleHollowProfileDef");
    public static readonly SteelClass Tee         = new("tee", ifcSubtype: "IfcTShapeProfileDef");
    public static readonly SteelClass Composite   = new("composite", ifcSubtype: "IfcArbitraryClosedProfileDef");
    public static readonly SteelClass ColdFormed  = new("cold-formed", ifcSubtype: "IfcUShapeProfileDef");
    public string IfcSubtype { get; }
    public bool IsComposite => this == Composite;
    public bool IsColdFormed => this == ColdFormed;

    // The published AISC/EN family taxonomy IS the discriminant: fold AmericanShape (13) / EuropeanShape (25) onto
    // the SteelClass set rather than minting a parallel section-family enum (api-vividorange-profiles-catalogue [04]).
    public static SteelClass OfShape(AmericanShape shape) => shape switch {
        AmericanShape.W or AmericanShape.M or AmericanShape.S or AmericanShape.HP => IShape,
        AmericanShape.C or AmericanShape.MC                                       => UShape,
        AmericanShape.L                                                           => LShape,
        AmericanShape.DoubleL                                                     => DoubleAngle,
        AmericanShape.HSS                                                         => HssRect,
        AmericanShape.Pipe                                                        => HssRound,
        AmericanShape.WT or AmericanShape.MT or AmericanShape.ST                  => Tee,
        _                                                                         => IShape,
    };
    public static SteelClass OfShape(EuropeanShape shape) => shape switch {
        EuropeanShape.UPE or EuropeanShape.PFC or EuropeanShape.UPN or EuropeanShape.U or EuropeanShape.CH => UShape,
        EuropeanShape.UBP or EuropeanShape.UB or EuropeanShape.UC or EuropeanShape.J                       => IShape,
        _                                                                                                  => IShape,
    };
}

// The AISC Table B4.1 width-to-thickness verdict — a 3-state design class, never a 2-state IsCompact flag.
[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new();
    public static readonly CompactnessClass Noncompact = new();
    public static readonly CompactnessClass Slender    = new();
}

// The structural-steel grade band bound to its VividOrange.Materials EnSteelGrade — the yield strength the LRFD Capacity
// reads from EnSteelFactory.CreateLinearElastic (the EN 1993-1-1 Table 3.1 f_y by grade × specification × thickness-band)
// rather than a caller-supplied raw double, so a steel member's design yield is the registered grade DATA, never hand-keyed.
// The AISC bands (a36/a992/a572) carry None and fall back to their spec-nominal yield; the EN bands resolve EnSteelFactory.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class SteelGrade {
    public static readonly SteelGrade A36   = new("a36",   nominalYieldMpa: 250.0, enGrade: None);
    public static readonly SteelGrade A992  = new("a992",  nominalYieldMpa: 345.0, enGrade: None);
    public static readonly SteelGrade A572  = new("a572",  nominalYieldMpa: 345.0, enGrade: None);
    public static readonly SteelGrade S235  = new("s235",  nominalYieldMpa: 235.0, enGrade: Some(EnSteelGrade.S235));
    public static readonly SteelGrade S275  = new("s275",  nominalYieldMpa: 275.0, enGrade: Some(EnSteelGrade.S275));
    public static readonly SteelGrade S355  = new("s355",  nominalYieldMpa: 355.0, enGrade: Some(EnSteelGrade.S355));
    public static readonly SteelGrade S460  = new("s460",  nominalYieldMpa: 460.0, enGrade: Some(EnSteelGrade.S460));
    public double NominalYieldMpa { get; }
    public Option<EnSteelGrade> EnGrade { get; }

    // The thickness-banded design yield: an EN grade reads EnSteelFactory.CreateLinearElastic Table 3.1 f_y (≤40 mm vs
    // 40-80 mm), trapping the EN derivation throw onto ProfileFault; an AISC band returns its spec-nominal yield.
    public Fin<double> YieldMpa(double elementThicknessMm, NationalAnnex annex, Op key) =>
        EnGrade.Match(
            Some: g => Try(() => EnSteelFactory.CreateLinearElastic(new EnSteelMaterial(g, annex), Length.FromMillimeters(elementThicknessMm)).Strength.Megapascals).ToFin()
                .MapFail(e => ProfileFault.Family(key, $"<en-steel-grade:{g}:{annex}:{e.Message}>")),
            None: () => Fin.Succ(NominalYieldMpa));
}

// --- [MODELS] ------------------------------------------------------------------------------
// Dims read off the family geometry interface as UnitsNet.Length (carried in their native published unit), then
// coerced once to SI millimetres at SectionReader.Read; Fillet is Option because only IIParallelFlange carries it.
public readonly record struct SectionDims(double DepthMm, double FlangeWidthMm, double WebThicknessMm, double FlangeThicknessMm, double FilletMm);

public readonly record struct SteelSection(
    SteelClass Class,
    PositiveMagnitude DepthMm,
    PositiveMagnitude FlangeWidthMm,
    PositiveMagnitude WebThicknessMm,
    PositiveMagnitude FlangeThicknessMm,
    PositiveMagnitude FilletMm,
    PositiveMagnitude AreaMm2,
    PositiveMagnitude IxMm4,        // MomentOfInertiaYy — strong-axis second moment from the solver
    PositiveMagnitude IyMm4,        // MomentOfInertiaZz — weak-axis second moment from the solver
    PositiveMagnitude SxMm3,        // ElasticSectionModulusYy from the solver
    PositiveMagnitude SyMm3,        // ElasticSectionModulusZz from the solver
    PositiveMagnitude ZxMm3,        // plastic modulus — DERIVED from geometry (the solver yields only elastic)
    PositiveMagnitude RxMm,         // RadiusOfGyrationYy from the solver
    PositiveMagnitude RyMm,         // RadiusOfGyrationZz from the solver — governs flexural buckling
    Option<CompositeDetail> Composite = default,
    Option<ColdFormedDetail> ColdFormed = default) {

    public double FlangeSlenderness => FlangeWidthMm.Value / (2.0 * FlangeThicknessMm.Value);
    public double WebSlenderness => (DepthMm.Value - 2.0 * FlangeThicknessMm.Value) / WebThicknessMm.Value;
    public double GoverningRadiusMm => Math.Min(RxMm.Value, RyMm.Value);   // weak-axis governs column buckling

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

    // AISC 360 Chapters F/E/G LRFD for rolled shapes: φMn = φb·Fy·Zx with the LTB/FLB reduction, φPn = φc·Fcr·A over
    // the WEAK-axis flexural-buckling stress, φVn = φv·0.6·Fy·Aw; the Composite arm runs AISC 360 Ch I plastic composite
    // Mn and the ColdFormed arm the AISI S100 effective-section Mn = Seff·Fy, each over the SAME computed columns.
    [BoundaryAdapter]
    public DesignCapacity Capacity(double yieldMpa, double unbracedLengthMm, double effectiveLengthMm) {
        const double φb = 0.90, φc = 0.90, φv = 0.90, E = 200_000.0;
        double r = GoverningRadiusMm, λc = effectiveLengthMm / r;
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        double Fcr = Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe;
        double Lb = unbracedLengthMm, Lp = 1.76 * RxMm.Value * Math.Sqrt(E / yieldMpa);
        double Mp = yieldMpa * ZxMm3.Value;
        double rolledMn = Lb <= Lp ? Mp : Math.Max(yieldMpa * SxMm3.Value, Mp - (Mp - 0.7 * yieldMpa * SxMm3.Value) * Math.Clamp((Lb - Lp) / (3.0 * Lp), 0.0, 1.0));
        double Mn = Composite.Match(Some: c => CompositeMn(c, yieldMpa), None: () => ColdFormed.Match(Some: cf => yieldMpa * SxMm3.Value * cf.EffectiveSectionModulusRatio, None: () => rolledMn));
        double Aw = DepthMm.Value * WebThicknessMm.Value;
        return new DesignCapacity(
            FlexuralNmm: φb * Mn,
            CompressionN: φc * Fcr * AreaMm2.Value,
            ShearN: φv * 0.6 * yieldMpa * Aw,
            Classification: Classify(yieldMpa),
            Slenderness: λc);
    }

    // AISC 360 Eq C-I3: the fully-composite plastic moment — the steel yields in tension (As·Fy) balanced by the
    // concrete compression block (0.85·f'c·b·a), the moment the couple about the slab; capped at the stud horizontal
    // shear ΣQn the Connection/joint#JOINT_FAMILY StudClass develops (partial composite action when ΣQn < As·Fy).
    private double CompositeMn(CompositeDetail c, double yieldMpa) {
        double cConc = 0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value;
        double tSteel = AreaMm2.Value * yieldMpa;
        double horizShear = Math.Min(tSteel, c.ConcreteFcMpa * 0.85 * c.SlabEffectiveWidthMm.Value * c.SlabDepthMm.Value);
        double a = Math.Min(c.SlabDepthMm.Value, horizShear / Math.Max(cConc, double.Epsilon));
        double leverArm = 0.5 * DepthMm.Value + c.SlabDepthMm.Value - 0.5 * a;
        return horizShear * leverArm;
    }
}

public readonly record struct DesignCapacity(double FlexuralNmm, double CompressionN, double ShearN, CompactnessClass Classification, double Slenderness);

// AISC 360 Chapter I composite-action detail: the concrete slab over the steel core, plus the shear-stud reference
// into the Connection/joint#JOINT_FAMILY StudClass vocabulary the composite shear interface develops.
public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    int StudsPerMetre);

// AISI S100 effective-width detail: a cold-formed section reduces to an effective section under local buckling,
// the effective-section-modulus Seff/S ratio the local-buckling reduction yields over the gross/effective width.
public readonly record struct ColdFormedDetail(
    PositiveMagnitude GrossWidthMm,
    PositiveMagnitude EffectiveWidthMm,
    PositiveMagnitude CornerRadiusMm,
    PositiveMagnitude DesignThicknessMm,
    double EffectiveSectionModulusRatio);

public sealed record SteelShape(ProfileId Id, SteelSection Section, ProfileStandard Standard);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The plastic modulus the polygon integral cannot yield (it returns elastic S = I/c): derived from the admitted
// dimensional columns per SteelClass — the I/channel rectangular-component sum, the HSS closed forms, the angle/tee
// shape-factor over the solver's elastic Sx. One derived projection grounding Zx in geometry, never a literal.
public static class PlasticModulus {
    public static double Yy(SteelClass cls, double d, double bf, double tw, double tf, double sx) =>
        cls.Switch(
            iShape:      _ => bf * tf * (d - tf) + 0.25 * tw * Math.Pow(Math.Max(0.0, d - 2.0 * tf), 2.0),
            uShape:      _ => bf * tf * (d - tf) + 0.25 * tw * Math.Pow(Math.Max(0.0, d - 2.0 * tf), 2.0),
            tee:         _ => 1.7 * sx,
            lShape:      _ => 1.6 * sx,
            doubleAngle: _ => 1.6 * sx,
            hssRect:     _ => 0.25 * bf * d * d - 0.25 * Math.Max(0.0, bf - 2.0 * tw) * Math.Pow(Math.Max(0.0, d - 2.0 * tw), 2.0),
            hssRound:    _ => (Math.Pow(d, 3.0) - Math.Pow(Math.Max(0.0, d - 2.0 * tw), 3.0)) / 6.0,
            composite:   _ => 1.14 * sx,
            coldFormed:  _ => sx);
}

// The ONE VividOrange admission boundary: an ICatalogue (a polymorphic IProfile carrying published UnitsNet.Length
// geometry) → its family-cast dimensional columns + the SectionProperties polygon-integral solver outputs, every
// UnitsNet quantity coerced to an SI-millimetre scalar and admitted once into the kernel PositiveMagnitude columns.
public static class SectionReader {
    public static Fin<SteelSection> Read(ICatalogue catalogue, Op key) =>
        from cls in ResolveClass(catalogue, key)
        let props = new SectionProperties((IProfile)catalogue)   // solver consumes the polymorphic IProfile — no family cast
        from dims in ReadDims(catalogue, key)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: dims.DepthMm)
        from bf in key.AcceptValidated<PositiveMagnitude>(candidate: dims.FlangeWidthMm)
        from tw in key.AcceptValidated<PositiveMagnitude>(candidate: dims.WebThicknessMm)
        from tf in key.AcceptValidated<PositiveMagnitude>(candidate: dims.FlangeThicknessMm)
        from fillet in key.AcceptValidated<PositiveMagnitude>(candidate: Math.Max(dims.FilletMm, double.Epsilon))
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: props.Area.SquareMillimeters)
        from ix in key.AcceptValidated<PositiveMagnitude>(candidate: props.MomentOfInertiaYy.MillimetersToTheFourth)
        from iy in key.AcceptValidated<PositiveMagnitude>(candidate: props.MomentOfInertiaZz.MillimetersToTheFourth)
        from sx in key.AcceptValidated<PositiveMagnitude>(candidate: props.ElasticSectionModulusYy.CubicMillimeters)
        from sy in key.AcceptValidated<PositiveMagnitude>(candidate: props.ElasticSectionModulusZz.CubicMillimeters)
        from rx in key.AcceptValidated<PositiveMagnitude>(candidate: props.RadiusOfGyrationYy.Millimeters)
        from ry in key.AcceptValidated<PositiveMagnitude>(candidate: props.RadiusOfGyrationZz.Millimeters)
        from zx in key.AcceptValidated<PositiveMagnitude>(candidate: PlasticModulus.Yy(cls, dims.DepthMm, dims.FlangeWidthMm, dims.WebThicknessMm, dims.FlangeThicknessMm, sx.Value))
        select new SteelSection(cls, depth, bf, tw, tf, fillet, area, ix, iy, sx, sy, zx, rx, ry);

    static Fin<SteelClass> ResolveClass(ICatalogue catalogue, Op key) => catalogue switch {
        IAmericanCatalogue a => Fin.Succ(SteelClass.OfShape(a.Shape)),
        IEuropeanCatalogue e => Fin.Succ(SteelClass.OfShape(e.Shape)),
        _ => Fin.Fail<SteelClass>(ProfileFault.Family(key, $"<catalogue-not-american-or-european:{catalogue.Label}>")),
    };

    // The family geometry read: the dimensional columns the LRFD slenderness reads come off the family interface the
    // AmericanShape/EuropeanShape discriminant selects, as UnitsNet.Length in the native published unit (.Millimeters
    // converts). HSS rides the rectangle envelope + wall thickness, round-HSS the diameter + wall (HSS_COLUMN_REUSE).
    // An unmatched catalogue geometry interface rails ProfileFault.Family — never a fabricated (1,1,1,1) sentinel that
    // would pass every PositiveMagnitude admission and seed a degenerate 1mm SteelSection (the deleted ghost form).
    static Fin<SectionDims> ReadDims(ICatalogue catalogue, Op key) => catalogue switch {
        IIParallelFlange i => Fin.Succ(new SectionDims(i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, i.FilletRadius.Millimeters)),
        II i               => Fin.Succ(new SectionDims(i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, 0.0)),
        IChannel c         => Fin.Succ(new SectionDims(c.Height.Millimeters, c.Width.Millimeters, c.WebThickness.Millimeters, c.FlangeThickness.Millimeters, 0.0)),
        ITee t             => Fin.Succ(new SectionDims(t.Height.Millimeters, t.Width.Millimeters, t.WebThickness.Millimeters, t.FlangeThickness.Millimeters, 0.0)),
        IDoubleAngle da    => Fin.Succ(new SectionDims(da.Height.Millimeters, da.Width.Millimeters, da.WebThickness.Millimeters, da.FlangeThickness.Millimeters, 0.0)),
        IAngle an          => Fin.Succ(new SectionDims(an.Height.Millimeters, an.Width.Millimeters, an.WebThickness.Millimeters, an.FlangeThickness.Millimeters, 0.0)),
        IRectangularHollow rh when catalogue is IHollowStructuralSection h => Fin.Succ(new SectionDims(rh.Height.Millimeters, rh.Width.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0)),
        ICircularHollow ch when catalogue is IHollowStructuralSection h    => Fin.Succ(new SectionDims(ch.Diameter.Millimeters, ch.Diameter.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0)),
        _ => Fin.Fail<SectionDims>(ProfileFault.Family(key, $"<catalogue-geometry-interface-unsupported:{catalogue.Label}>")),
    };
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly ProfileStandard Aisc = new("us", StandardJointThicknessMm: 0.0, Authority: "AISC v16.0");
    static readonly ProfileStandard En   = new("eu", StandardJointThicknessMm: 0.0, Authority: "EN 10365:2017");

    // The seed selects identities from the registered database (2299 American / 558 European); a new section is one
    // enum value added here, its full geometry and computed properties already behind the CatalogueFactory singleton.
    static readonly Seq<American> AmericanSeed = Seq(
        American.W12x26, American.W14x90, American.W18x50, American.W21x68, American.W24x76,
        American.C15x33_9, American.MC18x45_8, American.L6x6x3over4, American.L4x4x1over2,
        American.HSS8x8x_500, American.HSS6x4x_375, American.WT9x38, American.WT6x25);

    static readonly Seq<European> EuropeanSeed = Seq(
        European.IPE300, European.IPE450, European.HE300A, European.HE400B, European.UPN200);

    static Fin<SteelShape> AmericanShapeOf(American id, Op key) =>
        from section in SectionReader.Read(CatalogueFactory.CreateAmerican(id), key)
        select new SteelShape(ProfileId.Of($"steel.{id.ToString().ToLowerInvariant()}"), section, Aisc);

    static Fin<SteelShape> EuropeanShapeOf(European id, Op key) =>
        from section in SectionReader.Read(CatalogueFactory.CreateEuropean(id), key)
        select new SteelShape(ProfileId.Of($"steel.{id.ToString().ToLowerInvariant()}"), section, En);

    // A composite section is a rolled I-shape steel core augmented with the AISC 360 Ch I slab + Connection/joint
    // StudClass shear-stud detail; a cold-formed section is the AISI S100 light-gauge channel with its effective-width
    // reduction. Both ride the SAME SteelSection over the composite/cold-formed SteelClass discriminant + Option detail.
    static Fin<SteelShape> CompositeOf(string designation, American core, CompositeDetail detail, Op key) =>
        from baseShape in AmericanShapeOf(core, key)
        select baseShape with { Id = ProfileId.Of(designation), Section = baseShape.Section with { Class = SteelClass.Composite, Composite = Some(detail) } };

    static Fin<SteelShape> ColdFormedOf(string designation, American gauge, ColdFormedDetail detail, Op key) =>
        from baseShape in AmericanShapeOf(gauge, key)
        select baseShape with { Id = ProfileId.Of(designation), Section = baseShape.Section with { Class = SteelClass.ColdFormed, ColdFormed = Some(detail) } };

    static Fin<Seq<SteelShape>> CompositeColdFormedRows(Op key) =>
        Seq(
            // W18x50 acting compositely with a 1200×100 mm normal-weight slab (f'c 28 MPa), 3/4in studs at 2/m.
            CompositeOf("steel.comp-w18x50-slab120", American.W18x50,
                new CompositeDetail(PositiveMagnitude.Create(1200.0), PositiveMagnitude.Create(100.0), 28.0, StudClass.S19, 2), key),
            // 600S162-54 AISI S100 stud reduced to a C-channel core with an effective-section modulus 0.78 of gross.
            ColdFormedOf("steel.cf-600s162-54", American.C15x33_9,
                new ColdFormedDetail(PositiveMagnitude.Create(152.0), PositiveMagnitude.Create(119.0), PositiveMagnitude.Create(4.76), PositiveMagnitude.Create(1.37), 0.78), key))
        .Sequence();

    public static FrozenDictionary<ProfileId, Profile> BuildSteelRows(Context context) =>
        AmericanSeed.Choose(id => AmericanShapeOf(id, default).ToOption())
            .Concat(EuropeanSeed.Choose(id => EuropeanShapeOf(id, default).ToOption()))
            .Concat(CompositeColdFormedRows(default).IfFail(Seq<SteelShape>()))
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Steel, unit, Coring.None, shape.Standard, MaterialId.Of("metal.iron")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ProfileKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [CATALOGUE_GROUNDED_SEED]: REALIZED — the hand-keyed `SteelRow` literal table is the deleted form; `BuildSteelRows` seeds from the `VividOrange.Profiles.Catalogue` registered database (`CatalogueFactory.CreateAmerican(American)` / `CreateEuropean(European)` minting the sealed-singleton `IProfile` carrying published `UnitsNet.Length` geometry), and every section property is COMPUTED by the `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral over that `IProfile` rather than transcribed. The `AmericanSeed`/`EuropeanSeed` enum lists select the realized subset of the 2299 American + 558 European sections; a new section is one `American`/`European` enum value added to a seed, its full geometry and computed properties already registered behind the `CatalogueFactory` singleton — never a `SteelRow` of literal doubles. The catalogue's `IIParallelFlange.FilletRadius` and the HSS corner radii integrate EXACTLY through the solver's `.Utility.Parts` typed `EllipseQuarterPart` decomposition, so the computed `Ix`/`Sx` carry the fillet without the polygonized loss a straight shoelace would force (`api-vividorange-sections-sectionproperties.md` [POLYGON_INTEGRAL_CONTRACT]). The `SteelClass.OfShape` fold maps the catalogue's own `AmericanShape` (13) / `EuropeanShape` (25) family taxonomy onto the `SteelClass` discriminant, so the published family axis IS the section discriminant — never a parallel local enum.
- [BOUNDARY_ADMISSION_UNITS]: REALIZED — `SectionReader.Read` is the single point raw `VividOrange` `UnitsNet` is admitted: the catalogue dimensions (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the `Length`) and the solver's `Area`/`AreaMomentOfInertia`/`Volume`/`Length` outputs coerce to SI-millimetre scalars (`.Millimeters`/`.SquareMillimeters`/`.MillimetersToTheFourth`/`.CubicMillimeters`, `UnitsNet` owning the Inch→mm conversion in the accessor) and admit once into the kernel `PositiveMagnitude` columns; the interior carries raw SI doubles and no `UnitsNet` quantity crosses an interior signature, per the BOUNDARY_ADMISSION law the in-folder `MaterialUnits` boundary (`Appearance/photometric#PHOTOMETRIC`) already enforces. A non-positive or non-finite computed column rails the value-object's own `Fin`, so a malformed section drops from `BuildSteelRows` through `Choose` rather than seeding a degenerate `Profile`. The solver consumes the polymorphic `IProfile` for the area/inertia/modulus/radius outputs (no family cast); the family cast is needed ONLY for the dimensional columns the AISC Table B4.1 `Classify` slenderness verdict reads, dispatched by `ReadDims` over the `II`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow` interface the `AmericanShape`/`EuropeanShape` discriminant selects.
- [WEAK_AXIS_AND_PLASTIC]: REALIZED — the solver supplies BOTH axes (`MomentOfInertiaYy`/`Zz`, `ElasticSectionModulusYy`/`Zz`, `RadiusOfGyrationYy`/`Zz`), so `SteelSection` carries the weak-axis `IyMm4`/`SyMm3`/`RyMm` columns the hand-keyed table lacked and the `Capacity` flexural-buckling check governs about the WEAK axis (`GoverningRadiusMm => min(rx, ry)`) the way real column design does, replacing the strong-axis-only `Math.Sqrt(Ix/A)` approximation. The plastic modulus `Zx` the elastic polygon integral cannot yield is the one DERIVED column — `PlasticModulus.Yy` computes it from the admitted dimensional columns per `SteelClass` (the I/channel rectangular-component sum `Bf·Tf·(D−Tf) + ¼·Tw·(D−2Tf)²`, the HSS-rect/round closed forms, the angle/tee shape-factor over the solver's elastic `Sx`), grounding the plastic moment `Mp = Fy·Zx` in geometry not a literal.
- [COMPOSITE_AND_COLDFORMED]: REALIZED — the `SteelClass.Composite` (AISC 360 Chapter I steel-concrete) and `SteelClass.ColdFormed` (AISI S100 light-gauge) cases ride the SAME `SteelSection` with an `Option<CompositeDetail>`/`Option<ColdFormedDetail>` augmentation (defaulted `None` for rolled shapes) rather than a parallel section owner; both seed through `CompositeColdFormedRows` over a catalogued `American` core (the composite W18x50, the cold-formed C-channel core) with the detail applied through `with`. The `CompositeDetail` carries the slab effective-width/depth, the concrete `f'c`, and the `Connection/joint#JOINT_FAMILY` `StudClass` shear-stud reference + studs-per-metre; the `CompositeMn` projection runs the AISC 360 Ch I plastic composite moment capped at the stud horizontal shear `ΣQn` for partial composite action. The `ColdFormedDetail` carries the gross/effective width, corner radius, design thickness, and the `EffectiveSectionModulusRatio` the AISI S100 effective-width method yields, the cold-formed flexural `Mn = Seff·Fy` over the solver `Sx` scaled by the ratio. Both map to their `IfcProfileDef` subtype (`Composite`→`IfcArbitraryClosedProfileDef`, `ColdFormed`→`IfcUShapeProfileDef`). Ripple counterpart: `Connection/joint` `[JOINT_FAMILY]` (the shared `StudClass` the composite shear interface reads). A built-up plate girder remains a parametric `SteelSection` over a `VividOrange.Profiles.Perimeter` `Perimeter` (outer polyline + void edges) fed to the SAME `SectionProperties` solver, computing identically to a catalogued section.
- [IFCPROFILEDEF_SUBTYPE_AXIS]: `SteelClass` carries the full `IfcProfileDef` parameterized-profile subtype axis — `IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef` for the rolled families, `IfcArbitraryClosedProfileDef` for the `DoubleL`/`Composite` sections that have no single parametric form, `IfcUShapeProfileDef` for the cold-formed channel-stud — so a steel member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the `Construction/assembly#MATERIAL_COMPOSITION` `ProfileSet` shape). The reciprocal ingress is realized at the `Rasm.Bim` `Semantics/composition#MATERIAL_COMPOSITION` owner, so the round-trip is symmetric rather than a remaining probe: its `ProfileDefKind` discriminates the `IfcMaterialProfile.Profile` runtime type onto this same subtype axis, its `ProfileDims` folds the `IfcParameterizedProfileDef` members back onto the section `DepthMm`/`FlangeWidthMm`/`WebThicknessMm`/`FlangeThicknessMm`/`FilletMm` columns, the per-family `IfcMaterialProfileSetUsage` cardinal-point/orientation lands on the Bim `ProfileSetUsage` `CardinalPoint`/`ReferenceExtent`, and the `DoubleL` back-to-back spacing crosses on the `IDoubleAngle.BackToBackDistance` the catalogue carries onto the Bim `ProfileDims.BackToBackMm`. Ripple counterpart: `Rasm.Bim` `Semantics/composition` `[MATERIAL_COMPOSITION]`.
- [EN_STEEL_GRADE_YIELD]: REALIZED — the `SteelGrade` `[SmartEnum]` binds the structural-steel grade band to its `VividOrange.Materials` `EnSteelGrade` (the EN bands S235/S275/S355/S460 carrying `Some`, the AISC bands A36/A992/A572 carrying `None` and falling back to their spec-nominal yield), so `SteelGrade.YieldMpa(thicknessMm, annex, key)` reads the EN 1993-1-1 Table 3.1 `f_y` from `EnSteelFactory.CreateLinearElastic(new EnSteelMaterial(g, annex), Length)` — the thickness-banded design yield (≤40 mm vs 40-80 mm) the `.api/api-vividorange-materials.md` `[EN_TABLE_CONTRACT]` tabulates — rather than a caller-supplied raw `yieldMpa` double, the EN derivation `ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException` trapped onto `ProfileFault.Family` at the grade admission. The `SteelSection.Classify`/`Capacity` LRFD projections read the EN-registered thickness-banded `f_y` so a member's design yield is the registered grade DATA citing `En1993`, never a hand-keyed scalar; the realized capacity check stays the AISC 360 Chapters F/E/G derivation over the computed `SteelSection` columns, the EN grade supplying only the `f_y` strength the elastic/plastic moment scales. The `EnSteelFactory.CreateBiLinear` σ(ε) law and the per-grade ductility are a `Profiles/capacity#SECTION_CAPACITY` concern when a strain-based steel check lands, the elastic LRFD this owner's. Ripple counterpart: `Profiles/capacity` `[SECTION_CAPACITY]` (the unified capacity rail the steel `DesignCapacity` lifts into).
