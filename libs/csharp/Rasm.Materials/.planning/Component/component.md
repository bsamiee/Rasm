# [MATERIALS_COMPONENT]

THE POLYMORPHIC COMPONENT OWNER. One `Component` record is the canonical standardized-TYPE concept every steel/timber member, masonry/CMU/IGU unit, sheet-goods board, and rebar/fastener/connector/joint part parameterizes: a closed `SectionProfile` cross-section algebra (a FIELD, never a peer union of per-family payloads), an `IfcBinding` row stamp, a `Coring` void class, a `ComponentStandard` receipt, two independent `MaterialId` slots, and an `Option<PropertyBag>` detail whose presence is a TYPE LAW of the family's `DetailLane`. One `ComponentFamily` `[SmartEnum<string>]` is THE policy row: each of the ten rows {masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint · panel} carries its `ComponentClass`, its `DetailLane`, its profile-admission predicate, its cross-nominal selector, and its `<Family>Seed.Rows` fold — so a NEW FAMILY is one row plus one seed page with zero central edits, and a NEW PART is one seed row with zero type edits. The prior `ComponentSection` ten-arm payload union, its five lockstep switches (`Family`/`CrossNominalMm`/`GrossRectangleMm`/`IfcEntity`/`PredefinedToken`), the cross-file `Detail` switch, and `ParametricSection` are the DELETED forms: every former switch is now a stored column, a base-constructor fact, a family delegate, or a field read.

