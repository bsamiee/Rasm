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
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

// Every family page declares in the ONE Rasm.Materials.Component namespace, so the parent COMPONENT_OWNER types AND
// StudClass (joint#JOINT_FAMILY defines it here) resolve by bare name; component#COMPONENT_OWNER binds
// SteelSeed.Roster/Law/Capacity on the ComponentFamily.Steel policy row.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The supplement arm AND the flexure regime in one discriminant: OPEN carries positive warping and a web-vs-flange
// shear split, CLOSED engineering-zero warping and perimeter shear, SOLID likewise and compact by definition.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelTopology {
    public static readonly SteelTopology Open   = new("open");
    public static readonly SteelTopology Closed = new("closed");
    public static readonly SteelTopology Solid  = new("solid");
}

// The AISC 360 Chapter F flexure regime as a PER-CLASS row. Each open class names its OWN chapter because they
// disagree: §F9 caps a tee at 1.6·My where an F2 reading credits its full plastic couple, and an angle leg has no F2
// spelling at all.
[SmartEnum]
public sealed partial class FlexureRegime {
    public static readonly FlexureRegime F2      = new();
    public static readonly FlexureRegime F9      = new();
    public static readonly FlexureRegime F10     = new();
    public static readonly FlexureRegime Plastic = new();
}

// The AISC 360 Table B4.1b flexure slenderness coefficients as PER-CLASS DATA (×√(E/Fy)): FlangeDivisor 2 for the
// half-outstand rolled I/tee flange (case 10), 1 for the full channel flange, angle leg, and HSS wall (cases 10/12/17);
// WebClear deducts both flanges (case 15/19) where tee stems and angle legs read the full depth (cases 12/14). The
// HssRound row rides the case-20 D/t E/Fy reference form.
public readonly record struct SlendernessRow(double FlangeDivisor, double FlangeLambdaP, double FlangeLambdaR, bool WebClear, double WebLambdaP, double WebLambdaR);

// The IfcProfileDef subtype axis over the published family taxonomy — topology, the parameterized subtype the seam
// ProfileSet round-trips (DoubleL/Composite have no single parametric form), the Table B4.1 row, the §6.3
// imperfection factors, and the ELEMENT the grade table bands on. BandThicknessMm is a delegate column because the
// banded element is a fact of the shape class: reading a flange for a wall prices a 3 mm stud on a 20 mm band.
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
    // The FORMED class: the AISI post-buckling premise, so its own effective-width fold returns the compactness
    // verdict and the grade table bands on the sheet WALL.
    public static readonly SteelClass ColdFormed  = new("cold-formed",  topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(1, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.76, bandThicknessMm: Wall);
    // Catalogued SOLID stock carries NO slenderness row rather than a fabricated one — a solid section has no plate
    // element, which is exactly what §B4 means by calling it compact by definition, and the absence is what makes
    // SteelTopology.Solid reachable without any arm reading a limit that does not exist.
    public static readonly SteelClass SolidBar    = new("solid-bar",    topology: SteelTopology.Solid,  ifcSubtype: "IfcRectangleProfileDef",        regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);
    public static readonly SteelClass SolidRound  = new("solid-round",  topology: SteelTopology.Solid,  ifcSubtype: "IfcCircleProfileDef",           regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None, bandThicknessMm: Flange);

    public SteelTopology Topology { get; }
    public string IfcSubtype { get; }
    public FlexureRegime Regime { get; }
    public Option<SlendernessRow> Slenderness { get; }

    // The EN 1993-1-1 §6.3.1 Table 6.1/6.2 imperfection factor α (a0 0.13 · a 0.21 · b 0.34 · c 0.49 · d 0.76),
    // selected per SHAPE because the estate's column check governs on the WEAK axis: a rolled I answers curve b, a
    // hot-finished hollow curve a, the L/T/channel/welded/formed shapes curve c.
    public double BucklingAlpha { get; }

    // The §6.3.2.2 Table 6.4 lateral-torsional α_LT, ABSENT where the mode does not exist — a closed hollow section
    // has no lateral-torsional buckling, so the arm reads χ_LT = 1.0 rather than guarding a zero sentinel.
    public Option<double> LtbAlpha { get; }

    [UseDelegateFromConstructor] public partial double BandThicknessMm(SectionDims dims);
    static double Flange(SectionDims d) => d.FlangeMm.Value;
    static double Wall(SectionDims d) => d.WebMm.Value;

    // The published AISC family taxonomy IS the discriminant — TOTAL, an unrecognized family rails, never a silent
    // `_ => IShape` mis-classifying a tee/angle/hollow. HSS maps to the RECTANGULAR default; the round/rect split is
    // SteelShape.Of's GEOMETRY pre-empt, never this enum.
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

    // TOTAL over the 25 EN families: the H/I families -> i-shape, the channel families -> u-shape. EN 10365 publishes
    // NO European angle/hollow/tee family, so these two arms exhaust the 25 and the `_` arm is the defensive rail.
    public static Fin<SteelClass> OfShape(EuropeanShape shape, Op key) => shape switch {
        EuropeanShape.IPEAA or EuropeanShape.IPEA or EuropeanShape.IPE or EuropeanShape.IPEO or EuropeanShape.IPEV
            or EuropeanShape.HEAA or EuropeanShape.HEA or EuropeanShape.HEB or EuropeanShape.HEC or EuropeanShape.HEM
            or EuropeanShape.HE or EuropeanShape.HL or EuropeanShape.HLZ or EuropeanShape.HD or EuropeanShape.HP
            or EuropeanShape.UBP or EuropeanShape.UB or EuropeanShape.UC or EuropeanShape.IPN or EuropeanShape.J => Fin.Succ(IShape),
        EuropeanShape.UPE or EuropeanShape.PFC or EuropeanShape.UPN or EuropeanShape.U or EuropeanShape.CH        => Fin.Succ(UShape),
        _ => Fin.Fail<SteelClass>(new KernelFault.InvalidValue(nameof(shape), "a declared European steel shape", Some(key))),
    };
}

// The Table B4.1 verdict — a 3-state design class, never a 2-state IsCompact flag; Worse folds the independent
// flange and web verdicts to the governing class.
[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new(rank: 0);
    public static readonly CompactnessClass Noncompact = new(rank: 1);
    public static readonly CompactnessClass Slender    = new(rank: 2);
    public int Rank { get; }
    public CompactnessClass Worse(CompactnessClass other) => Rank >= other.Rank ? this : other;
}

// The EN 10088 product-form axis of a stainless proof cell: the -2 flat forms (C cold-rolled strip t ≤ 8, H
// hot-rolled strip t ≤ 13.5, P hot-rolled plate t ≤ 75) and the -3 solution-annealed bar/section d ≤ 160.
public enum StainlessForm : byte { ColdStrip = 0, HotStrip = 1, Plate = 2, Bar = 3 }

