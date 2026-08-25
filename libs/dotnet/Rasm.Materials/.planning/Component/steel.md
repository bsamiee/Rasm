# [MATERIALS_STEEL]

THE STEEL SEED FAMILY GROUNDED IN THE PUBLISHED SECTION DATABASE. `SteelSeed.Roster` is the full registered AISC American and EN 10365 European domain beside the generated SSMA cold-formed lattice and the fabricated rows, and `SteelSeed.Law` is the `SeedLaw<SteelRowSeed>` value `ComponentFamily.Steel` binds — the traverse, the coherence census, the profile route, the detail fold and the railed `Component.Of` lift all belong to `component#COMPONENT_SEED`. Each row carries one published `ICatalogue` identity, one policy-selected `MaterialGrade` steel row, one `SectionProfile.Catalogued` or admitted parametric profile, its section-map membership deriving from the profile's own topology. `SectionSolver.Solve` owns the twenty-column integral and the open-section supplement; `SteelDesign` owns the railed AISC/AISI/EN capacity projection, the composite augmentation, and the EN 1993-1-2 fire receipt over that receipt. `SteelClass` carries the profile taxonomy, the Table B4.1 slenderness row, the buckling-curve imperfection factors, the grade-band thickness selector, and the `IfcProfileDef` subtype the seeded realization bag publishes; `SteelJurisdiction` owns the classification ladder each `DesignBasis` runs. Growth stays a registered catalogue member, a policy row, or an authored fabricated row rather than a per-shape type.

## [01]-[INDEX]

- [02]-[STEEL_FAMILY]: the `SteelTopology` open/closed/solid axis, the `SteelClass` eleven-row subtype axis with the TOTAL `OfShape` folds, the `StainlessForm`/`StainlessRow`/`StainlessBands` EN 10088 proof-cell registry and the product-form recovery, the `GradeProperties.Steel` physics members (the thickness-banded EN yield, the stainless routing, the one `DesignYieldMpa` entry), the `SteelJurisdiction` ladder table with the EN 1993-1-4 reduced-ε row, the `SectionDims` published-dims currency, the `SteelShape.Of` catalogue admission boundary, the `CompositeDetail` augmentation, the generated `ColdFormedRow`/`ColdFormedSections` SSMA lattice, `CompactnessClass` + `SteelDesign`'s one polymorphic `Capacity` entry over the profile arm, the grade row and the `capacity#SECTION_CAPACITY` `DesignBasis`, the `SteelFireFacts` EN 1993-1-2 receipt, and the `SteelSeed.Roster`/`Law`/`Capacity` triple the policy row binds.

## [02]-[STEEL_FAMILY]