This page also owns the ONE section-computation spine: `SectionSolver.Solve` is the single exhaustive generated `Switch` over the closed `SectionProfile` axis — a new arm is compiler-forced HERE and nowhere else. Every solvable arm routes the ONE `VividOrange.Sections.SectionProperties` Green's-theorem integral through the ONE twenty-column `Admit` lift, then a topology supplement: `Curves` lowers each arm onto the TYPED `VividOrange.IProfiles` floor contract (`IRectangle`/`IRoundedRectangle`/`IRectangularHollow`/`IRoundedRectangularHollow`/`ICircle`/`ICircularHollow`/`IEllipse`/`IIParallelFlange`/`ICustomI`/`IChannel`/`IC`/`ITee`/`IAngle`/`IZ`/`ITrapezoid`) so the solver's internal part decomposition (`TrapezoidalPart` + `EllipseQuarterPart`) integrates fillets and curved edges EXACTLY — a faceted `LocalPolyline2d` loop survives only where no typed contract exists (`CellularRectangle` multi-void, `Corrugated` fold, `FilletTriangle`, `Outline`); `Forms` supplies the plastic/torsion/warping/shear/asymmetry closed forms, including the relocated `steel#STEEL_FAMILY` open-thin-walled algebra (`OpenI`/`MonoI`/`OpenChannel`/`OpenCee`/`MonoTee`/`OpenAngle`/`PointSymmetricZ`) and the carrier-free `.Utility` kernels (`SectionModuli`/`Inertiae`/`Areas`) where a singly-symmetric arm needs elastic moduli. `ComputedSection` stays the FROZEN twenty-column receipt (`[FROZEN_INVARIANTS]`), `ComponentCatalogue.Of` the ONE fail-loud `Traverse` fold computing BOTH frozen maps in one pass, `ComponentResolution` the byte-identical M7 one-hop, `QuantityRow` the one typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose, and `ComponentDetail` the relocated seed-time bag constructors every `Realization`/`Product`-lane seed composes. The page composes the `Rasm.Vectors` kernel `PositiveMagnitude`, the `Rasm.Domain` `Op`/`Context`/`Expected`/`AcceptValidated` rail, the `Rasm.Element` seam (`MaterialId`, `ProfileRef`, `FaultBand`, the `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`DetailSchema` property vocabulary), and the VividOrange profile/section floor. `ProfileRef`/`ProfileSet`/`SectionProperties`/`ComputedSection` stay seam-canonical — the rename STOPS at the Materials boundary.

## [01]-[INDEX]

- [02]-[COMPONENT_OWNER]: `ComponentId`, `ComponentClass`, `DetailLane`, `CoringClass`/`Coring`, `ComponentAuthority`, `IfcBinding`, the `ComponentFamily` policy row, the band-2300 `ComponentFault` (registry read), `ComponentUnit`, `ComponentStandard`, and the `Component` record with its ONE polymorphic `Of` admission.
- [03]-[SECTION_PROFILE]: `VoidCell`, `Ply`, and the closed `SectionProfile` `[Union]` — named `PositiveMagnitude` dimensions, gross bounding facts as base-constructor state, and one railed `Of` factory per arm.
- [04]-[SECTION_SOLVER]: `SectionSupplement`, `SectionSolver.Solve` (the ONE exhaustive dispatch), the twenty-column `Admit` lift, the RC gross-outline `ProfileOf`, the `Curves` typed-contract perimeter table, and the `Forms` closed-form supplement table carrying the relocated steel open-thin-walled algebra.
- [05]-[COMPUTED_SECTION]: the FROZEN twenty-field receipt + `GoverningRadiusMm`.
- [06]-[CATALOGUE]: `ComponentRow`, `ComponentCatalogue.Of`/`Lookup` — the fail-loud one-pass fold over `ComponentFamily.Items`.
- [07]-[QUANTITY_ROW]: the one typed-mint `[SmartEnum]` the seam lift and the property ingress compose.
- [08]-[COMPONENT_DETAIL]: the relocated bag constructors (`Joint`/`Token`/`Measured`/`RealizationRows`/`ProductRows`).
- [09]-[COMPONENT_RESOLUTION]: `ResolvedComponent` + the frozen `Build`/`Resolve` M7 cache, byte-identical.
- [10]-[RESEARCH]: realized decisions and register.

## [02]-[COMPONENT_OWNER]

- Owner: `Component` over the ten-row `ComponentFamily` policy axis; `ComponentId` the `family.designation` key; `ComponentFault` band 2300 read from the `FaultBand` registry; `IfcBinding` the Materials-side IFC pair as ROW DATA (was two 10-arm switches); `DetailLane` the bag-presence law; `Coring`/`CoringClass` the cross-family void class; `ComponentUnit`/`ComponentStandard` the shared dimensional and regional receipts.
- Cases: one `Component` shape across all ten families — `Family` (the policy row), `Designation`, `Profile` (the `SectionProfile` arm), `Ifc` (the stored `IfcBinding` row), `Coring`, `Standard`, `SubstanceId`/`AppearanceId` (independent `MaterialId` slots — a coated rebar keeps capacity steel and epoxy appearance distinct), `Detail` (`Option<PropertyBag>`, `Some` iff the family lane is not `None`). Every former `ComponentSection` switch is a field read (`IfcEntity`/`PredefinedToken`), a family delegate (`CrossNominalMm`), or base-constructor state (`GrossRectangleMm`).
- Entry: `Component.Of(family, designation, profile, ifc, coring, standard, substanceId, appearanceId, detail, key)` — ONE polymorphic admission guarding ONLY Component-owned invariants: the `ComponentId` format, the family/profile admission predicate, the coring range, the laminate build, and the lane/detail consistency (the deleted `Detail` switch's totality, now a type law). The `CrossNominalMm > 0` guard is DELETED as unrepresentable — the value is a `PositiveMagnitude` by construction.
- Packages: Rasm.Vectors (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`Expected`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `ProfileRef`, `FaultBand`, `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`PropertyName`/`PropertyValue`/`DetailSchema`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`[UseDelegateFromConstructor]`, generated exhaustive `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`guard` — rails-and-effects doctrine substrate), VividOrange.IProfiles + VividOrange.Profiles.Perimeter + VividOrange.Geometry + VividOrange.Sections.SectionProperties + UnitsNet (`Rasm.Materials/.api` catalogues), BCL inbox (`FrozenDictionary`).
- Growth: a new part is one seed row; a new family is one `ComponentFamily` row plus one seed page (no union edit, no switch edit, no `.Concat` edit); a new section shape is one `SectionProfile` arm plus one compiler-forced `Solve` arm (buildingSMART profile-schema cadence, never thing cadence); a new fault is one `ComponentFault` case; a new structural column is one `ComputedSection` field every solvable arm fills; a new detail row is one `DetailSchema`-named tuple in a seed bag; a new typed mint is one `QuantityRow` row.
- Boundary: `ComponentFault` derives `Expected` (`IValidationError<ComponentFault>`), its `Code` the `FaultBand.Component` registry read — disjointness is type-enforced, never prose. `IfcBinding` strings stay NEUTRAL here; the generated `Rasm.Bim` roster is the validation authority (composition-time `IfcLegality`, egress-time `AdmitPredefined`, design-time emitter stamp audit) — Materials never references `Rasm.Bim`. Seed pages in child sub-namespaces (`Masonry`/`Cmu`/`Steel`/`Panel`/`Reinforcement`) resolve through file-local aliases so the policy rows read `<Family>Seed.Rows` uniformly.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;                          // CultureInfo.InvariantCulture (the ComponentId.Validate format provider)
using LanguageExt;
using Rasm.Vectors;                                  // PositiveMagnitude — the kernel >0-finite magnitude (Atoms.cs)
using Rasm.Domain;                                   // Op, Context, Expected, AcceptValidated
using Rasm.Element;                                  // MaterialId, ProfileRef, FaultBand, MeasureValue/QuantityType/PropertyBag/PropertyName/PropertyValue/DetailSchema
using Thinktecture;
using UnitsNet;                                      // Length — the typed profile-contract dimension currency
using VividOrange.Geometry;                          // LocalPoint2d, LocalPolyline2d, ILocalPoint2d, ILocalPolyline2d
using VividOrange.Profiles;                          // IProfile + the typed parametric contracts (IRectangle/ICircle/IEllipse/II/IC/IZ/ITee/IAngle/ITrapezoid/…), Perimeter
using VividOrange.Sections.SectionProperties.Utility; // Areas/Inertiae/SectionModuli — the carrier-free Green's-theorem kernels
using Expected = Rasm.Domain.Expected;               // the kernel Expected (parameterless ctor + virtual Code/Category), NOT LanguageExt.Common.Expected
using Dimension = Rasm.Element.Dimension;            // the seam 7-exponent measure Dimension (Rasm.Vectors.Dimension, the count atom, is unused here)
using SectionProperties = VividOrange.Sections.SectionProperties.SectionProperties;   // the solver carrier; the seam Rasm.Element.SectionProperties is prose-only on this page
using static LanguageExt.Prelude;

// component#COMPONENT_OWNER owns Component/ComponentClass/ComponentFamily/ComponentId/ComponentFault/DetailLane/
// IfcBinding/Coring/CoringClass/ComponentUnit/ComponentStandard/ComponentAuthority/SectionProfile/VoidCell/Ply/
// SectionSolver/ComputedSection/ComponentRow/ComponentCatalogue/QuantityRow/ComponentDetail/ComponentResolution.
// Every <Family>Seed declares in this ONE Rasm.Materials.Component namespace (the <Family>Seed naming keeps rows
// collision-free), so the ComponentFamily policy rows bind SteelSeed.Rows/MasonrySeed.Rows/… by bare name — no alias.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The component key: 'family.designation' (steel.w14x90, masonry.us-modular, cmu.8in-hollow) — a '.'-separated non-empty
// token; the family discriminant rides the prefix so every family keys uniformly.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ComponentId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<component-id requires 'family.designation'>") : null;
}

// The structural-class discriminant, a THREE-row axis: Primary a space-bounding one-piece member (geometry-on-type,
// one occurrence one piece), Panel a standardized sheet-goods board (one board type, MANY laid pieces — assay-verified
// IfcCovering/IfcPlate/IfcSlab : IfcBuiltElement), Minor a standardized part (one type, many discrete pieces). The
// concrete IFC leaf is the family's IfcBinding row (discrete parts, panel kind-determined leaves) or the occurrence
// refinement the Bim egress resolves (the supertype-projecting profiled families).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentClass {
    public static readonly ComponentClass Primary = new("primary", ifcSupertype: "IfcBuiltElement");
    public static readonly ComponentClass Panel   = new("panel",   ifcSupertype: "IfcBuiltElement");
    public static readonly ComponentClass Minor   = new("minor",   ifcSupertype: "IfcElementComponent");
    public string IfcSupertype { get; }
}

// The bag-presence law: None carries no PropertyBag; Realization carries the DetailSchema.Realization bag (discrete
// realizing parts + masonry's EN 771 envelope); Product carries the DetailSchema.Product bag (panel boards, glazing
// IGU builds). Component.Of enforces lane/detail consistency as a type law — the deleted Detail switch's totality.
public enum DetailLane : byte { None = 0, Realization = 1, Product = 2 }

// The ASTM C652 / C90 net-area threshold a SolidFraction falls into: C652 fixes the hollow classes (H40V <=25% void at
// <=40% net, H60V) and C62/C216 the solid (>=75% net); NetAreaFloor is the minimum net-area ratio the class admits.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid      = new("solid",      netAreaFloor: 0.75);   // ASTM C62/C216 solid clay
    public static readonly CoringClass Frogged    = new("frogged",    netAreaFloor: 0.80);   // single-face indentation, solid for net-area
    public static readonly CoringClass Cored      = new("cored",      netAreaFloor: 0.75);   // <=25% coring, structurally solid
    public static readonly CoringClass Cellular   = new("cellular",   netAreaFloor: 0.60);   // closed cavity one bed face
    public static readonly CoringClass Perforated = new("perforated", netAreaFloor: 0.50);   // through-perforations, net 50-75%
    public static readonly CoringClass Hollow     = new("hollow",     netAreaFloor: 0.00);   // ASTM C652 H40V/H60V hollow brick / C90 CMU
    public double NetAreaFloor { get; }
}

// The void-class row a Component carries: the shared vocabulary spanning clay-brick (frog/cored/perforated/cellular)
// AND concrete-masonry (hollow 2-cell / 3-cell) geometry. VoidFraction stays a double in [0,1): Component.Of guards it
// with a relational pattern over the raw key. A non-masonry/non-cmu Component carries Coring.None, total across the axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coring {
    public static readonly Coring None             = new("none",                voidFraction: 0.00, classification: CoringClass.Solid);
    public static readonly Coring Frog             = new("frog",                voidFraction: 0.10, classification: CoringClass.Frogged);
    public static readonly Coring Cored3Hole       = new("cored-3-hole",        voidFraction: 0.20, classification: CoringClass.Cored);
    public static readonly Coring Cellular         = new("cellular",            voidFraction: 0.35, classification: CoringClass.Cellular);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell",  voidFraction: 0.42, classification: CoringClass.Perforated);
    public static readonly Coring Hollow3Cell      = new("hollow-3-cell",       voidFraction: 0.47, classification: CoringClass.Hollow);   // 12-in 3-cell CMU — more web material than the 2-cell unit
    public static readonly Coring Hollow2Cell      = new("hollow-2-cell",       voidFraction: 0.50, classification: CoringClass.Hollow);
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
    public double NetAreaFraction => 1.0 - VoidFraction;   // the C90 net-area ratio the structural capacity scales by
}

// The published standards body a regional ComponentStandard cites — a bounded vocabulary, never a free Authority string.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentAuthority {
    public static readonly ComponentAuthority Astm = new("ASTM", region: "us");          // ASTM C90/C216/C652 masonry & CMU, A615/A706 rebar, F3125 bolts
    public static readonly ComponentAuthority Aisc = new("AISC", region: "us");          // AISC Shapes Database
    public static readonly ComponentAuthority Aisi = new("AISI", region: "us");          // AISI S100 cold-formed
    public static readonly ComponentAuthority Aws  = new("AWS",  region: "us");          // AWS A5.1/D1.1 weld consumables
    public static readonly ComponentAuthority En   = new("EN",   region: "eu");          // EN 10365/338/14080/1279/10080/898-1/ISO 261
    public static readonly ComponentAuthority Bs   = new("BS",   region: "uk");          // BS EN 771 masonry, BS 8666 rebar schedule
    public static readonly ComponentAuthority Din  = new("DIN",  region: "din");         // DIN 105 masonry
    public static readonly ComponentAuthority As   = new("AS",   region: "au");          // AS 4773 masonry
    public static readonly ComponentAuthority Is   = new("IS",   region: "is");          // IS 1077 masonry
    public static readonly ComponentAuthority Apa  = new("APA",  region: "us");          // APA PRG 320 CLT
    public static readonly ComponentAuthority Csa  = new("CSA",  region: "ca");          // CSA G30.18 rebar
    public static readonly ComponentAuthority Sdi  = new("SDI",  region: "us");          // ANSI/SDI steel deck
    public string Region { get; }
}

// The Materials-side IFC pair as ROW DATA (was two 10-arm switches). Strings stay neutral here; the Bim generated
// roster is the validation authority — composition-time via IfcLegality, egress-time via AdmitPredefined, design-time
// via the emitter stamp audit. Materials never references Rasm.Bim.
public readonly record struct IfcBinding(string Entity, string Predefined) {
    public static IfcBinding Of(string entity, string predefined) => new(entity, predefined);
    public static IfcBinding Supertype(ComponentClass @class) => new(@class.IfcSupertype, "NOTDEFINED");
}

// The ONE family axis as a POLICY ROW: class, detail lane, the admission predicate (was the family/section mismatch
// guard), the cross-nominal selector (was the 10-arm switch), and the seed-row fold. The catalogue folds Items, so a
// NEW FAMILY is this row plus one seed page — no central edit anywhere. Seed owners are named <Family>Seed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentFamily {
    public static readonly ComponentFamily Masonry       = new("masonry",       ComponentClass.Minor,   DetailLane.Realization, admits: static p => p is SectionProfile.Rectangle or SectionProfile.CellularRectangle,                                    crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: MasonrySeed.Rows);
    public static readonly ComponentFamily Cmu           = new("cmu",           ComponentClass.Minor,   DetailLane.None,        admits: static p => p is SectionProfile.CellularRectangle,                                                                crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: CmuSeed.Rows);
    public static readonly ComponentFamily Steel         = new("steel",         ComponentClass.Primary, DetailLane.None,        admits: static p => p is SectionProfile.Catalogued or SectionProfile.RectangleHollow or SectionProfile.CircleHollow or SectionProfile.ColdFormedC, crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: SteelSeed.Rows);
    public static readonly ComponentFamily Timber        = new("timber",        ComponentClass.Primary, DetailLane.None,        admits: static p => p is SectionProfile.Rectangle or SectionProfile.Layered,                                              crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: TimberSeed.Rows);
    public static readonly ComponentFamily Glazing       = new("glazing",       ComponentClass.Minor,   DetailLane.Product,     admits: static p => p is SectionProfile.Layered,                                                                           crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: GlazingSeed.Rows);
    public static readonly ComponentFamily Reinforcement = new("reinforcement", ComponentClass.Minor,   DetailLane.Realization, admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: ReinforcementSeed.Rows);
    public static readonly ComponentFamily Fastener      = new("fastener",      ComponentClass.Minor,   DetailLane.Realization, admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: FastenerSeed.Rows);
    public static readonly ComponentFamily Connector     = new("connector",     ComponentClass.Minor,   DetailLane.Realization, admits: static p => p is SectionProfile.Rectangle or SectionProfile.Outline,                                              crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: ConnectorSeed.Rows);
    public static readonly ComponentFamily Joint         = new("joint",         ComponentClass.Minor,   DetailLane.Realization, admits: static p => p is SectionProfile.FilletTriangle or SectionProfile.Trapezium or SectionProfile.Circle or SectionProfile.Nominal, crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: JointSeed.Rows);
    public static readonly ComponentFamily Panel         = new("panel",         ComponentClass.Panel,   DetailLane.Product,     admits: static p => p is SectionProfile.Layered or SectionProfile.Corrugated,                                             crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: PanelSeed.Rows);

    [UseDelegateFromConstructor] public partial bool Admits(SectionProfile profile);
    [UseDelegateFromConstructor] public partial PositiveMagnitude CrossNominal(SectionProfile profile);
    [UseDelegateFromConstructor] public partial Fin<Seq<ComponentRow>> Rows(Context context);
    public ComponentClass Class { get; }
    public DetailLane Lane { get; }
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The component fault band: Expected-derived so a typed case lifts BARE onto Fin<T>/Validation<Error,T>; Code is the
// FaultBand.Component registry read (band disjointness type-enforced at FaultBand initialization, never prose). Nine
// disjoint slots by Category: Dimension a non-positive/non-finite length or degenerate geometry, Coring a void fraction
// outside [0,1), Family a family/profile/registration mismatch, Bond a masonry COURSE-PATTERN fault, Mortar a masonry
// MORTAR-SPEC fault, Section a section-INTEGRAL failure, Capacity a capacity-SOLVE failure, Designation a malformed
// ComponentId, Grade a registered-grade-band miss. No [GenerateUnionOps] (the kernel union-ops generator is strictly
// opt-in; every case carries an explicit Op, wanting no generated SelfOp); [Union] generates Switch/Map, never
// factories — the nested …Case records + same-name-less static factories return the Expected-derived base so a case
// lifts bare with no .ToError() hop.
[Union]
public abstract partial record ComponentFault : Expected, IValidationError<ComponentFault> {
    private ComponentFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Component;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record DimensionCase(Op Key, string Detail)   : ComponentFault(Key, Detail) { public override string Category => "Dimension"; }
    public sealed record CoringCase(Op Key, string Detail)      : ComponentFault(Key, Detail) { public override string Category => "Coring"; }
    public sealed record FamilyCase(Op Key, string Detail)      : ComponentFault(Key, Detail) { public override string Category => "Family"; }
    public sealed record BondCase(Op Key, string Detail)        : ComponentFault(Key, Detail) { public override string Category => "Bond"; }
    public sealed record MortarCase(Op Key, string Detail)      : ComponentFault(Key, Detail) { public override string Category => "Mortar"; }
    public sealed record SectionCase(Op Key, string Detail)     : ComponentFault(Key, Detail) { public override string Category => "Section"; }
    public sealed record CapacityCase(Op Key, string Detail)    : ComponentFault(Key, Detail) { public override string Category => "Capacity"; }
    public sealed record DesignationCase(Op Key, string Detail) : ComponentFault(Key, Detail) { public override string Category => "Designation"; }
    public sealed record GradeCase(Op Key, string Detail)       : ComponentFault(Key, Detail) { public override string Category => "Grade"; }

    public static ComponentFault Dimension(Op key, string detail)   => new DimensionCase(key, detail);
    public static ComponentFault Coring(Op key, string detail)      => new CoringCase(key, detail);
    public static ComponentFault Family(Op key, string detail)      => new FamilyCase(key, detail);
    public static ComponentFault Bond(Op key, string detail)        => new BondCase(key, detail);
    public static ComponentFault Mortar(Op key, string detail)      => new MortarCase(key, detail);
    public static ComponentFault Section(Op key, string detail)     => new SectionCase(key, detail);
    public static ComponentFault Capacity(Op key, string detail)    => new CapacityCase(key, detail);
    public static ComponentFault Designation(Op key, string detail) => new DesignationCase(key, detail);
    public static ComponentFault Grade(Op key, string detail)       => new GradeCase(key, detail);
    public static ComponentFault Create(string message) => Family(Admission, message);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The shared dimensional value-object the coursing families compose: width/height/length plus the coursing module, each
// column admitted ONCE through Of so no length re-validates downstream. LengthOverHeight is the course-laying aspect the
// masonry BondGeometry.Admits tiling gate reads (the course tiles along the bed — height, not width, is the denominator).
public readonly record struct ComponentUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    public double LengthOverHeight => LengthMm.Value / HeightMm.Value;
    public static Fin<ComponentUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: heightMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: lengthMm)
        from c in key.AcceptValidated<PositiveMagnitude>(candidate: courseHeightMm)
        select new ComponentUnit(w, h, l, c);
}

// The regional source receipt: region token + bounded Authority + the as-published coordinating joint thickness a
// masonry/cmu coursing reads (0 for a rolled/sawn/IGU/discrete component — a coursing input, never a section input).
public readonly record struct ComponentStandard(string Region, double StandardJointThicknessMm, ComponentAuthority Authority);

// The ONE standardized-type owner — profile + IFC stamp + seed-built detail replace the section union; every former
// switch is a field read or a family-delegate read. Of guards Component-owned invariants only: the ComponentId format,
// the family/profile admission, the coring range, the laminate build, and the lane/detail consistency (a None-lane
// family carries no bag; a Realization/Product-lane family always carries one). Dimensional admission already happened
// in the SectionProfile Of factories — a Component never re-proves a PositiveMagnitude the kernel admitted.
public sealed record Component(
    ComponentFamily Family, ComponentId Designation, SectionProfile Profile, IfcBinding Ifc,
    Coring Coring, ComponentStandard Standard, MaterialId SubstanceId, MaterialId AppearanceId,
    Option<PropertyBag> Detail) {

    const double LaminateBuildTolMm = 0.5;   // ply-sum-vs-overall closure: half a millimetre absorbs published rounding, never a missing ply

    public ComponentClass Class => Family.Class;
    public string IfcEntity => Ifc.Entity;
    public string PredefinedToken => Ifc.Predefined;
    public PositiveMagnitude CrossNominalMm => Family.CrossNominal(Profile);
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm => Profile.GrossRectangleMm;

    public static Fin<Component> Of(
        ComponentFamily family, string designation, SectionProfile profile, IfcBinding ifc,
        Coring coring, ComponentStandard standard, MaterialId substanceId, MaterialId appearanceId,
        Option<PropertyBag> detail, Op key) =>
        from id in ComponentId.Validate(designation, CultureInfo.InvariantCulture, out ComponentId built) is { } error
            ? Fin.Fail<ComponentId>(ComponentFault.Designation(key, $"<malformed-designation:{designation}:{error.Message}>"))
            : Fin.Succ(built)
        from admitted in guard(family.Admits(profile), ComponentFault.Family(key, $"<family-profile-mismatch:{family.Key}>"))
        from voided in guard(coring.VoidFraction is >= 0.0 and < 1.0, ComponentFault.Coring(key, $"<void-fraction-out-of-range:{coring.VoidFraction:R}>"))
        from laminated in guard(profile is not SectionProfile.Layered laminate
                || Math.Abs(laminate.Plies.Sum(static p => p.ThicknessMm.Value) - laminate.OverallMm.Value) <= LaminateBuildTolMm,
            ComponentFault.Dimension(key, $"<laminate-build-mismatch:{designation}>"))
        from laned in guard(detail.IsSome == (family.Lane != DetailLane.None),
            ComponentFault.Family(key, $"<detail-lane-mismatch:{family.Key}:{family.Lane}>"))
        select new Component(family, id, profile, ifc, coring, standard, substanceId, appearanceId, detail);
}
```

## [03]-[SECTION_PROFILE]

- Owner: `SectionProfile` the closed cross-section `[Union]` — the `IfcParameterizedProfileDef` set carried natively with NAMED `PositiveMagnitude` dimensions, widened by the domain arms the ten families need (`CellularRectangle` per-cell voids, `Layered` plies, `Corrugated` deck fold, `Nominal` bond-line, `Catalogued` published identity, `Outline` typed free tail); `VoidCell` the fill-state cell row; `Ply` the laminate row.
- Cases: twenty-one arms; gross bounding `(WidthMm, DepthMm)` facts are BASE-CONSTRUCTOR STATE declared once per arm — no interior switch exists on this axis; the ONE dispatch site is `SectionSolver.Solve`. Growth cadence is the profile schema (buildingSMART parameterized-profile set), never thing cadence.
- Entry: construction is RAILED — each arm carries an `Of` factory returning `Fin<SectionProfile>` that lifts magnitudes through `key.AcceptValidated<PositiveMagnitude>` AND validates the non-magnitude slots (non-negative finite fillets/edges, finite slopes/offsets, cell containment PLUS pairwise cell disjointness — an overlapping lattice double-subtracts the net section and voids silently, so it faults at admission — wall-vs-envelope closure) on `ComponentFault.Dimension`; seed folds construct through `Of` INSIDE their `Traverse`, never a throwing lift. `Catalogued` constructs direct — its payload is already-admitted `SteelShape` state.
- Boundary: `VoidCell` is MIN-CORNER anchored in the profile's `[0,WidthMm]×[0,DepthMm]` corner frame (the convention `MasonryCells.Of` and the cmu lattice emit); `Grouted` drives the as-built net (only ungrouted cells void), the grouted fraction, self-weight, and the parallel-path thermal split; `Reinforced` marks bar-bearing cells; geometry-only cells collapse as-built physics to the ungrouted case. `NonNegativeMagnitude` does not exist in `Rasm.Vectors` (verified) — the non-negative/finite edge check is the inline `Edge` guard inside `Of` (R8 resolved).

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Fill-state rides the cell: Grouted drives the as-built net section (only ungrouted cells void), the grouted-cell
// fraction, self-weight, and the parallel-path thermal split; Reinforced marks the bar-bearing cells. XMm/YMm are the
// MIN-CORNER in the profile's [0,Width]x[0,Depth] corner frame; WidthMm/HeightMm the first/second-axis extents.
public readonly record struct VoidCell(double XMm, double YMm, double WidthMm, double HeightMm, bool Grouted = false, bool Reinforced = false);
public readonly record struct Ply(MaterialId Material, PositiveMagnitude ThicknessMm, string Role);

// The closed cross-section vocabulary. CONSTRUCTION IS RAILED: each arm's Of lifts magnitudes and validates the
// non-magnitude slots on ComponentFault.Dimension — a negative fillet, NaN slope, or out-of-bounds cell faults loud
// before it reaches Curves. Pm/Edge/Slope are the three page-local admission kernels every factory composes.
[Union]
public abstract partial record SectionProfile {
    private SectionProfile(PositiveMagnitude widthMm, PositiveMagnitude depthMm) => GrossRectangleMm = (widthMm, depthMm);
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm { get; }

    public sealed record Rectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) select (SectionProfile)new Rectangle(w, d);
    }

    // cmu cells, brick cores/frogs — Curves/Forms emit BOTH the design net (all cells ungrouted) and the as-built net
    // (Grouted cells filled) through ONE code path: the FILL-STATE selects the result, never a second solver call.
    public sealed record CellularRectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, Seq<VoidCell> Cells) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, Seq<VoidCell> cells, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm)
            from contained in guard(cells.ForAll(c => c.WidthMm > 0.0 && c.HeightMm > 0.0 && c.XMm >= 0.0 && c.YMm >= 0.0
                    && double.IsFinite(c.XMm + c.YMm + c.WidthMm + c.HeightMm)
                    && c.XMm + c.WidthMm <= w.Value && c.YMm + c.HeightMm <= d.Value),
                ComponentFault.Dimension(key, $"<cell-outside-profile:{widthMm:R}x{depthMm:R}>"))
            from disjoint in guard(cells.Index().All(a => cells.Index().All(b => b.Index <= a.Index
                    || a.Item.XMm + a.Item.WidthMm <= b.Item.XMm || b.Item.XMm + b.Item.WidthMm <= a.Item.XMm
                    || a.Item.YMm + a.Item.HeightMm <= b.Item.YMm || b.Item.YMm + b.Item.HeightMm <= a.Item.YMm)),
                ComponentFault.Dimension(key, $"<cells-overlap:{widthMm:R}x{depthMm:R}>"))
            select (SectionProfile)new CellularRectangle(w, d, cells);
    }

    public sealed record RectangleHollow(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, PositiveMagnitude WallMm, double InnerFilletMm, double OuterFilletMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, double wallMm, double innerFilletMm, double outerFilletMm, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) from t in key.Pm(wallMm)
            from fi in key.Edge(innerFilletMm) from fo in key.Edge(outerFilletMm)
            from closed in guard(2.0 * t.Value < Math.Min(w.Value, d.Value), ComponentFault.Dimension(key, $"<hollow-wall-consumes-envelope:{wallMm:R}>"))
            select (SectionProfile)new RectangleHollow(w, d, t, fi, fo);
    }

    public sealed record RoundedRectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, PositiveMagnitude RoundingMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, double roundingMm, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) from r in key.Pm(roundingMm)
            from fits in guard(2.0 * r.Value <= Math.Min(w.Value, d.Value), ComponentFault.Dimension(key, $"<rounding-exceeds-envelope:{roundingMm:R}>"))
            select (SectionProfile)new RoundedRectangle(w, d, r);
    }

    public sealed record Circle(PositiveMagnitude DiameterMm) : SectionProfile(DiameterMm, DiameterMm) {
        public static Fin<SectionProfile> Of(double diameterMm, Op key) =>
            key.Pm(diameterMm).Map(static d => (SectionProfile)new Circle(d));
    }

    public sealed record CircleHollow(PositiveMagnitude DiameterMm, PositiveMagnitude WallMm) : SectionProfile(DiameterMm, DiameterMm) {
        public static Fin<SectionProfile> Of(double diameterMm, double wallMm, Op key) =>
            from d in key.Pm(diameterMm) from t in key.Pm(wallMm)
            from closed in guard(2.0 * t.Value < d.Value, ComponentFault.Dimension(key, $"<pipe-wall-consumes-envelope:{wallMm:R}>"))
            select (SectionProfile)new CircleHollow(d, t);
    }

    public sealed record Ellipse(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) select (SectionProfile)new Ellipse(w, d);
    }

    public sealed record IShape(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double EdgeMm, double FlangeSlopeDeg) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double edgeMm, double flangeSlopeDeg, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from e in key.Edge(edgeMm) from s in key.Slope(flangeSlopeDeg)
            select (SectionProfile)new IShape(d, b, tw, tf, f, e, s);
    }

    public sealed record AsymmetricIShape(PositiveMagnitude DepthMm, PositiveMagnitude TopWidthMm, PositiveMagnitude BottomWidthMm, PositiveMagnitude WebMm, PositiveMagnitude TopFlangeMm, PositiveMagnitude BottomFlangeMm, double FilletMm) : SectionProfile(Wider(TopWidthMm, BottomWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double topWidthMm, double bottomWidthMm, double webMm, double topFlangeMm, double bottomFlangeMm, double filletMm, Op key) =>
            from d in key.Pm(depthMm) from bt in key.Pm(topWidthMm) from bb in key.Pm(bottomWidthMm)
            from tw in key.Pm(webMm) from tt in key.Pm(topFlangeMm) from tb in key.Pm(bottomFlangeMm) from f in key.Edge(filletMm)
            select (SectionProfile)new AsymmetricIShape(d, bt, bb, tw, tt, tb, f);
    }

    public sealed record Channel(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double EdgeMm, double FlangeSlopeDeg) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double edgeMm, double flangeSlopeDeg, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from e in key.Edge(edgeMm) from s in key.Slope(flangeSlopeDeg)
            select (SectionProfile)new Channel(d, b, tw, tf, f, e, s);
    }

    public sealed record ColdFormedC(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WallMm, PositiveMagnitude GirthMm, double InnerFilletMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double wallMm, double girthMm, double innerFilletMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(wallMm) from g in key.Pm(girthMm) from f in key.Edge(innerFilletMm)
            from lipped in guard(g.Value < d.Value / 2.0, ComponentFault.Dimension(key, $"<cee-lip-exceeds-half-depth:{girthMm:R}>"))
            select (SectionProfile)new ColdFormedC(d, b, t, g, f);
    }

    public sealed record Tee(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double FlangeEdgeMm, double WebEdgeMm, double FlangeSlopeDeg, double WebSlopeDeg) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeEdgeMm, double webEdgeMm, double flangeSlopeDeg, double webSlopeDeg, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from fe in key.Edge(flangeEdgeMm) from we in key.Edge(webEdgeMm)
            from fs in key.Slope(flangeSlopeDeg) from ws in key.Slope(webSlopeDeg)
            select (SectionProfile)new Tee(d, b, tw, tf, f, fe, we, fs, ws);
    }

    public sealed record Angle(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude ThicknessMm, double FilletMm, double EdgeMm, double LegSlopeDeg) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double thicknessMm, double filletMm, double edgeMm, double legSlopeDeg, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(thicknessMm)
            from f in key.Edge(filletMm) from e in key.Edge(edgeMm) from s in key.Slope(legSlopeDeg)
            select (SectionProfile)new Angle(d, b, t, f, e, s);
    }

    public sealed record Zed(PositiveMagnitude DepthMm, PositiveMagnitude FlangeWidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, double EdgeMm) : SectionProfile(FlangeWidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double flangeWidthMm, double webMm, double flangeMm, double filletMm, double edgeMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(flangeWidthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from e in key.Edge(edgeMm)
            select (SectionProfile)new Zed(d, b, tw, tf, f, e);
    }

    // PJP/CJP groove-weld prep envelope; TopOffsetMm the one-sided bevel shift (signed, finite).
    public sealed record Trapezium(PositiveMagnitude BottomWidthMm, PositiveMagnitude TopWidthMm, PositiveMagnitude DepthMm, double TopOffsetMm) : SectionProfile(Wider(BottomWidthMm, TopWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double bottomWidthMm, double topWidthMm, double depthMm, double topOffsetMm, Op key) =>
            from bb in key.Pm(bottomWidthMm) from bt in key.Pm(topWidthMm) from d in key.Pm(depthMm) from o in key.Slope(topOffsetMm)
            select (SectionProfile)new Trapezium(bb, bt, d, o);
    }

    // Fillet weld cross-section; the structural throat stays the joint family's DEFINED derivation (0.707·leg).
    public sealed record FilletTriangle(PositiveMagnitude LegMm, PositiveMagnitude Leg2Mm) : SectionProfile(LegMm, Leg2Mm) {
        public static Fin<SectionProfile> Of(double legMm, double leg2Mm, Op key) =>
            from a in key.Pm(legMm) from b in key.Pm(leg2Mm) select (SectionProfile)new FilletTriangle(a, b);
    }

    // Steel-deck rib, first-class: one fold generator drives the Curves polyline and the Forms thin-fold supplement.
    public sealed record Corrugated(PositiveMagnitude CoverWidthMm, PositiveMagnitude RibDepthMm, PositiveMagnitude RibPitchMm, PositiveMagnitude GaugeMm, PositiveMagnitude TopFlatMm, PositiveMagnitude BottomFlatMm) : SectionProfile(CoverWidthMm, RibDepthMm) {
        public static Fin<SectionProfile> Of(double coverWidthMm, double ribDepthMm, double ribPitchMm, double gaugeMm, double topFlatMm, double bottomFlatMm, Op key) =>
            from cw in key.Pm(coverWidthMm) from rd in key.Pm(ribDepthMm) from rp in key.Pm(ribPitchMm)
            from g in key.Pm(gaugeMm) from tf in key.Pm(topFlatMm) from bf in key.Pm(bottomFlatMm)
            from folds in guard(tf.Value + bf.Value < rp.Value && rp.Value <= cw.Value, ComponentFault.Dimension(key, $"<deck-flats-exceed-pitch:{ribPitchMm:R}>"))
            select (SectionProfile)new Corrugated(cw, rd, rp, g, tf, bf);
    }

    // Boards, IGUs, membranes; glazing seeds pass widthMm = overallMm preserving the prior square gross projection.
    // The ply-sum-vs-overall closure is Component.Of's laminate guard — one owner, never re-checked here.
    public sealed record Layered(Seq<Ply> Plies, PositiveMagnitude OverallMm, PositiveMagnitude WidthMm) : SectionProfile(WidthMm, OverallMm) {
        public static Fin<SectionProfile> Of(Seq<Ply> plies, double overallMm, double widthMm, Op key) =>
            from stacked in guard(!plies.IsEmpty, ComponentFault.Dimension(key, "<layered-profile-empty>"))
            from o in key.Pm(overallMm) from w in key.Pm(widthMm)
            select (SectionProfile)new Layered(plies, o, w);
    }

    // Continuous joint / adhesive bond-line — unsectioned by design.
    public sealed record Nominal(PositiveMagnitude NominalMm) : SectionProfile(NominalMm, NominalMm) {
        public static Fin<SectionProfile> Of(double nominalMm, Op key) =>
            key.Pm(nominalMm).Map(static n => (SectionProfile)new Nominal(n));
    }

    // The VividOrange published identity — payload pre-admitted by SteelShape.Of, so construction is direct.
    public sealed record Catalogued(SteelShape Shape) : SectionProfile(Shape.Section.WidthMm, Shape.Section.DepthMm);

    // Typed free tail — a Perimeter, never a point-list erasure; the connector stamped-plate silhouette lands here.
    public sealed record Outline(Perimeter Boundary, PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(Perimeter boundary, double widthMm, double depthMm, Op key) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) select (SectionProfile)new Outline(boundary, w, d);
    }

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The three admission kernels every SectionProfile.Of composes: Pm the kernel PositiveMagnitude lift, Edge the
// non-negative finite slot (fillets/edges — NonNegativeMagnitude has no Rasm.Vectors owner, so the guard is inline),
// Slope the finite signed slot (slopes/offsets). C# 14 extension members over the admission Op.
file static class Admit {
    extension(Op key) {
        public Fin<PositiveMagnitude> Pm(double candidateMm) => key.AcceptValidated<PositiveMagnitude>(candidate: candidateMm);
        public Fin<double> Edge(double candidateMm) =>
            double.IsFinite(candidateMm) && candidateMm >= 0.0 ? Fin.Succ(candidateMm) : ComponentFault.Dimension(key, $"<edge-negative-or-nonfinite:{candidateMm:R}>");
        public Fin<double> Slope(double candidate) =>
            double.IsFinite(candidate) ? Fin.Succ(candidate) : ComponentFault.Dimension(key, $"<slope-nonfinite:{candidate:R}>");
    }
}
```

## [04]-[SECTION_SOLVER]

- Owner: `SectionSolver` — ONE solver replaces every per-family perimeter builder: one generated exhaustive `Switch` over the closed profile axis routes the ONE `new SectionProperties((IProfile)…)` Green's-theorem integral (decompile-verified: both `SectionProperties(IProfile)` and `SectionProperties(ISection)` constructors exist — R3 discharged) through the ONE twenty-column `Admit` lift plus a `SectionSupplement` topology row; `Curves` the per-arm profile table; `Forms` the per-arm supplement table.
- Cases: `Curves` lowers each arm onto the TYPED `VividOrange.IProfiles` contract — decompile-verified: `ProfileParts.GetParts` dispatches on `IDoubleAngle`/`IAngle`/`IC`/`IDoubleChannel`/`IChannel`/`ICircularHollow`/`ICircle`/`ICruciform`/`ICustomI`/`IEllipseHollow`/`IEllipse`/`IIParallelFlange`/`II`/`IRectangularHollow`/`IRoundedRectangularHollow`/`IRoundedRectangle`/`IRectangle`/`ITee`/`ITrapezoid`/`IZ` and decomposes fillets, rounded HSS corners, and circular/elliptical edges into EXACT `EllipseQuarterPart`s (an unknown non-`IPerimeter` profile THROWS, so every `Curves` output is a verified-dispatch member by construction); `Areas.CalculateArea(IProfile)` short-circuits an `IPerimeter` to the polyline path, which carries the void-bearing and free-fold arms (`CellularRectangle`/`Corrugated`/`FilletTriangle`/`Outline`).
- Entry: `SectionSolver.Solve(profile, key)` the one dispatch; `SectionSolver.ProfileOf(widthMm, depthMm, key)` the RC gross-outline entry the `reinforcement#RC_SECTION` consumer feeds (preserved from the retired `ParametricSection`, now minting the typed `IRectangle` contract).
- Boundary: `Forms` carries the RELOCATED `steel#STEEL_FAMILY` `SteelStiffness` open-thin-walled algebra (`OpenI`/`MonoI`/`OpenChannel`/`OpenCee`/`MonoTee`/`OpenAngle`/`PointSymmetricZ` fill `Iw` and the asymmetry columns; `FromCatalogue` keys the same cores by `SteelClass`); the singly-symmetric arms source elastic moduli from the carrier-free `.Utility` kernels (`SectionModuli.CalculateSectionModulusYy/Zz`, `Inertiae`, `Areas`) — the direct static path the catalogue documents — never a second carrier. `Layered`/`Nominal` are unsectioned by design and fault loudly if a mis-flagged `Sectioned` row reaches them. A supplement column that is non-positive or non-finite rails `ComponentFault.Section` at the lift, and the lazy carrier reads trap in ONE `Try` lift so a degenerate `Outline` perimeter's THROW rails the same slot (NaN and throw alike) — a degenerate section never seeds the seam and no `VividOrange` exception escapes the boundary.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The plastic/torsion/shear/warping/asymmetry supplement the elastic polygon integral cannot yield. Parametric closed
// forms fill the last four columns 0.0 (a doubly-symmetric solid or closed section places its shear centre AT the
// centroid); the relocated SteelStiffness algebra fills them for the open thin-walled arms.
public readonly record struct SectionSupplement(
    double Zx, double Zy, double J, double Avy, double Avz,
    double Iw = 0.0, double ShearCentreY = 0.0, double ShearCentreZ = 0.0, double Monosymmetry = 0.0);

