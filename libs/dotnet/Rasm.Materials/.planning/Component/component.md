# [MATERIALS_COMPONENT]

THE POLYMORPHIC COMPONENT OWNER. One `Component` record is the canonical standardized-TYPE concept every steel/timber/concrete/aluminum member, masonry/CMU/IGU unit, sheet-goods board, precast product, covering/insulation product, pipe/duct/conductor segment, and rebar/fastener/connector/joint part parameterizes: a closed `SectionProfile` cross-section algebra (a FIELD, never a peer union of per-family payloads), an `IfcBinding` row stamp, a `Coring` void class, a `ComponentStandard` regional row, two independent `MaterialId` slots, and an `Option<PropertyBag>` detail whose presence is a TYPE LAW of the family's `DetailLane`. One `ComponentFamily` `[SmartEnum<string>]` is THE policy row: each of the nineteen rows {masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint · panel · concrete · precast · aluminum · insulation · finish · fireproofing · pipework · ductwork · electrical} carries its `ComponentClass`, its `DetailLane`, its profile-admission predicate, its cross-nominal selector, its symmetry projection, and its `<Family>Seed.Rows` fold — so a NEW FAMILY is one row and one seed page with zero central edits, and a NEW PART is one seed row with zero type edits. Every per-family discriminant is a stored column, a base-constructor fact, a family delegate, or a field read — the family axis carries its own class, lane, IFC leaf, admission predicate, cross-nominal selector, symmetry projection, seed fold, and capacity producer, so nothing about a family is decided by dispatch anywhere else.

This page also owns the ONE section-computation spine: `SectionSolver.Solve` is the single exhaustive generated `Switch` over the closed `SectionProfile` axis — a new arm is compiler-forced HERE and nowhere else. Every solvable arm routes the ONE `VividOrange.Sections.SectionProperties` Green's-theorem integral through the ONE twenty-column `Admit` lift, then a topology supplement: `Curves` lowers each arm onto the TYPED `VividOrange.IProfiles` floor contract (`IRectangle`/`IRoundedRectangle`/`IRectangularHollow`/`IRoundedRectangularHollow`/`ICircle`/`ICircularHollow`/`IEllipse`/`IIParallelFlange`/`ICustomI`/`IChannel`/`IC`/`ITee`/`IAngle`/`IZ`) or an exact `Perimeter`; the typed contracts let the solver decompose fillets and curved edges through `TrapezoidalPart` + `EllipseQuarterPart`, while the `Perimeter` path preserves multi-void, corrugated, offset-trapezium, triangle, and free-outline geometry. `Forms` supplies the plastic/torsion/warping/shear/asymmetry supplement through TWO kernels: `ThinWalled` sweeps each open arm's own midline for its shear centre, warping constant, and Saint-Venant torsion, and `Plastic` integrates the exact equal-area-axis first moment over each arm's strip stack — so a shape factor against an elastic modulus and a per-shape warping transcription are both unrepresentable. `ComputedSection` stays the FROZEN twenty-column record (`[FROZEN_INVARIANTS]`), `ComponentCatalogue.Of` the ONE fail-loud `Traverse` fold computing BOTH frozen maps in one pass beside `AdmitImported`, the reverse-direction fold lowering ingested IFC type candidates onto the same result-typed `Component.Of` construction, `ComponentResolution` the byte-identical M7 one-hop, `QuantityRow` the one typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose, and `ComponentDetail` the seed-time bag constructors every `Realization`/`Product`-lane seed composes. The page composes the `Rasm.Numerics` kernel `PositiveMagnitude`, the `Rasm.Domain` `Op`/`Context`/`Fault`/`AcceptValidated` result, the `Rasm.Element` contract (`MaterialId`, `ProfileRef`, `FaultBand`, the `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`DetailSchema` property vocabulary), and the VividOrange profile/section floor. `ProfileRef`/`ProfileSet`/`SectionProperties`/`ComputedSection` stay contract-canonical — the rename STOPS at the Materials boundary. This page also owns the folder's TWO seed spines: `MaterialGrade` over the closed `GradeProperties` payload is the ONE registered-grade identity (the six per-family grade owners collapse to rows here, one typed `ComponentAuthority` axis), and `SeedLaw<TRow>`/`ComponentSeed.Rows` the ONE accumulating seed traverse every `ComponentFamily.rows:` delegate binds — a family page carries vocabulary, rows, and its law value alone.

## [01]-[INDEX]

- [02]-[COMPONENT_OWNER]: `ComponentId`, `ComponentClass`, `DetailLane`, `CoringClass`/`Coring`, `ComponentAuthority`, `IfcBinding`, the `ComponentFamily` policy row, `ComponentSymmetry` the construction-row projection onto the kernel `MaterialSymmetry`, the band-2300 `ComponentFault`, `ComponentUnit`, `ComponentStandard`, and the `Component` record with its ONE polymorphic `Of` admission and its `Symmetry` derivation.
- [03]-[SECTION_PROFILE]: `VoidCell`, the bounded `PlyRole` vocabulary, `Ply`, and the closed `SectionProfile` `[Union]` — named `PositiveMagnitude` dimensions, gross bounding facts as base-constructor state, and one result-typed `Of` factory per arm.
- [04]-[SECTION_SOLVER]: `SectionSupplement`, the `ProfileGeometry` row and the ONE `SectionGeometry.Of` dispatch behind `SectionSolver.Solve`/`ProfileOf`, the twenty-column `Admit` lift, the `Curves` typed-contract table, the `Forms` supplement table, and the `ThinWalled` sectorial and `Plastic` strip kernels every arm composes.
- [05]-[COMPUTED_SECTION]: the FROZEN twenty-field `ComputedSection` + `GoverningRadiusMm`.
- [06]-[CATALOGUE]: `SeedJoin`, `ComponentRow`, `ComponentCatalogue.Of`/`Lookup`/`AdmitImported` — the result-typed seed-table join, the fail-loud one-pass fold over `ComponentFamily.Items`, and the imported-candidate admission fold beside it.
- [07]-[QUANTITY_ROW]: the one typed-mint `[SmartEnum]` the contract lift and the property ingress compose.
- [08]-[COMPONENT_DETAIL]: the seed-time bag constructors (`Joint`/`Sourced`/`Token`/`Measured`/`RealizationRows`/`ProductRows`).
- [09]-[COMPONENT_RESOLUTION]: `ResolvedComponent` + the frozen `Build`/`Resolve` M7 cache, byte-identical.
- [10]-[MATERIAL_GRADE]: `GradeProperties` the closed per-family payload `[Union]` and `MaterialGrade` the ONE grade identity — every family's registered grade is a row here, its physics an arm member on its family page.
- [11]-[COMPONENT_SEED]: `SeedLaw<TRow>` the per-family policy value and `ComponentSeed.Rows` the ONE accumulating seed traverse every family's `rows:` delegate binds.

## [02]-[COMPONENT_OWNER]

- Owner: `Component` over the nineteen-row `ComponentFamily` policy axis; `ComponentId` the `family.designation` key; `ComponentFault` on the `FaultBand.Component` allocation; `IfcBinding` the Materials-side IFC pair as ROW DATA (was two 10-arm switches); `DetailLane` the bag-presence law; `Coring`/`CoringClass` the cross-family void class; `ComponentUnit`/`ComponentStandard` the shared dimensional and regional rows; `ComponentSymmetry` the construction-row projection onto the kernel `MaterialSymmetry` — the ONE symmetry authority for grain, nap, and face direction, bound per family as the `Symmetry` policy column.
- Cases: one `Component` shape across all nineteen families — `Family` (the policy row), `Designation`, `Profile` (the `SectionProfile` arm), `Ifc` (the stored `IfcBinding` row), `Coring`, `Standard`, `SubstanceId`/`AppearanceId` (independent `MaterialId` slots — a coated rebar keeps capacity steel and epoxy appearance distinct), `Detail` (`Option<PropertyBag>`, `Some` iff the family lane is not `None`). Every per-family discriminant is a field read (`IfcEntity`/`PredefinedToken`), a family delegate (`CrossNominalMm`, `Symmetry`), or base-constructor state (`GrossRectangleMm`).
- Entry: `Component.Of(family, designation, profile, ifc, coring, standard, substanceId, appearanceId, detail, key)` guards the exact `family.designation` grammar, family-prefix ownership, family/profile admission, coring, and lane/detail consistency. `SectionProfile.Layered.Of` owns ply-stack closure because every admitted layered value must be valid before a `Component` exists.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`Fault`/`AcceptValidated`), Rasm (project — `MaterialSymmetry`/`RotationOrder`/`MirrorGrant`, the `Rasm/Parametric/patternmap#PATTERNING` placement-legality law the `Symmetry` derivation projects onto), Rasm.Element (project — `MaterialId`, `ProfileRef`, `FaultBand`, `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`PropertyName`/`PropertyValue`/`DetailSchema`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`[UseDelegateFromConstructor]`, generated exhaustive `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`; `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`guard` — results-and-effects doctrine substrate), VividOrange.IProfiles + VividOrange.Profiles.Perimeter + VividOrange.Geometry + VividOrange.Sections.SectionProperties + UnitsNet (`Rasm.Materials/.api` catalogues), BCL inbox (`FrozenDictionary`).
- Growth: a new part is one seed row; a new family is one `ComponentFamily` row and one seed page (no union edit, no switch edit, no `.Concat` edit); a new section shape is one `SectionProfile` arm and one compiler-forced `Solve` arm (buildingSMART profile-schema cadence, never thing cadence); a new fault is one `ComponentFault` case; a new structural column is one `ComputedSection` field every solvable arm fills; a new detail row is one `DetailSchema`-named tuple in a seed bag; a new typed mint is one `QuantityRow` row; a new evidence tier is an Element `EvidenceGrade` row (contract-owned, wire tokens append-only), never a local provenance roster; a new grade is one `MaterialGrade` row plus its `GradeProperties` arm. Three admissions widen by declared trigger, never speculatively: the reinforcement predicate grows `CircleHollow` when a certified tendon-duct diameter lands (the `reinforcement#REINFORCEMENT_FAMILY` Growth clause seeds the annular `IfcTendonConduit` row through the same generator), the precast predicate grows `Layered` when a dimensioned sandwich-wythe build two-sources over the `PlyRole.ConcreteWythe` row (its symmetry already rides `ComponentSymmetry.Stack`, so the wythe stack derives with zero symmetry edits), and the electrical predicate grows `CircleHollow` when the NEC Chapter 9 Table 4 OD/ID ladder proves (the `electrical#CONTAINMENT` conduit systems then mint through their own fold beside the conductor folds — without this widening the declared containment growth would fault at family admission). The symmetry projection widens by declared trigger the same way: `MirrorGrant.Matched` lands when a book-match veneer pairing fact two-sources onto a construction row — the pair obligation then derives from the material fact alone, never a caller flag — and the nap vector (`RotationOrder.Identity`: a pile lies one way, so a half-turn shows) lands when a napped covering build authors an oriented face row.
- Boundary: `ComponentFault` derives `Fault`; generated identity binds each direct leaf to `FaultBand.Component`. Documented VividOrange grade, section, capacity-solve, and capacity-decode refusals use cause-bearing semantic leaves; unknown provider throws stay exact `Exceptional` errors. `IfcBinding` strings stay NEUTRAL here; the generated `Rasm.Bim` roster is the validation authority (composition-time `IfcLegality`, egress-time `AdmitPredefined`, design-time emitter stamp audit) — Materials never references `Rasm.Bim`. Every seed page declares the ONE flat `Rasm.Materials.Component` namespace — the `Component/` folder is one namespace under `dotnet_style_namespace_match_folder = true:error` — so the policy rows bind `<Family>Seed.Rows` by bare name with no alias. `Component.Symmetry` is the ONE symmetry authority: the kernel `PatternPlan.Symmetry`/`PanelPolicy.Symmetry` admissions and the `Rasm.Fabrication` nesting move gate read this projection, never a re-derivation from appearance or geometry — a consumer-side legality knob beside a material that already models direction is the named re-mint. The derivation under-grants and never over-grants (a directional read admits a subset of the isotropic placements), so a missing construction fact costs placements, never legality; masonry stays isotropic by the same boundary — `BondGeometry`'s wallpaper group is ASSEMBLY symmetry, how units tile, while this law is the unit substance's own, and the two never merge.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Parametric;
using Thinktecture;
using UnitsNet;
using VividOrange.Geometry;
using VividOrange.Profiles;
using VividOrange.Sections.SectionProperties.Utility;
using Dimension = Rasm.Element.Properties.Dimension;
using SectionProperties = VividOrange.Sections.SectionProperties.SectionProperties;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ComponentId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("Component ids require the family.designation form.");
            return;
        }

        int separator = value.IndexOf('.');
        validationError = separator <= 0 || separator != value.LastIndexOf('.') || separator == value.Length - 1
            ? new ValidationError("Component ids require the family.designation form.")
            : null;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentClass {
    public static readonly ComponentClass Primary = new("primary");
    public static readonly ComponentClass Panel   = new("panel");
    public static readonly ComponentClass Minor   = new("minor");
}