- Owner: `SteelTopology` the open/closed/solid discriminant; `SteelClass` the `IfcProfileDef` subtype axis folded onto the published taxonomies, carrying its Table B4.1 row, its §6.3 imperfection factors, and its grade-band thickness selector; `StainlessBands` the EN 10088 published proof-cell registry the stainless `MaterialGrade` rows bind; `SectionDims` the admitted published-dims currency; `SteelShape` the catalogued profile payload; `CompositeDetail` the composite augmentation row; `ColdFormedRow`/`ColdFormedSections` the generated SSMA designation lattice; `SteelRowSource` the closed profile-origin axis (rolled catalogue · published cold-formed row · fabricated build delegate); `SteelJurisdiction` the basis-keyed classification ladder; `CompactnessClass`/`DesignCapacity`/`SteelFireFacts`/`SteelDesign` the AISC 360 + AISI S100 + EN 1993 projection and the fire receipt; `SteelSeed` the roster and the seed law.
- Cases: class {i-shape (W/M/S/HP + the EN H/I families, open) · u-shape (C/MC/UPE/PFC/UPN/U/CH, open) · l-shape (L, open) · double-angle (2L, open) · hss-rect (closed) · hss-round (round HSS + Pipe, closed) · tee (WT/MT/ST, open) · composite (AISC 360 Ch I, open core) · cold-formed (AISI S100, open) · solid-bar / solid-round (solid stock)} × grade {the nineteen `ComponentFamily.Steel` `MaterialGrade` rows — AISC spec-nominal, EN Table 3.1 registered, EN 10088 published stainless} × topology {open · closed · solid} — a section is one seed row over one published identity; the composite variant is the SAME row with a `Some CompositeDetail` and a reclassed `SteelClass` on its `Rolled` source arm, and the cold-formed stud is the SAME row on its `Formed` source arm over a parametric `ColdFormedC` profile.
- Entry: `ComponentSeed.Rows(context, SteelSeed.Roster, SteelSeed.Law)` — this page states the roster and the policy, never the fold. `SteelDesign.Capacity` admits the rolled, cold-formed, or deck modality on the shape of its typed input and resolves the REGISTERED yield from the grade's `GradeProperties.Steel` arm at the class's own band thickness — the `CapacityPlacement` `DesignBasis` and `NationalAnnex` cross together, never a caller yield double. `SteelDesign.Fire(section, steelTemperatureC, utilisation, key)` is the ONE EN 1993-1-2 receipt entry.
- Packages: VividOrange.Profiles.Catalogue (`CatalogueFactory`, the `American`/`European` identity enums, the `II`/`IIParallelFlange`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`IRoundedRectangularHollow`/`ICircularHollow`+`IHollowStructuralSection` contracts), VividOrange.Materials (`EnSteelMaterial`/`EnSteelFactory.CreateLinearElastic`), VividOrange.Standards (`NationalAnnex`), MathNet.Numerics (`Interpolate.Linear` + `IInterpolation.Interpolate` — `libs/dotnet/.api/api-mathnet-numerics.md` rows `[10]`/`[INTERPOLATION_SEAM]`), UnitsNet (`Length` at the admission edge), Rasm.Numerics (`PositiveMagnitude`, `EpsilonPolicy`), Rasm.Domain (`Op`/`Context`/`AcceptValidated`, `ToleranceLane`/`Tolerance`), Rasm.Element (`MaterialId`, `EvidenceGrade`), Rasm.Materials.Component (`component#COMPONENT_OWNER`/`#MATERIAL_GRADE`/`#COMPONENT_SEED`, `capacity#SECTION_CAPACITY` `DesignBasis`/`SafetyFormat`/`CapacityPlacement`, `joint#JOINT_FAMILY` `StudClass`/`StudGroup`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: the seed IS the registered database (the full `American` and `European` identity domains enumerate through `Enum.GetValues` — a stocked subset is a policy filter over the roster, never the hard bound); a new composite variant one `Augmented` row with its detail; a new cold-formed stud one designation triple the `ColdFormedSections` lattice already generates; a new fabricated section one `Augmented` row over a `SteelRowSource.Plated` build delegate; a new grade one `MaterialGrade` steel row on `component#MATERIAL_GRADE` binding its EN designation or its `StainlessBands` registry row; a new DESIGN CODE one `capacity#SECTION_CAPACITY` `DesignBasis` row plus one `SteelJurisdiction` row and its resistance arm here; a new shape family one `SteelClass` row carrying topology, `FlexureRegime`, `IfcProfileDef` subtype, imperfection factors, band selector and `OfShape` arm, AND the compiler-forced `SectionProfile` arm and `SectionSolver.Solve`/`Forms` arm on `component#SECTION_SOLVER` — never a per-section type, never a transcribed property literal, never a parallel section receipt.
- Boundary: `SteelShape.Of` admits raw `VividOrange` geometry once; unsupported catalogue/profile implementations rail `ProfileMismatch`, while published dimensions lift into proven-positive SI `SectionDims` columns.
- Boundary: `SteelDesign.Capacity` binds yield from the admitted grade and product-form thickness band. Missing or unparsed published cells rail `GradeBandMissing`; documented provider refusals preserve their exact cause through `GradeDerivation`.
- Boundary: `SteelDesign` reads ONLY canonical `ComputedSection` columns (`Iw`, `GoverningRadiusMm`, `Avy`, `J/c`) — a re-minted dimension or a parallel `SteelBeamCheck` surface has no place here, and `DesignCapacity.TorsionalNmm`/`FlexuralMinorNmm` are the one source `CapacityReceipt.Steel` reads onto `SectionCapacity.SteelMember`. The DESIGN CODE is `DesignCapacity.Basis` DATA rather than a per-code receipt type, and the resistance BODY is the basis's own `ComponentAuthority`, so the retired `SteelBody` enum was one fact spelled twice. Steel carries `DetailLane.Realization` because `SteelClass.IfcSubtype` reaches the Bim profile lane only as a seeded `DetailSchema.ProfileSubtype` row. The AISI data path is CLOSED in-page — `FormOf` lowers the `ColdFormedC` and `Corrugated` arms straight onto `SectionDims`, so no reverse row lookup and no designation parse exists.
- Boundary: `SteelFireFacts` is the WHOLE EN 1993-1-2 surface this page publishes — the section factor, the Table 3.1 retention pair, and the §4.2.4 critical temperature in ONE railed receipt rather than three loose statics. Its consumer is LANDED: `capacity#SECTION_CAPACITY` `CapacityReceipt.Fire` mints over `FireState.Steel(DesignCapacity, SteelFireFacts)`; the family-side half is LANDED beside the seed: `SteelSeed.Capacity` dispatches on `CapacityPlacement.FireExposure` through the `SteelFire` §4.2.5.1 unprotected-member temperature step onto `CapacityReceipt.Fire`.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using VividOrange.Profiles;
using VividOrange.Materials.StandardMaterials.En;
using VividOrange.Standards.Eurocode;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelTopology {
    public static readonly SteelTopology Open   = new("open");
    public static readonly SteelTopology Closed = new("closed");
    public static readonly SteelTopology Solid  = new("solid");
}

[SmartEnum]
public sealed partial class FlexureRegime {
    public static readonly FlexureRegime F2      = new();
    public static readonly FlexureRegime F9      = new();
    public static readonly FlexureRegime F10     = new();
    public static readonly FlexureRegime Plastic = new();
}

public readonly record struct SlendernessRow(double FlangeDivisor, double FlangeLambdaP, double FlangeLambdaR, bool WebClear, double WebLambdaP, double WebLambdaR);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelClass {
    public static readonly SteelClass IShape      = new("i-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcIShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.34, ltbAlpha: 0.34, bandThicknessMm: Flange);
    public static readonly SteelClass UShape      = new("u-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(1, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Flange);
    public static readonly SteelClass LShape      = new("l-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcLShapeProfileDef",           regime: FlexureRegime.F10,     slenderness: Some(new SlendernessRow(1, 0.54, 0.91, false, 0.54, 0.91)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Flange);
    public static readonly SteelClass DoubleAngle = new("double-angle", topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F9,      slenderness: Some(new SlendernessRow(1, 0.54, 0.91, false, 0.54, 0.91)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Flange);
    public static readonly SteelClass HssRect     = new("hss-rect",     topology: SteelTopology.Closed, ifcSubtype: "IfcRectangleHollowProfileDef",  regime: FlexureRegime.Plastic, slenderness: Some(new SlendernessRow(1, 1.12, 1.40, true,  2.42, 5.70)), bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);
    public static readonly SteelClass HssRound    = new("hss-round",    topology: SteelTopology.Closed, ifcSubtype: "IfcCircleHollowProfileDef",     regime: FlexureRegime.Plastic, slenderness: Some(new SlendernessRow(1, 0.07, 0.31, false, 0.07, 0.31)), bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);
    public static readonly SteelClass Tee         = new("tee",          topology: SteelTopology.Open,   ifcSubtype: "IfcTShapeProfileDef",           regime: FlexureRegime.F9,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, false, 0.84, 1.52)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Flange);
    public static readonly SteelClass Composite   = new("composite",    topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.49, bandThicknessMm: Flange);
    public static readonly SteelClass ColdFormed  = new("cold-formed",  topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(1, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Wall);
    public static readonly SteelClass SolidBar    = new("solid-bar",    topology: SteelTopology.Solid,  ifcSubtype: "IfcRectangleProfileDef",        regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);
    public static readonly SteelClass SolidRound  = new("solid-round",  topology: SteelTopology.Solid,  ifcSubtype: "IfcCircleProfileDef",           regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);

    public SteelTopology Topology { get; }
    public string IfcSubtype { get; }
    public FlexureRegime Regime { get; }
    public Option<SlendernessRow> Slenderness { get; }

    public double BucklingAlpha { get; }

    public Option<double> LtbAlpha { get; }

    [UseDelegateFromConstructor] public partial double BandThicknessMm(SectionDims dims);
    static double Flange(SectionDims d) => d.FlangeMm.Value;
    static double Wall(SectionDims d) => d.WebMm.Value;

    public static Fin<SteelClass> OfShape(AmericanShape shape, Op key) => shape switch {
        AmericanShape.W or AmericanShape.M or AmericanShape.S or AmericanShape.HP => Fin.Succ(IShape),
        AmericanShape.C or AmericanShape.MC                                       => Fin.Succ(UShape),
        AmericanShape.L                                                           => Fin.Succ(LShape),
        AmericanShape.DoubleL                                                     => Fin.Succ(DoubleAngle),
        AmericanShape.HSS                                                         => Fin.Succ(HssRect),
        AmericanShape.Pipe                                                        => Fin.Succ(HssRound),
        AmericanShape.WT or AmericanShape.MT or AmericanShape.ST                  => Fin.Succ(Tee),
        _ => Fin.Fail<SteelClass>(new KernelFault.InvalidValue(nameof(shape), "a declared American steel shape", Some(key))),
    };

    public static Fin<SteelClass> OfShape(EuropeanShape shape, Op key) => shape switch {
        EuropeanShape.IPEAA or EuropeanShape.IPEA or EuropeanShape.IPE or EuropeanShape.IPEO or EuropeanShape.IPEV
            or EuropeanShape.HEAA or EuropeanShape.HEA or EuropeanShape.HEB or EuropeanShape.HEC or EuropeanShape.HEM
            or EuropeanShape.HE or EuropeanShape.HL or EuropeanShape.HLZ or EuropeanShape.HD or EuropeanShape.HP
            or EuropeanShape.UBP or EuropeanShape.UB or EuropeanShape.UC or EuropeanShape.IPN or EuropeanShape.J => Fin.Succ(IShape),
        EuropeanShape.UPE or EuropeanShape.PFC or EuropeanShape.UPN or EuropeanShape.U or EuropeanShape.CH        => Fin.Succ(UShape),
        _ => Fin.Fail<SteelClass>(new KernelFault.InvalidValue(nameof(shape), "a declared European steel shape", Some(key))),
    };
}

[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new(rank: 0);
    public static readonly CompactnessClass Noncompact = new(rank: 1);
    public static readonly CompactnessClass Slender    = new(rank: 2);
    public int Rank { get; }
    public CompactnessClass Worse(CompactnessClass other) => Rank >= other.Rank ? this : other;
}

public enum StainlessForm : byte { ColdStrip = 0, HotStrip = 1, Plate = 2, Bar = 3 }

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StainlessRow(
    string EnNumber, Option<double> ColdStripMpa, Option<double> HotStripMpa, Option<double> PlateMpa, Option<double> BarMpa) {

    public Option<double> Cell(StainlessForm form) => form switch {
        StainlessForm.ColdStrip => ColdStripMpa,
        StainlessForm.HotStrip  => HotStripMpa,
        StainlessForm.Plate     => PlateMpa,
        _                       => BarMpa,
    };

    public Fin<double> ProofMpa(StainlessForm form, Op key) =>
        Cell(form).ToFin(new ComponentFault.GradeBandMissing(key, ComponentFamily.Steel, typeof(StainlessForm)));

    public static StainlessForm FormOf(SectionProfile profile) => profile switch {
        SectionProfile.Catalogued => StainlessForm.Bar,
        SectionProfile.ColdFormedC or SectionProfile.Zed or SectionProfile.Corrugated => StainlessForm.ColdStrip,
        _ => StainlessForm.Plate,
    };
}

public static class StainlessBands {
    public static readonly StainlessRow S14301 = new("1.4301", Some(230.0), Some(210.0), Some(210.0), None);
    public static readonly StainlessRow S14307 = new("1.4307", Some(220.0), Some(200.0), Some(200.0), Some(175.0));
    public static readonly StainlessRow S14401 = new("1.4401", Some(240.0), Some(220.0), Some(220.0), None);
    public static readonly StainlessRow S14404 = new("1.4404", Some(240.0), None,        None,        Some(200.0));
    public static readonly StainlessRow S14462 = new("1.4462", None,        None,        None,        Some(450.0));
    public static readonly StainlessRow S14571 = new("1.4571", None,        None,        None,        None);
}

public partial record GradeProperties {
    public sealed partial record Steel {
        public Fin<double> DesignYieldMpa(SectionProfile origin, double bandThicknessMm, NationalAnnex annex, Op key) =>
            Stainless.Match(
                Some: row => row.ProofMpa(StainlessRow.FormOf(origin), key),
                None: () => EnDesignation.Match(
                    Some: designation => EnSteelMaterial.TryCreateFromDesignition(designation, annex, out EnSteelMaterial material)
                        ? key.Catch(
                            () => Fin.Succ(EnSteelFactory.CreateLinearElastic(material, Length.FromMillimeters(bandThicknessMm)).Strength.Megapascals),
                            cause => EnGrade.GradeRefusal(key, cause))
                        : Fin.Fail<double>(new ComponentFault.GradeBandMissing(key, ComponentFamily.Steel, typeof(EnSteelMaterial))),
                    None: () => Fin.Succ(NominalYieldMpa)));
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Steel> SteelArm => Columns is GradeProperties.Steel arm ? Some(arm) : None;
}

public readonly record struct SectionDims(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double BackToBackMm);

public sealed record SteelShape(
    string Label, SteelClass Class, IProfile Profile, SectionDims Section,
    MaterialGrade Grade, ComponentStandard Standard, string Catalogue,
    Option<CompositeDetail> Composite = default) {

    public static Fin<SteelShape> Of(ICatalogue catalogue, MaterialGrade grade, ComponentStandard standard, Op key) =>
        from outline in Outline(catalogue, key)
        from cls in ClassOf(catalogue, key)
        from dims in DimsOf(catalogue, key)
        select new SteelShape(catalogue.Label, cls, outline, dims, grade, standard, $"{catalogue.Catalogue}");

    static Fin<IProfile> Outline(ICatalogue catalogue, Op key) =>
        catalogue is IProfile profile
            ? Fin.Succ(profile)
            : new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, catalogue.GetType());

    static Fin<SteelClass> ClassOf(ICatalogue catalogue, Op key) => catalogue switch {
        ICircularHollow                                           => Fin.Succ(SteelClass.HssRound),
        ICircle when catalogue is not IHollowStructuralSection    => Fin.Succ(SteelClass.SolidRound),
        IRectangle when catalogue is not IHollowStructuralSection => Fin.Succ(SteelClass.SolidBar),
        IRoundedRectangularHollow                                 => Fin.Succ(SteelClass.HssRect),
        IRectangularHollow                                        => Fin.Succ(SteelClass.HssRect),
        IAmericanCatalogue a                                      => SteelClass.OfShape(a.Shape, key),
        IEuropeanCatalogue e                                      => SteelClass.OfShape(e.Shape, key),
        _ => Fin.Fail<SteelClass>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, catalogue.GetType())),
    };

    static Fin<SectionDims> DimsOf(ICatalogue catalogue, Op key) =>
        (catalogue switch {
            IIParallelFlange i => Fin.Succ((i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, i.FilletRadius.Millimeters, 0.0)),
            II i               => Fin.Succ((i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, 0.0, 0.0)),
            IDoubleAngle da    => Fin.Succ((da.Height.Millimeters, da.Width.Millimeters, da.WebThickness.Millimeters, da.FlangeThickness.Millimeters, 0.0, da.BackToBackDistance.Millimeters)),
            IChannel c         => Fin.Succ((c.Height.Millimeters, c.Width.Millimeters, c.WebThickness.Millimeters, c.FlangeThickness.Millimeters, 0.0, 0.0)),
            ITee t             => Fin.Succ((t.Height.Millimeters, t.Width.Millimeters, t.WebThickness.Millimeters, t.FlangeThickness.Millimeters, 0.0, 0.0)),
            IAngle an          => Fin.Succ((an.Height.Millimeters, an.Width.Millimeters, an.WebThickness.Millimeters, an.FlangeThickness.Millimeters, 0.0, 0.0)),
            ICircularHollow ch when catalogue is IHollowStructuralSection h           => Fin.Succ((ch.Diameter.Millimeters, ch.Diameter.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0, 0.0)),
            IRoundedRectangularHollow rr when catalogue is IHollowStructuralSection h => Fin.Succ((rr.Height.Millimeters, rr.Width.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, (rr.Width.Millimeters - rr.FlatWidth.Millimeters) / 2.0, 0.0)),
            IRectangularHollow rh when catalogue is IHollowStructuralSection h        => Fin.Succ((rh.Height.Millimeters, rh.Width.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0, 0.0)),
            ICircle c          => Fin.Succ((c.Diameter.Millimeters, c.Diameter.Millimeters, c.Diameter.Millimeters, c.Diameter.Millimeters, 0.0, 0.0)),
            IRectangle rect    => Fin.Succ((rect.Height.Millimeters, rect.Width.Millimeters, rect.Width.Millimeters, rect.Height.Millimeters, 0.0, 0.0)),
            _ => Fin.Fail<(double, double, double, double, double, double)>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, catalogue.GetType())),
        })
        .Bind(raw =>
            from depth in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item1)
            from width in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item2)
            from web in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item3)
            from flange in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item4)
            select new SectionDims(depth, width, web, flange, raw.Item5, raw.Item6));
}

