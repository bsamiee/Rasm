# [MATERIALS_COMPONENT]

THE POLYMORPHIC COMPONENT OWNER. One `Component` record is the canonical standardized-TYPE concept every steel/timber member, masonry/CMU/IGU unit, sheet-goods board, and rebar/fastener/connector/joint part parameterizes: a closed `SectionProfile` cross-section algebra (a FIELD, never a peer union of per-family payloads), an `IfcBinding` row stamp, a `Coring` void class, a `ComponentStandard` receipt, two independent `MaterialId` slots, and an `Option<PropertyBag>` detail whose presence is a TYPE LAW of the family's `DetailLane`. One `ComponentFamily` `[SmartEnum<string>]` is THE policy row: each of the ten rows {masonry · cmu · steel · timber · glazing · reinforcement · fastener · connector · joint · panel} carries its `ComponentClass`, its `DetailLane`, its profile-admission predicate, its cross-nominal selector, and its `<Family>Seed.Rows` fold — so a NEW FAMILY is one row plus one seed page with zero central edits, and a NEW PART is one seed row with zero type edits. Every per-family discriminant is a stored column, a base-constructor fact, a family delegate, or a field read — the family axis carries its own class, lane, IFC leaf, admission predicate, cross-nominal selector, seed fold, and capacity producer, so nothing about a family is decided by dispatch anywhere else.

This page also owns the ONE section-computation spine: `SectionSolver.Solve` is the single exhaustive generated `Switch` over the closed `SectionProfile` axis — a new arm is compiler-forced HERE and nowhere else. Every solvable arm routes the ONE `VividOrange.Sections.SectionProperties` Green's-theorem integral through the ONE twenty-column `Admit` lift, then a topology supplement: `Curves` lowers each arm onto the TYPED `VividOrange.IProfiles` floor contract (`IRectangle`/`IRoundedRectangle`/`IRectangularHollow`/`IRoundedRectangularHollow`/`ICircle`/`ICircularHollow`/`IEllipse`/`IIParallelFlange`/`ICustomI`/`IChannel`/`IC`/`ITee`/`IAngle`/`IZ`) or an exact `Perimeter`; the typed contracts let the solver decompose fillets and curved edges through `TrapezoidalPart` + `EllipseQuarterPart`, while the `Perimeter` path preserves multi-void, corrugated, offset-trapezium, triangle, and free-outline geometry. `Forms` supplies the plastic/torsion/warping/shear/asymmetry supplement through TWO kernels: `ThinWalled` sweeps each open arm's own midline for its shear centre, warping constant, and Saint-Venant torsion, and `Plastic` integrates the exact equal-area-axis first moment over each arm's strip stack — so a shape factor against an elastic modulus and a per-shape warping transcription are both unrepresentable. `ComputedSection` stays the FROZEN twenty-column receipt (`[FROZEN_INVARIANTS]`), `ComponentCatalogue.Of` the ONE fail-loud `Traverse` fold computing BOTH frozen maps in one pass beside `AdmitImported`, the reverse-direction fold lowering ingested IFC type candidates onto the same railed `Component.Of` construction, `ComponentResolution` the byte-identical M7 one-hop, `QuantityRow` the one typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose, and `ComponentDetail` the seed-time bag constructors every `Realization`/`Product`-lane seed composes. The page composes the `Rasm.Numerics` kernel `PositiveMagnitude`, the `Rasm.Domain` `Op`/`Context`/`Expected`/`AcceptValidated` rail, the `Rasm.Element` seam (`MaterialId`, `ProfileRef`, `FaultBand`, the `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`DetailSchema` property vocabulary), and the VividOrange profile/section floor. `ProfileRef`/`ProfileSet`/`SectionProperties`/`ComputedSection` stay seam-canonical — the rename STOPS at the Materials boundary.

## [01]-[INDEX]

- [02]-[COMPONENT_OWNER]: `ComponentId`, `ComponentClass`, `DetailLane`, `CoringClass`/`Coring`, `ComponentAuthority`, `IfcBinding`, the `ComponentFamily` policy row, the band-2300 `ComponentFault` (registry read), `ComponentUnit`, `ComponentStandard`, and the `Component` record with its ONE polymorphic `Of` admission.
- [03]-[SECTION_PROFILE]: `VoidCell`, the bounded `PlyRole` vocabulary, `Ply`, and the closed `SectionProfile` `[Union]` — named `PositiveMagnitude` dimensions, gross bounding facts as base-constructor state, and one railed `Of` factory per arm.
- [04]-[SECTION_SOLVER]: `SectionSupplement`, the `ProfileGeometry` row and the ONE `SectionGeometry.Of` dispatch behind `SectionSolver.Solve`/`ProfileOf`, the twenty-column `Admit` lift, the `Curves` typed-contract table, the `Forms` supplement table, and the `ThinWalled` sectorial and `Plastic` strip kernels every arm composes.
- [05]-[COMPUTED_SECTION]: the FROZEN twenty-field receipt + `GoverningRadiusMm`.
- [06]-[CATALOGUE]: `SeedJoin`, `ComponentRow`, `ComponentCatalogue.Of`/`Lookup`/`AdmitImported` — the railed seed-table join, the fail-loud one-pass fold over `ComponentFamily.Items`, and the imported-candidate admission fold beside it.
- [07]-[QUANTITY_ROW]: the one typed-mint `[SmartEnum]` the seam lift and the property ingress compose.
- [08]-[COMPONENT_DETAIL]: the seed-time bag constructors (`Joint`/`Sourced`/`Token`/`Measured`/`RealizationRows`/`ProductRows`).
- [09]-[COMPONENT_RESOLUTION]: `ResolvedComponent` + the frozen `Build`/`Resolve` M7 cache, byte-identical.

## [02]-[COMPONENT_OWNER]

- Owner: `Component` over the ten-row `ComponentFamily` policy axis; `ComponentId` the `family.designation` key; `ComponentFault` band 2300 read from the `FaultBand` registry; `IfcBinding` the Materials-side IFC pair as ROW DATA (was two 10-arm switches); `DetailLane` the bag-presence law; `Coring`/`CoringClass` the cross-family void class; `ComponentUnit`/`ComponentStandard` the shared dimensional and regional receipts.
- Cases: one `Component` shape across all ten families — `Family` (the policy row), `Designation`, `Profile` (the `SectionProfile` arm), `Ifc` (the stored `IfcBinding` row), `Coring`, `Standard`, `SubstanceId`/`AppearanceId` (independent `MaterialId` slots — a coated rebar keeps capacity steel and epoxy appearance distinct), `Detail` (`Option<PropertyBag>`, `Some` iff the family lane is not `None`). Every per-family discriminant is a field read (`IfcEntity`/`PredefinedToken`), a family delegate (`CrossNominalMm`), or base-constructor state (`GrossRectangleMm`).
- Entry: `Component.Of(family, designation, profile, ifc, coring, standard, substanceId, appearanceId, detail, key)` guards the exact `family.designation` grammar, family-prefix ownership, family/profile admission, coring, and lane/detail consistency. `SectionProfile.Layered.Of` owns ply-stack closure because every admitted layered value must be valid before a `Component` exists.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`/`Context`/`Expected`/`AcceptValidated`), Rasm.Element (project — `MaterialId`, `ProfileRef`, `FaultBand`, `MeasureValue`/`QuantityType`/`Dimension`/`PropertyBag`/`PropertyName`/`PropertyValue`/`DetailSchema`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[ValueObject<string>]`/`[UseDelegateFromConstructor]`, generated exhaustive `Switch`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`; `libs/csharp/.api/api-thinktecture-runtime-extensions.md`), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`guard` — rails-and-effects doctrine substrate), VividOrange.IProfiles + VividOrange.Profiles.Perimeter + VividOrange.Geometry + VividOrange.Sections.SectionProperties + UnitsNet (`Rasm.Materials/.api` catalogues), BCL inbox (`FrozenDictionary`).
- Growth: a new part is one seed row; a new family is one `ComponentFamily` row plus one seed page (no union edit, no switch edit, no `.Concat` edit); a new section shape is one `SectionProfile` arm plus one compiler-forced `Solve` arm (buildingSMART profile-schema cadence, never thing cadence); a new fault is one `ComponentFault` case; a new structural column is one `ComputedSection` field every solvable arm fills; a new detail row is one `DetailSchema`-named tuple in a seed bag; a new typed mint is one `QuantityRow` row; a new row provenance is one `Provenance` row, never a second catalogue.
- Boundary: `ComponentFault` derives `Expected` (`IValidationError<ComponentFault>`), its `Code` the `FaultBand.Component` registry read — disjointness is type-enforced, never prose. `IfcBinding` strings stay NEUTRAL here; the generated `Rasm.Bim` roster is the validation authority (composition-time `IfcLegality`, egress-time `AdmitPredefined`, design-time emitter stamp audit) — Materials never references `Rasm.Bim`. Every seed page declares the ONE flat `Rasm.Materials.Component` namespace — the `Component/` folder is one namespace under `dotnet_style_namespace_match_folder = true:error` — so the policy rows bind `<Family>Seed.Rows` by bare name with no alias.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;                          // CultureInfo.InvariantCulture (the ComponentId.Validate format provider)
using System.Linq;                                   // Index, All, Sum, ForAll over the Seq and cell spans
using LanguageExt;
using LanguageExt.Common;                            // Error (the MapFail carrier the solver traps VividOrange throws onto)
using Rasm.Numerics;                                  // PositiveMagnitude — the kernel >0-finite magnitude (Atoms.cs)
using Rasm.Domain;                                   // Op, Context, Expected, AcceptValidated
using Rasm.Element.Composition;      // MaterialId, ProfileRef
using Rasm.Element.Projection;       // FaultBand
using Rasm.Element.Properties;       // MeasureValue, QuantityType, PropertyBag, PropertyName, PropertyValue, DetailSchema, Dimension
using Thinktecture;
using UnitsNet;                                      // Length — the typed profile-contract dimension currency
using VividOrange.Geometry;                          // LocalPoint2d, LocalPolyline2d, ILocalPoint2d, ILocalPolyline2d
using VividOrange.Profiles;                          // IProfile + the typed parametric contracts (IRectangle/ICircle/IEllipse/II/IC/IZ/ITee/IAngle/…), Perimeter
using VividOrange.Sections.SectionProperties.Utility; // Areas/Inertiae/SectionModuli — the carrier-free Green's-theorem kernels
using Expected = Rasm.Domain.Expected;               // the kernel Expected (parameterless ctor + virtual Code/Category), NOT LanguageExt.Common.Expected
using Dimension = Rasm.Element.Properties.Dimension;            // the seam 7-exponent measure Dimension (Rasm.Numerics.Dimension, the count atom, is unused here)
using SectionProperties = VividOrange.Sections.SectionProperties.SectionProperties;   // the solver carrier; the seam Rasm.Element.Composition.SectionProperties is prose-only on this page
using static LanguageExt.Prelude;

// component#COMPONENT_OWNER owns Component/ComponentClass/ComponentFamily/ComponentId/ComponentFault/DetailLane/
// IfcBinding/Coring/CoringClass/ComponentUnit/ComponentStandard/ComponentAuthority/SectionProfile/VoidCell/PlyRole/
// Ply/SectionSolver/ComputedSection/ComponentRow/ComponentCatalogue/QuantityRow/ComponentDetail/ComponentResolution.
// Every <Family>Seed declares in this ONE Rasm.Materials.Component namespace (the <Family>Seed naming keeps rows
// collision-free), so the ComponentFamily policy rows bind SteelSeed.Rows/MasonrySeed.Rows/… by bare name — no alias.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The component key is exactly `family.designation`: one non-edge separator and two non-empty tokens. Component.Of
// proves that the prefix equals the selected ComponentFamily key.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ComponentId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("<component-id requires 'family.designation'>");
            return;
        }

        int separator = value.IndexOf('.');
        validationError = separator <= 0 || separator != value.LastIndexOf('.') || separator == value.Length - 1
            ? new ValidationError("<component-id requires 'family.designation'>")
            : null;
    }
}

// The structural-class discriminant, a THREE-row axis: Primary a space-bounding one-piece member (geometry-on-type,
// one occurrence one piece), Panel a standardized sheet-goods board (one board type, MANY laid pieces), Minor a
// standardized part (one type, many discrete pieces). The class carries NO IFC column: `IfcBuiltElement` and
// `IfcElementComponent` are ABSTRACT IFC4 entities no file can instantiate, and one abstract pair shared across six
// Minor families leaves the reverse Claimant read electing nothing. The concrete leaf is the ComponentFamily row's
// own IfcBinding, unique per family by construction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentClass {
    public static readonly ComponentClass Primary = new("primary");
    public static readonly ComponentClass Panel   = new("panel");
    public static readonly ComponentClass Minor   = new("minor");
}

// The bag-presence law: None carries no PropertyBag; Realization carries the DetailSchema.Realization bag (discrete
// realizing parts + masonry's EN 771 envelope); Product carries the DetailSchema.Product bag (panel boards, glazing
// IGU builds). Component.Of enforces lane/detail consistency as a TYPE LAW, so no consumer dispatches on presence.
public enum DetailLane : byte { None = 0, Realization = 1, Product = 2 }

// The ASTM C652 / C90 net-area FAMILY a void class belongs to — a pure classification token the IFC profile lane and
// the appearance readers key on. It stores no net-area floor: the C652/C216/C90 band thresholds live inside the two
// derivations that compare against them (`masonry#MASONRY_FAMILY` `MasonryVoids.Bucket`, `cmu#CMU_FAMILY`
// `CmuPhysics.CoringOf`), each reading the unit's TRUE lattice geometry, and every capacity path reads the solved
// `ComputedSection.AreaMm2` — a stored fraction beside that geometry could only contradict it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid      = new("solid");        // ASTM C62/C216 solid clay
    public static readonly CoringClass Frogged    = new("frogged");      // single-face indentation, solid for net-area
    public static readonly CoringClass Cored      = new("cored");        // <=25% coring, structurally solid
    public static readonly CoringClass Cellular   = new("cellular");     // closed cavity one bed face
    public static readonly CoringClass Perforated = new("perforated");   // through-perforations, net 50-75%
    public static readonly CoringClass Hollow     = new("hollow");       // ASTM C652 H40V/H60V hollow brick / C90 CMU
}