// --- [MODELS] ------------------------------------------------------------------------------
// One EN 10088-2/-3 grade's proof-stress cells (Rp0.2 min, MPa) per product form — every cell an Option because only
// what two independent sources print seeds. ProofMpa refuses an absent cell rather than borrowing a neighbouring
// form, and FormOf recovers the form from the profile ORIGIN, a product fact the arm already states.
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

    // A catalogued identity is a hot-rolled long product (the -3 bar/section table), a formed sheet is cold strip
    // (-2 C), and every fabricated arm is hot-rolled plate (-2 P).
    public static StainlessForm FormOf(SectionProfile profile) => profile switch {
        SectionProfile.Catalogued => StainlessForm.Bar,
        SectionProfile.ColdFormedC or SectionProfile.Zed or SectionProfile.Corrugated => StainlessForm.ColdStrip,
        _ => StainlessForm.Plate,
    };
}

// The standard-table print corroborated by an independent producer datasheet fills a cell; a single-sourced cell
// stays None — the 1.4404 H/P flats, the 1.4301/1.4401 bar rows, the 1.4462 flat rows, and EVERY 1.4571 cell, which
// is why 1.4571 registers with no MaterialGrade band until a second source lands one.
public static class StainlessBands {
    public static readonly StainlessRow S14301 = new("1.4301", Some(230.0), Some(210.0), Some(210.0), None);
    public static readonly StainlessRow S14307 = new("1.4307", Some(220.0), Some(200.0), Some(200.0), Some(175.0));
    public static readonly StainlessRow S14401 = new("1.4401", Some(240.0), Some(220.0), Some(220.0), None);
    public static readonly StainlessRow S14404 = new("1.4404", Some(240.0), None,        None,        Some(200.0));
    public static readonly StainlessRow S14462 = new("1.4462", None,        None,        None,        Some(450.0));
    public static readonly StainlessRow S14571 = new("1.4571", None,        None,        None,        None);
}

// The STEEL arm's physics, co-located with the family that owns it. DesignYieldMpa is the ONE yield entry: the two
// sources are mutually exclusive by roster construction, so the stainless cell and the EN Table 3.1 derivation are
// ARMS of one read rather than two members a caller could pick between and get wrong.
public partial record GradeProperties {
    public sealed partial record Steel {
        // TryCreateFromDesignition is the package's ONE non-throwing constructor, so an unparseable designation is a
        // TYPED refusal; the trap covers only the derivation, which throws on a missing annex or an invalid
        // specification. An AISC band returns its spec-nominal — no admitted package owns the AISC grade table.
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

// The steel arm read every path here takes — a total projection over the closed payload, not a dispatch shell: four
// consumers would otherwise each spell the same Switch, and the Option is what makes a non-steel grade reaching a
// steel path a typed refusal instead of an arm nobody matched.
public sealed partial class MaterialGrade {
    public Option<GradeProperties.Steel> SteelArm => Columns is GradeProperties.Steel arm ? Some(arm) : None;
}

// The admitted published-dims currency: four proven-positive dims (WidthMm/DepthMm the Catalogued gross state,
// WebMm/FlangeMm feeding the Forms closed forms and the classifier — the hollow arms carry wall thickness in both)
// beside two >=0 slots (the fillet AND rounded-HSS corner radius; the IDoubleAngle spacing the Bim round-trip reads).
public readonly record struct SectionDims(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double BackToBackMm);

// The published-identity payload of the Catalogued arm. The twenty-column ComputedSection is NOT a field — it lives
// in the catalogue section map SectionSolver.Solve fills for every Sectioned row.
public sealed record SteelShape(
    string Label, SteelClass Class, IProfile Profile, SectionDims Section,
    MaterialGrade Grade, ComponentStandard Standard, string Catalogue,
    Option<CompositeDetail> Composite = default) {

    public static Fin<SteelShape> Of(ICatalogue catalogue, MaterialGrade grade, ComponentStandard standard, Op key) =>
        from outline in Outline(catalogue, key)
        from cls in ClassOf(catalogue, key)
        from dims in DimsOf(catalogue, key)
        select new SteelShape(catalogue.Label, cls, outline, dims, grade, standard, $"{catalogue.Catalogue}");

    // The cast is RAILED: a catalogue member that does not implement the geometry floor is a package-surface fact
    // this boundary refuses, never an InvalidCastException from inside a traverse where no Op names the failing row.
    static Fin<IProfile> Outline(ICatalogue catalogue, Op key) =>
        catalogue is IProfile profile
            ? Fin.Succ(profile)
            : new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, catalogue.GetType());

    // Geometry pre-empts the family fold: a round and a rectangular HSS carry the SAME AmericanShape.HSS, and the
    // ROUNDED contract does NOT extend IRectangularHollow (verified), so BOTH rectangular arms are load-bearing; the
    // solid arms sit BELOW every hollow contract.
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

