# [MATERIALS_STEEL]

THE STEEL SEED FAMILY GROUNDED IN THE PUBLISHED SECTION DATABASE. `SteelSeed.Rows` folds the full registered AISC American and EN 10365 European domains through `SteelShape.Of` and `Component.Of`; each row carries one published `ICatalogue` identity, one policy-selected `SteelGrade`, one `SectionProfile.Catalogued` or admitted cold-formed profile, and `Sectioned: true`. `SectionSolver.Solve` owns the twenty-column integral and open-section supplement, while `SteelDesign` owns the railed AISC/AISI capacity projection, composite augmentation, and EN 1993-1-2 fire facts over that receipt. `SteelClass` carries the profile taxonomy and IFC subtype, `IfcBinding.Supertype(ComponentFamily.Steel.Class)` keeps occurrence refinement outside Materials, and growth remains a registered catalogue member, policy row, or authored cold-formed row rather than a per-shape type.

## [01]-[INDEX]

- [02]-[STEEL_FAMILY]: the `SteelTopology` open/closed/solid axis, the `SteelClass` nine-row subtype axis with the TOTAL `OfShape(AmericanShape)`/`OfShape(EuropeanShape)` folds and the `IfcProfileDef` mapping, the `SteelGrade` registered-yield band over `EnSteelFactory`/`EnSteelDeliveryCondition`, the `SectionDims` published-dims currency, the `SteelShape.Of` catalogue admission boundary (geometry-driven hollow split + family fold + `PositiveMagnitude` lift), the `CompositeDetail` augmentation and the `ColdFormedRow`/`ColdFormedSections` published AISI stud table feeding the parametric `SectionProfile.ColdFormedC` lane, the `CompactnessClass` + `SteelDesign` AISC classification and LRFD `DesignCapacity` projection over the canonical `ComputedSection` (the AISI stud modality on the `ColdFormedRow` overload), the `FireRetention` EN 1993-1-2 Table 3.1 rows with the `SteelDesign` fire facts, and the fail-loud full-database `SteelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Steel` policy row binds (`Sectioned: true`).

## [02]-[STEEL_FAMILY]

- Owner: `SteelTopology` the open/closed/solid discriminant; `SteelClass` the `IfcProfileDef` subtype axis folded onto the published taxonomies; `SteelGrade` the registered-yield band; `SectionDims` the admitted published-dims currency; `SteelShape` the catalogued profile payload; `CompositeDetail` the composite augmentation row; `ColdFormedRow`/`ColdFormedSections` the published AISI stud table and typed resolver; `SteelRowSource` the closed profile-origin axis; `CompactnessClass`/`DesignCapacity`/`SteelDesign` the AISC + AISI projection and railed EN 1993-1-2 fire operations; `SteelSeed` the catalogue fold.
- Cases: class {i-shape (W/M/S/HP + the EN H/I families, open) · u-shape (C/MC/UPE/PFC/UPN/U/CH, open) · l-shape (L, open) · double-angle (2L, open) · hss-rect (closed) · hss-round (round HSS + Pipe, closed) · tee (WT/MT/ST, open) · composite (AISC 360 Ch I, open core) · cold-formed (AISI S100, open)} × grade {A36/A992/A572 AISC spec-nominal · S235/S275/S355/S420/S450/S460 EN Table 3.1 registered} × topology {open · closed · solid} — a section is one seed row over one published identity; the composite variant is the SAME row with a `Some CompositeDetail` and a reclassed `SteelClass` on its `Rolled` source arm, and the cold-formed stud is the SAME row on its `Formed` source arm — a parametric `ColdFormedC` profile over the published `ColdFormedRow`, never a catalogue impersonation and never a parallel owner.
- Entry: `SteelSeed.Rows(Context)` traverses the unified `SteelRowSeed` table. `SteelDesign.Capacity` admits the rolled or cold-formed modality and its physical inputs before deriving `DesignCapacity`. `SteelDesign.RetentionAt(double, Op) : Fin<(double Ky, double KE)>` rejects non-finite steel temperature before interpolating the EN 1993-1-2 table.
- Packages: VividOrange.Profiles.Catalogue (`CatalogueFactory.CreateAmerican`/`CreateEuropean`, `American`/`European` identities, `AmericanShape`/`EuropeanShape` families, the `II`/`IIParallelFlange`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`+`IHollowStructuralSection` geometry contracts with `IIParallelFlange.FilletRadius`/`IDoubleAngle.BackToBackDistance`; `.api/api-vividorange-profiles-catalogue.md`), VividOrange.Materials (`EnSteelMaterial`/`EnSteelFactory.CreateLinearElastic` the Table 3.1 `f_y` by grade × delivery × thickness band, `EnSteelDeliveryCondition`; the derivation throws trapped at the grade admission; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1993` the EN grades cite, `NationalAnnex`; `.api/api-vividorange-standards.md`), UnitsNet (`Length`/`Pressure` at the admission edge; `.api/api-unitsnet.md`), Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER`; `StudClass` the composite `ΣQn` reads is `joint#JOINT_FAMILY`'s, DEFINED in this parent namespace — no `.Joint` child namespace exists), Thinktecture.Runtime.Extensions (`[SmartEnum]`/`[SmartEnum<string>]` with `[KeyMemberEqualityComparer]` + `[KeyMemberComparer]` stacked for ordered key lookup), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`).
- Growth: the seed IS the registered database (the full `American` and `European` identity domains enumerate through `Enum.GetValues` — a stocked subset is a policy filter over the fold, never the hard bound); a new composite variant one `Augmented` row with its detail; a new cold-formed stud one published `ColdFormedRow`; a new grade one `SteelGrade` row binding its `EnSteelGrade` + the delivery condition whose Table 3.1 sub-table holds it; a new shape family one `SteelClass` row carrying its topology + `FlexureRegime` + `IfcProfileDef` subtype + `OfShape` arm, PLUS the compiler-forced `SectionProfile` arm and `SectionSolver.Solve`/`Forms` arm on `component#SECTION_SOLVER` (buildingSMART profile-schema cadence, never thing cadence) — never a per-section type, never a transcribed property literal, never a parallel section receipt.
- Boundary: `SteelShape.Of` is the BOUNDARY_ADMISSION point where raw `VividOrange` `UnitsNet` geometry is admitted EXACTLY ONCE — the published dims (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the quantity, `.Millimeters` owning the conversion) lift into the `PositiveMagnitude` `SectionDims` columns, an unmatched geometry interface rails `ComponentFault.Family` (never a fabricated sentinel), and the interior carries proven-positive SI scalars with no `UnitsNet` type in a signature; the hollow split is GEOMETRY-driven (`ICircularHollow` before `IRoundedRectangularHollow` before `IRectangularHollow` before the family folds — a round HSS and a rectangular HSS share `AmericanShape.HSS`, so the family enum cannot discriminate them, and the AISC rectangular HSS concretes implement the ROUNDED contract, which does not extend the sharp one) and `SteelClass.OfShape` is TOTAL over both published taxonomies (the EN 10365 families are exclusively i-shape and channel; an unrecognized family rails, never a silent `_ => IShape`); the SOLVE is `component#SECTION_SOLVER`'s — `SectionSolver.Solve` dispatches the `Catalogued` arm over `Shape.Profile` (the exact-fillet `.Utility.Parts` `TrapezoidalPart`/`EllipseQuarterPart` integral) and `Forms.FromCatalogue(Shape)` fills the eight derived columns from `Shape.Section` + `Shape.Class` per topology, so this page holds NO stiffness algebra and NO twenty-column lift (the relocated `SteelStiffness` per-topology closed forms live on the solver's `Forms` kernel; R4 verified the elastic solver computes ONLY `Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter` — no plastic/torsion/warping/shear source exists in VividOrange, so the relocated algebra is load-bearing, never redundant); the design yield is the registered `SteelGrade.YieldMpa(thicknessMm, annex, key)` DATA — the EN bands build the `EnSteelMaterial`, set `Specification.DeliveryCondition` to the band's `Delivery` (AR holds S235/S275/S355/S450; N holds S420/S460), and read the thickness-banded Table 3.1 `f_y` from `EnSteelFactory.CreateLinearElastic`, the derivation throw trapped onto `ComponentFault.Grade`; the AISC/ASTM bands stay spec-nominal (no .NET package owns the AISC or A653 grade tables); `SteelDesign` reads ONLY canonical `ComputedSection` columns (`Iw` for F2 LTB, the receipt's derived `GoverningRadiusMm` for the weak-axis buckling the real column design governs on, `Avy` the major-axis web shear matching the seam `AvY`, `J/c` the §H3.1 closed-section torsional constant `C`) — a re-minted dimension or a parallel `SteelBeamCheck` surface is the deleted form, and `DesignCapacity.TorsionalNmm`/`FlexuralMinorNmm` are the one source the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Steel` arm reads onto `SectionCapacity.SteelLrfd.TorsionalKnm`/`FlexuralMinorKnm`; the composite `ΣQn` reads `joint#JOINT_FAMILY` `StudClass.SteelShearKn × StudsPerMetre × ShearSpanMm` (the one stud vocabulary summed over the AISC §I3.2d max-moment-to-zero-moment span), never a re-derived stud shear and never a per-metre rate against a total force; the element IFC stamp is `IfcBinding.Supertype(ComponentFamily.Steel.Class)` (`IfcBuiltElement` + `NOTDEFINED` — role and placement are occurrence refinements the Bim egress gates) while `SteelClass.IfcSubtype` (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef`, `IfcArbitraryClosedProfileDef` for `DoubleL`/`Composite`) rides the seam `MaterialComposition.ProfileSet` round-trip with `SectionDims.BackToBackMm` crossing onto the Bim `ProfileDims.BackToBackMm` — the parametric `ColdFormedC` stud's profile wire is the projector's profile-arm read (`IfcUShapeProfileDef`), never a `SteelClass` reclass; `DetailLane.None` — a rolled section carries no detail bag, its parametric data riding the solved `ComputedSection` and the Type geometry (the lane/detail totality law `Component.Of` proves); the AISI capacity data path is CLOSED in-page — the capacity consumer resolves the `ColdFormedRow` from a resolved `ColdFormedC` profile through the dims-keyed `ColdFormedSections.Of(profile, key)` (the profile was constructed FROM the row, so the dims round-trip bit-identically), never a designation-string parse.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;                  // ImmutableArray (the frozen ColdFormedSections + FireRetention rosters)
using LanguageExt;                                   // Fin, Option, Seq, Traverse
using Rasm.Numerics;                                  // PositiveMagnitude — the kernel atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                                   // Context, Op, AcceptValidated
using Rasm.Element.Composition;      // MaterialId, MaterialPropertySet (the seam Orthotropic lowering target)
using Thinktecture;                                  // [SmartEnum]/[KeyMemberEqualityComparer]/[KeyMemberComparer], ComparerAccessors
using VividOrange.Profiles;                          // CatalogueFactory, American/European, AmericanShape/EuropeanShape, ICatalogue, IProfile, II/IChannel/...
using VividOrange.Materials.StandardMaterials.En;    // EnSteelGrade, EnSteelMaterial, EnSteelFactory, EnSteelDeliveryCondition (the Table 3.1 f_y source)
using VividOrange.Standards.Eurocode;                // NationalAnnex (the EN factory annex axis)
using UnitsNet;                                      // Length (the thickness-band selector; the native-unit dims at the admission edge)
using static LanguageExt.Prelude;                    // Some, None (Try.lift rides the LanguageExt namespace)