// The void-class row a Component carries: the shared vocabulary spanning clay-brick (frog/cored/perforated/cellular)
// AND concrete-masonry (hollow 2-cell / 3-cell) geometry, each row naming its C652/C90 family. A non-masonry/non-cmu
// Component carries Coring.None, total across the axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coring {
    public static readonly Coring None             = new("none",               classification: CoringClass.Solid);
    public static readonly Coring Frog             = new("frog",               classification: CoringClass.Frogged);
    public static readonly Coring Cored3Hole       = new("cored-3-hole",       classification: CoringClass.Cored);
    public static readonly Coring Cellular         = new("cellular",           classification: CoringClass.Cellular);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell", classification: CoringClass.Perforated);
    public static readonly Coring Hollow3Cell      = new("hollow-3-cell",      classification: CoringClass.Hollow);   // 12-in 3-cell CMU — more web material than the 2-cell unit
    public static readonly Coring Hollow2Cell      = new("hollow-2-cell",      classification: CoringClass.Hollow);
    public CoringClass Classification { get; }
}

// The SEED_ROW_LAW provenance of a row, as a TYPED column rather than a comment above the table. ARCHITECTURE
// [04]-[ROUTING] promises per-column provenance on every standards table, and a prose banner cannot survive a row
// being copied, split, or extended — so each row STATES where its values came from and a mixed table (a mostly
// published roster carrying one authored fallback) is representable instead of mislabelled wholesale.
// Attributable is the column's BEHAVIOUR, and it is what makes this axis a policy row rather than a label: a value an
// external body published, derived by its own formula, or shipped in an admitted package traces to a producer outside
// this estate, while an Authored value traces only to this estate's own judgement. TWO consumers read it — the
// ComponentCatalogue.AdmitImported election refuses to hand a vendor type an Authored row's geometry as though a
// standard had published it, and ComponentDetail.Sourced stamps it onto every Realization/Product bag so the
// provenance crosses the seam as detail evidence beside the values it qualifies.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Provenance {
    public static readonly Provenance Published = new("published", attributable: true);    // transcribed verbatim from a standards or manufacturer table
    public static readonly Provenance Defined   = new("defined",   attributable: true);    // derived by the standard's own formula from published columns
    public static readonly Provenance Vendor    = new("vendor",    attributable: true);    // read from an admitted package factory or catalogue
    public static readonly Provenance Authored  = new("authored",  attributable: false);   // this estate's stated policy value with no external producer
    public bool Attributable { get; }
}

// The published standards body a regional ComponentStandard cites — a bounded vocabulary, never a free Authority string.
// Jacking is the body's PRESTRESSING proof-stress ceiling, and it is a PAIR because both codes state the ceiling as a
// minimum of TWO terms against TWO different strengths — ACI 318 Table 20.3.2.5.1 min(0.80·fpu, 0.94·fpy), EC2
// §5.10.2.1 min(0.80·fpk, 0.90·fp0,1k). Storing the proof coefficient alone left the ultimate term to be re-typed at
// the reinforcement#REINFORCEMENT_FAMILY TendonBasis.Jacking site under whichever literal that site happened to
// carry, so the two codes' ceilings could diverge from their own bodies without either column moving. A body
// publishing no jacking rule carries None and a tendon it stamps projects ABSENCE rather than a fabricated ceiling.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentAuthority {
    public static readonly ComponentAuthority Astm = new("ASTM", region: "us",  jacking: Some((Ultimate: 0.80, Proof: 0.94)));   // ASTM C90/C216/C652 masonry & CMU, A615/A706 rebar, F3125 bolts, A416 strand (ACI 318 jacking)
    public static readonly ComponentAuthority Aisc = new("AISC", region: "us");          // AISC Shapes Database
    public static readonly ComponentAuthority Aisi = new("AISI", region: "us");          // AISI S100 cold-formed
    public static readonly ComponentAuthority Aws  = new("AWS",  region: "us");          // AWS A5.1/D1.1 weld consumables
    public static readonly ComponentAuthority Awc  = new("AWC",  region: "us");          // AWC SDPWS wood lateral-force provisions — the American Wood Council, NOT the like-spelled welding society above
    public static readonly ComponentAuthority Sae  = new("SAE",  region: "us");          // SAE J429 bolt grades
    public static readonly ComponentAuthority IccEs = new("ICC-ES", region: "us");       // ICC-ES / UES evaluation reports — the connector allowables' issuing body
    public static readonly ComponentAuthority En   = new("EN",   region: "eu",  jacking: Some((Ultimate: 0.80, Proof: 0.90)));   // EN 10365/338/14080/1279/10080/898-1/ISO 261, EN 10138-3 strand (EC2 jacking)
    public static readonly ComponentAuthority Bs   = new("BS",   region: "uk");          // BS EN 771 masonry, BS 8666 rebar schedule
    public static readonly ComponentAuthority Din  = new("DIN",  region: "din");         // DIN 105 masonry
    public static readonly ComponentAuthority As   = new("AS",   region: "au");          // AS 4773 masonry
    public static readonly ComponentAuthority Is   = new("IS",   region: "is");          // IS 1077 masonry
    public static readonly ComponentAuthority Apa  = new("APA",  region: "us");          // APA PRG 320 CLT
    public static readonly ComponentAuthority Csa  = new("CSA",  region: "ca");          // CSA G30.18 rebar
    public static readonly ComponentAuthority Sdi  = new("SDI",  region: "us");          // ANSI/SDI steel deck
    public string Region { get; }
    public Option<(double Ultimate, double Proof)> Jacking { get; }

    // The ceiling itself, so the two-term minimum is evaluated at its OWNER and a consumer passes strengths rather
    // than re-deriving a rule from coefficients.
    public Option<double> JackingCeilingMpa(double ultimateMpa, double proofMpa) =>
        Jacking.Map(row => Math.Min(row.Ultimate * ultimateMpa, row.Proof * proofMpa));
}

// The Materials-side IFC stamp as ROW DATA. Entity is a CONCRETE IFC4 leaf — an abstract supertype is uninstantiable
// and therefore never stamped — and ObjectType carries the user-defined discriminator IFC itself REQUIRES whenever
// Predefined is `USERDEFINED`, which is also what keeps the (Entity, Predefined, ObjectType) triple unique per family
// where IFC publishes no distinct leaf for two products (a clay unit and a concrete-masonry unit are both
// IfcBuildingElementPart). Strings stay neutral here; the Bim generated roster is the validation authority —
// composition-time via IfcLegality, egress-time via AdmitPredefined, design-time via the emitter stamp audit.
// Materials never references Rasm.Bim.
public readonly record struct IfcBinding(string Entity, string Predefined, Option<string> ObjectType) {
    public static IfcBinding Of(string entity, string predefined) => new(entity, predefined, None);
    public static IfcBinding Named(string entity, string objectType) => new(entity, "USERDEFINED", Some(objectType));
}