    // The family geometry read in the native published unit: IIParallelFlange precedes the II base (S/HP taper
    // flanges carry no fillet), IDoubleAngle precedes IAngle, the rounded-rect arm derives its corner radius from the
    // flat-width delta, and the solid arms match LAST. Columns are (depth, width, web, flange, fillet, backToBack).
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

// AISC 360 Chapter I composite-action detail. ΣQn sums the studs over the SHEAR SPAN (§I3.2d) at the per-stud Eq
// I8-1 cap, never a per-metre rate against a total force; the StudGroup is a PLACEMENT fact Eq I8-1 reads directly —
// Rg falls to 0.85 at two studs per rib and Rp to 0.60 weak-position, over half the couple.
public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    StudGroup Group,
    int StudsPerMetre,
    PositiveMagnitude ShearSpanMm);

// The design-resistance receipt, BASIS-TAGGED: one column set carries the φ-format and the γM-format resistances and
// Basis names which. TorsionalNmm is engineering-zero for an OPEN shape whose warping torsion is not a single
// resistance, so a torsion demand there surfaces as the governing over-ratio; Chi/ChiLt read 1.0 on the φ-format arms
// because AISC folds buckling INTO Fcr and Mn rather than publishing it beside them.
public readonly record struct DesignCapacity(
    DesignBasis Basis, double FlexuralNmm, double FlexuralMinorNmm, double CompressionN, double ShearN,
    double TorsionalNmm, CompactnessClass Classification, double Slenderness, double Chi, double ChiLt);

// The EN 1993-1-2 fire receipt: the section factor (the AISC Appendix 4 W/D analogue a Compute fire runner drives),
// the Table 3.1 retention pair, and the §4.2.4 critical temperature. ONE receipt, because a fire verdict needs all
// four and three loose statics let a caller take two.
public readonly record struct SteelFireFacts(double SectionFactorPerM, double Ky, double KE, double CriticalTemperatureC);

// --- [OPERATIONS] --------------------------------------------------------------------------
// --- [CLASSIFICATION]
// Which slenderness ladder each DESIGN BASIS runs, and under which ε basis. Keying on the basis KEY rather than its
// SafetyFormat is the DesignBasis ruling's own law — a format is a property a code HAS, not its identity — so an
// unserved (code, family) pair reports NOT-APPLICABLE instead of falling into whichever arm the format selected. The
// ladder is a DELEGATE COLUMN and Classify feeds it the row's own modulus ratio, which is what lets ONE Table 5.2
// generator serve the carbon and the stainless basis alike.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelJurisdiction {
    public static readonly SteelJurisdiction Aisc360  = new("aisc360",   ladder: Aisc,     epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction AisiS100 = new("aisi-s100", ladder: Computed, epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction En1993   = new("en1993",    ladder: Eurocode, epsilonModulusRatio: 1.0);
    public static readonly SteelJurisdiction En1994   = new("en1994",    ladder: Eurocode, epsilonModulusRatio: 1.0);
    // EN 1993-1-4: the STAINLESS jurisdiction rides the shared Eurocode ladder under its own reduced ε —
    // ε² = (235/f_y)·(E/210000) at the 200 GPa stainless design modulus, so every Table 5.2 limit tightens by the
    // modulus ratio and nothing else on the arm re-derives. Its γM set is the capacity#SECTION_CAPACITY
    // `en1993-1-4` DesignBasis row the Eurocode arm divides by.
    public static readonly SteelJurisdiction En1993Stainless = new("en1993-1-4", ladder: Eurocode, epsilonModulusRatio: 200_000.0 / 210_000.0);

    // The E/210000 term of the jurisdiction's OWN ε basis — 1.0 on the carbon rows, the modulus ratio on the
    // stainless row; a column, never a second classifier.
    public double EpsilonModulusRatio { get; }

    [UseDelegateFromConstructor] private partial CompactnessClass Run(SteelClass cls, SectionDims dims, double yieldMpa, double modulusRatio);
    public CompactnessClass Classify(SteelClass cls, SectionDims dims, double yieldMpa) => Run(cls, dims, yieldMpa, EpsilonModulusRatio);

    public static Fin<SteelJurisdiction> Of(DesignBasis basis, Op key) =>
        TryGet(basis.Key, out SteelJurisdiction? row) && row is { } served
            ? Fin.Succ(served)
            : new ComponentFault.BasisUnsupported(key, basis, ComponentFamily.Steel);

    // The COMPUTED ladder: the verdict the effective-width fold itself returns, which is what a post-buckling code
    // publishes in place of a width-to-thickness table. FormedSection overwrites it with its own measured answer.
    static CompactnessClass Computed(SteelClass cls, SectionDims dims, double yieldMpa, double modulusRatio) => CompactnessClass.Compact;

    // ONE Table B4.1 generator over the per-class row — the rolled-I model on every open class halves a channel's
    // FULL flange and reads a tee stem against case-15 web limits it does not have. The class's reference form is
    // √(E/Fy), the HssRound row the case-20 E/Fy form, and the WORSE element governs.
    static CompactnessClass Aisc(SteelClass cls, SectionDims d, double yieldMpa, double modulusRatio) => cls.Slenderness.Match(
        Some: row => {
            double r = cls == SteelClass.HssRound ? SteelDesign.E / yieldMpa : Math.Sqrt(SteelDesign.E / yieldMpa);
            (double flange, double web) = Ratios(row, d);
            return Verdict(flange, row.FlangeLambdaP * r, row.FlangeLambdaR * r).Worse(Verdict(web, row.WebLambdaP * r, row.WebLambdaR * r));
        },
        None: static () => CompactnessClass.Compact);

    // EN 1993-1-1 Table 5.2 over ε = √(235/f_y · E/210000): an INTERNAL part is Class 1-2 at c/t ≤ 72ε and Class 3
    // at ≤ 124ε, an OUTSTAND flange 9ε and 14ε. The verdict lands on the shared CompactnessClass vocabulary
    // (Class 1-2 -> Compact, 3 -> Noncompact, 4 -> Slender) so the capacity rail reads one spelling on both bases.
    static CompactnessClass Eurocode(SteelClass cls, SectionDims d, double yieldMpa, double modulusRatio) => cls.Slenderness.Match(
        Some: row => {
            double e = Math.Sqrt(235.0 / yieldMpa * modulusRatio);
            (double flange, double web) = Ratios(row, d);
            return Verdict(flange, EnFlangeCompact * e, EnFlangeSemiCompact * e).Worse(Verdict(web, EnWebCompact * e, EnWebSemiCompact * e));
        },
        None: static () => CompactnessClass.Compact);

    // The two width-to-thickness ratios BOTH ladders measure — one geometry read, so a divisor or a web-clear
    // deduction can never disagree between the codes that share the class row.
    static (double Flange, double Web) Ratios(SlendernessRow row, SectionDims d) => (
        d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value),
        (row.WebClear ? Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, 0.0) : d.DepthMm.Value) / d.WebMm.Value);

    static CompactnessClass Verdict(double ratio, double lambdaP, double lambdaR) =>
        ratio > lambdaR ? CompactnessClass.Slender : ratio <= lambdaP ? CompactnessClass.Compact : CompactnessClass.Noncompact;

    const double EnWebCompact = 72.0, EnWebSemiCompact = 124.0, EnFlangeCompact = 9.0, EnFlangeSemiCompact = 14.0;
}

// --- [DESIGN]
// The AISC 360 + AISI S100 + EN 1993 projections over the CANONICAL ComputedSection the resolution cache supplies
// (the solve ran once at catalogue build): classification reads the shape's admitted dims, capacity reads the
// receipt's real Iw (LTB), GoverningRadiusMm (weak-axis buckling), Avy (major-axis web shear), J/c (§H3.1 C).
public static class SteelDesign {
    const double PhiB = 0.90, PhiC = 0.90, PhiV = 0.90;
    // The elastic constants are the family's ONE spelling — internal because the generated ColdFormedRow lattice
    // reads E for its §B4.2 stiffener slenderness and the jurisdiction ladder reads it for both reference forms. The
    // 200 GPa value is ALSO the EN 1993-1-4 stainless design modulus, so the stainless bands ride the same constant
    // and the carbon 210 GPa reference lives only inside the jurisdiction's own ratio column.
    internal const double E = 200_000.0, G = 77_200.0;

    // THE ONE ENTRY, discriminating on the SectionProfile arm and the grade row — discriminating on the SOURCE
    // payload would leave every FABRICATED arm with no design projection at all. TOPOLOGY picks the effective-SECTION
    // step and the BASIS BODY picks the resistance arm, two INDEPENDENT axes: a formed sheet is post-buckling
    // effective under every code that covers it, so routing it by basis skipped effective width on a partial-factor
    // project entirely.
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