// Every family page declares in the ONE Rasm.Materials.Component namespace, so the parent COMPONENT_OWNER types AND
// StudClass (joint#JOINT_FAMILY defines it here) resolve by bare name; component#COMPONENT_OWNER binds SteelSeed.Rows
// on the ComponentFamily.Steel policy row (the <Family>Seed naming keeps rows collision-free).
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The thin-walled topology selecting the SectionSolver.Forms supplement arm AND the flexure regime (open F2 LTB vs
// closed/solid M_p per §F7): OPEN (I/channel/tee/angle) carries positive warping and a web-vs-flange shear split;
// CLOSED (HSS/pipe) engineering-zero warping and perimeter shear; SOLID bar stock likewise, compact by definition.
// The ONE discriminant — never a per-class duplicate formula; the B4.1 coefficients ride the SteelClass rows.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelTopology {
    public static readonly SteelTopology Open   = new("open");
    public static readonly SteelTopology Closed = new("closed");
    public static readonly SteelTopology Solid  = new("solid");
}

// The AISC 360 Chapter F flexure regime as a PER-CLASS row: F2 the doubly-symmetric/channel LTB over the real Iw,
// F9 the tee/double-angle Mcr with the 1.6·My cap, F10 the single-angle yield/LTB bands, Plastic the §F7/§F8
// closed-section M_p bound — the deleted form ran F2 on EVERY open class (a tee at Zx = 1.7·Sx credits 1.7·My where
// §F9 caps 1.6·My; an angle leg has no F2 spelling at all). A new flexure chapter is one row plus its kernel arm.
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
// HssRound row rides the case-20 D/t E/Fy reference form Classify selects — never one I-shape model on every class.
public readonly record struct SlendernessRow(double FlangeDivisor, double FlangeLambdaP, double FlangeLambdaR, bool WebClear, double WebLambdaP, double WebLambdaR);