// The ONE family axis as a POLICY ROW: class, detail lane, the admission predicate (was the family/section mismatch
// guard), the cross-nominal selector (was the 10-arm switch), the seed-row fold, and the CAPACITY PRODUCER. The
// catalogue folds Items, so a NEW FAMILY is this row plus one seed page — no central edit anywhere. Seed owners are
// named <Family>Seed.
// The capacity column is what gives capacity#SECTION_CAPACITY's Lift/Check/SectionSelection folds real producers: a
// family EITHER binds the entry that lowers its own Component onto a SectionCapacity, or binds an explicit typed
// refusal naming the route that does own its capacity (a bar's rides the RcSection assembler, a fastener's the
// FastenerAssembly). Silence is unrepresentable — a new family that forgets its capacity is a missing constructor
// argument, and the placement facts a catalogue row cannot carry ride the ONE CapacityPlacement currency.
// Steel admits every FABRICATED open-thin-walled arm beside the catalogued and hollow ones — a welded plate girder is
// AsymmetricIShape, a cranked mono-I an IShape, a fabricated channel/tee/angle its own arm, a cold-formed purlin Zed,
// and a rolled-corner bar RoundedRectangle — so the Forms.OpenI/MonoI/OpenChannel/MonoTee/OpenAngle/PointSymmetricZ/
// SolidRounded algebra the solver already prices is reachable from an admitted Component; Timber admits
// RoundedRectangle for the eased-arris glulam. An arm no family admits is capability the catalogue cannot seed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ComponentFamily {
    public static readonly ComponentFamily Masonry       = new("masonry",       ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcBuildingElementPart", "MasonryUnit"),          admits: static p => p is SectionProfile.Rectangle or SectionProfile.CellularRectangle,                                    crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: MasonrySeed.Rows, capacity: MasonrySeed.Capacity);
    public static readonly ComponentFamily Cmu           = new("cmu",           ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcBuildingElementPart", "ConcreteMasonryUnit"),  admits: static p => p is SectionProfile.CellularRectangle,                                                                crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: CmuSeed.Rows, capacity: CmuSeed.Capacity);
    public static readonly ComponentFamily Steel         = new("steel",         ComponentClass.Primary, DetailLane.Realization, IfcBinding.Named("IfcBuildingElementProxy", "SteelSection"),        admits: static p => p is SectionProfile.Catalogued or SectionProfile.RectangleHollow or SectionProfile.CircleHollow or SectionProfile.ColdFormedC or SectionProfile.BuiltUp or SectionProfile.IShape or SectionProfile.AsymmetricIShape or SectionProfile.Channel or SectionProfile.Tee or SectionProfile.Angle or SectionProfile.Zed or SectionProfile.RoundedRectangle, crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: SteelSeed.Rows, capacity: SteelSeed.Capacity);
    public static readonly ComponentFamily Timber        = new("timber",        ComponentClass.Primary, DetailLane.None,        IfcBinding.Named("IfcBuildingElementProxy", "TimberSection"),       admits: static p => p is SectionProfile.Rectangle or SectionProfile.RoundedRectangle || p is SectionProfile.Layered l && l.Plies.ForAll(static ply => ply.Role == PlyRole.Longitudinal || ply.Role == PlyRole.Transverse), crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: TimberSeed.Rows, capacity: TimberSeed.Capacity);
    public static readonly ComponentFamily Glazing       = new("glazing",       ComponentClass.Minor,   DetailLane.Product,     IfcBinding.Of("IfcPlate", "SHEET"),                                 admits: static p => p is SectionProfile.Layered l && l.Plies.ForAll(static ply => ply.Role == PlyRole.Pane || ply.Role == PlyRole.Interlayer || ply.Role == PlyRole.Cavity),               crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: GlazingSeed.Rows, capacity: GlazingSeed.Capacity);
    public static readonly ComponentFamily Reinforcement = new("reinforcement", ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Of("IfcReinforcingBar", "NOTDEFINED"),                   admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: ReinforcementSeed.Rows, capacity: ReinforcementSeed.Capacity);
    public static readonly ComponentFamily Fastener      = new("fastener",      ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Of("IfcMechanicalFastener", "BOLT"),                     admits: static p => p is SectionProfile.Circle,                                                                            crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: FastenerSeed.Rows, capacity: FastenerSeed.Capacity);
    public static readonly ComponentFamily Connector     = new("connector",     ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcDiscreteAccessory", "Connector"),              admits: static p => p is SectionProfile.Rectangle or SectionProfile.Outline,                                              crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: ConnectorSeed.Rows, capacity: ConnectorSeed.Capacity);
    public static readonly ComponentFamily Joint         = new("joint",         ComponentClass.Minor,   DetailLane.Realization, IfcBinding.Named("IfcFastener", "Joint"),                           admits: static p => p is SectionProfile.FilletTriangle or SectionProfile.Trapezium or SectionProfile.Circle or SectionProfile.Nominal, crossNominal: static p => p.GrossRectangleMm.WidthMm, rows: JointSeed.Rows, capacity: JointSeed.Capacity);
    public static readonly ComponentFamily Panel         = new("panel",         ComponentClass.Panel,   DetailLane.Product,     IfcBinding.Of("IfcPlate", "NOTDEFINED"),                            admits: static p => p is SectionProfile.Layered or SectionProfile.Corrugated,                                             crossNominal: static p => p.GrossRectangleMm.DepthMm, rows: PanelSeed.Rows, capacity: PanelSeed.Capacity);

    [UseDelegateFromConstructor] public partial bool Admits(SectionProfile profile);
    [UseDelegateFromConstructor] public partial PositiveMagnitude CrossNominal(SectionProfile profile);
    [UseDelegateFromConstructor] public partial Fin<Seq<ComponentRow>> Rows(Context context);
    [UseDelegateFromConstructor] public partial Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key);
    public ComponentClass Class { get; }
    public DetailLane Lane { get; }

    // The family's own CONCRETE IFC4 leaf, unique across the ten rows by construction — the reverse Claimant read in
    // ComponentCatalogue.AdmitImported keys on this triple, so an ingested type elects exactly one family instead of
    // six Minor families colliding on one abstract pair. A family whose leaf is genuinely kind-determined (a panel
    // board versus a deck sheet versus a covering) overrides it PER SEED ROW; this column is the family default and
    // the reverse-read key.
    public IfcBinding Ifc { get; }
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The component fault band: Expected-derived so a typed case lifts BARE onto Fin<T>/Validation<Error,T>; Code is the
// FaultBand.Component registry read (band disjointness type-enforced at FaultBand initialization, never prose). Nine
// disjoint slots by Category: Dimension a non-positive/non-finite length or degenerate geometry, Coring a DERIVED void
// fraction outside [0,1) at its bucket owner, Family a family/profile/registration mismatch, Bond a masonry COURSE-PATTERN fault, Mortar a masonry
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

// The ONE standardized-type owner — profile + IFC stamp + seed-built detail carry the whole type; every
// switch is a field read or a family-delegate read. Of guards Component-owned invariants only: the ComponentId format,
// the family/profile admission, and the lane/detail consistency (a None-lane family carries no bag; a
// Realization/Product-lane family always carries one). Dimensional admission already happened in the SectionProfile Of
// factories, and the void fraction is proven at its DERIVATION owner (MasonryVoids.Bucket) where the lattice geometry
// lives — a Component never re-proves a PositiveMagnitude the kernel admitted nor a fraction no column stores.
public sealed record Component(
    ComponentFamily Family, ComponentId Designation, SectionProfile Profile, IfcBinding Ifc,
    Coring Coring, ComponentStandard Standard, MaterialId SubstanceId, MaterialId AppearanceId,
    Option<PropertyBag> Detail) {

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
        from owned in guard(id.Value.StartsWith($"{family.Key}.", StringComparison.Ordinal),
            ComponentFault.Designation(key, $"<designation-family-mismatch:{family.Key}:{id.Value}>"))
        from admitted in guard(family.Admits(profile), ComponentFault.Family(key, $"<family-profile-mismatch:{family.Key}>"))
        from laned in guard(detail.IsSome == (family.Lane != DetailLane.None),
            ComponentFault.Family(key, $"<detail-lane-mismatch:{family.Key}:{family.Lane}>"))
        select new Component(family, id, profile, ifc, coring, standard, substanceId, appearanceId, detail);
}
```

## [03]-[SECTION_PROFILE]

- Owner: `SectionProfile` the closed cross-section `[Union]` — the `IfcParameterizedProfileDef` set carried natively with NAMED `PositiveMagnitude` dimensions, widened by the domain arms the ten families need (`CellularRectangle` per-cell voids, `Layered` plies, `Corrugated` deck fold, `Nominal` bond-line, `Catalogued` published identity, `Outline` typed free tail, `BuiltUp` positioned composition); `VoidCell` the fill-state cell row; `PlyRole` the BOUNDED layer-semantics vocabulary; `Ply` the laminate row.
- Cases: twenty-two arms; gross bounding `(WidthMm, DepthMm)` facts are BASE-CONSTRUCTOR STATE declared once per arm — no interior switch exists on this axis; the ONE dispatch site is `SectionSolver.Solve`. Growth cadence is the profile schema (buildingSMART parameterized-profile set), never thing cadence.
- Entry: construction is RAILED — each arm carries an `Of` factory returning `Fin<SectionProfile>` that lifts magnitudes through `key.AcceptValidated<PositiveMagnitude>` AND validates the non-magnitude slots (non-negative finite fillets/edges, finite slopes/offsets, cell containment PLUS pairwise cell disjointness — an overlapping lattice double-subtracts the net section and voids silently, so it faults at admission — wall-vs-envelope closure) on `ComponentFault.Dimension`; seed folds construct through `Of` INSIDE their `Traverse`, never a throwing lift. `Catalogued` constructs direct — its payload is already-admitted `SteelShape` state.
- Boundary: `VoidCell` is MIN-CORNER anchored in the profile's `[0,WidthMm]×[0,DepthMm]` corner frame (the convention `MasonryVoids.Cells` and the cmu lattice emit); `Grouted` drives the as-built net (only ungrouted cells void), the grouted fraction, self-weight, and the parallel-path thermal split; `Reinforced` marks bar-bearing cells; geometry-only cells collapse as-built physics to the ungrouted case. `Ply.Role` is the BOUNDED `PlyRole` row, never a free string: `Longitudinal`/`Transverse` are the timber structural discriminants, `Pane`/`Interlayer`/`Cavity` the glazing stack semantics, and the panel face/core rows carry appearance-facing policy. `ComponentFamily.Admits` proves the lane subset, so a known panel role cannot enter timber and a known timber role cannot enter glazing. The human-readable `IfcMaterialLayer.Name` is a BOUNDARY projection over `(Material, Role, ordinal)` — an unknown role is unrepresentable, and no consumer parses a role string. `NonNegativeMagnitude` does not exist in `Rasm.Numerics` (verified) — the non-negative/finite edge check is the inline `Edge` guard inside `Of` (R8 resolved).

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The bounded layer-semantics vocabulary a Ply carries: timber orientation, glazing stack position, and panel
// face/core material function share one identity regime and one IFC layer-name boundary. Facing states that the row
// is BONDED TO AN OUTWARD FACE of a build, which `Layered.Of` admits as a geometric invariant and `Layered.Facings`
// reads — every timber/glazing/core row is false because none of them faces.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlyRole {
    public static readonly PlyRole Longitudinal        = new("0",                       facing: false);
    public static readonly PlyRole Transverse          = new("90",                      facing: false);
    public static readonly PlyRole Pane                = new("pane",                    facing: false);
    public static readonly PlyRole Interlayer          = new("interlayer",              facing: false);
    public static readonly PlyRole Cavity              = new("cavity",                  facing: false);
    public static readonly PlyRole PaperFace           = new("paper-face",              facing: true);
    public static readonly PlyRole GlassMatFacer       = new("glass-mat-facer",         facing: true);
    public static readonly PlyRole FoilFacer           = new("foil-facer",              facing: true);
    public static readonly PlyRole GlassFiberMatFacer  = new("glass-fiber-mat-facer",   facing: true);
    public static readonly PlyRole CoatedGlassFacer    = new("coated-glass-facer",     facing: true);
    public static readonly PlyRole GlassMeshScrim      = new("glass-mesh-scrim",        facing: true);
    public static readonly PlyRole GypsumCore          = new("gypsum-core",             facing: false);
    public static readonly PlyRole VeneerPly           = new("veneer-ply",              facing: false);
    public static readonly PlyRole StrandLayer         = new("strand-layer",            facing: false);
    public static readonly PlyRole CementAggregateCore = new("cement-aggregate-core",   facing: false);
    public static readonly PlyRole FoamCore            = new("foam-core",               facing: false);
    public static readonly PlyRole MembraneCore        = new("membrane-core",           facing: false);
    public bool Facing { get; }
}

// The SECTION-TOPOLOGY discriminant every downstream physical law dispatches on, stored as a geometry-row column
// rather than re-derived per consumer: it selects the Saint-Venant torsion route (a thin OPEN wall carries Σb·t³/3,
// a CLOSED cell Bredt's law, a compact SOLID the Roark rectangle form), it decides whether a warping constant and a
// shear-centre offset can be non-zero at all, and it is what `ComponentRow.Sectioned` reads so section-map
// membership is DERIVED from the geometry instead of hand-asserted per seed row.
// `Composition` is the positioned built-up fold — no single outline exists to integrate — and `Unsectioned` the
// laminate and bond-line arms a section integral is meaningless over.
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

// --- [MODELS] ------------------------------------------------------------------------------
// Fill-state rides the cell: Grouted drives the as-built net section (only ungrouted cells void), the grouted-cell
// fraction, self-weight, and the parallel-path thermal split; Reinforced marks the bar-bearing cells. XMm/YMm are the
// MIN-CORNER in the profile's [0,Width]x[0,Depth] corner frame; WidthMm/HeightMm the first/second-axis extents.
public readonly record struct VoidCell(double XMm, double YMm, double WidthMm, double HeightMm, bool Grouted = false, bool Reinforced = false);

public readonly record struct Ply(MaterialId Material, PositiveMagnitude ThicknessMm, PlyRole Role);

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
            from disjoint in guard(Disjoint(cells), ComponentFault.Dimension(key, $"<cells-overlap:{widthMm:R}x{depthMm:R}>"))
            select (SectionProfile)new CellularRectangle(w, d, cells);

        // Pairwise disjointness over the UPPER TRIANGLE: the relation is symmetric, so the indexed projection binds
        // ONCE in the outer walk and each unordered pair is tested exactly once.
        static bool Disjoint(Seq<VoidCell> cells) =>
            toSeq(cells.Index()).ForAll(a => cells.Skip(a.Index + 1).ForAll(b => Apart(a.Item, b)));

        static bool Apart(VoidCell a, VoidCell b) =>
            a.XMm + a.WidthMm <= b.XMm || b.XMm + b.WidthMm <= a.XMm
            || a.YMm + a.HeightMm <= b.YMm || b.YMm + b.HeightMm <= a.YMm;
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

    // The taper-flange pair is PUBLISHED product data on every rolled S/HP/IPN/UPN section: FlangeMm the thickness at
    // the web face, FlangeToeMm the thickness at the free edge. A parallel-flange section carries them EQUAL and
    // every taper term downstream degenerates exactly, so one arm serves both families and no published root radius
    // or toe thickness is discarded. FlangeSlopeDeg is a DERIVED read off that pair — storing the slope beside the
    // two thicknesses is a third column whose value can contradict the two that determine it.
    public sealed record IShape(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double FlangeSlopeDeg => Math.Atan2(2.0 * (FlangeMm.Value - FlangeToeMm.Value), WidthMm.Value - WebMm.Value) * (180.0 / Math.PI);
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from te in key.Pm(flangeToeMm)
            from tapered in guard(te.Value <= tf.Value, ComponentFault.Dimension(key, $"<flange-toe-exceeds-root:{flangeToeMm:R}>"))
            from fits in guard(tw.Value < b.Value && 2.0 * tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - 2.0 * tf.Value),
                ComponentFault.Dimension(key, $"<i-profile-members-consume-envelope:{depthMm:R}x{widthMm:R}>"))
            select (SectionProfile)new IShape(d, b, tw, tf, f, te);
    }

    // Parameter names are the FABRICATION vocabulary a plate-girder seed states — top/bottom flange width and
    // thickness, web thickness — so a seed call site reads as the shop drawing it transcribes and a transposed
    // width-for-thickness argument is a named-argument mismatch at compile time rather than a silently solved girder.
    public sealed record AsymmetricIShape(PositiveMagnitude DepthMm, PositiveMagnitude TopFlangeWidthMm, PositiveMagnitude BottomFlangeWidthMm, PositiveMagnitude WebThicknessMm, PositiveMagnitude TopFlangeThicknessMm, PositiveMagnitude BottomFlangeThicknessMm, double FilletMm) : SectionProfile(Wider(TopFlangeWidthMm, BottomFlangeWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double topFlangeWidthMm, double bottomFlangeWidthMm, double webThicknessMm, double topFlangeThicknessMm, double bottomFlangeThicknessMm, double filletMm, Op key) =>
            from d in key.Pm(depthMm) from bt in key.Pm(topFlangeWidthMm) from bb in key.Pm(bottomFlangeWidthMm)
            from tw in key.Pm(webThicknessMm) from tt in key.Pm(topFlangeThicknessMm) from tb in key.Pm(bottomFlangeThicknessMm) from f in key.Edge(filletMm)
            from fits in guard(tw.Value < Math.Min(bt.Value, bb.Value) && tt.Value + tb.Value < d.Value
                    && 2.0 * f <= Math.Min(Math.Min(bt.Value, bb.Value) - tw.Value, d.Value - tt.Value - tb.Value),
                ComponentFault.Dimension(key, $"<asymmetric-i-members-consume-envelope:{depthMm:R}>"))
            select (SectionProfile)new AsymmetricIShape(d, bt, bb, tw, tt, tb, f);
    }

    public sealed record Channel(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double FlangeSlopeDeg => Math.Atan2(FlangeMm.Value - FlangeToeMm.Value, WidthMm.Value - WebMm.Value) * (180.0 / Math.PI);
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from te in key.Pm(flangeToeMm)
            from tapered in guard(te.Value <= tf.Value, ComponentFault.Dimension(key, $"<flange-toe-exceeds-root:{flangeToeMm:R}>"))
            from fits in guard(tw.Value < b.Value && 2.0 * tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - 2.0 * tf.Value),
                ComponentFault.Dimension(key, $"<channel-members-consume-envelope:{depthMm:R}x{widthMm:R}>"))
            select (SectionProfile)new Channel(d, b, tw, tf, f, te);
    }

    public sealed record ColdFormedC(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WallMm, PositiveMagnitude GirthMm, double InnerFilletMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double wallMm, double girthMm, double innerFilletMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(wallMm) from g in key.Pm(girthMm) from f in key.Edge(innerFilletMm)
            from lipped in guard(2.0 * t.Value < Math.Min(d.Value, b.Value) && g.Value + t.Value < b.Value && g.Value < d.Value / 2.0
                    && f <= Math.Min(b.Value - 2.0 * t.Value, d.Value / 2.0 - t.Value),
                ComponentFault.Dimension(key, $"<cee-wall-or-lip-consumes-envelope:{depthMm:R}x{widthMm:R}>"))
            select (SectionProfile)new ColdFormedC(d, b, t, g, f);
    }

    // A rolled tee is a split I, so it carries the SAME taper pair on both cut elements: the flange tapers toward its
    // toe and the stem toward its free edge, both PUBLISHED on the WT/MT/ST and split-IPN tables. Equal pairs
    // degenerate to the parallel-flange tee exactly.
    public sealed record Tee(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude WebMm, PositiveMagnitude FlangeMm, double FilletMm, PositiveMagnitude FlangeToeMm, PositiveMagnitude WebToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double MeanFlangeMm => (FlangeMm.Value + FlangeToeMm.Value) / 2.0;
        public double MeanWebMm => (WebMm.Value + WebToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double webMm, double flangeMm, double filletMm, double flangeToeMm, double webToeMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from tw in key.Pm(webMm) from tf in key.Pm(flangeMm)
            from f in key.Edge(filletMm) from fe in key.Pm(flangeToeMm) from we in key.Pm(webToeMm)
            from tapered in guard(fe.Value <= tf.Value && we.Value <= tw.Value,
                ComponentFault.Dimension(key, $"<tee-toe-exceeds-root:{flangeToeMm:R}:{webToeMm:R}>"))
            from fits in guard(tw.Value < b.Value && tf.Value < d.Value && 2.0 * f <= Math.Min(b.Value - tw.Value, d.Value - tf.Value),
                ComponentFault.Dimension(key, $"<tee-members-consume-envelope:{depthMm:R}x{widthMm:R}>"))
            select (SectionProfile)new Tee(d, b, tw, tf, f, fe, we);
    }

    public sealed record Angle(PositiveMagnitude DepthMm, PositiveMagnitude WidthMm, PositiveMagnitude ThicknessMm, double FilletMm, PositiveMagnitude LegToeMm) : SectionProfile(WidthMm, DepthMm) {
        public double MeanLegMm => (ThicknessMm.Value + LegToeMm.Value) / 2.0;

        public static Fin<SectionProfile> Of(double depthMm, double widthMm, double thicknessMm, double filletMm, double legToeMm, Op key) =>
            from d in key.Pm(depthMm) from b in key.Pm(widthMm) from t in key.Pm(thicknessMm)
            from f in key.Edge(filletMm) from e in key.Pm(legToeMm)
            from tapered in guard(e.Value <= t.Value, ComponentFault.Dimension(key, $"<angle-toe-exceeds-root:{legToeMm:R}>"))
            from fits in guard(t.Value < Math.Min(d.Value, b.Value) && 2.0 * f <= Math.Min(d.Value - t.Value, b.Value - t.Value),
                ComponentFault.Dimension(key, $"<angle-leg-consumes-envelope:{depthMm:R}x{widthMm:R}>"))
            select (SectionProfile)new Angle(d, b, t, f, e);
    }

    // A cold-formed Z is ONE sheet folded, so web and flanges share a single WallMm — a separate flange thickness is
    // a second value for one gauge that the typed IZ contract (one Thickness slot) could never carry anyway. The two
    // flange widths and two lip lengths are genuinely independent on a purlin, and the point-symmetric arm needs
    // both to place its shear centre.
    public sealed record Zed(PositiveMagnitude DepthMm, PositiveMagnitude TopFlangeWidthMm, PositiveMagnitude BottomFlangeWidthMm, PositiveMagnitude WallMm, double TopLipMm, double BottomLipMm, double InnerFilletMm) : SectionProfile(Wider(TopFlangeWidthMm, BottomFlangeWidthMm), DepthMm) {
        public static Fin<SectionProfile> Of(double depthMm, double topFlangeWidthMm, double bottomFlangeWidthMm, double thicknessMm, double topLipMm, double bottomLipMm, double innerFilletMm, Op key) =>
            from d in key.Pm(depthMm) from bt in key.Pm(topFlangeWidthMm) from bb in key.Pm(bottomFlangeWidthMm) from t in key.Pm(thicknessMm)
            from lt in key.Edge(topLipMm) from lb in key.Edge(bottomLipMm) from f in key.Edge(innerFilletMm)
            from fits in guard(t.Value < Math.Min(bt.Value, bb.Value) && 2.0 * t.Value < d.Value
                    && lt < d.Value / 2.0 && lb < d.Value / 2.0 && 2.0 * f <= d.Value - 2.0 * t.Value,
                ComponentFault.Dimension(key, $"<zed-members-consume-envelope:{depthMm:R}x{topFlangeWidthMm:R}>"))
            select (SectionProfile)new Zed(d, bt, bb, t, lt, lb, f);
    }

    // PJP/CJP groove-weld prep envelope; TopOffsetMm the one-sided bevel shift (signed, finite). The envelope span is
    // computed INSIDE Of and handed to a private constructor already proven, so the base initializer never runs a
    // THROWING PositiveMagnitude.Create over an unvalidated offset — the same posture Catalogued and BuiltUp take.
    public sealed record Trapezium : SectionProfile {
        private Trapezium(PositiveMagnitude bottomWidthMm, PositiveMagnitude topWidthMm, PositiveMagnitude depthMm, double topOffsetMm, PositiveMagnitude spanMm)
            : base(spanMm, depthMm) =>
            (BottomWidthMm, TopWidthMm, DepthMm, TopOffsetMm) = (bottomWidthMm, topWidthMm, depthMm, topOffsetMm);
        public PositiveMagnitude BottomWidthMm { get; }
        public PositiveMagnitude TopWidthMm { get; }
        public PositiveMagnitude DepthMm { get; }
        public double TopOffsetMm { get; }

        public static Fin<SectionProfile> Of(double bottomWidthMm, double topWidthMm, double depthMm, double topOffsetMm, Op key) =>
            from bb in key.Pm(bottomWidthMm) from bt in key.Pm(topWidthMm) from d in key.Pm(depthMm) from o in key.Slope(topOffsetMm)
            from span in key.Pm(Span(bb.Value, bt.Value, o))
            select (SectionProfile)new Trapezium(bb, bt, d, o, span);

        // The bevelled envelope: the top face shifted by the offset can overhang either side, so the span is the
        // extreme right edge minus the extreme left one.
        internal static double Span(double bottom, double top, double offset) {
            double topMin = (bottom - top) / 2.0 + offset;
            return Math.Max(bottom, topMin + top) - Math.Min(0.0, topMin);
        }
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

    // Boards, IGUs, membranes; a glazing seed passes widthMm = overallMm for the square gross projection.
    public sealed record Layered(Seq<Ply> Plies, PositiveMagnitude OverallMm, PositiveMagnitude WidthMm) : SectionProfile(WidthMm, OverallMm) {
        const double BuildToleranceMm = 0.5;

        // The facing plies of the stack, ordinal-ordered — the appearance and IFC layer-name readers take the stack's
        // own outward faces from here rather than re-deriving which roles happen to face out.
        public Seq<Ply> Facings => Plies.Filter(static p => p.Role.Facing);

        public static Fin<SectionProfile> Of(Seq<Ply> plies, double overallMm, double widthMm, Op key) =>
            from stacked in guard(!plies.IsEmpty, ComponentFault.Dimension(key, "<layered-profile-empty>"))
            from o in key.Pm(overallMm) from w in key.Pm(widthMm)
            // PlyRole.Facing is a GEOMETRIC claim, not a label: a facer is bonded to an outward face of the build, so
            // an interior ply carrying it describes a stack that cannot be manufactured. Admitting the claim here is
            // what makes the column load-bearing — every downstream reader takes facing presence as proven.
            from faced in guard(toSeq(plies.Index()).ForAll(p => !p.Item.Role.Facing || p.Index == 0 || p.Index == plies.Count - 1),
                ComponentFault.Dimension(key, "<layered-profile-interior-facer>"))
            from closed in guard(Math.Abs(plies.Sum(static p => p.ThicknessMm.Value) - o.Value) <= BuildToleranceMm,
                ComponentFault.Dimension(key, $"<layered-profile-build-mismatch:{overallMm:R}>"))
            select (SectionProfile)new Layered(plies, o, w);
    }

    // Continuous joint / adhesive bond-line — unsectioned by design.
    public sealed record Nominal(PositiveMagnitude NominalMm) : SectionProfile(NominalMm, NominalMm) {
        public static Fin<SectionProfile> Of(double nominalMm, Op key) =>
            key.Pm(nominalMm).Map(static n => (SectionProfile)new Nominal(n));
    }

    // The VividOrange published identity — payload pre-admitted by SteelShape.Of, so construction is direct.
    public sealed record Catalogued(SteelShape Shape) : SectionProfile(Shape.Section.WidthMm, Shape.Section.DepthMm);

    // The free tail — a closed RING of neutral (y, z) millimetre vertices, with the connector stamped-plate
    // silhouette the landed instance. The ring is the SOURCE and `Curves.Free` mints the VividOrange `Perimeter` from
    // it at the integral, because `Perimeter` publishes no vertex read-back: storing only the carrier makes the
    // boundary write-only, and every fold that needs the real geometry — the exact plastic modulus, the torsion
    // envelope — is then forced onto a shape-factor guess. Topology is DECLARED by the seed that knows whether it
    // stamped a solid silhouette or folded a thin plate, since no polygon predicate can recover that intent.
    public sealed record Outline(Seq<(double YMm, double ZMm)> Ring, ProfileTopology Topology, PositiveMagnitude WidthMm, PositiveMagnitude DepthMm) : SectionProfile(WidthMm, DepthMm) {
        public static Fin<SectionProfile> Of(Seq<(double YMm, double ZMm)> ring, ProfileTopology topology, double widthMm, double depthMm, Op key) =>
            from closed in guard(ring.Count >= 3, ComponentFault.Dimension(key, $"<outline-ring-degenerate:{ring.Count}>"))
            from finite in guard(ring.ForAll(static v => double.IsFinite(v.YMm) && double.IsFinite(v.ZMm)),
                ComponentFault.Dimension(key, "<outline-ring-nonfinite>"))
            from free in guard(topology == ProfileTopology.SolidPolygon || topology == ProfileTopology.OpenThin,
                ComponentFault.Dimension(key, $"<outline-topology-unadmitted:{topology.Key}>"))
            from w in key.Pm(widthMm) from d in key.Pm(depthMm)
            select (SectionProfile)new Outline(ring, topology, w, d);
    }

    // The positioned built-up composition — the IfcCompositeProfileDef modality: already-admitted member arms placed by signed centroid
    // offsets in the composite frame (DyMm across the width axis, DzMm across the depth axis). The OPEN welded-assembly space — plate
    // girders, battened/laced columns, toe-to-toe double channels, cranked built-up chords — solved by parallel-axis composition of the
    // members' own Solve arms; a closed built-up cell is RectangleHollow's Bredt law, and nesting is rejected so composition stays one
    // level deep. The unsectioned Layered/Nominal arms cannot member a composition.
    // The envelope spans are computed INSIDE Of and handed to a private constructor already proven, so the base
    // initializer never re-runs a throwing PositiveMagnitude.Create over unvalidated parts — the same posture
    // Catalogued takes with its pre-admitted SteelShape state.
    public sealed record BuiltUp : SectionProfile {
        private BuiltUp(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts, PositiveMagnitude widthMm, PositiveMagnitude depthMm)
            : base(widthMm, depthMm) => Parts = parts;
        public Seq<(SectionProfile Part, double DyMm, double DzMm)> Parts { get; }

        public static Fin<SectionProfile> Of(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts, Op key) =>
            from stacked in guard(parts.Count >= 2, ComponentFault.Dimension(key, $"<built-up-needs-two-members:{parts.Count}>"))
            from placed in guard(parts.ForAll(static p => double.IsFinite(p.DyMm) && double.IsFinite(p.DzMm)),
                ComponentFault.Dimension(key, "<built-up-offset-nonfinite>"))
            from membered in guard(parts.ForAll(static p => p.Part is not (SectionProfile.Layered or SectionProfile.Nominal or SectionProfile.BuiltUp)),
                ComponentFault.Dimension(key, "<built-up-member-unsectionable-or-nested>"))
            from w in key.Pm(Span(parts, static p => (p.DyMm, p.Part.GrossRectangleMm.WidthMm.Value)))
            from d in key.Pm(Span(parts, static p => (p.DzMm, p.Part.GrossRectangleMm.DepthMm.Value)))
            select (SectionProfile)new BuiltUp(parts, w, d);

        // The composite envelope along ONE axis: the extreme member face above minus the extreme face below, the
        // selector naming that axis's offset and gross extent so both spans are one derivation, never two.
        static double Span(Seq<(SectionProfile Part, double DyMm, double DzMm)> parts, Func<(SectionProfile Part, double DyMm, double DzMm), (double Offset, double Extent)> axis) =>
            parts.Map(axis).Fold((Hi: double.NegativeInfinity, Lo: double.PositiveInfinity),
                    static (bounds, member) => (Math.Max(bounds.Hi, member.Offset + member.Extent / 2.0), Math.Min(bounds.Lo, member.Offset - member.Extent / 2.0)))
                is var span ? span.Hi - span.Lo : 0.0;
    }

    // Case identity, topology, the outline mint, and the section mint are FOUR COLUMNS off ONE row: `SectionGeometry`
    // owns the single exhaustive Switch over this closed axis, and a further arm breaks THERE at compile time. Three
    // lockstep 22-arm switches over one axis — one naming the case, one minting the outline for the RC path, one
    // minting the section — are one dispatch written three times whose arms drift independently the moment a mapping
    // moves on one of them; the geometry row is the page's own five-delegate policy shape applied to the profile axis.
    public string Case => SectionGeometry.Of(this).Case;
    public ProfileTopology Topology => SectionGeometry.Of(this).Topology;

    static PositiveMagnitude Wider(PositiveMagnitude a, PositiveMagnitude b) => a.Value >= b.Value ? a : b;
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The three admission kernels every SectionProfile.Of composes: Pm the kernel PositiveMagnitude lift, Edge the
// non-negative finite slot (fillets/edges — NonNegativeMagnitude has no Rasm.Numerics owner, so the guard is inline),
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

- Owner: `SectionSolver` — ONE solver replaces every per-family perimeter builder: one generated exhaustive `Switch` over the closed profile axis routes the ONE `new SectionProperties((IProfile)…)` Green's-theorem integral (decompile-verified: both `SectionProperties(IProfile)` and `SectionProperties(ISection)` constructors exist) through the ONE twenty-column `Admit` lift plus a `SectionSupplement` topology row; `Curves` the per-arm profile table; `Forms` the per-arm supplement table.
- Cases: `Curves` lowers each arm onto a verified `VividOrange.IProfiles` contract or a typed `Perimeter`. The `Perimeter` path owns multi-void, corrugated, asymmetric-trapezium, triangle, and free-outline geometry; `Trapezium.TopOffsetMm` therefore changes the integrated centroid and moments instead of disappearing through the symmetric `ITrapezoid` contract.
- Entry: `SectionSolver.Solve(profile, key)` and `SectionSolver.ProfileOf(profile, key)` are two COLUMN READS over the one `SectionGeometry.Of` row; `ProfileOf` is the PROFILE-FAITHFUL RC-outline entry `reinforcement#RC_SECTION` feeds, and because both reads take the SAME outline column, a circular column feeds `ConcreteSection` its true `ICircle`, a trapezoidal member its integrated `Perimeter`, and a cellular unit its void-bearing perimeter whose grouted cells are already filled — so a partly-grouted unit cannot present as solid stock on the RC path while presenting its true net on the elastic one.
- Boundary: `SectionSolver.Solve` is the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.SectionSolve(Key, Profile, Section, Elapsed)` tap SUBJECT and the `Projection/benchmarks#BENCH_CORPUS` `BenchKernel.SectionSolve` measured kernel; the tap is a composition-root decorator over `MaterialsHooks.SectionSolve`, so this owner emits nothing, carries no `Duration`, and references no signal type — the seam is declared at both ends and instrumented at neither.
- Boundary: `Forms` states each arm's MIDLINE and STRIP STACK and nothing else — `ThinWalled` derives every open arm's shear centre, warping constant, and Saint-Venant torsion from its own midline, and `Plastic` integrates the exact equal-area-axis first moment over the strip stack, so no arm carries a shape factor against an elastic modulus and no arm transcribes a warping expression. `FromCatalogue` LOWERS a published identity onto the same parametric arm a fabricated section takes, its `SectionDims` columns already-proven magnitudes, so one algebra prices a rolled W and a welded plate girder; `Areas`/`Inertiae` remain the carrier-free `.Utility` reads the free-outline torsion envelope needs. `Layered`/`Nominal` are unsectioned by design and their geometry row carries the typed refusal in both columns, so `Sectioned` derives false for them rather than a seed asserting it. A supplement column that is non-positive or non-finite rails `ComponentFault.Section` at the lift, and the lazy carrier reads trap in ONE `Try` lift so a degenerate `Outline` perimeter's THROW rails the same slot (NaN and throw alike) — a degenerate section never seeds the seam and no `VividOrange` exception escapes the boundary.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The plastic/torsion/shear/warping/asymmetry supplement the elastic polygon integral cannot yield. A doubly-symmetric
// solid or closed section places its shear centre AT the centroid and carries no warping couple, so its last four
// columns are engineering-zero; the `ThinWalled` sectorial kernel fills them for every open arm.
// SHEAR-AREA CONVENTION, declared ONCE for the whole column: Avy and Avz are the SHEAR-EFFECTIVE areas A/κ over the
// section's own elastic shear form factor κ — the area across which the MEAN shear stress equals the section's
// MAXIMUM. One convention spans every arm (κ = 1 a thin web carrying uniform shear, 3/2 a solid rectangle, 4/3 a
// solid circle, 2 a thin round tube), so a consumer dividing a shear demand by this column reads ONE physical
// quantity on every topology, and a thick tube approaching solid stock reads continuously against the bar it becomes.
public readonly record struct SectionSupplement(
    double Zx, double Zy, double J, double Avy, double Avz,
    double Iw = 0.0, double ShearCentreY = 0.0, double ShearCentreZ = 0.0, double Monosymmetry = 0.0);

// The ONE per-arm policy row over the closed profile axis: the case token every telemetry dimension and analytics
// column names, the topology discriminant the torsion route and section-map membership read, the outline mint the
// RC-outline consumer and the solver's own integrand SHARE, and the section mint. `SectionGeometry.Of` is the single
// exhaustive generated Switch producing it, so a new profile arm is compiler-forced at exactly one site and the four
// columns can never disagree about which arm they describe.
public readonly record struct ProfileGeometry(
    string Case,
    ProfileTopology Topology,
    Func<Op, Fin<IProfile>> Outline,
    Func<Op, Fin<ComputedSection>> Section);

// THE ONE DISPATCH over the closed profile axis. Every arm lands one `ProfileGeometry` row, and `Solve`, `ProfileOf`,
// `SectionProfile.Case`, and `SectionProfile.Topology` are COLUMN READS over it — so a new arm is compiler-forced
// HERE and nowhere else, and the case token, the outline, and the section can never describe different arms.
// The `Outline` column is SHARED by the solver's own integrand and the RC-outline consumer, which is what closes the
// gross-substitution hole: a cellular unit's outline is its void-bearing perimeter on BOTH paths, and because
// `Curves.RectWithVoids` filters GROUTED cells out of the void list, a fully-grouted unit degenerates to the solid
// gross rectangle by construction while an ungrouted one keeps its voids. An ungrouted CMU therefore cannot reach an
// RC solver as solid stock — the grout state IS the gate, not a flag beside it.
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
            // The catalogued arm reads its topology off the ADMITTED steel class rather than re-deciding it, so the
            // published taxonomy is the one owner of whether a section is open, closed, or solid stock.
            catalogued:        static c => Solid(nameof(SectionProfile.Catalogued), TopologyOf(c.Shape.Class.Topology), c, () => c.Shape.Profile, () => Forms.FromCatalogue(c.Shape)),
            outline:           static o => Solid(nameof(SectionProfile.Outline), o.Topology, o, () => Curves.Free(o), () => Forms.Integrated(o)),
            builtUp:           static b => new ProfileGeometry(nameof(SectionProfile.BuiltUp), ProfileTopology.Composition,
                                   key => ComponentFault.Section(key, "<built-up-profile-no-single-outline>"),
                                   key => SectionSolver.Compose(b, key)));

    // A solvable arm: ONE `Try` lift wraps outline construction, the Green's-theorem integral, AND the supplement, so
    // a degenerate geometry that THROWS and one that nets NaN rail the identical typed fault and no VividOrange
    // exception escapes the owning boundary.
    static ProfileGeometry Solid(string @case, ProfileTopology topology, SectionProfile source, Func<IProfile> outline, Func<SectionSupplement> supplement) =>
        new(@case, topology,
            key => Try.lift(outline).Run().MapFail(e => (Error)ComponentFault.Section(key, $"<section-outline-throw:{e.Message}>")),
            key => SectionSolver.Admit(outline, source, supplement, key));

    static ProfileGeometry Unsectioned(string @case) =>
        new(@case, ProfileTopology.Unsectioned,
            key => ComponentFault.Section(key, $"<unsectioned-profile:{@case}>"),
            key => ComponentFault.Section(key, $"<unsectioned-profile:{@case}>"));

    static ProfileTopology TopologyOf(SteelTopology topology) => topology.Map(
        open: ProfileTopology.OpenThin, closed: ProfileTopology.ClosedThin, solid: ProfileTopology.SolidPolygon);
}