public enum DetailLane : byte { None = 0, Realization = 1, Product = 2 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid      = new("solid");
    public static readonly CoringClass Frogged    = new("frogged");
    public static readonly CoringClass Cored      = new("cored");
    public static readonly CoringClass Cellular   = new("cellular");
    public static readonly CoringClass Perforated = new("perforated");
    public static readonly CoringClass Hollow     = new("hollow");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coring {
    public static readonly Coring None             = new("none",               classification: CoringClass.Solid);
    public static readonly Coring Frog             = new("frog",               classification: CoringClass.Frogged);
    public static readonly Coring Cored3Hole       = new("cored-3-hole",       classification: CoringClass.Cored);
    public static readonly Coring Cellular         = new("cellular",           classification: CoringClass.Cellular);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell", classification: CoringClass.Perforated);
    public static readonly Coring Hollow3Cell      = new("hollow-3-cell",      classification: CoringClass.Hollow);
    public static readonly Coring Hollow2Cell      = new("hollow-2-cell",      classification: CoringClass.Hollow);
    public CoringClass Classification { get; }
}


[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentAuthority {
    public static readonly ComponentAuthority Astm = new("ASTM", region: "us",  jacking: Some((Ultimate: 0.80, Proof: 0.94)));
    public static readonly ComponentAuthority Aisc = new("AISC", region: "us");
    public static readonly ComponentAuthority Aisi = new("AISI", region: "us");
    public static readonly ComponentAuthority Aws  = new("AWS",  region: "us");
    public static readonly ComponentAuthority Awc  = new("AWC",  region: "us");
    public static readonly ComponentAuthority Sae  = new("SAE",  region: "us");
    public static readonly ComponentAuthority IccEs = new("ICC-ES", region: "us");
    public static readonly ComponentAuthority En   = new("EN",   region: "eu",  jacking: Some((Ultimate: 0.80, Proof: 0.90)));
    public static readonly ComponentAuthority Bs   = new("BS",   region: "uk");
    public static readonly ComponentAuthority Din  = new("DIN",  region: "din");
    public static readonly ComponentAuthority As   = new("AS",   region: "au");
    public static readonly ComponentAuthority Is   = new("IS",   region: "is");
    public static readonly ComponentAuthority Apa  = new("APA",  region: "us");
    public static readonly ComponentAuthority Csa  = new("CSA",  region: "ca");
    public static readonly ComponentAuthority Sdi  = new("SDI",  region: "us");
    public static readonly ComponentAuthority Smacna = new("SMACNA", region: "us");
    public static readonly ComponentAuthority Nfpa = new("NFPA", region: "us");
    public static readonly ComponentAuthority Iec  = new("IEC",  region: "int");
    public string Region { get; }
    public Option<(double Ultimate, double Proof)> Jacking { get; }

    public Option<double> JackingCeilingMpa(double ultimateMpa, double proofMpa) =>
        Jacking.Map(row => Math.Min(row.Ultimate * ultimateMpa, row.Proof * proofMpa));
}

public readonly record struct IfcBinding(string Entity, string Predefined, Option<string> ObjectType) {
    public static IfcBinding Of(string entity, string predefined) => new(entity, predefined, None);
    public static IfcBinding Named(string entity, string objectType) => new(entity, "USERDEFINED", Some(objectType));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentFamily {
    public static readonly ComponentFamily Masonry       = new("masonry",       ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcBuildingElementPart", "MasonryUnit"),          admits: static p => p is SectionProfile.Rectangle or SectionProfile.CellularRectangle,                                    crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, MasonrySeed.Roster, MasonrySeed.Law), capacity: MasonrySeed.Capacity);
    public static readonly ComponentFamily Cmu           = new("cmu",           ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcBuildingElementPart", "ConcreteMasonryUnit"),  admits: static p => p is SectionProfile.CellularRectangle,                                                                crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, CmuSeed.Roster, CmuSeed.Law), capacity: CmuSeed.Capacity);
    public static readonly ComponentFamily Steel         = new("steel",         ComponentClass.Primary, DetailLane.Realization, IfcBinding.Named("IfcBuildingElementProxy", "SteelSection"),        admits: static p => p is SectionProfile.Catalogued or SectionProfile.RectangleHollow or SectionProfile.CircleHollow or SectionProfile.ColdFormedC or SectionProfile.BuiltUp or SectionProfile.IShape or SectionProfile.AsymmetricIShape or SectionProfile.Channel or SectionProfile.Tee or SectionProfile.Angle or SectionProfile.Zed or SectionProfile.RoundedRectangle, crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, SteelSeed.Roster, SteelSeed.Law), capacity: SteelSeed.Capacity);
    public static readonly ComponentFamily Timber        = new("timber",        ComponentClass.Primary, DetailLane.None,        IfcBinding.Named("IfcBuildingElementProxy", "TimberSection"),       admits: static p => p is SectionProfile.Rectangle or SectionProfile.RoundedRectangle || p is SectionProfile.Layered l && l.Plies.ForAll(static ply => ply.Role == PlyRole.Longitudinal || ply.Role == PlyRole.Transverse), crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Grain,     rows: static context => ComponentSeed.Rows(context, TimberSeed.Roster, TimberSeed.Law), capacity: TimberSeed.Capacity);
    public static readonly ComponentFamily Glazing       = new("glazing",       ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcPlate", "SHEET"),                                 admits: static p => p is SectionProfile.Layered l && l.Plies.ForAll(static ply => ply.Role == PlyRole.Pane || ply.Role == PlyRole.Interlayer || ply.Role == PlyRole.Cavity),               crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Stack,     rows: static context => ComponentSeed.Rows(context, GlazingSeed.Roster, GlazingSeed.Law), capacity: GlazingSeed.Capacity);
    public static readonly ComponentFamily Reinforcement = new("reinforcement", ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Of("IfcReinforcingBar", "NOTDEFINED"),                   admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, ReinforcementSeed.Roster, ReinforcementSeed.Law), capacity: ReinforcementSeed.Capacity);
    public static readonly ComponentFamily Fastener      = new("fastener",      ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Of("IfcMechanicalFastener", "BOLT"),                     admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, FastenerSeed.Roster, FastenerSeed.Law), capacity: FastenerSeed.Capacity);
    public static readonly ComponentFamily Connector     = new("connector",     ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcDiscreteAccessory", "Connector"),              admits: static p => p is SectionProfile.Rectangle or SectionProfile.Outline,                                              crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, ConnectorSeed.Roster, ConnectorSeed.Law), capacity: ConnectorSeed.Capacity);
    public static readonly ComponentFamily Joint         = new("joint",         ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcFastener", "Joint"),                           admits: static p => p is SectionProfile.FilletTriangle or SectionProfile.Trapezium or SectionProfile.Circle or SectionProfile.Nominal, crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, JointSeed.Roster, JointSeed.Law), capacity: JointSeed.Capacity);
    public static readonly ComponentFamily Panel         = new("panel",         ComponentClass.Panel,   DetailLane.Product,     IfcBinding.Of("IfcPlate", "NOTDEFINED"),                            admits: static p => p is SectionProfile.Layered or SectionProfile.Corrugated,                                             crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Stack,     rows: static context => ComponentSeed.Rows(context, PanelSeed.Roster, PanelSeed.Law), capacity: PanelSeed.Capacity);
    public static readonly ComponentFamily Concrete      = new("concrete",      ComponentClass.Primary, DetailLane.Realization, IfcBinding.Named("IfcBuildingElementProxy", "ConcreteSection"),     admits: static p => p is SectionProfile.Rectangle or SectionProfile.Circle,                                               crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, ConcreteSeed.Roster, ConcreteSeed.Law), capacity: ConcreteSeed.Capacity);
    public static readonly ComponentFamily Precast       = new("precast",       ComponentClass.Primary, DetailLane.Product,     IfcBinding.Named("IfcBuildingElementProxy", "PrecastSection"),      admits: static p => p is SectionProfile.Rectangle or SectionProfile.CellularRectangle,                                    crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Stack,     rows: static context => ComponentSeed.Rows(context, PrecastSeed.Roster, PrecastSeed.Law), capacity: PrecastSeed.Capacity);
    public static readonly ComponentFamily Aluminum      = new("aluminum",      ComponentClass.Primary, DetailLane.Product,     IfcBinding.Of("IfcMember", "MULLION"),                              admits: static p => p is SectionProfile.RectangleHollow or SectionProfile.CircleHollow or SectionProfile.IShape or SectionProfile.Channel or SectionProfile.Tee or SectionProfile.Angle or SectionProfile.ColdFormedC or SectionProfile.Rectangle or SectionProfile.Outline, crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, AluminumSeed.Roster, AluminumSeed.Law), capacity: AluminumSeed.Capacity);
    public static readonly ComponentFamily Insulation    = new("insulation",    ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcCovering", "INSULATION"),                         admits: static p => p is SectionProfile.Rectangle or SectionProfile.Nominal,                                              crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, InsulationSeed.Roster, InsulationSeed.Law), capacity: InsulationSeed.Capacity);
    public static readonly ComponentFamily Finish        = new("finish",        ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcCovering", "FLOORING"),                           admits: static p => p is SectionProfile.Rectangle or SectionProfile.Nominal,                                              crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, FinishSeed.Roster, FinishSeed.Law), capacity: FinishSeed.Capacity);
    public static readonly ComponentFamily Fireproofing  = new("fireproofing",  ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcCovering", "Fireproofing"),                    admits: static p => p is SectionProfile.Nominal,                                                                          crossNominal: static p => p.GrossRectangleMm.DepthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, FireproofingSeed.Roster, FireproofingSeed.Law), capacity: FireproofingSeed.Capacity);
    public static readonly ComponentFamily Pipework      = new("pipework",      ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcPipeSegment", "RIGIDSEGMENT"),                    admits: static p => p is SectionProfile.CircleHollow,                                                                     crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, PipeworkSeed.Roster, PipeworkSeed.Law), capacity: PipeworkSeed.Capacity);
    public static readonly ComponentFamily Ductwork      = new("ductwork",      ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcDuctSegment", "RIGIDSEGMENT"),                    admits: static p => p is SectionProfile.CircleHollow or SectionProfile.RectangleHollow,                                   crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, DuctworkSeed.Roster, DuctworkSeed.Law), capacity: DuctworkSeed.Capacity);
    public static readonly ComponentFamily Electrical    = new("electrical",    ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcCableSegment", "CONDUCTORSEGMENT"),               admits: static p => p is SectionProfile.Circle,                                                                           crossNominal: static p => p.GrossRectangleMm.WidthMm, symmetry: ComponentSymmetry.Isotropic, rows: static context => ComponentSeed.Rows(context, ElectricalSeed.Roster, ElectricalSeed.Law), capacity: ElectricalSeed.Capacity);

    [UseDelegateFromConstructor] public partial bool Admits(SectionProfile profile);
    [UseDelegateFromConstructor] public partial PositiveMagnitude CrossNominal(SectionProfile profile);
    [UseDelegateFromConstructor] public partial MaterialSymmetry Symmetry(SectionProfile profile);
    [UseDelegateFromConstructor] public partial Fin<Seq<ComponentRow>> Rows(Context context);
    [UseDelegateFromConstructor] public partial Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement);
    public ComponentClass Class { get; }
    public DetailLane Lane { get; }

    public IfcBinding Ifc { get; }
}

public static class ComponentSymmetry {
    public static MaterialSymmetry Isotropic(SectionProfile _) => MaterialSymmetry.Free;

    public static MaterialSymmetry Grain(SectionProfile profile) => new(
        RotationOrder.Twofold,
        profile is SectionProfile.Layered layered ? Mirror(layered.Plies) : MirrorGrant.Turned);

    public static MaterialSymmetry Stack(SectionProfile profile) =>
        profile is SectionProfile.Corrugated ? new MaterialSymmetry(RotationOrder.Twofold, MirrorGrant.Reflective)
        : profile is SectionProfile.Layered layered ? new MaterialSymmetry(Rotation(layered.Plies), Mirror(layered.Plies))
        : MaterialSymmetry.Free;

    static RotationOrder Rotation(Seq<Ply> plies) =>
        plies.Exists(static ply => ply.Role.Oriented) ? RotationOrder.Twofold : RotationOrder.Free;

    static MirrorGrant Mirror(Seq<Ply> plies) =>
        !plies.Exists(static ply => ply.Role.Oriented) ? MirrorGrant.Reflective
        : plies == plies.Rev() ? MirrorGrant.Turned
        : MirrorGrant.Refused;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComponentFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Component;
    private ComponentFault(string message) { Key = key; MessageCore = message; }
    private string MessageCore { get; }
    public override string Message => MessageCore;

    [FaultCase(0)] public sealed partial record GradeFamilyMismatch(MaterialGrade Grade, ComponentFamily Expected)
        : ComponentFault(Key, $"Grade {Grade.Key} belongs to {Grade.Family.Key}, not {Expected.Key}.");
    [FaultCase(1)] public sealed partial record ProfileMismatch(ComponentFamily Family, Type Profile)
        : ComponentFault(Key, $"{Profile.Name} is not an admitted {Family.Key} profile.");
    [FaultCase(2)] public sealed partial record DetailMismatch(ComponentFamily Family, DetailLane Lane, bool Present)
        : ComponentFault(Key, $"{Family.Key} detail presence {Present} conflicts with lane {Lane}.");
    [FaultCase(3)] public sealed partial record CatalogueConflict(int Collisions)
        : ComponentFault(Key, $"The component catalogue contains {Collisions} designation collisions.");
    [FaultCase(4)] public sealed partial record ComponentMissing(ProfileRef Reference)
        : ComponentFault(Key, $"Component {Reference.Designation} is not registered.");
    [FaultCase(5)] public sealed partial record DesignationOwnerMismatch(ComponentId Subject, ComponentFamily Family)
        : ComponentFault(Key, $"Component {Subject.Value} is not owned by family {Family.Key}.");
    [FaultCase(6)] public sealed partial record GradeBodyMissing(MaterialGrade Grade, ComponentFamily Family)
        : ComponentFault(Key, $"Grade {Grade.Key} carries no {Family.Key} body.");
    [FaultCase(7)] public sealed partial record GradeBandMissing(ComponentFamily Family, Type Band)
        : ComponentFault(Key, $"The {Family.Key} grade has no published value in {Band.Name}.");
    [FaultCase(8)] public sealed partial record EnvelopeRejected(Type Profile, double WidthMm, double DepthMm)
        : ComponentFault(Key, $"{Profile.Name} members do not fit inside {WidthMm:R} x {DepthMm:R} mm.");
    [FaultCase(9)] public sealed partial record CellLayoutRejected(double WidthMm, double DepthMm, int Cells)
        : ComponentFault(Key, $"{Cells} cells do not form a disjoint layout inside {WidthMm:R} x {DepthMm:R} mm.");
    [FaultCase(10)] public sealed partial record PlyStackRejected(int Plies, double DeclaredMm, double BuiltMm)
        : ComponentFault(Key, $"{Plies} plies build {BuiltMm:R} mm instead of {DeclaredMm:R} mm.");
    [FaultCase(11)] public sealed partial record OutlineRejected(int Vertices, ProfileTopology Topology)
        : ComponentFault(Key, $"A {Topology.Key} outline with {Vertices} vertices is not solvable.");
    [FaultCase(12)] public sealed partial record CompositionRejected(int Members)
        : ComponentFault(Key, $"A built-up section with {Members} members is not solvable.");
    [FaultCase(13)] public sealed partial record SectionUnavailable(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no section for this operation.");
    [FaultCase(14)] public sealed partial record SectionIncoherent(Type Profile)
        : ComponentFault(Key, $"{Profile.Name} produced an incoherent section.");
    [FaultCase(15)] public sealed partial record CoringRejected(double VoidFraction)
        : ComponentFault(Key, $"Void fraction {VoidFraction:R} is outside the physical coring interval.");
    [FaultCase(16)] public sealed partial record BondRejected(Option<int> Course)
        : ComponentFault(Key, Course.Match(Some: course => $"Bond course {course} cannot be realized.", None: static () => "The bond pattern cannot be realized."));
    [FaultCase(17)] public sealed partial record MortarUnavailable(double DeclaredMpa)
        : ComponentFault(Key, $"Mortar strength {DeclaredMpa:R} MPa has no admitted class.");
    [FaultCase(18)] public sealed partial record BasisUnsupported(DesignBasis Basis, ComponentFamily Family)
        : ComponentFault(Key, $"Design basis {Basis.Key} does not govern {Family.Key}.");
    [FaultCase(19)] public sealed partial record CapacityUnavailable(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} publishes no capacity for this demand.");
    [FaultCase(20)] public sealed partial record CoverCellMissing(ExposureToken Exposure, StructuralClass Structural)
        : ComponentFault(Key, $"Exposure {Exposure.Key} publishes no cover for structural class {Structural.Key}.");
    [FaultCase(21)] public sealed partial record FireResistanceExhausted(ComponentFamily Family, double Minutes)
        : ComponentFault(Key, $"{Family.Key} resistance is exhausted after {Minutes:R} minutes.");
    [FaultCase(22)] public sealed partial record FaceLayoutUnsupported(Type Profile)
        : ComponentFault(Key, $"{Profile.Name} does not admit face reinforcement layers.");
    [FaultCase(23)] public sealed partial record GradeDerivation(Error Cause)
        : ComponentFault(Key, "Grade derivation was refused by the provider."), ICausedFault;
    [FaultCase(24)] public sealed partial record SectionConstruction(Error Cause)
        : ComponentFault(Key, "Section construction was refused by the provider."), ICausedFault;
    [FaultCase(25)] public sealed partial record CapacitySolve(Error Cause)
        : ComponentFault(Key, "Capacity solving was refused by the provider."), ICausedFault;
    [FaultCase(26)] public sealed partial record CapacityDecode(Error Cause)
        : ComponentFault(Key, "Capacity decoding was refused by the provider."), ICausedFault;
    [FaultCase(27)] public sealed partial record EffectiveDepthUnavailable(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no effective depth for the requested capacity model.");
    [FaultCase(28)] public sealed partial record TensionChordUnavailable(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no tension chord for the requested capacity model.");
    [FaultCase(29)] public sealed partial record CapacityDocumentEmpty(ComponentId Subject)
        : ComponentFault(Key, $"The decoded capacity document for {Subject.Value} was empty.");
    [FaultCase(30)] public sealed partial record SelectionExhausted(Type Subject)
        : ComponentFault(Key, $"No {Subject.Name} candidate satisfies the admitted demand.");
    [FaultCase(31)] public sealed partial record GradeUnavailable(ComponentFamily Family, MaterialId Substance)
        : ComponentFault(Key, $"No {Family.Key} grade is registered for substance {Substance.Value}.");
    [FaultCase(32)] public sealed partial record ConnectionMissing(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no connection state for the requested capacity model.");
    [FaultCase(33)] public sealed partial record LateralFormatUnsupported(SafetyFormat Format, LateralHazard Hazard)
        : ComponentFault(Key, $"Safety format {Format.Key} does not price {Hazard.Key} lateral resistance.");
    [FaultCase(34)] public sealed partial record FireThicknessMissing(RatingPeriod Period)
        : ComponentFault(Key, $"No fire-equivalent thickness is published for {Period.Key}.");
    [FaultCase(35)] public sealed partial record LateralCellMissing(WspGrade Grade, SheathingNail Nail, double ThicknessIn)
        : ComponentFault(Key, $"No lateral cell exists for {Grade.Key}, {Nail.Key}, and {ThicknessIn:R} in sheathing.");
    [FaultCase(36)] public sealed partial record WaterAbsorptionMissing(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no water-absorption value for the requested masonry basis.");
    [FaultCase(37)] public sealed partial record MortarBandMissing(MortarType Mortar)
        : ComponentFault(Key, $"Mortar {Mortar.Key} has no admitted masonry resistance band.");
    [FaultCase(38)] public sealed partial record FlexuralCellMissing(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no published masonry flexural cell.");
    [FaultCase(39)] public sealed partial record AssemblageStrengthMissing(ComponentId Subject)
        : ComponentFault(Key, $"Component {Subject.Value} has no admitted masonry assemblage strength.");
    [FaultCase(40)] public sealed partial record ServiceClassUnsupported(TimberForm Form, ServiceClass Service)
        : ComponentFault(Key, $"Timber form {Form.Key} is not admitted in service class {Service.Key}.");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ComponentUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    public double LengthOverHeight => LengthMm.Value / HeightMm.Value;
    public static Fin<ComponentUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm) =>
        from w in FactoryBridge.Accept<PositiveMagnitude>(candidate: widthMm)
        from h in FactoryBridge.Accept<PositiveMagnitude>(candidate: heightMm)
        from l in FactoryBridge.Accept<PositiveMagnitude>(candidate: lengthMm)
        from c in FactoryBridge.Accept<PositiveMagnitude>(candidate: courseHeightMm)
        select new ComponentUnit(w, h, l, c);
}

public readonly record struct ComponentStandard(string Region, double StandardJointThicknessMm, ComponentAuthority Authority);

public sealed record Component(
    ComponentFamily Family, ComponentId Designation, SectionProfile Profile, IfcBinding Ifc,
    Coring Coring, ComponentStandard Standard, MaterialId SubstanceId, MaterialId AppearanceId,
    Option<PropertyBag> Detail) {

    public ComponentClass Class => Family.Class;
    public string IfcEntity => Ifc.Entity;
    public string PredefinedToken => Ifc.Predefined;
    public PositiveMagnitude CrossNominalMm => Family.CrossNominal(Profile);
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm => Profile.GrossRectangleMm;

    public MaterialSymmetry Symmetry => Family.Symmetry(Profile);

    public static Fin<Component> Of(
        ComponentFamily family, string designation, SectionProfile profile, IfcBinding ifc,
        Coring coring, ComponentStandard standard, MaterialId substanceId, MaterialId appearanceId,
        Option<PropertyBag> detail) =>
        from id in FactoryBridge.Accept<ComponentId>(designation)
        from owned in guard(id.Value.StartsWith($"{family.Key}.", StringComparison.Ordinal),
            new ComponentFault.DesignationOwnerMismatch(id, family))
        from admitted in guard(family.Admits(profile), new ComponentFault.ProfileMismatch(family, profile.GetType()))
        from laned in guard(detail.IsSome == (family.Lane != DetailLane.None),
            new ComponentFault.DetailMismatch(family, family.Lane, detail.IsSome))
        select new Component(family, id, profile, ifc, coring, standard, substanceId, appearanceId, detail);
}
```

## [03]-[SECTION_PROFILE]

- Owner: `SectionProfile` the closed cross-section `[Union]` — the `IfcParameterizedProfileDef` set carried natively with NAMED `PositiveMagnitude` dimensions, widened by the domain arms the nineteen families need (`CellularRectangle` per-cell voids, `Layered` plies, `Corrugated` deck fold, `Nominal` bond-line, `Catalogued` published identity, `Outline` typed free tail, `BuiltUp` positioned composition); `VoidCell` the fill-state cell row; `PlyRole` the BOUNDED layer-semantics vocabulary; `Ply` the laminate row.
- Cases: twenty-two arms; gross bounding `(WidthMm, DepthMm)` facts are BASE-CONSTRUCTOR STATE declared once per arm — no interior switch exists on this axis; the ONE dispatch site is `SectionSolver.Solve`. Growth cadence is the profile schema (buildingSMART parameterized-profile set), never thing cadence.
- Entry: each `Of` factory lifts universal magnitudes through `AcceptValidated`, fails scalar edge/slope invariants through `KernelFault`, and uses semantic geometry leaves for envelope, cell, ply, outline, and composition coherence.
- Boundary: `VoidCell` is MIN-CORNER anchored in the profile's `[0,WidthMm]×[0,DepthMm]` corner frame (the convention `MasonryVoids.Cells` and the cmu lattice emit); `Grouted` drives the as-built net (only ungrouted cells void), the grouted fraction, self-weight, and the parallel-path thermal split; `Reinforced` marks bar-bearing cells; geometry-only cells collapse as-built physics to the ungrouted case. `Ply.Role` is the BOUNDED `PlyRole` row, never a free string: `Longitudinal`/`Transverse` are the timber structural discriminants, `Pane`/`Interlayer`/`Cavity` the glazing stack semantics, the panel face/core rows carry appearance-facing policy, and `ConcreteWythe` is the precast sandwich structural layer a dimensioned wythe build instantiates; the `Oriented` column marks the grain-bearing rows (`Longitudinal`/`Transverse`/`VeneerPly`/`StrandLayer`) the `ComponentSymmetry` stack read folds, so a new figured layer authors placement direction by declaring its row, never by a consumer edit. `ComponentFamily.Admits` proves the lane subset, so a known panel role cannot enter timber and a known timber role cannot enter glazing. The human-readable `IfcMaterialLayer.Name` is a BOUNDARY projection over `(Material, Role, ordinal)` — an unknown role is unrepresentable, and no consumer parses a role string. `NonNegativeMagnitude` does not exist in `Rasm.Numerics` (verified) — the non-negative/finite edge check is the inline `Edge` guard inside `Of` (R8 resolved).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlyRole {
    public static readonly PlyRole Longitudinal        = new("0",                       facing: false, oriented: true);
    public static readonly PlyRole Transverse          = new("90",                      facing: false, oriented: true);
    public static readonly PlyRole Pane                = new("pane",                    facing: false, oriented: false);
    public static readonly PlyRole Interlayer          = new("interlayer",              facing: false, oriented: false);
    public static readonly PlyRole Cavity              = new("cavity",                  facing: false, oriented: false);
    public static readonly PlyRole PaperFace           = new("paper-face",              facing: true,  oriented: false);
    public static readonly PlyRole GlassMatFacer       = new("glass-mat-facer",         facing: true,  oriented: false);
    public static readonly PlyRole FoilFacer           = new("foil-facer",              facing: true,  oriented: false);
    public static readonly PlyRole GlassFiberMatFacer  = new("glass-fiber-mat-facer",   facing: true,  oriented: false);
    public static readonly PlyRole CoatedGlassFacer    = new("coated-glass-facer",     facing: true,  oriented: false);
    public static readonly PlyRole GlassMeshScrim      = new("glass-mesh-scrim",        facing: true,  oriented: false);
    public static readonly PlyRole GypsumCore          = new("gypsum-core",             facing: false, oriented: false);
    public static readonly PlyRole VeneerPly           = new("veneer-ply",              facing: false, oriented: true);
    public static readonly PlyRole StrandLayer         = new("strand-layer",            facing: false, oriented: true);
    public static readonly PlyRole CementAggregateCore = new("cement-aggregate-core",   facing: false, oriented: false);
    public static readonly PlyRole FoamCore            = new("foam-core",               facing: false, oriented: false);
    public static readonly PlyRole MembraneCore        = new("membrane-core",           facing: false, oriented: false);
    public static readonly PlyRole ConcreteWythe       = new("concrete-wythe",          facing: false, oriented: false);
    public bool Facing { get; }
    public bool Oriented { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileTopology {
    public static readonly ProfileTopology SolidPolygon = new("solid-polygon");
    public static readonly ProfileTopology SolidCurved  = new("solid-curved");
    public static readonly ProfileTopology Voided       = new("voided");
    public static readonly ProfileTopology ClosedThin   = new("closed-thin");
    public static readonly ProfileTopology OpenThin     = new("open-thin");
    public static readonly ProfileTopology Composition  = new("composition");
    public static readonly ProfileTopology Unsectioned  = new("unsectioned");

    public bool Solvable => this != Unsectioned;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct VoidCell(double XMm, double YMm, double WidthMm, double HeightMm, bool Grouted = false, bool Reinforced = false);

public readonly record struct Ply(MaterialId Material, PositiveMagnitude ThicknessMm, PlyRole Role);

[Union]
public abstract partial record SectionProfile {
    private SectionProfile(PositiveMagnitude widthMm, PositiveMagnitude depthMm) => GrossRectangleMm = (widthMm, depthMm);
    public (PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) GrossRectangleMm { get; }

    public sealed record Rectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) select (SectionProfile)new Rectangle(w, d);
    }

    public sealed record CellularRectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, Seq<VoidCell> Cells) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, Seq<VoidCell> cells) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm)
            from contained in guard(cells.ForAll(c => c.WidthMm > 0.0 && c.HeightMm > 0.0 && c.XMm >= 0.0 && c.YMm >= 0.0
                    && double.IsFinite(c.XMm + c.YMm + c.WidthMm + c.HeightMm)
                    && c.XMm + c.WidthMm <= w.Value && c.YMm + c.HeightMm <= d.Value),
                new ComponentFault.CellLayoutRejected(widthMm, depthMm, cells.Count))
            from disjoint in guard(Disjoint(cells), new ComponentFault.CellLayoutRejected(widthMm, depthMm, cells.Count))
            select (SectionProfile)new CellularRectangle(w, d, cells);

        static bool Disjoint(Seq<VoidCell> cells) =>
            toSeq(cells.Index()).ForAll(a => cells.Skip(a.Index + 1).ForAll(b => Apart(a.Item, b)));

        static bool Apart(VoidCell a, VoidCell b) =>
            a.XMm + a.WidthMm <= b.XMm || b.XMm + b.WidthMm <= a.XMm
            || a.YMm + a.HeightMm <= b.YMm || b.YMm + b.HeightMm <= a.YMm;
    }

    public sealed record RectangleHollow(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, PositiveMagnitude WallMm, double InnerFilletMm, double OuterFilletMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, double wallMm, double innerFilletMm, double outerFilletMm) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) from t in key.Pm(wallMm)
            from fi in key.Edge(innerFilletMm) from fo in key.Edge(outerFilletMm)
            from closed in guard(2.0 * t.Value < Math.Min(w.Value, d.Value), new ComponentFault.EnvelopeRejected(typeof(RectangleHollow), widthMm, depthMm))
            select (SectionProfile)new RectangleHollow(w, d, t, fi, fo);
    }

    public sealed record RoundedRectangle(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm, PositiveMagnitude RoundingMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm, double roundingMm) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) from r in key.Pm(roundingMm)
            from fits in guard(2.0 * r.Value <= Math.Min(w.Value, d.Value), new ComponentFault.EnvelopeRejected(typeof(RoundedRectangle), widthMm, depthMm))
            select (SectionProfile)new RoundedRectangle(w, d, r);
    }

    public sealed record Circle(PositiveMagnitude DiameterMm) : SectionProfile(DiameterMm, DiameterMm) {
        public static Fin<SectionProfile> Of(double diameterMm) =>
            key.Pm(diameterMm).Map(static d => (SectionProfile)new Circle(d));
    }

    public sealed record CircleHollow(PositiveMagnitude DiameterMm, PositiveMagnitude WallMm) : SectionProfile(DiameterMm, DiameterMm) {
        public static Fin<SectionProfile> Of(double diameterMm, double wallMm) =>
            from d in key.Pm(diameterMm) from t in key.Pm(wallMm)
            from closed in guard(2.0 * t.Value < d.Value, new ComponentFault.EnvelopeRejected(typeof(CircleHollow), diameterMm, diameterMm))
            select (SectionProfile)new CircleHollow(d, t);
    }

    public sealed record Ellipse(PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double widthMm, double depthMm) =>
            from w in key.Pm(widthMm) from d in key.Pm(depthMm) select (SectionProfile)new Ellipse(w, d);
    }

    public sealed record IShape(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double FlangeSlopeDeg => Math.Atan2(2.0 * (FlangeMm.Value - FlangeToeMm.Value), WidthMm.Value - WebMm.Value) * (180.0 / Math.PI);
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from te in key.Pm(flangeToeMm)
            from tapered in guard(te.Value <= tf.Value, new ComponentFault.EnvelopeRejected(typeof(IShape), widthMm, depthMm))
            from fits in guard(tw.Value < b.Value && 2.0 * tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - 2.0 * tf.Value),
                new ComponentFault.EnvelopeRejected(typeof(IShape), widthMm, depthMm))
            select (SectionProfile)new IShape(d, b, tw, tf, f, te);
    }

    public sealed record AsymmetricIShape(PositiveMagnitude DepthMm, PositiveMagnitude TopFlangeWidthMm, PositiveMagnitude BottomFlangeWidthMm, PositiveMagnitude WebThicknessMm, PositiveMagnitude TopFlangeThicknessMm, PositiveMagnitude BottomFlangeThicknessMm, double FilletMm) : SectionProfile(Wider(TopFlangeWidthMm, BottomFlangeWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double topFlangeWidthMm, double bottomFlangeWidthMm, double webThicknessMm, double topFlangeThicknessMm, double bottomFlangeThicknessMm, double filletMm) =>
            from d in key.Pm(depthMm) from bt in key.Pm(topFlangeWidthMm) from bb in key.Pm(bottomFlangeWidthMm)
            from tw in key.Pm(webThicknessMm) from tt in key.Pm(topFlangeThicknessMm) from tb in key.Pm(bottomFlangeThicknessMm) from f in key.Edge(filletMm)
            from fits in guard(tw.Value < Math.Min(bt.Value, bb.Value) && tt.Value + tb.Value < d.Value
                    && 2.0 * f <= Math.Min(Math.Min(bt.Value, bb.Value) - tw.Value, d.Value - tt.Value - tb.Value),
                new ComponentFault.EnvelopeRejected(typeof(AsymmetricIShape), Math.Max(topFlangeWidthMm, bottomFlangeWidthMm), depthMm))
            select (SectionProfile)new AsymmetricIShape(d, bt, bb, tw, tt, tb, f);
    }

    public sealed record Channel(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double FlangeSlopeDeg => Math.Atan2(FlangeMm.Value - FlangeToeMm.Value, WidthMm.Value - WebMm.Value) * (180.0 / Math.PI);
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from te in key.Pm(flangeToeMm)
            from tapered in guard(te.Value <= tf.Value, new ComponentFault.EnvelopeRejected(typeof(Channel), widthMm, depthMm))
            from fits in guard(tw.Value < b.Value && 2.0 * tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - 2.0 * tf.Value),
                new ComponentFault.EnvelopeRejected(typeof(Channel), widthMm, depthMm))
            select (SectionProfile)new Channel(d, b, tw, tf, f, te);
    }

    public sealed record ColdFormedC(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WallMm, PositiveMagnitude GirthMm, double InnerFilletMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double wallMm, double girthMm, double innerFilletMm) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(wallMm) from g in key.Pm(girthMm) from f in key.Edge(innerFilletMm)
            from lipped in guard(2.0 * t.Value < Math.Min(d.Value, b.Value) && g.Value + t.Value < b.Value && g.Value < d.Value / 2.0
                    && f <= Math.Min(b.Value - 2.0 * t.Value, d.Value / 2.0 - t.Value),
                new ComponentFault.EnvelopeRejected(typeof(ColdFormedC), widthMm, depthMm))
            select (SectionProfile)new ColdFormedC(d, b, t, g, f);
    }

    public sealed record Tee(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm, PositiveMagnitude WebToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;
        public double MeanWebMm => (WebMm.Value + WebToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm, double webToeMm) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from fe in key.Pm(flangeToeMm) from we in key.Pm(webToeMm)
            from tapered in guard(fe.Value <= tf.Value && we.Value <= tw.Value,
                new ComponentFault.EnvelopeRejected(typeof(Tee), widthMm, depthMm))
            from fits in guard(tw.Value < b.Value && tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - tf.Value),
                new ComponentFault.EnvelopeRejected(typeof(Tee), widthMm, depthMm))
            select (SectionProfile)new Tee(d, b, tw, tf, f, fe, we);
    }

    public sealed record Angle(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude ThicknessMm, double FilletMm, PositiveMagnitude LegToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double MeanLegMm => (ThicknessMm.Value + LegToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double thicknessMm, double filletMm, double legToeMm) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(thicknessMm)
            from f in key.Edge(filletMm) from e in key.Pm(legToeMm)
            from tapered in guard(e.Value <= t.Value, new ComponentFault.EnvelopeRejected(typeof(Angle), widthMm, depthMm))
            from fits in guard(t.Value < Math.Min(d.Value, b.Value) && 2.0 * f <= Math.Min(d.Value - t.Value, b.Value - t.Value),
                new ComponentFault.EnvelopeRejected(typeof(Angle), widthMm, depthMm))
            select (SectionProfile)new Angle(d, b, t, f, e);
    }

    public sealed record Zed(PositiveMagnitude DepthMm, PositiveMagnitude TopFlangeWidthMm, PositiveMagnitude BottomFlangeWidthMm, PositiveMagnitude WallMm, double TopLipMm, double BottomLipMm, double InnerFilletMm) : SectionProfile(Wider(TopFlangeWidthMm, BottomFlangeWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double topFlangeWidthMm, double bottomFlangeWidthMm, double thicknessMm, double topLipMm, double bottomLipMm, double innerFilletMm) =>
            from d in key.Pm(depthMm) from bt in key.Pm(topFlangeWidthMm) from bb in key.Pm(bottomFlangeWidthMm) from t in key.Pm(thicknessMm)
            from lt in key.Edge(topLipMm) from lb in key.Edge(bottomLipMm) from f in key.Edge(innerFilletMm)
            from fits in guard(t.Value < Math.Min(bt.Value, bb.Value) && 2.0 * t.Value < d.Value
                    && lt < d.Value / 2.0 && lb < d.Value / 2.0 && 2.0 * f <= d.Value - 2.0 * t.Value,
                new ComponentFault.EnvelopeRejected(typeof(Zed), Math.Max(topFlangeWidthMm, bottomFlangeWidthMm), depthMm))
            select (SectionProfile)new Zed(d, bt, bb, t, lt, lb, f);
    }

    public sealed record Trapezium : SectionProfile {
        private Trapezium(PositiveMagnitude bottomWidthMm, PositiveMagnitude topWidthMm, PositiveMagnitude depthMm, double topOffsetMm, PositiveMagnitude spanMm)
            : base(spanMm, depthMm) =>
            (BottomWidthMm, TopWidthMm, DepthMm, TopOffsetMm) = (bottomWidthMm, topWidthMm, depthMm, topOffsetMm);
        public PositiveMagnitude BottomWidthMm { get; }
        public PositiveMagnitude TopWidthMm { get; }
        public PositiveMagnitude DepthMm { get; }
        public double TopOffsetMm { get; }

        public static Fin<SectionProfile> Of(double bottomWidthMm, double topWidthMm, double depthMm, double topOffsetMm) =>
            from bb in key.Pm(bottomWidthMm) from bt in key.Pm(topWidthMm) from d in key.Pm(depthMm) from o in key.Slope(topOffsetMm)
            from span in key.Pm(Span(bb.Value, bt.Value, o))
            select (SectionProfile)new Trapezium(bb, bt, d, o, span);

        internal static double Span(double bottom, double top, double offset) {
            double topMin = (bottom - top) / 2.0 + offset;
            return Math.Max(bottom, topMin + top) - Math.Min(0.0, topMin);
        }
    }

    public sealed record FilletTriangle(PositiveMagnitude LegMm, PositiveMagnitude Leg2Mm) : SectionProfile(LegMm, Leg2Mm) {
        public static Fin<SectionProfile> Of(double legMm, double leg2Mm) =>
            from a in key.Pm(legMm) from b in key.Pm(leg2Mm) select (SectionProfile)new FilletTriangle(a, b);
    }

    public sealed record Corrugated(PositiveMagnitude CoverWidthMm, PositiveMagnitude RibDepthMm, PositiveMagnitude RibPitchMm, PositiveMagnitude GaugeMm, PositiveMagnitude TopFlatMm, PositiveMagnitude BottomFlatMm) : SectionProfile(CoverWidthMm, RibDepthMm) {
        public static Fin<SectionProfile> Of(double coverWidthMm, double ribDepthMm, double ribPitchMm, double gaugeMm, double topFlatMm, double bottomFlatMm) =>
            from cw in key.Pm(coverWidthMm) from rd in key.Pm(ribDepthMm) from rp in key.Pm(ribPitchMm)
            from g in key.Pm(gaugeMm) from tf in key.Pm(topFlatMm) from bf in key.Pm(bottomFlatMm)
            from folds in guard(tf.Value + bf.Value < rp.Value && rp.Value <= cw.Value, new ComponentFault.EnvelopeRejected(typeof(Corrugated), coverWidthMm, ribDepthMm))
            select (SectionProfile)new Corrugated(cw, rd, rp, g, tf, bf);
    }

    public sealed record Layered(Seq<Ply> Plies, PositiveMagnitude OverallMm, PositiveMagnitude WidthMm) : SectionProfile(WidthMm, OverallMm) {
        const double BuildBandMm = 0.5;

        public Seq<Ply> Facings => Plies.Filter(static p => p.Role.Facing);

        public static Fin<SectionProfile> Of(Seq<Ply> plies, double overallMm, double widthMm) =>
            from stacked in guard(!plies.IsEmpty, new ComponentFault.PlyStackRejected(plies.Count, overallMm, 0.0))
            from o in key.Pm(overallMm) from w in key.Pm(widthMm)
            from faced in guard(toSeq(plies.Index()).ForAll(p => !p.Item.Role.Facing || p.Index == 0 || p.Index == plies.Count - 1),
                new ComponentFault.PlyStackRejected(plies.Count, overallMm, plies.Sum(static p => p.ThicknessMm.Value)))
            from band in Tolerance.Of(lane: ToleranceLane.Build, value: BuildBandMm)
            from closed in guard(Math.Abs(plies.Sum(static p => p.ThicknessMm.Value) - o.Value) <= band.Value,
                new ComponentFault.PlyStackRejected(plies.Count, overallMm, plies.Sum(static p => p.ThicknessMm.Value)))
            select (SectionProfile)new Layered(plies, o, w);
    }

    public sealed record Nominal(PositiveMagnitude NominalMm) : SectionProfile(NominalMm, NominalMm) {
        public static Fin<SectionProfile> Of(double nominalMm) =>
            key.Pm(nominalMm).Map(static n => (SectionProfile)new Nominal(n));
    }

    public sealed record Catalogued(SteelShape Shape) : SectionProfile(Shape.Section.WidthMm, Shape.Section.DepthMm);

    public sealed record Outline(Seq<(double YMm, double ZMm)> Ring, ProfileTopology Topology, PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(Seq<(double YMm, double ZMm)> ring, ProfileTopology topology, double widthMm, double depthMm) =>
            from closed in guard(ring.Count >= 3, new ComponentFault.OutlineRejected(ring.Count, topology))
            from finite in guard(ring.ForAll(static v => double.IsFinite(v.YMm) && double.IsFinite(v.ZMm)),
                new ComponentFault.OutlineRejected(ring.Count, topology))
            from free in guard(topology == ProfileTopology.SolidPolygon || topology == ProfileTopology.OpenThin,
                new ComponentFault.OutlineRejected(ring.Count, topology))
            from w in key.Pm(widthMm) from d in key.Pm(depthMm)
            select (SectionProfile)new Outline(ring, topology, w, d);
    }

    public sealed record BuiltUp : SectionProfile {
        private BuiltUp(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts, PositiveMagnitude widthMm, PositiveMagnitude depthMm)
            : base(widthMm, depthMm) => Parts = parts;
        public Seq<(SectionProfile Part, double DyMm, double DzMm)> Parts { get; }

        public static Fin<SectionProfile> Of(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts) =>
            from stacked in guard(parts.Count >= 2, new ComponentFault.CompositionRejected(parts.Count))
            from placed in guard(parts.ForAll(static p => double.IsFinite(p.DyMm) && double.IsFinite(p.DzMm)),
                new ComponentFault.CompositionRejected(parts.Count))
            from membered in guard(parts.ForAll(static p => p.Part is not (SectionProfile.Layered or SectionProfile.Nominal or SectionProfile.BuiltUp)),
                new ComponentFault.CompositionRejected(parts.Count))
            from w in key.Pm(Span(parts, static p => (p.DyMm, p.Part.GrossRectangleMm.WidthMm.Value)))
            from d in key.Pm(Span(parts, static p => (p.DzMm, p.Part.GrossRectangleMm.DepthMm.Value)))
            select (SectionProfile)new BuiltUp(parts, w, d);

        static double Span(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts, Func<(SectionProfile Part, double DyMm, double DzMm), (double Offset, double Extent)> axis) =>
            parts.Map(axis).Fold((Hi: double.NegativeInfinity, Lo: double.PositiveInfinity),
                    static (bounds, member) => (Math.Max(bounds.Hi, member.Offset + member.Extent / 2.0), Math.Min(bounds.Lo, member.Offset - member.Extent / 2.0)))
                is var span ? span.Hi - span.Lo : 0.0;
    }

    public string Case => SectionGeometry.Of(this).Case;
    public ProfileTopology Topology => SectionGeometry.Of(this).Topology;

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
file static class Admit {
    extension() {
        public Fin<PositiveMagnitude> Pm(double candidateMm) => FactoryBridge.Accept<PositiveMagnitude>(candidate: candidateMm);
        public Fin<double> Edge(double candidateMm) =>
            double.IsFinite(candidateMm) && candidateMm >= 0.0
                ? Fin.Succ(candidateMm)
                : new KernelFault.OutOfRange(nameof(candidateMm), candidateMm, "finite and non-negative");
        public Fin<double> Slope(double candidate) =>
            double.IsFinite(candidate)
                ? Fin.Succ(candidate)
                : new KernelFault.InvalidValue(nameof(candidate), "a finite scalar");
    }
}
```

## [04]-[SECTION_SOLVER]

- Owner: `SectionSolver` — ONE solver replaces every per-family perimeter builder: one generated exhaustive `Switch` over the closed profile axis routes the ONE `new SectionProperties((IProfile)…)` Green's-theorem integral (decompile-verified: both `SectionProperties(IProfile)` and `SectionProperties(ISection)` constructors exist) through the ONE twenty-column `Admit` lift with a `SectionSupplement` topology row; `Curves` the per-arm profile table; `Forms` the per-arm supplement table.
- Cases: `Curves` lowers each arm onto a verified `VividOrange.IProfiles` contract or a typed `Perimeter`. The `Perimeter` path owns multi-void, corrugated, asymmetric-trapezium, triangle, and free-outline geometry; `Trapezium.TopOffsetMm` therefore changes the integrated centroid and moments instead of disappearing through the symmetric `ITrapezoid` contract.
- Entry: `SectionSolver.Solve(profile, key)` and `SectionSolver.ProfileOf(profile, key)` are two COLUMN READS over the one `SectionGeometry.Of` row; `ProfileOf` is the PROFILE-FAITHFUL RC-outline entry `reinforcement#RC_SECTION` feeds, and because both reads take the SAME outline column, a circular column feeds `ConcreteSection` its true `ICircle`, a trapezoidal member its integrated `Perimeter`, and a cellular unit its void-bearing perimeter whose grouted cells are already filled — so a partly-grouted unit cannot present as solid stock on the RC path while presenting its true net on the elastic one.
- Boundary: `SectionSolver.Solve` is the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.SectionSolve(Key, Profile, Section, Elapsed)` tap SUBJECT and the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.SectionSolve` measured kernel; the tap is a composition-root decorator on the folder hook set at `MaterialsPoint.SectionSolve`, so this owner emits nothing, carries no `Duration`, and references no signal type — the boundary is declared at both ends and instrumented at neither.
- Boundary: `Forms` states each arm's midline and strip stack; solver-generated value invariants cross the kernel bridge, cross-column coherence fails `SectionIncoherent`, and provider throws retain their exact exceptional `Error` through one `Op.Catch`.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public readonly record struct SectionSupplement(
    double Zx, double Zy, double J, double Avy, double Avz,
    double Iw = 0.0, double ShearCentreY = 0.0, double ShearCentreZ = 0.0, double Monosymmetry = 0.0);

public readonly record struct ProfileGeometry(
    string Case,
    ProfileTopology Topology,
    Func< Fin<IProfile>> Outline,
    Func< Fin<ComputedSection>> Section);

internal static class SectionGeometry {
    public static ProfileGeometry Of(SectionProfile profile) =>
        profile.Switch(
            rectangle:         static r => Solid(nameof(SectionProfile.Rectangle), ProfileTopology.SolidPolygon, r, () => Curves.Rect(r.WidthMm.Value, r.DepthMm.Value), () => Forms.SolidRectangle(r)),
            cellularRectangle: static c => Solid(nameof(SectionProfile.CellularRectangle), ProfileTopology.Voided, c, () => Curves.RectWithVoids(c.WidthMm.Value, c.DepthMm.Value, c.Cells), () => Forms.NetRectangle(c)),
            rectangleHollow:   static r => Solid(nameof(SectionProfile.RectangleHollow), ProfileTopology.ClosedThin, r, () => Curves.RectTube(r), () => Forms.BoxTube(r)),
            roundedRectangle:  static r => Solid(nameof(SectionProfile.RoundedRectangle), ProfileTopology.SolidPolygon, r, () => Curves.RoundedRect(r), () => Forms.SolidRounded(r)),
            circle:            static c => Solid(nameof(SectionProfile.Circle), ProfileTopology.SolidCurved, c, () => Curves.Disc(c), () => Forms.SolidCircle(c)),
            circleHollow:      static c => Solid(nameof(SectionProfile.CircleHollow), ProfileTopology.ClosedThin, c, () => Curves.Ring(c), () => Forms.Pipe(c)),
            ellipse:           static e => Solid(nameof(SectionProfile.Ellipse), ProfileTopology.SolidCurved, e, () => Curves.Ellipse(e), () => Forms.SolidEllipse(e)),
            iShape:            static i => Solid(nameof(SectionProfile.IShape), ProfileTopology.OpenThin, i, () => Curves.I(i), () => Forms.OpenI(i)),
            asymmetricIShape:  static i => Solid(nameof(SectionProfile.AsymmetricIShape), ProfileTopology.OpenThin, i, () => Curves.AsymI(i), () => Forms.MonoI(i)),
            channel:           static u => Solid(nameof(SectionProfile.Channel), ProfileTopology.OpenThin, u, () => Curves.U(u), () => Forms.OpenChannel(u)),
            coldFormedC:       static c => Solid(nameof(SectionProfile.ColdFormedC), ProfileTopology.OpenThin, c, () => Curves.Cee(c), () => Forms.OpenCee(c)),
            tee:               static t => Solid(nameof(SectionProfile.Tee), ProfileTopology.OpenThin, t, () => Curves.T(t), () => Forms.MonoTee(t)),
            angle:             static l => Solid(nameof(SectionProfile.Angle), ProfileTopology.OpenThin, l, () => Curves.L(l), () => Forms.OpenAngle(l)),
            zed:               static z => Solid(nameof(SectionProfile.Zed), ProfileTopology.OpenThin, z, () => Curves.Z(z), () => Forms.PointSymmetricZ(z)),
            trapezium:         static t => Solid(nameof(SectionProfile.Trapezium), ProfileTopology.SolidPolygon, t, () => Curves.Trapezoid(t), () => Forms.SolidTrapezoid(t)),
            filletTriangle:    static f => Solid(nameof(SectionProfile.FilletTriangle), ProfileTopology.SolidPolygon, f, () => Curves.RightTriangle(f), () => Forms.SolidTriangle(f)),
            corrugated:        static d => Solid(nameof(SectionProfile.Corrugated), ProfileTopology.OpenThin, d, () => Curves.Deck(d), () => Forms.ThinFold(d)),
            layered:           static _ => Unsectioned(nameof(SectionProfile.Layered)),
            nominal:           static _ => Unsectioned(nameof(SectionProfile.Nominal)),
            catalogued:        static c => Solid(nameof(SectionProfile.Catalogued), TopologyOf(c.Shape.Class.Topology), c, () => c.Shape.Profile, () => Forms.FromCatalogue(c.Shape)),
            outline:           static o => Solid(nameof(SectionProfile.Outline), o.Topology, o, () => Curves.Free(o), () => Forms.Integrated(o)),
            builtUp:           static b => new ProfileGeometry(nameof(SectionProfile.BuiltUp), ProfileTopology.Composition,
                                   key => new ComponentFault.SectionIncoherent(typeof(SectionProfile.BuiltUp)),
                                   key => SectionSolver.Compose(b)));

    static ProfileGeometry Solid(string @case, ProfileTopology topology, SectionProfile source, Func<IProfile> outline, Func<SectionSupplement> supplement) =>
        new(@case, topology,
            key => Try.lift(() => Fin.Succ(outline())).Run().Bind(static inner => inner),
            key => SectionSolver.Admit(outline, source, supplement));

    static ProfileGeometry Unsectioned(string @case) =>
        new(@case, ProfileTopology.Unsectioned,
            key => new ComponentFault.SectionIncoherent(@case == nameof(SectionProfile.Layered) ? typeof(SectionProfile.Layered) : typeof(SectionProfile.Nominal)),
            key => new ComponentFault.SectionIncoherent(@case == nameof(SectionProfile.Layered) ? typeof(SectionProfile.Layered) : typeof(SectionProfile.Nominal)));

    static ProfileTopology TopologyOf(SteelTopology topology) => topology.Map(
        open: ProfileTopology.OpenThin, closed: ProfileTopology.ClosedThin, solid: ProfileTopology.SolidPolygon);
}

public static class SectionSolver {
    public static Fin<ComputedSection> Solve(SectionProfile profile) => SectionGeometry.Of(profile).Section();

    public static Fin<IProfile> ProfileOf(SectionProfile profile) => SectionGeometry.Of(profile).Outline();

    internal static Fin<ComputedSection> Compose(SectionProfile.BuiltUp b) =>
        b.Parts.Traverse(p => Solve(p.Part).Map(cs => (S: cs, p.DyMm, p.DzMm))).As().Bind(members => {
            double a = members.Sum(static m => m.S.AreaMm2.Value);
            double cy = members.Sum(static m => m.S.AreaMm2.Value * m.DyMm) / a;
            double cz = members.Sum(static m => m.S.AreaMm2.Value * m.DzMm) / a;
            double ix = members.Sum(m => m.S.IxMm4.Value + m.S.AreaMm2.Value * Math.Pow(m.DzMm - cz, 2.0));
            double iy = members.Sum(m => m.S.IyMm4.Value + m.S.AreaMm2.Value * Math.Pow(m.DyMm - cy, 2.0));
            double fibreZ = members.Max(m => Math.Abs(m.DzMm - cz) + m.S.DepthMm.Value / 2.0);
            double fibreY = members.Max(m => Math.Abs(m.DyMm - cy) + m.S.WidthMm.Value / 2.0);
            double zx = Plastic.Modulus(members.Map(static m => (m.S.AreaMm2.Value, m.DzMm, m.S.DepthMm.Value, m.S.ZxMm3.Value)));
            double zy = Plastic.Modulus(members.Map(static m => (m.S.AreaMm2.Value, m.DyMm, m.S.WidthMm.Value, m.S.ZyMm3.Value)));
            return
                from area in Section(a)
                from ixV in Section(ix)
                from iyV in Section(iy)
                from sx in Section(ix / fibreZ)
                from sy in Section(iy / fibreY)
                from rx in Section(Math.Sqrt(ix / a))
                from ry in Section(Math.Sqrt(iy / a))
                from zxV in Section(zx)
                from zyV in Section(zy)
                from j in Section(members.Sum(static m => m.S.JMm4.Value))
                from avy in Section(members.Sum(static m => m.S.AvyMm2.Value))
                from avz in Section(members.Sum(static m => m.S.AvzMm2.Value))
                from perim in Section(members.Sum(static m => m.S.HeatedPerimeterMm.Value))
                select new ComputedSection(area, ixV, iyV, sx, sy, rx, ry, zxV, zyV, j, IwMm6: 0.0, avy, avz,
                    DepthMm: b.GrossRectangleMm.DepthMm, WidthMm: b.GrossRectangleMm.WidthMm, HeatedPerimeterMm: perim,
                    AxisDistanceMm: 0.0, ShearCentreYMm: 0.0, ShearCentreZMm: 0.0, MonosymmetryFactor: 0.0);
        });

    internal static Fin<ComputedSection> Admit(Func<IProfile> outline, SectionProfile source, Func<SectionSupplement> supplement) =>
        Try.lift(() => {
                SectionProperties carrier = new(outline());
                return Fin.Succ((Area: carrier.Area.SquareMillimeters,
                    Ix: carrier.MomentOfInertiaYy.MillimetersToTheFourth, Iy: carrier.MomentOfInertiaZz.MillimetersToTheFourth,
                    Sx: carrier.ElasticSectionModulusYy.CubicMillimeters, Sy: carrier.ElasticSectionModulusZz.CubicMillimeters,
                    Rx: carrier.RadiusOfGyrationYy.Millimeters, Ry: carrier.RadiusOfGyrationZz.Millimeters,
                    Perim: carrier.Perimeter.Millimeters, Supplement: supplement()));
            }).Run().Bind(static inner => inner)
            .Bind(p => {
                SectionSupplement s = p.Supplement;
                return (Section(p.Area), Section(p.Ix), Section(p.Iy),
                        Section(p.Sx), Section(p.Sy), Section(p.Rx),
                        Section(p.Ry), Section(s.Zx), Section(s.Zy),
                        Section(s.J))
                    .Apply(static (area, ix, iy, sx, sy, rx, ry, zx, zy, jj) => (area, ix, iy, sx, sy, rx, ry, zx, zy, jj))
                    .As()
                    .Bind(head =>
                        (Section(s.Avy), Section(s.Avz), Section(p.Perim),
                         AdmissionSlots.Gate(
                             s.Iw >= 0.0 && double.IsFinite(s.Iw) && double.IsFinite(s.ShearCentreY)
                             && double.IsFinite(s.ShearCentreZ) && double.IsFinite(s.Monosymmetry),
                             new ComponentFault.SectionIncoherent(source.GetType())))
                        .Apply((avy, avz, perim, _) => new ComputedSection(
                            head.area, head.ix, head.iy, head.sx, head.sy, head.rx, head.ry, head.zx, head.zy, head.jj,
                            IwMm6: s.Iw, avy, avz,
                            DepthMm: source.GrossRectangleMm.DepthMm, WidthMm: source.GrossRectangleMm.WidthMm,
                            HeatedPerimeterMm: perim, AxisDistanceMm: 0.0,
                            ShearCentreYMm: s.ShearCentreY, ShearCentreZMm: s.ShearCentreZ, MonosymmetryFactor: s.Monosymmetry))
                        .As())
                    .ToFin();
            });

    static Validation<Error, PositiveMagnitude> Section(double mm) =>
        FactoryBridge.Accept<PositiveMagnitude>(candidate: mm)
            .ToValidation();

    // --- [CURVES]
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
        public static IProfile I(SectionProfile.IShape i) => new IBeamRow(Mm(i.DepthMm.Value), Mm(i.WidthMm.Value), Mm(i.MeanFlangeMm), Mm(i.WebMm.Value), Mm(i.FilletMm));
        public static IProfile AsymI(SectionProfile.AsymmetricIShape i) =>
            new MonoIRow(Mm(i.DepthMm.Value), Mm(i.TopFlangeWidthMm.Value), Mm(i.BottomFlangeWidthMm.Value), Mm(i.TopFlangeThicknessMm.Value), Mm(i.BottomFlangeThicknessMm.Value), Mm(i.WebThicknessMm.Value));
        public static IProfile U(SectionProfile.Channel u) => new ChannelRow(Mm(u.DepthMm.Value), Mm(u.WidthMm.Value), Mm(u.WebMm.Value), Mm(u.MeanFlangeMm));
        public static IProfile Cee(SectionProfile.ColdFormedC c) => new CeeRow(Mm(c.DepthMm.Value), Mm(c.WidthMm.Value), Mm(c.WallMm.Value), Mm(c.WallMm.Value), Mm(c.GirthMm.Value));
        public static IProfile T(SectionProfile.Tee t) => new TeeRow(Mm(t.DepthMm.Value), Mm(t.WidthMm.Value), Mm(t.MeanWebMm), Mm(t.MeanFlangeMm));
        public static IProfile L(SectionProfile.Angle l) => new AngleRow(Mm(l.DepthMm.Value), Mm(l.WidthMm.Value), Mm(l.MeanLegMm), Mm(l.MeanLegMm));
        public static IProfile Z(SectionProfile.Zed z) =>
            new ZedRow(Mm(z.DepthMm.Value), Mm(z.TopFlangeWidthMm.Value), Mm(z.BottomFlangeWidthMm.Value), Mm(z.WallMm.Value), Mm(z.TopLipMm), Mm(z.BottomLipMm));

        public static IProfile Free(SectionProfile.Outline o) =>
            new Perimeter(o.Ring.Map(static v => Pt(v.YMm, v.ZMm)).ToList());

        public static IProfile Trapezoid(SectionProfile.Trapezium t) {
            double topMin = (t.BottomWidthMm.Value - t.TopWidthMm.Value) / 2.0 + t.TopOffsetMm;
            double shift = -Math.Min(0.0, topMin);
            return new Perimeter(new List<ILocalPoint2d> {
                Pt(shift, 0.0), Pt(shift + t.BottomWidthMm.Value, 0.0),
                Pt(shift + topMin + t.TopWidthMm.Value, t.DepthMm.Value), Pt(shift + topMin, t.DepthMm.Value),
            });
        }
        public static IProfile RightTriangle(SectionProfile.FilletTriangle f) =>
            new Perimeter(new List<ILocalPoint2d> { Pt(0.0, 0.0), Pt(f.LegMm.Value, 0.0), Pt(0.0, f.Leg2Mm.Value) });

        public static IProfile Deck(SectionProfile.Corrugated d) {
            double rp = d.RibPitchMm.Value, g = d.GaugeMm.Value, tf = d.TopFlatMm.Value, bf = d.BottomFlatMm.Value, rd = d.RibDepthMm.Value;
            int ribs = Math.Max(1, (int)Math.Floor(d.CoverWidthMm.Value / rp));
            double web = (rp - tf - bf) / 2.0;
            Seq<(double X, double Z)> crest = toSeq(Enumerable.Range(0, ribs)).Bind(i =>
                Seq((i * rp, 0.0), (i * rp + bf, 0.0), (i * rp + bf + web, rd), (i * rp + bf + web + tf, rd), (i * rp + rp, 0.0)));
            return new Perimeter(new LocalPolyline2d((crest + crest.Rev().Map(p => (p.X, p.Z + g))).Map(p => Pt(p.X, p.Z)).ToList()));
        }

        static ILocalPolyline2d CornerLoop(double x0, double y0, double w, double h) =>
            new LocalPolyline2d(new List<ILocalPoint2d> { Pt(x0, y0), Pt(x0 + w, y0), Pt(x0 + w, y0 + h), Pt(x0, y0 + h) });
        static ILocalPoint2d Pt(double y, double z) => new LocalPoint2d(Mm(y), Mm(z));
        static Length Mm(double mm) => Length.FromMillimeters(mm);
    }

    // --- [FORMS]
    internal static class Forms {
        static SectionSupplement Open(Seq<WallSegment> midline, Seq<Strip> major, Seq<Strip> minor, double avy, double avz) {
            OpenWall wall = ThinWalled.Of(midline);
            return new(Plastic.Modulus(major), Plastic.Modulus(minor), wall.Torsion, avy, avz,
                Iw: wall.Warping, ShearCentreY: wall.ShearCentreY, ShearCentreZ: wall.ShearCentreZ,
                Monosymmetry: 1.8 * wall.ShearCentreZ);
        }

        public static SectionSupplement SolidRectangle(SectionProfile.Rectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);
        public static SectionSupplement SolidRounded(SectionProfile.RoundedRectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);

        // --- [OPEN_THIN_WALLED]
        public static SectionSupplement OpenI(SectionProfile.IShape i) {
            double d = i.DepthMm.Value, b = i.WidthMm.Value, tw = i.WebMm.Value, tf = i.FlangeMm.Value, te = i.FlangeToeMm.Value, tm = i.MeanFlangeMm;
            double h = Math.Max(0.0, d - 2.0 * tm);
            return Open(
                Seq(Wall(-1, 0.0, tm / 2.0, 0.0, d - tm / 2.0, tw),
                    Wall(0, 0.0, d - tm / 2.0, b / 2.0, d - tm / 2.0, tm),
                    Wall(0, 0.0, d - tm / 2.0, -b / 2.0, d - tm / 2.0, tm),
                    Wall(-1, 0.0, tm / 2.0, b / 2.0, tm / 2.0, tm),
                    Wall(-1, 0.0, tm / 2.0, -b / 2.0, tm / 2.0, tm)),
                FlangedMajor(d, b, tw, tf, te),
                FlangedMinor(d, b, tw, tf, te),
                d * tw, 2.0 * b * tm);
        }

        public static SectionSupplement MonoI(SectionProfile.AsymmetricIShape i) {
            double d = i.DepthMm.Value, tw = i.WebThicknessMm.Value;
            double bt = i.TopFlangeWidthMm.Value, bb = i.BottomFlangeWidthMm.Value;
            double tt = i.TopFlangeThicknessMm.Value, tb = i.BottomFlangeThicknessMm.Value;
            return Open(
                Seq(Wall(-1, 0.0, tb / 2.0, 0.0, d - tt / 2.0, tw),
                    Wall(0, 0.0, d - tt / 2.0, bt / 2.0, d - tt / 2.0, tt),
                    Wall(0, 0.0, d - tt / 2.0, -bt / 2.0, d - tt / 2.0, tt),
                    Wall(-1, 0.0, tb / 2.0, bb / 2.0, tb / 2.0, tb),
                    Wall(-1, 0.0, tb / 2.0, -bb / 2.0, tb / 2.0, tb)),
                Seq(Band(0.0, tb, bb), Band(tb, d - tt, tw), Band(d - tt, d, bt)),
                Seq(Band(-bb / 2.0, -tw / 2.0, tb), Band(-bt / 2.0, -tw / 2.0, tt),
                    Band(-tw / 2.0, tw / 2.0, d), Band(tw / 2.0, bt / 2.0, tt), Band(tw / 2.0, bb / 2.0, tb)),
                d * tw, bt * tt + bb * tb);
        }

        public static SectionSupplement OpenChannel(SectionProfile.Channel u) {
            double d = u.DepthMm.Value, b = u.WidthMm.Value, tw = u.WebMm.Value, tf = u.FlangeMm.Value, te = u.FlangeToeMm.Value, tm = u.MeanFlangeMm;
            double reach = b - tw / 2.0;
            return Open(
                Seq(Wall(-1, 0.0, tm / 2.0, 0.0, d - tm / 2.0, tw),
                    Wall(0, 0.0, d - tm / 2.0, reach, d - tm / 2.0, tm),
                    Wall(-1, 0.0, tm / 2.0, reach, tm / 2.0, tm)),
                FlangedMajor(d, b, tw, tf, te),
                Seq(Band(0.0, tw, d), Taper(tw, b, 2.0 * tf, 2.0 * te)),
                d * tw, 2.0 * b * tm);
        }

        public static SectionSupplement OpenCee(SectionProfile.ColdFormedC c) {
            double d = c.DepthMm.Value, b = c.WidthMm.Value, t = c.WallMm.Value, lip = c.GirthMm.Value;
            double reach = b - t / 2.0;
            return Open(
                Seq(Wall(-1, 0.0, t / 2.0, 0.0, d - t / 2.0, t),
                    Wall(0, 0.0, d - t / 2.0, reach, d - t / 2.0, t),
                    Wall(1, reach, d - t / 2.0, reach, d - t / 2.0 - lip, t),
                    Wall(-1, 0.0, t / 2.0, reach, t / 2.0, t),
                    Wall(3, reach, t / 2.0, reach, t / 2.0 + lip, t)),
                Seq(Band(0.0, t, b), Band(t, d - t, t), Band(d - t, d, b), Band(t, t + lip, t), Band(d - t - lip, d - t, t)),
                Seq(Band(0.0, t, d), Band(t, b, 2.0 * t), Band(b - t, b, 2.0 * lip)),
                d * t, 2.0 * b * t);
        }

        public static SectionSupplement MonoTee(SectionProfile.Tee t) {
            double d = t.DepthMm.Value, b = t.WidthMm.Value, tw = t.WebMm.Value, we = t.WebToeMm.Value;
            double tf = t.FlangeMm.Value, fe = t.FlangeToeMm.Value, fm = t.MeanFlangeMm, wm = t.MeanWebMm;
            return Open(
                Seq(Wall(-1, 0.0, 0.0, 0.0, d - fm / 2.0, wm),
                    Wall(0, 0.0, d - fm / 2.0, b / 2.0, d - fm / 2.0, fm),
                    Wall(0, 0.0, d - fm / 2.0, -b / 2.0, d - fm / 2.0, fm)),
                Seq(Taper(0.0, d - tf, we, tw), Taper(d - tf, d - fe, tw, b), Band(d - fe, d, b)),
                Seq(Band(-b / 2.0, -wm / 2.0, fm), Band(-wm / 2.0, wm / 2.0, d), Band(wm / 2.0, b / 2.0, fm)),
                d * wm, b * fm);
        }

        public static SectionSupplement OpenAngle(SectionProfile.Angle l) {
            double d = l.DepthMm.Value, b = l.WidthMm.Value, t = l.ThicknessMm.Value, e = l.LegToeMm.Value, m = l.MeanLegMm;
            return Open(
                Seq(Wall(-1, 0.0, 0.0, 0.0, d - m / 2.0, m),
                    Wall(-1, 0.0, 0.0, b - m / 2.0, 0.0, m)),
                Seq(Band(0.0, m, b), Taper(m, d, t, e)),
                Seq(Band(0.0, m, d), Taper(m, b, t, e)),
                d * m, b * m);
        }

        public static SectionSupplement PointSymmetricZ(SectionProfile.Zed z) {
            double d = z.DepthMm.Value, t = z.WallMm.Value;
            double bt = z.TopFlangeWidthMm.Value, bb = z.BottomFlangeWidthMm.Value, lt = z.TopLipMm, lb = z.BottomLipMm;
            double reachT = bt - t / 2.0, reachB = bb - t / 2.0;
            return Open(
                Seq(Wall(-1, 0.0, t / 2.0, 0.0, d - t / 2.0, t),
                    Wall(0, 0.0, d - t / 2.0, reachT, d - t / 2.0, t),
                    Wall(1, reachT, d - t / 2.0, reachT, d - t / 2.0 - lt, t),
                    Wall(-1, 0.0, t / 2.0, -reachB, t / 2.0, t),
                    Wall(3, -reachB, t / 2.0, -reachB, t / 2.0 + lb, t)),
                Seq(Band(0.0, t, bb), Band(t, d - t, t), Band(d - t, d, bt), Band(t, t + lb, t), Band(d - t - lt, d - t, t)),
                Seq(Band(-bb, -bb + t, lb), Band(-bb, 0.0, t), Band(0.0, t, d), Band(0.0, bt, t), Band(bt - t, bt, lt)),
                d * t, (bt + bb) * t);
        }

        // --- [STRIP_BUILDERS]
        static Seq<Strip> FlangedMajor(double d, double b, double tw, double tf, double te) =>
            Seq(Band(0.0, te, b), Taper(te, tf, tw, b),
                Band(tf, d - tf, tw),
                Taper(d - tf, d - te, b, tw), Band(d - te, d, b));

        static Seq<Strip> FlangedMinor(double d, double b, double tw, double tf, double te) =>
            Seq(Taper(-b / 2.0, -tw / 2.0, 2.0 * te, 2.0 * tf),
                Band(-tw / 2.0, tw / 2.0, d),
                Taper(tw / 2.0, b / 2.0, 2.0 * tf, 2.0 * te));

        static Strip Band(double lo, double hi, double width) => new(lo, hi, width, width);
        static Strip Taper(double lo, double hi, double widthLo, double widthHi) => new(lo, hi, widthLo, widthHi);
        static WallSegment Wall(int parent, double y0, double z0, double y1, double z1, double thickness) => new(parent, y0, z0, y1, z1, thickness);

        public static SectionSupplement NetRectangle(SectionProfile.CellularRectangle c) {
            double w = c.WidthMm.Value, d = c.DepthMm.Value;
            Seq<VoidCell> open = c.Cells.Filter(static v => !v.Grouted);
            double zx = w * d * d / 4.0 - open.Sum(v => PlasticCut(v.WidthMm, v.HeightMm, v.YMm + v.HeightMm / 2.0 - d / 2.0));
            double zy = d * w * w / 4.0 - open.Sum(v => PlasticCut(v.HeightMm, v.WidthMm, v.XMm + v.WidthMm / 2.0 - w / 2.0));
            double j = RectJ(w, d) - open.Sum(static v => RectJ(v.WidthMm, v.HeightMm));
            double net = w * d - open.Sum(static v => v.WidthMm * v.HeightMm);
            return new(zx, zy, j, 2.0 * net / 3.0, 2.0 * net / 3.0);
        }
        static double PlasticCut(double b, double h, double yBar) =>
            Math.Abs(yBar) < h / 2.0 ? b * (h * h / 4.0 + yBar * yBar) : b * h * Math.Abs(yBar);

        public static SectionSupplement BoxTube(SectionProfile.RectangleHollow r) {
            double b = r.WidthMm.Value, h = r.DepthMm.Value, t = r.WallMm.Value, bi = b - 2.0 * t, hi = h - 2.0 * t;
            return new(0.25 * (b * h * h - bi * hi * hi), 0.25 * (h * b * b - hi * bi * bi),
                ClosedRectJ(b, h, t, r.InnerFilletMm, r.OuterFilletMm), 2.0 * h * t, 2.0 * b * t);
        }

        public static SectionSupplement SolidCircle(SectionProfile.Circle c) {
            double dia = c.DiameterMm.Value, a = Math.PI * dia * dia / 4.0;
            return new(dia * dia * dia / 6.0, dia * dia * dia / 6.0, Math.PI * Math.Pow(dia, 4.0) / 32.0,
                0.75 * a, 0.75 * a);
        }

        public static SectionSupplement Pipe(SectionProfile.CircleHollow c) => RoundTube(c.DiameterMm.Value, c.WallMm.Value);
        static SectionSupplement RoundTube(double od, double t) {
            double ri = Math.Max(0.0, od / 2.0 - t), ro = od / 2.0;
            double z = (Math.Pow(od, 3.0) - Math.Pow(2.0 * ri, 3.0)) / 6.0;
            double area = Math.PI * (ro * ro - ri * ri);
            return new(z, z, Math.PI * 0.5 * (Math.Pow(ro, 4.0) - Math.Pow(ri, 4.0)), area * 0.5, area * 0.5);
        }

        public static SectionSupplement SolidEllipse(SectionProfile.Ellipse e) {
            double a = e.WidthMm.Value / 2.0, b = e.DepthMm.Value / 2.0, area = Math.PI * a * b;
            return new(4.0 * a * b * b / 3.0, 4.0 * b * a * a / 3.0,
                Math.PI * Math.Pow(a, 3.0) * Math.Pow(b, 3.0) / (a * a + b * b), 0.75 * area, 0.75 * area);
        }

        public static SectionSupplement SolidTrapezoid(SectionProfile.Trapezium t) => Free(Curves.Trapezoid(t), TrapezoidRing(t), ProfileTopology.SolidPolygon);
        public static SectionSupplement SolidTriangle(SectionProfile.FilletTriangle f) => Free(Curves.RightTriangle(f), TriangleRing(f), ProfileTopology.SolidPolygon);
        public static SectionSupplement Integrated(SectionProfile.Outline o) => Free(Curves.Free(o), o.Ring, o.Topology);

        static SectionSupplement Free(IProfile p, Seq<(double YMm, double ZMm)> ring, ProfileTopology topology) {
            double a = Areas.CalculateArea(p).SquareMillimeters;
            double ip = Inertiae.CalculateInertiaYy(p).MillimetersToTheFourth + Inertiae.CalculateInertiaZz(p).MillimetersToTheFourth;
            double perimeter = toSeq(ring.Index()).Sum(v => Edge(v.Item, ring[(v.Index + 1) % ring.Count]));
            double thickness = perimeter > 0.0 ? 2.0 * a / perimeter : 0.0;
            return new(
                Plastic.Modulus(RingStrips(ring, static v => v.ZMm, static v => v.YMm)),
                Plastic.Modulus(RingStrips(ring, static v => v.YMm, static v => v.ZMm)),
                topology == ProfileTopology.OpenThin
                    ? perimeter / 2.0 * thickness * thickness * thickness / 3.0
                    : Math.Pow(a, 4.0) / (40.0 * Math.Max(ip, EpsilonPolicy.ZeroTolerance)),
                2.0 * a / 3.0, 2.0 * a / 3.0);
        }

        static double Edge((double YMm, double ZMm) a, (double YMm, double ZMm) b) =>
            Math.Sqrt((b.YMm - a.YMm) * (b.YMm - a.YMm) + (b.ZMm - a.ZMm) * (b.ZMm - a.ZMm));

        static Seq<(double YMm, double ZMm)> TrapezoidRing(SectionProfile.Trapezium t) {
            double topMin = (t.BottomWidthMm.Value - t.TopWidthMm.Value) / 2.0 + t.TopOffsetMm;
            double shift = -Math.Min(0.0, topMin);
            return Seq((shift, 0.0), (shift + t.BottomWidthMm.Value, 0.0),
                (shift + topMin + t.TopWidthMm.Value, t.DepthMm.Value), (shift + topMin, t.DepthMm.Value));
        }

        static Seq<(double YMm, double ZMm)> TriangleRing(SectionProfile.FilletTriangle f) =>
            Seq((0.0, 0.0), (f.LegMm.Value, 0.0), (0.0, f.Leg2Mm.Value));

        static Seq<Strip> RingStrips(Seq<(double YMm, double ZMm)> ring,
            Func<(double YMm, double ZMm), double> along, Func<(double YMm, double ZMm), double> across) {
            Seq<double> levels = toSeq(ring.Map(along).Distinct().OrderBy(static z => z));
            return toSeq(Enumerable.Range(0, Math.Max(0, levels.Count - 1)))
                .Map(i => BandOf(ring, along, across, levels[i], levels[i + 1]));
        }

        static Strip BandOf(Seq<(double YMm, double ZMm)> ring,
            Func<(double YMm, double ZMm), double> along, Func<(double YMm, double ZMm), double> across, double lo, double hi) {
            double span = hi - lo;
            double w1 = Measure(ring, along, across, lo + span / 3.0), w2 = Measure(ring, along, across, lo + 2.0 * span / 3.0);
            return new(lo, hi, 2.0 * w1 - w2, 2.0 * w2 - w1);
        }

        static double Measure(Seq<(double YMm, double ZMm)> ring,
            Func<(double YMm, double ZMm), double> along, Func<(double YMm, double ZMm), double> across, double at) {
            Seq<double> stations = toSeq(ring.Index()).Bind(v => {
                (double YMm, double ZMm) a = v.Item, b = ring[(v.Index + 1) % ring.Count];
                double a0 = along(a), a1 = along(b);
                return a0 == a1 || at < Math.Min(a0, a1) || at >= Math.Max(a0, a1)
                    ? Seq<double>()
                    : Seq(across(a) + (across(b) - across(a)) * (at - a0) / (a1 - a0));
            });
            Seq<double> sorted = toSeq(stations.OrderBy(static y => y));
            return toSeq(Enumerable.Range(0, sorted.Count / 2)).Sum(i => sorted[2 * i + 1] - sorted[2 * i]);
        }

        public static SectionSupplement ThinFold(SectionProfile.Corrugated d) {
            double rp = d.RibPitchMm.Value, g = d.GaugeMm.Value, tf = d.TopFlatMm.Value, bf = d.BottomFlatMm.Value, rd = d.RibDepthMm.Value;
            int ribs = Math.Max(1, (int)Math.Floor(d.CoverWidthMm.Value / rp));
            double web = Math.Sqrt(Math.Pow((rp - tf - bf) / 2.0, 2.0) + rd * rd);
            double chord = rd > 0.0 ? 2.0 * ribs * g * web / rd : 2.0 * ribs * g;
            return new(
                Plastic.Modulus(Seq(new Strip(0.0, g, ribs * bf, ribs * bf),
                    new Strip(0.0, rd, chord, chord),
                    new Strip(rd, rd + g, ribs * tf, ribs * tf))),
                Plastic.Modulus(Seq(new Strip(0.0, ribs * rp, g, g))),
                ribs * (tf + bf + 2.0 * web) * g * g * g / 3.0,
                ribs * 2.0 * web * g, ribs * (tf + bf) * g);
        }

        public static SectionSupplement FromCatalogue(SteelShape s) => s.Class.Switch(
            state: s.Section,
            iShape:      static x => OpenI(new SectionProfile.IShape(x.DepthMm, x.WidthMm, x.WebMm, x.FlangeMm, x.FilletMm, x.FlangeMm)),
            uShape:      static x => OpenChannel(new SectionProfile.Channel(x.DepthMm, x.WidthMm, x.WebMm, x.FlangeMm, x.FilletMm, x.FlangeMm)),
            lShape:      static x => OpenAngle(new SectionProfile.Angle(x.DepthMm, x.WidthMm, x.WebMm, x.FilletMm, x.WebMm)),
            doubleAngle: static x => Doubled(OpenAngle(new SectionProfile.Angle(x.DepthMm, x.WidthMm, x.WebMm, x.FilletMm, x.WebMm)),
                             x.BackToBackMm, (x.WidthMm.Value + x.DepthMm.Value) * x.WebMm.Value),
            hssRect:     static x => BoxTube(new SectionProfile.RectangleHollow(x.WidthMm, x.DepthMm, x.WebMm, x.FilletMm, x.FilletMm)),
            hssRound:    static x => RoundTube(x.DepthMm.Value, x.WebMm.Value),
            tee:         static x => MonoTee(new SectionProfile.Tee(x.DepthMm, x.WidthMm, x.WebMm, x.FlangeMm, x.FilletMm, x.FlangeMm, x.WebMm)),
            composite:   static x => OpenI(new SectionProfile.IShape(x.DepthMm, x.WidthMm, x.WebMm, x.FlangeMm, x.FilletMm, x.FlangeMm)),
            coldFormed:  static x => OpenChannel(new SectionProfile.Channel(x.DepthMm, x.WidthMm, x.WebMm, x.FlangeMm, x.FilletMm, x.FlangeMm)));

        static SectionSupplement Doubled(SectionSupplement one, double separationMm, double legAreaMm2) =>
            one with {
                Zx = 2.0 * one.Zx,
                Zy = 2.0 * (one.Zy + legAreaMm2 * separationMm / 2.0),
                J = 2.0 * one.J, Avy = 2.0 * one.Avy, Avz = 2.0 * one.Avz, Iw = 2.0 * one.Iw,
                ShearCentreY = 0.0,
            };

        static double RectJ(double a, double b) {
            double lng = Math.Max(a, b), sht = Math.Min(a, b);
            return lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));
        }
        static double ClosedRectJ(double b, double h, double t, double innerFilletMm, double outerFilletMm) {
            double bm = Math.Max(0.0, b - t), hm = Math.Max(0.0, h - t);
            double rm = Math.Clamp((innerFilletMm + outerFilletMm) / 2.0, 0.0, Math.Min(bm, hm) / 2.0);
            double area = bm * hm - (4.0 - Math.PI) * rm * rm;
            double perimeter = 2.0 * (bm + hm) - 8.0 * rm + 2.0 * Math.PI * rm;
            return bm <= 0.0 || hm <= 0.0 || perimeter <= 0.0 ? 0.0 : 4.0 * area * area * t / perimeter;
        }
        static SectionSupplement SolidRect(double w, double d) { double a = w * d; return new(w * d * d / 4.0, d * w * w / 4.0, RectJ(w, d), 2.0 * a / 3.0, 2.0 * a / 3.0); }
    }
}

// --- [KERNELS] -------------------------------------------------------------------------
internal readonly record struct WallSegment(int Parent, double Y0, double Z0, double Y1, double Z1, double Thickness);

internal readonly record struct OpenWall(
    double Area, double CentroidY, double CentroidZ, double Iy, double Iz,
    double ShearCentreY, double ShearCentreZ, double Warping, double Torsion);

internal static class ThinWalled {
    public static OpenWall Of(Seq<WallSegment> wall) {
        double area = wall.Sum(static s => s.Thickness * Length(s));
        double cy = wall.Sum(s => s.Thickness * Length(s) * Mid(s.Y0, s.Y1)) / area;
        double cz = wall.Sum(s => s.Thickness * Length(s) * Mid(s.Z0, s.Z1)) / area;
        double iy = wall.Sum(s => Quadratic(s, s.Z0 - cz, s.Z1 - cz));
        double iz = wall.Sum(s => Quadratic(s, s.Y0 - cy, s.Y1 - cy));
        Seq<(WallSegment Segment, double Start, double End)> swept = Sweep(wall, cy, cz);
        double mean = swept.Sum(x => x.Segment.Thickness * Length(x.Segment) * Mid(x.Start, x.End)) / area;
        double coupledY = swept.Sum(x => Coupled(x.Segment, x.Start - mean, x.End - mean, x.Segment.Z0 - cz, x.Segment.Z1 - cz));
        double coupledZ = swept.Sum(x => Coupled(x.Segment, x.Start - mean, x.End - mean, x.Segment.Y0 - cy, x.Segment.Y1 - cy));
        double sy = coupledY / Math.Max(Math.Abs(iy), EpsilonPolicy.ZeroTolerance) * Math.Sign(iy is 0.0 ? 1.0 : iy);
        double sz = -coupledZ / Math.Max(Math.Abs(iz), EpsilonPolicy.ZeroTolerance) * Math.Sign(iz is 0.0 ? 1.0 : iz);
        Seq<(WallSegment Segment, double Start, double End)> polar = Sweep(wall, cy + sy, cz + sz);
        double polarMean = polar.Sum(x => x.Segment.Thickness * Length(x.Segment) * Mid(x.Start, x.End)) / area;
        return new(area, cy, cz, iy, iz, sy, sz,
            polar.Sum(x => Quadratic(x.Segment, x.Start - polarMean, x.End - polarMean)),
            wall.Sum(static s => Length(s) * s.Thickness * s.Thickness * s.Thickness / 3.0));
    }

    static double Length(WallSegment s) => Math.Sqrt((s.Y1 - s.Y0) * (s.Y1 - s.Y0) + (s.Z1 - s.Z0) * (s.Z1 - s.Z0));
    static double Mid(double a, double b) => (a + b) / 2.0;

    static double Quadratic(WallSegment s, double a, double b) => s.Thickness * Length(s) * (a * a + a * b + b * b) / 3.0;

    static double Coupled(WallSegment s, double a0, double a1, double b0, double b1) =>
        s.Thickness * Length(s) * (2.0 * a0 * b0 + a0 * b1 + a1 * b0 + 2.0 * a1 * b1) / 6.0;

    static Seq<(WallSegment Segment, double Start, double End)> Sweep(Seq<WallSegment> wall, double poleY, double poleZ) =>
        wall.Fold(Seq<(WallSegment Segment, double Start, double End)>(), (rows, s) => {
            double start = s.Parent >= 0 && s.Parent < rows.Count ? rows[s.Parent].End : 0.0;
            double delta = (s.Y0 - poleY) * (s.Z1 - s.Z0) - (s.Z0 - poleZ) * (s.Y1 - s.Y0);
            return rows.Add((s, start, start + delta));
        });
}

internal readonly record struct Strip(double Lo, double Hi, double WidthLo, double WidthHi);

internal static class Plastic {
    public static double Modulus(Seq<Strip> strips) {
        double axis = EqualAreaLevel(strips, strips.Sum(Area) / 2.0);
        return strips.Sum(s => FirstMoment(s, axis));
    }

    public static double Modulus(Seq<(double Area, double Offset, double Extent, double OwnModulus)> members) {
        double axis = EqualAreaLevel(
            members.Map(static m => new Strip(m.Offset - m.Extent / 2.0, m.Offset + m.Extent / 2.0, m.Area / m.Extent, m.Area / m.Extent)),
            members.Sum(static m => m.Area) / 2.0);
        return members.Sum(m => Math.Abs(m.Offset - axis) >= m.Extent / 2.0
            ? m.Area * Math.Abs(m.Offset - axis)
            : m.OwnModulus + m.Area / m.Extent * Math.Pow(m.Offset - axis, 2.0));
    }

    static double Area(Strip s) => (s.WidthLo + s.WidthHi) / 2.0 * (s.Hi - s.Lo);

    static double EqualAreaLevel(Seq<Strip> strips, double half) {
        Seq<double> bounds = toSeq(strips.Bind(static s => Seq(s.Lo, s.Hi)).Distinct().OrderBy(static t => t));
        return toSeq(Enumerable.Range(0, Math.Max(0, bounds.Count - 1)))
            .Fold((Below: 0.0, Level: bounds.IsEmpty ? 0.0 : bounds[0], Done: false), (acc, i) => {
                if (acc.Done) { return acc; }
                (double lo, double hi) = (bounds[i], bounds[i + 1]);
                double band = strips.Sum(s => Area(Clip(s, lo, hi)));
                return acc.Below + band >= half
                    ? (acc.Below, Station(strips, lo, hi, half - acc.Below), true)
                    : (acc.Below + band, hi, false);
            }).Level;
    }

    static double Station(Seq<Strip> strips, double lo, double hi, double wanted) {
        double w0 = strips.Sum(s => Width(Clip(s, lo, hi), lo));
        double w1 = strips.Sum(s => Width(Clip(s, lo, hi), hi));
        double span = hi - lo, slope = span > 0.0 ? (w1 - w0) / span : 0.0;
        return Math.Abs(slope) <= double.Epsilon
            ? lo + wanted / Math.Max(w0, double.Epsilon)
            : lo + (Math.Sqrt(Math.Max(w0 * w0 + 2.0 * slope * wanted, 0.0)) - w0) / slope;
    }

    static double FirstMoment(Strip s, double axis) =>
        axis <= s.Lo || axis >= s.Hi
            ? Math.Abs(Signed(s, axis))
            : Math.Abs(Signed(Clip(s, s.Lo, axis), axis)) + Math.Abs(Signed(Clip(s, axis, s.Hi), axis));

    static double Signed(Strip s, double axis) {
        double span = s.Hi - s.Lo;
        if (span <= 0.0) { return 0.0; }
        double slope = (s.WidthHi - s.WidthLo) / span, a = s.Lo - axis, b = s.Hi - axis;
        return s.WidthLo * (b * b - a * a) / 2.0 + slope * ((b * b * b - a * a * a) / 3.0 - a * (b * b - a * a) / 2.0);
    }

    static Strip Clip(Strip s, double lo, double hi) {
        double a = Math.Max(s.Lo, lo), b = Math.Min(s.Hi, hi);
        return b <= a ? new(a, a, 0.0, 0.0) : new(a, b, Width(s, a), Width(s, b));
    }

    static double Width(Strip s, double at) =>
        s.Hi - s.Lo <= 0.0 ? s.WidthLo : s.WidthLo + (s.WidthHi - s.WidthLo) * (at - s.Lo) / (s.Hi - s.Lo);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
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
```

## [05]-[COMPUTED_SECTION]

The FROZEN twenty-column `ComputedSection` (`[FROZEN_INVARIANTS]`): field names, types, and order are byte-identical wire law — `Projection/component#COMPONENT_PROJECTOR` `ContractSection` lifts the whole set onto the contract `SectionProperties` (mm→SI typed `MeasureValue`s through `QuantityRow`, `Iw` fifth after `J`, the three asymmetry columns last), so a `Rasm.Compute` structural/fire runner reads `graph.SectionOf(member)` without re-resolving or admitting VividOrange. The elastic columns and `HeatedPerimeterMm` come from the ONE polygon integral; `ZxMm3`/`ZyMm3`/`JMm4`/`AvyMm2`/`AvzMm2` from the `Forms` supplement; `IwMm6` (EN 1993-1-1 §6.3.2 lateral-torsional-buckling input) is positive ONLY for an open thin-walled arm; `ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor` (the EN 1993-1-1 NCCI SN030 general-LTB inputs) are signed zero-valid plain doubles — engineering-zero for every doubly-symmetric arm, non-zero for a channel/tee/angle/mono-I, so the contract `IsDoublySymmetric` reads zero-as-symmetric EXACTLY. `AxisDistanceMm` is the EN 1992-1-2 cover-to-reinforcement, zero for every non-RC section — the RC value rides the `reinforcement#RC_SECTION` `ConcreteSectionProperties` path.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
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
    PositiveMagnitude AvyMm2,
    PositiveMagnitude AvzMm2,
    PositiveMagnitude DepthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeatedPerimeterMm,
    double AxisDistanceMm,
    double ShearCentreYMm,
    double ShearCentreZMm,
    double MonosymmetryFactor) {

    public double GoverningRadiusMm => Math.Min(RxMm.Value, RyMm.Value);
}
```

## [06]-[CATALOGUE]

- Owner: `ComponentRow` the campaign row currency (`[ROW_CURRENCIES]`) carrying its one contract `EvidenceGrade` column and its derived section-map membership; `SeedJoin` the result-typed designation-keyed table join every seed page composes; `TypeCandidate` the contract-declared ingested-type row the reverse fold consumes; `ComponentCatalogue` the ONE fold computing BOTH frozen maps in one pass, and the `AdmitImported` reverse admission beside it.
- Cases: `Sectioned` is DERIVED from the profile's own `ProfileTopology`, so section-map membership is decided by the one arm that knows whether a section integral exists and a seed row cannot assert a membership the geometry contradicts; an unsectioned laminate and a bond line answer `None` at `graph.SectionOf` because they carry no outline, and every solvable arm answers `Some`. `Source` is the ONE origin axis: the contract `EvidenceGrade` already states where a row's values came from, so `AdmitImported` stamps `Import` and imported rows sit BESIDE seeds in ONE `ComponentId` space — a parallel imported catalogue forking `Lookup`, `ComponentResolution.Build`, and every `graph.SectionOf` read is unrepresentable.
- Boundary: `ComponentCatalogue.Of` is the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CatalogueAdmit(Row)` VETO subject — a composition-root decorator on the folder hook set at `MaterialsPoint.CatalogueAdmit` may transform or refuse each row pre-freeze, so this owner emits nothing and the frozen maps see only admitted rows. `TypeCandidate` is DECLARED ONCE at the contract — `Rasm.Element/Projection/projection#PROJECTION_CONTRACT`, the lowest stratum both non-referencing peers reach — and this fold composes that declaration under the `[PORT]: IIfcTypeReconciler` contract alignment; the producer is `Rasm.Bim` `Projection/foreign#REINGEST` `Reingest.ExportTypeCandidates`, and Materials still references no `Rasm.Bim`. That port's forward leg reconciles ingested types AGAINST Materials ids; `AdmitImported` is its reverse leg, minting rows FROM the types the forward leg left unreconciled, so the two directions close one loop rather than opening a second surface.
- Entry: `ComponentCatalogue.Of(context)` folds `ComponentFamily.Items`; `Traverse` is the result (a seed row `Component.Of` rejects, a `Sectioned` row `SectionSolver.Solve` cannot solve, or a cross-seed `ComponentId` collision, ABORTS the build TYPED — never a `Choose`/`ToOption` swallow and never the raw frozen-map duplicate-key throw); `Lookup` preserved. Composition-root code binds `Of`'s `Fin` ONCE and passes `catalogue.Rows`/`catalogue.Sections` into `ComponentResolution.Build`. `ComponentCatalogue.AdmitImported(candidates, context)` is the ONE reverse fold: it re-reads the seeded rows through the same `ComponentFamily.Items` traverse, elects each candidate's family by the REVERSE read of its `IfcBinding` pair, resolves the section from the seeded row the candidate's `ProfileDesignation` publishes, and lowers the result through the SAME result-typed `Component.Of` — an unclaimed pair, a pair two families both claim, an unpublished section designation, or a declared profile standard disagreeing with the matched row's authority each SKIP typed, while a `Component.Of` rejection or a within-batch designation collision ABORTS the whole admission under the fail-loud CATALOGUE law.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
public static class SeedJoin {
    public static Lazy<Fin<FrozenDictionary<ComponentId, TRow>>> Of<TRow>(Seq<TRow> rows, Func<TRow, string> designation) where TRow : struct =>
        new(() => rows
            .Traverse(row => Admit(designation(row)).Map(id => (Id: id, Row: row))).As()
            .Bind(keyed =>
                let collisions = keyed.Count - keyed.Map(static k => k.Id).Distinct().Count
                from unique in guard(collisions == 0, new ComponentFault.CatalogueConflict(Table, collisions))
                select keyed.ToFrozenDictionary(static k => k.Id, static k => k.Row)));

    public static Fin<TRow> Resolve<TRow>(Lazy<Fin<FrozenDictionary<ComponentId, TRow>>> table, ComponentId id) where TRow : struct =>
        table.Value.Bind(map => map.TryGetValue(id, out TRow row)
            ? Fin.Succ(row)
            : Fin.Fail<TRow>(new ComponentFault.ComponentMissing(ProfileRef.Of(id.Value))));

    static Fin<ComponentId> Admit(string designation) =>
        FactoryBridge.Accept<ComponentId>(designation);
}

public readonly record struct ComponentRow(Component Item, EvidenceGrade Source) {
    public bool Sectioned => Item.Profile.Topology.Solvable;
}

public sealed record ComponentCatalogue(
    FrozenDictionary<ComponentId, Component> Rows,
    FrozenDictionary<ComponentId, ComputedSection> Sections) {

    public static Fin<ComponentCatalogue> Of(Context context) =>
        toSeq(ComponentFamily.Items)
            .Traverse(family => family.Rows(context)).As()
            .Map(static perFamily => perFamily.Bind(static rows => rows))
            .Bind(rows =>
                let collisions = rows.Count - rows.Map(static r => r.Item.Designation).Distinct().Count
                from keyed in guard(collisions == 0, new ComponentFault.CatalogueConflict(context.Key, collisions))
                from catalogue in rows.Filter(static r => r.Sectioned)
                    .Traverse(r => SectionSolver.Solve(r.Item.Profile)
                        .Map(section => (r.Item.Designation, Section: section))).As()
                    .Map(solved => new ComponentCatalogue(
                        rows.ToFrozenDictionary(static r => r.Item.Designation, static r => r.Item),
                        solved.ToFrozenDictionary(static s => s.Designation, static s => s.Section)))
                select catalogue);

    public static Fin<Component> Lookup(FrozenDictionary<ComponentId, Component> rows, ComponentId id) =>
        rows.TryGetValue(id, out Component? row) && row is { } r
            ? Fin.Succ(r)
            : new ComponentFault.ComponentMissing(ProfileRef.Of(id.Value));

    // --- [IMPORT_ADMISSION]
    public static Fin<Seq<ComponentRow>> AdmitImported(Seq<TypeCandidate> candidates, Context context) =>
        from seeded in toSeq(ComponentFamily.Items)
            .Traverse(family => family.Rows(context).Map(rows => (Family: family, Rows: rows))).As()
        let claims = Claims(seeded)
        let catalogued = Catalogued(seeded)
        from elected in candidates.Traverse(candidate => Imported(candidate, claims, catalogued)).As()
        let rows = elected.Somes()
        let collisions = rows.Count - rows.Map(static r => r.Item.Designation).Distinct().Count
        from keyed in guard(collisions == 0, new ComponentFault.CatalogueConflict(collisions))
        select rows;

    static Fin<Option<ComponentRow>> Imported(
        TypeCandidate candidate,
        HashMap<IfcBinding, Seq<ComponentFamily>> claims,
        HashMap<(string Family, string Section), ComponentRow> catalogued) {
        IfcBinding pair = IfcBinding.Of(candidate.IfcEntity, candidate.PredefinedToken);
        return (from family in Claimant(claims, pair)
                from seed in catalogued.Find((family.Key, SectionKey(candidate.ProfileDesignation.IfNone(""))))
                from standing in Standing(candidate, seed)
                from attributable in Attributable(seed)
                select (Family: family, Seed: seed))
            .TraverseM(election => Component.Of(
                        election.Family,
                        $"{election.Family.Key}.{Tail(candidate)}",
                        election.Seed.Item.Profile,
                        pair,
                        election.Seed.Item.Coring,
                        election.Seed.Item.Standard,
                        election.Seed.Item.SubstanceId,
                        election.Seed.Item.AppearanceId,
                        Detail(election.Family, candidate))
                    .Map(item => new ComponentRow(item, EvidenceGrade.Import)))
            .As();
    }

    static HashMap<IfcBinding, Seq<ComponentFamily>> Claims(Seq<(ComponentFamily Family, Seq<ComponentRow> Rows)> seeded) =>
        seeded.Fold(HashMap<IfcBinding, Seq<ComponentFamily>>(), static (index, seed) =>
            seed.Rows.Fold(index, (held, row) =>
                held.AddOrUpdate(row.Item.Ifc, held.Find(row.Item.Ifc).IfNone(Seq<ComponentFamily>()).Add(seed.Family))));

    static Option<ComponentFamily> Claimant(HashMap<IfcBinding, Seq<ComponentFamily>> claims, IfcBinding pair) =>
        claims.Find(pair).Bind(static families => families.Distinct() is { Count: 1 } sole ? sole.Head : None);

    static HashMap<(string Family, string Section), ComponentRow> Catalogued(Seq<(ComponentFamily Family, Seq<ComponentRow> Rows)> seeded) =>
        seeded.Fold(HashMap<(string, string), ComponentRow>(), static (index, seed) =>
            seed.Rows.Fold(index, (held, row) => held.AddOrUpdate((seed.Family.Key, SectionKey(row.Item.Designation.Value)), row)));

    static string SectionKey(string designation) =>
        designation.LastIndexOf('.') is var dot && dot >= 0 ? designation[(dot + 1)..] : designation;

    static Option<Unit> Attributable(ComponentRow seed) => seed.Source.Attributable ? Some(unit) : None;

    static Option<Unit> Standing(TypeCandidate candidate, ComponentRow seed) =>
        candidate.ProfileStandard.Filter(static declared => !string.IsNullOrWhiteSpace(declared)).Match(
            Some: declared => string.Equals(declared, seed.Item.Standard.Authority.Key, StringComparison.Ordinal) ? Some(unit) : None,
            None: static () => Some(unit));

    static string Tail(TypeCandidate candidate) =>
        Sanitized(candidate.Name) is { Length: > 0 } named ? named : Sanitized(candidate.GlobalId);

    static string Sanitized(string source) =>
        string.Concat(source.Select(static c => char.IsAsciiLetterOrDigit(c) ? c : '-')).Trim('-');

    static Option<PropertyBag> Detail(ComponentFamily family, TypeCandidate candidate) => family.Lane switch {
        DetailLane.Realization => Some(ComponentDetail.RealizationRows(DetailRows(candidate))),
        DetailLane.Product     => Some(ComponentDetail.ProductRows(DetailRows(candidate))),
        _                      => Option<PropertyBag>.None,
    };

    static (PropertyName Name, PropertyValue Value)[] DetailRows(TypeCandidate candidate) =>
        (candidate.MaterialName
            .Map(static name => (Name: PropertyCategory.Materials.Row(nameof(TypeCandidate.MaterialName)), Value: (PropertyValue)new PropertyValue.Text(name)))
            .ToSeq()
         + toSeq(candidate.Properties.AsIterable().Map(static row => (Name: row.Key, Value: row.Value))))
        .ToArray();
}
```

## [07]-[QUANTITY_ROW]

The ONE bounded typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose — a page-local `(QuantityType, Dimension, unit)` triple at a mint site is the fork this owner closes. Every `QuantityType` spelling and `Dimension` vector below is frozen wire law (`[FROZEN_INVARIANTS]`); each `Scale` states its row's own NATIVE catalog basis, so a millimetre-sourced column and an SI-native one both reach the contract in SI. BOUNDARY: detail-bag rows keep the DIMENSION-ONLY `MeasureValue.OfSi(dim, si)` overload so an authored and an imported bag content-key identically — `QuantityRow` owns TYPED mints only.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
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
    public static readonly QuantityRow ChlorideDiffusivity     = new("ChlorideDiffusivity",     QuantityType.Create("ChlorideDiffusivity"),     Dimension.Create(2, 0, -1, 0, 0, 0, 0),  1.0,   "m2/s");


    public static readonly QuantityRow VolumePerLength      = new("VolumePerLength",      QuantityType.Create("VolumePerLength"),      Dimension.AreaDim,                      1e-6, "m2");
    public static readonly QuantityRow SurfaceAreaPerLength = new("SurfaceAreaPerLength", QuantityType.Create("SurfaceAreaPerLength"), Dimension.LengthDim,                    1e-3, "m");
    public static readonly QuantityRow LinearDensity        = new("LinearDensity",        QuantityType.Create("LinearDensity"),        Dimension.Create(-1, 1, 0, 0, 0, 0, 0), 1e-6, "kg/m");

    public QuantityType Type { get; }
    public Dimension Dim { get; }
    public double Scale { get; }
    public string Unit { get; }
    public Fin<MeasureValue> OfNative(double native) => MeasureValue.OfSi(Type, Dim, native * Scale);
}
```

## [08]-[COMPONENT_DETAIL]

The seed-time bag constructors: each `Realization`/`Product`-lane seed page builds its family's bag AT SEED TIME (`MasonryDetail.Of`, `GlazingDetail.Of`, `FastenerDetail.Of`, `PanelDetail.Of` and siblings compose this owner), so a bag is built where its values are known and the projector reads one already-conforming set. `ProjectType`'s read becomes `c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), bag), tolerance))`. The `Measured` SI value carries the DIMENSION-only mint (the overload Bim uses) so an authored and an imported row content-key identically; `Joint` routes the `PropertyValue.Enumerated` through the schema's CLOSED allowed set, never a local re-spelling.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ComponentDetail {
    public static Fin<(PropertyName, PropertyValue)> Joint(string kind) =>
        DetailSchema.Realization.Joint(kind).Map(static value => (DetailSchema.JointType, value));

    public static (PropertyName, PropertyValue) Sourced(EvidenceGrade source) =>
        (PropertyCategory.Materials.Row(nameof(EvidenceGrade)), new PropertyValue.Text(source.Token));

    public static (PropertyName, PropertyValue) Token(PropertyName name, string value) => (name, new PropertyValue.Text(value));
    public static Fin<(PropertyName, PropertyValue)> Measured(PropertyName name, Dimension dim, double si) =>
        MeasureValue.OfSi(dim, si).Map(value => (name, (PropertyValue)new PropertyValue.Measure(value)));

    public static PropertyBag RealizationRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        toSeq(rows).Fold(DetailSchema.Realization.Bag(), static (bag, r) => bag.With(r.Name, r.Value));

    public static PropertyBag ProductRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        toSeq(rows).Fold(DetailSchema.Product.Bag(), static (bag, r) => bag.With(r.Name, r.Value));

    public static Option<PropertyBag> Bag(DetailLane lane, params (PropertyName Name, PropertyValue Value)[] rows) =>
        lane == DetailLane.None ? Option<PropertyBag>.None
            : Some(lane == DetailLane.Product ? ProductRows(rows) : RealizationRows(rows));
}
```

## [09]-[COMPONENT_RESOLUTION]

- Owner: `ResolvedComponent` the one-hop `(Component, Option<ComputedSection>)` pair; `ComponentResolution` the contract-`ProfileRef` resolver and frozen cache — BYTE-IDENTICAL (`[FROZEN_INVARIANTS]`): only catalogue CONSTRUCTION went `Fin` (`ComponentCatalogue.Of`), bound once at composition; `graph.SectionOf` and every downstream signature are untouched.
- Cases: a `ProfileRef` keys exactly one `ResolvedComponent`; a component present in BOTH maps joins `Some(section)`, one present only in the row map joins `None` — the contract-honest absence (a `PositiveMagnitude` rejects zero, so a forged all-zero `ComputedSection` is unrepresentable), total over every registered ref. The `Option` mirrors the contract `MaterialComposition.ProfileSet` `Option<SectionProperties>` the projector bakes.
- Entry: `Build(rows, sections)` is the total build-time join; `Resolve(reference, table, key)` aborts an unregistered reference on `ComponentMissing`, distinct from an earlier section-integral refusal.
- Boundary: the resolver owns NO section math and NO contract type — the section is DATA captured at the catalogue-build site that owns the geometry, never a `Func<Component, Op, Fin<ComputedSection>>` re-invoked at resolution. `ProfileRef` stays contract-canonical.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ResolvedComponent(Component Component, Option<ComputedSection> Section);

// --- [TABLES] --------------------------------------------------------------------------
public static class ComponentResolution {
    public static FrozenDictionary<ProfileRef, ResolvedComponent> Build(FrozenDictionary<ComponentId, Component> rows, FrozenDictionary<ComponentId, ComputedSection> sections) =>
        rows.ToFrozenDictionary(
            static kv => ProfileRef.Of(kv.Key.Value),
            kv => new ResolvedComponent(kv.Value, sections.TryGetValue(kv.Key, out ComputedSection s) ? Some(s) : Option<ComputedSection>.None));

    public static Fin<ResolvedComponent> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedComponent> table) =>
        table.TryGetValue(reference, out ResolvedComponent resolved)
            ? Fin.Succ(resolved)
            : new ComponentFault.ComponentMissing(reference);
}
```

## [10]-[MATERIAL_GRADE]

- Owner: `MaterialGrade` the ONE registered-grade identity over every `ComponentFamily` — the six per-family grade owners (`SteelGrade`, the fastener `GradeRow`+`Grades`, the reinforcement `RebarGradeRow`+`Grades`+`Strands`, `AluminumGrade`, `TimberGrade`+`TimberGrades`, `ConcreteGradeRow`+`ConcreteGrades`, the cmu `CmuStrength` class rows) collapse to rows on this one `[SmartEnum<string>]`; `GradeProperties` the CLOSED per-family payload `[Union]` whose arm carries each family's real columns under its own names. Authority is ONE typed `ComponentAuthority` column — the four prior spellings (a `ComponentStandard` value, the `RebarStandard.Authority` free text, a bare `ComponentAuthority`, the `SteelBody` enum) were one fact spelled four ways.
- Cases: `GradeProperties.{Steel · Rebar · Strand · Fastener · Aluminum · Timber · Concrete · Cmu}` — a closed family union, NOT `Option`-typed columns and NOT a generic `MaterialGrade<TColumns>`: a flat row erases totality (a family reads a column another family owns as `None`) and a type parameter loses the closed family and the total `Switch`. Cylinder-vs-cube stays two named Concrete columns; the timber clause-bound axes stay NAMED columns `OrthotropicLaw()` reads positionally; `FmEdgeK` is NOT a grade column — a product/layup fact, seated on the timber form row (E-M13).
- Entry: rows are the registered vocabulary (`MaterialGrade.A992`, `MaterialGrade.Gr60`, `MaterialGrade.C24`, `MaterialGrade.C30_37` — symbolic access preserved from the retired per-family rosters); family physics reads `grade.Columns` and pattern-matches or `Switch`es its own arm. Family-specific derivations (`Steel.YieldMpa` thickness-banded EN derivation, `Fastener.At`/`Admits`, `Aluminum.Strengths`, `Timber.OrthotropicLaw`) are PARTIAL members landed on the owning family page beside the vocabulary they compose — the arm's columns and identity live HERE, its physics stays co-located with its family (ownership co-location, never a mechanical split).
- Growth: a new grade is ONE row plus (at most) one new column on its family's arm; a new graded family is one `GradeProperties` arm — the generated total `Switch` breaks every reader loudly; a new authority is one `ComponentAuthority` row. The M1 `Grades` CS0101 (`fastener.md` vs `reinforcement.md`, both `public static class Grades` in one namespace) dissolves here — both static classes become row blocks on this owner.
- Boundary: NAMED LOSS — a family's property set is now reached through one `Switch` arm instead of a direct member on a family-typed roster; per-family symbolic rows survive as `MaterialGrade` statics, so no call site loses its name. WITNESS — the steel design read `SteelGrade.A992.NominalYieldMpa` rebuilds as `MaterialGrade.A992.Columns.Switch(steel: static s => s.NominalYieldMpa, …)` (or the arm pattern-match where one family is already proven). A grade's substance id stays the design-contract `MaterialId` key; `Appearance` is `Option` because most families ride the seed's two-slot law and only the coated rosters (rebar, fastener) publish a per-grade appearance.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record GradeProperties {
    private GradeProperties() { }

    public sealed partial record Steel(double NominalYieldMpa, Option<string> EnDesignation, Option<StainlessRow> Stainless) : GradeProperties;

    public sealed partial record Rebar(Option<double> YieldMpa, RebarStandard Standard, Option<EnRebarGrade> En) : GradeProperties;

    public sealed partial record Strand(double DiameterMm, double AreaMm2, double UltimateMpa, double YieldRatio, RelaxationClass Relaxation) : GradeProperties;

    public sealed partial record Fastener(
        SizeBand Sizes, Option<double> ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa,
        double SpecifiedUltimateMpa, Option<double> EurocodeAlphaV, bool Preloadable, Option<GradeStep> Step = default) : GradeProperties;

    public sealed partial record Aluminum(BucklingClass Class, Seq<AlloyBand> Bands, Option<HazRow> Haz) : GradeProperties;

    public sealed partial record Timber(
        double Fmk, double Ft0k, double Fc0k, double Fc90k, double Fvk, Option<double> FRvk,
        double E0Mean, double E005, double E90Mean, double GMean, double DensityK, double K90Base) : GradeProperties;

    public sealed partial record Concrete(EnConcreteGrade En, double FckMpa, double FckCubeMpa, string MixToken) : GradeProperties;

    public sealed partial record Cmu(double FmMpa, double NetUnitMsMpa, Option<double> NetUnitNMpa) : GradeProperties;
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialGrade {
    // --- [STEEL]
    public static readonly MaterialGrade A36  = Steel("a36",  ComponentAuthority.Astm, 250.0, "steel.a36");
    public static readonly MaterialGrade A992 = Steel("a992", ComponentAuthority.Astm, 345.0, "steel.a992");
    public static readonly MaterialGrade A572 = Steel("a572", ComponentAuthority.Astm, 345.0, "steel.a572");
    public static readonly MaterialGrade A653Gr33 = Steel("a653-gr33", ComponentAuthority.Astm, 230.0, "steel.g33");
    public static readonly MaterialGrade A653Gr50 = Steel("a653-gr50", ComponentAuthority.Astm, 340.0, "steel.g50");
    public static readonly MaterialGrade A500Rect  = Steel("a500-grc-rect",  ComponentAuthority.Astm, 345.0, "steel.a500-rect");
    public static readonly MaterialGrade A500Round = Steel("a500-grc-round", ComponentAuthority.Astm, 317.0, "steel.a500-round");
    public static readonly MaterialGrade A53  = Steel("a53-grb", ComponentAuthority.Astm, 240.0, "steel.a53");
    public static readonly MaterialGrade S235 = Steel("s235", ComponentAuthority.En, 235.0, "steel.s235", enDesignation: Some("S235"));
    public static readonly MaterialGrade S275 = Steel("s275", ComponentAuthority.En, 275.0, "steel.s275", enDesignation: Some("S275"));
    public static readonly MaterialGrade S355 = Steel("s355", ComponentAuthority.En, 355.0, "steel.s355", enDesignation: Some("S355"));
    public static readonly MaterialGrade S420 = Steel("s420", ComponentAuthority.En, 420.0, "steel.s420", enDesignation: Some("S420N"));
    public static readonly MaterialGrade S450 = Steel("s450", ComponentAuthority.En, 440.0, "steel.s450", enDesignation: Some("S450"));
    public static readonly MaterialGrade S460 = Steel("s460", ComponentAuthority.En, 460.0, "steel.s460", enDesignation: Some("S460N"));
    public static readonly MaterialGrade Ss14301 = Steel("ss1-4301", ComponentAuthority.En, 210.0, "steel.1.4301", stainless: Some(StainlessBands.S14301));
    public static readonly MaterialGrade Ss14307 = Steel("ss1-4307", ComponentAuthority.En, 175.0, "steel.1.4307", stainless: Some(StainlessBands.S14307));
    public static readonly MaterialGrade Ss14401 = Steel("ss1-4401", ComponentAuthority.En, 220.0, "steel.1.4401", stainless: Some(StainlessBands.S14401));
    public static readonly MaterialGrade Ss14404 = Steel("ss1-4404", ComponentAuthority.En, 200.0, "steel.1.4404", stainless: Some(StainlessBands.S14404));
    public static readonly MaterialGrade Ss14462 = Steel("ss1-4462", ComponentAuthority.En, 450.0, "steel.1.4462", stainless: Some(StainlessBands.S14462));

    // --- [FASTENER]
    public static readonly MaterialGrade G46   = Iso("4.6",  IsoRange, 225.0,  400.0,  240.0, Some(0.60), false);
    public static readonly MaterialGrade G48   = Iso("4.8",  IsoRange, 310.0,  420.0,  340.0, Some(0.50), false);
    public static readonly MaterialGrade G56   = Iso("5.6",  IsoRange, 280.0,  500.0,  300.0, Some(0.60), false);
    public static readonly MaterialGrade G58   = Iso("5.8",  IsoRange, 380.0,  520.0,  420.0, Some(0.50), false);
    public static readonly MaterialGrade G68   = Iso("6.8",  IsoRange, 440.0,  600.0,  480.0, Some(0.50), false);
    public static readonly MaterialGrade G88   = Iso("8.8",  IsoRange, 580.0,  800.0,  640.0, Some(0.60), true, Some(new GradeStep(16.0, 600.0, 830.0, 660.0)));
    public static readonly MaterialGrade G98   = Iso("9.8",  new SizeBand(1.6, 16.0), 650.0, 900.0, 720.0, None, false);
    public static readonly MaterialGrade G109  = Iso("10.9", IsoRange, 830.0, 1040.0,  940.0, Some(0.50), true);
    public static readonly MaterialGrade G129  = Iso("12.9", IsoRange, 970.0, 1220.0, 1100.0, None, false);
    public static readonly MaterialGrade Gr1   = Us("gr1",   ComponentAuthority.Sae,  SaeRange,      Some(33.0),  60.0,  36.0, false);
    public static readonly MaterialGrade Gr2   = Us("gr2",   ComponentAuthority.Sae,  SaeRange,      Some(55.0),  74.0,  57.0, false, Some((0.75, 33.0, 60.0, 36.0)));
    public static readonly MaterialGrade Gr5   = Us("gr5",   ComponentAuthority.Sae,  SaeRange,      Some(85.0), 120.0,  92.0, false, Some((1.00, 74.0, 105.0, 81.0)));
    public static readonly MaterialGrade Gr52  = Us("gr5.2", ComponentAuthority.Sae,  SaeCapRange,   Some(85.0), 120.0,  92.0, false);
    public static readonly MaterialGrade Gr8   = Us("gr8",   ComponentAuthority.Sae,  SaeRange,     Some(120.0), 150.0, 130.0, false);
    public static readonly MaterialGrade Gr82  = Us("gr8.2", ComponentAuthority.Sae,  SaeCapRange,  Some(120.0), 150.0, 130.0, false);
    public static readonly MaterialGrade A325  = Us("a325",  ComponentAuthority.Astm, F3125Range,          None, 120.0,  92.0, true);
    public static readonly MaterialGrade F1852 = Us("f1852", ComponentAuthority.Astm, TwistOffRange,       None, 120.0,  92.0, true);
    public static readonly MaterialGrade A490  = Us("a490",  ComponentAuthority.Astm, F3125Range,          None, 150.0, 130.0, true);
    public static readonly MaterialGrade F2280 = Us("f2280", ComponentAuthority.Astm, TwistOffRange,       None, 150.0, 130.0, true);
    public static readonly MaterialGrade F155436  = Us("f1554-36",  ComponentAuthority.Astm, F1554Range,      None,  58.0,  36.0, false);
    public static readonly MaterialGrade F155455  = Us("f1554-55",  ComponentAuthority.Astm, F1554Range,      None,  75.0,  55.0, false);
    public static readonly MaterialGrade F1554105 = Us("f1554-105", ComponentAuthority.Astm, F1554HighRange,  None, 125.0, 105.0, false);

    // --- [REBAR]
    public static readonly MaterialGrade Gr40   = Rebar("gr40",  ComponentAuthority.Astm, Some(280.0), RebarStandard.A615,    "steel.gr40",  "metal.iron");
    public static readonly MaterialGrade Gr60   = Rebar("gr60",  ComponentAuthority.Astm, Some(420.0), RebarStandard.A615,    "steel.gr60",  "metal.iron");
    public static readonly MaterialGrade Gr75   = Rebar("gr75",  ComponentAuthority.Astm, Some(520.0), RebarStandard.A615,    "steel.gr75",  "metal.iron");
    public static readonly MaterialGrade Gr80   = Rebar("gr80",  ComponentAuthority.Astm, Some(550.0), RebarStandard.A615,    "steel.gr80",  "metal.iron");
    public static readonly MaterialGrade Gr60W  = Rebar("gr60w", ComponentAuthority.Astm, Some(420.0), RebarStandard.A706,    "steel.gr60w", "metal.steel");
    public static readonly MaterialGrade Gr80W  = Rebar("gr80w", ComponentAuthority.Astm, Some(550.0), RebarStandard.A706,    "steel.gr80w", "metal.steel");
    public static readonly MaterialGrade Gr400W = Rebar("400w",  ComponentAuthority.Csa,  Some(400.0), RebarStandard.G30,     "steel.400w",  "metal.steel");
    public static readonly MaterialGrade Gr500W = Rebar("500w",  ComponentAuthority.Csa,  Some(500.0), RebarStandard.G30,     "steel.500w",  "metal.steel");
    public static readonly MaterialGrade B500A  = Rebar("b500a", ComponentAuthority.En,   None,        RebarStandard.En10080, "steel.b500a", "metal.steel", Some(EnRebarGrade.B500A));
    public static readonly MaterialGrade B500B  = Rebar("b500b", ComponentAuthority.En,   None,        RebarStandard.En10080, "steel.b500b", "metal.steel", Some(EnRebarGrade.B500B));
    public static readonly MaterialGrade B500C  = Rebar("b500c", ComponentAuthority.En,   None,        RebarStandard.En10080, "steel.b500c", "metal.steel", Some(EnRebarGrade.B500C));

    // --- [STRAND]
    public static readonly MaterialGrade S13Gr1725   = Strand("strand-13-gr250",    ComponentAuthority.Astm, 12.70, 92.9,  1725.0, 0.90, "steel.strand-1725");
    public static readonly MaterialGrade S13Gr1860   = Strand("strand-13-gr270",    ComponentAuthority.Astm, 12.70, 98.7,  1860.0, 0.90, "steel.strand-1860");
    public static readonly MaterialGrade S15Gr1860   = Strand("strand-15-gr270",    ComponentAuthority.Astm, 15.24, 140.0, 1860.0, 0.90, "steel.strand-1860");
    public static readonly MaterialGrade Y1860S7D125 = Strand("strand-y1860s7-125", ComponentAuthority.En,   12.50, 93.0,  1860.0, 0.88, "steel.y1860s7");
    public static readonly MaterialGrade Y1860S7D157 = Strand("strand-y1860s7-157", ComponentAuthority.En,   15.70, 150.0, 1860.0, 0.88, "steel.y1860s7");

    // --- [ALUMINUM]
    public static readonly MaterialGrade A6061T6 = Aluminum("6061-t6", BucklingClass.A, "aluminium.6061t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 20.0, 240.0, 260.0)),
        haz: Option<HazRow>.None);
    public static readonly MaterialGrade A6063T5 = Aluminum("6063-t5", BucklingClass.B, "aluminium.6063t5",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 3.0, 130.0, 175.0),
            new AlloyBand(Seq(ExtrusionForm.Profile), 3.0, 25.0, 110.0, 160.0)),
        haz: Option<HazRow>.None);
    public static readonly MaterialGrade A6063T6 = Aluminum("6063-t6", BucklingClass.A, "aluminium.6063t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 25.0, 160.0, 195.0)),
        haz: Option<HazRow>.None);
    public static readonly MaterialGrade A6082T6 = Aluminum("6082-t6", BucklingClass.A, "aluminium.6082t6",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile), 0.0, 5.0, 250.0, 290.0),
            new AlloyBand(Seq(ExtrusionForm.Tube), 5.0, 15.0, 260.0, 310.0),
            new AlloyBand(Seq(ExtrusionForm.Rod), 0.0, 20.0, 250.0, 295.0)),
        haz: Option<HazRow>.None);
    public static readonly MaterialGrade A5083 = Aluminum("5083", BucklingClass.B, "aluminium.5083",
        Seq(new AlloyBand(Seq(ExtrusionForm.Profile, ExtrusionForm.Tube, ExtrusionForm.Rod), 0.0, 200.0, 110.0, 270.0),
            new AlloyBand(Seq(ExtrusionForm.Sheet), 0.0, 50.0, 125.0, 275.0),
            new AlloyBand(Seq(ExtrusionForm.Plate), 50.0, 80.0, 115.0, 270.0)),
        haz: Some(new HazRow(1.0, 1.0)));

    // --- [TIMBER]
    public static readonly MaterialGrade C14    = Timber("c14",    14,   7.2,   16,    2,    3, None,   7000,   4700,  230,  440,  290, 1.35);
    public static readonly MaterialGrade C16    = Timber("c16",    16,   8.5,   17,  2.2,  3.2, None,   8000,   5400,  270,  500,  310, 1.35);
    public static readonly MaterialGrade C18    = Timber("c18",    18,    10,   18,  2.2,  3.4, None,   9000,   6000,  300,  560,  320, 1.35);
    public static readonly MaterialGrade C20    = Timber("c20",    20,  11.5,   19,  2.3,  3.6, None,   9500,   6400,  320,  590,  330, 1.35);
    public static readonly MaterialGrade C22    = Timber("c22",    22,    13,   20,  2.4,  3.8, None,  10000,   6700,  330,  630,  340, 1.35);
    public static readonly MaterialGrade C24    = Timber("c24",    24,  14.5,   21,  2.5,    4, None,  11000,   7400,  370,  690,  350, 1.35);
    public static readonly MaterialGrade C27    = Timber("c27",    27,  16.5,   22,  2.5,    4, None,  11500,   7700,  380,  720,  360, 1.35);
    public static readonly MaterialGrade C30    = Timber("c30",    30,    19,   24,  2.7,    4, None,  12000,   8000,  400,  750,  380, 1.35);
    public static readonly MaterialGrade C35    = Timber("c35",    35,  22.5,   25,  2.7,    4, None,  13000,   8700,  430,  810,  390, 1.35);
    public static readonly MaterialGrade C40    = Timber("c40",    40,    26,   27,  2.8,    4, None,  14000,   9400,  470,  880,  400, 1.35);
    public static readonly MaterialGrade C45    = Timber("c45",    45,    30,   29,  2.9,    4, None,  15000,  10100,  500,  940,  410, 1.35);
    public static readonly MaterialGrade C50    = Timber("c50",    50,  33.5,   30,    3,    4, None,  16000,  10700,  530, 1000,  430, 1.35);

    public static readonly MaterialGrade D18    = Timber("d18",    18,    11,   18,  4.8,  3.5, None,   9500,   8000,  630,  590,  475, 0.90);
    public static readonly MaterialGrade D24    = Timber("d24",    24,    14,   21,  4.9,  3.7, None,  10000,   8400,  670,  630,  485, 0.90);
    public static readonly MaterialGrade D27    = Timber("d27",    27,    16,   22,  5.1,  3.8, None,  10500,   8800,  700,  660,  510, 0.90);
    public static readonly MaterialGrade D30    = Timber("d30",    30,    18,   24,  5.3,  3.9, None,  11000,   9200,  730,  690,  530, 0.90);
    public static readonly MaterialGrade D35    = Timber("d35",    35,    21,   25,  5.4,  4.1, None,  12000,  10100,  800,  750,  540, 0.90);
    public static readonly MaterialGrade D40    = Timber("d40",    40,    24,   27,  5.5,  4.2, None,  13000,  10900,  870,  810,  550, 0.90);
    public static readonly MaterialGrade D45    = Timber("d45",    45,    27,   29,  5.8,  4.4, None,  13500,  11300,  900,  840,  580, 0.90);
    public static readonly MaterialGrade D50    = Timber("d50",    50,    30,   30,  6.2,  4.5, None,  14000,  11800,  930,  880,  620, 0.90);
    public static readonly MaterialGrade D55    = Timber("d55",    55,    33,   32,  6.6,  4.7, None,  15500,  13000, 1030,  970,  660, 0.90);
    public static readonly MaterialGrade D60    = Timber("d60",    60,    36,   33, 10.5,  4.8, None,  17000,  14300, 1130, 1060,  700, 0.90);
    public static readonly MaterialGrade D65    = Timber("d65",    65,    39,   35, 11.3,    5, None,  18500,  15500, 1230, 1160,  750, 0.90);
    public static readonly MaterialGrade D70    = Timber("d70",    70,    42,   36,   12,    5, None,  20000,  16800, 1330, 1250,  800, 0.90);
    public static readonly MaterialGrade D75    = Timber("d75",    75,    45,   37, 12.8,    5, None,  22000,  18500, 1470, 1380,  850, 0.90);
    public static readonly MaterialGrade D80    = Timber("d80",    80,    48,   38, 13.5,    5, None,  24000,  20200, 1600, 1500,  900, 0.90);

    public static readonly MaterialGrade Gl20c  = Glulam("gl20c",    20,    15, 18.5,  10400,   8600,  355);
    public static readonly MaterialGrade Gl22c  = Glulam("gl22c",    22,    16,   20,  10400,   8600,  355);
    public static readonly MaterialGrade Gl24c  = Glulam("gl24c",    24,    17, 21.5,  11000,   9100,  365);
    public static readonly MaterialGrade Gl26c  = Glulam("gl26c",    26,    19, 23.5,  12000,  10000,  385);
    public static readonly MaterialGrade Gl28c  = Glulam("gl28c",    28,  19.5,   24,  12500,  10400,  390);
    public static readonly MaterialGrade Gl30c  = Glulam("gl30c",    30,  19.5, 24.5,  13000,  10800,  390);
    public static readonly MaterialGrade Gl32c  = Glulam("gl32c",    32,  19.5, 24.5,  13500,  11200,  400);
    public static readonly MaterialGrade Gl20h  = Glulam("gl20h",    20,    16,   20,   8400,   7000,  340);
    public static readonly MaterialGrade Gl22h  = Glulam("gl22h",    22,  17.6,   22,  10500,   8800,  370);
    public static readonly MaterialGrade Gl24h  = Glulam("gl24h",    24,  19.2,   24,  11500,   9600,  385);
    public static readonly MaterialGrade Gl26h  = Glulam("gl26h",    26,  20.8,   26,  12100,  10100,  405);
    public static readonly MaterialGrade Gl28h  = Glulam("gl28h",    28,  22.3,   28,  12600,  10500,  425);
    public static readonly MaterialGrade Gl30h  = Glulam("gl30h",    30,    24,   30,  13600,  11300,  430);
    public static readonly MaterialGrade Gl32h  = Glulam("gl32h",    32,  25.6,   32,  14200,  11800,  440);

    public static readonly MaterialGrade Lvl48p = Timber("lvl48p", 48.0, 36.0, 40.0, 6.0, 4.6, Some(2.3), 13_800, 11_700, 430, 760, 510, 1.30);

    // --- [CONCRETE]
    public static readonly MaterialGrade C25_30 = Concrete("c25_30", EnConcreteGrade.C25_30, 25.0, 30.0, "C25/30");
    public static readonly MaterialGrade C30_37 = Concrete("c30_37", EnConcreteGrade.C30_37, 30.0, 37.0, "C30/37");
    public static readonly MaterialGrade C35_45 = Concrete("c35_45", EnConcreteGrade.C35_45, 35.0, 45.0, "C35/45");
    public static readonly MaterialGrade C40_50 = Concrete("c40_50", EnConcreteGrade.C40_50, 40.0, 50.0, "C40/50");
    public static readonly MaterialGrade C45_55 = Concrete("c45_55", EnConcreteGrade.C45_55, 45.0, 55.0, "C45/55");
    public static readonly MaterialGrade C50_60 = Concrete("c50_60", EnConcreteGrade.C50_60, 50.0, 60.0, "C50/60");
    public static readonly MaterialGrade C55_67 = Concrete("c55_67", EnConcreteGrade.C55_67, 55.0, 67.0, "C55/67");
    public static readonly MaterialGrade C60_75 = Concrete("c60_75", EnConcreteGrade.C60_75, 60.0, 75.0, "C60/75");
    public static readonly MaterialGrade C70_85 = Concrete("c70_85", EnConcreteGrade.C70_85, 70.0, 85.0, "C70/85");

    // --- [CMU]
    public static readonly MaterialGrade F2000 = Cmu("f2000", fmMpa: 13.79, netUnitMsMpa: 13.79, netUnitNMpa: Some(18.27));
    public static readonly MaterialGrade F2250 = Cmu("f2250", fmMpa: 15.51, netUnitMsMpa: 17.93, netUnitNMpa: Some(23.44));
    public static readonly MaterialGrade F2500 = Cmu("f2500", fmMpa: 17.24, netUnitMsMpa: 22.41, netUnitNMpa: Some(28.96));
    public static readonly MaterialGrade F2750 = Cmu("f2750", fmMpa: 18.96, netUnitMsMpa: 26.89, netUnitNMpa: Option<double>.None);
    public static readonly MaterialGrade F3000 = Cmu("f3000", fmMpa: 20.69, netUnitMsMpa: 31.03, netUnitNMpa: Option<double>.None);

    public ComponentFamily Family { get; }
    public ComponentAuthority Authority { get; }
    public string SubstanceId { get; }
    public Option<string> AppearanceId { get; }
    public GradeProperties Columns { get; }
    public MaterialId Substance => MaterialId.Create(SubstanceId);
    public Option<MaterialId> Appearance => AppearanceId.Map(MaterialId.Create);

    // --- [MINTS]
    static MaterialGrade Steel(string key, ComponentAuthority authority, double nominalYieldMpa, string substanceId,
        Option<string> enDesignation = default, Option<StainlessRow> stainless = default) =>
        new(ComponentFamily.Steel, authority, substanceId, None,
            new GradeProperties.Steel(nominalYieldMpa, enDesignation, stainless));

    static MaterialGrade Iso(string designation, SizeBand sizes, double proofMpa, double tensileMinMpa, double yieldMinMpa,
        Option<double> alphaV, bool preloadable, Option<GradeStep> step = default) =>
        new(designation, ComponentFamily.Fastener, ComponentAuthority.En,
            $"steel.fastener-{designation.Replace('.', '_')}", Some(tensileMinMpa >= 800.0 ? "metal.steel" : "metal.iron"),
            new GradeProperties.Fastener(sizes, Some(proofMpa), tensileMinMpa, yieldMinMpa,
                SpecifiedUltimateMpa: double.Parse(designation.Split('.')[0]) * 100.0, alphaV, preloadable, step));

    static MaterialGrade Us(string key, ComponentAuthority authority, SizeBand sizes, Option<double> proofKsi,
        double tensileKsi, double yieldKsi, bool preloadable,
        Option<(double AboveIn, double ProofKsi, double TensileKsi, double YieldKsi)> step = default) =>
        new(ComponentFamily.Fastener, authority,
            $"steel.fastener-{key}", Some(tensileKsi >= 120.0 ? "metal.steel" : "metal.iron"),
            new GradeProperties.Fastener(sizes, proofKsi.Map(static ksi => ksi * KsiToMpa), tensileKsi * KsiToMpa,
                yieldKsi * KsiToMpa, SpecifiedUltimateMpa: tensileKsi * KsiToMpa, EurocodeAlphaV: None, preloadable,
                step.Map(static s => new GradeStep(s.AboveIn * ThreadRow.InchToMm, s.ProofKsi * KsiToMpa,
                    s.TensileKsi * KsiToMpa, s.YieldKsi * KsiToMpa))));

    static MaterialGrade Rebar(string key, ComponentAuthority authority, Option<double> yieldMpa, RebarStandard standard,
        string substanceId, string appearanceId, Option<EnRebarGrade> en = default) =>
        new(ComponentFamily.Reinforcement, authority, substanceId, Some(appearanceId),
            new GradeProperties.Rebar(yieldMpa, standard, en));

    static MaterialGrade Strand(string key, ComponentAuthority authority, double diameterMm, double areaMm2,
        double ultimateMpa, double yieldRatio, string substanceId) =>
        new(ComponentFamily.Reinforcement, authority, substanceId, None,
            new GradeProperties.Strand(diameterMm, areaMm2, ultimateMpa, yieldRatio, RelaxationClass.LowRelaxation));

    static MaterialGrade Aluminum(string key, BucklingClass bucklingClass, string substanceId, Seq<AlloyBand> bands,
        Option<HazRow> haz) =>
        new(ComponentFamily.Aluminum, ComponentAuthority.En, substanceId, None,
            new GradeProperties.Aluminum(bucklingClass, bands, haz));

    static MaterialGrade Timber(string key, double fmk, double ft0k, double fc0k, double fc90k, double fvk,
        Option<double> fRvk, double e0Mean, double e005, double e90Mean, double gMean, double densityK, double k90Base) =>
        new(ComponentFamily.Timber, ComponentAuthority.En, $"wood.{key}", None,
            new GradeProperties.Timber(fmk, ft0k, fc0k, fc90k, fvk, fRvk, e0Mean, e005, e90Mean, gMean, densityK, k90Base));

    static MaterialGrade Glulam(string key, double fmk, double ft0k, double fc0k, double e0Mean, double e005, double rhoK) =>
        Timber(fmk, ft0k, fc0k, Fc90GlulamMpa, FvGlulamMpa, Some(FrGlulamMpa), e0Mean, e005, E90GlulamMpa, GGlulamMpa, rhoK, 1.35);

    static MaterialGrade Concrete(string key, EnConcreteGrade en, double fckMpa, double fckCubeMpa, string mixToken) =>
        new(ComponentFamily.Concrete, ComponentAuthority.En, $"concrete.{key}", None,
            new GradeProperties.Concrete(en, fckMpa, fckCubeMpa, mixToken));

    static MaterialGrade Cmu(string key, double fmMpa, double netUnitMsMpa, Option<double> netUnitNMpa) =>
        new(ComponentFamily.Cmu, ComponentAuthority.Astm, "concrete.cmu", None,
            new GradeProperties.Cmu(fmMpa, netUnitMsMpa, netUnitNMpa));

    const double KsiToMpa = 6.894757;
    static readonly SizeBand IsoRange = new(1.6, 39.0);
    static readonly SizeBand SaeRange = new(0.25 * ThreadRow.InchToMm, 1.5 * ThreadRow.InchToMm);
    static readonly SizeBand SaeCapRange = new(0.25 * ThreadRow.InchToMm, 1.0 * ThreadRow.InchToMm);
    static readonly SizeBand F3125Range = new(0.5 * ThreadRow.InchToMm, 1.5 * ThreadRow.InchToMm);
    static readonly SizeBand TwistOffRange = new(0.5 * ThreadRow.InchToMm, 1.25 * ThreadRow.InchToMm);
    static readonly SizeBand F1554Range = new(0.5 * ThreadRow.InchToMm, 4.0 * ThreadRow.InchToMm);
    static readonly SizeBand F1554HighRange = new(0.5 * ThreadRow.InchToMm, 3.0 * ThreadRow.InchToMm);
    const double Fc90GlulamMpa = 2.5;
    const double FvGlulamMpa = 3.5;
    const double FrGlulamMpa = 1.2;
    const double E90GlulamMpa = 300.0;
    const double GGlulamMpa = 650.0;
}
```

## [11]-[COMPONENT_SEED]

- Owner: `SeedLaw<TRow>` the per-family seed POLICY VALUE and `ComponentSeed` the ONE generator fold — the traverse + per-row coherence proof + profile route + detail fold + result-typed `Component.Of` lift + `ComponentRow` wrap every `<Family>Seed.Rows(Context)` body hand-rolled. A family page now carries its VOCABULARY and its ROWS alone; its `ComponentFamily.rows:` delegate binds `context => ComponentSeed.Rows(context, Roster, Law)`.
- Cases: `Coherence` is the row's multi-column proof on the `Validation` result (form/extent consistency, band coverage — every offending column named, never first-defect); `Profile` the family's profile route; `Detail` present exactly where the family lane is not `DetailLane.None` (the lane law `Component.Of` re-proves); `Ifc`/`Voids`/`Standard`/`Substance`/`Appearance`/`Source` the per-row selectors, most family-constant.
- Entry: `ComponentSeed.Rows(context, roster, law)` — rows admit INDEPENDENTLY on the `Validation` applicative, so a roster with three malformed rows names all three in ONE verdict, then the build aborts typed under the fail-loud CATALOGUE law (the abort is unchanged; the census is new — the hand folds' `Traverse` over `Fin` stopped at the first bad row).
- Growth: a new family seed is ONE `SeedLaw` value beside its roster — the algorithm is closed; a new per-row proof is one conjunct in that family's `Coherence`.
- Boundary: NAMED LOSS — a family page can no longer vary the seed ALGORITHM, only its policy value; a family needing a genuinely different fold shape (none exists on disk — every retired body was this shape) would be a new entry on THIS owner, never a local fold. SECOND NAMED LOSS — `Coherence` proves and DISCARDS: the arm a family proved present re-reads its own `Option` inside `Profile`/`Detail`; the proof-carrying widening (`SeedLaw<TRow, TProof>` threading the proven shape) is the declared growth move the day a family's coherence outgrows a guard, never a per-family re-derivation. WITNESS — `insulation.md` `InsulationSeed.Rows` (a per-row `AcceptValidated` + `ProfileOf` coherence guard + `InsulationDetail.Of` + `Component.Of` traverse) rebuilds as `ComponentSeed.Rows(context, Roster, InsulationLaw)` with `InsulationLaw` declaring the SAME coherence, route, and bag as data — and its form/extent guard now ACCUMULATES across rows.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record SeedLaw<TRow>(
    ComponentFamily Family,
    Func<TRow, string> Designation,
    Func<TRow, Validation<Error, Unit>> Coherence,
    Func<TRow, Fin<SectionProfile>> Profile,
    Func<TRow, MaterialId> Substance,
    Func<TRow, MaterialId> Appearance,
    Func<TRow, EvidenceGrade> Source,
    Func<TRow, IfcBinding> Ifc,
    Func<TRow, Coring> Voids,
    Func<TRow, ComponentStandard> Standard,
    Option<Func<TRow, SectionProfile, Fin<PropertyBag>>> Detail) {

    public static SeedLaw<TRow> Of(
        ComponentFamily family,
        Func<TRow, string> designation,
        Func<TRow, Validation<Error, Unit>> coherence,
        Func<TRow, Fin<SectionProfile>> profile,
        Func<TRow, MaterialId> substance,
        Func<TRow, EvidenceGrade> source,
        Func<TRow, ComponentStandard> standard,
        Option<Func<TRow, SectionProfile, Fin<PropertyBag>>> detail,
        Func<TRow, MaterialId>? appearance = null,
        Func<TRow, IfcBinding>? ifc = null,
        Func<TRow, Coring>? voids = null) =>
        new(family, designation, coherence, profile, substance,
            appearance ?? substance, source,
            ifc ?? (_ => family.Ifc),
            voids ?? (static _ => Coring.None),
            standard, detail);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ComponentSeed {
    public static Fin<Seq<ComponentRow>> Rows<TRow>(Context context, Seq<TRow> roster, SeedLaw<TRow> law) =>
        roster.Traverse(row => RowOf(context, row, law).ToValidation()).As().ToFin();

    static Fin<ComponentRow> RowOf<TRow>(Context context, TRow row, SeedLaw<TRow> law) =>
        from proven in law.Coherence(row, context.Key).ToFin()
        from profile in law.Profile(row, context.Key)
        from detail in law.Detail
            .TraverseM(fold => fold(row, profile, context.Key))
            .As()
        from item in Component.Of(
            law.Family, law.Designation(row), profile, law.Ifc(row), law.Voids(row), law.Standard(row),
            substanceId: law.Substance(row), appearanceId: law.Appearance(row), detail, context.Key)
        select new ComponentRow(item, law.Source(row));
}
```

## [12]-[RESEARCH]

(none)