public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    StudGroup Group,
    int StudsPerMetre,
    PositiveMagnitude ShearSpanMm);

public readonly record struct DesignCapacity(
    DesignBasis Basis, double FlexuralNmm, double FlexuralMinorNmm, double CompressionN, double ShearN,
    double TorsionalNmm, CompactnessClass Classification, double Slenderness, double Chi, double ChiLt);

public readonly record struct SteelFireFacts(double SectionFactorPerM, double Ky, double KE, double CriticalTemperatureC);

// --- [OPERATIONS] ----------------------------------------------------------------------
// --- [CLASSIFICATION]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelJurisdiction {
    public static readonly SteelJurisdiction Aisc360  = new("aisc360",   ladder: Aisc,     epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction AisiS100 = new("aisi-s100", ladder: Computed, epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction En1993   = new("en1993",    ladder: Eurocode, epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction En1994   = new("en1994",    ladder: Eurocode, epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction En1993Stainless = new("en1993-1-4", ladder: Eurocode, epsilonModulusRatio: 200_000.0 / 210_000.0);

    public double EpsilonModulusRatio { get; }

    [UseDelegateFromConstructor] private partial CompactnessClass Run(SteelClass cls, SectionDims dims, double yieldMpa, double modulusRatio);
    public CompactnessClass Classify(SteelClass cls, SectionDims dims, double yieldMpa) => Run(cls, dims, yieldMpa, EpsilonModulusRatio);

    public static Fin<SteelJurisdiction> Of(DesignBasis basis, Op key) =>
        TryGet(basis.Key, out SteelJurisdiction? row) && row is { } served
            ? Fin.Succ(served)
            : new ComponentFault.BasisUnsupported(key, basis, ComponentFamily.Steel);

    static CompactnessClass Computed(SteelClass cls, SectionDims dims, double yieldMpa, double modulusRatio) => CompactnessClass.Compact;

    static CompactnessClass Aisc(SteelClass cls, SectionDims d, double yieldMpa, double modulusRatio) => cls.Slenderness.Match(
        Some: row => {
            double r = cls == SteelClass.HssRound ? SteelDesign.E / yieldMpa : Math.Sqrt(SteelDesign.E / yieldMpa);
            (double flange, double web) = Ratios(row, d);
            return Verdict(flange, row.FlangeLambdaP * r, row.FlangeLambdaR * r).Worse(Verdict(web, row.WebLambdaP * r, row.WebLambdaR * r));
        },
        None: static () => CompactnessClass.Compact);

    static CompactnessClass Eurocode(SteelClass cls, SectionDims d, double yieldMpa, double modulusRatio) => cls.Slenderness.Match(
        Some: row => {
            double e = Math.Sqrt(235.0 / yieldMpa * modulusRatio);
            (double flange, double web) = Ratios(row, d);
            return Verdict(flange, EnFlangeCompact * e, EnFlangeSemiCompact * e).Worse(Verdict(web, EnWebCompact * e, EnWebSemiCompact * e));
        },
        None: static () => CompactnessClass.Compact);

    static (double Flange, double Web) Ratios(SlendernessRow row, SectionDims d) => (
        d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value),
        (row.WebClear ? Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, 0.0) : d.DepthMm.Value) / d.WebMm.Value);

    static CompactnessClass Verdict(double ratio, double lambdaP, double lambdaR) =>
        ratio > lambdaR ? CompactnessClass.Slender : ratio <= lambdaP ? CompactnessClass.Compact : CompactnessClass.Noncompact;

    const double EnWebCompact = 72.0, EnWebSemiCompact = 124.0, EnFlangeCompact = 9.0, EnFlangeSemiCompact = 14.0;
}

// --- [DESIGN]
public static class SteelDesign {
    const double PhiB = 0.90, PhiC = 0.90, PhiV = 0.90;
    internal const double E = 200_000.0, G = 77_200.0;

    public static Fin<DesignCapacity> Capacity(SectionProfile profile, MaterialGrade grade, ComputedSection s, CapacityPlacement placement, Op key) =>
        from lengths in guard(
            double.IsFinite(placement.UnbracedLengthMm + placement.EffectiveLengthMm)
                && placement.UnbracedLengthMm >= 0.0 && placement.EffectiveLengthMm > 0.0,
            new KernelFault.InvalidValue(nameof(placement), "finite non-negative unbraced length and finite positive effective length", Some(key)))
        from jurisdiction in SteelJurisdiction.Of(placement.Basis, key)
        from arm in grade.SteelArm.ToFin(new ComponentFault.GradeBodyMissing(key, grade, ComponentFamily.Steel))
        from form in FormOf(profile, key)
        from yieldMpa in arm.DesignYieldMpa(profile, form.Class.BandThicknessMm(form.Dims), placement.Annex, key)
        let classification = jurisdiction.Classify(form.Class, form.Dims, yieldMpa)
        select form.Class == SteelClass.ColdFormed
            ? FormedSection(placement.Basis, form.Dims, s, yieldMpa, placement.EffectiveLengthMm)
            : placement.Basis.Body == ComponentAuthority.En
                ? Eurocode(placement.Basis, form.Class, form.Dims, form.Composite, s, yieldMpa, classification, placement.UnbracedLengthMm, placement.EffectiveLengthMm)
                : Rolled(placement.Basis, form.Class, form.Dims, form.Composite, s, yieldMpa, classification, placement.UnbracedLengthMm, placement.EffectiveLengthMm);

    static (double Slenderness, double Fcr) Column(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double lambdaC = effectiveLengthMm / s.GoverningRadiusMm;
        double fe = Math.PI * Math.PI * E / (lambdaC * lambdaC);
        return (lambdaC, fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / fe) : 0.877 * fe);
    }

    static double Resist(DesignBasis basis, double nominal, double phi, double gamma) =>
        basis.Format == SafetyFormat.LimitState ? nominal / gamma : phi * nominal;

    static Fin<(SteelClass Class, SectionDims Dims, Option<CompositeDetail> Composite)> FormOf(SectionProfile profile, Op key) => profile switch {
        SectionProfile.Catalogued c        => Fin.Succ((c.Shape.Class, c.Shape.Section, c.Shape.Composite)),
        SectionProfile.IShape i            => Dims(SteelClass.IShape, i.DepthMm, i.WidthMm, i.WebMm, i.FlangeMm, i.FilletMm),
        SectionProfile.AsymmetricIShape a  => Dims(SteelClass.IShape, a.DepthMm, Wider(a.TopFlangeWidthMm, a.BottomFlangeWidthMm), a.WebThicknessMm, Thinner(a.TopFlangeThicknessMm, a.BottomFlangeThicknessMm), a.FilletMm),
        SectionProfile.Channel c           => Dims(SteelClass.UShape, c.DepthMm, c.WidthMm, c.WebMm, c.FlangeMm, c.FilletMm),
        SectionProfile.Tee t               => Dims(SteelClass.Tee, t.DepthMm, t.WidthMm, t.WebMm, t.FlangeMm, t.FilletMm),
        SectionProfile.Angle an            => Dims(SteelClass.LShape, an.DepthMm, an.WidthMm, an.ThicknessMm, an.ThicknessMm, an.FilletMm),
        SectionProfile.Zed z               => Dims(SteelClass.ColdFormed, z.DepthMm, Wider(z.TopFlangeWidthMm, z.BottomFlangeWidthMm), z.WallMm, z.WallMm, z.InnerFilletMm),
        SectionProfile.ColdFormedC cf      => Dims(SteelClass.ColdFormed, cf.DepthMm, cf.WidthMm, cf.WallMm, cf.WallMm, cf.InnerFilletMm),
        SectionProfile.Corrugated deck     => Dims(SteelClass.ColdFormed, deck.RibDepthMm, deck.TopFlatMm, deck.GaugeMm, deck.GaugeMm, 0.0),
        SectionProfile.RectangleHollow rh  => Dims(SteelClass.HssRect, rh.DepthMm, rh.WidthMm, rh.WallMm, rh.WallMm, rh.InnerFilletMm),
        SectionProfile.CircleHollow ch     => Dims(SteelClass.HssRound, ch.DiameterMm, ch.DiameterMm, ch.WallMm, ch.WallMm, 0.0),
        SectionProfile.RoundedRectangle rr => Dims(SteelClass.HssRect, rr.DepthMm, rr.WidthMm, rr.WidthMm, rr.DepthMm, rr.RoundingMm),
        SectionProfile.BuiltUp b           => Dims(SteelClass.IShape, b.GrossRectangleMm.DepthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.DepthMm, 0.0),
        _ => Fin.Fail<(SteelClass, SectionDims, Option<CompositeDetail>)>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, profile.GetType())),
    };

    static Fin<(SteelClass Class, SectionDims Dims, Option<CompositeDetail> Composite)> Dims(SteelClass cls, PositiveMagnitude depth, PositiveMagnitude width, PositiveMagnitude web, PositiveMagnitude flange, double fillet) =>
        Fin.Succ((cls, new SectionDims(depth, width, web, flange, fillet, 0.0), Option<CompositeDetail>.None));

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
    static PositiveMagnitude Thinner(PositiveMagnitude a, PositiveMagnitude b) => a.Value <= b.Value ? a : b;

    static DesignCapacity Rolled(DesignBasis basis, SteelClass cls, SectionDims d, Option<CompositeDetail> composite, ComputedSection s, double yieldMpa, CompactnessClass classification, double unbracedLengthMm, double effectiveLengthMm) {
        (double lambdaC, double fcr) = Column(s, yieldMpa, effectiveLengthMm);
        double effective = classification == CompactnessClass.Slender ? EffectiveAreaRatio(d, fcr) : 1.0;
        double mp = yieldMpa * s.ZxMm3.Value;
        double rolledMn = cls.Regime.Switch(
            state: (Class: cls, Dims: d, Section: s, Lb: unbracedLengthMm, Fy: yieldMpa, Mp: mp),
            f2:      static x => Math.Min(LateralTorsionalMn(x.Dims, x.Section, x.Lb, x.Fy, x.Mp), FlangeLocalMn(x.Class, x.Dims, x.Section, x.Fy, x.Mp)),
            f9:      static x => TeeMn(x.Section, x.Lb, x.Fy, x.Mp),
            f10:     static x => AngleMn(x.Dims, x.Section, x.Lb, x.Fy),
            plastic: static x => x.Mp);
        double mn = composite.Match(Some: c => Math.Max(CompositeMn(c, s, yieldMpa), rolledMn), None: () => rolledMn);
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: PhiB * mn,
            FlexuralMinorNmm: PhiB * MinorMn(cls, d, s, yieldMpa),
            CompressionN: PhiC * fcr * s.AreaMm2.Value * effective,
            ShearN: PhiV * 0.6 * yieldMpa * s.AvyMm2.Value,
            TorsionalNmm: TorsionalResistance(cls, s, yieldMpa),
            Classification: classification,
            Slenderness: lambdaC,
            Chi: 1.0,
            ChiLt: 1.0);
    }

    static DesignCapacity Eurocode(DesignBasis basis, SteelClass cls, SectionDims d, Option<CompositeDetail> composite, ComputedSection s, double yieldMpa, CompactnessClass classification, double unbracedLengthMm, double effectiveLengthMm) {
        double effective = classification == CompactnessClass.Slender ? EffectiveAreaRatio(d, yieldMpa) : 1.0;
        double wy = (classification == CompactnessClass.Compact ? s.ZxMm3.Value : s.SxMm3.Value) * effective;
        double wz = (classification == CompactnessClass.Compact ? s.ZyMm3.Value : s.SyMm3.Value) * effective;
        double lambdaBar = EnSlenderness(s, yieldMpa, effectiveLengthMm);
        double chi = EnChi(lambdaBar, cls.BucklingAlpha);
        double chiLt = cls.LtbAlpha.Match(
            Some: alpha => EnChi(EnLtbSlenderness(s, wy, yieldMpa, unbracedLengthMm), alpha),
            None: static () => 1.0);
        double mRd = chiLt * wy * yieldMpa / basis.GammaM1;
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: composite.Match(Some: c => Math.Max(CompositeMn(c, s, yieldMpa) / basis.GammaM0, mRd), None: () => mRd),
            FlexuralMinorNmm: wz * yieldMpa / basis.GammaM0,
            CompressionN: chi * s.AreaMm2.Value * effective * yieldMpa / basis.GammaM1,
            ShearN: s.AvyMm2.Value * yieldMpa / (Math.Sqrt(3.0) * basis.GammaM0),
            TorsionalNmm: cls.Topology == SteelTopology.Closed
                ? s.JMm4.Value / Math.Max(0.5 * d.DepthMm.Value, EpsilonPolicy.ZeroTolerance) * yieldMpa / (Math.Sqrt(3.0) * basis.GammaM0)
                : 0.0,
            Classification: classification,
            Slenderness: lambdaBar,
            Chi: chi,
            ChiLt: chiLt);
    }

    static double EnSlenderness(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double ncr = Math.PI * Math.PI * E * s.AreaMm2.Value * s.GoverningRadiusMm * s.GoverningRadiusMm
            / (effectiveLengthMm * effectiveLengthMm);
        return Math.Sqrt(s.AreaMm2.Value * yieldMpa / Math.Max(ncr, EpsilonPolicy.ZeroTolerance));
    }

    static double EnLtbSlenderness(ComputedSection s, double modulusMm3, double yieldMpa, double unbracedLengthMm) {
        double mcr = Math.PI / Math.Max(unbracedLengthMm, 1.0)
            * Math.Sqrt(Math.Max(E * s.IyMm4.Value * G * s.JMm4.Value, 0.0));
        return Math.Sqrt(modulusMm3 * yieldMpa / Math.Max(mcr, EpsilonPolicy.ZeroTolerance));
    }

    static double EnChi(double lambdaBar, double alpha) {
        double phi = 0.5 * (1.0 + alpha * (lambdaBar - 0.2) + lambdaBar * lambdaBar);
        return Math.Min(1.0, 1.0 / (phi + Math.Sqrt(Math.Max(phi * phi - lambdaBar * lambdaBar, EpsilonPolicy.ZeroTolerance))));
    }

    static DesignCapacity FormedSection(DesignBasis basis, SectionDims d, ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        (double lambdaC, double fcr) = Column(s, yieldMpa, effectiveLengthMm);
        double flexuralRatio = EffectiveModulusRatio(d, yieldMpa);
        double axialRatio = EffectiveAreaRatio(d, fcr);
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: Resist(basis, yieldMpa * s.SxMm3.Value * flexuralRatio, PhiB, basis.GammaM0),
            FlexuralMinorNmm: Resist(basis, yieldMpa * s.SyMm3.Value * flexuralRatio, PhiB, basis.GammaM0),
            CompressionN: Resist(basis, fcr * s.AreaMm2.Value * axialRatio, PhiC, basis.GammaM1),
            ShearN: Resist(basis, 0.6 * yieldMpa * s.AvyMm2.Value, PhiV, basis.GammaM0),
            TorsionalNmm: 0.0,
            Classification: flexuralRatio < 1.0 ? CompactnessClass.Slender : CompactnessClass.Compact,
            Slenderness: lambdaC,
            Chi: 1.0,
            ChiLt: 1.0);
    }

    const double KStiffened = 4.0, KUnstiffened = 0.43, KWebBending = 23.9, WinterLimit = 0.673;

    static double Winter(double flatMm, double thicknessMm, double stressMpa, double k) {
        double lambda = 1.052 / Math.Sqrt(k) * (flatMm / thicknessMm) * Math.Sqrt(stressMpa / E);
        return lambda <= WinterLimit ? 1.0 : Math.Clamp((1.0 - 0.22 / lambda) / lambda, 0.0, 1.0);
    }

    static (double Web, double Flange) Flats(SectionDims d) =>
        (Math.Max(d.DepthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value),
         Math.Max(d.WidthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value));

    internal static double EffectiveModulus(ColdFormedRow row, double yieldMpa) =>
        EffectiveModulusRatio(new SectionDims(
            PositiveMagnitude.Create(row.DepthMm), PositiveMagnitude.Create(row.FlangeMm),
            PositiveMagnitude.Create(row.WallMm), PositiveMagnitude.Create(row.WallMm), row.FilletMm, 0.0), yieldMpa);

    static double EffectiveModulusRatio(SectionDims d, double yieldMpa) {
        (double web, double flange) = Flats(d);
        double t = d.WebMm.Value, half = d.DepthMm.Value * 0.5;
        double flangeLoss = (1.0 - Winter(flange, t, yieldMpa, KStiffened)) * flange * t * half;
        double webLoss = (1.0 - Winter(web, t, yieldMpa, KWebBending)) * (web * 0.5) * t * (half * 0.5);
        double gross = flange * t * half + web * 0.5 * t * (half * 0.5);
        return gross > 0.0 ? Math.Clamp(1.0 - (flangeLoss + webLoss) / gross, 0.0, 1.0) : 1.0;
    }

    static double EffectiveAreaRatio(SectionDims d, double stressMpa) {
        (double web, double flange) = Flats(d);
        double t = d.WebMm.Value;
        double effective = Winter(web, t, stressMpa, KStiffened) * web + 2.0 * Winter(flange, t, stressMpa, KUnstiffened) * flange;
        double gross = web + 2.0 * flange;
        return gross > 0.0 ? Math.Clamp(effective / gross, 0.0, 1.0) : 1.0;
    }

    static double FlangeLocalMn(SteelClass cls, SectionDims d, ComputedSection s, double fy, double mp) {
        if (cls.Slenderness.Case is not SlendernessRow row) { return mp; }
        double r = Math.Sqrt(E / fy);
        double lambda = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value), lambdaP = row.FlangeLambdaP * r, lambdaR = row.FlangeLambdaR * r;
        double kc = Math.Clamp(4.0 / Math.Sqrt(Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, d.WebMm.Value) / d.WebMm.Value), 0.35, 0.76);
        return lambda <= lambdaP ? mp
            : lambda <= lambdaR ? mp - (mp - 0.7 * fy * s.SxMm3.Value) * (lambda - lambdaP) / (lambdaR - lambdaP)
            : 0.9 * E * kc * s.SxMm3.Value / (lambda * lambda);
    }

    static double MinorMn(SteelClass cls, SectionDims d, ComputedSection s, double fy) {
        double cap = cls.Regime == FlexureRegime.F10 ? 1.5 : 1.6;
        double mpy = Math.Min(fy * s.ZyMm3.Value, cap * fy * s.SyMm3.Value);
        if (cls.Regime != FlexureRegime.F2 || cls.Slenderness.Case is not SlendernessRow row) { return mpy; }
        double r = Math.Sqrt(E / fy);
        double lambda = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value), lambdaP = row.FlangeLambdaP * r, lambdaR = row.FlangeLambdaR * r;
        return lambda <= lambdaP ? mpy
            : lambda <= lambdaR ? mpy - (mpy - 0.7 * fy * s.SyMm3.Value) * (lambda - lambdaP) / (lambdaR - lambdaP)
            : 0.69 * E * s.SyMm3.Value / (lambda * lambda);
    }

    static double TorsionalResistance(SteelClass cls, ComputedSection s, double yieldMpa) {
        double closedForm = PhiV * 0.6 * yieldMpa * s.JMm4.Value / (0.5 * s.DepthMm.Value);
        return cls.Topology.Map(open: 0.0, closed: closedForm, solid: closedForm);
    }

    static double TeeMn(ComputedSection s, double lb, double fy, double mp) {
        double cap = Math.Min(mp, 1.6 * fy * s.SxMm3.Value);
        if (lb <= 0.0) { return cap; }
        double b = 2.3 * (s.DepthMm.Value / lb) * Math.Sqrt(s.IyMm4.Value / s.JMm4.Value);
        return Math.Min(cap, Math.PI * Math.Sqrt(E * s.IyMm4.Value * G * s.JMm4.Value) / lb * (b + Math.Sqrt(1.0 + b * b)));
    }

    static double AngleMn(SectionDims d, ComputedSection s, double lb, double fy) {
        double my = fy * s.SxMm3.Value, cap = 1.5 * my;
        if (lb <= 0.0) { return cap; }
        double me = 0.46 * E * Math.Pow(s.WidthMm.Value * d.WebMm.Value, 2.0) / lb;
        return me <= my ? (0.92 - 0.17 * me / my) * me : Math.Min(cap, (1.92 - 1.17 * Math.Sqrt(my / me)) * my);
    }

    static double LateralTorsionalMn(SectionDims d, ComputedSection s, double lb, double fy, double mp) {
        double ry = s.RyMm.Value, sx = s.SxMm3.Value, iy = s.IyMm4.Value, iw = s.IwMm6, jj = s.JMm4.Value;
        double lp = 1.76 * ry * Math.Sqrt(E / fy);
        double rts = iw > 0.0 ? Math.Sqrt(Math.Sqrt(iy * iw) / sx) : ry;
        double c = 1.0, ho = Math.Max(d.DepthMm.Value - d.FlangeMm.Value, d.FlangeMm.Value);
        double term = jj * c / (sx * ho);
        double lr = 1.95 * rts * E / (0.7 * fy) * Math.Sqrt(term + Math.Sqrt(term * term + 6.76 * Math.Pow(0.7 * fy / E, 2.0)));
        return lb <= lp
            ? mp
            : lb <= lr
                ? Math.Max(0.7 * fy * sx, mp - (mp - 0.7 * fy * sx) * Math.Clamp((lb - lp) / (lr - lp), 0.0, 1.0))
                : Math.Min(mp, FcrLtb(lb, rts, jj, c, sx, ho) * sx);
    }

    static double FcrLtb(double lb, double rts, double jj, double c, double sx, double ho) {
        double slender = lb / rts;
        return Math.PI * Math.PI * E / (slender * slender) * Math.Sqrt(1.0 + 0.078 * jj * c / (sx * ho) * slender * slender);
    }

    static double CompositeMn(CompositeDetail c, ComputedSection s, double yieldMpa) {
        double tSteel = s.AreaMm2.Value * yieldMpa;
        double cConcMax = 0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value * c.SlabDepthMm.Value;
        double sumQn = c.Stud.SteelShearKn(c.Group) * 1e3 * Math.Max(0, c.StudsPerMetre) * c.ShearSpanMm.Value / 1000.0;
        double horizShear = Math.Min(Math.Min(tSteel, cConcMax), sumQn);
        double a = Math.Min(c.SlabDepthMm.Value, horizShear / (0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value));
        double leverArm = 0.5 * s.DepthMm.Value + c.SlabDepthMm.Value - 0.5 * a;
        return horizShear * leverArm;
    }

    public static Fin<SteelFireFacts> Fire(ComputedSection s, double steelTemperatureC, double utilisation, Op key) =>
        from retention in FireRetention.At(steelTemperatureC, key)
        from admitted in guard(double.IsFinite(utilisation) && utilisation is > 0.0 and <= 1.0,
            new KernelFault.OutOfRange(nameof(utilisation), utilisation, "finite and inside (0, 1]", Some(key)))
        select new SteelFireFacts(
            SectionFactorPerM: s.HeatedPerimeterMm.Value / s.AreaMm2.Value * 1000.0,
            Ky: retention.Ky,
            KE: retention.KE,
            CriticalTemperatureC: 39.19 * Math.Log(1.0 / (0.9674 * Math.Pow(Math.Max(utilisation, UtilisationValidityFloor), 3.833)) - 1.0) + 482.0);

    const double UtilisationValidityFloor = 0.013;
}