    // The §E3 flexural-buckling column both design lanes drive: the effective slenderness over the receipt's own
    // WEAK-axis governing radius and the 0.658/0.877 critical stress it selects.
    static (double Slenderness, double Fcr) Column(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double lambdaC = effectiveLengthMm / s.GoverningRadiusMm;
        double fe = Math.PI * Math.PI * E / (lambdaC * lambdaC);
        return (lambdaC, fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / fe) : 0.877 * fe);
    }

    // The ONE safety-format application: a resistance-factor code MULTIPLIES a nominal by φ, a partial-factor code
    // DIVIDES a characteristic by γM. Every lane states its nominal and hands it here.
    static double Resist(DesignBasis basis, double nominal, double phi, double gamma) =>
        basis.Format == SafetyFormat.LimitState ? nominal / gamma : phi * nominal;

    // The ONE profile -> (class, dims) lowering every design arm reads. A Catalogued profile hands back the state
    // SteelShape.Of already admitted; each FABRICATED arm maps its OWN named dimensions onto the same six columns
    // under the SteelClass whose Table B4.1 row governs it. An arm the steel family does not admit rails rather than
    // defaulting to an I-shape model.
    static Fin<(SteelClass Class, SectionDims Dims, Option<CompositeDetail> Composite)> FormOf(SectionProfile profile, Op key) => profile switch {
        SectionProfile.Catalogued c        => Fin.Succ((c.Shape.Class, c.Shape.Section, c.Shape.Composite)),
        SectionProfile.IShape i            => Dims(SteelClass.IShape, i.DepthMm, i.WidthMm, i.WebMm, i.FlangeMm, i.FilletMm),
        SectionProfile.AsymmetricIShape a  => Dims(SteelClass.IShape, a.DepthMm, Wider(a.TopFlangeWidthMm, a.BottomFlangeWidthMm), a.WebThicknessMm, Thinner(a.TopFlangeThicknessMm, a.BottomFlangeThicknessMm), a.FilletMm),
        SectionProfile.Channel c           => Dims(SteelClass.UShape, c.DepthMm, c.WidthMm, c.WebMm, c.FlangeMm, c.FilletMm),
        SectionProfile.Tee t               => Dims(SteelClass.Tee, t.DepthMm, t.WidthMm, t.WebMm, t.FlangeMm, t.FilletMm),
        SectionProfile.Angle an            => Dims(SteelClass.LShape, an.DepthMm, an.WidthMm, an.ThicknessMm, an.ThicknessMm, an.FilletMm),
        SectionProfile.Zed z               => Dims(SteelClass.ColdFormed, z.DepthMm, Wider(z.TopFlangeWidthMm, z.BottomFlangeWidthMm), z.WallMm, z.WallMm, z.InnerFilletMm),
        SectionProfile.ColdFormedC cf      => Dims(SteelClass.ColdFormed, cf.DepthMm, cf.WidthMm, cf.WallMm, cf.WallMm, cf.InnerFilletMm),
        // A deck's §B2 flange is its TOP FLAT — the plate element that actually buckles between folds. The rib PITCH
        // spans flats and inclined webs alike, so feeding it to the flange column measures effective width over a
        // dimension no plate has.
        SectionProfile.Corrugated deck     => Dims(SteelClass.ColdFormed, deck.RibDepthMm, deck.TopFlatMm, deck.GaugeMm, deck.GaugeMm, 0.0),
        SectionProfile.RectangleHollow rh  => Dims(SteelClass.HssRect, rh.DepthMm, rh.WidthMm, rh.WallMm, rh.WallMm, rh.InnerFilletMm),
        SectionProfile.CircleHollow ch     => Dims(SteelClass.HssRound, ch.DiameterMm, ch.DiameterMm, ch.WallMm, ch.WallMm, 0.0),
        SectionProfile.RoundedRectangle rr => Dims(SteelClass.HssRect, rr.DepthMm, rr.WidthMm, rr.WidthMm, rr.DepthMm, rr.RoundingMm),
        // A positioned built-up composition is a FABRICATED member, not a composite one: SteelClass.Composite names
        // the Chapter I steel-and-concrete regime and carries its slab-couple flexure row, so classing a battened
        // column there advertises a concrete couple no such member has.
        SectionProfile.BuiltUp b           => Dims(SteelClass.IShape, b.GrossRectangleMm.DepthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.DepthMm, 0.0),
        _ => Fin.Fail<(SteelClass, SectionDims, Option<CompositeDetail>)>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Steel, profile.GetType())),
    };

    static Fin<(SteelClass Class, SectionDims Dims, Option<CompositeDetail> Composite)> Dims(SteelClass cls, PositiveMagnitude depth, PositiveMagnitude width, PositiveMagnitude web, PositiveMagnitude flange, double fillet) =>
        Fin.Succ((cls, new SectionDims(depth, width, web, flange, fillet, 0.0), Option<CompositeDetail>.None));

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
    static PositiveMagnitude Thinner(PositiveMagnitude a, PositiveMagnitude b) => a.Value <= b.Value ? a : b;

    // The φ-format projection over the ADMITTED yield — the registered Table 3.1 value the grade arm resolved at the
    // section's own band thickness, never a caller double. A composite augmentation rides the resolved Catalogued
    // shape, so the couple enters through the SAME profile the class came from.
    static DesignCapacity Rolled(DesignBasis basis, SteelClass cls, SectionDims d, Option<CompositeDetail> composite, ComputedSection s, double yieldMpa, CompactnessClass classification, double unbracedLengthMm, double effectiveLengthMm) {
        (double lambdaC, double fcr) = Column(s, yieldMpa, effectiveLengthMm);
        // §E7: a SLENDER-element section buckles on its EFFECTIVE area, never its gross. The reduction is the same
        // Winter post-buckling fold the formed lane runs — §E7 and AISI §B2 are one physics under two names —
        // evaluated at the critical stress the column actually reached.
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

    // THE EN 1993-1-1 ARM — the partial-factor twin of Rolled, and why an EN-seeded IPE at S355 receives an EC3
    // verdict rather than an AISC one. §6.3.1 reduces by χ over the class's own Table 6.1 α, §6.3.2 by χ_LT over the
    // warping-free M_cr (a CLOSED section has no such mode, so its absent α reads 1.0), §6.2.6 shear on the von-Mises
    // √3 the code states rather than the AISC 0.6 — cross-section resistances at γM0, buckling at γM1, exactly as
    // §6.1 partitions them. The §5.5.2 Class-4 outcome drives the ELASTIC modulus, mirroring the F3 bound opposite.
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

    // §6.3.1.3 λ̄ = √(A·f_y/N_cr) over the receipt's WEAK-axis governing radius — the same axis the φ-format Column
    // body buckles about, so a basis swap never changes WHICH axis governs, only how the reduction is spelled. The
    // divisor floors at EpsilonPolicy.ZeroTolerance: double.Epsilon is a denormal, so dividing by it answers infinity
    // and the slenderness reads zero for a degenerate section instead of refusing to be finite.
    static double EnSlenderness(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double ncr = Math.PI * Math.PI * E * s.AreaMm2.Value * s.GoverningRadiusMm * s.GoverningRadiusMm
            / (effectiveLengthMm * effectiveLengthMm);
        return Math.Sqrt(s.AreaMm2.Value * yieldMpa / Math.Max(ncr, EpsilonPolicy.ZeroTolerance));
    }

    // §6.3.2.2 λ̄_LT = √(W_y·f_y/M_cr) over the warping-free M_cr = (π/L)·√(E·Iz·G·It) the receipt's real IyMm4 and
    // JMm4 supply — the same columns the F2 LTB body reads, so neither basis re-mints a section property.
    static double EnLtbSlenderness(ComputedSection s, double modulusMm3, double yieldMpa, double unbracedLengthMm) {
        double mcr = Math.PI / Math.Max(unbracedLengthMm, 1.0)
            * Math.Sqrt(Math.Max(E * s.IyMm4.Value * G * s.JMm4.Value, 0.0));
        return Math.Sqrt(modulusMm3 * yieldMpa / Math.Max(mcr, EpsilonPolicy.ZeroTolerance));
    }

    // §6.3.1.2 the ONE buckling-curve reduction both stability modes drive: Φ = 0.5(1 + α(λ̄ − 0.2) + λ̄²),
    // χ = 1/(Φ + √(Φ² − λ̄²)) capped at unity. One body, two α columns — the code publishes one formula and varies
    // only its imperfection factor. The radicand floors at the ZERO-TOLERANCE degeneracy anchor, which is a real
    // positive number the sum can absorb; double.Epsilon in that slot is a floor no finite arithmetic can reach.
    static double EnChi(double lambdaBar, double alpha) {
        double phi = 0.5 * (1.0 + alpha * (lambdaBar - 0.2) + lambdaBar * lambdaBar);
        return Math.Min(1.0, 1.0 / (phi + Math.Sqrt(Math.Max(phi * phi - lambdaBar * lambdaBar, EpsilonPolicy.ZeroTolerance))));
    }

    // The ONE AISI S100 body every formed lane drives. Effective section is COMPUTED through the §B2 Winter
    // reduction, never a per-row published Seff/S proxy: a transcribed ratio is one stress state at one thickness, so
    // it mis-prices the same profile in compression, in flexure, and at any other yield, and cannot exist at all for
    // a generated row nobody published.
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
            // The verdict is the COMPUTED reduction's own answer: a section that loses effective width at yield IS
            // slender, so the classification states what the algorithm found rather than a stored flag.
            Classification: flexuralRatio < 1.0 ? CompactnessClass.Slender : CompactnessClass.Compact,
            Slenderness: lambdaC,
            Chi: 1.0,
            ChiLt: 1.0);
    }

    // AISI S100 §B2.1 Winter effective width: λ = (1.052/√k)·(w/t)·√(f/E), ρ = 1 at λ ≤ 0.673 and (1 − 0.22/λ)/λ
    // above. k is the plate-buckling coefficient of the ELEMENT — 4.0 stiffened, 0.43 unstiffened, 23.9 for a web
    // under the pure-bending stress gradient.
    const double KStiffened = 4.0, KUnstiffened = 0.43, KWebBending = 23.9, WinterLimit = 0.673;

    static double Winter(double flatMm, double thicknessMm, double stressMpa, double k) {
        double lambda = 1.052 / Math.Sqrt(k) * (flatMm / thicknessMm) * Math.Sqrt(stressMpa / E);
        return lambda <= WinterLimit ? 1.0 : Math.Clamp((1.0 - 0.22 / lambda) / lambda, 0.0, 1.0);
    }

    // The FLAT widths a formed element actually buckles over: the overall dimension less one corner allowance
    // (inside radius plus wall) at each formed junction — the geometry the §B2 w/t reads, never the overall dim.
    static (double Web, double Flange) Flats(SectionDims d) =>
        (Math.Max(d.DepthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value),
         Math.Max(d.WidthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value));

    // Seff/S in bending: the compression flange and the compressed half-web each shed their INEFFECTIVE width at
    // their own lever from mid-depth, so the modulus ratio is one minus the first-moment loss over the gross first
    // moment. The drift census reads the SAME fold the design lane runs, over the generated row's own derived
    // dimensions — a second effective-width spelling for the census would grade one algorithm against another.
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

    // Aeff/A in uniform compression at the buckling stress: every element is uniformly compressed, so the web takes
    // the stiffened k and the reduction is the area-weighted mean of the two flats.
    static double EffectiveAreaRatio(SectionDims d, double stressMpa) {
        (double web, double flange) = Flats(d);
        double t = d.WebMm.Value;
        double effective = Winter(web, t, stressMpa, KStiffened) * web + 2.0 * Winter(flange, t, stressMpa, KUnstiffened) * flange;
        double gross = web + 2.0 * flange;
        return gross > 0.0 ? Math.Clamp(effective / gross, 0.0, 1.0) : 1.0;
    }

    // §F3 flange local buckling bounding the F2 classes: compact passes Mp through; noncompact interpolates
    // Mp -> 0.7·Fy·Sx across λpf..λrf (F3-1); slender reads the elastic 0.9·E·kc·Sx/λ² with kc = 4/√(h/tw) clamped to
    // [0.35, 0.76] (F3-2).
    static double FlangeLocalMn(SteelClass cls, SectionDims d, ComputedSection s, double fy, double mp) {
        if (cls.Slenderness.Case is not SlendernessRow row) { return mp; }
        double r = Math.Sqrt(E / fy);
        double lambda = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value), lambdaP = row.FlangeLambdaP * r, lambdaR = row.FlangeLambdaR * r;
        double kc = Math.Clamp(4.0 / Math.Sqrt(Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, d.WebMm.Value) / d.WebMm.Value), 0.35, 0.76);
        return lambda <= lambdaP ? mp
            : lambda <= lambdaR ? mp - (mp - 0.7 * fy * s.SxMm3.Value) * (lambda - lambdaP) / (lambdaR - lambdaP)
            : 0.9 * E * kc * s.SxMm3.Value / (lambda * lambda);
    }

    // §F6 weak-axis flexure: Mny = min(Fy·Zy, cap·Fy·Sy) per F6-1 (cap 1.6; 1.5 on the F10 single-angle regime per
    // F10-1 — no minor-axis LTB limit state exists). The F2 classes bound by §F6.2 flange local buckling over the SAME
    // per-class row FlangeLocalMn reads (noncompact F6-2 interpolation, slender F6-4 Fcr = 0.69·E/λ² on Sy).
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

    // §H3.1 φTn = φT·Fcr·C for the CLOSED topologies (and the solid bar's J/c modulus): Fcr = 0.6·Fy the compact-wall
    // governing limit, C = J/c with c the outer half-depth — grounded in the carried JMm4. An OPEN shape returns 0:
    // §H3.3 non-HSS torsion is a warping-normal-stress interaction, not a single resistance a ratio fold can divide.
    static double TorsionalResistance(SteelClass cls, ComputedSection s, double yieldMpa) {
        double closedForm = PhiV * 0.6 * yieldMpa * s.JMm4.Value / (0.5 * s.DepthMm.Value);
        return cls.Topology.Map(open: 0.0, closed: closedForm, solid: closedForm);
    }

    // §F9 tee / double-angle LTB: Mcr = π·√(E·Iy·G·J)/Lb·(B + √(1+B²)), B = 2.3·(d/Lb)·√(Iy/J) — the stem-in-TENSION
    // positive branch; the plastic bound is Mp ≤ 1.6·My, NOT the bare Zx couple the F2 arm credits.
    static double TeeMn(ComputedSection s, double lb, double fy, double mp) {
        double cap = Math.Min(mp, 1.6 * fy * s.SxMm3.Value);
        if (lb <= 0.0) { return cap; }
        double b = 2.3 * (s.DepthMm.Value / lb) * Math.Sqrt(s.IyMm4.Value / s.JMm4.Value);
        return Math.Min(cap, Math.PI * Math.Sqrt(E * s.IyMm4.Value * G * s.JMm4.Value) / lb * (b + Math.Sqrt(1.0 + b * b)));
    }

    // §F10 single-angle geometric-axis bending: yield cap 1.5·My (F10-1); elastic Me = 0.46·E·b²·t²/Lb (F10-5a,
    // equal-leg, Cb = 1); the (0.92 − 0.17·Me/My)·Me elastic band below My, the (1.92 − 1.17·√(My/Me))·My inelastic
    // band above, capped at 1.5·My.
    static double AngleMn(SectionDims d, ComputedSection s, double lb, double fy) {
        double my = fy * s.SxMm3.Value, cap = 1.5 * my;
        if (lb <= 0.0) { return cap; }
        double me = 0.46 * E * Math.Pow(s.WidthMm.Value * d.WebMm.Value, 2.0) / lb;
        return me <= my ? (0.92 - 0.17 * me / my) * me : Math.Min(cap, (1.92 - 1.17 * Math.Sqrt(my / me)) * my);
    }

    // §F2 LTB reading the REAL warping: Lp = 1.76·ry·√(E/Fy), rts ≈ √(√(Iy·Iw)/Sx), Lr from J/Iw, the linear
    // Mp -> 0.7·Fy·Sx interpolation between, elastic Fcr·Sx beyond Lr. ho is the FLANGE-CENTROID separation d − tf:
    // reading the full depth over-states the couple arm and credits plastic moment past the real inelastic-LTB
    // transition on every shallow-web rolled shape.
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

    // AISC 360 Eq C-I3 fully-OR-partially-composite plastic moment: As·Fy tension balanced by the 0.85·f'c·b·a block,
    // capped at the joint#JOINT_FAMILY ΣQn summed over the §I3.2d shear span. ΣQn = 0 zeroes the COUPLE — the caller
    // floors the row at the bare-steel resistance, so the beam never reads below its own section.
    static double CompositeMn(CompositeDetail c, ComputedSection s, double yieldMpa) {
        double tSteel = s.AreaMm2.Value * yieldMpa;
        double cConcMax = 0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value * c.SlabDepthMm.Value;
        double sumQn = c.Stud.SteelShearKn(c.Group) * 1e3 * Math.Max(0, c.StudsPerMetre) * c.ShearSpanMm.Value / 1000.0;
        double horizShear = Math.Min(Math.Min(tSteel, cConcMax), sumQn);
        double a = Math.Min(c.SlabDepthMm.Value, horizShear / (0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value));
        double leverArm = 0.5 * s.DepthMm.Value + c.SlabDepthMm.Value - 0.5 * a;
        return horizShear * leverArm;
    }

    // The ONE EN 1993-1-2 entry: section factor, Table 3.1 retention pair, and the §4.2.4 critical temperature
    // θ_cr = 39.19·ln(1/(0.9674·μ₀^3.833) − 1) + 482 in one receipt. μ₀ admits on the physical (0, 1] domain and the
    // standard's 0.013 lower validity bound applies only after admission. The exposure and time-temperature side
    // stays the placement-level caller input the timber exposureMinutes convention fixes.
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