// The section spine: two column reads over the ONE geometry row plus the positioned-composition fold no single
// outline can express. Every solvable arm routes the ONE VividOrange Green's-theorem integral through the ONE
// twenty-column `Admit` lift, then its topology supplement.
public static class SectionSolver {
    public static Fin<ComputedSection> Solve(SectionProfile profile, Op key) => SectionGeometry.Of(profile).Section(key);

    // The RC concrete-outline entry the reinforcement RC path and capacity.md feed, PROFILE-FAITHFUL: it is the SAME
    // outline column the solver integrates, so a circular column or drilled shaft feeds `ConcreteSection` its true
    // `ICircle` and a trapezoidal member its integrated `Perimeter` (VividOrange `ConcreteSection` and
    // `FaceReinforcementLayer.GetPath(IProfile, …)` are profile-polymorphic) rather than a bounding rectangle that
    // would starve `RebarLayout.PerimeterCount` of its natural geometry. Arm dimensional admission rides each
    // `SectionProfile` case's own base-constructor proof, so no dimension re-proves here.
    public static Fin<IProfile> ProfileOf(SectionProfile profile, Op key) => SectionGeometry.Of(profile).Outline(key);

    // The BuiltUp composition: each member solves through its OWN Solve arm (fillets, voids, catalogued identities intact), then the
    // composite folds — ΣA, the area-weighted centroid, Steiner Ix/Iy, extreme-fibre elastic moduli, the equal-area-axis plastic fold
    // (a one-side member transfers A·|d|, a straddling member its own Z plus the uniform-band A/Ext·d² correction — exact for the plate
    // members that dominate fabricated sections), ΣJ (open thin-walled composition — a closed built-up cell is RectangleHollow's), the
    // Σ shear-area pair, and Σ perimeter (member contact lines not deducted — the conservative heated bound). Iw and the shear-centre/
    // monosymmetry columns stay engineering-zero, so the F2 LTB rts read falls back to ry — the conservative open-composition floor.
    // EXPRESSION_SPINE measured-kernel exemption: the composite scalars bind once, one Section-lift chain exits.
    internal static Fin<ComputedSection> Compose(SectionProfile.BuiltUp b, Op key) =>
        b.Parts.Traverse(p => Solve(p.Part, key).Map(cs => (S: cs, p.DyMm, p.DzMm))).As().Bind(members => {
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
                from area in Section(a, key)
                from ixV in Section(ix, key)
                from iyV in Section(iy, key)
                from sx in Section(ix / fibreZ, key)
                from sy in Section(iy / fibreY, key)
                from rx in Section(Math.Sqrt(ix / a), key)
                from ry in Section(Math.Sqrt(iy / a), key)
                from zxV in Section(zx, key)
                from zyV in Section(zy, key)
                from j in Section(members.Sum(static m => m.S.JMm4.Value), key)
                from avy in Section(members.Sum(static m => m.S.AvyMm2.Value), key)
                from avz in Section(members.Sum(static m => m.S.AvzMm2.Value), key)
                from perim in Section(members.Sum(static m => m.S.HeatedPerimeterMm.Value), key)
                select new ComputedSection(area, ixV, iyV, sx, sy, rx, ry, zxV, zyV, j, IwMm6: 0.0, avy, avz,
                    DepthMm: b.GrossRectangleMm.DepthMm, WidthMm: b.GrossRectangleMm.WidthMm, HeatedPerimeterMm: perim,
                    AxisDistanceMm: 0.0, ShearCentreYMm: 0.0, ShearCentreZMm: 0.0, MonosymmetryFactor: 0.0);
        });