// ONE solver replaces every per-family perimeter builder: one generated exhaustive Switch over the closed profile axis
// — a new arm is compiler-forced HERE and nowhere else. Every arm routes the ONE VividOrange Green-theorem integral
// through the ONE twenty-column Admit lift, then the topology supplement: Forms closed forms for solid/hollow arms, the
// relocated SteelStiffness thin-walled algebra for open arms, the published-identity route for Catalogued.
public static class SectionSolver {
    public static Fin<ComputedSection> Solve(SectionProfile profile, Op key) =>
        profile.Switch(
            rectangle:         r => Admit(Curves.Rect(r.WidthMm.Value, r.DepthMm.Value), r, Forms.SolidRectangle(r), key),
            cellularRectangle: c => Admit(Curves.RectWithVoids(c.WidthMm.Value, c.DepthMm.Value, c.Cells), c, Forms.NetRectangle(c), key),
            rectangleHollow:   r => Admit(Curves.RectTube(r), r, Forms.BoxTube(r), key),
            roundedRectangle:  r => Admit(Curves.RoundedRect(r), r, Forms.SolidRounded(r), key),
            circle:            c => Admit(Curves.Disc(c), c, Forms.SolidCircle(c), key),
            circleHollow:      c => Admit(Curves.Ring(c), c, Forms.Pipe(c), key),
            ellipse:           e => Admit(Curves.Ellipse(e), e, Forms.SolidEllipse(e), key),
            iShape:            i => Admit(Curves.I(i), i, Forms.OpenI(i), key),
            asymmetricIShape:  i => Admit(Curves.AsymI(i), i, Forms.MonoI(i), key),
            channel:           u => Admit(Curves.U(u), u, Forms.OpenChannel(u), key),
            coldFormedC:       c => Admit(Curves.Cee(c), c, Forms.OpenCee(c), key),
            tee:               t => Admit(Curves.T(t), t, Forms.MonoTee(t), key),
            angle:             l => Admit(Curves.L(l), l, Forms.OpenAngle(l), key),
            zed:               z => Admit(Curves.Z(z), z, Forms.PointSymmetricZ(z), key),
            trapezium:         t => Admit(Curves.Trapezoid(t), t, Forms.SolidTrapezoid(t), key),
            filletTriangle:    f => Admit(Curves.RightTriangle(f), f, Forms.SolidTriangle(f), key),
            corrugated:        d => Admit(Curves.Deck(d), d, Forms.ThinFold(d), key),
            layered:           _ => ComponentFault.Section(key, "<unsectioned-profile:layered>"),
            nominal:           _ => ComponentFault.Section(key, "<unsectioned-profile:nominal>"),
            catalogued:        c => Admit(c.Shape.Profile, c, Forms.FromCatalogue(c.Shape), key),
            outline:           o => Admit(o.Boundary, o, Forms.Integrated(o), key));