// --- [TABLES] ------------------------------------------------------------------------------
// The SSMA cold-formed section as a GENERATED LATTICE over its own designation grammar: `<web>S<flange>-<mils>`
// names the web and flange in hundredths of an inch and the base metal in thousandths, so every dimension DERIVES
// from the tokens and a new stud is a token, never a transcribed row. PublishedSeffRatio is an OPTIONAL CHECK column
// — the design lane reads the COMPUTED §B2 reduction either way, and the published value GRADES the algorithm
// through Drift, never feeds it.
public readonly record struct ColdFormedRow(int WebToken, int FlangeToken, int Mils, MaterialGrade Grade, Option<double> PublishedSeffRatio = default) {
    const double InchMm = 25.4;
    const double BendFactor = 1.5;          // AISI S100 minimum inside forming radius ≈ 1.5·t
    const double StiffenerCoefficient = 399.0, StiffenerCapSlope = 115.0, StiffenerCapIntercept = 5.0, StiffenerOnset = 0.328;

    public string Key => $"{WebToken}s{FlangeToken}-{Mils}";
    public double DepthMm => WebToken / 100.0 * InchMm;
    public double FlangeMm => FlangeToken / 100.0 * InchMm;
    public double WallMm => Mils / 1000.0 * InchMm;
    public double FilletMm => BendFactor * WallMm;
    public double FlangeFlatMm => Math.Max(FlangeMm - 2.0 * (FilletMm + WallMm), WallMm);

    // The computed §B2 reduction this row would design on, exposed so the drift census can grade it against a printed
    // ratio without re-entering the design projection.
    public double ComputedSeffRatio(double yieldMpa) => SteelDesign.EffectiveModulus(this, yieldMpa);

    // The census agreement band arrives ADMITTED: it is a kernel Tolerance on the Residual lane, admitted once by the
    // census that folds this row, so a band outside [seam-ulp, 1] refuses at that boundary instead of quietly
    // widening the agreement every row is graded against.
    public bool Drifts(double yieldMpa, Tolerance band) =>
        PublishedSeffRatio.Exists(published => Math.Abs(published - ComputedSeffRatio(yieldMpa)) > band.Value);

    // AISI S100 §B4.2 minimum adequate SIMPLE lip: S = 1.28·√(E/f), an element below 0.328·S needs no stiffener, and
    // above it I_a = 399·t⁴·[(w/t)/S − 0.328]³ caps at t⁴·[115·(w/t)/S + 5]; a 90° lip has I_s = d³·t/12, so the depth
    // inverts to (12·I_a/t)^(1/3). The roster generates AT the code minimum, so a stocked row is never weaker.
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

// The web × flange × gauge cross product bounded only by the grammar's own rule that a flange token never exceeds
// its web token. A gauge-versus-depth restriction would be stocking policy this estate holds no list to state, so the
// lattice generates the whole admissible space and a stocked subset stays a caller's FILTER.
public static class ColdFormedSections {
    static readonly ImmutableArray<int> WebTokens = [250, 350, 362, 400, 550, 600, 800, 1000, 1200];
    static readonly ImmutableArray<int> FlangeTokens = [137, 162, 200, 250];
    static readonly ImmutableArray<int> Gauges = [33, 43, 54, 68, 97];   // mils base metal — the SSMA structural band

    public static readonly ImmutableArray<ColdFormedRow> Rows = [..
        from web in WebTokens
        from flange in FlangeTokens
        from mils in Gauges
        where flange <= web
        let row = new ColdFormedRow(web, flange, mils, MaterialGrade.A653Gr50)
        select row with { PublishedSeffRatio = PublishedSeff.TryGetValue(row.Key, out double ratio) ? Some(ratio) : Option<double>.None }];

    // The PUBLISHED check cells, two-source acquired: Se/Sx at Fy = 50 ksi off the AISI S100-07/-12 print lineage —
    // the SSMA Product Technical Guide corroborated by the independent lineage publishers, agreement exact to the
    // printed digit. Absent cells are the 33/43-mil manufacturer-only Fy50 prints, the designations no publisher
    // lists, the single-sourced ones, and the 2.5"-flange 33-mil rows exceeding the AISI B4.1 w/t 60 bound. The
    // S100-16/S2-20 lineage differs up to ~4% on Se, so a drift row names print-basis divergence before it names the
    // algorithm.
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

    // The census agreement band: a two-percent residual between the printed and computed effective-section ratios.
    // It is a BAND, not a length — the kernel Residual lane is the one that admits a dimensionless agreement scalar,
    // and Tolerance.Of is what refuses a band outside it.
    const double SeffAgreementBand = 0.02;

    // The DRIFT CENSUS: the band admits ONCE at this boundary and every row is graded against the admitted value, so
    // the census cannot run on an unproven scalar. The published value never feeds the design — it GRADES the
    // algorithm — so a row carrying no printed ratio contributes nothing rather than a fabricated agreement.
    public static Fin<Seq<(string Key, double Published, double Computed, bool Drifts)>> Drift(double yieldMpa, Op key) =>
        Tolerance.Of(ToleranceLane.Residual, SeffAgreementBand, key).Map(band =>
            toSeq(Rows).Bind(row => row.PublishedSeffRatio
                .Map(published => (row.Key, Published: published, Computed: row.ComputedSeffRatio(yieldMpa), Drifts: row.Drifts(yieldMpa, band)))
                .ToSeq()));
}

// EN 1993-1-2 Table 3.1 retention as PUBLISHED rows (SEED_ROW_LAW — standards data as a readonly record struct row
// table): ky,θ the effective-yield retention, kE,θ the Young's-modulus retention; steel keeps full yield to 400 °C and
// is spent at 1200 °C.
public readonly record struct FireRetentionRow(double TemperatureC, double Ky, double KE);

public static class FireRetention {
    public static readonly ImmutableArray<FireRetentionRow> Rows = [
        new(20.0, 1.00, 1.000), new(100.0, 1.00, 1.000), new(200.0, 1.00, 0.900), new(300.0, 1.00, 0.800),
        new(400.0, 1.00, 0.700), new(500.0, 0.78, 0.600), new(600.0, 0.47, 0.310), new(700.0, 0.23, 0.130),
        new(800.0, 0.11, 0.090), new(900.0, 0.06, 0.0675), new(1000.0, 0.04, 0.0450), new(1100.0, 0.02, 0.0225),
        new(1200.0, 0.00, 0.000)];

    // Two piecewise-linear splines over ONE abscissa — the table's own interpolation law, fitted once at type init
    // rather than re-walked per read. The hand fold this replaced re-scanned the roster with a TakeWhile, recovered
    // the bracketing pair by index, and re-derived the lever inline; MathNet owns that correspondence and its
    // Interpolate seam is catalogued.
    static readonly IInterpolation KyCurve = Interpolate.Linear(Rows.Select(static r => r.TemperatureC), Rows.Select(static r => r.Ky));
    static readonly IInterpolation KeCurve = Interpolate.Linear(Rows.Select(static r => r.TemperatureC), Rows.Select(static r => r.KE));

    // The CLAMP is load-bearing and stays: a linear spline extrapolates its end segments, so an unclamped read above
    // 1200 °C returns a NEGATIVE retention and one below 20 °C a retention above unity. Non-finite input rails before
    // the clamp, because clamping a NaN answers a bound rather than refusing.
    public static Fin<(double Ky, double KE)> At(double temperatureC, Op key) =>
        double.IsFinite(temperatureC)
            ? Fin.Succ(Sample(Math.Clamp(temperatureC, Rows[0].TemperatureC, Rows[^1].TemperatureC)))
            : new KernelFault.OutOfRange(nameof(temperatureC), temperatureC, "finite", Some(key));

    static (double Ky, double KE) Sample(double temperatureC) => (KyCurve.Interpolate(temperatureC), KeCurve.Interpolate(temperatureC));
}

// --- [POLICIES] ----------------------------------------------------------------------------
// The THREE legal origins of a steel profile, closed so a hybrid is unrepresentable. The FABRICATED case carries the
// row's own RAILED build delegate rather than a shape enum plus a dims bag, so fabricated dimensions are call-site
// arguments to the profile's real Of factory and a malformed row aborts the catalogue TYPED, never at type init.
[Union]
public abstract partial record SteelRowSource {
    private SteelRowSource() { }
    public sealed record Rolled(ICatalogue Catalogue, Option<CompositeDetail> Composite) : SteelRowSource;
    public sealed record Formed(ColdFormedRow Row) : SteelRowSource;
    public sealed record Plated(Func<Op, Fin<SectionProfile>> Build) : SteelRowSource;
}

// ONE seed vocabulary for every steel row — rolled American, rolled European, composite, cold-formed, fabricated — so
// the ONE ComponentSeed traverse folds the whole family; the Source union carries the per-lane payload.
public readonly record struct SteelRowSeed(string Designation, SteelRowSource Source, MaterialGrade Grade);

public static class SteelSeed {
    // The AISC grade-selection policy over the minted geometry/family: W-series and structural tees A992, HSS A500
    // Gr C (the rect/round split resolved by the SAME geometry interfaces the class split reads), Pipe A53 Gr B, the
    // remaining rolled families A36; every EN 10365 identity seeds S355. One policy value per family — never a
    // per-row grade literal.
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

    // The augmented rows ride the SAME seed vocabulary: one composite floor beam, and the FABRICATED rows that make
    // the open-thin-walled Forms algebra reachable from an admitted Component — a welded plate girder (Forms.MonoI),
    // a cold-formed Z purlin (Forms.PointSymmetricZ), and a welded STAINLESS girder whose Plated origin resolves the
    // EN 10088-2 plate cell. Fabricated dims are seed DATA; the profile arms admit through their own railed factories.
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

    // The FORMED lane is the whole generated SSMA lattice: every admissible (web, flange, gauge) triple seeds through
    // the railed ColdFormedC.Of. Both rosters FREEZE — a property re-minting the lattice on every read re-runs the
    // cross product and every catalogue-factory call once per catalogue build, per sizing sweep, and per census read.
    static readonly Seq<SteelRowSeed> Formed =
        toSeq(ColdFormedSections.Rows).Map(static row =>
            new SteelRowSeed($"steel.cf-{row.Key}", new SteelRowSource.Formed(row), row.Grade));

    // The seed domain is the ENUMERATED identity space of the admitted package — every `American` and `European`
    // member it declares — so a model importing a W40x593 dereferences it and a sizing fold scans the whole space.
    // The bound is the enum, never a claim about the published database.
    public static readonly Seq<SteelRowSeed> Roster =
        toSeq(Enum.GetValues<American>()).Map(Rolled)
            .Concat(toSeq(Enum.GetValues<European>()).Map(Rolled))
            .Concat(Formed)
            .Concat(Augmented);

    // The enumeration-bounded census: the rolled roster is exactly the two identity enums, so a package bump that
    // adds or retires a section moves this count and the guard names the drift instead of the catalogue absorbing it.
    public static readonly (int American, int European, int Formed, int Augmented) Census =
        (Enum.GetValues<American>().Length, Enum.GetValues<European>().Length, Formed.Count, Augmented.Count);

    // The seed POLICY value: this page states the roster and the law, and component#COMPONENT_SEED owns the traverse,
    // the coherence gate, the profile route, the detail fold and the railed lift.
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

    // The row census, ACCUMULATING: a roster row naming a grade from another family and a row whose grade carries no
    // steel payload are INDEPENDENT defects, so both name themselves in one verdict instead of the first hiding the
    // second. The family column and the arm are separately wrong-able — a Reinforcement row could carry a Steel arm
    // in a future roster edit, and a Steel row could be given a Concrete arm.
    static Validation<Error, Unit> Coherence(SteelRowSeed row, Op key) =>
        (guard(row.Grade.Family == ComponentFamily.Steel,
             new ComponentFault.GradeFamilyMismatch(key, row.Grade, ComponentFamily.Steel)).ToValidation(),
         guard(row.Grade.SteelArm.IsSome,
             new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Steel)).ToValidation())
            .Apply(static (_, _) => unit).As();

    // The profile route: the Source union selects the origin — a catalogue admitted once through SteelShape.Of (the
    // composite reclass riding the Rolled case), the railed parametric ColdFormedC from the published stud row at the
    // AISI code-minimum lip, or the fabricated row's own build delegate. The lip is generated at the row's NOMINAL
    // yield because geometry never depends on a national annex, so the section is one shape wherever it is checked.
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

    // The regional receipt derives from the grade's own authority row, so an AISC-bodied grade seeds a `us` standard
    // and an EN-bodied one `eu` without a per-lane ComponentStandard static.
    static ComponentStandard Standard(SteelRowSeed seed) =>
        new(seed.Grade.Authority.Region, StandardJointThicknessMm: 0.0, seed.Grade.Authority);

    // The steel REALIZATION bag. SteelClass.IfcSubtype has no other landing surface — the Bim egress profile lane
    // reads DetailSchema.ProfileSubtype off a seeded bag — and BackToBack rides beside it, a double angle's spacing
    // being a REALIZATION fact of the fabricated pair.
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

    // A row's EVIDENCE states where its VALUES came from: a catalogue identity is read from an admitted package
    // (Import), the generated stud lattice is derived by the AISI grammar's own rules (Defined), and a fabricated row
    // is this estate's own shop dimensions (User).
    static EvidenceGrade Source(SteelRowSeed seed) => seed.Source switch {
        SteelRowSource.Rolled => EvidenceGrade.Import,
        SteelRowSource.Formed => EvidenceGrade.Defined,
        _                     => EvidenceGrade.User,
    };

    // The ComponentFamily.Steel CAPACITY producer: the profile arm and the seeded grade are the only inputs
    // SteelDesign.Capacity discriminates on, and a steel row is always Sectioned, so an unresolved section is a
    // catalogue defect railed here rather than a silent absence. A placement declaring FireExposure routes the
    // SAME ambient design through the SteelFire heating step and SteelDesign.Fire onto the ONE fire mint, so the
    // ambient and fire verdicts are one dispatch, never sibling entries.
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

    // The seeded grade recovered from the row: a Catalogued profile carries it on its admitted SteelShape, and every
    // parametric arm carries it on the substance MaterialId the seed stamped — one read, no designation parse. The
    // reverse scan is bounded to the steel rows, so a substance a second family also spells cannot elect across.
    static Fin<MaterialGrade> GradeOfComponent(Component component, Op key) =>
        component.Profile is SectionProfile.Catalogued c
            ? Fin.Succ(c.Shape.Grade)
            : toSeq(MaterialGrade.Items)
                .Find(g => g.Family == ComponentFamily.Steel && g.Substance == component.SubstanceId)
                .ToFin(new ComponentFault.GradeUnavailable(key, ComponentFamily.Steel, component.SubstanceId));
}