    // The twenty-column PositiveMagnitude lift, one for every solvable arm. The outline construction, the lazy
    // Green's-theorem reads, AND the supplement all trap in ONE `Try`: a degenerate geometry faults
    // ComponentFault.Section whether it THROWS or nets NaN, so no VividOrange exception escapes the owning boundary
    // and the outline is built exactly once per solve.
    // Depth/Width are the arm's proven base-constructor gross pair (never re-proven); every solver/supplement column
    // re-labels its value-object Fin onto the Section slot (a degenerate net column is a SECTION failure, not a raw
    // dimension input); the four signed columns admit on a single finiteness gate. AxisDistanceMm stays 0.0 — the RC
    // cover rides the reinforcement#RC_SECTION ConcreteSectionProperties path.
    internal static Fin<ComputedSection> Admit(Func<IProfile> outline, SectionProfile source, Func<SectionSupplement> supplement, Op key) =>
        from p in Try.lift(() => {
                SectionProperties carrier = new(outline());
                return (Area: carrier.Area.SquareMillimeters,
                    Ix: carrier.MomentOfInertiaYy.MillimetersToTheFourth, Iy: carrier.MomentOfInertiaZz.MillimetersToTheFourth,
                    Sx: carrier.ElasticSectionModulusYy.CubicMillimeters, Sy: carrier.ElasticSectionModulusZz.CubicMillimeters,
                    Rx: carrier.RadiusOfGyrationYy.Millimeters, Ry: carrier.RadiusOfGyrationZz.Millimeters,
                    Perim: carrier.Perimeter.Millimeters, Supplement: supplement());
            }).Run().MapFail(e => (Error)ComponentFault.Section(key, $"<section-integral-throw:{e.Message}>"))
        let s = p.Supplement
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
        from signed in guard(s.Iw >= 0.0 && double.IsFinite(s.Iw) && double.IsFinite(s.ShearCentreY) && double.IsFinite(s.ShearCentreZ) && double.IsFinite(s.Monosymmetry),
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
        // The typed parametric contracts carry ONE flange-thickness slot, so a taper-flange arm hands them its
        // AREA-EXACT mean thickness — the published root and toe values reach the elastic integral as the section
        // area and inertia they actually produce, instead of the root value alone over-stating both, and the exact
        // taper geometry rides the `Forms` strip stack and the `ThinWalled` midline where it changes the answer.
        public static IProfile I(SectionProfile.IShape i) => new IBeamRow(Mm(i.DepthMm.Value), Mm(i.WidthMm.Value), Mm(i.MeanFlangeMm), Mm(i.WebMm.Value), Mm(i.FilletMm));
        public static IProfile AsymI(SectionProfile.AsymmetricIShape i) =>   // ICustomI carries no fillet slot; FilletMm rides the Forms midline
            new MonoIRow(Mm(i.DepthMm.Value), Mm(i.TopFlangeWidthMm.Value), Mm(i.BottomFlangeWidthMm.Value), Mm(i.TopFlangeThicknessMm.Value), Mm(i.BottomFlangeThicknessMm.Value), Mm(i.WebThicknessMm.Value));
        public static IProfile U(SectionProfile.Channel u) => new ChannelRow(Mm(u.DepthMm.Value), Mm(u.WidthMm.Value), Mm(u.WebMm.Value), Mm(u.MeanFlangeMm));
        public static IProfile Cee(SectionProfile.ColdFormedC c) => new CeeRow(Mm(c.DepthMm.Value), Mm(c.WidthMm.Value), Mm(c.WallMm.Value), Mm(c.WallMm.Value), Mm(c.GirthMm.Value));
        public static IProfile T(SectionProfile.Tee t) => new TeeRow(Mm(t.DepthMm.Value), Mm(t.WidthMm.Value), Mm(t.MeanWebMm), Mm(t.MeanFlangeMm));
        public static IProfile L(SectionProfile.Angle l) => new AngleRow(Mm(l.DepthMm.Value), Mm(l.WidthMm.Value), Mm(l.MeanLegMm), Mm(l.MeanLegMm));
        public static IProfile Z(SectionProfile.Zed z) =>                    // one folded sheet: the IZ Thickness slot IS WallMm
            new ZedRow(Mm(z.DepthMm.Value), Mm(z.TopFlangeWidthMm.Value), Mm(z.BottomFlangeWidthMm.Value), Mm(z.WallMm.Value), Mm(z.TopLipMm), Mm(z.BottomLipMm));

        // The free ring lifted into the typed carrier at the integral — the neutral vertices are the stored source,
        // this is their one projection.
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
    // The supplement table. TWO kernels carry every arm that once hand-typed a closed form: `Plastic` integrates the
    // EXACT equal-area-axis first moment over the arm's own strip stack, and `ThinWalled` derives the shear centre,
    // the warping constant, and the Saint-Venant torsion of every open arm from its own midline. A shape-factor
    // multiplier against a kernel ELASTIC modulus is a guess at an integral this page can take exactly, and a
    // per-shape warping transcription can cancel to a geometry-independent constant with nothing type-checking it —
    // both failure modes are unrepresentable once the integral and the sectorial sweep are the only sources.
    internal static class Forms {
        // The shared OPEN-arm assembler: the sectorial kernel over the midline supplies J, I_w, and BOTH shear-centre
        // offsets; the two strip stacks supply the exact plastic moduli; the shear pair states the arm's own κ.
        // β_y is DERIVED from the located shear centre (β_y = 1.8·z_s, the SN030 0.9·h₀·(2ψ_f − 1) written against a
        // shear centre the kernel found instead of a flange-inertia fraction re-derived per shape), so a shape whose
        // shear centre sits at the centroid answers zero by construction.
        static SectionSupplement Open(Seq<WallSegment> midline, Seq<Strip> major, Seq<Strip> minor, double avy, double avz) {
            OpenWall wall = ThinWalled.Of(midline);
            return new(Plastic.Modulus(major), Plastic.Modulus(minor), wall.Torsion, avy, avz,
                Iw: wall.Warping, ShearCentreY: wall.ShearCentreY, ShearCentreZ: wall.ShearCentreZ,
                Monosymmetry: 1.8 * wall.ShearCentreZ);
        }

        public static SectionSupplement SolidRectangle(SectionProfile.Rectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);
        public static SectionSupplement SolidRounded(SectionProfile.RoundedRectangle r) => SolidRect(r.WidthMm.Value, r.DepthMm.Value);   // corner relief is the elastic integral's; plastic and J keep the gross-rectangle bound

        // --- [OPEN_THIN_WALLED]
        // Each open arm states its MIDLINE and its two STRIP STACKS and nothing else — the physics is the kernels'.
        // A taper flange enters both: the midline segment carries the area-exact mean thickness, and the strip stack
        // carries the true trapezoidal band (full width over the toe depth, then tapering to the web over the root
        // depth), so a published toe thickness changes the plastic modulus instead of being discarded. A
        // parallel-flange section passes root == toe and every taper band collapses to zero height exactly.
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

        // Mono-symmetric I: the SAME midline with two flange geometries. The shear centre and β_y fall out of the
        // sweep rather than out of a flange-inertia fraction re-derived here.
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

        // Channel: flanges on ONE side of the web, which is exactly why the sectorial sweep is load-bearing here —
        // the shear centre lies OUTSIDE the web, and its offset is what a cancelling closed form silently pinned to a
        // constant for every geometry.
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

        // Lipped cee: the channel midline plus its two return lips, so the lip contribution to the warping constant
        // and to the shear-centre offset is integrated rather than omitted.
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

        // Tee: the stem is the major shear web, the flange the minor. Both elements carry their published taper.
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

        // Angle: two legs meeting at the heel. The shear centre is LOCATED by the sweep rather than asserted at the
        // heel, so β_y follows the same 1.8·z_s law every other singly-symmetric arm answers.
        public static SectionSupplement OpenAngle(SectionProfile.Angle l) {
            double d = l.DepthMm.Value, b = l.WidthMm.Value, t = l.ThicknessMm.Value, e = l.LegToeMm.Value, m = l.MeanLegMm;
            return Open(
                Seq(Wall(-1, 0.0, 0.0, 0.0, d - m / 2.0, m),
                    Wall(-1, 0.0, 0.0, b - m / 2.0, 0.0, m)),
                Seq(Band(0.0, m, b), Taper(m, d, t, e)),
                Seq(Band(0.0, m, d), Taper(m, b, t, e)),
                d * m, b * m);
        }

        // Point-symmetric lipped Z: flanges on OPPOSITE sides, so the sweep answers a shear centre AT the centroid
        // and both offsets fall to zero without the arm asserting it.
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
        // The doubly-symmetric flanged stacks both I and channel share. MAJOR slices along the depth: the flange
        // presents its full width over the TOE depth and then tapers to the web over the remaining root depth.
        // MINOR slices along the width: outside the web the material extent is twice the local flange thickness,
        // across the web it is the full depth.
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

        // The net superposition over UNGROUTED cells (a grouted cell integrates as solid): plastic first-moment
        // subtraction about the gross centroidal axes with the exact spans-the-axis/one-side piecewise term (deeper
        // exact for a cell straddling the axis and for one wholly to a side), Roark J subtraction, net shear area.
        public static SectionSupplement NetRectangle(SectionProfile.CellularRectangle c) {
            double w = c.WidthMm.Value, d = c.DepthMm.Value;
            Seq<VoidCell> open = c.Cells.Filter(static v => !v.Grouted);
            double zx = w * d * d / 4.0 - open.Sum(v => PlasticCut(v.WidthMm, v.HeightMm, v.YMm + v.HeightMm / 2.0 - d / 2.0));
            double zy = d * w * w / 4.0 - open.Sum(v => PlasticCut(v.HeightMm, v.WidthMm, v.XMm + v.WidthMm / 2.0 - w / 2.0));
            double j = RectJ(w, d) - open.Sum(static v => RectJ(v.WidthMm, v.HeightMm));
            double net = w * d - open.Sum(static v => v.WidthMm * v.HeightMm);
            return new(zx, zy, j, 2.0 * net / 3.0, 2.0 * net / 3.0);   // κ = 3/2 over the NET area — the voided rectangle's own form factor
        }
        // First moment of a b-wide, h-tall cut whose centre sits yBar off the axis: straddling -> b*(h²/4 + yBar²),
        // fully one side -> b*h*|yBar| — the exact |y| strip integral.
        static double PlasticCut(double b, double h, double yBar) =>
            Math.Abs(yBar) < h / 2.0 ? b * (h * h / 4.0 + yBar * yBar) : b * h * Math.Abs(yBar);

        public static SectionSupplement BoxTube(SectionProfile.RectangleHollow r) {
            double b = r.WidthMm.Value, h = r.DepthMm.Value, t = r.WallMm.Value, bi = b - 2.0 * t, hi = h - 2.0 * t;
            return new(0.25 * (b * h * h - bi * hi * hi), 0.25 * (h * b * b - hi * bi * bi),
                ClosedRectJ(b, h, t, r.InnerFilletMm, r.OuterFilletMm), 2.0 * h * t, 2.0 * b * t);   // κ = 1 on each wall pair
        }

        public static SectionSupplement SolidCircle(SectionProfile.Circle c) {
            double dia = c.DiameterMm.Value, a = Math.PI * dia * dia / 4.0;
            return new(dia * dia * dia / 6.0, dia * dia * dia / 6.0, Math.PI * Math.Pow(dia, 4.0) / 32.0,
                0.75 * a, 0.75 * a);   // κ = 4/3, the solid-circle form factor — the gross area is the TUBE's discontinuity
        }

        // Round tube: plastic (D³ − Di³)/6, polar J, and κ = 2 — the thin-tube shear form factor, the same convention
        // the solid bar answers at 4/3 rather than a second reading of the same column.
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
                Math.PI * Math.Pow(a, 3.0) * Math.Pow(b, 3.0) / (a * a + b * b), 0.75 * area, 0.75 * area);   // κ = 4/3
        }