// --- [TABLES] --------------------------------------------------------------------------
public readonly record struct ColdFormedRow(int WebToken, int FlangeToken, int Mils, MaterialGrade Grade, Option<double> PublishedSeffRatio = default) {
    const double InchMm = 25.4;
    const double BendFactor = 1.5;
    const double StiffenerCoefficient = 399.0, StiffenerCapSlope = 115.0, StiffenerCapIntercept = 5.0, StiffenerOnset = 0.328;

    public string Key => $"{WebToken}s{FlangeToken}-{Mils}";
    public double DepthMm => WebToken / 100.0 * InchMm;
    public double FlangeMm => FlangeToken / 100.0 * InchMm;
    public double WallMm => Mils / 1000.0 * InchMm;
    public double FilletMm => BendFactor * WallMm;
    public double FlangeFlatMm => Math.Max(FlangeMm - 2.0 * (FilletMm + WallMm), WallMm);

    public double ComputedSeffRatio(double yieldMpa) => SteelDesign.EffectiveModulus(this, yieldMpa);

    public bool Drifts(double yieldMpa, Tolerance band) =>
        PublishedSeffRatio.Exists(published => Math.Abs(published - ComputedSeffRatio(yieldMpa)) > band.Value);

    public double LipMm(double yieldMpa) {
        double slenderness = 1.28 * Math.Sqrt(SteelDesign.E / yieldMpa);
        double ratio = FlangeFlatMm / WallMm / slenderness;
        if (ratio <= StiffenerOnset) { return 0.0; }
        double t4 = Math.Pow(WallMm, 4.0);
        double required = Math.Min(
            StiffenerCoefficient * t4 * Math.Pow(ratio - StiffenerOnset, 3.0),
            t4 * (StiffenerCapSlope * ratio + StiffenerCapIntercept));
        return Math.Cbrt(12.0 * required / WallMm);
    }
}