// EN 1993-1-2 §4.2.5.1 unprotected-member heating: the ISO 834 gas curve drives the lumped-capacitance step
// dTheta = ksh*(Am/V)/(ca(theta)*rho)*hnet*dt, hnet the §3.1 convective + radiative sum. ksh rides 1.0 — the
// §4.2.5.1(2) conservative ceiling (the boxed-value shadow refinement lowers it only for I-sections and needs the
// box perimeter no ComputedSection column carries; landing that column is the declared refinement trigger). The
// fold is pure over the step count, so one exposure replays byte-identically.
public static class SteelFire {
    public const double DefaultUtilisation = 0.65;  // §4.2.4(2) NOTE: the mu0 default where no demand-side value crosses the placement
    const double DensityKgM3 = 7850.0;              // §3.2.2(1): temperature-independent by the code's own statement
    const double ConvectionWM2K = 25.0;             // §3.1(6) alpha_c under the standard temperature-time curve
    const double ResultantEmissivity = 0.7;         // §4.2.5.1(3) Phi*eps_m*eps_f
    const double StefanBoltzmann = 5.670e-8;
    const double StepSeconds = 5.0;                 // §4.2.5.1(4) dt ceiling, taken exactly
    const double AmbientC = 20.0;

    static double GasC(double minutes) => AmbientC + 345.0 * Math.Log10(8.0 * minutes + 1.0);   // §3.2.1 ISO 834

    // §3.4.1.2 specific heat — the four published branches with the 735 °C peak, J/(kg·K).
    static double SpecificHeat(double c) => c switch {
        < 600.0 => 425.0 + 0.773 * c - 1.69e-3 * c * c + 2.22e-6 * c * c * c,
        < 735.0 => 666.0 + 13002.0 / (738.0 - c),
        < 900.0 => 545.0 + 17820.0 / (c - 731.0),
        _       => 650.0,
    };

    public static Fin<double> TemperatureC(ComputedSection s, PositiveMagnitude exposureMinutes, Op key) {
        double sectionFactor = s.HeatedPerimeterMm.Value / s.AreaMm2.Value * 1000.0;   // Am/V in 1/m off the solver's own columns
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