// The IfcProfileDef subtype axis over the published family taxonomy — nine rows, each carrying its topology, the
// parameterized-profile subtype the seam MaterialComposition.ProfileSet round-trips (DoubleL/Composite have no single
// parametric form -> IfcArbitraryClosedProfileDef), and its Table B4.1 slenderness row. The ColdFormed row is the
// classification home for a CATALOGUED cold-formed identity; the seeded AISI stud rides the parametric
// SectionProfile.ColdFormedC lane (Formed source), so its shape does not reclass here — AISI S100 effective-width
// governs its capacity through the ColdFormedRow overload, never B4.1.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelClass {
    public static readonly SteelClass IShape      = new("i-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcIShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: new(2, 0.38, 1.00, true,  3.76, 5.70));
    public static readonly SteelClass UShape      = new("u-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: new(1, 0.38, 1.00, true,  3.76, 5.70));
    public static readonly SteelClass LShape      = new("l-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcLShapeProfileDef",           regime: FlexureRegime.F10,     slenderness: new(1, 0.54, 0.91, false, 0.54, 0.91));
    public static readonly SteelClass DoubleAngle = new("double-angle", topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F9,      slenderness: new(1, 0.54, 0.91, false, 0.54, 0.91));
    public static readonly SteelClass HssRect     = new("hss-rect",     topology: SteelTopology.Closed, ifcSubtype: "IfcRectangleHollowProfileDef",  regime: FlexureRegime.Plastic, slenderness: new(1, 1.12, 1.40, true,  2.42, 5.70));
    public static readonly SteelClass HssRound    = new("hss-round",    topology: SteelTopology.Closed, ifcSubtype: "IfcCircleHollowProfileDef",     regime: FlexureRegime.Plastic, slenderness: new(1, 0.07, 0.31, false, 0.07, 0.31));
    public static readonly SteelClass Tee         = new("tee",          topology: SteelTopology.Open,   ifcSubtype: "IfcTShapeProfileDef",           regime: FlexureRegime.F9,      slenderness: new(2, 0.38, 1.00, false, 0.84, 1.52));
    public static readonly SteelClass Composite   = new("composite",    topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F2,      slenderness: new(2, 0.38, 1.00, true,  3.76, 5.70));
    public static readonly SteelClass ColdFormed  = new("cold-formed",  topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: new(1, 0.38, 1.00, true,  3.76, 5.70));
    public SteelTopology Topology { get; }
    public string IfcSubtype { get; }
    public FlexureRegime Regime { get; }
    public SlendernessRow Slenderness { get; }

    // The published AISC family taxonomy IS the discriminant — TOTAL, an unrecognized family rails ComponentFault.Family,
    // never a silent `_ => IShape` mis-classifying a tee/angle/hollow (the deleted runtime-silent arm). HSS maps to the
    // RECTANGULAR default; the round/rect split is SteelShape.Of's GEOMETRY pre-empt, never this enum.
    public static Fin<SteelClass> OfShape(AmericanShape shape, Op key) => shape switch {
        AmericanShape.W or AmericanShape.M or AmericanShape.S or AmericanShape.HP => Fin.Succ(IShape),
        AmericanShape.C or AmericanShape.MC                                       => Fin.Succ(UShape),
        AmericanShape.L                                                           => Fin.Succ(LShape),
        AmericanShape.DoubleL                                                     => Fin.Succ(DoubleAngle),
        AmericanShape.HSS                                                         => Fin.Succ(HssRect),
        AmericanShape.Pipe                                                        => Fin.Succ(HssRound),
        AmericanShape.WT or AmericanShape.MT or AmericanShape.ST                  => Fin.Succ(Tee),
        _ => Fin.Fail<SteelClass>(ComponentFault.Family(key, $"<american-shape-unmapped:{shape}>")),
    };

    // TOTAL over the 25 EN families: the H/I families -> i-shape, the channel families -> u-shape. EN 10365 publishes NO
    // European angle/hollow/tee family, so these two arms exhaust the 25 — the `_` arm is the defensive rail.
    public static Fin<SteelClass> OfShape(EuropeanShape shape, Op key) => shape switch {
        EuropeanShape.IPEAA or EuropeanShape.IPEA or EuropeanShape.IPE or EuropeanShape.IPEO or EuropeanShape.IPEV
            or EuropeanShape.HEAA or EuropeanShape.HEA or EuropeanShape.HEB or EuropeanShape.HEC or EuropeanShape.HEM
            or EuropeanShape.HE or EuropeanShape.HL or EuropeanShape.HLZ or EuropeanShape.HD or EuropeanShape.HP
            or EuropeanShape.UBP or EuropeanShape.UB or EuropeanShape.UC or EuropeanShape.IPN or EuropeanShape.J => Fin.Succ(IShape),
        EuropeanShape.UPE or EuropeanShape.PFC or EuropeanShape.UPN or EuropeanShape.U or EuropeanShape.CH        => Fin.Succ(UShape),
        _ => Fin.Fail<SteelClass>(ComponentFault.Family(key, $"<european-shape-unmapped:{shape}>")),
    };
}

// The AISC Table B4.1 width-to-thickness verdict — a 3-state design class, never a 2-state IsCompact flag.
// Rank is the severity order; Worse folds the independent flange/web verdicts to the governing class.
[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new(rank: 0);
    public static readonly CompactnessClass Noncompact = new(rank: 1);
    public static readonly CompactnessClass Slender    = new(rank: 2);
    public int Rank { get; }
    public CompactnessClass Worse(CompactnessClass other) => Rank >= other.Rank ? this : other;
}

// The structural-steel grade band: the EN bands bind their EnSteelGrade + the EnSteelDeliveryCondition whose EN 1993-1-1
// Table 3.1 sub-table HOLDS the grade (AR/EN 10025-2 holds S235/S275/S355/S450; N/EN 10025-3 and M/EN 10025-4 hold
// S420/S460 — the default AR spec rails them), so the design yield is registered DATA citing En1993; the AISC bands carry
// their spec-nominal (no .NET package owns the AISC grade table). SubstanceId is the per-grade Mechanical row the design
// seam reads; the render AppearanceId rides the seed (the two-slot independence law).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelGrade {
    public static readonly SteelGrade A36  = new("a36",  nominalYieldMpa: 250.0, substanceId: "steel.a36",  enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade A992 = new("a992", nominalYieldMpa: 345.0, substanceId: "steel.a992", enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade A572 = new("a572", nominalYieldMpa: 345.0, substanceId: "steel.a572", enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade A653 = new("a653-gr50", nominalYieldMpa: 340.0, substanceId: "steel.a653", enGrade: None, delivery: EnSteelDeliveryCondition.AR);   // ASTM A653 SS Gr 50 — the cold-formed sheet band the AISI stud lane rolls
    public static readonly SteelGrade A500Rect  = new("a500-grc-rect",  nominalYieldMpa: 345.0, substanceId: "steel.a500", enGrade: None, delivery: EnSteelDeliveryCondition.AR);   // ASTM A500 Gr C rectangular HSS
    public static readonly SteelGrade A500Round = new("a500-grc-round", nominalYieldMpa: 317.0, substanceId: "steel.a500", enGrade: None, delivery: EnSteelDeliveryCondition.AR);   // ASTM A500 Gr C round HSS
    public static readonly SteelGrade A53  = new("a53-grb", nominalYieldMpa: 240.0, substanceId: "steel.a53",  enGrade: None, delivery: EnSteelDeliveryCondition.AR);   // ASTM A53 Gr B pipe
    public static readonly SteelGrade S235 = new("s235", nominalYieldMpa: 235.0, substanceId: "steel.s235", enGrade: Some(EnSteelGrade.S235), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S275 = new("s275", nominalYieldMpa: 275.0, substanceId: "steel.s275", enGrade: Some(EnSteelGrade.S275), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S355 = new("s355", nominalYieldMpa: 355.0, substanceId: "steel.s355", enGrade: Some(EnSteelGrade.S355), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S420 = new("s420", nominalYieldMpa: 420.0, substanceId: "steel.s420", enGrade: Some(EnSteelGrade.S420), delivery: EnSteelDeliveryCondition.N);
    public static readonly SteelGrade S450 = new("s450", nominalYieldMpa: 440.0, substanceId: "steel.s450", enGrade: Some(EnSteelGrade.S450), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S460 = new("s460", nominalYieldMpa: 460.0, substanceId: "steel.s460", enGrade: Some(EnSteelGrade.S460), delivery: EnSteelDeliveryCondition.N);
    public double NominalYieldMpa { get; }
    public string SubstanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public Option<EnSteelGrade> EnGrade { get; }
    public EnSteelDeliveryCondition Delivery { get; }

    // The thickness-banded design yield: an EN grade builds the EnSteelMaterial, routes its Specification.DeliveryCondition
    // to the sub-table that holds it, and reads the <=40 mm vs 40-80 mm f_y the factory's elementThickness selects (the
    // section's flange thickness is the band selector); the derivation throw traps onto ComponentFault.Grade. An AISC band
    // returns its spec-nominal.
    public Fin<double> YieldMpa(double elementThicknessMm, NationalAnnex annex, Op key) =>
        EnGrade.Match(
            Some: g => Try.lift(() => {
                    EnSteelMaterial material = new(g, annex);
                    material.Specification.DeliveryCondition = Delivery;
                    return EnSteelFactory.CreateLinearElastic(material, Length.FromMillimeters(elementThicknessMm)).Strength.Megapascals;
                }).Run()
                .MapFail(e => ComponentFault.Grade(key, $"<en-steel-grade:{g}:{annex}:{Delivery}:{e.Message}>")),
            None: () => Fin.Succ(NominalYieldMpa));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The admitted published-dims currency: the four load-bearing dims are proven-positive PositiveMagnitude (WidthMm/DepthMm
// are the SectionProfile.Catalogued gross base-constructor state; WebMm/FlangeMm feed the Forms closed forms and the B4.1
// classifier — the hollow arms carry wall thickness in both), FilletMm/BackToBackMm are the >=0 slots (fillet on
// IIParallelFlange AND the rounded-HSS corner radius; BackToBackMm the IDoubleAngle spacing crossing onto the Bim
// ProfileDims.BackToBackMm round-trip).
public readonly record struct SectionDims(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double BackToBackMm);

// The published-identity payload of the SectionProfile.Catalogued arm: Profile the IProfile the SectionSolver Catalogued
// arm integrates (exact fillet/HSS-corner parts), Section the admitted dims Forms.FromCatalogue and Classify read, Grade
// the registered yield band, plus the composite augmentation. The twenty-column ComputedSection is NOT a field — it
// lives in the catalogue section map SectionSolver.Solve fills for every Sectioned row.
public sealed record SteelShape(
    string Label, SteelClass Class, IProfile Profile, SectionDims Section,
    SteelGrade Grade, ComponentStandard Standard,
    Option<CompositeDetail> Composite = default) {

    // The ONE catalogue admission boundary: geometry-driven class resolution, the family-interface dims read, and the
    // PositiveMagnitude lift — raw UnitsNet admitted exactly once; the interior never sees a quantity or a sentinel.
    public static Fin<SteelShape> Of(ICatalogue catalogue, SteelGrade grade, ComponentStandard standard, Op key) =>
        from cls in ClassOf(catalogue, key)
        from dims in DimsOf(catalogue, key)
        select new SteelShape(catalogue.Label, cls, (IProfile)catalogue, dims, grade, standard);

    // Geometry pre-empts the family fold: a round HSS and a rectangular HSS carry the SAME AmericanShape.HSS (verified:
    // HSS13_375x_625 is ICircularHollow, HSS8x8x_500 is IRoundedRectangularHollow — the ROUNDED contract does NOT
    // extend IRectangularHollow, so BOTH rectangular arms are load-bearing); the open families dispatch onto the
    // TOTAL OfShape folds.
    static Fin<SteelClass> ClassOf(ICatalogue catalogue, Op key) => catalogue switch {
        ICircularHollow            => Fin.Succ(SteelClass.HssRound),
        IRoundedRectangularHollow  => Fin.Succ(SteelClass.HssRect),
        IRectangularHollow         => Fin.Succ(SteelClass.HssRect),
        IAmericanCatalogue a       => SteelClass.OfShape(a.Shape, key),
        IEuropeanCatalogue e       => SteelClass.OfShape(e.Shape, key),
        _ => Fin.Fail<SteelClass>(ComponentFault.Family(key, $"<catalogue-not-american-or-european:{catalogue.Label}>")),
    };

    // The family geometry read in the native published unit (AISC Inch, EN Millimeter — .Millimeters owns the conversion):
    // IIParallelFlange (W/HEA/IPE, with fillet) precedes the II base (S/HP taper flanges carry no fillet); IDoubleAngle
    // precedes IAngle; the hollow arms ride envelope + IHollowStructuralSection wall, the AISC rounded-rect arm
    // (HSS8x8x_500 : IRoundedRectangularHollow — NOT an IRectangularHollow) carrying its corner radius onto the fillet
    // slot from the flat-width deltas. An unmatched interface rails ComponentFault.Family — never a fabricated sentinel
    // passing the PositiveMagnitude admission.
    // Tuple columns are (depth, width, web, flange, fillet, backToBack) — positional, so every arm unifies.
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
            _ => Fin.Fail<(double, double, double, double, double, double)>(ComponentFault.Family(key, $"<catalogue-geometry-interface-unsupported:{catalogue.Label}>")),
        })
        .Bind(raw =>
            from depth in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item1)
            from width in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item2)
            from web in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item3)
            from flange in key.AcceptValidated<PositiveMagnitude>(candidate: raw.Item4)
            select new SectionDims(depth, width, web, flange, raw.Item5, raw.Item6));
}

// AISC 360 Chapter I composite-action detail: the slab over the steel core plus the joint#JOINT_FAMILY StudClass
// shear-stud reference. ΣQn sums the studs over the SHEAR SPAN (§I3.2d — max-moment to zero-moment):
// ΣQn = StudClass.SteelShearKn × StudsPerMetre × ShearSpanM (the per-stud Eq I8-1 cap, NEVER a re-derived stud
// shear; the deleted form multiplied a per-metre rate against total forces — a dimensional mismatch).
public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    int StudsPerMetre,
    PositiveMagnitude ShearSpanMm);

// The AISC 360 LRFD receipt over the canonical ComputedSection. FlexuralMinorNmm is the §F6 weak-axis φMny =
// φb·min(Fy·Zy, 1.6·Fy·Sy) bounded by F6.2 flange local buckling (the F10 single-angle regime caps 1.5·Fy·Sy).
// TorsionalNmm is the §H3.1 design torsional resistance φT·Fcr·C — positive for a CLOSED HSS/pipe (C = J/c off the
// carried JMm4), engineering-zero for an OPEN shape whose §H3.3 warping torsion is not a single-resistance scalar, so
// a torsion demand on an open shape surfaces as the governing over-ratio. These are the columns capacity#SECTION_CAPACITY
// SectionCapacity.Lift(CapacityReceipt) CapacityReceipt.Steel arm reads onto SectionCapacity.SteelLrfd — capacity columns, never re-passed lift arguments.
public readonly record struct DesignCapacity(double FlexuralNmm, double FlexuralMinorNmm, double CompressionN, double ShearN, double TorsionalNmm, CompactnessClass Classification, double Slenderness);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The AISC 360 + AISI S100 design projections over the CANONICAL ComputedSection the resolution cache supplies
// (graph.SectionOf / the catalogue section map — the solve ran once at catalogue build): classification reads the
// shape's admitted dims, capacity reads the receipt's real Iw (LTB), GoverningRadiusMm (weak-axis buckling), Avy
// (major-axis web shear), J/c (§H3.1 C); the AISI stud modality is the ColdFormedRow overload of the SAME Capacity.
// Lifted into capacity#SECTION_CAPACITY through SectionCapacity.Lift(CapacityReceipt) on its CapacityReceipt.Steel case.
public static class SteelDesign {
    const double φb = 0.90, φc = 0.90, φv = 0.90, E = 200_000.0, G = 77_200.0;

    // ONE Table B4.1 generator over the per-class SlendernessRow — the deleted form ran the rolled-I model on every
    // open class (a channel's FULL flange width halved, a tee stem and an angle leg misread against case-15 web
    // limits). Flange and web λ verdict independently against λp/λr in the class's reference form (√(E/Fy); the
    // HssRound row rides the case-20 E/Fy form), the WORSE verdict governs; solid bar stock is compact by definition.
    // HSS-rect flange b/t reads the full width over the wall — conservative against the B−3t flat-width allowance
    // (the admitted corner radius on FilletMm refines the geometric owner; this classifier keeps the conservative bound).
    public static CompactnessClass Classify(SteelShape shape, double yieldMpa) {
        SectionDims d = shape.Section;
        SlendernessRow row = shape.Class.Slenderness;
        double r = shape.Class == SteelClass.HssRound ? E / yieldMpa : Math.Sqrt(E / yieldMpa);
        double flange = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value);
        double web = (row.WebClear ? Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, 0.0) : d.DepthMm.Value) / d.WebMm.Value;
        return shape.Class.Topology == SteelTopology.Solid
            ? CompactnessClass.Compact
            : Verdict(flange, row.FlangeLambdaP * r, row.FlangeLambdaR * r).Worse(Verdict(web, row.WebLambdaP * r, row.WebLambdaR * r));
    }

    static CompactnessClass Verdict(double ratio, double λp, double λr) =>
        ratio > λr ? CompactnessClass.Slender : ratio <= λp ? CompactnessClass.Compact : CompactnessClass.Noncompact;

    // Chapters F/E/G LRFD over the resolved receipt: φMn through the per-class FlexureRegime row — F2 LTB reading the
    // REAL IwMm6 bounded by the §F3 flange-local resistance (the B4.1 verdict is a CAPACITY input, never a side label),
    // F9 tee/double-angle Mcr capped 1.6·My, F10 single-angle bands, closed/solid Mp per §F7/§F8 (the deleted form ran
    // F2 on every open class AND on closed sections); φPn = φc·Fcr·A over the receipt's weak-axis GoverningRadiusMm
    // (never the strong-axis Math.Sqrt(Ix/A) approximation); φVn = φv·0.6·Fy·Avy over the MAJOR-axis web (the seam AvY,
    // NOT the minor flange AvzMm2); the Composite arm the Ch I plastic couple capped at ΣQn and FLOORED at the bare-steel
    // Mn — the steel section alone always carries its rolled resistance, so ΣQn = 0 degrades to non-composite, never to
    // zero (the deleted zero-floor). The AISI stud rides the ColdFormedRow overload below, never a Some-detail override.
    public static Fin<DesignCapacity> Capacity(SteelShape shape, ComputedSection s, double yieldMpa, double unbracedLengthMm, double effectiveLengthMm, Op key) {
        if (!double.IsFinite(yieldMpa + unbracedLengthMm + effectiveLengthMm) || yieldMpa <= 0.0 || unbracedLengthMm < 0.0 || effectiveLengthMm <= 0.0) {
            return ComponentFault.Capacity(key, $"<steel-design-input-rejected:{yieldMpa:R}:{unbracedLengthMm:R}:{effectiveLengthMm:R}>");
        }
        double λc = effectiveLengthMm / s.GoverningRadiusMm;
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        double Fcr = Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe;
        double Mp = yieldMpa * s.ZxMm3.Value;
        double rolledMn = shape.Class.Regime.Switch(
            state: (Shape: shape, Section: s, Lb: unbracedLengthMm, Fy: yieldMpa, Mp),
            f2:      static x => Math.Min(LateralTorsionalMn(x.Section, x.Lb, x.Fy, x.Mp), FlangeLocalMn(x.Shape, x.Section, x.Fy, x.Mp)),
            f9:      static x => TeeMn(x.Section, x.Lb, x.Fy, x.Mp),
            f10:     static x => AngleMn(x.Shape, x.Section, x.Lb, x.Fy),
            plastic: static x => x.Mp);
        double Mn = shape.Composite.Match(
            Some: c => Math.Max(CompositeMn(c, s, yieldMpa), rolledMn),
            None: () => rolledMn);
        return Fin.Succ(new DesignCapacity(
            FlexuralNmm: φb * Mn,
            FlexuralMinorNmm: φb * MinorMn(shape, s, yieldMpa),
            CompressionN: φc * Fcr * s.AreaMm2.Value,
            ShearN: φv * 0.6 * yieldMpa * s.AvyMm2.Value,
            TorsionalNmm: TorsionalResistance(shape, s, yieldMpa),
            Classification: Classify(shape, yieldMpa),
            Slenderness: λc));
    }

    // The AISI S100 stud modality of the SAME projection — the ColdFormedRow input discriminates it (a parametric stud
    // carries no SteelShape): F3.1 initiation-of-yielding φb·Fy·(Seff/S)·Sx over the REAL solved ColdFormedC receipt
    // (the minor arm the SAME F3.1 form on SyMm3 — one published Seff proxy, both axes, matching the E3.1 posture),
    // E3.1 flexural buckling on the same 0.658 curve with the published ratio as the conservative effective-area proxy,
    // G2 web shear on the receipt AvyMm2, no closed-torsion arm (an open lipped C). A published Seff/S < 1 IS the
    // slender verdict — post-buckling effective width is the cold-formed design premise, never a B4.1 read.
    public static Fin<DesignCapacity> Capacity(ColdFormedRow stud, ComputedSection s, double yieldMpa, double effectiveLengthMm, Op key) {
        if (!double.IsFinite(yieldMpa + effectiveLengthMm) || yieldMpa <= 0.0 || effectiveLengthMm <= 0.0) {
            return ComponentFault.Capacity(key, $"<cold-formed-design-input-rejected:{yieldMpa:R}:{effectiveLengthMm:R}>");
        }
        double λc = effectiveLengthMm / s.GoverningRadiusMm;
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        double Fcr = Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe;
        return Fin.Succ(new DesignCapacity(
            FlexuralNmm: φb * yieldMpa * s.SxMm3.Value * stud.SeffRatio,
            FlexuralMinorNmm: φb * yieldMpa * s.SyMm3.Value * stud.SeffRatio,
            CompressionN: φc * Fcr * s.AreaMm2.Value * stud.SeffRatio,
            ShearN: φv * 0.6 * yieldMpa * s.AvyMm2.Value,
            TorsionalNmm: 0.0,
            Classification: stud.SeffRatio < 1.0 ? CompactnessClass.Slender : CompactnessClass.Compact,
            Slenderness: λc));
    }

    // §F3 flange local buckling bounding the F2 classes: compact passes Mp through; noncompact interpolates
    // Mp -> 0.7·Fy·Sx across λpf..λrf (F3-1); slender reads the elastic 0.9·E·kc·Sx/λ² with kc = 4/√(h/tw)
    // clamped to [0.35, 0.76] (F3-2).
    static double FlangeLocalMn(SteelShape shape, ComputedSection s, double Fy, double Mp) {
        SectionDims d = shape.Section;
        SlendernessRow row = shape.Class.Slenderness;
        double r = Math.Sqrt(E / Fy);
        double λ = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value), λp = row.FlangeLambdaP * r, λr = row.FlangeLambdaR * r;
        double kc = Math.Clamp(4.0 / Math.Sqrt(Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, d.WebMm.Value) / d.WebMm.Value), 0.35, 0.76);
        return λ <= λp ? Mp
            : λ <= λr ? Mp - (Mp - 0.7 * Fy * s.SxMm3.Value) * (λ - λp) / (λr - λp)
            : 0.9 * E * kc * s.SxMm3.Value / (λ * λ);
    }

    // §F6 weak-axis flexure: Mny = min(Fy·Zy, cap·Fy·Sy) per F6-1 (cap 1.6; 1.5 on the F10 single-angle regime per
    // F10-1 — no minor-axis LTB limit state exists). The F2 I/channel classes bound by §F6.2 flange local buckling
    // over the SAME per-class SlendernessRow lambda spelling FlangeLocalMn reads (noncompact F6-2 interpolation,
    // slender F6-4 Fcr = 0.69·E/λ² on Sy) — zero new coefficients.
    static double MinorMn(SteelShape shape, ComputedSection s, double Fy) {
        double cap = shape.Class.Regime == FlexureRegime.F10 ? 1.5 : 1.6;
        double mpy = Math.Min(Fy * s.ZyMm3.Value, cap * Fy * s.SyMm3.Value);
        if (shape.Class.Regime != FlexureRegime.F2) { return mpy; }
        SectionDims d = shape.Section;
        SlendernessRow row = shape.Class.Slenderness;
        double r = Math.Sqrt(E / Fy);
        double λ = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value), λp = row.FlangeLambdaP * r, λr = row.FlangeLambdaR * r;
        return λ <= λp ? mpy
            : λ <= λr ? mpy - (mpy - 0.7 * Fy * s.SyMm3.Value) * (λ - λp) / (λr - λp)
            : 0.69 * E * s.SyMm3.Value / (λ * λ);
    }

    // §H3.1 φTn = φT·Fcr·C for the CLOSED topologies (and the solid bar's J/c modulus): Fcr = 0.6·Fy the compact-wall
    // governing limit, C = J/c with c the outer half-depth — grounded in the carried JMm4, never a re-derived wall
    // integral. An OPEN shape returns 0: §H3.3 non-HSS torsion is a warping-normal-stress interaction, not a single
    // resistance the capacity ratio fold can divide against.
    static double TorsionalResistance(SteelShape shape, ComputedSection s, double yieldMpa) {
        double closedForm = φv * 0.6 * yieldMpa * s.JMm4.Value / (0.5 * s.DepthMm.Value);
        return shape.Class.Topology.Map(open: 0.0, closed: closedForm, solid: closedForm);
    }

    // §F9 tee / double-angle LTB: Mcr = π·√(E·Iy·G·J)/Lb·(B + √(1+B²)), B = 2.3·(d/Lb)·√(Iy/J) — the stem-in-TENSION
    // positive branch (the rolled default; a stem-compression demand refines the sign caller-side, below the cap);
    // the plastic bound is Mp ≤ 1.6·My, NOT the bare Zx couple the F2 arm credits.
    static double TeeMn(ComputedSection s, double Lb, double Fy, double Mp) {
        double cap = Math.Min(Mp, 1.6 * Fy * s.SxMm3.Value);
        if (Lb <= 0.0) return cap;
        double b = 2.3 * (s.DepthMm.Value / Lb) * Math.Sqrt(s.IyMm4.Value / s.JMm4.Value);
        return Math.Min(cap, Math.PI * Math.Sqrt(E * s.IyMm4.Value * G * s.JMm4.Value) / Lb * (b + Math.Sqrt(1.0 + b * b)));
    }

    // §F10 single-angle geometric-axis bending: yield cap 1.5·My (F10-1); elastic Me = 0.46·E·b²·t²/Lb (F10-5a,
    // equal-leg, Cb = 1); the (0.92 − 0.17·Me/My)·Me elastic band below My, the (1.92 − 1.17·√(My/Me))·My inelastic
    // band above, capped at 1.5·My (F10-2/F10-3). Leg thickness is the admitted SectionDims wall column.
    static double AngleMn(SteelShape shape, ComputedSection s, double Lb, double Fy) {
        double my = Fy * s.SxMm3.Value, cap = 1.5 * my;
        if (Lb <= 0.0) return cap;
        double me = 0.46 * E * Math.Pow(s.WidthMm.Value * shape.Section.WebMm.Value, 2.0) / Lb;
        return me <= my ? (0.92 - 0.17 * me / my) * me : Math.Min(cap, (1.92 - 1.17 * Math.Sqrt(my / me)) * my);
    }

    // §F2 LTB reading the REAL warping: Lp = 1.76·ry·√(E/Fy), rts ≈ √(√(Iy·Iw)/Sx), Lr from J/Iw, the linear
    // Mp -> 0.7·Fy·Sx interpolation between, elastic Fcr·Sx beyond Lr.
    static double LateralTorsionalMn(ComputedSection s, double Lb, double Fy, double Mp) {
        double ry = s.RyMm.Value, sx = s.SxMm3.Value, iy = s.IyMm4.Value, iw = s.IwMm6, jj = s.JMm4.Value, depth = s.DepthMm.Value;
        double Lp = 1.76 * ry * Math.Sqrt(E / Fy);
        double rts = iw > 0.0 ? Math.Sqrt(Math.Sqrt(iy * iw) / sx) : ry;
        double c = 1.0, ho = depth;                              // doubly-symmetric c = 1; ho conservatively the section depth
        double term = jj * c / (sx * ho);
        double Lr = 1.95 * rts * E / (0.7 * Fy) * Math.Sqrt(term + Math.Sqrt(term * term + 6.76 * Math.Pow(0.7 * Fy / E, 2.0)));
        return Lb <= Lp
            ? Mp
            : Lb <= Lr
                ? Math.Max(0.7 * Fy * sx, Mp - (Mp - 0.7 * Fy * sx) * Math.Clamp((Lb - Lp) / (Lr - Lp), 0.0, 1.0))
                : Math.Min(Mp, FcrLtb(Lb, rts, jj, c, sx, ho) * sx);
    }

    // §F2 elastic-LTB critical stress beyond Lr (Cb = 1 conservative).
    static double FcrLtb(double Lb, double rts, double jj, double c, double sx, double ho) {
        double slender = Lb / rts;
        return Math.PI * Math.PI * E / (slender * slender) * Math.Sqrt(1.0 + 0.078 * jj * c / (sx * ho) * slender * slender);
    }

    // AISC 360 Eq C-I3 fully-OR-partially-composite plastic moment: As·Fy tension balanced by the 0.85·f'c·b·a block,
    // capped at the joint#JOINT_FAMILY ΣQn summed over the §I3.2d shear span (partial composite when ΣQn < As·Fy;
    // ΣQn = 0 zeroes the COUPLE — the caller's Match floors the row at the bare-steel rolledMn, so the beam never
    // reads below its own section).
    static double CompositeMn(CompositeDetail c, ComputedSection s, double yieldMpa) {
        double tSteel = s.AreaMm2.Value * yieldMpa;
        double cConcMax = 0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value * c.SlabDepthMm.Value;
        double sumQn = c.Stud.SteelShearKn * 1e3 * Math.Max(0, c.StudsPerMetre) * c.ShearSpanMm.Value / 1000.0;
        double horizShear = Math.Min(Math.Min(tSteel, cConcMax), sumQn);
        double a = Math.Min(c.SlabDepthMm.Value, horizShear / (0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value));
        double leverArm = 0.5 * s.DepthMm.Value + c.SlabDepthMm.Value - 0.5 * a;
        return horizShear * leverArm;
    }

    // --- [FIRE]
    // EN 1993-1-2 critical-temperature facts over the CARRIED receipt — the columns the ambient rail never consumed:
    // Am/V from the receipt's own HeatedPerimeterMm/AreaMm2 (the heating-rate driver a Compute fire runner feeds its
    // time-temperature curve; the AISC Appendix 4 W/D analogue), the Table 3.1 retention pair at a steel
    // temperature, and the §4.2.4 critical temperature of a load ratio — the exposure/time-temperature side stays
    // the placement-level caller input the timber exposureMinutes convention fixes.
    public static double SectionFactorPerM(ComputedSection s) => s.HeatedPerimeterMm.Value / s.AreaMm2.Value * 1000.0;

    public static Fin<(double Ky, double KE)> RetentionAt(double steelTemperatureC, Op key) =>
        FireRetention.At(steelTemperatureC, key);

    // §4.2.4 θ_cr = 39.19·ln(1/(0.9674·μ₀^3.833) − 1) + 482 over μ₀ = E_fi,d/R_fi,d,0; μ₀ admits on the physical
    // (0, 1] domain, and the standard's 0.013 lower validity bound applies only after admission.
    public static Fin<double> CriticalTemperatureC(double utilisation, Op key) =>
        double.IsFinite(utilisation) && utilisation is > 0.0 and <= 1.0
            ? Fin.Succ(39.19 * Math.Log(1.0 / (0.9674 * Math.Pow(Math.Max(utilisation, 0.013), 3.833)) - 1.0) + 482.0)
            : ComponentFault.Capacity(key, $"<steel-fire-utilisation-rejected:{utilisation:R}>");
}

