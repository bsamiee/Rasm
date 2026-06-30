# [MATERIALS_STEEL]

THE STEEL COMPONENTFAMILY GROUNDED IN THE PUBLISHED SECTION DATABASE. The steel cross-section vocabulary — the `VividOrange.Profiles.Catalogue` AISC Shapes Database v16.0 (2299 American) and EN 10365:2017 (558 European) published sections as typed sealed-singleton profile classes carrying real `UnitsNet.Length` geometry, the `VividOrange.Sections.SectionProperties` Green's-theorem polygon-integral solver computing every elastic property from that geometry, and the `IfcProfileDef` i-shape/u-shape/l-shape/double-angle/hss-rect/hss-round/tee/composite/cold-formed subtype discriminant — is the steel cross-section the one `component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Steel` case, the cross-section a `ComponentSection.Steel(SteelShape)` FIELD of the `Component` and never a peer, spanning the full structural-steel product range: rolled shapes, AISC 360 Chapter I composite steel-concrete sections, and AISI S100 cold-formed light-gauge members. Steel is a `ComponentClass.Primary` member — the space-bounding `IfcBuiltElement` stratum where one occurrence is one piece — so a steel member extrudes through one `Component` over the `RunPath`, never a per-family layout, and resolves its section ONCE through the family axis. A wide-flange section is a `Component` row keyed by a `CatalogueFactory.CreateAmerican(American)` / `CreateEuropean(European)` identity, never a `WSection` type and never a hand-keyed dimension literal: the section shape, the published geometry, and the composite/cold-formed detail are steel-`Component` columns sourced from the registered database, and the steel section computes the SAME canonical `component#COMPONENT_OWNER` `ComputedSection` (the twenty-field strong-AND-weak-axis stiffness receipt every family shares, the steel family the one that fills the three asymmetric-section LTB columns a channel/tee/angle carries) that the `component#COMPONENT_RESOLUTION` one-hop caches and the `Component/capacity#SECTION_CAPACITY` `SteelLrfd` arm reads.

The steel family is the structurally RICHEST member of the axis and therefore the one that fills the `ComputedSection` columns the masonry/cmu/timber rectangles leave engineering-zero: an OPEN thin-walled rolled shape carries a positive warping constant `IwMm6` (the EN 1993-1-1 §6.3.2 / AISC 360 Chapter F lateral-torsional-buckling input the bare St-Venant `J` cannot supply), distinct web-and-flange shear areas `AvyMm2`/`AvzMm2`, a both-axis plastic modulus `ZxMm3`/`ZyMm3` the elastic polygon integral cannot yield, AND — for a singly-symmetric channel/tee/angle — the shear-centre offsets `ShearCentreYMm`/`ShearCentreZMm` and the EN 1993-1-1 NCCI SN030 mono-symmetry factor `MonosymmetryFactor` the §6.3.2 GENERAL LTB route needs (engineering-zero for a doubly-symmetric I/HSS/solid). `SectionReader.Read` admits the catalogued `IProfile` to that ONE `ComputedSection` — the elastic columns from the solver, the plastic/torsion/warping/shear AND asymmetric-section LTB columns from the `SteelStiffness` closed-form algebra keyed by the `SteelClass` open/closed/solid topology — so a steel W-shape and a glulam rectangle resolve their section through the SAME `component#COMPONENT_OWNER` cache, the structural-design seam reading `graph.SectionOf(member)` with no idea whether the column set came from a polygon integral or a published shape. The LRFD `DesignCapacity` and the AISC Table B4.1 `CompactnessClass` are derived projections over THAT `ComputedSection` (never re-minted dimensions), lifted into the unified `Component/capacity#SECTION_CAPACITY` `Utilisation` rail; the steel grade yield is the registered `VividOrange.Materials` `EnSteelFactory` Table 3.1 DATA, never a caller-supplied scalar.

The steel vocabulary grows by data — a new section is one `American`/`European` identity in the seed, a new subtype one `SteelClass` case carrying its `SteelTopology` and `IfcProfileDef` mapping — never a per-section type and never a transcribed section-property literal. A composite floor beam references the `Component/joint#JOINT_FAMILY` shear-stud `StudClass` for its steel-concrete shear interface (the one stud vocabulary, the `StudClass.SteelShearKn` per-stud cap the composite `ΣQn` reads, never a parallel stud owner). The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentSection`/`ComponentStandard` shape AND the canonical `ComputedSection` receipt + `ParametricSection` perimeter bridge, `VividOrange.Profiles.Catalogue` + `VividOrange.Sections.SectionProperties` for the published geometry and its computed elastic properties, `VividOrange.Materials` `EnSteelFactory` for the registered grade yield, the in-folder `MaterialUnits` boundary for the `UnitsNet`→SI-millimetre admission, the `Component/capacity#SECTION_CAPACITY` rail for the unified utilisation verdict, and the `Rasm.Vectors` `PositiveMagnitude` for every admitted length/property column; cmu/timber/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[STEEL_FAMILY]: the `SteelClass` subtype axis (i-shape · u-shape · l-shape · double-angle · hss-rect · hss-round · tee · composite · cold-formed) carrying its `SteelTopology` (open/closed/solid) and `IfcProfileDef` mapping, folded onto the catalogue `AmericanShape` (13) / `EuropeanShape` (25) family taxonomy by the TOTAL `SteelClass.OfShape` and the GEOMETRY-driven `ComponentCatalogue.ClassOf` round/rectangular hollow split; the `SteelStiffness` closed-form algebra deriving the plastic moduli / St-Venant torsion / warping constant / both-axis shear areas the elastic polygon integral cannot yield, per `SteelClass` topology; the `SectionReader` `VividOrange` catalogue+solver admission boundary that reads one `ICatalogue` into the canonical `component#COMPONENT_OWNER` `ComputedSection`; the `SteelGrade` `[SmartEnum]` binding the design yield to `VividOrange.Materials` `EnSteelFactory` through the per-grade `EnSteelDeliveryCondition`; the `SteelDesign` AISC Table B4.1 `CompactnessClass` + LRFD `DesignCapacity` projection (rolled F/E/G · AISC 360 Ch I composite · AISI S100 cold-formed) over the `ComputedSection`; the `CompositeDetail`/`ColdFormedDetail` augmentation; and the `ComponentCatalogue.BuildSteelRows` + `SteelSections` catalogue-seeded row table and section map the `[03]` `component#COMPONENT_RESOLUTION` data-join reads.

## [02]-[STEEL_FAMILY]