    // The RC gross-outline entry the reinforcement RC path and capacity.md feed — the retired ParametricSection.ProfileOf
    // admission preserved, now minting the TYPED IRectangle contract so the RC concrete outline integrates exactly.
    public static Fin<IProfile> ProfileOf(double widthMm, double depthMm, Op key) =>
        widthMm > 0.0 && depthMm > 0.0 && double.IsFinite(widthMm) && double.IsFinite(depthMm)
            ? Fin.Succ(Curves.Rect(widthMm, depthMm))
            : ComponentFault.Dimension(key, $"<component-perimeter-degenerate:{widthMm:R}x{depthMm:R}>");

    // The retired ParametricSection.Admit twenty-column PositiveMagnitude lift, widened to read the SectionSupplement's
    // Iw/ShearCentreY/ShearCentreZ/Monosymmetry instead of hardcoded zeros — one lift for all twenty-one solvable arms.
    // The lazy Green's-theorem reads trap in ONE Try lift: a degenerate caller-supplied Outline perimeter faults
    // ComponentFault.Section whether the integral THROWS or nets NaN — no exception escapes the owning boundary.
    // Depth/Width are the arm's proven base-constructor gross pair (never re-proven); every solver/supplement column
    // re-labels its value-object Fin onto the Section slot (a degenerate net column is a SECTION failure, not a raw
    // dimension input); the four signed columns admit on a single finiteness gate. AxisDistanceMm stays 0.0 — the RC
    // cover rides the reinforcement#RC_SECTION ConcreteSectionProperties path.
    static Fin<ComputedSection> Admit(IProfile profile, SectionProfile source, SectionSupplement s, Op key) =>
        from p in Try.lift(() => {
                SectionProperties carrier = new(profile);
                return (Area: carrier.Area.SquareMillimeters,
                    Ix: carrier.MomentOfInertiaYy.MillimetersToTheFourth, Iy: carrier.MomentOfInertiaZz.MillimetersToTheFourth,
                    Sx: carrier.ElasticSectionModulusYy.CubicMillimeters, Sy: carrier.ElasticSectionModulusZz.CubicMillimeters,
                    Rx: carrier.RadiusOfGyrationYy.Millimeters, Ry: carrier.RadiusOfGyrationZz.Millimeters,
                    Perim: carrier.Perimeter.Millimeters);
            }).Run().MapFail(e => (Error)ComponentFault.Section(key, $"<section-integral-throw:{e.Message}>"))
        from area in Section(p.Area, key)
        from ix in Section(p.Ix, key)
        from iy in Section(p.Iy, key)
        from sx in Section(p.Sx, key)
        from sy in Section(p.Sy, key)
        from rx in Section(p.Rx, key)
        from ry in Section(p.Ry, key)
        from zx in Section(s.Zx, key)
        from zy in Section(s.Zy, key)
        from jj in Section(s.J, key)
        from avy in Section(s.Avy, key)
        from avz in Section(s.Avz, key)
        from perim in Section(p.Perim, key)
        from signed in guard(double.IsFinite(s.Iw) && double.IsFinite(s.ShearCentreY) && double.IsFinite(s.ShearCentreZ) && double.IsFinite(s.Monosymmetry),
            ComponentFault.Section(key, "<section-supplement-nonfinite>"))
        select new ComputedSection(area, ix, iy, sx, sy, rx, ry, zx, zy, jj, IwMm6: s.Iw, avy, avz,
            DepthMm: source.GrossRectangleMm.DepthMm, WidthMm: source.GrossRectangleMm.WidthMm, HeatedPerimeterMm: perim,
            AxisDistanceMm: 0.0, ShearCentreYMm: s.ShearCentreY, ShearCentreZMm: s.ShearCentreZ, MonosymmetryFactor: s.Monosymmetry);