// --- [TABLES] ------------------------------------------------------------------------------
// The published AISI/SSMA stud section as a FROZEN ROW (SEED_ROW_LAW AUTHORED — no catalogue producer exists):
// depth/flange/lip/wall/corner the SSMA profile dims, EffectiveWebMm and SeffRatio the published effective-section
// values. The stud seeds the PARAMETRIC SectionProfile.ColdFormedC — never a hot-rolled catalogue impersonating a
// 1.4 mm stud — so the solved ComputedSection carries the REAL stud columns the AISI Capacity overload reads.
public readonly record struct ColdFormedRow(string Key, double DepthMm, double FlangeMm, double WallMm, double LipMm, double FilletMm, double EffectiveWebMm, double SeffRatio);

public static class ColdFormedSections {
    public static readonly ColdFormedRow S600S162_54 = new("600s162-54", 152.4, 41.3, 1.37, 12.7, 4.76, 119.0, 0.78);
    public static readonly ImmutableArray<ColdFormedRow> Rows = [S600S162_54];

    // The dims-keyed resolve the capacity consumer calls on a resolved ColdFormedC profile: the profile was CONSTRUCTED
    // from a row, so the four dims round-trip bit-identically — a miss is a foreign parametric profile, railed loud.
    public static Fin<ColdFormedRow> Of(SectionProfile.ColdFormedC profile, Op key) =>
        Rows.ToSeq().Find(r => r.DepthMm == profile.DepthMm.Value && r.FlangeMm == profile.WidthMm.Value
                && r.WallMm == profile.WallMm.Value && r.LipMm == profile.GirthMm.Value)
            .Match(Some: Fin.Succ, None: () => Fin.Fail<ColdFormedRow>(ComponentFault.Family(key, $"<cold-formed-profile-unregistered:{profile.DepthMm.Value:R}x{profile.WallMm.Value:R}>")));
}