- Owner: the steel section vocabulary (`SteelClass` the `IfcProfileDef` subtype discriminant carrying its `SteelTopology` open/closed/solid flag, folded onto the catalogue `AmericanShape`/`EuropeanShape` family enum; `SteelStiffness` the closed-form plastic/torsion/warping/shear + shear-centre-offset/mono-symmetry algebra; `SteelGrade` the registered-yield band; `CompactnessClass` the Table B4.1 verdict; `DesignCapacity` the LRFD receipt); `SectionReader` the `VividOrange.Profiles.Catalogue`+`VividOrange.Sections.SectionProperties` admission boundary that reads one `ICatalogue` into the canonical `component#COMPONENT_OWNER` `ComputedSection` (NOT a parallel `SteelSection`); `SteelDesign` the AISC classification + LRFD projection over that `ComputedSection`; `ComponentCatalogue.BuildSteelRows` the catalogue-seeded registered-row seed `component#COMPONENT_OWNER` composes and `ComponentCatalogue.SteelSections` the `ComponentId`→`ComputedSection` map the `[03]` `component#COMPONENT_RESOLUTION` `ComponentResolution.Build` data-join consumes.
- Cases: class {i-shape (W/M/S/HP, open), u-shape (C/MC channel, open), l-shape (L angle, open), double-angle (2L from `AmericanShape.DoubleL`, open), hss-rect (closed), hss-round (round-HSS + Pipe, closed), tee (WT/MT/ST, open), composite (AISC 360 Ch I steel-concrete, open core), cold-formed (AISI S100 light-gauge, open)} — the `IfcProfileDef` parameterized-profile subtype set carrying a `SteelTopology` (`open`=I/channel/tee/angle thin-walled, `closed`=HSS/pipe hollow, `solid`=the rare bar), folded onto the published `AmericanShape` (13) / `EuropeanShape` (25) family taxonomy by the TOTAL `SteelClass.OfShape`; a section is the canonical `ComputedSection` over one `SteelClass` carried in a `ComponentSection.Steel(SteelShape)` cross-section FIELD (the composite/cold-formed augmentation riding its `CompositeDetail`/`ColdFormedDetail` `Option` on the `SteelShape` row), never a section subtype, a parallel composite owner, a parallel section-family enum duplicating `AmericanShape`/`EuropeanShape`, or a parallel narrow section receipt duplicating `ComputedSection`.
- Entry: `public static Fin<(ComputedSection Section, SectionDims Dims)> SectionReader.Read(ICatalogue catalogue, SteelClass cls, Op key)` — the ONE `VividOrange` admission boundary that produces the CANONICAL `component#COMPONENT_OWNER` `ComputedSection` (plus the published dims the classifier reads, in ONE pass): it runs `new SectionProperties((IProfile)catalogue)` over the polymorphic `IProfile` for the elastic columns (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter`), reads the family-cast dimensional columns (`II`/`IIParallelFlange`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`+`IHollowStructuralSection`) as `UnitsNet.Length`, computes the plastic moduli `ZxMm3`/`ZyMm3` + St-Venant torsion `JMm4` + warping constant `IwMm6` + both-axis shear areas `AvyMm2`/`AvzMm2` + the asymmetric-section LTB columns `ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor` from the `SteelStiffness` closed forms keyed by `SteelClass`/`SteelTopology`, and admits every quantity to its SI-millimetre scalar through the in-folder `MaterialUnits` boundary into the kernel `PositiveMagnitude` columns once — the SAME twenty-field receipt `ParametricSection` produces for the rectangle families. `public static Fin<SteelClass> ComponentCatalogue.ClassOf(ICatalogue catalogue, Op key)` is the catalogue→class resolver: it splits the round-vs-rectangular hollow by the `ICircularHollow`/`IRectangularHollow` GEOMETRY interface (AISC assigns BOTH a round HSS and a rectangular HSS the SAME `AmericanShape.HSS` family value, so the geometry — never the family enum — is the round/rect discriminant), then dispatches the open families onto the TOTAL `SteelClass.OfShape`. `public static CompactnessClass SteelDesign.Classify(SteelShape shape, double yieldMpa)` is the AISC Table B4.1 per-topology slenderness verdict (compact/noncompact/slender) over the shape's `Dims`, and `public static DesignCapacity SteelDesign.Capacity(SteelShape shape, double yieldMpa, double unbracedLengthMm, double effectiveLengthMm)` the LRFD projection emitting `φMn` (flexural — yielding/LTB-with-warping/FLB for a rolled shape via the `ComputedSection` `IwMm6`/`SxMm3`/`ZxMm3`, the AISC 360 Ch I plastic composite moment for a `Composite` section over its slab + `StudClass` `ΣQn`, the AISI S100 effective-section `Seff·Fy` for a `ColdFormed` section), `φPn` (compression — flexural buckling over the WEAK-axis `min(RxMm, RyMm)`), `φVn` (shear over the major-axis web shear area `AvyMm2` — the seam `AvY`(major), distinct from the minor-axis flange `AvzMm2`), and `φTn` (the AISC 360 §H3.1 design torsional resistance `φT·Fcr·C` over the CLOSED-topology HSS torsional constant `C = JMm4/c`, engineering-zero for an OPEN thin-walled shape whose §H3.3 warping torsion is not a single-resistance scalar) — every capacity a derived projection over the canonical `ComputedSection` columns, lifted into the `Component/capacity#SECTION_CAPACITY` rail by `SectionCapacityResolver.SteelLrfd(DesignCapacity)` which reads the `TorsionalNmm` column directly onto `SectionCapacity.SteelLrfd.TorsionalKnm`, never a re-minted dimension, a parallel composite owner, or a redundant lift parameter beside the carried column.
- Packages: VividOrange.Profiles.Catalogue (`CatalogueFactory.CreateAmerican`/`CreateEuropean` → `ICatalogue`/`IProfile`, the `American`/`European` identity enums, the `AmericanShape`/`EuropeanShape` family enums, the `II`/`IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`IRectangularHollow`/`ICircularHollow`/`IHollowStructuralSection` `UnitsNet.Length` geometry interfaces with `IIParallelFlange.FilletRadius`/`IDoubleAngle.BackToBackDistance`; `.api/api-vividorange-profiles-catalogue.md`), VividOrange.Sections.SectionProperties (`new SectionProperties(IProfile)` + the `Area`/`MomentOfInertiaYy`/`Zz`/`ElasticSectionModulusYy`/`Zz`/`RadiusOfGyrationYy`/`Zz`/`Perimeter` `UnitsNet` properties; `.api/api-vividorange-sections-sectionproperties.md`), VividOrange.Materials (`EnSteelMaterial`/`EnSteelFactory.CreateLinearElastic` the Table 3.1 `f_y` by grade × `EnSteelSpecification` × thickness-band — the `SteelGrade.YieldMpa(thicknessMm)` source; the `EnSteelDeliveryCondition` AR/N/M selector; the `ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException` derivation throws trapped at the grade admission; `.api/api-vividorange-materials.md`), VividOrange.Standards (`En1993` the grade cites; `.api/api-vividorange-standards.md`), UnitsNet (the `MaterialUnits` in-folder SI-millimetre coercion, the `Pressure`/`Length` from the EN factory; `.api/api-unitsnet.md`), Rasm.Vectors (project — `PositiveMagnitude` for every admitted column), Rasm (project — `Context`/`Op`), Rasm.Element (project — `ProfileRef` the `[03]` `component#COMPONENT_RESOLUTION` resolves, `MaterialId`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the steel vocabulary grows by data — a new AISC/EN section is one `American`/`European` identity added to `AmericanSeed`/`EuropeanSeed` (the catalogue carries all 2299 + 558 published sections; the seed selects the realized subset, every other section already present behind its enum value), a new shape family one `SteelClass` case carrying its `SteelTopology`, its `IfcProfileDef` subtype mapping, its `SteelClass.OfShape` fold arm, and its `SteelStiffness` plastic/torsion/warping/shear-centre arm, a new built-up section one parametric `Perimeter`-backed `ComputedSection` over the matching class through `component#COMPONENT_OWNER` `ParametricSection` — never a per-section type, never a transcribed section-property literal, never a parallel composite/cold-formed owner, never a parallel section receipt. A new steel grade is one `SteelGrade` row binding its `EnSteelGrade` + the `EnSteelDeliveryCondition` whose Table 3.1 sub-table holds the grade (AR holds S235/S275/S355/S450, N/M hold S420/S460), or its AISC spec-nominal. A cmu/timber/glazing family lands its own vocabulary on its own page the way steel carries `SteelClass`/`SteelStiffness`.
- Boundary: the steel vocabulary is the realized structural `ComponentFamily.Steel` cross-section — a per-section class, a hand-keyed section-property literal table, AND a parallel narrow `SteelSection` receipt are the deleted forms; `SectionReader.Read` is the BOUNDARY_ADMISSION point where raw `VividOrange` `UnitsNet` geometry is admitted EXACTLY ONCE into the canonical `component#COMPONENT_OWNER` `ComputedSection` — the catalogue's published dimensions (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the quantity) and the solver's computed `Area`/`AreaMomentOfInertia`/`Volume`/`Length` properties coerce to SI-millimetre scalars through the in-folder `MaterialUnits` boundary and admit into the kernel `PositiveMagnitude` (double-backed `> 0` finite), so the section never re-mints a length primitive, a fractional AISC web thickness admits without truncation, and the interior carries raw SI doubles never a `UnitsNet` type in a signature; the elastic `IxMm4`/`IyMm4`/`SxMm3`/`SyMm3`/`RxMm`/`RyMm` columns come from the ONE `VividOrange` polygon integral, never hand-keyed, and the plastic `ZxMm3`/`ZyMm3`, the St-Venant torsion `JMm4`, the warping `IwMm6` (positive for the OPEN topologies, engineering-zero for the closed/solid — the EN 1993-1-1 §6.3.2 lateral-torsional-buckling input the bare `J` cannot supply, exactly the column `component#COMPONENT_OWNER` `ComputedSection` reserves for the steel family), and the both-axis shear areas `AvyMm2`/`AvzMm2` are the DERIVED columns the elastic integral cannot yield — `SteelStiffness` computes them from the admitted dimensional columns per `SteelClass`/`SteelTopology` (the I/channel/tee web-flange decomposition, the HSS-rect/round closed form, the angle/built-up shape-factor), grounding them in geometry not a literal; the asymmetric-section LTB columns `ShearCentreYMm`/`ShearCentreZMm` (the centroid→shear-centre offsets) and `MonosymmetryFactor` (the EN 1993-1-1 NCCI SN030 β_y) are the further DERIVED columns `SteelStiffness` computes for the singly-symmetric open shapes (a channel's minor-axis offset, a tee/angle's offset + β_y) and leaves engineering-zero for the doubly-symmetric I/HSS/solid so the seam `IsDoublySymmetric` reads them symmetric — so the steel `ComputedSection` is the SAME twenty-field shape the rectangle families produce and the `[03]` `component#COMPONENT_RESOLUTION` data-join keys uniformly; the `RadiusOfGyrationMm` hand-`Math.Sqrt(Ix/A)` is the deleted form — both `RxMm` and `RyMm` come from the solver, so the `Capacity` flexural-buckling check governs about the WEAK axis (`min(rx, ry)`) the way real column design does, never the strong-axis-only approximation; `SteelClass.OfShape` is TOTAL over the published `AmericanShape` (13) / `EuropeanShape` (25) family taxonomy (the EN families are exclusively i-shape and channel — there is no European angle/hollow/tee family in EN 10365, so the two `OfShape(EuropeanShape)` arms exhaust the 25; the round-vs-rectangular hollow split the family enum cannot express is the GEOMETRY-driven `ComponentCatalogue.ClassOf` `ICircularHollow`/`IRectangularHollow` discriminant, never a silent `_ => IShape` swallowing a family — an unrecognized family rails `ComponentFault.Family`) so the catalogue's own family axis IS the section discriminant and each shape maps to its `IfcProfileDef` subtype (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef`, the `DoubleL`/`Composite` to `IfcArbitraryClosedProfileDef` since neither has a single parametric form, the `ColdFormed` channel-stud to `IfcUShapeProfileDef`) so a steel member round-trips to IFC 4.3 as an `IfcMaterialProfileSet`; the `IsCompact` bool is the deleted form — `SteelDesign.Classify` returns the 3-state `CompactnessClass` over the AISC Table B4.1 PER-TOPOLOGY slenderness limits (the I-shape flange `λpf`/`λrf` + web `λpw`/`λrw`, the HSS-rect wall `b/t`, the round-HSS `D/t`, the angle `b/t`, the tee stem `d/tw`) — never one I-shape model misapplied to a round pipe — and `SteelDesign.Capacity` derives the LRFD `φMn`/`φPn`/`φVn` from the canonical `ComputedSection` columns (the LTB reduction reading the real `IwMm6`/`SxMm3`, the web shear reading the MAJOR-axis `AvyMm2` — the seam `AvY`(major), the flange `AvzMm2` the minor `AvZ` — so `SeamSection` publishes the web area as the major-axis shear a downstream EN 1993 consumer reads); the `Composite` section reads the `Component/joint#JOINT_FAMILY` `StudClass.SteelShearKn` per-stud cap × `StudsPerMetre` for its horizontal-shear `ΣQn` (partial composite action when `ΣQn` < `As·Fy`), never a re-derived stud shear, and the `ColdFormed` section reads its AISI S100 `EffectiveSectionModulusRatio` for the local-buckling flexural reduction, both derived projections over the `ComputedSection`; the steel design yield is the registered `SteelGrade.YieldMpa(thicknessMm, annex, key)` grade DATA citing `En1993` (the EN bands reading `EnSteelFactory.CreateLinearElastic` Table 3.1 `f_y` through the grade's `EnSteelDeliveryCondition`, the AISC bands their spec-nominal), never a hand-keyed scalar, the EN derivation throw trapped onto `ComponentFault.Family`; `DesignCapacity` lifts into the unified `Component/capacity#SECTION_CAPACITY` `SectionCapacity.SteelLrfd` rail (the steel capacity computation staying THIS owner, the utilisation verdict the capacity page's), never a parallel `SteelBeamCheck` surface; `ComponentCatalogue.BuildSteelRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue.Rows` table by folding `AmericanSeed`/`EuropeanSeed` through `CatalogueFactory` and `SectionReader.Read`, keyed `steel.<designation>` into a `Component` whose cross-section FIELD is `ComponentSection.Steel(shape)`, and `ComponentCatalogue.SteelSections` is the precomputed `ComponentId`→`ComputedSection` map the `[03]` `component#COMPONENT_RESOLUTION` `ComponentResolution.Build` JOINS (the section captured at this catalogue-build site, never a `SteelSectionOf` delegate re-invoked per resolution call — the deleted phantom), so a steel W-shape resolves its `ComputedSection` ONCE into the one cache the structural consumer reads, the realized cross-section grounded in the registered published database rather than a hand-transcribed table.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                                   // Fin, Option, Seq
using Rasm.Vectors;                                  // PositiveMagnitude (the import-fix: PositiveMagnitude lives in Rasm.Vectors, NOT Rasm.Domain)
using Rasm.Domain;                                   // Context, Op (the kernel admission context)
using Rasm.Element;                                  // MaterialId (the seam appearance handle BuildSteelRows assigns)
using Thinktecture;                                  // [SmartEnum]/[Union]/KeyMemberEqualityComparer, ComparerAccessors
using VividOrange.Profiles;                          // CatalogueFactory, American/European, AmericanShape/EuropeanShape, ICatalogue, II/IChannel/...
using VividOrange.Sections.SectionProperties;        // SectionProperties polygon-integral solver over IProfile
using VividOrange.Materials.StandardMaterials.En;    // EnSteelGrade, EnSteelMaterial, EnSteelFactory, EnSteelDeliveryCondition (the Table 3.1 f_y source)
using VividOrange.Standards.Eurocode;                // NationalAnnex (the EN factory annex axis — the .Eurocode floor enum, NOT VividOrange.Standards)
using UnitsNet;                                      // Length (the EnSteelFactory thickness-band selector)
using Op = Rasm.Domain.Op;
using Rasm.Materials.Component;                      // Component/ComponentSection/ComponentFamily/ComponentClass/ComponentStandard/ComponentAuthority/ComponentId/ComponentFault/ComputedSection/ParametricSection/ComponentCatalogue (the parent COMPONENT_OWNER)
using Rasm.Materials.Component.Joint;                // StudClass (Component/joint#JOINT_FAMILY — the composite-shear per-stud cap CompositeDetail reads)
using static LanguageExt.Prelude;                    // Some, None, toSeq, Try

// Each family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the sibling `ComponentCatalogue` static
// classes are distinct types (one shared namespace collides at CS0101); component#COMPONENT_OWNER stays the
// parent Rasm.Materials.Component and folds Steel.ComponentCatalogue.BuildSteelRows / Steel.ComponentCatalogue.SteelSections
// by the sub-namespace-qualified name. The parent owner types and the Component.Joint StudClass are composed via the usings.
namespace Rasm.Materials.Component.Steel;

// --- [TYPES] -------------------------------------------------------------------------------
// The thin-walled topology that selects the SteelStiffness plastic/torsion/warping/shear closed form AND the AISC
// Table B4.1 slenderness model: an OPEN shape (I/channel/tee/angle) carries a positive warping constant and a
// web-vs-flange shear split; a CLOSED hollow (HSS/pipe) has engineering-zero warping and a perimeter shear; a SOLID
// bar (the rare round/square stock) the same. The topology is the ONE discriminant the stiffness fold and the
// classifier branch on — never a per-class duplicate formula.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelTopology {
    public static readonly SteelTopology Open   = new("open");     // I/channel/tee/angle thin-walled — positive Iw, web+flange shear
    public static readonly SteelTopology Closed = new("closed");   // HSS-rect/round/pipe hollow — Iw≈0, perimeter shear
    public static readonly SteelTopology Solid  = new("solid");    // round/square bar stock — Iw≈0, gross shear
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelClass {
    public static readonly SteelClass IShape      = new("i-shape", topology: SteelTopology.Open, ifcSubtype: "IfcIShapeProfileDef");
    public static readonly SteelClass UShape      = new("u-shape", topology: SteelTopology.Open, ifcSubtype: "IfcUShapeProfileDef");
    public static readonly SteelClass LShape      = new("l-shape", topology: SteelTopology.Open, ifcSubtype: "IfcLShapeProfileDef");
    public static readonly SteelClass DoubleAngle = new("double-angle", topology: SteelTopology.Open, ifcSubtype: "IfcArbitraryClosedProfileDef");
    public static readonly SteelClass HssRect     = new("hss-rect", topology: SteelTopology.Closed, ifcSubtype: "IfcRectangleHollowProfileDef");
    public static readonly SteelClass HssRound    = new("hss-round", topology: SteelTopology.Closed, ifcSubtype: "IfcCircleHollowProfileDef");
    public static readonly SteelClass Tee         = new("tee", topology: SteelTopology.Open, ifcSubtype: "IfcTShapeProfileDef");
    public static readonly SteelClass Composite   = new("composite", topology: SteelTopology.Open, ifcSubtype: "IfcArbitraryClosedProfileDef");
    public static readonly SteelClass ColdFormed  = new("cold-formed", topology: SteelTopology.Open, ifcSubtype: "IfcUShapeProfileDef");
    public SteelTopology Topology { get; }
    public string IfcSubtype { get; }
    public bool IsComposite => this == Composite;
    public bool IsColdFormed => this == ColdFormed;

    // The published AISC/EN family taxonomy IS the discriminant: fold AmericanShape (13) / EuropeanShape (25) onto the
    // SteelClass set TOTALLY rather than minting a parallel section-family enum (api-vividorange-profiles-catalogue [02]).
    // An unrecognized family rails ComponentFault.Family — never a silent `_ => IShape` mis-classifying a tee/angle/hollow
    // family as an I-shape (the deleted runtime-silent default arm). The HSS arm maps to the RECTANGULAR default; the
    // round/rectangular hollow split (a round HSS carries the SAME AmericanShape.HSS as a rectangular one) is the
    // GEOMETRY-driven ComponentCatalogue.ClassOf ICircularHollow/IRectangularHollow discriminant, never this enum.
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

    // TOTAL over the 25 EN families (api-vividorange-profiles-catalogue [02]): the H/I families (UB/UC/HEA/HEB/HEM/.../IPE)
    // → i-shape, the channel families (UPE/PFC/UPN/U/CH) → u-shape. EN 10365 publishes NO angle/hollow/tee European family,
    // so these two arms exhaust the 25 members — the `_` arm is the defensive unrecognized-family rail, not a dropped family.
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
[SmartEnum]
public sealed partial class CompactnessClass {
    public static readonly CompactnessClass Compact    = new();
    public static readonly CompactnessClass Noncompact = new();
    public static readonly CompactnessClass Slender    = new();
}

// The structural-steel grade band bound to its VividOrange.Materials EnSteelGrade — the design yield the LRFD Capacity
// reads from EnSteelFactory.CreateLinearElastic (the EN 1993-1-1 Table 3.1 f_y by grade × delivery condition × thickness-
// band) rather than a caller-supplied raw double, so a steel member's design yield is the registered grade DATA citing
// En1993, never hand-keyed. The AISC bands (a36/a992/a572) carry None and fall back to their spec-nominal yield (no .NET
// package owns AISC grades — the design-codes-hand-rolled-in-fence caveat); the EN bands resolve EnSteelFactory through Delivery:
// the AR (as-rolled, EN 10025-2) Table 3.1 sub-table holds S235/S275/S355/S450, the N (normalized, EN 10025-3) and M
// (thermomechanical, EN 10025-4) sub-tables hold S420/S460 — so S420/S460 carry Delivery.N (the default AR spec rails them).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelGrade {
    public static readonly SteelGrade A36  = new("a36",  nominalYieldMpa: 250.0, enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade A992 = new("a992", nominalYieldMpa: 345.0, enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade A572 = new("a572", nominalYieldMpa: 345.0, enGrade: None, delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S235 = new("s235", nominalYieldMpa: 235.0, enGrade: Some(EnSteelGrade.S235), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S275 = new("s275", nominalYieldMpa: 275.0, enGrade: Some(EnSteelGrade.S275), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S355 = new("s355", nominalYieldMpa: 355.0, enGrade: Some(EnSteelGrade.S355), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S420 = new("s420", nominalYieldMpa: 420.0, enGrade: Some(EnSteelGrade.S420), delivery: EnSteelDeliveryCondition.N);
    public static readonly SteelGrade S450 = new("s450", nominalYieldMpa: 440.0, enGrade: Some(EnSteelGrade.S450), delivery: EnSteelDeliveryCondition.AR);
    public static readonly SteelGrade S460 = new("s460", nominalYieldMpa: 460.0, enGrade: Some(EnSteelGrade.S460), delivery: EnSteelDeliveryCondition.N);
    public double NominalYieldMpa { get; }
    public Option<EnSteelGrade> EnGrade { get; }
    public EnSteelDeliveryCondition Delivery { get; }

    // The thickness-banded design yield: an EN grade builds the EnSteelMaterial, sets its Specification.DeliveryCondition
    // to the band's Delivery so CreateLinearElastic reaches the AR/N/M Table 3.1 sub-table that HOLDS the grade (the default
    // AR spec carries only S235/S275/S355/S450, so S420/S460 carry Delivery.N), reads the ≤40 mm vs 40-80 mm f_y the factory's
    // elementThickness selects, and traps the EN derivation throw onto ComponentFault. An AISC band returns its spec-nominal
    // yield. The flange thickness the section reports is the band selector.
    public Fin<double> YieldMpa(double elementThicknessMm, NationalAnnex annex, Op key) =>
        EnGrade.Match(
            Some: g => Try(() => {
                    EnSteelMaterial material = new(g, annex);
                    material.Specification.DeliveryCondition = Delivery;
                    return EnSteelFactory.CreateLinearElastic(material, Length.FromMillimeters(elementThicknessMm)).Strength.Megapascals;
                }).ToFin()
                .MapFail(e => ComponentFault.Family(key, $"<en-steel-grade:{g}:{annex}:{Delivery}:{e.Message}>")),
            None: () => Fin.Succ(NominalYieldMpa));
}

// --- [MODELS] ------------------------------------------------------------------------------
// Dims read off the family geometry interface as UnitsNet.Length (carried in their native published unit), then
// coerced once to SI millimetres at SectionReader.Read. Fillet is present only on IIParallelFlange (0 elsewhere); the
// DoubleL back-to-back spacing rides BackToBackMm (IDoubleAngle.BackToBackDistance, 0 for a single shape) onto the
// Bim ProfileDims.BackToBackMm round-trip. These dims drive the SteelStiffness plastic/torsion/warping/shear closed forms.
public readonly record struct SectionDims(double DepthMm, double FlangeWidthMm, double WebThicknessMm, double FlangeThicknessMm, double FilletMm, double BackToBackMm);

// The catalogued/parametric steel section row — the payload of the component#COMPONENT_OWNER ComponentSection.Steel
// cross-section FIELD: the canonical ComputedSection (the twenty-field receipt every family shares), the SteelClass, the
// published dimensional columns, the IFC standard, plus the composite/cold-formed augmentation. There is NO parallel narrow
// section receipt — the structural columns ARE the component#COMPONENT_OWNER ComputedSection the [03] component#COMPONENT_RESOLUTION
// data-join caches and the SteelDesign LRFD reads.
public sealed record SteelShape(
    ComponentId Id,
    SteelClass Class,
    ComputedSection Section,
    SectionDims Dims,
    ComponentStandard Standard,
    Option<CompositeDetail> Composite = default,
    Option<ColdFormedDetail> ColdFormed = default);

// The AISC 360 LRFD design receipt over the canonical ComputedSection. TorsionalNmm is the §H3.1 design torsional
// resistance φT·Fcr·C (φT 0.90, Fcr the shear-buckling critical stress, C the HSS torsional constant the closed section
// carries) — positive for a CLOSED HSS/pipe the St-Venant constant J/c yields, engineering-zero for an OPEN thin-walled
// shape whose §H3.3 non-HSS torsion is a warping-normal-stress check the unified rail does not model, so an open W-shape
// lifts TorsionalNmm 0 and a torsion demand on it surfaces as the governing over-ratio. This is the column the
// Component/capacity#SECTION_CAPACITY SteelLrfd lift reads onto SectionCapacity.SteelLrfd.TorsionalKnm (one source, no
// parallel parameter — the resistance is a capacity column, never a re-passed lift argument).
public readonly record struct DesignCapacity(double FlexuralNmm, double CompressionN, double ShearN, double TorsionalNmm, CompactnessClass Classification, double Slenderness);

// AISC 360 Chapter I composite-action detail: the concrete slab over the steel core, plus the shear-stud reference into
// the Component/joint#JOINT_FAMILY StudClass vocabulary the composite shear interface develops. The horizontal shear
// ΣQn the composite plastic-moment couple is capped at is StudClass.SteelShearKn × StudsPerMetre (the one stud vocabulary's
// per-stud AISC 360-22 Eq I8-1 cap), NEVER a re-derived stud shear (the joint#JOINT_FAMILY owns StudClass.SteelShearKn).
public readonly record struct CompositeDetail(
    PositiveMagnitude SlabEffectiveWidthMm,
    PositiveMagnitude SlabDepthMm,
    double ConcreteFcMpa,
    StudClass Stud,
    int StudsPerMetre);

// AISI S100 effective-width detail: a cold-formed section reduces to an effective section under local buckling, the
// effective-section-modulus Seff/S ratio the local-buckling reduction yields over the gross/effective width.
public readonly record struct ColdFormedDetail(
    PositiveMagnitude GrossWidthMm,
    PositiveMagnitude EffectiveWidthMm,
    PositiveMagnitude CornerRadiusMm,
    PositiveMagnitude DesignThicknessMm,
    double EffectiveSectionModulusRatio);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The plastic/torsion/warping/shear AND asymmetric-section LTB columns the elastic VividOrange polygon integral CANNOT
// yield — derived from the admitted dimensional columns per SteelClass/SteelTopology, so the steel ComputedSection is the
// SAME twenty-field receipt ParametricSection produces for the rectangle families (component#COMPONENT_OWNER): ZxMm3/ZyMm3
// the plastic moduli, JMm4 the St-Venant torsion constant, IwMm6 the EN 1993-1-1 §6.3.2 warping constant (POSITIVE for the
// OPEN topologies the rectangle families leave engineering-zero — the LTB input the bare J cannot supply), AvyMm2/AvzMm2
// the both-axis shear areas, AND ShearCentreYMm/ShearCentreZMm/MonosymmetryFactor the asymmetric-section LTB columns the
// EN 1993-1-1 §6.3.2 GENERAL route needs for a channel/tee/angle (engineering-zero for a doubly-symmetric I/HSS/solid, so
// the seam IsDoublySymmetric reads them as symmetric — non-zero ONLY for the singly-symmetric open shapes whose shear
// centre falls off the centroid). One topology-keyed fold over the geometry, never a per-class transcribed literal and
// never the duplicate iShape/uShape arms a prior flat dispatch carried.
public static class SteelStiffness {
    // The eight derived columns the ComputedSection reserves for the steel family, over the published dims — the five
    // stiffness/shear columns plus the three asymmetric-section LTB columns (the centroid→shear-centre offsets Scy/Scz
    // and the EN 1993-1-1 NCCI SN030 mono-symmetry factor βy). A doubly-symmetric shape returns 0/0/0 for the trio.
    public static (double Zx, double Zy, double J, double Iw, double Avy, double Avz, double Scy, double Scz, double By) Derive(SteelClass cls, SectionDims d, double sx, double sy) =>
        cls.Topology.Switch(
            // OPEN thin-walled: the rectangular-component plastic moduli, the open thin-walled St-Venant J = Σbt³/3, the
            // warping constant from the per-class web/flange decomposition, the AISC §G web-vs-flange shear areas, AND the
            // shear-centre offset + βy mono-symmetry the singly-symmetric channel/tee/angle carry (the I-shape's are zero).
            open:   _ => OpenShape(cls, d, sx, sy),
            // CLOSED hollow (HSS-rect/round/pipe): the closed-form plastic modulus, the thin-walled-tube torsion J = 4A²/∮(ds/t),
            // engineering-zero warping (a closed cell does not warp), the perimeter shear area Av = 2·h·t (rect) / A/2 (round),
            // and a zero shear-centre offset + zero βy (every closed HSS is doubly-symmetric, the shear centre AT the centroid).
            closed: _ => ClosedShape(cls, d, sx),
            // SOLID bar stock: the b·h²/4 plastic moduli, the rectangle/round St-Venant torsion, zero warping, gross shear,
            // and a zero shear-centre offset + zero βy (a solid rectangle/round is doubly-symmetric).
            solid:  _ => SolidShape(d));

    // The tuple shear slots are (AvMajor, AvMinor) — Avy the MAJOR-axis (web) shear area, Avz the MINOR-axis (flange)
    // shear area — matching the seam SectionProperties.AvY(major)/AvZ(minor) the Projection/component#COMPONENT_PROJECTOR
    // SeamSection maps ComputedSection.AvyMm2->AvY, AvzMm2->AvZ; the §G design web shear φVn therefore reads AvyMm2. The
    // last three slots are (Scy, Scz, By): a doubly-symmetric I-shape's shear centre coincides with its centroid (0/0/0),
    // while a channel carries a minor-axis shear-centre offset Scz = e (the web-to-shear-centre distance the closed-form
    // ho²·tf·bf³/(4·Iw) yields, the EN 1993-1-1 §6.3.2 general-route input) and a tee/angle carry the offset to the
    // flange/heel and the SN030 βy ≈ 0.9·hₒ·(2·ψf − 1) singly-symmetric parameter (ψf the flange-asymmetry ratio).
    static (double, double, double, double, double, double, double, double, double) OpenShape(SteelClass cls, SectionDims d, double sx, double sy) {
        double bf = d.FlangeWidthMm, tf = d.FlangeThicknessMm, tw = d.WebThicknessMm, h = Math.Max(0.0, d.DepthMm - 2.0 * tf), depth = d.DepthMm, ho = depth - tf;
        double zxI = bf * tf * (depth - tf) + 0.25 * tw * h * h;                 // I/channel rectangular-component plastic modulus Yy
        double zyI = 0.5 * tf * bf * bf + 0.25 * (depth - 2.0 * tf) * tw * tw;   // weak-axis plastic modulus Zz (two flanges + web)
        double jOpen = (2.0 * bf * tf * tf * tf + h * tw * tw * tw) / 3.0;       // open thin-walled St-Venant J = Σ b·t³/3
        double iwI = tf * bf * bf * bf * (depth - tf) * (depth - tf) / 24.0;     // doubly-symmetric I warping Iw = Iy·hₒ²/4 ≈ tf·bf³·hₒ²/24
        double avMajorWeb = depth * tw, avMinorFlange = 5.0 / 3.0 * bf * tf;     // AISC §G major-axis web shear d·tw, minor-axis flange shear ⁵⁄₃·bf·tf
        double channelE = ho * ho * tf * bf * bf * bf / Math.Max(1.0, 12.0 * iwI);  // channel web→shear-centre offset e (closed form over the half-I warping)
        double teeScz = Math.Max(0.0, depth - tf) * 0.5;                          // tee/angle shear centre at the flange/heel: offset ≈ half the stem from the centroid
        double teeBy = 0.9 * ho;                                                  // SN030 βy for a tee (one flange only, ψf = 1): the singly-symmetric mono-symmetry parameter
        return cls.Switch(
            iShape:      _ => (zxI, zyI, jOpen, iwI, avMajorWeb, avMinorFlange, 0.0, 0.0, 0.0),                  // doubly-symmetric: shear centre AT centroid, βy = 0
            uShape:      _ => (zxI, zyI, jOpen, 0.5 * iwI, avMajorWeb, avMinorFlange, 0.0, channelE, 0.0),       // channel: monosymmetric about the major axis — minor-axis shear-centre offset, βy = 0 (symmetric in the bending plane)
            tee:         _ => (1.7 * sx, 0.85 * sy, bf * tf * tf * tf / 3.0 + h * tw * tw * tw / 3.0, 0.0, depth * tw, 5.0 / 3.0 * bf * tf, 0.0, teeScz, teeBy),  // tee: stem web is the major shear, no warping couple, singly-symmetric βy
            lShape:      _ => (1.5 * sx, 1.5 * sy, (bf + depth) * tw * tw * tw / 3.0, 0.0, depth * tw, bf * tw, 0.5 * bf, teeScz, 0.5 * teeBy),                    // angle: shear centre at the heel (both legs), torsion-only, no warping, reduced βy
            doubleAngle: _ => (1.6 * sx, 1.5 * sy, 2.0 * (bf + depth) * tw * tw * tw / 3.0, 0.0, 2.0 * depth * tw, 2.0 * bf * tw, 0.0, teeScz, teeBy),             // 2L: symmetric about the connected axis (Scy = 0), the unconnected axis singly-symmetric
            composite:   _ => (zxI, zyI, jOpen, iwI, avMajorWeb, avMinorFlange, 0.0, 0.0, 0.0),                  // steel core is its I-shape; slab adds capacity not stiffness here
            coldFormed:  _ => (sx, sy, h * tw * tw * tw / 3.0, 0.0, depth * tw, depth * tw, 0.0, channelE, 0.0));// thin-gauge channel-stud: monosymmetric like a channel, effective-section governs
    }

    static (double, double, double, double, double, double, double, double, double) ClosedShape(SteelClass cls, SectionDims d, double sx) {
        double b = d.FlangeWidthMm, h = d.DepthMm, t = d.WebThicknessMm;
        return cls.Switch(
            hssRect:  _ => (0.25 * b * h * h - 0.25 * Math.Max(0.0, b - 2.0 * t) * Math.Pow(Math.Max(0.0, h - 2.0 * t), 2.0),
                            0.25 * h * b * b - 0.25 * Math.Max(0.0, h - 2.0 * t) * Math.Pow(Math.Max(0.0, b - 2.0 * t), 2.0),
                            ClosedRectJ(b, h, t), 0.0, 2.0 * h * t, 2.0 * b * t, 0.0, 0.0, 0.0),                  // HSS-rect: doubly-symmetric — major = the 2 webs 2·h·t, minor = the 2 flanges 2·b·t, zero offsets/βy
            hssRound: _ => RoundTube(h, t),                                                                          // round-HSS/pipe: D=h, wall=t
            _         => (sx, sx, ClosedRectJ(b, h, t), 0.0, 2.0 * h * t, 2.0 * b * t, 0.0, 0.0, 0.0));
    }

    static (double, double, double, double, double, double, double, double, double) RoundTube(double od, double t) {
        double ri = Math.Max(0.0, od / 2.0 - t), ro = od / 2.0;
        double zRound = (Math.Pow(od, 3.0) - Math.Pow(2.0 * ri, 3.0)) / 6.0;                 // round-tube plastic modulus
        double jRound = Math.PI * 0.5 * (Math.Pow(ro, 4.0) - Math.Pow(ri, 4.0));             // polar torsion 2·I for a closed tube
        double area = Math.PI * (ro * ro - ri * ri);
        return (zRound, zRound, jRound, 0.0, area * 0.5, area * 0.5, 0.0, 0.0, 0.0);          // axisymmetric: Zx=Zy, Av=A/2, doubly-symmetric (zero offsets/βy)
    }

    // Closed rectangular thin-walled tube St-Venant torsion J = 4·A_m²·t / perimeter_m (Bredt), A_m the mid-wall enclosed area.
    static double ClosedRectJ(double b, double h, double t) {
        double bm = Math.Max(0.0, b - t), hm = Math.Max(0.0, h - t);
        return bm <= 0.0 || hm <= 0.0 ? 0.0 : 4.0 * bm * bm * hm * hm * t / (2.0 * (bm + hm));
    }

    static (double, double, double, double, double, double, double, double, double) SolidShape(SectionDims d) {
        double w = d.FlangeWidthMm, h = d.DepthMm, lng = Math.Max(w, h), sht = Math.Min(w, h), area = w * h;
        double j = lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));   // Roark solid-rectangle J
        return (w * h * h / 4.0, h * w * w / 4.0, j, 0.0, area, area, 0.0, 0.0, 0.0);                                 // doubly-symmetric bar: zero offsets/βy
    }
}

// The ONE VividOrange admission boundary: an ICatalogue (a polymorphic IProfile carrying published UnitsNet.Length
// geometry) → the CANONICAL component#COMPONENT_OWNER ComputedSection (the twenty-field receipt every family shares),
// every UnitsNet quantity coerced to an SI-millimetre scalar and admitted once into the kernel PositiveMagnitude columns.
// There is NO parallel narrow SteelSection — steel produces the SAME ComputedSection the rectangle families do.
public static class SectionReader {
    // The canonical ComputedSection AND the published dims it was built from, computed ONCE — the catalogue reads both
    // onto the SteelShape (Section + Dims) without a second family-cast pass (the deleted duplicate-dispatch form).
    public static Fin<(ComputedSection Section, SectionDims Dims)> Read(ICatalogue catalogue, SteelClass cls, Op key) =>
        from dims in ReadDims(catalogue, key)
        let props = new SectionProperties((IProfile)catalogue)   // solver consumes the polymorphic IProfile — no family cast for the elastic columns
        from sx in key.AcceptValidated<PositiveMagnitude>(candidate: props.ElasticSectionModulusYy.CubicMillimeters)
        from sy in key.AcceptValidated<PositiveMagnitude>(candidate: props.ElasticSectionModulusZz.CubicMillimeters)
        let stiff = SteelStiffness.Derive(cls, dims, sx.Value, sy.Value)
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: props.Area.SquareMillimeters)
        from ix in key.AcceptValidated<PositiveMagnitude>(candidate: props.MomentOfInertiaYy.MillimetersToTheFourth)
        from iy in key.AcceptValidated<PositiveMagnitude>(candidate: props.MomentOfInertiaZz.MillimetersToTheFourth)
        from rx in key.AcceptValidated<PositiveMagnitude>(candidate: props.RadiusOfGyrationYy.Millimeters)
        from ry in key.AcceptValidated<PositiveMagnitude>(candidate: props.RadiusOfGyrationZz.Millimeters)
        from zx in key.AcceptValidated<PositiveMagnitude>(candidate: stiff.Zx)
        from zy in key.AcceptValidated<PositiveMagnitude>(candidate: stiff.Zy)
        from jj in key.AcceptValidated<PositiveMagnitude>(candidate: stiff.J)
        from avy in key.AcceptValidated<PositiveMagnitude>(candidate: stiff.Avy)
        from avz in key.AcceptValidated<PositiveMagnitude>(candidate: stiff.Avz)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: dims.DepthMm)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: dims.FlangeWidthMm)
        from perim in key.AcceptValidated<PositiveMagnitude>(candidate: props.Perimeter.Millimeters)
        // IwMm6 is the EN 1993-1-1 §6.3.2 warping constant (positive for OPEN topologies, 0 for CLOSED/SOLID — the column
        // component#COMPONENT_OWNER ComputedSection reserves for an OPEN thin-walled steel shape); AxisDistanceMm 0.0 (non-RC);
        // the three asymmetric-section LTB columns (stiff.Scy/Scz the centroid→shear-centre offsets, stiff.By the SN030 βy) are
        // plain signed doubles like Iw — non-zero ONLY for the singly-symmetric channel/tee/angle, zero for a doubly-symmetric
        // I/HSS/solid (so the seam IsDoublySymmetric reads them symmetric) — admitted directly, never an AcceptValidated rail.
        select (new ComputedSection(area, ix, iy, sx, sy, rx, ry, zx, zy, jj, stiff.Iw, avy, avz, depth, width, perim, AxisDistanceMm: 0.0, ShearCentreYMm: stiff.Scy, ShearCentreZMm: stiff.Scz, MonosymmetryFactor: stiff.By), dims);

    // The ONE family geometry read: the dimensional columns the AISC Table B4.1 Classify slenderness AND the SteelStiffness
    // closed forms read come off the family interface the AmericanShape/EuropeanShape discriminant selects, as UnitsNet.Length
    // in the native published unit (.Millimeters converts). IIParallelFlange (W/HEA/IPE/HP, with fillet) is tested before the
    // II base (the S/HP taper-flange shapes whose IITaperFlange : II carries no fillet); HSS rides the rectangle envelope +
    // wall thickness, round-HSS the diameter + wall. An unmatched catalogue geometry interface rails ComponentFault.Family —
    // never a fabricated (1,1,1,1) sentinel passing every PositiveMagnitude admission to seed a degenerate section.
    public static Fin<SectionDims> ReadDims(ICatalogue catalogue, Op key) => catalogue switch {
        IIParallelFlange i => Fin.Succ(new SectionDims(i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, i.FilletRadius.Millimeters, 0.0)),
        II i               => Fin.Succ(new SectionDims(i.Height.Millimeters, i.Width.Millimeters, i.WebThickness.Millimeters, i.FlangeThickness.Millimeters, 0.0, 0.0)),
        IDoubleAngle da    => Fin.Succ(new SectionDims(da.Height.Millimeters, da.Width.Millimeters, da.WebThickness.Millimeters, da.FlangeThickness.Millimeters, 0.0, da.BackToBackDistance.Millimeters)),
        IChannel c         => Fin.Succ(new SectionDims(c.Height.Millimeters, c.Width.Millimeters, c.WebThickness.Millimeters, c.FlangeThickness.Millimeters, 0.0, 0.0)),
        ITee t             => Fin.Succ(new SectionDims(t.Height.Millimeters, t.Width.Millimeters, t.WebThickness.Millimeters, t.FlangeThickness.Millimeters, 0.0, 0.0)),
        IAngle an          => Fin.Succ(new SectionDims(an.Height.Millimeters, an.Width.Millimeters, an.WebThickness.Millimeters, an.FlangeThickness.Millimeters, 0.0, 0.0)),
        ICircularHollow ch when catalogue is IHollowStructuralSection h    => Fin.Succ(new SectionDims(ch.Diameter.Millimeters, ch.Diameter.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0, 0.0)),
        IRectangularHollow rh when catalogue is IHollowStructuralSection h => Fin.Succ(new SectionDims(rh.Height.Millimeters, rh.Width.Millimeters, h.Thickness.Millimeters, h.Thickness.Millimeters, 0.0, 0.0)),
        _ => Fin.Fail<SectionDims>(ComponentFault.Family(key, $"<catalogue-geometry-interface-unsupported:{catalogue.Label}>")),
    };
}

// The AISC 360 design projections over the CANONICAL ComputedSection — the classification and the LRFD capacity read the
// twenty-field receipt's real Iw (LTB), Avy (major-axis web shear), Zx/Zy (plastic), Rx/Ry (buckling), never a re-minted dimension.
// The capacity is lifted into the unified Component/capacity#SECTION_CAPACITY rail by SectionCapacityResolver.SteelLrfd.
public static class SteelDesign {
    const double φb = 0.90, φc = 0.90, φv = 0.90, E = 200_000.0;

    // AISC 360 Table B4.1 PER-TOPOLOGY slenderness over the SteelShape's published dims: an OPEN I/channel reads flange
    // λpf/λrf + web λpw/λrw over √(E/Fy), a CLOSED HSS-rect reads the wall b/t, a round-HSS the D/t — never one I-shape
    // model on a round pipe (the deleted misapplied form). The round-HSS arm lives ONLY under the closed topology it
    // belongs to, never an unreachable arm under open.
    public static CompactnessClass Classify(SteelShape shape, double yieldMpa) {
        SectionDims d = shape.Dims;
        double r = Math.Sqrt(E / yieldMpa);
        return shape.Class.Topology.Switch(
            open:   _ => OpenIClass(d.FlangeWidthMm, d.FlangeThicknessMm, d.DepthMm, d.WebThicknessMm, r),
            closed: _ => shape.Class == SteelClass.HssRound
                ? Round(d.DepthMm, d.WebThicknessMm, yieldMpa)
                : Verdict(d.FlangeWidthMm / Math.Max(d.WebThicknessMm, double.Epsilon), 1.12 * r, 1.40 * r),   // HSS-rect wall b/t: λp 1.12√(E/Fy), λr 1.40
            solid:  _ => CompactnessClass.Compact);
    }

    static CompactnessClass OpenIClass(double bf, double tf, double d, double tw, double r) {
        double flange = bf / (2.0 * tf), web = (d - 2.0 * tf) / tw;
        bool slender = flange > 1.0 * r || web > 5.70 * r;
        bool compact = flange <= 0.38 * r && web <= 3.76 * r;
        return slender ? CompactnessClass.Slender : compact ? CompactnessClass.Compact : CompactnessClass.Noncompact;
    }

    // Round-HSS/pipe wall: AISC Table B4.1 D/t against 0.07·E/Fy (compact) and 0.31·E/Fy (slender) — the dimensionless E/Fy form.
    static CompactnessClass Round(double od, double t, double fy) {
        double dt = od / Math.Max(t, double.Epsilon), eFy = E / fy;
        return dt > 0.31 * eFy ? CompactnessClass.Slender : dt <= 0.07 * eFy ? CompactnessClass.Compact : CompactnessClass.Noncompact;
    }

    static CompactnessClass Verdict(double ratio, double λp, double λr) =>
        ratio > λr ? CompactnessClass.Slender : ratio <= λp ? CompactnessClass.Compact : CompactnessClass.Noncompact;

    // AISC 360 Chapters F/E/G LRFD over the canonical ComputedSection: φMn = φb·Mn with the LTB reduction reading the REAL
    // warping Iw (Lr the inelastic-LTB limit derived from Iw/J, not omitted), φPn = φc·Fcr·A over the WEAK-axis flexural-
    // buckling stress (the governing radius min(RxMm, RyMm) read off the ComputedSection's both-axis columns, never the
    // strong-axis-only Math.Sqrt(Ix/A)), φVn = φv·0.6·Fy·Aw over the MAJOR-axis web shear AvyMm2 (the seam AvY); the Composite arm runs the
    // AISC 360 Ch I plastic composite Mn capped at the StudClass ΣQn, the ColdFormed arm the AISI S100 Mn = Seff·Fy.
    public static DesignCapacity Capacity(SteelShape shape, double yieldMpa, double unbracedLengthMm, double effectiveLengthMm) {
        ComputedSection s = shape.Section;
        double rWeak = Math.Min(s.RxMm.Value, s.RyMm.Value);   // weak-axis radius governs column flexural buckling
        double λc = effectiveLengthMm / Math.Max(rWeak, double.Epsilon);
        double Fe = Math.PI * Math.PI * E / (λc * λc);
        double Fcr = Fe >= 0.44 * yieldMpa ? yieldMpa * Math.Pow(0.658, yieldMpa / Fe) : 0.877 * Fe;
        double Mp = yieldMpa * s.ZxMm3.Value;
        double rolledMn = LateralTorsionalMn(s, unbracedLengthMm, yieldMpa, Mp);
        double Mn = shape.Composite.Match(
            Some: c => CompositeMn(c, s, yieldMpa),
            None: () => shape.ColdFormed.Match(Some: cf => yieldMpa * s.SxMm3.Value * cf.EffectiveSectionModulusRatio, None: () => rolledMn));
        return new DesignCapacity(
            FlexuralNmm: φb * Mn,
            CompressionN: φc * Fcr * s.AreaMm2.Value,
            ShearN: φv * 0.6 * yieldMpa * s.AvyMm2.Value,   // §G major-axis web shear Aw — the seam AvY(major), NOT AvzMm2(minor flange)
            TorsionalNmm: TorsionalResistance(shape, yieldMpa),
            Classification: Classify(shape, yieldMpa),
            Slenderness: λc);
    }

    // AISC 360 §H3.1 design torsional resistance φTn = φT·Fcr·C for a CLOSED HSS/pipe — the only topology §H3.1 owns a
    // single-scalar resistance for. C is the HSS torsional constant; the canonical ComputedSection already carries the
    // St-Venant constant JMm4, so the elastic torsional section modulus is C = J/c with c the outer half-dimension
    // (DepthMm/2), grounding C in the section geometry rather than a re-derived wall integral. Fcr per §H3.1 is the
    // shear-buckling critical stress capped at 0.6·Fy; for a compact HSS wall (the catalogued sections here) the 0.6·Fy
    // limit governs, so Fcr = 0.6·Fy is the conservative compact-wall resistance the unified rail reads. An OPEN
    // thin-walled shape returns 0: §H3.3 non-HSS torsion is a warping-normal-stress interaction, not a single resistance
    // the Component/capacity#SECTION_CAPACITY ratio fold can divide against — so an open W-shape's TorsionalNmm is 0 and a
    // torsion demand on it surfaces as the governing over-ratio (the CONSUMED-action discipline) rather than a silent pass.
    static double TorsionalResistance(SteelShape shape, double yieldMpa) {
        ComputedSection s = shape.Section;
        double closedForm = φv * 0.6 * yieldMpa * s.JMm4.Value / Math.Max(0.5 * s.DepthMm.Value, double.Epsilon);   // φTn = φT·Fcr·C, Fcr=0.6·Fy, C=J/c, φT=φv=0.90 baked in (the SAME φ-in-field convention as FlexuralNmm/ShearN)
        return shape.Class.Topology.Switch(
            closed: _ => closedForm,
            open:   _ => 0.0,
            solid:  _ => closedForm);   // a solid bar's St-Venant torsion modulus J/c, same φTn form
    }

    // AISC 360 §F2 lateral-torsional buckling reading the REAL warping Iw: Lp = 1.76·ry·√(E/Fy), the elastic-LTB modulus
    // rts ≈ √(√(Iy·Iw)/Sx), Lr the inelastic limit, and the linear Mp→0.7·Fy·Sx interpolation between — the bare-J prior form
    // (no Iw) is the deleted approximation. A short unbraced length yields Mp; beyond Lr the elastic-LTB Fcr·Sx governs.
    static double LateralTorsionalMn(ComputedSection s, double Lb, double Fy, double Mp) {
        double ry = s.RyMm.Value, sx = s.SxMm3.Value, iy = s.IyMm4.Value, iw = s.IwMm6, jj = s.JMm4.Value, depth = s.DepthMm.Value;
        double Lp = 1.76 * ry * Math.Sqrt(E / Fy);
        double rts = iw > 0.0 ? Math.Sqrt(Math.Sqrt(iy * iw) / Math.Max(sx, double.Epsilon)) : ry;       // §F2 effective radius (open shapes with warping)
        double c = 1.0, ho = depth;                                                                       // doubly-symmetric c=1; ho the flange-centroid distance, conservatively the section depth
        double term = jj * c / Math.Max(sx * ho, double.Epsilon);
        double Lr = 1.95 * rts * E / (0.7 * Fy) * Math.Sqrt(term + Math.Sqrt(term * term + 6.76 * Math.Pow(0.7 * Fy / E, 2.0)));
        return Lb <= Lp
            ? Mp
            : Lb <= Lr
                ? Math.Max(0.7 * Fy * sx, Mp - (Mp - 0.7 * Fy * sx) * Math.Clamp((Lb - Lp) / Math.Max(Lr - Lp, double.Epsilon), 0.0, 1.0))
                : Math.Min(Mp, FcrLtb(Lb, rts, jj, c, sx, ho) * sx);   // elastic LTB beyond Lr
    }

    // The §F2 elastic-LTB critical stress beyond Lr (Cb=1 conservative): Fcr = π²E/(Lb/rts)² · √(1 + 0.078·(J·c)/(Sx·ho)·(Lb/rts)²).
    static double FcrLtb(double Lb, double rts, double jj, double c, double sx, double ho) {
        double slender = Lb / Math.Max(rts, double.Epsilon);
        return Math.PI * Math.PI * E / (slender * slender) * Math.Sqrt(1.0 + 0.078 * jj * c / Math.Max(sx * ho, double.Epsilon) * slender * slender);
    }

    // AISC 360 Eq C-I3: the fully-OR-partially-composite plastic moment — the steel yields in tension (As·Fy) balanced by the
    // concrete compression block (0.85·f'c·b·a), the moment the couple about the slab; capped at the stud horizontal shear
    // ΣQn the Component/joint#JOINT_FAMILY StudClass.SteelShearKn × StudsPerMetre develops (partial composite when ΣQn < As·Fy),
    // never a re-derived stud shear (joint#JOINT_FAMILY owns StudClass.SteelShearKn).
    static double CompositeMn(CompositeDetail c, ComputedSection s, double yieldMpa) {
        double tSteel = s.AreaMm2.Value * yieldMpa;                                          // As·Fy steel tension
        double cConcMax = 0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value * c.SlabDepthMm.Value;   // full slab compression block
        double sumQn = c.Stud.SteelShearKn * Math.Max(0, c.StudsPerMetre) * 1e3;             // ΣQn from the joint StudClass cap (kN → N per metre)
        double horizShear = Math.Min(Math.Min(tSteel, cConcMax), sumQn);                     // governing horizontal shear — ΣQn=0 ⇒ non-composite ⇒ Mn→0
        double a = Math.Min(c.SlabDepthMm.Value, horizShear / Math.Max(0.85 * c.ConcreteFcMpa * c.SlabEffectiveWidthMm.Value, double.Epsilon));
        double leverArm = 0.5 * s.DepthMm.Value + c.SlabDepthMm.Value - 0.5 * a;
        return horizShear * leverArm;
    }
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The bounded standards body the section cites (the component#COMPONENT_OWNER ComponentAuthority [SmartEnum] row, region
    // carried by the ComponentStandard token): ComponentAuthority.Aisc for the AISC Shapes Database v16.0 American sections,
    // ComponentAuthority.En for the EN 10365:2017 European sections — never a free authority string.
    static readonly ComponentStandard Aisc = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Aisc);
    static readonly ComponentStandard En   = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);

    // The seed selects identities from the registered database (2299 American / 558 European); a new section is one enum
    // value added here, its full geometry and computed properties already behind the CatalogueFactory singleton. The seed
    // spans every AmericanShape family — W/S/HP (i-shape), C/MC (u-shape), L + DoubleL (angle), HSS-rect + the ICircularHollow
    // round HSS (HSS13_375x_625, the round/rectangular split the geometry-driven ClassOf resolves off the shared
    // AmericanShape.HSS family), Pipe (hss-round), WT/ST/MT (tee).
    static readonly Seq<American> AmericanSeed = Seq(
        American.W12x26, American.W14x90, American.W18x50, American.W21x68, American.W24x76,   // i-shape (W, parallel flange)
        American.S24x121, American.HP18x204,                                                   // i-shape (S taper flange via IITaperFlange:II, HP bearing pile)
        American.C15x33_9, American.MC18x45_8,                                                 // u-shape (channel)
        American.L6x6x3over4, American.L4x4x1over2, American.DoubleL6x6x3over4,                 // l-shape + double-angle
        American.HSS8x8x_500, American.HSS6x4x_375, American.HSS13_375x_625,                    // hss-rect (square/rect) + the round HSS the geometry split resolves
        American.Pipe12STD,                                                                    // hss-round (Pipe)
        American.WT9x38, American.WT6x25, American.ST12x53, American.MT6x5_4);                  // tee (WT/ST/MT)

    // The EN families are exclusively i-shape and channel: IPE/HEA(HE…A)/HEB(HE…B)/HEM(HE…M)/IPN/UC → i-shape, UPN → u-shape.
    // The HEM heavy series names HE<size>M (HE300M), NOT HEM300 — the catalogue identity is the section name, HEM is only
    // the EuropeanShape FAMILY value (api-vividorange-profiles-catalogue [02]).
    static readonly Seq<European> EuropeanSeed = Seq(
        European.IPE300, European.IPE450, European.HE300A, European.HE400B, European.HE300M,
        European.IPN200, European.UC254x254x73, European.UPN200);

    // CreateAmerican/CreateEuropean mint the catalogue ONCE; ClassOf resolves the class (hollow by geometry, open by the
    // family fold), and SectionReader.Read returns the ComputedSection AND its dims in one pass (no duplicate family-cast).
    static Fin<SteelShape> AmericanShapeOf(American id, Op key) => ShapeOf(CatalogueFactory.CreateAmerican(id), $"steel.{id.ToString().ToLowerInvariant()}", Aisc, key);
    static Fin<SteelShape> EuropeanShapeOf(European id, Op key) => ShapeOf(CatalogueFactory.CreateEuropean(id), $"steel.{id.ToString().ToLowerInvariant()}", En, key);

    static Fin<SteelShape> ShapeOf(ICatalogue cat, string designation, ComponentStandard standard, Op key) =>
        from cls in ClassOf(cat, key)
        from read in SectionReader.Read(cat, cls, key)
        select new SteelShape(ComponentId.Of(designation), cls, read.Section, read.Dims, standard);

    // The ONE catalogue→class resolution: the round-vs-rectangular hollow split is the ICircularHollow/IRectangularHollow
    // GEOMETRY interface — a round HSS and a rectangular HSS carry the SAME AmericanShape.HSS family value (verified:
    // HSS13_375x_625 is ICircularHollow, HSS8x8x_500 is IRoundedRectangularHollow, both Shape => AmericanShape.HSS), so the
    // geometry is the only round/rect discriminant; the open families dispatch the IAmericanCatalogue.Shape /
    // IEuropeanCatalogue.Shape onto the TOTAL SteelClass.OfShape. An unmatched catalogue kind rails ComponentFault.Family.
    static Fin<SteelClass> ClassOf(ICatalogue catalogue, Op key) => catalogue switch {
        ICircularHollow      => Fin.Succ(SteelClass.HssRound),                          // round HSS + Pipe — geometry wins over the ambiguous HSS family enum
        IRectangularHollow   => Fin.Succ(SteelClass.HssRect),                           // rectangular/square HSS
        IAmericanCatalogue a => SteelClass.OfShape(a.Shape, key),
        IEuropeanCatalogue e => SteelClass.OfShape(e.Shape, key),
        _ => Fin.Fail<SteelClass>(ComponentFault.Family(key, $"<catalogue-not-american-or-european:{catalogue.Label}>")),
    };

    // A composite section is a rolled I-shape steel core augmented with the AISC 360 Ch I slab + Component/joint StudClass
    // shear-stud detail; a cold-formed section is the AISI S100 light-gauge channel with its effective-width reduction. Both
    // ride the SAME ComputedSection over the composite/cold-formed SteelClass discriminant + Option detail on the SteelShape.
    static Fin<SteelShape> CompositeOf(string designation, American core, CompositeDetail detail, Op key) =>
        from baseShape in AmericanShapeOf(core, key)
        select baseShape with { Id = ComponentId.Of(designation), Class = SteelClass.Composite, Composite = Some(detail) };

    static Fin<SteelShape> ColdFormedOf(string designation, American gauge, ColdFormedDetail detail, Op key) =>
        from baseShape in AmericanShapeOf(gauge, key)
        select baseShape with { Id = ComponentId.Of(designation), Class = SteelClass.ColdFormed, ColdFormed = Some(detail) };

    static Fin<Seq<SteelShape>> CompositeColdFormedRows(Op key) =>
        Seq(
            // W18x50 acting compositely with a 1200×100 mm normal-weight slab (f'c 28 MPa), 3/4in studs at 2/m — the
            // ΣQn cap reads the joint#JOINT_FAMILY StudClass.S19.SteelShearKn, never a re-derived stud shear.
            CompositeOf("steel.comp-w18x50-slab120", American.W18x50,
                new CompositeDetail(PositiveMagnitude.Create(1200.0), PositiveMagnitude.Create(100.0), 28.0, StudClass.S19, 2), key),
            // 600S162-54 AISI S100 stud reduced to a C-channel core with an effective-section modulus 0.78 of gross.
            ColdFormedOf("steel.cf-600s162-54", American.C15x33_9,
                new ColdFormedDetail(PositiveMagnitude.Create(152.0), PositiveMagnitude.Create(119.0), PositiveMagnitude.Create(4.76), PositiveMagnitude.Create(1.37), 0.78), key))
        .Sequence();

    // Every realized steel shape resolved to its (Id, SteelClass, ComputedSection, Dims, …) row ONCE, the seed the
    // component#COMPONENT_OWNER ComponentCatalogue.Build folds and the SteelSections map keys — the polygon integral + the
    // SteelStiffness closed forms run exactly once per section here, never per resolution call.
    static readonly Seq<SteelShape> Shapes =
        AmericanSeed.Choose(id => AmericanShapeOf(id, default).ToOption())
            .Concat(EuropeanSeed.Choose(id => EuropeanShapeOf(id, default).ToOption()))
            .Concat(CompositeColdFormedRows(default).IfFail(Seq<SteelShape>()));

    // The ComponentId → ComputedSection map computed ONCE from Shapes — the section the [03] component#COMPONENT_RESOLUTION
    // ComponentResolution.Build JOINS by ComponentId (the data captured at build, never a SteelSectionOf delegate re-invoked
    // per resolution call); component#COMPONENT_OWNER ComponentCatalogue.Sections folds this with the cmu/timber section maps.
    // SteelShape.Id's ComponentId generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so
    // NO explicit comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a
    // type mismatch on a ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Sections convention the master fold follows).
    public static readonly FrozenDictionary<ComponentId, ComputedSection> SteelSections =
        Shapes.ToFrozenDictionary(static s => s.Id, static s => s.Section);

    // The steel catalogue contribution: each realized SteelShape projects to a Component row whose cross-section FIELD is
    // ComponentSection.Steel(shape) — the SteelShape carries the canonical ComputedSection + dims + class, so the Component
    // holds no separate steel ComponentUnit (the cross-section IS the field). Steel is ComponentClass.Primary (the
    // IfcBuiltElement stratum ComponentFamily.Steel derives), Coring.None (no void class), the two INDEPENDENT MaterialId
    // slots the structural metal.steel Mechanical-property row (CapacityKey) and the stable metal.iron render row
    // (AppearanceId). The Id is the pre-validated ComponentId from Shapes, so the row takes the direct Component ctor (the
    // total form, no Fin) over the Context-uniform builder signature component#COMPONENT_OWNER ComponentCatalogue.Build folds
    // — the section integral ran once at Shapes build, never per row (the SAME shape timber#TIMBER_FAMILY BuildTimberRows uses).
    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO explicit
    // comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a type mismatch
    // on a ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildSteelRows(Context context) =>
        Shapes
            .Map(shape => (shape.Id, Component: new Component(
                ComponentFamily.Steel,
                shape.Id,
                ComponentSection.Steel(shape),
                Coring.None,
                shape.Standard,
                CapacityKey: MaterialId.Of("metal.steel"),
                AppearanceId: MaterialId.Of("metal.iron"))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Component);
}
```

## [03]-[RESEARCH]

- [CANONICAL_COMPUTEDSECTION]: REALIZED — the steel family produces the ONE canonical `component#COMPONENT_OWNER` `ComputedSection` (the twenty-field strong-AND-weak-axis receipt `Area`/`IxMm4`/`IyMm4`/`SxMm3`/`SyMm3`/`RxMm`/`RyMm`/`ZxMm3`/`ZyMm3`/`JMm4`/`IwMm6`/`AvyMm2`/`AvzMm2`/`DepthMm`/`WidthMm`/`HeatedPerimeterMm`/`AxisDistanceMm`/`ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor`) that `ParametricSection` produces for the rectangle families, NOT a parallel narrow `SteelSection` receipt (the deleted form). `SectionReader.Read` runs `new SectionProperties((IProfile)catalogue)` for the elastic columns and `SteelStiffness.Derive` for the plastic/torsion/warping/shear AND asymmetric-section LTB columns the elastic integral cannot yield, so a steel W-shape and a glulam rectangle resolve their section through the SAME `[03]` `component#COMPONENT_RESOLUTION` cache the structural-design seam reads via `graph.SectionOf(member)`. The steel family is the one that FILLS the `IwMm6` warping column `component#COMPONENT_OWNER` `ComputedSection` reserves for the OPEN thin-walled shape (the EN 1993-1-1 §6.3.2 / AISC 360 Ch F lateral-torsional-buckling input the rectangle families leave engineering-zero), the distinct `AvyMm2`/`AvzMm2` web-vs-flange shear areas, AND the `ShearCentreYMm`/`ShearCentreZMm` shear-centre offsets + `MonosymmetryFactor` β_y the EN 1993-1-1 §6.3.2 GENERAL LTB route needs for a singly-symmetric channel/tee/angle (engineering-zero for a doubly-symmetric I/HSS/solid, the seam `IsDoublySymmetric` reading them symmetric). Ripple counterpart: `Component/component` `[COMPONENT_OWNER]`/`[COMPONENT_RESOLUTION]` (the `ComputedSection` shape this family fills and the one-hop cache the `SteelSections` map feeds).
- [CROSS_SECTION_AS_COMPONENT_FIELD]: REALIZED — the steel cross-section is a `ComponentSection.Steel(SteelShape)` FIELD of the `Component`, never a peer object — the unified Material/Component/Element paradigm makes the cross-section a column on the standardized TYPE. `ComponentCatalogue.BuildSteelRows` projects each realized `SteelShape` to a steel `Component` through the direct `Component` ctor (the total form, the `Id` a pre-validated `ComponentId` from `Shapes`, the SAME shape `timber#TIMBER_FAMILY` `BuildTimberRows` uses): `ComponentFamily.Steel`, `ComponentSection.Steel(shape)` (the `SteelShape` carrying the canonical `ComputedSection` + dims + class), `Coring.None`, the AISC/EN `ComponentStandard`, and the two INDEPENDENT `MaterialId` slots — the structural `metal.steel` `Mechanical`-property `CapacityKey` and the stable `metal.iron` render `AppearanceId` (the FROZEN appearance column preserved). Steel carries no separate `ComponentUnit` because the catalogued geometry IS the field, and is a `ComponentClass.Primary` member (the space-bounding `IfcBuiltElement` stratum `ComponentFamily.Steel` derives). Ripple counterpart: `Component/component` `[COMPONENT_OWNER]` (the `Component` record + the `ComponentSection` `[Union]` whose `Steel` arm carries this page's `SteelShape`).
- [CATALOGUE_GROUNDED_SEED]: REALIZED — the hand-keyed `SteelRow` literal table is the deleted form; `BuildSteelRows` seeds from the `VividOrange.Profiles.Catalogue` registered database (`CatalogueFactory.CreateAmerican(American)` / `CreateEuropean(European)` minting the sealed-singleton `IProfile` carrying published `UnitsNet.Length` geometry), and every elastic section property is COMPUTED by the `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral over that `IProfile` rather than transcribed. The `AmericanSeed`/`EuropeanSeed` enum lists select the realized subset of the 2299 American + 558 European sections — spanning every published family (W/S/HP i-shape, C/MC channel, L + DoubleL angle, HSS-rect, the round HSS, Pipe, WT/ST/MT tee on the American side; IPE/HEA/HEB/HEM/IPN/UC i-shape + UPN channel on the European side) — a new section is one `American`/`European` enum value added to a seed, its full geometry already registered behind the `CatalogueFactory` singleton, never a `SteelRow` of literal doubles. The European heavy series names `HE<size>M` (`European.HE300M`), never `HEM300` (the catalogue identity is the section name; `HEM` is the `EuropeanShape` family value). The catalogue's `IIParallelFlange.FilletRadius` and the HSS corner radii integrate EXACTLY through the solver's typed quarter-ellipse decomposition, so the computed `Ix`/`Sx` carry the fillet without the polygonized loss a straight-shoelace integration incurs (`api-vividorange-sections-sectionproperties.md` `[POLYGON_INTEGRAL_CONTRACT]`).
- [TOTAL_FAMILY_FOLD]: REALIZED — `SteelClass.OfShape(AmericanShape, key)` and `OfShape(EuropeanShape, key)` are TOTAL `Fin`-returning folds over the published 13 American / 25 European families: the H/I families (`UB`/`UC`/`HEA`/`HEB`/`HEM`/`HEC`/`HE`/`HL`/`HLZ`/`HD`/`HP`/`UBP`/`IPN`/`IPEAA`/`IPEA`/`IPE`/`IPEO`/`IPEV`/`J`) → `IShape`, the channel families (`UPE`/`PFC`/`UPN`/`U`/`CH`) → `UShape`, with the American `L`/`DoubleL`/`WT`/`MT`/`ST` mapping to `LShape`/`DoubleAngle`/`Tee`; an unrecognized family rails `ComponentFault.Family`. The EN families are exclusively i-shape and channel — EN 10365 publishes NO European angle/hollow/tee family — so the two `OfShape(EuropeanShape)` arms exhaust all 25 members (the `_` arm is the defensive unrecognized-family rail). The prior `_ => IShape` silent default — which mis-classified a family as an I-shape and seeded a wrong slenderness — is the deleted runtime-silent default arm. The `SteelTopology` (open/closed/solid) carried on each `SteelClass` is the ONE discriminant the `SteelStiffness` closed forms and the `CompactnessClass` slenderness model branch on, never a per-class duplicate.
- [HOLLOW_TOPOLOGY_BY_GEOMETRY]: REALIZED — AISC assigns BOTH a round HSS and a rectangular HSS the SAME `AmericanShape.HSS` family value (verified: `HSS13_375x_625` is `ICircularHollow`, `HSS8x8x_500` is `IRoundedRectangularHollow`, both `Shape => AmericanShape.HSS`), so the family enum CANNOT distinguish round from rectangular — the round/rect split is the `ComponentCatalogue.ClassOf` `ICircularHollow`/`IRectangularHollow` GEOMETRY discriminant tested BEFORE the `OfShape` family fold. A round HSS therefore resolves `SteelClass.HssRound` (the round-tube polar `J`, the `D/t` Table B4.1 classification, `IfcCircleHollowProfileDef`) rather than the rectangular closed-form a family-only dispatch (`OfShape(HSS) => HssRect`) mis-applies; a rectangular HSS resolves `HssRect`, a `Pipe` resolves `HssRound`. `OfShape(AmericanShape.HSS)` stays the rectangular family default for completeness, but `ClassOf`'s geometry pre-empt is authoritative for the catalogue path. Ripple counterpart: `Component/component` `[COMPONENT_OWNER]` (the `ComputedSection` the round-tube closed form fills correctly under the geometry split).
- [STIFFNESS_CLOSED_FORMS]: REALIZED — `SteelStiffness.Derive` computes the eight columns the elastic VividOrange integral cannot yield (`ZxMm3`/`ZyMm3` plastic moduli, `JMm4` St-Venant torsion, `IwMm6` warping constant, `AvyMm2`/`AvzMm2` shear areas, AND the asymmetric-section LTB columns `ShearCentreYMm`/`ShearCentreZMm` shear-centre offsets + `MonosymmetryFactor` β_y) from the admitted dimensional columns keyed by `SteelClass`/`SteelTopology`: the OPEN topology runs the I/channel/tee/angle web-flange decomposition (the rectangular-component plastic moduli, the open thin-walled `J = Σbt³/3`, the doubly-symmetric warping `Iw ≈ tf·bf³·hₒ²/24`, the AISC §G web `d·tw` and flange `⁵⁄₃·bf·tf` shear areas, the channel minor-axis shear-centre offset / the tee-angle offset + SN030 β_y), the CLOSED topology the HSS-rect/round closed form (the Bredt thin-walled-tube torsion `J = 4A_m²·t/perimeter`, engineering-zero warping, the perimeter shear area, zero shear-centre offset/β_y), the SOLID topology the bar closed form. The warping `IwMm6` is the OPEN-topology column the §F2 LTB reads (the rectangle families leave it engineering-zero); the three asymmetric-section LTB columns are non-zero ONLY for the singly-symmetric channel/tee/angle (a doubly-symmetric I/HSS/solid places its shear centre AT the centroid, all three zero, the seam `IsDoublySymmetric` reading them symmetric — the EN 1993-1-1 §6.3.2 general route's inputs a PFC/tee needs and a W-shape leaves zero). The channel half-warping, the angle/double-angle torsion-only (no warping couple), and the round-tube polar `J` are the topology-correct refinements, each one fold arm not a parallel owner.
- [BOUNDARY_ADMISSION_UNITS]: REALIZED — `SectionReader.Read` is the single point raw `VividOrange` `UnitsNet` is admitted: the catalogue dimensions (AISC native `LengthUnit.Inch`, EN native `LengthUnit.Millimeter`, the unit travelling WITH the `Length`) and the solver's `Area`/`AreaMomentOfInertia`/`Volume`/`Length` outputs coerce to SI-millimetre scalars (`.Millimeters`/`.SquareMillimeters`/`.MillimetersToTheFourth`/`.CubicMillimeters`, `UnitsNet` owning the Inch→mm conversion in the accessor) and admit once into the kernel `PositiveMagnitude` columns; the interior carries raw SI doubles and no `UnitsNet` quantity crosses an interior signature, per the BOUNDARY_ADMISSION law the in-folder `MaterialUnits` boundary (`Appearance/photometric#PHOTOMETRIC`) already enforces. A non-positive or non-finite computed column rails the value-object's own `Fin`, so a malformed section drops from `Shapes` through `Choose` rather than seeding a degenerate row. The solver consumes the polymorphic `IProfile` for the area/inertia/modulus/radius outputs (no family cast); the family cast (`ReadDims`) is needed ONLY for the dimensional columns the AISC Table B4.1 `Classify` slenderness AND the `SteelStiffness` closed forms read, dispatched over the `IIParallelFlange`(W/HEA/HP) → `II`(S taper) → `IChannel`/`ITee`/`IAngle`/`IDoubleAngle`/`ICircularHollow`/`IRectangularHollow` interface order the geometry selects, an unmatched interface railing `ComponentFault.Family` rather than a fabricated sentinel.
- [WEAK_AXIS_AND_PLASTIC]: REALIZED — the solver supplies BOTH axes (`MomentOfInertiaYy`/`Zz`, `ElasticSectionModulusYy`/`Zz`, `RadiusOfGyrationYy`/`Zz`), so the `ComputedSection` carries the weak-axis `IyMm4`/`SyMm3`/`RyMm` columns and the `Capacity` flexural-buckling check governs about the WEAK axis (`min(RxMm, RyMm)` over the `ComputedSection`'s both-axis radius columns) the way real column design does, replacing the strong-axis-only `Math.Sqrt(Ix/A)` approximation. The plastic moduli `ZxMm3`/`ZyMm3` the elastic polygon integral cannot yield are DERIVED columns — `SteelStiffness` computes them from the admitted dimensional columns per `SteelClass` (the I/channel rectangular-component sum `Bf·Tf·(D−Tf) + ¼·Tw·(D−2Tf)²` strong-axis and the two-flange-plus-web weak-axis sum, the HSS-rect/round closed forms, the angle/tee shape-factor over the solver's elastic `Sx`/`Sy`), grounding the plastic moment `Mp = Fy·Zx` and the weak-axis `Mpy = Fy·Zy` in geometry not a literal.
- [LTB_WITH_WARPING]: REALIZED — `SteelDesign.LateralTorsionalMn` reads the REAL `ComputedSection.IwMm6` warping constant for the AISC 360 §F2 lateral-torsional-buckling check: `Lp = 1.76·ry·√(E/Fy)`, the effective radius `rts ≈ √(√(Iy·Iw)/Sx)`, the inelastic-LTB limit `Lr` derived from `J`/`Iw`, the linear `Mp → 0.7·Fy·Sx` interpolation between `Lp` and `Lr`, and the elastic-LTB `Fcr·Sx` beyond `Lr` (the §F2 `Fcr` with the `1 + 0.078·(J·c)/(Sx·ho)·(Lb/rts)²` warping term). The prior bare-`J` form (which omitted `Iw`) is the deleted approximation. The shear capacity reads the MAJOR-axis web shear area `AvyMm2` (`φv·0.6·Fy·Aw`, the seam `AvY`(major) — the `SteelStiffness` tuple's 5th slot, NOT the minor-axis flange `AvzMm2`), so a HSS-rect's two-web `2·h·t` major shear and a round-pipe's `A/2` shear are the section's true mechanics, the convention matched to the seam `SectionProperties.AvY`(major)/`AvZ`(minor) the `Projection/component#COMPONENT_PROJECTOR` `SeamSection` lift pins.
- [COMPOSITE_AND_COLDFORMED]: REALIZED — the `SteelClass.Composite` (AISC 360 Chapter I steel-concrete) and `SteelClass.ColdFormed` (AISI S100 light-gauge) cases ride the SAME `ComputedSection` with an `Option<CompositeDetail>`/`Option<ColdFormedDetail>` augmentation on the `SteelShape` row (defaulted `None` for rolled shapes) rather than a parallel section owner; both seed through `CompositeColdFormedRows` over a catalogued `American` core (the composite W18x50, the cold-formed C-channel core) with the detail applied through `with`. The `CompositeDetail` carries the slab effective-width/depth, the concrete `f'c`, and the `Component/joint#JOINT_FAMILY` `StudClass` shear-stud reference + studs-per-metre; the `CompositeMn` projection runs the AISC 360 Ch I plastic composite moment capped at the stud horizontal shear `ΣQn = StudClass.SteelShearKn × StudsPerMetre` (the one stud vocabulary's AISC 360-22 Eq I8-1 per-stud steel-side cap, NEVER a re-derived stud shear) for partial composite action. The `ColdFormedDetail` carries the gross/effective width, corner radius, design thickness, and the `EffectiveSectionModulusRatio` the AISI S100 effective-width method yields, the cold-formed flexural `Mn = Seff·Fy` over the solver `Sx` scaled by the ratio. Both map to their `IfcProfileDef` subtype (`Composite`→`IfcArbitraryClosedProfileDef`, `ColdFormed`→`IfcUShapeProfileDef`). Ripple counterpart: `Component/joint` `[JOINT_FAMILY]` (the shared `StudClass.SteelShearKn` the composite shear interface reads).
- [IFCPROFILEDEF_SUBTYPE_AXIS]: `SteelClass` carries the full `IfcProfileDef` parameterized-profile subtype axis — `IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef` for the rolled families, `IfcArbitraryClosedProfileDef` for the `DoubleL`/`Composite` sections that have no single parametric form, `IfcUShapeProfileDef` for the cold-formed channel-stud — so a steel member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the `component#COMPONENT_OWNER` `MaterialComposition.ProfileSet(ProfileRef)` shape). The reciprocal ingress is realized at the `Rasm.Bim` `Semantics/composition#MATERIAL_COMPOSITION` owner, so the round-trip is symmetric: its `ProfileDefKind` discriminates the `IfcMaterialProfile.Profile` runtime type onto this same subtype axis, its `ProfileDims` folds the `IfcParameterizedProfileDef` members back onto the section `DepthMm`/`FlangeWidthMm`/`WebThicknessMm`/`FlangeThicknessMm`/`FilletMm` columns, and the `DoubleL` back-to-back spacing crosses on the `SectionDims.BackToBackMm` (the `IDoubleAngle.BackToBackDistance` the catalogue carries) onto the Bim `ProfileDims.BackToBackMm`. Ripple counterpart: `Rasm.Bim` `Semantics/composition` `[MATERIAL_COMPOSITION]`.
- [EN_STEEL_GRADE_YIELD]: REALIZED — the `SteelGrade` `[SmartEnum]` binds the structural-steel grade band to its `VividOrange.Materials` `EnSteelGrade` + the `EnSteelDeliveryCondition` whose Table 3.1 sub-table HOLDS the grade, so `SteelGrade.YieldMpa(thicknessMm, annex, key)` builds the `EnSteelMaterial`, sets `Specification.DeliveryCondition` to the band's `Delivery`, and reads the EN 1993-1-1 Table 3.1 `f_y` from `EnSteelFactory.CreateLinearElastic` — the thickness-banded design yield (≤40 mm vs 40-80 mm) the `.api/api-vividorange-materials.md` `[EN_TABLE_CONTRACT]` tabulates — rather than a caller-supplied raw double. The AR (as-rolled, EN 10025-2) Table 3.1 sub-table holds S235/S275/S355/S450; the N (normalized, EN 10025-3) and M (thermomechanical, EN 10025-4) sub-tables hold S420/S460, so S420/S460 carry `Delivery.N` — the default AR specification rails them (the factory throws `ArgumentException` on a grade the AR table omits, trapped onto `ComponentFault.Family`). The AISC grades stay hand-keyed spec-nominal because no .NET package owns the AISC grade table; the EN grades cite `En1993`. The `DesignCapacity.TorsionalNmm` AISC 360 §H3.1 design torsional resistance `φT·Fcr·C` (positive for the CLOSED HSS/pipe topology, engineering-zero for an OPEN warping-torsion shape) is REALIZED on the receipt — the `Component/capacity#SECTION_CAPACITY` `SectionCapacityResolver.SteelLrfd` lift reads it directly onto `SectionCapacity.SteelLrfd.TorsionalKnm`. Ripple counterpart: `Component/capacity` `[SECTION_CAPACITY]` (the unified capacity rail the steel `DesignCapacity` lifts into via `SectionCapacityResolver.SteelLrfd`, the `TorsionalNmm` column the `SteelLrfd` arm folds against `demand.TorsionKnm`).