public static class ColdFormedSections {
    static readonly ImmutableArray<int> WebTokens = [250, 350, 362, 400, 550, 600, 800, 1000, 1200];
    static readonly ImmutableArray<int> FlangeTokens = [137, 162, 200, 250];
    static readonly ImmutableArray<int> Gauges = [33, 43, 54, 68, 97];

    public static readonly ImmutableArray<ColdFormedRow> Rows = [..
        from web in WebTokens
        from flange in FlangeTokens
        from mils in Gauges
        where flange <= web
        let row = new ColdFormedRow(web, flange, mils, MaterialGrade.A653Gr50)
        select row with { PublishedSeffRatio = PublishedSeff.TryGetValue(row.Key, out double ratio) ? Some(ratio) : Option<double>.None }];

    static readonly FrozenDictionary<string, double> PublishedSeff = new Dictionary<string, double> {
        ["250s137-54"] = 0.957, ["250s137-68"] = 0.997, ["250s137-97"] = 1.000,
        ["250s162-54"] = 0.959, ["250s162-68"] = 0.992, ["250s162-97"] = 1.000,
        ["250s200-54"] = 0.912, ["250s200-68"] = 0.970, ["250s200-97"] = 1.000,
        ["250s250-54"] = 0.814, ["250s250-68"] = 0.868, ["250s250-97"] = 0.958,
        ["350s137-54"] = 0.920, ["350s137-68"] = 0.973, ["350s137-97"] = 0.974,
        ["350s162-54"] = 0.926, ["350s162-68"] = 0.975, ["350s162-97"] = 0.979,
        ["350s200-54"] = 0.866, ["350s200-68"] = 0.957, ["350s200-97"] = 0.981,
        ["350s250-54"] = 0.773, ["350s250-68"] = 0.840, ["350s250-97"] = 0.934,
        ["362s137-54"] = 0.914, ["362s137-68"] = 0.969, ["362s137-97"] = 0.976,
        ["362s162-54"] = 0.923, ["362s162-68"] = 0.973, ["362s162-97"] = 0.980,
        ["362s200-54"] = 0.863, ["362s200-68"] = 0.954, ["362s200-97"] = 0.983,
        ["362s250-54"] = 0.769, ["362s250-68"] = 0.838, ["362s250-97"] = 0.936,
        ["400s137-54"] = 0.897, ["400s137-68"] = 0.959, ["400s137-97"] = 0.981,
        ["400s162-54"] = 0.907, ["400s162-68"] = 0.963, ["400s162-97"] = 0.985,
        ["400s200-54"] = 0.850, ["400s200-68"] = 0.945, ["400s200-97"] = 0.987,
        ["400s250-54"] = 0.762, ["400s250-68"] = 0.832, ["400s250-97"] = 0.937,
        ["550s137-54"] = 0.964, ["550s137-68"] = 0.999, ["550s137-97"] = 1.000,
        ["550s162-54"] = 0.960, ["550s162-68"] = 0.991, ["550s162-97"] = 1.000,
        ["550s200-54"] = 0.916, ["550s200-68"] = 0.963, ["550s200-97"] = 1.000,
        ["550s250-54"] = 0.836, ["550s250-68"] = 0.877, ["550s250-97"] = 0.952,
        ["600s137-54"] = 0.926, ["600s137-68"] = 0.999, ["600s137-97"] = 1.000,
        ["600s162-54"] = 0.961, ["600s162-68"] = 0.991, ["600s162-97"] = 1.000,
        ["600s200-54"] = 0.918, ["600s200-68"] = 0.963, ["600s200-97"] = 1.000,
        ["600s250-54"] = 0.840, ["600s250-68"] = 0.879, ["600s250-97"] = 0.953,
        ["800s137-54"] = 0.848, ["800s137-68"] = 0.931, ["800s137-97"] = 1.000,
        ["800s162-54"] = 0.857, ["800s162-68"] = 0.938, ["800s162-97"] = 1.000,
        ["800s200-54"] = 0.912, ["800s200-68"] = 0.965, ["800s200-97"] = 1.000,
        ["800s250-54"] = 0.817, ["800s250-68"] = 0.889, ["800s250-97"] = 0.955,
        ["1000s162-54"] = 0.790, ["1000s162-68"] = 0.874, ["1000s162-97"] = 0.963,
        ["1000s200-54"] = 0.756, ["1000s200-68"] = 0.865, ["1000s200-97"] = 0.967,
        ["1000s250-54"] = 0.741, ["1000s250-68"] = 0.879, ["1000s250-97"] = 0.958,
        ["1200s162-54"] = 0.730, ["1200s162-68"] = 0.813, ["1200s162-97"] = 0.910,
        ["1200s200-54"] = 0.704, ["1200s200-68"] = 0.810, ["1200s200-97"] = 0.919,
        ["1200s250-54"] = 0.655, ["1200s250-68"] = 0.737, ["1200s250-97"] = 0.889,
    }.ToFrozenDictionary();