    // The section-column admission: a positive finite SI-millimetre magnitude into the kernel PositiveMagnitude, the
    // value-object's own Fin RE-LABELLED to the Section fault slot so a structural consumer reads the true cause.
    static Fin<PositiveMagnitude> Section(double mm, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: mm).MapFail(_ => (Error)ComponentFault.Section(key, $"<section-column-nonpositive:{mm:R}>"));

    // --- [CURVES]
    // The per-arm profile table: each arm lowers onto its TYPED VividOrange.IProfiles contract so the solver's internal
    // part decomposition integrates fillets and curved edges EXACTLY (IIParallelFlange fillets, rounded-HSS corners,
    // circular/elliptical arcs -> EllipseQuarterPart); the polyline Perimeter path survives only for the void-bearing
    // and free-fold arms no contract covers. The typed rows are the file-scoped records below this owner.
    internal static class Curves {
        public static IProfile Rect(double w, double d) => new RectRow(Mm(w), Mm(d));
        public static IProfile RectWithVoids(double w, double d, Seq<VoidCell> cells) =>
            new Perimeter(CornerLoop(0.0, 0.0, w, d),
                cells.Filter(static c => !c.Grouted).Map(c => CornerLoop(c.XMm, c.YMm, c.WidthMm, c.HeightMm)).ToList());
        public static IProfile RectTube(SectionProfile.RectangleHollow r) =>
            r.OuterFilletMm > 0.0
                ? new RoundedBoxRow(Mm(r.WidthMm.Value), Mm(r.DepthMm.Value), Mm(r.WidthMm.Value - 2.0 * r.OuterFilletMm), Mm(r.DepthMm.Value - 2.0 * r.OuterFilletMm), Mm(r.WallMm.Value))
                : new BoxRow(Mm(r.WidthMm.Value), Mm(r.DepthMm.Value), Mm(r.WallMm.Value));
        public static IProfile RoundedRect(SectionProfile.RoundedRectangle r) =>
            new RoundedRectRow(Mm(r.WidthMm.Value), Mm(r.DepthMm.Value), Mm(r.WidthMm.Value - 2.0 * r.RoundingMm.Value), Mm(r.DepthMm.Value - 2.0 * r.RoundingMm.Value));
        public static IProfile Disc(SectionProfile.Circle c) => new DiscRow(Mm(c.DiameterMm.Value));
        public static IProfile Ring(SectionProfile.CircleHollow c) => new RingRow(Mm(c.DiameterMm.Value), Mm(c.WallMm.Value));
        public static IProfile Ellipse(SectionProfile.Ellipse e) => new EllipseRow(Mm(e.DepthMm.Value), Mm(e.WidthMm.Value));
        public static IProfile I(SectionProfile.IShape i) => new IBeamRow(Mm(i.DepthMm.Value), Mm(i.WidthMm.Value), Mm(i.FlangeMm.Value), Mm(i.WebMm.Value), Mm(i.FilletMm));
        public static IProfile AsymI(SectionProfile.AsymmetricIShape i) =>   // ICustomI carries no fillet slot; FilletMm stays a Forms J term
            new MonoIRow(Mm(i.DepthMm.Value), Mm(i.TopWidthMm.Value), Mm(i.BottomWidthMm.Value), Mm(i.TopFlangeMm.Value), Mm(i.BottomFlangeMm.Value), Mm(i.WebMm.Value));
        public static IProfile U(SectionProfile.Channel u) => new ChannelRow(Mm(u.DepthMm.Value), Mm(u.WidthMm.Value), Mm(u.WebMm.Value), Mm(u.FlangeMm.Value));
        public static IProfile Cee(SectionProfile.ColdFormedC c) => new CeeRow(Mm(c.DepthMm.Value), Mm(c.WidthMm.Value), Mm(c.WallMm.Value), Mm(c.WallMm.Value), Mm(c.GirthMm.Value));
        public static IProfile T(SectionProfile.Tee t) => new TeeRow(Mm(t.DepthMm.Value), Mm(t.WidthMm.Value), Mm(t.WebMm.Value), Mm(t.FlangeMm.Value));
        public static IProfile L(SectionProfile.Angle l) => new AngleRow(Mm(l.DepthMm.Value), Mm(l.WidthMm.Value), Mm(l.ThicknessMm.Value), Mm(l.ThicknessMm.Value));
        public static IProfile Z(SectionProfile.Zed z) =>                    // thin-gauge Z: one uniform IZ.Thickness = WebMm; FlangeMm feeds Forms only
            new ZedRow(Mm(z.DepthMm.Value), Mm(z.FlangeWidthMm.Value), Mm(z.FlangeWidthMm.Value), Mm(z.WebMm.Value), Mm(z.EdgeMm), Mm(z.EdgeMm));
        public static IProfile Trapezoid(SectionProfile.Trapezium t) =>      // ITrapezoid is axis-symmetric; TopOffsetMm shifts no area moment about the bending axes
            new TrapezoidRow(Mm(t.DepthMm.Value), Mm(t.TopWidthMm.Value), Mm(t.BottomWidthMm.Value));
        public static IProfile RightTriangle(SectionProfile.FilletTriangle f) =>
            new Perimeter(new List<ILocalPoint2d> { Pt(0.0, 0.0), Pt(f.LegMm.Value, 0.0), Pt(0.0, f.Leg2Mm.Value) });

        // The deck fold trace: n full ribs across the cover width, crest surface then the gauge-offset return path —
        // the thin-plate vertical-offset outline the polyline integral nets (the fold generator, never a per-rib table).
        public static IProfile Deck(SectionProfile.Corrugated d) {
            double rp = d.RibPitchMm.Value, g = d.GaugeMm.Value, tf = d.TopFlatMm.Value, bf = d.BottomFlatMm.Value, rd = d.RibDepthMm.Value;
            int ribs = Math.Max(1, (int)Math.Floor(d.CoverWidthMm.Value / rp));
            double web = (rp - tf - bf) / 2.0;
            Seq<(double X, double Z)> crest = toSeq(Enumerable.Range(0, ribs)).Bind(i =>
                Seq((i * rp, 0.0), (i * rp + bf, 0.0), (i * rp + bf + web, rd), (i * rp + bf + web + tf, rd), (i * rp + rp, 0.0)));
            return new Perimeter(new LocalPolyline2d((crest + crest.Rev().Map(p => (p.X, p.Z + g))).Map(p => Pt(p.X, p.Z)).ToList()));
        }

        static ILocalPolyline2d CornerLoop(double x0, double y0, double w, double h) =>
            new LocalPolyline2d(new List<ILocalPoint2d> { Pt(x0, y0), Pt(x0 + w, y0), Pt(x0 + w, y0 + h), Pt(x0, y0 + h) });   // Perimeter auto-closes
        static ILocalPoint2d Pt(double y, double z) => new LocalPoint2d(Mm(y), Mm(z));
        static Length Mm(double mm) => Length.FromMillimeters(mm);
    }

    // --- [FORMS]
    // The closed-form supplement table: the retired RectanglePlastics/HollowPlastics/Torsion (Roark Table 10.1) rows
    // plus the RELOCATED steel.md SteelStiffness per-topology algebra. Arms needing elastic moduli read the carrier-free
    // .Utility kernels over their own Curves row (the singly-symmetric factors are policy values, never re-derivations).
    internal static class Forms {
        public static SectionSupplement SolidRectangle(SectionProfile.Rectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);
        public static SectionSupplement SolidRounded(SectionProfile.RoundedRectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);   // corner relief is the elastic integral's; plastic/J keep the gross-rectangle bound

        // The net superposition over UNGROUTED cells (a grouted cell integrates as solid): plastic first-moment
        // subtraction about the gross centroidal axes with the exact spans-the-axis/one-side piecewise term (deeper
        // than the retired centred-void approximation), Roark J subtraction, net shear area.
        public static SectionSupplement NetRectangle(SectionProfile.CellularRectangle c) {
            double w = c.WidthMm.Value, d = c.DepthMm.Value;
            Seq<VoidCell> open = c.Cells.Filter(static v => !v.Grouted);
            double zx = w * d * d / 4.0 - open.Sum(v => PlasticCut(v.WidthMm, v.HeightMm, v.YMm + v.HeightMm / 2.0 - d / 2.0));
            double zy = d * w * w / 4.0 - open.Sum(v => PlasticCut(v.HeightMm, v.WidthMm, v.XMm + v.WidthMm / 2.0 - w / 2.0));
            double j = RectJ(w, d) - open.Sum(static v => RectJ(v.WidthMm, v.HeightMm));
            double net = w * d - open.Sum(static v => v.WidthMm * v.HeightMm);
            return new(zx, zy, j, net, net);
        }
        // First moment of a b-wide, h-tall cut whose centre sits yBar off the axis: straddling -> b*(h²/4 + yBar²),
        // fully one side -> b*h*|yBar| — the exact |y| strip integral.
        static double PlasticCut(double b, double h, double yBar) =>
            Math.Abs(yBar) < h / 2.0 ? b * (h * h / 4.0 + yBar * yBar) : b * h * Math.Abs(yBar);

