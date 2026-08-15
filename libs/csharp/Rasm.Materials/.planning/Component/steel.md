# [MATERIALS_STEEL]

THE STEEL SEED FAMILY GROUNDED IN THE PUBLISHED SECTION DATABASE. `SteelSeed.Rows` folds the full registered AISC American and EN 10365 European domains through `SteelShape.Of` and `Component.Of`; each row carries one published `ICatalogue` identity, one policy-selected `SteelGrade`, one `SectionProfile.Catalogued` or admitted cold-formed profile, its section-map membership deriving from the profile's own topology. `SectionSolver.Solve` owns the twenty-column integral and open-section supplement, while `SteelDesign` owns the railed AISC/AISI capacity projection, composite augmentation, and EN 1993-1-2 fire facts over that receipt. `SteelClass` carries the profile taxonomy and the IFC subtype the seeded realization bag publishes, the `ComponentFamily.Steel.Ifc` concrete leaf keeps occurrence refinement outside Materials, and growth remains a registered catalogue member, policy row, or authored cold-formed row rather than a per-shape type.

## [01]-[INDEX]

- [02]-[STEEL_FAMILY]: the `SteelTopology` open/closed/solid axis, the `SteelClass` nine-row subtype axis with the TOTAL `OfShape(AmericanShape)`/`OfShape(EuropeanShape)` folds and the `IfcProfileDef` mapping, the `SteelGrade` registered-yield band over `EnSteelMaterial.TryCreateFromDesignition`/`EnSteelFactory` and the EN 10088 `StainlessBands` form-banded proof-cell registry, the `SteelJurisdiction` basis-keyed ladder-and-body table with the EN 1993-1-4 reduced-ε row, the `SectionDims` published-dims currency, the `SteelShape.Of` catalogue admission boundary (geometry-driven hollow split + family fold + `PositiveMagnitude` lift), the `CompositeDetail` augmentation and the generated `ColdFormedRow`/`ColdFormedSections` SSMA lattice feeding the parametric `SectionProfile.ColdFormedC` lane, the `CompactnessClass` + `SteelDesign` one polymorphic `Capacity` entry over the profile arm, grade band, and `capacity#SECTION_CAPACITY` `DesignBasis` — the AISC Table B4.1 and EN Table 5.2 classifications, the LRFD projection, the computed AISI effective width, and the EN 1993-1-1 §6.3.1/§6.3.2 `χ`/`χLT` partial-factor arm over the per-class imperfection factors `SteelClass` carries, the `FireRetention` EN 1993-1-2 Table 3.1 rows with the `SteelDesign` fire facts, and the fail-loud full-database `SteelSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Steel` policy row binds.

## [02]-[STEEL_FAMILY]