    const double SeffAgreementBand = 0.02;

    public static Fin<Seq<(string Key, double Published, double Computed, bool Drifts)>> Drift(double yieldMpa, Op key) =>
        Tolerance.Of(ToleranceLane.Residual, SeffAgreementBand, key).Map(band =>
            toSeq(Rows).Bind(row => row.PublishedSeffRatio
                .Map(published => (row.Key, Published: published, Computed: row.ComputedSeffRatio(yieldMpa), Drifts: row.Drifts(yieldMpa, band)))
                .ToSeq()));
}

public readonly record struct FireRetentionRow(double TemperatureC, double Ky, double KE);

public static class FireRetention {
    public static readonly ImmutableArray<FireRetentionRow> Rows = [
        new(20.0, 1.00, 1.000), new(100.0, 1.00, 1.000), new(200.0, 1.00, 0.900), new(300.0, 1.00, 0.800),
        new(400.0, 1.00, 0.700), new(500.0, 0.78, 0.600), new(600.0, 0.47, 0.310), new(700.0, 0.23, 0.130),
        new(800.0, 0.11, 0.090), new(900.0, 0.06, 0.0675), new(1000.0, 0.04, 0.0450), new(1100.0, 0.02, 0.0225),
        new(1200.0, 0.00, 0.000)];