// EN 1993-1-2 Table 3.1 retention as PUBLISHED rows (SEED_ROW_LAW — standards data as a readonly record struct row
// table): ky,θ the effective-yield retention, kE,θ the Young's-modulus retention; steel keeps full yield to 400 °C
// and is spent at 1200 °C. At rejects non-finite input, then interpolates linearly between anchors and clamps only
// finite temperatures at the published band ends.
public readonly record struct FireRetentionRow(double TemperatureC, double Ky, double KE);

public static class FireRetention {
    public static readonly ImmutableArray<FireRetentionRow> Rows = [
        new(20.0, 1.00, 1.000), new(100.0, 1.00, 1.000), new(200.0, 1.00, 0.900), new(300.0, 1.00, 0.800),
        new(400.0, 1.00, 0.700), new(500.0, 0.78, 0.600), new(600.0, 0.47, 0.310), new(700.0, 0.23, 0.130),
        new(800.0, 0.11, 0.090), new(900.0, 0.06, 0.0675), new(1000.0, 0.04, 0.0450), new(1100.0, 0.02, 0.0225),
        new(1200.0, 0.00, 0.000)];

    public static Fin<(double Ky, double KE)> At(double temperatureC, Op key) =>
        double.IsFinite(temperatureC)
            ? Fin.Succ(Interpolate(temperatureC))
            : ComponentFault.Capacity(key, $"<steel-fire-temperature-rejected:{temperatureC:R}>");