        // The SOLID free arms take the EXACT plastic integral over their own polygon: a trapezoid, a weld-fillet
        // triangle, and a stamped silhouette all decompose into the trapezoidal strips `Plastic` folds, so the 1.5,
        // 2.34, and 1.0 shape-factor policies against an elastic modulus retire whole. Saint-Venant torsion DISPATCHES
        // on the declared topology: a compact polygon takes A⁴/(40·I_p), a thin open plate the (L·t³)/3 developed
        // form over its own equivalent length and thickness (L = P/2, t = 2A/P). Pricing a folded plate by the
        // compact form over-states its torsional stiffness several-fold, and no polygon predicate can recover whether
        // a silhouette was stamped solid or folded thin — which is why the topology is DECLARED at the arm.
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
                    : Math.Pow(a, 4.0) / (40.0 * Math.Max(ip, double.Epsilon)),
                2.0 * a / 3.0, 2.0 * a / 3.0);   // κ = 3/2, the solid-polygon shear form factor
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

        // A simple polygon's slice MEASURE is piecewise-linear between consecutive vertex ordinates, so the ring folds
        // into one exact strip per band. The two selectors name which coordinate slices and which one measures, so
        // ONE projection serves both principal axes rather than a transposed copy of the ring.
        static Seq<Strip> RingStrips(Seq<(double YMm, double ZMm)> ring,
            Func<(double YMm, double ZMm), double> along, Func<(double YMm, double ZMm), double> across) {
            Seq<double> levels = toSeq(ring.Map(along).Distinct().OrderBy(static z => z));
            return toSeq(Enumerable.Range(0, Math.Max(0, levels.Count - 1)))
                .Map(i => BandOf(ring, along, across, levels[i], levels[i + 1]));
        }

        // The measure is LINEAR across a band bounded by consecutive vertex ordinates, so two interior samples
        // determine it exactly — sampling AT a vertex level is ambiguous where an edge begins or ends there.
        static Strip BandOf(Seq<(double YMm, double ZMm)> ring,
            Func<(double YMm, double ZMm), double> along, Func<(double YMm, double ZMm), double> across, double lo, double hi) {
            double span = hi - lo;
            double w1 = Measure(ring, along, across, lo + span / 3.0), w2 = Measure(ring, along, across, lo + 2.0 * span / 3.0);
            return new(lo, hi, 2.0 * w1 - w2, 2.0 * w2 - w1);
        }

        // The total width of material at one level: every edge crossing it contributes a station, and the sorted
        // stations pair into occupied intervals — so a multi-lobed silhouette measures its real material, never its
        // bounding chord.
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

        // Thin-fold deck: the idealized rib trace as three additive strip families — bottom flats, the two inclined
        // web legs presenting their own horizontal chord g/sinθ, and top flats — plus the developed-length thin-open
        // J. The fold repeats, so it carries no single-cell warping couple and no shear-centre offset.
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

        // The published-identity route LOWERS a catalogued shape onto the SAME parametric arm every fabricated
        // section takes, so one algebra prices a rolled W and a welded plate girder. The lowering re-admits nothing:
        // `SectionDims` columns are already proven `PositiveMagnitude`s, so the arm records construct directly.
        // TAPER: `SectionDims` carries one flange thickness because the typed catalogue contracts publish one, so an
        // IPN/UPN taper-flange identity is priced as its PARALLEL-FLANGE equivalent at the published root thickness —
        // stated here rather than silently, and closed the moment a catalogue surface publishes the toe.
        // COLD-FORMED: a catalogued cold-formed identity publishes no lip, so it lowers to the UNLIPPED channel —
        // the conservative open section, never a fabricated stiffener.
        // COMPOSITE: this row prices the BARE STEEL core alone. That is the honest floor and it is load-bearing —
        // `steel#STEEL_FAMILY` `SteelDesign.CompositeMn` owns the Chapter I plastic couple and FLOORS it at exactly
        // this value, so the slab contributes to capacity where the code grants it and never to the elastic section.
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

        // Two angles across a gap: the major columns double outright, and the minor plastic modulus takes each leg's
        // own modulus PLUS its exact area transfer across the half-separation, which is the equal-area result for a
        // symmetric pair straddling the axis. The pair is symmetric, so the heel offset cancels to zero.
        static SectionSupplement Doubled(SectionSupplement one, double separationMm, double legAreaMm2) =>
            one with {
                Zx = 2.0 * one.Zx,
                Zy = 2.0 * (one.Zy + legAreaMm2 * separationMm / 2.0),
                J = 2.0 * one.J, Avy = 2.0 * one.Avy, Avz = 2.0 * one.Avz, Iw = 2.0 * one.Iw,
                ShearCentreY = 0.0,
            };

        // Roark Table 10.1 solid-rectangle St-Venant J (a/b the long/short side) and the Bredt closed thin-walled
        // tube J = 4·A_m²·t / p_m over the MID-LINE, whose corners the wall's own fillet pair rounds — an inside and
        // an outside radius shrink the enclosed area by (4 − π)·r_m² and shorten the mid-line perimeter, so the
        // published corner geometry of a rolled HSS changes its torsion constant instead of sitting inert.
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
        // κ = 3/2 for a solid rectangle: the elastic shear form factor, one convention with every other arm.
        static SectionSupplement SolidRect(double w, double d) { double a = w * d; return new(w * d * d / 4.0, d * w * w / 4.0, RectJ(w, d), 2.0 * a / 3.0, 2.0 * a / 3.0); }
    }
}

// --- [KERNELS] -------------------------------------------------------------------------------
// The open-thin-walled SECTORIAL kernel: shear centre, warping constant, and Saint-Venant torsion for EVERY open arm,
// derived from that arm's own MIDLINE rather than from a per-shape transcribed formula. One derivation over one input
// replaces seven per-shape warping and shear-centre expressions — and a transcription that CANCELS to a
// geometry-independent constant (a warping constant substituted into its own shear-centre expression divides itself
// out) is unrepresentable here, because nothing in this kernel is per-shape.
// The algebra is classical: the sectorial coordinate sweeps the midline about a provisional pole, normalizes to zero
// mean over the wall, and its first moments against the principal coordinates LOCATE the shear centre; re-swept about
// that pole and re-normalized, its second moment IS I_w.
internal readonly record struct WallSegment(int Parent, double Y0, double Z0, double Y1, double Z1, double Thickness);

internal readonly record struct OpenWall(
    double Area, double CentroidY, double CentroidZ, double Iy, double Iz,
    double ShearCentreY, double ShearCentreZ, double Warping, double Torsion);