    static readonly IInterpolation KyCurve = Interpolate.Linear(Rows.Select(static r => r.TemperatureC), Rows.Select(static r => r.Ky));
    static readonly IInterpolation KeCurve = Interpolate.Linear(Rows.Select(static r => r.TemperatureC), Rows.Select(static r => r.KE));

    public static Fin<(double Ky, double KE)> At(double temperatureC, Op key) =>
        double.IsFinite(temperatureC)
            ? Fin.Succ(Sample(Math.Clamp(temperatureC, Rows[0].TemperatureC, Rows[^1].TemperatureC)))
            : new KernelFault.OutOfRange(nameof(temperatureC), temperatureC, "finite", Some(key));

    static (double Ky, double KE) Sample(double temperatureC) => (KyCurve.Interpolate(temperatureC), KeCurve.Interpolate(temperatureC));
}

// --- [POLICIES] ------------------------------------------------------------------------
[Union]
public abstract partial record SteelRowSource {
    private SteelRowSource() { }
    public sealed record Rolled(ICatalogue Catalogue, Option<CompositeDetail> Composite) : SteelRowSource;
    public sealed record Formed(ColdFormedRow Row) : SteelRowSource;
    public sealed record Plated(Func<Op, Fin<SectionProfile>> Build) : SteelRowSource;
}

public readonly record struct SteelRowSeed(string Designation, SteelRowSource Source, MaterialGrade Grade);

public static class SteelSeed {
    static MaterialGrade GradeOf(ICatalogue catalogue) => catalogue switch {
        IAmericanCatalogue a when a.Shape is AmericanShape.Pipe                  => MaterialGrade.A53,
        ICircularHollow                                                          => MaterialGrade.A500Round,
        IRoundedRectangularHollow or IRectangularHollow                          => MaterialGrade.A500Rect,
        IAmericanCatalogue a when a.Shape is AmericanShape.W or AmericanShape.WT => MaterialGrade.A992,
        IAmericanCatalogue                                                       => MaterialGrade.A36,
        _                                                                        => MaterialGrade.S355,
    };

    static SteelRowSeed Rolled(American id) {
        ICatalogue minted = CatalogueFactory.CreateAmerican(id);
        return new($"steel.{id.ToString().ToLowerInvariant()}", new SteelRowSource.Rolled(minted, None), GradeOf(minted));
    }

    static SteelRowSeed Rolled(European id) =>
        new($"steel.{id.ToString().ToLowerInvariant()}", new SteelRowSource.Rolled(CatalogueFactory.CreateEuropean(id), None), MaterialGrade.S355);