- Owner: `SteelTopology` the open/closed/solid discriminant; `SteelClass` the `IfcProfileDef` subtype axis folded onto the published taxonomies; `SteelGrade` the registered-yield band; `StainlessBands` the EN 10088 published proof-cell registry the stainless bands read; `SectionDims` the admitted published-dims currency; `SteelShape` the catalogued profile payload; `CompositeDetail` the composite augmentation row; `ColdFormedRow`/`ColdFormedSections` the generated SSMA designation lattice; `SteelRowSource` the closed profile-origin axis (rolled catalogue · published cold-formed row · fabricated build delegate); `CompactnessClass`/`DesignCapacity`/`SteelDesign` the basis-dispatched AISC 360 + AISI S100 + EN 1993-1-1 projection and railed EN 1993-1-2 fire operations; `SteelSeed` the catalogue fold.
- Cases: class {i-shape (W/M/S/HP + the EN H/I families, open) · u-shape (C/MC/UPE/PFC/UPN/U/CH, open) · l-shape (L, open) · double-angle (2L, open) · hss-rect (closed) · hss-round (round HSS + Pipe, closed) · tee (WT/MT/ST, open) · composite (AISC 360 Ch I, open core) · cold-formed (AISI S100, open)} × grade {A36/A992/A572 AISC spec-nominal · S235/S275/S355/S420/S450/S460 EN Table 3.1 registered · 1.4301/1.4307/1.4401/1.4404/1.4462 EN 10088 published stainless} × topology {open · closed · solid} — a section is one seed row over one published identity; the composite variant is the SAME row with a `Some CompositeDetail` and a reclassed `SteelClass` on its `Rolled` source arm, and the cold-formed stud is the SAME row on its `Formed` source arm — a parametric `ColdFormedC` profile over the published `ColdFormedRow`, never a catalogue impersonation and never a parallel owner.
- Entry: `SteelSeed.Rows(Context)` traverses the unified `SteelRowSeed` table. `SteelDesign.Capacity` admits the rolled, cold-formed-stud, or steel-deck modality on the shape of its typed input and resolves the REGISTERED yield from the grade band at the element thickness — the `CapacityPlacement` `DesignBasis` and `NationalAnnex` cross together, never a caller yield double and never a loose code flag. `SteelDesign.RetentionAt(double, Op) : Fin<(double Ky, double KE)>` rejects non-finite steel temperature before interpolating the EN 1993-1-2 table.
- Packages: VividOrange.Profiles.Catalogue (`CatalogueFactory.CreateAmerican`/`CreateEuropean`, `American`/`European` identities, `AmericanShape`/`EuropeanShape` families, the `II`/`IIParallelFlange`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`+`IHollowStructuralSection` geometry contracts with `IIParallelFlange.FilletRadius`/`IDoubleAngle.BackToBackDistance`; `.api/api-vividorange-profiles-catalogue.md`), VividOrange.Materials (`EnSteelMaterial`/`EnSteelFactory.CreateLinearElastic` the Table 3.1 `f_y` by grade × delivery × thickness band, `EnSteelDeliveryCondition`; the derivation throws trapped at the grade admission; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1993` the EN grades cite, `NationalAnnex`; `.api/api-vividorange-standards.md`), UnitsNet (`Length`/`Pressure` at the admission edge; `libs/csharp/.api/api-unitsnet.md`), Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`AcceptValidated`), Rasm.Element (project — `MaterialId`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER`; `capacity#SECTION_CAPACITY` `DesignBasis`/`SafetyFormat`/`CapacityPlacement` the jurisdiction currency the one `Capacity` entry dispatches on; `StudClass` the composite `ΣQn` reads is `joint#JOINT_FAMILY`'s, DEFINED in this parent namespace — no `.Joint` child namespace exists), Thinktecture.Runtime.Extensions (`[SmartEnum]`/`[SmartEnum<string>]` with `[KeyMemberEqualityComparer]` + `[KeyMemberComparer]` stacked for ordered key lookup), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Traverse`).
- Growth: the seed IS the registered database (the full `American` and `European` identity domains enumerate through `Enum.GetValues` — a stocked subset is a policy filter over the fold, never the hard bound); a new composite variant one `Augmented` row with its detail; a new cold-formed stud one designation triple the `ColdFormedSections` lattice already generates; a new fabricated section one `Augmented` row over a `SteelRowSource.Plated` build delegate (the profile arm and its `Forms` supplement already exist — the row is DATA); a new grade one `SteelGrade` row binding its `EnSteelGrade` + the delivery condition whose Table 3.1 sub-table holds it — or, stainless, its `StainlessBands` registry row whose two-sourced EN 10088 cells are the yield source; a new DESIGN CODE one `capacity#SECTION_CAPACITY` `DesignBasis` row with its resistance arm here (never a second receipt record and never a per-code `Capacity` overload); a new shape family one `SteelClass` row carrying its topology + `FlexureRegime` + `IfcProfileDef` subtype + the §6.3.1/§6.3.2 imperfection factors + `OfShape` arm, AND the compiler-forced `SectionProfile` arm and `SectionSolver.Solve`/`Forms` arm on `component#SECTION_SOLVER` (buildingSMART profile-schema cadence, never thing cadence) — never a per-section type, never a transcribed property literal, never a parallel section receipt.
- Boundary: `SteelShape.Of` is the BOUNDARY_ADMISSION point where raw `VividOrange` `UnitsNet` geometry is admitted EXACTLY ONCE — the published dims (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the quantity, `.Millimeters` owning the conversion) lift into the `PositiveMagnitude` `SectionDims` columns, an unmatched geometry interface rails `ComponentFault.Family` (never a fabricated sentinel), and the interior carries proven-positive SI scalars with no `UnitsNet` type in a signature; the hollow split is GEOMETRY-driven (`ICircularHollow` before `IRoundedRectangularHollow` before `IRectangularHollow` before the family folds — a round HSS and a rectangular HSS share `AmericanShape.HSS`, so the family enum cannot discriminate them, and the AISC rectangular HSS concretes implement the ROUNDED contract, which does not extend the sharp one) and `SteelClass.OfShape` is TOTAL over both published taxonomies (the EN 10365 families are exclusively i-shape and channel; an unrecognized family rails, never a silent `_ => IShape`); the SOLVE is `component#SECTION_SOLVER`'s — `SectionSolver.Solve` dispatches the `Catalogued` arm over `Shape.Profile` (the exact-fillet `.Utility.Parts` `TrapezoidalPart`/`EllipseQuarterPart` integral) and `Forms.FromCatalogue(Shape)` fills the eight derived columns from `Shape.Section` + `Shape.Class` per topology, so this page holds NO stiffness algebra and NO twenty-column lift (the `ThinWalled` sectorial and `Plastic` strip kernels live on the solver; the elastic solver computes ONLY `Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter`, and VividOrange publishes no plastic, torsion, warping, or shear source at all, so those kernels are load-bearing); the design yield is the registered `SteelGrade.YieldMpa(thicknessMm, annex, key)` DATA and `SteelDesign.Capacity` BINDS it rather than accepting it — the one `Capacity` entry reads the band selector off its own lowered `SectionDims` (a rolled shape's flange, a formed sheet's wall), so no caller can price an S355 shape at 235 MPa and no path bypasses the registered EN Table 3.1 read; an EN band carries its PUBLISHED DESIGNATION and `EnSteelMaterial.TryCreateFromDesignition` resolves grade and specification together — the designation suffix IS the delivery statement, so no second mapping can pair a grade with a sub-table that does not hold it — then reads the thickness-banded `f_y` from `EnSteelFactory.CreateLinearElastic`, the unparsed designation railing typed and the derivation throw trapped onto `ComponentFault.Grade`; the AISC/ASTM bands stay spec-nominal — reflection over the whole admitted VividOrange train at the locked versions finds no A36/A992/A500/A572/A653 type, member, or embedded string, `EnSteelGrade` S235–S460 being the train's only steel-grade surface, so the spec-nominal literal is the strongest form the ecosystem admits; a STAINLESS band resolves through the published EN 10088 `StainlessBands` registry cell at the product form its profile ORIGIN states (a catalogued identity the EN 10088-3 bar/section table, a formed sheet the -2 cold strip, a fabricated plate arm the -2 plate) under the `en1993-1-4` jurisdiction whose reduced ε carries the E/210000 term at the 200 GPa stainless design modulus — the γM set (1.10/1.10/1.25) rides the `capacity#SECTION_CAPACITY` `DesignBasis` row, and an absent registry cell rails typed rather than borrowing a neighbouring form; `SteelDesign` reads ONLY canonical `ComputedSection` columns (`Iw` for F2 LTB, the receipt's derived `GoverningRadiusMm` for the weak-axis buckling the real column design governs on, `Avy` the major-axis web shear matching the seam `AvY`, `J/c` the §H3.1 closed-section torsional constant `C`) — a re-minted dimension or a parallel `SteelBeamCheck` surface has no place here, and `DesignCapacity.TorsionalNmm`/`FlexuralMinorNmm` are the one source the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Steel` arm reads onto `SectionCapacity.SteelMember.TorsionalKnm`/`FlexuralMinorKnm`; the DESIGN CODE is `DesignCapacity.Basis` DATA rather than a per-code receipt type — the AISC 360 φ-format and EN 1993-1-1 partial-factor arms fill ONE column set and the capacity rail folds either through the basis's own interaction kernel, so an EN-seeded IPE/HE/UPN at S355 receives an EC3 verdict without a second receipt record, a second lift arm, or a sibling capacity case; the composite `ΣQn` reads `joint#JOINT_FAMILY` `StudClass.SteelShearKn(StudGroup) × StudsPerMetre × ShearSpanMm` (the one stud vocabulary summed over the AISC §I3.2d max-moment-to-zero-moment span), never a re-derived stud shear and never a per-metre rate against a total force; the element IFC stamp is the `ComponentFamily.Steel.Ifc` concrete leaf (role and placement are occurrence refinements the Bim egress gates; every IFC string here stays neutral, the generated `Rasm.Bim` roster holding validation authority at composition-time `IfcLegality` and egress-time `AdmitPredefined`) while `SteelClass.IfcSubtype` (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef`/`IfcRectangleProfileDef`/`IfcCircleProfileDef`, `IfcArbitraryClosedProfileDef` for `DoubleL`/`Composite`) reaches the Bim profile lane as the seeded `DetailSchema.ProfileSubtype` realization row — steel therefore carries `DetailLane.Realization`, because a family declaring no lane seeds no bag and that lane resolves every steel section to `None` however many subtype tokens this page publishes; BACK-TO-BACK publishes as its own realization row beside it, the double angle's spacing being a fabrication fact of the pair rather than a column on a section-geometry type; the AISI capacity data path is CLOSED in-page — `SteelDesign.FormOf` lowers the `ColdFormedC` and `Corrugated` arms straight onto `SectionDims` and the §B2 effective width computes from that geometry, so no reverse row lookup and no designation-string parse exists.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary (the PublishedSeff check-value table)
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
// closed-section M_p bound. Each open class names its OWN chapter because they disagree: §F9 caps a tee at 1.6·My
// where an F2 reading would credit its full plastic couple, and an angle leg has no F2 spelling at all. A new
// flexure chapter is one row plus its kernel arm.
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
    public static readonly SteelClass IShape      = new("i-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcIShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.34, ltbAlpha: 0.34);
    public static readonly SteelClass UShape      = new("u-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(1, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.76);
    public static readonly SteelClass LShape      = new("l-shape",      topology: SteelTopology.Open,   ifcSubtype: "IfcLShapeProfileDef",           regime: FlexureRegime.F10,     slenderness: Some(new SlendernessRow(1, 0.54, 0.91, false, 0.54, 0.91)), bucklingAlpha: 0.49, ltbAlpha: 0.76);
    public static readonly SteelClass DoubleAngle = new("double-angle", topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F9,      slenderness: Some(new SlendernessRow(1, 0.54, 0.91, false, 0.54, 0.91)), bucklingAlpha: 0.49, ltbAlpha: 0.76);
    public static readonly SteelClass HssRect     = new("hss-rect",     topology: SteelTopology.Closed, ifcSubtype: "IfcRectangleHollowProfileDef",  regime: FlexureRegime.Plastic, slenderness: Some(new SlendernessRow(1, 1.12, 1.40, true,  2.42, 5.70)), bucklingAlpha: 0.21, ltbAlpha: Option<double>.None);
    public static readonly SteelClass HssRound    = new("hss-round",    topology: SteelTopology.Closed, ifcSubtype: "IfcCircleHollowProfileDef",     regime: FlexureRegime.Plastic, slenderness: Some(new SlendernessRow(1, 0.07, 0.31, false, 0.07, 0.31)), bucklingAlpha: 0.21, ltbAlpha: Option<double>.None);
    public static readonly SteelClass Tee         = new("tee",          topology: SteelTopology.Open,   ifcSubtype: "IfcTShapeProfileDef",           regime: FlexureRegime.F9,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, false, 0.84, 1.52)), bucklingAlpha: 0.49, ltbAlpha: 0.76);
    public static readonly SteelClass Composite   = new("composite",    topology: SteelTopology.Open,   ifcSubtype: "IfcArbitraryClosedProfileDef",  regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(2, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.49);
    public static readonly SteelClass ColdFormed  = new("cold-formed",  topology: SteelTopology.Open,   ifcSubtype: "IfcUShapeProfileDef",           regime: FlexureRegime.F2,      slenderness: Some(new SlendernessRow(1, 0.38, 1.00, true,  3.76, 5.70)), bucklingAlpha: 0.49, ltbAlpha: 0.76);
    // Catalogued SOLID stock. A solid bar has no plate element, so it carries NO slenderness row rather than a
    // fabricated one — the width-to-thickness ladder has nothing to measure, which is exactly what §B4 means by
    // calling solid sections compact by definition. Absence here is what makes `SteelTopology.Solid` reachable
    // without any arm reading a limit that does not exist.
    public static readonly SteelClass SolidBar   = new("solid-bar",   topology: SteelTopology.Solid,  ifcSubtype: "IfcRectangleProfileDef",        regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None);
    public static readonly SteelClass SolidRound = new("solid-round", topology: SteelTopology.Solid,  ifcSubtype: "IfcCircleProfileDef",           regime: FlexureRegime.Plastic, slenderness: Option<SlendernessRow>.None, bucklingAlpha: 0.21, ltbAlpha: Option<double>.None);
    public SteelTopology Topology { get; }
    public string IfcSubtype { get; }
    public FlexureRegime Regime { get; }
    public Option<SlendernessRow> Slenderness { get; }

    // The EN 1993-1-1 §6.3.1 Table 6.1/6.2 imperfection factor α the buckling curve carries (a0 0.13 · a 0.21 ·
    // b 0.34 · c 0.49 · d 0.76), selected here per SHAPE because the estate's column check governs on the WEAK axis:
    // a rolled I answers curve b, a hot-finished hollow curve a, and the L/T/channel/welded/formed shapes curve c.
    public double BucklingAlpha { get; }

    // The §6.3.2.2 Table 6.4 lateral-torsional α_LT, ABSENT where the mode does not exist — a closed hollow section
    // has no lateral-torsional buckling, so the row states absence and the arm reads χ_LT = 1.0 rather than a zero
    // sentinel a divisor would then have to guard.
    public Option<double> LtbAlpha { get; }

    // The published AISC family taxonomy IS the discriminant — TOTAL, an unrecognized family rails ComponentFault.Family,
    // never a silent `_ => IShape` mis-classifying a tee/angle/hollow. HSS maps to the
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

// The EN 10088 product-form axis of a stainless proof cell: the -2 flat forms (C cold-rolled strip t ≤ 8, H
// hot-rolled strip t ≤ 13.5, P hot-rolled plate t ≤ 75) and the -3 solution-annealed bar/section d ≤ 160. The form
// is a PRODUCT fact the profile origin already states, which is what lets the capacity entry recover it from the
// value instead of a caller flag.
public enum StainlessForm : byte { ColdStrip = 0, HotStrip = 1, Plate = 2, Bar = 3 }

// One EN 10088-2/-3 grade's proof-stress cells (Rp0.2 min, MPa) per product form — every cell an Option because
// only what two independent sources print seeds (SEED_ROW_LAW, Published). ProofMpa is the ONE read: an absent
// cell rails typed rather than borrowing a neighbouring form, so a plate girder in a grade whose plate row is
// unpublished refuses instead of pricing on a bar cell.
public readonly record struct StainlessRow(
    string EnNumber, Option<double> ColdStripMpa, Option<double> HotStripMpa, Option<double> PlateMpa, Option<double> BarMpa) {

    public Option<double> Cell(StainlessForm form) => form switch {
        StainlessForm.ColdStrip => ColdStripMpa,
        StainlessForm.HotStrip  => HotStripMpa,
        StainlessForm.Plate     => PlateMpa,
        _                       => BarMpa,
    };

    public Fin<double> ProofMpa(StainlessForm form, Op key) =>
        Cell(form).ToFin(ComponentFault.Grade(key, $"<stainless-cell-unpublished:{EnNumber}:{form}>"));
}

// The EN 10088 registry: the standard-table print corroborated by an independent second producer datasheet fills a
// cell; a single-sourced cell stays None — the 1.4404 H/P flats (numerically equal to the two-sourced 1.4401
// 316-family block yet never captured under their own label), the 1.4301/1.4401 bar rows, the 1.4462 flat rows
// (the EN 1993-1-4 460 print is one source), and EVERY 1.4571 cell. 1.4571 therefore registers with no SteelGrade
// band at all: its substance id stands in the Properties catalogue, and its band is one SteelGrade row the moment
// a second source lands a cell.
public static class StainlessBands {
    public static readonly StainlessRow S14301 = new("1.4301", Some(230.0), Some(210.0), Some(210.0), None);
    public static readonly StainlessRow S14307 = new("1.4307", Some(220.0), Some(200.0), Some(200.0), Some(175.0));
    public static readonly StainlessRow S14401 = new("1.4401", Some(240.0), Some(220.0), Some(220.0), None);
    public static readonly StainlessRow S14404 = new("1.4404", Some(240.0), None,        None,        Some(200.0));
    public static readonly StainlessRow S14462 = new("1.4462", None,        None,        None,        Some(450.0));
    public static readonly StainlessRow S14571 = new("1.4571", None,        None,        None,        None);
}

// The structural-steel grade band: the EN bands bind their EnSteelGrade + the EnSteelDeliveryCondition whose EN 1993-1-1
// Table 3.1 sub-table HOLDS the grade (AR/EN 10025-2 holds S235/S275/S355/S450; N/EN 10025-3 and M/EN 10025-4 hold
// S420/S460 — the default AR spec rails them), so the design yield is registered DATA citing En1993; the AISC bands carry
// their spec-nominal (no admitted package carries any AISC/ASTM grade member or string). The STAINLESS bands bind a
// StainlessBands registry row instead of an EnDesignation — EnSteelMaterial parses carbon EN 10025 designations only —
// and the capacity entry routes them through the row's own form-banded cell, so NominalYieldMpa on a stainless band is
// the row's least two-sourced cell — a conservative floor no design path consults. SubstanceId is the per-grade Mechanical row the design
// seam reads; the render AppearanceId rides the seed (the two-slot independence law).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelGrade {
    public static readonly SteelGrade A36  = new("a36",  nominalYieldMpa: 250.0, substanceId: "steel.a36");
    public static readonly SteelGrade A992 = new("a992", nominalYieldMpa: 345.0, substanceId: "steel.a992");
    public static readonly SteelGrade A572 = new("a572", nominalYieldMpa: 345.0, substanceId: "steel.a572");
    public static readonly SteelGrade A653Gr33 = new("a653-gr33", nominalYieldMpa: 230.0, substanceId: "steel.g33");   // ASTM A653 SS Gr 33 — the LIGHT cold-formed sheet band (22/20/18/16 ga)
    public static readonly SteelGrade A653Gr50 = new("a653-gr50", nominalYieldMpa: 340.0, substanceId: "steel.g50");   // ASTM A653 SS Gr 50 — the STRUCTURAL band the AISI stud lane and the heavy deck gauges roll
    public static readonly SteelGrade A500Rect  = new("a500-grc-rect",  nominalYieldMpa: 345.0, substanceId: "steel.a500-rect");    // ASTM A500 Gr C rectangular HSS
    public static readonly SteelGrade A500Round = new("a500-grc-round", nominalYieldMpa: 317.0, substanceId: "steel.a500-round");   // ASTM A500 Gr C round HSS — a DISTINCT substance, because the reverse grade read keys on it
    public static readonly SteelGrade A53  = new("a53-grb", nominalYieldMpa: 240.0, substanceId: "steel.a53");   // ASTM A53 Gr B pipe
    public static readonly SteelGrade S235 = new("s235", nominalYieldMpa: 235.0, substanceId: "steel.s235", enDesignation: Some("S235"));
    public static readonly SteelGrade S275 = new("s275", nominalYieldMpa: 275.0, substanceId: "steel.s275", enDesignation: Some("S275"));
    public static readonly SteelGrade S355 = new("s355", nominalYieldMpa: 355.0, substanceId: "steel.s355", enDesignation: Some("S355"));
    public static readonly SteelGrade S420 = new("s420", nominalYieldMpa: 420.0, substanceId: "steel.s420", enDesignation: Some("S420N"));
    public static readonly SteelGrade S450 = new("s450", nominalYieldMpa: 440.0, substanceId: "steel.s450", enDesignation: Some("S450"));
    public static readonly SteelGrade S460 = new("s460", nominalYieldMpa: 460.0, substanceId: "steel.s460", enDesignation: Some("S460N"));
    public static readonly SteelGrade Ss14301 = new("ss1-4301", nominalYieldMpa: 210.0, substanceId: "steel.1.4301", stainless: Some(StainlessBands.S14301));
    public static readonly SteelGrade Ss14307 = new("ss1-4307", nominalYieldMpa: 175.0, substanceId: "steel.1.4307", stainless: Some(StainlessBands.S14307));
    public static readonly SteelGrade Ss14401 = new("ss1-4401", nominalYieldMpa: 220.0, substanceId: "steel.1.4401", stainless: Some(StainlessBands.S14401));
    public static readonly SteelGrade Ss14404 = new("ss1-4404", nominalYieldMpa: 200.0, substanceId: "steel.1.4404", stainless: Some(StainlessBands.S14404));
    public static readonly SteelGrade Ss14462 = new("ss1-4462", nominalYieldMpa: 450.0, substanceId: "steel.1.4462", stainless: Some(StainlessBands.S14462));
    public double NominalYieldMpa { get; }
    public string SubstanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);

    // The published EN 10088 registry row of a stainless band — the yield SOURCE the capacity entry routes through
    // at the profile-origin product form. Mutually exclusive with EnDesignation by construction of the roster: a
    // band's yield has exactly one producer.
    public Option<StainlessRow> Stainless { get; }

    // The EN grade as its PUBLISHED DESIGNATION rather than a grade enum plus a hand-set delivery condition. The
    // designation IS the delivery statement — `S420N` names the normalized sub-table, `S355` the as-rolled one — so
    // the package's own parser resolves grade AND specification together, and this page carries no second mapping
    // that could pair a grade with the sub-table that does not hold it.
    public Option<string> EnDesignation { get; }

    // The thickness-banded design yield. `TryCreateFromDesignition` is the package's ONE non-throwing constructor, so
    // an unparseable designation is a TYPED refusal rather than a caught exception, and the trap covers only the
    // derivation, which does throw on a missing national annex or an invalid specification. The section's own element
    // thickness selects the published band; an AISC band returns its spec-nominal because no admitted package owns
    // the AISC grade table.
    public Fin<double> YieldMpa(double elementThicknessMm, NationalAnnex annex, Op key) =>
        EnDesignation.Match(
            Some: designation => EnSteelMaterial.TryCreateFromDesignition(designation, annex, out EnSteelMaterial material)
                ? Try.lift(() => EnSteelFactory.CreateLinearElastic(material, Length.FromMillimeters(elementThicknessMm)).Strength.Megapascals).Run()
                    .MapFail(e => ComponentFault.Grade(key, $"<en-steel-derivation:{designation}:{annex}:{e.Message}>"))
                : Fin.Fail<double>(ComponentFault.Grade(key, $"<en-steel-designation-unparsed:{designation}:{annex}>")),
            None: () => Fin.Succ(NominalYieldMpa));
}

// The steel family's JURISDICTION table: which slenderness ladder and which resistance body each DESIGN BASIS serves.
// Keying on the basis KEY rather than on its SafetyFormat is the `DesignBasis` ruling's own law — a safety format is
// a property a code HAS, not its identity, and two codes sharing one format do not share a classification ladder. An
// unserved (code, family) pair reports NOT-APPLICABLE here rather than falling into whichever arm the format happened
// to select, which is what lets a steel section checked under a masonry or timber basis refuse instead of silently
// receiving a verdict from a code that never covered it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelJurisdiction {
    public static readonly SteelJurisdiction Aisc360  = new("aisc360",   ladder: SteelLadder.Aisc,     body: SteelBody.Aisc);
    public static readonly SteelJurisdiction AisiS100 = new("aisi-s100", ladder: SteelLadder.Computed, body: SteelBody.Aisc);
    public static readonly SteelJurisdiction En1993   = new("en1993",    ladder: SteelLadder.Eurocode, body: SteelBody.Eurocode);
    public static readonly SteelJurisdiction En1994   = new("en1994",    ladder: SteelLadder.Eurocode, body: SteelBody.Eurocode);
    // EN 1993-1-4: the STAINLESS jurisdiction rides the shared Eurocode ladder and body under its own reduced ε —
    // ε² = (235/f_y)·(E/210000) with the 200 GPa stainless design modulus, so every Table 5.2 limit tightens by the
    // modulus ratio and nothing else on the arm re-derives. Its γM set (γM0 = γM1 = 1.10, γM2 = 1.25) is the
    // capacity#SECTION_CAPACITY `en1993-1-4` DesignBasis row the Eurocode arm divides by.
    public static readonly SteelJurisdiction En1993Stainless = new("en1993-1-4", ladder: SteelLadder.Eurocode, body: SteelBody.Eurocode, epsilonModulusRatio: 200_000.0 / 210_000.0);
    public SteelLadder Ladder { get; }
    public SteelBody Body { get; }

    // The E/210000 term of the jurisdiction's OWN ε basis — 1.0 on the carbon rows (EN 1993-1-1 ε = √(235/f_y)),
    // the modulus ratio on the stainless row; a column, never a second classifier.
    public double EpsilonModulusRatio { get; }

    public static Fin<SteelJurisdiction> Of(DesignBasis basis, Op key) =>
        TryGet(basis.Key, out SteelJurisdiction? row) && row is { } served
            ? Fin.Succ(served)
            : ComponentFault.Capacity(key, $"<steel-basis-not-applicable:{basis.Key}>");
}

// The slenderness ladder a jurisdiction runs: the AISC Table B4.1 per-class rows, the EN 1993-1-1 Table 5.2
// element-role limits, or COMPUTED — the verdict the effective-width fold itself returns, which is what a
// post-buckling code publishes in place of a width-to-thickness table.
public enum SteelLadder : byte { Aisc = 0, Eurocode = 1, Computed = 2 }

// The resistance body a jurisdiction applies once a section's effective geometry is known. The FORMED lane is NOT a
// body — it is a GEOMETRY step every body consumes, which is why the profile's own topology selects it and a basis
// never can.
public enum SteelBody : byte { Aisc = 0, Eurocode = 1 }

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
    SteelGrade Grade, ComponentStandard Standard, string Catalogue,
    Option<CompositeDetail> Composite = default) {

    // The ONE catalogue admission boundary: geometry-driven class resolution, the family-interface dims read, and the
    // PositiveMagnitude lift — raw UnitsNet admitted exactly once; the interior never sees a quantity or a sentinel.
    public static Fin<SteelShape> Of(ICatalogue catalogue, SteelGrade grade, ComponentStandard standard, Op key) =>
        from outline in Outline(catalogue, key)
        from cls in ClassOf(catalogue, key)
        from dims in DimsOf(catalogue, key)
        select new SteelShape(catalogue.Label, cls, outline, dims, grade, standard, $"{catalogue.Catalogue}");

    // The published identity implements the geometry floor by contract, but the cast is RAILED: a catalogue member
    // that does not is a package-surface fact this boundary reads and refuses, never an InvalidCastException thrown
    // out of the middle of a seed traverse where no Op names the row that failed.
    static Fin<IProfile> Outline(ICatalogue catalogue, Op key) =>
        catalogue is IProfile profile
            ? Fin.Succ(profile)
            : ComponentFault.Family(key, $"<catalogue-not-a-profile:{catalogue.Label}>");

    // Geometry pre-empts the family fold: a round HSS and a rectangular HSS carry the SAME AmericanShape.HSS (verified:
    // HSS13_375x_625 is ICircularHollow, HSS8x8x_500 is IRoundedRectangularHollow — the ROUNDED contract does NOT
    // extend IRectangularHollow, so BOTH rectangular arms are load-bearing); the open families dispatch onto the
    // TOTAL OfShape folds.
    static Fin<SteelClass> ClassOf(ICatalogue catalogue, Op key) => catalogue switch {
        ICircularHollow            => Fin.Succ(SteelClass.HssRound),
        // The solid arms sit BELOW every hollow contract for the same reason the dims read does.
        ICircle when catalogue is not IHollowStructuralSection    => Fin.Succ(SteelClass.SolidRound),
        IRectangle when catalogue is not IHollowStructuralSection => Fin.Succ(SteelClass.SolidBar),
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
            // Catalogued SOLID stock, matched AFTER every hollow arm so a wall-bearing contract never falls here: a
            // bar has no wall, so its own dimensions ARE its thickness columns and `SteelTopology.Solid` becomes a
            // reachable member instead of a class no admitted identity can carry.
            ICircle c                                                                 => Fin.Succ((c.Diameter.Millimeters, c.Diameter.Millimeters, c.Diameter.Millimeters, c.Diameter.Millimeters, 0.0, 0.0)),
            IRectangle rect                                                           => Fin.Succ((rect.Height.Millimeters, rect.Width.Millimeters, rect.Width.Millimeters, rect.Height.Millimeters, 0.0, 0.0)),
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
// shear, and never a per-metre rate against a total force, which is a dimensional mismatch).
// The StudGroup is a PLACEMENT fact of the deck-and-position the studs are welded into, and AISC Eq I8-1 reads it
// directly: Rg falls to 0.85 at two studs per rib and Rp to 0.60 the moment a stud sits weak-position, so a couple
// priced without it over-states a three-per-rib weak-position connector by more than half.
public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    StudGroup Group,
    int StudsPerMetre,
    PositiveMagnitude ShearSpanMm);

// The design-resistance receipt over the canonical ComputedSection, BASIS-TAGGED: one column set carries the AISC 360
// φ-format resistances and the EN 1993-1-1 γM-format ones, and Basis names which — so an IPE at S355 receives an EC3
// verdict and a W at A992 an AISC one through the SAME receipt shape, never a parallel EN record. FlexuralMinorNmm is
// the §F6 weak-axis φMny = φb·min(Fy·Zy, 1.6·Fy·Sy) bounded by F6.2 flange local buckling (the F10 single-angle
// regime caps 1.5·Fy·Sy) or the §6.2.5 W_z·f_y/γM0. TorsionalNmm is the §H3.1 φT·Fcr·C or the §6.2.7 W_t·f_y/(√3·γM0)
// — positive for a CLOSED HSS/pipe (C = J/c off the carried JMm4), engineering-zero for an OPEN shape whose warping
// torsion is not a single-resistance scalar, so a torsion demand on an open shape surfaces as the governing
// over-ratio. Chi/ChiLt are the §6.3.1 flexural-buckling and §6.3.2 lateral-torsional reduction factors, 1.0 on the
// φ-format arms because AISC folds buckling INTO Fcr and Mn rather than publishing it beside them — the columns state
// each format's own premise, and a design report reads the EN reductions it must cite. These are the columns
// capacity#SECTION_CAPACITY SectionCapacity.Lift(CapacityReceipt) CapacityReceipt.Steel reads onto
// SectionCapacity.SteelMember — capacity columns, never re-passed lift arguments.
public readonly record struct DesignCapacity(
    DesignBasis Basis, double FlexuralNmm, double FlexuralMinorNmm, double CompressionN, double ShearN,
    double TorsionalNmm, CompactnessClass Classification, double Slenderness, double Chi, double ChiLt);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The AISC 360 + AISI S100 design projections over the CANONICAL ComputedSection the resolution cache supplies
// (graph.SectionOf / the catalogue section map — the solve ran once at catalogue build): classification reads the
// shape's admitted dims, capacity reads the receipt's real Iw (LTB), GoverningRadiusMm (weak-axis buckling), Avy
// (major-axis web shear), J/c (§H3.1 C); the AISI stud modality is the ColdFormedRow overload of the SAME Capacity.
// Lifted into capacity#SECTION_CAPACITY through SectionCapacity.Lift(CapacityReceipt) on its CapacityReceipt.Steel case.
public static class SteelDesign {
    const double φb = 0.90, φc = 0.90, φv = 0.90;
    // The elastic constants are the family's ONE spelling — internal because the generated ColdFormedRow lattice
    // reads E for its §B4.2 stiffener slenderness, and one spelling is what keeps the two in step. The 200 GPa
    // value is ALSO the EN 1993-1-4 stainless design modulus, so the stainless bands ride the same constant and
    // the reduced-ε basis carries the carbon 210 GPa reference only inside the jurisdiction's own ratio column.
    internal const double E = 200_000.0, G = 77_200.0;

    // The §E3 flexural-buckling column both design lanes drive: the effective slenderness over the receipt's own
    // WEAK-axis governing radius and the 0.658/0.877 critical stress it selects. One body, two arms — the rolled
    // arm scales the gross area, the cold-formed arm the §B2 effective area.
    static (double Slenderness, double Fcr) Column(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double λc = effectiveLengthMm / s.GoverningRadiusMm;
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        return (λc, Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe);
    }

    // THE ONE classifier, dispatched on the JURISDICTION the basis names — never on its safety format. A format is
    // a property a code has; the ladder is the code's own. `Computed` is the post-buckling verdict the effective-width
    // fold returns, so a code that publishes no width-to-thickness table states that rather than borrowing one.
    public static Fin<CompactnessClass> Classify(SteelClass cls, SectionDims d, double yieldMpa, SteelJurisdiction jurisdiction) =>
        jurisdiction.Ladder switch {
            SteelLadder.Aisc     => Fin.Succ(ClassifyAisc(cls, d, yieldMpa)),
            SteelLadder.Eurocode => Fin.Succ(ClassifyEn(cls, d, yieldMpa, jurisdiction.EpsilonModulusRatio)),
            _                    => Fin.Succ(CompactnessClass.Compact),   // the formed lane overwrites this with its own computed verdict
        };

    // EN 1993-1-1 Table 5.2 over ε = √(235/f_y): an INTERNAL compression part (the web in bending) is Class 1-2 at
    // c/t ≤ 72ε and Class 3 at ≤ 124ε; an OUTSTAND flange in compression is Class 1-2 at ≤ 9ε and Class 3 at ≤ 14ε.
    // The ELEMENT ROLE decides the limit pair, so this classifier needs no per-shape row where the B4.1 ladder needs
    // twenty; the WORSE element governs and the verdict lands on the shared CompactnessClass vocabulary (Class 1-2 →
    // Compact, Class 3 → Noncompact, Class 4 → Slender) so ONE verdict column serves both bases and the capacity rail
    // reads one spelling.
    const double EnWebCompact = 72.0, EnWebSemiCompact = 124.0, EnFlangeCompact = 9.0, EnFlangeSemiCompact = 14.0;

    // The ELEMENT ROLE decides the limit pair, and the element GEOMETRY is the class's own Table B4.1 row: a
    // channel flange is a full outstand where a rolled-I flange is a half one, and a tee stem and an angle leg read
    // the full depth where an I web deducts both flanges. Reading one I-shape model for every class hands an HSS wall
    // and a channel flange the half-outstand divisor, which under-states their slenderness by a factor of two.
    static CompactnessClass ClassifyEn(SteelClass cls, SectionDims d, double yieldMpa, double modulusRatio) => cls.Slenderness.Match(
        Some: row => EnVerdict(row, d, yieldMpa, modulusRatio),
        None: static () => CompactnessClass.Compact);

    // ε² carries the jurisdiction's own E/210000 term — 1.0 carbon, the 200/210 stainless ratio — so the ONE
    // Table 5.2 generator serves both codes and the stainless limits tighten by exactly the published basis.
    static CompactnessClass EnVerdict(SlendernessRow row, SectionDims d, double yieldMpa, double modulusRatio) {
        double e = Math.Sqrt(235.0 / yieldMpa * modulusRatio);
        double flange = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value);
        double web = (row.WebClear ? Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, 0.0) : d.DepthMm.Value) / d.WebMm.Value;
        return Verdict(flange, EnFlangeCompact * e, EnFlangeSemiCompact * e)
            .Worse(Verdict(web, EnWebCompact * e, EnWebSemiCompact * e));
    }

    // ONE Table B4.1 generator over the per-class SlendernessRow — running the rolled-I model on every open class halves a
    // channel's FULL flange width and reads a tee stem and an angle leg against case-15 web limits neither has. Flange and web λ verdict independently against λp/λr in the class's reference form (√(E/Fy); the
    // HssRound row rides the case-20 E/Fy form), the WORSE verdict governs; solid bar stock is compact by definition.
    // HSS-rect flange b/t reads the full width over the wall — conservative against the B−3t flat-width allowance
    // (the admitted corner radius on FilletMm refines the geometric owner; this classifier keeps the conservative bound).
    static CompactnessClass ClassifyAisc(SteelClass cls, SectionDims d, double yieldMpa) => cls.Slenderness.Match(
        Some: row => {
            double r = cls == SteelClass.HssRound ? E / yieldMpa : Math.Sqrt(E / yieldMpa);
            double flange = d.WidthMm.Value / (row.FlangeDivisor * d.FlangeMm.Value);
            double web = (row.WebClear ? Math.Max(d.DepthMm.Value - 2.0 * d.FlangeMm.Value, 0.0) : d.DepthMm.Value) / d.WebMm.Value;
            return Verdict(flange, row.FlangeLambdaP * r, row.FlangeLambdaR * r).Worse(Verdict(web, row.WebLambdaP * r, row.WebLambdaR * r));
        },
        None: static () => CompactnessClass.Compact);   // solid stock: no plate element to limit

    static CompactnessClass Verdict(double ratio, double λp, double λr) =>
        ratio > λr ? CompactnessClass.Slender : ratio <= λp ? CompactnessClass.Compact : CompactnessClass.Noncompact;

    // Chapters F/E/G LRFD over the resolved receipt: φMn through the per-class FlexureRegime row — F2 LTB reading the
    // REAL IwMm6 bounded by the §F3 flange-local resistance (the B4.1 verdict is a CAPACITY input, never a side label),
    // F9 tee/double-angle Mcr capped 1.6·My, F10 single-angle bands, closed/solid Mp per §F7/§F8; φPn = φc·Fcr·A over the receipt's weak-axis GoverningRadiusMm
    // (never the strong-axis Math.Sqrt(Ix/A) approximation); φVn = φv·0.6·Fy·Avy over the MAJOR-axis web (the seam AvY,
    // NOT the minor flange AvzMm2); the Composite arm the Ch I plastic couple capped at ΣQn and FLOORED at the bare-steel
    // Mn — the steel section alone always carries its rolled resistance, so ΣQn = 0 degrades to non-composite rather than to zero.
    // THE ONE ENTRY, discriminating on the SectionProfile arm and the SteelGrade band — discriminating on the SOURCE payload
    // instead leaves every FABRICATED arm — a plated girder, a Zed purlin, a built-up chord — with no design
    // projection at all, because a section the seed admits and the solver prices would match no overload. FormOf lowers every arm onto the same (SteelClass, SectionDims) pair, so a plate
    // girder classifies through the identical Table B4.1 row a rolled W does; the cold-formed lane is selected by
    // TOPOLOGY of the arm (a lipped C, a ribbed deck), because post-buckling effective width — not B4.1 — is the
    // design premise for a formed sheet.
    public static Fin<DesignCapacity> Capacity(SectionProfile profile, SteelGrade grade, ComputedSection s, CapacityPlacement placement, Op key) =>
        from lengths in guard(
            double.IsFinite(placement.UnbracedLengthMm + placement.EffectiveLengthMm)
                && placement.UnbracedLengthMm >= 0.0 && placement.EffectiveLengthMm > 0.0,
            ComponentFault.Capacity(key, $"<steel-design-input-rejected:{placement.UnbracedLengthMm:R}:{placement.EffectiveLengthMm:R}>"))
        from jurisdiction in SteelJurisdiction.Of(placement.Basis, key)
        from form in FormOf(profile, key)
        // The band selector is the element the grade table bands on: a rolled shape's FLANGE, a formed sheet's WALL —
        // one column read off the lowered dims, never a caller-supplied thickness. A STAINLESS band routes through
        // its registry row at the profile-origin product form instead — the form is recovered from the value, so no
        // caller can price a plate girder on a bar cell and an unpublished cell refuses typed.
        from yieldMpa in grade.Stainless.Match(
            Some: row => row.ProofMpa(StainlessFormOf(profile), key),
            None: () => grade.YieldMpa(
                Formed(form.Class) ? form.Dims.WebMm.Value : form.Dims.FlangeMm.Value, placement.Annex, key))
        from classification in Classify(form.Class, form.Dims, yieldMpa, jurisdiction)
        // TOPOLOGY picks the effective-SECTION step and the JURISDICTION picks the resistance body — two independent
        // axes, dispatched independently. A formed sheet's section is post-buckling EFFECTIVE under every code that
        // covers it, so routing it by basis let a partial-factor project silently price a stud on its GROSS section
        // and skip effective-width entirely; the fold now runs first and the body applies its own φ or γM to what it
        // returns.
        select Formed(form.Class)
            ? FormedSection(placement.Basis, form.Dims, s, yieldMpa, placement.EffectiveLengthMm)
            : jurisdiction.Body == SteelBody.Eurocode
                ? Eurocode(placement.Basis, form.Class, form.Dims, form.Composite, s, yieldMpa, classification, placement.UnbracedLengthMm, placement.EffectiveLengthMm)
                : Rolled(placement.Basis, form.Class, form.Dims, form.Composite, s, yieldMpa, classification, placement.UnbracedLengthMm, placement.EffectiveLengthMm);

    static bool Formed(SteelClass cls) => cls == SteelClass.ColdFormed;

    // The EN 10088 product form recovered from the profile ORIGIN: a catalogued identity is a hot-rolled long
    // product (the -3 bar/section table), a formed sheet is cold strip (-2 C), and every fabricated arm is
    // hot-rolled plate (-2 P) — a product fact the arm already states, never a caller flag.
    static StainlessForm StainlessFormOf(SectionProfile profile) => profile switch {
        SectionProfile.Catalogued => StainlessForm.Bar,
        SectionProfile.ColdFormedC or SectionProfile.Zed or SectionProfile.Corrugated => StainlessForm.ColdStrip,
        _ => StainlessForm.Plate,
    };

    // The ONE safety-format application: a resistance-factor code MULTIPLIES a nominal by φ, a partial-factor code
    // DIVIDES a characteristic by γM. Every lane states its nominal and hands it here, so one lane serves both
    // formats and no geometry step is skipped because a basis selected a different body.
    static double Resist(DesignBasis basis, double nominal, double phi, double gamma) =>
        basis.Format == SafetyFormat.LimitState ? nominal / gamma : phi * nominal;

    // The ONE profile -> (class, dims) lowering every design arm reads. A Catalogued profile hands back the state
    // SteelShape.Of already admitted; each FABRICATED arm maps its OWN named dimensions onto the same six SectionDims
    // columns under the SteelClass whose Table B4.1 row governs it — so the classifier, the flange-local bound, and
    // the minor-axis fold read one shape of input regardless of origin. An arm the steel family does not admit rails
    // rather than defaulting to an I-shape model.
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
        // is a repeat distance spanning flats and inclined webs alike, so feeding it to the flange column measured
        // effective width over a dimension no plate has.
        SectionProfile.Corrugated deck     => Dims(SteelClass.ColdFormed, deck.RibDepthMm, deck.TopFlatMm, deck.GaugeMm, deck.GaugeMm, 0.0),
        SectionProfile.RectangleHollow rh  => Dims(SteelClass.HssRect, rh.DepthMm, rh.WidthMm, rh.WallMm, rh.WallMm, rh.InnerFilletMm),
        SectionProfile.CircleHollow ch     => Dims(SteelClass.HssRound, ch.DiameterMm, ch.DiameterMm, ch.WallMm, ch.WallMm, 0.0),
        // A rolled-corner bar and a positioned built-up composition are SOLID for classification: the receipt already
        // carries their real integrated columns, and no thin-walled element exists to slenderness-verdict.
        SectionProfile.RoundedRectangle rr => Dims(SteelClass.HssRect, rr.DepthMm, rr.WidthMm, rr.WidthMm, rr.DepthMm, rr.RoundingMm),
        // A positioned built-up composition is a FABRICATED member, not a composite one: `SteelClass.Composite`
        // names the AISC Chapter I steel-and-concrete regime and carries its slab-couple flexure row, so classing a
        // battened column there advertises a concrete couple no such member has. It classes as its own plated
        // I-family row, and its already-integrated receipt carries the real columns.
        SectionProfile.BuiltUp b           => Dims(SteelClass.IShape, b.GrossRectangleMm.DepthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.WidthMm, b.GrossRectangleMm.DepthMm, 0.0),
        _ => Fin.Fail<(SteelClass, SectionDims, Option<CompositeDetail>)>(ComponentFault.Family(key, $"<steel-profile-arm-unpriced:{profile.Case}>")),
    };

    static Fin<(SteelClass Class, SectionDims Dims, Option<CompositeDetail> Composite)> Dims(SteelClass cls, PositiveMagnitude depth, PositiveMagnitude width, PositiveMagnitude web, PositiveMagnitude flange, double fillet) =>
        Fin.Succ((cls, new SectionDims(depth, width, web, flange, fillet, 0.0), Option<CompositeDetail>.None));

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
    static PositiveMagnitude Thinner(PositiveMagnitude a, PositiveMagnitude b) => a.Value <= b.Value ? a : b;

    // The projection body over the ADMITTED yield — the registered Table 3.1 value the grade band resolved at the
    // section's own flange thickness, never a caller double that could price an S355 shape at 235 MPa. A composite
    // augmentation rides the resolved Catalogued shape, so the couple enters through the SAME profile the class came
    // from and needs no second discriminant.
    static DesignCapacity Rolled(DesignBasis basis, SteelClass cls, SectionDims d, Option<CompositeDetail> composite, ComputedSection s, double yieldMpa, CompactnessClass classification, double unbracedLengthMm, double effectiveLengthMm) {
        (double λc, double Fcr) = Column(s, yieldMpa, effectiveLengthMm);
        // §E7: a SLENDER-element section buckles on its EFFECTIVE area, never its gross. The reduction is the same
        // Winter post-buckling fold the formed lane runs — §E7 and AISI §B2 are one physics under two names —
        // evaluated at the critical stress the column actually reached. Pricing φPn on the gross area of a section
        // the classifier just called slender credits material the code has already declared ineffective.
        double effective = classification == CompactnessClass.Slender ? EffectiveAreaRatio(d, Fcr) : 1.0;
        double Mp = yieldMpa * s.ZxMm3.Value;
        double rolledMn = cls.Regime.Switch(
            state: (Class: cls, Dims: d, Section: s, Lb: unbracedLengthMm, Fy: yieldMpa, Mp),
            f2:      static x => Math.Min(LateralTorsionalMn(x.Dims, x.Section, x.Lb, x.Fy, x.Mp), FlangeLocalMn(x.Class, x.Dims, x.Section, x.Fy, x.Mp)),
            f9:      static x => TeeMn(x.Section, x.Lb, x.Fy, x.Mp),
            f10:     static x => AngleMn(x.Dims, x.Section, x.Lb, x.Fy),
            plastic: static x => x.Mp);
        double Mn = composite.Match(
            Some: c => Math.Max(CompositeMn(c, s, yieldMpa), rolledMn),
            None: () => rolledMn);
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: φb * Mn,
            FlexuralMinorNmm: φb * MinorMn(cls, d, s, yieldMpa),
            CompressionN: φc * Fcr * s.AreaMm2.Value * effective,
            ShearN: φv * 0.6 * yieldMpa * s.AvyMm2.Value,
            TorsionalNmm: TorsionalResistance(cls, s, yieldMpa),
            Classification: classification,
            Slenderness: λc,
            Chi: 1.0,
            ChiLt: 1.0);
    }

    // THE EN 1993-1-1 ARM — the partial-factor twin of Rolled, and the reason the estate's EN-seeded catalogue half
    // (IPE/HE/UPN at S355) receives an EC3 verdict instead of an AISC one. §6.3.1 flexural buckling reduces the
    // cross-section resistance by χ over the class's OWN Table 6.1 imperfection α at the non-dimensional slenderness
    // λ̄ = √(A·f_y/N_cr); §6.3.2 lateral-torsional reduces the bending resistance by χ_LT over the warping-free M_cr
    // the receipt's real Izz/J supply, and a CLOSED section has no such mode, so its absent α reads χ_LT = 1.0.
    // §6.2.5 minor bending, §6.2.6 shear over the SAME major-axis web area the AISC arm reads (Av·f_y/(√3·γM0), the
    // von-Mises shear yield the code states rather than the AISC 0.6 factor), §6.2.7 torsion over the closed-section
    // torsional modulus — cross-section resistances at γM0, the two buckling resistances at γM1, exactly as §6.1
    // partitions them. The classification is the §5.5 Table 5.2 verdict the shared Classify routes, and its Class-3/4
    // outcome drives the ELASTIC modulus where Class 1-2 takes the plastic — the one place a compactness verdict is a
    // capacity INPUT on this basis, mirroring the F3 flange-local bound on the AISC arm. The composite couple rides
    // the EN 1994-1-1 §6.7.3.2 plastic bound the shared CompositeMn computes, floored at the bare-steel resistance so
    // ΣQn = 0 degrades to non-composite.
    static DesignCapacity Eurocode(DesignBasis basis, SteelClass cls, SectionDims d, Option<CompositeDetail> composite, ComputedSection s, double yieldMpa, CompactnessClass classification, double unbracedLengthMm, double effectiveLengthMm) {
        // §5.5.2: a Class 4 cross-section is verified on its EFFECTIVE section, exactly as §E7 does on the other
        // basis, so the same post-buckling reduction scales both the moduli and the area rather than the class
        // verdict lowering only the modulus choice.
        double effective = classification == CompactnessClass.Slender ? EffectiveAreaRatio(d, yieldMpa) : 1.0;
        double wy = (classification == CompactnessClass.Compact ? s.ZxMm3.Value : s.SxMm3.Value) * effective;
        double wz = (classification == CompactnessClass.Compact ? s.ZyMm3.Value : s.SyMm3.Value) * effective;
        double λbar = EnSlenderness(s, yieldMpa, effectiveLengthMm);
        double χ = EnChi(λbar, cls.BucklingAlpha);
        double χlt = cls.LtbAlpha.Match(
            Some: α => EnChi(EnLtbSlenderness(s, wy, yieldMpa, unbracedLengthMm), α),
            None: static () => 1.0);
        double mRd = χlt * wy * yieldMpa / basis.GammaM1;
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: composite.Match(Some: c => Math.Max(CompositeMn(c, s, yieldMpa) / basis.GammaM0, mRd), None: () => mRd),
            FlexuralMinorNmm: wz * yieldMpa / basis.GammaM0,
            CompressionN: χ * s.AreaMm2.Value * effective * yieldMpa / basis.GammaM1,
            ShearN: s.AvyMm2.Value * yieldMpa / (Math.Sqrt(3.0) * basis.GammaM0),
            TorsionalNmm: cls.Topology == SteelTopology.Closed
                ? s.JMm4.Value / Math.Max(0.5 * d.DepthMm.Value, 1.0) * yieldMpa / (Math.Sqrt(3.0) * basis.GammaM0)
                : 0.0,
            Classification: classification,
            Slenderness: λbar,
            Chi: χ,
            ChiLt: χlt);
    }

    // §6.3.1.3 λ̄ = √(A·f_y/N_cr) over the receipt's WEAK-axis governing radius — the same axis the AISC Column body
    // buckles about, so a basis swap never changes WHICH axis governs, only how the reduction is spelled.
    static double EnSlenderness(ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        double ncr = Math.PI * Math.PI * E * s.AreaMm2.Value * s.GoverningRadiusMm * s.GoverningRadiusMm
            / (effectiveLengthMm * effectiveLengthMm);
        return Math.Sqrt(s.AreaMm2.Value * yieldMpa / Math.Max(ncr, double.Epsilon));
    }

    // §6.3.2.2 λ̄_LT = √(W_y·f_y/M_cr) over the warping-free elastic critical moment M_cr = (π/L)·√(E·Iz·G·It) the
    // receipt's real MINOR inertia IyMm4 and torsion constant JMm4 supply — the same columns the AISC F2 LTB body
    // reads, so neither basis re-mints a section property.
    static double EnLtbSlenderness(ComputedSection s, double modulusMm3, double yieldMpa, double unbracedLengthMm) {
        double mcr = Math.PI / Math.Max(unbracedLengthMm, 1.0)
            * Math.Sqrt(Math.Max(E * s.IyMm4.Value * G * s.JMm4.Value, 0.0));
        return Math.Sqrt(modulusMm3 * yieldMpa / Math.Max(mcr, double.Epsilon));
    }

    // §6.3.1.2 the ONE buckling-curve reduction both stability modes drive: Φ = 0.5(1 + α(λ̄ − 0.2) + λ̄²),
    // χ = 1/(Φ + √(Φ² − λ̄²)) capped at unity. One body, two α columns — a second transcription for the LTB curve is
    // one body, because the code publishes one formula and varies only its imperfection factor.
    static double EnChi(double lambdaBar, double alpha) {
        double φ = 0.5 * (1.0 + alpha * (lambdaBar - 0.2) + lambdaBar * lambdaBar);
        return Math.Min(1.0, 1.0 / (φ + Math.Sqrt(Math.Max(φ * φ - lambdaBar * lambdaBar, double.Epsilon))));
    }

    // The ONE AISI S100 cold-formed body both formed lanes drive — a lipped C stud, a cold-formed Z purlin, and a
    // ribbed deck sheet alike. Effective section is COMPUTED from geometry through the §B2 Winter reduction, never a
    // per-row published Seff/S proxy: a transcribed ratio is one stress state at one thickness, so it mis-prices the
    // same profile in axial compression, in flexure, and at any other yield, and it cannot exist at all for a
    // generated lattice row nobody published. F3.1 initiation-of-yielding φb·Fy·Seff over each axis, E3.1 flexural
    // buckling on the 0.658 curve with the effective AREA, G2 web shear on the receipt AvyMm2, and no closed-torsion
    // arm (every formed arm is an open thin-walled shape).
    static DesignCapacity FormedSection(DesignBasis basis, SectionDims d, ComputedSection s, double yieldMpa, double effectiveLengthMm) {
        (double λc, double Fcr) = Column(s, yieldMpa, effectiveLengthMm);
        double flexuralRatio = EffectiveModulusRatio(d, yieldMpa);
        double axialRatio = EffectiveAreaRatio(d, Fcr);
        return new DesignCapacity(
            Basis: basis,
            FlexuralNmm: Resist(basis, yieldMpa * s.SxMm3.Value * flexuralRatio, φb, basis.GammaM0),
            FlexuralMinorNmm: Resist(basis, yieldMpa * s.SyMm3.Value * flexuralRatio, φb, basis.GammaM0),
            CompressionN: Resist(basis, Fcr * s.AreaMm2.Value * axialRatio, φc, basis.GammaM1),
            ShearN: Resist(basis, 0.6 * yieldMpa * s.AvyMm2.Value, φv, basis.GammaM0),
            TorsionalNmm: 0.0,
            // The slenderness verdict is the COMPUTED reduction's own answer: a section that loses effective width at
            // yield IS slender, so the classification states what the algorithm found rather than a stored flag.
            Classification: flexuralRatio < 1.0 ? CompactnessClass.Slender : CompactnessClass.Compact,
            Slenderness: λc,
            Chi: 1.0,
            ChiLt: 1.0);
    }

    // AISI S100 §B2.1 Winter effective width: λ = (1.052/√k)·(w/t)·√(f/E), ρ = 1 at λ ≤ 0.673 and (1 − 0.22/λ)/λ
    // above, b_eff = ρ·w. k is the plate-buckling coefficient of the ELEMENT — 4.0 for a uniformly compressed
    // stiffened element (a flange between web and lip), 0.43 for an unstiffened one (the free edge of an unlipped
    // flange), 23.9 for a web under the pure-bending stress gradient.
    const double KStiffened = 4.0, KUnstiffened = 0.43, KWebBending = 23.9, WinterLimit = 0.673;

    static double Winter(double flatMm, double thicknessMm, double stressMpa, double k) {
        double λ = 1.052 / Math.Sqrt(k) * (flatMm / thicknessMm) * Math.Sqrt(stressMpa / E);
        return λ <= WinterLimit ? 1.0 : Math.Clamp((1.0 - 0.22 / λ) / λ, 0.0, 1.0);
    }

    // The FLAT widths a formed element actually buckles over: the overall dimension less one corner allowance
    // (inside radius plus wall) at each formed junction — the geometry the §B2 w/t reads, never the overall dim.
    static (double Web, double Flange) Flats(SectionDims d) =>
        (Math.Max(d.DepthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value),
         Math.Max(d.WidthMm.Value - 2.0 * (d.FilletMm + d.WebMm.Value), d.WebMm.Value));

    // Seff/S in bending: the compression flange and the compressed half-web each shed their INEFFECTIVE width at
    // their own lever from mid-depth, so the modulus ratio is one minus the first-moment loss over the gross first
    // moment. The flange sits at the extreme fibre (lever d/2), the compressed web portion at the centroid of its
    // own half (lever d/4) — one fold over the two flats, replacing the published constant entirely.
    // The drift census reads the SAME fold the design lane runs, over the generated row's own derived dimensions —
    // a second effective-width spelling for the census would grade one algorithm against another.
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
    // Mp -> 0.7·Fy·Sx across λpf..λrf (F3-1); slender reads the elastic 0.9·E·kc·Sx/λ² with kc = 4/√(h/tw)
    // clamped to [0.35, 0.76] (F3-2).
    static double FlangeLocalMn(SteelClass cls, SectionDims d, ComputedSection s, double Fy, double Mp) {
        if (cls.Slenderness.Case is not SlendernessRow row) { return Mp; }   // solid stock reaches its full plastic moment
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
    static double MinorMn(SteelClass cls, SectionDims d, ComputedSection s, double Fy) {
        double cap = cls.Regime == FlexureRegime.F10 ? 1.5 : 1.6;
        double mpy = Math.Min(Fy * s.ZyMm3.Value, cap * Fy * s.SyMm3.Value);
        if (cls.Regime != FlexureRegime.F2 || cls.Slenderness.Case is not SlendernessRow row) { return mpy; }
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
    static double TorsionalResistance(SteelClass cls, ComputedSection s, double yieldMpa) {
        double closedForm = φv * 0.6 * yieldMpa * s.JMm4.Value / (0.5 * s.DepthMm.Value);
        return cls.Topology.Map(open: 0.0, closed: closedForm, solid: closedForm);
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
    static double AngleMn(SectionDims d, ComputedSection s, double Lb, double Fy) {
        double my = Fy * s.SxMm3.Value, cap = 1.5 * my;
        if (Lb <= 0.0) return cap;
        double me = 0.46 * E * Math.Pow(s.WidthMm.Value * d.WebMm.Value, 2.0) / Lb;
        return me <= my ? (0.92 - 0.17 * me / my) * me : Math.Min(cap, (1.92 - 1.17 * Math.Sqrt(my / me)) * my);
    }

    // §F2 LTB reading the REAL warping: Lp = 1.76·ry·√(E/Fy), rts ≈ √(√(Iy·Iw)/Sx), Lr from J/Iw, the linear
    // Mp -> 0.7·Fy·Sx interpolation between, elastic Fcr·Sx beyond Lr.
    static double LateralTorsionalMn(SectionDims d, ComputedSection s, double Lb, double Fy, double Mp) {
        double ry = s.RyMm.Value, sx = s.SxMm3.Value, iy = s.IyMm4.Value, iw = s.IwMm6, jj = s.JMm4.Value;
        double Lp = 1.76 * ry * Math.Sqrt(E / Fy);
        double rts = iw > 0.0 ? Math.Sqrt(Math.Sqrt(iy * iw) / sx) : ry;
        // ho is the FLANGE-CENTROID separation d − tf, read off the admitted dims: reading the full depth instead
        // over-states the couple arm and pushes Lr high, crediting plastic moment past the real inelastic-LTB
        // transition on every shallow-web rolled shape.
        double c = 1.0, ho = Math.Max(d.DepthMm.Value - d.FlangeMm.Value, d.FlangeMm.Value);
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
        double sumQn = c.Stud.SteelShearKn(c.Group) * 1e3 * Math.Max(0, c.StudsPerMetre) * c.ShearSpanMm.Value / 1000.0;
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
// The SSMA cold-formed section as a GENERATED LATTICE over its own designation grammar. `<web>S<flange>-<mils>` names the web
// and flange in hundredths of an inch and the base metal in thousandths, so DepthMm/FlangeMm/WallMm DERIVE from the
// designation tokens and the roster is the cross product of the three nomenclature axes — a new stud is a token,
// never a transcribed row. LipMm is the AISI S100 §B4.2 minimum ADEQUATE simple stiffener computed from the flange
// flat (a code derivation over the section's own geometry, not a return-lip column nobody on this estate holds), and
// FilletMm the AISI minimum inside forming radius 1.5·t the connector#CONNECTOR_FAMILY sheet stock already states.
// PublishedSeffRatio is an OPTIONAL CHECK column: a size whose printed effective-section ratio this estate holds
// carries it, every other row carries None, and SteelDesign reads the COMPUTED §B2 reduction either way — the
// published value GRADES the algorithm through ColdFormedSections.Drift, never feeds it. The filled cells are the
// two-source acquisition (PublishedSeff below); a row outside that census carries None: a sample value invented to
// exercise the column is a measurement no source took.
public readonly record struct ColdFormedRow(int WebToken, int FlangeToken, int Mils, SteelGrade Grade, Option<double> PublishedSeffRatio = default) {
    const double InchMm = 25.4;
    const double SeffTolerance = 0.02;
    const double BendFactor = 1.5;          // AISI S100 minimum inside forming radius ≈ 1.5·t
    const double StiffenerCoefficient = 399.0, StiffenerCapSlope = 115.0, StiffenerCapIntercept = 5.0, StiffenerOnset = 0.328;

    public string Key => $"{WebToken}s{FlangeToken}-{Mils}";
    public double DepthMm => WebToken / 100.0 * InchMm;
    public double FlangeMm => FlangeToken / 100.0 * InchMm;
    public double WallMm => Mils / 1000.0 * InchMm;
    public double FilletMm => BendFactor * WallMm;
    public double FlangeFlatMm => Math.Max(FlangeMm - 2.0 * (FilletMm + WallMm), WallMm);

    // The computed §B2 reduction this row would design on, exposed so the drift census can grade it against a
    // printed ratio without re-entering the design projection.
    public double ComputedSeffRatio(double yieldMpa) => SteelDesign.EffectiveModulus(this, yieldMpa);

    public bool Drifts(double yieldMpa) =>
        PublishedSeffRatio.Exists(published => Math.Abs(published - ComputedSeffRatio(yieldMpa)) > SeffTolerance);

    // AISI S100 §B4.2 minimum adequate SIMPLE lip: S = 1.28·√(E/f) is the slenderness reference, an element below
    // 0.328·S needs no stiffener at all, and above it the required stiffener moment I_a = 399·t⁴·[(w/t)/S − 0.328]³
    // caps at t⁴·[115·(w/t)/S + 5]. A 90° simple lip has I_s = d³·t/12, so the minimum depth inverts to
    // d = (12·I_a/t)^(1/3). The roster generates AT the code minimum: a manufacturer ships at or above it, so a
    // generated row is the conservative section and a stocked one is never weaker than what this lattice prices.
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

// The generated roster: the SSMA web × flange × gauge cross product over the three nomenclature axes the published
// designation grammar itself declares — `<web>S<flange>-<mils>` in hundredths and thousandths of an inch — bounded
// only by the grammar's own rule that a flange token never exceeds its web token.
public static class ColdFormedSections {
    static readonly ImmutableArray<int> WebTokens = [250, 350, 362, 400, 550, 600, 800, 1000, 1200];
    static readonly ImmutableArray<int> FlangeTokens = [137, 162, 200, 250];
    static readonly ImmutableArray<int> Gauges = [33, 43, 54, 68, 97];   // mils base metal — the SSMA structural band

    // The roster is the GRAMMAR's own cross product under the one bound the grammar itself states — a flange token
    // never exceeds its web token. A further gauge-versus-depth restriction would be a stocking policy, and this
    // estate holds no manufacturer stock list to state one from, so the lattice generates the whole admissible space
    // and a stocked subset stays a FILTER a caller applies over it.
    public static readonly ImmutableArray<ColdFormedRow> Rows = [..
        from web in WebTokens
        from flange in FlangeTokens
        from mils in Gauges
        where flange <= web
        let row = new ColdFormedRow(web, flange, mils, SteelGrade.A653Gr50)
        select row with { PublishedSeffRatio = PublishedSeff.TryGetValue(row.Key, out double ratio) ? Some(ratio) : Option<double>.None }];

    // The PUBLISHED check cells, two-source acquired: Se/Sx at Fy = 50 ksi from the printed gross and effective
    // section-modulus columns of the AISI S100-07/-12 print lineage — the SSMA Product Technical Guide (© 2022,
    // eff. 2/24/22) corroborated by the independent lineage publishers (The Steel Network, MarinoWare,
    // ClarkDietrich, CEMCO), intra-lineage agreement exact to the printed digit. The census over the 180-row
    // lattice: 102 cells fill — the 54/68/97-mil band where the SSMA guide itself prints the Fy50 column; the
    // 33/43-mil Fy50 prints are manufacturer-only and stay absent; 21 designations no publisher lists, 5 are
    // single-sourced, and the 2.5"-flange 33-mil rows exceed the AISI B4.1 w/t 60 bound (SFIA dashes them), all
    // absent. The S100-16/S2-20 print lineage differs up to ~4% on Se — a drift row breaching SeffTolerance names
    // print-basis divergence (spec generation, cold-work Fya basis) before it names the algorithm.
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

    // The DRIFT CENSUS: wherever a row carries a printed effective-section ratio, this grades the computed §B2
    // reduction against it and surfaces the delta. The published value never feeds the design — it GRADES the
    // algorithm — so the reader is a census a composition root or a proof run folds, and a row carrying no printed
    // ratio contributes nothing rather than a fabricated agreement.
    public static Seq<(string Key, double Published, double Computed)> Drift(double yieldMpa) =>
        toSeq(Rows).Bind(row => row.PublishedSeffRatio
            .Map(published => (row.Key, Published: published, Computed: row.ComputedSeffRatio(yieldMpa)))
            .ToSeq());
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

// The profile SOURCE axis of one seed row — the THREE legal origins of a steel profile, closed so a hybrid is
// unrepresentable: a published catalogue identity (its composite augmentation riding the case), a generated
// cold-formed stud row, or a fabricated build delegate.
[Union]
public abstract partial record SteelRowSource {
    private SteelRowSource() { }
    public sealed record Rolled(ICatalogue Catalogue, Option<CompositeDetail> Composite) : SteelRowSource;
    public sealed record Formed(ColdFormedRow Row) : SteelRowSource;
    // The FABRICATED origin: a plate girder, a cranked mono-I, a welded tee, a cold-formed Z purlin — a section no
    // catalogue publishes, built as a parametric SectionProfile arm whose Forms open-thin-walled algebra the solver
    // already prices. The case carries the row's own RAILED build delegate rather than a shape enum plus a dims bag,
    // so the fabricated dimensions are seed-call-site arguments to the profile's real Of factory and a malformed
    // fabricated row aborts the catalogue TYPED on the one fold, never at type init.
    public sealed record Plated(Func<Op, Fin<SectionProfile>> Build) : SteelRowSource;
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
    // 3/4in studs at 2/m over the 4.5 m §I3.2d shear span of a 9 m simple beam — ΣQn reads StudClass.S19.SteelShearKn),
    // the 600S162-54 AISI stud on its OWN generated ColdFormedRow at the A653 SS Gr 50 band, and the FABRICATED rows that make the open-thin-walled Forms algebra reachable from an
    // admitted Component: a welded plate girder (1200 mm web × 12 mm, 400/500 mm asymmetric flanges × 25/32 mm — the
    // AsymmetricIShape arm Forms.MonoI prices), a cold-formed Z purlin (200 mm depth, 75/65 mm flanges, 2.5 mm
    // wall, 20/18 mm lips — the Zed arm Forms.PointSymmetricZ prices), and a welded STAINLESS girder in 1.4301 whose
    // Plated origin resolves the EN 10088-2 plate cell (210 MPa) — the row that makes the stainless lane reachable.
    // Fabricated dims are seed DATA, the same posture as a SmartEnum row column; the profile arms admit through
    // their own railed Of factories inside RowOf.
    static readonly Seq<SteelRowSeed> Augmented = Seq(
        new SteelRowSeed("steel.comp-w18x50-slab120",
            new SteelRowSource.Rolled(CatalogueFactory.CreateAmerican(American.W18x50),
                Some(new CompositeDetail(PositiveMagnitude.Create(1200.0), PositiveMagnitude.Create(100.0), 28.0, StudClass.S19, StudGroup.Direct, 2, PositiveMagnitude.Create(4500.0)))),
            SteelGrade.A992, Aisc),
        new SteelRowSeed("steel.pg-1200x400-500", new SteelRowSource.Plated(static key => SectionProfile.AsymmetricIShape.Of(
            depthMm: 1200.0, topFlangeWidthMm: 400.0, bottomFlangeWidthMm: 500.0,
            topFlangeThicknessMm: 25.0, bottomFlangeThicknessMm: 32.0, webThicknessMm: 12.0, filletMm: 0.0, key)), SteelGrade.S355, En),
        new SteelRowSeed("steel.zed-200x75x25", new SteelRowSource.Plated(static key => SectionProfile.Zed.Of(
            depthMm: 200.0, topFlangeWidthMm: 75.0, bottomFlangeWidthMm: 65.0,
            thicknessMm: 2.5, topLipMm: 20.0, bottomLipMm: 18.0, innerFilletMm: 3.75, key)), SteelGrade.A653Gr50, Aisc),
        new SteelRowSeed("steel.pg-ss-800x300", new SteelRowSource.Plated(static key => SectionProfile.IShape.Of(
            depthMm: 800.0, widthMm: 300.0, webMm: 8.0, flangeMm: 20.0, filletMm: 0.0, flangeToeMm: 20.0, key)), SteelGrade.Ss14301, En));

    // The seed domain is the ENUMERATED identity space of the admitted catalogue package — every `American` and
    // `European` member the package declares, minted through its singleton factory, so a model importing a W40x593 or
    // an HL1100R dereferences it and a sizing fold scans the whole space. It is bounded by the enum, not by a claim
    // about the published database: a section the package never declared is unreachable here whatever the standard
    // prints. `Census` pins that bound so a package bump SURFACES as a count change rather than silently widening or
    // narrowing the catalogue.
    // The FORMED lane is the whole generated SSMA lattice: `ColdFormedSections.Rows` is the designation grammar's own
    // cross product, so every admissible (web, flange, gauge) triple seeds through the railed `ColdFormedC.Of`.
    // Both rosters FREEZE — a property re-minting the whole lattice on every read re-runs the cross product and every
    // catalogue-factory call once per catalogue build, per sizing sweep, and per census read.
    static readonly Seq<SteelRowSeed> Formed =
        toSeq(ColdFormedSections.Rows).Map(static row =>
            new SteelRowSeed($"steel.cf-{row.Key}", new SteelRowSource.Formed(row), row.Grade, Aisc));

    static readonly Seq<SteelRowSeed> Seeds =
        toSeq(Enum.GetValues<American>()).Map(Rolled)
            .Concat(toSeq(Enum.GetValues<European>()).Map(Rolled))
            .Concat(Formed)
            .Concat(Augmented);

    // The enumeration-bounded census: the rolled roster is exactly the two identity enums, so a package bump that
    // adds or retires a section moves this count and the guard names the drift instead of the catalogue absorbing it.
    public static readonly (int American, int European, int Formed, int Augmented) Census =
        (Enum.GetValues<American>().Length, Enum.GetValues<European>().Length, Formed.Count, Augmented.Count);

    // The ONE generator arm: the Source union selects the profile origin — a catalogue admitted once through
    // SteelShape.Of (the composite reclass riding the Rolled case), the railed parametric ColdFormedC from the
    // published stud row, or the fabricated row's own railed build delegate — then Component.Of constructs the row:
    // the family's own concrete IFC leaf (the occurrence role is a Bim-egress refinement), Coring.None,
    // DetailLane.None (no bag), the grade Substance and the stable metal.iron render row on the two independent
    // MaterialId slots. A third origin was one case and one arm; Component.Of's family admission is what gates which
    // fabricated profile arms may seed.
    static Fin<ComponentRow> RowOf(SteelRowSeed seed) {
        Op key = Op.Of(name: seed.Designation);
        return seed.Source.Switch(
                state: (Seed: seed, Key: key),
                rolled: static (x, r) => SteelShape.Of(r.Catalogue, x.Seed.Grade, x.Seed.Standard, x.Key)
                    .Map(shape => r.Composite.IsSome ? shape with { Class = SteelClass.Composite, Composite = r.Composite } : shape)
                    .Map(shape => (SectionProfile)new SectionProfile.Catalogued(shape)),
                // The lip is the AISI code minimum at the row's own NOMINAL yield — geometry never depends on a
                // national annex, so the generated section is one shape regardless of where it is checked.
                formed: static (x, f) => SectionProfile.ColdFormedC.Of(
                    f.Row.DepthMm, f.Row.FlangeMm, f.Row.WallMm, f.Row.LipMm(f.Row.Grade.NominalYieldMpa), f.Row.FilletMm, x.Key),
                plated: static (x, p) => p.Build(x.Key))
            .Bind(profile => Component.Of(
                ComponentFamily.Steel, seed.Designation, profile,
                ComponentFamily.Steel.Ifc, Coring.None, seed.Standard,
                seed.Grade.Substance, MaterialId.Of("metal.iron"), Some(Detail(seed, profile)), key))
            .Map(item => new ComponentRow(item, Source(seed)));
    }

    // The steel REALIZATION bag. Steel carries `DetailLane.Realization` because `SteelClass.IfcSubtype` has no other
    // landing surface: the Bim egress profile lane reads `DetailSchema.ProfileSubtype` off a seeded bag, so a family
    // declaring no lane seeds no bag and every steel section reaches that lane as an unresolved subtype however many
    // subtype tokens this page publishes. BackToBack rides beside it as its own realization row — a double angle's
    // spacing is a REALIZATION fact of the fabricated pair, and publishing it here keeps it on the one surface an
    // egress reader can reach.
    static PropertyBag Detail(SteelRowSeed seed, SectionProfile profile) =>
        ComponentDetail.RealizationRows(
            [ComponentDetail.Token(DetailSchema.ProfileSubtype, SubtypeOf(profile)),
             ComponentDetail.Sourced(Source(seed))]
            .Append(BackToBack(profile).Map(static mm => ComponentDetail.Token(PropertyCategory.Materials.Row("BackToBack"), mm.ToString("R", CultureInfo.InvariantCulture))).ToSeq())
            .ToArray());

    static string SubtypeOf(SectionProfile profile) =>
        profile is SectionProfile.Catalogued c ? c.Shape.Class.IfcSubtype : SteelClass.ColdFormed.IfcSubtype;

    static Option<double> BackToBack(SectionProfile profile) =>
        profile is SectionProfile.Catalogued { Shape.Section.BackToBackMm: > 0.0 and var mm } ? Some(mm) : None;

    // A row's PROVENANCE states where its VALUES came from: a catalogue identity is read from an admitted package,
    // the generated stud lattice is derived by the AISI grammar's own rules, and a fabricated row is this estate's
    // own shop dimensions.
    static Provenance Source(SteelRowSeed seed) => seed.Source switch {
        SteelRowSource.Rolled => Provenance.Vendor,
        SteelRowSource.Formed => Provenance.Defined,
        _                     => Provenance.Authored,
    };

    // Fail-loud: ONE Traverse over the unified seed — a ClassOf/dims/admission failure ABORTS the catalogue build. A
    // steel row is sectioned by its own topology, so a row that cannot admit or solve is fatal, never silently
    // absent. The Context parameter is the ComponentFamily.Rows delegate contract; this seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Seeds.Traverse(RowOf).As();

    // The ComponentFamily.Steel CAPACITY producer: the profile arm and the seeded grade are the only inputs
    // SteelDesign.Capacity discriminates on, and a steel row is always Sectioned, so an unresolved section is a
    // catalogue defect railed here rather than a silent absence. The grade rides the row's own substance identity,
    // which the seed already bound — no caller re-selects it.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from solved in section.ToFin(ComponentFault.Section(key, $"<steel-section-unresolved:{component.Designation.Value}>"))
        from grade in GradeOfComponent(component, key)
        from design in SteelDesign.Capacity(component.Profile, grade, solved, placement, key)
        select SectionCapacity.Lift(new CapacityReceipt.Steel(component.Designation, design));

    // The seeded grade recovered from the row: a Catalogued profile carries it on its admitted SteelShape, and every
    // parametric arm carries it on the substance MaterialId the seed stamped — one read, no designation parse.
    static Fin<SteelGrade> GradeOfComponent(Component component, Op key) =>
        component.Profile is SectionProfile.Catalogued c
            ? Fin.Succ(c.Shape.Grade)
            : toSeq(SteelGrade.Items).Find(g => g.Substance == component.SubstanceId)
                .ToFin(ComponentFault.Grade(key, $"<steel-grade-unregistered:{component.SubstanceId}>"));
}
```

## [03]-[RESEARCH]

(none)