        public static SectionSupplement BoxTube(SectionProfile.RectangleHollow r) {
            double b = r.WidthMm.Value, h = r.DepthMm.Value, t = r.WallMm.Value, bi = b - 2.0 * t, hi = h - 2.0 * t;
            return new(0.25 * (b * h * h - bi * hi * hi), 0.25 * (h * b * b - hi * bi * bi), ClosedRectJ(b, h, t), 2.0 * h * t, 2.0 * b * t);
        }

        public static SectionSupplement SolidCircle(SectionProfile.Circle c) {
            double dia = c.DiameterMm.Value, a = Math.PI * dia * dia / 4.0;
            return new(dia * dia * dia / 6.0, dia * dia * dia / 6.0, Math.PI * Math.Pow(dia, 4.0) / 32.0, a, a);
        }

        // Round tube (relocated verbatim): plastic (D³-Di³)/6, polar J, AISC §G5 Av = A/2 per axis.
        public static SectionSupplement Pipe(SectionProfile.CircleHollow c) => RoundTube(c.DiameterMm.Value, c.WallMm.Value);
        static SectionSupplement RoundTube(double od, double t) {
            double ri = Math.Max(0.0, od / 2.0 - t), ro = od / 2.0;
            double z = (Math.Pow(od, 3.0) - Math.Pow(2.0 * ri, 3.0)) / 6.0;
            double area = Math.PI * (ro * ro - ri * ri);
            return new(z, z, Math.PI * 0.5 * (Math.Pow(ro, 4.0) - Math.Pow(ri, 4.0)), area * 0.5, area * 0.5);
        }

        public static SectionSupplement SolidEllipse(SectionProfile.Ellipse e) {
            double a = e.WidthMm.Value / 2.0, b = e.DepthMm.Value / 2.0, area = Math.PI * a * b;
            return new(4.0 * a * b * b / 3.0, 4.0 * b * a * a / 3.0, Math.PI * Math.Pow(a, 3.0) * Math.Pow(b, 3.0) / (a * a + b * b), area, area);
        }

        // RELOCATED steel.md SteelStiffness — doubly-symmetric open I: rectangular-component plastic moduli, thin-walled
        // St-Venant J = Σb·t³/3, warping Iw = tf·bf³·hₒ²/24, AISC §G web/flange shear split, zero offsets/βy.
        public static SectionSupplement OpenI(SectionProfile.IShape i) => OpenIDims(i.DepthMm.Value, i.WidthMm.Value, i.WebMm.Value, i.FlangeMm.Value);
        static SectionSupplement OpenIDims(double depth, double bf, double tw, double tf) {
            double h = Math.Max(0.0, depth - 2.0 * tf), ho = depth - tf;
            double zx = bf * tf * (depth - tf) + 0.25 * tw * h * h;
            double zy = 0.5 * tf * bf * bf + 0.25 * h * tw * tw;
            double j = (2.0 * bf * tf * tf * tf + h * tw * tw * tw) / 3.0;
            double iw = tf * bf * bf * bf * ho * ho / 24.0;
            return new(zx, zy, j, depth * tw, 5.0 / 3.0 * bf * tf, Iw: iw);
        }

        // Mono-symmetric I: two-flange warping Iw = hₒ²·If1·If2/(If1+If2); shear centre between flanges by flange-inertia
        // fraction (thin-walled mid-height reference); SN030 βy = 0.9·hₒ·(2ψf−1); plastic by the mono-I policy factors
        // (1.12 major / 1.5 weak) over the exact ICustomI kernel moduli.
        public static SectionSupplement MonoI(SectionProfile.AsymmetricIShape i) {
            double d = i.DepthMm.Value, tw = i.WebMm.Value, tt = i.TopFlangeMm.Value, tb = i.BottomFlangeMm.Value;
            double bt = i.TopWidthMm.Value, bb = i.BottomWidthMm.Value, ho = d - (tt + tb) / 2.0, h = Math.Max(0.0, d - tt - tb);
            double ift = tt * Math.Pow(bt, 3.0) / 12.0, ifb = tb * Math.Pow(bb, 3.0) / 12.0, psi = ift / (ift + ifb);
            IProfile p = Curves.AsymI(i);
            double j = (bt * tt * tt * tt + bb * tb * tb * tb + h * tw * tw * tw + 2.0 * i.FilletMm * Math.Pow(tw, 3.0)) / 3.0;
            return new(1.12 * Sx(p), 1.5 * Sy(p), j, d * tw, 5.0 / 6.0 * (bt * tt + bb * tb),
                Iw: ho * ho * ift * ifb / (ift + ifb), ShearCentreZ: ho * (2.0 * psi - 1.0) / 2.0, Monosymmetry: 0.9 * ho * (2.0 * psi - 1.0));
        }

        // RELOCATED — channel: I-component plastic moduli, half-I warping, minor-axis shear-centre offset
        // e = hₒ²·tf·bf³ / (12·Iw_channel), βy = 0 (symmetric in the bending plane).
        public static SectionSupplement OpenChannel(SectionProfile.Channel u) => ChannelDims(u.DepthMm.Value, u.WidthMm.Value, u.WebMm.Value, u.FlangeMm.Value);
        static SectionSupplement ChannelDims(double depth, double bf, double tw, double tf) {
            double h = Math.Max(0.0, depth - 2.0 * tf), ho = depth - tf;
            double zx = bf * tf * (depth - tf) + 0.25 * tw * h * h;
            double zy = 0.5 * tf * bf * bf + 0.25 * h * tw * tw;
            double iw = 0.5 * tf * bf * bf * bf * ho * ho / 24.0;
            return new(zx, zy, (2.0 * bf * tf * tf * tf + h * tw * tw * tw) / 3.0, depth * tw, 5.0 / 3.0 * bf * tf,
                Iw: iw, ShearCentreZ: ho * ho * tf * bf * bf * bf / Math.Max(1.0, 12.0 * iw));
        }

        // RELOCATED — thin-gauge lipped cee: monosymmetric like a channel, lip terms in J, effective-section design
        // governs so the plastic slots carry the kernel elastic moduli (the AISI convention, a policy value not a loss).
        public static SectionSupplement OpenCee(SectionProfile.ColdFormedC c) {
            double d = c.DepthMm.Value, b = c.WidthMm.Value, t = c.WallMm.Value, lip = c.GirthMm.Value, ho = d - t;
            double iw = 0.5 * t * b * b * b * ho * ho / 24.0;
            IProfile p = Curves.Cee(c);
            return new(Sx(p), Sy(p), (2.0 * (b + lip) * t * t * t + (d - 2.0 * t) * t * t * t) / 3.0, d * t, d * t,
                ShearCentreZ: ho * ho * t * b * b * b / Math.Max(1.0, 12.0 * iw));
        }

        // RELOCATED — tee: stem web is the major shear, no warping couple, shear centre at the flange, singly-symmetric
        // βy = 0.9·hₒ; the 1.7/0.85 plastic factors ride the exact ITee kernel moduli.
        public static SectionSupplement MonoTee(SectionProfile.Tee t) {
            double d = t.DepthMm.Value, bf = t.WidthMm.Value, tw = t.WebMm.Value, tf = t.FlangeMm.Value, ho = d - tf;
            IProfile p = Curves.T(t);
            return new(1.7 * Sx(p), 0.85 * Sy(p), (bf * tf * tf * tf + (d - tf) * tw * tw * tw) / 3.0, d * tw, 5.0 / 3.0 * bf * tf,
                ShearCentreZ: ho * 0.5, Monosymmetry: 0.9 * ho);
        }

        // RELOCATED — angle: shear centre at the heel (both legs), torsion-only, no warping, reduced βy.
        public static SectionSupplement OpenAngle(SectionProfile.Angle l) {
            double d = l.DepthMm.Value, b = l.WidthMm.Value, t = l.ThicknessMm.Value, ho = d - t;
            IProfile p = Curves.L(l);
            return new(1.5 * Sx(p), 1.5 * Sy(p), (b + d) * t * t * t / 3.0, d * t, b * t,
                ShearCentreY: 0.5 * b, ShearCentreZ: ho * 0.5, Monosymmetry: 0.45 * ho);
        }

        // Point-symmetric lipped Z: component plastic moduli, thin-open J incl. lips, Roark Z warping
        // Iw = (t·b³·h²/12)·(b+2h)/(2b+h); the shear centre sits AT the centroid (zero offsets, zero βy).
        public static SectionSupplement PointSymmetricZ(SectionProfile.Zed z) {
            double d = z.DepthMm.Value, b = z.FlangeWidthMm.Value, tw = z.WebMm.Value, tf = z.FlangeMm.Value, lip = z.EdgeMm, h = d - 2.0 * tf;
            double zx = 2.0 * b * tf * (d - tf) / 2.0 + 0.25 * tw * h * h + 2.0 * lip * tw * (d / 2.0 - lip / 2.0);
            double zy = tf * b * b + 0.25 * h * tw * tw;
            return new(zx, zy, (2.0 * (b + lip) * tf * tf * tf + h * tw * tw * tw) / 3.0, d * tw, 2.0 * b * tf,
                Iw: tw * Math.Pow(b, 3.0) * d * d / 12.0 * (b + 2.0 * d) / (2.0 * b + d));
        }

        // The odd solid arms ride the ONE kernel-backed general row: plastic = shape-factor policy × exact kernel
        // modulus (trapezoid 1.5 rectangle-family bound, triangle 2.34, free outline 1.0 conservative), Saint-Venant
        // J ≈ A⁴/(40·Ip) — the classical solid approximation — and gross shear.
        public static SectionSupplement SolidTrapezoid(SectionProfile.Trapezium t) => General(Curves.Trapezoid(t), 1.5, 1.5);
        public static SectionSupplement SolidTriangle(SectionProfile.FilletTriangle f) => General(Curves.RightTriangle(f), 2.34, 2.34);
        public static SectionSupplement Integrated(SectionProfile.Outline o) => General(o.Boundary, 1.0, 1.0);
        static SectionSupplement General(IProfile p, double fx, double fy) {
            double a = Areas.CalculateArea(p).SquareMillimeters;
            double ip = Inertiae.CalculateInertiaYy(p).MillimetersToTheFourth + Inertiae.CalculateInertiaZz(p).MillimetersToTheFourth;
            return new(fx * Sx(p), fy * Sy(p), Math.Pow(a, 4.0) / (40.0 * ip), a, a);
        }

        // Thin-fold deck: kernel elastic moduli (folded-plate PNA is effective-width territory, not a closed form),
        // developed-length thin-open J, inclined-web major shear, flat-sum minor shear.
        public static SectionSupplement ThinFold(SectionProfile.Corrugated d) {
            double rp = d.RibPitchMm.Value, g = d.GaugeMm.Value, tf = d.TopFlatMm.Value, bf = d.BottomFlatMm.Value, rd = d.RibDepthMm.Value;
            int ribs = Math.Max(1, (int)Math.Floor(d.CoverWidthMm.Value / rp));
            double web = Math.Sqrt(Math.Pow((rp - tf - bf) / 2.0, 2.0) + rd * rd);
            IProfile p = Curves.Deck(d);
            return new(Sx(p), Sy(p), ribs * (tf + bf + 2.0 * web) * g * g * g / 3.0, ribs * 2.0 * web * g, ribs * (tf + bf) * g);
        }