internal static class ThinWalled {
    // An open section BRANCHES (an I leaves its web at both ends), so the sweep is a TREE walk, not a running sum: a
    // segment's sectorial coordinate starts where its PARENT ended, and a root segment starts at zero. A flat
    // accumulation would hand the second flange the first flange's swept area and place the shear centre nowhere.
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
        double sy = coupledY / Math.Max(Math.Abs(iy), double.Epsilon) * Math.Sign(iy is 0.0 ? 1.0 : iy);
        double sz = -coupledZ / Math.Max(Math.Abs(iz), double.Epsilon) * Math.Sign(iz is 0.0 ? 1.0 : iz);
        Seq<(WallSegment Segment, double Start, double End)> polar = Sweep(wall, cy + sy, cz + sz);
        double polarMean = polar.Sum(x => x.Segment.Thickness * Length(x.Segment) * Mid(x.Start, x.End)) / area;
        return new(area, cy, cz, iy, iz, sy, sz,
            polar.Sum(x => Quadratic(x.Segment, x.Start - polarMean, x.End - polarMean)),
            wall.Sum(static s => Length(s) * s.Thickness * s.Thickness * s.Thickness / 3.0));
    }

    static double Length(WallSegment s) => Math.Sqrt((s.Y1 - s.Y0) * (s.Y1 - s.Y0) + (s.Z1 - s.Z0) * (s.Z1 - s.Z0));
    static double Mid(double a, double b) => (a + b) / 2.0;

    // The exact integral of a LINEAR coordinate SQUARED along a segment: L·(a² + a·b + b²)/3.
    static double Quadratic(WallSegment s, double a, double b) => s.Thickness * Length(s) * (a * a + a * b + b * b) / 3.0;

    // The exact integral of the PRODUCT of two linear coordinates: L·(2a₀b₀ + a₀b₁ + a₁b₀ + 2a₁b₁)/6.
    static double Coupled(WallSegment s, double a0, double a1, double b0, double b1) =>
        s.Thickness * Length(s) * (2.0 * a0 * b0 + a0 * b1 + a1 * b0 + 2.0 * a1 * b1) / 6.0;

    // The sweep about a pole: each segment contributes the constant doubled swept area r × ds, and its start value is
    // its parent's end (a root segment starts at zero), so the coordinate stays continuous along every branch.
    static Seq<(WallSegment Segment, double Start, double End)> Sweep(Seq<WallSegment> wall, double poleY, double poleZ) =>
        wall.Fold(Seq<(WallSegment Segment, double Start, double End)>(), (rows, s) => {
            double start = s.Parent >= 0 && s.Parent < rows.Count ? rows[s.Parent].End : 0.0;
            double delta = (s.Y0 - poleY) * (s.Z1 - s.Z0) - (s.Z0 - poleZ) * (s.Y1 - s.Y0);
            return rows.Add((s, start, start + delta));
        });
}

// The EXACT plastic-modulus kernel over a STRIP STACK — the equal-area axis located on the section's own cumulative
// area and the first moment integrated about it. A strip is a band of linearly varying width, so ONE input shape
// covers a rolled flange, a tapering flange root, a trapezoid slice, and a folded-deck chord alike, and strips may
// OVERLAP in level because disjoint material at one height is exactly what an interleaved lip-and-web stack is.
// Every shape-factor multiplier against an elastic modulus is a guess at this integral.
internal readonly record struct Strip(double Lo, double Hi, double WidthLo, double WidthHi);

internal static class Plastic {
    public static double Modulus(Seq<Strip> strips) {
        double axis = EqualAreaLevel(strips, strips.Sum(Area) / 2.0);
        return strips.Sum(s => FirstMoment(s, axis));
    }

    // The COMPOSITE arm: a positioned member set whose parts are already-solved receipts carries only (area, offset,
    // extent, own modulus) per member, so the equal-area axis rides the uniform-band cumulative and a straddling
    // member adds its own modulus plus the uniform-band correction — exact for the plate members that dominate a
    // fabricated section, exact for every member at zero eccentricity, and the STATED approximation between.
    public static double Modulus(Seq<(double Area, double Offset, double Extent, double OwnModulus)> members) {
        double axis = EqualAreaLevel(
            members.Map(static m => new Strip(m.Offset - m.Extent / 2.0, m.Offset + m.Extent / 2.0, m.Area / m.Extent, m.Area / m.Extent)),
            members.Sum(static m => m.Area) / 2.0);
        return members.Sum(m => Math.Abs(m.Offset - axis) >= m.Extent / 2.0
            ? m.Area * Math.Abs(m.Offset - axis)
            : m.OwnModulus + m.Area / m.Extent * Math.Pow(m.Offset - axis, 2.0));
    }

    static double Area(Strip s) => (s.WidthLo + s.WidthHi) / 2.0 * (s.Hi - s.Lo);

    // Cumulative area is piecewise-QUADRATIC in the level because total width is piecewise-linear, so the half-area
    // station solves in CLOSED FORM inside the band that contains it. The scan walks every strip BOUNDARY rather than
    // each strip in turn, which is what makes overlapping strips well-defined.
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

    // Inside one boundary-to-boundary band every active strip has linear width, so the SUMMED width is linear and the
    // accrued area is quadratic: w₀·x + s·x²/2 = wanted. A zero slope degenerates to the linear station exactly
    // rather than through a divide guard.
    static double Station(Seq<Strip> strips, double lo, double hi, double wanted) {
        double w0 = strips.Sum(s => Width(Clip(s, lo, hi), lo));
        double w1 = strips.Sum(s => Width(Clip(s, lo, hi), hi));
        double span = hi - lo, slope = span > 0.0 ? (w1 - w0) / span : 0.0;
        return Math.Abs(slope) <= double.Epsilon
            ? lo + wanted / Math.Max(w0, double.Epsilon)
            : lo + (Math.Sqrt(Math.Max(w0 * w0 + 2.0 * slope * wanted, 0.0)) - w0) / slope;
    }

    // ∫|t − axis|·w(t) dt over one strip, SPLIT at the axis where it falls inside — the exact first moment, never a
    // rectangle standing in for a tapering slice.
    static double FirstMoment(Strip s, double axis) =>
        axis <= s.Lo || axis >= s.Hi
            ? Math.Abs(Signed(s, axis))
            : Math.Abs(Signed(Clip(s, s.Lo, axis), axis)) + Math.Abs(Signed(Clip(s, axis, s.Hi), axis));

    // ∫(t − axis)·w(t) dt with w linear across the strip, in closed form.
    static double Signed(Strip s, double axis) {
        double span = s.Hi - s.Lo;
        if (span <= 0.0) { return 0.0; }
        double slope = (s.WidthHi - s.WidthLo) / span, a = s.Lo - axis, b = s.Hi - axis;
        return s.WidthLo * (b * b - a * a) / 2.0 + slope * ((b * b * b - a * a * a) / 3.0 - a * (b * b - a * a) / 2.0);
    }

    // The portion of a strip inside a level window, its widths re-read at the clipped ends; a strip outside the
    // window clips to zero height and contributes nothing.
    static Strip Clip(Strip s, double lo, double hi) {
        double a = Math.Max(s.Lo, lo), b = Math.Min(s.Hi, hi);
        return b <= a ? new(a, a, 0.0, 0.0) : new(a, b, Width(s, a), Width(s, b));
    }

    static double Width(Strip s, double at) =>
        s.Hi - s.Lo <= 0.0 ? s.WidthLo : s.WidthLo + (s.WidthHi - s.WidthLo) * (at - s.Lo) / (s.Hi - s.Lo);
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

- Owner: `ComponentRow` the campaign row currency (`[ROW_CURRENCIES]`) carrying its one `Provenance` column and its derived section-map membership; `SeedJoin` the railed designation-keyed table join every seed page composes; `TypeCandidate` the seam-declared ingested-type row the reverse fold consumes; `ComponentCatalogue` the ONE fold computing BOTH frozen maps in one pass, and the `AdmitImported` reverse admission beside it.
- Cases: `Sectioned` is DERIVED from the profile's own `ProfileTopology`, so section-map membership is decided by the one arm that knows whether a section integral exists and a seed row cannot assert a membership the geometry contradicts; an unsectioned laminate and a bond line answer `None` at `graph.SectionOf` because they carry no outline, and every solvable arm answers `Some`. `Source` is the ONE origin axis: `Provenance` already states where a row's values came from, so `AdmitImported` stamps `Vendor` and imported rows sit BESIDE seeds in ONE `ComponentId` space — a parallel imported catalogue forking `Lookup`, `ComponentResolution.Build`, and every `graph.SectionOf` read is unrepresentable.
- Boundary: `ComponentCatalogue.Of` is the `Projection/observability#SIGNAL_FACTS` `MaterialsFact.CatalogueAdmit(Row)` VETO subject — a composition-root decorator over `MaterialsHooks.CatalogueAdmit` may transform or refuse each row pre-freeze, so this owner emits nothing and the frozen maps see only admitted rows. `TypeCandidate` is DECLARED ONCE at the seam — `Rasm.Element/Projection/projection#PROJECTION_CONTRACT`, the lowest stratum both non-referencing peers reach — and this fold composes that declaration under the `[PORT]: IIfcTypeReconciler` contract alignment; the producer is `Rasm.Bim` `Exchange/import#REIMPORT` `BimIo.ExportTypeCandidates`, and Materials still references no `Rasm.Bim`. That port's forward leg reconciles ingested types AGAINST Materials ids; `AdmitImported` is its reverse leg, minting rows FROM the types the forward leg left unreconciled, so the two directions close one loop rather than opening a second surface.
- Entry: `ComponentCatalogue.Of(context)` folds `ComponentFamily.Items`; `Traverse` is the rail (a seed row `Component.Of` rejects, a `Sectioned` row `SectionSolver.Solve` cannot solve, or a cross-seed `ComponentId` collision, ABORTS the build TYPED — never a `Choose`/`ToOption` swallow and never the raw frozen-map duplicate-key throw); `Lookup` preserved. Composition-root code binds `Of`'s `Fin` ONCE and passes `catalogue.Rows`/`catalogue.Sections` into `ComponentResolution.Build`. `ComponentCatalogue.AdmitImported(candidates, context, key)` is the ONE reverse fold: it re-reads the seeded rows through the same `ComponentFamily.Items` traverse, elects each candidate's family by the REVERSE read of its `IfcBinding` pair, resolves the section from the seeded row the candidate's `ProfileDesignation` publishes, and lowers the result through the SAME railed `Component.Of` — an unclaimed pair, a pair two families both claim, an unpublished section designation, or a declared profile standard disagreeing with the matched row's authority each SKIP typed, while a `Component.Of` rejection or a within-batch designation collision ABORTS the whole admission under the fail-loud CATALOGUE law.

```csharp signature
// --- [TABLES] ------------------------------------------------------------------------------
// The ONE railed designation-keyed join every <Family>Seed composes for its typed axis table. A `static readonly`
// `FrozenDictionary` built over a THROWING `ComponentId.Create` turns one malformed designation into a
// `TypeInitializationException` at the first touch of the seed class — a fault carrying no `Op`, no row identity, and
// no rail to land on, raised from a static constructor the composition root cannot attribute. Admission runs inside
// the `Lazy` body instead, so the map builds at most once on first `Resolve` and a malformed or duplicated
// designation lands typed on the SAME `ComponentFault` rail its `Component.Of` sibling would have taken.
public static class SeedJoin {
    public static Lazy<Fin<FrozenDictionary<ComponentId, TRow>>> Of<TRow>(Seq<TRow> rows, Func<TRow, string> designation) where TRow : struct =>
        new(() => rows
            .Traverse(row => Admit(designation(row)).Map(id => (Id: id, Row: row))).As()
            .Bind(keyed =>
                from unique in guard(keyed.Map(static k => k.Id).Distinct().Count == keyed.Count,
                    ComponentFault.Designation(Table, "<duplicate-seed-designation>"))
                select keyed.ToFrozenDictionary(static k => k.Id, static k => k.Row)));

    public static Fin<TRow> Resolve<TRow>(Lazy<Fin<FrozenDictionary<ComponentId, TRow>>> table, ComponentId id, Op key) where TRow : struct =>
        table.Value.Bind(map => map.TryGetValue(id, out TRow row)
            ? Fin.Succ(row)
            : Fin.Fail<TRow>(ComponentFault.Family(key, $"<seed-row-unregistered:{id.Value}>")));

    static readonly Op Table = Op.Of(name: nameof(SeedJoin));

    static Fin<ComponentId> Admit(string designation) =>
        ComponentId.Validate(designation, CultureInfo.InvariantCulture, out ComponentId built) is { } error
            ? Fin.Fail<ComponentId>(ComponentFault.Designation(Op.Of(name: designation), $"<malformed-seed-designation:{designation}:{error.Message}>"))
            : Fin.Succ(built);
}

// The Materials catalogue row: the standardized-type item and the ONE provenance column. `Sectioned` is DERIVED from
// the profile's own topology, never asserted per seed row — the arm that decides whether a section integral exists is
// the geometry row, so a seed asserting membership could only ever agree with it or be wrong. `Source` is the single
// origin axis: where a row's VALUES came from already states whether a fold authored it or a vendor library did, so a
// second seed-versus-imported boolean beside it is one fact stored twice.
public readonly record struct ComponentRow(Component Item, Provenance Source) {
    public bool Sectioned => Item.Profile.Topology.Solvable;
}

// The catalogue, de-lockstepped and fail-loud: ONE Items fold computes BOTH frozen maps in one pass. ComponentId's
// generated ordinal value-equality keys the frozen dictionaries — no explicit comparer is threaded. A cross-seed
// designation collision faults TYPED before the frozen build — the raw ToFrozenDictionary duplicate-key throw never
// escapes the rail (two seed pages minting one ComponentId is a seed-data defect, not a composition-root crash).
public sealed record ComponentCatalogue(
    FrozenDictionary<ComponentId, Component> Rows,
    FrozenDictionary<ComponentId, ComputedSection> Sections) {

    public static Fin<ComponentCatalogue> Of(Context context) =>
        toSeq(ComponentFamily.Items)
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

    // --- [IMPORT_ADMISSION]
    // Declared [PORT]: IIfcTypeReconciler runs the FORWARD leg, reconciling ingested IFC types AGAINST Materials-minted
    // ids; this fold runs the REVERSE leg, minting rows FROM whatever that leg left unreconciled, so vendor product
    // content and authored seeds share ONE ComponentId space. Election, section, coring, standard, and both MaterialId
    // slots all read the SEEDED rows — reversing the forward IfcBinding stamp and ProfileRef.Of(ComponentId.Value) — so
    // one correspondence serves both directions and nothing on an imported row is invented; only Ifc, Designation, and
    // Detail come from the candidate itself. Skips stay TYPED and total: a pair no family claims, a pair two families
    // both claim, a section designation no seed published, and a declared profile standard disagreeing with the matched
    // row's authority all yield None rather than a guessed family or a fabricated section. Genuine defects stay
    // fail-loud per the CATALOGUE law — a Component.Of rejection and a within-batch designation collision each
    // abort the whole admission.
    public static Fin<Seq<ComponentRow>> AdmitImported(Seq<TypeCandidate> candidates, Context context, Op key) =>
        from seeded in toSeq(ComponentFamily.Items)
            .Traverse(family => family.Rows(context).Map(rows => (Family: family, Rows: rows))).As()
        let claims = Claims(seeded)
        let catalogued = Catalogued(seeded)
        from elected in candidates.Traverse(candidate => Imported(candidate, claims, catalogued, key)).As()
        let rows = elected.Somes()
        from keyed in guard(rows.Map(static r => r.Item.Designation).Distinct().Count == rows.Count,
            ComponentFault.Designation(key, "<duplicate-imported-designation>"))
        select rows;

    static Fin<Option<ComponentRow>> Imported(
        TypeCandidate candidate,
        HashMap<IfcBinding, Seq<ComponentFamily>> claims,
        HashMap<(string Family, string Section), ComponentRow> catalogued,
        Op key) {
        IfcBinding pair = IfcBinding.Of(candidate.IfcEntity, candidate.PredefinedToken);
        return (from family in Claimant(claims, pair)
                from seed in catalogued.Find((family.Key, SectionKey(candidate.ProfileDesignation.IfNone(""))))
                from standing in Standing(candidate, seed)
                from attributable in Attributable(seed)
                select (Family: family, Seed: seed))
            .Match(
                // Ifc takes the candidate pair VERBATIM, never the matched row's stamp: a vendor type declares the leaf
                // and the predefined token it exports under, so re-stamping it with the seed's pair hands egress a
                // token the source file never carried. Everything dimensional rides the matched row.
                Some: election => Component.Of(
                        election.Family,
                        $"{election.Family.Key}.{Tail(candidate)}",
                        election.Seed.Item.Profile,
                        pair,
                        election.Seed.Item.Coring,
                        election.Seed.Item.Standard,
                        election.Seed.Item.SubstanceId,
                        election.Seed.Item.AppearanceId,
                        Detail(election.Family, candidate),
                        key)
                    .Map(item => Some(new ComponentRow(item, Provenance.Vendor))),
                None: static () => Fin.Succ(Option<ComponentRow>.None));
    }

    // Claims reverses the forward IfcBinding stamp, building from the seeded rows themselves so one pair has exactly
    // ONE owner. Seq is the value because a family MAY override its leaf per seed row: a triple two families both
    // claim elects NOTHING, since taking the last writer IS the guessed family this fold refuses, and an ambiguous
    // claim and an unclaimed triple collapse to one typed skip.
    static HashMap<IfcBinding, Seq<ComponentFamily>> Claims(Seq<(ComponentFamily Family, Seq<ComponentRow> Rows)> seeded) =>
        seeded.Fold(HashMap<IfcBinding, Seq<ComponentFamily>>(), static (index, seed) =>
            seed.Rows.Fold(index, (held, row) =>
                held.AddOrUpdate(row.Item.Ifc, held.Find(row.Item.Ifc).IfNone(Seq<ComponentFamily>()).Add(seed.Family))));

    static Option<ComponentFamily> Claimant(HashMap<IfcBinding, Seq<ComponentFamily>> claims, IfcBinding pair) =>
        claims.Find(pair).Bind(static families => families.Distinct() is { Count: 1 } sole ? sole.Head : None);

    // Catalogued indexes published sections by family and designation TAIL: an imported candidate carries no
    // dimensions, so its section is whichever one this catalogue already publishes under the designation its IFC
    // profile names. Matched rows also supply Sectioned, Coring, Standard, and both MaterialId slots, which is why the
    // index stores whole ROWS — a candidate resolving to a published section inherits that section's receipts rather
    // than fabricating them, and a designation this catalogue never published resolves to nothing at all.
    static HashMap<(string Family, string Section), ComponentRow> Catalogued(Seq<(ComponentFamily Family, Seq<ComponentRow> Rows)> seeded) =>
        seeded.Fold(HashMap<(string, string), ComponentRow>(), static (index, seed) =>
            seed.Rows.Fold(index, (held, row) => held.AddOrUpdate((seed.Family.Key, SectionKey(row.Item.Designation.Value)), row)));

    // ProfileRef.Of(ComponentId.Value) puts the WHOLE `family.designation` string in the Designation slot, so a
    // signature written off a RECONCILED profile arrives dotted while a vendor profile arrives bare — one tail read
    // inverts that mint exactly and covers both shapes with no second normalization.
    static string SectionKey(string designation) =>
        designation.LastIndexOf('.') is var dot && dot >= 0 ? designation[(dot + 1)..] : designation;

    // Standing holds a declared profile standard to AGREE with the matched row's published authority; a signature
    // declaring none passes, because its source stated nothing rather than something else. Adopting EN section
    // geometry under an AISC row is the silent wrong-section this guard forecloses.
    // A vendor type inherits its section from a seeded row, so that row's values become the vendor product's
    // DECLARED geometry. An Authored row is this estate's own policy value with no external producer, and handing one
    // to an imported type restates a judgement as though a standards body had published it — the one place the
    // provenance column changes an outcome rather than describing one. A Published, Defined, or Vendor row traces to
    // a producer outside this estate and admits.
    static Option<Unit> Attributable(ComponentRow seed) => seed.Source.Attributable ? Some(unit) : None;

    static Option<Unit> Standing(TypeCandidate candidate, ComponentRow seed) =>
        candidate.ProfileStandard.Filter(static declared => !string.IsNullOrWhiteSpace(declared)).Match(
            Some: declared => string.Equals(declared, seed.Item.Standard.Authority.Key, StringComparison.Ordinal) ? Some(unit) : None,
            None: static () => Some(unit));

    // Tail reduces the vendor type NAME to the ComponentId grammar — one dot separates family from tail, so every
    // non-alphanumeric character (dot included) folds to '-'. Blank names fall to the GlobalId every ingested IfcRoot
    // carries; a token still empty after that fails ComponentId.Validate inside Component.Of and rails
    // ComponentFault.Designation rather than minting a malformed key.
    static string Tail(TypeCandidate candidate) =>
        Sanitized(candidate.Name) is { Length: > 0 } named ? named : Sanitized(candidate.GlobalId);

    static string Sanitized(string source) =>
        string.Concat(source.Select(static c => char.IsAsciiLetterOrDigit(c) ? c : '-')).Trim('-');

    // DetailLane decides bag PRESENCE and Component.Of enforces it, so an imported bag enters the family's own
    // schema-conforming constructor or drops whole — a None-lane family carrying imported rows is unrepresentable.
    static Option<PropertyBag> Detail(ComponentFamily family, TypeCandidate candidate) => family.Lane switch {
        DetailLane.Realization => Some(ComponentDetail.RealizationRows(DetailRows(candidate))),
        DetailLane.Product     => Some(ComponentDetail.ProductRows(DetailRows(candidate))),
        _                      => Option<PropertyBag>.None,
    };

    // DetailRows carries a candidate's own property rows beside its material name — one signature axis with no
    // Component column, kept as bag evidence rather than dropped. Values already crossed as the seam PropertyValue
    // family, so an imported bag and an authored one content-key identically with no second mint here.
    static (PropertyName Name, PropertyValue Value)[] DetailRows(TypeCandidate candidate) =>
        (candidate.MaterialName
            .Map(static name => (Name: PropertyCategory.Materials.Row(nameof(TypeCandidate.MaterialName)), Value: (PropertyValue)new PropertyValue.Text(name)))
            .ToSeq()
         + toSeq(candidate.Properties.AsIterable().Map(static row => (Name: row.Key, Value: row.Value))))
        .ToArray();
}
```

## [07]-[QUANTITY_ROW]

The ONE bounded typed-mint owner `Projection/component#COMPONENT_PROJECTOR` `SeamSection` and `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Admit` compose — a page-local `(QuantityType, Dimension, unit)` triple at a mint site is the fork this owner closes. Every `QuantityType` spelling and `Dimension` vector below is frozen wire law (`[FROZEN_INVARIANTS]`); each `Scale` states its row's own NATIVE catalog basis, so a millimetre-sourced column and an SI-native one both reach the seam in SI. BOUNDARY: detail-bag rows keep the DIMENSION-ONLY `MeasureValue.OfSi(dim, si)` overload so an authored and an imported bag content-key identically — `QuantityRow` owns TYPED mints only.

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