    static readonly Seq<SteelRowSeed> Augmented = Seq(
        new SteelRowSeed("steel.comp-w18x50-slab120",
            new SteelRowSource.Rolled(CatalogueFactory.CreateAmerican(American.W18x50),
                Some(new CompositeDetail(PositiveMagnitude.Create(1200.0), PositiveMagnitude.Create(100.0), 28.0, StudClass.S19, StudGroup.Direct, 2, PositiveMagnitude.Create(4500.0)))),
            MaterialGrade.A992),
        new SteelRowSeed("steel.pg-1200x400-500", new SteelRowSource.Plated(static key => SectionProfile.AsymmetricIShape.Of(
            depthMm: 1200.0, topFlangeWidthMm: 400.0, bottomFlangeWidthMm: 500.0,
            topFlangeThicknessMm: 25.0, bottomFlangeThicknessMm: 32.0, webThicknessMm: 12.0, filletMm: 0.0, key)), MaterialGrade.S355),
        new SteelRowSeed("steel.zed-200x75x25", new SteelRowSource.Plated(static key => SectionProfile.Zed.Of(
            depthMm: 200.0, topFlangeWidthMm: 75.0, bottomFlangeWidthMm: 65.0,
            thicknessMm: 2.5, topLipMm: 20.0, bottomLipMm: 18.0, innerFilletMm: 3.75, key)), MaterialGrade.A653Gr50),
        new SteelRowSeed("steel.pg-ss-800x300", new SteelRowSource.Plated(static key => SectionProfile.IShape.Of(
            depthMm: 800.0, widthMm: 300.0, webMm: 8.0, flangeMm: 20.0, filletMm: 0.0, flangeToeMm: 20.0, key)), MaterialGrade.Ss14301));

    static readonly Seq<SteelRowSeed> Formed =
        toSeq(ColdFormedSections.Rows).Map(static row =>
            new SteelRowSeed($"steel.cf-{row.Key}", new SteelRowSource.Formed(row), row.Grade));

    public static readonly Seq<SteelRowSeed> Roster =
        toSeq(Enum.GetValues<American>()).Map(Rolled)
            .Concat(toSeq(Enum.GetValues<European>()).Map(Rolled))
            .Concat(Formed)
            .Concat(Augmented);

    public static readonly (int American, int European, int Formed, int Augmented) Census =
        (Enum.GetValues<American>().Length, Enum.GetValues<European>().Length, Formed.Count, Augmented.Count);

    public static readonly SeedLaw<SteelRowSeed> Law = SeedLaw<SteelRowSeed>.Of(
        family: ComponentFamily.Steel,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: ProfileOf,
        substance: static row => row.Grade.Substance,
        source: Source,
        standard: Standard,
        detail: Some<Func<SteelRowSeed, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        appearance: static _ => MaterialId.Of("metal.iron"));

    static Validation<Error, Unit> Coherence(SteelRowSeed row, Op key) =>
        (guard(row.Grade.Family == ComponentFamily.Steel,
             new ComponentFault.GradeFamilyMismatch(key, row.Grade, ComponentFamily.Steel)).ToValidation(),
         guard(row.Grade.SteelArm.IsSome,
             new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Steel)).ToValidation())
            .Apply(static (_, _) => unit).As();

    static Fin<SectionProfile> ProfileOf(SteelRowSeed seed, Op key) =>
        seed.Source.Switch(
            state: (Seed: seed, Key: key),
            rolled: static (x, r) => SteelShape.Of(r.Catalogue, x.Seed.Grade, Standard(x.Seed), x.Key)
                .Map(shape => r.Composite.IsSome ? shape with { Class = SteelClass.Composite, Composite = r.Composite } : shape)
                .Map(shape => (SectionProfile)new SectionProfile.Catalogued(shape)),
            formed: static (x, f) => x.Seed.Grade.SteelArm
                .ToFin(new ComponentFault.GradeBodyMissing(x.Key, x.Seed.Grade, ComponentFamily.Steel))
                .Bind(arm => SectionProfile.ColdFormedC.Of(
                    f.Row.DepthMm, f.Row.FlangeMm, f.Row.WallMm, f.Row.LipMm(arm.NominalYieldMpa), f.Row.FilletMm, x.Key)),
            plated: static (x, p) => p.Build(x.Key));

    static ComponentStandard Standard(SteelRowSeed seed) =>
        new(seed.Grade.Authority.Region, StandardJointThicknessMm: 0.0, seed.Grade.Authority);

    static Fin<PropertyBag> Detail(SteelRowSeed seed, SectionProfile profile, Op key) =>
        Fin.Succ(ComponentDetail.RealizationRows(
            [ComponentDetail.Token(DetailSchema.ProfileSubtype, SubtypeOf(profile)),
             ComponentDetail.Sourced(Source(seed))]
            .Append(BackToBack(profile).Map(static mm => ComponentDetail.Token(PropertyCategory.Materials.Row("BackToBack"), mm.ToString("R", CultureInfo.InvariantCulture))).ToSeq())
            .ToArray()));

    static string SubtypeOf(SectionProfile profile) =>
        profile is SectionProfile.Catalogued c ? c.Shape.Class.IfcSubtype : SteelClass.ColdFormed.IfcSubtype;

    static Option<double> BackToBack(SectionProfile profile) =>
        profile is SectionProfile.Catalogued { Shape.Section.BackToBackMm: > 0.0 and var mm } ? Some(mm) : None;

    static EvidenceGrade Source(SteelRowSeed seed) => seed.Source switch {
        SteelRowSource.Rolled => EvidenceGrade.Import,
        SteelRowSource.Formed => EvidenceGrade.Defined,
        _                     => EvidenceGrade.User,
    };

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from solved in section.ToFin(new ComponentFault.SectionUnavailable(key, component.Designation))
        from grade in GradeOfComponent(component, key)
        from design in SteelDesign.Capacity(component.Profile, grade, solved, placement, key)
        from capacity in placement.FireExposure.Match(
            Some: minutes =>
                from theta in SteelFire.TemperatureC(solved, minutes, key)
                from facts in SteelDesign.Fire(solved, theta, SteelFire.DefaultUtilisation, key)
                from lifted in SectionCapacity.Lift(CapacityReceipt.Fire(component.Designation, new FireState.Steel(design, facts)), key)
                select lifted,
            None: () => SectionCapacity.Lift(new CapacityReceipt.Steel(component.Designation, design), key))
        select capacity;

    static Fin<MaterialGrade> GradeOfComponent(Component component, Op key) =>
        component.Profile is SectionProfile.Catalogued c
            ? Fin.Succ(c.Shape.Grade)
            : toSeq(MaterialGrade.Items)
                .Find(g => g.Family == ComponentFamily.Steel && g.Substance == component.SubstanceId)
                .ToFin(new ComponentFault.GradeUnavailable(key, ComponentFamily.Steel, component.SubstanceId));
}

public static class SteelFire {
    public const double DefaultUtilisation = 0.65;
    const double DensityKgM3 = 7850.0;
    const double ConvectionWM2K = 25.0;
    const double ResultantEmissivity = 0.7;
    const double StefanBoltzmann = 5.670e-8;
    const double StepSeconds = 5.0;
    const double AmbientC = 20.0;

    static double GasC(double minutes) => AmbientC + 345.0 * Math.Log10(8.0 * minutes + 1.0);

    static double SpecificHeat(double c) => c switch {
        < 600.0 => 425.0 + 0.773 * c - 1.69e-3 * c * c + 2.22e-6 * c * c * c,
        < 735.0 => 666.0 + 13002.0 / (738.0 - c),
        < 900.0 => 545.0 + 17820.0 / (c - 731.0),
        _       => 650.0,
    };

    public static Fin<double> TemperatureC(ComputedSection s, PositiveMagnitude exposureMinutes, Op key) {
        double sectionFactor = s.HeatedPerimeterMm.Value / s.AreaMm2.Value * 1000.0;
        int steps = (int)Math.Ceiling(exposureMinutes.Value * 60.0 / StepSeconds);
        double theta = toSeq(Enumerable.Range(1, steps)).Fold(AmbientC, (held, i) => {
            double gas = GasC(i * StepSeconds / 60.0);
            double net = ConvectionWM2K * (gas - held)
                + ResultantEmissivity * StefanBoltzmann * (Math.Pow(gas + 273.15, 4.0) - Math.Pow(held + 273.15, 4.0));
            return held + sectionFactor / (SpecificHeat(held) * DensityKgM3) * net * StepSeconds;
        });
        return double.IsFinite(theta) && theta >= AmbientC
            ? Fin.Succ(theta)
            : Fin.Fail<double>(new KernelFault.OutOfRange(nameof(theta), theta, "finite and at least ambient temperature", Some(key)));
    }
}
```

## [03]-[RESEARCH]

(none)