    static (double Ky, double KE) Interpolate(double temperatureC) {
        double t = Math.Clamp(temperatureC, Rows[0].TemperatureC, Rows[^1].TemperatureC);
        int i = Rows.TakeWhile(r => r.TemperatureC < t).Count();
        if (i == 0) { return (Rows[0].Ky, Rows[0].KE); }
        FireRetentionRow lo = Rows[i - 1], hi = Rows[i];
        double f = (t - lo.TemperatureC) / (hi.TemperatureC - lo.TemperatureC);
        return (lo.Ky + f * (hi.Ky - lo.Ky), lo.KE + f * (hi.KE - lo.KE));
    }
}

// The profile SOURCE axis of one seed row — the two legal origins of a steel profile, closed so a rolled+formed
// hybrid is unrepresentable: a published catalogue identity (its composite augmentation riding the case) or a
// published cold-formed stud row.
[Union]
public abstract partial record SteelRowSource {
    private SteelRowSource() { }
    public sealed record Rolled(ICatalogue Catalogue, Option<CompositeDetail> Composite) : SteelRowSource;
    public sealed record Formed(ColdFormedRow Row) : SteelRowSource;
}

// ONE seed vocabulary for every steel row — rolled American, rolled European, composite, cold-formed — so ONE Traverse
// folds the whole family; the Source union carries the per-lane payload.
public readonly record struct SteelRowSeed(string Designation, SteelRowSource Source, SteelGrade Grade, ComponentStandard Standard);