    // The takeoff re-types: a section's own area and perimeter READ as per-length quantities on the projection
    // seam, and a linear density is that area against the substance density. They are ROWS here rather than
    // page-local QuantityType.Create spellings at the projector, so the typed-mint law holds at one owner.
    // Their SOURCE BASIS is the solver's millimetre receipt exactly like every other section row — a section area is
    // mm² and a heated perimeter mm — so each carries its own mm→SI scale. A unit scale on a millimetre-sourced row
    // publishes an area 10⁶ times its true value and a perimeter 10³ times its own.
    public static readonly QuantityRow VolumePerLength      = new("VolumePerLength",      QuantityType.Create("VolumePerLength"),      Dimension.AreaDim,                      1e-6, "m2");
    public static readonly QuantityRow SurfaceAreaPerLength = new("SurfaceAreaPerLength", QuantityType.Create("SurfaceAreaPerLength"), Dimension.LengthDim,                    1e-3, "m");
    public static readonly QuantityRow LinearDensity        = new("LinearDensity",        QuantityType.Create("LinearDensity"),        Dimension.Create(-1, 1, 0, 0, 0, 0, 0), 1e-6, "kg/m");

    public QuantityType Type { get; }
    public Dimension Dim { get; }
    public double Scale { get; }
    public string Unit { get; }
    // The input contract is the row's NATIVE catalog basis — the Scale column's source unit (mm-power section rows, SI physics rows) —
    // scaled ONCE to SI here; a genuine SI magnitude enters through a Scale-1.0 row unchanged. The prior OfSi name lied
    // about the input basis, double-scaling a true SI caller.
    public Fin<MeasureValue> OfNative(double native) => MeasureValue.OfSi(Type, Dim, native * Scale);
}
```

## [08]-[COMPONENT_DETAIL]

The seed-time bag constructors: each `Realization`/`Product`-lane seed page builds its family's bag AT SEED TIME (`MasonryDetail.Of`, `GlazingDetail.Of`, `FastenerDetail.Of`, `PanelDetail.Of` and siblings compose this owner), so a bag is built where its values are known and the projector reads one already-conforming set. `ProjectType`'s read becomes `c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))`. The `Measured` SI value carries the DIMENSION-only mint (the overload Bim uses) so an authored and an imported row content-key identically; `Joint` routes the `PropertyValue.Enumerated` through the schema's CLOSED allowed set, never a local re-spelling.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The bag-row constructors composing the seam DetailSchema conforming bags (neutral SetName + precedence pinned by the
// schema); Rows folds the tuples into the selected schema's Bag() through ValueBag.With (last-write-wins). Bodies are
// every seed page composes them, and every bag carries its row's own Sourced provenance beside its values.
public static class ComponentDetail {
    // The seam mints a joint token through its schema's CLOSED allowed-set and the admission is RAILED, so this
    // constructor threads the rail rather than dropping a Fin into a PropertyValue slot — a token outside the set is
    // a typed refusal at the seed fold, never a bag row carrying a rail as its value.
    public static Fin<(PropertyName, PropertyValue)> Joint(string kind, Op key) =>
        DetailSchema.Realization.Joint(kind, key).Map(static value => (DetailSchema.JointType, value));

    // The provenance row EVERY Realization/Product bag carries: the seed row's own Provenance crosses the seam as
    // detail evidence beside the values it qualifies, so a downstream reader of a projected property set can tell a
    // transcribed standards value from this estate's own judgement without re-entering the catalogue. The name mints
    // through the owner-blessed producer scope — a call-site PropertyName.Create is the fork between non-referencing
    // peers the seam vocabulary deletes.
    public static (PropertyName, PropertyValue) Sourced(Provenance source) =>
        (PropertyCategory.Materials.Row(nameof(Provenance)), new PropertyValue.Text(source.Key));

    public static (PropertyName, PropertyValue) Token(PropertyName name, string value) => (name, new PropertyValue.Text(value));
    public static Fin<(PropertyName, PropertyValue)> Measured(PropertyName name, Dimension dim, double si) =>
        MeasureValue.OfSi(dim, si).Map(value => (name, (PropertyValue)new PropertyValue.Measure(value)));

    public static PropertyBag RealizationRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        toSeq(rows).Fold(DetailSchema.Realization.Bag(), static (bag, r) => bag.With(r.Name, r.Value));

    public static PropertyBag ProductRows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        toSeq(rows).Fold(DetailSchema.Product.Bag(), static (bag, r) => bag.With(r.Name, r.Value));
}
```

## [09]-[COMPONENT_RESOLUTION]

- Owner: `ResolvedComponent` the one-hop `(Component, Option<ComputedSection>)` receipt; `ComponentResolution` the seam-`ProfileRef` resolver and frozen cache — BYTE-IDENTICAL (`[FROZEN_INVARIANTS]`): only catalogue CONSTRUCTION went `Fin` (`ComponentCatalogue.Of`), bound once at composition; `graph.SectionOf` and every downstream signature are untouched.
- Cases: a `ProfileRef` keys exactly one `ResolvedComponent`; a component present in BOTH maps joins `Some(section)`, one present only in the row map joins `None` — the seam-honest absence (a `PositiveMagnitude` rejects zero, so a forged all-zero `ComputedSection` is unrepresentable), total over every registered ref. The `Option` mirrors the seam `MaterialComposition.ProfileSet` `Option<SectionProperties>` the projector bakes.
- Entry: `Build(rows, sections)` the O(1)-total build-time JOIN (no fault rail, no `Op` — every row resolves; the integral ran once at catalogue build); `Resolve(reference, table, key)` the one-hop dereference the `capacity#SECTION_CAPACITY` and `Rasm.Compute` structural routes call, `Fin<T>` aborting on an unregistered ref (`ComponentFault.Family` — distinct from the `Section` integral failure that already aborted the build).
- Boundary: the resolver owns NO section math and NO seam type — the section is DATA captured at the catalogue-build site that owns the geometry, never a `Func<Component, Op, Fin<ComputedSection>>` re-invoked at resolution. `ProfileRef` stays seam-canonical.

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
            : ComponentFault.Family(key, $"<unresolved-component-ref:{reference.Designation}>");
}
```

## [10]-[RESEARCH]

(none)