        // The published-identity route: the SteelClass topology keys the relocated cores over the admitted SectionDims;
        // the singly-symmetric arms read the kernels over the catalogued IProfile (fillet/corner parts included).
        public static SectionSupplement FromCatalogue(SteelShape s) {
            double d = s.Section.DepthMm.Value, b = s.Section.WidthMm.Value, tw = s.Section.WebMm.Value, tf = s.Section.FlangeMm.Value;
            return s.Class.Switch(
                iShape:      () => OpenIDims(d, b, tw, tf),
                uShape:      () => ChannelDims(d, b, tw, tf),
                lShape:      () => new SectionSupplement(1.5 * Sx(s.Profile), 1.5 * Sy(s.Profile), (b + d) * tw * tw * tw / 3.0, d * tw, b * tw, ShearCentreY: 0.5 * b, ShearCentreZ: 0.5 * (d - tf), Monosymmetry: 0.45 * (d - tf)),
                doubleAngle: () => new SectionSupplement(1.6 * Sx(s.Profile), 1.5 * Sy(s.Profile), 2.0 * (b + d) * tw * tw * tw / 3.0, 2.0 * d * tw, 2.0 * b * tw, ShearCentreZ: 0.5 * (d - tf), Monosymmetry: 0.9 * (d - tf)),
                hssRect:     () => new SectionSupplement(0.25 * (b * d * d - Math.Max(0.0, b - 2.0 * tw) * Math.Pow(Math.Max(0.0, d - 2.0 * tw), 2.0)), 0.25 * (d * b * b - Math.Max(0.0, d - 2.0 * tw) * Math.Pow(Math.Max(0.0, b - 2.0 * tw), 2.0)), ClosedRectJ(b, d, tw), 2.0 * d * tw, 2.0 * b * tw),
                hssRound:    () => RoundTube(d, tw),
                tee:         () => new SectionSupplement(1.7 * Sx(s.Profile), 0.85 * Sy(s.Profile), (b * tf * tf * tf + (d - tf) * tw * tw * tw) / 3.0, d * tw, 5.0 / 3.0 * b * tf, ShearCentreZ: 0.5 * (d - tf), Monosymmetry: 0.9 * (d - tf)),
                composite:   () => OpenIDims(d, b, tw, tf),    // steel core is its I-shape; the slab adds capacity, not stiffness, here
                coldFormed:  () => new SectionSupplement(Sx(s.Profile), Sy(s.Profile), (d - 2.0 * tw) * tw * tw * tw / 3.0 + 2.0 * b * tw * tw * tw / 3.0, d * tw, d * tw, ShearCentreZ: 0.5 * (d - tf)));
        }

        // Roark Table 10.1 solid-rectangle St-Venant J (a/b the long/short side) and the Bredt closed thin-walled tube
        // J = 4·A_m²·t / perimeter_m — the two shared torsion kernels, computed once, never transcribed per arm.
        static double RectJ(double a, double b) {
            double lng = Math.Max(a, b), sht = Math.Min(a, b);
            return lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));
        }
        static double ClosedRectJ(double b, double h, double t) {
            double bm = Math.Max(0.0, b - t), hm = Math.Max(0.0, h - t);
            return bm <= 0.0 || hm <= 0.0 ? 0.0 : 4.0 * bm * bm * hm * hm * t / (2.0 * (bm + hm));
        }
        static SectionSupplement SolidRect(double w, double d) { double a = w * d; return new(w * d * d / 4.0, d * w * w / 4.0, RectJ(w, d), a, a); }

        // The carrier-free Green's-theorem kernels — the direct .Utility statics, no second SectionProperties carrier.
        static double Sx(IProfile p) => SectionModuli.CalculateSectionModulusYy(p).CubicMillimeters;
        static double Sy(IProfile p) => SectionModuli.CalculateSectionModulusZz(p).CubicMillimeters;
    }
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The typed-contract rows Curves mints: file-scoped boundary records implementing the verified VividOrange.IProfiles
// floor so ProfileParts decomposes them onto its exact TrapezoidalPart/EllipseQuarterPart integrals. ITaxonomySerializable
// is an empty marker (verified); IProfile adds only Description. Positional names MATCH the contract members exactly.
file sealed record RectRow(Length Width, Length Height) : IRectangle { public string Description => nameof(RectRow); }
file sealed record BoxRow(Length Width, Length Height, Length Thickness) : IRectangularHollow { public string Description => nameof(BoxRow); }
file sealed record RoundedRectRow(Length Width, Length Height, Length FlatWidth, Length FlatHeight) : IRoundedRectangle { public string Description => nameof(RoundedRectRow); }
file sealed record RoundedBoxRow(Length Width, Length Height, Length FlatWidth, Length FlatHeight, Length Thickness) : IRoundedRectangularHollow { public string Description => nameof(RoundedBoxRow); }
file sealed record DiscRow(Length Diameter) : ICircle { public string Description => nameof(DiscRow); }
file sealed record RingRow(Length Diameter, Length Thickness) : ICircularHollow { public string Description => nameof(RingRow); }
file sealed record EllipseRow(Length Height, Length Width) : IEllipse { public string Description => nameof(EllipseRow); }
file sealed record IBeamRow(Length Height, Length Width, Length FlangeThickness, Length WebThickness, Length FilletRadius) : IIParallelFlange { public string Description => nameof(IBeamRow); }
file sealed record MonoIRow(Length Height, Length TopFlangeWidth, Length BottomFlangeWidth, Length TopFlangeThickness, Length BottomFlangeThickness, Length WebThickness) : ICustomI { public string Description => nameof(MonoIRow); }
file sealed record ChannelRow(Length Height, Length Width, Length WebThickness, Length FlangeThickness) : IChannel { public string Description => nameof(ChannelRow); }
file sealed record CeeRow(Length Height, Length Width, Length WebThickness, Length FlangeThickness, Length Lip) : IC { public string Description => nameof(CeeRow); }
file sealed record TeeRow(Length Height, Length Width, Length WebThickness, Length FlangeThickness) : ITee { public string Description => nameof(TeeRow); }
file sealed record AngleRow(Length Height, Length Width, Length WebThickness, Length FlangeThickness) : IAngle { public string Description => nameof(AngleRow); }
file sealed record ZedRow(Length Height, Length TopFlangeWidth, Length BottomFlangeWidth, Length Thickness, Length TopLip, Length BottomLip) : IZ { public string Description => nameof(ZedRow); }
file sealed record TrapezoidRow(Length Height, Length TopWidth, Length BottomWidth) : ITrapezoid { public string Description => nameof(TrapezoidRow); }
```

## [05]-[COMPUTED_SECTION]

The FROZEN twenty-column receipt (`[FROZEN_INVARIANTS]`): field names, types, and order are byte-identical wire law — `Projection/component#COMPONENT_PROJECTOR` `SeamSection` lifts the whole set onto the seam `SectionProperties` (mm→SI typed `MeasureValue`s through `QuantityRow`, `Iw` fifth after `J`, the three asymmetry columns last), so a `Rasm.Compute` structural/fire runner reads `graph.SectionOf(member)` without re-resolving or admitting VividOrange. The elastic columns and `HeatedPerimeterMm` come from the ONE polygon integral; `ZxMm3`/`ZyMm3`/`JMm4`/`AvyMm2`/`AvzMm2` from the `Forms` supplement; `IwMm6` (EN 1993-1-1 §6.3.2 lateral-torsional-buckling input) is positive ONLY for an open thin-walled arm; `ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor` (the EN 1993-1-1 NCCI SN030 general-LTB inputs) are signed zero-valid plain doubles — engineering-zero for every doubly-symmetric arm, non-zero for a channel/tee/angle/mono-I, so the seam `IsDoublySymmetric` reads zero-as-symmetric EXACTLY. `AxisDistanceMm` is the EN 1992-1-2 cover-to-reinforcement, zero for every non-RC section — the RC value rides the `reinforcement#RC_SECTION` `ConcreteSectionProperties` path.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// FROZEN twenty columns, order byte-identical (wire law). PositiveMagnitude columns are the always-positive solver
// facts; the four signed doubles (IwMm6/ShearCentreYMm/ShearCentreZMm/MonosymmetryFactor) and AxisDistanceMm are the
// zero-admitting engineering columns.
public readonly record struct ComputedSection(
    PositiveMagnitude AreaMm2,
    PositiveMagnitude IxMm4,
    PositiveMagnitude IyMm4,
    PositiveMagnitude SxMm3,
    PositiveMagnitude SyMm3,
    PositiveMagnitude RxMm,
    PositiveMagnitude RyMm,
    PositiveMagnitude ZxMm3,
    PositiveMagnitude ZyMm3,
    PositiveMagnitude JMm4,
    double IwMm6,
    PositiveMagnitude AvyMm2,   // MAJOR-axis shear area (the web of an OPEN steel shape — the §G design web shear reads it); the seam AvY
    PositiveMagnitude AvzMm2,   // MINOR-axis shear area (the flanges); the seam AvZ — symmetric solid arms fill BOTH with the net area
    PositiveMagnitude DepthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeatedPerimeterMm,
    double AxisDistanceMm,
    double ShearCentreYMm,      // centroid→shear-centre y offset (mm); non-zero for an angle heel — the seam ShearCentreY
    double ShearCentreZMm,      // centroid→shear-centre z offset (mm); non-zero for a channel/tee/mono-I — the seam ShearCentreZ
    double MonosymmetryFactor) {// the SN030 βy; signed, zero-valid — NEVER a PositiveMagnitude

    // The WEAK-axis radius the column-buckling check governs about — a derived read over the two solver radii, never a
    // stored column that drifts from the pair the polygon integral supplies.
    public double GoverningRadiusMm => Math.Min(RxMm.Value, RyMm.Value);
}
```

## [06]-[CATALOGUE]

- Owner: `ComponentRow` the campaign row currency (`[ROW_CURRENCIES]`); `ComponentCatalogue` the ONE fold computing BOTH frozen maps in one pass.
- Cases: `Sectioned` freezes today's section-map membership per ROW — every steel/cmu row true, timber `!CrossPly`, panel true only for structural deck rows, everything else false — so `graph.SectionOf` `Some`/`None` results are value-identical this campaign; widening membership is a flagged next-campaign column flip.
- Entry: `ComponentCatalogue.Of(context)` — the ten-way and four-way `.Concat` folds are DELETED into the `ComponentFamily.Items` fold; `Traverse` is the rail (a seed row `Component.Of` rejects, a `Sectioned` row `SectionSolver.Solve` cannot solve, or a cross-seed `ComponentId` collision, ABORTS the build TYPED — never a `Choose`/`ToOption` swallow and never the raw frozen-map duplicate-key throw); `Lookup` preserved. The composition root binds `Of`'s `Fin` ONCE and passes `catalogue.Rows`/`catalogue.Sections` into `ComponentResolution.Build`.

```csharp signature
// --- [TABLES] ------------------------------------------------------------------------------
// The Materials catalogue row: the standardized-type item plus the sections-map membership pin.
public readonly record struct ComponentRow(Component Item, bool Sectioned);