public static class SteelSeed {
    static readonly ComponentStandard Aisc = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Aisc);
    static readonly ComponentStandard En   = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);

    // The AISC grade-selection policy over the minted geometry/family: W-series and structural tees A992,
    // HSS A500 Gr C (the rect/round split resolved by the SAME geometry interfaces the class split reads), Pipe
    // A53 Gr B, the remaining rolled families A36; every EN 10365 identity seeds S355. One policy value per
    // family — never a per-row grade literal.
    static SteelGrade GradeOf(ICatalogue catalogue) => catalogue switch {
        IAmericanCatalogue a when a.Shape is AmericanShape.Pipe                  => SteelGrade.A53,
        ICircularHollow                                                          => SteelGrade.A500Round,
        IRoundedRectangularHollow or IRectangularHollow                          => SteelGrade.A500Rect,
        IAmericanCatalogue a when a.Shape is AmericanShape.W or AmericanShape.WT => SteelGrade.A992,
        IAmericanCatalogue                                                       => SteelGrade.A36,
        _                                                                        => SteelGrade.S355,
    };

    static SteelRowSeed Rolled(American id) {
        ICatalogue minted = CatalogueFactory.CreateAmerican(id);
        return new($"steel.{id.ToString().ToLowerInvariant()}", new SteelRowSource.Rolled(minted, None), GradeOf(minted), Aisc);
    }

    static SteelRowSeed Rolled(European id) => new($"steel.{id.ToString().ToLowerInvariant()}", new SteelRowSource.Rolled(CatalogueFactory.CreateEuropean(id), None), SteelGrade.S355, En);

    // The augmented rows ride the SAME seed vocabulary: a W18x50 composite floor beam (1200×100 mm slab, f'c 28 MPa,
    // 3/4in studs at 2/m over the 4.5 m §I3.2d shear span of a 9 m simple beam — ΣQn reads StudClass.S19.SteelShearKn)
    // and the 600S162-54 AISI stud on its OWN published ColdFormedRow at the A653 SS Gr 50 band — the prior C15x33_9
    // catalogue core published a 381 mm hot-rolled channel's twenty columns under the stud designation, the deleted
    // impersonation. Literal positive constants — seed DATA, the same posture as a SmartEnum row column.
    static readonly Seq<SteelRowSeed> Augmented = Seq(
        new SteelRowSeed("steel.comp-w18x50-slab120",
            new SteelRowSource.Rolled(CatalogueFactory.CreateAmerican(American.W18x50),
                Some(new CompositeDetail(PositiveMagnitude.Create(1200.0), PositiveMagnitude.Create(100.0), 28.0, StudClass.S19, 2, PositiveMagnitude.Create(4500.0)))),
            SteelGrade.A992, Aisc),
        new SteelRowSeed("steel.cf-600s162-54", new SteelRowSource.Formed(ColdFormedSections.S600S162_54), SteelGrade.A653, Aisc));

    // The FULL registered database is the seed domain: every AISC American and EN 10365 European identity
    // enumerates through the singleton factory — a model importing a W40x593 or an HL1100R dereferences it, and a
    // sizing fold scans the whole space; a stocked subset is a policy FILTER over this fold, never the hard bound.
    // A new published section arrives with the catalogue package, zero seed edits.
    static Seq<SteelRowSeed> Seeds =>
        toSeq(Enum.GetValues<American>()).Map(Rolled)
            .Concat(toSeq(Enum.GetValues<European>()).Map(Rolled))
            .Concat(Augmented);

    // The ONE generator arm: the Source union selects the profile origin — a catalogue admitted once through
    // SteelShape.Of (the composite reclass riding the Rolled case) or the railed parametric ColdFormedC from the
    // published stud row — then Component.Of constructs the row: IfcBinding.Supertype the IfcBuiltElement stamp (the
    // leaf role is an occurrence refinement), Coring.None, DetailLane.None (no bag), the grade Substance and the
    // stable metal.iron render row on the two independent MaterialId slots.
    static Fin<ComponentRow> RowOf(SteelRowSeed seed) {
        Op key = Op.Of(name: seed.Designation);
        return seed.Source.Switch(
                state: (Seed: seed, Key: key),
                rolled: static (x, r) => SteelShape.Of(r.Catalogue, x.Seed.Grade, x.Seed.Standard, x.Key)
                    .Map(shape => r.Composite.IsSome ? shape with { Class = SteelClass.Composite, Composite = r.Composite } : shape)
                    .Map(shape => (SectionProfile)new SectionProfile.Catalogued(shape)),
                formed: static (x, f) => SectionProfile.ColdFormedC.Of(f.Row.DepthMm, f.Row.FlangeMm, f.Row.WallMm, f.Row.LipMm, f.Row.FilletMm, x.Key))
            .Bind(profile => Component.Of(
                ComponentFamily.Steel, seed.Designation, profile,
                IfcBinding.Supertype(ComponentFamily.Steel.Class), Coring.None, seed.Standard,
                seed.Grade.Substance, MaterialId.Of("metal.iron"), detail: None, key))
            .Map(static item => new ComponentRow(item, Sectioned: true));
    }

    // Fail-loud: ONE Traverse over the unified seed — a ClassOf/dims/admission failure ABORTS the catalogue build. The
    // prior Shapes fold's THREE swallow sites (.Choose(...).ToOption() twice, .IfFail(Seq<SteelShape>())) are the deleted
    // forms: a steel row is Sectioned, so a row that cannot admit or solve is fatal, never silently absent. The Context
    // parameter is the ComponentFamily.Rows delegate contract; the steel seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Seeds.Traverse(RowOf).As();
}
```

## [03]-[RESEARCH]

- [POLICY_ROW_AND_SEED]: REALIZED — steel is one `ComponentFamily.Steel` policy row, and `SteelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` traverses one `SteelRowSeed` vocabulary. `SteelRowSource.Rolled` carries the already-minted `ICatalogue`; deferred payload creation carried no distinct timing policy and is collapsed. Every row is `Sectioned: true`, so a profile admission or solve failure aborts the catalogue build.
- [RELOCATION_LEDGER]: REALIZED — `SteelStiffness` RELOCATED to `component#SECTION_SOLVER` `SectionSolver.Forms` as the open-thin-walled supplement kernel, and `SectionReader.Read`'s twenty-column lift RELOCATED to `SectionSolver.Admit` with the `Catalogued` arm integrating `Shape.Profile` and `Forms.FromCatalogue(Shape)` deriving the supplement from `Shape.Section` + `Shape.Class` per topology. The relocated per-topology content, preserved without loss: OPEN — rectangular-component plastic moduli (`Zx = bf·tf·(D−tf) + ¼·tw·h²`, the two-flange-plus-web `Zy`), open thin-walled `J = Σb·t³/3`, doubly-symmetric `Iw ≈ tf·bf³·hₒ²/24` (channel half), AISC §G web `d·tw` major / `⁵⁄₃·bf·tf` minor shear, the channel minor-axis shear-centre offset `e = hₒ²·tf·bf³/(12·Iw)`, the tee/angle heel offsets and the SN030 `βy ≈ 0.9·hₒ` singly-symmetric parameter (angle halved, 2L symmetric about the connected axis), tee/angle shape-factor plastic moduli (`1.7/0.85·S`, `1.5·S`, `1.6/1.5·S`) and torsion-only `Σb·t³/3`; CLOSED — HSS-rect shell plastic moduli, Bredt `J = 4·A_m²·t/p_m`, zero warping, `2·h·t`/`2·b·t` shear pair; round tube `Z = (D³−d³)/6`, polar `J = π/2·(r_o⁴−r_i⁴)`, `Av = A/2`; SOLID — `b·h²/4`, Roark solid-rectangle `J`. R4 stands verified: the VividOrange elastic integral yields ONLY `Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter` — no plastic/torsion/warping/shear/shear-centre source exists in the package, so the relocated algebra is LOAD-BEARING and no VividOrange warping source is to be invented. The exact-fillet `.Utility.Parts` `TrapezoidalPart`/`EllipseQuarterPart` integration survives on the solver's `Catalogued` arm (`new SectionProperties((IProfile)shape.Profile)`).
- [ADMISSION_AND_TOTALITY]: REALIZED — `SteelShape.Of` is the single point raw `VividOrange` `UnitsNet` is admitted: `ClassOf` pre-empts the hollow split by GEOMETRY (`ICircularHollow`/`IRoundedRectangularHollow`/`IRectangularHollow` — a round and a rectangular HSS share `AmericanShape.HSS`, verified `HSS13_375x_625 : ICircularHollow` vs `HSS8x8x_500 : IRoundedRectangularHollow`, and the ROUNDED contract does not extend the sharp one, so both rectangular arms are load-bearing — the prior sharp-only arm ABORTED every AISC rectangular HSS seed row at `DimsOf`) then dispatches the open families onto the TOTAL `SteelClass.OfShape` folds (13 American; the 25 EN families exhaust into i-shape/channel — EN 10365 publishes no European angle/hollow/tee family); `DimsOf` reads the family interface in the native published unit (`IIParallelFlange` with fillet before the `II` taper base, `IDoubleAngle` before `IAngle`, envelope + `IHollowStructuralSection.Thickness` for the hollows, the rounded-rect arm minting its corner radius from the `FlatWidth` delta) and lifts the four load-bearing dims ONCE into the `PositiveMagnitude` `SectionDims` (`WidthMm`/`DepthMm` doubling as the `Catalogued` gross pair, `WebMm`/`FlangeMm` the closed-form and B4.1 inputs, fillet/back-to-back the `>= 0` doubles with `BackToBackMm` crossing onto the Bim `ProfileDims.BackToBackMm` round-trip). An unmatched geometry interface rails `ComponentFault.Family`; band integers ride the `FaultBand.Component` registry row.
- [DESIGN_PROJECTION]: REALIZED — `SteelDesign` stays this page's owner over the RESOLVED canonical receipt: `Classify` is ONE Table B4.1 generator over the per-class `SlendernessRow` coefficient DATA (rolled I/tee half-outstand flange 0.38/1.00 vs the channel FULL flange, angle legs case-12 0.54/0.91, tee stems case-14 0.84/1.52, HSS-rect wall 1.12/1.40 plus its case-19 web 2.42/5.70, round `D/t` on the case-20 0.07/0.31·E/Fy reference form — the deleted roster ran the rolled-I model on every open class), the worse of the independent flange/web verdicts governing through `CompactnessClass.Worse`; `Capacity(shape, section, fy, Lb, KL)` the LRFD projection — `φMn` through the per-class `FlexureRegime` row: §F2 LTB reading the REAL `IwMm6` (`rts = √(√(Iy·Iw)/Sx)`, `Lr` from `J`/`Iw`, elastic `Fcr·Sx` beyond) for the I/channel classes BOUNDED by the §F3 flange-local resistance `FlangeLocalMn` (compact `M_p`; noncompact the F3-1 `M_p → 0.7·Fy·Sx` interpolation across `λpf..λrf`; slender the F3-2 elastic `0.9·E·kc·Sx/λ²`, `kc = 4/√(h/tw)` in `[0.35, 0.76]` — the B4.1 verdict is now a CAPACITY input, the deleted form carried it as an inert receipt label while crediting `M_p` to a slender flange), §F9 `M_cr = π·√(E·Iy·G·J)/L_b·(B+√(1+B²))` capped `min(M_p, 1.6·M_y)` for tees/double angles (the deleted F2-everywhere form credited a tee `1.7·M_y` where §F9 caps `1.6·M_y`), §F10 single-angle yield/LTB bands over `M_e = 0.46·E·b²t²/L_b`, and `M_p` for closed/solid per §F7/§F8 (LTB is not a closed-section limit state — the deleted form ran F2 on an HSS), `φMny` the §F6 weak-axis resistance `MinorMn` — `min(Fy·Zy, 1.6·Fy·Sy)` per F6-1 (the 1.5 cap on the F10 single-angle regime per F10-1; no minor-axis LTB limit state exists), the F2 I/channel classes bounded by the §F6.2 flange-local form over the SAME per-class `SlendernessRow` data `FlangeLocalMn` reads (noncompact F6-2 interpolation to `0.7·Fy·Sy`, slender F6-4 `0.69·E/λ²` on `Sy` — zero new coefficients), the receipt's Forms-supplement `ZyMm3`/`SyMm3` minor moduli the inputs; `φPn` over the receipt's WEAK-axis `GoverningRadiusMm`, `φVn` over the MAJOR-axis web `AvyMm2` (the seam `AvY`; the flange `AvzMm2` is the minor `AvZ`), `φTn` the §H3.1 `φT·0.6·Fy·(J/c)` closed-section resistance (open shapes 0 — §H3.3 warping torsion is not a single scalar, a torsion demand on an open shape surfaces as the governing over-ratio); the Composite arm the Ch I plastic couple capped at `ΣQn = StudClass.SteelShearKn × StudsPerMetre × ShearSpanMm` (the `joint#JOINT_FAMILY` per-stud Eq I8-1 cap summed over the §I3.2d shear span — the deleted per-metre rate against a total force was a dimensional mismatch) and FLOORED at the bare-steel `rolledMn` (`ΣQn = 0` degrades to non-composite, never to the deleted zero); the AISI stud modality the `Capacity(ColdFormedRow, ComputedSection, fy, KL)` OVERLOAD over the REAL solved `ColdFormedC` receipt — F3.1 `φb·Fy·(Seff/S)·Sx` (the minor arm the SAME F3.1 form on `Sy`, one published `Seff` proxy on both axes), E3.1 buckling on the same 0.658 curve, G2 web shear on `AvyMm2`, the published `SeffRatio < 1` the slender verdict (the deleted `Some`-detail arm multiplied the ratio into a C15 catalogue `Sx`). `DesignCapacity`'s shape is seam-frozen — the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Steel(DesignCapacity)` case lifts it WHOLE, `TorsionalNmm`/`FlexuralMinorNmm` onto `SectionCapacity.SteelLrfd.TorsionalKnm`/`FlexuralMinorKnm`; the stud's `ColdFormedRow` resolves through the dims-keyed `ColdFormedSections.Of(profile, key)`. The `ComputedSection` argument arrives from the resolution cache (`graph.SectionOf`), never re-solved per check.
- [EN_STEEL_GRADE_YIELD]: REALIZED — `SteelGrade` binds the EN bands to `EnSteelGrade` + the `EnSteelDeliveryCondition` whose Table 3.1 sub-table HOLDS the grade (AR/EN 10025-2: S235/S275/S355/S450; N/EN 10025-3: S420/S460 — the default AR spec throws on them, hence per-row `Delivery`), so `YieldMpa(thicknessMm, annex, key)` builds the `EnSteelMaterial`, routes `Specification.DeliveryCondition`, and reads the thickness-banded `f_y` (≤40 vs 40–80 mm, the section flange thickness the selector) from `EnSteelFactory.CreateLinearElastic` — registered DATA citing `En1993`, the derivation throw trapped onto `ComponentFault.Grade` (the grade slot, not the prior `Family` mislabel). The AISC/ASTM bands (A36/A992/A572, A653 SS Gr 50 for the cold-formed lane) stay spec-nominal. `[KeyMemberComparer]` is stacked beside `[KeyMemberEqualityComparer]` on `SteelGrade`/`SteelClass`/`SteelTopology` for ordered key lookup, matching the campaign `ComponentFamily`/`QuantityRow` owners; the trivial `IsComposite`/`IsColdFormed` forwarders are deleted (ONE_HOP).
- [IFC_STAMP_SPLIT]: REALIZED — the ELEMENT stamp is seed-computed `IfcBinding.Supertype(ComponentFamily.Steel.Class)` (`IfcBuiltElement` + `NOTDEFINED`): a W-shape serves as beam, column, or brace, so the concrete leaf (`IfcBeam`/`IfcColumn`) and the predefined role are occurrence/`Construction` refinements the Bim `AdmitPredefined` egress gate validates — never a cross-section property. The PROFILE subtype `SteelClass.IfcSubtype` rides the seam `MaterialComposition.ProfileSet` round-trip (`IfcMaterialProfileSet`), the Bim ingress `ProfileDefKind`/`ProfileDims` folding the parameterized members back onto `SectionDims` columns. Strings stay neutral here; the generated `Rasm.Bim` roster is the validation authority at composition (`IfcLegality`) and egress (`AdmitPredefined`).
- [FULL_DATABASE_SEED]: REALIZED — `SteelSeed.Seeds` enumerates the registered `American` and `European` identity domains through `CatalogueFactory`, and every row enters through `SteelShape.Of`. `GradeOf` derives the AISC grade-selection policy from the minted geometry family, while every EN identity selects `S355`; a stocked subset is a downstream filter over the complete fold.
- [FIRE_FACTS]: REALIZED — `SteelDesign` carries `SectionFactorPerM`, railed `RetentionAt(temperature, key)`, and `CriticalTemperatureC(utilisation, key)`. `FireRetention` interpolates the published EN 1993-1-2 Table 3.1 `ky,θ`/`kE,θ` rows after finite-temperature admission; `NaN` and infinities fault instead of collapsing to the ambient anchor. Exposure and time-temperature development remain placement inputs, so a fire re-check composes these facts onto the ambient capacity rail.