// The catalogue, de-lockstepped and fail-loud: ONE Items fold computes BOTH frozen maps in one pass. ComponentId's
// generated ordinal value-equality keys the frozen dictionaries — no explicit comparer is threaded. A cross-seed
// designation collision faults TYPED before the frozen build — the raw ToFrozenDictionary duplicate-key throw never
// escapes the rail (two seed pages minting one ComponentId is a seed-data defect, not a composition-root crash).
public sealed record ComponentCatalogue(
    FrozenDictionary<ComponentId, Component> Rows,
    FrozenDictionary<ComponentId, ComputedSection> Sections) {

    public static Fin<ComponentCatalogue> Of(Context context) =>
        ComponentFamily.Items.ToSeq()
            .Traverse(family => family.Rows(context)).As()
            .Map(static perFamily => perFamily.Bind(static rows => rows))
            .Bind(rows =>
                from keyed in guard(rows.Map(static r => r.Item.Designation).Distinct().Count == rows.Count,
                    ComponentFault.Family(context.Key, "<duplicate-designation-across-seeds>"))
                from catalogue in rows.Filter(static r => r.Sectioned)
                    .Traverse(r => SectionSolver.Solve(r.Item.Profile, Op.Of(name: r.Item.Designation.Value))
                        .Map(section => (r.Item.Designation, Section: section))).As()
                    .Map(solved => new ComponentCatalogue(
                        rows.ToFrozenDictionary(static r => r.Item.Designation, static r => r.Item),
                        solved.ToFrozenDictionary(static s => s.Designation, static s => s.Section)))
                select catalogue);

    public static Fin<Component> Lookup(FrozenDictionary<ComponentId, Component> rows, ComponentId id, Op key) =>
        rows.TryGetValue(id, out Component? row) && row is { } r ? Fin.Succ(r) : ComponentFault.Family(key, $"<unregistered-component:{id.Value}>");
}
```

## [07]-[QUANTITY_ROW]

The ONE bounded typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose — the per-page `Len`/`Area`/`Modulus`/`Inertia`/`Torsion`/`Warping` local statics and the `(QuantityType, Dimension, unit)` triples are the DELETED forms. Every `QuantityType` spelling, `Dimension` vector, and SI scale below is the current on-disk mint verbatim, so `MeasureValue` content keys are byte-identical (`[FROZEN_INVARIANTS]`). BOUNDARY: detail-bag rows keep the DIMENSION-ONLY `MeasureValue.OfSi(dim, si)` overload so an authored and an imported bag content-key identically — `QuantityRow` owns TYPED mints only.

```csharp signature
// --- [TABLES] ------------------------------------------------------------------------------
// The one typed-mint table: row key = the seam quantity name; Type/Dim/Scale/Unit the frozen mint columns. A new
// typed quantity is one row — never a page-local OfSi re-spelling.
[SmartEnum<string>]
public sealed partial class QuantityRow {
    public static readonly QuantityRow Length                  = new("Length",                  QuantityType.Length,                            Dimension.LengthDim,                     1e-3,  "m");
    public static readonly QuantityRow Area                    = new("Area",                    QuantityType.Area,                              Dimension.AreaDim,                       1e-6,  "m2");
    public static readonly QuantityRow SectionModulus          = new("SectionModulus",          QuantityType.Create("SectionModulus"),          Dimension.VolumeDim,                     1e-9,  "m3");
    public static readonly QuantityRow SecondMomentOfArea      = new("SecondMomentOfArea",      QuantityType.Create("SecondMomentOfArea"),      Dimension.Create(4, 0, 0, 0, 0, 0, 0),   1e-12, "m4");
    public static readonly QuantityRow TorsionConstant         = new("TorsionConstant",         QuantityType.Create("TorsionConstant"),         Dimension.Create(4, 0, 0, 0, 0, 0, 0),   1e-12, "m4");
    public static readonly QuantityRow WarpingConstant         = new("WarpingConstant",         QuantityType.Create("WarpingConstant"),         Dimension.Create(6, 0, 0, 0, 0, 0, 0),   1e-18, "m6");
    public static readonly QuantityRow Density                 = new("Density",                 QuantityType.Create("Density"),                 Dimension.DensityDim,                    1.0,   "kg/m3");
    public static readonly QuantityRow Pressure                = new("Pressure",                QuantityType.Create("Pressure"),                Dimension.PressureDim,                   1.0,   "Pa");
    public static readonly QuantityRow ThermalConductivity     = new("ThermalConductivity",     QuantityType.Create("ThermalConductivity"),     Dimension.Create(1, 1, -3, 0, -1, 0, 0), 1.0,   "W/(m.K)");
    public static readonly QuantityRow SpecificEntropy         = new("SpecificEntropy",         QuantityType.Create("SpecificEntropy"),         Dimension.Create(2, 0, -2, 0, -1, 0, 0), 1.0,   "J/(kg.K)");
    public static readonly QuantityRow HeatTransferCoefficient = new("HeatTransferCoefficient", QuantityType.Create("HeatTransferCoefficient"), Dimension.ThermalTransmittanceDim,       1.0,   "W/(m2.K)");

    public QuantityType Type { get; }
    public Dimension Dim { get; }
    public double Scale { get; }
    public string Unit { get; }
    public MeasureValue OfSi(double value) => MeasureValue.OfSi(Type, Dim, value * Scale);
}
```

## [08]-[COMPONENT_DETAIL]

The relocated seed-time bag constructors — the `Projection/component.md` `Detail(Component)` switch is DELETED; each `Realization`/`Product`-lane seed page builds its family's bag AT SEED TIME from the same values the switch read (`MasonryDetail.Of`, `GlazingDetail.Of`, `FastenerDetail.Of`, `PanelDetail.Of` and siblings compose this owner), so the bag rows, the `PropertyBag` content, and the projected `Node.PropertySet` bytes are identical. `ProjectType`'s read becomes `c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))`. The `Measured` SI value carries the DIMENSION-only mint (the overload Bim uses) so an authored and an imported row content-key identically; `Joint` routes the `PropertyValue.Enumerated` through the schema's CLOSED allowed set, never a local re-spelling.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The bag-row constructors composing the seam DetailSchema conforming bags (neutral SetName + precedence pinned by the
// schema); Rows folds the tuples into the selected schema's Bag() through ValueBag.With (last-write-wins). Bodies are
// the relocated Projection constructors verbatim, now public — every seed page composes them.
public static class ComponentDetail {
    public static (PropertyName, PropertyValue) Joint(string kind) => (DetailSchema.JointType, DetailSchema.Realization.Joint(kind));
    public static (PropertyName, PropertyValue) Token(PropertyName name, string value) => (name, new PropertyValue.Text(value));
    public static (PropertyName, PropertyValue) Measured(PropertyName name, Dimension dim, double si) => (name, new PropertyValue.Measure(MeasureValue.OfSi(dim, si)));

    public static PropertyBag RealizationRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        rows.ToSeq().Fold(DetailSchema.Realization.Bag(), static (bag, r) => bag.With(r.Name, r.Value));

    public static PropertyBag ProductRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        rows.ToSeq().Fold(DetailSchema.Product.Bag(), static (bag, r) => bag.With(r.Name, r.Value));
}
```

## [09]-[COMPONENT_RESOLUTION]

- Owner: `ResolvedComponent` the one-hop `(Component, Option<ComputedSection>)` receipt; `ComponentResolution` the seam-`ProfileRef` resolver and frozen cache — BYTE-IDENTICAL (`[FROZEN_INVARIANTS]`): only catalogue CONSTRUCTION went `Fin` (`ComponentCatalogue.Of`), bound once at composition; `graph.SectionOf` and every downstream signature are untouched.
- Cases: a `ProfileRef` keys exactly one `ResolvedComponent`; a component present in BOTH maps joins `Some(section)`, one present only in the row map joins `None` — the seam-honest absence (a `PositiveMagnitude` rejects zero, so a forged all-zero `ComputedSection` is unrepresentable), total over every registered ref. The `Option` mirrors the seam `MaterialComposition.ProfileSet(Material, Profile, Option<SectionProperties>)` the projector bakes.
- Entry: `Build(rows, sections)` the O(1)-total build-time JOIN (no fault rail, no `Op` — every row resolves; the integral ran once at catalogue build); `Resolve(reference, table, key)` the one-hop dereference the `capacity#SECTION_CAPACITY` and `Rasm.Compute` structural routes call, `Fin<T>` aborting on an unregistered ref (`ComponentFault.Family` — distinct from the `Section` integral failure that already aborted the build).
- Boundary: the resolver owns NO section math and NO seam type — the section is DATA captured at the catalogue-build site that owns the geometry, never a `Func<Component, Op, Fin<ComputedSection>>` re-invoked at resolution (the deleted phantom). `ProfileRef` stays seam-canonical.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The one-hop resolution receipt (M7): a ProfileRef dereferences to its Component AND its OPTIONAL computed section in
// one lookup. Section is Some for a Sectioned row, None for the rest — never a forged zero section.
public readonly record struct ResolvedComponent(Component Component, Option<ComputedSection> Section);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentResolution {
    // Pre-resolves every catalogued component ONCE, keyed by the SEAM ProfileRef; the frozen table is the M7 one-hop —
    // resolution is an O(1) lookup, the section a build-time JOIN of the two frozen catalogue maps.
    public static FrozenDictionary<ProfileRef, ResolvedComponent> Build(FrozenDictionary<ComponentId, Component> rows, FrozenDictionary<ComponentId, ComputedSection> sections) =>
        rows.ToFrozenDictionary(
            static kv => ProfileRef.Of(kv.Key.Value),
            kv => new ResolvedComponent(kv.Value, sections.TryGetValue(kv.Key, out ComputedSection s) ? Some(s) : Option<ComputedSection>.None));

    public static Fin<ResolvedComponent> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedComponent> table, Op key) =>
        table.TryGetValue(reference, out ResolvedComponent resolved)
            ? Fin.Succ(resolved)
            : ComponentFault.Family(key, $"<unresolved-component-ref:{reference.Value}>");
}
```

## [10]-[RESEARCH]

- [SECTION_PROFILE_ALGEBRA]: REALIZED — `ComponentSection` and its ten bespoke payload records are DELETED with zero column loss: geometry landed as `SectionProfile` named `PositiveMagnitude` dims (gross facts base-constructor state), vocabulary stayed on the seed-page SmartEnums, realization columns ride seed rows and seed-built bags. The five lockstep switches and the cross-file `Detail` switch are field reads, family delegates, or type law. Each arm's railed `Of` factory routes `key.AcceptValidated<PositiveMagnitude>` (R8 spelling) and guards the non-magnitude slots inline — `NonNegativeMagnitude` verified ABSENT from `Rasm.Vectors`, so the fillet/edge slots carry the inline `Edge` guard (R8 resolved by verification).
- [TYPED_CONTRACT_CURVES]: REALIZED — `Curves` lowers sixteen arms onto the decompile-verified `VividOrange.IProfiles` contracts (`IRectangle`/`IRectangularHollow`/`IRoundedRectangle`/`IRoundedRectangularHollow`/`ICircle`/`ICircularHollow`/`IEllipse`/`IIParallelFlange`/`ICustomI`/`IChannel`/`IC`/`ITee`/`IAngle`/`IZ`/`ITrapezoid`) so `ProfileParts.GetParts` integrates fillets and curved edges via EXACT `EllipseQuarterPart`s; `IRoundedRectangularHollow` verified NOT to extend `IRectangularHollow`, so the rounded record cannot fall to the sharp arm. The `IPerimeter` polyline path (short-circuited first by every `.Utility` kernel, verified) carries `CellularRectangle`/`Corrugated`/`FilletTriangle`/`Outline`. The VividOrange unknown-profile throw is unreachable by construction — every `Curves` output is a dispatch member.
- [STEEL_ALGEBRA_RELOCATION]: REALIZED — the `steel#STEEL_FAMILY` `SteelStiffness` open-thin-walled algebra lands in `Forms` (`OpenI`/`OpenChannel`/`MonoTee`/`OpenAngle` cores verbatim; `MonoI`/`OpenCee`/`PointSymmetricZ` the widened singly-symmetric/point-symmetric arms; `FromCatalogue` keys the same cores by `SteelClass` over the admitted `SectionDims`), the singly-symmetric factors carried as policy values over the carrier-free `.Utility` kernel moduli (`SectionModuli.CalculateSectionModulusYy/Zz` — the direct static path). R4 stands: if VividOrange's typed-part decomposition natively fills cold-formed C/Z shear-centre/warping, `OpenCee`/`PointSymmetricZ` re-class to VENDOR columns; until then the relocated algebra fills them.
- [CATALOGUE_ONE_PASS]: REALIZED — `ComponentCatalogue.Of` folds `ComponentFamily.Items` through `Traverse` (fail-loud: a malformed seed row, an unsolvable `Sectioned` row, or a cross-seed designation collision aborts the build typed — the frozen-map duplicate-key throw never escapes the rail); the ten-way/four-way `.Concat` folds and the per-family `ComponentCatalogue` static classes are DELETED. `ComponentResolution` byte-identical; `Sectioned` pins today's `graph.SectionOf` membership.
- [QUANTITY_AND_DETAIL_OWNERS]: REALIZED — `QuantityRow` is the one typed-mint table (`SeamSection` locals and Properties triples deleted at their pages); `ComponentDetail` the relocated public bag constructors every `Realization`/`Product` seed composes at seed time. Both live HERE because Projection and Properties both compose them and this owner is their shared parent.
- [RESEARCH_REGISTER]: R1 (masonry/CMU/timber/fastener/panel/glazing/EPD producer watch) and R4 (VividOrange cold-formed columns) carried; R3 and R8 discharged above. Seed-page namespace split (masonry/cmu/steel/panel/reinforcement in child leaves; timber/glazing/fastener/connector/joint in the parent) is bridged by the prelude aliases — unification is a reconcile-stage sweep, not a this-page decision.
